import {
	PXScreen, createSingle, createCollection, graphInfo, PXView, PXFieldState, columnSettings, PXFieldOptions
} from 'client-controls';

export class FADocumentList extends PXView {

	@columnSettings({ allowCheckAll: true, allowNull: false, allowUpdate: false })
	Selected: PXFieldState;

	@columnSettings({ allowUpdate: false })
	RefNbr: PXFieldState;

	@columnSettings({ allowNull: false, allowUpdate: false })
	Origin: PXFieldState;

	@columnSettings({ allowNull: false, allowUpdate: false })
	Status: PXFieldState;

	@columnSettings({ allowUpdate: false })
	DocDate: PXFieldState;

	@columnSettings({ allowUpdate: false })
	FinPeriodID: PXFieldState;

	@columnSettings({ allowUpdate: false })
	DocDesc: PXFieldState;
}

export class Filter extends PXView {
	Origin: PXFieldState<PXFieldOptions.CommitChanges>;
}

@graphInfo({ graphType: 'PX.Objects.FA.AssetTranRelease', primaryView: 'Filter' })
export class FA503000 extends PXScreen {
	Filter = createSingle(Filter);
	FADocumentList = createCollection(FADocumentList,
		{ adjustPageSize: true, syncPosition: true, mergeToolbarWith: 'ScreenToolbar' });
}
