import {
	SO301000
} from '../SO301000';

import {
	PXView,
	createCollection,
	PXFieldState,
	PXFieldOptions,
	localizable,
	featureInstalled,
	GridColumnGeneration,
	createSingle,
	viewInfo
} from 'client-controls';

@localizable
export class MatrixItemsTabHeaders {
	static AddMatrixItemMatrixView = "Add Matrix Item: Matrix View";
}

export interface SO301000_MatrixItems extends SO301000 {}
@featureInstalled('PX.Objects.CS.FeaturesSet+MatrixItem')
export class SO301000_MatrixItems {
	MatrixItemsTabHeaders = MatrixItemsTabHeaders;
	Header = createSingle(AddMatrixItemHeader);

	@viewInfo({containerName: "Add Matrix Item"})
	AdditionalAttributes = createCollection(
		MatrixAttributes,
		{
			syncPosition: true,
			allowInsert: false,
			allowDelete: false,
			generateColumns: GridColumnGeneration.Recreate,
		});

	@viewInfo({containerName: "Add Matrix Item"})
	Matrix = createCollection(
		EntryMatrix,
		{
			syncPosition: true,
			allowInsert: false,
			allowDelete: false,
			generateColumns: GridColumnGeneration.Recreate,
		});
}

export class AddMatrixItemHeader extends PXView {
	TemplateItemID: PXFieldState<PXFieldOptions.CommitChanges>;
	ColAttributeID: PXFieldState<PXFieldOptions.CommitChanges>;
	RowAttributeID: PXFieldState<PXFieldOptions.CommitChanges>;
	ShowAvailable: PXFieldState<PXFieldOptions.CommitChanges>;
	SiteID: PXFieldState<PXFieldOptions.CommitChanges>;
}

export class MatrixAttributes extends PXView {
}

export class EntryMatrix extends PXView {
}

