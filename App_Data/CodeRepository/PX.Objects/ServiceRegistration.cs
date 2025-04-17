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
using System.Linq;
using System.Web.Compilation;

using Autofac;
using Microsoft.Extensions.Options;
using PX.CloudServices;
using PX.Data;
using PX.Data.EP;
using PX.Data.Process;
using PX.Data.RelativeDates;
using PX.Data.Search;
using PX.Objects.AP.InvoiceRecognition;
using PX.Objects.AR.Repositories;
using PX.Objects.AU;
using PX.Objects.CA;
using PX.Objects.CA.Repositories;
using PX.Objects.CM.Extensions;
using PX.Objects.CR.CRMarketingListMaint_Extensions;
using PX.Objects.CS;
using PX.Objects.EndpointAdapters;
using PX.Objects.EndpointAdapters.WorkflowAdapters.AP;
using PX.Objects.EndpointAdapters.WorkflowAdapters.AR;
using PX.Objects.EndpointAdapters.WorkflowAdapters.IN;
using PX.Objects.EndpointAdapters.WorkflowAdapters.PO;
using PX.Objects.EndpointAdapters.WorkflowAdapters.SO;
using PX.Objects.EP;
using PX.Objects.FA;
using PX.Objects.GL.FinPeriods;
using PX.Objects.IN.Services;
using PX.Objects.PM;
using PX.Objects.SM;

using Assembly = System.Reflection.Assembly;

namespace PX.Objects
{
	public class ServiceRegistration : Module
	{
		protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<FinancialPeriodManager>()
                .As<IFinancialPeriodManager>();

            // Acuminator disable once PX1003 NonSpecificPXGraphCreateInstance [Justification]
            builder
                .RegisterType<FinPeriodScheduleAdjustmentRule>()
                .WithParameter(TypedParameter.From<Func<PXGraph>>(() => new PXGraph())) // TODO: Use single PXGraph instance to cache queries
                .As<IScheduleAdjustmentRule>()
                .SingleInstance();

			builder
				.RegisterType<TodayBusinessDate>()
				.As<ITodayUtc>();

			RegisterNotificationServices(builder);

			builder
				.RegisterType<FinPeriodRepository>()
				.As<IFinPeriodRepository>();

			builder
				.RegisterType<FinPeriodUtils>()
				.As<IFinPeriodUtils>();

			builder
				.Register<Func<PXGraph, IPXCurrencyService>>(context
					=>
					{
						return (graph)
						=>
						{
							return new DatabaseCurrencyService(graph);
						};
					});

			builder
				.RegisterType<FABookPeriodRepository>()
				.As<IFABookPeriodRepository>();

			builder
				.RegisterType<FABookPeriodUtils>()
				.As<IFABookPeriodUtils>();

			builder
				.RegisterType<BudgetService>()
				.As<IBudgetService>();

			builder
				.RegisterType<UnitRateService>()
				.As<IUnitRateService>();

			builder
				.RegisterType<PM.ProjectSettingsManager>()
				.As<PM.IProjectSettingsManager>();

			builder
				.RegisterType<PM.CostCodeManager>()
				.As<PM.ICostCodeManager>();

			builder
				.RegisterType<PM.ProjectMultiCurrency>()
				.As<PM.IProjectMultiCurrency>();

			RegisterCbApiAdapters(builder);

			builder.RegisterType<BillAdapter>().AsSelf();
			builder.RegisterType<CheckAdapter>().AsSelf();

			builder.RegisterType<InvoiceAdapter>().AsSelf();
			builder.RegisterType<PaymentAdapter>().AsSelf();

			builder.RegisterType<InventoryReceiptAdapter>().AsSelf();
			builder.RegisterType<AdjustmentAdapter>().AsSelf();
			builder.RegisterType<InventoryAdjustmentAdapter>().AsSelf();
			builder.RegisterType<TransferOrderAdapter>().AsSelf();
			builder.RegisterType<KitAssemblyAdapter>().AsSelf();

			builder.RegisterType<PurchaseOrderAdapter>().AsSelf();
			builder.RegisterType<PurchaseReceiptAdapter>().AsSelf();

			builder.RegisterType<SalesOrderAdapter>().AsSelf();
			builder.RegisterType<ShipmentAdapter>().AsSelf();
			builder.RegisterType<SalesInvoiceAdapter>().AsSelf();

			builder
				.RegisterType<CN.Common.Services.NumberingSequenceUsage>()
				.As<CN.Common.Services.INumberingSequenceUsage>();

			builder
				.RegisterType<AdvancedAuthenticationRestrictor>()
				.As<IAdvancedAuthenticationRestrictor>()
				.SingleInstance();

