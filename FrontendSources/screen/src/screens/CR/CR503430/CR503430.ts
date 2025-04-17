import { autoinject } from 'aurelia-framework';
import {
	PXView, PXFieldState, commitChanges, graphInfo, PXScreen, createCollection, createSingle, GridColumnShowHideMode, PXFieldOptions
} from "client-controls";

@graphInfo({ graphType: 'PX.Objects.CR.CRValidationProcess', primaryView: 'Filter' })
@autoinject
export class CR503430 extends PXScreen {
	Filter = createSingle(Filter);
	Contacts = createCollection(Contacts, {
		adjustPageSize: true,
		columnsSettings: [
			{
				field: 'ProcessingStatus',
				visible: false,
			},
			{
				field: 'ProcessingMessage',
				visible: false,
			},
			{
				field: 'Files',
				allowShowHide: GridColumnShowHideMode.False,
				visible: false,
			},
			{
				field: 'Notes',
				allowShowHide: GridColumnShowHideMode.False,
				visible: false,
			},
			{
				field: 'Selected',
				allowCheckAll: true,
				allowShowHide: GridColumnShowHideMode.False,
				visible: false,
			},
			{
				field: 'BAccountID',
				linkCommand: 'Contacts_BAccount_ViewDetails',
			},
			{
				field: 'DisplayName',
				linkCommand: 'Contacts_Contact_ViewDetails',
			},
		]
	});
}

export class Filter extends PXView {
	@commitChanges ValidationType: PXFieldState;
}

export class Contacts extends PXView {
	ProcessingStatus: PXFieldState<PXFieldOptions.Hidden>;
	ProcessingMessage: PXFieldState<PXFieldOptions.Hidden>;
	Selected: PXFieldState
	AggregatedType: PXFieldState;
	BAccountID: PXFieldState;
	AcctName: PXFieldState;
	DisplayName: PXFieldState;
	AggregatedStatus: PXFieldState;
	DuplicateStatus: PXFieldState;
}
