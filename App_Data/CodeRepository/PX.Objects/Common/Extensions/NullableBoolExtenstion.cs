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
	public static class NullableBoolExtenstion
	{
		/// <summary>
		/// Returns <c>true</c> if the value is not null and is equal true. Owervise returns <c>false</c>.
		/// </summary>
		public static bool IsTrue(this bool? value) => value == true;

		/// <summary>
		/// Returns <c>true</c> if the value is not null and is equal false. Owervise returns <c>false</c>.
		/// </summary>
		public static bool IsFalse(this bool? value) => value == false;
	}
}
