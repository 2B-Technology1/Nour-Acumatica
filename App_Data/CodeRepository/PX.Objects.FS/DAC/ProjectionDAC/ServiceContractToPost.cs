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
using PX.Objects.GL;
using System;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
    [Serializable]
    [PXProjection(typeof(
        Select5<FSServiceContract,
                  InnerJoin<FSContractPeriod,
                        On<FSContractPeriod.serviceContractID, Equal<FSServiceContract.serviceContractID>>,
                  InnerJoin<FSContractPeriodDet,
                        On<FSContractPeriodDet.contractPeriodID, Equal<FSContractPeriod.contractPeriodID>>>>,
                  Where<
                        FSContractPeriod.invoiced, Equal<False>,
                        And<FSServiceContract.status, Equal<FSServiceContract.status.Active>,
                        And2<
                            Where<FSServiceContract.billingType, Equal<FSServiceContract.billingType.Values.standardizedBillings>,
                               Or<FSServiceContract.billingType, Equal<FSServiceContract.billingType.Values.fixedRateAsPerformedBillings>,
                               Or<FSServiceContract.billingType, Equal<FSServiceContract.billingType.Values.fixedRateBillings>>>>,
                        And<
                Where<
                    FSContractPeriod.status, Equal<FSContractPeriod.status.Active>,
                    Or<FSContractPeriod.status, Equal<FSContractPeriod.status.Pending>>>>>>>,
                Aggregate<
                    GroupBy<FSServiceContract.serviceContractID,
                    GroupBy<FSServiceContract.noteID,
                    GroupBy<FSContractPeriod.contractPeriodID>>>>>))]
    [PXGroupMask(typeof(InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<ContractPeriodToPost.billCustomerID>, And<Match<Customer, Current<AccessInfo.userName>>>>>))]
    public partial class ContractPeriodToPost : IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<ContractPeriodToPost>.By<serviceContractID, contractPeriodID>
        {
            public static ContractPeriodToPost Find(PXGraph graph, int? serviceContractID, int? contractPeriodID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, serviceContractID, contractPeriodID, options);
        }

        public static class FK
        {
            public class Customer : AR.Customer.PK.ForeignKeyOf<ContractPeriodToPost>.By<customerID> { }
            public class ServiceContract : FSServiceContract.UK.ForeignKeyOf<ContractPeriodToPost>.By<refNbr> { }
            public class BillCustomer : AR.Customer.PK.ForeignKeyOf<ContractPeriodToPost>.By<billCustomerID> { }
            public class BillCustomerLocation : Location.PK.ForeignKeyOf<ContractPeriodToPost>.By<billCustomerID, billLocationID> { }
            public class Branch : GL.Branch.PK.ForeignKeyOf<ContractPeriodToPost>.By<branchID> { }
            public class BranchLocation : FSBranchLocation.PK.ForeignKeyOf<ContractPeriodToPost>.By<branchLocationID> { }
            public class ContractPeriod : FSContractPeriod.UK.ForeignKeyOf<ContractPeriodToPost>.By<contractPeriodID> { }
            public class ContractPostBatch : FSContractPostBatch.PK.ForeignKeyOf<ContractPeriodToPost>.By<contractPostBatchID> { }
        }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID>
		{
        }

        [PXDBInt(BqlField = typeof(FSServiceContract.customerID))]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
        }

        [PXDBString(BqlField = typeof(FSServiceContract.refNbr))]
        [PXUIField(DisplayName = "Service Contract ID", Enabled = true, Visibility = PXUIVisibility.SelectorVisible, Visible = true)]
        [PXSelector(typeof(Search<FSServiceContract.refNbr, Where<FSServiceContract.customerID, Equal<Current<ContractPeriodToPost.customerID>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion

        #region CustomerContractNbr
        public abstract class customerContractNbr : PX.Data.IBqlField
        {
        }

        //Included in FSRouteContractScheduleFSServiceContract projection
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(FSServiceContract.customerContractNbr))]
        [PXUIField(DisplayName = "Customer Contract Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false)]
        [FSSelectorCustomerContractNbrAttributeAttribute(typeof(ListField_RecordType_ContractSchedule.ServiceContract), typeof(ContractPeriodToPost.customerID))]
        public virtual string CustomerContractNbr { get; set; }
        #endregion
        #region ServiceContractID
        public abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

        [PXDBInt(BqlField = typeof(FSServiceContract.serviceContractID), IsKey = true)]
        public virtual int? ServiceContractID { get; set; }
        #endregion
        #region BillCustomerID
        public abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        [PXDBInt(BqlField = typeof(FSServiceContract.billCustomerID))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Billing Customer", Enabled = false)]
        [FSSelectorBAccountTypeCustomerOrCombined]
        [PXRestrictor(typeof(Where<Customer.status, IsNull,
               Or<Customer.status, Equal<CustomerStatus.active>,
               Or<Customer.status, Equal<CustomerStatus.oneTime>>>>),
               PX.Objects.AR.Messages.CustomerIsInStatus, typeof(Customer.status))]
        public virtual int? BillCustomerID { get; set; }
        #endregion
        #region BillLocationID
        public abstract class billLocationID : PX.Data.BQL.BqlInt.Field<billLocationID>
		{
        }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<ContractPeriodToPost.billCustomerID>>>), 
                    BqlField = typeof(FSServiceContract.billLocationID), DescriptionField = typeof(Location.descr), 
                    DirtyRead = true, DisplayName = "Billing Location", Enabled = false)]
        public virtual int? BillLocationID { get; set; }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID>
		{
        }

        [PXDBInt(BqlField = typeof(FSServiceContract.branchID))]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Branch.branchID), DescriptionField = typeof(Branch.acctName), SubstituteKey = typeof(Branch.branchCD))]
        public virtual int? BranchID { get; set; }
        #endregion
        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID>
		{
        }

        [PXDBInt(BqlField = typeof(FSServiceContract.branchLocationID))]
        [PXUIField(DisplayName = "Branch Location")]
        [PXSelector(typeof(Search<FSBranchLocation.branchLocationID, Where<FSBranchLocation.branchID, Equal<Current<ContractPeriodToPost.branchID>>>>), 
                           DescriptionField = typeof(FSBranchLocation.descr), SubstituteKey = typeof(FSBranchLocation.branchLocationCD))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region BillingType
        public abstract class billingType : PX.Data.BQL.BqlString.Field<billingType>
        {
            public abstract class Values : ListField.ServiceContractBillingType { }
        }

        [PXDBString(4, IsUnicode = true, BqlField = typeof(FSServiceContract.billingType))]
        [billingType.Values.List]
        [PXUIField(DisplayName = "Billing Type")]
        public virtual string BillingType { get; set; }
        #endregion
        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate>
		{
        }

        [PXDBDate(BqlField = typeof(FSServiceContract.startDate))]
        [PXUIField(DisplayName = "Start Date")]
        public virtual DateTime? StartDate { get; set; }
        #endregion
        #region Status
        public abstract class status : ListField_Status_ContractPeriod
        {
        }

        [PXDBString(BqlField = typeof(FSContractPeriod.status))]
        [ListField_Status_ContractPeriod.ListAttribute]
        [PXUIField(DisplayName = "Status", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Status { get; set; }
        #endregion
        #region DocDesc
        public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc>
		{
        }

        [PXDBString(BqlField = typeof(FSServiceContract.docDesc))]
        [PXUIField(DisplayName = "Description")]
        public virtual string DocDesc { get; set; }
        #endregion
        #region EndPeriodDate
        public abstract class endPeriodDate : PX.Data.BQL.BqlDateTime.Field<endPeriodDate>
		{
        }

        [PXDBDate(BqlField = typeof(FSContractPeriod.endPeriodDate))]
        public virtual DateTime? EndPeriodDate { get; set; }
        #endregion
        #region StartPeriodDate
        public abstract class startPeriodDate : PX.Data.BQL.BqlDateTime.Field<startPeriodDate>
		{
        }

        [PXDBDate(BqlField = typeof(FSContractPeriod.startPeriodDate))]
        public virtual DateTime? StartPeriodDate { get; set; }
        #endregion
        #region NextBillingInvoiceDate 
        public abstract class nextBillingInvoiceDate : PX.Data.BQL.BqlDateTime.Field<nextBillingInvoiceDate> { }

        [PXDBDate(BqlField = typeof(FSServiceContract.nextBillingInvoiceDate))]
        [PXUIField(DisplayName = "Next Billing Date", Enabled = false)]
        public virtual DateTime? NextBillingInvoiceDate { get; set; }
        #endregion
        #region ContractPeriodID
        public abstract class contractPeriodID : PX.Data.BQL.BqlInt.Field<contractPeriodID> { }

        [PXDBInt(BqlField = typeof(FSContractPeriod.contractPeriodID), IsKey = true)]

        public virtual int? ContractPeriodID { get; set; }
        #endregion
        #region ContractPostBatchID
        public abstract class contractPostBatchID : PX.Data.BQL.BqlInt.Field<contractPostBatchID> { }

        [PXInt]
        [PXUIField(DisplayName = "Batch Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<FSContractPostBatch.contractPostBatchID>), SubstituteKey = typeof(FSContractPostBatch.contractPostBatchNbr))]
        public virtual int? ContractPostBatchID { get; set; }
        #endregion
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected>
		{
        }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion
        #region BillingPeriod
        public abstract class billingPeriod : PX.Data.BQL.BqlString.Field<billingPeriod>
		{
        }

        [PXString]
        [PXUIField(DisplayName = "Billing Period", Enabled = false, IsReadOnly = true)]
        public virtual string BillingPeriod
        {
            get
            {
                if (this.StartPeriodDate.HasValue && this.EndPeriodDate.HasValue)
                    return this.StartPeriodDate.Value.ToString("MM/dd/yyyy") + " - " + this.EndPeriodDate.Value.ToString("MM/dd/yyyy");
                return string.Empty;
            }
        }
        #endregion
    }
}