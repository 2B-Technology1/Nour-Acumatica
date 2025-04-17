import { autoinject } from "aurelia-framework";
import { HttpClient } from 'aurelia-fetch-client'

const apiRoot = 'ui/'
const companyRoot = apiRoot + 'company'
@autoinject
export class LoginApiClient
{
	constructor(private client: HttpClient) {	}

	async getCompanies()
	{
		return this.client.fetch(`${companyRoot}`).then(x => x.json());
	}

	async getLocalesFor(company: string): Promise<{ Name: string, DisplayName: string }[]>
	{
		return this.client.fetch(`${companyRoot}/${company}/locale`).then(x => x.json());
	}
}
