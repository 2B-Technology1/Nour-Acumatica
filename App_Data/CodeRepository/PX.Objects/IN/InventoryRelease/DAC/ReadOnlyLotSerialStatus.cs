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
using PX.Data.BQL;

namespace PX.Objects.IN.InventoryRelease.DAC
{
	[PXHidden]
	public partial class ReadOnlyLotSerialStatus : INLotSerialStatus
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(INLotSerialStatusByCostCenter.inventoryID))]
		[PXDefault()]
		public override Int32? InventoryID
		{
			get => base.InventoryID;
			set => base.InventoryID = value;
		}
		#endregion
		#region SubItemID
		public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		[SubItem(IsKey = true, BqlField = typeof(INLotSerialStatusByCostCenter.subItemID))]
		public override Int32? SubItemID
		{
			get => base.SubItemID;
			set => base.SubItemID = value;
		}
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(INLotSerialStatusByCostCenter.siteID))]
		public override Int32? SiteID
		{
			get => base.SiteID;
			set => base.SiteID = value;
		}
		#endregion
		#region LocationID
		public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		[IN.Location(typeof(siteID), IsKey = true, ValidComboRequired = false, BqlField = typeof(INLotSerialStatusByCostCenter.locationID))]
		[PXDefault()]
		public override Int32? LocationID
		{
			get => base.LocationID;
			set => base.LocationID = value;
		}
		#endregion
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		[PXDBString(100, IsUnicode = true, IsKey = true, BqlField = typeof(INLotSerialStatusByCostCenter.lotSerialNbr))]
		[PXDefault()]
		public override String LotSerialNbr
		{
			get => base.LotSerialNbr;
			set => base.LotSerialNbr = value;
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		[PXDBQuantity(BqlField = typeof(INLotSerialStatusByCostCenter.qtyOnHand))]
		public override Decimal? QtyOnHand
		{
			get => base.QtyOnHand;
			set => base.QtyOnHand = value;
		}
		#endregion
		#region LotSerTrack
		public new abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
		[PXDBString(1, IsFixed = true, BqlField = typeof(INLotSerialStatusByCostCenter.lotSerTrack))]
		public override String LotSerTrack
		{
			get => base.LotSerTrack;
			set => base.LotSerTrack = value;
		}
		#endregion
		#region ExpireDate
		public new abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected new DateTime? _ExpireDate;
		[PXDBDate(BqlField = typeof(INLotSerialStatusByCostCenter.expireDate))]
		[PXUIField(DisplayName = "Expiry Date")]
		public override DateTime? ExpireDate
		{
			get => base.ExpireDate;
			set => base.ExpireDate = value;
		}
		#endregion
		#region ReceiptDate
		public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
		[PXDBDate(BqlField = typeof(INLotSerialStatusByCostCenter.receiptDate))]
		public override DateTime? ReceiptDate
		{
			get => base.ReceiptDate;
			set => base.ReceiptDate = value;
		}
		#endregion
	}
}
