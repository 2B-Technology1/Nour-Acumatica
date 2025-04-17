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

using System.Collections;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.GraphExtensions;
using PX.Objects.PJ.RequestsForInformation.Descriptor;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Objects.PJ.RequestsForInformation.PJ.Extensions;
using PX.Objects.PJ.RequestsForInformation.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.PM;

namespace PX.Objects.PJ.RequestsForInformation.PJ.GraphExtensions
{
    public class RequestForInformationMaintDrawingLogsExtension :
        DrawingLogBaseExtension<RequestForInformationMaint, RequestForInformation, RequestForInformationDrawingLog>
    {
        protected override string ProjectChangeWarningMessage =>
            RequestForInformationMessages.UnlinkDrawingLogsOnProjectChange;

        public override void Initialize()
        {
            LinkDrawingLogToEntity.SetCaption(RequestForInformationLabels.LinkToRequestForInformation);
        }

        public override IEnumerable drawingLogReferences()
        {
            return SelectFrom<RequestForInformationDrawingLog>
                .Where<RequestForInformationDrawingLog.requestForInformationId
                    .IsEqual<RequestForInformation.requestForInformationId.FromCurrent>>.View.Select(Base);
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [ProjectWithoutVerification]
        [PXRemoveBaseAttribute(typeof(ProjectAttribute))]
        protected virtual void DrawingLog_ProjectId_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), Constants.AttributeProperties.Visible, false)]
        protected virtual void DrawingLog_OwnerId_CacheAttached(PXCache cache)
        {
        }

        protected override bool IsLinkDrawingActionEnabled(RequestForInformation requestForInformation)
        {
            return Base.IsRequestForInformationSaved(requestForInformation) &&
                   !requestForInformation.IsClosed();
        }

        protected override void SetReferenceEntityId(RequestForInformationDrawingLog reference)
        {
            reference.RequestForInformationId = Base.RequestForInformation.Current.RequestForInformationId;
        }

        protected override DrawingLogReferenceBase CreateDrawingLogReference(int? drawingLogId)
        {
            return new RequestForInformationDrawingLog
            {
                RequestForInformationId = Base.RequestForInformation.Current.RequestForInformationId,
                DrawingLogId = drawingLogId
            };
        }
    }
}