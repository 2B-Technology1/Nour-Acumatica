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

namespace PX.Objects.PR
{
	public class AcaCoverageType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { Employee, Spouse, Children },
				new string[]
				{
					Messages.Employee,
					Messages.Spouse,
					Messages.Children,
				})
			{
			}
		}

		public class employee : PX.Data.BQL.BqlString.Constant<employee>
		{
			public employee() : base(Employee) { }
		}

		public class spouse : PX.Data.BQL.BqlString.Constant<spouse>
		{
			public spouse() : base(Spouse) { }
		}

		public class children : PX.Data.BQL.BqlString.Constant<children>
		{
			public children() : base(Children) { }
		}

		public const string Employee = "EMP";
		public const string Spouse = "SPO";
		public const string Children = "CHI";
	}
}
