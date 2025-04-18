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
using PX.Data.WorkflowAPI;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.PM;
using System.Web.Caching;

namespace PX.Objects.FS
{
    using static PX.Data.WorkflowAPI.BoundedTo<CustomerMaint, Customer>;
	using static PX.SM.EMailAccount;

    public class SM_CustomerMaint : PXGraphExtension<CustomerMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        private bool doCopyBillingSettings;
        public Customer PersistedCustomerWithBillingOptionsChanged { get; set; }

        #region Selects

        [PXHidden]
        public PXSelect<FSSetup> Setup;

		public FSSetup GetFSSetup()
		{
			if (Setup.Current == null)
			{
				return Setup.Select();
			}
			else
			{
				return Setup.Current;
			}
		}

        public PXSelectJoin<FSCustomerBillingSetup,
               CrossJoin<FSSetup>,
               Where<FSCustomerBillingSetup.customerID, Equal<Current<Customer.bAccountID>>,
                    And<Where2<
                        Where<FSSetup.customerMultipleBillingOptions, Equal<True>,
                            And<FSCustomerBillingSetup.srvOrdType, IsNotNull>>,
                        Or<Where<FSSetup.customerMultipleBillingOptions, Equal<False>,
                            And<FSCustomerBillingSetup.srvOrdType, IsNull>>>>>>> CustomerBillingCycles;
        #endregion

        #region WorkflowChanges
        public class WorkflowChanges : PXGraphExtension<CustomerMaint_Workflow, CustomerMaint>
        {
            public static bool IsActive() => SM_CustomerMaint.IsActive();

            public sealed override void Configure(PXScreenConfiguration config) =>
                Configure(config.GetScreenConfigurationContext<CustomerMaint, Customer>());

            protected static void Configure(WorkflowContext<CustomerMaint, Customer> context)
            {
                var servicesCategory = context.Categories.Get(CustomerMaint_Workflow.CategoryID.Services);

                #region Conditions
                Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
                var conditions = new
                {
                    IsOpenMultipleStaffMemberBoardDisabled
                        = Bql<Customer.status.IsNotIn<CustomerStatus.active, CustomerStatus.oneTime>>(),
                    IsOpenSingleStaffMemberBoardDisabled
                        = Bql<Customer.status.IsNotIn<CustomerStatus.active, CustomerStatus.oneTime>>(),
                }.AutoNameConditions();
                #endregion

                context.UpdateScreenConfigurationFor(config =>
                {
                    return config
                        .WithActions(actions =>
                        {
                            actions.Add<SM_CustomerMaint>(g =>
                                            g.openMultipleStaffMemberBoard, a => a
                                                .WithCategory(servicesCategory)
                                                .IsDisabledWhen(conditions.IsOpenMultipleStaffMemberBoardDisabled));
                            actions.Add<SM_CustomerMaint>(g =>
                                            g.openSingleStaffMemberBoard, a => a
                                                .WithCategory(servicesCategory, nameof(SM_CustomerMaint.openMultipleStaffMemberBoard))
                                                .IsDisabledWhen(conditions.IsOpenSingleStaffMemberBoardDisabled));

                            // "Inquiries" folder
                            actions.Add<SM_CustomerMaint>(g => g.viewServiceOrderHistory, a => a.WithCategory(PredefinedCategory.Inquiries));
                            actions.Add<SM_CustomerMaint>(g => g.viewAppointmentHistory, a => a.WithCategory(PredefinedCategory.Inquiries));
                            actions.Add<SM_CustomerMaint>(g => g.viewEquipmentSummary, a => a.WithCategory(PredefinedCategory.Inquiries));
                            actions.Add<SM_CustomerMaint>(g => g.viewContractScheduleSummary, a => a.WithCategory(PredefinedCategory.Inquiries));
                        });
                });
            }
        }
        #endregion

        #region Actions

