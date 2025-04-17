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

using PX.Data;
using PX.Objects.IN;
using System;

namespace PX.Objects.AR
{
	/// <summary>
	/// This class contains <see cref="ARSalesPrice"/> projection definition.
	/// For <see cref="ARSalesPriceMaint"/> graph, the projection contains joined <see cref="InventoryItem"/>, <see cref="ARPriceClass"/> and <see cref="Customer"/> entities.
	/// </summary>
	public class ARSalesPriceProjectionAttribute : PXProjectionAttribute
	{
		public ARSalesPriceProjectionAttribute() 
			: base(typeof(Select<ARSalesPrice>), new[] { typeof(ARSalesPrice) })
		{ }

		protected override Type GetSelect(PXCache sender)
		{
			if (sender.Graph is ARSalesPriceMaint)
				return typeof(Select2<ARSalesPrice,
					InnerJoin<InventoryItem,
						On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
					LeftJoin<ARPriceClass,
						On<ARSalesPrice.priceType, Equal<PriceTypes.customerPriceClass>,
						And<ARSalesPrice.custPriceClassID, Equal<ARPriceClass.priceClassID>>>,
					LeftJoin<Customer,
						On<ARSalesPrice.priceType, Equal<PriceTypes.customer>,
						And<ARSalesPrice.customerID, Equal<Customer.bAccountID>>>>>>>);

			return base.GetSelect(sender);
		}
	}
}
