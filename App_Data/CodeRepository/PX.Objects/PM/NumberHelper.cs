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

namespace PX.Objects.PM
{
	internal static class NumberHelper
	{
		internal static string IncreaseNumber(string number, int increaseValue)
		{
			int lastNumericDigitsWithZeroCount;
			long currentNumericValue = GetNumericValue(number, out lastNumericDigitsWithZeroCount);

			long newNumber = currentNumericValue + increaseValue;
			string newNumberString = newNumber.ToString();

			string textPart = string.Empty;
			if (number.Length - lastNumericDigitsWithZeroCount > 0)
			{
				textPart = number.Substring(0, number.Length - lastNumericDigitsWithZeroCount);
			}

			string zeroPart = string.Empty;
			if (newNumberString.Length < lastNumericDigitsWithZeroCount)
			{
				zeroPart = new string((char)48, lastNumericDigitsWithZeroCount - newNumberString.Length);
			}

			return string.Concat(textPart, zeroPart, newNumberString);
		}

		internal static string DecreaseNumber(string number, int decreaseValue)
		{
			int lastNumericDigitsWithZeroCount;
			long currentNumericValue = GetNumericValue(number, out lastNumericDigitsWithZeroCount);

			long newNumber = currentNumericValue - decreaseValue;
			if (newNumber < 0)
			{
				newNumber = 0;
			}
			string newNumberString = newNumber.ToString();

			string textPart = string.Empty;
			if (number.Length - lastNumericDigitsWithZeroCount > 0)
			{
				textPart = number.Substring(0, number.Length - lastNumericDigitsWithZeroCount);
			}

			string zeroPart = string.Empty;
			if (newNumberString.Length < lastNumericDigitsWithZeroCount)
			{
				zeroPart = new string((char)48, lastNumericDigitsWithZeroCount - newNumberString.Length);
			}

			return string.Concat(textPart, zeroPart, newNumberString);
		}

		internal static string GetTextPrefix(string number)
		{
			int lastNumericDigitsWithZeroCount;
			long currentNumericValue = GetNumericValue(number, out lastNumericDigitsWithZeroCount);

			string textPart = string.Empty;
			if (number.Length - lastNumericDigitsWithZeroCount > 0)
			{
				textPart = number.Substring(0, number.Length - lastNumericDigitsWithZeroCount);
			}

			return textPart;
		}

		internal static long GetNumericValue(string number)
		{
			int lastNumericDigitsWithZeroCount;
			long numericValue = GetNumericValue(number, out lastNumericDigitsWithZeroCount);
			return numericValue;
		}
		
		private static long GetNumericValue(string number, out int lastNumericDigitsWithZeroCount)
		{
			lastNumericDigitsWithZeroCount = 0;

			int lastNumericDigitsCount = 0;
			for (int i = number.Length - 1; i >= 0; i--)
			{
				int symbolCode = number[i];
				if (symbolCode == 48)
				{
					lastNumericDigitsWithZeroCount++;
				}
				else if (symbolCode >= 49 && symbolCode <= 57)
				{
					lastNumericDigitsWithZeroCount++;
					lastNumericDigitsCount = lastNumericDigitsWithZeroCount;
				}
				else
				{
					break;
				}
			}

			long currentNumber = 0;
			if (lastNumericDigitsCount > 0)
			{
				currentNumber = long.Parse(number.Substring(number.Length - lastNumericDigitsCount, lastNumericDigitsCount));
			}

			return currentNumber;
		}
	}
}
