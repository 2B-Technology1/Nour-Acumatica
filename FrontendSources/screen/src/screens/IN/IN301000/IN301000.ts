import {
	PXScreen,
	createCollection,
	createSingle,
	graphInfo,
	commitChanges,
	PXView,
	PXFieldState,
	PXActionState,
	localizable
} from 'client-controls';
import { Messages as SysMessages } from 'client-controls/services/messages';

@localizable
class Messages {
	static TestHeader = "Test";
}

@localizable
class TabHeaders {
	static Details = "Details";
	static Financial = "Financial";
	static Manufacturing = "Manufacturing";
}

export class INRegister extends PXView {
	@commitChanges
	RefNbr: PXFieldState;
	Status: PXFieldState;
	@commitChanges
	BranchID: PXFieldState;
	@commitChanges
	TranDate: PXFieldState;

	@commitChanges
	FinPeriodID: PXFieldState;

	@commitChanges
	TransferNbr: PXFieldState;
	ExtRefNbr: PXFieldState;
	TranDesc: PXFieldState;

	TotalQty: PXFieldState;
	ControlQty: PXFieldState;
	TotalCost: PXFieldState;
	ControlCost: PXFieldState;
	//Financial
	BatchNbr: PXFieldState;
	BranchBaseCuryID: PXFieldState;
	//Manufacturing
	AMBatNbr: PXFieldState;
	AMDocType: PXFieldState;
}

export class INTran extends PXView {
	LineSplittingExtension_ShowSplits: PXActionState; //Line Details button on Details tab
	AddInvBySite: PXActionState; //Add Items button on Details tab

	@commitChanges
	BranchID: PXFieldState;
	@commitChanges
	InventoryID: PXFieldState;
	SubItemID: PXFieldState;
	@commitChanges SiteID: PXFieldState;
	@commitChanges LocationID: PXFieldState;
	Qty: PXFieldState;
	@commitChanges UOM: PXFieldState;
	UnitCost: PXFieldState;
	TranCost: PXFieldState;
	@commitChanges LotSerialNbr: PXFieldState;
	ExpireDate: PXFieldState;
	@commitChanges ReasonCode: PXFieldState;
	CostLayerType: PXFieldState;
	SpecialOrderCostCenterID: PXFieldState;
	@commitChanges ProjectID: PXFieldState;
	@commitChanges TaskID: PXFieldState;
	@commitChanges CostCodeID: PXFieldState;
	TranDesc: PXFieldState;
	POReceiptType: PXFieldState;
	POReceiptNbr: PXFieldState;
}

export class INSiteStatusFilter extends PXView {
	@commitChanges Inventory: PXFieldState;
	@commitChanges BarCode: PXFieldState;
	@commitChanges ItemClass: PXFieldState;
	@commitChanges SubItem: PXFieldState;
	@commitChanges SiteID: PXFieldState;
	@commitChanges LocationID: PXFieldState;

	@commitChanges OnlyAvailable: PXFieldState;
}

export class INSiteStatusSelected extends PXView {
	Selected: PXFieldState;
	QtySelected: PXFieldState;
	SiteID: PXFieldState;
	//SiteCD: PXFieldState;
	LocationID: PXFieldState;
	//LocationCD: PXFieldState;
	ItemClassID: PXFieldState;
	ItemClassDescription: PXFieldState;
	PriceClassID: PXFieldState;
	PriceClassDescription: PXFieldState;
	InventoryCD: PXFieldState;
	SubItemID: PXFieldState;
	//SubItemCD: PXFieldState;
	Descr: PXFieldState;
	BaseUnit: PXFieldState;
	QtyAvail: PXFieldState;
	QtyOnHand: PXFieldState;
}

export class LineSplittingHeader extends PXView {
	@commitChanges UnassignedQty: PXFieldState;
	@commitChanges Qty: PXFieldState;
	@commitChanges StartNumVal: PXFieldState;
}

export class INTranSplit extends PXView {
	InventoryID: PXFieldState;
	SubItemID: PXFieldState;
	LocationID: PXFieldState;
	LotSerialNbr: PXFieldState;
	Qty: PXFieldState;
	UOM: PXFieldState;
	ExpireDate: PXFieldState;
}

@graphInfo({ graphType: 'PX.Objects.IN.INReceiptEntry', primaryView: 'receipt' })
export class IN301000 extends PXScreen {
	SysMessages = SysMessages;
	TabHeaders = TabHeaders;
	Msg = Messages;


	LineSplittingExtension_GenerateNumbers: PXActionState; //Generate button on Line Details panel

	AddInvSelBySite: PXActionState; //Add button on Inventory Lookup panel

	receipt = createSingle(INRegister); //Header
	CurrentDocument = createSingle(INRegister); //Header

	//Details
	transactions = createCollection(INTran, {
		initNewRow: true,
		syncPosition: true,
		adjustPageSize: true,
		wrapToolbar: true,
		columnsSettings: [
			{ field: "BranchID", hideViewLink: true },
			{ field: "SiteID", hideViewLink: true },
			{ field: "LocationID", hideViewLink: true },
			{ field: "UOM", hideViewLink: true },
			{ field: "ReasonCode", hideViewLink: true },
			{ field: "LotSerialNbr", hideViewLink: true }
		]
	});

	//Line Details header
	LineSplittingExtension_LotSerOptions = createSingle(LineSplittingHeader);
	//Line Details grid
	splits = createCollection(INTranSplit, {
		syncPosition: true,
		adjustPageSize: true,
		columnsSettings: [
			{ field: "InventoryID", hideViewLink: true },
			{ field: "LocationID", hideViewLink: true },
			{ field: "LotSerialNbr", hideViewLink: true },
			{ field: "UOM", hideViewLink: true }
		],
	});

	//Inventory lookup filter
	sitestatusfilter = createSingle(INSiteStatusFilter);
	//Inventory lookup grid
	sitestatus = createCollection(INSiteStatusSelected, {
		syncPosition: true,
		adjustPageSize: true,
		allowInsert: false,
		allowDelete: false,
		columnsSettings: [
			{ field: "Selected", allowCheckAll: true },
			{ field: "SiteID", hideViewLink: true },
			{ field: "LocationID", hideViewLink: true },
			{ field: "ItemClassID", hideViewLink: true },
			{ field: "PriceClassID", hideViewLink: true },
			{ field: "InventoryCD", hideViewLink: true },
			{ field: "SubItemID", hideViewLink: true },
			{ field: "BaseUnit", hideViewLink: true }
		],
	});
}
