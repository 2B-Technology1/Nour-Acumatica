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
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.RequestsForInformation.CR.CacheExtensions;
using PX.Objects.PJ.Submittals.PJ.Descriptor;
using PX.Data;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.PJ.Submittals.PJ.Graphs;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.PJ.Submittals.PJ.Services
{
    public class SubmittalEmailAttachService : EmailFileAttachService<PJSubmittal>
    {
        public SubmittalEmailAttachService(CREmailActivityMaint graph) :
            base(graph, new SubmittalsReportGenerator(graph.Message.Current, graph.Message.Cache))
        {
        }

        public override void FillRequiredFields(NoteDoc file)
        {
            base.FillRequiredFields(file);

            var fileExtension = file.GetExtension<NoteDocRequestForInformationExt>();
            fileExtension.FileSource = EmailActivityFileSource.Submittal;
        }

        #region ReportGenerator
        public class SubmittalsReportGenerator : ReportGeneratorBase<PJSubmittal>
        {
            public SubmittalsReportGenerator(CRSMEmail email, PXCache emailCache)
                : base(email, emailCache)
            {
            }

            protected override string ReportScreenId { get => ScreenIds.SubmittalReport; }

            protected override Dictionary<string, string> GetParameters(PJSubmittal document)
            {
                var parameters = SubmittalEntry.GetReportParameters(document);
                parameters[SubmittalConstants.ReportParameters.EmailActivityNoteId] = Email.NoteID?.ToString("D");
                return parameters;
            }

            protected override string GenerateReportName(PJSubmittal document)
            {
                var emailLastModified = Email.EmailLastModifiedDateTime.GetValueOrDefault();
                return string.Format(SubmittalMessage.SubmittleReportNamePattern,
                    document.SubmittalID, document.RevisionID, emailLastModified.ToString("MM.dd.yy H:mm:ss"));
            }
        }
        #endregion ReportGenerator
    }
}
