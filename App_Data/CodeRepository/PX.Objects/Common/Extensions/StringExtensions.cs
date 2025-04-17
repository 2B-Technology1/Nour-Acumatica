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

namespace PX.Objects.Common.Extensions
{
	public static class StringExtensions
	{
		public static string Capitalize(this string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			else
			{
				return char.ToUpper(text[0]) + text.Substring(1);
			}
		}

		/// <summary>
		/// Removes all leading occurrences of whitespace from the current string
		/// object. Does not throw an exception if string is null.
		/// </summary>
		/// <param name="str">The string object to remove leading whitespace from. Can safely be null.</param>
		/// <returns>The string that remains after all occurrences of leading whitespace are removed 
		/// from <paramref name="str"/>, or null if <paramref name="str"/> was null itself.</returns>
		public static string SafeTrimStart(this string str)
        {
            if (str == null)
                return str;

            return str.TrimStart();
        }
    }
}