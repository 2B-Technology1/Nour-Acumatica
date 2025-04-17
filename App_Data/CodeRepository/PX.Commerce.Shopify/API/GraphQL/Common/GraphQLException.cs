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

using GraphQL;
using PX.Data;
using System.Runtime.Serialization;
using PX.Common;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Represents errors that occur during execution of a GraphQL request.
	/// </summary>
	public class GraphQLException : PXException
	{
		/// <summary>
		/// The <see cref="GraphQLError"/> that describes this exception.
		/// </summary>
		public GraphQLError GraphQLError { get; }

		/// <summary>
		/// Constructs a <see cref="GraphQLException"/> from a <see cref="GraphQLError"/>.
		/// </summary>
		/// <param name="error">The <see cref="GraphQLError"/> that caused this exception.</param>
		public GraphQLException(GraphQLError error) : base(GraphQLMessages.ExceptionMessage, error.Message)
		{
			GraphQLError = error;
		}

		protected GraphQLException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			ReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			ReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}
	}
}
