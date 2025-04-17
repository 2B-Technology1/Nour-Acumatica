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

using PX.Api;
using PX.Common;
using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Common.GraphExtensions.Abstract
{
	internal sealed class FileDto
	{
		// db field size 255 - but it form as {fileId}/{name} - plus little reserve (10 symbols)
		public const int FileNameMaxLength = 208; //2^7 + 2^6 + 2^4
		public FileDto(Guid entityId, string name, byte[] content, Guid? fileId = null, string contentId = null)
		{
			EntityNoteId = entityId;
			Content = content ?? throw new ArgumentNullException(nameof(content));
			Name = name ?? throw new ArgumentNullException(nameof(name));
			try
			{
				var extension = System.IO.Path.GetExtension(Name);
				if (Name.Length > FileNameMaxLength)
				{
					if (extension.IsNullOrEmpty())
					{
						var newName = Name.Substring(0, FileNameMaxLength);
						PXTrace.WriteWarning("Trying to save file with too long name. The name was cut off. Original value: {0}, new value: {1}", name, newName);
						Name = newName;
					}
					else
					{
						var newName = Name.Substring(0, FileNameMaxLength + extension.Length);
						PXTrace.WriteWarning("Trying to save file with too long name. The name was cut off. Original value: {0}, new value: {1}", name, newName);
						Name = newName + extension;
					}
				}
			}
			catch
			{
				PXTrace.WriteWarning("Trying to save file with invalid file name. The name will be replaced with default: \'file\'.");
				Name = "file";
			}

			FileId = fileId ?? Guid.NewGuid();
			ContentId = contentId;
		}

		public Guid EntityNoteId { get; }
		public Guid FileId { get; }
		public string Name { get; }
		public string FullName => FileId + "\\" + Name;
		public byte[] Content { get; }
		public string ContentId { get; }
	}

	[PXInternalUseOnly]
	public class AttachmentsHandlerExtension<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph, new()
	{
		protected string PrimaryScreenID { get; private set; }

		public override void Initialize()
		{
			base.Initialize();

			PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeByGraphType(PX.Api.CustomizedTypeManager.GetTypeNotCustomized(typeof(TGraph).FullName));
			PrimaryScreenID = node?.ScreenID ?? "CR306015";
		}


		#region DAC overrides
		[PXDBString(InputMask = "", IsUnicode = true)]
		public virtual void _(Events.CacheAttached<UploadFile.name> e)
		{
		}
		#endregion


		internal void InsertFile(FileDto file)
		{
			InsertFile(file, out var __);
		}

		private protected void InsertFile(FileDto file, out Action revertCallback)
		{
			if (file is null)
				throw new ArgumentNullException(nameof(file));

			Base.EnsureCachePersistence(typeof(UploadFile));
			var uploadFileCache = Base.Caches[typeof(UploadFile)];
			var uploadFile = (UploadFile)uploadFileCache.CreateInstance();
			uploadFile.FileID = file.FileId;
			uploadFile.LastRevisionID = 1;
			uploadFile.Versioned = true;
			uploadFile.IsPublic = false;
			uploadFile.Name = file.FullName;
			uploadFile.PrimaryScreenID = PrimaryScreenID;
			uploadFileCache.Insert(uploadFile);

			Base.EnsureCachePersistence(typeof(UploadFileRevision));
			var fileRevisionCache = Base.Caches[typeof(UploadFileRevision)];
			var fileRevision = (UploadFileRevision)fileRevisionCache.CreateInstance();
			fileRevision.FileID = file.FileId;
			fileRevision.FileRevisionID = 1;
			fileRevision.Data = file.Content;
			fileRevision.Size = UploadFileHelper.BytesToKilobytes(file.Content.Length);
			fileRevisionCache.Insert(fileRevision);

			Base.EnsureCachePersistence(typeof(NoteDoc));
			var noteDocCache = Base.Caches[typeof(NoteDoc)];
			var noteDoc = (NoteDoc)noteDocCache.CreateInstance();
			noteDoc.NoteID = file.EntityNoteId;
			noteDoc.FileID = file.FileId;
			noteDocCache.Insert(noteDoc);

			revertCallback = () =>
			{
				noteDocCache.SetStatus(noteDoc, PXEntryStatus.InsertedDeleted);
				uploadFileCache.SetStatus(uploadFile, PXEntryStatus.InsertedDeleted);
				fileRevisionCache.SetStatus(fileRevision, PXEntryStatus.InsertedDeleted);
			};
		}
	}
}
