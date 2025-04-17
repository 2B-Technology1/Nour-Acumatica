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

using PX.Objects.PJ.Common.Descriptor.Attributes;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CN.Common.DAC;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.TM;
using System;
using System.Collections.Generic;
using PX.Objects.PJ.Submittals.PJ.Descriptor;

namespace PX.Objects.PJ.Submittals.PJ.DAC
{
	/// <summary>
	/// Contains the main properties of a submittal.
	/// The records of this type are created and edited through the Submittals (PJ306000) form
	/// (which corresponds to the <see cref="SubmittalEntry"/> graph).
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(typeof(Graphs.SubmittalEntry))]
	[PXCacheName("Submittal")]
	public class PJSubmittal : BaseCache,
		IBqlTable,
		IProjectManagementDocumentBase,
		IAssign
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<PJSubmittal>.By<submittalID, revisionID>
		{
			public static PJSubmittal Find(PXGraph graph, string submittalID, string revisionID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, submittalID, revisionID, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Submittal Type
			/// </summary>
			public class SubmittalType : PJSubmittalType.PK.ForeignKeyOf<PJSubmittal>.By<typeID> { }

			/// <summary>
			/// Project
			/// </summary>
			public class Project : PMProject.PK.ForeignKeyOf<PJSubmittal>.By<projectId> { }

			/// <summary>
			/// Project Task
			/// </summary>
			public class ProjectTask : PMTask.PK.ForeignKeyOf<PJSubmittal>.By<projectId, projectTaskId> { }

			/// <summary>
			/// Cost Code
			/// </summary>
			public class CostCode : PMCostCode.PK.ForeignKeyOf<PJSubmittal>.By<costCodeID> { }

			/// <summary>
			/// Owner
			/// </summary>
			public class OwnerContact : Contact.PK.ForeignKeyOf<PJSubmittal>.By<ownerID> { }

			/// <summary>
			/// Ball In Court Contact
			/// </summary>
			public class CurrentWorkflowItemContact : Contact.PK.ForeignKeyOf<PJSubmittal>.By<currentWorkflowItemContactID> { }
		}
		#endregion

		#region SubmittalID
		/// <inheritdoc cref="SubmittalID"/>
		public abstract class submittalID : PX.Data.BQL.BqlString.Field<submittalID>
		{
		}

		/// <summary>
		/// The unique identifier of the submittal.
		/// </summary>
		[PXFieldDescription]
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[SubmittalIDOfLastRevisionSelector(ValidateValue = false)]
		[PXUIField(DisplayName = "Submittal ID", 
			Visibility = PXUIVisibility.SelectorVisible,
			Required = true)]
		[PXDefault]
		[SubmittalAutoNumber]
		public virtual string SubmittalID
		{
			get;
			set;
		}
		#endregion

		#region RevisionID
		/// <inheritdoc cref="RevisionID"/>
		public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

		/// <summary>
		/// The revision number of the submittal.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Revision ID", 
			Visibility = PXUIVisibility.SelectorVisible,
			Required = true)]
		[PXFieldDescription]
		[SubmittalRevisionIDSelector(typeof(submittalID))]
		public virtual int? RevisionID
		{
			get;
			set;
		}
		#endregion

		#region TypeID

		/// <inheritdoc cref="TypeID"/>
		public abstract class typeID : PX.Data.BQL.BqlInt.Field<typeID>
		{
		}

		/// <summary>
		/// The <see cref="PJSubmittalType">type</see> of the submittal. 
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PJSubmittalType.submittalTypeID"/> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Submittal Type")]
		[PXSelector(typeof(SearchFor<PJSubmittalType.submittalTypeID>),
			SubstituteKey = typeof(PJSubmittalType.typeName))]
		[PXForeignReference(typeof(Field<typeID>.IsRelatedTo<PJSubmittalType.submittalTypeID>))]
		public virtual int? TypeID
		{
			get;
			set;
		}
		#endregion

		#region Summary
		/// <inheritdoc cref="Summary"/>
		public abstract class summary : PX.Data.BQL.BqlString.Field<summary>
		{
		}

		/// <summary>
		/// The summary of the submittal.
		/// </summary>
		[PXFieldDescription]
		[PXDBString(255, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", 
			Visibility = PXUIVisibility.SelectorVisible,
			Required = true)]
		public virtual string Summary
		{
			get;
			set;
		}
		#endregion

		#region Status

		/// <inheritdoc cref="Status"/>
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public const string
				New = "N",
				Open = "O",
				Closed = "C";

			public class Labels : ILabelProvider
			{
				private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
				{
					{ New, ProjectManagementMessages.New },
					{ Open, ProjectManagementMessages.Open },
					{ Closed, ProjectManagementMessages.Closed },
				};

				public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
			}

			public class newStatus : PX.Data.BQL.BqlString.Constant<newStatus>
			{
				public newStatus() : base(New) {; }
			}

			public class open : PX.Data.BQL.BqlString.Constant<open>
			{
				public open() : base(Open) {; }
			}

			public class closed : PX.Data.BQL.BqlString.Constant<closed>
			{
				public closed() : base(Closed) {; }
			}
		}

		/// <summary>
		/// The status of the submittal.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="status.Labels"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(status.New)]
		[PXUIField(DisplayName = "Status", Required = true)]
		[LabelList(typeof(status.Labels))]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion

		#region Reason
		/// <inheritdoc cref="Reason"/>
		public abstract class reason : PX.Data.BQL.BqlString.Field<reason> 
		{
			public const string
				New = "New",
				Revision = "Revision",
				Issued = "Issued",
				Submitted = "Submitted",
				PendingApproval = "PendingApproval",
				Approved = "Approved",
				ApprovedAsNoted = "ApprovedAsNoted",
				Rejected = "Rejected",
				Canceled = "Canceled",
				ReviseAndResubmit = "ReviseAndResubmit";
		}

		/// <summary>
		/// The reason of the submittal.
		/// </summary>
		[PXDBString(20, IsUnicode = true)]
		[PXDefault(reason.New)]
		[PXStringList(new string[0], new string[0])]
		[PXUIField(DisplayName = "Reason", Required = false)]
		public virtual string Reason
		{
			get;
			set;
		}
		#endregion

		#region IsLastRevision
		/// <inheritdoc cref="IsLastRevision"/>
		public abstract class isLastRevision : PX.Data.BQL.BqlBool.Field<isLastRevision>
		{
		}

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that this record is the last revision for submittal.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		public virtual bool? IsLastRevision
		{
			get;
			set;
		}
		#endregion

		#region ProjectID
		/// <inheritdoc cref="ProjectId"/>
		public abstract class projectId : PX.Data.BQL.BqlInt.Field<projectId>
		{
		}

		/// <summary>
		/// The <see cref="PMProject">project</see> of the submittal.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.contractID"/> field.
		/// </value>
		[PXDefault]
		[PXUIField(DisplayName = "Project")]
		[ProjectBase]
		[PXRestrictor(typeof(Where<PMProject.nonProject, Equal<False>>), PM.Messages.NonProjectCodeIsInvalid)]
		[PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.ProjectIsCanceled)]
		public virtual int? ProjectId
		{
			get;
			set;
		}
		#endregion

		#region ProjectTaskID
		/// <inheritdoc cref="ProjectTaskId"/>
		public abstract class projectTaskId : PX.Data.BQL.BqlInt.Field<projectTaskId>
		{
		}

		/// <summary>
		/// The <see cref="PMTask">project task</see> of the submittal.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.taskID"/> field.
		/// </value>
		[PXUIField(DisplayName = "Project Task")]
		[ProjectTask(typeof(projectId), AllowNullIfContract = true, AllowNull = true)]
		public virtual int? ProjectTaskId
		{
			get;
			set;
		}
		#endregion

		#region CostCodeID
		/// <inheritdoc cref="CostCodeID"/>
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}

		/// <summary>
		/// The <see cref="PMCostCode">cost code</see> of the submittal.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMCostCode.costCodeID"/> field.
		/// </value>
		[CostCode(null, 
			typeof(projectTaskId), 
			DisplayName = "Cost Code",
			AllowNullValue = true)]
		public virtual int? CostCodeID
		{
			get;
			set;
		}
		#endregion

		#region SpecificationInfo
		/// <inheritdoc cref="SpecificationInfo"/>
		public abstract class specificationInfo : PX.Data.BQL.BqlString.Field<specificationInfo>
		{
		}

		/// <summary>
		/// The specification information of the submittal.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Specification")]
		public virtual string SpecificationInfo
		{
			get;
			set;
		}
		#endregion

		#region SpecificationSection
		/// <inheritdoc cref="SpecificationSection"/>
		public abstract class specificationSection : PX.Data.BQL.BqlString.Field<specificationSection> { }

		/// <summary>
		/// The specification section of the submittal.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Specification Section")]
		public virtual string SpecificationSection
		{
			get;
			set;
		}
		#endregion

		#region DateOnSite
		/// <inheritdoc cref="DateOnSite"/>
		public abstract class dateOnSite : PX.Data.BQL.BqlDateTime.Field<dateOnSite> { }

		/// <summary>
		/// The date by which the documents or materials related to the submittal have to be at the construction site.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Date Required on Site")]
		public virtual DateTime? DateOnSite
		{
			get;
			set;
		}
		#endregion

		#region DateCreated
		/// <inheritdoc cref="DateCreated"/>
		public abstract class dateCreated : PX.Data.BQL.BqlDateTime.Field<dateCreated> { }

		/// <summary>
		/// The date when the submittal was created.
		/// </summary>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date Created")]
		public virtual DateTime? DateCreated
		{
			get;
			set;
		}
		#endregion

		#region DueDate
		/// <inheritdoc cref="DueDate"/>
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }

		/// <summary>
		/// The due date of the submittal.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Due Date")]
		public virtual DateTime? DueDate
		{
			get;
			set;
		}
		#endregion

		#region DateClosed
		/// <inheritdoc cref="DateClosed"/>
		public abstract class dateClosed : PX.Data.BQL.BqlDateTime.Field<dateClosed> { }

		/// <summary>
		/// The date when the submittal was closed.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Date Closed", Enabled = false)]
		public virtual DateTime? DateClosed
		{
			get;
			set;
		}
		#endregion

		#region DaysOverdue
		/// <inheritdoc cref="DaysOverdue"/>
		public abstract class daysOverdue : PX.Data.BQL.BqlInt.Field<daysOverdue> { }

		/// <summary>
		/// The quantity of days for which the submittal was overdue.
		/// </summary>
		[PXInt]
		[PXUIField(DisplayName = "Days Overdue", Enabled = false)]
		[DaysOverdue(typeof(dueDate), typeof(dateClosed))]
		public virtual int? DaysOverdue
		{
			[PXDependsOnFields(typeof(dueDate), typeof(dateClosed))]
			get;
			set;
		}
		#endregion


		#region OwnerID
		/// <inheritdoc cref="OwnerID"/>
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		/// <summary>
		/// The <see cref="Contact">owner</see> of the submittal.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Contact.contactID"/> field.
		/// </value>
		[Owner]
		[PXDefault(typeof(AccessInfo.contactID))]
		public virtual int? OwnerID
		{
			get;
			set;
		}
		#endregion

		#region WorkgroupID
		/// <inheritdoc cref="WorkgroupID"/>
		public abstract class workgroupID : PX.Data.BQL.BqlGuid.Field<workgroupID> { }

		/// <summary>
		/// The workgroup of the submittal.
		/// </summary>
		[PXInt]
		public virtual int? WorkgroupID
		{
			get;
			set;
		}
		#endregion

		#region CurrentWorkflowItemContact
		/// <inheritdoc cref="CurrentWorkflowItemContactID"/>
		public abstract class currentWorkflowItemContactID : PX.Data.BQL.BqlInt.Field<currentWorkflowItemContactID> { }

		/// <summary>
		/// The identifier of the <see cref="Contact"> contact</see> of the current workflow.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Contact.contactID"/> field.
		/// </value>
		[PXDBInt]
		[PXDefault]
		[PXSelector(typeof(Search<Contact.contactID>), DescriptionField = typeof(Contact.displayName))]
		[PXUIField(DisplayName = SubmittalMessage.BallInCourt, Enabled = false)]
		public virtual int? CurrentWorkflowItemContactID
		{
			get;
			set;
		}
		#endregion

		#region CurrentWorkflowItemLineNbr

		/// <inheritdoc cref="CurrentWorkflowItemLineNbr"/>
		public abstract class currentWorkflowItemLineNbr : PX.Data.BQL.BqlInt.Field<currentWorkflowItemLineNbr> { }

		/// <summary>
		/// The line number of current workflow of submittal.
		/// </summary>
		[PXDBInt]
		public virtual int? CurrentWorkflowItemLineNbr
		{
			get;
			set;
		}
		#endregion

		#region Description
		/// <inheritdoc cref="Description"/>
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>.
		/// The detailed description of the submittal.
		/// </summary>
		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Description
		{
			get;
			set;
		}
		#endregion

		#region FormCaptionDescription
		[PXString]
		[PXFormula(typeof(Selector<projectId, PMProject.description>))]
		public string FormCaptionDescription { get; set; }
		#endregion

		#region System Columns

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[SubmittalSearchable]
		[PXNote(
			DescriptionField = typeof(submittalID),
			ShowInReferenceSelector = true,
			Selector = typeof(Search<submittalID>),
			FieldList = new[]
			{
				typeof(submittalID),
				typeof(summary),
				typeof(status)
			})]
		public override Guid? NoteID
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }

		[PXDBTimestamp()]
		public override Byte[] Tstamp
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdById : PX.Data.BQL.BqlGuid.Field<createdById> { }

		[PXDBCreatedByID]
		public override Guid? CreatedById
		{
			get;
			set;
		}
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenId : PX.Data.BQL.BqlString.Field<createdByScreenId> { }

		[PXDBCreatedByScreenID()]
		public override String CreatedByScreenId
		{
			get;
			set;
		}
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public override DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedById : PX.Data.BQL.BqlGuid.Field<lastModifiedById> { }

		[PXDBLastModifiedByID]
		public override Guid? LastModifiedById
		{
			get;
			set;
		}
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenId : PX.Data.BQL.BqlString.Field<lastModifiedByScreenId> { }

		[PXDBLastModifiedByScreenID()]
		public override String LastModifiedByScreenId
		{
			get;
			set;
		}

		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public override DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion

		#endregion
	}
}
