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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.IN;
using PX.Objects.PO;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class DropshipReturn : PXGraphExtension<SOOrderEntry>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.dropShipments>();

		public PXAction<SOOrder> createVendorReturn;
		[PXUIField(DisplayName = "Create Vendor Return", MapEnableRights = PXCacheRights.Select)]
		[PXButton(CommitChanges = true)]
		protected virtual IEnumerable CreateVendorReturn(PXAdapter adapter)
		{
			List<SOOrder> list = adapter.Get<SOOrder>().ToList();
			Base.Save.Press();
			var currentDoc = Base.Document.Current;

			PXLongOperation.StartOperation(Base, () =>
			{
				var receiptGraph = PXGraph.CreateInstance<POReceiptEntry>();
				var dropshipReturnExt = receiptGraph.FindImplementation<PO.GraphExtensions.POReceiptEntryExt.DropshipReturn>();
				var receiptList = new DocumentList<POReceipt>(receiptGraph);

				dropshipReturnExt.CreatePOReturn(currentDoc, receiptList);

				if (receiptList.Count == 0)
				{
					throw new PXException(PO.Messages.NoLinesForVendorReturn);
				}
			});

			return list;
		}
	}
}
