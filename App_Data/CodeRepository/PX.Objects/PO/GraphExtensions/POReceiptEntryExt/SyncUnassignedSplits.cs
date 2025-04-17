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
using PX.Common;
using PX.Objects.IN;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SyncUnassignedSplits : PXGraphExtension<POReceiptEntry>
	{
		#region Views

		public SelectFrom<Unassigned.POReceiptLineSplit>
			.Where<Unassigned.POReceiptLineSplit.receiptType.IsEqual<POReceiptLine.receiptType.FromCurrent>
				.And<Unassigned.POReceiptLineSplit.receiptNbr.IsEqual<POReceiptLine.receiptNbr.FromCurrent>>
				.And<Unassigned.POReceiptLineSplit.lineNbr.IsEqual<POReceiptLine.lineNbr.FromCurrent>>>
			.View unassignedSplits;

		#endregion

		#region Overrides

		/// Overrides <see cref="POReceiptEntry.SyncUnassigned"/>
		[PXOverride]
		public virtual void SyncUnassigned(Action baseImpl)
		{
			baseImpl();

			var linesCache = Base.transactions.Cache;
			var modifiedLines = linesCache
				//.linesCache.Deleted //cascade delete
				.Updated
				.Concat_(linesCache.Inserted);

			foreach (POReceiptLine line in modifiedLines)
			{
				SyncUnassigned(line);
			}
		}

		#endregion

		#region Implementation

		protected virtual bool SupportsUnassignedSplits(POReceiptLine line)
		{
			if (!Base.LineSplittingExt.IsLSEntryEnabled(line))
				return false;

			var item = InventoryItem.PK.Find(Base, line.InventoryID);

			var lotSerClass = INLotSerClass.PK.Find(Base, item?.LotSerClassID);
			if (lotSerClass != null && lotSerClass.LotSerTrack.IsIn(INLotSerTrack.SerialNumbered, INLotSerTrack.LotNumbered)
				&& lotSerClass.LotSerAssign == INLotSerAssign.WhenReceived
				&& lotSerClass.AutoNextNbr != true)
				return true;

			return false;
		}

		protected virtual void SyncUnassigned(POReceiptLine line)
		{
			if ((line.UnassignedQty ?? 0) == 0
				|| !SupportsUnassignedSplits(line))
			{
				if (line.IsUnassigned == true)
				{
					DeleteUnassignedSplits(line, null);
					line.IsUnassigned = false;
				}
				return;
			}

			Base.transactions.Current = line;

			var item = InventoryItem.PK.Find(Base, line.InventoryID);
			var lotSerClass = INLotSerClass.PK.Find(Base, item.LotSerClassID);
			var splitByOne = lotSerClass?.LotSerTrack == INLotSerTrack.SerialNumbered;

			var unassignedQty = line.UnassignedQty ?? 0;

			var splitCache = unassignedSplits.Cache;
			var oldUnassignedSplits = line.IsUnassigned == true
				? SelectUnassignedSplits(line)
				: new List<Unassigned.POReceiptLineSplit>();

			line.IsUnassigned = true;

			while (unassignedQty > 0)
			{
				decimal splitQty = splitByOne
					? 1
					: unassignedQty;

				var split = oldUnassignedSplits.FirstOrDefault();

				if (split != null)
				{
					//update an existing
					oldUnassignedSplits.Remove(split);

					InitUnassignedSplit(split, line, item.BaseUnit, splitQty);
					if (split.LocationID == null)
						splitCache.SetDefaultExt<Unassigned.POReceiptLineSplit.locationID>(split);

					splitCache.Update(split);
				}
				else
				{
					//create new
					split = new Unassigned.POReceiptLineSplit
					{
						ReceiptType = line.ReceiptType,
						ReceiptNbr = line.ReceiptNbr,
						LineNbr = line.LineNbr
					};
					InitUnassignedSplit(split, line, item.BaseUnit, splitQty);
					PXParentAttribute.SetParent(splitCache, split, typeof(POReceiptLine), line);

					splitCache.Insert(split);
				}
				unassignedQty -= splitQty;
			}

			//remove unused splits
			foreach(var split in oldUnassignedSplits)
				splitCache.Delete(split);
		}

		protected virtual void DeleteUnassignedSplits(POReceiptLine line, IEnumerable<Unassigned.POReceiptLineSplit> unassignedSplitRows)
		{
			if (unassignedSplitRows == null)
				unassignedSplitRows = SelectUnassignedSplits(line);

			foreach (Unassigned.POReceiptLineSplit s in unassignedSplitRows)
			{
				unassignedSplits.Cache.Delete(s);
			}
		}

		protected virtual List<Unassigned.POReceiptLineSplit> SelectUnassignedSplits(POReceiptLine line)
		{
			return unassignedSplits.View
				.SelectMultiBound(new[] { line })
				.RowCast<Unassigned.POReceiptLineSplit>()
				.ToList();
		}

		protected virtual void InitUnassignedSplit(Unassigned.POReceiptLineSplit split, POReceiptLine line, string uom, decimal? qty)
		{
			split.LineType = line.LineType;

			split.InventoryID = line.InventoryID;
			split.SubItemID = line.SubItemID;

			split.SiteID = line.SiteID;
			split.LocationID = line.LocationID;

			split.LotSerialNbr = string.Empty;
			split.ExpireDate = line.ExpireDate;

			split.UOM = uom;
			split.Qty = qty;
			split.BaseQty = qty;

			split.InvtMult = line.InvtMult;
			split.OrigPlanType = line.OrigPlanType;

			split.ProjectID = line.ProjectID;
			split.TaskID = line.TaskID;
		}

		#endregion
	}
}
