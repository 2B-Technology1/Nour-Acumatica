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
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.BigCommerce.API.WebDAV;
using PX.Data;
using PX.Commerce.Core;
using PX.Commerce.Objects;
using PX.Objects.CS;
using System.Linq;

namespace PX.Commerce.BigCommerce
{
	public class BCBigCommerceStoreMaint : BCStoreMaint
	{
		public PXSelect<BCBindingBigCommerce, Where<BCBindingBigCommerce.bindingID, Equal<Current<BCBinding.bindingID>>>> CurrentBindingBigCommerce;

	
		private const string ADMINPATHEXT = "/manage";
		private const string WEBDAVPATHEXT = "/dav";

		public BCBigCommerceStoreMaint()
		{
			base.Bindings.WhereAnd<Where<BCBinding.connectorType, Equal<BCConnector.bcConnectorType>>>();
		}

		#region Actions
		public PXAction<BCBinding> TestConnection;
		[PXButton(IsLockedOnToolbar = true)]
		[PXUIField(DisplayName = "Test Connection", Enabled = false)]
		protected virtual IEnumerable testConnection(PXAdapter adapter)
		{
			Actions.PressSave();

			BCBinding binding = Bindings.Current;
			BCBindingExt bindingExt = CurrentStore.Current ?? CurrentStore.Select();
			BCBindingBigCommerce bindingBigCommerce = CurrentBindingBigCommerce.Current ?? CurrentBindingBigCommerce.Select();

			Boolean webDavEnabled = Entities.Select().Select(_ => _.GetItem<BCEntity>())
				.FirstOrDefault(_ => _.EntityType == BCEntitiesAttribute.ProductImage)?.IsActive ?? false;

			if (binding == null || bindingBigCommerce == null
				|| bindingBigCommerce.StoreBaseUrl == null
				|| (webDavEnabled && bindingBigCommerce.StoreWDAVServerUrl == null))
			{
				throw new PXException(BCMessages.TestConnectionFailedParameters);
			}

			PXLongOperation.StartOperation(this, delegate
			{
				BCBigCommerceStoreMaint graph = PXGraph.CreateInstance<BCBigCommerceStoreMaint>();
				graph.Bindings.Current = binding;
				graph.CurrentStore.Current = bindingExt;
				graph.CurrentBindingBigCommerce.Current = bindingBigCommerce;

				BCRestClient storeClient = BCConnector.GetRestClient(bindingBigCommerce);
				StoreRestDataProvider storeProvider = new StoreRestDataProvider(storeClient);
				StoreCurrencyDataProvider storeCurrency = new StoreCurrencyDataProvider(storeClient);
				try
				{
					var store = storeProvider.Get();
					if (store == null || store.Id == null) throw new PXException(BigCommerceMessages.TestConnectionStoreNotFound);
					var currencies = storeCurrency.Get().Select(c => c.CurrencyCode).ToArray();

					graph.CurrentStore.Cache.SetValueExt<BCBindingExt.defaultStoreCurrency>(bindingExt, store.Currency);
					graph.CurrentStore.Cache.SetValueExt<BCBindingExt.supportedCurrencies>(bindingExt, string.Join(",", currencies));
					graph.CurrentStore.Cache.SetValueExt<BCBindingExt.storeTimeZone>(bindingExt, store.Timezone?.Name);
					graph.CurrentStore.Cache.IsDirty = true;
					graph.CurrentStore.Cache.Update(binding);

					graph.Persist();
				}
				catch (Exception ex)
				{
					throw new PXException(ex, BCMessages.TestConnectionFailedGeneral, ex.Message);
				}

				if (webDavEnabled)
				{
					BCWebDavClient webDavClient = BCConnector.GetWebDavClient(bindingBigCommerce);
					try
					{
						var folder = webDavClient.GetFolder();
						if (folder == null) throw new PXException(BigCommerceMessages.TestConnectionFolderNotFound);
					}
					catch (Exception ex)
					{
						throw new PXException(ex, BCMessages.TestConnectionFailedGeneral, ex.Message);
					}
				}
			});

			return adapter.Get();
		}
		#endregion

		#region BCBinding Events

		public override void _(Events.RowSelected<BCBindingExt> e)
		{
			base._(e);

			BCBindingExt row = e.Row;
			if (row == null) return;
		}

