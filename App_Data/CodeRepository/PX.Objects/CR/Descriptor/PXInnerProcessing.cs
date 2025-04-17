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
using PX.Common;
using PX.Data;

namespace PX.Objects.CR
{
	/// <exclude/>
	[PXInternalUseOnly]
	public class PXInnerProcessing<TPrimary, TDetail> : PXProcessing<TPrimary>
		where TPrimary : class, IBqlTable, new()
		where TDetail : class, IBqlTable, new()
	{
		public PXInnerProcessing(PXGraph graph) : base(graph) { }
		public PXInnerProcessing(PXGraph graph, Delegate handler) : base(graph, handler) { }

		protected override void _PrepareGraph<Table>()
		{
			AttachActions<Table>();

			AttachBaseActions<Table>();

			this.SetProcessAllVisible(false);
			this.SetProcessVisible(false);
			this._ScheduleButton.SetVisible(false);

			this.SuppressMerge = true;
			this.SuppressUpdate = true;
		}

		[PXUIField(DisplayName = ActionsMessages.Close)]
		[PXButton(DisplayOnMainToolbar = false)]
		protected override IEnumerable actionCloseProcessing(PXAdapter adapter)
		{
			// override to prevent window closing because of CloseToList

			foreach (var result in this._Graph.Actions["Cancel"].Press(adapter))
			{
				PXRedirectHelper.TryRedirect(this._Graph, PXRedirectHelper.WindowMode.Same);

				yield return result;
			}
		}
	}
}
