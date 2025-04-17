import { Disposable, bindable, autoinject } from 'aurelia-framework';
import { getLogger } from "aurelia-logging";
import { EventAggregator } from 'aurelia-event-aggregator';
import { EventManager, delegationStrategy } from "aurelia-binding";
import { Container } from 'aurelia-dependency-injection';
import {
	QpEventManager, OpenPopupEvent, ExecuteCommandEvent, PXScreen,
	QpToolBarCustomElement, IToolBarItem,
	IScreenTitleControlConfig, IToolBarControlConfig, IDataComponentParams, IMenuControlConfig, IMenuItem,
	IScreenService, RefreshScreenEvent, ScreenApiClientSettings, SessionUrlRouter, RedirectHelper
} from 'client-controls';
import { IScreenPreferences, IUserInfo, PreferenceLoadMode, PreferencesService } from "client-controls/services/preferences";

import { getScreenIdFromUrl, getScreenPath } from './screen-utils';
import { ILongRunControlConfig } from 'client-controls/controls/dialog/long-run/qp-long-run';
import { LongRunDataComponent } from 'client-controls/controls/dialog/long-run/long-run-data-component';
import { IQuickProcessingConfig } from 'client-controls/controls/dialog/quick-processing/qp-quick-processing';
import { QuickProcessingDataComponent } from 'client-controls/controls/dialog/quick-processing/quick-processing-data-component';
import { localizable, QpTranslator } from 'client-controls/services/localization';
import { AlertService } from 'client-controls/services/alert-service';
import { ScreenService } from 'client-controls/services/screen-service';
import { PopupMessageOpenEvent } from 'client-controls/services/screen-service';
import { KeyboardService } from 'client-controls/services/keyboard-service';
import { DialogHelper } from 'client-controls/controls/dialog/base-dialog/dialog-helper';
import { BusyCounter } from 'client-controls/utils';


const logger = getLogger('qp-app');

@localizable
class Messages {
	static ScreenID = "Screen ID";
	static Note = "Note";
}

declare const HTML_MERGED: boolean;

@autoinject
export class App {
	@bindable screenName?: string;
	@bindable useStaticRendering: boolean = HTML_MERGED;
	initialized = false;
	viewModel?: PXScreen;
	screenService?: IScreenService;
	contentElement!: HTMLElement;
	forceUI?: string;
	keyboardService: KeyboardService;

	@bindable longRunConfig?: ILongRunControlConfig = {
		id: "ctl00_phDS_ds_LongRun",
		switchedOff: false,
		enabled: true,
		spinHidden: true,
		hidden: true,
		status: "aborted",
		longRunAborted: '',
		longRunCompleted: '',
		longRunInProcess: '',
		longRunMessage: '',
		longRunAbort: ''
	};

	@bindable quickProcessingConfig?: IQuickProcessingConfig = {
		id: "ctl00_phDS_ds_QuickProcessing",
		hidden: true,
		enabled: true,
	}

	eventSubscriptions: Disposable[] = [];
	baseHref: string = "";

	toolsMenu: IMenuControlConfig;
	customizationMenu: IMenuControlConfig;
	toolbar: IToolBarControlConfig;
	caption: IScreenTitleControlConfig;
	editMode: boolean = false;

	notesMenu: any;

	private toolBarVM?: QpToolBarCustomElement = undefined;
	private Msg = Messages;
	private screenEventManager: QpEventManager;
	private giScreenID = 'GenericInquiry';
	private toolsMenuElem: HTMLElement;
	private captionElem: HTMLElement;
	private toolBarElem: HTMLElement;

	constructor(private container: Container,
		private eventAggregator: EventAggregator,
		protected dialogHelper: DialogHelper,
		private translator: QpTranslator,
		private alertServce: AlertService,
		private preferencesService: PreferencesService,
		protected eventManager: EventManager,
		private redirectHelper: RedirectHelper
	) {
		this.screenEventManager = container.get(QpEventManager);
		this.keyboardService = container.invoke(KeyboardService);
		container.registerInstance(KeyboardService, this.keyboardService);
		(<any>window).busyService = container.get(BusyCounter);
	}

