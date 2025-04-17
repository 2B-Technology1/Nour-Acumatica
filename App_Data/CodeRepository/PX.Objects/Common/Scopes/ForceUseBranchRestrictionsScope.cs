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

using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Common.Scopes
{
	public sealed class ForceUseBranchRestrictionsScope : IDisposable
	{
		private class BoolWrapper
		{
			public bool Value { get; set; }
			public BoolWrapper() { this.Value = false; }
		}

		private static string _SLOT_KEY = "ForceUseBranchRestrictionsScope_Running";

		public ForceUseBranchRestrictionsScope()
		{
			BoolWrapper val = PXDatabase.GetSlot<BoolWrapper>(_SLOT_KEY);
			val.Value = true;
		}

		public void Dispose()
		{
			PXDatabase.ResetSlot<BoolWrapper>(_SLOT_KEY);
		}

		public static bool IsRunning
		{
			get
			{
				return PXDatabase.GetSlot<BoolWrapper>(_SLOT_KEY).Value;
			}
		}
	}
}
