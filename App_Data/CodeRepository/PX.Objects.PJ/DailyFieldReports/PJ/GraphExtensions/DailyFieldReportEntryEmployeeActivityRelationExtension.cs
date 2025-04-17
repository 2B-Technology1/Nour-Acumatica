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
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryEmployeeActivityRelationExtension :
        DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportEmployeeActivity>
            .Where<DailyFieldReportEmployeeActivity.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View DailyFieldReportEmployeeActivity;

        [PXHidden]
        public SelectFrom<EPActivityApprove>.View EpActivityApprove;

        public PXAction<DailyFieldReport> ViewTimeCard;

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.EmployeeTimeActivity, ViewNames.EmployeeActivities);

        protected override Type RelationPrimaryCacheType => typeof(EPActivityApprove);

        public override void Initialize()
        {
            Relations = new PXSelectExtension<DailyFieldReportRelation>(DailyFieldReportEmployeeActivity);
        }

        [PXDefault(typeof(DailyFieldReport.projectId.FromCurrent))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public virtual void EPActivityApprove_ProjectID_CacheAttached(PXCache cache)
        {
        }

        [PXButton]
        [PXUIField]
        public virtual void viewTimeCard()
        {
            var timeCardCd = (Base.Caches<EPActivityApprove>().Current as EPActivityApprove)?.TimeCardCD;
            if (timeCardCd != null)
            {
                var timeCardMaint = PXGraph.CreateInstance<TimeCardMaint>();
                timeCardMaint.Document.Current = timeCardMaint.Document.Search<EPTimeCard.timeCardCD>(timeCardCd);
                timeCardMaint.RedirectToEntity(timeCardMaint.Document.Current, PXRedirectHelper.WindowMode.NewWindow);
            }
        }

		[Obsolete]
		public virtual void _(Events.RowPersisting<EPActivityApprove> args)
		{ }

		[Obsolete]
		public virtual void _(Events.RowPersisted<EPActivityApprove> args)
		{ }

		protected virtual void _(Events.RowInserted<EPActivityApprove> e)
		{
			var dailyFieldReportEmployeeActivity = new DailyFieldReportEmployeeActivity
			{
				DailyFieldReportId = Base.DailyFieldReport.Current.DailyFieldReportId,
				EmployeeActivityId = e.Row.NoteID
			};
			DailyFieldReportEmployeeActivity.Insert(dailyFieldReportEmployeeActivity);
		}

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportEmployeeActivity))
            {
                RelationNoteId = typeof(DailyFieldReportEmployeeActivity.employeeActivityId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(DailyFieldReportEmployeeActivity);
        }
    }
}
