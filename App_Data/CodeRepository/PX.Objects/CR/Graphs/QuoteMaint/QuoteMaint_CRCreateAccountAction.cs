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
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CRCreateActions;

namespace PX.Objects.CR.QuoteMaint_Extensions
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class QuoteMaint_CRCreateBothContactAndAccountAction : CRCreateBothContactAndAccountAction<QuoteMaint, CRQuote, QuoteMaint_CRCreateAccountAction, QuoteMaint_CRCreateContactAction> { }

	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class QuoteMaint_CRCreateAccountAction : CRCreateAccountAction<QuoteMaint, CRQuote>
	{
		#region Initialization

		protected override string TargetType => CRTargetEntityType.CRQuote;

		public override void Initialize()
		{
			base.Initialize();

			Addresses = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentAddress>(Base.Quote_Address);
			Contacts = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentContact>(Base.Quote_Contact);
		}

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(CRQuote)) { RefContactID = typeof(CRQuote.contactID) };
		}
		protected override DocumentContactMapping GetDocumentContactMapping()
		{
			return new DocumentContactMapping(typeof(CRContact));
		}
		protected override DocumentAddressMapping GetDocumentAddressMapping()
		{
			return new DocumentAddressMapping(typeof(CRAddress));
		}

		protected override PXSelectBase<CRPMTimeActivity> Activities => Base.GetExtension<QuoteMaint_ActivityDetailsExt>().Activities;

		#endregion

		#region Events

		protected virtual void _(Events.FieldDefaulting<AccountsFilter, AccountsFilter.accountClass> e)
		{
			if (ExistingAccount.SelectSingle() is BAccount existingAccount)
			{
				e.NewValue = existingAccount.ClassID;
				e.Cancel = true;

				return;
			}

			e.NewValue = Base.Setup.Current?.DefaultCustomerClassID;

			e.Cancel = true;
		}

		protected override void _(Events.RowSelected<AccountsFilter> e)
		{
			base._(e);

			AccountsFilter row = e.Row as AccountsFilter;
			if (row == null)
				return;

			CRQuote quote = Base.Quote.Current;
			if (quote.ContactID != null)
			{
				PXUIFieldAttribute.SetVisible<AccountsFilter.linkContactToAccount>(e.Cache, row, true);
				Contact contact = Base.CurrentContact.Current ?? Base.CurrentContact.SelectSingle();
				if (contact == null)
				{
					PXUIFieldAttribute.SetEnabled<AccountsFilter.linkContactToAccount>(e.Cache, row, false);
				}
				else
				{
					if (contact.BAccountID != null)
					{
						PXUIFieldAttribute.SetWarning<AccountsFilter.linkContactToAccount>(e.Cache, row, Messages.AccountContactValidation);
					}
					else
					{
						PXUIFieldAttribute.SetEnabled<AccountsFilter.linkContactToAccount>(e.Cache, row, true);
					}
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<AccountsFilter, AccountsFilter.linkContactToAccount> e)
		{
			AccountsFilter row = e.Row as AccountsFilter;
			if (row == null)
				return;

			CRQuote quote = Base.Quote.Current;
			if (quote.ContactID != null)
			{
				Contact contact = Base.CurrentContact.Current ?? Base.CurrentContact.SelectSingle();
				if (contact == null)
				{
					e.NewValue = false;
				}
				else
				{
					if (contact.BAccountID != null)
					{
						e.NewValue = false;
					}
					else
					{
						e.NewValue = true;
					}
				}
			}
			else
			{
				e.NewValue = false;
			}

			e.Cancel = true;
		}

		#endregion
	}
}
