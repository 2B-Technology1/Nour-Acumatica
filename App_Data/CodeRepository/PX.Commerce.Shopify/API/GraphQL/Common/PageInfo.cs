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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Returns information about pagination in a connection, in accordance with the Relay specification.
	/// </summary>
	public class PageInfo 
	{
		/// <summary>
		/// The cursor corresponding to the last node in edges.
		/// </summary>
		[JsonProperty(GraphQLConstants.PageInfo.EndCursor)]
		public virtual string EndCursor { get; set; }

		/// <summary>
		/// Whether there are more pages to fetch following the current page.
		/// </summary>
		[JsonProperty(GraphQLConstants.PageInfo.HasNextPage)]
		public virtual bool? HasNextPage { get; set; }

		/// <summary>
		/// Whether there are any pages prior to the current page.
		/// </summary>
		[JsonProperty(GraphQLConstants.PageInfo.HasPreviousPage)]
		public virtual bool? HasPreviousPage { get; set; }

		/// <summary>
		/// The cursor corresponding to the first node in edges.
		/// </summary>
		[JsonProperty(GraphQLConstants.PageInfo.StartCursor)]
		public virtual string StartCursor { get; set; }
	}
}
