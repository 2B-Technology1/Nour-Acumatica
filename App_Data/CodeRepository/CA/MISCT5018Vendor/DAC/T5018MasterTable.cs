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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;
using PX.Objects.Localizations.CA.Messages;
using static PX.Objects.Localizations.CA.MISCT5018Vendor.Attributes;

namespace PX.Objects.Localizations.CA {
	public class localizationCode
	{
		public const string CA = "CA";

		public class cA : BqlString.Constant<cA> { public cA() : base(CA) { } }
	}

	/// <summary>
	/// The master record of an organization's T5018 revision for a given year. The record is comprised of one or multiple <see cref="T5018EFileRow">records</see>.
	/// </summary>
	[Serializable]
	[PXCacheName(T5018Messages.T5018MasterTable)]
	[PXPrimaryGraph(typeof(T5018Fileprocessing))]
	public class T5018MasterTable : IBqlTable
	{
		#region Keys
		/// <summary>
		/// T5018MasterTable Primary key
		/// </summary>
		public class PK : PrimaryKeyOf<T5018MasterTable>.By<organizationID, year, revision>
		{
			public static T5018MasterTable Find(PXGraph graph, int organizationID, string year, string revision)
				=> FindBy(graph, organizationID, year, revision);
		}
		#endregion

		#region OrganizationID
		public abstract class organizationID : BqlInt.Field<organizationID> { }
		/// <summary>
		/// The organization ID of the associated organization.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Organization.OrganizationID"/> field.
		/// </value>
		[Organization(IsKey = true, IsDBField = true, DisplayName = T5018Messages.Transmitter)]

		[PXRestrictor(typeof(Where<Organization.organizationLocalizationCode, Equal<localizationCode.cA>>), "Company '{0}' does not have Canadian localization enabled.", typeof(Organization.organizationName))]
		public virtual int? OrganizationID
		{
			get;
			set;
		}
		#endregion

		#region Year
		public abstract class year : BqlString.Field<year> { }
		/// <summary>
		/// The year of the revision.
		/// </summary>
		[PXDBString(4, IsKey = true)]
		[PXUIField(DisplayName = T5018Messages.T5018Year, Required = true)]
		[T5018TaxYearSelector(typeof(FinYear.year), ValidateValue = false)]
		public virtual string Year
		{
			get;
			set;
		}
		#endregion

		#region Revision
		public abstract class revision : BqlString.Field<revision> { }
		/// <summary>
		/// The revision number.
		/// </summary>
		[PXDBString(6, IsKey = true, InputMask = ">CCCCCC")]
		[PXSelector(typeof(SearchFor<T5018MasterTable.revision>.In<
			SelectFrom<T5018MasterTable>.
			Where<T5018MasterTable.organizationID.IsEqual<organizationID.FromCurrent>.
			And<T5018MasterTable.year.IsEqual<year.FromCurrent>>>>),
			new System.Type[] { typeof(T5018MasterTable.revision), typeof(T5018MasterTable.createdDateTime) },
			ValidateValue = false)]
		[PXUIField(DisplayName = T5018Messages.Revision, Required = true)]
		public virtual string Revision
		{
			get;
			set;
		}
		#endregion

		#region Revision Submitted
		public abstract class revisionSubmitted : BqlBool.Field<revisionSubmitted> { }
		/// <summary>
		/// A Boolean value that indicates whether the revision has been submit to the CRA.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Null)]
		[PXUIField(DisplayName = "E-File Submitted to CRA")]
		public virtual bool? RevisionSubmitted
		{
			get;
			set;
		}
		#endregion

		#region FromDate
		public abstract class fromDate : BqlDateTime.Field<fromDate> { }
		/// <summary>
		/// The date of the year start.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "From", IsReadOnly = true)]
		public virtual DateTime? FromDate
		{
			get;
			set;
		}
		#endregion

		#region ToDate
		public abstract class toDate : BqlDateTime.Field<toDate> { }
		/// <summary>
		/// The date of the year end.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "To", IsReadOnly = true)]
		public virtual DateTime? ToDate
		{
			get;
			set;
		}
		#endregion

