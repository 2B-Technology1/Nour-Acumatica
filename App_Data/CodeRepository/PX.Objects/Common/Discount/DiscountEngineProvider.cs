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

namespace PX.Objects.Common.Discount
{
	/// <summary>
	/// Provides instances of discount engines for a specific discounted line type
	/// </summary>
	public class DiscountEngineProvider
	{
		/// <summary>
		/// Get instance of discount engines for a specific discounted line type
		/// </summary>
		/// <typeparam name="TLine">The type of discounted line</typeparam>
		/// <returns></returns>
		public static DiscountEngine<TLine, TDiscountDetail> GetEngineFor<TLine, TDiscountDetail>()
			where TLine : class, IBqlTable, new()
			where TDiscountDetail : class, IBqlTable, IDiscountDetail, new()
			=> EnginesCache<TLine, TDiscountDetail>.Engine;

		/// <summary>
		/// Caches discount engines for a specific discounted line type
		/// </summary>
		/// <typeparam name="TLine">The type of discounted line</typeparam>
		private static class EnginesCache<TLine, TDiscountDetail>
			where TLine : class, IBqlTable, new()
			where TDiscountDetail : class, IBqlTable, IDiscountDetail, new()
		{
			private static readonly Lazy<DiscountEngine<TLine, TDiscountDetail>> _engine = new Lazy<DiscountEngine<TLine, TDiscountDetail>>(PXGraph.CreateInstance<DiscountEngine<TLine, TDiscountDetail>>);

			public static DiscountEngine<TLine, TDiscountDetail> Engine => _engine.Value;
		}
	}
}