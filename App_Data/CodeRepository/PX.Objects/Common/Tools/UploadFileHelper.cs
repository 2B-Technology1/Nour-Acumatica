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
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Tools;
using PX.SM;

namespace PX.Objects.Common.Tools
{
	public static class UploadFileHelper
	{
		/// <summary>
		/// AttachDataAsFile
		/// </summary>
		/// <remarks>Invoke before persisting of DAC</remarks>
		public static void AttachDataAsFile(string fileName, string data, IDACWithNote dac, PXGraph graph)
		{
			var file = new FileInfo(Guid.NewGuid(), fileName, null, SerializationHelper.GetBytes(data));

			var uploadFileMaintGraph = PXGraph.CreateInstance<UploadFileMaintenance>();

			if (uploadFileMaintGraph.SaveFile(file) || file.UID == null)
			{
				var fileNote = new NoteDoc { NoteID = dac.NoteID, FileID = file.UID };

				graph.Caches[typeof(NoteDoc)].Insert(fileNote);

				graph.Persist(typeof(NoteDoc), PXDBOperation.Insert);
			}
		}
	}
}
