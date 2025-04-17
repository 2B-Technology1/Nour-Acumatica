import { GL401000, GLHistoryEnqFilter, GLHistoryEnquiryResult } from '../GL401000';
import {
	PXFieldState, commitChanges, GridColumnShowHideMode, columnSettings,
	featureInstalled,
	placeAfterProperty,
	group
} from 'client-controls';

export interface GL401000_MultiCurrency extends GL401000 {}
@featureInstalled('PX.Objects.CS.FeaturesSet+Multicurrency')
export class GL401000_MultiCurrency  {
}

export interface GLHistoryEnqFilter_MultiCurrency extends GLHistoryEnqFilter { }
@featureInstalled('PX.Objects.CS.FeaturesSet+Multicurrency')
export class GLHistoryEnqFilter_MultiCurrency {
	@placeAfterProperty("SubCD") @group('column3') @commitChanges ShowCuryDetail: PXFieldState;
}

export interface GLHistoryEnquiryResult_MultiCurrency extends GLHistoryEnquiryResult { }
@featureInstalled('PX.Objects.CS.FeaturesSet+Multicurrency')
export class GLHistoryEnquiryResult_MultiCurrency {
	@placeAfterProperty("SignEndBalance") @columnSettings({ allowShowHide: GridColumnShowHideMode.Server })
	CuryID: PXFieldState;
	@placeAfterProperty("SignEndBalance") @columnSettings({ allowShowHide: GridColumnShowHideMode.Server })
	SignCuryBegBalance: PXFieldState;
	@placeAfterProperty("SignEndBalance") @columnSettings({ allowShowHide: GridColumnShowHideMode.Server })
	CuryPtdDebitTotal: PXFieldState;
	@placeAfterProperty("SignEndBalance") @columnSettings({ allowShowHide: GridColumnShowHideMode.Server })
	CuryPtdCreditTotal: PXFieldState;
	@placeAfterProperty("SignEndBalance") @columnSettings({ allowShowHide: GridColumnShowHideMode.Server })
	SignCuryEndBalance: PXFieldState;
	@placeAfterProperty("SignEndBalance") @columnSettings({ allowShowHide: GridColumnShowHideMode.Server })
	CuryPtdSaldo: PXFieldState;
}
