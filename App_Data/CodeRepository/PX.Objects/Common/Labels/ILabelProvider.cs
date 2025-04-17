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

using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common
{
	public interface ILabelProvider
	{
		IEnumerable<ValueLabelPair> ValueLabelPairs { get; }
	}

	public static class LabelProvider
	{
		public static string GetLabel(this ILabelProvider provider, string value)
		{
			return provider
				.ValueLabelPairs
				.Single(pair => pair.Value == value)
				.Label;
		}

		public static IEnumerable<string> Values(this ILabelProvider provider)
		{
			return provider.ValueLabelPairs.Select(pair => pair.Value);
		}

		public static IEnumerable<string> Labels(this ILabelProvider provider)
		{
			return provider.ValueLabelPairs.Select(pair => pair.Label);
		}
	}
}
