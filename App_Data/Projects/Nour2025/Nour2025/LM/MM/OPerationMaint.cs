using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.IN;
using PX.SM;
namespace MyMaintaince
{
    public class OPerationMaint : PXGraph<OPerationMaint, Operation>
    {
        
        public PXSelect<Operation> operation;
        public PXSelect<StockItems, Where<StockItems.operationId, Equal<Current<Operation.oPerationID>>>> stockItems;
        public PXSelect<NonStockItem, Where<NonStockItem.operationId, Equal<Current<Operation.oPerationID>>>> nonStockItems;
        //public PXSelect<Model, Where<Model.operationID, Equal<Current<Operation.oPerationID>>>> model;
        public PXSelect<MoelOperation, Where<MoelOperation.operationID, Equal<Current<Operation.oPerationID>>>> modelOperation;
        
        
        //protected virtual void StockItems_code_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)//poo
        protected virtual void StockItems_inventoryCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row != null)
            {
                StockItems row = e.Row as StockItems;
                InventoryItem invt = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.InventoryCode);
                
                if (invt != null)
                {
                    sender.SetValue<StockItems.inventoryName>(row, invt.Descr);
                }
            }
        }


        //protected virtual void NonStockItem_code_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        protected virtual void NonStockItem_serviceCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row != null)
            {
                NonStockItem row = e.Row as NonStockItem;
                InventoryItem invt = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.ServiceCode);
                if (invt != null)
                {
                    sender.SetValue<NonStockItem.serviceName>(row, invt.Descr);
                }
                PXSelectBase<JobTimeAttachedNonStocks> jobTimeAttachedNonStocks = new PXSelectReadonly<JobTimeAttachedNonStocks, Where<JobTimeAttachedNonStocks.nonStockID, Equal<Required<JobTimeAttachedNonStocks.nonStockID>>>>(this);
                jobTimeAttachedNonStocks.Cache.ClearQueryCache();
                PXResultset<JobTimeAttachedNonStocks> jobTimeAttachedNonStocksResult = jobTimeAttachedNonStocks.Select(row.ServiceCode);
                if (jobTimeAttachedNonStocksResult.Count <= 0)
                {
                    throw new PXException("this item is not defined in any job Time!");
                }
                else
                {
                    JobTimeAttachedNonStocks jobTimeAttachedNonStock = jobTimeAttachedNonStocksResult;
                    sender.SetValue<JobOrderNonStockItems.time>(row, jobTimeAttachedNonStock.StandardQty);

                }
            }
        }

        protected virtual void MoelOperation_modelCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row != null)
            {
                MoelOperation row = e.Row as MoelOperation;
                Model invt = PXSelect<Model, Where<Model.modelID, Equal<Required<MoelOperation.modelCode>>>>.Select(this, row.ModelCode);
                if (invt != null)
                {
                    sender.SetValue<MoelOperation.modelName>(row, invt.Name);
                }
            }
        }
        public virtual void Operation_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            Operation row = this.operation.Current;
            PXSelectBase<OperationJOB> oper = new PXSelect<OperationJOB, Where<OperationJOB.operationID, Equal<Required<OperationJOB.operationID>>>>(this);//poo
            PXResultset<OperationJOB> result = oper.Select(row.OPerationID); //poo
             
            if (result.Count > 0)
            {
                throw new PXSetPropertyException("can not delete this operation because selected in Job Order.");
            }
        }
        public virtual void Operation_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            //MoelOperation row = this.modelOperation.Current;

            //if (row== null)
            //{
            //    throw new PXSetPropertyException("can not Save this operation because No Any Vechiles is Attached");
            //}
            //StockItems row2 = this.stockItems.Current;
            //NonStockItem row3 = this.nonStockItems.Current;

            //if ((row2 == null) && (row3 == null))
            //{
            //    throw new PXSetPropertyException("can not Save this operation because No Any Stock Items or Service is Attached");
            //}

        }
        public virtual void StockItems_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            //StockItems row = this.stockItems.Current;
            //PXSelectBase<OperationJOB> oper = new PXSelect<OperationJOB, Where<OperationJOB.operationID, Equal<Required<OperationJOB.operationID>>>>(this);
            //PXResultset<OperationJOB> result = oper.Select(row.OperationId);
            //if (result.Count > 0)
            //{
            //    throw new PXSetPropertyException("can not delete this Stockitem because selected in Job Order.");
            //}
        }
        public virtual void StockItems_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            StockItems row = this.stockItems.Current;
            if (row.Quantity <= 0)
            {
                throw new PXSetPropertyException("can not Insert Zero Quantity");
            }
           
        }
        public virtual void NonStockItem_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            //NonStockItem row = this.nonStockItems.Current;
            //PXSelectBase<OperationJOB> oper = new PXSelect<OperationJOB, Where<OperationJOB.operationID, Equal<Required<OperationJOB.operationID>>>>(this);
            //PXResultset<OperationJOB> result = oper.Select(row.OperationId);
            //if (result.Count > 0)
            //{
            //    throw new PXSetPropertyException("can not delete this NonStockitem because selected in Job Order.");
            //}
        }
        public virtual void MoelOperation_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            MoelOperation row = this.modelOperation.Current;
            PXSelectBase<OperationJOB> oper = new PXSelect<OperationJOB, Where<OperationJOB.operationID, Equal<Required<OperationJOB.operationID>>>>(this);
            PXResultset<OperationJOB> result = oper.Select(row.OperationID);
            if (result.Count > 0)
            {
                throw new PXSetPropertyException("can not delete this Brand because selected in Job Order.");
            }
        }
         
        }
    }
    
