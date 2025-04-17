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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;
using PX.ACHPlugInBase;
using PX.Api;
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.AP.MigrationMode;
using PX.Objects.Common;
using PX.Common;
using PX.SM;
using PaymentInstruction = PX.ACHPlugInBase.PaymentMethodDetail;

namespace PX.Objects.CA
{
	public class CABatchEntry : PXGraph<CABatchEntry>, PX.ACHPlugInBase.IACHDataProvider
	{
		#region Internal Variables
		private bool IsPrintProcess { get; set; } = false;
		#endregion

		#region Toolbar buttons
		public PXSave<CABatch> Save;
		public PXInsert<CABatch> Insert;
		public PXCancel<CABatch> Cancel;
		public PXDelete<CABatch> Delete;
		public PXFirst<CABatch> First;
		public PXPrevious<CABatch> Previous;
		public PXNext<CABatch> Next;
		public PXLast<CABatch> Last;
		#endregion

		#region Internal defintions
		public static class ExportProviderParams
		{
			public const string FileName = "FileName";
			public const string BatchNbr = "BatchNbr";
			public const string BatchSequenceStartingNbr = "BatchStartNumber";
			public const string FileSeqNumber = "FileSeqNumber";
		}

		public class PrintProcessContext : IDisposable
		{
			private CABatchEntry BatchEntryGraph { get; set; }

			public PrintProcessContext(CABatchEntry graph)
			{
				graph.IsPrintProcess = true;
				BatchEntryGraph = graph;
			}

			public void Dispose()
			{
				BatchEntryGraph.IsPrintProcess = false;
			}
		}
		#endregion

		#region Buttons
		public PXInitializeState<CABatch> initializeState;


		public PXAction<CABatch> putOnHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable PutOnHold(PXAdapter adapter) => adapter.Get();

		public PXAction<CABatch> releaseFromHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable ReleaseFromHold(PXAdapter adapter) => adapter.Get();

