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

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.WorkflowAPI;
using PX.Objects.CN.Common.Descriptor.Attributes;
using PX.Objects.CN.Common.Utilities;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes.ProjectTaskWithType;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.Common.Descriptor.Attributes;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectsIssue.Descriptor;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectsIssue.PJ.Graphs;
using PX.Objects.PJ.ProjectsIssue.PJ.Utilities;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PM;
using PX.TM;

using Constants = PX.Objects.PJ.Common.Descriptor.Constants;

namespace PX.Objects.PJ.ProjectsIssue.PJ.DAC
{
	/// <summary>
	/// Contains the main properties of a project issue.
	/// Project issues are used for tracking and reporting on all activities
	/// related to various issues discovered on a construction site.
	/// The records of this type are created and edited through the Project Issue (PJ302000) form
	/// (which corresponds to the <see cref="ProjectIssueMaint"/> graph).
	/// </summary>
    [Serializable]
    [PXEMailSource]
    [PXPrimaryGraph(typeof(ProjectIssueMaint))]
    [PXCacheName(CacheNames.ProjectIssue)]
    public class ProjectIssue : ProjectManagementImpact, IBqlTable, IAssign, IPXSelectable, IProjectManagementDocumentBase
    {
        #region Keys

        /// <summary>
        /// Primary Key
        /// </summary>
        public class PK : PrimaryKeyOf<ProjectIssue>.By<projectIssueCd>
        {
            public static ProjectIssue Find(PXGraph graph, string projectIssueCd, PKFindOptions options = PKFindOptions.None) => FindBy(graph, projectIssueCd, options);
        }

        /// <summary>
        /// Foreign Keys
        /// </summary>
        public static class FK
        {
            /// <summary>
            /// Project
            /// </summary>
            public class Project : PMProject.PK.ForeignKeyOf<ProjectIssue>.By<projectId> { }

            /// <summary>
            /// Project Task
            /// </summary>
            public class ProjectTask : PMTask.PK.ForeignKeyOf<ProjectIssue>.By<projectId, projectTaskId> { }

            /// <summary>
            /// Owner
            /// </summary>
            public class OwnerContact : Contact.PK.ForeignKeyOf<ProjectIssue>.By<ownerID> { }
        }
        #endregion
        #region Events
        public class Events : PXEntityEvent<ProjectIssue>.Container<Events>
        {
            public PXEntityEvent<ProjectIssue> ConvertToChangeRequest;
            public PXEntityEvent<ProjectIssue> ConvertToRfi;
            public PXEntityEvent<ProjectIssue> Open;
        }
		#endregion

		/// <summary>
		/// The unique identifier of the project issue in the database.
		/// </summary>
		[PXDBIdentity]
        [PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible,
            DisplayName = "Project Issue ID")]
        [CascadeDelete(typeof(ProjectIssueDrawingLog), typeof(ProjectIssueDrawingLog.projectIssueId))]
        public virtual int? ProjectIssueId
        {
            get;
            set;
        }

