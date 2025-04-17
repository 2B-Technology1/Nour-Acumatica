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
	/// DAC Extension of ARContact to add additional properties.
	/// </summary>
	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			ARContact,
		Where<
			ARContact.contactID, Equal<Current<ARInvoice.billContactID>>, Or<ARContact.contactID, Equal<Current<ARInvoice.shipContactID>>>>>))]
	[PXNonInstantiatedExtension]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public sealed class ARContactExt : PXCacheExtension<ARContact>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		/// <summary>
		/// The attention for this contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(ARContact.isEncrypted), typeof(PX.Objects.GDPR.ARContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<attention>.WithDependencies<ARContact.isEncrypted, PX.Objects.GDPR.ARContactExt.pseudonymizationStatus>))]
		public string Attention { get; set; }
		/// <inheritdoc cref="Attention"/>
		public abstract class attention : PX.Data.BQL.BqlString.Field<attention> { }

		/// <summary>
		/// The salutation to use with this contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(ARContact.isEncrypted), typeof(PX.Objects.GDPR.ARContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<salutation>.WithDependencies<ARContact.isEncrypted, PX.Objects.GDPR.ARContactExt.pseudonymizationStatus>))]
		public string Salutation { get; set; }
		/// <inheritdoc cref="Salutation"/>
		public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }

		/// <summary>
		/// The email address of the contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptEmailDataAttribute(typeof(ARContact.isEncrypted), typeof(PX.Objects.GDPR.ARContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<email>.WithDependencies<ARContact.isEncrypted, PX.Objects.GDPR.ARContactExt.pseudonymizationStatus>))]
		public string Email { get; set; }
		/// <inheritdoc cref="Email"/>
		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }

		/// <summary>
		/// The primary phone for this contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(ARContact.isEncrypted), typeof(PX.Objects.GDPR.ARContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<phone1>.WithDependencies<ARContact.isEncrypted, PX.Objects.GDPR.ARContactExt.pseudonymizationStatus>))]
		public string Phone1 { get; set; }
		/// <inheritdoc cref="Phone1"/>
		public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }

		/// <summary>
		/// The secondary phone for this contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(ARContact.isEncrypted), typeof(PX.Objects.GDPR.ARContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<phone2>.WithDependencies<ARContact.isEncrypted, PX.Objects.GDPR.ARContactExt.pseudonymizationStatus>))]
		public string Phone2 { get; set; }
		/// <inheritdoc cref="Phone2"/>
		public abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }

		/// <summary>
		/// The tertiary phone for this contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(ARContact.isEncrypted), typeof(PX.Objects.GDPR.ARContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<phone3>.WithDependencies<ARContact.isEncrypted, PX.Objects.GDPR.ARContactExt.pseudonymizationStatus>))]
		public string Phone3 { get; set; }
		/// <inheritdoc cref="Phone3"/>
		public abstract class phone3 : PX.Data.BQL.BqlString.Field<phone3> { }

		/// <summary>
		/// The full name for the contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(ARContact.isEncrypted), typeof(PX.Objects.GDPR.ARContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<fullName>.WithDependencies<ARContact.isEncrypted, PX.Objects.GDPR.ARContactExt.pseudonymizationStatus>))]
		public string FullName { get; set; }
		/// <inheritdoc cref="FullName"/>
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }

		/// <summary>
		/// The fax number for this contact.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(ARContact.isEncrypted), typeof(PX.Objects.GDPR.ARContactExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<fax>.WithDependencies<ARContact.isEncrypted, PX.Objects.GDPR.ARContactExt.pseudonymizationStatus>))]
		public string Fax { get; set; }
		/// <inheritdoc cref="Fax"/>
		public abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }
	}

	/// <summary>
	/// DAC Extension of ARContact to add additional properties.
	/// </summary>
	[PXHidden]
	public class ARContact2 : PX.Objects.AR.ARContact
	{
		/// <summary>
		/// <inheritdoc cref="ARContact.ContactID"/>
		/// </summary>
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		/// <summary>
		/// <inheritdoc cref="ARContact.CustomerID"/>
		/// </summary>
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
	}

}
