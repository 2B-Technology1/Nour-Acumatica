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
using PX.Objects.CM.Extensions;
using System;

namespace PX.Objects.PM
{
	public sealed class ProjectSummaryBudget : PXCacheExtension<PMBudget>
	{
		public static bool IsActive()
		{
			return PXContext.GetScreenID()?.Replace(".", string.Empty) == "PMGI0025";
		}

		public abstract class restrictedCoAmount : PX.Data.BQL.BqlDecimal.Field<restrictedCoAmount> { }

		#region RestrictedCoAmount
		[PXBaseCury]
		[PXUIField(DisplayName = "Budgeted CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		[PXDBCalced(typeof(Switch<Case<Where<PMBudget.type, Equal<GL.AccountType.income>>, PMBudget.changeOrderAmount>, Zero>), typeof(decimal))]
		public decimal? RestrictedCoAmount
		{
			get;
			set;
		}
		#endregion
	}
}
