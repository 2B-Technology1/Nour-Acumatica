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

using PX.Objects.CM;

using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common
{
	/// <exclude/>
	public static class PXRounder
	{
		private const int TolerancePrecision = 12;

		public const int DefaultPrecision = 2;

		/// <exclude/>
		public enum SpreadType
		{
			Top,
			Bottom,
			Evenly,
			Random,
			First,
			Last,
			Flow
		}

		private static IEnumerable<int> GetAdjustedIndexes(SpreadType spreadType, int length, int countOfAdjusted, bool invertSpread)
		{
			// Contracts
			if (countOfAdjusted > length / 2)
			{
				throw new ArgumentOutOfRangeException(nameof(countOfAdjusted));
			}
			// end of contracts

			if (invertSpread)
			{
				switch (spreadType)
				{
					case SpreadType.Top:
						spreadType = SpreadType.Bottom;
						break;
					case SpreadType.Bottom:
						spreadType = SpreadType.Top;
						break;
					case SpreadType.First:
						spreadType = SpreadType.Last;
						break;
					case SpreadType.Last:
						spreadType = SpreadType.First;
						break;
				}
			}

			Random randomizer = new Random();
			HashSet<int> usedRandomIndexes = new HashSet<int>();
			for (int i = 0; i < countOfAdjusted; i++)
			{
				switch (spreadType)
				{
					case SpreadType.Top:
						yield return i;
						break;
					case SpreadType.Bottom:
						yield return length - i - 1;
						break;
					case SpreadType.Evenly:
						yield return (int)Round((decimal)length / countOfAdjusted * i, 0);
						break;
					case SpreadType.Random:
						int index;
						while (usedRandomIndexes.Contains(index = randomizer.Next(length))) { }
						usedRandomIndexes.Add(index);
						yield return index;
						break;
					case SpreadType.First:
						yield return 0;
						break;
					case SpreadType.Last:
						yield return length - 1;
						break;
					default:
						throw new NotImplementedException();
				}
			}
		}

		public static int GetBaseCuryPrecision() => CurrencyCollection.GetBaseCurrency()?.DecimalPlaces ?? DefaultPrecision;

		public static int GetCuryPrecision(string curyID) => CurrencyCollection.GetCurrency(curyID)?.DecimalPlaces ?? DefaultPrecision;

		public static decimal Round(decimal sourceValue, int precision)
		{
			return Math.Round(sourceValue, precision, MidpointRounding.AwayFromZero);
		}

		public static decimal RoundBaseCury(decimal sourceValue)
		{
			return Round(sourceValue, GetBaseCuryPrecision());
		}

		public static decimal RoundCury(decimal sourceValue, string curyID)
		{
			return Round(sourceValue, GetCuryPrecision(curyID));
		}

		public static IEnumerable<decimal> RoundBaseCury(
			this IEnumerable<decimal> sourceSequence,
			SpreadType spreadType)
		{
			return Round(sourceSequence, GetBaseCuryPrecision(), spreadType);
		}

		public static IEnumerable<decimal> RoundCury(
			this IEnumerable<decimal> sourceSequence,
			string curyID,
			SpreadType spreadType)
		{
			return Round(sourceSequence, GetCuryPrecision(curyID), spreadType);
		}

		public static IEnumerable<decimal> Round(
			this IEnumerable<decimal> sourceSequence, 
			int precision, 
			SpreadType spreadType)
		{
			List<decimal> result = new List<decimal>();

			decimal roundedSum = 0;
			decimal sumOfRounded = 0;
			decimal accumulatedDelta = 0;
			decimal absoluteAtom = (decimal)Math.Pow(10, -precision);
			decimal atom;

			foreach (decimal sourceValue in sourceSequence)
			{
				decimal roundedValue = Round(sourceValue, precision);

				roundedSum += sourceValue;
				sumOfRounded += roundedValue;

				if (spreadType == SpreadType.Flow)
				{
					if (Math.Abs(Round(accumulatedDelta, TolerancePrecision)) >= absoluteAtom)
					{
						atom = Math.Sign(accumulatedDelta) * absoluteAtom;
						roundedValue += atom;
						accumulatedDelta -= atom;
					}
				}
				result.Add(roundedValue);
			}

			if (spreadType == SpreadType.Flow)
			{
				return result;
			}
			
			roundedSum = Round(roundedSum, precision);

			decimal delta = roundedSum - sumOfRounded;
			if (Math.Abs(Round(delta, TolerancePrecision)) >= absoluteAtom)
			{
				atom = Math.Sign(delta) * absoluteAtom;
				int countOfAdjusted = (int)Round(delta / atom, 0);

				foreach (int index in GetAdjustedIndexes(spreadType, result.Count, countOfAdjusted, delta < 0m))
				{
					result[index] += atom;
				}
			}
			return result;
		}

		public static ICollection<TItem> Round<TItem>(
			this ICollection<TItem> items, 
			Func<TItem, decimal> getValue,
			Action<TItem, decimal> setValue,
			int precision, 
			SpreadType spreadType)
			where TItem: class
		{
			decimal roundedSum = 0;
			decimal sumOfRounded = 0;
			decimal accumulatedDelta = 0;
			decimal absoluteAtom = (decimal)Math.Pow(10, -precision);
			decimal atom;

			foreach (TItem sourceItem in items)
			{
				decimal sourceValue = getValue(sourceItem);
				decimal roundedValue = Round(sourceValue, precision);
				accumulatedDelta += sourceValue - roundedValue;

				roundedSum += sourceValue;
				sumOfRounded += roundedValue;

				if (spreadType == SpreadType.Flow)
				{
					if (Math.Abs(Round(accumulatedDelta, TolerancePrecision)) >= absoluteAtom)
					{
						atom = Math.Sign(accumulatedDelta) * absoluteAtom;
						roundedValue += atom;
						accumulatedDelta -= atom;
					}
				}
				setValue(sourceItem, roundedValue);
			}

			if (spreadType == SpreadType.Flow)
			{
				return items;
			}

			roundedSum = Round(roundedSum, precision);

			decimal delta = roundedSum - sumOfRounded;
			if (Math.Abs(Round(delta, TolerancePrecision)) >= absoluteAtom)
			{
				atom = Math.Sign(delta) * absoluteAtom;
				int countOfAdjusted = (int)Round(delta / atom, 0);

				foreach (int index in GetAdjustedIndexes(spreadType, items.Count, countOfAdjusted, delta < 0m))
				{
					TItem adjustingItem = items.ElementAt(index);
					setValue(adjustingItem, getValue(adjustingItem) + atom);
				}
			}
			return items;
		}
	}
}