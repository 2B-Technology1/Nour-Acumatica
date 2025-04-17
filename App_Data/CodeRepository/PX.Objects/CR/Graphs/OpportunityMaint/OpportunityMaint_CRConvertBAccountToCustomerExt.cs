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
using PX.Objects.CR.Extensions.CRConvertLinkedEntityActions;

namespace PX.Objects.CR.OpportunityMaint_Extensions
{
	/// <inheritdoc/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class OpportunityMaint_CRConvertBAccountToCustomerExt : CRConvertBAccountToCustomerExt<OpportunityMaint, CROpportunity>
	{
		public override void Initialize()
		{
			base.Initialize();

			Addresses = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentAddress>(Base.Opportunity_Address);
			Contacts = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentContact>(Base.Opportunity_Contact);
		}

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(CROpportunity)) { RefContactID = typeof(CROpportunity.contactID) };
		}
		protected override DocumentContactMapping GetDocumentContactMapping()
		{
			return new DocumentContactMapping(typeof(CRContact));
		}
		protected override DocumentAddressMapping GetDocumentAddressMapping()
		{
			return new DocumentAddressMapping(typeof(CRAddress));
		}
	}
}
