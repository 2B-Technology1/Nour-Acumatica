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
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.Extensions.CustomerCreditHold;
using PX.Objects.GL;
using PX.Objects.SO;
using EnforceType = PX.Objects.Extensions.CustomerCreditHold.CreditVerificationResult.EnforceType;

namespace PX.Objects.AR.GraphExtensions
{
	/// <summary>A mapped generic graph extension that defines the AR invoice credit helper functionality.</summary>	
	public abstract class ARInvoiceCustomerCreditExtension<TGraph> : CustomerCreditExtension<
			TGraph,
			ARInvoice,
			ARInvoice.customerID,
			ARInvoice.creditHold,
			ARInvoice.released,
			ARInvoice.status>
		where TGraph : PXGraph
	{
		#region State

		private Dictionary<Type, List<PXRowUpdated>> _preupdatedevents = new Dictionary<Type, List<PXRowUpdated>>();

		#endregion

		#region Implementation
		protected override bool? GetHoldValue(PXCache sender, ARInvoice Row)
		{
			return (Row?.Hold == true || Row?.PendingProcessing == true || Row?.CreditHold == true);
		}

		protected override bool? GetCreditCheckError(PXCache sender, ARInvoice Row)
		{
			if (Row.OrigModule == BatchModule.SO)
			{
				SOSetup soSetup = GetSOSetup();
				return soSetup?.CreditCheckError;
			}
			else
			{
				ARSetup arSetup = GetARSetup();
				return arSetup?.CreditCheckError;
			}
		}

		protected override void _(Events.RowUpdated<ARInvoice> e)
		{
			ARInvoice row = e.Row;
			ARInvoice oldRow = e.OldRow;

			if (row == null) return;

			if (e.Row != null && e.OldRow != null)
				using (new ReadOnlyScope(e.Cache, Base.Caches[typeof(ARBalances)]))
				UpdateARBalances(e.Cache, e.Row, e.OldRow);

			if (_InternalCall)
			{
				return;
			}
			List<PXRowUpdated> evLst;
			if (_preupdatedevents.TryGetValue(typeof(ARInvoice), out evLst))
			{
				foreach (var ev in evLst)
				{
					PXRowUpdatedEventArgs arInvoiceEventErgs = new PXRowUpdatedEventArgs(row, oldRow, e.ExternalCall);
					ev.Invoke(e.Cache, arInvoiceEventErgs);
				}
			}

			if (oldRow != null && row.CreditHold != oldRow.CreditHold && row.CreditHold == false
				&& row.Hold == false && row.PendingProcessing == false)
			{
				object DocumentBal = row.OrigDocAmt;

				e.Cache.SetValue<ARInvoice.approvedCredit>(row, true);
				e.Cache.SetValue<ARInvoice.approvedCreditAmt>(row, DocumentBal);
				if (row.CaptureFailedCntr > 0)
				{
					e.Cache.SetValue<ARInvoice.approvedCaptureFailed>(row, true);
				}
				var customer = EnsureCustomer(e.Cache, row);
				if (IsPrepaymentRequired(customer, row))
				{
					e.Cache.SetValue<ARInvoice.approvedPrepaymentRequired>(row, true);
				}
			}

			base._(e);
			if(Base.PrimaryItemType != typeof(ARInvoice) && !e.Cache.ObjectsEqual<ARInvoice.creditHold>(e.Row, e.OldRow))
				ARInvoice.Events.FireOnPropertyChanged<ARInvoice.creditHold>(e.Cache.Graph, e.Row);
		}

		public virtual void AppendPreUpdatedEvent(Type entity, PXRowUpdated del)
		{
			List<PXRowUpdated> evLst;
			if (!_preupdatedevents.TryGetValue(entity, out evLst))
				evLst = new List<PXRowUpdated>();
			evLst.Add(del);
			_preupdatedevents[entity] = evLst;
		}
		public virtual void RemovePreUpdatedEvent(Type entity, PXRowUpdated del)
		{
			List<PXRowUpdated> evLst;
			if (!_preupdatedevents.TryGetValue(entity, out evLst))
				return;
			evLst.Remove(del);
		}

		protected override decimal? GetDocumentBalance(PXCache cache, ARInvoice row)
		{
			decimal? DocumentBal = 0m;
			ARBalances accumbal = cache.Current as ARBalances;
			if (accumbal != null && cache.GetStatus(accumbal) == PXEntryStatus.Inserted)
			{
				//get balance only from PXAccumulator
				DocumentBal = accumbal.UnreleasedBal;
			}

			if (DocumentBal > 0m && IsFullAmountApproved(row))
			{
				DocumentBal = 0m;
			}

			return DocumentBal;
		}

		protected override void PlaceOnHold(PXCache sender, ARInvoice row, EnforceType enforceType)
		{
			if (enforceType == EnforceType.AdminHold)
			{
				sender.RaiseExceptionHandling<ARInvoice.hold>(row, true, new PXSetPropertyException(AR.Messages.AdminHoldEntry, PXErrorLevel.Warning));

				object oldRow = sender.CreateCopy(row);
				sender.SetValueExt<ARInvoice.creditHold>(row, false);
				sender.SetValueExt<ARInvoice.hold>(row, true);
				sender.RaiseRowUpdated(row, oldRow);
			}
			else
			{
				base.PlaceOnHold(sender, row, EnforceType.CreditHold);
			}

			sender.SetValue<ARInvoice.approvedCredit>(row, false);
			sender.SetValue<ARInvoice.approvedCreditAmt>(row, 0m);
			sender.SetValue<ARInvoice.approvedCaptureFailed>(row, false);
			sender.SetValue<ARInvoice.approvedPrepaymentRequired>(row, false);
		}

		protected virtual void _(Events.RowInserted<ARInvoice> e)
		{
			if (e.Row == null) return;

			UpdateARBalances(e.Cache, e.Row, null);
		}

		protected virtual void _(Events.RowDeleted<ARInvoice> e)
		{
			if (e.Row == null) return;

			UpdateARBalances(e.Cache, null, e.Row);
		}

		public override void UpdateARBalances(PXCache cache, ARInvoice newRow, ARInvoice oldRow)
		{
			if (oldRow != null)
			{
				ARReleaseProcess.UpdateARBalances(cache.Graph, oldRow, -oldRow.OrigDocAmt);
			}

			if (newRow != null)
			{
				ARReleaseProcess.UpdateARBalances(cache.Graph, newRow, newRow.OrigDocAmt);
			}
		}

		protected virtual void _(Events.FieldUpdated<ARInvoice, ARInvoice.hold> eventArgs)
		{
			if (eventArgs.Row?.Hold == true && eventArgs.Row.Released != true)
				eventArgs.Cache.SetValue<ARInvoice.creditHold>(eventArgs.Row, false);
			if (eventArgs.Row?.Status == ARDocStatus.CCHold)
				ARInvoice.Events.FireOnPropertyChanged<ARInvoice.creditHold>(eventArgs.Cache.Graph, eventArgs.Row);
		}

		protected virtual void _(Events.FieldUpdated<ARInvoice, ARInvoice.pendingProcessing> eventArgs)
		{
			if (eventArgs.Row?.PendingProcessing == true && eventArgs.Row.Released != true)
				eventArgs.Cache.SetValue<ARInvoice.creditHold>(eventArgs.Row, false);
			if (eventArgs.Row?.Status == ARDocStatus.CCHold)
				ARInvoice.Events.FireOnPropertyChanged<ARInvoice.creditHold>(eventArgs.Cache.Graph, eventArgs.Row);
		}

		protected virtual bool IsFullAmountApproved(ARInvoice row)
			=> row.ApprovedCredit == true && row.ApprovedCreditAmt >= row.OrigDocAmt;

		protected override CreditVerificationResult VerifyByCreditRules(
			PXCache sender, ARInvoice Row,
			Customer customer, CustomerClass customerclass)
		{
			var ret = base.VerifyByCreditRules(sender, Row, customer, customerclass);
			if (ret.Failed == false
				&& Row.OrigModule == BatchModule.SO
				&& Row.CaptureFailedCntr > 0 && Row.ApprovedCaptureFailed == false)
			{
				ret.Failed = true;
				ret.Enforce = EnforceType.CreditHold;
			}

			if (ret.Failed == false
				&& ret.Hold == false
				&& Row.OrigModule == BatchModule.SO
				&& Row.ApprovedPrepaymentRequired == false)
			{
				if (IsPrepaymentRequired(customer, Row))
				{
					ret.Failed = true;
					ret.Enforce = EnforceType.CreditHold;
				}
			}

			return ret;
		}

		public virtual bool IsPrepaymentRequired(Customer customer, ARInvoice invoice)
		{
			if (invoice.OrigModule != BatchModule.SO
				|| invoice.DocType.IsIn(ARDocType.CashSale, ARDocType.CashReturn)
				|| invoice.CuryUnpaidBalance <= 0m)
			{
				return false;
			}

			if (invoice.TermsID == null)
				return false;

			Terms terms = Terms.PK.Find(Base, invoice.TermsID);
			if (terms == null || terms.PrepaymentRequired != true || terms.PrepaymentPct <= 0m)
			{
				return false;
			}

			SOOrderShipment orderInvoiced = PXSelect<SOOrderShipment,
				Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>,
					And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>>
				.SelectSingleBound(Base, new[] { invoice });
			if (orderInvoiced == null)
			{
				return false;
			}

			return true;
		}

		#endregion

		#region Abstract methods

		protected abstract SOSetup GetSOSetup();

		#endregion // Abstract methods
	}

	public class ARInvoiceEntry_ARInvoiceCustomerCreditExtension : ARInvoiceCustomerCreditExtension<ARInvoiceEntry>
	{
		protected override ARSetup GetARSetup()
			=> Base.ARSetup.Current;

		protected override SOSetup GetSOSetup()
			=> Base.soSetup.Current;
	}

	public class ARPaymentEntry_ARInvoiceCustomerCreditExtension : ARInvoiceCustomerCreditExtension<ARPaymentEntry>
	{
		protected override void _(Events.RowSelected<ARInvoice> e)
		{
			// suppress base behavior for better performance
		}

		protected override void _(Events.RowPersisting<ARInvoice> e)
		{
			// suppress base behavior for better performance
		}

		protected override decimal? GetDocumentBalance(PXCache cache, ARInvoice row)
		{
			// we cannot use original approach with accumulators
			// because inside of ARPaymentEntry several documents may affect the balance
			if (GetHoldValue(cache, row) == true || IsFullAmountApproved(row))
				return 0m;

			return row.SignBalance * row.OrigDocAmt;
		}

		protected override ARSetup GetARSetup()
			=> Base.arsetup.Current;

		protected override SOSetup GetSOSetup()
			=> PXSetup<SOSetup>.Select(Base);
	}
}
