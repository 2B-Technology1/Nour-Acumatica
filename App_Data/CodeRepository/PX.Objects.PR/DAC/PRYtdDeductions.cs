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
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Year-to-date, quarter-to-date and month-to-date amounts for Deduction and Benefit Codes.
	/// </summary>
	[PXCacheName(Messages.PRYtdDeductions)]
	[Serializable]
	[PXAccumulator(new Type[] { typeof(PRYtdDeductions.amount), typeof(PRYtdDeductions.employerAmount) }, new Type[] { typeof(PRYtdDeductions.amount), typeof(PRYtdDeductions.employerAmount) }, SingleRecord = true)]
	public class PRYtdDeductions : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRYtdDeductions>.By<year, employeeID, codeID>
		{
			public static PRYtdDeductions Find(PXGraph graph, string year, int? employeeID, int? codeID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, year, employeeID, codeID, options);
		}

		public static class FK
		{
			public class Employee : PREmployee.PK.ForeignKeyOf<PRYtdDeductions>.By<employeeID> { }
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRYtdDeductions>.By<codeID> { }
		}
		#endregion

		#region Year
		[PXDBString(4, IsKey = true, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Year")]
		public virtual string Year { get; set; }
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		#endregion

		#region EmployeeID
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Employee")]
		public virtual int? EmployeeID { get; set; }
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		#endregion

		#region CodeID
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Code")]
		public virtual int? CodeID { get; set; }
		public abstract class codeID : PX.Data.BQL.BqlInt.Field<codeID> { }
		#endregion

		#region Amount
		[PRCurrency]
		[PXUIField(DisplayName = "Amount")]
		public virtual Decimal? Amount { get; set; }
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		#endregion

		#region EmployerAmount
		[PRCurrency]
		[PXUIField(DisplayName = "Employer Amount")]
		public virtual Decimal? EmployerAmount { get; set; }
		public abstract class employerAmount : PX.Data.BQL.BqlDecimal.Field<employerAmount> { }
		#endregion

		#region System Columns
		#region CreatedByID
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion System Columns
	}
}
