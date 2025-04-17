import {
	PXView, createSingle, PXFieldState, graphInfo, group, PXScreen, createCollection, PXFieldOptions, PXActionState
} from "client-controls";
import { autoinject } from 'aurelia-framework';

@graphInfo({ graphType: 'PX.Objects.GL.AccountHistoryEnq', primaryView: 'Filter' })
@autoinject
export class GL401000 extends PXScreen {

	ViewDetails: PXActionState;
	Filter = createSingle(GLHistoryEnqFilter);
	EnqResult = createCollection(GLHistoryEnquiryResult,
		{ adjustPageSize: true, allowSkipTabs: true, mergeToolbarWith: 'ScreenToolbar', syncPosition: true });
}

export class GLHistoryEnqFilter extends PXView {

	@group('column1')
	OrgBAccountID: PXFieldState<PXFieldOptions.CommitChanges>;

	@group('column1')
	LedgerID: PXFieldState<PXFieldOptions.CommitChanges>;

	@group('column1')
	FinPeriodID: PXFieldState<PXFieldOptions.CommitChanges>;

	@group('column2')
	AccountClassID: PXFieldState<PXFieldOptions.CommitChanges>;

	@group('column2')
	SubCD: PXFieldState<PXFieldOptions.CommitChanges>;

	@group('column2')
	UseMasterCalendar: PXFieldState<PXFieldOptions.CommitChanges>;

}

export class GLHistoryEnquiryResult extends PXView {
	BranchID: PXFieldState;
	AccountCD: PXFieldState;
	LedgerID: PXFieldState<PXFieldOptions.Hidden>;
	SubCD: PXFieldState<PXFieldOptions.Hidden>;
	Type: PXFieldState;
	Description: PXFieldState;
	LastActivityPeriod: PXFieldState;
	SignBegBalance: PXFieldState;
	PtdDebitTotal: PXFieldState;
	PtdCreditTotal: PXFieldState;
	SignEndBalance: PXFieldState;
	PtdSaldo: PXFieldState;
	ConsolAccountCD: PXFieldState;
	AccountClassID: PXFieldState;
}
