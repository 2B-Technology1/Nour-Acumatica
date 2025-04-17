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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data.WorkflowAPI;

namespace PX.Objects.AM
{
	/// <summary>
	/// Engineering Workbench (AM208100)
	/// </summary>
	[Serializable]
	public class EngineeringWorkbenchMaint : BOMGraph<EngineeringWorkbenchMaint>
	{
		// Primary Documents view comes from BOMGraph/PXRevisionableGraph

		#region  Views

		public PXFilter<TreeNodeSelected> SelectedTreeNode;

		public SelectFrom<AMBomMatl>
			.Where<AMBomMatl.bOMID.IsEqual<TreeNodeSelected.bOMID.FromCurrent>
				.And<AMBomMatl.revisionID.IsEqual<TreeNodeSelected.revisionID.FromCurrent>
					.And<AMBomMatl.operationID.IsEqual<TreeNodeSelected.operationID.FromCurrent>
						.And<AMBomMatl.lineID.IsEqual<TreeNodeSelected.lineID.FromCurrent>>>>>
			.View SelectedBomMatl;

		public SelectFrom<AMBomOper>
			.Where<AMBomOper.bOMID.IsEqual<TreeNodeSelected.bOMID.FromCurrent>
				.And<AMBomOper.revisionID.IsEqual<TreeNodeSelected.revisionID.FromCurrent>
					.And<AMBomOper.operationID.IsEqual<TreeNodeSelected.operationID.FromCurrent>>>>
			.View SelectedBomOper;

		public SelectFrom<AMBomItem2>
			.Where<AMBomItem2.bOMID.IsEqual<TreeNodeSelected.bOMID.FromCurrent>
				.And<AMBomItem2.revisionID.IsEqual<TreeNodeSelected.revisionID.FromCurrent>>>
			.View SelectedBomItem2;

		public SelectFrom<AMBomItem3>
			.Where<AMBomItem3.bOMID.IsEqual<TreeNodeSelected.subassemblyBOMID.FromCurrent>
				.And<AMBomItem3.revisionID.IsEqual<TreeNodeSelected.subassemblyRevisionID.FromCurrent>>>
			.View SubassemblyBomItem3;

		public SelectFrom<WorkbenchTreeNode>.View BomTree;

		#endregion

		protected const char NEWCDSEPERATOR = '-';

		protected virtual bool TraceTree() =>
#if DEBUG
		true;
#else
		false;
#endif

		protected virtual int OperationStep() => 10;

