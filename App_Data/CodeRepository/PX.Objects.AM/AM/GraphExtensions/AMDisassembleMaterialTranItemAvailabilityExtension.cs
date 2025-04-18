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
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Exceptions;
using PX.Objects.IN;

namespace PX.Objects.AM
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class AMDisassembleMaterialTranItemAvailabilityExtension : IN.GraphExtensions.ItemAvailabilityExtension<DisassemblyEntry, AMDisassembleTran, AMDisassembleTranSplit>
	{
		protected override AMDisassembleTranSplit EnsureSplit(ILSMaster row)
			=> Base.FindImplementation<AMDisassembleMaterialTranLineSplittingExtension>().EnsureSplit(row);

		protected override decimal GetUnitRate(AMDisassembleTran line) => GetUnitRate<AMDisassembleTran.inventoryID, AMDisassembleTran.uOM>(line);

		public override void Initialize()
		{
			base.Initialize();
			ManualEvent.Row<AMDisassembleTran>.Updated.Subscribe(Base, EventHandler);
		}

		protected override string GetStatus(AMDisassembleTran line)
		{
			string status = string.Empty;

			if (FetchWithLineUOM(line, excludeCurrent: line?.Released != true, CostCenter.FreeStock) is IStatus availability)
				status = FormatStatus(availability, line.UOM);

			return status;
		}

		private string FormatStatus(IStatus availability, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefixNLA(
				IN.Messages.Availability_ActualInfo,
				uom,
				FormatQty(availability.QtyOnHand),
				FormatQty(availability.QtyAvail),
				FormatQty(availability.QtyHardAvail),
				FormatQty(availability.QtyActual));
		}

		protected virtual void EventHandler(ManualEvent.Row<AMDisassembleTran>.Updated.Args e)
		{
			if (e.Row == null || PXLongOperation.Exists(Base.UID))
				return;

			if (FetchWithLineUOM(e.Row, excludeCurrent: true, CostCenter.FreeStock) is IStatus availability)
				Check(e.Row, availability);
		}

		protected override IEnumerable<PXExceptionInfo> GetCheckErrors(ILSMaster row, IStatus availability)
		{
			foreach (var errorInfo in base.GetCheckErrors(row, availability))
				yield return errorInfo;

			if (row.InvtMult == -1 && row.BaseQty > 0m && availability != null)
				if (availability.QtyAvail < row.Qty)
				{
					string message = GetErrorMessageQtyAvail(GetStatusLevel(availability));

					if (message != null)
						yield return new PXExceptionInfo(message);
				}
		}

		protected override void RaiseQtyExceptionHandling(AMDisassembleTran line, PXExceptionInfo ei, decimal? newValue)
		{
			LineCache.RaiseExceptionHandling<AMDisassembleTran.qty>(line, null,
				new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning,
					LineCache.GetStateExt<AMDisassembleTran.inventoryID>(line),
					LineCache.GetStateExt<AMDisassembleTran.subItemID>(line),
					LineCache.GetStateExt<AMDisassembleTran.siteID>(line),
					LineCache.GetStateExt<AMDisassembleTran.locationID>(line),
					LineCache.GetValue<AMDisassembleTran.lotSerialNbr>(line)));
		}

		protected override void RaiseQtyExceptionHandling(AMDisassembleTranSplit split, PXExceptionInfo ei, decimal? newValue)
		{
			SplitCache.RaiseExceptionHandling<AMDisassembleTranSplit.qty>(split, null,
				new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning,
					SplitCache.GetStateExt<AMDisassembleTranSplit.inventoryID>(split),
					SplitCache.GetStateExt<AMDisassembleTranSplit.subItemID>(split),
					SplitCache.GetStateExt<AMDisassembleTranSplit.siteID>(split),
					SplitCache.GetStateExt<AMDisassembleTranSplit.locationID>(split),
					SplitCache.GetValue<AMDisassembleTranSplit.lotSerialNbr>(split)));
		}
	}
}
