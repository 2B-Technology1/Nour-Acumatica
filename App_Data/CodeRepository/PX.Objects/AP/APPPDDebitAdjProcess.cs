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
using PX.Data.EP;
using PX.Objects.AP.MigrationMode;
using PX.Objects.CM.Extensions;
using PX.Objects.Common.Abstractions;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;
using PX.Objects.GL.Exceptions;
using PX.Objects.TX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static PX.Objects.AP.APInvoiceType;
using static PX.Objects.TX.CSTaxCalcType;

namespace PX.Objects.AP
{
	[TableAndChartDashboardType]
	public class APPPDDebitAdjProcess : PXGraph<APPPDDebitAdjProcess>
	{
		private const bool AutoReleaseDebitAdjustments = true; //TODO: now (26.10.2017) we can't apply nonreleased debit adjustment
		private const bool AutoReleaseCreditAdjustments = true;

		public PXCancel<APPPDVATAdjParameters> Cancel;
		public PXFilter<APPPDVATAdjParameters> Filter;

		[PXFilterable]
		public PXFilteredProcessing<PendingPPDVATAdjApp, APPPDVATAdjParameters> Applications;
		public APSetupNoMigrationMode apsetup;

		public override bool IsDirty => false;

		public PXAction<APPPDVATAdjParameters> viewPayment;

		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ViewPayment(PXAdapter adapter)
		{
			PendingPPDVATAdjApp adj = Applications.Current;
			if (adj != null)
			{
				APPayment payment = PXSelect<APPayment, Where<APPayment.refNbr, Equal<Current<PendingPPDVATAdjApp.payRefNbr>>,
					And<APPayment.docType, Equal<Current<PendingPPDVATAdjApp.payDocType>>>>>
						.Select(this).First();
				if (payment != null)
				{
					PXGraph graph = PXGraph.CreateInstance<APPaymentEntry>();
					PXRedirectHelper.TryRedirect(graph, payment, PXRedirectHelper.WindowMode.NewWindow);
				}
			}
			return adapter.Get();
		}

		public PXAction<APPPDVATAdjParameters> viewInvoice;

		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			PendingPPDVATAdjApp adj = Applications.Current;
			if (adj != null)
			{
				APInvoice invoice = PXSelect<APInvoice, Where<APInvoice.refNbr, Equal<Current<PendingPPDVATAdjApp.invRefNbr>>,
					And<APInvoice.docType, Equal<Current<PendingPPDVATAdjApp.invDocType>>>>>
						.Select(this).First();
				if (invoice != null)
				{
					PXGraph graph = PXGraph.CreateInstance<APInvoiceEntry>();
					PXRedirectHelper.TryRedirect(graph, invoice, PXRedirectHelper.WindowMode.NewWindow);
				}
			}
			return adapter.Get();
		}
		#region Cache Attached
		[Vendor]
		protected virtual void PendingPPDVATAdjApp_VendorID_CacheAttached(PXCache sender) { }

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[APInvoiceType.RefNbr(typeof(Search2<
			Standalone.APRegisterAlias.refNbr,
				InnerJoinSingleTable<APInvoice,
					On<APInvoice.docType, Equal<Standalone.APRegisterAlias.docType>,
					And<APInvoice.refNbr, Equal<Standalone.APRegisterAlias.refNbr>>>,
				InnerJoinSingleTable<Vendor,
					On<Standalone.APRegisterAlias.vendorID, Equal<Vendor.bAccountID>>>>,
			Where<
				Standalone.APRegisterAlias.docType, Equal<Optional<PendingPPDVATAdjApp.invDocType>>,
				And2<Where<
					Standalone.APRegisterAlias.origModule, Equal<BatchModule.moduleAP>,
					Or<Standalone.APRegisterAlias.released, Equal<True>>>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>>,
			OrderBy<Desc<Standalone.APRegisterAlias.refNbr>>>))]
		[APInvoiceType.Numbering]
		//[APInvoiceNbr]
		[PXFieldDescription]
		protected virtual void PendingPPDVATAdjApp_AdjdRefNbr_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Doc. Date")]
		protected virtual void PendingPPDVATAdjApp_AdjdDocDate_CacheAttached(PXCache sender) { }

