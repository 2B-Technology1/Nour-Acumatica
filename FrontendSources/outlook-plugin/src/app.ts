import { HttpClient } from 'aurelia-fetch-client';
import { autoinject, PLATFORM , DOM } from 'aurelia-framework';
import { Router, RouterConfiguration, Redirect, NavigationInstruction, Next } from 'aurelia-router';
import { updateCssVariables, ScreenApiClientSettings } from 'client-controls';
import { AuthenticationService } from './services/authentication-service';
import { Container } from 'aurelia-dependency-injection';

declare let Office: any;

@autoinject
export class App {
	router: Router;
	initialized = false;

	constructor(private container: Container, private authService: AuthenticationService, private client: HttpClient) {
		updateCssVariables(this.client);
	}

	configureRouter (config: RouterConfiguration, router: Router) {
		this.router = router;
		config.title = 'Acumatica';
		config.addAuthorizeStep(AuthorizeStep);

		config.map([ {
			route: ['', 'OU201000'], name: 'OU201000',
			moduleId: PLATFORM.moduleName('./screens/OU/OU201000/OU201000'),
			nav: true, title: 'Outlook plugin',
			layoutView: PLATFORM.moduleName('main-layout.html'),
			layoutViewModel: PLATFORM.moduleName('main-layout')
		},
		{ route: 'login', name: 'login', moduleId: PLATFORM.moduleName('./screens/Login/login', 'login'), title: 'Login' },
		{ route: 'first-run', name: 'first-run', moduleId: PLATFORM.moduleName('./first-run', 'first-run'), title: 'Welcome Page' },
		]);

		config.mapUnknownRoutes(PLATFORM.moduleName('not-found'));
		this.router = router;
	}

	async attached() {
		const screenApiClientSettings = this.container.get(ScreenApiClientSettings);
		screenApiClientSettings.clearSessionOnGetRequest = true;
		if (PLATFORM.global.Office === undefined) {
			this.initialized = true;
			return null;
		}

		let signalReady;
		const appReady = new Promise((r) => {
			signalReady = r;
		});

		Office.initialize = () => {
			signalReady();
			if (this.authService.isFirstRun) {
				this.initialized = true;
			}
			else {
				this.authService.signIn()
					.then(() => {
						this.initialized = true;
					})
					.catch(x => {
						this.initialized = true;
					});
			}
		};
		return appReady;
	}
}


@autoinject
class AuthorizeStep {
	constructor(private authService: AuthenticationService) {
	}

	run(navInstruction: NavigationInstruction, next: Next): Promise<any> {
		const needRedirect = this.authService.isFirstRun || !this.authService.authenticated;
		const redirectPage = this.authService.isFirstRun ? 'first-run' : 'login';
		const isRedirectPage = navInstruction.config.name === redirectPage;

		return (isRedirectPage || !needRedirect) ? next() : next.cancel(new Redirect(redirectPage));
	}
}
