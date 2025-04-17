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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.EP
{
	public class ExpenseClaimDetailSalesAccountID<IsBillable, InventoryID, CustomerID, CustomerLocationID> : BqlFormulaEvaluator<IsBillable, InventoryID, CustomerID, CustomerLocationID>
		where InventoryID : IBqlField
		where CustomerID : IBqlField
		where IsBillable : IBqlField
		where CustomerLocationID: IBqlField
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			bool? isBillable = (bool?)parameters[typeof(IsBillable)];
			if (isBillable != true)
			{
				return null;
			}

			ARSetup setup = SelectFrom<ARSetup>.View.Select(cache.Graph);

			if (setup?.IntercompanySalesAccountDefault == ARAcctSubDefault.MaskLocation)
			{
				int? customerID = (int?)parameters[typeof(CustomerID)];
				PXResult<Customer, Location> result = (PXResult<Customer, Location>)SelectFrom<Customer>
					.LeftJoin<Location>
						.On<Customer.defLocationID.IsEqual<Location.locationID>>
					.Where<Customer.bAccountID.IsEqual<@P.AsInt>>
					.View
					.SelectSingleBound(cache.Graph, null, customerID);

				Customer customer = result;
				Location defaultLocation = result;

				if(customer?.IsBranch == true)
				{
					int? locationID = (int?)parameters[typeof(CustomerLocationID)];
					Location location = SelectFrom<Location>
						.Where<Location.locationID.IsEqual<@P.AsInt>>
						.View
						.SelectSingleBound(cache.Graph, null, locationID);
					return (location ?? defaultLocation)?.CSalesAcctID;
				}
			}

			int? inventoryID = (int?)parameters[typeof(InventoryID)];
			InventoryItem inventoryItem = SelectFrom<InventoryItem>
				.Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>>
				.View
				.SelectSingleBound(cache.Graph, null, inventoryID);

			return inventoryItem?.SalesAcctID;
		}
	}
}
