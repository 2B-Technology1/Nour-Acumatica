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
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the information about the deduction and benefit amounts related to a union. The information will be displayed on the Paychecks and Adjustments (PR302000) form.
	/// </summary>
	[PXCacheName(Messages.PRPaymentUnionPackageDeduct)]
	[Serializable]
	public class PRPaymentUnionPackageDeduct : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRPaymentUnionPackageDeduct>.By<recordID>
		{
			public static PRPaymentUnionPackageDeduct Find(PXGraph graph, string field, PKFindOptions options = PKFindOptions.None) => FindBy(graph, field, options);
		}

		public class UK : PrimaryKeyOf<PRPaymentUnionPackageDeduct>.By<docType, refNbr, deductCodeID, unionID, laborItemID, contribType>
		{
			public static PRPaymentUnionPackageDeduct Find(PXGraph graph, string docType, string refNbr, int? deductCodeID, string unionID, int? laborItemID, string contribType, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, docType, refNbr, deductCodeID, unionID, laborItemID, contribType, options);
		}

		public static class FK
		{
			public class Payment : PRPayment.PK.ForeignKeyOf<PRPaymentUnionPackageDeduct>.By<docType, refNbr> { }
			public class Union : PMUnion.PK.ForeignKeyOf<PRPaymentUnionPackageDeduct>.By<unionID> { }
			public class LaborItem : InventoryItem.PK.ForeignKeyOf<PRPaymentUnionPackageDeduct>.By<laborItemID> { }
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRPaymentUnionPackageDeduct>.By<deductCodeID> { }
		}
		#endregion

		#region RecordID
		public abstract class recordID : BqlInt.Field<recordID> { }
		[PXDBIdentity(IsKey = true)]
		public virtual int? RecordID { get; set; }
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Payment Doc. Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		public string DocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Ref. Number")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(FK.Payment))]
		public String RefNbr { get; set; }
		#endregion
		#region UnionID
		public abstract class unionID : BqlString.Field<unionID> { }
		[PRUnion(DisplayName = "Union")]
		[PXDefault]
		[PXForeignReference(typeof(Field<unionID>.IsRelatedTo<PMUnion.unionID>))]
		public string UnionID { get; set; }
		#endregion
		#region LaborItemID
		public abstract class laborItemID : Data.BQL.BqlInt.Field<laborItemID> { }
		[PMLaborItem(
			null,
			null,
			typeof(SelectFrom<EPEmployee>
				.InnerJoin<PRPayment>.On<PRPayment.docType.IsEqual<docType.FromCurrent>
					.And<PRPayment.refNbr.IsEqual<refNbr.FromCurrent>>>
				.Where<EPEmployee.bAccountID.IsEqual<PRPayment.employeeID>>))]
		[PXForeignReference(typeof(Field<laborItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual int? LaborItemID { get; set; }
		#endregion
		#region DeductCodeID
		public abstract class deductCodeID : PX.Data.BQL.BqlInt.Field<deductCodeID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Deduction Code")]
		[DeductionActiveSelector(typeof(Where<PRDeductCode.isUnion.IsEqual<True>>), typeof(paymentCountryID))]
		[PXRestrictor(
			typeof(Where<Brackets<PRDeductCode.contribType.IsEqual<ContributionTypeListAttribute.employerContribution>
					.Or<PRDeductCode.dedCalcType.IsNotEqual<DedCntCalculationMethod.percentOfNet>>>
				.And<PRDeductCode.contribType.IsEqual<ContributionTypeListAttribute.employeeDeduction>
					.Or<PRDeductCode.cntCalcType.IsNotEqual<DedCntCalculationMethod.percentOfNet>>>>),
			Messages.PercentOfNetInUnion)]
		[PXDefault]
		[PXForeignReference(typeof(Field<deductCodeID>.IsRelatedTo<PRDeductCode.codeID>))]
		[PXCheckUnique(typeof(docType), typeof(refNbr), typeof(unionID), typeof(laborItemID), typeof(contribType), ClearOnDuplicate = false)]
		public int? DeductCodeID { get; set; }
		#endregion
		#region RegularWageBaseHours
		public abstract class regularWageBaseHours : PX.Data.BQL.BqlDecimal.Field<regularWageBaseHours> { }
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Applicable Regular Hours")]
		public decimal? RegularWageBaseHours { get; set; }
		#endregion
		#region OvertimeWageBaseHours
		public abstract class overtimeWageBaseHours : PX.Data.BQL.BqlDecimal.Field<overtimeWageBaseHours> { }
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Applicable Overtime Hours")]
		public decimal? OvertimeWageBaseHours { get; set; }
		#endregion
		#region RegularWageBaseAmount
		public abstract class regularWageBaseAmount : PX.Data.BQL.BqlDecimal.Field<regularWageBaseAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Applicable Regular Wages")]
		public decimal? RegularWageBaseAmount { get; set; }
		#endregion
		#region OvertimeWageBaseAmount
		public abstract class overtimeWageBaseAmount : PX.Data.BQL.BqlDecimal.Field<overtimeWageBaseAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Applicable Overtime Wages")]
		public decimal? OvertimeWageBaseAmount { get; set; }
		#endregion
		#region WageBaseHours
		public abstract class wageBaseHours : PX.Data.BQL.BqlDecimal.Field<wageBaseHours> { }
		[PXDecimal]
		[PXFormula(typeof(Add<regularWageBaseHours, overtimeWageBaseHours>))]
		[PXUIField(DisplayName = "Total Applicable Hours", Enabled = false)]
		public decimal? WageBaseHours { get; set; }
		#endregion
		#region WageBaseAmount
		public abstract class wageBaseAmount : PX.Data.BQL.BqlDecimal.Field<wageBaseAmount> { }
		[PXDecimal]
		[PXFormula(typeof(Add<regularWageBaseAmount, overtimeWageBaseAmount>))]
		[PXUIField(DisplayName = "Total Applicable Wages", Enabled = false)]
		public decimal? WageBaseAmount { get; set; }
		#endregion
		#region DeductionAmount
		public abstract class deductionAmount : PX.Data.BQL.BqlDecimal.Field<deductionAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Deduction Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIEnabled(typeof(Where<contribType.IsNotEqual<ContributionTypeListAttribute.employerContribution>>))]
		public decimal? DeductionAmount { get; set; }
		#endregion
		#region BenefitAmount
		public abstract class benefitAmount : PX.Data.BQL.BqlDecimal.Field<benefitAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Benefit Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIEnabled(typeof(Where<contribType.IsNotEqual<ContributionTypeListAttribute.employeeDeduction>>))]
		public decimal? BenefitAmount { get; set; }
		#endregion
		#region ContribType
		public abstract class contribType : PX.Data.BQL.BqlString.Field<contribType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(typeof(Selector<deductCodeID, PRDeductCode.contribType>))]
		[PXFormula(typeof(Default<deductCodeID>))]
		public string ContribType { get; set; }
		#endregion

		#region DeductionCalcType
		public abstract class deductionCalcType : PX.Data.BQL.BqlString.Field<deductionCalcType> { }
		[PXString(3)]
		[DedCntCalculationMethod.List]
		[PXUIField(DisplayName = "Deduction Calculation Method", Enabled = false)]
		[PXFormula(typeof(Switch<Case<Where<contribType.IsNotEqual<ContributionTypeListAttribute.employerContribution>>, Selector<deductCodeID, PRDeductCode.dedCalcType>>, Null>))]
		public string DeductionCalcType { get; set; }
		#endregion
		#region BenefitCalcType
		public abstract class benefitCalcType : PX.Data.BQL.BqlString.Field<benefitCalcType> { }
		[PXString(3)]
		[DedCntCalculationMethod.List]
		[PXUIField(DisplayName = "Benefit Calculation Method", Enabled = false)]
		[PXFormula(typeof(Switch<Case<Where<contribType.IsNotEqual<ContributionTypeListAttribute.employeeDeduction>>, Selector<deductCodeID, PRDeductCode.cntCalcType>>, Null>))]
		public string BenefitCalcType { get; set; }
		#endregion
		#region PaymentCountryID
		[PXString(2)]
		[PXUnboundDefault(typeof(Parent<PRPayment.countryID>))]
		public virtual string PaymentCountryID { get; set; }
		public abstract class paymentCountryID : PX.Data.BQL.BqlString.Field<paymentCountryID> { }
		#endregion

		#region System columns
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
