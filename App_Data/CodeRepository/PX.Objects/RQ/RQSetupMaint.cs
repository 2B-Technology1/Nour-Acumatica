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
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.CS;

namespace PX.Objects.RQ
{
	public class RQSetupMaint : PXGraph<RQSetupMaint>
	{
		public PXSave<RQSetup> Save;
		public PXCancel<RQSetup> Cancel;		
		public PXSelect<RQSetup> Setup;
		public PXSelect<RQSetupApproval> SetupApproval;


		public CRNotificationSetupList<RQNotification> Notifications;
		public PXSelect<NotificationSetupRecipient,
			Where<NotificationSetupRecipient.setupID, Equal<Current<RQNotification.setupID>>>> Recipients;

		
		public RQSetupMaint()
		{
		}

		#region CacheAttached
		[PXDBString(10)]
		[PXDefault]
		[VendorContactType.ClassList]
		[PXUIField(DisplayName = "Contact Type")]
		[PXCheckDistinct(typeof(NotificationSetupRecipient.contactID),
			Where = typeof(Where<NotificationSetupRecipient.setupID, Equal<Current<NotificationSetupRecipient.setupID>>>))]
		public virtual void NotificationSetupRecipient_ContactType_CacheAttached(PXCache sender)
		{			
		}
		[PXDBInt]
		[PXUIField(DisplayName = "Contact ID")]
		[PXNotificationContactSelector(typeof(NotificationSetupRecipient.contactType),
			typeof(Search2<Contact.contactID,
				LeftJoin<EPEmployee,
							On<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>,
							And<EPEmployee.defContactID, Equal<Contact.contactID>>>>,
				Where<Current<NotificationSetupRecipient.contactType>, Equal<NotificationContactType.employee>,
							And<EPEmployee.acctCD, IsNotNull>>>))]
		public virtual void NotificationSetupRecipient_ContactID_CacheAttached(PXCache sender)
		{
		}
		#endregion				

		protected virtual void RQSetup_RequestApproval_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXCache cache = this.Caches[typeof(RQSetupApproval)];
			PXResultset<RQSetupApproval> setups = PXSelect<RQSetupApproval, Where<RQSetupApproval.type, Equal<RQType.requestItem>>>.Select(sender.Graph, null);
			foreach (RQSetupApproval setup in setups)
			{
				setup.IsActive = (bool?)e.NewValue;
				cache.Update(setup);
			}
		}

		protected virtual void RQSetup_RequisitionApproval_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXCache cache = this.Caches[typeof(RQSetupApproval)];
			PXResultset<RQSetupApproval> setups = PXSelect<RQSetupApproval, Where<RQSetupApproval.type, Equal<RQType.requisition>>>.Select(sender.Graph, null);
			foreach (RQSetupApproval setup in setups)
			{
				setup.IsActive = (bool?)e.NewValue;
				cache.Update(setup);
			}
		}

	}
}
