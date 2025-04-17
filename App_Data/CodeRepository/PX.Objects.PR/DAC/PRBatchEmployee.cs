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
using PX.Objects.GL;
using System;
using System.Linq;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores information related to the employees that are part of a payroll batch. Contains detailed information for each employee that is part of the batch.
	/// This information will be displayed on the Payroll Batches (PR301000) form.
	/// </summary>
	[PXCacheName(Messages.PRBatchEmployee)]
	[Serializable]
	[PXProjection(typeof(SelectFrom<PRBatchEmployee>
							.LeftJoin<PREmployee>.On<PREmployee.bAccountID.IsEqual<PRBatchEmployee.employeeID>>
							.LeftJoin<BranchWithAddress>.On<BranchWithAddress.bAccountID.IsEqual<PREmployee.parentBAccountID>>), new Type[] { typeof(PRBatchEmployee) }, Persistent = true)]
	public class PRBatchEmployee : IBqlTable, IEmployeeType
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRBatchEmployee>.By<batchNbr, employeeID>
		{
			public static PRBatchEmployee Find(PXGraph graph, string batchNbr, int? employeeID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, batchNbr, employeeID, options);
		}

		public static class FK
		{
			public class PayrollBatch : PRBatch.PK.ForeignKeyOf<PRBatchEmployee>.By<batchNbr> { }
			public class Employee : PREmployee.PK.ForeignKeyOf<PRBatchEmployee>.By<employeeID> { }
		}
		#endregion

		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number")]
		[PXDBDefault(typeof(PRBatch.batchNbr))]
		[PXParent(typeof(FK.PayrollBatch))]
		public string BatchNbr { get; set; }
		#endregion
		#region BatchStatus
		public abstract class batchStatus : PX.Data.BQL.BqlString.Field<batchStatus> { }
		[PXString(3, IsFixed = true)]
		[PXUIField(Visible = false)]
		[PXFormula(typeof(Parent<PRBatch.status>))]
		[BatchStatus.List]
		public virtual string BatchStatus { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[PXDBInt(IsKey = true)]
		[PXFormula(null, typeof(CountCalc<PRBatch.numberOfEmployees>))]
		[PXForeignReference(typeof(Field<PRBatchEmployee.employeeID>.IsRelatedTo<PREmployee.bAccountID>))]
		[PXUIEnabled(typeof(Where<employeeID.IsNull>))]
		public int? EmployeeID { get; set; }
		#endregion
		#region EmpType
		public abstract class empType : PX.Data.BQL.BqlString.Field<empType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Employee Type")]
		[PXDefault(typeof(SearchFor<PREmployee.empType>.Where<PREmployee.bAccountID.IsEqual<PRBatchEmployee.employeeID.FromCurrent>>))]
		[EmployeeType.List]
		public virtual string EmpType { get; set; }
		#endregion
		#region HourQty
		public abstract class hourQty : PX.Data.BQL.BqlDecimal.Field<hourQty> { }
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Hours", Enabled = false)]
		[PXFormula(null, typeof(SumCalc<PRBatch.totalHourQty>))]
		public virtual Decimal? HourQty { get; set; }
		#endregion
		#region Rate
		public abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Rate", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(amount.Divide<hourQty>.When<hourQty.IsGreater<decimal0>>.Else<decimal0>))]
		[PayRatePrecision]
		public Decimal? Rate { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<PRBatch.totalEarnings>))]
		public virtual Decimal? Amount { get; set; }
		#endregion
		#region RegularAmount
		public abstract class regularAmount : PX.Data.BQL.BqlDecimal.Field<regularAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Regular Amount to Be Paid")]
		[PXUIEnabled(typeof(Where<batchStatus.IsEqual<BatchStatus.hold>.Or<batchStatus.IsEqual<BatchStatus.balanced>>>))]
		[BatchEmployeeRegularAmount]
		public virtual Decimal? RegularAmount { get; set; }
		#endregion
		#region ManualRegularAmount
		public abstract class manualRegularAmount : PX.Data.BQL.BqlBool.Field<manualRegularAmount> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Manual Amount")]
		[PXUIEnabled(typeof(Where<batchStatus.IsEqual<BatchStatus.hold>.Or<batchStatus.IsEqual<BatchStatus.balanced>>>))]
		[PXDefault(false)]
		public virtual bool? ManualRegularAmount { get; set; }
		#endregion

		#region PaymentRefNbr
		public abstract class paymentRefNbr : BqlString.Field<paymentRefNbr> { }
		[PXString]
		public string PaymentRefNbr { get; set; }
		#endregion

		#region PaymentDocAndRef
		public abstract class paymentDocAndRef : BqlString.Field<paymentDocAndRef> { }
		[PXString]
		[PXUIField(DisplayName = "Paycheck Ref", Enabled = false)]
		public string PaymentDocAndRef { get; set; }
		#endregion

		#region VoidPaymentDocAndRef
		public abstract class voidPaymentDocAndRef : BqlString.Field<voidPaymentDocAndRef> { }
		[PXString]
		[PXUIField(DisplayName = "Void Paycheck Ref", Visible = false)]
		public string VoidPaymentDocAndRef { get; set; }
		#endregion

		#region HasNegativeHoursEarnings
		[PXBool]
		[PXUnboundDefault(false)]
		public bool? HasNegativeHoursEarnings { get; set; }
		public abstract class hasNegativeHoursEarnings : BqlBool.Field<hasNegativeHoursEarnings> { }
		#endregion

		#region AcctCD
		[PXDBString(BqlField = typeof(PREmployee.acctCD))]
		[PXUIField(DisplayName = "Employee", Enabled = false)]
		[PXDefault(typeof(SearchFor<PREmployee.acctCD>.Where<PREmployee.bAccountID.IsEqual<employeeID.FromCurrent>>))]
		public string AcctCD { get; set; }
		public abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		#endregion

		#region AcctName
		[PXDBString(BqlField = typeof(PREmployee.acctName))]
		[PXUIField(DisplayName = "Employee Name", Enabled = false)]
		[PXDefault(typeof(SearchFor<PREmployee.acctName>.Where<PREmployee.bAccountID.IsEqual<employeeID.FromCurrent>>))]
		public string AcctName { get; set; }
		public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		#endregion

		#region ParentBAccountID
		[PXDBInt(BqlField = typeof(PREmployee.parentBAccountID))]
		[PXDefault(typeof(SearchFor<PREmployee.parentBAccountID>.Where<PREmployee.bAccountID.IsEqual<employeeID.FromCurrent>>))]
		public int? ParentBAccountID { get; set; }
		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		#endregion

		#region BranchID
		[PXDBInt(BqlField = typeof(Branch.branchID))]
		[PXDefault(typeof(SearchFor<Branch.branchID>.Where<GL.Branch.bAccountID.IsEqual<parentBAccountID.FromCurrent>>))]
		public int? BranchID { get; set; }
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion

		#region PayGroupID
		[PXDBString(BqlField = typeof(PREmployee.payGroupID))]
		[PXDefault(typeof(SearchFor<PREmployee.payGroupID>.Where<PREmployee.bAccountID.IsEqual<employeeID.FromCurrent>>))]
		public string PayGroupID { get; set; }
		public abstract class payGroupID : PX.Data.BQL.BqlString.Field<payGroupID> { }
		#endregion

		#region EmployeeCountryID
		[PXDBString(BqlField = typeof(BranchWithAddress.addressCountryID))]
		[PXDefault(typeof(SearchFor<PREmployee.countryID>.Where<PREmployee.bAccountID.IsEqual<employeeID.FromCurrent>>))]
		public string EmployeeCountryID { get; set; }
		public abstract class employeeCountryID : PX.Data.BQL.BqlString.Field<employeeCountryID> { }
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
}
