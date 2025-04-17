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

using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Services;
using PX.Objects.CS;

namespace PX.Objects.PJ.Common.GraphExtensions
{
    public class NumberingMaintExt : PXGraphExtension<NumberingMaint>
    {
        [InjectDependency]
        public INumberingSequenceUsage NumberingSequenceUsage
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        protected virtual void _(Events.RowDeleting<Numbering> args)
        {
            NumberingSequenceUsage
                .CheckForNumberingUsage<ProjectManagementSetup, ProjectManagementSetup.projectIssueNumberingId>(
                    args.Row, Base, CacheNames.ProjectIssue);
            NumberingSequenceUsage
                .CheckForNumberingUsage<ProjectManagementSetup,
                    ProjectManagementSetup.requestForInformationNumberingId>(
                    args.Row, Base, CacheNames.RequestForInformation);
            NumberingSequenceUsage
                .CheckForNumberingUsage<DrawingLogSetup, DrawingLogSetup.drawingLogNumberingSequenceId>(
                    args.Row, Base, CacheNames.DrawingLog);
            NumberingSequenceUsage
                .CheckForNumberingUsage<ProjectManagementSetup, ProjectManagementSetup.dailyFieldReportNumberingId>(
                    args.Row, Base, CacheNames.DailyFieldReport);
            NumberingSequenceUsage
                .CheckForNumberingUsage<ProjectManagementSetup, ProjectManagementSetup.submittalNumberingId>(
                    args.Row, Base, CacheNames.Submittal);
        }
    }
}