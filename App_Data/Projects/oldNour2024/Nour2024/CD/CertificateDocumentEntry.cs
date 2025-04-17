using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.SO;
using PX.Objects.PO;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.CM.Extensions;
using Maintenance.CO;


namespace Maintenance.CD
{
    public class CertificateDocumentEntry : PXGraph<CertificateDocumentEntry, CertificateDocument>
    {
        public PXSetup<CDSetUp> AutoNumSetup;
        public PXSelect<CertificateDocument> certificateDocument;
        public PXSelect<CertificateDocument, Where<CertificateDocument.refNbr, Equal<Current<CertificateDocument.refNbr>>>> certificateDocumentActionsTab;
        public PXSelect<CertificateDocument, Where<CertificateDocument.refNbr, Equal<Current<CertificateDocument.refNbr>>>> finSettingsTab;

        #region smart panel
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelectJoin<SOLineSplit, InnerJoin<SOOrder, On<SOOrder.orderNbr, Equal<SOLineSplit.orderNbr>, And<SOOrder.orderType, Equal<SOLineSplit.orderType>>>>, Where<SOOrder.customerID, Equal<Current<CertificateDocument.customerID>>, And<SOOrder.status, Equal<SOOrderStatus.completed>,And<SOLineSplit.lotSerialNbr,IsNotNull>>>> sOLineSplit;
        