        #region ViewServiceOrderHistory
        public PXAction<Customer> viewServiceOrderHistory;
        [PXUIField(DisplayName = "Service Order History", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable ViewServiceOrderHistory(PXAdapter adapter)
        {
            Customer customerRow = Base.CurrentCustomer.Current;

            if (customerRow != null && customerRow.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                Branch branchRow = PXSelect<Branch,
                                   Where<
                                       Branch.branchID, Equal<Required<Branch.branchID>>>>
                                   .Select(Base, Base.Accessinfo.BranchID);

                parameters["BranchID"] = branchRow.BranchCD;
                parameters["CustomerID"] = customerRow.AcctCD;
                throw new PXRedirectToGIWithParametersRequiredException(new Guid(TX.GenericInquiries_GUID.SERVICE_ORDER_HISTORY), parameters);
            }

            return adapter.Get();
        }
        #endregion
        #region ViewAppointmentHistory
        public PXAction<Customer> viewAppointmentHistory;
        [PXUIField(DisplayName = "Appointment History", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable ViewAppointmentHistory(PXAdapter adapter)
        {
            Customer customerRow = Base.CurrentCustomer.Current;

            if (customerRow != null && customerRow.BAccountID > 0L)
            {
                AppointmentInq graph = PXGraph.CreateInstance<AppointmentInq>();

                graph.Filter.Current.BranchID = Base.Accessinfo.BranchID;
                graph.Filter.Current.CustomerID = customerRow.BAccountID;

                throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.Same };
            }

            return adapter.Get();
        }
        #endregion
        #region ViewEquipmentSummary
        public PXAction<Customer> viewEquipmentSummary;
        [PXUIField(DisplayName = "Equipment Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable ViewEquipmentSummary(PXAdapter adapter)
        {
            Customer customerRow = Base.CurrentCustomer.Current;

            if (customerRow != null && customerRow.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                parameters["CustomerID"] = customerRow.AcctCD;
                throw new PXRedirectToGIWithParametersRequiredException(new Guid(TX.GenericInquiries_GUID.EQUIPMENT_SUMMARY), parameters);
            }

            return adapter.Get();
        }
        #endregion
        #region ViewContractScheduleSummary
        public PXAction<Customer> viewContractScheduleSummary;
        [PXUIField(DisplayName = "Contract Schedule Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable ViewContractScheduleSummary(PXAdapter adapter)
        {
            Customer customerRow = Base.CurrentCustomer.Current;

            if (customerRow != null && customerRow.BAccountID > 0L)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                parameters["CustomerID"] = customerRow.AcctCD;
                throw new PXRedirectToGIWithParametersRequiredException(new Guid(TX.GenericInquiries_GUID.CONTRACT_SCHEDULE_SUMMARY), parameters);
            }

            return adapter.Get();
        }
        #endregion

        #region OpenMultipleStaffMemberBoard
        public PXAction<Customer> openMultipleStaffMemberBoard;
        [PXUIField(DisplayName = TX.ActionCalendarBoardAccess.MULTI_EMP_CALENDAR, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable OpenMultipleStaffMemberBoard(PXAdapter adapter)
        {
            Customer customerRow = Base.CurrentCustomer.Current;

            if (customerRow != null && customerRow.BAccountID > 0L)
            {
                KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(typeof(FSServiceOrder.customerID).Name, customerRow.BAccountID.Value.ToString())
                };

                throw new PXRedirectToBoardRequiredException(Paths.ScreenPaths.MULTI_EMPLOYEE_DISPATCH, parameters);
            }

            return adapter.Get();
        }
        #endregion
        #region OpenSingleStaffMemberBoard
        public PXAction<Customer> openSingleStaffMemberBoard;
        [PXUIField(DisplayName = TX.ActionCalendarBoardAccess.SINGLE_EMP_CALENDAR, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable OpenSingleStaffMemberBoard(PXAdapter adapter)
        {
            Customer customerRow = Base.CurrentCustomer.Current;

            if (customerRow != null && customerRow.BAccountID > 0L)
            {
                KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(typeof(FSServiceOrder.customerID).Name, customerRow.BAccountID.Value.ToString())
                };

                throw new PXRedirectToBoardRequiredException(Paths.ScreenPaths.SINGLE_EMPLOYEE_DISPATCH, parameters);
            }

            return adapter.Get();
        }
        #endregion

        #endregion

        #region Overrides
        [PXOverride]
        public virtual void Persist(Action baseMethod)
        {
            if (baseMethod == null)
            {
                return;
            }

            Customer customer = Base.BAccount.Current;

            if (customer == null)
            {
                baseMethod();
                return;
            }

            PXEntryStatus entryStatus = Base.BAccount.Cache.GetStatus(customer);

            if (entryStatus != PXEntryStatus.Notchanged && entryStatus != PXEntryStatus.Updated)
            {
                baseMethod();
                return;
            }

            PersistedCustomerWithBillingOptionsChanged = null;

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                try
                {
                    Base.Persist(typeof(FSCustomerBillingSetup), PXDBOperation.Update);
                    Base.Persist(typeof(FSCustomerBillingSetup), PXDBOperation.Insert);
                }
                catch
                {
                    Base.Caches[typeof(FSCustomerBillingSetup)].Persisted(true);
                    throw;
                }

                try
                {
                    baseMethod();
                }
                catch
                {
                    Base.Caches[typeof(FSCustomerBillingSetup)].Persisted(true);
                    throw;
                }

                ts.Complete();
            }
        }
        #endregion

        #region Virtual Methods

        /// <summary>
        /// Sets the Customer Billing Cycle from its Customer Class.
        /// </summary>
        public virtual void SetBillingCycleFromCustomerClass(PXCache cache, Customer customerRow)
        {
            if (customerRow.CustomerClassID == null)
            {
                return;
            }


			FSSetup fsSetupRow = GetFSSetup();
			if (fsSetupRow?.CustomerMultipleBillingOptions == true && Base.BAccount?.Current?.BAccountID != null)
			{
				bool serviceOrderExist = PXSelectJoin<FSCustomerBillingSetup,
					InnerJoin<FSServiceOrder, On<FSServiceOrder.srvOrdType, Equal<FSCustomerBillingSetup.srvOrdType>>,
					CrossJoin<FSSetup>>,
					Where<
						FSCustomerBillingSetup.customerID, Equal<Current<Customer.bAccountID>>,
						And<FSCustomerBillingSetup.active, Equal<True>,
						And<FSServiceOrder.customerID, Equal<Required<Customer.bAccountID>>,
							And<Where<Where2<
							Where<
								FSSetup.customerMultipleBillingOptions, Equal<True>,
								And<FSCustomerBillingSetup.srvOrdType, IsNotNull>>,
							Or<Where<
								FSSetup.customerMultipleBillingOptions, Equal<False>,
								And<FSCustomerBillingSetup.srvOrdType, IsNull>>>>>>>>>>.Select(Base, Base.BAccount.Current.BAccountID).Any();
				if (serviceOrderExist)
			{
				Base.BAccount.Cache.RaiseExceptionHandling<Customer.customerClassID>(customerRow, customerRow.CustomerClassID,
					new PXSetPropertyException(TX.Warning.BILLING_SETTINGS_NOT_MODIFIED_SRV_ORDERS_AVAIL, PXErrorLevel.Warning));
					return;
			}
			}

            if (fsSetupRow != null
                    && fsSetupRow.CustomerMultipleBillingOptions == true)
            {
                foreach (FSCustomerBillingSetup fsCustomerBillingSetupRow in this.CustomerBillingCycles.Select())
                {
                    this.CustomerBillingCycles.Delete(fsCustomerBillingSetupRow);
                }

                var customerClsBillingSetupRows = PXSelect<FSCustomerClassBillingSetup,
                                                  Where<
                                                      FSCustomerClassBillingSetup.customerClassID, Equal<Required<FSCustomerClassBillingSetup.customerClassID>>>>
                                                  .Select(Base, customerRow.CustomerClassID);

                foreach (FSCustomerClassBillingSetup fsCustomerClassBillingSetupRow in customerClsBillingSetupRows)
                {
                    FSCustomerBillingSetup fsCustomerBillingSetupRow = new FSCustomerBillingSetup();
                    fsCustomerBillingSetupRow.SrvOrdType = fsCustomerClassBillingSetupRow.SrvOrdType;
                    fsCustomerBillingSetupRow.BillingCycleID = fsCustomerClassBillingSetupRow.BillingCycleID;
                    fsCustomerBillingSetupRow.SendInvoicesTo = fsCustomerClassBillingSetupRow.SendInvoicesTo;
                    fsCustomerBillingSetupRow.BillShipmentSource = fsCustomerClassBillingSetupRow.BillShipmentSource;
                    fsCustomerBillingSetupRow.FrequencyType = fsCustomerClassBillingSetupRow.FrequencyType;

                    using (var r = new ReadOnlyScope(this.CustomerBillingCycles.Cache))
                    {
                        this.CustomerBillingCycles.Insert(fsCustomerBillingSetupRow);
                    }
                }

                return;  
            }

            SetSingleBillingSettings(cache, customerRow);
        }

        public virtual void SetSingleBillingSettings(PXCache cache, Customer customerRow)
        {
            if (customerRow.CustomerClassID == null)
            {
                return;
            }

            FSxCustomer fsxCustomerRow = cache.GetExtension<FSxCustomer>(customerRow);
            FSxCustomerClass fsxCustomerClassRow = Base.CustomerClass.Cache.GetExtension<FSxCustomerClass>(Base.CustomerClass.Current);

            if (fsxCustomerClassRow == null)
            {
                return;
            }

            if (fsxCustomerClassRow.DefaultBillingCycleID != null)
            {
				cache.SetValueExt<FSxCustomer.billingCycleID>(customerRow, fsxCustomerClassRow.DefaultBillingCycleID);
            }

            if (fsxCustomerClassRow.SendInvoicesTo != null)
            {
				cache.SetValueExt<FSxCustomer.sendInvoicesTo>(customerRow, fsxCustomerClassRow.SendInvoicesTo);
            }

            if (fsxCustomerClassRow.BillShipmentSource != null)
            {
				cache.SetValueExt<FSxCustomer.billShipmentSource>(customerRow, fsxCustomerClassRow.BillShipmentSource);
            }
        }

        /// <summary>
        /// Resets the values of the Frequency Fields depending on the Frequency Type value.
        /// </summary>
        /// <param name="fsCustomerBillingSetupRow"><c>fsCustomerBillingRow</c> row.</param>
        public virtual void ResetTimeCycleOptions(FSCustomerBillingSetup fsCustomerBillingSetupRow)
        {
            switch (fsCustomerBillingSetupRow.FrequencyType)
            {
                case ID.Time_Cycle_Type.DAY_OF_MONTH:
                    fsCustomerBillingSetupRow.MonthlyFrequency = 31;
                    break;
                case ID.Time_Cycle_Type.WEEKDAY:
                    fsCustomerBillingSetupRow.WeeklyFrequency = 5;
                    break;
                default:
                    fsCustomerBillingSetupRow.WeeklyFrequency  = null;
                    fsCustomerBillingSetupRow.MonthlyFrequency = null;
                    break;
            }
        }

        /// <summary>
        /// Configures the Multiple Services Billing options for the given Customer.
        /// </summary>
        /// <param name="cache">Cache of the view.</param>
        /// <param name="customerRow">Customer row.</param>
        public virtual void DisplayCustomerBillingOptions(PXCache cache, Customer customerRow, FSxCustomer fsxCustomerRow)
        {
            FSSetup fsSetupRow = PXSelect<FSSetup>.Select(Base);
            
            bool enableMultipleServicesBilling = fsSetupRow != null ? fsSetupRow.CustomerMultipleBillingOptions == true : false;

            PXUIFieldAttribute.SetVisible<FSxCustomer.billingCycleID>(cache, customerRow, !enableMultipleServicesBilling);
            PXUIFieldAttribute.SetVisible<FSxCustomer.sendInvoicesTo>(cache, customerRow, !enableMultipleServicesBilling);
            PXUIFieldAttribute.SetVisible<FSxCustomer.billShipmentSource>(cache, customerRow, !enableMultipleServicesBilling);

            CustomerBillingCycles.AllowSelect = enableMultipleServicesBilling;

            if (fsxCustomerRow != null)
            {
                FSBillingCycle fsBillingCycleRow = FSBillingCycle.PK.Find(Base, fsxCustomerRow.BillingCycleID);

                bool forbidUpdateBillingOptions = SharedFunctions.IsNotAllowedBillingOptionsModification(fsBillingCycleRow);

                PXUIFieldAttribute.SetEnabled<FSxCustomer.sendInvoicesTo>(cache,
                                                                          customerRow,
                                                                          forbidUpdateBillingOptions == false);

                PXUIFieldAttribute.SetEnabled<FSxCustomer.billShipmentSource>(cache,
                                                                              customerRow,
                                                                              forbidUpdateBillingOptions == false);

                PXUIFieldAttribute.SetEnabled<FSxCustomer.billingCycleID>(cache, customerRow);
            }
        }

        /// <summary>
        /// Resets the value from Send to Invoices dropdown if the billing cycle can not be sent to specific locations.
        /// </summary>
        public virtual void ResetSendInvoicesToFromBillingCycle(Customer customerRow, FSCustomerBillingSetup fsCustomerBillingSetupRow)
        {
            List<object> args = new List<object>();
            FSBillingCycle fsBillingCycleRow = null;
            BqlCommand billingCycleCommand = new Select<FSBillingCycle,
                                                 Where<
                                                     FSBillingCycle.billingCycleID, Equal<Required<FSBillingCycle.billingCycleID>>>>();

            PXView billingCycleView = new PXView(Base, true, billingCycleCommand);

            if (customerRow != null)
            {
                FSxCustomer fsxCustomerRow = PXCache<Customer>.GetExtension<FSxCustomer>(customerRow);
                args.Add(fsxCustomerRow.BillingCycleID);

                fsBillingCycleRow = (FSBillingCycle)billingCycleView.SelectSingle(args.ToArray());

                if (fsBillingCycleRow != null)
                {
                    if (SharedFunctions.IsNotAllowedBillingOptionsModification(fsBillingCycleRow))
                    {
                        fsxCustomerRow.SendInvoicesTo = ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO;
                        fsxCustomerRow.BillShipmentSource = ID.Ship_To.SERVICE_ORDER_ADDRESS;
                    }
                    if(fsxCustomerRow.SendInvoicesTo == null)
                        Base.CurrentCustomer.Cache.SetDefaultExt<FSxCustomer.sendInvoicesTo>(customerRow);
                    if(fsxCustomerRow.BillShipmentSource == null)
                        Base.CurrentCustomer.Cache.SetDefaultExt<FSxCustomer.billShipmentSource>(customerRow);
                    if(fsxCustomerRow.DefaultBillingCustomerSource == null)
                        Base.CurrentCustomer.Cache.SetDefaultExt<FSxCustomer.defaultBillingCustomerSource>(customerRow);
                }
            }
            else if (fsCustomerBillingSetupRow != null)
            {
                args.Add(fsCustomerBillingSetupRow.BillingCycleID);
                fsBillingCycleRow = (FSBillingCycle)billingCycleView.SelectSingle(args.ToArray());

                if (fsBillingCycleRow != null)
                {
                    if (SharedFunctions.IsNotAllowedBillingOptionsModification(fsBillingCycleRow))
                    {
                        fsCustomerBillingSetupRow.SendInvoicesTo = ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO;
                        fsCustomerBillingSetupRow.BillShipmentSource = ID.Ship_To.SERVICE_ORDER_ADDRESS;
                    }
                }
            }
        }

        public virtual void InsertUpdateCustomerBillingSetup(PXCache cache, Customer customerRow, FSxCustomer fsxCustomerRow)
        {
            FSSetup fsSetupRow = PXSelect<FSSetup>.Select(Base);
            if (fsSetupRow != null && fsSetupRow.CustomerMultipleBillingOptions == false)
            {
                FSCustomerBillingSetup fsCustomerBillingSetupRow = CustomerBillingCycles.Select();

                if (fsxCustomerRow.BillingCycleID == null)
                {
                    CustomerBillingCycles.Delete(fsCustomerBillingSetupRow);
                    return;
                }

                if (fsCustomerBillingSetupRow == null)
                {
					fsCustomerBillingSetupRow = new FSCustomerBillingSetup
					{
						CustomerID = customerRow.BAccountID
					};
					fsCustomerBillingSetupRow = CustomerBillingCycles.Insert(fsCustomerBillingSetupRow);
                }

                fsCustomerBillingSetupRow.BillingCycleID = fsxCustomerRow.BillingCycleID;
                fsCustomerBillingSetupRow.SendInvoicesTo = fsxCustomerRow.SendInvoicesTo;
                fsCustomerBillingSetupRow.BillShipmentSource = fsxCustomerRow.BillShipmentSource;
                fsCustomerBillingSetupRow.FrequencyType = ID.Frequency_Type.NONE;

                CustomerBillingCycles.Update(fsCustomerBillingSetupRow);
            }
        }

        public virtual void SetBillingCustomerSetting(PXCache cache, Customer customerRow)
        {
            FSxCustomer fsxCustomerRow = cache.GetExtension<FSxCustomer>(customerRow);
            FSxCustomerClass fsxCustomerClassRow = Base.CustomerClass.Cache.GetExtension<FSxCustomerClass>(Base.CustomerClass.Current);

            fsxCustomerRow.DefaultBillingCustomerSource = fsxCustomerClassRow.DefaultBillingCustomerSource;
            fsxCustomerRow.BillCustomerID = fsxCustomerClassRow.BillCustomerID;
            fsxCustomerRow.BillLocationID = fsxCustomerClassRow.BillLocationID;
        }

        public virtual void EnableDisableCustomerBilling(PXCache cache, Customer customerRow, FSxCustomer fsxCustomerRow)
        {
            bool isSpecificCustomer = fsxCustomerRow.DefaultBillingCustomerSource == ID.Default_Billing_Customer_Source.SPECIFIC_CUSTOMER;

            PXUIFieldAttribute.SetVisible<FSxCustomer.billCustomerID>(cache, customerRow, isSpecificCustomer);
            PXUIFieldAttribute.SetVisible<FSxCustomer.billLocationID>(cache, customerRow, isSpecificCustomer);
            PXDefaultAttribute.SetPersistingCheck<FSxCustomer.billCustomerID>(cache, customerRow, isSpecificCustomer == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<FSxCustomer.billLocationID>(cache, customerRow, isSpecificCustomer == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            if (isSpecificCustomer == false)
            {
                fsxCustomerRow.BillCustomerID = null;
                fsxCustomerRow.BillLocationID = null;
            }
        }

        public virtual void VerifyPrepaidContractRelated(PXCache cache, FSCustomerBillingSetup fsCustomerBillingSetupRow)
        {
            int? billingCycleIDOldValue = (int?)cache.GetValueOriginal<FSCustomerBillingSetup.billingCycleID>(fsCustomerBillingSetupRow);

            if (billingCycleIDOldValue == null || fsCustomerBillingSetupRow.BillingCycleID == billingCycleIDOldValue)
            {
                return;
            }

            FSBillingCycle newbillingCycleRow = PXSelect<FSBillingCycle,
                                                Where<
                                                    FSBillingCycle.billingCycleID, Equal<Required<FSBillingCycle.billingCycleID>>>>
                                                .Select(cache.Graph, fsCustomerBillingSetupRow.BillingCycleID);

            FSBillingCycle oldbillingCycleRow = PXSelect<FSBillingCycle,
                                                Where<
                                                    FSBillingCycle.billingCycleID, Equal<Required<FSBillingCycle.billingCycleID>>>>
                                                .Select(cache.Graph, billingCycleIDOldValue);

            if (newbillingCycleRow.BillingBy != oldbillingCycleRow.BillingBy)
            {
                List<object> args = new List<object>();
                BqlCommand bqlCommand = null;
                string entityDocument = TX.Billing_By.SERVICE_ORDER;

                if (oldbillingCycleRow.BillingBy == ID.Billing_By.SERVICE_ORDER)
                {
                    bqlCommand = new Select2<FSServiceOrder,
                                     InnerJoin<FSServiceContract,
                                        On<FSServiceContract.serviceContractID, Equal<FSServiceOrder.billServiceContractID>>,
                                     InnerJoin<FSContractPeriod,
                                        On<FSContractPeriod.serviceContractID, Equal<FSServiceContract.serviceContractID>,
                                        And<FSContractPeriod.contractPeriodID, Equal<FSServiceOrder.billContractPeriodID>>>>>,
                                    Where<
                                         FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>,
                                         And<FSServiceOrder.billCustomerID, Equal<Required<FSServiceOrder.billCustomerID>>,
                                         And<FSServiceOrder.canceled, Equal<False>,
                                         And<FSServiceContract.status, NotEqual<FSServiceContract.status.Canceled>,
                                         And<FSContractPeriod.status, Equal<FSContractPeriod.status.Active>>>>>>>();
                }
                else if (oldbillingCycleRow.BillingBy == ID.Billing_By.APPOINTMENT)
                {
                    bqlCommand = new Select2<FSServiceOrder,
                                     InnerJoin<FSAppointment,
                                        On<FSAppointment.sOID, Equal<FSServiceOrder.sOID>>,
                                     InnerJoin<FSServiceContract,
                                        On<FSServiceContract.serviceContractID, Equal<FSAppointment.billServiceContractID>>,
                                     InnerJoin<FSContractPeriod,
                                        On<FSContractPeriod.serviceContractID, Equal<FSServiceContract.serviceContractID>,
                                        And<FSContractPeriod.contractPeriodID, Equal<FSAppointment.billContractPeriodID>>>>>>,
                                     Where<
                                         FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>,
                                         And<FSServiceOrder.billCustomerID, Equal<Required<FSServiceOrder.billCustomerID>>,
                                         And<FSAppointment.canceled, Equal<False>,
                                         And<FSServiceContract.status, NotEqual<FSServiceContract.status.Canceled>,
                                         And<FSContractPeriod.status, Equal<FSContractPeriod.status.Active>>>>>>>();

                    entityDocument = TX.Billing_By.APPOINTMENT;
                }

                args.Add(fsCustomerBillingSetupRow.SrvOrdType);
                args.Add(fsCustomerBillingSetupRow.CustomerID);

                PXView documentsView = new PXView(new PXGraph(), true, bqlCommand);
                var document = documentsView.SelectSingle(args.ToArray());

                if (document != null)
                {
                    PXException exception = new PXSetPropertyException(TX.Error.NO_UPDATE_BILLING_CYCLE_SERVICE_CONTRACT_RELATED, PXErrorLevel.Error, entityDocument);
					FSSetup fsSetupRow = GetFSSetup();
					if (fsSetupRow?.CustomerMultipleBillingOptions == false)
                    {
                        FSxCustomer fsxCustomerRow = Base.CurrentCustomer.Cache.GetExtension<FSxCustomer>(Base.CurrentCustomer.Current);

                        if (fsxCustomerRow != null)
                        {
                            Base.CurrentCustomer.Cache.RaiseExceptionHandling<FSxCustomer.billingCycleID>(Base.CurrentCustomer.Current,
                                                                                                          fsxCustomerRow.BillingCycleID,
                                                                                                          exception);
                        }
                    }
                    else
                    {
                        cache.RaiseExceptionHandling<FSCustomerBillingSetup.srvOrdType>(fsCustomerBillingSetupRow, 
                                                                                        fsCustomerBillingSetupRow.SrvOrdType, 
                                                                                        exception);
                    }

                    throw exception;
                }
            }
        }

        public virtual void UpdateBillCustomerInfoInDocsExtendCustomer(PXGraph callerGraph, int? currentCustomerID)
        {
            ServiceOrderEntry serviceOrderEntry = null;

            foreach(FSServiceOrder row in PXSelect<FSServiceOrder, 
                                                Where<FSServiceOrder.customerID, Equal<Required<FSServiceOrder.customerID>>,
                                                And<FSServiceOrder.billCustomerID, IsNull, And<FSServiceOrder.billLocationID, IsNull>>>>
                                                .Select(callerGraph, currentCustomerID))
            {
                if(serviceOrderEntry == null)
                {
                    serviceOrderEntry = PXGraph.CreateInstance<ServiceOrderEntry>();
                }

                serviceOrderEntry.ServiceOrderRecords.Current = serviceOrderEntry.ServiceOrderRecords.Search<FSServiceOrder.refNbr>(row.RefNbr, row.SrvOrdType);

                if (serviceOrderEntry.ServiceOrderRecords.Current != null)
                {
                    serviceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.billCustomerID>(serviceOrderEntry.ServiceOrderRecords.Current, row.CustomerID);
                    serviceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.billLocationID>(serviceOrderEntry.ServiceOrderRecords.Current, row.LocationID);

                    serviceOrderEntry.Save.Press();
                }
            }
        }

        #endregion

        #region Event Handlers

        #region BAccount Events
        protected virtual void _(Events.RowUpdated<BAccount> e)
        {
            if (e.Row == null)
            {
                return;
            }

            Customer customerRow = (Customer)Base.BAccount.Current;
            FSxCustomer fsxCustomerRow = Base.BAccount.Cache.GetExtension<FSxCustomer>(customerRow);

            if (e.OldRow.Type == BAccountType.ProspectType
                && e.Row.Type == BAccountType.CustomerType)
            {
                fsxCustomerRow.IsExtendingToCustomer = true;
            }
        }
        #endregion

        #region Customer Events

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        protected virtual void _(Events.FieldDefaulting<Customer, FSxCustomer.billLocationID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSxCustomer fsxCustomerRow = e.Cache.GetExtension<FSxCustomer>(e.Row);

            BAccountR BARow =   PXSelectJoin<BAccountR,
                                InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>,
                                          And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
                                Where<BAccountR.bAccountID, Equal<Required<BAccountR.bAccountID>>,
                                      And<CRLocation.isActive, Equal<True>,
                                      And<MatchWithBranch<CRLocation.cBranchID>>>>>
                                .Select(Base, fsxCustomerRow.BillCustomerID);

            if (BARow != null && BARow.DefLocationID != null)
            {
                e.NewValue = BARow.DefLocationID;
            }
            else
            {
                CRLocation CRLRow = PXSelect<CRLocation,
                                    Where<CRLocation.bAccountID, Equal<Required<CRLocation.bAccountID>>,
                                    And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.cBranchID>>>>>
                                    .Select(Base, fsxCustomerRow.BillCustomerID);

                if (CRLRow != null && CRLRow.LocationID != null)
                {
                    e.NewValue = CRLRow.LocationID;
                }
            }
        }
        #endregion
        #region FieldVerifying

        protected virtual void _(Events.FieldVerifying<Customer, Customer.customerClassID> e)
        {
            Customer customerRow = (Customer)e.Row;
            PXCache cache = e.Cache;

            CustomerClass customerClassRow = (CustomerClass)PXSelectorAttribute.Select<Customer.customerClassID>(cache, customerRow, e.NewValue);

            this.doCopyBillingSettings = false;

            if (customerClassRow != null)
            {
                this.doCopyBillingSettings = true;

                if (cache.GetStatus(customerRow) != PXEntryStatus.Inserted && Base.UnattendedMode == false && Base.IsContractBasedAPI == false)
                {
                    if (Base.BAccount.Ask(TX.WebDialogTitles.UPDATE_BILLING_SETTINGS, TX.Warning.CUSTOMER_CLASS_BILLING_SETTINGS, MessageButtons.YesNo) == WebDialogResult.No)
                    {
                        this.doCopyBillingSettings = false;
                    }
                }
            }
        }

        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<Customer, Customer.customerClassID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            Customer customerRow = (Customer)e.Row;
            PXCache cache = e.Cache;

            if (this.doCopyBillingSettings == true)
            {
                FSxCustomer fsxCustomerRow = cache.GetExtension<FSxCustomer>(customerRow);
                SetBillingCycleFromCustomerClass(cache, customerRow);
                InsertUpdateCustomerBillingSetup(cache, customerRow, fsxCustomerRow);
            }

            SetBillingCustomerSetting(cache, customerRow);
        }

        protected virtual void _(Events.FieldUpdated<Customer, FSxCustomer.billingCycleID> e)
        {
            if (e.Row == null)
            {
                return;
            }

			Customer customerRow = (Customer)e.Row;
			PXCache cache = e.Cache;

			FSxCustomer fsxCustomerRow = cache.GetExtension<FSxCustomer>(customerRow);

			if (fsxCustomerRow.BillingCycleID == null)
			{
				fsxCustomerRow.SendInvoicesTo = ID.Send_Invoices_To.DEFAULT_BILLING_CUSTOMER_LOCATION;
			}

			ResetSendInvoicesToFromBillingCycle(customerRow, null);
			InsertUpdateCustomerBillingSetup(cache, customerRow, fsxCustomerRow);

			if (e.NewValue != e.OldValue)
			{
				PXUIFieldAttribute.SetWarning<FSxCustomer.billingCycleID>(cache, customerRow, TX.Warning.SystemUsesSpecifiedBillingCycleSpecified);
			}
		}

        protected virtual void _(Events.FieldUpdated<Customer, FSxCustomer.sendInvoicesTo> e)
        {
            if (e.Row == null)
            {
                return;
            }

            Customer customerRow = (Customer)e.Row;
            PXCache cache = e.Cache;

            FSxCustomer fsxCustomerRow = cache.GetExtension<FSxCustomer>(customerRow);
            InsertUpdateCustomerBillingSetup(cache, customerRow, fsxCustomerRow);
        }

        protected virtual void _(Events.FieldUpdated<Customer, FSxCustomer.billShipmentSource> e)
        {
            if (e.Row == null)
            {
                return;
            }

            Customer customerRow = (Customer)e.Row;
            PXCache cache = e.Cache;

            FSxCustomer fsxCustomerRow = cache.GetExtension<FSxCustomer>(customerRow);
            InsertUpdateCustomerBillingSetup(cache, customerRow, fsxCustomerRow);
        }

        protected virtual void _(Events.FieldUpdated<Customer, FSxCustomer.billCustomerID> e)
        {
            if (e.Row == null)
            {
                return;
            }
            
            FSxCustomer fsxCustomerRow = e.Cache.GetExtension<FSxCustomer>(e.Row);

            if ((int?)e.OldValue != fsxCustomerRow.BillCustomerID)
            {
                e.Cache.SetDefaultExt<FSxCustomer.billLocationID>(e.Row);
            }
        }
        #endregion

        protected virtual void _(Events.RowSelecting<Customer> e)
        {
        }

        protected virtual void _(Events.RowSelected<Customer> e)
        {
            if (e.Row == null)
            {
                return;
            }

            Customer customerRow = (Customer)e.Row;
            PXCache cache = e.Cache;

            FSxCustomer fsxCustomerRow = cache.GetExtension<FSxCustomer>(customerRow);
            PXUIFieldAttribute.SetEnabled<FSxCustomer.sendInvoicesTo>(cache, customerRow, fsxCustomerRow.BillingCycleID != null);
            PXUIFieldAttribute.SetEnabled<FSxCustomer.billShipmentSource>(cache, customerRow, fsxCustomerRow.BillingCycleID != null);

            DisplayCustomerBillingOptions(cache, customerRow, fsxCustomerRow);

            viewServiceOrderHistory.SetEnabled(customerRow.BAccountID > 0);
            viewAppointmentHistory.SetEnabled(customerRow.BAccountID > 0);
            viewEquipmentSummary.SetEnabled(customerRow.BAccountID > 0);
            viewContractScheduleSummary.SetEnabled(customerRow.BAccountID > 0);

            openMultipleStaffMemberBoard.SetEnabled(customerRow.BAccountID > 0);
            openSingleStaffMemberBoard.SetEnabled(customerRow.BAccountID > 0);

            EnableDisableCustomerBilling(cache, customerRow, fsxCustomerRow);
        }

        protected virtual void _(Events.RowInserting<Customer> e)
        {
        }

        protected virtual void _(Events.RowInserted<Customer> e)
        {
            if (e.Row == null)
            {
                return;
            }

            Customer customerRow = (Customer)e.Row;

            if (this.doCopyBillingSettings == false)
            {
                SetBillingCycleFromCustomerClass(e.Cache, customerRow);
            }
        }

        protected virtual void _(Events.RowUpdating<Customer> e)
        {
        }

        protected virtual void _(Events.RowUpdated<Customer> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (Base.IsCopyPasteContext == true)
            {
                foreach (var item in CustomerBillingCycles.Select())
                {
                    CustomerBillingCycles.Delete(item);
                }
            }
        }

        protected virtual void _(Events.RowDeleting<Customer> e)
        {
        }

        protected virtual void _(Events.RowDeleted<Customer> e)
        {
        }

        protected virtual void _(Events.RowPersisting<Customer> e)
        {
            if (e.Row == null)
            {
                return;
            }

            Customer customerRow = (Customer)e.Row;
            PXCache cache = e.Cache;

            FSxCustomer fsxCustomerRow = cache.GetExtension<FSxCustomer>(customerRow);

            if (e.Operation == PXDBOperation.Insert)
            {
                if (this.doCopyBillingSettings == false)
                {
                    InsertUpdateCustomerBillingSetup(cache, customerRow, fsxCustomerRow);
                }
            }
        }

		protected virtual void _(Events.RowPersisted<Customer> e)
		{
			if (e.Row == null)
			{
				return;
			}

			Customer customerRow = (Customer)e.Row;
			FSxCustomer fsxCustomerRow = e.Cache.GetExtension<FSxCustomer>(customerRow);

			if (e.Operation == PXDBOperation.Update && e.TranStatus == PXTranStatus.Completed && fsxCustomerRow.IsExtendingToCustomer == true)
			{
				UpdateBillCustomerInfoInDocsExtendCustomer(Base, customerRow.BAccountID);
			}
        }

        #endregion

        #region FSCustomerSetupBillingEvents

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSCustomerBillingSetup, FSCustomerBillingSetup.frequencyType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSCustomerBillingSetup fsCustomerBillingSetupRow = (FSCustomerBillingSetup)e.Row;
            ResetTimeCycleOptions(fsCustomerBillingSetupRow);
        }

        protected virtual void _(Events.FieldUpdated<FSCustomerBillingSetup, FSCustomerBillingSetup.billingCycleID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSCustomerBillingSetup fsCustomerBillingSetupRow = (FSCustomerBillingSetup)e.Row;
            ResetSendInvoicesToFromBillingCycle(null, fsCustomerBillingSetupRow);

			if (e.NewValue != e.OldValue)
			{
				PXUIFieldAttribute.SetWarning<FSxCustomer.billingCycleID>(e.Cache, e.Row, TX.Warning.SystemUsesSpecifiedBillingCycleSpecified);
			}
		}

        #endregion

        protected virtual void _(Events.RowSelecting<FSCustomerBillingSetup> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSCustomerBillingSetup> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSCustomerBillingSetup fsCustomerBillingSetupRow = (FSCustomerBillingSetup)e.Row;
            PXCache cache = e.Cache;

            bool enableFieldsBySrvOrdType = string.IsNullOrEmpty(fsCustomerBillingSetupRow.SrvOrdType) == false;
            bool enableSrvOrdType = !enableFieldsBySrvOrdType || cache.GetStatus(fsCustomerBillingSetupRow) == PXEntryStatus.Inserted;

            PXUIFieldAttribute.SetEnabled<FSCustomerBillingSetup.srvOrdType>(cache, fsCustomerBillingSetupRow, enableSrvOrdType);
            PXUIFieldAttribute.SetEnabled<FSCustomerBillingSetup.billingCycleID>(cache, fsCustomerBillingSetupRow, enableFieldsBySrvOrdType);

            //Disables the TimeCycleType field if the type of the BillingCycleID selected is Time Cycle.
            if (fsCustomerBillingSetupRow.BillingCycleID != null)
            {
                FSBillingCycle fsBillingCycleRow = FSBillingCycle.PK.Find(Base, fsCustomerBillingSetupRow.BillingCycleID);

                PXUIFieldAttribute.SetEnabled<FSCustomerBillingSetup.frequencyType>(cache, fsCustomerBillingSetupRow, fsBillingCycleRow.BillingCycleType != ID.Billing_Cycle_Type.TIME_FRAME);

                bool forbidUpdateBillingOptions = SharedFunctions.IsNotAllowedBillingOptionsModification(fsBillingCycleRow);

                PXUIFieldAttribute.SetEnabled<FSCustomerBillingSetup.sendInvoicesTo>(cache, fsCustomerBillingSetupRow, forbidUpdateBillingOptions == false);
                PXUIFieldAttribute.SetEnabled<FSCustomerBillingSetup.billShipmentSource>(cache, fsCustomerBillingSetupRow, forbidUpdateBillingOptions == false);
            }
            else
            {
                PXUIFieldAttribute.SetEnabled<FSCustomerBillingSetup.frequencyType>(cache, fsCustomerBillingSetupRow, false);
            }
        }

        protected virtual void _(Events.RowUpdating<FSCustomerBillingSetup> e)
        {
            if (e.Row == null)
            {
                return;
            }

            VerifyPrepaidContractRelated(e.Cache, e.Row);
        }
		
        #endregion

        #endregion
    }
}
