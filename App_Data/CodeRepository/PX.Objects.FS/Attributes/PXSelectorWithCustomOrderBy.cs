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
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class PXSelectorWithCustomOrderByAttribute : PXSelectorAttribute
    {
        public PXSelectorWithCustomOrderByAttribute(Type type) : base(type)
        {
        }

        public PXSelectorWithCustomOrderByAttribute(Type type, params Type[] fieldList) : base(type, fieldList)
        {
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            CreateViewWithOrderBy(sender);
        }

        private void CreateViewWithOrderBy(PXCache sender)
        {
            PXView view = new PXViewWithOrderBy(sender.Graph, true, _Select);
            sender.Graph.Views[_ViewName] = view;
        }

        private class PXViewWithOrderBy : PXView
        {
            public PXViewWithOrderBy(PXGraph graph, bool isReadOnly, BqlCommand select) : base(graph, isReadOnly, select)
            {
            }

            public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
            {
                IBqlSortColumn[] bqlSortColumns = BqlSelect.GetSortColumns();

                for (int i = 0; i < bqlSortColumns.Count(); i++)
                {
                    if (searches.Count() > i && searches[i] == null)
                    {
                        sortcolumns[i] = bqlSortColumns[i].GetReferencedType().Name;
                        descendings[i] = bqlSortColumns[i].IsDescending;
                    }
                }

                return base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
            }
        }
    }
}
