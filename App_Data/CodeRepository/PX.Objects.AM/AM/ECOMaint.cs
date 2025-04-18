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

using PX.Data;
using PX.Objects.IN;
using System;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Objects.CS;
using System.Collections;
using PX.Data.WorkflowAPI;
using PX.Objects.EP;
using PX.Objects.Common;
using PX.Objects.Common.GraphExtensions;
using System.Collections.Generic;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.AM
{
    /// <summary>
    /// Engineering Change Request graph
    /// Main graph for managing a Engineering Change Request (ECO)
    /// </summary>
    public class ECOMaint : PXGraph<ECOMaint, AMECOItem>
    {
		[PXViewName(Messages.ECOItem)]
        public PXSelect<AMECOItem> Documents;

        [PXImport(typeof(AMECOItem))]
        public PXSelect<AMBomOper,
            Where<AMBomOper.bOMID, Equal<Current<AMECOItem.eCOID>>,
                And<AMBomOper.revisionID, Equal<AMECOItem.eCORev>>>,
            OrderBy<Asc<AMBomOper.operationCD>>> BomOperRecords;

        [PXImport(typeof(AMECOItem))]
        [PXCopyPasteHiddenFields]
        public AMOrderedMatlSelect<AMECOItem, AMBomMatl,
            Where<AMBomMatl.bOMID, Equal<Current<AMBomOper.bOMID>>,
                And<AMBomMatl.revisionID, Equal<Current<AMBomOper.revisionID>>,
                And<AMBomMatl.operationID, Equal<Current<AMBomOper.operationID>>>>>,
            OrderBy<Asc<AMBomMatl.sortOrder, Asc<AMBomMatl.lineID>>>> BomMatlRecords;

		protected virtual IEnumerable bomMatlRecords()
		{
			bool itVar1 = false;
			foreach (var master in BomMatlRecords.Cache.Inserted)
			{
				itVar1 = true;
				yield return master;
			}

			if (!itVar1)
			{
				var query = SelectFrom<AMBomMatl>.Where<AMBomMatl.bOMID.IsEqual<AMBomOper.bOMID.FromCurrent>
								.And<AMBomMatl.revisionID.IsEqual<AMBomOper.revisionID.FromCurrent>>
								.And<AMBomMatl.operationID.IsEqual<AMBomOper.operationID.FromCurrent>>>
							.OrderBy<Asc<AMBomMatl.sortOrder, Asc<AMBomMatl.lineID>>>.View.ReadOnly.Select(this);
				foreach (PXResult<AMBomMatl> result in query)
				{
					var master = (AMBomMatl)result;
					if (master == null)
						yield break;

					AMBomMatlCury cury = AMBomMatlCury.PK.Find(this, master.BOMID, master.RevisionID, master.OperationID, master.LineID, Accessinfo.BaseCuryID);

					master.UnitCost = cury?.UnitCost ?? 0;
					master.PlanCost = (decimal?)PXFormulaAttribute.Evaluate<AMBomMatl.planCost>(BomMatlRecords.Cache, master);
					master.SiteID = cury?.SiteID;
					master.LocationID = cury?.LocationID;
					yield return master;
				}
			}
		}

		[PXImport(typeof(AMECOItem))]
        public PXSelect<AMBomStep,
            Where<AMBomStep.bOMID, Equal<Current<AMBomOper.bOMID>>,
                And<AMBomStep.revisionID, Equal<Current<AMBomOper.revisionID>>,
                And<AMBomStep.operationID, Equal<Current<AMBomOper.operationID>>>>>> BomStepRecords;

        [PXImport(typeof(AMECOItem))]
        public PXSelectJoin<AMBomTool,
            InnerJoin<AMToolMst, On<AMBomTool.toolID, Equal<AMToolMst.toolID>>>,
            Where<AMBomTool.bOMID, Equal<Current<AMBomOper.bOMID>>,
                And<AMBomTool.revisionID, Equal<Current<AMBomOper.revisionID>>,
                And<AMBomTool.operationID, Equal<Current<AMBomOper.operationID>>>>>> BomToolRecords;

		protected virtual IEnumerable bomToolRecords()
		{
			this.Caches[typeof(AMBomTool)].ClearQueryCache();
			this.Caches[typeof(AMBomToolCury)].ClearQueryCache();

			bool itVar1 = false;
			IEnumerator enumerator = this.BomToolRecords.Cache.Inserted.GetEnumerator();
			while (enumerator.MoveNext())
			{
				AMBomTool master = (AMBomTool)enumerator.Current;
				itVar1 = true;
				yield return master;
			}
			if (!itVar1)
			{
				var query = SelectFrom<AMBomTool>.Where<AMBomTool.bOMID.IsEqual<AMBomOper.bOMID.FromCurrent>
								.And<AMBomTool.revisionID.IsEqual<AMBomOper.revisionID.FromCurrent>>
								.And<AMBomTool.operationID.IsEqual<AMBomOper.operationID.FromCurrent>>>
								.OrderBy<Asc<AMBomTool.lineID>>.View.Select(this);

				foreach (PXResult<AMBomTool> result in query)
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

		[PXImport(typeof(AMECOItem))]
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

        public PXSetup<AMBSetup> ambsetup;
        public PXSetup<AMPSetup> ProdSetup;
        public PXSelect<AMECOItem, Where<AMECOItem.eCOID, Equal<Current<AMECOItem.eCOID>>>> CurrentDocument;

		public class BomOperCurySettings : CurySettingsExtension<ECOMaint, AMBomOper, AMBomOperCury>
		{
			public static bool IsActive() => true;

			protected override List<Type> ComposeCommand()
			{
				List<Type> list = new List<Type>(15)
				{
					typeof(Select<,>),
					typeof(AMBomOperCury),
					typeof(Where<,,>),
					typeof(AMBomOperCury.bOMID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomOper.bOMID),
					typeof(And<,,>),
					typeof(AMBomOperCury.revisionID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomOper.revisionID),
					typeof(And<,,>),
					typeof(AMBomOperCury.operationID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomOper.operationID),
					typeof(And<,>),
					typeof(AMBomOperCury.curyID),
					typeof(Equal<>),
					typeof(Optional<>),
					typeof(AccessInfo.baseCuryID)
				};

				return list;
			}
		}

		public class BomToolCurySettings : CurySettingsExtension<ECOMaint, AMBomTool, AMBomToolCury>
		{
			public static bool IsActive() => true;

			protected override List<Type> ComposeCommand()
			{
				List<Type> list = new List<Type>(15)
				{
					typeof(Select<,>),
					typeof(AMBomToolCury),
					typeof(Where<,,>),
					typeof(AMBomToolCury.bOMID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomTool.bOMID),
					typeof(And<,,>),
					typeof(AMBomToolCury.revisionID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomTool.revisionID),
					typeof(And<,,>),
					typeof(AMBomToolCury.operationID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomTool.operationID),
					typeof(And<,,>),
					typeof(AMBomToolCury.lineID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomTool.lineID),
					typeof(And<,>),
					typeof(AMBomToolCury.curyID),
					typeof(Equal<>),
					typeof(Optional<>),
					typeof(AccessInfo.baseCuryID)
				};

				return list;
			}
		}

		public class BomMatlCurySettings : CurySettingsExtension<ECOMaint, AMBomMatl, AMBomMatlCury>
		{
			public static bool IsActive() => true;

			protected override List<Type> ComposeCommand()
			{
				List<Type> list = new List<Type>(15)
				{
					typeof(Select<,>),
					typeof(AMBomMatlCury),
					typeof(Where<,,>),
					typeof(AMBomMatlCury.bOMID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomMatl.bOMID),
					typeof(And<,,>),
					typeof(AMBomMatlCury.revisionID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomMatl.revisionID),
					typeof(And<,,>),
					typeof(AMBomMatlCury.operationID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomMatl.operationID),
					typeof(And<,,>),
					typeof(AMBomMatlCury.lineID),
					typeof(Equal<>),
					typeof(Current<>),
					typeof(AMBomMatl.lineID),
					typeof(And<,>),
					typeof(AMBomMatlCury.curyID),
					typeof(Equal<>),
					typeof(Optional<>),
					typeof(AccessInfo.baseCuryID)
				};

				return list;
			}
		}

		public bool MBCEnabled => PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();

		[PXHidden]
        public PXSelect<
            AMBomAttribute,
            Where<AMBomAttribute.bOMID, Equal<Current<AMECOItem.eCOID>>,
                And<AMBomAttribute.revisionID, Equal<AMECOItem.eCORev>>>> BomAttributes;

        [PXHidden]
        public PXSelect<AMECRItem> ECRItem;

        [PXHidden]
        public PXSelect<AMBomOper,
            Where<AMBomOper.bOMID, Equal<Current<AMBomOper.bOMID>>,
                And<AMBomOper.revisionID, Equal<Current<AMBomOper.revisionID>>,
                And<AMBomOper.operationID, Equal<Current<AMBomOper.operationID>>>>>> OutsideProcessingOperationSelected;

        public ECOMaint()
        {
            var bomSetup = ambsetup.Current;
            if (string.IsNullOrWhiteSpace(bomSetup?.ECONumberingID))
            {
                throw new BOMSetupNotEnteredException();
            }

			this.Insert.SetEnabled(!bomSetup.RequireECRBeforeECO.GetValueOrDefault());
			this.CopyPaste.SetVisible(!bomSetup.RequireECRBeforeECO.GetValueOrDefault());
		}

        public PXSelect<AMECOSetupApproval> SetupApproval;

        [PXViewName(PX.Objects.EP.Messages.Approval)]
        public PX.Objects.EP.EPApprovalAutomation<AMECOItem, AMECOItem.approved, AMECOItem.rejected, AMECOItem.hold, AMECOSetupApproval> Approval;

        #region CACHE ATTACHED

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
        [PXLineNbr(typeof(AMECOItem.lineCntrOperation))]
		protected virtual void _(Events.CacheAttached<AMBomOper.operationID> e)
        {
#if DEBUG
            //Cache attached to change display name so we can provide the user with a way to see the DB ID if needed 
#endif
        }

        [BomID(IsKey = true, Visible = false, Enabled = false)]
        [BOMIDSelector(ValidateValue = false)]
        [PXDBDefault(typeof(AMECOItem.eCOID))]
        [PXParent(typeof(Select<AMECOItem, Where<AMECOItem.eCOID, Equal<Current<AMBomOper.bOMID>>,
            And<AMECOItem.eCORev, Equal<Current<AMBomOper.revisionID>>>>>))]
		protected virtual void _(Events.CacheAttached<AMBomOper.bOMID> e) { }

        [BomID(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMECOItem.eCOID))]
        [PXParent(typeof(Select<AMECOItem, Where<AMECOItem.eCOID, Equal<Current<AMBomAttribute.bOMID>>,
            And<AMECOItem.eCORev, Equal<Current<AMBomAttribute.revisionID>>>>>))]
		protected virtual void _(Events.CacheAttached<AMBomAttribute.bOMID> e) { }

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMECOItem.lineCntrAttribute))]
		protected virtual void _(Events.CacheAttached<AMBomAttribute.lineNbr> e) { }

        [PXDBDate]
        [PXDefault(typeof(AMECOItem.requestDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.docDate> e) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECOItem))]
        [PXDefault]
		protected virtual void _(Events.CacheAttached<AMBomAttribute.rowStatus> e) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECOItem))]
        [PXDefault]
		protected virtual void _(Events.CacheAttached<AMBomMatl.rowStatus> e) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECOItem))]
        [PXDefault]
		protected virtual void _(Events.CacheAttached<AMBomOper.rowStatus> e) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECOItem))]
        [PXDefault]
		protected virtual void _(Events.CacheAttached<AMBomOvhd.rowStatus> e) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECOItem))]
        [PXDefault]
		protected virtual void _(Events.CacheAttached<AMBomRef.rowStatus> e) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECOItem))]
        [PXDefault]
		protected virtual void _(Events.CacheAttached<AMBomStep.rowStatus> e) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMRowStatusEvent(typeof(AMECOItem))]
        [PXDefault]
		protected virtual void _(Events.CacheAttached<AMBomTool.rowStatus> e) { }

        [PXDBInt]
        [PXDefault(typeof(AMECOItem.requestor), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.bAccountID> e) { }
		protected virtual void EPApproval_Details_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var eco = Documents.Current;
            if (eco != null)
            {
                InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<AMECRItem.inventoryID>(Documents.Cache, eco);
                if (item != null)
                {
                    e.NewValue = ECRMaint.BOMRevItemDisplay(eco.BOMID, eco.BOMRevisionID, item.InventoryCD);
                }
            }
        }

        protected virtual void EPApproval_Descr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var eco = Documents.Current;
            if (eco != null)
            {
                e.NewValue = eco.Descr;
            }
        }

        #endregion

        #region EP Approval Actions
        public PXInitializeState<AMECOItem> initializeState;
        public PXAction<AMECOItem> hold;
        [PXUIField(DisplayName = "Hold", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Hold(PXAdapter adapter) => adapter.Get();

        public PXAction<AMECOItem> approve;
        public PXAction<AMECOItem> reject;

        [PXUIField(DisplayName = "Approve", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public IEnumerable Approve(PXAdapter adapter) => adapter.Get();


        [PXUIField(DisplayName = "Reject", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public IEnumerable Reject(PXAdapter adapter) => adapter.Get();

        public PXAction<AMECOItem> submit;

        [PXUIField(DisplayName = "Submit", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        public IEnumerable Submit(PXAdapter adapter) => adapter.Get();

        public PXAction<AMECOItem> commitChanges;
        [PXUIField(DisplayName = "Commit Changes to BOM", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable CommitChanges(PXAdapter adapter)
        {
            var eco = Documents.Current;
            if (eco?.ECOID != null)
            {
                PXLongOperation.StartOperation(this, () =>
                {
                    var bomGraph = CopyECOtoBOM(eco);
                    if (bomGraph != null)
                    {
                        PXRedirectHelper.TryRedirect(bomGraph, PXRedirectHelper.WindowMode.NewWindow);
                    }
                });
            }
            return adapter.Get();
        }

        #endregion

		protected virtual void _(Events.RowSelected<AMECOItem> e)
        {
            Approval.AllowSelect = ambsetup?.Current?.ECORequestApproval == true;

            if (e.Row == null)
            {
                return;
            }

            EnableECOItemFields(e.Cache, e.Row);
            // When inserted we want to disable because updated row status is not possible as no save yet
            var holdEnabled = e.Row.Hold.GetValueOrDefault() && !e.Cache.IsRowInserted(e.Row);
            EnableOperCache(holdEnabled);
            EnableOperChildCache(holdEnabled);

            commitChanges.SetEnabled(e.Row.Approved.GetValueOrDefault());
        }

		protected virtual void _(Events.RowDeleted<AMECOItem> e)
        {
            if (e.Row != null)
            {
                UpdateECRStatus(e.Row.ECOID, AMECRStatus.Approved);
            }
        }

        protected virtual void UpdateECRStatus(string ecoID, string newStatus)
        {
            foreach (AMECRItem ecr in PXSelect<AMECRItem, Where<AMECRItem.eCOID,
                Equal<Required<AMECRItem.eCOID>>>>.Select(this, ecoID))
            {
                if (ecr != null && ecr.Status != newStatus)
                {
                    ecr.Status = newStatus;
                    ecr.ECOID = newStatus == AMECRStatus.Approved ? null : ecoID;
                    ECRItem.Update(ecr);
                }
            }
        }

        protected virtual void EnableOperCache(bool enabled)
        {
            BomOperRecords.AllowInsert = enabled;
            BomOperRecords.AllowUpdate = enabled;
            BomOperRecords.AllowDelete = enabled;
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

			BomAttributes.AllowInsert = enabled;
			BomAttributes.AllowUpdate = enabled;
			BomAttributes.AllowDelete = enabled;
		}

        protected virtual void AMECOItem_RevisionID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = (AMECOItem)e.Row;
            if (row == null || e.NewValue == null)
            {
                return;
            }
            AMBomItem item = PXSelect<AMBomItem, Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>>>>.Select(this, row.BOMID, e.NewValue);
            if (item != null)
            {
                e.Cancel = true;
                throw new PXSetPropertyException(Messages.GetLocal(Messages.BomRevisionExists), PXErrorLevel.Error, row.BOMID, e.NewValue);
            }
        }

        #region BOM Oper Processes

        protected virtual AMWC GetCurrentWorkcenter()
        {
            AMWC workCenter = PXSelect<AMWC, Where<AMWC.wcID, Equal<Current<AMBomOper.wcID>>>>.Select(this);

            if (this.Caches<AMWC>() != null)
            {
                this.Caches<AMWC>().Current = workCenter;
            }

            return workCenter;
        }

        protected virtual void AMBomOper_WcID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            SetWorkCenterFields(cache, (AMBomOper)e.Row);
        }

        protected virtual void AMBomOper_RevisionID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMECOItem.ECORev;
        }

        protected virtual void AMBomOper_WcID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetWorkCenterFields(cache, (AMBomOper)e.Row);
        }

        protected virtual void SetWorkCenterFields(PXCache cache, AMBomOper bomOper)
        {
            if (cache == null || bomOper == null)
            {
                return;
            }

            var amWC = GetCurrentWorkcenter();

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

            // Set the Scrap Action from Work Center
            cache.SetValueExt<AMBomOper.scrapAction>(bomOper, amWC.ScrapAction);
            cache.SetValueExt<AMBomOper.outsideProcess>(bomOper, amWC.OutsideFlg.GetValueOrDefault());
        }

        protected virtual void AMBomOper_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            var row = (AMBomOper)e.Row;
            if (row == null || Documents.Cache.IsCurrentRowDeleted())
            {
                return;
            }

            AMBomAttribute bomOperAttribute = PXSelect<AMBomAttribute,
                Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
                And<AMBomAttribute.revisionID, Equal<Required<AMBomAttribute.revisionID>>,
                And<AMBomAttribute.operationID, Equal<Required<AMBomAttribute.operationID>>>>
                >>.Select(this, row.BOMID, row.RevisionID, row.OperationID);

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

            DeleteBomOperationAttributes(row);
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

        #endregion

        #region BOM Matl Processes

        protected virtual void AMBomMatl_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMBomMatl)e.Row;
            if (row == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMBomMatl.subItemID>(sender, e.Row, row.IsStockItem.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMBomMatl.subcontractSource>(sender, e.Row, row.MaterialType == AMMaterialType.Subcontract);

            if (IsImport || IsContractBasedAPI)
            {
                return;
            }

            var isMatlExpired = row.ExpDate > Common.Current.BusinessDate(this) || Common.Dates.IsDateNull(row.ExpDate);
            if (!isMatlExpired)
            {
                sender.RaiseExceptionHandling<AMBomMatl.inventoryID>(row, row.InventoryID,
                    new PXSetPropertyException(Messages.MaterialExpiredOnBom, PXErrorLevel.Warning, row.BOMID, row.RevisionID));
            }
        }

        protected virtual void AMBomMatl_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            var matl = (AMBomMatl)e.Row;
            if (matl == null)
            {
                return;
            }

            var subItemFeatureEnabled = InventoryHelper.SubItemFeatureEnabled;

            // Require SUBITEMID when the item is a stock item
            if (subItemFeatureEnabled && matl.InventoryID != null && matl.IsStockItem.GetValueOrDefault() && matl.SubItemID == null)
            {
                cache.RaiseExceptionHandling<AMBomMatl.subItemID>(
                        matl,
                        matl.SubItemID,
                        new PXSetPropertyException(Messages.SubItemIDRequiredForStockItem, PXErrorLevel.Error));
            }

            //  PREVENT A USER FROM ADDING THE MATERIAL ITEM TO ITSELF
            //      More in depth prevention can be added down the road
            if (Documents.Current != null && matl.InventoryID.GetValueOrDefault() != 0)
            {
                if (matl.InventoryID == Documents.Current.InventoryID)
                {
                    if (subItemFeatureEnabled
                        && matl.IsStockItem.GetValueOrDefault()
                        && Documents.Current.SubItemID != null
                        && matl.SubItemID.GetValueOrDefault() != Documents.Current.SubItemID.GetValueOrDefault())
                    {
                        //this should allow different sub items to be consumed on the same BOM as the item being built
                        return;
                    }

                    cache.RaiseExceptionHandling<AMBomMatl.inventoryID>(
                        matl,
                        matl.InventoryID,
                        new PXSetPropertyException(Messages.BomMatlCircularRefAttempt, PXErrorLevel.Error));
                }
            }
        }

        protected virtual void AMBomMatl_SubItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var amBomMatl = (AMBomMatl)e.Row;
            if (amBomMatl == null || Documents.Current == null
                || e.NewValue == null || amBomMatl.InventoryID == null
                || !InventoryHelper.SubItemFeatureEnabled)
            {
                return;
            }

            int? subItemID = Convert.ToInt32(e.NewValue ?? 0);
            if (amBomMatl.InventoryID == Documents.Current.InventoryID
                && (Documents.Current.SubItemID == null
                || Documents.Current.SubItemID.GetValueOrDefault() == subItemID))
            {
                e.NewValue = null;
                e.Cancel = true;
                throw new PXSetPropertyException(Messages.BomMatlCircularRefAttempt, PXErrorLevel.Error);
            }

            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, amBomMatl.InventoryID);
            if (item == null)
            {
                return;
            }
            CheckDuplicateEntry(e, amBomMatl, item, subItemID);
        }

        protected virtual void AMBomMatl_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var amBomMatl = (AMBomMatl)e.Row;
            if (amBomMatl == null || Documents.Current == null
                || e.NewValue == null || InventoryHelper.SubItemFeatureEnabled)
            {
                return;
            }

            int? inventoryID = Convert.ToInt32(e.NewValue);
            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);

            if (item == null)
            {
                return;
            }

            //  PREVENT A USER FROM ADDING THE MATERIAL ITEM TO ITSELF
            //      More in depth prevention can be added down the road
            if (inventoryID == Documents.Current.InventoryID)
            {
                e.NewValue = item.InventoryCD;
                e.Cancel = true;
                throw new PXSetPropertyException(Messages.BomMatlCircularRefAttempt, PXErrorLevel.Error);
            }
            CheckDuplicateEntry(e, amBomMatl, item, amBomMatl.SubItemID);
        }

        protected virtual void AMBomMatl_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amBomMatl = (AMBomMatl)e.Row;
            if (amBomMatl == null)
            {
                return;
            }

            if (Documents.Current != null && amBomMatl.InventoryID.GetValueOrDefault() != 0)
            {
                cache.SetDefaultExt<AMBomMatl.descr>(e.Row);
                cache.SetDefaultExt<AMBomMatl.subItemID>(e.Row);
                cache.SetDefaultExt<AMBomMatl.uOM>(e.Row);
                cache.SetDefaultExt<AMBomMatl.unitCost>(e.Row);
            }
        }

        protected virtual void DefaultUnitCost(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            object MatlUnitCost;
            sender.RaiseFieldDefaulting<AMBomMatl.unitCost>(e.Row, out MatlUnitCost);

            if (MatlUnitCost != null && (decimal)MatlUnitCost != 0m)
            {
                decimal? matlUnitCost = INUnitAttribute.ConvertToBase<AMBomMatl.inventoryID>(sender, e.Row, ((AMBomMatl)e.Row).UOM, (decimal)MatlUnitCost, INPrecision.UNITCOST);
                sender.SetValueExt<AMBomMatl.unitCost>(e.Row, matlUnitCost);
            }

        }

        protected virtual void AMBomMatl_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            DefaultUnitCost(sender, e);
        }

        protected virtual void AMBomAttribute_RevisionID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMECRItem.ECRRev;
        }

        protected virtual void AMBomAttribute_AttributeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMBomAttribute)e.Row;
            if (row == null)
            {
                return;
            }

            var item = (CSAttribute)PXSelectorAttribute.Select<AMBomAttribute.attributeID>(sender, row);
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(row.Label))
            {
                sender.SetValueExt<AMBomAttribute.label>(row, item.AttributeID);
            }
            if (string.IsNullOrWhiteSpace(row.Descr))
            {
                sender.SetValueExt<AMBomAttribute.descr>(row, item.Description);
            }
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
            if (IsImport && bomSetup.DupInvBOM.Trim() == SetupMessage.WarningMsg)
            {
                bomSetup.DupInvBOM = SetupMessage.AllowMsg;
            }
            if (IsImport && bomSetup.DupInvOper.Trim() == SetupMessage.WarningMsg)
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
                            And<AMBomOper.operationID, Equal<Required<AMBomOper.operationID>>>>>
                            .Select(this, duplicateAMBomMatl.BOMID, duplicateAMBomMatl.OperationID);

            var userMessage = Messages.GetLocal(Messages.EcoMatlDupItems, operBomValue?.OperationCD, operBomValue?.BOMID);            

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

        #endregion

        protected virtual void AMBomTool_ToolID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (AMBomTool)e.Row;
            if (row == null)
            {
                return;
            }

            var toolMst = (AMToolMst)PXSelectorAttribute.Select<AMToolMst.toolID>(cache, row);

            row.Descr = toolMst?.Descr;
			var toolCury = AMToolMstCurySettings.PK.Find(this, toolMst.ToolID, Accessinfo.BaseCuryID);
            row.UnitCost = toolCury?.UnitCost ?? 0m;
        }


        #region Copy ECR
        public virtual void CopyECRtoECO(PXCache ecoItemCache, AMECOItem eco, AMECRItem sourceECR)
        {     
            if (sourceECR?.ECRID == null)
            {
                return;
            }

            eco.RevisionID = sourceECR.RevisionID;
            eco.InventoryID = sourceECR.InventoryID;
            eco.SubItemID = sourceECR.SubItemID;
            eco.SiteID = sourceECR.SiteID;
            eco.Descr = sourceECR.Descr;
            eco.BOMID = sourceECR.BOMID;
            eco.BOMRevisionID = sourceECR.BOMRevisionID;
            eco.Requestor = sourceECR.Requestor;
            eco.RequestDate = sourceECR.RequestDate;
            eco.EffectiveDate = sourceECR.EffectiveDate;
            eco.LineCntrAttribute = sourceECR.LineCntrAttribute;
			eco.Priority = sourceECR.Priority;

            PXNoteAttribute.CopyNoteAndFiles(this.Caches<AMECRItem>(), sourceECR, ecoItemCache, eco);

			using (new DisableFormulaCalculationScope(BomOperRecords.Cache, typeof(AMBomOper.queueTime), typeof(AMBomOper.finishTime), typeof(AMBomOper.moveTime)))
			{
				CopyBomOper(sourceECR.ECRID, AMECRItem.ECRRev, eco.ECOID, AMECOItem.ECORev, true);
			}

            CopyBomMatl(sourceECR.ECRID, AMECRItem.ECRRev, eco.ECOID, AMECOItem.ECORev, true);
            CopyBomStep(sourceECR.ECRID, AMECRItem.ECRRev, eco.ECOID, AMECOItem.ECORev, true);
            CopyBomRef(sourceECR.ECRID, AMECRItem.ECRRev, eco.ECOID, AMECOItem.ECORev);
            CopyBomTool(sourceECR.ECRID, AMECRItem.ECRRev, eco.ECOID, AMECOItem.ECORev, true);
            CopyBomOvhd(sourceECR.ECRID, AMECRItem.ECRRev, eco.ECOID, AMECOItem.ECORev, true);
            CopyBomAttributes(sourceECR.ECRID, AMECRItem.ECRRev, eco.ECOID, AMECOItem.ECORev);
        }

        public virtual void UpdateECRStatus(AMECRItem ecrItem, string newStatus)
        {
            if (ecrItem == null)
            {
                return;
            }

            ecrItem.Status = newStatus;
            ECRItem.Update(ecrItem);
        }

        protected virtual bool CheckBomRevisionExists(string bomId, string revisionId)
        {
            //TODO 19R1 Implement Primary and Foreign Key API - See attachment on work item #2431
            return false;
        }

        protected virtual AMBomItem CreateNewBomItem(BOMMaint bomGraph, AMECOItem ecoItem)
        {
            if (ecoItem?.BOMID == null)
            {
                return null;
            }

            var newBomItem = bomGraph.Documents.Insert(new AMBomItem
            {
                BOMID = ecoItem.BOMID,
                RevisionID = ecoItem.RevisionID,
                InventoryID = ecoItem.InventoryID,
                SubItemID = ecoItem.SubItemID,
                SiteID = ecoItem.SiteID
            });

            newBomItem.Descr = ecoItem.Descr;
            newBomItem.EffStartDate = ecoItem.EffectiveDate;
            newBomItem.LineCntrAttribute = ecoItem.LineCntrAttribute;
            return bomGraph.Documents.Update(newBomItem);
        }

        protected virtual BOMMaint CopyECOtoBOM(AMECOItem source)
        {
            if (source?.BOMID == null || source.RevisionID == null)
            {
                return null;
            }

            if (CheckBomRevisionExists(source.BOMID, source.BOMRevisionID))
            {
                throw new PXException(Messages.BomRevisionExists, source.BOMID, source.BOMRevisionID);
            }

            var bomGraph = CreateInstance<BOMMaint>();
            bomGraph.IsImport = true;
            // prevent update from here
            bomGraph.copyBomFilter.Current.UpdateMaterialWarehouse = false;

            var newBomItem = CreateNewBomItem(bomGraph, source);

            if (newBomItem?.RevisionID == null)
            {
                return null;
            }

            PXNoteAttribute.CopyNoteAndFiles(Documents.Cache, source, bomGraph.Documents.Cache, newBomItem);
            newBomItem = bomGraph.Documents.Update(newBomItem);

            //This will force the BOM ID to set when auto number
            var rowStatus = bomGraph.Documents.Cache.GetStatus(newBomItem);
            bomGraph.Documents.Cache.SetValue<AMBomItem.bOMID>(newBomItem, source.BOMID);
            bomGraph.Documents.Cache.SetStatus(newBomItem, rowStatus);

            bomGraph.CopyBomOper(source.ECOID, AMECOItem.ECORev, newBomItem.BOMID, newBomItem.RevisionID, true);
            bomGraph.CopyBomMatl(source.ECOID, AMECOItem.ECORev, newBomItem.BOMID, newBomItem.RevisionID, true);
            bomGraph.CopyBomStep(source.ECOID, AMECOItem.ECORev, newBomItem.BOMID, newBomItem.RevisionID, true);
            bomGraph.CopyBomRef(source.ECOID, AMECOItem.ECORev, newBomItem.BOMID, newBomItem.RevisionID);
            bomGraph.CopyBomTool(source.ECOID, AMECOItem.ECORev, newBomItem.BOMID, newBomItem.RevisionID, true);
            bomGraph.CopyBomOvhd(source.ECOID, AMECOItem.ECORev, newBomItem.BOMID, newBomItem.RevisionID, true);
            bomGraph.CopyBomAttributes(source.ECOID, AMECOItem.ECORev, newBomItem.BOMID, newBomItem.RevisionID);

            //set the ECO to completed
            source.Status = AMECRStatus.Completed;
            bomGraph.EcoItem.Update(source);

            return bomGraph;
        }

        protected virtual void CopyBomOper(string srcBOMID, string srcRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
        {
			using (new DisableSelectorValidationScope(BomOperRecords.Cache))
			{
				var fromRows = PXSelect<AMBomOper,
				Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
					And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>>>
					>.Select(this, srcBOMID, srcRevisionID);

				foreach (AMBomOper fromRow in fromRows)
				{
					var toRow = PXCache<AMBomOper>.CreateCopy(fromRow);
					toRow.BOMID = newBOMID;
					toRow.RevisionID = newRevisionID;
					toRow.NoteID = null;
					toRow = BomOperRecords.Insert(toRow);

					if (copyNotes)
					{
						PXNoteAttribute.CopyNoteAndFiles(BomOperRecords.Cache, fromRow, BomOperRecords.Cache, toRow);
						BomOperRecords.Update(toRow);
					}

					foreach (PXResult<AMBOMCurySettings> fromCury in SelectFrom<AMBOMCurySettings>.Where<AMBOMCurySettings.bOMID.IsEqual<@P.AsString>
						.And<AMBOMCurySettings.revisionID.IsEqual<@P.AsString>>
						.And<AMBOMCurySettings.operationID.IsEqual<@P.AsInt>>
						.And<AMBOMCurySettings.lineType.IsEqual<BOMCurySettingsLineType.operation>>>.View.Select(this, fromRow.BOMID, fromRow.RevisionID, fromRow.OperationID))
					{
						var toCury = PXCache<AMBOMCurySettings>.CreateCopy(fromCury);
						toCury.BOMID = newBOMID;
						toCury.RevisionID = newRevisionID;
						this.Caches<AMBOMCurySettings>().Insert(toCury);
					}
				}
			}
        }

        protected virtual void CopyBomMatl(string srcBOMID, string srcRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
        {
            foreach (PXResult<AMBomMatl, InventoryItem> result in PXSelectJoin<
                AMBomMatl,
                InnerJoin<InventoryItem,
                    On<AMBomMatl.inventoryID, Equal<InventoryItem.inventoryID>>>,
                Where<AMBomMatl.bOMID, Equal<Required<AMBomMatl.bOMID>>,
                    And<AMBomMatl.revisionID, Equal<Required<AMBomMatl.revisionID>>,
                    And<Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
                        And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>>>>,
                OrderBy<
                    Asc<AMBomMatl.sortOrder,
                    Asc<AMBomMatl.lineID>>>
                >
                .Select(this, srcBOMID, srcRevisionID))
            {
                var fromRow = (AMBomMatl)result;
                var inventoryItem = (InventoryItem)result;

                if (fromRow == null || inventoryItem == null ||
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
                if (toRow.CompBOMID != null)
                {
                    toRow.CompBOMID = null;
                    toRow.CompBOMRevisionID = null;
                }

                try
                {
                    toRow = BomMatlRecords.Insert(toRow);

                    if (copyNotes)
                    {
                        PXNoteAttribute.CopyNoteAndFiles(BomMatlRecords.Cache, fromRow, BomMatlRecords.Cache, toRow);
                        BomMatlRecords.Update(toRow);
                    }

					foreach (PXResult<AMBomMatlCury> fromCury in SelectFrom<AMBomMatlCury>.Where<AMBomMatlCury.bOMID.IsEqual<@P.AsString>
						.And<AMBomMatlCury.revisionID.IsEqual<@P.AsString>>
						.And<AMBomMatlCury.operationID.IsEqual<@P.AsInt>>
						.And<AMBomMatlCury.lineID.IsEqual<@P.AsInt>>
						.And<AMBomMatlCury.lineType.IsEqual<BOMCurySettingsLineType.material>>>.View.Select(this, fromRow.BOMID, fromRow.RevisionID, fromRow.OperationID, fromRow.LineID))
					{
						var toCury = PXCache<AMBomMatlCury>.CreateCopy(fromCury);
						toCury.BOMID = newBOMID;
						toCury.RevisionID = newRevisionID;
						this.Caches<AMBomMatlCury>().Insert(toCury);
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
        protected virtual void CopyBomStep(string srcBOMID, string srcRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
        {
            var fromRows = PXSelect<AMBomStep,
                Where<AMBomStep.bOMID, Equal<Required<AMBomStep.bOMID>>,
                    And<AMBomStep.revisionID, Equal<Required<AMBomStep.revisionID>>
                    >>>.Select(this, srcBOMID, srcRevisionID);

            foreach (AMBomStep fromRow in fromRows)
            {
                var toRow = PXCache<AMBomStep>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow = BomStepRecords.Insert(toRow);
                toRow.RowStatus = fromRow.RowStatus;
                toRow = BomStepRecords.Update(toRow);

                if (copyNotes)
                {
                    PXNoteAttribute.CopyNoteAndFiles(BomStepRecords.Cache, fromRow, BomStepRecords.Cache, toRow);
                    BomStepRecords.Update(toRow);
                }
            }
        }
        protected virtual void CopyBomRef(string srcBOMID, string srcRevisionID, string newBOMID, string newRevisionID)
        {
            var fromRows = PXSelect<AMBomRef,
                Where<AMBomRef.bOMID, Equal<Required<AMBomRef.bOMID>>,
                    And<AMBomRef.revisionID, Equal<Required<AMBomRef.revisionID>>
                    >>>.Select(this, srcBOMID, srcRevisionID);

            foreach (AMBomRef fromRow in fromRows)
            {
                var toRow = PXCache<AMBomRef>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow = BomRefRecords.Insert(toRow);
                toRow.RowStatus = fromRow.RowStatus;
                BomRefRecords.Update(toRow);
            }
        }

        protected virtual void CopyBomTool(string srcBOMID, string srcRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
        {
            var fromRows = PXSelectJoin<AMBomTool,
                InnerJoin<AMToolMst, On<AMBomTool.toolID, Equal<AMToolMst.toolID>>>,
                Where<AMBomTool.bOMID, Equal<Required<AMBomTool.bOMID>>,
                    And<AMBomTool.revisionID, Equal<Required<AMBomTool.revisionID>>
                    >>>.Select(this, srcBOMID, srcRevisionID);

            foreach (AMBomTool fromRow in fromRows)
            {
                var toRow = PXCache<AMBomTool>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow = BomToolRecords.Insert(toRow);
                toRow.RowStatus = fromRow.RowStatus;
                toRow = BomToolRecords.Update(toRow);

                if (copyNotes)
                {
                    PXNoteAttribute.CopyNoteAndFiles(BomToolRecords.Cache, fromRow, BomToolRecords.Cache, toRow);
                    BomToolRecords.Update(toRow);
                }

				foreach (PXResult<AMBomToolCury> fromCury in SelectFrom<AMBomToolCury>.Where<AMBomToolCury.bOMID.IsEqual<@P.AsString>
					.And<AMBomToolCury.revisionID.IsEqual<@P.AsString>>
					.And<AMBomToolCury.operationID.IsEqual<@P.AsInt>>
					.And<AMBomToolCury.lineID.IsEqual<@P.AsInt>>
					.And<AMBomToolCury.lineType.IsEqual<BOMCurySettingsLineType.tool>>>.View.Select(this, fromRow.BOMID, fromRow.RevisionID, fromRow.OperationID, fromRow.LineID))
				{
					var toCury = PXCache<AMBomToolCury>.CreateCopy(fromCury);
					toCury.BOMID = newBOMID;
					toCury.RevisionID = newRevisionID;
					this.Caches<AMBomToolCury>().Insert(toCury);
				}
			}
        }

        protected virtual void CopyBomOvhd(string srcBOMID, string srcRevisionID, string newBOMID, string newRevisionID, bool copyNotes)
        {
            var fromRows = PXSelectJoin<AMBomOvhd,
                InnerJoin<AMOverhead, On<AMBomOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
                Where<AMBomOvhd.bOMID, Equal<Required<AMBomOvhd.bOMID>>,
                    And<AMBomOvhd.revisionID, Equal<Required<AMBomOvhd.revisionID>>
                    >>>.Select(this, srcBOMID, srcRevisionID);

            foreach (AMBomOvhd fromRow in fromRows)
            {
                var toRow = PXCache<AMBomOvhd>.CreateCopy(fromRow);
                toRow.BOMID = newBOMID;
                toRow.RevisionID = newRevisionID;
                toRow.NoteID = null;
                toRow = BomOvhdRecords.Insert(toRow);
                toRow.RowStatus = fromRow.RowStatus;
                toRow = BomOvhdRecords.Update(toRow);

                if (copyNotes)
                {
                    PXNoteAttribute.CopyNoteAndFiles(BomOvhdRecords.Cache, fromRow, BomOvhdRecords.Cache, toRow);
                    BomOvhdRecords.Update(toRow);
                }
            }
        }

        protected virtual void CopyBomAttributes(string srcBOMID, string srcRevisionID, string newBOMID, string newRevisionID)
        {
            FieldVerifying.AddHandler<AMBomAttribute.operationID>((sender, e) => { e.Cancel = true; });

            foreach (PXResult<AMBomAttribute, AMBomOper> result in PXSelectJoin<AMBomAttribute,
                    LeftJoin<AMBomOper, On<AMBomAttribute.bOMID, Equal<AMBomOper.bOMID>,
                            And<AMBomAttribute.revisionID, Equal<AMBomOper.revisionID>,
                        And<AMBomAttribute.operationID, Equal<AMBomOper.operationID>>>>>,
                Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
                    And<AMBomAttribute.revisionID, Equal<Required<AMBomAttribute.revisionID>>>>>
                .Select(this, srcBOMID, srcRevisionID))
            {
                var fromBomAttribute = (AMBomAttribute)result;
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
                var insertedAttribute = BomAttributes.Insert(newBomAtt);
                if (insertedAttribute != null)
                {
                    insertedAttribute.RowStatus = fromBomAttribute.RowStatus;
                    BomAttributes.Update(insertedAttribute);
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

        #endregion

        protected virtual void EnableECOItemFields(PXCache cache, AMECOItem item)
        {
            if (item == null)
            {
                return;
            }

			if (!item.Hold.GetValueOrDefault())
            {
				PXUIFieldAttribute.SetEnabled(cache, item, false);
				PXUIFieldAttribute.SetEnabled<AMECOItem.eCOID>(cache, item, true);
                return;
            }

			var allowDirectECO = IsImport || IsContractBasedAPI ||
				(ambsetup?.Current?.RequireECRBeforeECO == false && cache.IsRowInserted(item));
			if(allowDirectECO && ECRItem?.Current != null && ECRItem.Cache.GetStatus(ECRItem.Current) == PXEntryStatus.Updated)
			{
				allowDirectECO = false;
			}

			PXUIFieldAttribute.SetEnabled<AMECOItem.bOMID>(cache, item, allowDirectECO);
			PXUIFieldAttribute.SetEnabled<AMECOItem.bOMRevisionID>(cache, item, allowDirectECO);
			PXUIFieldAttribute.SetEnabled<AMECOItem.priority>(cache, item, allowDirectECO);
			PXUIFieldAttribute.SetVisible<AMECOItem.siteID>(cache, item, !MBCEnabled);
		}

        protected virtual void _(Events.RowPersisted<AMECOItem> e)
        {
            if (e?.Row == null || e.TranStatus != PXTranStatus.Open || e.Operation != PXDBOperation.Insert)
            {
                return;
            }

            foreach (AMECRItem ecr in ECRItem.Cache.Updated)
            {
                var copy = (AMECRItem)ECRItem.Cache.CreateCopy(ecr);
                if (copy == null)
                {
                    continue;
                }
                copy.ECOID = e.Row.ECOID;
                ECRItem.Update(copy);
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
    }

	public class ECOMaintECCExt : ECCBaseGraph<ECOMaint, AMECOItem>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.manufacturingECC>();

		protected override string ECCRev => AMECOItem.ECORev;

		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ECOMaint, AMECOItem>());
		protected static void Configure(WorkflowContext<ECOMaint, AMECOItem> context)
		{
			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						actions.Add<ECOMaintECCExt>(g => g.BOMCompare, c => c.WithCategory(PredefinedCategory.Inquiries));
					});
			});
		}
	}
}
