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

namespace PX.Objects.SO
{
	using System;
	using Data.ReferentialIntegrity.Attributes;
	using PX.Data;
	using PX.Objects.IN;
	using PX.Objects.CS;
	using PX.Objects.GL;
    using PX.Data.BQL.Fluent;

    [System.SerializableAttribute()]
	[PXCacheName(Messages.SOShipLineSplit)]
	[SOShipLineSplitProjection(typeof(Select<SOShipLineSplit>), typeof(SOShipLineSplit.isUnassigned), false)]
	public partial class SOShipLineSplit : PX.Data.IBqlTable, ILSDetail, ILSGeneratedDetail, IItemPlanSOShipSource
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOShipLineSplit>.By<shipmentNbr, lineNbr, splitLineNbr>
		{
			public static SOShipLineSplit Find(PXGraph graph, string shipmentNbr, int? lineNbr, int? splitLineNbr, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, shipmentNbr, lineNbr, splitLineNbr, options);
		}

		public static class FK
		{
			public class Shipment : SOShipment.PK.ForeignKeyOf<SOShipLineSplit>.By<shipmentNbr> { }
			public class ShipmentLine : SOShipLine.PK.ForeignKeyOf<SOShipLineSplit>.By<shipmentNbr, lineNbr> { }
			public class ShipmentLineSplit : SOShipLineSplit.PK.ForeignKeyOf<SOShipLineSplit>.By<shipmentNbr, lineNbr, splitLineNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<SOShipLineSplit>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<SOShipLineSplit>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<SOShipLineSplit>.By<siteID> { }
            public class SiteStatus : IN.INSiteStatus.PK.ForeignKeyOf<SOShipLineSplit>.By<inventoryID, subItemID, siteID> { }
            public class Location : INLocation.PK.ForeignKeyOf<SOShipLineSplit>.By<locationID> { }
            public class LocationStatus : IN.INLocationStatus.PK.ForeignKeyOf<SOShipLineSplit>.By<inventoryID, subItemID, siteID, locationID> { }
            public class LotSerialStatus : IN.INLotSerialStatus.PK.ForeignKeyOf<SOShipLineSplit>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr> { }
            public class PlanType : INPlanType.PK.ForeignKeyOf<SOShipLineSplit>.By<planType> { }
			public class ItemPlan : INItemPlan.PK.ForeignKeyOf<SOShipLineSplit>.By<planID> { }
			public class OriginalOrderType : SOOrderType.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType> { }
			public class OriginalOrder : SOOrder.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType, origOrderNbr> { }
			public class OriginalOrderLine : SOLine.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType, origOrderNbr, origLineNbr> { }
			public class OriginalOrderLineSplit : SOLineSplit.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType, origOrderNbr, origLineNbr, origSplitLineNbr> { }
			public class OriginalPlanType : INPlanType.PK.ForeignKeyOf<SOShipLineSplit>.By<origPlanType> { }
			public class OriginalLineSplit : SOLineSplit.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType, origOrderNbr, origLineNbr, origSplitLineNbr> { }
            public class Operation : SOOrderTypeOperation.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType, operation> { }
            public class Project : PM.PMProject.PK.ForeignKeyOf<SOShipLineSplit>.By<projectID> { }
            public class Task : PM.PMTask.PK.ForeignKeyOf<SOShipLineSplit>.By<projectID, taskID> { }
            //todo public class UnitOfMeasure : INUnit.UK.ByInventory.ForeignKeyOf<SOShipLineSplit>.By<inventoryID, uOM> { }
        }
		#endregion

