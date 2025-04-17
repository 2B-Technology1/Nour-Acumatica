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

namespace PX.Objects.GL
{
	[PXLocalizable]
	public static class InfoMessages
	{
		public const string SomeTransactionsCannotBeReclassified = "Some transactions that match the specified selection criteria cannot be reclassified. These transactions will not be loaded.";
		public const string NoReclassifiableTransactionsHaveBeenFoundToMatchTheCriteria = "No transactions, for which the reclassification can be performed, have been found to match the specified criteria.";
		public const string NoReclassifiableTransactionsHaveBeenSelected = "No transactions, for which the reclassification can be performed, have been selected.";
		public const string SomeTransactionsOfTheBatchCannotBeReclassified = "Some transactions of the batch cannot be reclassified. These transactions will not be loaded.";
		public const string NoReclassifiableTransactionsHaveBeenFoundInTheBatch = "No transactions, for which the reclassification can be performed, have been found in the batch.";
		public const string TransactionsListedOnTheFormIfAnyWillBeRemoved = "Transactions listed on the form (if any) will be removed. New transactions that match the selection criteria will be loaded. Do you want to continue?";

		public const string ReleasedDocCannotBeDeleted = "Released documents cannot be deleted.";
	}
}
