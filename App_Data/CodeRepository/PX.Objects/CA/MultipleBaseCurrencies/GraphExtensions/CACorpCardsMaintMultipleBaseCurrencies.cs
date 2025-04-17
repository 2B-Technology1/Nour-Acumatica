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
using PX.Objects.EP.DAC;

namespace PX.Objects.CA
{
	public class CACorpCardsMaintMultipleBaseCurrencies : PXGraphExtension<CACorpCardsMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDimensionSelector("EMPLOYEE",
			typeof(Search2<CR.Standalone.EPEmployee.bAccountID,
						InnerJoin<CashAccount,
							On<CashAccount.cashAccountID, Equal<Current<CACorpCard.cashAccountID>>>>,
						Where<CR.Standalone.EPEmployee.baseCuryID, Equal<CashAccount.baseCuryID>>>),
				typeof(CR.Standalone.EPEmployee.acctCD),
				typeof(CR.Standalone.EPEmployee.bAccountID),
				typeof(CR.Standalone.EPEmployee.acctCD),
				typeof(CR.Standalone.EPEmployee.acctName),
			typeof(CR.Standalone.EPEmployee.departmentID), DescriptionField = typeof(CR.Standalone.EPEmployee.acctName))]
		protected virtual void _(Events.CacheAttached<EPEmployeeCorpCardLink.employeeID> e)
		{
		}
	}
}