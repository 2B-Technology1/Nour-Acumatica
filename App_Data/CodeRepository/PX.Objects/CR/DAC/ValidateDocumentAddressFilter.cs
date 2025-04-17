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
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.PO;
using PX.Objects.PM;

namespace PX.Objects.CR
{
	/// <summary>
	/// An Unbound DAC used for the filters on the screen for validating addresses in documents. The DocumentType field is overridden
	/// in the graphs of different processing screens to show only documents which would be processed by the respective screen.
	/// </summary>
	/// <remarks>
	/// The DAC is used for filters on:
	/// <list type="bullet">
	/// <item><description>The <i>Validate Addresses in Sales Documents (SO508000)</i> form (corresponds to the <see cref="ValidateSODocumentAddressProcess"/> graph)</description></item>
	/// <item><description>The <i>Validate Addresses in AR Documents (AR509010)</i> form (corresponds to the <see cref="ValidateARDocumentAddressProcess"/> graph)</description></item>
	/// <item><description>The <i>Validate Addresses in AP Documents (AP508000)</i> form (corresponds to the <see cref="ValidateAPDocumentAddressProcess"/> graph)</description></item>
	/// <item><description>The <i>Validate Addresses in Purchase Documents (PO507000)</i> form (corresponds to the <see cref="ValidatePODocumentAddressProcess"/> graph)</description></item>
	/// <item><description>The <i>Validate Addresses in CRM Documents (CR508000)</i> form (corresponds to the <see cref="ValidateCRDocumentAddressProcess"/> graph)</description></item>
	/// <item><description>The <i>Validate Addresses in Project Documents (PM507000)</i> form (corresponds to the <see cref="ValidatePMDocumentAddressProcess"/> graph)</description></item>
	/// </list>
	/// </remarks>
	[PXHidden]
	public class ValidateDocumentAddressFilter : PX.Data.IBqlTable
	{
		#region IsOverride
		public abstract class isOverride : PX.Data.BQL.BqlBool.Field<isOverride> { }

		/// <summary>
		/// A boolean value that determines (if set to <see langword="true" />) that the incorrect address would be overridden as a result of address verification.
		/// If the value is set to <see langword="false" />, the incorrect address would not be overridden as a result of address verification.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Override Addresses Automatically")]
		public virtual bool? IsOverride { get; set; }
		#endregion

		#region Country
		public abstract class country : PX.Data.BQL.BqlString.Field<country> { }

		/// <summary>
		/// The filter that loads the records in the grid based on the selected country.
		/// </summary>
		[PXString(2, InputMask = ">??")]
		[PXUIField(DisplayName = "Country")]
		[PXSelector(typeof(Search<Country.countryID>),
			typeof(Country.countryID),
			typeof(Country.description),
			typeof(Country.addressValidatorPluginID),
			DescriptionField = typeof(Country.description))]
		public virtual string Country { get; set; }
		#endregion

		#region DocumentType
		public abstract class documentType : PX.Data.BQL.BqlString.Field<documentType> { }

		/// <summary>
		/// A Document Type filter. Classes implementing the base graph <see cref="ValidateDocumentAddressGraph{TGraph}"/>
		/// would override this field with a StringList attribute to show the required Document Types in the drop down for this field.
		/// </summary>
		[PXString(20)]
		[PXUIField(DisplayName = "Document Type", Required = true)]
		public virtual string DocumentType { get; set; }
		#endregion
	}
}
