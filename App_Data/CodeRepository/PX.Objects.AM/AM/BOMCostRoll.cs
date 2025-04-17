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
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;
using PX.Objects.Common;
using PX.Objects.Common.GraphExtensions;

namespace PX.Objects.AM
{
	public class BOMCostRoll : PXGraph<BOMCostRoll>
	{
        public class CurySettings : CurySettingsExtension<BOMCostRoll, InventoryItem, InventoryItemCurySettings>
        { // To update pending cost in the InventoryItemCurySettings table.
            public static bool IsActive() => true;
        }

        public PXFilter<RollupSettings> Settings;
        [PXViewName(Messages.BOMCost)]
        public PXSelectJoin<AMBomCost,
            InnerJoin<AMBomItem, On<AMBomCost.bOMID, Equal<AMBomItem.bOMID>,
                And<AMBomCost.revisionID, Equal<AMBomItem.revisionID>>>>,
            Where<AMBomCost.userID, Equal<Current<AccessInfo.userID>>,
				And<AMBomCost.curyID, Equal<Current<AccessInfo.baseCuryID>>>>> BomCostRecs;
        [PXHidden]
        public PXSelect<InventoryItem> InvItemRecs;
		[PXHidden]
        public PXSelect<INItemSite> ItemSiteRecs;
        //Required to update InventoryItem
        public PXSetup<CommonSetup> CommonSetupView;

        //Required when updating InventoryItem and/or INItemSite
        public PXSetup<INSetup> INSetup;

        //Required to control Visibility of the Archive button
        public PXSetup<AMBSetup> AMBomSetup;
		[PXHidden]
        public PXSelect<AMBomCostHistory> BomCostHistoryRecs;

		public bool MBCEnabled => PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();

		public BOMCostRoll()
        {
            BomCostRecs.AllowDelete = false;
            BomCostRecs.AllowInsert = false;
            PXUIFieldAttribute.SetEnabled(BomCostRecs.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<AMBomCost.selected>(BomCostRecs.Cache, null, true);
            ViewBOM.SetEnabled(true);

            Archive.SetVisible(AMBomSetup?.Current?.AllowArchiveWithoutUpdatePending == true);
			PXUIFieldAttribute.SetVisible<RollupSettings.siteID>(Settings.Cache, null, !MBCEnabled);
        }

		#region CACHEATTACHED

        [PXInt]
        protected virtual void _(Events.CacheAttached<InventoryItem.parentItemClassID> e) { }

        [PXDBInt]
        [PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void _(Events.CacheAttached<InventoryItem.itemClassID> e) { }

		#endregion

		#region ACTIONS

		public PXAction<RollupSettings> ViewBOM;

        [PXUIField(DisplayName = "View BOM")]
        [PXButton]
        protected virtual void viewBOM()
        {
            BOMMaint.Redirect(BomCostRecs?.Current?.BOMID, BomCostRecs?.Current?.RevisionID);
        }

        public PXAction<RollupSettings> start;
        [PXUIField(DisplayName = Messages.RollCosts)]
        [PXProcessButton]
        protected virtual IEnumerable Start(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, () => RollCostsRetry(this.Settings.Current));
            return adapter.Get();
        }

        public PXAction<RollupSettings> Archive;

        [PXUIField(DisplayName = "Archive")]
        [PXButton]
        protected IEnumerable archive(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, ArchiveBomCostRecords);
            return adapter.Get();
        }

        public PXAction<RollupSettings> updpnd;
        [PXUIField(DisplayName = Messages.UpdatePending)]
        [PXProcessButton]
        protected virtual IEnumerable UpdPnd(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, UpdatePending);
            return adapter.Get();
        }

		#endregion

        /// <summary>
        /// Indicates if the process is running for multi level
        /// </summary>
        public virtual bool IsMultiLevel => Settings?.Current != null && Settings.Current.SnglMlti == RollupSettings.SelectOptSM.Multi;

        /// <summary>
        /// Perform cost roll with retry on PXExceptions
        /// </summary>
        /// <param name="filter"></param>
        public static void RollCostsRetry(RollupSettings filter)
        {
            var retryCnt = 1;
            while (true)
            {
                try
                {
                    RollCosts(filter);
                    break;
                }
				catch (PXOperationCompletedException)
				{
					throw;
				}
                catch (PXException pe)
                {
                    if (retryCnt-- < 0)
                    {
                        throw;
                    }

                    PXTrace.WriteError(pe);
                }
            }
        }

        public static void RollCosts(RollupSettings filter)
        {
            var costRollGraph = CreateInstance<BOMCostRoll>();
            costRollGraph.Settings.Current = filter;
            costRollGraph.RollCosts();
        }

        protected virtual void RollCosts()
        {
            var isPersistMode = Settings?.Current?.IsPersistMode == true;
            if (isPersistMode)
            {
                DeleteUserCostRollData();
            }

            this.BomCostRecs.Cache.Clear();
#if DEBUG
			var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
			var processedAll = RollCosts(GetMultiLevelData(Settings?.Current));
			if (isPersistMode)
			{
				Actions.PressSave();
			}

#if DEBUG
			sw.Stop();
			AMDebug.TraceWriteLine("CostRoll - Total Process Time: {0}", PXTraceHelper.CreateTimespanMessage(sw.Elapsed));
#endif
			var warningMsg = string.Empty;

            if (!processedAll || Settings?.Current?.FoundRecursiveBom == true)
            {
                warningMsg = Messages.GetLocal(Messages.InvalidValuesOnOneOrMoreBOMS);
            }

            if(string.IsNullOrWhiteSpace(warningMsg))
            {
                return;
            }

            if (IsImport || IsContractBasedAPI)
            {
                PXTrace.WriteWarning(warningMsg);
                return;
            }

            throw new PXOperationCompletedException(warningMsg);
        }

		protected virtual List<MultiLevelBomResult> GetMultiLevelData(RollupSettings costRollSettings)
		{
			// override in extension
			return null;
		}

		/// <summary>
		/// Update the pending standard cost for each BOM cost item
		/// </summary>
		protected virtual void UpdatePending()
        {
            foreach (PXResult<INItemSite, AMBomCost, InventoryItem> result in PXSelectJoin<
                    INItemSite,
                    InnerJoin<AMBomCost,
                        On<INItemSite.inventoryID, Equal<AMBomCost.inventoryID>>,
                    InnerJoin<InventoryItem,
                        On<AMBomCost.inventoryID, Equal<InventoryItem.inventoryID>>>>,
                    Where<AMBomCost.userID, Equal<Current<AccessInfo.userID>>,
                        And<InventoryItem.valMethod, Equal<INValMethod.standard>,
                        And<Where<InventoryItemExt.aMBOMID, Equal<AMBomCost.bOMID>,
                            Or<InventoryItemExt.aMBOMID, IsNull>>>>>,
                    OrderBy<
                        Asc<INItemSite.inventoryID,
                            Asc<AMBomCost.isDefaultBom,
                                Asc<AMBomCost.bOMID,
                                    Asc<AMBomCost.revisionID,
                                        Asc<INItemSite.siteID>>>>>>>
                .Select(this))
            {
                var bomCost = (AMBomCost) result;
                var inventoryItem = (InventoryItem) result;
                var itemSite = (INItemSite) result;

                if (bomCost.Selected != true || bomCost?.InventoryID == null || bomCost.UnitCost.GetValueOrDefault() < 0 || inventoryItem?.InventoryID == null || itemSite?.InventoryID == null)
                {
                    continue;
                }

                inventoryItem.PendingStdCost = bomCost.UnitCost.GetValueOrDefault();
				inventoryItem.DfltReceiptLocationID = itemSite.DfltReceiptLocationID.GetValueOrDefault();
				inventoryItem.PendingStdCostDate = Accessinfo.BusinessDate;
                InvItemRecs.Update(inventoryItem);
#if DEBUG
                INSite inSite = PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(this, itemSite.SiteID);
                if (inSite != null)
                {
                    AMDebug.TraceWriteMethodName($"Updating item {inventoryItem.InventoryCD.TrimIfNotNullEmpty()} at warehouse {inSite.SiteCD.TrimIfNotNullEmpty()} pending cost to {bomCost.UnitCost.GetValueOrDefault()} from BOM {bomCost.BOMID.TrimIfNotNullEmpty()}");
                }
                else
                {
                    AMDebug.TraceWriteMethodName("No INSite record found");
                }
#endif

                itemSite.PendingStdCost = bomCost.UnitCost.GetValueOrDefault();
                itemSite.PendingStdCostDate = Accessinfo.BusinessDate;
                ItemSiteRecs.Update(itemSite);
            }

            UpdateItemSiteStdCostOverrides();

            if (AMBomSetup.Current.AutoArchiveWhenUpdatePending == true)
            {
                ArchiveBomCostRecords();
            }

            Actions.PressSave();
        }

