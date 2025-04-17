import { autoinject } from 'aurelia-framework';
import {
	PXScreen, createSingle, createCollection, graphInfo, commitChanges, autoRefresh, PXView,
	PXFieldState
} from 'client-controls';

@graphInfo({ graphType: 'PX.Objects.CT.TemplateMaint', primaryView: 'Templates' })
@autoinject
export class CT202000 extends PXScreen {
	Templates = createSingle(Templates);

	CurrentTemplate = createSingle(CurrentTemplate);

	Billing = createSingle(Billing);

	ContractDetails = createCollection(ContractDetails, {
		adjustPageSize: true
	});

	Contracts = createCollection(Contracts, {
		adjustPageSize: true
	});
}

export class Templates extends PXView {
	@commitChanges ContractCD: PXFieldState;
	Description: PXFieldState;
	Status: PXFieldState;
}

export class CurrentTemplate extends PXView {
	@commitChanges Type: PXFieldState;
	@commitChanges Duration: PXFieldState;
	@commitChanges DurationType: PXFieldState;
	@commitChanges Refundable: PXFieldState;
	@commitChanges RefundPeriod: PXFieldState;
	Days: PXFieldState;
	AutoRenew: PXFieldState;
	AutoRenewDays: PXFieldState;
	DaysBeforeExpiration: PXFieldState;
	GracePeriod: PXFieldState;
	@commitChanges CuryID: PXFieldState;
	EffectiveFrom: PXFieldState;
	DiscontinuedAfter: PXFieldState;
}

export class Billing extends PXView {
	Type: PXFieldState;
	InvoiceFormula: PXFieldState;
	TranFormula: PXFieldState;
}

export class ContractDetails extends PXView {
	@commitChanges @autoRefresh ContractItemID: PXFieldState;
	Description: PXFieldState;
	@commitChanges Qty: PXFieldState;
	BasePriceVal: PXFieldState;
	FixedRecurringPriceVal: PXFieldState;
	UsagePriceVal: PXFieldState;
	RenewalPriceVal: PXFieldState;
}

export class Contracts extends PXView {
	ContractCD: PXFieldState;
	CustomerID: PXFieldState;
	Status: PXFieldState;
	StartDate: PXFieldState;
	ExpireDate: PXFieldState;
	Description: PXFieldState;
}
