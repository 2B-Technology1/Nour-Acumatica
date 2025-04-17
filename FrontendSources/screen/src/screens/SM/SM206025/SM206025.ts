import { autoinject } from 'aurelia-framework';
import {
	PXScreen, createSingle, createCollection, graphInfo, commitChanges, PXView,
	PXFieldState, PXFieldOptions
} from 'client-controls';

@graphInfo({ graphType: 'PX.Api.SYImportMaint', primaryView: 'Mappings' })
@autoinject
export class SM206025 extends PXScreen {
	Mappings = createSingle(Mappings);

	FieldMappings = createCollection(FieldMappings, { adjustPageSize: true });
}

export class Mappings extends PXView {
	@commitChanges Name: PXFieldState;
	ScreenID: PXFieldState;
	ProviderID: PXFieldState;
	ProviderObject: PXFieldState;
}

export class FieldMappings extends PXView {
	IsActive: PXFieldState;
	ObjectName: PXFieldState;
	FieldName: PXFieldState;
	NeedCommit: PXFieldState;
	Value: PXFieldState;
	IgnoreError: PXFieldState;
	LineNbr: PXFieldState<PXFieldOptions.Hidden>;
	OrderNumber: PXFieldState;
	IsVisible: PXFieldState;
	ParentLineNbr: PXFieldState<PXFieldOptions.Hidden>;
	MappingID: PXFieldState<PXFieldOptions.Hidden>;
}