        /// <summary>
        /// Handle standard cost overrides at the site level using primary bom for the site
        /// </summary>
        protected virtual void UpdateItemSiteStdCostOverrides()
        {
            foreach (PXResult<AMBomCost, INItemSite> result in PXSelectJoin<
                AMBomCost,
                InnerJoin<INItemSite,
                    On<AMBomCost.inventoryID, Equal<INItemSite.inventoryID>,
                    And<AMBomCost.siteID, Equal<INItemSite.siteID>>>>,
                Where<AMBomCost.userID, Equal<Current<AccessInfo.userID>>,
                    And<INItemSite.valMethod, Equal<INValMethod.standard>,
                    And<INItemSite.stdCostOverride, Equal<boolTrue>,
                    And<Where<INItemSiteExt.aMBOMID, Equal<AMBomCost.bOMID>,
                        Or<INItemSiteExt.aMBOMID, IsNull>>>>>>>
                .Select(this))
            {
                var amBomCost = (AMBomCost)result;
                var inItemSite = (INItemSite)result;

                if (amBomCost?.InventoryID == null || amBomCost.UnitCost.GetValueOrDefault() < 0 ||
                    amBomCost.SiteID == null || inItemSite?.InventoryID == null || inItemSite.SiteID == null)
                {
                    continue;
                }
#if DEBUG
                INSite inSite = PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(this, inItemSite.SiteID);
                if (inSite != null)
                {
                    AMDebug.TraceWriteMethodName($"Updating item ID [{amBomCost.InventoryID}] at warehouse {inSite.SiteCD.TrimIfNotNullEmpty()} pending cost to {amBomCost.UnitCost.GetValueOrDefault()} from BOM {amBomCost.BOMID.TrimIfNotNullEmpty()}");
                }
                else
                {
                    AMDebug.TraceWriteMethodName("No INSite record found");
                }
#endif

                INItemSite itemSite = this.ItemSiteRecs.Locate(inItemSite) ?? inItemSite;
                itemSite.PendingStdCost = amBomCost.UnitCost.GetValueOrDefault();
                itemSite.PendingStdCostDate = Accessinfo.BusinessDate;
                ItemSiteRecs.Update(itemSite);
            }
        }

        protected virtual bool ProcessCost(AMBomItem bomItem, int level, bool isDefault, InventoryItem inventoryItem, INItemSite itemSite)
        {
            var successful = true;

            if (bomItem?.BOMID == null)
            {
                return false;
            }

            var bomcostrec = new AMBomCost
            {
                InventoryID = bomItem.InventoryID,
                SubItemID = bomItem.SubItemID,
                BOMID = bomItem.BOMID,
                RevisionID = bomItem.RevisionID,
                SiteID = bomItem.SiteID,
                MultiLevelProcess = Settings.Current.SnglMlti == RollupSettings.SelectOptSM.Multi,
                UserID = this.Accessinfo.UserID,
                Level = level,
                // Might have to update later for subitem indication - currently only looks at INItemSite default BOM ID
                IsDefaultBom = isDefault,
				ItemClassID = inventoryItem?.ItemClassID,
				StdCost = itemSite?.StdCost ?? 0m,
				PendingStdCost = itemSite?.PendingStdCost ?? 0m,
				CuryID = Accessinfo.BaseCuryID
            };

			if(inventoryItem == null)
			{
				inventoryItem = InventoryHelper.CacheQueryInventoryItem(InvItemRecs.Cache, bomcostrec.InventoryID);
				bomcostrec.ItemClassID = inventoryItem?.ItemClassID;
			}

			if(itemSite == null)
			{
				itemSite = InventoryHelper.CacheQueryINItemSite(ItemSiteRecs.Cache, bomcostrec.InventoryID, bomcostrec.SiteID);
				bomcostrec.StdCost = itemSite?.StdCost ?? 0m;
				bomcostrec.PendingStdCost = itemSite?.PendingStdCost ?? 0m;
			}

            // Set Lot Size based on Filter Settings
            if(Settings.Current.IgnoreMinMaxLotSizeValues == true)
            {
                bomcostrec.LotSize = 1;
            }
            else if(bomcostrec.BOMID == Settings.Current.BOMID && Settings.Current.LotSize.GetValueOrDefault() != 0
                && Settings.Current.IgnoreMinMaxLotSizeValues == false)
            {
                bomcostrec.LotSize = Settings.Current.LotSize.GetValueOrDefault();
            }
            else
            {
                bomcostrec.LotSize = inventoryItem == null ? 
                    InventoryHelper.GetMfgReorderQty(this, bomcostrec.InventoryID, bomcostrec.SiteID) :
                    InventoryHelper.GetMfgReorderQty(this, inventoryItem, itemSite);
            }

            if (bomcostrec.LotSize.GetValueOrDefault() <= 0)
            {
                bomcostrec.LotSize = 1;
            }

            bomcostrec.FLaborCost = 0;
            bomcostrec.VLaborCost = 0;
            var laborCostAndHours = SetLaborCost(ref bomcostrec, Settings.Current?.IncFixed == true);

            bomcostrec.MachCost = GetMachineCost(bomcostrec);

            bomcostrec.ToolCost = GetToolCost(bomcostrec);

            var allMaterial = PXSelectReadonly2<AMBomMatl,
                InnerJoin<InventoryItem, On<AMBomMatl.inventoryID, Equal<InventoryItem.inventoryID>>,
                LeftJoin<INItemSite, On<AMBomMatl.inventoryID, Equal<INItemSite.inventoryID>,
                      And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>>,
                Where<AMBomMatl.bOMID, Equal<Required<AMBomMatl.bOMID>>,
                    And<AMBomMatl.revisionID, Equal<Required<AMBomMatl.revisionID>>
                    >>>.Select(this, bomcostrec.SiteID, bomcostrec.BOMID, bomcostrec.RevisionID);

            //Merge of Regular Material and Subcontract Material (excluding Reference/vendor supplied material)
            OperationCosts matlTotal = new OperationCosts();

            if (allMaterial.Count > 0)
            {
                var purchase = new List<PXResult<AMBomMatl, InventoryItem, INItemSite>>();
                var manufactured = new List<PXResult<AMBomMatl, InventoryItem, INItemSite>>();
                var subcontract = new List<PXResult<AMBomMatl, InventoryItem, INItemSite>>();
                var refMaterial = new List<PXResult<AMBomMatl, InventoryItem, INItemSite>>();

                foreach (PXResult<AMBomMatl, InventoryItem, INItemSite> result in allMaterial)
                {
                    var bomMatl = (AMBomMatl) result;
                    if(bomMatl == null || 
                        (bomMatl.EffDate != null && bomMatl.EffDate > Accessinfo.BusinessDate) ||
                        (bomMatl.ExpDate != null && bomMatl.ExpDate <= Accessinfo.BusinessDate))
                    {
                        continue;
                    }    

                    // Check for COMP BOMID, if exists, item is Manufactured
                    if (bomMatl.CompBOMID != null)
                    {
                        manufactured.Add(result);
                        continue;
                    }

                    if (bomMatl.MaterialType == AMMaterialType.Subcontract && bomMatl.SubcontractSource != AMSubcontractSource.VendorSupplied)
                    {
                        subcontract.Add(result);
                        continue;
                    }

                    if (bomMatl.MaterialType == AMMaterialType.Subcontract && bomMatl.SubcontractSource == AMSubcontractSource.VendorSupplied)
                    {
                        refMaterial.Add(result);
                        continue;
                    }

                    var replenishmentSource = GetReplenishmentSource((InventoryItem)result, (INItemSite)result);
                    if (replenishmentSource == INReplenishmentSource.Manufactured)
                    {
                        manufactured.Add(result);
                        continue;
                    }

                    purchase.Add(result);
                }

                var purchaseCost = GetMaterialCost(bomcostrec, purchase, IsMultiLevel, out var purchaseMatlMessages);
                var manufacturedCost = GetMaterialCost(bomcostrec, manufactured, IsMultiLevel, out var manufacturedMatlMessages);
                var subcontractCost = GetMaterialCost(bomcostrec, subcontract, IsMultiLevel, out var subContractMatlMessages);
                var refmaterialCost = GetMaterialCost(bomcostrec, refMaterial, IsMultiLevel, out var refMaterialMatlMessages);

                if (purchaseMatlMessages != null)
                {
                    foreach (var matlMessage in purchaseMatlMessages)
                    {
                        successful = false;
                        PXTrace.WriteWarning(matlMessage);
                    }
                }

                if (manufacturedMatlMessages != null)
                {
                    foreach (var matlMessage in manufacturedMatlMessages)
                    {
                        successful = false;
                        PXTrace.WriteWarning(matlMessage);
                    }
                }

                if (subContractMatlMessages != null)
                {
                    foreach (var matlMessage in subContractMatlMessages)
                    {
                        successful = false;
                        PXTrace.WriteWarning(matlMessage);
                    }
                }

                if (refMaterialMatlMessages != null)
                {
                    foreach (var matlMessage in refMaterialMatlMessages)
                    {
                        successful = false;
                        PXTrace.WriteWarning(matlMessage);
                    }
                }

                bomcostrec.MatlManufacturedCost = manufacturedCost?.TotalCost ?? 0m;
                bomcostrec.MatlNonManufacturedCost = purchaseCost?.TotalCost ?? 0m;
                bomcostrec.SubcontractMaterialCost = subcontractCost?.TotalCost ?? 0m;
                bomcostrec.ReferenceMaterialCost = refmaterialCost?.TotalCost ?? 0m;

                matlTotal = new OperationCosts(manufacturedCost);
                matlTotal.Add(purchaseCost, true);
                matlTotal.Add(subcontractCost, true);
            }

            bomcostrec.FOvdCost = 0;
            bomcostrec.VOvdCost = 0;
            SetOverheadCosts(ref bomcostrec, Settings.Current.IncFixed.GetValueOrDefault(), matlTotal, laborCostAndHours.Item1, laborCostAndHours.Item2);

            bomcostrec.TotalCost = bomcostrec.FLaborCost.GetValueOrDefault()
                + bomcostrec.VLaborCost.GetValueOrDefault()
                + bomcostrec.MachCost.GetValueOrDefault()
                + bomcostrec.MatlManufacturedCost.GetValueOrDefault()
                + bomcostrec.MatlNonManufacturedCost.GetValueOrDefault()
                + bomcostrec.FOvdCost.GetValueOrDefault()
                + bomcostrec.VOvdCost.GetValueOrDefault()
                + bomcostrec.ToolCost.GetValueOrDefault()
                + bomcostrec.OutsideCost.GetValueOrDefault()
                + bomcostrec.DirectCost.GetValueOrDefault()
                + bomcostrec.SubcontractMaterialCost.GetValueOrDefault()
                + bomcostrec.ReferenceMaterialCost.GetValueOrDefault();

            bomcostrec.UnitCost = UomHelper.PriceCostRound(bomcostrec.TotalCost.GetValueOrDefault() / bomcostrec.LotSize.GetValueOrDefault());

            try
            {
                BomCostRecs.Insert(bomcostrec);
            }
            catch (Exception e)
            {
                if (e is PXOuterException)
                {
                    PXTraceHelper.PxTraceOuterException((PXOuterException)e, PXTraceHelper.ErrorLevel.Error);
                }

                InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                        .Select(this, bomcostrec.InventoryID);

                if (item == null)
                {
                    PXTrace.WriteInformation(Messages.InvalidInventoryIDOnBOM, bomItem.BOMID);
                    successful = false;
                }
                else
                {
                    throw new PXException(Messages.GetLocal(Messages.UnableToSaveRecordForInventoryID), Messages.GetLocal(Messages.BOMCost), item.InventoryCD.Trim(), e.Message);
                }

            }

            return successful;
        }

