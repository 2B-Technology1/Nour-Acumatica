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

using PX.Api.Payroll;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CA;
using PX.Payroll;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the tax information related to a specific employee. The information will be displayed on the Employee Payroll Settings (PR203000) form.
	/// </summary>
	[PXCacheName(Messages.PREmployeeAttribute)]
	[Serializable]
	public class PREmployeeAttribute : IBqlTable, IPRSetting, IStateSpecific
	{
		#region Keys
		public class PK : PrimaryKeyOf<PREmployeeAttribute>.By<bAccountID, settingName>
		{
			public static PREmployeeAttribute Find(PXGraph graph, int? bAccountID, string settingName, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, bAccountID, settingName, options);
		}

		public static class FK
		{
			public class Employee : PREmployee.PK.ForeignKeyOf<PREmployeeAttribute>.By<bAccountID> { }
			public class State : CS.State.PK.ForeignKeyOf<PREmployeeAttribute>.By<countryID, state> { }
			public class Country : CS.Country.PK.ForeignKeyOf<PREmployeeAttribute>.By<countryID> { }
			public class CompanyTaxAttribute : PRCompanyTaxAttribute.PK.ForeignKeyOf<PREmployeeAttribute>.By<settingName> { }
		}
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PREmployee.bAccountID))]
		[PXParent(typeof(Select<PREmployee, Where<PREmployee.bAccountID, Equal<Current<PREmployeeAttribute.bAccountID>>>>))]
		public virtual int? BAccountID { get; set; }
		#endregion
		#region TypeName
		public abstract class typeName : PX.Data.BQL.BqlString.Field<typeName> { }
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Type", Visible = false, Enabled = false)]
		public virtual string TypeName { get; set; }
		#endregion
		#region SettingName
		public abstract class settingName : PX.Data.BQL.BqlString.Field<settingName> { }
		[PXDBString(255, IsKey = true, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Setting", Visible = false, Enabled = false)]
		[PXSelector(typeof(SearchFor<PRCompanyTaxAttribute.settingName>))]
		public virtual string SettingName { get; set; }
		#endregion
		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		[PXDBString(3, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "State", Visible = true, Enabled = false)]
		public virtual string State { get; set; }
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		[PXDBString(2, IsFixed = true)]
		[PXDefault]
		[PRCountry]
		public virtual string CountryID { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Name", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Description { get; set; }
		#endregion
		#region IsEncryptionRequired
		public abstract class isEncryptionRequired : PX.Data.BQL.BqlBool.Field<isEncryptionRequired> { }
		/// <summary>
		/// Indicates (if set to <see langword="true" />) that the encryption is required.
		/// </summary>
		[PXDBBool]
		[PXFormula(typeof(Default<aatrixMapping>))]
		[PXDefault(typeof(aatrixMapping.IsEqual<ssnAatrixMappingCode>))]
		public virtual bool? IsEncryptionRequired { get; set; }
		#endregion
		#region IsEncrypted
		public abstract class isEncrypted : PX.Data.BQL.BqlBool.Field<isEncrypted> { }
		/// <summary>
		/// Indicates (if set to <see langword="true" />) that the information is encrypted.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsEncrypted { get; set; }
		#endregion
		#region CanadaReportMapping
		public abstract class canadaReportMapping : PX.Data.BQL.BqlInt.Field<canadaReportMapping> { }
		[PXDBInt]
		[PXUIField(Visible = false)]
		public virtual int? CanadaReportMapping { get; set; }
		#endregion
		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }
		[PXRSACryptStringWithConditional(255, typeof(isEncryptionRequired), typeof(isEncrypted), IsViewDecrypted = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Value")]
		[PXUIEnabled(typeof(Where<PREmployeeAttribute.useDefault, Equal<False>>))]
		[PXDependsOnFields(typeof(isEncryptionRequired), typeof(isEncrypted))]
		public virtual string Value { get; set; }
		#endregion
		#region UseDefault
		public abstract class useDefault : PX.Data.BQL.BqlBool.Field<useDefault> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Default")]
		public virtual bool? UseDefault { get; set; }
		#endregion
		#region AllowOverride
		public abstract class allowOverride : PX.Data.BQL.BqlBool.Field<allowOverride> { }
		[PXBool]
		[PXUIField(DisplayName = "Allow Employee Override", Enabled = false, Visible = false)]
		public virtual bool? AllowOverride { get; set; }
		#endregion
		#region IsFederal
		public abstract class isFederal : PX.Data.BQL.BqlBool.Field<isFederal> { }
		[PXBool]
		[PXUIField(DisplayName = "Is Federal Attribute")]
		public virtual bool? IsFederal
		{
			[PXDependsOnFields(typeof(PREmployeeAttribute.state))]
			get
			{
				return State == LocationConstants.USFederalStateCode || State == LocationConstants.CanadaFederalStateCode;
			}
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		[PXInt]
		[PXUIField(DisplayName = "Sort Order")]
		public virtual int? SortOrder { get; set; }
		#endregion
		#region Required
		public abstract class required : PX.Data.BQL.BqlBool.Field<required> { }
		[PXBool]
		[PXUIField(DisplayName = "Required", Enabled = false, Visible = false)]
		public virtual bool? Required { get; set; }
		#endregion
		#region AatrixMapping
		public abstract class aatrixMapping : PX.Data.BQL.BqlInt.Field<aatrixMapping> { }
		[PXDBInt]
		[PXUIField(Visible = false)]
		public virtual int? AatrixMapping { get; set; }
		#endregion
		#region AdditionalInformation
		[PXDBString(2048, IsUnicode = true)]
		[PXUIField(DisplayName = "Additional Information", Enabled = false)]
		public virtual string AdditionalInformation { get; set; }
		public abstract class additionalInformation : PX.Data.BQL.BqlString.Field<additionalInformation> { }
		#endregion
		#region UsedForTaxCalculation
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Used for Tax Calculation", Enabled = false)]
		public virtual bool? UsedForTaxCalculation { get; set; }
		public abstract class usedForTaxCalculation : PX.Data.BQL.BqlBool.Field<usedForTaxCalculation> { }
		#endregion
		#region UsedForGovernmentReporting
		[PXBool]
		[PXUnboundDefault(typeof(aatrixMapping.IsNotNull.Or<canadaReportMapping.IsNotNull>))]
		[PXUIField(DisplayName = "Used for Government Reporting", Enabled = false)]
		public virtual bool? UsedForGovernmentReporting { get; set; }
		public abstract class usedForGovernmentReporting : PX.Data.BQL.BqlBool.Field<usedForGovernmentReporting> { }
		#endregion
		#region FormBox
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Form/Box", Enabled = false)]
		public virtual string FormBox { get; set; }
		public abstract class formBox : PX.Data.BQL.BqlString.Field<formBox> { }
		#endregion
		#region CompanyNotes
		[PXString]
		[PXUIField(DisplayName = "Company Notes", Enabled = false)]
		[CompanyNoteText(typeof(settingName))]
		public virtual string CompanyNotes { get; set; }
		public abstract class companyNotes : PX.Data.BQL.BqlString.Field<companyNotes> { }
		#endregion
		#region NoteID
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		public abstract class noteID : IBqlField { }
		#endregion
		#region ErrorLevel
		public abstract class errorLevel : PX.Data.BQL.BqlInt.Field<errorLevel> { }
		[PXInt]
		public virtual int? ErrorLevel { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}

	public class ssnAatrixMappingCode : PX.Data.BQL.BqlInt.Constant<ssnAatrixMappingCode>
	{
		public ssnAatrixMappingCode() : base(AatrixField.EMP.SocialSecurityNumber)
		{
		}
	}
}
