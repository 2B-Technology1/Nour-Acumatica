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

using System.Linq;
using System.Web;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.SM;
using Constants = PX.Objects.PJ.PhotoLogs.Descriptor.Constants;

namespace PX.Objects.PJ.PhotoLogs.PJ.Graphs
{
    public class PhotoEntry : PXGraph<PhotoEntry, Photo>
    {
        [PXCopyPasteHiddenFields(typeof(Photo.isMainPhoto))]
        public SelectFrom<Photo>.View Photos;

        public CRAttributeList<Photo> Attributes;

        public PXSetup<PhotoLogSetup> PhotoLogSetup;

        public PXAction<Photo> UploadPhoto;

        public PhotoEntry()
        {
	        PhotoLogSetup setup = PhotoLogSetup.Current;
        }

        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Change Photo")]
        public virtual void uploadPhoto()
        {
            if (Photos.AskExt() == WebDialogResult.OK)
            {
				DeleteAttachedPhoto();
				var fileInfo = PXContext.SessionTyped<PXSessionStatePXData>().FileInfo[Constants.UploadFileKey];
                HttpContext.Current.Session.Remove(Constants.UploadFileKey);
				if (fileInfo != null)
				{
					DeleteFile(fileInfo);
					fileInfo = AttachPhoto(fileInfo);
					FillPhotoFields(fileInfo);
				}
            }
        }

        public virtual void _(Events.RowSelected<Photo> args)
        {
            var photo = args.Row;
            if (photo == null)
            {
                return;
            }
            EnableFieldsInCopyPastContext(args.Cache, photo);
            if (IsCopyPasteContext && PXNoteAttribute.GetFileNotes(args.Cache, photo).IsEmpty() && photo.FileId != null)
            {
                PXNoteAttribute.SetFileNotes(args.Cache, photo, photo.FileId.Value);
            }
        }

        public virtual void _(Events.RowSelecting<Photo> args)
        {
            var photo = args.Row;
            if (photo != null)
            {
                photo.ImageUrl = photo.ImageUrl ?? string.Concat(PXUrl.SiteUrlWithPath(), Constants.FileUrl, photo.FileId);
            }
        }

        private FileInfo AttachPhoto(FileInfo fileInfo)
        {
            var graph = CreateInstance<UploadFileMaintenance>();
			var photoName = BuildPhotoName(Photos.Current, fileInfo.Name);
			var photo = new FileInfo(photoName, null, fileInfo.BinData);

            graph.SaveFile(photo);
            PXNoteAttribute.SetFileNotes(Photos.Cache, Photos.Current, photo.UID.GetValueOrDefault());

			return photo;
        }

		public static string BuildPhotoName(Photo photo, string fileName)
			=> $"{photo.NoteID}\\{fileName}";

		private void FillPhotoFields(FileInfo fileInfo)
        {
			FillPhotoFields(Photos.Current, fileInfo, Accessinfo);
		}

		public static void FillPhotoFields(Photo photo, FileInfo fileInfo, AccessInfo accessInfo)
		{
			photo.Name = fileInfo.Name;
			photo.FileId = fileInfo.UID;
			photo.ImageUrl = ComposeImageUrl(fileInfo);
			photo.UploadedDate = accessInfo.BusinessDate;
			photo.UploadedById = accessInfo.UserID;
		}

		private static string ComposeImageUrl(FileInfo fileInfo)
        {
            return string.Concat(PXUrl.SiteUrlWithPath(), Constants.FileUrl, fileInfo.UID.ToString());
        }

        private void DeleteAttachedPhoto()
        {
            var fileNote = PXNoteAttribute.GetFileNotes(Photos.Cache, Photos.Current).SingleOrDefault();
			if (fileNote != System.Guid.Empty)
			{
				UploadFileMaintenance.DeleteFile(fileNote);
				RemoveTempNoteDoc(fileNote);
			}
		}

		private void DeleteFile(FileInfo fileInfo)
		{
			UploadFileMaintenance.DeleteFile(fileInfo.UID);
			RemoveTempNoteDoc(fileInfo.UID);
		}

		private void RemoveTempNoteDoc(System.Guid? fileID)
		{
			foreach (NoteDoc noteDoc in Caches[typeof(NoteDoc)].Inserted)
			{
				if (noteDoc.FileID == fileID)
				{
					Caches[typeof(NoteDoc)].Delete(noteDoc);
				}
			}
		}

		private void EnableFieldsInCopyPastContext(PXCache cache, Photo photo)
        {
            Enable<Photo.name>(cache, photo);
            Enable<Photo.uploadedById>(cache, photo);
            Enable<Photo.uploadedDate>(cache, photo);
        }

        private void Enable<TField>(PXCache cache, Photo photo)
            where TField : IBqlField
        {
            PXUIFieldAttribute.SetEnabled<TField>(cache, photo, IsCopyPasteContext);
        }
    }
}
