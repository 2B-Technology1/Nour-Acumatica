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
using PX.Objects.AR;
using PX.Objects.CN.Common.Descriptor.Attributes;
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
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.Descriptor;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Objects.PJ.RequestsForInformation.PJ.Graphs;
using PX.Objects.PM;
using PX.TM;

namespace PX.Objects.PJ.RequestsForInformation.PJ.DAC
{
	/// <summary>
	/// Contains the main properties of a request for information.
	/// Requests for information (RFIs) are used for obtaining information
	/// that is not present in (or cannot be inferred from) the contract documents.
	/// The records of this type are created and edited through the Request for Information (PJ301000) form
	/// (which corresponds to the <see cref="RequestForInformationMaint"/> graph).
	/// </summary>
    [Serializable]
    [PXPrimaryGraph(typeof(RequestForInformationMaint))]
    [PXCacheName(CacheNames.RequestForInformation)]
    [PXEMailSource]
    public class RequestForInformation : ProjectManagementImpact, IAssign, IPXSelectable, IBqlTable, IProjectManagementDocumentBase
    {
        #region Keys

        /// <summary>
        /// Primary Key
        /// </summary>
        public class PK : PrimaryKeyOf<RequestForInformation>.By<requestForInformationCd>
        {
            public static RequestForInformation Find(PXGraph graph, string requestForInformationCd, PKFindOptions options = PKFindOptions.None) => FindBy(graph, requestForInformationCd, options);
        }

        /// <summary>
        /// Foreign Keys
        /// </summary>
        public static class FK
        {
            /// <summary>
            /// Project
            /// </summary>
            public class Project : PMProject.PK.ForeignKeyOf<RequestForInformation>.By<projectId> { }

            /// <summary>
            /// Project Task
            /// </summary>
            public class ProjectTask : PMTask.PK.ForeignKeyOf<RequestForInformation>.By<projectId, projectTaskId> { }

            /// <summary>
            /// Owner
            /// </summary>
            public class OwnerContact : PX.Objects.CR.Contact.PK.ForeignKeyOf<RequestForInformation>.By<ownerID> { }

            /// <summary>
            /// Business Account
            /// </summary>
            public class BusinessAccount : BAccountR.PK.ForeignKeyOf<RequestForInformation>.By<businessAccountId> { }

            /// <summary>
            /// Contact
            /// </summary>
            public class Contact : PX.Objects.CR.Contact.PK.ForeignKeyOf<RequestForInformation>.By<contactID> { }
        }
        #endregion
        #region Events
        public class Events : PXEntityEvent<RequestForInformation>.Container<Events>
        {
            public PXEntityEvent<RequestForInformation> ConvertToChangeRequest;
            public PXEntityEvent<RequestForInformation> DeleteChangeRequest;
        }
		#endregion

		/// <summary>
		/// The unique identifier of the request for information in the database.
		/// </summary>
		[PXDBIdentity]
        [PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible,
            DisplayName = RequestForInformationLabels.RequestForInformationNumberId)]
        [CascadeDelete(typeof(RequestForInformationDrawingLog), typeof(RequestForInformationDrawingLog.requestForInformationId))]
        public virtual int? RequestForInformationId
        {
            get;
            set;
        }

		/// <summary>
		/// The unique identifier of the request for information displayed on the form.
		/// </summary>
		[PXDefault]
        [PXFieldDescription]
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = RequestForInformationLabels.RequestForInformationNumberId,
            Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        [AutoNumber(typeof(ProjectManagementSetup.requestForInformationNumberingId), typeof(AccessInfo.businessDate))]
		[PXSelector(
			typeof(SelectFrom<RequestForInformation>
				.LeftJoin<Contact>.On<Contact.contactID.IsEqual<contactID>>
				.InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<projectId>>
				.Where<MatchUserFor<PMProject>>
				.SearchFor<requestForInformationCd>),
			typeof(requestForInformationCd),
			typeof(projectId),
			typeof(projectTaskId),
			typeof(businessAccountId),
			typeof(Contact.displayName),
			typeof(classId),
			typeof(summary),
			typeof(status),
			typeof(reason),
			typeof(ownerID),
			Filterable = true,
			Headers = new[]
			{
				RequestForInformationLabels.RequestForInformationNumberId,
				RequestForInformationLabels.ProjectId,
				RequestForInformationLabels.ProjectTask,
				RequestForInformationLabels.BusinessAccount,
				RequestForInformationLabels.ContactId,
				RequestForInformationLabels.ClassId,
				RequestForInformationLabels.Summary,
				RequestForInformationLabels.Status,
				RequestForInformationLabels.Reason,
				RequestForInformationLabels.Owner
			})]
		public virtual string RequestForInformationCd
        {
            get;
            set;
        }

