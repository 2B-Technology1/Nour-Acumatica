import { PXView, group, PXFieldState, PXFieldOptions, PXFieldStatePair, text, commitChanges } from "client-controls";
import { PXSimpleSelectorState } from "./extensions/selector-state";

export class OUSearchEntity extends PXView {
	@group('Title') @commitChanges OutgoingEmail: PXFieldState<PXFieldOptions.Default, 'email'>;
	@group('Title') @commitChanges ContactID: PXSimpleSelectorState<PXFieldOptions.Default, 'Person'>;
	@group('Title') EMail: PXFieldState<PXFieldOptions.Readonly | PXFieldOptions.NoLabel>;

	@group('Title') ErrorMessage: PXFieldState<PXFieldOptions.Multiline | PXFieldOptions.NoLabel>;
	@group('Info') DisplayName: PXFieldState<PXFieldOptions.Hidden>;
	@group('Info') NewContactFirstName: PXFieldState;
	@group('Info') NewContactLastName: PXFieldState;
	@group('Info') NewContactEmail: PXFieldState;
	@group('Info') Salutation: PXFieldState;
	@group('Info') @commitChanges BAccountID: PXSimpleSelectorState;
	// @group('Info') FullName = FieldState.createUnbound(() => { return (this.NewContactFirstName?.value ?? '') + ' ' + (this.NewContactLastName?.value ?? '') });
	@group('Info') FullName: PXFieldState;
	@group('Info') LeadSource: PXFieldState<PXFieldOptions.Default, 'Lead source'>;
	@group('Info') ContactSource: PXFieldState<PXFieldOptions.Default, 'Contact source'>;
	@group('Info') CountryID: PXSimpleSelectorState;
	@group('Info') EntityID: PXFieldState;
	EntityName: PXFieldState<PXFieldOptions.Hidden>;
	AttachmentNames: PXFieldState<PXFieldOptions.Hidden>;
	AttachmentsCount: PXFieldState<PXFieldOptions.Hidden>;
	IsRecognitionInProgress: PXFieldState<PXFieldOptions.Hidden>;
	NumOfRecognizedDocuments: PXFieldState<PXFieldOptions.Hidden>;
	Operation: PXFieldState<PXFieldOptions.Hidden>;
	DuplicateFilesMsg: PXFieldState<PXFieldOptions.Multiline>;
}

export class OUCase extends PXView {
	@commitChanges CaseClassID: PXSimpleSelectorState;
	@commitChanges ContractID: PXSimpleSelectorState;
	Severity: PXFieldState;
	Subject: PXFieldState;
}

export class OUOpportunity extends PXView {
	ClassID: PXSimpleSelectorState;
	Subject: PXFieldState;
	StageID: PXFieldState;
	CloseDate: PXFieldState;
	@commitChanges ManualAmount: PXFieldStatePair<OUOpportunity, 'CurrencyID'>;
	CurrencyID: PXSimpleSelectorState<PXFieldOptions.Hidden>;
	BranchID: PXSimpleSelectorState;
}

export class OUActivity extends PXView {
	Subject: PXFieldState;
	CaseCD: PXSimpleSelectorState;
	OpportunityID: PXSimpleSelectorState;
	@commitChanges IsLinkContact: PXFieldState;
	@commitChanges IsLinkCase: PXFieldState;
	@commitChanges IsLinkOpportunity: PXFieldState;
}

export class OUMessage extends PXView {
	MessageId: PXFieldState<PXFieldOptions.Hidden>;
	To: PXFieldState<PXFieldOptions.Hidden>;
	CC: PXFieldState<PXFieldOptions.Hidden>;
	Subject: PXFieldState<PXFieldOptions.Hidden>;
	ItemId: PXFieldState<PXFieldOptions.Hidden>;
	IsIncome: PXFieldState<PXFieldOptions.Hidden>;
	EwsUrl: PXFieldState<PXFieldOptions.Hidden>
	@commitChanges  Token: PXFieldState<PXFieldOptions.Hidden>;
}

export class OUAPBillAttachment extends PXView {
	ItemId: PXFieldState<PXFieldOptions.Hidden>;
	Id: PXFieldState<PXFieldOptions.Hidden>;
	Name: PXFieldState<PXFieldOptions.NoLabel>;
	@commitChanges @text('') Selected: PXFieldState;
	RecognitionStatus: PXFieldState<PXFieldOptions.Hidden>;
}
