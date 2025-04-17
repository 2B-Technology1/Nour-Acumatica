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
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.GL
{
	public class GLHistoryPrimaryGraphAttribute : PXPrimaryGraphAttribute
	{
		private static readonly Type[] _possibleGraphTypes = { typeof(AccountByPeriodEnq), typeof(GLBudgetEntry) };

		public GLHistoryPrimaryGraphAttribute()
			: base(typeof(AccountByPeriodEnq))
		{
			Filter = typeof(AccountByPeriodFilter);
		}

		public override Type GetGraphType(PXCache cache, ref object row, bool checkRights, Type preferedType)
		{
			int? ledgerID = (int?)cache.GetValue<GLHistoryByPeriod.ledgerID>(row);

			if (ledgerID != null)
			{
				Ledger ledger = Ledger.PK.Find(cache.Graph, ledgerID);					
				if (ledger?.BalanceType == LedgerBalanceType.Budget)
				{
					Filter = typeof(BudgetFilter);
					return typeof(GLBudgetEntry);
				}
			}			
			return base.GetGraphType(cache, ref row, checkRights, preferedType);
		}

		public override IEnumerable<Type> GetAllGraphTypes() => _possibleGraphTypes;
	}
}
