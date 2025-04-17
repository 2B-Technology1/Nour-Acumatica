import {
	SO301000
} from '../SO301000';

import {
	PXView,
	createCollection,
	PXFieldState,
	PXFieldOptions,
	viewInfo,
	columnSettings,
	localizable
} from 'client-controls';


export interface SO301000_Approvals extends SO301000 {}
export class SO301000_Approvals {
	@viewInfo({containerName: "Approvals"})
	Approval = createCollection(
		SOApproval,
		{
			allowInsert: false,
			allowUpdate: false,
			allowDelete: false
		}
	);
}

export class SOApproval extends PXView {
	ApproverEmployee__AcctCD: PXFieldState;
	ApproverEmployee__AcctName: PXFieldState;
	@columnSettings({hideViewLink: true}) WorkgroupID: PXFieldState;
	ApprovedByEmployee__AcctCD: PXFieldState;
	ApprovedByEmployee__AcctName: PXFieldState;
	ApproveDate: PXFieldState;
	@columnSettings({allowUpdate: false}) Status: PXFieldState;
	@columnSettings({allowUpdate: false}) Reason: PXFieldState;
	AssignmentMapID: PXFieldState;
	RuleID: PXFieldState;
	StepID: PXFieldState;
	CreatedDateTime: PXFieldState<PXFieldOptions.Hidden>;
}
