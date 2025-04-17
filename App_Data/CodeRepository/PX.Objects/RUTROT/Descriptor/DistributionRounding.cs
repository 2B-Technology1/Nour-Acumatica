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

using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CM;

namespace PX.Objects.RUTROT
{
	public class DistributionRounding
	{
		private decimal? _precision;
		private string _rounding;

		public int CuryPlaces
		{
			get;
			set;
		}

		public bool PreventOverflow
		{
			get;
			set;
		}

		public decimal Round(decimal amount, int curyPlaces)
		{
			if (this._rounding == RoundingType.Currency)
			{
				return decimal.Round(amount, curyPlaces);
			}
			else
			{
				return ARReleaseProcess.RoundAmount(amount, this._rounding, this._precision).Value;
			}
		}

		public decimal? Round(decimal? amount, int curyPlaces)
		{
			if (amount == null)
			{
				return null;
			}

			return Round(amount.Value, curyPlaces);
		}

		public decimal? Round(decimal? amount)
		{
			return Round(amount, CuryPlaces);
		}

		public decimal Round(decimal amount)
		{
			return Round(amount, CuryPlaces);
		}

		public IEnumerable<decimal> DistributeEven(decimal? total, int count)
		{
			if (total == null)
			{
				return null;
			}

			decimal forPerson = Round(total.Value / count);

			var distributed = Enumerable.Repeat(forPerson, count - 1).ToList();
			distributed.Add(FinishDistribution(total.Value, distributed));

			return distributed;
		}

		public IEnumerable<decimal> DistributeInShares(decimal? total, IEnumerable<decimal> shares)
		{
			if (total == null)
			{
				return null;
			}

			var distribution = new List<decimal>();

			for (int i = 0; i < shares.Count() - 1; i++)
			{
				decimal s = shares.ElementAt(i);
				distribution.Add(Round(s * total.Value));
			}

			distribution.Add(FinishDistribution(total.Value, distribution));

			return distribution;
		}

		private decimal FinishDistribution(decimal total, IEnumerable<decimal> distributed)
		{
			decimal rounded = Round(total - distributed.Sum());
			decimal sum = distributed.Sum();

			if (PreventOverflow == false || sum + rounded <= total)
			{
				return rounded;
			}
			else
			{
				decimal step = FinishStep;

				decimal difference = rounded + sum - total;
				decimal overLimit = Math.Ceiling(difference / step) * step;
				return rounded - overLimit;
			}
		}

		public decimal FinishStep
		{
			get
			{
				if (this._rounding == RoundingType.Currency)
				{
					return CuryPlaces == 0 ? 1.0m : (decimal)Math.Pow(0.1, CuryPlaces);
				}
				else
				{
					return this._precision.Value;
				}
			}
		}

		#region Ctors
		public DistributionRounding(ARSetup setup, bool roundingEnabled)
			: this(roundingEnabled ? setup.InvoiceRounding : RoundingType.Currency, roundingEnabled ? setup.InvoicePrecision : 0.0m)
		{
		}

        public DistributionRounding(Currency currency, bool roundingEnabled)
            : this(roundingEnabled ? currency.ARInvoiceRounding : RoundingType.Currency, roundingEnabled ? currency.ARInvoicePrecision : 0.0m)
        {
        }

        public DistributionRounding(string rounding, decimal? precision)
		{
			this._rounding = rounding;
			this._precision = precision;
			PreventOverflow = false;
			CuryPlaces = 0;
		}
		#endregion
	}
}
