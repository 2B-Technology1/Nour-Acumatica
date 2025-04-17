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
using PX.Objects.GL;
using PX.Objects.PO;
using PX.Objects.CM;
using PX.Objects.IN;
using System.Collections;

namespace PX.Objects.PM
{
	/// <summary>
	/// Extends AP Invoice Entry with Project related functionality. Requires Project Accounting feature.
	/// </summary>
	public class APInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>();
		}

		[PXOverride]
		public virtual void CopyCustomizationFieldsToAPTran(APTran apTranToFill, IAPTranSource poSourceLine, bool areCurrenciesSame)
		{
			if (CopyProjectFromLine(poSourceLine))
			{
				apTranToFill.ProjectID = poSourceLine.ProjectID;
				apTranToFill.TaskID = poSourceLine.TaskID;
			}
			else
			{
				apTranToFill.ProjectID = ProjectDefaultAttribute.NonProject();
			}

			apTranToFill.CostCodeID = poSourceLine.CostCodeID;
						
			if (IsPartiallyBilledCompleteByAmountSubcontractLine(apTranToFill, poSourceLine))
			{
				RedefaultInvoiceAmountForSubcontract(apTranToFill, poSourceLine, areCurrenciesSame);
			}
			else if(IsDebitAdjSubcontractLine(apTranToFill, poSourceLine))
			{
				RedefaultDebitAdjAmountForSubcontract(apTranToFill, poSourceLine, areCurrenciesSame);
			}
		}

		protected virtual bool CopyProjectFromLine(IAPTranSource poSourceLine)
		{
			return true;
		}

		private bool IsPartiallyBilledCompleteByAmountSubcontractLine(APTran tran, IAPTranSource line)
		{
			return line.OrderType == POOrderType.RegularSubcontract &&
					tran.TranType != APDocType.DebitAdj &&
					line.CompletePOLine == CompletePOLineTypes.Amount &&
					line.IsPartiallyBilled;
		}

		private void RedefaultInvoiceAmountForSubcontract(APTran tran, IAPTranSource line, bool areCurrenciesSame)
		{
			tran.CuryRetainageAmt = null;
			if (areCurrenciesSame)
			{
				tran.CuryLineAmt = line.UnbilledAmt;
			}
			else
			{
				decimal unbilledAmount;
				PXCurrencyAttribute.PXCurrencyHelper.CuryConvCury(Base.Document.Cache, Base.Document.Current, line.UnbilledAmt.GetValueOrDefault(), out unbilledAmount);
				tran.CuryLineAmt = unbilledAmount;
			}
		}

		private bool IsDebitAdjSubcontractLine(APTran tran, IAPTranSource line)
		{
			return line.OrderType == POOrderType.RegularSubcontract &&
					tran.TranType == APDocType.DebitAdj;
		}

		private void RedefaultDebitAdjAmountForSubcontract(APTran tran, IAPTranSource line, bool areCurrenciesSame)
		{
			tran.Qty = 0;
			tran.BaseQty = 0;
			tran.UnitCost = 0;
			tran.CuryUnitCost = 0;
			tran.RetainagePct = 0;
			tran.RetainageAmt = 0;
			tran.CuryRetainageAmt = 0;

			if (areCurrenciesSame)
			{
				tran.CuryLineAmt = line.BilledAmt;
			}
			else
			{
				decimal billedAmount;
				PXCurrencyAttribute.PXCurrencyHelper.CuryConvCury(Base.Document.Cache, Base.Document.Current, line.BilledAmt.GetValueOrDefault(), out billedAmount);
				tran.CuryLineAmt = billedAmount;
			}
		}

		[PXOverride]
		public virtual APTran InsertTranOnAddPOReceiptLine(IAPTranSource line, APTran tran, Func<IAPTranSource, APTran, APTran> baseMethod)
        {			
			if (line.OrderType == POOrderType.RegularSubcontract)
			{
				ValidateAndRaiseError(line);
				tran.CuryRetainageAmt = null;
				tran.CuryDiscAmt = 0;
				tran.DiscAmt = 0;
				tran.DiscPct = 0;
			}

			return baseMethod(line, tran);
		}

		private void ValidateAndRaiseError(IAPTranSource line)
		{
			if (line.TaskID != null && line.ExpenseAcctID != null)
			{
				GL.Account account = GL.Account.PK.Find(Base, line.ExpenseAcctID);
				if (account != null && account.AccountGroupID == null)
				{
					throw new PXSetPropertyException(Messages.NoAccountGroup2, PXErrorLevel.Error, account.AccountCD);
				}
			}
		}

		[PXOverride]
		public virtual IEnumerable Release(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
		{
			ValidatePOLines();
			return baseHandler(adapter);
		}

		private void ValidatePOLines()
		{
			bool accountValidationFailed = false;
			foreach (APTran tran in Base.Transactions.Select())
			{
				Account account = Account.PK.Find(Base, tran.AccountID);
				if (account?.AccountGroupID == null && tran.LineType == POLineType.Service && ProjectDefaultAttribute.IsProject(Base, tran.ProjectID))
				{
					InventoryItem item = InventoryItem.PK.Find(Base, tran.InventoryID);
					if (item != null && item.PostToExpenseAccount == InventoryItem.postToExpenseAccount.Sales)
						continue;

					PXTrace.WriteError(Messages.NoAccountGroup2, account.AccountCD);
					accountValidationFailed = true;
				}
			}

			if (accountValidationFailed)
			{
				throw new PXException(Messages.FailedToAddLine);
			}
		}

		protected virtual void _(Events.RowSelected<APTran> e)
		{
			if (e.Row == null)
			{
				return;
			}

			if (IsItEditableReturnRow(e.Row))
			{
				EnableReturnProperties(e.Cache, e.Row);
			}
		}

		protected virtual bool IsItEditableReturnRow(APTran tran)
			=> IsInvoiceEditable()
			&& IsItEditableReturnRowOfProjectDropShipPurchaseOrder(tran);

		protected virtual bool IsInvoiceEditable()
			=> Invoice != null
			&& Invoice.Status == APDocStatus.Hold
			&& Invoice.DocType == APDocType.DebitAdj
			&& Invoice.RetainageApply != true;

		protected virtual bool IsItEditableReturnRowOfProjectDropShipPurchaseOrder(APTran tran)
			=> tran.POOrderType == POOrderType.ProjectDropShip
			&& tran.PONbr != null
			&& tran.POLineNbr.HasValue
			&& tran.ReceiptNbr == null;

		protected virtual void EnableReturnProperties(PXCache cache, APTran row)
		{
			PXUIFieldAttribute.SetEnabled<APTran.qty>(cache, row, true);
			PXUIFieldAttribute.SetEnabled<APTran.curyUnitCost>(cache, row, true);
			PXUIFieldAttribute.SetEnabled<APTran.curyLineAmt>(cache, row, true);
		}

		private APInvoice Invoice => Base.Document.Current;
	}
}
