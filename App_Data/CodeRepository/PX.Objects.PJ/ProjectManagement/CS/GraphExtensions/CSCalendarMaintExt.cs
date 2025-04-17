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
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.PJ.ProjectManagement.CS.GraphExtensions
{
    public class CsCalendarMaintExt : PXGraphExtension<CSCalendarMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        protected virtual void _(Events.RowDeleting<CSCalendar> args)
        {
            var calendar = args.Row;
            if (IsCalendarUsed(calendar.CalendarID))
            {
                throw new Exception(ProjectManagementMessages.WorkCalendarCannotBeDeleted);
            }
        }

        private bool IsCalendarUsed(string calendarId)
        {
            return new PXSelect<ProjectManagementSetup,
                    Where<ProjectManagementSetup.calendarId,
                        Equal<Required<ProjectManagementSetup.calendarId>>>>(Base)
                .SelectSingle(calendarId) != null;
        }
    }
}