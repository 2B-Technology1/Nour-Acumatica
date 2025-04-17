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

using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PO
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = false)]
	public class POReceiptEntryReportsAttribute : PXStringListAttribute
	{
		public static class Values
		{
			public const string PurchaseReceipt = "PO646000";
			public const string BillingDetails = "PO632000";
			public const string Allocated = "PO622000";
		}

		[PXLocalizable]
		public static class DisplayNames
		{
			public const string PurchaseReceipt = "Print Purchase Receipt"; //from AUStepAction instead of "Purchase Receipt";
			public const string BillingDetails = "Purchase Receipt Billing History"; //from AUStepAction instead of "Purchase Receipt Billing Details"(Messages.ReportPOReceiptBillingDetails)
			public const string Allocated = Messages.ReportPOReceipAllocated;
		}

		private static Tuple<string, string>[] ValuesToLabels => new Tuple<string, string>[]
		{
			Pair(Values.PurchaseReceipt, DisplayNames.PurchaseReceipt),
			Pair(Values.BillingDetails, DisplayNames.BillingDetails),
			Pair(Values.Allocated, DisplayNames.Allocated),
		};

		public POReceiptEntryReportsAttribute() : base(ValuesToLabels)
		{ }
	}
}
