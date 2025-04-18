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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores different tax codes associated with the work addresses and home addresses of the employees.
	/// The tax codes are sent to the system by a third-party provider and will be displayed on the Tax Maintenance (PR208000) form.
	/// </summary>
	[PXCacheName(Messages.PRTaxCode)]
	[Serializable]
	public class PRTaxCode : PRTaxTypeSettingMapper, IBqlTable, ITaxCode
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRTaxCode>.By<taxID>
		{
			public static PRTaxCode Find(PXGraph graph, int? taxID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, taxID, options);
		}

		public class UK : PrimaryKeyOf<PRTaxCode>.By<taxCD>
		{
			public static PRTaxCode Find(PXGraph graph, string taxCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, taxCD, options);
		}

		public static class FK
		{
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<PRTaxCode>.By<bAccountID> { }
			public class State : CS.State.PK.ForeignKeyOf<PRTaxCode>.By<countryID, taxState> { }
			public class Country : CS.Country.PK.ForeignKeyOf<PRTaxCode>.By<countryID> { }
			public class ExpenseAccount : Account.PK.ForeignKeyOf<PRTaxCode>.By<expenseAcctID> { }
			public class ExpenseSubaccount : Sub.PK.ForeignKeyOf<PRTaxCode>.By<expenseSubID> { }
			public class LiabilityAccount : Account.PK.ForeignKeyOf<PRTaxCode>.By<liabilityAcctID> { }
			public class LiabilitySubaccount : Sub.PK.ForeignKeyOf<PRTaxCode>.By<liabilitySubID> { }

		}
		#endregion

		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlInt.Field<taxID> { }
		[PXDBIdentity]
		public int? TaxID { get; set; }
		#endregion
		#region TaxCD
		public abstract class taxCD : PX.Data.BQL.BqlString.Field<taxCD> { }
		[PXDBString(60, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(SearchFor<taxCD>
			.Where<MatchPRCountry<countryID>>))]
		[PXDefault]
		public string TaxCD { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public string Description { get; set; }
		#endregion
		#region TaxCategory
		public abstract class taxCategory : PX.Data.BQL.BqlString.Field<taxCategory> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Tax Category", Enabled = false)]
		[PXDefault]
		[TaxCategory.List]
		public string TaxCategory { get; set; }
		#endregion
		#region TypeName
		public abstract class typeName : PX.Data.BQL.BqlString.Field<typeName> { }
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[TaxTypeSelector(typeof(PRTaxCode.taxCategory))]
		public override string TypeName { get; set; }
		#endregion
		#region JurisdictionLevel
		public abstract class jurisdictionLevel : PX.Data.BQL.BqlString.Field<jurisdictionLevel> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Jurisdiction Level", Enabled = false)]
		[TaxJurisdiction.List]
		[PXFormula(typeof(Switch<Case<Where<typeName, IsNull>, jurisdictionLevel>, Selector<typeName, PRTaxType.taxJurisdiction>>))]
		public string JurisdictionLevel { get; set; }
		#endregion
		#region TaxTypeDescription
		public abstract class taxTypeDescription : PX.Data.BQL.BqlString.Field<taxTypeDescription> { }
		[PXDBString(100)]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		[PXFormula(typeof(Switch<Case<Where<typeName, IsNull>, taxTypeDescription>, Selector<typeName, PRTaxType.description>>))]
		public string TaxTypeDescription { get; set; }
		#endregion
		#region GovtRefNbr
		public abstract class govtRefNbr : PX.Data.BQL.BqlString.Field<govtRefNbr> { }
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Employer Govt. Tax ID")]
		[PXUIEnabled(typeof(Where<jurisdictionLevel.IsNotEqual<TaxJurisdiction.federal>>))]
		[PXSelector(typeof(Search4<PRTaxCode.govtRefNbr, Where<PRTaxCode.govtRefNbr, IsNotNull>, Aggregate<GroupBy<PRTaxCode.govtRefNbr>>>), new Type[] { typeof(PRTaxCode.govtRefNbr) }, ValidateValue = false)]
		public string GovtRefNbr { get; set; }
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[VendorActive(typeof(Search<Vendor.bAccountID, Where<Vendor.taxAgency, Equal<True>>>))]
		public int? BAccountID { get; set; }
		#endregion
		#region TaxUniqueCode
		public abstract class taxUniqueCode : PX.Data.BQL.BqlString.Field<taxUniqueCode> { }
		[PXDBString(30, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Tax Unique ID", Enabled = false)]
		public string TaxUniqueCode { get; set; }
		#endregion
		#region TaxInvDescrType
		public abstract class taxInvDescrType : PX.Data.BQL.BqlString.Field<taxInvDescrType> { }
		[PXDBString(3, IsUnicode = false)]
		[PXUIField(DisplayName = "Invoice Description Source")]
		[InvoiceDescriptionType.TaxInvoiceListAttribute]
		public string TaxInvDescrType { get; set; }
		#endregion
		#region VndInvDescr
		public abstract class vndInvDescr : PX.Data.BQL.BqlString.Field<vndInvDescr> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Vendor Invoice Description")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PRTaxCode.taxInvDescrType, Equal<InvoiceDescriptionType.freeFormatEntry>>))]
		[PXUIRequired(typeof(Where<PRTaxCode.taxInvDescrType, Equal<InvoiceDescriptionType.freeFormatEntry>>))]
		public string VndInvDescr { get; set; }
		#endregion
		#region TaxState
		public abstract class taxState : PX.Data.BQL.BqlString.Field<taxState> { }
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Tax State", Enabled = false)]
		[PXDefault]
		[TaxStateSelector(typeof(countryID))]
		[PXUIEnabled(typeof(Where<PRTaxCode.jurisdictionLevel, NotEqual<TaxJurisdiction.federal>>))]
		[PXUIRequired(typeof(Where<PRTaxCode.jurisdictionLevel, NotEqual<TaxJurisdiction.federal>>))]
		public override string TaxState { get; set; }
		#endregion
		#region ExpenseAcctID
		public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		[Account(DisplayName = "Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<PRTaxCode.expenseAcctID>.IsRelatedTo<Account.accountID>))]
		[PXUIEnabled(typeof(Where<PRTaxCode.taxCategory.IsEqual<TaxCategory.employerTax>>))]
		[PRTaxExpenseAccountRequired(GLAccountSubSource.TaxCode, typeof(Where<taxCategory.IsEqual<TaxCategory.employerTax>>))]
		public virtual Int32? ExpenseAcctID { get; set; }
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		[SubAccount(typeof(PRTaxCode.expenseAcctID), DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRTaxCode.expenseSubID>.IsRelatedTo<Sub.subID>))]
		[PXUIEnabled(typeof(Where<PRTaxCode.taxCategory.IsEqual<TaxCategory.employerTax>>))]
		[PRTaxExpenseSubRequired(GLAccountSubSource.TaxCode, typeof(Where<taxCategory.IsEqual<TaxCategory.employerTax>>))]
		public virtual Int32? ExpenseSubID { get; set; }
		#endregion
		#region LiabilityAcctID
		public abstract class liabilityAcctID : PX.Data.BQL.BqlInt.Field<liabilityAcctID> { }
		[Account(DisplayName = "Liability Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<PRTaxCode.liabilityAcctID>.IsRelatedTo<Account.accountID>))]
		[PRTaxLiabilityAccountRequired(GLAccountSubSource.TaxCode)]
		public virtual Int32? LiabilityAcctID { get; set; }
		#endregion
		#region LiabilitySubID
		public abstract class liabilitySubID : PX.Data.BQL.BqlInt.Field<liabilitySubID> { }
		[SubAccount(typeof(PRTaxCode.liabilityAcctID), DisplayName = "Liability Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRTaxCode.liabilitySubID>.IsRelatedTo<Sub.subID>))]
		[PRTaxLiabilitySubRequired(GLAccountSubSource.TaxCode)]
		public virtual Int32? LiabilitySubID { get; set; }
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		[PXDBString(2, IsFixed = true)]
		[PXDefault]
		[PRCountry]
		[PXUIField(Visible = false)]
		public virtual string CountryID { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
		#region ErrorLevel
		public abstract class errorLevel : PX.Data.BQL.BqlInt.Field<errorLevel> { }
		[PXInt]
		[PXUnboundDefault((int)PXErrorLevel.Undefined)]
		public virtual int? ErrorLevel { get; set; }
		#endregion
		#region System Columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }


		#endregion
		#endregion
	}
}


