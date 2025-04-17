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
using PX.Commerce.BigCommerce;
using PX.Commerce.Core;
using PX.Data.ReferentialIntegrity.Attributes;
using static PX.Commerce.BigCommerce.BCConnector;
using PX.Commerce.Objects;
namespace PX.Commerce.BigCommerce
{
	/// <summary>
	/// Represents a binding for a BigCommerce store.
	/// </summary>
	[Serializable]
	[PXCacheName("BigCommerce Settings")]
	public class BCBindingBigCommerce : IBqlTable
	{
		/// <summary>
		/// Utility to retrieve a binding by binding ID.
		/// </summary>
		public class PK : PrimaryKeyOf<BCBindingBigCommerce>.By<BCBindingBigCommerce.bindingID>
		{
			/// <summary>
			/// Find the binding with the specified binding ID.
			/// </summary>
			/// <param name="graph">The graph to search.</param>
			/// <param name="binding">The ID of the binding to find.</param>
			/// <returns>The found binding or null if not found.</returns>
			public static BCBindingBigCommerce Find(PXGraph graph, int? binding, PKFindOptions options = PKFindOptions.None) => FindBy(graph, binding, options);
		}

		#region BindingID
		/// <summary>
		/// The primary key and identity of the binding.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(BCBinding.bindingID))]
		[PXUIField(DisplayName = "Store", Visible = false)]
		[PXParent(typeof(Select<BCBinding, Where<BCBinding.bindingID, Equal<Current<BCBindingBigCommerce.bindingID>>>>))]
		public int? BindingID { get; set; }

		///<inheritdoc cref="BindingID"/>
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		#endregion

		//Connection
		#region StoreBaseUrl
		/// <summary>
		/// The base URL of the BigCommerce store.
		/// </summary>
		[PXDBString(50, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "API Path")]
		[PXDefault()]
		public virtual string StoreBaseUrl { get; set; }

		///<inheritdoc cref="StoreBaseUrl"/>
		public abstract class storeBaseUrl : PX.Data.BQL.BqlString.Field<storeBaseUrl> { }
		#endregion
		#region StoreXAuthClient

		/// <summary>
		/// The client ID of the BigCommerce store.
		/// </summary>
		//[PXDBString(50, IsUnicode = true, InputMask = "")]
		[PXRSACryptString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Client ID")]
		[PXDefault()]
		public virtual string StoreXAuthClient { get; set; }

		///<inheritdoc cref="StoreXAuthClient"/>
		public abstract class storeXAuthClient : PX.Data.BQL.BqlString.Field<storeXAuthClient> { }
		#endregion
		#region StoreXAuthToken

		/// <summary>
		/// The access token of the BigCommerce store.
		/// </summary>
		//[PXDBString(50, IsUnicode = true, InputMask = "")]
		[PXRSACryptString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Access Token")]
		[PXDefault()]
		public virtual string StoreXAuthToken { get; set; }

		///<inheritdoc cref="StoreXAuthToken"/>
		public abstract class storeXAuthToken : PX.Data.BQL.BqlString.Field<storeXAuthToken> { }
		#endregion

		#region StoreWDAVServerUrl

		/// <summary>
		/// The URL of the DAV server of the BigCommerce store.
		/// </summary>
		[PXDBString(100, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "WebDAV Path")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.ProductImage })]
		public virtual string StoreWDAVServerUrl { get; set; }

		///<inheritdoc cref="StoreWDAVServerUrl"/>
		public abstract class storeWDAVServerUrl : PX.Data.BQL.BqlString.Field<storeWDAVServerUrl> { }

		#endregion

		#region StoreWDAVClientUser

		/// <summary>
		/// The username for the DAV server of the BigCommerce store.
		/// </summary>
		[PXDBString(50, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "WebDAV Username")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.ProductImage })]
		public virtual string StoreWDAVClientUser { get; set; }

		///<inheritdoc cref="StoreWDAVClientUser"/>
		public abstract class storeWDAVClientUser : PX.Data.BQL.BqlString.Field<storeWDAVClientUser> { }

		#endregion
		#region StoreWDAVClientPass

		/// <summary>
		/// The password for the DAV server of the BigCommerce store.
		/// </summary>
		[PXRSACryptString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "WebDAV Password")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.ProductImage })]
		public virtual string StoreWDAVClientPass { get; set; }

		///<inheritdoc cref="StoreWDAVClientPass"/>
		public abstract class storeWDAVClientPass : PX.Data.BQL.BqlString.Field<storeWDAVClientPass> { }

		#endregion

		#region StoreAdminURL

		/// <summary>
		/// The admin URL of the BigCommerce store.
		/// </summary>
		[PXDBString(100, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Store Admin Path")]
		[PXDefault()]
		public virtual string StoreAdminUrl { get; set; }

		///<inheritdoc cref="StoreAdminUrl"/>
		public abstract class storeAdminUrl : PX.Data.BQL.BqlString.Field<storeAdminUrl> { }

		#endregion
	}

	/// <summary>
	/// Cache extension for BC Binding for Big Commerce specific features.
	/// </summary>
	[PXPrimaryGraph(new Type[] { typeof(BCBigCommerceStoreMaint) },
					new Type[] { typeof(Where<BCBinding.connectorType, Equal<bcConnectorType>>)})]
	public sealed class BCBindingBigCommerceExtension : PXCacheExtension<BCBinding>
	{
		public static bool IsActive() { return true; }
	}
}
