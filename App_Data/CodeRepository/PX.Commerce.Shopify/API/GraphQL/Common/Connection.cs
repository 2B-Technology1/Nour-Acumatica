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
	/// Represents a page of objects returned by a GraphQL API.
	/// </summary>
	/// <typeparam name="TNode"></typeparam>
	public class Connection<TNode> //: IConnection<TNode>
		where TNode : BCAPIEntity, new()
	{
		/// <summary>
		/// The list of nodes returned in the current page.
		/// </summary>
		[JsonProperty(GraphQLConstants.Connection.Nodes)]
		public IEnumerable<TNode> Nodes { get; set; }

		/// <summary>
		/// Object containing meta information about the current page.
		/// </summary>
		[JsonProperty(GraphQLConstants.Connection.PageInfo)]
		public PageInfo PageInfo { get; set; }
	}
}
