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
using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.IN;
using System.Text;
using PX.Objects.AM.CacheExtensions;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using static PX.Data.Events;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing release process for all manufacturing document types
    /// </summary>
    public class AMReleaseProcess : PXGraph<AMReleaseProcess>
    {
        public class AMProdMatlSplitPlan : AMProdMatlSplitPlan<AMReleaseProcess> { }
        public class AMProdItemSplitPlan : AMProdItemSplitPlan<AMReleaseProcess> { }

        public PXSelectJoin<AMBatch,
			InnerJoin<Branch, On<AMBatch.branchID, Equal<Branch.branchID>>>,
			Where<AMBatch.released, Equal<False>, And<AMBatch.hold, Equal<False>,
				And<Branch.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>>>>> BatchRecs;
        public PXSelect<AMMTran, Where<AMMTran.docType, Equal<Current<AMBatch.docType>>, And<AMMTran.batNbr, Equal<Current<AMBatch.batNbr>>>>> TranRecs;
        public PXSelect<AMMTranSplit, Where<AMMTranSplit.docType, Equal<Current<AMMTran.docType>>, And<AMMTranSplit.batNbr, Equal<Current<AMMTran.batNbr>>, And<AMMTranSplit.lineNbr, Equal<Current<AMMTran.lineNbr>>>>>> SplitRecs;
        public PXSelect<AMProdItem, Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>> ProductionItems;
        public PXSelect<AMProdEvnt, Where<AMProdEvnt.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdEvnt.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProductionEvents;
        public PXSelect<AMProdOper, Where<AMProdOper.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdOper.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProductionOpers;
        public PXSelect<AMProdMatl, Where<AMProdMatl.orderType, Equal<Current<AMProdOper.orderType>>, And<AMProdMatl.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, And<AMProdMatl.operationID, Equal<Current<AMProdOper.operationID>>>>>> ProductionMatl;
        public PXSelect<AMProdOvhd, Where<AMProdOvhd.orderType, Equal<Current<AMProdOper.orderType>>, And<AMProdOvhd.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, And<AMProdOvhd.operationID, Equal<Current<AMProdOper.operationID>>>>>> ProductionOvhd;
        public PXSelect<AMProdTool, Where<AMProdTool.orderType, Equal<Current<AMProdOper.orderType>>, And<AMProdTool.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>, And<AMProdTool.operationID, Equal<Current<AMProdOper.operationID>>>>>> ProductionTool;
        public PXSelect<AMProdTotal, Where<AMProdTotal.orderType, Equal<Current<AMProdItem.orderType>>, And<AMProdTotal.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> ProdTotalRecs;
        public PXSelect<AMClockTran> ClockTranRecs;
        public PXSelect<AMToolMst> ToolMaster;
		public PXSelect<AMToolMstCurySettings> ToolMasterCury;
		public PXSetup<AMPSetup> ProductionSetup;
        public PXSelect<AMProdMatlLotSerial> MatlLotSerialRecs;

        /// <summary>
        /// Used to update batch control totals when INTran is updated.
        /// This happens automatically but without the view the values are not updated to the DB
        /// </summary>
        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelect<INRegister> INRegisterRecs;
        /// <summary>
        /// Used to update INTran values during unit cost updates.
        /// Should not perform insert or updates on this view. Possibly use production transaction helper to build the IN transactions.
        /// </summary>
        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelect<INTran> INTranRecs;

        public bool UpdateProduction;

        protected UniqueProductionNbrList ReferencedProductionOrders;
        protected UniqueStringCollection ReferencedInBatches;
        protected UniqueStringCollection ReferencedGlBatches;
        protected List<ProductionEventHelper> ProductionBatchEvents;
        protected List<INRegister> InventoryBatches;
        protected List<Batch> GeneralLedgerBatches;
        protected Dictionary<string, ProductionOperationCostResults> FirstTranOperCostResults;
		public Lazy<ProdDetail> ProductionDetailGraph = PXGraph.CreateLazyInstance<ProdDetail>();

		public PXSelect<AMProdItemSplit,
            Where<AMProdItemSplit.orderType, Equal<Current<AMProdItem.orderType>>,
                And<AMProdItemSplit.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>> prodItemSplits;

        public PXSelect<AMProdMatlSplit,
            Where<AMProdMatlSplit.orderType, Equal<Current<AMProdMatl.orderType>>,
                And<AMProdMatlSplit.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>,
                    And<AMProdMatlSplit.operationID, Equal<Current<AMProdMatl.operationID>>,
                        And<AMProdMatlSplit.lineID, Equal<Current<AMProdMatl.lineID>>>>>>> ProdMatlSplits;

        public AMReleaseProcess()
        {
            InitReleaseProcess();
        }

        public override void Clear()
        {
            base.Clear();
            InitReleaseProcess();
        }

        protected void InitReleaseProcess()
        {
            ReferencedProductionOrders = new UniqueProductionNbrList();
            ProductionBatchEvents = new List<ProductionEventHelper>();
            ReferencedInBatches = new UniqueStringCollection();
            ReferencedGlBatches = new UniqueStringCollection();
            InventoryBatches = new List<INRegister>();
            GeneralLedgerBatches = new List<Batch>();
            UpdateProduction = false;
            FirstTranOperCostResults = new Dictionary<string, ProductionOperationCostResults>();
        }

		#region Cache Attached

		[PXDBString(20, InputMask = ">aaaaaaaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Reason Code")]
		protected virtual void _(Events.CacheAttached<AMMTran.reasonCodeID> e) { }

        [OperationIDField]
		protected virtual void _(Events.CacheAttached<AMMTran.operationID> e) { }

        [PXDefault(typeof(AMPSetup.defaultOrderType), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMMTran.orderType> e) { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMMTran.prodOrdID> e) { }

        [Inventory]
		protected virtual void _(Events.CacheAttached<AMMTran.inventoryID> e) { }

        [PXDefault(typeof(Search<AMProdItem.siteID,
            Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SiteAvail(typeof(AMMTran.inventoryID), typeof(AMMTran.subItemID), typeof(CostCenter.freeStock))]
		protected virtual void _(Events.CacheAttached<AMMTran.siteID> e) { }

        [PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), PX.Objects.IN.Messages.InactiveLocation, typeof(INLocation.locationCD), CacheGlobal = true)]
        [Location(typeof(AMMTran.siteID))]
		protected virtual void _(Events.CacheAttached<AMMTran.locationID> e) { }

        [PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), PX.Objects.IN.Messages.InactiveLocation, typeof(INLocation.locationCD), CacheGlobal = true)]
        [Location(typeof(AMMTranSplit.siteID))]
		protected virtual void _(Events.CacheAttached<AMMTranSplit.locationID> e) { }

        [INUnit(typeof(AMMTran.inventoryID))]
		protected virtual void _(Events.CacheAttached<AMMTran.uOM> e) { }

        [PXDBInt]
        [PXUIField(DisplayName = "WIP Account")]
		protected virtual void _(Events.CacheAttached<AMMTran.wIPAcctID> e) { }

        [PXDBInt]
        [PXUIField(DisplayName = "WIP Subaccount")]
		protected virtual void _(Events.CacheAttached<AMMTran.wIPSubID> e) { }

        [PXDBGuid]
		protected virtual void _(Events.CacheAttached<AMMTran.noteID> e)
        {
            //Prevent joined lookup to note table
        }

        [PXDBGuid]
		protected virtual void _(Events.CacheAttached<AMProdItem.noteID> e)
        {
            //Prevent joined lookup to note table
        }

        [PXDBGuid]
		protected virtual void _(Events.CacheAttached<AMProdOper.noteID> e)
        {
            //Prevent joined lookup to note table
        }

		[WorkCenterIDField(Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<AMProdOper.wcID> e) { }

        //Removing PXUIRequired
        [RevisionIDField(DisplayName = "BOM Revision")]
		protected virtual void _(Events.CacheAttached<AMProdItem.bOMRevisionID> e) { }

        //Removing PXUIRequired
        [AMOrderTypeField(DisplayName = "Source Order Type", Visibility = PXUIVisibility.Undefined)]
		protected virtual void _(Events.CacheAttached<AMProdItem.sourceOrderType> e) { }

        //Removing PXUIRequired
        [ProductionNbr(DisplayName = "Source Production Nbr", Visibility = PXUIVisibility.Undefined)]
		protected virtual void _(Events.CacheAttached<AMProdItem.sourceProductionNbr> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Mult<INTran.qty, INTran.unitCost>), typeof(SumCalc<INRegister.controlCost>))]
		protected virtual void _(Events.CacheAttached<INTran.tranCost> e) { }

        #endregion

		protected virtual void _(Events.RowUpdated<AMProdMatl> e)
		{
			if (e.Row == null || (e.Cache.ObjectsEqual<AMProdMatl.totalQtyRequired>(e.OldRow, e.Row)
				&& !(e.OldRow.QtyRemaining == 0 && e.Row.QtyRemaining > 0)))
			{
				return;
			}

			UpdateProductionMaterialSplitQty(e.Row);

		}

        /// <summary>
        /// Clear the production cache (updates and query)
        /// </summary>
        protected virtual void ClearProdRecsCache()
        {
            ClearProdRecsCache(false);
        }

        /// <summary>
        /// Clear the production cache (updates and query)
        /// </summary>
        internal virtual void ClearProdRecsCache(bool preserveCostValues)
        {
            //Need to clear the updated and the query cache
            // Using cache for running totals for part 1 of Manufacturing batch release. 
            //  Need a way to clear the non transactional cache. Will recalculate once part 3 is executed and committed then

            //Clear ProdOper before ProductionItems
            ClearProdOperCache(preserveCostValues);

            ProductionItems.Cache.Clear();
            ProductionItems.Cache.ClearQueryCache();

            ProdTotalRecs.Cache.Clear();
            ProdTotalRecs.Cache.ClearQueryCache();

            ProductionEvents.Cache.Clear();
            ProductionEvents.Cache.ClearQueryCache();

            ProductionMatl.Cache.Clear();
            ProductionMatl.Cache.ClearQueryCache();

            //Certain processes require the calculated overhead to remain calculated (persist will only save if on update order)
            if (!preserveCostValues)
            {
                ProductionOvhd.Cache.Clear();
                ProductionOvhd.Cache.ClearQueryCache(); 
            }

            ProductionTool.Cache.Clear();
            ProductionTool.Cache.ClearQueryCache();

            ToolMaster.Cache.Clear();
            ToolMaster.Cache.ClearQueryCache();

			ToolMasterCury.Cache.Clear();
			ToolMasterCury.Cache.ClearQueryCache();

			ProdMatlSplits.Cache.Clear();
			ProdMatlSplits.Cache.ClearQueryCache();

			this.Caches[typeof(INItemPlan)].Clear();
			this.Caches[typeof(INItemPlan)].ClearQueryCache();
        }

        private void ClearProdOperCache(bool preserveCostValues)
        {
            var preservedActCostOpers = new List<AMProdOper>();
            if(preserveCostValues)
            {
                foreach (AMProdOper oper in ProductionOpers.Cache.Updated)
                {
                    var origRow = (AMProdOper)ProductionOpers.Cache.GetOriginal(oper);
                    if (oper?.OperationID == null || origRow?.OperationID == null)
                    {
                        continue;
                    }

                    var copy = (AMProdOper)ProductionOpers.Cache.CreateCopy(origRow);
                    if (copy == null)
                    {
                        continue;
                    }

                    var prodItem = AMProdOper.FK.ProductionOrder.FindParent(this, oper);
                    if(prodItem?.CostMethod == CostMethod.Actual)
                    {
                        //Actual items relay in the wip values and this would throw them off
                        // Only estimated values (which standard calculates against too) will require these values for plan estimated values
                        continue;
                    }

                    copy.ActualLabor = oper.ActualLabor;
                    copy.ActualLaborTime = oper.ActualLaborTime;
                    copy.ActualMachine = oper.ActualMachine;
                    copy.ActualMachineTime = oper.ActualMachineTime;
                    copy.ActualMaterial = oper.ActualMaterial;
                    copy.ActualTool = oper.ActualTool;
                    copy.ActualFixedOverhead = oper.ActualFixedOverhead;
                    copy.ActualVariableOverhead = oper.ActualVariableOverhead;
                    preservedActCostOpers.Add(copy);
                }
            }

            ProductionOpers.Cache.Clear();
            ProductionOpers.Cache.ClearQueryCache();

            if(preserveCostValues)
            {
                foreach (var row in preservedActCostOpers)
                {
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Preserve Operation Cost Values: {row.DebuggerDisplay}");
#endif
                    ProductionOpers.Update(row);
                }
            }
        }

        public override void Persist()
        {
			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.refNbr>(INTranRecs.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.tranDate>(INTranRecs.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.origModule>(INTranRecs.Cache, null, false);
            var itemUpdates = new List<AMProdItem>();
            var totalUpdates = new List<AMProdTotal>();
            var operUpdates = new List<AMProdOper>();
            var ovhdUpdates = new List<AMProdOvhd>();
            var prodMatlSplits = new List<AMProdMatlSplit>();
            var itemPlans = new List<INItemPlan>();
            if (!UpdateProduction)
            {
                // We need to preserve the updates in the cache but not yet to the DB.
                // The updated values are required to continue with the calculated values.
                // This is due to the complexities with multiple transactions scopes and releasing of Acumatica bases to be
                //  completed before Manufacturing batches are "released" and production orders updates are committed to the DB.

                foreach (AMProdItem rowUpdated in ProductionItems.Cache.Updated)
                {
                    itemUpdates.Add(rowUpdated);
                }
                foreach (AMProdTotal rowUpdated in ProdTotalRecs.Cache.Updated)
                {
                    totalUpdates.Add(rowUpdated);
                }
                foreach (AMProdOper rowUpdated in ProductionOpers.Cache.Updated)
                {
                    operUpdates.Add(rowUpdated);
                }
                foreach (AMProdOvhd rowUpdated in ProductionOvhd.Cache.Updated)
                {
                    ovhdUpdates.Add(rowUpdated);
                }
                foreach (AMProdMatlSplit rowUpdated in ProdMatlSplits.Cache.Updated)
                {
					prodMatlSplits.Add(rowUpdated);
                }
                foreach (INItemPlan rowUpdated in this.Caches[typeof(INItemPlan)].Updated)
                {
					itemPlans.Add(rowUpdated);
                }
                ClearProdRecsCache();
            }

			var cachedBatches = BatchRecs.Cache.Cached.RowCast<AMBatch>().Where(b=> BatchRecs.Cache.GetStatus(b) != PXEntryStatus.Deleted);
			foreach (AMBatch item in cachedBatches)
			{
				if (item.Released == true) continue;

				if (AllLinesReleased(item))
				{
					item.Released = true;
					BatchRecs.Update(item);
				}
			}

            base.Persist();

            if (!UpdateProduction)
            {
                //Preserve the updates for query cache...
                foreach (var itemUpdate in itemUpdates)
                {
                    ProductionItems.Update(itemUpdate);
                }
                foreach (var totalUpdate in totalUpdates)
                {
                    ProdTotalRecs.Update(totalUpdate);
                }
                foreach (var operUpdate in operUpdates)
                {
                    ProductionOpers.Update(operUpdate);
                }
                foreach (var ovhdUpdate in ovhdUpdates)
                {
                    ProductionOvhd.Update(ovhdUpdate);
                }
				foreach (var prodMatlSplit in prodMatlSplits)
				{
					ProdMatlSplits.Update(prodMatlSplit);
				}
				foreach (var itemPlan in itemPlans)
				{
					this.Caches[typeof(INItemPlan)].Update(itemPlan);
				}
            }
        }

        public virtual bool IncludeScrap => ProductionSetup.Current != null && ProductionSetup.Current.InclScrap.GetValueOrDefault();

        public virtual void ReleaseDocProc(AMBatch doc)
        {
            if (doc == null)
            {
                return;
            }

            if (doc.Hold.GetValueOrDefault())
            {
                throw new PXException(Messages.DocumentOnHoldCannotRelease);
            }

            if (doc.Released.GetValueOrDefault())
            {
                return;
            }

            InitReleaseProcess();

            ProductionSetup.Current = ProductionSetup.Select();
            BatchRecs.Current = PXSelect<AMBatch,
                Where<AMBatch.docType, Equal<Required<AMBatch.docType>>, And<AMBatch.batNbr, Equal<Required<AMBatch.batNbr>>>>>
                .Select(this, doc.DocType, doc.BatNbr);

            #region PART 1 - Build documents
            PXTrace.WriteVerbose($"Releasing AM Document {doc.DocType}:{doc.BatNbr}. Pt 1 - Building Documents");
#if DEBUG
            AMDebug.TraceWriteMethodName($"[{AMDocType.GetDocTypeDesc(doc.DocType)}:{doc.BatNbr}] -------------------------- PART 1 - Build documents --------------------------");
            var swPart1 = System.Diagnostics.Stopwatch.StartNew();
#endif
            try
            {
				BatchRecs.Cache.SetStatus(doc, PXEntryStatus.Held);
                DocumentProcessSelector(doc);
            }
            catch (Exception e)
            {
                PXTrace.WriteInformation($"Exception releasing {AMDocType.GetDocTypeDesc(doc.DocType)} transaction {doc.BatNbr}. Pt 1. {e.Message}");

                DeleteOperationWipCompleteRecords(doc, true);

                PXTraceHelper.PxTraceException(e);
                throw;
            }
#if DEBUG
            finally
            {
                swPart1.Stop();
                PXTraceHelper.TraceWrite(PXTraceHelper.CreateTimespanMessage(swPart1.Elapsed, $"Release Part 1 - {AMDocType.GetDocTypeDesc(doc.DocType)} {doc.BatNbr} Runtime"));
            }
#endif

            #endregion

            // When using with preserve cost the actual values for recalculating the estimated unit cost will be the correct values
            //  Part 3 will rebuild the prod rec data and persist correctly
            ClearProdRecsCache(preserveCostValues: AMDocType.IsDocTypeMove(doc.DocType));

            #region PART 2 - Release IN/GL/AM related batches
            //PART 2  Cannot use a transaction scope. 
            PXTrace.WriteVerbose($"Releasing AM Document {doc.DocType}:{doc.BatNbr}. Pt 2 - Release related documents");
#if DEBUG
            AMDebug.TraceWriteMethodName($"[{AMDocType.GetDocTypeDesc(doc.DocType)}:{doc.BatNbr}] -------------------------- PART 2 - Release IN/GL/AM related batches --------------------------");
            var swPart2 = System.Diagnostics.Stopwatch.StartNew();
#endif
            try
            {
                ReleaseReferenceBatches(doc);
            }
            catch (Exception exception)
            {
                if (exception.InnerException != null
                    /* IN Errors print the same details in inner exception */
                    && !exception.InnerException.Message.Equals(exception.Message))
                {
                    PXTrace.WriteError(exception.InnerException);
                }
                PXTrace.WriteError(Messages.GetLocal(Messages.UnableToReleaseRelatedTransactions, AMDocType.GetDocTypeDesc(doc.DocType), doc.BatNbr, exception.Message));
                DeletePreAllocatedPlans(doc);
                throw;
            }
#if DEBUG
            finally
            {
                swPart2.Stop();
                PXTraceHelper.TraceWrite(PXTraceHelper.CreateTimespanMessage(swPart2.Elapsed, $"Release Part 2 - {AMDocType.GetDocTypeDesc(doc.DocType)} {doc.BatNbr} Runtime"));
            }
#endif

            #endregion

            ClearProdRecsCache(); //let part 3 rebuild the prod rec data

            #region PART 3 - Update production references
            PXTrace.WriteVerbose($"Releasing AM Document {doc.DocType}:{doc.BatNbr}. Pt 3 - Update production references");
#if DEBUG
            AMDebug.TraceWriteMethodName($"[{AMDocType.GetDocTypeDesc(doc.DocType)}:{doc.BatNbr}] -------------------------- PART 3 - Update production references --------------------------");
            var swPart3 = System.Diagnostics.Stopwatch.StartNew();
            try
            {
#endif
            UpdateProduction = true;

            try
            {
				// Supporting AC-269022
				var allLinesPreviouslyReleased = AllLinesReleased(doc);
                MarkBatchReleased(doc);
				if (!allLinesPreviouslyReleased)
				{
                UpdateProductionOrders(doc);
                UpdateClockEntries(doc);

                // Process needed to run for all but production costs. The prod costs will update when the move/labor updates. We need BF material to update for actual cost orders or the wIP values will be incorrect.
                if (doc.DocType != AMDocType.ProdCost)
                {
                    ProductionTransactionHelper.UpdateProdItemCosts(ReferencedProductionOrders.KeysList(), this);
                }
            }
				else
				{
					PXTrace.WriteWarning($"All lines for batch {AMDocType.GetDocTypeDesc(doc.DocType)} {doc.BatNbr} previously released.");
				}
            }
            catch (PXOuterException exception)
            {
                PXTrace.WriteInformation($"Exception releasing {AMDocType.GetDocTypeDesc(doc.DocType)} transaction {doc.BatNbr}. Pt 3. {exception.Message}");
                PXTraceHelper.PxTraceOuterException(exception, PXTraceHelper.ErrorLevel.Error);

                throw;
            }
            catch (Exception e)
            {
                PXTrace.WriteInformation($"Exception releasing {AMDocType.GetDocTypeDesc(doc.DocType)} transaction {doc.BatNbr}. Pt 3. {e.Message}");
                throw;
            }

            if (IsDirty)
            {
                Actions.PressSave();
            }
#if DEBUG
            }
            finally
            {
                swPart3.Stop();
                PXTraceHelper.TraceWrite(PXTraceHelper.CreateTimespanMessage(swPart3.Elapsed, $"Release Part 3 - {AMDocType.GetDocTypeDesc(doc.DocType)} {doc.BatNbr} Runtime"));
            }
#endif
            #endregion
        }

		protected virtual bool AllLinesReleased(AMBatch doc)
		{
			var result = PXSelect<
					AMMTran,
					Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
						And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>>
					.Select(this, doc?.DocType, doc?.BatNbr);

			bool allReleased = false;
			foreach (AMMTran ammTran in result)				
			{
				allReleased = true;
				if (ammTran?.Released != true)
				{
					return false;
				}
			}

			return allReleased;
		}

        protected virtual void DeletePreAllocatedPlans(AMBatch doc)
        {
            if (doc != null)
            {
                PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict<INItemPlan.refNoteID>(doc.NoteID),
                    new PXDataFieldRestrict<INItemPlan.planType>(INPlanConstants.Plan64));
            }
        }

        protected virtual void UpdateClockEntries(AMBatch doc)
        {
            if (doc?.OrigDocType != AMDocType.Clock)
            {
                return;
            }

            foreach(PXResult<AMMTran, AMClockTran> result in PXSelectJoin<AMMTran, InnerJoin<AMClockTran, 
                On<AMMTran.origBatNbr, Equal<AMClockTran.employeeID>, And<AMMTran.origLineNbr, Equal<AMClockTran.lineNbr>>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>, 
                And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>,
                And<AMMTran.origDocType, Equal<Required<AMMTran.origDocType>>>>>>.Select(this, doc.DocType, doc.BatNbr, AMDocType.Clock))
            {
                var clock = (AMClockTran)result;
                if (clock?.EmployeeID == null)
                {
                    continue;
                }
                clock.Closeflg = true;
                ClockTranRecs.Update(clock);
            }
        }

        /// <summary>
        /// Release all related batches linked to current document being released
        /// </summary>
        private void ReleaseReferenceBatches(AMBatch doc)
        {
            //The current order of this method is important. 
            //  *** DO NOT CHANGE ***

            var materialBatchList = new List<AMBatch>();
            var prodCostBatchList = new List<AMBatch>();
            var wipBatchList = new List<AMBatch>();

            var hasMaterial = false;
            foreach (AMBatch relatedBatch in PXSelect<AMBatch,
                Where<AMBatch.origBatNbr, Equal<Required<AMBatch.origBatNbr>>,
                    And<AMBatch.origDocType, Equal<Required<AMBatch.origDocType>>
                    >>>.Select(this, doc.BatNbr, doc.DocType))
            {
                var isMatlDoc = relatedBatch.DocType == AMDocType.Material;
                hasMaterial |= isMatlDoc;

                if(relatedBatch.Released == true)
                {
                    continue;
                }

                relatedBatch.Hold = false;
                if (isMatlDoc)
                {
                    materialBatchList.Add(relatedBatch);
                    continue;
                }
                if (relatedBatch.DocType == AMDocType.ProdCost)
                {
                    prodCostBatchList.Add(relatedBatch);
                    continue;
                }
                if (relatedBatch.DocType == AMDocType.WipAdjust)
                {
                    wipBatchList.Add(relatedBatch);
                }
            }
#if DEBUG
            AMDebug.MeasureElapsed(() => ReleaseAM(materialBatchList), $"ReleaseAM({materialBatchList?.Count ?? 0})");
#else
            //material
            ReleaseAM(materialBatchList);
#endif

            if (hasMaterial || prodCostBatchList.Count > 0)
            {
#if DEBUG
                var prodCostBatchList2 = AMDebug.MeasureElapsed(() => MoveTranPostBackflushMaterialCleanup(doc), "MoveTranPostBackflushMaterialCleanup");
#else
                var prodCostBatchList2 = MoveTranPostBackflushMaterialCleanup(doc);
#endif
                if (prodCostBatchList2 != null)
                {
                    prodCostBatchList = prodCostBatchList2;
                }
            }

#if DEBUG
            AMDebug.MeasureElapsed(ReleaseIN, "ReleaseIN");
            AMDebug.MeasureElapsed(ReleaseGL, "ReleaseGL");
#else
            ReleaseIN();
            ReleaseGL();
#endif
            UpdateAdjustmentCostVariance(prodCostBatchList.FirstOrDefault());

            var releaseAllList = new List<AMBatch>();
            releaseAllList.AddRange(prodCostBatchList);
            releaseAllList.AddRange(wipBatchList);
            ReleaseAM(releaseAllList);

            DeleteEmptyGL(doc);

            if (doc.DocType == AMDocType.ProdCost)
            {
                return;
            }

            DeleteUnreleasedIN(doc);
        }

        protected virtual void UpdateAdjustmentCostVariance(AMBatch costBatch)
        {
            foreach (var adjBatch in InventoryBatches.Where(batch => batch.Released.GetValueOrDefault() && batch.DocType == INDocType.Adjustment).ToList())
            {
                var results = SelectFrom<INTran>.
                    InnerJoin<AMMTran>.On<INTranExt.aMDocType.IsEqual<AMMTran.docType>.
                        And<INTranExt.aMBatNbr.IsEqual<AMMTran.batNbr>.
                        And<INTranExt.aMLineNbr.IsEqual<AMMTran.lineNbr>>>>.
                    InnerJoin<AMProdItem>.On<AMMTran.orderType.IsEqual<AMProdItem.orderType>.
                        And<AMMTran.prodOrdID.IsEqual<AMProdItem.prodOrdID>>>.
                    Where<INTran.docType.IsEqual<@P.AsString>
                        .And<INTran.refNbr.IsEqual<@P.AsString>>>
                        .View.Select(this, adjBatch.DocType, adjBatch.RefNbr);

                foreach (PXResult<INTran, AMMTran, AMProdItem> result in results)
                {
                    var inTran = (INTran)result;
                    var moveTran = (AMMTran)result;
                    var order = (AMProdItem)result;

                    var varianceAmt = inTran.TranCost - moveTran.TranAmt;

                    if (moveTran.BaseQty < 0 && order.CostMethod != CostMethod.Standard && Math.Abs(varianceAmt.GetValueOrDefault()) > 0.02m)
                    {
                        var transactionHelper = new ProductionTransactionHelper(this);
                        transactionHelper.CreateInventoryCostAdjEntry(costBatch, moveTran, varianceAmt.GetValueOrDefault(), 0m);

                    }
                }
            }
        }

		protected virtual List<AMBatch> MoveTranPostBackflushMaterialCleanup(AMBatch doc)
        {
            if (doc.DocType != AMDocType.Move && doc.DocType != AMDocType.Labor)
            {
                return null;
            }

            var productionGLTranBuilder = GetCostBatchTranBuilder(doc) ?? new ProductionGLTranBuilder();

            //Right here we need to make sure the cost transactions for overheads related to material are updated.
            AddOverheadByBackflushMaterial(doc, ref productionGLTranBuilder);

            //NEED TO RE-CALC UNIT COST FOR LINES IN DOC WITH LAST OPER TRUE TO MAKE SURE THEY ARE CORRECT AFTER BACKFLUSHING, SCRAP, OVERHEAD, ETC.
            if (UpdateUnitCosts(doc, ref productionGLTranBuilder))
            {
                Persist();
                // Force of correct current values later
                ProductionTransactionHelper.RemoveProdCostFromCache(this);
                _HasOverheadByBackflushMaterial = false;
            }

            if (productionGLTranBuilder.HasAmTransactions)
            {
                productionGLTranBuilder.Save(); 
            }

            return productionGLTranBuilder.CurrentAmDocument != null ? new List<AMBatch> { productionGLTranBuilder.CurrentAmDocument } : null;
        }

        protected virtual void AMMTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row?.LaborType == null || row.LaborType == AMLaborType.Direct)
            {
                return;
            }

            if (row.WIPAcctID == null)
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.wIPAcctID>(sender));
            }

            if (row.WIPSubID == null)
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.wIPSubID>(sender));
            }
        }

		protected virtual void _(Events.RowPersisting<AMBatch> e, PXRowPersisting baseMethod)
		{
			if (baseMethod != null)
			{
			baseMethod.Invoke(e.Cache, e.Args);
			}
			CheckForUnreleasing(e);
		}

		protected virtual void CheckForUnreleasing(RowPersisting<AMBatch> e)
		{
			ProductionTransactionHelper.CheckForUnreleasedBatches(e);
		}

        /// <summary>
        /// Find the total amount of related batches linked to the given AM Batch and production order
        /// </summary>
        /// <param name="amBatch">Current document</param>
        /// <returns>Unreleased WIP amount</returns>
        protected virtual Dictionary<string, decimal> GetUnreleasedWIPByOperation(AMBatch amBatch)
        {
            return ProductionTransactionHelper.GetUnreleasedWipByOperation(this, amBatch);
        }

        /// <summary>
        /// Find the given production record in the cache.
        /// If found return the cache version in place of the passed version.
        /// </summary>
        protected virtual AMProdItem FindInCache(AMProdItem amProdItem)
        {
            if (amProdItem == null
                || string.IsNullOrWhiteSpace(amProdItem.OrderType)
                || string.IsNullOrWhiteSpace(amProdItem.ProdOrdID))
            {
                return amProdItem;
            }
            return ProductionItems.Cache.LocateElse(amProdItem);
        }

        /// <summary>
        /// Find the given production record in the cache.
        /// If found return the cache version in place of the passed version.
        /// </summary>
        protected virtual AMProdOper FindInCache(AMProdOper amProdOper)
        {
            if (amProdOper == null
                || string.IsNullOrWhiteSpace(amProdOper.OrderType)
                || string.IsNullOrWhiteSpace(amProdOper.ProdOrdID))
            {
                return amProdOper;
            }

            return ProductionOpers.Cache.LocateElse(amProdOper);
        }

        /// <summary>
        /// Update each transaction line with a corrected unit costs.
        /// Should be called after backflushing has occurred.
        /// Requires AMProdItem and AMProdOper updated cache to exist so wip and qty totals are correctly set.
        /// (Ex: backflush material not making it to WIP until release so unit cost for actual would be incorrect unless adjusted after material release)
        /// </summary>
        /// <param name="doc">Current document being processed</param>
        /// <param name="productionGLTranBuilder"></param>
        /// <returns>true when costs are updated</returns>
        protected virtual bool UpdateUnitCosts(AMBatch doc, ref ProductionGLTranBuilder productionGLTranBuilder)
        {
            var unitCostsUpdated = false;
            if(productionGLTranBuilder == null)
            {
                throw new ArgumentNullException(nameof(productionGLTranBuilder));
            }

            if (doc == null
                || doc.DocType != AMDocType.Move
                && doc.DocType != AMDocType.Labor)
            {
                return unitCostsUpdated;
            }

            var productionTranHelper = new ProductionTransactionHelper(this);

            PXResultset<AMMTran> qReqults = PXSelectJoin<
                AMMTran,
                InnerJoin<InventoryItem, 
                    On<AMMTran.inventoryID, Equal<InventoryItem.inventoryID>>,
                InnerJoin<AMOrderType, 
                    On<AMMTran.orderType, Equal<AMOrderType.orderType>>,
                InnerJoin<AMProdItem, 
                    On<AMMTran.orderType, Equal<AMProdItem.orderType>,
                    And<AMMTran.prodOrdID, Equal<AMProdItem.prodOrdID>>>,
                InnerJoin<AMProdOper, 
                    On<AMMTran.orderType, Equal<AMProdOper.orderType>,
                    And<AMMTran.prodOrdID, Equal<AMProdOper.prodOrdID>,
                    And<AMMTran.operationID, Equal<AMProdOper.operationID>>>>,
                LeftJoin<INTran, 
                    On<AMMTran.iNDocType, Equal<INTran.docType>,
                    And<AMMTran.iNBatNbr, Equal<INTran.refNbr>,
                    And<AMMTran.iNLineNbr, Equal<INTran.lineNbr>>>>>>>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>
            >
                .Select(this, doc.DocType, doc.BatNbr);

            var scrapTransactions = new List<Tuple<AMMTran, MoveOperationQtyTotals>>();
            var moveOperationQtyBuilder = MoveOperationQtyBuilder.ConstructPostOperUpdate(this, qReqults);
            Dictionary<string, decimal> unreleasedOperationWip = null;
            foreach (PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper, INTran> result in qReqults)
            {
                var ammTran = (AMMTran)result;
                var inTran = (INTran)result;
                var amProdItem = (AMProdItem)result;

                if (string.IsNullOrWhiteSpace(ammTran?.BatNbr)
                    || !ammTran.LastOper.GetValueOrDefault()
                    || ammTran.InventoryID == null
                    || ammTran.Released.GetValueOrDefault()
                    || inTran?.Released == true
                    || string.IsNullOrWhiteSpace(amProdItem?.ProdOrdID))
                {
                    continue;
                }

                var noCostChangeCondition = ammTran.Qty.GetValueOrDefault() <= 0 || inTran?.DocType == null || 
                        amProdItem.CostMethod == CostMethod.Standard ||
                        ((InventoryItem)result)?.ValMethod == INValMethod.Standard;

                if(noCostChangeCondition && !_HasOverheadByBackflushMaterial)
                {
                    continue;
                }

                var moveOperationQtyTotals = moveOperationQtyBuilder.GetMoveOperationTotals(ammTran);
                if (amProdItem.CostMethod == CostMethod.Actual)
                {
                    // Load this on first query...
                    if (unreleasedOperationWip == null)
                    {
                        unreleasedOperationWip = GetUnreleasedWIPByOperation(doc) ?? new Dictionary<string, decimal>();
                    }

                    if (unreleasedOperationWip.Count != 0)
                    {
                        //Storing unreleased WIP as additional transaction wip
                        foreach (var moveOperationQtyTotal in moveOperationQtyTotals.OperationTotalsList)
                        {
                            if (unreleasedOperationWip.TryGetValue(moveOperationQtyTotal.Key, out var keyValue))
                            {
                                moveOperationQtyTotals.SetAdditionalOperationWipCost(moveOperationQtyTotal.ProdOper, keyValue);
                            }
                        } 
                    }
                }

                if(amProdItem.CostMethod == CostMethod.Actual && _HasOverheadByBackflushMaterial && RequiresScrapTransaction(ammTran))
                {
                    scrapTransactions.Add(new Tuple<AMMTran, MoveOperationQtyTotals>(ammTran, moveOperationQtyTotals));

                    if (productionTranHelper.WIPAdjustmentGraph.batch.Current == null)
                    {
	                    productionTranHelper.SetCurrentScrapWIPAdjustment(ammTran);
                    }

                    // We will rebuild the lines from scratch
                    productionTranHelper.DeleteLinkedWIPAdjustmentLines(ammTran);
                }

                if(noCostChangeCondition)
                {
                    continue;
                }

                if(amProdItem.CostMethod == CostMethod.Actual && _HasOverheadByBackflushMaterial && RequiresScrapTransaction(ammTran))
                {
                    scrapTransactions.Add(new Tuple<AMMTran, MoveOperationQtyTotals>(ammTran, moveOperationQtyTotals));

                    if (productionTranHelper.WIPAdjustmentGraph.batch.Current == null)
                    {
	                    productionTranHelper.SetCurrentScrapWIPAdjustment(ammTran);
                    }

                    // We will rebuild the lines from scratch
                    productionTranHelper.DeleteLinkedWIPAdjustmentLines(ammTran);
                }

                if(noCostChangeCondition)
                {
                    continue;
                }

                unitCostsUpdated |= UpdateUnitCosts(ref productionGLTranBuilder,
                new ProductionCostCalculator(this).CalculateCompletedUnitCostByOperation(ammTran, amProdItem,
                    moveOperationQtyTotals), ammTran, inTran);
            }

            CreateScrapWipAdjustment(productionTranHelper, scrapTransactions);

            return unitCostsUpdated;
        }

        protected virtual bool UpdateUnitCosts(ref ProductionGLTranBuilder productionGLTranBuilder, ProductionOperationCostResults operationResults, AMMTran tran, INTran inTran)
        {
            if (operationResults == null || tran == null || inTran == null)
            {
                return false;
            }

            var unitCost = UomHelper.PriceCostRound(operationResults.UnitAmount);
            var totalCost = PXCurrencyAttribute.BaseRound(this, operationResults.UnitAmount * inTran.Qty.GetValueOrDefault());

            if (unitCost < 0)
            {
                unitCost = 0m;
                totalCost = 0m;
            }

            if (unitCost != tran.UnitCost.GetValueOrDefault() || totalCost != tran.TranAmt.GetValueOrDefault())
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"{tran.DebuggerDisplay}: unit cost change {tran.UnitCost} to {unitCost} and/or total cost {tran.TranAmt} to {totalCost}");
#endif
                tran.UnitCost = unitCost;
                var updatedAmmTran = TranRecs.Update(tran);
                if (updatedAmmTran == null)
                {
                    return false;
                }

                updatedAmmTran.TranAmt = totalCost;
                updatedAmmTran = TranRecs.Update(updatedAmmTran);
                productionGLTranBuilder.SetOperationWipCompleteRecords(updatedAmmTran, operationResults);

                inTran.UnitCost = unitCost;
                var updatedInTran = INTranRecs.Update(inTran);
                if (updatedInTran == null)
                {
                    //ok for true when the AM transaction is updated
                    return true;
                }

                updatedInTran.TranCost = totalCost;
                INTranRecs.Update(inTran);
                return true;
            }

            return false;
        }

        protected virtual ProductionGLTranBuilder GetCostBatchTranBuilder(AMBatch doc)
        {
            return FindProdCostBatch(doc, false);
        }

        internal bool _HasOverheadByBackflushMaterial = false;
        /// <summary>
        /// Due to lag in backflush material and cost transactions processing at the same time this will create the overhead by material with the difference 
        /// based on the released backflush material transaction. No need to update the production overhead record as it will update correctly during the update order cleanup process.
        /// </summary>
        protected virtual void AddOverheadByBackflushMaterial(AMBatch sourceDoc, ref ProductionGLTranBuilder productionGLTranBuilder)
        {
            if (sourceDoc == null || productionGLTranBuilder == null)
            {
                return;
            }

            AMBatch materialAmBatch = PXSelect<AMBatch,
                Where<AMBatch.origBatNbr, Equal<Required<AMBatch.origBatNbr>>,
                    And<AMBatch.origDocType, Equal<Required<AMBatch.origDocType>>,
                        And<AMBatch.released, Equal<True>,
                                And<AMBatch.docType, Equal<AMDocType.material>>>
                            >>>.Select(this, sourceDoc.BatNbr, sourceDoc.DocType);

            if (materialAmBatch == null
                || materialAmBatch.OrigBatNbr != sourceDoc.BatNbr
                || materialAmBatch.DocType != AMDocType.Material
                // material batch must be released
                || !materialAmBatch.Released.GetValueOrDefault())
            {
                return;
            }

            var processedOverheads = new HashSet<string>();
            foreach (PXResult<AMMTran, AMProdItem, AMProdOper, AMProdOvhd, AMOverhead, AMOverheadCurySettings> result in PXSelectJoin<AMMTran,
                InnerJoin<AMProdItem, On<AMMTran.orderType, Equal<AMProdItem.orderType>,
                    And<AMMTran.prodOrdID, Equal<AMProdItem.prodOrdID>>>,
                InnerJoin<AMProdOper, On<AMProdItem.orderType, Equal<AMProdOper.orderType>,
                    And<AMProdItem.prodOrdID, Equal<AMProdOper.prodOrdID>>>,
                InnerJoin<AMProdOvhd, On<AMProdOper.orderType, Equal<AMProdOvhd.orderType>,
                    And<AMProdOper.prodOrdID, Equal<AMProdOvhd.prodOrdID>,
                    And<AMProdOper.operationID, Equal<AMProdOvhd.operationID>>>>,
                InnerJoin<AMOverhead, On<AMProdOvhd.ovhdID, Equal<AMOverhead.ovhdID>,
                    And<AMOverhead.ovhdType, Equal<OverheadType.varMatlCost>>>,
				LeftJoin<AMOverheadCurySettings, On<AMOverheadCurySettings.ovhdID, Equal<AMOverhead.ovhdID>,
					And<AMOverheadCurySettings.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>>>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>>.Select(this, sourceDoc.DocType, sourceDoc.BatNbr))
            {
                var ammTran = (AMMTran)result;
                var amProdItem = (AMProdItem)result;
                var amProdOvhd = (AMProdOvhd)result;
                var amOverhead = (AMOverhead)result;
				var curysettings = (AMOverheadCurySettings)result;


				if (amProdOvhd == null || string.IsNullOrWhiteSpace(amProdOvhd.OvhdID)
                    || amOverhead == null || string.IsNullOrWhiteSpace(amOverhead.OvhdID)
                    || ammTran == null || string.IsNullOrWhiteSpace(ammTran.BatNbr)
                    || amProdItem == null || string.IsNullOrWhiteSpace(amProdItem.ProdOrdID))
                {
                    continue;
                }

                // Query will return repeat rows depending on amount of re-use of production order on a single move batch... this makes it unique to only processing the necessary overheads once
                if (!processedOverheads.Add($"{amProdOvhd.OrderType.TrimIfNotNullEmpty()}{amProdOvhd.ProdOrdID.TrimIfNotNullEmpty()}{amProdOvhd.OperationID.GetValueOrDefault()}{amProdOvhd.LineID}"))
                {
                    continue;
                }

                AMMTran bfMaterialAmmTran = PXSelectGroupBy<AMMTran,
                    Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                        And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>,
                        And<AMMTran.released, Equal<True>,
                        And<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
                        And<AMMTran.prodOrdID, Equal<Required<AMMTran.prodOrdID>>,
                        And<AMMTran.operationID, Equal<Required<AMMTran.operationID>>>>>>>>,
                    Aggregate<Sum<AMMTran.tranAmt>>>.Select(this, materialAmBatch.DocType, materialAmBatch.BatNbr, amProdOvhd.OrderType, amProdOvhd.ProdOrdID, amProdOvhd.OperationID);

                if (bfMaterialAmmTran == null)
                {
                    continue;
                }

                decimal overheadCost = 0;
                if (bfMaterialAmmTran.TranAmt.GetValueOrDefault() != 0)
                {					 
                    overheadCost = amProdOvhd.OFactor.GetValueOrDefault() * (curysettings?.CostRate ?? 0) * bfMaterialAmmTran.TranAmt.GetValueOrDefault();
                }

                if (overheadCost != 0)
                {
#if DEBUG
                    var debugOper = (AMProdOper)result;
                    AMDebug.TraceWriteMethodName($"Adding overhead {amProdOvhd?.OvhdID} for amount {overheadCost} to order {debugOper?.OrderType}-{debugOper?.ProdOrdID}-{debugOper?.OperationCD}({debugOper?.OperationID})");
#endif
                    productionGLTranBuilder.CreateAmGlLine(
                        amProdItem,
                        ammTran,
                        overheadCost,
                        amOverhead.AcctID,
                        amOverhead.SubID,
                        AMTranType.VarOvhd,
                        amProdOvhd.OperationID,
                        ProductionTransactionHelper.BuildTransactionDescription(AMTranType.VarOvhd, amProdOvhd.OvhdID),
                        null,
                        amProdOvhd.OvhdID,
						bfMaterialAmmTran.MatlLineId);
                    
                    amProdOvhd.TotActCost = amProdOvhd.TotActCost.GetValueOrDefault() + overheadCost;
                    ProductionOvhd.Update(amProdOvhd);
                    _HasOverheadByBackflushMaterial = true;
                }
            }

            if (productionGLTranBuilder.HasAmTransactions || productionGLTranBuilder.HasGlTransactions)
            {
                productionGLTranBuilder.Save();
            }
        }

		protected virtual void UpdateNonBackflushMaterialStatus(AMProdItem prodItem)
		{
			if (prodItem?.ProdOrdID == null)
			{
				return;
			}

			foreach (AMProdMatl prodMatl in SelectFrom<AMProdMatl>
				.Where<AMProdMatl.orderType.IsEqual<@P.AsString>
					.And<AMProdMatl.prodOrdID.IsEqual<@P.AsString>
					.And<AMProdMatl.bFlush.IsEqual<False>>>>
				.View.Select(this, prodItem.OrderType, prodItem.ProdOrdID))
			{
				var cachedMatl = ProductionMatl.Cache.LocateElse(prodMatl);
				var matlStatus = ProductionDetailGraph.Value.GetMaterialStatus(prodItem, cachedMatl);
				if (matlStatus != null && cachedMatl.StatusID != matlStatus)
				{
					cachedMatl.StatusID = matlStatus;
					ProductionMatl.Update(cachedMatl);
				}
			}
		}

		protected virtual void UpdateProductionOrders(AMBatch doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
#endif
            //Step 1: update production item/operation
            UpdateProductionOrdersForMoveLabor(doc);
            UpdateDisassemblyProductionOrder(doc);

            //Step 2: update production material
            if (AMDocType.IsDocTypeMaterial(doc.DocType))
            {
                UpdateProductionMaterial(doc);
                AddMaterialToOrder(doc);
				StoreLotSerialRelationship(doc);
			}

            UpdateAMProdItemStatus(doc);
#if DEBUG
            }
            finally
            {
                sw.Stop();
                PXTraceHelper.TraceWrite(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, $"{AMDocType.GetDocTypeDesc(doc.DocType)} {doc.BatNbr} UpdateProductionOrders Runtime"));
            }
