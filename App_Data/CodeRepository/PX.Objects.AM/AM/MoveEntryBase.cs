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
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;
using PX.Objects.GL;

namespace PX.Objects.AM
{
    /// <summary>
    /// Base class for related move transactions
    /// </summary>
    public abstract class MoveEntryBase<TWhere> : AMBatchEntryBase
        where TWhere : class, IBqlWhere, new()
    {		

        public PXSelectJoin<AMBatch,
			InnerJoinSingleTable<Branch, On<AMBatch.branchID, Equal<Branch.branchID>>>,
			Where2<TWhere, And<Branch.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>>>> batch;
        [PXImport(typeof(AMBatch))]
        public PXSelect<AMMTran, Where<AMMTran.docType, Equal<Current<AMBatch.docType>>, And<AMMTran.batNbr, Equal<Current<AMBatch.batNbr>>>>> transactions;

        [PXCopyPasteHiddenView]
        public PXSelect<AMMTranSplit, Where<AMMTranSplit.docType, Equal<Current<AMMTran.docType>>, And<AMMTranSplit.batNbr, Equal<Current<AMMTran.batNbr>>, And<AMMTranSplit.lineNbr, Equal<Current<AMMTran.lineNbr>>>>>> splits;

        [PXCopyPasteHiddenView]
        public PXSelect<AMMTranAttribute, Where<AMMTranAttribute.docType, Equal<Current<AMMTran.docType>>,
                    And<AMMTranAttribute.batNbr, Equal<Current<AMMTran.batNbr>>, And<AMMTranAttribute.tranLineNbr,
                        Equal<Current<AMMTran.lineNbr>>>>>> TransactionAttributes;

        protected bool _skipReleasedReferenceDocsCheck = false;

        protected abstract void AMMTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e);

        public override void InitCacheMapping(Dictionary<Type, Type> map)
        {
            base.InitCacheMapping(map);

            this.Caches.AddCacheMapping(typeof(INLotSerialStatusByCostCenter), typeof(INLotSerialStatusByCostCenter));
        }

        #region Buttons
        public PXAction<AMBatch> release;
        [PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton]
        public virtual IEnumerable Release(PXAdapter adapter)
        {
            PXCache cache = batch.Cache;
            List<AMBatch> list = new List<AMBatch>();
            foreach (AMBatch amdoc in adapter.Get<AMBatch>())
            {
                if (amdoc.Hold == false && amdoc.Released == false)
                {
                    cache.Update(amdoc);
                    list.Add(amdoc);
                }
            }
            if (list.Count == 0)
            {
                throw new PXException(PX.Objects.IN.Messages.Document_Status_Invalid);
            }

            if (IsDirty)
            {
                Save.Press();
            }

            PXLongOperation.StartOperation(this, delegate () { AMDocumentRelease.ReleaseDoc(list); });
            return list;
        }

        public PXAction<AMBatch> lateAssignmentEntry;
        [PXUIField(DisplayName = "Late Assignment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable LateAssignmentEntry(PXAdapter adapter)
        {
            var currentTran = transactions.Current;
            if(currentTran == null || string.IsNullOrWhiteSpace(currentTran.ProdOrdID))
            {
                return adapter.Get();
            }

			LateAssignmentMaint.Redirect(currentTran.OrderType, currentTran.ProdOrdID, currentTran.ParentLotSerialNbr);

            return adapter.Get();
        }
        #endregion

        public override void Persist()
        {
                try
                {
                    ValidateLotSerialInformation();

                    if (ReferenceDeleteGraph.ContainsDeletes<AMBatch, AMMTran>(this))
                    {
                        using (var ts = new PXTransactionScope())
                        {
                        ReferenceDeleteGraph.DeleteReferenceTransactions(this);
                            BasePersist();
                            ts.Complete();
                        }

                        return;
                    }

                    BasePersist();
                }
                catch (Exception e)
                {
                    PXTraceHelper.PxTraceException(e);
                    throw;
                }
            }

        protected virtual void ValidateLotSerialInformation()
        {
            //first we sum the quantities being received for the same type/order/lotserial combination, then make sure it doesn't exceed the split
            var prodLotSerialTotals = new List<ProdTranLotSerialTotals>();
            var prodList = new HashSet<string>();
            foreach(AMMTran tran in transactions.Select())
			{
				if (tran?.IsLotSerialPreassigned != true || tran?.Qty == 0)
				{
					continue;
				}

				var hasLastOperLotSerial = false;
				foreach (AMMTranSplit split in splits.Cache.Cached.RowCast<AMMTranSplit>()
					.Where(x => x.DocType == tran.DocType && x.BatNbr == tran.BatNbr && x.LineNbr == tran.LineNbr && !splits.Cache.GetStatus(x).IsDeleted()))
                    {
					if (split?.BaseQty == null)
					{
						continue;
					}

					var lineQty = Math.Abs(split.BaseQty.GetValueOrDefault()) * tran.InvtMult.GetValueOrDefault();

					var totalIdx = prodLotSerialTotals.FindIndex(x => x.OrderType == tran.OrderType && x.ProdOrdID == tran.ProdOrdID && string.Equals(x.LotSerNbr, split.LotSerialNbr, StringComparison.OrdinalIgnoreCase));
                        if(totalIdx != -1)
                        {
						prodLotSerialTotals[totalIdx].Qty += lineQty;
                        }
                        else
                        {
						prodLotSerialTotals.Add(new ProdTranLotSerialTotals
						{
                                OrderType = tran.OrderType,
                                ProdOrdID = tran.ProdOrdID,
                                LotSerNbr = split.LotSerialNbr,
							Qty = lineQty
                            });
                        }

					hasLastOperLotSerial = tran?.LastOper == true && tran?.IsScrap != true;
                    }

				// Exclude scrap trans
				if(hasLastOperLotSerial)
				{
					prodList.Add(string.Join("~", tran.OrderType, tran.ProdOrdID));
                }				
            }

			ValidateParentLotSerialInformation(prodList.ToList());

			foreach (var total in prodLotSerialTotals)
            {
                var itemSplit = (AMProdItemSplit)PXSelect<AMProdItemSplit, Where<AMProdItemSplit.orderType, Equal<Required<AMProdItemSplit.orderType>>,
                    And<AMProdItemSplit.prodOrdID, Equal<Required<AMProdItemSplit.prodOrdID>>,
                    And<AMProdItemSplit.lotSerialNbr, Equal<Required<AMProdItemSplit.lotSerialNbr>>>>>>.Select(this, total.OrderType, total.ProdOrdID, total.LotSerNbr);
                if(itemSplit == null)
                {
                    throw new Exception(Messages.GetLocal(Messages.PreassignedLotSerialNumberDoesNotExist, total.OrderType, total.ProdOrdID, total.LotSerNbr));
                }

				if(itemSplit.QtyRemaining.GetValueOrDefault() < total.Qty)
                {
                    throw new Exception(Messages.GetLocal(Messages.ExceededPreassignedLotSerialQuantity, total.OrderType, total.ProdOrdID, total.LotSerNbr));
                }
            }
        }

        protected virtual void ValidateParentLotSerialInformation(List<string> prodList)
        {
            foreach(var key in prodList)
            {
                var prodOrder = key.Split('~');
                var results = PXSelectJoin<AMProdItem, 
                    LeftJoin<AMProdMatlLotSerial,
                        On<AMProdItem.orderType, Equal<AMProdMatlLotSerial.orderType>,
                            And<AMProdItem.prodOrdID, Equal<AMProdMatlLotSerial.prodOrdID>>>>, 
                    Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>.Select(this, prodOrder[0], prodOrder[1]);
                foreach(PXResult<AMProdItem, AMProdMatlLotSerial>result in results)
                {
                    var prod = (AMProdItem)result;
                    var prodLotSer = (AMProdMatlLotSerial)result; 
                    if(prod != null && prod.ParentLotSerialRequired == ParentLotSerialAssignment.OnCompletion 
                        && prodLotSer?.LotSerialNbr != null && string.IsNullOrWhiteSpace(prodLotSer.ParentLotSerialNbr) && prodLotSer?.QtyIssued > 0)
                    {
                        throw new Exception(Messages.GetLocal(Messages.MaterialLotSerialNotAssigned, prod.OrderType, prod.ProdOrdID));
                    }
                }
            }
        }

        protected virtual void BasePersist()
        {
            base.Persist();
        }

        protected virtual void AMBatch_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Move;
        }

        protected virtual void AMMTran_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Move;
        }

