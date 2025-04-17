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
using PX.Common;
using PX.Data;

namespace PX.Objects.CR.Wizard
{
	/// <summary>
	/// The aliases for <see cref="WebDialogResult"/> that are used inside the wizard.
	/// </summary>
	[PXInternalUseOnly]
	public static class WizardResult
	{
		/// <summary>
		/// The abort (Cancel) button.
		/// </summary>
		/// <remarks>
		/// Formally it is an abort button, but in the UI, it's name is Cancel.
		/// </remarks>
		public const WebDialogResult Abort = WebDialogResult.Abort;
		/// <summary>
		/// The Back button.
		/// </summary>
		public const WebDialogResult Back = (WebDialogResult)72; // just a random (high) enough number to avoid collision
	}
}
