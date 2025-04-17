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

using System.Collections;
using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.ProjectAccounting.PM.Services;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntrySubcontractorActivityExtension :
        DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXViewName(ViewNames.Subcontractors)]
        [PXCopyPasteHiddenFields(typeof(DailyFieldReportSubcontractorActivity.totalWorkingTimeSpent),
            typeof(Note.noteText))]
        public SelectFrom<DailyFieldReportSubcontractorActivity>
            .LeftJoin<Vendor>
                .On<Vendor.bAccountID.IsEqual<DailyFieldReportSubcontractorActivity.vendorId>>
            .Where<DailyFieldReportSubcontractorActivity.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View Subcontractors;

        [InjectDependency]
        public IProjectTaskDataProvider ProjectTaskDataProvider
        {
            get;
            set;
        }

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.Subcontractor, ViewNames.Subcontractors);

        public virtual void _(Events.FieldVerifying<DailyFieldReportSubcontractorActivity,
            DailyFieldReportSubcontractorActivity.workingTimeSpent> args)
        {
            var defaultWorkingTimeSpent = args.Row.DefaultWorkingTimeSpent;
            var subcontractorActivity = args.Row;
            if ((int) args.NewValue > defaultWorkingTimeSpent
                && subcontractorActivity.TimeArrived < subcontractorActivity.TimeDeparted)
            {
                var message =
                    string.Format(DailyFieldReportMessages.WorkingHoursCannotExceedDefaultValue,
                        PXTimeListAttribute.GetString(defaultWorkingTimeSpent));
                throw new PXSetPropertyException<DailyFieldReportSubcontractorActivity.workingTimeSpent>(message);
            }
        }

        public virtual void _(Events.FieldUpdated<DailyFieldReportSubcontractorActivity,
            DailyFieldReportSubcontractorActivity.projectTaskID> args)
        {
            if (args.NewValue is int costTaskId && args.Row.Description == null)
            {
                var projectTask = ProjectTaskDataProvider.GetProjectTask(Base, Base.DailyFieldReport.Current.ProjectId, costTaskId);
                args.Cache.SetValueExt<DailyFieldReportSubcontractorActivity.description>(args.Row,
                    projectTask.Description);
            }
        }

        public virtual void _(Events.RowSelected<DailyFieldReportSubcontractorActivity> args)
        {
            var subcontractorActivity = args.Row;
            if (Base.IsMobile && subcontractorActivity != null)
            {
                subcontractorActivity.LastModifiedDateTime =
                    subcontractorActivity.LastModifiedDateTime.GetValueOrDefault().Date;
            }
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportSubcontractorActivity))
            {
                RelationId = typeof(DailyFieldReportSubcontractorActivity.subcontractorId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(Subcontractors);
        }
        
        public PXAction<VendorR> viewVendor;
        [PXUIField(DisplayName = "View Vendor", MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select)]
        [PXButton(DisplayOnMainToolbar = false, VisibleOnProcessingResults = false, PopupVisible = false)]
        public virtual IEnumerable ViewVendor(PXAdapter adapter)
        {
            VendorMaint target = PXGraph.CreateInstance<VendorMaint>();
            DailyFieldReportSubcontractorActivity subcontractor = (DailyFieldReportSubcontractorActivity) Subcontractors.Current;
            
            if (subcontractor == null)
                return adapter.Get();
        
            target.BAccount.Current = target.BAccount.Search<VendorR.bAccountID>(subcontractor.VendorId);
            if (target.BAccount.Current != null)
            {
                throw new PXRedirectRequiredException(target, true, "redirect")
                    {Mode = PXBaseRedirectException.WindowMode.NewWindow};
            }
            else
            {
                VendorR vendor = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Current<DailyFieldReportSubcontractorActivity.vendorId>>>>.Select(Base);
                throw new PXException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExistOrNoRights, vendor.AcctCD));
            }
        }
    }
}
