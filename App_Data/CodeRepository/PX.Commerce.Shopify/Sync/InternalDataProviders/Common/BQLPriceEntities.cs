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
using PX.Data.BQL;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify
{

	/// <summary>
	/// Sequence used to filter inventory items by type
	/// </summary>
	public class InventoryItemsEntityTypeSequence : IBqlConstants,
							IBqlConstantsOf<IImplement<IBqlEquitable>>,
							IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlInt>>>,
							IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlString>>>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public IEnumerable<object> GetValues(PXGraph graph)
		{
			yield return BCEntitiesAttribute.StockItem;
			yield return BCEntitiesAttribute.NonStockItem;
			yield return BCEntitiesAttribute.ProductWithVariant;
		}
	}
	public class ExcludedInventoryItemStatusSequence : IBqlConstants,
								IBqlConstantsOf<IImplement<IBqlEquitable>>,
								IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlInt>>>,
								IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlString>>>
	{
		public IEnumerable<object> GetValues(PXGraph graph)
		{
			yield return INItemStatus.Inactive;
			yield return InventoryItemStatus.Unknown;
			yield return INItemStatus.ToDelete;
			yield return INItemStatus.NoSales;
		}
	}

	public class InvalidSyncStatusSquence : IBqlConstants,
							IBqlConstantsOf<IImplement<IBqlEquitable>>,
							IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlInt>>>,
							IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlString>>>
	{
		public IEnumerable<object> GetValues(PXGraph graph)
		{
			yield return BCSyncStatusAttribute.Filtered;
			yield return BCSyncStatusAttribute.Invalid;
			yield return BCSyncStatusAttribute.Skipped;
			yield return BCSyncStatusAttribute.Deleted;
		}
	}

}