			builder
				.RegisterType<PXEntitySearchEnriched>()
				.As<IEntitySearchService>();

			builder
				.RegisterType<InventoryAccountService>()
				.As<IInventoryAccountService>();


			builder
				.RegisterType<PXEntitySearchEnriched>()
				.As<IEntitySearchService>();

			builder
				.RegisterType<CustomTimeRegionProvider>()
				.As<PX.Common.ITimeRegionProvider>()
				.SingleInstance();
			builder
				.RegisterType<DirectDepositTypeService>()
				.AsSelf();

			builder
				.RegisterType<CABankTransactionsRepository>()
				.As<ICABankTransactionsRepository>();

			builder
				.RegisterType<CCDisplayMaskService>()
				.As<ICCDisplayMaskService>()
				.PreserveExistingDefaults();

			RegisterEPModuleServices(builder);
			RegisterMailServices(builder);

			RegisterCrHelpers(builder);

			builder
				.BindFromConfiguration<InvoiceRecognitionModelOptions>("invoicerecognitionmodel");
			builder
				.RegisterInstance<IValidateOptions<InvoiceRecognitionModelOptions>>(
					new ValidateOptions<InvoiceRecognitionModelOptions>(null, options => Models.KnownModels.ContainsKey(options.Name),
						"Unknown invoice recognition model"));
		}

		private void RegisterNotificationServices(ContainerBuilder builder)
		{
			builder.RegisterType<EP.NotificationProvider>()
				.As<INotificationSender>()
				.SingleInstance()
				.PreserveExistingDefaults();

			builder.RegisterType<EP.NotificationProvider>()
				.As<INotificationSenderWithActivityLink>()
				.SingleInstance()
				.PreserveExistingDefaults();

			builder
				.RegisterType<PX.Objects.SM.NotificationService>()
				.As<PX.SM.INotificationService>()
				.SingleInstance();
		}

		private void RegisterEPModuleServices(ContainerBuilder builder)
		{
			builder
				.RegisterType<EPEventVCalendarProcessor>()
				.As<PX.Objects.EP.Imc.IVCalendarProcessor>()
				.SingleInstance();

			builder
				.RegisterType<PX.Objects.EP.Imc.VCalendarFactory>()
				.As<PX.Objects.EP.Imc.IVCalendarFactory>()
				.SingleInstance();

			builder
				.RegisterType<PX.Objects.EP.ActivityService>()
				.As<IActivityService>()
				.SingleInstance();

			builder
				.RegisterType<EP.ReportNotificationGenerator>()
				.AsSelf()
				.InstancePerDependency();
		}

		private void RegisterMailServices(ContainerBuilder builder)
		{
			builder
				.RegisterType<PX.Objects.EP.CommonMailSendProvider>()
				.As<IMailSendProvider>()
				.SingleInstance();

			builder
				.RegisterType<PX.Objects.EP.CommonMailReceiveProvider>()
				.UsingConstructor(typeof(IEmailProcessorsProvider))
				.As<IMailReceiveProvider>()
				.As<IMessageProccessor>()
				.As<IOriginalMailProvider>()
				.SingleInstance();

			var assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray();
			builder
				.RegisterAssemblyTypesAssignableToWithCaching(null, typeof(IEmailProcessor), assemblies)
				.AssignableTo<IEmailProcessor>()
				.As<IEmailProcessor>()
				.SingleInstance();

			builder
				.RegisterType<OrderedEmailProcessorsProvider>()
				.As<IEmailProcessorsProvider>()
				.SingleInstance()
				.PreserveExistingDefaults();
		}

		private void RegisterCbApiAdapters(ContainerBuilder builder)
		{
			builder.RegisterType<DefaultEndpointImplCR>().AsSelf();
			builder.RegisterType<DefaultEndpointImplCR20>().AsSelf();
			builder.RegisterType<DefaultEndpointImplCR22>().AsSelf();
			builder.RegisterType<DefaultEndpointImplCR23>().AsSelf();
			builder.RegisterType<DefaultEndpointImplPM>().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.CaseApplicator>().SingleInstance().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.OpportunityApplicator>().SingleInstance().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.LeadApplicator>().SingleInstance().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.ProjectTemplateApplicator>().SingleInstance().AsSelf();
			builder.RegisterType<CbApiWorkflowApplicator.ProjectTaskApplicator>().SingleInstance().AsSelf();
		}

		private void RegisterCrHelpers(ContainerBuilder builder)
		{
			builder
				.RegisterType<CRMarketingListMemberRepository>()
				.As<ICRMarketingListMemberRepository>();
		}
	}
}
