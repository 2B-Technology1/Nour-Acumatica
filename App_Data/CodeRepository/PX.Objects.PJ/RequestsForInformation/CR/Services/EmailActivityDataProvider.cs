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
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.SM;
using NoteDoc = PX.Data.NoteDoc;

namespace PX.Objects.PJ.RequestsForInformation.CR.Services
{
    public class EmailActivityDataProvider
    {
        private readonly PXGraph graph;

        public EmailActivityDataProvider(PXGraph graph)
        {
            this.graph = graph;
        }

        public IEnumerable<NoteDoc> GetFileNotesAttachedToEntity(Guid? relatedEntityNoteId)
        {
            return new PXSelect<NoteDoc,
                    Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>(graph)
                .Select(relatedEntityNoteId).FirstTableItems;
        }

        public NoteDoc GetFileReference(Guid? fileId, Guid? noteId)
        {
            return new PXSelect<NoteDoc,
                    Where<NoteDoc.fileID, Equal<Required<NoteDoc.fileID>>,
                        And<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>>(graph)
                .SelectSingle(fileId, noteId);
        }

        public UploadFile GetFile(Guid? fileId)
        {
            return new PXSelect<UploadFile,
                Where<UploadFile.fileID, Equal<Required<UploadFile.fileID>>>>(graph).SelectSingle(fileId);
        }

        public UploadFileRevision GetFileRevision(Guid? fileId)
        {
            return SelectFrom<UploadFileRevision>.Where<UploadFileRevision.fileID.IsEqual<P.AsGuid>>.View
                .Select(graph, fileId);
        }

        public IEnumerable<NoteDoc> GetFilesLinkedToEmail(Guid? noteId, IEnumerable<Guid> fileIds)
        {
            return new PXSelect<NoteDoc,
                    Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>,
                        And<NoteDoc.fileID, In<Required<NoteDoc.fileID>>>>>(graph)
                .Select(noteId, fileIds.ToArray()).FirstTableItems;
        }

        public Notification GetNotification(string name)
        {
            return new PXSelect<Notification,
                Where<Notification.name, Equal<Required<Notification.name>>>>(graph).SelectSingle(name);
        }
    }
}