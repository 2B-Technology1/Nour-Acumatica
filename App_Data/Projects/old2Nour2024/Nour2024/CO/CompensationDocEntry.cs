using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.AR;
using Maintenance.CD;
using PX.Objects.CM.Extensions;


namespace Maintenance.CO
{
    public class CompensationDocEntry : PXGraph<CompensationDocEntry, CompensationDocument>
    {
        public PXSetup<COSetUp> AutoNumSetup;
        public PXSelect<CompensationDocument> compensationDocument;
        public PXSelect<CompensationDocument, Where<CompensationDocument.refNbr, Equal<Current<CompensationDocument.refNbr>>>> compensationDocumentActionsTab;
        public PXSelect<CompensationDocument, Where<CompensationDocument.refNbr, Equal<Current<CompensationDocument.refNbr>>>> finSettingsTab;
       
        
        public CompensationDocEntry() {
            COSetUp setup = AutoNumSetup.Current;
            ActionsMenu.AddMenuAction(Print);
            ActionsMenu.AddMenuAction(Receive);
            ActionsMenu.AddMenuAction(Send);
            ActionsMenu.AddMenuAction(RecieveMaintInvoice);
            ActionsMenu.AddMenuAction(RecieveMaintInvoiceToCompany);
            ActionsMenu.AddMenuAction(CheckRecieve);
            ActionsMenu.AddMenuAction(CheckDeliver);
            ActionsMenu.AddMenuAction(Close);
            ActionsMenu.AddMenuAction(Release);
            

            ActionsMenu.MenuAutoOpen = true;
        }

        #region fields actions
        protected void CompensationDocument_PaymentRef_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CompensationDocument row = (CompensationDocument)e.Row;
            if(row != null){
               PXSelectBase<ARPayment> payRes=new PXSelectReadonly<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>, And<ARPayment.docType, Equal<ARDocType.payment>>>>(this);
               payRes.Cache.ClearQueryCache();
               ARPayment p=payRes.Select(row.PaymentRef);
               if(p != null){
                   row.Cost = (float)p.CuryOrigDocAmt;
               }

            }
             
         }
        #endregion
        
