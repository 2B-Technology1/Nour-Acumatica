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
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.WorkflowAPI;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.TM;

namespace PX.Objects.PM
{
	/// <summary>
	/// Contains the main properties of a progress worksheet. The records of this type are created and edited through the Progress Worksheets (PM303000) form (which corresponds to the
	/// <see cref="ProgressWorksheetEntry" /> graph).
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.ProgressWorksheet)]
	[PXPrimaryGraph(typeof(ProgressWorksheetEntry))]
	[Serializable]
	[PXEMailSource]
	public class PMProgressWorksheet : PX.Data.IBqlTable, IAssign
	{
		#region Events
		public class Events : PXEntityEvent<PMProgressWorksheet>.Container<Events>
		{
			public PXEntityEvent<PMProgressWorksheet> Release;
		}
		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
			public const int Length = 15;
		}

		/// <summary>
		/// The reference number of the progress worksheet.
		/// </summary>
		/// <value>
		/// The number is generated from the <see cref="Numbering">numbering sequence</see>,
		/// which is specified on the <see cref="PMSetup">Projects Preferences</see> (PM101000) form.
		/// </value>
		[PXDBString(refNbr.Length, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Worksheet Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(
			typeof(SelectFrom<PMProgressWorksheet>
				.InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<projectID>>
				.Where<hidden.IsNotEqual<True>.And<MatchUserFor<PMProject>>>
				.SearchFor<refNbr>),
			DescriptionField = typeof(description))]
		[ProgressWorksheetAutoNumber]
		public virtual string RefNbr { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <summary>
		/// The status of the progress worksheet.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"H"</c>: On Hold,
		/// <c>"A"</c>: Pending Approval,
		/// <c>"O"</c>: Open,
		/// <c>"C"</c>: Closed,
		/// <c>"R"</c>: Rejected
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[ProgressWorksheetStatus.List()]
		[PXDefault(ProgressWorksheetStatus.OnHold)]
		[PXUIField(DisplayName = "Status", Required = true, Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Status
		{
			get;
			set;
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the document is on hold.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold")]
		[PXDefault(true)]
		public virtual Boolean? Hold
		{
			get;
			set;
		}
		#endregion
		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the document is approved.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? Approved
		{
			get;
			set;
		}
		#endregion
		#region Rejected
		public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the document is rejected.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public bool? Rejected
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the document has been released.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Released")]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get;
			set;
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

		/// <summary>
		/// The date on which the progress worksheet was created.
		/// </summary>
		/// <value>
		/// By default, the value is set to the current <see cref="AccessInfo.BusinessDate">business date</see>.
		/// </value>
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? Date
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		/// <summary>
		/// The identifier of the <see cref="PMProject">project</see> associated with the progress worksheet.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID" /> field.
		/// </value>
		[PXRestrictor(typeof(Where<PMProject.status, Equal<ProjectStatus.active>, And<PMProject.nonProject, Equal<False>>>), PM.Messages.PWProjectIsNotActive, typeof(PMProject.contractCD))]
		[PXDefault]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		[Project(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>, And<PMProject.status, Equal<ProjectStatus.active>, And<PMProject.nonProject, Equal<False>>>>), Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>
		/// The description of the progress worksheet.
		/// </summary>
		[PXDBString(PX.Objects.Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

		/// <summary>
		/// A counter of the document lines, which is used internally to assign <see cref="PMProgressWorksheetLine.LineNbr">numbers</see> to new lines. We do not recommend that you
		/// rely on this field to determine the exact number of lines because it might not reflect this number under various conditions.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
		{
			get;
			set;
		}
		#endregion
		#region Hidden
		public abstract class hidden : PX.Data.BQL.BqlBool.Field<hidden> { }

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the document is hidden on the Progress Worksheets (PM303000) form.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? Hidden
		{
			get;
			set;
		}
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		/// <summary>The workgroup that is responsible for the document.</summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PX.TM.EPCompanyTree.WorkGroupID">EPCompanyTree.WorkGroupID</see> field.
		/// </value>
		[PXDBInt]
		[PXDefault(typeof(Customer.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup", Visibility = PXUIVisibility.Visible)]
		public virtual int? WorkgroupID
		{
			get;
			set;
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		/// <summary>The <see cref="Contact">contact</see> responsible for the document.</summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Contact.ContactID" /> field.
		/// </value>
		[PXDBInt]
		[PXDefault(typeof(Customer.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.Visible)]
		public virtual int? OwnerID
		{
			get;
			set;
		}
		#endregion

		#region Hidden RefNbr
		public abstract class hiddenRefNbr : PX.Data.BQL.BqlString.Field<hiddenRefNbr>
		{
			public const int Length = 15;
		}

		/// <summary>
		/// The reference number of the progress worksheet.
		/// </summary>
		/// <value>
		/// If the progress worksheet is unhidden, the value of this field contains the reference number. If the progress worksheet is hidden, the value is empty.
		/// </value>
		[PXString(refNbr.Length, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Worksheet Nbr.")]
		[PXFormula(typeof(Switch<Case<Where<hidden, Equal<True>>, Empty>, refNbr>))]
		public virtual String HiddenRefNbr
		{
			get;
			set;
		}
		#endregion
		#region Hidden Status
		public abstract class hiddenStatus : PX.Data.BQL.BqlString.Field<hiddenStatus> { }

		/// <summary>
		/// The status of the progress worksheet.
		/// </summary>
		/// <value>
		/// If the progress worksheet is unhidden, the value of this field contains the status of the progress worksheet. If the progress worksheet is hidden, the value is empty.
		/// </value>
		[PXString(1, IsFixed = true)]
		[ProgressWorksheetStatus.List()]
		[PXUIField(DisplayName = "Status")]
		[PXFormula(typeof(Switch<Case<Where<hidden, Equal<True>>, Empty>, status>))]
		public virtual String HiddenStatus
		{
			get;
			set;
		}
		#endregion

		#region DailyFieldReportCD
		public abstract class dailyFieldReportCD : PX.Data.BQL.BqlInt.Field<dailyFieldReportCD> { }

		/// <summary>
		/// The reference number of the linked daily field report.
		/// </summary>
		[PXString(10)]
		[PXUIField(DisplayName = "Daily Field Report", FieldClass = nameof(FeaturesSet.ConstructionProjectManagement))]
		public virtual string DailyFieldReportCD
		{
			get;
			set;
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[ProgressWorksheetSearchable]
		[PXNote(DescriptionField = typeof(PMProgressWorksheet.description))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion

	}
}
