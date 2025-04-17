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
using PX.Common;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO
{
	public class SOOrderTypeApproval : IPrefetchable
	{
		public static string[] GetOrderTypes() => PXDatabase.GetSlot<SOOrderTypeApproval>(nameof(SOOrderTypeApproval), typeof(SOSetupApproval))._orderTypes;

		private string[] _orderTypes;

		void IPrefetchable.Prefetch()
		{
			var orderTypes = new HashSet<string>();
			foreach (PXDataRecord rec in PXDatabase.SelectMulti(typeof(SOSetupApproval),
					new PXDataField<SOSetupApproval.orderType>(),
					new PXDataFieldValue<SOSetupApproval.isActive>(true)))
			{
				var orderType = rec.GetString(0);
				if (!string.IsNullOrEmpty(orderType))
					orderTypes.Add(orderType);
			}
			_orderTypes = orderTypes.ToArray();
		}
	}
}
