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

using System.Collections.Generic;
using CostLayerInfo = PX.Objects.IN.INPIEntry.CostLayerInfo;

namespace PX.Objects.IN.PhysicalInventory
{
	public class CostLayerComparer : IComparer<CostLayerInfo>
	{
		private INItemSiteSettings _itemSite;

		public CostLayerComparer(INItemSiteSettings itemSite)
		{
			_itemSite = itemSite;
		}

		public virtual int Compare(CostLayerInfo x, CostLayerInfo y)
		{
			int compareByCostLayerType = CompareByCostLayerType(x, y);
			if (compareByCostLayerType != 0)
				return compareByCostLayerType;

			if (_itemSite.ValMethod == INValMethod.FIFO)
				return CompareFIFO(x.CostLayer, y.CostLayer);

			return CompareDefault(x.CostLayer, y.CostLayer);
		}

		protected virtual int CompareDefault(INCostStatus x, INCostStatus y)
		{
			if (x.AccountID != y.AccountID || x.SubID != y.SubID)
			{
				if (y.AccountID == _itemSite.InvtAcctID && y.SubID == _itemSite.InvtSubID)
					return -1;

				if (x.AccountID == _itemSite.InvtAcctID && x.SubID == _itemSite.InvtSubID)
					return 1;
			}

			int result = x.AccountID.Value.CompareTo(y.AccountID.Value);
			if (result != 0)
				return result;

			result = x.SubID.Value.CompareTo(y.SubID.Value);
			if (result != 0)
				return result;

			return x.CostID.Value.CompareTo(y.CostID.Value);
		}

		protected virtual int CompareFIFO(INCostStatus x, INCostStatus y)
		{
			int result = x.ReceiptDate.Value.CompareTo(y.ReceiptDate.Value);
			if (result != 0)
				return result;

			result = x.ReceiptNbr.CompareTo(y.ReceiptNbr);
			if (result != 0)
				return result;

			return CompareDefault(x, y);
		}

		protected virtual int CompareByCostLayerType(CostLayerInfo x, CostLayerInfo y)
			=> x.CostLayerType.CompareTo(y.CostLayerType);
	}
}