        protected virtual void AMMTranAttribute_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Move;
        }


        [PXDefault(AMTranType.Receipt)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<AMMTran.tranType> e)
        {
        }

        [PXDefault(typeof(Search<AMProdOper.scrapAction,
            Where<AMProdOper.orderType, Equal<Current<AMMTran.orderType>>,
            And<AMProdOper.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<AMMTran.scrapAction> e) { }

        [PXFormula(typeof(Default<AMMTran.operationID>))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<AMMTran.isLotSerialPreassigned> e) { }

        [AMMoveLotSerialNbr(typeof(AMMTran.orderType), typeof(AMMTran.prodOrdID), 
            typeof(AMMTran.inventoryID), typeof(AMMTran.subItemID), typeof(AMMTran.locationID),    
            PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void _(Events.CacheAttached<AMMTran.lotSerialNbr> e) { }

        [AMMoveLotSerialNbr(typeof(AMMTran.orderType), typeof(AMMTran.prodOrdID),
            typeof(AMMTran.inventoryID), typeof(AMMTran.subItemID), typeof(AMMTran.locationID),
            PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void _(Events.CacheAttached<AMMTranSplit.lotSerialNbr> e) { }

        protected virtual void AMMTran_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMTranType.InvtMult(((AMMTran)e.Row).TranType, ((AMMTran)e.Row).Qty);
        }

        protected virtual void AMMTranSplit_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Move;
        }

        protected virtual void AMBatch_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMBatch)e.Row;
            if (row == null || sender.GetStatus(row) == PXEntryStatus.InsertedDeleted)
            {
                return;
            }

            _skipReleasedReferenceDocsCheck = true;

            if (ReferenceDeleteGraph.HasReleasedReferenceDocs(this, row, true))
            {
                throw new PXException(Messages.ReleasedBatchExist);
            }
        }

        protected virtual void AMBatch_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (ampsetup.Current.RequireControlTotal.GetValueOrDefault() == false)
            {
                if (PXCurrencyAttribute.IsNullOrEmpty(((AMBatch)e.Row).TotalAmount) == false)
                {
                    sender.SetValue<AMBatch.controlAmount>(e.Row, ((AMBatch)e.Row).TotalAmount);
                }
                else
                {
                    sender.SetValue<AMBatch.controlAmount>(e.Row, 0m);
                }

                if (PXCurrencyAttribute.IsNullOrEmpty(((AMBatch)e.Row).TotalQty) == false)
                {
                    sender.SetValue<AMBatch.controlQty>(e.Row, ((AMBatch)e.Row).TotalQty);
                }
                else
                {
                    sender.SetValue<AMBatch.controlQty>(e.Row, 0m);
                }
            }

            if (((AMBatch)e.Row).Hold == false && ((AMBatch)e.Row).Released == false)
            {
                if (ampsetup.Current.RequireControlTotal.GetValueOrDefault())
                {
                    if (((AMBatch)e.Row).TotalQty != ((AMBatch)e.Row).ControlQty)
                    {
                        sender.RaiseExceptionHandling<AMBatch.controlQty>(e.Row, ((AMBatch)e.Row).ControlQty, new PXSetPropertyException(PX.Objects.IN.Messages.DocumentOutOfBalance));
                    }
                    else
                    {
                        sender.RaiseExceptionHandling<AMBatch.controlQty>(e.Row, ((AMBatch)e.Row).ControlQty, null);
                    }
                }
            }

            if (!sender.ObjectsEqual<AMMTran.tranDate>(e.Row, e.OldRow))
            {
                foreach (var tran in PXParentAttribute.SelectChildren(transactions.Cache, e.Row, typeof(AMBatch)))
                {
                    transactions.Cache.MarkUpdated(tran);
                }
            }
        }

        protected virtual void AMBatch_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            bool editablebatch = ((AMBatch)e.Row).EditableBatch == true;

            release.SetEnabled(e.Row != null && ((AMBatch)e.Row).Hold == false && editablebatch);

            sender.AllowInsert = true;
            sender.AllowUpdate = 
                sender.AllowDelete = 
                    transactions.Cache.AllowInsert = 
                        transactions.Cache.AllowUpdate = 
                            transactions.Cache.AllowDelete = 
                                TransactionAttributes.Cache.AllowUpdate = editablebatch;


            //Supporting transaction attributes from API calls. Users can provide the correct attributes and the graph will manage them correctly based on the expected production attributes.
            //  To allow the calling app the chance to set the tran attributes, we will make the view and fields updatable only to this type of call to this graph
            TransactionAttributes.AllowInsert = editablebatch && (IsImport || IsContractBasedAPI);
            TransactionAttributes.AllowDelete = editablebatch && (IsImport || IsContractBasedAPI);
            PXUIFieldAttribute.SetEnabled<AMMTranAttribute.label>(TransactionAttributes.Cache, null, editablebatch && (IsImport || IsContractBasedAPI));

            PXUIFieldAttribute.SetVisible<AMBatch.controlQty>(sender, e.Row, ampsetup.Current.RequireControlTotal.GetValueOrDefault());
            PXUIFieldAttribute.SetVisible<AMBatch.controlAmount>(sender, e.Row, ampsetup.Current.RequireControlTotal.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<AMBatch.status>(sender, e.Row, false);
            PXUIFieldAttribute.SetEnabled<AMBatch.hold>(sender, e.Row, editablebatch);
            PXUIFieldAttribute.SetEnabled<AMBatch.finPeriodID>(sender, e.Row, editablebatch);
            PXUIFieldAttribute.SetEnabled<AMBatch.controlQty>(sender, e.Row, ampsetup.Current.RequireControlTotal.GetValueOrDefault() && editablebatch);
            PXUIFieldAttribute.SetEnabled<AMBatch.controlAmount>(sender, e.Row, ampsetup.Current.RequireControlTotal.GetValueOrDefault() && editablebatch);

            _skipReleasedReferenceDocsCheck = false;
        }

        #region AMMTran Events
        #region Row Events
        protected virtual void AMMTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null)
            {
                return;
            }

            var isDirectLabor = (row.LaborType ?? AMLaborType.Direct) == AMLaborType.Direct;

            PXUIFieldAttribute.SetEnabled<AMMTran.inventoryID>(sender, row, false);
            PXUIFieldAttribute.SetEnabled<AMMTran.subItemID>(sender, row, false);
            PXUIFieldAttribute.SetEnabled<AMMTran.uOM>(sender, row, false);

            // Enable or Disable Scrap Action Fields
            var reasonCodeEnabled = row.ScrapAction != null && row.ScrapAction.GetValueOrDefault() != ScrapAction.NoAction;
            var isQuarantine = row.ScrapAction.GetValueOrDefault() == ScrapAction.Quarantine;

            // Check Production Order Function for Disassembly
            AMProdItem prodItem = PXSelect<AMProdItem, Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>
                >>.Select(this, row.OrderType, row.ProdOrdID);
            var isDisassembly = prodItem != null && prodItem.Function == OrderTypeFunction.Disassemble;

            PXUIFieldAttribute.SetEnabled<AMMTran.scrapAction>(sender, row, isDirectLabor && !isDisassembly);
            PXUIFieldAttribute.SetEnabled<AMMTran.reasonCodeID>(sender, row, reasonCodeEnabled && isDirectLabor && !isDisassembly);
            PXUIFieldAttribute.SetEnabled<AMMTran.isScrap>(sender, row, isQuarantine && isDirectLabor && !isDisassembly);
            PXUIFieldAttribute.SetEnabled<AMMTran.qtyScrapped>(sender, row, !isQuarantine && isDirectLabor && !isDisassembly);
            PXUIFieldAttribute.SetEnabled<AMMTran.orderType>(sender, row, isDirectLabor);
            PXUIFieldAttribute.SetEnabled<AMMTran.prodOrdID>(sender, row, isDirectLabor);
            PXUIFieldAttribute.SetEnabled<AMMTran.operationID>(sender, row, isDirectLabor);
            PXUIFieldAttribute.SetEnabled<AMMTran.qty>(sender, row, isDirectLabor && !isDisassembly);
            PXUIFieldAttribute.SetEnabled<AMMTran.siteID>(sender, row, isDirectLabor);
            PXUIFieldAttribute.SetEnabled<AMMTran.locationID>(sender, row, isDirectLabor);
            PXUIFieldAttribute.SetEnabled<AMMTran.laborCodeID>(sender, row, !isDirectLabor);

            if (row.HasReference.GetValueOrDefault())
            {
                //Disable all fields
                PXUIFieldAttribute.SetEnabled(sender, row, false);
            }
			PXUIFieldAttribute.SetEnabled<AMMTran.receiptNbr>(sender, row, row.Qty.GetValueOrDefault() < 0 && row.InventoryID != null && row.LastOper.GetValueOrDefault() && isDirectLabor);
        }

        #endregion

        #region Field Events

        protected virtual void AMMTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var ammTran = (AMMTran)e.Row;
            if (ammTran == null)
            {
                return;
            }

            if (ammTran.IsScrap == true)
            {
                ammTran.QtyScrapped = ammTran.Qty;
            }

        }

        protected virtual void AMMTran_Qty_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            var ammTran = (AMMTran)e.Row;
            var newQty = Convert.ToDecimal(e.NewValue);

            if (string.IsNullOrWhiteSpace(ammTran?.OrderType) || string.IsNullOrWhiteSpace(ammTran.ProdOrdID) || ammTran.OperationID == null || newQty >= 0 || ammTran.IsScrap.GetValueOrDefault())
            {
                return;
            }

            if (ProductionTransactionHelper.TryCheckNegativeMoveQty(sender, ammTran, newQty, 
                transactions.Select().FirstTableItems.ToList(), out var exception))
            {
                throw exception;
            }
        }

        protected virtual void TransactionChecks(PXCache sender, AMMTran row)
        {
            // When any of the checks is an error - we need to break.
            //  Without it another check could be a warning and trump the error (and just show the yellow icon on qty).

            if (CheckMoveOnCompletedOperation(sender, row))
            {
                return;
            }
            if (row.LastOper.GetValueOrDefault() && CheckOverCompletedOrders(sender, row))
            {
                return;
            }
			if (CheckOverCompletedOperation(sender, row))
			{
				return;
			}

            CheckUnderIssueMaterial(sender, row);
        }

        protected virtual void _(Events.FieldUpdating<AMMTran, AMMTran.qty> e)
        {
            if (HasMixedSigns(e.Row?.QtyScrapped ?? 0m, Convert.ToDecimal(e.NewValue ?? 0m)))
            {
                e.NewValue = e.Row?.Qty;
                e.Cancel = true;
                RaiseMixedQtySigns<AMMTran.qty, AMMTran.qtyScrapped>(e.Cache, e.Row, e.Row?.Qty);
            }
        }

        protected virtual void _(Events.FieldUpdating<AMMTran, AMMTran.qtyScrapped> e)
        {
            if (HasMixedSigns(e.Row?.Qty ?? 0m, Convert.ToDecimal(e.NewValue ?? 0m)))
            {
                e.NewValue = e.Row?.QtyScrapped;
                e.Cancel = true;
                RaiseMixedQtySigns<AMMTran.qtyScrapped, AMMTran.qty>(e.Cache, e.Row, e.Row?.QtyScrapped);
            }
        }

		private bool HasMixedSigns(decimal d1, decimal d2)
        {
            if (d1 == 0 || d2 == 0)
            {
                return false;
            }

            return (d1 < 0 && d2 > 0) || (d2 < 0 && d1 > 0);
        }

        protected virtual void RaiseMixedQtySigns<Field1, Field2>(PXCache cache, object row, object fieldValue) 
            where Field1 : IBqlField
            where Field2 : IBqlField
        {
            if (row == null)
            {
                return;
            }

            cache.RaiseExceptionHandling<Field1>(
                row,
                fieldValue,
                new PXSetPropertyException(AM.Messages.GetLocal(AM.Messages.BothValuesMustBePosOrNeg),
                    PXUIFieldAttribute.GetDisplayName<Field1>(cache), PXUIFieldAttribute.GetDisplayName<Field2>(cache))
            );
        }

        protected virtual void AMMTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (AMMTran)e.Row;

            if (string.IsNullOrWhiteSpace(row?.ProdOrdID) || e.Operation == PXDBOperation.Delete)
            {
                return;
            }

            if (row.DocType == AMDocType.Move && row.Qty.GetValueOrDefault() == 0 && row.QtyScrapped.GetValueOrDefault() == 0)
            {
                sender.RaiseExceptionHandling<AMMTran.qty>(
                    row,
                    row.Qty,
                    new PXSetPropertyException(Messages.GetLocal(Messages.FieldCannotBeZero, PXUIFieldAttribute.GetDisplayName<AMMTran.qty>(sender)),
                        PXErrorLevel.Error));
            }
            
            // Check For Reason Code Required and null
            if (row.QtyScrapped.GetValueOrDefault() != 0 && row.ScrapAction == ScrapAction.WriteOff && row.ReasonCodeID == null
                || row.IsScrap.GetValueOrDefault() && row.Qty.GetValueOrDefault() != 0 && row.ReasonCodeID == null)
            {
                throw new PXRowPersistingException(typeof(AMMTran.reasonCodeID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AMMTran.reasonCodeID).Name);
            }

            TransactionChecks(sender, row);

            if (row.Qty.GetValueOrDefault() >= 0 || !row.LastOper.GetValueOrDefault())
            {
                return;
            }

            InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<AMMTran.inventoryID>(sender, e.Row);
            INLotSerClass itemclass =
                (INLotSerClass)PXSelectorAttribute.Select<InventoryItem.lotSerClassID>(Caches[typeof(InventoryItem)], item);

            if (item != null && itemclass == null)
            {
                itemclass = PXSelect<INLotSerClass,
                    Where<INLotSerClass.lotSerClassID, Equal<Required<INLotSerClass.lotSerClassID>>>
                    >.Select(this, item.LotSerClassID);
            }

            PXPersistingCheck check =
                row.InvtMult != 0 && (
                (item != null && item.ValMethod == INValMethod.Specific) ||
                (itemclass != null &&
                 itemclass.LotSerTrack != INLotSerTrack.NotNumbered &&
                 itemclass.LotSerAssign == INLotSerAssign.WhenReceived &&
                 row.Qty != 0m))
                 ? PXPersistingCheck.NullOrBlank
                 : PXPersistingCheck.Nothing;

            PXDefaultAttribute.SetPersistingCheck<AMMTran.locationID>(sender, e.Row, PXPersistingCheck.Null);
            PXDefaultAttribute.SetPersistingCheck<AMMTran.lotSerialNbr>(sender, e.Row, check);

            if (row != null
                && row.Qty.GetValueOrDefault() < 0
                && (row.LastOper.GetValueOrDefault() || row.IsScrap.GetValueOrDefault())
                && item != null
                && item.ValMethod == INValMethod.FIFO)
            {
				if (row.ReceiptNbr == null)
            {
                throw new PXRowPersistingException(typeof(AMMTran.receiptNbr).Name, null, ErrorMessages.FieldIsEmpty, typeof(AMMTran.receiptNbr).Name);
            }

				var status = GetINCostStatus(row);
					if (status == null || status.QtyOnHand < Math.Abs((decimal)row.Qty))
					{
						throw new PXRowPersistingException(typeof(AMMTran.qty).Name, row.Qty, IN.Messages.StatusCheck_QtyNegative, item.InventoryCD, row.SubItemID,
							row.SiteID);
					}
				}
			}

		protected virtual INCostStatus GetINCostStatus(AMMTran row)
		{
			// When checking FIFO valued items, no need to include check for LotSerialNbr
			return PXSelect<INCostStatus,
					Where<INCostStatus.inventoryID, Equal<Required<INCostStatus.inventoryID>>,
						And<INCostStatus.costSiteID, Equal<Required<INCostStatus.costSiteID>>,
						And<INCostStatus.costSubItemID, Equal<Required<INCostStatus.costSubItemID>>,
						And<INCostStatus.receiptNbr, Equal<Required<INCostStatus.receiptNbr>>>>>>>
						.Select(this, row?.InventoryID, row?.SiteID, row?.SubItemID, row?.ReceiptNbr);
        }
        
        protected virtual void AMMTran_OperationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var ammTran = (AMMTran)e.Row;
            if (!string.IsNullOrWhiteSpace(ammTran?.ProdOrdID) && ammTran.OperationID != null)
            {
				var prodItem = AMProdItem.PK.Find(this, ammTran.OrderType, ammTran.ProdOrdID);

                sender.SetValueExt<AMMTran.lastOper>(ammTran, prodItem?.LastOperationID == ammTran.OperationID);

                var amProdOper = (AMProdOper)PXSelectorAttribute.Select<AMMTran.operationID>(sender, ammTran);

                sender.SetValueExt<AMMTran.scrapAction>(ammTran, amProdOper?.ScrapAction ?? Attributes.ScrapAction.NoAction);

                if (ammTran.ScrapAction.GetValueOrDefault() != ScrapAction.Quarantine)
                {
                    ammTran.IsScrap = false;
                }
                else
                {
                    ammTran.QtyScrapped = 0m;
                }

				SetIsLotSerialPreassigned(sender, ammTran, prodItem);

				ammTran.IsLotSerialPreassigned = prodItem != null && ammTran.LastOper == true && prodItem.PreassignLotSerial == true;
            }

            SyncTransactionAttributes(ammTran);
        }

		protected virtual void SetIsLotSerialPreassigned(PXCache ammTranCache, AMMTran row, AMProdItem prodItem)
		{
			if(row == null)
			{
				return;
			}
			var isPreassigned = prodItem?.PreassignLotSerial == true && (row.LastOper == true || row.IsScrap == true);
			if(row.IsLotSerialPreassigned != isPreassigned)
			{
				ammTranCache.SetValueExt<AMMTran.isLotSerialPreassigned>(row, isPreassigned);
			}
		}

        protected virtual void AMMTran_ProdOrdID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var ammTran = (AMMTran)e.Row;
            if (string.IsNullOrWhiteSpace(ammTran?.ProdOrdID))
            {
                return;
            }

            if (ampsetup.Current == null)
            {
                ampsetup.Current = PXSelect<AMPSetup>.Select(this);
            }

            var amproditem = (AMProdItem)PXSelectorAttribute.Select<AMMTran.prodOrdID>(sender, ammTran);

            if (amproditem == null)
            {
                return;
            }

            var singleOpeation = amproditem.FirstOperationID == amproditem.LastOperationID;
            var isApi = IsImport || IsContractBasedAPI;

            if(isApi && singleOpeation)
            {
                // Scenario for API when an OperationID is not passed to the screen and the LastOper/InvtMult is not set correctly. (API assumes defaulted operation)
                sender.SetValueExt<AMMTran.operationID>(ammTran, amproditem.LastOperationID);
            }

            if (!isApi)
            {
                AMProdOper oper = ampsetup.Current.InclScrap.GetValueOrDefault()
                    ? PXSelectJoin<AMProdOper,
                            InnerJoin<AMProdItem, On<AMProdOper.orderType, Equal<AMProdItem.orderType>,
                                And<AMProdOper.prodOrdID, Equal<AMProdItem.prodOrdID>>>>,
                            Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                                And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                                    And<Sub<AMProdItem.qtytoProd, Add<AMProdOper.qtyComplete, AMProdOper.qtyScrapped>>,
                                        Greater<decimal0>>>>,
                            OrderBy<Asc<AMProdOper.operationCD>>>
                        .SelectWindowed(this, 0, 1, ammTran.OrderType, ammTran.ProdOrdID)
                    : PXSelectJoin<AMProdOper,
                            InnerJoin<AMProdItem, On<AMProdOper.orderType, Equal<AMProdItem.orderType>,
                                And<AMProdOper.prodOrdID, Equal<AMProdItem.prodOrdID>>>>,
                            Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                                And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                                    And<Sub<AMProdItem.qtytoProd, AMProdOper.qtyComplete>, Greater<decimal0>>>>,
                            OrderBy<Asc<AMProdOper.operationCD>>>
                        .SelectWindowed(this, 0, 1, ammTran.OrderType, ammTran.ProdOrdID);

                if (oper?.OperationID != null)
                {
                    sender.SetValueExt<AMMTran.operationID>(ammTran, oper.OperationID);
                }
                else if (ammTran.OperationID == null)
                {
                    sender.SetValueExt<AMMTran.operationID>(ammTran, amproditem.FirstOperationID);
                }
            }

            sender.SetValueExt<AMMTran.inventoryID>(ammTran, amproditem.InventoryID);
            sender.SetValueExt<AMMTran.subItemID>(ammTran, amproditem.SubItemID);
            sender.SetValueExt<AMMTran.siteID>(ammTran, amproditem.SiteID);
            sender.SetValueExt<AMMTran.locationID>(ammTran, amproditem.LocationID);
            sender.SetValueExt<AMMTran.uOM>(ammTran, amproditem.UOM);
            sender.SetValue<AMMTran.wIPAcctID>(ammTran, amproditem.WIPAcctID);
            sender.SetValue<AMMTran.wIPSubID>(ammTran, amproditem.WIPSubID);

            SyncTransactionAttributes(ammTran);
        }

        protected virtual void AMMTran_IsScrap_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null)
            {
                return;
            }

			var amProdItem = (AMProdItem)PXSelectorAttribute.Select<AMMTran.prodOrdID>(sender, row);
            if (amProdItem?.ProdOrdID == null)
            {
                return;
            }

			var siteId = amProdItem.SiteID;
			var locationId = amProdItem.LocationID;
			var qty = 0m;
			var qtyScrap = 0m;

			if (row.IsScrap.GetValueOrDefault())
			{
				if(amProdItem.ScrapSiteID != null)
            {
					siteId = amProdItem.ScrapSiteID;
					locationId = amProdItem.ScrapLocationID;
            }

				qty = row.Qty.GetValueOrDefault();
				qtyScrap = row.QtyScrapped.GetValueOrDefault();
				if (qtyScrap == 0 && qty > 0)
				{
					qtyScrap = qty;
				}
				sender.SetValueExt<AMMTran.qty>(row, qtyScrap);
            }

			row.SiteID = siteId;
			row.LocationID = locationId;
			row.QtyScrapped = qtyScrap;

			SetIsLotSerialPreassigned(sender, row, amProdItem);
        }

        protected virtual void AMMTran_ScrapAction_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            SetScrapFields(sender, (AMMTran) e.Row);
        }
        protected virtual void AMMTran_ReasonCodeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            SetScrapFields(sender, (AMMTran)e.Row);
        }

        protected virtual void SetScrapFields(PXCache cache, AMMTran row)
        {
            if (row == null)
            {
                return;
            }

            if (row.ScrapAction == ScrapAction.Quarantine)
            {
                if (!row.IsScrap.GetValueOrDefault() && !string.IsNullOrWhiteSpace(row.ReasonCodeID))
                {
                    cache.SetValueExt<AMMTran.isScrap>(row, true);
                }
                return;
            }

			if (row.ScrapAction == ScrapAction.NoAction)
            {
                if (!string.IsNullOrWhiteSpace(row.ReasonCodeID))
                {
                    cache.SetValueExt<AMMTran.reasonCodeID>(row, null);
                }
            }

            if (row.IsScrap.GetValueOrDefault())
            {
                cache.SetValueExt<AMMTran.isScrap>(row, false);
            }
        }

        #endregion
        #endregion

        protected virtual void AMMTranAttribute_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            var row = (AMMTranAttribute) e.Row;
            var isApi = IsImport || IsContractBasedAPI;
            if (row == null || !isApi || IsCopyPasteContext)
            {
                return;
            }
