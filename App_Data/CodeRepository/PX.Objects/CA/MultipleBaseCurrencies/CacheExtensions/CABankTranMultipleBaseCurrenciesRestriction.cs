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
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CR;

namespace PX.Objects.CA
{
	public sealed class CABankTranMultipleBaseCurrenciesRestriction : PXCacheExtension<CABankTran>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<BAccountR.baseCuryID, Equal<Current<CABankTransactionsMaint.Filter.baseCuryID>>, Or<BAccountR.baseCuryID, IsNull>>),
			Messages.IncorrectBAccountBaseCuryID,
		new Type[] { typeof(CABankTransactionsMaint.Filter.cashAccountID), typeof(CashAccount.branchID), typeof(CR.BAccountR.cOrgBAccountID), typeof(CR.BAccountR.bAccountID) })]
		public int? PayeeBAccountID { get; set; }
	}
}