		protected virtual string GetReplenishmentSource(InventoryItem item, int? siteID)
		{
			return GetReplenishmentSource(item, INItemSite.PK.Find(this, item?.InventoryID, siteID));
		}

		protected virtual string GetReplenishmentSource(InventoryItem invItem, INItemSite inItemSite)
		{
			// Override in multi level extension
			return null;
		}

        /// <summary>
        /// Calculate the total tool cost for a BOM
        /// </summary>
        protected virtual decimal? GetToolCost(AMBomCost amBomCost)
        {
            var vartool = 0m;
            foreach (AMBomTool bomToolRec in PXSelect<
                AMBomTool,
                Where<AMBomTool.bOMID, Equal<Required<AMBomTool.bOMID>>,
                    And<AMBomTool.revisionID, Equal<Required<AMBomTool.revisionID>>>>>
                .Select(this, amBomCost.BOMID, amBomCost.RevisionID))
            {
				var toolCury = AMBomToolCury.PK.Find(this, bomToolRec.BOMID, bomToolRec.RevisionID, bomToolRec.OperationID, bomToolRec.LineID, Accessinfo.BaseCuryID);
                vartool += (toolCury?.UnitCost ?? 0) * bomToolRec.QtyReq.GetValueOrDefault() * amBomCost.LotSize.GetValueOrDefault();
            }
            return UomHelper.PriceCostRound(vartool);
        }

        /// <summary>
        /// Update the current BOM Cost row with the correct calculated overhead costs
        /// </summary>
        /// <param name="currentAMBomCost">BOM Cost row being updated</param>
        /// <param name="includeFixValues">Should the fixed overheads be included</param>
        /// <param name="operationMatlCosts">previously calculated material costs</param>
        /// <param name="operationLaborCosts">previously calculated labor costs</param>
        /// <param name="operationLaborHours">previously calculated labor hours</param>
        protected virtual void SetOverheadCosts(ref AMBomCost currentAMBomCost, bool includeFixValues,
            OperationCosts operationMatlCosts, OperationCosts operationLaborCosts, OperationCosts operationLaborHours)
        {
            SetWorkCenterOverheadCosts(ref currentAMBomCost, includeFixValues,
                operationMatlCosts, operationLaborCosts, operationLaborHours);

            SetBomOverheadCosts(ref currentAMBomCost, includeFixValues,
                operationMatlCosts, operationLaborCosts, operationLaborHours);
        }

        /// <summary>
        /// Update the current BOM Cost row with the correct calculated BOM Only overhead costs
        /// </summary>
        /// <param name="currentAMBomCost">BOM Cost row being updated</param>
        /// <param name="includeFixValues">Should the fixed overheads be included</param>
        /// <param name="operationMatlCosts">previously calculated material costs</param>
        /// <param name="operationLaborCosts">previously calculated labor costs</param>
        /// <param name="operationLaborHours">previously calculated labor hours</param>
        protected virtual void SetBomOverheadCosts(ref AMBomCost currentAMBomCost, bool includeFixValues,
            OperationCosts operationMatlCosts, OperationCosts operationLaborCosts, OperationCosts operationLaborHours)
        {
            foreach (PXResult<AMBomOper, AMBomOvhd, AMOverhead, AMOverheadCurySettings> result in PXSelectJoin<
                AMBomOper,
                InnerJoin<AMBomOvhd,
                    On<AMBomOper.bOMID, Equal<AMBomOvhd.bOMID>,
                    And<AMBomOper.revisionID, Equal<AMBomOvhd.revisionID>,
                    And<AMBomOper.operationID, Equal<AMBomOvhd.operationID>>>>,
                InnerJoin<AMOverhead,
                    On<AMBomOvhd.ovhdID, Equal<AMOverhead.ovhdID>>,
				InnerJoin<AMOverheadCurySettings,
					On<AMOverheadCurySettings.ovhdID, Equal<AMOverhead.ovhdID>,
						And<AMOverheadCurySettings.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>>>,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>>>>
                .Select(this, currentAMBomCost.BOMID, currentAMBomCost.RevisionID))
            {
                var amBomOper = (AMBomOper)result;
                var amBomOvhd = (AMBomOvhd)result;
                var amOverhead = (AMOverhead)result;
				var amOverheadCury = (AMOverheadCurySettings)result;

                if (string.IsNullOrEmpty(amBomOper?.BOMID)
                   || string.IsNullOrWhiteSpace(amBomOvhd?.OvhdID)
                   || string.IsNullOrWhiteSpace(amOverhead?.OvhdID))
                {
                    continue;
                }

                var overheadCost = CalculateOverheadCost(amOverhead, amBomOvhd, amBomOper, currentAMBomCost.LotSize.GetValueOrDefault(),
                    operationMatlCosts, operationLaborCosts, operationLaborHours, amOverheadCury);

                SetBomCostOverheadValues(ref currentAMBomCost, amOverhead, overheadCost, includeFixValues);
            }
        }

        /// <summary>
        /// Sets the correct overhead field of the BOM Cost row
        /// </summary>
        /// <param name="currentAMBomCost">BOM Cost row being updated</param>
        /// <param name="amOverhead">Overhead master row</param>
        /// <param name="overheadCost">calculated overhead cost</param>
        /// <param name="includeFixValues">Should the fixed overheads be included</param>
        protected virtual void SetBomCostOverheadValues(ref AMBomCost currentAMBomCost, AMOverhead amOverhead, decimal overheadCost, bool includeFixValues)
        {
            if (amOverhead.OvhdType == OverheadType.FixedType)
            {
                currentAMBomCost.FOvdCost += includeFixValues ? overheadCost : 0;
                return;
            }

            currentAMBomCost.VOvdCost += overheadCost;
        }

        /// <summary>
        /// Update the current BOM Cost row with the correct calculated Operation Work Center Only overhead costs
        /// </summary>
        /// <param name="currentAMBomCost">BOM Cost row being updated</param>
        /// <param name="includeFixValues">Should the fixed overheads be included</param>
        /// <param name="operationMatlCosts">previously calculated material costs</param>
        /// <param name="operationLaborCosts">previously calculated labor costs</param>
        /// <param name="operationLaborHours">previously calculated labor hours</param>
        protected virtual void SetWorkCenterOverheadCosts(ref AMBomCost currentAMBomCost, bool includeFixValues,
            OperationCosts operationMatlCosts, OperationCosts operationLaborCosts, OperationCosts operationLaborHours)
        {
            foreach (PXResult<AMBomOper, AMWC, AMWCOvhd, AMOverhead, AMOverheadCurySettings> result in PXSelectJoin<
                AMBomOper,
                InnerJoin<AMWC,
                    On<AMBomOper.wcID, Equal<AMWC.wcID>>,
                InnerJoin<AMWCOvhd,
                    On<AMWC.wcID, Equal<AMWCOvhd.wcID>>,
                InnerJoin<AMOverhead,
                    On<AMWCOvhd.ovhdID, Equal<AMOverhead.ovhdID>>,
				LeftJoin<AMOverheadCurySettings, On<AMOverheadCurySettings.ovhdID, Equal<AMOverhead.ovhdID>,
					And<AMOverheadCurySettings.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>>>>,
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>>>>
                .Select(this, currentAMBomCost.BOMID, currentAMBomCost.RevisionID))
            {
                var amBomOper = (AMBomOper)result;
                var amWCOvhd = (AMWCOvhd)result;
                var amOverhead = (AMOverhead)result;
				var ovhdCury = (AMOverheadCurySettings)result;

                if (string.IsNullOrEmpty(amBomOper?.BOMID)
                   || string.IsNullOrWhiteSpace(amWCOvhd?.OvhdID)
                   || string.IsNullOrWhiteSpace(amOverhead?.OvhdID))
                {
                    continue;
                }

                var overheadCost = CalculateOverheadCost(amOverhead, amWCOvhd, amBomOper, currentAMBomCost.LotSize.GetValueOrDefault(),
                    operationMatlCosts, operationLaborCosts, operationLaborHours, ovhdCury);

                SetBomCostOverheadValues(ref currentAMBomCost, amOverhead, overheadCost, includeFixValues);
            }
        }

