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
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;

using SiteStatusByCostCenter = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.SiteStatusByCostCenter;

namespace PX.Objects.SO.GraphExtensions
{
	public abstract class ItemAvailabilityAllocatedExtension<TGraph, TItemAvailExt, TLine, TSplit> : PXGraphExtension<TItemAvailExt, TGraph>
		where TGraph : PXGraph
		where TItemAvailExt : ItemAvailabilityExtension<TGraph, TLine, TSplit>
		where TLine : class, IBqlTable, ILSPrimary, new()
		where TSplit : class, IBqlTable, ILSDetail, new()
	{
		protected TItemAvailExt ItemAvailBase => Base1;

		protected abstract Type LineQtyAvail { get; }
		protected abstract Type LineQtyHardAvail { get; }

		public virtual bool IsAllocationEntryEnabled
		{
			get
			{
				SOOrderType ordertype = PXSetup<SOOrderType>.Select(Base);
				return ordertype == null || ordertype.RequireShipping == true || ordertype.Behavior == SOBehavior.BL;
			}
		}


		/// <summary>
		/// Overrides <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.GetStatus(TLine)"/>
		/// </summary>
		[PXOverride]
		public virtual string GetStatus(TLine line,
			Func<TLine, string> base_GetStatus)
		{
			if (IsAllocationEntryEnabled)
				return GetStatusWithAllocated(line);
			else
				return base_GetStatus(line);
		}

		protected abstract string GetStatusWithAllocated(TLine line);


		protected TLine LineToExcludeAllocated { get; private set; }

		/// <summary>
		/// Overrides <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.FetchWithLineUOM(TLine, bool, int?)"/>
		/// </summary>
		[PXOverride]
		public virtual IStatus FetchWithBaseUOM(ILSMaster row, bool excludeCurrent, int? costCenterID,
			Func<ILSMaster, bool, int?, IStatus> base_FetchWithBaseUOM)
		{
			try
			{
				if (row is TLine line)
					LineToExcludeAllocated = line;

				return base_FetchWithBaseUOM(row, excludeCurrent, costCenterID);
			}
			finally
			{
				LineToExcludeAllocated = null;
			}
		}

		/// <summary>
		/// Overrides <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.ExcludeCurrent(ILSDetail, IStatus, AvailabilitySigns)"/>
		/// </summary>
		[PXOverride]
		public virtual void ExcludeCurrent(ILSDetail currentSplit, IStatus allocated, AvailabilitySigns signs,
			Action<ILSDetail, IStatus, AvailabilitySigns> base_ExcludeCurrent)
		{
			if (LineToExcludeAllocated != null)
				ExcludeAllocated(LineToExcludeAllocated, allocated);
			else
				base_ExcludeCurrent(currentSplit, allocated, signs);
		}

		protected virtual IStatus ExcludeAllocated(TLine line, IStatus availability)
		{
			if (availability == null)
				return null;

			var lineCache = Base.Caches<TLine>();

			decimal? lineQtyAvail = (decimal?) lineCache.GetValue(line, LineQtyAvail.Name);
			decimal? lineQtyHardAvail = (decimal?) lineCache.GetValue(line, LineQtyHardAvail.Name);

			if (lineQtyAvail == null || lineQtyHardAvail == null)
			{
				var splitCache = Base.Caches<TSplit>();

				lineQtyAvail = 0m;
				lineQtyHardAvail = 0m;

				foreach (TSplit split in GetSplits(line))
				{
					TSplit actualSplit = EnsurePlanType(split);

					PXParentAttribute.SetParent(splitCache, actualSplit, typeof(TLine), line);

					var signs = ItemAvailBase.GetAvailabilitySigns<SiteStatusByCostCenter>(split);
					if (signs.SignQtyAvail != Sign.Zero)
						lineQtyAvail -= signs.SignQtyAvail * (actualSplit.BaseQty ?? 0m);

					if (signs.SignQtyHardAvail != Sign.Zero)
						lineQtyHardAvail -= signs.SignQtyHardAvail * (actualSplit.BaseQty ?? 0m);
				}

				lineCache.SetValue(line, LineQtyAvail.Name, lineQtyAvail);
				lineCache.SetValue(line, LineQtyHardAvail.Name, lineQtyHardAvail);
			}

			return CalculateAllocatedQuantity(availability, lineQtyAvail, lineQtyHardAvail);
		}

		protected virtual IStatus CalculateAllocatedQuantity(IStatus availability, decimal? lineQtyAvail, decimal? lineQtyHardAvail)
		{
			availability.QtyAvail += lineQtyAvail;
			availability.QtyHardAvail += lineQtyHardAvail;
			availability.QtyNotAvail = -lineQtyAvail;

			return availability;
		}

		protected abstract TSplit EnsurePlanType(TSplit split);
		protected virtual INItemPlan GetItemPlan(long? planID) => SelectFrom<INItemPlan>.Where<INItemPlan.planID.IsEqual<@P.AsLong>>.View.Select(Base, planID);

		protected abstract TSplit[] GetSplits(TLine line);


		/// <summary>
		/// Overrides <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.Optimize"/>
		/// </summary>
		[PXOverride]
		public virtual void Optimize(Action base_Optimize)
		{
			base_Optimize();

			if (DocumentNoteID != null)
				foreach (INItemPlan plan in
					SelectFrom<INItemPlan>.
					Where<INItemPlan.refNoteID.IsEqual<@P.AsGuid>>.
					View.Select(Base, DocumentNoteID))
				{
					SelectFrom<INItemPlan>.
					Where<INItemPlan.planID.IsEqual<@P.AsLong>>.
					View.StoreResult(Base, plan);
				}
		}

		protected abstract Guid? DocumentNoteID { get; }
	}
}
