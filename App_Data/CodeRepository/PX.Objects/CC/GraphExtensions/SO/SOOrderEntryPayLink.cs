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
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.Extensions.PayLink;
using PX.Objects.TX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using PX.Objects.CS;
using PX.Objects.CC.PaymentProcessing.Helpers;
using PX.Objects.SO;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;

namespace PX.Objects.CC.GraphExtensions
{
	public class SOOrderEntryPayLink : PayLinkDocumentGraph<SOOrderEntry, SOOrder>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.acumaticaPayments>();
		}

		public PXSelect<CCPayLink, Where<CCPayLink.payLinkID, Equal<Current<SOOrderPayLink.payLinkID>>>> PayLink;

		public override void Initialize()
		{
			base.Initialize();
			Base.Actions.Move(nameof(ResendLink), nameof(DeactivateLink));
		}

		public PXAction<SOOrder> deactivateLink;
		[PXUIField(DisplayName = "Close Payment Link", Visible = true)]
		[PXButton(CommitChanges = true)]
		public virtual IEnumerable DeactivateLink(PXAdapter adapter)
		{
			SaveDoc();
			var docs = adapter.Get<SOOrder>().ToList();
			
			PXLongOperation.StartOperation(Base, delegate
			{
				foreach (var doc in docs)
				{
					var graph = PXGraph.CreateInstance<SOOrderEntry>();
					var ext = graph.GetExtension<SOOrderEntryPayLink>();
					ext.SetCurrentDocument(doc);
					ext.GetPaymentsAndCloseLink();
				}
			});
			
			return docs;
		}

		private void CheckDocBeforeLinkCreation(SOOrder doc)
		{
			if (!AllowedOrderStatus(doc))
			{
				throw new PXException(Messages.UseRelatedInvoiceToCreateLink, doc.OrderNbr);
			}

			var invoiceWithLink = GetRelatedInvoicesWithPayLink(doc).FirstOrDefault();
			if (invoiceWithLink != null)
			{
				var invoice = invoiceWithLink.Item1;
				throw new PXException(Messages.UseInvoiceToProcessPayLink, invoice.RefNbr, doc.OrderNbr);
			}
		}

		public override void CollectDataAndCreateLink()
		{
			var doc = Base.Document.Current;
			CheckDocBeforeLinkCreation(doc);
			var data = CollectDataToCreateLink(doc);
			var payLinkProcessing = GetPayLinkProcessing();
			var payLinkDoc = PayLinkDocument.Current;
			try
			{
				payLinkProcessing.CreateLink(payLinkDoc, data);
			}
			finally
			{
				if (payLinkDoc.PayLinkID != null)
				{
					PayLinkDocument.Update(payLinkDoc);
					Base.Save.Press();
				}
			}

			if (payLinkDoc.DeliveryMethod == PayLinkDeliveryMethod.Email)
			{
				SendNotification();
			}
		}

		public override void SendNotification()
		{
			const string id = "SALES ORDER PAY LINK";

			var payLinkDoc = PayLinkDocument.Current;
			if (payLinkDoc.DeliveryMethod != PayLinkDeliveryMethod.Email) return;

			var order = Base.Document.Current;
			var prms = new Dictionary<string, string>
			{
				["OrderType"] = order.OrderType,
				["OrderNbr"] = order.OrderNbr
			};
			var activityExt = Base.GetExtension<SOOrderEntry_ActivityDetailsExt>();
			activityExt.SendNotification(ARNotificationSource.Customer, id, order.BranchID, prms, true);
		}

		public virtual void GetPaymentsAndCloseLink()
		{
			var payLinkProcessing = GetPayLinkProcessing();
			var doc = Base.Document.Current;
			var payLinkDoc = PayLinkDocument.Current;
			var link = PayLink.SelectSingle();
			var payLinkData = payLinkProcessing.GetPayments(payLinkDoc, link);
			if (payLinkData.Transactions != null && payLinkData.Transactions.Count() > 0)
			{
				try
				{
					CreatePayments(doc, link, payLinkData);
				}
				catch (Exception ex)
				{
					payLinkProcessing.SetErrorStatus(ex, link);
					throw;
				}
			}

			payLinkProcessing.SetLinkStatus(link, payLinkData);

			if (payLinkData.StatusCode == V2.PayLinkStatus.Closed)
			{
				return;
			}

			var data = new V2.PayLinkProcessingParams();
			data.LinkGuid = link.NoteID;
			data.ExternalId = link.ExternalID;

			payLinkProcessing.CloseLink(payLinkDoc, link, data);
		}

		public override void CollectDataAndSyncLink()
		{
			var payLinkProcessing = GetPayLinkProcessing();
			var doc = Base.Document.Current;
			var payLinkDoc = PayLinkDocument.Current;
			var link = PayLink.SelectSingle();
			var payLinkData = payLinkProcessing.GetPayments(payLinkDoc, link);
			if (payLinkData.Transactions != null && payLinkData.Transactions.Count() > 0)
			{
				try
				{
					CreatePayments(doc, link, payLinkData);
				}
				catch (Exception ex)
				{
					payLinkProcessing.SetErrorStatus(ex, link);
					throw;
				}
			}

			payLinkProcessing.SetLinkStatus(link, payLinkData);

			if ((doc.Completed == true || doc.CuryUnpaidBalance == 0)
				&& payLinkData.StatusCode == V2.PayLinkStatus.Open)
			{
				var closeParams = new V2.PayLinkProcessingParams();
				closeParams.LinkGuid = link.NoteID;
				closeParams.ExternalId = link.ExternalID;
				payLinkProcessing.CloseLink(payLinkDoc, link, closeParams);
				return;
			}

			if (link.NeedSync == false
				|| payLinkData.StatusCode == V2.PayLinkStatus.Closed
				|| payLinkData.PaymentStatusCode == V2.PayLinkPaymentStatus.Paid)
			{
				return;
			}

			var data = CollectDataToSyncLink(doc, link);
			payLinkProcessing.SyncLink(payLinkDoc, link, data);
		}

		public virtual void UpdatePayLinkAndCreatePayments(V2.PayLinkData payLinkData)
		{
			var link = PayLink.SelectSingle();
			var doc = Base.Document.Current;
			var payLinkProcessing = GetPayLinkProcessing();
			var copyDoc = Base.Document.Cache.CreateCopy(doc) as SOOrder;
			payLinkProcessing.UpdatePayLinkByData(link, payLinkData);
			if (payLinkData.Transactions != null && payLinkData.Transactions.Count() > 0)
			{
				try
				{

					CreatePayments(copyDoc, link, payLinkData);
				}
				catch (Exception ex)
				{
					payLinkProcessing.SetErrorStatus(ex, link);
					throw;
				}
			}
			payLinkProcessing.SetLinkStatus(link, payLinkData);
		}

		[PXOverride]
		public virtual void Persist(Action baseMethod)
		{
			var payLink = PayLink.SelectSingle();

			if (payLink != null && CheckPayLinkRelatedToDoc(payLink) && PayLinkHelper.PayLinkOpen(payLink))
			{
				var curDoc = Base.Document.Current;
				var originalVal = Base.Document.Cache
						.GetValueOriginal<SOOrder.curyUnpaidBalance>(curDoc) as decimal?;
				var needSync = payLink.Amount != curDoc.CuryUnpaidBalance
					&& originalVal != curDoc.CuryUnpaidBalance;
				
				if (needSync)
				{
					payLink = GetActualPayLink(payLink);
					if (payLink.NeedSync == false)
					{
						payLink.NeedSync = needSync;
						PayLink.Update(payLink);
					}
				}
			}
			baseMethod();
		}

		protected virtual void _(Events.RowSelected<SOOrder> e)
		{
			var doc = e.Row;
			var cache = e.Cache;
			if (doc == null) return;

			var disablePayLink = false;
			var allowOverrideDeliveryMethod = false;
			var custClass = Base.customerclass.SelectSingle();
			if (custClass != null)
			{
				var custClassExt = GetCustomerClassExt(custClass);
				disablePayLink = custClassExt.DisablePayLink.GetValueOrDefault();
				allowOverrideDeliveryMethod = custClassExt.AllowOverrideDeliveryMethod.GetValueOrDefault();
			}

			var docExt = PayLinkDocument.Current;
			var link = PayLink.SelectSingle();
			var allowedOrderType = AllowedOrderType();
			var allowedOrderStatus = AllowedOrderStatus(doc);
			var needNewLink = link?.Url == null || PayLinkHelper.PayLinkWasProcessed(link);
			var allowCloseLink = link != null && PayLinkHelper.PayLinkOpen(link);
			var closedAndUnpaid = link != null && PayLinkHelper.PayLinkWasProcessed(link)
				&& link.PaymentStatus == PayLinkPaymentStatus.Unpaid;
			var rowStoredInDb = cache.GetStatus(doc) != PXEntryStatus.Inserted;

			createLink.SetEnabled(allowedOrderType && allowedOrderStatus
				&& docExt.ProcessingCenterID != null && needNewLink && !disablePayLink && rowStoredInDb);
			createLink.SetVisible(!disablePayLink && allowedOrderType);
			syncLink.SetEnabled(allowedOrderType && !needNewLink
				&& !disablePayLink && docExt.ProcessingCenterID != null);
			syncLink.SetVisible(!disablePayLink && allowedOrderType);
			deactivateLink.SetEnabled(allowedOrderType && allowCloseLink
				&& !needNewLink && !disablePayLink && docExt.ProcessingCenterID != null);
			deactivateLink.SetVisible(!disablePayLink && allowedOrderType);
			resendLink.SetEnabled(allowedOrderType && docExt.ProcessingCenterID != null
				&& !needNewLink && !disablePayLink
				&& docExt.DeliveryMethod == PayLinkDeliveryMethod.Email);
			resendLink.SetVisible(!disablePayLink && allowedOrderType);

			var hidePayLinkFields = disablePayLink && PayLink.SelectSingle()?.Url == null;
			var payLinkControlsVisible = !hidePayLinkFields && allowedOrderType;
			var payLinkControlsEnabled = payLinkControlsVisible && (link?.Url == null || closedAndUnpaid);

			PXUIFieldAttribute.SetEnabled<SOOrderPayLink.processingCenterID>(cache, doc, payLinkControlsEnabled);
			PXUIFieldAttribute.SetEnabled<SOOrderPayLink.deliveryMethod>(cache, doc, payLinkControlsEnabled && allowOverrideDeliveryMethod);

			PXUIFieldAttribute.SetVisible<SOOrderPayLink.processingCenterID>(cache, doc, payLinkControlsVisible);
			PXUIFieldAttribute.SetVisible<SOOrderPayLink.deliveryMethod>(cache, doc, payLinkControlsVisible);
			PXUIFieldAttribute.SetVisible<CCPayLink.url>(PayLink.Cache, null, payLinkControlsVisible);
			PXUIFieldAttribute.SetVisible<CCPayLink.needSync>(PayLink.Cache, null, payLinkControlsVisible);
			PXUIFieldAttribute.SetVisible<CCPayLink.linkStatus>(PayLink.Cache, null, payLinkControlsVisible);
		}

		protected virtual void _(Events.RowSelected<CCPayLink> e)
		{
			var doc = e.Row;
			var cache = e.Cache;
			if (doc == null) return;

			ShowActionStatusWarningIfNeeded(cache, doc);
		}

		protected virtual void _(Events.RowPersisting<SOOrder> e)
		{
			var row = e.Row;
			if (row == null || e.Operation != PXDBOperation.Delete) return;

			var payLinkDoc = e.Cache.GetExtension<SOOrderPayLink>(row);
			if (payLinkDoc?.PayLinkID != null)
			{
				var payLink = CCPayLink.PK.Find(Base, payLinkDoc.PayLinkID);

				if (payLink != null && !PayLinkHelper.PayLinkWasProcessed(payLink))
				{
					throw new PXRowPersistingException(nameof(CCPayLink.linkStatus), payLink.Url,
						Messages.ClosePayLinkBeforeCancelOrder, payLink.OrderType, payLink.OrderNbr);
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<SOOrder, SOOrderPayLink.deliveryMethod> e)
		{
			var row = e.Row;
			if (row == null) return;

			if (AllowedOrderType())
			{
				var custClass = Base.customerclass.SelectSingle();
				if (custClass == null) return;
				
				var custClassExt = GetCustomerClassExt(custClass);
				if (custClassExt.DisablePayLink.GetValueOrDefault() == false)
				{
					e.NewValue = custClassExt.DeliveryMethod;
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.curyID> e)
		{
			var order = e.Row;
			if (order == null) return;

			var payLink = PayLink.SelectSingle();
			if (payLink != null && !PayLinkHelper.PayLinkWasProcessed(payLink))
			{
				throw new PXSetPropertyException(Messages.DocHasPaymentLink, order.OrderType, order.OrderNbr);
			}
		}

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.customerID> e)
		{
			var order = e.Row;
			if (order == null) return;

			var payLink = PayLink.SelectSingle();
			if (payLink != null && !PayLinkHelper.PayLinkWasProcessed(payLink))
			{
				var errMsg = PXMessages.LocalizeFormatNoPrefix(Messages.DeactivatePayLinkToChangeCustomer, payLink.OrderType, payLink.OrderNbr);
				Base.RaiseCustomerIDSetPropertyException(e.Cache, order, e.NewValue, PXMessages.LocalizeFormatNoPrefix(errMsg));
			}
		}

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.cancelled> e)
		{
			var order = e.Row;
			var newValue = e.NewValue as bool?;
			if (order == null) return;

			var payLink = PayLink.SelectSingle();
			if (newValue == true && payLink != null && !PayLinkHelper.PayLinkWasProcessed(payLink))
			{
				throw new PXException(Messages.ClosePayLinkBeforeCancelOrder, payLink.OrderType, payLink.OrderNbr);
			}
		}

		protected virtual void _(Events.FieldUpdated<SOOrder, SOOrder.curyID> e)
		{
			var order = e.Row;
			if (order == null) return;

			e.Cache.SetDefaultExt<SOOrderPayLink.processingCenterID>(order);
		}

		protected virtual void _(Events.FieldUpdated<SOOrder, SOOrder.branchID> e)
		{
			var order = e.Row;
			var cache = e.Cache;
			var newVal = e.NewValue as int?;
			var oldVal = e.OldValue as int?;
			if (order == null) return;
			if (newVal == oldVal) return;

			var payLink = PayLink.SelectSingle();
			if (payLink != null && !PayLinkHelper.PayLinkWasProcessed(payLink)) return;

			cache.SetDefaultExt<SOOrderPayLink.processingCenterID>(order);
		}

		protected virtual void _(Events.FieldUpdated<SOOrder, SOOrder.customerID> e)
		{
			var order = e.Row;
			if (order != null)
			{
				e.Cache.SetDefaultExt<SOOrderPayLink.deliveryMethod>(order);
				e.Cache.SetDefaultExt<SOOrderPayLink.processingCenterID>(order);
			}
		}

		protected virtual void _(Events.FieldDefaulting<SOOrder, SOOrderPayLink.processingCenterID> e)
		{
			var row = e.Row;
			if (row == null || !AllowedOrderType()) return;

			var disablePayLink = false;
			var custClass = Base.customerclass.SelectSingle();
			if (custClass != null)
			{
				var custClassExt = GetCustomerClassExt(custClass);
				disablePayLink = custClassExt.DisablePayLink.GetValueOrDefault();
			}

			if (disablePayLink == false)
			{
				CCProcessingCenter procCenter = PXSelectJoin<CCProcessingCenter,
					InnerJoin<CashAccount, On<CCProcessingCenter.cashAccountID, Equal<CashAccount.cashAccountID>>,
					InnerJoin<CCProcessingCenterBranch, On<CCProcessingCenterBranch.processingCenterID, Equal<CCProcessingCenter.processingCenterID>>>>,
					Where<CashAccount.curyID, Equal<Required<CashAccount.curyID>>,
						And<CCProcessingCenter.allowPayLink, Equal<True>,
						And<CCProcessingCenter.isActive, Equal<True>,
						And<CCProcessingCenterBranch.defaultForBranch, Equal<True>,
						And<CCProcessingCenterBranch.branchID, Equal<Required<CCProcessingCenterBranch.branchID>>>>>>>>
					.Select(Base, row.CuryID, row.BranchID);

				if (procCenter != null)
				{
					e.NewValue = procCenter.ProcessingCenterID;
				}
			}
		}

		protected virtual string CreateFormTitle(SOOrder doc)
		{
			var cust = GetCustomer(doc.CustomerID);
			var title = string.Empty;
			if (doc.CustomerOrderNbr != null)
			{
				title += doc.CustomerOrderNbr;
			}
			if (cust.AcctName != null)
			{
				title += " " + cust.AcctName;
			}
			if (doc.OrderDesc != null)
			{
				title += " " + doc.OrderDesc;
			}

			title = title.Trim();
			return title;
		}

		protected virtual V2.PayLinkProcessingParams CollectDataToCreateLink(SOOrder doc)
		{
			var docExt = PayLinkDocument.Current;
			var payLinkData = new V2.PayLinkProcessingParams();
			var procCenterStr = docExt.ProcessingCenterID;
			var pc = GetPaymentProcessingRepo().GetProcessingCenterByID(procCenterStr);
			var contact = Base.Billing_Contact.SelectSingle();
			var meansOfPayment = GetMeansOfPayment(docExt, Base.customerclass.SelectSingle());

			string customerPCID = GetCustomerProfileId(doc.CustomerID, docExt.ProcessingCenterID);
			if (customerPCID == null)
			{
				var payLinkProc = GetPayLinkProcessing();
				customerPCID = payLinkProc.CreateCustomerProfileId(doc.CustomerID, docExt.ProcessingCenterID);
			}

			payLinkData.MeansOfPayment = meansOfPayment;
			payLinkData.DueDate = doc.RequestDate.Value;
			payLinkData.CustomerProfileId = customerPCID;
			payLinkData.AllowPartialPayments = pc.AllowPartialPayment.GetValueOrDefault();
			payLinkData.FormTitle = CreateFormTitle(doc);

			CalculateAndSetLinkAmount(doc, payLinkData);

			return payLinkData;
		}
		protected virtual V2.PayLinkProcessingParams CollectDataToSyncLink(SOOrder doc, CCPayLink payLink)
		{
			var payLinkData = new V2.PayLinkProcessingParams();
			payLinkData.DueDate = doc.RequestDate.Value;
			payLinkData.LinkGuid = payLink.NoteID;
			payLinkData.ExternalId = payLink.ExternalID;

			CalculateAndSetLinkAmount(doc, payLinkData);

			return payLinkData;
		}

		protected virtual void CalculateAndSetLinkAmount(SOOrder doc, V2.PayLinkProcessingParams payLinkParams)
		{
			var amountToSend = 0m;
			var docExt = PayLinkDocument.Current;
			var docData = new V2.DocumentData();
			docData.DocType = doc.OrderType;
			docData.DocRefNbr = doc.OrderNbr;
			docData.DocBalance = doc.CuryUnpaidBalance.Value;
			payLinkParams.DocumentData = docData;

			List<V2.DocumentDetailData> detailData = new List<V2.DocumentDetailData>();
			amountToSend += PopulateDocDetailData(detailData);
			docData.DocumentDetails = detailData;

			var headerTaxes = CalculateHeaderTaxes(doc);
			if (headerTaxes > 0)
			{
				docData.ExcludedTaxes = headerTaxes.Value;
				amountToSend += docData.ExcludedTaxes;
			}

			var docDiscounts = CalculateDocDiscounts(doc);
			if (docDiscounts != 0)
			{
				docData.DocDiscounts = docDiscounts.Value;
				amountToSend += -1 * docData.DocDiscounts;
			}

			if (doc.CuryFreightTot != 0)
			{
				docData.Freight = doc.CuryFreightTot.Value;
				amountToSend += docData.Freight;
			}

			var aplDocData = new List<V2.AppliedDocumentData>();
			amountToSend -= PopulateAppliedDocData(aplDocData, docExt, payLinkParams);

			docData.AppliedDocuments = aplDocData;
			payLinkParams.Amount = amountToSend;
		}

		[PXOverride]
		public virtual void OrderCreated(SOOrder document, SOOrder source, SOOrderEntry.OrderCreatedDelegate baseMethod)
		{
			baseMethod(document, source);
			var currDoc = PayLinkDocument.Current;
			if (currDoc?.PayLinkID != null)
			{
				currDoc.PayLinkID = null;
				PayLinkDocument.Update(currDoc);
			}
		}
		protected virtual CCPayLink GetActualPayLink(CCPayLink payLink)
		{
			//The CCPayLink.NeedSync flag can be updated in parallel by Business Event and by an user through UI. 
			//This method allows you to read the current DB state (not cached) of the NeedSync flag.
			Base.Caches[typeof(CCPayLink)].ClearQueryCache();
			CCPayLink actualPayLink = PXSelect<CCPayLink, Where<CCPayLink.payLinkID,
				Equal<Required<CCPayLink.payLinkID>>>>.Select(Base, payLink.PayLinkID);
			return actualPayLink;
		}

		private decimal PopulateDocDetailData(List<V2.DocumentDetailData> detailData)
		{
			decimal total = 0;
			foreach (var detail in Base.Transactions.Select().RowCast<SOLine>())
			{
				var detailDataItem = new V2.DocumentDetailData();
				detailDataItem.ItemName = detail.TranDesc;
				detailDataItem.Price = detail.CuryLineAmt.GetValueOrDefault();
				detailDataItem.Quantity = detail.Qty.Value;
				detailDataItem.Uom = detail.UOM;
				detailDataItem.LineNbr = detail.LineNbr.Value;

				total += detailDataItem.Price;

				detailData.Add(detailDataItem);
			}
			return total;
		}

		protected virtual PayLinkDocumentMapping GetMapping()
		{
			return new PayLinkDocumentMapping(typeof(SOOrder));
		}

		public override void SetCurrentDocument(SOOrder doc)
		{

			Base.Document.Current = Base.Document.Search<SOOrder.orderNbr>(doc.OrderNbr, doc.OrderType);
		}

		protected override void SaveDoc()
		{
			Base.Save.Press();
		}

		protected virtual IEnumerable<Tuple<ARInvoice, CCPayLink>> GetRelatedInvoicesWithPayLink(SOOrder order)
		{
			foreach (PXResult<ARInvoice, SOOrderShipment, ARRegister, CCPayLink> row in PXSelectJoin<ARInvoice,
			InnerJoin<SOOrderShipment, On<SOOrderShipment.invoiceType, Equal<ARInvoice.docType>,
				And<SOOrderShipment.invoiceNbr, Equal<ARInvoice.refNbr>>>,
			InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARInvoice.docType>,
				And<ARRegister.refNbr, Equal<ARInvoice.refNbr>>>,
			InnerJoin<CCPayLink, On<CCPayLink.payLinkID, Equal<ARInvoicePayLink.payLinkID>>>>>,
			Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
				And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
				And<ARInvoice.docType, Equal<ARDocType.invoice>,
				And<ARRegister.openDoc, Equal<True>,
				And<ARRegister.curyDocBal, Greater<Zero>>>>>>,
			OrderBy<Asc<ARRegister.createdDateTime>>>.Select(Base, order.OrderType, order.OrderNbr))
			{
				CCPayLink payLink = (CCPayLink)row;
				yield return new Tuple<ARInvoice, CCPayLink>(row, payLink);
			}
		}

		private decimal PopulateAppliedDocData(List<V2.AppliedDocumentData> aplDocData, PayLinkDocument payLinkDoc, V2.PayLinkProcessingParams payLinkParams)
		{
			var adjDocTotal = 0m;
			var newLink = payLinkParams.ExternalId == null;
			var adjustments = Base.Adjustments.Select().RowCast<SOAdjust>().Where(i => i.Voided == false);

			IEnumerable<ExternalTransaction> payLinkExtTran = null;

			if (payLinkDoc.PayLinkID != null && !newLink)
			{
				payLinkExtTran = GetPaymentProcessingRepo().GetExternalTransactionsByPayLinkID(payLinkDoc.PayLinkID);
			}

			foreach (var detail in adjustments)
			{
				if (payLinkExtTran != null)
				{
					var res = payLinkExtTran.Any(i => i.DocType == detail.AdjgDocType && i.RefNbr == detail.AdjgRefNbr);
					if (res) continue;
				}

				var aplDocDataItem = new V2.AppliedDocumentData();
				aplDocDataItem.Amount = detail.CuryAdjdAmt.Value + detail.CuryAdjdBilledAmt.Value + detail.CuryAdjdDiscAmt.Value;
				aplDocDataItem.DocRefNbr = detail.AdjgRefNbr;
				aplDocDataItem.DocType = detail.AdjgDocType;
				adjDocTotal += aplDocDataItem.Amount;
				aplDocData.Add(aplDocDataItem);
			}

			return adjDocTotal;
		}

		private decimal? CalculateDocDiscounts(SOOrder order)
		{
			var docDiscounts = 0m;
			var discDetails = Base.DiscountDetails.Select().RowCast<SOOrderDiscountDetail>();
			foreach (var discItem in discDetails.Where(i => i.SkipDiscount == false))
			{
				docDiscounts += discItem.CuryDiscountAmt.GetValueOrDefault();
			}
			return docDiscounts;
		}

		private decimal? CalculateHeaderTaxes(SOOrder order)
		{
			var headerTaxes = order.CuryTaxTotal;

			foreach (var taxItem in GetTaxes(order))
			{
				if (taxItem.Item2.TaxCalcLevel == "0")
				{
					headerTaxes -= taxItem.Item1.CuryTaxAmt.GetValueOrDefault();
				}
			}
			return headerTaxes;
		}

		private List<Tuple<SOTaxTran, Tax>> GetTaxes(SOOrder order)
		{
			var res = PXSelectJoin<SOTaxTran, InnerJoin<Tax, On<SOTaxTran.taxID, Equal<Tax.taxID>>>,
				Where<SOTaxTran.orderNbr, Equal<Required<SOTaxTran.orderNbr>>,
					And<SOTaxTran.orderType, Equal<Required<SOTaxTran.orderType>>>>>
			.Select(Base, order.OrderNbr, order.OrderType);

			var taxList = new List<Tuple<SOTaxTran, Tax>>();
			foreach (PXResult<SOTaxTran, Tax> item in res)
			{
				taxList.Add(Tuple.Create((SOTaxTran)item, (Tax)item));
			}
			return taxList;
		}

		private CustomerClassPayLink GetCustomerClassExt(CustomerClass custClass)
		{
			return Base.customerclass.Cache.GetExtension<CustomerClassPayLink>(custClass);
		}

		private bool AllowedOrderStatus(SOOrder doc)
		{
			bool allowed = doc.Completed == false && doc.Cancelled == false && doc.IsExpired == false
				&& (doc.Approved == true || doc.Hold == true)
				&& doc.CuryUnpaidBalance > 0
				&& (doc.Behavior.IsNotIn(SOBehavior.IN, SOBehavior.MO) || doc.BilledCntr == 0);
			return allowed;
		}

		private bool AllowedOrderType()
		{
			var allowed = Base.soordertype.Current?.CanHavePayments ?? false;
			return allowed;
		}

		protected class PayLinkDocumentMapping : IBqlMapping
		{
			public Type OrderType = typeof(PayLinkDocument.orderType);
			public Type OrderNbr = typeof(PayLinkDocument.orderNbr);
			public Type BranchID = typeof(PayLinkDocument.branchID);
			public Type ProcessingCenterID = typeof(PayLinkDocument.processingCenterID);
			public Type DeliveryMethod = typeof(PayLinkDocument.deliveryMethod);
			public Type PayLinkID = typeof(PayLinkDocument.payLinkID);
			public Type Table { get; private set; }
			public Type Extension => typeof(PayLinkDocument);
			public PayLinkDocumentMapping(Type table)
			{
				Table = table;
			}
		}
	}
}
