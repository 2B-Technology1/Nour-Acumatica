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
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public abstract class EntityNodeResponse<T> : INodeResponse<T>
		where T: BCAPIEntity
	{
		[JsonProperty(GraphQLConstants.Edge.Node)]
		public T Node { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public abstract class NodesEntitiesResponse<T> : INodeListResponse<T>
		where T : BCAPIEntity
	{
		/// <summary>
		/// The response data part
		/// </summary>
		[JsonProperty(GraphQLConstants.Connection.Nodes)]
		public IEnumerable<T> Nodes { get; set; }

		/// <summary>
		/// The response PageInfo part
		/// </summary>
		[JsonProperty(GraphQLConstants.Connection.PageInfo)]
		public PageInfo PageInfo { get; set; }

	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public abstract class EdgesEntitiesResponse<T, TNode> : IEdgesResponse<T, TNode>
		where T : BCAPIEntity
		where TNode : class, INodeResponse<T>
	{
		/// <summary>
		/// The response data part
		/// </summary>
		[JsonProperty(GraphQLConstants.Connection.Edges)]
		public IEnumerable<TNode> Edges { get; set; }

		/// <summary>
		/// The response PageInfo part
		/// </summary>
		[JsonProperty(GraphQLConstants.Connection.PageInfo)]
		public PageInfo PageInfo { get; set; }

	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public abstract class EntitiesResponse<T, TNode> : INodeListResponse<T>, IEdgesResponse<T, TNode>
		where T : BCAPIEntity
		where TNode : class, INodeResponse<T>
	{
		/// <summary>
		/// The response data part
		/// </summary>
		[JsonProperty(GraphQLConstants.Connection.Nodes)]
		public IEnumerable<T> Nodes { get; set; }
		/// <summary>
		/// The response data part
		/// </summary>
		[JsonProperty(GraphQLConstants.Connection.Edges)]
		public IEnumerable<TNode> Edges { get; set; }

		/// <summary>
		/// The response PageInfo part
		/// </summary>
		[JsonProperty(GraphQLConstants.Connection.PageInfo)]
		public PageInfo PageInfo { get; set; }

	}
}
