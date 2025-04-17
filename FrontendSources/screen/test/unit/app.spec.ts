jest.mock('aurelia-fetch-client',
() => ({
	HttpClient: class {
		configure = jest.fn()
		fetch = jest.fn(() => Promise.resolve())
	}
}));

import { App } from 'src/app';
import { ComponentTester } from 'aurelia-testing';
import { beforeAllTests, createComponent } from './common';

describe('Stage App Component', () => {
	let component: ComponentTester<App>;

	beforeAll(beforeAllTests);

	beforeEach(async function () {
		component = await createComponent<App>({
			resources: ['../../src/app',
				'client-controls/controls/compound/tool-bar/qp-tool-bar'],
			html: '<app></app>'
		});
	});

	afterEach(() => {
		component.dispose();
	});

	it('should render message', (done) => {
		const view = component.element;
		expect(view.textContent.trim()).toBe('Be patient...');
		done();
	});
});