	viewModelActivated() {
		this.screenService = this.viewModel.getScreenService();
		this.container.registerInstance(ScreenService, this.screenService);

		// all default screen preferences come on first GET request
		this.screenService.registerDataComponent("DefaultScreenPreferences", {
			setComponentData: (r: any) => {
				this.applyScreenPreferences(r.defaultPreferences, "def");
				this.screenService.unregisterDataComponent("DefaultScreenPreferences");
			},
			getQueryParams: this.getQueryParams,
		});
		// this data component works only on first POST request - all user preferences come on first POST request
		this.screenService.registerDataComponent("UserScreenPreferences", {
			setComponentData: (r: any) => {
				this.applyScreenPreferences(r.userPreferences, "user");
				this.screenService.unregisterDataComponent("UserScreenPreferences");
			},
			getQueryParams: this.getQueryParams,
		});
		this.screenService.registerDataComponent("ToolMenuData", {
			element: this.toolsMenuElem,
			setComponentData: (r: any) => this.applyMenuResult(r),
			getQueryParams: this.getQueryParams,
		});
		this.screenService.registerDataComponent("CustomizationMenuData", {
			element: this.toolsMenuElem,
			setComponentData: (r: any) => this.applyCustomizationResult(r),
			getQueryParams: this.getQueryParams,
		});
		this.screenService.registerDataComponent("NotesMenuData", {
			element: this.toolsMenuElem,
			setComponentData: (r: any) => this.applyNotesResult(r),
			getQueryParams: this.getQueryParams,
		});
		this.screenService.registerDataComponent("TitleData", {
			element: this.captionElem,
			setComponentData: (r: any) => this.applyCaptionResult(r),
			getQueryParams: this.getQueryParams,
		});
		this.screenService.registerDataComponent("ScreenToolbar", {
			element: this.toolBarElem,
			setComponentData: (r: IToolBarControlConfig) => {
				this.toolbar = r;
				this.toolbar.id = 'MainToolbar';

				this.toolBarVM?.unbindStates();
				for (const key of this.viewModel.actions.keys()) {
					const state = this.viewModel.actions.get(key);
					if (state.specialType === 1) {
						this.toolBarVM?.bindStateWith(state);

						const entry = Object.values(this.toolbar?.items || {}).find( (item: IToolBarItem) => {
							if (item.config.commandName === state.commandName) return true;
							return false;
						} );

						entry.config.state = state;
					}
				}

			},
			getQueryParams: this.getQueryParams,
			appendComponentData: (d: IToolBarControlConfig) => {
				this.toolBarVM?.appendConfig(d);
				return "#toolBarSuffix";
			}
		});
		this.screenService.registerDataComponent("LongRunData", new LongRunDataComponent(this.eventAggregator, this.screenService));
		this.screenService.registerDataComponent("QuickProcessingData", new QuickProcessingDataComponent(this.eventAggregator));

	}

	screenIsDirty(): boolean {
		if (this.viewModel) return this.viewModel.isDirty;
		return false;
	}

	applyScreenPreferences(result: IScreenPreferences, mode: "user" | "def"): void {
		for (const cid in result) {
			this.preferencesService.setPreferences(cid, result[cid], mode);
		}
	}

	applyMenuResult(result: any): void {
		let name = this.viewModel.screenID;
		// eslint-disable-next-line @typescript-eslint/no-magic-numbers
		name = `${name.substring(0, 2)}.${name.substring(2, 4)}.${name.substring(4, 6)}.${name.substring(6, 8)}`;
		if (this.viewModel?.isCustomized()) name = `CST.${name}`;

		const sid: IMenuItem = {
			type: "",
			id: "screenID",
			text: `<div class='qp-menu-label'>${this.Msg.ScreenID}</div><span class='size-sm qp-menu-value'>${name}</span>`,
		};
		result.options.unshift(sid);
		this.toolsMenu = result;
	}
	applyCustomizationResult(result: any): void {
		this.customizationMenu = result;
	}
	applyNotesResult(result: any): void {
		this.notesMenu = result;
	}

	applyCaptionResult(result: any): void {
		this.caption = {
			title: "<No title>",
			caption: "",
			href: "",
			hidden: false,
			id: "screen-title",
			name: "screenTitle",
			tabIndex: -1,
			propsTemplate: "",
			hasCaption: false,
			isFavoritable: false,
			isFavorite: false,
		};
		if (result) this.caption = { ...this.caption, ...result };

		if (this.caption.href?.length) {
			this.caption.href = this.redirectHelper.getAbsoluteUrl(this.caption.href, false);
		}
		this.caption.caption = this.viewModel?.getCaptionText();
		this.caption.rowErrors = this.viewModel?.getRowErrors();
		if (this.caption.caption) this.caption.hasCaption = true;
	}

	getQueryParams(): IDataComponentParams {
		return {};
	}

	openDialog(msg: OpenPopupEvent) {
		this.dialogHelper.openDialog({
			templateId: msg.name,
			context: this.viewModel,
			rootElement: this.contentElement,
			command: msg.commandName,
			autoRepaint: msg.autoRepaint
		}, {
			overlayDismiss: false
		}).whenClosed(() => undefined);
	}

	serializeParameters(p: { [key: string]: string }): string {
		let res = "";
		for (const key in p) {
			res += `&${key}=${p[key]}`;
		}
		return res;
	}

