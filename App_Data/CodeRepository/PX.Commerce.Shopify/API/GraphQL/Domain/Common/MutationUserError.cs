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
	/// An error that happens during the execution of a mutation.
	/// </summary>
	public class MutationUserError
	{
		/// <summary>
		/// The error code
		/// </summary>
		[JsonProperty("code")]
		//Shopify doesn't design the unique UserError object, in some objects it includes the Code field but some doesn't.
		//To avoid unexpected compile error, we don't add this field to the GraphQL query
		//[GraphQLField("code", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Code { get; set; }

		/// <summary>
		/// The path to the input field that caused the error.
		/// </summary>
		[JsonProperty("field")]
		[GraphQLField("field", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string[] Field { get; set; }

		/// <summary>
		/// The error message.
		/// </summary>
		[JsonProperty("message")]
		[GraphQLField("message", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Message { get; set; }
	}
}
