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

namespace PX.Objects.PR
{
	/// <summary>
	/// PR and wildcard, to use with Like BQL operator
	/// </summary>
	public class pr_ : PX.Data.BQL.BqlString.Constant<pr_>
	{
		public pr_() : base(PRWildCard) { }
		public const string PRWildCard = "PR%";
	}

	internal static class PRSubAccountMaskConstants
	{
		public const string EarningMaskName = "PREarnings";
		public const string AlternateEarningMaskName = "PREarningsAlternate";
		public const string DeductionMaskName = "PRDeductions";
		public const string BenefitExpenseMaskName = "PRBenefitExpense";
		public const string AlternateBenefitExpenseMaskName = "PRBenefitExpenseAlternate";
		public const string TaxMaskName = "PRTaxes";
		public const string TaxExpenseMaskName = "PRTaxExpense";
		public const string AlternateTaxExpenseMaskName = "PRTaxExpenseAlternate";
		public const string PTOMaskName = "PRPTO";
		public const string PTOExpenseMaskName = "PRPTOExpense";
		public const string AlternatePTOExpenseMaskName = "PRPTOExpenseAlternate";
	}

	public static class PRQueryParameters
	{
		public const string DownloadAuf = "DbgDownloadAuf";
	}

	internal static class PRFileNames
	{
		public const string Auf = "auf.txt";
	}

	public static class PayStubsDirectDepositReportParameters
	{
		public const string ReportID = "PR641015";
		public const string BatchNbr = "DDBatchID";
	}

	public class GLAccountSubSource
	{
		public const string Branch = "B";
		public const string Employee = "E";
		public const string DeductionCode = "D";
		public const string LaborItem = "L";
		public const string PayGroup = "G";
		public const string EarningType = "R";
		public const string TaxCode = "X";
		public const string Project = "J";
		public const string Task = "T";
		public const string PTOBank = "O";
	}

	public static class DateConstants
	{
		public const byte HoursPerDay = 24;
		public const byte HoursPerWeek = 7 * HoursPerDay;
		public const byte WeeksPerYear = 52;
	}
	
	public static class WebserviceContants
	{
		// Has to match the first value defined in PX.Payroll.Data.AatrixPredefinedFields in the payrollws repo
		public const int FirstAatrixPredefinedField = 4000;

		// Has to be greater than the last value defined in PX.Payroll.Data.AatrixPredefinedFields in the payrollws repo
		public const int LastAatrixPredefinedField = 4999;
		
		public const string IncludeRailroadTaxesSetting = "IncludeRailroadTaxes";
		public const string CompanyWagesYtdSetting = "CompanyWagesYtd";
		public const string CompanyWagesQtdSetting = "CompanyWagesQtd";
	}

	public static class RL1BoxNames
	{
		public const string RL1BoxO = "O_AutreRevenu";
		public const string RL1BoxOAmount = "MontantCaseO";
	}

	internal static class PRSelectionPeriodIDs
	{
		public const string LastMonth = "LastMonth";
		public const string Last12Months = "Last12Months";
		public const string CurrentQuarter = "CurrentQuarter";
		public const string CurrentCalYear = "CurrentCalYear";
		public const string CurrentFinYear = "CurrentFinYear";
	}

	// TO-DO: Remove temporary EmploymentCodes during implementation of AC-275100: PX.Payroll 2b: Move Reporting Type information to the App
	public static class EmploymentCodes
	{
		public const string PlacementAgencyWorker = "11";
		public const string TaxiDrivers = "12";
		public const string Barbers = "13";
		public const string WithdrawalFromPrescribedSalary = "14";
		public const string SeasonalAgricultural = "15";
		public const string DetachedEmployee = "16";
		public const string Fishers = "17";
	}

	// TO-DO: Remove temporary ReportingCodes during implementation of AC-275100: PX.Payroll 2b: Move Reporting Type information to the App
	public static class ReportingCodesForT4OtherSection
	{
		public const string BoardAndLodging = "Box30";
		public const string PersonalUseOfVehicle = "Box34";
		public const string OtherTaxableAllowances = "Box40";
	}
}
