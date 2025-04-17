import {
	SO301000
} from '../SO301000';

import {
	PXView,
	PXFieldState,
	PXFieldOptions,
	localizable,
	createSingle,
	selectorSettings,
	viewInfo,
	PXActionState
} from 'client-controls';

@localizable
export class QuickProcessPanelHeaders {
	static ProcessOrder = "Process Order";
}

export interface SO301000_QuickProcess extends SO301000 {}
export class SO301000_QuickProcess {
	QuickProcessPanelHeaders = QuickProcessPanelHeaders;

	@viewInfo({containerName: "Process Order"})
	QuickProcessParameters = createSingle(SOQuickProcessParameters);
}

export class SOQuickProcessParameters extends PXView {
	QuickProcessOk: PXActionState;

	@selectorSettings("INSite__SiteCD", "INSite__descr") SiteID: PXFieldState<PXFieldOptions.CommitChanges>;
	ShipDateMode: PXFieldState<PXFieldOptions.CommitChanges>;
	ShipDate: PXFieldState<PXFieldOptions.CommitChanges>;
	PrintWithDeviceHub: PXFieldState<PXFieldOptions.CommitChanges>;
	DefinePrinterManually: PXFieldState<PXFieldOptions.CommitChanges>;
	PrinterID: PXFieldState<PXFieldOptions.CommitChanges>;
	NumberOfCopies: PXFieldState;
	GreenStatus: PXFieldState;
	YellowStatus: PXFieldState;
	RedStatus: PXFieldState;
	AvailabilityMessage: PXFieldState;
	CreateShipment: PXFieldState<PXFieldOptions.CommitChanges>;
	PrintPickList: PXFieldState<PXFieldOptions.CommitChanges>;
	ConfirmShipment: PXFieldState<PXFieldOptions.CommitChanges>;
	PrintLabels: PXFieldState<PXFieldOptions.CommitChanges>;
	PrintConfirmation: PXFieldState<PXFieldOptions.CommitChanges>;
	UpdateIN: PXFieldState<PXFieldOptions.CommitChanges>;
	PrepareInvoiceFromShipment: PXFieldState<PXFieldOptions.CommitChanges>;
	PrepareInvoice: PXFieldState<PXFieldOptions.CommitChanges>;
	PrintInvoice: PXFieldState<PXFieldOptions.CommitChanges>;
	EmailInvoice: PXFieldState<PXFieldOptions.CommitChanges>;
	ReleaseInvoice: PXFieldState<PXFieldOptions.CommitChanges>;
}

