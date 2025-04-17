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
using PX.Objects.AM.Attributes;
using PX.Data.BQL;
using PX.Objects.CS;

namespace PX.Objects.AM.CacheExtensions
{
	/// <summary>
	/// I n item site ext
	/// </summary>
	[Serializable]
    public sealed class INItemSiteExt : PXCacheExtension<INItemSite>
    {
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
                Where<AMBomItem.inventoryID, Equal<Current<INItemSite.inventoryID>>,
                And<Where<AMBomItem.siteID, Equal<Current<INItemSite.siteID>>, Or<AMBomItem.siteID, IsNull>>>>,
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
                Where<AMBomItem.inventoryID, Equal<Current<INItemSite.inventoryID>>,
					And<Where<AMBomItem.siteID, Equal<Current<INItemSite.siteID>>, Or<AMBomItem.siteID, IsNull>>>>,
				OrderBy<Asc<AMBomItem.bOMID>>>)
            , typeof(AMBomItem.bOMID)
            , typeof(AMBomItem.siteID)
            , typeof(AMBomItem.revisionID)
            , DescriptionField = typeof(AMBomItem.descr))]
        [ActiveBOMRestrictor]
        public string AMPlanningBOMID { get; set; }
        #endregion
        #region AMLotSizeOverride
        public abstract class aMLotSizeOverride : PX.Data.BQL.BqlBool.Field<aMLotSizeOverride> { }

		/// <summary>
		/// Override of Lot Size
		/// </summary>
		[PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMLotSizeOverride { get; set; }
        #endregion
        #region AMLotSize
        public abstract class aMLotSize : PX.Data.BQL.BqlDecimal.Field<aMLotSize> { }

		/// <summary>
		/// Lot Size
		/// </summary>
		[PXDBQuantity]
        [PXUIField(DisplayName = "Lot Size")]
		[PXDefault(typeof(Search<InventoryItemExt.aMLotSize,
						Where<InventoryItem.inventoryID,
							Equal<Current<INItemSite.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMLotSize { get; set; }
        #endregion
        #region AMMaxOrdQtyOverride
        public abstract class aMMaxOrdQtyOverride : PX.Data.BQL.BqlBool.Field<aMMaxOrdQtyOverride> { }

		/// <summary>
		/// Override of Max Order Qty
		/// </summary>
		[PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMMaxOrdQtyOverride { get; set; }
        #endregion
        #region AMMaxOrdQty
        public abstract class aMMaxOrdQty : PX.Data.BQL.BqlDecimal.Field<aMMaxOrdQty> { }

		/// <summary>
		/// Max Order Qty
		/// </summary>
		[PXDBQuantity]
        [PXUIField(DisplayName = "Max Order Qty")]
		[PXDefault(typeof(Search<InventoryItemExt.aMMaxOrdQty,
						Where<InventoryItem.inventoryID,
							Equal<Current<INItemSite.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMMaxOrdQty { get; set; }
        #endregion
        #region AMMinOrdQtyOverride
        public abstract class aMMinOrdQtyOverride : PX.Data.BQL.BqlBool.Field<aMMinOrdQtyOverride> { }

		/// <summary>
		/// Override Min Order Qty
		/// </summary>
		[PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMMinOrdQtyOverride { get; set; }
        #endregion
        #region AMMinOrdQty
        public abstract class aMMinOrdQty : PX.Data.BQL.BqlDecimal.Field<aMMinOrdQty> { }

		/// <summary>
		/// Min Order Qty
		/// </summary>
		[PXDBQuantity]
        [PXUIField(DisplayName = "Min Order Qty")]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMMinOrdQty { get; set; }
        #endregion
        #region AMMFGLeadTimeOverride
        public abstract class aMMFGLeadTimeOverride : PX.Data.BQL.BqlBool.Field<aMMFGLeadTimeOverride> { }

		/// <summary>
		/// Override of Manufacturing Lead Time
		/// </summary>
		[PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Override", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        public Boolean? AMMFGLeadTimeOverride { get; set; }
        #endregion
        #region AMMFGLeadTime
        public abstract class aMMFGLeadTime : PX.Data.BQL.BqlInt.Field<aMMFGLeadTime> { }

		/// <summary>
		/// Manufacturing Lead Time
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Manufacturing Lead Time", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Int32? AMMFGLeadTime { get; set; }
        #endregion
        #region AMGroupWindowOverride
        public abstract class aMGroupWindowOverride : PX.Data.BQL.BqlBool.Field<aMGroupWindowOverride> { }

		/// <summary>
		/// Override Days of Supply
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
		[PXDBInt(MinValue = 0)]
		[PXUIField(DisplayName = "Days of Supply", FieldClass = Features.MRPDRP)]
        [PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(Where<aMGroupWindowOverride, Equal<True>>))]
        public Int32? AMGroupWindow { get; set; }
        #endregion
        #region AMConfigurationID

        public abstract class aMConfigurationID : PX.Data.BQL.BqlString.Field<aMConfigurationID> { }
        /// <summary>
        /// Item configuration ID. If null, this item will fallback to StockItem configuration ID
        /// </summary>
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Configuration ID", FieldClass = Features.PRODUCTCONFIGURATORFIELDCLASS)]
        [PXSelector(typeof(Search2<AMConfiguration.configurationID,
                InnerJoin<AMBomItem,
                    On<AMBomItem.bOMID, Equal<AMConfiguration.bOMID>,
                        And<AMBomItem.revisionID, Equal<AMConfiguration.bOMRevisionID>>>>,
                Where<AMConfiguration.status, Equal<ConfigRevisionStatus.active>,
                    And<AMConfiguration.inventoryID, Equal<Current<INItemSite.inventoryID>>>>>),
            new[]
            {
                typeof(AMConfiguration.configurationID),
                typeof(AMConfiguration.revision),
                typeof(AMConfiguration.bOMID),
                typeof(AMBomItem.siteID)
            },
            DescriptionField = typeof(AMConfiguration.descr))]
        public string AMConfigurationID { get; set; }

        #endregion
		#region AMSafetyStockOverride
        public abstract class aMSafetyStockOverride : PX.Data.BQL.BqlBool.Field<aMSafetyStockOverride> { }

		/// <summary>
		/// Override of Safety Stock
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Override", FieldClass = Features.MRPDRP)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public Boolean? AMSafetyStockOverride { get; set; }
        #endregion
		#region AMSafetyStock
        public abstract class aMSafetyStock : PX.Data.BQL.BqlDecimal.Field<aMSafetyStock> { }

		/// <summary>
		/// Safety Stock
		/// </summary>
		[PXQuantity]
        [PXUIField(DisplayName = "Safety Stock", FieldClass = Features.MRPDRP)]
        public Decimal? AMSafetyStock { get; set; }
        #endregion
		#region AMMinQtyOverride
        public abstract class aMMinQtyOverride : PX.Data.BQL.BqlBool.Field<aMMinQtyOverride> { }

		/// <summary>
		/// Override of Reorder Point
		/// </summary>
		[PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public Boolean? AMMinQtyOverride { get; set; }
        #endregion
		#region AMMinQty
        public abstract class aMMinQty : PX.Data.BQL.BqlDecimal.Field<aMMinQty> { }

		/// <summary>
		/// Reorder Point
		/// </summary>
		[PXQuantity]
        [PXUIField(DisplayName = "Reorder Point")]
        public Decimal? AMMinQty { get; set; }
        #endregion
        #region AMScrapOverride
        public abstract class aMScrapOverride : PX.Data.BQL.BqlBool.Field<aMScrapOverride> { }

		/// <summary>
		/// Scrap Override
		/// </summary>
		[PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Scrap Override", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        public Boolean? AMScrapOverride { get; set; }
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
		[Location(typeof(INItemSiteExt.aMScrapSiteID))]
        [PXUIField(DisplayName = "Scrap Location", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXRestrictor(typeof(Where<INLocation.active, Equal<True>>),
            PX.Objects.IN.Messages.InactiveLocation, typeof(INLocation.locationCD), CacheGlobal = true)]
        public Int32? AMScrapLocationID { get; set; }
		#endregion
		#region AMTransferLeadTimeOverride
		public abstract class aMTransferLeadTimeOverride : PX.Data.BQL.BqlBool.Field<aMTransferLeadTimeOverride> { }

		/// <summary>
		/// Override of Transfer Lead Time
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Override", FieldClass = nameof(FeaturesSet.Warehouse))]
		public Boolean? AMTransferLeadTimeOverride { get; set; }
		#endregion
		#region AMTransferLeadTime
		public abstract class aMTransferLeadTime : PX.Data.BQL.BqlInt.Field<aMTransferLeadTime> { }

		/// <summary>
		/// Transfer Lead Time
		/// </summary>
		[DefaultWithOverride(typeof(aMTransferLeadTimeOverride), null, typeof(Search<AMSiteTransfer.transferLeadTime,
		Where<AMSiteTransfer.siteID, Equal<Current<INItemSite.siteID>>,
		And<AMSiteTransfer.transferSiteID, Equal<Current<INItemSite.replenishmentSourceSiteID>>,
		And<Where<Current<INItemSiteExt.aMTransferLeadTimeOverride>, IsNull,
					Or<Current<INItemSiteExt.aMTransferLeadTimeOverride>, Equal<False>>>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<INItemSite.replenishmentSourceSiteID,INItemSiteExt.aMSourceSiteID, INItemSiteExt.aMTransferLeadTimeOverride, INItemSite.siteID>))]
		[PXDBInt(MinValue = 0)]
		[PXUIEnabled(typeof(Where<INItemSiteExt.aMTransferLeadTimeOverride, Equal<True>>))]
		[PXUIField(DisplayName = "Transfer Lead Time", FieldClass = nameof(FeaturesSet.Warehouse))]
		public Int32? AMTransferLeadTime { get; set; }
		#endregion
		#region AMSourceSiteID
		public abstract class aMSourceSiteID : BqlInt.Field<aMSourceSiteID> { }

		/// <summary>
		/// Source Warehouse
		/// </summary>
		[AMSite(DisplayName = "Source Warehouse", FieldClass = nameof(FeaturesSet.Warehouse))]
		public int? AMSourceSiteID
		{
			get;
			set;
		}
		#endregion
		#region AMSourceSiteIDOverride
		public abstract class aMSourceSiteIDOverride : BqlBool.Field<aMSourceSiteIDOverride> { }

		/// <summary>
		/// Override of Source Warehouse
		/// </summary>
		[PXBool]
		[PXUIEnabled(typeof(Where<INItemSite.replenishmentSource, Equal<INReplenishmentSource.transfer>,
			Or2<Where<INItemSite.planningMethod, Equal<INPlanningMethod.mRP>, Or<INItemSite.planningMethod, Equal<INPlanningMethod.dRP>>>,
				And<Where<INItemSite.replenishmentSource, Equal<INReplenishmentSource.purchased>, Or<INItemSite.replenishmentSource, Equal<INReplenishmentSource.purchaseToOrder>>>>>>))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Override", FieldClass = nameof(FeaturesSet.Warehouse))]
		public bool? AMSourceSiteIDOverride
		{
			get;
			set;
		}
		#endregion
		#region AMReplenishmentSourceOverride
		public abstract class aMReplenishmentSourceOverride : PX.Data.BQL.BqlBool.Field<aMReplenishmentSourceOverride> { }

		/// <summary>
		/// Override of Source
		/// </summary>
		[PXDBBool]
		[PXDefault(false, typeof(Switch<Case<Where<INItemSite.replenishmentSource, NotEqual<Current<InventoryItem.replenishmentSource>>>, False>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Override")]
		public Boolean? AMReplenishmentSourceOverride { get; set; }
		#endregion
		#region AMReplenishmentSource
		public abstract class aMReplenishmentSource : PX.Data.BQL.BqlString.Field<aMReplenishmentSource> { }

		/// <summary>
		/// Source
		/// </summary>
		[PXString(1, IsFixed = true)]
		[PXDefault(INReplenishmentSource.None, typeof(INItemSite.replenishmentSource), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Source")]
		[PXUIEnabled(typeof(Where<aMReplenishmentSourceOverride, Equal<True>>))]
		[INReplenishmentSource.List] 
		public string AMReplenishmentSource { get; set; }
		#endregion
	}
}
