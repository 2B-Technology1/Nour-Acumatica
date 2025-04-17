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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.PJ.Common.Actions;
using PX.Data;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.Common.Extensions;

namespace PX.Objects.PJ.Submittals.PJ.Graphs
{
	public partial class SubmittalEntry
	{
		public class CancelAction<TNode> : PXCancel<TNode>
			where TNode : class, IBqlTable, new()
		{
			public CancelAction(PXGraph graph, string name) : base(graph, name)
			{
			}

			public CancelAction(PXGraph graph, Delegate handler) : base(graph, handler)
			{
			}

			[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
			[PXCancelButton]
			protected override IEnumerable Handler(PXAdapter adapter)
			{
				string submittalIDToSelect = (string)adapter.Searches.GetSearchValueByPosition(0);

				PJSubmittal previousSubmittal = (PJSubmittal)Graph.GetPrimaryCache().Current;

				if (previousSubmittal?.SubmittalID != submittalIDToSelect)
				{
					adapter.SortColumns = new string[] {adapter.SortColumns[0]};
					adapter.Searches = new object[] {adapter.Searches[0]};
					adapter.Descendings = new[] {adapter.Descendings[0]};
				}

				return base.Handler(adapter);
			}

			protected override void Insert(PXAdapter adapter)
			{
				PXActionHelper.InsertNoKeysFromUI(adapter);
			}
		}
	}
}
