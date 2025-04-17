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

using PX.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PX.Api;
using PX.Common;
using PX.DataSync;
using MSZip = System.IO.Compression;
using PX.Objects.Localizations.CA.DataSync;

namespace PX.DataSync
{
	public class EFTDownloadProvider : CPA005ExportProvider
	{
		protected override SM.FileInfo SetFile(byte[] bytes)
		{
			if (_file != null)
			{
				_file.Comment = PX.Data.PXMessages.LocalizeFormatNoPrefixNLA(Messages.ExportedComment, this.ProviderName);
				_file.BinData = bytes;
				var info = new EFTFileInfo(_file);
				PXLongOperation.SetCustomInfo(info);
			}

			return _file;
		}

		public override string FormatFileCreationNumber(string batchNbr)
		{
			return base.FormatFileCreationNumber(batchNbr, EFTDownloadMessages.FileCreationNumberInvalid);
		}

		public override string ProviderName
		{
			get
			{
				return PX.Data.PXMessages.Localize(EFTDownloadMessages.EFTDownloadProvider);
			}
		}
	}

	public class EFTFileInfo : IPXCustomInfo
	{
		private SM.FileInfo _File;
		private static HashSet<object> _OperationsPendingCompletion = new HashSet<object>();

		public EFTFileInfo(SM.FileInfo file)
		{
			_File = file;
		}

		public void Complete(PXLongRunStatus status, PXGraph graph)
		{
			if (status == PXLongRunStatus.Completed)
			{
				if (!_OperationsPendingCompletion.Contains(graph.UID))
				{
					if (_File != null)
					{
						_OperationsPendingCompletion.Add(graph.UID);
						var fileToDownload = _File;
						_File = null;
						throw new PXRedirectToFileException(fileToDownload, true);
					}
				}
				else
				{
					_OperationsPendingCompletion.Remove(graph.UID);
				}
			}
		}
	}

	[PX.Common.PXLocalizable]
	public static class EFTDownloadMessages
	{
		public const string EFTDownloadProvider = "EFT Download Provider";
		public const string FileCreationNumberInvalid = "The Batch Number '{0}' is invalid. The valid values are between 1 and 9999.";
	}
}

