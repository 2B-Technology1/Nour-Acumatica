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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.CA
{
	public class CABankFeedType
	{
		public const string Plaid = "P";
		public const string MX = "M";
		public const string TestPlaid = "T";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(GetTypes)
			{

			}

			public static (string, string)[] GetTypes => new[] {
				(Plaid, Messages.Plaid), (MX, Messages.MX), (TestPlaid, Messages.TestPlaid)
			};
		}

		public class mx : PX.Data.BQL.BqlString.Constant<mx>
		{
			public mx() : base(MX) { }
		}

		public class plaid : PX.Data.BQL.BqlString.Constant<plaid>
		{
			public plaid() : base(Plaid) { }
		}

		public class testPlaid : PX.Data.BQL.BqlString.Constant<testPlaid>
		{
			public testPlaid() : base(Plaid) { }
		}
	}
}