		#region ShipmentNbr
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		protected String _ShipmentNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(SOShipment.shipmentNbr))]
		[PXParent(typeof(FK.Shipment))]
		[PXParent(typeof(FK.ShipmentLine))]
		public virtual String ShipmentNbr
		{
			get
			{
				return this._ShipmentNbr;
			}
			set
			{
				this._ShipmentNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(SOShipLine.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Enabled = false, Visible = false)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region OrigOrderType
		public abstract class origOrderType : PX.Data.BQL.BqlString.Field<origOrderType> { }
		protected String _OrigOrderType;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(typeof(SOShipLine.origOrderType))]
		public virtual String OrigOrderType
		{
			get
			{
				return this._OrigOrderType;
			}
			set
			{
				this._OrigOrderType = value;
			}
		}
		#endregion
		#region OrigOrderNbr
		public abstract class origOrderNbr : PX.Data.BQL.BqlString.Field<origOrderNbr> { }
		protected String _OrigOrderNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXDefault(typeof(SOShipLine.origOrderNbr))]
		public virtual String OrigOrderNbr
		{
			get
			{
				return this._OrigOrderNbr;
			}
			set
			{
				this._OrigOrderNbr = value;
			}
		}
		#endregion
		#region OrigLineNbr
		public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
		protected Int32? _OrigLineNbr;
		[PXDBInt()]
		[PXDefault(typeof(SOShipLine.origLineNbr))]
		public virtual Int32? OrigLineNbr
		{
			get
			{
				return this._OrigLineNbr;
			}
			set
			{
				this._OrigLineNbr = value;
			}
		}
		#endregion
		#region OrigSplitLineNbr
		public abstract class origSplitLineNbr : PX.Data.BQL.BqlInt.Field<origSplitLineNbr> { }
		protected Int32? _OrigSplitLineNbr;
		[PXDBInt()]
		[PXDefault(typeof(SOShipLine.origSplitLineNbr))]
		public virtual Int32? OrigSplitLineNbr
		{
			get
			{
				return this._OrigSplitLineNbr;
			}
			set
			{
				this._OrigSplitLineNbr = value;
			}
		}
		#endregion
		#region OrigPlanType
		public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
		[PXDBString(2, IsFixed = true)]
		[PXDefault(typeof(SOShipLine.origPlanType))]
		[PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
		public virtual String OrigPlanType
		{
			get;
			set;
		}
		#endregion
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a")]
		[PXDefault(typeof(SOShipLine.operation))]
		[PXSelector(typeof(Search<SOOrderTypeOperation.operation, Where<SOOrderTypeOperation.orderType, Equal<Current<SOShipLineSplit.origOrderType>>>>))]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
		protected Int32? _SplitLineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXLineNbr(typeof(SOShipment.lineCntr), DecrementOnDelete = false)]
		public virtual Int32? SplitLineNbr
		{
			get
			{
				return this._SplitLineNbr;
			}
			set
			{
				this._SplitLineNbr = value;
			}
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		protected Int16? _InvtMult;
		[PXDBShort()]
		[PXDefault(typeof(INTran.invtMult))]
		public virtual Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(Enabled = false, Visible = true)]
		[PXDefault(typeof(SOShipLine.inventoryID))]
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
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(typeof(SOShipLine.lineType))]
		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
			}
		}
		#endregion
		#region IsStockItem
		public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
		[PXDBBool()]
		[PXFormula(typeof(Selector<SOShipLineSplit.inventoryID, InventoryItem.stkItem>))]
		public bool? IsStockItem
		{
			get;
			set;
		}
		#endregion
		#region IsComponentItem
		public abstract class isComponentItem : PX.Data.BQL.BqlBool.Field<isComponentItem> { }
		[PXDBBool()]
		[PXFormula(typeof(Switch<Case<Where<SOShipLineSplit.inventoryID, Equal<Current<SOShipLine.inventoryID>>>, False>, True>))]
		public bool? IsComponentItem
		{
			get;
			set;
		}
		#endregion
		#region IsIntercompany
		public abstract class isIntercompany : Data.BQL.BqlBool.Field<isIntercompany> { }
		[PXDBBool]
		[PXDefault(typeof(SOShipLine.isIntercompany))]
		public virtual bool? IsIntercompany
		{
			get;
			set;
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXFormula(typeof(Selector<SOShipLineSplit.operation, SOOrderTypeOperation.iNDocType>))]
		[PXString(SOOrderTypeOperation.iNDocType.Length, IsFixed = true)]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region TranDate
		public virtual DateTime? TranDate
		{
			get { return this._ShipDate; }
		}
		#endregion
		#region PlanType
		public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
		protected String _PlanType;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(typeof(SOShipLine.planType))]
		public virtual String PlanType
		{
			get
			{
				return this._PlanType;
			}
			set
			{
				this._PlanType = value;
			}
		}
		#endregion
		#region PlanID
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		protected Int64? _PlanID;
		[PXDBLong(IsImmutable = true)]
		public virtual Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(Enabled = false, Visible = false)]
		[PXDefault(typeof(SOShipLine.siteID))]
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
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[SOLocationAvail(typeof(SOShipLineSplit.inventoryID), typeof(SOShipLineSplit.subItemID), typeof(SOShipLine.costCenterID), typeof(SOShipLineSplit.siteID), typeof(SOShipLineSplit.tranType), typeof(SOShipLineSplit.invtMult))]
		[PXDefault()]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[IN.SubItem(typeof(SOShipLineSplit.inventoryID))]
		[PXDefault()]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
        [SOShipLotSerialNbr(typeof(SOShipLineSplit.siteID), typeof(SOShipLineSplit.inventoryID), typeof(SOShipLineSplit.subItemID), typeof(SOShipLineSplit.locationID), typeof(SOShipLine.lotSerialNbr), typeof(SOShipLine.costCenterID))]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
        #region LastLotSerialNbr
        public abstract class lastLotSerialNbr : PX.Data.BQL.BqlString.Field<lastLotSerialNbr> { }
        protected String _LastLotSerialNbr;
        [PXString(100, IsUnicode = true)]
        public virtual String LastLotSerialNbr
        {
            get
            {
                return this._LastLotSerialNbr;
            }
            set
            {
                this._LastLotSerialNbr = value;
            }
        }
        #endregion
		#region LotSerClassID
		public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }
		protected String _LotSerClassID;
		[PXString(10, IsUnicode = true)]
		public virtual String LotSerClassID
		{
			get
			{
				return this._LotSerClassID;
			}
			set
			{
				this._LotSerClassID = value;
			}
		}
		#endregion
		#region AssignedNbr
		public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr> { }
		protected String _AssignedNbr;
		[PXString(30, IsUnicode = true)]
		public virtual String AssignedNbr
		{
			get
			{
				return this._AssignedNbr;
			}
			set
			{
				this._AssignedNbr = value;
			}
		}
		#endregion
		#region HasGeneratedLotSerialNbr
		public abstract class hasGeneratedLotSerialNbr : PX.Data.BQL.BqlBool.Field<hasGeneratedLotSerialNbr> { }
		[PXDBBool]
		[PXDefault(false)]
		[Common.Attributes.HasFieldBeenModified(typeof(lotSerialNbr), OriginalValueField = typeof(OriginalValues.originalLotSerialNbr), InvertResult = true)]
		public bool? HasGeneratedLotSerialNbr
		{
			get;
			set;
		}
		#endregion
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
		[SOShipExpireDate(typeof(SOShipLineSplit.inventoryID))]
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(SOShipLineSplit.inventoryID), DisplayName = "UOM", Enabled = false)]
		[PXDefault]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity(typeof(SOShipLineSplit.uOM), typeof(SOShipLineSplit.baseQty), InventoryUnitType.BaseUnit)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region BaseQty
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		protected Decimal? _BaseQty;
		[PXDBDecimal(6)]
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseQty;
			}
			set
			{
				this._BaseQty = value;
			}
		}
		#endregion
		#region PickedQty
		[PXDBQuantity(typeof(uOM), typeof(basePickedQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Picked Quantity", Enabled = false)]
		public virtual Decimal? PickedQty { get; set; }
		public abstract class pickedQty : PX.Data.BQL.BqlDecimal.Field<pickedQty> { }
		#endregion
		#region BasePickedQty
		[PXDBDecimal(6)]
		public virtual Decimal? BasePickedQty { get; set; }
		public abstract class basePickedQty : PX.Data.BQL.BqlDecimal.Field<basePickedQty> { }
		#endregion
		#region PackedQty
		[PXDBQuantity(typeof(uOM), typeof(basePackedQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Packed Quantity", Enabled = false)]
		public virtual Decimal? PackedQty { get; set; }
		public abstract class packedQty : PX.Data.BQL.BqlDecimal.Field<packedQty> { }
		#endregion
		#region BasePackedQty
		[PXDBDecimal(6)]
		public virtual Decimal? BasePackedQty { get; set; }
		public abstract class basePackedQty : PX.Data.BQL.BqlDecimal.Field<basePackedQty> { }
		#endregion
		#region ShipDate
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
		protected DateTime? _ShipDate;
		[PXDBDate()]
		[PXDBDefault(typeof(SOShipment.shipDate))]
		public virtual DateTime? ShipDate
		{
			get
			{
				return this._ShipDate;
			}
			set
			{
				this._ShipDate = value;
			}
		}
		#endregion
		#region Confirmed
		public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }
		protected Boolean? _Confirmed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Confirmed")]
		public virtual Boolean? Confirmed
		{
			get
			{
				return this._Confirmed;
			}
			set
			{
				this._Confirmed = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region IsUnassigned
		public abstract class isUnassigned : PX.Data.BQL.BqlBool.Field<isUnassigned> { }
		protected Boolean? _IsUnassigned;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsUnassigned
		{
			get
			{
				return this._IsUnassigned;
			}
			set
			{
				this._IsUnassigned = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXInt]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXInt]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion	
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public sealed class OriginalValues : PXCacheExtension<SOShipLineSplit>
		{
			#region OriginalLotSerialNbr
			[PXString]
			[PXDBCalced(typeof(lotSerialNbr), typeof(string))]
			public String OriginalLotSerialNbr { get; set; }
			public abstract class originalLotSerialNbr : PX.Data.BQL.BqlString.Field<originalLotSerialNbr> { }
			#endregion
		}
	}
    namespace Unassigned
    {
        /// <summary>
        /// Is exact copy of SOShipLineSplit except PXProjection Where clause.
        /// </summary>
        [PXHidden]
        [System.SerializableAttribute()]
		[SOShipLineSplitProjection(typeof(Select<SOShipLineSplit>), typeof(SOShipLineSplit.isUnassigned), true)]
        public partial class SOShipLineSplit : PX.Data.IBqlTable, ILSDetail, ILSGeneratedDetail, IItemPlanSOShipSource
		{
			#region Keys
			public class PK : PrimaryKeyOf<SOShipLineSplit>.By<shipmentNbr, lineNbr, splitLineNbr>
			{
				public static SOShipLineSplit Find(PXGraph graph, string shipmentNbr, int? lineNbr, int? splitLineNbr, PKFindOptions options = PKFindOptions.None)
					=> FindBy(graph, shipmentNbr, lineNbr, splitLineNbr, options);
			}

			public static class FK
			{
				public class Shipment : SOShipment.PK.ForeignKeyOf<SOShipLineSplit>.By<shipmentNbr> { }
				public class ShipmentLine : SOShipLine.PK.ForeignKeyOf<SOShipLineSplit>.By<shipmentNbr, lineNbr> { }
				public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<SOShipLineSplit>.By<inventoryID> { }
				public class Site : INSite.PK.ForeignKeyOf<SOShipLineSplit>.By<siteID> { }
				public class PlanType : INPlanType.PK.ForeignKeyOf<SOShipLineSplit>.By<planType> { }
				public class OrigPlanType : INPlanType.PK.ForeignKeyOf<SOShipLineSplit>.By<origPlanType> { }
				public class OrigLineSplit : SOLineSplit.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType, origOrderNbr, origLineNbr, origSplitLineNbr> { }
            }
			#endregion

			#region ShipmentNbr
			public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
            protected String _ShipmentNbr;
            [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
            [PXDBDefault(typeof(SOShipment.shipmentNbr))]
			[PXParent(typeof(Select<SOShipment, Where<SOShipment.shipmentNbr, Equal<Current<SOShipLineSplit.shipmentNbr>>>>))]
            [PXParent(typeof(Select<SOShipLine, Where<SOShipLine.shipmentNbr, Equal<Current<SOShipLineSplit.shipmentNbr>>, And<SOShipLine.lineNbr, Equal<Current<SOShipLineSplit.lineNbr>>>>>))]
            public virtual String ShipmentNbr
            {
                get
                {
                    return this._ShipmentNbr;
                }
                set
                {
                    this._ShipmentNbr = value;
                }
            }
            #endregion
            #region LineNbr
            public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
            protected Int32? _LineNbr;
            [PXDBInt(IsKey = true)]
            [PXDefault(typeof(SOShipLine.lineNbr))]
            public virtual Int32? LineNbr
            {
                get
                {
                    return this._LineNbr;
                }
                set
                {
                    this._LineNbr = value;
                }
            }
            #endregion
            #region OrigOrderType
            public abstract class origOrderType : PX.Data.BQL.BqlString.Field<origOrderType> { }
            protected String _OrigOrderType;
            [PXDBString(2, IsFixed = true)]
            [PXDefault(typeof(SOShipLine.origOrderType))]
            public virtual String OrigOrderType
            {
                get
                {
                    return this._OrigOrderType;
                }
                set
                {
                    this._OrigOrderType = value;
                }
            }
            #endregion
            #region OrigOrderNbr
            public abstract class origOrderNbr : PX.Data.BQL.BqlString.Field<origOrderNbr> { }
            protected String _OrigOrderNbr;
            [PXDBString(15, IsUnicode = true)]
            [PXDefault(typeof(SOShipLine.origOrderNbr))]
            public virtual String OrigOrderNbr
            {
                get
                {
                    return this._OrigOrderNbr;
                }
                set
                {
                    this._OrigOrderNbr = value;
                }
            }
            #endregion
            #region OrigLineNbr
            public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
            protected Int32? _OrigLineNbr;
            [PXDBInt()]
            [PXDefault(typeof(SOShipLine.origLineNbr))]
            public virtual Int32? OrigLineNbr
            {
                get
                {
                    return this._OrigLineNbr;
                }
                set
                {
                    this._OrigLineNbr = value;
                }
            }
            #endregion
            #region OrigSplitLineNbr
            public abstract class origSplitLineNbr : PX.Data.BQL.BqlInt.Field<origSplitLineNbr> { }
            protected Int32? _OrigSplitLineNbr;
            [PXDBInt()]
            [PXDefault(typeof(SOShipLine.origSplitLineNbr), PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual Int32? OrigSplitLineNbr
            {
                get
                {
                    return this._OrigSplitLineNbr;
                }
                set
                {
                    this._OrigSplitLineNbr = value;
                }
            }
            #endregion
            #region OrigPlanType
            public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
            [PXDBString(2, IsFixed = true)]
            [PXDefault(typeof(SOShipLine.origPlanType))]
            [PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
            public virtual String OrigPlanType
            {
                get;
                set;
            }
            #endregion
            #region Operation
            public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
            protected String _Operation;
            [PXDBString(1, IsFixed = true, InputMask = ">a")]
            [PXDefault(typeof(SOShipLine.operation))]
            public virtual String Operation
            {
                get
                {
                    return this._Operation;
                }
                set
                {
                    this._Operation = value;
                }
            }
            #endregion
            #region SplitLineNbr
            public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
            protected Int32? _SplitLineNbr;
            [PXDBInt(IsKey = true)]
            [PXDefault()]
			[PXLineNbr(typeof(SOShipment.lineCntr), DecrementOnDelete = false)]
            public virtual Int32? SplitLineNbr
            {
                get
                {
                    return this._SplitLineNbr;
                }
                set
                {
                    this._SplitLineNbr = value;
                }
            }
            #endregion
            #region InvtMult
            public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
            protected Int16? _InvtMult;
            [PXDBShort()]
            [PXDefault(typeof(INTran.invtMult))]
            public virtual Int16? InvtMult
            {
                get
                {
                    return this._InvtMult;
                }
                set
                {
                    this._InvtMult = value;
                }
            }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
            protected Int32? _InventoryID;
            [Inventory(Enabled = false, Visible = true)]
            [PXDefault(typeof(SOShipLine.inventoryID))]
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
            #region LineType
            public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
            protected String _LineType;
            [PXDBString(2, IsFixed = true)]
            [PXDefault(typeof(SOShipLine.lineType))]
            public virtual String LineType
            {
                get
                {
                    return this._LineType;
                }
                set
                {
                    this._LineType = value;
                }
            }
            #endregion
            #region IsStockItem
            public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
            [PXDBBool()]
            [PXFormula(typeof(Selector<SOShipLineSplit.inventoryID, InventoryItem.stkItem>))]
            public bool? IsStockItem
            {
                get;
                set;
            }
            #endregion
            #region IsComponentItem
            public abstract class isComponentItem : PX.Data.BQL.BqlBool.Field<isComponentItem> { }
            [PXDBBool()]
            [PXFormula(typeof(Switch<Case<Where<SOShipLineSplit.inventoryID, Equal<Current<SOShipLine.inventoryID>>>, False>, True>))]
            public bool? IsComponentItem
            {
                get;
                set;
            }
            #endregion
			#region IsIntercompany
			public abstract class isIntercompany : Data.BQL.BqlBool.Field<isIntercompany> { }
			[PXDBBool]
			[PXDefault(typeof(SOShipLine.isIntercompany))]
			public virtual bool? IsIntercompany
			{
				get;
				set;
			}
			#endregion
            #region TranType
            public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
            protected String _TranType;
			[PXFormula(typeof(Selector<SOShipLineSplit.operation, SOOrderTypeOperation.iNDocType>))]
            [PXString(SOOrderTypeOperation.iNDocType.Length, IsFixed = true)]
            public virtual String TranType
            {
                get
                {
                    return this._TranType;
                }
                set
                {
                    this._TranType = value;
                }
            }
            #endregion
            #region TranDate
            public virtual DateTime? TranDate
            {
                get { return this._ShipDate; }
            }
            #endregion
            #region PlanType
            public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
            protected String _PlanType;
            [PXDBString(2, IsFixed = true)]
            [PXDefault(typeof(SOShipLine.planType))]
            public virtual String PlanType
            {
                get
                {
                    return this._PlanType;
                }
                set
                {
                    this._PlanType = value;
                }
            }
            #endregion
            #region PlanID
            public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
            protected Int64? _PlanID;
            [PXDBLong(IsImmutable = true)]
            public virtual Int64? PlanID
            {
                get
                {
                    return this._PlanID;
                }
                set
                {
                    this._PlanID = value;
                }
            }
            #endregion
            #region SiteID
            public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
            protected Int32? _SiteID;
            [Site(Enabled = false, Visible = false)]
            [PXDefault(typeof(SOShipLine.siteID))]
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
            #region LocationID
            public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
            protected Int32? _LocationID;
            [SOLocationAvail(typeof(SOShipLineSplit.inventoryID), typeof(SOShipLineSplit.subItemID), typeof(SOShipLine.costCenterID), typeof(SOShipLineSplit.siteID), typeof(SOShipLineSplit.tranType), typeof(SOShipLineSplit.invtMult))]
            [PXDefault()]
            public virtual Int32? LocationID
            {
                get
                {
                    return this._LocationID;
                }
                set
                {
                    this._LocationID = value;
                }
            }
            #endregion
            #region SubItemID
            public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
            protected Int32? _SubItemID;
            [IN.SubItem(typeof(SOShipLineSplit.inventoryID))]
            [PXDefault()]
            public virtual Int32? SubItemID
            {
                get
                {
                    return this._SubItemID;
                }
                set
                {
                    this._SubItemID = value;
                }
            }
            #endregion
            #region LotSerialNbr
            public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
            protected String _LotSerialNbr;
            [SOShipLotSerialNbr(typeof(SOShipLineSplit.siteID), typeof(SOShipLineSplit.inventoryID), typeof(SOShipLineSplit.subItemID), typeof(SOShipLineSplit.locationID), typeof(SOShipLine.lotSerialNbr), typeof(SOShipLine.costCenterID))]
            public virtual String LotSerialNbr
            {
                get
                {
                    return this._LotSerialNbr;
                }
                set
                {
                    this._LotSerialNbr = value;
                }
            }
            #endregion
            #region LastLotSerialNbr
            public abstract class lastLotSerialNbr : PX.Data.BQL.BqlString.Field<lastLotSerialNbr> { }
            protected String _LastLotSerialNbr;
            [PXString(100, IsUnicode = true)]
            public virtual String LastLotSerialNbr
            {
                get
                {
                    return this._LastLotSerialNbr;
                }
                set
                {
                    this._LastLotSerialNbr = value;
                }
            }
            #endregion
            #region LotSerClassID
            public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }
            protected String _LotSerClassID;
            [PXString(10, IsUnicode = true)]
            public virtual String LotSerClassID
            {
                get
                {
                    return this._LotSerClassID;
                }
                set
                {
                    this._LotSerClassID = value;
                }
            }
            #endregion
            #region AssignedNbr
            public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr> { }
            protected String _AssignedNbr;
            [PXString(30, IsUnicode = true)]
            public virtual String AssignedNbr
            {
                get
                {
                    return this._AssignedNbr;
                }
                set
                {
                    this._AssignedNbr = value;
                }
            }
            #endregion
			#region HasGeneratedLotSerialNbr
			public abstract class hasGeneratedLotSerialNbr : PX.Data.BQL.BqlBool.Field<hasGeneratedLotSerialNbr> { }
			[PXDBBool]
			[PXDefault(false)]
			[Common.Attributes.HasFieldBeenModified(typeof(lotSerialNbr), OriginalValueField = typeof(OriginalValues.originalLotSerialNbr), InvertResult = true)]
			public bool? HasGeneratedLotSerialNbr
			{
				get;
				set;
			}
			#endregion
            #region ExpireDate
            public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
            protected DateTime? _ExpireDate;
            [SOShipExpireDate(typeof(SOShipLineSplit.inventoryID))]
            public virtual DateTime? ExpireDate
            {
                get
                {
                    return this._ExpireDate;
                }
                set
                {
                    this._ExpireDate = value;
                }
            }
            #endregion
            #region UOM
            public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
            protected String _UOM;
            [INUnit(typeof(SOShipLineSplit.inventoryID), DisplayName = "UOM", Enabled = false)]
            [PXDefault(typeof(Selector<inventoryID, InventoryItem.baseUnit>))]
            public virtual String UOM
            {
                get
                {
                    return this._UOM;
                }
                set
                {
                    this._UOM = value;
                }
            }
            #endregion
            #region Qty
            public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
            protected Decimal? _Qty;
            [PXDBQuantity(typeof(SOShipLineSplit.uOM), typeof(SOShipLineSplit.baseQty), InventoryUnitType.BaseUnit)]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Quantity")]
            public virtual Decimal? Qty
            {
                get
                {
                    return this._Qty;
                }
                set
                {
                    this._Qty = value;
                }
            }
            #endregion
            #region BaseQty
            public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
            protected Decimal? _BaseQty;
            [PXDBDecimal(6)]
            public virtual Decimal? BaseQty
            {
                get
                {
                    return this._BaseQty;
                }
                set
                {
                    this._BaseQty = value;
                }
            }
			#endregion
			#region PickedQty
			[PXDBQuantity(typeof(uOM), typeof(basePickedQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Picked Quantity")]
			public virtual Decimal? PickedQty { get; set; }
			public abstract class pickedQty : PX.Data.BQL.BqlDecimal.Field<pickedQty> { }
			#endregion
			#region BasePickedQty
			[PXDBDecimal(6)]
			public virtual Decimal? BasePickedQty { get; set; }
			public abstract class basePickedQty : PX.Data.BQL.BqlDecimal.Field<basePickedQty> { }
			#endregion
			#region PackedQty
			[PXDBQuantity(typeof(uOM), typeof(basePackedQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Packed Quantity")]
			public virtual Decimal? PackedQty { get; set; }
			public abstract class packedQty : PX.Data.BQL.BqlDecimal.Field<packedQty> { }
			#endregion
			#region BasePackedQty
			[PXDBDecimal(6)]
			public virtual Decimal? BasePackedQty { get; set; }
			public abstract class basePackedQty : PX.Data.BQL.BqlDecimal.Field<basePackedQty> { }
			#endregion
			#region ShipDate
			public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
            protected DateTime? _ShipDate;
            [PXDBDate()]
            [PXDBDefault(typeof(SOShipment.shipDate))]
            public virtual DateTime? ShipDate
            {
                get
                {
                    return this._ShipDate;
                }
                set
                {
                    this._ShipDate = value;
                }
            }
            #endregion
            #region Confirmed
            public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }
            protected Boolean? _Confirmed;
            [PXDBBool()]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Confirmed")]
            public virtual Boolean? Confirmed
            {
                get
                {
                    return this._Confirmed;
                }
                set
                {
                    this._Confirmed = value;
                }
            }
            #endregion
            #region Released
            public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
            protected Boolean? _Released;
            [PXDBBool()]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Released")]
            public virtual Boolean? Released
            {
                get
                {
                    return this._Released;
                }
                set
                {
                    this._Released = value;
                }
            }
            #endregion
            #region IsUnassigned
            public abstract class isUnassigned : PX.Data.BQL.BqlBool.Field<isUnassigned> { }
            protected Boolean? _IsUnassigned;
            [PXDBBool()]
            [PXDefault(typeof(SOShipLine.isUnassigned))]
            public virtual Boolean? IsUnassigned
            {
                get
                {
                    return this._IsUnassigned;
                }
                set
                {
                    this._IsUnassigned = value;
                }
            }
            #endregion
            #region ProjectID
            public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
            protected Int32? _ProjectID;
            [PXInt]
            public virtual Int32? ProjectID
            {
                get
                {
                    return this._ProjectID;
                }
                set
                {
                    this._ProjectID = value;
                }
            }
            #endregion
            #region TaskID
            public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
            protected Int32? _TaskID;
            [PXInt]
            public virtual Int32? TaskID
            {
                get
                {
                    return this._TaskID;
                }
                set
                {
                    this._TaskID = value;
                }
            }
            #endregion
            #region CreatedByID
            public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
            protected Guid? _CreatedByID;
            [PXDBCreatedByID()]
            public virtual Guid? CreatedByID
            {
                get
                {
                    return this._CreatedByID;
                }
                set
                {
                    this._CreatedByID = value;
                }
            }
            #endregion
            #region CreatedByScreenID
            public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
            protected String _CreatedByScreenID;
            [PXDBCreatedByScreenID()]
            public virtual String CreatedByScreenID
            {
                get
                {
                    return this._CreatedByScreenID;
                }
                set
                {
                    this._CreatedByScreenID = value;
                }
            }
            #endregion
            #region CreatedDateTime
            public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
            protected DateTime? _CreatedDateTime;
            [PXDBCreatedDateTime()]
            public virtual DateTime? CreatedDateTime
            {
                get
                {
                    return this._CreatedDateTime;
                }
                set
                {
                    this._CreatedDateTime = value;
                }
            }
            #endregion
            #region LastModifiedByID
            public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
            protected Guid? _LastModifiedByID;
            [PXDBLastModifiedByID()]
            public virtual Guid? LastModifiedByID
            {
                get
                {
                    return this._LastModifiedByID;
                }
                set
                {
                    this._LastModifiedByID = value;
                }
            }
            #endregion
            #region LastModifiedByScreenID
            public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
            protected String _LastModifiedByScreenID;
            [PXDBLastModifiedByScreenID()]
            public virtual String LastModifiedByScreenID
            {
                get
                {
                    return this._LastModifiedByScreenID;
                }
                set
                {
                    this._LastModifiedByScreenID = value;
                }
            }
            #endregion
            #region LastModifiedDateTime
            public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
            protected DateTime? _LastModifiedDateTime;
            [PXDBLastModifiedDateTime()]
            public virtual DateTime? LastModifiedDateTime
            {
                get
                {
                    return this._LastModifiedDateTime;
                }
                set
                {
                    this._LastModifiedDateTime = value;
                }
            }
            #endregion
            #region tstamp
            public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
            protected Byte[] _tstamp;
            [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
            public virtual Byte[] tstamp
            {
                get
                {
                    return this._tstamp;
                }
                set
                {
                    this._tstamp = value;
                }
            }
			#endregion

			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public sealed class OriginalValues : PXCacheExtension<SOShipLineSplit>
            {
                #region OriginalLotSerialNbr
                [PXString]
                [PXDBCalced(typeof(lotSerialNbr), typeof(string))]
                public String OriginalLotSerialNbr { get; set; }
                public abstract class originalLotSerialNbr : PX.Data.BQL.BqlString.Field<originalLotSerialNbr> { }
                #endregion
            }
        }
    }

    namespace Table
    { 
        /// <summary>
		/// It's a table, not a projection.
		/// </summary>
		[System.SerializableAttribute()]
        public partial class SOShipLineSplit : PX.Data.IBqlTable, ILSDetail, ILSGeneratedDetail, IItemPlanSOShipSource
		{
            #region Keys
            public class PK : PrimaryKeyOf<SOShipLineSplit>.By<shipmentNbr, lineNbr, splitLineNbr>
            {
                public static SOShipLineSplit Find(PXGraph graph, string shipmentNbr, int? lineNbr, int? splitLineNbr, PKFindOptions options = PKFindOptions.None)
                    => FindBy(graph, shipmentNbr, lineNbr, splitLineNbr, options);
            }

            public static class FK
            {
                public class Shipment : SOShipment.PK.ForeignKeyOf<SOShipLineSplit>.By<shipmentNbr> { }
                public class ShipmentLine : SOShipLine.PK.ForeignKeyOf<SOShipLineSplit>.By<shipmentNbr, lineNbr> { }
                public class ShipmentLineSplit : SOShipLineSplit.PK.ForeignKeyOf<SOShipLineSplit>.By<shipmentNbr, lineNbr, splitLineNbr> { }
                public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<SOShipLineSplit>.By<inventoryID> { }
                public class SubItem : INSubItem.PK.ForeignKeyOf<SOShipLineSplit>.By<subItemID> { }
                public class Site : INSite.PK.ForeignKeyOf<SOShipLineSplit>.By<siteID> { }
                public class Location : INLocation.PK.ForeignKeyOf<SOShipLineSplit>.By<locationID> { }
                public class PlanType : INPlanType.PK.ForeignKeyOf<SOShipLineSplit>.By<planType> { }
                public class ItemPlan : INItemPlan.PK.ForeignKeyOf<SOShipLineSplit>.By<planID> { }
                public class OriginalOrderType : SOOrderType.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType> { }
                public class OriginalOrder : SOOrder.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType, origOrderNbr> { }
                public class OriginalOrderLine : SOLine.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType, origOrderNbr, origLineNbr> { }
                public class OriginalOrderLineSplit : SOLineSplit.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType, origOrderNbr, origLineNbr, origSplitLineNbr> { }
                public class OriginalPlanType : INPlanType.PK.ForeignKeyOf<SOShipLineSplit>.By<origPlanType> { }
                public class OriginalLineSplit : SOLineSplit.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType, origOrderNbr, origLineNbr, origSplitLineNbr> { }
                public class Operation : SOOrderTypeOperation.PK.ForeignKeyOf<SOShipLineSplit>.By<origOrderType, operation> { }
                //todo public class UnitOfMeasure : INUnit.UK.ByInventory.ForeignKeyOf<SOShipLineSplit>.By<inventoryID, uOM> { }
            }
            #endregion

            #region ShipmentNbr
            public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
            protected String _ShipmentNbr;
            [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
            [PXDBDefault(typeof(SOShipment.shipmentNbr))]
			[PXParent(typeof(Select<SOShipment, Where<SOShipment.shipmentNbr, Equal<Current<SOShipLineSplit.shipmentNbr>>>>))]
            [PXParent(typeof(Select<SOShipLine, Where<SOShipLine.shipmentNbr, Equal<Current<SOShipLineSplit.shipmentNbr>>, And<SOShipLine.lineNbr, Equal<Current<SOShipLineSplit.lineNbr>>>>>))]
            public virtual String ShipmentNbr
            {
                get
                {
                    return this._ShipmentNbr;
                }
                set
                {
                    this._ShipmentNbr = value;
                }
            }
            #endregion
            #region LineNbr
            public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
            protected Int32? _LineNbr;
            [PXDBInt(IsKey = true)]
            [PXDefault(typeof(SOShipLine.lineNbr))]
            public virtual Int32? LineNbr
            {
                get
                {
                    return this._LineNbr;
                }
                set
                {
                    this._LineNbr = value;
                }
            }
            #endregion
            #region OrigOrderType
            public abstract class origOrderType : PX.Data.BQL.BqlString.Field<origOrderType> { }
            protected String _OrigOrderType;
            [PXDBString(2, IsFixed = true)]
            [PXDefault(typeof(SOShipLine.origOrderType))]
            public virtual String OrigOrderType
            {
                get
                {
                    return this._OrigOrderType;
                }
                set
                {
                    this._OrigOrderType = value;
                }
            }
            #endregion
            #region OrigOrderNbr
            public abstract class origOrderNbr : PX.Data.BQL.BqlString.Field<origOrderNbr> { }
            protected String _OrigOrderNbr;
            [PXDBString(15, IsUnicode = true)]
            [PXDefault(typeof(SOShipLine.origOrderNbr))]
            public virtual String OrigOrderNbr
            {
                get
                {
                    return this._OrigOrderNbr;
                }
                set
                {
                    this._OrigOrderNbr = value;
                }
            }
            #endregion
            #region OrigLineNbr
            public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
            protected Int32? _OrigLineNbr;
            [PXDBInt()]
            [PXDefault(typeof(SOShipLine.origLineNbr))]
            public virtual Int32? OrigLineNbr
            {
                get
                {
                    return this._OrigLineNbr;
                }
                set
                {
                    this._OrigLineNbr = value;
                }
            }
            #endregion
            #region OrigSplitLineNbr
            public abstract class origSplitLineNbr : PX.Data.BQL.BqlInt.Field<origSplitLineNbr> { }
            protected Int32? _OrigSplitLineNbr;
            [PXDBInt()]
            [PXDefault(typeof(SOShipLine.origSplitLineNbr), PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual Int32? OrigSplitLineNbr
            {
                get
                {
                    return this._OrigSplitLineNbr;
                }
                set
                {
                    this._OrigSplitLineNbr = value;
                }
            }
            #endregion
            #region OrigPlanType
            public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
            [PXDBString(2, IsFixed = true)]
            [PXDefault(typeof(SOShipLine.origPlanType))]
            [PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
            public virtual String OrigPlanType
            {
                get;
                set;
            }
            #endregion
            #region Operation
            public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
            protected String _Operation;
            [PXDBString(1, IsFixed = true, InputMask = ">a")]
            [PXDefault(typeof(SOShipLine.operation))]
            public virtual String Operation
            {
                get
                {
                    return this._Operation;
                }
                set
                {
                    this._Operation = value;
                }
            }
            #endregion
            #region SplitLineNbr
            public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
            protected Int32? _SplitLineNbr;
            [PXDBInt(IsKey = true)]
            [PXDefault()]
            public virtual Int32? SplitLineNbr
            {
                get
                {
                    return this._SplitLineNbr;
                }
                set
                {
                    this._SplitLineNbr = value;
                }
            }
            #endregion
            #region InvtMult
            public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
            protected Int16? _InvtMult;
            [PXDBShort()]
            [PXDefault(typeof(INTran.invtMult))]
            public virtual Int16? InvtMult
            {
                get
                {
                    return this._InvtMult;
                }
                set
                {
                    this._InvtMult = value;
                }
            }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
            protected Int32? _InventoryID;
            [Inventory(Enabled = false, Visible = true)]
            [PXDefault(typeof(SOShipLine.inventoryID))]
			[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
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
            #region LineType
            public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
            protected String _LineType;
            [PXDBString(2, IsFixed = true)]
            [PXDefault(typeof(SOShipLine.lineType))]
            public virtual String LineType
            {
                get
                {
                    return this._LineType;
                }
                set
                {
                    this._LineType = value;
                }
            }
            #endregion
            #region IsStockItem
            public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
            [PXDBBool()]
            [PXFormula(typeof(Selector<SOShipLineSplit.inventoryID, InventoryItem.stkItem>))]
            public bool? IsStockItem
            {
                get;
                set;
            }
            #endregion
            #region IsComponentItem
            public abstract class isComponentItem : PX.Data.BQL.BqlBool.Field<isComponentItem> { }
            [PXDBBool()]
            [PXFormula(typeof(Switch<Case<Where<SOShipLineSplit.inventoryID, Equal<Current<SOShipLine.inventoryID>>>, False>, True>))]
            public bool? IsComponentItem
            {
                get;
                set;
            }
            #endregion
			#region IsIntercompany
			public abstract class isIntercompany : Data.BQL.BqlBool.Field<isIntercompany> { }
			[PXDBBool]
			[PXDefault(typeof(SOShipLine.isIntercompany))]
			public virtual bool? IsIntercompany
			{
				get;
				set;
			}
			#endregion
            #region TranType
            public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
            protected String _TranType;
            [PXFormula(typeof(Selector<SOShipLine.operation, SOOrderTypeOperation.iNDocType>))]
            [PXString(SOOrderTypeOperation.iNDocType.Length, IsFixed = true)]
            public virtual String TranType
            {
                get
                {
                    return this._TranType;
                }
                set
                {
                    this._TranType = value;
                }
            }
            #endregion
            #region TranDate
            public virtual DateTime? TranDate
            {
                get { return this._ShipDate; }
            }
            #endregion
            #region PlanType
            public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
            protected String _PlanType;
            [PXDBString(2, IsFixed = true)]
            [PXDefault(typeof(SOShipLine.planType))]
            public virtual String PlanType
            {
                get
                {
                    return this._PlanType;
                }
                set
                {
                    this._PlanType = value;
                }
            }
            #endregion
            #region PlanID
            public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
            protected Int64? _PlanID;
            [PXDBLong(IsImmutable = true)]
            public virtual Int64? PlanID
            {
                get
                {
                    return this._PlanID;
                }
                set
                {
                    this._PlanID = value;
                }
            }
            #endregion
            #region SiteID
            public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
            protected Int32? _SiteID;
            [Site(Enabled = false, Visible = false)]
            [PXDefault(typeof(SOShipLine.siteID))]
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
            #region LocationID
            public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
            protected Int32? _LocationID;
            [SOLocationAvail(typeof(SOShipLineSplit.inventoryID), typeof(SOShipLineSplit.subItemID), typeof(SOShipLine.costCenterID), typeof(SOShipLineSplit.siteID), typeof(SOShipLineSplit.tranType), typeof(SOShipLineSplit.invtMult))]
            [PXDefault()]
            public virtual Int32? LocationID
            {
                get
                {
                    return this._LocationID;
                }
                set
                {
                    this._LocationID = value;
                }
            }
            #endregion
            #region SubItemID
            public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
            protected Int32? _SubItemID;
            [IN.SubItem(typeof(SOShipLineSplit.inventoryID))]
            [PXDefault()]
            public virtual Int32? SubItemID
            {
                get
                {
                    return this._SubItemID;
                }
                set
                {
                    this._SubItemID = value;
                }
            }
            #endregion
            #region LotSerialNbr
            public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
            protected String _LotSerialNbr;
            [SOShipLotSerialNbr(typeof(SOShipLineSplit.siteID), typeof(SOShipLineSplit.inventoryID), typeof(SOShipLineSplit.subItemID), typeof(SOShipLineSplit.locationID), typeof(SOShipLine.lotSerialNbr), typeof(SOShipLine.costCenterID))]
            public virtual String LotSerialNbr
            {
                get
                {
                    return this._LotSerialNbr;
                }
                set
                {
                    this._LotSerialNbr = value;
                }
            }
            #endregion
            #region LastLotSerialNbr
            public abstract class lastLotSerialNbr : PX.Data.BQL.BqlString.Field<lastLotSerialNbr> { }
            protected String _LastLotSerialNbr;
            [PXString(100, IsUnicode = true)]
            public virtual String LastLotSerialNbr
            {
                get
                {
                    return this._LastLotSerialNbr;
                }
                set
                {
                    this._LastLotSerialNbr = value;
                }
            }
            #endregion
            #region LotSerClassID
            public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }
            protected String _LotSerClassID;
            [PXString(10, IsUnicode = true)]
            public virtual String LotSerClassID
            {
                get
                {
                    return this._LotSerClassID;
                }
                set
                {
                    this._LotSerClassID = value;
                }
            }
            #endregion
            #region AssignedNbr
            public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr> { }
            protected String _AssignedNbr;
            [PXString(30, IsUnicode = true)]
            public virtual String AssignedNbr
            {
                get
                {
                    return this._AssignedNbr;
                }
                set
                {
                    this._AssignedNbr = value;
                }
            }
            #endregion
            #region HasGeneratedLotSerialNbr
            public abstract class hasGeneratedLotSerialNbr : PX.Data.BQL.BqlBool.Field<hasGeneratedLotSerialNbr> { }
            [PXDBBool]
            [PXDefault(false)]
            [Common.Attributes.HasFieldBeenModified(typeof(lotSerialNbr), OriginalValueField = typeof(OriginalValues.originalLotSerialNbr), InvertResult = true)]
            public bool? HasGeneratedLotSerialNbr
            {
                get;
                set;
            }
            #endregion
            #region ExpireDate
            public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
            protected DateTime? _ExpireDate;
            [SOShipExpireDate(typeof(SOShipLineSplit.inventoryID))]
            public virtual DateTime? ExpireDate
            {
                get
                {
                    return this._ExpireDate;
                }
                set
                {
                    this._ExpireDate = value;
                }
            }
            #endregion
            #region UOM
            public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
            protected String _UOM;
            [INUnit(typeof(SOShipLineSplit.inventoryID), DisplayName = "UOM", Enabled = false)]
            [PXDefault]
            public virtual String UOM
            {
                get
                {
                    return this._UOM;
                }
                set
                {
                    this._UOM = value;
                }
            }
            #endregion
            #region Qty
            public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
            protected Decimal? _Qty;
            [PXDBQuantity(typeof(SOShipLineSplit.uOM), typeof(SOShipLineSplit.baseQty), InventoryUnitType.BaseUnit)]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Quantity")]
            public virtual Decimal? Qty
            {
                get
                {
                    return this._Qty;
                }
                set
                {
                    this._Qty = value;
                }
            }
            #endregion
            #region BaseQty
            public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
            protected Decimal? _BaseQty;
            [PXDBDecimal(6)]
            public virtual Decimal? BaseQty
            {
                get
                {
                    return this._BaseQty;
                }
                set
                {
                    this._BaseQty = value;
                }
            }
			#endregion
			#region PickedQty
			[PXDBQuantity(typeof(uOM), typeof(basePickedQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Picked Quantity")]
			public virtual Decimal? PickedQty { get; set; }
			public abstract class pickedQty : PX.Data.BQL.BqlDecimal.Field<pickedQty> { }
			#endregion
			#region BasePickedQty
			[PXDBDecimal(6)]
			public virtual Decimal? BasePickedQty { get; set; }
			public abstract class basePickedQty : PX.Data.BQL.BqlDecimal.Field<basePickedQty> { }
			#endregion
			#region PackedQty
			[PXDBQuantity(typeof(uOM), typeof(basePackedQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Packed Quantity")]
			public virtual Decimal? PackedQty { get; set; }
			public abstract class packedQty : PX.Data.BQL.BqlDecimal.Field<packedQty> { }
			#endregion
			#region BasePackedQty
			[PXDBDecimal(6)]
			public virtual Decimal? BasePackedQty { get; set; }
			public abstract class basePackedQty : PX.Data.BQL.BqlDecimal.Field<basePackedQty> { }
			#endregion
			#region ShipDate
			public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
            protected DateTime? _ShipDate;
            [PXDBDate()]
            [PXDBDefault(typeof(SOShipment.shipDate))]
            public virtual DateTime? ShipDate
            {
                get
                {
                    return this._ShipDate;
                }
                set
                {
                    this._ShipDate = value;
                }
            }
            #endregion
            #region Confirmed
            public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }
            protected Boolean? _Confirmed;
            [PXDBBool()]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Confirmed")]
            public virtual Boolean? Confirmed
            {
                get
                {
                    return this._Confirmed;
                }
                set
                {
                    this._Confirmed = value;
                }
            }
            #endregion
            #region Released
            public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
            protected Boolean? _Released;
            [PXDBBool()]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Released")]
            public virtual Boolean? Released
            {
                get
                {
                    return this._Released;
                }
                set
                {
                    this._Released = value;
                }
            }
            #endregion
            #region IsUnassigned
            public abstract class isUnassigned : PX.Data.BQL.BqlBool.Field<isUnassigned> { }
            protected Boolean? _IsUnassigned;
            [PXDBBool()]
            [PXDefault(typeof(SOShipLine.isUnassigned))]
            public virtual Boolean? IsUnassigned
            {
                get
                {
                    return this._IsUnassigned;
                }
                set
                {
                    this._IsUnassigned = value;
                }
            }
            #endregion
            #region ProjectID
            public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
            protected Int32? _ProjectID;
            [PXInt]
            public virtual Int32? ProjectID
            {
                get
                {
                    return this._ProjectID;
                }
                set
                {
                    this._ProjectID = value;
                }
            }
            #endregion
            #region TaskID
            public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
            protected Int32? _TaskID;
            [PXInt]
            public virtual Int32? TaskID
            {
                get
                {
                    return this._TaskID;
                }
                set
                {
                    this._TaskID = value;
                }
            }
            #endregion
            #region CreatedByID
            public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
            protected Guid? _CreatedByID;
            [PXDBCreatedByID()]
            public virtual Guid? CreatedByID
            {
                get
                {
                    return this._CreatedByID;
                }
                set
                {
                    this._CreatedByID = value;
                }
            }
            #endregion
            #region CreatedByScreenID
            public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
            protected String _CreatedByScreenID;
            [PXDBCreatedByScreenID()]
            public virtual String CreatedByScreenID
            {
                get
                {
                    return this._CreatedByScreenID;
                }
                set
                {
                    this._CreatedByScreenID = value;
                }
            }
            #endregion
            #region CreatedDateTime
            public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
            protected DateTime? _CreatedDateTime;
            [PXDBCreatedDateTime()]
            public virtual DateTime? CreatedDateTime
            {
                get
                {
                    return this._CreatedDateTime;
                }
                set
                {
                    this._CreatedDateTime = value;
                }
            }
            #endregion
            #region LastModifiedByID
            public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
            protected Guid? _LastModifiedByID;
            [PXDBLastModifiedByID()]
            public virtual Guid? LastModifiedByID
            {
                get
                {
                    return this._LastModifiedByID;
                }
                set
                {
                    this._LastModifiedByID = value;
                }
            }
            #endregion
            #region LastModifiedByScreenID
            public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
            protected String _LastModifiedByScreenID;
            [PXDBLastModifiedByScreenID()]
            public virtual String LastModifiedByScreenID
            {
                get
                {
                    return this._LastModifiedByScreenID;
                }
                set
                {
                    this._LastModifiedByScreenID = value;
                }
            }
            #endregion
            #region LastModifiedDateTime
            public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
            protected DateTime? _LastModifiedDateTime;
            [PXDBLastModifiedDateTime()]
            public virtual DateTime? LastModifiedDateTime
            {
                get
                {
                    return this._LastModifiedDateTime;
                }
                set
                {
                    this._LastModifiedDateTime = value;
                }
            }
            #endregion
            #region tstamp
            public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
            protected Byte[] _tstamp;
            [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
            public virtual Byte[] tstamp
            {
                get
                {
                    return this._tstamp;
                }
                set
                {
                    this._tstamp = value;
                }
            }
			#endregion

			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public sealed class OriginalValues : PXCacheExtension<SOShipLineSplit>
            {
                #region OriginalLotSerialNbr
                [PXString]
                [PXDBCalced(typeof(lotSerialNbr), typeof(string))]
                public String OriginalLotSerialNbr { get; set; }
                public abstract class originalLotSerialNbr : PX.Data.BQL.BqlString.Field<originalLotSerialNbr> { }
                #endregion
            }
        }
    }

	namespace Report
	{
		[PXCacheName(Messages.SOShipLineSplitForPacking)]
		[PXProjection(typeof(
			SelectFrom<Table.SOShipLineSplit>.
			AggregateTo<
				GroupBy<Table.SOShipLineSplit.shipmentNbr>,
				GroupBy<Table.SOShipLineSplit.inventoryID>,
				GroupBy<Table.SOShipLineSplit.subItemID>,
				GroupBy<Table.SOShipLineSplit.lotSerialNbr>,
				Sum<Table.SOShipLineSplit.qty>,
				Sum<Table.SOShipLineSplit.baseQty>,
				Sum<Table.SOShipLineSplit.pickedQty>,
				Sum<Table.SOShipLineSplit.basePickedQty>,
				Sum<Table.SOShipLineSplit.packedQty>,
				Sum<Table.SOShipLineSplit.basePackedQty>
			>), Persistent = false)]
		public class SOShipLineSplitForPacking : IBqlTable
		{
			#region ShipmentNbr
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual String ShipmentNbr { get; set; }
			public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
			#endregion
			#region LineNbr
			[PXDBInt(IsKey = true, BqlTable = typeof(Table.SOShipLineSplit))]
			[PXUIField(DisplayName = "Line Nbr.", Enabled = false, Visible = false)]
			public virtual Int32? LineNbr { get; set; }
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			#endregion
			#region OrigOrderType
			[PXDBString(2, IsFixed = true, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual String OrigOrderType { get; set; }
			public abstract class origOrderType : PX.Data.BQL.BqlString.Field<origOrderType> { }
			#endregion
			#region OrigOrderNbr
			[PXDBString(15, IsUnicode = true, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual String OrigOrderNbr { get; set; }
			public abstract class origOrderNbr : PX.Data.BQL.BqlString.Field<origOrderNbr> { }
			#endregion
			#region OrigLineNbr
			[PXDBInt(BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual Int32? OrigLineNbr { get; set; }
			public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
			#endregion
			#region OrigSplitLineNbr
			[PXDBInt(BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual Int32? OrigSplitLineNbr { get; set; }
			public abstract class origSplitLineNbr : PX.Data.BQL.BqlInt.Field<origSplitLineNbr> { }
			#endregion
			#region OrigPlanType
			[PXDBString(2, IsFixed = true, BqlTable = typeof(Table.SOShipLineSplit))]
			[PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
			public virtual String OrigPlanType { get; set; }
			public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
			#endregion
			#region Operation
			[PXDBString(1, IsFixed = true, InputMask = ">a", BqlTable = typeof(Table.SOShipLineSplit))]
			[PXSelector(typeof(Search<SOOrderTypeOperation.operation, Where<SOOrderTypeOperation.orderType, Equal<Current<origOrderType>>>>))]
			public virtual String Operation { get; set; }
			public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
			#endregion
			#region SplitLineNbr
			[PXDBInt(IsKey = true, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual Int32? SplitLineNbr { get; set; }
			public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
			#endregion
			#region InvtMult
			[PXDBShort(BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual Int16? InvtMult { get; set; }
			public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
			#endregion
			#region InventoryID
			[Inventory(Enabled = false, Visible = true, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual Int32? InventoryID { get; set; }
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			#endregion
			#region LineType
			[PXDBString(2, IsFixed = true, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual String LineType { get; set; }
			public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
			#endregion
			#region IsStockItem
			[PXDBBool(BqlTable = typeof(Table.SOShipLineSplit))]
			[PXFormula(typeof(Selector<inventoryID, InventoryItem.stkItem>))]
			public bool? IsStockItem { get; set; }
			public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
			#endregion
			#region IsComponentItem
			[PXDBBool(BqlTable = typeof(Table.SOShipLineSplit))]
			[PXFormula(typeof(Switch<Case<Where<inventoryID, Equal<Current<SOShipLine.inventoryID>>>, False>, True>))]
			public bool? IsComponentItem { get; set; }
			public abstract class isComponentItem : PX.Data.BQL.BqlBool.Field<isComponentItem> { }
			#endregion
			#region TranType
			[PXString(SOOrderTypeOperation.iNDocType.Length, IsFixed = true, BqlTable = typeof(Table.SOShipLineSplit))]
			[PXFormula(typeof(Selector<operation, SOOrderTypeOperation.iNDocType>))]
			public virtual String TranType { get; set; }
			public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
			#endregion
			#region TranDate
			public virtual DateTime? TranDate => ShipDate;
			#endregion
			#region PlanType
			[PXDBString(2, IsFixed = true, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual String PlanType { get; set; }
			public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
			#endregion
			#region PlanID
			[PXDBLong(IsImmutable = true, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual Int64? PlanID { get; set; }
			public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
			#endregion
			#region SiteID
			[Site(BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual Int32? SiteID { get; set; }
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			#endregion
			#region SubItemID
			[SubItem(typeof(inventoryID), BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual Int32? SubItemID { get; set; }
			public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
			#endregion
			#region LotSerialNbr
			[PXDBString(BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual String LotSerialNbr { get; set; }
			public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
			#endregion
			#region LastLotSerialNbr
			[PXString(100, IsUnicode = true, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual String LastLotSerialNbr { get; set; }
			public abstract class lastLotSerialNbr : PX.Data.BQL.BqlString.Field<lastLotSerialNbr> { }
			#endregion
			#region LotSerClassID
			[PXString(10, IsUnicode = true, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual String LotSerClassID { get; set; }
			public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }
			#endregion
			#region AssignedNbr
			[PXString(30, IsUnicode = true, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual String AssignedNbr { get; set; }
			public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr> { }
			#endregion
			#region ExpireDate
			[PXDBDate(BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual DateTime? ExpireDate { get; set; }
			public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
			#endregion
			#region UOM
			[INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual String UOM { get; set; }
			public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
			#endregion
			#region Qty
			[PXDBQuantity(typeof(uOM), typeof(baseQty), BqlTable = typeof(Table.SOShipLineSplit))]
			[PXUIField(DisplayName = "Quantity")]
			public virtual Decimal? Qty { get; set; }
			public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
			#endregion
			#region BaseQty
			[PXDBDecimal(6, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual Decimal? BaseQty { get; set; }
			public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
			#endregion
			#region PickedQty
			[PXDBQuantity(typeof(uOM), typeof(basePickedQty), BqlTable = typeof(Table.SOShipLineSplit))]
			[PXUIField(DisplayName = "Picked Quantity", Enabled = false)]
			public virtual Decimal? PickedQty { get; set; }
			public abstract class pickedQty : PX.Data.BQL.BqlDecimal.Field<pickedQty> { }
			#endregion
			#region BasePickedQty
			[PXDBDecimal(6, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual Decimal? BasePickedQty { get; set; }
			public abstract class basePickedQty : PX.Data.BQL.BqlDecimal.Field<basePickedQty> { }
			#endregion
			#region PackedQty
			[PXDBQuantity(typeof(uOM), typeof(basePackedQty), BqlTable = typeof(Table.SOShipLineSplit))]
			[PXUIField(DisplayName = "Packed Quantity", Enabled = false)]
			public virtual Decimal? PackedQty { get; set; }
			public abstract class packedQty : PX.Data.BQL.BqlDecimal.Field<packedQty> { }
			#endregion
			#region BasePackedQty
			[PXDBDecimal(6, BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual Decimal? BasePackedQty { get; set; }
			public abstract class basePackedQty : PX.Data.BQL.BqlDecimal.Field<basePackedQty> { }
			#endregion
			#region ShipDate
			[PXDBDate(BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual DateTime? ShipDate { get; set; }
			public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
			#endregion
			#region Confirmed
			[PXDBBool(BqlTable = typeof(Table.SOShipLineSplit))]
			[PXUIField(DisplayName = "Confirmed")]
			public virtual Boolean? Confirmed { get; set; }
			public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }
			#endregion
			#region Released
			[PXDBBool(BqlTable = typeof(Table.SOShipLineSplit))]
			[PXUIField(DisplayName = "Released")]
			public virtual Boolean? Released { get; set; }
			public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			#endregion
			#region IsUnassigned
			[PXDBBool(BqlTable = typeof(Table.SOShipLineSplit))]
			public virtual Boolean? IsUnassigned { get; set; }
			public abstract class isUnassigned : PX.Data.BQL.BqlBool.Field<isUnassigned> { }
			#endregion
		}
	}
}
