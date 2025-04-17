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
	public class QuoteMaint_CRCreateContactAction : CRCreateContactAction<QuoteMaint, CRQuote>
	{
		#region Initialization

		protected override string TargetType => CRTargetEntityType.CRQuote;

		public override void Initialize()
		{
			base.Initialize();

			Addresses = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentAddress>(Base.Quote_Address);
			Contacts = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentContact>(Base.Quote_Contact);
			ContactMethod = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentContactMethod>(Base.Quote_Contact);
		}

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(CRQuote)) { RefContactID = typeof(CRQuote.contactID) };
		}
		protected override DocumentContactMapping GetDocumentContactMapping()
		{
			return new DocumentContactMapping(typeof(CRContact));
		}
		protected override DocumentContactMethodMapping GetDocumentContactMethodMapping()
		{
			return new DocumentContactMethodMapping(typeof(CRContact));
		}
		protected override DocumentAddressMapping GetDocumentAddressMapping()
		{
			return new DocumentAddressMapping(typeof(CRAddress));
		}

		protected override PXSelectBase<CRPMTimeActivity> Activities => Base.GetExtension<QuoteMaint_ActivityDetailsExt>().Activities;

		#endregion

		#region Events

		protected virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.contactClass> e)
		{
			if (ExistingContact.SelectSingle() is Contact existingContact)
			{
				e.NewValue = existingContact.ClassID;
				e.Cancel = true;

				return;
			}

			CRQuote quote = Base.Quote.Current;
			if (quote == null)
				return;

			e.NewValue = Base.Setup.Current?.DefaultContactClassID;

			e.Cancel = true;
		}

		public virtual void _(Events.RowSelected<ContactFilter> e)
		{
			bool isBAccountSelected = (Base?.Quote?.Current?.BAccountID != null);
			PXUIFieldAttribute.SetReadOnly<ContactFilter.fullName>(e.Cache, e.Row, isBAccountSelected);
		}

		#endregion

		#region Overrides

		protected override void MapContactMethod(DocumentContactMethod source, Contact target)
		{
		}

		protected override object GetDefaultFieldValueFromCache<TExistingField, TField>()
		{
			if (typeof(TExistingField) == typeof(Contact.fullName)
				|| Base.Quote.Current?.BAccountID == null
				|| Base.Quote.Current?.AllowOverrideContactAddress == true)
			{
				return base.GetDefaultFieldValueFromCache<TExistingField, TField>();
			}
			return null;
		}

		#endregion
	}
}
