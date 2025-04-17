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
using System.Runtime.Serialization;
using PX.Data;

namespace PX.Objects.DR
{
	public class NoFairValuePriceFoundException : PXException
	{
		public NoFairValuePriceFoundException(string InventoryCD, string UOM, string CuryID, DateTime DocDate) : base(Messages.NoFairValuePriceFoundForItem, InventoryCD.Trim(), UOM, CuryID, DocDate.ToShortDateString()) { }
		public NoFairValuePriceFoundException(string ComponentCD, string InventoryCD, string UOM, string CuryID, DateTime DocDate) : base(Messages.NoFairValuePriceFoundForComponent, ComponentCD, InventoryCD.Trim(), UOM, CuryID, DocDate.ToShortDateString()) { }
		public NoFairValuePriceFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	public class NoFairValuePricesFoundException : PXException
	{
		public NoFairValuePricesFoundException(string message) : base(message) { }
		public NoFairValuePricesFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
