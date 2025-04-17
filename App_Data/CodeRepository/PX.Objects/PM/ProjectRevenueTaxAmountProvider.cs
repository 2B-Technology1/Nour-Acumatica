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
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.TX;

namespace PX.Objects.PM
{
	/// <summary>
	/// Provides the project revenue tax amount calculations in project and base currencies calculated from the data of <see cref="ARTran.CuryTaxAmt"/>
	/// and from the data of <see cref="ARTax.CuryRetainedTaxAmt"/>.
	/// </summary>
	public static class ProjectRevenueTaxAmountProvider
	{
		/// <summary>
		/// Calculates the inclusive tax amount from the data of <see cref="ARTran.CuryTaxAmt"/>.
		/// </summary>
		public static (decimal? CuryAmount, decimal? Amount) GetInclusiveTaxAmount(PXGraph graph, ARTran tran)
		{
			return (tran?.CuryTaxAmt, tran?.TaxAmt);
		}

		/// <summary>
		/// Calculates the inclusive retained tax amount from the data of <see cref="ARTax.CuryRetainedTaxAmt"/>.
		/// </summary>
		public static (decimal? CuryAmount, decimal? Amount) GetRetainedInclusiveTaxAmount(PXGraph graph, ARTran tran)
		{
			if (tran == null)
				return (null, null);

			ARTax result = SelectFrom<ARTax>
			   .InnerJoin<ARTran>
				   .On<ARTax.tranType.IsEqual<ARTran.tranType>
				   .And<ARTax.refNbr.IsEqual<ARTran.refNbr>
				   .And<ARTax.lineNbr.IsEqual<ARTran.lineNbr>>>>
			   .InnerJoin<Tax>
				   .On<Tax.taxID.IsEqual<ARTax.taxID>>
			   .Where<ARTran.tranType.IsEqual<ARTran.tranType.FromCurrent>
				   .And<ARTran.refNbr.IsEqual<ARTran.refNbr.FromCurrent>>
				   .And<ARTran.lineNbr.IsEqual<ARTran.lineNbr.FromCurrent>>
				   .And<Tax.taxType.IsIn<CSTaxType.sales, CSTaxType.vat>>
				   .And<Tax.taxCalcLevel.IsEqual<CSTaxCalcLevel.inclusive>>>
			   .AggregateTo<GroupBy<ARTran.lineNbr>,
				   Sum<ARTax.curyRetainedTaxAmt>,
				   Sum<ARTax.retainedTaxAmt>>
			   .View.SelectSingleBound(graph, new[] { tran });

			return (result?.CuryRetainedTaxAmt, result?.RetainedTaxAmt);
		}
	}
}
