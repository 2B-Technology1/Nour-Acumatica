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

using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.PJ.PhotoLogs.PJ.Services
{
    public class PhotoLogEmailActivityService : EmailActivityService<PhotoLog>
    {
        private readonly FileInfo photoLogZip;

        public PhotoLogEmailActivityService(PXGraph graph, FileInfo photoLogZip)
            : base(graph, graph.Accessinfo.ContactID)
        {
            Entity = graph.Caches<PhotoLog>().Current as PhotoLog;
            this.photoLogZip = photoLogZip;
        }

        public PXGraph GetEmailActivityGraph()
        {
            var emailActivityGraph = CreateEmailActivityGraph();
            var cache = emailActivityGraph.Caches<CRSMEmail>();
            AttachPhotoLogZip(cache);
            return emailActivityGraph;
        }

        protected override string GetSubject()
        {
            return photoLogZip.Name.Replace(".zip", string.Empty);
        }

        private void AttachPhotoLogZip(PXCache cache)
        {
            var uploadFileMaintenance = PXGraph.CreateInstance<UploadFileMaintenance>();
            SetZipFileFullName(cache);
            uploadFileMaintenance.SaveFile(photoLogZip);
            PXNoteAttribute.SetFileNotes(cache, cache.Current, photoLogZip.UID?.SingleToArray());
        }

        private void SetZipFileFullName(PXCache cache)
        {
            var email = cache.Current as CRSMEmail;
            photoLogZip.FullName = $"{email?.NoteID}\\{photoLogZip.Name}";
        }
    }
}