        #region row actions
        protected virtual void CompensationDocument_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row == null) return;

            if(compensationDocument.Current.Status == CompensationStatus.NewC)
            {
                 
                #region text fields enablement
                //print
                PXUIFieldAttribute.SetEnabled<CompensationDocument.insurranceCertificateRefNbr>(this.compensationDocument.Cache, this.compensationDocument.Current,true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerID>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.plateNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.startDate>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.endDate>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.paymentRef>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                //received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDescr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentNotifyDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.planDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverLicenceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received maint invoice
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceCenter>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.bankName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkReceiveDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkAmt>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check delivered
                //add branch
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerDeliveryPlace>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDelivered>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDeliveredDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                #endregion

                
                Print.SetEnabled(true);
                Receive.SetEnabled(false);
                Send.SetEnabled(false);
                RecieveMaintInvoice.SetEnabled(false);
                RecieveMaintInvoiceToCompany.SetEnabled(false);
                CheckRecieve.SetEnabled(false);
                CheckDeliver.SetEnabled(false);
                Close.SetEnabled(false);
                Release.SetEnabled(false);
            }
            else if (compensationDocument.Current.Status == CompensationStatus.Printed)
            {
                #region text fields enablement
                //print
                PXUIFieldAttribute.SetEnabled<CompensationDocument.insurranceCertificateRefNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerID>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.plateNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.startDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.endDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.paymentRef>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDescr>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDate>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentNotifyDate>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseDate>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.planDate>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverName>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverLicenceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                //received maint invoice
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceCenter>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.bankName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkReceiveDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkAmt>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check delivered
                //add branch
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerDeliveryPlace>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDelivered>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDeliveredDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                #endregion

                Print.SetEnabled(false);
                Receive.SetEnabled(true);
                Send.SetEnabled(false);
                RecieveMaintInvoice.SetEnabled(false);
                RecieveMaintInvoiceToCompany.SetEnabled(false);
                CheckRecieve.SetEnabled(false);
                CheckDeliver.SetEnabled(false);
                Close.SetEnabled(false);
                Release.SetEnabled(false);
            }
            else if (compensationDocument.Current.Status == CompensationStatus.Received)
            {

                #region text fields enablement
                //print
                PXUIFieldAttribute.SetEnabled<CompensationDocument.insurranceCertificateRefNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerID>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.plateNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.startDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.endDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.paymentRef>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDescr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentNotifyDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.planDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverLicenceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received maint invoice
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceCenter>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceDate>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                //check received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.bankName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkReceiveDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkAmt>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check delivered
                //add branch
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerDeliveryPlace>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDelivered>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDeliveredDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                #endregion

                Print.SetEnabled(false);
                Receive.SetEnabled(false);
                Send.SetEnabled(true);
                RecieveMaintInvoice.SetEnabled(false);
                RecieveMaintInvoiceToCompany.SetEnabled(false);
                CheckRecieve.SetEnabled(false);
                CheckDeliver.SetEnabled(false);
                Close.SetEnabled(false);
                Release.SetEnabled(false);
            }
            else if (compensationDocument.Current.Status == CompensationStatus.Sended)
            {

                #region text fields enablement
                //print
                PXUIFieldAttribute.SetEnabled<CompensationDocument.insurranceCertificateRefNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerID>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.plateNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.startDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.endDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.paymentRef>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDescr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentNotifyDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.planDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverLicenceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received maint invoice
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceCenter>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceDate>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                //check received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.bankName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkReceiveDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkAmt>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check delivered
                //add branch
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerDeliveryPlace>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDelivered>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDeliveredDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                #endregion

                Print.SetEnabled(false);
                Receive.SetEnabled(false);
                Send.SetEnabled(false);
                RecieveMaintInvoice.SetEnabled(true);
                RecieveMaintInvoiceToCompany.SetEnabled(false);
                CheckRecieve.SetEnabled(false);
                CheckDeliver.SetEnabled(false);
                Close.SetEnabled(false);
                Release.SetEnabled(false);
            }
            else if (compensationDocument.Current.Status == CompensationStatus.RecievedMaintInvoice)
            {

                #region text fields enablement
                //print
                PXUIFieldAttribute.SetEnabled<CompensationDocument.insurranceCertificateRefNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerID>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.plateNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.startDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.endDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.paymentRef>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDescr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentNotifyDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.planDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverLicenceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received maint invoice
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceCenter>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.bankName>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkReceiveDate>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkAmt>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                //check delivered
                //add branch
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerDeliveryPlace>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDelivered>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDeliveredDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                #endregion

                Print.SetEnabled(false);
                Receive.SetEnabled(false);
                Send.SetEnabled(false);
                RecieveMaintInvoice.SetEnabled(false);
                RecieveMaintInvoiceToCompany.SetEnabled(true);
                CheckRecieve.SetEnabled(false);
                CheckDeliver.SetEnabled(false);
                Close.SetEnabled(false);
                Release.SetEnabled(false);
            }
            else if (compensationDocument.Current.Status == CompensationStatus.RecievedMaintInvoiceToCompany)
            {
                #region text fields enablement
                //print
                PXUIFieldAttribute.SetEnabled<CompensationDocument.insurranceCertificateRefNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerID>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.plateNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.startDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.endDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.paymentRef>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDescr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentNotifyDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.planDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverLicenceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received maint invoice
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceCenter>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.bankName>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkReceiveDate>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkAmt>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                //check delivered
                //add branch
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerDeliveryPlace>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDelivered>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDeliveredDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                #endregion

                Print.SetEnabled(false);
                Receive.SetEnabled(false);
                Send.SetEnabled(false);
                RecieveMaintInvoice.SetEnabled(false);
                RecieveMaintInvoiceToCompany.SetEnabled(false);
                CheckRecieve.SetEnabled(true);
                CheckDeliver.SetEnabled(false);
                Close.SetEnabled(false);
                Release.SetEnabled(false);
            }else if (compensationDocument.Current.Status == CompensationStatus.CheckRecieved)
            {
                #region text fields enablement
                //print
                PXUIFieldAttribute.SetEnabled<CompensationDocument.insurranceCertificateRefNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerID>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.plateNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.startDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.endDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.paymentRef>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDescr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentNotifyDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.planDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverLicenceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received maint invoice
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceCenter>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.bankName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkReceiveDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkAmt>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check delivered
                //add branch
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerDeliveryPlace>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDelivered>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDeliveredDate>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                #endregion

                Print.SetEnabled(false);
                Receive.SetEnabled(false);
                Send.SetEnabled(false);
                RecieveMaintInvoice.SetEnabled(false);
                RecieveMaintInvoiceToCompany.SetEnabled(false);
                CheckRecieve.SetEnabled(false);
                CheckDeliver.SetEnabled(true);
                Close.SetEnabled(false);
                Release.SetEnabled(false); 

            }else if (compensationDocument.Current.Status == CompensationStatus.CheckDelivered){

                #region text fields enablement
                //print
                PXUIFieldAttribute.SetEnabled<CompensationDocument.insurranceCertificateRefNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerID>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.plateNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.startDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.endDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.paymentRef>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDescr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentNotifyDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.planDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverLicenceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received maint invoice
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceCenter>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.bankName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkReceiveDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkAmt>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check delivered
                //add branch
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerDeliveryPlace>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDelivered>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDeliveredDate>(this.compensationDocument.Cache, this.compensationDocument.Current, true);
                #endregion

                Print.SetEnabled(false);
                Receive.SetEnabled(false);
                Send.SetEnabled(false);
                RecieveMaintInvoice.SetEnabled(false);
                RecieveMaintInvoiceToCompany.SetEnabled(false);
                CheckRecieve.SetEnabled(false);
                CheckDeliver.SetEnabled(false);
                Close.SetEnabled(true);
                Release.SetEnabled(false); 
            } 
            else if (compensationDocument.Current.Status == CompensationStatus.Closed)
            {
                #region text fields enablement
                //print
                PXUIFieldAttribute.SetEnabled<CompensationDocument.insurranceCertificateRefNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerID>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.plateNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.startDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.endDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.paymentRef>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDescr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentNotifyDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.planDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverLicenceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received maint invoice
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceCenter>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.bankName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkReceiveDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkAmt>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check delivered
                //add branch
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerDeliveryPlace>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDelivered>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDeliveredDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                #endregion

                Print.SetEnabled(false);
                Receive.SetEnabled(false);
                Send.SetEnabled(false);
                RecieveMaintInvoice.SetEnabled(false);
                RecieveMaintInvoiceToCompany.SetEnabled(false);
                CheckRecieve.SetEnabled(false);
                CheckDeliver.SetEnabled(false);
                Close.SetEnabled(false);
                Release.SetEnabled(true);
                PXUIFieldAttribute.SetEnabled(compensationDocument.Cache, compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled(compensationDocumentActionsTab.Cache, compensationDocumentActionsTab.Current, false);
                PXUIFieldAttribute.SetEnabled(finSettingsTab.Cache, finSettingsTab.Current, false);
            }
            else if (compensationDocument.Current.Status == CompensationStatus.Released)
            {

                #region text fields enablement
                //print
                PXUIFieldAttribute.SetEnabled<CompensationDocument.insurranceCertificateRefNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerID>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.plateNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.startDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.endDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.paymentRef>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDescr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.accidentNotifyDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.policeCaseDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.planDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.driverLicenceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //received maint invoice
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceCenter>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.maintenanceInvoiceDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check received
                PXUIFieldAttribute.SetEnabled<CompensationDocument.bankName>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkReceiveDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkNbr>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.checkAmt>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                //check delivered
                //add branch
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerDeliveryPlace>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDelivered>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled<CompensationDocument.customerCheckDeliveredDate>(this.compensationDocument.Cache, this.compensationDocument.Current, false);
                #endregion

                Print.SetEnabled(false);
                Receive.SetEnabled(false);
                Send.SetEnabled(false);
                RecieveMaintInvoice.SetEnabled(false);
                RecieveMaintInvoiceToCompany.SetEnabled(false);
                CheckRecieve.SetEnabled(false);
                CheckDeliver.SetEnabled(false);
                Close.SetEnabled(false);
                Release.SetEnabled(false);
                PXUIFieldAttribute.SetEnabled(compensationDocument.Cache, compensationDocument.Current,false);
                PXUIFieldAttribute.SetEnabled(compensationDocumentActionsTab.Cache, compensationDocumentActionsTab.Current, false);
                PXUIFieldAttribute.SetEnabled(finSettingsTab.Cache, finSettingsTab.Current, false);
                
            }

      
        }
        #endregion

        #region Buttons
        public PXAction<CompensationDocument> ActionsMenu;
        [PXButton]
        [PXUIField(DisplayName = "Actions")]
        protected virtual void actionsMenu()
        {
        }

        public PXAction<CompensationDocument> Print;
        [PXUIField(DisplayName = "Print",
        MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton()]
        public IEnumerable print(PXAdapter adapter)
        {
            this.Actions.PressSave();

            PXLongOperation.StartOperation(this, delegate()
            {
            
            compensationDocument.Cache.SetValue<CompensationDocument.status>(compensationDocument.Current, CompensationStatus.Printed);
            compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.printUser>(compensationDocumentActionsTab.Current, this.Accessinfo.UserID);
            compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.printDate>(compensationDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
            compensationDocumentActionsTab.Current.PrintUser = this.Accessinfo.UserID;
            compensationDocumentActionsTab.Current.PrintDate = this.Accessinfo.BusinessDate;
            this.compensationDocumentActionsTab.Update(compensationDocumentActionsTab.Current);
            this.Persist();
            this.Actions.PressSave();
            Print.SetEnabled(false);
            Send.SetEnabled(true);

            });

            
            return adapter.Get();
        }


        public PXAction<CompensationDocument> Receive;
        [PXUIField(DisplayName = "Receive",MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton()]
        public IEnumerable receive(PXAdapter adapter)
        {
            this.Actions.PressSave();

            PXLongOperation.StartOperation(this, delegate()
            {
                compensationDocument.Cache.SetValue<CompensationDocument.status>(compensationDocument.Current, CompensationStatus.Received);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.receiveUser>(compensationDocumentActionsTab.Current, this.Accessinfo.UserID);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.receiveDate>(compensationDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
                compensationDocumentActionsTab.Current.ReceiveUser = this.Accessinfo.UserID;
                compensationDocumentActionsTab.Current.ReceiveDate = this.Accessinfo.BusinessDate;
                this.compensationDocumentActionsTab.Update(compensationDocumentActionsTab.Current);
                this.Persist();
                this.Actions.PressSave();


                Print.SetEnabled(false);
                Send.SetEnabled(true);
            });

            return adapter.Get();
        }


        public PXAction<CompensationDocument> Send;
        [PXUIField(DisplayName = "Send",MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton()]
        public IEnumerable send(PXAdapter adapter)
        {

            this.Actions.PressSave();

            PXLongOperation.StartOperation(this, delegate()
            {
                compensationDocument.Cache.SetValue<CompensationDocument.status>(compensationDocument.Current, CompensationStatus.Sended);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.sendUser>(compensationDocumentActionsTab.Current, this.Accessinfo.UserID);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.sendDate>(compensationDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
                compensationDocumentActionsTab.Current.SendUser = this.Accessinfo.UserID;
                compensationDocumentActionsTab.Current.SendDate = this.Accessinfo.BusinessDate;
                this.compensationDocumentActionsTab.Update(compensationDocumentActionsTab.Current);
                this.Persist();
                this.Actions.PressSave();


                Print.SetEnabled(false);
                Send.SetEnabled(true);
            });

            return adapter.Get();
        }


        public PXAction<CompensationDocument> RecieveMaintInvoice;
        [PXUIField(DisplayName = "Recieve Maint Invoice",MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton()]
        public IEnumerable recieveMaintInvoice(PXAdapter adapter)
        {
            this.Actions.PressSave();

            PXLongOperation.StartOperation(this, delegate()
            {

                compensationDocument.Cache.SetValue<CompensationDocument.status>(compensationDocument.Current, CompensationStatus.RecievedMaintInvoice);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.recievedMaintInvoiceUser>(compensationDocumentActionsTab.Current, this.Accessinfo.UserID);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.recievedMaintInvoiceDate>(compensationDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
                compensationDocumentActionsTab.Current.RecievedMaintInvoiceUser = this.Accessinfo.UserID;
                compensationDocumentActionsTab.Current.RecievedMaintInvoiceDate = this.Accessinfo.BusinessDate;
                this.compensationDocumentActionsTab.Update(compensationDocumentActionsTab.Current);
                this.Persist();
                this.Actions.PressSave();


                Print.SetEnabled(false);
                Send.SetEnabled(true);
            });
            return adapter.Get();
        }


        public PXAction<CompensationDocument> RecieveMaintInvoiceToCompany;
        [PXUIField(DisplayName = "Recieved Maint Invoice To Company", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton()]
        public IEnumerable recieveMaintInvoiceToCompany(PXAdapter adapter)
        {
            this.Actions.PressSave();

            PXLongOperation.StartOperation(this, delegate()
            {
                compensationDocument.Cache.SetValue<CompensationDocument.status>(compensationDocument.Current, CompensationStatus.RecievedMaintInvoiceToCompany);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.recievedMaintInvoiceToCompanyUser>(compensationDocumentActionsTab.Current, this.Accessinfo.UserID);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.recievedMaintInvoiceToCompanyDate>(compensationDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
                compensationDocumentActionsTab.Current.RecievedMaintInvoiceToCompanyUser = this.Accessinfo.UserID;
                compensationDocumentActionsTab.Current.RecievedMaintInvoiceToCompanyDate = this.Accessinfo.BusinessDate;
                this.compensationDocumentActionsTab.Update(compensationDocumentActionsTab.Current);
                this.Persist();
                this.Actions.PressSave();


                Print.SetEnabled(false);
                Send.SetEnabled(true);
            });
            return adapter.Get();
        }

        public PXAction<CompensationDocument> CheckRecieve;
        [PXUIField(DisplayName = "Check Recieve", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton()]
        public IEnumerable checkRecieve(PXAdapter adapter)
        {
            this.Actions.PressSave();

            PXLongOperation.StartOperation(this, delegate()
            {
                compensationDocument.Cache.SetValue<CompensationDocument.status>(compensationDocument.Current, CompensationStatus.CheckRecieved);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.checkRecievedUser>(compensationDocumentActionsTab.Current, this.Accessinfo.UserID);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.checkRecievedDate>(compensationDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
                compensationDocumentActionsTab.Current.CheckRecievedUser = this.Accessinfo.UserID;
                compensationDocumentActionsTab.Current.CheckRecievedDate = this.Accessinfo.BusinessDate;
                this.compensationDocumentActionsTab.Update(compensationDocumentActionsTab.Current);
                this.Persist();
                this.Actions.PressSave();


                Print.SetEnabled(false);
                Send.SetEnabled(true);
            });
            return adapter.Get();
        }


        public PXAction<CompensationDocument> CheckDeliver;
        [PXUIField(DisplayName = "Check Deliver", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton()]
        public IEnumerable checkDeliver(PXAdapter adapter)
        {
            this.Actions.PressSave();

            PXLongOperation.StartOperation(this, delegate()
            {
                compensationDocument.Cache.SetValue<CompensationDocument.status>(compensationDocument.Current, CompensationStatus.CheckDelivered);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.checkDeliveredUser>(compensationDocumentActionsTab.Current, this.Accessinfo.UserID);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.checkDeliveredDate>(compensationDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
                compensationDocumentActionsTab.Current.CheckDeliveredUser = this.Accessinfo.UserID;
                compensationDocumentActionsTab.Current.CheckDeliveredDate = this.Accessinfo.BusinessDate;
                this.compensationDocumentActionsTab.Update(compensationDocumentActionsTab.Current);
                this.Persist();
                this.Actions.PressSave();


                Print.SetEnabled(false);
                Send.SetEnabled(true);
            });
            return adapter.Get();
        }


        public PXAction<CompensationDocument> Close;
        [PXUIField(DisplayName = "Close", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton()]
        public IEnumerable close(PXAdapter adapter)
        {

            this.Actions.PressSave();

            PXLongOperation.StartOperation(this, delegate()
            {
                compensationDocument.Cache.SetValue<CompensationDocument.status>(compensationDocument.Current, CompensationStatus.Closed);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.closedUser>(compensationDocumentActionsTab.Current, this.Accessinfo.UserID);
                compensationDocumentActionsTab.Cache.SetValue<CompensationDocument.closedDate>(compensationDocumentActionsTab.Current, this.Accessinfo.BusinessDate);
                compensationDocumentActionsTab.Current.ClosedUser = this.Accessinfo.UserID;
                compensationDocumentActionsTab.Current.ClosedDate = this.Accessinfo.BusinessDate;
                this.compensationDocumentActionsTab.Update(compensationDocumentActionsTab.Current);
                this.Persist();
                this.Actions.PressSave();


                Print.SetEnabled(false);
                Send.SetEnabled(true);
            });
            return adapter.Get();
        }


        public PXAction<CompensationDocument> Release;
        [PXUIField(DisplayName = "Release",
        MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton()]
        public IEnumerable release(PXAdapter adapter)
        {
            this.Actions.PressSave();

            PXLongOperation.StartOperation(this, delegate()
            {
                CertificateDocument certificateDocument = PXSelect<CertificateDocument, Where<CertificateDocument.refNbr, Equal<Required<CertificateDocument.refNbr>>>>.Select(this, this.compensationDocument.Current.InsurranceCertificateRefNbr);
                ARInvoiceEntry arGrph = PXGraph.CreateInstance<ARInvoiceEntry>();
                ARInvoice doc = new ARInvoice();
                doc.DocType = ARDocType.CreditMemo;
                doc.RefNbr = "-548645354";
                doc.CustomerID = certificateDocument.CustomerID;
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
                tran.CuryUnitPrice = (decimal)this.compensationDocument.Current.CheckAmt;
                tran.AccountID = certificateDocument.AccountID;
                tran.SubID = cc.ARSubID;
                arGrph.Transactions.Insert(tran);
                arGrph.Persist(typeof(ARTran), PXDBOperation.Insert);
                arGrph.Actions.PressSave();

                compensationDocument.Cache.SetValue<CompensationDocument.closeBatchNbr>(this.compensationDocument.Current, arGrph.Document.Current.RefNbr);
                this.compensationDocument.Current.CloseBatchNbr = arGrph.Document.Current.RefNbr;
                this.compensationDocument.Update(this.compensationDocument.Current);
                this.Persist();


                compensationDocument.Cache.SetValue<CompensationDocument.released>(compensationDocument.Current, true);
                this.compensationDocument.Update(compensationDocument.Current);
                this.Actions.PressSave();

                Release.SetEnabled(false);
                PXUIFieldAttribute.SetEnabled(compensationDocument.Cache, compensationDocument.Current, false);
                PXUIFieldAttribute.SetEnabled(compensationDocumentActionsTab.Cache, compensationDocumentActionsTab.Current, false);
                PXUIFieldAttribute.SetEnabled(finSettingsTab.Cache, finSettingsTab.Current, false);
            });
            return adapter.Get();
        }
        #endregion

    }
}