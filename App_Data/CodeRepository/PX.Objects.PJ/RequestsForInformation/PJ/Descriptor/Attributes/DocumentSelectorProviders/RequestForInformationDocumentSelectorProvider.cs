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
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Graphs;
using PX.Data;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public class RequestForInformationDocumentSelectorProvider : DocumentSelectorProvider
    {
        public RequestForInformationDocumentSelectorProvider(PXGraph graph, string fieldName)
            : base(graph, fieldName)
        {
        }

        public override string DocumentType => RequestForInformationRelationTypeAttribute.RequestForInformation;

        protected override Type SelectorType => typeof(RequestForInformation);

        protected override Type SelectorQuery => typeof(Select<RequestForInformation>);

		protected override Type DescriptionFieldType => typeof(RequestForInformation.summary);

		protected override Type SubstituteKeyType => typeof(RequestForInformation.requestForInformationCd);

		protected override Type[] SelectorFieldTypes =>
            new[]
            {
                typeof(RequestForInformation.requestForInformationCd),
                typeof(RequestForInformation.status),
                typeof(RequestForInformation.projectId),
                typeof(RequestForInformation.projectTaskId),
                typeof(RequestForInformation.summary),
                typeof(RequestForInformation.dueResponseDate),
                typeof(RequestForInformation.incoming),
                typeof(RequestForInformation.isScheduleImpact),
                typeof(RequestForInformation.isCostImpact)
            };

        public override void NavigateToDocument(Guid? noteId)
        {
            var requestForInformationMaint = PXGraph.CreateInstance<RequestForInformationMaint>();
            requestForInformationMaint.RequestForInformation.Current = GetRequestForInformation(noteId);

            throw new PXRedirectRequiredException(requestForInformationMaint, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private RequestForInformation GetRequestForInformation(Guid? noteId)
        {
            var query = new PXSelect<RequestForInformation,
                Where<RequestForInformation.noteID, Equal<Required<RequestForInformation.noteID>>>>(Graph);

            return query.SelectSingle(noteId);
        }
    }
}
