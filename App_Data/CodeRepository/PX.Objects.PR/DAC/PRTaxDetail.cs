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
using System;
using System.Linq;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the information related to the tax amounts for a specific paycheck.
	/// The information will be displayed on the Paychecks and Adjustments (PR302000) form.
	/// </summary>
	[PXCacheName(Messages.PRTaxDetail)]
	[Serializable]
	public class PRTaxDetail : IBqlTable, IPaycheckExpenseDetail<int?>
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRTaxDetail>.By<recordID>
		{
			public static PRTaxDetail Find(PXGraph graph, int? recordID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, recordID, options);
		}

		public class UK : PrimaryKeyOf<PRTaxDetail>.By<branchID, paymentDocType, paymentRefNbr, taxID, projectID, projectTaskID, labourItemID, earningTypeCD, costCodeID>
		{
			public static PRTaxDetail Find(PXGraph graph, int? branchID, string paymentDocType, string paymentRefNbr, int? taxID, int? projectID, int? projectTaskID, int? laborItemID, string earningTypeCD, int? costCodeID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, branchID, paymentDocType, paymentRefNbr, taxID, projectID, projectTaskID, laborItemID, earningTypeCD, costCodeID, options);
		}

		public static class FK
		{
			public class Employee : PREmployee.PK.ForeignKeyOf<PRTaxDetail>.By<employeeID> { }
			public class PayrollBatch : PRBatch.PK.ForeignKeyOf<PRTaxDetail>.By<batchNbr> { }
			public class Payment : PRPayment.PK.ForeignKeyOf<PRTaxDetail>.By<paymentDocType, paymentRefNbr> { }
			public class Branch : GL.Branch.PK.ForeignKeyOf<PRTaxDetail>.By<branchID> { }
			public class TaxCode : PRTaxCode.PK.ForeignKeyOf<PRTaxDetail>.By<taxID> { }
			public class PaymentTax : PRPaymentTax.PK.ForeignKeyOf<PRTaxDetail>.By<paymentDocType, paymentRefNbr, taxID> { }
			public class LaborItem : InventoryItem.PK.ForeignKeyOf<PRTaxDetail>.By<labourItemID> { }
			public class EarningType : EPEarningType.PK.ForeignKeyOf<PRTaxDetail>.By<earningTypeCD> { }
			public class ExpenseAccount : Account.PK.ForeignKeyOf<PRTaxDetail>.By<expenseAccountID> { }
			public class ExpenseSubaccount : Sub.PK.ForeignKeyOf<PRTaxDetail>.By<expenseSubID> { }
			public class LiabilityAccount : Account.PK.ForeignKeyOf<PRTaxDetail>.By<liabilityAccountID> { }
			public class LiabilitySubaccount : Sub.PK.ForeignKeyOf<PRTaxDetail>.By<liabilitySubID> { }
			public class Project : PMProject.PK.ForeignKeyOf<PRTaxDetail>.By<projectID> { }
			public class ProjectTask : PMTask.PK.ForeignKeyOf<PRTaxDetail>.By<projectID, projectTaskID> { }
			public class CostCode : PMCostCode.PK.ForeignKeyOf<PRTaxDetail>.By<costCodeID> { }
			public class Invoice : APInvoice.PK.ForeignKeyOf<PRTaxDetail>.By<apInvoiceDocType, apInvoiceRefNbr> { }
		}
		#endregion

		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
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
		[Employee]
		[PXDefault(typeof(PRPayment.employeeID.FromCurrent))]
		public int? EmployeeID { get; set; }
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number")]
		[PXDBDefault(typeof(PRBatch.batchNbr), DefaultForUpdate = true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXParent(typeof(Select<PRBatch, Where<PRBatch.batchNbr, Equal<Current<PRTaxDetail.batchNbr>>>>))]
		public string BatchNbr { get; set; }
		#endregion
		#region PaymentDocType
		public abstract class paymentDocType : PX.Data.BQL.BqlString.Field<paymentDocType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Payment Doc. Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		public string PaymentDocType { get; set; }
		#endregion
		#region PaymentRefNbr
		public abstract class paymentRefNbr : PX.Data.BQL.BqlString.Field<paymentRefNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Ref. Number")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(Select<PRPayment, Where<PRPayment.docType, Equal<Current<paymentDocType>>, And<PRPayment.refNbr, Equal<Current<paymentRefNbr>>>>>))]
		[PXFormula(null, typeof(CountCalc<PRPayment.detailLinesCount>))]
		public string PaymentRefNbr { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[GL.Branch(typeof(Parent<PRPayment.branchID>), typeof(SearchFor<BranchWithAddress.branchID>), IsDetail = false)]
		public int? BranchID { get; set; }
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlInt.Field<taxID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Tax", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(typeof(
			SearchFor<PRTaxCode.taxID>
				.Where<PRTaxCode.countryID.IsEqual<paymentCountryID.FromCurrent>>),
			DescriptionField = typeof(PRTaxCode.description),
			SubstituteKey = typeof(PRTaxCode.taxCD))]
		[PXParent(typeof(Select<PRPaymentTax,
						Where<PRPaymentTax.docType, Equal<Current<PRTaxDetail.paymentDocType>>,
							And<PRPaymentTax.refNbr, Equal<Current<PRTaxDetail.paymentRefNbr>>,
							And<PRPaymentTax.taxID, Equal<Current<PRTaxDetail.taxID>>>>>>))]
		[PXCheckUnique(typeof(branchID), typeof(paymentRefNbr), typeof(paymentDocType), typeof(projectID), typeof(projectTaskID), typeof(labourItemID), typeof(earningTypeCD), typeof(costCodeID),
			ErrorMessage = Messages.CantDuplicateTaxDetail)]
		public int? TaxID { get; set; }
		#endregion
		#region TaxCategory
		public abstract class taxCategory : PX.Data.BQL.BqlString.Field<taxCategory> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Tax Category", Enabled = false)]
		[TaxCategory.List]
		[PXFormula(typeof(Selector<PRTaxDetail.taxID, PRTaxCode.taxCategory>))]
		public string TaxCategory { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIEnabled(typeof(Where<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>))]
		public virtual Decimal? Amount { get; set; }
		#endregion
		#region LabourItemID
		public abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }
		[PMLaborItem(typeof(projectID), null, null)]
		[PXForeignReference(typeof(Field<labourItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.TaxLaborItem, Equal<True>>))]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>
			.And<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>>))]
		public virtual int? LabourItemID { get; set; }
		#endregion
		#region EarningTypeCD
		public abstract class earningTypeCD : PX.Data.BQL.BqlString.Field<earningTypeCD> { }
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXUIField(DisplayName = "Earning Type Code")]
		[PREarningTypeSelector]
		[PXForeignReference(typeof(Field<earningTypeCD>.IsRelatedTo<EPEarningType.typeCD>))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.TaxEarningType, Equal<True>>))]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>
			.And<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>>))]
		public string EarningTypeCD { get; set; }
		#endregion
		#region ExpenseAccountID
		public abstract class expenseAccountID : PX.Data.BQL.BqlInt.Field<expenseAccountID> { }
		[TaxExpenseAccount(
			typeof(branchID),
			typeof(taxID),
			typeof(employeeID),
			typeof(PRPayment.payGroupID),
			typeof(taxCategory),
			typeof(earningTypeCD),
			typeof(labourItemID),
			typeof(projectID),
			typeof(projectTaskID),
			DisplayName = "Expense Account")]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>>))]
		public virtual Int32? ExpenseAccountID { get; set; }
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		[TaxExpenseSubAccount(typeof(PRTaxDetail.expenseAccountID), typeof(PRTaxDetail.branchID), typeof(PRTaxDetail.taxCategory), true,
			DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible, Filterable = true)]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>>))]
		public virtual int? ExpenseSubID { get; set; }
		#endregion
		#region LiabilityAccountID
		public abstract class liabilityAccountID : PX.Data.BQL.BqlInt.Field<liabilityAccountID> { }
		[TaxLiabilityAccount(typeof(PRTaxDetail.branchID),
		   typeof(PRTaxDetail.taxID),
		   typeof(PRTaxDetail.employeeID),
		   typeof(PRPayment.payGroupID),
		   typeof(PRTaxDetail.taxID), DisplayName = "Liability Account")]
		public virtual Int32? LiabilityAccountID { get; set; }
		#endregion
		#region LiabilitySubID
		public abstract class liabilitySubID : PX.Data.BQL.BqlInt.Field<liabilitySubID> { }
		[TaxLiabilitySubAccount(typeof(PRTaxDetail.liabilityAccountID), typeof(PRTaxDetail.branchID), true,
			DisplayName = "Liability Sub.", Visibility = PXUIVisibility.Visible, Filterable = true)]
		public virtual int? LiabilitySubID { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[ProjectBase(DisplayName = "Project")]
		[TaxDetailProjectDefault(typeof(PRTaxDetail.taxCategory), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>
			.And<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>>))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.TaxProject, Equal<True>>))]
		public int? ProjectID { get; set; }
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Project Task", FieldClass = ProjectAttribute.DimensionName)]
		[PXSelector(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<PRTaxDetail.projectID>>>>),
			typeof(PMTask.taskCD), typeof(PMTask.description), SubstituteKey = typeof(PMTask.taskCD))]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>
			.And<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>>))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.TaxProject, Equal<True>>))]
		[PXForeignReference(typeof(FK.ProjectTask))]
		public int? ProjectTaskID { get; set; }
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[CostCode(typeof(liabilityAccountID), typeof(projectTaskID), GL.AccountType.Expense, SkipVerificationForDefault = true, AllowNullValue = true, ReleasedField = typeof(released))]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>>))]
		public virtual Int32? CostCodeID { get; set; }
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		/// <summary>
		/// Indicates whether the line is released or not.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public virtual Boolean? Released { get; set; }
		#endregion
		#region APInvoiceDocType
		public abstract class apInvoiceDocType : PX.Data.BQL.BqlString.Field<apInvoiceDocType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Type")]
		public string APInvoiceDocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class apInvoiceRefNbr : PX.Data.BQL.BqlString.Field<apInvoiceRefNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Reference Nbr.")]
		public string APInvoiceRefNbr { get; set; }
		#endregion
		#region LiabilityPaid
		public abstract class liabilityPaid : PX.Data.BQL.BqlBool.Field<liabilityPaid> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Liability Paid")]
		public virtual Boolean? LiabilityPaid { get; set; }
		#endregion

		#region AmountErrorMessage
		public abstract class amountErrorMessage : PX.Data.BQL.BqlString.Field<amountErrorMessage> { }
		[PXString]
		public virtual string AmountErrorMessage { get; set; }
		#endregion
		#region PaymentCountryID
		[PXString(2)]
		[PXUnboundDefault(typeof(Parent<PRPayment.countryID>))]
		[PXDBScalar(typeof(SearchFor<PRPayment.countryID>
			.Where<PRPayment.docType.IsEqual<paymentDocType>
				.And<PRPayment.refNbr.IsEqual<paymentRefNbr>>>))]
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

		public int? ParentKeyID { get => TaxID; set => TaxID = value; }
	}
}
