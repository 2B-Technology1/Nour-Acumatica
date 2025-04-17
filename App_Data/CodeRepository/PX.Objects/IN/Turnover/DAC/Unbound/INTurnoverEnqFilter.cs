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
using PX.Objects.GL;
using PX.Objects.GL.Attributes;

namespace PX.Objects.IN.Turnover
{
	[PXCacheName(Messages.INTurnoverEnqFilter)]
	public class INTurnoverEnqFilter : IBqlTable
	{
		#region OrganizationID
		[Organization(false, Required = false)]
		public virtual int? OrganizationID { get; set; }
		public abstract class organizationID : BqlInt.Field<organizationID> { }
		#endregion

		#region BranchID
		[BranchOfOrganization(typeof(organizationID), false)]
		public virtual int? BranchID { get; set; }
		public abstract class branchID : BqlInt.Field<branchID> { }
		#endregion

		#region OrgBAccountID
		[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false, Required = true)]
		public virtual int? OrgBAccountID { get; set; }
		public abstract class orgBAccountID : BqlInt.Field<orgBAccountID> { }
		#endregion

		#region UseMasterCalendar
		[PXDBBool]
		public bool? UseMasterCalendar { get; set; }
		public abstract class useMasterCalendar : BqlBool.Field<useMasterCalendar> { }
		#endregion

		#region FromPeriodID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [The field have PXDBStringAttribute]
		[AnyPeriodFilterable(null, null,
			branchSourceType: typeof(branchID),
			organizationSourceType: typeof(organizationID),
			useMasterCalendarSourceType: typeof(useMasterCalendar),
			redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
		[PXUIField(DisplayName = "From Period", Required = true)]
		public virtual string FromPeriodID { get; set; }
		public abstract class fromPeriodID : BqlString.Field<fromPeriodID> { }
		#endregion

		#region ToPeriodID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [The field have PXDBStringAttribute]
		[AnyPeriodFilterable(null, null,
			branchSourceType: typeof(branchID),
			organizationSourceType: typeof(organizationID),
			useMasterCalendarSourceType: typeof(useMasterCalendar),
			redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
		[PXUIField(DisplayName = "To Period", Required = true)]
		public virtual string ToPeriodID { get; set; }
		public abstract class toPeriodID : BqlString.Field<toPeriodID> { }
		#endregion

		#region SiteID
		[Site(typeof(Where<INSite.branchID.Is<Inside<orgBAccountID.FromCurrent>>>), false)]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region ItemClassID
		[PXDBInt]
		[PXUIField(DisplayName = "Item Class")]
		[PXDimensionSelector(INItemClass.Dimension,
			typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem.IsEqual<True>>>),
			typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), CacheGlobal = true)]
		public virtual int? ItemClassID { get; set; }
		public abstract class itemClassID : BqlInt.Field<itemClassID> { }
		#endregion

		#region InventoryID
		[AnyInventory(typeof(SelectFrom<InventoryItem>
			.Where<Match<AccessInfo.userName.FromCurrent>
				.And<InventoryItem.stkItem.IsEqual<True>>
				.And<itemClassID.FromCurrent.IsNull.Or<InventoryItem.itemClassID.IsEqual<itemClassID.FromCurrent>>>>
			.SearchFor<InventoryItem.inventoryID>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))]
		[PXRestrictor(typeof(Where<InventoryItem.itemStatus.IsNotEqual<InventoryItemStatus.markedForDeletion>>),
			IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion

		#region IsSuitableCalcsFound
		[PXDBBool]
		public virtual bool? IsSuitableCalcsFound { get; set; }
		public abstract class isSuitableCalcsFound : BqlBool.Field<isSuitableCalcsFound> { }
		#endregion

		#region IsPartialSuitableCalcs
		[PXDBBool]
		public virtual bool? IsPartialSuitableCalcs { get; set; }
		public abstract class isPartialSuitableCalcs : BqlBool.Field<isPartialSuitableCalcs> { }
		#endregion

		#region IsMixedSuitableCalcs
		[PXDBBool]
		public virtual bool? IsMixedSuitableCalcs { get; set; }
		public abstract class isMixedSuitableCalcs : BqlBool.Field<isMixedSuitableCalcs> { }
		#endregion

		#region SuitableCalcsSiteID
		[Site]
		public virtual int? SuitableCalcsSiteID { get; set; }
		public abstract class suitableCalcsSiteID : BqlInt.Field<suitableCalcsSiteID> { }
		#endregion
		#region SuitableCalcsItemClassID
		[PXDBInt]
		[PXDimensionSelector(INItemClass.Dimension,
			typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem.IsEqual<True>>>),
			typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), CacheGlobal = true)]
		public virtual int? SuitableCalcsItemClassID { get; set; }
		public abstract class suitableCalcsItemClassID : BqlInt.Field<suitableCalcsItemClassID> { }
		#endregion
		#region SuitableCalcsInventoryID
		[AnyInventory]
		public virtual int? SuitableCalcsInventoryID { get; set; }
		public abstract class suitableCalcsInventoryID : BqlBool.Field<suitableCalcsInventoryID> { }
		#endregion
		#region IsInventoryListCalc
		[PXDBBool]
		public virtual bool? IsInventoryListCalc { get; set; }
		public abstract class isInventoryListCalc : BqlBool.Field<isInventoryListCalc> { }
		#endregion
	}
}
