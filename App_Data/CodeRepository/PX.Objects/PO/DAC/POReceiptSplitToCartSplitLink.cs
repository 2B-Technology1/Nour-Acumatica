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

using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.PO
{
	[PXCacheName(Messages.POReceiptSplitToCartSplitLink, PXDacType.Details)]
	public class POReceiptSplitToCartSplitLink : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POReceiptSplitToCartSplitLink>.By<receiptType, receiptNbr, receiptLineNbr, receiptSplitLineNbr, siteID, cartID, cartSplitLineNbr>
		{
			public static POReceiptSplitToCartSplitLink Find(PXGraph graph, string receiptType, string receiptNbr, int? receiptLineNbr, int? receiptSplitLineNbr,
				int? siteID, int? cartID, int? cartSplitLineNbr, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, receiptType, receiptNbr, receiptLineNbr, receiptSplitLineNbr, siteID, cartID, cartSplitLineNbr, options);
		}
		public static class FK
		{
			public class Receipt : POReceipt.PK.ForeignKeyOf<POReceiptSplitToCartSplitLink>.By<receiptType, receiptNbr> { }
			public class ReceiptLine : POReceiptLine.PK.ForeignKeyOf<POReceiptSplitToCartSplitLink>.By<receiptType, receiptNbr, receiptLineNbr> { }
			public class ReceiptLineSplit : Objects.PO.POReceiptLineSplit.PK.ForeignKeyOf<POReceiptSplitToCartSplitLink>.By<receiptType, receiptNbr, receiptLineNbr, receiptSplitLineNbr> { }
			public class Site : INSite.PK.ForeignKeyOf<POReceiptSplitToCartSplitLink>.By<siteID> { }
			public class Cart : INCart.PK.ForeignKeyOf<POReceiptSplitToCartSplitLink>.By<siteID, cartID> { }
			public class CartSplit : INCartSplit.PK.ForeignKeyOf<POReceiptSplitToCartSplitLink>.By<siteID, cartID, cartSplitLineNbr> { }
		}
		#endregion
		#region ReceiptType
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault]
		public virtual string ReceiptType { get; set; }
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
		#endregion
		#region ReceiptNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		public virtual String ReceiptNbr { get; set; }
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		#endregion
		#region ReceiptLineNbr
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual Int32? ReceiptLineNbr { get; set; }
		public abstract class receiptLineNbr : PX.Data.BQL.BqlInt.Field<receiptLineNbr> { }
		#endregion
		#region ReceiptSplitLineNbr
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXParent(typeof(FK.ReceiptLineSplit))]
		public virtual Int32? ReceiptSplitLineNbr { get; set; }
		public abstract class receiptSplitLineNbr : PX.Data.BQL.BqlInt.Field<receiptSplitLineNbr> { }
		#endregion

		#region SiteID
		[Site(IsKey = true, Visible = false)]
		[PXParent(typeof(FK.Site))]
		public int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region CartID
		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(Search<INCart.cartID, Where<INCart.active, Equal<True>>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
		[PXParent(typeof(FK.Cart))]
		public int? CartID { get; set; }
		public abstract class cartID : PX.Data.BQL.BqlInt.Field<cartID> { }
		#endregion
		#region CartSplitLineNbr
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXParent(typeof(FK.CartSplit))]
		public virtual Int32? CartSplitLineNbr { get; set; }
		public abstract class cartSplitLineNbr : PX.Data.BQL.BqlInt.Field<cartSplitLineNbr> { }
		#endregion

		#region Qty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity", Enabled = false)]
		public virtual Decimal? Qty { get; set; }
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		#endregion
	}
}
