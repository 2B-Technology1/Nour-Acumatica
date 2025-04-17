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
using System;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The Attribute that using for the GraphQL data object
	/// </summary>
	public class GraphQLObjectAttribute : System.Attribute
	{
		public virtual string NodeName { get; set; }

		public virtual string ConnectionName { get; set; }

		public virtual string MutationName { get; set; }

		public GraphQLObjectAttribute() : base() { }

		public GraphQLObjectAttribute(string nodeName, string connectionName, string mutationName)
		{
			NodeName = nodeName;
			ConnectionName = connectionName;
			MutationName = mutationName;
		}
	}
}