		/// <summary>
		/// The project issue ID displayed on the form.
		/// </summary>
		[PXDefault]
        [PXFieldDescription]
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = "Project Issue ID", Required = true,
            Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(
			typeof(SelectFrom<ProjectIssue>
				.InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<projectId>>
				.Where<MatchUserFor<PMProject>>
				.SearchFor<projectIssueCd>),
			typeof(projectIssueCd),
			typeof(projectId),
			typeof(projectTaskId),
			typeof(classId),
			typeof(summary),
			typeof(status),
			typeof(ownerID),
			Filterable = true)]
		[AutoNumber(typeof(ProjectManagementSetup.projectIssueNumberingId), typeof(AccessInfo.businessDate))]
        public virtual string ProjectIssueCd
        {
            get;
            set;
        }

		/// <summary>
		/// The summary of the project issue.
		/// </summary>
		[PXDefault]
        [PXFieldDescription]
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Summary
        {
            get;
            set;
        }

		/// <summary>
		/// The identifier of the <see cref="PMProject">project</see> that is related to this project issue.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.contractID"/> field.
		/// </value>
		[PXDefault]
        [Project(typeof(Where<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<CTPRType.project>>>),
            DisplayName = "Project")]
        public virtual int? ProjectId
        {
            get;
            set;
        }

		/// <summary>
		/// The identifier of the <see cref="PMTask">project task</see> that is related to this project issue.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.taskID"/> field.
		/// </value>
		[ProjectTaskWithType(typeof(projectId), AlwaysEnabled = true, AllowNull = true,
            DisplayName = "Project Task")]
        [PXFormula(typeof(Validate<projectId>))]
        [ProjectTaskTypeValidation(
			ProjectIdField = typeof(projectId),
			ProjectTaskIdField = typeof(projectTaskId),
            Message = ProjectAccountingMessages.CostTaskTypeIsNotValid,
            WrongProjectTaskType = ProjectTaskType.Revenue)]
		[PXForeignReference(typeof(CompositeKey<Field<projectId>.IsRelatedTo<PMTask.projectID>, Field<projectTaskId>.IsRelatedTo<PMTask.taskID>>))]
		public virtual int? ProjectTaskId
        {
            get;
            set;
        }

		/// <summary>
		/// The <see cref="ProjectManagementClass">project management class</see> of the project issue.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="ProjectManagementClass.ProjectManagementClassId"/> field.
		/// </value>
		[PXDefault]
        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = "Class ID")]
        [ResetResponseDueDate(typeof(dueDate))]
        [PXSelector(typeof(Search<ProjectManagementClass.projectManagementClassId,
                Where<ProjectManagementClass.useForProjectIssue, Equal<True>>>),
            typeof(ProjectManagementClass.projectManagementClassId),
            DescriptionField = typeof(ProjectManagementClass.description))]
        [ClassChangeConfirmation(
            Message = ProjectIssueMessages.WarningRemovingProjectIssueAttributes,
            ClassIdField = typeof(classId),
            ViewName = nameof(ProjectIssue))]
        [ClassPriorityDefaulting(nameof(PriorityId))]
        public virtual string ClassId
        {
            get;
            set;
        }

		/// <summary>
		/// The <see cref="Contact">employee</see> assigned to the project issue.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Contact.contactID"/> field.
		/// </value>
		[Owner(typeof(workgroupID))]
        [PXMassUpdatableField]
        [PXFormula(typeof(Default<workgroupID>))]
        [PXDefault(typeof(Coalesce<
            Search<EPCompanyTreeMember.contactID,
                Where<EPCompanyTreeMember.workGroupID, Equal<Current<workgroupID>>,
                    And<EPCompanyTreeMember.contactID, Equal<Current<AccessInfo.contactID>>>>>,
            Search<CREmployee.defContactID,
                Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
                    And<Current<workgroupID>, IsNull>>>>))]
        public virtual int? OwnerID
        {
            get;
            set;
        }

		/// <summary>
		/// The identifier of the <see cref="EPCompanyTree">project workgroup</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EPCompanyTree.WorkGroupID"/> field.
		/// </value>
		[PXDBInt]
        [PXUIField(DisplayName = "Workgroup")]
        [PXCompanyTreeSelector]
        [PXMassUpdatableField]
        public virtual int? WorkgroupID
        {
            get;
            set;
        }

		/// <summary>
		/// The details of the project issue. 
		/// </summary>
		[PXDBText(IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Description
        {
            get;
            set;
        }

		/// <summary>
		/// The <see cref="ProjectManagementClassPriority">priority</see> of the project issue.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="ProjectManagementClassPriority.PriorityId"/> field.
		/// </value>
		[PXDBInt]
        [ProjectManagementPrioritySelector(typeof(classId))]
        [PXUIField(DisplayName = "Priority")]
        public virtual int? PriorityId
        {
            get;
            set;
        }

		/// <summary>
		/// The status of the project issue.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
        [ProjectIssueStatus]
        [PXDefault(ProjectIssueStatusAttribute.Open)]
        [PXUIField(DisplayName = "Status", Required = true)]
        public virtual string Status
        {
            get;
            set;
        }

		/// <summary>
		/// The major status of the project issue.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
        public virtual string MajorStatus
        {
            get;
            set;
        }

		/// <summary>
		/// The due date of the project issue.
		/// </summary>
		[PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXUIField(DisplayName = "Due Date")]
        public virtual DateTime? DueDate
        {
            get;
            set;
        }

		/// <summary>
		/// The resolved date of the project issue.
		/// </summary>
		[PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXUIField(DisplayName = "Resolved On")]
        public virtual DateTime? ResolvedOn
        {
            get;
            set;
        }

		/// <summary>
		/// The creation date of the project issue.
		/// </summary>
		[PXDBDateAndTime(DisplayNameDate = "Created On")]
        [DefaultWorkingTimeStart]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Created On", Required = false)]
        public DateTime? CreationDate
        {
            get;
            set;
        }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "Created By")]
        public override Guid? CreatedById
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? Selected
        {
            get;
            set;
        }

		/// <summary>
		/// The reference number of the request for information or change request created for the project issue.
		/// </summary>
		[LinkRefNote(typeof(RequestForInformation.requestForInformationCd), typeof(PMChangeRequest.refNbr))]
        [PXUIField(DisplayName = "Converted To")]
        public virtual Guid? ConvertedTo
        {
            get;
            set;
        }

        [PXNote(ShowInReferenceSelector = true,
            Selector = typeof(Search<projectIssueCd>),
            FieldList = new[]
            {
                typeof(projectIssueCd),
                typeof(summary),
                typeof(status)
            })]
        [ProjectIssueSearchable]
        public override Guid? NoteID
        {
            get;
            set;
        }

        [PXDBGuid(DatabaseFieldName = "RelatedEntityId")]
        public virtual Guid? RefNoteId
        {
            get;
            set;
        }

		/// <summary>
		/// The record or document related to the project issue.
		/// </summary>
		[PXString(IsUnicode = true)]
        [PXUIField(DisplayName = "Related Entity", Enabled = false)]
        [PXFormula(typeof(RelatedEntityDescription<refNoteId>))]
        public virtual string RelatedEntityDescription
        {
            get;
            set;
        }

		/// <summary>
		/// The priority icon of the project issue.
		/// </summary>
		[PXUIField(DisplayName = "Priority Icon", IsReadOnly = true)]
        [PXImage(HeaderImage = Constants.PriorityIconHeaderImage)]
        [PXFormula(typeof(PriorityIcon<priorityId>))]
        public virtual string PriorityIcon
        {
            get;
            set;
        }

		/// <summary>
		/// The <see cref="ProjectIssueType">type</see> of the project issue.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="ProjectIssueType.ProjectIssueTypeId"/> field.
		/// </value>
		[PXDBInt]
        [PXUIField(DisplayName = "Project Issue Type")]
        [PXSelector(typeof(SearchFor<ProjectIssueType.projectIssueTypeId>),
            SubstituteKey = typeof(ProjectIssueType.typeName))]
        public virtual int? ProjectIssueTypeId
        {
            get;
            set;
        }

        [CRAttributesField(typeof(classId), typeof(noteID))]
        public virtual string[] Attributes
        {
            get;
            set;
        }

        [PXUIField(DisplayName = "Last Modification Date")]
        [PXDBLastModifiedDateTime]
        public override DateTime? LastModifiedDateTime
        {
            get;
            set;
        }

        public abstract class refNoteId : BqlGuid.Field<refNoteId>
        {
        }

		/// <inheritdoc cref="Summary"/>
		public abstract class summary : BqlString.Field<summary>
        {
        }

		/// <inheritdoc cref="ProjectIssueCd"/>
		public abstract class projectIssueCd : BqlString.Field<projectIssueCd>
        {
        }

		/// <inheritdoc cref="ProjectIssueId"/>
		public abstract class projectIssueId : BqlInt.Field<projectIssueId>
        {
        }

		/// <inheritdoc cref="ProjectId"/>
		public abstract class projectId : BqlInt.Field<projectId>
        {
        }

		/// <inheritdoc cref="ProjectTaskId"/>
		public abstract class projectTaskId : BqlInt.Field<projectTaskId>
        {
        }

		/// <inheritdoc cref="ClassId"/>
		public abstract class classId : BqlString.Field<classId>
        {
        }

		public abstract class attributes : BqlAttributes.Field<attributes>
        {
        }

		/// <inheritdoc cref="DueDate"/>
		public abstract class dueDate : BqlDateTime.Field<dueDate>
        {
        }

		/// <inheritdoc cref="ResolvedOn"/>
		public abstract class resolvedOn : BqlDateTime.Field<resolvedOn>
        {
        }

		/// <inheritdoc cref="OwnerID"/>
		public abstract class ownerID : BqlInt.Field<ownerID>
        {
        }

		/// <inheritdoc cref="WorkgroupID"/>
		public abstract class workgroupID : BqlInt.Field<workgroupID>
        {
        }

		/// <inheritdoc cref="Description"/>
		public abstract class description : BqlString.Field<description>
        {
        }

		/// <inheritdoc cref="PriorityId"/>
		public abstract class priorityId : BqlInt.Field<priorityId>
        {
        }

		/// <inheritdoc cref="Status"/>
		public abstract class status : BqlString.Field<status>
        {
        }

		public abstract class noteID : BqlGuid.Field<noteID>
        {
        }

		/// <inheritdoc cref="CreationDate"/>
		public abstract class creationDate : BqlDateTime.Field<creationDate>
        {
        }

		/// <inheritdoc cref="PriorityIcon"/>
		public abstract class priorityIcon : BqlString.Field<priorityIcon>
        {
        }

		/// <inheritdoc cref="ConvertedTo"/>
		public abstract class convertedTo : BqlGuid.Field<convertedTo>
        {
        }

		/// <inheritdoc cref="ProjectIssueTypeId"/>
		public abstract class projectIssueTypeId : BqlInt.Field<projectIssueTypeId>
        {
        }

		/// <inheritdoc cref="IsScheduleImpact"/>
		public new abstract class isScheduleImpact : BqlBool.Field<isScheduleImpact>
        {
        }

		/// <inheritdoc cref="ScheduleImpact"/>
		public new abstract class scheduleImpact : BqlInt.Field<scheduleImpact>
        {
        }

		/// <inheritdoc cref="IsCostImpact"/>
		public new abstract class isCostImpact : BqlBool.Field<isCostImpact>
        {
        }

		/// <inheritdoc cref="CostImpact"/>
		public new abstract class costImpact : BqlDecimal.Field<costImpact>
        {
        }

        public new abstract class createdById : BqlGuid.Field<createdById>
        {
        }

        public class typeName : BqlString.Constant<typeName>
        {
            public typeName()
                : base(typeof(ProjectIssue).FullName)
            {
            }
        }
    }
}
