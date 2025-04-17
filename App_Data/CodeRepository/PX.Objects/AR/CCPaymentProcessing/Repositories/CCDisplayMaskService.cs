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

using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PX.Objects.AR.Repositories
{
	public class CCDisplayMaskService : ICCDisplayMaskService
	{
		private static readonly Regex parseCardNum = new Regex("[\\d]+", RegexOptions.Compiled);

		private const string MaskedCardTmpl = "****-****-****-";
		private const char CS_UNDERSCORE = '_';
		private const char CS_DASH = '-';
		private const char CS_DOT = '.';
		private const char CS_MASKER = '*';
		private const char CS_NUMBER_MASK_0 = '#';
		private const char CS_NUMBER_MASK_1 = '0';
		private const char CS_NUMBER_MASK_2 = '9';
		private const char CS_ANY_CHAR_0 = '&';
		private const char CS_ANY_CHAR_1 = 'C';
		private const char CS_ALPHANUMBER_MASK_0 = 'a';
		private const char CS_ALPHANUMBER_MASK_1 = 'A';
		private const char CS_ALPHA_MASK_0 = 'L';
		private const char CS_ALPHA_MASK_1 = '?';

		public virtual string UseDefaultMaskForCardNumber(string cardNbr, string cardType)
		{
			string ret = null;
			Match match = parseCardNum.Match(cardNbr);
			if (match.Success)
			{
				if (cardType == null)
				{
					ret = MaskedCardTmpl + match.Value;
				}
				else
				{
					ret = cardType.Trim() + ":" + MaskedCardTmpl + match.Value;
				}
			}
			return ret;
		}



		public virtual string UseDisplayMaskForCardNumber(string aID, string aDisplayMask)
		{
			if (string.IsNullOrEmpty(aID) || string.IsNullOrEmpty(aDisplayMask) || string.IsNullOrEmpty(aDisplayMask.Trim())) return aID;
			int mskLength = aDisplayMask.Length;
			int valueLength = aID.Length;
			char[] displayMask = aDisplayMask.ToCharArray();
			char[] value = aID.ToCharArray();
			int valueIndex = 0;
			StringBuilder res = new StringBuilder(mskLength);
			for (int i = 0; i < mskLength; i++)
			{
				if (valueIndex >= valueLength) break;
				if (IsSymbol(displayMask[i]))
				{
					res.Append(value[valueIndex]);
					valueIndex++;
				}
				else
				{
					//Any other characters are treated as separator and are omited
					if (IsSeparator(displayMask[i]))
					{
						res.Append(displayMask[i]);
					}
					else
					{
						res.Append(CS_MASKER);
						valueIndex++;
					}
				}
			}
			return res.ToString();
		}

		public virtual string UseAdjustedDisplayMaskForCardNumber(string aID, string aDisplayMask)
		{
			string adjustedID = aID;
			int maskSymbolsLength = aDisplayMask.ToArray().Where(symbol => !IsSeparator(symbol)).Count();
			if (aID.Length > maskSymbolsLength)
			{
				adjustedID = aID.Substring(aID.Length - maskSymbolsLength);
			}
			else if (aID.Length < maskSymbolsLength)
			{
				adjustedID = aID.PadLeft(maskSymbolsLength, '0');
			}

			return UseDisplayMaskForCardNumber(adjustedID, aDisplayMask);
		}

		private static bool IsSeparator(char aCh)
		{
			return aCh == CS_UNDERSCORE || aCh == CS_DASH || aCh == CS_DOT;
		}
		private static bool IsSymbol(char aCh)
		{
			switch (aCh)
			{
				case CS_NUMBER_MASK_0:
				case CS_NUMBER_MASK_1:
				case CS_NUMBER_MASK_2:
				case CS_ALPHANUMBER_MASK_0:
				case CS_ALPHANUMBER_MASK_1:
				case CS_ANY_CHAR_0:
				case CS_ANY_CHAR_1:
				case CS_ALPHA_MASK_0:
				case CS_ALPHA_MASK_1:
					return true;
			}
			return false;
		}
	}
}
