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
using PX.Data.WorkflowAPI;

namespace PX.Objects.CR.Workflows
{
	internal static class WorkflowExtensions
	{
		public static BoundedTo<TGraph, TPrimary>.FieldState.IAllowOptionalConfig IsDisabled<TGraph, TPrimary>(
			this BoundedTo<TGraph, TPrimary>.FieldState.IAllowOptionalConfig config,
			bool disabled)
			where TGraph : PXGraph
			where TPrimary : class, IBqlTable, new()
		{
			return disabled ? config.IsDisabled() : config;
		}

		public static BoundedTo<TGraph, TPrimary>.ActionState.IAllowOptionalConfig IsDuplicatedInToolbar<TGraph, TPrimary>(
			this BoundedTo<TGraph, TPrimary>.ActionState.IAllowOptionalConfig config,
			bool duplicated)
			where TGraph : PXGraph
			where TPrimary : class, IBqlTable, new()
		{
			return duplicated ? config.IsDuplicatedInToolbar() : config;
		}
	}
}