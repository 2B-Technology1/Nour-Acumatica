import
{
	PXView, PXFieldState, commitChanges, headerDescription, ICurrencyInfo, disabled, selectorSettings, PXFieldOptions, linkCommand, columnSettings, GridColumnShowHideMode, GridColumnType, PXActionState
} from "client-controls";


export class SOOrderHeader extends PXView {
	OrderType: PXFieldState;
	OrderNbr: PXFieldState;
	Status: PXFieldState<PXFieldOptions.Disabled>;
	DontApprove: PXFieldState<PXFieldOptions.Disabled>;
	Approved: PXFieldState<PXFieldOptions.Disabled>;
	OrderDate: PXFieldState<PXFieldOptions.CommitChanges>;
	RequestDate: PXFieldState<PXFieldOptions.CommitChanges>;
	CustomerOrderNbr: PXFieldState<PXFieldOptions.CommitChanges>;
	CustomerRefNbr: PXFieldState;
	CuryInfoID: PXFieldState;

	@headerDescription CustomerID: PXFieldState<PXFieldOptions.CommitChanges>;
	CustomerLocationID: PXFieldState<PXFieldOptions.CommitChanges>;
	ContactID: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryID: PXFieldState<PXFieldOptions.CommitChanges>;
	DestinationSiteID: PXFieldState<PXFieldOptions.CommitChanges>;
	ProjectID: PXFieldState<PXFieldOptions.CommitChanges>;
	OrderDesc: PXFieldState;

	OrderQty: PXFieldState<PXFieldOptions.Disabled>;
	CuryDiscTot: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryVatExemptTotal: PXFieldState<PXFieldOptions.Disabled>;
	CuryVatTaxableTotal: PXFieldState<PXFieldOptions.Disabled>;
	CuryTaxTotal: PXFieldState<PXFieldOptions.Disabled>;
	CuryOrderTotal: PXFieldState<PXFieldOptions.Disabled>;
	CuryControlTotal: PXFieldState<PXFieldOptions.CommitChanges>;
	ArePaymentsApplicable: PXFieldState<PXFieldOptions.CommitChanges>;
	IsRUTROTDeductible: PXFieldState<PXFieldOptions.CommitChanges>;
	IsFSIntegrated: PXFieldState<PXFieldOptions.Disabled>;

	ShowDiscountsTab: PXFieldState;
	ShowShipmentsTab: PXFieldState;
	ShowOrdersTab: PXFieldState;
}

export class SOOrder extends PXView {
	BranchID: PXFieldState<PXFieldOptions.CommitChanges>;
	BranchBaseCuryID: PXFieldState;
	DisableAutomaticTaxCalculation: PXFieldState<PXFieldOptions.CommitChanges>;
	OverrideTaxZone: PXFieldState<PXFieldOptions.CommitChanges>;
	TaxZoneID: PXFieldState<PXFieldOptions.CommitChanges>;
	TaxCalcMode: PXFieldState<PXFieldOptions.CommitChanges>;
	ExternalTaxExemptionNumber: PXFieldState<PXFieldOptions.CommitChanges>;
	AvalaraCustomerUsageType: PXFieldState;
	BillSeparately: PXFieldState<PXFieldOptions.CommitChanges>;
	InvoiceNbr: PXFieldState;
	InvoiceDate: PXFieldState<PXFieldOptions.CommitChanges>;
	TermsID: PXFieldState<PXFieldOptions.CommitChanges>;
	DueDate: PXFieldState;
	DiscDate: PXFieldState;
	FinPeriodID: PXFieldState;

	OverridePrepayment: PXFieldState<PXFieldOptions.CommitChanges>;
	PrepaymentReqPct: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryPrepaymentReqAmt: PXFieldState<PXFieldOptions.CommitChanges>;
	PrepaymentReqSatisfied: PXFieldState;
	PaymentMethodID: PXFieldState<PXFieldOptions.CommitChanges>;
	PMInstanceID: PXFieldState<PXFieldOptions.CommitChanges>;
	CashAccountID: PXFieldState<PXFieldOptions.CommitChanges>;
	ExtRefNbr: PXFieldState<PXFieldOptions.CommitChanges>;

	WorkgroupID: PXFieldState<PXFieldOptions.CommitChanges>;
	OwnerID: PXFieldState<PXFieldOptions.CommitChanges>;

