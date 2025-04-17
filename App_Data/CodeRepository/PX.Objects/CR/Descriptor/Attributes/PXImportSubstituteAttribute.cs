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
using PX.Common;
using PX.Data;

namespace PX.Objects.CR
{
	/// <exclude/>
	[PXInternalUseOnly]
	public class PXImportSubstituteAttribute : PXImportAttribute
	{
		#region Ctors

		private readonly Type _itemsCacheType = null;

		/// <exclude/>
		public PXImportSubstituteAttribute(Type substituteCacheType)
			: this(null, substituteCacheType)
		{
		}

		/// <exclude/>
		public PXImportSubstituteAttribute(Type primaryTable, Type substituteCacheType)
			: base(primaryTable)
		{
			_table = primaryTable;
			_itemsCacheType = substituteCacheType;
		}

		/// <exclude/>
		public PXImportSubstituteAttribute(Type primaryTable, Type substituteCacheType, IPXImportWizard importer)
			: this(primaryTable, substituteCacheType)
		{
			_importer = importer;
		}

		/// <exclude/>
		public override void ViewCreated(PXGraph graph, string viewName)
		{
			_itemsCache = graph.Caches[_itemsCacheType];

			base.ViewCreated(graph, viewName);

			if (_importer != null)
			{
				var importerType = BqlCommand.Compose(typeof(PXImporter<>), _itemsCache.GetItemType());
				importerType.GetField("_suppressLongRun", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_importer, true);
			}
		}

		#endregion
	}
}
