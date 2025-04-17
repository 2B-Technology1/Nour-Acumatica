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

using PX.Data;

using PX.Objects.Common;

namespace PX.Objects.AR
{
	public class ARStatementType : ILabelProvider
	{
		public const string OpenItem = "O";
		public const string BalanceBroughtForward = "B";

		public const string CS_OPEN_ITEM = OpenItem;
		public const string CS_BALANCE_BROUGHT_FORWARD = BalanceBroughtForward;

		public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
		{
			{ OpenItem, Messages.OpenItem },
			{ BalanceBroughtForward, Messages.BalanceBroughtForward },
		};

		public class balanceBroughtForward : PX.Data.BQL.BqlString.Constant<balanceBroughtForward>
		{
			public balanceBroughtForward() : base(BalanceBroughtForward) { }
		}

		public class openItem : PX.Data.BQL.BqlString.Constant<openItem>
		{
			public openItem() : base(OpenItem) { }
		}

		public class balanceFoward : balanceBroughtForward { }
	}

	public class StatementTypes : ARStatementType
	{ }
}
