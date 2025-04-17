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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Services;
using PX.Objects.CS;
using PX.Objects.PJ.PhotoLogs.Descriptor;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Objects.PM;
using PX.SM;

using FileInfo = PX.SM.FileInfo;

namespace PX.Objects.PJ.PhotoLogs.PJ.Graphs
{
	public class PhotoLogEntry : ProjectManagementBaseMaint<PhotoLogEntry, PhotoLog>
	{
		[PXCopyPasteHiddenFields(typeof(PhotoLog.date))]
		[PXViewName("Photo Log")]
		public SelectFrom<PhotoLog>
			.LeftJoin<PMProject>
				.On<PMProject.contractID.IsEqual<PhotoLog.projectId>>
			.Where<
				PMProject.contractID.IsNull
				.Or<MatchUserFor<PMProject>>>
			.View PhotoLog;

		[PXCopyPasteHiddenView]
		[PXFilterable(typeof(Photo))]
		public SelectFrom<Photo>
			.Where<Photo.photoLogId.IsEqual<PhotoLog.photoLogId.FromCurrent>>.View Photos;

		[PXHidden]
		public SelectFrom<Photo>.View PhotoImage;
		public virtual IEnumerable photoImage()
		{
			return SelectFrom<Photo>
				.Where<Photo.photoCd.IsEqual<P.AsString>
					.And<Photo.photoLogId.IsEqual<P.AsInt>>>.View
				.Select(this, Photos.Current?.PhotoCd, Photos.Current?.PhotoLogId);
		}

		public PXSetup<PhotoLogSetup> PhotoLogSetup;
				
		public PXSelect<CSAttributeGroup,
			Where<CSAttributeGroup.entityType, Equal<Photo.typeName>,
				And<CSAttributeGroup.entityClassID, Equal<Photo.photoClassId>>>> PhotoAttributes;

		public SelectFrom<CSAnswers>
			.RightJoin<Photo>.On<CSAnswers.refNoteID.IsEqual<Photo.noteID>>.View Answers;

		public PhotoLogEntry()
		{
			PhotoLogSetup setup = PhotoLogSetup.Current;

			var commonAttributeColumnCreator = new CommonAttributeColumnCreator(this, PhotoAttributes);
			commonAttributeColumnCreator.GenerateColumns(Photos.Cache, nameof(Photos), nameof(Answers), false);
		}

		public PXAction<PhotoLog> UploadFiles;
		[PXButton]
		[PXUIField(DisplayName = "Upload Files", Visible = false)]
		public virtual IEnumerable uploadFiles(PXAdapter adapter)
		{
			var filesData = new List<KeyValuePair<string, byte[]>>();

			foreach (var pair in adapter.Arguments)
			{
				if (!(pair.Value is byte[] fileData))
				{
					continue;
				}

				filesData.Add(new KeyValuePair<string, byte[]>(pair.Key, fileData));
			}

			if (filesData.Any())
			{
				var photoLogEntry = CreateInstance<PhotoLogEntry>();
				photoLogEntry.PhotoLog.Current = PhotoLog.Current;

				PXLongOperation.StartOperation(this, () =>
				{
					photoLogEntry.UploadPhotos(filesData);

					// Show error message if there were some unsupported file types
					photoLogEntry.CheckUnsupportedFileTypes(filesData.Select(x => x.Key).ToArray());
				});
			}

			return adapter.Get();
		}

		public PXAction<PhotoLog> UploadFromAttachments;
		[PXButton]
		[PXUIField(DisplayName = "Upload Photos from Attachments")]
		public virtual IEnumerable uploadFromAttachments(PXAdapter adapter)
		{
			var attachmentIds = PXNoteAttribute.GetFileNotes(PhotoLog.Cache, PhotoLog.Current);

			if (attachmentIds.Any())
			{
				var photoLogEntry = CreateInstance<PhotoLogEntry>();
				photoLogEntry.PhotoLog.Current = PhotoLog.Current;

				var photoLogNoteId = photoLogEntry.PhotoLog.Current.NoteID;

				PXLongOperation.StartOperation(this, () =>
				{
					var uploadedAttachmentIds = photoLogEntry.GeneratePhotosByAttachmentIds(attachmentIds);

					if (uploadedAttachmentIds.Any())
					{
						photoLogEntry.DeleteAttachments(photoLogNoteId.Value, uploadedAttachmentIds);
					}
				});
			}

			return adapter.Get();
		}

