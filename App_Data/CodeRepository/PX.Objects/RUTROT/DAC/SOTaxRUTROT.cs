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
using PX.Objects.CM;
using PX.Objects.SO;
using PX.Objects.TX;

namespace PX.Objects.RUTROT
{
	[Serializable]
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public partial class SOTaxRUTROT : PXCacheExtension<SOTax>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.rutRotDeduction>();
		}
		#region CuryRUTROTTaxAmt
		public abstract class curyRUTROTTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyRUTROTTaxAmt> { }
		/// <summary>
		/// The amount of tax (VAT) associated with the <see cref="SOLine">lines</see> in the selected currency.
		/// </summary>
		[PXCurrency(typeof(SOTax.curyInfoID), typeof(rUTROTTaxAmt))]
		[PXUnboundFormula(typeof(Switch<Case<Where<Selector<SOTax.taxID, Tax.taxCalcLevel>, NotEqual<CSTaxCalcLevel.inclusive>>, SOTax.curyTaxAmt>, CS.decimal0>),
				   typeof(SumCalc<SOLineRUTROT.curyRUTROTTaxAmountDeductible>), FieldClass = RUTROTMessages.FieldClass)]
		public virtual decimal? CuryRUTROTTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region RUTROTTaxAmt
		public abstract class rUTROTTaxAmt : PX.Data.BQL.BqlDecimal.Field<rUTROTTaxAmt> { }
		/// <summary>
		/// The amount of tax (VAT) associated with the <see cref="SOLine">line</see> in the base currency.
		/// </summary>
		[PXBaseCury]
		public virtual decimal? RUTROTTaxAmt
		{
			get;
			set;
		}
		#endregion
	}
}
