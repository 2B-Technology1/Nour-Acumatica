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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.CR
{
	public class CRBillingContactAttribute : CRContactAttribute
	{
		public CRBillingContactAttribute(Type SelectType)
			: base(typeof(CRBillingContact.contactID), typeof(CRBillingContact.isDefaultContact), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<CRBillingContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<CRBillingContact, CRBillingContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<CRBillingContact, CRBillingContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var newValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Justification]
			Contact_IsDefaultContact_FieldVerifying<CRBillingContact>(sender, new PXFieldVerifyingEventArgs(e.Row, newValue, e.ExternalCall));
		}

		protected override (PXView, object[]) GetViewWithParameters(PXCache sender, object DocumentRow, object ContactRow)
		{
			PXView view = null;
			object[] parm = null;

			if (sender.GetValue<CROpportunity.bAccountID>(DocumentRow) != null)
			{
				var id = sender.GetValue<CROpportunity.bAccountID>(DocumentRow);
				parm = new object[] { id, id };

				BqlCommand Select = new SelectFrom<Contact>
					.InnerJoin<BAccount>
						.On<BAccount.bAccountID.IsEqual<Contact.bAccountID>>
					.LeftJoin<Customer>
						.On<Customer.bAccountID.IsEqual<Contact.bAccountID>
						.And<Customer.defBillContactID.IsEqual<Contact.contactID>>>
					.LeftJoin<CRBillingContact>
						.On<CRBillingContact.bAccountID.IsEqual<Contact.bAccountID>
						.And<CRBillingContact.bAccountContactID.IsEqual<Contact.contactID>>
						.And<CRBillingContact.revisionID.IsEqual<Contact.revisionID>>
						.And<CRBillingContact.isDefaultContact.IsEqual<True>>>
					.Where<
						Customer.bAccountID.IsEqual<@P.AsInt>
						.Or<BAccount.defContactID.IsEqual<Contact.contactID>>
							.And<BAccount.bAccountID.IsEqual<@P.AsInt>>>();

				view = sender.Graph.TypedViews.GetView(Select, false);
			}

			return (view, parm);
		}
	}
}