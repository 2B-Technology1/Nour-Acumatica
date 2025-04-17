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

using System.Collections.Generic;

using PX.Data;

namespace PX.Objects.Common
{
	public static class PXSelectBaseExtensions
	{
		/// <summary>
		/// Returns a value indicating whether the view's Select method
		/// will return at least one record.
		/// </summary>
		/// <param name="select">The view to be checked.</param>
		/// <param name="parameters">
		/// The explicit values for such parameters as <see cref="Required{Field}"/>,
		/// <see cref="Optional{Field}"/>, and <see cref="Argument{ArgumentType}"/> that
		/// will be passed into the select method.
		/// </param>
		/// <returns>
		/// <c>true</c>, if the view's Select execution result contains at least one record,
		/// <c>false</c> otherwise.
		/// </returns>
		public static bool Any<T>(this PXSelectBase<T> select, params object[] parameters) 
			where T : class, IBqlTable, new()
		{
			return select.SelectWindowed(0, 1, parameters).Count > 0;
		}

		public static WebDialogResult Ask(
			this PXSelectBase select,
			string message,
			MessageButtons buttons,
			IReadOnlyDictionary<WebDialogResult, string> buttonNames)
		=> select.View.Ask(
			null,
			string.Empty,
			message,
			buttons,
			buttonNames,
			MessageIcon.None
		);

		public static WebDialogResult Ask(
			this PXSelectBase select,
			string header,
			string message,
			MessageButtons buttons,
			IReadOnlyDictionary<WebDialogResult, string> buttonNames)
		=> select.View.Ask(
			null,
			header,
			message,
			buttons,
			buttonNames,
			MessageIcon.None
		);

		public static WebDialogResult Ask(
			this PXSelectBase select,
			string header,
			string message,
			MessageButtons buttons,
			MessageIcon icon,
			IReadOnlyDictionary<WebDialogResult, string> buttonNames)
		=> select.View.Ask(
			null,
			header,
			message,
			buttons,
			buttonNames,
			icon
		);

		public static WebDialogResult Ask(
			this PXSelectBase select,
			string header,
			string message,
			MessageButtons buttons,
			MessageIcon icon,
			bool isRefreshRequired,
			IReadOnlyDictionary<WebDialogResult, string> buttonNames)
		=> select.View.Ask(
			null,
			header,
			message,
			buttons,
			buttonNames,
			icon,
			isRefreshRequired
		);

		public static WebDialogResult Ask(
			this PXSelectBase select,
			object row,
			string header,
			string message,
			MessageButtons buttons,
			MessageIcon icon,
			IReadOnlyDictionary<WebDialogResult, string> buttonNames)
		=> select.View.Ask(
			row,
			header,
			message,
			buttons,
			buttonNames,
			icon
		);

		public static WebDialogResult Ask(
			this PXSelectBase select,
			object row,
			string header,
			string message,
			MessageButtons buttons,
			MessageIcon icon,
			bool isRefreshRequired,
			IReadOnlyDictionary<WebDialogResult, string> buttonNames)
		=> select.View.Ask(
			row,
			header,
			message,
			buttons,
			buttonNames,
			icon,
			isRefreshRequired
		);
	}
}
