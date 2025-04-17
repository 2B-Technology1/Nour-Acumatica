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

namespace PX.Objects.SO
{
	public class SOIntercompanyAcctDefault
	{
		public class AcctSalesListAttribute : SOCustomListAttribute
		{
			private static Tuple<string, string>[] Pairs =>
				new[]
				{
					Pair(MaskItem, IN.Messages.MaskItem),
					Pair(MaskLocation, MaskLocationLabel),
				};

			public AcctSalesListAttribute() : base(Pairs) { }
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public class AcctCOGSListAttribute : PXStringListAttribute
		{
			public AcctCOGSListAttribute() : base(
				new[]
				{
					Pair(MaskItem, IN.Messages.MaskItem),
					Pair(MaskLocation, AR.Messages.MaskCustomer),
				})
			{
			}
		}

		public const string MaskItem = "I";
		public const string MaskLocation = "L";

		public class maskItem : PX.Data.BQL.BqlString.Constant<maskItem>
		{
			public maskItem() : base(MaskItem) { }
		}

		public class maskLocation : PX.Data.BQL.BqlString.Constant<maskLocation>
		{
			public maskLocation() : base(MaskLocation) { }
		}
	}
}
