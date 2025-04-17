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

using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Shopify.API.REST;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using PX.Commerce.Shopify.API.GraphQL;

namespace PX.Commerce.Shopify
{
	public class SPPaymentEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Payment;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedPayment Payment;
		public MappedOrder Order;
	}

	public class SPPaymentsRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			return null;
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			#region Payments
			return base.Restrict<MappedPayment>(mapped, delegate (MappedPayment obj)
			{
				if (obj.Extern != null)
				{
					if (obj.Extern.Kind == TransactionType.Void)
					{
						// we should skip void payments
						return new FilterResult(FilterStatus.Ignore,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedVoid, obj.Extern.Id));
					}

					if (obj.Extern.Kind == TransactionType.Refund)
					{
						// we should skip refund payments now, and we will support later
						return new FilterResult(FilterStatus.Ignore,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedMethodNotSupported, obj.Extern.Id, TransactionType.Refund.ToString()));
					}

					if (obj.Extern.Kind == TransactionType.Capture)
					{
						// we should skip Capture to avoid the duplicated processing
						return new FilterResult(FilterStatus.Ignore,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedMethodNotSupported, obj.Extern.Id, TransactionType.Capture.ToString()));
					}

					if (obj.Extern.ParentId != null && processor.SelectStatus(BCEntitiesAttribute.Payment, new Object[] { obj.Extern.OrderId, obj.Extern.ParentId }.KeyCombine()) != null)
					{
						return new FilterResult(FilterStatus.Ignore,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedParentSynced, obj.Extern.Id, obj.Extern.ParentId));
					}


					var orderStatus = (BCSyncStatus)processor.SelectStatus(BCEntitiesAttribute.Order, obj.Extern?.OrderId.ToString(), false);

					//If the Sales order sync record is not in Synchronized, Pending status, then the Payment Entity should not be processed as it depends on the Sales Order entity.
					bool shouldSkipIfOrderNotSync = orderStatus == null || (processor is SPPaymentProcessor && orderStatus.LocalID == null) ||
													(orderStatus.LocalID != null && orderStatus.Status != BCSyncStatusAttribute.Synchronized && orderStatus.Status != BCSyncStatusAttribute.Pending);
					if (shouldSkipIfOrderNotSync)
					{
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedOrderNotSynced, obj.Extern.Id, obj.Extern.OrderId));
					}

					if (obj.Extern.Status.IsNotIn(TransactionStatus.Success, TransactionStatus.Pending))
					{
						string errorMessage = obj.Extern.Status == TransactionStatus.Failure
												? PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedFailed, obj.Extern.OrderId, obj.Extern.Message)
												: PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedError, obj.Extern.Id);
						// we should skip payments with error
						return new FilterResult(FilterStatus.Invalid, errorMessage);
					}

					//skip if active is not true
					IEnumerable<BCPaymentMethods> paymentMethod = BCPaymentMethodsMappingSlot.Get(processor.Operation.Binding).Where(x => x.StorePaymentMethod == obj.Extern.Gateway.ReplaceEmptyString(BCConstants.NoneGateway).ToUpper());
					BCPaymentMethods matchedMethod = paymentMethod?.FirstOrDefault();
					if (mode != FilterMode.Merge
						&& matchedMethod != null && matchedMethod.Active != true)
					{
						return new FilterResult(FilterStatus.Filtered,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedNotConfigured, obj.Extern.Gateway.ReplaceEmptyString(BCConstants.NoneGateway)));
					}
				}

				return null;
			});
			#endregion
		}
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.Payment, BCCaptions.Payment, 90,
		IsInternal = false,
		Direction = SyncDirection.Import,
		PrimaryDirection = SyncDirection.Import,
		PrimarySystem = PrimarySystem.Extern,
		PrimaryGraph = typeof(PX.Objects.AR.ARPaymentEntry),
		ExternTypes = new Type[] { typeof(OrderTransaction) },
		LocalTypes = new Type[] { typeof(Payment) },
		AcumaticaPrimaryType = typeof(PX.Objects.AR.ARPayment),
		//AcumaticaPrimarySelect = typeof(Search<PX.Objects.AR.ARPayment.refNbr, //Entity Requires Parent Selection, which is not possible in Add/Edit Panel now.
		//	Where<PX.Objects.AR.ARPayment.docType, Equal<ARDocType.payment>,
		//		Or<PX.Objects.AR.ARPayment.docType, Equal<ARDocType.prepayment>>>>),
		URL = "orders/{0}",
		Requires = new string[] { BCEntitiesAttribute.Order }
	)]
	[BCProcessorRealtime(PushSupported = false, HookSupported = false)]
	public class SPPaymentProcessor : PaymentProcessorBase<SPPaymentProcessor, SPPaymentEntityBucket, MappedPayment>
	{
		protected IOrderRestDataProvider orderDataRESTProvider;
		protected IOrderGQLDataProvider orderDataGQLProvider;
		protected BCBinding currentBinding;
		protected BCBindingExt currentBindingExt;
		protected BCBindingShopify currentShopifySettings;

		#region Factories
		[InjectDependency]
		protected ISPRestDataProviderFactory<IOrderRestDataProvider> orderDataRESTProviderFactory { get; set; }
		[InjectDependency]
		public ISPGraphQLAPIClientFactory shopifyGraphQLClientFactory { get; set; }
		[InjectDependency]
		protected ISPGraphQLDataProviderFactory<OrderGQLDataProvider> orderDataGQLProviderFactory { get; set; }
		#endregion
		#region Constructor
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
			currentBinding = GetBinding();
			currentBindingExt = GetBindingExt<BCBindingExt>();
			currentShopifySettings = GetBindingExt<BCBindingShopify>();

			var client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());
			orderDataRESTProvider = orderDataRESTProviderFactory.CreateInstance(client);

			var graphQLClient = shopifyGraphQLClientFactory.GetClient(GetBindingExt<BCBindingShopify>());
			orderDataGQLProvider = orderDataGQLProviderFactory.GetProvider(graphQLClient);
		}

		#endregion

		public override async Task<PXTransactionScope> WithTransaction(Func<Task> action)
		{
			await action();
			return null;
		}

		public override async Task<PullSimilarResult<MappedPayment>> PullSimilar(IExternEntity entity, CancellationToken cancellationToken = default)
		{
			OrderTransaction externEntity = (OrderTransaction)entity;

			var uniqueFieldValue = externEntity.Id.ToString();

			if (string.IsNullOrEmpty(uniqueFieldValue))
				return null;
			List<MappedPayment> result = new List<MappedPayment>();
			foreach (PX.Objects.AR.ARRegister item in GetHelper<SPHelper>().PaymentByExternalRef.Select(uniqueFieldValue))
			{
				Payment data = new Payment() { SyncID = item.NoteID, SyncTime = item.LastModifiedDateTime };
				result.Add(new MappedPayment(data, data.SyncID, data.SyncTime));
			}
			return new PullSimilarResult<MappedPayment>() { UniqueField = uniqueFieldValue, Entities = result };
		}

		public override void ControlDirection(SPPaymentEntityBucket bucket, BCSyncStatus status, ref bool shouldImport, ref bool shouldExport, ref bool skipSync, ref bool skipForce)
		{
			MappedPayment payment = bucket.Payment;
			if (payment?.IsNew == true) return;

			bool? shouldSkipSync = payment.Local?.Status?.Value == PX.Objects.AR.Messages.Voided ||
				(payment.Local?.Status?.Value != PX.Objects.AR.Messages.CCHold && payment.Extern?.Action.IsIn(TransactionType.Capture, TransactionType.Sale) == true);
			if (shouldSkipSync == true)
				{
					shouldImport = false;
				skipForce = true;
					skipSync = true;
				UpdateStatus(payment, status.LastOperation, status.LastErrorMessage);// to update extern hash in case of Shopify payment if its voided or captured in acumatica
				}
		}

		#region Pull
		public override async Task<MappedPayment> PullEntity(Guid? localID, Dictionary<string, object> fields, CancellationToken cancellationToken = default)
		{
			Payment impl = cbapi.GetByID<Payment>(localID);
			if (impl == null) return null;

			MappedPayment obj = new MappedPayment(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}
		public override async Task<MappedPayment> PullEntity(String externID, String jsonObject, CancellationToken cancellationToken = default)
		{
			OrderTransaction data = orderDataRESTProvider.GetOrderSingleTransaction(externID.KeySplit(0), externID.KeySplit(1));
			if (data == null) return null;

			MappedPayment obj = new MappedPayment(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), null, data.DateModifiedAt.ToDate(false), data.CalculateHash());

			return obj;
		}
		#endregion

		#region Import
		public override async Task FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			var delaySecs = -currentShopifySettings.ApiDelaySeconds ?? 0;

			FilterOrders filter = new FilterOrders { Status = OrderStatus.Any, Fields = "id,source_name,financial_status,updated_at,created_at,cancelled_at,closed_at" };
			GetHelper<SPHelper>().SetFilterMinDate(filter, minDateTime, currentBindingExt.SyncOrdersFrom, delaySecs);
			if (maxDateTime != null) filter.UpdatedAtMax = maxDateTime.Value.ToLocalTime();

			IEnumerable<OrderData> datas = orderDataRESTProvider.GetAll(filter, cancellationToken);

			foreach (OrderData orderData in datas)
			{
				if (this.SelectStatus(BCEntitiesAttribute.Order, orderData.Id.ToString()) == null)
					continue; //Skip if order not synced

				var transactionList = orderDataRESTProvider.GetOrderTransactions(orderData.Id.ToString(), cancellationToken);
				if (transactionList == null || transactionList.Count == 0) continue;
				//Only process the successful Transaction
				foreach (OrderTransaction data in transactionList)
				{
					if ((data.Status == TransactionStatus.Success && data.Kind == TransactionType.Sale &&
						transactionList.Any(x => x.ParentId == data.Id && x.Status == TransactionStatus.Success && x.Kind == TransactionType.Refund && x.Amount == data.Amount &&
						x.ProcessedAt.HasValue && x.ProcessedAt.Value.Subtract(data.ProcessedAt.Value).TotalSeconds < 10)) ||
						(data.Status == TransactionStatus.Success && data.Kind == TransactionType.Refund && data.ParentId != null &&
						transactionList.Any(x => x.Id == data.ParentId && x.Status == TransactionStatus.Success && x.Kind == TransactionType.Sale && x.Amount == data.Amount &&
						data.ProcessedAt.HasValue && data.ProcessedAt.Value.Subtract(x.ProcessedAt.Value).TotalSeconds < 10)))
					{
						//Skip successful payment and its refund payment to avoid payment amount greater than order total amount issue if another payment is unsuccessful and system rolls back all payments 
						continue;
					}
					SPPaymentEntityBucket bucket = CreateBucket();

					MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.OrderNumber, orderData.DateModifiedAt.ToDate(false));
					EntityStatus orderStatus = EnsureStatus(order);

					GetHelper<SPHelper>().PopulateAction(transactionList, data);
					MappedPayment obj = bucket.Payment = bucket.Payment.Set(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), orderData.OrderNumber, data.DateModifiedAt.ToDate(false), data.CalculateHash()).With(_ => { _.ParentID = order.SyncID; return _; });
					EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
				}
			}
		}
		public override async Task<EntityStatus> GetBucketForImport(SPPaymentEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			string orderId = syncstatus.ExternID.KeySplit(0);
			string transactionId = syncstatus.ExternID.KeySplit(1);
			OrderData orderData = BCExtensions.GetSharedSlot<OrderData>(orderId) ?? orderDataRESTProvider.GetByID(orderId, false, true, false);
			if (orderData == null || orderData.Transactions == null || !orderData.Transactions.Any(x => x?.Id.ToString() == transactionId))
				return EntityStatus.None;

			OrderTransaction data = orderData.Transactions.FirstOrDefault(x => x?.Id.ToString() == transactionId);
			string lastKind = GetHelper<SPHelper>().PopulateAction(orderData.Transactions, data);
			var paymentLastModified = data.DateModifiedAt.ToDate(false);

			// only retrieve fees when payment is of ShopifyPayments type
			if (IsShopifyPayments(data.Gateway))
			{
				IEnumerable<OrderTransactionGQL> orderTransactions = await orderDataGQLProvider.GetOrderTransactionsAsync(data.OrderId.ToString(), cancellationToken);
				OrderTransactionGQL targetOrderTransaction = GetTargetOrderTransaction(data.Kind, data.Id.ToString(), orderTransactions);
				paymentLastModified = targetOrderTransaction?.ProcessedAt?.ToDate(true) ?? paymentLastModified;
				data.Fees = targetOrderTransaction?.Fees ?? new List<OrderTransactionFeeGQL>();
			}

			MappedPayment obj = bucket.Payment = bucket.Payment.Set(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), orderData.Name ?? orderData.OrderNumber, data.DateModifiedAt.ToDate(false), data.CalculateHash());
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			//Used to determine transaction type in case of credit card
			data.LastKind = lastKind;

			MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.Name ?? orderData.OrderNumber, orderData.DateModifiedAt.ToDate(false));
			EntityStatus orderStatus = EnsureStatus(order);

			return status;
		}

		/// <summary>
		/// Retrieves the <see cref="OrderTransactionGQL"/> from <paramref name="orderTransactions"/> based on <paramref name="orderTransactionID"/> and the provided <see cref="TransactionType"/>.<br/>
		/// If current transaction is of type Authorization, then check if there's any corresponding transaction of type Capture for it.<br/>
		/// If there's a Capture transaction for it, then we need to use info for Fees of that transaction and apply to the current Authorization transaction.
		/// </summary>
		/// <param name="transactionType">The <see cref="TransactionType"/> if the refered transaction.</param>
		/// <param name="orderTransactionID"></param>
		/// <param name="orderTransactions"></param>
		/// <returns>The found <see cref="OrderTransactionGQL"/> or null.</returns>
		public virtual OrderTransactionGQL GetTargetOrderTransaction(string transactionType, string orderTransactionID, IEnumerable<OrderTransactionGQL> orderTransactions)
		{
            if (string.Equals(transactionType, TransactionType.Authorization, StringComparison.OrdinalIgnoreCase))
                return orderTransactions?.LastOrDefault(x => string.Equals(x.ParentTransaction?.Id.ConvertGidToId(), orderTransactionID)
                    && string.Equals(x.Kind, OrderTransactionTypeGQL.Capture, StringComparison.OrdinalIgnoreCase));

            return orderTransactions?.FirstOrDefault(x => x.Id.ConvertGidToId() == orderTransactionID);
        }

		public override async Task MapBucketImport(SPPaymentEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedPayment obj = bucket.Payment;

			OrderTransaction data = obj.Extern;
			Payment impl = obj.Local = new Payment();
			Payment presented = existing?.Local as Payment;

			PXResult<PX.Objects.SO.SOOrder, PX.Objects.AR.Customer, PX.Objects.CR.Location, BCSyncStatus> result = PXSelectJoin<PX.Objects.SO.SOOrder,
				InnerJoin<PX.Objects.AR.Customer, On<PX.Objects.AR.Customer.bAccountID, Equal<SOOrder.customerID>>,
				InnerJoin<PX.Objects.CR.Location, On<PX.Objects.CR.Location.locationID, Equal<SOOrder.customerLocationID>>,
				InnerJoin<BCSyncStatus, On<PX.Objects.SO.SOOrder.noteID, Equal<BCSyncStatus.localID>>>>>,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
					And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>
				.Select(this, BCEntitiesAttribute.Order, data.OrderId).Select(r => (PXResult<SOOrder, PX.Objects.AR.Customer, PX.Objects.CR.Location, BCSyncStatus>)r).FirstOrDefault();
			if (result == null) throw new PXException(BCMessages.OrderNotSyncronized, data.OrderId);

			PX.Objects.SO.SOOrder order = result.GetItem<PX.Objects.SO.SOOrder>();
			PX.Objects.AR.Customer customer = result.GetItem<PX.Objects.AR.Customer>();
			PX.Objects.CR.Location location = result.GetItem<PX.Objects.CR.Location>();

			//if payment already exists and then no need to map fields just call action, except by NeedRelease field.
			BCPaymentMethods methodMapping = GetHelper<SPHelper>().GetPaymentMethodMapping(data.Gateway.ReplaceEmptyString(BCConstants.NoneGateway), data.Currency, out string cashAcount);

			// Calculate charges for Shopify_Payments payments
			impl.Charges = IsShopifyPayments(data.Gateway)
				? CalculateShopifyFeesAsCharges(data.Fees, methodMapping)
				: new List<PaymentCharge>();

			if (presented?.Id != null && obj.Extern.Action.IsIn(TransactionType.Void, TransactionType.Capture))
			{
				impl.NeedRelease = methodMapping?.ReleasePayments ?? false;
				return;
			}
			impl.Type = presented?.Type ?? PX.Objects.AR.Messages.Prepayment.ValueField();
			impl.CustomerID = customer.AcctCD.ValueField();
			impl.CustomerLocationID = location.LocationCD.ValueField();
			impl.CurrencyID = data.Currency.ValueField();
			var date = data.DateCreatedAt.ToDate(false, PXTimeZoneInfo.FindSystemTimeZoneById(currentBindingExt.OrderTimeZone));
			if (date.HasValue)
				impl.ApplicationDate = (new DateTime(date.Value.Date.Ticks)).ValueField();
			impl.PaymentAmount = ((decimal)data.Amount).ValueField();
			impl.BranchID = Branch.PK.Find(this, currentBinding.BranchID)?.BranchCD?.ValueField();
			impl.Hold = false.ValueField();

			bool isExistentMethodValid = presented != null && presented.PaymentMethod != methodMapping?.PaymentMethodID?.Trim()?.ValueField();
			impl.PaymentMethod = (isExistentMethodValid) ? presented.PaymentMethod : methodMapping?.PaymentMethodID?.Trim()?.ValueField();
			impl.CashAccount = (isExistentMethodValid) ? presented.CashAccount : cashAcount?.Trim()?.ValueField();
			impl.NeedRelease = methodMapping?.ReleasePayments ?? false;
			impl.ExternalRef = data.Id.ToString().ValueField();

			impl.PaymentRef = GetHelper<SPHelper>().ParseTransactionNumber(data, out bool isCreditCardTransaction).ValueField();

			TransactionStatus? lastStatus = bucket.Order.Extern.Transactions.LastOrDefault(x => x.ParentId == data.Id && x.Status == TransactionStatus.Success)?.Status ?? data.Status;

			var paymentDesc = PXMessages.LocalizeFormat(ShopifyMessages.PaymentDescription, currentBinding.BindingName, bucket.Order?.Extern?.Name, GetHelper<SPHelper>().FirstCharToUpper(data.Kind), lastStatus?.ToString(), GetHelper<SPHelper>().GetGatewayDescr(data));
			impl.Description = paymentDesc.ValueField();
			if (!(presented?.Id != null && (obj.Extern.Action == TransactionType.Void || obj.Extern.Action == TransactionType.Capture)))
			{
				//Credit Card:
				if (methodMapping?.ProcessingCenterID != null && isCreditCardTransaction)
				{
					GetHelper<SPHelper>().AddCreditCardProcessingInfo(methodMapping, impl, data.LastKind);
				}
			}

			//Calculated Unpaid Balance
			decimal curyUnpaidBalance = order.CuryOrderTotal ?? 0m;
			foreach (SOAdjust adj in PXSelect<SOAdjust,
				Where<SOAdjust.adjdOrderType, Equal<Required<SOOrder.orderType>>,
					And<SOAdjust.adjdOrderNbr, Equal<Required<SOOrder.orderNbr>>>>>
				.Select(this, order.OrderType, order.OrderNbr))
			{
				curyUnpaidBalance -= adj.CuryAdjdAmt ?? 0m;
			}

			decimal remainingAmount = (decimal)data.Amount;

			//First try to attach Invoice if payment is not fully invoiced then try to attach SO
			if (presented?.DocumentsToApply == null || (presented != null && presented?.AppliedToDocuments?.Value != curyUnpaidBalance))
			{
				foreach (PXResult<SOOrderShipment, ARInvoice> pXResult in PXSelectJoin<SOOrderShipment,
						 InnerJoin<ARInvoice, On<SOOrderShipment.invoiceType, Equal<ARInvoice.docType>, And<SOOrderShipment.invoiceNbr, Equal<ARInvoice.refNbr>>>>,
						 Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
						 And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>>>>.
						 Select(this, order.OrderType, order.OrderNbr))
				{
					var shipment = pXResult.GetItem<SOOrderShipment>();
					var invoice = pXResult.GetItem<ARInvoice>();
					if (!string.IsNullOrEmpty(shipment.InvoiceNbr) && invoice.CuryDocBal > 0)
					{
						if (remainingAmount == 0) break;
						//if there is already invoice attached to payment then make sure both payment and invoice are released otherwise cannot attach new payment to invoice
						foreach (ARAdjust adjust in PXSelect<ARAdjust, Where<ARAdjust.adjgDocType, Equal<ARDocType.prepayment>,
								And<Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>, And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>>>>>>.
								Select(this, invoice.DocType, invoice.RefNbr))
						{
							if (adjust.Released != true || invoice.Released != true) throw new PXException(BCMessages.PaymentAndInvoiceNotReleased, invoice.RefNbr, adjust.AdjgRefNbr);
						}

						Core.API.PaymentDetail detail = new Core.API.PaymentDetail();
						detail.DocType = PX.Objects.AR.Messages.Invoice.ValueField();
						detail.ReferenceNbr = shipment.InvoiceNbr.ValueField();
						//if invoice balance is less than remain amount then apply invoice balance to payment
						decimal amountToApply = remainingAmount > (invoice.CuryDocBal ?? 0) ? (invoice.CuryDocBal ?? 0) : remainingAmount;
						remainingAmount -= amountToApply;
						detail.AmountPaid = amountToApply.ValueField();

						if (impl.DocumentsToApply == null) impl.DocumentsToApply = new List<Core.API.PaymentDetail>();
						impl.DocumentsToApply.Add(detail);

					}
				}
			}
			//If we have applied already, or if fully invoiced than skip
			if ((order.CuryUnbilledOrderTotal > 0 || (order.CuryUnbilledOrderTotal == 0 && order.Status == SOOrderStatus.Cancelled)) && ((existing as MappedPayment) == null
				|| ((MappedPayment)existing).Local == null
				|| ((MappedPayment)existing).Local.OrdersToApply == null
				|| !((MappedPayment)existing).Local.OrdersToApply.Any(d => d.OrderType?.Value == order.OrderType && d.OrderNbr?.Value == order.OrderNbr)))
			{
				var applicationAmount = remainingAmount;
				//Validation of unpaid balance
				if (applicationAmount > curyUnpaidBalance) applicationAmount = curyUnpaidBalance;

				//validation of payment balance
				decimal? balance = presented?.AvailableBalance?.Value;
				if (balance != null && applicationAmount > balance) applicationAmount = balance.Value;

				//validation of Unbilled balance. If any invoice is created/released for the order, we cannot apply more than the left unbilled amount.
				if ((order.BilledCntr != null && order.BilledCntr > 0) || (order.ReleasedCntr != null && order.ReleasedCntr > 0))
				{
					if (applicationAmount > order.CuryUnbilledOrderTotal) applicationAmount = order.CuryUnbilledOrderTotal ?? 0;
				}
				if (applicationAmount < 0) applicationAmount = 0m;

				//If order is refunded or canceled we still link payment with the order, so we pass 0 to AppliedToOrder.
				//Even openDoc is false, we removed the validation from the OrderToApply field  to support it.
				if (bucket.Order.Extern.FinancialStatus == OrderFinancialStatus.Refunded)
				{
					applicationAmount = 0m;
				}

				//Order to Apply
				PaymentOrderDetail detail = new PaymentOrderDetail();
				detail.OrderType = order.OrderType.ValueField();
				detail.OrderNbr = order.OrderNbr.ValueField();
				detail.AppliedToOrder = applicationAmount.ValueField();
				impl.OrdersToApply = new List<PaymentOrderDetail>(new[] { detail });
			}
		}

		public override async Task SaveBucketImport(SPPaymentEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
			MappedPayment obj = bucket.Payment;
			Boolean needRelease = obj.Local.NeedRelease;
			Payment impl = (Payment)existing?.Local;

			// when payment is Shopify payment we need to update it first to add charges to the payment before perform Capture
			if (existing?.IsNew != false || IsShopifyPayments(obj.Extern.Gateway))
			{
				using (var transaction = await base.WithTransaction(async () =>
				{
					impl = UpsertPayment(obj);
				}))
				{
					transaction?.Complete();
				}
			}
		

			impl = await ReleasePayment(obj, impl, needRelease);

			//Executing actions or updating existing payment.
			using (var transaction = await base.WithTransaction(async () =>
			{
				string transactionNumber = GetHelper<SPHelper>().ParseTransactionNumber(obj.Extern, out bool isCreditCard);
				//If we import an inexistent credit card payment with a Capture Action, it is already created when we create payment, we don't need to do it again.
				bool shouldCapture = obj.Extern.Action.IsIn(TransactionType.Capture, TransactionType.Sale)
										&& isCreditCard && existing?.IsNew == false && impl?.Status?.Value != PX.Objects.AR.Messages.Capture;
				//It is not needed to void a payment again if it has been already voided.									
				bool shouldVoid = obj.Extern.Action == TransactionType.Void && impl?.Type.Value.IsNotIn(PX.Objects.AR.Messages.VoidPayment, PX.Objects.AR.Messages.Voided) == true;
				//Lastly, we have to update the payment in cases where it wasn't created before.
				bool shouldUpdate = existing?.IsNew == false && impl?.Status?.Value.IsIn(PX.Objects.AR.Messages.Balanced, PX.Objects.AR.Messages.Hold) == true;

				if (shouldCapture)
				{
					impl = CapturePayment(obj, impl, transactionNumber);
				}
				else if (shouldVoid)
				{
					bool cannotVoid = impl.Status?.Value.IsIn(PX.Objects.AR.Messages.Balanced, PX.Objects.AR.Messages.Hold) == true && impl.IsCCPayment?.Value != true;
					if (cannotVoid) throw new PXException(BCMessages.PaymentNeedsToBeReleased, impl.ReferenceNbr.Value, impl.PaymentMethod.Value);

					impl = VoidTransaction(obj, impl, transactionNumber);
				}
				else if (shouldUpdate)
				{
					impl = UpsertPayment(obj);
				}
			}))
			{
				transaction?.Complete();
			}

			impl = await ReleasePayment(obj, impl, needRelease);
			UpdateStatus(obj, operation);

			BCSyncStatus orderStatus = SelectFrom<BCSyncStatus>
										.InnerJoin<SOOrder>.On<SOOrder.noteID.IsEqual<BCSyncStatus.localID>>
										.Where<BCSyncStatus.syncID.IsEqual<@P.AsInt>>
										.View.SelectSingleBound(this, null, bucket.Order.SyncID);

			if (orderStatus?.LocalID != null) //Payment save updates the order, we need to change the saved timestamp.
			{
				orderStatus.LocalTS = BCSyncExactTimeAttribute.SelectDateTime<SOOrder.lastModifiedDateTime>(orderStatus.LocalID.Value);
				orderStatus = (BCSyncStatus)Statuses.Cache.Update(orderStatus);
			}
		}

		/// <summary>
		/// Performs card operation.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="impl"></param>
		/// <param name="transactionNumber"></param>
		/// <returns>Updated payment.</returns>
		public virtual Payment CapturePayment(MappedPayment obj, Payment impl, string transactionNumber)
		{
			impl = cbapi.Invoke<Payment, CardOperation>(null, impl.Id, action: new CardOperation()
			{
				TranType = CCTranTypeCode.PriorAuthorizedCapture.ValueField(),
				Amount = obj.Extern.Amount.ValueField(),
				TranNbr = transactionNumber.ValueField(),
			});
			obj.AddLocal(impl, obj.LocalID, impl.SyncTime);
			return impl;
		}

		/// <summary>
		/// Creates or updates payment.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>Updated Payment.</returns>
		public virtual Payment UpsertPayment(MappedPayment obj)
		{
			Payment impl = cbapi.Put<Payment>(obj.Local, obj.LocalID);
			obj.AddLocal(impl, impl?.SyncID, impl?.SyncTime);
			return impl;
		}

		/// <summary>
		/// Evaluates if <see cref="Payment.NeedRelease"/> in <paramref name="obj"/> is true and <paramref name="payment"/> status is <see cref="PX.Objects.AR.Messages.Balanced"/>.
		/// If the evaluation is positive then releases <paramref name="payment"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="needRelease"></param>
		/// <param name="payment"></param>
		/// <param name="ignoreResult">Sets if payment releasing should be ignored. False by default.</param>
		/// <remarks>This method uses its own transaction.</remarks>
		/// <returns>The updated payment or the provided payment</returns>
		public virtual async Task<Payment> ReleasePayment(MappedPayment obj, Payment payment, bool needRelease, bool ignoreResult = false)
		{
			Payment respPayment = payment;
			if (needRelease && payment?.Status?.Value == PX.Objects.AR.Messages.Balanced)
			{
				using (var transaction = await base.WithTransaction(async () =>
				{
					respPayment = cbapi.Invoke<Payment, ReleasePayment>(null, payment.NoteID.Value, ignoreResult: ignoreResult);
					if (respPayment != null)
					{
						//It is not allowed to change localID from obj and we have to update the sync time when we release voided payment.
						Guid? localID = (payment.Type?.Value == PX.Objects.AR.Messages.VoidPayment) ? obj.LocalID : respPayment.SyncID;
						obj.AddLocal(null, localID, respPayment.SyncTime);
					}
				}))
				{
					transaction?.Complete();
				}
			}

			return respPayment;
		}

		/// <summary>
		/// Voids the <paramref name="payment"/> if there is a <see cref="Payment.ProcessingCenterID"/>. Otherwise creates a new <see cref="VoidCardPayment"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="payment"></param>
		/// <param name="transactionNumber"></param>
		/// <returns>Updated payment.</returns>
		public virtual Payment VoidTransaction(MappedPayment obj, Payment payment, string transactionNumber = null)
		{
			Payment impl = !string.IsNullOrEmpty(payment.ProcessingCenterID?.Value) ? cbapi.Invoke<Payment, VoidCardPayment>(null, payment.Id, action: new VoidCardPayment()
			{
				TranType = CCTranTypeCode.VoidTran.ValueField(),
				TranNbr = (transactionNumber ?? GetHelper<SPHelper>().ParseTransactionNumber(obj.Extern, out _)).ValueField()
			}) : cbapi.Invoke<Payment, VoidPayment>(null, payment.Id);
			obj.AddLocal(null, obj.LocalID, impl.SyncTime);

			return impl;
		}

		/// <summary>
		/// Calculates Shopify fees based on the linked fees.
		/// </summary>
		/// <param name="externFees"></param>
		/// <param name="paymentMethod"></param>
		/// <exception cref="PXException"></exception>
		public virtual List<PaymentCharge> CalculateShopifyFeesAsCharges(IEnumerable<OrderTransactionFeeGQL> externFees, BCPaymentMethods paymentMethod)
		{
			PXCache cache = Caches[typeof(BCFeeMapping)];
			List<BCFeeMapping> existingFees = BCFeeMappingSlot.Get(currentBinding.BindingID, paymentMethod.PaymentMappingID);
			List<string> externFeesTypes = externFees.Select(x => x.Type)?.ToList() ?? new List<string>();

			//Validation
			List<string> undefinedFees = GetUndefinedFees(existingFees.Select(x => x.FeeType), externFeesTypes, paymentMethod.PaymentMappingID, cache);
			var unlinkedFees = existingFees.Where(x => externFeesTypes.Contains(x.FeeType) && string.IsNullOrEmpty(x.EntryTypeID)).Select(x => x.FeeType);
			undefinedFees.AddRange(unlinkedFees);

			if (undefinedFees.Any())
				throw new PXException(ShopifyMessages.PaymentFailedFeesNotLinkedToEntryType, paymentMethod.StorePaymentMethod, paymentMethod.PaymentMethodID, string.Join(", ", undefinedFees));

			return CreatePaymentCharges(externFees, existingFees);
		}

		/// <summary>
		/// Creates a list <see cref="PaymentCharge"/> from <see cref="externFees"/> that are greater than 0.
		/// </summary>
		/// <param name="externFees"></param>
		/// <param name="existingFees"></param>
		/// <returns>A new list of charges.</returns>
		public virtual List<PaymentCharge> CreatePaymentCharges(IEnumerable<OrderTransactionFeeGQL> externFees, List<BCFeeMapping> existingFees)
		{
			List<PaymentCharge> existentCharges = new List<PaymentCharge>();

			foreach (OrderTransactionFeeGQL itemFee in externFees.Where(fee => fee.Amount?.Amount > 0))
			{
				string entryType = existingFees.FirstOrDefault(x => x.FeeType == itemFee.Type)?.EntryTypeID;

				existentCharges.Add(new PaymentCharge()
				{
					EntryTypeID = entryType.ValueField(),
					Amount = itemFee.Amount?.Amount.ValueField()
				});
			}

			return existentCharges;
		}

		/// <summary>
		/// Checks the existence of Fees not saved in store settings of ERP and saves them if found.
		/// </summary>
		/// <param name="existingFees"></param>
		/// <param name="externFeesTypes"></param>
		/// <param name="paymentMappingID"></param>
		/// <param name="cache"></param>
		/// <returns>List of not defined fees</returns>
		public virtual List<string> GetUndefinedFees(IEnumerable<string> existingFees, IEnumerable<string> externFeesTypes, int? paymentMappingID, PXCache cache)
		{
			List<string> undefinedFees = externFeesTypes.Except(existingFees).ToList();
			if (undefinedFees.Any())
				SaveFeesMapping(undefinedFees, paymentMappingID, cache);

			return undefinedFees;
		}

		/// <summary>
		/// Creates new <see cref="BCFeeMapping"/> for each <paramref name="feesToSave"/> and saves them in database.
		/// </summary>
		/// <param name="feesToSave"></param>
		/// <param name="paymentMappingID"></param>
		/// <param name="cache"></param>
		public virtual void SaveFeesMapping(IEnumerable<string> feesToSave, int? paymentMappingID, PXCache cache)
		{
			foreach (string fee in feesToSave)
			{
				BCFeeMapping newFee = new BCFeeMapping()
				{
					BindingID = currentBinding.BindingID,
					PaymentMappingID = paymentMappingID,
					FeeType = fee
				};
				cache.Insert(newFee);
			}
			cache.Persist(PXDBOperation.Insert);
			cache.Persisted(false);
		}

		/// <summary>
		/// Checks if the <paramref name="gateway"/> is <see cref="ShopifyConstants.ShopifyPayments"/>.
		/// </summary>
		/// <param name="gateway"></param>
		/// <returns>A boolean representing the result of the operation.</returns>
		public virtual bool IsShopifyPayments(string gateway) => gateway == ShopifyConstants.ShopifyPayments;
		#endregion

		#region Export
		public override async Task FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
		}
		public override async Task<EntityStatus> GetBucketForExport(SPPaymentEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			Payment impl = cbapi.GetByID<Payment>(syncstatus.LocalID);
			if (impl == null) return EntityStatus.None;

			MappedPayment obj = bucket.Payment = bucket.Payment.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(bucket.Payment, SyncDirection.Export);

			return status;
		}

		public override async Task MapBucketExport(SPPaymentEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
		}
		public override async Task SaveBucketExport(SPPaymentEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
		}
		#endregion
	}
}