	private async attached() {
		const parts = window.location.search.split("&");
		if (parts[0][0] === "?") parts[0] = parts[0].substr(1);

		const params: { [key: string]: string } = {};
		let forceUI = null;
		let isRedirect = null;
		let isSidePanel = false;
		for (const part of parts) {
			const idx = part.indexOf("=");
			if (idx < 0) continue;
			const key = part.substr(0, idx);
			const value = part.substr(idx + 1);

			switch (key) {
				case "ScreenId":
				case "unum":
				case "HideScript":
				case "timeStamp":
					break;
				case "PopupPanel":
				case "id":
					if (value) isRedirect = true;
					if (value === "Layer") isSidePanel = true;
					break;
				case "InLayer":
					if (value === "On") isSidePanel = true;
					break;
				case "isRedirect":
					isRedirect = Boolean(value);
					break;
				case "ui":
				case "UI":
					forceUI = value;
					break;
				default:
					params[key] = value;
			}
		}

		const name = getScreenIdFromUrl();
		if (!name) this.screenName = "not-found";
		else {
			this.screenName = getScreenPath(name);
		}

		if (forceUI || (<any>window).isRedirect || (<any>window.parent)?.isRedirect || isRedirect) {
			this.forceUI = forceUI;
			const screenApiClientSettings = this.container.get(ScreenApiClientSettings);
			screenApiClientSettings.forceUI = forceUI;
			screenApiClientSettings.dueToRedirect = (<any>window).isRedirect || (<any>window.parent)?.isRedirect || isRedirect;

			delete (<any>window).isRedirect;
			delete (<any>window.parent)?.isRedirect;
		}

		let mainHref = window.top.location.href;

		const queryParamSplitterIdx = mainHref.indexOf('?');
		if (queryParamSplitterIdx >= 0) mainHref = mainHref.substr(0, queryParamSplitterIdx);
		this.baseHref = `${mainHref}?`;
		if (forceUI) this.baseHref += `ui=${forceUI}&`;

		if (window.frameElement) {
			if (name !== this.giScreenID) {
				mainHref = `${this.baseHref}ScreenId=${name}${this.serializeParameters(params)}`;
				if (!isSidePanel) window.top.history.replaceState({}, "Title", mainHref);
			}
		}

		let ready = false;

		this.screenEventManager.subscribe("OpenPopup", (message: OpenPopupEvent) => {
			message.stop();
			this.openDialog(message);
		});
		this.screenEventManager.subscribe("ExecuteCommand", (evt: ExecuteCommandEvent) => {
			if (evt.Command) {
				this.screenService.executeCommand(evt.Command, evt.Params);
				evt.stop();
			}
		});
		this.eventSubscriptions.push(this.screenEventManager.subscribe("RefreshScreen", (message: RefreshScreenEvent) => {
			this.screenService.update();
		}));

		const ea = this.eventAggregator;
		this.eventSubscriptions.push(
			ea.subscribe("screen-initialize-data-ready", () => {
				this.viewModel.setupParameters(params);
			})
		);

		this.eventSubscriptions.push(
			ea.subscribe("screen-initialized", (data: any) => {
				if (this.viewModel?.screenID === data?.screenID) ready = true;
				if (data?.failure) this.initialized = true;
			})
		);

		this.eventSubscriptions.push(
			ea.subscribe("screen-updated", (data: any) => {
				if (ready && this.viewModel?.screenID === data?.screenID) {
					this.initialized = true;
					const screenId =  this.viewModel.siteMapScreenID ? this.viewModel.siteMapScreenID : this.viewModel.screenID;

					let mainHref = `${this.baseHref}ScreenId=${screenId}`;
					if (!this.viewModel.isNewEntry()) {
						const params = this.viewModel.getKeys();
						mainHref += this.serializeParameters(params);
					}
					window.top.history.replaceState({}, "Title", mainHref);
				}
			})
		);

		this.eventSubscriptions.push(
			ea.subscribe(
				PopupMessageOpenEvent,
				(eventArgs: PopupMessageOpenEvent) => {
					const model = {
						title: this.Msg.Note,
						message: eventArgs.popupMessage,
					};
					this.alertServce.openAlert(model).then((result) => undefined);
				}
			)
		);

		this.eventSubscriptions.push(this.eventManager.addEventListener(document.body, "startScreenConfiguration", () => {
			this.editMode = true;
		}, delegationStrategy.none, true));

		this.eventSubscriptions.push(this.eventManager.addEventListener(document.body, "endScreenConfiguration", () => {
			this.editMode = false;
		}, delegationStrategy.none, true));

		this.keyboardService.setActiveArea(this.contentElement);
		return;
	}

	private detached(): void {
		this.eventSubscriptions.forEach((s) => s.dispose());
	}
}
