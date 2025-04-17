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

namespace PX.Objects.GL.JournalEntryState
{
	public class CommonTypeStateController : EditableStateControllerBase
	{
		public CommonTypeStateController(JournalEntry journalEntry) : base(journalEntry)
		{
		}

		public override void Batch_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (JournalEntry.UnattendedMode)
				return;

			var batch = e.Row as Batch;

			base.Batch_RowSelected(cache, e);

			PXUIFieldAttribute.SetEnabled(cache, batch, true);
			PXUIFieldAttribute.SetEnabled<Batch.status>(cache, batch, false);
			PXUIFieldAttribute.SetEnabled<Batch.curyCreditTotal>(cache, batch, false);
			PXUIFieldAttribute.SetEnabled<Batch.curyDebitTotal>(cache, batch, false);
			PXUIFieldAttribute.SetEnabled<Batch.origBatchNbr>(cache, batch, false);
			PXUIFieldAttribute.SetEnabled<Batch.autoReverse>(cache, batch, (batch.AutoReverseCopy != true));
			PXUIFieldAttribute.SetEnabled<Batch.autoReverseCopy>(cache, batch, false);

			GLTranCache.AllowInsert = true;
			GLTranCache.AllowDelete = true;

			bool isAutoReverse = batch.AutoReverseCopy == true || batch.AutoReverse == true;
			bool createTaxTranEnabled = IsTaxTranCreationAllowed(batch) && isAutoReverse != true;

			PXUIFieldAttribute.SetEnabled<Batch.createTaxTrans>(cache, batch, createTaxTranEnabled);
		}
	}
}
