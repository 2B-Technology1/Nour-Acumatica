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

using Newtonsoft.Json;
using PX.Commerce.Core.API;
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The package that includes all input data for creating/updating/deleting Catalog, PriceList and PriceListPrice
	/// </summary>
	public class PriceListMutationPackage
	{
		public CatalogCreateInput CatalogCreation { get; set; }

		public CatalogUpdateInput CatalogUpdate { get; set; }

		public string CatalogId { get; set; }

		public PriceListCreateInput PriceListCreation { get; set; }

		public PriceListUpdateInput PriceListUpdate { get; set; }

		public string PriceListId { get; set; }

		public List<PriceListPriceInput> PriceListPriceInputs { get; set; } = new List<PriceListPriceInput>();

		public List<string> VariantsToDelete { get; set; } = new List<string>();

		public CompanyLocationCatalogGQL CatalogData { get; set; }

		public PriceListGQL PriceListData { get; set; }

		public PriceListGQL ExistingPriceListData { get; set; }

		public bool PriceListToDelete { get; set; }

		public bool PriceListToArchived { get; set; }

		public IEnumerable<PriceListPriceGQL> PriceListPriceDetails { get; set; } = new List<PriceListPriceGQL>();

		public Dictionary<string, SalesPriceDetail> SalesPriceMappings { get; set; } = new Dictionary<string, SalesPriceDetail>();

	}
}
