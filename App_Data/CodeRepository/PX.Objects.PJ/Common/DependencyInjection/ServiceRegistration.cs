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

using Autofac;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services;
using PX.Objects.PJ.DailyFieldReports.SM.Services;
using PX.Objects.PJ.DrawingLogs.PJ.Services;
using PX.Objects.PJ.PhotoLogs.PJ.Services;
using PX.Objects.PJ.ProjectManagement.PJ.Services;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.PJ.DailyFieldReports.PJ.Services;

namespace PX.Objects.PJ.Common.DependencyInjection
{
    public class ServiceRegistration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<DrawingLogDataProvider>().As<IDrawingLogDataProvider>();
            builder.RegisterType<ProjectManagementClassUsageService>().As<IProjectManagementClassUsageService>();
            builder.RegisterType<ProjectManagementClassDataProvider>().As<IProjectManagementClassDataProvider>();
            builder.RegisterType<ProjectManagementImpactService>().As<IProjectManagementImpactService>();
            builder.RegisterType<NumberingSequenceUsage>().As<INumberingSequenceUsage>();
            builder.RegisterType<BusinessAccountDataProvider>().As<IBusinessAccountDataProvider>();
            builder.RegisterType<ProjectDataProvider>().As<IProjectDataProvider>();
            builder.RegisterType<PhotoLogDataProvider>().As<IPhotoLogDataProvider>();
            builder.RegisterType<PhotoConfirmationService>().As<IPhotoConfirmationService>();
            builder.RegisterType<FilesDataProvider>().As<IFilesDataProvider>();
            builder.RegisterType<WeatherIntegrationService>().As<IWeatherIntegrationService>();
            builder.RegisterType<WeatherIntegrationUnitOfMeasureService>()
                .As<IWeatherIntegrationUnitOfMeasureService>();
        }
    }
}