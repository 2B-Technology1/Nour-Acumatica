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

namespace PX.Objects.FS
{
     public class FSxCustomer : PXCacheExtension<Customer>
     {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region BillingCycleID
        public abstract class billingCycleID : PX.Data.BQL.BqlInt.Field<billingCycleID> { }
        [PXDBInt]
        [PXDefault(typeof(Search<FSxCustomerClass.defaultBillingCycleID,
                        Where<
                            CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>)
                , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Billing Cycle")]
        [PXSelector(typeof(FSBillingCycle.billingCycleID), SubstituteKey = typeof(FSBillingCycle.billingCycleCD), DescriptionField = typeof(FSBillingCycle.descr))]
        public virtual int? BillingCycleID { get; set; }
		#endregion
		#region SendInvoicesTo
		public abstract class sendInvoicesTo : ListField_Send_Invoices_To { }
        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO, PersistingCheck = PXPersistingCheck.Nothing)]
        [sendInvoicesTo.ListAtrribute]
        [PXUIField(DisplayName = "Bill-To Address")]
        public virtual string SendInvoicesTo { get; set; }
		#endregion
		#region BillShipmentSource
		public abstract class billShipmentSource : ListField_Ship_To { }
        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Ship_To.SERVICE_ORDER_ADDRESS, PersistingCheck = PXPersistingCheck.Nothing)]
        [billShipmentSource.ListAtrribute]
        [PXUIField(DisplayName = "Ship-To Address")]
        public virtual string BillShipmentSource { get; set; }
		#endregion
		#region DefaultBillingCustomerSource
		public abstract class defaultBillingCustomerSource : ListField_Default_Billing_Customer_Source { }
        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Default_Billing_Customer_Source.SERVICE_ORDER_CUSTOMER, PersistingCheck = PXPersistingCheck.Nothing)]
        [defaultBillingCustomerSource.ListAtrribute]
        [PXUIField(DisplayName = "Default Billing Customer")]
        public virtual string DefaultBillingCustomerSource { get; set; }
        #endregion
        #region BillCustomerID
        public abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Billing Customer")]
        [FSSelectorCustomer]
        public virtual int? BillCustomerID { get; set; }
        #endregion
        #region BillLocationID
        public abstract class billLocationID : PX.Data.BQL.BqlInt.Field<billLocationID> { }
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [LocationActive(typeof(Where<Location.bAccountID, Equal<Current<FSxCustomer.billCustomerID>>,
                            And<MatchWithBranch<Location.cBranchID>>>),
                    DescriptionField = typeof(Location.descr), DisplayName = "Billing Location")]
        public virtual int? BillLocationID { get; set; }
        #endregion
        #region RequireCustomerSignature
        public abstract class requireCustomerSignature : PX.Data.BQL.BqlBool.Field<requireCustomerSignature> { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Require Customer Signature on Mobile App")]
        public virtual bool? RequireCustomerSignature { get; set; }
        #endregion
        #region ChkServiceManagement
        public abstract class ChkServiceManagement : PX.Data.BQL.BqlBool.Field<ChkServiceManagement> { }
        [PXBool]
        [PXUIField(Visible = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? chkServiceManagement
        {
            get
            {
                return true;
            }
        }
        #endregion

        #region IsExtendingToCustomer
        public abstract class isExtendingToCustomer : PX.Data.BQL.BqlBool.Field<isExtendingToCustomer> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? IsExtendingToCustomer { get; set; }
        #endregion
    }
}