        /// <summary>
        /// Calculate the overhead costs based on a work center overhead row
        /// </summary>
        /// <returns>Calculated overhead cost</returns>
        protected virtual decimal CalculateOverheadCost(AMOverhead amOverhead, AMWCOvhd amWCOvhd, AMBomOper amBomOper, decimal quantity,
            OperationCosts operationMatlCosts, OperationCosts operationLaborCosts, OperationCosts operationLaborHours, AMOverheadCurySettings ovhdCury)
        {
            var baseOverheadCost = amWCOvhd.OFactor.GetValueOrDefault() * ovhdCury?.CostRate ?? 0;
            return CalculateOverheadCost(baseOverheadCost, amOverhead.OvhdType, quantity,
                operationLaborHours?.OperationCost(amBomOper.OperationID) ?? 0,
                operationLaborCosts?.OperationCost(amBomOper.OperationID) ?? 0,
                operationMatlCosts?.OperationCost(amBomOper.OperationID) ?? 0,
                amBomOper.GetMachineUnitsPerHour() == 0m ? 0m : quantity / amBomOper.GetMachineUnitsPerHour());
        }

        /// <summary>
        /// Calculate the overhead costs based on a BOM overhead row
        /// </summary>
        /// <returns>Calculated overhead cost</returns>
        protected virtual decimal CalculateOverheadCost(AMOverhead amOverhead, AMBomOvhd amBomOvhd, AMBomOper amBomOper, decimal quantity,
            OperationCosts operationMatlCosts, OperationCosts operationLaborCosts, OperationCosts operationLaborHours, AMOverheadCurySettings ovhdCury)
        {
            var baseOverheadCost = amBomOvhd.OFactor.GetValueOrDefault() * ovhdCury?.CostRate ?? 0;
            return CalculateOverheadCost(baseOverheadCost, amOverhead.OvhdType, quantity,
                operationLaborHours?.OperationCost(amBomOper.OperationID) ?? 0,
                operationLaborCosts?.OperationCost(amBomOper.OperationID) ?? 0,
                operationMatlCosts?.OperationCost(amBomOper.OperationID) ?? 0,
                amBomOper.GetMachineUnitsPerHour() == 0m ? 0m : quantity / amBomOper.GetMachineUnitsPerHour());
        }

        /// <summary>
        /// Calculate the overhead costs based on type and correct variables
        /// </summary>
        /// <returns>Calculated overhead cost</returns>
        protected virtual decimal CalculateOverheadCost(decimal baseOverheadCost, string overheadType,
            decimal quantity, decimal laborHours, decimal laborCosts, decimal materialCosts, decimal machineHours)
        {
            switch (overheadType)
            {
                case OverheadType.VarQtyTot:
                case OverheadType.VarQtyComp:
                    return baseOverheadCost * quantity;

                case OverheadType.VarLaborHrs:
                    return baseOverheadCost * laborHours;

                case OverheadType.VarLaborCost:
                    return baseOverheadCost * laborCosts;

                case OverheadType.VarMatlCost:
                    return baseOverheadCost * materialCosts;

                case OverheadType.VarMachHrs:
                    return baseOverheadCost * machineHours;

                case OverheadType.FixedType:
                    return baseOverheadCost;
            }

            return 0;
        }

        /// <summary>
        /// Update the BOM Material unit cost into the cache
        /// </summary>
        /// <param name="amBomMatl">BOM Material Row</param>
        /// <param name="unitCost">Unit cost to use in the update process</param>
        protected virtual void UpdateMaterialUnitCost(AMBomMatl amBomMatl, decimal? unitCost)
        {
            if (amBomMatl == null || unitCost == null)
            {
                return;
            }

			if(!PXDatabase.Update<AMBOMCurySettings>(
				new PXDataFieldRestrict<AMBOMCurySettings.bOMID>(PXDbType.NVarChar, 15, amBomMatl.BOMID),
				new PXDataFieldRestrict<AMBOMCurySettings.revisionID>(PXDbType.NVarChar, 10, amBomMatl.RevisionID),
				new PXDataFieldRestrict<AMBOMCurySettings.operationID>(PXDbType.Int, amBomMatl.OperationID),
				new PXDataFieldRestrict<AMBOMCurySettings.lineID>(PXDbType.Int, amBomMatl.LineID),
				new PXDataFieldRestrict<AMBOMCurySettings.lineType>(PXDbType.NChar, 1, BOMCurySettingsLineType.Material),
				new PXDataFieldRestrict<AMBOMCurySettings.curyID>(PXDbType.NVarChar, 10, Accessinfo.BaseCuryID),
				new PXDataFieldAssign<AMBOMCurySettings.unitCost>(PXDbType.Decimal, unitCost.GetValueOrDefault()),
				new PXDataFieldAssign<AMBOMCurySettings.lastModifiedDateTime>(PXDbType.DateTime, Common.Dates.UtcNow),
				new PXDataFieldAssign<AMBOMCurySettings.lastModifiedByScreenID>(PXDbType.Char, 8, Accessinfo?.ScreenID.Replace(".", "")),
				new PXDataFieldAssign<AMBOMCurySettings.lastModifiedByID>(PXDbType.UniqueIdentifier, Accessinfo?.UserID)
				))
			{
				PXDatabase.Insert<AMBOMCurySettings>(
					new PXDataFieldAssign<AMBOMCurySettings.bOMID>(PXDbType.NVarChar, 15, amBomMatl.BOMID),
					new PXDataFieldAssign<AMBOMCurySettings.revisionID>(PXDbType.NVarChar, 10, amBomMatl.RevisionID),
					new PXDataFieldAssign<AMBOMCurySettings.operationID>(PXDbType.Int, amBomMatl.OperationID),
					new PXDataFieldAssign<AMBOMCurySettings.lineID>(PXDbType.Int, amBomMatl.LineID),
					new PXDataFieldAssign<AMBOMCurySettings.lineType>(PXDbType.NChar, 1, BOMCurySettingsLineType.Material),
					new PXDataFieldAssign<AMBOMCurySettings.curyID>(PXDbType.NVarChar, 10, Accessinfo.BaseCuryID),
					new PXDataFieldAssign<AMBOMCurySettings.unitCost>(PXDbType.Decimal, unitCost.GetValueOrDefault()),
					new PXDataFieldAssign<AMBOMCurySettings.createdDateTime>(PXDbType.DateTime, Common.Dates.UtcNow),
					new PXDataFieldAssign<AMBOMCurySettings.createdByScreenID>(PXDbType.Char, 8, Accessinfo?.ScreenID.Replace(".", "")),
					new PXDataFieldAssign<AMBOMCurySettings.createdByID>(PXDbType.UniqueIdentifier, Accessinfo?.UserID),
					new PXDataFieldAssign<AMBOMCurySettings.lastModifiedDateTime>(PXDbType.DateTime, Common.Dates.UtcNow),
					new PXDataFieldAssign<AMBOMCurySettings.lastModifiedByScreenID>(PXDbType.Char, 8, Accessinfo?.ScreenID.Replace(".", "")),
					new PXDataFieldAssign<AMBOMCurySettings.lastModifiedByID>(PXDbType.UniqueIdentifier, Accessinfo?.UserID)
					);
			}

            PXDatabase.Update<AMBomMatl>(                
                new PXDataFieldAssign<AMBomMatl.lastModifiedDateTime>(PXDbType.DateTime, Common.Dates.UtcNow),
                new PXDataFieldAssign<AMBomMatl.lastModifiedByScreenID>(PXDbType.Char, 8, Accessinfo?.ScreenID.Replace(".", "")),
                new PXDataFieldAssign<AMBomMatl.lastModifiedByID>(PXDbType.UniqueIdentifier, Accessinfo?.UserID),
                new PXDataFieldRestrict<AMBomMatl.bOMID>(PXDbType.NVarChar, 15, amBomMatl.BOMID),
                new PXDataFieldRestrict<AMBomMatl.revisionID>(PXDbType.NVarChar, 10, amBomMatl.RevisionID),
                new PXDataFieldRestrict<AMBomMatl.operationID>(PXDbType.Int, amBomMatl.OperationID),
                new PXDataFieldRestrict<AMBomMatl.lineID>(PXDbType.Int, amBomMatl.LineID));
        }

