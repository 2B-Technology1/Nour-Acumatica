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
using PX.Objects.GL;
using PX.Objects.CR;

namespace PX.Objects.AR
{
	public sealed class ARDunningCustomerClassMultipleBaseCurrenciesRestriction : PXCacheExtension<ARDunningCustomerClass>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		#region DunningFee
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDecimal(typeof(Search3<CM.CurrencyList.decimalPlaces,
			InnerJoin<Branch, On<Branch.baseCuryID, Equal<CM.CurrencyList.curyID>>>,
			OrderBy<Desc<CM.CurrencyList.decimalPlaces>>>), MinValue = 0)]
		public decimal? DunningFee { get; set; }
		#endregion
	}
}
