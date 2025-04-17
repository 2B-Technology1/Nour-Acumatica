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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	/// <summary>
	/// This is a readonly DAC with limited fields of the <see cref="SOLine"/> DAC.
	/// </summary>
	[PXHidden]
	[PXProjection(typeof(SelectFrom<SOLine>), Persistent = false)]
	public class SOLineShort: IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOLineShort>.By<orderType, orderNbr, lineNbr>
		{
			public static SOLineShort Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr) => FindBy(graph, orderType, orderNbr, lineNbr);
		}
		public static class FK
		{
			public class Order : SOOrder.PK.ForeignKeyOf<SOLineShort>.By<orderType, orderNbr> { }
			public class SiteStatus : INSiteStatus.PK.ForeignKeyOf<SOLineShort>.By<inventoryID, subItemID, siteID> { }
			public class SiteStatusByCostCenter : INSiteStatusByCostCenter.PK.ForeignKeyOf<SOLineShort>.By<inventoryID, subItemID, siteID, costCenterID> { }
		}
		#endregion

		#region OrderType
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLine.orderType))]
		public virtual string OrderType { get; set; }
		public abstract class orderType : BqlString.Field<orderType> { }
		#endregion
		#region OrderNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLine.orderNbr))]
		public virtual string OrderNbr { get; set; }
		public abstract class orderNbr : BqlString.Field<orderNbr> { }
		#endregion
		#region LineNbr
		[PXDBInt(IsKey = true, BqlField = typeof(SOLine.lineNbr))]
		public virtual int? LineNbr { get; set; }
		public abstract class lineNbr : BqlInt.Field<lineNbr> { }
		#endregion

		#region InventoryID
		[PXDBInt(BqlField = typeof(SOLine.inventoryID))]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion

		#region SubItemID
		[PXDBInt(BqlField = typeof(SOLine.subItemID))]
		public virtual int? SubItemID { get; set; }
		public abstract class subItemID : BqlInt.Field<subItemID> { }
		#endregion

		#region SiteID
		[PXDBInt(BqlField = typeof(SOLine.siteID))]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region UOM
		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa", BqlField = typeof(SOLine.uOM))]
		public virtual string UOM { get; set; }
		public abstract class uOM : BqlString.Field<uOM> { }
		#endregion

		#region CostCenterID
		[PXDBInt(BqlField = typeof(SOLine.costCenterID))]
		public virtual int? CostCenterID { get; set; }
		public abstract class costCenterID : BqlInt.Field<costCenterID> { }
		#endregion
	}
}
