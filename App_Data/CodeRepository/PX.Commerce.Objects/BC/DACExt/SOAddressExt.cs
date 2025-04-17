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
	/// DAC extension of SOAddress to add additional attributes.
	/// </summary>
	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			SOAddress,
		Where<
			SOAddress.addressID, Equal<Current<SOOrder.billAddressID>>,Or<SOAddress.addressID,Equal<Current<SOOrder.shipAddressID>>>>>))]
	[PXNonInstantiatedExtension]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public sealed class SOAddressExt : PXCacheExtension<SOAddress>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		/// <summary>
		/// <inheritdoc cref="SOAddress.AddressLine1"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(SOAddress.isEncrypted), typeof(PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<addressLine1>.WithDependencies<SOAddress.isEncrypted, PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus>))]
		public string AddressLine1 { get; set; }
		/// <inheritdoc cref="AddressLine1"/>
		public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }

		/// <summary>
		/// <inheritdoc cref="SOAddress.AddressLine1"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(SOAddress.isEncrypted), typeof(PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<addressLine2>.WithDependencies<SOAddress.isEncrypted, PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus>))]
		public string AddressLine2 { get; set; }
		/// <inheritdoc cref="AddressLine2"/>
		public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }

		/// <summary>
		/// <inheritdoc cref="SOAddress.AddressLine1"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(SOAddress.isEncrypted), typeof(PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<addressLine3>.WithDependencies<SOAddress.isEncrypted, PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus>))]
		public string AddressLine3 { get; set; }
		/// <inheritdoc cref="AddressLine3"/>
		public abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }

		/// <summary>
		/// <inheritdoc cref="SOAddress.AddressLine1"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(SOAddress.isEncrypted), typeof(PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<city>.WithDependencies<SOAddress.isEncrypted, PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus>))]
		public string City { get; set; }
		/// <inheritdoc cref="City"/>
		public abstract class city : PX.Data.BQL.BqlString.Field<city> { }

		/// <summary>
		/// <inheritdoc cref="SOAddress.AddressLine1"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXPersonalDataField]
		[BCEncryptPersonalDataAttribute(typeof(SOAddress.isEncrypted), typeof(PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<state>.WithDependencies<SOAddress.isEncrypted, PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus>))]
		public string State { get; set; }
		/// <inheritdoc cref="State"/>
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }

		/// <summary>
		/// <inheritdoc cref="SOAddress.AddressLine1"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXPersonalDataField]
		[BCEncryptPersonalDataAttribute(typeof(SOAddress.isEncrypted), typeof(PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<countryID>.WithDependencies<SOAddress.isEncrypted, PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus>))]
		public string CountryID { get; set; }
		/// <inheritdoc cref="CountryID"/>
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }

		/// <summary>
		/// <inheritdoc cref="SOAddress.AddressLine1"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[BCEncryptPersonalDataAttribute(typeof(SOAddress.isEncrypted), typeof(PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus))]
		[PXFormula(typeof(Row<postalCode>.WithDependencies<SOAddress.isEncrypted, PX.Objects.GDPR.SOAddressExt.pseudonymizationStatus>))]
		public  string PostalCode { get; set; }
		/// <inheritdoc cref="PostalCode"/>
		public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
	}

	/// <summary>
	/// DAC extension of SOAddress to add additional properties.
	/// </summary>
	[PXHidden]
	public class SOAddress2 : PX.Objects.SO.SOAddress
	{
		/// <summary>
		/// <inheritdoc cref="SOAddress.AddressID"/>
		/// </summary>
		public new abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		/// <summary>
		/// <inheritdoc cref="SOAddress.CustomerID"/>
		/// </summary>
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
	}
}
