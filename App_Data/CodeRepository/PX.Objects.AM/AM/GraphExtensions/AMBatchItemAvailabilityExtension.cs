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
using PX.Objects.Common.Exceptions;
using PX.Objects.IN;

namespace PX.Objects.AM
{
	public abstract class AMBatchItemAvailabilityExtension<TBatchGraph> : IN.GraphExtensions.ItemAvailabilityExtension<TBatchGraph, AMMTran, AMMTranSplit>
		where TBatchGraph : AMBatchEntryBase
	{
		protected override AMMTranSplit EnsureSplit(ILSMaster row)
			=> Base.FindImplementation<AMBatchLineSplittingExtension<TBatchGraph>>().EnsureSplit(row);

		protected override decimal GetUnitRate(AMMTran line) => GetUnitRate<AMMTran.inventoryID, AMMTran.uOM>(line);

		protected override string GetStatus(AMMTran line)
		{
			string status = string.Empty;

			if (FetchWithLineUOM(line, excludeCurrent: line?.Released != true, CostCenter.FreeStock) is IStatus availability)
			{
				status = FormatStatus(availability, line.UOM);
			}

			return status;
		}

		private string FormatStatus(IStatus availability, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefixNLA(
				Messages.LSTranStatus,
				uom,
				FormatQty(availability.QtyOnHand.GetValueOrDefault()),
				FormatQty(availability.QtyAvail.GetValueOrDefault()),
				FormatQty(availability.QtyHardAvail.GetValueOrDefault()));
		}

		protected override void RaiseQtyExceptionHandling(AMMTran line, PXExceptionInfo ei, decimal? newValue)
		{
#if DEBUG
			AMDebug.TraceWriteMethodName(ei.MessageFormat,
				LineCache.GetStateExt<AMMTran.inventoryID>(line),
				LineCache.GetStateExt<AMMTran.subItemID>(line),
				LineCache.GetStateExt<AMMTran.siteID>(line),
				LineCache.GetStateExt<AMMTran.locationID>(line),
				LineCache.GetValue<AMMTran.lotSerialNbr>(line));
#endif
			LineCache.RaiseExceptionHandling<AMMTran.qty>(line, newValue,
				new PXSetPropertyException(ei.MessageFormat, ei.ErrorLevel ?? PXErrorLevel.Warning,
					LineCache.GetStateExt<AMMTran.inventoryID>(line),
					LineCache.GetStateExt<AMMTran.subItemID>(line),
					LineCache.GetStateExt<AMMTran.siteID>(line),
					LineCache.GetStateExt<AMMTran.locationID>(line),
					LineCache.GetValue<AMMTran.lotSerialNbr>(line)));
		}

		protected override void RaiseQtyExceptionHandling(AMMTranSplit split, PXExceptionInfo ei, decimal? newValue)
		{
			SplitCache.RaiseExceptionHandling<AMMTranSplit.qty>(split, newValue,
				new PXSetPropertyException(ei.MessageFormat, ei.ErrorLevel ?? PXErrorLevel.Warning,
					SplitCache.GetStateExt<AMMTranSplit.inventoryID>(split),
					SplitCache.GetStateExt<AMMTranSplit.subItemID>(split),
					SplitCache.GetStateExt<AMMTranSplit.siteID>(split),
					SplitCache.GetStateExt<AMMTranSplit.locationID>(split),
					SplitCache.GetValue<AMMTranSplit.lotSerialNbr>(split)));
		}
	}
}
