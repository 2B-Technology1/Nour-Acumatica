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
using PX.Objects.PJ.Common.Descriptor.Attributes;
using PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.TM;

namespace PX.Objects.PJ.DrawingLogs.PJ.DAC
{
    [Serializable]
    [PXCacheName(CacheNames.DrawingLogRelation)]
    public class DrawingLogRelation : IBqlTable
    {
        [PXGuid(IsKey = true)]
        [ViewDetailsRefNote(typeof(ProjectIssue), typeof(RequestForInformation))]
        public virtual Guid? DocumentId
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "Document ID", Enabled = false)]
        public virtual string DocumentCd
        {
            get;
            set;
        }

        [PXString]
        [DrawingLogRelationDocumentType]
        [PXUIField(DisplayName = "Document Type", Enabled = false)]
        public virtual string DocumentType
        {
            get;
            set;
        }

        [Project(typeof(Where<PMProject.nonProject, Equal<False>,
                And<PMProject.baseType, Equal<CTPRType.project>>>),
            DisplayName = "Project", Enabled = false)]
        public virtual int? ProjectId
        {
            get;
            set;
        }

        [PXInt]
        [PXSelector(typeof(Search<PMTask.taskID>),
            SubstituteKey = typeof(PMTask.taskCD))]
        [PXUIField(DisplayName = "Project Task", Enabled = false)]
        public virtual int? ProjectTaskId
        {
            get;
            set;
        }

        [PXString]
        [DrawingLogRelationStatus]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        public virtual string Status
        {
            get;
            set;
        }

        [PXInt]
        [ProjectManagementPrioritySelector]
        [PXUIField(DisplayName = "Priority", Enabled = false)]
        public virtual int? PriorityId
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string Summary
        {
            get;
            set;
        }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "Created By", Enabled = false)]
        public virtual Guid? CreatedById
        {
            get;
            set;
        }

        [Owner(Enabled = false, IsDBField = false)]
        public virtual int? OwnerId
        {
            get;
            set;
        }

        [PXDate(InputMask = "d")]
        [PXUIField(DisplayName = "Due Date", Enabled = false)]
        public virtual DateTime? DueDate
        {
            get;
            set;
        }

        public abstract class documentId : BqlGuid.Field<documentId>
        {
        }

        public abstract class documentCd : BqlString.Field<documentCd>
        {
        }

        public abstract class documentType : BqlString.Field<documentType>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class projectTaskId : BqlInt.Field<projectTaskId>
        {
        }

        public abstract class status : BqlString.Field<status>
        {
        }

        public abstract class priorityId : BqlInt.Field<projectId>
        {
        }

        public abstract class summary : BqlString.Field<summary>
        {
        }

        public abstract class createdById : BqlGuid.Field<createdById>
        {
        }

        public abstract class ownerId : BqlInt.Field<ownerId>
        {
        }

        public abstract class dueDate : BqlDateTime.Field<dueDate>
        {
        }
    }
}
