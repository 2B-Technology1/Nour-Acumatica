import {
	PXScreen, createSingle, createCollection, graphInfo, PXView, PXFieldState, columnSettings, linkCommand, PXActionState, PXFieldOptions
} from 'client-controls';

export class FABookBalance extends PXView {

	@columnSettings({ allowCheckAll: true })
	Selected: PXFieldState;

	FixedAsset__BranchID: PXFieldState;

	@linkCommand('ViewAsset')
	@columnSettings({ allowUpdate: false })
	AssetID: PXFieldState;

	FixedAsset__Description: PXFieldState;

	@linkCommand('ViewClass')
	@columnSettings({ allowUpdate: false })
	ClassID: PXFieldState;

	FixedAsset__ParentAssetID: PXFieldState;

	@linkCommand('ViewBook')
	@columnSettings({ allowUpdate: false })
	BookID: PXFieldState;

	@columnSettings({ allowUpdate: false })
	CurrDeprPeriod: PXFieldState;

	@columnSettings({ allowNull: false })
	YtdDeprBase: PXFieldState;

	@columnSettings({ allowUpdate: false })
	FixedAsset__BaseCuryID: PXFieldState;

	FADetails__ReceiptDate: PXFieldState;

	FixedAsset__UsefulLife: PXFieldState;

	FixedAsset__FAAccountID: PXFieldState;

	FixedAsset__FASubID: PXFieldState;

	FADetails__TagNbr: PXFieldState;

	Account__AccountClassID: PXFieldState;
}

export class Filter extends PXView {
	OrgBAccountID: PXFieldState<PXFieldOptions.CommitChanges>;
	BookID: PXFieldState<PXFieldOptions.CommitChanges>;
	PeriodID: PXFieldState<PXFieldOptions.CommitChanges>;
	Action: PXFieldState<PXFieldOptions.CommitChanges>;
	ClassID: PXFieldState<PXFieldOptions.CommitChanges>;
	ParentAssetID: PXFieldState<PXFieldOptions.CommitChanges>;
}

@graphInfo({ graphType: 'PX.Objects.FA.CalcDeprProcess', primaryView: 'Filter' })
export class FA502000 extends PXScreen {

	ViewBook: PXActionState;
	ViewAsset: PXActionState;
	ViewClass: PXActionState;

	Filter = createSingle(Filter);
	Balances = createCollection(FABookBalance,
		{ adjustPageSize: true, syncPosition: true, mergeToolbarWith: 'ScreenToolbar' });
}