#endif
        }

        /// <summary>
        /// Update production order based on disassembly document
        /// </summary>
        protected virtual void UpdateDisassemblyProductionOrder(AMBatch doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            if (doc.DocType != AMDocType.Disassembly)
            {
                return;
            }

            UpdateProductionOrders(doc, PXSelectJoin<
                AMMTran,
                LeftJoin<InventoryItem,
                        On<AMMTran.inventoryID, Equal<InventoryItem.inventoryID>>,
                LeftJoin<AMOrderType, 
                    On<AMMTran.orderType, Equal<AMOrderType.orderType>>,
                LeftJoin<AMProdItem, 
                    On<AMMTran.orderType, Equal<AMProdItem.orderType>,
                    And<AMMTran.prodOrdID, Equal<AMProdItem.prodOrdID>>>,
                LeftJoin<AMProdOper, 
                    On<AMMTran.orderType, Equal<AMProdOper.orderType>,
                    And<AMMTran.prodOrdID, Equal<AMProdOper.prodOrdID>,
                    And<AMMTran.operationID, Equal<AMProdOper.operationID>>>>>>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>,
                    And<AMMTran.lineNbr, Equal<Required<AMMTran.lineNbr>>>>>
            >
                .Select(this, doc.DocType, doc.BatNbr, doc.RefLineNbr));
        }

        /// <summary>
        /// Update production order based on move or labor document
        /// </summary>
        protected virtual void UpdateProductionOrdersForMoveLabor(AMBatch doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            if (doc.DocType != AMDocType.Labor && doc.DocType != AMDocType.Move)
            {
                return;
            }

            UpdateProductionOrders(doc, PXSelectJoin<
                AMMTran,
                LeftJoin<InventoryItem, 
                    On<AMMTran.inventoryID, Equal<InventoryItem.inventoryID>>,
                LeftJoin<AMOrderType, 
                    On<AMMTran.orderType, Equal<AMOrderType.orderType>>,
                LeftJoin<AMProdItem, 
                    On<AMMTran.orderType, Equal<AMProdItem.orderType>,
                    And<AMMTran.prodOrdID, Equal<AMProdItem.prodOrdID>>>,
                LeftJoin<AMProdOper, 
                    On<AMMTran.orderType, Equal<AMProdOper.orderType>,
                    And<AMMTran.prodOrdID, Equal<AMProdOper.prodOrdID>,
                    And<AMMTran.operationID, Equal<AMProdOper.operationID>>>>>>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>
            >
                .Select(this, doc.DocType, doc.BatNbr));
        }


        protected virtual void UpdateProductionOrders(AMBatch doc, PXResultset<AMMTran> results)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            var productionTranHelper = new ProductionTransactionHelper(this) { IncludeScrap = IncludeScrap };
            var moveOperationQtyBuilder = MoveOperationQtyBuilder.Construct(this, results);
			var resetInProcessProdItems = new List<AMProdItem>();

			foreach (PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper> result in results)
            {
                var ammTran = TranRecs.Cache.LocateElse((AMMTran)result);
                var amProdItem = FindInCache((AMProdItem)result);
                var amProdOper = FindInCache((AMProdOper)result);

                if (string.IsNullOrWhiteSpace(ammTran.ProdOrdID))
                {
                    //Indirect labor would have an empty prodordid...
                    continue;
                }

                if (string.IsNullOrWhiteSpace(amProdItem?.ProdOrdID))
                {
                    throw new PXException(Messages.GetLocal(Messages.InvalidProductionNbr, ammTran.OrderType.TrimIfNotNullEmpty(), ammTran.ProdOrdID.TrimIfNotNullEmpty()));
                }

                if (amProdOper?.OperationID == null)
                {
                    PXTrace.WriteError(Messages.GetLocal(Messages.InvalidProductionOperationNbr,
                        ammTran.OrderType.TrimIfNotNullEmpty(), ammTran.ProdOrdID.TrimIfNotNullEmpty(), ammTran.OperationID));

                    throw new PXException(Messages.InvalidOperationOnTransaction, AMDocType.GetDocTypeDesc(ammTran.DocType), ammTran.BatNbr, ammTran.LineNbr);
                }

                ProductionItems.Current = amProdItem;
                ProductionOpers.Current = amProdOper;

                if (ammTran.QtyScrapped.GetValueOrDefault() != 0)
                {
                    //Removed formula on AMProdOper.QtyScrapped as it was causing issues with the cache clear on amproditem (was not clearing the formula calculated value).
                    //  we need it to clear as we don't save the calculated values until all related batches are released - then we save to the production order.
                    amProdItem.QtyScrapped = amProdItem.QtyScrapped.GetValueOrDefault() + ammTran.QtyScrapped.GetValueOrDefault();
                    amProdItem = ProductionItems.Update(amProdItem);
                }

                if ((amProdItem.LastOperationID == amProdOper.OperationID || ammTran.IsScrap == true)
                    && (ammTran.Qty.GetValueOrDefault() != 0 || ammTran.QtyScrapped.GetValueOrDefault() != 0))
                {
                    // Assumes the tran qty is in the same units as the prod item. The move/labor graphs should not 
                    //  allow for the UOM to change from what the production order is listed to build so this should be ok.
					if(ammTran.IsScrap != true)
                    amProdItem.QtyComplete += ammTran.Qty;

					if ((IncludeScrap
							? amProdItem.QtyComplete + amProdItem.QtyScrapped.GetValueOrDefault()
							: amProdItem.QtyComplete) < amProdItem.QtytoProd)
					{
						var updated = false;
						if (amProdItem.HasTransactions == false)
						{
							amProdItem.HasTransactions = true;
							updated = true;
						}

						if (amProdItem.Completed == true)
						{
							if (!resetInProcessProdItems.Where(r => r.IsSameOrder(amProdItem)).Any())
							{
								resetInProcessProdItems.Add(amProdItem);
							}

							amProdItem.Completed = false;
							updated = true;
						}

						if (updated)
						{
							amProdItem = ProdDetail.RaiseProdItemEvent(this, amProdItem, AMProdItem.Events.Select(ev => ev.UpdateStatus));
						}
					}

					if ((IncludeScrap
                            ? amProdItem.QtyComplete + amProdItem.QtyScrapped.GetValueOrDefault()
                            : amProdItem.QtyComplete) >= amProdItem.QtytoProd
                        && amProdItem.Completed != true)
                    {
						var newCloseOrderEvent = ProductionEventHelper.BuildStatusEvent(amProdItem,
                            amProdItem.StatusID, ProductionOrderStatus.Completed);
                        if (newCloseOrderEvent != null)
                        {
                            newCloseOrderEvent.Description =
                                Messages.GetLocal(Messages.ProductionEventOrderCompleted,
                                    AMDocType.GetDocTypeDesc(doc.DocType), doc.BatNbr,
                                    newCloseOrderEvent.Description);
                            ProductionEvents.Insert(newCloseOrderEvent);
                        }
						amProdItem.HasTransactions = true;
						amProdItem.Completed = true;
						amProdItem = ProdDetail.RaiseProdItemEvent(this, amProdItem, AMProdItem.Events.Select(ev => ev.UpdateStatus));
					}

					// Only required to resolve existing data issues - main fix for AC-240815 is in INReleaseProcessAMExtension.UpdateParentSalesLine
					//  Can remove this if statement in future versions
					if (amProdItem.DemandPlanID != null && (amProdItem.StatusID == ProductionOrderStatus.Completed || ammTran.Qty.GetValueOrDefault() < 0))
					{
						var itemPlan = IN.INItemPlan.PK.Find(this, amProdItem.DemandPlanID);
						if (itemPlan == null)
						{
							amProdItem.DemandPlanID = null;
						}
					}

					amProdItem = ProductionItems.Update(amProdItem);

					var isPreassignedTransaction = amProdItem.PreassignLotSerial.GetValueOrDefault() && (ammTran.LastOper == true || ammTran.IsScrap == true);
					if(isPreassignedTransaction)
					{
						var splits = (PXResultset<AMMTranSplitProdMatlLotSerial>)PXSelectJoin<AMMTranSplitProdMatlLotSerial,
							InnerJoin<AMProdItemSplit, On<AMMTranSplitProdMatlLotSerial.lotSerialNbr, Equal<AMProdItemSplit.lotSerialNbr>,
							And<AMMTranSplitProdMatlLotSerial.orderType, Equal<AMProdItemSplit.orderType>,
							And<AMMTranSplitProdMatlLotSerial.prodOrdID, Equal<AMProdItemSplit.prodOrdID>>>>>,
							Where<AMMTranSplitProdMatlLotSerial.docType, Equal<Required<AMMTranSplitProdMatlLotSerial.docType>>,
							And<AMMTranSplitProdMatlLotSerial.batNbr, Equal<Required<AMMTranSplitProdMatlLotSerial.batNbr>>,
								And<AMMTranSplitProdMatlLotSerial.lineNbr, Equal<Required<AMMTranSplitProdMatlLotSerial.lineNbr>>,
								And<AMProdItemSplit.orderType, Equal<Required<AMProdItemSplit.orderType>>,
								And<AMProdItemSplit.prodOrdID, Equal<Required<AMProdItemSplit.prodOrdID>>>>>>>>
								.Select(this, ammTran.DocType, ammTran.BatNbr, ammTran.LineNbr, ammTran.OrderType, ammTran.ProdOrdID);
						foreach (PXResult<AMMTranSplitProdMatlLotSerial, AMProdItemSplit> split in splits)
						{
							var tranSplit = (AMMTranSplitProdMatlLotSerial)split;
							var itemSplit = prodItemSplits.Cache.LocateElse((AMProdItemSplit)split);
							if (ammTran.IsScrap == true)
							{
								itemSplit.QtyScrapped += (Math.Abs(tranSplit.Qty.GetValueOrDefault()) * tranSplit.TranInvtMult);
								itemSplit.BaseQtyScrapped += (Math.Abs(tranSplit.BaseQty.GetValueOrDefault()) * tranSplit.TranInvtMult);
							}
							else
							{
								itemSplit.QtyComplete += (Math.Abs(tranSplit.Qty.GetValueOrDefault()) * tranSplit.TranInvtMult);
								itemSplit.BaseQtyComplete += (Math.Abs(tranSplit.BaseQty.GetValueOrDefault()) * tranSplit.TranInvtMult);
							}

							prodItemSplits.Update(itemSplit);
						}
					}
					else
					{
						// Update the Production Item Split records
						AMProdItemSplit splitRecord = PXSelect<
							AMProdItemSplit,
							Where<AMProdItemSplit.orderType, Equal<Required<AMProdItemSplit.orderType>>,
								And<AMProdItemSplit.prodOrdID, Equal<Required<AMProdItemSplit.prodOrdID>>
							>>>
							.Select(this, amProdItem.OrderType, amProdItem.ProdOrdID);

						// This assumes only a single split record?
						if (splitRecord != null)
						{
							splitRecord.QtyScrapped += ammTran.QtyScrapped;
							splitRecord.BaseQtyScrapped += ammTran.BaseQtyScrapped;
							if(ammTran.IsScrap != true)
							{
								splitRecord.QtyComplete += ammTran.BaseQty;
								splitRecord.BaseQtyComplete += ammTran.BaseQty;
							}

							prodItemSplits.Update(splitRecord);
						}
					}
				}

                amProdOper.ActualLabor += ammTran.ExtCost.GetValueOrDefault();
                amProdOper.ActualLaborTime += ammTran.LaborTime.GetValueOrDefault();
                amProdOper.QtyScrapped += ammTran.QtyScrapped.GetValueOrDefault();
                amProdOper.BaseQtyScrapped += ammTran.BaseQtyScrapped.GetValueOrDefault();
                ProductionOpers.Update(amProdOper);

                var moveOperationQtyTotals = moveOperationQtyBuilder?.GetMoveOperationTotals(ammTran);
                if (moveOperationQtyTotals == null)
                {
                    continue;
                }

                if (ammTran.QtyScrapped.GetValueOrDefault() != 0)
                {
                    // reporting scrap and not including scrap will increase the overall operation total which drives total material...
                    ProductionTransactionHelper.UpdateOperationQty(this, amProdItem, moveOperationQtyTotals.OperationsList.LocateElseCopy(this), productionTranHelper.IncludeScrap);
                }

				ProductionItems.Current = amProdItem;
                productionTranHelper.BuildRelatedTransactions(ammTran, moveOperationQtyTotals);
            }

            var orderHashSet = new HashSet<string>();
            foreach (PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper> result in results)
            {
                var amProdItem = FindInCache((AMProdItem)result);
                if (amProdItem?.ProdOrdID == null || 
                    !orderHashSet.Add(string.Join(":", amProdItem.OrderType, amProdItem.ProdOrdID)))
                {
                    return;
                }

                ProductionScheduleEngine.UpdateScheduleQuantities(this, amProdItem);
            }
			foreach (var resetInProcessOrder in resetInProcessProdItems)
			{
				UpdateNonBackflushMaterialStatus(resetInProcessOrder);
			}
		}

        private void UpdateAMProdItemStatus(AMBatch doc)
        {
            if (string.IsNullOrWhiteSpace(doc?.BatNbr))
            {
                throw new PXArgumentException(nameof(doc));
            }

            foreach (PXResult<AMMTran, AMProdItem> result in PXSelectJoin<AMMTran,
                InnerJoin<AMProdItem, On<AMMTran.orderType, Equal<AMProdItem.orderType>,
                    And<AMMTran.prodOrdID, Equal<AMProdItem.prodOrdID>>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>,
                        And<AMMTran.released, Equal<True>>>>>.Select(this, doc.DocType, doc.BatNbr))
            {
                var prodItem = FindInCache((AMProdItem)result);

				if (prodItem != null && prodItem.Released == true && prodItem.HasTransactions == false)
				{
					prodItem.HasTransactions = true;
					prodItem = ProdDetail.RaiseProdItemEvent(this, prodItem, AMProdItem.Events.Select(ev => ev.UpdateStatus));
				}
			}
        }

        /// <summary>
        /// Select the correct document process method based on batch document type
        /// </summary>
        /// <param name="doc">Document being released</param>
        protected virtual void DocumentProcessSelector(AMBatch doc)
        {
            if (doc?.DocType == null)
            {
                throw new ArgumentException(nameof(doc));
            }

            switch (doc.DocType)
            {
                case AMDocType.Labor:
                case AMDocType.Move:
                    ProcessMoveTran(doc);
                    break;

                case AMDocType.Material:
                    ProcessMaterialTran(doc);
                    break;

                case AMDocType.WipAdjust:
                    ProcessWipAdjustment(doc);
                    break;

                case AMDocType.ProdCost:
                    ProcessAmGlTran(doc);
                    break;

                case AMDocType.Disassembly:
                    ProcessDisassembly(doc);
                    break;

                default:
                    throw new PXException($"{Messages.GetLocal(Messages.UnknownDocType)} '{doc.DocType}'");
            }
        }

        private bool TryGetInventoryTotalTranAmount(AMMTran tran, List<PXResult<AMMTran,AMProdOper>> relatedTranLines, out decimal? inTranAmount)
        {
            inTranAmount = null;
            if (tran.TranType != AMTranType.ScrapQuarantine)
            {
                return false;
            }

            var total = 0m;
            var isFirst = true;
            foreach (PXResult<AMMTran, AMProdOper> result in relatedTranLines.OrderByDescending(x => ((AMProdOper)x).OperationCD))
            {
                var relatedTran = (AMMTran) result;
                if (relatedTran == null || !AMMTran.IsSameOrigLine(tran, relatedTran))
                {
                    continue;
                }

                if (isFirst && tran.OperationID != relatedTran.OperationID )
                {
                    // We are only creating the IN entry on the matching operation. All other operations do not get an IN or GL entry
                    return false;
                }
                isFirst = false;
                total += relatedTran.TranAmt.GetValueOrDefault();
            }
            inTranAmount = total;
            return true; // inTranAmount.GetValueOrDefault() != 0; //we are now allowing zero cost inventory transactions
        }

        protected virtual void ProcessWipAdjustment(AMBatch doc)
        {
            var productionGlTranBuilder = new ProductionGLTranBuilder(FindResetRelatedWipAdjustmentBatch(doc));
            var adjustmentBuilder = CreateInventoryTranBuilder(doc, InventoryTranBuilder.TransactionType.Adjustment);
            var receiptBuilder = CreateInventoryTranBuilder(doc, InventoryTranBuilder.TransactionType.Receipt);

			adjustmentBuilder.DeleteAllTranLines();
			receiptBuilder.DeleteAllTranLines();			

            var refBatchReleasedDictionary = ReferencedBatchesReleased(doc);

            PXResultset<AMMTran> resultSet = PXSelectJoin<AMMTran,
                InnerJoin<AMProdOper, On<AMMTran.orderType, Equal<AMProdOper.orderType>,
                    And<AMMTran.prodOrdID, Equal<AMProdOper.prodOrdID>,
                    And<AMMTran.operationID, Equal<AMProdOper.operationID>>>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>>.Select(this, doc.DocType, doc.BatNbr);

            foreach (PXResult<AMMTran, AMProdOper> result in resultSet)
            {
                var ammTran = (AMMTran) result;

                if (refBatchReleasedDictionary.ContainsKey(ammTran.LineNbr.GetValueOrDefault())
                    && refBatchReleasedDictionary[ammTran.LineNbr.GetValueOrDefault()])
                {
                    continue;
                }

                TranRecs.Update(productionGlTranBuilder.CreateGlLine(ammTran));

                if (ammTran.IsScrap != true || ammTran.TranType != AMTranType.ScrapQuarantine ||
                    !TryGetInventoryTotalTranAmount(ammTran, resultSet.ToLocatedList<AMMTran, AMProdOper>(this), out var inTranAmount))
                {
                    continue;
                }

                // Create an Inventory Transaction for Quarantined Scrap. User request ot have zero cost scrap quarantine inventory transaction
                var updatedTranRec = ammTran.QtyScrapped.GetValueOrDefault() <= 0
                    ? adjustmentBuilder.CreateScrapTranLine(ammTran, inTranAmount.GetValueOrDefault())
                    : receiptBuilder.CreateScrapTranLine(ammTran, inTranAmount.GetValueOrDefault());

                if (updatedTranRec != null)
                {
                    TranRecs.Update(updatedTranRec);
                }
            }

            if (adjustmentBuilder?.RefDocNoteRecordRequired == true || receiptBuilder?.RefDocNoteRecordRequired == true)
            {
                EnsureDocNote(doc);
            }

            using (var ts = new PXTransactionScope())
            {
                if (productionGlTranBuilder.HasGlTransactions)
                {
                    productionGlTranBuilder.SyncJournalEntryBatch(doc);
                    productionGlTranBuilder.Save();
                    GeneralLedgerBatches.Add(productionGlTranBuilder.CurrentGlDocument);
                }
                else
                {
                    if (productionGlTranBuilder.CurrentGlDocument != null)
                    {
                        PXTraceHelper.WriteInformation($"Deleting journal entry batch '{productionGlTranBuilder.CurrentGlDocument.BatchNbr.TrimIfNotNullEmpty()}' due to lack of transaction detail");
                    }
                    productionGlTranBuilder.DeleteJournalEntryBatchOnly();

                    productionGlTranBuilder.Save();
                }

                if (adjustmentBuilder != null)
                {
                    if (adjustmentBuilder.HasInTransactions)
                    {
                        adjustmentBuilder.SyncBatch(doc);
                        adjustmentBuilder.Save();
                        InventoryBatches.Add(adjustmentBuilder.CurrentInDocument);
                    }
                    else
                    {
                        if (adjustmentBuilder.CurrentInDocument != null)
                        {
                            PXTraceHelper.WriteInformation($"Deleting (WIP) IN adjustment entry batch '{adjustmentBuilder.CurrentInDocument.DocType.TrimIfNotNullEmpty()}-{adjustmentBuilder.CurrentInDocument.RefNbr.TrimIfNotNullEmpty()}' due to lack of transaction detail");
                            adjustmentBuilder.DeleteINBatchOnly();
                        }
                        if (adjustmentBuilder.CurrentGlDocument != null)
                        {
                            PXTraceHelper.WriteInformation($"Deleting (WIP) GL adjustment entry batch '{adjustmentBuilder.CurrentGlDocument.BatchNbr.TrimIfNotNullEmpty()}' due to lack of transaction detail");
                            adjustmentBuilder.DeleteGLBatchOnly();
                        }
                        adjustmentBuilder.Save();
                    }
                }

                if (receiptBuilder != null)
                {
                    if (receiptBuilder.HasInTransactions)
                    {
                        receiptBuilder.SyncBatch(doc);
                        receiptBuilder.Save();
                        InventoryBatches.Add(receiptBuilder.CurrentInDocument);
                    }
                    else
                    {
                        if (receiptBuilder.CurrentInDocument != null)
                        {
                            PXTraceHelper.WriteInformation($"Deleting (WIP) IN receipt entry batch '{receiptBuilder.CurrentInDocument.DocType.TrimIfNotNullEmpty()}-{receiptBuilder.CurrentInDocument.RefNbr.TrimIfNotNullEmpty()}' due to lack of transaction detail");
                            receiptBuilder.DeleteINBatchOnly();
                        }
                        if (receiptBuilder.CurrentGlDocument != null)
                        {
                            PXTraceHelper.WriteInformation($"Deleting (WIP) GL receipt entry batch '{receiptBuilder.CurrentGlDocument.BatchNbr.TrimIfNotNullEmpty()}' due to lack of transaction detail");
                            receiptBuilder.DeleteGLBatchOnly();
                        }
                        receiptBuilder.Save();
                    }
                }

                if (IsDirty)
                {
                    Actions.PressSave();
                }
                ts.Complete();
            }
        }

        /// <summary>
        /// Mark document and transaction lines as released
        /// </summary>
        /// <param name="doc">Document being released</param>
        protected virtual void MarkBatchReleased(AMBatch doc)
        {
            if (doc.DocType == null)
            {
                return;
            }

            if (doc.DocType == AMDocType.Move || 
                doc.DocType == AMDocType.Labor || 
                doc.DocType == AMDocType.Material ||
                doc.DocType == AMDocType.Disassembly)
            {
                MarkBatchReleasedWithINTran(doc);
                return;
            }

            foreach (AMMTran ammTran in
                PXSelect<
                    AMMTran,                 
                    Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                        And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>>
                    .Select(this, doc.DocType, doc.BatNbr))
            {
                if (ammTran?.BatNbr == null || ammTran.Released == true)
                {
                    continue;
                }

                ReferencedProductionOrders.Add(ammTran);

                ammTran.Released = true;
                TranRecs.Update(ammTran);
            }

            doc.Released = true;
            BatchRecs.Update(doc);

            InsertProdTransactionEvents(doc);
        }

        /// <summary>
        /// Mark document and transaction lines as released for a document related to Inventory
        /// </summary>
        /// <param name="doc">Document being released</param>
        private void MarkBatchReleasedWithINTran(AMBatch doc)
        {
            foreach (PXResult<AMMTran, INTran> result in
                PXSelectJoin<
                    AMMTran,
                    LeftJoin<INTran, 
                        On<AMMTran.iNDocType, Equal<INTran.docType>,
                        And<AMMTran.iNBatNbr, Equal<INTran.refNbr>,
                        And<AMMTran.iNLineNbr, Equal<INTran.lineNbr>>>>>,
                    Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                        And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>>
                    .Select(this, doc.DocType, doc.BatNbr))
            {
                var ammTran = (AMMTran)result;
                var inTran = (INTran)result;
                
                if (ammTran?.BatNbr == null || ammTran.Released == true)
                {
                    continue;
                }

                ReferencedProductionOrders.Add(ammTran);

                //True up the IN unit cost if available (skip if IN Adjustment as there could be many adjustment entries for 1 AMMTran and the UOM could also be diff. Adjustments cost should not change)
				if (ammTran.DocType != AMDocType.WipAdjust && !string.IsNullOrWhiteSpace(inTran?.RefNbr) && inTran.UnitCost != null)
                {
                    var newMfgTranAmt = inTran.TranCost.GetValueOrDefault();
                    if (!ammTran.TranAmt.GetValueOrDefault().IsSameSign(newMfgTranAmt))
                    {
                        newMfgTranAmt *= -1;
                    }

					if (ammTran.UOM == inTran.UOM && ammTran.UnitCost != inTran.UnitCost)
                    {
                    ammTran.UnitCost = inTran.UnitCost;
						ammTran = TranRecs.Update(ammTran);
					}

                    ammTran.TranAmt = newMfgTranAmt;
                }

				ammTran.Released = true;
                TranRecs.Update(ammTran);
            }

			PXFormulaAttribute.CalcAggregate<AMMTran.tranAmt>(TranRecs.Cache, doc);
			doc.Released = true;
			var updatedDoc = BatchRecs.Update(doc);

            InsertProdTransactionEvents(updatedDoc);
        }

        /// <summary>
        /// Process Manufacturing GL transactions
        /// </summary>
        /// <param name="doc"></param>
        protected virtual void ProcessAmGlTran(AMBatch doc)
        {
            var hashSet = new HashSet<string>();

            foreach (var batch in GeneralLedgerBatches)
            {
                hashSet.Add(batch.BatchNbr);
            }

            // CHECK GL BATCH REFERENCED TO THE PASSED BATCH LIST
            foreach (Batch unRelGLBatch in PXSelectJoin<Batch,
                InnerJoin<AMMTran,
                On<Batch.batchNbr, Equal<AMMTran.gLBatNbr>,
                        And<Batch.module, Equal<BatchModule.moduleGL>>>>,
                Where<Batch.released, NotEqual<True>,
                    And<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                        And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>>
                >.Select(this, doc.DocType, doc.BatNbr))
            {
                if (hashSet.Add(unRelGLBatch.BatchNbr))
                {
                    GeneralLedgerBatches.Add(unRelGLBatch);
                }
            }
        }

        private bool IsGLInventory(AMMTran row)
        {
            return row != null && (row.IsScrap.GetValueOrDefault() || !row.IsStockItem.GetValueOrDefault());
        }

        /// <summary>
        /// Does the given transaction require a scrap WIP Adjustment transaction
        /// </summary>
        public virtual bool RequiresScrapTransaction(AMMTran row)
        {
            return row != null && (row.DocType == AMDocType.Labor ||
                                   row.DocType == AMDocType.Move ||
                                   row.DocType == AMDocType.Disassembly) &&
                   (row.QtyScrapped.GetValueOrDefault() != 0 && row.ScrapAction == ScrapAction.WriteOff ||
                    row.Qty.GetValueOrDefault() != 0 && row.ScrapAction == ScrapAction.Quarantine &&
                    row.IsScrap.GetValueOrDefault());
        }

        /// <summary>
        /// Process Manufacturing disassembly documents
        /// </summary>
        /// <param name="doc"></param>
        protected virtual void ProcessDisassembly(AMBatch doc)
        {
            if (doc?.BatNbr == null)
            {
                throw new NullReferenceException(nameof(doc));
            }

            if (doc.DocType != AMDocType.Disassembly)
            {
                throw new PXException($"Invalid document type {AMDocType.GetDocTypeDesc(doc.DocType)} for {AMDocType.Desc.Disassembly} transaction");
            }

            InventoryTranBuilder issueTranBuilder = null;
            InventoryTranBuilder receiptTranBuilder = null;
            InventoryTranBuilder adjustmentBuilder = null;

            PXResultset<AMMTran> results = PXSelectJoin<
                AMMTran,
                InnerJoin<InventoryItem, 
                    On<AMMTran.inventoryID, Equal<InventoryItem.inventoryID>>,
                LeftJoin<AMOrderType, 
                    On<AMMTran.orderType, Equal<AMOrderType.orderType>>,
                LeftJoin<AMProdItem, 
                    On<AMMTran.orderType, Equal<AMProdItem.orderType>,
                    And<AMMTran.prodOrdID, Equal<AMProdItem.prodOrdID>>>>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>
           >
                .Select(this, doc.DocType, doc.BatNbr);

            var productionTranHelper = new ProductionTransactionHelper(this);
            var prodGLTranBuilder = FindResetProdCostBatch(doc);
            if (prodGLTranBuilder != null)
            {
                productionTranHelper.ProductionGlTranBuilder = prodGLTranBuilder;
            }

            var containsGlInventory = false;
            foreach (AMMTran row in results)
            {
                if (row?.BatNbr != null && IsGLInventory(row))
                {
                    containsGlInventory = true;
                    break;
                }
            }

            JournalEntry journalEntry = null;

            var allTranSplits = PXSelect<
                    AMMTranSplit,
                    Where<AMMTranSplit.docType, Equal<Required<AMMTranSplit.docType>>,
                        And<AMMTranSplit.batNbr, Equal<Required<AMMTranSplit.batNbr>>>>>
                .Select(this, doc.DocType, doc.BatNbr)
                .ToFirstTableList();

            foreach (PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem> result in results)
            {
                var tran = TranRecs.Cache.LocateElse((AMMTran)result);

                if (tran?.LineNbr == null)
                {
                    continue;
                }

                //make sure display fields are in sync
                tran.FinPeriodID = doc.FinPeriodID;
                tran.TranDate = doc.TranDate;

                if (AMDisassembleBatch.IsDisassembleBatchTran(doc, tran))
                {
                    var prodItem = (AMProdItem)result;
                    if (prodItem?.ProdOrdID == null)
                    {
                        throw new PXException(Messages.InvalidProductionNbr, tran.OrderType, tran.ProdOrdID);
                    }

					object prodOrderID = tran?.ProdOrdID;
					TranRecs.View.Cache.RaiseFieldVerifying<AMMTran.prodOrdID>(tran, ref prodOrderID);

                    var batchTranBuilder = (tran.TranType ?? AMTranType.Disassembly) == AMTranType.Adjustment
                        ? adjustmentBuilder ?? (adjustmentBuilder = CreateInventoryTranBuilder(doc, InventoryTranBuilder.TransactionType.Adjustment, false))
                        : issueTranBuilder ?? (issueTranBuilder = CreateInventoryTranBuilder(doc, InventoryTranBuilder.TransactionType.IssueReturn, false));
                    ProcessDisassembleBatchTran(doc, tran, allTranSplits?.Where(x => x.LineNbr == tran.LineNbr).ToList(), (InventoryItem) result, prodItem, productionTranHelper, ref batchTranBuilder);
                    continue;
                }

                var materialTranBuilder = (tran.TranType ?? AMTranType.Receipt) == AMTranType.Adjustment
                    ? adjustmentBuilder ?? (adjustmentBuilder = CreateInventoryTranBuilder(doc, InventoryTranBuilder.TransactionType.Adjustment, false))
                    : receiptTranBuilder ?? (receiptTranBuilder = CreateInventoryTranBuilder(doc, InventoryTranBuilder.TransactionType.Receipt, false));

                // Inside this if we make sure the journal entry graph is the same for both transaction builders to avoid multiple GL batches
                if (containsGlInventory && materialTranBuilder != null && !materialTranBuilder.GlGraphLoaded)
                {
                    if (journalEntry == null)
                    {
                        LoadExistingJournalEntry(doc, ref materialTranBuilder);
                        journalEntry = materialTranBuilder.GlGraphLoaded ? materialTranBuilder.GlGraph : InventoryTranBuilder.ConstructJournalEntry();
                    }

                    if (!materialTranBuilder.GlGraphLoaded && journalEntry != null)
                    {
                        materialTranBuilder.GlGraph = journalEntry;
                    }
                }

                ProcessDisassembleMaterial(tran, allTranSplits?.Where(x => x.LineNbr == tran.LineNbr).ToList(), (InventoryItem)result, ref materialTranBuilder);
            }

            if (issueTranBuilder?.RefDocNoteRecordRequired == true || 
                receiptTranBuilder?.RefDocNoteRecordRequired == true ||
                adjustmentBuilder?.RefDocNoteRecordRequired == true)
            {
                EnsureDocNote(doc);
            }

            using (var ts = new PXTransactionScope())
            {
                issueTranBuilder?.Save();
                receiptTranBuilder?.Save();
                adjustmentBuilder?.Save();

                // INVENTORY (Stock items)
                if (issueTranBuilder?.CurrentInDocument != null)
                {
                    InventoryBatches.Add(issueTranBuilder.CurrentInDocument);
                }
                if (receiptTranBuilder?.CurrentInDocument != null)
                {
                    InventoryBatches.Add(receiptTranBuilder.CurrentInDocument);
                }
                if (adjustmentBuilder?.CurrentInDocument != null)
                {
                    InventoryBatches.Add(adjustmentBuilder.CurrentInDocument);
                }
                //  GENERAL LEDGER (Non stock/scrap items)
                if (issueTranBuilder?.CurrentGlDocument != null)
                {
                    GeneralLedgerBatches.Add(issueTranBuilder.CurrentGlDocument);
                }
                if (receiptTranBuilder?.CurrentGlDocument != null)
                {
                    GeneralLedgerBatches.Add(receiptTranBuilder.CurrentGlDocument);
                }
                if (adjustmentBuilder?.CurrentGlDocument != null)
                {
                    GeneralLedgerBatches.Add(adjustmentBuilder.CurrentGlDocument);
                }

                if (productionTranHelper.IsProductionGLTranBuilderInitialized)
                {
                    if (productionTranHelper.ProductionGlTranBuilder.HasGlTransactions)
                    {
                        GeneralLedgerBatches.Add(productionTranHelper.ProductionGlTranBuilder.CurrentGlDocument);
                    }
                    else if (!productionTranHelper.ProductionGlTranBuilder.HasAmTransactions)
                    {
                        //Possible to have non GL cost entries... only delete here if either do not exist
                        productionTranHelper.ProductionGlTranBuilder.DeleteBatch();
                    }
                    productionTranHelper.ProductionGlTranBuilder.Save();
                }

                if (IsDirty)
                {
                    Actions.PressSave();
                }

                ts.Complete();
            }
        }

        protected virtual void EnsureDocNote(AMBatch doc)
        {
            //This call will get down to EnsureNoteID which will auto create the record for us if it doesn't exist. We don't need note records for all transactions so no need to use AutoNoteIDAttribute.
            PXNoteAttribute.GetNoteID<AMBatch.noteID>(BatchRecs.Cache, doc);
        }

        /// <summary>
        /// Process the disassembly batch/tran row (AMDisassembleBatch when projection).
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="tranBatch"></param>
        /// <param name="splits">Split records related to <see cref="AMMTran"/> record</param>
        /// <param name="inventoryItem"><see cref="InventoryItem"/> related to InventoryID of <see cref="AMMTran"/> record</param>
        /// <param name="prodItem"></param>
        /// <param name="productionTranHelper"></param>
        /// <param name="inventoryBuilder">builder for generating the Inventory related transactions</param>
        protected virtual void ProcessDisassembleBatchTran(AMBatch doc, AMMTran tranBatch, List<AMMTranSplit> splits, InventoryItem inventoryItem, AMProdItem prodItem, ProductionTransactionHelper productionTranHelper, ref InventoryTranBuilder inventoryBuilder)
        {
            // Will process these as split records vs the single AMDisassembleBatch record.
            if (doc?.BatNbr == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            if (tranBatch?.LineNbr == null)
            {
                throw new ArgumentNullException(nameof(tranBatch));
            }

            if (tranBatch.Released.GetValueOrDefault())
            {
                return;
            }

            if (prodItem?.ProdOrdID == null)
            {
                throw new ArgumentNullException(nameof(prodItem));
            }

            if (doc.RefLineNbr == null || doc.RefLineNbr != tranBatch.LineNbr)
            {
                throw new PXException(Messages.GetLocal(Messages.DisassemblyRefLineNbrInvalid, doc.RefLineNbr == null ? "<null>" : tranBatch.LineNbr.ToString()));
            }

            if (inventoryBuilder == null)
            {
                throw new ArgumentNullException(nameof(inventoryBuilder));
            }

            if (tranBatch.OperationID == null || tranBatch.OperationID != prodItem.LastOperationID)
            {
                tranBatch.OperationID = prodItem.LastOperationID;
                tranBatch = TranRecs.Update(tranBatch);
            }
#if DEBUG
            AMDebug.TraceWriteMethodName($"Tran: {tranBatch.DocType} - {tranBatch.BatNbr.TrimIfNotNullEmpty()} - {tranBatch.LineNbr} Order: {tranBatch.OrderType.TrimIfNotNullEmpty()} - {tranBatch.ProdOrdID.TrimIfNotNullEmpty()} - {tranBatch.OperationID}");
#endif

            if (ReferencedProductionOrders.Add(tranBatch))
            {
				object prodOrderID = tranBatch?.ProdOrdID;
				TranRecs.View.Cache.RaiseFieldVerifying<AMMTran.prodOrdID>(tranBatch, ref prodOrderID);
			}

            var moveOperationQtyTotals = MoveOperationQtyBuilder.ConstructSingleBuilder(this, tranBatch);
            productionTranHelper.AmReleaseGraph.ProductionItems.Current = prodItem;
            productionTranHelper.BuildRelatedTransactions(tranBatch, moveOperationQtyTotals);
            TranRecs.Update(inventoryBuilder.CreateTranLine(tranBatch, inventoryItem, splits));
        }

        /// <summary>
        /// Process the disassembly tran (material) row (AMDisassembleTran when projection)
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="splits">Split records related to <see cref="AMMTran"/> record</param>
        /// <param name="inventoryItem"><see cref="InventoryItem"/> related to InventoryID of <see cref="AMMTran"/> record</param>
        /// <param name="inventoryBuilder">builder for generating the Inventory related transactions</param>
        protected virtual void ProcessDisassembleMaterial(AMMTran tran, List<AMMTranSplit> splits, InventoryItem inventoryItem, ref InventoryTranBuilder inventoryBuilder)
        {
            if (tran?.LineNbr == null)
            {
                throw new NullReferenceException(nameof(tran));
            }

            if (inventoryBuilder == null)
            {
                throw new NullReferenceException(nameof(inventoryBuilder));
            }

            var wipIsDebit = tran.TranType == AMTranType.Adjustment;
            if (tran.IsScrap.GetValueOrDefault())
            {
                TranRecs.Update(inventoryBuilder.CreateScrapWriteOffLine(tran, wipIsDebit));
                return;
            }

            if (!tran.IsStockItem.GetValueOrDefault())
            {
                TranRecs.Update(inventoryBuilder.CreateNonStockTranLine(tran, wipIsDebit));
                return;
            }

            // All other should be stock item...
            TranRecs.Update(inventoryBuilder.CreateTranLine(tran, inventoryItem, splits));
        }

        /// <summary>
        /// Checks to see if current document contains references for the given type to reuse the unreleased batches from past release attempt.
        /// If nothing is found then a new IN batch number will be used.
        /// </summary>
        /// <param name="doc">Releasing document</param>
        /// <param name="transactionType">Transaction type (Issue/Receipt)</param>
        protected virtual InventoryTranBuilder CreateInventoryTranBuilder(AMBatch doc, InventoryTranBuilder.TransactionType transactionType)
        {
            return CreateInventoryTranBuilder(doc, transactionType, true);
        }

        /// <summary>
        /// Checks to see if current document contains references for the given type to reuse the unreleased batches from past release attempt.
        /// If nothing is found then a new IN batch number will be used.
        /// </summary>
        /// <param name="doc">Releasing document</param>
        /// <param name="transactionType">Transaction type (Issue/Receipt)</param>
        /// <param name="useExistingJournalEntry">Should the creation of the inventory build re-use any found journal entry batch?</param>
        protected virtual InventoryTranBuilder CreateInventoryTranBuilder(AMBatch doc, InventoryTranBuilder.TransactionType transactionType, bool useExistingJournalEntry)
        {
            return CreateInventoryTranBuilder(doc, transactionType, useExistingJournalEntry, null);
        }

		protected virtual InventoryTranBuilder CreateInventoryTranBuilder(AMBatch doc, InventoryTranBuilder.TransactionType transactionType, bool useExistingJournalEntry, Batch excludedBatch)
		{
            if (transactionType == InventoryTranBuilder.TransactionType.None)
            {
                transactionType = InventoryTranBuilder.TransactionType.Adjustment;
            }

            var inDoctype = InventoryTranBuilder.ConvertToINDocType(transactionType);

            INRegister inRegister = PXSelect
                <
                INRegister, 
                Where<INRegister.docType, Equal<Required<INRegister.docType>>,
                    And<INRegister.released, Equal<False>,
                    And<INRegisterExt.aMDocType, Equal<Required<INRegisterExt.aMDocType>>,
                    And<INRegisterExt.aMBatNbr, Equal<Required<INRegisterExt.aMBatNbr>>>>>>
                    >
                .Select(this, inDoctype, doc.DocType, doc.BatNbr);

            var refDocEntityType = typeof(AMBatch).FullName;
            InventoryTranBuilder inventoryTranBuilder = null;
            if (inRegister == null || inRegister.Released.GetValueOrDefault())
            {
                inventoryTranBuilder = new InventoryTranBuilder(transactionType) { PostPeriod = doc.FinPeriodID, TranDate = doc.TranDate, RefDocNoteId = doc.NoteID, RefDocEntityType = refDocEntityType };
            }
            else if (inDoctype == INDocType.Receipt)
            {
                var receiptEntryGraph = CreateInstance<INReceiptEntry>();
                receiptEntryGraph.Clear();
                receiptEntryGraph.receipt.Current = inRegister;
                inventoryTranBuilder = new InventoryTranBuilder(receiptEntryGraph) { PostPeriod = doc.FinPeriodID, TranDate = doc.TranDate, RefDocNoteId = doc.NoteID, RefDocEntityType = refDocEntityType };
            }
            else if (inDoctype == INDocType.Issue)
            {
                var issueEntryGraph = CreateInstance<INIssueEntry>();
                issueEntryGraph.Clear();
                issueEntryGraph.issue.Current = inRegister;
                inventoryTranBuilder = new InventoryTranBuilder(issueEntryGraph) { PostPeriod = doc.FinPeriodID, TranDate = doc.TranDate, RefDocNoteId = doc.NoteID, RefDocEntityType = refDocEntityType };
            }
            else
            {
                var adjustmentEntryGraph = CreateInstance<INAdjustmentEntry>();
                adjustmentEntryGraph.Clear();
                adjustmentEntryGraph.adjustment.Current = inRegister;
                inventoryTranBuilder = new InventoryTranBuilder(adjustmentEntryGraph) { PostPeriod = doc.FinPeriodID, TranDate = doc.TranDate, RefDocNoteId = doc.NoteID, RefDocEntityType = refDocEntityType };
            }

            // Update Post Period and FinPeriod to ensure Batch in Sync
            inventoryTranBuilder.UpdateInRegister();

            if (useExistingJournalEntry)
            {
				LoadExistingJournalEntry(doc, ref inventoryTranBuilder, excludedBatch);
            }

            return inventoryTranBuilder;
        }

        public virtual void LoadExistingJournalEntry(AMBatch doc, ref InventoryTranBuilder inventoryTranBuilder)
        {
			LoadExistingJournalEntry(doc, ref inventoryTranBuilder, null);
		}

		public virtual void LoadExistingJournalEntry(AMBatch doc, ref InventoryTranBuilder inventoryTranBuilder, Batch excludedBatch)
		{
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            if (inventoryTranBuilder == null)
            {
                throw new ArgumentNullException(nameof(inventoryTranBuilder));
            }

			Batch glBatch = excludedBatch != null ?
				PXSelect<
				Batch,
				Where<Batch.released, Equal<False>,
					And<BatchExt.aMDocType, Equal<Required<BatchExt.aMDocType>>,
					And<BatchExt.aMBatNbr, Equal<Required<BatchExt.aMBatNbr>>,
					And<Batch.module, NotEqual<Required<Batch.module>>,
					And<Batch.batchNbr, NotEqual<Required<Batch.batchNbr>>>>>>>>
				.Select(this, doc.DocType, doc.BatNbr, excludedBatch.Module, excludedBatch.BatchNbr)
			:
				PXSelect<
                Batch, 
                Where<Batch.released, Equal<False>,
                    And<BatchExt.aMDocType, Equal<Required<BatchExt.aMDocType>>,
                    And<BatchExt.aMBatNbr, Equal<Required<BatchExt.aMBatNbr>>>>>>
                .Select(this, doc.DocType, doc.BatNbr);

            //regardless if IN found add GL if it was found and not released
            if (glBatch != null && !glBatch.Released.GetValueOrDefault())
            {
                inventoryTranBuilder.ConstructGlGraph();
                glBatch.FinPeriodID = doc.FinPeriodID;
                glBatch.DateEntered = doc.TranDate;
                inventoryTranBuilder.GlGraph.BatchModule.Current = inventoryTranBuilder.GlGraph.BatchModule.Update(glBatch);
                // Delete to allow the process to rebuild the transactions
                inventoryTranBuilder.DeleteAllGlTrans();
            }
        }

        /// <summary>
        /// Find a related backflush material batch (possible during re-release of transaction)
        /// </summary>
        /// <param name="doc">Move or Labor releasing document</param>
        /// <param name="materialReleased">has the related material transaction been released</param>
        /// <param name="inRegisterReleased">has the related material transaction's inventory transaction been released</param>
        /// <returns>Found MaterialEntry graph, otherwise null</returns>
        protected virtual MaterialEntry FindResetBackflushMaterialBatch(AMBatch doc, out bool materialReleased, out bool inRegisterReleased)
        {
            materialReleased = false;
            inRegisterReleased = false;
            if (doc.DocType != AMDocType.Move && doc.DocType != AMDocType.Labor)
            {
                return null;
            }

            var bfMaterialBatch = (AMBatch) PXSelect<AMBatch,
                Where<AMBatch.origBatNbr, Equal<Required<AMBatch.origBatNbr>>,
                    And<AMBatch.origDocType, Equal<Required<AMBatch.origDocType>>,
                        And<AMBatch.batNbr, NotEqual<Required<AMBatch.batNbr>>,
							 And<AMBatch.docType, Equal<Required<AMBatch.docType>>>
						>>>>.Select(this, doc.BatNbr, doc.DocType, doc.BatNbr, AMDocType.Material);

            if (bfMaterialBatch == null)
            {
                return null;
            }

            //Will only hit this call on rerelease of batch which is not normal - ok as 2nd query
            var inRegister = (INRegister)PXSelect<INRegister,
                Where<INRegisterExt.aMDocType, Equal<Required<INRegisterExt.aMDocType>>,
                    And<INRegisterExt.aMBatNbr, Equal<Required<INRegisterExt.aMBatNbr>>>>
            >.Select(this, bfMaterialBatch.DocType, bfMaterialBatch.BatNbr);

            materialReleased = bfMaterialBatch.Released == true;
            inRegisterReleased = inRegister?.Released == true;

#if DEBUG
            AMDebug.TraceWriteMethodName($"Re-release of Material transaction {bfMaterialBatch.BatNbr} with released = {bfMaterialBatch.Released}; IN reference {inRegister?.DocType} {inRegister?.RefNbr} with released = {inRegisterReleased}");
#endif
            if (materialReleased || inRegisterReleased)
            {
                return null;
            }

            //ready to clear out the lines
            var materialEntry = CreateInstance<MaterialEntry>();
            materialEntry.Clear();
            materialEntry.batch.Current = bfMaterialBatch;
            if (materialEntry.ampsetup.Current != null)
            {
                materialEntry.ampsetup.Current.RequireControlTotal = false;
            }
            materialEntry.batch.Cache.IsDirty = false;

            foreach (AMMTran row in materialEntry.transactions.Select())
            {
                if (row.Released.GetValueOrDefault())
                {
                    continue;
                }

                materialEntry.transactions.Delete(row);
            }

            if (materialEntry.transactions.Cache.Deleted.Any_())
            {
                //For some reason (and appears the same across all Manufacturing pages but no Acumatica pages even though we are using the same PXLineNbr attribute) the linecntr is the same as the last linenbr used. Where in Acumatica it is +1
                //  Not a major issue but what happens is the first line on rebuild is the same as the last line deleted.
                var matlBatch = materialEntry.batch.Current;
                matlBatch.LineCntr += 1;
                materialEntry.batch.Update(matlBatch);

                materialEntry.Persist();
            }

            return materialEntry;
        }

        protected virtual ProductionGLTranBuilder FindResetProdCostBatch(AMBatch doc)
        {
            return FindProdCostBatch(doc, true);
        }

        internal virtual ProductionGLTranBuilder FindProdCostBatch(AMBatch doc, bool resetBatch)
        {
            if (doc.DocType != AMDocType.Move && doc.DocType != AMDocType.Labor && doc.DocType != AMDocType.Disassembly)
            {
                return null;
            }

            // It is possible when re-run that we could have the AMBatch released or not and same for GL. 
            //  The checks below will cover the different conditions and load previous transactions as we need to process changes between last run only for new transactions
           
            var relatedCostBatchNumbers = new List<string>();
            var releasedTransactionsFound = false;
            AMBatch unreleasedAmBatch = null;
            foreach (AMBatch amBatch in SelectFrom<AMBatch>.
                Where<AMBatch.origDocType.IsEqual<@P.AsString>.
                    And<AMBatch.origBatNbr.IsEqual<@P.AsString>>.
                    And<AMBatch.docType.IsEqual<AMDocType.prodCost>>.
                    And<AMBatch.batNbr.IsNotEqual<@P.AsString>>>.
                View.Select(this, doc.DocType, doc.BatNbr, doc.BatNbr))
            {
                relatedCostBatchNumbers.Add(amBatch?.BatNbr);

                if(amBatch?.Released == false)
                {
                    unreleasedAmBatch = amBatch;
                    continue;
                }
                releasedTransactionsFound = true;
            }

            if(relatedCostBatchNumbers.Count == 0)
            {
                return null;
            }
            var relatedCostTrans = new List<AMMTran>();
            foreach(var costTran in SelectFrom<AMMTran>.
                Where<AMMTran.docType.IsEqual<AMDocType.prodCost>.
                    And<AMMTran.batNbr.IsIn<@P.AsString>>>.
                View.Select(this, new[] { relatedCostBatchNumbers.ToArray() } ))
            {
                relatedCostTrans.Add(costTran);
            }

            var hasRelatedCostTrans = relatedCostTrans.Count > 0;
            if(!hasRelatedCostTrans && unreleasedAmBatch == null)
            {
                return null;
            }

            if(unreleasedAmBatch == null)
            {
                return new ProductionGLTranBuilder() { PreviousCostTrans = relatedCostTrans };
            }

            //ready to clear out the lines
            var productionCostEntry = CreateInstance<ProductionCostEntry>();
            productionCostEntry.Clear();
            productionCostEntry.batch.Current = unreleasedAmBatch;
            productionCostEntry.batch.Cache.IsDirty = false;
            if (productionCostEntry.ampsetup.Current != null)
            {
                productionCostEntry.ampsetup.Current.RequireControlTotal = false;
            }

            Batch glBatch = null;
            foreach (Batch batch in SelectFrom<Batch>.
                Where<Batch.module.IsEqual<BatchModule.moduleGL>.
                    And<BatchExt.aMDocType.IsEqual<@P.AsString>>.
                    And<BatchExt.aMBatNbr.IsEqual<@P.AsString>>>.
                View.Select(this, unreleasedAmBatch.DocType, unreleasedAmBatch.BatNbr))
            {
                if(batch?.OrigModule == null || batch.OrigModule != Common.ModuleAM)
                {
                    // In case transaction numbers similar to other modules
                    continue;
                }

                if (batch?.Released == true)
                {
                    releasedTransactionsFound = true;
                    continue;
                }

                glBatch = batch;
                break;
            }

            JournalEntry journalEntry;
            if (resetBatch && glBatch?.BatchNbr != null)
            {
                journalEntry = FindResetRelatedGLBatch(glBatch.BatchNbr);
            }
            else
            {
                journalEntry = CreateInstance<JournalEntry>();
                journalEntry.Clear();
                if(glBatch?.BatchNbr != null)
                {
                    journalEntry.BatchModule.Current = journalEntry.BatchModule.Search<Batch.batchNbr>(glBatch.BatchNbr, BatchModule.GL);
					GeneralLedgerBatches.RemoveAll(_=>_.NoteID == glBatch.NoteID);
					GeneralLedgerBatches.Add(journalEntry.BatchModule.Current);
				}
            }

            if (!releasedTransactionsFound && resetBatch)
            {
                relatedCostTrans.Clear();

                foreach (AMMTran row in productionCostEntry.transactions.Select())
                {
                    if (row.Released.GetValueOrDefault())
                    {
                        continue;
                    }

                    productionCostEntry.transactions.Delete(row);
                }

                if (productionCostEntry.transactions.Cache.Deleted.Any_())
                {
                    productionCostEntry.Persist();
                }
            }

            return new ProductionGLTranBuilder(journalEntry, productionCostEntry) { PreviousCostTrans = relatedCostTrans };
        }

        protected virtual JournalEntry FindResetRelatedWipAdjustmentBatch(AMBatch doc)
        {
            if (doc == null)
            {
                throw new PXArgumentException(nameof(doc));
            }

            Batch relatedBatch = PXSelect<Batch,
                Where<Batch.released, Equal<False>,
                    And<Batch.module, Equal<BatchModule.moduleGL>,
                        And<BatchExt.aMDocType, Equal<Required<BatchExt.aMDocType>>,
                            And<BatchExt.aMBatNbr, Equal<Required<BatchExt.aMBatNbr>>>>>>
                >.Select(this, doc.DocType, doc.BatNbr);

            return FindResetRelatedGLBatch(relatedBatch?.BatchNbr);
        }

        protected virtual JournalEntry FindResetRelatedGLBatch(string glBatchNbr)
        {
            var journalEntry = CreateInstance<JournalEntry>();
            journalEntry.Clear();
            JournalEntryAMExtension.SetIsInternalCall(journalEntry, true);

            if (string.IsNullOrWhiteSpace(glBatchNbr))
            {
                return journalEntry;
            }

            journalEntry.BatchModule.Current = journalEntry.BatchModule.Search<Batch.batchNbr>(glBatchNbr, BatchModule.GL);

            if (journalEntry.BatchModule.Current == null)
            {
                return journalEntry;
            }

            foreach (GLTran row in journalEntry.GLTranModuleBatNbr.Select())
            {
                if (row.Released.GetValueOrDefault())
                {
                    continue;
                }

                journalEntry.GLTranModuleBatNbr.Delete(row);
            }

            if (journalEntry.GLTranModuleBatNbr.Cache.Deleted.Any_())
            {
                if (journalEntry.glsetup.Current != null)
                {
                    journalEntry.glsetup.Current.RequireControlTotal = false;
                }

                journalEntry.Persist();
            }

            return journalEntry;
        }

        protected virtual WIPAdjustmentEntry FindResetWIPAdjustmentBatch(AMBatch doc)
        {
            if (doc.DocType != AMDocType.Move && doc.DocType != AMDocType.Labor)
            {
                return null;
            }

            //Check for current referenced WIP adjustment transaction and reuse it
            AMBatch unreleasedAmBatch = PXSelect<
                AMBatch,
                Where<AMBatch.origBatNbr, Equal<Required<AMBatch.origBatNbr>>,
                    And<AMBatch.origDocType, Equal<Required<AMBatch.origDocType>>,
                    And<AMBatch.released, Equal<False>,
                    And<AMBatch.batNbr, NotEqual<Required<AMBatch.batNbr>>,
                    And<AMBatch.docType, Equal<Required<AMBatch.docType>>>>
                        >>>>
                .Select(this, doc.BatNbr, doc.DocType, doc.BatNbr, AMDocType.WipAdjust);

            if (unreleasedAmBatch == null)
            {
                return null;
            }

            //ready to clear out the lines
            var wipAdjustmentEntry = CreateInstance<WIPAdjustmentEntry>();
            wipAdjustmentEntry.Clear();
            wipAdjustmentEntry.batch.Current = unreleasedAmBatch;
            if (wipAdjustmentEntry.ampsetup.Current != null)
            {
                wipAdjustmentEntry.ampsetup.Current.RequireControlTotal = false;
            }
            wipAdjustmentEntry.batch.Cache.IsDirty = false;

            foreach (AMMTran row in wipAdjustmentEntry.transactions.Select())
            {
                if (row.Released.GetValueOrDefault())
                {
                    continue;
                }

                wipAdjustmentEntry.transactions.Delete(row);
            }

            if (wipAdjustmentEntry.transactions.Cache.Deleted.Any_())
            {
                wipAdjustmentEntry.Persist();
            }

            return wipAdjustmentEntry;
        }

        protected virtual void CreateLaborGL(ProductionTransactionHelper productionTranHelper, AMBatch doc, AMMTran tran, AMProdItem amproditem)
        {
            tran.WIPAcctID = amproditem.WIPAcctID;
            tran.WIPSubID = amproditem.WIPSubID;

            AMLaborCode amLaborCode = PXSelect<AMLaborCode, Where<AMLaborCode.laborCodeID, Equal<Required<AMLaborCode.laborCodeID>>
                    >>.Select(this, tran.LaborCodeID);

            if (amLaborCode == null)
            {
                throw new PXException(Messages.GetLocal(Messages.MissingLaborCode), tran.LaborCodeID ?? string.Empty);
            }

            tran.AcctID = amLaborCode.LaborAccountID;
            tran.SubID = amLaborCode.LaborSubID;

            var emp = (PX.Objects.EP.EPEmployee)PXSelectorAttribute.Select<AMMTran.employeeID>(TranRecs.Cache, tran);
            var descCodes = string.IsNullOrWhiteSpace(emp?.AcctCD)
                ? tran.LaborCodeID
                : $"{tran.LaborCodeID.TrimIfNotNullEmpty()}, {emp.AcctCD.TrimIfNotNullEmpty()}";

            productionTranHelper.ProductionGlTranBuilder.CreateAmGlLine(amproditem, tran, tran.ExtCost,
                amLaborCode.LaborAccountID, amLaborCode.LaborSubID, AMTranType.Labor, tran.OperationID,
                ProductionTransactionHelper.BuildTransactionDescription(AMTranType.Labor, descCodes), tran.LaborTime, emp?.AcctCD);

            TranRecs.Update(tran);
        }

        /// <summary>
        /// Process indirect labor lines only
        /// </summary>
        protected virtual void ProcessIndirectLaborLine(ProductionTransactionHelper productionTranHelper, AMBatch doc, AMMTran tran)
        {
            if (productionTranHelper == null
                || doc == null
                || tran == null
                || tran.Released.GetValueOrDefault())
            {
                return;
            }

            AMLaborCode amLaborCode = PXSelect
                <AMLaborCode, Where<AMLaborCode.laborCodeID, Equal<Required<AMLaborCode.laborCodeID>>
                    >>.Select(this, tran.LaborCodeID);

            if (amLaborCode == null)
            {
                throw new PXException(Messages.GetLocal(Messages.MissingLaborCode), tran.LaborCodeID.TrimIfNotNullEmpty());
            }

            if (amLaborCode.OverheadAccountID == null || amLaborCode.OverheadSubID == null)
            {
                throw new PXException(Messages.GetLocal(Messages.OverheadAccountRequiredIndirectLabor), tran.LaborCodeID.TrimIfNotNullEmpty());
            }

            tran.AcctID = amLaborCode.LaborAccountID;
            tran.SubID = amLaborCode.LaborSubID;
            tran.WIPAcctID = amLaborCode.OverheadAccountID;
            tran.WIPSubID = amLaborCode.OverheadSubID;

            if (tran.ExtCost.GetValueOrDefault() != 0)
            {
                productionTranHelper.ProductionGlTranBuilder.CreateAmGlLine(amLaborCode, tran, tran.ExtCost,
                    AMTranType.IndirectLabor,
                    ProductionTransactionHelper.BuildTransactionDescription(AMTranType.IndirectLabor,
                        amLaborCode.LaborCodeID));
            }

            TranRecs.Update(tran);
        }

        /// <summary>
        /// Process a standard move/labor transactions (no indirect labor)
        /// </summary>
        protected virtual void ProcessMoveTranLine(ProductionTransactionHelper productionTranHelper,
            InventoryTranBuilder receiptBuilder, InventoryTranBuilder adjustmentBuilder,
            AMBatch doc, AMMTran tran, List<AMMTranSplit> splits, InventoryItem inventoryItem, AMProdItem amproditem,
            AMProdOper amprodoper, MoveOperationQtyTotals moveOperationQtyTotals)
        {
            if (amproditem == null)
            {
                if (string.IsNullOrWhiteSpace(tran.ProdOrdID))
                {
                    throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.prodOrdID>(this.TranRecs.Cache));
                }

                if (string.IsNullOrWhiteSpace(tran.OrderType))
                {
                    throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.orderType>(this.TranRecs.Cache));
                }

                throw new PXException(Messages.RecordMissing, Common.Cache.GetCacheName(typeof(AMProdItem)));
            }
