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

namespace PX.Objects.CR.Wizard
{
	/// <summary>
	/// The scope that shows that the current execution stack is inside an action that is triggered from the wizard.
	/// </summary>
	/// <remarks>
	/// The class can be used by actions that can be triggered from the wizard and outside the wizard.
	/// </remarks>
	[PXInternalUseOnly]
	public class WizardScope : IDisposable
	{
		private readonly bool prevState = false;
		private const string _WizardScope_ = nameof(_WizardScope_);

		public static bool IsScoped
		{
			get => PXContext.GetSlot<bool>(_WizardScope_);
		}

		public WizardScope()
		{
			prevState = IsScoped;

			PXContext.SetSlot<bool>(_WizardScope_, true);
		}

		public void Dispose()
		{
			PXContext.SetSlot<bool>(_WizardScope_, prevState);
		}
	}
}
