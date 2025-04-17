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
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CA
{
	public sealed class CustomerPaymentMethodMultipleBaseCurrencies : PXCacheExtension<CustomerPaymentMethod>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		#region BaseCuryID
		public abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }

		[PXString(5, IsUnicode = true)]
		[PXFormula(typeof(Selector<CustomerPaymentMethod.bAccountID, BAccountR.baseCuryID>))]
		[PXUIField(DisplayName = "Currency")]
		public string BaseCuryID { get; set; }
		#endregion

		#region CashAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<CashAccount.baseCuryID, Equal<Current<CustomerPaymentMethodMultipleBaseCurrencies.baseCuryID>>>), null)]
		public int? CashAccountID { get; set; }
		#endregion
	}
}
