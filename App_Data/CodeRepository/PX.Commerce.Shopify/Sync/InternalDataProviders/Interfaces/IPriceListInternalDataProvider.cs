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
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data.SQLTree;
using Microsoft.Extensions.Logging;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Objects.Availability;
using InventoryItem = PX.Objects.IN.InventoryItem;

namespace PX.Commerce.Shopify
{
	/// <summary>
	/// Data Provider interface to implement data provider for the Price List Processor.
	/// </summary>
	public interface IPriceListInternalDataProvider
	{
		/// <summary>
		/// Retrieve All Price Class records with/without specified PriceClassIds
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="priceClassIds"></param>
		/// <returns></returns>
		IEnumerable<PriceListSalesPrice> GetPriceClasses(PXGraph graph, List<string> priceClassIds = null);

		/// <summary>
		/// Retrieve the specified Price Class record
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="priceClassNoteId"></param>
		/// <returns></returns>
		PriceListSalesPrice GetPriceClass(PXGraph graph, Guid priceClassNoteId);

		/// <summary>
		/// Return the whole list of prices for a specified priceClass Id
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="priceClassNoteId"></param>
		/// <returns></returns>ConvertToStr
		IEnumerable<SalesPriceDetail> GetSalesPrices(PXGraph graph, Guid priceClassNoteId);

		/// <summary>
		/// Retrieve All Customer Locations associated with specified PriceClassId
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="priceClassId"></param>
		/// <returns></returns>
		IEnumerable<CustomerLocation> GetCustomerLocationsWithPriceClass(PXGraph graph, string priceClassId);

		/// <summary>
		/// Get the BCSyncDetails for all inventoryItems that exported to Shopify
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="operation"></param>
		/// <param name="inventoryIDs"></param>
		/// <returns></returns>
		IEnumerable<BCSyncDetail> GetBCSyncDetailsForInventoryItem(PXGraph graph, ConnectorOperation operation, Guid?[] inventoryIDs);

		/// <summary>
		/// Get the BCSyncDetails for all Company Locations that exported to Shopify
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="operation"></param>
		/// <param name="customerLocationIDs"></param>
		/// <returns></returns>
		IEnumerable<BCSyncDetail> GetBCSyncDetailsForLocations(PXGraph graph, ConnectorOperation operation, Guid?[] customerLocationIDs);

		/// <summary>
		/// Get the BCSyncDetails for all matrix items from the TemplateItem that exported to Shopify
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="operation"></param>
		/// <param name="inventoryIDs"></param>
		/// <returns></returns>
		IEnumerable<BCSyncDetail> GetBCSyncDetailsForTemplateItem(PXGraph graph, ConnectorOperation operation, Guid?[] inventoryIDs);
	}
}
