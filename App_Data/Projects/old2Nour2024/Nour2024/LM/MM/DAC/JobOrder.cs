﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	using PX.Objects.EP;
	using PX.Objects.AR;
	using PX.Data.EP;
	using PX.Objects.CR;
	using Maintenance.MM;
    using Nour20220913V1;
    using Nour20231012VSolveUSDNew;

    [System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(JobOrderMaint))]
	public class JobOrder : PX.Data.IBqlTable
	{
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
        #region JobOrdrID
        public abstract class jobOrdrID : PX.Data.IBqlField
        {
        }
        protected string _JobOrdrID;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCC")]
        [PXDefault("<NEW>")]
        //[PXDefault()]
        //[PXUIField(DisplayName = "Job Order ID.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXUIField(DisplayName = "Job Order ID.")]
        //[JOAutoNumbering(typeof(SetUp.autoNumbering), typeof(SetUp.receiptLastRefNbr))]
        //[JOAutoNumbering(typeof(SetUp.autoNumbering),typeof(Search<SetUp.receiptLastRefNbr, Where<SetUp.branchCD, Equal<Current<JobOrder.branchesss>>>>))]
        //[JOAutoNumbering(typeof(SetUp.autoNumbering),typeof(Search<'ABCD10'>)];
        //[PXSelector(typeof(Search<SetUp.receiptLastRefNbr, Where<SetUp.branchCD, Equal<Current<JobOrder.branchesss>>>>)
        [PXSelector(typeof(Search<JobOrder.jobOrdrID, Where<JobOrder.branchID, Equal<Current<AccessInfo.branchID>>>>)
                          , new Type[] { typeof(JobOrder.jobOrdrID), typeof(JobOrder.customer) })]
        //[JobOrderAutoNumber(typeof(True),typeof(SetUp.receiptLastRefNbr))]
        //[AutoNumber(typeof(SetUp.receiptLastRefNbr), typeof(JobOrder.jobOrdrID))]
        //[MyMaintaince.AutoNumber(typeof(JobOrder.jobOrdrID), typeof(JobOrder.createDate), false, "<NEW>", "INISSUE")]
        public virtual string JobOrdrID
        {
            get
            {
                return this._JobOrdrID;
            }
            set
            {
                this._JobOrdrID = value;
            }
        }
        #endregion
		#region ItemsID
		public abstract class itemsID : PX.Data.IBqlField
		{
		}
		protected int? _ItemsID;
		[PXDBInt()]
		[PXDefault(0)]
        [PXUIField(DisplayName = "Vin Number")]
		[PXSelector(typeof(Search<Items.itemsID>)
                    , new Type[] { 
                        typeof(Items.code),
                        typeof(Items.name),
                        //typeof(Items.customer),
                        typeof(Items.brandID),
                        typeof(Items.modelID),
                        typeof(Items.purchesDate),
                        typeof(Items.lincensePlat),
                        typeof(Items.mgfDate),
                        typeof(Items.gurarantYear),
                    }
                    , DescriptionField = typeof(Items.name)
                    , SubstituteKey = typeof(Items.code))]
		public virtual int? ItemsID
		{
			get
			{
				return this._ItemsID;
			}
			set
			{
				this._ItemsID = value;
			}
		}
		#endregion
		#region ClassID
		public abstract class classID : PX.Data.IBqlField
		{
		}
		protected int? _ClassID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Class ID")]
		[PXSelector(typeof(Cls.classID),
                     new Type[] { typeof(Cls.Code), typeof(Cls.Code), typeof(Cls.Name) }
                     , DescriptionField = typeof(Cls.Name)
                     , SubstituteKey    = typeof(Cls.Code))]
		public virtual int? ClassID
		{
			get
			{
				return this._ClassID;
			}
			set
			{
				this._ClassID = value;
			}
		}
		#endregion
		#region AssignedTo
		public abstract class assignedTo : PX.Data.IBqlField
		{
		}
		protected string _AssignedTo;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Assigned To", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(EPEmployee.acctCD)
                     ,new Type[]{
                     typeof(EPEmployee.acctCD)
                     ,typeof(EPEmployee.acctName)
                     ,typeof(EPEmployee.classID)
                     }
                     , SubstituteKey = typeof(EPEmployee.acctCD)
                     , DescriptionField = typeof(EPEmployee.acctName))]
		public virtual string AssignedTo
		{
			get
			{
				return this._AssignedTo;
			}
			set
			{
				this._AssignedTo = value;
			}
		}
		#endregion
		#region WorkshopEngineer
		public abstract class workshopEngineer : PX.Data.IBqlField
		{
		}
		protected string _WorkshopEngineer;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Workshop Engineer", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(EPEmployee.acctCD)
                     , new Type[]{
                     typeof(EPEmployee.acctCD)
                     ,typeof(EPEmployee.acctName)
                     ,typeof(EPEmployee.classID)
                     }
                     , SubstituteKey = typeof(EPEmployee.acctCD)
                     , DescriptionField = typeof(EPEmployee.acctName))]
		public virtual string WorkshopEngineer
		{
			get
			{
				return this._WorkshopEngineer;
			}
			set
			{
				this._WorkshopEngineer = value;
			}
		}
		#endregion
		#region WarrantyEngineer
		public abstract class warrantyEngineer : PX.Data.IBqlField
		{
		}
		protected string _WarrantyEngineer;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Warranty Engineer", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(EPEmployee.acctCD)
                     , new Type[]{
                     typeof(EPEmployee.acctCD)
                     ,typeof(EPEmployee.acctName)
                     ,typeof(EPEmployee.classID)
                     }
                     , SubstituteKey = typeof(EPEmployee.acctCD)
                     , DescriptionField = typeof(EPEmployee.acctName))]
		public virtual string WarrantyEngineer
		{
			get
			{
				return this._WarrantyEngineer;
			}
			set
			{
				this._WarrantyEngineer = value;
			}
		}
		#endregion
		#region ClaimNbr
		public abstract class claimNbr : PX.Data.IBqlField
		{
		}
		protected string _ClaimNbr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Claim Nbr.")]
		public virtual string ClaimNbr
		{
			get
			{
				return this._ClaimNbr;
			}
			set
			{
				this._ClaimNbr = value;
			}
		}
		#endregion
		#region AssignedToDateTime
		public abstract class assignedToDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _AssignedToDateTime;
		[PXDBDate()]
		[PXUIField(DisplayName = "Assigned To Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? AssignedToDateTime
		{
			get
			{
				return this._AssignedToDateTime;
			}
			set
			{
				this._AssignedToDateTime = value;
			}
		}
		#endregion
		#region KM
		public abstract class kM : PX.Data.IBqlField
		{
		}
		protected string _KM;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Kilometer")]
		public virtual string KM
		{
			get
			{
				return this._KM;
			}
			set
			{
				this._KM = value;
			}
		}
		#endregion
		#region CreateDate
		public abstract class createDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _CreateDate;
		[PXDBCreatedDateTime]                     
        [PXUIField(DisplayName = "Create Date Time")]
		public virtual DateTime? CreateDate
		{
			get
			{
				return this._CreateDate;
			}
			set
			{
				this._CreateDate = value;
			}
		}
		#endregion
		#region Descrption
		public abstract class descrption : PX.Data.IBqlField
		{
		}
		protected string _Descrption;
		[PXDBString(-1, IsUnicode = true)]
		[PXUIField(DisplayName = "Descrption")]
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
		#region OldJobOrderID
		public abstract class oldJobOrderID : PX.Data.IBqlField
		{
		}
		protected string _OldJobOrderID;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Old Job Order ID.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<JobOrder.jobOrdrID, Where<JobOrder.branchID, Equal<Current<AccessInfo.branchID>>>>),
          new Type[] { typeof(JobOrder.jobOrdrID), typeof(JobOrder.customer) })]

        //[PXSelector(typeof(Search2<JobOrder.jobOrdrID, InnerJoin<ItemCustomers, On<ItemCustomers.customerID, Equal<Customer.bAccountID>>,
        //                                           InnerJoin<Items, On<Items.itemsID, Equal<ItemCustomers.itemsID>>>>,
        //                                           Where<Items.code, Equal<Current<RequestForm.vechile>>>>)
        //     , new Type[]{
        //               typeof(Customer.acctCD),
        //               typeof(Customer.acctName)
        //             }
        //     , DescriptionField = typeof(Customer.acctName)
        //     , SubstituteKey = typeof(Customer.acctCD))] 

        //[PXSelector(typeof(Search2<Customer.acctCD, InnerJoin<ItemCustomers, On<ItemCustomers.customerID, Equal<Customer.bAccountID>>,
        //                                            InnerJoin<Items, On<Items.itemsID, Equal<ItemCustomers.itemsID>>>>,
        //                                            Where<Items.code, Equal<Current<RequestForm.vechile>>>>)
        //      , new Type[]{
        //               typeof(Customer.acctCD),
        //               typeof(Customer.acctName)
        //             }
        //      , DescriptionField = typeof(Customer.acctName)
        //      , SubstituteKey = typeof(Customer.acctCD))] 
		public virtual string OldJobOrderID
		{
			get
			{
				return this._OldJobOrderID;
			}
			set
			{
				this._OldJobOrderID = value;
			}
		}
		#endregion
		#region Branchesss
		public abstract class branchesss : PX.Data.IBqlField
		{
		}
		protected string _Branchesss;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Search<SetUp.branchCD, Where<SetUp.autoNumbering, Equal<True>>>))]
		//[PXSelector(typeof(SetUp.branchCD))]
		[PXDefault()]
		public virtual string Branchesss
		{
			get
			{
				return this._Branchesss;
			}
			set
			{
				this._Branchesss = value;
			}
		}
		#endregion
		#region status
		public abstract class Status : PX.Data.IBqlField
		{
		}
		protected string _status;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Status")]
		[PXDefault(JobOrderStatus.Open)]
		public virtual string status
		{
			get
			{
				return this._status;
			}
			set
			{
				this._status = value;
			}
		}
		#endregion
		#region Complete
		public abstract class complete : PX.Data.IBqlField
		{
		}
		protected bool? _Complete;
		[PXDBBool()]
		[PXUIField(DisplayName = "Complete",IsReadOnly=true)]
		public virtual bool? Complete
		{
			get
			{
				return this._Complete;
			}
			set
			{
				this._Complete = value;
			}
		}
		#endregion
		#region ComeBack
		public abstract class comeBack : PX.Data.IBqlField
		{
		}
		protected bool? _ComeBack;
		[PXDBBool()]
        [PXUIField(DisplayName = "ComeBack")]
		public virtual bool? ComeBack
		{
			get
			{
				return this._ComeBack;
			}
			set
			{
				this._ComeBack = value;
			}
		}
		#endregion
		#region Price
		public abstract class price : PX.Data.IBqlField
		{
		}
		protected decimal? _Price;
		[PXDBDecimal(2)]
		[PXUIField(DisplayName = "Total Price",Enabled= false)]
		[PXDefault( "0.0")]
		public virtual decimal? Price
		{
			get
			{
				return this._Price;
			}
			set
			{
				this._Price = value;
			}
		}
		#endregion
		#region ItemBrand
		public abstract class itemBrand : PX.Data.IBqlField
		{
		}
		protected int? _ItemBrand;
		[PXDBInt()]
		[PXUIField(DisplayName = "ItemBrand")]
		public virtual int? ItemBrand
		{
			get
			{
				return this._ItemBrand;
			}
			set
			{
				this._ItemBrand = value;
			}
		}
		#endregion
		#region job
		public abstract class Job : PX.Data.IBqlField
		{
		}
		protected string _job;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "job")]
		public virtual string job
		{
			get
			{
				return this._job;
			}
			set
			{
				this._job = value;
			}
		}
		#endregion
		#region Notice
		public abstract class notice : PX.Data.IBqlField
		{
		}
		protected string _Notice;
        [PXDBString(-1, IsUnicode = true)]
		[PXUIField(DisplayName = "Notice")]
		public virtual string Notice
		{
			get
			{
				return this._Notice;
			}
			set
			{
				this._Notice = value;
			}
		}
		#endregion
        #region Customer
        public abstract class customer : PX.Data.IBqlField
        {
        }
        protected string _Customer;
        [PXDBString(50, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Customer ID")]
        [PXSelector(typeof(Search2<Customer.acctCD, InnerJoin<ItemCustomers, On<ItemCustomers.customerID, Equal<Customer.bAccountID>>>, Where<ItemCustomers.itemsID, Equal<Current<JobOrder.itemsID>>>>)
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
        #region StopReason
        public abstract class stopReason : PX.Data.IBqlField
        {
        }
        protected string _StopReason;
        [PXDBString(-1, IsUnicode = true)]
        [PXUIField(DisplayName = "Stop Details")]
        public virtual string StopReason
        {
            get
            {
                return this._StopReason;
            }
            set
            {
                this._StopReason = value;
            }
        }
        #endregion

        #region StopPurpose
        public abstract class stopPurpose : PX.Data.IBqlField
        {
        }
        protected string _StopPurpose;
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Stop Reason")]
        [PXStringList(new string[] { "Wait Spare Parts", "Wait Approval", "Wait Tech sup Reply", "Wait register delivery date", "Wait Customer" },
                      new string[] { "Wait Spare Parts", "Wait Approval", "Wait Tech sup Reply", "Wait register delivery date", "Wait Customer" })]
        public virtual string StopPurpose
        {
            get
            {
                return this._StopPurpose;
            }
            set
            {
                this._StopPurpose = value;
            }
        }
        #endregion

        #region StopLocation
        public abstract class stopLocation : PX.Data.IBqlField
        {
        }
        protected string _StopLocation;
        [PXDBString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Stop Location")]
        [PXStringList(new string[] { "Indoor", "Outdoor"},
                      new string[] { "Indoor", "Outdoor" })]
        public virtual string StopLocation
        {
            get
            {
                return this._StopLocation;
            }
            set
            {
                this._StopLocation = value;
            }
        }
        #endregion

        #region LastUpdateDateTime
        public abstract class lastUpdateDateTime : PX.Data.IBqlField
        {
        }
        protected DateTime? _LastUpdateDateTime;
        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "last Modified Date Time", Enabled = false)]
        public virtual DateTime? LastUpdateDateTime
        {
            get
            {
                return this._LastUpdateDateTime;
            }
            set
            {
                this._LastUpdateDateTime = value;
            }
        }
        #endregion
        #region LastUpdateByID

        public abstract class lastUpdateByID : PX.Data.IBqlField
        {
        }
        protected Guid? _LastUpdateByID;
        [PXDBLastModifiedByID()]
        [PXUIField(DisplayName = "Last Update User", Enabled = false)]
        public virtual Guid? LastUpdateByID
        {
            get
            {
                return this._LastUpdateByID;
            }
            set
            {
                this._LastUpdateByID = value;
            }
        }
        #endregion

        #region StartByUserName
        public abstract class startByUserName : PX.Data.IBqlField
        {
        }
        protected string _StartByUserName;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Started BY", Enabled = false)]
        public virtual string StartByUserName
        {
            get
            {
                return this._StartByUserName;
            }
            set
            {
                this._StartByUserName = value;
            }
        }
        #endregion
        #region FinishedByUserName
        public abstract class finishedByUserName : PX.Data.IBqlField
        {
        }
        protected string _FinishedByUserName;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Finished By", Enabled = false)]
        public virtual string FinishedByUserName
        {
            get
            {
                return this._FinishedByUserName;
            }
            set
            {
                this._FinishedByUserName = value;
            }
        }
        #endregion
        #region StartDateTime
        public abstract class startDateTime : PX.Data.IBqlField
        {
        }
        protected DateTime? _StartDateTime;
        [PXDBDateAndTime()]
        [PXUIField(DisplayName = "Start DateTime", Enabled = false)]
        public virtual DateTime? StartDateTime
        {
            get
            {
                return this._StartDateTime;
            }
            set
            {
                this._StartDateTime = value;
            }
        }
        #endregion
        #region FinishedDateTime
        public abstract class finishedDateTime : PX.Data.IBqlField
        {
        }
        protected DateTime? _FinishedDateTime;
        [PXDBDateAndTime()]
        [PXUIField(DisplayName = "Finished DateTime", Enabled = false)]
        public virtual DateTime? FinishedDateTime
        {
            get
            {
                return this._FinishedDateTime;
            }
            set
            {
                this._FinishedDateTime = value;
            }
        }
        #endregion
        #region Duration
        public abstract class duration : PX.Data.IBqlField
        {
        }
        protected int? _Duration;
        [PXDBInt()]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Duration", Enabled = false)]

        public virtual int? Duration
        {
            get
            {
                return this._Duration;
            }
            set
            {
                this._Duration = value;
            }
        }
        #endregion

        #region Inspection Number
        [PXDBString()]
        [PXUIField(DisplayName = "Inspection Number", Enabled = false)]
        [PXSelector(
            typeof(Search<InspectionFormMaster.inspectionFormNbr>))]
        public virtual string InspectionNbr { get; set; }
        public abstract class inspectionNbr : PX.Data.BQL.BqlString.Field<inspectionNbr> { }
        #endregion

        #region Virtual Fields Unbounded DAC Fields


        #region LicensePlate
       
        [PXString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "License Plate")]
       
        public virtual string LicensePlate { get; set; }
        public abstract class licensePlate : PX.Data.BQL.BqlString.Field<licensePlate> { }
        #endregion

        #region BrandName
        public abstract class brandName : PX.Data.IBqlField
        {
        }
        [PXInt()]
        [PXUIField(DisplayName = "Brand Name")]
        [PXSelector(typeof(Brand.brandID)
                    , DescriptionField = typeof(Brand.Code)
                    , SubstituteKey = typeof(Brand.name))]
        public virtual int? BrandName { get; set; }
        #endregion

        #region ModelName
        public abstract class modelName : PX.Data.IBqlField
        {
        }
        [PXInt()]
        [PXUIField(DisplayName = "Model Name")]
        [PXSelector(typeof(Model.modelID)
           , DescriptionField = typeof(Model.code)
           , SubstituteKey = typeof(Model.name))]
        public virtual int? ModelName { get; set; }
        #endregion

        #region Email
        public abstract class email : PX.Data.IBqlField
        {
        }
        [PXString(255)]
        [PXUIField(DisplayName = "Email",IsReadOnly = true,Enabled = false)]
        //[PXDefault]
        public virtual string Email { get; set; }
        #endregion

        #region Address
        public abstract class address : PX.Data.IBqlField
        {
        }
        [PXString(255)]
        [PXUIField(DisplayName = "Address", IsReadOnly = true, Enabled = false)]
        //[PXDefault]
        public virtual string Address { get; set; }
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
        #region Campaign
        public abstract class campaign : PX.Data.IBqlField
        {
        }
        protected string _Campaign;
        [PXDBString(30, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Campaign",Required =true)]
        [PXStringList(new string[] { "Sms", "Rec", "Crm", "Social media", "Non" },
                      new string[] { "Sms", "Rec", "Crm", "Social media", "Non" })]

       // [PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), "Required", true)]
        public virtual string Campaign
        {
            get
            {
                return this._Campaign;
            }
            set
            {
                this._Campaign = value;
            }
        }
        #endregion
        #endregion


        #region UsrMaintenanceStatus
        [PXDBString(255)]
        [PXUIField(DisplayName = "Maintenance Status",Required =true)]
        [PXDefault]
        [PXStringList(
       new string[] { "S of 1000 K.M", "S of 5000 K.M", "S of 10000 K.M", "S of 20000 K.M", "S of 30000 K.M", "S of 40000 K.M", "S of 50000 K.M", "S of 60000 K.M", "S of 70000 K.M", "S of 80000 K.M", "S of 90000 K.M", "S of 100000 K.M", "S of 110000 K.M", "S of 120000 K.M", "S of 130000 K.M", "S of 140000 K.M", "S of 150000 K.M", "S of 160000 K.M", "S of 170000 K.M", "S of 180000 K.M", "S of 190000 K.M", "S of 200000 K.M", "تغيير زيت وفلتر", "الكشف علي الكهرباء", "فحص مشكلة فنية", "الكشف علي العفشة", "فحص سمكرة ودهان", "تقرير فحص شامل للسيارة", "فحص عيب ظهر بعد الاصلاح او الصيانة", "حجز عرض للصيانة او الاصلاح", "ضبط زوايا واتزان للسيارة", "ترصيص عجل وفحص الكاوتش", "فحص عيب بالخامات الداخلية او الخارجية", "تغيير زجاج للسيارة", "تركيب قطعة خاصة بالضمان" },
       new string[] { "S of 1000 K.M", "S of 5000 K.M", "S of 10000 K.M", "S of 20000 K.M", "S of 30000 K.M", "S of 40000 K.M", "S of 50000 K.M", "S of 60000 K.M", "S of 70000 K.M", "S of 80000 K.M", "S of 90000 K.M", "S of 100000 K.M", "S of 110000 K.M", "S of 120000 K.M", "S of 130000 K.M", "S of 140000 K.M", "S of 150000 K.M", "S of 160000 K.M", "S of 170000 K.M", "S of 180000 K.M", "S of 190000 K.M", "S of 200000 K.M", "تغيير زيت وفلتر", "الكشف علي الكهرباء", "فحص مشكلة فنية", "الكشف علي العفشة", "فحص سمكرة ودهان", "تقرير فحص شامل للسيارة", "فحص عيب ظهر بعد الاصلاح او الصيانة", "حجز عرض للصيانة او الاصلاح", "ضبط زوايا واتزان للسيارة", "ترصيص عجل وفحص الكاوتش", "فحص عيب بالخامات الداخلية او الخارجية", "تغيير زجاج للسيارة", "تركيب قطعة خاصة بالضمان" }
         )]
        public virtual string UsrMaintenanceStatus { get; set; }
        public abstract class usrMaintenanceStatus : PX.Data.BQL.BqlString.Field<usrMaintenanceStatus> { }
        #endregion
    }
}
