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
using PX.Api;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CA;
using PX.Objects.Localizations.CA.DataSync;
using PX.Objects.CR.Extensions;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.BackwardCompatibility;

namespace PX.Objects.Localizations.CA.CA
{
	#region Extensions

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CABatchEntry_ActivityDetailsExt : ActivityDetailsExt<CABatchEntry, CABatch, CABatch.noteID>
	{
		#region IsActive

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>();
		}

		#endregion

		public override Type GetBAccountIDCommand() => typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<APPayment.vendorID>>>>);
	}

	#endregion

	public class CABatchEntryExt : PXGraphExtension<CABatchEntry>
    {
        #region IsActive

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>();
        }

        #endregion

        #region Constants and definitions

        private const string PaymentNoticeMailingId = "PAYMENTNOTICE";
        private const string ResultOK = "OK";

		#endregion

		#region DataViews

		public PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>> Vendor;

		public PXSelectJoin<Vendor, InnerJoin<CREmployee, On<Vendor.bAccountID, Equal<CREmployee.bAccountID>>>, Where<Vendor.bAccountID, Equal<Required<APPayment.vendorID>>>> VendorAsEmployee;

		public SelectFrom<PaymentMethodAccount>
			.Where<PaymentMethodAccount.paymentMethodID.IsEqual<CABatch.paymentMethodID.FromCurrent>
				.And<PaymentMethodAccount.cashAccountID.IsEqual<CABatch.cashAccountID.FromCurrent>>>.View PaymentMethodAccount;

        #endregion

        #region Events
        protected virtual void CABatch_RowSelected(PXCache sender, PXRowSelectedEventArgs args)
        {
            bool enable = false;

            CABatch caBatchRecord = (CABatch)args.Row;
            if (caBatchRecord != null)
            {
                if (caBatchRecord.Released == true &&
                    !string.IsNullOrWhiteSpace(caBatchRecord.ExportFileName) &&
                    caBatchRecord.ExportTime.HasValue)
                {
                    enable = true;
                }
            }

			SendNotificationsByEmail.SetEnabled(enable);

			bool cpaTestButtonEnabled = false;

			PaymentMethod pt = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(Base, caBatchRecord.PaymentMethodID);

			if (pt != null)
			{
				if (pt.DirectDepositFileFormat == EFTDirectDepositType.Code)
				{
					cpaTestButtonEnabled = true;
				}
			}
			generateTestFile.SetVisible(cpaTestButtonEnabled);
			generateTestFile.SetEnabled(cpaTestButtonEnabled);
		}

		protected virtual void CABatch_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            CABatch doc = (CABatch)e.Row;

            if (doc == null || doc.Released == true)
            {
                return;
            }

            PXEntryStatus docStatus = sender.GetStatus(doc);

            if (docStatus != PXEntryStatus.Inserted && docStatus != PXEntryStatus.Updated)
            {
                return;
            }

            PXResult<PaymentMethod, SYMapping, SYProvider> res = GetPaymentMethodDetails(doc);
			PaymentMethod pm = res;

			if (!IsProviderACP005(pm.DirectDepositFileFormat))
            {
                return;
            }

            short batchSeqNbr = 0;

            if (docStatus == PXEntryStatus.Inserted)
            {
                // Retrieve a previously generated batch number
                CABatch previousBatch = PXSelect<CABatch,
                        Where<CABatch.cashAccountID, Equal<Required<CABatch.cashAccountID>>,
                            And<CABatch.paymentMethodID, Equal<Required<CABatch.paymentMethodID>>>>,
                        OrderBy<
                            Desc<CABatch.createdDateTime,
                                Desc<CABatch.batchSeqNbr>>>>.Select(sender.Graph,
                                                                    doc.CashAccountID,
                                                                    doc.PaymentMethodID)
                                                            .RowCast<CABatch>()
                                                            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.BatchSeqNbr));

                if (previousBatch == null)
                {
                    return;
                }

                if (short.TryParse(previousBatch.BatchSeqNbr, out batchSeqNbr))
                {
					PaymentMethodAccount paymentMethodAccount = PaymentMethodAccount.SelectSingle();

					if (string.IsNullOrEmpty(paymentMethodAccount.APBatchLastRefNbr))
					{
						if (batchSeqNbr < 1 || batchSeqNbr > 9999)
						{
							throw new PXException(Messages.CPA005.FileCreationNumberInvalid, batchSeqNbr);
						}

						batchSeqNbr++;

						if (batchSeqNbr == 10000)
						{
							// Max is 9999, restart
							batchSeqNbr = 1;
						}
					}
                }
            }
            else if (docStatus == PXEntryStatus.Updated)
            {
                short.TryParse(doc.BatchSeqNbr, out batchSeqNbr);
            }

            if (batchSeqNbr != 0)
            {
				sender.SetValueExt<CABatch.batchSeqNbr>(doc, batchSeqNbr);
            }
        }

        protected void CABatchDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs args, PXRowSelected del)
        {
            if (del != null)
            {
                del(sender, args);
            }

            IDictionary<string, string> resultStatus = PXLongOperation.GetCustomInfo(Base.UID, PaymentNoticeMailingId) as IDictionary<string, string>;

            if (resultStatus == null)
            {
                return;
            }

            CABatchDetail detailRecord = args.Row as CABatchDetail;

            if (detailRecord == null)
            {
                return;
            }

            TimeSpan timeSpan;
            Exception exception;

            PXLongRunStatus longRunStatus = PXLongOperation.GetStatus(Base.UID, out timeSpan, out exception);

            if ((longRunStatus == PXLongRunStatus.Aborted) || (longRunStatus == PXLongRunStatus.Completed))
            {
                string resultMessage;

                if (resultStatus.TryGetValue(detailRecord.OrigRefNbr, out resultMessage))
                {
                    sender.RaiseExceptionHandling<CABatchDetail.origRefNbr>(detailRecord, detailRecord.OrigRefNbr, GetException(resultMessage));
                }
            }
        }
		#endregion

		#region Actions
		public PXAction<PX.Objects.CA.CABatch> SendNotificationsByEmail;
        [PXProcessButton]
        [PXUIField(DisplayName = "Send Notifications by Email")]
        protected virtual IEnumerable sendNotificationsByEmail(PXAdapter adapter)
        {
			var batchEntry = Base;
			var document = Base.Document.Current;
            PXLongOperation.StartOperation(batchEntry, delegate ()
            {
                IDictionary<string, string> resultStatus = new Dictionary<string, string>();

                PXLongOperation.SetCustomInfo(resultStatus, PaymentNoticeMailingId);

                // Important: The SetCustomInfo must be initialized with a string (any string) so that it will still exist in the CABatchDetail_RowSelected event routine.
                //            If not done and the Activity.SendNotification is not called, then the resultStatus object will not be avaiable.
                PXLongOperation.SetCustomInfo("StartEmailOperation");

                CABatchEntry mainGraph = PXGraph.CreateInstance<CABatchEntry>();
                CABatchEntryExt extGraph = mainGraph.GetExtension<CABatchEntryExt>();
				mainGraph.Caches[typeof(CABatch)].Current = document;

                extGraph.SendEmails((PXResultset<APPayment>)batchEntry.APPaymentList.Select(), resultStatus);
            });

            Base.BatchPayments.View.RequestRefresh();

            return adapter.Get();
        }

		public PXAction<CABatch> generateTestFile;
		[PXProcessButton]
		[PXUIField(DisplayName = "Generate Test File")]
		protected virtual IEnumerable GenerateTestFile(PXAdapter adapter)
		{
			var batch = Base.Document.Current;

			RunExport(batch, true);
			return adapter.Get();
		}

		#endregion

		#region Method called in a seperate thread
		private void SendEmails(PXResultset<APPayment> paymentList, IDictionary<string, string> resultStatus)
        {
            NotificationUtility utility = new NotificationUtility(this.Base);

            Guid? nsID = utility.SearchSetupID(APNotificationSource.Vendor, PaymentNoticeMailingId);

            if (nsID == null)
            {
                throw new PXException(Messages.CA.MailingIdNotFound, PaymentNoticeMailingId);
            }

            Dictionary<string, string> reportParms = new Dictionary<string, string>();
            int emailSent = 0;
            int emailFailed = 0;

            foreach (PXResult<APPayment, CABatchDetail> resultItem in paymentList)
            {
                APPayment paymentItem = resultItem;

				if (this.VendorAsEmployee.Select(paymentItem.VendorID).Count > 0)
					continue;

                if (!IsVendorMailingOk(nsID.Value, paymentItem.VendorID, paymentItem.BranchID))
                {
                    emailFailed++;

                    resultStatus.Add(paymentItem.RefNbr, PXLocalizer.LocalizeFormat(Messages.CA.CheckMailingOnVendor, (GetVendorCode(paymentItem.VendorID)), PaymentNoticeMailingId));

                    continue;
                }

                // Note: Using the StartCheckNbr parameter (populated with paymentItem.ExtRefNbr) did not work because of the
                // possibility of duplicate check number.
                reportParms.Clear();
                reportParms["RefNbr"] = paymentItem.RefNbr;

                Base.APPaymentList.Cache.Current = paymentItem;

                try
                {
                    Base.GetExtension<CABatchEntry_ActivityDetailsExt>().SendNotification(APNotificationSource.Vendor, PaymentNoticeMailingId, paymentItem.BranchID, reportParms);

                    emailSent++;

                    resultStatus.Add(paymentItem.RefNbr, ResultOK);

                }
                catch (Exception inException)
                {
                    emailFailed++;

                    resultStatus.Add(paymentItem.RefNbr, PXLocalizer.LocalizeFormat(Messages.CA.EmailFailed, inException.Message));
                }
            }

            if (emailFailed > 0)
            {
                throw new PXException((Messages.CA.EmailStatus), emailSent, emailFailed);
            }

        }
        #endregion

        #region Private methods

        private bool IsVendorMailingOk(Guid nsId, int? vendorId, int? branchId)
        {
            Vendor vendorRecord = Vendor.Select(vendorId);
            if (vendorRecord == null)
            {
                return false;
            }

            NotificationUtility utility = new NotificationUtility(this.Base);

            NotificationSource ns = utility.GetSource(APNotificationSource.Vendor, vendorRecord, nsId, branchId);

            if (ns == null)
            {
                return false;
            }

            return true;
        }

        private string GetVendorCode(int? vendorId)
        {
            try
            {
                Vendor vendorRecord = Vendor.Select(vendorId);

                return vendorRecord.AcctCD;
            }
            catch (Exception)
            {
                return null;
            }

        }

        private PXSetPropertyException GetException(string resultMessage)
        {
            PXSetPropertyException exception = null;

            if (resultMessage == ResultOK)
            {
                exception = new PXSetPropertyException(PXLocalizer.Localize(Messages.CA.EmailSent), PXErrorLevel.RowInfo);
            }
            else
            {
                exception = new PXSetPropertyException(resultMessage, PXErrorLevel.RowWarning);
            }

            return (exception);
        }
        #endregion

        #region Export Action Override

        [PXOverride]
        public IEnumerable Export(PXAdapter adapter, Func<PXAdapter, IEnumerable> del)
        {
			var batch = Base.Document.Current;

			if (RunExport(batch, false))
				return adapter.Get();

			if (del != null)
			{
				return (del(adapter));
			}
			else
			{
				return (adapter.Get());
			}
		}

		protected bool RunExport(CABatch doc, bool isTest)
		{
			bool needUpdate = false;

			Base.Save.Press();

			if (CanRunExport(doc))
			{
				if (doc?.SkipExport == false)
				{
					if (doc != null && doc.Released != true && doc.Hold != true && doc.Exported != true)
					{
						doc.Exported = true;
						doc.DateSeqNbr = CABatchEntry.GetNextDateSeqNbr(Base, doc);
						needUpdate = true;
					}
				}

				PXSYParameter[] paramList = setExportParameters(doc, isTest);
				PXResult<PaymentMethod, SYMapping, SYProvider> res = GetPaymentMethodDetails(doc);
				SYMapping map = res;

				Base.LongOperationManager.StartOperation(cancellationToken =>
				{
					PX.Api.SYExportProcess.RunScenario(map.Name, SYMapping.RepeatingOption.All,
						true,
						true,
						cancellationToken,
						paramList);

					if (needUpdate)
					{
						Base.Document.Update(doc);
						Base.Save.Press();
					}
				});

				return true;
			}
			else
				return false;
		}

		protected bool CanRunExport(CABatch doc)
		{
			bool result = false;

			if (PXLongOperation.GetStatus(Base.UID) == PXLongRunStatus.InProcess)
			{
				throw new ApplicationException(PX.Objects.GL.Messages.PrevOperationNotCompleteYet);
			}

			if (doc != null && doc.Hold == false)
			{
				PXResult<PaymentMethod, SYMapping> res = GetPaymentMethodDetails(doc);

				PaymentMethod pm = res;
				SYMapping map = res;

				if (pm != null &&
					pm.APCreateBatchPayment == true &&
					pm.APBatchExportSYMappingID != null &&
					map != null)
				{
					if (IsProviderACP005(pm.DirectDepositFileFormat))
					{
						result = true;
					}
				}
			}

			return result;
		}
		protected PXSYParameter[] setExportParameters(CABatch doc, bool isTest)
		{
			string bank = GetAccountSettingValue(doc, "3");
			string zipFormat = GetAccountSettingValue(doc, "8") ?? "0";
			string fileName = BuildFileName(Base, doc, bank, zipFormat, isTest);

			return new PXSYParameter[] {
								new PX.Api.PXSYParameter(CPA005ExportProvider.Params.FileName, fileName),
								new PX.Api.PXSYParameter(CPA005ExportProvider.Params.BatchNbr, doc.BatchNbr),
								new PX.Api.PXSYParameter(CPA005ExportProvider.Params.ZipFormat, zipFormat),
								new PX.Api.PXSYParameter(CPA005ExportProvider.Params.IncomingBankCode, bank),
								new PX.Api.PXSYParameter(CPA005ExportProvider.Params.IsTest, isTest.ToString())
						};
		}

		#endregion

		#region Release Action Override

		[PXOverride]
        public IEnumerable Release(PXAdapter adapter, Func<PXAdapter, IEnumerable> del)
        {
            if (PXLongOperation.GetStatus(Base.UID) == PXLongRunStatus.InProcess)
            {
                throw new ApplicationException(PX.Objects.GL.Messages.PrevOperationNotCompleteYet);
            }

            CABatch doc = Base.Document.Current;

            PXResult<PaymentMethod, SYMapping> res = GetPaymentMethodDetails(doc);

            PaymentMethod pm = res;
            SYMapping map = res;

			if (pm != null && pm.APCreateBatchPayment == true && pm.APBatchExportSYMappingID != null && map != null)
            {
                if (IsProviderACP005(pm.DirectDepositFileFormat))
                {
                    CashAccount account = Base.cashAccount.Select(doc.CashAccountID);

                    string currency = account.CuryID;

                    if (currency != "CAD" && currency != "USD")
                    {
                        throw new PXException(Messages.CPA005.CurrencyCodeInvalid, currency);
                    }

					short batchSeqNbr = 0;

                    short.TryParse(doc.BatchSeqNbr, out batchSeqNbr);

                    if (batchSeqNbr < 1 || batchSeqNbr > 9999)
                    {

                        Base.Document.Cache.RaiseExceptionHandling<CABatch.batchSeqNbr>(
                            doc,
                            doc.BatchSeqNbr,
                            new PXSetPropertyException(Messages.CPA005.BatchSeqNumberAcceptedValues, PXErrorLevel.Error));

                        throw new PXException(Messages.CPA005.BatchCannotBeReleased);
                    }
                }
            }

            if (del != null)
            {
                return (del(adapter));
            }
            else
            {
                return (adapter.Get());
            }
        }
        #endregion

        #region Private methods

        private PXResult<PaymentMethod, SYMapping, SYProvider> GetPaymentMethodDetails(CABatch doc)
        {
            PXResult<PaymentMethod, SYMapping, SYProvider> res =
                (PXResult<PaymentMethod, SYMapping, SYProvider>)
                PXSelectJoin<PaymentMethod,
                        LeftJoin<SYMapping,
                            On<PaymentMethod.aPBatchExportSYMappingID,
                                Equal<SYMapping.mappingID>>,
                            LeftJoin<SYProvider,
                                On<SYMapping.providerID,
                                    Equal<SYProvider.providerID>>>>,
                        Where<PaymentMethod.paymentMethodID,
                            Equal<Optional<CABatch.paymentMethodID>>>>
                    .Select(Base, doc.PaymentMethodID);

            return res;
        }

        private string GetAccountSettingValue(CABatch doc, string detailID)
        {
            string res = null;

            CashAccountPaymentMethodDetail cashAccountSettings =
                PXSelect<CashAccountPaymentMethodDetail,
                        Where<CashAccountPaymentMethodDetail.cashAccountID,
                            Equal<Required<CashAccount.accountID>>,
                            And<CashAccountPaymentMethodDetail.paymentMethodID,
                                Equal<Required<CABatch.paymentMethodID>>,
                                And<CashAccountPaymentMethodDetail.detailID,
                                    Equal<Required<CashAccountPaymentMethodDetail.detailID>>>>>>
                    .Select(Base,
                            doc.CashAccountID,
                            doc.PaymentMethodID,
                            detailID);

            if (cashAccountSettings != null)
            {
                res = cashAccountSettings.DetailValue;
            }

            return res;
        }

        private string BuildFileName(CABatchEntry graph, CABatch doc, string bank, string zipFormat, bool isTest)
        {
            string fileName = "CPA_ACP_005.txt"; // Should never appear, a released batch is always linked to a cash account

			if (bank == "006") // Specific filename for the National Bank of Canada
            {
                fileName = "TF" + doc.BatchSeqNbr.Trim().PadLeft(10, '0') + ".txt";
            }
            else
            {
                CashAccount cashAccount = graph.cashAccount.Select(doc.CashAccountID);

                if (cashAccount != null)
                {
                    short batchSeqNbr;

                    if (short.TryParse(doc.BatchSeqNbr, out batchSeqNbr))
                    {
						fileName = string.Format("{0}-{1}-{2:yyyyMMdd}-{3:0000}", doc.PaymentMethodID, cashAccount.CashAccountCD.Trim(), doc.TranDate.Value, batchSeqNbr);

						if (!isTest)
							fileName += ".txt";
						else
							fileName += "-Test.txt";
					}
                }
            }

            if (zipFormat == "1")
            {
                // File name is validated by a RegEx and cannot start nor end with a dot (unless the RegEx gets overridden)
                int dotIndex = fileName.LastIndexOf('.');

                if (dotIndex > 0) // we need at least one character before the dot
                {
                    fileName = fileName.Substring(0, dotIndex);
                }

                fileName += ".zip";
            }

            return fileName;
        }

        private bool IsProviderACP005(string directDepositFileFormat)
        {
			if (directDepositFileFormat == EFTDirectDepositType.Code)
				return true;
			else
				return false;
        }

        #endregion
    }
}
