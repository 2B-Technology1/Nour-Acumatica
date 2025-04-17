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

using PX.Common;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AM.GraphExtensions
{
	/// <summary>
	/// Manufacturing extension to <see cref="SOOrderLineSplittingAllocatedExtension"/>
	/// </summary>
	[PXProtectedAccess(typeof(SOOrderLineSplittingAllocatedExtension))]
	public abstract class SOOrderLineSplittingAllocatedExtensionAMExtension : PXGraphExtension<SOOrderLineSplittingAllocatedExtension, SOOrderLineSplittingExtension, SOOrderEntry>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();

		[PXProtectedAccess]
		protected abstract bool IsSplitRequired(SOLine line);

		[PXProtectedAccess]
		protected abstract SOLineSplit[] SelectSplits(SOLineSplit split, bool compareInventoryID = true);

		[PXProtectedAccess]
		protected abstract PXCache<SOLineSplit> SplitCache { get; }

		/// <summary>
		/// Overrides <see cref="SOOrderLineSplittingExtension.EventHandler(ManualEvent.Row{SOLine}.Updated.Args)"/>
		/// </summary>
		[PXOverride]
		public virtual void EventHandlerInternal(ManualEvent.Row<SOLine>.Updated.Args e, Action<ManualEvent.Row<SOLine>.Updated.Args> base_EventHandler)
		{
			if (CanSyncProdCreateSplits(e.Cache, e.Row, e.OldRow))
			{
				var split = SelectFirstSplit(e.Row);
				if (split != null)
				{
					// Must sync before base to make sure increase of line qty will also update existing splits correctly
					SyncProdCreateSplit(e.Row, split);
				}
			}

			base_EventHandler(e);
		}

		/// <summary>
		/// Selection of Mark for Production <see cref="SOLineSplit"/> first. If one doesn't exist then select first available open <see cref="SOLineSplit"/>.
		/// </summary>
		protected virtual SOLineSplit SelectFirstSplit(SOLine soLine)
		{
			return SelectFirstSplit(SelectSplits(soLine));
		}

		/// <summary>
		/// Selection of expected first <see cref="SOLineSplit"/>.
		/// </summary>
		/// <param name="splits">Collection of <see cref="SOLineSplit"/> to select from.</param>
		/// <param name="selectFirstProdCreate">when true, find the first mark for production <see cref="SOLineSplit"/>, else will find the first non mark for production <see cref="SOLineSplit"/>.</param>
		protected virtual SOLineSplit SelectFirstSplit(IEnumerable<SOLineSplit> splits, bool selectFirstProdCreate = true)
		{
			if (splits == null)
			{
				return null;
			}

			var firstProdCreate = (SOLineSplit)null;
			var firstNonProdCreate = (SOLineSplit)null;

			foreach (SOLineSplit split in splits)
			{
				if (!IsOpenSplit(split))
				{
					continue;
				}

				if (firstProdCreate == null && selectFirstProdCreate && split.AMProdCreate == true)
				{
					firstProdCreate = split;
				}

				if (firstNonProdCreate == null && split.AMProdCreate != true)
				{
					firstNonProdCreate = split;
				}

				if (firstProdCreate != null && firstNonProdCreate != null)
				{
					break;
				}
			}

			return firstProdCreate ?? firstNonProdCreate;
		}

		protected virtual bool CanSyncProdCreateSplits(PXCache cache, SOLine currentRow, SOLine oldRow)
		{
			//rows not null, not completed, allocation enabled, linetype is inventory or NS
			var notnull = currentRow != null && !currentRow.Completed.GetValueOrDefault() && Base2.IsAllocationEntryEnabled && oldRow != null && currentRow.LineType.IsIn(SOLineType.Inventory, SOLineType.NonInventory);
			if (!notnull)
				return false;

			//key fields match but AM fields do not match
			var keymatch =  cache.ObjectsEqual<SOLine.inventoryID, SOLine.subItemID, SOLine.siteID, SOLine.invtMult, SOLine.uOM, SOLine.projectID, SOLine.taskID>(currentRow, oldRow)
				&& !cache.ObjectsEqual<SOLineExt.aMProdCreate, SOLineExt.aMProdStatusID, SOLineExt.aMOrderType, SOLineExt.aMProdOrdID, SOLineExt.aMProdQtyComplete, SOLineExt.aMProdBaseQtyComplete>(currentRow, oldRow);

			if (keymatch)
				return true;

			//if siteid was the only thing that changed, sync the splits
			var sitechange = cache.ObjectsEqual<SOLine.inventoryID, SOLine.subItemID, SOLine.invtMult, SOLine.uOM, SOLine.projectID, SOLine.taskID>(currentRow, oldRow)
				&& !cache.ObjectsEqual<SOLine.siteID>(currentRow, oldRow);

			return sitechange;
		}

		protected virtual bool IsOpenSplit(SOLineSplit split) => split != null && !split.IsAllocated.GetValueOrDefault() && !split.Completed.GetValueOrDefault();

		protected virtual SOLineSplit SyncProdCreateSplit(SOLine soLine, SOLineSplit split)
		{
			if (!IsOpenSplit(split))
			{
				return null;
			}

			SOLineSplit splitUpd = PXCache<SOLineSplit>.CreateCopy(split);
			var soLineExt = PXCache<SOLine>.GetExtension<SOLineExt>(soLine);
			if (soLine == null || soLineExt == null || soLine.Completed.GetValueOrDefault())
			{
				return null;
			}

			var splitExt = PXCache<SOLineSplit>.GetExtension<SOLineSplitExt>(splitUpd);
			if (splitExt == null)
			{
				return null;
			}

			splitUpd.AMProdCreate = soLineExt.AMProdCreate.GetValueOrDefault();
			splitExt.AMOrderType = soLineExt.AMOrderType;
			splitExt.AMProdOrdID = soLineExt.AMProdOrdID;
			splitExt.AMProdStatusID = soLineExt.AMProdStatusID;
			splitExt.AMProdQtyComplete = soLineExt.AMProdQtyComplete;
			splitExt.AMProdBaseQtyComplete = soLineExt.AMProdBaseQtyComplete;
			return (SOLineSplit)SplitCache.Update(splitUpd);
		}

		/// <summary>
		/// Overrides <see cref="SOOrderLineSplittingExtension.EventHandler(ManualEvent.Row{SOLineSplit}.Inserting.Args)"/>
		/// </summary>
		[PXOverride]
		public virtual void EventHandler(ManualEvent.Row<SOLineSplit>.Inserting.Args e,
			Action<ManualEvent.Row<SOLineSplit>.Inserting.Args> base_EventHandler)
		{
			if (e.Row != null)
			{
				var parent = PXParentAttribute.SelectParent<SOLine>(e.Cache, e.Row);
				if (parent != null)
				{
					if (PXCache<SOLine>.GetExtension<SOLineExt>(parent)?.AMProdCreate == true)
					{
						var siblingSplit = SelectFirstSplit(PXParentAttribute.SelectSiblings(e.Cache, e.Row, typeof(SOLine)).OfType<SOLineSplit>());
						if (siblingSplit == null || siblingSplit.AMProdCreate != true)
						{
							e.Row.AMProdCreate = true;
						}
					}
				}
			}

			base_EventHandler(e);
		}

		protected virtual void _(Events.RowDeleted<SOLineSplit> e)
		{
			if (e.Row == null)
			{
				return;
			}

			var parent = PXParentAttribute.SelectParent<SOLine>(e.Cache, e.Row);
			if (parent == null)
			{
				return;
			}

			var parentExt = PXCache<SOLine>.GetExtension<SOLineExt>(parent);
			if (!Base.Transactions.Cache.GetStatus(parent).IsDeleted() && parentExt?.AMProdCreate == true)
			{
				var existingNonProdCreateSplit = SelectFirstSplit(PXParentAttribute.SelectSiblings(e.Cache, e.Row, typeof(SOLine)).OfType<SOLineSplit>(), false);
				if (existingNonProdCreateSplit != null)
				{
					existingNonProdCreateSplit.AMProdCreate = true;
					Base.splits.Cache.SetStatus(existingNonProdCreateSplit, PXEntryStatus.Updated);
				}
			}
		}
	}
}
