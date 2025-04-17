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

using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.PJ.ProjectManagement.PJ.GraphExtensions
{
    public class SmNotificationMaintExt : PXGraphExtension<SMNotificationMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public void _(Events.RowPersisting<Notification> args)
        {
            var notificationTemplate = args.Row;
            if (notificationTemplate != null && args.Operation == PXDBOperation.Delete)
            {
                UpdateProjectManagementSetupIfNeeded(notificationTemplate.NotificationID);
            }
        }

        private void UpdateProjectManagementSetupIfNeeded(int? notificationId)
        {
            var projectManagementSetup = new PXSelect<ProjectManagementSetup>(Base).SelectSingle();
            if (projectManagementSetup.DefaultEmailNotification == notificationId)
            {
                UpdateProjectManagementSetup(projectManagementSetup);
            }
        }

        private void UpdateProjectManagementSetup(ProjectManagementSetup projectManagementSetup)
        {
            projectManagementSetup.DefaultEmailNotification = null;
            Base.Caches<ProjectManagementSetup>().Update(projectManagementSetup);
            Base.Caches<ProjectManagementSetup>().PersistUpdated(projectManagementSetup);
        }
    }
}