        /// <summary>
        /// Get the total BOM/Operations material cost
        /// </summary>
        protected virtual OperationCosts GetMaterialCost(AMBomCost currentAMBomCost, IEnumerable<PXResult<AMBomMatl, InventoryItem, INItemSite>> material, bool isMultLevel, out List<string> materialMessage)
        {
            materialMessage = new List<string>();

            if (currentAMBomCost == null || string.IsNullOrWhiteSpace(currentAMBomCost.BOMID) || material == null)
            {
                return null;
            }

            var operationMaterialCosts = new OperationCosts();
            foreach(var result in material)
            {
				var matlRec = result.GetItem<AMBomMatl>();
				var inventoryItem = result.GetItem<InventoryItem>();
				var itemSite = result.GetItem<INItemSite>();

                if (matlRec?.BOMID == null || inventoryItem?.InventoryCD == null)
                {
                    continue;
                }

                decimal? unitcost = null;
                var matlSiteID = matlRec.SiteID ?? currentAMBomCost.SiteID;
                if (isMultLevel)
                {
					var replenishmentSource = GetReplenishmentSource(inventoryItem, itemSite);
					if (replenishmentSource == INReplenishmentSource.Manufactured)
					{
						var bomItem = PrimaryBomIDManager.GetNotArchivedRevisionBomItem(this,
							new PrimaryBomIDManager(this)?.GetPrimaryAllLevels(inventoryItem, itemSite, matlRec.SubItemID));
						unitcost = GetCurrentBomCost(bomItem?.BOMID, bomItem?.RevisionID);
					}
#if DEBUG
                    if (unitcost != null)
                    {
                        AMDebug.TraceWriteMethodName($"Item {inventoryItem.InventoryCD} ({matlRec.InventoryID}) on BOM {currentAMBomCost.BOMID}-{currentAMBomCost.RevisionID} using bom cost value of {unitcost}");
                    }
#endif
                }

                if (unitcost == null)
                {
                    unitcost = GetUnitCost(inventoryItem, itemSite, matlSiteID);
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Item {inventoryItem.InventoryCD} ({matlRec.InventoryID}) on BOM {currentAMBomCost.BOMID}-{currentAMBomCost.RevisionID} using inventory cost value of {unitcost}");
#endif
                }

                var inUnit = (INUnit) PXSelectorAttribute.Select<AMBomMatl.uOM>(this.Caches<AMBomMatl>(), matlRec) ??
                             (INUnit) PXSelect<INUnit,
                                Where<INUnit.inventoryID, Equal<Required<INUnit.inventoryID>>,
                                   And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>
                                >>.Select(this, matlRec.InventoryID, matlRec.UOM);

                if (inUnit == null)
                {
                    materialMessage.Add(Messages.GetLocal(Messages.InvalidUOMForMaterialonBOM, matlRec.UOM.TrimIfNotNullEmpty(), inventoryItem.InventoryCD, matlRec.BOMID, matlRec.RevisionID));
                    continue;
                }

                if (UomHelper.TryConvertToBaseCost<AMBomMatl.inventoryID>(this.Caches[typeof(AMBomMatl)], matlRec, matlRec.UOM, unitcost.GetValueOrDefault(), out var matlUnitCost))
                {
                    unitcost = matlUnitCost.GetValueOrDefault();
                }

                var itemExt = inventoryItem.GetExtension<InventoryItemExt>();

                var totalQtyRequired = matlRec.QtyReq.GetValueOrDefault() *
                        (1 + (Settings.Current.IncMatScrp.GetValueOrDefault() ? matlRec.ScrapFactor.GetValueOrDefault() : 0m)) *
                        (matlRec.BatchSize.GetValueOrDefault() == 0m ? 1m :
                        currentAMBomCost.LotSize.GetValueOrDefault() / matlRec.BatchSize.GetValueOrDefault());

                totalQtyRequired = itemExt.AMQtyRoundUp == false ? totalQtyRequired : Math.Ceiling(totalQtyRequired);

                var matlCost = totalQtyRequired * unitcost.GetValueOrDefault();

                operationMaterialCosts.Add(matlRec.OperationID, matlCost, true);

                if (Settings.Current.UpdateMaterial.GetValueOrDefault())
                {
                    UpdateMaterialUnitCost(matlRec, unitcost);
                }
            }

            return operationMaterialCosts;
        }

        /// <summary>
        /// Get the current BOM unitcost calculated in the AMBomCost table/cache
        /// </summary>
        /// <param name="bomId">BOM ID of unit cost to search for</param>
        /// <param name="revisionId">BOM Revision of unit cost to search for</param>
        /// <returns></returns>
        protected virtual decimal? GetCurrentBomCost(string bomId, string revisionId)
        {
            if (string.IsNullOrWhiteSpace(bomId) || string.IsNullOrWhiteSpace(revisionId))
            {
                return null;
            }

            var foundAMBomCost = BomCostRecs.Locate(new AMBomCost
            {
                BOMID = bomId,
                RevisionID = revisionId,
                UserID = this.Accessinfo.UserID
            }) ?? PXSelect<
                    AMBomCost,
                    Where<AMBomCost.bOMID, Equal<Required<AMBomCost.bOMID>>,
                        And<AMBomCost.revisionID, Equal<Required<AMBomCost.revisionID>>,
                            And<AMBomCost.userID, Equal<Current<AccessInfo.userID>>>>>>
                .Select(this, bomId, revisionId);

            return foundAMBomCost?.UnitCost;
        }

        /// <summary>
        /// Get total BOM Machine cost
        /// </summary>
        /// <param name="amBomCost">BOM Cost row being processed</param>
        /// <returns></returns>
        protected virtual decimal? GetMachineCost(AMBomCost amBomCost)
        {
            var varmach = 0m;

            decimal machineMinutes = 0;

            foreach (AMBomOper operrec in this.Caches[typeof(AMBomOper)].Cached.RowCast<AMBomOper>()
                .Where(r => r.BOMID == amBomCost.BOMID && r.RevisionID == amBomCost.RevisionID))
            {
                if (operrec.MachineUnitTime.GetValueOrDefault() <= 0 || operrec.MachineUnits.GetValueOrDefault() <= 0)
                {
                    continue;
                }

                machineMinutes += operrec.MachineUnitTime.GetValueOrDefault() / operrec.MachineUnits.GetValueOrDefault();
				//TODO: remove the subquery and make this a single query

				foreach (PXResult<AMWCMach, AMMach, AMMachCurySettings, AMWCMachCury> result in PXSelectJoin<AMWCMach,
					InnerJoin<AMMach, On<AMWCMach.machID, Equal<AMMach.machID>>,
						LeftJoin<AMMachCurySettings, On<AMMachCurySettings.machID, Equal<AMMach.machID>,
							And<AMMachCurySettings.curyID, Equal<Current<AccessInfo.baseCuryID>>>>,
						LeftJoin<AMWCMachCury, On<AMWCMachCury.wcID, Equal<AMWCMach.wcID>,
							And<AMWCMachCury.detailID, Equal<AMWCMach.machID>,
							And<AMWCMachCury.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>>>>,
					Where<AMWCMach.wcID, Equal<Required<AMWCMach.wcID>>>
				>.Select(this, operrec.WcID))
				{
                    var wcMachine = (AMWCMach)result;
                    var machine = (AMMach)result;
					var machCury = (AMMachCurySettings)result;
					var wcMachCury = (AMWCMachCury)result;

					if (string.IsNullOrWhiteSpace(wcMachine?.MachID)
                        || string.IsNullOrWhiteSpace(machine?.MachID)
                        || !machine.ActiveFlg.GetValueOrDefault())
                    {
                        continue;
                    }

					decimal? standardCost = machCury?.StdCost ?? 0;
                    if (wcMachine.MachineOverride.GetValueOrDefault())
                    {						
						standardCost = wcMachCury?.StdCost ?? 0;
                    }

                    varmach += operrec.GetMachineUnitsPerHour() == 0m ? 0m
                        : standardCost.GetValueOrDefault() * amBomCost.LotSize.GetValueOrDefault() / operrec.GetMachineUnitsPerHour();
                }
            }

            // Set the machine Time Rounded to nearest Minute
            amBomCost.MachineTime = machineMinutes.ToCeilingInt();

            return UomHelper.PriceCostRound(varmach);
        }

        /// <summary>
        /// Converts the bom operation record into the total labor hours
        /// </summary>
        /// <param name="amBomOper"></param>
        /// <param name="includeSetupTime">Include the setup hours in the total</param>
        /// <returns>Total operation labor hours</returns>
        protected static decimal? GetLaborHours(AMBomOper amBomOper, bool includeSetupTime = true)
        {
            if (amBomOper == null
                || string.IsNullOrWhiteSpace(amBomOper.BOMID)
                || amBomOper.OperationID == null)
            {
                return null;
            }

            decimal? operSetupHrsPerPc = 0;
            decimal? operRunHrsPerPc = 0;

            operSetupHrsPerPc = amBomOper.SetupTime.ToHours();

            operRunHrsPerPc = amBomOper.GetRunUnitsPerHour() == 0m ? 0m : 1 / amBomOper.GetRunUnitsPerHour();

            if (includeSetupTime)
            {
                return operSetupHrsPerPc.GetValueOrDefault() + operRunHrsPerPc.GetValueOrDefault();
            }

            return operRunHrsPerPc.GetValueOrDefault();
        }

        /// <summary>
        /// Get the total labor cost and hours while updating the current ambomcost record with the correct labor values.
        /// </summary>
        /// <param name="currentAmBomCost">Current AMBomCost record that will be updated</param>
        /// <param name="includeFixValues">Indicates if fixed labor should be included in the calculations</param>
        /// <returns>Operation Labor Costs (Item1) and Operation Labor Hours (Item2)</returns>
        protected virtual Tuple<OperationCosts, OperationCosts> SetLaborCost(ref AMBomCost currentAmBomCost, bool includeFixValues)
        {
            var operationLaborCosts = new OperationCosts();
            var operationLaborHours = new OperationCosts();

            decimal fixedMinutes = 0;
            decimal variableMinutes = 0;

            foreach (PXResult<AMBomOper> result in PXSelect<
                AMBomOper,               
                Where<AMBomOper.bOMID, Equal<Required<AMBomOper.bOMID>>,
                    And<AMBomOper.revisionID, Equal<Required<AMBomOper.revisionID>>
                >>>.Select(this, currentAmBomCost.BOMID, currentAmBomCost.RevisionID))
            {
                var amBomOper = (AMBomOper)result;

                if (amBomOper?.OperationID == null
                    || string.IsNullOrWhiteSpace(amBomOper?.WcID))
                {
                    continue;
                }

                // Cache oper record for use later in other cost lookups
                Common.Cache.AddRowToCache(this, amBomOper);

                var laborHours = includeFixValues && amBomOper.SetupTime.GetValueOrDefault() > 0 ? amBomOper.SetupTime.ToHours() : 0;
                var wcStdCost = ShiftDiffType.GetShiftDifferentialCost(this, amBomOper.WcID);
                var laborCosts = includeFixValues && amBomOper.SetupTime.GetValueOrDefault() > 0 ? amBomOper.SetupTime.ToHours() * wcStdCost : 0;

                currentAmBomCost.FLaborCost += laborCosts;
                fixedMinutes += laborHours.GetValueOrDefault() * 60m;

                var varLaborHours = GetLaborHours(amBomOper, false) * currentAmBomCost.LotSize.GetValueOrDefault();
                variableMinutes += varLaborHours.GetValueOrDefault() * 60m;
                currentAmBomCost.VLaborCost += varLaborHours.GetValueOrDefault() * wcStdCost;

                laborCosts += varLaborHours.GetValueOrDefault() * wcStdCost;

                laborHours += varLaborHours.GetValueOrDefault();

                operationLaborHours.Add(amBomOper.OperationID, laborHours.GetValueOrDefault());
                operationLaborCosts.Add(amBomOper.OperationID, laborCosts.GetValueOrDefault());
            }

            // Set the Fixed and Variable Labor Time Rounded to nearest Minute
            currentAmBomCost.FixedLaborTime = fixedMinutes.ToCeilingInt();
            currentAmBomCost.VariableLaborTime = variableMinutes.ToCeilingInt();

            return new Tuple<OperationCosts, OperationCosts>(operationLaborCosts, operationLaborHours);
        }

