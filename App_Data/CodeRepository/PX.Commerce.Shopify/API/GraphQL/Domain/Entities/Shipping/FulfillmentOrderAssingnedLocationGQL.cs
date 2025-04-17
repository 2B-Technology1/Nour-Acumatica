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
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The fulfillment order's assigned location. This is the location where the fulfillment is expected to happen.
	/// </summary>
	public class FulfillmentOrderAssingnedLocationGQL : BaseAddress
	{
		/// <summary>
		/// The two-letter code for the country of the assigned location.
		/// </summary>
		[JsonProperty("countryCode", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("countryCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string CountryCode { get; set; }

		/// <summary>
		/// The location where the fulfillment is expected to happen.
		/// This value might be different from FulfillmentOrderAssignedLocation if the location's attributes were updated after the fulfillment order was taken into work of canceled.
		/// </summary>
		[JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("location", GraphQLConstants.DataType.Object, typeof(LocationGQL))]
		public LocationGQL Location { get; set; }

		/// <summary>
		/// The name of the assigned location.
		/// </summary>
		[JsonProperty("name")]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Name { get; set; }

		/// <summary>
		/// The phone number of the assigned location.
		/// </summary>
		[JsonProperty("phone")]
		[GraphQLField("phone", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string PhoneNumber { get; set; }

		/// <summary>
		/// The province of the destination.
		/// </summary>
		[JsonProperty("province")]
		[GraphQLField("province", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Province { get; set; }
	}
}
