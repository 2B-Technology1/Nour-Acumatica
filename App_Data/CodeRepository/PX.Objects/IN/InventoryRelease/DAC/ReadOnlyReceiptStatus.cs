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
using PX.Objects.GL;

namespace PX.Objects.IN.InventoryRelease.DAC
{
	[PXHidden]
	public class ReadOnlyReceiptStatus : INReceiptStatus
	{
		#region ReceiptID
		[PXDBLong(IsKey = true)]
		[PXDefault]
		public override long? ReceiptID
		{
			get => _ReceiptID;
			set => _ReceiptID = value;
		}
		public new abstract class receiptID : BqlLong.Field<receiptID> { }
		#endregion
		#region InventoryID
		[StockItem(IsKey = true)]
		[PXDefault]
		public override int? InventoryID
		{
			get => _InventoryID;
			set => _InventoryID = value;
		}
		public new abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region CostSubItemID
		public new abstract class costSubItemID : BqlInt.Field<costSubItemID> { }
		#endregion
		#region CostSiteID
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? CostSiteID
		{
			get => _CostSiteID;
			set => _CostSiteID = value;
		}
		public new abstract class costSiteID : BqlInt.Field<costSiteID> { }
		#endregion
		#region AccountID

		[Account(IsKey = true)]
		[PXDefault]
		public override int? AccountID
		{
			get => _AccountID;
			set => _AccountID = value;
		}
		public new abstract class accountID : BqlInt.Field<accountID> { }
		#endregion
		#region SubID

		[SubAccount(typeof(accountID), IsKey = true)]
		[PXDefault]
		public override int? SubID
		{
			get => _SubID;
			set => _SubID = value;
		}
		public new abstract class subID : BqlInt.Field<subID> { }
		#endregion
		#region DocType
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDefault]
		public override string DocType
		{
			get;
			set;
		}
		public new abstract class docType : BqlString.Field<docType> { }
		#endregion
		#region ReceiptNbr
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public override string ReceiptNbr
		{
			get => _ReceiptNbr;
			set => _ReceiptNbr = value;
		}
		public new abstract class receiptNbr : BqlString.Field<receiptNbr> { }
		#endregion
		#region ReceiptDate
		public new abstract class receiptDate : BqlDateTime.Field<receiptDate> { }
		#endregion
		#region OrigQty
		public new abstract class origQty : BqlDecimal.Field<origQty> { }
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : BqlDecimal.Field<qtyOnHand> { }
		#endregion
	}
}
