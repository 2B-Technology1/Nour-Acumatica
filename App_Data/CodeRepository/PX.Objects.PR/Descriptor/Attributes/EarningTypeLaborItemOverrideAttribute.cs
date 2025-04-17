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
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.PR
{
	public class EarningTypeLaborItemOverrideAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
	{
		private Type _EmployeeIDField;
		private Type _LaborItemField;

		public EarningTypeLaborItemOverrideAttribute(Type employeeIDField, Type laborItemField)
		{
			_EmployeeIDField = employeeIDField;
			_LaborItemField = laborItemField;
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object employeeID = sender.GetValue(e.Row, _EmployeeIDField.Name);
			object earningType = sender.GetValue(e.Row, _FieldName);

			EPEmployeeClassLaborMatrix laborItemOverride = SelectFrom<EPEmployeeClassLaborMatrix>
				.Where<EPEmployeeClassLaborMatrix.employeeID.IsEqual<P.AsInt>
					.And<EPEmployeeClassLaborMatrix.earningType.IsEqual<P.AsString>>>.View.Select(sender.Graph, employeeID, earningType).FirstTableItems.FirstOrDefault();
			if (laborItemOverride != null)
			{
				sender.SetValue(e.Row, _LaborItemField.Name, laborItemOverride.LabourItemID);
			}
		}
	}
}
