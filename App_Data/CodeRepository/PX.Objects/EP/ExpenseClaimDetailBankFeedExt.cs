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
using PX.Objects.CS;
using System.Collections.Generic;

namespace PX.Objects.EP
{
	public class ExpenseClaimDetailBankFeedExt : PXGraphExtension<ExpenseClaimDetailEntry>
	{
		public delegate void CopyPasteGetScriptDelegate(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers);

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.bankFeedIntegration>();
		}

		protected virtual void _(Events.FieldUpdated<EPExpenseClaimDetails.hold> e)
		{
			var row = e.Row as EPExpenseClaimDetails;
			if (row == null) return;

			bool? oldValue = (bool?)e.OldValue;

			var bankFeedExt = PXCache<EPExpenseClaimDetails>.GetExtension<DAC.ExpenseClaimDetailsBankFeedExt>(row);

			if (oldValue == true && row.Hold == false && bankFeedExt.BankTranStatus == EPBankTranStatus.Pending)
			{
				throw new PXException(Messages.BankTranIsPending);
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXSubordinateAndWingmenSelectorAttribute))]
		[PXConfigureSubordinateAndWingmenSelector]
		protected virtual void _(Events.CacheAttached<EPExpenseClaimDetails.employeeID> e)
		{
		}

		[PXOverride]
		public void CopyPasteGetScript(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers, CopyPasteGetScriptDelegate baseMethod)
		{
			baseMethod(isImportSimple, script, containers);

			int bankTranStatusIndex = script.FindIndex(i => i.FieldName == nameof(DAC.ExpenseClaimDetailsBankFeedExt.BankTranStatus));

			if (bankTranStatusIndex == -1) return;

			script.RemoveAt(bankTranStatusIndex);
			containers.RemoveAt(bankTranStatusIndex);
		}
	}
}
