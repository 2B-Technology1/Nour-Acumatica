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
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.SM;

namespace PX.Objects.PJ.ProjectManagement.PJ.DAC
{
	/// <summary>
	/// Represents the tenant-level project management preferences record that contains the general settings
	/// for project issues, requests for information, daily field reports, and other aspects of project management.
	/// The single record of this type is created and edited on the Project Management Preferences (PJ101000) form 
	/// (which corresponds to the <see cref="ProjectManagementSetupMaint"/> graph).
	/// </summary> 
	[Serializable]
	[PXPrimaryGraph(typeof(ProjectManagementSetupMaint))]
	[PXCacheName(CacheNames.ProjectManagementPreferences)]
	public class ProjectManagementSetup : BaseCache, IBqlTable
	{
		/// <summary>
		/// The way of calculating the date in the <see cref="ProjectIssue.dueDate"/> and <see cref="RequestForInformation.dueResponseDate"/>.
		/// </summary>
		[PXDefault(AnswerDaysCalculationTypeAttribute.SequentialDays)]
		[PXDBString(1, IsUnicode = true)]
		[AnswerDaysCalculationType]
		[PXUIField(DisplayName = "Due Date Calculation Type", Required = true)]
		public virtual string AnswerDaysCalculationType
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="CSCalendar">calendar</see> that defines the working days.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="CSCalendar.calendarID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = ProjectManagementLabels.CalendarId, Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
		public virtual string CalendarId
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="Numbering">numbering sequence</see> that is used for requests for information.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Numbering.numberingID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("REQFORINFO")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "RFI Numbering Sequence")]
		public virtual string RequestForInformationNumberingId
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="Numbering">numbering sequence</see> that is used for project issue.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Numbering.numberingID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("PROISSUE")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Project Issue Numbering Sequence")]
		public virtual string ProjectIssueNumberingId
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="Notification">notification template</see> that is used for the emails related to requests for information.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Notification.notificationID"/> field.
		/// </value>
		[PXUIField(DisplayName = "Default Email Notification")]
		[PXDBInt]
		[PXSelector(typeof(Search<Notification.notificationID>), DescriptionField = typeof(Notification.name),
			SubstituteKey = typeof(Notification.name))]
		public virtual int? DefaultEmailNotification
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="EPAssignmentMap">assignment map</see> that is used to assign owners to project issues.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EPAssignmentMap.assignmentMapID"/> field.
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID,
			Where<EPAssignmentMap.entityType, Equal<ProjectIssueAssignmentMapType>>>))]
		[PXUIField(DisplayName = "Project Issue Assignment Map")]
		public virtual int? ProjectIssueAssignmentMapId
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="EPAssignmentMap">assignment map</see> that is used to assign owners to requests for information.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Numbering.numberingID"/> field.
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID,
			Where<EPAssignmentMap.entityType, Equal<RequestForInformationAssignmentMapType>>>))]
		[PXUIField(DisplayName = "RFI Assignment Map")]
		public virtual int? RequestForInformationAssignmentMapId
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="Numbering">numbering sequence</see> that is used for daily field reports.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Numbering.numberingID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("DFREPORT")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "DFR Numbering Sequence")]
		public virtual string DailyFieldReportNumberingId
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="EPAssignmentMap">approval map</see> of a daily field report.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EPAssignmentMap.assignmentMapID"/> field.
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(SearchFor<EPAssignmentMap.assignmentMapID>.
			Where<EPAssignmentMap.entityType.IsEqual<DailyFieldReportApprovalMapType>
				.And<EPAssignmentMap.mapType.IsNotEqual<EPMapType.assignment>>>))]
		[PXUIField(DisplayName = "DFR Approval Map")]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.approvalWorkflow>))]
		public virtual int? DailyFieldReportApprovalMapId
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="Notification">notification template</see> that is used to send email notifications to approvers of daily field reports.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Notification.notificationID"/> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "DFR Approval Notification")]
		[PXSelector(typeof(Search<Notification.notificationID>))]
		[PXDefault(typeof(SearchFor<Notification.notificationID>
				.Where<Notification.name.IsEqual<DailyFieldReportConstants.Notification.name>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.approvalWorkflow>))]
		public virtual int? PendingApprovalNotification
		{
			get;
			set;
		}

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the history tab, with the history of each action performed on a daily field report, is displayed on the Daily Field Report (PJ304000) form.
		/// </summary>
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Enable History Log")]
		public virtual bool? IsHistoryLogEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="Numbering">numbering sequence</see> that is used for submittals.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Numbering.numberingID"/> field.
		/// </value>
		[PXDefault("SUBMITTAL")]
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Submittal Numbering Sequence")]
		public virtual string SubmittalNumberingId
		{
			get;
			set;
		}

		/// <inheritdoc cref="RequestForInformationNumberingId"/>
		public abstract class requestForInformationNumberingId : BqlString.Field<requestForInformationNumberingId>
		{
		}

		/// <inheritdoc cref="DefaultEmailNotification"/>
		public abstract class defaultEmailNotification : BqlInt.Field<defaultEmailNotification>
		{
		}

		/// <inheritdoc cref="ProjectIssueNumberingId"/>
		public abstract class projectIssueNumberingId : BqlString.Field<projectIssueNumberingId>
		{
		}

		/// <inheritdoc cref="ProjectIssueAssignmentMapId"/>
		public abstract class projectIssueAssignmentMapId : BqlInt.Field<projectIssueAssignmentMapId>
		{
		}

		/// <inheritdoc cref="RequestForInformationAssignmentMapId"/>
		public abstract class requestForInformationAssignmentMapId : BqlInt.Field<requestForInformationAssignmentMapId>
		{
		}

		/// <inheritdoc cref="AnswerDaysCalculationType"/>
		public abstract class answerDaysCalculationType : BqlString.Field<answerDaysCalculationType>
		{
		}

		/// <inheritdoc cref="CalendarId"/>
		public abstract class calendarId : BqlString.Field<calendarId>
		{
		}

		/// <inheritdoc cref="DailyFieldReportNumberingId"/>
		public abstract class dailyFieldReportNumberingId : BqlString.Field<dailyFieldReportNumberingId>
		{
		}

		/// <inheritdoc cref="DailyFieldReportApprovalMapId"/>
		public abstract class dailyFieldReportApprovalMapId : BqlInt.Field<dailyFieldReportApprovalMapId>
		{
		}

		/// <inheritdoc cref="PendingApprovalNotification"/>
		public abstract class pendingApprovalNotification : BqlInt.Field<pendingApprovalNotification>
		{
		}

		/// <inheritdoc cref="IsHistoryLogEnabled"/>
		public abstract class isHistoryLogEnabled : BqlBool.Field<isHistoryLogEnabled>
		{
		}

		/// <inheritdoc cref="SubmittalNumberingId"/>
		public abstract class submittalNumberingId : BqlString.Field<submittalNumberingId>
		{
		}

		public class ProjectIssueAssignmentMapType : BqlString.Constant<ProjectIssueAssignmentMapType>
		{
			public ProjectIssueAssignmentMapType()
				: base(typeof(ProjectIssue).FullName)
			{
			}
		}

		public class RequestForInformationAssignmentMapType : BqlString.Constant<RequestForInformationAssignmentMapType>
		{
			public RequestForInformationAssignmentMapType()
				: base(typeof(RequestForInformation).FullName)
			{
			}
		}

		public class DailyFieldReportApprovalMapType : BqlString.Constant<DailyFieldReportApprovalMapType>
		{
			public DailyFieldReportApprovalMapType()
				: base(typeof(DailyFieldReport).FullName)
			{
			}
		}
	}

	[PXHidden]
	[PXBreakInheritance]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PJSetupDailyFieldReportApproval : ProjectManagementSetup, IAssignedMap
	{
		int? IAssignedMap.AssignmentMapID
		{
			get
			{
				return this.DailyFieldReportApprovalMapId;
			}
			set
			{
				this.DailyFieldReportApprovalMapId = value;
			}
		}

		int? IAssignedMap.AssignmentNotificationID
		{
			get
			{
				return this.DailyFieldReportApprovalMapId;
			}
			set
			{
				this.DailyFieldReportApprovalMapId = value;
			}
		}

		bool? IAssignedMap.IsActive
		{
			get
			{
				return this.DailyFieldReportApprovalMapId.HasValue;
			}
		}
	}
}
