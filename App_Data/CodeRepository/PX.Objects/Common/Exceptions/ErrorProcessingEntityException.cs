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
using PX.Objects.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Common.Exceptions
{
	/// <exclude/>
	public class ErrorProcessingEntityException : PXException
	{
		public IBqlTable Entity { get; protected set; }

		protected ErrorProcessingEntityException(PXCache cache, IBqlTable entity, string localizedError, Exception innerException)
			: base(innerException, Messages.ProcessingEntityError, cache.GetRowDescription(entity), localizedError)
		{
			Entity = entity;
		}

		public ErrorProcessingEntityException(PXCache cache, IBqlTable entity, PXException innerException)
			: this(cache, entity, innerException.MessageNoPrefix, innerException)
		{
		}

		public ErrorProcessingEntityException(PXCache cache, IBqlTable entity, PXOuterException innerException)
			: this(cache, entity, GetFullMessage(innerException), innerException)
		{
		}

		public ErrorProcessingEntityException(PXCache cache, IBqlTable entity, string errorMessage)
			: this(cache, entity, PXMessages.LocalizeNoPrefix(errorMessage), null)
		{
		}

		public static ErrorProcessingEntityException Create(PXCache cache, IBqlTable entity, Exception exception)
		{
			if (exception is PXOuterException pXOuterException)
			{
				return new ErrorProcessingEntityException(cache, entity, pXOuterException);
			}
			else if (exception is PXException pxException)
			{
				return new ErrorProcessingEntityException(cache, entity, pxException);
			}
			return new ErrorProcessingEntityException(cache, entity, exception.Message);
		}

		public ErrorProcessingEntityException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		protected static string GetFullMessage(PXOuterException outerException)
		{
			string separator = Environment.NewLine;
			return outerException.MessageNoPrefix + separator + String.Join(separator, outerException.InnerMessages);
		}
	}
}