	OrigOrderType: PXFieldState<PXFieldOptions.Disabled>;
	OrigOrderNbr: PXFieldState<PXFieldOptions.Disabled>;
	Emailed: PXFieldState<PXFieldOptions.Disabled>;
	Printed: PXFieldState<PXFieldOptions.Disabled>;

	SalesPersonID: PXFieldState<PXFieldOptions.CommitChanges>;
	DisableAutomaticDiscountCalculation: PXFieldState<PXFieldOptions.CommitChanges>;

	CuryUnreleasedPaymentAmt: PXFieldState<PXFieldOptions.Disabled>;
	CuryCCAuthorizedAmt: PXFieldState<PXFieldOptions.Disabled>;
	CuryPaidAmt: PXFieldState<PXFieldOptions.Disabled>;
	CuryPaymentTotal: PXFieldState<PXFieldOptions.Disabled>;
	CuryBilledPaymentTotal: PXFieldState<PXFieldOptions.Disabled>;
	CuryTransferredToChildrenPaymentTotal: PXFieldState<PXFieldOptions.Disabled>;
	CuryUnpaidBalance: PXFieldState<PXFieldOptions.Disabled>;
	CuryUnbilledOrderTotal: PXFieldState<PXFieldOptions.Disabled>;
	RiskStatus: PXFieldState<PXFieldOptions.Disabled>;

	ShipVia: PXFieldState<PXFieldOptions.CommitChanges>;
	WillCall: PXFieldState;
	DeliveryConfirmation: PXFieldState<PXFieldOptions.CommitChanges>;
	EndorsementService: PXFieldState<PXFieldOptions.CommitChanges>;
	FreightClass: PXFieldState;
	FOBPoint: PXFieldState;
	Priority: PXFieldState;
	ShipTermsID: PXFieldState<PXFieldOptions.CommitChanges>;
	ShipZoneID: PXFieldState<PXFieldOptions.CommitChanges>;
	Resedential: PXFieldState;
	SaturdayDelivery: PXFieldState;
	Insurance: PXFieldState;
	UseCustomerAccount: PXFieldState<PXFieldOptions.CommitChanges>;
	GroundCollect: PXFieldState<PXFieldOptions.CommitChanges>;
	IntercompanyPOType: PXFieldState;
	IntercompanyPONbr: PXFieldState<PXFieldOptions.Disabled>;
	IntercompanyPOReturnNbr: PXFieldState<PXFieldOptions.Disabled>;
	ShipDate: PXFieldState<PXFieldOptions.CommitChanges>;
	ShipSeparately: PXFieldState;
	ShipComplete: PXFieldState<PXFieldOptions.CommitChanges>;
	CancelDate: PXFieldState;
	Cancelled: PXFieldState<PXFieldOptions.Disabled>;
	DefaultSiteID: PXFieldState<PXFieldOptions.CommitChanges>;

	OrderWeight: PXFieldState<PXFieldOptions.Disabled>;
	OrderVolume: PXFieldState<PXFieldOptions.Disabled>;
	PackageWeight: PXFieldState<PXFieldOptions.Disabled>;
	CuryFreightCost: PXFieldState<PXFieldOptions.CommitChanges>;
	FreightCostIsValid: PXFieldState;
	OverrideFreightAmount: PXFieldState<PXFieldOptions.CommitChanges>;
	FreightAmountSource: PXFieldState;
	CuryFreightAmt: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryPremiumFreightAmt: PXFieldState<PXFieldOptions.CommitChanges>;
	FreightTaxCategoryID: PXFieldState<PXFieldOptions.CommitChanges>;

	AMCuryEstimateTotal: PXFieldState<PXFieldOptions.Disabled>;
	CuryLineTotal: PXFieldState<PXFieldOptions.Disabled>;
	CuryTaxTotal: PXFieldState<PXFieldOptions.Disabled>;
	CuryMiscTot: PXFieldState<PXFieldOptions.Disabled>;

	AMEstimateQty: PXFieldState<PXFieldOptions.Disabled>;
	BlanketOpenQty: PXFieldState<PXFieldOptions.Disabled>;
	OpenOrderQty: PXFieldState<PXFieldOptions.Disabled>;
	CuryOpenOrderTotal: PXFieldState<PXFieldOptions.Disabled>;
	UnbilledOrderQty: PXFieldState<PXFieldOptions.Disabled>;
}

