using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.IN;


namespace MyMaintaince
{
    public class JobTimeMaint2 : PXGraph<JobTimeMaint2, JobTime>
    {    
        public PXSelect<JobTime> jobTime;
        [PXImport(typeof(JobTime))]
        public PXSelect<JobTimeAttachedNonStocks, Where<JobTimeAttachedNonStocks.jobTimeID, Equal<Current<JobTime.Code>>>> jobTimeAttachedNonStocks;


        protected virtual void JobTimeAttachedNonStocks_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
              if(e.Row != null){
                  JobTimeAttachedNonStocks row = e.Row as JobTimeAttachedNonStocks;
                  //verify if this non-stock already existed before
                  PXSelectBase<JobTimeAttachedNonStocks> jobTimeAttachedNonStocks = new PXSelectReadonly<JobTimeAttachedNonStocks, Where<JobTimeAttachedNonStocks.nonStockID, Equal<Required<JobTimeAttachedNonStocks.nonStockID>>,And<JobTimeAttachedNonStocks.lineID,NotEqual<Required<JobTimeAttachedNonStocks.lineID>>>>>(this);
                  jobTimeAttachedNonStocks.Cache.ClearQueryCache();
                  PXResultset<JobTimeAttachedNonStocks> jobTimeAttachedNonStocksResult = jobTimeAttachedNonStocks.Select(row.NonStockID,row.LineID);
                  if (jobTimeAttachedNonStocksResult.Count > 0)
                  {
                      e.Cancel = true;
                      //sender.RaiseExceptionHandling<JobTimeAttachedNonStocks.nonStockID>(row, row.NonStockID, new PXSetPropertyException("already exists in another Job Time!!", PXErrorLevel.Error));
                      throw new PXSetPropertyException("already exists in another Job Time!!", PXErrorLevel.Error);
                  }
  
              }
        }

        protected virtual void JobTimeAttachedNonStocks_nonStockID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            JobTimeAttachedNonStocks row = e.Row as JobTimeAttachedNonStocks;
            if (e.Row != null)
            {
            
                
                PXSelectBase<InventoryItem> invt = new PXSelectReadonly<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(this);
                invt.Cache.ClearQueryCache();
                InventoryItem result = invt.Select(row.NonStockID);
                sender.SetValue<JobTimeAttachedNonStocks.nonStockName>(row, result.Descr);
            }         
        
        }
    
    }
}