		public PXAction<PhotoLog> CreatePhoto;
		[PXButton(CommitChanges = true, OnClosingPopup = PXSpecialButtonType.Cancel)]
		[PXUIField(DisplayName = "Create Photo", Visible = false)]
		public void createPhoto()
		{
			var photoEntry = CreateInstance<PhotoEntry>();
			if (PhotoLog.Cache.GetStatus(PhotoLog.Current) != PXEntryStatus.Inserted)
			{
				var photo = photoEntry.Photos.Insert();
				photo.PhotoLogId = PhotoLog.Current.PhotoLogId;
			}
			PXRedirectHelper.TryRedirect(photoEntry, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<PhotoLog> ViewPhoto;
		[PXButton]
		[PXUIField(DisplayName = "View Photo", Visible = false)]
		public void viewPhoto()
		{
			var photoEntry = CreateInstance<PhotoEntry>();
			photoEntry.Photos.Current =
				photoEntry.Photos.Search<Photo.photoId>(Photos.Current.PhotoId);
			PXRedirectHelper.TryRedirect(photoEntry, PXRedirectHelper.WindowMode.NewWindow);
		}

		public virtual void _(Events.RowSelected<Photo> args)
		{
			PXUIFieldAttribute.SetEnabled(args.Cache, args.Row, false);
			PXUIFieldAttribute.SetEnabled<Photo.isMainPhoto>(args.Cache, args.Row);
		}

		public virtual void _(Events.RowSelected<PhotoLog> args)
		{
			if (args.Row is PhotoLog photoLog)
			{
				var uploadingIsEnabled = PhotoLog.Cache.GetStatus(photoLog) != PXEntryStatus.Inserted;

				CreatePhoto.SetEnabled(uploadingIsEnabled);
				UploadFiles.SetEnabled(uploadingIsEnabled);
				UploadFromAttachments.SetEnabled(uploadingIsEnabled);
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

		private string[] _imageFileExtensions = null;
		private string[] GetImageFileExtensions()
		{
			if (_imageFileExtensions != null)
				return _imageFileExtensions;

			var allowedFileTypes = SelectFrom<UploadAllowedFileTypes>
				.Where<UploadAllowedFileTypes.isImage.IsEqual<True>
				.And<UploadAllowedFileTypes.forbidden.IsNotEqual<True>>>
				.View.Select(this)
				.FirstTableItems.ToArray();

			return _imageFileExtensions = allowedFileTypes
				.Select(x => x.FileExt)
				.Where(ext => !string.IsNullOrWhiteSpace(ext))
				.ToArray();
		}

		private bool IsImageFile(string fileName)
		{
			var fileNameExtension = Path.GetExtension(fileName);

			if (!string.IsNullOrWhiteSpace(fileNameExtension))
			{
				return GetImageFileExtensions().Any(x => x.Equals(fileNameExtension, StringComparison.OrdinalIgnoreCase));
			}

			return false;
		}

		private Guid[] GeneratePhotosByAttachmentIds(Guid[] attachmentIds)
		{
			var uploadFileMaintenace = CreateInstance<UploadFileMaintenance>();
			var imagesAttachments = new List<Guid>();
			var images = new List<FileInfo>();

			foreach (var attachmentId in attachmentIds)
			{
				var fileInfo = uploadFileMaintenace.GetFile(attachmentId);

				if (fileInfo != null && IsImageFile(fileInfo.FullName))
				{
					images.Add(fileInfo);
					imagesAttachments.Add(attachmentId);
				}
			}

			if (!images.Any())
			{
				return Array.Empty<Guid>();
			}

			UploadPhotos(images);

			return imagesAttachments.ToArray();
		}

		private void DeleteAttachments(Guid noteId, IEnumerable<Guid> attachmentIds)
		{
			var noteDocsToDelete
				= SelectFrom<NoteDoc>
					.Where<NoteDoc.noteID.IsEqual<@P.AsGuid>
					.And<NoteDoc.fileID.IsIn<@P.AsGuid>>>
					.View.Select(this, noteId, attachmentIds.ToArray())
					.FirstTableItems.ToArray();

			if (noteDocsToDelete.Length == 0)
				return;

			var uploadFileMaintenance = CreateInstance<UploadFileMaintenance>();

			foreach (var noteDocToDelete in noteDocsToDelete)
			{
				uploadFileMaintenance.Caches<NoteDoc>().Delete(noteDocToDelete);
			}

			uploadFileMaintenance.Caches<NoteDoc>().Persist(PXDBOperation.Delete);
		}

		private void CheckUnsupportedFileTypes(string[] fileNames)
		{
			if (fileNames.Any(x => !IsImageFile(x)))
			{
				throw new PXException(PhotoLogEntryMessages.UnsupprtedFileType, string.Join(",", GetImageFileExtensions()));
			}
		}

		private void UploadPhotos(IEnumerable<FileInfo> fileInfos)
		{
			UploadPhotos(fileInfos.Select(x => new KeyValuePair<string, byte[]>(x.Name, x.BinData)));
		}

		private void UploadPhotos(IEnumerable<KeyValuePair<string, byte[]>> filesData)
		{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				var existingPhotoNames = Photos.Select().RowCast<Photo>()
					.Select(x => Path.GetFileName(x.Name).ToLower()).ToHashSet();

				foreach (var fileData in filesData)
				{
					if (existingPhotoNames.Contains(Path.GetFileName(fileData.Key).ToLower()))
						continue;

					AppendPhoto(fileData.Key, fileData.Value);
				}

				ts.Complete();
			}
		}

		private bool AppendPhoto(string fileName, byte[] fileData)
		{
			if (!IsImageFile(fileName))
				return false;

			var photo = Photos.Insert();
			photo.PhotoLogId = PhotoLog.Current.PhotoLogId;

			AttachImageToPhoto(photo, fileName, fileData);
			Persist();

			return true;
		}

		private void AttachImageToPhoto(Photo photo, string fileName, byte[] fileData)
		{
			var uploadFileMaintenance = CreateInstance<UploadFileMaintenance>();
			var photoName = PhotoEntry.BuildPhotoName(photo, fileName);

			var attachedImageInfo = new FileInfo(photoName, null, fileData);
			if (!uploadFileMaintenance.SaveFile(attachedImageInfo))
			{
				throw new PXException(PhotoLogEntryMessages.FileNotFound, fileName);
			}

			PhotoEntry.FillPhotoFields(photo, attachedImageInfo, Accessinfo);
			PXNoteAttribute.SetFileNotes(Photos.Cache, photo, attachedImageInfo.UID.GetValueOrDefault());
		}
	}

	[PXLocalizable]
	public static class PhotoLogEntryMessages
	{
		public const string FileNotFound = "The {0} file cannot be saved.";
		public const string UnsupprtedFileType = PX.SM.Messages.UnsupportedFile + "{0}";
	}
}
