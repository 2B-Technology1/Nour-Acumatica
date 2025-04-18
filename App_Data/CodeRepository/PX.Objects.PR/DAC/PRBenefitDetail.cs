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
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// The child record belonging to a <see cref="PRPayment"/> and representing a benefit code applied to it.
	/// </summary>
	[PXCacheName(Messages.PRBenefitDetail)]
	[Serializable]
	public class PRBenefitDetail : IBqlTable, IPaycheckExpenseDetail<int?>
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRBenefitDetail>.By<recordID>
		{
			public static PRBenefitDetail Find(PXGraph graph, int? recordID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, recordID, options);
		}

		public class UK : PrimaryKeyOf<PRBenefitDetail>.By<branchID, paymentDocType, paymentRefNbr, codeID, projectID, projectTaskID, labourItemID, earningTypeCD, costCodeID>
		{
			public static PRBenefitDetail Find(PXGraph graph, int? branchID, string paymentDocType, string paymentRefNbr, int? codeID, int? projectID, int? projectTaskID, int? laborItemID, string earningTypeCD, int? costCodeID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, branchID, paymentDocType, paymentRefNbr, codeID, projectID, projectTaskID, laborItemID, earningTypeCD, costCodeID, options);
		}

		public static class FK
		{
			public class Employee : PREmployee.PK.ForeignKeyOf<PRBenefitDetail>.By<employeeID> { }
			public class PayrollBatch : PRBatch.PK.ForeignKeyOf<PRBenefitDetail>.By<batchNbr> { }
			public class Payment : PRPayment.PK.ForeignKeyOf<PRBenefitDetail>.By<paymentDocType, paymentRefNbr> { }
			public class Branch : GL.Branch.PK.ForeignKeyOf<PRBenefitDetail>.By<branchID> { }
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRBenefitDetail>.By<codeID> { }
			public class LaborItem : InventoryItem.PK.ForeignKeyOf<PRBenefitDetail>.By<labourItemID> { }
			public class EarningType : EPEarningType.PK.ForeignKeyOf<PRBenefitDetail>.By<earningTypeCD> { }
			public class ExpenseAccount : Account.PK.ForeignKeyOf<PRBenefitDetail>.By<expenseAccountID> { }
			public class ExpenseSubaccount : Sub.PK.ForeignKeyOf<PRBenefitDetail>.By<expenseSubID> { }
			public class LiabilityAccount : Account.PK.ForeignKeyOf<PRBenefitDetail>.By<liabilityAccountID> { }
			public class LiabilitySubaccount : Sub.PK.ForeignKeyOf<PRBenefitDetail>.By<liabilitySubID> { }
			public class Project : PMProject.PK.ForeignKeyOf<PRBenefitDetail>.By<projectID> { }
			public class ProjectTask : PMTask.PK.ForeignKeyOf<PRBenefitDetail>.By<projectID, projectTaskID> { }
			public class CostCode : PMCostCode.PK.ForeignKeyOf<PRBenefitDetail>.By<costCodeID> { }
			public class Invoice : APInvoice.PK.ForeignKeyOf<PRBenefitDetail>.By<apInvoiceDocType, apInvoiceRefNbr> { }
			public class PaymentBenefit : PRPaymentDeduct.PK.ForeignKeyOf<PRBenefitDetail>.By<paymentDocType, paymentRefNbr, codeID> { }
		}
		#endregion

		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		/// <summary>
		/// The unique identifier of the benefit detail record.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID { get; set; }
		#endregion
		#region OriginalRecordID
		public abstract class originalRecordID : PX.Data.BQL.BqlInt.Field<originalRecordID> { }
		[PXDBInt]
		public virtual Int32? OriginalRecordID { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		/// <summary>
		/// The unique identifier of the employee to which the detail record is attached.
		/// The field is included in <see cref="FK.Employee"/>.
		/// </summary>
		[Employee]
		[PXDefault(typeof(PRPayment.employeeID.FromCurrent))]
		public int? EmployeeID { get; set; }
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		/// <summary>
		/// The number of the batch (if any) to which the detail's parent payment belongs.
		/// The field is included in <see cref="FK.PayrollBatch"/>.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number")]
		[PXDBDefault(typeof(PRBatch.batchNbr), DefaultForUpdate = true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXParent(typeof(Select<PRBatch, Where<PRBatch.batchNbr, Equal<Current<PRBenefitDetail.batchNbr>>>>))]
		public string BatchNbr { get; set; }
		#endregion
		#region PaymentDocType
		public abstract class paymentDocType : PX.Data.BQL.BqlString.Field<paymentDocType> { }
		/// <summary>
		/// The type of the parent paycheck.
		/// The field is included in <see cref="FK.Payment"/> and <see cref="FK.PaymentBenefit"/>.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PayrollType.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Payment Doc. Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		public string PaymentDocType { get; set; }
		#endregion
		#region PaymentRefNbr
		public abstract class paymentRefNbr : PX.Data.BQL.BqlString.Field<paymentRefNbr> { }
		/// <summary>
		/// The reference number of the parent paycheck.
		/// The field is included in <see cref="FK.Payment"/> and <see cref="FK.PaymentBenefit"/>.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Ref. Number")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(Select<PRPayment, Where<PRPayment.docType, Equal<Current<paymentDocType>>, And<PRPayment.refNbr, Equal<Current<paymentRefNbr>>>>>))]
		[PXFormula(null, typeof(CountCalc<PRPayment.detailLinesCount>))]
		public string PaymentRefNbr { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		/// <summary>
		/// The branch linked to the benefit.
		/// The field is included in <see cref="FK.Branch"/>.
		/// </summary>
		[GL.Branch(typeof(Parent<PRPayment.branchID>), typeof(SearchFor<BranchWithAddress.branchID>), IsDetail = false)]
		public int? BranchID { get; set; }
		#endregion
		#region CodeID
		public abstract class codeID : PX.Data.BQL.BqlInt.Field<codeID> { }
		/// <summary>
		/// The benefit code.
		/// The field is included in <see cref="FK.DeductionCode"/> and <see cref="FK.PaymentBenefit"/>.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(
			typeof(SearchFor<PRDeductCode.codeID>
				.Where<paymentDocType.FromCurrent.IsEqual<PayrollType.voidCheck>
					.And<PRDeductCode.countryID.IsEqual<paymentCountryID.FromCurrent>>
					.Or<PRDeductCode.contribType.IsNotEqual<ContributionTypeListAttribute.employeeDeduction>
						.And<PRDeductCode.noFinancialTransaction.IsEqual<False>>>>),
			SubstituteKey = typeof(PRDeductCode.codeCD),
			DescriptionField = typeof(PRDeductCode.description))]
		[PXCheckUnique(typeof(branchID), typeof(paymentRefNbr), typeof(paymentDocType), typeof(projectID), typeof(projectTaskID), typeof(labourItemID), typeof(earningTypeCD), typeof(costCodeID),
			ErrorMessage = Messages.CantDuplicateBenefitDetail,
			ClearOnDuplicate = false)]
		[PXRestrictor(typeof(
			Where<PRDeductCode.isActive.IsEqual<True>>),
			Messages.DeductCodeInactive)]
		public int? CodeID { get; set; }
		#endregion
		#region IsPayableBenefit
		public abstract class isPayableBenefit : PX.Data.BQL.BqlBool.Field<isPayableBenefit> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the benefit may contribute to gross calculation.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Selector<codeID, PRDeductCode.isPayableBenefit>))]
		[PXUIField(Visible = false)]
		public bool? IsPayableBenefit { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		/// <summary>
		/// The benefit amount.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIEnabled(typeof(Where<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>))]
		public virtual Decimal? Amount { get; set; }
		#endregion
		#region LabourItemID
		public abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }
		/// <summary>
		/// The unique identifier of the labor item (if any).
		/// The field is included in <see cref="FK.LaborItem"/>.
		/// </summary>
		[PMLaborItem(typeof(projectID), null, null)]
		[PXForeignReference(typeof(Field<labourItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.BenefitLaborItem, Equal<True>>))]
		[PXUIEnabled(typeof(Where<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>))]
		public virtual int? LabourItemID { get; set; }
		#endregion
		#region EarningTypeCD
		public abstract class earningTypeCD : PX.Data.BQL.BqlString.Field<earningTypeCD> { }
		/// <summary>
		/// The user-friendly unique identifier of the earning type.
		/// The field is included in <see cref="FK.EarningType"/>.
		/// </summary>
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXUIField(DisplayName = "Earning Type Code")]
		[PREarningTypeSelector]
		[PXForeignReference(typeof(Field<earningTypeCD>.IsRelatedTo<EPEarningType.typeCD>))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.BenefitEarningType, Equal<True>>))]
		[PXUIEnabled(typeof(Where<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>))]
		public string EarningTypeCD { get; set; }
		#endregion
		#region ExpenseAccountID
		public abstract class expenseAccountID : PX.Data.BQL.BqlInt.Field<expenseAccountID> { }
		/// <summary>
		/// The expense account associated with the benefit code.
		/// The field is included in <see cref="FK.ExpenseAccount"/>.
		/// </summary>
		[BenExpenseAccount(
			typeof(branchID),
			typeof(codeID),
			typeof(employeeID),
			typeof(PRPayment.payGroupID),
			typeof(earningTypeCD),
			typeof(labourItemID),
			typeof(projectID),
			typeof(projectTaskID),
			DisplayName = "Expense Account")]
		public virtual Int32? ExpenseAccountID { get; set; }
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		/// <summary>
		/// The corresponding subaccount used with the expense account.
		/// The field is included in <see cref="FK.ExpenseSubaccount"/>.
		/// </summary>
		[BenExpenseSubAccount(typeof(PRBenefitDetail.expenseAccountID), typeof(PRBenefitDetail.branchID), true,
			DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible, Filterable = true)]
		public virtual int? ExpenseSubID { get; set; }
		#endregion
		#region LiabilityAccountID
		public abstract class liabilityAccountID : PX.Data.BQL.BqlInt.Field<liabilityAccountID> { }
		/// <summary>
		/// The liability account associated with the benefit code.
		/// The field is included in <see cref="FK.LiabilityAccount"/>.
		/// </summary>
		[BenLiabilityAccount(typeof(PRBenefitDetail.branchID), 
			typeof(PRBenefitDetail.codeID), 
			typeof(PRBenefitDetail.employeeID), 
			typeof(PRPayment.payGroupID), 
			typeof(PRBenefitDetail.codeID), 
			typeof(PRBenefitDetail.isPayableBenefit), DisplayName = "Liability Account")]
		public virtual Int32? LiabilityAccountID { get; set; }
		#endregion
		#region LiabilitySubID
		public abstract class liabilitySubID : PX.Data.BQL.BqlInt.Field<liabilitySubID> { }
		/// <summary>
		/// The corresponding subaccount used with the liability account.
		/// The field is included in <see cref="FK.LiabilitySubaccount"/>.
		/// </summary>
		[BenLiabilitySubAccount(typeof(PRBenefitDetail.liabilityAccountID), typeof(PRBenefitDetail.branchID), typeof(PRBenefitDetail.isPayableBenefit), true,
			DisplayName = "Liability Sub.", Visibility = PXUIVisibility.Visible, Filterable = true)]
		public virtual int? LiabilitySubID { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		/// <summary>
		/// The unique identifier of the associated project (if any).
		/// The field is included in <see cref="FK.Project"/>.
		/// </summary>
		[ProjectBase(DisplayName = "Project")]
		[ProjectDefault]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.BenefitProject, Equal<True>>))]
		[PXUIEnabled(typeof(Where<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>))]
		public int? ProjectID { get; set; }
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		/// <summary>
		/// The unique identifier of the associated project task (if any).
		/// The field is included in <see cref="FK.ProjectTask"/>.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Project Task", FieldClass = ProjectAttribute.DimensionName)]
		[PXSelector(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<PRBenefitDetail.projectID>>>>),
			typeof(PMTask.taskCD), typeof(PMTask.description), SubstituteKey = typeof(PMTask.taskCD))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.BenefitProject, Equal<True>>))]
		[PXUIEnabled(typeof(Where<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>))]
		[PXForeignReference(typeof(FK.ProjectTask))]
		public int? ProjectTaskID { get; set; }
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		/// <summary>
		/// The unique identifier of the WCC code applied (if any).
		/// The field is included in <see cref="FK.CostCode"/>.
		/// </summary>
		[CostCode(typeof(liabilityAccountID), typeof(projectTaskID), GL.AccountType.Expense, SkipVerificationForDefault = true, AllowNullValue = true, ReleasedField = typeof(released))]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public virtual Int32? CostCodeID { get; set; }
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the parent payment's status is set to <see cref="PaymentStatus.Released"/>.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public virtual Boolean? Released { get; set; }
		#endregion
		#region APInvoiceDocType
		public abstract class apInvoiceDocType : PX.Data.BQL.BqlString.Field<apInvoiceDocType> { }
		/// <summary>
		/// The type of the parent invoice (if any).
		/// The field is included in <see cref="FK.Invoice"/>.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="APMigrationModeDependentActionRestrictionAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Type")]
		public string APInvoiceDocType { get; set; }
		#endregion
		#region APInvoiceRefNbr
		public abstract class apInvoiceRefNbr : PX.Data.BQL.BqlString.Field<apInvoiceRefNbr> { }
		/// <summary>
		/// The reference number of the parent invoice (if any).
		/// The field is included in <see cref="FK.Payment"/>.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Reference Nbr.")]
		public string APInvoiceRefNbr { get; set; }
		#endregion
		#region LiabilityPaid
		public abstract class liabilityPaid : PX.Data.BQL.BqlBool.Field<liabilityPaid> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the benefit has been paid.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Liability Paid")]
		public virtual Boolean? LiabilityPaid { get; set; }
		#endregion

		#region PaymentCountryID
		[PXString(2)]
		[PXUnboundDefault(typeof(Parent<PRPayment.countryID>))]
		public virtual string PaymentCountryID { get; set; }
		public abstract class paymentCountryID : PX.Data.BQL.BqlString.Field<paymentCountryID> { }
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

		public int? ParentKeyID { get => CodeID; set => CodeID = value; }
	}
}