#if DEBUG
            AMDebug.TraceWriteMethodName($"DocType = {row.DocType}, BatNbr = {row.BatNbr}, TranLineNbr = {row.TranLineNbr}, LineNbr = {row.LineNbr}, Label = {row.Label}, Value = {row.Value}, Order = {row.OrderType}-{row.ProdOrdID}");
            // We cannot cancel the row insert for API calls or we will end up with this message to the service call...
            //      PX.Api.ContractBased.OutcomeEntityHasErrorsException: PX.Data.PXException: Error: The system failed to commit the TransactionAttributes row.
            //e.Cancel = true; 
            //lets update the values of this row inserting to equal what is already in place for the row with the same label and remove the other row (as labels are unique)
#endif

            if (string.IsNullOrWhiteSpace(row.Label))
            {
                return;
            }

            var parentRow = (AMMTran) PXParentAttribute.SelectParent(sender, row, typeof(AMMTran));
            if (parentRow == null)
            {
                return;
            }

            var prodTranAttributes = GetProductionAttributeDictionary(parentRow);
            if (prodTranAttributes == null || !prodTranAttributes.ContainsKey(row.Label.Trim()))
            {
                throw new PXException(Messages.GetLocal(Messages.OrderAttributeNotFound, row.Label.TrimIfNotNullEmpty(), parentRow.OrderType.TrimIfNotNullEmpty(), parentRow.ProdOrdID.TrimIfNotNullEmpty()));
            }

            var cachedTranAttributes = GetTransactionAttributeDictionary(parentRow, true);
            if (!cachedTranAttributes.ContainsKey(row.Label.Trim()))
            {
                return;
            }

            var tranAttWithSameLabel = cachedTranAttributes[row.Label.Trim()];
            row.OrderType = tranAttWithSameLabel.OrderType;
            row.ProdOrdID = tranAttWithSameLabel.ProdOrdID;
            row.OperationID = tranAttWithSameLabel.OperationID;
            row.ProdAttributeLineNbr = tranAttWithSameLabel.LineNbr;
            row.AttributeID = tranAttWithSameLabel.AttributeID;
            row.Label = tranAttWithSameLabel.Label;
            row.Descr = tranAttWithSameLabel.Descr;
            row.TransactionRequired = tranAttWithSameLabel.TransactionRequired;
            if (string.IsNullOrWhiteSpace(row.Value))
            {
                row.Value = tranAttWithSameLabel.Value;
            }

            DeleteAMMTranAttribute(sender, tranAttWithSameLabel);
        }

        protected virtual void AMMTranAttribute_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (AMMTranAttribute)e.Row;
            if (row == null || e.Operation == PXDBOperation.Delete)
            {
                return;
            }

            // Check for Value if the attribute is transaction required
            if (row.TransactionRequired == true && row.Value == null)
            {
                sender.RaiseExceptionHandling<AMMTranAttribute.value>(row, row.Value,
                    new PXSetPropertyException(Messages.GetLocal(Messages.TranLineRequiresAttribute,
                    AMDocType.GetDocTypeDesc(row.DocType),
                    sender.GetStatus(row) == PXEntryStatus.Inserted ? string.Empty : row.BatNbr.TrimIfNotNullEmpty(), 
                    row.TranLineNbr, row.Label), PXErrorLevel.Error));
            }
        }

        /// <summary>
        /// Query all production attributes related to transaction
        /// </summary>
        /// <param name="row">transaction row</param>
        /// <returns>query results in a dictionary with a key by Label</returns>
        protected virtual Dictionary<string, AMProdAttribute> GetProductionAttributeDictionary(AMMTran row)
        {
            if (string.IsNullOrWhiteSpace(row?.ProdOrdID))
            {
                return null;
            }

            PXSelectBase<AMProdAttribute> cmd = new PXSelect<AMProdAttribute,
                Where<AMProdAttribute.orderType, Equal<Required<AMProdAttribute.orderType>>,
                    And<AMProdAttribute.prodOrdID, Equal<Required<AMProdAttribute.prodOrdID>>,
                        And<AMProdAttribute.enabled, Equal<boolTrue>,
                            And<AMProdAttribute.source, NotEqual<AMAttributeSource.configuration>,
                            And<Where<AMProdAttribute.operationID, IsNull, 
                                Or<AMProdAttribute.operationID, Equal<Required<AMMTran.operationID>>>>>>>>>>(this);

            var dic = new Dictionary<string, AMProdAttribute>();
            foreach (AMProdAttribute result in cmd.Select(row.OrderType, row.ProdOrdID, row.OperationID))
            {
                if (result.Label == null || dic.ContainsKey(result.Label.Trim()))
                {
                    continue;
                }

                dic.Add(result.Label.Trim(), result);
            }
            return dic;
        }

        /// <summary>
        /// Insert given production attribute into the cache as a tran attribute
        /// </summary>
        /// <param name="prodAttribute">Production attribute row</param>
        /// <param name="row">Related parent transaction row</param>
        /// <returns>Inserted transaction attribute</returns>
        protected virtual AMMTranAttribute InsertAMMTranAttribute(AMProdAttribute prodAttribute, AMMTran row)
        {
            return TransactionAttributes.Insert(new AMMTranAttribute
            {
                // Need the keys for API calls to work correctly
                DocType = row.DocType,
                BatNbr = row.BatNbr,
                TranLineNbr = row.LineNbr,
                OrderType = row.OrderType,
                ProdOrdID = row.ProdOrdID,
                OperationID = prodAttribute.OperationID,
                ProdAttributeLineNbr = prodAttribute.LineNbr,
                AttributeID = prodAttribute.AttributeID,
                Label = prodAttribute.Label,
                Descr = prodAttribute.Descr,
                TransactionRequired = prodAttribute.TransactionRequired,
                Value = prodAttribute.Value
            });
        }

        /// <summary>
        /// Delete the given transaction attribute
        /// </summary>
        /// <param name="cache">cache of AMMTranAttribute</param>
        /// <param name="row">AMMTranAttribute to delete</param>
        protected virtual void DeleteAMMTranAttribute(PXCache cache, AMMTranAttribute row)
        {
            var status = cache.GetStatus(cache.LocateElse(row));
            if (status == PXEntryStatus.Inserted)
            {
                cache.Remove(row);
                return;
            }
            cache.Delete(row);
        }

        /// <summary>
        /// Sync the given transaction row's production transaction attributes. Add/Update/Delete tran attributes based on changed values
        /// </summary>
        /// <param name="row">Parent transaction row</param>
        protected virtual void SyncTransactionAttributes(AMMTran row)
        {
            if (string.IsNullOrWhiteSpace(row.OrderType) && string.IsNullOrWhiteSpace(row.ProdOrdID))
            {
                return;
            }

            var tranAttributes = GetTransactionAttributeDictionary(row);
            var prodAttributes = GetProductionAttributeDictionary(row);

            var deleteList = new List<AMMTranAttribute>();

            // Check for DELETES of incorrectly added attributes...
            foreach (var kvp in tranAttributes)
            {
                if (!prodAttributes.ContainsKey(kvp.Key))
                {
                    deleteList.Add(kvp.Value);
                    continue;
                }
                if (row.OperationID != null &&
                    kvp.Value.OperationID != null
                    && row.OperationID != kvp.Value.OperationID)
                {
                    deleteList.Add(kvp.Value);
                }
            }

            // Then delete those tran attributes from the cache...
            foreach (var tranAttribute in deleteList)
            {
                if (tranAttribute.Label != null && tranAttributes.ContainsKey(tranAttribute.Label.Trim()))
                {
                    tranAttributes.Remove(tranAttribute.Label.Trim());
                }
                DeleteAMMTranAttribute(TransactionAttributes.Cache, tranAttribute);
            }

            // Next insert/update tran attributes for missing or found but needs updated results
            foreach (var kvp in prodAttributes)
            {
                if (!tranAttributes.ContainsKey(kvp.Key))
                {
                    InsertAMMTranAttribute(kvp.Value, row);
                    continue;
                }

                //Update tranAttributes
                var cachedRow = TransactionAttributes.Cache.LocateElseCopy(tranAttributes[kvp.Key]);
                if (cachedRow == null)
                {
                    continue;
                }

                cachedRow.AttributeID = kvp.Value.AttributeID;
                cachedRow.Descr = kvp.Value.Descr;
                cachedRow.ProdAttributeLineNbr = kvp.Value.LineNbr;
                cachedRow.OperationID = kvp.Value.OperationID;
                cachedRow.TransactionRequired = kvp.Value.TransactionRequired;
                if (cachedRow.Value == null)
                {
                    cachedRow.Value = kvp.Value.Value;
                }
            }
        }

        /// <summary>
        /// Get the existing transaction attributes in the form of a dictionary by label key
        /// </summary>
        /// <param name="row">Parent transaction row</param>
        /// <returns></returns>
        protected virtual Dictionary<string, AMMTranAttribute> GetTransactionAttributeDictionary(AMMTran row)
        {
            return GetTransactionAttributeDictionary(row, false);
        }

        /// <summary>
        /// Get the existing transaction attributes in the form of a dictionary by label key
        /// </summary>
        /// <param name="row">Parent transaction row</param>
        /// <param name="cachedOnly">only return cached rows (no select on AMMTranAttributes)</param>
        /// <returns></returns>
        protected virtual Dictionary<string, AMMTranAttribute> GetTransactionAttributeDictionary(AMMTran row, bool cachedOnly)
        {
            if (row == null)
            {
                return null;
            }

            var tranAttributeDic = new Dictionary<string, AMMTranAttribute>();
            foreach (AMMTranAttribute att in TransactionAttributes.Cache.Cached)
            {
                if (att.Label == null || att.TranLineNbr != row.LineNbr || tranAttributeDic.ContainsKey(att.Label.Trim()))
                {
                    continue;
                }

                tranAttributeDic.Add(att.Label.Trim(), att);
            }

            if (cachedOnly)
            {
                return tranAttributeDic;
            }

            foreach (AMMTranAttribute att in PXSelect<
                AMMTranAttribute, 
                Where<AMMTranAttribute.docType, Equal<Current<AMMTran.docType>>,
                    And<AMMTranAttribute.batNbr, Equal<Current<AMMTran.batNbr>>, 
                    And<AMMTranAttribute.tranLineNbr, Equal<Current<AMMTran.lineNbr>>>>>>
                .SelectMultiBound(this, new object[] { row }))
            {
                if (att.Label == null || att.TranLineNbr != row.LineNbr || tranAttributeDic.ContainsKey(att.Label.Trim()))
                {
                    continue;
                }

                tranAttributeDic.Add(att.Label.Trim(), att);
            }
            return tranAttributeDic;
        }

        /// <summary>
        /// When deleting then inserting, the line counters are not getting set correctly on the insert. 
        /// Use this to increase the line counter so the inserts work correctly
        /// </summary>
        /// <param name="row"></param>
        protected virtual void BurnAttributeLineNbr(AMMTran row)
        {
            PXLineNbrAttribute.NewLineNbr<AMMTranAttribute.lineNbr>(TransactionAttributes.Cache, row);
        }

        protected virtual AMMTran GetParent(PXCache cache, AMMTranAttribute ammTranAttribute)
        {
            var parent = (AMMTran)PXParentAttribute.LocateParent(cache, ammTranAttribute, typeof(AMMTran));

            if (parent != null)
            {
                return parent;
            }

            parent = (AMMTran)PXParentAttribute.SelectParent(cache, ammTranAttribute, typeof(AMMTran));

            if (parent != null)
            {
                return parent;
            }

            foreach (AMMTran ammTran in transactions.Cache.Inserted)
            {
                if (ammTran.LineNbr == ammTranAttribute.TranLineNbr)
                {
                    return ammTran;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks for Under Issue of Material for a given move entry.
        /// If under issue found related to check level. cache received raised exception handling.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="row"></param>
        /// <returns>True when raiseexceptionhandling is called on cache AS AN ERROR</returns>
        protected virtual bool CheckUnderIssueMaterial(PXCache sender, AMMTran row)
        {
            if (row == null
                || row.Qty.GetValueOrDefault() == 0)
            {
                return false;
            }

            var amOrderType = (AMOrderType)PXSelectorAttribute.Select<AMMTran.orderType>(sender, row);
            if (amOrderType == null || amOrderType.UnderIssueMaterial == SetupMessage.AllowMsg)
            {
                return false;
            }

            if (row.Qty > 0 && ProductionTransactionHelper.CheckUnderIssuedMaterial(this, row, amOrderType.UnderIssueMaterial, false, out var exception)
                && exception != null)
            {
                sender.RaiseExceptionHandling<AMMTran.qty>(
                    row,
                    row.Qty,
                    new PXSetPropertyException(
                        exception.Message,
                        exception.IsWarning ? PXErrorLevel.Warning : PXErrorLevel.Error));

                if (exception.IsWarning && !IsImport && !IsContractBasedAPI)
                {
                    PXTrace.WriteWarning(exception.Message);
                }

                return !exception.IsWarning;
            }
            return false;
        }

        /// <summary>
        /// Checks for Operation is complete and handle exceptions as needed.
        /// If condition found related to check level. cache received raised exception handling.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="row"></param>
        /// <returns>True when raiseexceptionhandling is called on cache AS AN ERROR</returns>
        protected virtual bool CheckMoveOnCompletedOperation(PXCache sender, AMMTran row)
        {
            if (row?.OrderType == null || row.Qty.GetValueOrDefault() == 0 && row.QtyScrapped.GetValueOrDefault() == 0)
            {
                return false;
            }

            var amOrderType = (AMOrderType)PXSelectorAttribute.Select<AMMTran.orderType>(sender, row);
            if (amOrderType == null 
                || amOrderType.MoveCompletedOrders == SetupMessage.AllowMsg)
            {
                return false;
            }

            if (ProductionTransactionHelper.CheckMoveOnCompletedOperation(sender, row, amOrderType.MoveCompletedOrders, out var exception)
                && exception != null)
            {
                sender.RaiseExceptionHandling<AMMTran.qty>(
                    row,
                    row.Qty,
                    new PXSetPropertyException(
                        exception.Message,
                        exception.IsWarning ? PXErrorLevel.Warning : PXErrorLevel.Error));

                if (exception.IsWarning && !IsImport && !IsContractBasedAPI)
                {
                    PXTrace.WriteWarning(exception.Message);
                }

                return !exception.IsWarning;
            }
            return false;
        }

        /// <summary>
        /// Checks for transaction attempt to over complete the order at the last operation
        /// If condition found related to check level. cache received raised exception handling.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="row"></param>
        /// <returns>True when raiseexceptionhandling is called on cache AS AN ERROR</returns>
        protected virtual bool CheckOverCompletedOrders(PXCache sender, AMMTran row)
        {
            if (row?.OrderType == null || !row.LastOper.GetValueOrDefault() || row.Qty.GetValueOrDefault() == 0 && row.QtyScrapped.GetValueOrDefault() == 0)
            {
                return false;
            }

            var amOrderType = (AMOrderType)PXSelectorAttribute.Select<AMMTran.orderType>(sender, row);
            if (amOrderType == null
                || amOrderType.OverCompleteOrders == SetupMessage.AllowMsg)
            {
                return false;
            }

            if (ProductionTransactionHelper.CheckOverCompletedOrders(sender, row, amOrderType.OverCompleteOrders, 
                ampsetup.Current.InclScrap.GetValueOrDefault(), out var exception)
                && exception != null)
            {
                sender.RaiseExceptionHandling<AMMTran.qty>(
                    row,
                    row.Qty,
                    new PXSetPropertyException(
                        exception.Message,
                        exception.IsWarning ? PXErrorLevel.Warning : PXErrorLevel.Error));

                if (exception.IsWarning && !IsImport && !IsContractBasedAPI)
                {
                    PXTrace.WriteWarning(exception.Message);
                }

                return !exception.IsWarning;
            }
            return false;
        }

		protected virtual bool CheckOverCompletedOperation(PXCache sender, AMMTran row)
		{
			if (row?.OrderType == null || row.Qty.GetValueOrDefault() == 0 && row.QtyScrapped.GetValueOrDefault() == 0)
			{
				return false;
			}

			var amOrderType = (AMOrderType)PXSelectorAttribute.Select<AMMTran.orderType>(sender, row);
			if (amOrderType == null
				|| amOrderType.ExceedQtyOperations == SetupMessage.AllowMsg)
			{
				return false;
			}

			if (ProductionTransactionHelper.CheckOverCompletedOperation(sender, row, amOrderType.ExceedQtyOperations,
				ampsetup.Current.InclScrap.GetValueOrDefault(), out var exception)
				&& exception != null)
			{
				sender.RaiseExceptionHandling<AMMTran.qty>(
					row,
					row.Qty,
					new PXSetPropertyException(
						exception.Message,
						exception.IsWarning ? PXErrorLevel.Warning : PXErrorLevel.Error));

				if (exception.IsWarning && !IsImport && !IsContractBasedAPI)
				{
					PXTrace.WriteWarning(exception.Message);
				}

				return !exception.IsWarning;
			}

			return false;
		}

		#region AMBatchEntryBase members

		public override PXSelectBase<AMBatch> AMBatchDataMember => batch;
        public override PXSelectBase<AMMTran> AMMTranDataMember => transactions;
        public override PXSelectBase<AMMTranSplit> AMMTranSplitDataMember => splits;

        #endregion

        public class ProdTranLotSerialTotals
        {
            public string OrderType { get; set; }
            public string ProdOrdID { get; set; }
            public string LotSerNbr { get; set; }
            public decimal Qty { get; set; }
			public decimal QtyScrapped { get; set; }
		}
    }
}
