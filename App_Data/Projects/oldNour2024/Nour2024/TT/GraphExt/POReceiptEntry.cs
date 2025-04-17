using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.PO
{
  
  public class POReceiptEntry_Extension:PXGraphExtension<POReceiptEntry>
  {
        public AccessInfo Accessinfo = new AccessInfo();
        public static bool IsActive()
        {
            return true;
        }

        #region Event Handlers

        protected void POReceiptLine_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            int companyId = PX.Data.Update.PXInstanceHelper.CurrentCompany;
            var row = (POReceiptLine)e.Row; //GetExtension<CASplitEx>().UsrVendorName
            if (row != null)
            {
                if (row.CuryLineAmt != 0)
                {
                    decimal? tot
                      = row.GetExtension<POReceiptLineExt>().UsrBasePrice
                      + row.GetExtension<POReceiptLineExt>().UsrInsurancePrice
                      + row.GetExtension<POReceiptLineExt>().UsrTechSupportPrice
                      + row.GetExtension<POReceiptLineExt>().UsrAccesoriesPrice
                      + row.GetExtension<POReceiptLineExt>().UsrGovDevelopFees;

                    if (tot != row.CuryLineAmt)
                    {
                        if (companyId == 2)
                        {
                            //throw new PXException("Price details do not match the total price.");
                        }
                    }

                }
            }

        }


        protected virtual void POReceipt_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
      {
          if(e.Row != null){

          POReceipt row = e.Row as POReceipt;

          PXSelectBase<POReceiptLine> rlines = new PXSelect<POReceiptLine, Where<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>, And<POReceiptLine.receiptType, Equal<Required<POReceiptLine.receiptType>>>>>(this.Base);
          rlines.Cache.ClearQueryCache();
          PXResultset<POReceiptLine> rlinesRes = rlines.Select(row.ReceiptNbr, row.ReceiptType);

          foreach(PXResult<POReceiptLine> linees in rlinesRes){
              POReceiptLine line = (POReceiptLine)linees;
              PXSelectBase<POLine> lines = new PXSelect<POLine, Where<POLine.orderNbr, Equal<Required<POLine.orderNbr>>, And<POLine.orderType, Equal<Required<POLine.orderType>>, And<POLine.inventoryID, Equal<Required<POLine.inventoryID>>>>>>(this.Base);
              lines.Cache.ClearQueryCache();
              POLine linesRes = lines.Select(line.PONbr, line.POType, line.InventoryID);
              if (linesRes != null)
              {
                  POLineExt linesResExt = PXCache<POLine>.GetExtension<POLineExt>(linesRes);
                  if (linesResExt != null)
                  {
                      this.Base.transactions.Cache.SetValue<POReceiptLineExt.usrCommercialPrice>(line, linesResExt.UsrCommercialPrice);
                      this.Base.transactions.Cache.SetValue<POReceiptLineExt.usrVendorDiscount>(line, linesResExt.UsrVendorDiscount);
                      this.Base.transactions.Cache.IsDirty = true;
                      this.Base.transactions.Cache.RaiseFieldUpdated<POReceiptLineExt.usrCommercialPrice>(line, null);
                  }

              }
          }
          
          

          }
    }
            
    protected void POReceiptLine_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
      { 
          if(e.Row != null){
              POReceiptLine row = e.Row as POReceiptLine;
              if (!String.IsNullOrEmpty(row.SiteID + ""))
              {
                  PXSelectBase<INSite> sites = new PXSelectReadonly<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>(this.Base);
                  sites.Cache.ClearQueryCache();
                  INSite site = sites.Select(row.SiteID);
                  sender.SetValue<POReceiptLineExt.usrWarehouseDesc>(row, site.Descr);
              }
          }
      }

            
    protected void POReceiptLineSplit_UsrModelYear_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
    {
        if (e.Row != null)
        {
            POReceiptLineSplit row = e.Row as POReceiptLineSplit;
            POReceiptLineSplitExt rowExt = sender.GetExtension<POReceiptLineSplitExt>(row);
            try
            {
                int year =Int32.Parse(rowExt.UsrModelYear);
                if (year > 1950 && year < 2090)
                {

                }
                else {
                        // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                        throw new PXException("Invalid Year , must be number between 1950-2090 ");
                }
            }catch(InvalidCastException){
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Invalid Year , must be number between 1950-2090 ");
            }
        }
    }
          
    #endregion

  }


}