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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Commerce.Shopify.API.REST;
using PX.Api;
using PX.Data;
using RestSharp;
using PX.Commerce.Core;
using PX.Objects.CA;
using PX.Commerce.Objects;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Common;

namespace PX.Commerce.Shopify
{
	public class BCShopifyStoreMaint : BCStoreMaint
	{
		public PXSelect<BCBindingShopify, Where<BCBindingShopify.bindingID, Equal<Current<BCBinding.bindingID>>>> CurrentBindingShopify;

		public BCShopifyStoreMaint()
		{
			base.Bindings.WhereAnd<Where<BCBinding.connectorType, Equal<SPConnector.spConnectorType>>>();

			PXStringListAttribute.SetList<BCBindingExt.visibility>(base.CurrentStore.Cache, null,
				new[]
				{
					BCItemVisibility.Visible,
					BCItemVisibility.Invisible,
				},
				new[]
				{
					BCCaptions.Visible,
					BCCaptions.Invisible,
				});
		}

		#region Cache Attached
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Shopify Location")]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual void ExportBCLocations_ExternalLocationID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Shopify Location")]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual void ImportBCLocations_ExternalLocationID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(BCItemNotAvailModes.List))]
		[PXStringList(
			new[] { BCItemNotAvailModes.DoNothing, BCItemNotAvailModes.DisableItem, BCItemNotAvailModes.PreOrderItem },
			new[] { BCCaptions.DoNothing, BCCaptions.DisableItem, BCCaptions.ContinueSellingItem } )]
		public virtual void BCBindingExt_NotAvailMode_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXSelector(typeof(Search2<CCProcessingCenter.processingCenterID,
			InnerJoin<CCProcessingCenterPmntMethod,
				On<CCProcessingCenterPmntMethod.processingCenterID, Equal<CCProcessingCenter.processingCenterID>>,
			LeftJoin<CCProcessingCenterDetail,
				On<CCProcessingCenterDetail.processingCenterID, Equal<CCProcessingCenter.processingCenterID>,
					And<CCProcessingCenterDetail.detailID, Equal<ShopifyPayments.ShopifyPluginHelper.SettingsKeys.Const_StoreName>>>
			>>,
			Where<CCProcessingCenter.isActive, Equal<True>,
				And2<
					Where<CCProcessingCenter.processingTypeName, IsNull,
						Or<CCProcessingCenter.processingTypeName, NotEqual<ShopifyPayments.ShopifyPaymentsProcessingPlugin.Const_PluginName>,
						Or<CCProcessingCenterDetail.value, Equal<Current<BCPaymentMethods.bindingID>>>>>,
				And<CCProcessingCenterPmntMethod.isActive, Equal<True>,
				And<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<BCPaymentMethods.paymentMethodID>>,
				And<CCProcessingCenter.cashAccountID, Equal<Current<BCPaymentMethods.cashAccountID>>>>>>>>))]
		public virtual void BCPaymentMethods_ProcessingCenterID_CacheAttached(PXCache sender) { }
		#endregion

		#region Actions
		public PXAction<BCBinding> TestConnection;
		[PXButton(IsLockedOnToolbar = true)]
		[PXUIField(DisplayName = "Test Connection", Enabled = false)]
		protected virtual IEnumerable testConnection(PXAdapter adapter)
		{
			Actions.PressSave();

			BCBinding binding = Bindings.Current;
			BCBindingExt bindingExt = CurrentStore.Current ?? CurrentStore.Select();
			BCBindingShopify bindingShopify = CurrentBindingShopify.Current ?? CurrentBindingShopify.Select();

			if (binding.ConnectorType != SPConnector.TYPE) return adapter.Get();
			if (binding == null || bindingShopify == null || bindingShopify.ShopifyApiBaseUrl == null
				|| string.IsNullOrWhiteSpace(bindingShopify.ShopifyAccessToken))
			{
				throw new PXException(BCMessages.TestConnectionFailedParameters);
			}

			PXLongOperation.StartOperation(this, delegate
			{
				BCShopifyStoreMaint graph = PXGraph.CreateInstance<BCShopifyStoreMaint>();
				graph.Bindings.Current = binding;
				graph.CurrentStore.Current = bindingExt;
				graph.CurrentBindingShopify.Current = bindingShopify;

				StoreRestDataProvider restClient = new StoreRestDataProvider(SPConnector.GetRestClient(bindingShopify));
				try
				{
					var store = restClient.Get();
					if (store == null || store.Id == null)
						throw new PXException(ShopifyMessages.TestConnectionStoreNotFound);

					graph.CurrentBindingShopify.Cache.SetValueExt<BCBindingShopify.shopifyStoreUrl>(bindingShopify, store.Domain);
					graph.CurrentBindingShopify.Cache.IsDirty = true;
					graph.CurrentBindingShopify.Update(bindingShopify);

					graph.CurrentStore.Cache.SetValueExt<BCBindingExt.defaultStoreCurrency>(bindingExt, store.Currency);
					graph.CurrentStore.Cache.SetValueExt<BCBindingExt.supportedCurrencies>(bindingExt, string.Join(",", store.EnabledPresentmentCurrencies));
					graph.CurrentStore.Cache.SetValueExt<BCBindingExt.storeTimeZone>(bindingExt, store.Timezone);
					graph.CurrentStore.Cache.IsDirty = true;
					graph.CurrentStore.Cache.Update(binding);
					graph.Persist();
				}
				catch (Exception ex)
				{
					throw new PXException(ex, BCMessages.TestConnectionFailedGeneral, ex.Message);
				}
			});

			return adapter.Get();
		}
		#endregion

		#region BCBinding Events
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(BCConnectorsAttribute), "DefaultConnector", SPConnector.TYPE)]
		public virtual void _(Events.CacheAttached<BCBinding.connectorType> e) { }

		public override void _(Events.RowSelected<BCBinding> e)
		{
			base._(e);

			BCBinding row = e.Row as BCBinding;
			if (row == null) return;

			//Actions
			TestConnection.SetEnabled(row.BindingID > 0 && row.ConnectorType == SPConnector.TYPE);
		}
		public override void _(Events.RowSelected<BCBindingExt> e)
		{
			base._(e);

			BCBindingExt row = e.Row as BCBindingExt;
			if (row == null) return;

			PXStringListAttribute.SetList<BCBindingExt.availability>(e.Cache, row, new[] {
					BCItemAvailabilities.AvailableTrack,
					BCItemAvailabilities.AvailableSkip,
					BCItemAvailabilities.DoNotUpdate,
					BCItemAvailabilities.Disabled,
				},
				new[]
				{
					BCCaptions.AvailableTrack,
					BCCaptions.AvailableSkip,
					BCCaptions.DoNotUpdate,
					BCCaptions.Disabled,
				});
		}

		public virtual void _(Events.RowSelected<BCBindingShopify> e)
		{
			BCBindingShopify row = e.Row;
			if (row == null) return;

			var entities = Entities.Select().RowCast<BCEntity>();

			if (PXAccess.FeatureInstalled<FeaturesSet.shopifyPOS>() && row.ShopifyPOS == true && entities?.FirstOrDefault(x => x.EntityType == BCEntitiesAttribute.Order)?.IsActive == true)
			{
				PXUIFieldAttribute.SetRequired<BCBindingShopify.pOSDirectOrderType>(e.Cache, true);
				PXUIFieldAttribute.SetRequired<BCBindingShopify.pOSShippingOrderType>(e.Cache, true);
				PXDefaultAttribute.SetPersistingCheck<BCBindingShopify.pOSDirectOrderType>(e.Cache, e.Row, PXPersistingCheck.NullOrBlank);
				PXDefaultAttribute.SetPersistingCheck<BCBindingShopify.pOSShippingOrderType>(e.Cache, e.Row, PXPersistingCheck.NullOrBlank);
			}
			else
			{
				PXUIFieldAttribute.SetRequired<BCBindingShopify.pOSDirectOrderType>(e.Cache, false);
				PXUIFieldAttribute.SetRequired<BCBindingShopify.pOSShippingOrderType>(e.Cache, false);
				PXDefaultAttribute.SetPersistingCheck<BCBindingShopify.pOSDirectOrderType>(e.Cache, e.Row, PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<BCBindingShopify.pOSShippingOrderType>(e.Cache, e.Row, PXPersistingCheck.Nothing);
			}
		}

		public override void _(Events.RowInserted<BCBinding> e)
		{
			base._(e);

			bool dirty = CurrentBindingShopify.Cache.IsDirty;
			CurrentBindingShopify.Insert();
			CurrentBindingShopify.Cache.IsDirty = dirty;
		}
		protected virtual void _(Events.RowPersisting<BCBindingExt> e)
		{
			BCBindingExt row = e.Row as BCBindingExt;
			BCBinding binding = CurrentBinding.Current ?? CurrentBinding.Select();
			BCBindingShopify bindingShopify = CurrentBindingShopify.Current ?? CurrentBindingShopify.Select();
			if (row == null || binding == null || bindingShopify == null) return;

			FetchDataFromShopify(row, bindingShopify);
		}

		protected virtual void _(Events.FieldVerifying<BCBindingShopify, BCBindingShopify.shopifyApiBaseUrl> e)
		{
			string val = e.NewValue?.ToString();
			if (val != null)
			{
				val = val.ToLower();
				if (!val.EndsWith("/")) val += "/";
				if (val.EndsWith(".myshopify.com/")) val += "admin/";
				if (!val.EndsWith("/admin/"))
				{
					throw new PXSetPropertyException(ShopifyMessages.InvalidStoreUrl, PXErrorLevel.Warning);
				}
				e.NewValue = val;
			}
		}

		protected virtual void _(Events.FieldSelecting<BCBindingShopify, BCBindingShopify.shopifyApiVersion> e)
		{
			string ApiVersion = WebConfig.GetString(ShopifyConstants.ShopifyAPIVersion, ShopifyConstants.ApiVersion_202301);
			e.ReturnValue = ApiVersion;
		}

		public override void _(Events.FieldUpdated<BCEntity, BCEntity.isActive> e)
		{
			base._(e);

			BCEntity row = e.Row;
			if (row == null || row.CreatedDateTime == null) return;

			if (row.IsActive == true)
			{
				if (row.EntityType == BCEntitiesAttribute.ProductWithVariant)
					if (PXAccess.FeatureInstalled<FeaturesSet.matrixItem>() == false)
					{
						EntityReturn(row.EntityType).IsActive = false;
						e.Cache.Update(EntityReturn(row.EntityType));
						throw new PXSetPropertyException(BCMessages.MatrixFeatureRequired);
					}
			}
		}

		protected void FetchDataFromShopify(BCBindingExt rowExt, BCBindingShopify rowShopify)
		{
			if (rowShopify == null || string.IsNullOrEmpty(rowShopify.ShopifyApiBaseUrl) || string.IsNullOrWhiteSpace(rowShopify.ShopifyAccessToken))
				return;

			StoreRestDataProvider restClient = new StoreRestDataProvider(SPConnector.GetRestClient(rowShopify));
			try
			{
				var store = restClient.Get();
				CurrentBindingShopify.Cache.SetValueExt<BCBindingShopify.shopifyStoreUrl>(rowShopify, store.Domain);
				CurrentBindingShopify.Cache.IsDirty = true;
				CurrentBindingShopify.Cache.Update(rowShopify);

				CurrentStore.Cache.SetValueExt<BCBindingExt.defaultStoreCurrency>(rowExt, store.Currency);
				CurrentStore.Cache.SetValueExt<BCBindingExt.supportedCurrencies>(rowExt, string.Join(",", store.EnabledPresentmentCurrencies));
				CurrentStore.Cache.SetValueExt<BCBindingExt.storeTimeZone>(rowExt, store.Timezone);
				CurrentStore.Cache.IsDirty = true;
				CurrentStore.Cache.Update(store);

			}
			catch (Exception ex)
			{
				//throw new PXException(ex.Message);
			}
		}
		#endregion
	}
}
