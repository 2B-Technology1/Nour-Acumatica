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
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;
using PX.Objects.IN.Matrix.Interfaces;
using PX.Objects.PO;

namespace PX.Objects.IN.GraphExtensions.POOrderEntryExt
{
	public class MatrixEntryExt : Matrix.GraphExtensions.SmartPanelExt<POOrderEntry, POOrder>
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
			=> Base.Transactions.Update((POLine)line);

		protected override void CreateNewLine(int? siteID, int? inventoryID, decimal qty)
			=> CreateNewLine(siteID, inventoryID, null, qty, null);

		protected override void CreateNewLine(int? siteID, int? inventoryID, string taxCategoryID, decimal qty, string uom)
		{
			POLine newline = PXCache<POLine>.CreateCopy(Base.Transactions.Insert(new POLine()));

			Base.Transactions.Cache.SetValueExt<POLine.inventoryID>(newline, inventoryID);
			newline = PXCache<POLine>.CreateCopy(Base.Transactions.Update(newline));

			if (siteID != null)
			{
				newline.SiteID = siteID;
				newline = PXCache<POLine>.CreateCopy(Base.Transactions.Update(newline));
			}

			if (uom != null)
				newline.UOM = uom;

			newline.OrderQty = qty;
			newline = Base.Transactions.Update(newline);

			if (!string.IsNullOrEmpty(taxCategoryID))
			{
				Base.Transactions.Cache.SetValueExt<POLine.taxCategoryID>(newline, taxCategoryID);
				newline = Base.Transactions.Update(newline);
			}
		}

		protected override bool IsDocumentOpen()
			=> Base.Transactions.Cache.AllowInsert;

		protected override void DeductAllocated(SiteStatusByCostCenter allocated, IMatrixItemLine line)
		{
			POLine poLine = (POLine)line;

			decimal lineQtyAvail = 0m;
			decimal lineQtyHardAvail = 0m;

			var signs = Base.FindImplementation<IItemPlanHandler<POLine>>().GetAvailabilitySigns<SiteStatusByCostCenter>(poLine);

			if (signs.SignQtyAvail != Sign.Zero)
			{
				lineQtyAvail -= signs.SignQtyAvail * (poLine.BaseOrderQty ?? 0m);
			}

			if (signs.SignQtyHardAvail != Sign.Zero)
			{
				lineQtyHardAvail -= signs.SignQtyHardAvail * (poLine.BaseOrderQty ?? 0m);
			}

			allocated.QtyAvail += lineQtyAvail;
			allocated.QtyHardAvail += lineQtyHardAvail;
		}

		protected override string GetAvailabilityMessage(int? siteID, InventoryItem item, SiteStatusByCostCenter allocated, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefix(PO.Messages.Availability_POOrder,
						uom,
						FormatQty(allocated.QtyOnHand),
						FormatQty(allocated.QtyAvail),
						FormatQty(allocated.QtyHardAvail),
						FormatQty(allocated.QtyActual),
						FormatQty(allocated.QtyPOOrders));
		}

		protected override int? GetQtyPrecision()
		{
			object returnValue = null;
			Base.Transactions.Cache.RaiseFieldSelecting<POOrder.orderQty>(null, ref returnValue, true);
			if (returnValue is PXDecimalState state)
				return state.Precision;
			return null;
		}

		protected override bool IsItemStatusDisabled(InventoryItem item)
		{
			return base.IsItemStatusDisabled(item) || item?.ItemStatus == InventoryItemStatus.NoPurchases;
		}

		protected override int? GetDefaultBranch()
			=> Base.Document.Current?.BranchID;

		protected override string GetDefaultUOM(int? inventoryID)
			=> InventoryItem.PK.Find(Base, inventoryID)?.PurchaseUnit;
	}
}
