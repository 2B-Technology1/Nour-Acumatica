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

using PX.Objects.PJ.RequestsForInformation.Descriptor;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.Common;
using System;
using System.Collections.Generic;
using PX.Objects.PJ.Common.DAC;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.PJ.Submittals.PJ.DAC
{
	/// <summary>
	/// Represents a submittal workflow record.
	/// The records of this type are created and edited through the <b>Submittal Workflow</b> tab of the
	/// Submittals (PJ306000) form (which corresponds to the <see cref="SubmittalEntry"/> graph).
	/// </summary>
	[Serializable]
	[PXCacheName("Submittal Workflow Item")]
	public class PJSubmittalWorkflowItem : PX.Data.IBqlTable
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<PJSubmittalWorkflowItem>.By<submittalID, revisionID, lineNbr>
		{
			public static PJSubmittalWorkflowItem Find(PXGraph graph, string submittalID, string revisionID, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, submittalID, revisionID, lineNbr, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Submittal
			/// </summary>
			public class Submittal : PJSubmittal.PK.ForeignKeyOf<PJSubmittalWorkflowItem>.By<submittalID, revisionID> { }

			/// <summary>
			/// Contact For Current Project
			/// </summary>
			public class Contact : ContactForCurrentProject.PK.ForeignKeyOf<PJSubmittalWorkflowItem>.By<contactID> { }
		}
		#endregion

		#region RefNbr
		/// <inheritdoc cref="SubmittalID"/>
		public abstract class submittalID : PX.Data.BQL.BqlString.Field<submittalID>
		{
		}

		/// <summary>
		/// The identifier of a workflow for processing of the submittal.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(PJSubmittal.submittalID))]
		[PXParent(typeof(Select<PJSubmittal,
			Where<PJSubmittal.submittalID, Equal<Current<PJSubmittalWorkflowItem.submittalID>>,
				And<PJSubmittal.revisionID, Equal<Current<PJSubmittalWorkflowItem.revisionID>>>>>))]
		public virtual string SubmittalID
		{
			get;
			set;
		}
		#endregion

		#region RevisionID
		/// <inheritdoc cref="RevisionID"/>
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID>
		{
		}

		/// <summary>
		/// The identifier of the <see cref="PJSubmittal">revision</see> of the submittal.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PJSubmittal.revisionID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(PJSubmittal.revisionID))]
		public virtual int? RevisionID
		{
			get;
			set;
		}
		#endregion

		#region LineNbr
		/// <inheritdoc cref="LineNbr"/>
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr>
		{
		}

		/// <summary>
		/// The identifier of the submittal workflow item.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PJSubmittal))]
		[PXUIField(DisplayName = "Line Nbr.")]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion

		#region ContactID
		/// <inheritdoc cref="ContactID"/>
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		/// <summary>
		/// The identifier of the <see cref="ContactForCurrentProject"> Contact.</see>
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="ContactForCurrentProject.contactID"/> field.
		/// </value>
		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.Visible,
			Required = true)]
		[PXSelector(typeof(Search<ContactForCurrentProject.contactID,
				Where<ContactForCurrentProject.contactType,
					In3<ContactTypesAttribute.person, ContactTypesAttribute.employee>>>),
			DescriptionField = typeof(ContactForCurrentProject.displayName),
			Filterable = true)]
		[PXRestrictor(typeof(Where<ContactForCurrentProject.isActive, Equal<True>>),
			RequestForInformationMessages.OnlyActiveContactsAreAllowed)]
		public virtual int? ContactID
		{
			get;
			set;
		}
		#endregion

		#region Role
		/// <inheritdoc cref="Role"/>
		public abstract class role : PX.Data.BQL.BqlString.Field<role>
		{
			public const string Submitter = "S";
			public const string Approver = "A";
			public const string Reviewer = "R";

			public class Labels : ILabelProvider
			{
				private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
				{
					{ Submitter, ProjectManagementMessages.SubmitterLabel },
					{ Approver, ProjectManagementMessages.ApproverLabel },
					{ Reviewer, ProjectManagementMessages.ReviewerLabel },
				};

				public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
			}
		}

		/// <summary>
		/// The role of a workflow for processing of the submittal.
		/// </summary>
		[PXDefault]
		[PXDBString(1, IsFixed = true)]
		[LabelList(typeof(role.Labels))]
		[PXUIField(DisplayName = "Role", Required = true)]
		public virtual string Role
		{
			get;
			set;
		}
		#endregion

		#region Status
		/// <inheritdoc cref="Status"/>
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public const string Planned = "Planned";
			public const string Pending = "Pending";
			public const string Completed = "Completed";
			public const string Canceled = "Canceled";
			public const string Approved = "Approved";
			public const string Rejected = "Rejected";

			public class FullLabels : ILabelProvider
			{
				private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
				{
					{ Planned, ProjectManagementMessages.Planned },
					{ Pending, ProjectManagementMessages.Pending },
					{ Completed, ProjectManagementMessages.Completed },
					{ Approved, ProjectManagementMessages.Approved },
					{ Rejected, ProjectManagementMessages.Rejected },
					{ Canceled, ProjectManagementMessages.Canceled }
				};

				public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
			}

			public class ApproverLabels : ILabelProvider
			{
				private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
				{
					{ Planned, ProjectManagementMessages.Planned },
					{ Pending, ProjectManagementMessages.Pending },
					{ Approved, ProjectManagementMessages.Approved },
					{ Rejected, ProjectManagementMessages.Rejected },
					{ Canceled, ProjectManagementMessages.Canceled }
				};

				public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
			}

			public class SubmitterReviewerLabels : ILabelProvider
			{
				private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
				{
					{ Planned, ProjectManagementMessages.Planned },
					{ Pending, ProjectManagementMessages.Pending },
					{ Completed, ProjectManagementMessages.Completed },
					{ Canceled, ProjectManagementMessages.Canceled }
				};

				public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
			}

		}

		/// <summary>
		/// The status of the workflow for processing of the submittal.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="status.FullLabels"/>.
		/// </value>
		[PXDBString(10)]
		[PXDefault(status.Planned)]
		[LabelList(typeof(status.FullLabels))]
		[PXUIField(DisplayName = "Status")]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion

		#region StartDate
		/// <inheritdoc cref="StartDate"/>
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		/// <summary>
		/// The start date of the workflow for processing of the submittal.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate
		{
			get;
			set;
		}
		#endregion

		#region DaysForReview
		/// <inheritdoc cref="DaysForReview"/>
		public abstract class daysForReview : PX.Data.BQL.BqlDateTime.Field<daysForReview> { }

		/// <summary>
		/// The number of days to review the workflow for processing of the submittal.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Days for Review")]
		public virtual int? DaysForReview
		{
			get;
			set;
		}
		#endregion

		#region DueDate
		/// <inheritdoc cref="DaysForReview"/>
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }

		/// <summary>
		/// The due date of the workflow for processing of the submittal.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Due Date")]
		public virtual DateTime? DueDate
		{
			get;
			set;
		}
		#endregion

		#region CompletionDate
		/// <inheritdoc cref="CompletionDate"/>
		public abstract class completionDate : PX.Data.BQL.BqlDateTime.Field<completionDate> { }

		/// <summary>
		/// The completion date of the workflow for processing of the submittal.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Completion Date")]
		public virtual DateTime? CompletionDate
		{
			get;
			set;
		}
		#endregion

		#region DateReceived
		/// <inheritdoc cref="DateReceived"/>
		public abstract class dateReceived : PX.Data.BQL.BqlDateTime.Field<dateReceived> { }

		/// <summary>
		/// The date when the workflow for processing of the submittal was received.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Date Received")]
		public virtual DateTime? DateReceived
		{
			get;
			set;
		}
		#endregion

		#region DateSent
		/// <inheritdoc cref="DateSent"/>
		public abstract class dateSent : PX.Data.BQL.BqlDateTime.Field<dateSent> { }

		/// <summary>
		/// The date when the workflow for processing of the submittal was sent.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Date Sent")]
		public virtual DateTime? DateSent
		{
			get;
			set;
		}
		#endregion

		#region CanDelete
		/// <inheritdoc cref="CanDelete"/>
		public abstract class canDelete : PX.Data.BQL.BqlBool.Field<canDelete> { }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that a user can delete the workflow for processing of the submittal.
		/// </summary>
		[PXBool]
		public virtual bool? CanDelete { get; set; }
		#endregion

		#region EmailTo
		/// <inheritdoc cref="EmailTo"/>
		public abstract class emailTo : PX.Data.BQL.BqlBool.Field<emailTo> { }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that a user can email the workflow for processing of the submittal.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField]
		public virtual bool? EmailTo { get; set; }
		#endregion

		#region System Columns

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}

		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion

		#endregion
	}
}
