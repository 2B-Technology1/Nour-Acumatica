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

using PX.CCProcessingBase.Interfaces.V2;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Api.Services;
using PX.Objects.CC;
using PX.Objects.CC.GraphExtensions;
using PX.Objects.Extensions.PaymentTransaction;
using PX.Objects.GL;
using PX.Objects.SO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Extensions.PayLink
{
	public abstract class PayLinkDocumentGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
	where TGraph : PXGraph, new()
	where TPrimary : class, IBqlTable, new()
	{
		public PXSelectExtension<PayLinkDocument> PayLinkDocument;
		private ICCPaymentProcessingRepository _paymentProcRepo;
		const string processKey = "PayLinkOperation";

		[InjectDependency]
		public ICompanyService CompanyService { get; set; }

		public PXAction<TPrimary> createLink;
		[PXUIField(DisplayName = "Create Payment Link", Visible = true)]
		[PXButton(CommitChanges = true)]
		public virtual IEnumerable CreateLink(PXAdapter adapter)
		{
			SaveDoc();
			var docs = adapter.Get<TPrimary>().ToList();
			var docKeys = GetKeysAsString(docs);

			CheckAnotherProcessRunning(docKeys);

			PXLongOperation.StartOperation(Base, delegate
			{
				SetProcessRunning(docKeys);
				foreach (var doc in docs)
				{
					var graph = PXGraph.CreateInstance<TGraph>();
					var ext = graph.FindImplementation<PayLinkDocumentGraph<TGraph, TPrimary>>();
					ext.SetCurrentDocument(doc);
					ext.CollectDataAndCreateLink();
				}
			});
			return docs;
		}

		public PXAction<TPrimary> syncLink;
		[PXUIField(DisplayName = "Sync Payment Link", Visible = true)]
		[PXButton(CommitChanges = true)]
		public virtual IEnumerable SyncLink(PXAdapter adapter)
		{
			SaveDoc();
			var docs = adapter.Get<TPrimary>().ToList();
			PXLongOperation.StartOperation(Base, delegate
			{
				foreach (var doc in docs)
				{
					TGraph graph = PXGraph.CreateInstance<TGraph>();
					var ext = graph.FindImplementation<PayLinkDocumentGraph<TGraph, TPrimary>>();
					ext.SetCurrentDocument(doc);
					ext.CollectDataAndSyncLink();
				}
			});
			return docs;
		}

		public PXAction<TPrimary> resendLink;
		[PXUIField(DisplayName = "Resend Payment Link", Visible = true)]
		[PXButton(CommitChanges = true)]
		public virtual IEnumerable ResendLink(PXAdapter adapter)
		{
			SaveDoc();
			var docs = adapter.Get<TPrimary>().ToList();
			PXLongOperation.StartOperation(Base, delegate
			{
				foreach (var doc in docs)
				{
					var graph = PXGraph.CreateInstance<TGraph>();
					var ext = graph.FindImplementation<PayLinkDocumentGraph<TGraph, TPrimary>>();
					ext.SetCurrentDocument(doc);
					ext.SendNotification();
				}
			});
			return docs;
		}

		public virtual void CreatePayments(SOOrder order, CCPayLink payLink, PayLinkData payLinkData)
		{
			if (payLinkData.Transactions == null) return;

			PXException ex = null;
			var orderExt = Base.Caches[typeof(SOOrder)].GetExtension<SOOrderPayLink>(order);
			var paymentGraph = PXGraph.CreateInstance<ARPaymentEntry>();

			var pc = GetPaymentProcessingRepo().GetProcessingCenterByID(orderExt.ProcessingCenterID);

			foreach (var tranData in payLinkData.Transactions.OrderBy(i => i.SubmitTime))
			{
				paymentGraph.AutoPaymentApp = true;
				using (var scope = new PXTransactionScope())
				{
					if (!(tranData.TranType == CCTranType.AuthorizeAndCapture
						&& tranData.TranStatus == CCTranStatus.Approved))
					{
						continue;
					}
					try
					{
						TranValidationHelper.CheckTranAlreadyRecorded(tranData,
							new TranValidationHelper.AdditionalParams()
							{
								ProcessingCenter = pc.ProcessingCenterID,
								Repo = new CCPaymentProcessingRepository(Base)
							});
					}
					catch (TranValidationHelper.TranValidationException)
					{
						continue;
					}

					var mappingRow = GetMappingRow(order.BranchID, orderExt.ProcessingCenterID);

					try
					{
						CheckTranAgainstMapping(mappingRow, tranData);
					}
					catch (PXException checkMappingEx)
					{
						ex = checkMappingEx;
						continue;
					}

					string paymentMethodId = null;
					int? cashAccId = null;

					if (tranData.PaymentMethodType == MeansOfPayment.CreditCard)
					{
						paymentMethodId = mappingRow.CCPaymentMethodID;
						cashAccId = mappingRow.CCCashAccountID;
					}
					else
					{
						paymentMethodId = mappingRow.EFTPaymentMethodID;
						cashAccId = mappingRow.EFTCashAccountID;
					}

					var pmtDate = PXTimeZoneInfo.ConvertTimeFromUtc(tranData.SubmitTime, LocaleInfo.GetTimeZone()).Date;

					ARPayment payment = new ARPayment();
					payment.AdjDate = pmtDate;
					payment.BranchID = order.BranchID;
					payment.DocType = ARDocType.Payment;
					payment = paymentGraph.Document.Insert(payment);
					payment.CustomerID = order.CustomerID;
					payment.CustomerLocationID = order.CustomerLocationID;
					payment.PaymentMethodID = paymentMethodId;
					payment.CuryOrigDocAmt = tranData.Amount;
					payment.DocDesc = order.DocDesc;
					payment = paymentGraph.Document.Update(payment);
					payment.PMInstanceID = PaymentTranExtConstants.NewPaymentProfile;
					payment.ProcessingCenterID = pc.ProcessingCenterID;
					payment.CashAccountID = cashAccId;
					payment.Hold = false;
					payment.DocDesc = PXMessages.LocalizeFormatNoPrefix(CC.Messages.PayLinkPaymentDescr, payLink.OrderType, payLink.OrderNbr, payLink.ExternalID);
					payment = paymentGraph.Document.Update(payment);
					paymentGraph.Save.Press();

					ApplyPaymentWithoutNeedSync(paymentGraph, order);

					var extension = paymentGraph.GetExtension<AR.GraphExtensions.ARPaymentEntryPaymentTransaction>();
					CCPaymentEntry entry = new CCPaymentEntry(paymentGraph);
					extension.RecordTransaction(paymentGraph.Document.Current, tranData, entry);

					paymentGraph.Clear();
					scope.Complete();
				}
			}

			if (ex != null)
			{
				throw ex;
			}
		}

		protected virtual void ApplyPaymentToDocuments(ARPaymentEntry paymentGraph, SOOrder order)
		{
			SOAdjust orderAdjust = null;
			var orderLinked = false;
			var appliedOrdersTab = paymentGraph.GetOrdersToApplyTabExtension(true);
			if (order.CuryOrderTotal - order.CuryPaidAmt > 0
						&& order.Cancelled == false && order.Completed == false)
			{
				var newAdjust = new SOAdjust()
				{
					AdjdOrderType = order.OrderType,
					AdjdOrderNbr = order.OrderNbr,
				};

				orderAdjust = appliedOrdersTab.SOAdjustments.Insert(newAdjust);
				paymentGraph.Save.Press();
				orderLinked = true;
			}

			var invoiceLinked = false;
			foreach (ARInvoice invoice in GetInvoicesRelatedToOrder(order))
			{
				var payment = paymentGraph.Document.Current;
		
				var amt = payment.CuryUnappliedBal + payment.CurySOApplAmt > invoice.CuryUnpaidBalance
					? invoice.CuryUnpaidBalance : payment.CuryUnappliedBal + payment.CurySOApplAmt;

				if (amt != 0)
				{
					var adjust = new ARAdjust
					{
						AdjdDocType = invoice.DocType,
						AdjdRefNbr = invoice.RefNbr,
					};

					if (orderLinked)
					{
						adjust.AdjdOrderNbr = order.OrderNbr;
						adjust.AdjdOrderType = order.OrderType;
					}

					adjust = paymentGraph.Adjustments.Insert(adjust);
					adjust.CuryAdjdAmt = amt;
					adjust.CuryAdjgAmt = amt;
					adjust = paymentGraph.Adjustments.Update(adjust);
					invoiceLinked = true;
				}
			}

			if (invoiceLinked)
			{
				if (orderLinked)
				{
					var payment = paymentGraph.Document.Current;
					var amt = payment.CuryUnappliedBal + payment.CurySOApplAmt > order.CuryUnpaidBalance
						? order.CuryUnpaidBalance : payment.CuryUnappliedBal + payment.CurySOApplAmt;
					orderAdjust = appliedOrdersTab.SOAdjustments.Locate(orderAdjust);
					orderAdjust.CuryAdjgAmt = amt;
					orderAdjust.CuryAdjdAmt = amt;
					appliedOrdersTab.SOAdjustments.Update(orderAdjust);
				}
				paymentGraph.Save.Press();
			}
		}

		protected virtual CC.PaymentProcessing.PayLinkProcessing GetPayLinkProcessing()
		{
			var paymentRepo = GetPaymentProcessingRepo();
			return new CC.PaymentProcessing.PayLinkProcessing(paymentRepo);
		}

		protected virtual ICCPaymentProcessingRepository GetPaymentProcessingRepo()
		{
			if (_paymentProcRepo == null)
			{
				_paymentProcRepo = CCPaymentProcessingRepository.GetCCPaymentProcessingRepository();
			}
			return _paymentProcRepo;
		}

		protected CCProcessingCenterBranch GetMappingRow(int? branchId, string procCenter)
		{
			var res = GetPaymentProcessingRepo().GetProcessingCenterBranchByBranchAndProcCenterIDs(branchId, procCenter);

			if (res == null)
			{
				Branch branch = Branch.PK.Find(Base, branchId);
				throw new PXException(CC.Messages.ProcCenterBranchMappingNotFound, branch.BranchCD, procCenter);
			}

			return res;
		}

		protected virtual string[] GetKeysAsString(List<TPrimary> docs)
		{
			var cnt = docs.Count;
			var ret = new string[cnt];
			var keys = Base.Caches[typeof(TPrimary)].BqlKeys;
			for(int i=0; i<cnt; i++)
			{
				var keyAsStr = string.Empty;
				foreach (var key in keys)
				{
					keyAsStr += Base.Caches[typeof(TPrimary)].GetValue(docs[i], key.Name);
				}
				ret[i] = keyAsStr;
			}
			return ret;
		}

		protected static void SetProcessRunning(string[] docKeys)
		{
			PXLongOperation.SetCustomInfo(docKeys, processKey);
		}

		protected virtual void CheckAnotherProcessRunning(string[] docKeys)
		{
			
			var curUser = PXContext.PXIdentity.User.Identity.Name;
			var curCompany = GetCompanyName(curUser);
			foreach (var proc in PXLongOperation.GetTaskList()
				.Where(i => PXLongOperation.GetStatus(i.NativeKey) == PXLongRunStatus.InProcess
					&& GetCompanyName(i.User) == curCompany))
			{
				var info = PXLongOperation.GetCustomInfo(proc.NativeKey, processKey) as string[];
				if (info != null)
				{
					var result = docKeys.Intersect(info);
					if (result.Count() > 0)
					{
						throw new PXException(ErrorMessages.PrevOperationNotCompleteYet);
					}
				}
			}
		}

		protected string GetCompanyName(string userName)
		{
			string companyName;
			if (CompanyService.IsMultiCompany)
			{
				companyName = CompanyService.ExtractCompany(userName);
			}
			else
			{
				companyName = CompanyService.GetSingleCompanyLoginName();
			}
			return companyName;
		}

		protected void CheckTranAgainstMapping(CCProcessingCenterBranch mappingRow, TransactionData tranData)
		{
			bool mappingOk = true;

			if (tranData.PaymentMethodType == MeansOfPayment.CreditCard
				&& (mappingRow?.CCPaymentMethodID == null || mappingRow?.CCCashAccountID == null))
			{
				mappingOk = false;
			}

			if (tranData.PaymentMethodType == MeansOfPayment.EFT
				&& (mappingRow?.EFTPaymentMethodID == null || mappingRow?.EFTCashAccountID == null))
			{
				mappingOk = false;
			}

			if (!mappingOk)
			{
				throw new PXException(CC.Messages.NoSuitablePMForPayment);
			}
		}

		protected MeansOfPayment GetMeansOfPayment(PayLinkDocument doc, CustomerClass customerClass)
		{
			var ret = MeansOfPayment.NotSpecified;

			if (doc == null)
			{
				throw new PXArgumentException(nameof(doc), ErrorMessages.ArgumentNullException);
			}

			if (customerClass == null)
			{
				throw new PXArgumentException(nameof(customerClass), ErrorMessages.ArgumentNullException);
			}

			var custClassExt = Base.Caches[typeof(CustomerClass)].GetExtension<CustomerClassPayLink>(customerClass);
			if (custClassExt.PayLinkPaymentMethod == PayLinkPaymentMethod.CreditCard)
			{
				ret = MeansOfPayment.CreditCard;
			}
			else if (custClassExt.PayLinkPaymentMethod == PayLinkPaymentMethod.Eft)
			{
				ret = MeansOfPayment.EFT;
			}

			var row = GetMappingRow(doc.BranchID, doc.ProcessingCenterID);

			if (ret == MeansOfPayment.NotSpecified && !CCMeansOfPmtIsReady(row)
				&& EFTMeansOfPmtIsReady(row))
			{
				ret = MeansOfPayment.EFT;
			}
			if (ret == MeansOfPayment.NotSpecified && !EFTMeansOfPmtIsReady(row)
				&& CCMeansOfPmtIsReady(row))
			{
				ret = MeansOfPayment.CreditCard;
			}

			if (ret == MeansOfPayment.CreditCard && !CCMeansOfPmtIsReady(row))
			{
				throw new PXException(CC.Messages.NoSuitablePMAndCAForMeansOfPayment, MeansOfPayment.CreditCard);
			}
			else if (ret == MeansOfPayment.EFT && !EFTMeansOfPmtIsReady(row))
			{
				throw new PXException(CC.Messages.NoSuitablePMAndCAForMeansOfPayment, MeansOfPayment.EFT);
			}

			return ret;
		}

		protected virtual void ShowActionStatusWarningIfNeeded(PXCache cache, CCPayLink payLink)
		{
			if (payLink.ActionStatus == PayLinkActionStatus.Error && payLink.ErrorMessage != null)
			{
				cache.RaiseExceptionHandling<CCPayLink.linkStatus>(payLink, payLink.Url,
					new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(CC.Messages.PayLinkProcessingError, payLink.ErrorMessage), PXErrorLevel.Warning));
			}
		}

		protected string GetCustomerProfileId(int? baccountID, string procCenterID)
		{
			var row = GetPaymentProcessingRepo().GetCustomerProcessingCenterByAccountAndProcCenterIDs(baccountID, procCenterID);
			return row?.CustomerCCPID;
		}

		protected virtual bool CheckPayLinkRelatedToDoc(CCPayLink payLink)
		{
			bool ret = true;
			var payLinkDoc = PayLinkDocument.Current;

			if (payLinkDoc?.PayLinkID != null)
			{
				var origVal = PayLinkDocument.Cache.GetValueOriginal<PayLinkDocument.payLinkID>(payLinkDoc) as int?;
				if (origVal == payLinkDoc.PayLinkID) return ret;

				var rowStatus = PayLinkDocument.Cache.GetStatus(payLinkDoc);

				if (rowStatus == PXEntryStatus.Inserted)
				{
					ret = false;
				}

				bool isOrder = payLinkDoc.OrderType != null;

				if (rowStatus == PXEntryStatus.Updated)
				{
					if (isOrder && (payLinkDoc.OrderType != payLink.OrderType || payLinkDoc.OrderNbr != payLink.OrderNbr))
					{
						ret = false;
					}
					if(!isOrder && (payLinkDoc.DocType != payLink.DocType || payLinkDoc.RefNbr != payLink.RefNbr))
					{
						ret = false;
					}
				}

				if (!ret)
				{
					PayLinkDocument.Cache.SetValue<PayLinkDocument.payLinkID>(payLinkDoc, null);
				}
			}
			return ret;
		}

		protected virtual IEnumerable<ARInvoice> GetInvoicesRelatedToOrder(SOOrder order)
		{
			foreach (ARInvoice invoice in PXSelectReadonly2<ARInvoice,
				InnerJoin<SOOrderShipment, On<SOOrderShipment.invoiceType, Equal<ARInvoice.docType>,
					And<SOOrderShipment.invoiceNbr, Equal<ARInvoice.refNbr>>>,
				InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARInvoice.docType>,
					And<ARRegister.refNbr, Equal<ARInvoice.refNbr>>>>>,
				Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
					And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
					And<ARInvoice.docType, Equal<ARDocType.invoice>,
					And<ARRegister.openDoc, Equal<True>,
					And<ARRegister.curyDocBal, Greater<Zero>>>>>>,
				OrderBy<Asc<ARRegister.createdDateTime>>>.Select(Base, order.OrderType, order.OrderNbr))
			{
				yield return invoice;
			}
		}

		protected virtual Customer GetCustomer(int? customerId)
		{
			Customer cust = PXSelect<Customer, Where<Customer.bAccountID,
				Equal<Required<Customer.bAccountID>>>>.Select(Base, customerId);
			return cust;
		}

		private void ApplyPaymentWithoutNeedSync(ARPaymentEntry paymentGraph, SOOrder order)
		{
			var pmtPayLinkExt = paymentGraph.GetExtension<ARPaymentEntryPayLink>();
			try
			{
				pmtPayLinkExt.DoNotSetNeedSync = true;
				ApplyPaymentToDocuments(paymentGraph, order);
			}
			finally
			{
				pmtPayLinkExt.DoNotSetNeedSync = false;
			}
		}

		private bool CCMeansOfPmtIsReady(CCProcessingCenterBranch row)
		{
			return row.CCPaymentMethodID != null && row.CCCashAccountID != null;
		}

		private bool EFTMeansOfPmtIsReady(CCProcessingCenterBranch row)
		{
			return row.EFTPaymentMethodID != null && row.EFTCashAccountID != null;
		}

		protected abstract void SaveDoc();
		public abstract void SetCurrentDocument(TPrimary doc);
		public abstract void CollectDataAndSyncLink();
		public abstract void CollectDataAndCreateLink();
		public abstract void SendNotification();
	}
}
