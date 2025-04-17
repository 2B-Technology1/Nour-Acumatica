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
using System.Security.Permissions;
using PX.Data;
using PX.Data.EP;
using PX.Objects.EP.Imc;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class EPCalendarSync : PXGraph<EPCalendarSync>
	{
		[InjectDependency]
		protected internal IVCalendarFactory VCalendarFactory { get; private set; }

		//[SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
		public IEnumerable<CRActivity> GetCalendarEvents(Guid settingsId)
		{
			var result = new List<CRActivity>();
			Load();
			if (IsPublished(settingsId)) result.AddRange(GetEvents(settingsId)); //read items before scope desposing
			return result;
		}

		public bool IsPublished(Guid id)
		{
			PXResultset<SMCalendarSettings> set = PXSelect<SMCalendarSettings,
				Where<SMCalendarSettings.urlGuid,
					Equal<Required<SMCalendarSettings.urlGuid>>>>.Select(this, id);
			if (set != null && set.Count > 0) return ((SMCalendarSettings)set[0]).IsPublic.Value;
			return false;
		}

		public virtual IEnumerable<CRActivity> GetEvents(Guid id)
		{
			foreach (var item in PXSelectJoin<CRActivity,
				LeftJoin<EPAttendee, 
					On<EPAttendee.eventNoteID, Equal<CRActivity.noteID>>,
				InnerJoin<SMCalendarSettings, 
					On<SMCalendarSettings.userID, Equal<CRActivity.createdByID>,
					Or<SMCalendarSettings.userID, Equal<CRActivity.ownerID>,
					Or<SMCalendarSettings.userID, Equal<EPAttendee.contactID>>>>>>, //#B36E
				Where2<
					Where<CRActivity.classID, Equal<CRActivityClass.events>>,
					And<SMCalendarSettings.urlGuid, Equal<Required<SMCalendarSettings.urlGuid>>,
					And<SMCalendarSettings.isPublic, Equal<True>>>>,
				OrderBy<
					Desc<CRActivity.priority, 
					Asc<CRActivity.startDate, 
					Asc<CRActivity.endDate>>>>>.
					Select(this, id))
			{
				yield return item;
			}
		}
	}
}
