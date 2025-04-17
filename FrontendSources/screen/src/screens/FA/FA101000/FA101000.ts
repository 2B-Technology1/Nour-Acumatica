import {
	PXScreen, createSingle, graphInfo, PXView,
	PXFieldState, PXFieldOptions
} from 'client-controls';

export class FASetup extends PXView {
	FAAccrualAcctID: PXFieldState<PXFieldOptions.CommitChanges>;
	FAAccrualSubID: PXFieldState<PXFieldOptions.CommitChanges>;
	ProceedsAcctID: PXFieldState<PXFieldOptions.CommitChanges>;
	ProceedsSubID: PXFieldState<PXFieldOptions.CommitChanges>;
	DeprHistoryView: PXFieldState;
	DepreciateInDisposalPeriod: PXFieldState;
	AccurateDepreciation: PXFieldState;
	ReconcileBeforeDisposal: PXFieldState;
	AllowEditPredefinedDeprMethod: PXFieldState;
	RegisterNumberingID: PXFieldState;
	AssetNumberingID: PXFieldState;
	BatchNumberingID: PXFieldState;
	TagNumberingID: PXFieldState;
	CopyTagFromAssetID: PXFieldState;
	AutoReleaseAsset: PXFieldState;
	AutoReleaseDepr: PXFieldState;
	AutoReleaseDisp: PXFieldState;
	AutoReleaseTransfer: PXFieldState;
	AutoReleaseReversal: PXFieldState;
	AutoReleaseSplit: PXFieldState;
	UpdateGL: PXFieldState;
	AutoPost: PXFieldState;
	SummPost: PXFieldState;
	SummPostDepreciation: PXFieldState;
}

@graphInfo({ graphType: 'PX.Objects.FA.SetupMaint', primaryView: 'FASetupRecord' })
export class FA101000 extends PXScreen {
	FASetupRecord = createSingle(FASetup);
}
