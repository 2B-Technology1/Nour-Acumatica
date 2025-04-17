import {
	SO301000
} from '../SO301000';

import {
	PXView,
	createCollection,
	createSingle,
	PXFieldState,
	PXFieldOptions,
	localizable,
	linkCommand,
	columnSettings,
	viewInfo
} from 'client-controls';


@localizable
export class RelatedItemsPanelHeaders {
	static AddRelatedItems = "Add Related Items";
}

export interface SO301000_RelatedItems extends SO301000 {}
export class SO301000_RelatedItems {
	RelatedItemsPanelHeaders = RelatedItemsPanelHeaders;

	@viewInfo({containerName: "Add Related Items"})
	RelatedItemsFilter = createSingle(RelatedItemsFilter);

	@viewInfo({containerName: "Add Related Items"})
	allRelatedItems = createCollection(
		RelatedItems,
		{
			syncPosition: true,
			adjustPageSize: true,
			allowInsert: false,
			allowDelete: false
		}
	);

	@viewInfo({containerName: "Add Related Items"})
	substituteItems = createCollection(
		RelatedItems,
		{
			syncPosition: true,
			adjustPageSize: true,
			allowInsert: false,
			allowDelete: false
		}
	);

	@viewInfo({containerName: "Add Related Items"})
	upSellItems = createCollection(
		RelatedItems,
		{
			syncPosition: true,
			adjustPageSize: true,
			allowInsert: false,
			allowDelete: false
		}
	);

	@viewInfo({containerName: "Add Related Items"})
	crossSellItems = createCollection(
		RelatedItems,
		{
			syncPosition: true,
			adjustPageSize: true,
			allowInsert: false,
			allowDelete: false
		}
	);

	@viewInfo({containerName: "Add Related Items"})
	otherRelatedItems = createCollection(
		RelatedItems,
		{
			syncPosition: true,
			adjustPageSize: true,
			allowInsert: false,
			allowDelete: false
		}
	);
}

export class RelatedItemsFilter extends PXView {
	InventoryID: PXFieldState<PXFieldOptions.CommitChanges>;
	CuryUnitPrice: PXFieldState;
	CuryID: PXFieldState<PXFieldOptions.CommitChanges>;
	Qty: PXFieldState<PXFieldOptions.CommitChanges>;
	UOM: PXFieldState;
	CuryExtPrice: PXFieldState;
	AvailableQty: PXFieldState;
	SiteID: PXFieldState;
	KeepOriginalPrice: PXFieldState<PXFieldOptions.CommitChanges>;
	OnlyAvailableItems: PXFieldState<PXFieldOptions.CommitChanges>;
	ShowForAllWarehouses: PXFieldState<PXFieldOptions.CommitChanges>;
	ShowSubstituteItems: PXFieldState;
	ShowUpSellItems: PXFieldState;
	ShowCrossSellItems: PXFieldState;
	ShowOtherRelatedItems: PXFieldState;
	ShowAllRelatedItems: PXFieldState;
}

export class RelatedItems extends PXView {
	Selected: PXFieldState<PXFieldOptions.CommitChanges>;
	QtySelected: PXFieldState;
	Rank: PXFieldState;
	Relation: PXFieldState;
	Tag: PXFieldState;
	@linkCommand("ViewRelatedItem") RelatedInventoryID: PXFieldState;
	SubItemID: PXFieldState;
	SubItemCD: PXFieldState<PXFieldOptions.Hidden>;
	Desc: PXFieldState;
	@columnSettings({hideViewLink: true}) UOM: PXFieldState;
	CuryUnitPrice: PXFieldState;
	CuryExtPrice: PXFieldState;
	PriceDiff: PXFieldState;
	AvailableQty: PXFieldState;
	@columnSettings({hideViewLink: true}) SiteID: PXFieldState;
	SiteCD: PXFieldState<PXFieldOptions.Hidden>;
	Interchangeable: PXFieldState;
	Required: PXFieldState;
}
