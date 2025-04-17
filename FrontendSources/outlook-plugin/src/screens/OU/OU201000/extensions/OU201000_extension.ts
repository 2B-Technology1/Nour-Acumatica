
import {
	featureInstalled,
	PXActionState,
	createSingle,
	placeBeforeView,
	PXView,
	PXFieldState,
	commitChanges,
	placeBeforeProperty
} from 'client-controls';
import { OU201000 } from '../OU201000';
import { OUActivity } from '../views';
import { PXSimpleSelectorState } from './selector-state';


// eslint-disable-next-line @typescript-eslint/no-empty-interface
// tslint:disable-next-line interface-name
export interface OU201000_extension extends OU201000 { }
@featureInstalled('PX.Objects.CS.FeaturesSet+ConstructionProjectManagement')
export class OU201000_extension {
	createRequestForInformation: PXActionState;
	createProjectIssue: PXActionState;
	redirectToCreateRequestForInformation: PXActionState;
	redirectToCreateProjectIssue: PXActionState;

	RequestForInformationOutlook = createSingle(RequestForInformationOutlook);
	ProjectIssueOutlook = createSingle(ProjectIssueOutlook);
}

@placeBeforeView("Filter")
export class RequestForInformationOutlook extends PXView {
  Summary: PXFieldState;
  @commitChanges ProjectId: PXSimpleSelectorState;
  contactId: PXSimpleSelectorState;
  @commitChanges Incoming: PXFieldState;
  @commitChanges Status: PXFieldState;
  @commitChanges ClassId: PXSimpleSelectorState;
  @commitChanges PriorityId: PXSimpleSelectorState;
  OwnerId: PXSimpleSelectorState;
  @commitChanges DueResponseDate: PXFieldState;
  @commitChanges IsScheduleImpact: PXFieldState;
  ScheduleImpact: PXFieldState;
  @commitChanges IsCostImpact: PXFieldState;
  CostImpact: PXFieldState;
  DesignChange: PXFieldState;
}

@placeBeforeView("Filter")
export class ProjectIssueOutlook extends PXView {
  Summary: PXFieldState;
  @commitChanges ProjectId: PXSimpleSelectorState;
  @commitChanges ClassId: PXSimpleSelectorState;
  @commitChanges PriorityId: PXSimpleSelectorState;
  OwnerID: PXSimpleSelectorState;
  @commitChanges DueDate: PXFieldState;
}


// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface INewActivityExtension extends OUActivity { }
@featureInstalled('PX.Objects.CS.FeaturesSet+ConstructionProjectManagement')
export	class INewActivityExtension  {
	@placeBeforeProperty("caseCD") ProjectId: PXSimpleSelectorState;
	@placeBeforeProperty("caseCD") RequestForInformationId: PXSimpleSelectorState;
	@placeBeforeProperty("caseCD") ProjectIssueId: PXSimpleSelectorState;
	@commitChanges IsLinkProject: PXFieldState;
	@commitChanges IsLinkRequestForInformation: PXFieldState;
	@commitChanges IsLinkProjectIssue: PXFieldState;
}
