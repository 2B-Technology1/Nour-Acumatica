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
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using System;

namespace PX.Objects.SO
{
	public class InvoiceOrderArgs
	{
		public DateTime InvoiceDate { get; set; }
		public SOOrderShipment OrderShipment { get; set; }
		public SOOrder SOOrder { get; set; }
		public CurrencyInfo SoCuryInfo { get; set; }
		public SOAddress SoBillAddress { get; set; }
		public SOContact SoBillContact { get; set; }
		public PXResultset<SOShipLine, SOLine> Details { get; set; }
		public Customer Customer { get; set; }
		public InvoiceList List { get; set; }
		public PXQuickProcess.ActionFlow QuickProcessFlow { get; set; } = PXQuickProcess.ActionFlow.NoFlow;
		public bool GroupByDefaultOperation { get; set; } = false;
		public bool GroupByCustomerOrderNumber { get; set; } = false;

		public InvoiceOrderArgs() { }

		public InvoiceOrderArgs(PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact> order)
		{
			OrderShipment = order;
			SOOrder = order;
			SoCuryInfo = order;
			SoBillAddress = order;
			SoBillContact = order;
		}

		public InvoiceOrderArgs(PXResult<SOOrderShipment, SOOrder, CM.CurrencyInfo, SOAddress, SOContact> order)
		{
			OrderShipment = order;
			SOOrder = order;
			SoCuryInfo = CurrencyInfo.GetEX(order);
			SoBillAddress = order;
			SoBillContact = order;
		}
	}
}
