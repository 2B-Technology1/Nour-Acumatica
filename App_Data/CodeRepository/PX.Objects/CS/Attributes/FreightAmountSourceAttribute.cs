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
using PX.Data;

namespace PX.Objects.CS
{
	public class FreightAmountSourceAttribute : PXStringListAttribute
	{
		public const string ShipmentBased = "S";
		public const string OrderBased = "O";

		public class shipmentBased : PX.Data.BQL.BqlString.Constant<shipmentBased>
		{
			public shipmentBased() : base(ShipmentBased) {; }
		}
		public class orderBased : PX.Data.BQL.BqlString.Constant<orderBased>
		{
			public orderBased() : base(OrderBased) {; }
		}

		public FreightAmountSourceAttribute() : base(
			new[]
			{
				Pair(ShipmentBased, Messages.ShipmentBased),
				Pair(OrderBased, Messages.OrderBased),
			})
		{
		}
	}
}
