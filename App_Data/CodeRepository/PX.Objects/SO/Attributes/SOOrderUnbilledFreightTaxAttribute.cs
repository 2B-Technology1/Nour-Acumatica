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
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.TX;

namespace PX.Objects.SO.Attributes
{
	/// <summary>
	/// Extends <see cref="SOOrderTaxAttribute"/> and calculates <see cref="SOOrder.CuryUnbilledOrderTotal"/> and <see cref="SOOrder.CuryUnbilledTaxTotal"/> for the Parent (Header) SOOrder.
	/// This Attribute overrides some of functionality of <see cref="SOTaxAttribute"/>.
	/// This Attribute is applied to the <see cref="SOOrder.FreightTaxCategoryID"/> field instead of SO Line.
	/// </summary>
	/// <example>
	/// [SOOrderUnbilledFreightTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), typeof(taxCalcMode), TaxCalc = TaxCalc.ManualLineCalc)]
	/// </example>
	public class SOOrderUnbilledFreightTaxAttribute : SOOrderTaxAttribute
	{
		protected override short SortOrder
		{
			get
			{
				return 2;
			}
		}

		public SOOrderUnbilledFreightTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: this(ParentType, TaxType, TaxSumType, null)
		{
		}

		public SOOrderUnbilledFreightTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType, Type TaxCalculationMode)
			: base(ParentType, TaxType, TaxSumType, TaxCalculationMode)
		{
			_CuryTaxableAmt = typeof(SOTax.curyUnbilledTaxableAmt).Name;
			_CuryExemptedAmt = typeof(SOTax.curyUnbilledTaxableAmt).Name;
			_CuryTaxAmt = typeof(SOTax.curyUnbilledTaxAmt).Name;

			CuryLineTotal = typeof(SOOrder.curyUnbilledLineTotal);
			CuryTaxTotal = typeof(SOOrder.curyUnbilledTaxTotal);
			CuryTranAmt = typeof(SOOrder.curyUnbilledFreightTot);

			this._Attributes.Clear();
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (sender.Graph is SOInvoiceEntry)
				TaxCalc = TaxCalc.Calc;
		}

		protected override void CalcDocTotals(
			PXCache sender,
			object row,
			decimal CuryTaxTotal,
			decimal CuryInclTaxTotal,
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			_CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);

			decimal CuryUnbilledLineTotal = (decimal)(ParentGetValue<SOOrder.curyUnbilledLineTotal>(sender.Graph) ?? 0m);
			decimal CuryUnbilledMiscTotal = (decimal)(ParentGetValue<SOOrder.curyUnbilledMiscTot>(sender.Graph) ?? 0m);
			decimal CuryUnbilledFreightTotal = (decimal)(ParentGetValue<SOOrder.curyUnbilledFreightTot>(sender.Graph) ?? 0m);
			decimal CuryUnbilledDiscTotal = (decimal)(ParentGetValue<SOOrder.curyUnbilledDiscTotal>(sender.Graph) ?? 0m);

			CuryUnbilledLineTotal += CuryUnbilledMiscTotal;

			decimal CuryUnbilledDocTotal = CuryUnbilledLineTotal + CuryUnbilledFreightTotal + CuryTaxTotal - CuryInclTaxTotal - CuryUnbilledDiscTotal;

			if (object.Equals(CuryUnbilledDocTotal, (decimal)(ParentGetValue<SOOrder.curyUnbilledOrderTotal>(sender.Graph) ?? 0m)) == false)
			{
				ParentSetValue<SOOrder.curyUnbilledOrderTotal>(sender.Graph, CuryUnbilledDocTotal);
				ParentSetValue<SOOrder.openDoc>(sender.Graph, (CuryUnbilledDocTotal != 0m));
			}
		}

		//Only base attribute should re-default taxes
		protected override void ReDefaultTaxes(PXCache cache, List<object> details)
		{
		}
		protected override void ReDefaultTaxes(PXCache cache, object clearDet, object defaultDet, bool defaultExisting = true)
		{
		}
	}
}
