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
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Payroll.Data;
using System;
using System.Diagnostics;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the paid time off information related to the configuration of a specific employee. The information will be displayed on the Employee Payroll Settings (PR203000) form.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.PREmployeePTOBank)]
	[DebuggerDisplay("{GetType().Name,nq}: BAccountID = {BAccountID,nq}, BankID = {BankID,nq}, StartDate = {StartDate,nq}")]
	public class PREmployeePTOBank : IBqlTable, IPTOBank
	{
		#region Keys
		public class PK : PrimaryKeyOf<PREmployeePTOBank>.By<bAccountID, bankID>
		{
			public static PREmployeePTOBank Find(PXGraph graph, int? bAccountID, string bankID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, bAccountID, bankID, options);
		}
		
		public static class FK
		{
			public class Employee : PREmployee.PK.ForeignKeyOf<PREmployeePTOBank>.By<bAccountID> { }
			public class PTOBank : PRPTOBank.PK.ForeignKeyOf<PREmployeePTOBank>.By<bankID> { }
			public class EmployeeClass : PREmployeeClass.PK.ForeignKeyOf<PREmployeePTOBank>.By<employeeClassID> { }
		}

		[Obsolete("This foreign key is obsolete and is going to be removed in 2021R1. Use FK.PTOBank instead.")]
		public class PTOBankFK : FK.PTOBank { }
		#endregion

		#region BAccountID
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PREmployee.bAccountID))]
		[PXParent(typeof(Select<PREmployee, Where<PREmployee.bAccountID, Equal<Current<PREmployeePTOBank.bAccountID>>>>))]
		public int? BAccountID { get; set; }
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion

		#region BankID
		[PXDBString(3, IsKey = true, IsUnicode = true, InputMask = ">CCC")]
		[PXUIField(DisplayName = "PTO Bank")]
		[PXSelector(typeof(SearchFor<PRPTOBank.bankID>), DescriptionField = typeof(PRPTOBank.description))]
		[PXRestrictor(typeof(Where<PRPTOBank.isActive.IsEqual<True>>), Messages.InactivePTOBank, typeof(PRPTOBank.bankID))]
		[PXForeignReference(typeof(FK.PTOBank))]
		public virtual string BankID { get; set; }
		public abstract class bankID : PX.Data.BQL.BqlString.Field<bankID> { }
		#endregion

		#region EmployeeClassID
		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Class ID")]
		[PXUnboundDefault(typeof(PREmployee.employeeClassID))]
		public string EmployeeClassID { get; set; }
		public abstract class employeeClassID : PX.Data.BQL.BqlString.Field<employeeClassID> { }
		#endregion

		#region IsActive
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public virtual bool? IsActive { get; set; }
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		#endregion

		#region UseClassDefault
		[PXDBBool]
		[PXUIField(DisplayName = "Use Class Default Values")]
		[PXDefault(false)]
		public virtual bool? UseClassDefault { get; set; }
		public abstract class useClassDefault : PX.Data.BQL.BqlBool.Field<useClassDefault> { }
		#endregion

		#region AccrualMethod
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Accrual Method")]
		[PTOAccrualMethod.List]
		[PXDefault(typeof(PTOAccrualMethod.percentage))]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>
			.And<isActive.IsEqual<True>>
			.And<createFinancialTransaction.IsEqual<False>>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.accrualMethod),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) })]
		public virtual string AccrualMethod { get; set; }
		public abstract class accrualMethod : PX.Data.BQL.BqlString.Field<accrualMethod> { }
		#endregion

		#region AccrualRate
		[PXDBDecimal(6, MinValue = 0)]
		[PXUIField(DisplayName = "Accrual %")]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>
			.And<isActive.IsEqual<True>
			.And<accrualMethod.IsEqual<PTOAccrualMethod.percentage>>>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.accrualRate),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) })]
		[ShowValueWhen(typeof(Where<accrualMethod.IsEqual<PTOAccrualMethod.percentage>>))]
		[PXDefault(TypeCode.Decimal, "0")]
		public virtual Decimal? AccrualRate { get; set; }
		public abstract class accrualRate : PX.Data.BQL.BqlDecimal.Field<accrualRate> { }
		#endregion

		#region HoursPerYear
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Hours per Year")]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>
			.And<isActive.IsEqual<True>
			.And<accrualMethod.IsEqual<PTOAccrualMethod.totalHoursPerYear>>>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.hoursPerYear),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) })]
		[ShowValueWhen(typeof(Where<accrualMethod.IsEqual<PTOAccrualMethod.totalHoursPerYear>>))]
		[PXDefault(TypeCode.Decimal, "0")]
		public virtual Decimal? HoursPerYear { get; set; }
		public abstract class hoursPerYear : PX.Data.BQL.BqlDecimal.Field<hoursPerYear> { }
		#endregion

		#region AccrualLimit
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Balance Limit")]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>.And<isActive.IsEqual<True>>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.accrualLimit),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) })]
		public virtual Decimal? AccrualLimit
		{
			get => _AccrualLimit != 0 ? _AccrualLimit : null;
			set => _AccrualLimit = value;
		}
		private decimal? _AccrualLimit;
		public abstract class accrualLimit : PX.Data.BQL.BqlDecimal.Field<accrualLimit> { }
		#endregion

		#region CarryoverType
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Carryover Type")]
		[CarryoverType.List]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>
			.And<isActive.IsEqual<True>>
			.And<createFinancialTransaction.IsEqual<False>>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.carryoverType),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) })]
		public virtual string CarryoverType { get; set; }
		public abstract class carryoverType : PX.Data.BQL.BqlString.Field<carryoverType> { }
		#endregion

		#region CarryoverAmount
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Carryover Amount")]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>
			.And<carryoverType.IsEqual<CarryoverType.partial>>
			.And<isActive.IsEqual<True>>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.carryoverAmount),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) },
			typeof(Where<carryoverType.IsEqual<CarryoverType.partial>>))]
		public virtual Decimal? CarryoverAmount { get; set; }
		public abstract class carryoverAmount : PX.Data.BQL.BqlDecimal.Field<carryoverAmount> { }
		#endregion

		#region FrontLoadingAmount
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Front Loading Amount")]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>
			.And<isActive.IsEqual<True>>
			.And<createFinancialTransaction.IsEqual<False>>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.frontLoadingAmount),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) },
			typeof(Where<createFinancialTransaction.IsEqual<False>>))]
		public virtual Decimal? FrontLoadingAmount { get; set; }
		public abstract class frontLoadingAmount : PX.Data.BQL.BqlDecimal.Field<frontLoadingAmount> { }
		#endregion

		#region StartDate
		[PXDBDate(IsKey = true)]
		[PXUIField(DisplayName = "Effective Date")]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>.And<isActive.IsEqual<True>>>))]
		[PXDefault]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion

		#region PTOYearStartDate
		[PXDate]
		[PXDBScalar(typeof(SearchFor<PRPTOBank.startDate>.Where<PRPTOBank.bankID.IsEqual<bankID>>))]
		[PXUnboundDefault(typeof(SearchFor<PRPTOBank.startDate>.Where<PRPTOBank.bankID.IsEqual<bankID.FromCurrent>>))]
		public virtual DateTime? PTOYearStartDate { get; set; }
		public abstract class pTOYearStartDate : PX.Data.BQL.BqlDateTime.Field<pTOYearStartDate> { }
		#endregion

		#region AccumulatedAmount
		[PXDecimal]
		[PXUIField(DisplayName = "Total Accrued Hours", Enabled = false)]
		public virtual Decimal? AccumulatedAmount { get; set; }
		public abstract class accumulatedAmount : PX.Data.BQL.BqlDecimal.Field<accumulatedAmount> { }
		#endregion

		#region AccumulatedMoney
		[PXDecimal]
		[PXUIField(DisplayName = "Amount Accrued", Enabled = false, FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[HideValueIfDisabled(false, typeof(Where<createFinancialTransaction.IsEqual<True>>))]
		public virtual Decimal? AccumulatedMoney { get; set; }
		public abstract class accumulatedMoney : PX.Data.BQL.BqlDecimal.Field<accumulatedMoney> { }
		#endregion

		#region UsedAmount
		[PXDecimal]
		[PXUIField(DisplayName = "Total Used Hours", Enabled = false)]
		public virtual Decimal? UsedAmount { get; set; }
		public abstract class usedAmount : PX.Data.BQL.BqlDecimal.Field<usedAmount> { }
		#endregion

		#region UsedMoney
		[PXDecimal]
		[PXUIField(DisplayName = "Amount Used", Enabled = false, FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[HideValueIfDisabled(false, typeof(Where<createFinancialTransaction.IsEqual<True>>))]
		public virtual Decimal? UsedMoney { get; set; }
		public abstract class usedMoney : PX.Data.BQL.BqlDecimal.Field<usedMoney> { }
		#endregion

		#region AvailableAmount
		[PXDecimal]
		[PXUIField(DisplayName = "Total Available Hours", Enabled = false)]
		public virtual Decimal? AvailableAmount { get; set; }
		public abstract class availableAmount : PX.Data.BQL.BqlDecimal.Field<availableAmount> { }
		#endregion

		#region AvailableMoney
		[PXDecimal]
		[PXUIField(DisplayName = "Amount Available", Enabled = false, FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[HideValueIfDisabled(false, typeof(Where<createFinancialTransaction.IsEqual<True>>))]
		public virtual Decimal? AvailableMoney { get; set; }
		public abstract class availableMoney : PX.Data.BQL.BqlDecimal.Field<availableMoney> { }
		#endregion

		#region AllowDelete
		[PXBool]
		[PXUIField(Visible = false)]
		public virtual bool? AllowDelete { get; set; }
		public abstract class allowDelete : PX.Data.BQL.BqlBool.Field<allowDelete> { }
		#endregion

		#region AllowNegativeBalance
		[PXBool]
		[PXUIField(DisplayName = "Allow Negative Balance")]
		[PXDBScalar(typeof(SearchFor<PRPTOBank.allowNegativeBalance>.Where<PRPTOBank.bankID.IsEqual<PREmployeePTOBank.bankID>>))]
		public virtual bool? AllowNegativeBalance { get; set; }
		public abstract class allowNegativeBalance : PX.Data.BQL.BqlBool.Field<allowNegativeBalance> { }
		#endregion

		#region CarryoverPayMonthLimit
		[PXInt]
		[PXUIField(DisplayName = "Pay Carryover after (Months)")]
		[PXDBScalar(typeof(SearchFor<PRPTOBank.carryoverPayMonthLimit>.Where<PRPTOBank.bankID.IsEqual<PREmployeePTOBank.bankID>>))]
		public virtual int? CarryoverPayMonthLimit { get; set; }
		public abstract class carryoverPayMonthLimit : PX.Data.BQL.BqlInt.Field<carryoverPayMonthLimit> { }
		#endregion

		#region DisburseFromCarryover
		[PXBool]
		[PXUIField(DisplayName = "Can Only Disburse from Carryover")]
		[PXDBScalar(typeof(SearchFor<PRPTOBank.disburseFromCarryover>.Where<PRPTOBank.bankID.IsEqual<PREmployeePTOBank.bankID>>))]
		public virtual bool? DisburseFromCarryover { get; set; }
		public abstract class disburseFromCarryover : PX.Data.BQL.BqlBool.Field<disburseFromCarryover> { }
		#endregion

		#region DisbursingType
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Disbursing Type")]
		[PXUIVisible(typeof(Where<Parent<PREmployee.countryID>, Equal<BQLLocationConstants.CountryCAN>>))]
		[PXUIRequired(typeof(Where<Parent<PREmployee.countryID>, Equal<BQLLocationConstants.CountryCAN>, And<createFinancialTransaction, Equal<True>>>))]
		[PTODisbursingType.List]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>
			.And<isActive.IsEqual<True>>
			.And<createFinancialTransaction.IsEqual<True>>>))]
		[DefaultSource(typeof(useClassDefault),
			typeof(PREmployeeClassPTOBank.disbursingType),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(bankID), typeof(employeeClassID) },
			typeof(Where<createFinancialTransaction.IsEqual<True>>))]
		public virtual string DisbursingType { get; set; }
		public abstract class disbursingType : PX.Data.BQL.BqlString.Field<disbursingType> { }
		#endregion

		#region CreateFinancialTransaction
		[PXBool]
		[PXUnboundDefault(typeof(Selector<bankID, PRPTOBank.createFinancialTransaction>))]
		public virtual bool? CreateFinancialTransaction { get; set; }
		public abstract class createFinancialTransaction : PX.Data.BQL.BqlBool.Field<createFinancialTransaction> { }
		#endregion

		#region AllowViewAvailablePTOPaidHours
		/// <summary>
		/// Allow (if set to <see langword="true" />) to view the available PTO paid hours.
		/// </summary>
		[PXBool]
		[PXDependsOnFields(typeof(createFinancialTransaction), typeof(availableMoney))]
		public virtual bool? AllowViewAvailablePTOPaidHours => CreateFinancialTransaction == true && AvailableMoney >= 0;
		public abstract class allowViewAvailablePTOPaidHours : PX.Data.BQL.BqlBool.Field<allowViewAvailablePTOPaidHours> { }
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
