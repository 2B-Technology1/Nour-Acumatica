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
using PX.Data;
using PX.Objects.PO;

namespace PX.Objects.IN
{
	[PXHidden]
	public class INReplenishmentMaint : PXGraph<INReplenishmentMaint,INReplenishmentOrder>
	{
		public PXSelect<INReplenishmentOrder> Document;
		public PXSelect<POLine> planRelease;
		public PXSetup<INSetup> setup;

		public class POLinePlan : PO.GraphExtensions.POOrderEntryExt.POLinePlan<INReplenishmentMaint>
		{
			protected virtual void _(Events.RowInserted<INReplenishmentOrder> e)
			{
				PXNoteAttribute.GetNoteID<INReplenishmentOrder.noteID>(e.Cache, e.Row);
			}

			public override void _(Events.RowPersisting<INItemPlan> e)
			{
				if (e.Operation.Command() != PXDBOperation.Delete)
				{
					e.Row.RefNoteID = PXNoteAttribute.GetNoteID<INReplenishmentOrder.noteID>(Base.Document.Cache, Base.Document.Current);
					e.Row.RefEntityType = typeof(INReplenishmentOrder).FullName;
				}
				base._(e);
			}
		}
	}
}
