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
using System.Linq;
using System.Collections.Generic;

using PX.Objects.PJ.Common.Services;
using PX.Common;
using PX.Data;
using PX.Reports;
using PX.Reports.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.SM;
using PX.Objects.Common.Tools;

namespace PX.Objects.PJ.Common.Services
{
    public class EmailFileAttachService<TEntity> : BaseEmailFileAttachService<TEntity>
        where TEntity : class, IBqlTable, new()
    {
        protected ReportGeneratorBase<TEntity> ReportGenerator { get; }

        public EmailFileAttachService(CREmailActivityMaint graph, ReportGeneratorBase<TEntity> reportGenerator) 
            : base(graph)
        {
            ReportGenerator = reportGenerator;
        }

        protected override IEnumerable<NoteDoc> GetFilesLinkedToRelatedEntities()
        {
            return EmailActivityDataProvider.GetFileNotesAttachedToEntity(Graph.Message.Current.RefNoteID);
        }

        public virtual FileInfo GenerateAndAttachReport(TEntity document, UploadFileMaintenance uploadFileMaintenance)
        {
            var file = ReportGenerator.GenerateReportFile(document, Graph.ReportLoader, Graph.ReportDataBinder);
            uploadFileMaintenance.SaveFile(file);

            // attach generated report to Email
            var fileId = file.UID.GetValueOrDefault();
            PXNoteAttribute.SetFileNotes(Graph.Message.Cache, Graph.Message.Current, fileId.SingleToArray());

            return file;
        }

        #region ReportGenerator
        public abstract class ReportGeneratorBase<T>
        {
            public CRSMEmail Email { get; }
            public PXCache EmailCache { get; }

            public ReportGeneratorBase(CRSMEmail email, PXCache emailCache)
            {
                Email = email;
                EmailCache = emailCache;
            }

            public FileInfo GenerateReportFile(T document, IReportLoaderService reportLoader, IReportDataBinder reportDataBinder)
            {
				reportLoader.ThrowOnNull(nameof(reportLoader));
                reportDataBinder.ThrowOnNull(nameof(reportDataBinder));

				var parameters = GetParameters(document);
                var fileName = GenerateReportName(document);
                var report = reportLoader.LoadReport(ReportScreenId, incoming: null);

                reportLoader.InitDefaultReportParameters(report, parameters);

                var reportNode = reportDataBinder.ProcessReportDataBinding(report);
                var data = PX.Reports.Mail.Message.GenerateReport(reportNode, RenderType.FilterPdf).First();
                var file = new FileInfo(fileName, null, data);

                return file;
            }

            protected abstract string ReportScreenId { get; }


            protected abstract Dictionary<string, string> GetParameters(T document);

            protected abstract string GenerateReportName(T document);
        }
        #endregion ReportGenerator
    }
}
