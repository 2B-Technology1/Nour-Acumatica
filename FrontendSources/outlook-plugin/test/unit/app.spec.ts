jest.mock('aurelia-fetch-client',
	() => ({
		HttpClient: class {
			configure = jest.fn()
			fetch = jest.fn(() => Promise.resolve())
		},
		HttpClientConfiguration: class {
			defaults: {}
		},
	}));
jest.mock('client-controls/plugins/css-theme/css-theme', () => ({
	updateCssVariables: jest.fn(),
}));

import { App } from 'src/app';
import { ComponentTester } from 'aurelia-testing';
import { beforeAllTests, createComponent } from './common';

describe('Stage App Component', () => {
	let component: ComponentTester<App>;

	beforeAll(beforeAllTests);

	beforeEach(async function () {
		component = await createComponent<App>({
			resources: '../../src/app',
			html: '<app></app>'
		});
	});

	afterEach(() => {
		component.dispose();
	});

	it('should render', async () => {
		const host = await component.waitForElement('.page-host', { timeout: 50 });
		expect(host).not.toBeNull();
	});
});
