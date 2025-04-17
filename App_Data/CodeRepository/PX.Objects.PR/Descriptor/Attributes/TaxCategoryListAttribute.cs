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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class TaxCategory
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { EmployerTax, EmployeeWithholding },
				new string[] { Messages.EmployerTax, Messages.EmployeeWithholding })
			{ }
		}

		public class ListWithUnknownAttribute : PXStringListAttribute
		{
			public ListWithUnknownAttribute()
				: base(
				new string[] { EmployerTax, EmployeeWithholding, Unknown },
				new string[] { Messages.EmployerTax, Messages.EmployeeWithholding, Messages.UnknownTaxCategory })
			{ }
		}

		public class employerTax : PX.Data.BQL.BqlString.Constant<employerTax>
		{
			public employerTax() : base(EmployerTax) { }
		}

		public class employeeWithholding : PX.Data.BQL.BqlString.Constant<employeeWithholding>
		{
			public employeeWithholding() : base(EmployeeWithholding) { }
		}

		public const string EmployerTax = "CNT";
		public const string EmployeeWithholding = "EWH";
		public const string Unknown = "ZZZ";

		public static string GetTaxCategory(Payroll.TaxCategory taxCategory)
		{
			switch (taxCategory)
			{
				case Payroll.TaxCategory.Employee:
					return TaxCategory.EmployeeWithholding;
				case Payroll.TaxCategory.Employer:
					return TaxCategory.EmployerTax;
				default:
					return TaxCategory.Unknown;
			}
		}

		public static Payroll.TaxCategory GetTaxCategory(string taxCategory)
		{
			switch (taxCategory)
			{
				case TaxCategory.EmployeeWithholding:
					return Payroll.TaxCategory.Employee;
				case TaxCategory.EmployerTax:
					return Payroll.TaxCategory.Employer;
				default:
					return Payroll.TaxCategory.Any;
			}
		}
	}
}
