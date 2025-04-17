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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// A code which is used to record hours and earnings information for employees.
	/// </summary>
	[PXCacheName(Messages.PREarningType)]
	[PXPrimaryGraph(typeof(PREarningTypeMaint))]
	[Serializable]
	[PXTable(IsOptional = true)] //ToDo: AC-142439 Ensure PXForeignReference attribute works correctly with PXCacheExtension DACs.
	public sealed class PREarningType : PXCacheExtension<EPEarningType>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region Keys
		public static class FK
		{
			public class RegularEarningType : EPEarningType.PK.ForeignKeyOf<EPEarningType>.By<regularTypeCD> { }
			public class EarningsAccount : GL.Account.PK.ForeignKeyOf<EPEarningType>.By<earningsAcctID> { }
			public class EarningsSubaccount : GL.Sub.PK.ForeignKeyOf<EPEarningType>.By<earningsSubID> { }
			public class BenefitExpenseAccount : GL.Account.PK.ForeignKeyOf<EPEarningType>.By<benefitExpenseAcctID> { }
			public class BenefitExpenseSubaccount : GL.Sub.PK.ForeignKeyOf<EPEarningType>.By<benefitExpenseSubID> { }
			public class TaxExpenseAccount : GL.Account.PK.ForeignKeyOf<EPEarningType>.By<taxExpenseAcctID> { }
			public class TaxExpenseSubaccount : GL.Sub.PK.ForeignKeyOf<EPEarningType>.By<taxExpenseSubID> { }
			public class PTOExpenseAccount : GL.Account.PK.ForeignKeyOf<EPEarningType>.By<ptoExpenseAcctID> { }
			public class PTOExpenseSubaccount : GL.Sub.PK.ForeignKeyOf<EPEarningType>.By<ptoExpenseSubID> { }
		}
		#endregion

		#region RegularTypeCD
		public abstract class regularTypeCD : BqlString.Field<regularTypeCD> { }
		/// <summary>
		/// The user-friendly unique identifier of the earning type to be used for calculation of the PTO amount.
		/// The field is included in <see cref="FK.RegularEarningType"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EPEarningType.TypeCD"/> field.
		/// </value>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXUIField(DisplayName = "Regular Time Type Code")]
		// Only 'Wage' earning types should be shown in this selector.
		// Thus, all four flags ('IsOvertime', 'IsPiecework', 'IsAmountBased', and 'IsPTO') should be false.
		// Even though all these flags are not nullable, the 'PREarningType' table might be empty if the 'Payroll Module' feature has been just switched on.
		// Since the 'PREarningType' DAC is a CacheExtension of the 'EPEarningType' DAC, a Left Join operator will be applied in a generated SQL code.
		// This is the reason why we need the following condition in SQL:
		// ... AND (IsAmountBased IS NULL AND IsPTO IS NULL AND IsPiecework IS NULL   OR   IsAmountBased = 0 AND IsPTO = 0 AND IsPiecework = 0) ...
		[PXSelector(typeof(SearchFor<EPEarningType.typeCD>
			.Where<EPEarningType.isActive.IsEqual<True>
				.And<EPEarningType.isOvertime.IsEqual<False>>
				.And<PREarningType.isPiecework.IsNull
					.And<PREarningType.isAmountBased.IsNull>
					.And<PREarningType.isPTO.IsNull>
					.Or<PREarningType.isPiecework.IsEqual<False>
						.And<PREarningType.isAmountBased.IsEqual<False>>
						.And<PREarningType.isPTO.IsEqual<False>>>>>),
			DescriptionField = typeof(EPEarningType.description))]
		[PXUIVisible(typeof(Where<PREarningType.isPTO.IsEqual<True>>))]
		[PXFormula(typeof(Switch<Case<Where<PREarningType.isPTO.IsNotEqual<True>>, Null>>))]
		[PXReferentialIntegrityCheck]
		public string RegularTypeCD { get; set; }
		#endregion
		#region WageTypeCD
		public abstract class wageTypeCD : BqlInt.Field<wageTypeCD> { }
		/// <summary>
		/// The user-friendly unique identifier of the wage type. The tax engine uses this value to determine the rate to apply to the earning code.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Wage Type", Required = true, FieldClass = nameof(FeaturesSet.PayrollUS))]
		[PRTypeSelector(typeof(PRWage), PRWage.DefaultWageType, LocationConstants.USCountryCode)]
		public int? WageTypeCD { get; set; }
		#endregion
		#region IsWCCCalculation
		public abstract class isWCCCalculation : BqlBool.Field<isWCCCalculation> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the system uses a WC code from the employee time activities or payroll settings when inserting an earning line in a paycheck or in a payroll batch.
		/// </summary>
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Contributes to WCC Calculation")]
		public bool? IsWCCCalculation { get; set; }
		#endregion
		#region IsAmountBased
		public abstract class isAmountBased : BqlBool.Field<isAmountBased> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the earning type is based on a fixed amount as opposed to a rate.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Amount-Based", Visible = false)]
		public bool? IsAmountBased { get; set; }
		#endregion
		#region IsPiecework
		public abstract class isPiecework : BqlBool.Field<isPiecework> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the earning type is based on units other than hours or years.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Piecework", Visible = false)]
		public bool? IsPiecework { get; set; }
		#endregion
		#region IsTimeOff
		public abstract class isPTO : BqlBool.Field<isPTO> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the earning type represents paid time-off earnings.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Time Off", Visible = false)]
		public bool? IsPTO { get; set; }
		#endregion
		#region EarningTypeCategory
		public abstract class earningTypeCategory : BqlString.Field<earningTypeCategory> { }
		/// <summary>
		/// The category to which an earning type belongs.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="EarningTypeCategory.ListAttribute"/>.
		/// </value>
		[PXString(3, IsUnicode = false, InputMask = ">LLL", IsFixed = true)]
		[EarningTypeCategory.List]
		[PXUIField(DisplayName = "Earning Type Category")]
		public string EarningTypeCategory { get; set; }
		#endregion
		#region IncludeType
		public abstract class includeType : BqlString.Field<includeType> { }
		/// <summary>
		/// The method to be used to determine the list of applicable taxes.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="SubjectToTaxes.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Subject to Taxes", Required = true, FieldClass = nameof(FeaturesSet.PayrollUS))]
		[PXDefault(SubjectToTaxes.PerTaxEngine, PersistingCheck = PXPersistingCheck.Nothing)]
		[SubjectToTaxes.List]
		public string IncludeType { get; set; }
		#endregion
		#region ReportType
		public abstract class reportType : BqlInt.Field<reportType> { }
		/// <summary>
		/// A code that determines whether this earning type will appear in Box 12 of the W-2 report and which code it will use.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Reporting Type", Required = true, FieldClass = nameof(FeaturesSet.PayrollUS))]
		[PRReportingTypeSelector(typeof(PRWage), LocationConstants.USCountryCode)]
		public int? ReportType { get; set; }
		#endregion
		#region EarningsAcctID
		public abstract class earningsAcctID : BqlInt.Field<earningsAcctID> { }
		/// <summary>
		/// The unique identifier of the expense account to be used by default to record the earnings.
		/// The field is included in <see cref="FK.EarningsAccount"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Earnings Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PREarningAccountRequired(GLAccountSubSource.EarningType)]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of account if in use.  Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(Field<PREarningType.earningsAcctID>.IsRelatedTo<Account.accountID>), BqlTable = typeof(PREarningType))]
		public int? EarningsAcctID { get; set; }
		#endregion
		#region EarningsSubID
		public abstract class earningsSubID : BqlInt.Field<earningsSubID> { }
		/// <summary>
		/// The unique identifier of the corresponding subaccount to be used with the earnings account.
		/// The field is included in <see cref="FK.EarningsSubaccount"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(PREarningType.earningsAcctID), DisplayName = "Earnings Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PREarningSubRequired(GLAccountSubSource.EarningType)]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of subaccount if in use. Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(Field<PREarningType.earningsSubID>.IsRelatedTo<Sub.subID>), BqlTable = typeof(PREarningType))]
		public int? EarningsSubID { get; set; }
		#endregion
		#region AccruePTO
		public abstract class accruePTO : BqlBool.Field<accruePTO> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the hours linked with the code will be considered for PTO calculation.
		/// </summary>
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Accrue Time Off")]
		public bool? AccruePTO { get; set; }
		#endregion
		#region PublicHoliday
		public abstract class publicHoliday : BqlBool.Field<publicHoliday> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the type refers to earnings guaranteed by a mandatory holiday.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Public Holiday", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXUIVisible(typeof(Where<isPTO.IsEqual<True>>))]
		[PXFormula(typeof(Switch<Case<Where<isPTO.IsNotEqual<True>>, False>, publicHoliday>))]
		public bool? PublicHoliday { get; set; }
		#endregion
		#region BenefitExpenseAcctID
		public abstract class benefitExpenseAcctID : BqlInt.Field<benefitExpenseAcctID> { }
		/// <summary>
		/// The unique identifier of the expense account to be used by default to record the benefit expense linked with the earning.
		/// The field is included in <see cref="FK.BenefitExpenseAccount"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Benefit Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PRBenExpenseAccountRequired(GLAccountSubSource.EarningType)]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of account if in use.  Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(Field<PREarningType.benefitExpenseAcctID>.IsRelatedTo<Account.accountID>), BqlTable = typeof(PREarningType))]
		public int? BenefitExpenseAcctID { get; set; }
		#endregion
		#region BenefitExpenseSubID
		public abstract class benefitExpenseSubID : BqlInt.Field<benefitExpenseSubID> { }
		/// <summary>
		/// The unique identifier of the corresponding subaccount to be used with the benefit expense account.
		/// The field is included in <see cref="FK.BenefitExpenseSubaccount"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(PREarningType.benefitExpenseAcctID), DisplayName = "Benefit Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PRBenExpenseSubRequired(GLAccountSubSource.EarningType)]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of subaccount if in use. Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(Field<PREarningType.benefitExpenseSubID>.IsRelatedTo<Sub.subID>), BqlTable = typeof(PREarningType))]
		public int? BenefitExpenseSubID { get; set; }
		#endregion
		#region TaxExpenseAcctID
		public abstract class taxExpenseAcctID : BqlInt.Field<taxExpenseAcctID> { }
		/// <summary>
		/// The unique identifier of the expense account to be used by default to record the tax expenses linked with the earning.
		/// The field is included in <see cref="FK.TaxExpenseAccount"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Tax Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PRTaxExpenseAccountRequired(GLAccountSubSource.EarningType)]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of account if in use.  Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(Field<PREarningType.taxExpenseAcctID>.IsRelatedTo<Account.accountID>), BqlTable = typeof(PREarningType))]
		public int? TaxExpenseAcctID { get; set; }
		#endregion
		#region TaxExpenseSubID
		public abstract class taxExpenseSubID : BqlInt.Field<taxExpenseSubID> { }
		/// <summary>
		/// The unique identifier of the corresponding subaccount to be used with the tax expense account.
		/// The field is included in <see cref="FK.TaxExpenseSubaccount"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(PREarningType.taxExpenseAcctID), DisplayName = "Tax Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PRTaxExpenseSubRequired(GLAccountSubSource.EarningType)]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of subaccount if in use. Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(Field<PREarningType.taxExpenseSubID>.IsRelatedTo<Sub.subID>), BqlTable = typeof(PREarningType))]
		public int? TaxExpenseSubID { get; set; }
		#endregion
		#region PTOExpenseAcctID
		public abstract class ptoExpenseAcctID : BqlInt.Field<ptoExpenseAcctID> { }
		/// <summary>
		/// The unique identifier of the expense account to be used by default to record the paid time-off expenses linked with the earning.
		/// The field is included in <see cref="FK.PTOExpenseAccount"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "PTO Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true, FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PRPTOExpenseAccountRequired(GLAccountSubSource.EarningType, typeof(Where<FeatureInstalled<FeaturesSet.payrollCAN>>))]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of account if in use.  Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(FK.PTOExpenseAccount), BqlTable = typeof(PREarningType))]
		public int? PTOExpenseAcctID { get; set; }
		#endregion
		#region PTOExpenseSubID
		public abstract class ptoExpenseSubID : BqlInt.Field<ptoExpenseSubID> { }
		/// <summary>
		/// The unique identifier of the corresponding subaccount to be used with the paid time-off expense account.
		/// The field is included in <see cref="FK.PTOExpenseSubaccount"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(ptoExpenseAcctID), DisplayName = "PTO Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PRPTOExpenseSubRequired(GLAccountSubSource.EarningType, typeof(Where<FeatureInstalled<FeaturesSet.payrollCAN>>))]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of subaccount if in use. Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(FK.PTOExpenseSubaccount), BqlTable = typeof(PREarningType))]
		public int? PTOExpenseSubID { get; set; }
		#endregion
		#region WageTypeCDCAN
		public abstract class wageTypeCDCAN : BqlInt.Field<wageTypeCDCAN> { }
		/// <summary>
		/// The user-friendly unique identifier of the wage type. The tax engine uses this value to determine the rate to apply to the earning code.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Wage Type", Required = true, FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PRWebServiceTypeFromDatabaseSelector(PRTaxWebServiceDataSlot.DataType.WageTypes, LocationConstants.CanadaCountryCode, true)]
		public int? WageTypeCDCAN { get; set; }
		#endregion
		#region IsSupplementalCAN
		public abstract class isSupplementalCAN : BqlBool.Field<isSupplementalCAN> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the earning type represents an additional payment made to an employee outside of their regular wages.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Supplemental Income", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public bool? IsSupplementalCAN { get; set; }
		#endregion
		#region ReportTypeCAN
		public abstract class reportTypeCAN : BqlInt.Field<reportTypeCAN> { }
		/// <summary>
		/// The unique identifier of the federal reporting type which the earning type corresponds to, if any.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Federal Reporting Type", Required = true, FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PRWebServiceTypeFromDatabaseSelector(PRTaxWebServiceDataSlot.DataType.ReportingTypes, LocationConstants.CanadaCountryCode, true)]
		public int? ReportTypeCAN { get; set; }
		#endregion
		#region QuebecReportTypeCAN
		public abstract class quebecReportTypeCAN : PX.Data.BQL.BqlInt.Field<quebecReportTypeCAN> { }
		/// <summary>
		/// The Quebec reporting type.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Quebec Reporting Type", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PRWebServiceTypeFromDatabaseSelector(PRTaxWebServiceDataSlot.DataType.QuebecReportingTypes, LocationConstants.CanadaCountryCode, false)]
		public int? QuebecReportTypeCAN { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : BqlGuid.Field<noteID> { }
		[PXNote]
		public Guid? NoteID { get; set; }
		#endregion
	}
}
