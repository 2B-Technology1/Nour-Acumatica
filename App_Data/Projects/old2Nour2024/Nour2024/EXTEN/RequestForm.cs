using System;
using PX.Data;
using PX.Objects.AR;
using MyMaintaince;
using PX.Data.BQL;
namespace NourSc202007071
{
    [System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(RequestFormMaint3))]
    public class RequestForm : IBqlTable
    {
        public class statusConatant : PX.Data.IBqlField
        {
            //Constant declaration
            public const string open = "Open";
            public const string Closed = "Closed";
            public const string Cancel = "Cancel";
            public const string Hold = "Hold";

            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                : base
                    (
                        new string[] { open, Closed, Cancel, Hold },
                        new string[] { open, Closed, Cancel, Hold }
                    )
                {; }
            }
            public class Open :BqlString.Constant<Open>
            {
                public Open() : base(open) {; }
            }
            public class closed :BqlString.Constant<closed>
            {
                public closed() : base(Closed) {; }
            }
            public class cancel :BqlString.Constant<cancel>
            {
                public cancel() : base(Cancel) {; }
            }
            public class hold :BqlString.Constant<hold>
            {
                public hold() : base(Hold) {; }
            }

        }
        #region statusDocType
        public abstract class Doctype : PX.Data.IBqlField
        {
            //Constant declaration
            public const string SparPart = "Spare Parts";
            public const string Warranty = "Warranty";
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                        new string[] { SparPart, Warranty },
                        new string[] { SparPart, Warranty })
                {; }
            }
            public class wararanty : BqlString.Constant<wararanty>
            {
                public wararanty() : base(Warranty) {; }
            }
            public class sparePart :BqlString.Constant<sparePart>
            {
                public sparePart() : base(SparPart) {; }
            }
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.IBqlField
        {
        }
        protected string _RefNbr;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "RefNbr")]
        [PXDefault("<NEW>")]
        [PXSelector(typeof(Search<RequestForm.refNbr, Where<RequestForm.branchID, Equal<Current<AccessInfo.branchID>>>>))]
        public virtual string RefNbr
        {
            get
            {
                return this._RefNbr;
            }
            set
            {
                this._RefNbr = value;
            }
        }
        #endregion
        #region DocType
        public abstract class docType : PX.Data.IBqlField
        {
        }
        protected string _DocType;
        [PXDBString(50, IsUnicode = true)]
        [PXDefault("Warranty")]
        [PXUIField(DisplayName = "DocType")]
        [Doctype.List()]
        public virtual string DocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }
        #endregion
        #region RequstDate
        public abstract class requstDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _RequstDate;
        [PXDBDate()]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "RequstDate")]
        public virtual DateTime? RequstDate
        {
            get
            {
                return this._RequstDate;
            }
            set
            {
                this._RequstDate = value;
            }
        }
        #endregion
        #region Status
        public abstract class status : PX.Data.IBqlField
        {
        }
        protected string _Status;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault("Open")]
        [PXUIField(DisplayName = "Status")]
        [statusConatant.List()]
        public virtual string Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                this._Status = value;
            }
        }
        #endregion
        #region Customer
        public abstract class customer : PX.Data.IBqlField
        {
        }
        protected string _Customer;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Customer")]
        [PXDefault()]
        [PXSelector(typeof(Search2<Customer.acctCD, InnerJoin<ItemCustomers, On<ItemCustomers.customerID, Equal<Customer.bAccountID>>,
                                                        InnerJoin<Items, On<Items.itemsID, Equal<ItemCustomers.itemsID>>>>,
                                                        Where<Items.code, Equal<Current<RequestForm.vechile>>>>)
                  , new Type[]{
                       typeof(Customer.acctCD),
                       typeof(Customer.acctName)
                         }
                  , DescriptionField = typeof(Customer.acctName)
                  , SubstituteKey = typeof(Customer.acctCD))]


        public virtual string Customer
        {
            get
            {
                return this._Customer;
            }
            set
            {
                this._Customer = value;
            }
        }
        #endregion
        // #region Name
        // public abstract class name : PX.Data.IBqlField
        // {
        // }
        // protected string _Name;
        // [PXDBString(200, IsUnicode = true)]
        // [PXUIField(DisplayName = "Name")]
        //// [PXDefault(typeof(Search<PX.Objects.AR.Customer.acctName, Where<PX.Objects.AR.Customer.acctCD, Equal<Current<RequestForm.customer>>>>))]
        // public virtual string Name
        // {
        //     get
        //     {
        //         return this._Name;
        //     }
        //     set
        //     {
        //         this._Name = value;
        //     }
        // }
        // #endregion
        #region Vechile
        public abstract class vechile : PX.Data.IBqlField
        {
        }
        protected string _Vechile;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Vechile")]
        //[PXSelector(typeof(Search2<Items.code, InnerJoin<ItemCustomers, On<ItemCustomers.itemsID, Equal<Items.itemsID>>>,
        //Where<ItemCustomers.customerID, Equal<Current<RequestForm.customer>>>>),
        //[PXSelector(typeof(Search2<Items.code,InnerJoin<ItemCustomers, On<ItemCustomers.itemsID, Equal<Items.itemsID>>>>))]
        //Where<ItemCustomers.customerID, Equal<Current<RequestForm.customer>>>>),
        //new Type[] { typeof(Items.code) })]
        [PXSelector(typeof(Search<Items.code>)
                    , new Type[] {
                        typeof(Items.code),
                        typeof(Items.name),
                        typeof(Items.brandID),
                        typeof(Items.modelID),
                        }
                    , DescriptionField = typeof(Items.name)
                    , SubstituteKey = typeof(Items.code))]
        public virtual string Vechile
        {
            get
            {
                return this._Vechile;
            }
            set
            {
                this._Vechile = value;
            }
        }
        #endregion
        #region JobOrder
        public abstract class jobOrder : PX.Data.IBqlField
        {
        }
        protected string _JobOrder;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "JobOrder")]
        [PXSelector(typeof(Search<JobOrder.jobOrdrID, Where<JobOrder.customer, Equal<Current<RequestForm.customer>>>>))]
        // [PXSelector(typeof(Search<JobOrder.jobOrdrID, Where<JobOrder.itemsID, Equal<Current<RequestForm.vechile>>, And<JobOrder.customer, Equal<Current<RequestForm.customer>>, And<JobOrder.branchID, Equal<Current<AccessInfo.branchID>>>>>>))]
        public virtual string JobOrder
        {
            get
            {
                return this._JobOrder;
            }
            set
            {
                this._JobOrder = value;
            }
        }
        #endregion
        #region CaseNbr
        public abstract class caseNbr : PX.Data.IBqlField
        {
        }
        protected int? _CaseNbr;
        [PXDBInt()]
        [PXUIField(DisplayName = "CaseNbr")]
        [PXDefault(0)]
        public virtual int? CaseNbr
        {
            get
            {
                return this._CaseNbr;
            }
            set
            {
                this._CaseNbr = value;
            }
        }
        #endregion
        #region Descrption
        public abstract class descrption : PX.Data.IBqlField
        {
        }
        protected string _Descrption;
        [PXDBString(2147483647, IsUnicode = true)]
        [PXUIField(DisplayName = "Descrption")]
        [PXDefault()]
        public virtual string Descrption
        {
            get
            {
                return this._Descrption;
            }
            set
            {
                this._Descrption = value;
            }
        }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.IBqlField
        {
        }
        protected int? _BranchID;
        [PXDBInt()]
        [PXUIField(DisplayName = "BranchID")]
        [PXDBDefault(typeof(AccessInfo.branchID))]
        public virtual int? BranchID
        {
            get
            {
                return this._BranchID;
            }
            set
            {
                this._BranchID = value;
            }
        }
        #endregion

        #region CreatedByID

        public abstract class createdByID : PX.Data.IBqlField
        {
        }
        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
        [PXUIField(DisplayName = "Created By")]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion

        #region UsrDlvrDate
        [PXDBDate]
        [PXUIField(DisplayName = "Delivery Date")]
        public virtual DateTime? UsrDlvrDate { get; set; }
        public abstract class usrDlvrDate : IBqlField { }
        #endregion

        #region UsrApprovalDate
        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Warranty Approval Date", Enabled = false, IsReadOnly = true)]
        public virtual DateTime? UsrApprovalDate { get; set; }
        public abstract class usrApprovalDate : IBqlField { }
        #endregion

        #region UsrExpDlvrDate
        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Expected Delivery Date")]
        public virtual DateTime? UsrExpDlvrDate { get; set; }
        public abstract class usrExpDlvrDate : IBqlField { }
        #endregion

        #region UsrExpDlvrDate2
        [PXDBDate]
        [PXUIField(DisplayName = "2nd Expected Delivery Date")]
        public virtual DateTime? UsrExpDlvrDate2 { get; set; }
        public abstract class usrExpDlvrDate2 : IBqlField { }
        #endregion

        #region UsrWarrantyStatus
        public abstract class usrWarrantyStatus : PX.Data.IBqlField
        {
        }
        protected string _UsrWarrantyStatus;
        [PXDBString(200, IsUnicode = true)]
        [PXUIField(DisplayName = "Warranty Status")]
        //[PXDefault]
        [PXStringList(new string[] { "Waiting Approval", "Approved", "Rejected" },
                          new string[] { "Waiting Approval", "Approved", "Rejected" })]
        [PXDefault("")]
        public virtual string UsrWarrantyStatus
        {
            get
            {
                return this._UsrWarrantyStatus;
            }
            set
            {
                this._UsrWarrantyStatus = value;
            }
        }
        #endregion

        #region UsrPartsStatus
        public abstract class usrPartsStatus : PX.Data.IBqlField
        {
        }
        protected string _UsrPartsStatus;
        [PXDBString(200, IsUnicode = true)]
        [PXUIField(DisplayName = "Parts Status")]
        [PXDefault]
        [PXStringList(new string[] { "Available", "Not Available" },
                          new string[] { "Available", "Not Available" })]
        //[PXDefault("")]
        public virtual string UsrPartsStatus
        {
            get
            {
                return this._UsrPartsStatus;
            }
            set
            {
                this._UsrPartsStatus = value;
            }
        }
        #endregion

        #region approvedby
        public abstract class approvedby : PX.Data.IBqlField
        {
        }
        protected string _Approvedby;
        [PXDBString(300, IsUnicode = true)]
        [PXUIField(DisplayName = "Warranty Approved by", Enabled = false, IsReadOnly = true)]
        public virtual string Approvedby
        {
            get
            {
                return this._Approvedby;
            }
            set
            {
                this._Approvedby = value;
            }
        }
        #endregion

        #region LastModifiedByID

        public abstract class lastModifiedByID : PX.Data.IBqlField
        {
        }
        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion

        #region PartsDescr
        public abstract class partsDescr : PX.Data.IBqlField
        {
        }
        protected string _PartsDescr;
        [PXDBString(2147483647, IsUnicode = true)]
        [PXUIField(DisplayName = "Spare Parts Notes")]
        // [PXDefault("")]
        public virtual string PartsDescr
        {
            get
            {
                return this._PartsDescr;
            }
            set
            {
                this._PartsDescr = value;
            }
        }
        #endregion

        #region WarrantyDescr
        public abstract class warrantyDescr : PX.Data.IBqlField
        {
        }
        protected string _WarrantyDescr;
        [PXDBString(2147483647, IsUnicode = true)]
        [PXUIField(DisplayName = "Warranty Notes")]
        // [PXDefault("")]
        public virtual string WarrantyDescr
        {
            get
            {
                return this._WarrantyDescr;
            }
            set
            {
                this._WarrantyDescr = value;
            }
        }
        #endregion

        #region LastModefidDateTime
        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modefid Date", IsReadOnly = true, Enabled = false)]
        public virtual DateTime? LastModefidDateTime { get; set; }
        public abstract class lastModefidDateTime : IBqlField { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created Date", IsReadOnly = true, Enabled = false)]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : IBqlField { }
        #endregion

        #region ClosedDateTime
        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Closed Date", IsReadOnly = true, Enabled = false)]
        public virtual DateTime? ClosedDateTime { get; set; }
        public abstract class closedDateTime : IBqlField { }
        #endregion
        #region closedBy
        public abstract class closedBy : PX.Data.IBqlField
        {
        }
        protected string _ClosedBy;
        [PXDBString(300, IsUnicode = true)]
        [PXUIField(DisplayName = "Closed by", Enabled = false, IsReadOnly = true)]
        public virtual string ClosedBy
        {
            get
            {
                return this._ClosedBy;
            }
            set
            {
                this._ClosedBy = value;
            }
        }
        #endregion

        #region PartsApprovalDate 
        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Parts Approval Date", Enabled = false, IsReadOnly = true)]
        public virtual DateTime? PartsApprovalDate { get; set; }
        public abstract class partsApprovalDate : IBqlField { }
        #endregion

        #region partsApprovedby
        public abstract class partsApprovedby : PX.Data.IBqlField
        {
        }
        protected string _PartsApprovedby;
        [PXDBString(300, IsUnicode = true)]
        [PXUIField(DisplayName = "Parts Approved by", Enabled = false, IsReadOnly = true)]
        public virtual string PartsApprovedby
        {
            get
            {
                return this._PartsApprovedby;
            }
            set
            {
                this._PartsApprovedby = value;
            }
        }
        #endregion
    }
}