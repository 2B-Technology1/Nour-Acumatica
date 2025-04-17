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
using PX.Objects.CR;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// A class for accumulating the total amount an employee has paid for a certain tax during a specific pay period, whether it be weekly or monthly.
	/// </summary>
	[PXCacheName(Messages.PRPeriodTaxes)]
	[Serializable]
	[PeriodTaxesAccumulator]
	public class PRPeriodTaxes : IBqlTable, IAggregatePaycheckData
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRPeriodTaxes>.By<year, employeeID, taxID, periodNbr>
		{
			public static PRPeriodTaxes Find(PXGraph graph, string year, int? employeeID, int? taxID, int? periodNbr, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, year, employeeID, taxID, periodNbr, options);
		}

		public static class FK
		{
			public class YearToDateTax : PRYtdTaxes.PK.ForeignKeyOf<PRPeriodTaxes>.By<year, employeeID, taxID> { }
			public class Employee : PREmployee.PK.ForeignKeyOf<PRPeriodTaxes>.By<employeeID> { }
			public class TaxCode : PRTaxCode.PK.ForeignKeyOf<PRPeriodTaxes>.By<taxID> { }
		}
		#endregion

		#region Year
		/// <summary>
		/// The year during which the amount was paid.
		/// The field is included in <see cref="FK.YearToDateTax"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRYtdTaxes.Year"/> field.
		/// </value>
		[PXDBString(4, IsKey = true, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Year")]
		[PXParent(typeof(
			Select<PRYtdTaxes,
				Where<PRYtdTaxes.year, Equal<Current<PRPeriodTaxes.year>>,
				And<PRYtdTaxes.employeeID, Equal<Current<PRPeriodTaxes.employeeID>>,
				And<PRYtdTaxes.taxID, Equal<Current<PRPeriodTaxes.taxID>>>>>>))]
		public virtual string Year { get; set; }
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		#endregion

		#region EmployeeID
		/// <summary>
		/// The employee who paid the amount.
		/// The field is included in <see cref="FK.YearToDateTax"/> and <see cref="FK.Employee"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRYtdTaxes.EmployeeID"/> and <see cref="BAccount.BAccountID"/> fields.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Employee")]
		public virtual int? EmployeeID { get; set; }
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		#endregion

		#region TaxID
		/// <summary>
		/// The tax for which the amount was paid.
		/// The field is included in <see cref="FK.YearToDateTax"/> and <see cref="FK.TaxCode"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRYtdTaxes.TaxID"/> and <see cref="PRTaxCode.TaxID"/> fields.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Tax Code")]
		public virtual int? TaxID { get; set; }
		public abstract class taxID : PX.Data.BQL.BqlInt.Field<taxID> { }
		#endregion

		#region PeriodNbr
		/// <summary>
		/// The pay period associated with the amount.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Period")]
		public virtual int? PeriodNbr { get; set; }
		public abstract class periodNbr : PX.Data.BQL.BqlInt.Field<periodNbr> { }
		#endregion

		#region Amount
		/// <summary>
		/// The amount which was paid by the employee for the specific tax.
		/// </summary>
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		[PXFormula(null, typeof(SumCalc<PRYtdTaxes.amount>))]
		public virtual decimal? Amount { get; set; }
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		#endregion

		#region AdjustedGrossAmount
		/// <summary>
		/// The adjusted gross amount which is determined according to whether the associated payment is of debit or credit type.
		/// </summary>
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AdjustedGrossAmount { get; set; }
		public abstract class adjustedGrossAmount : PX.Data.BQL.BqlDecimal.Field<adjustedGrossAmount> { }
		#endregion

		#region ExemptionAmount
		/// <summary>
		/// The amount which is deducted from the applicable total upon which the tax is calculated.
		/// </summary>
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ExemptionAmount { get; set; }
		public abstract class exemptionAmount : PX.Data.BQL.BqlDecimal.Field<exemptionAmount> { }
		#endregion

		#region Week
		/// <summary>
		/// The week during which the amount was paid. Determined by the pay period.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Week")]
		public virtual int? Week { get; set; }
		public abstract class week : PX.Data.BQL.BqlInt.Field<week> { }
		#endregion

		#region Month
		/// <summary>
		/// The month during which the amount was paid. Determined by the pay period.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Month")]
		public virtual int? Month { get; set; }
		public abstract class month : PX.Data.BQL.BqlInt.Field<month> { }
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

	public class PeriodTaxesAccumulatorAttribute : PXAccumulatorAttribute
	{
		public PeriodTaxesAccumulatorAttribute()
		{
			SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			var record = row as PRPeriodTaxes;
			if (record == null)
			{
				return false;
			}

			columns.Update<PRPeriodTaxes.amount>(record.Amount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PRPeriodTaxes.adjustedGrossAmount>(record.AdjustedGrossAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PRPeriodTaxes.exemptionAmount>(record.ExemptionAmount, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}
	}
}
