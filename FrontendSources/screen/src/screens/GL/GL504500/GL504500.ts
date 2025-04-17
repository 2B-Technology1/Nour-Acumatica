import {
	PXScreen, createSingle, createCollection, graphInfo, PXView, PXFieldState,
	PXFieldOptions, linkCommand, columnSettings, PXActionState
} from 'client-controls';

@graphInfo({ graphType: 'PX.Objects.GL.AllocationProcess', primaryView: 'Filter' })
export class GL504500 extends PXScreen {

	Filter = createSingle(Filter);
	Allocations = createCollection(Allocations);

	// this action is from Allocations. I define it here to prevent showing it in grid toolbar. It's platform bug for the moment.
	viewBatch: PXActionState; 

}

export class Filter extends PXView {

	DateEntered: PXFieldState<PXFieldOptions.CommitChanges>;
	FinPeriodID: PXFieldState<PXFieldOptions.CommitChanges>;

}

export class Allocations extends PXView {

	@columnSettings({ allowCheckAll: true, allowSort:false })
	Selected: PXFieldState;

	BranchID: PXFieldState;

	@linkCommand("EditDetail")
	GLAllocationID: PXFieldState;

	Descr: PXFieldState;
	AllocMethod: PXFieldState;
	AllocLedgerID: PXFieldState;
	SortOrder: PXFieldState;

	@linkCommand("viewBatch")
	BatchNbr: PXFieldState;

	BatchPeriod: PXFieldState;
	ControlTotal: PXFieldState;
	Status: PXFieldState;

}
