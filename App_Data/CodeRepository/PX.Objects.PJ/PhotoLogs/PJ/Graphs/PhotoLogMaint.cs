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
using PX.Objects.CN.Common.Extensions;
using PX.Objects.PJ.PhotoLogs.Descriptor;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PM;

namespace PX.Objects.PJ.PhotoLogs.PJ.Graphs
{
	public class PhotoLogMaint : PXGraph<PhotoLogMaint>
	{
		public PXFilter<PhotoLogFilter> Filter;

		public PXSetup<PhotoLogSetup> PhotoLogSetup;

		[PXFilterable(typeof(PhotoLog))]
		[PXViewName("Photo Logs")]
		public SelectFrom<PhotoLog>
			.InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<PhotoLog.projectId>>
			.Where<
				Brackets<PhotoLog.projectId.IsEqual<PhotoLogFilter.projectId.FromCurrent>
					.Or<PhotoLogFilter.projectId.FromCurrent.IsNull>>
				.And<PhotoLog.projectTaskId.IsEqual<PhotoLogFilter.projectTaskId.FromCurrent>
					.Or<PhotoLogFilter.projectTaskId.FromCurrent.IsNull>>
				.And<PhotoLog.date.IsGreaterEqual<PhotoLogFilter.dateFrom.FromCurrent>
					.Or<PhotoLogFilter.dateFrom.FromCurrent.IsNull>>
				.And<PhotoLog.date.IsLessEqual<PhotoLogFilter.dateTo.FromCurrent>
					.Or<PhotoLogFilter.dateTo.FromCurrent.IsNull>>
				.And<MatchUserFor<PMProject>>>
			.OrderBy<Desc<PhotoLog.photoLogCd>>
			.View PhotoLogs;

		[PXHidden]
		public SelectFrom<Photo>
			.InnerJoin<PhotoLog>
				.On<Photo.photoLogId.IsEqual<PhotoLog.photoLogId>>
			.Where<PhotoLog.photoLogCd.IsEqual<PhotoLog.photoLogCd.FromCurrent>
				.And<Photo.isMainPhoto.IsEqual<True>>>.View MainPhoto;

		public PXInsert<PhotoLogFilter> InsertPhotoLog;
		public PXAction<PhotoLogFilter> EditPhotoLog;

		public PhotoLogMaint()
		{
			PhotoLogSetup setup = PhotoLogSetup.Current;
		}

		[PXInsertButton]
		[PXUIField(DisplayName = "")]
		public virtual void insertPhotoLog()
		{
			var graph = CreateInstance<PhotoLogEntry>();
			graph.PhotoLog.Insert();
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
		}

		[PXEditDetailButton]
		[PXUIField(DisplayName = "")]
		public virtual void editPhotoLog()
		{
			this.RedirectToEntity(PhotoLogs.Current, PXRedirectHelper.WindowMode.InlineWindow);
		}

		public virtual void _(Events.RowSelecting<Photo> args)
		{
			var photo = args.Row;
			if (photo != null)
			{
				photo.ImageUrl = photo.ImageUrl ?? string.Concat(PXUrl.SiteUrlWithPath(), Constants.FileUrl, photo.FileId);
			}
		}

		public virtual void _(Events.RowInserted<PhotoLogFilter> args)
		{
			Filter.SetValueExt<PhotoLogFilter.dateTo>(Filter.Current, Accessinfo.BusinessDate);
		}
	}
}