#if DEBUG
            AMDebug.TraceWriteMethodName($"Tran: {tran.DocType} - {tran.BatNbr.TrimIfNotNullEmpty()} - {tran.LineNbr} Order: {tran.OrderType.TrimIfNotNullEmpty()} - {tran.ProdOrdID.TrimIfNotNullEmpty()} - {tran.OperationID}");
#endif

            if (ReferencedProductionOrders.Add(tran))
            {
				object prodOrderID = tran?.ProdOrdID;
				TranRecs.View.Cache.RaiseFieldVerifying<AMMTran.prodOrdID>(tran, ref prodOrderID);
			}

            if (amprodoper?.ProdOrdID == null)
            {
                if (tran.OperationID == null)
                {
                    throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.operationID>(TranRecs.Cache));
                }

                PXTrace.WriteError(Messages.GetLocal(Messages.InvalidProductionOperationNbr,
                    tran.OrderType.TrimIfNotNullEmpty(), tran.ProdOrdID.TrimIfNotNullEmpty(), tran.OperationID));

                throw new PXException(Messages.InvalidOperationOnTransaction, AMDocType.GetDocTypeDesc(tran.DocType), tran.BatNbr, tran.LineNbr);
            }

			//if batch was not marked as is last operation then lets update accordingly...
			tran.LastOper = tran.OperationID == amproditem.LastOperationID;

            if (!tran.Released.GetValueOrDefault())
            {
                amprodoper.ActualLabor += tran.ExtCost.GetValueOrDefault();
                amprodoper.ActualLaborTime += tran.LaborTime.GetValueOrDefault();
                amprodoper.QtyScrapped += tran.QtyScrapped.GetValueOrDefault();
                amprodoper.BaseQtyScrapped += tran.BaseQtyScrapped.GetValueOrDefault();

                ProductionOpers.Update(amprodoper);

                if (tran.QtyScrapped.GetValueOrDefault() != 0)
                {
                    // reporting scrap and not including scrap will increase the overall operation total which drives total material...
                    ProductionTransactionHelper.UpdateOperationQty(this, amproditem, moveOperationQtyTotals.OperationsList.LocateElseCopy(this), productionTranHelper.IncludeScrap);
                }

                //Create a labor transaction GL Batch - allow when zero cost if labor time to correctly record labor total on production order
                if (tran.ExtCost.GetValueOrDefault() != 0 || tran.LaborTime.GetValueOrDefault() != 0)
                {
                    CreateLaborGL(productionTranHelper, doc, tran, amproditem);
                }

                productionTranHelper.AmReleaseGraph.ProductionItems.Current = amproditem;

                // BuildTrelatedTransactions to process...
                // + backflush labor
                // + machine
                // + backflush material
                // + tool
                // + overhead
                productionTranHelper.BuildRelatedTransactions(tran, moveOperationQtyTotals);

                amproditem = PXSelect<AMProdItem,
                    Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                        And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>
                        >.Select(this, tran.OrderType, tran.ProdOrdID);
                ProductionItems.Current = amproditem;

                tran.TranAmt = 0;
                tran.UnitCost = 0;
                if (tran.LastOper.GetValueOrDefault() && !tran.IsScrap.GetValueOrDefault())
                {
                    tran.FinPeriodID = doc.FinPeriodID;

                    if (tran.Qty.GetValueOrDefault() > 0)
                    {
                        var operationResults = CalculateCompletedUnitCostByOperation(tran, amproditem, moveOperationQtyTotals);

                        // The standard cost for the item will return within the operation results unit amount. 
                        //  There is no need to update the operations as standard cost items skip the update to totals by operation.
                        if (amproditem.CostMethod != CostMethod.Standard)
                        {
                            CreateWipCompleteCostEntries(tran, productionTranHelper, operationResults, moveOperationQtyTotals);
                        }
                        //a receipt can't go into inventory at a negative unit cost
                        tran.UnitCost = operationResults.UnitAmount.NotLessZero();
                        receiptBuilder.CreateTranLine(tran, inventoryItem, splits);
                    }

					var returnOperTotalAmount = (decimal?)null;
                    if (tran.Qty.GetValueOrDefault() < 0)
                    {
                        var previouslyReturnedQty = GetCachedProcessedOrderBaseQty(tran);
                        // returnOperationResults is in base unit values
                        var returnOperationResults = new ProductionCostCalculator(this, this.ProductionSetup.Current).CalculateReturnUnitCost(tran, amproditem, moveOperationQtyTotals, previouslyReturnedQty);
                        if (amproditem.CostMethod != CostMethod.Standard)
                        {
                            CreateWipCompleteCostEntries(tran, productionTranHelper, returnOperationResults);
                        }

						tran.UnitCost = Math.Abs(returnOperationResults.UnitAmount);
						returnOperTotalAmount = Math.Abs(returnOperationResults.TotalAmount) * -1;
                        adjustmentBuilder.CreateTranLine(tran, inventoryItem, splits);
                    }

                    tran.UnitCost = UomHelper.PriceCostRound(tran.UnitCost.GetValueOrDefault());
                    tran = TranRecs.Update(tran);
					var tranAmt = returnOperTotalAmount != null
						? returnOperTotalAmount
						: (tran.Qty.GetValueOrDefault() * tran.UnitCost.GetValueOrDefault());
					if (tran.TranAmt != tranAmt)
					{
                    tran.TranAmt = PXCurrencyAttribute.BaseRound(this, tranAmt);
					}

                amproditem.QtyScrapped += tran.QtyScrapped.GetValueOrDefault();
                if (tran.LastOper.GetValueOrDefault() && (tran.Qty.GetValueOrDefault() != 0 || tran.QtyScrapped.GetValueOrDefault() != 0))
                {
                    //Must occur after the cost calculations?
                    amproditem.QtyComplete += tran.Qty.GetValueOrDefault();
                    //amproditem = ProductionItems.Update(amproditem);
                }

                ProductionItems.Update(amproditem);
                tran.Selected = true; //processing indicated it was been processed in this stage of release
                TranRecs.Update(tran);
            }

            ReferencedInBatches.Add(string.Join("~", tran.INDocType, tran.INBatNbr));
            ReferencedGlBatches.Add(tran.GLBatNbr);
        }
		}

        // This would only run for last operation when the WIP numbers are calculated by operation
        private ProductionOperationCostResults CalculateCompletedUnitCostByOperation(AMMTran tran, AMProdItem prodItem, MoveOperationQtyTotals moveOperationQtyTotals)
        {
            if (string.IsNullOrWhiteSpace(tran.ProdOrdID))
            {
                return null;
            }

            var invtMult = tran.Qty.GetValueOrDefault() >= 0 ? "A" : "B";

            // We are using the first calculated result to avoid rounding and calculation problems when the same operation for the same order exists in the same transaction.
            var operKey = string.Join(":", tran.OrderType, tran.ProdOrdID, tran.OperationID, invtMult);
            if (FirstTranOperCostResults.TryGetValue(operKey, out var firstResults))
            {
                return firstResults;
            }

            var newResults = new ProductionCostCalculator(this, this.ProductionSetup.Current).CalculateCompletedUnitCostByOperation(tran, prodItem, moveOperationQtyTotals);

            FirstTranOperCostResults[operKey] = newResults;

            return newResults;
        }

        protected virtual void CreateWipCompleteCostEntries(AMMTran tran, ProductionTransactionHelper productionTranHelper, ProductionOperationCostResults operationResults, MoveOperationQtyTotals moveOperationQtyTotals)
        {
			bool isNegativeTotal = operationResults.TotalAmount <= 0;
			if (isNegativeTotal)
			{
				return;
			}
            foreach (var operationResult in operationResults.OperationResults)
            {
                //This method is only called from an inventory receipt so the cost should not be a negative number
                CreateWipCompleteCostEntry(tran, productionTranHelper, operationResult, tran.BaseQty.GetValueOrDefault());
            }
        }

        protected virtual void CreateWipCompleteCostEntries(AMMTran tran, ProductionTransactionHelper productionTranHelper, ProductionOperationCostResults operationResults)
        {
            foreach (var operationResult in operationResults.OperationResults)
            {
                CreateWipCompleteCostEntry(tran, productionTranHelper, operationResult, operationResult.ProcessQty);
            }
        }

        protected virtual void CreateWipCompleteCostEntry(AMMTran tran, ProductionTransactionHelper productionTranHelper, ProductionOperationCostResults.OperationCostResult operationResult, decimal moveBaseQty)
        {
            if (operationResult.TotalAmount == 0 || moveBaseQty == 0)
            {
                return;
            }

            //Convert to Tran UOM...
            var moveQty = moveBaseQty;
            if (moveBaseQty != 0 && ProductionTransactionHelper.UsingAlternativeUom(tran) &&
                UomHelper.TryConvertFromBaseQty<AMMTran.inventoryID>(TranRecs.Cache, tran, tran.UOM, moveBaseQty, out var convertedQty))
            {
                moveQty = convertedQty ?? moveBaseQty;
                if (moveQty == 0)
                {
                    return;
                }
            }

            var updatingOper = ProductionOpers.Cache.LocateElseCopy(operationResult.ProdOper);
            var tranAmount = PXCurrencyAttribute.BaseRound(this, operationResult.UnitAmount * moveQty);
#if DEBUG
            AMDebug.TraceWriteMethodName($"[{tran.DebuggerDisplay}]-[UnitAmount={operationResult.UnitAmount}; moveQty={moveQty}; tranAmount={tranAmount}]");
#endif
            updatingOper.WIPComp = updatingOper.WIPComp.GetValueOrDefault() + tranAmount;
            updatingOper = ProductionOpers.Update(updatingOper);

            if (tranAmount == 0)
            {
                return;
            }

            productionTranHelper.CreateWipCompleteCostEntry(tran, updatingOper, tranAmount, moveQty);
        }

        /// <summary>
        /// For the same transaction what has already been processed for the same order.
        /// </summary>
        protected virtual decimal GetCachedProcessedOrderBaseQty(AMMTran tran)
        {
            var retBaseQty = 0m;
            var isReturnQtyCheck = tran.BaseQty.GetValueOrDefault() < 0;
            foreach (AMMTran updatedTran in this.TranRecs.Cache.Updated)
            {
                //exclude self and unselected (which is the indicator for processed). Also we are matching by same order/oper
                if (updatedTran.DocType != tran.DocType || updatedTran.BatNbr != tran.BatNbr ||
                    !updatedTran.Selected.GetValueOrDefault() || updatedTran.LineNbr == tran.LineNbr ||
                    updatedTran.OrderType == null || updatedTran.OrderType != tran.OrderType ||
                    updatedTran.ProdOrdID != tran.ProdOrdID || updatedTran.OperationID != tran.OperationID)
                {
                    continue;
                }

                if (isReturnQtyCheck && updatedTran.BaseQty.GetValueOrDefault() >= 0 ||
                    !isReturnQtyCheck && updatedTran.BaseQty.GetValueOrDefault() < 0)
                {
                    continue;
                }

                retBaseQty += updatedTran.BaseQty.GetValueOrDefault();
            }

            return retBaseQty;
        }

        /// <summary>
        /// Remove the release process only WIP Complete tran type records. They should only exist when the transaction completes/release
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="persistDelete"></param>
        protected virtual void DeleteOperationWipCompleteRecords(AMBatch doc, bool persistDelete)
        {
            if (doc.Released.GetValueOrDefault()
                || (doc.DocType != AMDocType.Labor
                && doc.DocType != AMDocType.Move))
            {
                return;
            }

            foreach (AMMTran row in PXSelect<AMMTran,
                Where<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>
                    >.Select(this, doc.DocType, doc.BatNbr))
            {
                if (row.TranType != AMTranType.OperWIPComplete)
                {
                    continue;
                }

                var currentStatus = TranRecs.Cache.GetStatus(row);

                if (currentStatus == PXEntryStatus.Inserted)
                {
                    TranRecs.Cache.Remove(row);
                }

                var deletedrow = TranRecs.Delete(row);
                if (persistDelete)
                {
                    TranRecs.Cache.PersistDeleted(deletedrow);
                }
            }
        }

        protected virtual decimal FindOperationWipCompleteTotalQty(string orderType, string prodOrdId, int? operationID, List<AMMTran> WipCompleteRecords)
        {
            var totalBaseQty = 0m;
            if(WipCompleteRecords == null)
            {
                return totalBaseQty;
            }
            foreach (var wipComplete in WipCompleteRecords)
            {
                if (wipComplete.OrderType.Equals(orderType)
                    && wipComplete.ProdOrdID.Equals(prodOrdId)
                    && wipComplete.OperationID == operationID)
                {
                    totalBaseQty += wipComplete.BaseQty.GetValueOrDefault();
                }
            }
            return totalBaseQty;
        }

        protected virtual void RunMoveTransactionChecks(AMBatch doc, MoveOperationQtyBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            CheckTranAttributes(doc);
#if DEBUG
            var lastElapsed = sw.Elapsed;
            AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(lastElapsed, "CheckTranAttributes"));
