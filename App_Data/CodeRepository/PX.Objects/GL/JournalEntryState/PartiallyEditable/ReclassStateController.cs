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

namespace PX.Objects.GL.JournalEntryState.PartiallyEditable
{
	public class ReclassStateController : PartiallyEditableStateControllerBase
	{
		public ReclassStateController(JournalEntry journalEntry) : base(journalEntry)
		{
		}

		public override void Batch_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var batch = e.Row as Batch;

			base.Batch_RowSelected(cache, e);

			PXUIFieldAttribute.SetEnabled<Batch.dateEntered>(cache, batch, true);
			PXUIFieldAttribute.SetEnabled<Batch.description>(cache, batch, true);
			PXUIFieldAttribute.SetEnabled<Batch.branchID>(cache, batch, true);

			PXUIFieldAttribute.SetVisible<GLTran.origBatchNbr>(GLTranCache, null, true);
			
			JournalEntry.editReclassBatch.SetVisible(true);
		}

		[System.Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2019R2)]
		public static bool HasRamainingAmount(bool? hasRemainingAmount, GLTran tran)
		{
			return (hasRemainingAmount ?? false) || (tran.CuryReclassRemainingAmt ?? 0m) != 0m;
		}
		
	}
}
