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

using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.DrawingLogs.CR.CacheExtensions;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.PJ.DrawingLogs.PJ.Services
{
    public class DrawingLogEmailFileAttachService : BaseEmailFileAttachService<DrawingLog>
    {
        public DrawingLogEmailFileAttachService(CREmailActivityMaint graph)
            : base(graph)
        {
        }

        public override void FillRequiredFields(NoteDoc file)
        {
            base.FillRequiredFields(file);
            FillDrawingLogFieldsIfRequired(file);
        }

        protected override IEnumerable<NoteDoc> GetFilesLinkedToRelatedEntities()
        {
	        var drawingLog = GetParentEntity();
			var originalDrawingLogNoteId = drawingLog.OriginalDrawingId ?? drawingLog.NoteID;
            var originalDrawingLogWithRevisions =
                DrawingLogDataProvider.GetOriginalDrawingLogWithRevisions(originalDrawingLogNoteId);
            return originalDrawingLogWithRevisions.SelectMany(dl =>
                EmailActivityDataProvider.GetFileNotesAttachedToEntity(dl.NoteID));
        }

        private void FillDrawingLogFieldsIfRequired(NoteDoc file)
        {
            var relatedDrawingLog =
                DrawingLogDataProvider.GetDrawingLog(file.FileID);
            if (relatedDrawingLog != null)
            {
                var extension = PXCache<NoteDoc>.GetExtension<NoteDocDrawingLogExt>(file);
                extension.DrawingLogCd = relatedDrawingLog.DrawingLogCd;
                extension.Number = relatedDrawingLog.Number;
                extension.Revision = relatedDrawingLog.Revision;
            }
        }
    }
}