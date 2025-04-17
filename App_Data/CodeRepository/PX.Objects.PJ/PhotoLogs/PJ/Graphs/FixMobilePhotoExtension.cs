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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.SM;

namespace PX.Objects.PJ.PhotoLogs.PJ.Graphs
{
	//TODO: Should be removed after implementing AC-190658
	public class FixMobilePhotoExtension<T> : PXGraphExtension<T> where T : PXGraph
	{
		public virtual void _(Events.RowSelecting<Photo> args)
		{
			var photo = args.Row;
			if (photo != null && (photo.FileId == null || photo.ImageUrl == null))
			{
				var fileSelect = new SelectFrom<UploadFile>
								.InnerJoin<NoteDoc>.On<NoteDoc.fileID.IsEqual<UploadFile.fileID>>
								.Where<NoteDoc.noteID.IsEqual<@P.AsGuid>>.View(Base);

				using (new PXFieldScope(fileSelect.View, typeof(UploadFile.fileID), typeof(UploadFile.createdByID), typeof(UploadFile.name)))
				{
					UploadFile file = fileSelect.SelectSingle(photo.NoteID);
					if (file != null)
					{
						photo.FileId = file.FileID;
						photo.UploadedById = file.CreatedByID;
						photo.Name = file.Name;
						photo.ImageUrl = string.Concat(PXUrl.SiteUrlWithPath(), Descriptor.Constants.FileUrl, photo.FileId);
					}
				}
			}
		}
	}

	public class FixMobilePhotoEntryExtension : FixMobilePhotoExtension<PhotoEntry> { }
	public class FixMobilePhotoLogMaintExtension : FixMobilePhotoExtension<PhotoLogMaint> { }
	public class FixMobilePhotoLogEntryExtension : FixMobilePhotoExtension<PhotoLogEntry> { }
	public class FixMobileDailyFieldReportEntryExtension : FixMobilePhotoExtension<DailyFieldReportEntry> { }
}
