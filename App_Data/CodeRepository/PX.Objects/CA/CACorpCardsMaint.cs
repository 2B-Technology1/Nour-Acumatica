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
using PX.Objects.EP.DAC;

namespace PX.Objects.CA
{
	public class CACorpCardsMaint : PXGraph<CACorpCardsMaint, CACorpCard>
	{
		#region Repo

		public static CashAccount GetCardCashAccount(PXGraph graph, int? corpCardID)
		{
			return PXSelectJoin<CashAccount,
						InnerJoin<CACorpCard,
							On<CashAccount.cashAccountID, Equal<CACorpCard.cashAccountID>>>,
						Where<CACorpCard.corpCardID, Equal<Required<CACorpCard.corpCardID>>>>
						.Select(graph, corpCardID);
		}

		#endregion

		public PXSelect<CACorpCard> CreditCards;
		public PXSelect<EPEmployeeCorpCardLink, Where<EPEmployeeCorpCardLink.corpCardID, Equal<Current<CACorpCard.corpCardID>>>> EmployeeLinks;

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(CACorpCard.corpCardID))]
		protected virtual void _(Events.CacheAttached<EPEmployeeCorpCardLink.corpCardID> e)
		{
		}
		
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.FieldUpdated<CACorpCard, CACorpCard.branchID> e)
		{
			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<CACorpCard.cashAccountID>(e.Row);
			}
		}
	}
}
