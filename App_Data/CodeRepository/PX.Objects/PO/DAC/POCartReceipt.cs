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
	[PXCacheName(Messages.POCartReceipt, PXDacType.Details)]
	public class POCartReceipt : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POCartReceipt>.By<siteID, cartID>
		{
			public static POCartReceipt Find(PXGraph graph, int? siteID, int? cartID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, siteID, cartID, options);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<POCartReceipt>.By<siteID> { }
			public class Cart : INCart.PK.ForeignKeyOf<POCartReceipt>.By<siteID, cartID> { }
			public class Receipt : POReceipt.PK.ForeignKeyOf<POCartReceipt>.By<receiptType, receiptNbr> { }
			//todo AC-169735 public class Transfer : INRegister.PK.ForeignKeyOf<POCartReceipt>.By<INRegister.docType, transferNbr> { }
		}
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
		#region ReceiptType
		[PXDBString(2, IsFixed = true)]
		[PXDefault]
		public virtual string ReceiptType { get; set; }
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
		#endregion
		#region ReceiptNbr
		[PXDBString(15, IsUnicode = true)]
		[PXDefault]
		[PXParent(typeof(FK.Receipt))]
		public virtual String ReceiptNbr { get; set; }
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		#endregion
		#region TransferNbr
		[PXDBString(15, IsUnicode = true)]
		[PXParent(typeof(Select<INRegister, Where<INRegister.refNbr, Equal<Current<transferNbr>>, And<INRegister.docType, Equal<INDocType.transfer>>>>))]
		public virtual String TransferNbr { get; set; }
		public abstract class transferNbr : PX.Data.BQL.BqlString.Field<transferNbr> { }
		#endregion
		#region tstamp
		[PXDBTimestamp]
		public virtual Byte[] tstamp { get; set; }
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
	}
}
