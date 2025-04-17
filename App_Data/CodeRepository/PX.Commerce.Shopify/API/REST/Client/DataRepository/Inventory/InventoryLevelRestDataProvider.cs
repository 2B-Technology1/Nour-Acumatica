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

using PX.Commerce.Core;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using static PX.Data.BQL.BqlPlaceholder;

namespace PX.Commerce.Shopify.API.REST
{
	public class InventoryLevelRestDataProviderFactory : ISPRestDataProviderFactory<IInventoryLevelRestDataProvider<InventoryLevelData>>
	{
		public virtual IInventoryLevelRestDataProvider<InventoryLevelData> CreateInstance(IShopifyRestClient restClient) => new InventoryLevelRestDataProvider(restClient);
	}

	public class InventoryLevelRestDataProvider : RestDataProviderBase, IInventoryLevelRestDataProvider<InventoryLevelData>
	{
		protected override string GetListUrl { get; } = "inventory_levels.json";
		protected override string GetSingleUrl => throw new NotImplementedException();
		protected override string GetSearchUrl => throw new NotImplementedException();
		private string GetDeleteUrl { get; } = "inventory_levels.json?inventory_item_id={0}&location_id={1}";
		private string GetPostSetUrl { get; } = "inventory_levels/set.json";
		private string GetPostAdjustUrl { get; } = "inventory_levels/adjust.json";
		private string GetPostConnectUrl { get; } = "inventory_levels/connect.json";

		public InventoryLevelRestDataProvider(IShopifyRestClient restClient) : base()
		{
			ShopifyRestClient = restClient;
		}

		public virtual InventoryLevelData Create(InventoryLevelData entity) => throw new NotImplementedException();

		public virtual InventoryLevelData Update(InventoryLevelData entity) => throw new NotImplementedException();
		public virtual InventoryLevelData Update(InventoryLevelData entity, string id) => throw new NotImplementedException();

		public virtual bool Delete(InventoryLevelData entity, string id) => throw new NotImplementedException();

		public virtual bool Delete(string id) => throw new NotImplementedException();

		public virtual bool Delete(string inventoryItemId, string inventoryLocationId)
		{
			var request = BuildRequest(string.Format(GetDeleteUrl, inventoryItemId, inventoryLocationId), nameof(Delete), null, null);
			return ShopifyRestClient.Delete(request);
		}

		public virtual IEnumerable<InventoryLevelData> GetAll(IFilter filter, CancellationToken cancellationToken = default)
		{
			if (filter == null) throw new PXException(ShopifyMessages.InventoryLevelGetAllNoFilter);
			return GetAll<InventoryLevelData, InventoryLevelsResponse>(filter, cancellationToken: cancellationToken);
		}

		public virtual InventoryLevelData GetByID(string id) => throw new NotImplementedException();

		public virtual InventoryLevelData AdjustInventory(InventoryLevelData entity)
		{
			APIHelper.LogIntoProfiler(ShopifyRestClient.Logger,
				string.Format("{0}: adjusting {1} entry", BCCaptions.CommerceLogCaption, entity.GetType().ToString()),
				new BCLogTypeScope(GetType()), entity);
			
			var request = BuildRequest(GetPostAdjustUrl, nameof(AdjustInventory), null, null);
			return ShopifyRestClient.Post<InventoryLevelData, InventoryLevelResponse>(request, entity, false);
		}

		public virtual InventoryLevelData SetInventory(InventoryLevelData entity)
		{
			APIHelper.LogIntoProfiler(ShopifyRestClient.Logger,
				string.Format("{0}: setting {1} entry", BCCaptions.CommerceLogCaption, entity.GetType().ToString()),
				new BCLogTypeScope(GetType()), entity);
			
			var request = BuildRequest(GetPostSetUrl, nameof(SetInventory), null, null);
			return ShopifyRestClient.Post<InventoryLevelData, InventoryLevelResponse>(request, entity, false);
		}

		public virtual InventoryLevelData ConnectInventory(InventoryLevelData entity)
		{
			APIHelper.LogIntoProfiler(ShopifyRestClient.Logger,
				string.Format("{0}: connecting {1} entry", BCCaptions.CommerceLogCaption, entity.GetType().ToString()),
				new BCLogTypeScope(GetType()), entity);
			
			var request = BuildRequest(GetPostConnectUrl, nameof(ConnectInventory), null, null);
			return ShopifyRestClient.Post<InventoryLevelData, InventoryLevelResponse>(request, entity, false);
		}
	}
}
