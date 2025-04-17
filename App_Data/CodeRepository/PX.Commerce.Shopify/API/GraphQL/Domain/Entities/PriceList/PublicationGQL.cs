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
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PublicationGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// Whether new products are automatically published to this publication.
		/// </summary>
		[JsonProperty("autoPublish")]
		[GraphQLField("autoPublish", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? AutoPublish { get; set; }

		/// <summary>
		/// The catalog associated with the publication.
		/// </summary>
		[JsonProperty("catalog")]
		[GraphQLField("catalog", GraphQLConstants.DataType.Object, typeof(CompanyLocationCatalogGQL))]
		public CompanyLocationCatalogGQL Catalog { get; set; }

		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// Whether the publication supports future publishing.
		/// </summary>
		[JsonProperty("supportsFuturePublishing", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("supportsFuturePublishing", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? SupportsFuturePublishing { get; set; }
	}
}