export class SOLine extends PXView {
	AddInvBySite: PXActionState;
	ShowMatrixPanel: PXActionState;
	AddInvoice: PXActionState;
	AddBlanketLine: PXActionState;
	SOOrderLineSplittingExtension_ShowSplits: PXActionState;
	POSupplyOK: PXActionState;
	ItemAvailability: PXActionState;
	ConfigureEntry: PXActionState;
	linkProdOrder: PXActionState;

	Availability: PXFieldState<PXFieldOptions.Hidden>;
	@columnSettings({ allowShowHide: GridColumnShowHideMode.Server }) ExcludedFromExport: PXFieldState;
	IsConfigurable: PXFieldState;
	@columnSettings({ hideViewLink: true })
	BranchID: PXFieldState<PXFieldOptions.CommitChanges>;
	OrderType: PXFieldState;
	OrderNbr: PXFieldState;
	LineNbr: PXFieldState;
	AssociatedOrderLineNbr: PXFieldState;
	GiftMessage: PXFieldState;
	SortOrder: PXFieldState;
	LineType: PXFieldState;
	@columnSettings({ allowShowHide: GridColumnShowHideMode.Server }) InvoiceNbr: PXFieldState;
	@columnSettings({ allowShowHide: GridColumnShowHideMode.Server }) Operation: PXFieldState<PXFieldOptions.CommitChanges>;
	InventoryID: PXFieldState<PXFieldOptions.CommitChanges>;
	@columnSettings({
		type: GridColumnType.Icon,
		allowShowHide: GridColumnShowHideMode.Server,
		allowFilter: false,
		allowSort: false
	})
	@linkCommand("AddRelatedItems")
	RelatedItems: PXFieldState;
	@columnSettings({ allowShowHide: GridColumnShowHideMode.Server }) SubstitutionRequired: PXFieldState;
	IsSpecialOrder: PXFieldState<PXFieldOptions.CommitChanges>;
	EquipmentAction: PXFieldState<PXFieldOptions.CommitChanges>;
	Comment: PXFieldState;
	SMEquipmentID: PXFieldState<PXFieldOptions.CommitChanges>;
	NewEquipmentLineNbr: PXFieldState<PXFieldOptions.CommitChanges>;
	ComponentID: PXFieldState<PXFieldOptions.CommitChanges>;
	EquipmentComponentLineNbr: PXFieldState<PXFieldOptions.CommitChanges>;
	@linkCommand("SOLine$RelatedDocument$Link") RelatedDocument: PXFieldState;
	SDSelected: PXFieldState<PXFieldOptions.CommitChanges>;
	SubItemID: PXFieldState<PXFieldOptions.CommitChanges>;
	@columnSettings({ allowShowHide: GridColumnShowHideMode.Server }) AutoCreateIssueLine: PXFieldState;
	IsFree: PXFieldState;
	@columnSettings({ hideViewLink: true }) SiteID: PXFieldState<PXFieldOptions.CommitChanges>;
	@columnSettings({ allowShowHide: GridColumnShowHideMode.Server }) LocationID: PXFieldState;
	TranDesc: PXFieldState;
	@columnSettings({ hideViewLink: true }) UOM: PXFieldState<PXFieldOptions.CommitChanges>;
	OrderQty: PXFieldState<PXFieldOptions.CommitChanges>;
	BaseOrderQty: PXFieldState;
	QtyOnOrders: PXFieldState;
	BlanketOpenQty: PXFieldState;
	UnshippedQty: PXFieldState;
	ShippedQty: PXFieldState<PXFieldOptions.Disabled>;
	OpenQty: PXFieldState<PXFieldOptions.Disabled>;
	CuryUnitCost: PXFieldState;
	CuryUnitPrice: PXFieldState<PXFieldOptions.CommitChanges>;
	ManualPrice: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryExtPrice: PXFieldState<PXFieldOptions.CommitChanges>;
	DiscPct: PXFieldState;
	CuryDiscAmt: PXFieldState;
	DiscountID: PXFieldState<PXFieldOptions.CommitChanges>;
	DiscountSequenceID: PXFieldState;
	ManualDisc: PXFieldState<PXFieldOptions.CommitChanges>;
	AutomaticDiscountsDisabled: PXFieldState;
	CuryDiscPrice: PXFieldState;
	@columnSettings({ nullText: "0.0" }) AvgCost: PXFieldState;
	CuryLineAmt: PXFieldState;
	SchedOrderDate: PXFieldState<PXFieldOptions.CommitChanges>;
	CustomerOrderNbr: PXFieldState<PXFieldOptions.CommitChanges>;
	@columnSettings({ hideViewLink: true })CustomerLocationID: PXFieldState<PXFieldOptions.CommitChanges>;
	CustomerLocationID_Location_descr: PXFieldState;
	ShipVia: PXFieldState<PXFieldOptions.CommitChanges>;
	FOBPoint: PXFieldState;
	ShipTermsID: PXFieldState;
	ShipZoneID: PXFieldState;
	SchedShipDate: PXFieldState<PXFieldOptions.CommitChanges>;
	TaxZoneID: PXFieldState<PXFieldOptions.CommitChanges>;
	DRTermStartDate: PXFieldState<PXFieldOptions.CommitChanges>;
	DRTermEndDate: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryUnbilledAmt: PXFieldState<PXFieldOptions.Disabled>;
	RequestDate: PXFieldState;
	ShipDate: PXFieldState;
	ShipComplete: PXFieldState;
	CompleteQtyMin: PXFieldState;
	CompleteQtyMax: PXFieldState;
	Completed: PXFieldState<PXFieldOptions.CommitChanges>;
	POCreate: PXFieldState<PXFieldOptions.CommitChanges>;
	IsPOLinkAllowed: PXFieldState;
	POSource: PXFieldState<PXFieldOptions.CommitChanges>;
	POCreateDate: PXFieldState<PXFieldOptions.CommitChanges>;
	POOrderNbr: PXFieldState;
	POOrderStatus: PXFieldState;
	POLineNbr: PXFieldState;
	POLinkActive: PXFieldState<PXFieldOptions.CommitChanges>;
	@columnSettings({ allowShowHide: GridColumnShowHideMode.Server }) LotSerialNbr: PXFieldState<PXFieldOptions.CommitChanges>;
	@columnSettings({ allowShowHide: GridColumnShowHideMode.Server }) ExpireDate: PXFieldState;
	ReasonCode: PXFieldState<PXFieldOptions.CommitChanges>;
	@columnSettings({ hideViewLink: true }) SalesPersonID: PXFieldState;
	@columnSettings({ hideViewLink: true }) TaxCategoryID: PXFieldState;
	AvalaraCustomerUsageType: PXFieldState;
	Commissionable: PXFieldState<PXFieldOptions.CommitChanges>;
	BlanketNbr: PXFieldState;
	AlternateID: PXFieldState;
	@columnSettings({ hideViewLink: true })SalesAcctID: PXFieldState<PXFieldOptions.CommitChanges>;
	@columnSettings({ hideViewLink: true })SalesSubID: PXFieldState;
	TaskID: PXFieldState<PXFieldOptions.CommitChanges>;
	CostCodeID: PXFieldState;
	@columnSettings({ allowShowHide: GridColumnShowHideMode.Server }) CuryUnitPriceDR: PXFieldState;
	@columnSettings({ allowShowHide: GridColumnShowHideMode.Server }) DiscPctDR: PXFieldState;
	IsRUTROTDeductible: PXFieldState<PXFieldOptions.CommitChanges>;
	RUTROTItemType: PXFieldState<PXFieldOptions.CommitChanges>;
	RUTROTWorkTypeID: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryRUTROTAvailableAmt: PXFieldState;
	AMProdCreate: PXFieldState<PXFieldOptions.CommitChanges>;
	AMorderType: PXFieldState;
	AMProdOrdID: PXFieldState;
	AMEstimateID: PXFieldState;
	AMEstimateRevisionID: PXFieldState;
	AMParentLineNbr: PXFieldState;
	AMIsSupplemental: PXFieldState;
	AMConfigKeyID: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class LineSplittingHeader extends PXView {
	UnassignedQty: PXFieldState<PXFieldOptions.Disabled>;
	Qty: PXFieldState<PXFieldOptions.CommitChanges>;
	StartNumVal: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class SOLineSplit extends PXView {
	@columnSettings({ hideViewLink: true }) SplitLineNbr: PXFieldState;
	ParentSplitLineNbr: PXFieldState;
	InventoryID: PXFieldState;
	SubItemID: PXFieldState;
	SchedOrderDate: PXFieldState;
	SchedShipDate: PXFieldState;
	CustomerOrderNbr: PXFieldState;
	ShipDate: PXFieldState;
	IsAllocated: PXFieldState<PXFieldOptions.CommitChanges>;
	SiteID: PXFieldState;
	Completed: PXFieldState;
	@columnSettings({ hideViewLink: true }) LocationID: PXFieldState;
	@columnSettings({ hideViewLink: true }) LotSerialNbr: PXFieldState;
	Qty: PXFieldState<PXFieldOptions.CommitChanges>;
	QtyOnOrders: PXFieldState;
	ShippedQty: PXFieldState;
	ReceivedQty: PXFieldState;
	BlanketOpenQty: PXFieldState;
	@columnSettings({ hideViewLink: true }) UOM: PXFieldState;
	ExpireDate: PXFieldState;
	POCreate: PXFieldState<PXFieldOptions.CommitChanges>;
	POCreateDate: PXFieldState<PXFieldOptions.CommitChanges>;
	@linkCommand("SOLineSplit$RefNoteID$Link") RefNoteID: PXFieldState;
}

export class SOShippingAddress extends PXView {
	AddressID: PXFieldState;
	AddressLine1: PXFieldState;
	AddressLine2: PXFieldState;
	AddressLine3: PXFieldState;
	City: PXFieldState;
	CountryID: PXFieldState<PXFieldOptions.CommitChanges>;
	State: PXFieldState<PXFieldOptions.CommitChanges>;
	PostalCode: PXFieldState<PXFieldOptions.CommitChanges>;
	Latitude: PXFieldState;
	Longitude: PXFieldState;
	OverrideAddress: PXFieldState<PXFieldOptions.CommitChanges>;
	IsValidated: PXFieldState<PXFieldOptions.Disabled>;
}

export class SOShippingContact extends PXView {
	ContactID: PXFieldState;
	OverrideContact: PXFieldState<PXFieldOptions.CommitChanges>;
	FullName: PXFieldState;
	Attention: PXFieldState;
	Phone1: PXFieldState;
	Email: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class SOBillingAddress extends PXView {
	AddressID: PXFieldState;
	AddressLine1: PXFieldState;
	AddressLine2: PXFieldState;
	AddressLine3: PXFieldState;
	City: PXFieldState;
	CountryID: PXFieldState<PXFieldOptions.CommitChanges>;
	State: PXFieldState<PXFieldOptions.CommitChanges>;
	PostalCode: PXFieldState<PXFieldOptions.CommitChanges>;
	Latitude: PXFieldState;
	Longitude: PXFieldState;
	OverrideAddress: PXFieldState<PXFieldOptions.CommitChanges>;
	IsValidated: PXFieldState<PXFieldOptions.Disabled>;
}

export class SOBillingContact extends PXView {
	ContactID: PXFieldState;
	OverrideContact: PXFieldState<PXFieldOptions.CommitChanges>;
	FullName: PXFieldState;
	Attention: PXFieldState;
	Phone1: PXFieldState;
	Email: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class SOTaxTran extends PXView {
	@columnSettings({allowUpdate: false}) TaxZoneID: PXFieldState<PXFieldOptions.CommitChanges>;
	@columnSettings({allowUpdate: false}) TaxID: PXFieldState<PXFieldOptions.CommitChanges>;
	@columnSettings({allowUpdate: false}) TaxRate: PXFieldState<PXFieldOptions.Disabled>;
	CuryTaxableAmt: PXFieldState;
	CuryExemptedAmt: PXFieldState;
	TaxUOM: PXFieldState;

	TaxableQty: PXFieldState;
	CuryTaxAmt: PXFieldState;
	Tax__TaxType: PXFieldState;
	Tax__PendingTax: PXFieldState;
	Tax__ReverseTax: PXFieldState;

	Tax__ExemptTax: PXFieldState;
	Tax__StatisticalTax: PXFieldState;
}

export class SOSalesPerTran extends PXView {
	@columnSettings({hideViewLink: true}) SalespersonID: PXFieldState<PXFieldOptions.CommitChanges>;
	CommnPct: PXFieldState;
	@columnSettings({allowUpdate: false}) CuryCommnAmt: PXFieldState;
	@columnSettings({allowUpdate: false}) CuryCommnblAmt: PXFieldState;
}

export class SOOrderShipment extends PXView {
	ShipmentType: PXFieldState;
	@columnSettings({allowUpdate: false}) ShipmentNbr: PXFieldState;
	@linkCommand("SOOrderShipment~DisplayShippingRefNoteID~Link") DisplayShippingRefNoteID: PXFieldState;
	SOShipment__StatusIsNull: PXFieldState;
	@columnSettings({allowUpdate: false}) Operation: PXFieldState;
	@columnSettings({allowUpdate: false}) OrderType: PXFieldState<PXFieldOptions.Disabled>;
	@columnSettings({allowUpdate: false}) OrderNbr: PXFieldState<PXFieldOptions.Disabled>;
	ShipDate: PXFieldState;
	ShipmentQty: PXFieldState;
	ShipmentWeight: PXFieldState;
	ShipmentVolume: PXFieldState;
	@columnSettings({allowUpdate: false}) InvoiceType: PXFieldState;
	@columnSettings({allowUpdate: false}) InvoiceNbr: PXFieldState;
	@columnSettings({allowUpdate: false}) InvtDocType: PXFieldState;
	@columnSettings({allowUpdate: false}) InvtRefNbr: PXFieldState;
}

export class SOBlanketOrderDisplayLink extends PXView {
	@columnSettings({hideViewLink: true}) CustomerLocationID: PXFieldState;
	@columnSettings({allowUpdate: false}) @linkCommand("ViewChildOrder") OrderNbr: PXFieldState;
	@columnSettings({allowUpdate: false}) OrderDate: PXFieldState;
	OrderStatus: PXFieldState;
	OrderedQty: PXFieldState;
	CuryOrderedAmt: PXFieldState;
	ShipmentType: PXFieldState;
	@columnSettings({allowUpdate: false}) ShipmentNbr: PXFieldState;
	@linkCommand("SOBlanketOrderDisplayLink~DisplayShippingRefNoteID~Link") DisplayShippingRefNoteID: PXFieldState;
	@columnSettings({allowUpdate: false}) ShipmentDate: PXFieldState;
	ShipmentStatus: PXFieldState;
	@columnSettings({allowUpdate: false}) ShippedQty: PXFieldState;
	@columnSettings({allowUpdate: false}) InvoiceType: PXFieldState;
	@columnSettings({allowUpdate: false}) InvoiceNbr: PXFieldState;
	@columnSettings({allowUpdate: false}) InvoiceDate: PXFieldState;
	InvoiceStatus: PXFieldState;
}

export class OpenBlanketSOLineSplit extends PXView {
	@columnSettings({allowCheckAll: true}) Selected: PXFieldState;
	@columnSettings({hideViewLink: true}) OrderType: PXFieldState<PXFieldOptions.Disabled>;
	@columnSettings({hideViewLink: true}) OrderNbr: PXFieldState<PXFieldOptions.Disabled>;
	SchedOrderDate: PXFieldState;
	@columnSettings({hideViewLink: true}) InventoryID: PXFieldState;
	@columnSettings({hideViewLink: true}) SubItemID: PXFieldState;
	TranDesc: PXFieldState;
	@columnSettings({hideViewLink: true}) SiteID: PXFieldState;
	CustomerOrderNbr: PXFieldState;
	@columnSettings({hideViewLink: true}) UOM: PXFieldState;
	BlanketOpenQty: PXFieldState;
	@columnSettings({hideViewLink: true}) CustomerLocationID: PXFieldState;
	@columnSettings({hideViewLink: true}) TaxZoneID: PXFieldState;
}

export class SOAdjustments extends PXView {
	CreateDocumentPayment: PXActionState;
	CreateOrderPrepayment: PXActionState;
	CaptureDocumentPayment: PXActionState;
	VoidDocumentPayment: PXActionState;
	ImportDocumentPayment: PXActionState;
	CreateDocumentRefund: PXActionState;

	AdjgDocType: PXFieldState;
	@linkCommand("ViewPayment") AdjgRefNbr: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryAdjdAmt: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryAdjdBilledAmt: PXFieldState;
	CuryAdjdTransferredToChildrenAmt: PXFieldState;
	@columnSettings({allowUpdate: false}) CuryDocBal: PXFieldState<PXFieldOptions.Disabled>;
	@columnSettings({allowUpdate: false}) ARPayment__Status: PXFieldState<PXFieldOptions.Disabled>;
	ExtRefNbr: PXFieldState;
	@columnSettings({hideViewLink: true, allowUpdate: false}) PaymentMethodID: PXFieldState<PXFieldOptions.Disabled>;
	@columnSettings({hideViewLink: true}) CashAccountID: PXFieldState;
	CuryOrigDocAmt: PXFieldState;
	@columnSettings({hideViewLink: true}) ARPayment__CuryID: PXFieldState;
	ExternalTransaction__ProcStatus: PXFieldState;
	CanVoid: PXFieldState;
	CanCapture: PXFieldState;
}

export class CurrencyInfo extends PXView implements ICurrencyInfo {
	CuryInfoID: PXFieldState;
	BaseCuryID: PXFieldState;
	BaseCalc: PXFieldState;
	CuryID: PXFieldState<PXFieldOptions.CommitChanges>;
	DisplayCuryID: PXFieldState;
	CuryRateTypeID: PXFieldState<PXFieldOptions.CommitChanges>;
	BasePrecision: PXFieldState;
	CuryRate: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryEffDate: PXFieldState<PXFieldOptions.CommitChanges>;
	RecipRate: PXFieldState<PXFieldOptions.CommitChanges>;
	SampleCuryRate: PXFieldState<PXFieldOptions.CommitChanges>;
	SampleRecipRate: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class SOParamFilter extends PXView {
	ShipDate: PXFieldState<PXFieldOptions.CommitChanges>;
	@selectorSettings("INSite__SiteCD", "INSite__descr") SiteID: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class RecalcDiscountsFilter extends PXView {
	RecalcTarget: PXFieldState;
	RecalcUnitPrices: PXFieldState<PXFieldOptions.CommitChanges>;
	OverrideManualPrices: PXFieldState<PXFieldOptions.CommitChanges>;
	RecalcDiscounts: PXFieldState<PXFieldOptions.CommitChanges>;
	OverrideManualDiscounts: PXFieldState<PXFieldOptions.CommitChanges>;
	OverrideManualDocGroupDiscounts: PXFieldState<PXFieldOptions.CommitChanges>;
	CalcDiscountsOnLinesWithDisabledAutomaticDiscounts: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class CopyParamFilter extends PXView {
	OrderType: PXFieldState<PXFieldOptions.CommitChanges>;
	OrderNbr: PXFieldState<PXFieldOptions.CommitChanges>;
	RecalcUnitPrices: PXFieldState<PXFieldOptions.CommitChanges>;
	OverrideManualPrices: PXFieldState<PXFieldOptions.CommitChanges>;
	RecalcDiscounts: PXFieldState<PXFieldOptions.CommitChanges>;
	OverrideManualDiscounts: PXFieldState<PXFieldOptions.CommitChanges>;
	AMIncludeEstimate: PXFieldState<PXFieldOptions.CommitChanges>;
	CopyConfigurations: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class SOLinePOLink extends PXView {
	POSource: PXFieldState<PXFieldOptions.CommitChanges>;
	VendorID: PXFieldState<PXFieldOptions.CommitChanges>;
	POSiteID: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class LinkedPOLines extends PXView {
	Selected: PXFieldState;
	OrderType: PXFieldState;
	OrderNbr: PXFieldState;
	VendorRefNbr: PXFieldState;
	LineType: PXFieldState;
	@columnSettings({hideViewLink: true}) InventoryID: PXFieldState;
	@columnSettings({hideViewLink: true}) SubItemID: PXFieldState;
	VendorID: PXFieldState;
	VendorID_Vendor_AcctName: PXFieldState;
	PromisedDate: PXFieldState;
	@columnSettings({hideViewLink: true}) UOM: PXFieldState;
	OrderQty: PXFieldState;
	OpenQty: PXFieldState;
	TranDesc: PXFieldState;
}

export class AddInvoiceHeader extends PXView {
	DocType: PXFieldState<PXFieldOptions.CommitChanges>;
	RefNbr: PXFieldState<PXFieldOptions.CommitChanges>;
	Expand: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class AddInvoiceDetails extends PXView {
	@columnSettings({allowCheckAll: true}) Selected: PXFieldState;
	@columnSettings({hideViewLink: true}) InventoryID: PXFieldState;
	@columnSettings({hideViewLink: true}) SubItemID: PXFieldState;
	@columnSettings({hideViewLink: true}) SiteID: PXFieldState;
	@columnSettings({hideViewLink: true}) LocationID: PXFieldState;
	@columnSettings({hideViewLink: true}) LotSerialNbr: PXFieldState;
	@columnSettings({hideViewLink: true}) UOM: PXFieldState;
	Qty: PXFieldState;
	TranDesc: PXFieldState;
	DropShip: PXFieldState;
}

export class SOQuickPayment extends PXView {
	CuryOrigDocAmt: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryRefundAmt: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryID: PXFieldState<PXFieldOptions.CommitChanges>;
	DocDesc: PXFieldState;
	PaymentMethodID: PXFieldState<PXFieldOptions.CommitChanges>;
	RefTranExtNbr: PXFieldState<PXFieldOptions.CommitChanges>;
	NewCard: PXFieldState<PXFieldOptions.CommitChanges>;
	NewAccount: PXFieldState<PXFieldOptions.CommitChanges>;
	SaveCard: PXFieldState<PXFieldOptions.CommitChanges>;
	SaveAccount: PXFieldState<PXFieldOptions.CommitChanges>;
	PMInstanceID: PXFieldState;
	CashAccountID: PXFieldState<PXFieldOptions.CommitChanges>;
	ProcessingCenterID: PXFieldState<PXFieldOptions.CommitChanges>;
	ExtRefNbr: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class Relations extends PXView {
	Role: PXFieldState<PXFieldOptions.CommitChanges>;
	IsPrimary: PXFieldState<PXFieldOptions.CommitChanges>;
	TargetType: PXFieldState<PXFieldOptions.CommitChanges>;
	@linkCommand("RelationsViewTargetDetails") TargetNoteID: PXFieldState<PXFieldOptions.CommitChanges>;
	@columnSettings({allowUpdate: false}) @linkCommand("RelationsViewEntityDetails") EntityID: PXFieldState<PXFieldOptions.CommitChanges>;
	Name: PXFieldState;
	@columnSettings({allowUpdate: false}) @linkCommand("RelationsViewContactDetails") ContactID: PXFieldState;
	Email: PXFieldState;
	AddToCC: PXFieldState<PXFieldOptions.CommitChanges>;
	CreatedDateTime: PXFieldState<PXFieldOptions.Hidden>;
	@columnSettings({hideViewLink: true}) CreatedByID: PXFieldState<PXFieldOptions.Hidden>;
	@columnSettings({hideViewLink: true}) LastModifiedByID: PXFieldState<PXFieldOptions.Hidden>;
}

export class ShopForRatesHeader extends PXView {
	OrderWeight: PXFieldState;
	PackageWeight: PXFieldState;
	IsManualPackage: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class CarrierRates extends PXView {
	RefreshRates: PXActionState;
	Selected: PXFieldState<PXFieldOptions.CommitChanges>;
	Method: PXFieldState;
	Description: PXFieldState;
	Amount: PXFieldState;
	DaysInTransit: PXFieldState;
	DeliveryDate: PXFieldState;
}

export class Packages extends PXView {
	RecalculatePackages: PXActionState;
	@columnSettings({hideViewLink: true}) BoxID: PXFieldState<PXFieldOptions.CommitChanges>;
	Description: PXFieldState;
	@columnSettings({hideViewLink: true}) SiteID: PXFieldState;
	Length: PXFieldState;
	Width: PXFieldState;
	Height: PXFieldState;
	LinearUOM: PXFieldState;
	WeightUOM: PXFieldState;
	Weight: PXFieldState;
	BoxWeight: PXFieldState;
	GrossWeight: PXFieldState;
	DeclaredValue: PXFieldState;
	COD: PXFieldState;
	StampsAddOns: PXFieldState;
}