#endif

			CheckScrapReturn(builder);
#if DEBUG
			lastElapsed = sw.Elapsed;
			AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(lastElapsed, "CheckScrapReturn"));
#endif
            CheckMoveOnCompletedOrder(builder.AllTranRows);

#if DEBUG
            lastElapsed = sw.Elapsed;
            AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(lastElapsed, "CheckMoveOnCompletedOrder"));
#endif
			CheckControlPointQty(builder);
#if DEBUG
            lastElapsed = sw.Elapsed;
            AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(lastElapsed, "CheckControlPointQty"));
#endif
            CheckNegMoveQty(doc, builder.AllTranRowsAMMTran);
#if DEBUG
            var currentElapsed = sw.Elapsed;
            AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(currentElapsed - lastElapsed, "CheckNegMoveQty"));
            lastElapsed = currentElapsed;
#endif
            CheckOverCompletedOrders(builder);
#if DEBUG
            currentElapsed = sw.Elapsed;
            AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(currentElapsed - lastElapsed, "CheckOverCompletedOrders"));
            lastElapsed = currentElapsed;
#endif
            CheckUnderIssuedMaterial(builder);

#if DEBUG
			currentElapsed = sw.Elapsed;
			AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(currentElapsed - lastElapsed, "CheckInterBranchTransaction"));
			lastElapsed = currentElapsed;
