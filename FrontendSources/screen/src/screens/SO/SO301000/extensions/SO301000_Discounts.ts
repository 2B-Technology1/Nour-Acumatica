import {
	SO301000
} from '../SO301000';

import {
	PXView,
	createCollection,
	PXFieldState,
	PXFieldOptions,
	localizable,
	featureInstalled,
	viewInfo
} from 'client-controls';
import { SO301000_Approvals } from './SO301000_Approvals';

@localizable
export class DiscountsTabHeaders {
	static Discounts = "Discounts";
}

export interface SO301000_Discounts extends SO301000, SO301000_Approvals {}
@featureInstalled('PX.Objects.CS.FeaturesSet+CustomerDiscounts')
export class SO301000_Discounts {
	DiscountsTabHeaders = DiscountsTabHeaders;

	@viewInfo({containerName: "Discounts"})
	DiscountDetails = createCollection(
		SODiscountDetails,
		{
			syncPosition: true,
		}
	);
}


export class SODiscountDetails extends PXView {
	SkipDiscount: PXFieldState;
	DiscountID: PXFieldState<PXFieldOptions.CommitChanges>;
	DiscountSequenceID: PXFieldState<PXFieldOptions.CommitChanges>;
	Type: PXFieldState<PXFieldOptions.Disabled>;

	IsManual: PXFieldState;
	CuryDiscountableAmt: PXFieldState;
	DiscountableQty: PXFieldState;
	CuryDiscountAmt: PXFieldState<PXFieldOptions.CommitChanges>;
	DiscountPct: PXFieldState<PXFieldOptions.CommitChanges>;

	FreeItemID: PXFieldState;
	FreeItemQty: PXFieldState;
	ExtDiscCode: PXFieldState;
	Description: PXFieldState;
}

