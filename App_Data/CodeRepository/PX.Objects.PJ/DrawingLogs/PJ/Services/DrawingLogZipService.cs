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
using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.CacheExtensions;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.SM;

namespace PX.Objects.PJ.DrawingLogs.PJ.Services
{
    public class DrawingLogZipService : BaseZipService<DrawingLog>
    {
        private readonly bool? shouldUseOnlyCurrentFiles;

        public DrawingLogZipService(PXCache cache, bool? shouldUseOnlyCurrentFiles)
            : base(cache)
        {
            this.shouldUseOnlyCurrentFiles = shouldUseOnlyCurrentFiles;
        }

        protected override string ZipFileName => Constants.DrawingLogsZipFileName;

        protected override string NoDocumentsSelectedMessage => DrawingLogMessages.NoRecordsWereSelected;

        protected override string NoAttachedFilesInDocumentMessage => DrawingLogMessages.NoAttachedFilesAreAvailableForDrawingLog;

        protected override string NoAttachedFilesInDocumentsMessage => DrawingLogMessages.NoAttachedFilesAreAvailableForSelectedDrawingLogs;

        protected override IEnumerable<Guid> GetFileIdsOfDocument(DrawingLog document)
        {
            var fileIds = PXNoteAttribute.GetFileNotes(Cache, document).ToList();
            return shouldUseOnlyCurrentFiles == true
                ? GetFileIdsWithCurrentValue(fileIds)
                : fileIds;
        }

        private IEnumerable<Guid> GetFileIdsWithCurrentValue(IEnumerable<Guid> fileIds)
        {
            return SelectFrom<UploadFileRevision>
                .Where<UploadFileRevision.fileID.IsIn<P.AsGuid>
                    .And<UploadFileRevisionExt.isDrawingLogCurrentFile.IsEqual<True>>>.View
                .Select(Cache.Graph, fileIds.ToArray()).FirstTableItems
                .Select(revision => revision.FileID.GetValueOrDefault());
        }
    }
}
