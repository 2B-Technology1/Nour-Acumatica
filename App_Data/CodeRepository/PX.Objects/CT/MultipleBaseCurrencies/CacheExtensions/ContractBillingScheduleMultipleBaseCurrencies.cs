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
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CT;

namespace PX.Objects.CT
{
	public sealed class ContractBillingScheduleMultipleBaseCurrencies : PXCacheExtension<ContractBillingSchedule>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		#region COrgBAccountID
		public new abstract class cOrgBAccountID : PX.Data.BQL.BqlString.Field<cOrgBAccountID> { }

		[PXInt]
		[PXFormula(typeof(Selector<ContractBillingSchedule.accountID, Customer.cOrgBAccountID>))]
		public int? COrgBAccountID { get; set; }
		#endregion

		#region CustomerBaseCuryID
		public new abstract class customerBaseCuryID : PX.Data.BQL.BqlString.Field<customerBaseCuryID> { }

		[PXString]
		[PXFormula(typeof(Selector<ContractBillingSchedule.accountID, Customer.baseCuryID>))]
		public string CustomerBaseCuryID { get; set; }
		#endregion
	}
}
