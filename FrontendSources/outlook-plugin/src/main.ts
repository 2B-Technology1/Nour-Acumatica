import { configureMock } from './../test/unit/__mocks__/aurelia-fetch-client';
import { HttpClient, HttpClientConfiguration } from 'aurelia-fetch-client';
import '../static/font-awesome.css';
import '../static/styles.css';
import { Aurelia, PLATFORM } from 'aurelia-framework';
import * as env from '../config/environment.json';
import { BASE_PATH, configureGlue, configureHappenedOnBody, configurePropertyInjection, DEFAULT_HTTP_CONFIGURATION } from 'client-controls';
import { AuthenticationService, getHttpClientConfiguration } from './services/authentication-service';



const environment = env;

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
		.feature(configureGlue as any)
		.feature(configureHappenedOnBody as any)
		.feature(configurePropertyInjection as any)
		.feature(PLATFORM.moduleName('resources/index'));

	aurelia.use.developmentLogging(environment.debug ? 'debug' : 'warn');

	if (environment.testing) {
		aurelia.use.plugin(PLATFORM.moduleName('aurelia-testing'));
	}

	const path = PLATFORM.global.location.pathname;
	const root = path.substring(0, path.toLowerCase().indexOf('scripts/'));

	const defaultConfiguration = (config: HttpClientConfiguration) =>
		config
			.withBaseUrl(root)
			.withDefaults({
				headers: { 'Accept': 'application/json,text/html', 'X-Requested-With': 'Fetch' },
				credentials: 'same-origin',
			});

	aurelia.container.registerHandler(HttpClientConfiguration, () => defaultConfiguration(new HttpClientConfiguration()));

	const client = aurelia.container.get(HttpClient);
	client.configure(defaultConfiguration);

	aurelia.container.registerInstance(BASE_PATH, root);
	aurelia.container.registerInstance(DEFAULT_HTTP_CONFIGURATION,
		(config: HttpClientConfiguration) => {
			return getHttpClientConfiguration(aurelia.container.get(AuthenticationService), defaultConfiguration(config));
		});

	aurelia.start().then(() => aurelia.setRoot(PLATFORM.moduleName('app')));
}
