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
using PX.Objects.CS;

namespace PX.Objects.CR
{
	public class CROpportunityContactAttribute : CRContactAttribute
	{
		public CROpportunityContactAttribute(Type SelectType)
			: base(typeof(CRContact.contactID), typeof(CRContact.isDefaultContact), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<CRContact.overrideContact>(Record_Override_FieldVerifying);
		}

		protected override (PXView, object[]) GetViewWithParameters(PXCache sender, object DocumentRow, object ContactRow)
		{
			PXView view = null;
			object parm = null;

			if (sender.GetValue<CROpportunity.contactID>(DocumentRow) != null)
			{
				parm = sender.GetValue<CROpportunity.contactID>(DocumentRow);

				BqlCommand Select = new SelectFrom<Contact>
					.LeftJoin<CRContact>
						.On<CRContact.bAccountID.IsEqual<Contact.bAccountID>
						.And<CRContact.bAccountContactID.IsEqual<Contact.contactID>>
						.And<CRContact.revisionID.IsEqual<Contact.revisionID>>
						.And<CRContact.isDefaultContact.IsEqual<boolTrue>>>
					.Where<
						Contact.contactID.IsEqual<@P.AsInt>>();

				view = sender.Graph.TypedViews.GetView(Select, false);
			}
			else if (sender.GetValue<CROpportunity.bAccountID>(DocumentRow) != null)
			{
				parm = sender.GetValue<CROpportunity.bAccountID>(DocumentRow);

				BqlCommand Select = new SelectFrom<Contact>
					.LeftJoin<BAccount>
						.On<BAccount.defContactID.IsEqual<Contact.contactID>>
					.LeftJoin<CRContact>
						.On<CRContact.bAccountID.IsEqual<Contact.bAccountID>
						.And<CRContact.bAccountContactID.IsEqual<Contact.contactID>>
						.And<CRContact.revisionID.IsEqual<Contact.revisionID>>
						.And<CRContact.isDefaultContact.IsEqual<boolTrue>>>
					.Where<
						BAccount.bAccountID.IsEqual<@P.AsInt>>();

				view = sender.Graph.TypedViews.GetView(Select, false);
			}
			else if (sender.GetValue<CROpportunity.locationID>(DocumentRow) != null)
			{
				parm = sender.GetValue<CROpportunity.locationID>(DocumentRow);

				BqlCommand Select = new SelectFrom<Contact>
					.LeftJoin<Location>
						.On<Location.defContactID.IsEqual<Contact.contactID>>
					.LeftJoin<CRContact>
						.On<CRContact.bAccountID.IsEqual<Contact.bAccountID>
						.And<CRContact.bAccountContactID.IsEqual<Contact.contactID>>
						.And<CRContact.revisionID.IsEqual<Contact.revisionID>>
						.And<CRContact.isDefaultContact.IsEqual<boolTrue>>>
					.Where<
						Location.locationID.IsEqual<@P.AsInt>>();

				view = sender.Graph.TypedViews.GetView(Select, false);
			}

			return (view, new[] { parm });
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<CRContact, CRContact.contactID>(sender, DocumentRow, Row);            
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<CRContact, CRContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<CRContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}
	}
}