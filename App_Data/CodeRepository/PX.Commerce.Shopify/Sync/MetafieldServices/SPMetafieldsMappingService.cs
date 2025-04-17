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

using IdentityServer4.Validation;
using PX.Commerce.Shopify.API.GraphQL;
using PX.Commerce.Shopify.API.REST;
using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;


namespace PX.Commerce.Shopify
{
	public class SPMetafieldsMappingServiceFactory : ISPMetafieldsMappingServiceFactory
	{
		public ISPMetafieldsMappingService GetInstance(IMetafieldsGQLDataProvider dataProvider) => new SPMetafieldsMappingService(dataProvider);
	}

	public class SPMetafieldsMappingService :  ISPMetafieldsMappingService
	{
		private IMetafieldsGQLDataProvider _metafieldsDataProvider;

		/// <summary>
		/// contains the definitions that have been loaded from shopify
		/// </summary>
		private Dictionary<string, MetafieldDefintionGQL> _metafieldsDefnitionCache = new Dictionary<string, MetafieldDefintionGQL>();
		/// <summary>
		/// Contains the list of types for which we have already tried to load in the cache.
		/// </summary>
		private List<string> _loadedTypes = new List<string>();
		public ShopifyMetafieldType DefaultMetafieldType { get; } = ShopifyMetafieldType.single_line_text_field;

		public SPMetafieldsMappingService(IMetafieldsGQLDataProvider dataProvider)
		{
			_metafieldsDataProvider = dataProvider;
		}

		/// <summary>
		/// Return a MetafieldValue object that contains the formatted value of the metafield according to its type defined in Shopify as well as its
		/// defined type in Shopify and its category.
		/// Note that for list types, the Type of the metafield will represent the subtype (what the list contains).
		/// </summary>
		/// <param name="ns">namespace used in the ERP (mapping)</param>
		/// <param name="key">key used in the ERP (mapping)</param>		
		/// <param name="key">key used in the ERP (mapping)</param>		
		/// <param name="fieldValue">the string value stored in the ERP</param>
		/// <returns></returns>
		public MetafieldValue GetFormattedMetafieldValue(string entityType, string ns, string key, string fieldValue)
		{
			var definition = GetMetafieldDefitionFromShopify(entityType, ns, key, true);

			if (definition == null)
				return new MetafieldValue(fieldValue, MetafieldCategory.SingleValue, this.DefaultMetafieldType, definition?.Type.Name);

			var shopifyKey = $"{ns}.{key}";
			
			return new MetafieldValue(fieldValue, definition.TypeCategory, definition.InternalType, definition.Type.Name);
		}

		/// <summary>
		/// Verifies spelling of metafields mapping 
		/// </summary>
		/// <param name="nameSpaceField"></param>
		/// <param name="keyField"></param>
		/// <param name="metafields"></param>
		/// <exception cref="PXException"></exception>
		public Tuple<string, string> CorrectMetafieldName(string entityType, string nameSpaceField, string keyField, IEnumerable<MetafieldData> metafields)
		{
			//first check whether it is known metafield defined 			
			var caseSensitiveMetafield = metafields?.FirstOrDefault(x => string.Equals(x.Namespace, nameSpaceField) && string.Equals(x.Key, keyField));

			//Found in the list of the entity metafields. No need to correct it.
			if (caseSensitiveMetafield != null)
				return new Tuple<string, string>(caseSensitiveMetafield.Namespace, caseSensitiveMetafield.Key);

			//Check in the list of predefined metafielfs
			var metafieldDefinitionCaseSensitive = GetMetafieldDefitionFromShopify(entityType, nameSpaceField, keyField, true);
			if (metafieldDefinitionCaseSensitive != null)
				return new Tuple<string, string>(metafieldDefinitionCaseSensitive.Namespace, metafieldDefinitionCaseSensitive.Key);

			//Could not find it. Look whether case insensitive search will find it
			//in which case we correct it.
			var caseInsensetiveMetafield = metafields?.FirstOrDefault(x => string.Equals(x.Namespace, nameSpaceField, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Key, keyField, StringComparison.OrdinalIgnoreCase));
			if (caseInsensetiveMetafield != null)
				return new Tuple<string, string>(caseInsensetiveMetafield.Namespace, caseInsensetiveMetafield.Key);

			//look in the list of predefined metafields but this time case insensitive
			var metafieldDefinitionCaseInsensitive = GetMetafieldDefitionFromShopify(entityType, nameSpaceField, keyField, isKeySensitive: false);
			if (metafieldDefinitionCaseInsensitive != null)
				return new Tuple<string, string>(metafieldDefinitionCaseInsensitive.Namespace, metafieldDefinitionCaseInsensitive.Key);

			//nothing found. Return the original name
			return new Tuple<string, string>(nameSpaceField, keyField);
		}

