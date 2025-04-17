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
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR.Extensions
{
	[PXInternalUseOnly]
	public static class ViewExtensions
	{
		/// <summary>
		/// The method that can be used to create a view from the specified select type or
		/// get an existing view with the specified name and create an instance of this select type in both cases as a result.
		/// </summary>
		/// <remarks>
		/// <typeparamref name="TSelect"/> must be non abstract or to be <see cref="PXSelectBase{Table}"/> or <see cref="PXSelectBase"/>.
		/// If the <see cref="PXSelectBase{Table}"/> provided as <typeparamref name="TSelect"/> than
		/// <see cref="PXSelect{Table}"/> would be returned. If it is non generic <see cref="PXSelectBase"/>
		/// than resulting <see cref="PXSelect{Table}"/> would be for the primary type of existing view,
		/// if view doesn't exist either, that it would be for the one lightweight DAC.
		/// In both cases if view doesn't exist it is considered as dummy and will return empty array (via view delegate).
		/// This method could be helpful if graph extension adds some hidden view that could (but not must) be overriden
		/// in multiple child extensions and all such views should be independent of each other.
		/// Such views could overriden by the common override views way:
		/// just add field of type <see cref="PXSelect{Table}"/> with name <paramref name="viewName"/> to the any graph extension for specified graph.
		/// </remarks>
		/// <typeparam name="TSelect">The desired select type, which
		/// must be non-abstract or be <see cref="PXSelectBase{Table}"/> or <see cref="PXSelectBase"/>.
		/// Also it must have a constructor with one parameter of the <see cref="PXGraph"/> type.</typeparam>
		/// <param name="graph">The graph.</param>
		/// <param name="viewName">The view name on which the desired <typeparamref name="TSelect"/> is based.
		/// If this view is not presented in <see cref="PXGraph.Views"/>, the new view with this name is created.</param>
		/// <param name="initializer">The initializer of the desired select.
		/// You can provide <see cref="PXSelectBase{Table}"/> as <typeparamref name="TSelect"/>
		/// and create a concrete select via initializer.</param>
		/// <returns>The desired select.</returns>
		/// <exception cref="PXException">Is raised if <typeparamref name="TSelect"/> is abstract and is not <see cref="PXSelectBase{Table}"/> or <see cref="PXSelectBase"/>
		/// or if the desired <typeparamref name="TSelect"/> cannot be instantiated for any reason.</exception>
		public static TSelect GetOrCreateSelectFromView<TSelect>(
			this PXGraph graph, string viewName, Func<PXSelectBase> initializer = null)
			where TSelect : PXSelectBase
		{
			try
			{
				if (graph.Views.TryGetValue(viewName, out var view))
				{
					return DefaultInitializer(view);
				}
				initializer ??= () => DefaultInitializer();
				var newSelect = initializer();
				graph.Views.Add(viewName, newSelect.View);
				if (newSelect is TSelect rightSelect)
					return rightSelect;

				return DefaultInitializer(newSelect.View);
			}
			catch (Exception ex)
			{
				throw new PXException(ex, MessagesNoPrefix.CannotInitializeSelectForView, typeof(TSelect).Name, viewName);
			}

			TSelect DefaultInitializer(PXView view = null)
			{
				var selectType = typeof(TSelect);
				bool suppressViewSelect = false;
				if (selectType.IsAbstract)
				{
					// allow initialize for base abstract PXSelectBase
					Type itemType;
					if (selectType.IsGenericType && selectType.GetGenericTypeDefinition() == typeof(PXSelectBase<>))
					{
						itemType = selectType.GenericTypeArguments[0];
					}
					else if (selectType == typeof(PXSelectBase))
					{
						// if we don't have view it is dummy select, so try to make it executable and fast
						// use CRSetup, as it won't be too expensive to fetch it from db "just in case..."
						itemType = view?.GetItemType() ?? typeof(CRSetup);
					}
					else
					{
						throw new PXArgumentException(nameof(TSelect),
							PXMessages.LocalizeFormatNoPrefixNLA(
								MessagesNoPrefix.CannotInitializeSelectForView_AbstractSelect,
								typeof(TSelect),
								viewName));
					}

					selectType = typeof(PXSelect<>).MakeGenericType(itemType);
					suppressViewSelect = view is null;
				}

				var select = (TSelect)Activator.CreateInstance(selectType, graph);
				if (suppressViewSelect)
				{
					view = new PXView(graph, true,
						select.View.BqlSelect, new PXSelectDelegate(Array.Empty<object>));
				}

				if (view != null)
					select.View = view;

				return select;
			}
		}

		public static List<object> SelectWithViewContext(this PXView view,
			params object[] parameters)
		{
			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = view.Select(
				PXView.Currents,
				parameters ?? PXView.Parameters,
				PXView.Searches,
				PXView.SortColumns,
				PXView.Descendings,
				PXView.Filters,
				ref startRow,
				PXView.MaximumRows,
				ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		public static IEnumerable<PXResult<T>> SelectChunked<T>(
			this PXSelectBase<T> select,
			int chunkSize = 100, params object[] parameters)
			where T : class, IBqlTable, new()
		{
			if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));

			return SelectChunked(select, null, parameters, null, null, null, null, chunkSize);
		}


		public static IEnumerable<PXResult<T>> SelectChunked<T>(
			this PXSelectBase<T> select,
			object[] currents = null,
			object[] parameters = null,
			object[] searches = null,
			string[] sortcolumns = null,
			bool[] descendings = null,
			PXFilterRow[] filters = null,
			int chunkSize = 100)
			where T : class, IBqlTable, new()
		{
			if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));
			return Select().SelectMany(i => i);

			IEnumerable<IEnumerable<PXResult<T>>> Select()
			{
				int count;
				int startRow = 0;
				int limit = 0;
				do
				{
					yield return select.SelectExtended(out count,
						currents, parameters, searches, sortcolumns,
						descendings, filters, startRow, maxRow: chunkSize);
					// currently there is a problem that count is increased each time...
					startRow = count;
					limit += chunkSize;
				} while (count >= limit);
			}
		}

		public static IEnumerable<PXResult<T>> SelectExtended<T>(
			this PXSelectBase<T> select,
			out int totalRows,
			object[] currents = null,
			object[] parameters = null,
			object[] searches = null,
			string[] sortcolumns = null,
			bool[] descendings = null,
			PXFilterRow[] filters = null,
			int startRow = 0,
			int maxRow = 0)
			where T : class, IBqlTable, new()
		{
			int refStart = startRow;
			totalRows = 0;
			var list = select
					  .View
					  .Select(currents, parameters, searches,
							  sortcolumns, descendings, filters,
							  ref refStart, maxRow, ref totalRows);
			return Select();
			// to allow out param
			IEnumerable<PXResult<T>> Select()
			{
				foreach (var item in list)
				{
					if (item is PXResult<T> res)
						yield return res;
					if (item is T t)
						yield return new PXResult<T>(t);
				}
			}
		}

		public static IEnumerable<PXResult<T>> SelectExtended<T>(
			this PXSelectBase<T> select,
			object[] currents = null,
			object[] parameters = null,
			object[] searches = null,
			string[] sortcolumns = null,
			bool[] descendings = null,
			PXFilterRow[] filters = null,
			int startRow = 0,
			int maxRow = 0)
			where T : class, IBqlTable, new()
		{
			return SelectExtended(select, out _, currents, parameters,
								  searches, sortcolumns, descendings,
								  filters, startRow, maxRow);
		}

		/// <summary>
		/// Clones items of the view to caches of provided Quote graph
		/// </summary>
		/// <param name="view">View, which content should be copied</param>
		/// <param name="graph">target Quote graph</param>
		/// <param name="quoteId">ID of Quote we started to create</param>
		/// <param name="currencyInfo">Currency info, created for the Uuota</param>
		/// <param name="keyField">key field, which should be cleared before insertion</param>
		public static void CloneView(this PXView view, PXGraph graph, Guid? quoteId, CM.Extensions.CurrencyInfo currencyInfo, string keyField = null)
		{
			Type cacheType = view.Cache.GetItemType();
			var cache = graph.Caches[view.Cache.GetItemType()];
			cache.Clear();
			foreach (object rec in view.SelectMulti())
			{
				object orig = PXResult.Unwrap(rec, cacheType);
				object item = view.Cache.CreateCopy(orig);

				view.Cache.SetValue<CROpportunityProducts.quoteID>(item, quoteId);
				view.Cache.SetValue<CROpportunityProducts.curyInfoID>(item, currencyInfo.CuryInfoID);

				if (view.Cache.Fields.Contains(nameof(INotable.NoteID)))
					view.Cache.SetValue(item, nameof(INotable.NoteID), Guid.NewGuid());

				if (!string.IsNullOrEmpty(keyField) && view.Cache.Fields.Contains(keyField))
				{
					view.Cache.SetValue(item, keyField, null);
					item = cache.Insert(item);
				}
				else
				{
					cache.SetStatus(item, PXEntryStatus.Inserted);
				}
				cache.Current = item;

				if (PXNoteAttribute.GetNoteIDIfExists(view.Cache, orig) != null)
				{
					string note = PXNoteAttribute.GetNote(view.Cache, orig);
					Guid[] files = PXNoteAttribute.GetFileNotes(view.Cache, orig);
					PXNoteAttribute.SetNote(cache, item, note);
					PXNoteAttribute.SetFileNotes(cache, item, files);
				}
			}
		}
	}
}
