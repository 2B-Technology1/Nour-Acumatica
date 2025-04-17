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

using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.CS
{
	/// <summary>
	/// Visibility Restriction utilits
	/// </summary>
	public static class VisibilityRestriction
	{
		/// <summary>
		/// Default BAccountID, that means, that visibility restriction is not set.
		/// </summary>
		public const int EmptyBAccountID = 0;

		/// <summary>
		/// Checks, if BAccountID, that is set as 'Restrict Visibility To' for some entity is empty (or default).
		/// </summary>
		/// <param name="restrictVisibilityToBAccountID"></param>
		/// <returns></returns>
		public static bool IsEmpty(int? restrictVisibilityToBAccountID)
		{
			return restrictVisibilityToBAccountID == null || restrictVisibilityToBAccountID == EmptyBAccountID;
		}

		/// <summary>
		/// Checks, if BAccountID, that is set as 'Restrict Visibility To' for some entity is not empty (or default).
		/// </summary>
		/// <param name="restrictVisibilityToBAccountID"></param>
		/// <returns></returns>
		public static bool IsNotEmpty(int? restrictVisibilityToBAccountID)
		{
			return !IsEmpty(restrictVisibilityToBAccountID);
		}
	}
}