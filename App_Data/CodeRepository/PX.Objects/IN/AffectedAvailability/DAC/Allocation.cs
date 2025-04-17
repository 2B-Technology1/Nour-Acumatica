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
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.SO;
using CustomerAttribute = PX.Objects.AR.CustomerAttribute;

namespace PX.Objects.IN.AffectedAvailability
{
	// Used in IN622000 report (Sales Allocations Affected by Inventory Adjustments).
	/// <exclude/>
	[PXCacheName(Messages.Allocation)]
	[PXProjection(typeof(Select<AllocationInternal>))]
	public class Allocation : IBqlTable
	{
		#region PlanID
		[PXDBLong(IsKey = true, BqlField = typeof(AllocationInternal.planID))]
		public virtual Int64? PlanID { get; set; }
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		#endregion
		#region DocType
		[PXString]
		[PXEntityName(typeof(refNoteID))]
		[PXUIField(DisplayName = "Document Type")]
		public virtual string DocType { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion
		#region RefNoteID
		[PXRefNote(BqlField = typeof(AllocationInternal.refNoteID))]
		[PXUIField(DisplayName = "Document Nbr.")]
		public virtual Guid? RefNoteID { get; set; }
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		#endregion
		#region RefEntityType
		[PXDBString(255, IsUnicode = false, BqlField = typeof(AllocationInternal.refEntityType))]
		public string RefEntityType { get; set; }
		public abstract class refEntityType : PX.Data.BQL.BqlString.Field<refEntityType> { }
		#endregion
		#region LineNbr
		[PXDBInt(BqlField = typeof(AllocationInternal.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.")]
		public virtual Int32? LineNbr { get; set; }
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#endregion
		#region InventoryID
		[StockItem(BqlField = typeof(AllocationInternal.inventoryID))]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SiteID
		[Site(BqlField = typeof(AllocationInternal.siteID))]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region LotSerialNbr
		[PXUIField(DisplayName = "Allocated Lot/Serial Nbr.", FieldClass = "LotSerial")]
		[PXDBString(INLotSerialStatusByCostCenter.lotSerialNbr.Length, IsUnicode = true, InputMask = "", BqlField = typeof(AllocationInternal.lotSerialNbr))]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region AllocatedQty
		[PXDBQuantity(BqlField = typeof(AllocationInternal.allocatedQty))]
		[PXUIField(DisplayName = "Allocated Qty.")]
		public virtual Decimal? AllocatedQty { get; set; }
		public abstract class allocatedQty : PX.Data.BQL.BqlDecimal.Field<allocatedQty> { }
		#endregion

		#region Status
		[DocumentStatus.List]
		[PXDBString(255, IsUnicode = false, BqlField = typeof(AllocationInternal.status))]
		public virtual String Status { get; set; }
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		#endregion
		#region OwnerID
		[PXDBInt(BqlField = typeof(AllocationInternal.ownerID))]
		[PXUIField(DisplayName = "Owner")]
		[PXSelector(typeof(Search<Contact.contactID, Where<Contact.contactType, Equal<ContactTypesAttribute.employee>>>),
			SubstituteKey = typeof(Contact.displayName))]
		public virtual int? OwnerID { get; set; }
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		#endregion
		#region CustomerID
		[Customer(BqlField = typeof(AllocationInternal.customerID))]
		public virtual Int32? CustomerID { get; set; }
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		#endregion
		#region Date
		[PXDBDate(BqlField = typeof(AllocationInternal.date))]
		[PXUIField(DisplayName = "Document date")]
		public virtual DateTime? Date { get; set; }
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		#endregion
		#region RequestedDate
		[PXDBDate(BqlField = typeof(AllocationInternal.requestedDate))]
		[PXUIField(DisplayName = "Requested On")]
		public virtual DateTime? RequestedDate { get; set; }
		public abstract class requestedDate : PX.Data.BQL.BqlDateTime.Field<requestedDate> { }
		#endregion
	}

	[PXHidden]
	[PXProjection(typeof(Select2<INItemPlan,
		LeftJoin<SOOrder, On<INItemPlan.refEntityType, Equal<EntityType.soOrder>,
			And<SOOrder.noteID, Equal<INItemPlan.refNoteID>,
			And<INItemPlan.planType, In3<INPlanConstants.plan61, INPlanConstants.plan63>>>>,
		LeftJoin<SOLineSplit, On<SOLineSplit.orderType, Equal<SOOrder.orderType>,
			And<SOLineSplit.orderNbr, Equal<SOOrder.orderNbr>,
			And<SOLineSplit.planID, Equal<INItemPlan.planID>>>>,
		LeftJoin<SOShipment, On<INItemPlan.refEntityType, Equal<EntityType.soShipment>,
			And<SOShipment.noteID, Equal<INItemPlan.refNoteID>,
			And<INItemPlan.planType, In3<INPlanConstants.plan61, INPlanConstants.plan63>>>>,
		LeftJoin<SOShipLineSplit, On<SOShipLineSplit.shipmentNbr, Equal<SOShipment.shipmentNbr>,
			And<SOShipLineSplit.planID, Equal<INItemPlan.planID>>>,
		LeftJoin<INRegister, On<INItemPlan.refEntityType, In3<EntityType.inRegister, EntityType.inKitRegister>,
			And<INRegister.noteID, Equal<INItemPlan.refNoteID>,
			And<INItemPlan.planType, In3<INPlanConstants.plan20, INPlanConstants.plan40, INPlanConstants.plan41, INPlanConstants.plan50>>>>,
		LeftJoin<INTranSplit, On<INTranSplit.docType, Equal<INRegister.docType>,
			And<INTranSplit.refNbr, Equal<INRegister.refNbr>,
			And<INTranSplit.planID, Equal<INItemPlan.planID>>>>,
		LeftJoin<ChildPlans, On<ChildPlans.origPlanID, Equal<INItemPlan.planID>>,
		LeftJoin<ChildPlansByLS, On<ChildPlansByLS.origPlanID, Equal<INItemPlan.planID>,
			And<ChildPlansByLS.lotSerialNbr, Equal<INItemPlan.lotSerialNbr>>>>>>>>>>>,
		Where<INItemPlan.planType, In3<INPlanConstants.plan20, INPlanConstants.plan40, INPlanConstants.plan41, INPlanConstants.plan50,
				INPlanConstants.plan61, INPlanConstants.plan63, INPlanConstants.planF2, INPlanConstants.planM7>,
			And<Where<ChildPlans.origPlanID, IsNull,
				Or<ChildPlans.overalQty, Less<INItemPlan.planQty>,
				Or<INItemPlan.lotSerialNbr, IsNotNull,
					And<Where<ChildPlansByLS.lotSerialNbr, IsNull,
						Or<ChildPlansByLS.overalQty, Less<ChildPlans.overalQty>>>>>>>>>>))]
	public class AllocationInternal : IBqlTable
	{
		#region PlanID
		[PXDBLong(IsKey = true, BqlField = typeof(INItemPlan.planID))]
		public virtual Int64? PlanID { get; set; }
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		#endregion
		#region RefNoteID
		[PXRefNote(BqlField = typeof(INItemPlan.refNoteID))]
		public virtual Guid? RefNoteID { get; set; }
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		#endregion
		#region RefEntityType
		[PXDBString(255, IsUnicode = false, BqlField = typeof(INItemPlan.refEntityType))]
		public string RefEntityType { get; set; }
		public abstract class refEntityType : PX.Data.BQL.BqlString.Field<refEntityType> { }
		#endregion
		#region LineNbr
		[PXInt]
		[PXDBCalced(typeof(Switch<Case<Where<INItemPlan.refEntityType, Equal<EntityType.soOrder>>, SOLineSplit.lineNbr,
			Case<Where<INItemPlan.refEntityType, Equal<EntityType.soShipment>>, SOShipLineSplit.lineNbr>>, INTranSplit.lineNbr>), typeof(int))]
		public virtual Int32? LineNbr { get; set; }
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#endregion
		#region InventoryID
		[StockItem(BqlField = typeof(INItemPlan.inventoryID))]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SiteID
		[Site(BqlField = typeof(INItemPlan.siteID))]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region LotSerialNbr
		[PXString(INLotSerialStatusByCostCenter.lotSerialNbr.Length, IsUnicode = true, InputMask = "")]
		[PXDBCalced(typeof(Switch<Case<Where<INItemPlan.lotSerialNbr, IsNotNull>, INItemPlan.lotSerialNbr>, StringEmpty>), typeof(string))]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region AllocatedQty
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<INItemPlan.lotSerialNbr, IsNotNull, And<ChildPlans.origPlanID, IsNotNull,
				And<Where<ChildPlansByLS.lotSerialNbr, IsNull, Or<ChildPlansByLS.overalQty, Less<INItemPlan.planQty>>>>>>,
					Sub<INItemPlan.planQty, IsNull<ChildPlansByLS.overalQty, decimal0>>,
			Case<Where<ChildPlans.origPlanID, IsNotNull, And<ChildPlans.overalQty, Less<INItemPlan.planQty>>>,
				Sub<INItemPlan.planQty, ChildPlans.overalQty>>>, INItemPlan.planQty>), typeof(decimal))]
		public virtual Decimal? AllocatedQty { get; set; }
		public abstract class allocatedQty : PX.Data.BQL.BqlDecimal.Field<allocatedQty> { }
		#endregion

		#region Status
		[PXString(255, IsUnicode = false)]
		[PXDBCalced(typeof(Switch<Case<Where<INItemPlan.refEntityType, Equal<EntityType.soOrder>>, Add<INItemPlan.refEntityType, SOOrder.status>,
			Case<Where<INItemPlan.refEntityType, Equal<EntityType.soShipment>>, Add<INItemPlan.refEntityType, SOShipment.status>>>,
				Add<EntityType.inRegister, INRegister.status>>), typeof(string))]
		public virtual String Status { get; set; }
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		#endregion
		#region OwnerID
		[PXInt]
		[PXDBCalced(typeof(Switch<Case<Where<INItemPlan.refEntityType, Equal<EntityType.soOrder>>, SOOrder.ownerID,
			Case<Where<INItemPlan.refEntityType, Equal<EntityType.soShipment>>, SOShipment.ownerID>>, Null>), typeof(int))]
		public virtual int? OwnerID { get; set; }
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		#endregion
		#region CustomerID
		[PXInt]
		[PXDBCalced(typeof(Switch<Case<Where<INItemPlan.refEntityType, Equal<EntityType.soOrder>>, SOOrder.customerID,
			Case<Where<INItemPlan.refEntityType, Equal<EntityType.soShipment>>, SOShipment.customerID>>, Null>), typeof(int))]
		public virtual Int32? CustomerID { get; set; }
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		#endregion
		#region Date
		[PXDate]
		[PXDBCalced(typeof(Switch<Case<Where<INItemPlan.refEntityType, Equal<EntityType.soOrder>>, SOOrder.orderDate,
			Case<Where<INItemPlan.refEntityType, Equal<EntityType.soShipment>>, SOShipment.shipDate>>, INRegister.tranDate>), typeof(DateTime))]
		public virtual DateTime? Date { get; set; }
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		#endregion
		#region RequestedDate
		[PXDBDate(BqlField = typeof(SOOrder.requestDate))]
		public virtual DateTime? RequestedDate { get; set; }
		public abstract class requestedDate : PX.Data.BQL.BqlDateTime.Field<requestedDate> { }
		#endregion
	}

