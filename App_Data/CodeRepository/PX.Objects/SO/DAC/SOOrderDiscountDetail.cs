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

namespace PX.Objects.SO
{
	using System;
	using System.Collections;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.Common.Discount;
    using PX.Objects.IN;
	using PX.Objects.CM;
	using PX.Objects.GL;
    using PX.Objects.AR;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.SOOrderDiscountDetail)]
    public partial class SOOrderDiscountDetail : PX.Data.IBqlTable, IDiscountDetail
    {
		#region Keys
		public class PK : PrimaryKeyOf<SOOrderDiscountDetail>.By<orderType, orderNbr, recordID>
		{
			public static SOOrderDiscountDetail Find(PXGraph graph, string orderType, string orderNbr, int? recordID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, orderType, orderNbr, recordID, options);
		}
		public static class FK
		{
			public class OrderType : SOOrderType.PK.ForeignKeyOf<SOOrderDiscountDetail>.By<orderType> { }
			public class Order : SOOrder.PK.ForeignKeyOf<SOOrderDiscountDetail>.By<orderType, orderNbr> { }
			public class FreeInventoryItem : InventoryItem.PK.ForeignKeyOf<SOOrderDiscountDetail>.By<freeItemID> { }
			public class Discount : ARDiscount.PK.ForeignKeyOf<SOOrderDiscountDetail>.By<discountID> { }
			public class DiscountSequence : AR.DiscountSequence.PK.ForeignKeyOf<SOOrderDiscountDetail>.By<discountID, discountSequenceID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<SOOrderDiscountDetail>.By<curyInfoID> { }
		}
		#endregion
		
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
        protected Int32? _RecordID;
        [PXDBIdentity(IsKey = true)]
        public virtual Int32? RecordID
		{
            get
            {
                return this._RecordID;
            }
            set
            {
                this._RecordID = value;
            }
        }
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected ushort? _LineNbr;
		[PXDBUShort()]
		[PXUIField(DisplayName = "Line Nbr.")]
		[PXLineNbr(typeof(SOOrder))]
		//[PXLineNbr(typeof(SOOrder), ReuseGaps = true)]
		public virtual ushort? LineNbr
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
		#region SkipDiscount
		public abstract class skipDiscount : PX.Data.BQL.BqlBool.Field<skipDiscount> { }
        protected Boolean? _SkipDiscount;
        [PXDBBool()]
        [PXDefault(false)]
		[PXUIEnabled(typeof(Where<SOOrderDiscountDetail.type, NotEqual<DiscountType.ExternalDocumentDiscount>, And<SOOrderDiscountDetail.discountID, IsNotNull>>))]
		[PXUIField(DisplayName = "Skip Discount", Enabled = true)]
        public virtual Boolean? SkipDiscount
        {
            get
            {
                return this._SkipDiscount;
            }
            set
            {
                this._SkipDiscount = value;
            }
        }
        #endregion
        #region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(SOOrder.orderType))]
		[PXUIField(DisplayName = "Order Type", Enabled = false)]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(SOOrder.orderNbr))]
		[PXParent(typeof(FK.Order))]
		[PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region DiscountID
		public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
		protected String _DiscountID;
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Discount Code")]
		[PXUIEnabled(typeof(Where<SOOrderDiscountDetail.type, NotEqual<DiscountType.ExternalDocumentDiscount>>))]
		[PXSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, NotEqual<DiscountType.LineDiscount>>>))]
		[PXForeignReference(typeof(FK.DiscountSequence))]
		public virtual String DiscountID
		{
			get
			{
				return this._DiscountID;
			}
			set
			{
				this._DiscountID = value;
			}
		}
		#endregion
		#region DiscountSequenceID
		public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }
		protected String _DiscountSequenceID;
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Sequence ID")]
		[PXUIEnabled(typeof(Where<SOOrderDiscountDetail.type, NotEqual<DiscountType.ExternalDocumentDiscount>>))]
		[PXSelector(typeof(Search<DiscountSequence.discountSequenceID, Where<DiscountSequence.isActive, Equal<True>, And<DiscountSequence.discountID, Equal<Current<SOOrderDiscountDetail.discountID>>>>>))]
		public virtual String DiscountSequenceID
		{
			get
			{
				return this._DiscountSequenceID;
			}
			set
			{
				this._DiscountSequenceID = value;
			}
		}
		#endregion
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected String _Type;
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		[DiscountType.List()]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		public virtual String Type
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
		#region ManualOrder
		public abstract class manualOrder : PX.Data.BQL.BqlShort.Field<manualOrder> { }
		protected Int16? _ManualOrder;
		[PXDBShort()]
		[PXLineNbr(typeof(SOOrder))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
		public virtual Int16? ManualOrder
		{
			get
			{
				return this._ManualOrder;
			}
			set
			{
				this._ManualOrder = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(SOOrder.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region DiscountableAmt
		public abstract class discountableAmt : PX.Data.BQL.BqlDecimal.Field<discountableAmt> { }
		protected Decimal? _DiscountableAmt;
		[PXDBDecimal(4)]
		public virtual Decimal? DiscountableAmt
		{
			get
			{
				return this._DiscountableAmt;
			}
			set
			{
				this._DiscountableAmt = value;
			}
		}
		#endregion
		#region CuryDiscountableAmt
		public abstract class curyDiscountableAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscountableAmt> { }
		protected Decimal? _CuryDiscountableAmt;
		[PXDBCurrency(typeof(SOOrderDiscountDetail.curyInfoID), typeof(SOOrderDiscountDetail.discountableAmt))]
		[PXUIField(DisplayName = "Discountable Amt.", Enabled = false)]
		public virtual Decimal? CuryDiscountableAmt
		{
			get
			{
				return this._CuryDiscountableAmt;
			}
			set
			{
				this._CuryDiscountableAmt = value;
			}
		}
		#endregion
		#region DiscountableQty
		public abstract class discountableQty : PX.Data.BQL.BqlDecimal.Field<discountableQty> { }
		protected Decimal? _DiscountableQty;
		[PXDBQuantity(MinValue=0)]
		[PXUIField(DisplayName = "Discountable Qty.", Enabled = false)]
		public virtual Decimal? DiscountableQty
		{
			get
			{
				return this._DiscountableQty;
			}
			set
			{
				this._DiscountableQty = value;
			}
		}
		#endregion
		#region DiscountAmt
		public abstract class discountAmt : PX.Data.BQL.BqlDecimal.Field<discountAmt> { }
		protected Decimal? _DiscountAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscountAmt
		{
			get
			{
				return this._DiscountAmt;
			}
			set
			{
				this._DiscountAmt = value;
			}
		}
		#endregion
		#region CuryDiscountAmt
		public abstract class curyDiscountAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscountAmt> { }
		protected Decimal? _CuryDiscountAmt;
		[PXDBCurrency(typeof(SOOrderDiscountDetail.curyInfoID), typeof(SOOrderDiscountDetail.discountAmt))]
		[PXUIEnabled(typeof(Where<SOOrderDiscountDetail.type, Equal<DiscountType.DocumentDiscount>, Or<SOOrderDiscountDetail.type, Equal<DiscountType.ExternalDocumentDiscount>>>))]
		[PXUIField(DisplayName = "Discount Amt.")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryDiscountAmt
		{
			get
			{
				return this._CuryDiscountAmt;
			}
			set
			{
				this._CuryDiscountAmt = value;
			}
		}
		#endregion
		#region DiscountPct
		public abstract class discountPct : PX.Data.BQL.BqlDecimal.Field<discountPct> { }
		protected Decimal? _DiscountPct;
		[PXDBDecimal(6)]
		[PXUIEnabled(typeof(Where<SOOrderDiscountDetail.type, Equal<DiscountType.DocumentDiscount>, Or<SOOrderDiscountDetail.type, Equal<DiscountType.ExternalDocumentDiscount>>>))]
		[PXUIField(DisplayName = "Discount Percent")]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? DiscountPct
		{
			get
			{
				return this._DiscountPct;
			}
			set
			{
				this._DiscountPct = value;
			}
		}
		#endregion
		#region FreeItemID
		public abstract class freeItemID : PX.Data.BQL.BqlInt.Field<freeItemID> { }
		protected Int32? _FreeItemID;
		[Inventory(DisplayName = "Free Item", Enabled = false)]
		[PXForeignReference(typeof(FK.FreeInventoryItem))]
		public virtual Int32? FreeItemID
		{
			get
			{
				return this._FreeItemID;
			}
			set
			{
				this._FreeItemID = value;
			}
		}
		#endregion
		#region FreeItemQty
		public abstract class freeItemQty : PX.Data.BQL.BqlDecimal.Field<freeItemQty> { }
		protected Decimal? _FreeItemQty;
		[PXDBQuantity(MinValue = 0)]
		[PXUIField(DisplayName = "Free Item Qty.", Enabled = false)]
		public virtual Decimal? FreeItemQty
		{
			get
			{
				return this._FreeItemQty;
			}
			set
			{
				this._FreeItemQty = value;
			}
		}
		#endregion
        #region IsManual
        public abstract class isManual : PX.Data.BQL.BqlBool.Field<isManual> { }
        protected Boolean? _IsManual;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Discount", Enabled = false)]
        public virtual Boolean? IsManual
        {
            get
            {
                return this._IsManual;
            }
            set
            {
                this._IsManual = value;
            }
        }
		#endregion
		#region IsOrigDocDiscount
		public abstract class isOrigDocDiscount : PX.Data.BQL.BqlBool.Field<isOrigDocDiscount> { }
		protected Boolean? _IsOrigDocDiscount;
		[PXBool()]
		[PXFormula(typeof(False))]
		public virtual Boolean? IsOrigDocDiscount
		{
			get
			{
				return this._IsOrigDocDiscount;
			}
			set
			{
				this._IsOrigDocDiscount = value;
			}
		}
		#endregion
		#region ExtDiscCode
		public abstract class extDiscCode : PX.Data.BQL.BqlString.Field<extDiscCode> { }
		protected String _ExtDiscCode;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "External Discount Code")]
		public virtual String ExtDiscCode
		{
			get
			{
				return this._ExtDiscCode;
			}
			set
			{
				this._ExtDiscCode = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		[PXDefault(typeof(Search<DiscountSequence.description, Where<DiscountSequence.discountID, Equal<Current<SOOrderDiscountDetail.discountID>>, And<DiscountSequence.discountSequenceID, Equal<Current<SOOrderDiscountDetail.discountSequenceID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion

		#region System Columns
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
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
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
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
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion
	}
}
