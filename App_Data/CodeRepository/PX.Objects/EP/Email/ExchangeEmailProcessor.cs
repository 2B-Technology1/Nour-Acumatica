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
using PX.SM;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	public class ExchangeEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			CRSMEmail activity = package.Message;
			EMailAccount account = package.Account;
			PXGraph graph = package.Graph;

			if (account.EmailAccountType != EmailAccountTypesAttribute.Exchange) return false;


			foreach (EPEmployee employee in PXSelectJoin<EPEmployee,
				InnerJoin<EMailSyncAccount, On<EPEmployee.bAccountID, Equal<EMailSyncAccount.employeeID>>>,
				Where<EMailSyncAccount.emailAccountID, Equal<Required<EMailSyncAccount.emailAccountID>>>>.Select(graph, account.EmailAccountID))
			{
				if (employee.UserID != null)
				{
					activity.OwnerID = employee.DefContactID;
					return true;
				}
			}

			return false;
		}
	}
}
