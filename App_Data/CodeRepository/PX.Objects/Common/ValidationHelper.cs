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

namespace PX.Objects.Common
{
	public sealed class ValidationHelper : ValidationHelper<ValidationHelper>
	{
		public ValidationHelper(PXCache cache, object row) : base(cache, row)
		{
		}
	}

	public class ValidationHelper<T> 
		where T : ValidationHelper<T>
	{
		protected readonly object Row;
		protected readonly PXCache Cache;
		public bool IsValid { get; protected set; }

		public static bool SetErrorEmptyIfNull<TField>(PXCache cache, object row, object value)
			where TField : IBqlField
		{
			if (value == null)
			{
				cache.RaiseExceptionHandling<TField>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<TField>(cache)));
				return false;
			}

			return true;
		}

		public ValidationHelper(PXCache cache, object row)
		{
			Row = row;
			Cache = cache;
			IsValid = true;
		}

		public T SetErrorEmptyIfNull<TField>(object value)
			where TField : IBqlField
		{
			IsValid &= SetErrorEmptyIfNull<TField>(Cache, Row, value);
			return (T)this;
		}
	}
}
