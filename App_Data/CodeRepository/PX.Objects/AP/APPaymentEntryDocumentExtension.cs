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
using PX.Objects.CM.Extensions;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using System;
using System.Linq;

namespace PX.Objects.AP
{
	public partial class APPaymentEntry
	{
		public class APPaymentEntryDocumentExtension : PaymentGraphExtension<APPaymentEntry, APPayment, APAdjust, APInvoice, APTran>
		{
			#region Override 

			protected override bool DiscOnDiscDate => !Base.TakeDiscAlways;

			protected override bool InternalCall => Base.UnattendedMode;

			public override PXSelectBase<APAdjust> Adjustments => Base.Adjustments_Raw;

			protected override AbstractPaymentBalanceCalculator<APAdjust, APTran> GetAbstractBalanceCalculator()
				=> new APPaymentBalanceCalculator(Base.GetExtension<MultiCurrency>());

			public override void Initialize()
			{
				base.Initialize();

				Documents = new PXSelectExtension<Payment>(Base.Document);
			}

			protected override PaymentMapping GetPaymentMapping()
			{
				return new PaymentMapping(typeof(APPayment));
			}

			public override void CalcBalancesFromAdjustedDocument(APAdjust adj, bool isCalcRGOL, bool DiscOnDiscDate)
			{
				if (Base.balanceCache == null || !Base.balanceCache.TryGetValue(adj, out var source))
					source = Base.APInvoice_VendorID_DocType_RefNbr.Select(adj.AdjdLineNbr, adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr);

				foreach (PXResult<APInvoice, APTran> res in source)
				{
					APInvoice voucher = res;
					APTran tran = res;
					CalcBalances(adj, voucher, isCalcRGOL, DiscOnDiscDate, tran);
					return;
				}

				foreach (APPayment payment in Base.APPayment_VendorID_DocType_RefNbr.Select(adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr))
				{
					CalcBalances(adj, payment, isCalcRGOL, DiscOnDiscDate, null);
				}
			}

			#endregion

			#region Handlers

			protected virtual void _(Events.FieldUpdated<APAdjust, APAdjust.curyAdjgPPDAmt> e)
			{
				if (e.Row == null) return;

				if (e.OldValue != null && e.Row.CuryDocBal == 0m && e.Row.CuryAdjgAmt < (decimal)e.OldValue)
				{
					e.Row.CuryAdjgDiscAmt = 0m;
				}
				e.Row.FillDiscAmts();
				CalcBalancesFromAdjustedDocument(e.Row, true, !Base.TakeDiscAlways);
			}


			protected virtual void _(Events.FieldUpdating<APAdjust, APAdjust.curyWhTaxBal> e)
			{
				e.Cancel = true;
				if (InternalCall || e.Row == null) return;


				if (e.Row.AdjdCuryInfoID != null && e.Row.CuryWhTaxBal == null && e.Cache.GetStatus(e.Row) != PXEntryStatus.Deleted)
				{
					CalcBalancesFromAdjustedDocument(e.Row, false, DiscOnDiscDate);
				}
				e.NewValue = e.Row.CuryWhTaxBal;
			}

			protected virtual void _(Events.FieldVerifying<APAdjust, APAdjust.curyAdjgAmt> e)
			{
				APAdjust adj = e.Row;

				foreach (string key in e.Cache.Keys.Where(key => e.Cache.GetValue(adj, key) == null))
				{
					throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName(e.Cache, key));
				}

				if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
				{
					CalcBalancesFromAdjustedDocument(e.Row, false, DiscOnDiscDate);
				}

				if (adj.CuryDocBal == null)
				{
					e.Cache.RaiseExceptionHandling<APAdjust.adjdRefNbr>(adj, adj.AdjdRefNbr,
						new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APAdjust.adjdRefNbr>(e.Cache)));
					return;
				}

				Sign balanceSign = adj.CuryOrigDocAmt < 0m ? Sign.Minus : Sign.Plus;

				if (adj.VoidAdjNbr == null && (decimal)e.NewValue * balanceSign < 0m)
				{
					throw new PXSetPropertyException(balanceSign == Sign.Plus
						? CS.Messages.Entry_GE
						: CS.Messages.Entry_LE, ((int)0).ToString());
				}

				if (adj.VoidAdjNbr != null && (decimal)e.NewValue * balanceSign > 0m)
				{
					throw new PXSetPropertyException(balanceSign == Sign.Plus
						? CS.Messages.Entry_LE
						: CS.Messages.Entry_GE, ((int)0).ToString());
				}

