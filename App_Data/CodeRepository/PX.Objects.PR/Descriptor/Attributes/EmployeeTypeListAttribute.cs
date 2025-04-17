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

namespace PX.Objects.PR
{
	public class EmployeeType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { SalariedExempt, SalariedNonExempt, Hourly, Other },
				new string[] { Messages.SalariedExempt, Messages.SalariedNonExempt, Messages.Hourly, Messages.Other })
			{ }
		}

		public class salariedExempt : BqlString.Constant<salariedExempt>
		{
			public salariedExempt() : base(SalariedExempt)
			{
			}
		}

		public class salariedNonExempt : BqlString.Constant<salariedNonExempt>
		{
			public salariedNonExempt() : base(SalariedNonExempt)
			{
			}
		}

		public const string SalariedExempt = "SAL";
		public const string SalariedNonExempt = "SLN";
		public const string Hourly = "HOR";
		public const string Other = "OTH";

		public static bool IsSalaried(string empType)
		{
			return empType == SalariedExempt || empType == SalariedNonExempt;
		}

		public static bool IsOvertimeEarningForSalariedExempt<ParentType>(PXCache cache, PREarningDetail row)
			 where ParentType : IEmployeeType
		{
			if (row?.IsOvertime == true)
			{
				var parent = (IEmployeeType)PXParentAttribute.SelectParent(cache, row, typeof(ParentType));
				return parent?.EmpType == EmployeeType.SalariedExempt;
			}

			return false;
		}
	}
}