        public PXAction<CertificateDocument> AddVechile;
        [PXButton(Tooltip = "")]
        [PXUIField(DisplayName = "Add Vechile", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
        [PXLookupButton]
        public virtual IEnumerable addVechile(PXAdapter adapter)
        {
            if (sOLineSplit.AskExt(true) == WebDialogResult.OK)
            {
                return AddVechileOrder(adapter);
            }
            sOLineSplit.Cache.Clear();
            return adapter.Get();
        }


        public PXAction<CertificateDocument> addVechileOrder;
        [PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable AddVechileOrder(PXAdapter adapter)
        {

                SOLineSplit line = (SOLineSplit)sOLineSplit.Cache.Current;
                if (line != null && !String.IsNullOrEmpty(line.LotSerialNbr+""))
                {
                    
                    PXSelectBase<POReceiptLineSplit> reciepts = new PXSelectReadonly<POReceiptLineSplit, Where<POReceiptLineSplit.lotSerialNbr, Equal<Required<POReceiptLineSplit.lotSerialNbr>>>>(this);
                    reciepts.Cache.ClearQueryCache();
                    POReceiptLineSplit receipt = reciepts.Select(line.LotSerialNbr);
                    POReceiptLineSplitExt receiptExt = PXCache<POReceiptLineSplit>.GetExtension<POReceiptLineSplitExt>(receipt);
                    if (receipt != null)
                    {
                        if (!string.IsNullOrEmpty(line.LotSerialNbr+""))
                        {
                            certificateDocument.Cache.SetValue<CertificateDocument.inventoryID>(certificateDocument.Current, line.InventoryID);
                            certificateDocument.Current.InventoryID = line.InventoryID;
                            certificateDocument.Cache.RaiseFieldUpdated<CertificateDocument.inventoryID>(certificateDocument.Current, null);

                            certificateDocument.Cache.SetValue<CertificateDocument.lotSerialNbr>(certificateDocument.Current, line.LotSerialNbr);
                            certificateDocument.Current.LotSerialNbr = line.LotSerialNbr;
                    
                            certificateDocument.Cache.SetValue<CertificateDocument.invoiceNbr>(certificateDocument.Current, line.OrderNbr);
                            certificateDocument.Current.InvoiceNbr = line.OrderNbr;
                    
                            certificateDocument.Cache.SetValue<CertificateDocument.modelYear>(certificateDocument.Current, receiptExt.UsrModelYear);
                            certificateDocument.Current.ModelYear = receiptExt.UsrModelYear;
                    
                            certificateDocument.Cache.SetValue<CertificateDocument.color>(certificateDocument.Current, receiptExt.UsrColor);
                            certificateDocument.Current.Color = receiptExt.UsrColor;
                    
                            this.certificateDocument.Update(certificateDocument.Current);
                            this.Persist();

                        }
                    }
                    


                }
            
            
            sOLineSplit.Cache.Clear();
            return adapter.Get();
        }
        #endregion

        public CertificateDocumentEntry()
        {
            CDSetUp setup = AutoNumSetup.Current;
           
           ActionsMenu.AddMenuAction(Print);
           ActionsMenu.AddMenuAction(Send);
           ActionsMenu.AddMenuAction(Receive);
           ActionsMenu.AddMenuAction(Release);
           ActionsMenu.AddMenuAction(Transfer);
           ActionsMenu.AddMenuAction(CustomerReceive);
           ActionsMenu.MenuAutoOpen = true;

           ClosingMenu.AddMenuAction(PendCloseout);
           ClosingMenu.AddMenuAction(Closeout);
           ClosingMenu.MenuAutoOpen = true;
           
        }
       #region Buttons
       
       public PXAction<CertificateDocument> ActionsMenu;
       [PXButton]
       [PXUIField(DisplayName = "Actions")]
       protected virtual void actionsMenu()
       {
       }

       public PXAction<CertificateDocument> ClosingMenu;
       [PXButton]
       [PXUIField(DisplayName = "Closing")]
       protected virtual void closingMenu()
       {
       }

       public PXAction<CertificateDocument> Print;
       [PXUIField(DisplayName = "Print",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable print(PXAdapter adapter)
       {

           certificateDocument.Cache.SetValue<CertificateDocument.status>(certificateDocument.Current, CertificateStatus.Printed);
           certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.printUser>(certificateDocumentActionsTab.Current, this.Accessinfo.UserID);
           certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.printDate>(certificateDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
           certificateDocumentActionsTab.Current.PrintUser = this.Accessinfo.UserID;
           certificateDocumentActionsTab.Current.PrintDate = this.Accessinfo.BusinessDate;
           this.certificateDocumentActionsTab.Update(certificateDocumentActionsTab.Current);
           this.Persist();
           this.Actions.PressSave();
            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            Print.SetEnabled(false);
            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            Send.SetEnabled(true);
           

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

           throw new PXReportRequiredException(parameters, "AE809045", "Report AE809045");


           
          // return adapter.Get();
       }

       public PXAction<CertificateDocument> Pay;
       [PXUIField(DisplayName = "Pay",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable pay(PXAdapter adapter)
       {

           certificateDocument.Cache.SetValue<CertificateDocument.payed>(certificateDocument.Current, true);
           this.certificateDocumentActionsTab.Update(certificateDocumentActionsTab.Current);
           this.Persist();
           this.Actions.PressSave();

           return adapter.Get();
       }

       public PXAction<CertificateDocument> Send;
       [PXUIField(DisplayName = "Send",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable send(PXAdapter adapter)
       {
           certificateDocument.Cache.SetValue<CertificateDocument.status>(certificateDocument.Current, CertificateStatus.Sended);
           certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.sendUser>(certificateDocumentActionsTab.Current, this.Accessinfo.UserID);
           certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.sendDate>(certificateDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
           certificateDocumentActionsTab.Current.SendUser = this.Accessinfo.UserID;
           certificateDocumentActionsTab.Current.SendDate = this.Accessinfo.BusinessDate;
           this.certificateDocumentActionsTab.Update(certificateDocumentActionsTab.Current);
           this.Persist();
           this.Actions.PressSave();

            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            Print.SetEnabled(false);
            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            Send.SetEnabled(false);
            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            Receive.SetEnabled(true);
           

           return adapter.Get();
       }


       public PXAction<CertificateDocument> Receive;
       [PXUIField(DisplayName = "Receive",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable receive(PXAdapter adapter)
       {
           if (!String.IsNullOrEmpty(this.certificateDocument.Current.RealCertificateDocumentNbr + ""))
           {
               certificateDocument.Cache.SetValue<CertificateDocument.status>(certificateDocument.Current, CertificateStatus.Received);
               certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.receiveUser>(certificateDocumentActionsTab.Current, this.Accessinfo.UserID);
               certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.receiveDate>(certificateDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
               certificateDocumentActionsTab.Current.ReceiveUser = this.Accessinfo.UserID;
               certificateDocumentActionsTab.Current.ReceiveDate = this.Accessinfo.BusinessDate;
               this.certificateDocumentActionsTab.Update(certificateDocumentActionsTab.Current);
               this.Actions.PressSave();
                // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                Print.SetEnabled(false);
                // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                Send.SetEnabled(false);
                // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                Receive.SetEnabled(false);
                // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                Release.SetEnabled(true);
           }
           else {
               PXUIFieldAttribute.SetError<CertificateDocument.realCertificateDocumentNbr>(this.certificateDocument.Cache, this.certificateDocument.Current, "RealCertificateDocumentNbr must be entered before receive !");
           }
           return adapter.Get();
       }

       public PXAction<CertificateDocument> Release;
       [PXUIField(DisplayName = "Release",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable release(PXAdapter adapter)
       {

           /**
           //create journal Transcation  Debit/Account Receivable   Credit / Company
           //Get Branch-Base-Currency
           PXSelectBase<Company> company = new PXSelect<Company, Where<Company.companyCD, Equal<Required<Company.companyCD>>>>(this);
           company.Cache.ClearQueryCache();
           Company com = company.Select(this.Accessinfo.CompanyName);


           JournalEntry glGrph = PXGraph.CreateInstance<JournalEntry>();
           Batch batch = new Batch();
           batch.Module = "GL";
           batch.BatchNbr = "-1";
           batch.BatchType = BatchTypeCode.Normal;
           batch.BranchID = this.Accessinfo.BranchID;
           batch.Description = this.certificateDocument.Current.Descr;
           batch.CuryID = com.BaseCuryID;
           
           CurrencyInfo infocopy = new CurrencyInfo();
           infocopy = glGrph.currencyinfo.Insert(infocopy) ?? infocopy;
           batch.CuryInfoID = infocopy.CuryInfoID;

           glGrph.BatchModule.Current = batch;
           glGrph.BatchModule.Insert(batch);
           glGrph.Persist();
           glGrph.Actions.PressSave();


           GLSetup glSetup = PXSelectReadonly<GLSetup>.Select(this);

           //debit
           GLTran debitTran = new GLTran();
           //query the Customer account >> better query customer then customer class the AR Account attached to customer class
           PXSelectBase<ARSetup> arSetup = new PXSelectReadonly<ARSetup>(this);
           arSetup.Cache.ClearQueryCache();
           ARSetup ars = arSetup.Select();
           PXSelectBase<CustomerClass> customerClass = new PXSelectReadonly<CustomerClass, Where<CustomerClass.customerClassID, Equal<Required<CustomerClass.customerClassID>>>>(this);
           customerClass.Cache.ClearQueryCache();
           CustomerClass cc = customerClass.Select(ars.DfltCustomerClassID);


           debitTran.AccountID = cc.ARAcctID;
           debitTran.SubID = cc.ARSubID;
           debitTran.DebitAmt = (decimal)this.certificateDocument.Current.DocumentAmount;
           debitTran.LineNbr = 1;


           //credit
           GLTran creditTran = new GLTran();
           creditTran.AccountID = this.certificateDocument.Current.AccountID;
           creditTran.SubID = glSetup.DefaultSubID;
           creditTran.CreditAmt = (decimal)this.certificateDocument.Current.DocumentAmount;
           creditTran.LineNbr = 2;
           glGrph.GLTranModuleBatNbr.Insert(debitTran);
           glGrph.Persist(typeof(GLTran), PXDBOperation.Insert);
           glGrph.GLTranModuleBatNbr.Insert(creditTran);
           glGrph.Persist(typeof(GLTran), PXDBOperation.Insert);
           glGrph.Actions.PressSave();
           **/

           ARInvoiceEntry arGrph = PXGraph.CreateInstance<ARInvoiceEntry>();
           ARInvoice doc = new ARInvoice();
           doc.DocType = ARDocType.DebitMemo;
           doc.RefNbr = "-548645354";
           doc.CustomerID = this.certificateDocument.Current.CustomerID;
           doc.DueDate = this.Accessinfo.BusinessDate;
           doc.DiscDate = this.Accessinfo.BusinessDate;
           CurrencyInfo infocopy = new CurrencyInfo();
           infocopy = arGrph.currencyinfo.Insert(infocopy) ?? infocopy;
           doc.CuryInfoID = infocopy.CuryInfoID;

           //arGrph.Document.Current = doc;
           arGrph.Document.Insert(doc);
           arGrph.Persist();
           arGrph.Actions.PressSave();


           ARTran tran = new ARTran();
           //query the Customer account >> better query customer then customer class the AR Account attached to customer class
           PXSelectBase<ARSetup> arSetup = new PXSelectReadonly<ARSetup>(this);
           arSetup.Cache.ClearQueryCache();
           ARSetup ars = arSetup.Select();
           PXSelectBase<CustomerClass> customerClass = new PXSelectReadonly<CustomerClass, Where<CustomerClass.customerClassID, Equal<Required<CustomerClass.customerClassID>>>>(this);
           customerClass.Cache.ClearQueryCache();
           CustomerClass cc = customerClass.Select(ars.DfltCustomerClassID);

           
           tran.BranchID = this.Accessinfo.BranchID;
           tran.Qty = 1;
           if (this.certificateDocument.Current.Type == CertificateType.Extension)
           {
               tran.CuryUnitPrice = (decimal)this.certificateDocument.Current.ExtensionAmt;
           }
           else {
               tran.CuryUnitPrice = (decimal)this.certificateDocument.Current.DocumentAmount;
           }
           tran.AccountID = this.certificateDocument.Current.AccountID;
           tran.SubID = cc.ARSubID;
           arGrph.Transactions.Insert(tran);
           arGrph.Persist(typeof(ARTran), PXDBOperation.Insert);
           arGrph.Actions.PressSave();

           certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.gLBatchNbr>(this.finSettingsTab.Current, arGrph.Document.Current.RefNbr);
           this.finSettingsTab.Current.GLBatchNbr = arGrph.Document.Current.RefNbr;
           this.finSettingsTab.Update(this.finSettingsTab.Current);
           this.Persist();
            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            PXUIFieldAttribute.SetVisible<CertificateDocument.toSiteID>(this.certificateDocument.Cache, this.certificateDocument.Current, true);


           certificateDocument.Cache.SetValue<CertificateDocument.status>(certificateDocument.Current, CertificateStatus.Released);
           certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.releaseUser>(certificateDocumentActionsTab.Current, this.Accessinfo.UserID);
           certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.releaseDate>(certificateDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
           certificateDocumentActionsTab.Current.ReleaseUser = this.Accessinfo.UserID;
           certificateDocumentActionsTab.Current.ReleaseDate = this.Accessinfo.BusinessDate;
           this.certificateDocumentActionsTab.Update(certificateDocumentActionsTab.Current);
           this.Actions.PressSave();
            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            Print.SetEnabled(false);
            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            Send.SetEnabled(false);
            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            Receive.SetEnabled(false);
            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            Release.SetEnabled(false);
            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            Transfer.SetEnabled(true);
            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            CustomerReceive.SetEnabled(true);

           return adapter.Get();
       }

       public PXAction<CertificateDocument> Transfer;
       [PXUIField(DisplayName = "Transfer",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable transfer(PXAdapter adapter)
       {
           if (!String.IsNullOrEmpty(this.certificateDocument.Current.ToSiteID + ""))
           {
               certificateDocument.Cache.SetValue<CertificateDocument.status>(certificateDocument.Current, CertificateStatus.Transfered);
               certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.transferUser>(certificateDocumentActionsTab.Current, this.Accessinfo.UserID);
               certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.transferDate>(certificateDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
               certificateDocumentActionsTab.Current.TransferUser = this.Accessinfo.UserID;
               certificateDocumentActionsTab.Current.TransferDate = this.Accessinfo.BusinessDate;
               this.certificateDocumentActionsTab.Update(certificateDocumentActionsTab.Current);
               this.Actions.PressSave();

                // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                Print.SetEnabled(false);
                // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                Send.SetEnabled(false);
                // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                Receive.SetEnabled(false);
                // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                Release.SetEnabled(false);
                // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                Transfer.SetEnabled(false);
                // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                CustomerReceive.SetEnabled(true);
           }
           else {
               PXUIFieldAttribute.SetError<CertificateDocument.toSiteID>(this.certificateDocument.Cache, this.certificateDocument.Current, "New Branch must be entered before receive !");
           }
           
           
           
           return adapter.Get();
       }

       public PXAction<CertificateDocument> CustomerReceive;
       [PXUIField(DisplayName = "CustomerReceive",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable customerReceive(PXAdapter adapter)
       {
           certificateDocument.Cache.SetValue<CertificateDocument.status>(certificateDocument.Current, CertificateStatus.CustomerReceived);
           certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.customerReceivedUser>(certificateDocumentActionsTab.Current, this.Accessinfo.UserID);
           certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.customerReceivedDate>(certificateDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
           certificateDocumentActionsTab.Current.CustomerReceivedUser = this.Accessinfo.UserID;
           certificateDocumentActionsTab.Current.CustomerReceivedDate = this.Accessinfo.BusinessDate;
           this.certificateDocumentActionsTab.Update(certificateDocumentActionsTab.Current);
           this.Actions.PressSave();

            // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
            PXUIFieldAttribute.SetEnabled(this.certificateDocument.Cache, this.certificateDocument.Current, false);

           return adapter.Get();
       }


       public PXAction<CertificateDocument> ReNew;
       [PXUIField(DisplayName = "Re-New",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable reNew(PXAdapter adapter)
       {

           if (this.certificateDocument.Current.Status == CertificateStatus.CustomerReceived)
           {
               /**
               certificateDocument.Cache.SetValue<CertificateDocument.status>(certificateDocument.Current, CertificateStatus.CustomerReceived);
               certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.customerReceivedUser>(certificateDocumentActionsTab.Current, this.Accessinfo.UserID);
               certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.customerReceivedDate>(certificateDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
               certificateDocumentActionsTab.Current.CustomerReceivedUser = this.Accessinfo.UserID;
               certificateDocumentActionsTab.Current.CustomerReceivedDate = this.Accessinfo.BusinessDate;
               this.certificateDocumentActionsTab.Update(certificateDocumentActionsTab.Current);
               this.Actions.PressSave();
               **/
               CertificateDocumentEntry graph = PXGraph.CreateInstance<CertificateDocumentEntry>();
               CertificateDocument doc = this.certificateDocument.Current;
               doc.RefNbr = "<NEW>";
               doc.Type = CertificateType.ReNew;
               doc.Status = CertificateStatus.NewC;
               doc.InvoicePaymentType = InvoicePaymentTypes.Cash;
               doc.InsuranceCoverageAmt = 0;
               doc.InsuranceRatio = 0;
               doc.DocumentAmount = 0;
               doc.RealCertificateDocumentNbr = "";
               doc.GLBatchNbr = "";
               
               doc.PrintDate= null;
               doc.SendDate = null;
               doc.ReceiveDate = null;
               doc.ReleaseDate = null;
               doc.TransferDate = null;
               doc.CustomerReceivedDate = null;

               doc.PrintUser = null;
               doc.SendUser = null;
               doc.ReceiveUser = null;
               doc.ReleaseUser = null;
               doc.TransferUser = null;
               doc.CustomerReceivedUser = null;

               graph.certificateDocument.Current = doc;
               graph.certificateDocument.Insert(doc);
               
               
               throw new PXRedirectRequiredException(graph, true, "Extended Insurance Certificate Document");

           }
           return adapter.Get();
       }


       public PXAction<CertificateDocument> Extension;
       [PXUIField(DisplayName = "Extension",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable extension(PXAdapter adapter)
       {

           if (this.certificateDocument.Current.Status == CertificateStatus.CustomerReceived)
           {
               /**
               certificateDocument.Cache.SetValue<CertificateDocument.status>(certificateDocument.Current, CertificateStatus.CustomerReceived);
               certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.customerReceivedUser>(certificateDocumentActionsTab.Current, this.Accessinfo.UserID);
               certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.customerReceivedDate>(certificateDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
               certificateDocumentActionsTab.Current.CustomerReceivedUser = this.Accessinfo.UserID;
               certificateDocumentActionsTab.Current.CustomerReceivedDate = this.Accessinfo.BusinessDate;
               this.certificateDocumentActionsTab.Update(certificateDocumentActionsTab.Current);
               this.Actions.PressSave();
               **/
               CertificateDocumentEntry graph = PXGraph.CreateInstance<CertificateDocumentEntry>();
               CertificateDocument doc = this.certificateDocument.Current;
               doc.RefNbr = "<NEW>";
               doc.Type = CertificateType.Extension;
               doc.Status = CertificateStatus.NewC;
               
               doc.GLBatchNbr = "";

               doc.PrintDate = null;
               doc.SendDate = null;
               doc.ReceiveDate = null;
               doc.ReleaseDate = null;
               doc.TransferDate = null;
               doc.CustomerReceivedDate = null;

               doc.PrintUser = null;
               doc.SendUser = null;
               doc.ReceiveUser = null;
               doc.ReleaseUser = null;
               doc.TransferUser = null;
               doc.CustomerReceivedUser = null;

               graph.certificateDocument.Current = doc;
               graph.certificateDocument.Insert(doc);


               throw new PXRedirectRequiredException(graph, true, "Extended Insurance Certificate Document");

           }
           return adapter.Get();
       }

       public PXAction<CertificateDocument> PendCloseout;
       [PXUIField(DisplayName = "Pend Document to Close Out",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable pendCloseout(PXAdapter adapter)
       {

           if (this.certificateDocument.Current.Status == CertificateStatus.PendingClosedOut && (String.IsNullOrEmpty(this.certificateDocument.Current.CloseoutONDate + "") || String.IsNullOrEmpty(this.certificateDocument.Current.CloseoutAmt + "")))
           {
               return adapter.Get();
           }
           if (this.certificateDocument.Current.Status != CertificateStatus.CustomerReceived && this.certificateDocument.Current.Status != CertificateStatus.PendingClosedOut)
           {
               PXUIFieldAttribute.SetError<CertificateDocument.status>(this.certificateDocument.Cache, this.certificateDocument.Current, "close-out must be done after Customer Received !");
               return adapter.Get();
           }


           certificateDocument.Cache.SetValue<CertificateDocument.status>(certificateDocument.Current, CertificateStatus.PendingClosedOut);
           certificateDocument.Current.Status = CertificateStatus.PendingClosedOut;
           object status = certificateDocument.Current.Status;
           this.certificateDocument.Cache.RaiseFieldUpdated<CertificateDocument.status>(this.certificateDocument.Current, CertificateStatus.CustomerReceived);
           this.certificateDocumentActionsTab.Update(certificateDocumentActionsTab.Current);
           this.Actions.PressSave();

           
           this.Actions.PressSave();
           return adapter.Get();
       }



       public PXAction<CertificateDocument> Closeout;
       [PXUIField(DisplayName = "Close Out",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable closeout(PXAdapter adapter)
       {

           if (this.certificateDocument.Current.Status == CertificateStatus.PendingClosedOut && !String.IsNullOrEmpty(this.certificateDocument.Current.CloseoutONDate + "") && !String.IsNullOrEmpty(this.certificateDocument.Current.CloseoutAmt + ""))
           {
               
               int ChosenAndStart = DateTime.Compare((DateTime)this.certificateDocument.Current.CloseoutONDate, (DateTime)this.certificateDocument.Current.StartDate);//greater or equal to 0
               int ChosenAndEnd = DateTime.Compare((DateTime)this.certificateDocument.Current.CloseoutONDate, (DateTime)this.certificateDocument.Current.EndDate);   //less or equal to zero
               
               if ((ChosenAndStart > 0 || ChosenAndStart == 0) && (ChosenAndEnd < 0 || ChosenAndEnd == 0))
               {
                   ARInvoiceEntry arGrph = PXGraph.CreateInstance<ARInvoiceEntry>();
                   ARInvoice doc = new ARInvoice();
                   doc.DocType = ARDocType.CreditMemo;
                   doc.RefNbr = "-548645354";
                   doc.CustomerID = this.certificateDocument.Current.CustomerID;
                   doc.DueDate = this.Accessinfo.BusinessDate;
                   doc.DiscDate = this.Accessinfo.BusinessDate;
                   CurrencyInfo infocopy = new CurrencyInfo();
                   infocopy = arGrph.currencyinfo.Insert(infocopy) ?? infocopy;
                   doc.CuryInfoID = infocopy.CuryInfoID;

                   //arGrph.Document.Current = doc;
                   arGrph.Document.Insert(doc);
                   arGrph.Persist();
                   arGrph.Actions.PressSave();


                   ARTran tran = new ARTran();
                   //query the Customer account >> better query customer then customer class the AR Account attached to customer class
                   PXSelectBase<ARSetup> arSetup = new PXSelectReadonly<ARSetup>(this);
                   arSetup.Cache.ClearQueryCache();
                   ARSetup ars = arSetup.Select();
                   PXSelectBase<CustomerClass> customerClass = new PXSelectReadonly<CustomerClass, Where<CustomerClass.customerClassID, Equal<Required<CustomerClass.customerClassID>>>>(this);
                   customerClass.Cache.ClearQueryCache();
                   CustomerClass cc = customerClass.Select(ars.DfltCustomerClassID);


                   tran.BranchID = this.Accessinfo.BranchID;
                   tran.Qty = 1;
                   tran.CuryUnitPrice = (decimal)this.certificateDocument.Current.CloseoutAmt;
                   tran.AccountID = this.certificateDocument.Current.AccountID;
                   tran.SubID = cc.ARSubID;

                   arGrph.Transactions.Insert(tran);
                   arGrph.Persist(typeof(ARTran), PXDBOperation.Insert);
                   arGrph.Actions.PressSave();

                   certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.closeBatchNbr>(this.finSettingsTab.Current, arGrph.Document.Current.RefNbr);
                   this.finSettingsTab.Current.CloseBatchNbr = arGrph.Document.Current.RefNbr;
                   this.finSettingsTab.Update(this.finSettingsTab.Current);
                   this.Persist();
                    // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                    Print.SetEnabled(false);
                    // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                    Send.SetEnabled(false);
                    // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                    Receive.SetEnabled(false);
                    // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                    Release.SetEnabled(false);

                    // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                    Transfer.SetEnabled(true);
                    // Acuminator disable once PX1089 UiPresentationLogicInActionDelegates [Justification]
                    CustomerReceive.SetEnabled(true);


                   certificateDocument.Cache.SetValue<CertificateDocument.status>(certificateDocument.Current, CertificateStatus.ClosedOut);
                   certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.closedoutUser>(certificateDocumentActionsTab.Current, this.Accessinfo.UserID);
                   certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.closedoutDate>(certificateDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
                   certificateDocumentActionsTab.Current.ClosedoutUser = this.Accessinfo.UserID;
                   certificateDocumentActionsTab.Current.ClosedoutDate = this.Accessinfo.BusinessDate;
                   this.certificateDocumentActionsTab.Update(certificateDocumentActionsTab.Current);
                   this.Actions.PressSave();
               }
               else
               {
                   PXUIFieldAttribute.SetError<CertificateDocument.closeoutONDate>(this.certificateDocument.Cache, this.certificateDocument.Current, "close-out Date must be  be between Start And End Date !");
               }    
           }
           
           

           return adapter.Get();
       }
        
       /**
       public PXAction<CertificateDocument> Closeout;
       [PXUIField(DisplayName = "Close Out",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable closeout(PXAdapter adapter)
       {

           if (this.certificateDocument.Current.Status == CertificateStatus.PendingClosedOut && (String.IsNullOrEmpty(this.certificateDocument.Current.CloseoutONDate + "") || String.IsNullOrEmpty(this.certificateDocument.Current.CloseoutAmt + "")) )
           {
               return adapter.Get();
           }
           if (this.certificateDocument.Current.Status != CertificateStatus.CustomerReceived && this.certificateDocument.Current.Status != CertificateStatus.PendingClosedOut)
           {
               PXUIFieldAttribute.SetError<CertificateDocument.status>(this.certificateDocument.Cache, this.certificateDocument.Current, "close-out must be done after Customer Received !");
               return adapter.Get();
           }


           certificateDocument.Cache.SetValue<CertificateDocument.status>(certificateDocument.Current, CertificateStatus.PendingClosedOut);
           certificateDocument.Current.Status = CertificateStatus.PendingClosedOut;
           object status = certificateDocument.Current.Status;
           this.certificateDocument.Cache.RaiseFieldUpdated<CertificateDocument.status>(this.certificateDocument.Current, CertificateStatus.CustomerReceived);

           if (String.IsNullOrEmpty(this.certificateDocument.Current.CloseoutONDate+""))
           {
               return adapter.Get();
           }
           

           int ChosenAndStart = DateTime.Compare((DateTime)this.certificateDocument.Current.CloseoutONDate,(DateTime)this.certificateDocument.Current.StartDate);//greater or equal to 0
           int ChosenAndEnd = DateTime.Compare((DateTime)this.certificateDocument.Current.CloseoutONDate, (DateTime)this.certificateDocument.Current.EndDate);   //less or equal to zero

           if ((ChosenAndStart > 0 || ChosenAndStart == 0) && (ChosenAndEnd < 0 || ChosenAndEnd == 0))
           {
               ARInvoiceEntry arGrph = PXGraph.CreateInstance<ARInvoiceEntry>();
               ARInvoice doc = new ARInvoice();
               doc.DocType = ARDocType.CreditMemo;
               doc.RefNbr = "-548645354";
               doc.CustomerID = this.certificateDocument.Current.CustomerID;
               doc.DueDate = this.Accessinfo.BusinessDate;
               doc.DiscDate = this.Accessinfo.BusinessDate;
               CurrencyInfo infocopy = new CurrencyInfo();
               infocopy = arGrph.currencyinfo.Insert(infocopy) ?? infocopy;
               doc.CuryInfoID = infocopy.CuryInfoID;

               //arGrph.Document.Current = doc;
               arGrph.Document.Insert(doc);
               arGrph.Persist();
               arGrph.Actions.PressSave();


               ARTran tran = new ARTran();
               //query the Customer account >> better query customer then customer class the AR Account attached to customer class
               PXSelectBase<ARSetup> arSetup = new PXSelectReadonly<ARSetup>(this);
               arSetup.Cache.ClearQueryCache();
               ARSetup ars = arSetup.Select();
               PXSelectBase<CustomerClass> customerClass = new PXSelectReadonly<CustomerClass, Where<CustomerClass.customerClassID, Equal<Required<CustomerClass.customerClassID>>>>(this);
               customerClass.Cache.ClearQueryCache();
               CustomerClass cc = customerClass.Select(ars.DfltCustomerClassID);


               tran.BranchID = this.Accessinfo.BranchID;
               tran.Qty = 1;
               tran.CuryUnitPrice = (decimal)this.certificateDocument.Current.CloseoutAmt;
               tran.AccountID = this.certificateDocument.Current.AccountID;
               tran.SubID = cc.ARSubID;
               
               arGrph.Transactions.Insert(tran);
               arGrph.Persist(typeof(ARTran), PXDBOperation.Insert);
               arGrph.Actions.PressSave();

               certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.closeBatchNbr>(this.finSettingsTab.Current, arGrph.Document.Current.RefNbr);
               this.finSettingsTab.Current.CloseBatchNbr = arGrph.Document.Current.RefNbr;
               this.finSettingsTab.Update(this.finSettingsTab.Current);
               this.Persist();
               
               Print.SetEnabled(false);
               Send.SetEnabled(false);
               Receive.SetEnabled(false);
               Release.SetEnabled(false);
               Transfer.SetEnabled(true);
               CustomerReceive.SetEnabled(true);


               certificateDocument.Cache.SetValue<CertificateDocument.status>(certificateDocument.Current, CertificateStatus.ClosedOut);
               certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.closedoutUser>(certificateDocumentActionsTab.Current, this.Accessinfo.UserID);
               certificateDocumentActionsTab.Cache.SetValue<CertificateDocument.closedoutDate>(certificateDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
               certificateDocumentActionsTab.Current.ClosedoutUser = this.Accessinfo.UserID;
               certificateDocumentActionsTab.Current.ClosedoutDate = this.Accessinfo.BusinessDate;
               this.certificateDocumentActionsTab.Update(certificateDocumentActionsTab.Current);
               this.Actions.PressSave();
           }else {
               PXUIFieldAttribute.SetError<CertificateDocument.closeoutONDate>(this.certificateDocument.Cache, this.certificateDocument.Current, "close-out Date must be  be between Start And End Date !");
           }
           
           return adapter.Get();
       }
       **/
       public PXAction<CertificateDocument> Compensate;
       [PXUIField(DisplayName = "Report Accident",
       MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable compensate(PXAdapter adapter)
       {
           if (this.certificateDocument.Current.Status != CertificateStatus.CustomerReceived)
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                throw new PXException("Must be Customer Received !");
           CompensationDocEntry graph = PXGraph.CreateInstance<CompensationDocEntry>();
           CompensationDocument doc = new CompensationDocument();
           doc.RefNbr = "<NEW>";
           doc.InsurranceCertificateRefNbr = this.certificateDocument.Current.RefNbr;
           doc.CustomerID = this.certificateDocument.Current.CustomerID;
           doc.StartDate = this.certificateDocument.Current.StartDate;
           doc.EndDate = this.certificateDocument.Current.EndDate;
           graph.compensationDocument.Current = doc;
           graph.compensationDocument.Insert(doc);
           throw new PXRedirectRequiredException(graph, true, "Compensation Details");
           //return adapter.Get();
       }
       
       
       #endregion

      #region Row Actions
       protected virtual void CertificateDocument_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
       {

           CertificateDocument row = e.Row as CertificateDocument;
           if (row != null)
           {

               if (row.InvoicePaymentType == InvoicePaymentTypes.Cash)
               {
                   PXUIFieldAttribute.SetVisible<CertificateDocument.bankName>(sender, row, false);
                   PXUIFieldAttribute.SetVisible<CertificateDocument.bankBranch>(sender, row, false);
                   PXUIFieldAttribute.SetVisible<CertificateDocument.installmentPeriodYear>(sender, row, false);
                   PXUIFieldAttribute.SetVisible<CertificateDocument.installmentPeriodMonth>(sender, row, false);
                   PXUIFieldAttribute.SetVisible<CertificateDocument.demandDate>(sender, row, false);

               }
               else
               {
                   PXUIFieldAttribute.SetVisible<CertificateDocument.bankName>(sender, row, true);
                   PXUIFieldAttribute.SetVisible<CertificateDocument.bankBranch>(sender, row, true);
                   PXUIFieldAttribute.SetVisible<CertificateDocument.installmentPeriodYear>(sender, row, true);
                   PXUIFieldAttribute.SetVisible<CertificateDocument.installmentPeriodMonth>(sender, row, true);
                   PXUIFieldAttribute.SetVisible<CertificateDocument.demandDate>(sender, row, true);

               }

               if (row.Status == CertificateStatus.Released)
               {
                   PXUIFieldAttribute.SetVisible<CertificateDocument.toSiteID>(sender, row, true);
               }
               else {
                   PXUIFieldAttribute.SetVisible<CertificateDocument.toSiteID>(sender, row, false);
               }

               if (row.Status == CertificateStatus.CustomerReceived)
               {
                   PXUIFieldAttribute.SetEnabled(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   /**
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.closeoutONDate>(this.certificateDocument.Cache, this.certificateDocument.Current, true);
                   PXUIFieldAttribute.SetWarning<CertificateDocument.closeoutONDate>(this.certificateDocument.Cache, this.certificateDocument.Current, "close-out Date Must Be Entered Before Closing out The Insurance Doc.");
                   PXUIFieldAttribute.SetWarning<CertificateDocument.closeoutAmt>(this.certificateDocument.Cache, this.certificateDocument.Current, "close-out Amount Must Be Entered Before Closing out The Insurance Doc.");
                   //PXUIFieldAttribute.SetEnabled<CertificateDocument.closeoutAmt>(this.certificateDocument.Cache, this.certificateDocument.Current, true);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.type>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.customerID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.customerPhone>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.siteID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.toSiteID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.accountClassID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.accountID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.startDate>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.endDate>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.insuranceCoverageAmt>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.insuranceRatio>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.documentAmount>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.insuranceKind>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.invoicePaymentType>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.bankName>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.bankBranch>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.installmentPeriodYear>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.installmentPeriodMonth>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.demandDate>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.realCertificateDocumentNbr>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.descr>(this.certificateDocument.Cache, this.certificateDocument.Current, false);

                   PXUIFieldAttribute.SetEnabled<CertificateDocument.extensionReason>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.extensionAmt>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   **/
               }
               
               if (row.Status == CertificateStatus.ClosedOut)
               {
                   PXUIFieldAttribute.SetEnabled(this.certificateDocument.Cache, this.certificateDocument.Current, false);
               }
               
               if (row.Status == CertificateStatus.PendingClosedOut)
               {
                   PXUIFieldAttribute.SetEnabled(this.certificateDocument.Cache, this.certificateDocument.Current, true);
                   
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.closeoutAmt>(this.certificateDocument.Cache, this.certificateDocument.Current, true);
                   PXUIFieldAttribute.SetWarning<CertificateDocument.closeoutONDate>(this.certificateDocument.Cache, this.certificateDocument.Current, "close-out Date Must Be Entered Before Closing out The Insurance Doc.");
                   PXUIFieldAttribute.SetWarning<CertificateDocument.closeoutAmt>(this.certificateDocument.Cache, this.certificateDocument.Current, "close-out Amount Must Be Entered Before Closing out The Insurance Doc.");

                   PXUIFieldAttribute.SetEnabled<CertificateDocument.type>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.customerID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.customerPhone>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.siteID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.toSiteID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.accountClassID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.accountID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.startDate>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.endDate>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.insuranceCoverageAmt>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.insuranceRatio>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.documentAmount>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.insuranceKind>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.invoicePaymentType>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.bankName>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.bankBranch>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.installmentPeriodYear>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.installmentPeriodMonth>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.demandDate>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.realCertificateDocumentNbr>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.descr>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.extensionReason>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.extensionAmt>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   

               }

               if (row.Status == CertificateStatus.NewC) {
                   Print.SetEnabled(true);
                   Send.SetEnabled(false);
                   Receive.SetEnabled(false);
                   Release.SetEnabled(false);
                   Transfer.SetEnabled(false);
                   CustomerReceive.SetEnabled(false);
               }

               if (row.Status == CertificateStatus.Printed)
               {
                   Print.SetEnabled(false);
                   Send.SetEnabled(true);
                   Receive.SetEnabled(false);
                   Release.SetEnabled(false);
                   Transfer.SetEnabled(false);
                   CustomerReceive.SetEnabled(false);
               }

               if (row.Status == CertificateStatus.Sended)
               {
                   Print.SetEnabled(false);
                   Send.SetEnabled(false);
                   Receive.SetEnabled(true);
                   Release.SetEnabled(false);
                   Transfer.SetEnabled(false);
                   CustomerReceive.SetEnabled(false);
               }

               if (row.Status == CertificateStatus.Received)
               {
                   Print.SetEnabled(false);
                   Send.SetEnabled(false);
                   Receive.SetEnabled(false);
                   Release.SetEnabled(true);
                   Transfer.SetEnabled(false);
                   CustomerReceive.SetEnabled(false);
               }

               if (row.Status == CertificateStatus.Released)
               {
                   Print.SetEnabled(false);
                   Send.SetEnabled(false);
                   Receive.SetEnabled(false);
                   Release.SetEnabled(false);
                   Transfer.SetEnabled(true);
                   CustomerReceive.SetEnabled(true);
               }

               if (row.Status == CertificateStatus.Transfered)
               {
                   Print.SetEnabled(false);
                   Send.SetEnabled(false);
                   Receive.SetEnabled(false);
                   Release.SetEnabled(false);
                   Transfer.SetEnabled(false);
                   CustomerReceive.SetEnabled(true);
               }

               if (row.Status == CertificateStatus.CustomerReceived)
               {
                   Print.SetEnabled(false);
                   Send.SetEnabled(false);
                   Receive.SetEnabled(false);
                   Release.SetEnabled(false);
                   Transfer.SetEnabled(false);
                   CustomerReceive.SetEnabled(false);
               }

               if (row.Status == CertificateStatus.ClosedOut || row.Status == CertificateStatus.PendingClosedOut)
               {
                   Print.SetEnabled(false);
                   Send.SetEnabled(false);
                   Receive.SetEnabled(false);
                   Release.SetEnabled(false);
                   Transfer.SetEnabled(false);
                   CustomerReceive.SetEnabled(false);
               }

               if (row.Type == CertificateType.Extension)
               {
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.closeoutONDate>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.closeoutAmt>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.type>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.customerID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.customerPhone>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.siteID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.toSiteID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.accountClassID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.accountID>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.startDate>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.endDate>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.insuranceCoverageAmt>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.insuranceRatio>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.documentAmount>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.insuranceKind>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.invoicePaymentType>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.bankName>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.bankBranch>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.installmentPeriodYear>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.installmentPeriodMonth>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.demandDate>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.realCertificateDocumentNbr>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.descr>(this.certificateDocument.Cache, this.certificateDocument.Current, false);
                   
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.extensionReason>(this.certificateDocument.Cache, this.certificateDocument.Current, true);
                   PXUIFieldAttribute.SetEnabled<CertificateDocument.extensionAmt>(this.certificateDocument.Cache, this.certificateDocument.Current, true);


               }

           }

       }

       protected virtual void _(Events.RowInserted<CertificateDocument> e)
       { 
          if(e.Row != null){
             CertificateDocument doc=e.Row as CertificateDocument;
              if (doc.Type ==  CertificateType.Extension)
              {

                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    Print.SetEnabled(true);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    Send.SetEnabled(false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    Receive.SetEnabled(false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    Release.SetEnabled(false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    Transfer.SetEnabled(false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    CustomerReceive.SetEnabled(false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetEnabled(this.certificateDocument.Cache, doc, true);

              }
          }
       }
       #endregion

      #region Field Actions
       
      protected virtual void CertificateDocument_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
       {
           CertificateDocument row = e.Row as CertificateDocument;
           if (row != null && !String.IsNullOrEmpty(row.CustomerID+""))
           {
               PXSelectBase<Customer> res = new PXSelectReadonly<Customer,Where<Customer.bAccountID,Equal<Required<Customer.bAccountID>>>>(this);
               res.Cache.ClearQueryCache();
               Customer cus = res.Select(row.CustomerID);
               if (cus != null) {
                   PXSelectBase<Contact> conRes = new PXSelectReadonly<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>(this);
                   conRes.Cache.ClearQueryCache();
                   Contact con = conRes.Select(cus.DefBillContactID);
                   this.certificateDocument.Cache.SetValue<CertificateDocument.customerPhone>(this.certificateDocument.Current, con.Phone1);
                   this.certificateDocument.Current.CustomerPhone = con.Phone1;

               }
           }
       }

       protected virtual void CertificateDocument_InsuranceCoverageAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
       {
           CertificateDocument row = e.Row as CertificateDocument;
           if (row != null)
           {
               this.certificateDocument.Cache.SetValue<CertificateDocument.documentAmount>(this.certificateDocument.Current, this.certificateDocument.Current.InsuranceCoverageAmt * this.certificateDocument.Current.InsuranceRatio);
               this.certificateDocument.Current.DocumentAmount = this.certificateDocument.Current.InsuranceCoverageAmt * this.certificateDocument.Current.InsuranceRatio;
           }
       }

       protected virtual void CertificateDocument_InsuranceRatio_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
       {
           CertificateDocument row = e.Row as CertificateDocument;
           if (row != null)
           {
               this.certificateDocument.Cache.SetValue<CertificateDocument.documentAmount>(this.certificateDocument.Current, this.certificateDocument.Current.InsuranceCoverageAmt * this.certificateDocument.Current.InsuranceRatio);
               this.certificateDocument.Current.DocumentAmount = this.certificateDocument.Current.InsuranceCoverageAmt * this.certificateDocument.Current.InsuranceRatio;
           }
       }
       #endregion

      #region Field Actions

       protected virtual void CertificateDocument_InvoicePaymentType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
       {

           CertificateDocument row = e.Row as CertificateDocument;
           if (row != null)
           {
               if (row.InvoicePaymentType == InvoicePaymentTypes.Cash)
               {
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.bankName>(sender, row, false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.bankBranch>(sender, row, false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.installmentPeriodYear>(sender, row, false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.installmentPeriodMonth>(sender, row, false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.demandDate>(sender, row, false);
                   
               }
               else
               {
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.bankName>(sender, row, true);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.bankBranch>(sender, row, true);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.installmentPeriodYear>(sender, row, true);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.installmentPeriodMonth>(sender, row, true);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.demandDate>(sender, row, true);
                   
               }

           }

       }      
       protected virtual void CertificateDocument_InvoicePaymentType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
       { 
       
           CertificateDocument row = e.Row as CertificateDocument;
           if (row != null)
           {
               if (row.InvoicePaymentType == InvoicePaymentTypes.Cash)
                {
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.bankName>(sender,row,false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.bankBranch>(sender,row, false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.installmentPeriodYear>(sender,row, false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.installmentPeriodMonth>(sender,row, false);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.demandDate>(sender,row, false);
                   
               }
               else
                {
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.bankName>(sender,row, true);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.bankBranch>(sender,row, true);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.installmentPeriodYear>(sender,row, true);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.installmentPeriodMonth>(sender,row, true);
                    // Acuminator disable once PX1070 UiPresentationLogicInEventHandlers [Justification]
                    PXUIFieldAttribute.SetVisible<CertificateDocument.demandDate>(sender,row, true);
                   
               }

           }
       
       }

      #endregion
    }
}