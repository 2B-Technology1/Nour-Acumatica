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
using System.Linq;
using PX.Data;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;
using PX.Objects.IN.Attributes;

namespace PX.Objects.IN.GraphExtensions
{
	public abstract class CostCenterDispatcher<TGraph, TLine, TCostCenterField> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TLine : class, IItemPlanMaster, IBqlTable, new()
		where TCostCenterField : class, IBqlField
	{
		[PXCopyPasteHiddenView()]
		public PXSelect<INCostCenter> CostCenters;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CostCenterDBDefault]
		protected virtual void _(Events.CacheAttached<INItemPlan.costCenterID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CostCenterDBDefault]
		protected virtual void _(Events.CacheAttached<SiteStatusByCostCenter.costCenterID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CostCenterDBDefault]
		protected virtual void _(Events.CacheAttached<LocationStatusByCostCenter.costCenterID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CostCenterDBDefault]
		protected virtual void _(Events.CacheAttached<LotSerialStatusByCostCenter.costCenterID> e) { }


		public override void Initialize()
		{
			base.Initialize();
			MoveCostCenterViewCacheToTop();
			SubscribeToFieldsDependOn();
		}

		protected virtual void MoveCostCenterViewCacheToTop()
		{
			int index = Base.Views.Caches.IndexOf(typeof(INCostCenter));
			if (index > 0)
			{
				Base.Views.Caches.RemoveAt(index);
				Base.Views.Caches.Insert(0, typeof(INCostCenter));
			}
		}

		protected virtual void SubscribeToFieldsDependOn()
		{
			foreach (Type field in Base.FindAllImplementations<ICostCenterSupport<TLine>>()
				.SelectMany(ext => ext.GetFieldsDependOn()))
			{
				if (!typeof(IBqlField).IsAssignableFrom(field) || BqlCommand.GetItemType(field) != typeof(TLine))
				{
					throw new PXArgumentException();
				}
				Base.FieldUpdated.AddHandler(typeof(TLine), field.Name, FieldDependOnUpdated);
			}
		}

		protected virtual void FieldDependOnUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = (TLine)e.Row;
			int newCostCenterID = GetCostCenterIDByLine(row);
			var oldCostCenterID = (int?)cache.GetValue<TCostCenterField>(row);
			if (oldCostCenterID != newCostCenterID)
			{
				cache.SetValueExt<TCostCenterField>(row, newCostCenterID);
			}
		}

		protected ICostCenterSupport<TLine> GetCostCenterSupportByLine(TLine line)
		{
			return Base.FindAllImplementations<ICostCenterSupport<TLine>>()
				.OrderBy(e => e.SortOrder)
				.FirstOrDefault(e => e.IsSpecificCostCenter(line));
		}

		public int GetCostCenterIDByLine(TLine line)
		{
			var costCenterSupport = GetCostCenterSupportByLine(line);
			return costCenterSupport?.GetCostCenterID(line) ?? CostCenter.FreeStock;
		}
	}
}
