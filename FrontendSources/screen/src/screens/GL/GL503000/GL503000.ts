import { autoinject } from 'aurelia-framework';
import {
	PXScreen, createSingle, createCollection, graphInfo, commitChanges, PXView,
	PXFieldState, PXFieldOptions
} from 'client-controls';

@graphInfo({ graphType: 'PX.Objects.GL.FinPeriodStatusProcess', primaryView: 'Filter' })
@autoinject
export class GL503000 extends PXScreen {
	Filter = createSingle(Filter);
	FinPeriods = createCollection(FinPeriods,
		{ adjustPageSize: true, mergeToolbarWith: 'ScreenToolbar' });
}

export class Filter extends PXView {
	OrganizationID: PXFieldState<PXFieldOptions.CommitChanges>;
	Action: PXFieldState<PXFieldOptions.CommitChanges>;
	FromYear: PXFieldState<PXFieldOptions.CommitChanges>;
	ToYear: PXFieldState<PXFieldOptions.CommitChanges>;
	ReopenInSubledgers: PXFieldState;
}

export class FinPeriods extends PXView {
	Selected: PXFieldState<PXFieldOptions.CommitChanges>;
	ProcessingStatus: PXFieldState<PXFieldOptions.Hidden>;
	ProcessingMessage: PXFieldState<PXFieldOptions.Hidden>;
	FinPeriodID: PXFieldState;
	Descr: PXFieldState;
	Status: PXFieldState;
	APClosed: PXFieldState;
	ARClosed: PXFieldState;
	INClosed: PXFieldState;
	CAClosed: PXFieldState;
	FAClosed: PXFieldState;
}