        public static void UpdatePlannedMaterialCosts(PXGraph graph, AMProdItem amproditem)
        {
            if (amproditem == null || graph == null)
            {
                return;
            }

            var costRollGraph = PXGraph.CreateInstance<BOMCostRoll>();
            foreach (PXResult<AMProdMatl, InventoryItem> result in PXSelectJoin<AMProdMatl,
                InnerJoin<InventoryItem, On<AMProdMatl.inventoryID, Equal<InventoryItem.inventoryID>>>,
                    Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>>
                        >>.Select(graph, amproditem.OrderType, amproditem.ProdOrdID))
            {
                UpdatePlannedMaterialCost(graph, costRollGraph, amproditem, graph.Caches[typeof(AMProdMatl)].LocateElse((AMProdMatl)result), result);
            }
        }

        public static void UpdatePlannedMaterialCost(PXGraph callingGraph, BOMCostRoll costRollGraph, AMProdItem amproditem, AMProdMatl amProdMatl, InventoryItem inventoryItem)
        {
            if (callingGraph == null)
            {
                throw new PXArgumentException(nameof(callingGraph));
            }

            if (costRollGraph == null)
            {
                throw new PXArgumentException(nameof(costRollGraph));
            }

            if (amproditem == null || string.IsNullOrWhiteSpace(amproditem.ProdOrdID)
                                   || amProdMatl == null || string.IsNullOrWhiteSpace(amProdMatl.ProdOrdID) ||
                                   inventoryItem?.InventoryID == null)
            {
                return;
            }

            decimal? materialUnitCost = null;
            costRollGraph.Clear();
            costRollGraph.IsImport = true;
            if (costRollGraph?.Settings?.Current == null)
            {
                costRollGraph.Settings.Current = new RollupSettings();
            }
            costRollGraph.Settings.Current.IsPersistMode = false;
            costRollGraph.Settings.Current.ApplyPend = false;
            costRollGraph.Settings.Current.IncFixed = true;
            costRollGraph.Settings.Current.IncMatScrp = true;
            costRollGraph.Settings.Current.UpdateMaterial = false;
            costRollGraph.Settings.Current.SnglMlti = RollupSettings.SelectOptSM.Multi;

            int? siteID = amProdMatl.SiteID ?? amproditem.SiteID;

            //for production we do not want to roll standard cost items. Standard cost is standard cost.
            if (inventoryItem.ValMethod != INValMethod.Standard
                && costRollGraph.GetReplenishmentSource(inventoryItem, siteID) == INReplenishmentSource.Manufactured)
            {
                var totalQtyRequired = amProdMatl.TotalQtyRequired.GetValueOrDefault();
                if (totalQtyRequired <= 0)
                {
                    totalQtyRequired = 1;
                }

                costRollGraph.Settings.Current.LotSize = totalQtyRequired;

                if (!string.IsNullOrWhiteSpace(amProdMatl.CompBOMID))
                {
                    costRollGraph.Settings.Current.BOMID = amProdMatl.CompBOMID;
                    costRollGraph.Settings.Current.RevisionID = amProdMatl.CompBOMRevisionID;
                }
                else
                {
                    var bomId = new PrimaryBomIDManager(callingGraph).GetItemSitePrimary(amProdMatl.InventoryID, siteID, amProdMatl.SubItemID);
                    var bomItem = PrimaryBomIDManager.GetNotArchivedRevisionBomItem(costRollGraph, bomId);
                    if (bomItem?.BOMID != null)
                    {
                        costRollGraph.Settings.Current.BOMID = bomItem.BOMID;
                        costRollGraph.Settings.Current.RevisionID = bomItem.RevisionID;
                    }
                }

                if (costRollGraph.Settings.Current != null
                    && !string.IsNullOrWhiteSpace(costRollGraph.Settings.Current.BOMID)
                    && !string.IsNullOrWhiteSpace(costRollGraph.Settings.Current.RevisionID))
                {
                    costRollGraph.RollCosts();
                    var amBomCost = costRollGraph.LocateBomCost(costRollGraph.Settings.Current.BOMID, costRollGraph.Settings.Current.RevisionID);
                    if (amBomCost != null && amBomCost.InventoryID == amProdMatl.InventoryID)
                    {
                        materialUnitCost = amBomCost.UnitCost.GetValueOrDefault();
                    }
                }
            }

            if (materialUnitCost.GetValueOrDefault() == 0)
            {
                materialUnitCost = costRollGraph.GetUnitCost(inventoryItem, null, siteID);
            }

            // Convert the Unit Cost to Production Material UOM
            if (UomHelper.TryConvertToBaseCost<AMProdMatl.inventoryID>(callingGraph.Caches[typeof(AMProdMatl)], amProdMatl, amProdMatl.UOM, materialUnitCost.GetValueOrDefault(), out var matlUnitCost))
            {
                materialUnitCost = matlUnitCost;
            }

            if (materialUnitCost.GetValueOrDefault() == 0 ||
                amProdMatl.UnitCost.GetValueOrDefault() == materialUnitCost.GetValueOrDefault())
            {
                //leave the current cost if the new calculated cost is zero
                return;
            }

            amProdMatl.UnitCost = materialUnitCost.GetValueOrDefault();
            callingGraph.Caches[typeof(AMProdMatl)].Update(amProdMatl);
        }

        internal AMBomCost LocateBomCost(string bomId, string revisionId)
        {
            return (AMBomCost)BomCostRecs.Cache.Locate(new AMBomCost
            {
                BOMID = bomId,
                RevisionID = revisionId,
                UserID = Accessinfo.UserID
            });
        }

		[Obsolete("This method has been deprecated and will be removed in Acumatica ERP 2023R1. Logic in BOMCostRollMultiLevelExt")]
        public static decimal? GetUnitCostFromINItemSiteTable(INItemSite inItemSite)
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

		[Obsolete("This method has been deprecated and will be removed in Acumatica ERP 2023R1. Logic in BOMCostRollMultiLevelExt")]
        public static decimal? GetUnitCostFromINItemCostTable(INItemCost inItemCost)
        {
            if (inItemCost != null && (inItemCost.TranUnitCost != null || inItemCost.LastCost != null))
            {
                if ((inItemCost.TranUnitCost ?? 0) == 0 && (inItemCost.LastCost ?? 0) != 0)
                {
                    return inItemCost.LastCost;
                }

                return inItemCost.TranUnitCost;
            }

            return null;
        }

        public virtual decimal? GetUnitCost(InventoryItem inventoryItem, INItemSite inItemSite, int? siteid)
        {
			// override in extension
			return null;
        }

        /// <summary>
        /// Delete any calculated BOM cost records for the current user
        /// </summary>
        protected virtual void DeleteUserCostRollData()
        {
            DeleteUserCostRollData(Accessinfo.UserID);
        }

        /// <summary>
        /// Delete any calculated BOM cost records for the given user
        /// </summary>
        public static void DeleteUserCostRollData(Guid userID)
        {
            PXDatabase.Delete<AMBomCost>(new PXDataFieldRestrict<AMBomCost.userID>(userID));
        }

        /// <summary>
        /// Record costs or units for a set of operations
        /// </summary>
        public class OperationCosts
        {
            private Dictionary<int?, decimal> _operationCostDictionary;
            private decimal _totalCost;

            /// <summary>
            /// Total costs across all operation costs
            /// </summary>
            public decimal TotalCost => _totalCost;

            /// <summary>
            /// Cost for the given operation
            /// </summary>
            /// <param name="operationID">Operation ID</param>
            /// <returns></returns>
            public decimal OperationCost(int? operationID)
            {
                if (_operationCostDictionary.ContainsKey(operationID))
                {
                    return _operationCostDictionary[operationID];
                }

                return 0;
            }

            public OperationCosts()
            {
                _operationCostDictionary = new Dictionary<int?, decimal>();
            }

            public OperationCosts(OperationCosts existingOperCost)
            {
                _operationCostDictionary = existingOperCost?._operationCostDictionary ?? new Dictionary<int?, decimal>();
                _totalCost = existingOperCost?._totalCost ?? 0m;
            }

            public void Add(OperationCosts existingOperCost, bool addToExistingCost)
            {
                if (existingOperCost?._operationCostDictionary == null)
                {
                    return;
                }

                foreach (var kvp in existingOperCost._operationCostDictionary)
                {
                    Add(kvp.Key, kvp.Value, addToExistingCost);
                }
            }

