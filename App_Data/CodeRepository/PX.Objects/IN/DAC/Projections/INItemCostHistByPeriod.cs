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

using PX.Objects.GL.FinPeriods;

namespace PX.Objects.IN
{
	using System;
	using PX.Data;
    using PX.Data.ReferentialIntegrity.Attributes;
    using PX.Objects.GL;
	using PX.Objects.CS;

    [System.SerializableAttribute()]
	[PXCacheName(Messages.INItemCostHistByPeriod)]
	[PXProjection(typeof(Select5<INItemCostHist,
		InnerJoin<MasterFinPeriod, On<MasterFinPeriod.finPeriodID, GreaterEqual<INItemCostHist.finPeriodID>>>,
			Aggregate<GroupBy<INItemCostHist.inventoryID,
				GroupBy<INItemCostHist.costSubItemID,
				GroupBy<INItemCostHist.costSiteID,
				GroupBy<INItemCostHist.accountID,
				GroupBy<INItemCostHist.subID,
				Max<INItemCostHist.finPeriodID,
				GroupBy<MasterFinPeriod.finPeriodID>>>>>>>>>))]
	public partial class INItemCostHistByPeriod : PX.Data.IBqlTable
	{
        #region Keys
        public class PK : PrimaryKeyOf<INItemCostHistByPeriod>.By<inventoryID, costSubItemID, costSiteID, accountID, subID, finPeriodID>
        {
            public static INItemCostHistByPeriod Find(PXGraph graph, int? inventoryID, int? costSubItemID, int? costSiteID, int? accountID, int? subID, string finPeriodID, PKFindOptions options = PKFindOptions.None)
                => FindBy(graph, inventoryID, costSubItemID, costSiteID, accountID, subID, finPeriodID, options);
        }
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemCostHistByPeriod>.By<inventoryID> { }
			public class CostSubItem : INSubItem.PK.ForeignKeyOf<INItemCostHistByPeriod>.By<costSubItemID> { }
			public class CostSite : INSite.PK.ForeignKeyOf<INItemCostHistByPeriod>.By<costSiteID> { }
			public class Subaccount : GL.Sub.PK.ForeignKeyOf<INItemCostHistByPeriod>.By<subID> { }
			public class Site : INSite.PK.ForeignKeyOf<INItemCostHistByPeriod>.By<siteID> { }
			//todo public class FinPeriod : GL.FinPeriods.TableDefinition.FinPeriod.PK.ForeignKeyOf<INItemCostHistByPeriod>.By<finPeriodID> { }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(IsKey = true, BqlField = typeof(INItemCostHist.inventoryID))]
		[PXDefault()]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region CostSubItemID
		public abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
		protected Int32? _CostSubItemID;
		[SubItem(IsKey = true, BqlField = typeof(INItemCostHist.costSubItemID))]
		[PXDefault()]
		public virtual Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
		protected Int32? _CostSiteID;
		[Site(IsKey = true, BqlField = typeof(INItemCostHist.costSiteID))]
		[PXDefault()]
		public virtual Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(IsKey = true, BqlField = typeof(INItemCostHist.accountID))]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount(IsKey = true, BqlField = typeof(INItemCostHist.subID))]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(true, BqlField = typeof(INItemCostHist.siteID))]
		[PXDefault()]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion


		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		protected String _LastActivityPeriod;
		[FinPeriodID(BqlField = typeof(INItemCostHist.finPeriodID))]
		public virtual String LastActivityPeriod
		{
			get
			{
				return this._LastActivityPeriod;
			}
			set
			{
				this._LastActivityPeriod = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
		[PXUIField(DisplayName = "Fin. Period")]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
	}
}
