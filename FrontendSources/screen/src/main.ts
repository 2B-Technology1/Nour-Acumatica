import '../static/font-awesome.css';
import '../static/styles.css';
import '../static/basic-layout.css';
import '../static/custom.css';
import { bootstrap } from 'aurelia-bootstrapper';
import { Aurelia, PLATFORM, TemplatingEngine, TaskQueue, DOM } from 'aurelia-framework';
import * as env from '../config/environment.json';
import { disableConnectQueue } from 'aurelia-binding';
import { HttpClient } from 'aurelia-fetch-client';
import { IScreenModel, DEFAULT_HTTP_CONFIGURATION, BASE_PATH } from 'client-controls';
import { HttpClientConfiguration } from 'aurelia-fetch-client';
const environment = env;

// eslint-disable-next-line @typescript-eslint/naming-convention
declare const px_cm: any;

const nativeAlert = window.alert;
window.alert = function (msg) {
	nativeAlert(msg);
};

const nativeConfirm = window.confirm;
window.confirm = function (msg) {
	return nativeConfirm(msg);
};


disableConnectQueue();

const focusPoucus = function (e: Element | null) {
	const id = e instanceof Element && e.getAttribute('id') || undefined;
	return {
		element: e,
		id: id
	};
};

let focusedElementBeforeAU: { element: Element | null; id: string | undefined } = focusPoucus(document.activeElement);
if (focusedElementBeforeAU.element && focusedElementBeforeAU.element.tagName === "BODY") {
	// not yet selected by PX
	document.addEventListener('focus', (e) => {
		focusedElementBeforeAU = focusPoucus(e.target as Element);
	}, { capture: true, once: true });
}

const configureEnhance = async function (au: Aurelia) {

	const globalFeatures = PLATFORM.global.globalControlsModules;
	const minDialogZIndex = 10000;
	const dialogConfig = (config: any) => {
		config.useDefaults();
		config.settings.showCloseButton = true;
		// config.settings.lock = false;
		config.settings.startingZIndex = minDialogZIndex;
		config.settings.mouseEvent = 'mousedown';
	};

	au.use
		.basicConfiguration()
		.defaultResources()
		.plugin(PLATFORM.moduleName('aurelia-ui-virtualization'))
		.plugin(PLATFORM.moduleName('aurelia-dialog'), dialogConfig)
		.plugin(PLATFORM.moduleName('aurelia-portal-attribute'))
		.feature(PLATFORM.moduleName('client-controls/plugins/dependency-property-injection/index'))
		.feature(PLATFORM.moduleName('client-controls/plugins/glue/index'))
		.feature(PLATFORM.moduleName('resources/index'))
		.feature(PLATFORM.moduleName("client-controls/plugins/enhance-n-watch/index"))
		.feature(PLATFORM.moduleName('client-controls/plugins/happeded-on-body/index'))
		.feature(PLATFORM.moduleName('client-controls/services/hover-activator-service/index'));

	au.use.developmentLogging(environment.debug ? 'debug' : 'warn');

	await au.start();

	const path = PLATFORM.global.location.pathname;
	const root = `${path.substring(0, path.indexOf('/(W('))}/`;
	au.container.registerInstance(BASE_PATH, root);
	const defaultConfiguration = (config: HttpClientConfiguration) =>
		config
			.withBaseUrl(root)
			.withDefaults({
				headers: { 'Accept': 'application/json', 'X-Requested-With': 'Fetch' },
				credentials: 'same-origin',
			});

	au.container.registerInstance(DEFAULT_HTTP_CONFIGURATION, defaultConfiguration);
	const client = au.container.get(HttpClient);
	client.configure(defaultConfiguration);

	const screen: IScreenModel = {
		elements: {},
		values: {}
	};

	if (environment.debug) PLATFORM.global.fauxScreen = screen;

	await (au as any).enhanceAndWatch(screen);

	PLATFORM.global.getViewModelById = function(id: string) {
		if (!screen.elements[id]) {
			return undefined;
		}
		else {
			let elem: any = document.getElementById(id);
			if (elem) {
				do {
					if (elem.au) {
						return elem.au.controller.viewModel;
					}
					elem = elem.parentNode;
				} while (elem);
				return undefined;
			}
		}
		return undefined;
	};

	if (focusedElementBeforeAU.id) {
		const elem = DOM.getElementById(focusedElementBeforeAU.id) as HTMLElement;
		if (elem) {
			elem.focus();
		}
	}
	else if (focusedElementBeforeAU.element instanceof HTMLElement && focusedElementBeforeAU.element.parentNode != null) {
		if (focusedElementBeforeAU.element !== document.activeElement) {
			focusedElementBeforeAU.element.focus();
		}
	}
	if (PLATFORM.global.px_cm) {
		px_cm.handleLoad();
		if (px_cm.auEnhanceCompleted) px_cm.auEnhanceCompleted();
		if (px_cm.requiredReloadPage(false)) px_cm.reloadPage(true);

		px_cm.au_flush = function () {
			const queue: TaskQueue = au.container.get(TaskQueue);
			if (!queue.flushing) {
				queue.flushMicroTaskQueue();
			}
		};

		px_cm.au_enhance = function (element: HTMLElement) {
			const engine = au.container.get(TemplatingEngine);
			const view = engine.enhance({
				element: element,
				container: au.container, resources: au.resources, bindingContext: screen
			});
			px_cm.au_flush();
			return view;
		};

		const q = px_cm.AureliaEnhanceAwaitQueue as Array<Function>;
		if (q) {
			for (const func of q) {
				func();
			}
			q.splice(0, q.length);
		}
	}
};

