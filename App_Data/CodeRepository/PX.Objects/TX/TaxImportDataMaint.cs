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
using PX.Data;
using System.Collections;

namespace PX.Objects.TX
{
	public class TaxImportDataMaint : PXGraph<TaxImportDataMaint>
	{
		public PXSelect<TXImportFileData> Data;
        public PXSavePerRow<TXImportFileData> Save;
        public PXCancel<TXImportFileData> Cancel;

		private bool _importing;
		private bool _cleared = false;

		protected virtual void TXImportFileData_StateCode_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null) _importing = sender.GetValuePending(e.Row, PXImportAttribute.ImportFlag) != null;
		}


		protected virtual void TXImportFileData_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			TXImportFileData row = e.Row as TXImportFileData;
			if (row != null)
			{
				if (_importing && !_cleared)
				{
					PXDatabase.Delete<TXImportFileData>();
					_cleared = true;
				}

			}
		}

		public override void Persist()
		{
			base.Persist();
			Clear();
		}
	}

	public class TaxImportZipDataMaint : PXGraph<TaxImportZipDataMaint>
	{
		public PXSelect<TXImportZipFileData> Data;
        public PXSavePerRow<TXImportZipFileData> Save;
        public PXCancel<TXImportZipFileData> Cancel;

		private bool _importing;
		private bool _cleared = false;

		protected virtual void TXImportZipFileData_ZipCode_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null) _importing = sender.GetValuePending(e.Row, PXImportAttribute.ImportFlag) != null;
		}


		protected virtual void TXImportZipFileData_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			TXImportZipFileData row = e.Row as TXImportZipFileData;
			if (row != null)
			{
				if (_importing && !_cleared)
				{
					PXDatabase.Delete<TXImportZipFileData>();
					_cleared = true;
				}

			}
		}

		public override void Persist()
		{
			base.Persist();
			Clear();
		}
	}
}
