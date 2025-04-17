﻿namespace Maintenance.CO
{
	using System;
	using PX.Data;
    using PX.Objects.AR;
    using PX.SM;
	
	[System.SerializableAttribute()]
	public class CompensationDocument : PX.Data.IBqlTable
	{   
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected string _RefNbr;
        [PXDBString(100, IsUnicode = true, IsKey = true,InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "RefNbr")]
        [PXSelector(typeof(CompensationDocument.refNbr))]
        [COAutoNumbering(typeof(COSetUp.autoNumbering), typeof(COSetUp.lastRefNbr))]
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
        #region Released
        public abstract class released : PX.Data.IBqlField
        {
        }
        protected bool? _Released;
        [PXDBBool]
        [PXUIField(DisplayName = "Released", Enabled = false)]
        public virtual bool? Released
        {
            get
            {
                return this._Released;
            }
            set
            {
                this._Released = value;
            }
        }
        #endregion
        #region InsurranceCertificateRefNbr
		public abstract class insurranceCertificateRefNbr : PX.Data.IBqlField
		{
		}
		protected string _InsurranceCertificateRefNbr;
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "InsurranceCertificateRefNbr")]
		public virtual string InsurranceCertificateRefNbr
		{
			get
			{
				return this._InsurranceCertificateRefNbr;
			}
			set
			{
				this._InsurranceCertificateRefNbr = value;
			}
		}
		#endregion
        #region Status
        public abstract class status : PX.Data.IBqlField
        {
        }
        protected string _Status;
        [PXDBString(50, IsUnicode = true)]
        [PXDefault(CompensationStatus.NewC)]
        [PXUIField(DisplayName = "Status")]
        [PXStringList(
             new string[] { CompensationStatus.NewC, CompensationStatus.Printed, CompensationStatus.Received, CompensationStatus.Sended,CompensationStatus.RecievedMaintInvoice,CompensationStatus.RecievedMaintInvoiceToCompany,CompensationStatus.CheckRecieved,CompensationStatus.CheckDelivered,CompensationStatus.Closed,CompensationStatus.Released},
             new string[] { CompensationStatus.NewC, CompensationStatus.Printed, CompensationStatus.Received, CompensationStatus.Sended,CompensationStatus.RecievedMaintInvoice,CompensationStatus.RecievedMaintInvoiceToCompany,CompensationStatus.CheckRecieved,CompensationStatus.CheckDelivered,CompensationStatus.Closed,CompensationStatus.Released}
         )]
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
		#region CustomerID
		public abstract class customerID : PX.Data.IBqlField
		{
		}
		protected int? _CustomerID;
		[PXDBInt()]
		[PXUIField(DisplayName = "CustomerID")]
        [PXSelector(typeof(Customer.bAccountID)
                    ,new Type[]{
                       typeof(Customer.acctCD),
                       typeof(Customer.acctName)
                    }
                    , SubstituteKey = typeof(Customer.acctCD)
                    , DescriptionField = typeof(Customer.acctName))]
		public virtual int? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region PlateNbr
		public abstract class plateNbr : PX.Data.IBqlField
		{
		}
		protected string _PlateNbr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "PlateNbr")]
		public virtual string PlateNbr
		{
			get
			{
				return this._PlateNbr;
			}
			set
			{
				this._PlateNbr = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "StartDate")]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "EndDate")]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region PaymentRef
		public abstract class paymentRef : PX.Data.IBqlField
		{
		}
		protected string _PaymentRef;
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "PaymentRef")]
        [PXSelector(typeof(Search<ARPayment.refNbr, Where<ARPayment.customerID, Equal<Current<CompensationDocument.customerID>>>>)
                    ,new Type[]{
                        typeof(ARPayment.docType),
                        typeof(ARPayment.refNbr),
                        typeof(ARPayment.customerID),
                        typeof(ARPayment.status),
                        typeof(ARPayment.curyOrigDocAmt),
                        typeof(ARPayment.curyUnappliedBal)
                    }
                    )]
		public virtual string PaymentRef
		{
			get
			{
				return this._PaymentRef;
			}
			set
			{
				this._PaymentRef = value;
			}
		}
		#endregion
		#region Cost
		public abstract class cost : PX.Data.IBqlField
		{
		}
		protected float? _Cost;
		[PXDBFloat(4)]
		[PXUIField(DisplayName = "Cost")]
		public virtual float? Cost
		{
			get
			{
				return this._Cost;
			}
			set
			{
				this._Cost = value;
			}
		}
		#endregion
		#region AccidentDescr
		public abstract class accidentDescr : PX.Data.IBqlField
		{
		}
		protected string _AccidentDescr;
		[PXDBString(500, IsUnicode = true)]
		[PXUIField(DisplayName = "AccidentDescr")]
		public virtual string AccidentDescr
		{
			get
			{
				return this._AccidentDescr;
			}
			set
			{
				this._AccidentDescr = value;
			}
		}
		#endregion
		#region AccidentDate
		public abstract class accidentDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _AccidentDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "AccidentDate")]
		public virtual DateTime? AccidentDate
		{
			get
			{
				return this._AccidentDate;
			}
			set
			{
				this._AccidentDate = value;
			}
		}
		#endregion
		#region AccidentNotifyDate
		public abstract class accidentNotifyDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _AccidentNotifyDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "AccidentNotifyDate")]
		public virtual DateTime? AccidentNotifyDate
		{
			get
			{
				return this._AccidentNotifyDate;
			}
			set
			{
				this._AccidentNotifyDate = value;
			}
		}
		#endregion
		#region PoliceCaseNbr
		public abstract class policeCaseNbr : PX.Data.IBqlField
		{
		}
		protected string _PoliceCaseNbr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "PoliceCaseNbr")]
		public virtual string PoliceCaseNbr
		{
			get
			{
				return this._PoliceCaseNbr;
			}
			set
			{
				this._PoliceCaseNbr = value;
			}
		}
		#endregion
		#region PoliceCaseDate
		public abstract class policeCaseDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PoliceCaseDate;
		[PXDBDate]
		[PXUIField(DisplayName = "PoliceCaseDate")]
        public virtual DateTime? PoliceCaseDate
		{
			get
			{
				return this._PoliceCaseDate;
			}
			set
			{
				this._PoliceCaseDate = value;
			}
		}
		#endregion
		#region PlanDate
		public abstract class planDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _PlanDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "PlanDate")]
		public virtual DateTime? PlanDate
		{
			get
			{
				return this._PlanDate;
			}
			set
			{
				this._PlanDate = value;
			}
		}
		#endregion
		#region DriverName
		public abstract class driverName : PX.Data.IBqlField
		{
		}
		protected string _DriverName;
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "DriverName")]
		public virtual string DriverName
		{
			get
			{
				return this._DriverName;
			}
			set
			{
				this._DriverName = value;
			}
		}
		#endregion
		#region DriverLicenceNbr
		public abstract class driverLicenceNbr : PX.Data.IBqlField
		{
		}
		protected string _DriverLicenceNbr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "DriverLicenceNbr")]
		public virtual string DriverLicenceNbr
		{
			get
			{
				return this._DriverLicenceNbr;
			}
			set
			{
				this._DriverLicenceNbr = value;
			}
		}
		#endregion
		#region MaintenanceCenter
		public abstract class maintenanceCenter : PX.Data.IBqlField
		{
		}
		protected int? _MaintenanceCenter;
		[PXDBInt()]
		[PXUIField(DisplayName = "MaintenanceCenter")]
        [PXSelector(typeof(CompensationCenters.centerID)
                   ,new Type[]{
                     typeof(CompensationCenters.centerCD),
                     typeof(CompensationCenters.descr)
                   }
                   , SubstituteKey = typeof(CompensationCenters.centerCD)
                   , DescriptionField = typeof(CompensationCenters.descr))]
		public virtual int? MaintenanceCenter
		{
			get
			{
				return this._MaintenanceCenter;
			}
			set
			{
				this._MaintenanceCenter = value;
			}
		}
		#endregion
		#region MaintenanceInvoiceNbr
		public abstract class maintenanceInvoiceNbr : PX.Data.IBqlField
		{
		}
		protected string _MaintenanceInvoiceNbr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "MaintenanceInvoiceNbr")]
		public virtual string MaintenanceInvoiceNbr
		{
			get
			{
				return this._MaintenanceInvoiceNbr;
			}
			set
			{
				this._MaintenanceInvoiceNbr = value;
			}
		}
		#endregion
		#region MaintenanceInvoiceDate
		public abstract class maintenanceInvoiceDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _MaintenanceInvoiceDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "MaintenanceInvoiceDate")]
		public virtual DateTime? MaintenanceInvoiceDate
		{
			get
			{
				return this._MaintenanceInvoiceDate;
			}
			set
			{
				this._MaintenanceInvoiceDate = value;
			}
		}
		#endregion
		#region CheckReceiveDate
		public abstract class checkReceiveDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _CheckReceiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Check Receive Date")]
		public virtual DateTime? CheckReceiveDate
		{
			get
			{
				return this._CheckReceiveDate;
			}
			set
			{
				this._CheckReceiveDate = value;
			}
		}
		#endregion
		#region CheckNbr
		public abstract class checkNbr : PX.Data.IBqlField
		{
		}
		protected string _CheckNbr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "CheckNbr")]
		public virtual string CheckNbr
		{
			get
			{
				return this._CheckNbr;
			}
			set
			{
				this._CheckNbr = value;
			}
		}
		#endregion
		#region CheckAmt
		public abstract class checkAmt : PX.Data.IBqlField
		{
		}
		protected float? _CheckAmt;
		[PXDBFloat(2)]
		[PXUIField(DisplayName = "CheckAmt")]
		public virtual float? CheckAmt
		{
			get
			{
				return this._CheckAmt;
			}
			set
			{
				this._CheckAmt = value;
			}
		}
		#endregion
		#region CustomerDeliveryPlace
		public abstract class customerDeliveryPlace : PX.Data.IBqlField
		{
		}
		protected string _CustomerDeliveryPlace;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "CustomerDeliveryPlace")]
		public virtual string CustomerDeliveryPlace
		{
			get
			{
				return this._CustomerDeliveryPlace;
			}
			set
			{
				this._CustomerDeliveryPlace = value;
			}
		}
		#endregion
		#region BankName
		public abstract class bankName : PX.Data.IBqlField
		{
		}
		protected string _BankName;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "BankName")]
		public virtual string BankName
		{
			get
			{
				return this._BankName;
			}
			set
			{
				this._BankName = value;
			}
		}
		#endregion
		#region CustomerCheckDelivered
		public abstract class customerCheckDelivered : PX.Data.IBqlField
		{
		}
		protected bool? _CustomerCheckDelivered;
		[PXDBBool()]
		[PXUIField(DisplayName = "CustomerCheckDelivered")]
		public virtual bool? CustomerCheckDelivered
		{
			get
			{
				return this._CustomerCheckDelivered;
			}
			set
			{
				this._CustomerCheckDelivered = value;
			}
		}
		#endregion
		#region CustomerCheckDeliveredDate
		public abstract class customerCheckDeliveredDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _CustomerCheckDeliveredDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "CustomerCheckDeliveredDate")]
		public virtual DateTime? CustomerCheckDeliveredDate
		{
			get
			{
				return this._CustomerCheckDeliveredDate;
			}
			set
			{
				this._CustomerCheckDeliveredDate = value;
			}
		}
		#endregion

        #region PrintUser
        public abstract class printUser : PX.Data.IBqlField
        {
        }
        protected Guid? _PrintUser;
        [PXDBGuid(false)]
        [PXUIField(DisplayName = "Print User", Enabled = false)]
        [PXSelector(typeof(Users.pKID)
                    , SubstituteKey = typeof(Users.username))]
        public virtual Guid? PrintUser
        {
            get
            {
                return this._PrintUser;
            }
            set
            {
                this._PrintUser = value;
            }
        }
        #endregion
        #region PrintDate
        public abstract class printDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _PrintDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Print Date", Enabled = false)]
        public virtual DateTime? PrintDate
        {
            get
            {
                return this._PrintDate;
            }
            set
            {
                this._PrintDate = value;
            }
        }
        #endregion
        #region ReceiveUser
        public abstract class receiveUser : PX.Data.IBqlField
        {
        }
        protected Guid? _ReceiveUser;
        [PXDBGuid()]
        [PXUIField(DisplayName = "Receive User", Enabled = false)]
        [PXSelector(typeof(Users.pKID)
                    , SubstituteKey = typeof(Users.username))]
        public virtual Guid? ReceiveUser
        {
            get
            {
                return this._ReceiveUser;
            }
            set
            {
                this._ReceiveUser = value;
            }
        }
        #endregion
        #region ReceiveDate
        public abstract class receiveDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _ReceiveDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Receive Date", Enabled = false)]
        public virtual DateTime? ReceiveDate
        {
            get
            {
                return this._ReceiveDate;
            }
            set
            {
                this._ReceiveDate = value;
            }
        }
        #endregion
        #region SendUser
        public abstract class sendUser : PX.Data.IBqlField
        {
        }
        protected Guid? _SendUser;
        [PXDBGuid()]
        [PXUIField(DisplayName = "Send User", Enabled = false)]
        [PXSelector(typeof(Users.pKID)
                    , SubstituteKey = typeof(Users.username))]
        public virtual Guid? SendUser
        {
            get
            {
                return this._SendUser;
            }
            set
            {
                this._SendUser = value;
            }
        }
        #endregion
        #region SendDate
        public abstract class sendDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _SendDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Send Date", Enabled = false)]
        public virtual DateTime? SendDate
        {
            get
            {
                return this._SendDate;
            }
            set
            {
                this._SendDate = value;
            }
        }
        #endregion

        #region RecievedMaintInvoiceUser
        public abstract class recievedMaintInvoiceUser : PX.Data.IBqlField
        {
        }
        protected Guid? _RecievedMaintInvoiceUser;
        [PXDBGuid(false)]
        [PXUIField(DisplayName = "Recieved Maint Invoice User", Enabled = false)]
        [PXSelector(typeof(Users.pKID)
                    , SubstituteKey = typeof(Users.username))]
        public virtual Guid? RecievedMaintInvoiceUser
        {
            get
            {
                return this._RecievedMaintInvoiceUser;
            }
            set
            {
                this._RecievedMaintInvoiceUser = value;
            }
        }
        #endregion
        #region RecievedMaintInvoiceDate
        public abstract class recievedMaintInvoiceDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _RecievedMaintInvoiceDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Recieved Maint Invoice Date", Enabled = false)]
        public virtual DateTime? RecievedMaintInvoiceDate
        {
            get
            {
                return this._RecievedMaintInvoiceDate;
            }
            set
            {
                this._RecievedMaintInvoiceDate = value;
            }
        }
        #endregion
        #region RecievedMaintInvoiceToCompanyUser
        public abstract class recievedMaintInvoiceToCompanyUser : PX.Data.IBqlField
        {
        }
        protected Guid? _RecievedMaintInvoiceToCompanyUser;
        [PXDBGuid()]
        [PXUIField(DisplayName = "Recieved Maint Invoice To Company User", Enabled = false)]
        [PXSelector(typeof(Users.pKID)
                    , SubstituteKey = typeof(Users.username))]
        public virtual Guid? RecievedMaintInvoiceToCompanyUser
        {
            get
            {
                return this._RecievedMaintInvoiceToCompanyUser;
            }
            set
            {
                this._RecievedMaintInvoiceToCompanyUser = value;
            }
        }
        #endregion
        #region RecievedMaintInvoiceToCompanyDate
        public abstract class recievedMaintInvoiceToCompanyDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _RecievedMaintInvoiceToCompanyDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Recieved Maint Invoice To Company Date", Enabled = false)]
        public virtual DateTime? RecievedMaintInvoiceToCompanyDate
        {
            get
            {
                return this._RecievedMaintInvoiceToCompanyDate;
            }
            set
            {
                this._RecievedMaintInvoiceToCompanyDate = value;
            }
        }
        #endregion
        #region CheckRecievedUser
        public abstract class checkRecievedUser : PX.Data.IBqlField
        {
        }
        protected Guid? _CheckRecievedUser;
        [PXDBGuid()]
        [PXUIField(DisplayName = "Check Recieved User", Enabled = false)]
        [PXSelector(typeof(Users.pKID)
                    , SubstituteKey = typeof(Users.username))]
        public virtual Guid? CheckRecievedUser
        {
            get
            {
                return this._CheckRecievedUser;
            }
            set
            {
                this._CheckRecievedUser = value;
            }
        }
        #endregion
        #region CheckRecievedDate
        public abstract class checkRecievedDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _CheckRecievedDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Check Recieved Date", Enabled = false)]
        public virtual DateTime? CheckRecievedDate
        {
            get
            {
                return this._CheckRecievedDate;
            }
            set
            {
                this._CheckRecievedDate = value;
            }
        }
        #endregion

        #region CheckDeliveredUser
        public abstract class checkDeliveredUser : PX.Data.IBqlField
        {
        }
        protected Guid? _CheckDeliveredUser;
        [PXDBGuid(false)]
        [PXUIField(DisplayName = "Print User", Enabled = false)]
        [PXSelector(typeof(Users.pKID)
                    , SubstituteKey = typeof(Users.username))]
        public virtual Guid? CheckDeliveredUser
        {
            get
            {
                return this._CheckDeliveredUser;
            }
            set
            {
                this._CheckDeliveredUser = value;
            }
        }
        #endregion
        #region CheckDeliveredDate
        public abstract class checkDeliveredDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _CheckDeliveredDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Check Delivered Date", Enabled = false)]
        public virtual DateTime? CheckDeliveredDate
        {
            get
            {
                return this._CheckDeliveredDate;
            }
            set
            {
                this._CheckDeliveredDate = value;
            }
        }
        #endregion
        #region ClosedUser
        public abstract class closedUser : PX.Data.IBqlField
        {
        }
        protected Guid? _ClosedUser;
        [PXDBGuid()]
        [PXUIField(DisplayName = "Closed User", Enabled = false)]
        [PXSelector(typeof(Users.pKID)
                    , SubstituteKey = typeof(Users.username))]
        public virtual Guid? ClosedUser
        {
            get
            {
                return this._ClosedUser;
            }
            set
            {
                this._ClosedUser = value;
            }
        }
        #endregion
        #region ClosedDate
        public abstract class closedDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _ClosedDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Closed Date", Enabled = false)]
        public virtual DateTime? ClosedDate
        {
            get
            {
                return this._ClosedDate;
            }
            set
            {
                this._ClosedDate = value;
            }
        }
        #endregion

        #region ReleaseUser
        public abstract class releaseUser : PX.Data.IBqlField
        {
        }
        protected Guid? _ReleaseUser;
        [PXDBGuid()]
        [PXUIField(DisplayName = "Release User", Enabled = false)]
        [PXSelector(typeof(Users.pKID)
                    , SubstituteKey = typeof(Users.username))]
        public virtual Guid? ReleaseUser
        {
            get
            {
                return this._ReleaseUser;
            }
            set
            {
                this._ReleaseUser = value;
            }
        }
        #endregion
        #region ReleaseDate
        public abstract class releaseDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _ReleaseDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Release Date", Enabled = false)]
        public virtual DateTime? ReleaseDate
        {
            get
            {
                return this._ReleaseDate;
            }
            set
            {
                this._ReleaseDate = value;
            }
        }
        #endregion
        
        #region CloseBatchNbr
        public abstract class closeBatchNbr : PX.Data.IBqlField
        {
        }
        protected string _CloseBatchNbr;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Batch Nbr", Enabled = false)]
        public virtual string CloseBatchNbr
        {
            get
            {
                return this._CloseBatchNbr;
            }
            set
            {
                this._CloseBatchNbr = value;
            }
        }
        #endregion
	}
}