#endif
			CheckInterBranchTransaction(doc, builder);
#if DEBUG
            sw.Stop();
            currentElapsed = sw.Elapsed;
            lastElapsed = currentElapsed;
            AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(currentElapsed - lastElapsed, "CheckUnderIssuedMaterial"));
            AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, "Total"));
#endif
            try
			{
                CheckDuplicateSerialNumber(doc);
			}
			catch (AMTransactionFailedCheckException)
			{
				if(!HasReferencedBatchesReleased(ReferencedBatchesReleased(doc)))
				{
					throw;
				}
			}
#if DEBUG
            sw.Stop();
            currentElapsed = sw.Elapsed;
            lastElapsed = currentElapsed;
            AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(currentElapsed - lastElapsed, "CheckDuplicateSerialNumber"));
            AMDebug.TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, "Total"));
#endif
        }

        /// <summary>
		/// Validate over return scraps for a particular operation
		/// </summary>
		/// <param name="builder"></param>
		/// <exception cref="PXException"></exception>
		private static void CheckScrapReturn(MoveOperationQtyBuilder builder)
		{
			var groupedTransactions = builder.AllTranRowsAMMTran.GroupBy(g => new { g.OrderType, g.ProdOrdID, g.OperationID });
			foreach (var item in groupedTransactions)
			{
				var operation = builder.AllTranRows.Select(s => s.GetItem<AMProdOper>())
									   .First(a => a.OrderType == item.Key.OrderType
						&& a.ProdOrdID == item.Key.ProdOrdID
						&& a.OperationID == item.Key.OperationID);
				decimal? scrapSum = item.Sum(s => s.QtyScrapped);
				if (scrapSum < 0 && Math.Abs(scrapSum.Value) > operation.QtyScrapped)
				{
					throw new PXException(Messages.NegativeScrapReversalValidationError,
						UomHelper.FormatQty(Math.Abs(scrapSum.Value)),
						operation.OrderType,
						operation.ProdOrdID,
						operation.OperationCD,
						UomHelper.FormatQty(operation.QtyScrapped));
				}
			}
		}


		/// <summary>
        /// Process Manufacturing move or labor transaction
        /// </summary>
        /// <param name="doc"></param>
        protected virtual void ProcessMoveTran(AMBatch doc)
        {
            var receiptBuilder = CreateInventoryTranBuilder(doc, InventoryTranBuilder.TransactionType.Receipt);
            var adjustmentBuilder = CreateInventoryTranBuilder(doc, InventoryTranBuilder.TransactionType.Adjustment);

            var productionTranHelper = new ProductionTransactionHelper(this) { IncludeScrap = IncludeScrap };

            var backflushMatlGraph = FindResetBackflushMaterialBatch(doc, out productionTranHelper.BackflushMaterialReleased, out productionTranHelper.BackflushMaterialINRegisterReleased);
            if (backflushMatlGraph != null)
            {
                productionTranHelper.ReleaseMatlGraph = backflushMatlGraph;
            }
            var prodGLTranBuilder = FindResetProdCostBatch(doc);
            if (prodGLTranBuilder != null)
            {
                productionTranHelper.ProductionGlTranBuilder = prodGLTranBuilder;
            }
            var wipAdjustGraph = FindResetWIPAdjustmentBatch(doc);
            if (wipAdjustGraph != null)
            {
                productionTranHelper.WIPAdjustmentGraph = wipAdjustGraph;
            }

            var refBatchReleasedDictionary = ReferencedBatchesReleased(doc);

            //Left Join on InventoryItem, ProdItem, etc due to Indirect Labor (No item/order provided)

            PXResultset<AMMTran> results = PXSelectJoin<AMMTran,
                LeftJoin<InventoryItem, On<AMMTran.inventoryID, Equal<InventoryItem.inventoryID>>,
                LeftJoin<AMOrderType, On<AMMTran.orderType, Equal<AMOrderType.orderType>>,
                    LeftJoin<AMProdItem, On<AMMTran.orderType, Equal<AMProdItem.orderType>,
                            And<AMMTran.prodOrdID, Equal<AMProdItem.prodOrdID>>>,
                        LeftJoin<AMProdOper, On<AMMTran.orderType, Equal<AMProdOper.orderType>,
                            And<AMMTran.prodOrdID, Equal<AMProdOper.prodOrdID>,
                                And<AMMTran.operationID, Equal<AMProdOper.operationID>>>>>>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>
            >.Select(this, doc.DocType, doc.BatNbr);

            var moveOperationQtyBuilder = MoveOperationQtyBuilder.Construct(this, results);

            RunMoveTransactionChecks(doc, moveOperationQtyBuilder);
            var scrapTransactions = new List<Tuple<AMMTran, MoveOperationQtyTotals>>();

            var allTranSplits = PXSelect<
                    AMMTranSplit,
                    Where<AMMTranSplit.docType, Equal<Required<AMMTranSplit.docType>>,
                        And<AMMTranSplit.batNbr, Equal<Required<AMMTranSplit.batNbr>>>>>
                .Select(this, doc.DocType, doc.BatNbr)
                .ToFirstTableList();

            foreach (PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper> result in results)
            {
                var tran = TranRecs.Cache.LocateElse((AMMTran)result);

				if (tran.Released == true) continue;

                //make sure display fields are in sync
                tran.FinPeriodID = doc.FinPeriodID;
                tran.TranDate = doc.TranDate;

                if (refBatchReleasedDictionary.ContainsKey(tran.LineNbr.GetValueOrDefault())
                    && refBatchReleasedDictionary[tran.LineNbr.GetValueOrDefault()])
                {
                    continue;
                }

                if (tran.LaborType != null && tran.LaborType == AMLaborType.Indirect)
                {
                    //call me for indirect
                    ProcessIndirectLaborLine(productionTranHelper, doc, tran);
                    continue;
                }

                var moveOperTotals = moveOperationQtyBuilder?.GetMoveOperationTotals(tran);
                if (moveOperTotals == null)
                {
                    continue;
                }

                //call me for everything else
                ProcessMoveTranLine(productionTranHelper, receiptBuilder, adjustmentBuilder, doc, tran,
                    allTranSplits?.Where(x => x.LineNbr == tran.LineNbr).ToList(),
                    (InventoryItem) result,
                    ProductionItems.Cache.LocateElse((AMProdItem)result),
                    ProductionOpers.Cache.LocateElse((AMProdOper)result),
                    moveOperTotals);

                if (RequiresScrapTransaction(tran))
                {
                    scrapTransactions.Add(new Tuple<AMMTran, MoveOperationQtyTotals>(tran, moveOperTotals));
                }
            }

            if (receiptBuilder?.RefDocNoteRecordRequired == true ||
                adjustmentBuilder?.RefDocNoteRecordRequired == true)
            {
                EnsureDocNote(BatchRecs.Current);
            }

            using (var ts = new PXTransactionScope())
            {
                if (receiptBuilder.HasInTransactions)
                {
                    InventoryBatches.Add(receiptBuilder.CurrentInDocument);
                }
                receiptBuilder.Save();
                if (adjustmentBuilder.HasInTransactions)
                {
                    InventoryBatches.Add(adjustmentBuilder.CurrentInDocument);
                }
                adjustmentBuilder.Save();

                var AMRelMatlgraph = productionTranHelper.ReleaseMatlGraph;
                if (AMRelMatlgraph.transactions.Select().Count > 0)
                {
                    AMRelMatlgraph.batch.Current.Hold = false;
                    AMRelMatlgraph.batch.Current.OrigBatNbr = doc.BatNbr;
                    AMRelMatlgraph.batch.Current.OrigDocType = doc.DocType;
                    AMRelMatlgraph.batch.Current.FinPeriodID = doc.FinPeriodID;
                    AMRelMatlgraph.batch.Current.TranDate = doc.TranDate;

                    AMRelMatlgraph.Persist();
                }

                if (productionTranHelper.IsProductionGLTranBuilderInitialized)
                {
                    var productionGlTranBuilder = productionTranHelper.ProductionGlTranBuilder;
                    if (productionGlTranBuilder.HasGlTransactions)
                    {
                        GeneralLedgerBatches.Add(productionGlTranBuilder.CurrentGlDocument);
                    }
                    else if (!productionGlTranBuilder.HasAmTransactions)
                    {
                        //Possible to have non GL cost entries... only delete here if either do not exist
                        productionGlTranBuilder.DeleteBatch();
                    }
                    productionGlTranBuilder.Save();
                }

                CreateScrapWipAdjustment(productionTranHelper, scrapTransactions);

                if (IsDirty)
                {
                    Actions.PressSave();
                }
                ts.Complete();
            }
        }

        private void CreateScrapWipAdjustment(ProductionTransactionHelper productionTranHelper, List<Tuple<AMMTran, MoveOperationQtyTotals>> scrapTransactions)
        {
            if(productionTranHelper == null)
            {
                return;
            }

            productionTranHelper.CreateScrapWipAdjustements(scrapTransactions);

            if (productionTranHelper.HasWIPAdjustmentChanges)
            {
                productionTranHelper.WIPAdjustmentGraph.Persist();
            }
        }

        /// <summary>
        /// Check all transaction attributes on a move/labor document
        /// </summary>
        /// <param name="doc">Move/Labor document to validate the attributes</param>
        protected virtual void CheckTranAttributes(AMBatch doc)
        {
            if (doc == null || doc.DocType == null)
            {
                throw new PXArgumentException("doc");
            }

            if (doc.DocType != AMDocType.Labor
                && doc.DocType != AMDocType.Move)
            {
                return;
            }

            var sb = new System.Text.StringBuilder();

            foreach (PXResult<AMMTranAttribute, AMMTran> result in PXSelectJoin<AMMTranAttribute,
                InnerJoin<AMMTran, On<AMMTranAttribute.docType, Equal<AMMTran.docType>,
                    And<AMMTranAttribute.batNbr, Equal<AMMTran.batNbr>,
                    And<AMMTranAttribute.tranLineNbr, Equal<AMMTran.lineNbr>>>>>,
                Where<AMMTranAttribute.docType, Equal<Required<AMMTranAttribute.docType>>,
                    And<AMMTranAttribute.batNbr, Equal<Required<AMMTranAttribute.batNbr>>>>>.Select(this, doc.DocType, doc.BatNbr))
            {
                var attribute = (AMMTranAttribute)result;
                var tran = (AMMTran)result;

                if (attribute == null || string.IsNullOrWhiteSpace(attribute.BatNbr)
                    || tran == null || string.IsNullOrWhiteSpace(tran.BatNbr)
					|| tran.Released == true)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(tran.ProdOrdID)
                    && (tran.Qty.GetValueOrDefault() != 0 || tran.QtyScrapped.GetValueOrDefault() != 0)
                    && attribute.TransactionRequired.GetValueOrDefault()
                    && string.IsNullOrWhiteSpace(attribute.Value))
                {
                    sb.AppendLine(Messages.GetLocal(Messages.TranLineRequiresAttribute,
                        AMDocType.GetDocTypeDesc(attribute.DocType),
                        attribute.BatNbr.TrimIfNotNullEmpty(),
                        attribute.TranLineNbr,
                        attribute.Label.TrimIfNotNullEmpty()));
                }
            }

            if (!string.IsNullOrWhiteSpace(sb.ToString()))
            {
                throw new PXException(sb.ToString());
            }
        }

        /// <summary>
        /// Check all transactions for under issued material
        /// </summary>
        /// <param name="builder">Builder containing pre-processed operation quantity data</param>
        protected virtual void CheckUnderIssuedMaterial(MoveOperationQtyBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            foreach (var result in builder.AllTranRows)
            {
                var ammTran = (AMMTran)result;
                var orderType = (AMOrderType)result;
                var prodItem = (AMProdItem)result;

                if (ammTran == null
                    || string.IsNullOrWhiteSpace(ammTran.ProdOrdID)
                    || string.IsNullOrWhiteSpace(ammTran.DocType)
                    || orderType == null || orderType.OrderType == null
                    || prodItem == null || prodItem.ProdOrdID == null
					|| ammTran.Released == true)
                {
                    continue;
                }

                if (ammTran.DocType != AMDocType.Labor
                    && ammTran.DocType != AMDocType.Move)
                {
                    break;
                }

                AMTransactionFailedCheckException tranException = null;
                if (ProductionTransactionHelper.CheckUnderIssuedMaterial(this, ammTran, builder.GetMoveOperationTotals(ammTran), orderType.UnderIssueMaterial,
                    true, out tranException) && tranException != null && !tranException.IsWarning)
                {
                    throw tranException;
                }
            }
        }

		/// <summary>
		/// Check transaction for over issued material
		/// </summary>
		/// <param name="ammTran">Transaction to check over issued material</param>
		/// <param name="allTrans">List of transactions to calculate total quantity of same material for over issued materials</param>
		protected virtual void CheckOverIssuedMaterial(AMMTran ammTran, PXResultset<AMMTran> allTrans)
		{
			var totalQty = ammTran.Qty;
			var rows = allTrans.RowCast<AMMTran>().Where(r => r.OrderType == ammTran.OrderType
													&& r.ProdOrdID == ammTran.ProdOrdID
													&& r.OperationID == ammTran.OperationID
													&& r.MatlLineId == ammTran.MatlLineId
													&& r.LineNbr != ammTran.LineNbr).ToList();
			foreach (var row in rows)
			{
				totalQty += row.Qty.GetValueOrDefault();
			}
			if (ProductionTransactionHelper.TryCheckOverIssueMaterialSetPropertyException(TranRecs.Cache, ammTran, totalQty, out var exception)
				&& exception.ErrorLevel == PXErrorLevel.Error)
			{
				throw exception;
			}
		}

		/// <summary>
		/// Validate operation control point quantity
		/// </summary>
		protected virtual void CheckControlPointQty(MoveOperationQtyBuilder builder)
		{
			var errorMessages = new System.Text.StringBuilder();
			var orderHash = new HashSet<string>();
			foreach (PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper> result in builder.AllTranRows)
			{
				var tran = result.GetItem<AMMTran>();
				var prodOper = result.GetItem<AMProdOper>();
				if (prodOper == null || !orderHash.Add(string.Join(":", prodOper.OrderType, prodOper.ProdOrdID, prodOper.OperationID)) || tran.Released == true)
				{
					continue;
				}

				var operTotals = builder.GetMoveOperationTotals(tran);

				if (!operTotals.HasControlPoints)
				{
					continue;
				}

				ProductionTransactionHelper.CheckControlPointQuantity(tran, operTotals, out var eMessage);

				if(!string.IsNullOrWhiteSpace(eMessage))
				{
					errorMessages.Append($"{eMessage} ");
				}
			}

			if(errorMessages.Length != 0)
			{
				throw new AMTransactionFailedCheckException(errorMessages.ToString());
			}
		}

        /// <summary>
        /// Check returning qty and make sure the numbers make sense  
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="results"></param>
        protected virtual void CheckNegMoveQty(AMBatch doc, List<AMMTran> results)
        {
            if (results == null || doc.DocType != AMDocType.Labor && doc.DocType != AMDocType.Move)
            {
                return;
            }

            foreach (var result in results)
            {
                if (result.Released == true || result.Qty.GetValueOrDefault() >= 0)
                {
                    continue;
                }
                if (ProductionTransactionHelper.TryCheckNegativeMoveQty(TranRecs.Cache, result, result.Qty.GetValueOrDefault(), results, out var rowException))
                {
                    throw rowException;
                }
            }
        }

        /// <summary>
        /// Check all transactions for attempted transactions move on a completed order
        /// </summary>
        /// <param name="results"></param>
        protected virtual void CheckMoveOnCompletedOrder(List<PXResult<AMMTran, InventoryItem, AMOrderType, AMProdItem, AMProdOper>> results)
        {
            if (results == null)
            {
                return;
            }

            foreach (var result in results)
            {
                var ammTran = (AMMTran)result;
                var orderType = (AMOrderType)result;

                if (string.IsNullOrWhiteSpace(ammTran?.ProdOrdID)
                    || string.IsNullOrWhiteSpace(ammTran.DocType)
                    || orderType?.MoveCompletedOrders == null
					|| ammTran.Released == true)
                {
                    continue;
                }

                if (ammTran.DocType != AMDocType.Labor
                    && ammTran.DocType != AMDocType.Move)
                {
                    break;
                }

                AMTransactionFailedCheckException tranException = null;
                if (ProductionTransactionHelper.CheckMoveOnCompletedOperation(
                    this.TranRecs.Cache, ammTran, result, result, orderType.MoveCompletedOrders, out tranException)
                    && tranException != null && !tranException.IsWarning)
                {
                    throw tranException;
                }
            }
        }

        /// <summary>
        /// Check all transactions for over completion
        /// </summary>
        /// <param name="builder">Builder containing pre-processed operation quantity data</param>
        protected virtual void CheckOverCompletedOrders(MoveOperationQtyBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            foreach (var result in builder.AllTranRows)
            {
                var ammTran = (AMMTran)result;
                var orderType = (AMOrderType)result;
                var prodItem = (AMProdItem)result;
                var prodOper = (AMProdOper)result;

                if (ammTran == null
                    || string.IsNullOrWhiteSpace(ammTran.ProdOrdID)
                    || string.IsNullOrWhiteSpace(ammTran.DocType) 
                    || orderType?.OrderType == null 
                    || prodItem?.ProdOrdID == null
					|| ammTran.Released == true)
                {
                    continue;
                }

                if (ammTran.DocType != AMDocType.Labor
                    && ammTran.DocType != AMDocType.Move)
                {
                    break;
                }

				if (ProductionTransactionHelper.CheckOverCompletedOperation(
					this, ammTran, prodOper, builder.GetMoveOperationTotals(ammTran), orderType.ExceedQtyOperations,
					ProductionSetup.Current.InclScrap.GetValueOrDefault(), out var operationTranException)
					&& operationTranException != null && !operationTranException.IsWarning)
				{
					throw operationTranException;
				}

                if (ProductionTransactionHelper.CheckOverCompletedOrders(
                    this, ammTran, prodItem, builder.GetMoveOperationTotals(ammTran), orderType.OverCompleteOrders,
                    ProductionSetup.Current.InclScrap.GetValueOrDefault(), out var tranException)
                    && tranException != null && !tranException.IsWarning)
                {
                    throw tranException;
                }
            }
        }

		//private void CheckDuplicateSerialNumberWithoutReleasedTransactions(AMBatch doc)
		//{
		//	try
		//	{
		//		CheckDuplicateSerialNumber(doc);
		//	}
		//	catch (AMTransactionFailedCheckException)
		//	{
		//		if(!HasReferencedBatchesReleased(doc))
		//		{
		//			throw;
		//		}
		//	}
		//}

        /// <summary>
        /// Check all transactions for Repeat Serial Number in current Batch
        /// Equivalent to IN Error Duplicate Serial Number found in Document
        /// Also Check if the Serial Number has already been Issued
        /// </summary>
        protected virtual void CheckDuplicateSerialNumber(AMBatch doc)
        {
            if (doc == null || !PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>())
            {
                return;
            }

            var itemLots = new HashSet<string>();
            var sb = new StringBuilder();

            var bySubItem = PXAccess.FeatureInstalled<FeaturesSet.subItem>();

            foreach (AMDuplicateSerialNumber duplicateResult in PXSelect<
                AMDuplicateSerialNumber,
                Where<AMDuplicateSerialNumber.docType, Equal<Required<AMDuplicateSerialNumber.docType>>,
                    And<AMDuplicateSerialNumber.batNbr, Equal<Required<AMDuplicateSerialNumber.batNbr>>
                >>>.Select(this, doc.DocType, doc.BatNbr))
            {
                if (string.IsNullOrWhiteSpace(duplicateResult?.LotSerialNbr)
                    || string.IsNullOrWhiteSpace(duplicateResult.ProdOrdID)
                    || string.IsNullOrWhiteSpace(duplicateResult.OrderType)
                    || duplicateResult.InventoryID == null
                    || duplicateResult.TranQty.GetValueOrDefault() == 0
                    || duplicateResult.InvtMult.GetValueOrDefault() == 0
					|| duplicateResult.Released == true)
                {
                    continue;
                }

                var si = bySubItem ? duplicateResult.SubItemID.GetValueOrDefault() : 0;
                var newValue = itemLots.Add(string.Join(":", duplicateResult.InventoryID, duplicateResult.LotSerialNbr, si));

                // Check for Duplicate in Document
                if (!newValue)
                {
                    sb.AppendLine(Messages.GetLocal(
                    Messages.DuplicateSerialNumberInDocument,
                    duplicateResult.InventoryCD.TrimIfNotNullEmpty(),
					duplicateResult.LotSerialNbr.TrimIfNotNullEmpty(),
                    duplicateResult.OrderType.TrimIfNotNullEmpty(),
                    duplicateResult.ProdOrdID.TrimIfNotNullEmpty()));
                    continue;
                }

                var isLastOperMove =
                    (duplicateResult.DocType == AMDocType.Move || duplicateResult.DocType == AMDocType.Labor) &&
                    duplicateResult.LastOper.GetValueOrDefault();

                var isReceipt = (isLastOperMove || duplicateResult.DocType == AMDocType.Material) &&
                                duplicateResult.InvtMult > 0;

                if (isReceipt)
                {
                    if (duplicateResult.QtyOnHand.GetValueOrDefault() > 0)
                    {
                        sb.AppendLine(Messages.GetLocal(
                            PX.Objects.IN.Messages.SerialNumberAlreadyReceived,
                            duplicateResult.InventoryCD.TrimIfNotNullEmpty(),
                            duplicateResult.LotSerialNbr.TrimIfNotNullEmpty()));
                    }
                    continue;
                }

                var isIssue = (isLastOperMove || duplicateResult.DocType == AMDocType.Material) &&
                              duplicateResult.InvtMult < 0;

                // If when used the INLotSerialStatus record should be null so QtyOnHand would be null if not yet issued, otherwise would contain a value(record)
                if (isIssue && (duplicateResult.LotSerAssign == INLotSerAssign.WhenReceived && duplicateResult.QtyOnHand.GetValueOrDefault() <= 0 ||
                                duplicateResult.LotSerAssign == INLotSerAssign.WhenUsed && duplicateResult.QtyOnHand != null))
                {
                    sb.AppendLine(Messages.GetLocal(
                        Messages.DuplicateSerialNumber,
                        duplicateResult.InventoryCD.TrimIfNotNullEmpty(),
						duplicateResult.LotSerialNbr.TrimIfNotNullEmpty(),
                        duplicateResult.OrderType.TrimIfNotNullEmpty(),
                        duplicateResult.ProdOrdID.TrimIfNotNullEmpty()));
                }
            }

            if (sb.Length == 0)
            {
                return;
            }

            throw new AMTransactionFailedCheckException(sb.ToString());
        }

        /// <summary>
		/// Check all transactions, if the branch ID doesn't match
		/// make sure an account mapping is set up between the branches
		/// </summary>
		protected virtual void CheckInterBranchTransaction(AMBatch doc, MoveOperationQtyBuilder builder)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.interBranch>())
				return;

			foreach (var result in builder.AllTranRows)
			{
				var tran = (AMMTran)result;
				var item = (InventoryItem)result;
				//the GLtran for the receipt will use site.branchid so we have to check it against the batch branch ahead of time
				var tranSite = INSite.PK.Find(this, tran.SiteID);
				if (!GetAccountMapping(this, doc, item, tranSite, out BranchAcctMapFrom mapfrom, out BranchAcctMapTo mapto))
				{
					Branch branchfrom = (Branch)PXSelectorAttribute.Select<AMBatch.branchID>(BatchRecs.Cache, doc, doc.BranchID);
					Branch branchto = (Branch)PXSelectorAttribute.Select<INSite.branchID>(this.Caches[typeof(INSite)], tranSite, tranSite.BranchID);

					throw new PXException(GL.Messages.BrachAcctMapMissing, (branchfrom != null ? branchfrom.BranchCD : "Undefined"), (branchto != null ? branchto.BranchCD : "Undefined"));
				}
			}
		}

		/// <summary>
        /// Get material transaction lines
        /// </summary>
        /// <param name="doc">material batch header</param>
        /// <returns></returns>
        private PXResultset<AMMTran> GetMaterialTransactions(AMBatch doc)
        {
            if (doc == null || string.IsNullOrWhiteSpace(doc.DocType) || !doc.DocType.EqualsWithTrim(AMDocType.Material))
            {
                return null;
            }

            return PXSelectJoin<AMMTran,
                InnerJoin<InventoryItem, On<AMMTran.inventoryID, Equal<InventoryItem.inventoryID>>,
                LeftJoin<INLotSerClass, On<InventoryItem.lotSerClassID, Equal<INLotSerClass.lotSerClassID>>,
                LeftJoin<AMOrderType, On<AMMTran.orderType, Equal<AMOrderType.orderType>>>>>,
                Where<AMMTran.docType, Equal<Required<AMBatch.docType>>,
                And<AMMTran.batNbr, Equal<Required<AMBatch.batNbr>>>>
                >.Select(this, doc.DocType, doc.BatNbr);
        }

        /// <summary>
        /// Process Manufacturing material transaction
        /// </summary>
        /// <param name="doc"></param>
        protected virtual void ProcessMaterialTran(AMBatch doc)
        {
            //InventoryTranBuilder handles both stock and non stock items
            var inventoryTranBuilder = CreateInventoryTranBuilder(doc, InventoryTranBuilder.TransactionType.IssueReturn);
			var receiptBuilder = CreateInventoryTranBuilder(doc, InventoryTranBuilder.TransactionType.Receipt, true, inventoryTranBuilder.CurrentGlDocument);

            var refBatchReleasedDictionary = ReferencedBatchesReleased(doc);
			if(!HasReferencedBatchesReleased(refBatchReleasedDictionary))
            {
                CheckDuplicateSerialNumber(doc);
            }

			CheckPreassignedSerialNumbers(doc);

            var allTranSplits = PXSelect<
                AMMTranSplit,
                Where<AMMTranSplit.docType, Equal<Required<AMMTranSplit.docType>>,
                    And<AMMTranSplit.batNbr, Equal<Required<AMMTranSplit.batNbr>>>>>
                .Select(this, doc.DocType, doc.BatNbr)
                .ToFirstTableList();
			var allTrans = PXSelectJoin<
                AMMTran, 
                InnerJoin<InventoryItem, On<AMMTran.inventoryID, Equal<InventoryItem.inventoryID>>>,
                Where<AMMTran.docType, Equal<Required<AMBatch.docType>>, 
                    And<AMMTran.batNbr, Equal<Required<AMBatch.batNbr>>>>>
				.Select(this, doc.DocType, doc.BatNbr);

			foreach (PXResult<AMMTran, InventoryItem> result in allTrans)
            {
                var tran = (AMMTran) result;
                var inventoryItem = (InventoryItem) result;

                if (string.IsNullOrWhiteSpace(tran.ProdOrdID))
                {
                    throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.prodOrdID>(TranRecs.Cache));
                }

                if (string.IsNullOrWhiteSpace(tran.OrderType))
                {
                    throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.orderType>(TranRecs.Cache));
                }

				if (tran.Released == true)
				{
					continue;
				}

                if (ReferencedProductionOrders.Add(tran))
                {
					object prodOrderID = tran?.ProdOrdID;
					TranRecs.View.Cache.RaiseFieldVerifying<AMMTran.prodOrdID>(tran, ref prodOrderID);
				}

				CheckOverIssuedMaterial(result, allTrans);

                var refBatchesReleased = refBatchReleasedDictionary.ContainsKey(tran.LineNbr.GetValueOrDefault()) && refBatchReleasedDictionary[tran.LineNbr.GetValueOrDefault()];

                if (!tran.Released.GetValueOrDefault() && !refBatchesReleased)
                {
                    var splits = allTranSplits?.Where(x => x.LineNbr == tran.LineNbr).ToList();
                    tran.FinPeriodID = doc.FinPeriodID;
                    tran.TranDate = doc.TranDate;
                    TranRecs.Update(tran.TranType == AMTranType.Receipt
                        ? receiptBuilder.CreateTranLine(tran, inventoryItem, splits)
                        : inventoryTranBuilder.CreateTranLine(tran, inventoryItem, splits));                        
                }
                else
                {
                    ReferencedInBatches.Add(string.Join("~", tran.INDocType, tran.INBatNbr));
                    ReferencedGlBatches.Add(tran.GLBatNbr);
                }
            }

            if (receiptBuilder?.RefDocNoteRecordRequired == true ||
                inventoryTranBuilder?.RefDocNoteRecordRequired == true)
            {
                EnsureDocNote(doc);
            }

            using (var ts = new PXTransactionScope())
            {
                if (inventoryTranBuilder.HasInTransactions)
                {
                    InventoryBatches.Add(inventoryTranBuilder.CurrentInDocument);
                }
                if (inventoryTranBuilder.HasGlTransactions)
                {
                    GeneralLedgerBatches.Add(inventoryTranBuilder.CurrentGlDocument);
                }
                inventoryTranBuilder.Save();

                if (receiptBuilder.HasInTransactions)
                {
                    InventoryBatches.Add(receiptBuilder.CurrentInDocument);
                }
				if (receiptBuilder.HasGlTransactions)
				{
					GeneralLedgerBatches.Add(receiptBuilder.CurrentGlDocument);
				}
                receiptBuilder.Save();

                if (IsDirty)
                {
                    Actions.PressSave();
                }
                ts.Complete();
            }

            //  re-release General Ledger Batch
            if (ReferencedGlBatches.HasValues)
            {
                Batch checkBatchGL;

                foreach (var gLbatchNumber in ReferencedGlBatches)
                {
                    checkBatchGL = PXSelect<Batch,
                        Where<Batch.module, Equal<BatchModule.moduleGL>,
                            And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>,
                            And<Batch.released, Equal<False>>>
                        >>.Select(this, gLbatchNumber);

                    if (checkBatchGL != null && !GeneralLedgerBatches.Contains(checkBatchGL))
                    {
                        GeneralLedgerBatches.Add(checkBatchGL);
                    }
                }
            }

            //  re-release Inventory Batch
            if (ReferencedInBatches.HasValues)
            {
                INRegister checkBatchIN;

                foreach (var inbatchNumber in ReferencedInBatches)
                {
					var inbatch = inbatchNumber.Split('~');
					if (inbatch.Length != 2)
						continue;
                    checkBatchIN = PXSelect<INRegister,
                        Where<INRegister.docType, Equal<Required<INRegister.docType>>, 
							And<INRegister.refNbr, Equal<Required<INRegister.refNbr>>,
                            And<INRegister.released, Equal<False>>>>>
                        .Select(this, inbatch[0], inbatch[1]);

                    if (checkBatchIN != null && !InventoryBatches.Contains(checkBatchIN))
                    {
                        InventoryBatches.Add(checkBatchIN);
                    }
                }
            }
        }

		private void CheckPreassignedSerialNumbers(AMBatch doc)
		{
			foreach(PXResult<AMMTranSplit, AMMTran, AMProdItem> result in PXSelectJoin<AMMTranSplit, 
				InnerJoin<AMMTran, On<AMMTranSplit.batNbr, Equal<AMMTran.batNbr>,
					And<AMMTranSplit.lineNbr, Equal<AMMTran.lineNbr>>>,
					InnerJoin<AMProdItem, On<AMMTran.orderType, Equal<AMProdItem.orderType>,
						And<AMMTran.prodOrdID, Equal<AMProdItem.prodOrdID>>>>>,
				Where<AMMTranSplit.docType, Equal<Required<AMMTranSplit.docType>>,
					And<AMMTranSplit.batNbr, Equal<Required<AMMTranSplit.batNbr>>>>>.Select(this, doc.DocType, doc.BatNbr))
			{
				var split = (AMMTranSplit)result;
				var tran = (AMMTran)result;
				var item = (AMProdItem)result;

				if(item.PreassignLotSerial == true && item.ParentLotSerialRequired == ParentLotSerialAssignment.OnIssue)
				{
					INLotSerClass lotClass = SelectFrom<INLotSerClass>
						.InnerJoin<InventoryItem>
							.On<INLotSerClass.lotSerClassID.IsEqual<InventoryItem.lotSerClassID>>
						.Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>>.View.Select(this, tran.InventoryID);
					if(lotClass.LotSerTrack != INLotSerTrack.NotNumbered && string.IsNullOrEmpty(split.ParentLotSerialNbr))
					{
						throw new PXException(Messages.MaterialLotSerialNotAssigned, item.OrderType, item.ProdOrdID);
					}
				}
			}
		}

		protected virtual void StoreLotSerialRelationship(AMBatch doc)
        {
			if(!InventoryHelper.LotSerialTrackingFeatureEnabled)
			{
				return;
			}

			List<AMProdItemSplit> returnMaterialParents = new List<AMProdItemSplit>();

			// Counts on index: AMMTranSplit_AMMTranSplitProdMatlLotSerial
			foreach (PXResult<AMMTranSplitProdMatlLotSerial,AMProdMatlLotSerial,AMProdItemSplit> result in SelectFrom<AMMTranSplitProdMatlLotSerial>
				.LeftJoin<AMProdMatlLotSerial>
					.On<AMMTranSplitProdMatlLotSerial.orderType.IsEqual<AMProdMatlLotSerial.orderType>
						.And<AMMTranSplitProdMatlLotSerial.prodOrdID.IsEqual<AMProdMatlLotSerial.prodOrdID>
						.And<AMMTranSplitProdMatlLotSerial.operationID.IsEqual<AMProdMatlLotSerial.operationID>
						.And<AMMTranSplitProdMatlLotSerial.matlLineId.IsEqual<AMProdMatlLotSerial.lineID>
						.And<AMMTranSplitProdMatlLotSerial.lotSerialNbr.IsEqual<AMProdMatlLotSerial.lotSerialNbr>
						.And<Brackets<AMMTranSplitProdMatlLotSerial.parentLotSerialNbr.IsNull
						.Or<AMMTranSplitProdMatlLotSerial.parentLotSerialNbr.IsEqual<AMProdMatlLotSerial.parentLotSerialNbr>>>>>>>>>	
						.LeftJoin<AMProdItemSplit>
						.On<AMMTranSplitProdMatlLotSerial.orderType.IsEqual<AMProdItemSplit.orderType>
						.And<AMMTranSplitProdMatlLotSerial.prodOrdID.IsEqual<AMProdItemSplit.prodOrdID>
						.And<AMMTranSplitProdMatlLotSerial.parentLotSerialNbr.IsEqual<AMProdItemSplit.lotSerialNbr>>>>
				.Where<AMMTranSplitProdMatlLotSerial.docType.IsEqual<@P.AsString>
					.And<AMMTranSplitProdMatlLotSerial.batNbr.IsEqual<@P.AsString>>>.View.ReadOnly.Select(this, doc.DocType, doc.BatNbr))
			{
				var split = result.GetItem<AMMTranSplitProdMatlLotSerial>();
				if(split?.LotSerialNbr == null)
				{
					continue;
				}

				var matlLot = FindCached(split, result.GetItem<AMProdMatlLotSerial>());

				var matlLineId = split.MatlLineId;
				if(matlLineId == null)
				{
					// Covers material added on the fly
					var tran = TranRecs.Locate(new AMMTran { DocType = split.DocType, BatNbr = split.BatNbr, LineNbr = split.LineNbr });
					matlLineId = tran?.MatlLineId;

					matlLot = AMProdMatlLotSerial.PK.FindDirty(this, split.OrderType, split.ProdOrdID, split.OperationID,
						matlLineId, split.LotSerialNbr, split.ParentLotSerialNbr);
				}

				var qtyIssued = split.TranType == AMTranType.Return ? -1 * split.BaseQty.GetValueOrDefault() : split.BaseQty.GetValueOrDefault();

				if (matlLot != null && !string.IsNullOrWhiteSpace(matlLot?.LotSerialNbr))
				{
					matlLot.QtyIssued += qtyIssued;
					if(matlLot.QtyIssued > 0)
					matlLot = MatlLotSerialRecs.Update(matlLot);
					else
					{
						//look up the parent splits and add them to the list to be rechecked
						var parentSplits = SelectFrom<AMProdItemSplit>.Where<AMProdItemSplit.orderType.IsEqual<@P.AsString>
							.And<AMProdItemSplit.prodOrdID.IsEqual<@P.AsString>>
							.And<AMProdItemSplit.lotSerialNbr.IsEqual<@P.AsString>>>.View.Select(this, matlLot.OrderType, matlLot.ProdOrdID, matlLot.ParentLotSerialNbr);
						MatlLotSerialRecs.Delete(matlLot);
						foreach(var parentSplit in parentSplits)
							returnMaterialParents.Add(parentSplit);
					}
					continue;
				}

				if(matlLineId == null)
				{
					continue;
				}

				var newMatlLot = new AMProdMatlLotSerial
				{
					OrderType = split.OrderType,
					ProdOrdID = split.ProdOrdID,
					OperationID = split.OperationID,
					LineID = matlLineId,
					LotSerialNbr = split.LotSerialNbr.ToUpper(),
					ParentLotSerialNbr = split.ParentLotSerialNbr ?? string.Empty,
					QtyIssued = qtyIssued
				};

				matlLot = MatlLotSerialRecs.Insert(newMatlLot);

				if (matlLot == null)
                {
					throw new Exception($"Error inserting AMProdMatlLotSerial record. {newMatlLot.DebuggerDisplay}");
                }
				var prodItemSplit = FindCached(split, result.GetItem<AMProdItemSplit>());
				if (!string.IsNullOrWhiteSpace(prodItemSplit?.LotSerialNbr) && prodItemSplit.IsMaterialLinked != true)
				{
					if (qtyIssued > 0)
					{
						prodItemSplit.IsMaterialLinked = true;
						prodItemSplits.Update(prodItemSplit);
					}
					else
					{
						returnMaterialParents.Add(prodItemSplit);
					}
				}
			}
			SetMaterialLinkedItems(returnMaterialParents);
        }

		protected virtual void SetMaterialLinkedItems(List<AMProdItemSplit> aMProdItems)
		{
			if (aMProdItems != null && aMProdItems.Count > 0)
			{
				aMProdItems.Distinct(p => new { p.OrderType, p.ProdOrdID, p.SplitLineNbr });
			}
			var results = new AMProdMatlLotSerial();
			foreach (AMProdItemSplit item in aMProdItems)
			{
				if (item?.LotSerialNbr != null)
				{
					results = PXSelectGroupBy<AMProdMatlLotSerial
									, Where<AMProdMatlLotSerial.orderType, Equal<Required<AMProdMatlLotSerial.orderType>>
									, And<AMProdMatlLotSerial.prodOrdID, Equal<Required<AMProdMatlLotSerial.prodOrdID>>
									, And<AMProdMatlLotSerial.parentLotSerialNbr, Equal<Required<AMProdMatlLotSerial.parentLotSerialNbr>>>>>
									, Aggregate<Sum<AMProdMatlLotSerial.qtyIssued>>>
									.Select(this, item.OrderType, item.ProdOrdID, item.LotSerialNbr);
					var prodItemSplit = prodItemSplits.Cache.LocateElse(item);
					item.IsMaterialLinked = (results != null && results.QtyIssued > 0);
					prodItemSplits.Update(item);
				}
			}
        }
		private AMProdMatlLotSerial FindCached(AMMTranSplitProdMatlLotSerial split, AMProdMatlLotSerial matlLotSerial)
		{
			// Here we need to look in cache to see if this combination has already been looped on and cached
				if(matlLotSerial?.LotSerialNbr == null)
				{
					return (AMProdMatlLotSerial)MatlLotSerialRecs.Cache.Locate(new AMProdMatlLotSerial
					{
						OrderType = split.OrderType,
						ProdOrdID = split.ProdOrdID,
						OperationID = split.OperationID,
						LineID = split.MatlLineId,
						LotSerialNbr = split.LotSerialNbr.ToUpper(),
						ParentLotSerialNbr = split.ParentLotSerialNbr ?? string.Empty
					});
				}

				return MatlLotSerialRecs.Cache.LocateElse(matlLotSerial);
		}

		private AMProdItemSplit FindCached(AMMTranSplitProdMatlLotSerial split, AMProdItemSplit itemSplit)
		{
			//in the queried object is empty for some reason, look it up from the cache based on lotserialnbr
			if (itemSplit?.ProdOrdID == null)
			{
				return prodItemSplits.Cache.Cached.RowCast<AMProdItemSplit>().FirstOrDefault(x =>
					x.OrderType == split.OrderType && x.ProdOrdID == split.ProdOrdID && x.LotSerialNbr == split.ParentLotSerialNbr);
			}

			return prodItemSplits.Cache.LocateElse(itemSplit);
		}

		private bool HasReferencedBatchesReleased(Dictionary<int, bool> refBatchLines)
		{
			if(refBatchLines == null)
			{
				return false;
			}

			foreach (var keyValue in refBatchLines)
			{
				if (keyValue.Value == true)
				{
					return true;
				}
			}

			return false;
		}

        /// <summary>
        /// Records which lines have released related batches
        /// </summary>
        /// <param name="doc">Manufacturing document header</param>
        /// <returns>Dictionary of line nbrs for ammtran lines with bool value for found released reference (true) or not found (false)</returns>
        protected virtual Dictionary<int, bool> ReferencedBatchesReleased(AMBatch doc)
        {
            var dic = new Dictionary<int, bool>();
            INRegister inRegister = null;
            Batch glBatch = null;
            foreach (AMMTran tran in PXSelect<AMMTran,
                Where<AMMTran.docType, Equal<Required<AMBatch.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMBatch.batNbr>>>>
                >.Select(this, doc.DocType, doc.BatNbr))
            {
                bool refBatchesReleased = false;

                if (!tran.Released.GetValueOrDefault() &&
                    !string.IsNullOrWhiteSpace(tran.INBatNbr))
                {
                    if (inRegister == null
                        || !inRegister.DocType.EqualsWithTrim(tran.INDocType)
                        || !inRegister.RefNbr.EqualsWithTrim(tran.INBatNbr))
                    {
                        inRegister = PXSelect<INRegister,
                            Where<INRegister.docType, Equal<Required<INRegister.docType>>,
                                And<INRegister.refNbr, Equal<Required<INRegister.refNbr>>>>
                            >.SelectWindowed(this, 0, 1, tran.INDocType, tran.INBatNbr);
                    }
                    if (inRegister != null && inRegister.Released.GetValueOrDefault())
                    {
                        refBatchesReleased = true;
                    }
                }

                if (!tran.Released.GetValueOrDefault() &&
                    !string.IsNullOrWhiteSpace(tran.GLBatNbr))
                {
                    if (glBatch == null
                        || !glBatch.BatchNbr.EqualsWithTrim(tran.GLBatNbr))
                    {
                        glBatch = PXSelect<Batch,
                            Where<Batch.module, Equal<BatchModule.moduleGL>,
                                And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>
                            >.SelectWindowed(this, 0, 1, tran.GLBatNbr);
                    }
                    if (glBatch != null && glBatch.Released.GetValueOrDefault())
                    {
                        refBatchesReleased = true;
                    }
                }

                dic.Add(tran.LineNbr.GetValueOrDefault(), refBatchesReleased);
            }
            return dic;
        }

        /// <summary>
        /// Update material records on production orders related to the document
        /// </summary>
        /// <param name="doc">document being processed</param>
        protected virtual void UpdateProductionMaterial(AMBatch doc)
        {
            if (!AMDocType.IsDocTypeMaterial(doc.DocType))
            {
                return;
            }

            var allProdMatlSplits = PXSelectJoin<AMProdMatlSplit,
                LeftJoin<AMMTranProdMatl, On<AMProdMatlSplit.orderType, Equal<AMMTranProdMatl.orderType>,
                    And<AMProdMatlSplit.prodOrdID, Equal<AMMTranProdMatl.prodOrdID>,
                        And<AMProdMatlSplit.operationID, Equal<AMMTranProdMatl.operationID>,
                            And<AMProdMatlSplit.lineID, Equal<AMMTranProdMatl.matlLineId>>>>>>,
                Where<AMMTranProdMatl.docType, Equal<Required<AMMTranProdMatl.docType>>,
                    And<AMMTranProdMatl.batNbr, Equal<Required<AMMTranProdMatl.batNbr>>>>>.Select(this, doc.DocType, doc.BatNbr).ToFirstTableList();

            foreach (PXResult<AMMTran, AMProdMatl, AMProdItem> result in PXSelectJoin<AMMTran,
                LeftJoin<AMProdMatl, On<AMMTran.orderType, Equal<AMProdMatl.orderType>,
                    And<AMMTran.prodOrdID, Equal<AMProdMatl.prodOrdID>,
                    And<AMMTran.operationID, Equal<AMProdMatl.operationID>,
                    And<AMMTran.matlLineId, Equal<AMProdMatl.lineID>>>>>, LeftJoin<AMProdItem, On < AMProdMatl.FK.ProductionOrder>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>>.Select(this, doc.DocType, doc.BatNbr))
            {
                var ammTran = (AMMTran) result;

                if (AMDisassembleBatch.IsDisassembleBatchTran(doc, ammTran) || ammTran.IsScrap.GetValueOrDefault())
                {
                    continue;
                }

                var amProdMatl = ProductionMatl.Cache.LocateElse((AMProdMatl)result);
                var prodItem = ProductionItems.Cache.LocateElse((AMProdItem)result);

                if (ammTran.MatlLineId == null || amProdMatl?.LineID == null)
                {
                    UpdateProductionMaterial(ammTran, prodItem);
                    continue;
                }

                UpdateProductionMaterialByMatlLineID(ammTran, amProdMatl, 
                    allProdMatlSplits?.Where(x =>
                        x.OrderType == amProdMatl.OrderType
                        && x.ProdOrdID == amProdMatl.ProdOrdID && x.OperationID == amProdMatl.OperationID &&
                        x.LineID == amProdMatl.LineID).ToList(), prodItem);
            }
        }

		/// <summary>
		/// Lookup process for updating production material prior to 5.30.2126.49
		/// </summary>
		protected virtual void UpdateProductionMaterial(AMMTran ammTran, AMProdItem prodItem)
		{
			if (ammTran == null)
			{
				throw new PXArgumentException(nameof(ammTran));
			}

			PXResultset<AMProdMatl> resultset = PXSelect<AMProdMatl,
					Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
						And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
						And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>,
							And<AMProdMatl.inventoryID, Equal<Required<AMProdMatl.inventoryID>>>>>>
							>.Select(this, ammTran.OrderType, ammTran.ProdOrdID, ammTran.OperationID, ammTran.InventoryID);

			if (resultset == null
					|| !resultset.Any_())
			{
				return;
			}

			decimal? ammTranAsProdQty = 0m;
			decimal? prodMatlUomQtyRT = 0m;
			decimal? amountRT = 0m;
			int counter = 0;
			string qtyUom = string.Empty;

			//foreach in case same material repeats in prod/oper
			//  accounts for ammtran in a diff uom than material uom
			foreach (AMProdMatl amProdMatl in resultset)
			{
				counter++;
				if (counter == 1)
				{
					prodMatlUomQtyRT = ammTran.Qty;
					ammTranAsProdQty = prodMatlUomQtyRT;
					amountRT = ammTran.TranAmt;
					qtyUom = ammTran.UOM;
				}

				if (prodMatlUomQtyRT == 0)
				{
					break;
				}

				if (!amProdMatl.UOM.EqualsWithTrim(qtyUom))
				{
					decimal qtyFrom = prodMatlUomQtyRT ?? 0;
					decimal? qtyTo = 0;

					if (UomHelper.TryConvertFromToQty<AMProdMatl.inventoryID>(ProductionMatl.Cache, amProdMatl, qtyUom,
							amProdMatl.UOM, qtyFrom, out qtyTo))
					{
						prodMatlUomQtyRT = qtyTo;

						if (counter == 1)
						{
							ammTranAsProdQty = prodMatlUomQtyRT;
						}

						// shows the UOM related to the current qty value - converted to prod uom
						qtyUom = amProdMatl.UOM.Trim();
					}
				}

				var expectedQtyComplete = amProdMatl.TotalQtyRequired.GetValueOrDefault();

				if (expectedQtyComplete < 0)
				{
					expectedQtyComplete = 0;
				}

				decimal? prodMatlQty = prodMatlUomQtyRT;
				decimal? prodAmount = amountRT;

				// if more than 1 matl only the last oper matl rec gets the full overflow qty - else use qty required

				if (prodMatlQty > expectedQtyComplete && counter != resultset.Count)
				{
					prodMatlQty = expectedQtyComplete;

					if (ammTranAsProdQty != 0)
					{
						prodAmount = prodAmount * (prodMatlQty / ammTranAsProdQty);
					}
				}

				if (prodMatlQty != 0)
				{
					amProdMatl.QtyActual += prodMatlQty;
					amProdMatl.TotActCost += prodAmount;
					var matlStatus = ProductionDetailGraph.Value.GetMaterialStatus(prodItem, amProdMatl);

					if (matlStatus != null)
					{
						amProdMatl.StatusID = matlStatus;
					}

					UpdateProductionMaterialSplits(ammTran, ProductionMatl.Update(amProdMatl));
				}
				prodMatlUomQtyRT -= prodMatlQty;
				amountRT -= prodAmount;
			}
		}

		protected virtual void UpdateProductionMaterialByMatlLineID(AMMTran ammTran, AMProdMatl amProdMatl, List<AMProdMatlSplit> prodMatlSplits, AMProdItem prodItem)
        {
			if (ammTran?.BatNbr == null)
			{
				throw new PXArgumentException(nameof(ammTran));
			}

			if (amProdMatl == null
			|| ammTran.MatlLineId == null
			|| ammTran.Qty.GetValueOrDefault() == 0
			|| string.IsNullOrWhiteSpace(ammTran.UOM))
			{
				return;
			}

			var prodMatlQty = ammTran.Qty.GetValueOrDefault();
			var prodMatlAmount = ammTran.TranAmt.GetValueOrDefault();

			//Convert if different Units from transaction to production details
			if (!ammTran.UOM.EqualsWithTrim(amProdMatl.UOM))
			{
				if (UomHelper.TryConvertFromToQty<AMProdMatl.inventoryID>(ProductionMatl.Cache, amProdMatl,
				ammTran.UOM, amProdMatl.UOM, prodMatlQty, out var convertedQty))
				{
					prodMatlQty = convertedQty.GetValueOrDefault();
				}
			}

			if (prodMatlQty == 0 && prodMatlAmount == 0)
			{
				return;
			}

			amProdMatl.QtyActual = amProdMatl.QtyActual.GetValueOrDefault() + prodMatlQty;
			amProdMatl.TotActCost = amProdMatl.TotActCost.GetValueOrDefault() + prodMatlAmount;
			amProdMatl = ProductionMatl.Update(amProdMatl);
			var matlStatus = ProductionDetailGraph.Value.GetMaterialStatus(prodItem, amProdMatl);
			if (matlStatus != null)
			{
				amProdMatl.StatusID = matlStatus;
			}
			UpdateProductionMaterialSplits(ammTran, ProductionMatl.Update(amProdMatl), prodMatlSplits);
		}

        protected virtual void UpdateProductionMaterialSplits(AMMTran ammTran, AMProdMatl amProdMatl)
        {
            UpdateProductionMaterialSplits(ammTran, amProdMatl, AMProdMatl.GetSplits(this, amProdMatl));
        }

        protected virtual void UpdateProductionMaterialSplits(AMMTran ammTran, AMProdMatl prodMatl, List<AMProdMatlSplit> prodMatlSplits)
        {
            if (prodMatl?.ProdOrdID == null || prodMatlSplits == null)
            {
                return;
            }

            var isIssue = ammTran.TranType == AMTranType.Issue && ammTran.Qty.GetValueOrDefault() > 0;
            var allocatedIssueQty = isIssue ? ammTran.Qty.GetValueOrDefault() : 0m;
            var allocatedIssueBaseQty = isIssue ? ammTran.BaseQty.GetValueOrDefault() : 0m;
            var issueQty = ammTran.Qty.GetValueOrDefault();
            var issueBaseQty = ammTran.BaseQty.GetValueOrDefault();

			AMProdMatlSplit insertedSplit = ProdMatlSplits.Cache.Cached.RowCast<AMProdMatlSplit>()
				.FirstOrDefault(s => s.OrderType == ammTran.OrderType && s.ProdOrdID == ammTran.ProdOrdID
					&& s.OperationID == ammTran.OperationID && s.LineID == ammTran.MatlLineId);
			if (!prodMatlSplits.Any()
				&& insertedSplit != null)
			{
				prodMatlSplits.Insert(0, insertedSplit);
			}

            foreach (var split in prodMatlSplits.OrderByDescending(x => x.IsAllocated).ThenBy(x => x.PlanID))
            {
                // Need to find cached value in case already updated or deleted
                var splitCopy = ProdMatlSplits.Cache.LocateElseCopy(split);

                if (splitCopy?.ProdOrdID == null || ProdMatlSplits.Cache.GetStatus(splitCopy) == PXEntryStatus.Deleted)
                {
                    continue;
                }

                if (splitCopy.IsAllocated.GetValueOrDefault())
                {
                    if (allocatedIssueQty <= 0)
                    {
                        continue;
                    }

                    var qty = allocatedIssueQty;
                    var baseQty = allocatedIssueBaseQty;

                    if (qty > splitCopy.Qty.GetValueOrDefault())
                    {
                        qty = splitCopy.Qty.GetValueOrDefault();
                        baseQty = splitCopy.BaseQty.GetValueOrDefault();
                    }

                    allocatedIssueQty -= qty;
                    allocatedIssueBaseQty -= baseQty;

                    if (splitCopy.ParentSplitLineNbr == null)
                    {
                        //When not marked with a parent it was user allocated (not allocated from a PO) and as of writing this code the user allocated ...
                        //  ... lines will reduce the non allocated lines so the total is total issued. Where partial receipts from a PO are not total.
                        issueQty -= qty;
                        issueBaseQty -= baseQty;
                    }

                    UpdateProductionMaterialSplit(splitCopy, qty, baseQty);
                    continue;
                }

                var regQty = issueQty;
                var regBaseQty = issueBaseQty;

                if (regQty == 0m)
                {
                    continue;
                }

                if (regQty > splitCopy.Qty.GetValueOrDefault())
                {
                    regQty = splitCopy.Qty.GetValueOrDefault();
                    regBaseQty = splitCopy.BaseQty.GetValueOrDefault();
                }

                issueQty -= regQty;
                issueBaseQty -= regBaseQty;

                UpdateProductionMaterialSplit(splitCopy, regQty, regBaseQty);
            }
        }

        protected virtual void UpdateProductionMaterialSplit(AMProdMatlSplit split, decimal qty, decimal baseQty)
        {
            split.Qty = (split.Qty.GetValueOrDefault() - qty).NotLessZero();
            split.BaseQty = (split.BaseQty.GetValueOrDefault() - baseQty).NotLessZero();

            //if split is tied to a po receipt, don't delete the split record
            if (split.Qty.GetValueOrDefault() <= 0 && split.POLineNbr == null)
            {
                // Mark the qty to zero to correctly delete the item plan record, then delete to get ride of the split record
                ProdMatlSplits.Delete(ProdMatlSplits.Update(split));
                return;
            }

            ProdMatlSplits.Update(split);
        }

		protected virtual void UpdateProductionMaterialSplitQty(AMProdMatl matl)
		{
			var foundSplit = false;

			var prodMatl = ProductionMatl.Cache.LocateElse(matl);

			foreach (var split in AMProdMatl.GetSplits(this, prodMatl))
			{
				if (split.IsAllocated.GetValueOrDefault() == true)
				{
					continue;
				}

				foundSplit = true;

				split.Qty = prodMatl.Qty;
				split.BaseQty = prodMatl.BaseQty;

				ProdMatlSplits.Update(split);
			}

			if (foundSplit)
			{
				return;
			}

			ProdMatlSplits.Insert((AMProdMatlSplit)prodMatl);
		}

        /// <summary>
        /// Material transactions allow for adding material on the fly (not on the production order). 
        /// This call will add material to the production details if not already
        /// </summary>
        /// <param name="doc"></param>
        protected virtual void AddMaterialToOrder(AMBatch doc)
        {
            if (!AMDocType.IsDocTypeMaterial(doc.DocType))
            {
                return;
            }

            foreach (PXResult<AMMTran, InventoryItem> result in PXSelectJoin<AMMTran,
                InnerJoin<InventoryItem, On<AMMTran.inventoryID, Equal<InventoryItem.inventoryID>>>,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>>>>.Select(this, doc.DocType, doc.BatNbr))
            {
                var matlTran = (AMMTran)result;
                var inventoryItem = (InventoryItem)result;

                if (string.IsNullOrWhiteSpace(matlTran?.ProdOrdID) ||
                    matlTran.MatlLineId != null || AMDisassembleBatch.IsDisassembleBatchTran(doc, matlTran) || 
                    inventoryItem?.InventoryID == null || matlTran.IsScrap.GetValueOrDefault())
                {
                    continue;
                }

                AMProdMatl amProdMatl = PXSelect<AMProdMatl,
                    Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                        And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>,
                        And<AMProdMatl.operationID, Equal<Required<AMProdMatl.operationID>>,
                        And<AMProdMatl.inventoryID, Equal<Required<AMProdMatl.inventoryID>>,
                        And<Where<AMProdMatl.subItemID, Equal<Required<AMProdMatl.subItemID>>,
                            Or<Not<FeatureInstalled<FeaturesSet.subItem>>>>>>>>>
                    >.Select(this, matlTran.OrderType, matlTran.ProdOrdID, matlTran.OperationID, matlTran.InventoryID, matlTran.SubItemID);

                var cacheStatus = amProdMatl == null
                    ? PXEntryStatus.Notchanged
                    : ProductionMatl.Cache.GetStatus(amProdMatl);

                //   Could also be "inserted" status so we need to account for inserted also (and perform an update). [Ref Bug 1451]
                //   Inserted occurs for multiple material lines of the same item - all of which do not exist on the order details
                if (amProdMatl != null && cacheStatus != PXEntryStatus.Inserted)
                {
                    continue;
                }

                var orderResult = (PXResult<AMProdItem, AMProdOper>)PXSelectJoin<AMProdItem,
                    InnerJoin<AMProdOper, On<AMProdItem.orderType, Equal<AMProdOper.orderType>,
                        And<AMProdItem.prodOrdID, Equal<AMProdOper.prodOrdID>>>>,
                    Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                            And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>
                >.SelectWindowed(this, 0, 1, matlTran.OrderType, matlTran.ProdOrdID, matlTran.OperationID);

                if (orderResult == null)
                {
                    continue;
                }

                var amProdItem = ProductionItems.Cache.LocateElse((AMProdItem)orderResult);
                var amProdOper = ProductionOpers.Cache.LocateElse((AMProdOper)orderResult);

				if (amProdItem == null || string.IsNullOrWhiteSpace(amProdItem.ProdOrdID) ||
                    string.IsNullOrWhiteSpace(amProdItem.StatusID) ||
                    !ProductionStatus.IsValidTransactionStatus(amProdItem) ||
                    amProdOper == null || string.IsNullOrWhiteSpace(amProdOper.ProdOrdID))
                {
                    continue;
                }

                ProductionOpers.Current = amProdOper;

                if (amProdMatl != null && cacheStatus == PXEntryStatus.Inserted)
                {
                    var tranQty = matlTran.Qty.GetValueOrDefault();
                    var tranAmount = matlTran.TranAmt.GetValueOrDefault();
                    if (string.CompareOrdinal(matlTran.UOM, amProdMatl.UOM) != 0)
                    {
                        decimal? qtyTo = 0;
                        if (UomHelper.TryConvertFromToQty<AMProdMatl.inventoryID>(ProductionMatl.Cache, amProdMatl, matlTran.UOM,
                            amProdMatl.UOM, matlTran.Qty.GetValueOrDefault(), out qtyTo))
                        {
                            tranQty = qtyTo.GetValueOrDefault();
                        }
                        decimal? amountTo = 0;
                        if (UomHelper.TryConvertFromToCost<AMProdMatl.inventoryID>(ProductionMatl.Cache, amProdMatl, matlTran.UOM,
                            amProdMatl.UOM, matlTran.TranAmt.GetValueOrDefault(), out amountTo))
                        {
                            tranAmount = amountTo.GetValueOrDefault();
                        }
                    }

                    amProdMatl.QtyActual = amProdMatl.QtyActual.GetValueOrDefault() + tranQty;
                    amProdMatl.TotActCost = amProdMatl.TotActCost.GetValueOrDefault() + tranAmount;

                    ProductionMatl.Update(amProdMatl);
                    continue;
                }

                var newMatlLine = ProductionMatl.Insert(new AMProdMatl
                {
                    Descr = inventoryItem.Descr,
                    InventoryID = matlTran.InventoryID,
                    SubItemID = matlTran.SubItemID,
                    OperationID = matlTran.OperationID,
                    OrderType = matlTran.OrderType,
                    ProdOrdID = matlTran.ProdOrdID
                });

                if (newMatlLine == null)
                {
                    throw new PXException(Messages.UnableToInsertMaterialToProductionOrder, inventoryItem.InventoryCD, amProdOper.OrderType, amProdOper.ProdOrdID, amProdOper.OperationCD);
                }

                newMatlLine.UOM = matlTran.UOM;
                newMatlLine.QtyActual = matlTran.Qty.GetValueOrDefault();
                newMatlLine.QtyReq = matlTran.Qty.GetValueOrDefault();
                newMatlLine.BatchSize = 0m;
                newMatlLine.SiteID = matlTran.SiteID;
                newMatlLine.TotActCost = matlTran.TranAmt;
                newMatlLine.UnitCost = matlTran.UnitCost;
                newMatlLine.SortOrder = newMatlLine.LineNbr;

                newMatlLine = ProductionMatl.Update(newMatlLine);
				var newMatlStatus = ProductionDetailGraph.Value.GetMaterialStatus(amProdItem, newMatlLine);
				if (newMatlStatus != null && newMatlLine.StatusID != newMatlStatus)
				{
					newMatlLine.StatusID = newMatlStatus;
					ProductionMatl.Update(newMatlLine);
				}

				if (newMatlLine == null)
                {
                    throw new PXException(Messages.UnableToInsertMaterialToProductionOrder, inventoryItem.InventoryCD, amProdOper.OrderType, amProdOper.ProdOrdID, amProdOper.OperationCD);
                }

                matlTran.MatlLineId = newMatlLine.LineID;
                TranRecs.Update(matlTran);

                amProdOper = ProductionOpers.Cache.LocateElse(amProdOper);
                if (amProdOper == null || amProdOper.LineCntrMatl.GetValueOrDefault() >= newMatlLine.LineID.GetValueOrDefault())
                {
                    continue;
                }

                //For some reason the line counters for operations are not working here...
                amProdOper.LineCntrMatl = newMatlLine.LineID;
                ProductionOpers.Update(amProdOper);
            }
        }

        protected virtual void InsertProdTransactionEvents(AMBatch doc)
        {
            foreach (var amProdEvnt in ProductionEventHelper.BuildTransactionEvents(doc, ReferencedProductionOrders.KeysList().ToArray()))
            {
                ProductionItems.Current = AMProdItem.PK.Find(this, amProdEvnt.OrderType, amProdEvnt.ProdOrdID);
                ProductionEvents.Insert(amProdEvnt);
            }
        }

        /// <summary>
        /// Release related inventory transactions
        /// </summary>
        protected virtual void ReleaseIN()
        {
            if (InventoryBatches == null || !InventoryBatches.Any_())
            {
                return;
            }

            // We cannot pass in the full list as we get no error details back if there is a failure.
            //      If list > 1 then the error is always "Error: At least one item has not been processed." with no trace window additional details
            // Process one at a time to get better error results
            foreach (var unreleasedBatch in InventoryBatches.Where(batch => !batch.Released.GetValueOrDefault()).OrderBy(batch => batch.DocType).ToList())
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"Releasing inventory transaction {unreleasedBatch.DocType} {unreleasedBatch.RefNbr}");
#endif
                INDocumentRelease.ReleaseDoc(new List<INRegister>(new[] { unreleasedBatch }), false, releaseFromHold: true);
            }

			ActualizeINRegister(InventoryBatches);
		}

        protected virtual void ActualizeINRegister(List<INRegister> inventoryBatches)
        {
            for (var batchIndex = 0; batchIndex < inventoryBatches.Count; batchIndex++)
            {
                inventoryBatches[batchIndex] = SelectFrom<INRegister>
                    .Where<INRegister.docType.IsEqual<INRegister.docType.FromCurrent>
                        .And<INRegister.refNbr.IsEqual<INRegister.refNbr.FromCurrent>>>
                    .View.SelectSingleBound(this, new object[] { inventoryBatches[batchIndex] });
            }
        }

        /// <summary>
        /// Release related general ledger transactions
        /// </summary>
        protected virtual void ReleaseGL()
        {
            if (GeneralLedgerBatches != null && GeneralLedgerBatches.Any_())
            {
                JournalEntry.ReleaseBatch(GeneralLedgerBatches.Where(batch => batch != null && !batch.Released.GetValueOrDefault()).ToList(), null, true);
            }
        }

        /// <summary>
        /// There are re-release scenarios where users edit transactions leaving empty related transactions.
        /// Here we delete them so they are not left over after release
        /// </summary>
        protected virtual void DeleteEmptyGL(AMBatch doc)
        {
            if (doc?.DocType == null)
            {
                return;
            }

            var deleteList = new List<Batch>();
            foreach (Batch batch in PXSelectReadonly<Batch,
                Where<Batch.released, Equal<False>,
                    And<BatchExt.aMDocType, Equal<Required<BatchExt.aMDocType>>,
                        And<BatchExt.aMBatNbr, Equal<Required<BatchExt.aMBatNbr>>>>>
            >.Select(this, doc.DocType, doc.BatNbr))
            {
                if (batch == null || batch.CuryCreditTotal.GetValueOrDefault() != 0 ||
                    batch.CuryDebitTotal.GetValueOrDefault() != 0)
                {
                    continue;
                }

                deleteList.Add(batch);
            }

            if (deleteList.Count == 0)
            {
                return;
            }

            var je = CreateInstance<JournalEntry>();
            JournalEntryAMExtension.SetIsInternalCall(je, true);

            foreach (var batch in deleteList)
            {
                je.Clear();
                je.BatchModule.Delete(batch);
                je.Actions.PressSave();
            }
        }

        /// <summary>
        /// There are re-release scenarios where users edit transactions leaving empty related transactions.
        /// Here we delete them so they are not left over after release
        /// </summary>
        protected virtual void DeleteUnreleasedIN(AMBatch doc)
        {
            if (doc?.DocType == null)
            {
                return;
            }

            var deleteList = new List<INRegister>();
            foreach (INRegister inDoc in PXSelectReadonly<INRegister,
                Where<INRegister.released, Equal<False>,
                    And<INRegisterExt.aMDocType, Equal<Required<INRegisterExt.aMDocType>>,
                        And<INRegisterExt.aMBatNbr, Equal<Required<INRegisterExt.aMBatNbr>>>>>
            >.Select(this, doc.DocType, doc.BatNbr))
            {
                if (inDoc == null)
                {
                    continue;
                }

                deleteList.Add(inDoc);
            }

            if (deleteList.Count == 0)
            {
                return;
            }

            foreach (var inDoc in deleteList)
            {
                // Only listing those doc types which MFG creates
                switch (inDoc.DocType)
                {
                    case INDocType.Adjustment:
                        var aGraph = CreateInstance<INAdjustmentEntry>();
                        aGraph.adjustment.Current = inDoc;
                        aGraph.adjustment.Delete(inDoc);
                        PXTrace.WriteInformation(Messages.DeletingUnreleasedINTransaction,
                            InventoryHelper.GetINDocTypeDescription(inDoc.DocType), inDoc.RefNbr,
                            AMDocType.GetDocTypeDesc(doc.DocType), doc.BatNbr);
                        aGraph.Persist();
                        break; 
                    case INDocType.Receipt:
                        var rGraph = CreateInstance<INReceiptEntry>();
                        rGraph.receipt.Current = inDoc;
                        rGraph.receipt.Delete(inDoc);
                        PXTrace.WriteInformation(Messages.DeletingUnreleasedINTransaction,
                            InventoryHelper.GetINDocTypeDescription(inDoc.DocType), inDoc.RefNbr,
                            AMDocType.GetDocTypeDesc(doc.DocType), doc.BatNbr);
                        rGraph.Persist();
                        break;
                    case INDocType.Issue:
                        var iGraph = CreateInstance<INIssueEntry>();
                        iGraph.issue.Current = inDoc;
                        iGraph.issue.Delete(inDoc);
                        PXTrace.WriteInformation(Messages.DeletingUnreleasedINTransaction,
                            InventoryHelper.GetINDocTypeDescription(inDoc.DocType), inDoc.RefNbr,
                            AMDocType.GetDocTypeDesc(doc.DocType), doc.BatNbr);
                        iGraph.Persist();
                        break;
                }
            }
        }

        /// <summary>
        /// Release related Manufacturing transactions
        /// </summary>
        protected virtual void ReleaseAM(List<AMBatch> batchList)
        {
            if (batchList != null && batchList.Any_())
            {
                AMDocumentRelease.ReleaseDoc(batchList.Where(batch => batch != null && !batch.Released.GetValueOrDefault()).ToList(), false);
            }
        }

		/// <summary>
		/// Check that there is an account mapping between branches
		/// </summary>
		public static bool GetAccountMapping(PXGraph graph, AMBatch batch, InventoryItem item, INSite tranSite, out BranchAcctMapFrom mapfrom, out BranchAcctMapTo mapto)
		{
			mapfrom = null;
			mapto = null;

			if (batch.BranchID == tranSite.BranchID || tranSite.BranchID == null || item.InvtAcctID == null) return true;

			Branch branch = (Branch)PXSelectorAttribute.Select<INSite.branchID>(graph.Caches[typeof(INSite)], tranSite, tranSite.BranchID);

			if (branch == null)
			{
				throw new PXException(GL.Messages.BranchMissing);
			}

			Branch batchBranch = (Branch)PXSelectorAttribute.Select<AMBatch.branchID>(graph.Caches[typeof(AMBatch)], batch, batch.BranchID);
			Organization batchOrganization = OrganizationMaint.FindOrganizationByID(graph, batchBranch.OrganizationID);


			if (!(batchBranch.OrganizationID != branch.OrganizationID
				|| batchOrganization.OrganizationType == OrganizationTypes.WithBranchesBalancing
				&& tranSite.BranchID != batch.BranchID))
			{
				return true;
			}

			if (!PXAccess.FeatureInstalled<FeaturesSet.interBranch>()) return false;

			Account account = (Account)PXSelectorAttribute.Select<InventoryItem.invtAcctID>(graph.Caches[typeof(InventoryItem)], item, item.InvtAcctID);

			if (account == null)
			{
				throw new PXException(GL.Messages.AccountMissing);
			}

			mapfrom = PXSelectReadonly<BranchAcctMapFrom, Where<BranchAcctMapFrom.fromBranchID, Equal<Required<Batch.branchID>>,
				And<BranchAcctMapFrom.toBranchID, Equal<Required<GLTran.branchID>>, And<Required<Account.accountCD>,
				Between<BranchAcctMapFrom.fromAccountCD, BranchAcctMapFrom.toAccountCD>>>>>.Select(graph, batch.BranchID, tranSite.BranchID, account.AccountCD);
			if (mapfrom == null)
			{
				mapfrom = PXSelectReadonly<BranchAcctMapFrom, Where<BranchAcctMapFrom.fromBranchID, Equal<Required<Batch.branchID>>,
					And<BranchAcctMapFrom.toBranchID, IsNull, And<Required<Account.accountCD>,
					Between<BranchAcctMapFrom.fromAccountCD, BranchAcctMapFrom.toAccountCD>>>>>.Select(graph, batch.BranchID, account.AccountCD);

				if (mapfrom == null || mapfrom.MapSubID == null)
				{
					return false;
				}
			}

			mapto = PXSelectReadonly<BranchAcctMapTo, Where<BranchAcctMapTo.toBranchID, Equal<Required<Batch.branchID>>,
				And<BranchAcctMapTo.fromBranchID, Equal<Required<GLTran.branchID>>, And<Required<Account.accountCD>,
				Between<BranchAcctMapTo.fromAccountCD, BranchAcctMapTo.toAccountCD>>>>>.Select(graph, batch.BranchID, tranSite.BranchID, account.AccountCD);
			if (mapto == null)
			{
				mapto = PXSelectReadonly<BranchAcctMapTo, Where<BranchAcctMapTo.toBranchID, Equal<Required<Batch.branchID>>, And<BranchAcctMapTo.fromBranchID, IsNull, And<Required<Account.accountCD>, Between<BranchAcctMapTo.fromAccountCD, BranchAcctMapTo.toAccountCD>>>>>.Select(graph, batch.BranchID, account.AccountCD);

				if (mapto == null || mapto.MapSubID == null)
				{
					return false;
				}
			}
			return true;
		}

        protected virtual void INTran_ReasonCodeID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }
	}

    /// <summary>
    /// Summarize transaction data by production order/transaction/lot serial number
    /// </summary>
    [Serializable]
    [PXProjection(typeof(Select5<AMMTranSplit, 
        InnerJoin<AMMTran, On<AMMTran.docType, Equal<AMMTranSplit.docType>,
            And<AMMTran.batNbr, Equal<AMMTranSplit.batNbr>,
                And<AMMTran.lineNbr, Equal<AMMTranSplit.lineNbr>>>>>,
        Where<AMMTran.lastOper, Equal<True>,
            And<Where<AMMTranSplit.docType, Equal<AMDocType.move>,
                Or<AMMTranSplit.docType, Equal<AMDocType.labor>>>>>,
        Aggregate<
            GroupBy<AMMTran.orderType,
            GroupBy<AMMTran.prodOrdID,
            GroupBy<AMMTranSplit.docType,
            GroupBy<AMMTranSplit.batNbr,
            GroupBy<AMMTranSplit.siteID,
            GroupBy<AMMTranSplit.lotSerialNbr,
            Sum<AMMTranSplit.baseQty>>>>>>>>>), Persistent = false)]
    [PXCacheName("AM Transaction by LotSerial")]
    public class AMMTranMoveByLotSerial : IBqlTable
    {
        #region DocType (AMMTranSplit)

        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        protected String _DocType;
        [PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(AMMTranSplit.docType))]
        public virtual String DocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }
        #endregion
        #region BatNbr (AMMTranSplit)

        public abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }

        protected String _BatNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(AMMTranSplit.batNbr))]
        public virtual String BatNbr
        {
            get
            {
                return this._BatNbr;
            }
            set
            {
                this._BatNbr = value;
            }
        }
        #endregion
        #region SiteID (AMMTranSplit)

        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site(IsKey = true, BqlField = typeof(AMMTranSplit.siteID))]
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
        #region LotSerialNbr (AMMTranSplit)

        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected String _LotSerialNbr;
        [AMLotSerialNbr(typeof(AMMTranSplit.inventoryID), typeof(AMMTranSplit.subItemID),
            typeof(AMMTranSplit.locationID), typeof(AMMTran.lotSerialNbr), FieldClass = "LotSerial", IsKey = true, BqlField = typeof(AMMTranSplit.lotSerialNbr))]
        public virtual String LotSerialNbr
        {
            get
            {
                return this._LotSerialNbr;
            }
            set
            {
                this._LotSerialNbr = value;
            }
        }
        #endregion
        #region BaseQty (AMMTranSplit) SUM

        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        protected Decimal? _BaseQty;
        /// <summary>
        /// SUM of AMMTranSplit.BaseQty
        /// </summary>
        [PXDBQuantity(BqlField = typeof(AMMTranSplit.baseQty))]
        [PXUIField(DisplayName = "Base Quantity")]
        public virtual Decimal? BaseQty
        {
            get
            {
                return this._BaseQty;
            }
            set
            {
                this._BaseQty = value;
            }
        }
        #endregion
        #region OrderType (AMMTran)
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, BqlField = typeof(AMMTran.orderType))]
        [AMOrderTypeSelector]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region ProdOrdID (AMMTran)
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr(IsKey = true, BqlField = typeof(AMMTran.prodOrdID))]
        [ProductionOrderSelector(typeof(AMMTranMoveByLotSerial.orderType))]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion
    }
}
