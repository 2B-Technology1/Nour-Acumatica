import {
	PXScreen, createCollection, graphInfo, PXView,
	PXFieldState,
	columnSettings
} from 'client-controls';

export class FARegister extends PXView {

	@columnSettings({ allowCheckAll: true, allowNull: false })
	Selected: PXFieldState;

	RefNbr: PXFieldState;

	DocDate: PXFieldState;

	@columnSettings({ allowNull: false })
	Origin: PXFieldState;

	DocDesc: PXFieldState;

	@columnSettings({ allowNull: false })
	Hold: PXFieldState;

	@columnSettings({ allowNull: false })
	IsEmpty: PXFieldState;
}

@graphInfo({ graphType: 'PX.Objects.FA.DeleteDocsProcess', primaryView: 'Docs' })
export class FA508000 extends PXScreen {
	Docs = createCollection(FARegister,
		{ adjustPageSize: true, syncPosition: true, mergeToolbarWith: 'ScreenToolbar' });
}
