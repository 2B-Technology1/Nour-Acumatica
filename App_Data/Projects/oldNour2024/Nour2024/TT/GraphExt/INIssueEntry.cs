using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects;
using PX.Objects.IN;
using MyMaintaince.SN;
using MyMaintaince;
using Maintenance;
using PX.Objects.AR;

namespace PX.Objects.IN
{

    public class INIssueEntry_Extension : PXGraphExtension<INIssueEntry>
    {

        #region Event Handlers

        protected virtual void INTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            INTran row = (INTran)e.Row;

            if (e.Row != null)
            {
                INTranExt rowExt = PXCache<INTran>.GetExtension<INTranExt>(row);
                if (rowExt.UsrOrdID == null)
                {
                    rowExt.UsrOrdID = "-Default";
                    sender.SetValue<INTranExt.usrOrdID>(row, "-Default");
                }

            }

            if (e.Operation == PXDBOperation.Delete)
                return;

            INRegister reg = this.Base.CurrentDocument.Select();
            INRegisterExt regExt = PXCache<INRegister>.GetExtension<INRegisterExt>(reg);

            if (row != null)
            {
                InventoryWarehouseSerials line = PXSelect<InventoryWarehouseSerials, Where<InventoryWarehouseSerials.warehouse, Equal<Required<InventoryWarehouseSerials.warehouse>>, And<InventoryWarehouseSerials.tranType, Equal<WSTTypes.issue>>>>.Select(this.Base, row.SiteID);

                if ((regExt.UsrWarehouseSerial == 0 || String.IsNullOrEmpty(regExt.UsrWarehouseSerial + "")) && line != null)
                {

                    if (this.Base.transactions.Select().Count <= 0)
                        return;

                    regExt.UsrWarehouseSerial = line.LastRefNbr + 1;
                    sender.SetValueExt<INRegisterExt.usrWarehouseSerial>(reg, line.LastRefNbr + 1);

                    WarehouseSerialEntry g = PXGraph.CreateInstance<WarehouseSerialEntry>();
                    g.warehouseSerial.Current = line;
                    line.LastRefNbr = line.LastRefNbr + 1;
                    g.warehouseSerial.Update(line);
                    g.Persist(typeof(InventoryWarehouseSerials), PXDBOperation.Update);


                }
            }


        }
        protected virtual void INRegister_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
         try
            {
                INRegister InR = e.Row as INRegister;
                INIssueEntry x = new INIssueEntry();
                if (e.Row != null)
                {

                    INRegisterExt inreg = PXCache<INRegister>.GetExtension<INRegisterExt>(InR);

                    if (!String.IsNullOrEmpty(inreg.UsrOrderID + ""))
                    {
                        PXSelectBase<Customer> Jo = new PXSelectJoin<Customer, InnerJoin<JobOrder, On<JobOrder.customer, Equal<Customer.acctCD>>>, Where<JobOrder.jobOrdrID, Equal<Current<JobOrder.jobOrdrID>>>>(x);
                        Jo.Cache.ClearQueryCache();
                        Customer job = Jo.Select(inreg.UsrOrderID);
                        sender.SetValueExt<INRegisterExt.customer>(e.Row, job.AcctName);

                        PXSelectBase<Items> Itm = new PXSelectJoin<Items, InnerJoin<JobOrder, On<JobOrder.itemsID, Equal<Items.itemsID>>>, Where<JobOrder.jobOrdrID, Equal<Current<JobOrder.jobOrdrID>>>>(x);
                        Itm.Cache.ClearQueryCache();
                        Items it = Itm.Select(inreg.UsrOrderID);
                        sender.SetValueExt<INRegisterExt.code>(e.Row, it.Code);

                    }
                }
            }
            catch
            {
 
            }
        }
        protected virtual void INRegister_UsrOrderID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {

            if (e.Row != null)
            {
                INIssueEntry x = new INIssueEntry();
                INRegister header = (INRegister)x.issue.Current;
                INRegisterExt inreg = PXCache<INRegister>.GetExtension<INRegisterExt>(header);

                if (!String.IsNullOrEmpty(inreg.UsrOrderID + ""))
                {
                    PXSelectBase<Customer> Jo = new PXSelectJoin<Customer, InnerJoin<JobOrder, On<JobOrder.customer, Equal<Customer.acctCD>>>, Where<JobOrder.jobOrdrID, Equal<Current<JobOrder.jobOrdrID>>>>(x);
                    Jo.Cache.ClearQueryCache();
                    Customer job = Jo.Select(inreg.UsrOrderID);
                    sender.SetValueExt<INRegisterExt.customer>(e.Row, job.AcctName);

                    PXSelectBase<Items> Itm = new PXSelectJoin<Items, InnerJoin<JobOrder, On<JobOrder.itemsID, Equal<Items.itemsID>>>, Where<JobOrder.jobOrdrID, Equal<Current<JobOrder.jobOrdrID>>>>(x);
                    Itm.Cache.ClearQueryCache();
                    Items it = Itm.Select(inreg.UsrOrderID);
                    sender.SetValueExt<INRegisterExt.code>(e.Row, it.Code);

                }
            }
        }
        protected virtual void INTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            INRegister reg = this.Base.CurrentDocument.Current;
            INRegisterExt regExt = PXCache<INRegister>.GetExtension<INRegisterExt>(reg);
            if (this.Base.transactions.Select().Count <= 0)
            {
                regExt.UsrWarehouseSerial = 0;
                sender.SetValueExt<INRegisterExt.usrWarehouseSerial>(reg, 0);
            }
        }
        #endregion
    }


}