import {
	SO301000
} from '../SO301000';

import {
	PXView,
	createCollection,
	PXFieldState,
	viewInfo,
	localizable
} from 'client-controls';


export interface SO301000_Risks extends SO301000 {}
export class SO301000_Risks {
	@viewInfo({containerName: "Risks"})
	OrderRisks = createCollection(OrderRisks,
		{
			syncPosition: true,
			adjustPageSize: true,
			allowInsert: false,
			allowDelete: false,
			allowUpdate: false,
		}
	);
}

export class OrderRisks extends PXView {
	LineNbr: PXFieldState;
	Score: PXFieldState;
	Recommendation: PXFieldState;
	Message: PXFieldState;
}
