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
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.CR.CRTaskMaint_Extensions;
using PX.Objects.EP;
using PX.SM;

namespace PX.Objects.CR
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CRTaskMaint_ReferencedTasksExt : PXGraphExtension<CRTaskMaint>
	{
		[PXHidden]
		public sealed class CRActivityReferencedTasksBackwardCompatibility : CRChildActivity
		{
			#region RecordID
			public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

			[PXInt]
			[PXUIField(DisplayName = "Record ID", Visible = false)]
			public int? RecordID { get; set; }
			#endregion

			#region CompletedDateTime
			public abstract class completedDateTime : PX.Data.BQL.BqlDateTime.Field<completedDateTime> { }

			[PXDate(InputMask = "g", DisplayMask = "g")]
			[PXUIField(DisplayName = "Completed At", Enabled = false)]
			public DateTime? CompletedDateTime { get; set; }
			#endregion

			#region Status
			public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

			[PXString]
			[ActivityStatusList]
			[PXUIField(DisplayName = "Status", Enabled = false)]
			public string Status { get; set; }
			#endregion
		}

		[PXFilterable]
		public PXSelect<CRActivityReferencedTasksBackwardCompatibility> ReferencedTasks;

		public IEnumerable referencedTasks()
		{
			foreach (var cRChild in Base.GetExtension<CRTaskMaint_ActivityDetailsExt>().Activities.Select().RowCast<CRChildActivity>().ToList().Where(_ => _.ClassID == CRActivityClass.Task))
			{
				var item = new CRActivityReferencedTasksBackwardCompatibility();
				item.CompletedDateTime = cRChild.CompletedDate;
				item.EndDate = cRChild.EndDate;
				item.StartDate = cRChild.StartDate;
				item.Status = cRChild.UIStatus;
				item.Subject = cRChild.Subject;
				item.RefNoteID = cRChild.RefNoteID;
				item.NoteID = cRChild.NoteID;
				yield return item;
			}
		}
	}
}
