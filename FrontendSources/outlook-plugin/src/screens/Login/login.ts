import { autoinject, observable } from "aurelia-framework";
import { Router } from 'aurelia-router';
import { AuthenticationService } from "../../services/authentication-service";
import { LoginApiClient } from "../../services/login-api-client";
import { Messages } from '../../resources/messages'

@autoinject
export class Login
{
	companies: string[];
	locales: { Name: string, DisplayName: string }[];
	username: string;
	password: string;
	@observable	selectedCompany?: string
	selectedLocale?: string
	msg = Messages;

	constructor(private authService: AuthenticationService,
		private loginApiClient: LoginApiClient, private router: Router)
	{
	}

	attached()
	{
		this.loginApiClient.getCompanies().then(l => {
			this.companies = l;
			if (l[0]) this.getLocales(l[0]);
		});
	}

	selectedCompanyChanged(newValue, oldValue)
	{
		if (oldValue) this.getLocales(newValue);
	}

	getLocales(company: string)
	{
		this.loginApiClient.getLocalesFor(company).then(l => {
			this.locales = l;
			this.selectedLocale = this.authService.getLocale();
		});
	}

	async logIn()
	{
		await this.authService.logIn(this.username, this.password, this.selectedCompany, this.selectedLocale);
		if (this.authService.authenticated)
		{
			this.password = this.username = '';
			this.router.navigateToRoute(this.router.routes[0].name);
		}
	}
}
