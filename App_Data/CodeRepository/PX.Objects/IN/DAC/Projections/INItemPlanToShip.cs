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
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System;

namespace PX.Objects.IN
{
	[PXHidden]
	[PXProjection(typeof(
		SelectFrom<INItemPlan>
			.InnerJoin<INPlanType>
				.On<INItemPlan.FK.PlanType
				.And<INPlanType.isDemand.IsEqual<True>>
				.And<INPlanType.isFixed.IsNotEqual<True>>
				.And<INPlanType.isForDate.IsEqual<True>>>
		.Where<INItemPlan.hold.IsNotEqual<True>
			.And<INItemPlan.planQty.IsGreater<decimal0>>
			.And<INItemPlan.fixedSource.IsNull.Or<INItemPlan.fixedSource.IsNotEqual<INReplenishmentSource.transfer>>>>),
		Persistent = false)]
	public class INItemPlanToShip: IBqlTable
	{
		#region Keys
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INItemPlanToShip>.By<siteID> { }
		}
		#endregion

		#region PlanID
		[PXDBLong(IsKey = true, BqlField = typeof(INItemPlan.planID))]
		public virtual long? PlanID { get; set; }
		public abstract class planID : BqlLong.Field<planID> { }
		#endregion

		#region RefEntityType
		[PXDBString(255, IsUnicode = false, BqlField = typeof(INItemPlan.refEntityType))]
		public string RefEntityType { get; set; }
		public abstract class refEntityType : BqlString.Field<refEntityType> { }
		#endregion

		#region RefNoteID
		[PXDBGuid(BqlField = typeof(INItemPlan.refNoteID))]
		public virtual Guid? RefNoteID { get; set; }
		public abstract class refNoteID : BqlGuid.Field<refNoteID> { }
		#endregion

		#region SiteID
		[PXDBInt(BqlField = typeof(INItemPlan.siteID))]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region InventoryID
		[PXDBInt(BqlField = typeof(INItemPlan.inventoryID))]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion

		#region PlanType
		[PXDBString(2, IsFixed = true, BqlField = typeof(INItemPlan.planType))]
		public virtual string PlanType { get; set; }
		public abstract class planType : BqlString.Field<planType> { }
		#endregion

		#region PlanDate
		[PXDBDate(BqlField = typeof(INItemPlan.planDate))]
		public virtual DateTime? PlanDate { get; set; }
		public abstract class planDate : BqlDateTime.Field<planDate> { }
		#endregion

		#region PlanQty
		[PXDBDecimal(6, BqlField = typeof(INItemPlan.planQty))]
		public virtual decimal? PlanQty { get; set; }
		public abstract class planQty : BqlDecimal.Field<planQty> { }
		#endregion

		#region Reverse
		[PXDBBool(BqlField = typeof(INItemPlan.reverse))]
		public virtual Boolean? Reverse { get; set; }
		public abstract class reverse : BqlBool.Field<reverse> { }
		#endregion

		#region InclQtySOBackOrdered
		[PXDBShort(BqlField = typeof(INPlanType.inclQtySOBackOrdered))]
		public virtual short? InclQtySOBackOrdered { get; set; }
		public abstract class inclQtySOBackOrdered : BqlShort.Field<inclQtySOBackOrdered> { }
		#endregion
	}
}
