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
using System;

namespace PX.Objects.AP.InvoiceRecognition
{
	internal readonly struct RecognizedRecordFileInfo
	{
		public string FileName { get; }
		public byte[] FileData { get; }
		public Guid FileId { get; }
		public RecognizedRecord RecognizedRecord { get; }

		public RecognizedRecordFileInfo(string fileName, byte[] fileData, Guid fileId, RecognizedRecord record)
		{
			fileName.ThrowOnNullOrWhiteSpace(nameof(fileName));
			fileData.ThrowOnNull(nameof(fileData));

			FileName = fileName;
			FileData = fileData;
			FileId = fileId;
			RecognizedRecord = record;
		}

		public RecognizedRecordFileInfo(string fileName, byte[] fileData, Guid fileId)
			: this(fileName, fileData, fileId, null)
		{
		}
	}
}
