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

using System.Linq;

namespace PX.Commerce.BigCommerce
{
	public static class BigCommerceConstants
	{
		//Availability Values
		public const string AvailabilityAvailable = "available";
		public const string AvailabilityDisabled = "disabled";
		public const string AvailabilityPreOrder = "preorder";
		//Inventory Tracking Values
		public const string InventoryTrackingProduct = "product";
		public const string InventoryTrackingVariant = "variant";
		public const string InventoryTrackingNone = "none";
		//HTTP Responses
		/// <summary>
		/// Typically indicates that a partial failure has occurred, such as when a POST or PUT request is successful, but saving the URL has failed.
		/// </summary>
		public const string HttpPartialSuccess = "207";
		/// <summary>
		/// This is the result of missing required fields, or of invalid data.
		/// </summary>
		public const string HttpInvalidData = "422";
	}
}
