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
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CN.Compliance.CL.Services.DataProviders;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using ProjectDataProvider = PX.Objects.CN.Common.Services.DataProviders.ProjectDataProvider;

namespace PX.Objects.CN.Common.DependencyInjection
{
	public class Registration : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);
			builder.RegisterType<CacheService>().As<ICacheService>();
			builder.RegisterType<ComplianceAttributeTypeDataProvider>().As<IComplianceAttributeTypeDataProvider>();
			builder.RegisterType<ProjectDataProvider>().As<IProjectDataProvider>();
			builder.RegisterType<ProjectTaskDataProvider>().As<IProjectTaskDataProvider>();
			builder.RegisterType<BusinessAccountDataProvider>().As<IBusinessAccountDataProvider>();
			builder.RegisterType<LienWaiverReportCreator>().As<ILienWaiverReportCreator>();
			builder.RegisterType<PrintEmailLienWaiverBaseService>().As<IPrintEmailLienWaiverBaseService>();
			builder.RegisterType<PrintLienWaiversService>().As<IPrintLienWaiversService>();
			builder.RegisterType<EmailLienWaiverService>().As<IEmailLienWaiverService>();
			builder.RegisterType<RecipientEmailDataProvider>().As<IRecipientEmailDataProvider>();
			builder.RegisterType<EmployeeDataProvider>().As<IEmployeeDataProvider>();
		}
	}
}
