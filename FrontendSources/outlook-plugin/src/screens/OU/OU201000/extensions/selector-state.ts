import { registerFieldState, PXFieldOptions, PXFieldState, PXView } from "client-controls";

@registerFieldState('simple-selector')
export class PXSimpleSelectorState<Options extends PXFieldOptions = any, DisplayName extends string = any>
	extends PXFieldState {
	constructor (viewViewModel?: PXView, fieldName?: string, options? : Options, displayName? : DisplayName) {
		super(viewViewModel, fieldName, options, displayName);
	}

	public configureControl(config: any) {
		if (config?.suggester) config.suggester.simpleSelect = true;
	}
}
