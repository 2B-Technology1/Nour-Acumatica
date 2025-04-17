import {
	PXScreen, createSingle, createCollection, graphInfo, localizable, PXActionState
} from 'client-controls';
import {
	FixedAsset,
	FADetails,
	FALocationHistory,
	FABookBalance,
	FAComponent,
	FAHistory,
	DeprBookFilter,
	FASheetHistory,
	FASetup,
	TranBookFilter,
	FATran,
	GLTranFilter,
	DsplFAATran,
	DisposeParams,
	SuspendParameters,
	ReverseDisposalInfo,
} from './views';

@graphInfo({ graphType: 'PX.Objects.FA.AssetMaint', primaryView: 'Asset' })
export class FA303000 extends PXScreen {
	Asset = createSingle(FixedAsset);
	AssetDetails = createSingle(FADetails);
	AssetLocation = createSingle(FALocationHistory);

	AssetBalance = createCollection(FABookBalance,
		{ syncPosition: true });

	AssetElements = createCollection(FAComponent,
		{ allowInsert: false, allowUpdate: false, allowDelete: false });

	deprbookfilter = createSingle(DeprBookFilter);
	fasetup = createSingle(FASetup);
	AssetHistory = createCollection(FAHistory,
		{ allowInsert: false, allowUpdate: false, allowDelete: false, generateColumns: 1, adjustPageSize: true });

	BookSheetHistory = createCollection(FASheetHistory,
		{ allowInsert: false, allowUpdate: false, allowDelete: false, generateColumns: 1, pagerMode: 0 });

	ViewDocument: PXActionState;
	ViewBatch: PXActionState;
	bookfilter = createSingle(TranBookFilter);
	FATransactions = createCollection(FATran,
		{ syncPosition: true, adjustPageSize: true });

	LocationHistory = createCollection(FALocationHistory,
		{ allowInsert: false, allowUpdate: false, allowDelete: false });

	ReduceUnreconCost: PXActionState;
	GLTrnFilter = createSingle(GLTranFilter);
	DsplAdditions = createCollection(DsplFAATran);

	DisposalOK: PXActionState;
	DispParams = createSingle(DisposeParams);

	SuspendParams = createSingle(SuspendParameters);

	RevDispInfo = createSingle(ReverseDisposalInfo);
}
