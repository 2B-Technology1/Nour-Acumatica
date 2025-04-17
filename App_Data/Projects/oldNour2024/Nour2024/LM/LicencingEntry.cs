using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.SO;


namespace MyMaintaince.LM
{
    public class LicencingEntry : PXGraph<LicencingEntry, Licencing>
    {
        public PXSelect<Licencing> licencing;
        public PXSelect<Licencing, Where<Licencing.refNbr, Equal<Current<Licencing.refNbr>>>> licencingDetailsTab;
        public LicencingEntry() {
            Action.AddMenuAction(Print);
            Action.AddMenuAction(Send);
            Action.AddMenuAction(Transfer);
            Action.AddMenuAction(Receive);
            Action.AddMenuAction(Release);
            Action.AddMenuAction(CustomerReceive);
            Action.MenuAutoOpen = true;
        }

        #region Row Actions
        protected virtual void Licencing_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {

            Licencing row = e.Row as Licencing;
            if (row != null)
            {
                if (row.Status == LicenceStatus.Open)
                {
                    Print.SetEnabled(true);
                    Send.SetEnabled(false);
                    Transfer.SetEnabled(false);
                    Receive.SetEnabled(false);
                    Release.SetEnabled(false);
                    CustomerReceive.SetEnabled(false);
                }
                else if (row.Status == LicenceStatus.Printed)
                {
                    Print.SetEnabled(false);
                    Send.SetEnabled(true);
                    Transfer.SetEnabled(false);
                    Receive.SetEnabled(false);
                    Release.SetEnabled(false);
                    CustomerReceive.SetEnabled(false);
                }
                else if (row.Status == LicenceStatus.Sent)
                {
                    Print.SetEnabled(false);
                    Send.SetEnabled(false);
                    Transfer.SetEnabled(true);
                    Receive.SetEnabled(false);
                    Release.SetEnabled(true);
                    CustomerReceive.SetEnabled(false);
                }
                else if (row.Status == LicenceStatus.Transfered)
                {
                    Print.SetEnabled(false);
                    Send.SetEnabled(false);
                    Transfer.SetEnabled(false);
                    Receive.SetEnabled(true);
                    Release.SetEnabled(false);
                    CustomerReceive.SetEnabled(false);
                }
                else if (row.Status == LicenceStatus.Received)
                {
                    Print.SetEnabled(false);
                    Send.SetEnabled(false);
                    Transfer.SetEnabled(false);
                    Receive.SetEnabled(false);
                    Release.SetEnabled(true);
                    CustomerReceive.SetEnabled(false);
                }
                else if (row.Status == LicenceStatus.Released)
                {
                    Print.SetEnabled(false);
                    Send.SetEnabled(false);
                    Transfer.SetEnabled(false);
                    Receive.SetEnabled(false);
                    Release.SetEnabled(false);
                    CustomerReceive.SetEnabled(true);
                }
                else if (row.Status == LicenceStatus.CustomerReceived)
                {
                    Print.SetEnabled(false);
                    Send.SetEnabled(false);
                    Transfer.SetEnabled(false);
                    Receive.SetEnabled(false);
                    Release.SetEnabled(false);
                    CustomerReceive.SetEnabled(false);
                }
            }
        }
        #endregion
        protected virtual void Licencing_ChassisNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Licencing row = (Licencing)e.Row;
            
            if (row != null)
            {

                SOLineSplit line = PXSelect<SOLineSplit, Where<SOLineSplit.lotSerialNbr, Equal<Required<SOLineSplit.lotSerialNbr>>>>.Select(this, row.ChassisNbr);
                SOLineSplitExt lineExt = PXCache<SOLineSplit>.GetExtension<SOLineSplitExt>(line);
                SOOrder soOrder = PXSelect<SOOrder, Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>, And<SOOrder.orderType, Equal<Required<SOOrder.orderType>>>>>.Select(this, line.OrderNbr,line.OrderType);
                SOOrderExt soOrderExt = PXCache<SOOrder>.GetExtension<SOOrderExt>(soOrder);
                SOOrderShipment orderShipment = PXSelect<SOOrderShipment, Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>, And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>>>>.Select(this, line.OrderType, line.OrderNbr);
                row.SONbr = line.OrderNbr;
                row.SOType = line.OrderType;
                row.ARNbr = orderShipment.InvoiceNbr;
                row.ARType = orderShipment.InvoiceType;
                row.InvoiceDate = soOrder.OrderDate;
                
                row.ItemCode = line.InventoryID;
                if (soOrder != null)
                    row.CustomerID = soOrder.CustomerID;
                row.ModelYear = lineExt.UsrModelYear;
                row.Color = lineExt.UsrColor;
                row.SOPaymentType = soOrderExt.UsrSOPaymentType;
            }
        }


