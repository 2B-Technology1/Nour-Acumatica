import
{
	createCollection,
	createSingle,
	PXScreen,
	graphInfo,
	PXActionState,
	viewInfo
} from "client-controls";
import { Messages as SysMessages } from 'client-controls/services/messages';
import
{
	SOOrder,
	SOLine,
	SOLineSplit,
	SOTaxTran,
	SOSalesPerTran,
	SOAdjustments,
	CurrencyInfo,
	SOParamFilter,
	SOShippingContact,
	SOShippingAddress,
	SOBillingContact,
	SOBillingAddress,
	SOOrderShipment,
	SOBlanketOrderDisplayLink,
	OpenBlanketSOLineSplit,
	LineSplittingHeader,
	RecalcDiscountsFilter,
	SOLinePOLink,
	LinkedPOLines,
	AddInvoiceHeader,
	AddInvoiceDetails,
	CopyParamFilter,
	SOQuickPayment,
	Relations,
	ShopForRatesHeader,
	CarrierRates,
	Packages,
	SOOrderHeader
} from './views';


@graphInfo({graphType: 'PX.Objects.SO.SOOrderEntry', primaryView: 'Document', bpEventsIndicator: true, udfTypeField: 'OrderType'})
export class SO301000 extends PXScreen {
	SysMessages = SysMessages;

	AddInvoiceOK: PXActionState;

	OverrideBlanketTaxZone: PXActionState;
	AddRelatedItems: PXActionState;
	PasteLine: PXActionState;
	ShopRates: PXActionState;
	ShippingAddressLookup: PXActionState;
	AddressLookup: PXActionState;
	ViewChildOrder: PXActionState;
	ViewPayment: PXActionState;

	CreatePaymentRefund: PXActionState;
	CreatePaymentCapture: PXActionState;
	CreatePaymentAuthorize: PXActionState;
	DeletePayment: PXActionState;
	DeleteRefund: PXActionState;
	CreatePaymentOK: PXActionState;
	CalculateFreight: PXActionState;
	SOOrderLineSplittingExtension_GenerateNumbers: PXActionState;
	AddInvSelBySite: PXActionState;
	AddBlanketLineOK: PXActionState;
	CheckCopyParams: PXActionState;

	RecalcOk: PXActionState;

	@viewInfo({containerName: "Order Summary"})
	Document = createSingle(SOOrderHeader);

	@viewInfo({containerName: "Details"})
	Transactions = createCollection(
		SOLine,
		{
			initNewRow: true,
			syncPosition: true,
			wrapToolbar: true
		});

	@viewInfo({containerName: "Line Details"})
	SOOrderLineSplittingExtension_LotSerOptions = createSingle(LineSplittingHeader);

	@viewInfo({containerName: "Line Details"})
	splits = createCollection(SOLineSplit, {
		syncPosition: true,
		adjustPageSize: true
	});

	@viewInfo({containerName: "Taxes"})
	Taxes = createCollection(SOTaxTran);

	@viewInfo({containerName: "Commissions"})
	SalesPerTran = createCollection(
		SOSalesPerTran,
		{
			allowInsert: false,
			allowDelete: false,
		}
	);

	@viewInfo({containerName: "Commissions"})
	CurrentDocument = createSingle(SOOrder);

	@viewInfo({containerName: "Ship-To Contact"})
	Shipping_Contact = createSingle(SOShippingContact);
	@viewInfo({containerName: "Ship-To Address"})
	Shipping_Address = createSingle(SOShippingAddress);
	@viewInfo({containerName: "Bill-To Contact"})
	Billing_Contact = createSingle(SOBillingContact);
	@viewInfo({containerName: "Bill-To Address"})
	Billing_Address = createSingle(SOBillingAddress);

	@viewInfo({containerName: "Shipments"})
	shipmentlist = createCollection(
		SOOrderShipment,
		{
			syncPosition: true,
			allowInsert: false,
			allowUpdate: false,
			allowDelete: false
		}
	);

	@viewInfo({containerName: "Child Orders"})
	BlanketOrderChildrenDisplayList = createCollection(
		SOBlanketOrderDisplayLink,
		{
			syncPosition: true,
			adjustPageSize: true,
			allowInsert: false,
			allowDelete: false,
			allowUpdate: false
		}
	);

	@viewInfo({containerName: "Add Blanket Sales Order Line"})
	BlanketSplits = createCollection(
		OpenBlanketSOLineSplit,
		{
			syncPosition: true,
			adjustPageSize: true,
			allowInsert: false,
			allowDelete: false
		}
	);

	@viewInfo({containerName: "Payments"})
	Adjustments = createCollection(
		SOAdjustments,
		{
			syncPosition: true,
			initNewRow: true,
			adjustPageSize: true,
			wrapToolbar: true
		}
	);

	@viewInfo({containerName: "Relations"})
	Relations = createCollection(
		Relations,
		{
			syncPosition: true,
			initNewRow: true,
			adjustPageSize: true
		}
	);

	@viewInfo({containerName: "Process Order"})
	CurrencyInfo = createSingle(CurrencyInfo);

	@viewInfo({containerName: "Purchasing Details"})
	SOLineDemand = createSingle(SOLinePOLink);

	@viewInfo({containerName: "Purchasing Details"})
	SupplyPOLines = createCollection(
		LinkedPOLines,
		{
			syncPosition: true,
			adjustPageSize: true,
			allowInsert: false,
			allowDelete: false,
		}
	);

	@viewInfo({containerName: "Add Invoice Details"})
	addinvoicefilter = createSingle(AddInvoiceHeader);

	@viewInfo({containerName: "Add Invoice Details"})
	invoicesplits = createCollection(
		AddInvoiceDetails,
		{
			syncPosition: true,
			adjustPageSize: true,
			allowInsert: false,
			allowDelete: false,
		}
	);

	@viewInfo({containerName: "Specify Shipment Parameters"})
	soparamfilter = createSingle(SOParamFilter);

	@viewInfo({containerName: "Recalculate Prices"})
	recalcdiscountsfilter = createSingle(RecalcDiscountsFilter);

	@viewInfo({containerName: "Copy To"})
	copyparamfilter = createSingle(CopyParamFilter);

	//Header = createSingle(EntryHeader); // TODO: Must be placed in feature extension

	@viewInfo({containerName: "Create Payment"})
	QuickPayment = createSingle(SOQuickPayment);

	@viewInfo({containerName: "Shop For Rates"})
	DocumentProperties = createSingle(ShopForRatesHeader);

	@viewInfo({containerName: "Carrier Rates"})
	CarrierRates = createCollection(
		CarrierRates,
		{
			syncPosition: true,
			allowInsert: false,
			allowDelete: false
		}
	);

	@viewInfo({containerName: "Packages"})
	Packages = createCollection(
		Packages,
		{
			syncPosition: true,
			adjustPageSize: true
		}
	);
}
