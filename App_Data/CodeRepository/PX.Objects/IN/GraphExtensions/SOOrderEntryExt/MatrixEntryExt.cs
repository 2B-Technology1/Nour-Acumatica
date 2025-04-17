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

using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;
using PX.Objects.IN.Matrix.Interfaces;
using PX.Objects.SO;

namespace PX.Objects.IN.GraphExtensions.SOOrderEntryExt
{
	public class MatrixEntryExt : Matrix.GraphExtensions.SmartPanelExt<SOOrderEntry, SOOrder>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.matrixItem>();
		}

		protected override IEnumerable<IMatrixItemLine> GetLines(int? siteID, int? inventoryID)
			=> Base.Transactions.SelectMain().Where(l => l.InventoryID == inventoryID && (l.SiteID == siteID || siteID == null));

		protected override IEnumerable<IMatrixItemLine> GetLines(int? siteID, int? inventoryID, string taxCategoryID, string uom)
			=> Base.Transactions.SelectMain().Where(l => l.InventoryID == inventoryID && (l.SiteID == siteID || siteID == null)
				&& (l.TaxCategoryID == taxCategoryID || taxCategoryID == null) && l.UOM == uom);

		protected override void UpdateLine(IMatrixItemLine line)
			=> Base.Transactions.Update((SOLine)line);

		protected override void CreateNewLine(int? siteID, int? inventoryID, decimal qty)
			=> CreateNewLine(siteID, inventoryID, null, qty, null);

		protected override void CreateNewLine(int? siteID, int? inventoryID, string taxCategoryID, decimal qty, string uom)
		{
			SOLine newline = PXCache<SOLine>.CreateCopy(Base.Transactions.Insert(new SOLine()));
			newline.SiteID = siteID;
			newline.InventoryID = inventoryID;
			newline = PXCache<SOLine>.CreateCopy(Base.Transactions.Update(newline));
			if (newline.RequireLocation != true)
				newline.LocationID = null;
			newline = PXCache<SOLine>.CreateCopy(Base.Transactions.Update(newline));
			newline.Qty = qty;

			if (uom != null)
				newline.UOM = uom;

			newline = Base.Transactions.Update(newline);

			if (!string.IsNullOrEmpty(taxCategoryID))
			{
				Base.Transactions.Cache.SetValueExt<SOLine.taxCategoryID>(newline, taxCategoryID);
				newline = Base.Transactions.Update(newline);
			}
		}

		protected override bool IsDocumentOpen()
			=> Base.Transactions.Cache.AllowInsert;

		protected override void DeductAllocated(SiteStatusByCostCenter allocated, IMatrixItemLine line)
		{
			SOLine soLine = (SOLine)line;

			if (soLine.CostCenterID == CostCenter.FreeStock)
			{
				allocated.QtyAvail += soLine.LineQtyAvail;
				allocated.QtyHardAvail += soLine.LineQtyHardAvail;
			}
		}

		protected override string GetAvailabilityMessage(int? siteID, InventoryItem item, SiteStatusByCostCenter availability, string uom)
		{
			if (Base.LineSplittingAllocatedExt.IsAllocationEntryEnabled)
			{
				decimal? allocated = GetLines(siteID, item.InventoryID).Sum(l => ((SOLine)l).LineQtyHardAvail);

				if (uom != item.BaseUnit)
				{
					allocated = INUnitAttribute.ConvertFromBase(Matrix.Cache, item.InventoryID, uom, allocated ?? 0m, INPrecision.QUANTITY);
				}

				return PXMessages.LocalizeFormatNoPrefix(SO.Messages.Availability_AllocatedInfo,
						uom, FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail), FormatQty(allocated));
			}
			else
				return PXMessages.LocalizeFormatNoPrefix(Messages.Availability_Info,
						uom, FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));
		}

		protected override int? GetQtyPrecision()
		{
			object returnValue = null;
			Base.Transactions.Cache.RaiseFieldSelecting<SOOrder.orderQty>(null, ref returnValue, true);
			if (returnValue is PXDecimalState state)
				return state.Precision;
			return null;
		}

		protected override bool IsItemStatusDisabled(InventoryItem item)
		{
			return base.IsItemStatusDisabled(item) || item?.ItemStatus == InventoryItemStatus.NoSales;
		}

		protected override int? GetDefaultBranch()
			=> Base.Document.Current?.BranchID;
	}
}