		public PXAction<CABatch> cancelBatch;
		[PXButton(CommitChanges = true, OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXUIField(DisplayName = "Cancel", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable CancelBatch(PXAdapter adapter)
		{
			Cancel.Press();
			CABatch document = this.Document.Current;

			// Acuminator disable once PX1008 LongOperationDelegateSynchronousExecution [due to no synchronous execution, but mixed updates. Will be fixed in future.]
			PXLongOperation.StartOperation(this, delegate ()
			{
				CancelBatchProc(document);
			});

			return adapter.Get();
		}

		private void CancelBatchProc(CABatch document)
		{
			using (var ts = new PXTransactionScope())
			{
				var graph = PXGraph.CreateInstance<CABatchUpdate>();
				graph.VoidAPPayments(document);

				document.Canceled = true;
				Document.Update(document);

				_IsCancelContext = true;

				foreach (CABatchDetail detail in this.Details.Select())
				{
					Details.Delete(detail);
				}
				_IsCancelContext = false;

				Save.Press();
				ts.Complete(this);
			}
		}

		public PXAction<CABatch> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			Save.Press();
			CABatch document = this.Document.Current;
			PXLongOperation.StartOperation(this, delegate () { CABatchEntry.ReleaseDoc(document); });

			return adapter.Get();
		}

		public PXAction<CABatch> setBalanced;
		[PXUIField(DisplayName = "Correct", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable SetBalanced(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<CABatch> export;
		[PXUIField(DisplayName = Messages.Export, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Export(PXAdapter adapter)
		{
			Save.Press();
			CABatch document = this.Document.Current;
			// Acuminator disable once PX1008 LongOperationDelegateSynchronousExecution [due to the mix of new and legacy code, the refactoring needed]
			ExportBatch(document);

			return adapter.Get();
		}

		private void ExportBatch(CABatch document)
		{
			PXResult<PaymentMethod, SYMapping> res = (PXResult<PaymentMethod, SYMapping>)PXSelectJoin<PaymentMethod,
								LeftJoin<SYMapping, On<SYMapping.mappingID, Equal<PaymentMethod.aPBatchExportSYMappingID>>>,
									Where<PaymentMethod.paymentMethodID, Equal<Optional<CABatch.paymentMethodID>>>>.Select(this, document.PaymentMethodID);
			PaymentMethod pt = res;
			SYMapping map = res;
			if (pt != null && pt.APCreateBatchPayment == true)
			{
				var sequenceNumber = document.DateSeqNbr;

				if (document.Exported != true || !sequenceNumber.HasValue)
				{
					sequenceNumber = GetNextDateSeqNbr(this, document);
				}

				if (pt.APBatchExportMethod == ACHExportMethod.PlugIn)
				{
					if (pt.APCreateBatchPayment == true && !string.IsNullOrEmpty(pt.APBatchExportPlugInTypeName))
					{
						IACHPlugIn plugin = GetPlugIn(pt);

						if (plugin != null)
						{
							try
							{
								plugin.VerifySettings(this, document.BatchNbr);

								// Acuminator disable once PX1008 LongOperationDelegateSynchronousExecution [due to the Export() doesn't causes any updates of caches and uses the graph as data provider for selects]
								PXLongOperation.StartOperation(this, delegate ()
								{
									ExportBatchByPlugIn(document, sequenceNumber, plugin);
								});
							}
							catch (InvalidMappingSettingException e)
							{
								Document.Cache.RaiseExceptionHandling<CABatch.paymentMethodID>(Document.Current, Document.Current.PaymentMethodID,
									new PXSetPropertyException(e.Message, PXErrorLevel.Error));
								throw;
							}
							catch (InvalidRemittanceSettingException e)
							{
								var ca = cashAccount.SelectSingle();
								Document.Cache.RaiseExceptionHandling<CABatch.cashAccountID>(Document.Current, ca?.CashAccountCD,
									new PXSetPropertyException(e.Message, PXErrorLevel.Error));
								throw;
							}
							catch (InvalidPaymentInstructionsException e)
							{
								foreach (var payment in e.IssuedPayments)
								{
									CABatchDetail detail = PXSelect<CABatchDetail, Where<CABatchDetail.batchNbr, Equal<Current<CABatch.batchNbr>>,
										And<CABatchDetail.origModule, Equal<BatchModule.moduleAP>,
										And<CABatchDetail.origDocType, Equal<Required<APPayment.docType>>,
										And<CABatchDetail.origRefNbr, Equal<Required<APPayment.refNbr>>>>>>>.Select(this, payment.DocType, payment.RefNbr);

									BatchPayments.Cache.RaiseExceptionHandling<CABatchDetail.origRefNbr>(detail, detail.OrigRefNbr,
										new PXSetPropertyException(e.GetErrorFor(payment), PXErrorLevel.RowError));
								}
								throw new PXException(PXLocalizer.LocalizeFormat(Messages.SomeVendorPaymentMethodSettingAreInvalidOrEmpty, this.Document.Current?.PaymentMethodID));
							}
						}
						else
						{
							throw new PXException(Messages.CABatchExportProviderIsNotConfigured);
						}
					}
					else
					{
						throw new PXException(Messages.CABatchExportProviderIsNotConfigured);
					}
				}

				if (pt.APBatchExportMethod == ACHExportMethod.ExportScenario)
				{
					VerifySettings();

					if (pt != null && pt.APCreateBatchPayment == true && pt.APBatchExportSYMappingID != null && map != null)
					{
						string defaultFileName = this.GenerateFileName(document);

						LongOperationManager.StartOperation(cancellationToken =>
						{
							PX.Api.SYExportProcess.RunScenario(map.Name,
								SYMapping.RepeatingOption.All,
								true,
								true,
								cancellationToken,
								new PX.Api.PXSYParameter(ExportProviderParams.FileName, defaultFileName),
								new PX.Api.PXSYParameter(ExportProviderParams.BatchNbr, document.BatchNbr),
								new PX.Api.PXSYParameter(ExportProviderParams.FileSeqNumber, sequenceNumber.ToString()));
						});
					}
					else
					{
						throw new PXException(Messages.CABatchExportProviderIsNotConfigured);
					}
				}
			}
			else
			{
				throw new PXException(Messages.CABatchExportProviderIsNotConfigured);
			}
		}

		protected virtual void ExportBatchByPlugIn(CABatch document, short? sequenceNumber, IACHPlugIn plugin)
		{
			var graph = PXGraph.CreateInstance<CABatchEntry>();
			graph.Document.Current = document;
			plugin.Export(graph, document.BatchNbr, sequenceNumber);
			graph.Save.Press();
		}

		private static IACHPlugIn GetPlugIn(PaymentMethod pt)
		{
			Type pluginType = PXBuildManager.GetType(pt.APBatchExportPlugInTypeName, true);
			IACHPlugIn plugin;

			if (typeof(PXGraph).IsAssignableFrom(pluginType))
			{
				plugin = PXGraph.CreateInstance(pluginType) as PX.ACHPlugInBase.IACHPlugIn;
			}
			else
			{
				plugin = Activator.CreateInstance(pluginType) as PX.ACHPlugInBase.IACHPlugIn;
			}

			return plugin;
		}

		public PXAction<CABatch> voidBatch;
		[PXUIField(DisplayName = Messages.Void, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable VoidBatch(PXAdapter adapter)
		{
			CABatch document = this.Document.Current;

			if (voidFilter.AskExt((graph, viewName) => InitializeVoidPanel()) == WebDialogResult.OK)
			{
				if (voidFilter.Current.VoidDateOption == VoidFilter.voidDateOption.SpecificVoidDate)
				{
					var voidDate = voidFilter.Current?.VoidDate;
					VerifyIfVoidDateIsEmpty(voidDate);
					VerifyIfPaymentsDateEarlierThenVoidDate(document, voidDate);

					document.VoidDate = voidDate;
					Document.Update(document);
					Save.Press();
				}

				PXLongOperation.StartOperation(this, delegate () { CABatchEntry.VoidBatchProc(document); });
			}

			return adapter.Get();
		}

		private void InitializeVoidPanel()
		{
			voidFilter.Cache.SetDefaultExt<VoidFilter.voidDateOption>(voidFilter.Current);
			voidFilter.Cache.SetDefaultExt<VoidFilter.voidDate>(voidFilter.Current);
		}

		private DateTime? GetLastPaymentDate()
		{
			APRegister latestPayment = PXSelectReadonly2<APRegister,
											InnerJoin<CABatchDetail,
												On<CABatchDetail.origModule, Equal<GL.BatchModule.moduleAP>,
												And<CABatchDetail.origDocType, Equal<APRegister.docType>,
												And<CABatchDetail.origRefNbr, Equal<APRegister.refNbr>>>>>,
											Where<CABatchDetail.batchNbr, Equal<Current<CABatch.batchNbr>>,
												And<APRegister.released, Equal<boolTrue>>>,
											OrderBy<Desc<APRegister.docDate>>>.Select(this);

			return latestPayment?.DocDate;
		}

		private void VerifyIfVoidDateIsEmpty(DateTime? voidDate)
		{
			if (!voidDate.HasValue)
			{
				voidFilter.Cache.RaiseExceptionHandling<VoidFilter.voidDate>(voidFilter.Current,
					null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error, "Void Date"));
				throw new PXSetPropertyException<AddPaymentsFilter.nextPaymentRefNumber>(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error, "Void Date");
			}
		}

		private void VerifyIfPaymentsDateEarlierThenVoidDate(CABatch document, DateTime? voidDate)
		{
			PXSelectBase<APRegister> selectReleased = new PXSelectReadonly2<APRegister,
											InnerJoin<CABatchDetail,
												On<CABatchDetail.origModule, Equal<GL.BatchModule.moduleAP>,
												And<CABatchDetail.origDocType, Equal<APRegister.docType>,
												And<CABatchDetail.origRefNbr, Equal<APRegister.refNbr>>>>>,
											Where<CABatchDetail.batchNbr, Equal<Required<CABatch.batchNbr>>,
												And<APRegister.released, Equal<boolTrue>,
												And<APRegister.docDate, Greater<Required<VoidFilter.voidDate>>>>>>(this);

			if (selectReleased.Select(document.BatchNbr, voidDate).Any())
			{
				var lastPaymentDate = GetLastPaymentDate();

				if (lastPaymentDate.HasValue)
				{
					var lastPaymentDateString = lastPaymentDate.Value.ToShortDateString();

					voidFilter.Cache.RaiseExceptionHandling<VoidFilter.voidDate>(voidFilter.Current,
						voidDate, new PXSetPropertyException(Messages.VoidDateCannotBeEarlierThanDateOfLastPaymentInBatch, PXErrorLevel.Error, lastPaymentDateString));
					throw new PXSetPropertyException<AddPaymentsFilter.nextPaymentRefNumber>(Messages.VoidDateCannotBeEarlierThanDateOfLastPaymentInBatch, PXErrorLevel.Error, lastPaymentDateString);
				}
			}
		}

		protected virtual void VerifySettings()
		{
			VerifyCashAccountPMDetailsSettings();
			VerifyVendorPaymentMethodDetailsSettings();
		}

		private void VerifyCashAccountPMDetailsSettings()
		{
			var details = new Dictionary<string, string>();

			foreach (PaymentMethodDetail detail in PXSelectReadonly<PaymentMethodDetail,
							Where<PaymentMethodDetail.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>,
								And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
								And<PaymentMethodDetail.isRequired, Equal<True>>>>>.Select(this))
			{
				details.Add(detail.DetailID, null);
			}


			foreach (CashAccountPaymentMethodDetail cadetail in PXSelectReadonly<CashAccountPaymentMethodDetail,
							Where<CashAccountPaymentMethodDetail.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>,
								And<CashAccountPaymentMethodDetail.cashAccountID, Equal<Current<CABatch.cashAccountID>>>>>.Select(this))
			{
				if(details.ContainsKey(cadetail.DetailID))
				{
					details[cadetail.DetailID] = cadetail.DetailValue;
				}
			}

			foreach(var detail in details)
			{
				if (!string.IsNullOrEmpty(detail.Value)) continue;
				var ca = cashAccount.SelectSingle();
				var pmdetail = PaymentMethodDetail.PK.Find(this, Document.Current?.PaymentMethodID, detail.Key);
				Document.Cache.RaiseExceptionHandling<CABatch.cashAccountID>(Document.Current, ca?.CashAccountCD, new PXSetPropertyException(Messages.CashAccountPMSettingsIsEmpty, PXErrorLevel.Error, pmdetail.Descr, ca?.CashAccountCD, this.Document.Current?.PaymentMethodID));

				throw new PXException(Messages.SomeCashAccountPMSettingsAreEmpty, ca?.CashAccountCD?.Trim(), this.Document.Current?.PaymentMethodID);
			}
		}

		private void VerifyVendorPaymentMethodDetailsSettings()
		{
			var errors = new Dictionary<string, PXSetPropertyException>();
			bool hasErrors = false;

			foreach (PXResult<CABatchDetail, APPayment> result in BatchPayments.Select())
			{
				var row = (CABatchDetail)result;
				var payment = (APPayment)result;
				var key = string.Format($"{payment.PaymentMethodID}_{payment.VendorID}_{payment.VendorLocationID}");

				if (errors.ContainsKey(key))
				{
					if (errors[key] != null)
					{
						BatchPayments.Cache.RaiseExceptionHandling<CABatchDetail.origRefNbr>(row, row.OrigRefNbr, errors[key]);
					}
					continue;
				}

				var vendorLocation = Location.PK.Find(this, payment.VendorID, payment.VendorLocationID);

				foreach (PXResult<PaymentMethodDetail, Location, VendorPaymentMethodDetail> caresult in PXSelectReadonly2<PaymentMethodDetail,
								LeftJoin<Location,
									On<Location.vPaymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
										And<Location.bAccountID, Equal<Required<Location.bAccountID>>,
										And<Location.locationID, Equal<Required<Location.locationID>>>>>,
								LeftJoin<VendorPaymentMethodDetail,
									On<VendorPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
										And<VendorPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
										And<VendorPaymentMethodDetail.bAccountID, Equal<Required<Location.bAccountID>>,
										And<VendorPaymentMethodDetail.locationID, Equal<Required<Location.locationID>>>>>>>>,
								Where<PaymentMethodDetail.paymentMethodID, Equal<Required<CABatch.paymentMethodID>>,
									And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
									And<PaymentMethodDetail.isRequired, Equal<True>>>>>.Select(this, payment.VendorID, vendorLocation.VPaymentInfoLocationID, payment.VendorID, vendorLocation.VPaymentInfoLocationID, payment.PaymentMethodID))
				{
					VendorPaymentMethodDetail vendorDetail = caresult;

					if (!string.IsNullOrEmpty(vendorDetail?.DetailValue)) continue;

					PaymentMethodDetail detail = caresult;
					var currentVendor = vendor.SelectSingle(payment.VendorID);
					errors[key] = new PXSetPropertyException(Messages.VendorPaymentMethodSettingIsEmpty, PXErrorLevel.RowError, detail.Descr, currentVendor?.AcctCD, this.Document.Current?.PaymentMethodID);
					hasErrors = true;
					break;
				}

				if (errors.ContainsKey(key) && errors[key] != null)
				{
					BatchPayments.Cache.RaiseExceptionHandling<CABatchDetail.origRefNbr>(row, row.OrigRefNbr, errors[key]);
				}
			}

			if (hasErrors)
			{
				throw new PXException(Messages.SomeVendorPaymentMethodSettingAreEmpty, this.Document.Current?.PaymentMethodID);
			}
		}

		public PXAction<CABatch> ViewAPDocument;
		[PXUIField(
			DisplayName = PO.Messages.ViewAPDocument,
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXLookupButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable viewAPDocument(PXAdapter adapter)
		{
			CABatchDetail detail = this.BatchPayments.Current;

			if (detail == null)
			{
				return adapter.Get();
			}

			APRegister apDocument = PXSelect<APRegister,
							Where<APRegister.docType, Equal<Required<APRegister.docType>>,
							And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.Select(this, detail.OrigDocType, detail.OrigRefNbr);

			if (apDocument == null)
			{
				return adapter.Get();
			}

			IDocGraphCreator creator = new APDocGraphCreator();

			PXGraph graph = creator.Create(apDocument.DocType, apDocument.RefNbr, apDocument.VendorID);
			if (graph != null)
			{
				throw new PXRedirectRequiredException(graph, true, "ViewDocument") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return adapter.Get();
		}

		public PXAction<CABatch> addPayments;
		[PXUIField(DisplayName = Messages.AddARPayments, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddPayments(PXAdapter adapter)
		{
			var batch = Document.Current;
			VerifyPaymentInfo(batch);

			if (batch?.CashAccountID == null || batch?.PaymentMethodID == null)
			{
				return adapter.Get();
			}

			if (batch != null && batch.Released != true)
			{
				var filterRow = filter.Current;

				bool needRefresh = false;
				if (this.AvailablePayments.AskExt((graph, viewName) => InitializeAddPaymentsPanel()) == WebDialogResult.OK)
				{
					var paymentList = new List<APPayment>();

					foreach (APPayment it in AvailablePayments.Select())
					{
						if (it.Selected != true)
						{
							continue;
						}

						if (string.IsNullOrEmpty(filterRow.NextPaymentRefNumber))
						{
							filter.Cache.RaiseExceptionHandling<AddPaymentsFilter.nextPaymentRefNumber>(filterRow,
								null, new PXSetPropertyException(AP.Messages.NextCheckNumberIsRequiredForProcessing));
							throw new PXSetPropertyException<AddPaymentsFilter.nextPaymentRefNumber>(AP.Messages.NextCheckNumberIsRequiredForProcessing);
						}
						else if (!string.IsNullOrEmpty(filterRow.NextPaymentRefNumber) && !AutoNumberAttribute.CanNextNumber(filterRow.NextPaymentRefNumber))
						{
							throw new PXSetPropertyException<AddPaymentsFilter.nextPaymentRefNumber>(AP.Messages.NextCheckNumberCanNotBeInc);
						}
						else if (IsNextNumberDuplicated(filterRow.NextPaymentRefNumber))
						{
							throw new PXSetPropertyException<AddPaymentsFilter.nextPaymentRefNumber>(AP.Messages.ConflictWithExistingCheckNumber, filterRow.NextPaymentRefNumber);
						}

						paymentList.Add(it);
						needRefresh = true;
					}

					try
					{
						PaymentMethodAccountHelper.VerifyAPLastReferenceNumberSettings(this, batch.PaymentMethodID, batch.CashAccountID, paymentList.Count, filterRow.NextPaymentRefNumber);
					}
					catch(PXException ex)
					{
						filter.Cache.RaiseExceptionHandling<AddPaymentsFilter.nextPaymentRefNumber>(filterRow, null, new PXSetPropertyException(ex.Message));
						throw;
					}

					if (paymentList.Any())
					{
						Save.Press();
						batch = Document.Current;
					}

					PXLongOperation.StartOperation(this, delegate () { AddSelectedPayments(paymentList, batch, filterRow.NextPaymentRefNumber); });
				}
				else
				{
					foreach (APPayment it in this.AvailablePayments.Cache.Inserted)
						it.Selected = false;
				}
				this.AvailablePayments.Cache.Clear();
				if (needRefresh)
				{
					this.BatchPayments.View.RequestRefresh();
				}
				this.AvailablePayments.View.RequestRefresh();
			}

			return new List<CABatch> { batch };
		}

		private void InitializeAddPaymentsPanel()
		{
			filter.Cache.SetDefaultExt<AddPaymentsFilter.nextPaymentRefNumber>(filter.Current);
		}

		protected virtual bool IsNextNumberDuplicated(string nextNumber)
		{
			var document = Document.Current;
			return AP.PaymentRefAttribute.IsNextNumberDuplicated(this, document.CashAccountID, document.PaymentMethodID, nextNumber);
		}

		protected virtual void VerifyPaymentInfo(CABatch batch)
		{
			if (batch?.CashAccountID == null)
			{
				Document.Cache.RaiseExceptionHandling<CABatch.cashAccountID>(batch, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error, "Cash Account"));
			}


			if (batch?.PaymentMethodID == null)
			{
				Document.Cache.RaiseExceptionHandling<CABatch.paymentMethodID>(batch, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error, "Payment Method"));
			}
		}

		public static void AddSelectedPayments(List<APPayment> docList, CABatch batch, string expectingCheckNumber)
		{
			var graph = PXGraph.CreateInstance<CABatchEntry>();
			var pc = PXGraph.CreateInstance<APPrintChecks>();
			var pe = PXGraph.CreateInstance<APPaymentEntry>();

			graph.Document.Current = graph.Document.Search<CABatch.batchNbr>(batch.BatchNbr);

			string NextCheckNbr = expectingCheckNumber;
			foreach (APPayment pmt in docList)
			{
				graph.PrintPaymentAndAddToBatch(graph, pc, pe, batch, pmt, ref NextCheckNbr);
			}
		}

		protected virtual void PrintPaymentAndAddToBatch(CABatchEntry graph, APPrintChecks printChecks, APPaymentEntry pe, CABatch batch, APPayment pmt, ref string NextCheckNbr)
		{
			APPayment payment = pmt;

			if (pmt.CashAccountID != batch.CashAccountID || pmt.PaymentMethodID != batch.PaymentMethodID)
			{
				throw new PXException(AP.Messages.APPaymentDoesNotMatchCABatchByAccountOrPaymentType);
			}

			if (string.IsNullOrEmpty(pmt.ExtRefNbr) && string.IsNullOrEmpty(NextCheckNbr))
			{
				throw new PXException(AP.Messages.NextCheckNumberIsRequiredForProcessing);
			}

			payment = pe.Document.Search<APPayment.refNbr>(payment.RefNbr, payment.DocType);
			if (payment.PrintCheck != true)
			{
				throw new PXException(AP.Messages.CantPrintNonprintableCheck);
			}
			if (payment.DocType.IsIn(APDocType.Check, APDocType.QuickCheck, APDocType.Prepayment) &&
				payment.Status != APDocStatus.PendingPrint)
			{
				throw new PXException(AP.Messages.ChecksMayBePrintedInPendingPrintStatus);
			}

			printChecks.AssignNumbers(pe, payment, ref NextCheckNbr, true);

			if (payment.Passed == true)
			{
				pe.TimeStamp = payment.tstamp;
			}

			graph.AddPayment(payment, true);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				pe.Save.Press();
				payment.tstamp = pe.TimeStamp;
				pe.Clear();

				graph.Save.Press();
				ts.Complete();
			}
		}
		#endregion

		#region Ctor + Selects

		public CABatchEntry()
		{
			CASetup setup = CASetup.Current;
			APSetup apSetup = APSetup.Current;

			RowUpdated.AddHandler<CABatch>(ParentFieldUpdated);

			APPaymentApplications.Cache.AllowInsert = false;
			APPaymentApplications.Cache.AllowDelete = false;
			APPaymentApplications.Cache.AllowUpdate = false;
			BatchPayments.AllowInsert = true;
		}

		public PXSelect<CABatch, Where<CABatch.origModule, Equal<GL.BatchModule.moduleAP>>> Document;

		public PXSelect<CABatchDetail, Where<CABatchDetail.batchNbr, Equal<Current<CABatch.batchNbr>>>> Details;
		public PXSelectJoin<CABatchDetail,
							LeftJoin<APPayment,
								On<CABatchDetail.origDocType, Equal<APPayment.docType>,
								And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>>>,
							LeftJoin<AP.Standalone.APRegisterAlias,
								On<AP.Standalone.APRegisterAlias.origDocType, Equal<CABatchDetail.origDocType>,
								And<AP.Standalone.APRegisterAlias.origRefNbr, Equal<CABatchDetail.origRefNbr>>>>>,
							Where<CABatchDetail.batchNbr, Equal<Current<CABatch.batchNbr>>>> BatchPayments;
		public PXSelect<APPayment> APPayments;
		public PXSelect<AP.Standalone.APRegisterAlias> APRegisterStandalone;

		public PXSelectJoin<APPayment,
							InnerJoin<CABatchDetail,
								On<CABatchDetail.origModule, Equal<GL.BatchModule.moduleAP>,
								And<CABatchDetail.origDocType, Equal<APPayment.docType>,
								And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>>>>,
							InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<APPayment.paymentMethodID>>>>,
							Where<CABatchDetail.batchNbr, Equal<Current<CABatch.batchNbr>>,
								And<Where<APPayment.curyOrigDocAmt, NotEqual<decimal0>, Or<PaymentMethod.skipPaymentsWithZeroAmt, NotEqual<True>>>>>> APPaymentList;

		public PXSelectJoin<APPayment,
							InnerJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>,
							LeftJoin<CABatchDetail, On<CABatchDetail.origDocType, Equal<APPayment.docType>,
											And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>,
											And<CABatchDetail.origModule, Equal<BatchModule.moduleAP>>>>>>,
								Where2<Where<APPayment.status, Equal<APDocStatus.pendingPrint>,
									And<APPayment.cashAccountID, Equal<Current<CABatch.cashAccountID>>,
									And<APPayment.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>,
									And<Match<Vendor, Current<AccessInfo.userName>>>>>>,
									And<APPayment.docType, In3<APDocType.check, APDocType.prepayment, APDocType.quickCheck>>>> AvailablePayments;

		public PXSelectJoin<Address, InnerJoin<Location, On<Location.vRemitAddressID, Equal<Address.addressID>>>,
									Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>,
									   And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>> VendorRemitAddress;

		public PXSelectJoin<Contact, InnerJoin<Location, On<Location.vRemitContactID, Equal<Contact.contactID>>>,
									Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>,
									   And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>> VendorRemitContact;

		public PXSelectJoin<APInvoice, InnerJoin<APAdjust, On<APInvoice.docType, Equal<APAdjust.adjdDocType>,
							And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>,
							InnerJoin<APPayment, On<APAdjust.adjgDocType, Equal<APPayment.docType>,
								And<APAdjust.adjgRefNbr, Equal<APPayment.refNbr>>>>>,
					Where<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
					And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>>>> APPaymentApplications;

		public PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<CABatch.cashAccountID>>>> cashAccount;
		public PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>>> paymentMethod;
		public PXSelect<ACHPlugInParameter, Where<ACHPlugInParameter.paymentMethodID, Equal<Required<ACHPlugInParameter.paymentMethodID>>,
				And<ACHPlugInParameter.plugInTypeName, Equal<Required<ACHPlugInParameter.plugInTypeName>>>>> PlugInParameters;
		public PXSelect<ACHPlugInParameter, Where<ACHPlugInParameter.paymentMethodID, Equal<Required<ACHPlugInParameter.paymentMethodID>>,
				And<ACHPlugInParameter.plugInTypeName, Equal<Required<ACHPlugInParameter.plugInTypeName>>,
				And<ACHPlugInParameter.parameterID, Equal<Required<ACHPlugInParameter.parameterID>>>>>> PlugInParameterByName;
		public PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>> vendor;

		public PXFilter<AddPaymentsFilter> filter;
		public PXFilter<VoidFilter> voidFilter;

		public PXSetup<CASetup> CASetup;
		public APSetupNoMigrationMode APSetup;
		public PXSetup<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Current<CABatch.cashAccountID>>, And<PaymentMethodAccount.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>>>> paymentMethodAccount;

		public PXSelectJoin<CABatchDetail,
							InnerJoin<APPayment,
								On<APPayment.docType, Equal<CABatchDetail.origDocType>,
								And<APPayment.refNbr, Equal<CABatchDetail.origRefNbr>,
								And<APPayment.released, Equal<True>>>>>,
							Where<CABatchDetail.batchNbr, Equal<Current<CABatch.batchNbr>>>> ReleasedPayments;

		#region Selects, used in export
		public PXSelectReadonly<CashAccountPaymentMethodDetail,
		Where<CashAccountPaymentMethodDetail.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>,
		And<Current<APPayment.docType>, IsNotNull,
		And<Current<APPayment.refNbr>, IsNotNull,
		And<CashAccountPaymentMethodDetail.cashAccountID, Equal<Current<CABatch.cashAccountID>>,
		And<CashAccountPaymentMethodDetail.detailID, Equal<Required<CashAccountPaymentMethodDetail.detailID>>>>>>>> cashAccountSettings;

		public PXSelectReadonly2<VendorPaymentMethodDetail,
				InnerJoin<Location, On<Location.bAccountID, Equal<VendorPaymentMethodDetail.bAccountID>,
					And<Location.vPaymentInfoLocationID, Equal<VendorPaymentMethodDetail.locationID>>>>,
				Where<VendorPaymentMethodDetail.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>,
					And<Current<APPayment.docType>, IsNotNull,
					And<Current<APPayment.refNbr>, IsNotNull,
					And<Location.bAccountID, Equal<Current<APPayment.vendorID>>,
					And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>,
					And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>>>> vendorPaymentSettings;

		#endregion
		#region Select for Addenda Info
		//public PXSelectJoin<AP.Standalone.APPayment,
		//				InnerJoin<AP.Standalone.APRegister, On<AP.Standalone.APPayment.docType, Equal<AP.Standalone.APRegister.docType>,
		//					And<AP.Standalone.APPayment.refNbr, Equal<AP.Standalone.APRegister.refNbr>>>,
		//				InnerJoin<AP.Standalone.APAdjust, On<AP.Standalone.APPayment.docType, Equal<AP.Standalone.APAdjust.adjgDocType>,
		//					And<AP.Standalone.APPayment.refNbr, Equal<AP.Standalone.APAdjust.adjgRefNbr>>>,
		//				InnerJoin<AP.Standalone.APInvoice, On<AP.Standalone.APInvoice.docType, Equal<AP.Standalone.APAdjust.adjdDocType>,
		//					And<AP.Standalone.APInvoice.refNbr, Equal<AP.Standalone.APAdjust.adjdRefNbr>>>,
		//				InnerJoin<AP.Standalone.APRegister2, On<AP.Standalone.APPayment.docType, Equal<AP.Standalone.APRegister2.docType>,
		//					And<AP.Standalone.APPayment.refNbr, Equal<AP.Standalone.APRegister2.refNbr>>>,
		//				InnerJoin<Vendor, On<AP.Standalone.APRegister.vendorID, Equal<Vendor.bAccountID>>>>>>>,
		//				Where<AP.Standalone.APPayment.docType, Equal<Current<CABatchDetail.origDocType>>,
		//					And<AP.Standalone.APPayment.refNbr, Equal<Current<CABatchDetail.origRefNbr>>>>> AddendaInfo;

		public PXSelectJoin<APPayment,
						InnerJoin<APAdjust, On<APPayment.docType, Equal<APAdjust.adjgDocType>,
							And<APPayment.refNbr, Equal<APAdjust.adjgRefNbr>>>,
						InnerJoin<APInvoice, On<APInvoice.docType, Equal<APAdjust.adjdDocType>,
							And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>,
						InnerJoin<Vendor, On<APRegister.vendorID, Equal<Vendor.bAccountID>>>>>,
						Where<APPayment.docType, Equal<Current<CABatchDetail.origDocType>>,
							And<APPayment.refNbr, Equal<Current<CABatchDetail.origRefNbr>>>>> AddendaInfo;
		#endregion
		#endregion
		#region View Delegates
		public virtual IEnumerable availablePayments()
		{
			CABatch doc = this.Document.Current;
			AddPaymentsFilter filter = this.filter.Current;
			if (doc == null || doc.CashAccountID == null || doc.PaymentMethodID == null || doc.Released == true)
			{
				yield break;
			}

			PXSelectBase<APPayment> apPaymentSelect = GetAPPaymentSelect();

			if (filter.EndDate.HasValue)
			{
				apPaymentSelect.WhereAnd<Where<APPayment.docDate, LessEqual<Current<AddPaymentsFilter.endDate>>>>();
			}
			if (filter.StartDate.HasValue)
			{
				apPaymentSelect.WhereAnd<Where<APPayment.docDate, GreaterEqual<Current<AddPaymentsFilter.startDate>>>>();
			}

			foreach (PXResult<APPayment, Vendor, CABatchDetail> it in apPaymentSelect.Select())
			{
				APPayment payment = it;

				if (CheckIfPaymentAdded(payment))
				{
					continue;
				}

				yield return it;
			}
		}

		protected virtual PXSelectBase<APPayment> GetAPPaymentSelect()
		{
			return new PXSelectJoin<APPayment,
									InnerJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>,
									LeftJoin<CABatchDetail, On<CABatchDetail.origDocType, Equal<APPayment.docType>,
													And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>,
													And<CABatchDetail.origModule, Equal<BatchModule.moduleAP>>>>>>,
										Where2<Where<APPayment.status, Equal<APDocStatus.pendingPrint>,
											And<APPayment.cashAccountID, Equal<Current<CABatch.cashAccountID>>,
											And<APPayment.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>,
											And<Match<Vendor, Current<AccessInfo.userName>>>>>>,
											And<APPayment.docType, In3<APDocType.check, APDocType.prepayment, APDocType.quickCheck>>>>(this);
		}

		private bool CheckIfPaymentAdded(APPayment payment)
		{
			return BatchPayments.Locate(CreateDetail(payment.DocType, payment.RefNbr))?.OrigRefNbr != null;
		}
		#endregion

		private CABatchDetail CreateDetail(string docType, string refNbr) =>
			new CABatchDetail { BatchNbr = Document.Current.BatchNbr, OrigModule = BatchModule.AP, OrigDocType = docType, OrigRefNbr = refNbr, OrigLineNbr = CABatchDetail.origLineNbr.DefaultValue };

		#region Events
		#region AddPaymentsFilter Events
		protected virtual void _(Events.RowSelected<AddPaymentsFilter> e)
		{
			PaymentMethodAccount accountDetail = paymentMethodAccount.Current;

			string errorMessage;
			PXUIFieldAttribute.SetEnabled<AddPaymentsFilter.nextPaymentRefNumber>(e.Cache, e.Row, accountDetail?.APAutoNextNbr != true);
			bool paymentSelected = CheckSelectedPayments();

			if (string.IsNullOrEmpty(e.Row.NextPaymentRefNumber))
			{
				e.Cache.RaiseExceptionHandling<AddPaymentsFilter.nextPaymentRefNumber>(e.Row, e.Row.NextPaymentRefNumber,
					new PXSetPropertyException(AP.Messages.NextCheckNumberIsRequiredForProcessing, paymentSelected ? PXErrorLevel.Error : PXErrorLevel.Warning));
			}
			else if (!string.IsNullOrEmpty(e.Row.NextPaymentRefNumber) && !AutoNumberAttribute.TryToGetNextNumber(e.Row.NextPaymentRefNumber))
			{
				e.Cache.RaiseExceptionHandling<AddPaymentsFilter.nextPaymentRefNumber>(e.Row, e.Row.NextPaymentRefNumber,
					new PXSetPropertyException(AP.Messages.NextCheckNumberCanNotBeInc, paymentSelected ? PXErrorLevel.Error : PXErrorLevel.Warning));
			}
			else if (IsNextNumberDuplicated(e.Row.NextPaymentRefNumber))
			{
				e.Cache.RaiseExceptionHandling<AddPaymentsFilter.nextPaymentRefNumber>(e.Row, e.Row.NextPaymentRefNumber,
					new PXSetPropertyException(AP.Messages.ConflictWithExistingCheckNumber, e.Row.NextPaymentRefNumber));
			}
			else if (filter.Current.SelectedCount > 0 &&
				!PaymentMethodAccountHelper.TryToVerifyAPLastReferenceNumber(this, Document.Current.PaymentMethodID, Document.Current.CashAccountID, out errorMessage, e.Row.SelectedCount.Value - 1, e.Row.NextPaymentRefNumber))
			{
				e.Cache.RaiseExceptionHandling<PrintChecksFilter.nextCheckNbr>(e.Row, filter.Current.NextPaymentRefNumber, new PXSetPropertyException(errorMessage));
			}
			else
			{
				e.Cache.RaiseExceptionHandling<AddPaymentsFilter.nextPaymentRefNumber>(e.Row, e.Row.NextPaymentRefNumber, null);
			}
		}

		private bool CheckSelectedPayments()
		{
			var paymentSelected = false;
			foreach (APPayment it in AvailablePayments.Select())
			{
				if (it.Selected == true)
				{
					paymentSelected = true;
					break;
				}
			}

			return paymentSelected;
		}

		protected virtual void _(Events.FieldDefaulting<AddPaymentsFilter.nextPaymentRefNumber> e)
		{
			PaymentMethodAccount accountDetail = paymentMethodAccount.Current;

			if (accountDetail != null && accountDetail.APAutoNextNbr == true)
			{
				try
				{
					e.NewValue = AP.PaymentRefAttribute.GetNextPaymentRef(this,
						paymentMethodAccount.Current.CashAccountID,
						paymentMethodAccount.Current.PaymentMethodID);
				}
				catch { }
			}
		}
		#endregion
		#region APPayment Events
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.FromRecord)]
		protected virtual void APPayment_tstamp_CacheAttached(PXCache sender) { }

		protected virtual void _(Events.RowUpdated<APPayment> e)
		{
			AddPaymentsFilter currentFilter = filter.Current;
			if (filter != null)
			{
				APPayment old_row = e.OldRow as APPayment;
				APPayment new_row = e.Row as APPayment;

				currentFilter.SelectedCount -= old_row.Selected == true ? 1 : 0;
				currentFilter.SelectedCount += new_row.Selected == true ? 1 : 0;
			}
		}
		#endregion
		#region CABatch Events
		protected virtual void CABatch_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CABatch row = e.Row as CABatch;

			if (row == null || IsPrintProcess)
				return;

			bool skipExport = row.SkipExport == true;
			bool isExported = row.Exported == true;
			bool isReleased = row.Released == true;
			bool isVoided = row.Voided == true;
			bool isCanceled = row.Canceled == true;

			PXUIFieldAttribute.SetEnabled(sender, row, false);
			PXUIFieldAttribute.SetEnabled<CABatch.batchNbr>(sender, row, true);

			PXUIFieldAttribute.SetEnabled<CABatch.exportFileName>(sender, row, IsExport);
			PXUIFieldAttribute.SetEnabled<CABatch.exportTime>(sender, row, IsExport);

			bool allowDelete = (skipExport ? !isReleased : !isExported) || isCanceled;

			if (allowDelete)
			{
				allowDelete = !ReleasedPayments.Select().Any();
			}

			sender.AllowInsert = row.Released != null && row.Exported != null;
			sender.AllowDelete = allowDelete;

			CashAccount cashaccount = (CashAccount)PXSelectorAttribute.Select<CABatch.cashAccountID>(sender, row);
			bool clearEnabled = row.Released != true && cashaccount?.Reconcile == true;

			PXUIFieldAttribute.SetEnabled<CABatch.hold>(sender, row, !isReleased && !isCanceled);
			PXUIFieldAttribute.SetEnabled<CABatch.tranDesc>(sender, row, !isReleased && !isCanceled);
			PXUIFieldAttribute.SetEnabled<CABatch.tranDate>(sender, row, !isReleased && !isCanceled);
			PXUIFieldAttribute.SetEnabled<CABatch.batchSeqNbr>(sender, row, !isReleased && !isCanceled);
			PaymentMethod pt = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
			if (pt != null)
				PXUIFieldAttribute.SetRequired<CABatch.batchSeqNbr>(sender, pt.RequireBatchSeqNum.Value);

			PXUIFieldAttribute.SetEnabled<CABatch.extRefNbr>(sender, row, !isReleased && !isCanceled);

			if (!isReleased && !isCanceled)
			{
				bool hasDetails = row.CountOfPayments > 0;

				PXUIFieldAttribute.SetEnabled<CABatch.paymentMethodID>(sender, row, !hasDetails);
				PXUIFieldAttribute.SetEnabled<CABatch.cashAccountID>(sender, row, !hasDetails);
			}

			PXUIFieldAttribute.SetVisible<CABatch.dateSeqNbr>(sender, row, skipExport ? isReleased : isExported);

			BatchPayments.Cache.AllowDelete = !isVoided || !isExported;
			this.AvailablePayments.Cache.AllowInsert = false;
			this.AvailablePayments.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(AvailablePayments.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<APPayment.selected>(AvailablePayments.Cache, null, true);
			PXUIFieldAttribute.SetVisible<AP.Standalone.APRegisterAlias.docDate>(APRegisterStandalone.Cache, null, isVoided);

			PXUIFieldAttribute.SetEnabled(BatchPayments.Cache, null, false);

			var pm = paymentMethod.SelectSingle();
			var enableAddenda = false;
				
			if (pm?.APBatchExportMethod == ACHExportMethod.PlugIn)
			{
				// Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers [due to it's an class initialization, not a graph creation]
				IACHPlugIn plugin = GetPlugIn(pm);
				enableAddenda = plugin?.IsAddendaRecordsEnabled(this) == true;
			}

			PXUIFieldAttribute.SetEnabled<CABatchDetail.addendaPaymentRelatedInfo>(Details.Cache, null, enableAddenda && !isExported && !isReleased);
			PXUIFieldAttribute.SetVisible<CABatchDetail.addendaPaymentRelatedInfo>(Details.Cache, null, enableAddenda);
			AddendaInfo.AllowSelect = false;
		}

		protected virtual void CABatch_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABatch row = (CABatch)e.Row;
			row.Cleared = false;
			row.ClearDate = null;

			if (cashAccount.Current == null || cashAccount.Current.CashAccountID != row.CashAccountID)
			{
				cashAccount.Current = (CashAccount)PXSelectorAttribute.Select<CABatch.cashAccountID>(sender, row);
			}

			if (cashAccount.Current.Reconcile != true)
			{
				row.Cleared = true;
				row.ClearDate = row.TranDate;
			}

			sender.SetDefaultExt<CABatch.referenceID>(e.Row);
			sender.SetDefaultExt<CABatch.paymentMethodID>(e.Row);

			this.AvailablePayments.Cache.Clear();
			this.AvailablePayments.Cache.ClearQueryCache();
			this.AvailablePayments.View.RequestRefresh();
		}

		protected virtual void CABatch_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABatch row = (CABatch)e.Row;
			sender.SetDefaultExt<CABatch.batchSeqNbr>(e.Row);
			this.AvailablePayments.Cache.Clear();
			this.AvailablePayments.Cache.ClearQueryCache();
			this.AvailablePayments.View.RequestRefresh();
		}

		protected virtual void CABatch_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			this._isMassDelete = true;
		}

		private bool _isMassDelete = false;

		protected virtual void CABatch_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			_isMassDelete = false;
		}

		protected virtual void _(Events.RowUpdated<CABatch> e)
		{
			if (e.Row.Exported == true && e.OldRow.Exported != true && e.Row.Released != true && e.Row.SkipExport != true)
			{
				var document = (CABatch)Document.Cache.CreateCopy(e.Row);
				document.DateSeqNbr = GetNextDateSeqNbr(this, document);
				Document.Cache.Update(document);
			}
		}
		#endregion

		#region CABatch Detail events
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Type")]
		[APDocType.List()]
		public virtual void CABatchDetail_OrigDocType_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Void Date", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual void _(Events.CacheAttached<AP.Standalone.APRegisterAlias.docDate> e) { }

		protected virtual void CABatchDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CABatchDetail row = (CABatchDetail)e.Row;
			bool isReleased = false;

			if (row.OrigModule == GL.BatchModule.AP)
			{
				APRegister apDocument = PXSelect<APRegister,
										   Where<APRegister.docType, Equal<Required<APRegister.docType>>,
											 And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>
										.Select(this, row.OrigDocType, row.OrigRefNbr);

				isReleased = (bool)apDocument.Released;
			}

			if (row.OrigModule == GL.BatchModule.AR)
			{
				ARRegister arDocument = PXSelect<ARRegister,
										   Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
											 And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>
										.Select(this, row.OrigDocType, row.OrigRefNbr);
				isReleased = (bool)arDocument.Released;
			}

			if (isReleased)
			{
				throw new PXException(Messages.ReleasedDocumentMayNotBeAddedToCABatch);
			}
		}

		protected virtual void CABatchDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			CABatchDetail row = (CABatchDetail)e.Row;
			UpdateDocAmount(row, false);
		}

		private bool _IsCancelContext { get; set; } = false;

		protected virtual void CABatchDetail_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			CABatchDetail row = (CABatchDetail)e.Row;
			bool isExportedOnly = Document.Current.Exported == true && Document.Current.Released != true;

			if (isExportedOnly && _IsCancelContext != true)
			{
				throw new PXException(Messages.PaymentIsIncludedInExportedCABatch, row.OrigRefNbr);
			}

			bool isReleased = false;
			bool isVoided = false;
			bool isCanceled = Document.Current.Canceled == true;

			if (!isCanceled)
			{
				if (row.OrigModule == GL.BatchModule.AP)
				{
					APRegister apDocument = PXSelect<APRegister, Where<APRegister.docType, Equal<Required<APRegister.docType>>,
												And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);
					isReleased = (bool)apDocument.Released;
					isVoided = (bool)apDocument.Voided;
				}

				if (row.OrigModule == GL.BatchModule.AR)
				{
					ARRegister arDocument = PXSelect<ARRegister, Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
												And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);
					isReleased = (bool)arDocument.Released;
					isVoided = (bool)arDocument.Voided;
				}

				if (isReleased && !isVoided)
				{
					throw new PXException(Messages.ReleasedDocumentMayNotBeDeletedFromCABatch);
				}
			}

