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
using PX.Objects.AM.GraphExtensions;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.AM
{
    /// <summary>
    /// Graph used for building the production order details (copy Bom detail to the order)
    /// </summary>
    public class ProductionBomCopy : ProductionBomCopyBase
    {
        public virtual void CreateProductionDetails(AMProdItem amProdItem)
        {
            if (amProdItem == null)
            {
                return;
            }

            try
            {
                var order = ConvertToOrder(amProdItem);

                Order.Current = order;

                //Delete existing production detail records
                DeleteProductionDetail(amProdItem);

                SetCurrentProdItem(amProdItem);
                SetCurrentProdItemDescription();

                CacheLoadProdAttributes();

                CreateOperationDetail(order);

                CopyBomsToProductionOrder();
            }
            catch (Exception e)
            {
                PXTrace.WriteError(e.InnerException ?? e);

                throw new PXException(Messages.ErrorInCopyBomProcess, e.Message);
            }
        }

        protected virtual void CopyBomsToProductionOrder()
        {
            var excludedOperDetail = new List<OperationDetail>();
            foreach (var operationDetail in _operationDetails.OrderedList)
            {
                if (!operationDetail.IncludeOper)
                {
                    excludedOperDetail.Add(operationDetail);
                    continue;
                }

                CopyBomsToProductionOrder(operationDetail);
            }

            foreach (var operationDetail in excludedOperDetail)
            {
                //loading phantom data
                CopyBomsToProductionOrder(operationDetail);
            }

            UpdatePlannedOperationTotals();

            //Line Counters are screwy if we try to do before first operation insert. Cache has 2 updates to AMPRodItem (one with ProdOrdID number with no trailing spaces, one with trailing spaces)
            CopyBomOrderLevelAttributes(CurrentProdItem);
            CopyOrderTypeAttributes(CurrentProdItem);
        }

        protected virtual void CopyBomsToProductionOrder(OperationDetail operationDetail)
        {
            if (operationDetail == null)
            {
                return;
            }

            var oper = CopyOper(operationDetail);
            if (oper == null)
            {
                // need to correctly set current operation based on phantom parent order operation
                oper = FindCachedOperByCD(CurrentProdItem?.OrderType, CurrentProdItem?.ProdOrdID, operationDetail.ProdOperationCD);
            }

            if (oper != null)
            {
                //It is possible to have oper be null but we need to continue because of phantom routings and excluded operations.
                // OperationDetails will load in correct order so first operation is loaded then followed by all excludes until the next Include Operation

                // Must set the current so the line counters are in sync
                ProcessingGraph.Caches<AMProdOper>().Current = oper;
            }

            CopyMatl(operationDetail);
            CopyOvhd(operationDetail);
            CopyTool(operationDetail);
            CopyStep(operationDetail);
            CopyBomOperationLevelAttributes(CurrentProdOper?.OperationID, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID);
        }

        /// <summary>
        /// Copy the given BOM Level Attributes to the given production item record
        /// </summary>
        /// <param name="amProdItem"></param>
        public virtual void CopyBomOrderLevelAttributes(AMProdItem amProdItem)
        {
            if (amProdItem == null
                || string.IsNullOrWhiteSpace(amProdItem.OrderType)
                || string.IsNullOrWhiteSpace(amProdItem.BOMID))
            {
                return;
            }

            AMOrderType orderType = CurrentOrderType;
            if (orderType == null)
            {
                return;
            }

            foreach (AMBomAttribute bomAttribute in PXSelect<AMBomAttribute, 
                    Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
                    And2<Where<AMBomAttribute.orderFunction, Equal<Required<AMOrderType.function>>,
                        Or<AMBomAttribute.orderFunction, Equal<OrderTypeFunction.all>>>,
                    And<AMBomAttribute.level, Equal<AMAttributeLevels.bOM>>
                    >>>.Select(ProcessingGraph, amProdItem.BOMID, orderType.Function))
            {
                var newProdAttribute = ProductionBomCopyMap.CopyAttributes(bomAttribute);
                if (newProdAttribute == null
                    || string.IsNullOrWhiteSpace(newProdAttribute.Label))
                {
                    continue;
                }
                newProdAttribute.OrderType = CurrentProdItem?.OrderType;
                newProdAttribute.ProdOrdID = CurrentProdItem?.ProdOrdID;
                TryInsertAMProdAttribute(newProdAttribute);
            }
        }

        protected virtual void CopyBomOperationLevelAttributes(int? newOperationId, string fromBomId, string fromRevisionId, int? fromOperationId)
        {
            if (newOperationId == null)
            {
                throw new ArgumentNullException(nameof(newOperationId));
            }

            AMOrderType orderType = CurrentOrderType;
            if (orderType == null)
            {
                return;
            }

            foreach (AMBomAttribute amBomAttribute in PXSelect<AMBomAttribute, 
                    Where<AMBomAttribute.bOMID, Equal<Required<AMBomAttribute.bOMID>>,
                        And<AMBomAttribute.revisionID, Equal<Required<AMBomAttribute.revisionID>>,
                    And<AMBomAttribute.operationID, Equal<Required<AMBomAttribute.operationID>>,
                    And2<Where<AMBomAttribute.orderFunction, Equal<Required<AMOrderType.function>>,
                        Or<AMBomAttribute.orderFunction, Equal<OrderTypeFunction.all>>>,
                    And<AMBomAttribute.level, Equal<AMAttributeLevels.operation>>>
                    >>>>.Select(ProcessingGraph, fromBomId, fromRevisionId, fromOperationId, orderType.Function)) 
                        
            {
                var newProdAttribute = ProductionBomCopyMap.CopyAttributes(amBomAttribute);
                if (string.IsNullOrWhiteSpace(newProdAttribute?.Label))
                {
                    continue;
                }
                newProdAttribute.OrderType = CurrentProdItem?.OrderType;
                newProdAttribute.ProdOrdID = CurrentProdItem?.ProdOrdID;
                newProdAttribute.OperationID = newOperationId;

                TryInsertAMProdAttribute(newProdAttribute);
            }
        }

        protected virtual void CopyStep(OperationDetail operationDetail)
        {
            foreach (AMBomStep amBomStep in PXSelect<
                AMBomStep, 
                Where<AMBomStep.bOMID, Equal<Required<AMBomStep.bOMID>>,
                    And<AMBomStep.revisionID, Equal<Required<AMBomStep.revisionID>>,
                    And<AMBomStep.operationID, Equal<Required<AMBomStep.operationID>>>>>>
                .Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID))
            {
                var newProdStep = ProductionBomCopyMap.CopyStep(amBomStep);

                if (!operationDetail.IsProdBom)
                {
                    SetPhtmMatlReferences(ref newProdStep, operationDetail);
                }

                // Inserting and then updating the prod Step records is necessary to copy the notes and files
                // Without inserting and updating the record, the insert fails
                var prodStep = ProcessingGraph.Caches<AMProdStep>().Insert(newProdStep);

                if (CurrentOrderType?.CopyNotesStep == true && prodStep != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMBomStep>(), amBomStep, ProcessingGraph.Caches<AMProdStep>(), prodStep);
                    ProcessingGraph.Caches<AMProdStep>().Update(prodStep);
                }
            }
        }

        protected virtual void CopyTool(OperationDetail operationDetail)
        {
            foreach (PXResult<AMBomTool,  AMBomToolCury> result in SelectFrom<AMBomTool>
				.LeftJoin<AMBomToolCury>.On<AMBomTool.bOMID.IsEqual<AMBomToolCury.bOMID>
					.And<AMBomTool.revisionID.IsEqual<AMBomToolCury.revisionID>>
					.And<AMBomTool.operationID.IsEqual<AMBomToolCury.operationID>>
					.And<AMBomTool.lineID.IsEqual<AMBomToolCury.lineID>>
					.And<AMBomToolCury.curyID.IsEqual<AccessInfo.baseCuryID.FromCurrent>>>
				.Where<AMBomTool.bOMID.IsEqual<@P.AsString>
					.And<AMBomTool.revisionID.IsEqual<@P.AsString>
					.And<AMBomTool.operationID.IsEqual<@P.AsInt>>>>
                .View.Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID))
            {
				var amBomTool = (AMBomTool)result;
				var toolCury = (AMBomToolCury)result;
                var newProdTool = ProductionBomCopyMap.CopyTool(amBomTool);

				newProdTool.UnitCost = toolCury?.UnitCost ?? 0;

                if (!operationDetail.IsProdBom)
                {
                    SetPhtmMatlReferences(ref newProdTool, operationDetail);
                }

                // Inserting and then updating the the prod tool records is necessary to copy the notes and files
                // Without inserting and updating the record, the insert fails
                var prodTool = (AMProdTool)ProcessingGraph.Caches<AMProdTool>().Insert(newProdTool);
                if (CurrentOrderType?.CopyNotesTool == true && prodTool != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMBomTool>(), amBomTool, ProcessingGraph.Caches<AMProdTool>(), prodTool);
                    ProcessingGraph.Caches<AMProdTool>().Update(prodTool);
                }
            }
        }

        protected virtual void CopyOvhd(OperationDetail operationDetail)
        {
            foreach (AMBomOvhd amBomOvhd in PXSelectJoin<
                AMBomOvhd,
                InnerJoin<AMOverhead, 
                    On<AMBomOvhd.ovhdID, Equal<AMOverhead.ovhdID>>>,
                Where<AMBomOvhd.bOMID, Equal<Required<AMBomOvhd.bOMID>>,
                    And<AMBomOvhd.revisionID, Equal<Required<AMBomOvhd.revisionID>>,
                    And<AMBomOvhd.operationID, Equal<Required<AMBomOvhd.operationID>>>>>>
                .Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID))
            {
                var newProdOvhd = ProductionBomCopyMap.CopyOvhd(amBomOvhd);
                newProdOvhd.WCFlag = false;

                if (!operationDetail.IsProdBom)
                {
                    SetPhtmMatlReferences(ref newProdOvhd, operationDetail);
                }

                // Inserting and then updating the the prod overhead records is necessary to copy the notes and files
                // Without inserting and updating the record, the insert fails
                var prodovhd = (AMProdOvhd)ProcessingGraph.Caches<AMProdOvhd>().Insert(newProdOvhd);
                if (CurrentOrderType?.CopyNotesOvhd == true && prodovhd != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMBomOvhd>(), amBomOvhd, ProcessingGraph.Caches<AMProdOvhd>(), prodovhd);
                    ProcessingGraph.Caches<AMProdOvhd>().Update(prodovhd);
                }
            }

            if (operationDetail.IncludeOper)
            {
                //Copy workcenter overheads to production order
                CopyWorkCenterOverheads(operationDetail);
            }
        }

        protected virtual void CopyMatl(OperationDetail operationDetail)
        {
            var costRoll = CreateInstance<BOMCostRoll>();
            foreach (PXResult<AMBomMatl, InventoryItem, INLotSerClass, AMBomMatlCury> result in SelectFrom<AMBomMatl>
				.InnerJoin<InventoryItem>.On<AMBomMatl.inventoryID.IsEqual<InventoryItem.inventoryID>>
				.LeftJoin<INLotSerClass>.On<InventoryItem.lotSerClassID.IsEqual<INLotSerClass.lotSerClassID>>
				.LeftJoin<AMBomMatlCury>.On<AMBomMatlCury.bOMID.IsEqual<AMBomMatl.bOMID>
					.And<AMBomMatlCury.revisionID.IsEqual<AMBomMatl.revisionID>
					.And<AMBomMatlCury.operationID.IsEqual<AMBomMatl.operationID>
					.And<AMBomMatlCury.lineID.IsEqual<AMBomMatl.lineID>
					.And<AMBomMatlCury.curyID.IsEqual<AccessInfo.baseCuryID.FromCurrent>>>>>>
				.Where<AMBomMatl.bOMID.IsEqual<@P.AsString>.And<AMBomMatl.revisionID.IsEqual<@P.AsString>
					.And<AMBomMatl.operationID.IsEqual<@P.AsInt>
					.And<AMBomMatl.materialType.IsNotEqual<AMMaterialType.phantom>>>>>
				.OrderBy<Asc<AMBomMatl.sortOrder>, Asc<AMBomMatl.lineID>>
                .View.Select(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID))
            {
                var amBomMatl = (AMBomMatl)result;
                var inventoryItem = (InventoryItem)result;
                var matlLotSerialClass = (INLotSerClass)result;
				var matlCury = (AMBomMatlCury)result;
                var inventoryItemExt = inventoryItem?.GetExtension<InventoryItemExt>();

                if (inventoryItem?.InventoryID == null || SkipMaterial(amBomMatl))
                {
                    continue; 
                }

                var newProdMatl = ProductionBomCopyMap.CopyMatl(amBomMatl);
                newProdMatl.QtyRoundUp = inventoryItemExt?.AMQtyRoundUp ?? false;

                var prodItem = (AMProdItem)ProcessingGraph.Caches<AMProdItem>().Current;
                var splits = ProcessingGraph.Caches<AMProdItemSplit>().Cached.RowCast<AMProdItemSplit>();
                //check that bflush is allowed in preassign lot/ serial scenario
                if (!(prodItem.Function != OrderTypeFunction.Regular ||
                    prodItem.PreassignLotSerial == false ||
                    prodItem.ParentLotSerialRequired != ParentLotSerialAssignment.OnIssue ||
                    (prodItem.ParentLotSerialRequired == ParentLotSerialAssignment.OnIssue &&
                        splits.Count() == 1 &&
                        !string.IsNullOrEmpty(splits.FirstOrDefault()?.LotSerialNbr)) ||
                        matlLotSerialClass.LotSerTrack == INLotSerTrack.NotNumbered))
                {
                    newProdMatl.BFlush = false;
                }

                newProdMatl = (AMProdMatl)ProcessingGraph.Caches<AMProdMatl>().Insert(newProdMatl);

                if (newProdMatl == null)
                {
                    PXTrace.WriteWarning(Messages.GetLocal(Messages.UnableToInsertProdMatlFromBom, inventoryItem?.InventoryCD, amBomMatl.BOMID, amBomMatl.OperationID, amBomMatl.LineID));
                    continue;
                }

                //Copy all to show the sources for non phantoms to so users can perform reporting on the 
                //  production details specifically relating back to the location on the source BOM.
                //  This is useful as the operation numbers are not necessary the same on the production order as they
                //  might be on the BOM due to included phantom operations.
                newProdMatl.PhtmBOMID = amBomMatl.BOMID;
                newProdMatl.PhtmBOMRevisionID = amBomMatl.RevisionID;
                newProdMatl.PhtmBOMLineRef = amBomMatl.LineID;
                newProdMatl.PhtmBOMOperationID = amBomMatl.OperationID;
                newProdMatl.PhtmLevel = 0;
				newProdMatl.UnitCost = matlCury?.UnitCost ?? 0;
				newProdMatl.SiteID = matlCury?.SiteID ?? prodItem.SiteID;
				newProdMatl.WarehouseOverride = matlCury?.SiteID != null;
				newProdMatl.LocationID = matlCury?.LocationID;

                if (newProdMatl.IsByproduct.GetValueOrDefault() && CurrentProdItem != null && CurrentProdItem?.Function == OrderTypeFunction.Disassemble)
                {
                    // By products not supported on disassemble order types
                    continue;
                }

                if (!operationDetail.IsProdBom)
                {
                    newProdMatl.QtyReq = newProdMatl.QtyReq * operationDetail.BomQtyReq;
                    SetPhtmMatlReferences(ref newProdMatl, operationDetail);
                }

                newProdMatl.SortOrder = newProdMatl.LineID;

                // Inserting and then updating the the prod matl records is necessary to copy the notes and files
                // Without inserting and updating the record, the insert fails
                newProdMatl = (AMProdMatl)ProcessingGraph.Caches<AMProdMatl>().Update(newProdMatl);
                if (CurrentOrderType?.CopyNotesMatl == true && newProdMatl != null)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMBomMatl>(), amBomMatl, ProcessingGraph.Caches<AMProdMatl>(), newProdMatl);
                }
                newProdMatl = (AMProdMatl)ProcessingGraph.Caches<AMProdMatl>().Update(newProdMatl);
                BOMCostRoll.UpdatePlannedMaterialCost(ProcessingGraph, costRoll, CurrentProdItem, newProdMatl, result);
            }
        }

        protected virtual AMProdOper CopyOper(OperationDetail operationDetail)
        {
            if (!operationDetail.IncludeOper)
            {
                return null;
            }

			var amBomOper = AMBomOper.PK.Find(ProcessingGraph, operationDetail.BomID, operationDetail.BomRevisionID, operationDetail.BomOperationID);
            if (amBomOper == null)
            {
                return null;
            }

            try
            {
				var bomOperCury = AMBomOperCury.PK.Find(ProcessingGraph, amBomOper.BOMID, amBomOper.RevisionID, amBomOper.OperationID, Accessinfo.BaseCuryID);
				var oper = FindCachedOperByCD(CurrentProdItem?.OrderType, CurrentProdItem?.ProdOrdID, operationDetail.ProdOperationCD);
				if (oper?.OperationCD != null)
				{
					var cacheStatus = ProcessingGraph.Caches<AMProdOper>().GetStatus(oper);
					if (cacheStatus == PXEntryStatus.Deleted || cacheStatus == PXEntryStatus.InsertedDeleted)
					{
						// Rebuilding order
						ProcessingGraph.Caches<AMProdOper>().SetStatus(oper, cacheStatus == PXEntryStatus.InsertedDeleted ? PXEntryStatus.Inserted : PXEntryStatus.Updated);
					}
				}
				if (oper?.OperationCD == null)
				{
					oper = (AMProdOper)ProcessingGraph.Caches<AMProdOper>().Insert(new AMProdOper { OperationCD = operationDetail.ProdOperationCD });
				}


				var newProdOper = ProductionBomCopyMap.CopyOper(amBomOper, oper);
				
				newProdOper.VendorID = bomOperCury?.VendorID;
				newProdOper.VendorLocationID = bomOperCury?.VendorLocationID;


				newProdOper.TotalQty = CurrentProdItem?.QtytoProd.GetValueOrDefault();
                newProdOper.BaseTotalQty = CurrentProdItem?.BaseQtytoProd.GetValueOrDefault();
                newProdOper.BaseQtytoProd = 0m;
                newProdOper.WcID = operationDetail.WcID;
                newProdOper.BFlush = operationDetail.WcBFlushLabor.GetValueOrDefault();
                newProdOper.Descr = operationDetail.WcDesc;

                newProdOper = (AMProdOper)ProcessingGraph.Caches<AMProdOper>().Update(newProdOper);

#if DEBUG
                AMDebug.TraceWriteMethodName(newProdOper?.DebuggerDisplay);
#endif
                if (CurrentOrderType?.CopyNotesOper == true)
                {
                    PXNoteAttribute.CopyNoteAndFiles(ProcessingGraph.Caches<AMBomOper>(), amBomOper, ProcessingGraph.Caches<AMProdOper>(), newProdOper);
                }

                if (!operationDetail.IsProdBom)
                {
                    SetPhtmMatlReferences(ref newProdOper, operationDetail);
                    newProdOper.PhtmPriorLevelQty = operationDetail.BomQtyReq;
                }

                return (AMProdOper)ProcessingGraph.Caches<AMProdOper>().Update(newProdOper);
            }

            catch (Exception e)
            {
                throw new PXException(e, Messages.GetLocal(Messages.ErrorInsertingProductionOperation, 
                    Messages.GetLocal(Messages.BOM),
                    operationDetail?.ProdOperationCD,
                    amBomOper?.RevisionID,
                    amBomOper?.BOMID,
                    amBomOper?.OperationCD,
                    amBomOper?.Descr.TrimIfNotNullEmpty(),
                    amBomOper?.WcID,
                    e.Message));
            }
        }
    }
}
