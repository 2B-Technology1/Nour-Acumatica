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
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// An amount that's allocated to a line based on an associated discount application.
	/// </summary>
	public class DiscountAllocationGQL
	{
		/// <summary>
		/// The money amount that's allocated to a line based on the associated discount application in shop and presentment currencies.
		/// </summary>
		[JsonProperty("allocatedAmountSet")]
		[GraphQLField("allocatedAmountSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag AllocatedAmountSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.DiscountAmountPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? AllocatedAmountPresentment { get => AllocatedAmountSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The discount application that the allocated amount originated from.
		/// </summary>
		[JsonProperty("discountApplication")]
		[GraphQLField("discountApplication", GraphQLConstants.DataType.Object, typeof(DiscountApplicationGQL))]
		public DiscountApplicationGQL DiscountApplication { get; set; }
	}

	/// <summary>
	/// Discount applications capture the intentions of a discount source at the time of application on an order's line items or shipping lines.
	/// </summary>
	public class DiscountApplicationGQL: BCAPIEntity
	{
		/// <summary>
		/// The method by which the discount's value is applied to its entitled items.
		/// </summary>
		[JsonProperty("allocationMethod")]
		[GraphQLField("allocationMethod", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string AllocationMethod { get; set; }

		/// <summary>
		/// How the discount amount is distributed on the discounted lines.
		/// </summary>
		[JsonProperty("targetSelection")]
		[GraphQLField("targetSelection", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string TargetSelection { get; set; }

		/// <summary>
		/// An ordered index that can be used to identify the discount application and indicate the precedence of the discount application for calculations.
		/// </summary>
		[JsonProperty("index")]
		[GraphQLField("index", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int Index { get; set; }

		/// <summary>
		/// An ordered index that can be used to identify the discount application and indicate the precedence of the discount application for calculations.
		/// </summary>
		[JsonProperty("targetType", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("targetType", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string TargetType { get; set; }

		/// <summary>
		/// The value of the discount application.
		/// </summary>
		[JsonProperty("value")]
		[GraphQLField("value", GraphQLConstants.DataType.Union, typeof(PricingValueGQL))]
		public PricingValueGQL Value { get; set; }

		#region Interfaces Fields
		/// <summary>
		/// The description of the discount application.
		/// </summary>

		[JsonProperty("description")]		
		public string Description { get; set; }

		/// <summary>
		/// The title of the discount application or the title of the application as defined by the Script.
		/// </summary>
		[JsonProperty("title")]
		public string Title { get; set; }

		/// <summary>
		/// The string identifying the discount code that was used at the time of application.
		/// </summary>
		[JsonProperty("code")]
		public string Code { get; set; }

		/// <summary>
		/// Applied interfaces used in <see cref="DiscountApplicationGQL"/>.
		/// </summary>
		[GraphQLField(nameof(DiscountApplicationGQLInterfaces), GraphQLConstants.DataType.Interface, typeof(DiscountApplicationGQLInterfaces))]
		public DiscountApplicationGQLInterfaces DiscountApplicationGQLInterfaces { get; set; }
		#endregion
	}
}
