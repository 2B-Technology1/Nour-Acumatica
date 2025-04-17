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

using PX.Data;
using PX.Objects.Common.Exceptions;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.SO.GraphExtensions;

namespace PX.Objects.FS
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class FSAppointmentItemAvailabilityExtension : SOBaseItemAvailabilityExtension<AppointmentEntry, FSAppointmentDet, FSApptLineSplit>
	{
		public override void Initialize()
		{
			base.Initialize();

			Base.Caches.AddCacheMapping(typeof(INLocationStatus), typeof(INLocationStatus));
			Base.Caches.AddCacheMapping(typeof(LocationStatus), typeof(LocationStatus));
		}
		protected override FSApptLineSplit EnsureSplit(ILSMaster row)
			=> Base.FindImplementation<FSAppointmentLineSplittingExtension>().EnsureSplit(row);

		protected override decimal GetUnitRate(FSAppointmentDet line) => GetUnitRate<FSAppointmentDet.inventoryID, FSAppointmentDet.uOM>(line);

		protected override string GetStatus(FSAppointmentDet line)
		{
			string status = string.Empty;

			if (FetchWithLineUOM(line, excludeCurrent: line?.Status != ID.Status_AppointmentDet.COMPLETED, CostCenter.FreeStock) is IStatus availability)
			{
				status = FormatStatus(availability, line.UOM);
				Check(line, availability);
			}

			return status;
		}

		private string FormatStatus(IStatus availability, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefix(
				SO.Messages.Availability_Info,
				uom,
				FormatQty(availability.QtyOnHand),
				FormatQty(availability.QtyAvail),
				FormatQty(availability.QtyHardAvail)
			);
		}

		protected override IStatus Fetch(ILSDetail split, bool excludeCurrent, int? costCenterID)
		{
			return base.Fetch(split, excludeCurrent, costCenterID);
		}

		protected void ExcludeCurrent(ILSDetail currentSplit, IStatus allocated, decimal signQtyAvail, decimal signQtyHardAvail, decimal signQtyActual)
		{
			allocated.QtyAvail += currentSplit.BaseQty ?? 0m;
		}

		public override void Check(ILSMaster row, int? costCenterID)
		{
			base.Check(row, costCenterID);
			MemoCheck(row);
		}

		protected virtual void MemoCheck(ILSMaster row)
		{
			if (row is FSAppointmentDet line)
			{
				MemoCheck(line);

				FSApptLineSplit split = EnsureSplit(line);
				MemoCheck(line, split, triggeredBySplit: false);

				if (split.LotSerialNbr == null)
					row.LotSerialNbr = null;
			}
			else if (row is FSApptLineSplit split)
			{
				line = PXParentAttribute.SelectParent<FSAppointmentDet>(SplitCache, split);
				MemoCheck(line);
				MemoCheck(line, split, triggeredBySplit: true);
			}
		}

		public virtual bool MemoCheck(FSAppointmentDet line) => MemoCheckQty(line);
		protected virtual bool MemoCheckQty(FSAppointmentDet row) => true;
		protected virtual bool MemoCheck(FSAppointmentDet line, FSApptLineSplit split, bool triggeredBySplit) => true;

		protected override void RaiseQtyExceptionHandling(FSAppointmentDet line, PXExceptionInfo ei, decimal? newValue)
		{
			LineCache.RaiseExceptionHandling<FSAppointmentDet.effTranQty>(line, newValue,
				new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning,
					LineCache.GetStateExt<FSAppointmentDet.inventoryID>(line),
					LineCache.GetStateExt<FSAppointmentDet.subItemID>(line),
					LineCache.GetStateExt<FSAppointmentDet.siteID>(line),
					LineCache.GetStateExt<FSAppointmentDet.locationID>(line),
					LineCache.GetValue<FSAppointmentDet.lotSerialNbr>(line)));
		}

		protected override void RaiseQtyExceptionHandling(FSApptLineSplit split, PXExceptionInfo ei, decimal? newValue)
		{
			SplitCache.RaiseExceptionHandling<FSApptLineSplit.qty>(split, newValue,
				new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning,
					SplitCache.GetStateExt<FSApptLineSplit.inventoryID>(split),
					SplitCache.GetStateExt<FSApptLineSplit.subItemID>(split),
					SplitCache.GetStateExt<FSApptLineSplit.siteID>(split),
					SplitCache.GetStateExt<FSApptLineSplit.locationID>(split),
					SplitCache.GetValue<FSApptLineSplit.lotSerialNbr>(split)));
		}
	}
}
