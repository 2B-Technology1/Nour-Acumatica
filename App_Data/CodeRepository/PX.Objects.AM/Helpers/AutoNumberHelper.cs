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

namespace PX.Objects.AM
{
    public static class AutoNumberHelper
    {
        public static string NextNumber(string str)
        {
            return NextNumber(str, 1);
        }

        public static string NextNumber(string str, int count)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            var sb = new System.Text.StringBuilder();
            int remainder = count;

            foreach (var c in str.Reverse().ToCharArray())
            {
                if (remainder == 0 || !char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                    continue;
                }

                sb.Append(IncreaseChar(c, remainder, out remainder));
            }

            if (remainder > 0 && sb.Length > 0)
            {
                var currentStr = sb.ToString();
                var lastChar = currentStr[currentStr.Length - 1];
                sb.Append(char.IsLetter(lastChar) ? 'A' : '1');
            }

            return sb.ToString().Reverse();
        }

        private static char IncreaseChar(char c, int remainderIn, out int remainderOut)
        {
            var nextC = IncreaseChar(c, remainderIn);

            if (char.IsLetter(c))
            {
                if (nextC > 'Z')
                {
                    remainderOut = nextC - 'Z';
                    var baseInc = remainderOut > 1 ? remainderOut - 1 : 0;
                    remainderOut -= baseInc;
                    return IncreaseChar('A', baseInc);
                }

                remainderOut = 0;
                return nextC;
            }

            if (char.IsDigit(c))
            {
                if (nextC > '9')
                {
                    remainderOut = nextC - '9';
                    var baseInc = remainderOut > 1 ? remainderOut - 1 : 0;
                    remainderOut -= baseInc;
                    return IncreaseChar('0', baseInc);
                }

                remainderOut = 0;
                return nextC;
            }

            remainderOut = remainderIn;
            return nextC;
        }

        private static char IncreaseChar(char c, int count)
        {
            return (char)(c + count);
        }
    }
}