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
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.IN.InventoryRelease.Accumulators.CostStatuses
{
	using Abstraction;
	using static PXDataFieldAssign.AssignBehavior;

	[PXHidden]
	[Accumulator]
	public class OversoldCostStatus : INCostStatus
	{
		#region CostID
		[CostIdentity(typeof(INTranCost.costID))]
		[PXDefault]
		public override long? CostID
		{
			get => _CostID;
			set => _CostID = value;
		}
		public new abstract class costID : BqlLong.Field<costID> { }
		#endregion
		#region InventoryID
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
		[PXDefault]
		public override int? InventoryID
		{
			get => _InventoryID;
			set => _InventoryID = value;
		}
		public new abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region CostSubItemID
		[SubItem(IsKey = true)]
		[PXDefault]
		public override int? CostSubItemID
		{
			get => _CostSubItemID;
			set => _CostSubItemID = value;
		}
		public new abstract class costSubItemID : BqlInt.Field<costSubItemID> { }
		#endregion
		#region CostSiteID
		[PXDBInt(IsKey = true)]
		[CostSiteID]
		[PXDefault]
		public override int? CostSiteID
		{
			get => _CostSiteID;
			set => _CostSiteID = value;
		}
		public new abstract class costSiteID : BqlInt.Field<costSiteID> { }
		#endregion
		#region AccountID
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? AccountID
		{
			get => _AccountID;
			set => _AccountID = value;
		}
		public new abstract class accountID : BqlInt.Field<accountID> { }
		#endregion
		#region SubID
		[SubAccount(IsKey = true)]
		[PXDefault]
		public override int? SubID
		{
			get => _SubID;
			set => _SubID = value;
		}
		public new abstract class subID : BqlInt.Field<subID> { }
		#endregion
		#region LayerType
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(INLayerType.Oversold)]
		public override string LayerType
		{
			get => _LayerType;
			set => _LayerType = value;
		}
		public new abstract class layerType : BqlString.Field<layerType> { }
		#endregion
		#region ValMethod
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		public override string ValMethod
		{
			get => _ValMethod;
			set => _ValMethod = value;
		}
		public new abstract class valMethod : BqlString.Field<valMethod> { }
		#endregion
		#region ReceiptNbr
		[PXDBString(15, IsUnicode = true)]
		[PXDefault("OVERSOLD")]
		public override string ReceiptNbr
		{
			get => _ReceiptNbr;
			set => _ReceiptNbr = value;
		}
		public new abstract class receiptNbr : BqlString.Field<receiptNbr> { }
		#endregion
		#region ReceiptDate
		[PXDBDate]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		public override DateTime? ReceiptDate
		{
			get => _ReceiptDate;
			set => _ReceiptDate = value;
		}
		public new abstract class receiptDate : BqlDateTime.Field<receiptDate> { }
		#endregion
		#region LotSerialNbr
		[PXDBString(100, IsUnicode = true)]
		public override string LotSerialNbr
		{
			get => _LotSerialNbr;
			set => _LotSerialNbr = value;
		}
		public new abstract class lotSerialNbr : BqlString.Field<lotSerialNbr> { }
		#endregion
		#region OrigQty
		public new abstract class origQty : BqlDecimal.Field<origQty> { }
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : BqlDecimal.Field<qtyOnHand> { }
		#endregion
		#region UnitCost
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Coalesce<
			SearchFor<INItemSiteSettings.negativeCost>.
			Where<
				INItemSiteSettings.inventoryID.IsEqual<inventoryID.FromCurrent>.
				And<INItemSiteSettings.siteID.IsEqual<siteID.FromCurrent>>>,
			SearchFor<INItemCost.lastCost>.
			Where<
				INItemCost.inventoryID.IsEqual<inventoryID.FromCurrent>.
				And<INItemCost.curyID.IsEqual<INRegister.branchBaseCuryID.FromCurrent>>>>))]
		public override decimal? UnitCost
		{
			get => _UnitCost;
			set => _UnitCost = value;
		}
		public new abstract class unitCost : BqlDecimal.Field<unitCost> { }
		#endregion

		public class AccumulatorAttribute : CostStatusAccumulatorAttribute
		{
			public AccumulatorAttribute() : base(
				typeof(qtyOnHand),
				typeof(totalCost),
				typeof(inventoryID),
				typeof(costSubItemID),
				typeof(costSiteID),
				typeof(layerType),
				typeof(receiptNbr))
			{ }

			protected override bool PrepareInsert(PXCache cache, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(cache, row, columns))
					return false;

				columns.Update<INCostStatus.valMethod>(Replace);

				return true;
			}
		}
	}
}
