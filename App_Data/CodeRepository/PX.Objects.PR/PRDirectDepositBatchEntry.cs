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

using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Payroll.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PX.Objects.PR
{
	/// <summary>
	/// This graph display CABatches created from PRPayments with a Direct Deposit payment method. It can release the batch and export ACH files.
	/// </summary>
	public class PRDirectDepositBatchEntry : PXGraph<PRDirectDepositBatchEntry>
	{
		private const string _DefaultAchFileName = "ACH";
		private bool _IsExportingPrenote = false;

		// Used in ACHDownloadProvider
		public static class ExportProviderParams
		{
			public const string IsPrenote = "IsPrenote";
		}

		#region Toolbar buttons
		public PXSave<PRCABatch> Save;
		public PXCancel<PRCABatch> Cancel;
		public PXDelete<PRCABatch> Delete;
		public PXFirst<PRCABatch> First;
		public PXPrevious<PRCABatch> Previous;
		public PXNext<PRCABatch> Next;
		public PXLast<PRCABatch> Last;
		#endregion

		#region Views

		public SelectFrom<PRCABatch>
			.Where<PRCABatch.origModule.IsEqual<GL.BatchModule.modulePR>>.View Document;

		public PXSetup<CASetup> CASetup;
		public PXSetup<PRSetup> PRSetup;
		public class SetupValidation : PRSetupValidation<PRDirectDepositBatchEntry> { }

		public BatchPaymentsDetailsSelect.View BatchPaymentsDetails;
		public class BatchPaymentsDetailsSelect : SelectFrom<CABatchDetail>
				.InnerJoin<PRPayment>
					.On<PRPayment.docType.IsEqual<CABatchDetail.origDocType>
					.And<PRPayment.refNbr.IsEqual<CABatchDetail.origRefNbr>>>
				.LeftJoin<PRDirectDepositSplit>
					.On<PRDirectDepositSplit.docType.IsEqual<CABatchDetail.origDocType>
					.And<PRDirectDepositSplit.refNbr.IsEqual<CABatchDetail.origRefNbr>>
					.And<PRDirectDepositSplit.lineNbr.IsEqual<CABatchDetail.origLineNbr>>>
				.Where<CABatchDetail.batchNbr.IsEqual<PRCABatch.batchNbr.AsOptional>>
		{ }

		public PXFilter<PRPaymentBatchFilter> Filter;

		public SelectFrom<PRPayment>
				.InnerJoin<CABatchDetail>.On<CABatchDetail.origDocType.IsEqual<PRPayment.docType>
					.And<CABatchDetail.origRefNbr.IsEqual<PRPayment.refNbr>
					.And<CABatchDetail.origModule.IsEqual<GL.BatchModule.modulePR>>>>
				.Where<CABatchDetail.batchNbr.IsEqual<CABatchDetail.batchNbr.AsOptional>>.View Payments;

		public PXSelectReadonly<CashAccountPaymentMethodDetail,
			Where<CashAccountPaymentMethodDetail.paymentMethodID, Equal<Current<PRCABatch.paymentMethodID>>,
				And<Current<PRPayment.docType>, IsNotNull,
				And<Current<PRPayment.refNbr>, IsNotNull,
				And<CashAccountPaymentMethodDetail.cashAccountID, Equal<Current<PRCABatch.cashAccountID>>,
				And<CashAccountPaymentMethodDetail.detailID, Equal<Required<CashAccountPaymentMethodDetail.detailID>>>>>>>> CashAccountSettings;

		public SelectFrom<PRDirectDepositSplit>
			.Where<PRDirectDepositSplit.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRDirectDepositSplit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View EmployeePaymentSplits;

		public SelectFrom<BAccount>
			.Where<BAccount.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>.View Employee;

		public SelectFrom<PRPayment>
			.Where<PRPayment.paymentBatchNbr.IsNull
				.And<PRPayment.paymentMethodID.IsEqual<PRCABatch.paymentMethodID.FromCurrent>
				.And<PRPayment.cashAccountID.IsEqual<PRCABatch.cashAccountID.FromCurrent>>
				.And<PRPayment.status.IsEqual<PaymentStatus.pendingPayment>>>>.View PaymentsToAdd;

		public SelectFrom<PRPaymentBatchExportHistory>
			.Where<PRPaymentBatchExportHistory.paymentBatchNbr.IsEqual<PRCABatch.batchNbr.FromCurrent>>
			.OrderBy<PRPaymentBatchExportHistory.exportDateTime.Desc>.View ExportHistory;

		public SelectFrom<PRPaymentBatchExportHistory>
			.Where<PRPaymentBatchExportHistory.paymentBatchNbr.IsEqual<PRCABatch.batchNbr.FromCurrent>>
			.AggregateTo<Max<PRPaymentBatchExportHistory.exportDateTime>>.View LatestExport;

		public SelectFrom<PRPaymentBatchExportDetails>
			.Where<PRPaymentBatchExportDetails.paymentBatchNbr.IsEqual<PRCABatch.batchNbr.FromCurrent>
				.And<PRPaymentBatchExportDetails.exportHistoryLineNbr.IsEqual<PRPaymentBatchExportHistory.lineNbr.AsOptional>>>.View ExportDetails;

		public SelectFrom<PRPaymentBatchExportHistory>
			.Where<PRPaymentBatchExportHistory.paymentBatchNbr.IsEqual<PRPaymentBatchExportHistory.paymentBatchNbr.FromCurrent>
				.And<PRPaymentBatchExportHistory.lineNbr.IsEqual<PRPaymentBatchExportHistory.lineNbr.FromCurrent>>>.View CurrentExportHistory;

		public SelectFrom<PaymentMethodAccount>
			.Where<PaymentMethodAccount.paymentMethodID.IsEqual<PRCABatch.paymentMethodID.FromCurrent>
				.And<PaymentMethodAccount.cashAccountID.IsEqual<PRCABatch.cashAccountID.FromCurrent>>>.View PaymentMethodAccount;

		public PXSelectJoin<PaymentMethod,
			LeftJoin<SYMapping, On<SYMapping.mappingID, Equal<PRxPaymentMethod.prBatchExportSYMappingID>>>,
				Where<PaymentMethod.paymentMethodID, Equal<Current<PRCABatch.paymentMethodID>>,
					And<PRxPaymentMethod.prCreateBatchPayment, Equal<True>,
					And<PRxPaymentMethod.prBatchExportSYMappingID, IsNotNull>>>> ExportScenario;

		public SelectFrom<CATran>
				.Where<CATran.origTranType.IsEqual<PRPayment.docType.FromCurrent>
					.And<CATran.origRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View CashAccountTransactions;
		#region Delegates

		public virtual IEnumerable batchPaymentsDetails()
		{
			if (Filter.Current == null || Document.Current?.PaymentMethodID == null)
			{
				yield break;
			}

			PXView view = new PXView(this, false, BatchPaymentsDetails.View.BqlSelect);
			var results = view.SelectMulti().Cast<PXResult<CABatchDetail, PRPayment, PRDirectDepositSplit>>();
			string status = results.Any() ? PaymentBatchStatus.GetStatus(results.Select(x => (PRPayment)x)) : PaymentBatchStatus.ReadyForExport;
			Filter.Current.PaymentBatchStatus = status;
			Filter.Current.ReleaseEnabled =
				(Filter.Current.PaymentBatchStatus == PaymentBatchStatus.Paid || Filter.Current.PaymentBatchStatus == PaymentBatchStatus.Closed) &&
				results.Select(item => (PRPayment)item).Any(item => PRPayChecksAndAdjustments.IsReleaseActionEnabled(item, PRSetup.Current.UpdateGL ?? false));

			Pay.SetEnabled(results.All(x => ((PRPayment)x).Status == PaymentStatus.PaymentBatchCreated
				|| ((PRPayment)x).Status == PaymentStatus.Released
				|| ((PRPayment)x).Status == PaymentStatus.Paid));

			Filter.Current.AllPaymentsFromUS = true;

			foreach (PXResult<CABatchDetail, PRPayment, PRDirectDepositSplit> row in results)
			{
				PRPayment payment = row;

				if (payment.CountryID != LocationConstants.USCountryCode && Filter.Current.AllPaymentsFromUS == true)
				{
					Filter.Current.AllPaymentsFromUS = false;
				}

				SetStatusAttribute.StatusSet(Caches[typeof(PRPayment)], row);
				PXUIFieldAttribute.SetWarning<PRPayment.status>(Caches[typeof(PRPayment)], payment,
					payment.Status == PaymentStatus.NeedCalculation ? Messages.NeedCalculationWarning : null);

				//Using PRDirectDepositSplit.Amount column to display payment amount since splits don't exist for Checks
				if (payment.IsPrintChecksPaymentMethod == true)
				{
					PRDirectDepositSplit split = row;
					split.Amount = payment.NetAmount;
				}

				yield return row;
			}
		}

		public virtual IEnumerable payments()
		{
			PXView view = new PXView(this, false, Payments.View.BqlSelect);
			return view.SelectMulti().GroupBy(x => ExtractPaymentKeys(x)).Select(x => x.First());
		}

		#endregion Delegates

		#endregion Views

		public PRDirectDepositBatchEntry()
		{
			BatchPaymentsDetails.AllowInsert = false;
			BatchPaymentsDetails.AllowUpdate = false;
			ExportHistory.AllowInsert = false;
			ExportHistory.AllowUpdate = false;
			ExportHistory.AllowDelete = false;
			ExportDetails.AllowInsert = false;
			ExportDetails.AllowUpdate = false;
			ExportDetails.AllowDelete = false;
			PaymentsToAdd.AllowInsert = false;
			PaymentsToAdd.AllowUpdate = false;
			PaymentsToAdd.AllowDelete = false;

			ActionMenu.MenuAutoOpen = true;
			ActionMenu.AddMenuAction(Export);
			ActionMenu.AddMenuAction(ExportPrenote);
			ActionMenu.AddMenuAction(Pay);
			ActionMenu.AddMenuAction(Release);
			ActionMenu.AddMenuAction(CancelPayment);
			ActionMenu.AddMenuAction(DisplayPayStubs);
			ActionMenu.AddMenuAction(PrintChecks);
		}

		#region Events

		//Fixes PRPayment.PaymentBatchNbr PXDBScalar that doesn't fetch value from this graph.
		public virtual void _(Events.RowSelecting<PRPayment> e)
		{
			if (e.Row == null || Document.Current == null)
			{
				return;
			}

			e.Row.PaymentBatchNbr = Document.Current.BatchNbr;
		}

		public virtual void _(Events.RowSelected<PRCABatch> e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<PRCABatch.tranDesc>(e.Cache, e.Row, e.Row.Released == false);
			bool isPrintedCheck = GetPaymentMethodExtension(this, Document.Current.PaymentMethodID)?.PRPrintChecks == true;
			DisplayPayStubs.SetCaption(isPrintedCheck ? Messages.PrintPayStubs : Messages.DisplayPayStubs);
			ShowExportDetails.SetCaption(isPrintedCheck ? Messages.PrintDetails : Messages.ExportDetails);
			if (Filter.Current != null)
			{
				EnableBatchModifications(e.Row, Filter.Current);
			}
			Document.AllowUpdate = e.Row.Released == false || this.IsImport;
		}

		public virtual void _(Events.RowSelected<PRPaymentBatchFilter> e)
		{
			Pay.SetCaption(PRSetup.Current.AutoReleaseOnPay == true ? Messages.ConfirmPaymentAndRelease : Messages.ConfirmPayment);
			Release.SetEnabled(e.Row.ReleaseEnabled == true);
			CancelPayment.SetEnabled(e.Row.PaymentBatchStatus == PaymentBatchStatus.Paid);
			Delete.SetEnabled(e.Row.PaymentBatchStatus == PaymentBatchStatus.WaitingPaycheckCalculation || e.Row.PaymentBatchStatus == PaymentBatchStatus.ReadyForExport);
			AddPayment.SetEnabled(e.Row.PaymentBatchStatus == PaymentBatchStatus.WaitingPaycheckCalculation || e.Row.PaymentBatchStatus == PaymentBatchStatus.ReadyForExport);
			DeleteBatchDetails.SetEnabled(e.Row.PaymentBatchStatus == PaymentBatchStatus.WaitingPaycheckCalculation || e.Row.PaymentBatchStatus == PaymentBatchStatus.ReadyForExport);

			bool isPrintedCheck = GetPaymentMethodExtension(this, Document.Current.PaymentMethodID)?.PRPrintChecks == true;
			PrintChecks.SetEnabled(isPrintedCheck && e.Row.PaymentBatchStatus != PaymentBatchStatus.WaitingPaycheckCalculation);
			Export.SetEnabled(!isPrintedCheck && e.Row.PaymentBatchStatus != PaymentBatchStatus.WaitingPaycheckCalculation);
			ExportPrenote.SetEnabled(!isPrintedCheck && e.Row.PaymentBatchStatus != PaymentBatchStatus.WaitingPaycheckCalculation && e.Row.AllPaymentsFromUS == true);

			if (Document.Current != null)
			{
				EnableBatchModifications(Document.Current, e.Row);
			}
			if (e.Row != null && string.IsNullOrEmpty(e.Row.NextCheckNbr))
			{
				TrySetNextCheckNbr(e.Row);
			}

			string exportError = e.Row.ExportReason == PaymentBatchExportReason.OtherReason && string.IsNullOrWhiteSpace(e.Row.OtherExportReason)
				? string.Format(Messages.CantBeEmpty, PXUIFieldAttribute.GetDisplayName<PRPaymentBatchFilter.otherExportReason>(e.Cache)) : null;
			PXUIFieldAttribute.SetError<PRPaymentBatchFilter.otherExportReason>(e.Cache, e.Row, exportError);

			PXUIFieldAttribute.SetEnabled<PRPaymentBatchFilter.otherPrintReason>(e.Cache, e.Row, e.Row.PrintReason == PaymentBatchPrintReason.OtherReason);
			string printError = e.Row.PrintReason == PaymentBatchPrintReason.OtherReason && string.IsNullOrWhiteSpace(e.Row.OtherPrintReason)
				? string.Format(Messages.CantBeEmpty, PXUIFieldAttribute.GetDisplayName<PRPaymentBatchFilter.otherPrintReason>(e.Cache)) : null;
			PXUIFieldAttribute.SetError<PRPaymentBatchFilter.otherPrintReason>(e.Cache, e.Row, printError);
		}

		public virtual void _(Events.FieldSelecting<PRPaymentBatchFilter.batchTotal> e)
		{
			if (e.Row == null || Document.Current?.BatchNbr == null)
				return;

			if (e.ReturnValue == null)
				e.ReturnValue = Document.Current.BatchTotal;

			PXUIFieldAttribute.SetWarning(e.Cache, e.Row, nameof(PRPaymentBatchFilter.batchTotal), e.ReturnValue == null ? Messages.NoAccessToAllEmployeesInDDBatch : null);
		}

		public virtual void _(Events.FieldSelecting<PRPaymentBatchExportHistory, PRPaymentBatchExportHistory.batchTotal> e)
		{
			if (e.Row?.LineNbr == null)
				return;

			if (e.Row.BatchTotal != 0 && e.Row.BatchTotal != ExportDetails.Select(e.Row.LineNbr).FirstTableItems.Sum(item => item.NetAmount.GetValueOrDefault()))
				e.ReturnValue = null;

			PXUIFieldAttribute.SetWarning(e.Cache, e.Row, nameof(PRPaymentBatchExportHistory.batchTotal), e.ReturnValue == null ? Messages.NoAccessToAllEmployeesInDDBatch : null);
		}

		public virtual void _(Events.FieldSelecting<PRCABatch.curyID> e)
		{
			PRCABatch row = e.Row as PRCABatch;
			if (row == null)
			{
				return;
			}

			CashAccount cashAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);
			if (cashAccount != null)
			{
				e.ReturnValue = cashAccount.CuryID;
			}
		}

		public virtual void _(Events.RowDeleted<PRCABatch> e)
		{
			if (e.Row == null)
			{
				return;
			}

			foreach (PRPayment payment in Payments.Select(e.Row.BatchNbr))
			{
				payment.PaymentBatchNbr = null;
				SetStatusAttribute.StatusSet(Payments.Cache, payment);
				Payments.Update(payment);
			}
		}

		public virtual void _(Events.RowDeleted<CABatchDetail> e)
		{
			if (e.Row == null)
			{
				return;
			}

			PRPayment payment = Payments.Search<PRPayment.docType, PRPayment.refNbr>(e.Row.OrigDocType, e.Row.OrigRefNbr).TopFirst;
			if (payment == null)
			{
				return;
			}

			payment.PaymentBatchNbr = null;
			SetStatusAttribute.StatusSet(Payments.Cache, payment);
			Payments.Update(payment);
		}

		public virtual void _(Events.RowInserted<PRPaymentBatchExportHistory> e)
		{
			if (e.Row == null)
			{
				return;
			}

			IEnumerable<PRPayment> list = Payments.Select().FirstTableItems;
			if (GetPaymentMethodExtension(this, Document.Current.PaymentMethodID)?.PRPrintChecks == true && e.Row.Reason != PaymentBatchExportReason.Initial)
			{
				list = list.Where(x => x.Selected == true);
			}

			foreach (PRPayment payment in list)
			{
				var detail = new PRPaymentBatchExportDetails();
				detail.ExportHistoryLineNbr = e.Row.LineNbr;
				detail.DocType = payment.DocType;
				detail.RefNbr = payment.RefNbr;
				detail.EmployeeID = payment.EmployeeID;
				detail.PayGroupID = payment.PayGroupID;
				detail.PayPeriodID = payment.PayPeriodID;
				detail.DocDesc = payment.DocDesc;
				detail.ExtRefNbr = payment.ExtRefNbr;
				detail.NetAmount = _IsExportingPrenote ? 0 : payment.NetAmount;
				detail.PaymentBranchID = payment.BranchID;
				ExportDetails.Insert(detail);
			}
		}

		public virtual void _(Events.FieldUpdated<PRPayment.extRefNbr> e)
		{
			Payments.Current = (PRPayment)e.Row;
			PRPayment payment = Payments.Current;
			CATran tran = CashAccountTransactions.SelectSingle();
			tran.ExtRefNbr = payment.ExtRefNbr;
			CashAccountTransactions.Update(tran);
		}

		#endregion Events

		#region Actions

		public PXAction<PRCABatch> ActionMenu;
		[PXUIField(DisplayName = Messages.Actions, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable actionMenu(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<PRCABatch> Export;
		[PXUIField(DisplayName = CA.Messages.Export, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(Category = "Processing")]
		public virtual IEnumerable export(PXAdapter adapter)
		{
			CheckPrevOperation();
			bool askForReason = ExportHistory.Select().Count > 0;
			if (askForReason)
			{
				ExportHistory.AskExt();
				if (Filter.Current.ExportReason == PaymentBatchExportReason.OtherReason
					&& string.IsNullOrWhiteSpace(Filter.Current.OtherExportReason))
				{
					throw new PXException(Messages.CantBeEmpty, PXUIFieldAttribute.GetDisplayName<PRPaymentBatchFilter.otherExportReason>(Caches[typeof(PRPaymentBatchFilter)]));
				}
			}

			PRCABatch document = this.Document.Current;
			if (document != null)
			{
				var result = (PXResult<PaymentMethod, SYMapping>)ExportScenario.Select();
				PaymentMethod paymentMethod = result;
				SYMapping mapping = result;
				if (paymentMethod != null && mapping != null)
				{
					PRPaymentBatchFilter filter = Filter.Current;

					LongOperationManager.StartOperation(cancellationToken =>
					{
						var graph = PXGraph.CreateInstance<PRDirectDepositBatchEntry>();
						graph.Document.Current = document;
						graph.Filter.Current = filter;
						graph.RunAchExport(mapping.Name, false, cancellationToken);
						PRPaymentBatchExportHistory hist = new PRPaymentBatchExportHistory();
						hist.ExportDateTime = DateTime.Now;
						if (askForReason)
						{
							hist.Reason = graph.Filter.Current.ExportReason == PaymentBatchExportReason.OtherReason ? string.Format(Messages.ExportReasonOtherFormat, graph.Filter.Current.OtherExportReason) : hist.Reason = graph.Filter.Current.ExportReason;
						}
						hist.BatchTotal = document.CuryDetailTotal;

						graph.ExportHistory.Insert(hist);
						graph.Actions.PressSave();
					});
				}
				else
				{
					throw new PXException(CA.Messages.CABatchExportProviderIsNotConfigured);
				}
			}
			return adapter.Get();
		}

		public PXAction<PRCABatch> ExportPrenote;
		[PXUIField(DisplayName = Messages.ExportPrenote, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(Category = "Processing")]
		public virtual IEnumerable exportPrenote(PXAdapter adapter)
		{
			CheckPrevOperation();
			PRCABatch document = this.Document.Current;
			if (document != null)
			{
				var result = (PXResult<PaymentMethod, SYMapping>)ExportScenario.Select();
				PaymentMethod paymentMethod = result;
				SYMapping mapping = result;
				if (paymentMethod != null && mapping != null)
				{
					LongOperationManager.StartOperation(cancellationToken =>
					{
						_IsExportingPrenote = true;
						var graph = PXGraph.CreateInstance<PRDirectDepositBatchEntry>();
						graph.Document.Current = document;
						graph.RunAchExport(mapping.Name, true, cancellationToken);
						PRPaymentBatchExportHistory hist = new PRPaymentBatchExportHistory();
						hist.ExportDateTime = DateTime.Now;
						hist.Reason = PaymentBatchExportReason.Prenote;
						hist.BatchTotal = 0;

						graph.ExportHistory.Insert(hist);
						graph.Actions.PressSave();
						_IsExportingPrenote = false;
					});
				}
				else
				{
					throw new PXException(CA.Messages.CABatchExportProviderIsNotConfigured);
				}
			}
			return adapter.Get();
		}

		public PXAction<PRCABatch> ViewPRDocument;
		[PXUIField(DisplayName = Messages.ViewPRDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable viewPRDocument(PXAdapter adapter)
		{
			CABatchDetail detail = BatchPaymentsDetails.Current;
			if (detail == null)
			{
				return adapter.Get();
			}

			PRPayment payment = PXSelect<PRPayment,
							Where<PRPayment.docType, Equal<Required<PRPayment.docType>>,
							And<PRPayment.refNbr, Equal<Required<PRPayment.refNbr>>>>>.Select(this, detail.OrigDocType, detail.OrigRefNbr);
			if (payment == null)
			{
				return adapter.Get();
			}

			var graph = PXGraph.CreateInstance<PRPayChecksAndAdjustments>();
			graph.Document.Current = graph.Document.Search<PRPayment.refNbr>(payment.RefNbr, payment.DocType);
			if (graph.Document.Current != null)
			{
				throw new PXRedirectRequiredException(graph, true, "") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return adapter.Get();
		}

		public PXAction<PRCABatch> Pay;
		[PXUIField(DisplayName = Messages.ConfirmPayment, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(Category = "Processing")]
		public virtual IEnumerable pay(PXAdapter adapter)
		{
			Actions.PressSave();

			PRCABatch document = this.Document.Current;

			// Acuminator disable once PX1008 LongOperationDelegateSynchronousExecution [to avoid errors during the process]
			PXLongOperation.StartOperation(this, delegate ()
			{
				using (var scope = new PXTransactionScope())
				{
					var graph = PXGraph.CreateInstance<PRDirectDepositBatchEntry>();
					graph.Document.Current = document;

					List<PRPayment> payments = graph.Payments.Select().FirstTableItems.ToList();
					var paymentsToRelease = new List<PRPayment>();
					foreach (PRPayment payment in payments)
					{
						if(payment.Released == true)
						{
							continue;
						}
						if (payment.Status == PaymentStatus.NeedCalculation)
						{
							var errorMessages = PXMessages.Localize(Messages.NeedCalculationWarning) + $" {payment.PaymentDocAndRef}";
							throw new PXException(errorMessages);
						}

						payment.Paid = true;
						graph.Payments.Update(payment);
						graph.Payments.Cache.PersistUpdated(payment);
						paymentsToRelease.Add(payment);
					}

					if (PRSetup.Current.AutoReleaseOnPay == true)
					{
						foreach (PRPayment payment in paymentsToRelease)
						{
							if (PRPayChecksAndAdjustments.HasAssociatedTaxFormBatch(payment))
							{
								if (Document.View.Ask(null, Messages.ConfirmationHeader, Messages.TaxFormToBeGenerated, MessageButtons.YesNo, PRPayChecksAndAdjustments.ConfirmCancelButtons, MessageIcon.Question) == WebDialogResult.No)
								{
									scope.Complete();
									return;
								}
							}

							PRPrepareTaxFormsMaint.CheckCancellationTaxForm(Document.View, payment);
						}

						var paychecksAndAdjustmentsGraph = PXGraph.CreateInstance<PRPayChecksAndAdjustments>();
						paychecksAndAdjustmentsGraph.ReleasePaymentList(paymentsToRelease, false);
					}

					scope.Complete();
				}
			});

			return adapter.Get();
		}

		public PXAction<PRCABatch> Release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(Category = "Processing")]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			List<PRPayment> paymentsToRelease = Payments.Select().FirstTableItems.ToList();

			foreach (PRPayment payment in paymentsToRelease)
			{
				if (PRPayChecksAndAdjustments.HasAssociatedTaxFormBatch(payment))
				{
					if (Document.View.Ask(null, Messages.ConfirmationHeader, Messages.TaxFormToBeGenerated, MessageButtons.YesNo, PRPayChecksAndAdjustments.ConfirmCancelButtons, MessageIcon.Question) == WebDialogResult.No)
					{
						return adapter.Get();
					}
				}

				PRPrepareTaxFormsMaint.CheckCancellationTaxForm(Document.View, payment);
			}

			PXLongOperation.StartOperation(this, delegate ()
			{
				var paychecksGraph = CreateInstance<PRPayChecksAndAdjustments>();
				paychecksGraph.ReleasePaymentList(paymentsToRelease, false);
			});
			return adapter.Get();
		}

		public PXAction<PRCABatch> CancelPayment;
		[PXUIField(DisplayName = Messages.CancelPayment, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(Category = "Corrections")]
		public virtual IEnumerable cancelPayment(PXAdapter adapter)
		{
			PXResultset<PRPayment> payments = Payments.Select();
			if (payments.FirstTableItems.Any(x => x.DocType == PayrollType.Adjustment))
			{
				string message = Messages.DeductsWillBeRemoved + Messages.PressOK;
				if (Document.Ask(message, MessageButtons.OKCancel) != WebDialogResult.OK)
				{
					return adapter.Get();
				}
			}

			PXLongOperation.StartOperation(this, delegate ()
			{
				var graph = PXGraph.CreateInstance<PRPayChecksAndAdjustments>();
				using (var scope = new PXTransactionScope())
				{
					foreach (PRPayment payment in payments)
					{
						graph.Document.Current = payment;
						bool containsNotMatchingDeducts = graph.GetChildDeductRecordsWithSourceNotMatching(
							out List<PRPaymentDeduct> notMatchingPaymentDeducts,
							out List<PRPaymentProjectPackageDeduct> notMatchingProjectPackages,
							out List<PRPaymentUnionPackageDeduct> notMatchingUnionPackages,
							out List<PRPaymentWCPremium> notMatchingWorkCodePackages);

						if (containsNotMatchingDeducts)
						{
							graph.DeleteChildDeductRecordsWithSourceNotMatching(notMatchingPaymentDeducts, notMatchingProjectPackages, notMatchingUnionPackages, notMatchingWorkCodePackages);
							graph.Document.UpdateCurrent();
							graph.Actions.PressSave();
							scope.Complete();
						}

						payment.Paid = false;
						graph.Document.UpdateCurrent();
						graph.Actions.PressSave();
					}

					scope.Complete();
				}
			});

			return adapter.Get();
		}

		public PXAction<PRCABatch> DisplayPayStubs;
		[PXUIField(DisplayName = Messages.DisplayPayStubs, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(Category = "Reports")]
		public virtual IEnumerable displayPayStubs(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				var parameters = new Dictionary<string, string>();
				parameters[PayStubsDirectDepositReportParameters.BatchNbr] = Document.Current.BatchNbr;
				throw new PXReportRequiredException(parameters, PayStubsDirectDepositReportParameters.ReportID, PXBaseRedirectException.WindowMode.NewWindow, Messages.DisplayPayStubs);
			}

			return adapter.Get();
		}

		public PXAction<PRCABatch> AddPayment;
		[PXUIField(DisplayName = Messages.AddPayment, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual void addPayment()
		{
			if (PaymentsToAdd.AskExt() == WebDialogResult.OK && PaymentsToAdd.Current != null)
			{
				IEnumerable<CABatchDetail> details;
				if (GetPaymentMethodExtension(this, Document.Current.PaymentMethodID)?.PRPrintChecks == true)
				{
					var newDetail = new CABatchDetail();
					newDetail.BatchNbr = Document.Current.BatchNbr;
					newDetail.OrigRefNbr = PaymentsToAdd.Current.RefNbr;
					newDetail.OrigDocType = PaymentsToAdd.Current.DocType;
					newDetail.OrigModule = BatchModule.PR;
					details = new List<CABatchDetail>() { newDetail };
				}
				else
				{
					details = CreatePaymentBatchDetails(this, PaymentsToAdd.Current.DocType, PaymentsToAdd.Current.RefNbr, Document.Current.BatchNbr);
				}

				details.ForEach(x => BatchPaymentsDetails.Insert(x));
				PaymentsToAdd.Current.PaymentBatchNbr = Document.Current.BatchNbr;
				SetStatusAttribute.StatusSet(Payments.Cache, PaymentsToAdd.Current);
				Payments.Update(PaymentsToAdd.Current);
			}
		}

		public PXAction<PRCABatch> DeleteBatchDetails;
		[PXUIField(DisplayName = Messages.Remove, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual void deleteBatchDetails()
		{
			var current = BatchPaymentsDetails.Current;
			foreach (CABatchDetail row in BatchPaymentsDetails.Select().FirstTableItems
				.Where(x => x.OrigDocType == current.OrigDocType
					&& x.OrigRefNbr == current.OrigRefNbr
					&& x.OrigModule == current.OrigModule))
			{
				BatchPaymentsDetails.Delete(row);
			}
		}

		public PXAction<PRCABatch> PrintChecks;
		[PXUIField(DisplayName = Messages.PrintChecks, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(Category = "Printing and Emailing")]
		public virtual IEnumerable printChecks(PXAdapter adapter)
		{
			var nbrOfExports = ExportHistory.Select().FirstTableItems.Count();

			if(nbrOfExports == 0)
			{
				SetPaymentsSelected(this);
			}

			if (nbrOfExports == 0 || Payments.AskExt((gr, view) =>
					{
						if(nbrOfExports > 0)
						{
							SetPaymentsSelected(this);
						}						
					}
			) == WebDialogResult.OK)
			{
				PRCABatch document = Document.Current;
				PRPaymentBatchFilter filter = Filter.Current;

				int startRow = PXView.StartRow;
				int totalRows = 0;

				List<PXView.PXSearchColumn> searchColumns = Payments.View.GetContextualExternalSearchColumns();
				var select = Payments.View.Select(
					null,
					null,
					searchColumns.GetSearches(),
					searchColumns.GetSortColumns(),
					searchColumns.GetDescendings(),
					Payments.View.GetExternalFilters(),
					ref startRow,
					PXView.MaximumRows,
					ref totalRows);

				List<PRPayment> payments =  new List<PRPayment>();

				foreach (PXResult<PRPayment, CABatchDetail> res in select)
				{
					PRPayment payment = res;
					if (payment.Selected == true)
					{
						payments.Add(payment);
					}
				}

				PXLongOperation.StartOperation(this, delegate ()
				{
					var graph = PXGraph.CreateInstance<PRDirectDepositBatchEntry>();
					graph.Document.Current = document;
					graph.Filter.Current = filter;

					if (graph.Filter.Current.PrintReason == PaymentBatchPrintReason.OtherReason
						&& string.IsNullOrWhiteSpace(graph.Filter.Current.OtherPrintReason))
					{
						throw new PXException(Messages.CantBeEmpty, PXUIFieldAttribute.GetDisplayName<PRPaymentBatchFilter.otherPrintReason>(Caches[typeof(PRPaymentBatchFilter)]));
					}

					string firstCheckNbr = string.Empty;
					string lastCheckNbr = string.Empty;
					decimal batchTotal = 0;
					string nextCheckNbr = nbrOfExports > 0 ? graph.Filter.Current.NextCheckNbr : null;
					foreach (PRPayment payment in payments)
					{
						if (nbrOfExports == 0)
						{
							nextCheckNbr = payment.ExtRefNbr;
						}
						if (string.IsNullOrWhiteSpace(nextCheckNbr))
						{
							throw new PXException(Messages.RequiredCheckNumber);
						}

						if (string.IsNullOrEmpty(firstCheckNbr) || string.Compare(firstCheckNbr, nextCheckNbr) > 0)
						{
							firstCheckNbr = nextCheckNbr;
						}
						if (string.IsNullOrEmpty(lastCheckNbr) || string.Compare(lastCheckNbr, nextCheckNbr) < 0)
						{
							lastCheckNbr = nextCheckNbr;
						}

						if (nbrOfExports > 0)
						{
							payment.ExtRefNbr = nextCheckNbr;
						nextCheckNbr = AutoNumberAttribute.NextNumber(nextCheckNbr);
						}

						graph.Payments.Current = payment;
						graph.Payments.Update(payment);

						batchTotal += payment.NetAmount.GetValueOrDefault();
					}

					PRPaymentBatchExportHistory hist = new PRPaymentBatchExportHistory();
					hist.ExportDateTime = DateTime.Now;
					if (nbrOfExports == 0)
					{
						hist.Reason = PaymentBatchPrintReason.Initial;
					}
					else
					{
						hist.Reason = graph.Filter.Current.PrintReason == PaymentBatchPrintReason.OtherReason ?
										string.Format(Messages.ExportReasonOtherFormat, graph.Filter.Current.OtherPrintReason) :
										hist.Reason = graph.Filter.Current.PrintReason;
					}
					hist.BatchTotal = batchTotal;
					graph.ExportHistory.Insert(hist);

					if (!string.IsNullOrWhiteSpace(lastCheckNbr))
					{
						PaymentMethodAccount paymentMethodAccount = graph.PaymentMethodAccount.SelectSingle();
						paymentMethodAccount.APLastRefNbr = lastCheckNbr;
						graph.PaymentMethodAccount.Update(paymentMethodAccount);
					}

					if(payments.Count() > 0)
					{
					graph.Actions.PressSave();
					PRPrintChecks.RedirectToPrintedChecks(
						graph,
						GetPaymentMethodExtension(graph, graph.Document.Current.PaymentMethodID),
						firstCheckNbr,
						lastCheckNbr,
						graph.Document.Current.PaymentMethodID,
						graph.Document.Current.CashAccountID,
						PXBaseRedirectException.WindowMode.NewWindow);
					}					
				});
			}

			return adapter.Get();
		}

		public PXAction<PRCABatch> ShowExportDetails;
		[PXUIField(MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual void showExportDetails()
		{
			bool isPrintedCheck = GetPaymentMethodExtension(this, Document.Current.PaymentMethodID)?.PRPrintChecks == true;
			ExportDetails.Ask(isPrintedCheck ? Messages.PrintDetails : Messages.ExportDetails, null, MessageButtons.None, MessageIcon.None);
		}

		#endregion Actions

		public override void Persist()
		{
			bool batchInserted = Caches[typeof(PRCABatch)].Inserted.Any_();
			bool detailModified = Caches[typeof(CABatchDetail)].IsInsertedUpdatedDeleted;
			using (var scope = new PXTransactionScope())
			{
			base.Persist();

				if (batchInserted || detailModified)
			{
				PRCABatchUpdate.RecalculatePaymentBatchTotal(Document.Current);
			}

				scope.Complete();
			}
		}

		private void CheckPrevOperation()
		{
			if (PXLongOperation.Exists(UID))
			{
				throw new ApplicationException(GL.Messages.PrevOperationNotCompleteYet);
			}
		}

		public virtual string GenerateFileName(PRCABatch batch)
		{
			if (batch.CashAccountID != null && !string.IsNullOrEmpty(batch.PaymentMethodID))
			{
				CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, batch.CashAccountID);
				if (acct != null)
				{
					return string.Format(CA.Messages.CABatchDefaultExportFilenameTemplate, batch.PaymentMethodID, acct.CashAccountCD, batch.TranDate.Value, batch.DateSeqNbr);
				}
			}

			return _DefaultAchFileName;
		}

		public virtual void SetPaycheckPaidStatus(bool newValue)
		{
			foreach (PRPayment payment in Payments.Select())
			{
				payment.Paid = newValue;
				Payments.Update(payment);
				Persist(typeof(PRPayment), PXDBOperation.Update);
			}
		}

		private void EnableBatchModifications(PRCABatch batch, PRPaymentBatchFilter filter)
		{
			bool allowBatchModifications = batch.Released == false && filter.PaymentBatchStatus == PaymentBatchStatus.ReadyForExport
								|| filter.PaymentBatchStatus == PaymentBatchStatus.WaitingPaycheckCalculation;
			Document.AllowUpdate = allowBatchModifications || this.IsImport;
			Document.AllowDelete = allowBatchModifications;
		}

		public static IEnumerable<CABatchDetail> CreatePaymentBatchDetails(PXGraph graph, string docType, string refNbr, string batchNbr)
		{
			var results = new List<CABatchDetail>();
			foreach (PRDirectDepositSplit split in SelectFrom<PRDirectDepositSplit>
				.Where<PRDirectDepositSplit.docType.IsEqual<P.AsString>
					.And<PRDirectDepositSplit.refNbr.IsEqual<P.AsString>>>.View.Select(graph, docType, refNbr))
			{
				var detail = new CABatchDetail();
				detail.BatchNbr = batchNbr;
				detail.OrigRefNbr = split.RefNbr;
				detail.OrigDocType = split.DocType;
				detail.OrigModule = BatchModule.PR;
				detail.OrigLineNbr = split.LineNbr;
				results.Add(detail);
			}

			return results;
		}

		private static PRxPaymentMethod GetPaymentMethodExtension(PXGraph graph, string paymentMethodID)
		{
			if (string.IsNullOrEmpty(paymentMethodID))
			{
				return null;
			}

			PaymentMethod paymentMethod = SelectFrom<PaymentMethod>.Where<PaymentMethod.paymentMethodID.IsEqual<P.AsString>>.View.Select(graph, paymentMethodID).TopFirst;
			return PXCache<PaymentMethod>.GetExtension<PRxPaymentMethod>(paymentMethod);
		}

		private void TrySetNextCheckNbr(PRPaymentBatchFilter filter)
		{
			if (Document.Current.CashAccountID == null || Document.Current.PaymentMethodID == null)
			{
				return;
			}

			try
			{
				filter.NextCheckNbr = PaymentRefAttribute.GetNextPaymentRef(this, Document.Current.CashAccountID, Document.Current.PaymentMethodID);
			}
			catch (AutoNumberException) { }
		}

		private static object ExtractPaymentKeys(object payment)
		{
			PRPayment row = (PXResult<PRPayment, CABatchDetail>)payment;
			return new { row.DocType, row.RefNbr };
		}

		private void SetPaymentsSelected(PXGraph graph)
		{
			foreach (PRPayment row in Payments.Select().FirstTableItems)
			{
				row.Selected = true;
				Payments.Update(row);
			}
		}

		private void RunAchExport(string mappingName, bool isPrenote, CancellationToken token)
		{
			SYExportProcess.RunScenario(mappingName, SYMapping.RepeatingOption.All, true, true, token,
				new PXSYParameter(CABatchEntry.ExportProviderParams.FileName, GenerateFileName(Document.Current)),
				new PXSYParameter(CABatchEntry.ExportProviderParams.BatchNbr, Document.Current.BatchNbr),
				new PXSYParameter(ExportProviderParams.IsPrenote, isPrenote.ToString()));
		}
	}

	#region Processing Graph Definition
	[PXHidden]
	public class PRCABatchUpdate : PXGraph<PRCABatchUpdate>
	{
		public PXSelect<PRCABatch> Document;
		public SelectFrom<PRPayment>
				.InnerJoin<CABatchDetail>
					.On<PRPayment.docType.IsEqual<CABatchDetail.origDocType>
					.And<PRPayment.refNbr.IsEqual<CABatchDetail.origRefNbr>>>
				.Where<CABatchDetail.batchNbr.IsEqual<PRCABatch.batchNbr.FromCurrent>>.View BatchPaymentsDetails;

		public virtual void RecalcTotals()
		{
			PRCABatch row = this.Document.Current;
			if (row != null)
			{
				var total = BatchPaymentsDetails.Select().FirstTableItems.GroupBy(x => new { x.DocType, x.RefNbr }).Sum(x => x.First().NetAmount);
				row.CuryDetailTotal = total;
				row.DetailTotal = total;
				row.Total = total;
			}
		}

		public static void RecalculatePaymentBatchTotal(PRCABatch batch)
		{
			if (batch == null || string.IsNullOrWhiteSpace(batch.BatchNbr))
			{
				return;
			}

			var batchUpdateGraph = PXGraph.CreateInstance<PRCABatchUpdate>();
			batchUpdateGraph.Document.Current = SelectFrom<PRCABatch>.Where<PRCABatch.batchNbr.IsEqual<P.AsString>>.View.SelectSingleBound(batchUpdateGraph, null, batch.BatchNbr);
			batchUpdateGraph.RecalcTotals();
			batchUpdateGraph.Document.Update(batchUpdateGraph.Document.Current);
			batchUpdateGraph.Persist();
		}
	}
	#endregion

	public class PRPaymentBatchFilter : IBqlTable
	{
		#region PaymentBatchStatus
		public abstract class paymentBatchStatus : PX.Data.BQL.BqlString.Field<paymentBatchStatus> { }
		[PXString(3, IsFixed = true)]
		[PXUnboundDefault(PR.PaymentBatchStatus.ReadyForExport)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		[PaymentBatchStatus.List]
		public virtual string PaymentBatchStatus { get; set; }
		#endregion

		#region BatchTotal
		public abstract class batchTotal : PX.Data.BQL.BqlDecimal.Field<batchTotal> { }
		[PXDecimal]
		[PXUIField(DisplayName = "Batch Total", Enabled = false)]
		public virtual decimal? BatchTotal { get; set; }
		#endregion

		#region ReleaseEnabled
		public abstract class releaseEnabled : PX.Data.BQL.BqlBool.Field<releaseEnabled> { }
		[PXBool]
		public virtual bool? ReleaseEnabled { get; set; }
		#endregion

		#region ExportReason
		public abstract class exportReason : PX.Data.BQL.BqlString.Field<exportReason> { }
		[PXString]
		[PXUIField(DisplayName = "Reason")]
		[PaymentBatchExportReason]
		public virtual string ExportReason { get; set; }
		#endregion

		#region OtherExportReason
		public abstract class otherExportReason : PX.Data.BQL.BqlString.Field<otherExportReason> { }
		[PXString]
		[PXUIField(DisplayName = "Other Reason")]
		[PXUIEnabled(typeof(PRPaymentBatchFilter.exportReason.IsEqual<PaymentBatchReason.otherReason>))]
		public virtual string OtherExportReason { get; set; }
		#endregion

		#region PrintReason
		public abstract class printReason : PX.Data.BQL.BqlString.Field<exportReason> { }
		[PXString]
		[PXUIField(DisplayName = "Print Reason")]
		[PaymentBatchPrintReason]
		[PXDefault(PaymentBatchReason.PrinterIssue)]
		public virtual string PrintReason { get; set; }
		#endregion

		#region OtherPrintReason
		public abstract class otherPrintReason : PX.Data.BQL.BqlString.Field<otherPrintReason> { }
		[PXString]
		[PXUIField(DisplayName = "Other Reason")]
		public virtual string OtherPrintReason { get; set; }
		#endregion

		#region NextCheckNbr
		public abstract class nextCheckNbr : PX.Data.BQL.BqlString.Field<nextCheckNbr> { }
		[PXString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Next Check Number")]
		public virtual string NextCheckNbr { get; set; }
		#endregion

		#region AllPaymentsFromUS
		public abstract class allPaymentsFromUS : PX.Data.BQL.BqlBool.Field<allPaymentsFromUS> { }
		[PXBool]
		public virtual bool? AllPaymentsFromUS { get; set; }
		#endregion
	}
}
