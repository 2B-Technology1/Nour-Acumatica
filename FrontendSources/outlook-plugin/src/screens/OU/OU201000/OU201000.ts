import { inject } from 'aurelia-dependency-injection';
import { PLATFORM } from 'aurelia-framework';
import {
	graphInfo, featureInstalled, PXScreen, PXActionState, ILongRunIndicatorConfig,
	createSingle, createMatrix, QpTranslator, ErrorInfo, BASE_PATH
} from 'client-controls';
import {LongRunDataComponent} from 'client-controls/controls/dialog/long-run/long-run-data-component';
import { AuthenticationService } from '../../../services/authentication-service';
import { Messages } from '../../../resources/messages';
import { OUActivity, OUAPBillAttachment, OUCase, OUMessage, OUOpportunity, OUSearchEntity } from './views';
import { optional } from 'client-controls/dependency-property-injection';

declare const Office: any;

@graphInfo({ graphType: 'PX.Objects.CR.OUSearchMaint', primaryView: 'Filter' })
@featureInstalled('PX.Objects.CS.FeaturesSet+outlookIntegration')
export class OU201000 extends PXScreen {
	msg = Messages;
	back: PXActionState;
	logOut: PXActionState;

	longRunConfig: ILongRunIndicatorConfig;
	chooseDocumentLabel: string = Messages.TitleSelectADocument;
	attachmentsNames: any[];

	Filter = createSingle(OUSearchEntity);
	NewCase = createSingle(OUCase);
	NewOpportunity = createSingle(OUOpportunity);
	NewActivity = createSingle(OUActivity);
	SourceMessage = createSingle(OUMessage);

	APBillAttachments = createMatrix(OUAPBillAttachment);

	private helpUrl = "Main?ScreenId=ShowWiki&pageid=875b6f76-2820-4ea9-a420-7f7a8bbfb619";
	private accessDenied: boolean;

	constructor(@inject(QpTranslator) private translator: QpTranslator,
		@inject(AuthenticationService) private authService: AuthenticationService,
		@optional(BASE_PATH) private baseUrl: string) {
		super();
	}

	async attached() {
		this.optimizeDataSelection = false;

		this.longRunConfig = {
			id: "longRun",
			text: Messages.DocRecognitionProgress,
			successedTextCallback: () => <string> this.Filter.NumOfRecognizedDocuments?.value,
			cancelCallback: () => {
				this.screenService.executeCommand("LongRun");
			}
		};

		this.screenService.registerDataComponent("LongRunData", new LongRunDataComponent(this.eventAggregator, this.screenService, null));

		await super.attached();
	}

	onHelpClick = () : void => {
		Office.context.ui.displayDialogAsync((new Request(this.baseUrl + this.helpUrl)).url);
	}

	logOutAction() {
		this.authService.logOut().then(x => this.authService.lostAuth(true));
	}

	async updateValuesFromOffice(): Promise<void> {
		if (PLATFORM.global.Office === undefined) return;

		const mailbox = Office.context.mailbox;
		if (!mailbox) return;

		const message = mailbox.item;
		if (!message) return;

		const f = this.Filter; const sm = this.SourceMessage;
		if (f.EMail) f.EMail.updateValue(message.sender.emailAddress);
		if (f.DisplayName) f.DisplayName.updateValue(message.sender.displayName);
		if (f.NewContactFirstName) f.NewContactFirstName.updateValue(getFirstName(message.sender.displayName));
		if (f.NewContactLastName) f.NewContactLastName.updateValue( getLastName(message.sender.displayName));
		if (sm.IsIncome) sm.IsIncome.updateValue(true);

		if (!f.EMail || f.EMail.value === "" || message.sender.emailAddress === mailbox.userProfile.emailAddress) {
			if (sm.IsIncome) sm.IsIncome.updateValue(false);
			if (f.DisplayName) f.DisplayName.updateValue('');
			if (f.NewContactFirstName) f.NewContactFirstName.updateValue('');
			if (f.NewContactLastName) f.NewContactLastName.updateValue('');
		}

		if (sm.MessageId) sm.MessageId.updateValue(message.internetMessageId);
		if (sm.Subject) sm.Subject.updateValue(message.subject);
		if (sm.ItemId) sm.ItemId.updateValue(message.itemId);
		if (sm.To) sm.To.updateValue(serializeAddressList(message.to));
		if (sm.CC) sm.CC.updateValue(serializeAddressList(message.cc));
		if (sm.EwsUrl) sm.EwsUrl.updateValue(mailbox.ewsUrl);

		if (f.AttachmentNames) {
			const pdfFileExt = '.pdf';
			const names = (message.attachments as any[]).filter(function (a) {
				return a.contentType === 'application/pdf' ||
					a.contentType === 'application/x-pdf' ||
					a.contentType === 'application/octet-stream' &&
					// eslint-disable-next-line @typescript-eslint/no-magic-numbers
				a.name.toLowerCase().lastIndexOf(pdfFileExt) === a.name.length - pdfFileExt.length;
			}).map(function (a) {
				return a.name;
			});

			this.attachmentsNames = names;

			f.AttachmentsCount.updateValue(names.length);
			f.AttachmentNames.updateValue(`${names.join(';')  };`);
		}

		if (sm.IsIncome?.value) f.OutgoingEmail.updateValue(message.sender.emailAddress);
		if (!sm.Token?.value || sm.Token.value === 'none') {
			const screenService = this.screenService;
			await new Promise<void>((resolve, reject) => {
				try {
					mailbox.getCallbackTokenAsync(
						(asyncResult: any) => {
							if (asyncResult.status === 'succeeded') {
								sm.Token.updateValue(asyncResult.value);
								screenService.applyFieldUpdated();
							}
							else sm.Token.updateValue('none');
							resolve();
						});
				}
				catch (ex) {
					console.log(ex);
					sm.Token.updateValue("none");
					reject();
				}
			});
		}
	}

	protected async onBeforeInitialize() {
		this.accessDenied = false;
		const locale = this.authService.getLocale();
		if (locale) {
			const r = await this.authService.localizedStrings();
			QpTranslator.FillInDefaultDictionary(r);
			this.translator.SetLang(locale);
		}
	}

	protected async onInitialize() : Promise<void> {
		await this.updateValuesFromOffice();
	}

	protected onAfterInitialize() : void {
		this.screenService.applyFieldUpdated();
	}

	protected onInitializeError(e: ErrorInfo) {
		this.accessDenied = this.authService.screenNotFound;
		this.clearActions();
		this.back = this.logOut = undefined;

		this.Filter.hideAll();
		this.NewCase.hideAll();
		this.NewOpportunity.hideAll();
		this.NewActivity.hideAll();
		this.SourceMessage.hideAll();
		this.eventAggregator.publish("screen-updated");
	}
}

function serializeAddressList(list: Array<any>) : string {
	let msg = "";
	if (list != null) {
		list.forEach(function (recip, index) {
			msg = `${msg  }"${recip.displayName}" <${recip.emailAddress}>;`;
		});
	}
	return msg;
}

function getFirstName(displayName: string) : string {
	displayName = displayName.trim();
	while (displayName.indexOf("  ") > -1)	displayName = displayName.replace("  ", " ");

	const names = displayName.split(" ");
	const firstName = names.length > 1 ? names[0] : null;
	return firstName;
}

function getLastName(displayName: string) : string {
	displayName = displayName.trim();
	while (displayName.indexOf("  ") > -1) displayName = displayName.replace("  ", " ");

	const names = displayName.split(" ");
	const lastName = names.length > 1 ? names[names.length - 1] : names[0];
	return lastName;
}