		/// <summary>
		/// Translate a metafield definition from Shopify to internal definition.
		/// Basically, determines whether it is a list or a base type.
		/// If the type defined in Shopify is not supported it will be translated to a single string.
		/// </summary>
		/// <returns></returns>
		protected virtual void SetMetafieldDefinitionNodeType(MetafieldDefintionGQL metafieldDefinitionData)
		{
			var category = MetafieldCategory.SingleValue;
			var targetType = ShopifyMetafieldType.single_line_text_field;
			var type = metafieldDefinitionData.Type.Name.ToLower();

			if (type.StartsWith("list."))
			{
				type = type.RemoveFromStart("list.");
				category = MetafieldCategory.List;
			}

			var validType = Enum.TryParse(type, out targetType);

			if (!validType)
				targetType = ShopifyMetafieldType.NotSupportedShopifyType;

			if (targetType == ShopifyMetafieldType.weight ||
				targetType == ShopifyMetafieldType.dimension ||
				targetType == ShopifyMetafieldType.rating ||
				targetType == ShopifyMetafieldType.json ||
				targetType == ShopifyMetafieldType.money ||
				targetType == ShopifyMetafieldType.volume)
			{
				category = category == MetafieldCategory.SingleValue ? MetafieldCategory.jSon : MetafieldCategory.jSonList;
			}

			metafieldDefinitionData.InternalType = targetType;
			metafieldDefinitionData.TypeCategory = category;
		}

		/// <summary>
		/// Retrieves metafields definitions from Shopify and store them in a cache.
		/// Note that this method loads the definitions of metafields and not their actual values
		/// </summary>
		/// <param name="entityType"></param>
		/// <param name="ns"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="PXException"></exception>
		protected virtual MetafieldDefintionGQL GetMetafieldDefitionFromShopify(string entityType, string ns, string key, Boolean isKeySensitive)
		{
			string lookupCacheKey = GetCacheKey(entityType, ns, key);

			if (!_loadedTypes.Contains(entityType) && !_metafieldsDefnitionCache.ContainsKey(lookupCacheKey))
			{
				LoadMetafieldsForEntityType(entityType);
			}
						
			if (isKeySensitive && _metafieldsDefnitionCache.ContainsKey(lookupCacheKey))
				return _metafieldsDefnitionCache[lookupCacheKey];
			else if (!isKeySensitive)
			{
				var metafield = _metafieldsDefnitionCache.FirstOrDefault(x => x.Key.ToLower() == lookupCacheKey.ToLower());
				return metafield.Value;
			}

			return null;
		}

		protected virtual void LoadMetafieldsForEntityType(string entityType)
		{
			var nodes = _metafieldsDataProvider.GetAllForEntityTypeAsync(entityType).Result.ToList();

			if (!_loadedTypes.Contains(entityType))
				_loadedTypes.Add(entityType);

			if (nodes == null || nodes.Count == 0)
				return;

			foreach (var node in nodes)
			{
				SetMetafieldDefinitionNodeType(node);
				var cacheKey = GetCacheKey(entityType, node.Namespace, node.Key);
				if (!_metafieldsDefnitionCache.ContainsKey(cacheKey))
					_metafieldsDefnitionCache.Add(cacheKey, node);
			}
		}
		

		private static string GetCacheKey(string entityType, string ns, string key)
		{
			return $"{entityType}.{ns}.{key}";
		}
	
	}
}


