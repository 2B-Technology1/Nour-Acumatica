﻿namespace Maintenance.CD
{
	using System;
	using PX.Data;
    using PX.Objects.AR;
    using PX.Objects.IN;
    using PX.Objects.SO;
    using PX.Objects.GL;
    using PX.SM;
    using MyMaintaince;
	
	[System.SerializableAttribute()]
	public class CertificateDocument : PX.Data.IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected string _RefNbr;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXDefault()]
        [PXSelector(typeof(CertificateDocument.refNbr)
                    , new Type[]{
                    typeof(CertificateDocument.refNbr),
                    typeof(CertificateDocument.realCertificateDocumentNbr)
                    })]
        [PXUIField(DisplayName = "RefNbr", Visibility = PXUIVisibility.SelectorVisible)]
        [JOAutoNumbering(typeof(CDSetUp.autoNumbering), typeof(CDSetUp.lastRefNbr))]
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
		#region Type
		public abstract class type : PX.Data.IBqlField
		{
		}
		protected string _Type;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Type")]
        [PXStringList(
             new string[] { CertificateType.NewC , CertificateType.ReNew, CertificateType.Extension},
             new string[] { CertificateType.NewC , CertificateType.ReNew, CertificateType.Extension}
         )]
		public virtual string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.IBqlField
		{
		}
		protected string _Status;
		[PXDBString(50, IsUnicode = true)]
        [PXDefault(CertificateStatus.NewC)]
		[PXUIField(DisplayName = "Status")]
        [PXStringList(
             new string[] { CertificateStatus.NewC, CertificateStatus.Printed, CertificateStatus.Sended, CertificateStatus.Received, CertificateStatus.Released, CertificateStatus.Transfered, CertificateStatus.CustomerReceived, CertificateStatus.ClosedOut, CertificateStatus.PendingClosedOut},
             new string[] { CertificateStatus.NewC, CertificateStatus.Printed, CertificateStatus.Sended, CertificateStatus.Received, CertificateStatus.Released, CertificateStatus.Transfered, CertificateStatus.CustomerReceived, CertificateStatus.ClosedOut, CertificateStatus.PendingClosedOut }
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
		#region CustomerPhone
		public abstract class customerPhone : PX.Data.IBqlField
		{
		}
		protected string _CustomerPhone;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "CustomerPhone")]
		public virtual string CustomerPhone
		{
			get
			{
				return this._CustomerPhone;
			}
			set
			{
				this._CustomerPhone = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected int? _InventoryID;
        [PXDBInt()]
		[PXUIField(DisplayName = "Vechile")]
        [PXSelector(typeof(InventoryItem.inventoryID)
           , new Type[]{
                     typeof(InventoryItem.inventoryCD),
                     typeof(InventoryItem.descr),
                   }
           , SubstituteKey = typeof(InventoryItem.inventoryCD)
           , DescriptionField = typeof(InventoryItem.descr)
           )]
        public virtual int? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.IBqlField
		{
		}
		protected string _LotSerialNbr;
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Chassis Nbr",Enabled=false)]
		public virtual string LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region ModelYear
		public abstract class modelYear : PX.Data.IBqlField
		{
		}
		protected string _ModelYear;
		[PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Model Year", Enabled = false)]
		public virtual string ModelYear
		{
			get
			{
				return this._ModelYear;
			}
			set
			{
				this._ModelYear = value;
			}
		}
		#endregion
		#region Color
		public abstract class color : PX.Data.IBqlField
		{
		}
		protected int? _Color;
		[PXDBInt]
        [PXUIField(DisplayName = "Color", Enabled = false)]
        [PXSelector(typeof(Colors.colorID)
                   , SubstituteKey = typeof(Colors.colorCD))]
		public virtual int? Color
		{
			get
			{
				return this._Color;
			}
			set
			{
				this._Color = value;
			}
		}
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.IBqlField
		{
		}
		protected string _InvoiceNbr;
		[PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Invoice Nbr", Enabled = false)]
		public virtual string InvoiceNbr
		{
			get
			{
				return this._InvoiceNbr;
			}
			set
			{
				this._InvoiceNbr = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected int? _SiteID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(INSite.siteID)
                   , new Type[]{
                     typeof(INSite.siteCD),
                     typeof(INSite.descr)
                   }
                   , SubstituteKey = typeof(INSite.siteCD)
                   , DescriptionField = typeof(INSite.descr))]
		public virtual int? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region AccountClassID
		public abstract class accountClassID : PX.Data.IBqlField
		{
		}
		protected string _AccountClassID;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Company Class",Enabled=false)]
        [PXDefault("2600")]
        /**
        [PXSelector(typeof(AccountClass.accountClassID)
           , new Type[]{
                     typeof(AccountClass.accountClassID),
                     typeof(AccountClass.type),
                     typeof(INSite.descr)
                   }
           , DescriptionField = typeof(INSite.descr))]
         * */
		public virtual string AccountClassID
		{
			get
			{
				return this._AccountClassID;
			}
			set
			{
				this._AccountClassID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected int? _AccountID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Company")]
        [PXDefault()]
        [PXSelector(typeof(Search<Account.accountID,Where<Account.accountClassID,Equal<Current<CertificateDocument.accountClassID>>>>)
           , new Type[]{
                     typeof(Account.accountCD),
                     typeof(Account.description)
                   }
           , SubstituteKey = typeof(Account.accountCD)
           , DescriptionField = typeof(Account.description))]
		public virtual int? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[PXDefault]
        [PXDBDate()]
		[PXUIField(DisplayName = "Start Date")]
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
        [PXDefault]
        [PXDBDate()]
		[PXUIField(DisplayName = "End Date")]
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
		#region InsuranceCoverageAmt
		public abstract class insuranceCoverageAmt : PX.Data.IBqlField
		{
		}
		protected float? _InsuranceCoverageAmt;
		[PXDBFloat(2)]
		[PXUIField(DisplayName = "Insurance Coverage Amt.")]
		public virtual float? InsuranceCoverageAmt
		{
			get
			{
				return this._InsuranceCoverageAmt;
			}
			set
			{
				this._InsuranceCoverageAmt = value;
			}
		}
		#endregion
		#region InsuranceRatio
		public abstract class insuranceRatio : PX.Data.IBqlField
		{
		}
		protected float? _InsuranceRatio;
		[PXDBFloat(2)]
		[PXUIField(DisplayName = "Insurance Ratio")]
		public virtual float? InsuranceRatio
		{
			get
			{
				return this._InsuranceRatio;
			}
			set
			{
				this._InsuranceRatio = value;
			}
		}
		#endregion
		#region DocumentAmount
		public abstract class documentAmount : PX.Data.IBqlField
		{
		}
		protected float? _DocumentAmount;
		[PXDBFloat(2)]
		[PXUIField(DisplayName = "Document Amount")]
        [PXFormula(typeof(Mult<CertificateDocument.insuranceCoverageAmt, CertificateDocument.insuranceRatio>))]
        public virtual float? DocumentAmount
		{
			get
			{
				return this._DocumentAmount;
			}
			set
			{
				this._DocumentAmount = value;
			}
		}
		#endregion
		#region InsuranceKind
		public abstract class insuranceKind : PX.Data.IBqlField
		{
		}
		protected string _InsuranceKind;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Insurance Kind")]
        [PXDefault(InsuranceKinds.WithCoverage)]
        [PXStringList(
             new string[] { InsuranceKinds.WithCoverage, InsuranceKinds.WithoutCoverage },
             new string[] { InsuranceKinds.WithCoverage, InsuranceKinds.WithoutCoverage }
         )]
		public virtual string InsuranceKind
		{
			get
			{
				return this._InsuranceKind;
			}
			set
			{
				this._InsuranceKind = value;
			}
		}
		#endregion
		#region InvoicePaymentType
		public abstract class invoicePaymentType : PX.Data.IBqlField
		{
		}
		protected string _InvoicePaymentType;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Invoice Payment Type")]
        [PXDefault(InvoicePaymentTypes.Cash)]
        [PXStringList(
             new string[] { InvoicePaymentTypes.Cash, InvoicePaymentTypes.Installment },
             new string[] { InvoicePaymentTypes.Cash, InvoicePaymentTypes.Installment }
         )]
		public virtual string InvoicePaymentType
		{
			get
			{
				return this._InvoicePaymentType;
			}
			set
			{
				this._InvoicePaymentType = value;
			}
		}
		#endregion
		#region BankName
		public abstract class bankName : PX.Data.IBqlField
		{
		}
		protected string _BankName;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank Name")]
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
		#region BankBranch
		public abstract class bankBranch : PX.Data.IBqlField
		{
		}
		protected string _BankBranch;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank Branch")]
		public virtual string BankBranch
		{
			get
			{
				return this._BankBranch;
			}
			set
			{
				this._BankBranch = value;
			}
		}
		#endregion
		#region InstallmentPeriodYear
		public abstract class installmentPeriodYear : PX.Data.IBqlField
		{
		}
		protected string _InstallmentPeriodYear;
		[PXDBString(4, IsUnicode = true , InputMask="####")]
		[PXUIField(DisplayName = "Installment Period Year")]
		public virtual string InstallmentPeriodYear
		{
			get
			{
				return this._InstallmentPeriodYear;
			}
			set
			{
				this._InstallmentPeriodYear = value;
			}
		}
		#endregion
		#region InstallmentPeriodMonth
		public abstract class installmentPeriodMonth : PX.Data.IBqlField
		{
		}
		protected string _InstallmentPeriodMonth;
        [PXDBString(2, IsUnicode = true,InputMask = "##")]
		[PXUIField(DisplayName = "Installment Period Month")]
		public virtual string InstallmentPeriodMonth
		{
			get
			{
				return this._InstallmentPeriodMonth;
			}
			set
			{
				this._InstallmentPeriodMonth = value;
			}
		}
		#endregion
		#region DemandDate
		public abstract class demandDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DemandDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Demand Date")]
		public virtual DateTime? DemandDate
		{
			get
			{
				return this._DemandDate;
			}
			set
			{
				this._DemandDate = value;
			}
		}
		#endregion
		#region ToSiteID
		public abstract class toSiteID : PX.Data.IBqlField
		{
		}
		protected int? _ToSiteID;
		[PXDBInt()]
		[PXUIField(DisplayName = "New Branch")]
        [PXSelector(typeof(INSite.siteID)
                   , new Type[]{
                     typeof(INSite.siteCD),
                     typeof(INSite.descr)
                   }
                   , SubstituteKey = typeof(INSite.siteCD)
                   , DescriptionField = typeof(INSite.descr))]
        public virtual int? ToSiteID
		{
			get
			{
				return this._ToSiteID;
			}
			set
			{
				this._ToSiteID = value;
			}
		}
		#endregion
		#region RealCertificateDocumentNbr
		public abstract class realCertificateDocumentNbr : PX.Data.IBqlField
		{
		}
		protected string _RealCertificateDocumentNbr;
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Real Certificate Document Nbr.")]
		public virtual string RealCertificateDocumentNbr
		{
			get
			{
				return this._RealCertificateDocumentNbr;
			}
			set
			{
				this._RealCertificateDocumentNbr = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected string _Descr;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
        #region ExtensionReason
        public abstract class extensionReason : PX.Data.IBqlField
        {
        }
        protected string _ExtensionReason;
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Extension Reason")]
        [PXStringList(
             new string[] { "Period Ext.", "Motor Modification", "Chassis Modification", "Pallete Modification", "Name Modification", "Amount Increase", "Compensation", "Access Modification" },
             new string[] { "Period Ext.", "Motor Modification", "Chassis Modification", "Pallete Modification", "Name Modification", "Amount Increase", "Compensation", "Access Modification" }
         )]
        public virtual string ExtensionReason
        {
            get
            {
                return this._ExtensionReason;
            }
            set
            {
                this._ExtensionReason = value;
            }
        }
        #endregion
        #region ExtensionAmt
        public abstract class extensionAmt : PX.Data.IBqlField
        {
        }
        protected float? _ExtensionAmt;
        [PXDBFloat()]
        [PXUIField(DisplayName = "Extension Amt.")]
        public virtual float? ExtensionAmt
        {
            get
            {
                return this._ExtensionAmt;
            }
            set
            {
                this._ExtensionAmt = value;
            }
        }
        #endregion
        #region PrintUser
		public abstract class printUser : PX.Data.IBqlField
		{
		}
		protected Guid? _PrintUser;
		[PXDBGuid(false)]
        [PXUIField(DisplayName = "Print User",Enabled=false)]
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
		#region TransferUser
		public abstract class transferUser : PX.Data.IBqlField
		{
		}
		protected Guid? _TransferUser;
        [PXDBGuid()]
        [PXUIField(DisplayName = "Transfer User", Enabled = false)]
        [PXSelector(typeof(Users.pKID)
                    , SubstituteKey = typeof(Users.username))]
		public virtual Guid? TransferUser
		{
			get
			{
				return this._TransferUser;
			}
			set
			{
				this._TransferUser = value;
			}
		}
		#endregion
		#region TransferDate
		public abstract class transferDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TransferDate;
		[PXDBDate()]
        [PXUIField(DisplayName = "Transfer Date", Enabled = false)]
		public virtual DateTime? TransferDate
		{
			get
			{
				return this._TransferDate;
			}
			set
			{
				this._TransferDate = value;
			}
		}
		#endregion
		#region CustomerReceivedUser
		public abstract class customerReceivedUser : PX.Data.IBqlField
		{
		}
		protected Guid? _CustomerReceivedUser;
        [PXDBGuid()]
        [PXUIField(DisplayName = "Customer Received User", Enabled = false)]
        [PXSelector(typeof(Users.pKID)
                    , SubstituteKey = typeof(Users.username))]
		public virtual Guid? CustomerReceivedUser
		{
			get
			{
				return this._CustomerReceivedUser;
			}
			set
			{
				this._CustomerReceivedUser = value;
			}
		}
		#endregion
		#region CustomerReceivedDate
		public abstract class customerReceivedDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _CustomerReceivedDate;
		[PXDBDate()]
        [PXUIField(DisplayName = "Customer Received Date", Enabled = false)]
		public virtual DateTime? CustomerReceivedDate
		{
			get
			{
				return this._CustomerReceivedDate;
			}
			set
			{
				this._CustomerReceivedDate = value;
			}
		}
		#endregion
        #region ClosedoutUser
        public abstract class closedoutUser : PX.Data.IBqlField
        {
        }
        protected Guid? _ClosedoutUser;
        [PXDBGuid()]
        [PXUIField(DisplayName = "Closed-out User", Enabled = false)]
        [PXSelector(typeof(Users.pKID)
                    , SubstituteKey = typeof(Users.username))]
        public virtual Guid? ClosedoutUser
        {
            get
            {
                return this._ClosedoutUser;
            }
            set
            {
                this._ClosedoutUser = value;
            }
        }
        #endregion
        #region ClosedoutDate
        public abstract class closedoutDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _ClosedoutDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Closed-out Date", Enabled = false)]
        public virtual DateTime? ClosedoutDate
        {
            get
            {
                return this._ClosedoutDate;
            }
            set
            {
                this._ClosedoutDate = value;
            }
        }
        #endregion
        #region CloseoutONDate
        public abstract class closeoutONDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _CloseoutONDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Closed-out Date")]
        public virtual DateTime? CloseoutONDate
        {
            get
            {
                return this._CloseoutONDate;
            }
            set
            {
                this._CloseoutONDate = value;
            }
        }
        #endregion
        #region CloseoutAmt
        public abstract class closeoutAmt : PX.Data.IBqlField
        {
        }
        protected float? _CloseoutAmt;
        [PXDBFloat()]
        [PXUIField(DisplayName = "Close-out Amount")]
        public virtual float? CloseoutAmt
        {
            get
            {
                return this._CloseoutAmt;
            }
            set
            {
                this._CloseoutAmt = value;
            }
        }
        #endregion
        #region GLBatchNbr
        public abstract class gLBatchNbr : PX.Data.IBqlField
        {
        }
        protected string _GLBatchNbr;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Batch Nbr" ,Enabled=false)]
        public virtual string GLBatchNbr
        {
            get
            {
                return this._GLBatchNbr;
            }
            set
            {
                this._GLBatchNbr = value;
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

        #region Payed
        public abstract class payed : PX.Data.IBqlField
        {
        }
        protected bool? _Payed;
        [PXDBBool()]
        [PXUIField(DisplayName = "Payed", Enabled = false)]
        public virtual bool? Payed
        {
            get
            {
                return this._Payed;
            }
            set
            {
                this._Payed = value;
            }
        }
        #endregion
        #region LicencePlateNbr
        public abstract class licencePlateNbr : PX.Data.IBqlField
        {
        }
        protected string _LicencePlateNbr;
        [PXDBString(14, IsUnicode = true)]
        [PXUIField(DisplayName = "Licence Plate Nbr",Visible=true)]
        public virtual string LicencePlateNbr
        {
            get
            {
                return this._LicencePlateNbr;
            }
            set
            {
                this._LicencePlateNbr = value;
            }
        }
        #endregion
	}
}