		#region ProgramNumber
		public abstract class programNumber : PX.Data.BQL.BqlString.Field<programNumber> { }
		/// <summary>
		/// The program number for the organization.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="T5018OrganizationSettings.ProgramNumber"/> field.
		/// </value>
		[PXDBString(15)]
		[PXUIField(DisplayName = "Program Number", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual string ProgramNumber
		{
			get;
			set;
		}
		#endregion

		#region TransmitterNumber
		public abstract class transmitterNumber : BqlString.Field<transmitterNumber> { }
		/// <summary>
		/// The transmitter number for the organization.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="T5018OrganizationSettings.TransmitterNumber"/> field.
		/// </value>
		[PXDBString(8)]
		[PXUIField(DisplayName = "Transmitter Number", Required = true)]
		public virtual string TransmitterNumber
		{
			get;
			set;
		}
		#endregion

		#region AcctName
		public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		/// <summary>
		/// The name of the organization account.
		/// </summary>
		[PXDBString(60)]
		[PXUIField(DisplayName = "Company Name", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual string AcctName
		{
			get;
			set;
		}
		#endregion

		#region Address Line 1
		public abstract class addressLine1 : BqlString.Field<addressLine1> { }
		/// <summary>
		/// Address line 1 of the organization.
		/// </summary>
		[PXDBString(30)]
		[PXUIField(DisplayName = "Address Line 1")]
		public virtual string AddressLine1
		{
			get;
			set;
		}
		#endregion

		#region Address Line 2
		public abstract class addressLine2 : BqlString.Field<addressLine2> { }
		/// <summary>
		/// Address line 2 of the organization.
		/// </summary>
		[PXDBString(30)]
		[PXUIField(DisplayName = "Address Line 2")]
		public virtual string AddressLine2
		{
			get;
			set;
		}
		#endregion

		#region City
		public abstract class city : BqlString.Field<city> { }
		/// <summary>
		/// The city of the organization.
		/// </summary>
		[PXDBString(28)]
		[PXUIField(DisplayName = "City")]
		public virtual string City
		{
			get;
			set;
		}
		#endregion

		#region Province
		public abstract class province : BqlString.Field<province> { }
		/// <summary>
		/// The province or state of the organization.
		/// </summary>
		[PXDBString(2)]
		[PXUIField(DisplayName = "Province")]
		public virtual string Province
		{
			get;
			set;
		}
		#endregion

		#region Country
		public abstract class country : BqlString.Field<country> { }
		/// <summary>
		/// The country of the organization.
		/// </summary>
		[PXDBString(3)]
		[PXUIField(DisplayName = "Country")]
		public virtual string Country
		{
			get;
			set;
		}
		#endregion

		#region PostalCode
		public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
		/// <summary>
		/// The postal code of the organization.
		/// </summary>
		[PXDBString(10)]
		[PXUIField(DisplayName = "Postal Code")]
		public virtual string PostalCode
		{
			get;
			set;
		}
		#endregion

		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		/// <summary>
		/// The name of the organization's primary contact.
		/// </summary>
		[PXDBString(22)]
		[PXUIField(DisplayName = "Name", Required = true)]
		public virtual string Name
		{
			get;
			set;
		}
		#endregion

		#region Contact Area Code
		public abstract class areaCode : PX.Data.BQL.BqlString.Field<areaCode> { }
		/// <summary>
		/// The area code of the phone number of the organization's primary contact.
		/// </summary>
		[PXDBString(3, InputMask = "###")]
		[PXUIField(DisplayName = "Contact Area Code", Required = true)]
		public virtual string AreaCode
		{
			get;
			set;
		}
		#endregion

		#region Phone
		public abstract class phone : PX.Data.BQL.BqlString.Field<phone> { }
		/// <summary>
		/// The phone number of the organization's primary contact.
		/// </summary>
		[PXDBString(8)]
		[PXUIField(DisplayName = "Phone", Required = true)]
		public virtual string Phone
		{
			get;
			set;
		}
		#endregion

		#region Extension Number
		public abstract class extensionNbr : PX.Data.BQL.BqlString.Field<extensionNbr> { }
		/// <summary>
		/// The extension number for the organization's primary contact.
		/// </summary>
		[PXDBString(5, InputMask = "#####")]
		[PXUIField(DisplayName = "Extension Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ExtensionNbr
		{
			get;
			set;
		}
		#endregion

		#region Email
		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }
		/// <summary>
		/// The email of the organization's primary contact.
		/// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual string Email
		{
			get;
			set;
		}
		#endregion

		#region Second Email
		public abstract class secondEmail : PX.Data.BQL.BqlString.Field<secondEmail> { }
		/// <summary>
		/// The second email of the organization's primary contact.
		/// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Second Email", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string SecondEmail
		{
			get;
			set;
		}
		#endregion

		#region Language

		public abstract class language : PX.Data.BQL.BqlString.Field<language>
		{
			#region Language constants
			public const string English = "E";
			public const string French = "F";
			public class english : BqlString.Constant<english>
			{
				public english() : base(English) { }
			}
			public class french : BqlString.Constant<french>
			{
				public french() : base(French) { }
			}
			#endregion
		}
		/// <summary>
		/// The primary language of the organization.
		/// </summary>
		[PXDBString(50)]
		[PXStringList(new string[] { language.English, language.French }, new string[] { T5018Messages.English, T5018Messages.French })]
		[PXUIField(DisplayName = "Language", Required = true)]
		[PXDefault(language.English)]
		public virtual string Language
		{
			get;
			set;
		}
		#endregion

		#region FilingType
		public abstract class filingType : BqlString.Field<filingType>
		{
			#region Filing constants
			public const string Original = "O";
			public const string Amendment = "A";
			public class original : BqlString.Constant<original>
			{
				public original() : base(Original) { }
			}
			public class amendment : BqlString.Constant<amendment>
			{
				public amendment() : base(Amendment) { }
			}
			#endregion
		}
		/// <summary>
		/// The filing type of the revision.
		/// </summary>
		[PXDBString(50)]
		[PXDefault(filingType.Original)]
		[PXStringList(new string[] { filingType.Original, filingType.Amendment }, new string[] { T5018Messages.Original, T5018Messages.Amendment })]
		[PXUIField(DisplayName = "Filing Type", IsReadOnly =true)]
		public virtual string FilingType
		{
			get;
			set;
		}
		#endregion

		#region Submission Number
		public abstract class submissionNo : BqlString.Field<submissionNo> { }
		/// <summary>
		/// The submission number of the T5018 filing.
		/// </summary>
		[PXDBString(8, IsUnicode = true)]
		[PXUIField(DisplayName = "Submission Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string SubmissionNo
		{
			get;
			set;
		}
		#endregion

		#region Threshold Amount
		public abstract class thresholdAmount : BqlDecimal.Field<thresholdAmount> { }
		/// <summary>
		/// The threshold amount that defines when to include the summarized row's total amount in the revision.
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Threshold Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? ThresholdAmount
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion

		#region NoteID

		public abstract class noteID : BqlGuid.Field<noteID> { }

		[PXNote()]
		public virtual Guid? NoteID
		{
			get;
			set;
		}

		#endregion


	}
}
