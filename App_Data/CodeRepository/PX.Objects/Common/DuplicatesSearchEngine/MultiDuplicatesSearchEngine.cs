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

using PX.Common;
using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common
{
	[PXInternalUseOnly]
	public class MultiDuplicatesSearchEngine<TEntity> : DuplicatesSearchEngineBase<TEntity>
		where TEntity : class, IBqlTable, new()
	{
		protected readonly Dictionary<TEntity, List<TEntity>> _items;

		public MultiDuplicatesSearchEngine(PXCache cache, IEnumerable<Type> keyFields)
			: base(cache, keyFields)
		{
			_items = new Dictionary<TEntity, List<TEntity>>(_comparator);
		}

		public MultiDuplicatesSearchEngine(PXCache cache, IEnumerable<Type> keyFields, ICollection<TEntity> items)
			: this(cache, keyFields)
		{
			foreach (var item in items)
				AddItem(item);
		}

		public override void AddItem(TEntity item)
		{
			if (_items.TryGetValue(item, out List<TEntity> list))
			{
				list.Add(item);
			}
			else
			{
				_items.Add(item, new List<TEntity>() { item });
			}
		}

		public override TEntity Find(TEntity item)
		{
			return this[item].FirstOrDefault();
		}

		public virtual bool RemoveItem(TEntity item)
		{
			if (_items.TryGetValue(item, out List<TEntity> list))
			{
				if (list.Remove(item))
				{
					if (list.Count == 0)
						_items.Remove(item);

					return true;
				}
			}

			return false;
		}

		public IEnumerable<TEntity> this[TEntity item]
		{
            get 
			{
				List<TEntity> found;
				if (_items.TryGetValue(item, out found))
					return found;
				return Array<TEntity>.Empty;
			}
		}

		public virtual IEnumerable<TEntity> this[IDictionary itemValues]
		{
			get => this[CreateEntity(itemValues)];
		}
	}
}
