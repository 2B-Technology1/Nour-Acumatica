import {
	PXScreen, createCollection, graphInfo, PXView,
	PXFieldState, PXFieldOptions
} from 'client-controls';

export class FABook extends PXView {
	BookCode: PXFieldState;
	Description: PXFieldState;
	UpdateGL: PXFieldState<PXFieldOptions.CommitChanges>;
	MidMonthType: PXFieldState;
	MidMonthDay: PXFieldState;
}

@graphInfo({ graphType: 'PX.Objects.FA.BookMaint', primaryView: 'Book' })
export class FA205000 extends PXScreen {
	Book = createCollection(FABook, { adjustPageSize: true, syncPosition: true });
}
