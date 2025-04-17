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

using PX.Data;

namespace PX.Objects.Common
{
	/// <summary>
	/// Compares two data records based on the equality of a subset of their fields.
	/// The set of fields to compare the records on is explicitly specified in the
	/// comparer's constructor.
	/// </summary>
	public class FieldSubsetEqualityComparer<TRecord> : IEqualityComparer<TRecord>
		where TRecord : class, IBqlTable, new()
	{
		private PXCache _cache;
		private IEnumerable<string> _fieldNames;

		public FieldSubsetEqualityComparer(PXCache cache, IEnumerable<string> fieldNames)
		{
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (fieldNames == null) throw new ArgumentNullException(nameof(fieldNames));

			_cache = cache;
			_fieldNames = fieldNames;
		}

		public FieldSubsetEqualityComparer(PXCache cache, params string[] fieldNames)
			: this(cache, fieldNames as IEnumerable<string>)
		{ }

		public FieldSubsetEqualityComparer(PXCache cache, IEnumerable<Type> fields)
			: this(cache, fields.Select(field => field.Name))
		{ }

		public FieldSubsetEqualityComparer(PXCache cache, params Type[] fields)
			: this(cache, fields as IEnumerable<Type>)
		{ }

		private IEnumerable<object> CollectFieldValues(TRecord record) 
			=> _fieldNames.Select(fieldName => _cache.GetValue(record, fieldName));

		public bool Equals(TRecord first, TRecord second)
		{
			if (first == null) throw new ArgumentNullException(nameof(first));
			if (second == null) throw new ArgumentNullException(nameof(second));

			IEnumerable<Tuple<object, object>> fieldValuePairs = 
				CollectFieldValues(first).Zip(
					CollectFieldValues(second), (x, y) => Tuple.Create(x, y));

			foreach (Tuple<object, object> fieldValuePair in fieldValuePairs)
			{
				if (!Equals(fieldValuePair.Item1, fieldValuePair.Item2))
				{
					return false;
				}
			}

			return true;
		}

		public int GetHashCode(TRecord record)
		{
			if (record == null) throw new ArgumentNullException(nameof(record));

			return HashcodeCombiner.Combine(CollectFieldValues(record));
		}
	}
}