		public EngineeringWorkbenchMaint()
		{
			PXUIFieldAttribute.SetEnabled<AMBomItem2.bOMID>(SelectedBomItem2.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<AMBomItem2.revisionID>(SelectedBomItem2.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<AMBomItem2.inventoryID>(SelectedBomItem2.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<AMBomItem2.siteID>(SelectedBomItem2.Cache, null, false);

			SelectedTreeNode.AllowUpdate = false;
		}

		public override void Persist()
		{
			BomMatlRecords.RenumberAllBeforePersist = false;

			BomTree.Cache.Clear();

			var cdChangedOpers = BomOperRecords.Cache.Cached.RowCast<AMBomOper>().Where(r => r.NewOperationCD != null && !BomOperRecords.Cache.GetStatus(r).IsDeleted())?.ToList();
			base.Persist();
			DBUpdateOperCD(cdChangedOpers);
		}

		public virtual void HoldTreeNode(WorkbenchTreeNode treeNode)
		{
			this.BomTree.Cache.Hold(treeNode);
		}

		public virtual int GetCachedDuplicateTreeNodeIDCount(string id) => this.BomTree.Cache.Cached.RowCast<WorkbenchTreeNode>().Where(r => r.IDNameOriginal == id)?.Count() ?? 0;

		public virtual string CreateNonDuplicateTreeIDKey(string id)
		{
			var repeatIDCount = GetCachedDuplicateTreeNodeIDCount(id);
			if (repeatIDCount > 0)
			{
				// IDs must be unique for correct display in the tree. If repeat subassembly found we append a counter to its key
				return PXTreeHelper.AppendDuplicateIDSegment(id, repeatIDCount + 1);
			}

			return id;
		}

		protected virtual void DBUpdateOperCD(List<AMBomOper> cdChangedOpers)
		{
			if (cdChangedOpers == null)
			{
				return;
			}

			var updated = false;
			foreach (var bomOper in cdChangedOpers)
			{
				updated |= DBUpdateOperCD(bomOper);
			}

			if (updated)
			{
				BomOperRecords.Cache.Clear();
				BomOperRecords.Cache.ClearQueryCache();
			}
		}

		protected virtual bool DBUpdateOperCD(AMBomOper bomOper)
		{
			if(bomOper?.NewOperationCD == null || bomOper.OperationCD == bomOper.NewOperationCD)
			{
				return false;
			}

			return PXDatabase.Update<AMBomOper>(
					new PXDataFieldRestrict<AMBomOper.bOMID>(PXDbType.NVarChar, 15, bomOper.BOMID),
					new PXDataFieldRestrict<AMBomOper.revisionID>(PXDbType.NVarChar, 10, bomOper.RevisionID),
					new PXDataFieldRestrict<AMBomOper.operationID>(PXDbType.Int, bomOper.OperationID),
					new PXDataFieldAssign<AMBomOper.operationCD>(PXDbType.NVarChar, OperationCDFieldAttribute.OperationFieldLength, bomOper.NewOperationCD)
					);
		}

		protected virtual IEnumerable bomTree()
		{
			if(Documents.Current?.BOMID == null || Documents.Cache.IsCurrentRowInserted())
			{
				BomTree.Cache.Clear();
				return null;
			}

			var cachedresults  = BomTree.Cache.Cached.RowCast<WorkbenchTreeNode>().ToList();
			if (cachedresults.Any())
			{
				return cachedresults;
			}

			return SetBomTreeNodes();
		}

		/// <summary>
		/// Is access rights for graph set to read only permision
		/// </summary>
		internal bool IsReadOnly => PXAccess.Provider.GetRights(Documents.Cache) == PXCacheRights.Select;
		internal bool IsDeleteAccess => PXAccess.Provider.GetRights(Documents.Cache) == PXCacheRights.Delete;

		#region EVENTS

		protected virtual void _(Events.RowSelected<AMBomItem2> e)
		{
			EnableBomItemFields(e.Cache, e.Row);
		}

		protected override void _(Events.RowSelected<AMBomItem> e)
		{
			base._(e);
			var selectedBOMID = SelectedTreeNode.Current?.BOMID;
			SelectedBomItem2.AllowSelect = selectedBOMID != null && e.Row?.BOMID != null && selectedBOMID != e.Row?.BOMID;
		}

		#endregion

		protected virtual List<WorkbenchTreeNode> SetBomTreeNodes()
		{
			if(string.IsNullOrWhiteSpace(Documents.Current?.RevisionID) || Documents.Current.BOMID == null)
			{
				return null;
			}

			var treeNodes = CreateTreeNodes(Documents.Current);

			BomTree.Cache.IsDirty = false;
			return treeNodes;
		}

		// Override in multi level graph extension
		protected virtual List<WorkbenchTreeNode> CreateTreeNodes(AMBomItem bomItem)
		{
			return new List<WorkbenchTreeNode>();
		}

		protected virtual WorkbenchTreeNode CreateNewTreeNode(string id, string description, string parent, string extraColumns, TreeNodeActions actions)
		{
			return new WorkbenchTreeNode
			{
				Description = description,
				IDParent = parent ?? "null",
				IDName = id,
				IDNameOriginal = id,
				Actions = PXTreeHelper.MakeActionsString(actions),
				ExtraColumns = extraColumns ?? EmptyExtraColumns()
			};
		}

		protected virtual WorkbenchTreeNode CreateBomTreeNode(AMBomItem bomItem)
		{
			if (bomItem?.BOMID == null)
			{
				return null;
			}

			var bomItemInventoryItem = InventoryItem.PK.Find(this, bomItem.InventoryID);

			var bomItemTreeNode = CreateNewTreeNode(
				PXTreeHelper.GetBomItemKey(bomItem),
				GetBomTreeNodeDescription(bomItem, bomItemInventoryItem),
				null,
				null,
				new TreeNodeActions
				{
					Delete = false,
					DisableDrag = true,
					DisableDropChild = true,
					CreateChild = BOMAllowsEdit(bomItem) && !IsReadOnly
				}
				);
			bomItemTreeNode.Icon = WorkbenchTreeIcons.BOM;
			return bomItemTreeNode;
		}

		protected virtual string GetBomTreeNodeDescription(AMBomItem bomItem, InventoryItem inventoryItem)
		{
			return $"{bomItem?.BOMID} - {bomItem?.RevisionID} ({inventoryItem?.InventoryCD.TrimIfNotNullEmpty()})";
		}

		protected virtual WorkbenchTreeNode CreateOperationTreeNode(AMBomOper bomOper, WorkbenchTreeNode parentTreeNode, bool isEnabled, bool checkForDuplicates)
		{
			var operTree = CreateOperationTreeNode(bomOper, PXTreeHelper.GetBomOperKey(bomOper), parentTreeNode, isEnabled);
			if (checkForDuplicates && operTree != null)
			{
				operTree.IDName = CreateNonDuplicateTreeIDKey(operTree.IDNameOriginal);
			}
			return operTree;
		}

		protected virtual WorkbenchTreeNode CreateOperationTreeNode(AMBomOper bomOper, string id, WorkbenchTreeNode parentTreeNode, bool isEnabled)
		{
			return CreateOperationTreeNode(bomOper, id, GetOperTreeNodeDescription(bomOper), parentTreeNode?.IDName, isEnabled);
		}

		protected virtual WorkbenchTreeNode CreateOperationTreeNode(AMBomOper bomOper, string id, string description, string parentID, bool isEnabled)
		{
			if (bomOper?.BOMID == null)
			{
				return null;
			}

			var isRowEnabled = isEnabled && !IsReadOnly;
			var treeNode = CreateNewTreeNode(id, description, parentID, null,
				new TreeNodeActions
				{
					Delete = IsDeleteAccess && isEnabled,
					DisableDrag = !isRowEnabled,
					DisableDropChild = !isRowEnabled,
					CreateChild = isRowEnabled,
					CreateSibling = isRowEnabled
				});

			treeNode.Icon = bomOper.OutsideProcess == true ?
				WorkbenchTreeIcons.Operation.OutsideProcess :
				WorkbenchTreeIcons.Operation.Standard;

			return treeNode;
		}

		protected virtual string GetOperTreeNodeDescription(AMBomOper bomOper)
		{
			var operDesc = bomOper?.Descr.TrimIfNotNullEmpty();
			return string.IsNullOrWhiteSpace(operDesc)
				? $"{bomOper?.OperationCD} - {bomOper?.WcID}"
				: $"{bomOper?.OperationCD} - {bomOper?.WcID}, {operDesc}";
		}

		protected virtual string GetBomMatlKey(AMBomOper bomOper, AMBomMatl bomMatl, AMBomItem bomMatlSubBomItem)
		{
			return bomMatlSubBomItem?.BOMID != null
				? PXTreeHelper.GetBomMatlKeyWithSubBomID(bomOper, bomMatl, bomMatlSubBomItem)
				: PXTreeHelper.GetBomMatlKey(bomOper, bomMatl);
		}

		protected virtual WorkbenchTreeNode CreateMaterialTreeNode(AMBomItem bomItem, string parentID, AMBomMatl bomMatl, AMBomOper bomOper, InventoryItem inventoryItem, AMBomItem bomMatlSubBomItem, bool isEnabled)
		{
			var matlTreeNode = CreateMaterialTreeNode(
				bomItem,
				GetBomMatlKey(bomOper, bomMatl, bomMatlSubBomItem),
				GetMaterialTreeNodeDescription(bomMatl, inventoryItem),
				parentID,
				bomMatl,
				bomOper,
				bomMatlSubBomItem,
				isEnabled);
			if (matlTreeNode?.IDNameOriginal != null)
			{
				matlTreeNode.IDName = CreateNonDuplicateTreeIDKey(matlTreeNode.IDNameOriginal);
			}
			return matlTreeNode;
		}

		protected virtual WorkbenchTreeNode CreateMaterialTreeNode(AMBomItem bomItem, string id, string description, string parentID, AMBomMatl bomMatl, AMBomOper bomOper, AMBomItem bomMatlSubBomItem, bool isEnabled)
		{
			var extraColumns = MaterialExtraColumns(bomMatl);
			var isRowEnabled = isEnabled && !IsReadOnly;
			var hasMatlBom = bomMatlSubBomItem?.BOMID != null;
			var treeNode = CreateNewTreeNode(id, description, parentID, extraColumns,
				new TreeNodeActions
				{
					Delete = IsDeleteAccess && isEnabled,
					DisableDrag = !isRowEnabled,
					DisableDropChild = !isRowEnabled,
					CreateChild = !IsReadOnly && bomMatlSubBomItem?.BOMID != null && BOMAllowsEdit(bomMatlSubBomItem),
					CreateSibling = isRowEnabled
				});

			treeNode.Icon = GetMaterialIcon(bomMatl, hasMatlBom);

			return treeNode;
		}

		protected virtual string GetMaterialTreeNodeDescription(AMBomMatl bomMatl, InventoryItem inventoryItem)
		{
			return $"{inventoryItem?.InventoryCD.TrimIfNotNullEmpty()}, {bomMatl?.Descr.TrimIfNotNullEmpty()}";
		}

		protected virtual string GetMaterialIcon(AMBomMatl bomMatl, bool matlHasBom)
		{
			if(bomMatl?.IsStockItem != true)
			{
				switch (bomMatl?.MaterialType)
				{
					case AMMaterialType.Phantom:
						return WorkbenchTreeIcons.Material.NonStockPhantom;
					case AMMaterialType.Subcontract:
						return WorkbenchTreeIcons.Material.NonStockSubcontract;
					default: // Regular
						return WorkbenchTreeIcons.Material.NonStockRegular;
				}
			}

			switch (bomMatl?.MaterialType)
			{
				case AMMaterialType.Phantom:
					return matlHasBom ? WorkbenchTreeIcons.Material.StockPhantomBOM : WorkbenchTreeIcons.Material.StockPhantomNoBOM;
				case AMMaterialType.Subcontract:
					return matlHasBom ? WorkbenchTreeIcons.Material.StockSubcontractBOM : WorkbenchTreeIcons.Material.StockSubcontractNoBOM;
				default: // Regular
					return matlHasBom ? WorkbenchTreeIcons.Material.StockRegularBOM : WorkbenchTreeIcons.Material.StockRegularNoBOM;
			}
		}

		protected virtual string EmptyExtraColumns()
		{
			return "[\"\", \"\"]";
		}

		protected virtual string MaterialExtraColumns(AMBomMatl bomMatl)
		{
			return $"[\"{UomHelper.FormatQtyFixed(bomMatl.QtyReq)}\", \"{bomMatl.UOM}\"]";
		}

		#region TREE EVENTS

		public PXAction<AMBomItem> selectNode;
		[PXUIField(DisplayName = "Select node", Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable SelectNode(PXAdapter adapter)
		{
			var results = PXTreeHelper.CommandAsTreeNodeEventResults(adapter.CommandArguments);
			if (TraceTree())
			{
				var traceMsg = PXTreeHelper.FormatCommandArguments(results, "SelectNode");
				PXTrace.WriteInformation(traceMsg);
#if DEBUG
				AMDebug.TraceWriteLine(traceMsg);
#endif
			}

			if (results != null && results.Any())
			{
				TreeNodeSelected(results[0]);
			}

			return adapter.Get();
		}

		public PXAction<AMBomItem> addNode;
		[PXUIField(DisplayName = "Add node", Visible = false, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable AddNode(PXAdapter adapter)
		{
			var results = PXTreeHelper.CommandAsTreeNodeEventResults(adapter.CommandArguments);
			if (TraceTree())
			{
				var traceMsg = PXTreeHelper.FormatCommandArguments(results, "AddNode");
				PXTrace.WriteInformation(traceMsg);
#if DEBUG
				AMDebug.TraceWriteLine(traceMsg);
#endif
			}

			TreeNodeAdd(results);

			return adapter.Get();
		}

		public PXAction<AMBomItem> deleteNode;
		[PXUIField(DisplayName = "Delete node", Visible = false, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable DeleteNode(PXAdapter adapter)
		{
			var results = PXTreeHelper.CommandAsTreeNodeEventResults(adapter.CommandArguments);
			if (TraceTree())
			{
				var traceMsg = PXTreeHelper.FormatCommandArguments(results, "DeleteNode");
				PXTrace.WriteInformation(traceMsg);
#if DEBUG
				AMDebug.TraceWriteLine(traceMsg);
#endif
			}

			TreeNodeDelete(results);

			return adapter.Get();
		}

		public PXAction<AMBomItem> updateNode;
		[PXUIField(DisplayName = "Update node", Visible = false, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable UpdateNode(PXAdapter adapter)
		{
			var results = PXTreeHelper.CommandAsTreeNodeEventResults(adapter.CommandArguments);
			if (TraceTree())
			{
				var traceMsg = PXTreeHelper.FormatCommandArguments(results, "UpdateNode");
				PXTrace.WriteInformation(traceMsg);
#if DEBUG
				AMDebug.TraceWriteLine(traceMsg);
#endif
			}

			TreeNodeUpdated(results);

			return adapter.Get();
		}

		/// <summary>
		/// Linked to PXTree CheckDropCommand
		/// </summary>
		public PXAction<AMBomItem> checkDropAction;
		[PXUIField(DisplayName = "Check Drop Action", Visible = false, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ApiCallback)]
		public virtual IEnumerable CheckDropAction(PXAdapter adapter, string id, string parentId, string previousNodeID)
		{
			if (TraceTree())
			{
				var traceMsg = $"CheckDropAction: id = {id}; parentID = {parentId}; previousNodeID = {previousNodeID}";
				PXTrace.WriteInformation(traceMsg);
#if DEBUG
				AMDebug.TraceWriteLine(traceMsg);
#endif
			}

			var isParentMatl = PXTreeHelper.IsMaterialTreeNode(parentId);
			var isParentSubMatl = isParentMatl && PXTreeHelper.IsSubassemblyMaterialTreeNode(parentId);
			var isParentOper = !isParentMatl && PXTreeHelper.IsOperationTreeNode(parentId);
			if (PXTreeHelper.IsOperationTreeNode(id))
			{
				if(isParentOper)
				{
					throw new PXVerificationFailedException(Messages.InvalidTreeDropOperToOper);
				}

				if (isParentMatl && !isParentSubMatl)
				{
					throw new PXVerificationFailedException(Messages.InvalidTreeDropOperToNonSubMatl);
				}
			}
			else
			{
				var isIdMatl = PXTreeHelper.IsMaterialTreeNode(id);
				if (isIdMatl && isParentMatl)
				{
					throw new PXVerificationFailedException(Messages.InvalidTreeDropMatlToMatl);
				}
			}

			var bomItem = isParentSubMatl ? GetSubassemblyBOMFromTreeNode(parentId) : GetBomItemFromTreeNodeID(parentId);
			if (bomItem?.BOMID != null)
			{
				if (!ECRAllowEdit(bomItem.BOMID))
				{
					throw new PXVerificationFailedException(Messages.InvalidTreeDropECCRequired, bomItem.BOMID, bomItem.RevisionID);
				}

				if (!BOMOnHold(bomItem))
				{
					throw new PXVerificationFailedException(Messages.InvalidTreeDropBomOnHold, bomItem.BOMID, bomItem.RevisionID);
				}
			}
			yield return bomItem;
		}

		/// <summary>
		/// Runs after tree node action from tree "Create Sibling". Provides a way for defaults before node added to the tree.
		/// Return passes back to tree control.
		/// </summary>
		public PXAction<AMBomItem> newSiblingNodeDefault;
		[PXUIField(DisplayName = "New Sibling Node Default", Visible = false, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ApiCallback)]
		public virtual IEnumerable NewSiblingNodeDefault(PXAdapter adapter, string id, string parentId, string previousNodeID)
		{
			if (TraceTree())
			{
				var traceMsg = $"NewSiblingNodeDefault: id = {id}; parentID = {parentId}; previousNodeID = {previousNodeID}";
				PXTrace.WriteInformation(traceMsg);
#if DEBUG
				AMDebug.TraceWriteLine(traceMsg);
#endif
			}

			if(string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentNullException(nameof(id));
			}

			if(PXTreeHelper.IsMaterialTreeNode(id))
			{
				var bomMatl = GetBomMatlFromTreeNodeID(id);
				yield return CreateNewMaterialTreeNode(AMBomOper.PK.Find(this, bomMatl?.BOMID, bomMatl?.RevisionID, bomMatl?.OperationID), id, parentId, previousNodeID);
			}

			var bomOper = GetBomOperFromTreeNodeID(id);
			var bomItem = (AMBomItem)null;
			if(!string.IsNullOrWhiteSpace(parentId))
			{
				bomItem = AMBomItem.PK.Find(this, bomOper?.BOMID, bomOper?.RevisionID);
			}

			if(bomItem?.BOMID == null)
			{
				bomItem = GetBomItemFromTreeNodeID(id);
			}

			yield return CreateNewOperationTreeNode(bomItem, id, parentId, bomOper);
		}

		/// <summary>
		/// Runs after tree node action from tree "Create Child". Provides a way for defaults before node added to the tree.
		/// Return passes back to tree control.
		/// </summary>
		public PXAction<AMBomItem> newChildNodeDefault;
		[PXUIField(DisplayName = "New Child Node Default", Visible = false, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ApiCallback)]
		public virtual IEnumerable NewChildNodeDefault(PXAdapter adapter, string id, string parentId, string previousNodeID)
		{
			if (TraceTree())
			{
				var traceMsg = $"NewChildNodeDefault: id = {id}; parentID = {parentId}; previousNodeID = {previousNodeID}";
				PXTrace.WriteInformation(traceMsg);
#if DEBUG
				AMDebug.TraceWriteLine(traceMsg);
#endif
			}

			if (string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentNullException(nameof(id));
			}

			if (PXTreeHelper.IsOperationTreeNode(id))
			{
				yield return CreateNewMaterialTreeNode(GetBomOperFromTreeNodeID(id), id, parentId, previousNodeID);
			}

			var bomItem = (AMBomItem)null;
			if (!string.IsNullOrWhiteSpace(parentId))
			{
				var bomMatl = GetBomMatlFromTreeNodeID(id);
				bomItem = AMBomItem.PK.Find(this, bomMatl?.BOMID, bomMatl?.RevisionID);
			}

			if (bomItem?.BOMID == null)
			{
				bomItem = GetBomItemFromTreeNodeID(id);
			}

			yield return CreateNewOperationTreeNode(bomItem, id, parentId, GetLastBomOperation(bomItem));
		}

		#endregion

		protected virtual AddTreeNodeParams CreateNewMaterialTreeNode(AMBomOper bomOper, string id, string parentID, string previousNodeID)
		{
			if (bomOper?.BOMID == null)
			{
				throw new ArgumentNullException(nameof(bomOper));
			}

			var bomItem = AMBomItem.PK.Find(this, bomOper.BOMID, bomOper.RevisionID);
			var description = Messages.GetLocal(Messages.NewTreeNodeMaterial);
			var newTempMatl = new AMBomMatl
			{
				BOMID = bomOper.BOMID,
				RevisionID = bomOper.RevisionID,
				OperationID = bomOper.OperationID,
				LineID = bomOper.LineCntrMatl + 1,
				Descr = description
			};

			return (AddTreeNodeParams)CreateMaterialTreeNode(
				bomItem,
				PXTreeHelper.GetBomMatlKey(bomOper, newTempMatl),
				description,
				parentID,
				newTempMatl,
				bomOper,
				null,
				BOMAllowsEdit(bomItem));
		}

		protected virtual AddTreeNodeParams CreateNewOperationTreeNode(AMBomItem bomItem, string id, string parentID, AMBomOper siblingBomOper)
		{
			if(bomItem?.BOMID == null)
			{
				throw new ArgumentNullException(nameof(bomItem));
			}

			var newOperCD = GetNewOperationCD(bomItem, siblingBomOper);
			var description = Messages.GetLocal(Messages.NewTreeNodeOperation);
			var newTempOper = new AMBomOper
			{
				BOMID = bomItem.BOMID,
				RevisionID = bomItem.RevisionID,
				OperationID = bomItem.LineCntrOperation + 1,
				OperationCD = newOperCD,
				Descr = description
			};

			return (AddTreeNodeParams)CreateOperationTreeNode(
				newTempOper,
				PXTreeHelper.GetBomOperKey(newTempOper),
				description,
				parentID,
				BOMAllowsEdit(bomItem));
		}

		protected virtual string GetNewOperationCD(AMBomItem bomItem, AMBomOper siblingBomOper)
		{
			var hasSibling = siblingBomOper?.BOMID != null;
			var size = GetOperationCDSize(hasSibling ? new List<AMBomOper> { siblingBomOper } : GetBomOperations(bomItem));
			if (size == 0)
			{
				var lastBomCreated = (AMBomOper)SelectFrom<AMBomOper>
					.OrderBy<AMBomOper.createdDateTime.Desc>.View.SelectWindowed(this, 0, 1);
				if (lastBomCreated?.BOMID != null)
				{
					size = GetOperationCDSize(new List<AMBomOper> { lastBomCreated });
				}
			}
			if (size == 0)
			{
				size = 4;
			}

			if (!hasSibling)
			{
				return FormatOperationCD(OperationStep(), size);
			}

			return FormatOperationCD(Convert.ToInt32(siblingBomOper.OperationCD) + OperationStep(), size);
		}

		protected virtual void SetSelectedNode(TreeNodeEventResult treeNodeResult, AMBomOper operation)
		{
			SetSelectedNode(new TreeNodeSelected
			{
				ID = treeNodeResult?.ID,
				ParentID = treeNodeResult?.ParentID,
				Text = treeNodeResult?.Text,
				PrevNodeID = treeNodeResult?.PrevNodeID,
				BOMID = operation?.BOMID,
				RevisionID = operation?.RevisionID,
				OperationID = operation?.OperationID,
				OperationCD = operation?.OperationCD,
				LineID = null,
				IsOperation = true,
				WcID = operation?.WcID,
				OperationDescription = operation?.Descr,
				IsSubassembly = false
			});
		}

		protected virtual void SetSelectedNode(TreeNodeEventResult treeNodeResult, AMBomMatl material, AMBomItem subassemblyBomItem)
		{
			SetSelectedNode(new TreeNodeSelected
			{
				ID = treeNodeResult?.ID,
				ParentID = treeNodeResult?.ParentID,
				Text = treeNodeResult?.Text,
				PrevNodeID = treeNodeResult?.PrevNodeID,
				BOMID = material?.BOMID,
				RevisionID = material?.RevisionID,
				OperationID = material?.OperationID,
				OperationCD = null,
				LineID = material?.LineID,
				IsOperation = false,
				WcID = null,
				OperationDescription = null,
				IsSubassembly = subassemblyBomItem?.BOMID != null,
				SubassemblyBOMID = subassemblyBomItem?.BOMID,
				SubassemblyRevisionID = subassemblyBomItem?.RevisionID
			});
		}

		protected virtual void SetSelectedNode(TreeNodeSelected treeNodeSelected)
		{
			SelectedTreeNode.Current.BOMID = treeNodeSelected?.BOMID;
			SelectedTreeNode.Current.RevisionID = treeNodeSelected?.RevisionID;
			SelectedTreeNode.Current.OperationID = treeNodeSelected?.OperationID;
			SelectedTreeNode.Current.OperationCD = treeNodeSelected?.OperationCD;
			SelectedTreeNode.Current.LineID = treeNodeSelected?.LineID;
			SelectedTreeNode.Current.IsOperation = treeNodeSelected?.IsOperation ?? false;
			SelectedTreeNode.Current.IsSubassembly = treeNodeSelected?.IsSubassembly ?? false;
			SelectedTreeNode.Current.WcID = treeNodeSelected?.WcID;
			SelectedTreeNode.Current.OperationDescription = treeNodeSelected?.OperationDescription;
			SelectedTreeNode.Current.SubassemblyBOMID = treeNodeSelected?.SubassemblyBOMID;
			SelectedTreeNode.Current.SubassemblyRevisionID = treeNodeSelected?.SubassemblyRevisionID;

			SelectedTreeNode.Current.ID = treeNodeSelected?.ID;
			SelectedTreeNode.Current.ParentID = treeNodeSelected?.ParentID;
			SelectedTreeNode.Current.Text = treeNodeSelected?.Text;
			SelectedTreeNode.Current.PrevNodeID = treeNodeSelected?.PrevNodeID;

			BomOperRecords.Current = FindBomOper(treeNodeSelected?.BOMID, treeNodeSelected?.RevisionID, treeNodeSelected?.OperationID);
		}

		protected virtual void TreeNodeSelected(TreeNodeEventResult treeNodeResult)
		{
			var id = treeNodeResult?.ID;
			var isMatlTreeNode = PXTreeHelper.IsMaterialTreeNode(id);
			AMBomMatl material = null;
			AMBomItem subassemblyBomItem = null;
			if (isMatlTreeNode)
			{
				material = GetBomMatlFromTreeNodeID(id);
				if (material != null)
				{
					subassemblyBomItem = GetSubassemblyBOMFromTreeNode(treeNodeResult);
				}
			}

			AMBomOper operation = null;
			if (isMatlTreeNode || PXTreeHelper.IsOperationTreeNode(id))
			{
				operation = GetBomOperFromTreeNodeID(id);
			}

			TreeNodeSelected(treeNodeResult, operation, material, subassemblyBomItem);
		}

		protected virtual void TreeNodeSelected(TreeNodeEventResult treeNodeResult, AMBomOper operation, AMBomMatl material, AMBomItem subassemblyBomItem)
		{
			var id = treeNodeResult?.ID;
			var isMatlTreeNode = material?.BOMID != null;
			if (isMatlTreeNode)
			{
				SetSelectedNode(treeNodeResult, material, subassemblyBomItem);
			}

			if (isMatlTreeNode || PXTreeHelper.IsOperationTreeNode(id))
			{
				if (!isMatlTreeNode)
				{
					SetSelectedNode(treeNodeResult, operation);
					return;
				}

				SelectedTreeNode.Current.OperationCD = operation?.OperationCD;
				SelectedTreeNode.Current.WcID = operation?.WcID;
				SelectedTreeNode.Current.OperationDescription = operation?.Descr;
				return;
			}

			SelectedTreeNode.Current.BOMID = null;
			SelectedTreeNode.Current.RevisionID = null;
			SelectedTreeNode.Current.OperationID = null;
			SelectedTreeNode.Current.LineID = null;
		}

		protected virtual AMBomItem GetSubassemblyBOMFromTreeNode(TreeNodeEventResult materialTreeNode)
		{
			return GetSubassemblyBOMFromTreeNode(materialTreeNode?.ID);
		}

		protected virtual AMBomItem GetSubassemblyBOMFromTreeNode(string id)
		{
			return PXTreeHelper.GetBomMatlSubBomItemFromTreeNodeID(this, id);
		}

		protected virtual AMBomItem GetBomItemFromTreeNodeID(string id)
		{
			return PXTreeHelper.GetBomItemFromTreeNodeID(this, id);
		}

		protected virtual AMBomOper GetBomOperFromTreeNodeID(string id)
		{
			if(string.IsNullOrWhiteSpace(id))
			{
				return null;
			}

			// When Oper moved in tree or new, the ID no longer lines up to the new oper record
			var cached = BomOperRecords.Cache.Cached.RowCast<AMBomOper>()
				.Where(r => r.OriginalTreeNodeID == id && !BomOperRecords.Cache.GetStatus(r).IsDeleted()).FirstOrDefault();
			if (cached != null)
			{
				return cached;
			}

			return PXTreeHelper.GetBomOperFromTreeNodeID(this, id);
		}

		protected virtual AMBomMatl GetBomMatlFromTreeNodeID(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return null;
			}

			// When Matl moved in tree or new, the ID no longer lines up to the new matl record
			var cached = BomMatlRecords.Cache.Cached.RowCast<AMBomMatl>()
				.Where(r => r.OriginalTreeNodeID == id && !BomMatlRecords.Cache.GetStatus(r).IsDeleted()).FirstOrDefault();
			if (cached != null)
			{
				return cached;
			}

			return PXTreeHelper.GetBomMatlFromTreeNodeID(this, id);
		}

		protected virtual void TreeNodeUpdated(List<TreeNodeEventResult> treeNodeResults)
		{
			// Base tree control will pass in all child nodes but we only want the parent
			if(treeNodeResults == null || treeNodeResults.Count == 0)
			{
				return;
			}

			var first = treeNodeResults[0];
			if(first?.ParentID == null)
			{
				return;
			}

			TreeNodeMoved(first.ID, first.ParentID, first.OldParentID, first.PrevNodeID);
		}

		protected virtual void TreeNodeMoved(string id, string parent, string oldParent, string siblingBefore)
		{
			if(PXTreeHelper.IsMaterialTreeNode(id))
			{
				TreeNodeMaterialMoved(id, parent, siblingBefore);
				return;
			}

			TreeNodeOperationMoved(id, parent, oldParent ?? parent, siblingBefore);
		}

		protected virtual void TreeNodeOperationMoved(string id, string parent, string oldParent, string siblingBefore)
		{
			var bomOper = GetBomOperFromTreeNodeID(id);
			if(bomOper?.BOMID == null)
			{
				return;
			}

			if(bomOper?.OriginalTreeNodeID == null)
			{
				bomOper.OriginalTreeNodeID = id;
			}

			var bomItem = GetSubassemblyBOMFromTreeNode(parent) ?? GetBomItemFromTreeNodeID(parent);
			TreeNodeOperationMoved(bomOper,
				bomItem,
				GetBomOperFromTreeNodeID(siblingBefore),
				parent != oldParent);
		}

		protected virtual void TreeNodeOperationMoved(AMBomOper bomOper, AMBomItem bomItem, AMBomOper bomOperSibling, bool hasNewParent)
		{
			if(bomOper == null || bomItem == null || BomOperRecords.Cache.GetStatus(bomOper).IsDeleted())
			{
				return;
			}

			var movedBomOper = bomOper;
			var operations = GetBomOperations(bomItem);
			if (hasNewParent)
			{
				movedBomOper = CreateNewBomOper(bomOper, bomItem, bomOperSibling);
				if (operations != null)
				{
					operations.Add(movedBomOper); 
				}
			}

			var insertedOperations = UpdateOperationCD(
				bomItem,
				UpdateOperationCD(operations, movedBomOper, bomOperSibling, OperationStep()),
				operations);

			if (hasNewParent && insertedOperations != null)
			{
				foreach (var newBomOper in insertedOperations)
				{
					MoveOperationToNewBOM(bomOper, newBomOper);
				}
			}
		}

		protected virtual string EmptyOperationCD(int size)
		{
			var adjustedSize = (size <= 0 || size > OperationCDFieldAttribute.OperationFieldLength) ? OperationCDFieldAttribute.OperationMaskLength : size;
			return "".PadRight(adjustedSize, '0');
		}

		protected virtual AMBomOper InsertAMBomOper(AMBomOper bomOper, AMBomItem bomItem)
		{
			if(bomOper?.OperationCD == null)
			{
				return null;
			}

			if(bomItem?.BOMID == null || bomItem.BOMID != bomOper.BOMID)
			{
				bomItem = AMBomItem.PK.Find(this, bomOper.BOMID, bomOper.RevisionID);
			}

			if(bomItem == null)
			{
				return null;
			}

			bomItem.LineCntrOperation += 1;
			bomOper.OperationID = bomItem.LineCntrOperation;
			Documents.Update(bomItem);
			return BomOperRecords.Insert(bomOper);
		}

		protected virtual AMBomMatl InsertAMBomMatl(AMBomMatl bomMatl, AMBomOper bomOper)
		{
			if (bomMatl?.BOMID == null)
			{
				return null;
			}

			if (bomOper?.BOMID == null || bomOper.BOMID != bomMatl.BOMID)
			{
				bomOper = FindBomOper(bomMatl);
			}

			if (bomOper == null)
			{
				return null;
			}

			bomOper.LineCntrMatl += 1;
			bomMatl.LineID = bomOper.LineCntrMatl;
			BomOperRecords.Update(bomOper);
			return BomMatlRecords.Insert(bomMatl);
		}

		// When inserting new operations we run into an issue where the CD value might already exist as a key and we need a temp one until updated during persist
		protected virtual string GetNewOperationCD(AMBomItem bomItem, string operationCD)
		{
			var foundCount = OperationCDExists(GetBomOperations(bomItem), operationCD, false);
			if(foundCount > 0)
			{
				return $"{operationCD}{NEWCDSEPERATOR}{foundCount}";
			}

			return operationCD;
		}

		protected virtual int OperationCDExists(List<AMBomOper> bomOpers, string operationCD, bool excludeNewOpers)
		{
			var foundCount = 0;
			if (bomOpers == null)
			{
				return foundCount;
			}

			foreach (var bomOper in bomOpers)
			{
				if(excludeNewOpers && bomOper.OperationID == null)
				{
					continue;
				}

				var operCD = bomOper.NewOperationCD ?? GetSeperatedOperationCD(bomOper?.OperationCD.TrimIfNotNullEmpty());
				if (operCD == operationCD.TrimIfNotNullEmpty())
				{
					foundCount++;
				}
			}

			return foundCount;
		}

		protected virtual AMBomOper CreateNewBomOper(AMBomOper bomOper, AMBomItem bomItem, AMBomOper bomOperSibling)
		{
			if (bomOper?.BOMID == null)
			{
				throw new ArgumentNullException(nameof(bomOper));
			}

			if (bomItem?.BOMID == null)
			{
				throw new ArgumentNullException(nameof(bomItem));
			}

			var operationCD = bomOperSibling?.NewOperationCD ?? bomOperSibling?.OperationCD ?? string.Empty;

			var newBomOper = PXCache<AMBomOper>.CreateCopy(bomOper);
			newBomOper.BOMID = bomItem.BOMID;
			newBomOper.RevisionID = bomItem.RevisionID;
			newBomOper.OperationID = null;
			newBomOper.OperationCD = GetNewOperationCD(bomItem, operationCD);
			newBomOper.NewOperationCD = null;
			return newBomOper;
		}

		protected virtual void MoveOperationToNewBOM(AMBomOper oldBomOper, AMBomOper newBomOper)
		{
			if (newBomOper?.BOMID == null)
			{
				throw new ArgumentNullException(nameof(newBomOper));
			}

			if (oldBomOper?.BOMID == null)
			{
				throw new ArgumentNullException(nameof(oldBomOper));
			}

			CopyBomOperChildren(oldBomOper, newBomOper);

			BomOperRecords.Delete(oldBomOper);
		}

		protected virtual void CopyBomOperChildren(AMBomOper fromBomOper, AMBomOper toBomOper)
		{
			if (fromBomOper?.BOMID == null)
			{
				throw new ArgumentNullException(nameof(fromBomOper));
			}

			if (toBomOper?.BOMID == null)
			{
				throw new ArgumentNullException(nameof(toBomOper));
			}

			var allBomRef = SelectFrom<AMBomRef>
				.Where<AMBomRef.bOMID.IsEqual<@P.AsString>
				.And<AMBomRef.revisionID.IsEqual<@P.AsString>
				.And<AMBomRef.operationID.IsEqual<@P.AsInt>>>>
				.View.Select(this, fromBomOper.BOMID, fromBomOper.RevisionID, fromBomOper.OperationID)
				.ToFirstTableList();

			foreach (AMBomMatl matl in SelectFrom<AMBomMatl>
				.Where<AMBomMatl.bOMID.IsEqual<@P.AsString>
				.And<AMBomMatl.revisionID.IsEqual<@P.AsString>
				.And<AMBomMatl.operationID.IsEqual<@P.AsInt>>>>
				.View.Select(this, fromBomOper.BOMID, fromBomOper.RevisionID, fromBomOper.OperationID))
			{
				var newMatl = PXCache<AMBomMatl>.CreateCopy(matl);
				newMatl.BOMID = toBomOper.BOMID;
				newMatl.RevisionID = toBomOper.RevisionID;
				newMatl.OperationID = toBomOper.OperationID;
				newMatl.LineID = null;
				newMatl = BomMatlRecords.Insert(newMatl);

				foreach (var bomRef in allBomRef.Where(r => r.MatlLineID == matl.LineID))
				{
					var newBomRef = PXCache<AMBomRef>.CreateCopy(bomRef);
					newBomRef.BOMID = newMatl.BOMID;
					newBomRef.RevisionID = newMatl.RevisionID;
					newBomRef.OperationID = newMatl.OperationID;
					newBomRef.MatlLineID = newMatl.LineID;
					newBomRef.LineID = null;
					BomRefRecords.Insert(newBomRef);
				}

			}

			foreach (var step in SelectFrom<AMBomStep>
				.Where<AMBomStep.bOMID.IsEqual<@P.AsString>
				.And<AMBomStep.revisionID.IsEqual<@P.AsString>
				.And<AMBomStep.operationID.IsEqual<@P.AsInt>>>>
				.View.Select(this, fromBomOper.BOMID, fromBomOper.RevisionID, fromBomOper.OperationID))
			{
				var newStep = PXCache<AMBomStep>.CreateCopy(step);
				newStep.BOMID = toBomOper.BOMID;
				newStep.RevisionID = toBomOper.RevisionID;
				newStep.OperationID = toBomOper.OperationID;
				newStep.LineID = null;
				BomStepRecords.Insert(newStep);
			}

			foreach (var tool in SelectFrom<AMBomTool>
				.Where<AMBomTool.bOMID.IsEqual<@P.AsString>
				.And<AMBomTool.revisionID.IsEqual<@P.AsString>
				.And<AMBomTool.operationID.IsEqual<@P.AsInt>>>>
				.View.Select(this, fromBomOper.BOMID, fromBomOper.RevisionID, fromBomOper.OperationID))
			{
				var newTool = PXCache<AMBomTool>.CreateCopy(tool);
				newTool.BOMID = toBomOper.BOMID;
				newTool.RevisionID = toBomOper.RevisionID;
				newTool.OperationID = toBomOper.OperationID;
				newTool.LineID = null;
				BomToolRecords.Insert(newTool);
			}

			foreach (var overhead in SelectFrom<AMBomOvhd>
				.Where<AMBomOvhd.bOMID.IsEqual<@P.AsString>
				.And<AMBomOvhd.revisionID.IsEqual<@P.AsString>
				.And<AMBomOvhd.operationID.IsEqual<@P.AsInt>>>>
				.View.Select(this, fromBomOper.BOMID, fromBomOper.RevisionID, fromBomOper.OperationID))
			{
				var newOverhead = PXCache<AMBomOvhd>.CreateCopy(overhead);
				newOverhead.BOMID = toBomOper.BOMID;
				newOverhead.RevisionID = toBomOper.RevisionID;
				newOverhead.OperationID = toBomOper.OperationID;
				newOverhead.LineID = null;
				BomOvhdRecords.Insert(newOverhead);
			}
		}

		protected virtual string GetSeperatedOperationCD(string operationCD) => (operationCD ?? string.Empty).Split(NEWCDSEPERATOR)[0];

		protected virtual int GetOperationCDSize(List<AMBomOper> allOperations)
		{
			var size = 0;
			if(allOperations == null)
			{
				return size;
			}

			foreach (var oper in allOperations)
			{
				var operCD = GetSeperatedOperationCD(oper?.OperationCD);
				var currentSize = operCD.TrimIfNotNullEmpty().Length;
				if (currentSize > size)
				{
					size = currentSize;
				}
			}
			return size;
		}

		protected virtual string FormatOperationCD(int id, int size) => id.ToString().PadLeft(size, '0');


		protected virtual List<AMBomOper> UpdateOperationCD(List<AMBomOper> allOperations, AMBomOper changedBomOper, AMBomOper bomOperSibling, int step)
		{
			if (allOperations == null || allOperations.Count == 0)
			{
				return allOperations;
			}

			var cdSize = GetOperationCDSize(allOperations);
			var startingOperationCD = bomOperSibling?.NewOperationCD ?? GetSeperatedOperationCD(bomOperSibling?.OperationCD) ?? FormatOperationCD(step, cdSize);
			return UpdateOperationCD(allOperations, changedBomOper, bomOperSibling, step, cdSize, startingOperationCD);
		}

		protected virtual List<AMBomOper> UpdateOperationCD(List<AMBomOper> allOperations, AMBomOper changedBomOper, AMBomOper bomOperSibling, int step, int cdSize, string startingOperationCD)
		{
			if(allOperations == null || allOperations.Count == 0)
			{
				return allOperations;
			}

			if(step <= 0 || step > 10000)
			{
				throw new ArgumentOutOfRangeException(nameof(step));
			}

			AMBomOper original = null;
			if (changedBomOper?.OperationID != null)
			{
				original = allOperations.Where(r => r.OperationID == changedBomOper.OperationID).FirstOrDefault();
				if (original?.OperationID != null)
				{
					allOperations.Remove(original);
					original = PXCache<AMBomOper>.CreateCopy(original);
				}

				changedBomOper.OperationCD = $"{bomOperSibling?.NewOperationCD ?? GetSeperatedOperationCD(bomOperSibling?.OperationCD)}{NEWCDSEPERATOR}0";
				allOperations.Add(changedBomOper);
			}

			var startNewCD = bomOperSibling == null;
			var nextOperationCDint = ToNextRoundedStep(string.IsNullOrWhiteSpace(startingOperationCD) ? step : Convert.ToInt32(startingOperationCD), step);
			var updatedOperations = new List<AMBomOper>();
#if DEBUG
			var sb = new System.Text.StringBuilder();
			sb.AppendLine("[UpdateOperationCD]");
#endif
			foreach (var oper in allOperations.OrderBy(r => r.NewOperationCD ?? r.OperationCD))
			{
				var pendingOperationCD = oper.NewOperationCD ?? oper.OperationCD;
				var nextOperationCD = FormatOperationCD(nextOperationCDint, cdSize);
				var newCDmsg = string.Empty;
#if DEBUG
				if (oper.NewOperationCD != null)
				{
					newCDmsg = $" [New CD '{oper.NewOperationCD}']";
				} 
#endif
				if (!startNewCD)
				{
					if (oper.OperationID == bomOperSibling.OperationID)
					{
						startNewCD = true;
					}
#if DEBUG
					sb.AppendLine($"DB ID '{oper.OperationID}' CD '{oper.OperationCD}'{newCDmsg} not changing {(startNewCD ? "[SIBLING]" : string.Empty)}");
#endif
					if (pendingOperationCD == nextOperationCD)
					{
						nextOperationCDint += step;
					}
					continue;
				}

				oper.OperationCD = (original?.OperationID != null && original.OperationID == oper?.OperationID) ? original?.OperationCD : oper.OperationCD;
				oper.NewOperationCD = nextOperationCD;
#if DEBUG
				sb.AppendLine($"DB ID '{oper.OperationID}' changing CD '{pendingOperationCD}'{newCDmsg} to '{nextOperationCD}'");
#endif

				nextOperationCDint += step;
				updatedOperations.Add(oper);
			}
#if DEBUG
			PXTrace.WriteInformation(sb.ToString());
			AMDebug.TraceWriteMethodName(sb.ToString());
#endif
			return updatedOperations;
		}

		/// <summary>
		/// Updating <see cref="AMBomOper"/> OperationCD values
		/// </summary>
		/// <param name="updatingOperations">Rows to update/insert in cache</param>
		/// <param name="allBomOperations">All existing BOM Operations</param>
		/// <returns>Inserted AMBomOpers</returns>
		protected virtual List<AMBomOper> UpdateOperationCD(AMBomItem bomItem, List<AMBomOper> updatingOperations, List<AMBomOper> allBomOperations)
		{
			var insertedBomOpers = new List<AMBomOper>();
			var allOperations = new List<AMBomOper>(allBomOperations);
			if (updatingOperations == null)
			{
				return insertedBomOpers;
			}

			var toBeInserted = new List<AMBomOper>();
			foreach (var operation in updatingOperations)
			{
				if (operation.OperationID == null)
				{
					toBeInserted.Add(operation);
					continue;
				}

				allOperations.Add(BomOperRecords.Update(operation));
			}

			foreach (var operation in toBeInserted)
			{
				if (OperationCDExists(allOperations, operation.NewOperationCD, true) == 0)
				{
					operation.OperationCD = operation.NewOperationCD;
				}

				var inserted = InsertAMBomOper(operation, Documents.Cache.LocateElse(bomItem));
				if (inserted?.OperationID != null)
				{
					insertedBomOpers.Add(inserted);
					allOperations.Add(inserted);
				}
			}

			return insertedBomOpers;
		}


		protected virtual int ToNextRoundedStep(int val, int step)
		{
			var divEven = val / step;
			var balance = val - (divEven * step);
			return balance <= 0 ? val : (divEven + 1) * step;
		}

		protected virtual void TreeNodeMaterialMoved(string id, string parent, string siblingBefore)
		{
			var bomMatl = GetBomMatlFromTreeNodeID(id);
			if(bomMatl?.BOMID != null)
			{
				bomMatl.OriginalTreeNodeID = id;
			}

			TreeNodeMaterialMoved(bomMatl, GetBomOperFromTreeNodeID(parent), GetBomMatlFromTreeNodeID(siblingBefore));
		}

		protected virtual void TreeNodeMaterialMoved(AMBomMatl bomMatl, AMBomOper bomOper, AMBomMatl bomMatlSibling)
		{
			if(string.IsNullOrWhiteSpace(bomMatl?.BOMID) || string.IsNullOrWhiteSpace(bomOper?.BOMID) || BomMatlRecords.Cache.GetStatus(bomMatl).IsDeleted())
			{
				return;
			}

			var sameOper = bomMatl.SameBOMOper(bomOper);
			if (!sameOper)
			{
				var newBomMatl = MoveMaterialToOperation(bomMatl, bomOper, bomMatlSibling);
				if (newBomMatl?.OriginalTreeNodeID != null)
				{
					var subAssemblyBomItem = GetSubassemblyBOMFromTreeNode(bomMatl?.OriginalTreeNodeID);
					TreeNodeSelected(new TreeNodeEventResult { ID = GetBomMatlKey(bomOper, newBomMatl, subAssemblyBomItem) }, bomOper, newBomMatl, subAssemblyBomItem);
				}
				
				return;
			}

			var newSortOrder = (bomMatlSibling?.SortOrder ?? 0) + 1;
			if (newSortOrder != bomMatl.SortOrder)
			{
				bomMatl.SortOrder = newSortOrder;
				bomMatl = BomMatlRecords.Update(bomMatl);
			}

			MoveMaterialSortOrder(bomMatl, newSortOrder);
		}

		protected virtual AMBomMatl MoveMaterialToOperation(AMBomMatl oldBomMatl, AMBomOper newBomOper, AMBomMatl siblingBomMatl)
		{
			var newBomMatl = PXCache<AMBomMatl>.CreateCopy(oldBomMatl);
			newBomMatl.BOMID = newBomOper.BOMID;
			newBomMatl.RevisionID = newBomOper.RevisionID;
			newBomMatl.OperationID = newBomOper.OperationID;
			newBomMatl.LineID = null;
			newBomMatl.SortOrder = (siblingBomMatl?.BOMID == null ? 0 : (siblingBomMatl.SortOrder ?? 0)) + 1;

			var insertedBomMatl = MoveMaterialInsertDelete(oldBomMatl, newBomMatl, newBomOper);
			if (insertedBomMatl != null)
			{
				MoveMaterialSortOrder(insertedBomMatl, insertedBomMatl.SortOrder ?? insertedBomMatl.LineID ?? 0);
			}
			
			return insertedBomMatl;
		}

		/// <summary>
		/// When moving material to another operation the material moved is deleted and then inserted to the new operation
		/// </summary>
		/// <param name="deletingMatl">Material record deleting</param>
		/// <param name="insertingMatl">Material record copy of deleting now pointing to new operation</param>
		/// <param name="insertingMatlOper">Operation related to inserting material</param>
		/// <returns>Result of Inserted material record</returns>
		protected virtual AMBomMatl MoveMaterialInsertDelete(AMBomMatl deletingMatl, AMBomMatl insertingMatl, AMBomOper insertingMatlOper)
		{
			if (insertingMatl?.OperationID == null || insertingMatlOper?.OperationID == null)
			{
				return null;
			}

			var inserted = InsertAMBomMatl(insertingMatl, insertingMatlOper);
			if (inserted != null && deletingMatl?.LineID != null)
			{
				BomMatlRecords.Delete(deletingMatl);
			}
			return inserted;
		}

		protected virtual void MoveMaterialSortOrder(AMBomMatl currentBomMatl, int startingSortOrder)
		{
			if(currentBomMatl?.BOMID == null)
			{
				return;
			}

			var currentSortOrder = startingSortOrder;
			foreach (var matl in SelectSiblings(currentBomMatl)
				?.Where(r => r.SortOrder >= currentBomMatl.SortOrder && r.LineID != currentBomMatl.LineID)
				?.OrderBy(r => r.SortOrder))
			{
				matl.SortOrder = ++currentSortOrder;
				BomMatlRecords.Update(matl);
			}
		}

		public virtual IEnumerable<AMBomMatl> SelectSiblings(AMBomMatl bomMatl)
		{
			return SelectFrom<AMBomMatl>
				.Where<AMBomMatl.bOMID.IsEqual<@P.AsString>
				.And<AMBomMatl.revisionID.IsEqual<@P.AsString>
				.And<AMBomMatl.operationID.IsEqual<@P.AsInt>>>>
				.View.Select(this, bomMatl?.BOMID, bomMatl?.RevisionID, bomMatl?.OperationID).ToFirstTable();
		}

		protected virtual List<AMBomOper> GetBomOperations(IBomRevision bomRevision)
		{
			return SelectFrom<AMBomOper>
				.Where<AMBomOper.bOMID.IsEqual<@P.AsString>
					.And<AMBomOper.revisionID.IsEqual<@P.AsString>>>
				.View.Select(this, bomRevision?.BOMID, bomRevision?.RevisionID)
				.ToFirstTableList();
		}

		protected virtual AMBomOper GetLastBomOperation(IBomRevision bomRevision)
		{
			var bomOperations = GetBomOperations(bomRevision);
			return bomOperations != null && bomOperations.Any_() ? bomOperations.Last() : null;
		}

		protected virtual void TreeNodeAdd(List<TreeNodeEventResult> treeNodeResults)
		{
			foreach (var treeNode in treeNodeResults)
			{
				TreeNodeAdd(treeNode);
			}
		}

		protected virtual void TreeNodeAdd(TreeNodeEventResult treeNode)
		{
			if (PXTreeHelper.IsMaterialTreeNode(treeNode.ID))
			{
				TreeNodeAddMaterial(treeNode);
				return;
			}

			TreeNodeAddOperation(treeNode);
		}

		protected virtual void TreeNodeAddOperation(TreeNodeEventResult treeNode)
		{
			var bomOperKeys = PXTreeHelper.GetOperKeysFromTreeNodeID(treeNode?.ID);
			if(bomOperKeys == null || !bomOperKeys.IsValidID)
			{
				return;
			}

			var bomItem = AMBomItem.PK.Find(this, bomOperKeys.BOMID, bomOperKeys.RevisionID);
			if (bomItem?.BOMID == null)
			{
				return;
			}

			var operationCD = bomOperKeys.OperationCD != null && bomOperKeys.OperationCD.Contains(NEWCDSEPERATOR)
				? bomOperKeys.OperationCD
				: GetNewOperationCD(bomItem, bomOperKeys.OperationCD);
			var newBomOper = new AMBomOper
			{
				BOMID = bomItem.BOMID,
				RevisionID = bomItem.RevisionID,
				OperationCD = operationCD,
				NewOperationCD = bomOperKeys.OperationCD,
				OriginalTreeNodeID = treeNode?.ID
			};

			TreeNodeAddOperation(bomItem, newBomOper, GetBomOperFromTreeNodeID(treeNode.PrevNodeID));
		}

		protected virtual void TreeNodeAddOperation(AMBomItem bomItem, AMBomOper newBomOper, AMBomOper siblingBomOper)
		{
			var operations = GetBomOperations(bomItem);
			if(operations != null)
			{
				operations.Add(newBomOper);
			}

			UpdateOperationCD(
				bomItem,
				UpdateOperationCD(operations, newBomOper, siblingBomOper, OperationStep()),
				operations);
		}

		protected virtual AMBomOper FindBomOper(IBomOper bomOper)
		{
			return FindBomOper(bomOper?.BOMID, bomOper?.RevisionID, bomOper?.OperationID);
		}

		protected virtual AMBomOper FindBomOper(string bomID, string revisionID, int? operationID)
		{
			return AMBomOper.PK.Find(this, bomID, revisionID, operationID)
			// PK.Find will not find an inserted oper
			?? BomOperRecords.Cache.Inserted.RowCast<AMBomOper>()
				.Where(r => r.BOMID == bomID && r.RevisionID == revisionID && r.OperationID == operationID).FirstOrDefault();
		}

		protected virtual void TreeNodeAddMaterial(TreeNodeEventResult treeNode)
		{
			var bomMatlKeys = PXTreeHelper.GetMatlKeysFromTreeNodeID(treeNode?.ID);
			if (bomMatlKeys == null || !bomMatlKeys.IsValidID)
			{
				return;
			}

			// sub query required as join will not pick up inserted oper
			var bomOper = FindBomOper(bomMatlKeys.BOMID, bomMatlKeys.RevisionID, bomMatlKeys.OperationID);
			if (bomOper?.BOMID == null)
			{
				return;
			}

			var newBomMatl = new AMBomMatl
			{
				BOMID = bomOper.BOMID,
				RevisionID = bomOper.RevisionID,
				OperationID = bomOper.OperationID,
				OriginalTreeNodeID = treeNode?.ID,
				SortOrder = 0
			};

			TreeNodeAddMaterial(bomOper, newBomMatl, GetBomMatlFromTreeNodeID(treeNode.PrevNodeID));
		}

		protected virtual void TreeNodeAddMaterial(AMBomOper bomOper, AMBomMatl newBomMatl, AMBomMatl bomMatlSibling)
		{
			var newSortOrder = (bomMatlSibling?.SortOrder ?? 0) + 1;
			if (newSortOrder != newBomMatl.SortOrder)
			{
				newBomMatl.SortOrder = newSortOrder;
			}

			MoveMaterialSortOrder(InsertAMBomMatl(newBomMatl, bomOper), newSortOrder);
		}

		protected virtual void TreeNodeDelete(List<TreeNodeEventResult> treeNodeResults)
		{
			foreach (var treeNode in treeNodeResults)
			{
				TreeNodeDelete(treeNode?.ID);
			}
		}

		protected virtual void TreeNodeDelete(string id)
		{
			if(PXTreeHelper.IsMaterialTreeNode(id))
			{
				TreeNodeDeleteMaterial(GetBomMatlFromTreeNodeID(id));
				return;
			}

			if(PXTreeHelper.IsOperationTreeNode(id))
			{
				TreeNodeDeleteOperation(GetBomOperFromTreeNodeID(id));
				return;
			}
		}

		protected virtual void TreeNodeDeleteMaterial(AMBomMatl bomMatl)
		{
			if (string.IsNullOrWhiteSpace(bomMatl?.BOMID))
			{
				return;
			}

			BomMatlRecords.Delete(bomMatl);
		}

		protected virtual void TreeNodeDeleteOperation(AMBomOper bomOper)
		{
			if (string.IsNullOrWhiteSpace(bomOper?.BOMID))
			{
				return;
			}

			BomOperRecords.Delete(bomOper);
		}


		protected override void BOMRowEnableChildCache(PXCache cache, AMBomItem bomItem)
		{
			var selectedBomItem2 = SelectedBomItem2.Current;
			if(selectedBomItem2 == null || bomItem == null || selectedBomItem2.SameBOM(bomItem))
			{
				base.BOMRowEnableChildCache(cache, bomItem);
				return;
			}

			base.BOMRowEnableChildCache(cache, selectedBomItem2);
		}

		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<EngineeringWorkbenchMaint, AMBomItem>());
		protected static void Configure(WorkflowContext<EngineeringWorkbenchMaint, AMBomItem> context)
		{
			var customOtherCategory = context.Categories.CreateNew("CustomOther",
				category => category.DisplayName("Other"));

			context.AddScreenConfigurationFor(screen =>
			{
				return screen
					  .WithCategories(categories =>
					  {
						  categories.Add(customOtherCategory);
						  categories.Update(FolderType.InquiriesFolder, category => category.PlaceAfter(customOtherCategory));
						  categories.Update(FolderType.ReportsFolder, category => category.PlaceAfter(FolderType.InquiriesFolder));
					  })
					.WithActions(actions =>
					{
						actions.Add(g => g.AMCopyBom, c => c.WithCategory(customOtherCategory));
						actions.Add(g => g.AMBomCostSettings, c => c.WithCategory(customOtherCategory));
						actions.Add(g => g.MakeDefaultBomAction, c => c.WithCategory(customOtherCategory));
						actions.Add(g => g.MakePlanningBomAction, c => c.WithCategory(customOtherCategory));
						actions.Add(g => g.Attributes, c => c.WithCategory(customOtherCategory));
						actions.Add(g => g.ArchiveBom, c => c.WithCategory(customOtherCategory));
						actions.Add(g => g.CreateECR, c => c.WithCategory(customOtherCategory));

						actions.Add(g => g.BOMCompare, a => a.WithCategory(PredefinedCategory.Inquiries));
						actions.Add(g => g.ReportBOMSummary, a => a.WithCategory(PredefinedCategory.Reports));
						actions.Add(g => g.ReportMultiLevel, c => c.WithCategory(PredefinedCategory.Reports));
					});
			});
		}

		public static void RedirectActive(string bomID)
		{
			if (string.IsNullOrWhiteSpace(bomID))
			{
				return;
			}

			var graph = CreateInstance<EngineeringWorkbenchMaint>();
			Redirect(graph, PrimaryBomIDManager.GetActiveRevisionBomItem(graph, bomID));
		}

		public static void Redirect(string bomID, string revision)
		{
			if (string.IsNullOrWhiteSpace(bomID))
			{
				return;
			}

			var graph = CreateInstance<EngineeringWorkbenchMaint>();
			Redirect(graph, AMBomItem.PK.Find(graph, bomID, revision));
		}

		public static void Redirect(AMBomItem bomItem) => Redirect(CreateInstance<EngineeringWorkbenchMaint>(), bomItem);

		private static void Redirect(EngineeringWorkbenchMaint graph, AMBomItem bomItem)
		{
			if (string.IsNullOrWhiteSpace(bomItem?.BOMID))
			{
				return;
			}

			graph.Documents.Current = bomItem;
			if (graph.Documents.Current != null)
			{
				throw new PXRedirectRequiredException(graph, true, Messages.BOMMaint);
			}
		}

		protected override void EnableOperCache(bool enabled)
		{
			base.EnableOperCache(enabled);

			SelectedBomOper.AllowInsert = enabled;
			SelectedBomOper.AllowUpdate = enabled;
			SelectedBomOper.AllowDelete = enabled;
			PXUIFieldAttribute.SetEnabled<AMBomOper.operationCD>(SelectedBomOper.Cache, SelectedBomOper.Current, enabled);

			SelectedBomMatl.AllowInsert = enabled;
			SelectedBomMatl.AllowUpdate = enabled;
			SelectedBomMatl.AllowDelete = enabled;
		}
	}
}
