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

using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
    #region PXProjection
    [Serializable]
    [PXProjection(typeof(
            Select2<FSContractPostDoc,
                 InnerJoin<FSServiceContract,
                    On<FSServiceContract.serviceContractID, Equal<FSContractPostDoc.serviceContractID>>,
                LeftJoin<Customer,
                    On<Customer.bAccountID, Equal<FSServiceContract.billCustomerID>>>>>))]
    #endregion
    public class ContractPostBatchDetail : IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<ContractPostBatchDetail>.By<contractPostDocID, contractPostBatchID>
        {
            public static ContractPostBatchDetail Find(PXGraph graph, int? contractPostDocID, int? contractPostBatchID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, contractPostDocID, contractPostBatchID, options);
        }

        public static class FK
        {
            public class ContractPostDocument : FSContractPostDoc.PK.ForeignKeyOf<ContractPostBatchDetail>.By<contractPostDocID> { }
            public class ContractPostBatch : FSContractPostBatch.PK.ForeignKeyOf<ContractPostBatchDetail>.By<contractPostBatchID> { }
            public class BillCustomer : AR.Customer.PK.ForeignKeyOf<ContractPostBatchDetail>.By<billCustomerID> { }
            public class BillCustomerLocation : Location.PK.ForeignKeyOf<ContractPostBatchDetail>.By<billCustomerID, billLocationID> { }
            public class Branch : GL.Branch.PK.ForeignKeyOf<ContractPostBatchDetail>.By<branchID> { }
            public class BranchLocation : FSBranchLocation.PK.ForeignKeyOf<ContractPostBatchDetail>.By<branchLocationID> { }
            public class ServiceContract : FSServiceContract.PK.ForeignKeyOf<ContractPostBatchDetail>.By<serviceContractID> { }
        }
        #endregion

        #region ContractPostDocID
        public abstract class contractPostDocID : PX.Data.BQL.BqlInt.Field<contractPostDocID> { }

        [PXDBInt(IsKey = true, BqlField = typeof(FSContractPostDoc.contractPostDocID))]
        [PXUIField(DisplayName = "Contract Post Doc. ID")]
        public virtual int? ContractPostDocID { get; set; }
        #endregion
        #region ContractPostBatchID
        public abstract class contractPostBatchID : PX.Data.BQL.BqlInt.Field<contractPostBatchID> { }

        [PXDBInt(IsKey = true, BqlField = typeof(FSContractPostDoc.contractPostBatchID))]
        [PXUIField(DisplayName = "Contract Post Batch ID")]
        public virtual int? ContractPostBatchID { get; set; }
        #endregion
        #region PostedTO
        public abstract class postedTO : PX.Data.BQL.BqlString.Field<postedTO> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa", BqlField = typeof(FSContractPostDoc.postedTO))]
        [PXUIField(DisplayName = "Posted to")]
        public virtual string PostedTO { get; set; }
        #endregion
        #region PostRefNbr
        public abstract class postRefNbr : PX.Data.BQL.BqlString.Field<postRefNbr> { }

        [PXDBString(15, BqlField = typeof(FSContractPostDoc.postRefNbr))]
        [PXUIField(DisplayName = "Document Nbr.", Enabled = false)]
        public virtual string PostRefNbr { get; set; }
        #endregion
        #region PostDocType
        public abstract class postDocType : PX.Data.BQL.BqlString.Field<postDocType> { }

        [PXDBString(3, BqlField = typeof(FSContractPostDoc.postDocType))]
        [PXUIField(DisplayName = "Document Type", Enabled = false)]
        public virtual string PostDocType { get; set; }
        #endregion
        #region ContractRefNbr
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(FSServiceContract.refNbr))]
        [PXUIField(DisplayName = "Service Contract ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<FSServiceContract.refNbr>))]
		public virtual string ContractRefNbr { get; set; }
        #endregion
        #region CustomerContractNbr
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(FSServiceContract.customerContractNbr))]
        [PXUIField(DisplayName = "Customer Contract Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string CustomerContractNbr { get; set; }
        #endregion
        #region ServiceContractID
        public abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

        [PXDBInt(BqlField = typeof(FSServiceContract.serviceContractID))]
        public virtual int? ServiceContractID { get; set; }
        #endregion
        #region BillCustomerID
        public abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        [PXDBInt(BqlField = typeof(FSServiceContract.billCustomerID))]
        [PXUIField(DisplayName = "Billing Customer ID")]
        [FSSelectorBAccountTypeCustomerOrCombined]
        public virtual int? BillCustomerID { get; set; }
        #endregion
        #region AcctName
        [PXDBString(60, IsUnicode = true, BqlField = typeof(Customer.acctName))]
        [PXFieldDescription]
        [PXUIField(DisplayName = "Customer Name", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string AcctName { get; set; }
        #endregion
        #region BillLocationID
        public abstract class billLocationID : PX.Data.BQL.BqlInt.Field<billLocationID> { }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<ContractPostBatchDetail.billCustomerID>>>),
                    BqlField = typeof(FSServiceContract.billLocationID),
                    DescriptionField = typeof(Location.descr), DisplayName = "Billing Location", DirtyRead = true)]
        public virtual int? BillLocationID { get; set; }
        #endregion
        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

        [PXUIField(DisplayName = "Start Date")]
        [PXDBDate(BqlField = typeof(FSServiceContract.startDate))]
        public virtual DateTime? StartDate { get; set; }
        #endregion
        #region NextBillingInvoiceDate 
        public abstract class nextBillingInvoiceDate : PX.Data.BQL.BqlDateTime.Field<nextBillingInvoiceDate> { }

        [PXDBDate(BqlField = typeof(FSServiceContract.nextBillingInvoiceDate))]
        [PXUIField(DisplayName = "Next Billing Date", Enabled = false)]
        public virtual DateTime? NextBillingInvoiceDate { get; set; }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXDBInt(BqlField = typeof(FSServiceContract.branchID))]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual int? BranchID { get; set; }
        #endregion
        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt(BqlField = typeof(FSServiceContract.branchLocationID))]
        [PXUIField(DisplayName = "Branch Location")]
        [PXSelector(typeof(Search<FSBranchLocation.branchLocationID,
                    Where<FSBranchLocation.branchID, Equal<Current<ContractPostBatchDetail.branchID>>>>),
                    SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
                    DescriptionField = typeof(FSBranchLocation.descr))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region DocDesc
        public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }

        [PXDBString(255, IsUnicode = true, BqlField = typeof(FSServiceContract.docDesc))]
        [PXUIField(DisplayName = "Description")]
        public virtual string DocDesc { get; set; }
        #endregion
    }
}
