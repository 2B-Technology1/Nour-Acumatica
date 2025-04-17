import {
	PXScreen, createCollection, graphInfo, PXView,
	PXFieldState, PXFieldOptions
} from 'client-controls';

export class FAType extends PXView {
	AssetTypeID: PXFieldState<PXFieldOptions.CommitChanges>;
	Description: PXFieldState;
	IsTangible: PXFieldState<PXFieldOptions.CommitChanges>;
	Depreciable: PXFieldState<PXFieldOptions.CommitChanges>;
}

@graphInfo({ graphType: 'PX.Objects.FA.AssetTypeMaint', primaryView: 'AssetTypes' })
export class FA201010 extends PXScreen {
	AssetTypes = createCollection(FAType, { adjustPageSize: true, syncPosition: true });
}