            /// <summary>
            /// Add costs to an operation
            /// </summary>
            /// <param name="operationID">OperationID</param>
            /// <param name="cost">cost/unit value</param>
            /// <param name="addToExistingCost">Should the entry add to an existing cost if found (when true) or be replaced (when false)</param>
            public void Add(int? operationID, decimal cost, bool addToExistingCost = false)
            {
                decimal currentCost = 0;
                if (operationID == null)
                {
                    return;
                }

                if (_operationCostDictionary.ContainsKey(operationID))
                {
                    currentCost = _operationCostDictionary[operationID];
                    _operationCostDictionary.Remove(operationID);
                    _totalCost -= currentCost;
                    if (!addToExistingCost)
                    {
                        currentCost = 0;
                        if (_totalCost < 0)
                        {
                            _totalCost = 0;
                        }
                    }
                }

                _operationCostDictionary.Add(operationID, cost + currentCost);
                _totalCost += cost + currentCost;
            }
        }

        public virtual bool RollCosts(List<MultiLevelBomResult> multiLevelBomResults)
        {
             var processedAll = true;

            BomCostRecs.Cache.Clear();

			if(multiLevelBomResults == null)
			{
				return processedAll;
			}

            using(new DisableSelectorValidationScope(BomCostRecs.Cache))
            {
                foreach (var multiLevelBomResult in multiLevelBomResults.OrderByDescending(r => r.Level))
                {
					if(multiLevelBomResult.IsRecursive && Settings?.Current != null)
					{
						Settings.Current.FoundRecursiveBom = true;
					}

					processedAll &= ProcessCost(multiLevelBomResult.BomItem, multiLevelBomResult.Level, multiLevelBomResult.IsDefaultBOM, multiLevelBomResult.InventoryItem, multiLevelBomResult.ItemSite);
                }
            }

            return processedAll;
        }

        /// <summary>
        /// Write AMBomCostHistory record from AMBomCost record
        /// </summary>
        protected virtual AMBomCostHistory WriteAMBomCostHistoryRecord(AMBomCost bomCost)
        {
            var aMBomCostHistory = new AMBomCostHistory
            {
                BOMID = bomCost.BOMID,
                RevisionID = bomCost.RevisionID,
                StartDate = Common.Dates.Today,
                EndDate = null,
                MatlManufacturedCost = bomCost.MatlManufacturedCost,
                MatlNonManufacturedCost = bomCost.MatlNonManufacturedCost,
                FLaborCost = bomCost.FLaborCost,
                VLaborCost = bomCost.VLaborCost,
                MachCost = bomCost.MachCost,
                OutsideCost = bomCost.OutsideCost,
                DirectCost = bomCost.DirectCost,
                FOvdCost = bomCost.FOvdCost,
                VOvdCost = bomCost.VOvdCost,
                ToolCost = bomCost.ToolCost,
                SubcontractMaterialCost = bomCost.SubcontractMaterialCost,
                ReferenceMaterialCost = bomCost.ReferenceMaterialCost,
                InventoryID = bomCost.InventoryID,
                SubItemID = bomCost.SubItemID,
                SiteID = bomCost.SiteID,
                UnitCost = bomCost.UnitCost,
                TotalCost = bomCost.TotalCost,
                LotSize = bomCost.LotSize,
                MultiLevelProcess = bomCost.MultiLevelProcess,
                Level = bomCost.Level,
                IsDefaultBom = bomCost.IsDefaultBom,
                FixedLaborTime = bomCost.FixedLaborTime,
                VariableLaborTime = bomCost.VariableLaborTime,
                MachineTime = bomCost.MachineTime,
                ItemClassID = bomCost.ItemClassID,
                StdCost = bomCost.StdCost,
                PendingStdCost = bomCost.PendingStdCost
            };

            return BomCostHistoryRecs.Insert(aMBomCostHistory);
        }

        /// <summary>
        /// Update  existing AMBomCostHistory record from AMBomCost record
        /// </summary>
        protected virtual AMBomCostHistory UpdateAMBomCostHistoryRecord(AMBomCostHistory existingBomCostHistory, AMBomCost bomCost)
        {
            existingBomCostHistory.MatlManufacturedCost = bomCost.MatlManufacturedCost;
            existingBomCostHistory.MatlNonManufacturedCost = bomCost.MatlNonManufacturedCost;
            existingBomCostHistory.FLaborCost = bomCost.FLaborCost;
            existingBomCostHistory.VLaborCost = bomCost.VLaborCost;
            existingBomCostHistory.MachCost = bomCost.MachCost;
            existingBomCostHistory.OutsideCost = bomCost.OutsideCost;
            existingBomCostHistory.DirectCost = bomCost.DirectCost;
            existingBomCostHistory.FOvdCost = bomCost.FOvdCost;
            existingBomCostHistory.VOvdCost = bomCost.VOvdCost;
            existingBomCostHistory.ToolCost = bomCost.ToolCost;
            existingBomCostHistory.SubcontractMaterialCost = bomCost.SubcontractMaterialCost;
            existingBomCostHistory.ReferenceMaterialCost = bomCost.ReferenceMaterialCost;
            existingBomCostHistory.InventoryID = bomCost.InventoryID;
            existingBomCostHistory.SubItemID = bomCost.SubItemID;
            existingBomCostHistory.SiteID = bomCost.SiteID;
            existingBomCostHistory.UnitCost = bomCost.UnitCost;
            existingBomCostHistory.TotalCost = bomCost.TotalCost;
            existingBomCostHistory.LotSize = bomCost.LotSize;
            existingBomCostHistory.MultiLevelProcess = bomCost.MultiLevelProcess;
            existingBomCostHistory.Level = bomCost.Level;
            existingBomCostHistory.IsDefaultBom = bomCost.IsDefaultBom;
            existingBomCostHistory.FixedLaborTime = bomCost.FixedLaborTime;
            existingBomCostHistory.VariableLaborTime = bomCost.VariableLaborTime;
            existingBomCostHistory.MachineTime = bomCost.MachineTime;
            existingBomCostHistory.ItemClassID = bomCost.ItemClassID;
            existingBomCostHistory.StdCost = bomCost.StdCost;
            existingBomCostHistory.PendingStdCost = bomCost.PendingStdCost;

            return BomCostHistoryRecs.Update(existingBomCostHistory);
        }

