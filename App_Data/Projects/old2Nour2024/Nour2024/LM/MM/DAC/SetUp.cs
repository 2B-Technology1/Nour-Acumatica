using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace MyMaintaince
{
	

    [System.SerializableAttribute()]
	public class SetUp : PX.Data.IBqlTable
	{
        

       

        #region SetupID
        public abstract class setupID : PX.Data.IBqlField
		{
		}
		protected int? _SetupID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "SetupID")]
    
        //[PXDBInt(IsKey = true)]
        //[PXDefault()]
        public virtual int? SetupID
		{
			get
			{
				return this._SetupID;
			}
			set
			{
				this._SetupID = value;
			}
		}
		#endregion
		#region ReceiptLastRefNbr
		public abstract class receiptLastRefNbr : PX.Data.IBqlField
		{
		}
		protected string _ReceiptLastRefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "ReceiptLastRefNbr")]
		public virtual string ReceiptLastRefNbr
		{
			get
			{
				return this._ReceiptLastRefNbr;
			}
			set
			{
				this._ReceiptLastRefNbr = value;
			}
		}
		#endregion
		#region InreturnLastRefNbr
		public abstract class inreturnLastRefNbr : PX.Data.IBqlField
		{
		}
		protected string _InreturnLastRefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "InreturnLastRefNbr")]
		public virtual string InreturnLastRefNbr
		{
			get
			{
				return this._InreturnLastRefNbr;
			}
			set
			{
				this._InreturnLastRefNbr = value;
			}
		}
		#endregion
		#region SalesOrderLastRefNbr
		public abstract class salesOrderLastRefNbr : PX.Data.IBqlField
		{
		}
		protected string _SalesOrderLastRefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "SalesOrderLastRefNbr")]
		public virtual string SalesOrderLastRefNbr
		{
			get
			{
				return this._SalesOrderLastRefNbr;
			}
			set
			{
				this._SalesOrderLastRefNbr = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected int? _BranchID;
		[PXDBInt()]
		[PXUIField(Enabled = false)]
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
		#region BranchCD
		public abstract class branchCD : PX.Data.IBqlField
		{
		}
		protected string _BranchCD;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "BranchCD")]
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
		#region RequstRefNbr
		public abstract class requstRefNbr : PX.Data.IBqlField
		{
		}
		protected string _RequstRefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "RequstRefNbr")]
		public virtual string RequstRefNbr
		{
			get
			{
				return this._RequstRefNbr;
			}
			set
			{
				this._RequstRefNbr = value;
			}
		}
		#endregion
        #region ReserveRefNbr
        public abstract class reserveRefNbr : PX.Data.IBqlField
        {
        }
        protected string _ReserveRefNbr;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "ReserveRefNbr")]
        public virtual string ReserveRefNbr
        {
            get
            {
                return this._ReserveRefNbr;
            }
            set
            {
                this._ReserveRefNbr = value;
            }
        }
        #endregion
		#region SubAccount
		public abstract class subAccount : PX.Data.IBqlField
		{
		}
		protected int? _SubAccount;
		[PXDBInt()]
		[PXUIField(DisplayName = "SubAccount")]
        [PXSelector(typeof(Search<Sub.subID>)
                 , new Type[] 
                 {
                     typeof(Sub.subCD),
                     typeof(Sub.description)
                 }
                 , DescriptionField = typeof(Sub.description)
                 , SubstituteKey = typeof(Sub.subCD))]
        public virtual int? SubAccount
		{
			get
			{
				return this._SubAccount;
			}
			set
			{
				this._SubAccount = value;
			}
		}
		#endregion
		#region AutoNumbering
		public abstract class autoNumbering : PX.Data.IBqlField
		{
		}
		protected bool? _AutoNumbering;
		[PXDBBool()]
		[PXUIField(DisplayName = "AutoNumbering")]
		public virtual bool? AutoNumbering
		{
			get
			{
				return this._AutoNumbering;
			}
			set
			{
				this._AutoNumbering = value;
			}
		}
		#endregion

		#region TaxInvRefnbr
		public abstract class taxInvRefnbr : PX.Data.IBqlField
		{
		}
		protected string _TaxInvRefnbr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Invoic Refnbr")]
		public virtual string TaxInvRefnbr
		{
			get
			{
				return this._TaxInvRefnbr;
			}
			set
			{
				this._TaxInvRefnbr = value;
			}
		}
		#endregion

		#region TaxInvRefnbr
		public abstract class taxCmRefnbr : PX.Data.IBqlField
		{
		}
		protected string _TaxCmRefnbr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax CM Refnbr")]
		public virtual string TaxCmRefnbr
		{
			get
			{
				return this._TaxCmRefnbr;
			}
			set
			{
				this._TaxCmRefnbr = value;
			}
		}
		#endregion

		#region TaxSerial
		public abstract class taxSerial : PX.Data.IBqlField
		{
		}
		protected bool? _TaxSerial;
		[PXDBBool()]
		[PXUIField(DisplayName = "Tax Serial")]
		public virtual bool? TaxSerial
		{
			get
			{
				return this._TaxSerial;
			}
			set
			{
				this._TaxSerial = value;
			}
		}
		#endregion

		#region UsrGpsLink
		[PXDBString(200)]
		[PXUIField(DisplayName = "GpsLink")]
		public virtual string UsrGpsLink { get; set; }
		public abstract class usrGpsLink : PX.Data.BQL.BqlString.Field<usrGpsLink> { }
        #endregion
    }
}
