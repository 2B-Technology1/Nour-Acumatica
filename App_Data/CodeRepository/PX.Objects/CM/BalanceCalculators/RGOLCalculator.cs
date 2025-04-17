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

namespace PX.Objects.CM.Extensions
{
	public class RGOLCalculationResult
	{
		public decimal? ToCuryAdjAmt { get; set; }
		public decimal? RgolAmt { get; set; }
	}

	public class RGOLCalculator
	{
		private readonly CurrencyInfo from_info;
		private readonly CurrencyInfo to_info;
		private readonly CurrencyInfo to_originfo;

		public RGOLCalculator(
			CurrencyInfo from_info,
			CurrencyInfo to_info,
			CurrencyInfo to_originfo)
		{
			this.from_info = from_info ?? throw new ArgumentNullException(nameof(from_info));
			this.to_info = to_info ?? throw new ArgumentNullException(nameof(to_info));
			this.to_originfo = to_originfo ?? throw new ArgumentNullException(nameof(to_originfo));
		}

		public RGOLCalculationResult CalcRGOL(
			decimal? fromCuryAdjAmt,
			decimal? fromAdjAmt)
		{
			decimal? to_curyadjamt = Equals(from_info.CuryID, to_info.CuryID)
				? fromCuryAdjAmt
				: to_info.CuryConvCury(fromAdjAmt.Value);

			decimal? to_adjamt = Equals(from_info.CuryID, to_originfo.CuryID) &&
				Equals(from_info.CuryRate, to_originfo.CuryRate) &&
				Equals(from_info.CuryMultDiv, to_originfo.CuryMultDiv)
				? fromAdjAmt
				: to_originfo.CuryConvBase(to_curyadjamt.Value);

			return new RGOLCalculationResult
			{
				ToCuryAdjAmt = to_curyadjamt,
				RgolAmt = to_adjamt - fromAdjAmt
			};
		}				

		public RGOLCalculationResult CalcRGOL(
			decimal? applicationCuryBal,
			decimal? documentCuryBal,
			decimal? documentBaseBal,
			decimal? fromCuryAdjAmt,
			decimal? fromAdjAmt)
		{
			if (applicationCuryBal == 0m && fromCuryAdjAmt != 0m)
				return new RGOLCalculationResult
				{
					ToCuryAdjAmt = (decimal)documentCuryBal,
					RgolAmt = (decimal)documentBaseBal - (decimal)fromAdjAmt
				};
			else return CalcRGOL(fromCuryAdjAmt, fromAdjAmt);
		}
	}
}
