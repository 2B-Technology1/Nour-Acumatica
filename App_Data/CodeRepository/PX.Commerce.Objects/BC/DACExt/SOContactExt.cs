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

using PX.Commerce.Core;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.SO;
using System;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// DAC extension of SOContact to add additional properties.
	/// </summary>
	[Serializable]
	[PXPersonalDataTable(typeof(
		   Select<
			   SOContact,
		   Where<
			   SOContact.contactID, Equal<Current<SOOrder.billContactID>>, Or<SOContact.contactID, Equal<Current<SOOrder.shipContactID>>>>>))]
	[PXNonInstantiatedExtension]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public sealed class SOContactExt: PXCacheExtension<SOContact>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		/// <summary>
		/// The attention from the sales order contact's address.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(SOContact.isEncrypted), typeof(PX.Objects.GDPR.SOContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<attention>.WithDependencies<SOContact.isEncrypted, PX.Objects.GDPR.SOContactExt.pseudonymizationStatus>))]
		public string Attention { get; set; }
		/// <inheritdoc cref="Attention"/>
		public abstract class attention : PX.Data.BQL.BqlString.Field<attention> { }

		/// <summary>
		/// The salutation for the sales order contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(SOContact.isEncrypted), typeof(PX.Objects.GDPR.SOContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<salutation>.WithDependencies<SOContact.isEncrypted, PX.Objects.GDPR.SOContactExt.pseudonymizationStatus>))]
		public string Salutation { get; set; }
		/// <inheritdoc cref="Salutation"/>
		public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }

		/// <summary>
		/// The email for the sales order contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptEmailDataAttribute(typeof(SOContact.isEncrypted), typeof(PX.Objects.GDPR.SOContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<email>.WithDependencies<SOContact.isEncrypted, PX.Objects.GDPR.SOContactExt.pseudonymizationStatus>))]
		public string Email { get; set; }
		/// <inheritdoc cref="Email"/>
		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }

		/// <summary>
		/// The primary phone for the sales order contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(SOContact.isEncrypted), typeof(PX.Objects.GDPR.SOContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<phone1>.WithDependencies<SOContact.isEncrypted, PX.Objects.GDPR.SOContactExt.pseudonymizationStatus>))]
		public string Phone1 { get; set; }
		/// <inheritdoc cref="Phone1"/>
		public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }

		/// <summary>
		/// The secondary phone for the sales order contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(SOContact.isEncrypted), typeof(PX.Objects.GDPR.SOContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<phone2>.WithDependencies<SOContact.isEncrypted, PX.Objects.GDPR.SOContactExt.pseudonymizationStatus>))]
		public string Phone2 { get; set; }
		/// <inheritdoc cref="Phone2"/>
		public abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }

		/// <summary>
		/// The tertiary phone for the sales order contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(SOContact.isEncrypted), typeof(PX.Objects.GDPR.SOContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<phone3>.WithDependencies<SOContact.isEncrypted, PX.Objects.GDPR.SOContactExt.pseudonymizationStatus>))]
		public string Phone3 { get; set; }
		/// <inheritdoc cref="Phone3"/>
		public abstract class phone3 : PX.Data.BQL.BqlString.Field<phone3> { }

		/// <summary>
		/// The full name of the sales order contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(SOContact.isEncrypted), typeof(PX.Objects.GDPR.SOContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<fullName>.WithDependencies<SOContact.isEncrypted, PX.Objects.GDPR.SOContactExt.pseudonymizationStatus>))]
		public string FullName { get; set; }
		/// <inheritdoc cref="FullName"/>
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }

		/// <summary>
		/// The fax number for the sales order contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(SOContact.isEncrypted), typeof(PX.Objects.GDPR.SOContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<fax>.WithDependencies<SOContact.isEncrypted, PX.Objects.GDPR.SOContactExt.pseudonymizationStatus>))]
		public string Fax { get; set; }
		/// <inheritdoc cref="Fax"/>
		public abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }
	}

	/// <summary>
	/// DAC extension of SOContact to add additional properties.
	/// </summary>
	[PXHidden]
	public class SOContact2 : PX.Objects.SO.SOContact
	{
		/// <summary>
		/// <inheritdoc cref="SOContact.CustomerID"/>
		/// </summary>
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		/// <summary>
		/// <inheritdoc cref="SOContact.contactID"/>
		/// </summary>
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
	}
}
