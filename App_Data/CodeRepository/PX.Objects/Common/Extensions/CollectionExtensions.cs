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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Objects.Common.StringProcessing;

namespace PX.Objects.Common.Extensions
{
	/// <summary>
	/// This class contains extension methods for collections.
	/// </summary>
	public static class CollectionExtensions
	{
		/// <exclude/>
		public static string JoinIntoStringForMessageNoQuotes<T>(this ICollection<T> items, int maxCount = 0, string separator = Separators.Comma)
		{
			return JoinIntoStringForMessage(items, maxCount, separator, string.Empty);
		}

		/// <exclude/>
		public static string JoinIntoStringForMessage<T>(this ICollection<T> items, int maxCount = 0, string separator = Separators.Comma, string edgingSymbol = Edging.Quote)
		{
			IEnumerable<T> itemsToJoin = null;
			var postfix = string.Empty;

			if (items.Count > maxCount && maxCount != 0)
			{
				itemsToJoin = items.Take(maxCount);
				postfix = "...";
			}
			else
			{
				itemsToJoin = items;
			}

			var edgedItems = itemsToJoin.Select(item => string.Concat(edgingSymbol, item, edgingSymbol));

			return string.Concat(string.Join(separator, edgedItems), postfix);
		}

		/// <summary>
		/// Performs an index-dependent action upon each element of the sequence.
		/// The action takes the index of an element as an input.
		/// </summary>
		public static void ForEach<T>(this IEnumerable<T> sequence, Action<T, int> action)
		{
			int index = 0;

			foreach (T item in sequence)
			{
				action(item, index);
				++index;
			}
		}

		/// <summary>
		/// The method returns a new sequence in which the specified number of elements
		/// is dropped from the tail of the original sequence.
		/// The method performs this efficiently, without enumerating the sequence several times.
		/// </summary>
		/// <param name="elementsToSkip">The number of elements to drop from the tail.</param>
		public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int elementsToSkip)
		{
			if (elementsToSkip < 0) throw new ArgumentOutOfRangeException(nameof(elementsToSkip));

			IEnumerator<T> enumerator = source.GetEnumerator();

			bool hasRemainingItems;
			Queue<T> elementCache = new Queue<T>(elementsToSkip + 1);

			do
			{
				hasRemainingItems = enumerator.MoveNext();

				if (hasRemainingItems)
				{
					elementCache.Enqueue(enumerator.Current);

					if (elementCache.Count > elementsToSkip)
					{
						yield return elementCache.Dequeue();
					}
				}
			}
			while (hasRemainingItems);
		}

		/// <summary>
		/// Returns a collection consisting of elements of the <paramref name="source"/> collection
		/// that has even indices (starting from 0).
		/// </summary>
		public static IEnumerable<T> EvenElements<T>(this IEnumerable<T> source)
		{
			int index = 0;

			foreach (T element in source)
			{
				if (index % 2 == 0)
				{
					yield return element;
				}

				++index;
			}
		}

		/// <summary>
		/// Returns a collection consisting of elements of the <paramref name="source"/> collection
		/// that has odd indices (starting from 0).
		/// </summary>
		public static IEnumerable<T> OddElements<T>(this IEnumerable<T> source)
		{
			int index = 0;

			foreach (T element in source)
			{
				if (index % 2 != 0)
				{
					yield return element;
				}

				++index;
			}
		}

		/// <exclude/>
		public static TItem GetItemWithMax<TItem, TValue>(this IReadOnlyCollection<TItem> items, Func<TItem, TValue> getValue)
			where TItem : class
			where TValue : IComparable<TValue>
		{
			return GetPreferredItem(items, getValue, (curValue, maxValue) => curValue.CompareTo(maxValue) > 0);
		}

		/// <exclude/>
		public static TItem GetItemWithMin<TItem, TValue>(this IReadOnlyCollection<TItem> items, Func<TItem, TValue> getValue)
			where TItem : class
			where TValue : IComparable<TValue>
		{
			return GetPreferredItem(items, getValue, (curValue, minValue) => curValue.CompareTo(minValue) < 0);
		}

		/// <exclude/>
		public static TItem GetPreferredItem<TItem, TValue>(this IReadOnlyCollection<TItem> items, Func<TItem, TValue> getValue, Func<TValue, TValue, bool> compareFunc)
			where TItem : class
			where TValue : IComparable<TValue>
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));

			if (items == null)
				throw new ArgumentNullException(nameof(getValue));

			if (!items.Any())
				return null;

			var preferredItem = items.First();
			var preferredValue = getValue(preferredItem);

			foreach (var item in items)
			{
				var curValue = getValue(item);

				if (compareFunc(curValue, preferredValue))
				{
					preferredValue = curValue;
					preferredItem = item;
				}
			}

			return preferredItem;
		}

		/// <exclude/>
		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			if (items == null)
				throw new ArgumentNullException("items");

			foreach (T item in items)
			{
				collection.Add(item);
			}
		}

		/// <exclude/>
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}

		/// <exclude/>
		public static Dictionary<TSource, HashSet<TSource>> ReverseDictionary<TSource>(this Dictionary<TSource, TSource> source, bool keppAllKeys = false)
		{
			Dictionary<TSource, HashSet<TSource>> result = new Dictionary<TSource, HashSet<TSource>>();
			if (keppAllKeys)
			{
				foreach (TSource key in source.Keys)
				{
					result.Add(key, new HashSet<TSource> { });
				}
			}
			foreach (KeyValuePair<TSource, TSource> item in source)
			{
				if (item.Value != null)
				{
					if (result.ContainsKey(item.Value))
					{
						result[item.Value].Add(item.Key);
					}
					else
					{
						result[item.Value] = new HashSet<TSource> { item.Key };
					}
				}
			}
			return result;
		}
		public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
		{
			private readonly TKey key;
			private readonly IEnumerable<TElement> values;

			public Grouping(TKey key, IEnumerable<TElement> values)
			{
				if (values == null)
					throw new ArgumentNullException(nameof(values));
				this.key = key;
				this.values = values;
			}

			public TKey Key
			{
				get { return key; }
			}

			public IEnumerator<TElement> GetEnumerator()
			{
				return values.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		/// <exclude/>
		public static IEnumerable<IGrouping<int, TElement>> BreakInto<TElement>(this IEnumerable<TElement> source, int batchSize)
		{
			int sourceCount = source.Count();
			int bathesCount = (sourceCount / batchSize) + (sourceCount % batchSize > 0 ? 1 : 0);
			List<IGrouping<int, TElement>> result = new List<IGrouping<int, TElement>>(bathesCount);
			IEnumerable<TElement> notProcessedItems = source;
			for (int i = 0; i < bathesCount; i++)
			{
				result.Add(new Grouping<int, TElement>(0, notProcessedItems.Take(batchSize)));
				notProcessedItems = notProcessedItems.Skip(batchSize);
			}
			return result;
		}
	}
}
