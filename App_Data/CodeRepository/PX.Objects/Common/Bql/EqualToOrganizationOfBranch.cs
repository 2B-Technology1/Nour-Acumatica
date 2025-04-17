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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.SQLTree;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.Common.Bql
{
	public class EqualToOrganizationOfBranch<TOrganizationIDField, TBranchIDParameter> : IBqlUnary
		where TOrganizationIDField : IBqlOperand
		where TBranchIDParameter : IBqlParameter, new()
	{
		private IBqlParameter branchIDParameter;
		protected IBqlParameter BranchIDParameter => LazyInitializer.EnsureInitialized<IBqlParameter>(ref branchIDParameter, () => new TBranchIDParameter());

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			value = BqlHelper.GetOperandValue<TOrganizationIDField>(cache, item);

			result = (int?) value == GetOrganizationID(cache.Graph);

			if (pars != null)
			{
				pars.Add(BqlHelper.GetParameterValue(cache.Graph, BranchIDParameter));
			}
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
		    if (graph == null || !info.BuildExpression)
		        return true;

            SQLExpression fld = BqlCommand.GetSingleExpression(typeof(TOrganizationIDField), graph, info.Tables, selection, BqlCommand.FieldPlace.Condition);

			int? organizationID = GetOrganizationID(graph);

			exp = fld.EQ(organizationID);

			return true;
		}

		protected int? GetOrganizationID(PXGraph graph)
		{
			int? branchID = (int?)BqlHelper.GetParameterValue(graph, BranchIDParameter);

			return PXAccess.GetParentOrganizationID(branchID);
		}
	}
}
