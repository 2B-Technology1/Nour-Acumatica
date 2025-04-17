﻿namespace MyMaintaince.LM
{
	using System;
	using PX.Data;
    using PX.Objects.PO;
    using PX.Objects.AP;
    using PX.Objects.IN;

	[System.SerializableAttribute()]
	public class VendorReceivedDocumentsD : PX.Data.IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected string _RefNbr;
		[PXDBString(50, IsUnicode = true, IsKey = true)]
        [PXDBDefault(typeof(VendorReceivedDocumentsH.refNbr))]
        [PXParent(typeof(Select<VendorReceivedDocumentsH, Where<VendorReceivedDocumentsH.refNbr, Equal<Current<VendorReceivedDocumentsD.refNbr>>>>))]
		[PXUIField(DisplayName = "RefNbr")]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected int? _LineNbr;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
		public virtual int? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region PONbr
		public abstract class pONbr : PX.Data.IBqlField
		{
		}
		protected string _PONbr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "PONbr")]
        public virtual string PONbr
		{
			get
			{
				return this._PONbr;
			}
			set
			{
				this._PONbr = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.IBqlField
		{
		}
		protected int? _VendorID;
		[PXDBInt()]
		[PXUIField(DisplayName = "VendorID")]
        [PXSelector(typeof(Vendor.bAccountID)
                    , DescriptionField = typeof(Vendor.acctName)
                    , SubstituteKey = typeof(Vendor.acctCD))]
		public virtual int? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region ItemCode
		public abstract class itemCode : PX.Data.IBqlField
		{
		}
		protected int? _ItemCode;
		[PXDBInt()]
		[PXUIField(DisplayName = "ItemCode")]
        [PXSelector(typeof(InventoryItem.inventoryID)
            , DescriptionField = typeof(InventoryItem.descr)
            , SubstituteKey = typeof(InventoryItem.inventoryCD))]
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
		#region ChassisNbr
		public abstract class chassisNbr : PX.Data.IBqlField
		{
		}
		protected string _ChassisNbr;
		[PXDBString(300, IsUnicode = true)]
		[PXUIField(DisplayName = "ChassisNbr")]
        //[PXSelector(typeof(Search2<POReceiptLineSplit.lotSerialNbr, InnerJoin<POReceipt, On<POReceipt.receiptType, Equal<POReceiptLineSplit.receiptType>, And<POReceipt.receiptNbr, Equal<POReceiptLineSplit.receiptNbr>>>>,Where<POReceipt.released,Equal<True>>>)
        [PXSelector(typeof(Search<POReceiptLineSplit.lotSerialNbr>)
            ,new Type[]{
              typeof(POReceiptLineSplit.receiptType),
              typeof(POReceiptLineSplit.receiptNbr),
              typeof(POReceiptLineSplit.receiptDate),
              typeof(POReceiptLineSplit.pONbr),
              typeof(POReceiptLineSplit.siteID),
              typeof(POReceiptLineSplit.locationID),
              typeof(POReceiptLineSplit.inventoryID),
              typeof(POReceiptLineSplit.qty),
              typeof(POReceiptLineSplit.lotSerialNbr)
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
		#region ModelYear
		public abstract class modelYear : PX.Data.IBqlField
		{
		}
		protected string _ModelYear;
		[PXDBString(4, IsUnicode = true)]
		[PXUIField(DisplayName = "ModelYear")]
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
		[PXUIField(DisplayName = "Color")]
        [PXSelector(typeof(Colors.colorID)
            , DescriptionField = typeof(Colors.descr)
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
		#region Received
		public abstract class received : PX.Data.IBqlField
		{
		}
		protected bool? _Received;
		[PXDBBool()]
		[PXUIField(DisplayName = "Received")]
		public virtual bool? Received
		{
			get
			{
				return this._Received;
			}
			set
			{
				this._Received = value;
			}
		}
		#endregion
		#region ReceivedName
		public abstract class receivedName : PX.Data.IBqlField
		{
		}
		protected string _ReceivedName;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "ReceivedName")]
		public virtual string ReceivedName
		{
			get
			{
				return this._ReceivedName;
			}
			set
			{
				this._ReceivedName = value;
			}
		}
		#endregion
		#region ReceivedDate
		public abstract class receivedDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ReceivedDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "ReceivedDate")]
		public virtual DateTime? ReceivedDate
		{
			get
			{
				return this._ReceivedDate;
			}
			set
			{
				this._ReceivedDate = value;
			}
		}
		#endregion
		#region OrigCertificationDate
		public abstract class origCertificationDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _OrigCertificationDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "OrigCertificationDate")]
		public virtual DateTime? OrigCertificationDate
		{
			get
			{
				return this._OrigCertificationDate;
			}
			set
			{
				this._OrigCertificationDate = value;
			}
		}
		#endregion
	}
}
