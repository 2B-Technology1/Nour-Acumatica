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

namespace PX.Objects.IN.AffectedAvailability
{
	// Used in IN622000 report (Sales Allocations Affected by Inventory Adjustments).
	/// <exclude/>
	[PXCacheName(Messages.AdjustmentTranBySiteLotSerial)]
	[PXProjection(typeof(Select5<INTran,
	InnerJoin<InventoryItem, On<INTran.FK.InventoryItem>,
	InnerJoin<INItemClass, On<InventoryItem.FK.ItemClass>,
	InnerJoin<INLotSerClass, On<InventoryItem.FK.LotSerialClass>,
	InnerJoin<INSiteLotSerial, On<INSiteLotSerial.inventoryID, Equal<INTran.inventoryID>, And<INSiteLotSerial.siteID, Equal<INTran.siteID>, And<INSiteLotSerial.lotSerialNbr, Equal<INTran.lotSerialNbr>>>>>>>>,
	Where<INTran.docType, Equal<INDocType.adjustment>,
		And<INLotSerClass.lotSerTrack, In3<INLotSerTrack.serialNumbered, INLotSerTrack.lotNumbered>,
				And<INLotSerClass.lotSerAssign, Equal<INLotSerAssign.whenReceived>>>>,
	Aggregate<GroupBy<INTran.docType, GroupBy<INTran.refNbr, GroupBy<INTran.inventoryID, GroupBy<INTran.siteID, GroupBy<INTran.lotSerialNbr, Max<INSiteLotSerial.qtyHardAvail, Sum<INTran.baseQty>>>>>>>>>))]
	public class AdjustmentTranBySiteLotSerial : IBqlTable
	{
		#region DocType
		[PXDBString(1, IsFixed = true, BqlField = typeof(INTran.docType), IsKey = true)]
		public virtual String DocType { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion
		#region RefNbr
		[PXDBString(15, IsUnicode = true, BqlField = typeof(INTran.refNbr), IsKey = true)]
		[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<INDocType.adjustment>>>))]
		public virtual String RefNbr { get; set; }
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion
		#region InventoryID
		[StockItem(BqlField = typeof(INTran.inventoryID), IsKey = true)]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SiteID
		[Site(BqlField = typeof(INTran.siteID), IsKey = true)]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region LotSerialNbr
		[PXDBString(INLotSerialStatusByCostCenter.lotSerialNbr.Length, IsUnicode = true, InputMask = "", BqlField = typeof(INTran.lotSerialNbr), IsKey = true)]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region BaseUnit
		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa", BqlField = typeof(InventoryItem.baseUnit))]
		public virtual String BaseUnit { get; set; }
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
		#endregion
		#region QtyHardAvail
		[PXDBQuantity(BqlField = typeof(INSiteLotSerial.qtyHardAvail))]
		public virtual Decimal? QtyHardAvail { get; set; }
		public abstract class qtyHardAvail : PX.Data.BQL.BqlDecimal.Field<qtyHardAvail> { }
		#endregion
		#region QtyAdjusted
		[PXDBQuantity(BqlField = typeof(INTran.baseQty))]
		public virtual Decimal? QtyAdjusted { get; set; }
		public abstract class qtyAdjusted : PX.Data.BQL.BqlDecimal.Field<qtyAdjusted> { }
		#endregion
	}
}
