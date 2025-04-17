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
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System;

namespace PX.Objects.IN.DAC.Projections
{
	/// <exclude />
	[PXCacheName(Messages.INSiteCostStatusByCostLayerType)]
	[PXProjection(typeof(SelectFrom<INCostStatus>
		.InnerJoin<INCostSubItemXRef>.On<INCostSubItemXRef.costSubItemID.IsEqual<INCostStatus.costSubItemID>>
		.InnerJoin<INCostSite>.On<INCostStatus.costSiteID.IsEqual<INCostSite.costSiteID>>
		.LeftJoin<INCostCenter>.On<INCostStatus.costSiteID.IsEqual<INCostCenter.costCenterID>>
		.CrossJoin<CommonSetup>
		.Where<INCostSite.costSiteType.IsIn<NameOf<INCostCenter>, NameOf<INSite>>>
		.AggregateTo<GroupBy<INCostStatus.inventoryID>, GroupBy<INCostSubItemXRef.subItemID>,
			GroupBy<INCostStatus.siteID>, GroupBy<INCostCenter.costLayerType>,
			Sum<INCostStatus.qtyOnHand>, Sum<INCostStatus.totalCost>>))]
	public class INSiteCostStatusByCostLayerType : IBqlTable, ICostStatus
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.inventoryID))]
		[PXDefault()]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(INCostSubItemXRef.subItemID))]
		public virtual Int32? SubItemID
		{
			get;
			set;
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.siteID))]
		public virtual Int32? SiteID
		{
			get;
			set;
		}
		#endregion
		#region CostLayerType
		public abstract class costLayerType : PX.Data.BQL.BqlString.Field<costLayerType> { }
		[PXDBCalced(typeof(IsNull<INCostCenter.costLayerType, CostLayerType.normal>), typeof(string))]
		[PXString(1, IsKey = true)]
		public virtual string CostLayerType
		{
			get;
			set;
		}
		#endregion
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		[PXDBQuantity(BqlField = typeof(INCostStatus.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QtyOnHand
		{
			get;
			set;
		}
		#endregion
		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		[CM.PXDBBaseCury(BqlField = typeof(INCostStatus.totalCost))]
		public virtual Decimal? TotalCost
		{
			get;
			set;
		}
		#endregion
		#region DecPlPrcCst
		public abstract class decPlPrcCst : PX.Data.BQL.BqlShort.Field<decPlPrcCst> { }
		[PXDBShort(BqlField = typeof(CommonSetup.decPlPrcCst))]
		public virtual Int16? DecPlPrcCst
		{
			get;
			set;
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		[PXPriceCost()]
		public virtual Decimal? UnitCost
		{
			[PXDependsOnFields(typeof(qtyOnHand), typeof(totalCost), typeof(decPlPrcCst))]
			get
			{
				return (this.QtyOnHand == null || this.TotalCost == null) ? (decimal?)null :
					(QtyOnHand != 0m) ? Math.Round((decimal)TotalCost / (decimal)QtyOnHand, (int)DecPlPrcCst, MidpointRounding.AwayFromZero) : 0m;
			}
			set
			{
			}
		}
		#endregion
	}
}
