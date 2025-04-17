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
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// A record detailing a payment's earnings of a specific type.
	/// </summary>
	[PXCacheName(Messages.PREarningDetail)]
	[Serializable]
	public class PREarningDetail : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PREarningDetail>.By<recordID>
		{
			public static PREarningDetail Find(PXGraph graph, int? recordID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, recordID, options);
		}

		public static class FK
		{
			public class Employee : PREmployee.PK.ForeignKeyOf<PREarningDetail>.By<employeeID> { }
			public class PayrollBatch : PRBatch.PK.ForeignKeyOf<PREarningDetail>.By<batchNbr> { }
			public class Payment : PRPayment.PK.ForeignKeyOf<PREarningDetail>.By<paymentDocType, paymentRefNbr> { }
			public class EarningType : EPEarningType.PK.ForeignKeyOf<PREarningDetail>.By<typeCD> { }
			public class Location : PRLocation.PK.ForeignKeyOf<PREarningDetail>.By<locationID> { }
			public class Branch : GL.Branch.PK.ForeignKeyOf<PREarningDetail>.By<branchID> { }
			public class Account : GL.Account.PK.ForeignKeyOf<PREarningDetail>.By<accountID> { }
			public class Subaccount : GL.Sub.PK.ForeignKeyOf<PREarningDetail>.By<subID> { }
			public class Project : PMProject.PK.ForeignKeyOf<PREarningDetail>.By<projectID> { }
			public class ProjectTask : PMTask.PK.ForeignKeyOf<PREarningDetail>.By<projectID, projectTaskID> { }
			public class CostCode : PMCostCode.PK.ForeignKeyOf<PREarningDetail>.By<costCodeID> { }
			public class WorkCode : PMWorkCode.PK.ForeignKeyOf<PREarningDetail>.By<workCodeID> { }
			public class Union : PMUnion.PK.ForeignKeyOf<PREarningDetail>.By<unionID> { }
			public class LaborItem : InventoryItem.PK.ForeignKeyOf<PREarningDetail>.By<labourItemID> { }
			public class ShiftCode : EPShiftCode.PK.ForeignKeyOf<PREarningDetail>.By<shiftID> { }
		}
		#endregion

		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		/// <summary>
		/// The unique identifier of the earning detail.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID { get; set; }
		#endregion
		#region ExcelRecordID
		public abstract class excelRecordID : PX.Data.BQL.BqlString.Field<excelRecordID> { }
		/// <summary>
		/// The unique identifier of an earning detail being imported from an Excel document.
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Record ID")]
		public virtual string ExcelRecordID { get; set; }
		#endregion
		#region BaseOvertimeRecordID
		public abstract class baseOvertimeRecordID : PX.Data.BQL.BqlInt.Field<baseOvertimeRecordID> { }
		/// <summary>
		/// The unique identifier of the associated regular earning detail if the current one represents overtime earnings.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Base RecordID", Visible = false)]
		public virtual int? BaseOvertimeRecordID { get; set; }
		#endregion
		#region BasePTORecordID
		public abstract class basePTORecordID : PX.Data.BQL.BqlInt.Field<basePTORecordID> { }
		/// <summary>
		/// The unique identifier of the associated regular earning detail if the current one represents paid time-off earnings.
		/// </summary>
		[PXDBInt]
		public virtual int? BasePTORecordID { get; set; }
		#endregion
		#region SortingRecordID
		public abstract class sortingRecordID : PX.Data.BQL.BqlInt.Field<sortingRecordID> { }
		/// <summary>
		/// The unique identifier which is used for sorting purposes in BQL queries.
		/// </summary>
		/// <value>
		/// The first non-null field from either <see cref="BaseOvertimeRecordID"/>, <see cref="BasePTORecordID"/> or <see cref="RecordID"/> in that specific order.
		/// </value>
		[PXInt]
		[PXUIField(DisplayName = "Sorting RecordID", Visible = false)]
		[PXFormula(typeof(baseOvertimeRecordID.When<baseOvertimeRecordID.IsNotNull.And<isFringeRateEarning.IsNotEqual<True>>>
			.Else<basePTORecordID>.When<basePTORecordID.IsNotNull.And<isFringeRateEarning.IsNotEqual<True>>>
			.Else<int0.Subtract<recordID>>.When<recordID.IsLess<int0>>
			.Else<recordID>))]
		public virtual int? SortingRecordID { get; set; }
		#endregion
		#region AllowCopy
		public abstract class allowCopy : PX.Data.BQL.BqlBool.Field<allowCopy> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the earning detail may be copied.
		/// </summary>
		[PXBool]
		[PXUIField(Visible = false, Enabled = false)]
		[PXFormula(typeof(Where<baseOvertimeRecordID.IsNull
			.And<basePTORecordID.IsNull>
			.And<isFringeRateEarning.IsNotEqual<True>>
			.And<isPayingCarryover.IsNotEqual<True>>>))]
		public virtual bool? AllowCopy { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		/// <summary>
		/// The unique identifier of the employee.
		/// The field is included in <see cref="FK.Employee"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[PXDBInt]
		public int? EmployeeID { get; set; }
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		/// <summary>
		/// The unique identifier of the payroll batch.
		/// The field is included in <see cref="FK.PayrollBatch"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRBatch.BatchNbr"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number")]
		[PXDBDefault(typeof(PRBatch.batchNbr), DefaultForUpdate = true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXParent(typeof(Select<PRBatch, Where<PRBatch.batchNbr, Equal<Current<PREarningDetail.batchNbr>>>>))]
		public string BatchNbr { get; set; }
		#endregion
		#region PaymentDocType
		public abstract class paymentDocType : PX.Data.BQL.BqlString.Field<paymentDocType> { }
		/// <summary>
		/// The type of the payment.
		/// The field is included in <see cref="FK.Payment"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRPayment.DocType"/> field.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Payment Doc. Type")]
		public string PaymentDocType { get; set; }
		#endregion
		#region PaymentRefNbr
		public abstract class paymentRefNbr : PX.Data.BQL.BqlString.Field<paymentRefNbr> { }
		/// <summary>
		/// The unique identifier of the payment.
		/// The field is included in <see cref="FK.Payment"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRPayment.RefNbr"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Ref. Number")]
		[PXParent(typeof(Select<PRPayment, Where<PRPayment.docType, Equal<Current<paymentDocType>>, And<PRPayment.refNbr, Equal<Current<paymentRefNbr>>>>>))]
		[PXFormula(null, typeof(CountCalc<PRPayment.detailLinesCount>))]
		public string PaymentRefNbr { get; set; }
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		/// <summary>
		/// The date of the transaction.
		/// </summary>
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Date")]
		public DateTime? Date { get; set; }
		#endregion
		#region TypeCD
		public abstract class typeCD : PX.Data.BQL.BqlString.Field<typeCD> { }
		/// <summary>
		/// The user-friendly unique identifier of the earning type code.
		/// The field is included in <see cref="FK.EarningType"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EPEarningType.TypeCD"/> field.
		/// </value>
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXDefault]
		[PXUIField(DisplayName = "Code")]
		[PXRestrictor(typeof(Where<EPEarningType.isActive.IsEqual<True>>), Messages.EarningTypeIsNotActive, typeof(EPEarningType.typeCD))]
		[PREarningTypeSelector]
		[EarningDetailType(typeof(hours), typeof(units), typeof(workCodeID), new Type[] { typeof(isAmountBased), typeof(isPiecework) })]
		[EarningTypeProjectTaskDefault(typeof(projectID), typeof(projectTaskID))]
		[EarningTypeLaborItemOverride(typeof(employeeID), typeof(labourItemID))]
		[PXForeignReference(typeof(Field<typeCD>.IsRelatedTo<EPEarningType.typeCD>))] //ToDo: AC-142439 Ensure PXForeignReference attribute works correctly with PXCacheExtension DACs.
		[PXParent(typeof(Select<PRPaymentEarning,
							Where<PRPaymentEarning.docType,
								Equal<Current<PREarningDetail.paymentDocType>>,
							And<PRPaymentEarning.refNbr,
								Equal<Current<PREarningDetail.paymentRefNbr>>,
							And<PRPaymentEarning.typeCD,
								Equal<Current<PREarningDetail.typeCD>>,
							And<PRPaymentEarning.locationID,
								Equal<Current<PREarningDetail.locationID>>>>>>>), ParentCreate = true)]
		public string TypeCD { get; set; }
		#endregion
		#region IsOvertime
		public abstract class isOvertime : PX.Data.BQL.BqlBool.Field<isOvertime> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the earning detail represents overtime earnings.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Selector<typeCD, EPEarningType.isOvertime>))]
		[PXUIField(DisplayName = "Overtime", Visible = false)]
		public bool? IsOvertime { get; set; }
		#endregion
		#region IsPiecework
		public abstract class isPiecework : PX.Data.BQL.BqlBool.Field<isPiecework> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the earning detail is based on units other than hours or years.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Selector<typeCD, PREarningType.isPiecework>))]
		[PXUIField(DisplayName = "Piecework", Visible = false)]
		public bool? IsPiecework { get; set; }
		#endregion
		#region IsAmountBased
		public abstract class isAmountBased : PX.Data.BQL.BqlBool.Field<isAmountBased> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the earning detail is based on a fixed amount as opposed to a rate.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Selector<typeCD, PREarningType.isAmountBased>))]
		[PXUIField(DisplayName = "Amount-Based", Visible = false)]
		public bool? IsAmountBased { get; set; }
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		/// <summary>
		/// The unique identifier of the work location.
		/// The field is included in <see cref="FK.Location"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRLocation.LocationID"/> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Location")]
		[PXSelector(
			typeof(SelectFrom<PRLocation>
				.InnerJoin<PREmployee>.On<PREmployee.bAccountID.IsEqual<employeeID.FromCurrent>>
				.SearchFor<PRLocation.locationID>),
			SubstituteKey = typeof(PRLocation.locationCD))]
		[PRLocationDefault(typeof(employeeID), typeof(projectID))]
		[WorkLocationRestrictor]
		public int? LocationID { get; set; }
		#endregion
		#region Hours
		public abstract class hours : PX.Data.BQL.BqlDecimal.Field<hours> { }
		/// <summary>
		/// The number of earned hours.
		/// </summary>
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Hours")]
		[PXUIEnabled(typeof(isAmountBased.IsNotEqual<True>))]
		[PXUIRequired(typeof(isAmountBased.IsNotEqual<True>.And<unitType.IsEqual<UnitType.hour>>))]
		[PXDefault(typeof(Switch<Case<Where<isAmountBased, Equal<True>>, Null>, decimal0>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUnboundFormula(typeof(hours.When<isFringeRateEarning.IsEqual<False>.And<isPayingCarryover.IsEqual<False>>>.Else<decimal0>), typeof(SumCalc<PRBatchEmployee.hourQty>))]
		[PXUnboundFormula(typeof(hours.When<isFringeRateEarning.IsEqual<False>.And<isPayingCarryover.IsEqual<False>>>.Else<decimal0>), typeof(SumCalc<PRPayment.totalHours>))]
		[PXUnboundFormula(typeof(hours.When<isFringeRateEarning.IsEqual<False>>.Else<decimal0>), typeof(SumCalc<PRPaymentEarning.hours>))]
		public decimal? Hours { get; set; }
		#endregion
		#region Units
		public abstract class units : PX.Data.BQL.BqlDecimal.Field<units> { }
		/// <summary>
		/// The quantity of units (pieces) worked.
		/// </summary>
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Units")]
		[PXUIVisible(typeof(Where<GetSetupValue<PRSetup.enablePieceworkEarningType>, Equal<True>>))]
		[PXUIEnabled(typeof(isAmountBased.IsNotEqual<True>.And<unitType.IsEqual<UnitType.misc>>))]
		[PXUIRequired(typeof(isAmountBased.IsNotEqual<True>.And<unitType.IsEqual<UnitType.misc>>))]
		[PXDefault(typeof(Switch<Case<Where<unitType, Equal<UnitType.hour>, Or<isAmountBased, Equal<True>>>, Null>, decimal0>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? Units { get; set; }
		#endregion
		#region UnitType
		public abstract class unitType : PX.Data.BQL.BqlString.Field<unitType> { }
		/// <summary>
		/// The unit of measure for the time entry.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="UnitType.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[UnitType.List]
		[PXUIField(DisplayName = "Unit Type", Enabled = false)]
		[PXUIEnabled(typeof(isAmountBased.IsNotEqual<True>))]
		[PXFormula(typeof(Switch<
			Case<Where<isAmountBased, Equal<True>>, Null, 
			Case<Where<isPiecework, Equal<True>>, UnitType.misc>>, 
			UnitType.hour>))]
		[PXUIVisible(typeof(Where<GetSetupValue<PRSetup.enablePieceworkEarningType>, Equal<True>>))]
		public string UnitType { get; set; }
		#endregion
		#region Rate
		public abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }
		/// <summary>
		/// The rate to be multiplied by the units (or pieces, depending on the selected unit type) and any additional factors to determine the gross pay.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "Rate")]
		[PXUIEnabled(typeof(isAmountBased.IsNotEqual<True>.And<isRegularRate.IsNotEqual<True>>))]
		[PXUIRequired(typeof(isAmountBased.IsNotEqual<True>.And<isRegularRate.IsNotEqual<True>>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PayRate(typeof(Where<isFringeRateEarning.IsEqual<False>>))]
		[PayRatePrecision]
		public decimal? Rate { get; set; }
		#endregion
		#region ManualRate
		public abstract class manualRate : PX.Data.BQL.BqlBool.Field<manualRate> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the rate is specified manually.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual Rate")]
		[PXUIEnabled(typeof(isAmountBased.IsNotEqual<True>.And<isRegularRate.IsNotEqual<True>>))]
		[PXFormula(typeof(Switch<Case<Where<isAmountBased, Equal<True>>, False>, manualRate>))]
		public bool? ManualRate { get; set; }
		#endregion
		#region IsRegularRate
		public abstract class isRegularRate : PX.Data.BQL.BqlBool.Field<isRegularRate> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the rate is the default one, as opposed to overtime for example.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		public bool? IsRegularRate { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		/// <summary>
		/// The amount for the line, which is calculated as the number of units or pieces multiplied by the rate.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "Amount")]
		[PXUIEnabled(typeof(isAmountBased.IsEqual<True>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Switch<Case<Where<isAmountBased, NotEqual<True>, And<isRegularRate, NotEqual<True>>>,
			Mult<hours.When<unitType.IsEqual<UnitType.hour>>.Else<units>, rate>>, 
			amount>),
			typeof(SumCalc<PRBatchEmployee.amount>))]
		[PXFormula(null, typeof(SumCalc<PRPaymentEarning.amount>))]
		[PXFormula(null, typeof(SumCalc<PRPayment.totalEarnings>))]
		public virtual Decimal? Amount { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		/// <summary>
		/// The unique identifier of the company branch.
		/// The field is included in <see cref="FK.Branch"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Branch.BranchID"/> field.
		/// </value>
		[GL.Branch(typeof(Parent<PRPayment.branchID>), typeof(SearchFor<BranchWithAddress.branchID>), IsDetail = false)]
		public int? BranchID { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		/// <summary>
		/// The unique identifier of the associated project.
		/// The field is included in <see cref="FK.Project"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID"/> and <see cref="PMTask.ProjectID"/> fields.
		/// </value>
		[ProjectWithWarnings(DisplayName = "Project", WarnOfStatus = true)]
		public int? ProjectID { get; set; }
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		/// <summary>
		/// The unique identifier of the project task.
		/// The field is included in <see cref="FK.ProjectTask"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.TaskID"/> field.
		/// </value>
		[ProjectTask(typeof(projectID), DisplayName = "Project Task", AllowNull = false)]
		[EarningTaskStatusWarning]
		[PXForeignReference(typeof(FK.ProjectTask))]
		public int? ProjectTaskID { get; set; }
		#endregion
		#region LabourItemID
		public abstract class labourItemID : Data.BQL.BqlInt.Field<labourItemID> { }
		/// <summary>
		/// The unique identifier of the labor item, if any. The labor item may change the rate applied to the earning type.
		/// The field is included in <see cref="FK.LaborItem"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="InventoryItem.InventoryID"/> field.
		/// </value>
		[PRLaborItem(typeof(projectID), typeof(typeCD), typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<employeeID>>>>))]
		[PXForeignReference(typeof(Field<labourItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		[PXFormula(typeof(Default<typeCD, projectID, employeeID>))]
		public virtual int? LabourItemID { get; set; }
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		/// <summary>
		/// The unique identifier of the earnings account associated with the selected earning type.
		/// The field is included in <see cref="FK.Account"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID"/> field.
		/// </value>
		[EarningsAccount(typeof(PREarningDetail.branchID),
		   typeof(PREarningDetail.typeCD),
		   typeof(PREarningDetail.employeeID),
		   typeof(PRPayment.payGroupID),
		   typeof(PREarningDetail.typeCD),
		   typeof(PREarningDetail.labourItemID),
		   typeof(PREarningDetail.projectID),
		   typeof(PREarningDetail.projectTaskID))]
		public virtual Int32? AccountID { get; set; }
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		/// <summary>
		/// The unique identifier of the corresponding subaccount.
		/// The field is included in <see cref="FK.Subaccount"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID"/> field.
		/// </value>
		[EarningSubAccount(typeof(PREarningDetail.accountID), typeof(PREarningDetail.branchID), true, DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, Filterable = true)]
		public virtual int? SubID { get; set; }
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		/// <summary>
		/// The unique identifier of the applied cost code, if any.
		/// The field is included in <see cref="FK.CostCode"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMCostCode.CostCodeID"/> field.
		/// </value>
		[CostCode(typeof(accountID), typeof(projectTaskID), GL.AccountType.Expense, SkipVerificationForDefault = true, ReleasedField = typeof(released))]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public virtual Int32? CostCodeID { get; set; }
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the line is released or not.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public virtual Boolean? Released { get; set; }
		#endregion
		#region SourceType
		public abstract class sourceType : PX.Data.BQL.BqlString.Field<sourceType> { }
		/// <summary>
		/// The type which indicates the origin of the earning detail line.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="EarningDetailSourceType.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Data Source Type")]
		[EarningDetailSourceType.List]
		public string SourceType { get; set; }
		#endregion
		#region SourceNoteID
		public abstract class sourceNoteID : PX.Data.BQL.BqlGuid.Field<sourceNoteID> { }
		/// <summary>
		/// The unique global identifier of the associated time activity, if any.
		/// </summary>
		[PXDBGuid]
		[PXSelector(typeof(PMTimeActivity.noteID), SubstituteKey = typeof(PMTimeActivity.summary))]
		[PXUIField(DisplayName = "Time Activity", Enabled = false)]
		public Guid? SourceNoteID { get; set; }
		#endregion
		#region TimeCardMinutes
		public abstract class timeCardMinutes : PX.Data.BQL.BqlDecimal.Field<timeCardMinutes> { }
		/// <summary>
		/// The amount of minutes logged in the associated time activity, if any.
		/// </summary>
		[PXInt]
		[PXDBScalar(typeof(SearchFor<PMTimeActivity.timeSpent>.Where<sourceType.IsEqual<EarningDetailSourceType.timeActivity>.And<PMTimeActivity.noteID.IsEqual<sourceNoteID>>>))]
		[PXUnboundDefault(typeof(SearchFor<PMTimeActivity.timeSpent>.Where<sourceType.FromCurrent.IsEqual<EarningDetailSourceType.timeActivity>.And<PMTimeActivity.noteID.IsEqual<sourceNoteID.FromCurrent>>>))]
		[PXFormula(typeof(Default<sourceNoteID>))]
		public int? TimeCardMinutes { get; set; }
		#endregion
		#region SourceCommnPeriod
		public abstract class sourceCommnPeriod : PX.Data.BQL.BqlString.Field<sourceCommnPeriod> { }
		[PXDBString(6, IsUnicode = true)]
		[PXUIField(DisplayName = "Source Commn Period")]
		public string SourceCommnPeriod { get; set; }
		#endregion
		#region UnionID
		public abstract class unionID : Data.BQL.BqlString.Field<unionID> { }
		/// <summary>
		/// The unique identifier of the union local, if any. The union local may change the rate applied to the earning type.
		/// The field is included in <see cref="FK.Union"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMUnion.UnionID"/> field.
		/// </value>
		[PXForeignReference(typeof(Field<unionID>.IsRelatedTo<PMUnion.unionID>))]
		[PRUnion]
		[PXDefault(typeof(
			SelectFrom<PREmployee>.
			Where<PREmployee.bAccountID.IsEqual<employeeID.FromCurrent>>.
			SearchFor<PREmployee.unionID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<employeeID>))]
		public virtual string UnionID { get; set; }
		#endregion
		#region CertifiedJob
		public abstract class certifiedJob : Data.BQL.BqlBool.Field<certifiedJob> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the specified project is certified.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Coalesce<Search<PMProject.certifiedJob, Where<PMProject.contractID, Equal<Current<projectID>>>>,
			Search<PMProject.certifiedJob, Where<PMProject.nonProject, Equal<True>>>>))]
		[PXFormula(typeof(Default<projectID>))]
		[PXUIField(DisplayName = "Certified Job")]
		public virtual bool? CertifiedJob { get; set; }
		#endregion
		#region WorkCodeID
		public abstract class workCodeID : Data.BQL.BqlString.Field<workCodeID> { }
		/// <summary>
		/// The unique identifier of the WCC code applied, if any.
		/// The field is included in <see cref="FK.WorkCode"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMWorkCode.WorkCodeID"/> field.
		/// </value>
		[PXForeignReference(typeof(FK.WorkCode))]
		[PMWorkCode(typeof(costCodeID), typeof(projectID), typeof(projectTaskID), typeof(labourItemID), typeof(employeeID), FieldClass = null)]
		public virtual string WorkCodeID { get; set; }
		#endregion
		#region IsFringeRateEarning
		public abstract class isFringeRateEarning : Data.BQL.BqlBool.Field<isFringeRateEarning> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the earning detail represents fringe amounts.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual bool? IsFringeRateEarning { get; set; }
		#endregion
		#region IsPayingCarryover
		public abstract class isPayingCarryover : Data.BQL.BqlBool.Field<isPayingCarryover> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the earning detail may contribute to the PTO bank's carryover amount, if any.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual bool? IsPayingCarryover { get; set; }
		#endregion
		#region EmployeeAcctCD
		/// <summary>
		/// The user-friendly unique identifier of the employee.
		/// </summary>
		[PXString]
		[PXDBScalar(typeof(SearchFor<PREmployee.acctCD>.Where<PREmployee.bAccountID.IsEqual<employeeID>>))]
		[PXUnboundDefault(typeof(SearchFor<PREmployee.acctCD>.Where<PREmployee.bAccountID.IsEqual<employeeID.FromCurrent>>))]
		public virtual string EmployeeAcctCD { get; set; }
		public abstract class employeeAcctCD : PX.Data.BQL.BqlString.Field<employeeAcctCD> { }
		#endregion
		#region IsTimeActivityBillable
		public abstract class isTimeActivityBillable : Data.BQL.BqlBool.Field<isTimeActivityBillable> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the associated time activity, if any, represents billable hours.
		/// </summary>
		[PXBool]
		[PXDBScalar(typeof(SearchFor<PMTimeActivity.isBillable>.Where<PMTimeActivity.noteID.IsEqual<sourceNoteID>>))]
		[PXUnboundDefault(typeof(SearchFor<PMTimeActivity.isBillable>.Where<PMTimeActivity.noteID.IsEqual<sourceNoteID.FromCurrent>>))]
		public virtual bool? IsTimeActivityBillable { get; set; }
		#endregion
		#region ShiftID
		public abstract class shiftID : PX.Data.BQL.BqlInt.Field<shiftID> { }
		/// <summary>
		/// The unique identifier of the work shift during which the activity was performed.
		/// The field is included in <see cref="FK.ShiftCode"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EPShiftCode.ShiftID"/> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Shift Code", FieldClass = nameof(FeaturesSet.ShiftDifferential))]
		[PXUIEnabled(typeof(isAmountBased.IsNotEqual<True>.And<isRegularRate.IsNotEqual<True>.And<isFringeRateEarning.IsNotEqual<True>>>))]
		[ShowValueWhen(typeof(isAmountBased.IsNotEqual<True>.And<isRegularRate.IsNotEqual<True>.And<isFringeRateEarning.IsNotEqual<True>>>), true)]
		[DetailShiftCodeSelector(typeof(employeeID), typeof(date))]
		[EPShiftCodeActiveRestrictor]
		public virtual int? ShiftID { get; set; }
		#endregion
		#region IsPayingSettlement
		public abstract class isPayingSettlement : PX.Data.BQL.BqlBool.Field<isPayingSettlement> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the earning detail line is paying off PTO for settlement purposes.
		/// </summary>
		[PXDBBool]
		[PXUIField(Visible = false)]
		[PXDefault(false)]
		public bool? IsPayingSettlement { get; set; }
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
		#region PTOHelper
		#region PTODisbursementWithFinancialTransaction
		[PXBool]
		[PXUnboundDefault(false)]
		public bool? PTODisbursementWithFinancialTransaction { get; set; }
		public abstract class ptoDisbursementWithFinancialTransaction : Data.BQL.BqlBool.Field<ptoDisbursementWithFinancialTransaction> { }
		#endregion
		#region PTODisbursementWithAverageRate
		[PXBool]
		[PXUnboundDefault(false)]
		public bool? PTODisbursementWithAverageRate { get; set; }
		public abstract class ptoDisbursementWithAverageRate : Data.BQL.BqlBool.Field<ptoDisbursementWithAverageRate> { }
		#endregion
		#endregion
	}
}
