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

namespace PX.Objects.Common.Extensions
{
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Given a dictionary, gets the value by key. If the key is not 
		/// present, adds it to the dictionary along with the value generated 
		/// by the initializer function, and returns that value.
		/// </summary>
		public static TValue GetOrAdd<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary, 
			TKey key, 
			Func<TValue> initializer)
		{
			if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (initializer == null) throw new ArgumentNullException(nameof(initializer));

			TValue result;

			if (!dictionary.TryGetValue(key, out result))
			{
				result = initializer();
				dictionary[key] = result;
			}

			return result;
		}

		public static ICollection<TValue> GetValueOrEmpty<TKey, TValue>(
			this IDictionary<TKey, ICollection<TValue>> dictionary,
			TKey key)
		{
			ICollection<TValue> result;

			dictionary.TryGetValue(key, out result);

			return result ?? new TValue[0];
		}
	}
}