				if (((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt - (decimal)e.NewValue) * balanceSign < 0)
				{
					throw new PXSetPropertyException(balanceSign == Sign.Plus
						? Messages.Entry_LE
						: CS.Messages.Entry_GE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt).ToString());
				}
			}

			protected virtual void _(Events.FieldUpdated<APAdjust, APAdjust.curyAdjgAmt> e)
			{
				CalcBalancesFromAdjustedDocument(e.Row, true, false);
			}

			protected virtual void _(Events.FieldVerifying<APAdjust, APAdjust.curyAdjgPPDAmt> e)
			{
				APAdjust adj = e.Row;

				if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
				{
					CalcBalancesFromAdjustedDocument(e.Row, false, DiscOnDiscDate);
				}

				if (adj.CuryDocBal == null || adj.CuryDiscBal == null)
				{
					e.Cache.RaiseExceptionHandling<APAdjust.adjdRefNbr>(adj, adj.AdjdRefNbr,
						new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APAdjust.adjdRefNbr>(e.Cache)));
					return;
				}

				Sign balanceSign = adj.CuryOrigDocAmt < 0m ? Sign.Minus : Sign.Plus;


				if ((adj.VoidAdjNbr == null && balanceSign * Math.Sign((decimal)e.NewValue) < 0)
					|| (adj.VoidAdjNbr != null && balanceSign * Math.Sign((decimal)e.NewValue) > 0))
				{
					throw new PXSetPropertyException(
						(decimal)e.NewValue < 0m ? CS.Messages.Entry_GE : CS.Messages.Entry_LE,
						0);
				}

				decimal remainingCashDiscountBalance = (adj.CuryDiscBal ?? 0m) + (adj.CuryAdjgPPDAmt ?? 0m);

				if (adj.VoidAdjNbr == null && (adj.CuryDiscBal == 0 || Math.Sign((decimal)adj.CuryDiscBal) * Math.Sign((decimal)e.NewValue) > 0) &&
						Math.Abs(remainingCashDiscountBalance) < Math.Abs((decimal)e.NewValue))
				{
					throw new PXSetPropertyException(
						Messages.AmountEnteredExceedsRemainingCashDiscountBalance,
						remainingCashDiscountBalance.ToString());
				}

				if (adj.CuryAdjgAmt != null && (e.Cache.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (decimal?)e.Cache.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == adj.CuryAdjgAmt))
				{
					if (Math.Abs((decimal)adj.CuryDocBal) + Math.Abs((decimal)adj.CuryAdjgPPDAmt) < Math.Abs((decimal)e.NewValue))
					{
						throw new PXSetPropertyException(
							CS.Messages.Entry_LE,
							((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgPPDAmt).ToString()
							);
					}
				}

				if (adj.AdjdHasPPDTaxes == true &&
							adj.AdjgDocType == APDocType.DebitAdj)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_EQ, 0.ToString());
				}
			}

			protected virtual void _(Events.FieldVerifying<APAdjust, APAdjust.curyAdjgWhTaxAmt> e)
			{
				APAdjust adj = e.Row;

				if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
				{
					CalcBalancesFromAdjustedDocument(e.Row, false, DiscOnDiscDate);
				}

				if (adj.CuryDocBal == null || adj.CuryWhTaxBal == null)
				{
					e.Cache.RaiseExceptionHandling<APAdjust.adjdRefNbr>(adj, adj.AdjdRefNbr,
						new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APAdjust.adjdRefNbr>(e.Cache)));
					return;
				}

				if (adj.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
				}

				if (adj.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
				}
				if ((decimal)adj.CuryWhTaxBal + (decimal)adj.CuryAdjgWhTaxAmt - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(Messages.Entry_LE, ((decimal)adj.CuryWhTaxBal + (decimal)adj.CuryAdjgWhTaxAmt).ToString());
				}

				if (adj.CuryAdjgAmt != null && (e.Cache.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (decimal?)e.Cache.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == adj.CuryAdjgAmt))
				{
					if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgWhTaxAmt - (decimal)e.NewValue < 0)
					{
						throw new PXSetPropertyException(Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgWhTaxAmt).ToString());
					}
				}
			}

			protected virtual void _(Events.FieldUpdated<APAdjust, APAdjust.curyAdjgWhTaxAmt> e)
			{
				CalcBalancesFromAdjustedDocument(e.Row, true, DiscOnDiscDate);
			}
			#endregion

		}
	}
}
