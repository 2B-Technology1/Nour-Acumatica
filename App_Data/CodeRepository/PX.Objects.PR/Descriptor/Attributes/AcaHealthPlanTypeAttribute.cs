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
	public class AcaHealthPlanType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { MeetsEssentialCoverageAndValue, MeetsEssentialCoverage, SelfInsured, NoneOfTheAbove },
				new string[]
				{
					Messages.MeetsEssentialCoverageAndValue,
					Messages.MeetsEssentialCoverage,
					Messages.SelfInsured,
					Messages.NoneOfTheAbove,
				})
			{
			}
		}

		public class meetsEssentialCoverageAndValue : PX.Data.BQL.BqlString.Constant<meetsEssentialCoverageAndValue>
		{
			public meetsEssentialCoverageAndValue() : base(MeetsEssentialCoverageAndValue) { }
		}

		public class meetsEssentialCoverage : PX.Data.BQL.BqlString.Constant<meetsEssentialCoverage>
		{
			public meetsEssentialCoverage() : base(MeetsEssentialCoverage) { }
		}

		public class selfInsured : PX.Data.BQL.BqlString.Constant<selfInsured>
		{
			public selfInsured() : base(SelfInsured) { }
		}

		public class noneOfTheAbove : PX.Data.BQL.BqlString.Constant<noneOfTheAbove>
		{
			public noneOfTheAbove() : base(NoneOfTheAbove) { }
		}

		public const string MeetsEssentialCoverageAndValue = "MCV";
		public const string MeetsEssentialCoverage = "MEC";
		public const string SelfInsured = "SEL";
		public const string NoneOfTheAbove = "NON";
	}
}
