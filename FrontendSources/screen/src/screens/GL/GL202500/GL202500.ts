import { autoinject } from 'aurelia-framework';
import {
	PXScreen, createCollection, graphInfo, PXView, createSingle, PXFieldState, PXFieldOptions
} from 'client-controls';

@graphInfo({ graphType: 'PX.Objects.GL.AccountMaint', primaryView: 'AccountRecords' })
@autoinject
export class GL202500 extends PXScreen {
	AccountRecords = createCollection(AccountRecords,
		{ adjustPageSize: true, initNewRow: true, syncPosition: true, quickFilterFields: ['AccountClassID', 'Type', 'PostOption', 'CuryID'], mergeToolbarWith: 'ScreenToolbar' });

	AccountTypeChangePrepare = createSingle(AccountTypeChangePrepare);
}

export class AccountRecords extends PXView {
	AccountCD: PXFieldState
	AccountClassID: PXFieldState<PXFieldOptions.CommitChanges>;
	Type: PXFieldState;
	Active: PXFieldState;
	Description: PXFieldState;
	ControlAccountModule: PXFieldState<PXFieldOptions.CommitChanges>;
	AllowManualEntry: PXFieldState<PXFieldOptions.CommitChanges>;
	PostOption: PXFieldState;
	IsCashAccount: PXFieldState;
	CuryID: PXFieldState<PXFieldOptions.CommitChanges>;
	RevalCuryRateTypeId: PXFieldState<PXFieldOptions.CommitChanges>;
	AccountGroupID: PXFieldState<PXFieldOptions.CommitChanges>;
	AccountID: PXFieldState;
	RequireUnits: PXFieldState;
	CreatedDateTime: PXFieldState<PXFieldOptions.Hidden>;
}

export class AccountTypeChangePrepare extends PXView {
	Message: PXFieldState;
}
