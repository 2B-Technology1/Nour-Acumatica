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
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public class ProjectSelectorProvider : DocumentSelectorProvider
    {
        public ProjectSelectorProvider(PXGraph graph, string fieldName)
            : base(graph, fieldName)
        {
        }

        public override string DocumentType => RequestForInformationRelationTypeAttribute.Project;

        protected override Type SelectorType => typeof(PMProject);

        protected override Type SelectorQuery =>
            typeof(Select<PMProject,
                Where<PMProject.nonProject, Equal<False>,
                    And<PMProject.baseType, Equal<CTPRType.project>>>>);

		protected override Type DescriptionFieldType => typeof(PMProject.description);

		protected override Type SubstituteKeyType => typeof(PMProject.contractCD);

		protected override Type[] SelectorFieldTypes =>
            new[]
            {
                typeof(PMProject.contractCD),
                typeof(PMProject.customerID),
                typeof(PMProject.description),
                typeof(PMProject.status)
            };

        public override void NavigateToDocument(Guid? noteId)
        {
            var projectEntry = PXGraph.CreateInstance<ProjectEntry>();
            projectEntry.Project.Current = GetProject(noteId);

            throw new PXRedirectRequiredException(projectEntry, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private PMProject GetProject(Guid? noteId)
        {
            var query = new PXSelect<PMProject,
                Where<PMProject.noteID, Equal<Required<PMProject.noteID>>>>(Graph);

            return query.SelectSingle(noteId);
        }
    }
}
