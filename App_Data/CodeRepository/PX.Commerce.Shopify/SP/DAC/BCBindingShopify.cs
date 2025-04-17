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
using PX.Commerce.Objects;
using PX.Data.ReferentialIntegrity.Attributes;
using static PX.Commerce.Shopify.SPConnector;
using PX.Commerce.Shopify.API.REST;
using PX.Common;

namespace PX.Commerce.Shopify
{
	/// <summary>
	/// Represents a binding to a Shopify store.
	/// </summary>
	[Serializable]
	[PXCacheName("Shopify Settings")]
	public class BCBindingShopify : IBqlTable
	{
		/// <summary>
		/// Utility to retrieve a binding by binding ID.
		/// </summary>
		public class PK : PrimaryKeyOf<BCBindingShopify>.By<BCBindingShopify.bindingID>
		{
			/// <summary>
			/// Find the binding with the specified binding ID.
			/// </summary>
			/// <param name="graph">The graph to search.</param>
			/// <param name="binding">The ID of the binding to find.</param>
			/// <returns>The found binding or null if not found.</returns>
			public static BCBindingShopify Find(PXGraph graph, int? binding, PKFindOptions options = PKFindOptions.None) => FindBy(graph, binding, options);
		}

		#region BindingID

		/// <summary>
		/// The primary key and identity of the binding.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(BCBinding.bindingID))]
		[PXUIField(DisplayName = "Store", Visible = false)]
		[PXParent(typeof(Select<BCBinding, Where<BCBinding.bindingID, Equal<Current<BCBindingShopify.bindingID>>>>))]
		public int? BindingID { get; set; }

		/// <inheritdoc cref="BindingID"/>
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }

		#endregion

		//Connection
		#region StoreBaseUrl

		/// <summary>
		/// The admin URL of the Shopify store.
		/// </summary>
		[PXDBString(100, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Store Admin URL")]
		[PXDefault()]
		public virtual string ShopifyApiBaseUrl { get; set; }

		/// <inheritdoc cref="ShopifyApiBaseUrl"/>
		public abstract class shopifyApiBaseUrl : PX.Data.BQL.BqlString.Field<shopifyApiBaseUrl> { }

		#endregion

		#region ShopifyApiKey

		/// <summary>
		/// The API Key of the Shopify store.
		/// </summary>
		[PXRSACryptString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "API Key")]
		public virtual string ShopifyApiKey { get; set; }

		/// <inheritdoc cref="ShopifyApiKey"/>
		public abstract class shopifyApiKey : PX.Data.BQL.BqlString.Field<shopifyApiKey> { }

		#endregion

		#region ShopifyAccessToken

		/// <summary>
		/// The API access token of the Shopify store.
		/// </summary>
		[PXRSACryptString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "API Access Token")]
		[PXDefault]
		public virtual string ShopifyAccessToken { get; set; }

		/// <inheritdoc cref="ShopifyAccessToken"/>
		public abstract class shopifyAccessToken : IBqlField { }

		#endregion

		#region StoreSharedSecret

