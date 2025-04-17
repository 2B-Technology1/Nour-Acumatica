import { autoinject } from 'aurelia-dependency-injection';
import { PLATFORM } from 'aurelia-framework';
import { Router, Redirect } from 'aurelia-router';
import { HttpClient, json, HttpClientConfiguration } from 'aurelia-fetch-client';
import { showInformer } from 'client-controls';
import { HttpCodes, IRedirectData, IRedirectRequest, WindowMode } from 'client-controls/descriptors';
import { HttpClientFactory } from './http-client-factory';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
declare const Office: any;

@autoinject()
export class AuthenticationService {
	public authenticated = false
	public screenNotFound = false;
	outlookAuthToken: string;
	exceptionId: string;
	screenNotFoundExeption = "PXScreenNotFoundOrNotAccessibleException";

	private client: HttpClient;

	constructor(httpClientFactory: HttpClientFactory, private router: Router) {
		this.client = httpClientFactory.createHttpClient();
		this.client.configure(config => getHttpClientConfiguration(this, config));
	}

	setUserToken(token: string): void {
		this.outlookAuthToken = token;
	}

	get isFirstRun() : boolean {
		return !PLATFORM.global.localStorage.getItem('isOutlookVisited');
	}

	getLocale() : string {
		return PLATFORM.global.localStorage.getItem('acumaticaLocale');
	}

	setLocale(lang: string): void
	{
		PLATFORM.global.localStorage.setItem('acumaticaLocale', lang);
	}

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	async localizedStrings() : Promise<any> 	{
		return this.client.fetch('ui/Outlook/Strings').then(x => x.json());
	}

	async logOut(): Promise<Response> 	{
		return this.client.fetch('ui/Outlook/LogOut', { method: 'POST' });
	}

	lostAuth(preventException = false): void 	{
		this.authenticated = false;
		this.router.navigateToRoute('login');
		if (!preventException) throw new Error('Not authenticated');
	}

	async signIn(): Promise<unknown> 	{
		let signedIn;
		let failedToSignIn;
		const result = new Promise((resolve, reject) => { signedIn = resolve; failedToSignIn = reject; });

		Office.context.mailbox.getUserIdentityTokenAsync((asyncResult) =>
		{
			this.signInWithToken(asyncResult.value)
				.then(x => { signedIn(); })
				.catch(x => { failedToSignIn(); });
		});
		return result;
	}

	async signInWithToken(token: string): Promise<unknown> {
		this.outlookAuthToken = token;
		let signedIn;
		let failedToSignIn;
		const result = new Promise((resolve, reject) => { signedIn = resolve; failedToSignIn = reject; });

		const formData = new FormData();
		formData.append('token', this.outlookAuthToken);

		const path = PLATFORM.global.location.pathname;
		const page = path.substring(path.lastIndexOf(`/${  1}`));

		const lang = this.getLocale();
		let authAction = `Frames/AuthDock.ashx?_returnUrl_=${path}`;
		if (lang) authAction += `&_locale_=${  lang}`;

		this.client.fetch(authAction, { method: 'POST', body: formData })
			.then(async tokenAuthResult =>
			{
				if (tokenAuthResult.redirected) {
					const url = decodeURIComponent(tokenAuthResult.url);
					if (url.endsWith(page)) {
						this.authenticated = true;
						signedIn();
					}
					else {
						this.authenticated = false;
						if (url.toLocaleLowerCase().indexOf('login') > -1) {
							const target = new URL(url);
							this.exceptionId = target.searchParams.get('exceptionID');
						}
						failedToSignIn();
					}
				}
			});
		return result;
	}

	async logIn(username: string, password: string, company: string = null, locale: string = null): Promise<void>
	{
		if (!this.exceptionId && this.outlookAuthToken)
		{
			try { await this.signInWithToken(this.outlookAuthToken); } catch { }
		}

		return this.client.fetch(`ui/Outlook/LogIn?exceptionid=${this.exceptionId}`, {
			method: 'POST',
			body: json({ Login: username, Password: password, Company: company, Locale: locale })
		})
			.then((r) => {
				this.exceptionId = undefined;
				this.authenticated = !!r.ok;
				if (!this.authenticated) r.json().then(x => showInformer(x.ExceptionMessage, 'error'));
				else if (locale) this.setLocale(locale);
			});
	}

	async associateUser(username: string, password: string, company: string = null, locale: string = null): Promise<void>
	{
		this.client.fetch(`ui/Outlook/AssociateUser`, {
			method: 'POST',
			body: json({ Login: username, Password: password, Company: company, Locale: locale })
		})
		.then((r) =>
		{
			this.authenticated = !!r.ok;
		});
	}
}

export function getHttpClientConfiguration(authService: AuthenticationService, config: HttpClientConfiguration): HttpClientConfiguration {
	return config
		.withInterceptor({
			request(request: Request) {
				request.headers.set("Authorization", `Bearer ${authService.outlookAuthToken}`);
				return request;
			},
			async response(response: Response, request: Request) {
				const reqUrl = request.url.toLowerCase();
				if (reqUrl.indexOf('authdock') > -1 || reqUrl.indexOf('login') > -1) {
					return response;
				}

				if (response.redirected && response.url.toLowerCase().indexOf('login') > -1) {
					authService.lostAuth();
				}

				const r = response.clone();
				const contentType = r.headers.get("content-type");
				// tslint:disable: no-magic-numbers
				const respObj: any = (contentType && contentType.indexOf("application/json") !== -1)
					? await r.json()
					: null;

				if (response.status >= HttpCodes.BadRequest && response.status <= HttpCodes.ServerError) {
					if (response.status === HttpCodes.BadRequest || // TODO: 400 should ne removed when redirect problem will be solved
						response.status === HttpCodes.Forbidden ||
						response.status === HttpCodes.NotFound ||
						respObj?.ExceptionType?.indexOf(authService.screenNotFoundExeption) >= 0) {
						authService.screenNotFound = true;
					}
					throw new Error(respObj?.title || undefined);
				}
				else if (response.status === HttpCodes.Found || respObj?.diffType === "redirect") {
					const redirectObj: IRedirectData = respObj as IRedirectData;
					if (redirectObj?.redirects.findIndex((redirectResult: IRedirectRequest) => redirectResult.settings.mode === WindowMode.Same
						&& redirectResult.url.indexOf("Frames/Outlook/FirstRun.html") > -1) > -1) {
						(<any>response).cancelled = true;
						authService.lostAuth(true);
					}
				}

				// tslint:enable: no-magic-numbers
				return response;
			}
		});
}
