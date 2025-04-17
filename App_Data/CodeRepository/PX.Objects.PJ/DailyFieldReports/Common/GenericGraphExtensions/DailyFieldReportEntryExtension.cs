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
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions
{
    /// <summary>
    /// This generic graph extension is used only for <see cref="DailyFieldReportEntry" /> graph.
    /// Generic type of this extension is required for correct working with Features availability.
    /// </summary>
    public abstract class DailyFieldReportEntryExtension<TGraph> : PXGraphExtension<TGraph>
        where TGraph : PXGraph
    {
		public PXSelectExtension<DailyFieldReportRelation> Relations;

        protected abstract (string Entity, string View) Name
        {
            get;
        }

        protected virtual Type RelationPrimaryCacheType => Relations.Cache.BqlTable;

        public override void Initialize()
        {
            Relations = CreateRelationsExtension();
        }

        public virtual void _(Events.RowDeleting<DailyFieldReport> args)
        {
            if (Relations.Select().Any())
            {
                var message = string.Format(DailyFieldReportMessages.ThereAreRelatedEntitiesToDfrOnDelete,
                    Name.Entity);
                throw new PXException(message);
            }
        }

        public virtual void _(Events.RowSelected<DailyFieldReport> args)
        {
            if (args.Row is DailyFieldReport dailyFieldReport)
            {
                var isEnabled = ShouldTabsBeEditable(dailyFieldReport);
                Base.Caches[RelationPrimaryCacheType].Enable(isEnabled);
            }
        }

        public virtual void _(Events.FieldVerifying<DailyFieldReport.projectId> args)
        {
            if (Relations.Select().Any() && !Base.IsCopyPasteContext)
            {
                var message = string.Format(DailyFieldReportMessages
                    .ThereIsOneOrMoreEntitiesRelatedToTheProjectOnTheTab, Name.Entity, Name.View);
                var projectDataProvider = new ProjectDataProvider();
                var project = projectDataProvider.GetProject(Base, (int?) args.NewValue);
                args.NewValue = project?.ContractCD;
                throw new PXSetPropertyException<DailyFieldReport.projectId>(message);
            }
        }

        protected bool IsCreationActionAvailable(DailyFieldReport dailyFieldReport)
        {
            return Base.Caches<DailyFieldReport>().GetStatus(dailyFieldReport) != PXEntryStatus.Inserted &&
                ShouldTabsBeEditable(dailyFieldReport);
        }

		protected virtual bool IsChangeOrderWorkflowEnabled(DailyFieldReport dailyFieldReport)
		{
			var projectId = dailyFieldReport.ProjectId;

			if (projectId == null)
				return false;

			var project = PMProject.PK.Find(Base, projectId);

			return project?.ChangeOrderWorkflow == true;
		}

        protected abstract DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping();

        protected abstract PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension();

        protected bool ShouldTabsBeEditable(DailyFieldReport dailyFieldReport)
        {
            return dailyFieldReport.Hold == true && dailyFieldReport.ProjectId != null;
        }
    }
}
