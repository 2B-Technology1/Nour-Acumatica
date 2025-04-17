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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.PJ.Common.Actions
{
	public static class PXActionHelper
	{
		public static void InsertNoKeysFromUI(PXAdapter adapter)
		{
			Dictionary<string, object> vals = new Dictionary<string, object>();
			if (adapter.View.Cache.Insert(vals) == 1)
			{
				if (adapter.SortColumns == null)
				{
					adapter.SortColumns = adapter.View.Cache.Keys.ToArray();
				}
				else
				{
					List<string> cols = new List<string>(adapter.SortColumns);
					foreach (string key in adapter.View.Cache.Keys)
					{
						if (!CompareIgnoreCase.IsInList(cols, key))
						{
							cols.Add(key);
						}
					}
					adapter.SortColumns = cols.ToArray();
				}
				adapter.Searches = new object[adapter.SortColumns.Length];
				for (int i = 0; i < adapter.Searches.Length; i++)
				{
					object val;
					if (vals.TryGetValue(adapter.SortColumns[i], out val))
					{
						adapter.Searches[i] = val is PXFieldState ? ((PXFieldState)val).Value : val;
					}
				}
				adapter.StartRow = 0;
			}
		}
	}
}
