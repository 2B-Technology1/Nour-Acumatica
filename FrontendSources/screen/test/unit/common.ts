import { Aurelia, FrameworkConfiguration } from 'aurelia-framework';
import { initialize } from 'aurelia-pal-browser';
import { StageComponent } from 'aurelia-testing';
import { bootstrap } from 'aurelia-bootstrapper';
import { PLATFORM } from 'aurelia-pal';

export const beforeAllTests = () => {
	initialize();
	PLATFORM.global.ClientLocalizedStrings = PLATFORM.global.ClientLocalizedStrings || {}
};

export interface ICreateComponentOptions {
	resources: string | string[];
	html: string;
	bindingContext?: {}
	onBootstrap?(aurelia: Aurelia, configuration: FrameworkConfiguration): void;
}

export async function createComponent<T>(options: ICreateComponentOptions) {
	const component = StageComponent
		.withResources<T>(options.resources)
		.inView(options.html)
		.boundTo(options.bindingContext || { config: {} });
	component.bootstrap(function (aurelia) {
		const configuration = aurelia.use.basicConfiguration();
		if (options.onBootstrap) {
			options.onBootstrap(aurelia, configuration);
		}
		return aurelia.use
	});
	await component.create(bootstrap).catch((e) => expect(e).toBeNull());

	return component;
};

export const wait = async (delay: number) =>
	new Promise<void>(resolve => setTimeout(resolve, delay));
