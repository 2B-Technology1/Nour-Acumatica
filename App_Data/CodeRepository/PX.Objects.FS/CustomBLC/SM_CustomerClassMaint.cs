/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using System.Collections.Generic;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.FS
{
    public class SM_CustomerClassMaint : PXGraphExtension<CustomerClassMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region Selects

        public PXSelectJoin<FSCustomerClassBillingSetup,
               CrossJoin<FSSetup>,
               Where2<
                    Where<
                        FSSetup.customerMultipleBillingOptions, Equal<True>,
                        And<FSCustomerClassBillingSetup.customerClassID, Equal<Current<CustomerClass.customerClassID>>,
                        And<FSCustomerClassBillingSetup.srvOrdType, IsNotNull>>>,
                    Or<
                        Where<
                            FSSetup.customerMultipleBillingOptions, Equal<False>,
                            And<FSCustomerClassBillingSetup.customerClassID, Equal<Current<CustomerClass.customerClassID>>,
                            And<FSCustomerClassBillingSetup.srvOrdType, IsNull>>>>>>
                BillingCycles;

        #endregion

        #region Virtual Methods

        public virtual bool IsThisLineValid(FSCustomerClassBillingSetup fsCustomerClassBillingSetupRow_Current)
        {
            int count = 0;

            foreach (FSCustomerClassBillingSetup fsCustomerClassBillingRow in BillingCycles.Select())
            {
                if (fsCustomerClassBillingSetupRow_Current.SrvOrdType != null
                        && fsCustomerClassBillingSetupRow_Current.SrvOrdType.Equals(fsCustomerClassBillingRow.SrvOrdType))
                {
                    count++;
                }
            }

            return count <= 1;
        }

        public virtual void DisplayBillingOptions(PXCache cache, CustomerClass customerClassRow, FSxCustomerClass fsxCustomerClassRow)
        {
            FSSetup fsSetupRow = PXSelect<FSSetup>.Select(Base);

            bool enableMultipleServicesBilling = fsSetupRow != null ? fsSetupRow.CustomerMultipleBillingOptions == true : false;

            PXUIFieldAttribute.SetVisible<FSxCustomerClass.defaultBillingCycleID>(cache, customerClassRow, !enableMultipleServicesBilling);
            PXUIFieldAttribute.SetVisible<FSxCustomerClass.sendInvoicesTo>(cache, customerClassRow, !enableMultipleServicesBilling);
            PXUIFieldAttribute.SetVisible<FSxCustomerClass.billShipmentSource>(cache, customerClassRow, !enableMultipleServicesBilling);

            BillingCycles.AllowSelect = enableMultipleServicesBilling;

            if (fsxCustomerClassRow != null)
            {
                FSBillingCycle fsBillingCycleRow = FSBillingCycle.PK.Find(Base, fsxCustomerClassRow.DefaultBillingCycleID);

                bool forbidUpdateBillingOptions = SharedFunctions.IsNotAllowedBillingOptionsModification(fsBillingCycleRow);

                PXUIFieldAttribute.SetEnabled<FSxCustomerClass.sendInvoicesTo>(cache,
                                                                               customerClassRow,
                                                                               forbidUpdateBillingOptions == false);

                PXUIFieldAttribute.SetEnabled<FSxCustomerClass.billShipmentSource>(cache,
                                                                                   customerClassRow,
                                                                                   forbidUpdateBillingOptions == false);

                PXUIFieldAttribute.SetEnabled<FSxCustomerClass.defaultBillingCycleID>(cache, customerClassRow);

                if (fsxCustomerClassRow.DefaultBillingCycleID != null
                        && forbidUpdateBillingOptions == false)
                {
                    PXDefaultAttribute.SetPersistingCheck<FSxCustomerClass.sendInvoicesTo>(cache, customerClassRow, PXPersistingCheck.NullOrBlank);
                    PXDefaultAttribute.SetPersistingCheck<FSxCustomerClass.billShipmentSource>(cache, customerClassRow, PXPersistingCheck.NullOrBlank);
                }
                else
                {
                    PXDefaultAttribute.SetPersistingCheck<FSxCustomerClass.sendInvoicesTo>(cache, customerClassRow, PXPersistingCheck.Nothing);
                    PXDefaultAttribute.SetPersistingCheck<FSxCustomerClass.billShipmentSource>(cache, customerClassRow, PXPersistingCheck.Nothing);
                }
            }
        }

        public virtual void ResetSendInvoicesToFromBillingCycle(CustomerClass customerClassRow, FSCustomerClassBillingSetup fsCustomerClassBillingSetupRow)
        {
            List<object> args = new List<object>();
            FSBillingCycle fsBillingCycleRow = null;

            BqlCommand billingCycleCommand = 
                new Select<FSBillingCycle,
                    Where<
                        FSBillingCycle.billingCycleID, Equal<Required<FSBillingCycle.billingCycleID>>>>();

            PXView billingCycleView = new PXView(Base, true, billingCycleCommand);

            if (customerClassRow != null)
            {
                FSxCustomerClass fsxCustomerClassRow = PXCache<CustomerClass>.GetExtension<FSxCustomerClass>(customerClassRow);
                args.Add(fsxCustomerClassRow.DefaultBillingCycleID);

                fsBillingCycleRow = (FSBillingCycle)billingCycleView.SelectSingle(args.ToArray());

                if (fsBillingCycleRow != null)
                {
                    if (SharedFunctions.IsNotAllowedBillingOptionsModification(fsBillingCycleRow))
                    {
                        fsxCustomerClassRow.SendInvoicesTo = ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO;
                        fsxCustomerClassRow.BillShipmentSource = ID.Ship_To.SERVICE_ORDER_ADDRESS;
                    }
                }
            }
            else if (fsCustomerClassBillingSetupRow != null)
            {
                args.Add(fsCustomerClassBillingSetupRow.BillingCycleID);
                fsBillingCycleRow = (FSBillingCycle)billingCycleView.SelectSingle(args.ToArray());

                if (fsBillingCycleRow != null)
                {
                    if (SharedFunctions.IsNotAllowedBillingOptionsModification(fsBillingCycleRow))
                    {
                        fsCustomerClassBillingSetupRow.SendInvoicesTo = ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO;
                        fsCustomerClassBillingSetupRow.BillShipmentSource = ID.Ship_To.SERVICE_ORDER_ADDRESS;
                    }
                }
            }
        }

        public virtual void EnableDisableCustomerBilling(PXCache cache, CustomerClass customerClassRow, FSxCustomerClass fsxCustomerClassRow)
        {
            bool isSpecificCustomer = fsxCustomerClassRow.DefaultBillingCustomerSource == ID.Default_Billing_Customer_Source.SPECIFIC_CUSTOMER;

            PXUIFieldAttribute.SetVisible<FSxCustomerClass.billCustomerID>(cache, customerClassRow, isSpecificCustomer);
            PXUIFieldAttribute.SetVisible<FSxCustomerClass.billLocationID>(cache, customerClassRow, isSpecificCustomer);
            PXDefaultAttribute.SetPersistingCheck<FSxCustomerClass.billCustomerID>(cache, customerClassRow, isSpecificCustomer == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<FSxCustomerClass.billLocationID>(cache, customerClassRow, isSpecificCustomer == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            if (isSpecificCustomer == false)
            {
                fsxCustomerClassRow.BillCustomerID = null;
                fsxCustomerClassRow.BillLocationID = null;
            }
        }
        #endregion

        #region Event Handlers

        #region CustomerClass

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        protected virtual void _(Events.FieldDefaulting<CustomerClass, FSxCustomerClass.billLocationID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSxCustomerClass fsxCustomerClassRow = e.Cache.GetExtension<FSxCustomerClass>(e.Row);

            BAccountR BARow = PXSelectJoin<BAccountR,
                                InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>,
                                          And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
                                Where<BAccountR.bAccountID, Equal<Required<BAccountR.bAccountID>>,
                                      And<CRLocation.isActive, Equal<True>,
                                      And<MatchWithBranch<CRLocation.cBranchID>>>>>
                                .Select(Base, fsxCustomerClassRow.BillCustomerID);

            if (BARow != null && BARow.DefLocationID != null)
            {
                e.NewValue = BARow.DefLocationID;
            }
            else
            {
                CRLocation CRLRow = PXSelect<CRLocation,
                                    Where<CRLocation.bAccountID, Equal<Required<CRLocation.bAccountID>>,
                                    And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.cBranchID>>>>>
                                    .Select(Base, fsxCustomerClassRow.BillCustomerID);

                if (CRLRow != null && CRLRow.LocationID != null)
                {
                    e.NewValue = CRLRow.LocationID;
                }
            }
        }
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        protected virtual void _(Events.FieldUpdated<CustomerClass, FSxCustomerClass.billCustomerID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSxCustomerClass fsxCustomerClassRow = e.Cache.GetExtension<FSxCustomerClass>(e.Row);

            if ((int?)e.OldValue != fsxCustomerClassRow.BillCustomerID)
            {
                e.Cache.SetDefaultExt<FSxCustomerClass.billLocationID>(e.Row);
            }
        }
        #endregion

        protected virtual void _(Events.RowSelecting<CustomerClass> e)
        {
        }

        protected virtual void _(Events.RowSelected<CustomerClass> e)
        {
            if (e.Row == null)
            {
                return;
            }

            CustomerClass customerClassRow = (CustomerClass)e.Row;
            PXCache cache = e.Cache;

            FSxCustomerClass fsxCustomerClassRow = cache.GetExtension<FSxCustomerClass>(customerClassRow);
            this.DisplayBillingOptions(cache, customerClassRow, fsxCustomerClassRow);
            EnableDisableCustomerBilling(cache, customerClassRow, fsxCustomerClassRow);
        }

        protected virtual void _(Events.RowInserting<CustomerClass> e)
        {
        }

        protected virtual void _(Events.RowInserted<CustomerClass> e)
        {
        }

        protected virtual void _(Events.RowUpdating<CustomerClass> e)
        {
        }

        protected virtual void _(Events.RowUpdated<CustomerClass> e)
        {
        }

        protected virtual void _(Events.RowDeleting<CustomerClass> e)
        {
        }

        protected virtual void _(Events.RowDeleted<CustomerClass> e)
        {
        }

        protected virtual void _(Events.RowPersisting<CustomerClass> e)
        {
        }

        protected virtual void _(Events.RowPersisted<CustomerClass> e)
        {
        }

        #endregion

        #region FSCustomerClassBillingSetup

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSCustomerClassBillingSetup, FSCustomerClassBillingSetup.billingCycleID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSCustomerClassBillingSetup fsCustomerClassBillingSetupRow = (FSCustomerClassBillingSetup)e.Row;
            this.ResetSendInvoicesToFromBillingCycle(null, fsCustomerClassBillingSetupRow);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSCustomerClassBillingSetup> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSCustomerClassBillingSetup> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSCustomerClassBillingSetup fsCustomerClassBillingSetupRow = (FSCustomerClassBillingSetup)e.Row;
            PXCache cache = e.Cache;

            if (!IsThisLineValid(fsCustomerClassBillingSetupRow))
            {
                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.billingCycleID>(cache, fsCustomerClassBillingSetupRow, false);
                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.sendInvoicesTo>(cache, fsCustomerClassBillingSetupRow, false);
                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.billShipmentSource>(cache, fsCustomerClassBillingSetupRow, false);
                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.frequencyType>(cache, fsCustomerClassBillingSetupRow, false);
                return;
            }

            bool enableBillingCycleIDField = fsCustomerClassBillingSetupRow.BillingCycleID == null && string.IsNullOrEmpty(fsCustomerClassBillingSetupRow.SrvOrdType) == false;

            PXUIFieldAttribute.SetEnabled<FSCustomerBillingSetup.billingCycleID>(cache, fsCustomerClassBillingSetupRow, enableBillingCycleIDField);

            //Disables the TimeCycleType field if the type of the BillingCycleID selected is Time Cycle.
            if (fsCustomerClassBillingSetupRow.BillingCycleID != null)
            {
                FSBillingCycle fsBillingCycleRow = FSBillingCycle.PK.Find(Base, fsCustomerClassBillingSetupRow.BillingCycleID);

                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.frequencyType>(cache, fsCustomerClassBillingSetupRow, fsBillingCycleRow.BillingCycleType != ID.Billing_Cycle_Type.TIME_FRAME);

                bool forbidUpdateBillingOptions = SharedFunctions.IsNotAllowedBillingOptionsModification(fsBillingCycleRow);

                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.sendInvoicesTo>(cache, fsCustomerClassBillingSetupRow, forbidUpdateBillingOptions == false);
                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.billShipmentSource>(cache, fsCustomerClassBillingSetupRow, forbidUpdateBillingOptions == false);
            }
            else
            {
                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.frequencyType>(cache, fsCustomerClassBillingSetupRow, false);
            }
        }

        protected virtual void _(Events.RowInserting<FSCustomerClassBillingSetup> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSCustomerClassBillingSetup> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSCustomerClassBillingSetup> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSCustomerClassBillingSetup> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSCustomerClassBillingSetup> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSCustomerClassBillingSetup> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSCustomerClassBillingSetup> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSCustomerClassBillingSetup> e)
        {
        }

        #endregion

        #endregion
    }
}