		/// <summary>
		/// A brief description of the request for information.
		/// </summary>
		[PXDefault]
        [PXFieldDescription]
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = RequestForInformationLabels.Summary, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Summary
        {
            get;
            set;
        }

		/// <summary>
		/// The identifier of the <see cref="RequestForInformation">incoming request for information</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="RequestForInformation.RequestForInformationId"/> field.
		/// </value>
		[PXDBInt]
        [PXUIField(DisplayName = "Link to Incoming RFI")]
        [PXSelector(typeof(Search<requestForInformationId>), SubstituteKey = typeof(requestForInformationCd))]
        public virtual int? IncomingRequestForInformationId
        {
            get;
            set;
        }

		/// <summary>
		/// A character number of the request for information.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "RFI Number.")]
        public virtual string RequestForInformationNumber
        {
            get;
            set;
        }

		/// <summary>
		/// The <see cref="ProjectManagementClass">project management class</see> that the request for information belongs to.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="ProjectManagementClass.ProjectManagementClassId"/> field.
		/// </value>
		[PXDefault]
        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = RequestForInformationLabels.ClassId)]
        [ResetResponseDueDate(typeof(dueResponseDate))]
        [PXSelector(typeof(Search<ProjectManagementClass.projectManagementClassId,
                Where<ProjectManagementClass.useForRequestForInformation, Equal<True>>>),
            typeof(ProjectManagementClass.projectManagementClassId),
            DescriptionField = typeof(ProjectManagementClass.description))]
        [ClassChangeConfirmation(
            Message = RequestForInformationMessages.ClassChangeWillRemoveAttributes,
            ClassIdField = typeof(classId),
            ViewName = nameof(RequestForInformation))]
        [ClassPriorityDefaulting(nameof(PriorityId))]
        public virtual string ClassId
        {
            get;
            set;
        }

		/// <summary>
		/// The <see cref="PMProject">project</see> related to the request for information.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID"/> field.
		/// </value>
		[PXDefault]
        [Project(typeof(Where<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<CTPRType.project>>>),
            DisplayName = RequestForInformationLabels.ProjectId)]
        public virtual int? ProjectId
        {
            get;
            set;
        }

		/// <summary>
		/// The <see cref="PMTask">project task</see> related to the request for information.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.TaskID"/> field.
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
		/// The <see cref="PX.Objects.EP.EPEmployee">Employee</see> assigned to the request for information.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="PX.Objects.CR.BAccount.BAccountID"/> field.
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
		/// The <see cref="PX.TM.EPCompanyTree">workgroup</see> to work on the request for information.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="PX.TM.EPCompanyTree.WorkGroupID">EPCompanyTree.WorkGroupID</see> field.
		/// </value>
		[PXDBInt]
        [PXCompanyTreeSelector]
        [PXMassUpdatableField]
        [PXUIField(DisplayName = "Workgroup")]
        public virtual int? WorkgroupID
        {
            get;
            set;
        }

		/// <summary>
		/// The question related to the request for information.
		/// </summary>
		[PXDefault]
        [PXDBText(IsUnicode = true)]
        [PXUIField(DisplayName = RequestForInformationLabels.RequestDetails)]
        public virtual string RequestDetails
        {
            get;
            set;
        }

		/// <summary>
		/// The answer to the <see cref="RequestDetails">question</see>.
		/// </summary>
		[PXDBText(IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = RequestForInformationLabels.RequestAnswer)]
        public virtual string RequestAnswer
        {
            get;
            set;
        }

		/// <summary>
		/// The date on which the request for information was last modified.
		/// </summary>
		[PXDBDate(PreserveTime = false, DisplayMask = "d")]
        [PXUIField(DisplayName = "Last Modified Date", Enabled = false)]
        public virtual DateTime? LastModifiedRequestAnswer
        {
            get;
            set;
        }

		/// <summary>
		/// A set of attributes related to the request for information.
		/// </summary>
		[CRAttributesField(typeof(classId), typeof(noteID))]
        public virtual string[] Attributes
        {
            get;
            set;
        }

		/// <summary>
		/// The status of the request for information.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="RequestForInformationStatusAttribute"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
        [RequestForInformationStatus]
        [PXDefault(RequestForInformationStatusAttribute.NewStatus)]
        [PXUIField(DisplayName = RequestForInformationLabels.Status, Required = true)]
        public virtual string Status
        {
            get;
            set;
        }

		/// <summary>
		/// The major status of the request for information (legacy).
		/// </summary>
		[PXDBString(1, IsFixed = true)]
        public virtual string MajorStatus
        {
            get;
            set;
        }

		/// <summary>
		/// The customer or vendor account related to the request for information.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="PX.Objects.CR.BAccount.BAccountID"/> field.
		/// </value>
		[BAccount(DisplayName = RequestForInformationLabels.BusinessAccount, Filterable = true)]
		[PXRestrictor(typeof(Where<BAccount.type, In3<BAccountType.vendorType,
                    BAccountType.customerType, BAccountType.combinedType>>),
            RequestForInformationMessages.BusinessAccountRestrictionType, typeof(BAccount.type))]
        [PXRestrictor(typeof(Where<BAccount.status, NotEqual<CustomerStatus.inactive>>),
            RequestForInformationMessages.BusinessAccountRestrictionStatus)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? BusinessAccountId
        {
            get;
            set;
        }

		/// <summary>
		/// A <see cref="Contact">contact</see> on the customer side.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="Contact.ContactID"/> field.
		/// </value>
		[PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = RequestForInformationLabels.ContactId, Visibility = PXUIVisibility.Visible,
            Required = true)]
        [DependsOnField(typeof(businessAccountId), ShouldDisable = false)]
        [PXSelector(typeof(Search<Contact.contactID,
                Where<Contact.contactType,
                    In3<ContactTypesAttribute.person, ContactTypesAttribute.employee>,
                    And2<Where<Contact.bAccountID, Equal<Current<businessAccountId>>,
                            Or<Current<businessAccountId>, IsNull>>,
                    And<Contact.isActive, Equal<True>>>>>),
            DescriptionField = typeof(Contact.displayName),
            Filterable = true,
            DirtyRead = true)]
        [PXRestrictor(typeof(Where<Contact.isActive, Equal<True>>),
            RequestForInformationMessages.OnlyActiveContactsAreAllowed)]
        public virtual int? ContactId
        {
            get;
            set;
        }

		/// <summary>
		/// A Boolean value that indicates (if the value is <see langword="true" />) that the project design has been affected by the request for information.
		/// </summary>
		[PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Design Change", Visibility = PXUIVisibility.Visible)]
        public virtual bool? DesignChange
        {
            get;
            set;
        }

		/// <summary>
		/// A Boolean value that indicates (if the value is <see langword="true" />) that the request for information is incoming.
		/// </summary>
		[PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Incoming", Visibility = PXUIVisibility.Visible)]
        public virtual bool? Incoming
        {
            get;
            set;
        }

		/// <summary>
		/// The link to the external documentation.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Documentation Link")]
        public virtual string DocumentationLink
        {
            get;
            set;
        }

		/// <summary>
		/// The reference number of the section in the specification related to the request for information.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Specification Section")]
        public virtual string SpecSection
        {
            get;
            set;
        }

		/// <summary>
		/// The date and time when the request for information has been created.
		/// </summary>
		[PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Creation Date")]
        public DateTime? CreationDate
        {
            get;
            set;
        }

		/// <summary>
		/// The user name of the employee that has created the request for information.
		/// </summary>
		[PXDBCreatedByID]
        [PXUIField(DisplayName = "Created By", Required = true)]
        public override Guid? CreatedById
        {
            get;
            set;
        }

		/// <summary>
		/// The priority of the request for information.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="ProjectManagementClassPriority.PriorityId"/> field.
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
		/// The reference number of the change request or outgoing request for information created for the request for information.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="Note.NoteID"/> field.
		/// </value>
		[LinkRefNote(typeof(PMChangeRequest.refNbr))]
        [PXUIField(DisplayName = "Converted To", Enabled = false)]
        public virtual Guid? ConvertedTo
        {
            get;
            set;
        }

		/// <summary>
		/// The reference number of the document from which the request for information has been created.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="Note.NoteID"/> field.
		/// </value>
		[LinkRefNote(typeof(ProjectIssue.projectIssueCd))]
        [PXUIField(DisplayName = "Converted From", Enabled = false)]
        public virtual Guid? ConvertedFrom
        {
            get;
            set;
        }

		/// <summary>
		/// The reason of the request of information.
		/// </summary>
		[PXDBString(50)]
        [RequestForInformationReason]
        [PXDefault(RequestForInformationReasonAttribute.Unassigned)]
        [PXUIField(DisplayName = RequestForInformationLabels.Reason)]
        public virtual string Reason
        {
            get;
            set;
        }

		/// <summary>
		/// The date on which the answer for the request for information has to be received.
		/// </summary>
		[PXDBDate(PreserveTime = false, InputMask = "d")]
        [PXUIField(DisplayName = "Answer Due Date")]
        public virtual DateTime? DueResponseDate
        {
            get;
            set;
        }

		#region system fields
		[PXNote(ShowInReferenceSelector = true,
            Selector = typeof(Search<requestForInformationCd>),
            FieldList = new[]
            {
                typeof(requestForInformationCd),
                typeof(summary),
                typeof(status)
            })]
        [RequestForInformationSearchable]
        public override Guid? NoteID
        {
            get;
            set;
        }

        [PXString]
        [PXFormula(typeof(IIf<Where<isScheduleImpact, Equal<True>>,
            RequestForInformationMessages.NotificationTemplate.yesWithComma,
            RequestForInformationMessages.NotificationTemplate.no>))]
        public string IsScheduleImpactFormatted
        {
            get;
            set;
        }

        [PXString]
        [PXFormula(typeof(IIf<Where<isCostImpact, Equal<True>>,
            RequestForInformationMessages.NotificationTemplate.yesWithComma,
            RequestForInformationMessages.NotificationTemplate.no>))]
        public string IsCostImpactFormatted
        {
            get;
            set;
        }

        [PXString]
        [PXFormula(typeof(IIf<Where<designChange, Equal<True>>,
            RequestForInformationMessages.NotificationTemplate.yes,
            RequestForInformationMessages.NotificationTemplate.no>))]
        public string DesignChangeFormatted
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
		#endregion

		public abstract class businessAccountId : BqlInt.Field<businessAccountId>
        {
        }

        public abstract class priorityId : BqlInt.Field<priorityId>
        {
        }

        public abstract class convertedTo : BqlGuid.Field<convertedTo>
        {
        }

        public abstract class dueResponseDate : BqlDateTime.Field<dueResponseDate>
        {
        }

        public abstract class convertedFrom : BqlGuid.Field<convertedFrom>
        {
        }

        public abstract class reason : BqlString.Field<reason>
        {
        }

        public abstract class specSection : BqlString.Field<specSection>
        {
        }

        public abstract class incomingRequestForInformationId : BqlInt.Field<incomingRequestForInformationId>
        {
        }

        public abstract class documentationLink : BqlString.Field<documentationLink>
        {
        }

        public abstract class requestAnswer : BqlString.Field<requestAnswer>
        {
        }

        public abstract class designChange : BqlBool.Field<designChange>
        {
        }

        public abstract class contactID : BqlInt.Field<contactID>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }

        public abstract class incoming : BqlBool.Field<incoming>
        {
        }

        public abstract class status : BqlString.Field<status>
        {
        }

        public abstract class majorStatus : BqlString.Field<majorStatus>
        {
        }

        public abstract class noteID : BqlGuid.Field<noteID>
        {
        }

        public abstract class attributes : BqlAttributes.Field<attributes>
        {
        }

        public abstract class requestDetails : BqlString.Field<requestDetails>
        {
        }

        public abstract class lastModifiedRequestAnswer : BqlDateTime.Field<lastModifiedRequestAnswer>
        {
        }

        public abstract class summary : BqlString.Field<summary>
        {
        }

        public abstract class requestForInformationId : BqlInt.Field<requestForInformationId>
        {
        }

        public abstract class requestForInformationCd : BqlString.Field<requestForInformationCd>
        {
        }

        public abstract class classId : BqlString.Field<classId>
        {
        }

        public abstract class requestForInformationNumber : BqlString.Field<requestForInformationNumber>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class projectTaskId : BqlInt.Field<projectTaskId>
        {
        }

        public abstract class ownerID : BqlInt.Field<ownerID>
        {
        }

        public abstract class workgroupID : BqlInt.Field<workgroupID>
        {
        }

        public abstract class creationDate : BqlDateTime.Field<creationDate>
        {
        }

        public new abstract class isScheduleImpact : BqlBool.Field<isScheduleImpact>
        {
        }

        public new abstract class scheduleImpact : BqlInt.Field<scheduleImpact>
        {
        }

        public new abstract class isCostImpact : BqlBool.Field<isCostImpact>
        {
        }

        public new abstract class costImpact : BqlDecimal.Field<costImpact>
        {
        }

        public new abstract class createdById : BqlGuid.Field<createdById>
        {
        }

        public class typeName : BqlString.Constant<typeName>
        {
            public typeName()
                : base(typeof(RequestForInformation).FullName)
            {
            }
        }
    }
}
