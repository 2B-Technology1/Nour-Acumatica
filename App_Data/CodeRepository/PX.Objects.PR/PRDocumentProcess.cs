/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Payroll.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRDocumentProcess : PXGraph<PRDocumentProcess>
	{
		private const int MaxCalculateBatchSize = 10;

		public PXCancel<PRDocumentProcessFilter> Cancel;

		public PXFilter<PRDocumentProcessFilter> Filter;
		public PXFilteredProcessing<PRPayment, PRDocumentProcessFilter> DocumentList;
		public PXSetup<PRSetup> PRSetup;
		public class SetupValidation : PRSetupValidation<PRDocumentProcess> { }

		public override bool IsDirty => false;

		public PRDocumentProcess()
		{
			DocumentList.ParallelProcessingOptions = settings =>
			{
				settings.IsEnabled = Filter.Current?.Operation == PRDocumentProcessFilter.operation.Calculate
					|| Filter.Current?.Operation == PRDocumentProcessFilter.operation.Recalculate;
				settings.BatchSize = Math.Min(MaxCalculateBatchSize, WebConfig.ParallelProcessingBatchSize);
			};
		}

		protected IEnumerable documentList()
		{
			PRDocumentProcessFilter filter = Filter.Current;
			var paymentQuery = new SelectFrom<PRPayment>
				.InnerJoin<PREmployee>.On<PRPayment.employeeID.IsEqual<PREmployee.bAccountID>>
				.Where<PRPayment.status.IsEqual<P.AsString>>.View(this);
			switch (filter?.Operation)
			{
				case PRDocumentProcessFilter.operation.PutOnHold:
					paymentQuery.WhereOr(typeof(Where<PRPayment.status, Equal<PaymentStatus.pendingPayment>>));
					paymentQuery.WhereOr(typeof(Where<PRPayment.status, Equal<PaymentStatus.open>>));
					return paymentQuery.Select(PaymentStatus.NeedCalculation);
				case PRDocumentProcessFilter.operation.RemoveFromHold:
					return paymentQuery.Select(PaymentStatus.Hold);
				case PRDocumentProcessFilter.operation.Calculate:
				case PRDocumentProcessFilter.operation.Recalculate:
					return SelectFrom<PRPayment>.
						Where<PRPayment.calculated.IsEqual<P.AsBool>.
							And<PRPayment.hold.IsEqual<False>>.
							And<PRPayment.paid.IsEqual<False>>.
							And<PRPayment.released.IsEqual<False>>.
							And<PRPayment.voided.IsEqual<False>>>.View.Select(this, filter.Operation != PRDocumentProcessFilter.operation.Calculate);
				case PRDocumentProcessFilter.operation.Release:
					return SelectFrom<PRPayment>.View.Select(this).FirstTableItems.ToList()
						.Where(x => PRPayChecksAndAdjustments.IsReleaseActionEnabled(x, PRSetup.Current.UpdateGL == true));
				case PRDocumentProcessFilter.operation.Void:
					int startRow = PXView.StartRow;
					int totalRows = 0;

					string[] nonReleasedVoidPaychecks = SelectFrom<PRPayment>
						.Where<PRPayment.docType.IsEqual<PayrollType.voidCheck>
							.And<PRPayment.released.IsNotEqual<True>>>
						.View.Select(this).FirstTableItems
						.Select(item => item.RefNbr).ToArray();

					PXView query = new SelectFrom<PRPayment>
						.Where<Brackets<PRPayment.status.IsEqual<PaymentStatus.released>
								.Or<PRPayment.released.IsEqual<True>
									.And<PRPayment.voided.IsEqual<False>>
									.And<PRPayment.hasUpdatedGL.IsEqual<False>>>>
							.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>>.View(this).View;

					List<object> queryParameters = new List<object>(PXView.Parameters);
					if (nonReleasedVoidPaychecks.Length > 0)
					{
						query.WhereAnd<Where<PRPayment.refNbr.IsNotIn<P.AsString>>>();
						queryParameters.Add(nonReleasedVoidPaychecks);
					}

					List<object> result = query.Select(PXView.Currents, queryParameters.ToArray(), PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
							ref startRow, PXView.MaximumRows, ref totalRows);
					PXView.StartRow = 0;
					return result;
			}

			return new List<PRPayment>();
		}

		protected static void ProcessPayments(List<PRPayment> list, string operation)
		{
			PRPayChecksAndAdjustments payCheckGraph = PXGraph.CreateInstance<PRPayChecksAndAdjustments>();
			if (operation == PRDocumentProcessFilter.operation.Release)
			{
				payCheckGraph.ReleasePaymentList(list, true);
			}
			else if (operation == PRDocumentProcessFilter.operation.Calculate || operation == PRDocumentProcessFilter.operation.Recalculate)
			{
				BatchCalculate(payCheckGraph, list);
			}
			else
			{
				foreach (PRPayment payment in list)
				{
					try
					{
						switch (operation)
						{
							case PRDocumentProcessFilter.operation.PutOnHold:
								payment.Hold = true;
								payCheckGraph.Document.Update(payment);
								payCheckGraph.Persist();
								break;
							case PRDocumentProcessFilter.operation.RemoveFromHold:
								payment.Hold = false;
								payCheckGraph.Document.Update(payment);
								payCheckGraph.Persist();
								break;
							case PRDocumentProcessFilter.operation.Void:
								payCheckGraph.CurrentDocument.Current = payment;
								payCheckGraph.VoidPayment.Press();
								payCheckGraph.Save.Press();
								break;
						}
					}
					catch (Exception ex)
					{
						PXProcessing<PRPayment>.SetError(list.IndexOf(payment), ex);
					}
				}
			}
		}

		protected static void BatchCalculate(PRPayChecksAndAdjustments payCheckGraph, List<PRPayment> list)
		{
			foreach (List<PRPayment> listPartition in Partition(list, MaxCalculateBatchSize))
			{
				try
				{
					payCheckGraph.CalculatePaymentList(listPartition, true);
					foreach (PRPayment payment in listPartition)
					{
						PXProcessing.SetCurrentItem(payment);
						PXProcessing.SetProcessed();
					}
				}
				catch (CalculationEngineException ex)
				{
					bool errorMatched = false;
					for (int i = 0; i < listPartition.Count; i++)
					{
						PRPayment paymentInPartionedList = listPartition.ElementAt(i);
						if (payCheckGraph.Document.Cache.ObjectsEqual(paymentInPartionedList, ex.Payment))
						{
							PXProcessing.SetError<PRPayment>(list.IndexOf(paymentInPartionedList), ex.InnerException);
							errorMatched = true;
						}
						else
						{
							PXProcessing.SetError<PRPayment>(list.IndexOf(paymentInPartionedList), PXMessages.LocalizeFormat(Messages.ErrorOnOtherPayment, ex.Payment?.PaymentDocAndRef));
						}
					}

					if (!errorMatched)
					{
						throw ex.InnerException;
					}
				}
			}
		}

		// https://stackoverflow.com/questions/1396048/c-sharp-elegant-way-of-partitioning-a-list
		private static IEnumerable<List<T>> Partition<T>(IList<T> source, int size)
		{
			for (int i = 0; i < Math.Ceiling(source.Count / (double)size); i++)
			{
				yield return new List<T>(source.Skip(size * i).Take(size));
			}
		}

		#region Events
		protected virtual void _(Events.RowSelected<PRDocumentProcessFilter> e)
		{
			PRDocumentProcessFilter row = e.Row as PRDocumentProcessFilter;
			if (row == null)
			{
				return;
			}

			#pragma warning disable CS0618 // Type or member is obsolete
			DocumentList.SetProcessDelegate(list => ProcessPayments(list, row.Operation));
			#pragma warning restore CS0618 // Type or member is obsolete
		}

		protected virtual void _(Events.FieldVerifying<PRDocumentProcessFilter.operation> e)
		{
			if (Equals(e.NewValue, PRDocumentProcessFilter.operation.Release))
			{
				foreach (PRPayment payment in SelectFrom<PRPayment>.View.Select(this).FirstTableItems.ToList().Where(x => PRPayChecksAndAdjustments.IsReleaseActionEnabled(x, PRSetup.Current.UpdateGL == true)))
				{
					// Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers [needed for the PREmployeeTaxForm query]
					if (PRPayChecksAndAdjustments.HasAssociatedTaxFormBatch(payment))
					{
						if (DocumentList.View.Ask(null, Messages.ConfirmationHeader, Messages.TaxFormToBeGenerated, MessageButtons.YesNo, PRPayChecksAndAdjustments.ConfirmCancelButtons, MessageIcon.Question) == WebDialogResult.No)
						{
							e.NewValue = e.OldValue;
							return;
						}
					}

					// Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers [needed to be able to fill the fields of the PRPrepareTaxFormsMaint Filter]
					PRPrepareTaxFormsMaint.CheckCancellationTaxForm(DocumentList.View, payment);
				}
			}
		}
		#endregion

		#region Static release methods
		public static DocumentList<Batch> ReleaseDoc(List<PRPayment> list, bool isMassProcess)
		{
			if (isMassProcess)
			{
				return ReleaseDoc(list, isMassProcess, true);
			}
			else
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					try
					{
						DocumentList<Batch> batchList = ReleaseDoc(list, isMassProcess, true);
						ts.Complete();
						return batchList;
					}
					catch (Common.PXMassProcessException ex)
					{
						if (list.Count > 1 && ex.ListIndex >= 0 && ex.ListIndex < list.Count)
						{
							throw new PXException(Messages.BulkProcessErrorFormat, list[ex.ListIndex]?.PaymentDocAndRef, ex.Message);
						}
						else
						{
							throw;
						}
					}
				}
			}
		}

		public static DocumentList<Batch> ReleaseDoc(List<PRPayment> list, bool isMassProcess, bool autoPost)
		{
			bool failed = false;

			PRReleaseProcess rg = PXGraph.CreateInstance<PRReleaseProcess>();
			JournalEntry je = rg.UpdateGL ? CreateJournalEntryGraph() : null;
			PostGraph pg = rg.UpdateGL ? PXGraph.CreateInstance<PostGraph>() : null;

			List<int> batchbind = new List<int>();

			DocumentList<Batch> batchlist = new DocumentList<Batch>(rg);

			for (int i = 0; i < list.Count; i++)
			{
				PRPayment doc = list[i];
				try
				{
					rg.Clear();
					rg.VerifyPayment(doc);
					doc = rg.OnBeforeRelease(doc);
					rg.ReleaseDocProc(je, doc);

					if (rg.UpdateGL)
					{
						if (je.BatchModule.Current != null && batchlist.Find(je.BatchModule.Current) == null)
						{
							batchlist.Add(je.BatchModule.Current);
							batchbind.Add(i);
						}
					}

					if (isMassProcess)
					{
						PXProcessing<PRPayment>.SetInfo(i, ActionsMessages.RecordProcessed);
					}
				}
				catch (Exception e)
				{
					if (rg.UpdateGL)
					{
						batchlist.Remove(je.BatchModule.Current);
						je.Clear();
					}

					Exception exception = e;
					if (e is PXFieldValueProcessingException)
					{
						if (e.InnerException is PXTaskIsCompletedException taskIsCompletedException)
						{
							PXResult<PMTask, PMProject> query =
								(PXResult<PMTask, PMProject>)SelectFrom<PMTask>
									.InnerJoin<PMProject>.On<PMTask.projectID.IsEqual<PMProject.contractID>>
									.Where<PMTask.projectID.IsEqual<P.AsInt>.And<PMTask.taskID.IsEqual<P.AsInt>>>
									.View.SelectSingleBound(rg, null, taskIsCompletedException.ProjectID, taskIsCompletedException.TaskID);

							PMTask task = query;
							PMProject project = query;

							if (task != null && project != null)
								exception = new PXException(e, Messages.ProjectTaskIsNotActive, task.TaskCD, project.ContractCD);
						}
						else if (e.InnerException is PXSetPropertyException)
						{
							PMProject project = PMProject.PK.Find(rg, rg.Earnings.Current?.ProjectID);

							if (project != null && project.NonProject != true && project.BaseType == CT.CTPRType.Project)
							{
								Account account = Account.PK.Find(rg, rg.Earnings.Current?.AccountID);
								if (account != null && account.AccountGroupID == null)
								{
									exception = new PXException(e, Messages.NoAccountGroup, project.ContractCD, account.AccountCD);
								}
							}
						}
					}

					if (isMassProcess)
					{
						PXProcessing<PRPayment>.SetError(i, exception);
						PXTrace.WriteError($"{doc.PaymentDocAndRef} : {exception}");
						failed = true;
					}
					else
					{
						throw new Common.PXMassProcessException(i, exception);
					}
				}
			}

			if (rg.UpdateGL)
			{
				for (int i = 0; i < je.created.Count; i++)
				{
					Batch batch = je.created[i];
					try
					{
						if (rg.AutoPost && autoPost)
						{
							pg.Clear();
							pg.PostBatchProc(batch);
						}
					}
					catch (Exception e)
					{
						if (isMassProcess)
						{
							failed = true;
							PXProcessing<PRPayment>.SetError(batchbind[i], e);
						}
						else
						{
							throw new Common.PXMassProcessException(batchbind[i], e);
						}
					}
				}
			}

			if (failed)
			{
				throw new PXException(GL.Messages.DocumentsNotReleasedSeeTrace);
			}
			return rg.AutoPost && rg.UpdateGL ? batchlist : new DocumentList<Batch>(rg);
		}
		#endregion Static release methods

		/// <summary>
		/// Creates a JournalEntry graph instance without some restrictions (like PXRestrictorAttribute), which prevents releasing documents.
		/// </summary>
		private static JournalEntry CreateJournalEntryGraph()
		{
			var graph = PXGraph.CreateInstance<JournalEntry>();
			foreach (PXRestrictorAttribute restrictorAttribute in graph.Caches[typeof(GLTran)].GetAttributesOfType<PXRestrictorAttribute>(null, nameof(GLTran.projectID)))
			{
				var bql = BqlCommand.Decompose(restrictorAttribute.RestrictingCondition);
				if (bql.Contains(typeof(PMProject.isCompleted)) || bql.Contains(typeof(PMProject.isActive)) || bql.Contains(typeof(PMProject.isCancelled)))
				{
					restrictorAttribute.SuppressVerify = true;
				}
			}

			BaseProjectTaskAttribute taskAttribute = graph.Caches[typeof(GLTran)].GetAttributesOfType<BaseProjectTaskAttribute>(null, nameof(GLTran.taskID)).Single();
			taskAttribute.SuppressVerify = true;

			return graph;
		}
	}

	[PXHidden]
	public class PRReleaseProcess : PXGraph<PRReleaseProcess>
	{
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		#region Views
		public PXSelect<PRPayment> PRDocument;
		public SelectFrom<PRPayment>
			.Where<PRPayment.docType.IsEqual<P.AsString>
			.And<PRPayment.refNbr.IsEqual<P.AsString>>>.View SpecificPayment;
		public PXSelect<Batch> Batch;
		public PXSetup<GLSetup> GLSetup;
		public PXSetup<PRSetup> PRSetup;
		public class SetupValidation : PRSetupValidation<PRReleaseProcess> { }

		public PXSelect<CATran> CashTran;
		public PXSelect<PRYtdEarnings> YtdEarnings;
		public PXSelect<PRYtdDeductions> YtdDeductions;

		public SelectFrom<PRYtdTaxes>
			.Where<PRYtdTaxes.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
			.And<PRYtdTaxes.taxID.IsEqual<P.AsInt>.And<PRYtdTaxes.year.IsEqual<P.AsString>>>>.View YtdTaxes;

		public PXSelect<PRPeriodTaxes> PeriodTaxes;

		public PXSelect<PRPeriodTaxApplicableAmounts> PeriodTaxApplicableAmounts;

		public SelectFrom<PREarningDetail>.
					Where<PREarningDetail.employeeID.IsEqual<P.AsInt>.
					And<PREarningDetail.paymentDocType.IsEqual<P.AsString>>.
					And<PREarningDetail.paymentRefNbr.IsEqual<P.AsString>>>.
					OrderBy<Asc<PREarningDetail.date>>.View Earnings;

		public SelectFrom<GLTran>
			.InnerJoin<PMTran>
				.On<GLTran.pMTranID.IsEqual<PMTran.tranID>
					.And<GLTran.batchNbr.IsEqual<PMTran.batchNbr>>
					.And<GLTran.tranType.IsEqual<PMTran.tranType>>>
			.Where<PMTran.origRefID.IsIn<P.AsGuid>>
			.OrderBy<GLTran.batchNbr.Asc, GLTran.lineNbr.Asc>
			.View TimeModuleGLTransactionsToReverse;

		public SelectFrom<PRDeductionDetail>.
			InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRDeductionDetail.codeID>>.
			Where<PRDeductionDetail.employeeID.IsEqual<P.AsInt>.
				And<PRDeductionDetail.paymentDocType.IsEqual<P.AsString>.
				And<PRDeductionDetail.paymentRefNbr.IsEqual<P.AsString>>>>.View DeductionDetails;

		public SelectFrom<PRBenefitDetail>.
			InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRBenefitDetail.codeID>>.
			Where<PRBenefitDetail.employeeID.IsEqual<P.AsInt>.
				And<PRBenefitDetail.paymentDocType.IsEqual<P.AsString>.
				And<PRBenefitDetail.paymentRefNbr.IsEqual<P.AsString>>>>.View BenefitDetails;

		public SelectFrom<PRTaxDetail>.
			InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRTaxDetail.taxID>>.
			Where<PRTaxDetail.employeeID.IsEqual<P.AsInt>.
				And<PRTaxDetail.paymentDocType.IsEqual<P.AsString>.
				And<PRTaxDetail.paymentRefNbr.IsEqual<P.AsString>>>>.View TaxDetails;

		public SelectFrom<PRPaymentEarning>.
				Where<PRPaymentEarning.docType.IsEqual<P.AsString>.
				And<PRPaymentEarning.refNbr.IsEqual<P.AsString>>>.View SummaryEarnings;
		public SelectFrom<PRPaymentDeduct>.
				Where<PRPaymentDeduct.docType.IsEqual<P.AsString>.
				And<PRPaymentDeduct.refNbr.IsEqual<P.AsString>.
				And<PRPaymentDeduct.isActive.IsEqual<True>>>>.View SummaryDeductions;


		public SelectFrom<PRPaymentTax>.
				Where<PRPaymentTax.docType.IsEqual<P.AsString>.
				And<PRPaymentTax.refNbr.IsEqual<P.AsString>>>.View SummaryTaxes;

		public SelectFrom<PRPaymentTaxApplicableAmounts>.
				Where<PRPaymentTaxApplicableAmounts.docType.IsEqual<P.AsString>.
				And<PRPaymentTaxApplicableAmounts.refNbr.IsEqual<P.AsString>>>.View PaymentTaxApplicableAmounts;

		public SelectFrom<PREmployeeDeduct>.
			InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PREmployeeDeduct.codeID>>.
			Where<PREmployeeDeduct.bAccountID.IsEqual<P.AsInt>.
			And<PREmployeeDeduct.codeID.IsEqual<P.AsInt>.
			And<PRDeductCode.isGarnishment.IsEqual<True>>>>.View Garnishment;

		public SelectFrom<PRPaymentPTOBank>.
				Where<PRPaymentPTOBank.docType.IsEqual<P.AsString>.
				And<PRPaymentPTOBank.refNbr.IsEqual<P.AsString>>>.View PTOBanks;
		public SelectFrom<PRPayGroupYear>
			.Where<PRPayGroupYear.payGroupID.IsEqual<P.AsString>
			.And<PRPayGroupYear.year.IsEqual<P.AsString>>>.View PayrollYear;

		public SelectFrom<PRPayGroupPeriod>
			.Where<PRPayGroupPeriod.payGroupID.IsEqual<P.AsString>
			.And<PRPayGroupPeriod.finPeriodID.IsEqual<P.AsString>>>.View PayPeriod;

		public SelectFrom<PRBatch>.Where<PRBatch.batchNbr.IsEqual<P.AsString>>.View PayBatches;

		public SelectFrom<PRPayment>.Where<PRPayment.released.IsEqual<False>.
			And<PRPayment.payBatchNbr.IsEqual<P.AsString>>>.View NonReleasedPayBatchPayments;

		public SelectFrom<PRDirectDepositSplit>
			.Where<PRDirectDepositSplit.docType.IsEqual<P.AsString>
				.And<PRDirectDepositSplit.refNbr.IsEqual<P.AsString>>>.View DirectDepositSplits;

		public PRPayChecksAndAdjustments.DirectDepositBatchAndDetailsSelect.View DirectDepositBatchAndDetails;

		public SelectFrom<PREmployee>.View EmployeePayrollSettings;

		public SelectFrom<EPEmployeePosition>
			.Where<EPEmployeePosition.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>>.View EmployeePositions;

		public SelectFrom<EPEmployeePosition>
			.Where<PRxEPEmployeePosition.settlementPaycheckRefNoteID.IsEqual<P.AsGuid>>.View TerminatedEmployeePosition;

		public SelectFrom<PRPayment>
				.Where<PRPayment.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
					.And<PRPayment.payPeriodID.IsGreater<PRPayment.payPeriodID.FromCurrent>>
					.And<PRPayment.released.IsEqual<True>>>.View PaymentsReleasedAfter;
		#endregion Views

		public bool AutoPost
		{
			get
			{
				return (bool)PRSetup.Cache.GetValue<PRSetup.autoPost>(PRSetup.Current);
			}
		}

		public bool UpdateGL
		{
			get
			{
				return (bool)PRSetup.Cache.GetValue<PRSetup.updateGL>(PRSetup.Current);
			}
		}

		public virtual bool PaymentUpdatesGL(PRPayment payment)
		{
			return PaymentUpdatesGL(this, payment, PRSetup.Current);
		}

		public static bool PaymentUpdatesGL(PXGraph graph, PRPayment payment, PRSetup prSetup)
		{
			if (payment.DocType == PayrollType.VoidCheck && new SelectFrom<PRPayment>
				.Where<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>
					.And<PRPayment.refNbr.IsEqual<P.AsString>>>.View(graph).SelectSingle(payment.RefNbr)?.HasUpdatedGL == false)
			{
				return false;
			}

			if (payment.TotalEarnings == 0
				&& payment.GrossAmount == 0
				&& payment.TaxAmount == 0
				&& payment.NetAmount == 0
				&& payment.EmployerTaxAmount == 0)
			{
				bool noEarningDetailUpdatingGL = new SelectFrom<PREarningDetail>
					.Where<PREarningDetail.paymentDocType.IsEqual<P.AsString>
						.And<PREarningDetail.paymentRefNbr.IsEqual<P.AsString>>
						.And<PREarningDetail.amount.IsNotEqual<decimal0>>>.View(graph).SelectSingle(payment.DocType, payment.RefNbr) == null;

				bool noTaxDetailUpdatingGL = new SelectFrom<PRTaxDetail>
					.Where<PRTaxDetail.paymentDocType.IsEqual<P.AsString>
						.And<PRTaxDetail.paymentRefNbr.IsEqual<P.AsString>>
						.And<PRTaxDetail.amount.IsNotEqual<decimal0>>>.View(graph).SelectSingle(payment.DocType, payment.RefNbr) == null;

				bool noDedBenUpdatingGL = new SelectFrom<PRPaymentDeduct>
					.InnerJoin<PRDeductCode>.On<PRPaymentDeduct.FK.DeductionCode>
					.Where<PRPaymentDeduct.docType.IsEqual<P.AsString>
						.And<PRPaymentDeduct.refNbr.IsEqual<P.AsString>>
						.And<PRDeductCode.noFinancialTransaction.IsEqual<False>
						.And<PRPaymentDeduct.dedAmount.IsNotEqual<decimal0>.Or<PRPaymentDeduct.cntAmount.IsNotEqual<decimal0>>>>>.View(graph).SelectSingle(payment.DocType, payment.RefNbr) == null;

				if (noEarningDetailUpdatingGL && noTaxDetailUpdatingGL && noDedBenUpdatingGL)
			{
				return false;
			}
			}

			return prSetup.UpdateGL == true;
		}

		public virtual void ReleaseDocProc(JournalEntry je, PRPayment doc)
		{
			if (doc.Released != true)
			{
				CurrencyInfo info = GetCurrencyInfo(doc);
				if (PaymentUpdatesGL(doc))
				{
					SegregateBatch(je, doc.BranchID, info.CuryID, doc.TransactionDate, doc.FinPeriodID, doc.DocDesc, info);
				}
				var isDebit = doc.DrCr == GL.DrCr.Debit;

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					if (PaymentUpdatesGL(doc))
					{
						//Credit cash account for the amount equal to net pay total
						IEnumerable<GLTran> transactions = WriteNetPayTransactions(doc, info);
						foreach (var tran in transactions)
						{
							je.GLTranModuleBatNbr.Insert(tran);
						}
					}

					//Loop over earnings, debit line account for line amount
					foreach (IGrouping<Tuple<string, int?>, PXResult<PREarningDetail>> groupedEarningDetails in Earnings.Select(doc.EmployeeID, doc.DocType, doc.RefNbr)
						.GroupBy(x => new Tuple<string, int?>(((PREarningDetail)x).TypeCD, ((PREarningDetail)x).LocationID)))
					{
						foreach (PREarningDetail earningDetail in groupedEarningDetails)
						{
							Earnings.Current = earningDetail;

							if (PaymentUpdatesGL(doc))
							{
								//Insert GLTran
								GLTran earningTran = WriteEarning(doc, earningDetail, info, je.BatchModule.Current);
								earningTran = je.GLTranModuleBatNbr.Insert(earningTran);
							}

							earningDetail.Released = true;
							Earnings.Update(earningDetail);
						}

						//Insert YTDEarnings
						var ytdEarning = new PRYtdEarnings();
						ytdEarning.Year = doc.TransactionDate.Value.Year.ToString();
						ytdEarning.Month = doc.TransactionDate.Value.Month;
						ytdEarning.EmployeeID = doc.EmployeeID;
						ytdEarning.TypeCD = groupedEarningDetails.Key.Item1;
						ytdEarning.LocationID = groupedEarningDetails.Key.Item2;
						ytdEarning.Amount = (isDebit ? 1 : -1) * groupedEarningDetails.Sum(x => ((PREarningDetail)x).Amount ?? 0m);
						ytdEarning = YtdEarnings.Insert(ytdEarning);
					}

					// Revert Time GL transactions if they were created when 'Post' or 'OverridePMAndGLInPayroll' option was used.
					// It is also applicable for the case when the current posting option is different, for example 'OverridePMInPayroll' or 'PostPMAndGLFromPayroll'. 
					// This can happen if the time posting option was changed after the Time Card release, but before the Paycheck release.
					// In that case existing time GL transacton should still be reversed.
					if (PaymentUpdatesGL(doc))
					{
						Dictionary<Guid?, PMTimeActivity> timeActivitiesToReverse = ImportTimeActivitiesHelper.GetTimeActivitiesToReverse(this, doc.RefNbr);
	
						foreach (PXResult<GLTran, PMTran> record in TimeModuleGLTransactionsToReverse.Select(timeActivitiesToReverse.Keys.ToArray()))
						{
							GLTran glTran = record;
							PMTran pmTran = record;
							PMTimeActivity timeActivity = timeActivitiesToReverse[pmTran.OrigRefID];

							GLTran reverseTransaction = WriteReverseGLTran(doc, glTran, timeActivity.EarningTypeID);
							je.GLTranModuleBatNbr.Insert(reverseTransaction);
						}
					}

					foreach (PXResult<PRDeductionDetail, PRDeductCode> deductionDetail in DeductionDetails.Select(doc.EmployeeID, doc.DocType, doc.RefNbr))
					{
						if (PaymentUpdatesGL(doc))
						{
							//Insert GLTran
							GLTran deductionTran = WriteDeduction(doc, deductionDetail, deductionDetail, info, je.BatchModule.Current);
							je.GLTranModuleBatNbr.Insert(deductionTran);
						}

						((PRDeductionDetail)deductionDetail).Released = true;
						DeductionDetails.Update(deductionDetail);
					}

					foreach (PXResult<PRBenefitDetail, PRDeductCode> benefitDetail in BenefitDetails.Select(doc.EmployeeID, doc.DocType, doc.RefNbr))
					{
						if (PaymentUpdatesGL(doc))
						{
							//Insert GLTran
							GLTran expenseTran = WriteBenefitExpense(doc, benefitDetail, benefitDetail, info, je.BatchModule.Current);
							je.GLTranModuleBatNbr.Insert(expenseTran);
							if (((PRDeductCode)benefitDetail).IsPayableBenefit != true)
							{
								GLTran liabilityTran = WriteBenefitLiability(doc, benefitDetail, benefitDetail, info, je.BatchModule.Current);
								je.GLTranModuleBatNbr.Insert(liabilityTran);
							}
						}

						((PRBenefitDetail)benefitDetail).Released = true;
						BenefitDetails.Update(benefitDetail);
					}

					foreach (PXResult<PRTaxDetail, PRTaxCode> taxDetail in TaxDetails.Select(doc.EmployeeID, doc.DocType, doc.RefNbr))
					{
						if (PaymentUpdatesGL(doc))
						{
							//Insert GLTran
							GLTran liabilityTran = WriteTaxLiability(doc, taxDetail, taxDetail, info, je.BatchModule.Current);
							je.GLTranModuleBatNbr.Insert(liabilityTran);
							if (((PRTaxDetail)taxDetail).TaxCategory == TaxCategory.EmployerTax)
							{
								GLTran expenseTran = WriteTaxExpense(doc, taxDetail, taxDetail, info, je.BatchModule.Current);
								je.GLTranModuleBatNbr.Insert(expenseTran);
							}
						}

						((PRTaxDetail)taxDetail).Released = true;
						TaxDetails.Update(taxDetail);
					}

					//Insert YTDDeductions and update garnishment info
					foreach (IGrouping<int?, PXResult<PRPaymentDeduct>> groupedDeductions in SummaryDeductions.Select(doc.DocType, doc.RefNbr).GroupBy(x => ((PRPaymentDeduct)x).CodeID))
					{
						var ytdDeduction = new PRYtdDeductions();
						ytdDeduction.Year = doc.TransactionDate.Value.Year.ToString();
						ytdDeduction.EmployeeID = doc.EmployeeID;
						ytdDeduction.CodeID = groupedDeductions.Key;
						ytdDeduction.Amount = (isDebit ? 1 : -1) * groupedDeductions.Sum(x => ((PRPaymentDeduct)x).DedAmount ?? 0m);
						ytdDeduction.EmployerAmount = (isDebit ? 1 : -1) * groupedDeductions.Sum(x => ((PRPaymentDeduct)x).CntAmount ?? 0m);
						YtdDeductions.Insert(ytdDeduction);

						var employeeDeduct = (PREmployeeDeduct)Garnishment.SelectSingle(doc.EmployeeID, groupedDeductions.Key);
						if (employeeDeduct != null)
						{
							decimal delta = groupedDeductions.Sum(x => ((PRPaymentDeduct)x).DedAmount ?? 0m);
							employeeDeduct.GarnPaidAmount = (employeeDeduct.GarnPaidAmount ?? 0m) + (isDebit ? 1 : -1) * delta;
							Garnishment.Update(employeeDeduct);
						}
					}

					PRPayGroupYear payYear = PayrollYear.SelectSingle(doc.PayGroupID, doc.TransactionDate.Value.Year.ToString());
					DayOfWeek payWeekStart = payYear.StartDate.Value.DayOfWeek == DayOfWeek.Saturday ? DayOfWeek.Sunday : payYear.StartDate.Value.DayOfWeek + 1;
					PRPayGroupPeriod payPeriod = PayPeriod.SelectSingle(doc.PayGroupID, doc.PayPeriodID);

					// Insert Period and YTD Taxes
					foreach (PRPaymentTax taxLine in SummaryTaxes.Select(doc.DocType, doc.RefNbr))
					{
						var ytdTax = new PRYtdTaxes();
						ytdTax.Year = doc.TransactionDate.Value.Year.ToString();
						ytdTax.EmployeeID = doc.EmployeeID;
						ytdTax.TaxID = taxLine.TaxID;
						ytdTax.TaxableWages = (isDebit ? 1 : -1) * taxLine.WageBaseAmount;
						if (isDebit)
						{
							ytdTax.MostRecentWH = taxLine.TaxAmount;
						}
						YtdTaxes.Insert(ytdTax);

						var periodTax = new PRPeriodTaxes();
						periodTax.Year = doc.TransactionDate.Value.Year.ToString();
						periodTax.EmployeeID = doc.EmployeeID;
						periodTax.TaxID = taxLine.TaxID;
						periodTax.PeriodNbr = payPeriod.PeriodNbrAsInt;
						periodTax.Amount = (isDebit ? 1 : -1) * taxLine.TaxAmount;
						periodTax.AdjustedGrossAmount = (isDebit ? 1 : -1) * taxLine.AdjustedGrossAmount;
						periodTax.ExemptionAmount = (isDebit ? 1 : -1) * taxLine.ExemptionAmount;
						periodTax.Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(doc.TransactionDate.Value, CalendarWeekRule.FirstDay, payWeekStart);
						periodTax.Month = doc.TransactionDate.Value.Month;
						PeriodTaxes.Insert(periodTax);
					}

					// Insert Period Tax Applicable Amount records
					foreach (PRPaymentTaxApplicableAmounts line in PaymentTaxApplicableAmounts.Select(doc.DocType, doc.RefNbr))
					{
						PRPeriodTaxApplicableAmounts periodRecord = new PRPeriodTaxApplicableAmounts();
						periodRecord.Year = doc.TransactionDate.Value.Year.ToString();
						periodRecord.EmployeeID = doc.EmployeeID;
						periodRecord.TaxID = line.TaxID;
						periodRecord.WageTypeID = line.WageTypeID;
						periodRecord.IsSupplemental = line.IsSupplemental;
						periodRecord.PeriodNbr = payPeriod.PeriodNbrAsInt;
						periodRecord.AmountAllowed = (isDebit ? 1 : -1) * line.AmountAllowed;
						periodRecord.Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(doc.TransactionDate.Value, CalendarWeekRule.FirstDay, payWeekStart);
						periodRecord.Month = doc.TransactionDate.Value.Month;
						PeriodTaxApplicableAmounts.Insert(periodRecord);
					}

					ProcessPTO(doc);					
					foreach (PRDirectDepositSplit directDepositSplit in DirectDepositSplits.Select(doc.DocType, doc.RefNbr).FirstTableItems)
					{
						// This will trigger "CashTranIDAttribute.RowPersisting" handler that will use
						// the validation from the "PRDirectDepositCashTranIDAttribute.PreventCATranRecordCreation" method.
						// After that the "PRDirectDepositSplit.CATranID" field will be either assigned or set to null, or not changed.
						// The corresponding CATran record will be either created, deleted, or not changed.
						directDepositSplit.Released = true;
						DirectDepositSplits.Update(directDepositSplit);
					}
					doc.Released = true;
					doc.ReleasedToVerify = false;
					doc.HasUpdatedGL = PaymentUpdatesGL(doc);
					if (PaymentUpdatesGL(doc))
					{
						if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
						{
							je.Save.Press();
						}
						else if (je.BatchModule.Current != null)
						{
							je.BatchModule.Delete(je.BatchModule.Current);
							je.BatchModule.Current = null;
						}

						if (!je.BatchModule.Cache.IsDirty && string.IsNullOrEmpty(doc.BatchNbr) && je.BatchModule.Current != null)
						{
							doc.BatchNbr = ((Batch)je.BatchModule.Current).BatchNbr;
						}
					}

					doc = (PRPayment)PRDocument.Cache.Update(doc);

					ClosePayBatchIfAllPaymentsAreReleased(doc.PayBatchNbr);
					if (doc.DocType == PayrollType.VoidCheck)
					{
						UpdateVoidedCheck(doc);
						RemoveFromPaymentBatch(doc);
					}

					PostReleaseForSettlementPayments(doc);

					//After release, update MTD, QTD and YTD Amounts.
					foreach (PRPaymentEarning summary in SummaryEarnings.Select(doc.DocType, doc.RefNbr))
					{
						PRPayChecksAndAdjustments.UpdateSummaryEarning(this, doc, summary);
						summary.MTDAmount += summary.Amount;
						summary.QTDAmount += summary.Amount;
						summary.YTDAmount += summary.Amount;
						SummaryEarnings.Update(summary);
					}
					foreach (PRPaymentDeduct summary in SummaryDeductions.Select(doc.DocType, doc.RefNbr))
					{
						PRPayChecksAndAdjustments.UpdateSummaryDeductions(this, doc, summary);
						summary.YtdAmount += summary.DedAmount ?? 0m;
						summary.EmployerYtdAmount += summary.CntAmount ?? 0m;
						SummaryDeductions.Update(summary);
					}
					foreach (PRPaymentTax summary in SummaryTaxes.Select(doc.DocType, doc.RefNbr))
					{
						decimal amount = YtdTaxes.Select(summary.TaxID, doc.TransactionDate.Value.Year).TopFirst?.Amount ?? 0;
						summary.YtdAmount = amount + (summary.TaxAmount ?? 0m);
						SummaryTaxes.Update(summary);
					}

					this.Actions.PressSave();
					ts.Complete(this);
				}
			}
		}


		public virtual PRPayment OnBeforeRelease(PRPayment payment)
		{
			return payment;
		}

		private void UpdateVoidedCheck(PRPayment voidcheck)
		{
			foreach (PRPayment res in SpecificPayment.Select(voidcheck.OrigDocType, voidcheck.OrigRefNbr))
			{
				PRPayment payment = res;
				PRPayment cached = (PRPayment)PRDocument.Cache.Locate(payment);
				if (cached != null)
				{
					payment = cached;
				}

				payment.Voided = true;
				payment.Hold = false;
				PRDocument.Cache.Update(payment);
			}
		}

		private void ClosePayBatchIfAllPaymentsAreReleased(string payBatchNumber)
		{
			if (string.IsNullOrWhiteSpace(payBatchNumber))
				return;

			PRPayment nonReleasedPaymentInBatch = NonReleasedPayBatchPayments.SelectSingle(payBatchNumber);
			if (nonReleasedPaymentInBatch != null)
				return;

			PRBatch batch = PayBatches.Select(payBatchNumber);
			batch.Closed = true;
			PayBatches.Update(batch);
		}

		public virtual void RemoveFromPaymentBatch(PRPayment doc)
		{
			PXCache batchCache = Caches[typeof(PRCABatch)];
			PRCABatch batch = null;
			foreach (PXResult<CABatchDetail, PRCABatch> row in DirectDepositBatchAndDetails.Select(doc.OrigDocType, doc.OrigRefNbr))
			{
				batch = row;
				CABatchDetail detail = row;
				DirectDepositBatchAndDetails.Delete(detail);
			}

			object[] children = PXParentAttribute.SelectChildren(Caches[typeof(CABatchDetail)], batch, typeof(PRCABatch));
			if (children.Length == 0)
			{
				batchCache.Delete(batch);
				batchCache.Persist(PXDBOperation.Delete);
			}
			else
			{
				Caches[typeof(CABatchDetail)].Persist(PXDBOperation.Delete);
				var batchUpdateGraph = PXGraph.CreateInstance<PRCABatchUpdate>();
				batchUpdateGraph.Document.Current = batch;
				batchUpdateGraph.RecalcTotals();
				batchUpdateGraph.Document.Update(batch);
				batchUpdateGraph.Persist();
			}
		}

		protected virtual void PostReleaseForSettlementPayments(PRPayment payment)
		{
			if (payment.DocType == PayrollType.Final)
			{
				var prEmployee = PREmployee.PK.Find(this, payment.EmployeeID);
				prEmployee.ActiveInPayroll = false;
				EmployeePayrollSettings.Update(prEmployee);

				var activePosition = EmployeePositions.Select().FirstTableItems.SingleOrDefault(x => x.IsActive == true);
				if (activePosition != null)
				{
					activePosition.EndDate = payment.TerminationDate;
					activePosition.IsTerminated = true;
					activePosition.TermReason = payment.TerminationReason;
					activePosition.IsRehirable = payment.IsRehirable == true;
					EmployeePositions.SetValueExt<PRxEPEmployeePosition.settlementPaycheckRefNoteID>(activePosition, payment.NoteID);
					EmployeePositions.Update(activePosition);
				}
				else
				{
					var earliestEarning = SelectFrom<PREarningDetail>
						.Where<PREarningDetail.employeeID.IsEqual<P.AsInt>>
						.OrderBy<PREarningDetail.date.Asc>.View.Select(this, payment.EmployeeID).TopFirst;

					var employeeHistory = new EPEmployeePosition();
					employeeHistory.EmployeeID = payment.EmployeeID;
					employeeHistory.StartDate = earliestEarning.Date;
					employeeHistory.EndDate = payment.TerminationDate;
					employeeHistory.IsTerminated = true;
					employeeHistory.TermReason = payment.TerminationReason;
					employeeHistory.IsRehirable = payment.IsRehirable == true;
					EmployeePositions.SetValueExt<PRxEPEmployeePosition.settlementPaycheckRefNoteID>(employeeHistory, payment.NoteID);
					EmployeePositions.Insert(employeeHistory);
				}
			}
			else if (payment.DocType == PayrollType.VoidCheck)
			{
				var voidedPayment = SpecificPayment.Select(payment.OrigDocType, payment.OrigRefNbr).TopFirst;
				if(voidedPayment == null || voidedPayment.DocType != PayrollType.Final)
				{
					return;
				}

				var prEmployee = PREmployee.PK.Find(this, payment.EmployeeID);
				prEmployee.ActiveInPayroll = true;
				EmployeePayrollSettings.Update(prEmployee);

				var terminatedPosition = TerminatedEmployeePosition.Select(voidedPayment.NoteID).TopFirst;
				if (terminatedPosition != null)
				{
					if(terminatedPosition.PositionID == null)
					{
						EmployeePositions.Delete(terminatedPosition);
					}
					else
					{
						terminatedPosition.EndDate = null;
						terminatedPosition.IsTerminated = false;
						terminatedPosition.TermReason = null;
						terminatedPosition.IsRehirable = false;
						EmployeePositions.SetValueExt<PRxEPEmployeePosition.settlementPaycheckRefNoteID>(terminatedPosition, null);
						EmployeePositions.Update(terminatedPosition);
					}
				}
			}
		}

		#region Helpers

		public virtual IEnumerable<GLTran> WriteNetPayTransactions(PRPayment doc, CurrencyInfo currencyInfo)
		{
			const int shownAccountNumberSymbolsCount = 4;

			var paymentMethod = (PaymentMethod)PXSelectorAttribute.Select<PRPayment.paymentMethodID>(PRDocument.Cache, doc);
			var paymentMethodExt = paymentMethod.GetExtension<PRxPaymentMethod>();
			var cashAccount = (CashAccount)PXSelectorAttribute.Select<PRPayment.cashAccountID>(PRDocument.Cache, doc);
			var isDebit = doc.DrCr == GL.DrCr.Debit;
			var transactions = new List<GLTran>();

			if (paymentMethodExt.PRCreateBatchPayment == false || !DirectDepositSplits.Select(doc.DocType, doc.RefNbr).FirstTableItems.Any())
			{
				//Print check method
				GLTran tran = new GLTran();
				tran.SummPost = PRSetup.Current.SummPost;
				tran.BranchID = cashAccount.BranchID;
				tran.AccountID = cashAccount.AccountID;
				tran.SubID = cashAccount.SubID;
				tran.ReclassificationProhibited = true;
				tran.CuryDebitAmt = isDebit ? 0m : doc.NetAmount;
				tran.DebitAmt = isDebit ? 0m : doc.NetAmount;
				tran.CuryCreditAmt = isDebit ? doc.NetAmount : 0m;
				tran.CreditAmt = isDebit ? doc.NetAmount : 0m;
				tran.TranType = doc.DocType;
				tran.RefNbr = doc.RefNbr;
				tran.TranDesc = doc.DocDesc;
				tran.TranPeriodID = doc.PayPeriodID;
				tran.FinPeriodID = doc.FinPeriodID;
				tran.TranDate = doc.TransactionDate;
				tran.CuryInfoID = currencyInfo.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : doc.EmployeeID;
				tran.CATranID = doc.CATranID;
				transactions.Add(tran);
			}
			else
			{
				//Direct Deposit method
				foreach (PRDirectDepositSplit split in DirectDepositSplits.Select(doc.DocType, doc.RefNbr))
				{
					GLTran tran = new GLTran();
					tran.SummPost = false;
					tran.BranchID = cashAccount.BranchID;
					tran.AccountID = cashAccount.AccountID;
					tran.SubID = cashAccount.SubID;
					tran.ReclassificationProhibited = true;
					tran.CuryDebitAmt = isDebit ? 0m : split.Amount;
					tran.DebitAmt = isDebit ? 0m : split.Amount;
					tran.CuryCreditAmt = isDebit ? split.Amount : 0m;
					tran.CreditAmt = isDebit ? split.Amount : 0m;
					tran.TranType = doc.DocType;
					tran.RefNbr = doc.RefNbr;
					tran.TranPeriodID = doc.PayPeriodID;
					tran.FinPeriodID = doc.FinPeriodID;
					tran.TranDate = doc.TransactionDate;
					tran.CuryInfoID = currencyInfo.CuryInfoID;
					tran.Released = true;
					tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : doc.EmployeeID;
					tran.CATranID = split.CATranID;
					tran.TranDesc = GetTransactionDescription(doc, split, shownAccountNumberSymbolsCount);
					transactions.Add(tran);
				}
			}

			return transactions;
		}

		protected string GetTransactionDescription(PRPayment payment, PRDirectDepositSplit split, int shownAccountNumberSymbolsCount)
		{
			if (payment.CountryID == LocationConstants.USCountryCode)
			{
				return HideAccountNumber(split.BankRoutingNbr, shownAccountNumberSymbolsCount) + HideAccountNumber(split.BankAcctNbr, shownAccountNumberSymbolsCount);
			}
			else if (payment.CountryID == LocationConstants.CanadaCountryCode)
			{
				return HideAccountNumber(split.BankAcctNbrCan, shownAccountNumberSymbolsCount);
			}
			else
			{
				return "";
			}
		}

		protected virtual GLTran WriteEarning(PRPayment payment, PREarningDetail earningDetail, CurrencyInfo info, Batch batch)
		{
			var isDebit = payment.DrCr == GL.DrCr.Debit;

			GLTran tran = new GLTran();
			PRxGLTran glTranExt = PXCache<GLTran>.GetExtension<PRxGLTran>(tran);
			tran.SummPost = PRSetup.Current.SummPost;
			tran.BranchID = earningDetail.BranchID;
			tran.AccountID = earningDetail.AccountID;
			tran.SubID = earningDetail.SubID;
			tran.ReclassificationProhibited = true;
			tran.CuryDebitAmt = isDebit ? earningDetail.Amount : 0m;
			tran.DebitAmt = isDebit ? earningDetail.Amount : 0m;
			tran.CuryCreditAmt = isDebit ? 0m : earningDetail.Amount;
			tran.CreditAmt = isDebit ? 0m : earningDetail.Amount;
			tran.TranType = payment.DocType;
			tran.RefNbr = payment.RefNbr;
			tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.EarningDescriptionFormat, earningDetail.TypeCD, earningDetail.Date.Value.ToString("d"));
			tran.TranPeriodID = batch.TranPeriodID;
			tran.FinPeriodID = batch.FinPeriodID;
			tran.TranDate = payment.TransactionDate;
			tran.CuryInfoID = info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : earningDetail.EmployeeID;
			tran.NonBillable =
				PRSetup.Current.TimePostingOption == EPPostOptions.OverridePMInPayroll
				|| PRSetup.Current.TimePostingOption == EPPostOptions.OverridePMAndGLInPayroll
				|| PRSetup.Current.TimePostingOption == EPPostOptions.PostToOffBalance
				? earningDetail.IsTimeActivityBillable == false
				: EPEarningType.PK.Find(this, earningDetail.TypeCD)?.isBillable == false;
			tran.ProjectID = CostAssignmentType.GetEarningSetting(payment.LaborCostSplitType).AssignCostToProject && earningDetail.ProjectID != null ? earningDetail.ProjectID : ProjectDefaultAttribute.NonProject();
			tran.TaskID = CostAssignmentType.GetEarningSetting(payment.LaborCostSplitType).AssignCostToProject ? earningDetail.ProjectTaskID : null;
			tran.CostCodeID = earningDetail.CostCodeID;
			tran.InventoryID = earningDetail.LabourItemID;
			(tran.Qty, tran.UOM) = GetEarningExpenseQtyAndUOM(earningDetail);
			glTranExt.PayrollWorkLocationID = earningDetail.LocationID;
			glTranExt.EarningTypeCD = earningDetail.TypeCD;
			return tran;
		}

		protected virtual GLTran WriteReverseGLTran(PRPayment payment, GLTran glTran, string earningTypeCD)
		{
			GLTran reverseTransaction = PXCache<GLTran>.CreateCopy(glTran);
			reverseTransaction.Module = BatchModule.PR;
			reverseTransaction.TranType = payment.DocType;
			reverseTransaction.RefNbr = payment.RefNbr;
			reverseTransaction.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : payment.EmployeeID;
			reverseTransaction.TranDesc = string.Format(Messages.TimeTransactionReverse, glTran.TranDesc);
			reverseTransaction.BatchNbr = null;
			reverseTransaction.LineNbr = null;
			reverseTransaction.PMTranID = null;
			reverseTransaction.CATranID = null;
			reverseTransaction.TranID = null;
			reverseTransaction.Posted = false;

			if (!CostAssignmentType.GetEarningSetting(payment.LaborCostSplitType).AssignCostToProject)
			{
				reverseTransaction.ProjectID = null;
				reverseTransaction.TaskID = null;
			}

			reverseTransaction.CuryInfoID = glTran.CuryInfoID;
			reverseTransaction.OrigBatchNbr = glTran.BatchNbr;
			reverseTransaction.OrigModule = glTran.Module;
			reverseTransaction.OrigLineNbr = glTran.LineNbr;
			reverseTransaction.NoteID = null;
			reverseTransaction.TranDate = payment.TransactionDate;
			reverseTransaction.Released = true;

			if (payment.DocType != PayrollType.VoidCheck)
			{
				reverseTransaction.Qty = -1m * reverseTransaction.Qty;

				decimal? curyAmount = reverseTransaction.CuryCreditAmt;
				reverseTransaction.CuryCreditAmt = reverseTransaction.CuryDebitAmt;
				reverseTransaction.CuryDebitAmt = curyAmount;

				decimal? amount = reverseTransaction.CreditAmt;
				reverseTransaction.CreditAmt = reverseTransaction.DebitAmt;
				reverseTransaction.DebitAmt = amount;
			}

			PRxGLTran reverseExt = PXCache<GLTran>.GetExtension<PRxGLTran>(reverseTransaction);
			reverseExt.EarningTypeCD = earningTypeCD;

			return reverseTransaction;
		}

		protected virtual GLTran WriteDeduction(PRPayment payment, PRDeductionDetail deductionDetail, PRDeductCode deductCode, CurrencyInfo info, Batch batch)
		{
			var isDebit = payment.DrCr == GL.DrCr.Debit;

			GLTran tran = new GLTran();
			tran.SummPost = PRSetup.Current.SummPost;
			tran.BranchID = deductionDetail.BranchID;
			tran.AccountID = deductionDetail.AccountID;
			tran.SubID = deductionDetail.SubID;
			tran.ReclassificationProhibited = true;
			tran.CuryDebitAmt = isDebit ? 0m : deductionDetail.Amount;
			tran.DebitAmt = isDebit ? 0m : deductionDetail.Amount;
			tran.CuryCreditAmt = isDebit ? deductionDetail.Amount : 0m;
			tran.CreditAmt = isDebit ? deductionDetail.Amount : 0m;
			tran.TranType = payment.DocType;
			tran.RefNbr = payment.RefNbr;
			tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.DeductionLiabilityFormat, deductCode.CodeCD);
			tran.TranPeriodID = batch.TranPeriodID;
			tran.FinPeriodID = batch.FinPeriodID;
			tran.TranDate = payment.TransactionDate;
			tran.CuryInfoID = info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : deductionDetail.EmployeeID;
			(tran.Qty, tran.UOM) = GetDeductionLiabilityQtyAndUOM(deductionDetail);
			return tran;
		}

		protected virtual GLTran WriteBenefitExpense(PRPayment payment, PRBenefitDetail benefitDetail, PRDeductCode deductCode, CurrencyInfo info, Batch batch)
		{
			var isDebit = payment.DrCr == GL.DrCr.Debit;

			GLTran tran = new GLTran();
			tran.SummPost = PRSetup.Current.SummPost;
			tran.BranchID = benefitDetail.BranchID;
			tran.AccountID = benefitDetail.ExpenseAccountID;
			tran.SubID = benefitDetail.ExpenseSubID;
			tran.ReclassificationProhibited = true;
			tran.CuryDebitAmt = isDebit ? benefitDetail.Amount : 0m;
			tran.DebitAmt = isDebit ? benefitDetail.Amount : 0m;
			tran.CuryCreditAmt = isDebit ? 0m : benefitDetail.Amount;
			tran.CreditAmt = isDebit ? 0m : benefitDetail.Amount;
			tran.TranType = payment.DocType;
			tran.RefNbr = payment.RefNbr;
			tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.BenefitExpenseFormat, deductCode.CodeCD);
			tran.TranPeriodID = batch.TranPeriodID;
			tran.FinPeriodID = batch.FinPeriodID;
			tran.TranDate = payment.TransactionDate;
			tran.CuryInfoID = info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : benefitDetail.EmployeeID;
			tran.ProjectID = CostAssignmentType.GetBenefitSetting(payment.LaborCostSplitType).AssignCostToProject && benefitDetail.ProjectID != null ? benefitDetail.ProjectID : ProjectDefaultAttribute.NonProject();
			tran.TaskID = CostAssignmentType.GetBenefitSetting(payment.LaborCostSplitType).AssignCostToProject ? benefitDetail.ProjectTaskID : null;
			tran.CostCodeID = benefitDetail.CostCodeID;
			tran.InventoryID = benefitDetail.LabourItemID;
			(tran.Qty, tran.UOM) = GetBenefitExpenseQtyAndUOM(benefitDetail);
			return tran;
		}

		protected virtual GLTran WriteBenefitLiability(PRPayment payment, PRBenefitDetail benefitDetail, PRDeductCode deductCode, CurrencyInfo info, Batch batch)
		{
			var isDebit = payment.DrCr == GL.DrCr.Debit;

			GLTran tran = new GLTran();
			tran.SummPost = PRSetup.Current.SummPost;
			tran.BranchID = benefitDetail.BranchID;
			tran.AccountID = benefitDetail.LiabilityAccountID;
			tran.SubID = benefitDetail.LiabilitySubID;
			tran.ReclassificationProhibited = true;
			tran.CuryDebitAmt = isDebit ? 0m : benefitDetail.Amount;
			tran.DebitAmt = isDebit ? 0m : benefitDetail.Amount;
			tran.CuryCreditAmt = isDebit ? benefitDetail.Amount : 0m;
			tran.CreditAmt = isDebit ? benefitDetail.Amount : 0m;
			tran.TranType = payment.DocType;
			tran.RefNbr = payment.RefNbr;
			tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.BenefitLiabilityFormat, deductCode.CodeCD);
			tran.TranPeriodID = batch.TranPeriodID;
			tran.FinPeriodID = batch.FinPeriodID;
			tran.TranDate = payment.TransactionDate;
			tran.CuryInfoID = info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : benefitDetail.EmployeeID;
			(tran.Qty, tran.UOM) = GetBenefitLiabilityQtyAndUOM(benefitDetail);
			return tran;
		}

		protected virtual GLTran WriteTaxExpense(PRPayment payment, PRTaxDetail taxDetail, PRTaxCode taxCode, CurrencyInfo info, Batch batch)
		{
			var isDebit = payment.DrCr == GL.DrCr.Debit;

			GLTran tran = new GLTran();
			tran.SummPost = PRSetup.Current.SummPost;
			tran.BranchID = taxDetail.BranchID;
			tran.AccountID = taxDetail.ExpenseAccountID;
			tran.SubID = taxDetail.ExpenseSubID;
			tran.ReclassificationProhibited = true;
			tran.CuryDebitAmt = isDebit ? taxDetail.Amount : 0m;
			tran.DebitAmt = isDebit ? taxDetail.Amount : 0m;
			tran.CuryCreditAmt = isDebit ? 0m : taxDetail.Amount;
			tran.CreditAmt = isDebit ? 0m : taxDetail.Amount;
			tran.TranType = payment.DocType;
			tran.RefNbr = payment.RefNbr;
			tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TaxExpenseFormat, taxCode.TaxCD);
			tran.TranPeriodID = batch.TranPeriodID;
			tran.FinPeriodID = batch.FinPeriodID;
			tran.TranDate = payment.TransactionDate;
			tran.CuryInfoID = info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : taxDetail.EmployeeID;
			tran.ProjectID = CostAssignmentType.GetTaxSetting(payment.LaborCostSplitType).AssignCostToProject && taxDetail.ProjectID != null ? taxDetail.ProjectID : ProjectDefaultAttribute.NonProject();
			tran.TaskID = CostAssignmentType.GetTaxSetting(payment.LaborCostSplitType).AssignCostToProject ? taxDetail.ProjectTaskID : null;
			tran.CostCodeID = taxDetail.CostCodeID;
			tran.InventoryID = taxDetail.LabourItemID;
			(tran.Qty, tran.UOM) = GetTaxExpenseQtyAndUOM(taxDetail);
			return tran;
		}

		protected virtual GLTran WriteTaxLiability(PRPayment payment, PRTaxDetail taxDetail, PRTaxCode taxCode, CurrencyInfo info, Batch batch)
		{
			var isDebit = payment.DrCr == GL.DrCr.Debit;

			GLTran tran = new GLTran();
			tran.SummPost = PRSetup.Current.SummPost;
			tran.BranchID = taxDetail.BranchID;
			tran.AccountID = taxDetail.LiabilityAccountID;
			tran.SubID = taxDetail.LiabilitySubID;
			tran.ReclassificationProhibited = true;
			tran.CuryDebitAmt = isDebit ? 0m : taxDetail.Amount;
			tran.DebitAmt = isDebit ? 0m : taxDetail.Amount; ;
			tran.CuryCreditAmt = isDebit ? taxDetail.Amount : 0m;
			tran.CreditAmt = isDebit ? taxDetail.Amount : 0m;
			tran.TranType = payment.DocType;
			tran.RefNbr = payment.RefNbr;
			tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TaxLiabilityFormat, taxCode.TaxCD);
			tran.TranPeriodID = batch.TranPeriodID;
			tran.FinPeriodID = batch.FinPeriodID;
			tran.TranDate = payment.TransactionDate;
			tran.CuryInfoID = info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = PRSetup.Current.HideEmployeeInfo == true ? null : taxDetail.EmployeeID;
			(tran.Qty, tran.UOM) = GetTaxLiabilityQtyAndUOM(taxDetail);
			return tran;
		}

		/// <summary>
		/// Insert accumulated and used PTO
		/// </summary>
		public virtual void ProcessPTO(PRPayment doc)
		{
			foreach (IGrouping<string, PRPaymentPTOBank> bankGroup in PTOBanks.Select(doc.DocType, doc.RefNbr).FirstTableItems.GroupBy(x => x.BankID))
			{
				IPTOBank sourceBank = PTOHelper.GetBankSettings(this, bankGroup.Key, doc.EmployeeID.Value, doc.EndDate.Value);
				PRPaymentPTOBank lastEffectiveBank = PTOHelper.GetLastEffectiveBank(bankGroup, bankGroup.Key);
				PTOHelper.GetPTOBankYear(lastEffectiveBank.EffectiveStartDate.Value, sourceBank.PTOYearStartDate.Value, out DateTime ptoYearStartDate, out DateTime ptoYearEndDate);
				PTOHelper.PTOHistoricalAmounts historyToDate = PTOHelper.GetPTOHistory(this, doc.EndDate.Value, doc.EmployeeID.Value, sourceBank);

				foreach (PRPaymentPTOBank paymentBank in bankGroup)
				{
					if (paymentBank.EffectiveStartDate >= ptoYearStartDate && paymentBank.EffectiveEndDate <= ptoYearEndDate)
					{
						decimal newAccrualHours = paymentBank.IsActive == true ? paymentBank.TotalAccrual.GetValueOrDefault() : 0m;
						decimal newAccrualMoney = paymentBank.IsActive == true ? paymentBank.TotalAccrualMoney.GetValueOrDefault() : 0m;
						historyToDate.AccumulatedHours += newAccrualHours;
						historyToDate.AccumulatedMoney += newAccrualMoney;
						historyToDate.UsedHours += paymentBank.TotalDisbursement.GetValueOrDefault();
						historyToDate.UsedMoney += paymentBank.TotalDisbursementMoney.GetValueOrDefault();

						if (sourceBank.DisburseFromCarryover == true)
						{
							historyToDate.AvailableHours += paymentBank.CarryoverAmount.GetValueOrDefault();
							historyToDate.AvailableMoney += paymentBank.CarryoverMoney.GetValueOrDefault();
						}
						else
						{
							historyToDate.AvailableHours += newAccrualHours;
							historyToDate.AvailableMoney += newAccrualMoney;
						}

						historyToDate.AvailableHours -= paymentBank.TotalDisbursement.GetValueOrDefault();
						historyToDate.AvailableMoney -= paymentBank.TotalDisbursementMoney.GetValueOrDefault();
					}
				}

				lastEffectiveBank.AccumulatedAmount = historyToDate.AccumulatedHours;
				lastEffectiveBank.AccumulatedMoney = historyToDate.AccumulatedMoney;
				lastEffectiveBank.UsedAmount = historyToDate.UsedHours;
				lastEffectiveBank.UsedMoney = historyToDate.UsedMoney;
				lastEffectiveBank.AvailableAmount = historyToDate.AvailableHours;
				lastEffectiveBank.AvailableMoney = historyToDate.AvailableMoney;
				PTOBanks.Update(lastEffectiveBank);
			}
		}

		private void SegregateBatch(JournalEntry je, int? branchID, string curyID, DateTime? docDate, string finPeriodID, string description, CurrencyInfo curyInfo)
		{
			JournalEntry.SegregateBatch(je, BatchModule.PR, branchID, curyID, docDate, finPeriodID, description, curyInfo, null);
		}

		private string HideAccountNumber(string accountNumber, int shownSymbolsCount)
		{
			if (accountNumber.Length <= shownSymbolsCount)
				return new string('*', accountNumber.Length);

			return new string('*', accountNumber.Length - shownSymbolsCount) + accountNumber.Substring(accountNumber.Length - shownSymbolsCount);
		}

		public virtual void VerifyPayment(PRPayment doc)
		{
			if (doc.GrossAmount.GetValueOrDefault() == 0 && doc.DocType != PayrollType.Adjustment && doc.DocType != PayrollType.VoidCheck)
			{
				throw new PXException(Messages.CantReleasePaymentWithoutGrossPay);
			}

			PRDocument.Current = doc;

			if (doc.DocType == PayrollType.Final && PaymentsReleasedAfter.View.SelectMulti().Any())
			{
				throw new PXException(Messages.FinalPaycheckCantBeReleased, doc.PaymentDocAndRef);
			}

			if (doc.DocType == PayrollType.Adjustment)
			{
				PRPayment olderUnreleasedPayment = SelectFrom<PRPayment>
					.Where<PRPayment.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
						.And<PRPayment.transactionDate.IsLess<PRPayment.transactionDate.FromCurrent>>
						.And<PRPayment.released.IsNotEqual<True>>>
					.OrderBy<PRPayment.transactionDate.Asc>.View.Select(this).FirstOrDefault();

				if (olderUnreleasedPayment != null)
				{
					throw new PXException(PXMessages.LocalizeFormat(Messages.CannotReleaseBecauseOlderPaymentIsUnreleased, olderUnreleasedPayment.PaymentDocAndRef));
				}
			}
		}

		protected virtual (decimal qty, string uom) GetEarningExpenseQtyAndUOM(PREarningDetail earningDetail)
		{
			if (earningDetail.LabourItemID == null)
			{
				return GetDefaultLaborBurdenQtyAndUOM();
			}

			string uom = InventoryItem.PK.Find(this, earningDetail.LabourItemID)?.BaseUnit;
			if (uom == null)
			{
				return GetDefaultLaborBurdenQtyAndUOM();
			}

			decimal? qty = earningDetail.UnitType == UnitType.Hour ? earningDetail.Hours :
				earningDetail.UnitType == UnitType.Misc ? earningDetail.Units : 1;
			return (qty.GetValueOrDefault(), uom);
		}

		protected virtual (decimal qty, string uom) GetDeductionLiabilityQtyAndUOM(PRDeductionDetail _)
		{
			return GetDefaultLaborBurdenQtyAndUOM();
		}

		protected virtual (decimal qty, string uom) GetBenefitExpenseQtyAndUOM(PRBenefitDetail _)
		{
			return GetDefaultLaborBurdenQtyAndUOM();
		}

		protected virtual (decimal qty, string uom) GetBenefitLiabilityQtyAndUOM(PRBenefitDetail _)
		{
			return GetDefaultLaborBurdenQtyAndUOM();
		}

		protected virtual (decimal qty, string uom) GetTaxExpenseQtyAndUOM(PRTaxDetail _)
		{
			return GetDefaultLaborBurdenQtyAndUOM();
		}

		protected virtual (decimal qty, string uom) GetTaxLiabilityQtyAndUOM(PRTaxDetail _)
		{
			return GetDefaultLaborBurdenQtyAndUOM();
		}

		private (decimal qty, string uom) GetDefaultLaborBurdenQtyAndUOM()
		{
			return (0, null);
		}
		
		public virtual CurrencyInfo GetCurrencyInfo(PRPayment doc)
		{
			return PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(this, doc.CuryInfoID);
		}
		#endregion Helpers

		#region Obsolete
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		public static bool PaymentNotCreatingGLTrans(PRPayment payment)
		{
			return payment.DetailLinesCount == 0 && payment.TotalEarnings == 0m;
		}
		#endregion
	}

	[Serializable]
	[PXHidden]
	public class PRDocumentProcessFilter : IBqlTable
	{
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation>
		{
			public const string PutOnHold = "HLD";
			public const string RemoveFromHold = "RHL";
			public const string Calculate = "CAL";
			public const string Recalculate = "REC";
			public const string Release = "REL";
			public const string Void = "VOI";
		}
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Action")]
		[PXStringList(
			new string[] { operation.PutOnHold, operation.RemoveFromHold, operation.Calculate, operation.Recalculate, operation.Release, operation.Void },
			new string[] { Messages.PutOnHoldAction, Messages.RemoveFromHoldAction, Messages.Calculate, Messages.Recalculate, Messages.Release, Messages.Void })]
		public string Operation { get; set; }
		#endregion
	}
}
