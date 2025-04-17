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

using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Objects.PJ.DailyFieldReports.PM.CacheExtensions;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryChangeOrderExtension : DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
		[PXViewName(ViewNames.ChangeOrders)]
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportChangeOrder>
            .LeftJoin<PMChangeOrder>
                .On<DailyFieldReportChangeOrder.changeOrderId.IsEqual<PMChangeOrder.refNbr>>
            .LeftJoin<Customer>
                .On<Customer.bAccountID.IsEqual<PMChangeOrder.customerID>>
            .Where<DailyFieldReportChangeOrder.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View ChangeOrders;

        public PXAction<DailyFieldReport> CreateChangeOrder;

        public PXAction<DailyFieldReport> ViewChangeOrder;

		[InjectDependency]
		public IProjectDataProvider ProjectDataProvider
		{
			get;
			set;
		}

		protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.ChangeOrder, ViewNames.ChangeOrders);

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.changeOrder>();
        }

        [PXButton]
        [PXUIField]
        public virtual void viewChangeOrder()
        {
            var changeOrderEntry = PXGraph.CreateInstance<ChangeOrderEntry>();
            changeOrderEntry.Document.Current =
                changeOrderEntry.Document.Search<PMChangeOrder.refNbr>(ChangeOrders.Current.ChangeOrderId);
            PXRedirectHelper.TryRedirect(changeOrderEntry, PXRedirectHelper.WindowMode.NewWindow);
        }

        [PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
        [PXUIField(DisplayName = "Create New Change Order")]
        public virtual void createChangeOrder()
        {
            Base.Actions.PressSave();
            var graph = PXGraph.CreateInstance<ChangeOrderEntry>();
            var changeOrder = graph.Document.Insert();
            var dailyFieldReport = Base.DailyFieldReport.Current;
            changeOrder.ProjectID = dailyFieldReport.ProjectId;
            changeOrder.CustomerID = ProjectDataProvider.GetProject(Base, dailyFieldReport.ProjectId).CustomerID;
            changeOrder.Date = dailyFieldReport.Date;
            graph.Document.Cache.SetValueExt<PmChangeOrderExt.dailyFieldReportId>(changeOrder,
                dailyFieldReport.DailyFieldReportId);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
        }

        public override void _(Events.RowSelected<DailyFieldReport> args)
        {
            base._(args);
            if (args.Row is DailyFieldReport dailyFieldReport)
            {
				var isActionAvailable = IsCreationActionAvailable(dailyFieldReport)
									 && IsChangeOrderWorkflowEnabled(dailyFieldReport);

                CreateChangeOrder.SetEnabled(isActionAvailable);
				ChangeOrders.AllowInsert = isActionAvailable;
			}
            SetDisplayNames(Base.Caches<PMChangeOrder>());
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportChangeOrder))
            {
                RelationNumber = typeof(DailyFieldReportChangeOrder.changeOrderId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(ChangeOrders);
        }

        private static void SetDisplayNames(PXCache cache)
        {
            PXUIFieldAttribute.SetDisplayName<PMChangeOrder.extRefNbr>(cache, ProjectManagementLabels.ExtRefNbr);
            PXUIFieldAttribute.SetDisplayName<PMChangeOrder.lastModifiedDateTime>(
                cache, ProjectManagementLabels.LastModificationDate);
        }
    }
}
