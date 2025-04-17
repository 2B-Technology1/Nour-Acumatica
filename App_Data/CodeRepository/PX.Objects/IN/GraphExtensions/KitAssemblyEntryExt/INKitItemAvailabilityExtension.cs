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

namespace PX.Objects.IN.GraphExtensions.KitAssemblyEntryExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class INKitItemAvailabilityExtension : ItemAvailabilityExtension<KitAssemblyEntry, INKitRegister, INKitTranSplit>
	{
		protected override INKitTranSplit EnsureSplit(ILSMaster row)
			=> Base.FindImplementation<INKitLineSplittingExtension>().EnsureSplit(row);

		protected override decimal GetUnitRate(INKitRegister line) => GetUnitRate<INKitRegister.kitInventoryID, INKitRegister.uOM>(line);

		protected override string GetStatus(INKitRegister line)
		{
			string status = string.Empty;

			if (FetchWithLineUOM(line, excludeCurrent: line?.Released != true, line.CostCenterID) is IStatus availability)
			{
				status = FormatStatus(availability, line.UOM);
				Check(line, availability);
			}

			return status;
		}

		private string FormatStatus(IStatus availability, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefixNLA(
				Messages.Availability_ActualInfo,
				uom,
				FormatQty(availability.QtyOnHand),
				FormatQty(availability.QtyAvail),
				FormatQty(availability.QtyHardAvail),
				FormatQty(availability.QtyActual));
		}

		protected override void RaiseQtyExceptionHandling(INKitRegister line, PXExceptionInfo ei, decimal? newValue)
		{
			LineCache.RaiseExceptionHandling<INKitRegister.qty>(line, null,
				new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning,
					LineCache.GetStateExt<INKitRegister.kitInventoryID>(line),
					LineCache.GetStateExt<INKitRegister.subItemID>(line),
					LineCache.GetStateExt<INKitRegister.siteID>(line),
					LineCache.GetStateExt<INKitRegister.locationID>(line),
					LineCache.GetValue<INKitRegister.lotSerialNbr>(line)));
		}

		protected override void RaiseQtyExceptionHandling(INKitTranSplit split, PXExceptionInfo ei, decimal? newValue)
		{
			SplitCache.RaiseExceptionHandling<INKitTranSplit.qty>(split, null,
				new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning,
					SplitCache.GetStateExt<INKitTranSplit.inventoryID>(split),
					SplitCache.GetStateExt<INKitTranSplit.subItemID>(split),
					SplitCache.GetStateExt<INKitTranSplit.siteID>(split),
					SplitCache.GetStateExt<INKitTranSplit.locationID>(split),
					SplitCache.GetValue<INKitTranSplit.lotSerialNbr>(split)));
		}

		protected override void AddStatusField()
		{

		}
	}
}
