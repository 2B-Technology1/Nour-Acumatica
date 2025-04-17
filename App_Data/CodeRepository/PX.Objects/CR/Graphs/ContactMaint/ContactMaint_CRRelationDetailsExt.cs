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

namespace PX.Objects.CR.ContactMaint_Extensions
{
	/// <inheritdoc/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ContactMaint_CRRelationDetailsExt : CRRelationDetailsExt<ContactMaint, Contact, Contact.noteID>
	{
		#region Events

		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRRelation.contactID> e) { }

		protected virtual void _(Events.RowSelected<Contact> e)
		{
			if (e.Row == null)
				return;

			var isNotInserted = e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted;

			Relations.Cache.AllowInsert = e.Row.ContactType == ContactTypesAttribute.Person && isNotInserted;
			Relations.Cache.AllowDelete = e.Row.ContactType == ContactTypesAttribute.Person;
		}

		#endregion
	}
}
