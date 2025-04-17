using PX.Data;
using System.Collections;
using System;
using System.Collections.Generic;
using PX.Objects;
using PX.Objects.GL;

namespace PX.Objects.GL
{
  
  public class AccountHistoryBySubEnq_Extension:PXGraphExtension<AccountHistoryBySubEnq>
  {

   #region Event Handlers

      protected virtual void GLHistoryEnquiryResult_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
      { 
         GLHistoryEnquiryResult row = e.Row as GLHistoryEnquiryResult;
         if(row != null){
             if (!String.IsNullOrEmpty(row.SubCD))
             {
                 PXSelectBase<Sub> rows = new PXSelectReadonly<Sub, Where<Sub.subCD, Equal<Required<Sub.subCD>>>>(this.Base);
                 rows.Cache.ClearQueryCache();
                 Sub sub = rows.Select(row.SubCD);
                 if (sub != null){
                     sender.SetValue<GLHistoryEnquiryResultExt.usrSubDescr>(row,sub.Description);
                 }
             }
         }
       }
         
    #endregion

  }


}