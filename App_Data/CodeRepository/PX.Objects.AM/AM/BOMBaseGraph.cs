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
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AM.Attributes;
using PX.Objects.Common;
using PX.Objects.Common.Scopes;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM
{
	public abstract class BOMGraph<TGraph> : BOMBaseGraph<TGraph>
	where TGraph : BOMBaseGraph<TGraph>, new()
	{
		public new PXSaveCancel<AMBomItem> Save;
		public new PXRevisionableCancel<TGraph, AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Cancel;
		public new PXRevisionableInsert<AMBomItem> Insert;
		public new PXDelete<AMBomItem> Delete;
		public new PXRevisionableFirst<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> First;
		public new PXRevisionablePrevious<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Previous;
		public new PXRevisionableNext<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Next;
		public new PXRevisionableLast<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Last;
	}

	public abstract class BOMBaseGraph<TGraph> : PXRevisionableGraphBase<TGraph, AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID>
		where TGraph : PXRevisionableGraphBase<TGraph, AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID>, new()
	{
		public BOMBaseGraph()
		{
			var bomSetup = ambsetup.Current;
			if (string.IsNullOrWhiteSpace(bomSetup?.BOMNumberingID))
			{
				throw new BOMSetupNotEnteredException();
			}
		}

		public override bool CanClipboardCopyPaste()
		{
			return false;
		}

		public bool MBCEnabled => PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();

		public override void Persist()
		{
			InsertMissingINItemSite();

			var setPrimary = Documents.Cache.GetStatus(Documents.Current) == PXEntryStatus.Inserted &&
							 !HasAnotherRevision(Documents.Current);

			var estimateHistoryRow = GetInsertedEstimateHistory();

			UpdateLowLevelCompleted();
			base.Persist();


			if (setPrimary)
			{
				// Set Primary BOM is after persist so we can use the BOMID. Its doing a separate persist call
				SetPrimeBomID(Documents.Current);
			}

			PersistEstimateHistoryRow(Documents?.Current, estimateHistoryRow);
		}

		protected virtual void UpdateLowLevelCompleted()
		{
			var deletedCount = BomMatlRecords.Cache.Deleted.Count() + Documents.Cache.Deleted.Count();

			if (deletedCount > 0)
			{
				PXUpdate<Set<AMBSetup.lastLowLevelCompletedDateTime, Null>, AMBSetup>.Update(this);
			}
		}

		//We get field name cannot be empty but no indication to which DAC, so we add this for improved error reporting
		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			try
			{
				return base.Persist(cacheType, operation);
			}
			catch (Exception e)
			{
				PXTrace.WriteError($"Persist; cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}; {e.Message}");
#if DEBUG
				AMDebug.TraceWriteMethodName($"Persist; cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}; {e.Message}");
#endif
				throw;
			}
		}

		public virtual bool BOMAllowsEdit(AMBomItem bomItem)
		{
			return BOMOnHold(bomItem) && ECRAllowEdit(bomItem?.BOMID);
		}

		public virtual bool BOMOnHold(AMBomItem bomItem)
		{
			return bomItem?.Hold == true;
		}

		protected virtual bool ECRAllowEdit(string bomid)
		{
			if (Features.ECCEnabled() && ambsetup.Current?.ForceECR == true)
			{
				var existingECR = (AMECRItem)PXSelect<AMECRItem, Where<AMECRItem.bOMID, Equal<Required<AMECRItem.bOMID>>>>.SelectWindowed(this, 0, 1, bomid);
				if (existingECR != null)
				{
					return false;
				}
				var existingECO = (AMECOItem)PXSelect<AMECOItem, Where<AMECOItem.bOMID, Equal<Required<AMECOItem.bOMID>>>>.SelectWindowed(this, 0, 1, bomid);
				if (existingECO != null)
				{
					return false;
				}
			}

			return true;
		}

		#region Views

		//Primary view "Documents" comes from PXRevisionablegraph

		[PXImport(typeof(AMBomItem))]
		public PXSelect<AMBomOper,
			Where<AMBomOper.bOMID, Equal<Current<AMBomItem.bOMID>>,
				And<AMBomOper.revisionID, Equal<Current<AMBomItem.revisionID>>>>,
			OrderBy<Asc<AMBomOper.operationCD>>> BomOperRecords;

		[PXImport(typeof(AMBomItem))]
		public AMOrderedMatlSelect<AMBomItem, AMBomMatl,
			Where<AMBomMatl.bOMID, Equal<Current<AMBomOper.bOMID>>,
				And<AMBomMatl.revisionID, Equal<Current<AMBomOper.revisionID>>,
				And<AMBomMatl.operationID, Equal<Current<AMBomOper.operationID>>>>>,
			OrderBy<Asc<AMBomMatl.sortOrder, Asc<AMBomMatl.lineID>>>> BomMatlRecords;

		[PXImport(typeof(AMBomItem))]
		public PXSelect<AMBomStep,
			Where<AMBomStep.bOMID, Equal<Current<AMBomOper.bOMID>>,
				And<AMBomStep.revisionID, Equal<Current<AMBomOper.revisionID>>,
				And<AMBomStep.operationID, Equal<Current<AMBomOper.operationID>>>>>,
			OrderBy<Asc<AMBomStep.sortOrder, Asc<AMBomStep.lineID>>>> BomStepRecords;

		[PXImport(typeof(AMBomItem))]
		public PXSelectJoin<AMBomTool,
			InnerJoin<AMToolMst, On<AMBomTool.toolID, Equal<AMToolMst.toolID>>>,
			Where<AMBomTool.bOMID, Equal<Current<AMBomOper.bOMID>>,
				And<AMBomTool.revisionID, Equal<Current<AMBomOper.revisionID>>,
				And<AMBomTool.operationID, Equal<Current<AMBomOper.operationID>>>>>> BomToolRecords;

		[PXImport(typeof(AMBomItem))]
		public PXSelectJoin<AMBomOvhd,
			InnerJoin<AMOverhead, On<AMBomOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
			Where<AMBomOvhd.bOMID, Equal<Current<AMBomOper.bOMID>>,
				And<AMBomOvhd.revisionID, Equal<Current<AMBomOper.revisionID>>,
				And<AMBomOvhd.operationID, Equal<Current<AMBomOper.operationID>>>>>> BomOvhdRecords;

		public PXSelect<AMBomRef,
			Where<AMBomRef.bOMID, Equal<Current<AMBomMatl.bOMID>>,
				And<AMBomRef.revisionID, Equal<Current<AMBomMatl.revisionID>>,
				And<AMBomRef.operationID, Equal<Current<AMBomMatl.operationID>>,
				And<AMBomRef.matlLineID, Equal<Current<AMBomMatl.lineID>>>>>>> BomRefRecords;

		[PXHidden]
		public PXSelect<AMBomOper,
			Where<AMBomOper.bOMID, Equal<Current<AMBomOper.bOMID>>,
				And<AMBomOper.revisionID, Equal<Current<AMBomOper.revisionID>>,
				And<AMBomOper.operationID, Equal<Current<AMBomOper.operationID>>>>>> OutsideProcessingOperationSelected;


		public SelectFrom<AMBomOperCury>.Where<AMBomOperCury.bOMID.IsEqual<AMBomOper.bOMID.FromCurrent>
			.And<AMBomOperCury.revisionID.IsEqual<AMBomOper.revisionID.FromCurrent>>
			.And<AMBomOperCury.operationID.IsEqual<AMBomOper.operationID.FromCurrent>>
			.And<AMBomOperCury.curyID.IsEqual<AccessInfo.baseCuryID.AsOptional>>>.View BomOperCurySelected;

		public SelectFrom<AMBomMatlCury>.Where<AMBomMatlCury.bOMID.IsEqual<AMBomMatl.bOMID.FromCurrent>
			.And<AMBomMatlCury.revisionID.IsEqual<AMBomMatl.revisionID.FromCurrent>>
			.And<AMBomMatlCury.operationID.IsEqual<AMBomMatl.operationID.FromCurrent>>
			.And<AMBomMatlCury.lineID.IsEqual<AMBomMatl.lineID.FromCurrent>>>.View BomMatlCurySelected;

		[PXHidden]
		public PXSelect<
			AMBomAttribute,
			Where<AMBomAttribute.bOMID, Equal<Current<AMBomItem.bOMID>>,
				And<AMBomAttribute.revisionID, Equal<Current<AMBomItem.revisionID>>>>> BomAttributes;

		public PXSetup<AMBSetup> ambsetup;
		public PXSetup<AMPSetup> ProdSetup;

		public PXFilter<CopyBomFilter> copyBomFilter;
		public PXFilter<RollupSettings> rollsettings;

		[PXHidden]
		public PXSetup<Numbering,
			LeftJoin<AMBSetup, On<AMBSetup.bOMNumberingID, Equal<Numbering.numberingID>>>,
			Where<Numbering.numberingID, Equal<AMBSetup.bOMNumberingID>>> BomNumbering;

		[PXHidden]
		public PXSelect<AMBomCost, Where<AMBomCost.userID, Equal<Current<AccessInfo.userID>>,
			And<AMBomCost.bOMID, Equal<Current<AMBomItem.bOMID>>>>> BomCostRecs;
		[PXHidden]
		public PXSelect<INItemSite, Where<INItemSite.inventoryID, Equal<Current<AMBomItem.inventoryID>>,
			And<INItemSite.siteID, Equal<Current<AMBomItem.siteID>>>>> ItemSiteRecord;
		[PXHidden]
		public PXFilter<DefaultBomLevels> DefaultBomLevelsFilter;
		[PXHidden]
		public PXFilter<DefaultBomLevels> PlanningBomLevelsFilter;

		[PXHidden]
		public PXSelect<AMECOItem> EcoItem;

		#endregion

		#region Cache Attached

		[BomID(IsKey = true, Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[Rev.Key(typeof(AMBSetup.bOMNumberingID),
				typeof(AMBomItem.bOMID),
				typeof(AMBomItem.revisionID),
				new[]{
					typeof(AMBomItem.bOMID),
					typeof(AMBomItem.revisionID),
					typeof(AMBomItem.inventoryID),
					typeof(AMBomItem.subItemID),
					typeof(AMBomItem.siteID)
				})]
		protected virtual void _(Events.CacheAttached<AMBomItem.bOMID> e)
		{
			// Do Not Remove this
			// used to allow BOM Maint to use the Revisionable Graph for Copy and navigation while Reports and other screens use standard selectors
		}

		[RevisionIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		[PXDefault(typeof(AMBSetup.defaultRevisionID), PersistingCheck = PXPersistingCheck.Nothing)]
		[Rev.ID(typeof(AMBSetup.defaultRevisionID),
			typeof(AMBomItem.bOMID),
			typeof(AMBomItem.revisionID),
			typeof(AMBomItem.revisionID),
			typeof(AMBomItem.status),
			typeof(AMBomItem.descr),
			typeof(AMBomItem.effStartDate),
			typeof(AMBomItem.effEndDate),
			DescriptionField = typeof(AMBomItem.descr))]
		protected virtual void _(Events.CacheAttached<AMBomItem.revisionID> e)
		{
			// Do Not Remove this
			// used to allow BOM Maint to use the Revisionable Graph for Copy and navigation while Reports and other screens use standard selectors
		}

		[PXDBQuantity]
		[PXUIField(DisplayName = "Lot Size")]
		protected virtual void _(Events.CacheAttached<RollupSettings.lotSize> e)
		{
			//Removing default
		}

		[PXDBBool]
		[PXDefault(false, typeof(Search<AMWC.bflushMatl, Where<AMWC.wcID, Equal<Current<AMBomOper.wcID>>>>))]
		[PXUIField(DisplayName = "Backflush")]
		protected virtual void _(Events.CacheAttached<AMBomMatl.bFlush> e) { }

		[BomID(DisplayName = "Comp BOM ID")]
		[BOMIDSelector(typeof(Search2<AMBomItemActive.bOMID,
			LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<AMBomItemActive.inventoryID>>>,
			Where<AMBomItemActive.inventoryID, Equal<Current<AMBomMatl.inventoryID>>>>))]
		protected virtual void _(Events.CacheAttached<AMBomMatl.compBOMID> e) { }

		[OperationIDField(IsKey = true, Visible = false, Enabled = false, DisplayName = "Operation DB ID")]
		[PXLineNbr(typeof(AMBomItem.lineCntrOperation))]
		protected virtual void _(Events.CacheAttached<AMBomOper.operationID> e)
		{
#if DEBUG
			//Cache attached to change display name so we can provide the user with a way to see the DB ID if needed 
#endif
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelector(typeof(Search<AMBomOper.operationID,
				Where<AMBomOper.bOMID, Equal<Current<AMBomMatl.bOMID>>,
				And<AMBomOper.revisionID, Equal<Current<AMBomMatl.revisionID>>>>>),
			SubstituteKey = typeof(AMBomOper.operationCD),
			ValidateValue = false, DirtyRead = true)]
		protected virtual void _(Events.CacheAttached<AMBomMatl.operationID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelector(typeof(Search<AMBomOper.operationID,
				Where<AMBomOper.bOMID, Equal<Current<AMBomOvhd.bOMID>>,
				And<AMBomOper.revisionID, Equal<Current<AMBomOvhd.revisionID>>>>>),
			SubstituteKey = typeof(AMBomOper.operationCD),
			ValidateValue = false, DirtyRead = true)]
		protected virtual void _(Events.CacheAttached<AMBomOvhd.operationID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelector(typeof(Search<AMBomOper.operationID,
				Where<AMBomOper.bOMID, Equal<Current<AMBomRef.bOMID>>,
				And<AMBomOper.revisionID, Equal<Current<AMBomRef.revisionID>>>>>),
			SubstituteKey = typeof(AMBomOper.operationCD),
			ValidateValue = false, DirtyRead = true)]
		protected virtual void _(Events.CacheAttached<AMBomRef.operationID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelector(typeof(Search<AMBomOper.operationID,
				Where<AMBomOper.bOMID, Equal<Current<AMBomStep.bOMID>>,
				And<AMBomOper.revisionID, Equal<Current<AMBomStep.revisionID>>>>>),
			SubstituteKey = typeof(AMBomOper.operationCD),
			ValidateValue = false, DirtyRead = true)]
		protected virtual void _(Events.CacheAttached<AMBomStep.operationID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelector(typeof(Search<AMBomOper.operationID,
				Where<AMBomOper.bOMID, Equal<Current<AMBomTool.bOMID>>,
				And<AMBomOper.revisionID, Equal<Current<AMBomTool.revisionID>>>>>),
			SubstituteKey = typeof(AMBomOper.operationCD),
			ValidateValue = false, DirtyRead = true)]
		protected virtual void _(Events.CacheAttached<AMBomTool.operationID> e) { }

		#endregion

		#region View Delegates

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		protected virtual IEnumerable bomMatlRecords()
		{
			this.Caches[typeof(AMBomMatl)].ClearQueryCache();
			this.Caches[typeof(AMBomMatlCury)].ClearQueryCache();

			bool itVar1 = false;
			int? lineNbr = BomOperRecords.Current?.OperationID;
			IEnumerator enumerator = this.BomMatlRecords.Cache.Inserted.GetEnumerator();

			while (enumerator.MoveNext())
			{
				AMBomMatl master = (AMBomMatl)enumerator.Current;
				if (lineNbr == master.OperationID)
				{
				itVar1 = true;
				yield return master;
			}
			}

			if (!itVar1)
			{
				foreach (PXResult<AMBomMatl> result in SelectFrom<AMBomMatl>
			.Where<AMBomMatl.bOMID.IsEqual<AMBomOper.bOMID.FromCurrent>
			.And<AMBomMatl.revisionID.IsEqual<AMBomOper.revisionID.FromCurrent>>
			.And<AMBomMatl.operationID.IsEqual<AMBomOper.operationID.FromCurrent>>>
			.OrderBy<Asc<AMBomMatl.sortOrder, Asc<AMBomMatl.lineID>>>.View.Select(this))
				{
					var master = (AMBomMatl)result;
					if (master == null)
						yield break;

					if (this.BomMatlRecords.Cache.GetStatus(master) != PXEntryStatus.Updated)
					{
					AMBomMatlCury cury = AMBomMatlCury.PK.Find(this, master.BOMID, master.RevisionID, master.OperationID, master.LineID, Accessinfo.BaseCuryID);

					master.UnitCost = cury?.UnitCost ?? 0;
					master.PlanCost = (decimal?)PXFormulaAttribute.Evaluate<AMBomMatl.planCost>(BomMatlRecords.Cache, master);
					master.SiteID = cury?.SiteID;
					master.LocationID = cury?.LocationID;
					}
					yield return master;
				}
			}
		}

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		protected virtual IEnumerable bomToolRecords()
		{
			this.Caches[typeof(AMBomTool)].ClearQueryCache();
			this.Caches[typeof(AMBomToolCury)].ClearQueryCache();

			bool itVar1 = false;
			int? lineNbr = BomOperRecords.Current?.OperationID;
			IEnumerator enumerator = this.BomToolRecords.Cache.Inserted.GetEnumerator();

			while (enumerator.MoveNext())
			{
				AMBomTool master = (AMBomTool)enumerator.Current;
				if (lineNbr == master.OperationID)
				{
				itVar1 = true;
				AMBomToolCury cury = AMBomToolCury.PK.Find(this, master.BOMID, master.RevisionID, master.OperationID, master.LineID, Accessinfo.BaseCuryID);
					master.UnitCost = cury?.UnitCost ?? master.UnitCost;
				yield return master;
			}
			}
			
			if (!itVar1)
			{
				foreach (PXResult<AMBomTool> result in SelectFrom<AMBomTool>
					.Where<AMBomTool.bOMID.IsEqual<AMBomOper.bOMID.FromCurrent>
					.And<AMBomTool.revisionID.IsEqual<AMBomOper.revisionID.FromCurrent>>
					.And<AMBomTool.operationID.IsEqual<AMBomOper.operationID.FromCurrent>>>.View.Select(this))
				{
					var master = (AMBomTool)result;
					if (master == null)
						yield break;

					AMBomToolCury cury = AMBomToolCury.PK.Find(this, master.BOMID, master.RevisionID, master.OperationID, master.LineID, Accessinfo.BaseCuryID);

					master.UnitCost = cury?.UnitCost ?? 0;
					yield return master;
				}
			}
		}

		#endregion

		#region Actions

		public PXAction<AMBomItem> ViewCompBomID;

		[PXButton]
		[PXUIField(Visible = false)]
		public virtual IEnumerable viewCompBomID(PXAdapter adapter)
		{
			BOMMaint.Redirect(BomMatlRecords?.Current?.CompBOMID, BomMatlRecords?.Current?.CompBOMRevisionID);
			return adapter.Get();
		}

		public PXAction<AMBomItem> ReportBOMSummary;
		[PXUIField(DisplayName = Messages.BOMSummary, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable reportBOMSummary(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				var parameters = Reports.BOMSummaryReportParams.FromBomId(Documents.Current.BOMID, Documents.Current.RevisionID);
				throw new PXReportRequiredException(parameters, Reports.BOMSummaryReportParams.ReportID, PXBaseRedirectException.WindowMode.New, Reports.BOMSummaryReportParams.ReportName);
			}
			return adapter.Get();
		}

		public PXAction<AMBomItem> ReportMultiLevel;
		[PXUIField(DisplayName = Messages.MultiLevel, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable reportMultiLevel(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				MultiLevelBomInq.RunReportNewWindow(Documents.Current.BOMID, Documents.Current.RevisionID);
			}
			return adapter.Get();
		}

		public PXAction<AMBomItem> MakeDefaultBomAction;
		[PXUIField(DisplayName = Messages.MakeDefaultBom, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable makeDefaultBomAction(PXAdapter adapter)
		{
			if (Documents.Current == null)
			{
				return adapter.Get();
			}

			if (DefaultBomLevelsFilter.AskExt() == WebDialogResult.OK)
			{
				try
				{
					var bomIDManager = new PrimaryBomIDManager(this);
					bomIDManager.PersistChanges = false;
					bomIDManager.SetPrimaryOverride(Documents.Current,
						DefaultBomLevelsFilter.Current.Item.GetValueOrDefault(),
						DefaultBomLevelsFilter.Current.Warehouse.GetValueOrDefault(),
						DefaultBomLevelsFilter.Current.SubItem.GetValueOrDefault());

					ItemSiteRecord.Select(); //required

					Persist();
				}
				catch (Exception exception)
				{
					PXTraceHelper.PxTraceException(exception);
					throw new Exception(Messages.MakeDefaultBomFailed, exception);
				}
			}

			return adapter.Get();
		}

		public PXAction<AMBomItem> MakePlanningBomAction;
		[PXUIField(DisplayName = Messages.MakePlanningBom, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable makePlanningBomAction(PXAdapter adapter)
		{
			if (Documents.Current == null)
			{
				return adapter.Get();
			}

			if (PlanningBomLevelsFilter.AskExt() == WebDialogResult.OK)
			{
				try
				{
					var bomIDManager = new PrimaryBomIDManager(this);
					bomIDManager.PersistChanges = false;
					bomIDManager.BOMIDType = PrimaryBomIDManager.BomIDType.Planning;
					bomIDManager.SetPrimaryOverride(Documents.Current,
					PlanningBomLevelsFilter.Current.Item.GetValueOrDefault(),
					PlanningBomLevelsFilter.Current.Warehouse.GetValueOrDefault(),
					PlanningBomLevelsFilter.Current.SubItem.GetValueOrDefault());

					ItemSiteRecord.Select(); //required

					Persist();
				}
				catch (Exception exception)
				{
					PXTraceHelper.PxTraceException(exception);
					throw new Exception(Messages.MakePrimaryBomFailed, exception);
				}
			}

			return adapter.Get();
		}

		/// <summary>
		/// BOM attributes redirect action
		/// </summary>
		public PXAction<AMBomItem> Attributes;
		/// <summary>
		/// BOM attributes redirect delegate
		/// </summary>
		[PXButton(OnClosingPopup = PXSpecialButtonType.Cancel, Tooltip = "Launch BOM Attributes")]
		[PXUIField(DisplayName = Messages.BOMAttributes, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		public virtual IEnumerable attributes(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				var graph = CreateInstance<BOMAttributeMaint>();
				graph.Documents.Current = this.Documents.Current;
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
			}

			return adapter.Get();
		}

		/// <summary>
		/// Copy bom smart panel to copy the current BOM contents to a new BOM using the entered item/site/sub item
		/// </summary>
		public PXAction<AMBomItem> AMCopyBom;
		/// <summary>
		/// Copy bom smart panel to copy the current BOM contents to a new BOM using the entered item/site/sub item
		/// </summary>
		[PXUIField(DisplayName = Messages.CopyBom, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable aMCopyBom(PXAdapter adapter)
		{
			var list = adapter.Get<AMBomItem>().ToList();

			copyBomFilter.Current.FromBOMID = Documents.Current.BOMID;
			copyBomFilter.Current.FromRevisionID = Documents.Current.RevisionID;
			copyBomFilter.Current.FromInventoryID = Documents.Current.InventoryID;
			copyBomFilter.Current.FromSubItemID = Documents.Current.SubItemID;
			copyBomFilter.Current.FromSiteID = Documents.Current.SiteID;

			if (string.IsNullOrEmpty(copyBomFilter.Current.ToBOMID) && !BomNumbering.Current.UserNumbering.GetValueOrDefault())
			{
				copyBomFilter.Current.ToBOMID = BomNumbering.Current.NewSymbol;
			}
			if (copyBomFilter.Current.ToInventoryID.GetValueOrDefault() == 0)
			{
				copyBomFilter.Current.ToInventoryID = Documents.Current.InventoryID;
				copyBomFilter.Cache.SetDefaultExt<CopyBomFilter.toSubItemID>(copyBomFilter.Current);
				copyBomFilter.Cache.SetDefaultExt<CopyBomFilter.toSubItemCD>(copyBomFilter.Current);
			}
			if (copyBomFilter.Current.ToSiteID.GetValueOrDefault() == 0)
			{
				copyBomFilter.Current.ToSiteID = Documents.Current.SiteID;
			}

			if (copyBomFilter.AskExt() == WebDialogResult.OK)
			{
				if (BomNumbering.Current.UserNumbering.GetValueOrDefault())
				{
					CheckExistingBom(copyBomFilter.Current.ToBOMID);
				}

				if (!string.IsNullOrWhiteSpace(copyBomFilter.Current.ToSubItemCD) && InventoryHelper.SubItemFeatureEnabled)
				{
					copyBomFilter.Current.ToSubItemID = InventoryHelper.GetSubItem(this, copyBomFilter.Current.ToSubItemCD, true)?.SubItemID;
				}

				if (!ValidCopyBomFilter(copyBomFilter.Current))
				{
					throw new ArgumentException(Messages.GetLocal(Messages.ArgumentInObjectNameInvalid, copyBomFilter.Name));
				}

				var sourceBom = PXCache<AMBomItem>.CreateCopy(Documents.Current);

				using(new CopyingBomScope())
				{
					CopyBom(sourceBom, copyBomFilter.Current);
				}

				var rs = new List<AMBomItem> { Documents.Current };
				return rs;
			}
			return list;
		}

		public PXAction<AMBomItem> ArchiveBom;
		[PXUIField(DisplayName = Messages.ArchiveBom, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable archiveBom(PXAdapter adapter)
		{
			var currentBomItem = Documents.Current;
			if (currentBomItem == null)
			{
				return adapter.Get();
			}

			if (currentBomItem.Hold.GetValueOrDefault())
			{
				//Change of hold status will reset the overall status. Update in 2 steps if currently on hold.
				currentBomItem.Hold = false;
				Documents.Update(currentBomItem);
			}

			currentBomItem.Status = AMBomStatus.Archived;

			if (currentBomItem.EffEndDate == null)
			{
				currentBomItem.EffEndDate = Accessinfo.BusinessDate;
			}

			Documents.Update(currentBomItem);

			Persist();

			return adapter.Get();
		}

		public PXAction<AMBomItem> CreateECR;
		[PXUIField(DisplayName = "Create ECR", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visibility = PXUIVisibility.Invisible)]
		[PXButton]
		public virtual IEnumerable createECR(PXAdapter adapter)
		{
			var currentBomItem = Documents.Current;
			if (currentBomItem?.RevisionID == null)
			{
				return adapter.Get();
			}

			var ecrGraph = CreateInstance<ECRMaint>();

			ecrGraph.FieldDefaulting.AddHandler<AMECRItem.bOMRevisionID>((sender, e) => { e.NewValue = currentBomItem.RevisionID; });

			var newEcr = ecrGraph.Documents.Insert();
			if (newEcr == null)
			{
				return adapter.Get();
			}
			newEcr.BOMID = currentBomItem.BOMID;
			newEcr.BOMRevisionID = currentBomItem.RevisionID;
			ecrGraph.Documents.Update(newEcr);

			PXRedirectHelper.TryRedirect(ecrGraph, PXRedirectHelper.WindowMode.New);

			return adapter.Get();
		}

		public PXAction<AMBomItem> BOMCompare;
		[PXUIField(DisplayName = "Compare BOM", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable bOMCompare(PXAdapter adapter)
		{
			var currentBomItem = Documents.Current;
			if (currentBomItem != null)
			{
				var graph = CreateInstance<BOMCompareInq>();

				graph.Filter.Current.IDType1 = BOMCompareInq.IDTypes.BOM;
				graph.Filter.Current.BOMID1 = currentBomItem.BOMID;
				graph.Filter.Current.RevisionID1 = currentBomItem.RevisionID;

				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
			}

			return adapter.Get();
		}

		public PXAction<AMBomItem> AMBomCostSettings;
		[PXUIField(DisplayName = Messages.BOMCostSummary, MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable aMBomCostSettings(PXAdapter adapter)
		{
			if (Documents.Current != null
				&& rollsettings.Current != null
				&& rollsettings.Current.LotSize == null)
			{
				rollsettings.Cache.SetValueExt<RollupSettings.lotSize>(rollsettings.Current,
					InventoryHelper.GetMfgReorderQty(this,
						Documents.Current.InventoryID,
						Documents.Current.SiteID));
			}

			if (rollsettings.Current != null
				&& rollsettings.Current.LotSize.GetValueOrDefault() <= 0)
			{
				rollsettings.Current.LotSize = 1;
			}

			if (rollsettings.AskExt() == WebDialogResult.OK)
			{
				rollsettings.Current.ApplyPend = false;
				rollsettings.Current.BOMID = Documents.Current.BOMID;
				rollsettings.Current.RevisionID = Documents.Current.RevisionID;
				rollsettings.Current.IncFixed = true;
				rollsettings.Current.IncMatScrp = true;
				rollsettings.Current.UpdateMaterial = false;
				// Call the action to run and display the cost roll
				aMBomCostSummary(adapter);
			}

			rollsettings.Cache.Clear();
			rollsettings.ClearDialog();

			return adapter.Get();
		}

		public PXAction<AMBomItem> AMBomCostSummary;
		[PXUIField(DisplayName = Messages.BOMCostSummary, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable aMBomCostSummary(PXAdapter adapter)
		{
			if (rollsettings.Current != null)
			{
				if (Documents.Current?.EffEndDate != null)
				{
					rollsettings.Current.EffectiveDate = rollsettings.Current.EffectiveDate.LesserDateTime(Documents.Current?.EffEndDate);
				}
				BOMCostRoll.RollCostsRetry(rollsettings.Current);
				BomCostRecs.Select();
				BomCostRecs.AskExt();
			}
			return adapter.Get();
		}

		#endregion

		#region BOM Item Events

		protected virtual bool HasAnotherRevision(AMBomItem bomItem)
		{
			var otherBomRev = (AMBomItem)PXSelect<
				AMBomItem,
				Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
					And<AMBomItem.revisionID, NotEqual<Required<AMBomItem.revisionID>>>>>
				.Select(this, bomItem?.BOMID, bomItem?.RevisionID);
			return otherBomRev != null;
		}

		protected virtual void _(Events.RowDeleting<AMBomItem> e)
		{
			//Other areas where both bomid and revision are required should have the PXForeignReference attribute attached.
			//  The problem here - we want to first see if this is the only BOM rev for this BOMID and if so check to see if its on records where bom rev is optional

			if (e.Row?.BOMID == null || HasAnotherRevision(e.Row))
			{
				return;
			}

			var bomMatl = (AMBomMatl)PXSelect<
				AMBomMatl,
				Where<AMBomMatl.compBOMID, Equal<Required<AMBomMatl.compBOMID>>>>
				.SelectWindowed(this, 0, 1, e.Row.BOMID);

			if (bomMatl != null)
			{
				throw new PXException($"{Common.Cache.GetCacheName(typeof(AMBomItem))} cannot be deleted. BOM {e.Row.BOMID} is referenced in {PXUIFieldAttribute.GetDisplayName<AMBomMatl.compBOMID>(BomMatlRecords.Cache)} on {Common.Cache.GetCacheName(typeof(AMBomMatl))} ({bomMatl.BOMID}, {bomMatl.RevisionID}, {bomMatl.OperationID}, {bomMatl.LineID})");
			}

			var prodMatl = (AMProdMatl)PXSelect<
					AMProdMatl,
					Where<AMProdMatl.compBOMID, Equal<Required<AMProdMatl.compBOMID>>>>
				.SelectWindowed(this, 0, 1, e.Row.BOMID);

			if (prodMatl != null)
			{
				throw new PXException($"{Common.Cache.GetCacheName(typeof(AMBomItem))} cannot be deleted. BOM {e.Row.BOMID} is referenced in {PXUIFieldAttribute.GetDisplayName<AMProdMatl.compBOMID>(BomMatlRecords.Cache)} on {Common.Cache.GetCacheName(typeof(AMProdMatl))} ({prodMatl.OrderType}, {prodMatl.ProdOrdID}, {prodMatl.OperationID}, {prodMatl.LineID})");
			}
		}

		protected virtual void _(Events.RowDeleted<AMBomItem> e)
		{
			if (e.Row?.BOMID == null || HasAnotherRevision(e.Row))
			{
				return;
			}

			try
			{
#pragma warning disable PX1043 // Changes cannot be saved to the database from the event handler
				RemovePrimeBomID(e.Row);
#pragma warning restore PX1043 // Changes cannot be saved to the database from the event handler
			}
			catch (Exception exception)
			{
				PXTrace.WriteError(exception);
			}
		}

		protected virtual void _(Events.RowSelected<AMBomItem> e)
		{
			if (e.Row == null)
			{
				return;
			}

			EnableBomItemFields(e.Cache, e.Row);
			BOMRowEnableChildCache(e.Cache, e.Row);
			EnableButtons(!e.Cache.IsRowInserted(e.Row));

			MakeDefaultBomAction.SetEnabled(e.Row.Status == AMBomStatus.Active);
			MakePlanningBomAction.SetEnabled(e.Row.Status == AMBomStatus.Active);
		}

		protected virtual void BOMRowEnableChildCache(PXCache cache, AMBomItem bomItem)
		{
			if (bomItem == null)
			{
				return;
			}

			var isNewBom = cache.IsRowInserted(bomItem);
			var eccAllowEdit = ECRAllowEdit(bomItem.BOMID);
			var isApi = IsImport || IsContractBasedAPI;
			var hasRequiredFields = bomItem.InventoryID != null;
			var bomAllowsEdit = bomItem.Hold.GetValueOrDefault() || isNewBom;
			EnableOperCache(isApi || (bomAllowsEdit && eccAllowEdit && hasRequiredFields));
			EnableOperChildCache(isApi || (bomAllowsEdit && eccAllowEdit && hasRequiredFields));
		}

		protected virtual void _(Events.RowPersisting<AMBomItem> e)
		{
			if (e.Row == null)
			{
				return;
			}

			if (!Common.Dates.StartBeforeEnd(e.Row.EffStartDate, e.Row.EffEndDate))
			{
				e.Cache.RaiseExceptionHandling<AMBomItem.effEndDate>(e.Row, e.Row.EffStartDate,
					new PXSetPropertyException(AM.Messages.MustBeGreaterThanOrEqualTo,
						PXUIFieldAttribute.GetDisplayName<AMBomItem.effEndDate>(Documents.Cache),
						PXUIFieldAttribute.GetDisplayName<AMBomItem.effStartDate>(Documents.Cache)));
			}
		}

		protected virtual void EnableButtons(bool enable)
		{
			ArchiveBom.SetEnabled(enable);
			AMCopyBom.SetEnabled(enable);
			AMBomCostSettings.SetEnabled(enable);
			ReportBOMSummary.SetEnabled(enable);
			ReportMultiLevel.SetEnabled(enable);
			Attributes.SetEnabled(enable);
			CreateECR.SetEnabled(enable);
			BOMCompare.SetEnabled(enable);
		}

		protected virtual void EnableOperCache(bool enabled)
		{
			BomOperRecords.AllowInsert = enabled;
			BomOperRecords.AllowUpdate = enabled;
			BomOperRecords.AllowDelete = enabled;

			OutsideProcessingOperationSelected.AllowInsert = enabled;
			OutsideProcessingOperationSelected.AllowUpdate = enabled;
			OutsideProcessingOperationSelected.AllowDelete = enabled;
		}

		protected virtual void EnableOperChildCache(bool enabled)
		{
			BomMatlRecords.AllowInsert = enabled;
			BomMatlRecords.AllowUpdate = enabled;
			BomMatlRecords.AllowDelete = enabled;

			BomStepRecords.AllowInsert = enabled;
			BomStepRecords.AllowUpdate = enabled;
			BomStepRecords.AllowDelete = enabled;

			BomOvhdRecords.AllowInsert = enabled;
			BomOvhdRecords.AllowUpdate = enabled;
			BomOvhdRecords.AllowDelete = enabled;

			BomToolRecords.AllowInsert = enabled;
			BomToolRecords.AllowUpdate = enabled;
			BomToolRecords.AllowDelete = enabled;

			BomRefRecords.AllowInsert = enabled;
			BomRefRecords.AllowUpdate = enabled;
			BomRefRecords.AllowDelete = enabled;
		}

		protected virtual void _(Events.FieldUpdated<AMBomItem, AMBomItem.siteID> e)
		{
			DefaultBomLevelsFilter.Cache.SetDefaultExt<DefaultBomLevels.warehouse>(DefaultBomLevelsFilter.Current);
		}

		#endregion

		#region BOM Oper Processes

		protected virtual void _(Events.FieldDefaulting<AMBomOper, AMBomOper.wcID> e)
		{
			var newWcID = (string)e.NewValue;
			if(newWcID == null || newWcID == e.Row?.WcID)
			{
				return;
			}

			SetWorkCenterFields(e.Cache, e.Row, newWcID);
		}

		protected virtual void _(Events.FieldUpdated<AMBomOper, AMBomOper.wcID> e)
		{
			SetWorkCenterFields(e.Cache, e.Row, e.Row.WcID);
		}

		protected virtual void SetWorkCenterFields(PXCache cache, AMBomOper bomOper, string wcID)
		{
			if (bomOper == null || CopyingBomScope.IsActive)
			{
				return;
			}

			var amWC = AMWC.PK.Find(this, wcID);
			if (amWC == null)
			{
				return;
			}

			var isInsert = cache.GetStatus(bomOper) == PXEntryStatus.Inserted;
			if (string.IsNullOrWhiteSpace(bomOper.Descr) || isInsert)
			{
				cache.SetValueExt<AMBomOper.descr>(bomOper, amWC.Descr);
			}

			if (!bomOper.BFlush.GetValueOrDefault() || isInsert)
			{
				cache.SetValueExt<AMBomOper.bFlush>(bomOper, amWC.BflushLbr.GetValueOrDefault());
			}

			cache.SetValueExt<AMBomOper.scrapAction>(bomOper, amWC.ScrapAction);
			cache.SetValueExt<AMBomOper.outsideProcess>(bomOper, amWC.OutsideFlg.GetValueOrDefault());
		}

		protected virtual void _(Events.RowSelected<AMBomOper> e)
		{
			if (IsImport || IsContractBasedAPI)
			{
				return;
			}

			var isOutsideProcess = e?.Row?.OutsideProcess == true;
			PXUIFieldAttribute.SetEnabled<AMBomOper.dropShippedToVendor>(e.Cache, e.Row, isOutsideProcess && BomOperRecords.AllowUpdate);
			PXUIFieldAttribute.SetEnabled<AMBomOperCury.vendorID>(e.Cache, e.Row, isOutsideProcess && BomOperRecords.AllowUpdate);
			PXUIFieldAttribute.SetEnabled<AMBomOperCury.vendorLocationID>(e.Cache, e.Row, isOutsideProcess && BomOperRecords.AllowUpdate);
		}

		protected virtual void _(Events.RowDeleting<AMBomOper> e)
		{
			if (e.Row == null || Documents.Cache.IsCurrentRowDeleted())
			{
				return;
			}

			var configResults = GetActiveConfigurationForOperation(e.Row);
			if (configResults != null)
			{
				var configOption = (AMConfigurationOption)configResults;
				var configFeature = (AMConfigurationFeature)configResults;
				if (configOption != null
					&& configFeature != null
					&& !string.IsNullOrWhiteSpace(configFeature.ConfigurationID)
					&& !string.IsNullOrWhiteSpace(configFeature.Revision))
				{
					e.Cancel = true;
					throw new PXException(Messages.BomOperNbrOnConfiguration,
						e.Row.BOMID.TrimIfNotNullEmpty(),
						e.Row.OperationCD.TrimIfNotNullEmpty(),
						configFeature.ConfigurationID.TrimIfNotNullEmpty(),
						configFeature.Revision.TrimIfNotNullEmpty(),
						configFeature.Label.TrimIfNotNullEmpty(),
						e.Row.RevisionID.TrimIfNotNullEmpty());
				}
			}

			AMBomAttribute bomOperAttribute = PXSelect<AMBomAttribute,
				Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
				And<AMBomAttribute.revisionID, Equal<Required<AMBomAttribute.revisionID>>,
				And<AMBomAttribute.operationID, Equal<Required<AMBomAttribute.operationID>>>>
				>>.Select(this, e.Row.BOMID, e.Row.RevisionID, e.Row.OperationID);

			if (bomOperAttribute != null)
			{
				e.Cancel |= BomOperRecords.Ask(Messages.ConfirmDeleteTitle,
								Messages.GetLocal(Messages.ConfirmOperationDeleteWhenAttributesExist),
								MessageButtons.YesNo) != WebDialogResult.Yes;
			}

			if (e.Cancel)
			{
				return;
			}

			DeleteBomOperationAttributes(e.Row);
		}

		protected virtual void DeleteBomOperationAttributes(AMBomOper row)
		{
			foreach (AMBomAttribute bomOperAttribute in PXSelect<AMBomAttribute,
				Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
					And<AMBomAttribute.revisionID, Equal<Required<AMBomAttribute.revisionID>>,
					And<AMBomAttribute.operationID, Equal<Required<AMBomAttribute.operationID>>
					>>>>.Select(this, row.BOMID, row.RevisionID, row.OperationID))
			{
				BomAttributes.Delete(bomOperAttribute);
			}
		}

		protected virtual PXResult<AMConfigurationOption, AMConfigurationFeature, AMConfiguration> GetActiveConfigurationForOperation(AMBomOper amBomOper)
		{
			if (amBomOper?.OperationID == null)
			{
				return null;
			}

			return (PXResult<AMConfigurationOption, AMConfigurationFeature, AMConfiguration>)PXSelectJoin<AMConfigurationOption,
			   InnerJoin<AMConfigurationFeature,
				   On<AMConfigurationOption.configurationID, Equal<AMConfigurationFeature.configurationID>,
					   And<AMConfigurationOption.revision, Equal<AMConfigurationFeature.revision>,
					   And<AMConfigurationOption.configFeatureLineNbr, Equal<AMConfigurationFeature.lineNbr>>>>,
			   InnerJoin<AMConfiguration,
				   On<AMConfigurationOption.configurationID, Equal<AMConfiguration.configurationID>,
					   And<AMConfigurationOption.revision, Equal<AMConfiguration.revision>>>>>,
				   Where<AMConfiguration.status, NotEqual<ConfigRevisionStatus.inactive>,
					   And<AMConfiguration.bOMID, Equal<Required<AMConfiguration.bOMID>>,
					   And<AMConfiguration.bOMRevisionID, Equal<Required<AMConfiguration.bOMRevisionID>>,
					   And<AMConfigurationOption.operationID, Equal<Required<AMConfigurationOption.operationID>>>>>>
					   >.SelectWindowed(this, 0, 1, amBomOper.BOMID, amBomOper.RevisionID, amBomOper.OperationID);
		}

		#endregion

		#region BOM Matl Processes

		protected virtual void CompBOMIDFieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void _(Events.RowSelected<AMBomMatl> e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<AMBomMatl.subItemID>(e.Cache, e.Row, e.Row.IsStockItem.GetValueOrDefault());
			PXUIFieldAttribute.SetEnabled<AMBomMatl.subcontractSource>(e.Cache, e.Row, e.Row.MaterialType == AMMaterialType.Subcontract);

			if (IsImport || IsContractBasedAPI)
			{
				return;
			}

			var isMatlExpired = e.Row.ExpDate > Common.Current.BusinessDate(this) || Common.Dates.IsDateNull(e.Row.ExpDate);
			if (!isMatlExpired)
			{
				e.Cache.RaiseExceptionHandling<AMBomMatl.inventoryID>(e.Row, e.Row.InventoryID,
					new PXSetPropertyException(Messages.MaterialExpiredOnBom, PXErrorLevel.Warning, e.Row.BOMID, e.Row.RevisionID));
			}
		}

		protected virtual void _(Events.RowPersisting<AMBomMatl> e)
		{
			if (e.Row == null)
			{
				return;
			}

			var subItemFeatureEnabled = InventoryHelper.SubItemFeatureEnabled;

			// Require SUBITEMID when the item is a stock item
			if (subItemFeatureEnabled && e.Row.InventoryID != null && e.Row.IsStockItem.GetValueOrDefault() && e.Row.SubItemID == null)
			{
				e.Cache.RaiseExceptionHandling<AMBomMatl.subItemID>(
						e.Row,
						e.Row.SubItemID,
						new PXSetPropertyException(Messages.SubItemIDRequiredForStockItem, PXErrorLevel.Error));
			}

			//  PREVENT A USER FROM ADDING THE MATERIAL ITEM TO ITSELF
			//      More in depth prevention can be added down the road
			if (Documents.Current != null && e.Row.InventoryID.GetValueOrDefault() != 0)
			{
				if (e.Row.InventoryID == Documents.Current.InventoryID)
				{
					if (subItemFeatureEnabled
						&& e.Row.IsStockItem.GetValueOrDefault()
						&& Documents.Current.SubItemID != null
						&& e.Row.SubItemID.GetValueOrDefault() != Documents.Current.SubItemID.GetValueOrDefault())
					{
						//this should allow different sub items to be consumed on the same BOM as the item being built
						return;
					}

					e.Cache.RaiseExceptionHandling<AMBomMatl.inventoryID>(
						e.Row,
						e.Row.InventoryID,
						new PXSetPropertyException(Messages.BomMatlCircularRefAttempt, PXErrorLevel.Error));
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<AMBomMatl, AMBomMatl.subItemID> e)
		{
			if (e.Row == null || Documents.Current == null
				|| e.NewValue == null || e.Row.InventoryID == null
				|| !InventoryHelper.SubItemFeatureEnabled)
			{
				return;
			}

			int? subItemID = Convert.ToInt32(e.NewValue ?? 0);
			if (e.Row.InventoryID == Documents.Current.InventoryID
				&& (Documents.Current.SubItemID == null
				|| Documents.Current.SubItemID.GetValueOrDefault() == subItemID))
			{
				e.NewValue = null;
				e.Cancel = true;
				throw new PXSetPropertyException(Messages.BomMatlCircularRefAttempt, PXErrorLevel.Error);
			}

			var item = InventoryItem.PK.Find(this, e.Row.InventoryID);
			if (item == null)
			{
				return;
			}
			CheckDuplicateEntry(e.Args, e.Row, item, subItemID);
		}

		protected virtual void _(Events.FieldVerifying<AMBomMatl, AMBomMatl.inventoryID> e)
		{
			if (e.Row == null || Documents.Current == null
				|| e.NewValue == null || InventoryHelper.SubItemFeatureEnabled)
			{
				return;
			}

			var inventoryID = Convert.ToInt32(e.NewValue);
			var item = InventoryItem.PK.Find(this, inventoryID);
			if (item == null)
			{
				return;
			}

			//  PREVENT A USER FROM ADDING THE MATERIAL ITEM TO ITSELF
			if (inventoryID == Documents.Current.InventoryID)
			{
				e.NewValue = item.InventoryCD;
				e.Cancel = true;
				throw new PXSetPropertyException(Messages.BomMatlCircularRefAttempt, PXErrorLevel.Error);
			}
			CheckDuplicateEntry(e.Args, e.Row, item, e.Row.SubItemID);
		}

		protected virtual void _(Events.FieldUpdated<AMBomMatl, AMBomMatl.inventoryID> e)
		{
			if (e.Row == null)
			{
				return;
			}

			if (Documents.Current != null && e.Row.InventoryID.GetValueOrDefault() != 0)
			{
				e.Cache.SetDefaultExt<AMBomMatl.descr>(e.Row);
				e.Cache.SetDefaultExt<AMBomMatl.subItemID>(e.Row);
				e.Cache.SetDefaultExt<AMBomMatl.uOM>(e.Row);
				e.Cache.SetDefaultExt<AMBomMatl.unitCost>(e.Row);
			}
		}

		protected virtual void DefaultUnitCost(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object matlUnitCost;
			sender.RaiseFieldDefaulting<AMBomMatl.unitCost>(e.Row, out matlUnitCost);

			if (matlUnitCost == null || (decimal)(matlUnitCost ?? 0m) == 0m)
			{
				return;
			}

			sender.SetValueExt<AMBomMatl.unitCost>(e.Row, matlUnitCost);
		}

		protected virtual void _(Events.FieldUpdated<AMBomMatl, AMBomMatl.uOM> e)
		{
			DefaultUnitCost(e.Cache, e.Args);
		}

		/// <summary>
		/// Checks for duplicate item in a BOM
		/// </summary>
		/// <param name="e">Calling Field Verifying event args</param>
		/// <param name="matlRow">source material row to check against</param>
		/// <param name="inventoryItem">Inventory item row of newly entered inventory ID (from field verifying)</param>
		/// <returns>True if the row can be added, false otherwise</returns>
		protected virtual void CheckDuplicateEntry(PXFieldVerifyingEventArgs e, AMBomMatl matlRow, InventoryItem inventoryItem)
		{
			CheckDuplicateEntry(e, matlRow, inventoryItem, null);
		}

		/// <summary>
		/// Checks for duplicate item in a BOM
		/// </summary>
		/// <param name="e">Calling Field Verifying event args</param>
		/// <param name="matlRow">source material row to check against</param>
		/// <param name="inventoryItem">Inventory item row of newly entered inventory ID (from field verifying)</param>
		/// <param name="subItemID">SUbItemID</param>
		/// <returns>True if the row can be added, false otherwise</returns>
		protected virtual void CheckDuplicateEntry(PXFieldVerifyingEventArgs e, AMBomMatl matlRow, InventoryItem inventoryItem, int? subItemID)
		{
			AMDebug.TraceWriteMethodName();

			if (matlRow == null || this.ambsetup.Current == null || inventoryItem == null)
			{
				return;
			}

			AMBSetup bomSetup = this.ambsetup.Current;

			//If pages running as import treat warnings the same as allow
			if ((IsImport || CopyingBomScope.IsActive) && bomSetup.DupInvBOM.Trim() == SetupMessage.WarningMsg)
			{
				bomSetup.DupInvBOM = SetupMessage.AllowMsg;
			}
			if ((IsImport || CopyingBomScope.IsActive) && bomSetup.DupInvOper.Trim() == SetupMessage.WarningMsg)
			{
				bomSetup.DupInvOper = SetupMessage.AllowMsg;
			}

			if (bomSetup.DupInvBOM.Trim() == SetupMessage.AllowMsg
				&& bomSetup.DupInvOper.Trim() == SetupMessage.AllowMsg)
			{
				// both allow = nothing to validate
				return;
			}

			AMBomMatl dupBomMatl = null;
			AMBomMatl dupOperMatl = null;

			foreach (AMBomMatl duplicateAMBomMatl in PXSelect<AMBomMatl,
				Where<AMBomMatl.bOMID, Equal<Required<AMBomMatl.bOMID>>,
					And<AMBomMatl.revisionID, Equal<Required<AMBomMatl.revisionID>>,
					And<AMBomMatl.inventoryID, Equal<Required<AMBomMatl.inventoryID>>
					>>>>.Select(this, matlRow.BOMID, matlRow.RevisionID, inventoryItem.InventoryID))
			{
				if (subItemID != null && duplicateAMBomMatl.SubItemID.GetValueOrDefault() != subItemID.GetValueOrDefault() && InventoryHelper.SubItemFeatureEnabled)
				{
					continue;
				}
				if (duplicateAMBomMatl.OperationID.Equals(matlRow.OperationID) && duplicateAMBomMatl.LineID != matlRow.LineID && dupOperMatl == null)
				{
					dupOperMatl = duplicateAMBomMatl;
				}

				if (!duplicateAMBomMatl.OperationID.Equals(matlRow.OperationID) && dupBomMatl == null)
				{
					dupBomMatl = duplicateAMBomMatl;
				}

				if (dupOperMatl != null && dupBomMatl != null)
				{
					break;
				}
			}

			var skipBomCheck = false;
			if (dupOperMatl != null && bomSetup.DupInvOper.Trim() != SetupMessage.AllowMsg)
			{
				DuplicateEntryMessage(e, dupOperMatl, inventoryItem, bomSetup.DupInvOper.Trim());
				skipBomCheck = true;
			}

			if (dupBomMatl != null && !skipBomCheck && bomSetup.DupInvBOM.Trim() != SetupMessage.AllowMsg)
			{
				DuplicateEntryMessage(e, dupBomMatl, inventoryItem, bomSetup.DupInvBOM.Trim());
			}
		}

		/// <summary>
		/// Builds and creates the warning/error message related to duplicates items on a BOM
		/// </summary>
		/// <param name="e">Calling Field Verifying event args</param>
		/// <param name="duplicateAMBomMatl">The found duplicate AMBomMatl row</param>
		/// <param name="inventoryItem">Inventory item row of newly entered inventory ID (from field verifying)</param>
		/// <param name="setupCheck">BOM Setup duplicate setup option indicating warning or error</param>
		protected virtual void DuplicateEntryMessage(PXFieldVerifyingEventArgs e, AMBomMatl duplicateAMBomMatl, InventoryItem inventoryItem, string setupCheck)
		{
			if (duplicateAMBomMatl == null ||
				duplicateAMBomMatl.InventoryID == null ||
				inventoryItem == null ||
				string.IsNullOrWhiteSpace(setupCheck))
			{
				return;
			}

			var operBomValue = (AMBomOper)PXSelect<AMBomOper, Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
										And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>,
										And<AMBomOper.operationID, Equal<Required<AMBomOper.operationID>>>>>>
										.Select(this, duplicateAMBomMatl.BOMID, duplicateAMBomMatl.RevisionID, duplicateAMBomMatl.OperationID);

			var userMessage = Messages.GetLocal(Messages.BomMatlDupItems, operBomValue?.OperationCD, operBomValue?.BOMID, operBomValue?.RevisionID);

			switch (setupCheck)
			{
				case SetupMessage.WarningMsg:
					WebDialogResult response = BomMatlRecords.Ask(
						Messages.Warning,
						$"{userMessage} {Messages.GetLocal(Messages.Continue)}?",
						MessageButtons.YesNo);

					if (response != WebDialogResult.Yes)
					{
						e.NewValue = inventoryItem.InventoryCD;
						e.Cancel = true;
						throw new PXSetPropertyException(userMessage, PXErrorLevel.Error);
					}
					break;
				case SetupMessage.ErrorMsg:
					e.NewValue = inventoryItem.InventoryCD;
					e.Cancel = true;
					throw new PXSetPropertyException(userMessage, PXErrorLevel.Error);
			}
		}

		protected virtual void _(Events.FieldVerifying<AMBomMatl, AMBomMatl.subcontractSource> e)
		{
			var newContractSource = (int)e.NewValue;
			if (e.Row?.MaterialType == null)
			{
				return;
			}

			if (e.Row.MaterialType != AMMaterialType.Subcontract && newContractSource != AMSubcontractSource.None)
			{
				e.NewValue = AMSubcontractSource.None;
				e.Cancel = true;
				return;
			}

			if (e.Row.MaterialType == AMMaterialType.Subcontract && newContractSource == AMSubcontractSource.None)
			{
				e.NewValue = e.Row.SubcontractSource;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldUpdated<AMBomOper, AMBomOper.outsideProcess> e)
		{
			var oldValue = Convert.ToBoolean(e.OldValue ?? false);
			if (!oldValue || e.Row?.OutsideProcess == true)
			{
				return;
			}

			e.Cache.SetValueExt<AMBomOper.dropShippedToVendor>(e.Row, false);
			e.Cache.SetValueExt<AMBomOperCury.vendorID>(e.Row, null);
			e.Cache.SetValueExt<AMBomOperCury.vendorLocationID>(e.Row, null);

			foreach (AMBomMatl matl in BomMatlRecords.Select())
			{
				BomMatlRecords.Cache.SetValueExt<AMBomMatl.materialType>(matl, AMMaterialType.Regular);
			}
		}

		protected virtual void _(Events.FieldUpdated<AMBomMatl, AMBomMatl.materialType> e)
		{
			if (e.Row?.MaterialType == null)
			{
				return;
			}

			if (e.Row.MaterialType != AMMaterialType.Subcontract)
			{
				e.Cache.SetValueExt<AMBomMatl.subcontractSource>(e.Row, AMSubcontractSource.None);
				return;
			}

			e.Cache.SetValueExt<AMBomMatl.subcontractSource>(e.Row, AMSubcontractSource.Purchase);
		}

		#endregion

		#region Get/Set Primary BOMID

		protected virtual void SetPrimeBomID(AMBomItem bomItem)
		{
			if (bomItem?.RevisionID == null)
			{
				return;
			}

			new PrimaryBomIDManager(this).SetAllFirstOnlyPrimary(bomItem);
		}

		protected virtual void RemovePrimeBomID(AMBomItem bomItem)
		{
			if (bomItem?.RevisionID == null)
			{
				return;
			}

			new PrimaryBomIDManager(this)
			{
				PersistChanges = false
			}.RemovePrimary(bomItem);
		}

		#endregion

		protected virtual void _(Events.RowSelected<CopyBomFilter> e)
		{
			//set fields enabled status
			PXUIFieldAttribute.SetEnabled<CopyBomFilter.fromBOMID>(e.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CopyBomFilter.fromRevisionID>(e.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CopyBomFilter.fromInventoryID>(e.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CopyBomFilter.fromSubItemID>(e.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CopyBomFilter.fromSiteID>(e.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CopyBomFilter.toBOMID>(e.Cache, null, BomNumbering?.Current?.UserNumbering == true);
			PXUIFieldAttribute.SetVisible<CopyBomFilter.fromSiteID>(e.Cache, null, !MBCEnabled);
			PXUIFieldAttribute.SetVisible<CopyBomFilter.toSiteID>(e.Cache, null, !MBCEnabled);
			PXUIFieldAttribute.SetVisible<CopyBomFilter.updateMaterialWarehouse>(e.Cache, null, InventoryHelper.MultiWarehousesFeatureEnabled);
		}

		protected virtual void _(Events.RowSelected<DefaultBomLevels> e)
		{
			PXUIFieldAttribute.SetEnabled<DefaultBomLevels.warehouse>(e.Cache, null, Documents?.Current?.SiteID != null);
		}

		protected virtual void EnableBomItemFields(PXCache cache, AMBomItem item)
		{
			//  Handle enable status of inventory id and site id for a bom already created
			//      to prevent a user from changing accidentally

			var enableItemChange = cache.IsRowInserted(item);

			PXUIFieldAttribute.SetEnabled<AMBomItem.inventoryID>(cache, null, enableItemChange);
			PXUIFieldAttribute.SetEnabled<AMBomItem.subItemID>(cache, null, enableItemChange);
			PXUIFieldAttribute.SetEnabled<AMBomItem.siteID>(cache, null, enableItemChange);
			PXUIFieldAttribute.SetEnabled<AMBomItem.descr>(cache, null, enableItemChange || item.Hold.GetValueOrDefault() || IsImport || IsContractBasedAPI);
			PXUIFieldAttribute.SetEnabled<AMBomItem.effStartDate>(cache, null, enableItemChange || item.Hold.GetValueOrDefault() || IsImport || IsContractBasedAPI);
			PXUIFieldAttribute.SetEnabled<AMBomItem.effEndDate>(cache, null, enableItemChange || item.Hold.GetValueOrDefault() || IsImport || IsContractBasedAPI);
		}

		/// <summary>
		/// Create an INItemSite record if one doesn't exist for the bom item/site
		/// </summary>
		/// <param name="amBomItem">BOM containing item/site ids</param>
		protected virtual void DefaultItemSiteByBOM(AMBomItem amBomItem)
		{
			if (amBomItem?.InventoryID == null
				|| amBomItem.SiteID == null)
			{
				return;
			}

			DefaultItemSite(amBomItem.InventoryID, amBomItem.SiteID);
		}

		/// <summary>
		/// Create an INItemSite record if one doesn't exist for the bom item/site
		/// </summary>
		protected virtual void DefaultItemSite(int? inventoryID, int? siteID)
		{
			if (inventoryID == null || siteID == null || !InventoryHelper.MultiWarehousesFeatureEnabled)
			{
				return;
			}

			INItemSite inItemSite = null;
			if (InventoryHelper.MakeItemSiteByItem(this, inventoryID, siteID, out inItemSite))
			{
				INItemSite itemSite = ItemSiteRecord.Locate(inItemSite);
				if (itemSite == null)
				{
					ItemSiteRecord.Insert(inItemSite);
				}
			}
		}

		/// <summary>
		/// Insert INItemSite records based on inserted bom item or matl records
		/// </summary>
		protected virtual void InsertMissingINItemSite()
		{
			foreach (AMBomItem amBomItem in this.Documents.Cache.Inserted)
			{
				DefaultItemSiteByBOM(amBomItem);
			}

			foreach (AMBomMatl amBomMatl in this.BomMatlRecords.Cache.Inserted)
			{
				var matlSiteID = amBomMatl.SiteID;
				if (matlSiteID == null)
				{
					foreach (AMBomItem amBomItem in this.Documents.Cache.Cached.Cast<AMBomItem>().Where(amBomItem => amBomItem.BOMID == amBomMatl.BOMID && amBomItem.RevisionID == amBomMatl.RevisionID))
					{
						matlSiteID = amBomItem.SiteID;
					}
				}

				DefaultItemSite(amBomMatl.InventoryID, matlSiteID);
			}
		}

		public override bool CanCreateNewRevision(TGraph fromGraph, TGraph toGraph, string keyValue,
			string revisionValue, out string error)
		{
#if DEBUG
			AMDebug.TraceWriteMethodName($"key '{keyValue}' rev '{revisionValue}'");
#endif
			if (Features.ECCEnabled() && ambsetup.Current?.ForceECR == true)
			{
				error = Messages.GetLocal(Messages.ECRRequired);
				return false;
			}

			// Always returns true as new revisions can be created at any time
			error = string.Empty;
			return true;
		}

		public class CopyingBomScope : FlaggedModeScopeBase<CopyingBomScope> { }
		public override void CopyRevision(TGraph fromGraph, TGraph toGraph, string keyValue, string revisionValue)
		{
			if (toGraph?.Documents?.Current == null || fromGraph?.Documents?.Current == null)
			{
				//api calls should create new revs on their own - this causes issues when calling from api so we need to turn the copy rev logic off
				return;
			}

			toGraph.Documents.Cache.SetDefaultExt<AMBomItem.hold>(toGraph.Documents.Current);
			toGraph.Documents.Cache.SetDefaultExt<AMBomItem.status>(toGraph.Documents.Current);
			toGraph.Documents.Cache.SetDefaultExt<AMBomItem.effStartDate>(toGraph.Documents.Current);
			toGraph.Documents.Cache.SetDefaultExt<AMBomItem.effEndDate>(toGraph.Documents.Current);
			toGraph.Documents.Current.NoteID = null;

			PXNoteAttribute.CopyNoteAndFiles(fromGraph.Documents.Cache, fromGraph.Documents.Current,
				toGraph.Documents.Cache, toGraph.Documents.Current, true, true);

			if (SkipAutoCreateNewRevision())
			{
				return;
			}

			PXTrace.WriteInformation($"Copy BOM {fromGraph.Documents.Current?.BOMID} revision {fromGraph.Documents.Current?.RevisionID} to BOM {keyValue} revision {revisionValue}");
			using(new CopyingBomScope())
			{
				CreateNewRevision(toGraph, fromGraph.Documents.Current, keyValue, revisionValue);
			}
		}

		public virtual void CreateNewRevision(TGraph toGraph, AMBomItem sourceBOM, string newBOMID, string newRevisionID)
		{
			PXTrace.WriteInformation($"Creating new BOM {newBOMID} revision {newRevisionID}");
#if DEBUG
			var sw = new System.Diagnostics.Stopwatch();
			var sb = new System.Text.StringBuilder();
			sb.AppendLine($"Creating new BOM {newBOMID} revision {newRevisionID}");
#endif
			try
			{
				FieldVerifying.AddHandler<AMBomMatl.compBOMID>(CompBOMIDFieldVerifying);

				// prevent update from here
				copyBomFilter.Current.UpdateMaterialWarehouse = false;

#if DEBUG
				sw.Start();
#endif
				CopyBomOper(sourceBOM, newBOMID, newRevisionID, true);
#if DEBUG
				var lastElapsed = sw.Elapsed;
				sb.AppendLine(PXTraceHelper.CreateTimespanMessage(lastElapsed, "CopyBomOper"));
#endif
				CopyBomMatl(sourceBOM, newBOMID, newRevisionID, true);
#if DEBUG
				var currElapsed = sw.Elapsed;
				sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, "CopyBomMatl"));
				lastElapsed = currElapsed;
#endif
				CopyBomStep(sourceBOM, newBOMID, newRevisionID, true);
#if DEBUG
				currElapsed = sw.Elapsed;
				sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, "CopyBomStep"));
				lastElapsed = currElapsed;
#endif
				CopyBomRef(sourceBOM, newBOMID, newRevisionID);
#if DEBUG
				currElapsed = sw.Elapsed;
				sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, "CopyBomRef"));
				lastElapsed = currElapsed;
#endif
				CopyBomTool(sourceBOM, newBOMID, newRevisionID, true);
#if DEBUG
				currElapsed = sw.Elapsed;
				sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, "CopyBomTool"));
				lastElapsed = currElapsed;
#endif
				CopyBomOvhd(sourceBOM, newBOMID, newRevisionID, true);
#if DEBUG
				currElapsed = sw.Elapsed;
				sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, "CopyBomOvhd"));
				lastElapsed = currElapsed;
#endif
				CopyBomAttributes(sourceBOM, newBOMID, newRevisionID);
#if DEBUG
				currElapsed = sw.Elapsed;
				sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, "CopyBomAttributes"));
				lastElapsed = currElapsed;
#endif

				// Set the primary bom ID if none currently set
				new PrimaryBomIDManager(this).SetAllFirstOnlyPrimary(
					newBOMID,
					sourceBOM.InventoryID,
					sourceBOM.SiteID,
					sourceBOM.SubItemID);
#if DEBUG
				currElapsed = sw.Elapsed;
				sb.AppendLine(PXTraceHelper.CreateTimespanMessage(currElapsed - lastElapsed, $"PrimaryBomIDManager.SetAllFirstOnlyPrimary({newBOMID}, {sourceBOM.InventoryID}, {sourceBOM.SiteID}, {sourceBOM.SubItemID})"));
#endif
			}
			finally
			{
#if DEBUG
				sw.Stop();
				sb.AppendLine(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, $"Total CopyRevision Process Time / ElapsedTicks = {sw.ElapsedTicks}"));
				PXTraceHelper.WriteInformation(sb.ToString());
#endif
				FieldVerifying.RemoveHandler<AMBomMatl.compBOMID>(CompBOMIDFieldVerifying);
			}
		}

		public virtual void CopyBom(AMBomItem sourceBOM, CopyBomFilter copyFilter)
		{
			Clear(PXClearOption.PreserveTimeStamp);

			copyBomFilter.Current = copyFilter;

			if (sourceBOM == null)
			{
				throw new ArgumentNullException(nameof(sourceBOM));
			}

			if (copyFilter == null)
			{
				throw new ArgumentNullException(nameof(copyFilter));
			}

			var manualNumbering = BomNumbering?.Current?.UserNumbering ?? false;
			var newBomId = manualNumbering ? copyFilter.ToBOMID : null;

			if (manualNumbering && string.IsNullOrWhiteSpace(newBomId))
			{
				throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyBomFilter.toBOMID>(this.copyBomFilter.Cache));
			}

			if (string.IsNullOrWhiteSpace(copyFilter.ToRevisionID))
			{
				throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyBomFilter.toRevisionID>(this.copyBomFilter.Cache));
			}

			if (copyFilter.ToInventoryID == null)
			{
				throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyBomFilter.toInventoryID>(this.copyBomFilter.Cache));
			}

			if (InventoryHelper.SubItemFeatureEnabled && copyFilter.ToSubItemID == null)
			{
				throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyBomFilter.toSubItemID>(this.copyBomFilter.Cache));
			}

			PXTrace.WriteInformation($"Copy BOM {sourceBOM.BOMID} revision {sourceBOM.RevisionID} to BOM {newBomId} revision {copyFilter.ToRevisionID}");

			try
			{
				FieldVerifying.AddHandler<AMBomMatl.compBOMID>(CompBOMIDFieldVerifying);

				var targetBOM = PXCache<AMBomItem>.CreateCopy(sourceBOM);
				targetBOM.BOMID = newBomId;
				targetBOM.RevisionID = copyFilter.ToRevisionID;
				targetBOM.InventoryID = copyFilter.ToInventoryID;
				targetBOM.SubItemID = copyFilter.ToSubItemID;
				targetBOM.SiteID = copyFilter.ToSiteID;
				targetBOM.Hold = null;
				targetBOM.Status = null;
				targetBOM.NoteID = null;
				targetBOM.EffStartDate = null;
				targetBOM.EffEndDate = null;

				Documents.Cache.ForceExceptionHandling = true;
				targetBOM = Documents.Insert(targetBOM);

				if (copyFilter.CopyNotesItem.GetValueOrDefault())
				{
					PXNoteAttribute.CopyNoteAndFiles(Documents.Cache, sourceBOM, Documents.Cache, targetBOM, true, true);
				}

				targetBOM = Documents.Update(targetBOM);

				using (new DisableFormulaCalculationScope(BomOperRecords.Cache, typeof(AMBomOper.queueTime), typeof(AMBomOper.finishTime), typeof(AMBomOper.moveTime)))
				{
					CopyBomOper(sourceBOM, newBomId, copyFilter.ToRevisionID, copyFilter.CopyNotesOper.GetValueOrDefault());
				}

				CopyBomMatl(sourceBOM, newBomId, copyFilter.ToRevisionID, copyFilter.CopyNotesMatl.GetValueOrDefault());
				CopyBomStep(sourceBOM, newBomId, copyFilter.ToRevisionID, copyFilter.CopyNotesStep.GetValueOrDefault());
				CopyBomRef(sourceBOM, newBomId, copyFilter.ToRevisionID);
				CopyBomTool(sourceBOM, newBomId, copyFilter.ToRevisionID, copyFilter.CopyNotesTool.GetValueOrDefault());
				CopyBomOvhd(sourceBOM, newBomId, copyFilter.ToRevisionID, copyFilter.CopyNotesOvhd.GetValueOrDefault());
				CopyBomAttributes(sourceBOM, newBomId, copyFilter.ToRevisionID);

				// Set the primary bom ID if none currently set
				new PrimaryBomIDManager(this).SetAllFirstOnlyPrimary(
					newBomId,
					targetBOM.InventoryID,
					targetBOM.SiteID,
					targetBOM.SubItemID);
			}
			finally
			{
				FieldVerifying.RemoveHandler<AMBomMatl.compBOMID>(CompBOMIDFieldVerifying);
			}
		}

		protected virtual void CheckExistingBom(string bomId)
		{
			//check if manual BOM ID already exists
			AMBomItem existingBomItem = PXSelect<AMBomItem, Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>>>.Select(this, bomId);

			if (existingBomItem != null)
			{
				throw new PXException(Messages.GetLocal(Messages.BomExists, bomId));
			}
		}

		protected virtual void CopyBomOper(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
		{
			CopyBomOper(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID, copyNotes);
		}

		internal virtual void CopyBomOper(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
		{
			foreach (AMBomOper fromRow in PXSelect<AMBomOper,
				Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
					And<AMBomOper.revisionID, Equal<Required<AMBomItem.revisionID>>>>
			>.Select(this, sourceID, sourceRevisionID))
			{
				if (fromRow.RowStatus == AMRowStatus.Deleted)
				{
					continue;
				}

				var toRow = PXCache<AMBomOper>.CreateCopy(fromRow);
				toRow.BOMID = newBOMID;
				toRow.RevisionID = newRevisionID;
				toRow.NoteID = null;
				toRow.RowStatus = null;
				toRow = BomOperRecords.Insert(toRow);

				if (copyNotes)
				{
					PXNoteAttribute.CopyNoteAndFiles(BomOperRecords.Cache, fromRow, BomOperRecords.Cache, toRow);
					BomOperRecords.Update(toRow);
				}
			}
		}

		protected virtual void CopyBomMatl(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
		{
			CopyBomMatl(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID, copyNotes);
		}

		internal virtual void CopyBomMatl(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
		{
			foreach (PXResult<AMBomMatl, InventoryItem, AMBomItem, INItemSite> result in PXSelectJoin<
				AMBomMatl,
				InnerJoin<InventoryItem,
					On<AMBomMatl.inventoryID, Equal<InventoryItem.inventoryID>>,
				// Left Join because ECC uses this logic and no bom item
				LeftJoin<AMBomItem,
					On<AMBomMatl.bOMID, Equal<AMBomItem.bOMID>,
					And<AMBomMatl.revisionID, Equal<AMBomItem.revisionID>>>,
				LeftJoin<INItemSite,
					On<AMBomMatl.inventoryID, Equal<INItemSite.inventoryID>,
					And<AMBomItem.siteID, Equal<INItemSite.siteID>>>>>>,
				Where<AMBomMatl.bOMID, Equal<Required<AMBomMatl.bOMID>>,
					And<AMBomMatl.revisionID, Equal<Required<AMBomMatl.revisionID>>,
					And<Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
						And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>>>>,
				OrderBy<
					Asc<AMBomMatl.sortOrder,
					Asc<AMBomMatl.lineID>>>
				>
				.Select(this, sourceID, sourceRevisionID))
			{
				var fromRow = (AMBomMatl)result;
				var inventoryItem = (InventoryItem)result;

				if (fromRow.RowStatus == AMRowStatus.Deleted || inventoryItem == null ||
					fromRow.ExpDate.GetValueOrDefault(Common.Dates.BeginOfTimeDate) != Common.Dates.BeginOfTimeDate
					&& fromRow.ExpDate.GetValueOrDefault() < Accessinfo.BusinessDate.GetValueOrDefault())
				{
					//no point in copying expired material
					continue;
				}

				var toRow = PXCache<AMBomMatl>.CreateCopy(fromRow);
				toRow.BOMID = newBOMID;
				toRow.RevisionID = newRevisionID;
				toRow.NoteID = null;
				toRow.RowStatus = null;

				if (toRow.CompBOMID != null && !IsValidBom(toRow.CompBOMID, toRow.CompBOMRevisionID))
				{
					toRow.CompBOMID = null;
					toRow.CompBOMRevisionID = null;
				}

				try
				{
					toRow = BomMatlRecords.Insert(toRow);

					//copy the curysettings record if it exists
					var fromCury = AMBomMatlCury.PK.Find(this, fromRow.BOMID, fromRow.RevisionID, fromRow.OperationID, fromRow.LineID, Accessinfo.BaseCuryID);
					if (fromCury != null)
					{
						var toCury = PXCache<AMBomMatlCury>.CreateCopy(fromCury);
						toCury.BOMID = newBOMID;
						toCury.RevisionID = newRevisionID;

						if (toCury.SiteID != null && InventoryHelper.MultiWarehousesFeatureEnabled &&
							copyBomFilter.Current.UpdateMaterialWarehouse == true && toCury.SiteID != copyBomFilter.Current.ToSiteID)
						{
							toCury.SiteID = copyBomFilter.Current.ToSiteID;

							if (toCury.LocationID != null)
							{
								toCury.LocationID = null;
							}
						}

						toCury = BomMatlCurySelected.Insert(toCury);
					}

					// The result uses the bom siteid, so if material has a site id we still want to call DefaultItemSite
					var materialItemSite = (INItemSite)result;
					if ((toRow.SiteID != null || materialItemSite == null) && ((AMBomItem)result)?.RevisionID != null)
					{
						DefaultItemSite(toRow.InventoryID, toRow.SiteID ?? copyBomFilter.Current.ToSiteID);
					}

					if (copyNotes)
					{
						PXNoteAttribute.CopyNoteAndFiles(BomMatlRecords.Cache, fromRow, BomMatlRecords.Cache, toRow);
						BomMatlRecords.Update(toRow);
					}
				}
				catch (Exception exception)
				{
					PXTrace.WriteError(
						Messages.GetLocal(Messages.UnableToCopyMaterialFromToBomID),
						inventoryItem?.InventoryCD.TrimIfNotNullEmpty(),
						fromRow?.BOMID,
						fromRow?.RevisionID,
						toRow?.BOMID,
						toRow?.RevisionID,
						exception.Message);
					throw;
				}
			}
		}

		protected bool IsValidBom(string bomId, string revisionId)
		{
			if (string.IsNullOrWhiteSpace(bomId))
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(revisionId))
			{
				return (AMBomItem)PXSelect<
					AMBomItem,
					Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
						And<AMBomItem.status, NotEqual<AMBomStatus.archived>>>>
					.SelectWindowed(this, 0, 1, bomId) != null;
			}

			return (AMBomItem)PXSelect<AMBomItem,
					Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
						And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>,
						And<AMBomItem.status, NotEqual<AMBomStatus.archived>>>>>
				.SelectWindowed(this, 0, 1, bomId, revisionId) != null;
		}

		protected virtual void CopyBomStep(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
		{
			CopyBomStep(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID, copyNotes);
		}

		internal virtual void CopyBomStep(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
		{
			var fromRows = PXSelect<AMBomStep,
				Where<AMBomStep.bOMID, Equal<Required<AMBomStep.bOMID>>,
					And<AMBomStep.revisionID, Equal<Required<AMBomStep.revisionID>>
					>>>.Select(this, sourceID, sourceRevisionID);

			foreach (AMBomStep fromRow in fromRows)
			{
				if (fromRow.RowStatus == AMRowStatus.Deleted)
				{
					continue;
				}

				var toRow = PXCache<AMBomStep>.CreateCopy(fromRow);
				toRow.BOMID = newBOMID;
				toRow.RevisionID = newRevisionID;
				toRow.NoteID = null;
				toRow.RowStatus = null;
				toRow = BomStepRecords.Insert(toRow);

				if (copyNotes)
				{
					PXNoteAttribute.CopyNoteAndFiles(BomStepRecords.Cache, fromRow, BomStepRecords.Cache, toRow);
					BomStepRecords.Update(toRow);
				}
			}
		}

		protected virtual void CopyBomRef(AMBomItem sourceBOM, string newBOMID, string newRevisionID)
		{
			CopyBomRef(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID);
		}

		internal virtual void CopyBomRef(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID)
		{
			var fromRows = PXSelect<AMBomRef,
				Where<AMBomRef.bOMID, Equal<Required<AMBomRef.bOMID>>,
					And<AMBomRef.revisionID, Equal<Required<AMBomRef.revisionID>>
					>>>.Select(this, sourceID, sourceRevisionID);

			foreach (AMBomRef fromRow in fromRows)
			{
				if (fromRow.RowStatus == AMRowStatus.Deleted)
				{
					continue;
				}

				var toRow = PXCache<AMBomRef>.CreateCopy(fromRow);
				toRow.BOMID = newBOMID;
				toRow.RevisionID = newRevisionID;
				toRow.NoteID = null;
				toRow.RowStatus = null;
				BomRefRecords.Insert(toRow);
			}
		}

		protected virtual void CopyBomTool(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
		{
			CopyBomTool(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID, copyNotes);
		}

		internal virtual void CopyBomTool(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
		{
			var fromRows = PXSelectJoin<AMBomTool,
				InnerJoin<AMToolMst, On<AMBomTool.toolID, Equal<AMToolMst.toolID>>>,
				Where<AMBomTool.bOMID, Equal<Required<AMBomTool.bOMID>>,
					And<AMBomTool.revisionID, Equal<Required<AMBomTool.revisionID>>
					>>>.Select(this, sourceID, sourceRevisionID);

			foreach (AMBomTool fromRow in fromRows)
			{
				if (fromRow.RowStatus == AMRowStatus.Deleted)
				{
					continue;
				}

				var toRow = PXCache<AMBomTool>.CreateCopy(fromRow);
				toRow.BOMID = newBOMID;
				toRow.RevisionID = newRevisionID;
				toRow.NoteID = null;
				toRow.RowStatus = null;
				toRow = BomToolRecords.Insert(toRow);

				if (copyNotes)
				{
					PXNoteAttribute.CopyNoteAndFiles(BomToolRecords.Cache, fromRow, BomToolRecords.Cache, toRow);
					BomToolRecords.Update(toRow);
				}
			}
		}

		protected virtual void CopyBomOvhd(AMBomItem sourceBOM, string newBOMID, string newRevisionID, bool copyNotes)
		{
			CopyBomOvhd(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID, copyNotes);
		}

		internal virtual void CopyBomOvhd(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
		{
			var fromRows = PXSelectJoin<AMBomOvhd,
				InnerJoin<AMOverhead, On<AMBomOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
				Where<AMBomOvhd.bOMID, Equal<Required<AMBomOvhd.bOMID>>,
					And<AMBomOvhd.revisionID, Equal<Required<AMBomOvhd.revisionID>>
					>>>.Select(this, sourceID, sourceRevisionID);

			foreach (AMBomOvhd fromRow in fromRows)
			{
				if (fromRow.RowStatus == AMRowStatus.Deleted)
				{
					continue;
				}

				var toRow = PXCache<AMBomOvhd>.CreateCopy(fromRow);
				toRow.BOMID = newBOMID;
				toRow.RevisionID = newRevisionID;
				toRow.NoteID = null;
				toRow.RowStatus = null;
				toRow = BomOvhdRecords.Insert(toRow);

				if (copyNotes)
				{
					PXNoteAttribute.CopyNoteAndFiles(BomOvhdRecords.Cache, fromRow, BomOvhdRecords.Cache, toRow);
					BomOvhdRecords.Update(toRow);
				}
			}
		}

		protected virtual void CopyBomAttributes(AMBomItem sourceBOM, string newBOMID, string newRevisionID)
		{
			CopyBomAttributes(sourceBOM.BOMID, sourceBOM.RevisionID, newBOMID, newRevisionID);
		}

		internal virtual void CopyBomAttributes(string sourceID, string sourceRevisionID, string newBOMID, string newRevisionID)
		{
			FieldVerifying.AddHandler<AMBomAttribute.operationID>((sender, e) => { e.Cancel = true; });

			foreach (PXResult<AMBomAttribute, AMBomOper> result in PXSelectJoin<AMBomAttribute,
					LeftJoin<AMBomOper, On<AMBomAttribute.bOMID, Equal<AMBomOper.bOMID>,
							And<AMBomAttribute.revisionID, Equal<AMBomOper.revisionID>,
						And<AMBomAttribute.operationID, Equal<AMBomOper.operationID>>>>>,
				Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
					And<AMBomAttribute.revisionID, Equal<Required<AMBomAttribute.revisionID>>>>>
				.Select(this, sourceID, sourceRevisionID))
			{
				var fromBomAttribute = (AMBomAttribute)result;
				if (fromBomAttribute.RowStatus == AMRowStatus.Deleted)
				{
					continue;
				}

				var fromBomAttOper = (AMBomOper)result;

				int? newOperationId = null;
				if (fromBomAttOper?.OperationCD != null)
				{
					var newOperation = FindInsertedBomOperByCd(fromBomAttOper.OperationCD);
					if (newOperation?.OperationID == null)
					{
						continue;
					}

					newOperationId = newOperation.OperationID;
				}

				var newBomAtt = (AMBomAttribute)BomAttributes.Cache.CreateCopy(fromBomAttribute);
				newBomAtt.BOMID = newBOMID;
				newBomAtt.RevisionID = newRevisionID;
				newBomAtt.OperationID = newOperationId;
				newBomAtt.RowStatus = null;

				var insertedAttribute = BomAttributes.Insert(newBomAtt);
				if (insertedAttribute != null)
				{
					continue;
				}

				PXTrace.WriteWarning($"Unable to copy {Common.Cache.GetCacheName(typeof(AMBomAttribute))} from ({fromBomAttribute.BOMID};{fromBomAttribute.RevisionID};{fromBomAttribute.LineNbr})");
#if DEBUG
				AMDebug.TraceWriteMethodName($"Unable to copy {Common.Cache.GetCacheName(typeof(AMBomAttribute))} from ({fromBomAttribute.BOMID};{fromBomAttribute.RevisionID};{fromBomAttribute.LineNbr})");
#endif
			}
		}

		private AMBomOper FindInsertedBomOperByCd(string operationCd)
		{
			//Not including bom/rev as inserts should only be checked during copy process
			return BomOperRecords.Cache.Inserted.ToArray<AMBomOper>().FirstOrDefault(x => x.OperationCD == operationCd);
		}

		protected void TraceCopyBomFilterFieldEmpty<Field>() where Field : IBqlField
		{
			PXTrace.WriteWarning(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<Field>(copyBomFilter.Cache));
		}

		protected bool ValidCopyBomFilter(CopyBomFilter copyBomFilter)
		{
			if (copyBomFilter == null)
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(copyBomFilter.ToBOMID) && BomNumbering.Current.UserNumbering.GetValueOrDefault())
			{
				TraceCopyBomFilterFieldEmpty<CopyBomFilter.toBOMID>();
				return false;
			}

			if (string.IsNullOrWhiteSpace(copyBomFilter.FromBOMID))
			{
				TraceCopyBomFilterFieldEmpty<CopyBomFilter.fromBOMID>();
				return false;
			}

			if (string.IsNullOrWhiteSpace(copyBomFilter.ToRevisionID))
			{
				TraceCopyBomFilterFieldEmpty<CopyBomFilter.toRevisionID>();
				return false;
			}

			if (string.IsNullOrWhiteSpace(copyBomFilter.FromRevisionID))
			{
				TraceCopyBomFilterFieldEmpty<CopyBomFilter.fromRevisionID>();
				return false;
			}

			if (copyBomFilter.FromInventoryID == null)
			{
				TraceCopyBomFilterFieldEmpty<CopyBomFilter.fromInventoryID>();
				return false;
			}

			if (copyBomFilter.ToInventoryID == null)
			{
				TraceCopyBomFilterFieldEmpty<CopyBomFilter.toInventoryID>();
				return false;
			}

			if (!ambsetup.Current.AllowEmptyBOMSubItemID.GetValueOrDefault() && InventoryHelper.SubItemFeatureEnabled)
			{
				if (string.IsNullOrWhiteSpace(copyBomFilter.ToSubItemCD))
				{
					TraceCopyBomFilterFieldEmpty<CopyBomFilter.toSubItemCD>();
					return false;
				}

				if (copyBomFilter.ToSubItemID.GetValueOrDefault() <= 0 &&
					!string.IsNullOrWhiteSpace(copyBomFilter.ToSubItemCD))
				{
					throw new PXException(ErrorMessages.ElementOfFieldDoesntExist,
						copyBomFilter.ToSubItemCD,
						PXUIFieldAttribute.GetDisplayName<CopyBomFilter.toSubItemID>(this.copyBomFilter.Cache));
				}

			}

			return true;
		}

		protected virtual AMEstimateHistory GetInsertedEstimateHistory()
		{
			if (!this.Caches<AMEstimateHistory>().IsCurrentRowInserted())
			{
				return null;
			}

			var row = (AMEstimateHistory)this.Caches<AMEstimateHistory>().CreateCopy(this.Caches<AMEstimateHistory>().Current);
			this.Caches<AMEstimateHistory>().Remove(this.Caches<AMEstimateHistory>().Current);
			return row;
		}

		protected virtual void PersistEstimateHistoryRow(AMBomItem bomItem, AMEstimateHistory row)
		{
			if (Documents?.Current?.BOMID == null || row?.EstimateID == null)
			{
				return;
			}

			EstimateMaint.InsertEstimateHistory(row.EstimateID, row.RevisionID,
				Messages.GetLocal(Messages.EstimateCreatedBOM, Documents.Current.BOMID, Documents.Current.RevisionID, row.RevisionID));
		}
	}
}
