/* eslint-disable @typescript-eslint/no-magic-numbers */
import {
	PXFieldState, PXErrorLevel, showInformer,
	HandlerDef,
	CellCssHandlerArgs, PXViewCollection,
	Handler,
	CustomEventType,
	handleEvent,
	ValueChangedHandlerArgs,
	RowCssHandlerArgs,
	RowSelectedHandlerArgs
} from 'client-controls';
import { SO301000 } from "../SO301000";
import { Messages } from "./messages";
import { SOLine, SOOrderHeader } from '../views';

// Uncomment next line (and do some changes in html) to run extension
// export interface SO301000_FieldsSample extends SO301000 { }

export class SO301000_FieldsSample {
	public msg = Messages;
	public newDescription: string = '';
	public sumState = new PXFieldState(undefined, 'SumSample', undefined, Messages.sumFieldName);
	public activeRowAmount: number = 0;
	Document: SOOrderHeader;
	Transactions: PXViewCollection<SOLine>;
	private oldDesc: string;
	private oldNbr: string;
	private prevOrderNbrId: string;
	private prevOrderTypeId: string;

	@handleEvent(CustomEventType.ValueChanged, { view: 'Document', field: 'OrderDesc', order: 1 })
	onDescriptionChange0(args: ValueChangedHandlerArgs) {
		console.log('handler called', args);
	}

	@handleEvent(CustomEventType.ValueChanged, { view: 'Document', field: 'OrderDesc', order: 2, manualHandler: true })
	onDescriptionChange(args: ValueChangedHandlerArgs, handlers: Array<() => void>) {
		if (handlers && handlers.length > 0) {
			console.log('calling handler', args);
			handlers[0]();
		}
		this.oldDesc = args.newValue;
		this.sumState.value = (this.oldDesc?.length || 0) +
			(this.oldNbr?.length || 0);
	}

	@handleEvent(CustomEventType.GetRowCss, { view: 'Transactions' })
	getTransactionsRowCss(args: RowCssHandlerArgs) {
		if (args?.selector?.rowIndex === 1) {
			// see static/custom.css
			return 'bold-row';
		}

		return undefined;
	}

	@handleEvent(CustomEventType.ValueChanged, { view: 'Document', field: 'CustomerOrderNbr' })
	onCustomerOrderNbrChange(args: ValueChangedHandlerArgs) {
		this.oldNbr = args.newValue;
		this.sumState.value = (this.oldDesc?.length || 0) +
			(this.oldNbr?.length || 0);
	}

	@handleEvent(CustomEventType.RowSelected, { view: 'Document' })
	onSalesOrderChange(args: RowSelectedHandlerArgs<SOOrderHeader>) {
		const largeOrderTotal = 10000;
		const doc = args.viewModel;
		const total = doc?.CuryOrderTotal?.value || 0;
		const orderTypeId = doc?.OrderType?.value?.id;
		const orderNumberId = doc?.OrderNbr?.value?.id;
		if (orderNumberId === this.prevOrderNbrId && orderTypeId === this.prevOrderTypeId) {
			return;
		}
		this.prevOrderNbrId = orderNumberId;
		this.prevOrderTypeId = orderTypeId;
		if (total >= largeOrderTotal) {
			showInformer(Messages.orderIsHuge, "error");
		}
	}

	@handleEvent(CustomEventType.RowSelected, { view: 'Transactions' })
	onCuryLineAmtChange(args: RowSelectedHandlerArgs<PXViewCollection<SOLine>>) {
		this.activeRowAmount = args.viewModel.activeRow?.CuryLineAmt?.value || 0;
	}

	@handleEvent(CustomEventType.GetCellCss, { view: 'Transactions', column: 'CuryLineAmt' })
	getTransactionsCellCss(args: CellCssHandlerArgs): string | undefined {
		if (args?.selector?.rowIndex === 0 &&
			args?.selector?.cellValue > 1000) {
			// see static/custom.css
			return 'red-cell';
		}

		return undefined;
	}

	public changeDescription() {
		this.Document.OrderDesc.value = this.newDescription;
	}

	public changeCurrentRowDescription() {
		const soLine = this.Transactions.activeRow;
		if (soLine) {
			soLine.TranDesc.value = `${soLine.TranDesc.value} plus`;
			this.Document.OrderDesc.value = this.newDescription;
			this.Transactions.activeRowChanged = true;
		}
	}

	public switchReadonly() {
		this.Document.CustomerLocationID.readOnly = !this.Document.CustomerLocationID.readOnly;
	}

	public addError() {
		this.Document.OrderDesc.errorLevel = PXErrorLevel.Error;
		this.Document.OrderDesc.error = Messages.errorText;
	}
}

