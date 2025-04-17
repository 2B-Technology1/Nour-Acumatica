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

namespace PX.Objects.SO
{
	[PXCacheName(Messages.SOShipmentSplitToCartSplitLink, PXDacType.Details)]
	public class SOShipmentSplitToCartSplitLink : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOShipmentSplitToCartSplitLink>.By<shipmentNbr, shipmentLineNbr, shipmentSplitLineNbr, siteID, cartID, cartSplitLineNbr>
		{
			public static SOShipmentSplitToCartSplitLink Find(PXGraph graph, string shipmentNbr, int? shipmentLineNbr, int? shipmentSplitLineNbr, int? siteID, int? cartID, int? cartSplitLineNbr, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, shipmentNbr, shipmentLineNbr, shipmentSplitLineNbr, siteID, cartID, cartSplitLineNbr, options);
		}
		public static class FK
		{
			public class Shipment : SOShipment.PK.ForeignKeyOf<SOShipmentSplitToCartSplitLink>.By<shipmentNbr> { }
			public class ShipmentLine : SOShipLine.PK.ForeignKeyOf<SOShipmentSplitToCartSplitLink>.By<shipmentNbr, shipmentLineNbr> { }
			public class ShipmentSplitLine : SOShipLineSplit.PK.ForeignKeyOf<SOShipmentSplitToCartSplitLink>.By<shipmentNbr, shipmentLineNbr, shipmentSplitLineNbr> { }
			public class Site : INSite.PK.ForeignKeyOf<SOShipmentSplitToCartSplitLink>.By<siteID> { }
			public class Cart : INCart.PK.ForeignKeyOf<SOShipmentSplitToCartSplitLink>.By<siteID, cartID> { }
			public class CartSplit : INCartSplit.PK.ForeignKeyOf<SOShipmentSplitToCartSplitLink>.By<siteID, cartID, cartSplitLineNbr> { }
		}
		#endregion

		#region ShipmentNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		public virtual String ShipmentNbr { get; set; }
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		#endregion
		#region ShipmentLineNbr
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual Int32? ShipmentLineNbr { get; set; }
		public abstract class shipmentLineNbr : PX.Data.BQL.BqlInt.Field<shipmentLineNbr> { }
		#endregion
		#region ShipmentSplitLineNbr
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXParent(typeof(FK.ShipmentSplitLine))]
		public virtual Int32? ShipmentSplitLineNbr { get; set; }
		public abstract class shipmentSplitLineNbr : PX.Data.BQL.BqlInt.Field<shipmentSplitLineNbr> { }
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

		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion
		#region CreatedDateTime
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#region tstamp
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp { get; set; }
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
	}
}
