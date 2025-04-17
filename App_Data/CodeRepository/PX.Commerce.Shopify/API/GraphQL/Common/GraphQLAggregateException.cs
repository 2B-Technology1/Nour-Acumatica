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


using PX.Data;
using System.Runtime.Serialization;
using PX.Common;
using System.Text;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Represents one or more GraphQL errors that occurred during execution of a GraphQL request.
	/// </summary>
	public class GraphQLAggregateException : PXException
	{
		/// <summary>
		/// The GraphQLExceptions describe the list of errors.
		/// </summary>
		public GraphQLException[] Exceptions { get; }

		/// <summary>
		/// Constructs a <see cref="GraphQLAggregateException"/> from an array of <see cref="GraphQLException"/>s.
		/// </summary>
		/// <param name="exceptions">The array of inner <see cref="GraphQLException"/>.</param>
		public GraphQLAggregateException(GraphQLException[] exceptions)
			: base(GraphQLMessages.AggregateExceptionMessage, GetErrorMessages(exceptions))
		{
			Exceptions = exceptions;
		}

		public static string GetErrorMessages(GraphQLException[] exceptions)
		{
			StringBuilder errorMessages = new StringBuilder();
			foreach (var exception in exceptions)
			{
				if (!string.IsNullOrEmpty(exception.Message))
				{
					errorMessages.Append(exception.Message);
				}
				else if (!string.IsNullOrEmpty(exception.InnerException?.Message))
				{
					errorMessages.Append(exception.InnerException?.Message);
				}
				else
					continue;
				errorMessages.Append(";");
			}
			return errorMessages.ToString();
		}

		protected GraphQLAggregateException(SerializationInfo info, StreamingContext context) : base(info, context)
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
