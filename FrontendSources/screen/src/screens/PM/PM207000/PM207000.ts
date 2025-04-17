import { autoinject } from 'aurelia-framework';
import {
	PXScreen,
	createSingle,
	graphInfo,
	commitChanges,
	PXView,
	PXFieldState,
} from 'client-controls';

@graphInfo({ graphType: 'PX.Objects.PM.BillingMaint', primaryView: 'Billing' })
@autoinject
export class PM207000 extends PXScreen {
	Billing = createSingle(Billing);

	BillingRule = createSingle(BillingRule);
}

export class Billing extends PXView {
	@commitChanges BillingID: PXFieldState;
}

export class BillingRule extends PXView {
	QtyFormula: PXFieldState;
}
