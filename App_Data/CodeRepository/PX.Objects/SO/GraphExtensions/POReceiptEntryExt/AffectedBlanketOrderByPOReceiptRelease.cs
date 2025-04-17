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
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.Objects.SO.DAC.Projections;

namespace PX.Objects.SO.GraphExtensions.POReceiptEntryExt
{
	public class AffectedBlanketOrderByPOReceiptRelease : AffectedBlanketOrderByChildOrders<AffectedBlanketOrderByPOReceiptRelease, POReceiptEntry>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();

		protected virtual void _(Events.RowUpdated<SOLine4> e)
		{
			if (!string.IsNullOrEmpty(e.Row.BlanketNbr) && !e.Cache.ObjectsEqual<SOLine4.shippedQty, SOLine4.completed>(e.Row, e.OldRow))
			{
				decimal? shippedQtyDiff = e.Row.ShippedQty - e.OldRow.ShippedQty;

				BlanketSOLineSplit blanketSplit = SelectParentSplit(e.Row);
				blanketSplit.ShippedQty += shippedQtyDiff;
				if (blanketSplit.Completed != true)
				{
					blanketSplit.Completed = (blanketSplit.ShippedQty + blanketSplit.ReceivedQty >= blanketSplit.Qty);
				}
				blanketSplit = (BlanketSOLineSplit)Base.Caches<BlanketSOLineSplit>().Update(blanketSplit);

				var blanketLine = PXParentAttribute.SelectParent<BlanketSOLine>(Base.Caches<BlanketSOLineSplit>(), blanketSplit);
				if (blanketLine == null)
				{
					throw new Common.Exceptions.RowNotFoundException(Base.Caches<BlanketSOLine>(),
						blanketSplit.OrderType, blanketSplit.OrderNbr, blanketSplit.LineNbr);
				}
				blanketLine.ShippedQty += shippedQtyDiff;
				blanketLine.ClosedQty += shippedQtyDiff;
				if (blanketLine.ClosedQty > blanketLine.OrderQty)
					blanketLine.ClosedQty = blanketLine.OrderQty;
				if (blanketLine.Completed != true)
				{
					blanketLine.Completed = (blanketLine.ShippedQty >= blanketLine.OrderQty);
				}
				blanketLine = (BlanketSOLine)Base.Caches<BlanketSOLine>().Update(blanketLine);

				UpdateBlanketOrderShipmentCntr(blanketLine);
			}
		}

		private BlanketSOLineSplit SelectParentSplit(SOLine4 row)
		{
			BlanketSOLineSplit blanketSplit = SelectFrom<BlanketSOLineSplit>
				.Where<BlanketSOLineSplit.orderType.IsEqual<SOLine4.blanketType.FromCurrent>
				.And<BlanketSOLineSplit.orderNbr.IsEqual<SOLine4.blanketNbr.FromCurrent>>
				.And<BlanketSOLineSplit.lineNbr.IsEqual<SOLine4.blanketLineNbr.FromCurrent>>
				.And<BlanketSOLineSplit.splitLineNbr.IsEqual<SOLine4.blanketSplitLineNbr.FromCurrent>>>
				.View.SelectSingleBound(Base, new[] { row });
			if (blanketSplit == null)
			{
				throw new Common.Exceptions.RowNotFoundException(Base.Caches<BlanketSOLineSplit>(),
					row.BlanketType, row.BlanketNbr, row.BlanketLineNbr, row.BlanketSplitLineNbr);
			}
			return blanketSplit;
		}

		private void UpdateBlanketOrderShipmentCntr(BlanketSOLine blanketline)
		{
			var blanketOrder = PXParentAttribute.SelectParent<BlanketSOOrder>(Base.Caches<BlanketSOLine>(), blanketline);
			if (blanketOrder == null)
			{
				throw new Common.Exceptions.RowNotFoundException(Base.Caches<BlanketSOOrder>(), blanketline.OrderType, blanketline.OrderNbr);
			}
			if (blanketOrder.ShipmentCntrUpdated != true)
			{
				blanketOrder.ShipmentCntr++;
				blanketOrder.ShipmentCntrUpdated = true;
				blanketOrder = (BlanketSOOrder)Base.Caches<BlanketSOOrder>().Update(blanketOrder);
			}
		}
	}
}
