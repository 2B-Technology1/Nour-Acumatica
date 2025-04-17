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
using PX.Api;
using PX.Common;
using PX.Data;

namespace PX.Objects.SO
{
	public class SOPackageNoteAttribute : PXNoteAttribute
	{
		public const string CommercialInvoiceFilePostfix = "Customs";

		public override void noteFilesFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.noteFilesFieldSelecting(sender, e);

			if (e.ReturnValue is string[] files && files.Length > 1)
			{
				var orderedFiles = new List<string>();
				var filesCategory = files
					.Select(f => new { FileInfo = f, Category = getFileCategory(f) })
					.ToList();

				orderedFiles
					.AddRange(filesCategory.Where(f => !f.Category.HasFlag(PackageFileCategory.CommercialInvoice))
					.Select(f => f.FileInfo));
				orderedFiles
					.AddRange(filesCategory.Where(f => f.Category.HasFlag(PackageFileCategory.CommercialInvoice))
					.Select(f => f.FileInfo));

				e.ReturnValue = orderedFiles.ToArray();
			}

			PackageFileCategory getFileCategory(string fileInfo)
			{
				var category = PackageFileCategory.None;
				var fiParts = fileInfo.Split('$');
				if (fiParts.Length > 1)
				{
					var fileFullName = fiParts[1];
					category = GetFileCategory(fileFullName);
				}
				return category;
			}
		}

		public virtual PackageFileCategory GetFileCategory(string fileName)
		{
			string getFilePostfix(string fileName)
			{
				fileName = fileName.TrimEnd();
				var ext = System.IO.Path.GetExtension(fileName);
				if (!string.IsNullOrEmpty(ext))
					fileName = fileName.Substring(0, fileName.Length - ext.Length);

				var i = fileName.Length;
				char ch;

				do ch = fileName[--i];
				while (!char.IsLetterOrDigit(ch) && i > 0);

				string postfix = string.Empty;

				while (i >= 0)
				{
					ch = fileName[i--];
					if (char.IsLetter(ch))
						postfix = ch + postfix;
					else
					{
						if (string.IsNullOrEmpty(postfix))
							return string.Empty;
						return postfix;
					}
				}
				return string.Empty;
			}

			var filePostfix = getFilePostfix(fileName);

			if (!string.IsNullOrEmpty(filePostfix) && filePostfix.Equals(CommercialInvoiceFilePostfix, StringComparison.InvariantCultureIgnoreCase))
				return PackageFileCategory.CommercialInvoice;

			return PackageFileCategory.CarrierLabel;
		}
	}
}
