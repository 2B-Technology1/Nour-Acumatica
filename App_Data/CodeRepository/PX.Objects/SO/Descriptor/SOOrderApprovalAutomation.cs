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
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.SO
{
	public class SOOrderApprovalAutomation : EPApprovalAutomation<SOOrder, SOOrder.approved, SOOrder.rejected, SOOrder.hold, SOSetupApproval>
	{
		public SOOrderApprovalAutomation(PXGraph graph, Delegate handler) : base(graph, handler) { }

		public SOOrderApprovalAutomation(PXGraph graph) : base(graph) { }

		protected override bool AllowAssign(PXCache cache, SOOrder oldDoc, SOOrder doc)
		{
			var oldHold = oldDoc.Hold == true;
			var oldCancelled = oldDoc.Cancelled == true;
			var cancelled = doc.Cancelled == true;

			if (oldHold)
			{
				if (cancelled)
					return false;//Hold -> Cancelled
				return true;
			}

			return oldCancelled && !cancelled; //Cancelled -> Open(Pending Approval and other)
		}
	}
}
