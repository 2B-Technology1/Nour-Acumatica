import { autoinject } from 'aurelia-framework';
import {
	PXScreen, createSingle, createCollection, graphInfo, commitChanges, PXView,
	PXFieldState, PXFieldOptions
} from 'client-controls';

@graphInfo({ graphType: 'PX.CS.RMRowSetMaint', primaryView: 'RowSet' })
@autoinject
export class CS206010 extends PXScreen {
	RowSet = createSingle(RowSet);
	Rows = createCollection(Rows, { adjustPageSize: true });
}

export class RowSet extends PXView {
	@commitChanges RowSetCode: PXFieldState;
	@commitChanges Type: PXFieldState;
	@commitChanges Description: PXFieldState;
}

export class Rows extends PXView {
	RowCode: PXFieldState;
	Description: PXFieldState;
	RowType: PXFieldState;
	Formula: PXFieldState;
	Format: PXFieldState;
	DataSourceID: PXFieldState;
	StyleID: PXFieldState;
	PrintControl: PXFieldState;
	Height: PXFieldState;
	Indent: PXFieldState;
	LineStyle: PXFieldState;
	SuppressEmpty: PXFieldState;
	HideZero: PXFieldState;
	LinkedRowCode: PXFieldState;
	BaseRowCode: PXFieldState;
	ColumnGroupID: PXFieldState;
	UnitGroupID: PXFieldState;
	RowNbr: PXFieldState<PXFieldOptions.Hidden>;
}
