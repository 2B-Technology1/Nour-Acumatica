import { HttpClient, HttpClientConfiguration, RequestInit } from 'aurelia-fetch-client';
import { inject } from 'client-controls/dependency-property-injection';

@inject(HttpClientConfiguration)
export class HttpClientFactory {

	constructor(private defaults: HttpClientConfiguration) {
	}

	public createHttpClient() {
		const instance = new HttpClient();
		instance.configure(()=>this.defaults);
		return instance;
	}
}
