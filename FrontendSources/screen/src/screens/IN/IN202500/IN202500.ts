import {
	createSingle, PXActionState, graphInfo, viewInfo, PXScreen
} from "client-controls";
import { InventoryItem } from './views';

@graphInfo({ graphType: 'PX.Objects.IN.InventoryItemMaint', primaryView: 'Item' })
export class IN202500 extends PXScreen {
	backAction: PXActionState;
	@viewInfo({ displayName: 'Inventory Item' })
	Item = createSingle(InventoryItem);
}

