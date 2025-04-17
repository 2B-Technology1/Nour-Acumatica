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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;
using System.Linq;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
	/// <summary>
	/// Reusable business object for processing multi-level bom logic
	/// </summary>
	public abstract class MultiLevelBomProcessBase<TGraph, TPrimary> :
		PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		public virtual MultiLevelBomProcessFilter GetDefaultFilter => new MultiLevelBomProcessFilter
		{
			IsSingleLevel = false,
			EffectiveDate = Base?.Accessinfo?.BusinessDate ?? Common.Dates.Today,
			ExcludedBomStatuses = new List<string> { AMBomStatus.Archived }
		};

		public virtual List<MultiLevelBomResult> GetMultiLevelBomResults(MultiLevelBomProcessFilter filter)
		{
#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
			var results = new List<MultiLevelBomResult>();

			foreach (PXResult<AMBomItem, AMBomItemActiveAggregate, AMBomItemBomDefaults, InventoryItem, INItemSite> result in GetBomResultset(filter))
			{
				var bomItem = result.GetItem<AMBomItem>();
				var isDefualt = IsDefaultBOM(bomItem, (AMBomItemBomDefaults)result, ((AMBomItemActiveAggregate)result)?.RevisionID);
				var bomResult = new MultiLevelBomResult(0, bomItem, result.GetItem<InventoryItem>(), result.GetItem<INItemSite>())
				{
					IsDefaultBOM = isDefualt
				};

				results.Add(bomResult);

				if (filter.IsSingleLevel)
				{
					continue;
				}

				var processedBomRevs = new HashSet<string>();
				results = DrillDown(bomItem, 1, ref processedBomRevs, null, filter, results);
			}

#if DEBUG
            sw.Stop();
            AMDebug.TraceWriteLine("Multi Level Process Time: {0}", PXTraceHelper.CreateTimespanMessage(sw.Elapsed));
#endif
			return results;
		}

		public List<MultiLevelBomResult> CurrentBomResults = new List<MultiLevelBomResult>();

		public virtual List<MultiLevelBomResult> DrillDown(IBomRevision bomRevision, int level, ref HashSet<string> processedBomRevisions, AMBomItem[] drillDownBoms, MultiLevelBomProcessFilter filter, List<MultiLevelBomResult> currentBomResults)
		{
			CurrentBomResults = currentBomResults;
			return DrillDown(bomRevision, level, ref processedBomRevisions, drillDownBoms, filter);
		}

		// Method to change in 24R1 to method with List<MultiLevelBomResult> parameter
		public virtual List<MultiLevelBomResult> DrillDown(IBomRevision bomRevision, int level, ref HashSet<string> processedBomRevisions, AMBomItem[] drillDownBoms, MultiLevelBomProcessFilter filter)
		{
			var bomResults = new List<MultiLevelBomResult>();
			bomResults.AddRange(CurrentBomResults);
			if (level >= LowLevel.MaxLowLevel)
			{
				PXTrace.WriteWarning(Messages.GetLocal(Messages.MaxLevelsReached));
				return bomResults;
			}

			var bomResultExisting = MultiLevelBomResult.Find(bomResults, bomRevision);
			if (level > 1 && bomResultExisting != null)
			{
				var isLowerLevel = bomResultExisting.Level >= level;
				if (isLowerLevel && !bomResultExisting.IsMultiSubassembly)
				{
					bomResultExisting.IsMultiSubassembly = true;
				}

				// Not increasing level so no need to process again
				// or Avoids query again to AMBomMatlDrillDown
				if (isLowerLevel ||
					(bomResultExisting.Level < level && bomResultExisting.IsMultiSubassembly && !bomResultExisting.HasChildKeys))
				{
					return bomResults;
				}
			}

			var bomRevs = new AMBomItem[LowLevel.MaxLowLevel];
			if (drillDownBoms != null)
			{
				Array.Copy(drillDownBoms, bomRevs, LowLevel.MaxLowLevel);
			}

			var levelBomList = new List<AMBomItem>();

			foreach (PXResult<AMBomMatlDrillDown, AMBomItem, AMBomItemActiveAggregate, InventoryItem, AMBomItemBomDefaults> result in GetSubassemblyMaterialResultset(bomRevision, filter))
			{
				var bomItem = result.GetItem<AMBomItem>();
				var bomInventoryItem = result.GetItem<InventoryItem>();
				var bomMatl = result.GetItem<AMBomMatlDrillDown>();
				var bomDefaults = result.GetItem<AMBomItemBomDefaults>();
				if (string.IsNullOrWhiteSpace(bomItem?.BOMID) || string.IsNullOrWhiteSpace(bomMatl.BOMID))
				{
					continue;
				}

				if((filter.DefaultBomsOnly && !bomDefaults.IsDefaultBOM.GetValueOrDefault()) || !IsMaterialEffective(filter, bomMatl))
				{
					continue;
				}

				var isRecursive = ContainsBom(bomRevs, bomItem);
				var isProcessed = !processedBomRevisions.Add(string.Join(":", bomItem.BOMID, bomItem.RevisionID));

				if (isRecursive)
				{
					var recursiveMessage = GetRecursiveMessage(bomRevs, bomItem, bomInventoryItem);
					if(recursiveMessage == null)
					{
						continue;
					}

					PXTrace.WriteWarning(recursiveMessage);

					var bomResultRecursive = MultiLevelBomResult.Find(bomResults, bomItem);
					if (bomResultRecursive != null)
					{
						if (level <= bomResultRecursive.Level && bomResultRecursive.IsRecursive == true)
						{
							continue;
						}

						bomResultRecursive.IsMultiSubassembly = true;
						bomResultRecursive.Level = level;
						bomResultRecursive.RecursiveDetail = bomRevs;
						continue;
					}

					bomResults.Add(new MultiLevelBomResult(level, bomItem, bomInventoryItem, null)
					{
						IsDefaultBOM = bomDefaults?.IsDefaultBOM == true,
						IsPurchasedBOM = !IsManufacturedItem(filter, bomDefaults),
						RecursiveDetail = bomRevs
					});

					continue;
				}

				if (isProcessed)
				{
					continue;
				}

				var isMfgItem = IsManufacturedItem(filter, bomDefaults);
				if(!isMfgItem && filter?.IgnoreReplenishmentSettings == true)
				{
					continue;
				}

				levelBomList.Add(bomItem);
				if (bomResultExisting != null)
				{
					bomResultExisting.AddChildKey(bomItem);
				}

				var bomResult = MultiLevelBomResult.Find(bomResults, bomItem);
				var isExistingResult = bomResult != null;
				if (isExistingResult)
				{
					if (level <= bomResult.Level)
					{
						continue;
					}

					bomResult.IsMultiSubassembly = true;
					bomResult.Level = level;
					continue;
				}

				bomResults.Add(new MultiLevelBomResult(level, bomItem, bomInventoryItem, null)
				{
					IsDefaultBOM = bomDefaults?.IsDefaultBOM == true,
					IsPurchasedBOM = !isMfgItem
				});
			}

#if DEBUG
			var cntr = 0;
#endif
			foreach (var bomItem in levelBomList)
			{
#if DEBUG
				var nxtLevel = level + 1;
				AMDebug.TraceWriteLine($"[{nxtLevel}-{++cntr}] DrillDown({bomItem.BOMID}, {bomItem.RevisionID}, {nxtLevel}, processedBomRevisions: {processedBomRevisions?.Count})   [From {bomRevision?.BOMID}:{bomRevision?.RevisionID}]".PadLeft(3 * level, '-'));
#endif
				if (level.BetweenInclusive(1, LowLevel.MaxLowLevel))
				{
					bomRevs[level - 1] = bomItem;
				}
				bomResults = DrillDown(bomItem, level + 1, ref processedBomRevisions, bomRevs, filter, bomResults);
			}

			return bomResults;
		}

		public virtual bool IsMaterialEffective(MultiLevelBomProcessFilter filter, AMBomMatlDrillDown matlOper)
		{
			return IsMaterialEffective(filter, matlOper?.EffDate, matlOper?.ExpDate);
		}

		public virtual bool IsMaterialEffective(MultiLevelBomProcessFilter filter, DateTime? effDate, DateTime? expDate)
		{
			return filter?.EffectiveDate == null ||
				(effDate == null && expDate == null) ||
				effDate.LessThanOrEqualTo(filter.EffectiveDate)
					|| expDate.GreaterThan(filter.EffectiveDate);
		}

		public virtual string GetReplenishmentSource(MultiLevelBomProcessFilter filter, InventoryItem item, int? siteID)
        {
			return GetReplenishmentSource(filter, item, INItemSite.PK.Find(Base, item?.InventoryID, siteID));
        }

		public virtual string GetReplenishmentSource(MultiLevelBomProcessFilter filter, InventoryItem invItem, INItemSite inItemSite)
		{
			var ignoreSource = filter?.IgnoreReplenishmentSettings == true;
			var inSiteExtension = inItemSite == null ? null : PXCache<INItemSite>.GetExtension<INItemSiteExt>(inItemSite);
			var itemExtension = invItem == null ? null : PXCache<InventoryItem>.GetExtension<InventoryItemExt>(invItem);
			return GetReplenishmentSource(ignoreSource, itemExtension?.AMBOMID, invItem?.ReplenishmentSource, inSiteExtension?.AMBOMID, inItemSite?.ReplenishmentSource);
		}

		public virtual string GetReplenishmentSource(bool ignoreReplenishmentSettings, string itemBomID, string itemReplenishmentSource, string itemSiteBomID, string itemSiteReplenishmentSource)
		{
			var hasBom = !string.IsNullOrWhiteSpace(itemSiteBomID ?? itemBomID);
			if(hasBom && ignoreReplenishmentSettings)
			{
				return INReplenishmentSource.Manufactured;
			}

			var repSource = (itemSiteReplenishmentSource ?? itemReplenishmentSource) ?? INReplenishmentSource.None;
			return !hasBom && repSource == INReplenishmentSource.Manufactured ? INReplenishmentSource.Purchased : repSource;
		}

		public virtual string GetRecursiveMessage(AMBomItem[] bomLevels, IBomRevision recursiveBom, InventoryItem inventoryItem)
		{
			if (bomLevels == null || bomLevels.Length == 0 || recursiveBom?.BOMID == null)
			{
				return null;
			}

			var sb = new System.Text.StringBuilder();
			sb.AppendLine(Messages.GetLocal(Messages.RecursiveBomFound, recursiveBom.BOMID, recursiveBom.RevisionID));
			var lastLevel = -1;
			for (var i = 0; i < bomLevels.Length; i++)
			{
				var bom = bomLevels[i];
				if (bom?.BOMID == null)
				{
					continue;
				}

				lastLevel = i + 1;

				var inventoryCD = InventoryItem.PK.Find(Base, bom.InventoryID)?.InventoryCD ?? string.Empty;

				sb.Append($"[{i + 1}] {bom.BOMID}:{bom.RevisionID} {inventoryCD?.Trim()} -> ");
			}

			if (lastLevel != -1)
			{
				var inventoryCD = inventoryItem?.InventoryCD ?? string.Empty;
				sb.Append($"[{lastLevel+1}] {recursiveBom.BOMID}:{recursiveBom.RevisionID} {inventoryCD.Trim()}");
			}

			return sb.ToString();
		}

		private static bool ContainsBom(IBomRevision[] bomRevisions, IBomRevision bomRevision)
		{
			return bomRevisions != null && bomRevision?.BOMID != null &&
					Array.Find(bomRevisions, b => b?.BOMID == bomRevision.BOMID && b?.RevisionID == bomRevision.RevisionID) != null;
		}

		public virtual bool IsManufacturedItem(MultiLevelBomProcessFilter filter, AMBomItemBomDefaults bomItemDefaults)
		{
			return IsManufacturedItem(filter?.IgnoreReplenishmentSettings == true, bomItemDefaults?.ItemBOMID, bomItemDefaults?.ItemReplenishmentSource, bomItemDefaults?.ItemSiteBOMID, bomItemDefaults?.ItemSiteReplenishmentSource);
		}

		public virtual bool IsManufacturedItem(bool ignoreReplenishmentSettings, string itemBomID, string itemReplenishmentSource, string itemSiteBomID, string itemSiteReplenishmentSource)
		{
			return GetReplenishmentSource(ignoreReplenishmentSettings, itemBomID, itemReplenishmentSource, itemSiteBomID, itemSiteReplenishmentSource) == INReplenishmentSource.Manufactured;
		}

		public virtual Tuple<PXSelectBase<AMBomMatlDrillDown>, List<object>> GetSubassemblyMaterialCommand(IBomRevision bomRevision, MultiLevelBomProcessFilter filter)
		{
			var parameters = new List<object>();
			var matlCommand = GetSubassemblyMaterialCommandBase();

			parameters.Add(bomRevision?.BOMID);
			parameters.Add(bomRevision?.RevisionID);

			if (filter?.EffectiveDate != null)
			{
				matlCommand.WhereAnd<Where<AMBomItem.effStartDate, IsNull,
				Or<AMBomItem.effStartDate, LessEqual<Required<AMBomItem.effStartDate>>,
				And<Where<AMBomItem.effEndDate, IsNull,
				Or<AMBomItem.effEndDate, GreaterEqual<Required<AMBomItem.effEndDate>>>>>>>>();
				parameters.Add(filter.EffectiveDate);
				parameters.Add(filter.EffectiveDate);
			}

			if (filter?.ExcludedBomStatuses != null)
			{
				foreach (var excludeStatus in filter.ExcludedBomStatuses)
				{
					matlCommand.WhereAnd<Where<AMBomItem.status, NotEqual<Required<AMBomItem.status>>>>();
					parameters.Add(excludeStatus);
				}
			}

			return new Tuple<PXSelectBase<AMBomMatlDrillDown>, List<object>>(matlCommand, parameters);
		}

		public virtual PXSelectBase<AMBomMatlDrillDown> GetSubassemblyMaterialCommandBase()
		{
			return new PXSelectJoin<
				AMBomMatlDrillDown,
				InnerJoin<AMBomItem,
					On<AMBomMatlDrillDown.inventoryID, Equal<AMBomItem.inventoryID>>,
				LeftJoin<AMBomItemActiveAggregate,
					On<AMBomItem.bOMID, Equal<AMBomItemActiveAggregate.bOMID>>,
				InnerJoin<InventoryItem,
					On<AMBomItem.inventoryID, Equal<InventoryItem.inventoryID>>,
				LeftJoin<AMBomItemBomDefaults,
					On<AMBomItem.bOMID, Equal<AMBomItemBomDefaults.bOMID>,
					And<AMBomItem.revisionID, Equal<AMBomItemBomDefaults.revisionID>>>>>>>,
				Where<AMBomMatlDrillDown.bOMID, Equal<Required<AMBomMatlDrillDown.bOMID>>,
					And<AMBomMatlDrillDown.revisionID, Equal<Required<AMBomMatlDrillDown.revisionID>>>>,
				OrderBy<
					Asc<AMBomMatlDrillDown.operationCD,
					Asc<AMBomMatlDrillDown.sortOrder,
					Asc<AMBomMatlDrillDown.lineID>>>>
						>(Base);
		}

		public virtual PXResultset<AMBomMatlDrillDown> GetSubassemblyMaterialResultset(IBomRevision bomRevision, MultiLevelBomProcessFilter filter)
		{
			var resultCommand = GetSubassemblyMaterialCommand(bomRevision, filter);
			using (new PXFieldScope(resultCommand.Item1.View, GetSubassemblyMaterialQueryFieldScope()))
			{
			return resultCommand.Item1.Select(resultCommand.Item2?.ToArray());
		}
		}

		public virtual IEnumerable<Type> GetSubassemblyMaterialQueryFieldScope()
		{
			yield return typeof(AMBomMatlDrillDown);
			yield return typeof(AMBomItem);
			yield return typeof(AMBomItemActiveAggregate);
			yield return typeof(AMBomItemBomDefaults);

			yield return typeof(InventoryItem.inventoryID);
			yield return typeof(InventoryItem.inventoryCD);
			yield return typeof(InventoryItem.descr);
			yield return typeof(InventoryItem.valMethod);
			yield return typeof(InventoryItem.itemClassID);
			yield return typeof(InventoryItem.replenishmentSource);
			yield return typeof(InventoryItemExt.aMBOMID);
			yield return typeof(InventoryItemExt.aMMinOrdQty);
			yield return typeof(InventoryItemExt.aMMaxOrdQty);
			yield return typeof(InventoryItemExt.aMLotSize);
			yield return typeof(InventoryItemExt.aMQtyRoundUp);
		}

		public virtual PXResultset<AMBomItem> GetBomResultset(MultiLevelBomProcessFilter filter)
		{
			var resultCommand = GetBomResultCommand(filter);
			return resultCommand.Item1.Select(resultCommand.Item2?.ToArray());
		}

		public virtual PXSelectBase<AMBomItem> GetBomResultCommandBase()
		{
			return new PXSelectJoin<AMBomItem,
				LeftJoin<AMBomItemActiveAggregate,
					On<AMBomItem.bOMID, Equal<AMBomItemActiveAggregate.bOMID>>,
				LeftJoin<AMBomItemBomDefaults,
					On<AMBomItem.bOMID, Equal<AMBomItemBomDefaults.bOMID>,
						And<AMBomItem.revisionID, Equal<AMBomItemBomDefaults.revisionID>>>,
				InnerJoin<InventoryItem,
					On<AMBomItem.inventoryID, Equal<InventoryItem.inventoryID>>,
				LeftJoin<INItemSite, On<AMBomItem.inventoryID, Equal<INItemSite.inventoryID>,
					And<INItemSite.siteID, Equal<AMBomItem.siteID>>>>>>>>(Base);
		}

		public virtual Tuple<PXSelectBase<AMBomItem>, List<object>> GetBomResultCommand(MultiLevelBomProcessFilter filter)
		{
			var bomCommand = GetBomResultCommandBase();
			var parameters = new List<object>();

			if(filter == null)
			{
				return new Tuple<PXSelectBase<AMBomItem>, List<object>>(bomCommand, parameters);
			}

			var isByBom = false;
			if (!string.IsNullOrWhiteSpace(filter.BOMID))
            {
                bomCommand.WhereAnd<Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>>>();
				parameters.Add(filter.BOMID);
				isByBom = true;
            }

			var isByRevision = false;
            if (isByBom && !string.IsNullOrWhiteSpace(filter.RevisionID))
            {
                bomCommand.WhereAnd<Where<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>>>();
				parameters.Add(filter.RevisionID);
				isByRevision = true;
            }

			if (!isByRevision && filter.EffectiveDate != null)
			{
				bomCommand.WhereAnd<Where<AMBomItem.effStartDate, IsNull,
				Or<AMBomItem.effStartDate, LessEqual<Required<AMBomItem.effStartDate>>,
				And<Where<AMBomItem.effEndDate, IsNull,
				Or<AMBomItem.effEndDate, GreaterEqual<Required<AMBomItem.effEndDate>>>>>>>>();
				parameters.Add(filter.EffectiveDate);
				parameters.Add(filter.EffectiveDate);
			}

			if (!isByRevision && filter.ExcludedBomStatuses != null)
			{
				foreach (var excludeStatus in filter.ExcludedBomStatuses)
				{
					bomCommand.WhereAnd<Where<AMBomItem.status, NotEqual<Required<AMBomItem.status>>>>();
					parameters.Add(excludeStatus);
				}
			}

			if(isByBom)
			{
				return new Tuple<PXSelectBase<AMBomItem>, List<object>>(bomCommand, parameters);
			}

			if (filter.ItemClassID != null)
            {
                bomCommand.WhereAnd<Where<InventoryItem.itemClassID, Equal<Required<InventoryItem.itemClassID>>>>();
				parameters.Add(filter.ItemClassID);
            }

            if (filter.SiteID != null)
            {
                bomCommand.WhereAnd<Where<AMBomItem.siteID, Equal<Required<AMBomItem.siteID>>>>();
				parameters.Add(filter.SiteID);
            }

            if (filter.InventoryID != null)
            {
                bomCommand.WhereAnd<Where<AMBomItem.inventoryID, Equal<Required<AMBomItem.inventoryID>>>>();
				parameters.Add(filter.InventoryID);
            }

            if (PXAccess.FeatureInstalled<CS.FeaturesSet.subItem>() && filter.SubItemID != null)
            {
                bomCommand.WhereAnd<Where<AMBomItem.subItemID, Equal<Required<AMBomItem.subItemID>>>>();
				parameters.Add(filter.SubItemID);
            }

			return new Tuple<PXSelectBase<AMBomItem>, List<object>>(bomCommand, parameters);
		}

		public virtual bool IsDefaultBOM(AMBomItem bomItem, AMBomItemBomDefaults bomDefault, string defaultActiveRev)
		{
			return IsMatchingRevision(bomItem, defaultActiveRev) && bomDefault?.IsDefaultBOM == true;
		}

		public virtual bool IsMatchingRevision(AMBomItem bomItem, string defaultActiveRev)
		{
			return !string.IsNullOrWhiteSpace(defaultActiveRev) && bomItem?.RevisionID != null && bomItem.RevisionID.Equals(defaultActiveRev);
		}

		public virtual PXResultset<AMBomMatl> GetBomMaterial(AMBomItem bomItem)
		{
			return GetBomMaterial(bomItem?.BOMID, bomItem?.RevisionID);
		}

		public virtual PXResultset<AMBomMatl> GetBomMaterial(string bomID, string revisionID)
		{
			return SelectFrom<AMBomMatl>
				.InnerJoin<AMBomOper>
					.On<AMBomMatl.bOMID.IsEqual<AMBomOper.bOMID>
						.And<AMBomMatl.revisionID.IsEqual<AMBomOper.revisionID>
						.And<AMBomMatl.operationID.IsEqual<AMBomOper.operationID>>>>
				.InnerJoin<InventoryItem>
					.On<AMBomMatl.inventoryID.IsEqual<InventoryItem.inventoryID>>
				.Where<AMBomMatl.bOMID.IsEqual<@P.AsString>
					.And<AMBomMatl.revisionID.IsEqual<@P.AsString>>
					>.View.Select(Base, bomID, revisionID);
		}

		public virtual decimal? GetItemUnitCost(InventoryItem inventoryItem, INItemSite itemSite, int? siteid, bool usePending)
		{
			decimal? unitCost = null;
			if (inventoryItem?.ValMethod == null)
			{
				return unitCost;
			}

			// [1.1] Get the InItemSite Record
			itemSite = itemSite ?? InventoryHelper.CacheQueryINItemSite(Base.Caches[typeof(INItemSite)], inventoryItem.InventoryID, siteid);
			if (inventoryItem.ValMethod == INValMethod.Standard)
			{
				unitCost = GetItemStandardCost(inventoryItem, itemSite, usePending);
				if (unitCost != null)
				{
					return unitCost;
				}
			}

			unitCost = GetItemUnitCost(itemSite);
			if (unitCost != null)
			{
				return unitCost;
			}

			// [1.2] Get the item cost Record (same lookup order as bom/prod matl unit cost default)
			INItemCost itemCost = INItemCost.PK.Find(Base, inventoryItem.InventoryID, Base.Accessinfo.BaseCuryID);

			unitCost = GetItemUnitCost(itemCost);
			if (unitCost != null)
			{
				return unitCost;
			}

			// [2] Get the Default site for the Inventory Item
			itemSite = PXSelect<INItemSite,
				Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
					And<INItemSite.isDefault, Equal<True>>>>.Select(Base, inventoryItem.InventoryID);

			unitCost = GetItemUnitCost(itemSite);
			if (unitCost != null)
			{
				return unitCost;
			}

			// [3] Get the first found site for the Inventory Item
			itemSite = PXSelect<INItemSite,
				Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>
				>>.Select(Base, inventoryItem.InventoryID);

			unitCost = GetItemUnitCost(itemSite);
			return unitCost ?? 0m;
		}

		public virtual decimal? GetItemStandardCost(InventoryItem inventoryItem, INItemSite itemSite, bool usePending)
		{
			if (itemSite != null && itemSite.StdCostOverride == true)
			{
				return usePending ? itemSite.PendingStdCost : itemSite.StdCost;
			}

			if(inventoryItem != null && PXAccess.FeatureInstalled<CS.FeaturesSet.multipleBaseCurrencies>())
			{
				var itemCurySettings = InventoryItemCurySettings.PK.Find(Base, inventoryItem.InventoryID, Base.Accessinfo.BaseCuryID);
				if(itemCurySettings != null)
				{
					return usePending ? itemCurySettings.PendingStdCost : itemCurySettings.StdCost;
				}
			}

			return usePending ? inventoryItem?.PendingStdCost : inventoryItem?.StdCost;
		}

		public virtual decimal? GetItemUnitCost(INItemSite inItemSite)
        {
            if (inItemSite != null && (inItemSite.TranUnitCost != null || inItemSite.LastCost != null))
            {
                if ((inItemSite.TranUnitCost ?? 0) == 0 && (inItemSite.LastCost ?? 0) != 0)
                {
                    return inItemSite.LastCost;
                }

                return inItemSite.TranUnitCost;
            }

            return null;
        }

        public virtual decimal? GetItemUnitCost(INItemCost inItemCost)
        {
            if (inItemCost != null && (inItemCost.TranUnitCost != null || inItemCost.LastCost != null))
            {
                if ((inItemCost.TranUnitCost ?? 0) == 0 && (inItemCost.LastCost ?? 0) != 0)
                {
                    return inItemCost.LastCost;
                }

                return inItemCost.TranUnitCost ?? 0;
            }

            return null;
        }
	}

	/// <summary>
	/// Result of reusable multi level bom process
	/// </summary>
	[Serializable]
	[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class MultiLevelBomResult
	{
		internal string DebuggerDisplay => $"{Key}-{Level}";

		public int InventoryID  { get; private set; }
		public int Level { get; set; }
		public bool IsDefaultBOM { get; set; }
		public bool IsRecursive => RecursiveDetail != null && RecursiveDetail.Length > 0;
		public AMBomItem[] RecursiveDetail { get; set; }
		public bool IsPurchasedBOM { get; set; }
		public AMBomItem BomItem { get; private set; }
		public InventoryItem InventoryItem { get; private set; }
		public INItemSite ItemSite { get; private set; }
		[Obsolete]
		public AMBomMatlDrillDown MatlOper { get; private set; }
		[Obsolete]
		public decimal UnitCost { get; set; }
		public string Key { get; private set; }
		public List<string> ChildKeys { get; private set; }
		public bool HasChildKeys => ChildKeys != null && ChildKeys.Count > 0;
		public bool IsMultiSubassembly { get; set; }

		public MultiLevelBomResult(int level, AMBomItem bomItem, InventoryItem inventoryItem, INItemSite itemSite)
		{
			Level = level;
			BomItem = bomItem ?? throw new ArgumentNullException(nameof(bomItem));
			InventoryItem = inventoryItem ?? throw new ArgumentNullException(nameof(inventoryItem));
			ItemSite = itemSite;
			InventoryID = InventoryItem.InventoryID ?? 0;
			Key = CreateKey(bomItem);
		}

		public void AddChildKey(IBomRevision bomRevision)
		{
			if (bomRevision?.BOMID == null)
			{
				return;
		}

			if (ChildKeys == null)
			{
				ChildKeys = new List<string>();
			}

			ChildKeys.Add(CreateKey(bomRevision));
		}

		private static string CreateKey(IBomRevision bomRevision) => AsCombinedKey(bomRevision?.BOMID, bomRevision?.RevisionID);

		public static string AsCombinedKey(params object[] keys) => string.Join(":", keys);

		public static MultiLevelBomResult Find(List<MultiLevelBomResult> results, IBomRevision bomRevision) => results == null || bomRevision == null
			? null
			: results.Where(r => r.Key == MultiLevelBomResult.AsCombinedKey(bomRevision.BOMID, bomRevision.RevisionID)).FirstOrDefault();
	}

	/// <summary>
	/// Options used for running multi level bom process
	/// </summary>
	[Serializable]
	public class MultiLevelBomProcessFilter
	{
		public bool IsSingleLevel { get; set; }
		public virtual bool IgnoreReplenishmentSettings { get; set; }
		public virtual bool DefaultBomsOnly { get; set; }
		public virtual DateTime? EffectiveDate { get; set; }
		public virtual int? ItemClassID { get; set; }
		public virtual int? SiteID { get; set; }
		public virtual int? InventoryID { get; set; }
		public virtual int? SubItemID { get; set; }
		public virtual string BOMID { get; set; }
		public virtual string RevisionID { get; set; }
		public virtual List<string> ExcludedBomStatuses { get; set; }

		public MultiLevelBomProcessFilter()
		{
			ExcludedBomStatuses = new List<string>();
		}
	}
}
