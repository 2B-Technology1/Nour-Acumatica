import { PXView, createSingle, graphInfo, PXScreen, createCollection, columnSettings } from "client-controls";
import { PXFieldOptions, PXFieldState } from "client-controls/descriptors/fieldstate";
import { autoinject } from 'aurelia-framework';

@graphInfo({ graphType: 'PX.Objects.GL.GLHistoryValidate', primaryView: 'Filter' })
@autoinject
export class GL509900 extends PXScreen {

	Filter = createSingle(Filter);
	LedgerList = createCollection(LedgerList,
		{ adjustPageSize: true, mergeToolbarWith: 'ScreenToolbar' });
}

export class LedgerList extends PXView {

	@columnSettings({ allowCheckAll: true, allowNull: false })
	Selected: PXFieldState;
	LedgerCD: PXFieldState;
	Descr: PXFieldState;
}

export class Filter extends PXView {
	FinPeriodID: PXFieldState<PXFieldOptions.CommitChanges>;
}
