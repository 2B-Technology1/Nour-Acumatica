﻿namespace MyMaintaince.LM
{
	using System;
	using PX.Data;
    using PX.Objects.SO;
    using PX.Objects.AR;
    using PX.Objects.IN;
    using PX.Objects.EP;
    using PX.SM;
	
	[System.SerializableAttribute()]
	public class Licencing : PX.Data.IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected string _RefNbr;
        [PXDBString(100, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "RefNbr")]
        [PXSelector(typeof(Licencing.refNbr))]
        [LMTAutoNumbering(typeof(LMTSetup.autoNumbering), typeof(LMTSetup.lastRefNbr))]
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
        #region Status
        public abstract class status : PX.Data.IBqlField
        {
        }
        protected string _Status;
        [PXDBString(300, IsUnicode = true)]
        [PXDefault(LicenceStatus.Open)]
        [PXUIField(DisplayName = "Status")]
        [PXStringList(new String[] { LicenceStatus.Open, LicenceStatus.Printed, LicenceStatus.Sent, LicenceStatus.Transfered, LicenceStatus.Received, LicenceStatus.Released, LicenceStatus.CustomerReceived }, new String[] { LicenceStatus.Open, LicenceStatus.Printed, LicenceStatus.Sent, LicenceStatus.Transfered, LicenceStatus.Received, LicenceStatus.Released, LicenceStatus.CustomerReceived })]
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
        #region ChassisNbr
		public abstract class chassisNbr : PX.Data.IBqlField
		{
		}
		protected string _ChassisNbr;
		[PXDBString(300, IsUnicode = true)]
		[PXUIField(DisplayName = "ChassisNbr")]
        //[PXSelector(typeof(Search2<POReceiptLineSplit.lotSerialNbr, InnerJoin<POReceipt, On<POReceipt.receiptType, Equal<POReceiptLineSplit.receiptType>, And<POReceipt.receiptNbr, Equal<POReceiptLineSplit.receiptNbr>>>>,Where<POReceipt.released,Equal<True>>>)
        [PXSelector(typeof(Search2<SOLineSplit.lotSerialNbr,InnerJoin<SOOrder,On<SOOrder.orderType,Equal<SOLineSplit.orderType>,And<SOOrder.orderNbr,Equal<SOLineSplit.orderNbr>>>>,Where<SOOrder.status,Equal<SOOrderStatus.completed>>>)
            , new Type[]{             
              typeof(SOLineSplit.orderType),
              typeof(SOLineSplit.orderNbr),
              typeof(SOLineSplit.orderDate),
              typeof(SOLineSplit.siteID),
              typeof(SOLineSplit.locationID),
              typeof(SOLineSplit.inventoryID),
              typeof(SOLineSplit.qty),
              typeof(SOLineSplit.lotSerialNbr)
            }
           )]

		public virtual string ChassisNbr
		{
			get
			{
				return this._ChassisNbr;
			}
			set
			{
				this._ChassisNbr = value;
			}
		}
		#endregion
        #region SONbr
		public abstract class sONbr : PX.Data.IBqlField
		{
		}
		protected string _SONbr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "SONbr",Enabled=false)]
		public virtual string SONbr
		{
			get
			{
				return this._SONbr;
			}
			set
			{
				this._SONbr = value;
			}
		}
		#endregion
        #region SOType
        public abstract class sOType : PX.Data.IBqlField
        {
        }
        protected string _SOType;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "SO Type", Enabled = false)]
        [PXSelector(typeof(SOOrderType.orderType)
                   , DescriptionField = typeof(SOOrderType.descr))]
        public virtual string SOType
        {
            get
            {
                return this._SOType;
            }
            set
            {
                this._SOType = value;
            }
        }
        #endregion
        #region ARNbr
        public abstract class aRNbr : PX.Data.IBqlField
        {
        }
        protected string _ARNbr;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "AR Nbr", Enabled = false)]
        public virtual string ARNbr
        {
            get
            {
                return this._ARNbr;
            }
            set
            {
                this._ARNbr = value;
            }
        }
        #endregion
        #region ARType
        public abstract class aRType : PX.Data.IBqlField
        {
        }
        protected string _ARType;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "AR Type", Enabled = false)]
        [PXSelector(typeof(SOInvoice.docType))]
        public virtual string ARType
        {
            get
            {
                return this._ARType;
            }
            set
            {
                this._ARType = value;
            }
        }
        #endregion
        #region CustomerID
		public abstract class customerID : PX.Data.IBqlField
		{
		}
		protected int? _CustomerID;
		[PXDBInt()]
        [PXUIField(DisplayName = "CustomerID", Enabled = false)]
        [PXSelector(typeof(Customer.bAccountID)
                   , DescriptionField = typeof(Customer.acctName)
                   , SubstituteKey    = typeof(Customer.acctCD)
                   )]
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
        #region LicenceRepID
        public abstract class licenceRepID : PX.Data.IBqlField
        {
        }
        protected int? _LicenceRepID;
        [PXDBInt()]
        [PXUIField(DisplayName = "Licence Rep ID")]
        [PXSelector(typeof(EPEmployee.bAccountID)
                   , DescriptionField = typeof(EPEmployee.acctName)
                   , SubstituteKey = typeof(EPEmployee.acctCD)
                   )]
        public virtual int? LicenceRepID
        {
            get
            {
                return this._LicenceRepID;
            }
            set
            {
                this._LicenceRepID = value;
            }
        }
        #endregion
		#region ItemCode
		public abstract class itemCode : PX.Data.IBqlField
		{
		}
		protected int? _ItemCode;
		[PXDBInt()]
        [PXUIField(DisplayName = "ItemCode", Enabled = false)]
        [PXSelector(typeof(InventoryItem.inventoryID)
           , DescriptionField = typeof(InventoryItem.descr)
           , SubstituteKey = typeof(InventoryItem.inventoryCD)
           )]
		public virtual int? ItemCode
		{
			get
			{
				return this._ItemCode;
			}
			set
			{
				this._ItemCode = value;
			}
		}
		#endregion
		#region ModelYear
		public abstract class modelYear : PX.Data.IBqlField
		{
		}
		protected string _ModelYear;
		[PXDBString(4, IsUnicode = true)]
        [PXUIField(DisplayName = "ModelYear", Enabled = false)]
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
		[PXDBInt()]
        [PXUIField(DisplayName = "Color", Enabled = false)]
        [PXSelector(typeof(Colors.colorID)
           , DescriptionField = typeof(Colors.descr)
           , SubstituteKey    = typeof(Colors.colorCD)
           )]
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
		#region InvoiceDate
		public abstract class invoiceDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _InvoiceDate;
		[PXDBDate()]
        [PXUIField(DisplayName = "Order Date", Enabled = false)]
		public virtual DateTime? InvoiceDate
		{
			get
			{
				return this._InvoiceDate;
			}
			set
			{
				this._InvoiceDate = value;
			}
		}
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.IBqlField
		{
		}
		protected string _InvoiceNbr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "InvoiceNbr")]
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
		#region SOPaymentType
		public abstract class sOPaymentType : PX.Data.IBqlField
		{
		}
		protected string _SOPaymentType;
		[PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "SO Payment Type", Enabled = false)]
        [PXStringList(new string[] { "CA", "IN", "SP", "II" }, new string[] { "Cash", "Installment", "Sales Panned", "Internal Installment" })]
		public virtual string SOPaymentType
		{
			get
			{
				return this._SOPaymentType;
			}
			set
			{
				this._SOPaymentType = value;
			}
		}
		#endregion
		#region TrafficUnitDate
		public abstract class trafficUnitDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TrafficUnitDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "TrafficUnitDate")]
		public virtual DateTime? TrafficUnitDate
		{
			get
			{
				return this._TrafficUnitDate;
			}
			set
			{
				this._TrafficUnitDate = value;
			}
		}
		#endregion
		#region CustomerCarReceiveDate
		public abstract class customerCarReceiveDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _CustomerCarReceiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "CustomerCarReceiveDate")]
		public virtual DateTime? CustomerCarReceiveDate
		{
			get
			{
				return this._CustomerCarReceiveDate;
			}
			set
			{
				this._CustomerCarReceiveDate = value;
			}
		}
		#endregion
		#region CustomerLicenceReceiveDate
		public abstract class customerLicenceReceiveDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _CustomerLicenceReceiveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "CustomerLicenceReceiveDate")]
		public virtual DateTime? CustomerLicenceReceiveDate
		{
			get
			{
				return this._CustomerLicenceReceiveDate;
			}
			set
			{
				this._CustomerLicenceReceiveDate = value;
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
		#region StartLicencingDate
		public abstract class startLicencingDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartLicencingDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "StartLicencingDate")]
		public virtual DateTime? StartLicencingDate
		{
			get
			{
				return this._StartLicencingDate;
			}
			set
			{
				this._StartLicencingDate = value;
			}
		}
		#endregion
		#region EndLicencingDate
		public abstract class endLicencingDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EndLicencingDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "EndLicencingDate")]
		public virtual DateTime? EndLicencingDate
		{
			get
			{
				return this._EndLicencingDate;
			}
			set
			{
				this._EndLicencingDate = value;
			}
		}
		#endregion
		#region Governrate
		public abstract class governrate : PX.Data.IBqlField
		{
		}
		protected int? _Governrate;
		[PXDBInt()]
		[PXUIField(DisplayName = "Governrate")]
        [PXSelector(typeof(Governrate.govID)
           , DescriptionField = typeof(Governrate.descr)
           , SubstituteKey = typeof(Governrate.govCD)
           )]
        public virtual int? Governrate
		{
			get
			{
				return this._Governrate;
			}
			set
			{
				this._Governrate = value;
			}
		}
		#endregion
		#region TrafficUnit
		public abstract class trafficUnit : PX.Data.IBqlField
		{
		}
		protected int? _TrafficUnit;
		[PXDBInt()]
		[PXUIField(DisplayName = "TrafficUnit")]
        [PXSelector(typeof(Search<TrafficUnits.trafficUnitID, Where<TrafficUnits.govID,Equal<Current<Licencing.governrate>>>>)
           , DescriptionField = typeof(TrafficUnits.tRDescr)
           , SubstituteKey = typeof(TrafficUnits.trafficUnitCD)
           )]
		public virtual int? TrafficUnit
		{
			get
			{
				return this._TrafficUnit;
			}
			set
			{
				this._TrafficUnit = value;
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

	}
}
