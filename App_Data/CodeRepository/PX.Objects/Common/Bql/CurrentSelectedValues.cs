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
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using static PX.Data.BqlCommand;

namespace PX.Objects.Common.Bql
{
	public class CurrentSelectedValues<TField> : IBqlConstantsOf<IImplement<IBqlEquitable>>, IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlString>>>
		where TField : IBqlField
	{
		private const string MultiSelectSeparator = ",";

		public IEnumerable<object> GetValues(PXGraph graph)
		{
			if (graph == null)
				return Array<string>.Empty;

			PXCache cache = graph.Caches[GetItemType<TField>()];
			var selectedValue = (string)cache.GetValue<TField>(cache.Current);
			if (string.IsNullOrEmpty(selectedValue))
				return Array<string>.Empty;

			var values = selectedValue.Split(new[] { MultiSelectSeparator }, StringSplitOptions.RemoveEmptyEntries);

			return values;
		}
	}

	public class AllowedValues<TField> : IBqlConstantsOf<IImplement<IBqlEquitable>>, IBqlConstantsOf<IImplement<IBqlCastableTo<IBqlString>>>
		where TField : IBqlField
	{
		public IEnumerable<object> GetValues(PXGraph graph)
		{
			if (graph == null)
				return Array<string>.Empty;

			PXCache cache = graph.Caches[GetItemType<TField>()];
			return GetAllowedValues(cache, cache.Current);
		}

		protected string[] GetAllowedValues(PXCache cache, object row)
		{
			var stringListAttr = cache.GetAttributesReadonly<TField>(row)
				.OfType<PXStringListAttribute>()
				.FirstOrDefault();
			if (stringListAttr == null)
				return Array<string>.Empty;
			return stringListAttr.GetAllowedValues(cache);
		}
	}
}
