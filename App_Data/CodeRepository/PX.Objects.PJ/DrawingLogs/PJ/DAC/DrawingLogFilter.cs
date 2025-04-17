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
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes.ProjectTaskWithType;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;

namespace PX.Objects.PJ.DrawingLogs.PJ.DAC
{
    [Serializable]
    [PXCacheName("Drawing Log Filter")]
    public class DrawingLogFilter : IBqlTable
    {
        [Project(typeof(Where<PMProject.nonProject, Equal<False>,
                And<PMProject.baseType, Equal<CTPRType.project>>>),
            DisplayName = DrawingLogLabels.Project, WarnIfCompleted = false)]
        public virtual int? ProjectId
        {
            get;
            set;
        }

        [ActiveOrPlanningProjectTaskWithType(typeof(projectId), NeedsPrefilling = false,
            DisplayName = "Project Task", AlwaysEnabled = true)]
        public virtual int? ProjectTaskId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXSelector(typeof(Search<DrawingLogDiscipline.drawingLogDisciplineId,
                Where<DrawingLogDiscipline.isActive, Equal<True>>>),
            SubstituteKey = typeof(DrawingLogDiscipline.name))]
        [PXUIField(DisplayName = DrawingLogLabels.Discipline)]
        public virtual int? DisciplineId
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Current Only")]
        public virtual bool? IsCurrentOnly
        {
            get;
            set;
        }

		public abstract class statusID : BqlInt.Field<statusID> { }
		[PXInt]
		[PXSelector(typeof(DrawingLogStatus.statusId),
			SubstituteKey = typeof(DrawingLogStatus.name))]
		[PXUIField(DisplayName = DrawingLogLabels.Status, Visible = false)]
		public virtual int? StatusID { get; set; }

		public abstract class projectIssueID : BqlInt.Field<projectIssueID> { }

		/// <summary>
		/// The <see cref="ProjectIssue">project issue</see> for filtering drawing logs.
		/// </summary>
		[PXInt]
		[PXSelector(typeof(ProjectIssue.projectIssueId),
			SubstituteKey = typeof(ProjectIssue.projectIssueCd),
			DescriptionField = typeof(ProjectIssue.summary))]
		[PXUIField(DisplayName = "Project Issue", Visible = false)]
		public virtual int? ProjectIssueID { get; set; }

		public abstract class rfiID : BqlInt.Field<rfiID> { }

		/// <summary>
		/// The <see cref="RequestForInformation">request for information</see> for filtering drawing logs.
		/// </summary>
		[PXInt]
		[PXSelector(typeof(RequestForInformation.requestForInformationId),
			SubstituteKey = typeof(RequestForInformation.requestForInformationCd),
			DescriptionField = typeof(RequestForInformation.description))]
		[PXUIField(DisplayName = "Request For Information", Visible = false)]
		public virtual int? RFIID { get; set; }

		public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class projectTaskId : BqlInt.Field<projectTaskId>
        {
        }

        public abstract class disciplineId : BqlInt.Field<disciplineId>
        {
        }

        public abstract class isCurrentOnly : BqlBool.Field<isCurrentOnly>
        {
        }
    }
}
