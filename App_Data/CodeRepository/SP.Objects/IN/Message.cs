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

namespace SP.Objects.IN
{
	[PX.Common.PXLocalizable]
	public static class Messages
    {
        public const string noUnitCost = "Item unavailable for purchase. Try selecting different unit.";
        public const string noCost = "Item unavailable for purchase.";
        public const string deficiencyWarehouse = "Current Warehouse doesn’t have enough item available. Your order processing may take slightly more time.";
        public const string deficiencyWarehouses = "You've ordered more than available. Processing of your order may take slightly more time.";
		public const string NoAccessToWarehouseData = "No access to warehouse data.";
        
        public const string ShipComplete = "Completely";
        public const string BackOrderAllowed = "Available & Back Order";
        public const string CancelRemainder = "What’s available";

        public const string AtleastOnePackageIsRequired =
            "Sorry, we cannot calculate freight rate with this carrier right now. ~ Please select another carrier or proceed with order - freight rate will be provided to you separately.";

		public const string DefaultOrderFromPortal = "Customer Portal Sales Request ";
		public const string DefaultCreditLimit = "Credit Limit has been exceeded";
		public const string YourCartContainsItemsFor = "Your cart contains {0} items for {1} {2}";
		public const string ShipVia = "Ship Via";
		public const string DeliveryDate = "Delivery Date";
		public const string ItemSuccessfullyAdded = "Item successfully added";
		public const string ItemAddedToCart = "Item {0} has been added to your cart. Do you want to go back?";
		public const string ClearCart = "Clear Cart";
	}
}
