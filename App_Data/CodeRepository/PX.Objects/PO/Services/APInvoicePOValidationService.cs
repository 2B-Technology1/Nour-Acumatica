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
using PX.Objects.AP;
using System;
using System.Collections.Generic;
using System.Linq;
using POLineDTO = PX.Objects.PO.GraphExtensions.APInvoiceEntryExt.APInvoicePOValidation.POLineDTO;

namespace PX.Objects.PO
{
	public class APInvoicePOValidationService
	{
		protected Lazy<POSetup> _poSetup;

		public APInvoicePOValidationService(Lazy<POSetup> poSetup)
		{
			_poSetup = poSetup;
		}

		public virtual bool IsLineValidationRequired(APTran tran)
		{
			return tran != null && tran.TranType == APDocType.Invoice && tran.Sign > 0m
				&& tran.POOrderType != null && tran.PONbr != null && tran.POLineNbr != null
				&& _poSetup.Value.APInvoiceValidation == APInvoiceValidationMode.Warning;
		}

		public virtual bool ShouldCreateRevision(PXCache cache, APTran tran, string apTranCuryID, POLineDTO poLine)
		{
			return IsLineValidationRequired(tran) && (IsAPTranQtyExceedsPOLineUnbilledQty(tran, poLine)
				|| IsAPTranUnitCostExceedsPOLineUnitCost(cache, tran, apTranCuryID, poLine)
				|| IsAPTranAmountExceedsPOLineUnbilledAmount(tran, apTranCuryID, poLine));
		}

		public virtual bool IsAPTranQtyExceedsPOLineUnbilledQty(APTran tran, POLineDTO poLine)
		{
			bool isSameUom = tran.UOM == poLine.UOM;

			decimal apTranQty = isSameUom ? tran.Qty ?? 0m : tran.BaseQty ?? 0m;
			decimal poLineUnbilledQty = isSameUom ? poLine.UnbilledQty ?? 0m : poLine.BaseUnbilledQty ?? 0m;

			return apTranQty > poLineUnbilledQty;
		}

		public virtual bool IsAPTranUnitCostExceedsPOLineUnitCost(PXCache cache, APTran tran, string apTranCuryID, POLineDTO poLine)
		{
			bool isSameUom = tran.UOM == poLine.UOM;
			bool isSameCury = apTranCuryID == poLine.CuryID;
			decimal apTranUnitCost, poLineUnitCost;

			if (tran.InventoryID != null && poLine.InventoryID != null)
			{
				apTranUnitCost = isSameCury && isSameUom ? tran.CuryUnitCost ?? 0m
					: IN.INUnitAttribute.ConvertFromBase(cache, tran.InventoryID, tran.UOM, tran.UnitCost ?? 0m, IN.INPrecision.UNITCOST);
				poLineUnitCost = isSameCury && isSameUom ? poLine.CuryUnitCost ?? 0m
					: IN.INUnitAttribute.ConvertFromBase(cache, tran.InventoryID, poLine.UOM, poLine.UnitCost ?? 0m, IN.INPrecision.UNITCOST);
			}
			else
			{
				apTranUnitCost = isSameCury && isSameUom ? tran.CuryUnitCost ?? 0m : tran.UnitCost ?? 0m;
				poLineUnitCost = isSameCury && isSameUom ? poLine.CuryUnitCost ?? 0m
					: IN.INUnitAttribute.ConvertGlobal(cache.Graph, poLine.UOM, tran.UOM, poLine.UnitCost ?? 0m, IN.INPrecision.UNITCOST);
			}

			return apTranUnitCost > poLineUnitCost;
		}

		public virtual bool IsAPTranAmountExceedsPOLineUnbilledAmount(APTran tran, string apTranCuryID, POLineDTO poLine)
		{
			bool isSameCury = apTranCuryID == poLine.CuryID;

			decimal apTranAmt = isSameCury ? tran.CuryTranAmt ?? 0m : tran.TranAmt ?? 0m;
			decimal poLineUnbilledAmt = isSameCury ? poLine.CuryUnbilledAmt ?? 0m : poLine.UnbilledAmt ?? 0m;

			return apTranAmt > poLineUnbilledAmt;
		}
	}
}
