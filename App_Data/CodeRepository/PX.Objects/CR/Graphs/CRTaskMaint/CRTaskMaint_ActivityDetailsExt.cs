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
using PX.Data;
using PX.Objects.CR.Extensions;

namespace PX.Objects.CR.CRTaskMaint_Extensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CRTaskMaint_ActivityDetailsExt_Actions : ActivityDetailsExt_Child_Actions<CRTaskMaint_ActivityDetailsExt, CRTaskMaint, CRActivity, CRActivity.noteID> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CRTaskMaint_ActivityDetailsExt : ActivityDetailsExt_Child<CRTaskMaint, CRActivity, CRActivity.noteID>
	{
		public override Type GetBAccountIDCommand() => typeof(CRActivity.bAccountID);
		public override Type GetContactIDCommand() => typeof(CRActivity.contactID);

		protected virtual void _(Events.RowSelected<CRActivity> e)
		{
			CRActivity row = e.Row as CRActivity;
			if (row == null)
				return;

			string status = ((string)e.Cache.GetValueOriginal<CRActivity.uistatus>(row) ?? ActivityStatusListAttribute.Open);
			bool editable = status == ActivityStatusListAttribute.Open || status == ActivityStatusListAttribute.Draft || status == ActivityStatusListAttribute.InProcess;
			bool deleteable = this.AnyBillableChildExists(row.NoteID) == false;
			Base.Delete.SetEnabled(deleteable);

			this.Activities.AllowDelete = editable;
		}

		protected virtual void _(Events.RowSelected<PMTimeActivity> e)
		{
			var row = e.Row as PMTimeActivity;
			if (row == null) return;

			(
				row.TimeSpent,
				row.OvertimeSpent,
				row.TimeBillable,
				row.OvertimeBillable
			) = this.GetChildrenTimeTotals(row.RefNoteID);

			var cache = e.Cache;

			PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeSpent>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.overtimeSpent>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeBillable>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.overtimeBillable>(cache, row, false);
		}

		protected virtual void _(Events.RowSelecting<PMTimeActivity> e)
		{
			using (new PXConnectionScope())
			{
				var row = e.Row as PMTimeActivity;
				if (row == null) return;

				(
					row.TimeSpent,
					row.OvertimeSpent,
					row.TimeBillable,
					row.OvertimeBillable
				) = this.GetChildrenTimeTotals(row.RefNoteID);
			}
		}
	}
}
