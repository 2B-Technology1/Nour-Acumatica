using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.TX;
using PX.Objects.IN;
using PX.Objects.EP;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.SO;
using PX.TM;
using SOOrder = PX.Objects.SO.SOOrder;
using SOLine = PX.Objects.SO.SOLine;
//using Avalara.AvaTax.Adapter;
//using Avalara.AvaTax.Adapter.TaxService;
using PX.Objects.PM;
//using AvaAddress = Avalara.AvaTax.Adapter.AddressService;
//using AvaMessage = Avalara.AvaTax.Adapter.Message;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.Common;
using PX.Objects;
using PX.Objects.PO;

namespace PX.Objects.PO
{
  
  public class POOrderEntry_Extension:PXGraphExtension<POOrderEntry>
  {

    #region Event Handlers
    protected void POLine_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
      { 
          if(e.Row != null){
              POLine row = e.Row as POLine;
              PXSelectBase<INSite> sites = new PXSelectReadonly<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>(this.Base);
              sites.Cache.ClearQueryCache();
              if(!String.IsNullOrEmpty(row.SiteID+"")){     
                INSite site=sites.Select(row.SiteID);
                sender.SetValue<POLineExt.usrWarehouseDesc>(row, site.Descr);
              }
         }
      }  
            
    #endregion

  }


}