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
using PX.Objects.CS;

namespace PX.Objects.Common
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = true)]
	public class InterBranchRestrictorAttribute : PXRestrictorAttribute
	{
		static readonly Type EmptyWhere = typeof(Where<True, Equal<True>>);

		protected Type _interBranchWhere;

		public InterBranchRestrictorAttribute(Type where)
			: base(EmptyWhere, Messages.InterBranchFeatureIsDisabled)
		{
			_interBranchWhere = where;
		}

		protected override BqlCommand WhereAnd(PXCache sender, PXSelectorAttribute selattr, Type Where)
		{
			Type newWhere = IsReportOrInterBranchFeatureEnabled(sender) ? EmptyWhere : _interBranchWhere;

			return base.WhereAnd(sender, selattr, newWhere);
		}

		private bool IsReportOrInterBranchFeatureEnabled(PXCache sender)
		{
			return IsReportGraph(sender.Graph) || PXAccess.FeatureInstalled<FeaturesSet.interBranch>();
		}

		protected bool IsReportGraph(PXGraph graph) => graph.GetType() == typeof(PXGraph);
	}
}
