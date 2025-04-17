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
using PX.Objects.CT;

namespace PX.Objects.PM
{
	/// <summary>
	/// This is a virtual DAC. The fields/selector of this dac is used in reports.
	/// </summary>
	[PXHidden]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMReportMetadata : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		//Note: This field is used by Reports (Selector in parameters).
		[PXDefault]
		[Project(WarnIfCompleted = false)]
		[PXRestrictor(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>, And<PMProject.nonProject, NotEqual<True>>>), Messages.TemplateContract, typeof(PMProject.contractCD))]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion

		#region ActiveProjectID
		public abstract class activeProjectID : PX.Data.BQL.BqlInt.Field<activeProjectID> { }
		//Note: This field is used by Reports (Selector in parameters).
		[PXDefault]
		[Project(typeof(Where<PMProject.nonProject.IsEqual<False>
				.And<PMProject.baseType.IsEqual<CTPRType.project>>
				.And<Brackets<PMProject.status.IsEqual<ProjectStatus.active>
					.Or<PMProject.status.IsEqual<ProjectStatus.suspended>
					.Or<PMProject.status.IsEqual<ProjectStatus.completed>>>>>>),
			WarnIfCompleted = false)]
		[PXRestrictor(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>, And<PMProject.nonProject, NotEqual<True>>>), Messages.TemplateContract, typeof(PMProject.contractCD))]
		public virtual Int32? ActiveProjectID { get; set; }
		#endregion

		#region ActiveOrCompletedProjectID
		public abstract class activeOrCompletedProjectID : PX.Data.BQL.BqlInt.Field<activeOrCompletedProjectID> { }
		//Note: This field is used by Reports (Selector in parameters).
		[PXDefault]
		[Project(typeof(Where<PMProject.nonProject.IsEqual<False>
				.And<PMProject.baseType.IsEqual<CTPRType.project>>
				.And<Brackets<PMProject.status.IsEqual<ProjectStatus.active>
					.Or<PMProject.status.IsEqual<ProjectStatus.completed>>>>>),
			WarnIfCompleted = false)]
		[PXRestrictor(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>, And<PMProject.nonProject, NotEqual<True>>>), Messages.TemplateContract, typeof(PMProject.contractCD))]
		public virtual Int32? ActiveOrCompletedProjectID { get; set; }
		#endregion

		#region ProjectIDForProgressWorksheetReport
		public abstract class projectIDForProgressWorksheetReport : PX.Data.BQL.BqlInt.Field<projectIDForProgressWorksheetReport> { }

		/// <summary>
		/// The identifier of the <see cref="PMProject">project</see> associated with the progress worksheet report.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID" /> field.
		/// </value>
		[PXRestrictor(typeof(Where<PMProject.status, NotEqual<ProjectStatus.planned>, And<PMProject.nonProject, Equal<False>>>), PM.Messages.PWProjectIsNotActive, typeof(PMProject.contractCD))]
		[PXDefault]
		[Project(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>, And<PMProject.status, NotEqual<ProjectStatus.planned>, And<PMProject.nonProject, Equal<False>>>>), Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? ProjectIDForProgressWorksheetReport
		{
			get;
			set;
		}
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		protected Int32? _ProjectTaskID;
		//Note: This field is used by Reports (Selector in parameters).
		[BaseProjectTask(typeof(PMReportMetadata.projectID))]
		public virtual Int32? ProjectTaskID
		{
			get
			{
				return this._ProjectTaskID;
			}
			set
			{
				this._ProjectTaskID = value;
			}
		}
		#endregion

		#region ValidTaskID

		public abstract class validTaskID : BqlInt.Field<validTaskID> {}

		/// <summary>
		/// Field schema (in PXSelector) for Task field in reports (e.g. Project Budget Forecast by Month).
		/// Shows only tasks for project with ID <see cref="projectID">projectID</see>.
		/// Row-level security is taken into account, i.e. tasks from inaccessible projects are not shown.
		/// </summary>
		[PXInt]
		[PXSelector(
			typeof(SelectFrom<PMTask>
				.InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<PMTask.projectID>>
				.Where<PMTask.projectID.IsEqual<projectID.AsOptional>
					.And<MatchUserFor<PMProject>>>
				.SearchFor<PMTask.taskID>),
			typeof(PMTask.taskCD),
			typeof(PMTask.description),
			typeof(PMTask.status),
			SubstituteKey = typeof(PMTask.taskCD),
			DescriptionField = typeof(PMTask.description))]
		public virtual int? ValidTaskID { get; set; }

		#endregion
	}
}
