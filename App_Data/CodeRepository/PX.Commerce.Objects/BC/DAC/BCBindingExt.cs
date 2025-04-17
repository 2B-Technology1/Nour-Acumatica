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
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CS;
using System.Collections.Generic;
using PX.Commerce.Core;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Api;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// Extension over the <see cref="BCBinding"/> DAC that keeps additional information for the store. The extension is created together with BCBinding.
	/// </summary>
	[Serializable]
	[PXCacheName("Store Settings")]
	public class BCBindingExt : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BCBindingExt>.By<BCBindingExt.bindingID>
		{
			public static BCBindingExt Find(PXGraph graph, int? binding, PKFindOptions options = PKFindOptions.None) => FindBy(graph, binding, options);
		}
		public static class FK
		{
			public class CustomerGuest : Customer.PK.ForeignKeyOf<BCBinding>.By<guestCustomerID> { }
			public class OrderType : SOOrderType.PK.ForeignKeyOf<BCBinding>.By<orderType> { }
			public class ReturnOrderType : SOOrderType.PK.ForeignKeyOf<BCBinding>.By<returnorderType> { }
			public class GiftCertificate : InventoryItem.PK.ForeignKeyOf<BCBinding>.By<giftCertificateItemID> { }
			public class NonStockItemClass : INItemClass.PK.ForeignKeyOf<BCBinding>.By<nonStockItemClassID> { }
			public class StockItemClass : INItemClass.PK.ForeignKeyOf<BCBinding>.By<stockItemClassID> { }
		}
		#endregion

		#region BindingID
		/// <summary>
		/// The record identity.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(BCBinding.bindingID))]
		[PXUIField(DisplayName = "Store", Visible = false)]
		[PXParent(typeof(Select<BCBinding, Where<BCBinding.bindingID, Equal<Current<BCBindingExt.bindingID>>>>))]
		public int? BindingID { get; set; }
		///<inheritdoc cref="BindingID" />
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		#endregion

		#region DefaultStoreCurrency
		/// <summary>
		/// The default currency for this store.
		/// </summary>
		[PXDBString(12, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Currency",Enabled = false, IsReadOnly = true)]
		public virtual string DefaultStoreCurrency { get; set; }
		///<inheritdoc cref="DefaultStoreCurrency" />
		public abstract class defaultStoreCurrency : PX.Data.BQL.BqlString.Field<defaultStoreCurrency> { }
		#endregion
		#region StoreTimeZone
		/// <summary>
		/// The time zone for this store.
		/// </summary>
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Store Time Zone", Enabled = false, IsReadOnly = true)]
		public virtual string StoreTimeZone { get; set; }
		///<inheritdoc cref="StoreTimeZone" />
		public abstract class storeTimeZone : PX.Data.BQL.BqlString.Field<storeTimeZone> { }
		#endregion
		#region SupportedCurrencies
		/// <summary>
		/// A comma-separated list of the supported currencies for this store.
		/// </summary>
		[PXDBString(200, IsUnicode = true)]
		[PXUIField(DisplayName = "Supported Currencies", Enabled = false, IsReadOnly = true)]
		public virtual string SupportedCurrencies { get; set; }
		///<inheritdoc cref="SupportedCurrencies" />
		public abstract class supportedCurrencies : PX.Data.BQL.BqlString.Field<supportedCurrencies> { }
		#endregion

		//Numberings & Keys
		#region CustomerNumberingID
		/// <summary>
		/// Numbering sequence used to generate new customer IDs on customer import. The field is mandatory for the Customer Processor.
		/// </summary>
		[PXUIField(DisplayName = "Customer Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXRestrictor(typeof(Where<Numbering.userNumbering, NotEqual<True>>), "Manual numbering sequences are not supported")]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Customer, BCEntitiesAttribute.Company, BCEntitiesAttribute.Address, BCEntitiesAttribute.Order })]

		public virtual string CustomerNumberingID { get; set; }
		///<inheritdoc cref="CustomerNumberingID" />
		public abstract class customerNumberingID : PX.Data.BQL.BqlString.Field<customerNumberingID> { }
		#endregion
		#region LocationNumberingID
		/// <summary>
		/// Numbering sequence used to generate new location IDs on location import. The field is mandatory for the Location Processor.
		/// </summary>
		[PXUIField(DisplayName = "Location Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXRestrictor(typeof(Where<Numbering.userNumbering, NotEqual<True>>), "Manual numbering sequences are not supported")]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Address, BCEntitiesAttribute.Company })]
		public virtual string LocationNumberingID { get; set; }
		///<inheritdoc cref="LocationNumberingID" />
		public abstract class locationNumberingID : PX.Data.BQL.BqlString.Field<locationNumberingID> { }
		#endregion
		#region InventoryNumberingID
		/// <summary>
		/// Numbering sequence used to generate new inventory IDs.
		/// </summary>
		[PXUIField(DisplayName = "Inventory Numbering Sequence", Visible = false)]
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXRestrictor(typeof(Where<Numbering.userNumbering, NotEqual<True>>), "Manual numbering sequences are not supported")]
		public virtual string InventoryNumberingID { get; set; }
		///<inheritdoc cref="InventoryNumberingID" />
		public abstract class inventoryNumberingID : PX.Data.BQL.BqlString.Field<inventoryNumberingID> { }
		#endregion
		#region CustomerTemplate
		/// <summary>
		/// The template of the customer numbering for imported customers.
		/// </summary>
		[PXUIField(DisplayName = "Customer Numbering Template", Visibility = PXUIVisibility.Visible)]
		[PXDBString(30, IsUnicode = true)]
		[BCDimensionMaskAttribute(CustomerRawAttribute.DimensionName, typeof(BCBindingExt.customerNumberingID), typeof(BCBinding.branchID))]
		public virtual string CustomerTemplate { get; set; }
		///<inheritdoc cref="CustomerTemplate" />
		public abstract class customerTemplate : PX.Data.BQL.BqlString.Field<customerTemplate> { }
		#endregion
		#region LocationTemplate
		/// <summary>
		/// The template of the customer numbering for imported locations.
		/// </summary>
		[PXUIField(DisplayName = "Location Numbering Template", Visibility = PXUIVisibility.Visible)]
		[PXDBString(30, IsUnicode = true)]
		[BCDimensionMaskAttribute(LocationIDAttribute.DimensionName, typeof(BCBindingExt.locationNumberingID), typeof(BCBinding.branchID))]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Address })]
		public virtual string LocationTemplate { get; set; }
		///<inheritdoc cref="LocationTemplate" />
		public abstract class locationTemplate : PX.Data.BQL.BqlString.Field<locationTemplate> { }
		#endregion
		#region InventoryTemplate
		/// <summary>
		/// The template of the customer numbering for imported inventory items.
		/// </summary>
		[PXUIField(DisplayName = "Inventory Template", Visible = false)]
		[PXDBString(30, IsUnicode = true)]
		[BCDimensionMaskAttribute(InventoryAttribute.DimensionName, typeof(BCBindingExt.inventoryNumberingID), typeof(BCBinding.branchID))]
		public virtual string InventoryTemplate { get; set; }
		///<inheritdoc cref="InventoryTemplate" />
		public abstract class inventoryTemplate : PX.Data.BQL.BqlString.Field<inventoryTemplate> { }
		#endregion

		//Customer
		#region CustomerClassID
		/// <summary>
		/// Customer class used for creation of new customers on customer import.
		/// </summary>
		[PXDBString(10, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Customer Class")]
		[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr))]
		[PXDefault(typeof(ARSetup.dfltCustomerClassID), PersistingCheck = PXPersistingCheck.Nothing)]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Customer, BCEntitiesAttribute.Company, BCEntitiesAttribute.Address, BCEntitiesAttribute.Order })]
		public virtual string CustomerClassID { get; set; }
		///<inheritdoc cref="CustomerClassID" />
		public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
		#endregion
		#region GuestCustomerID
		/// <summary>
		/// ID of the customer account used for synchronization of orders created by a guest user in eCommerce.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Generic Guest Customer")]
		[PXDimensionSelector("BIZACCT", typeof(Search<Customer.bAccountID>), typeof(Customer.acctCD),
			typeof(Customer.acctCD),
			typeof(Customer.acctName),
			typeof(Customer.customerClassID),
			typeof(Customer.status),
			DescriptionField = typeof(Customer.acctName),
			DirtyRead = true)]
		[PXRestrictor(typeof(Where<Customer.status, IsNull,
						Or<Customer.status, Equal<CustomerStatus.active>,
						Or<Customer.status, Equal<CustomerStatus.oneTime>>>>), PX.Objects.AR.Messages.CustomerIsInStatus, typeof(Customer.status))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? GuestCustomerID { get; set; }
		///<inheritdoc cref="GuestCustomerID" />
		public abstract class guestCustomerID : PX.Data.BQL.BqlInt.Field<guestCustomerID> { }
		#endregion
		#region UseMultipleGuestAccounts
		/// <summary>
		/// Indicates if the multiple guest accounts feature is enabled. This setting automatically creates a new guest account when the current one reaches 10,000 or more orders.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Multiple Guest Accounts")]
		public virtual bool? MultipleGuestAccounts { get; set; }
		///<inheritdoc cref="MultipleGuestAccounts" />
		public abstract class multipleGuestAccounts : PX.Data.BQL.BqlBool.Field<multipleGuestAccounts> { }
		#endregion

		//Inventory
		#region StockItemClassID
		/// <summary>
		/// Inventory class used for creation of new stock items on stock item import.
		/// Currently, the field is not used due to restrictions for importing items.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Stock Item Class", Visible = false)]
		[PXSelector(typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<boolTrue>>>),
			SubstituteKey = typeof(INItemClass.itemClassCD),
			DescriptionField = typeof(INItemClass.descr))]
		[PXDefault(typeof(INSetup.dfltStkItemClassID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? StockItemClassID { get; set; }
		///<inheritdoc cref="StockItemClassID" />
		public abstract class stockItemClassID : PX.Data.BQL.BqlInt.Field<stockItemClassID> { }
		#endregion
		#region NonStockItemClassID
		/// <summary>
		/// Inventory class used for creation of new non-stock items on non-stock item import.
		/// Currently, the field is not used due to restrictions for importing items.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Non-Stock Item Class", Visible = false)]
		[PXSelector(typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<boolFalse>>>),
			SubstituteKey = typeof(INItemClass.itemClassCD),
			DescriptionField = typeof(INItemClass.descr))]
		[PXDefault(typeof(INSetup.dfltNonStkItemClassID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? NonStockItemClassID { get; set; }
		///<inheritdoc cref="NonStockItemClassID" />
		public abstract class nonStockItemClassID : PX.Data.BQL.BqlInt.Field<nonStockItemClassID> { }
		#endregion
		#region StockSalesCategoriesIDs
		/// <summary>
		/// Sales categories to be used for exporting stock items when no category is specified for them.
		/// </summary>
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Stock Categories")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SalesCategories()]
		public virtual string StockSalesCategoriesIDs { get; set; }
		///<inheritdoc cref="StockSalesCategoriesIDs" />
		public abstract class stockSalesCategoriesIDs : PX.Data.BQL.BqlString.Field<stockSalesCategoriesIDs> { }
		#endregion
		#region NonStockSalesCategoryID
		/// <summary>
		/// Sales category to be used for exporting non-stock items when no category is specified for them.
		/// </summary>
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Non-Stock Categories")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SalesCategories()]
		public virtual string NonStockSalesCategoriesIDs { get; set; }
		///<inheritdoc cref="NonStockSalesCategoriesIDs" />
		public abstract class nonStockSalesCategoriesIDs : PX.Data.BQL.BqlString.Field<nonStockSalesCategoriesIDs> { }
		#endregion
		#region RelatedItems
		/// <summary>
		/// Type of relations that should be specified during the item export.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Related Items")]
		[PX.Objects.IN.RelatedItems.InventoryRelation.List(MultiSelect = true)]
		public virtual string RelatedItems { get; set; }
		///<inheritdoc cref="RelatedItems" />
		public abstract class relatedItems : PX.Data.BQL.BqlString.Field<relatedItems> { }
		#endregion
		#region Visibility
		/// <summary>
		/// Item visibility settings assigned during the item export.
		/// </summary>
		[PXDBString(1, IsUnicode = false)]
		[PXUIField(DisplayName = "Default Visibility")]
		[BCItemVisibility.ListDef]
		[PXDefault(BCItemVisibility.Visible)]
		public virtual string Visibility { get; set; }
		///<inheritdoc cref="Visibility" />
		public abstract class visibility : PX.Data.BQL.BqlString.Field<visibility> { }
		#endregion

		//Availability
		#region Availability
		/// <summary>
		/// Purchase availability settings of the item that are assigned during the item export.
		/// </summary>
		[PXDBString(1, IsUnicode = false)]
		[PXUIField(DisplayName = "Default Availability")]
		[BCItemAvailabilities.List]
		[PXDefault(BCItemAvailabilities.AvailableSkip)]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.ProductAvailability })]
		public virtual string Availability { get; set; }
		///<inheritdoc cref="Availability" />
		public abstract class availability : PX.Data.BQL.BqlString.Field<availability> { }
		#endregion
		#region NotAvailMode
		/// <summary>
		/// Description of what the store should do when inventory runs out of the item. The value is assigned during the item export.
		/// </summary>
		[PXDBString(1, IsUnicode = false)]
		[PXUIField(DisplayName = "When Qty Unavailable")]
		[BCItemNotAvailModes.List]
		[PXDefault(BCItemNotAvailModes.DoNothing)]
		[PXUIEnabled(typeof(Where<BCBindingExt.availability, Equal<BCItemAvailabilities.availableTrack>>))]
		[PXFormula(typeof(Default<BCBindingExt.availability>))]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.ProductAvailability })]
		public virtual string NotAvailMode { get; set; }
		///<inheritdoc cref="NotAvailMode" />
		public abstract class notAvailMode : PX.Data.BQL.BqlString.Field<notAvailMode> { }
		#endregion
		#region AvailabilityCalcRule
		/// <summary>
		/// Specifies which value to use when exporting product availability, available, available for shipping, or on hand.
		/// </summary>
		[PXDBString(1)]
		[PXUIField(DisplayName = "Availability Mode")]
		[PXDefault(BCAvailabilityLevelsAttribute.AvailableForShipping,
			PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[BCAvailabilityLevels]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.ProductAvailability })]
		public virtual string AvailabilityCalcRule { get; set; }
		///<inheritdoc cref="AvailabilityCalcRule" />
		public abstract class availabilityCalcRule : PX.Data.BQL.BqlString.Field<availabilityCalcRule> { }
		#endregion
		#region WarehouseMode
		/// <summary>
		/// Specifies which warehouse to use when exporting product availability, all warehouses or specific warehouses.
		/// </summary>
		[PXDBString(1)]
		[PXUIField(DisplayName = "Warehouse Mode")]
		[PXDefault(BCWarehouseModeAttribute.AllWarehouse, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[BCWarehouseMode]
		public virtual string WarehouseMode { get; set; }
		///<inheritdoc cref="WarehouseMode" />
		public abstract class warehouseMode : PX.Data.BQL.BqlString.Field<warehouseMode> { }
		#endregion

		//Orders
		#region OrderType
		/// <summary>
		/// A constant for the order type RM.
		/// </summary>
		public class orderRMAType : PX.Data.BQL.BqlString.Constant<orderRMAType>
		{
			/// <summary>
			/// The order type RM.
			/// </summary>
			public const string OrderRmaType = "RM";
			/// <summary>
			/// Constructs the BQL constant.
			/// </summary>
			public orderRMAType() : base(OrderRmaType) { }
		}

		/// <summary>
		/// A constant for the order type CR.
		/// </summary>
		public class orderCRType : PX.Data.BQL.BqlString.Constant<orderCRType>
		{
			/// <summary>
			/// The order Type CR.
			/// </summary>
			public const string OrderCRType = "CR";
			/// <summary>
			/// Constructs the BQL constant.
			/// </summary>
			public orderCRType() : base(OrderCRType) { }
		}

		/// <summary>
		/// A constant for the order type RC.
		/// </summary>
		public class orderRCType : PX.Data.BQL.BqlString.Constant<orderRCType>
		{
			/// <summary>
			/// The order Type CR.
			/// </summary>
			public const string OrderRCType = "RC";
			/// <summary>
			/// Constructs the BQL constant.
			/// </summary>
			public orderRCType() : base(OrderRCType) { }
		}

		/// <summary>
		/// Type of an order that should be used for export and import.
		/// </summary>
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXSelector(
			typeof(Search<SOOrderType.orderType,
				Where<Where<SOOrderType.behavior, Equal<SOBehavior.iN>,
						Or<SOOrderType.behavior, Equal<SOBehavior.qT>,
							Or<Where<SOOrderType.behavior, In3<SOBehavior.sO, SOBehavior.tR>, And<SOOrderType.aRDocType, Equal<ARDocType.invoice>>>>>>>>),
			DescriptionField = typeof(SOOrderType.descr))]
		[PXRestrictor(typeof(Where<SOOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
		[PXUIField(DisplayName = "Order Type for Import")]
		[PXDefault(typeof(SOSetup.defaultOrderType), PersistingCheck = PXPersistingCheck.Nothing)]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Order, BCEntitiesAttribute.Shipment })]
		public virtual string OrderType { get; set; }
		///<inheritdoc cref="OrderType" />
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		#endregion
		#region OtherSalesOrderTypes
		/// <summary>
		/// Additional order types that should be fetched during export.
		/// </summary>
		[PXDBString(100)]
		[PXUIField(DisplayName = "Order Types for Export")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string OtherSalesOrderTypes { get; set; }
		///<inheritdoc cref="OtherSalesOrderTypes" />
		public abstract class otherSalesOrderTypes : PX.Data.BQL.BqlString.Field<otherSalesOrderTypes> { }
		#endregion

		#region ReturnOrderType
		/// <summary>
		/// Return order type that should be created.
		/// </summary>
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Return Order Type")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(
			typeof(Search<SOOrderType.orderType,
				Where<SOOrderType.active, Equal<True>,
					And<SOOrderType.behavior, Equal<SOBehavior.rM>,
					And<SOOrderType.aRDocType, Equal<ARDocType.creditMemo>,
					And<SOOrderType.defaultOperation, Equal<SOOperation.receipt>>>>>>),
			DescriptionField = typeof(SOOrderType.descr))]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.OrderRefunds })]
		public virtual string ReturnOrderType { get; set; }
		///<inheritdoc cref="ReturnOrderType" />
		public abstract class returnorderType : PX.Data.BQL.BqlString.Field<returnorderType> { }
		#endregion
		#region OrderTimeZone
		/// <summary>
		/// The time zone at which the store is located and the orders are placed.
		/// </summary>
		[PXDBString(32)]
		[PXUIField(DisplayName = "Order Time Zone")]
		[PXTimeZone]
		[PXDefault(typeof(Search<PX.SM.PreferencesGeneral.timeZone>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String OrderTimeZone { get; set; }
		///<inheritdoc cref="OrderTimeZone" />
		public abstract class orderTimeZone : PX.Data.BQL.BqlString.Field<orderTimeZone> { }

		#endregion
		#region GiftCertificateItemID
		/// <summary>
		/// Item that should be used to created a gift certificate.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Gift Certificate Item")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<InventoryItem.inventoryID,
			Where<InventoryItem.itemStatus, Equal<InventoryItemStatus.active>,
				And<InventoryItem.itemType, Equal<INItemTypes.nonStockItem>>>>),
			SubstituteKey = typeof(InventoryItem.inventoryCD),
			DescriptionField = typeof(InventoryItem.descr))]
		public virtual Int32? GiftCertificateItemID { get; set; }
		///<inheritdoc cref="GiftCertificateItemID" />
		public abstract class giftCertificateItemID : PX.Data.BQL.BqlInt.Field<giftCertificateItemID> { }
		#endregion

		#region GiftWrappingItemID
		/// <summary>
		/// Item that should used to create the gift wrapping.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Gift Wrapping Item")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<InventoryItem.inventoryID,
			Where<InventoryItem.nonStockShip, Equal<True>,
				And<InventoryItem.kitItem, Equal<False>,
				And2<Where<InventoryItem.itemStatus, Equal<InventoryItemStatus.active>,
										Or<InventoryItem.itemStatus, Equal<InventoryItemStatus.noPurchases>,
										Or<InventoryItem.itemStatus, Equal<InventoryItemStatus.noRequest>>>>,
				And<Where<InventoryItem.itemType, Equal<INItemTypes.serviceItem>,
				Or<InventoryItem.itemType, Equal<INItemTypes.nonStockItem>>>>>>>>),
			SubstituteKey = typeof(InventoryItem.inventoryCD),
			DescriptionField = typeof(InventoryItem.descr))]
		public virtual Int32? GiftWrappingItemID { get; set; }
		///<inheritdoc cref="GiftWrappingItemID" />
		public abstract class giftWrappingItemID : PX.Data.BQL.BqlInt.Field<giftWrappingItemID> { }
		#endregion

		#region RefundAmountItemID
		/// <summary>
		/// Item that should be used to create refund items.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Refund Amount Item")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<InventoryItem.inventoryID,
			Where<InventoryItem.itemStatus, Equal<InventoryItemStatus.active>,
				And<InventoryItem.itemType, Equal<INItemTypes.nonStockItem>,
				And<InventoryItem.nonStockReceipt, Equal<False>,
					And<InventoryItem.nonStockShip, Equal<False>>>>>>),
			SubstituteKey = typeof(InventoryItem.inventoryCD),
			DescriptionField = typeof(InventoryItem.descr))]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Order, BCEntitiesAttribute.OrderRefunds })]
		public virtual Int32? RefundAmountItemID { get; set; }
		///<inheritdoc cref="RefundAmountItemID" />
		public abstract class refundAmountItemID : PX.Data.BQL.BqlInt.Field<refundAmountItemID> { }
		#endregion
		#region ReasonCode
		/// <summary>
		/// The reason code to use when importing refunds.
		/// </summary>
		[PXDBString()]
		[PXUIField(DisplayName = "Refund Reason Code")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.issue>
				>>),
			DescriptionField = typeof(ReasonCode.descr))]
		public virtual string ReasonCode { get; set; }
		///<inheritdoc cref="ReasonCode" />
		public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
		#endregion
		#region PostDiscounts
		/// <summary>
		/// The method of posting discounts, either on the document level, or the order line level.
		/// </summary>
		[PXDBString(1)]
		[PXUIField(DisplayName = "Show Discounts As")]
		[PXDefault(BCPostDiscountAttribute.DocumentDiscount)]
		[BCPostDiscount]
		public virtual string PostDiscounts { get; set; }
		///<inheritdoc cref="PostDiscounts" />
		public abstract class postDiscounts : PX.Data.BQL.BqlString.Field<postDiscounts> { }
		#endregion
		#region ImportOrderRisks
		/// <summary>
		/// Whether to import order risks or not.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Import Order Risks")]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? ImportOrderRisks { get; set; }
		///<inheritdoc cref="ImportOrderRisks" />
		public abstract class importOrderRisks : PX.Data.BQL.BqlBool.Field<importOrderRisks> { }
		#endregion
		#region HoldOnRiskStatus
		/// <summary>
		/// The risk level at which orders should be put on hold, either medium to high or high.
		/// </summary>
		[PXDBString(1)]
		[PXUIField(DisplayName = "Hold on Risk Status")]
		[PXUIEnabled(typeof(Where<Current<BCBindingExt.importOrderRisks>, NotEqual<False>>))]
		[PXDefault(BCRiskStatusAttribute.HighRisk, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<BCBindingExt.importOrderRisks>))]
		[BCRiskStatusAttribute]
		public virtual string HoldOnRiskStatus { get; set; }
		///<inheritdoc cref="HoldOnRiskStatus" />
		public abstract class holdOnRiskStatus : PX.Data.BQL.BqlString.Field<holdOnRiskStatus> { }
		#endregion
		#region SyncOrderNbrToStore
		/// <summary>
		/// Whether to sync the order number to the store.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Tag Ext. Order with ERP Order Nbr.")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? SyncOrderNbrToStore { get; set; }
		///<inheritdoc cref="SyncOrderNbrToStore" />
		public abstract class syncOrderNbrToStore : PX.Data.BQL.BqlBool.Field<syncOrderNbrToStore> { }
		#endregion

		#region SyncOrdersFrom
		/// <summary>
		/// The earliest order date to sync orders, earlier orders are skipped.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Earliest Order Date")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? SyncOrdersFrom { get; set; }
		///<inheritdoc cref="SyncOrdersFrom" />
		public abstract class syncOrdersFrom : IBqlField { }
		#endregion

		#region AllowOrderEdit
		/// <summary>
		/// Indicates whether adding items to processed orders is allowed.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Allow Adding Items to Processed Orders")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? AllowOrderEdit { get; set; }
		///<inheritdoc cref="AllowOrderEdit" />
		public abstract class allowOrderEdit : PX.Data.BQL.BqlBool.Field<allowOrderEdit> { }
		#endregion

		//Taxes Synchronization
		#region TaxSynchronization
		/// <summary>
		/// Mode of the tax synchronization. Describes how the taxes are created for a sales order during its import.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Tax Synchronization")]
		[PXDefault(false)]
		public virtual Boolean? TaxSynchronization { get; set; }
		///<inheritdoc cref="TaxSynchronization" />
		public abstract class taxSynchronization : PX.Data.BQL.BqlBool.Field<taxSynchronization> { }
		#endregion
		#region DefaultTaxZoneID
		/// <summary>
		/// Tax zone that should be used for the taxes synchronization if the tax zone cannot be resolved by Acumatica ERP logic.
		/// </summary>
		[PXDBString]
		[PXUIField(DisplayName = "Default Tax Zone")]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(Where<Current<BCBindingExt.taxSynchronization>, NotEqual<False>>))]
		[PXFormula(typeof(Default<BCBindingExt.taxSynchronization>))]
		public virtual string DefaultTaxZoneID { get; set; }
		///<inheritdoc cref="DefaultTaxZoneID" />
		public abstract class defaultTaxZoneID : PX.Data.BQL.BqlString.Field<defaultTaxZoneID> { }
		#endregion
		#region UseasPrimaryTaxZone
		/// <summary>
		/// Definition of whether <see cref="DefaultTaxZoneID"/> should always be used for creation of a sales order.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Use as Primary Tax Zone")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(Where<Current<BCBindingExt.defaultTaxZoneID>, IsNotNull, And<Current<BCBindingExt.taxSynchronization>, NotEqual<False>>>))]
		[PXFormula(typeof(Default<BCBindingExt.defaultTaxZoneID>))]
		[PXFormula(typeof(Default<BCBindingExt.taxSynchronization>))]
		public virtual bool? UseAsPrimaryTaxZone { get; set; }
		///<inheritdoc cref="UseAsPrimaryTaxZone" />
		public abstract class useAsPrimaryTaxZone : PX.Data.BQL.BqlBool.Field<useAsPrimaryTaxZone> { }
		#endregion

		#region TaxSubstitutionListID
		/// <summary>
		/// The ID of the substitution list to use when substituting tax categories during order import.
		/// </summary>
		[PXDBString(16)]
		[PXUIField(DisplayName = "Taxes")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual String TaxSubstitutionListID { get; set; }
		///<inheritdoc cref="TaxSubstitutionListID" />
		public abstract class taxSubstitutionListID : PX.Data.BQL.BqlString.Field<taxSubstitutionListID> { }
		#endregion
		#region TaxCategorySubstitutionListID
		/// <summary>
		/// The ID of the substitution list to use when substituting tax categories during order import.
		/// </summary>
		[PXDBString(16)]
		[PXUIField(DisplayName = "Tax Categories")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual String TaxCategorySubstitutionListID { get; set; }
		///<inheritdoc cref="TaxCategorySubstitutionListID" />
		public abstract class taxCategorySubstitutionListID : PX.Data.BQL.BqlString.Field<taxCategorySubstitutionListID> { }
		#endregion
		#region ShippingCarrierListID
		/// <summary>
		/// The ID of the substitution list to use when substituting ShipVia during shipment export.
		/// </summary>
		[PXDBString(16)]
		[PXUIField(DisplayName = "Shipping Carriers")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String ShippingCarrierListID { get; set; }
		///<inheritdoc cref="ShippingCarrierListID" />
		public abstract class shippingCarrierListID : PX.Data.BQL.BqlString.Field<shippingCarrierListID> { }
		#endregion
	}
}
