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
using PX.Objects.IN;

namespace PX.Objects.AM
{
	/// <summary>
	/// Multi level process extension for Engineering Workbench (AM208100)
	/// </summary>
	[PXProtectedAccess]
	public abstract class EngineeringWorkbenchMultiLevelExtension : MultiLevelBomProcessBase<EngineeringWorkbenchMaint, AMBomItem>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
		}

		[PXProtectedAccess]
		protected abstract string MaterialExtraColumns(AMBomMatl bomMatl);

		[PXProtectedAccess]
		protected abstract WorkbenchTreeNode CreateNewTreeNode(string id, string description, string parent, string extraColumns, TreeNodeActions actions);

		[PXProtectedAccess]
		protected abstract WorkbenchTreeNode CreateBomTreeNode(AMBomItem bomItem);

		[PXProtectedAccess]
		protected abstract WorkbenchTreeNode CreateOperationTreeNode(AMBomOper bomOper, WorkbenchTreeNode parentTreeNode, bool isEnabled, bool checkForDuplicates);

		[PXProtectedAccess]
		protected abstract WorkbenchTreeNode CreateMaterialTreeNode(AMBomItem bomItem, string parentID, AMBomMatl bomMatl, AMBomOper bomOper, InventoryItem inventoryItem, AMBomItem bomMatlSubBomItem, bool isEnabled);

		[PXProtectedAccess]
		protected abstract List<AMBomOper> GetBomOperations(IBomRevision bomRevision);

		public delegate List<WorkbenchTreeNode> CreateTreeNodesDelegate(AMBomItem bomItem);

		[PXOverride]
		public virtual List<WorkbenchTreeNode> CreateTreeNodes(AMBomItem bomItem, CreateTreeNodesDelegate method)
		{
			var results = method?.Invoke(bomItem);
			if (results != null && results.Count > 0 || string.IsNullOrWhiteSpace(bomItem?.RevisionID))
			{
				return results;
			}

			results = new List<WorkbenchTreeNode>();

			var parent = CreateBomTreeNode(bomItem);
			results.Add(parent);

			var multiLevelFilter = GetDefaultFilter;
			multiLevelFilter.BOMID = bomItem.BOMID;
			multiLevelFilter.RevisionID = bomItem.RevisionID;
			multiLevelFilter.DefaultBomsOnly = true;
			var multiLevelResults = GetMultiLevelBomResults(multiLevelFilter);
			if (multiLevelResults == null)
			{
				return results;
			}

			var detailList = CreateBomMaterialTreeNodes(multiLevelFilter, 0, bomItem, parent, multiLevelResults);
			if (detailList != null && detailList.Count > 0)
			{
				results.AddRange(detailList);
			}

			return results;
		}

		protected virtual List<WorkbenchTreeNode> CreateBomMaterialTreeNodes(MultiLevelBomProcessFilter filter, int level, AMBomItem bomItem, WorkbenchTreeNode parentTreeNode, List<MultiLevelBomResult> multiLevelBoms)
		{
			var treeNodes = new List<WorkbenchTreeNode>();
			if (level >= LowLevel.MaxLowLevel)
			{
				PXTrace.WriteWarning(Messages.GetLocal(Messages.MaxLevelsReached));
				return treeNodes;
			}

			var bomIsEnabled = Base.BOMAllowsEdit(bomItem);
			var operationParentIDs = new Dictionary<int, string>();
			foreach (var matlOperResult in GetBomMaterial(bomItem))
			{
				var bomMatl = matlOperResult.GetItem<AMBomMatl>();
				var bomOper = matlOperResult.GetItem<AMBomOper>();
				var inventoryItem = matlOperResult.GetItem<InventoryItem>();

				if (bomOper?.OperationCD == null || bomMatl?.InventoryID == null || !IsMaterialEffective(filter, bomMatl?.EffDate, bomMatl?.ExpDate))
				{
					continue;
				}

				if (!operationParentIDs.TryGetValue(bomOper.OperationID.GetValueOrDefault(), out var operationParentID))
				{
					var operTreeNode = CreateOperationTreeNode(bomOper, parentTreeNode, bomIsEnabled, checkForDuplicates: level > 0);
					if (operTreeNode != null)
					{
						operationParentID = operTreeNode.IDName;
						Base.HoldTreeNode(operTreeNode);
						treeNodes.Add(operTreeNode);
						operationParentIDs.Add(bomOper.OperationID.GetValueOrDefault(), operationParentID); 
					}
				}

				var bomLevelResult = GetBomLevel(multiLevelBoms, bomMatl.InventoryID, bomMatl.SiteID ?? bomItem?.SiteID);
				var matlHasBom = bomLevelResult != null && !bomLevelResult.IsPurchasedBOM;

				// MATERIAL TO TREE
				var matlTreeNode = CreateMaterialTreeNode(bomItem, operationParentID, bomMatl, bomOper, inventoryItem, matlHasBom ? bomLevelResult.BomItem : null, bomIsEnabled);
				Base.HoldTreeNode(matlTreeNode);
				treeNodes.Add(matlTreeNode);

				if (matlHasBom)
				{
					//stop for recursive
					if (bomLevelResult.IsRecursive && bomLevelResult.Level > level)
					{
						continue;
					}

					var subTreeNodes = CreateBomMaterialTreeNodes(filter, level + 1, bomLevelResult.BomItem, matlTreeNode, multiLevelBoms);
					if (subTreeNodes != null)
					{
						treeNodes.AddRange(subTreeNodes);
					}
				}
			}

			// When operations do not have material we add them here:
			foreach (var bomOper in GetBomOperations(bomItem))
			{
				if (!operationParentIDs.ContainsKey(bomOper.OperationID.GetValueOrDefault()))
				{
					var operTreeNode = CreateOperationTreeNode(bomOper, parentTreeNode, bomIsEnabled, checkForDuplicates: level > 0);
					if (operTreeNode != null)
					{
						Base.HoldTreeNode(operTreeNode);
						treeNodes.Add(operTreeNode);
					}
				}
			}

			return treeNodes;
		}

		protected MultiLevelBomResult GetBomLevel(List<MultiLevelBomResult> multiLevelBoms, int? inventoryID, int? siteID)
		{
			if (multiLevelBoms == null || multiLevelBoms.Count == 0 || inventoryID == null)
			{
				return null;
			}

			MultiLevelBomResult rtnMultiLevel = null;
			foreach (var item in multiLevelBoms.Where(r => r.InventoryID == inventoryID))
			{
				if (rtnMultiLevel == null)
				{
					rtnMultiLevel = item;
				}

				var currentSiteID = item.BomItem?.SiteID ?? 0;
				if ((siteID == currentSiteID && rtnMultiLevel.BomItem?.SiteID != siteID) ||
					(!rtnMultiLevel.IsDefaultBOM && item.IsDefaultBOM))
				{
					rtnMultiLevel = item;
				}
			}

			return rtnMultiLevel;
		}
	}
}
