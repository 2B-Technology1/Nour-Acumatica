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
using PX.Objects.IN;

namespace PX.Objects.SO
{
	public abstract class SpecialOrderCostCenterSupport<TGraph, TLine> : PXGraphExtension<TGraph>, ICostCenterSupport<TLine>
		where TGraph : PXGraph
		where TLine : class, IItemPlanMaster, IBqlTable, new()
	{
		protected class CostCenterKeys
		{
			public int? SiteID { get; set; }
			public string OrderType { get; set; }
			public string OrderNbr { get; set; }
			public int? LineNbr { get; set; }
		}

		public virtual int SortOrder => 100;

		public abstract IEnumerable<Type> GetFieldsDependOn();

		public abstract bool IsSpecificCostCenter(TLine tran);

		protected abstract CostCenterKeys GetCostCenterKeys(TLine tran);

		public int GetCostCenterID(TLine tran)
		{
			return (int)FindOrCreateCostCenter(GetCostCenterKeys(tran));
		}

		protected virtual string BuildCostCenterCD(int? siteID, string orderType, string orderNbr, int? lineNbr)
		{
			return string.Format("{0} {1} {2}", orderType.Trim(), orderNbr.Trim(), lineNbr);
		}

		protected virtual int? FindOrCreateCostCenter(CostCenterKeys k)
		{
			var select = new PXSelect<INCostCenter,
				Where<INCostCenter.siteID, Equal<Required<INCostCenter.siteID>>,
				And<INCostCenter.sOOrderType, Equal<Required<INCostCenter.sOOrderType>>,
				And<INCostCenter.sOOrderNbr, Equal<Required<INCostCenter.sOOrderNbr>>,
				And<INCostCenter.sOOrderLineNbr, Equal<Required<INCostCenter.sOOrderLineNbr>>>>>>>(Base);

			INCostCenter existing = select.Select(k.SiteID, k.OrderType, k.OrderNbr, k.LineNbr);

			if (existing != null)
			{
				return existing.CostCenterID;
			}
			else
			{
				return InsertNewCostSite(k.SiteID, k.OrderType, k.OrderNbr, k.LineNbr);
			}
		}

		private int? InsertNewCostSite(int? siteID, string orderType, string orderNbr, int? lineNbr)
		{
			var costSite = new INCostCenter
			{
				CostLayerType = CostLayerType.Special,
				SiteID = siteID,
				SOOrderType = orderType,
				SOOrderNbr = orderNbr,
				SOOrderLineNbr = lineNbr,
				CostCenterCD = BuildCostCenterCD(siteID, orderType, orderNbr, lineNbr),
			};

			costSite = (INCostCenter)Base.Caches<INCostCenter>().Insert(costSite);
			if (costSite != null)
			{
				return costSite.CostCenterID;
			}

			throw new InvalidOperationException("Failed to insert new INCostCenter");
		}
	}
}