		[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjPPDAmt))]
		[PXUIField(DisplayName = "Cash Discount")]
		protected virtual void PendingPPDVATAdjApp_CuryAdjdPPDAmt_CacheAttached(PXCache sender) { }

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Payment Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[APPaymentType.RefNbr(typeof(Search2<
			Standalone.APRegisterAlias.refNbr,
				InnerJoinSingleTable<APPayment,
					On<APPayment.docType, Equal<Standalone.APRegisterAlias.docType>,
					And<APPayment.refNbr, Equal<Standalone.APRegisterAlias.refNbr>>>,
				InnerJoinSingleTable<Vendor,
					On<Standalone.APRegisterAlias.vendorID, Equal<Vendor.bAccountID>>>>,
			Where<
				Standalone.APRegisterAlias.docType, Equal<Current<PendingPPDVATAdjApp.payDocType>>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>,
			OrderBy<Desc<Standalone.APRegisterAlias.refNbr>>>))]
		[APPaymentType.Numbering]
		[PXFieldDescription]
		protected virtual void PendingPPDVATAdjApp_AdjgRefNbr_CacheAttached(PXCache sender) { }

		#endregion

		public APPPDDebitAdjProcess()
		{
			Applications.AllowDelete = true;
			Applications.AllowInsert = false;
			Applications.SetSelected<PendingPPDVATAdjApp.selected>();
		}

		public virtual IEnumerable applications(PXAdapter adapter)
		{
			APPPDVATAdjParameters filter = Filter.Current;
			if (filter == null || filter.ApplicationDate == null || filter.BranchID == null) yield break;

			PXSelectBase<PendingPPDVATAdjApp> select = new PXSelect<PendingPPDVATAdjApp,
				Where<PendingPPDVATAdjApp.adjgDocDate, LessEqual<Current<APPPDVATAdjParameters.applicationDate>>,
					And<PendingPPDVATAdjApp.adjdBranchID, Equal<Current<APPPDVATAdjParameters.branchID>>>>>(this);

			if (filter.VendorID != null)
			{
				select.WhereAnd<Where<PendingPPDVATAdjApp.vendorID, Equal<Current<APPPDVATAdjParameters.vendorID>>>>();
			}

			foreach (PendingPPDVATAdjApp res in select.Select())
			{
				yield return res;
			}

			Filter.Cache.IsDirty = false;
		}

		protected virtual void APPPDVATAdjParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			APPPDVATAdjParameters filter = (APPPDVATAdjParameters)e.Row;
			if (filter == null) return;

			APSetup setup = apsetup.Current;
			Applications.SetProcessDelegate(list => CreatePPDVATAdjs(sender, filter, setup, list));

			bool generateOnePerVendor = filter.GenerateOnePerVendor == true;
			PXUIFieldAttribute.SetEnabled<APPPDVATAdjParameters.debitAdjDate>(sender, filter, generateOnePerVendor);
			PXUIFieldAttribute.SetEnabled<APPPDVATAdjParameters.finPeriodID>(sender, filter, generateOnePerVendor);
			PXUIFieldAttribute.SetRequired<APPPDVATAdjParameters.debitAdjDate>(sender, generateOnePerVendor);
			PXUIFieldAttribute.SetRequired<APPPDVATAdjParameters.finPeriodID>(sender, generateOnePerVendor);
		}

		public virtual void APPPDVATAdjParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APPPDVATAdjParameters row = (APPPDVATAdjParameters)e.Row;
			APPPDVATAdjParameters oldRow = (APPPDVATAdjParameters)e.OldRow;
			if (row == null || oldRow == null) return;

			if (!sender.ObjectsEqual<APPPDVATAdjParameters.applicationDate, APPPDVATAdjParameters.branchID, APPPDVATAdjParameters.vendorID>(oldRow, row))
			{
				Applications.Cache.Clear();
				Applications.Cache.ClearQueryCacheObsolete();
			}
		}

		protected virtual void APPPDVATAdjParameters_GenerateOnePerVendor_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APPPDVATAdjParameters filter = (APPPDVATAdjParameters)e.Row;
			if (filter == null) return;

			if (filter.GenerateOnePerVendor != true && (bool?)e.OldValue == true)
			{
				filter.DebitAdjDate = null;
				filter.FinPeriodID = null;

				sender.SetValuePending<APPPDVATAdjParameters.debitAdjDate>(filter, null);
				sender.SetValuePending<APPPDVATAdjParameters.finPeriodID>(filter, null);
			}
		}

		public static void CreatePPDVATAdjs(PXCache cache, APPPDVATAdjParameters filter, APSetup setup, List<PendingPPDVATAdjApp> docs)
		{
			CreatePPDDebitAdjs(cache, filter, setup, docs);
			CreatePPDCreditAdjs(cache, filter, setup, docs);
		}

		public static void CreatePPDDebitAdjs(PXCache cache, APPPDVATAdjParameters filter, APSetup setup, List<PendingPPDVATAdjApp> docs)
		{
			int i = 0;
			bool failed = false;
			APInvoiceEntry ie = PXGraph.CreateInstance<APInvoiceEntry>();
			ie.APSetup.Current = setup;
			IEnumerable<PendingPPDVATAdjApp> invoicesAndCreditAdjustments = docs.Where(doc => doc.InvDocType != APDocType.DebitAdj);

			if (filter.GenerateOnePerVendor == true)
			{
				if (filter.DebitAdjDate == null)
					throw new PXSetPropertyException(CR.Messages.EmptyValueErrorFormat,
						PXUIFieldAttribute.GetDisplayName<APPPDVATAdjParameters.debitAdjDate>(cache));

				if (filter.FinPeriodID == null)
					throw new PXSetPropertyException(CR.Messages.EmptyValueErrorFormat,
						PXUIFieldAttribute.GetDisplayName<APPPDVATAdjParameters.finPeriodID>(cache));

				Dictionary<PPDApplicationKey, List<PendingPPDVATAdjApp>> dict = new Dictionary<PPDApplicationKey, List<PendingPPDVATAdjApp>>();
				foreach (PendingPPDVATAdjApp pendingPPDDebitAdjApp in invoicesAndCreditAdjustments)
				{
					CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(ie, pendingPPDDebitAdjApp.InvCuryInfoID);

					PPDApplicationKey key = new PPDApplicationKey();
					pendingPPDDebitAdjApp.Index = i++;
					key.BranchID = pendingPPDDebitAdjApp.AdjdBranchID;
					key.BAccountID = pendingPPDDebitAdjApp.VendorID;
					key.LocationID = pendingPPDDebitAdjApp.InvVendorLocationID;
					key.CuryID = info.CuryID;
					key.CuryRate = info.CuryRate;
					key.AccountID = pendingPPDDebitAdjApp.AdjdAPAcct;
					key.SubID = pendingPPDDebitAdjApp.AdjdAPSub;
					key.TaxZoneID = pendingPPDDebitAdjApp.InvTaxZoneID;

					List<PendingPPDVATAdjApp> list;
					if (!dict.TryGetValue(key, out list))
					{
						dict[key] = list = new List<PendingPPDVATAdjApp>();
					}

					list.Add(pendingPPDDebitAdjApp);
				}

				foreach (List<PendingPPDVATAdjApp> list in dict.Values)
				{
					APInvoice invoice = CreateAndReleasePPDDebitAdj(ie, filter, list, AutoReleaseDebitAdjustments);

					if (invoice == null)
					{
						failed = true;
					}
				}
			}
			else foreach (PendingPPDVATAdjApp pendingPPDDebitAdjApp in invoicesAndCreditAdjustments)
				{
					List<PendingPPDVATAdjApp> list = new List<PendingPPDVATAdjApp>(1);
					pendingPPDDebitAdjApp.Index = i++;
					list.Add(pendingPPDDebitAdjApp);

					APInvoice invoice = CreateAndReleasePPDDebitAdj(ie, filter, list, AutoReleaseDebitAdjustments);

					if (invoice == null)
					{
						failed = true;
					}
				}

			if (failed)
			{
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}
		}


		public static APInvoice CreateAndReleasePPDDebitAdj(APInvoiceEntry ie, APPPDVATAdjParameters filter, List<PendingPPDVATAdjApp> list, bool autoReleaseDebitAdjustments)
		{
			APInvoice invDebitAdj;

			try
			{
				ie.Clear(PXClearOption.ClearAll);
				PXUIFieldAttribute.SetError(ie.Document.Cache, null, null, null);

				using (var ts = new PXTransactionScope())
				{
					try
					{
						ie.IsPPDCreateContext = true;
						invDebitAdj = ie.CreatePPDDebitAdj(filter, list);
					}
					finally
					{
						ie.IsPPDCreateContext = false;
					}

					if (invDebitAdj != null)
					{
						if (autoReleaseDebitAdjustments == true)
						{
							using (new PXTimeStampScope(null))
							{
								APDocumentRelease.ReleaseDoc(new List<APRegister> { invDebitAdj }, false);
								APPaymentEntry paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();
								APPayment debitAdj = PXSelect<APPayment,
										Where<APPayment.docType, Equal<Required<APPayment.docType>>,
											And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>
									.Select(paymentEntry, invDebitAdj.DocType, invDebitAdj.RefNbr)
									.First();
								paymentEntry.Document.Current = debitAdj;
								paymentEntry.SelectTimeStamp();
								paymentEntry.GetExtension<APPaymentEntry.MultiCurrency>().currencyinfo.Insert(
									ie.GetExtension<APInvoiceEntry.MultiCurrency>().GetCurrencyInfo(invDebitAdj.CuryInfoID)
									);
								CreatePPDApplications(paymentEntry, list, debitAdj);
								if (debitAdj.AdjFinPeriodID == null)
								{
									throw new FinancialPeriodNotDefinedForDateException(debitAdj.AdjDate);
								}
								paymentEntry.Persist();
								PXCache<APRegister>.RestoreCopy(invDebitAdj, debitAdj);
								APDocumentRelease.ReleaseDoc(new List<APRegister> { invDebitAdj }, false); // It needs to release applications
							}
						}

						foreach (PendingPPDVATAdjApp adj in list)
						{
							PXProcessing<PendingPPDVATAdjApp>.SetInfo(adj.Index, ActionsMessages.RecordProcessed);
						}
					}
					ts.Complete();
				}
			}
			catch (Exception e)
			{
				foreach (PendingPPDVATAdjApp adj in list)
				{
					PXProcessing<PendingPPDVATAdjApp>.SetError(adj.Index, e);
				}

				invDebitAdj = null;
			}

			return invDebitAdj;
		}

		public static void CreatePPDCreditAdjs(PXCache cache, APPPDVATAdjParameters filter, APSetup setup, List<PendingPPDVATAdjApp> docs)
		{
			int i = 0;
			bool failed = false;
			APInvoiceEntry ie = PXGraph.CreateInstance<APInvoiceEntry>();
			ie.APSetup.Current = setup;
			IEnumerable<PendingPPDVATAdjApp> debitAdjustments = docs.Where(doc => doc.InvDocType == APDocType.DebitAdj);

			if (filter.GenerateOnePerVendor == true)
			{
				if (filter.DebitAdjDate == null)
					throw new PXSetPropertyException(CR.Messages.EmptyValueErrorFormat,
						PXUIFieldAttribute.GetDisplayName<APPPDVATAdjParameters.debitAdjDate>(cache));

				if (filter.FinPeriodID == null)
					throw new PXSetPropertyException(CR.Messages.EmptyValueErrorFormat,
						PXUIFieldAttribute.GetDisplayName<APPPDVATAdjParameters.finPeriodID>(cache));

				Dictionary<PPDApplicationKey, List<PendingPPDVATAdjApp>> dict = new Dictionary<PPDApplicationKey, List<PendingPPDVATAdjApp>>();
				foreach (PendingPPDVATAdjApp pendingPPDDebitAdjApp in debitAdjustments)
				{
					CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(ie, pendingPPDDebitAdjApp.InvCuryInfoID);

					PPDApplicationKey key = new PPDApplicationKey();
					pendingPPDDebitAdjApp.Index = i++;
					key.BranchID = pendingPPDDebitAdjApp.AdjdBranchID;
					key.BAccountID = pendingPPDDebitAdjApp.VendorID;
					key.LocationID = pendingPPDDebitAdjApp.InvVendorLocationID;
					key.CuryID = info.CuryID;
					key.CuryRate = info.CuryRate;
					key.AccountID = pendingPPDDebitAdjApp.AdjdAPAcct;
					key.SubID = pendingPPDDebitAdjApp.AdjdAPSub;
					key.TaxZoneID = pendingPPDDebitAdjApp.InvTaxZoneID;

					List<PendingPPDVATAdjApp> list;
					if (!dict.TryGetValue(key, out list))
					{
						dict[key] = list = new List<PendingPPDVATAdjApp>();
					}

					list.Add(pendingPPDDebitAdjApp);
				}

				foreach (List<PendingPPDVATAdjApp> list in dict.Values)
				{
					APInvoice invoice = CreateAndReleasePPDCreditAdj(ie, filter, list, AutoReleaseCreditAdjustments);

					if (invoice == null)
					{
						failed = true;
					}
				}
			}
			else foreach (PendingPPDVATAdjApp pendingPPDDebitAdjApp in debitAdjustments)
				{
					List<PendingPPDVATAdjApp> list = new List<PendingPPDVATAdjApp>(1);
					pendingPPDDebitAdjApp.Index = i++;
					list.Add(pendingPPDDebitAdjApp);

					APInvoice invoice = CreateAndReleasePPDCreditAdj(ie, filter, list, AutoReleaseCreditAdjustments);

					if (invoice == null)
					{
						failed = true;
					}
				}

			if (failed)
			{
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}
		}


		public static APInvoice CreateAndReleasePPDCreditAdj(APInvoiceEntry ie, APPPDVATAdjParameters filter, List<PendingPPDVATAdjApp> list, bool autoReleaseCreditAdjustments)
		{
			APInvoice invCreditAdj;

			try
			{
				ie.Clear(PXClearOption.ClearAll);
				PXUIFieldAttribute.SetError(ie.Document.Cache, null, null, null);

				using (var ts = new PXTransactionScope())
				{
					try
					{
						ie.IsPPDCreateContext = true;
						invCreditAdj = ie.CreatePPDCreditAdj(filter, list);
					}
					finally
					{
						ie.IsPPDCreateContext = false;
					}

					if (invCreditAdj != null)
					{
						if (autoReleaseCreditAdjustments == true)
						{
							using (new PXTimeStampScope(null))
							{
								APDocumentRelease.ReleaseDoc(new List<APRegister> { invCreditAdj }, false);
								APPaymentEntry paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();

								foreach (PendingPPDVATAdjApp doc in list)
								{
									UpdateAPAdjustPPDVatAdjRef(paymentEntry, invCreditAdj.DocType, invCreditAdj.RefNbr, doc.AdjdDocType, doc.AdjdRefNbr, doc.AdjgDocType, doc.AdjgRefNbr);
								}
							}
						}

						foreach (PendingPPDVATAdjApp adj in list)
						{
							PXProcessing<PendingPPDVATAdjApp>.SetInfo(adj.Index, ActionsMessages.RecordProcessed);
						}
					}
					ts.Complete();
				}
			}
			catch (Exception e)
			{
				foreach (PendingPPDVATAdjApp adj in list)
				{
					PXProcessing<PendingPPDVATAdjApp>.SetError(adj.Index, e);
				}

				invCreditAdj = null;
			}

			return invCreditAdj;
		}


		protected static void CreatePPDApplications(APPaymentEntry paymentEntry, List<PendingPPDVATAdjApp> list, APPayment debitAdj)
		{
			foreach (PendingPPDVATAdjApp doc in list)
			{
				var adj = new APAdjust();
				adj.AdjdDocType = doc.AdjdDocType;
				adj.AdjdRefNbr = doc.AdjdRefNbr;
				adj = paymentEntry.Adjustments_Raw.Insert(adj);

				adj.CuryAdjgAmt = doc.InvCuryDocBal;
				adj = paymentEntry.Adjustments_Raw.Update(adj);

				string refNbr = debitAdj.RefNbr;
				string docType = debitAdj.DocType;
				UpdateAPAdjustPPDVatAdjRef(paymentEntry, docType, refNbr, doc.AdjdDocType, doc.AdjdRefNbr, doc.AdjgDocType, doc.AdjgRefNbr);
			}
		}

		private static void UpdateAPAdjustPPDVatAdjRef(PXGraph graph, string docType, string refNbr, string adjdDocType, string adjdRefNbr, string adjgDocType, string adjgRefNbr)
		{
			PXUpdate<Set<APAdjust.pPDVATAdjRefNbr, Required<APAdjust.pPDVATAdjRefNbr>,
					Set<APAdjust.pPDVATAdjDocType, Required<APAdjust.pPDVATAdjDocType>>>,
				APAdjust,
					Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
						And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>,
						And<APAdjust.adjgDocType, Equal<Required<APAdjust.adjgDocType>>,
						And<APAdjust.adjgRefNbr, Equal<Required<APAdjust.adjgRefNbr>>,
						And<APAdjust.released, Equal<True>,
						And<APAdjust.voided, NotEqual<True>,
						And<APAdjust.pendingPPD, Equal<True>>>>>>>>>
				.Update(graph, refNbr, docType, adjdDocType, adjdRefNbr, adjgDocType, adjgRefNbr);
		}

		public static bool CalculateDiscountedTaxes(PXCache cache, APTaxTran aptaxTran, decimal cashDiscPercent)
		{
			bool? result = null;
			object value = null;

			IBqlCreator whereTaxable = (IBqlCreator)Activator.CreateInstance(typeof(WhereAPPPDTaxable<Required<APTaxTran.taxID>>));
			whereTaxable.Verify(cache, aptaxTran, new List<object> { aptaxTran.TaxID }, ref result, ref value);

			IPXCurrencyHelper pXCurrencyHelper = cache.Graph.FindImplementation<IPXCurrencyHelper>();

			aptaxTran.CuryDiscountedTaxableAmt = cashDiscPercent == 0m
				? aptaxTran.CuryTaxableAmt
				: pXCurrencyHelper.RoundCury((decimal)(aptaxTran.CuryTaxableAmt * (1m - cashDiscPercent)));

			aptaxTran.CuryDiscountedPrice = cashDiscPercent == 0m
				? aptaxTran.CuryTaxAmt
				: pXCurrencyHelper.RoundCury((decimal)(aptaxTran.TaxRate / 100m * aptaxTran.CuryDiscountedTaxableAmt));

			return result == true;
		}
	}
}
