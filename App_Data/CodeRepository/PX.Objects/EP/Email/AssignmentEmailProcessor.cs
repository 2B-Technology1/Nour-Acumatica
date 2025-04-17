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
	public class AssignmentEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			CRSMEmail activity = package.Message;
			EMailAccount account = package.Account;

			if (activity.IsIncome != true
			    || activity.OwnerID != null
			    || activity.WorkgroupID != null
			    || activity.ClassID == CRActivityClass.EmailRouting)
				return false;

			bool assigned = false;
			if (account.DefaultEmailAssignmentMapID != null)
			{
				var processor = PXGraph.CreateInstance<EPAssignmentProcessor<CRSMEmail>>();
				assigned = processor.Assign(activity, account.DefaultEmailAssignmentMapID);
			}
			if (!assigned)
			{
				// last chance, no need to validate the new value, just set as-is
				package.Graph.Caches[typeof(CRSMEmail)].SetValue<CRSMEmail.ownerID>(activity, account.DefaultOwnerID);
				package.Graph.Caches[typeof(CRSMEmail)].SetValue<CRSMEmail.workgroupID>(activity, account.DefaultWorkgroupID);
			}

			return true;
		}
	}
}
