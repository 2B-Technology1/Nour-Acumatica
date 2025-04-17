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
using PX.Objects.CM;

namespace PX.Objects.DR
{
	public sealed class DRScheduleMultipleBaseCurrencies : PXCacheExtension<DRSchedule>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		#region BaseCuryIDASC606
		public abstract class baseCuryIDASC606 : PX.Data.BQL.BqlString.Field<baseCuryIDASC606> { }

		[PXString]
		[PXSelector(typeof(Search<CurrencyList.curyID>))]
		[PXRestrictor(typeof(Where<CurrencyList.curyID, IsBaseCurrency>), Messages.CurrencyIsNotBaseCurrency)]
		[PXUIField(DisplayName = "Currency")]
		// Acuminator disable once PX1030 PXDefaultIncorrectUse used for unbound field linked to a hidden bound.
		[PXDefault(typeof(Current<AccessInfo.baseCuryID>))]
		[PXDBCalced(typeof(DRSchedule.baseCuryID), typeof(string))]
		public string BaseCuryIDASC606 { get; set; }
		#endregion
	}
}
