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
using PX.Objects.SO.GraphExtensions;

namespace PX.Objects.FS
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXProtectedAccess(typeof(FSServiceOrderItemAvailabilityExtension))]
	public abstract class FSServiceOrderItemAvailabilityAllocatedExtension : ItemAvailabilityAllocatedExtension<ServiceOrderEntry, FSServiceOrderItemAvailabilityExtension, FSSODet, FSSODetSplit>
	{
		protected FSSODet DetailLine { get; private set; }
		protected override string GetStatusWithAllocated(FSSODet line)
		{
			string status = string.Empty;
			DetailLine = line;

			if (Base1.FetchWithLineUOM(line, excludeCurrent: line?.Completed != true, CostCenter.FreeStock) is IStatus availability)
			{
				decimal allocated = GetAllocatedQty(line);

				status = FormatStatusAllocated(availability, allocated, line.UOM);
				Check(line, availability);
			}

			return status;
		}

		protected virtual decimal GetAllocatedQty(FSSODet line)
			=> PXDBQuantityAttribute.Round((line.LineQtyHardAvail ?? 0m) * GetUnitRate(line));

		private string FormatStatusAllocated(IStatus availability, decimal? allocated, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefixNLA(
				SO.Messages.Availability_AllocatedInfo,
				uom,
				FormatQty(availability.QtyOnHand),
				FormatQty(availability.QtyAvail),
				FormatQty(availability.QtyHardAvail),
				FormatQty(allocated));
		}


		protected override Type LineQtyAvail => typeof(FSSODet.lineQtyAvail);
		protected override Type LineQtyHardAvail => typeof(FSSODet.lineQtyHardAvail);

		protected override FSSODetSplit[] GetSplits(FSSODet line) => Base.FindImplementation<FSServiceOrderLineSplittingExtension>().GetSplits(line);

		protected override FSSODetSplit EnsurePlanType(FSSODetSplit split)
		{
			if (split.PlanID != null && GetItemPlan(split.PlanID) is INItemPlan plan)
			{
				split = PXCache<FSSODetSplit>.CreateCopy(split);
				split.PlanType = plan.PlanType;
			}
			return split;
		}

		protected override Guid? DocumentNoteID => Base.ServiceOrderRecords.Current?.NoteID;

		protected override IStatus CalculateAllocatedQuantity(IStatus availability, decimal? lineQtyAvail, decimal? lineQtyHardAvail)
		{
			availability.QtyAvail += (DetailLine?.SODetID > 0 && DetailLine?.Completed != true) ? 0 : lineQtyAvail;
			availability.QtyHardAvail += (DetailLine?.SODetID > 0 && DetailLine?.Completed != true) ? 0 : lineQtyHardAvail;
			availability.QtyNotAvail = (DetailLine?.SODetID > 0 && DetailLine?.Completed != true) ? 0 : -lineQtyAvail;

			return availability;
		}

		#region PXProtectedAccess
		[PXProtectedAccess] protected abstract string FormatQty(decimal? value);
		[PXProtectedAccess] protected abstract decimal GetUnitRate(FSSODet line);
		[PXProtectedAccess] protected abstract void Check(ILSMaster row, IStatus availability);
		#endregion
	}
}