        protected virtual void ArchiveBomCostRecords()
        {
            foreach (AMBomCost bomCostRec in PXSelect<AMBomCost,
                Where<AMBomCost.userID, Equal<Current<AccessInfo.userID>>
                   >>.Select(this))
            {
                if (bomCostRec.Selected != true)
                {
                    continue;
                }

                AMBomCostHistory bomCostHistory = PXSelect<AMBomCostHistory,
                    Where<AMBomCostHistory.bOMID, Equal<Required<AMBomCostHistory.bOMID>>,
                        And<AMBomCostHistory.revisionID, Equal<Required<AMBomCostHistory.revisionID>>>>,
                    OrderBy<Desc<AMBomCostHistory.startDate>>
                    >.SelectWindowed(this, 0, 1, bomCostRec.BOMID, bomCostRec.RevisionID);

                if (bomCostHistory == null)
                {
                    WriteAMBomCostHistoryRecord(bomCostRec);
                }
                else if (bomCostHistory.CreatedDateTime >= Common.Dates.UtcToday && 
                    bomCostHistory.CreatedDateTime <= Common.Dates.UtcToday.AddHours(23.98))
                {
                    UpdateAMBomCostHistoryRecord(bomCostHistory, bomCostRec);
                }
                else
                {
                    bomCostHistory.EndDate = Common.Dates.Today.AddDays(-1);
                    BomCostHistoryRecs.Update(bomCostHistory);
                    WriteAMBomCostHistoryRecord(bomCostRec);
                }
            }

            Actions.PressSave();
        }
    }

    /// <summary>
    /// BOM Cost roll filter DAC
    /// </summary>
    [Serializable]
    [PXCacheName("Cost Roll Settings")]
    public class RollupSettings : IBqlTable
    {
        #region SnglMlti
        public abstract class snglMlti : PX.Data.BQL.BqlString.Field<snglMlti> { }

        protected String _SnglMlti;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(SelectOptSM.Sngle)]
        [PXUIField(DisplayName = "Level")]
        [SelectOptSM.List]
        public virtual String SnglMlti
        {
            get
            {
                return this._SnglMlti;
            }
            set
            {
                this._SnglMlti = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [AMSite]
        public virtual Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItemNoRestrict]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [SubItem(typeof(RollupSettings.inventoryID))]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region LotSize
        public abstract class lotSize : PX.Data.BQL.BqlDecimal.Field<lotSize> { }

        protected decimal? _LotSize;
        [PXQuantity]
        [PXUIField(DisplayName = "Lot Size")]
        [PXUnboundDefault(TypeCode.Decimal,"0.0")]
        public virtual decimal? LotSize
        {
            get
            {
                return this._LotSize;
            }
            set
            {
                this._LotSize = value;
            }
        }
        #endregion
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

        protected String _BOMID;
        [BomID]
        [BOMIDSelector]
        public virtual String BOMID
        {
            get
            {
                return this._BOMID;
            }
            set
            {
                this._BOMID = value;
            }
        }
        #endregion
        #region RevisionID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        protected String _RevisionID;
        [RevisionIDField]
        [PXRestrictor(typeof(Where<AMBomItem.status, Equal<AMBomStatus.active>, Or<AMBomItem.status, Equal<AMBomStatus.hold>>>), Messages.BomRevisionIsArchived, typeof(AMBomItem.bOMID), typeof(AMBomItem.revisionID), CacheGlobal = true)]
        [PXSelector(typeof(Search<AMBomItem.revisionID,
                Where<AMBomItem.bOMID, Equal<Current<RollupSettings.bOMID>>>>)
            , typeof(AMBomItem.revisionID)
            , typeof(AMBomItem.status)
            , typeof(AMBomItem.descr)
            , typeof(AMBomItem.effStartDate)
            , typeof(AMBomItem.effEndDate)
            , DescriptionField = typeof(AMBomItem.descr))]
        public virtual String RevisionID
        {
            get
            {
                return this._RevisionID;
            }
            set
            {
                this._RevisionID = value;
            }
        }
        #endregion
        #region incMatScrp
        public abstract class incMatScrp : PX.Data.BQL.BqlBool.Field<incMatScrp> { }

        protected Boolean? _IncMatScrp;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include Material Scrap Factors")]
        public virtual Boolean? IncMatScrp
        {
            get
            {
                return this._IncMatScrp;
            }
            set
            {
                this._IncMatScrp = value;
            }
        }
        #endregion
        #region IncFixed
        public abstract class incFixed : PX.Data.BQL.BqlBool.Field<incFixed> { }

        protected Boolean? _IncFixed;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include Fixed Costs")]
        public virtual Boolean? IncFixed
        {
            get
            {
                return this._IncFixed;
            }
            set
            {
                this._IncFixed = value;
            }
        }
        #endregion
        #region UsePending
        public abstract class usePending : PX.Data.BQL.BqlBool.Field<usePending> { }

        protected Boolean? _UsePending;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Use Pending Standard Cost for Purchase Items")]
        public virtual Boolean? UsePending
        {
            get
            {
                return this._UsePending;
            }
            set
            {
                this._UsePending = value;
            }
        }
        #endregion
        #region Update Material
        public abstract class updateMaterial : PX.Data.BQL.BqlBool.Field<updateMaterial> { }

        protected Boolean? _UpdateMaterial;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Update Material")]
        public virtual Boolean? UpdateMaterial
        {
            get
            {
                return this._UpdateMaterial;
            }
            set
            {
                this._UpdateMaterial = value;
            }
        }
        #endregion
        #region ApplyPend
        public abstract class applyPend : PX.Data.BQL.BqlBool.Field<applyPend> { }

        protected Boolean? _ApplyPend;
        [PXDBBool]
        [PXDefault]
        [PXUIField(DisplayName = "Apply to Pending Costs")]
        public virtual Boolean? ApplyPend
        {
            get
            {
                return this._ApplyPend;
            }
            set
            {
                this._ApplyPend = value;
            }
        }
        #endregion
        #region Item Class ID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }

        protected int? _ItemClassID;
        [PXDBInt]
        [PXUIField(DisplayName = "Item Class")]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
        public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
        #endregion
        #region IgnoreMinMaxLotSizeValues
        public abstract class ignoreMinMaxLotSizeValues : PX.Data.BQL.BqlBool.Field<ignoreMinMaxLotSizeValues> { }

        protected Boolean? _IgnoreMinMaxLotSizeValues;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Ignore Min/Max/Lot Size Values")]
        public virtual Boolean? IgnoreMinMaxLotSizeValues
        {
            get
            {
                return this._IgnoreMinMaxLotSizeValues;
            }
            set
            {
                this._IgnoreMinMaxLotSizeValues = value;
            }
        }
        #endregion
		#region IgnoreReplenishmentSettings
        public abstract class ignoreReplenishmentSettings : PX.Data.BQL.BqlBool.Field<ignoreReplenishmentSettings> { }

        protected Boolean? _IgnoreReplenishmentSettings;
        /// <summary>
        /// When checked the exploded levels will ignore the items replenishment settings for manufactured items only. 
        /// If a default bom is found it will be used during processing
        /// </summary>
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Ignore Replenishment Source")]
        public virtual Boolean? IgnoreReplenishmentSettings
        {
            get { return this._IgnoreReplenishmentSettings; }
            set { this._IgnoreReplenishmentSettings = value; }
        }

        #endregion
        #region SelectSM Option
        public static class SelectOptSM
        {
            //Constants declaration 
            public const string Sngle = "S";
            public const string Multi = "M";

            //List attribute 
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                    new string[] { Sngle, Multi },
                    new string[] { "Single", "Multi" })
                {}
            }
            //BQL constants declaration
            public class sngle : PX.Data.BQL.BqlString.Constant<sngle>
            {
                public sngle() : base(Sngle) {}
            }
            public class multi : PX.Data.BQL.BqlString.Constant<multi>
            {
                public multi() : base(Multi) {}
            }

        }
        #endregion

        #region EffectiveDate
        public abstract class effectiveDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _EffectiveDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Effective Date")]
        public virtual DateTime? EffectiveDate
        {
            get
            {
                return this._EffectiveDate;
            }
            set
            {
                this._EffectiveDate = value;
            }
        }
        #endregion

        #region IsPersistMode
        /// <summary>
        /// Is the cost roll processing going to persist the overall results after processing?
        /// Useful when call from other processes where we want to skip persisting and just get the results from the cache
        /// </summary>
        public abstract class isPersistMode : PX.Data.BQL.BqlBool.Field<isPersistMode> { }

        protected Boolean? _IsPersistMode;
        /// <summary>
        /// Is the cost roll processing going to persist the overall results after processing?
        /// Useful when call from other processes where we want to skip persisting and just get the results from the cache
        /// </summary>
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Persist Results", Enabled = false, Visibility = PXUIVisibility.Invisible)]
        public virtual Boolean? IsPersistMode
        {
            get
            {
                return this._IsPersistMode;
            }
            set
            {
                this._IsPersistMode = value;
            }
        }
        #endregion

        #region FoundRecursiveBom
        /// <summary>
        /// Updated fields from cost roll process which sets the field true when a recursive is found
        /// </summary>
        public abstract class foundRecursiveBom : PX.Data.BQL.BqlBool.Field<foundRecursiveBom> { }

        protected Boolean? _FoundRecursiveBom;
        /// <summary>
        /// Updated fields from cost roll process which sets the field true when a recursive is found
        /// </summary>
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Recursive BOM Exists", Enabled = false, Visibility = PXUIVisibility.Invisible)]
        public virtual Boolean? FoundRecursiveBom
        {
            get
            {
                return this._FoundRecursiveBom;
            }
            set
            {
                this._FoundRecursiveBom = value;
            }
        }
        #endregion
    }

	/// <summary>
	/// Graph extension of <see cref="BOMCostRoll"/> to apply reusable business object <see cref="MultiLevelBomProcessBase{TGraph, TPrimary}"/>
	/// </summary>
	public class BOMCostRollMultiLevelExt : MultiLevelBomProcessBase<BOMCostRoll, RollupSettings>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
		}

		public override MultiLevelBomProcessFilter GetDefaultFilter
		{
			get
			{
				var filter = base.GetDefaultFilter;
				filter.ExcludedBomStatuses = new List<string> { AMBomStatus.Archived, AMBomStatus.Hold };
				return filter;
			}
		}

		public delegate List<MultiLevelBomResult> GetMultiLevelDataDelegate(RollupSettings costRollSettings);

		[PXOverride]
		public virtual List<MultiLevelBomResult> GetMultiLevelData(RollupSettings costRollSettings, GetMultiLevelDataDelegate baseMethod)
		{
			return baseMethod?.Invoke(costRollSettings) ?? GetMultiLevelBomResults(ConvertToMultiLevelBomProcessFilter(costRollSettings));
		}

		protected virtual MultiLevelBomProcessFilter ConvertToMultiLevelBomProcessFilter(RollupSettings costRollSettings)
		{
			var multiLevelFilter = GetDefaultFilter;
			multiLevelFilter.IsSingleLevel = costRollSettings?.SnglMlti != RollupSettings.SelectOptSM.Multi;
			multiLevelFilter.ItemClassID = costRollSettings?.ItemClassID;
			multiLevelFilter.BOMID = costRollSettings?.BOMID;
			multiLevelFilter.RevisionID = costRollSettings?.RevisionID;
			multiLevelFilter.InventoryID = costRollSettings?.InventoryID;
			multiLevelFilter.SubItemID = costRollSettings?.SubItemID;
			multiLevelFilter.EffectiveDate = costRollSettings?.EffectiveDate;
			multiLevelFilter.IgnoreReplenishmentSettings = costRollSettings?.IgnoreReplenishmentSettings == true;

			if(!string.IsNullOrWhiteSpace(multiLevelFilter.BOMID))
			{
				multiLevelFilter.ExcludedBomStatuses = new List<string> { AMBomStatus.Archived };
			}

			return multiLevelFilter;
		}

		public delegate string GetReplenishmentSourceDelegate(InventoryItem invItem, INItemSite inItemSite);

		[PXOverride]
		public virtual string GetReplenishmentSource(InventoryItem invItem, INItemSite inItemSite, GetReplenishmentSourceDelegate baseMethod)
		{
			return baseMethod?.Invoke(invItem, inItemSite) ?? GetReplenishmentSource(ConvertToMultiLevelBomProcessFilter(Base.Settings?.Current), invItem, inItemSite);
		}

		public delegate decimal? GetUnitCostDelegate(InventoryItem inventoryItem, INItemSite inItemSite, int? siteid);

		[PXOverride]
		public virtual decimal? GetUnitCost(InventoryItem inventoryItem, INItemSite inItemSite, int? siteid, GetUnitCostDelegate baseMethod)
		{
			return baseMethod?.Invoke(inventoryItem, inItemSite, siteid) ?? GetItemUnitCost(inventoryItem, inItemSite, siteid, Base.Settings?.Current?.UsePending == true);
		}
	}
}
