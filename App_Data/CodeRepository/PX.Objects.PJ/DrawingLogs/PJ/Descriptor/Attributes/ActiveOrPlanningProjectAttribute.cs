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

using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Data;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes
{
    public class ActiveOrPlanningProjectAttribute : ProjectAttribute
    {
        public ActiveOrPlanningProjectAttribute()
            : base(typeof(Where<PMProject.nonProject.IsEqual<False>
                .And<PMProject.baseType.IsEqual<CTPRType.project>>>))
        {
            DisplayName = DrawingLogLabels.Project;
            Visibility = PXUIVisibility.SelectorVisible;
            _Attributes.Add(new PXRestrictorAttribute(
                typeof(Where<PMProject.status.IsIn<ProjectStatus.active, ProjectStatusExtension.planning>>),
                DrawingLogMessages.OnlyProjectsInStatusesAreAllowed));
        }
    }
}
