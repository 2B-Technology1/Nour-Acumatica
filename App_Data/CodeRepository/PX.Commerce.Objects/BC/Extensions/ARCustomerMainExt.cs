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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Commerce.Core;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.SM;

namespace PX.Commerce.Objects
{
	public class BCCustomerMaintExt : PXGraphExtension<CustomerMaint>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		//Dimension Key Generator
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[BCCustomNumbering(CustomerRawAttribute.DimensionName, typeof(BCBindingExt.customerTemplate), typeof(BCBindingExt.customerNumberingID),
			typeof(Select2<BCBindingExt,
				InnerJoin<BCBinding, On<BCBinding.bindingID, Equal<BCBindingExt.bindingID>>>,
				Where<BCBinding.connectorType, Equal<Required<BCBinding.connectorType>>,
					And<BCBinding.bindingID, Equal<Required<BCBinding.bindingID>>>>>))]
		public void _(Events.CacheAttached<Customer.acctCD> e) { }

		//Sync Time 
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PX.Commerce.Core.BCSyncExactTime()]
		public void Customer_LastModifiedDateTime_CacheAttached(PXCache sender) { }
		public void _(Events.RowSelected<Customer> e)
		{
			PXUIFieldAttribute.SetVisible<CustomerExt.customerCategory>(e.Cache, e.Row, CommerceFeaturesHelper.CommerceB2B & CommerceFeaturesHelper.ShopifyConnector);
		}
	}
}
