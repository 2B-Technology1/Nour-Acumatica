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

using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.IN.DAC
{
	[PXCacheName(Messages.RegisterCartLine, PXDacType.Details)]
	public class INRegisterCartLine : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INRegisterCartLine>.By<siteID, cartID, docType, refNbr, lineNbr> { }
		public static class FK
		{
			public class RegisterCart : INRegisterCart.PK.ForeignKeyOf<INRegisterCartLine>.By<siteID, cartID, docType, refNbr> { }
			public class Register : INRegister.PK.ForeignKeyOf<INRegisterCartLine>.By<docType, refNbr> { }
			public class Tran : INTran.PK.ForeignKeyOf<INRegisterCartLine>.By<docType, refNbr, lineNbr> { }
			public class Site : INSite.PK.ForeignKeyOf<INRegisterCartLine>.By<siteID> { }
			public class Cart : INCart.PK.ForeignKeyOf<INRegisterCartLine>.By<siteID, cartID> { }
			public class CartSplit : INCartSplit.PK.ForeignKeyOf<INRegisterCartLine>.By<siteID, cartID, cartSplitLineNbr> { }
		}
		#endregion

		#region SiteID
		[Site(IsKey = true, Visible = false)]
		[PXDefault(typeof(INCart.siteID))]
		[PXParent(typeof(FK.Site))]
		public int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region CartID
		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(Search<INCart.cartID, Where<INCart.active, Equal<True>>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
		[PXDefault(typeof(INCart.cartID))]
		[PXParent(typeof(FK.Cart))]
		public int? CartID { get; set; }
		public abstract class cartID : BqlInt.Field<cartID> { }
		#endregion

		#region DocType
		[PXUIField(DisplayName = INRegister.docType.DisplayName)]
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(INRegister.docType))]
		public virtual string DocType { get; set; }
		public abstract class docType : BqlString.Field<docType> { }
		#endregion

		#region RefNbr
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDBDefault(typeof(INRegister.refNbr))]
		[PXParent(typeof(FK.Register))]
		[PXUIField(DisplayName = INRegister.refNbr.DisplayName)]
		public virtual string RefNbr { get; set; }
		public abstract class refNbr : BqlString.Field<refNbr> { }
		#endregion

		#region CartSplitLineNbr
		[PXDBInt]
		[PXDefault]
		[PXForeignReference(typeof(FK.CartSplit))]
		public virtual int? CartSplitLineNbr { get; set; }
		public abstract class cartSplitLineNbr : BqlInt.Field<cartSplitLineNbr> { }
		#endregion

		#region LineNbr
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(INTran.lineNbr))]
		[PXForeignReference(typeof(FK.Tran))]
		public virtual int? LineNbr { get; set; }
		public abstract class lineNbr : BqlInt.Field<lineNbr> { }
		#endregion

		#region CartQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity", Enabled = false)]
		public virtual decimal? Qty { get; set; }
		public abstract class qty : BqlDecimal.Field<qty> { }
		#endregion

		#region tstamp
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
