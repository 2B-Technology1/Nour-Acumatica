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
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common
{
	[PXInternalUseOnly]
	public class DuplicatesSearchEngine<TEntity> : DuplicatesSearchEngineBase<TEntity>
		where TEntity : class, IBqlTable, new()
	{
		protected readonly Dictionary<TEntity, TEntity> _items;

		public DuplicatesSearchEngine(PXCache cache, IEnumerable<Type> keyFields, ICollection<TEntity> items)
			: base(cache, keyFields)
		{
			_items = new Dictionary<TEntity, TEntity>(_comparator);

			foreach (var item in items)
				AddItem(item);
		}

		public override TEntity Find(TEntity item)
		{
			TEntity found;
			return _items.TryGetValue(item, out found)
				? found
				: null;
		}

		public override void AddItem(TEntity item)
		{
			if (!_items.ContainsKey(item))
				_items.Add(item, item);
		}
	}
}
