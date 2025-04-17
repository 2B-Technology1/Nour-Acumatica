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
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FA
{
	public class FABookCollection : IPrefetchable
	{
		public Dictionary<int, FABook> Books = new Dictionary<int, FABook>();

		public void Prefetch()
		{
			Books = PXDatabase
				.SelectMulti<FABook>(
					new PXDataField<FABook.bookID>(),
					new PXDataField<FABook.bookCode>(),
					new PXDataField<FABook.updateGL>(),
					new PXDataField<FABook.description>())
				.Select(row => new FABook
				{
					BookID = row.GetInt32(0),
					BookCode = row.GetString(1).Trim(),
					UpdateGL = row.GetBoolean(2),
					Description = row.GetString(3)?.Trim()
				})
				.ToDictionary(book => (int)book.BookID);
		}
	}
}
