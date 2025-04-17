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
using PX.Data;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXProtectedAccess(typeof(SOOrderItemAvailabilityExtension))]
	public abstract class SOOrderItemAvailabilityAllocatedExtension : ItemAvailabilityAllocatedExtension<SOOrderEntry, SOOrderItemAvailabilityExtension, SOLine, SOLineSplit>
	{
		protected override string GetStatusWithAllocated(SOLine line)
		{
			string status = string.Empty;

			if (Base1.FetchWithLineUOM(line, excludeCurrent: line?.Completed != true, line.CostCenterID) is IStatus availability)
			{
				decimal? allocated = GetAllocatedQty(line);

				status = FormatStatusAllocated(availability, allocated, line.UOM);
				Check(line, availability);
			}

			return status;
		}

		protected virtual decimal GetAllocatedQty(SOLine line)
			=> PXDBQuantityAttribute.Round((line.LineQtyHardAvail ?? 0m) * GetUnitRate(line));

		private string FormatStatusAllocated(IStatus availability, decimal? allocated, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefixNLA(
				Messages.Availability_AllocatedInfo,
				uom,
				FormatQty(availability.QtyOnHand),
				FormatQty(availability.QtyAvail),
				FormatQty(availability.QtyHardAvail),
				FormatQty(allocated));
		}


		protected override Type LineQtyAvail => typeof(SOLine.lineQtyAvail);
		protected override Type LineQtyHardAvail => typeof(SOLine.lineQtyHardAvail);

		protected override SOLineSplit[] GetSplits(SOLine line) => Base.FindImplementation<SOOrderLineSplittingExtension>().GetSplits(line);

		protected override SOLineSplit EnsurePlanType(SOLineSplit split)
		{
			if (split.PlanID != null && GetItemPlan(split.PlanID) is INItemPlan plan)
			{
				split = PXCache<SOLineSplit>.CreateCopy(split);
				split.PlanType = plan.PlanType;
			}
			return split;
		}

		protected override Guid? DocumentNoteID => Base.Document.Current?.NoteID;

		/// <summary>
		/// Overrides <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.Fetch(ILSDetail, bool, int?)"/>
		/// </summary>
		[PXOverride]
		public virtual IStatus Fetch(ILSDetail split, bool excludeCurrent, int? costCenterID, Func<ILSDetail, bool, int?, IStatus> base_Fetch)
		{
			if (IsAllocationEntryEnabled && LineToExcludeAllocated != null)
			{
				IStatus result = base_Fetch(split, false, costCenterID);

				return ExcludeAllocated(LineToExcludeAllocated, result);
			}
			return base_Fetch(split, excludeCurrent, costCenterID);
		}

		/// <summary>
		/// Intension of overriding ExcludeCurrent in here is to skip the condition check of LineToExcludeAllocated at
		/// <see cref="ItemAvailabilityAllocatedExtension{TGraph, TItemAvailExt, TLine, TSplit}.ExcludeCurrent(ILSDetail, IStatus, AvailabilitySigns, Action{ILSDetail, IStatus, AvailabilitySigns})"/>
		/// and invoke the supper base method of <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.ExcludeCurrent(ILSDetail, IStatus, AvailabilitySigns)"/>
		/// </summary>
		[PXOverride]
		public override void ExcludeCurrent(ILSDetail currentSplit, IStatus allocated, AvailabilitySigns signs,
			Action<ILSDetail, IStatus, AvailabilitySigns> base_ExcludeCurrent)
		{
			base_ExcludeCurrent(currentSplit, allocated, signs);
		}

		#region PXProtectedAccess
		[PXProtectedAccess] protected abstract string FormatQty(decimal? value);
		[PXProtectedAccess] protected abstract decimal GetUnitRate(SOLine line);
		[PXProtectedAccess] protected abstract void Check(ILSMaster row, IStatus availability);
		#endregion
	}	
}
