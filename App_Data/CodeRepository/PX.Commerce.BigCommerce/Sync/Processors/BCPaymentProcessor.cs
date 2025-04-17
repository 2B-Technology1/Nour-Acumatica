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

using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
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
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce
{
	public class BCPaymentEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Payment;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedPayment Payment;
		public MappedOrder Order;
	}

	public class BCPaymentsRestrictor : BCBaseRestrictor, IRestrictor
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
					// we do not need to check status and event of payments that are to be created from order 
					if (obj.Extern.PaymentMethod != BCConstants.Emulated)
					{
						if (obj.Extern.Event != OrderPaymentEvent.Authorization && obj.Extern.Event != OrderPaymentEvent.Purchase && obj.Extern.Event != OrderPaymentEvent.Pending)
					{
						// we should skip payment transactions except Authorized or Purchase
						return new FilterResult(FilterStatus.Ignore,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedEventNotSupported, obj.Extern.Id, obj.Extern.Event));
					}

					if (obj.Extern.Status != BCConstants.BCPaymentStatusOk)
					{
						// we should skip payments with error
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedError, obj.Extern.Id));
					}
					}

					if (mode != FilterMode.Merge
						&& String.IsNullOrEmpty(obj.Extern.PaymentMethod))
					{
						// we should skip custom payments
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedMethodEmpty, obj.Extern.Id));
					}

					var orderStatus = (BCSyncStatus)processor.SelectStatus(BCEntitiesAttribute.Order, obj.Extern.OrderId, false);

					//If the Sales order sync record is not in Synchronized, Pending status, then the Payment Entity should not be processed as it depends on the Sales Order entity.
					bool shouldSkipIfOrderNotSync = orderStatus == null || (processor is BCPaymentProcessor && orderStatus.LocalID == null) || 
													(orderStatus.LocalID != null && orderStatus.Status != BCSyncStatusAttribute.Synchronized && orderStatus.Status != BCSyncStatusAttribute.Pending);
					if (shouldSkipIfOrderNotSync)
					{
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedOrderNotSynced, obj.Extern.Id, obj.Extern.OrderId));
					}

					//skip if payment method not present at all or ProcessPayment is not true
					IEnumerable<BCPaymentMethods> paymentMethods = BCPaymentMethodsMappingSlot.Get(processor.Operation.Binding);
					string method;
					IEnumerable<BCPaymentMethods> matchedMethods = null;
					if (obj.Extern.PaymentMethod == BCConstants.Emulated)
					{
						method = obj.Extern.Gateway;
						matchedMethods = paymentMethods.Where(x => x.StorePaymentMethod == method?.ToUpper());
					}
					else
					{
						method = string.Format("{0} ({1})", obj.Extern.Gateway, obj.Extern.PaymentMethod);
						matchedMethods = paymentMethods.Where(x => x.StorePaymentMethod == method.ToUpper() && x.StoreOrderPaymentMethod?.ToUpper() == obj.Extern.OrderPaymentMethod?.ToUpper());
						if (matchedMethods == null || matchedMethods?.Count() == 0)
							matchedMethods = paymentMethods.Where(x => x.StorePaymentMethod == method.ToUpper());

					}
					BCPaymentMethods matchedMethod = matchedMethods?.FirstOrDefault();
					if (mode != FilterMode.Merge
						&& matchedMethod != null && matchedMethod.Active != true)
					{
						return new FilterResult(FilterStatus.Filtered,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogPaymentSkippedNotConfigured, obj.Extern.Gateway));
					}
				}

				return null;
			});

			#endregion
		}
	}

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.Payment, BCCaptions.Payment, 130,
		IsInternal = false,
		Direction = SyncDirection.Import,
		PrimaryDirection = SyncDirection.Import,
		PrimarySystem = PrimarySystem.Extern,
		PrimaryGraph = typeof(PX.Objects.AR.ARPaymentEntry),
		ExternTypes = new Type[] { typeof(OrdersTransactionData) },
		LocalTypes = new Type[] { typeof(Payment) },
		AcumaticaPrimaryType = typeof(PX.Objects.AR.ARPayment),
		//AcumaticaPrimarySelect = typeof(Search<PX.Objects.AR.ARPayment.refNbr, //Entity Requires Parent Selection, which is not possible in Add/Edit Panel now.
		//	Where<PX.Objects.AR.ARPayment.docType, Equal<ARDocType.payment>,
		//		Or<PX.Objects.AR.ARPayment.docType, Equal<ARDocType.prepayment>>>>),
		URL = "orders?keywords={0}&searchDeletedOrders=no",
		Requires = new string[] { BCEntitiesAttribute.Order }
	)]
	[BCProcessorRealtime(PushSupported = false, HookSupported = false)]
	public class BCPaymentProcessor : PaymentProcessorBase<BCPaymentProcessor, BCPaymentEntityBucket, MappedPayment>
	{
		[InjectDependency]
		protected IBCRestDataProviderFactory<IOrderRestDataProvider> orderDataProviderFactory { get; set; }
		protected IOrderRestDataProvider orderDataProvider;
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildReadOnlyRestDataProvider<OrdersTransactionData>> orderTransactionsRestDataProviderFactory { get; set; }
		protected IChildReadOnlyRestDataProvider<OrdersTransactionData> orderTransactionsRestDataProvider;

		#region Constructor
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
			var client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());
			orderDataProvider = orderDataProviderFactory.CreateInstance(client);
			orderTransactionsRestDataProvider = orderTransactionsRestDataProviderFactory.CreateInstance(client);
		}

		#endregion
		public override async Task<PXTransactionScope> WithTransaction(Func<Task> action)
		{
			await action();
			return null;
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
			OrdersTransactionData data = orderTransactionsRestDataProvider.GetByID(externID.KeySplit(1), externID.KeySplit(0));
			if (data == null) return null;

			MappedPayment obj = new MappedPayment(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), data.Id.ToString(), data.DateCreatedUT.ToDate(), data.CalculateHash());

			return obj;
		}
		public override async Task<PullSimilarResult<MappedPayment>> PullSimilar(IExternEntity entity, CancellationToken cancellationToken = default)
		{
			var externEntity = (OrdersTransactionData)entity;

			var uniqueFieldValue = externEntity.Id.ToString();
			if (string.IsNullOrEmpty(uniqueFieldValue))
				return null;
			List<MappedPayment> result = new List<MappedPayment>();
			foreach (PX.Objects.AR.ARRegister item in GetHelper<BCHelper>().PaymentByExternalRef.Select(uniqueFieldValue))
			{
				Payment data = new Payment() { SyncID = item.NoteID, SyncTime = item.LastModifiedDateTime };
				result.Add(new MappedPayment(data, data.SyncID, data.SyncTime));
			}
			return new PullSimilarResult<MappedPayment>() { UniqueField = uniqueFieldValue, Entities = result };
		}
		#endregion

		public override void ControlDirection(BCPaymentEntityBucket bucket, BCSyncStatus status, ref bool shouldImport, ref bool shouldExport, ref bool skipSync, ref bool skipForce)
		{
			MappedPayment payment = bucket.Payment;
			if (payment?.IsNew == true) return;

			bool? shouldSkipSync = (payment.Local?.Status?.Value == PX.Objects.AR.Messages.Voided) ||
				(payment.Local?.Status?.Value != PX.Objects.AR.Messages.CCHold && payment.Extern.Action.IsIn(OrderPaymentEvent.Capture, OrderPaymentEvent.Finalization, OrderPaymentEvent.Purchase));
			if (shouldSkipSync == true)
				{
					shouldImport = false;
				skipForce = true;
					skipSync = true;
				UpdateStatus(payment, status.LastOperation, status.LastErrorMessage);
				}
		}

		#region Import
		public override async Task FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();

			FilterOrders filter = new FilterOrders { IsDeleted = "false", Sort = "date_modified:asc" };
			GetHelper<BCHelper>().SetFilterMinDate(filter, minDateTime, bindingExt.SyncOrdersFrom);
			if (maxDateTime != null) filter.MaxDateModified = maxDateTime;

			IEnumerable<OrderData> orders = orderDataProvider.GetAll(filter, cancellationToken);

			foreach (OrderData orderData in orders)
			{
				if (this.SelectStatus(BCEntitiesAttribute.Order, orderData.Id.ToString()) == null)
					continue; //Skip if order not synced

				var transactions = orderTransactionsRestDataProvider.GetAll(orderData.Id.ToString(), cancellationToken)?.ToList() ?? new List<OrdersTransactionData>();
				foreach (OrdersTransactionData data in transactions)
				{
					BCPaymentEntityBucket bucket = CreateBucket();

					MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.Id?.ToString(), orderData.DateModifiedUT.ToDate());
					EntityStatus orderStatus = EnsureStatus(order);

					GetHelper<BCHelper>().PopulateAction(transactions, data);

					MappedPayment obj = bucket.Payment = bucket.Payment.Set(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), orderData.Id?.ToString(), data.DateCreatedUT.ToDate(), data.CalculateHash()).With(_ => { _.ParentID = order.SyncID; return _; });
					EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
				}
				//Should skip creating Payment from order if transactions exists, to avoid duplicated Payment methods.
				if (transactions.Any() == false && GetHelper<BCHelper>().CreatePaymentfromOrder(orderData.PaymentMethod))
				{
					OrdersTransactionData data = CreateOrderTransactionData(orderData);
					BCPaymentEntityBucket bucket = CreateBucket();

					MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.Id?.ToString(), orderData.DateModifiedUT.ToDate());
					EntityStatus orderStatus = EnsureStatus(order);

					MappedPayment obj = bucket.Payment = bucket.Payment.Set(data, data.Id.ToString(), data.Id.ToString(), orderData.DateModifiedUT.ToDate(), data.CalculateHash()).With(_ => { _.ParentID = order.SyncID; return _; });
					EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
				}
			}
		}
		public override  async Task<EntityStatus> GetBucketForImport(BCPaymentEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			if (syncstatus.ExternID.HasParent())
			{
				List<OrdersTransactionData> transactions = orderTransactionsRestDataProvider.GetAll(syncstatus.ExternID.KeySplit(0), cancellationToken)?.ToList();

				OrdersTransactionData data = transactions.FirstOrDefault(x => x.Id.ToString() == syncstatus.ExternID.KeySplit(1));
				if (data == null) return EntityStatus.None;

				OrderData orderData = orderDataProvider.GetByID(data.OrderId);
				data.OrderPaymentMethod = orderData.PaymentMethod;
				string lastEvent = GetHelper<BCHelper>().PopulateAction(transactions, data);

				MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.Id?.ToString(), orderData.DateModifiedUT.ToDate());
				EntityStatus orderStatus = EnsureStatus(order);
				MappedPayment obj = bucket.Payment = bucket.Payment.Set(data, new Object[] { data.OrderId, data.Id }.KeyCombine(), orderData.Id?.ToString(), data.DateCreatedUT.ToDate(), data.CalculateHash()).With(_ => { _.ParentID = order.SyncID; return _; }); ;
				EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

				data.LastEvent = lastEvent;


				return status;
			}
			else
			{
				OrderData orderData = orderDataProvider.GetByID(syncstatus.ExternID);
				if (GetHelper<BCHelper>().CreatePaymentfromOrder(orderData.PaymentMethod))
				{
					OrdersTransactionData data = CreateOrderTransactionData(orderData);
					MappedOrder order = bucket.Order = bucket.Order.Set(orderData, orderData.Id?.ToString(), orderData.Id?.ToString(), orderData.DateModifiedUT.ToDate());
					EntityStatus orderStatus = EnsureStatus(order);

					MappedPayment obj = bucket.Payment = bucket.Payment.Set(data, data.Id.ToString(), orderData.Id?.ToString(), orderData.DateModifiedUT.ToDate(), data.CalculateHash()).With(_ => { _.ParentID = order.SyncID; return _; });
					EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

					return status;
				}
				return EntityStatus.None;
			}
		}

		public override async Task MapBucketImport(BCPaymentEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedPayment obj = bucket.Payment;

			OrdersTransactionData data = obj.Extern;
			Payment impl = obj.Local = new Payment();
			BCBinding binding = GetBinding();
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
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

			//if payment already exists and then no need to map fields just call action, except by NeedRelease field.
			BCPaymentMethods methodMapping = GetHelper<BCHelper>().GetPaymentMethodMapping(GetHelper<BCHelper>().GetPaymentMethodName(data), bucket.Order?.Extern?.PaymentMethod, data.Currency, out string cashAcount);
			if (presented?.Id != null && (obj.Extern.Action.IsIn(OrderPaymentEvent.Void, OrderPaymentEvent.Capture, OrderPaymentEvent.Finalization)))
			{
				impl.NeedRelease = methodMapping?.ReleasePayments ?? false;
				return;
			}


			PX.Objects.SO.SOOrder order = result.GetItem<PX.Objects.SO.SOOrder>();
			PX.Objects.AR.Customer customer = result.GetItem<PX.Objects.AR.Customer>();
			PX.Objects.CR.Location location = result.GetItem<PX.Objects.CR.Location>();

			//Product
			impl.Type = presented?.Type ?? PX.Objects.AR.Messages.Prepayment.ValueField();
			impl.CustomerID = customer.AcctCD.ValueField();
			impl.CustomerLocationID = location.LocationCD.ValueField();
			impl.CurrencyID = data.Currency.ValueField();
			var date = data.DateCreatedUT.ToDate(PXTimeZoneInfo.FindSystemTimeZoneById(bindingExt.OrderTimeZone));
			if (date.HasValue)
				impl.ApplicationDate = (new DateTime(date.Value.Date.Ticks)).ValueField();

			impl.PaymentAmount = ((decimal)data.Amount).ValueField();
			impl.BranchID = Branch.PK.Find(this, binding.BranchID)?.BranchCD?.ValueField();
			impl.Hold = false.ValueField();

			if (presented != null && presented.PaymentMethod != methodMapping?.PaymentMethodID?.Trim()?.ValueField())
				impl.PaymentMethod = presented.PaymentMethod;
			else
				impl.PaymentMethod = methodMapping?.PaymentMethodID?.Trim()?.ValueField();

			impl.CashAccount = cashAcount?.Trim()?.ValueField();
			impl.NeedRelease = methodMapping?.ReleasePayments ?? false;

			if (methodMapping.StorePaymentMethod == BCObjectsConstants.GiftCertificateCode)
				impl.Description = PXMessages.LocalizeFormat(BigCommerceMessages.PaymentDescriptionGC, binding.BindingName, methodMapping.StorePaymentMethod, data.OrderId, data.Id, data.GiftCertificate?.Code).ValueField();
			else
				impl.Description = PXMessages.LocalizeFormat(BigCommerceMessages.PaymentDescription, binding.BindingName, methodMapping.StorePaymentMethod, data.OrderId, data.Id).ValueField();

			impl.ExternalRef = data.Id.ToString().ValueField();
			impl.PaymentRef = GetHelper<BCHelper>().ParseTransactionNumber(data, out bool isCreditCardTransaction).ValueField();

			if (!(presented?.Id != null && (obj.Extern.Action == OrderPaymentEvent.Void || obj.Extern.Action == OrderPaymentEvent.Capture)))
			{
				//Credit Card:
				if (methodMapping?.ProcessingCenterID != null && isCreditCardTransaction)
				{
					GetHelper<BCHelper>().AddCreditCardProcessingInfo(methodMapping, impl, data.LastEvent, data.PaymentInstrumentToken, data.CreditCard);
				}
			}

			//Calculated Unpaid Balance
			decimal curyUnpaidBalance = order.CuryOrderTotal ?? 0m;
			foreach (SOAdjust adj in PXSelect<SOAdjust,
				Where<SOAdjust.voided, Equal<False>,
					And<SOAdjust.adjdOrderType, Equal<Required<SOOrder.orderType>>,
					And<SOAdjust.adjdOrderNbr, Equal<Required<SOOrder.orderNbr>>>>>>
				.Select(this, order.OrderType, order.OrderNbr))
			{
				curyUnpaidBalance -= adj.CuryAdjdAmt ?? 0m;
			}

			Payment payment = ((MappedPayment)existing)?.Local;
			decimal remainingAmount = (decimal)data.Amount;
			
			//First try to attach Invoice if payment is not fully invoiced then try to attach SO
			if (payment?.DocumentsToApply == null || (payment != null && payment?.AppliedToDocuments?.Value != curyUnpaidBalance))
			{
				foreach (PXResult<SOOrderShipment, ARInvoice> pXResult in PXSelectJoin<SOOrderShipment,
						InnerJoin<ARInvoice, On<SOOrderShipment.invoiceType, Equal<ARInvoice.docType>, And<SOOrderShipment.invoiceNbr, Equal<ARInvoice.refNbr>>>>,
						Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
						And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>>>>.
						Select(this, order.OrderType, order.OrderNbr))
				{
					var shipment = pXResult.GetItem<SOOrderShipment>();
					var invoice = pXResult.GetItem<ARInvoice>();
					//if there is invoice and there is balance in invoice then attach invoice to payment
					if (!string.IsNullOrEmpty(shipment.InvoiceNbr) && invoice.CuryDocBal > 0)
					{
						if (remainingAmount == 0) break;

						//if there is already invoice attached to payment then make sure both payment and invoice are released otherwise cannot attach new payment to invoice
						foreach (ARAdjust adjust in PXSelect<ARAdjust, Where<ARAdjust.adjgDocType, Equal<ARDocType.prepayment>,
								And<Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>, And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>>>>>>.
								Select(this, invoice.DocType, invoice.RefNbr))
						{
							if (adjust.Released != true || invoice.Released != true) throw new PXException(BCMessages.PaymentAndInvoiceNotReleased,invoice.RefNbr, adjust.AdjgRefNbr);
						}
						PaymentDetail detail = new PaymentDetail();
						detail.DocType = PX.Objects.AR.Messages.Invoice.ValueField();
						detail.ReferenceNbr = shipment.InvoiceNbr.ValueField();

						//if invoice balance is less than remaing amount than apply invoice balance to payment
						decimal amountToApply = remainingAmount > (invoice.CuryDocBal ?? 0) ? (invoice.CuryDocBal ?? 0) : remainingAmount;
						remainingAmount -= amountToApply;
						detail.AmountPaid = amountToApply.ValueField();

						if (impl.DocumentsToApply == null) impl.DocumentsToApply = new List<PaymentDetail>();
						impl.DocumentsToApply.Add(detail);

					}
				}
			}

			//If we have applied already, or if fully invoiced than skip
			if ((order.CuryUnbilledOrderTotal > 0 || (order.CuryUnbilledOrderTotal == 0 && order.Status ==  SOOrderStatus.Cancelled)) && ((existing as MappedPayment) == null
				|| ((MappedPayment)existing).Local == null
				|| ((MappedPayment)existing).Local.OrdersToApply == null
				|| !((MappedPayment)existing).Local.OrdersToApply.Any(d => d.OrderType?.Value == order.OrderType && d.OrderNbr?.Value == order.OrderNbr)))
			{
				var applicationAmount = remainingAmount;
				//Validation of unpaid balance
				if (applicationAmount > curyUnpaidBalance) applicationAmount = curyUnpaidBalance;

				//validation of payment balance
				decimal? balance = payment?.AvailableBalance?.Value;
				if (balance != null && applicationAmount > balance) applicationAmount = balance.Value;

				//validation of Unbilled balance. If any invoice is created/released for the order, we cannot apply more than the left unbilled amount.
				if ((order.BilledCntr != null && order.BilledCntr > 0) || (order.ReleasedCntr != null && order.ReleasedCntr > 0))
				{
					if (applicationAmount > order.CuryUnbilledOrderTotal) applicationAmount = order.CuryUnbilledOrderTotal ?? 0;
				}
				if (applicationAmount < 0) applicationAmount = 0m;

				//If order is refunded or cancelled we still link payment with the order, so we pass 0 to AppliedToOrder.
				//Even openDoc is false, we removed the validation from the OrderToApply field  to supprot iit
				if (bucket.Order.Extern.StatusId == (int)OrderStatuses.Refunded)
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
		public override async Task SaveBucketImport(BCPaymentEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
			MappedPayment obj = bucket.Payment;
			Boolean needRelease = obj.Local.NeedRelease;
			Payment impl = (Payment)existing?.Local;

			if (existing?.IsNew != false)
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

			//Executing Actions
			using (var transaction = await base.WithTransaction(async () =>
			{
				string transactionNumber = GetHelper<BCHelper>().ParseTransactionNumber(obj.Extern, out bool isCreditCard);
				//If we import an inexistent credit card payment with a Capture Action, it is already created when we create payment, we don't need to do it again.
				bool shouldCapture = obj.Extern.Action.IsIn(OrderPaymentEvent.Capture, OrderPaymentEvent.Finalization)
					&& isCreditCard && existing?.IsNew == false && impl?.Status?.Value != PX.Objects.AR.Messages.Capture;
				//It is not needed to void a payment again if it has been already voided.
				bool shouldVoid = obj.Extern.Action == OrderPaymentEvent.Void
					&& impl?.Type.Value.IsNotIn(PX.Objects.AR.Messages.VoidPayment, PX.Objects.AR.Messages.Voided) == true;
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
			impl = cbapi.Invoke<Payment, CardOperation>(impl, action: new CardOperation()
		{
				TranType = CCTranTypeCode.PriorAuthorizedCapture.ValueField(),
				Amount = ((decimal)obj.Extern.Amount).ValueField(),
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
		/// Evaluates if <paramref name="needRelease"/> is true and <paramref name="payment"/> status is <see cref="PX.Objects.AR.Messages.Balanced"/>.
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
						Guid? localID = (payment.Type?.Value == PX.Objects.AR.Messages.VoidPayment) ? obj.LocalID: respPayment.SyncID;
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
			Payment impl = !string.IsNullOrEmpty(payment.ProcessingCenterID?.Value) ? cbapi.Invoke<Payment, VoidCardPayment>(payment, action: new VoidCardPayment()
			{
				TranType = CCTranTypeCode.VoidTran.ValueField(),
				TranNbr = (transactionNumber ?? GetHelper<BCHelper>().ParseTransactionNumber(obj.Extern, out bool isCreditCardTran)).ValueField()
			}) : cbapi.Invoke<Payment, VoidPayment>(null, payment.Id);
			obj.AddLocal(null, obj.LocalID, impl.SyncTime);

			return impl;
		}

		public virtual OrdersTransactionData CreateOrderTransactionData(OrderData orderData)
		{
			OrdersTransactionData data = new OrdersTransactionData();
			data.Id = orderData.Id.Value;
			data.OrderId = orderData.Id.ToString();
			data.Gateway = orderData.PaymentMethod;
			data.Currency = orderData.CurrencyCode;
			data.DateCreatedUT = orderData.DateCreatedUT;
			data.PaymentMethod = BCConstants.Emulated;
			data.Amount = Convert.ToDouble(orderData.TotalIncludingTax);
			return data;
		}
		#endregion

		#region Export
		public override async Task FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
		}
		public override  async Task<EntityStatus> GetBucketForExport(BCPaymentEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			Payment impl = cbapi.GetByID<Payment>(syncstatus.LocalID);
			if (impl == null) return EntityStatus.None;

			MappedPayment obj = bucket.Payment = bucket.Payment.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(bucket.Payment, SyncDirection.Export);

			return status;
		}

		public override async Task MapBucketExport(BCPaymentEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
		}
		public override async Task SaveBucketExport(BCPaymentEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
		}
		#endregion

	}
}
