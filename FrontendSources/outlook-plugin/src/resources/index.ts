import {FrameworkConfiguration, PLATFORM} from 'aurelia-framework';

export function configure(config: FrameworkConfiguration): void {
	config.globalResources([
		PLATFORM.moduleName('client-controls/controls/simple/icon/qp-icon'),
		PLATFORM.moduleName('client-controls/controls/simple/calendar/qp-calendar'),
		PLATFORM.moduleName('client-controls/controls/simple/button/qp-button'),
		PLATFORM.moduleName('client-controls/controls/simple/longrun-indicator/qp-longrun-indicator'),
		PLATFORM.moduleName('client-controls/controls/simple/text-editor/qp-text-editor'),
		PLATFORM.moduleName('client-controls/controls/simple/mask-editor/qp-mask-editor'),
		PLATFORM.moduleName('client-controls/controls/simple/text-editor/converter'),
		PLATFORM.moduleName('client-controls/controls/compound/grid/qp-grid'),
		PLATFORM.moduleName('client-controls/controls/compound/tool-bar/qp-tool-bar'),

		PLATFORM.moduleName('client-controls/controls/container/fieldset/qp-fieldset'),
		PLATFORM.moduleName("client-controls/controls/container/field/qp-field"),
		PLATFORM.moduleName("client-controls/controls/container/inline-field/qp-inline-field"),
		PLATFORM.moduleName("client-controls/controls/abstract/enhanced-compose/enhanced-compose"),
		PLATFORM.moduleName('client-controls/controls/abstract/screen-base-view-model/screen-base-view-model'),

		PLATFORM.moduleName('client-controls/controls/simple/check-box/qp-check-box'),
		PLATFORM.moduleName('client-controls/controls/dialog/informer/qp-informer-rack'),
		PLATFORM.moduleName('client-controls/controls/simple/date-editor/qp-datetime-edit'),
		PLATFORM.moduleName('client-controls/controls/simple/drop-down/qp-drop-down'),
		PLATFORM.moduleName('client-controls/controls/utility/tooltip/qp-tooltip'),
		PLATFORM.moduleName('client-controls/controls/simple/number-editor/qp-number-editor'),
		PLATFORM.moduleName('client-controls/controls/simple/selector/qp-selector'),
		PLATFORM.moduleName('client-controls/controls/utility/suggester/qp-editor-wrapper'),
		PLATFORM.moduleName('client-controls/controls/utility/suggester/elements/qp-suggester'),
		PLATFORM.moduleName('client-controls/controls/container/panel/qp-panel'),

		PLATFORM.moduleName('client-controls/controls/container/field-pair/qp-field-pair'),
		PLATFORM.moduleName('client-controls/controls/abstract/customization/customizable'),
		PLATFORM.moduleName('client-controls/controls/simple/caption/qp-caption'),
		PLATFORM.moduleName('client-controls/controls/simple/label/qp-label'),
	]);
}
