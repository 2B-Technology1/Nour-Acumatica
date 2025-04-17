import { autoinject, PLATFORM } from 'aurelia-framework';
import { Router } from 'aurelia-router';
import { AuthenticationService } from "./services/authentication-service";

declare var Office: any

@autoinject
export class FirstRun
{
	constructor(private authService: AuthenticationService, private router: Router)
	{
	}

	onLoginButtonClick()
	{
		Office.context.mailbox.getUserIdentityTokenAsync((asyncResult) =>
		{
			this.authService.setUserToken(asyncResult.value);
			PLATFORM.global.localStorage.setItem('isOutlookVisited', '1');
			this.router.navigateToRoute('login');
		});
	}
}
