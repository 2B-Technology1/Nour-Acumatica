import { autoinject } from 'aurelia-framework';
import {
	PXView, PXFieldState, commitChanges, graphInfo, PXScreen, createCollection
} from "client-controls";

@graphInfo({ graphType: 'PX.Objects.CR.CRActivitySetupMaint', primaryView: 'ActivityTypes' })
@autoinject
export class CR102000 extends PXScreen {
	ActivityTypes = createCollection(ActivityTypes, {
		adjustPageSize: true, quickFilterFields: ['ClassID', 'Type', 'Description'], mergeToolbarWith: "ScreenToolbar"
	});
}

export class ActivityTypes extends PXView {
	ClassID: PXFieldState
	Type: PXFieldState;
	Description: PXFieldState;
	Active: PXFieldState;
	IsDefault: PXFieldState;
	@commitChanges Application: PXFieldState;
	ImageUrl: PXFieldState;
	@commitChanges PrivateByDefault: PXFieldState;
	@commitChanges RequireTimeByDefault: PXFieldState;
	Incoming: PXFieldState;
	Outgoing: PXFieldState;
}