        protected virtual void Licencing_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            Licencing row = this.licencing.Current;
            if (row != null)
            {

                PXResultset<VendorReceivedDocumentsD> set = PXSelect<VendorReceivedDocumentsD, Where<VendorReceivedDocumentsD.chassisNbr, Equal<Required<VendorReceivedDocumentsD.chassisNbr>>>>.Select(this, row.ChassisNbr);
                if (set.Count <= 0)
                {
                    e.Cancel = true;
                    throw new PXException("Never Recieved in a document");
                }
            }
        }

        public PXAction<Licencing> Action;
        [PXButton]
        [PXUIField(DisplayName = "Actions")]
        protected IEnumerable action(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<Licencing> Print;
        [PXButton]
        [PXUIField(DisplayName = "Print")]
        protected IEnumerable print(PXAdapter adapter) {

            Licencing row = this.licencing.Current;
            
            
            row.Status = LicenceStatus.Printed;

            licencing.Cache.SetValue<Licencing.status>(licencing.Current, LicenceStatus.Printed);
            licencingDetailsTab.Cache.SetValue<Licencing.printUser>(licencingDetailsTab.Current, this.Accessinfo.UserID);
            licencingDetailsTab.Cache.SetValue<Licencing.printDate>(licencingDetailsTab.Current, this.Accessinfo.BusinessDate);
            licencingDetailsTab.Current.PrintUser = this.Accessinfo.UserID;
            licencingDetailsTab.Current.PrintDate = this.Accessinfo.BusinessDate;
            this.licencingDetailsTab.Update(licencingDetailsTab.Current);
            this.Persist();
            this.Actions.PressSave();
            

            Print.SetEnabled(false);
            Send.SetEnabled(true);
            Transfer.SetEnabled(false);
            Receive.SetEnabled(false);
            Release.SetEnabled(false);
            CustomerReceive.SetEnabled(false);

            this.licencing.Update(row);
            this.Persist();
            this.Actions.PressSave();
            
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            //string actualReportID = null;
            //parameters["SOOrder.OrderType"] = "IN";
            //parameters["SOOrder.OrderNbr"] = "000963";
            /**
            if (actualReportID == null)
                    actualReportID = new NotificationUtility(this).SearchReport(ARNotificationSource.Customer, customer.Current, reportID, order.BranchID);
            }
            throw new PXReportRequiredException(parameters, actualReportID,"Report " + actualReportID);
            **/

            throw new PXReportRequiredException(parameters, "AE809046", "Report AE809046");

            //return adapter.Get();
        }

        public PXAction<Licencing> Send;
        [PXButton]
        [PXUIField(DisplayName = "Send")]
        protected IEnumerable send(PXAdapter adapter)
        {
            Licencing row = this.licencing.Current;
            row.Status = LicenceStatus.Sent;

            licencing.Cache.SetValue<Licencing.status>(licencing.Current, LicenceStatus.Sent);
            licencingDetailsTab.Cache.SetValue<Licencing.sendUser>(licencingDetailsTab.Current, this.Accessinfo.UserID);
            licencingDetailsTab.Cache.SetValue<Licencing.sendDate>(licencingDetailsTab.Current, this.Accessinfo.BusinessDate);
            licencingDetailsTab.Current.SendUser = this.Accessinfo.UserID;
            licencingDetailsTab.Current.SendDate = this.Accessinfo.BusinessDate;
            this.licencingDetailsTab.Update(licencingDetailsTab.Current);
            this.Persist();
            this.Actions.PressSave();
            

            Print.SetEnabled(false);
            Send.SetEnabled(false);
            Transfer.SetEnabled(true);
            Receive.SetEnabled(false);
            Release.SetEnabled(true);
            CustomerReceive.SetEnabled(false);

            return adapter.Get();
        }

        public PXAction<Licencing> Transfer;
        [PXButton]
        [PXUIField(DisplayName = "Transfer")]
        protected IEnumerable transfer(PXAdapter adapter)
        {
            Licencing row = this.licencing.Current;
            row.Status = LicenceStatus.Transfered;
            LicencingEntry x = new LicencingEntry();
            licencing.Cache.SetValue<Licencing.status>(licencing.Current, LicenceStatus.Transfered);
            licencingDetailsTab.Cache.SetValue<Licencing.transferUser>(licencingDetailsTab.Current, this.Accessinfo.UserID);
            licencingDetailsTab.Cache.SetValue<Licencing.transferDate>(licencingDetailsTab.Current, this.Accessinfo.BusinessDate);
            licencingDetailsTab.Current.TransferUser =this.Accessinfo.UserID;
            licencingDetailsTab.Current.TransferDate = this.Accessinfo.BusinessDate;
            this.licencingDetailsTab.Update(licencingDetailsTab.Current);
            this.Persist();
            this.Actions.PressSave();
            

            Print.SetEnabled(false);
            Send.SetEnabled(false);
            Transfer.SetEnabled(false);
            Receive.SetEnabled(true);
            Release.SetEnabled(false);
            CustomerReceive.SetEnabled(false);

            return adapter.Get();
        }

        public PXAction<Licencing> Receive;
        [PXButton]
        [PXUIField(DisplayName = "Receive")]
        protected IEnumerable receive(PXAdapter adapter)
        {
            Licencing row = this.licencing.Current;
            row.Status = LicenceStatus.Received;

            licencing.Cache.SetValue<Licencing.status>(licencing.Current, LicenceStatus.Received);
            licencingDetailsTab.Cache.SetValue<Licencing.receiveUser>(licencingDetailsTab.Current, this.Accessinfo.UserID);
            licencingDetailsTab.Cache.SetValue<Licencing.receiveDate>(licencingDetailsTab.Current, this.Accessinfo.BusinessDate);
            licencingDetailsTab.Current.ReceiveUser = this.Accessinfo.UserID;
            licencingDetailsTab.Current.ReceiveDate = this.Accessinfo.BusinessDate;
            this.licencingDetailsTab.Update(licencingDetailsTab.Current);
            this.Persist();
            this.Actions.PressSave();
            

            Print.SetEnabled(false);
            Send.SetEnabled(false);
            Transfer.SetEnabled(false);
            Receive.SetEnabled(false);
            Release.SetEnabled(true);
            CustomerReceive.SetEnabled(false);
            return adapter.Get();
        }

        public PXAction<Licencing> Release;
        [PXButton]
        [PXUIField(DisplayName = "Release")]
        protected IEnumerable release(PXAdapter adapter)
        {
            Licencing row = this.licencing.Current;
            row.Status = LicenceStatus.Released;

            licencing.Cache.SetValue<Licencing.status>(licencing.Current, LicenceStatus.Released);
            licencingDetailsTab.Cache.SetValue<Licencing.releaseUser>(licencingDetailsTab.Current, this.Accessinfo.UserID);
            licencingDetailsTab.Cache.SetValue<Licencing.releaseDate>(licencingDetailsTab.Current, this.Accessinfo.BusinessDate);
            licencingDetailsTab.Current.ReleaseUser = this.Accessinfo.UserID;
            licencingDetailsTab.Current.ReleaseDate = this.Accessinfo.BusinessDate;
            this.licencingDetailsTab.Update(licencingDetailsTab.Current);
            this.Persist();
            this.Actions.PressSave();

            Print.SetEnabled(false);
            Send.SetEnabled(false);
            Transfer.SetEnabled(false);
            Receive.SetEnabled(false);
            Release.SetEnabled(false);
            CustomerReceive.SetEnabled(true);
            return adapter.Get();
        }

        public PXAction<Licencing> CustomerReceive;
        [PXButton]
        [PXUIField(DisplayName = "Customer Receive")]
        protected IEnumerable customerReceive(PXAdapter adapter)
        {
            Licencing row = this.licencing.Current;
            row.Status = LicenceStatus.CustomerReceived;

            licencing.Cache.SetValue<Licencing.status>(licencing.Current, LicenceStatus.CustomerReceived);
            licencingDetailsTab.Cache.SetValue<Licencing.customerReceivedUser>(licencingDetailsTab.Current, this.Accessinfo.UserID);
            licencingDetailsTab.Cache.SetValue<Licencing.customerReceivedDate>(licencingDetailsTab.Current, this.Accessinfo.BusinessDate);
            licencingDetailsTab.Current.CustomerReceivedUser = this.Accessinfo.UserID;
            licencingDetailsTab.Current.CustomerReceivedDate = this.Accessinfo.BusinessDate;
            this.licencingDetailsTab.Update(licencingDetailsTab.Current);
            this.Persist();
            this.Actions.PressSave();

            Print.SetEnabled(false);
            Send.SetEnabled(false);
            Transfer.SetEnabled(false);
            Receive.SetEnabled(false);
            Release.SetEnabled(false);
            CustomerReceive.SetEnabled(false);
            return adapter.Get();
        }
    }
}