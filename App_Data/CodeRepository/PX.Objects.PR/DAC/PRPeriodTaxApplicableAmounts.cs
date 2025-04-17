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
	/// A class for accumulating the applicable amount upon which a certain tax will be calculated and subsequently paid by an employee during a specific pay period, whether it be weekly or monthly.
	/// </summary>
	[PXCacheName(Messages.PRPeriodTaxApplicableAmounts)]
	[Serializable]
	[PeriodTaxApplicableAmountsAccumulator]
	public class PRPeriodTaxApplicableAmounts : IBqlTable, IAggregatePaycheckData
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRPeriodTaxApplicableAmounts>.By<year, employeeID, taxID, wageTypeID, isSupplemental, periodNbr>
		{
			public static PRPeriodTaxApplicableAmounts Find(PXGraph graph, string year, int? employeeID, int? taxID, int? wageTypeID, bool? isSupplemental, int? periodNbr, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, year, employeeID, taxID, wageTypeID, isSupplemental, periodNbr, options);
		}

		public static class FK
		{
			public class Employee : PREmployee.PK.ForeignKeyOf<PRPeriodTaxApplicableAmounts>.By<employeeID> { }
			public class Tax : PRTaxCode.PK.ForeignKeyOf<PRPeriodTaxApplicableAmounts>.By<taxID> { }
		}
		#endregion

		#region Year
		/// <summary>
		/// The year during which the amount should be paid.
		/// </summary>
		[PXDBString(4, IsKey = true, IsFixed = true)]
		public virtual string Year { get; set; }
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		#endregion

		#region EmployeeID
		/// <summary>
		/// The employee paying the amount.
		/// The field is included in <see cref="FK.Employee"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public virtual int? EmployeeID { get; set; }
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		#endregion

		#region TaxID
		/// <summary>
		/// The tax for which the amount is being paid.
		/// The field is included in <see cref="FK.Tax"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRTaxCode.TaxID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public virtual int? TaxID { get; set; }
		public abstract class taxID : PX.Data.BQL.BqlInt.Field<taxID> { }
		#endregion

		#region WageTypeID
		/// <summary>
		/// The unique identifier of the wage type.
		/// </summary>
		[PXDBInt(IsKey = true)]
		public virtual int? WageTypeID { get; set; }
		public abstract class wageTypeID : PX.Data.BQL.BqlInt.Field<wageTypeID> { }
		#endregion

		#region IsSupplemental
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the wage type represents an additional payment made to an employee outside of their regular wages.
		/// </summary>
		[PXDBBool(IsKey = true)]
		public virtual bool? IsSupplemental { get; set; }
		public abstract class isSupplemental : PX.Data.BQL.BqlBool.Field<isSupplemental> { }
		#endregion

		#region PeriodNbr
		/// <summary>
		/// The pay period associated with the amount.
		/// </summary>
		[PXDBInt(IsKey = true)]
		public virtual int? PeriodNbr { get; set; }
		public abstract class periodNbr : PX.Data.BQL.BqlInt.Field<periodNbr> { }
		#endregion

		#region Week
		/// <summary>
		/// The week during which the amount is being paid. Determined by the pay period.
		/// </summary>
		[PXDBInt]
		public virtual int? Week { get; set; }
		public abstract class week : PX.Data.BQL.BqlInt.Field<week> { }
		#endregion

		#region Month
		/// <summary>
		/// The month during which the amount is being paid. Determined by the pay period.
		/// </summary>
		[PXDBInt]
		public virtual int? Month { get; set; }
		public abstract class month : PX.Data.BQL.BqlInt.Field<month> { }
		#endregion

		#region AmountAllowed
		/// <summary>
		/// The total amount on which the tax may be applied and calculated.
		/// </summary>
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AmountAllowed { get; set; }
		public abstract class amountAllowed : PX.Data.BQL.BqlDecimal.Field<amountAllowed> { }
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

	public class PeriodTaxApplicableAmountsAccumulatorAttribute : PXAccumulatorAttribute
	{
		public PeriodTaxApplicableAmountsAccumulatorAttribute()
		{
			SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			var record = row as PRPeriodTaxApplicableAmounts;
			if (record == null)
			{
				return false;
			}

			columns.Update<PRPeriodTaxApplicableAmounts.amountAllowed>(record.AmountAllowed, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}
	}
}
