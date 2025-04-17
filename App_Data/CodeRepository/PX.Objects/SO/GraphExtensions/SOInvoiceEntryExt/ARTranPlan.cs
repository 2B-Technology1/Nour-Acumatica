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
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;

namespace PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt
{
	public class ARTranPlan : ItemPlan<SOInvoiceEntry, ARInvoice, ARTran>
	{
		public override void _(Events.RowUpdated<ARInvoice> e)
		{
			base._(e);

			if (!e.Cache.ObjectsEqual<ARInvoice.docDate, ARInvoice.hold, ARInvoice.creditHold>(e.Row, e.OldRow))
			{
				foreach (ARTran tran in PXSelect<ARTran,
					Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
						And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
						And<ARTran.lineType, Equal<SOLineType.inventory>,
						And<ARTran.sOOrderNbr, IsNull, And<ARTran.sOShipmentNbr, IsNull>>>>>>
					.Select(Base))
				{
					RaiseRowUpdated(tran);
					Base.Transactions.Cache.MarkUpdated(tran);
				}
			}
		}

		public override INItemPlan DefaultValues(INItemPlan planRow, ARTran tran)
		{
			if (tran.Released == true || tran.SOShipmentNbr != null || tran.SOOrderNbr != null || tran.LineType != SOLineType.Inventory
				|| tran.InvtMult == 0)
			{
				return null;
			}
			ARInvoice parent = (ARInvoice)Base.Caches<ARInvoice>().Current;
			bool? hold = parent.Hold | parent.CreditHold;

			planRow.BAccountID = tran.CustomerID;
			planRow.PlanType = (hold == true) ? INPlanConstants.Plan69 : INPlanConstants.Plan62;
			planRow.InventoryID = tran.InventoryID;
			planRow.SubItemID = tran.SubItemID;
			planRow.SiteID = tran.SiteID;
			planRow.LocationID = tran.LocationID;
			planRow.LotSerialNbr = tran.LotSerialNbr;
			planRow.ProjectID = tran.ProjectID;
			planRow.TaskID = tran.TaskID;
			planRow.Reverse = (tran.InvtMult > 0) ^ (tran.BaseQty < 0m);
			planRow.PlanDate = parent.DocDate;
			planRow.UOM = tran.UOM;
			planRow.PlanQty = Math.Abs(tran.BaseQty ?? 0m);
			planRow.RefNoteID = parent.NoteID;
			planRow.Hold = hold;

			return planRow;
		}
	}
}
