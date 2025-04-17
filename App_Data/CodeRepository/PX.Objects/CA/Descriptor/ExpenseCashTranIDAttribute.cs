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
using PX.Objects.GL;
using System.Linq;


namespace PX.Objects.CA
{
	#region ExpenseCashTranIDAttribute
	public class ExpenseCashTranIDAttribute : CashTranIDAttribute
	{
		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			CAExpense parentDoc = (CAExpense)orig_Row;

			if ((parentDoc.Released == true) && (catran_Row.TranID != null))
			{
				return null;
			}

			catran_Row.OrigModule = BatchModule.CA;
			catran_Row.OrigTranType = parentDoc.DocType;
			catran_Row.OrigRefNbr = parentDoc.RefNbr;

			catran_Row.CashAccountID = parentDoc.CashAccountID;
			catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
			catran_Row.CuryID = parentDoc.CuryID;
			catran_Row.CuryInfoID = parentDoc.CuryInfoID;
			catran_Row.CuryTranAmt = parentDoc.CuryTranAmt * (parentDoc.DrCr == DrCr.Debit ? 1 : -1);
			catran_Row.TranAmt = parentDoc.TranAmt * (parentDoc.DrCr == DrCr.Debit ? 1 : -1);
			catran_Row.DrCr = parentDoc.DrCr;
			catran_Row.TranDate = parentDoc.TranDate;
			catran_Row.TranDesc = parentDoc.TranDesc;
			catran_Row.ReferenceID = null;
			catran_Row.Released = parentDoc.Released;
			catran_Row.Hold = false;
		    SetPeriodsByMaster(sender, catran_Row, parentDoc.TranPeriodID);
            catran_Row.Cleared = parentDoc.Cleared;
			catran_Row.ClearDate = parentDoc.ClearDate;

			PXSelectBase<CashAccount> selectStatement = new PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>(sender.Graph);
			CashAccount cashacc = (CashAccount)selectStatement.View.SelectSingle(catran_Row.CashAccountID);
			if (cashacc != null && cashacc.Reconcile == false && (catran_Row.Cleared != true || catran_Row.TranDate == null))
			{
				catran_Row.Cleared = true;
				catran_Row.ClearDate = catran_Row.TranDate;
			}

			return catran_Row;
		}
	}
	#endregion
}