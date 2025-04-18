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
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.CS;
	using PX.Objects.EP;
	using PX.Objects.GL;

	/// <exclude />
	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(SOSetupMaint))]
    [PXCacheName(Messages.SOSetup)]
    public partial class SOSetup : PX.Data.IBqlTable
	{
		#region Keys
		public static class FK
		{
			public class ShipmentNumbering : Numbering.PK.ForeignKeyOf<SOSetup>.By<shipmentNumberingID> { }
			public class PickingWorksheetNumbering : Numbering.PK.ForeignKeyOf<SOSetup>.By<pickingWorksheetNumberingID> { }
			public class DefaultSalesOrderAssignmentMap : EPAssignmentMap.PK.ForeignKeyOf<SOSetup>.By<defaultOrderAssignmentMapID> { }
			public class DefaultSalesOrderShipmentAssignmentMap : EPAssignmentMap.PK.ForeignKeyOf<SOSetup>.By<defaultShipmentAssignmentMapID> { }
			public class DefaultOrderType : SOOrderType.PK.ForeignKeyOf<SOSetup>.By<defaultOrderType> { }
			public class TransferOrderType : SOOrderType.PK.ForeignKeyOf<SOSetup>.By<transferOrderType> { }

		}
		#endregion

		#region ShipmentNumberingID
		public abstract class shipmentNumberingID : PX.Data.BQL.BqlString.Field<shipmentNumberingID> { }
		protected String _ShipmentNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("SOSHIPMENT")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Shipment Numbering Sequence")]
		public virtual String ShipmentNumberingID
		{
			get
			{
				return this._ShipmentNumberingID;
			}
			set
			{
				this._ShipmentNumberingID = value;
			}
		}
		#endregion
		#region PickingWorksheetNumberingID
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("PICKWORKSH", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Picking Worksheet Numbering Sequence")]
		public virtual String PickingWorksheetNumberingID { get; set; }
		public abstract class pickingWorksheetNumberingID : PX.Data.BQL.BqlString.Field<pickingWorksheetNumberingID> { }
		#endregion
		#region HoldShipments
		public abstract class holdShipments : PX.Data.BQL.BqlBool.Field<holdShipments> { }
		protected Boolean? _HoldShipments;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Shipments on Entry")]
		public virtual Boolean? HoldShipments
		{
			get
			{
				return this._HoldShipments;
			}
			set
			{
				this._HoldShipments = value;
			}
		}
		#endregion
        #region OrderRequestApproval
        public abstract class orderRequestApproval : PX.Data.BQL.BqlBool.Field<orderRequestApproval> { }
        protected bool? _OrderRequestApproval;
        [EPRequireApproval]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Require Approval")]
        public virtual bool? OrderRequestApproval
        {
            get
            {
                return this._OrderRequestApproval;
            }
            set
            {
                this._OrderRequestApproval = value;
            }
        }
        #endregion	
		#region RequireShipmentTotal
		public abstract class requireShipmentTotal : PX.Data.BQL.BqlBool.Field<requireShipmentTotal> { }
		protected Boolean? _RequireShipmentTotal;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Validate Shipment Total on Confirmation")]
		public virtual Boolean? RequireShipmentTotal
		{
			get
			{
				return this._RequireShipmentTotal;
			}
			set
			{
				this._RequireShipmentTotal = value;
			}
		}
		#endregion
		#region AddAllToShipment
		public abstract class addAllToShipment : PX.Data.BQL.BqlBool.Field<addAllToShipment> { }
		protected Boolean? _AddAllToShipment;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Add Zero Lines for Items Not in Stock")]
		public virtual Boolean? AddAllToShipment
		{
			get
			{
				return this._AddAllToShipment;
			}
			set
			{
				this._AddAllToShipment = value;
			}
		}
		#endregion
		#region CreateZeroShipments
		public abstract class createZeroShipments : PX.Data.BQL.BqlBool.Field<createZeroShipments> { }
		protected Boolean? _CreateZeroShipments;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Create Zero Shipments")]
		[PXFormula(typeof(Switch<Case<Where<SOSetup.addAllToShipment, Equal<False>>, False>, SOSetup.createZeroShipments>))]
		public virtual Boolean? CreateZeroShipments
		{
			get
			{
				return this._CreateZeroShipments;
			}
			set
			{
				this._CreateZeroShipments = value;
			}
		}
		#endregion
		#region AutoReleaseIN
		public abstract class autoReleaseIN : PX.Data.BQL.BqlBool.Field<autoReleaseIN> { }
		protected bool? _AutoReleaseIN;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Release IN Documents")]
		public virtual bool? AutoReleaseIN
		{
			get
			{
				return this._AutoReleaseIN;
			}
			set
			{
				this._AutoReleaseIN = value;
			}
		}
		#endregion
		#region DefaultOrderAssignmentMapID
		public abstract class defaultOrderAssignmentMapID : PX.Data.BQL.BqlInt.Field<defaultOrderAssignmentMapID> { }
		protected int? _DefaultOrderAssignmentMapID;
		[PXDBInt]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID, Where<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeSalesOrder>>>))]
		[PXUIField(DisplayName = "Default Sales Order Assignment Map")]
		public virtual int? DefaultOrderAssignmentMapID
		{
			get
			{
				return this._DefaultOrderAssignmentMapID;
			}
			set
			{
				this._DefaultOrderAssignmentMapID = value;
			}
		}
		#endregion
		#region DefaultShipmentAssignmentMapID
		public abstract class defaultShipmentAssignmentMapID : PX.Data.BQL.BqlInt.Field<defaultShipmentAssignmentMapID> { }
		protected int? _DefaultShipmentAssignmentMapID;
		[PXDBInt]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID, Where<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeSalesOrderShipment>>>))]
		[PXUIField(DisplayName = "Default Sales Order Shipment Assignment Map")]
		public virtual int? DefaultShipmentAssignmentMapID
		{
			get
			{
				return this._DefaultShipmentAssignmentMapID;
			}
			set
			{
				this._DefaultShipmentAssignmentMapID = value;
			}
		}
		#endregion

		#region ProrateDiscounts
		public abstract class prorateDiscounts : PX.Data.BQL.BqlBool.Field<prorateDiscounts> { }
		protected Boolean? _ProrateDiscounts;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Prorate Discounts")]
		public virtual Boolean? ProrateDiscounts
		{
			get
			{
				return this._ProrateDiscounts;
			}
			set
			{
				this._ProrateDiscounts = value;
			}
		}
		#endregion
		#region FreeItemShipping
		public abstract class freeItemShipping : PX.Data.BQL.BqlString.Field<freeItemShipping> { }
		protected String _FreeItemShipping;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(FreeItemShipType.Proportional)]
		[FreeItemShipType.List()]
		[PXUIField(DisplayName = "Free Item Shipping", Visibility = PXUIVisibility.Visible, Required = true)]
		public virtual String FreeItemShipping
		{
			get
			{
				return this._FreeItemShipping;
			}
			set
			{
				this._FreeItemShipping = value;
			}
		}
		#endregion
		#region FreightOption
		public abstract class freightAllocation : PX.Data.BQL.BqlString.Field<freightAllocation> { }
		protected String _FreightAllocation;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(FreightAllocationList.FullAmount)]
		[FreightAllocationList.List()]
		[PXUIField(DisplayName = "Freight Allocation on Partial Shipping", Visibility = PXUIVisibility.Visible)]
		public virtual String FreightAllocation
		{
			get
			{
				return this._FreightAllocation;
			}
			set
			{
				this._FreightAllocation = value;
			}
		}
		#endregion
		#region MinGrossProfitValidation
		public abstract class minGrossProfitValidation : PX.Data.BQL.BqlString.Field<minGrossProfitValidation> { }
		protected String _MinGrossProfitValidation;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(MinGrossProfitValidationType.Warning)]
		[MinGrossProfitValidationType.List()]
        [PXUIField(DisplayName = "Validate Min. Markup", Visibility = PXUIVisibility.Visible)]
		public virtual String MinGrossProfitValidation
		{
			get
			{
				return this._MinGrossProfitValidation;
			}
			set
			{
				this._MinGrossProfitValidation = value;
			}
		}
		#endregion
		#region UsePriceAdjustmentMultiplier
		public abstract class usePriceAdjustmentMultiplier : PX.Data.BQL.BqlBool.Field<usePriceAdjustmentMultiplier> { }
		protected Boolean? _UsePriceAdjustmentMultiplier;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use a Price Adjustment Multiplier")]
		public virtual Boolean? UsePriceAdjustmentMultiplier
		{
			get
			{
				return this._UsePriceAdjustmentMultiplier;
			}
			set
			{
				this._UsePriceAdjustmentMultiplier = value;
			}
		}
		#endregion
		#region IgnoreMinGrossProfitCustomerPrice
		public abstract class ignoreMinGrossProfitCustomerPrice : IBqlField
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Customer")]
		public virtual bool? IgnoreMinGrossProfitCustomerPrice
		{
			get;
			set;
		}
		#endregion
		#region IgnoreMinGrossProfitCustomerPriceClass
		public abstract class ignoreMinGrossProfitCustomerPriceClass : IBqlField
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Customer Price Class")]
		public virtual bool? IgnoreMinGrossProfitCustomerPriceClass
		{
			get;
			set;
		}
		#endregion
		#region IgnoreMinGrossProfitPromotionalPrice
		public abstract class ignoreMinGrossProfitPromotionalPrice : IBqlField
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Promotional Prices")]
		public virtual bool? IgnoreMinGrossProfitPromotionalPrice
		{
			get;
			set;
		}
		#endregion
		#region DefaultOrderType
		public abstract class defaultOrderType : PX.Data.BQL.BqlString.Field<defaultOrderType> { }
		protected String _DefaultOrderType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Default Sales Order Type")]
		[PXDefault(SOOrderTypeConstants.SalesOrder, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<SOOrderType.orderType>),DescriptionField = typeof(SOOrderType.descr))]
        [PXRestrictor(typeof(Where<SOOrderType.active, Equal<True>>),Messages.OrderTypeInactive, typeof(SOOrderType.orderType))]
		public virtual String DefaultOrderType
		{
			get
			{
				return this._DefaultOrderType;
			}
			set
			{
				this._DefaultOrderType = value;
			}
		}
		#endregion
		#region TransferOrderType
		public abstract class transferOrderType : PX.Data.BQL.BqlString.Field<transferOrderType> { }
		protected String _TransferOrderType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Default Transfer Order Type")]
		[PXDefault(SOOrderTypeConstants.TransferOrder, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<SOOrderType.orderType, 
			InnerJoin<SOOrderTypeOperation, 
						 On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>,
						 And<SOOrderTypeOperation.iNDocType, Equal<IN.INTranType.transfer>>>>>), DescriptionField = typeof(SOOrderType.descr))]
		public virtual String TransferOrderType
		{
			get
			{
				return this._TransferOrderType;
			}
			set
			{
				this._TransferOrderType = value;
			}
		}
		#endregion
		#region DefaultReturnOrderType
		public abstract class defaultReturnOrderType : PX.Data.BQL.BqlString.Field<defaultReturnOrderType> { }

		///<summary>
		///	Gets or sets the data field from the drop-down that will be used as the default type for Return order.
		/// </summary>
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Default Return Order Type", FieldClass = FeaturesSet.caseManagement.FieldClass)]
		[PXDefault(SOOrderTypeConstants.RMAOrder, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<SOOrderType.orderType, Where<SOOrderType.behavior, Equal<SOBehavior.rM>>>), DescriptionField = typeof(SOOrderType.descr))]
		[PXRestrictor(typeof(Where<SOOrderType.active, Equal<True>>), Messages.OrderTypeInactive, typeof(SOOrderType.orderType), ShowWarning = true)]
		public virtual String DefaultReturnOrderType { get; set; }
		#endregion
		#region CreditCheckError
		public abstract class creditCheckError : PX.Data.BQL.BqlBool.Field<creditCheckError> { }
		protected Boolean? _CreditCheckError;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Invoices on Failed Credit Check", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? CreditCheckError
		{
			get
			{
				return this._CreditCheckError;
			}
			set
			{
				this._CreditCheckError = value;
			}
		}
		#endregion
		#region UseShipDateForInvoiceDate
		public abstract class useShipDateForInvoiceDate : PX.Data.BQL.BqlBool.Field<useShipDateForInvoiceDate> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Shipment Date for Invoice Date")]
		public virtual bool? UseShipDateForInvoiceDate
		{
			get;
			set;
		}
		#endregion
		#region AdvancedAvailCheck
		public abstract class advancedAvailCheck : PX.Data.BQL.BqlBool.Field<advancedAvailCheck> { }
		protected Boolean? _AdvancedAvailCheck;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Advanced Availability Validation")]
		public virtual Boolean? AdvancedAvailCheck
		{
			get
			{
				return this._AdvancedAvailCheck;
			}
			set
			{
				this._AdvancedAvailCheck = value;
			}
		}
		#endregion

		#region SalesProfitabilityForNSKits
		public abstract class salesProfitabilityForNSKits : PX.Data.BQL.BqlString.Field<salesProfitabilityForNSKits> { }
		protected String _SalesProfitabilityForNSKits;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SalesProfitabilityNSKitMethod.NSKitStandardAndStockComponentsCost)]
		[SalesProfitabilityNSKitMethod.List()]
		[PXUIField(DisplayName = "Cost Calculation Basis for Non-Stock Kits", Visibility = PXUIVisibility.Visible)]
		public virtual String SalesProfitabilityForNSKits
		{
			get
			{
				return this._SalesProfitabilityForNSKits;
			}
			set
			{
				this._SalesProfitabilityForNSKits = value;
			}
		}
		#endregion

		#region DfltIntercompanyOrderType
		public abstract class dfltIntercompanyOrderType : Data.BQL.BqlString.Field<dfltIntercompanyOrderType> { }
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Default Type for Intercompany Sales", FieldClass = nameof(FeaturesSet.InterBranch))]
		[PXDefault(SOOrderTypeConstants.SalesOrder)]
		[PXSelector(typeof(Search2<SOOrderType.orderType,
			InnerJoin<SOOrderTypeOperation,
				On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>,
				And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>,
				And<SOOrderTypeOperation.iNDocType, NotEqual<IN.INTranType.transfer>>>>>,
			Where<SOOrderType.behavior, In3<SOBehavior.sO, SOBehavior.iN>>>),
			DescriptionField = typeof(SOOrderType.descr))]
		[PXRestrictor(typeof(Where<SOOrderType.active, Equal<True>>), Messages.OrderTypeInactive, typeof(SOOrderType.orderType))]
		public virtual string DfltIntercompanyOrderType
		{
			get;
			set;
		}
		#endregion
		#region DfltIntercompanyRMAType
		public abstract class dfltIntercompanyRMAType : Data.BQL.BqlString.Field<dfltIntercompanyRMAType> { }
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Default Type for Intercompany Returns", FieldClass = nameof(FeaturesSet.InterBranch))]
		[PXDefault(SOOrderTypeConstants.RMAOrder)]
		[PXSelector(typeof(Search<SOOrderType.orderType,
			Where<SOOrderType.behavior, In3<SOBehavior.rM, SOBehavior.cM>>>),
			DescriptionField = typeof(SOOrderType.descr))]
		[PXRestrictor(typeof(Where<SOOrderType.active, Equal<True>>), Messages.OrderTypeInactive, typeof(SOOrderType.orderType))]
		public virtual string DfltIntercompanyRMAType
		{
			get;
			set;
		}
		#endregion
		#region DisableEditingPricesDiscountsForIntercompany
		public abstract class disableEditingPricesDiscountsForIntercompany : PX.Data.BQL.BqlBool.Field<disableEditingPricesDiscountsForIntercompany> { }
		protected Boolean? _DisableEditingPricesDiscountsForIntercompany;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable Editing Prices and Discounts", FieldClass = nameof(FeaturesSet.InterBranch))]
		public virtual Boolean? DisableEditingPricesDiscountsForIntercompany
		{
			get
			{
				return this._DisableEditingPricesDiscountsForIntercompany;
			}
			set
			{
				this._DisableEditingPricesDiscountsForIntercompany = value;
			}
		}
		#endregion
		#region DisableAddingItemsForIntercompany
		public abstract class disableAddingItemsForIntercompany : Data.BQL.BqlBool.Field<disableAddingItemsForIntercompany> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable Adding Items to Orders", FieldClass = nameof(FeaturesSet.InterBranch))]
		public virtual bool? DisableAddingItemsForIntercompany
		{
			get;
			set;
		}
		#endregion
		#region DeferPriceDiscountRecalculation
		public abstract class deferPriceDiscountRecalculation : PX.Data.BQL.BqlBool.Field<deferPriceDiscountRecalculation> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Defer Discount Recalculation")]
		public virtual Boolean? DeferPriceDiscountRecalculation
		{
			get; set;
		}
		#endregion

		#region ShowOnlyAvailableRelatedItems
		public abstract class showOnlyAvailableRelatedItems : PX.Data.BQL.BqlBool.Field<showOnlyAvailableRelatedItems> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Only Available Items")]
		public virtual bool? ShowOnlyAvailableRelatedItems { get; set; }
		#endregion
		#region UseBaseUomTransferringAllocations
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R1)]
		public abstract class useBaseUomTransferringAllocations : Data.BQL.BqlBool.Field<useBaseUomTransferringAllocations> { }
		[PXDBBool]
		[PXDefault(false)]
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R1)]
		public virtual bool? UseBaseUomTransferringAllocations { get; set; }
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
	}


	public static class MinGrossProfitValidationType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(None, Messages.None),
					Pair(Warning, Messages.Warning),
					Pair(SetToMin, Messages.SetToMin),
				}) {}
		}

		public const string None = "N";
		public const string Warning = "W";
		public const string SetToMin = "S";

	}

	public static class SalesPriceUpdateUnitType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(BaseUnit, Messages.BaseUnit),
					Pair(SalesUnit, Messages.SalesUnit),
				}) {}
		}

		public const string BaseUnit = "B";
		public const string SalesUnit = "S";

	}

	public static class FreeItemShipType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Proportional, Messages.Proportional),
					Pair(OnLastShipment, Messages.OnLastShipment),
				}) {}
		}

		public const string Proportional = "P";
		public const string OnLastShipment = "S";
	}

	public static class FreightAllocationList
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(FullAmount, Messages.FullAmount),
					Pair(Prorate, Messages.Prorate),
				}) {}
		}

		public const string FullAmount = "A";
		public const string Prorate = "P";

		public class fullAmount : PX.Data.BQL.BqlString.Constant<fullAmount>
		{
			public fullAmount() : base(FullAmount) {}
		}

		public class prorate : PX.Data.BQL.BqlString.Constant<prorate>
		{
			public prorate() : base(Prorate) {}
		}
	}

	public static class SalesProfitabilityNSKitMethod
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { StockComponentsCostOnly, NSKitStandardCostOnly, NSKitStandardAndStockComponentsCost },
				new string[] { Messages.StockComponentsCostOnly, Messages.NSKitStandardCostOnly, Messages.NSKitStandardAndStockComponentsCost })
			{ }
		}
		public const string StockComponentsCostOnly = "S";
		public const string NSKitStandardCostOnly = "K";
		public const string NSKitStandardAndStockComponentsCost = "C";
	}
}
