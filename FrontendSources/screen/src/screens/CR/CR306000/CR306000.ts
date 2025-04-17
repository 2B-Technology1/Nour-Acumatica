import { autoinject } from 'aurelia-framework';
import {
	PXView, PXFieldState, graphInfo, PXScreen, createSingle, localizable
} from "client-controls";

@localizable
class TabHeaders {
	static Details = "Details";
}

export class CRCase extends PXView {
	CaseCD: PXFieldState;
	ARRefNbr: PXFieldState;
	CaseClassID: PXFieldState;
	ContactID: PXFieldState;
	ContractID: PXFieldState;
	CreatedByID: PXFieldState;
	CustomerID: PXFieldState;
	LastModifiedByID: PXFieldState;
	LocationID: PXFieldState;
	OwnerID: PXFieldState;
	WorkgroupID: PXFieldState;
	Subject: PXFieldState;
	Description: PXFieldState;
}

@graphInfo({ graphType: 'PX.Objects.CR.CRCaseMaint', primaryView: 'Case' })
@autoinject
export class CR306000 extends PXScreen {
	TabHeaders = TabHeaders;

	Case = createSingle(CRCase);
}
