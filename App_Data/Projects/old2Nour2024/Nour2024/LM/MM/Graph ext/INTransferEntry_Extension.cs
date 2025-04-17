using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using System.Text;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects;
using PX.Objects.IN;
using PX.Objects.PO;



namespace Maintenance.MM.Graph_ext
{

    public class INTransferEntry_Extension : PXGraphExtension<INTransferEntry>
    {

         public static bool IsActive()
        {
            return true;
        }
        //    public PXAction<INRegister> release;
        //[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        //[PXProcessButton]
        //public virtual IEnumerable Release(PXAdapter adapter)
        //{
        //   INRegister row = this.Base.transfer.Current;
        //   //INRegister row = (INRegister)this.issue.Current;
        //  AccessInfo info = Base.Accessinfo;
        //  row.TranDate = info.BusinessDate;
        //  PXCache cache = this.Base.transfer.Cache;
        //  List<INRegister> list = new List<INRegister>();
        //  foreach (INRegister indoc in adapter.Get<INRegister>())
        //  {
        //    if (indoc.Hold == false && indoc.Released == false)
        //    {
        //      cache.Update(indoc);
        //      list.Add(indoc);
        //    }
        //  }
        //  if (list.Count == 0)
        //  {
        //    throw new PXException(PX.Objects.CM.Messages.Document_Status_Invalid);
        //  }
        //  Save.Press();
        //  PXLongOperation.StartOperation(this, delegate() { INDocumentRelease.ReleaseDoc(list, false); });
        //  return list;

        protected void INTran_Qty_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {

            try
            {
                var row = (INTran)e.Row;
                // Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers [Justification]
                INTransferEntry innn = PXGraph.CreateInstance<INTransferEntry>();



                //PXSelectBase<Customer> res = new PXSelectReadonly<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(this);
                //res.Cache.ClearQueryCache();
                //Customer cus = res.Select(row.CustomerID);

                POReceiptLine chassis = PXSelect<POReceiptLine,
                Where<POReceiptLine.inventoryID, Equal<Required<INTran.inventoryID>>,
                And<POReceiptLine.lotSerialNbr, Equal<Required<INTran.lotSerialNbr>>>>>.Select(innn, row.InventoryID, row.LotSerialNbr);
                if (chassis != null)
                {

                    //row.GetExtension<INTranExt>().UsrColor=chassis.GetExtension<POReceiptLineExt>().UsrColor;
                    cache.SetValueExt<INTran.tranDesc>(e.Row, chassis.GetExtension<POReceiptLineExt>().UsrCommercialPrice);
                    //cache.SetValueExt<INTranExt.usrColor>(e.Row, chassis.GetExtension<POReceiptLineExt>().UsrColor);
                }
            }
            catch { }
        }




    }
}



    

       