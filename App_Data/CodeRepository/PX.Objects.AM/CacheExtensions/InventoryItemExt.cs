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
using PX.Objects.IN;
using System;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.AM.Attributes;
using PX.Data.BQL;
using PX.Objects.CS;

namespace PX.Objects.AM.CacheExtensions
{
	/// <summary>
	/// Cache extension for <see cref="InventoryItem"/>
	/// </summary>
    [PXCopyPasteHiddenFields(
        typeof(InventoryItemExt.aMBOMID), 
        typeof(InventoryItemExt.aMPlanningBOMID), 
        typeof(InventoryItemExt.aMConfigurationID))]
    [Serializable]
    public sealed class InventoryItemExt : PXCacheExtension<InventoryItem>
    {
        // Developer note: new fields added here should also be added to InventoryItemMfgOnly

        public static bool IsActive()
        {
			return Features.ManufacturingOrDRPOrReplenishmentEnabled();
        }

        #region AMBOMID
        /// <summary>
        /// Default BOM ID
        /// </summary>
        public abstract class aMBOMID : PX.Data.BQL.BqlString.Field<aMBOMID> { }
        /// <summary>
        /// Default BOM ID
        /// </summary>
        [BomID(DisplayName = "Default BOM ID", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXSelector(typeof(Search<AMBomItem.bOMID,
                        Where<AMBomItem.inventoryID, Equal<Current<InventoryItem.inventoryID>>>,
                        OrderBy<Asc<AMBomItem.bOMID>>>)
            , typeof(AMBomItem.bOMID)
            , typeof(AMBomItem.siteID)
            , typeof(AMBomItem.revisionID)
            , DescriptionField = typeof(AMBomItem.descr))]
        [ActiveBOMRestrictor]
        public string AMBOMID { get; set; }
        #endregion
        #region AMPlanningBOMID
        /// <summary>
        /// Planning BOM ID
        /// </summary>
        public abstract class aMPlanningBOMID : PX.Data.BQL.BqlString.Field<aMPlanningBOMID> { }
        /// <summary>
        /// Planning BOM ID
        /// </summary>
        [BomID(DisplayName = "Planning BOM ID", Visibility = PXUIVisibility.Undefined, FieldClass = Features.MRPFIELDCLASS)]
        [PXSelector(typeof(Search<AMBomItem.bOMID,
                        Where<AMBomItem.inventoryID, Equal<Current<InventoryItem.inventoryID>>>,
                            OrderBy<Asc<AMBomItem.bOMID>>>)
            , typeof(AMBomItem.bOMID)
            , typeof(AMBomItem.siteID)
            , typeof(AMBomItem.revisionID)
            , DescriptionField = typeof(AMBomItem.descr))]
        [ActiveBOMRestrictor]
        public string AMPlanningBOMID { get; set; }
        #endregion
        #region AMLotSize
        public abstract class aMLotSize : PX.Data.BQL.BqlDecimal.Field<aMLotSize> { }

		/// <summary>
		/// Lot size
		/// </summary>
		[PXDBQuantity(MinValue = 0)]
        [PXUIField(DisplayName = "Lot Size")]
		[PXDefault(typeof(Search<INItemClassExt.aMLotSize, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMLotSize { get; set; }
        #endregion
        #region AMMaxOrdQty
        public abstract class aMMaxOrdQty : PX.Data.BQL.BqlDecimal.Field<aMMaxOrdQty> { }

		/// <summary>
		/// Max order qty
		/// </summary>
		[PXDBQuantity(MinValue = 0)]
        [PXUIField(DisplayName="Max Order Qty")]
		[PXDefault(typeof(Search<INItemClassExt.aMMaxOrdQty, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMMaxOrdQty { get; set; }
        #endregion
        #region AMMinOrdQty
        public abstract class aMMinOrdQty : PX.Data.BQL.BqlDecimal.Field<aMMinOrdQty> { }

		/// <summary>
		/// Min order qty
		/// </summary>
		[PXDBQuantity(MinValue = 0)]
        [PXUIField(DisplayName="Min Order Qty")]
		[PXDefault(typeof(Search<INItemClassExt.aMMinOrdQty, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMMinOrdQty { get; set; }
        #endregion
        #region AMLowLevel
        public abstract class aMLowLevel : PX.Data.BQL.BqlInt.Field<aMLowLevel> { }
        /// <summary>
        /// Non UI field - keeps items lowest bom level value used in calculations
        /// </summary>
        [PXDBInt]
        [PXUIField(DisplayName = "Low Level", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false, FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Int32? AMLowLevel { get; set; }
        #endregion
        #region AMMFGLeadTime
        public abstract class aMMFGLeadTime : PX.Data.BQL.BqlInt.Field<aMMFGLeadTime> { }

		/// <summary>
		/// Manufacturing lead time
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Manufacture Lead Time", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
		[PXDefault(typeof(Search<INItemClassExt.aMMFGLeadTime, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        public Int32? AMMFGLeadTime { get; set; }
        #endregion
        #region AMGroupWindowOverride
        public abstract class aMGroupWindowOverride : PX.Data.BQL.BqlBool.Field<aMGroupWindowOverride> { }

		/// <summary>
		/// Override of Days of Supply
		/// </summary>
		[PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Override", FieldClass = Features.MRPDRP)]
        public Boolean? AMGroupWindowOverride { get; set; }
        #endregion
        #region AMGroupWindow
        public abstract class aMGroupWindow : PX.Data.BQL.BqlInt.Field<aMGroupWindow> { }

		/// <summary>
		/// Days of Supply
		/// </summary>
		[PXDBInt(MinValue=0)]
		[PXUIField(DisplayName = "Days of Supply ", FieldClass = Features.MRPDRP)]
		[PXUIEnabled(typeof(Where<aMGroupWindowOverride, Equal<True>>))]
		[PXDefault(typeof(Search<INItemClassExt.aMDaysSupply, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public Int32? AMGroupWindow { get; set; }
        #endregion
        #region AMWIPAcctID
        public abstract class aMWIPAcctID : PX.Data.BQL.BqlInt.Field<aMWIPAcctID> { }

		/// <summary>
		/// Work In Process Account
		/// </summary>
		[Account(DisplayName = "Work In Process Account", FieldClass = Features.MANUFACTURINGFIELDCLASS, Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
        [PXDefault(typeof(Search<INPostClassExt.aMWIPAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXForeignReference(typeof(Field<InventoryItemExt.aMWIPAcctID>.IsRelatedTo<Account.accountID>))]
        public Int32? AMWIPAcctID { get; set; }
        #endregion
        #region AMWIPSubID
        public abstract class aMWIPSubID : PX.Data.BQL.BqlInt.Field<aMWIPSubID> { }

		/// <summary>
		/// Work In Process Sub.
		/// </summary>
		[SubAccount(typeof(InventoryItemExt.aMWIPAcctID), DisplayName = "Work In Process Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXDefault(typeof(Search<INPostClassExt.aMWIPSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXForeignReference(typeof(Field<InventoryItemExt.aMWIPSubID>.IsRelatedTo<Sub.subID>))]
        public Int32? AMWIPSubID { get; set; }
        #endregion
        #region AMWIPVarianceAcctID
        public abstract class aMWIPVarianceAcctID : PX.Data.BQL.BqlInt.Field<aMWIPVarianceAcctID> { }

		/// <summary>
		/// WIP Variance Account
		/// </summary>
		[Account(DisplayName = "WIP Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true, FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXDefault(typeof(Search<INPostClassExt.aMWIPVarianceAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXForeignReference(typeof(Field<InventoryItemExt.aMWIPVarianceAcctID>.IsRelatedTo<Account.accountID>))]
        public Int32? AMWIPVarianceAcctID { get; set; }
        #endregion
        #region AMWIPVarianceSubID
        public abstract class aMWIPVarianceSubID : PX.Data.BQL.BqlInt.Field<aMWIPVarianceSubID> { }

		/// <summary>
		/// WIP Variance Sub.
		/// </summary>
		[SubAccount(typeof(InventoryItemExt.aMWIPVarianceAcctID), DisplayName = "WIP Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXDefault(typeof(Search<INPostClassExt.aMWIPVarianceSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXForeignReference(typeof(Field<InventoryItemExt.aMWIPVarianceSubID>.IsRelatedTo<Sub.subID>))]
        public Int32? AMWIPVarianceSubID { get; set; }
        #endregion
        #region AMConfigurationID

        public abstract class aMConfigurationID : PX.Data.BQL.BqlString.Field<aMConfigurationID> { }
        /// <summary>
        /// Item configuration ID. If null, this item isn't configurable.
        /// </summary>
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Configuration ID", FieldClass = Features.PRODUCTCONFIGURATORFIELDCLASS)]
        [PXSelector(typeof(Search<AMConfiguration.configurationID, 
            Where<AMConfiguration.status, 
                Equal<ConfigRevisionStatus.active>, 
            And<AMConfiguration.inventoryID,
                Equal<Current<InventoryItem.inventoryID>>>>>),
            DescriptionField = typeof(AMConfiguration.descr))]
        public string AMConfigurationID { get; set; }

        #endregion
        #region AMSafetyStock
        public abstract class aMSafetyStock : PX.Data.BQL.BqlDecimal.Field<aMSafetyStock> { }

		/// <summary>
		/// Safety stock
		/// </summary>
		[PXDBQuantity(MinValue = 0)]
		[PXDefault(typeof(Search<INItemClassExt.aMSafetyStock, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Safety Stock", FieldClass = Features.MRPDRP)]
        public Decimal? AMSafetyStock { get; set; }
        #endregion
        #region AMMinQty
        public abstract class aMMinQty : PX.Data.BQL.BqlDecimal.Field<aMMinQty> { }

		/// <summary>
		/// Min qty
		/// </summary>
		[PXDBQuantity(MinValue = 0)]
		[PXDefault(typeof(Search<INItemClassExt.aMMinQty, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Reorder Point", FieldClass = Features.MRPDRP)]
        public Decimal? AMMinQty { get; set; }
        #endregion
        #region AMQtyRoundUp
        public abstract class aMQtyRoundUp : PX.Data.BQL.BqlBool.Field<aMQtyRoundUp> { }

		/// <summary>
		/// Qty round up
		/// </summary>
		[PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Quantity Round Up", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        public Boolean? AMQtyRoundUp { get; set; }
        #endregion
        #region AMScrapSiteID
        public abstract class aMScrapSiteID : PX.Data.BQL.BqlInt.Field<aMScrapSiteID> { }

		/// <summary>
		/// Scrap Warehouse
		/// </summary>
		[PXRestrictor(typeof(Where<INSite.active, Equal<True>>), PX.Objects.IN.Messages.InactiveWarehouse, typeof(INSite.siteCD), CacheGlobal = true)]
        [Site(DisplayName = "Scrap Warehouse", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        public Int32? AMScrapSiteID { get; set; }
        #endregion
        #region AMScrapLocationID
        public abstract class aMScrapLocationID : PX.Data.BQL.BqlInt.Field<aMScrapLocationID> { }

		/// <summary>
		/// Scrap Location
		/// </summary>
		[Location(typeof(InventoryItemExt.aMScrapSiteID), DisplayName = "Scrap Location", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXRestrictor(typeof(Where<INLocation.active, Equal<True>>),
            PX.Objects.IN.Messages.InactiveLocation, typeof(INLocation.locationCD), CacheGlobal = true)]
        public Int32? AMScrapLocationID { get; set; }
        #endregion
        #region AMMakeToOrderItem
        public abstract class aMMakeToOrderItem : PX.Data.BQL.BqlBool.Field<aMMakeToOrderItem> { }

		/// <summary>
		/// Make to Order Item
		/// </summary>
		[PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Make to Order Item", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        public Boolean? AMMakeToOrderItem { get; set; }
        #endregion
        #region AMDefaultMarkFor
        public abstract class aMDefaultMarkFor : PX.Data.BQL.BqlInt.Field<aMDefaultMarkFor> { }

		/// <summary>
		/// Default mark for
		/// </summary>
		[PXDBInt]
        [PXDefault(MaterialDefaultMarkFor.NoDefault, PersistingCheck = PXPersistingCheck.Nothing)]
        [MaterialDefaultMarkFor.StockItemList]
        [PXUIField(DisplayName = "Dflt Mark For", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        public int? AMDefaultMarkFor { get; set; }
        #endregion
        #region AMCheckSchdMatlAvailability
        /// <summary>
        /// APS Schedule option - Check for Material Availability.
        /// </summary>
        public abstract class aMCheckSchdMatlAvailability : PX.Data.BQL.BqlBool.Field<aMCheckSchdMatlAvailability> { }

        private Boolean? _AMCheckSchdMatlAvailability;
        /// <summary>
        /// APS Schedule option - Check for Material Availability.
        /// </summary>
        [PXDBBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Check for Material Availability", FieldClass = Features.ADVANCEDPLANNINGFIELDCLASS)]
        public Boolean? AMCheckSchdMatlAvailability
        {
            get
            {
                return this._AMCheckSchdMatlAvailability;
            }
            set
            {
                this._AMCheckSchdMatlAvailability = value ?? true;
            }
        }
        #endregion
        #region AMCTPItem
        /// <summary>
        /// Indicates item is a CTP calculated item
        /// </summary>
        public abstract class aMCTPItem : PX.Data.BQL.BqlBool.Field<aMCTPItem> { }

        private Boolean? _AMCTPItem;
        /// <summary>
        /// Indicates item is a CTP calculated item
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "CTP Item", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        public Boolean? AMCTPItem
        {
            get
            {
                return this._AMCTPItem;
            }
            set
            {
                this._AMCTPItem = value ?? false;
            }
        }
        #endregion

		#region AMSourceSiteID
		public abstract class aMSourceSiteID : BqlInt.Field<aMSourceSiteID> { }

		/// <summary>
		/// Source Warehouse
		/// </summary>
		[AMSite(DisplayName = "Source Warehouse", FieldClass = nameof(FeaturesSet.Warehouse))]
		[PXDefault(typeof(Search<INItemClassExt.aMSourceSiteID, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(Where<InventoryItem.replenishmentSource, Equal<INReplenishmentSource.transfer>,
			Or2<Where<InventoryItem.planningMethod, Equal<INPlanningMethod.mRP>, Or<InventoryItem.planningMethod, Equal<INPlanningMethod.dRP>>>,
				And<Where<InventoryItem.replenishmentSource, Equal<INReplenishmentSource.purchased>, Or<InventoryItem.replenishmentSource, Equal<INReplenishmentSource.purchaseToOrder>>>>>>))]
		public int? AMSourceSiteID
		{
			get;
			set;
		}
		#endregion
    }
}