const x = document.querySelector('body');
const noEnhance = x?.hasAttribute('aurelia-app');
if (!noEnhance) {
	const configureEnhanceDelay = 2500;
	setTimeout(() => {
		bootstrap(configureEnhance);
	}, configureEnhanceDelay);
}

export function configure(aurelia: Aurelia): void {
	const minDialogZIndex = 10000;
	const dialogConfig = (config: any) => {
		config.useDefaults();
		config.settings.showCloseButton = true;
		config.settings.startingZIndex = minDialogZIndex;
		config.settings.mouseEvent = 'mousedown';
	};

	aurelia.use
		.standardConfiguration()
		.plugin(PLATFORM.moduleName('aurelia-ui-virtualization'))
		.plugin(PLATFORM.moduleName('aurelia-dialog'), dialogConfig)
		.plugin(PLATFORM.moduleName('aurelia-portal-attribute'))
		.plugin(PLATFORM.moduleName('aurelia-animator-css'), cfg => cfg.useAnimationDoneClasses = true)
		.feature(PLATFORM.moduleName('client-controls/plugins/dependency-property-injection/index'))
		.feature(PLATFORM.moduleName('client-controls/plugins/glue/index'))
		.feature(PLATFORM.moduleName('resources/index'))
		.feature(PLATFORM.moduleName('client-controls/controls/compound/rich-text-editor/index'))
		.feature(PLATFORM.moduleName('client-controls/plugins/happeded-on-body/index'));

	const client = aurelia.container.get(HttpClient);
	const path = PLATFORM.global.location.pathname;
	const root = path.substring(0, path.toLowerCase().indexOf('scripts/'));
	aurelia.container.registerInstance(BASE_PATH, root);
	const defaultConfiguration = (config: HttpClientConfiguration) =>
		config
			.withBaseUrl(root)
			.withDefaults({
				headers: { 'Accept': 'application/json,text/html', 'X-Requested-With': 'Fetch' },
				credentials: 'same-origin',
			});
	aurelia.container.registerInstance(DEFAULT_HTTP_CONFIGURATION, defaultConfiguration);
	client.configure(defaultConfiguration);

	PLATFORM.moduleName('aurelia-logging-console');
	aurelia.use.developmentLogging(environment.debug ? 'debug' : 'warn');

	aurelia.start().then(() => aurelia.setRoot(PLATFORM.moduleName('app')));
}
