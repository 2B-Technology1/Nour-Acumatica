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
using System.Collections;
using System.Linq;

namespace PX.Objects.CR.Extensions
{
	public delegate IEnumerable ExecuteSelectDelegate(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows);
	public delegate int ExecuteUpdateDelegate(string viewName, IDictionary keys, IDictionary values, object[] parameters);

	public static class GraphExtensionHelpers
	{
		public static Extension GetProcessingExtension<Extension>(this PXGraph processingGraph)
			where Extension : PXGraphExtension
		{
			var processingExtesion = processingGraph.FindImplementation<Extension>();
			if (processingExtesion == null)
				throw new PXException(Messages.ExtensionCannotBeFound, typeof(Extension).ToString(), processingGraph.GetType().ToString());

			return processingExtesion;
		}
	}

	[PXInternalUseOnly]
	public static class GraphHelpers
	{
		public static TGraph CloneGraphState<TGraph>(this TGraph graph)
			where TGraph : PXGraph, new()
		{
			var clone = graph.Clone();

			clone.IsContractBasedAPI = graph.IsContractBasedAPI;
			clone.IsCopyPasteContext = graph.IsCopyPasteContext;
			//clone.IsArchiveContext = graph.IsArchiveContext;
			//clone.IsImportFromExcel = graph.IsImportFromExcel;
			clone.IsExport = graph.IsExport;
			clone.IsImport = graph.IsImport;
			clone.IsMobile = graph.IsMobile;

			if ((clone.IsImport || graph.IsImportFromExcel) && !PXContext.Session.IsSessionEnabled)
			{
				// manually copy all cache currents for proper import
				foreach (var (key, cache) in graph.Caches.ToList())
				{
					var cloneCache = clone.Caches[key];
					cloneCache.Current = cache.Current;

					foreach (var item in cache.Cached)
					{
						cloneCache.SetStatus(item, cache.GetStatus(item));
					}
				}
			}

			return clone;
		}
	}
}
