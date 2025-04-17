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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Includes the settings of the deduction or benefit code which may be used for the current employee.
	/// </summary>
	[PXCacheName(Messages.PREmployeeDeduct)]
	[Serializable]
	public class PREmployeeDeduct : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PREmployeeDeduct>.By<bAccountID, lineNbr>
		{
			public static PREmployeeDeduct Find(PXGraph graph, int? bAccountID, int? lineNbr, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, bAccountID, lineNbr, options);
		}

		public static class FK
		{
			public class Employee : PREmployee.PK.ForeignKeyOf<PREmployeeDeduct>.By<bAccountID> { }
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PREmployeeDeduct>.By<codeID> { }
			public class GarnishmentVendor : Vendor.PK.ForeignKeyOf<PREmployeeDeduct>.By<garnBAccountID> { }
		}
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		/// <summary>
		/// The unique identifier of the business account associated with the employee.
		/// The field is included in <see cref="FK.Employee"/>.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PREmployee.bAccountID))]
		[PXParent(typeof(Select<PREmployee, Where<PREmployee.bAccountID, Equal<Current<PREmployeeDeduct.bAccountID>>>>))]
		public int? BAccountID { get; set; }
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PREmployee.lineCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual int? LineNbr { get; set; }
		#endregion
		#region CodeID
		public abstract class codeID : PX.Data.BQL.BqlInt.Field<codeID> { }
		/// <summary>
		/// The unique identifier for this deduction or benefit.
		/// The field is included in <see cref="FK.DeductionCode"/>.
		/// </summary>
		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Deduction Code")]
		[DeductionActiveSelector(
			typeof(Where<PRDeductCode.isWorkersCompensation.IsEqual<False>
				.And<PRDeductCode.isCertifiedProject.IsEqual<False>>
				.And<PRDeductCode.isUnion.IsEqual<False>>>),
			typeof(employeeCountryID))]
		[PXForeignReference(typeof(FK.DeductionCode))]
		public int? CodeID { get; set; }
		#endregion
		#region ContribType
		public abstract class contribType : PX.Data.BQL.BqlString.Field<contribType> { }
		/// <summary>
		/// The type of a code that defines how the code affects employee earnings.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="ContributionTypeListAttribute"/>.
		/// </value>
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Contribution Type")]
		[ContributionTypeList]
		[PXFormula(typeof(Selector<codeID, PRDeductCode.contribType>))]
		public string ContribType { get; set; }
		#endregion
		#region CntCalcType
		public abstract class cntCalcType : PX.Data.BQL.BqlString.Field<cntCalcType> { }
		/// <summary>
		/// The method to be used for determining the contribution amount.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="DedCntCalculationMethod.ListAttribute"/>.
		/// </value>
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Calculation Method")]
		[DedCntCalculationMethod.List]
		[PXFormula(typeof(Selector<codeID, PRDeductCode.cntCalcType>))]
		public string CntCalcType { get; set; }
		#endregion
		#region CntMaxFreqType
		public abstract class cntMaxFreqType : PX.Data.BQL.BqlString.Field<cntMaxFreqType> { }
		/// <summary>
		/// How often the maximum contribution amount (if any) specified in the Contribution Max column is to be applied.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="DeductionMaxFrequencyType.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Contribution Limit Frequency")]
		[DeductionMaxFrequencyType.List]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employeeDeduction>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employeeDeduction>,
			And<PREmployeeDeduct.cntUseDflt, Equal<False>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.cntMaxFreqType), typeof(PREmployeeDeduct.dedUseDflt), ManageUIEnabled = false)]
		public string CntMaxFreqType { get; set; }
		#endregion
		#region DedCalcType
		public abstract class dedCalcType : PX.Data.BQL.BqlString.Field<dedCalcType> { }
		/// <summary>
		/// The method to be used for determining the deduction amount.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="DedCntCalculationMethod.ListAttribute"/>.
		/// </value>
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Calculation Method")]
		[DedCntCalculationMethod.List]
		[PXFormula(typeof(Selector<codeID, PRDeductCode.dedCalcType>))]
		public string DedCalcType { get; set; }
		#endregion
		#region DedMaxFreqType
		public abstract class dedMaxFreqType : PX.Data.BQL.BqlString.Field<dedMaxFreqType> { }
		/// <summary>
		/// How often the maximum deduction amount (if any) specified in the Deduction Max column is to be applied.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="DeductionMaxFrequencyType.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Deduction Limit Frequency")]
		[DeductionMaxFrequencyType.List]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employerContribution>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employerContribution>,
			And<PREmployeeDeduct.dedUseDflt, Equal<False>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.dedMaxFreqType), typeof(PREmployeeDeduct.dedUseDflt), ManageUIEnabled = false)]
		public string DedMaxFreqType { get; set; }
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		/// <summary>
		/// The date when this deduction or benefit came into effect for this employee.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Start Date", Required = true)]
		[PXDBDefault]
		public DateTime? StartDate { get; set; }
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		/// <summary>
		/// The date when this deduction or benefit became inactive for this employee.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "End Date")]
		public DateTime? EndDate { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the code is available for use.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public bool? IsActive { get; set; }
		#endregion
		#region DedAmount
		public abstract class dedAmount : PX.Data.BQL.BqlDecimal.Field<dedAmount> { }
		/// <summary>
		/// The amount to be used if the calculation method for this deduction is set to <see cref="DedCntCalculationMethod.FixedAmount"/> or <see cref="DedCntCalculationMethod.AmountPerHour"/>.
		/// </summary>
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Deduction Amount")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employerContribution>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfGross>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfCustom>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfNet>,
			And<PREmployeeDeduct.dedCalcType, IsNotNull>>>>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employerContribution>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfGross>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfCustom>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfNet>,
			And<PREmployeeDeduct.dedCalcType, IsNotNull,
			And<PREmployeeDeduct.dedUseDflt, Equal<False>>>>>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.dedAmount), typeof(PREmployeeDeduct.dedUseDflt), ManageUIEnabled = false)]
		public Decimal? DedAmount { get; set; }
		#endregion
		#region DedPercent
		public abstract class dedPercent : PX.Data.BQL.BqlDecimal.Field<dedPercent> { }
		/// <summary>
		/// The percentage to be used if the calculation method for this deduction is set to <see cref="DedCntCalculationMethod.PercentOfGross"/> or <see cref="DedCntCalculationMethod.PercentOfNet"/>.
		/// </summary>
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Deduction Percent")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employerContribution>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.fixedAmount>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.amountPerHour>,
			And<PREmployeeDeduct.dedCalcType, IsNotNull>>>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employerContribution>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.fixedAmount>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.amountPerHour>,
			And<PREmployeeDeduct.dedCalcType, IsNotNull,
			And<PREmployeeDeduct.dedUseDflt, Equal<False>>>>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.dedPercent), typeof(PREmployeeDeduct.dedUseDflt), ManageUIEnabled = false)]
		public Decimal? DedPercent { get; set; }
		#endregion
		#region DedMaxAmount
		public abstract class dedMaxAmount : PX.Data.BQL.BqlDecimal.Field<dedMaxAmount> { }
		/// <summary>
		/// The deduction amount cap.
		/// </summary>
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Deduction Limit")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employerContribution>,
			And<PREmployeeDeduct.dedMaxFreqType, NotEqual<DeductionMaxFrequencyType.noMaximum>>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employerContribution>,
			And<PREmployeeDeduct.dedMaxFreqType, NotEqual<DeductionMaxFrequencyType.noMaximum>,
			And<PREmployeeDeduct.dedUseDflt, Equal<False>>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.dedMaxAmount), typeof(PREmployeeDeduct.dedUseDflt), ManageUIEnabled = false)]
		public Decimal? DedMaxAmount { get; set; }
		#endregion
		#region DedUseDflt
		public abstract class dedUseDflt : PX.Data.BQL.BqlBool.Field<dedUseDflt> { }
		/// <summary>
		/// A boolean value that indicates (if set to <see langword="true" />) that the values for this row are not stored directly in the employee record but are always derived from the deduction code record.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Use Deduction Defaults")]
		[PXDefault(true)]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employerContribution>>))]
		public bool? DedUseDflt { get; set; }
		#endregion
		#region CntAmount
		public abstract class cntAmount : PX.Data.BQL.BqlDecimal.Field<cntAmount> { }
		/// <summary>
		/// The amount to be used if the calculation method for this contribution is set to <see cref="DedCntCalculationMethod.FixedAmount"/> or <see cref="DedCntCalculationMethod.AmountPerHour"/>.
		/// </summary>
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Contribution Amount")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employeeDeduction>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfGross>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfCustom>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfNet>,
			And<PREmployeeDeduct.cntCalcType, IsNotNull>>>>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employeeDeduction>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfGross>,
			And<PREmployeeDeduct.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfCustom>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfNet>,
			And<PREmployeeDeduct.cntCalcType, IsNotNull,
			And<PREmployeeDeduct.cntUseDflt, Equal<False>>>>>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.cntAmount), typeof(PREmployeeDeduct.cntUseDflt), ManageUIEnabled = false)]
		public Decimal? CntAmount { get; set; }
		#endregion
		#region CntPercent
		public abstract class cntPercent : PX.Data.BQL.BqlDecimal.Field<cntPercent> { }
		/// <summary>
		/// The percentage to be used if the calculation method for this contribution is set to <see cref="DedCntCalculationMethod.PercentOfGross"/> or <see cref="DedCntCalculationMethod.PercentOfNet"/>.
		/// </summary>
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Contribution Percent")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employeeDeduction>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.fixedAmount>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.amountPerHour>,
			And<PREmployeeDeduct.cntCalcType, IsNotNull>>>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employeeDeduction>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.fixedAmount>,
			And<PREmployeeDeduct.cntCalcType, NotEqual<DedCntCalculationMethod.amountPerHour>,
			And<PREmployeeDeduct.cntCalcType, IsNotNull,
			And<PREmployeeDeduct.cntUseDflt, Equal<False>>>>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.cntPercent), typeof(PREmployeeDeduct.cntUseDflt), ManageUIEnabled = false)]
		public Decimal? CntPercent { get; set; }
		#endregion
		#region CntMaxAmount
		public abstract class cntMaxAmount : PX.Data.BQL.BqlDecimal.Field<cntMaxAmount> { }
		/// <summary>
		/// The contribution amount cap.
		/// </summary>
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Contribution Limit")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employeeDeduction>,
			And<PREmployeeDeduct.cntMaxFreqType, NotEqual<DeductionMaxFrequencyType.noMaximum>>>))]
		[PXUIRequired(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employeeDeduction>,
			And<PREmployeeDeduct.cntMaxFreqType, NotEqual<DeductionMaxFrequencyType.noMaximum>,
			And<PREmployeeDeduct.cntUseDflt, Equal<False>>>>))]
		[UseDefaultValue(typeof(PREmployeeDeduct.codeID), typeof(PRDeductCode.cntMaxAmount), typeof(PREmployeeDeduct.cntUseDflt), ManageUIEnabled = false)]
		public Decimal? CntMaxAmount { get; set; }
		#endregion
		#region CntUseDflt
		public abstract class cntUseDflt : PX.Data.BQL.BqlBool.Field<cntUseDflt> { }
		/// <summary>
		/// A boolean value that indicates (if set to <see langword="true" />) that the values for this row are not stored directly in the employee record but are always derived from the benefit code record.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use Contribution Defaults")]
		[PXUIEnabled(typeof(Where<PREmployeeDeduct.contribType, NotEqual<ContributionTypeListAttribute.employeeDeduction>>))]
		public bool? CntUseDflt { get; set; }
		#endregion
		#region Sequence
		public abstract class sequence : PX.Data.BQL.BqlInt.Field<sequence> { }
		/// <summary>
		/// The sequence in which the deduction or benefit is taken.
		/// </summary>
		[PXDBInt(MinValue = 1)]
		[PXDefault]
		[PXUIField(DisplayName = "Sequence", Required = true)]
		[PXUIEnabled(typeof(Where<Selector<codeID, PRDeductCode.affectsTaxes>, Equal<False>>))]
		public int? Sequence { get; set; }
		#endregion
		#region IsGarnishment
		public abstract class isGarnishment : PX.Data.BQL.BqlBool.Field<isGarnishment> { }
		/// <summary>
		/// A boolean value that indicates (if set to <see langword="true" />) that the elements in the Garnishment Details dialog box are available for entry and maintenance.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Selector<codeID, PRDeductCode.isGarnishment>))]
		[PXUIField(DisplayName = "Garnishment", Enabled = false)]
		public bool? IsGarnishment { get; set; }
		#endregion
		#region GarnBAccountID
		public abstract class garnBAccountID : PX.Data.BQL.BqlInt.Field<garnBAccountID> { }
		/// <summary>
		/// The unique identifier of a vendor to whom the garnishment is paid.
		/// The field is included in <see cref="FK.GarnishmentVendor"/>.
		/// </summary>
		[VendorActive]
		[PXUIEnabled(typeof(isGarnishment))]
		public int? GarnBAccountID { get; set; }
		#endregion
		#region VndInvDescr
		public abstract class vndInvDescr : PX.Data.BQL.BqlString.Field<vndInvDescr> { }
		/// <summary>
		/// The description of the vendor invoice.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Vendor Invoice Description")]
		[PXUIEnabled(typeof(isGarnishment))]
		public string VndInvDescr { get; set; }
		#endregion
		#region GarnCourtDate
		public abstract class garnCourtDate : PX.Data.BQL.BqlDateTime.Field<garnCourtDate> { }
		/// <summary>
		/// The court date assigned to the garnishment order.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Court Date")]
		[PXUIEnabled(typeof(isGarnishment))]
		public DateTime? GarnCourtDate { get; set; }
		#endregion
		#region GarnCourtName
		public abstract class garnCourtName : PX.Data.BQL.BqlString.Field<garnCourtName> { }
		/// <summary>
		/// The name of the court that ordered the garnishment.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Court Name")]
		[PXUIEnabled(typeof(isGarnishment))]
		public string GarnCourtName { get; set; }
		#endregion
		#region GarnDocRefNbr
		public abstract class garnDocRefNbr : PX.Data.BQL.BqlString.Field<garnDocRefNbr> { }
		/// <summary>
		/// The unique identifier of the official government document.
		/// </summary>
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Document ID")]
		[PXUIEnabled(typeof(isGarnishment))]
		public string GarnDocRefNbr { get; set; }
		#endregion
		#region GarnOrigAmount
		public abstract class garnOrigAmount : PX.Data.BQL.BqlDecimal.Field<garnOrigAmount> { }
		/// <summary>
		/// The total amount that you enter if the garnishment is for a lump-sum amount that is eventually paid down by the garnishment withholding.
		/// </summary>
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Original Amount")]
		[PXUIEnabled(typeof(isGarnishment))]
		public Decimal? GarnOrigAmount { get; set; }
		#endregion
		#region GarnPaidAmount
		public abstract class garnPaidAmount : PX.Data.BQL.BqlDecimal.Field<garnPaidAmount> { }
		/// <summary>
		/// The life-to-date amount withheld for this garnishment for this employee.
		/// </summary>
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount Paid", Enabled = false)]
		public Decimal? GarnPaidAmount { get; set; }
		#endregion

		#region EmployeeCountryID
		[PXString(2)]
		[PXUnboundDefault(typeof(Parent<PREmployee.countryID>))]
		public virtual string EmployeeCountryID { get; set; }
		public abstract class employeeCountryID : PX.Data.BQL.BqlString.Field<employeeCountryID> { }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
