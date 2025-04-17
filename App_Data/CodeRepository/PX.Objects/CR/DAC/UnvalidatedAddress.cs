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
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.PO;
using PX.Objects.PM;

namespace PX.Objects.CR
{
	/// <summary>
	/// An unbound DAC used for the grid on the screen for validating addresses in documents. Each record represents an invalid address used in the document
	/// for which the <see cref="IValidatedAddress.IsValidated" /> flag is set to <see langword="false" />.
	/// </summary>
	/// <remarks>
	/// The DAC is used for grid on:
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
	public class UnvalidatedAddress : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		/// <summary>
		/// A boolean value that indicates (if set to <see langword="true" />) that the record is marked as selected.
		/// If the value is set to <see langword="false" />, the record is marked as unselected.
		/// </summary>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion

		#region DocumentNbr
		public abstract class documentNbr : PX.Data.BQL.BqlString.Field<documentNbr> { }

		/// <summary>
		/// The Document Nbr, which is a comma separated value of DocType/OrderType and RefNbr/OrderNbr fields.
		/// If Document Type is not present for a specific document, this field just holds the RefNbr/OrderNbr.
		/// </summary>
		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Document Nbr.")]
		public virtual string DocumentNbr { get; set; }
		#endregion

		#region AddressID
		public abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }

		/// <summary>
		/// The Address ID for the address.
		/// </summary>
		[PXInt(IsKey = true)]
		public virtual int? AddressID { get; set; }
		#endregion

		#region DocumentType
		public abstract class documentType : PX.Data.BQL.BqlString.Field<documentType> { }

		/// <summary>
		/// The DocumentType field that depicts the document type to which the address belongs.
		/// </summary>
		[PXString(30, IsUnicode = true)]
		public virtual string DocumentType { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <summary>
		/// The status of the document.
		/// </summary>
		[PXString(20, IsUnicode = true)]
		public virtual string Status { get; set; }
		#endregion

		#region AddressLine1
		public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }

		/// <summary>
		/// The first line of the address.
		/// </summary>
		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 1")]
		public virtual string AddressLine1 { get; set; }
		#endregion

		#region AddressLine2
		public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }

		/// <summary>
		/// The second line of the address.
		/// </summary>
		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 2")]
		public virtual string AddressLine2 { get; set; }
		#endregion

		#region City
		public abstract class city : PX.Data.BQL.BqlString.Field<city> { }

		/// <summary>
		/// The city of the address.
		/// </summary>
		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "City")]
		public virtual String City { get; set; }
		#endregion

		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }

		/// <summary>
		/// The state of the address.
		/// </summary>
		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		public virtual String State { get; set; }
		#endregion

		#region PostalCode
		public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }

		/// <summary>
		/// The postal code of the address.
		/// </summary>
		[PXString(20)]
		[PXUIField(DisplayName = "Postal Code")]
		public virtual String PostalCode { get; set; }
		#endregion

		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }

		/// <summary>
		/// The country of the address.
		/// </summary>
		[PXString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Country")]
		public virtual String CountryID { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region Address
		/// <summary>
		/// A field of type <see cref="IAddress" />, which is used to save the address record.
		/// </summary>
		public IAddress Address { get; set; }
		#endregion

		#region Document
		/// <summary>
		/// A field of type <see cref="IBqlTable"/>, which is used to save the document.
		/// </summary>
		public IBqlTable Document { get; set; }
		#endregion
	}
}
