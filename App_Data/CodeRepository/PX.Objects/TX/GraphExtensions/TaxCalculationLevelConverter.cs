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
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.TaxProvider;

namespace PX.Objects.TX.GraphExtensions
{
	public static class TaxCalculationLevelConverter
	{
		public static string ToCSTaxCalcLevel(this TaxCalculationLevel target)
		{
			switch (target)
			{
				case TaxCalculationLevel.Inclusive:
					return CSTaxCalcLevel.Inclusive;
				case TaxCalculationLevel.CalcOnItemAmt:
					return CSTaxCalcLevel.CalcOnItemAmt;
				case TaxCalculationLevel.CalcOnItemAmtPlusTaxAmt:
					return CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt;
				default:
					return CSTaxCalcLevel.Inclusive;
			}
		}
	}
}
