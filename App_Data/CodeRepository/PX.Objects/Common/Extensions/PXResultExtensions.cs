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

using System;

using PX.Data;

namespace PX.Objects.Common.Extensions
{
	public static class PXResultExtensions
	{
		public static Tuple<T0, T1> ToTuple<T0, T1>(this PXResult<T0, T1> result)
			where T0 : class, IBqlTable, new()
			where T1 : class, IBqlTable, new()
		{
			return Tuple.Create<T0, T1>(result, result);
		}

		public static Tuple<T0, T1, T2> ToTuple<T0, T1, T2>(this PXResult<T0, T1, T2> result)
			where T0 : class, IBqlTable, new()
			where T1 : class, IBqlTable, new()
			where T2 : class, IBqlTable, new()
		{
			return Tuple.Create<T0, T1, T2>(result, result, result);
		}
	}
}
