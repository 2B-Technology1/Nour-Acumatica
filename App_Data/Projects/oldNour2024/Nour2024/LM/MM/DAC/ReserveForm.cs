using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.EP;
using PX.Data.BQL;
using NourSc202007071;

namespace MyMaintaince
{
    [System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(ReserveFormMaint))]
    public class ReserveForm : PX.Data.IBqlTable
    {
        public abstract class statusConatant : PX.Data.IBqlField
        {
            //Constant declaration
            public const string open = "Open";
            public const string Closed = "Closed";
            public const string postponed = "Postponed";
            public const string Cancelled = "cancelled";
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                : base
                    (
                        new string[] { open, Closed, postponed, Cancelled },
                        new string[] { open, Closed, postponed, Cancelled }
                    )
                {; }
            }
            public class Open : BqlString.Constant<Open>
            {
                public Open() : base(open) {; }
            }
            public class closed : BqlString.Constant<closed>
            {
                public closed() : base(Closed) {; }
            }

            public class Postponed : BqlString.Constant<Postponed>
            {
                public Postponed() : base(postponed) {; }
            }
            public class cancelled : BqlString.Constant<cancelled>
            {
                public cancelled() : base(Cancelled) {; }
            }
        }
        #region statusDocType
        public abstract class Doctype : PX.Data.IBqlField
        {
            //Constant declaration 

            public const string ExistCust = "Exist Customer";
            public const string NewCust = "New Customer";
            public const string ComebackCust = "Comeback";
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                        new string[] { ExistCust, NewCust, ComebackCust },
                        new string[] { ExistCust, NewCust, ComebackCust })
                {; }
            }
            public class existCust : BqlString.Constant<existCust>
            {
                public existCust() : base(ExistCust) {; }
            }
            public class newCust : BqlString.Constant<newCust>
            {
                public newCust() : base(NewCust) {; }
            }
            public class comebackCust : BqlString.Constant<comebackCust>
            {
                public comebackCust() : base(ComebackCust) {; }
            }
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }


        protected string _RefNbr;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "RefNbr")]
        [PXDefault("<NEW>")]
        [PXSelector(typeof(Search<ReserveForm.refNbr>))]
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
        [PXDefault("")]
        [PXUIField(DisplayName = "Customer Sort")]
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
        [PXUIField(DisplayName = "Requst Date")]
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
        [PXSelector(typeof(Search2<Customer.acctCD, InnerJoin<ItemCustomers, On<ItemCustomers.customerID, Equal<Customer.bAccountID>>,
                                                    InnerJoin<Items, On<Items.itemsID, Equal<ItemCustomers.itemsID>>>>,
                                                    Where<Items.code, Equal<Current<ReserveForm.vechile>>>>)
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
        #region Name
        public abstract class name : PX.Data.IBqlField
        {
        }
        protected string _Name;
        [PXDBString(200, IsUnicode = true)]
        [PXUIField(DisplayName = "Name", Required = true)]
        [PXDefault]
        //[PXDefault(typeof(Search<PX.Objects.AR.Customer.acctName,Where<PX.Objects.AR.Customer.acctCD,Equal<Current<ReserveForm.customer>>>>))]
        public virtual string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
            }
        }
        #endregion
        #region Phone
        public abstract class phone : PX.Data.IBqlField
        {
        }
        protected string _Phone;
        [PXDBString(11, IsUnicode = true)]
        [PXUIField(DisplayName = "Phone", Required = true)]
        [PXDefault]
        //[PXDefault("")]
        //[PXDefault(typeof(Search<PX.Objects.AR.Customer.acctCD, Where<PX.Objects.AR.Customer.acctCD, Equal<Current<ReserveForm.customer>>>>))]

        public virtual string Phone
        {
            get
            {
                return this._Phone;
            }
            set
            {
                this._Phone = value;
            }
        }
        #endregion

        #region Vechile
        public abstract class vechile : PX.Data.IBqlField
        {
        }
        protected string _Vechile;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Vechile")]
        [PXSelector(typeof(Search<Items.code>)
                , new Type[] {
                        typeof(Items.code),
                        typeof(Items.name),
                        typeof(Items.brandID),
                        typeof(Items.modelID),
                    }
                , DescriptionField = typeof(Items.brandID)
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

        #region Descrption
        public abstract class descrption : PX.Data.IBqlField
        {
        }
        protected string _Descrption;
        [PXDBString(2147483647, IsUnicode = true)]
        [PXUIField(DisplayName = "Descrption")]
        //[PXDefault("")]
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
        #region Descrption2
        //public abstract class descrption2 : PX.Data.IBqlField
        //{
        //}
        //protected string _Descrption2;
        //[PXDBString(200, IsUnicode = true)]
        //[PXUIField(DisplayName = "Maintenance Status", Required = true)]
        //[PXDefault]
        //[PXStringList(
        //    new string[] { "S of 1000 K.M", "S of 5000 K.M", "S of 10000 K.M", "S of 20000 K.M", "S of 30000 K.M", "S of 40000 K.M", "S of 50000 K.M", "S of 60000 K.M", "S of 70000 K.M", "S of 80000 K.M", "S of 90000 K.M", "S of 100000 K.M", "S of 110000 K.M", "S of 120000 K.M", "S of 130000 K.M", "S of 140000 K.M", "S of 150000 K.M", "S of 160000 K.M", "S of 170000 K.M", "S of 180000 K.M", "S of 190000 K.M", "S of 200000 K.M", "تغيير زيت وفلتر", "الكشف علي الكهرباء", "فحص مشكلة فنية", "الكشف علي العفشة", "فحص سمكرة ودهان", "تقرير فحص شامل للسيارة", "فحص عيب ظهر بعد الاصلاح او الصيانة", "حجز عرض للصيانة او الاصلاح", "ضبط زوايا واتزان للسيارة", "ترصيص عجل وفحص الكاوتش", "فحص عيب بالخامات الداخلية او الخارجية", "تغيير زجاج للسيارة", "تركيب قطعة خاصة بالضمان" },
        //    new string[] { "S of 1000 K.M", "S of 5000 K.M", "S of 10000 K.M", "S of 20000 K.M", "S of 30000 K.M", "S of 40000 K.M", "S of 50000 K.M", "S of 60000 K.M", "S of 70000 K.M", "S of 80000 K.M", "S of 90000 K.M", "S of 100000 K.M", "S of 110000 K.M", "S of 120000 K.M", "S of 130000 K.M", "S of 140000 K.M", "S of 150000 K.M", "S of 160000 K.M", "S of 170000 K.M", "S of 180000 K.M", "S of 190000 K.M", "S of 200000 K.M", "تغيير زيت وفلتر", "الكشف علي الكهرباء", "فحص مشكلة فنية", "الكشف علي العفشة", "فحص سمكرة ودهان", "تقرير فحص شامل للسيارة", "فحص عيب ظهر بعد الاصلاح او الصيانة", "حجز عرض للصيانة او الاصلاح", "ضبط زوايا واتزان للسيارة", "ترصيص عجل وفحص الكاوتش", "فحص عيب بالخامات الداخلية او الخارجية", "تغيير زجاج للسيارة", "تركيب قطعة خاصة بالضمان" }
        //    )]
        //public virtual string Descrption2
        //{
        //    get
        //    {
        //        return this._Descrption2;
        //    }
        //    set
        //    {
        //        this._Descrption2 = value;
        //    }
        //}
        //[PXDBString(200, IsUnicode = true, IsFixed = true)]
        //[PXUIField(DisplayName = "Maintenance Status", Required = true)]
        //[PXDefault]
        //[PXStringList(
        //    new string[] { "S of 1000 K.M", "S of 5000 K.M", "S of 10000 K.M", "S of 20000 K.M", "S of 30000 K.M", "S of 40000 K.M", "S of 50000 K.M", "S of 60000 K.M", "S of 70000 K.M", "S of 80000 K.M", "S of 90000 K.M", "S of 100000 K.M", "S of 110000 K.M", "S of 120000 K.M", "S of 130000 K.M", "S of 140000 K.M", "S of 150000 K.M", "S of 160000 K.M", "S of 170000 K.M", "S of 180000 K.M", "S of 190000 K.M", "S of 200000 K.M", "تغيير زيت وفلتر", "الكشف علي الكهرباء", "فحص مشكلة فنية", "الكشف علي العفشة", "فحص سمكرة ودهان", "تقرير فحص شامل للسيارة", "فحص عيب ظهر بعد الاصلاح او الصيانة", "حجز عرض للصيانة او الاصلاح", "ضبط زوايا واتزان للسيارة", "ترصيص عجل وفحص الكاوتش", "فحص عيب بالخامات الداخلية او الخارجية", "تغيير زجاج للسيارة", "تركيب قطعة خاصة بالضمان" },
        //    new string[] { "S of 1000 K.M", "S of 5000 K.M", "S of 10000 K.M", "S of 20000 K.M", "S of 30000 K.M", "S of 40000 K.M", "S of 50000 K.M", "S of 60000 K.M", "S of 70000 K.M", "S of 80000 K.M", "S of 90000 K.M", "S of 100000 K.M", "S of 110000 K.M", "S of 120000 K.M", "S of 130000 K.M", "S of 140000 K.M", "S of 150000 K.M", "S of 160000 K.M", "S of 170000 K.M", "S of 180000 K.M", "S of 190000 K.M", "S of 200000 K.M", "تغيير زيت وفلتر", "الكشف علي الكهرباء", "فحص مشكلة فنية", "الكشف علي العفشة", "فحص سمكرة ودهان", "تقرير فحص شامل للسيارة", "فحص عيب ظهر بعد الاصلاح او الصيانة", "حجز عرض للصيانة او الاصلاح", "ضبط زوايا واتزان للسيارة", "ترصيص عجل وفحص الكاوتش", "فحص عيب بالخامات الداخلية او الخارجية", "تغيير زجاج للسيارة", "تركيب قطعة خاصة بالضمان" }
        //    )]
        //public virtual string MaintenanceStatus { get; set; }
        //public abstract class maintenanceStatus : BqlString.Field<maintenanceStatus> { }

        [PXDBString(200, IsUnicode = true)]
        [PXUIField(DisplayName = "Maintenance Status", Required = true)]
        [PXDefault]
        [PXStringList(
          new string[] { "S of 1000 K.M", "S of 5000 K.M", "S of 10000 K.M", "S of 20000 K.M", "S of 30000 K.M", "S of 40000 K.M", "S of 50000 K.M", "S of 60000 K.M", "S of 70000 K.M", "S of 80000 K.M", "S of 90000 K.M", "S of 100000 K.M", "S of 110000 K.M", "S of 120000 K.M", "S of 130000 K.M", "S of 140000 K.M", "S of 150000 K.M", "S of 160000 K.M", "S of 170000 K.M", "S of 180000 K.M", "S of 190000 K.M", "S of 200000 K.M", "تغيير زيت وفلتر", "الكشف علي الكهرباء", "فحص مشكلة فنية", "الكشف علي العفشة", "فحص سمكرة ودهان", "تقرير فحص شامل للسيارة", "فحص عيب ظهر بعد الاصلاح او الصيانة", "حجز عرض للصيانة او الاصلاح", "ضبط زوايا واتزان للسيارة", "ترصيص عجل وفحص الكاوتش", "فحص عيب بالخامات الداخلية او الخارجية", "تغيير زجاج للسيارة", "تركيب قطعة خاصة بالضمان" },
          new string[] { "S of 1000 K.M", "S of 5000 K.M", "S of 10000 K.M", "S of 20000 K.M", "S of 30000 K.M", "S of 40000 K.M", "S of 50000 K.M", "S of 60000 K.M", "S of 70000 K.M", "S of 80000 K.M", "S of 90000 K.M", "S of 100000 K.M", "S of 110000 K.M", "S of 120000 K.M", "S of 130000 K.M", "S of 140000 K.M", "S of 150000 K.M", "S of 160000 K.M", "S of 170000 K.M", "S of 180000 K.M", "S of 190000 K.M", "S of 200000 K.M", "تغيير زيت وفلتر", "الكشف علي الكهرباء", "فحص مشكلة فنية", "الكشف علي العفشة", "فحص سمكرة ودهان", "تقرير فحص شامل للسيارة", "فحص عيب ظهر بعد الاصلاح او الصيانة", "حجز عرض للصيانة او الاصلاح", "ضبط زوايا واتزان للسيارة", "ترصيص عجل وفحص الكاوتش", "فحص عيب بالخامات الداخلية او الخارجية", "تغيير زجاج للسيارة", "تركيب قطعة خاصة بالضمان" }
          )]
        public virtual string Descrption2 { get; set; }
        public abstract class descrption2 : BqlString.Field<descrption2> { }
        #endregion



        #region Governorate
        public abstract class usrGovernorate : PX.Data.IBqlField
        {
        }
        protected string _UsrGovernorate;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Governorate", Required = true)]
        [PXDefault]
        [PXStringList(
            new string[]
            {
                "Cairo", "Giza", "Alex", "Luxor", "Aswan", "Gharbia", "Sohag", "Monufia", "Sharqia", "BeniSuef",
                "Dakahlia", "Kafr El Sheikh", "Qena", "Ismailia", "Port Said", "Suez", "Beheira", "Fayoum",
                "Red Sea", "Al Wadi Al Jadid", "North Sinai", "South Sinai", "Matrouh", "Damietta", "Asyut",
                "Minya", "Qalyubia"
            }, new string[]
            {
                "Cairo", "Giza", "Alex", "Luxor", "Aswan", "Gharbia", "Sohag", "Monufia", "Sharqia", "BeniSuef",
                "Dakahlia", "Kafr El Sheikh", "Qena", "Ismailia", "Port Said", "Suez", "Beheira", "Fayoum",
                "Red Sea", "Al Wadi Al Jadid", "North Sinai", "South Sinai", "Matrouh", "Damietta", "Asyut",
                "Minya", "Qalyubia"
            }
            )]

        //[PXDefault("")]
        public virtual string UsrGovernorate
        {
            get
            {
                return this._UsrGovernorate;
            }
            set
            {
                this._UsrGovernorate = value;
            }
        }
        #endregion


        #region BranchCD
        public abstract class branchCD : PX.Data.IBqlField
        {
        }
        protected string _BranchCD;
        [PXDBString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Branch Name", Required = true)]
        [PXSelector(typeof(Search<SetUp.branchCD, Where<SetUp.autoNumbering, Equal<True>>>))]
        //[PXSelector(typeof(SetUp.branchCD))]
        [PXDefault]
        public virtual string BranchCD
        {
            get
            {
                return this._BranchCD;
            }
            set
            {
                this._BranchCD = value;
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
        #region CreatedDateTime
        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created Date")]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : IBqlField { }
        #endregion

        #region LastModefidByID

        public abstract class lastModefidByID : PX.Data.IBqlField
        {
        }
        protected Guid? _LastModefidByID;
        [PXDBLastModifiedByID()]
        [PXUIField(DisplayName = "Last Modefid By")]
        public virtual Guid? LastModefidByID
        {
            get
            {
                return this._LastModefidByID;
            }
            set
            {
                this._LastModefidByID = value;
            }
        }
        #endregion

        #region LastModefidDateTime
        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modefid Date")]
        public virtual DateTime? LastModefidDateTime { get; set; }
        public abstract class lastModefidDateTime : IBqlField { }
        #endregion
        #region TimeCD
        public abstract class timeCD : PX.Data.IBqlField
        {
        }
        protected string _TimeCD;
        [PXDBString(5, IsUnicode = true)]
        [PXUIField(DisplayName = "Requst Time", Required = true)]
        [PXDefault]
        [PXStringList(new string[] { "09:00","09:15","09:30","09:45","10:00","10:15","10:30","10:45","11:00","11:15","11:30","11:45","12:00","12:15","12:30",
                                      "12:45","13:00","13:15","13:30","13:45","14:00","14:15","14:30","14:45","15:00","15:15","15:30","15:45","16:00","16:15","16:30"},
                      new string[] { "09:00","09:15","09:30","09:45","10:00","10:15","10:30","10:45","11:00","11:15","11:30","11:45","12:00","12:15","12:30",
                                      "12:45","13:00","13:15","13:30","13:45","14:00","14:15","14:30","14:45","15:00","15:15","15:30","15:45","16:00","16:15","16:30" })]
        public virtual string TimeCD
        {
            get
            {
                return this._TimeCD;
            }
            set
            {
                this._TimeCD = value;
            }
        }
        #endregion
        #region OldMeter
        public abstract class oldMeter : PX.Data.IBqlField
        {
        }
        protected string _OldMeter;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Old Milage / KM")]
        public virtual string OldMeter
        {
            get
            {
                return this._OldMeter;
            }
            set
            {
                this._OldMeter = value;
            }
        }
        #endregion

        #region AssignedTo
        public abstract class assignedTo : PX.Data.IBqlField
        {
        }
        protected string _AssignedTo;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Assigned To", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        [PXDefault]
        [PXSelector(typeof(EPEmployee.acctCD)
                     , new Type[]{
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

        #region BrandName
        public abstract class brandName : PX.Data.IBqlField
        {
        }
        [PXDBInt()]
        [PXUIField(DisplayName = "Brand Name", Required = true)]
        [PXDefault]
        [PXSelector(typeof(Brand.brandID)
                    , DescriptionField = typeof(Brand.Code)
                    , SubstituteKey = typeof(Brand.name))]
        public virtual int? BrandName { get; set; }
        #endregion

        #region ModelName
        public abstract class modelName : PX.Data.IBqlField
        {
        }
        [PXDBInt()]
        [PXUIField(DisplayName = "Model Name", Required = true)]
        [PXDefault]
        [PXSelector(typeof(Model.modelID)
           , DescriptionField = typeof(Model.code)
           , SubstituteKey = typeof(Model.name))]
        public virtual int? ModelName { get; set; }
        #endregion

        #region Virtual Fields Unbounded DAC Fields


        #region LicensePlate
        public abstract class licensePlate : PX.Data.IBqlField
        {
        }
        [PXString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "License Plate")]
        public virtual string LicensePlate { get; set; }
        #endregion
        #endregion

    }
}
