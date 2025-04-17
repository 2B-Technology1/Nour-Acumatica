import {
	PXScreen, createSingle, createCollection, graphInfo, PXView, PXFieldState, PXFieldOptions, linkCommand, columnSettings
} from 'client-controls';

@graphInfo({ graphType: 'PX.Objects.GL.ScheduleRun', primaryView: 'Filter' })
export class GL504000 extends PXScreen {

	Filter = createSingle(Filter);
	Schedule_List = createCollection(Schedule_List);
}

export class Filter extends PXView {
	ExecutionDate: PXFieldState<PXFieldOptions.CommitChanges>;
	LimitTypeSel: PXFieldState<PXFieldOptions.CommitChanges>;
	RunLimit: PXFieldState;
}

export class Schedule_List extends PXView {

	@columnSettings({ allowCheckAll: true, allowSort:false })
	Selected: PXFieldState;

	@linkCommand("EditDetail")
	ScheduleID: PXFieldState;

	ScheduleName: PXFieldState;
	StartDate: PXFieldState;
	EndDate: PXFieldState;
	RunCntr: PXFieldState;
	RunLimit: PXFieldState;
	NextRunDate: PXFieldState;
	LastRunDate: PXFieldState;

}
