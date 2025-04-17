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
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PM;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public class ProjectTaskSelectorProvider : DocumentSelectorProvider
    {
        public ProjectTaskSelectorProvider(PXGraph graph, string fieldName)
            : base(graph, fieldName)
        {
        }

        public override string DocumentType => RequestForInformationRelationTypeAttribute.ProjectTask;

        protected override Type SelectorType => typeof(PMTask);

		protected override Type SelectorQuery =>
			typeof(Select<PMTask,
				Where<PMTask.projectID, Equal<Current<RequestForInformation.projectId>>>>);

		protected override Type DescriptionFieldType => typeof(PMTask.description);

		protected override Type SubstituteKeyType => typeof(PMTask.taskCD);

		protected override Type[] SelectorFieldTypes =>
            new[]
            {
                typeof(PMTask.taskCD),
                typeof(PMTask.description),
                typeof(PMTask.customerID),
                typeof(PMTask.billingOption),
                typeof(PMTask.completedPctMethod),
                typeof(PMTask.status),
                typeof(PMTask.approverID)
            };

        public override void NavigateToDocument(Guid? noteId)
        {
            var projectTaskEntry = PXGraph.CreateInstance<ProjectTaskEntry>();
            projectTaskEntry.Task.Current = GetProjectTask(noteId);

            throw new PXRedirectRequiredException(projectTaskEntry, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private PMTask GetProjectTask(Guid? noteId)
        {
            var query = new PXSelect<PMTask,
                Where<PMTask.noteID, Equal<Required<PMTask.noteID>>>>(Graph);

            return query.SelectSingle(noteId);
        }
    }
}
