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
using System.Collections.Generic;

using PX.Api.Models;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.EP;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Services;
using PX.Objects.PJ.DailyFieldReports.PM.CacheExtensions;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Graphs
{
	public class DailyFieldReportEntry : PXGraph<DailyFieldReportEntry, DailyFieldReport>
	{
		#region DAC Overrides

		#region EPApproval Cache Attached - Approvals Fields
		[PXDBDate()]
		[PXDefault(typeof(DailyFieldReport.date), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.docDate> e)
		{
		}
		#endregion

		#endregion

		public PXSetup<ProjectManagementSetup> ProjectManagementSetup;

		[PXViewName("Daily Field Report")]
		[PXCopyPasteHiddenFields(typeof(DailyFieldReport.icon))]
		public SelectFrom<DailyFieldReport>
			.LeftJoin<PMProject>
				.On<PMProject.contractID.IsEqual<DailyFieldReport.projectId>>
			.Where<
				PMProject.contractID.IsNull
				.Or<MatchUserFor<PMProject>>>
			.View DailyFieldReport;
		
		public PXSetup<ProjectManagementSetup> PJSetup;

		[PXCopyPasteHiddenView]
		[PXViewName(PX.Objects.EP.Messages.Approval)]
		public EPApprovalAutomation<DailyFieldReport, DailyFieldReport.approved, DailyFieldReport.rejected, DailyFieldReport.hold, PJSetupDailyFieldReportApproval> Approval;

		[InjectDependency]
		public IProjectDataProvider ProjectDataProvider
		{
			get;
			set;
		}

		public DailyFieldReportEntry()
		{
			var pjsetup = PJSetup.Current;
		}

		#region Actions

		public PXAction<DailyFieldReport> Print;
		[PXButton(CommitChanges = true)]
		[PXUIField(DisplayName = "Print Daily Field Report", MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select)]
		public virtual void print()
		{
			Persist();
			var parameters = new Dictionary<string, string>
			{
				[DailyFieldReportConstants.Print.DailyFieldReportId] = DailyFieldReport.Current.DailyFieldReportCd
			};
            throw new PXReportRequiredException(parameters, ScreenIds.DailyFieldReportForm, null);
		}

		public PXAction<DailyFieldReport> ViewAddressOnMap;
		[PXUIField(DisplayName = PX.Objects.CR.Messages.ViewOnMap)]
		[PXButton]
		public virtual void viewAddressOnMap()
		{
			var dailyFieldReport = DailyFieldReport.Current;
			new MapService(this).viewAddressOnMap(dailyFieldReport);
		}

		public delegate IEnumerable CompleteDelegate(PXAdapter adapter);

		public PXAction<DailyFieldReport> complete;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Complete")]
		protected virtual IEnumerable Complete(PXAdapter adapter)
		{
			OnDailyFieldReportCompleting();
			return adapter.Get();
		}

		// TODO Do we need this?
		protected virtual void OnDailyFieldReportCompleting() {}

		public PXAction<DailyFieldReport> hold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold")]
		protected virtual IEnumerable Hold(PXAdapter adapter) => adapter.Get();

		#endregion

		public override void CopyPasteGetScript(bool isImportSimple, List<Command> script, List<Container> containers)
		{
			var dailyFieldReportCopyConfigurationService = new DailyFieldReportCopyConfigurationService(this);
			dailyFieldReportCopyConfigurationService.ConfigureCopyPasteFields(script, containers);
		}

		public virtual void _(Events.RowPersisting<DailyFieldReport> args)
		{
			var dailyFieldReport = args.Row;
			var project = ProjectDataProvider.GetProject(this, dailyFieldReport.ProjectId);
			if (dailyFieldReport.Date < project?.StartDate)
			{
				args.Cache.RaiseException<DailyFieldReport.date>(dailyFieldReport,
					DailyFieldReportMessages.DfrDateMustNotBeEarlierThenProjectStartDate);
			}
		}

		public virtual void _(Events.RowInserting<DailyFieldReport> args)
		{
			PXContext.SetScreenID(ScreenIds.DailyFieldReport);
		}

		public virtual void _(Events.FieldUpdated<DailyFieldReport, DailyFieldReport.projectId> args)
		{
			var dailyFieldReport = args.Row;
			if (dailyFieldReport.ProjectId != null)
			{
				var project = ProjectDataProvider.GetProject(this, dailyFieldReport.ProjectId);
				var projectExt = PXCache<PMProject>.GetExtension<PMProjectExt>(project);
				PMSiteAddress address = PXSelect<PMSiteAddress, Where<PMSiteAddress.addressID, Equal<Required<PMProjectExt.siteAddressID>>>>.Select(this, projectExt.SiteAddressID);
				dailyFieldReport.SiteAddress = address.AddressLine1;
				dailyFieldReport.City = address.City;
				dailyFieldReport.CountryID = address.CountryID;
				dailyFieldReport.State = address.State;
				dailyFieldReport.PostalCode = address.PostalCode;
				dailyFieldReport.Latitude = address.Latitude;
				dailyFieldReport.Longitude = address.Longitude;
			}
		}

		public virtual void _(Events.FieldUpdated<DailyFieldReport.countryId> args)
		{
			DailyFieldReport.Current.State = null;
		}

		protected virtual void _(Events.FieldUpdated<EPActivityApprove.ownerID> e)
		{
			e.Cache.SetDefaultExt<EPActivityApprove.unionID>(e.Row);
			e.Cache.SetDefaultExt<EPActivityApprove.certifiedJob>(e.Row);
			e.Cache.SetDefaultExt<EPActivityApprove.labourItemID>(e.Row);
		}

		protected virtual void _(Events.RowInserted<EPActivityApprove> e)
		{
			if (this.IsCopyPasteContext == true)
			{
				if (e.Cache.GetEnabled<EPActivityApprove.unionID>(e.Row) == false)
				{
					e.Cache.SetDefaultExt<EPActivityApprove.unionID>(e.Row);
				}
				if (e.Cache.GetEnabled<EPActivityApprove.certifiedJob>(e.Row) == false)
				{
					e.Cache.SetDefaultExt<EPActivityApprove.certifiedJob>(e.Row);
				}
				if (e.Cache.GetEnabled<EPActivityApprove.labourItemID>(e.Row) == false)
				{
					e.Cache.SetDefaultExt<EPActivityApprove.labourItemID>(e.Row);
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<EPApproval, EPApproval.descr> e)
		{
			if (DailyFieldReport.Current != null)
			{
				PMProject project = PMProject.PK.Find(this, DailyFieldReport.Current.ProjectId);
				if (project != null)
				{
					e.NewValue = string.Format("Daily Field Report for {0} - {1}", project.ContractCD.Trim(), project.Description);
				}
			}
		}
	}
}
