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

using PX.Data;
using System;
using System.Collections;
using System.Linq;

namespace PX.Objects.EP
{
	public class EPApprovalView<Table> : PXSetup<Table>
		where Table : class, IBqlTable, new()
	{
		public EPApprovalView(PXGraph graph): base(graph) {}

		public new static PXResultset<Table> Select(PXGraph graph, params object[] pars) => SelectMultiBound(graph, null, pars);

		public new static PXResultset<Table> SelectMultiBound(PXGraph graph, object[] currents, params object[] pars)
		{
			PXCache cache = graph.Caches[typeof(Table)];
			PXGraph.GetDefaultDelegate del;
			if (graph.Defaults.TryGetValue(typeof(Table), out del))
			{
				cache.Current = null;

				PXResultset<Table> res = new PXResultset<Table>();
				object record = del();
				res.Add(new PXResult<Table>(record as Table));
				return res;
			}
			else if (cache.Keys.Count == 0)
			{
				return PXSelectReadonly<Table>.Select(graph, pars);
			}
			else
			{
				string name = null;
				lock (((ICollection)_members).SyncRoot)
				{
					if (!_members.TryGetValue(graph.GetType(), out name))
					{
						foreach (PXViewInfo view in GraphHelper.GetGraphViews(graph.GetType(), false))
						{
							if (view.Cache.Name == typeof(Table).FullName)
							{
								name = view.Name;
								break;
							}
						}

						if (name == null)
						{
							foreach (string key in graph.Views.Keys.OrderBy(s => s, new ViewNameComparer()))
							{
								var view = graph.Views[key];
								if (
									!view.IsReadOnly &&
									(view.GetItemType() == typeof(Table) || view.GetItemType().IsAssignableFrom(typeof(Table)) && !Attribute.IsDefined(typeof(Table), typeof(PXBreakInheritanceAttribute), false))
								)
								{
									name = key;
									break;
								}
							}
						}

						_members[graph.GetType()] = name;
					}
				}

				if (!string.IsNullOrEmpty(name))
				{
					var view = graph.Views[name];
					PXSelectBase<Table> ext = GetViewExternalMember(graph, view) as PXSelectBase<Table>;

					var ret = new PXResultset<Table>();

					if (ext != null)
					{
						foreach (object item in ext.View.SelectMultiBound(currents, pars))
						{
							if (!(item is PXResult<Table> result))
							{
								if (item is Table table)
								{
									ret.Add(new PXResult<Table>(table));
								}
							}
							else
							{
								ret.Add(result);
							}
						}

						return ret;
					}

					foreach (object item in view.SelectMultiBound(currents, pars))
					{
						if (!(item is PXResult<Table> result))
						{
							if (item is Table table)
							{
								ret.Add(new PXResult<Table>(table));
							}
						}
						else
						{
							ret.Add(result);
						}
					}

					return ret;
				}
			}

			return null;
		}
	}
}
