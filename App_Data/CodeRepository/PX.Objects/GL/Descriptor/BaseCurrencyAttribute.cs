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
using PX.Data;
using PX.Objects.CM;

namespace PX.Objects.GL
{
	[PXDBString(5, IsUnicode = true)]
	[PXSelector(typeof(Search<CurrencyList.curyID>))]
	[PXUIField(DisplayName = "Base Currency ID")]
	public class BaseCurrencyAttribute : PXAggregateAttribute
	{
		private Type _branchID;

		public BaseCurrencyAttribute(Type branchID)
		{
			_branchID = branchID;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, FieldDefaulting);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _branchID.Name, BranchID_FieldUpdated);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXFieldState state = (PXFieldState)sender.GetValueExt(e.Row, _branchID.Name);
			if (state != null && state.Value != null)
			{
				var branchID = (state.Value is int?) ? (int?)state.Value : PXAccess.GetBranchID(((string)state.Value).Trim());
				var branch = PXAccess.GetBranch(branchID);
				e.NewValue = branch?.BaseCuryID;
			}
			else
			{
				e.NewValue = null;
			}
		}

		public virtual void BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PXFieldState state = (PXFieldState)sender.GetValueExt(e.Row, _branchID.Name);
			if (state != null && state.Value != null)
			{
				var branchID = (state.Value is int?) ? (int?)state.Value : PXAccess.GetBranchID(((string)state.Value).Trim());
				var branch = PXAccess.GetBranch(branchID);
				sender.SetValueExt(e.Row, _FieldName, branch?.BaseCuryID);
			}
			else
			{
				sender.SetValueExt(e.Row, _FieldName, null);
			}
		}
	}
}
