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
	/// DAC extension of ARAddress to add additional attributes.
	/// </summary>
	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			ARAddress,
		Where<
			ARAddress.addressID, Equal<Current<ARInvoice.billAddressID>>, Or<ARAddress.addressID, Equal<Current<ARInvoice.shipAddressID>>>>>))]
	[PXNonInstantiatedExtension]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public sealed class ARAddressExt : PXCacheExtension<ARAddress>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		/// <summary>
		/// <inheritdoc cref="ARAddress.AddressLine1"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(ARAddress.isEncrypted), typeof(PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<addressLine1>.WithDependencies<ARAddress.isEncrypted, PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus>))]
		public string AddressLine1 { get; set; }
		/// <summary>
		/// <inheritdoc cref="AddressLine1"/>
		/// </summary>
		public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }

		/// <summary>
		/// <inheritdoc cref="ARAddress.AddressLine2"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(ARAddress.isEncrypted), typeof(PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<addressLine2>.WithDependencies<ARAddress.isEncrypted, PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus>))]
		public string AddressLine2 { get; set; }
		/// <summary>
		/// <inheritdoc cref="AddressLine2"/>
		/// </summary>
		public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }

		/// <summary>
		/// <inheritdoc cref="ARAddress.AddressLine3"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(ARAddress.isEncrypted), typeof(PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<addressLine3>.WithDependencies<ARAddress.isEncrypted, PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus>))]
		public string AddressLine3 { get; set; }
		/// <summary>
		/// <inheritdoc cref="AddressLine3"/>
		/// </summary>
		public abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }

		/// <summary>
		/// <inheritdoc cref="ARAddress.City"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(ARAddress.isEncrypted), typeof(PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<city>.WithDependencies<ARAddress.isEncrypted, PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus>))]
		public string City { get; set; }
		/// <summary>
		/// <inheritdoc cref="City"/>
		/// </summary>
		public abstract class city : PX.Data.BQL.BqlString.Field<city> { }

		/// <summary>
		/// <inheritdoc cref="ARAddress.State"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXPersonalDataField]
		[BCEncryptPersonalDataAttribute(typeof(ARAddress.isEncrypted), typeof(PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<state>.WithDependencies<ARAddress.isEncrypted, PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus>))]
		public string State { get; set; }
		/// <summary>
		/// <inheritdoc cref="State"/>
		/// </summary>
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }

		/// <summary>
		/// <inheritdoc cref="ARAddress.CountryID"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXPersonalDataField]
		[BCEncryptPersonalDataAttribute(typeof(ARAddress.isEncrypted), typeof(PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<countryID>.WithDependencies<ARAddress.isEncrypted, PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus>))]
		public string CountryID { get; set; }
		/// <summary>
		/// <inheritdoc cref="CountryID"/>
		/// </summary>
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }

		/// <summary>
		/// <inheritdoc cref="ARAddress.PostalCode"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(ARAddress.isEncrypted), typeof(PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<postalCode>.WithDependencies<ARAddress.isEncrypted, PX.Objects.GDPR.ARAddressExt.pseudonymizationStatus>))]
		public  string PostalCode { get; set; }
		/// <summary>
		/// <inheritdoc cref="PostalCode"/>
		/// </summary>
		public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }

	}

	/// <summary>
	/// DAC Extension of ARAddress to add additional properties.
	/// </summary>
	[PXHidden]
	public class ARAddress2 : PX.Objects.AR.ARAddress
	{
		/// <summary>
		/// <inheritdoc cref="ARAddress.AddressID"/>
		/// </summary>
		public new abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		/// <summary>
		/// <inheritdoc cref="ARAddress.CustomerID"/>
		/// </summary>
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
	}
}
