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
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt;

namespace PX.Objects.SO
{
	[Serializable]
	[PXHidden]
	[PXProjection(typeof(Select<AR.ARTran>), Persistent = false)]
	public class ARTranAsSplit : IBqlTable, IN.ILSDetail
	{
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(AR.ARTran.tranType))]
		public virtual string TranType { get; set; }
		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(AR.ARTran.refNbr))]
		public virtual string RefNbr { get; set; }
		#endregion

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		[PXDBInt(IsKey = true, BqlField = typeof(AR.ARTran.lineNbr))]
		[PXParent(typeof(Select<AR.ARTran, Where<AR.ARTran.tranType, Equal<Current<ARTranAsSplit.tranType>>, And<AR.ARTran.refNbr, Equal<Current<ARTranAsSplit.refNbr>>,
			And<AR.ARTran.lineNbr, Equal<Current<ARTranAsSplit.lineNbr>>>>>>))]
		public virtual int? LineNbr { get; set; }
		#endregion

		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }

		[PXInt(IsKey = true)]
		[PXFormula(typeof(int1))]
		public virtual int? SplitLineNbr { get; set; }
		#endregion

		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }

		[PXDBString(2, IsFixed = true, BqlField = typeof(AR.ARTran.lineType))]
		public virtual string LineType { get; set; }
		#endregion

		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

		[PXDBDate(BqlField = typeof(AR.ARTran.tranDate))]
		public virtual DateTime? TranDate { get; set; }
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		[IN.NonStockNonKitCrossItem(IN.INPrimaryAlternateType.CPN, Messages.CannotAddNonStockKitDirectly, typeof(AR.ARTran.sOOrderNbr),
			typeof(FeaturesSet.advancedSOInvoices), BqlField = typeof(AR.ARTran.inventoryID))]
		public virtual int? InventoryID { get; set; }
		#endregion

		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

		[IN.SubItem(BqlField = typeof(AR.ARTran.subItemID))]
		public virtual int? SubItemID { get; set; }
		#endregion

		#region IsStockItem
		public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }

		[PXBool]
		[PXFormula(typeof(Selector<ARTranAsSplit.inventoryID, IN.InventoryItem.stkItem>))]
		public bool? IsStockItem { get; set; }
		#endregion

		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

		[PXShort]
		[PXDBCalced(typeof(Switch<Case<Where<AR.ARTran.qty, Less<decimal0>>, Minus<AR.ARTran.invtMult>>, AR.ARTran.invtMult>), typeof(short))]
		public virtual short? InvtMult { get; set; }
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		[IN.SiteAvail(typeof(ARTranAsSplit.inventoryID), typeof(ARTranAsSplit.subItemID), typeof(CostCenter.freeStock), BqlField = typeof(AR.ARTran.siteID), DocumentBranchType = typeof(AR.ARInvoice.branchID))]
		public virtual int? SiteID { get; set; }
		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

		[IN.Location(typeof(siteID), BqlField = typeof(AR.ARTran.locationID))]
		public virtual int? LocationID { get; set; }
		#endregion

		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

		[IN.INUnit(typeof(ARTranAsSplit.inventoryID), BqlField = typeof(AR.ARTran.uOM))]
		public virtual string UOM { get; set; }
		#endregion

		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

		[IN.PXQuantity(typeof(ARTranAsSplit.uOM), typeof(ARTranAsSplit.baseQty))]
		[PXDBCalced(typeof(Switch<Case<Where<AR.ARTran.qty, Less<decimal0>>, Minus<AR.ARTran.qty>>, AR.ARTran.qty>), typeof(decimal))]
		public virtual decimal? Qty { get; set; }
		#endregion

		#region BaseQty
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

		[IN.PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<AR.ARTran.qty, Less<decimal0>>, Minus<AR.ARTran.baseQty>>, AR.ARTran.baseQty>), typeof(decimal))]
		public virtual decimal? BaseQty { get; set; }
		#endregion

		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

		[SOInvoiceLineSplittingExtension.ARLotSerialNbr(typeof(ARTranAsSplit.inventoryID), typeof(ARTranAsSplit.subItemID), typeof(ARTranAsSplit.locationID),
			PersistingCheck = PXPersistingCheck.Nothing, BqlField = typeof(AR.ARTran.lotSerialNbr))]
		public virtual string LotSerialNbr { get; set; }
		#endregion

		#region LotSerClassID
		public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }

		[PXString(10, IsUnicode = true)]
		public virtual string LotSerClassID { get; set; }
		#endregion

		#region AssignedNbr
		public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr> { }

		[PXString(30, IsUnicode = true)]
		public virtual string AssignedNbr { get; set; }
		#endregion

		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

		[PXDBDate(BqlField = typeof(AR.ARTran.expireDate))]
		public virtual DateTime? ExpireDate { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		[PXDBInt(BqlField = typeof(AR.ARTran.projectID))]
		public virtual int? ProjectID { get; set; }
		#endregion

		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

		[PXDBInt(BqlField = typeof(AR.ARTran.taskID))]
		public virtual int? TaskID { get; set; }
		#endregion

		bool? IN.ILSMaster.IsIntercompany => false;

		public static ARTranAsSplit FromARTran(AR.ARTran item)
		{
			ARTranAsSplit ret = new ARTranAsSplit
			{
				TranType = item.TranType,
				RefNbr = item.RefNbr,
				LineNbr = item.LineNbr,
				SplitLineNbr = 1,
				TranDate = item.TranDate,
				InventoryID = item.InventoryID,
				SubItemID = item.SubItemID,
				InvtMult = item.InvtMult,
				SiteID = item.SiteID,
				LocationID = item.LocationID,
				UOM = item.UOM,
				Qty = item.Qty,
				BaseQty = item.BaseQty,
				LotSerialNbr = item.LotSerialNbr,
				ExpireDate = item.ExpireDate,
				ProjectID = item.ProjectID,
				TaskID = item.TaskID
			};

			return ret;
		}

	}
}