			if (isReleased && !isVoided)
			{
				throw new PXException(Messages.ReleasedDocumentMayNotBeDeletedFromCABatch);
			}
		}

		protected virtual void CABatchDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			CABatchDetail row = (CABatchDetail)e.Row;

			if (!this._isMassDelete)
			{
				UpdateDocAmount(row, true);
				ChangeCountOfPayments(-1);
			}
			#region Update APPayment.status
			var cache = this.Caches<APPayment>();
			APPayment payment = PXSelect<APPayment,
								Where<APPayment.docType, Equal<Required<APPayment.docType>>,
									And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>
				.Select(this, row.OrigDocType, row.OrigRefNbr);
			APPayment.Events
				.Select(se => se.CancelPrintCheck)
				.FireOn(this, payment);
			#endregion

			this.AvailablePayments.View.RequestRefresh();
		}

		private CABatch UpdateDocAmount(CABatchDetail row, bool negative)
		{
			CABatch document = this.Document.Current;

			if (row.OrigDocType != null && row.OrigRefNbr != null)
			{
				decimal? curyAmount = null, amount = null;
				if (row.OrigModule == GL.BatchModule.AP)
				{
					APPayment payment = PXSelect<APPayment,
							Where<APPayment.docType, Equal<Required<APPayment.docType>>,
							And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);

					if (payment != null)
					{
						curyAmount = payment.CuryOrigDocAmt;
						amount = payment.OrigDocAmt;
					}
				}
				else
				{
					ARPayment payment = PXSelect<ARPayment,
							Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
							And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(this, row.OrigDocType, row.OrigRefNbr);

					if (payment != null)
					{
						curyAmount = payment.CuryOrigDocAmt;
						amount = payment.OrigDocAmt;
					}
				}

				if (curyAmount.HasValue)
				{
					document.CuryDetailTotal += negative ? -curyAmount : curyAmount;
				}

				if (amount.HasValue)
				{
					document.DetailTotal += negative ? -amount : amount;
				}

				document = this.Document.Update(document);
			}
			return document;
		}
		#endregion
		#region VoidFilter events
		protected virtual void _(Events.RowSelected<VoidFilter> e)
		{
			var isVoidDateAvailable = e.Row?.VoidDateOption == VoidFilter.voidDateOption.SpecificVoidDate;
			PXUIFieldAttribute.SetVisible<VoidFilter.voidDate>(e.Cache, e.Row, isVoidDateAvailable);
			PXUIFieldAttribute.SetEnabled<VoidFilter.voidDate>(e.Cache, e.Row, isVoidDateAvailable);
		}

		protected virtual void _(Events.FieldDefaulting<VoidFilter.voidDate> e)
		{
			var lastPaymentDate = GetLastPaymentDate();
			if (lastPaymentDate.HasValue)
			{
				e.NewValue = lastPaymentDate;
			}
		}
		#endregion
		#endregion

		#region Methods
		public virtual CABatchDetail AddPayment(APPayment aPayment, bool skipCheck)
		{
			if (!skipCheck)
			{
				foreach (CABatchDetail item in this.BatchPayments.Select())
				{
					if (IsKeyEqual(aPayment, item))
					{
						return item;
					}
				}
			}

			CABatchDetail detail = new CABatchDetail();
			detail.Copy(aPayment);
			detail = this.BatchPayments.Insert(detail);
			detail.AddendaPaymentRelatedInfo = BuildAddendaInfo(aPayment);
			detail = this.BatchPayments.Update(detail);

			ChangeCountOfPayments(+1);

			return detail;
		}

		private void ChangeCountOfPayments(int i)
		{
			var batch = Document.Current;
			batch.CountOfPayments += i;
			Document.Update(batch);
		}

		public virtual CABatchDetail AddPayment(ARPayment aPayment, bool skipCheck)
		{
			if (!skipCheck)
			{
				foreach (CABatchDetail item in this.BatchPayments.Select())
				{
					if (IsKeyEqual(aPayment, item))
					{
						return item;
					}
				}
			}

			CABatchDetail detail = new CABatchDetail();
			detail.Copy(aPayment);
			detail = this.BatchPayments.Insert(detail);

			ChangeCountOfPayments(+1);

			return detail;
		}

		private readonly SyFormulaProcessor _formulaProcessor = new SyFormulaProcessor();

		protected virtual string BuildAddendaInfo(APPayment aPayment)
		{
			var pm = paymentMethod.SelectSingle();
			if (pm.APBatchExportMethod != ACHExportMethod.PlugIn)
			{
				return null;
			}

			var plugIn = GetPlugIn(pm);
			if (plugIn == null)
			{
				return null;
			}

			var addendaTemplate = plugIn.GetAddendaRecordTemplate(this);
			if (string.IsNullOrEmpty(addendaTemplate))
			{
				return null;
			}

			AddendaInfo.AllowSelect = true;
			var addendaInfo = AddendaInfo.SelectSingle();
			var result = string.Empty;
			var lineResult = string.Empty;
			var results = new HashSet<string>();

			SyFormulaFinalDelegate fieldValueGetter;

			//foreach (PXResult<AP.Standalone.APPayment, AP.Standalone.APRegister, AP.Standalone.APAdjust, AP.Standalone.APInvoice, AP.Standalone.APRegister2, Vendor> info in AddendaInfo.Select())
			foreach (PXResult<APPayment, APAdjust, APInvoice, Vendor> info in AddendaInfo.Select())
			{
				//var payment = (AP.Standalone.APPayment)info;
				//var paymentReg = (AP.Standalone.APRegister)info;
				//var adjust = (AP.Standalone.APAdjust)info;
				//var invoice = (AP.Standalone.APInvoice)info;
				//var invoiceReg = (AP.Standalone.APRegister2)info;
				var payment = (APPayment)info;
				var adjust = (APAdjust)info;
				var invoice = (APInvoice)info;
				var vendor = (Vendor)info;

				fieldValueGetter = (names) =>
				{
					if (names.Length > 2)
					{
						names = new[] { string.Join(".", names.Take(names.Length - 1)), names.Last() };
					}
					if (names.Length == 1)
					{
						names = names[0].Split(SMNotificationMaint.ViewNameSeparator);
					}
					if (names.Length == 2)
					{
						string viewName = names[0];
						if (viewName == null) return null;
						string fieldName = names[1];
						if (fieldName == null) return null;

						PXCache cache = null;
						object data = null;

						if(AddendaAliases.Reverse.TryGetValue(viewName, out var typeName))
						{
							viewName = typeName;
						}

						switch(viewName)
						{
							case nameof(APPayment):
								cache = new PXCache<APPayment>(this);
								data = payment;
								break;
							case nameof(APAdjust):
								cache = new PXCache<APAdjust>(this);
								data = adjust;
								break;
							case nameof(APInvoice):
								cache = new PXCache<APInvoice>(this);
								data = invoice;
								break;
							case nameof(Vendor):
								cache = new PXCache<Vendor>(this);
								data = vendor;
								break;
						}

						return cache.GetValue(data, fieldName) ?? "";
					}
					throw new PXArgumentException(nameof(names), ErrorMessages.ArgumentOutOfRangeException);
				};

				lineResult = _formulaProcessor.Evaluate(addendaTemplate, fieldValueGetter).ToString();
				results.Add(lineResult);
				var iterationResult = string.Join("", results);

				if (iterationResult.Length > 80)
				{
					break;
				}
				else
				{
					result = iterationResult;
				}
			}

			if(string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(lineResult))
			{
				result = lineResult.Substring(0, 80);
			}

			AddendaInfo.AllowSelect = false;
			return result;
		}

		protected virtual void ParentFieldUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<CABatch.tranDate>(e.Row, e.OldRow))
			{
				foreach (CABatchDetail detail in this.Details.Select())
				{
					this.Details.Cache.MarkUpdated(detail);
				}
			}
		}
		#endregion

		#region Internal Utilities
		public virtual string GenerateFileName(CABatch aBatch)
		{
			if (aBatch.CashAccountID != null && !string.IsNullOrEmpty(aBatch.PaymentMethodID))
			{
				CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, aBatch.CashAccountID);
				if (acct != null)
				{
					return string.Format(Messages.CABatchDefaultExportFilenameTemplate, aBatch.PaymentMethodID, acct.CashAccountCD, aBatch.TranDate.Value, aBatch.DateSeqNbr);
				}
			}
			return string.Empty;
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R2)]
		public virtual void CalcDetailsTotal(ref decimal? aCuryTotal, ref decimal? aTotal)
		{
			aCuryTotal = 0m;
			aTotal = 0m;

			foreach (PXResult<CABatchDetail, APPayment> item in this.BatchPayments.Select())
			{
				APPayment payment = item;

				if (!string.IsNullOrEmpty(payment.RefNbr))
				{
					aCuryTotal += payment.CuryOrigDocAmt;
					aTotal += payment.OrigDocAmt;
				}
			}
		}
		#endregion

		#region Static Methods
		public static void ReleaseDoc(CABatch aDocument)
		{
			if ((bool)aDocument.Released || (bool)aDocument.Hold)
			{
				throw new PXException(Messages.CABatchStatusIsNotValidForProcessing);
			}

			CABatchUpdate batchEntry = PXGraph.CreateInstance<CABatchUpdate>();
			CABatch document = batchEntry.Document.Select(aDocument.BatchNbr);
			batchEntry.Document.Current = document;

			if ((bool)document.Released || (bool)document.Hold)
			{
				throw new PXException(Messages.CABatchStatusIsNotValidForProcessing);
			}

			PaymentMethod pt = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(batchEntry, document.PaymentMethodID);

			if (string.IsNullOrEmpty(document.BatchSeqNbr) && (pt.RequireBatchSeqNum == true))
			{
				batchEntry.Document.Cache.RaiseExceptionHandling<CABatch.batchSeqNbr>(document, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error, PXUIFieldAttribute.GetDisplayName<CABatch.batchSeqNbr>(batchEntry.Document.Cache)));
				throw new PXException(ErrorMessages.RecordRaisedErrors, Messages.Releasing, batchEntry.Document.Cache.DisplayName);
			}

			APRegister voided = PXSelectReadonly2<APRegister,
							InnerJoin<CABatchDetail, On<CABatchDetail.origModule, Equal<GL.BatchModule.moduleAP>,
							And<CABatchDetail.origDocType, Equal<APRegister.docType>,
							And<CABatchDetail.origRefNbr, Equal<APRegister.refNbr>>>>>,
							Where<CABatchDetail.batchNbr, Equal<Required<CABatch.batchNbr>>,
								And<APRegister.voided, Equal<True>>>>.Select(batchEntry, document.BatchNbr);

			if (voided != null && string.IsNullOrEmpty(voided.RefNbr) == false)
			{
				throw new PXException(Messages.CABatchContainsVoidedPaymentsAndConnotBeReleased);
			}

			List<APRegister> unreleasedList = new List<APRegister>();
			PXSelectBase<APPayment> selectUnreleased = new PXSelectReadonly2<APPayment,
							InnerJoin<CABatchDetail,
								On<CABatchDetail.origModule, Equal<GL.BatchModule.moduleAP>,
								And<CABatchDetail.origDocType, Equal<APPayment.docType>,
								And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>>>>>,
							Where<CABatchDetail.batchNbr, Equal<Optional<CABatch.batchNbr>>,
								And<APPayment.released, Equal<boolFalse>>>>(batchEntry);

			foreach (APPayment item in selectUnreleased.Select(document.BatchNbr))
			{
				if (item.Released != true)
				{
					unreleasedList.Add(item);
				}
			}

			if (unreleasedList.Count > 0)
			{
				APDocumentRelease.ReleaseDoc(unreleasedList, true);
			}

			selectUnreleased.View.Clear();

			APPayment payment = selectUnreleased.Select(document.BatchNbr);
			if (payment != null)
			{
				throw new PXException(Messages.CABatchContainsUnreleasedPaymentsAndCannotBeReleased);
			}

			document = batchEntry.ReleaseCABatch(document);
			batchEntry.Document.Cache.RestoreCopy(aDocument, document);
		}

		public static void VoidBatchProc(CABatch aDocument)
		{
			if ((bool)aDocument.Released != true || (bool)aDocument.Hold)
			{
				throw new PXException(Messages.CABatchStatusIsNotValidForProcessing);
			}

			CABatchUpdate batchEntry = PXGraph.CreateInstance<CABatchUpdate>();
			CABatch document = batchEntry.Document.Select(aDocument.BatchNbr);
			batchEntry.Document.Current = document;

			if ((bool)document.Released != true || (bool)document.Hold)
			{
				throw new PXException(Messages.CABatchStatusIsNotValidForProcessing);
			}

			PaymentMethod pt = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(batchEntry, document.PaymentMethodID);

			if (string.IsNullOrEmpty(document.BatchSeqNbr) && (pt.RequireBatchSeqNum == true))
			{
				batchEntry.Document.Cache.RaiseExceptionHandling<CABatch.batchSeqNbr>(document, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error, PXUIFieldAttribute.GetDisplayName<CABatch.batchSeqNbr>(batchEntry.Document.Cache)));
				throw new PXException(ErrorMessages.RecordRaisedErrors, Messages.Releasing, batchEntry.Document.Cache.DisplayName);
			}

			using (var ts = new PXTransactionScope())
			{
				batchEntry.VoidAPPayments(document);
				document = batchEntry.VoidCABatch(document);
				ts.Complete();
			}

			batchEntry.Document.Cache.RestoreCopy(aDocument, document);
		}

		public static bool IsKeyEqual(APPayment payment, CABatchDetail detail)
		{
			return (detail.OrigModule == BatchModule.AP && payment.DocType == detail.OrigDocType && payment.RefNbr == detail.OrigRefNbr);
		}

		public static bool IsKeyEqual(AR.ARPayment payment, CABatchDetail detail)
		{
			return (detail.OrigModule == BatchModule.AR && payment.DocType == detail.OrigDocType && payment.RefNbr == detail.OrigRefNbr);
		}

		public static short GetNextDateSeqNbr(PXGraph graph, CABatch aDocument)
		{
			short result = 0;
			PXSelectBase<CABatch> cmd = new PXSelectReadonly<CABatch,
							Where<CABatch.cashAccountID, Equal<Required<CABatch.cashAccountID>>,
							And<CABatch.paymentMethodID, Equal<Required<CABatch.paymentMethodID>>,
							And<CABatch.tranDate, Equal<Required<CABatch.tranDate>>,
							And<Where<CABatch.skipExport, Equal<True>, And<CABatch.released, Equal<True>,
								Or<CABatch.skipExport, NotEqual<True>, And<CABatch.exported, Equal<True>>>>>>>>>,
							OrderBy<Desc<CABatch.dateSeqNbr>>>(graph);

			CABatch lastCABatch = cmd.SelectSingle(aDocument.CashAccountID, aDocument.PaymentMethodID, aDocument.TranDate);

			if (lastCABatch != null)
			{
				result = lastCABatch.DateSeqNbr ?? (short)0;
				if (result >= short.MaxValue || result < short.MinValue)
				{
					throw new PXException(Messages.DateSeqNumberIsOutOfRange);
				}
				result++;
			}
			return result;
		}
		#endregion

		#region PX.ACHPlugInBase.IACHDataProvider implementation
		public BatchPayment GetBatchPayment(string batchNumber)
		{
			CABatch document = PXSelectReadonly<CABatch, Where<CABatch.origModule, Equal<GL.BatchModule.moduleAP>, And<CABatch.batchNbr, Equal<Required<CABatch.batchNbr>>>>>.Select(this, batchNumber);

			return new BatchPayment
			{
				BatchNumber = document.BatchNbr,
				CashAccountID = document.CashAccountID,
				PaymentMethodID = document.PaymentMethodID,
				DateSequenceNumber = document.DateSeqNbr ?? 0,
				TransactionDate = document.TranDate.Value,
				TransactionDescription = document.TranDesc,
				DateSeqNbr = document.DateSeqNbr,
			};
		}

		public IEnumerable<Payment> GetPayments(string batchNumber)
		{
			foreach (PXResult<APPayment, CABatchDetail> item in PXSelectJoin<APPayment,
							InnerJoin<CABatchDetail,
								On<CABatchDetail.origModule, Equal<GL.BatchModule.moduleAP>,
								And<CABatchDetail.origDocType, Equal<APPayment.docType>,
								And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>>>>,
							InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<APPayment.paymentMethodID>>>>,
							Where<CABatchDetail.batchNbr, Equal<Required<CABatch.batchNbr>>,
								And<Where<APPayment.curyOrigDocAmt, NotEqual<decimal0>, Or<PaymentMethod.skipPaymentsWithZeroAmt, NotEqual<True>>>>>>.Select(this, batchNumber))
			{
				APPayment payment = item;
				CABatchDetail batchDetail = item;

				yield return new Payment
				{
					DocType = payment.DocType,
					RefNbr = payment.RefNbr,
					ExtRefNbr = payment.ExtRefNbr,
					Amount = payment.CuryOrigDocAmt ?? 0m,
					VendorID = payment.VendorID,
					VendorLocationID = payment.VendorLocationID,
					AddendaPaymentRelatedInfo = batchDetail.AddendaPaymentRelatedInfo,
				};
			}
		}

		public Dictionary<string, PaymentInstruction> GetRemittanceDetails()
		{
			var remittanceDetails = new Dictionary<string, PaymentInstruction>();

			foreach (PXResult<PaymentMethodDetail, CashAccountPaymentMethodDetail> result in PXSelectReadonly2<PaymentMethodDetail,
							LeftJoin<CashAccountPaymentMethodDetail,
								On<CashAccountPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
									And<CashAccountPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>>>>,
							Where<PaymentMethodDetail.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>,
								And<CashAccountPaymentMethodDetail.cashAccountID, Equal<Current<CABatch.cashAccountID>>,
								And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>>>>>.Select(this))
			{
				var detail = (PaymentMethodDetail)result;
				var capmDetail = (CashAccountPaymentMethodDetail)result;

				remittanceDetails.Add(detail.DetailID.Trim(),
					new PaymentInstruction { DetailID = detail.DetailID, Description = detail.Descr, Value = capmDetail.DetailValue, Required = detail.IsRequired });
			}

			return remittanceDetails;
		}

		public Dictionary<string, PaymentInstruction> GetVendorDetails(int? vendorID, int? locationID)
		{
			var vendorDetails = new Dictionary<string, PaymentInstruction>();

			foreach (PXResult<PaymentMethodDetail, Location, VendorPaymentMethodDetail> result in PXSelectReadonly2<PaymentMethodDetail,
								LeftJoin<Location,
									On<Location.vPaymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
										And<Location.bAccountID, Equal<Required<Location.bAccountID>>,
										And<Location.locationID, Equal<Required<Location.locationID>>>>>,
								LeftJoin<VendorPaymentMethodDetail,
									On<VendorPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
										And<VendorPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
										And<VendorPaymentMethodDetail.bAccountID, Equal<Required<Location.bAccountID>>,
										And<VendorPaymentMethodDetail.locationID, Equal<Required<Location.locationID>>>>>>>>,
								Where<PaymentMethodDetail.paymentMethodID, Equal<Current<CABatch.paymentMethodID>>,
									And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>>>>
									.Select(this, vendorID, locationID, vendorID, locationID))
			{
				var detail = (PaymentMethodDetail)result;
				var vpmDetail = (VendorPaymentMethodDetail)result;

				vendorDetails.Add(detail.DetailID.Trim(),
					new PaymentInstruction { DetailID = detail.DetailID, Description = detail.Descr, Value = vpmDetail.DetailValue, Required = detail.IsRequired });
			}

			return vendorDetails;
		}

		public string GetCashAccountCode() => cashAccount.SelectSingle().CashAccountCD;
		public string GetVendorName(int? vendorID) => Vendor.PK.Find(this, vendorID).AcctCD;

		public bool SaveFile(byte[] data, string fileName)
		{
			return SaveFile(Document.Cache, Document.Current, fileName, data, true);
		}

		private static bool SaveFile(PXCache cache, object cacheRow, string fileName, byte[] data, bool createRevision)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				throw new PXArgumentException(nameof(fileName));
			}

			if (data == null)
			{
				throw new PXArgumentException(nameof(data));
			}

			if (data.Length == 0)
			{
				return false;
			}

			try
			{
				var filegraph = PXGraph.CreateInstance<UploadFileMaintenance>();
				var fileinfo = new PX.SM.FileInfo(fileName, null, data);
				filegraph.SaveFile(fileinfo, createRevision ? FileExistsAction.CreateVersion : FileExistsAction.ThrowException);

				if (fileinfo.UID.HasValue)
				{
					PXNoteAttribute.SetFileNotes(cache, cacheRow, fileinfo.UID.Value);
					return true;
				}
			}
			catch (Exception e)
			{
				PXTrace.WriteWarning($"Unable to save file '{fileName}'. {e.Message}");
			}

			return false;
		}

		public virtual IEnumerable<IACHPlugInParameter> GetPlugInParameters()
		{
			var pm = paymentMethod.SelectSingle();

			foreach(ACHPlugInParameter plugInParameter in PlugInParameters.Select(pm.PaymentMethodID, pm.APBatchExportPlugInTypeName))
			{
				yield return plugInParameter;
			}
		}

		public IACHPlugInParameter GetPlugInParameter(string parameterName)
		{
			var pm = paymentMethod.SelectSingle();
			return PlugInParameterByName.SelectSingle(pm.PaymentMethodID, pm.APBatchExportPlugInTypeName, parameterName);
		}
		#endregion

		#region Processing Grpah Definition
		[PXHidden]
		public class CABatchUpdate : PXGraph<CABatchUpdate>
		{
			public PXSelect<CABatch, Where<CABatch.batchNbr, Equal<Required<CABatch.batchNbr>>>> Document;
			public PXSelectJoin<APPayment,
							InnerJoin<CABatchDetail, On<CABatchDetail.origDocType, Equal<APPayment.docType>,
							And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>,
							And<CABatchDetail.origModule, Equal<GL.BatchModule.moduleAP>>>>>,
							Where<CABatchDetail.batchNbr, Equal<Optional<CABatch.batchNbr>>>> APPaymentList;
			public PXSetup<CASetup> casetup;

			public bool AutoPost
			{
				get
				{
					return (bool)casetup.Current.AutoPostOption;
				}
			}

			public virtual CABatch ReleaseCABatch(CABatch document)
			{
				document.Released = true;
				document.Exported = true;
				if (document.SkipExport == true)
				{
					document.DateSeqNbr = GetNextDateSeqNbr(this, document);
				}
				RecalcTotals();
				document = Document.Update(document);
				Actions.PressSave();
				return document;
			}

			public virtual CABatch VoidCABatch(CABatch document)
			{
				document.Voided = true;
				document = Document.Update(document);
				Actions.PressSave();
				return document;
			}

			public virtual void RecalcTotals()
			{
				CABatch row = this.Document.Current;
				if (row != null)
				{
					row.DetailTotal = row.CuryDetailTotal = row.Total = decimal.Zero;

					foreach (PXResult<APPayment, CABatchDetail> item in this.APPaymentList.Select())
					{
						APPayment payment = item;
						if (!string.IsNullOrEmpty(payment.RefNbr))
						{
							row.CuryDetailTotal += payment.CuryOrigDocAmt;
							row.DetailTotal += payment.OrigDocAmt;
						}
					}
				}
			}

			public virtual void VoidAPPayments(CABatch document)
			{
				List<APRegister> unreleasedList = new List<APRegister>();

				var paymentGraph = PXGraph.CreateInstance<APPaymentEntry>();

				PXSelectBase<APPayment> selectReleased = new PXSelectReadonly2<APPayment,
								InnerJoin<CABatchDetail,
									On<CABatchDetail.origModule, Equal<GL.BatchModule.moduleAP>,
									And<CABatchDetail.origDocType, Equal<APPayment.docType>,
									And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>>>>>,
								Where<CABatchDetail.batchNbr, Equal<Required<CABatch.batchNbr>>,
									And<APPayment.released, Equal<boolTrue>>>>(this);

				foreach (var item in selectReleased.Select(document.BatchNbr))
				{
					APPayment payment = item;

					if (payment.Released == true)
					{
						var voidItem = VoidDetail(paymentGraph, payment, document.VoidDate);

						if (voidItem != null)
						{
							var voidRegister = PXSelectReadonly<APRegister,
								Where<APRegister.docType, Equal<Required<APRegister.docType>>,
									And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.Select(this, voidItem.DocType, voidItem.RefNbr);

							unreleasedList.Add(voidRegister);
						}
					}
				}

				if (unreleasedList.Count > 0)
				{
					ReleaseVoidPayments(unreleasedList);
				}

				selectReleased.View.Clear();
			}

			private void ReleaseVoidPayments(List<APRegister> unreleasedList)
			{
				var externalPostList = new List<Batch>();

				APDocumentRelease.ReleaseDoc(unreleasedList, true, externalPostList);

				if (externalPostList.Count > 0 && AutoPost)
				{
					List<Batch> postFailedList = new List<Batch>();
					PostGraph pg = PXGraph.CreateInstance<PostGraph>();

					foreach (Batch iBatch in externalPostList)
					{
						try
						{
							pg.Clear();
							pg.PostBatchProc(iBatch);
						}
						catch (Exception)
						{
							postFailedList.Add(iBatch);
						}
					}

					if (postFailedList.Count > 0)
					{
						throw new PXException(GL.Messages.PostingOfSomeOfTheIncludedDocumentsFailed, postFailedList.Count, postFailedList.Count);
					}
				}
			}

			protected virtual APRegister VoidDetail(APPaymentEntry graph, APPayment payment, DateTime? voidDate)
			{
				graph.Clear();
				graph.Document.Current = payment;
				graph.currencyinfo.Select();

				if (graph.Document.Current != null &&
					graph.Document.Current.Released == true &&
					graph.Document.Current.Voided == false &&
					APPaymentType.VoidEnabled(graph.Document.Current.DocType))
				{

					APAdjust application = PXSelect<APAdjust,
						Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
							And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>,
							And<Where<APAdjust.adjgDocType, In3<APDocType.check, APDocType.prepayment>,
								Or<APAdjust.adjgDocType, Equal<APDocType.refund>,
							And<APAdjust.voided, NotEqual<True>>>>>>>>
						.Select(graph, graph.Document.Current.DocType, graph.Document.Current.RefNbr);

					if (application != null && application.IsSelfAdjustment() != true && (application.AdjdDocType == APDocType.Check || application.AdjdDocType == APDocType.Prepayment))
					{
						throw new PXException(AP.Messages.PaymentIsPayedByCheck, application.AdjgRefNbr);
					}

					if (application != null && application.IsSelfAdjustment() != true && application.AdjdDocType == APDocType.Refund && application.Voided != null)
					{
						throw new PXException(
							Common.Messages.DocumentHasBeenRefunded,
							GetLabel.For<APDocType>(graph.Document.Current.DocType),
							graph.Document.Current.RefNbr,
							GetLabel.For<APDocType>(application.AdjgDocType),
							application.AdjgRefNbr);
					}

					APPayment voidcheck = graph.Document.Search<APPayment.refNbr>(graph.Document.Current.RefNbr, APPaymentType.GetVoidingAPDocType(graph.Document.Current.DocType));

					if (voidcheck != null)
					{
						return voidcheck;
					}

					foreach (APAdjust adj in graph.Adjustments_Raw.Select())
					{
						graph.Adjustments.Cache.Delete(adj);
					}
					graph.Save.Press();

					APPayment doc = PXCache<APPayment>.CreateCopy(graph.Document.Current);

					graph.FinPeriodUtils.VerifyAndSetFirstOpenedFinPeriod<APPayment.finPeriodID, APPayment.branchID>(
						graph.Document.Cache,
						doc,
						graph.finperiod,
						typeof(GL.FinPeriods.OrganizationFinPeriod.aPClosed));

					graph.TryToVoidCheck(doc);

					if (voidDate.HasValue)
					{
						var newDoc = graph.Document.Current;
						newDoc.AdjDate = voidDate;
						newDoc = graph.Document.Update(newDoc);
					}

					graph.Document.Cache.RaiseExceptionHandling<APPayment.finPeriodID>(graph.Document.Current, graph.Document.Current.FinPeriodID, null);

					if (graph.Document.Current.Hold == true)
					{
						graph.releaseFromHold.Press();
					}
					graph.Save.Press();

					return graph.Document.Current;
				}

				return null;
			}
		}
		#endregion
	}

	[PXHidden]
	[PXCacheName(nameof(AddPaymentsFilter))]
	public partial class AddPaymentsFilter : IBqlTable
	{
		#region NextPaymentRefNumber
		public abstract class nextPaymentRefNumber : PX.Data.BQL.BqlString.Field<nextPaymentRefNumber> { }
		[PXDBString(15, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Next Payment Ref. Number", Visible = true)]
		public virtual string NextPaymentRefNumber
		{
			get;
			set;
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate>
		{
		}

		[PXDBDate]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate
		{
			get;
			set;
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate>
		{
		}

		[PXDBDate]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndDate
		{
			get;
			set;
		}
		#endregion
		#region SelectedCount
		public abstract class selectedCount : PX.Data.BQL.BqlInt.Field<selectedCount> { }
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Number of Payments", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual int? SelectedCount { get; set; }
		#endregion
	}

	[PXHidden]
	[PXCacheName(nameof(VoidFilter))]
	public partial class VoidFilter : IBqlTable
	{
		#region SetVoidDateOption
		public abstract class voidDateOption : PX.Data.BQL.BqlString.Field<voidDateOption>
		{
			public const string OriginalPaymentDates = "O";
			public const string SpecificVoidDate = "S";

			public class originalPaymentDates : PX.Data.BQL.BqlString.Constant<originalPaymentDates>
			{
				public originalPaymentDates() : base(OriginalPaymentDates) { }
			}

			public class specificVoidDate : PX.Data.BQL.BqlString.Constant<specificVoidDate>
			{
				public specificVoidDate() : base(SpecificVoidDate) { }
			}

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(new string[] { OriginalPaymentDates, SpecificVoidDate },
							new string[] { Messages.InitialPaymentDates, Messages.SetVoidDate })
				{
				}
			}
		}
		[PXString]
		[voidDateOption.List]
		[PXDefault(voidDateOption.OriginalPaymentDates, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string VoidDateOption
		{
			get;
			set;
		}
		#endregion
		#region VoidDate
		public abstract class voidDate : PX.Data.BQL.BqlDateTime.Field<voidDate>
		{
		}

		[PXDBDate]
		[PXUIField(DisplayName = "Void Date", Visible = false, Enabled = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? VoidDate
		{
			get;
			set;
		}
		#endregion
	}
}
