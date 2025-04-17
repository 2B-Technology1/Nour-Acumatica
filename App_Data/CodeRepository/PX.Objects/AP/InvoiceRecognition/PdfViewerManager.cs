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

using PX.CloudServices.DAC;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System;

namespace PX.Objects.AP.InvoiceRecognition
{
	[PXHidden]
	public class PdfFileInfo : IBqlTable
	{
		[PXDBGuid]
		public virtual Guid? RecognizedRecordRefNbr { get; set; }
		public abstract class recognizedRecordRefNbr : BqlGuid.Field<recognizedRecordRefNbr> { }

		[PXUIField(DisplayName = "File ID", Enabled = false)]
		[PXDBGuid]
		public virtual Guid? FileId { get; set; }
		public abstract class fileId : BqlGuid.Field<fileId> { }
	}

	[PXInternalUseOnly]
	public class PdfViewerManager : PXGraph<PdfViewerManager>
	{
		public PXFilter<PdfFileInfo> File;

		public virtual void _(Events.FieldUpdated<PdfFileInfo.recognizedRecordRefNbr> e)
		{
			if (!(e.Row is PdfFileInfo fileInfo) || fileInfo.RecognizedRecordRefNbr == null)
			{
				return;
			}

			fileInfo.FileId = GetFileId(fileInfo.RecognizedRecordRefNbr);
			if (fileInfo.FileId == null)
			{
				return;
			}

			var file = APInvoiceRecognitionEntry.GetFile(this, fileInfo.FileId.Value);
			if (file == null)
			{
				return;
			}

			// To load restricted file by page via GetFile.ashx
			var fileInfoInMemory = new PX.SM.FileInfo(fileInfo.FileId.Value, file.Name, link: null, file.Data);
			PXContext.SessionTyped<PXSessionStatePXData>().FileInfo[fileInfoInMemory.UID.ToString()] = fileInfoInMemory;
		}

		private Guid? GetFileId(Guid? recognizedRecordRefNbr)
		{
			var record = SelectFrom<RecognizedRecord>
				.Where<RecognizedRecord.refNbr.IsEqual<@P.AsGuid>>
				.View.ReadOnly
				.SelectSingleBound(this, currents: null, recognizedRecordRefNbr)
				.TopFirst;
			if (record == null)
			{
				return null;
			}

			var recordCache = Caches[typeof(RecognizedRecord)];

			var fileNotes = PXNoteAttribute.GetFileNotes(recordCache, record);
			if (fileNotes == null || fileNotes.Length == 0)
			{
				return null;
			}

			return fileNotes[0];
		}
	}
}
