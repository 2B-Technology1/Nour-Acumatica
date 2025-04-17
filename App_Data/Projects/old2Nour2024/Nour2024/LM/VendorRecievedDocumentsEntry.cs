using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.PO;


namespace MyMaintaince.LM
{
    public class VendorRecievedDocumentsEntry : PXGraph<VendorRecievedDocumentsEntry,VendorReceivedDocumentsH>
    {
        public PXSelect<VendorReceivedDocumentsH> vendorReceivedDocumentsH;
        public PXSelect<VendorReceivedDocumentsD, Where<VendorReceivedDocumentsD.refNbr, Equal<Current<VendorReceivedDocumentsH.refNbr>>>> vendorReceivedDocumentsD;

        
        public PXAction<VendorReceivedDocumentsH> AddChassises;
        [PXButton]
        [PXUIField(DisplayName = "Add Chassises")]
        protected IEnumerable addChassises(PXAdapter adapter) {
            VendorReceivedDocumentsH row=this.vendorReceivedDocumentsH.Current;
            if(row == null ||row.PONbr == null)
                return adapter.Get();
 
            PXResultset<POReceiptLineSplit> set= PXSelect<POReceiptLineSplit, Where<POReceiptLineSplit.pONbr, Equal<Required<POReceiptLineSplit.pONbr>>,And<POReceiptLineSplit.receiptType,Equal<POReceiptType.poreceipt>>>>.Select(this,row.PONbr);
            foreach(PXResult<POReceiptLineSplit> res in set){
                POReceiptLineSplit line = (POReceiptLineSplit)res;
                POReceiptLineSplitExt lineExt = PXCache<POReceiptLineSplit>.GetExtension<POReceiptLineSplitExt>(line);
                POOrder poOrder=PXSelect<POOrder ,Where<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>,And<POOrder.orderType,Equal<POOrderType.regularOrder>>>>.Select(this,row.PONbr);
                
                
                //query and check if chassis exist before in another recieve document
                VendorReceivedDocumentsD ceb = PXSelect<VendorReceivedDocumentsD, Where<VendorReceivedDocumentsD.chassisNbr, Equal<Required<VendorReceivedDocumentsD.chassisNbr>>>>.Select(this, line.LotSerialNbr);

                if (ceb != null)
                {
                    //skip adding this line
                    continue;
                }

                VendorReceivedDocumentsD documentsDetailLine = new VendorReceivedDocumentsD();
                
                documentsDetailLine.RefNbr = row.RefNbr;
                documentsDetailLine.PONbr = line.PONbr;
                documentsDetailLine.ItemCode = line.InventoryID;
                documentsDetailLine.ChassisNbr = line.LotSerialNbr;

                documentsDetailLine.VendorID = poOrder.VendorID;
                documentsDetailLine.ModelYear = lineExt.UsrModelYear;
                documentsDetailLine.Color = lineExt.UsrColor;
                this.vendorReceivedDocumentsD.Insert(documentsDetailLine);
            }

            return adapter.Get();
        }
        
        public PXAction<VendorReceivedDocumentsH> Release;
        [PXButton]
        [PXUIField(DisplayName = "Release")]
        protected IEnumerable release(PXAdapter adapter)
        {
            VendorReceivedDocumentsH row = this.vendorReceivedDocumentsH.Current;
            if (row == null )
                return adapter.Get();


            row.released = true;
            vendorReceivedDocumentsH.Cache.SetValue<VendorReceivedDocumentsH.Released>(vendorReceivedDocumentsH.Current, true);
            this.vendorReceivedDocumentsH.Update(row);

            foreach (PXResult<VendorReceivedDocumentsD> res in vendorReceivedDocumentsD.Select())
            {
                VendorReceivedDocumentsD line = (VendorReceivedDocumentsD)res;
                line.Received = true;
                vendorReceivedDocumentsD.Cache.SetValue<VendorReceivedDocumentsD.received>(vendorReceivedDocumentsD.Current, true);
                this.vendorReceivedDocumentsD.Update(line);
            }


            this.Persist(typeof(VendorReceivedDocumentsH), PXDBOperation.Update);
            this.Persist(typeof(VendorReceivedDocumentsD),PXDBOperation.Update);
            this.Actions.PressSave();
            
            return adapter.Get();
        }

        protected virtual void VendorReceivedDocumentsD_ChassisNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            VendorReceivedDocumentsD row = (VendorReceivedDocumentsD)e.Row;
            if (row != null)
            {
                if (isExists(row.ChassisNbr))
                {

                    PXUIFieldAttribute.SetError<VendorReceivedDocumentsD.chassisNbr>(sender, row, "Already Received Before !");
                    return;
                }
                POReceiptLineSplit line = PXSelect<POReceiptLineSplit, Where<POReceiptLineSplit.lotSerialNbr, Equal<Required<POReceiptLineSplit.lotSerialNbr>>, And<POReceiptLineSplit.receiptType, Equal<POReceiptType.poreceipt>>>>.Select(this, row.ChassisNbr);
                POReceiptLineSplitExt lineExt = PXCache<POReceiptLineSplit>.GetExtension<POReceiptLineSplitExt>(line);
                POOrder poOrder = PXSelect<POOrder, Where<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>.Select(this, line.PONbr);
                VendorReceivedDocumentsD documentsDetailLine = new VendorReceivedDocumentsD();
                row.RefNbr = row.RefNbr;
                row.PONbr = line.PONbr;
                row.ItemCode = line.InventoryID;
                row.ChassisNbr = line.LotSerialNbr;
                if(poOrder !=null)
                   row.VendorID = poOrder.VendorID;
                
                row.ModelYear = lineExt.UsrModelYear;
                row.Color = lineExt.UsrColor;
            }
        }
        protected virtual void VendorReceivedDocumentsD_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            
            VendorReceivedDocumentsD row = (VendorReceivedDocumentsD)e.Row;
            if (row != null)
            {

                if (!String.IsNullOrEmpty(row.Received+""))
                {
                    if (row.Received.Value)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }

        protected virtual void VendorReceivedDocumentsH_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            VendorReceivedDocumentsH row = (VendorReceivedDocumentsH)e.Row;
            if (row != null)
            {
                if( !String.IsNullOrEmpty(row.released+"") )
                {
                    if (row.released.Value)
                    {
                        PXUIFieldAttribute.SetEnabled(vendorReceivedDocumentsD.Cache, null, false);
                    }
                    else 
                    {
                        PXUIFieldAttribute.SetEnabled(vendorReceivedDocumentsD.Cache, null, true);
                    }
                }
            }
        }
        

        protected Boolean isExists(string chassisNbr) {
            Boolean res=false;
            PXResultset<VendorReceivedDocumentsD> rs= PXSelect<VendorReceivedDocumentsD, Where<VendorReceivedDocumentsD.chassisNbr, Equal<Required<VendorReceivedDocumentsD.chassisNbr>>>>.Select(this,chassisNbr);
            if (rs != null)
            {

                if (rs.Count > 0)
                {
                    res = true;
                }
                else
                {
                    res = false;
                }
            }
            return res;
        }

    
    }
}