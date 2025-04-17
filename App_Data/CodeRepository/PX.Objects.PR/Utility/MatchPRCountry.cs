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
using PX.Data.SQLTree;
using PX.Objects.CS;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class MatchPRCountry<CountryIDField> : Data.BQL.BqlChainableConditionLite<MatchPRCountry<CountryIDField>>, IBqlUnary
			where CountryIDField : IBqlOperand, IBqlField
	{
		/// <exclude/>
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			result = Verify(cache, item);
		}

		public static bool Verify(PXCache cache, object item)
		{
			string payrollCountry = PRCountryAttribute.GetPayrollCountry();
			return payrollCountry.Equals(cache.GetValue<CountryIDField>(item));
		}

		/// <exclude/>
		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			if (graph == null || !info.BuildExpression || !info.Tables.Contains(typeof(CountryIDField).DeclaringType))
			{
				return true;
			}

			SQLExpression fieldExpression = BqlCommand.GetSingleExpression(typeof(CountryIDField), graph, info.Tables, selection, BqlCommand.FieldPlace.Condition);

			exp = fieldExpression.EQ(PRCountryAttribute.GetPayrollCountry());
			return true;
		}
	}
}