	[PXHidden]
	[PXProjection(typeof(Select4<INItemPlan, Where<INItemPlan.origPlanID, IsNotNull>,
		Aggregate<GroupBy<INItemPlan.origPlanID, Sum<INItemPlan.planQty>>>>))]
	public class ChildPlans : IBqlTable
	{
		#region OrigPlanID
		[PXDBLong(BqlField = typeof(INItemPlan.origPlanID), IsKey = true)]
		public virtual Int64? OrigPlanID { get; set; }
		public new abstract class origPlanID : PX.Data.BQL.BqlLong.Field<origPlanID> { }
		#endregion
		#region OveralQty
		[PXDBQuantity(BqlField = typeof(INItemPlan.planQty))]
		public virtual Decimal? OveralQty { get; set; }
		public abstract class overalQty : PX.Data.BQL.BqlDecimal.Field<overalQty> { }
		#endregion
	}

	[PXHidden]
	[PXProjection(typeof(Select4<INItemPlan, Where<INItemPlan.origPlanID, IsNotNull, And<INItemPlan.lotSerialNbr, IsNotNull>>,
		Aggregate<GroupBy<INItemPlan.origPlanID, GroupBy<INItemPlan.lotSerialNbr, Sum<INItemPlan.planQty>>>>>))]
	public class ChildPlansByLS : IBqlTable
	{
		#region OrigPlanID
		[PXDBLong(BqlField = typeof(INItemPlan.origPlanID), IsKey = true)]
		public virtual Int64? OrigPlanID { get; set; }
		public new abstract class origPlanID : PX.Data.BQL.BqlLong.Field<origPlanID> { }
		#endregion
		#region LotSerialNbr
		[PXDBString(INLotSerialStatusByCostCenter.lotSerialNbr.Length, IsUnicode = true, InputMask = "",
			BqlField = typeof(INItemPlan.lotSerialNbr), IsKey = true)]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region OveralQty
		[PXDBQuantity(BqlField = typeof(INItemPlan.planQty))]
		public virtual Decimal? OveralQty { get; set; }
		public abstract class overalQty : PX.Data.BQL.BqlDecimal.Field<overalQty> { }
		#endregion
	}
}