		protected virtual void _(Events.RowPersisting<BCBindingExt> e)
		{
			BCBindingExt row = e.Row as BCBindingExt; 
			BCBinding binding = CurrentBinding.Current ?? CurrentBinding.Select();
			BCBindingBigCommerce bindingBigCommerce = CurrentBindingBigCommerce.Current ?? CurrentBindingBigCommerce.Select();
			if (row == null || bindingBigCommerce == null || binding == null || string.IsNullOrEmpty(bindingBigCommerce.StoreBaseUrl) || string.IsNullOrWhiteSpace(bindingBigCommerce.StoreXAuthClient) || string.IsNullOrWhiteSpace(bindingBigCommerce.StoreXAuthToken))
				return;

			BCRestClient client = BCConnector.GetRestClient(bindingBigCommerce);
			StoreRestDataProvider storeProvider = new StoreRestDataProvider(client);
			StoreCurrencyDataProvider storeCurrency = new StoreCurrencyDataProvider(client);
			try
			{
				var store = storeProvider.Get();
				var currencies = storeCurrency.Get().Select(c => c.CurrencyCode).ToArray();

				CurrentStore.Cache.SetValueExt<BCBindingExt.defaultStoreCurrency>(row, store.Currency);
				CurrentStore.Cache.SetValueExt<BCBindingExt.supportedCurrencies>(row, string.Join(",", currencies));
				CurrentStore.Cache.SetValueExt<BCBindingExt.storeTimeZone>(row, store.Timezone?.Name);
				CurrentStore.Cache.IsDirty = true;
				CurrentStore.Cache.Update(row);
			}
			catch (Exception ex)
			{
				//throw new PXException(ex.Message);
			}			
		}
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(BCConnectorsAttribute), "DefaultConnector", BCConnector.TYPE)]
		public virtual void _(Events.CacheAttached<BCBinding.connectorType> e) { }

		public virtual void _(Events.RowSelected<BCShippingMappings> e)
		{
			var row = e.Row;
			if (row == null) return;
			if (!row.Active.HasValue || row.Active.Value == false)
			{
				PXUIFieldAttribute.SetError<BCShippingMappings.carrierID>(e.Cache, e.Row, null);
				return;
			}

			if (!string.IsNullOrEmpty(row.CarrierID))
			{
				Carrier carrier = PXSelect<Carrier, Where<Carrier.carrierID, Equal<Required<Carrier.carrierID>>>>.Select(this, row.CarrierID);

				if (carrier == null || carrier.IsCommonCarrier == false)
				{
					ShippingMappings.Cache.RaiseExceptionHandling<BCShippingMappings.carrierID>(row, row.CarrierID,
						new PXSetPropertyException<BCShippingMappings.carrierID>(PX.Commerce.Core.BCMessages.ShipViaCodeUsedForPickUpAndCommonCarrierChecked, PXErrorLevel.RowWarning));
				}				
			}
		}

		public override void _(Events.RowSelected<BCBinding> e)
		{
			base._(e);

			BCBinding row = e.Row as BCBinding;
			if (row == null) return;

			//Actions
			TestConnection.SetEnabled(row.BindingID > 0 && row.ConnectorType == BCConnector.TYPE);
		}
		public override void _(Events.RowInserted<BCBinding> e)
		{
			base._(e);

			bool dirty = CurrentBindingBigCommerce.Cache.IsDirty;
			CurrentBindingBigCommerce.Insert();
			CurrentBindingBigCommerce.Cache.IsDirty = dirty;
		}
	
		public virtual void _(Events.FieldVerifying<BCBindingBigCommerce, BCBindingBigCommerce.storeBaseUrl> e)
		{
			string val = e.NewValue?.ToString();
			if (val != null)
			{
				val = val.TrimEnd('/');
				for (int i = 0; i < 10; i++)
				{
					string pattern = "/v" + i;
					if (val.EndsWith(pattern)) val = val.Substring(0, val.LastIndexOf(pattern) + 1);
				}
				if (!val.EndsWith("/")) val += "/";

				e.NewValue = val;
			}
		}		
		public virtual void _(Events.FieldUpdated<BCBindingBigCommerce, BCBindingBigCommerce.storeWDAVServerUrl> e)
		{
			BCBindingBigCommerce row = e.Row;
			if (row == null) return;

			if (!String.IsNullOrEmpty(row.StoreWDAVServerUrl))
			{
				var uri = new Uri(row.StoreWDAVServerUrl);
				var baseUri = uri.GetLeftPart(System.UriPartial.Authority);
				if (string.IsNullOrEmpty(baseUri)) return;

				if (row.StoreWDAVServerUrl.Length - baseUri.Length == WEBDAVPATHEXT.Length)
				{
					var attempWebDAV = row.StoreWDAVServerUrl.Substring(baseUri.Length, WEBDAVPATHEXT.Length);
					if (string.Equals(attempWebDAV, WEBDAVPATHEXT, StringComparison.InvariantCultureIgnoreCase))
					{
						row.StoreAdminUrl = baseUri + ADMINPATHEXT;
						return;
					}
				}
				row.StoreAdminUrl = baseUri + ADMINPATHEXT;
				row.StoreWDAVServerUrl = baseUri + WEBDAVPATHEXT;
			}
		}
		public virtual void _(Events.FieldUpdated<BCBindingBigCommerce, BCBindingBigCommerce.storeAdminUrl> e)
		{
			BCBindingBigCommerce row = e.Row;
			if (row == null) return;

			if (!String.IsNullOrEmpty(row.StoreAdminUrl))
			{
				var uri = new Uri(row.StoreAdminUrl);
				var baseUri = uri.GetLeftPart(System.UriPartial.Authority);
				if (string.IsNullOrEmpty(baseUri)) return;

				if (row.StoreAdminUrl.Length - baseUri.Length == ADMINPATHEXT.Length)
				{
					var attempAdm = row.StoreAdminUrl.Substring(baseUri.Length, ADMINPATHEXT.Length);
					if (string.Equals(attempAdm, ADMINPATHEXT, StringComparison.InvariantCultureIgnoreCase))
					{
						row.StoreWDAVServerUrl = baseUri + WEBDAVPATHEXT;
						return;
					}
				}
				row.StoreAdminUrl = baseUri + ADMINPATHEXT;
				row.StoreWDAVServerUrl = baseUri + WEBDAVPATHEXT;
			}
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
		#endregion

		#region Cache Attached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(BCItemNotAvailModes.List))]
		[PXStringList(
			new[] { BCItemNotAvailModes.DoNothing, BCItemNotAvailModes.DisableItem, BCItemNotAvailModes.PreOrderItem },
			new[] { BCCaptions.DoNothing, BCCaptions.DisableItem, BCCaptions.PreOrderItem })]
		public virtual void BCBindingExt_NotAvailMode_CacheAttached(PXCache sender) { }
		#endregion
	}
}

