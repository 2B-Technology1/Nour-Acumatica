/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using System;
using System.Collections.Generic;

using PX.Data;
using PX.BarcodeProcessing;

namespace PX.Objects.IN.WMS
{
	public static class AllWMSRedirects
	{
		public static IEnumerable<ScanRedirect<TScanExt>> CreateFor<TScanExt>()
			where TScanExt : PXGraphExtension, IBarcodeDrivenStateMachine
		{
			return new ScanRedirect<TScanExt>[]
			{
				new StoragePlaceLookup.RedirectFrom<TScanExt>(),
				new InventoryItemLookup.RedirectFrom<TScanExt>(),

				new INScanIssue.RedirectFrom<TScanExt>(),
				new INScanReceive.RedirectFrom<TScanExt>(),
				new INScanTransfer.RedirectFrom<TScanExt>(),
				new INScanCount.RedirectFrom<TScanExt>(),

				new PO.WMS.ReceivePutAway.ReceiveMode.RedirectFrom<TScanExt>(),
				new PO.WMS.ReceivePutAway.ReturnMode.RedirectFrom<TScanExt>(),
				new PO.WMS.ReceivePutAway.PutAwayMode.RedirectFrom<TScanExt>(),

				new SO.WMS.PickPackShip.PickMode.RedirectFrom<TScanExt>(),
				new SO.WMS.PickPackShip.PackMode.RedirectFrom<TScanExt>(),
				new SO.WMS.PickPackShip.ShipMode.RedirectFrom<TScanExt>(),
				new SO.WMS.PickPackShip.ReturnMode.RedirectFrom<TScanExt>(),
			};
		}
	}
}