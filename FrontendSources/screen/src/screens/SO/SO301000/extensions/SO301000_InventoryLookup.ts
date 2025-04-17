import {
	SO301000
} from '../SO301000';

import {
	PXView,
	createCollection,
	createSingle,
	PXFieldState,
	PXFieldOptions,
	viewInfo,
	columnSettings,
	localizable
} from 'client-controls';

@localizable
export class InventoryLookupPanelHeaders {
	static InventoryLookup = "Inventory Lookup";
}

export interface SO301000_InventoryLookup extends SO301000 {}
export class SO301000_InventoryLookup {
	InventoryLookupPanelHeaders = InventoryLookupPanelHeaders;

	@viewInfo({containerName: "Inventory Lookup"})
	sitestatusfilter = createSingle(SOSiteStatusFilter);

	@viewInfo({containerName: "Inventory Lookup"})
	sitestatus = createCollection(SOSiteStatusSelected, {
		syncPosition: true,
		adjustPageSize: true,
		allowInsert: false,
		allowDelete: false
	});
}

export class SOSiteStatusFilter extends PXView {
	Inventory: PXFieldState<PXFieldOptions.CommitChanges>;
	BarCode: PXFieldState<PXFieldOptions.CommitChanges>;
	SiteID: PXFieldState<PXFieldOptions.CommitChanges>;
	ItemClass: PXFieldState<PXFieldOptions.CommitChanges>;
	SubItem: PXFieldState<PXFieldOptions.CommitChanges>;

	Mode: PXFieldState<PXFieldOptions.CommitChanges>;
	HistoryDate: PXFieldState<PXFieldOptions.CommitChanges>;
	OnlyAvailable: PXFieldState<PXFieldOptions.CommitChanges>;
	DropShipSales: PXFieldState<PXFieldOptions.CommitChanges>;

	CustomerLocationID: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class SOSiteStatusSelected extends PXView {
	@columnSettings({allowCheckAll: true}) Selected: PXFieldState;
	QtySelected: PXFieldState;
	@columnSettings({hideViewLink: true}) SiteID: PXFieldState;
	@columnSettings({hideViewLink: true}) ItemClassID: PXFieldState;
	ItemClassDescription: PXFieldState;
	@columnSettings({hideViewLink: true}) PriceClassID: PXFieldState;
	PriceClassDescription: PXFieldState;
	@columnSettings({hideViewLink: true}) PreferredVendorID: PXFieldState;
	PreferredVendorDescription: PXFieldState;
	@columnSettings({hideViewLink: true}) InventoryCD: PXFieldState;
	@columnSettings({hideViewLink: true}) SubItemID: PXFieldState;
	Descr: PXFieldState;
	@columnSettings({hideViewLink: true}) SalesUnit: PXFieldState;
	QtyAvailSale: PXFieldState;
	QtyOnHandSale: PXFieldState;
	CuryID: PXFieldState;
	QtyLastSale: PXFieldState;
	CuryUnitPrice: PXFieldState;
	LastSalesDate: PXFieldState;
	DropShipLastQty: PXFieldState;
	DropShipCuryUnitPrice: PXFieldState;
	DropShipLastDate: PXFieldState;
	AlternateID: PXFieldState;
	AlternateType: PXFieldState;
	AlternateDescr: PXFieldState;
}
