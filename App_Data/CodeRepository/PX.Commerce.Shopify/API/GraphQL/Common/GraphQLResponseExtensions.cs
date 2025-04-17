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
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// GraphQL extension response part
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class GraphQLResponseExtension
	{
		/// <summary>
		/// The cost details
		/// </summary>
		[JsonProperty("cost")]
		public CostExtension Cost { get; set; }

	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CostExtension
	{
		/// <summary>
		/// The maximum potential cost of the query.
		/// </summary>
		[JsonProperty("requestedQueryCost")]
		public decimal? RequestedQueryCost { get; set; }

		/// <summary>
		/// The actual cost of the query.
		/// </summary>
		[JsonProperty("actualQueryCost")]
		public decimal? ActualQueryCost { get; set; }

		/// <summary>
		/// The current throttle status of the store's API.
		/// </summary>
		[JsonProperty("throttleStatus")]
		public ThrottleStatus ThrottleStatus { get; set; }

		/// <summary>
		/// The detail cost of all fields
		/// </summary>
		[JsonProperty("fields")]
		public List<CostField> Fields { get; set; }

	}

	/// <summary>
	/// A description of the current Throttle status of a Shopify store.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ThrottleStatus
	{
		/// <summary>
		/// THe maximum number of points the store allows.
		/// </summary>
		[JsonProperty("maximumAvailable")]
		public decimal? MaximumAvailable { get; set; }

		/// <summary>
		/// The number of points currently available.
		/// </summary>
		[JsonProperty("currentlyAvailable")]
		public decimal? CurrentlyAvailable { get; set; }

		/// <summary>
		/// THe rate at which points are restored per second.
		/// </summary>
		[JsonProperty("restoreRate")]
		public decimal? RestoreRate { get; set; }

	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CostField
	{
		/// <summary>
		/// THe path of cost field
		/// </summary>
		[JsonProperty("path")]
		public string[] Paths { get; set; }

		public string Path { get => string.Join(BCConstants.Arrow, Paths); }

		/// <summary>
		/// THe defined cost of the field
		/// </summary>
		[JsonProperty("definedCost")]
		public decimal? DefinedCost { get; set; }

		/// <summary>
		/// THe requested total cost by the end of the field
		/// </summary>
		[JsonProperty("requestedTotalCost")]
		public decimal? RequestedTotalCost { get; set; }

		/// <summary>
		/// THe requested child cost by the end of the field
		/// </summary>
		[JsonProperty("requestedChildrenCost")]
		public decimal? RequestedChildrenCost { get; set; }

	}
}
