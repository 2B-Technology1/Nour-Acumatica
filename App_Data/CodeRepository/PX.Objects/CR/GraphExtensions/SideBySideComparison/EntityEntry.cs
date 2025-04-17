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

using JetBrains.Annotations;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR.Extensions.SideBySideComparison
{
	/// <summary>
	/// Represents a set of <see cref="PXCache"/> and at least one <see cref="IBqlTable"/> that belongs to this cache.
	/// </summary>
	public sealed class EntityEntry
	{
		public EntityEntry([NotNull] Type entityType, [NotNull] PXCache cache, [NotNull, ItemNotNull] IEnumerable<IBqlTable> item)
		{
			Cache = cache ?? throw new ArgumentNullException(nameof(cache));
			Items = item ?? throw new ArgumentNullException(nameof(item));
			EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
		}

		public EntityEntry([NotNull] Type entityType, [NotNull] PXCache cache, [NotNull, ItemNotNull] params IBqlTable[] item)
			: this(entityType, cache, item?.AsEnumerable())
		{
		}

		public EntityEntry([NotNull] PXCache cache, [NotNull, ItemNotNull] IEnumerable<IBqlTable> items)
			: this(cache?.GetItemType(), cache, items)
		{
		}

		public EntityEntry([NotNull] PXCache cache, [NotNull, ItemNotNull] params IBqlTable[] items)
			: this(cache, items?.AsEnumerable())
		{
		}


		/// <summary>
		/// The type of <see cref="Items"/>.
		/// </summary>
		/// <remarks>
		/// The value can differ from the value retuned by <see cref="PXCache.GetItemType"/> if the item is treated as a parent.
		/// For instance, you are working with <see cref="CRLead"/> with the cache of <see cref="Contact"/>.
		/// </remarks>
		public Type EntityType { get; }
		/// <summary>
		/// The cache of <see cref="Items"/>.
		/// </summary>
		public PXCache Cache { get; }
		/// <summary>
		/// The list of <see cref="IBqlTable"/>s.
		/// </summary>
		public IEnumerable<IBqlTable> Items { get; }

		/// <summary>
		/// Returns an entity of the <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The item type.</typeparam>
		/// <returns>The DAC.</returns>
		/// <exception cref="InvalidCastException">
		/// The <see cref="Items"/> list contains more than one element 
		/// or the item cannot be converted to <typeparamref name="T"/>.
		/// </exception>
		public T Single<T>() where T : IBqlTable => Items.Cast<T>().Single();

		/// <summary>
		/// Returns the list of entities of the <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The item type.</typeparam>
		/// <returns>The DAC.</returns>
		/// <exception cref="InvalidCastException">
		/// At least one item in <see cref="Items"/> cannot be converted to <typeparamref name="T"/>.
		/// </exception>
		/// <returns>The list of DACs.</returns>
		public IEnumerable<T> Multiple<T>() where T : IBqlTable => Items.Cast<T>();
	}
}
