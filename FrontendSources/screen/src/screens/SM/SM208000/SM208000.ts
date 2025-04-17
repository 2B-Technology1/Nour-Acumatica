import { autoinject } from 'aurelia-framework';
import {
	PXScreen, createSingle, createCollection, graphInfo, commitChanges, PXView,
	PXFieldState, PXFieldOptions
} from 'client-controls';

@graphInfo({ graphType: 'PX.Data.Maintenance.GI.GenericInquiryDesigner', primaryView: 'Designs' })
@autoinject
export class SM208000 extends PXScreen {
	Designs = createSingle(Designs);

	CurrentDesign = createSingle(CurrentDesign);

	Tables = createCollection(Tables, {
		adjustPageSize: true
	});

	Relations = createCollection(Relations, {
		adjustPageSize: true,
		columnsSettings: [
			{ field: 'ParentTable', fullState: true },
			{ field: 'JoinType', fullState: true },
			{ field: 'ChildTable', fullState: true },
		]
	});

	JoinConditions = createCollection(JoinConditions, { adjustPageSize: true });

	Results = createCollection(Results, { adjustPageSize: true });

	NavigationScreens = createCollection(NavigationScreens, {
		adjustPageSize: true,
	});

	NavigationParameters = createCollection(NavigationParameters, { adjustPageSize: true });
}

export class Designs extends PXView {
	@commitChanges Name: PXFieldState;
	SitemapTitle: PXFieldState;
}

export class CurrentDesign extends PXView {
	RowStyleFormula: PXFieldState;
	PrimaryScreenID: PXFieldState;
}

export class Tables extends PXView {
	Name: PXFieldState;
	Descrition: PXFieldState;
	Alias: PXFieldState;
	// eslint-disable-next-line id-denylist
	Number: PXFieldState<PXFieldOptions.Hidden>;
	DesignID: PXFieldState<PXFieldOptions.Hidden>;
}

export class Relations extends PXView {
	IsActive: PXFieldState;
	ParentTable: PXFieldState;
	JoinType: PXFieldState;
	ChildTable: PXFieldState;
	DesignID: PXFieldState<PXFieldOptions.Hidden>;
	LineNbr: PXFieldState<PXFieldOptions.Hidden>;
}

export class JoinConditions extends PXView {
	OpenBrackets: PXFieldState;
	ParentField: PXFieldState;
	Condition: PXFieldState;
	ChildField: PXFieldState;
	CloseBrackets: PXFieldState;
	Operation: PXFieldState;
	DesignID: PXFieldState<PXFieldOptions.Hidden>;
	LineNbr: PXFieldState<PXFieldOptions.Hidden>;
}

export class Results extends PXView {
	IsActive: PXFieldState;
	ObjectName: PXFieldState;
	Field: PXFieldState;
	SchemaField: PXFieldState;
	Width: PXFieldState;
	StyleFormula: PXFieldState;
	IsVisible: PXFieldState;
	DefaultNav: PXFieldState;
	NavigationNbr: PXFieldState;
	DesignID: PXFieldState<PXFieldOptions.Hidden>;
	LineNbr: PXFieldState<PXFieldOptions.Hidden>;
}

export class NavigationScreens extends PXView {
	IsActive: PXFieldState;
	Link: PXFieldState;
	WindowMode: PXFieldState;
	DesignID: PXFieldState<PXFieldOptions.Hidden>;
	LineNbr: PXFieldState<PXFieldOptions.Hidden>;
}

export class NavigationParameters extends PXView {
	FieldName: PXFieldState;
	ParameterName: PXFieldState;
	IsExpression: PXFieldState;
	DesignID: PXFieldState<PXFieldOptions.Hidden>;
	LineNbr: PXFieldState<PXFieldOptions.Hidden>;
	NavigationScreenLineNbr: PXFieldState<PXFieldOptions.Hidden>;
}
