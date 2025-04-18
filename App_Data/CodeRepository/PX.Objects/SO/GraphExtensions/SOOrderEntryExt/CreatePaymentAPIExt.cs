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

using PX.Api;
using PX.CCProcessingBase;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.Extensions.PaymentTransaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.AR.GraphExtensions;
using ARRegisterAlias = PX.Objects.AR.Standalone.ARRegisterAlias;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{

	[PXHidden]
	public class InputCCTransactionSO : InputCCTransaction
	{
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		protected Int32? _RecordID;
		[PXInt(IsKey = true)]
		[PXDBDefault(typeof(SOAdjust.recordID))]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
	}

	public class CreatePaymentAPIExt : PXGraphExtension<CreatePaymentExt, SOOrderEntry>
	{
		public PXFilter<InputCCTransactionSO> apiInputCCTran;

		public virtual ARPaymentEntry CreatePaymentAPI(SOAdjust soAdjust, SOOrder order, string paymentType)
		{
			ARPaymentEntry paymentEntry = PXGraph.CreateInstance<ARPaymentEntry>();
			List<InputCCTransactionSO> ccTransactions = new List<InputCCTransactionSO>();
			foreach (InputCCTransactionSO tran in apiInputCCTran.Cache.Inserted)
			{
				if (tran.RecordID == soAdjust.RecordID)
					ccTransactions.Add(tran);
			}

			if (ccTransactions.Count() != 0)
				paymentEntry.IsContractBasedAPI = true;
			else if (soAdjust.Capture == true || soAdjust.Authorize == true)
				paymentEntry.arsetup.Current.HoldEntry = false;

			var payment = paymentEntry.Document.Insert(new ARPayment() { DocType = paymentType });
			FillARPaymentAPI(paymentEntry, payment, soAdjust, order);
			payment = paymentEntry.Document.Update(payment);
			PXNoteAttribute.CopyNoteAndFiles(Base.Caches[typeof(SOAdjust)], soAdjust, paymentEntry.Caches[typeof(ARPayment)], payment);

			ARPaymentEntryImportTransaction paymentExt = paymentEntry.GetExtension<ARPaymentEntryImportTransaction>();
			foreach (InputCCTransaction tran in ccTransactions)
			{
				paymentExt.apiInputCCTran.Insert(tran);
			}

			SOAdjust adj = new SOAdjust()
			{
				AdjdOrderType = order.OrderType,
				AdjdOrderNbr = order.OrderNbr
			};

			if (soAdjust.CuryAdjdAmt != null)
				adj.CuryAdjgAmt = soAdjust.CuryAdjdAmt;

			if (payment.DocType == ARPaymentType.Refund && soAdjust.ValidateCCRefundOrigTransaction != null)
			{
				adj.ValidateCCRefundOrigTransaction = soAdjust.ValidateCCRefundOrigTransaction;
			}

			var ordersToApplyTabExtension = paymentEntry.GetOrdersToApplyTabExtension(true);
			var currentOrder = ordersToApplyTabExtension.SOOrder_CustomerID_OrderType_RefNbr.Select(soAdjust.CustomerID, soAdjust.AdjdOrderType, Base.Document.Current.OrderNbr);
			if (currentOrder.Count != 1)
				throw new PXException(Messages.SOOrderNotFound);

			((SOOrder)currentOrder).ExternalTaxesImportInProgress = order.ExternalTaxesImportInProgress;
			adj = ordersToApplyTabExtension.SOAdjustments.Insert(adj);
			PXNoteAttribute.CopyNoteAndFiles(paymentEntry.Caches[typeof(ARPayment)], payment, paymentEntry.Caches[typeof(SOAdjust)], adj);

			return paymentEntry;
		}

		public virtual void FillARPaymentAPI(ARPaymentEntry paymentEntry, ARPayment arpayment, SOAdjust soAdjust, SOOrder order)
		{
			if (soAdjust.AdjgDocDate != null)
				arpayment.AdjDate = soAdjust.AdjgDocDate;
			arpayment.ExternalRef = soAdjust.ExternalRef;
			arpayment.CustomerID = order.CustomerID;
			arpayment.CustomerLocationID = order.CustomerLocationID;
			if (soAdjust.PaymentMethodID != null)
				paymentEntry.Document.Cache.SetValueExt<ARPayment.paymentMethodID>(arpayment, soAdjust.PaymentMethodID);

			FillARRefundAPI(paymentEntry, arpayment, soAdjust, order);

			if (soAdjust.PMInstanceID != null)
				paymentEntry.Document.Cache.SetValueExt<ARPayment.pMInstanceID>(arpayment, soAdjust.PMInstanceID);
			if (soAdjust.CashAccountID != null)
				paymentEntry.Document.Cache.SetValueExt<ARPayment.cashAccountID>(arpayment, soAdjust.CashAccountID);
			if (soAdjust.ProcessingCenterID != null)
				paymentEntry.Document.Cache.SetValueExt<ARPayment.processingCenterID>(arpayment, soAdjust.ProcessingCenterID);
			if (soAdjust.DocDesc != null)
				arpayment.DocDesc = soAdjust.DocDesc;
			if (soAdjust.ExtRefNbr != null)
				arpayment.ExtRefNbr = soAdjust.ExtRefNbr;
			if (soAdjust.ExternalRef != null)
				arpayment.ExternalRef = soAdjust.ExternalRef;

			if (soAdjust.NewCard == null)
			{
				//NewCard field is not currently mapped in the default endpoint (20.200.001). This section is kept to support legacy behavior.
				if (soAdjust.SaveCard != null)
					arpayment.SaveCard = soAdjust.SaveCard;
			}
			else
			{
				//Whatever PMInstanceID is null or not, it may be changed after ARPayment updated, we should ensure PMInstanceID is null if NewCard is true
				//Otherwise PMInstanceID may link to old profile and cause issue.
				if (soAdjust.NewCard == true)
				{
					//In RowSelected event for ARPayment, the NewCard value will be reset based on the following condition:
					//doc.NewCard = isCCPaymentMethod == true && doc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile ? true : false;
					//Based on the document from Acuminator(https://github.com/Acumatica/Acuminator/blob/master/docs/diagnostics/PX1047.md), the value that passed to RowSelected event should not be modified.
					//So we have to call Cache.Update(arpayment) 2 times to ensure PMInstanceID is null, otherwise NewCard value will be reset to false in RowSelected event.
					arpayment = (ARPayment)paymentEntry.Document.Cache.Update(arpayment);
					paymentEntry.Document.Cache.SetValueExt<ARPayment.pMInstanceID>(arpayment, null);
					arpayment = (ARPayment)paymentEntry.Document.Cache.Update(arpayment);
				}

				paymentEntry.Document.Cache.SetValueExt<ARPayment.newCard>(arpayment, soAdjust.NewCard);

				switch (Base1.GetSavePaymentProfileCode(soAdjust.NewCard, arpayment.ProcessingCenterID))
				{
					case SavePaymentProfileCode.Allow:
						arpayment.SaveCard = soAdjust.SaveCard;
						break;
					case SavePaymentProfileCode.Force:
						arpayment.SaveCard = true;
						break;
				}
			}

			if (soAdjust.Hold != null && soAdjust.Capture != true && soAdjust.Authorize != true && soAdjust.Refund != true)
				arpayment.Hold = soAdjust.Hold;
			arpayment.CuryOrigDocAmt = soAdjust.CuryOrigDocAmt;
		}

		public virtual void FillARRefundAPI(ARPaymentEntry paymentEntry, ARPayment arpayment, SOAdjust soAdjust, SOOrder order)
		{
			if (arpayment.DocType != ARPaymentType.Refund)
				return;

			if (soAdjust.RefTranExtNbr != null)
			{
				paymentEntry.Document.Cache.SetValueExt<ARPayment.cCTransactionRefund>(arpayment, true);
				paymentEntry.Document.Cache.SetValueExt<ARPayment.refTranExtNbr>(arpayment, soAdjust.RefTranExtNbr);
			}
			else
			{
				paymentEntry.Document.Cache.SetValueExt<ARPayment.cCTransactionRefund>(arpayment, false);
			}
		}

		protected virtual void CreatePaymentAPI(SOAdjust soAdjust, string paymentType)
		{
			ARPaymentEntry paymentEntry = CreatePaymentAPI(soAdjust, Base.Document.Current, paymentType);
			paymentEntry.Save.Press();
			Base.Adjustments.Cache.Remove(soAdjust);

			try
			{
				if (soAdjust.Capture == true)
				{
					Base1.PressButtonIfEnabled(paymentEntry, nameof(ARPaymentEntryPaymentTransaction.captureCCPayment));
				}
				else if (soAdjust.Authorize == true)
				{
					Base1.PressButtonIfEnabled(paymentEntry, nameof(ARPaymentEntryPaymentTransaction.authorizeCCPayment));
				}
				else if (soAdjust.Refund == true)
				{
					if (paymentEntry.Document.Current.Hold == false)
					{
						Base1.PressButtonIfEnabled(paymentEntry, nameof(ARPaymentEntryPaymentTransaction.creditCCPayment));
					}
				}
			}
			catch (PXBaseRedirectException)
			{
				throw;
			}
			catch (Exception exception)
			{
				Base1.RedirectToNewGraph(paymentEntry, exception);
			}
		}

		protected virtual void _(Events.RowPersisting<SOAdjust> e)
		{
			SOAdjust row = e.Row as SOAdjust;
			if (row == null) return;

			if (e.Cache.Graph.IsContractBasedAPI && row.AdjgRefNbr == null)
			{
				e.Cancel = true;
			}
		}

		private int _nesting = 0;

		public delegate void PersistDelegate();

		/// Overrides <see cref="SOOrderEntry.Persist"/>
		[PXOverride]
		public virtual void Persist(PersistDelegate baseMethod)
		{
			if (Base.Adjustments.Cache.Graph.IsContractBasedAPI && Base.Adjustments.Cache.Inserted.Count() != 0)
			{
				Action createPayments = () =>
				{
					foreach (SOAdjust row in Base.Adjustments.Cache.Inserted)
					{
						if (row.AdjgRefNbr == null)
						{
							CreatePaymentAPI(row, row.AdjgDocType ?? ARDocType.Payment);
						}
					}
					Base.Adjustments.Cache.ClearQueryCache();
				};

				bool processAsync = ((IEnumerable<SOAdjust>)Base.Adjustments.Cache.Inserted).Any(a => a.Authorize == true || a.Capture == true);

				if (processAsync)
				{
					try
					{
						_nesting++;

						try
						{
							Base.RecalculateExternalTaxesSync = true;
							baseMethod();
						}
						finally
						{
							Base.RecalculateExternalTaxesSync = false;
						}

						if (_nesting == 1)
						{
							PXLongOperation.StartOperation(Base, () =>
							{
								createPayments();
							});
						}
					}
					finally
					{
						_nesting--;
					}
				}
				else
				{
					using (var ts = new PXTransactionScope())
					{
						baseMethod();
						createPayments();
						ts.Complete();
					}
				}
			}
			else
			{
				baseMethod();
			}
		}
	}
}