		/// <summary>
		/// The API secret key of the Shopify store.
		/// </summary>
		[PXRSACryptString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "API Secret Key")]
		[PXDefault()]
		public virtual string StoreSharedSecret { get; set; }

		/// <inheritdoc cref="StoreSharedSecret"/>
		public abstract class storeSharedSecret : PX.Data.BQL.BqlString.Field<storeSharedSecret> { }

		#endregion

		#region ShopifyStoreUrl

		/// <summary>
		/// The store URL for the Shopify store.
		/// </summary>
		[PXDBString(200, IsUnicode = true)]
		[PXUIField(DisplayName = "Store URL", IsReadOnly = true)]
		public virtual string ShopifyStoreUrl { get; set; }

		/// <inheritdoc cref="ShopifyStoreUrl"/>
		public abstract class shopifyStoreUrl : PX.Data.BQL.BqlString.Field<shopifyStoreUrl> { }
		#endregion

		#region ShopifyPlus

		/// <summary>
		/// The store plan of the Shopify store.
		/// </summary>
		[PXDBString(2)]
		[PXUIField(DisplayName = "Store Plan")]
		[BCShopifyStorePlanAttribute]
		[PXDefault(BCShopifyStorePlanAttribute.NormalPlan, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string ShopifyStorePlan { get; set; }

		/// <inheritdoc cref="ShopifyStorePlan"/>
		public abstract class shopifyStorePlan : PX.Data.BQL.BqlString.Field<shopifyStorePlan> { }

		#endregion

		#region ApiCallLimit

		/// <summary>
		/// The maximum number of calls to make before throwing an error when synchronizing with the Shopify store.
		/// </summary>
		[PXInt()]
		public virtual int? ApiCallLimit
		{
			get
			{
				return this.ShopifyStorePlan == BCShopifyStorePlanAttribute.PlusPlan ? ShopifyConstants.ApiCallLimitPlus : ShopifyConstants.ApiCallLimitDefault;
			}
		}

		/// <inheritdoc cref="ApiCallLimit"/>
		public abstract class apiCallLimit : PX.Data.BQL.BqlInt.Field<apiCallLimit> { }

		#endregion

		#region ApiDelaySeconds

		/// <summary>
		/// The amount of time in seconds to wait before retrying an API call.
		/// </summary>
		[PXDBInt(MinValue = 0)]
		[PXDefault(180, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? ApiDelaySeconds { get; set; }

		/// <inheritdoc cref="ApiDelaySeconds"/>
		public abstract class apiDelaySeconds : PX.Data.BQL.BqlInt.Field<apiDelaySeconds> { }

		#endregion

		#region ShopifyApiVersion

		/// <summary>
		/// The shopify API version to use when synchronizing with the Shopify store.
		/// </summary>
		[PXUIField(DisplayName = "API Version", IsReadOnly = true)]
		[PXString()]
		public virtual string ShopifyApiVersion { get; set; }

		/// <inheritdoc cref="ShopifyApiVersion"/>
		public abstract class shopifyApiVersion : PX.Data.BQL.BqlString.Field<shopifyApiVersion> { }

		#endregion

		#region ShopifyPOS

		/// <summary>
		/// Whether to import Shopify POS orders from the Shopify store.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Import POS Orders")]
		[PXDefault(false,PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? ShopifyPOS { get; set; }

		/// <inheritdoc cref="ShopifyPOS"/>
		public abstract class shopifyPOS : PX.Data.BQL.BqlBool.Field<shopifyPOS> { }

		#endregion

		#region POSDirectOrderType

		/// <summary>
		/// The POS direct order type used when importing Shopify POS orders from the Shopify store.
		/// </summary>
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "POS Direct Order Type")]
		[PXSelector(
			typeof(Search<SOOrderType.orderType,
				Where<SOOrderType.active, Equal<True>,
					And<SOOrderType.behavior, Equal<SOBehavior.iN>, And<SOOrderType.aRDocType, Equal<ARDocType.invoice>>>>>),
			DescriptionField = typeof(SOOrderType.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string POSDirectOrderType { get; set; }

		/// <inheritdoc cref="POSDirectOrderType"/>
		public abstract class pOSDirectOrderType : PX.Data.BQL.BqlString.Field<pOSDirectOrderType> { }

		#endregion

		#region POSShippingOrderType

		/// <summary>
		/// The POS shipping order type used when importing Shopify POS orders from the Shopify store.
		/// </summary>
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "POS Shipping Order Type")]
		[PXSelector(
			typeof(Search<SOOrderType.orderType,
				Where<SOOrderType.active, Equal<True>,
					And<SOOrderType.behavior, In3<SOBehavior.sO, SOBehavior.tR>, And<SOOrderType.aRDocType, Equal<ARDocType.invoice>>>>>),
			DescriptionField = typeof(SOOrderType.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string POSShippingOrderType { get; set; }

		/// <inheritdoc cref="POSShippingOrderType"/>
		public abstract class pOSShippingOrderType : PX.Data.BQL.BqlString.Field<pOSShippingOrderType> { }

		#endregion

		#region CombineCategoriesToTags

		/// <summary>
		/// Whether to export sales categories in Acumatica as tags to the Shopify store.
		/// </summary>
		[PXDBString(2)]
		[PXUIField(DisplayName = "Sales Category Export")]
		[BCSalesCategoriesExport]
		[PXDefault(BCSalesCategoriesExportAttribute.SyncToProductTags, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string CombineCategoriesToTags { get; set; }

		/// <inheritdoc cref="CombineCategoriesToTags"/>
		public abstract class combineCategoriesToTags : PX.Data.BQL.BqlString.Field<combineCategoriesToTags> { }
		#endregion
	}

	/// <summary>
	/// An extension of BCBinding for Shopify specific fields.
	/// </summary>
	[PXPrimaryGraph(new Type[] { typeof(BCShopifyStoreMaint) },
					new Type[] { typeof(Where<BCBinding.connectorType, Equal<spConnectorType>>),})]
	public sealed class BCBindingShopifyExtension : PXCacheExtension<BCBinding>
	{
		public static bool IsActive() { return true; }
	}
}
