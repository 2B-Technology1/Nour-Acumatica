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
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.SQLTree;

namespace PX.Objects.FA
{
	public class FixedAssetCanBeDepreciated<TFieldAssetID> : IBqlWhere
			where TFieldAssetID : PX.Data.BQL.BqlInt.Field<TFieldAssetID>
	{
		private readonly IBqlCreator whereCanBeDepreciated;

		private readonly Type assetIDCacheType;

		public FixedAssetCanBeDepreciated()
		{
			assetIDCacheType = BqlCommand.GetItemType(typeof(TFieldAssetID));
			whereCanBeDepreciated = new Where<FixedAsset.depreciable, Equal<True>, And<FixedAsset.underConstruction, NotEqual<True>>>();
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			return whereCanBeDepreciated.AppendExpression(ref exp, graph, info, selection);
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			Type itemType = item.GetType();
			if (!assetIDCacheType.IsAssignableFrom(itemType))
			{
				throw new PXException(Messages.CanBeDepreciatedUsedWrongField, itemType.FullName, assetIDCacheType.FullName);
			}

			if (typeof(FixedAsset).IsAssignableFrom(itemType) || cache.GetValue<TFieldAssetID>(item) == null)
			{
				whereCanBeDepreciated.Verify(cache, item, pars, ref result, ref value);
			}
			else
			{
				PXCache fixedAssetCache = cache.Graph.Caches[typeof(FixedAsset)];

				FixedAsset fixedasset = SelectFrom<FixedAsset>.Where<FixedAsset.assetID.IsEqual<@P.AsInt>>.View.Select(cache.Graph, (int)cache.GetValue<TFieldAssetID>(item));
				whereCanBeDepreciated.Verify(fixedAssetCache, fixedasset, pars, ref result, ref value);
			}

		}
	}
}
