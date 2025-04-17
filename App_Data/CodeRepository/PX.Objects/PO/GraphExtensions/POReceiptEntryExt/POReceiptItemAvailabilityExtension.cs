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
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Exceptions;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class POReceiptItemAvailabilityExtension : ItemAvailabilityExtension<POReceiptEntry, POReceiptLine, POReceiptLineSplit>
	{
		protected override POReceiptLineSplit EnsureSplit(ILSMaster row)
			=> Base.FindImplementation<POReceiptLineSplittingExtension>().EnsureSplit(row);

		protected override decimal GetUnitRate(POReceiptLine line) => GetUnitRate<POReceiptLine.inventoryID, POReceiptLine.uOM>(line);

		protected override string GetStatus(POReceiptLine line)
		{
			string status = string.Empty;

			bool excludeCurrent = PXParentAttribute.SelectParent<POReceipt>(LineCache, line)?.Released != true;
			
			if (FetchWithLineUOM(line, excludeCurrent, line.CostCenterID) is IStatus availability)
			{
				status = FormatStatus(availability, line.UOM);
				Check(line, availability);
			}

			return status;
		}

		protected override void ExcludeCurrent(ILSDetail currentSplit, IStatus allocated, AvailabilitySigns signs)
		{
			base.ExcludeCurrent(currentSplit, allocated, signs);

			//UnassignedQty is excluded in POReceiptLineSplittingExtension.LineToSplit
			foreach (var unassignedSplit in SelectUnassignedSplits((POReceiptLineSplit)currentSplit))
			{
				if ((currentSplit.LocationID == null || currentSplit.LocationID == unassignedSplit.LocationID) &&
					(currentSplit.LotSerialNbr == null || string.IsNullOrEmpty(unassignedSplit.LotSerialNbr) || string.Equals(currentSplit.LotSerialNbr, unassignedSplit.LotSerialNbr, StringComparison.InvariantCultureIgnoreCase)))
				{
					base.ExcludeCurrent(unassignedSplit, allocated, signs);
				}
			}
		}

		private string FormatStatus(IStatus availability, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefixNLA(
				Messages.Availability_Info,
				uom,
				FormatQty(availability.QtyOnHand),
				FormatQty(availability.QtyAvail),
				FormatQty(availability.QtyHardAvail),
				FormatQty(availability.QtyActual));
		}

		protected override void Optimize()
		{
			base.Optimize();

			//package loading and caching
			var select = new
				SelectFrom<POReceiptLine>.
				InnerJoin<INSiteStatusByCostCenter>.On<POReceiptLine.FK.SiteStatusByCostCenter>.
				LeftJoin<INLocationStatusByCostCenter>.On<POReceiptLine.FK.LocationStatusByCostCenter>.
				LeftJoin<INLotSerialStatusByCostCenter>.On<POReceiptLine.FK.LotSerialStatusByCostCenter>.
				Where<
					POReceiptLine.receiptType.IsEqual<POReceipt.receiptType.FromCurrent>.
					And<POReceiptLine.receiptNbr.IsEqual<POReceipt.receiptNbr.FromCurrent>>>.
				View.ReadOnly(Base);
			using (new PXFieldScope(select.View, typeof(INSiteStatusByCostCenter), typeof(INLocationStatusByCostCenter), typeof(INLotSerialStatusByCostCenter)))
			{
				foreach (PXResult<POReceiptLine, INSiteStatusByCostCenter, INLocationStatusByCostCenter, INLotSerialStatusByCostCenter> res in select.Select())
				{
					(var _, var siteStatusByCostCenter, var locationStatusByCostCenter, var lotSerialStatusByCostCenter) = res;

					INSiteStatusByCostCenter.PK.StoreResult(Base, siteStatusByCostCenter);

					if (locationStatusByCostCenter.LocationID != null)
						INLocationStatusByCostCenter.PK.StoreResult(Base, locationStatusByCostCenter);

					if (lotSerialStatusByCostCenter?.LotSerialNbr != null)
						INLotSerialStatusByCostCenter.PK.StoreResult(Base, lotSerialStatusByCostCenter);
				}
			}
		}

		protected override void RaiseQtyExceptionHandling(POReceiptLine line, PXExceptionInfo ei, decimal? newValue)
		{
			LineCache.RaiseExceptionHandling<POReceiptLine.receiptQty>(line, newValue,
				new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning,
					LineCache.GetValueExt<POReceiptLine.inventoryID>(line),
					LineCache.GetValueExt<POReceiptLine.subItemID>(line),
					LineCache.GetValueExt<POReceiptLine.siteID>(line),
					LineCache.GetValueExt<POReceiptLine.locationID>(line),
					LineCache.GetValue<POReceiptLine.lotSerialNbr>(line)));
		}

		protected override void RaiseQtyExceptionHandling(POReceiptLineSplit split, PXExceptionInfo ei, decimal? newValue)
		{
			SplitCache.RaiseExceptionHandling<POReceiptLineSplit.qty>(split, newValue,
				new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning,
					SplitCache.GetValueExt<POReceiptLineSplit.inventoryID>(split),
					SplitCache.GetValueExt<POReceiptLineSplit.subItemID>(split),
					SplitCache.GetValueExt<POReceiptLineSplit.siteID>(split),
					SplitCache.GetValueExt<POReceiptLineSplit.locationID>(split),
					SplitCache.GetValue<POReceiptLineSplit.lotSerialNbr>(split)));
		}

		protected virtual List<Unassigned.POReceiptLineSplit> SelectUnassignedSplits(POReceiptLineSplit assignedSplit)
		{
			var unassignedRow = new Unassigned.POReceiptLineSplit
			{
				ReceiptType = assignedSplit.ReceiptType,
				ReceiptNbr = assignedSplit.ReceiptNbr,
				LineNbr = assignedSplit.LineNbr,
				SplitLineNbr = assignedSplit.SplitLineNbr
			};

			return PXParentAttribute
				.SelectSiblings(
					Base.Caches<Unassigned.POReceiptLineSplit>(),
					unassignedRow,
					IsOptimizationEnabled ? typeof(POReceipt) : typeof(POReceiptLine))
				.Cast<Unassigned.POReceiptLineSplit>()
				.Where(us =>
					us.InventoryID == assignedSplit.InventoryID &&
					us.LineNbr == assignedSplit.LineNbr)
				.ToList();
		}
	}
}
