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
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using PX.Concurrency.ParallelProcessing;

namespace PX.Objects.FS
{
	public abstract class CreateInvoiceBase<TGraph, TPostLine> : PXGraph<TGraph>, IInvoiceProcessGraph
        where TGraph : PXGraph
        where TPostLine : class, IBqlTable, IPostLine, new()
    {
        protected StringBuilder groupKey = null;
        protected string billingBy = null;

        #region Selects
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;
        public PXFilter<CreateInvoiceFilter> Filter;
        public PXCancel<CreateInvoiceFilter> Cancel;

        #endregion

        #region Actions
        #region FilterManually
        public PXAction<CreateInvoiceFilter> filterManually;
        [PXUIField(DisplayName = "Apply Filters")]
        public virtual IEnumerable FilterManually(PXAdapter adapter)
        {
            Filter.Current.LoadData = true;
            return adapter.Get();
        }

        #endregion
        #region OpenReviewTemporaryBatch
        public PXAction<CreateInvoiceFilter> openReviewTemporaryBatch;
        [PXUIField(DisplayName = "View Temporary Batches", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(VisibleOnProcessingResults = true)]
        public virtual void OpenReviewTemporaryBatch()
        {
            ReviewInvoiceBatches graphReviewInvoiceBatches = PXGraph.CreateInstance<ReviewInvoiceBatches>();
            PXRedirectHelper.TryRedirect(graphReviewInvoiceBatches, PXRedirectHelper.WindowMode.NewWindow);
        }
        #endregion
        #endregion

        public CreateInvoiceBase()
        {
            IncludeReviewInvoiceBatchesAction();
        }

        public OnDocumentHeaderInsertedDelegate OnDocumentHeaderInserted { get; set; }

        public OnTransactionInsertedDelegate OnTransactionInserted { get; set; }

        public BeforeSaveDelegate BeforeSave { get; set; }

        public AfterCreateInvoiceDelegate AfterCreateInvoice { get; set; }

        public PXGraph GetGraph()
        {
            return this;
        }

        #region Event Handlers

        protected virtual void _(Events.RowSelected<CreateInvoiceFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            CreateInvoiceFilter createInvoiceFilterRow = (CreateInvoiceFilter)e.Row;

            if (SetupRecord.Current != null)
            {
                filterManually.SetVisible(SetupRecord.Current.FilterInvoicingManually == true);
            }

            HideOrShowInvoiceActions(e.Cache, createInvoiceFilterRow);

            FSPostTo.SetLineTypeList<CreateInvoiceFilter.postTo>(e.Cache, e.Row, true, false, true, true);
        }

        protected virtual void _(Events.RowUpdated<CreateInvoiceFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            Filter.Current.LoadData = false;
        }

        #endregion

        #region Invoicing Methods
        public virtual Guid CreateInvoices(CreateInvoiceBase<TGraph, TPostLine> processGraph, List<TPostLine> postLineRows, CreateInvoiceFilter filter, PXQuickProcess.ActionFlow quickProcessFlow, bool isGenerateInvoiceScreen)
        {
            PXTrace.WriteInformation("Data preparation started.");

            Guid currentProcessID = processGraph.CreatePostDocsFromUserSelection(postLineRows);

			PXResultset<FSPostDoc> billingCycles = PXSelectGroupBy<FSPostDoc,
												   Where<FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>>,
												   Aggregate<GroupBy<FSPostDoc.billingCycleID>>,
												   OrderBy<Asc<FSPostDoc.billingCycleID>>>.Select(processGraph, currentProcessID);

            foreach (FSPostDoc billingCycle in billingCycles)
            {
                if (filter.SOQuickProcess == true && quickProcessFlow == PXQuickProcess.ActionFlow.NoFlow)
                {
                    quickProcessFlow = PXQuickProcess.ActionFlow.HasNextInFlow;
                }

				PXTrace.WriteInformation("Invoice generation started.");
				processGraph.CreatePostingBatchForBillingCycle(currentProcessID, (int)billingCycle.BillingCycleID, filter, postLineRows, quickProcessFlow, isGenerateInvoiceScreen);
				PXTrace.WriteInformation("Invoice generation completed.");
			}
            PXTrace.WriteInformation("Data preparation completed.");
            
            PXTrace.WriteInformation("Clean of unprocessed documents started.");
            processGraph.DeletePostDocsWithError(currentProcessID);
            PXTrace.WriteInformation("Clean of unprocessed documents completed.");

            PXTrace.WriteInformation("External tax calculation started.");
            processGraph.CalculateExternalTaxes(currentProcessID);
            PXTrace.WriteInformation("External tax calculation completed.");

            ApplyInvoiceActions(processGraph.GetGraph(), filter, currentProcessID);

            return currentProcessID;
        }

        public virtual void ApplyInvoiceActions(PXGraph graph, CreateInvoiceFilter filter, Guid currentProcessID)
        {
            switch (filter.PostTo)
            {
                case ID.Batch_PostTo.SO:

                    if (filter.EmailSalesOrder == true
                        || filter.PrepareInvoice == true
                            || filter.SOQuickProcess == true)
                    {
                        SOOrderEntry soOrderEntryGraph = PXGraph.CreateInstance<SOOrderEntry>();

                        var rows = PXSelectJoin<SOOrder,
                                   InnerJoin<FSPostDoc,
                                        On<FSPostDoc.postRefNbr, Equal<SOOrder.orderNbr>,
                                        And<
                                            Where<FSPostDoc.postOrderType, Equal<SOOrder.orderType>,
                                                Or<FSPostDoc.postOrderTypeNegativeBalance, Equal<SOOrder.orderType>>>>>>,
                                   Where<FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>>>
                                   .Select(graph, currentProcessID);

                        foreach (var row in rows)
                        {
                            SOOrder sOOrder = (SOOrder)row;
                            soOrderEntryGraph.Document.Current = soOrderEntryGraph.Document.Search<SOOrder.orderNbr>(sOOrder.OrderNbr, sOOrder.OrderType);

                            if (sOOrder.Hold == true)
                            {
                                soOrderEntryGraph.Document.Cache.SetValueExt<SOOrder.hold>(sOOrder, false);
                                soOrderEntryGraph.Save.Press();
                            }

                            PXAdapter adapterSO = new PXAdapter(soOrderEntryGraph.CurrentDocument);

                            if (filter.EmailSalesOrder == true)
                            {
                                var args = new Dictionary<string, object>();
                                args["notificationCD"] = "SALES ORDER";

                                adapterSO.Arguments = args;

                                soOrderEntryGraph.notification.PressButton(adapterSO);
                            }

                            if (filter.SOQuickProcess == true
                                    && soOrderEntryGraph.soordertype.Current != null
                                        && soOrderEntryGraph.soordertype.Current.AllowQuickProcess == true)
                            {
                                SO.SOOrderEntry.SOQuickProcess.InitQuickProcessPanel(soOrderEntryGraph, "");
                                PXQuickProcess.Start(soOrderEntryGraph, sOOrder, soOrderEntryGraph.SOQuickProcessExt.QuickProcessParameters.Current);
                            }
                            else
                            {
                                if (filter.PrepareInvoice == true)
                                {
                                    if (soOrderEntryGraph.prepareInvoice.GetEnabled() == true)
                                    {
                                        adapterSO.MassProcess = true;
                                        soOrderEntryGraph.prepareInvoice.PressButton(adapterSO);
                                    }

                                    if (filter.ReleaseInvoice == true)
                                    {
                                        var shipmentsList = soOrderEntryGraph.shipmentlist.Select();

                                        if (shipmentsList.Count > 0)
                                        {
                                            SOOrderShipment soOrderShipmentRow = shipmentsList[0];
                                            SOInvoiceEntry soInvoiceEntryGraph = PXGraph.CreateInstance<SOInvoiceEntry>();
                                            soInvoiceEntryGraph.Document.Current = soInvoiceEntryGraph.Document.Search<ARInvoice.docType, ARInvoice.refNbr>(soOrderShipmentRow.InvoiceType, soOrderShipmentRow.InvoiceNbr, soOrderShipmentRow.InvoiceType);

                                            PXAdapter adapterAR = new PXAdapter(soInvoiceEntryGraph.CurrentDocument);
                                            adapterAR.MassProcess = true;

                                            soInvoiceEntryGraph.release.PressButton(adapterAR);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    break;

                // @TODO AC-142850
                case ID.Batch_PostTo.AR_AP:
                    break;
                case ID.Batch_PostTo.PM:
                    try
                    {
                        ReleaseCreatedINIssues(graph, currentProcessID);
                        ReleaseCreatedPMTransactions(graph, currentProcessID);
                    }
                    catch (Exception ex)
                    {
                        PXTrace.WriteError(ex);
                    }
                    break;
            }
        }

        public virtual void ReleaseCreatedINIssues(PXGraph graph, Guid currentProcessID)
        {
            IEnumerable<INRegister> createdIssues = PXSelectJoin<INRegister,
                                   InnerJoin<FSPostDoc,
                                        On<FSPostDoc.postRefNbr, Equal<INRegister.refNbr>,
                                        And<FSPostDoc.postDocType, Equal<INRegister.docType>>>,
                                   InnerJoin<FSServiceOrder, On<FSServiceOrder.sOID, Equal<FSPostDoc.sOID>>,
                                   InnerJoin<FSSrvOrdType, On<FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>>>>,
                                   Where<FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>,
                                     And<FSPostDoc.postedTO, Equal<Required<FSPostDoc.postedTO>>,
                                     And<FSSrvOrdType.releaseIssueOnInvoice, Equal<True>>>>>
                                   .Select(graph, currentProcessID, ID.Batch_PostTo.IN)
                                   .RowCast<INRegister>()
                                   .AsEnumerable()
                                   .ToList();

            if (createdIssues.Count() > 0)
            {
                INIssueEntry issueEntry = PXGraph.CreateInstance<INIssueEntry>();
                foreach (INRegister record in createdIssues)
                {
                    INRegister inRegisterRow = issueEntry.issue.Current = issueEntry.issue.Search<INRegister.refNbr>(record.RefNbr, record.DocType);
                    if (inRegisterRow.Hold == true)
                    {
                        issueEntry.issue.Cache.SetValueExtIfDifferent<INRegister.hold>(inRegisterRow, false);
                        inRegisterRow = issueEntry.issue.Update(inRegisterRow);
                    }

                    issueEntry.release.Press();
                }
            }
        }

        public virtual void ReleaseCreatedPMTransactions(PXGraph graph, Guid currentProcessID)
        {
            IEnumerable<PMRegister> createdPMTran = PXSelectJoin<PMRegister,
                                   InnerJoin<FSPostDoc,
                                        On<FSPostDoc.postRefNbr, Equal<PMRegister.refNbr>,
                                        And<FSPostDoc.postDocType, Equal<PMRegister.module>>>,
                                   InnerJoin<FSServiceOrder, On<FSServiceOrder.sOID, Equal<FSPostDoc.sOID>>,
                                   InnerJoin<FSSrvOrdType, On<FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>>>>,
                                   Where<FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>,
                                     And<FSPostDoc.postedTO, Equal<Required<FSPostDoc.postedTO>>,
                                     And<FSSrvOrdType.releaseProjectTransactionOnInvoice, Equal<True>>>>>
                                   .Select(graph, currentProcessID, ID.Batch_PostTo.PM)
                                   .RowCast<PMRegister>()
                                   .AsEnumerable()
                                   .ToList();

            if (createdPMTran.Count() > 0)
            {
                RegisterEntry pmEntry = PXGraph.CreateInstance<RegisterEntry>();
                foreach (PMRegister record in createdPMTran)
                {
                    PMRegister pmRegisterRow = pmEntry.Document.Current = pmEntry.Document.Search<PMRegister.refNbr>(record.RefNbr, record.Module);

                    pmEntry.ReleaseDocument(pmRegisterRow);
                }
            }
        }

        protected virtual void CreatePostingBatchAndInvoices(Guid currentProcessID, int billingCycleID, DateTime? upToDate, DateTime? invoiceDate, string invoiceFinPeriodID, string postTo, List<FSPostDoc> invoiceList, List<TPostLine> postLineRows, PXQuickProcess.ActionFlow quickProcessFlow, bool isGenerateInvoiceScreen)
        {
			var postBatchShared = new PostBatchShared();
			CreatePostingBatch(postBatchShared, billingCycleID, upToDate, invoiceDate, invoiceFinPeriodID, postTo);

			int docsQty = 0;
			foreach (FSPostDoc invoiceItem in invoiceList)
            {
				List<DocLineExt> docLines = GetInvoiceLines(currentProcessID, billingCycleID, invoiceItem.GroupKey, false, out decimal? invoiceTotal, postTo);
				try
				{
					docsQty += CreateInvoiceDocument(postBatchShared, postTo, currentProcessID, billingCycleID, invoiceItem.GroupKey, invoiceItem.InvtMult, docLines, billingBy, quickProcessFlow);
					foreach (TPostLine postLineRow in postLineRows)
					{
						if (postLineRow.BillingCycleID == billingCycleID && postLineRow.GroupKey == invoiceItem.GroupKey)
						{
							postLineRow.BatchID = postBatchShared.FSPostBatchRow?.BatchID;
							if (isGenerateInvoiceScreen == true)
							{
								PXProcessing<TPostLine>.SetInfo((int)postLineRow.RowIndex, TX.Messages.RECORD_PROCESSED_SUCCESSFULLY);
							}
						}
					}
				}
				catch (Exception exc)
				{
					foreach (TPostLine postLineRow in postLineRows)
					{
						if (postLineRow.BillingCycleID == billingCycleID && postLineRow.GroupKey == invoiceItem.GroupKey)
						{
							postLineRow.BatchID = null;
							if (isGenerateInvoiceScreen == true)
							{
								PXProcessing<TPostLine>.SetError((int)postLineRow.RowIndex, exc is PXOuterException ? exc.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)exc).InnerMessages) : exc.Message);
							}
						}
					}
					if (isGenerateInvoiceScreen == false)
					{
						throw;
					}
				}
			}
			ApplyPrepayments(postBatchShared);
			CompletePostingBatch(postBatchShared, docsQty);
		}

		public virtual void CreatePostingBatch(PostBatchShared postBatchShared, int billingCycleID, DateTime? upToDate, DateTime? invoiceDate, string invoiceFinPeriodID,  string targetScreen)
		{
			postBatchShared.PostBatchEntryGraph = PXGraph.CreateInstance<PostBatchEntry>();
			postBatchShared.FSPostBatchRow = postBatchShared.PostBatchEntryGraph.CreatePostingBatch(billingCycleID, upToDate, invoiceDate, invoiceFinPeriodID, targetScreen);
		}

		public virtual void ApplyPrepayments(FSPostBatch fsPostBatchRow)
        {
			try
			{
				if (fsPostBatchRow != null && fsPostBatchRow.PostTo == ID.Batch_PostTo.SO)
				{
					SOOrderEntry graphSOOrderEntry = PXGraph.CreateInstance<SOOrderEntry>();

					var results = PXSelectJoin<PostingBatchDetail,
								  InnerJoin<FSAdjust,
								  On<
									  PostingBatchDetail.sORefNbr, Equal<FSAdjust.adjdOrderNbr>,
									  And<PostingBatchDetail.srvOrdType, Equal<FSAdjust.adjdOrderType>>>>,
								  Where<
									  PostingBatchDetail.batchID, Equal<Required<FSPostBatch.batchID>>>>
								  .Select(graphSOOrderEntry, fsPostBatchRow.BatchID);

					foreach (PXResult<PostingBatchDetail, FSAdjust> result in results)
					{
						graphSOOrderEntry.Clear();

						PostingBatchDetail postingBatchDetailRow = (PostingBatchDetail)result;
						FSAdjust fsAdjustRow = (FSAdjust)result;

						SOOrder sOOrderRow = null;
						sOOrderRow = graphSOOrderEntry.Document.Current = graphSOOrderEntry.Document.Search<SOOrder.orderNbr>(postingBatchDetailRow.SOOrderNbr, postingBatchDetailRow.SOOrderType);
						PXResultset<SOTax> soTaxRows = graphSOOrderEntry.Tax_Rows.Select();

						SharedClasses.SOPrepaymentHelper SOPrepaymentApplication = new SharedClasses.SOPrepaymentHelper();

						foreach (SOLine soLineRow in graphSOOrderEntry.Transactions.Select())
						{
							FSxSOLine fSxSOLineRow = graphSOOrderEntry.Transactions.Cache.GetExtension<FSxSOLine>(soLineRow);

							SOTax soTaxRow = soTaxRows.RowCast<SOTax>().Where(_ => _.LineNbr == soLineRow.LineNbr).FirstOrDefault();
							decimal soTaxLine = (soTaxRow != null) ? (soTaxRow.CuryTaxableAmt * soTaxRow.TaxRate / 100) ?? 0m : 0m;

							SOPrepaymentApplication.Add(soLineRow, fSxSOLineRow, soTaxLine);
						}

						decimal CuryDocBal = 0m;

						foreach (SharedClasses.SOPrepaymentBySO row in SOPrepaymentApplication.SOPrepaymentList)
						{
							PXResultset<ARPayment> PaymentList = row.GetPrepaymentBySO(graphSOOrderEntry);
							int i = 0;

							while (PaymentList != null && i < PaymentList.Count && row.unpaidAmount > 0)
							{
								if (string.Equals(((ARPayment)PaymentList[i]).CuryID, sOOrderRow.CuryID) == true)
								{
									SOAdjust sOAdjust = new SOAdjust();
									sOAdjust.AdjgDocType = ARPaymentType.Prepayment;
									sOAdjust = graphSOOrderEntry.Adjustments.Current = graphSOOrderEntry.Adjustments.Insert(sOAdjust);

									graphSOOrderEntry.Adjustments.SetValueExt<SOAdjust.adjgRefNbr>(sOAdjust, ((ARPayment)PaymentList[i]).RefNbr);

									CuryDocBal = sOAdjust.CuryDocBal ?? 0m;
									decimal? adjdAmt = 0;

									if (CuryDocBal > 0)
									{
										if (row.unpaidAmount > CuryDocBal)
										{
											adjdAmt = CuryDocBal;
                                            row.unpaidAmount -= CuryDocBal;
											CuryDocBal = 0;
										}
										else
										{
											graphSOOrderEntry.Adjustments.SetValueExt<SOAdjust.adjgRefNbr>(sOAdjust, ((ARPayment)PaymentList[i]).RefNbr);
											adjdAmt = row.unpaidAmount;
											CuryDocBal = CuryDocBal - row.unpaidAmount ?? 0;
											row.unpaidAmount = 0;
										}
										graphSOOrderEntry.Adjustments.SetValueExt<SOAdjust.curyAdjdAmt>(sOAdjust, adjdAmt);
									}

									foreach (SOTaxTran tax in graphSOOrderEntry.Taxes.Select())
									{
										if (CuryDocBal > 0)
										{
											if (tax.CuryTaxAmt > CuryDocBal)
											{
												adjdAmt += CuryDocBal;
												break;
											}
											else
											{
												adjdAmt += tax.CuryTaxAmt;
												CuryDocBal = CuryDocBal - tax.CuryTaxAmt ?? 0;
											}
											graphSOOrderEntry.Adjustments.SetValueExt<SOAdjust.curyAdjdAmt>(sOAdjust, adjdAmt);
										}
									}
								
									graphSOOrderEntry.Adjustments.Update(sOAdjust);
								}

								CuryDocBal = 0m;
								i++;
							}


						}

						foreach (SOAdjust soAdjustRow in graphSOOrderEntry.Adjustments.Select())
						{
							if (soAdjustRow.CuryAdjdAmt == 0)
							{
								graphSOOrderEntry.Adjustments.Delete(soAdjustRow);
							}
						}

						graphSOOrderEntry.Save.Press();
					}
				}
				else if (fsPostBatchRow != null && fsPostBatchRow.PostTo == ID.Batch_PostTo.SI)
				{
					SOInvoiceEntry graphSOInvoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();

					var results = PXSelectJoin<PostingBatchDetail,
								  InnerJoin<FSAdjust,
								  On<
									  PostingBatchDetail.sORefNbr, Equal<FSAdjust.adjdOrderNbr>,
									  And<PostingBatchDetail.srvOrdType, Equal<FSAdjust.adjdOrderType>>>>,
								  Where<
									  PostingBatchDetail.batchID, Equal<Required<FSPostBatch.batchID>>>>
								  .Select(graphSOInvoiceEntry, fsPostBatchRow.BatchID);

					foreach (PXResult<PostingBatchDetail, FSAdjust> result in results)
					{
						graphSOInvoiceEntry.Clear();

						PostingBatchDetail postingBatchDetailRow = (PostingBatchDetail)result;
						FSAdjust fsAdjustRow = (FSAdjust)result;

						if (postingBatchDetailRow.SOInvDocType != ARInvoiceType.Invoice)
						{
							continue;
						}

						ARInvoice arInvoiceRow = graphSOInvoiceEntry.Document.Current = graphSOInvoiceEntry.Document.Search<ARInvoice.refNbr>(postingBatchDetailRow.SOInvRefNbr, postingBatchDetailRow.SOInvDocType);

						graphSOInvoiceEntry.LoadDocumentsProc();

						SharedClasses.SOPrepaymentHelper SOPrepaymentApplication = new SharedClasses.SOPrepaymentHelper();

						PXResultset<ARTax> arTaxRows = graphSOInvoiceEntry.Tax_Rows.Select();

						foreach (PXResult<ARTran, FSARTran> details in PXSelectJoin<ARTran,
																			LeftJoin<FSARTran,
																				On<FSARTran.FK.ARTranLine>>,
																			Where<FSARTran.tranType, Equal<Required<ARTran.tranType>>,
																			And<FSARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>
																			.Select(graphSOInvoiceEntry, arInvoiceRow.DocType, arInvoiceRow.RefNbr))
						{
							ARTran arTranRow = (ARTran)details;
							FSARTran fsARTranRow = (FSARTran)details;
							ARTax arTaxRow = arTaxRows.RowCast<ARTax>().Where(_ => _.LineNbr == arTranRow.LineNbr).FirstOrDefault();
							decimal arTaxLine = (arTaxRow != null) ? (arTaxRow.CuryTaxableAmt * arTaxRow.TaxRate / 100) ?? 0m : 0m;

							SOPrepaymentApplication.Add(arTranRow, fsARTranRow, arTaxLine);
						}

						decimal CuryDocBal = 0m;

						foreach (SharedClasses.SOPrepaymentBySO row in SOPrepaymentApplication.SOPrepaymentList)
						{
							PXResultset<ARPayment> PaymentList = row.GetPrepaymentBySO(graphSOInvoiceEntry);
							int i = 0;

							while (PaymentList != null && i < PaymentList.Count && row.unpaidAmount > 0)
							{
								ARPayment arPaymentRow = (ARPayment)PaymentList[i];

								if (string.Equals(arPaymentRow.CuryID, arInvoiceRow.CuryID) == true)
								{
									ARAdjust2 arAdjust2Row = graphSOInvoiceEntry.Adjustments.Select().Where(x => ((ARAdjust2)x).AdjgRefNbr == arPaymentRow.RefNbr).FirstOrDefault();

									CuryDocBal = arAdjust2Row.CuryDocBal ?? 0m;
									decimal? adjdAmt = 0;
									if (CuryDocBal > 0)
									{
										if (row.unpaidAmount > CuryDocBal)
										{
											adjdAmt = CuryDocBal;
											row.unpaidAmount = row.unpaidAmount - CuryDocBal;
											CuryDocBal = 0;
										}
										else
										{
											adjdAmt = row.unpaidAmount;
											CuryDocBal = CuryDocBal - row.unpaidAmount ?? 0;
											row.unpaidAmount = 0;
										}
										graphSOInvoiceEntry.Adjustments.SetValueExt<ARAdjust2.curyAdjdAmt>(arAdjust2Row, adjdAmt);
									}

									foreach (ARTaxTran tax in graphSOInvoiceEntry.Taxes.Select())
									{
										if (CuryDocBal > 0)
										{
											if (tax.CuryTaxAmt > CuryDocBal)
											{
												adjdAmt += CuryDocBal;
												break;
											}
											else
											{
												adjdAmt += tax.CuryTaxAmt;
												CuryDocBal = CuryDocBal - tax.CuryTaxAmt ?? 0;
											}
											graphSOInvoiceEntry.Adjustments.SetValueExt<ARAdjust2.curyAdjdAmt>(arAdjust2Row, adjdAmt);
										}
									}
									graphSOInvoiceEntry.Adjustments.Update(arAdjust2Row);
								}

								CuryDocBal = 0m;
								i++;
							}
						}

						graphSOInvoiceEntry.Save.Press();
					}
				}
			}
			catch (Exception)
			{

			}
        }

        public virtual void ApplyPrepayments(PostBatchShared postBatchShared)
        {
            ApplyPrepayments(postBatchShared.FSPostBatchRow);
        }

        public virtual void CompletePostingBatch(PostBatchShared postBatchShared, int documentsQty)
        {
            postBatchShared.PostBatchEntryGraph.CompletePostingBatch(postBatchShared.FSPostBatchRow, documentsQty);
        }

		public virtual int CreateInvoiceDocument(PostBatchShared postBatchShared, string targetScreen, Guid currentProcessID, int billingCycleID, string groupKey, short? invtMult, List<DocLineExt> docLines, string billingBy, PXQuickProcess.ActionFlow quickProcessFlow)
		{
			var processShared = new InvoicingProcessStepGroupShared();
			processShared.Initialize(targetScreen, billingBy);
			processShared.InvoiceGraph.IsInvoiceProcessRunning = true;
			OnTransactionInsertedDelegate onTransactionInserted = processShared.ProcessGraph.OnTransactionInserted;

			int documentsQty = 0;

			FSCreatedDoc fsCreatedDocRow = null;
			FSCreatedDoc fsINCreatedDocRow = null;
			docLines = docLines.OrderBy(x => x.docLine.SrvOrdType).ThenBy(x => x.docLine.RefNbr).ThenBy(x => x.docLine.SortOrder).ToList();
			IInvoiceGraph inInvoiceGraph = null;
			bool hasInventoryItemInPM = postBatchShared.FSPostBatchRow.PostTo == ID.Batch_PostTo.PM && docLines.Where(x => x.docLine.LineType == ID.LineType_ALL.INVENTORY_ITEM).Count() > 0;


			processShared.InvoiceGraph.CreateInvoice(processShared.ProcessGraph.GetGraph(), docLines, invtMult ?? 0, postBatchShared.FSPostBatchRow.InvoiceDate, postBatchShared.FSPostBatchRow.FinPeriodID, processShared.ProcessGraph.OnDocumentHeaderInserted, onTransactionInserted, quickProcessFlow);

			inInvoiceGraph = (hasInventoryItemInPM == true) ? CreateInvoiceGraph(ID.Batch_PostTo.IN) : null;

			if (inInvoiceGraph != null)
			{
				inInvoiceGraph.CreateInvoice(processShared.ProcessGraph.GetGraph(), docLines, invtMult ?? 0, postBatchShared.FSPostBatchRow.InvoiceDate, postBatchShared.FSPostBatchRow.FinPeriodID, processShared.ProcessGraph.OnDocumentHeaderInserted, onTransactionInserted, quickProcessFlow);
			}

			DeallocateItemsThatAreBeingPosted(processShared.ServiceOrderGraph, docLines, processShared.ProcessGraph is CreateInvoiceByAppointmentPost);

			if (processShared.InvoiceGraph.GetGraph() is SOInvoiceEntry)
			{
				SOInvoiceEntry soInvoiceGraph = processShared.InvoiceGraph.GetGraph() as SOInvoiceEntry;

				foreach (ARTran currentRow in soInvoiceGraph.Transactions.Select())
				{
					if (currentRow.UnassignedQty > 0 && string.IsNullOrEmpty(currentRow.LotSerialNbr) == false)
					{
						var copyRow = (ARTran)soInvoiceGraph.Transactions.Cache.CreateCopy(currentRow);

						soInvoiceGraph.Transactions.Cache.RaiseFieldUpdated<ARTran.qty>(currentRow, copyRow.Qty);
						soInvoiceGraph.Transactions.Cache.RaiseRowUpdated(currentRow, copyRow);
					}
				}
			}

			if (inInvoiceGraph != null)
			{
				fsINCreatedDocRow = inInvoiceGraph.PressSave((int)postBatchShared.FSPostBatchRow.BatchID, docLines, processShared.ProcessGraph.BeforeSave);
			}

			fsCreatedDocRow = processShared.InvoiceGraph.PressSave((int)postBatchShared.FSPostBatchRow.BatchID, docLines, processShared.ProcessGraph.BeforeSave);

			processShared.CacheFSCreatedDoc.Insert(fsCreatedDocRow);
			processShared.CacheFSCreatedDoc.Persist(PXDBOperation.Insert);

			if (inInvoiceGraph != null)
			{
				processShared.CacheFSCreatedDoc.Insert(fsINCreatedDocRow);
				processShared.CacheFSCreatedDoc.Persist(PXDBOperation.Insert);
			}

			PXGraph graph = processShared.ProcessGraph.GetGraph();
			UpdateFSPostDoc(graph, fsCreatedDocRow, currentProcessID, billingCycleID, groupKey);

			List<DocLineExt> docs = docLines.GroupBy(r => r.docLine.DocID).Select(g => g.First()).ToList();
			CreatePostRegisterAndBillHistory(graph, docs, fsCreatedDocRow, currentProcessID);
			if (fsINCreatedDocRow != null)
			{
				CreatePostRegisterAndBillHistory(graph, docs, fsINCreatedDocRow, currentProcessID);
				CreateNewPostDocs(graph, docs, fsINCreatedDocRow, currentProcessID);
			}

			if (processShared.ProcessGraph.AfterCreateInvoice != null)
			{
				processShared.ProcessGraph.AfterCreateInvoice(processShared.InvoiceGraph.GetGraph(), fsCreatedDocRow);
			}

			UpdatePostInfoAndPostDet(processShared.ServiceOrderGraph, docLines, postBatchShared.FSPostBatchRow, processShared.PostInfoEntryGraph, processShared.CacheFSPostDet, fsCreatedDocRow, fsINCreatedDocRow);

			documentsQty = docLines.GroupBy(y => y.docLine.DocID).Count();

			processShared.InvoiceGraph.IsInvoiceProcessRunning = false;
			processShared.InvoiceGraph.Clear();
			processShared.CacheFSCreatedDoc.Clear();

			if (inInvoiceGraph != null)
			{
				inInvoiceGraph.Clear();
			}

			processShared.Clear();
			return documentsQty;
		}

		protected virtual Guid CreatePostDocsFromUserSelection(List<TPostLine> postLineRows)
        {
            Guid currentProcessID = Guid.NewGuid();
            int rowIndex = 0;
            var fsPostDoc = new FSPostDoc();
            string screenID = this.Accessinfo.ScreenID.Replace(".", string.Empty);

            foreach (TPostLine postLineRow in postLineRows)
            {
                if (postLineRow != null)
                {
                    fsPostDoc.ProcessID = currentProcessID;
                    fsPostDoc.BillingCycleID = postLineRow.BillingCycleID;
                    fsPostDoc.GroupKey = GetGroupKey(postLineRow);
                    fsPostDoc.SOID = postLineRow.SOID;
                    fsPostDoc.AppointmentID = postLineRow.AppointmentID;
                    fsPostDoc.RowIndex = rowIndex;
                    fsPostDoc.PostNegBalanceToAP = postLineRow.PostNegBalanceToAP;

                    fsPostDoc.PostOrderType = postLineRow.PostOrderType;
                    fsPostDoc.PostOrderTypeNegativeBalance = postLineRow.PostOrderTypeNegativeBalance;

                    postLineRow.RowIndex = fsPostDoc.RowIndex;
                    postLineRow.GroupKey = fsPostDoc.GroupKey;
                    fsPostDoc.EntityType = postLineRow.EntityType;

                    rowIndex++;

                    PXDatabase.Insert<FSPostDoc>(
                            new PXDataFieldAssign<FSPostDoc.processID>(fsPostDoc.ProcessID),
                            new PXDataFieldAssign<FSPostDoc.billingCycleID>(fsPostDoc.BillingCycleID),
                            new PXDataFieldAssign<FSPostDoc.groupKey>(fsPostDoc.GroupKey),
                            new PXDataFieldAssign<FSPostDoc.entityType>(fsPostDoc.EntityType),
                            new PXDataFieldAssign<FSPostDoc.sOID>(fsPostDoc.SOID),
                            new PXDataFieldAssign<FSPostDoc.appointmentID>(fsPostDoc.AppointmentID),
                            new PXDataFieldAssign<FSPostDoc.rowIndex>(fsPostDoc.RowIndex),
                            new PXDataFieldAssign<FSPostDoc.postNegBalanceToAP>(fsPostDoc.PostNegBalanceToAP),
                            new PXDataFieldAssign<FSPostDoc.postOrderType>(fsPostDoc.PostOrderType),
                            new PXDataFieldAssign<FSPostDoc.postOrderTypeNegativeBalance>(fsPostDoc.PostOrderTypeNegativeBalance),
                            new PXDataFieldAssign<FSPostDoc.createdByID>(this.Accessinfo.UserID),
                            new PXDataFieldAssign<FSPostDoc.createdByScreenID>(screenID),
                            new PXDataFieldAssign<FSPostDoc.createdDateTime>(DateTime.Now));
                }
            }

            return currentProcessID;
        }

        protected virtual void DeletePostDocsWithError(Guid currentProcessID)
        {
            PXDatabase.Delete<FSPostDoc>(
                new PXDataFieldRestrict<FSPostDoc.batchID>(PXDbType.Int, 4, null, PXComp.ISNULL),
                new PXDataFieldRestrict<FSPostDoc.processID>(currentProcessID));

            PXDatabase.Delete<FSPostDoc>(
                new PXDataFieldRestrict<FSPostDoc.batchID>(PXDbType.Int, 4, null, PXComp.ISNULL),
                new PXDataFieldRestrict<FSPostDoc.createdDateTime>(PXDbType.DateTime, 8, DateTime.Now.AddDays(-3), PXComp.LE));
        }

        protected virtual void CalculateExternalTaxes(Guid currentProcessID)
        {
            PXResultset<FSPostDoc> fsPostDocRows = PXSelectGroupBy<FSPostDoc,
                                                   Where<
                                                       FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>>,
                                                   Aggregate<
                                                       GroupBy<FSPostDoc.postedTO,
                                                       GroupBy<FSPostDoc.postDocType,
                                                       GroupBy<FSPostDoc.postRefNbr>>>>>
                                                   .Select(this, currentProcessID);

            SOOrderEntry graphSOOrderEntry = null;
            ARInvoiceEntry graphARInvoiceEntry = null;
            APInvoiceEntry graphAPInvoiceEntry = null;
            bool forceInstanciateGraph = false;

            foreach (FSPostDoc fsPostDoc in fsPostDocRows)
            {
                if (fsPostDoc.PostedTO == ID.Batch_PostTo.SO)
                {
                    if (graphSOOrderEntry == null || forceInstanciateGraph == true)
                    {
                        graphSOOrderEntry = (SOOrderEntry)CreateInvoiceGraph(fsPostDoc.PostedTO).GetGraph();
                        forceInstanciateGraph = false;
                    }

                    SOOrder soOrderRow = graphSOOrderEntry.Document.Current = graphSOOrderEntry.Document.Search<SOOrder.orderNbr>(fsPostDoc.PostRefNbr, fsPostDoc.PostDocType);

                    if (soOrderRow != null && soOrderRow.IsTaxValid == false && graphSOOrderEntry.IsExternalTax(soOrderRow.TaxZoneID) == true)
                    {
                        graphSOOrderEntry.Document.Update(graphSOOrderEntry.Document.Current);

                        try
                        {
                            graphSOOrderEntry.Save.Press();
                        }
                        catch(Exception e)
                        {
                            PXTrace.WriteError("Error trying to calculate external taxes for the Sales Order {0}-{1} with the message: {2}",
                                                soOrderRow.OrderType, soOrderRow.RefNbr, e.Message);
                            graphSOOrderEntry.Clear(PXClearOption.ClearAll);
                            forceInstanciateGraph = true;
                        }
                    }
                }
                else if (fsPostDoc.PostedTO == ID.Batch_PostTo.AR)
                {
                    if (graphARInvoiceEntry == null || forceInstanciateGraph == true)
                    {
                        graphARInvoiceEntry = (ARInvoiceEntry)CreateInvoiceGraph(fsPostDoc.PostedTO).GetGraph();
                        forceInstanciateGraph = false;
                    }

                    ARInvoice arInvoiceRow = graphARInvoiceEntry.Document.Current = graphARInvoiceEntry.Document.Search<ARInvoice.refNbr>(fsPostDoc.PostRefNbr, fsPostDoc.PostDocType);

                    if (arInvoiceRow != null && arInvoiceRow.IsTaxValid == false && graphARInvoiceEntry.IsExternalTax(arInvoiceRow.TaxZoneID) == true)
                    {
                        graphARInvoiceEntry.Document.Update(graphARInvoiceEntry.Document.Current);

                        try
                        {
                            graphARInvoiceEntry.Save.Press();
                        }
                        catch (Exception e)
                        {
                            PXTrace.WriteError("Error trying to calculate external taxes for the AR Invoice {0}-{1} with the message: {2}",
                                                arInvoiceRow.DocType, arInvoiceRow.RefNbr, e.Message);
                            graphARInvoiceEntry.Clear(PXClearOption.ClearAll);
                            forceInstanciateGraph = true;
                        }
                    }
                }
                else if (fsPostDoc.PostedTO == ID.Batch_PostTo.AP)
                {
                    if (graphAPInvoiceEntry == null || forceInstanciateGraph == true)
                    {
                        graphAPInvoiceEntry = (APInvoiceEntry)CreateInvoiceGraph(fsPostDoc.PostedTO).GetGraph();
                        forceInstanciateGraph = false;
                    }

                    APInvoice apInvoiceRow = graphAPInvoiceEntry.Document.Current = graphAPInvoiceEntry.Document.Search<APInvoice.refNbr>(fsPostDoc.PostRefNbr, fsPostDoc.PostDocType);
                    if (apInvoiceRow != null && apInvoiceRow.IsTaxValid == false && graphAPInvoiceEntry.IsExternalTax(apInvoiceRow.TaxZoneID) == true)
                    {
                        graphAPInvoiceEntry.Document.Update(graphAPInvoiceEntry.Document.Current);
                        try
                        {
                            graphAPInvoiceEntry.Save.Press();
                        }
                        catch (Exception e)
                        {
                            PXTrace.WriteError("Error trying to calculate external taxes for the AP Bill {0}-{1} with the message: {2}",
                                                apInvoiceRow.DocType, apInvoiceRow.RefNbr, e.Message);
                            graphAPInvoiceEntry.Clear(PXClearOption.ClearAll);
                            forceInstanciateGraph = true;
                        }
                    }
                }
            }
        }

		protected virtual string GetGroupKey(TPostLine postLineRow)
		{
			string result = postLineRow.PostTo != ID.SrvOrdType_PostTo.PROJECTS ? GetNonProjectGroupKey(postLineRow) : GetProjectGroupKey(postLineRow);
			return result;
		}

		protected virtual string GetNonProjectGroupKey(TPostLine postLineRow)
        {
            if (groupKey == null)
            {
                groupKey = new StringBuilder();
            }
            else
            {
                groupKey.Clear();
            }

            groupKey.Append(postLineRow.BranchID.ToString()
                            + "|" + postLineRow.BillCustomerID.ToString()
                            + "|" + postLineRow.CuryID.ToString()
                            + "|" + (postLineRow.TaxZoneID == null ? "" : postLineRow.TaxZoneID.ToString())
                            + "[" + (postLineRow.BillingCycleType == null ? string.Empty : postLineRow.BillingCycleType.ToString()) + "]");

            if (postLineRow.ProjectID != null
                    && ProjectDefaultAttribute.IsNonProject(postLineRow.ProjectID) == false)
            {
                groupKey.Append(postLineRow.ProjectID.ToString() + "|");
            }

            string billLocationID = postLineRow.GroupBillByLocations == true ? postLineRow.BillLocationID.ToString() : string.Empty;

            if (postLineRow.BillingCycleType == ID.Billing_Cycle_Type.APPOINTMENT)
            {
                groupKey.Append(postLineRow.AppointmentID.ToString());
            }
            else if (postLineRow.BillingCycleType == ID.Billing_Cycle_Type.SERVICE_ORDER)
            {
                groupKey.Append(postLineRow.SOID.ToString());
            }
            else if (postLineRow.BillingCycleType == ID.Billing_Cycle_Type.TIME_FRAME)
            {
                groupKey.Append(billLocationID);
            }
            else if (postLineRow.BillingCycleType == ID.Billing_Cycle_Type.PURCHASE_ORDER)
            {
                string custPORefNbr = postLineRow.CustPORefNbr == null ? string.Empty : postLineRow.CustPORefNbr.Trim();
                groupKey.Append(custPORefNbr + "|" + billLocationID);
            }
            else if (postLineRow.BillingCycleType == ID.Billing_Cycle_Type.WORK_ORDER)
            {
                string custWorkOrderRefNbr = postLineRow.CustWorkOrderRefNbr == null ? string.Empty : postLineRow.CustWorkOrderRefNbr.Trim();
                groupKey.Append(custWorkOrderRefNbr + "|" + billLocationID);
            }
            else
            {
                throw new PXException(TX.Error.BILLING_CYCLE_TYPE_NOT_VALID);
            }

            return groupKey.ToString();
        }

        protected virtual string GetProjectGroupKey(TPostLine postLineRow)
        {
            if (groupKey == null)
            {
                groupKey = new StringBuilder();
            }
            else
            {
                groupKey.Clear();
            }

            groupKey.Append(postLineRow.BranchID.ToString()
                            + "|" + postLineRow.DocType.ToString()
                            + "|" + postLineRow.SOID.ToString()
                            + "|" + postLineRow.AppointmentID.ToString());

            return groupKey.ToString();
        }

        protected virtual void CreatePostingBatchForBillingCycle(Guid currentProcessID, int billingCycleID, CreateInvoiceFilter filter, List<TPostLine> postLineRows, PXQuickProcess.ActionFlow quickProcessFlow, bool isGenerateInvoiceScreen)
        {
            PXResultset<FSPostDoc> billingCycleOptionsGroups =
                                PXSelectGroupBy<FSPostDoc,
                                Where<
                                    FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>,
                                    And<FSPostDoc.billingCycleID, Equal<Required<FSPostDoc.billingCycleID>>>>,
                                Aggregate<
                                    GroupBy<FSPostDoc.groupKey>>,
                                OrderBy<
                                    Asc<FSPostDoc.groupKey>>>
                                .Select(this, currentProcessID, billingCycleID);

            if (filter.PostTo == ID.Batch_PostTo.AR_AP)
            {
				var arInvoiceList = new List<FSPostDoc>();
				var apInvoiceList = new List<FSPostDoc>();
				decimal? invoiceTotal = 0;

				foreach (FSPostDoc billingCycleOptionsGroup in billingCycleOptionsGroups)
				{
					GetInvoiceLines(currentProcessID, billingCycleID, billingCycleOptionsGroup.GroupKey, true, out invoiceTotal, filter.PostTo);

					if (invoiceTotal < 0 && billingCycleOptionsGroup.PostNegBalanceToAP == true)
					{
						billingCycleOptionsGroup.InvtMult = -1;
						apInvoiceList.Add(billingCycleOptionsGroup);
					}
					else
					{
						if (invoiceTotal < 0)
						{
							billingCycleOptionsGroup.InvtMult = -1;
						}
						else
						{
							billingCycleOptionsGroup.InvtMult = 1;
						}

						arInvoiceList.Add(billingCycleOptionsGroup);
					}
				}

				if (arInvoiceList.Count > 0)
				{
					CreatePostingBatchAndInvoices(currentProcessID, billingCycleID, filter.UpToDate, filter.InvoiceDate, filter.InvoiceFinPeriodID, ID.Batch_PostTo.AR, arInvoiceList, postLineRows, quickProcessFlow, isGenerateInvoiceScreen);
					arInvoiceList.Clear();
				}

				if (apInvoiceList.Count > 0)
				{
					CreatePostingBatchAndInvoices(currentProcessID, billingCycleID, filter.UpToDate, filter.InvoiceDate, filter.InvoiceFinPeriodID, ID.Batch_PostTo.AP, apInvoiceList, postLineRows, quickProcessFlow, isGenerateInvoiceScreen);
					apInvoiceList.Clear();
				}
			}
            else if ((filter.PostTo == ID.Batch_PostTo.SO || filter.PostTo == ID.Batch_PostTo.SI)
                        && PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
            {
                var soInvoiceList = new List<FSPostDoc>();
                decimal? invoiceTotal = 0;

                foreach (FSPostDoc billingCycleOptionsGroup in billingCycleOptionsGroups)
                {
                    GetInvoiceLines(currentProcessID, billingCycleID, billingCycleOptionsGroup.GroupKey, true, out invoiceTotal, filter.PostTo);

                    if (invoiceTotal < 0)
                    {
                        billingCycleOptionsGroup.InvtMult = -1;
                    }
                    else
                    {
                        billingCycleOptionsGroup.InvtMult = 1;
                    }

                    soInvoiceList.Add(billingCycleOptionsGroup);
                }

				CreatePostingBatchAndInvoices(currentProcessID, billingCycleID, filter.UpToDate, filter.InvoiceDate, filter.InvoiceFinPeriodID, filter.PostTo, soInvoiceList, postLineRows, quickProcessFlow, isGenerateInvoiceScreen);
            }
            else if (filter.PostTo == ID.Batch_PostTo.PM) 
            {
                var pmInvoiceList = new List<FSPostDoc>();
                decimal? invoiceTotal = 0;

                foreach (FSPostDoc billingCycleOptionsGroup in billingCycleOptionsGroups)
                {
                    GetInvoiceLines(currentProcessID, billingCycleID, billingCycleOptionsGroup.GroupKey, true, out invoiceTotal, filter.PostTo);

                    if (invoiceTotal < 0)
                    {
                        billingCycleOptionsGroup.InvtMult = -1;
                    }
                    else
                    {
                        billingCycleOptionsGroup.InvtMult = 1;
                    }

                    pmInvoiceList.Add(billingCycleOptionsGroup);
                }

                CreatePostingBatchAndInvoices(currentProcessID, billingCycleID, filter.UpToDate, filter.InvoiceDate, filter.InvoiceFinPeriodID, filter.PostTo, pmInvoiceList, postLineRows, quickProcessFlow, isGenerateInvoiceScreen);
            }
        }

        public abstract List<DocLineExt> GetInvoiceLines(Guid currentProcessID, int billingCycleID, string groupKey, bool getOnlyTotal, out decimal? invoiceTotal, string postTo);

        public static void UpdateFSPostDoc(PXGraph graph, FSCreatedDoc fsCreatedDocRow, Guid currentProcessID, int? billingCycleID, string groupKey)
        {
            PXUpdate<
                Set<FSPostDoc.batchID, Required<FSPostDoc.batchID>,
                Set<FSPostDoc.postedTO, Required<FSPostDoc.postedTO>,
                Set<FSPostDoc.postDocType, Required<FSPostDoc.postDocType>,
                Set<FSPostDoc.postRefNbr, Required<FSPostDoc.postRefNbr>>>>>,
            FSPostDoc,
            Where<
                FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>,
                And<FSPostDoc.billingCycleID, Equal<Required<FSPostDoc.billingCycleID>>,
                And<FSPostDoc.groupKey, Equal<Required<FSPostDoc.groupKey>>>>>>
            .Update(graph,
                    fsCreatedDocRow.BatchID,
                    fsCreatedDocRow.PostTo,
                    fsCreatedDocRow.CreatedDocType,
                    fsCreatedDocRow.CreatedRefNbr,
                    currentProcessID,
                    billingCycleID,
                    groupKey);
        }

        public static void CreatePostRegisterAndBillHistory(PXGraph graph, List<DocLineExt> docs, FSCreatedDoc fsCreatedDocRow, Guid currentProcessID)
        {
            PXCache cacheFSPostRegister = graph.Caches[typeof(FSPostRegister)];
            PXCache cacheFSBillHistory =  graph.Caches[typeof(FSBillHistory)];

            foreach (var row in docs)
            {
                FSPostRegister fsPostRegisterRow = new FSPostRegister();

                fsPostRegisterRow.SrvOrdType = row.fsAppointment == null ? row.fsServiceOrder.SrvOrdType : row.fsAppointment.SrvOrdType;
                fsPostRegisterRow.RefNbr = row.fsAppointment == null ? row.fsServiceOrder.RefNbr : row.fsAppointment.RefNbr;
                fsPostRegisterRow.Type = ID.PostRegister_Type.BillingProcess;
                fsPostRegisterRow.BatchID = fsCreatedDocRow.BatchID;
                fsPostRegisterRow.EntityType = row.fsAppointment == null ? ID.PostDoc_EntityType.SERVICE_ORDER : ID.PostDoc_EntityType.APPOINTMENT;
                fsPostRegisterRow.ProcessID = currentProcessID;
                fsPostRegisterRow.PostedTO = fsCreatedDocRow.PostTo;
                fsPostRegisterRow.PostDocType = fsCreatedDocRow.CreatedDocType;
                fsPostRegisterRow.PostRefNbr = fsCreatedDocRow.CreatedRefNbr;

                cacheFSPostRegister.Insert(fsPostRegisterRow);

                FSBillHistory fsBillHistoryRow = new FSBillHistory();

                fsBillHistoryRow.BatchID = fsCreatedDocRow.BatchID;
                fsBillHistoryRow.SrvOrdType = fsPostRegisterRow.SrvOrdType;
                fsBillHistoryRow.ServiceOrderRefNbr = row.fsAppointment == null ? row.fsServiceOrder.RefNbr : row.fsAppointment.SORefNbr;
                fsBillHistoryRow.AppointmentRefNbr = row.fsAppointment != null ? row.fsAppointment.RefNbr : null;

                if (fsCreatedDocRow.PostTo == ID.Batch_PostTo.SO)
                {
                    fsBillHistoryRow.ChildEntityType = FSEntityType.SalesOrder;
                }
                else if (fsCreatedDocRow.PostTo == ID.Batch_PostTo.SI)
                {
                    fsBillHistoryRow.ChildEntityType = FSEntityType.SOInvoice;
                }
                else if (fsCreatedDocRow.PostTo == ID.Batch_PostTo.AR)
                {
                    fsBillHistoryRow.ChildEntityType = FSEntityType.ARInvoice;
                }
                else if (fsCreatedDocRow.PostTo == ID.Batch_PostTo.AP)
                {
                    fsBillHistoryRow.ChildEntityType = FSEntityType.APInvoice;
                }
                else if (fsCreatedDocRow.PostTo == ID.Batch_PostTo.PM)
                {
                    fsBillHistoryRow.ChildEntityType = FSEntityType.PMRegister;
                }
                else if (fsCreatedDocRow.PostTo == ID.Batch_PostTo.IN)
                {
                    fsBillHistoryRow.ChildEntityType = FSEntityType.INIssue;
                }
                else
                {
                    throw new NotImplementedException();
                }

                fsBillHistoryRow.ChildDocType = fsCreatedDocRow.CreatedDocType;
                fsBillHistoryRow.ChildRefNbr = fsCreatedDocRow.CreatedRefNbr;

                cacheFSBillHistory.Insert(fsBillHistoryRow);
            }

            cacheFSPostRegister.Persist(PXDBOperation.Insert);
            cacheFSBillHistory.Persist(PXDBOperation.Insert);
        }

        public static void CreateNewPostDocs(PXGraph graph, List<DocLineExt> docs, FSCreatedDoc createdDoc, Guid currentProcessID)
        {
            PXCache postDocCache = graph.Caches[typeof(FSPostDoc)];

            foreach (var row in docs)
            {
                FSPostDoc original = row.fsPostDoc;

                FSPostDoc postDoc = new FSPostDoc()
                {
                    AppointmentID = row.fsAppointment?.AppointmentID,
                    BatchID = createdDoc.BatchID,
                    BillingCycleID = original.BillingCycleID,
                    DocLineRef = original.DocLineRef,
                    EntityType = original.EntityType,
                    GroupKey = original.GroupKey,
                    INDocLineRef = original.INDocLineRef,
                    InvtMult = original.InvtMult,
                    PostDocType = createdDoc.CreatedDocType,
                    PostedTO = createdDoc.PostTo,
                    PostNegBalanceToAP = null,
                    PostOrderType = null,
                    PostOrderTypeNegativeBalance = null,
                    PostRefNbr = createdDoc.CreatedRefNbr,
                    ProcessID = currentProcessID,
                    RowIndex = original.RowIndex,
                    SOID = row.fsServiceOrder?.SOID,
                };

                postDocCache.Insert(postDoc);
            }

            postDocCache.Persist(PXDBOperation.Insert);
        }

        public virtual void UpdatePostInfoAndPostDet(ServiceOrderEntry soGraph, List<DocLineExt> docLinesWithPostInfo, FSPostBatch fsPostBatchRow, PostInfoEntry graphPostInfoEntry, PXCache<FSPostDet> cacheFSPostDet, FSCreatedDoc fsCreatedDocRow, FSCreatedDoc fsINCreatedDocRow = null)
        {
            IDocLine docLine = null;
            FSPostDoc fsPostDocRow = null;
            FSPostInfo fsPostInfoRow = null;
            FSPostDet fsPostDetRow = null;
            FSPostDet postDet2 = null;
            bool insertingPostInfo;

            SOLine soLineRow = null;
            ARTran arTranRow = null;
            APTran apTranRow = null;
            PMTran pmTranRow = null;
            INTran inTranRow = null;

            AppointmentEntry apptGraph = PXGraph.CreateInstance<AppointmentEntry>();

            foreach (DocLineExt docLineExt in docLinesWithPostInfo)
            {
                docLine = docLineExt.docLine;
                fsPostDocRow = docLineExt.fsPostDoc;
                fsPostInfoRow = docLineExt.fsPostInfo;

                fsPostDetRow = new FSPostDet();
                postDet2 = null;

                if (fsPostInfoRow == null || fsPostInfoRow.PostID == null)
                {
                    fsPostInfoRow = new FSPostInfo();
                    insertingPostInfo = true;
                }
                else
                {
                    insertingPostInfo = false;
                }

                if (fsPostDocRow.DocLineRef is SOLine)
                {
                    soLineRow = (SOLine)fsPostDocRow.DocLineRef;
                    fsPostInfoRow.SOPosted = true;

                    if (fsCreatedDocRow == null)
                    {
                        fsPostInfoRow.SOOrderType = soLineRow.OrderType;
                        fsPostInfoRow.SOOrderNbr = soLineRow.OrderNbr;
                    }
                    else
                    {
                        fsPostInfoRow.SOOrderType = fsCreatedDocRow.CreatedDocType;
                        fsPostInfoRow.SOOrderNbr = fsCreatedDocRow.CreatedRefNbr;
                    }

                    fsPostInfoRow.SOLineNbr = soLineRow.LineNbr;

                    fsPostDetRow.SOPosted = fsPostInfoRow.SOPosted;
                    fsPostDetRow.SOOrderType = fsPostInfoRow.SOOrderType;
                    fsPostDetRow.SOOrderNbr = fsPostInfoRow.SOOrderNbr;
                    fsPostDetRow.SOLineNbr = fsPostInfoRow.SOLineNbr;
                }
                else if (fsPostDocRow.DocLineRef is ARTran
                            && (fsPostBatchRow.PostTo == ID.Batch_PostTo.AR_AP || fsPostBatchRow.PostTo == ID.Batch_PostTo.AR))
                {
                    arTranRow = (ARTran)fsPostDocRow.DocLineRef;

                    fsPostInfoRow.ARPosted = true;

                    if (fsCreatedDocRow == null)
                    {
                        fsPostInfoRow.ARDocType = arTranRow.TranType;
                        fsPostInfoRow.ARRefNbr = arTranRow.RefNbr;
                    }
                    else
                    {
                        fsPostInfoRow.ARDocType = fsCreatedDocRow.CreatedDocType;
                        fsPostInfoRow.ARRefNbr = fsCreatedDocRow.CreatedRefNbr;
                    }

                    fsPostInfoRow.ARLineNbr = arTranRow.LineNbr;

                    fsPostDetRow.ARPosted = fsPostInfoRow.ARPosted;
                    fsPostDetRow.ARDocType = fsPostInfoRow.ARDocType;
                    fsPostDetRow.ARRefNbr = fsPostInfoRow.ARRefNbr;
                    fsPostDetRow.ARLineNbr = fsPostInfoRow.ARLineNbr;
                }
                else if (fsPostDocRow.DocLineRef is ARTran
                            && fsPostBatchRow.PostTo == ID.Batch_PostTo.SI)
                {
                    arTranRow = (ARTran)fsPostDocRow.DocLineRef;

                    fsPostInfoRow.SOInvPosted = true;
                    fsPostInfoRow.SOInvDocType = arTranRow.TranType;
                    fsPostInfoRow.SOInvRefNbr = arTranRow.RefNbr;
                    fsPostInfoRow.SOInvLineNbr = arTranRow.LineNbr;

                    fsPostDetRow.SOInvPosted = fsPostInfoRow.SOInvPosted;
                    fsPostDetRow.SOInvDocType = fsPostInfoRow.SOInvDocType;
                    fsPostDetRow.SOInvRefNbr = fsPostInfoRow.SOInvRefNbr;
                    fsPostDetRow.SOInvLineNbr = fsPostInfoRow.SOInvLineNbr;
                }
                else if (fsPostDocRow.DocLineRef is APTran)
                {
                    apTranRow = (APTran)fsPostDocRow.DocLineRef;

                    fsPostInfoRow.APPosted = true;

                    if (fsCreatedDocRow == null)
                    {
                        fsPostInfoRow.APDocType = apTranRow.TranType;
                        fsPostInfoRow.APRefNbr = apTranRow.RefNbr;
                    }
                    else
                    {
                        fsPostInfoRow.APDocType = fsCreatedDocRow.CreatedDocType;
                        fsPostInfoRow.APRefNbr = fsCreatedDocRow.CreatedRefNbr;
                    }

                    fsPostInfoRow.APLineNbr = apTranRow.LineNbr;

                    fsPostDetRow.APPosted = fsPostInfoRow.APPosted;
                    fsPostDetRow.APDocType = fsPostInfoRow.APDocType;
                    fsPostDetRow.APRefNbr = fsPostInfoRow.APRefNbr;
                    fsPostDetRow.APLineNbr = fsPostInfoRow.APLineNbr;
                }
                else if (fsPostDocRow.DocLineRef is PMTran)
                {
                    pmTranRow = (PMTran)fsPostDocRow.DocLineRef;

                    fsPostInfoRow.PMPosted = true;

                    if (fsCreatedDocRow == null)
                    {
                        fsPostInfoRow.PMDocType = pmTranRow.TranType;
                        fsPostInfoRow.PMRefNbr = pmTranRow.RefNbr;
                    }
                    else
                    {
                        fsPostInfoRow.PMDocType = fsCreatedDocRow.CreatedDocType;
                        fsPostInfoRow.PMRefNbr = fsCreatedDocRow.CreatedRefNbr;
                    }

                    fsPostInfoRow.PMTranID = pmTranRow.TranID;

                    fsPostDetRow.PMPosted = fsPostInfoRow.PMPosted;
                    fsPostDetRow.PMDocType = fsPostInfoRow.PMDocType;
                    fsPostDetRow.PMRefNbr = fsPostInfoRow.PMRefNbr;
                    fsPostDetRow.PMTranID = fsPostInfoRow.PMTranID;

                    if (fsINCreatedDocRow != null && fsPostDocRow.INDocLineRef != null && fsPostDocRow.INDocLineRef is INTran)
                    {
                        postDet2 = new FSPostDet();

                        inTranRow = (INTran)fsPostDocRow.INDocLineRef;

                        fsPostInfoRow.INPosted = true;

                        if (fsCreatedDocRow == null)
                        {
                            fsPostInfoRow.INDocType = inTranRow.TranType;
                            fsPostInfoRow.INRefNbr = inTranRow.RefNbr;
                        }
                        else
                        {
                            fsPostInfoRow.INDocType = fsINCreatedDocRow.CreatedDocType;
                            fsPostInfoRow.INRefNbr = fsINCreatedDocRow.CreatedRefNbr;
                        }

                        fsPostInfoRow.INLineNbr = inTranRow.LineNbr;

                        postDet2.INPosted = fsPostInfoRow.INPosted;
                        postDet2.INDocType = fsPostInfoRow.INDocType;
                        postDet2.INRefNbr = fsPostInfoRow.INRefNbr;
                        postDet2.INLineNbr = fsPostInfoRow.INLineNbr;
                    }
                }

                if (docLine.SourceTable == ID.TablePostSource.FSAPPOINTMENT_DET)
                {
                    fsPostInfoRow.AppointmentID = docLine.DocID;
                }
                else if (docLine.SourceTable == ID.TablePostSource.FSSO_DET)
                {
                    fsPostInfoRow.SOID = docLine.DocID;
                }

                if (insertingPostInfo == true)
                {
                    graphPostInfoEntry.PostInfoRecords.Current = graphPostInfoEntry.PostInfoRecords.Insert(fsPostInfoRow);
                }
                else
                {
                    graphPostInfoEntry.PostInfoRecords.Current = graphPostInfoEntry.PostInfoRecords.Update(fsPostInfoRow);
                }

                graphPostInfoEntry.Save.Press();
                fsPostInfoRow = graphPostInfoEntry.PostInfoRecords.Current;
                
                #region Insert PostDet1
                fsPostDetRow.BatchID = fsPostBatchRow.BatchID;
                fsPostDetRow.PostID = fsPostInfoRow.PostID;
                
                cacheFSPostDet.Insert(fsPostDetRow);
                #endregion

                if (postDet2 != null)
                {
                    #region Insert PostDet2
                    postDet2.BatchID = fsPostBatchRow.BatchID;
                    postDet2.PostID = fsPostInfoRow.PostID;

                    cacheFSPostDet.Insert(postDet2);
                    #endregion
                }

                if (insertingPostInfo == true)
                {
                    if (docLine.SourceTable == ID.TablePostSource.FSAPPOINTMENT_DET)
                    {
                        PXUpdate<
                            Set<FSAppointmentDet.postID, Required<FSAppointmentDet.postID>>,
                        FSAppointmentDet,
                        Where<
                            FSAppointmentDet.appDetID, Equal<Required<FSAppointmentDet.appDetID>>>>
                        .Update(cacheFSPostDet.Graph, fsPostInfoRow.PostID, docLine.LineID);
                    }
                    else if (docLine.SourceTable == ID.TablePostSource.FSSO_DET)
                    {
                        PXUpdate<
                            Set<FSSODet.postID, Required<FSSODet.postID>>,
                        FSSODet,
                        Where<
                            FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>
                        .Update(cacheFSPostDet.Graph, fsPostInfoRow.PostID, docLine.LineID);
                    }
                }

                UpdateSourcePostDoc(soGraph, apptGraph, cacheFSPostDet, fsPostBatchRow, fsPostDocRow);
            }

            cacheFSPostDet.Persist(PXDBOperation.Insert);
        }

        public virtual IInvoiceGraph CreateInvoiceGraph(string targetScreen)
        {
            return InvoiceHelper.CreateInvoiceGraph(targetScreen);
        }
        
        #endregion

        public abstract void UpdateSourcePostDoc(ServiceOrderEntry soGraph,
                                                 AppointmentEntry apptGraph,
                                                 PXCache<FSPostDet> cacheFSPostDet,
                                                 FSPostBatch fsPostBatchRow,
                                                 FSPostDoc fsPostDocRow);

        //Consider DeallocateItemsThatAreBeingPosted in PX.Objects.FS\CreateInvoiceByContractPost.cs
        public virtual void DeallocateItemsThatAreBeingPosted(ServiceOrderEntry graph, List<DocLineExt> docLines, bool postingAppointments)
        {
            List<FSSODetSplit> splitsToDeallocate = new List<FSSODetSplit>();

            IEnumerable<IGrouping<(string, string), DocLineExt>> docGroups = docLines.GroupBy(x => (x.fsServiceOrder.SrvOrdType, x.fsServiceOrder.RefNbr));

            if (postingAppointments == false)
            {
                foreach (IGrouping<(string, string), DocLineExt> orderGroup in docGroups)
                {
                    FSServiceOrder order = orderGroup.First().fsServiceOrder;

                    foreach (FSSODetSplit soSplit in PXSelect<FSSODetSplit,
                                                    Where<FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                                        And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                                        And<FSSODetSplit.completed, Equal<False>,
                                                        And<FSSODetSplit.pOCreate, Equal<False>,
                                                        And<FSSODetSplit.inventoryID, IsNotNull>>>>>,
                                                    OrderBy<Asc<FSSODetSplit.lineNbr,
                                                            Asc<FSSODetSplit.splitLineNbr>>>>
                                                    .Select(graph, order.SrvOrdType, order.RefNbr))
                    {
                        FSSODetSplit splitCopy = (FSSODetSplit)graph.Splits.Cache.CreateCopy(soSplit);
                        splitCopy.BaseQty = 0;
                        splitsToDeallocate.Add(splitCopy);
                    }
                }
            }
            else
            {
                PXCache apptLineCache = new PXCache<FSAppointmentDet>(this);
                PXCache apptLineSplitCache = new PXCache<FSApptLineSplit>(this);

                int? lastSOLineLineNbr = null;
                FSSODet soLine = null;
                bool isLotSerialRequired = false;
                List<FSAppointmentDet> apptLines = new List<FSAppointmentDet>();
                List<FSApptLineSplit> apptSplits = new List<FSApptLineSplit>();

                foreach (IGrouping<(string, string), DocLineExt> orderGroup in docGroups)
                {
                    FSServiceOrder order = orderGroup.First().fsServiceOrder;
                    
                    lastSOLineLineNbr = null;
                    soLine = null;
                    isLotSerialRequired = false;
                    apptLines.Clear();
                    apptSplits.Clear();

                    foreach (FSSODetSplit soSplit in PXSelect<FSSODetSplit,
                                                    Where<FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                                        And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                                        And<FSSODetSplit.completed, Equal<False>,
                                                        And<FSSODetSplit.pOCreate, Equal<False>,
                                                        And<FSSODetSplit.inventoryID, IsNotNull>>>>>,
                                                    OrderBy<Asc<FSSODetSplit.lineNbr,
                                                            Asc<FSSODetSplit.splitLineNbr>>>>
                                                    .Select(graph, order.SrvOrdType, order.RefNbr))
                    {
                        if (lastSOLineLineNbr == null || lastSOLineLineNbr != soSplit.LineNbr)
                        {
                            soLine = orderGroup.Where(x => x.fsSODet.LineNbr == soSplit.LineNbr).FirstOrDefault()?.fsSODet;
                            if (soLine == null)
                            {
                                continue;
                            }

                            isLotSerialRequired = SharedFunctions.IsLotSerialRequired(graph.ServiceOrderDetails.Cache, soSplit.InventoryID);
                            lastSOLineLineNbr = soSplit.LineNbr;

                            apptLines.Clear();
                            apptSplits.Clear();
                            foreach (FSAppointmentDet apptLine in orderGroup.Where(x => x.fsAppointmentDet.SODetID == soLine.SODetID)
                                                                            .Select(x => x.fsAppointmentDet))
                            {
                                apptLines.Add((FSAppointmentDet)apptLineCache.CreateCopy(apptLine));
                            }

                            if (isLotSerialRequired == true)
                            {
                                foreach (FSAppointmentDet apptLine in apptLines)
                                {
                                    foreach (FSApptLineSplit split in PXParentAttribute.SelectChildren(apptLineSplitCache, apptLine, typeof(FSAppointmentDet)))
                                    {
                                        apptSplits.Add((FSApptLineSplit)apptLineSplitCache.CreateCopy(split));
                                    }
                                }
                            }
                        }

                        if (isLotSerialRequired == true)
                        {
                            foreach (FSApptLineSplit apptSplit in apptSplits.Where(x => string.IsNullOrEmpty(x.LotSerialNbr) == false
                                                      && string.Equals(x.LotSerialNbr, soSplit.LotSerialNbr, StringComparison.OrdinalIgnoreCase)))
                            {
                                if (apptSplit.BaseQty <= soSplit.BaseQty)
                                {
                                    soSplit.BaseQty -= apptSplit.BaseQty;
                                    apptSplit.BaseQty = 0;
                                }
                                else
                                {
                                    apptSplit.BaseQty -= soSplit.BaseQty;
                                    soSplit.BaseQty = 0;
                                }

                                FSAppointmentDet apptLine = FSAppointmentDet.PK.Find(graph, apptSplit.SrvOrdType, apptSplit.ApptNbr, apptSplit.LineNbr);
                                
                                if (apptLine == null || apptLine.SrvOrdType != apptSplit.SrvOrdType || apptLine.RefNbr != apptSplit.ApptNbr || apptLine.LineNbr != apptSplit.LineNbr)
                                {
                                    throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSAppointmentDet)));
                                }
                            }
                        }
                        else
                        {
                            foreach (FSAppointmentDet apptLine in apptLines.Where(x => x.BaseEffTranQty > 0m))
                            {
                                if (apptLine.BaseEffTranQty <= soSplit.BaseQty)
                                {
                                    soSplit.BaseQty -= apptLine.BaseEffTranQty;
                                    apptLine.BaseEffTranQty = 0;
                                }
                                else
                                {
                                    apptLine.BaseEffTranQty -= soSplit.BaseQty;
                                    soSplit.BaseQty = 0;
                                }
                            }
                        }

                        splitsToDeallocate.Add(soSplit);
                    }
                }
            }

            FSAllocationProcess.DeallocateServiceOrderSplits(graph, splitsToDeallocate, calledFromServiceOrder: false);
        }

        #region Protected Methods
        protected virtual void IncludeReviewInvoiceBatchesAction()
        {
            var fsPostBatchRows = PXSelect<FSPostBatch, Where<FSPostBatch.status, Equal<FSPostBatch.status.temporary>>>.SelectWindowed(this, 0, 1);

            if (fsPostBatchRows.Count == 0)
            {
                openReviewTemporaryBatch.SetVisible(false);
            } 
            else
            {
                openReviewTemporaryBatch.SetVisible(true);
            }
        }

        protected virtual void HideOrShowInvoiceActions(PXCache cache, CreateInvoiceFilter createInvoiceFilterRow)
        {
            bool postToSO = createInvoiceFilterRow.PostTo == ID.Batch_PostTo_Filter.SO;

            // @TODO: AC-142850 Temporary hide AP/AR actions until will be developed 
            bool postToAPAR = createInvoiceFilterRow.PostTo == ID.Batch_PostTo_Filter.AR_AP & false;

            bool postToPM = createInvoiceFilterRow.PostTo == ID.Batch_PostTo_Filter.PM;

            PXUIFieldAttribute.SetVisible<CreateInvoiceFilter.prepareInvoice>(cache, createInvoiceFilterRow, postToSO);
            PXUIFieldAttribute.SetVisible<CreateInvoiceFilter.emailSalesOrder>(cache, createInvoiceFilterRow, postToSO);
            PXUIFieldAttribute.SetVisible<CreateInvoiceFilter.sOQuickProcess>(cache, createInvoiceFilterRow, postToSO);
            PXUIFieldAttribute.SetVisible<CreateInvoiceFilter.releaseInvoice>(cache, createInvoiceFilterRow, postToAPAR || postToSO);
            PXUIFieldAttribute.SetVisible<CreateInvoiceFilter.emailInvoice>(cache, createInvoiceFilterRow, postToAPAR);
            PXUIFieldAttribute.SetVisible<CreateInvoiceFilter.releaseBill>(cache, createInvoiceFilterRow, postToAPAR);
            PXUIFieldAttribute.SetVisible<CreateInvoiceFilter.payBill>(cache, createInvoiceFilterRow, postToAPAR);
            PXUIFieldAttribute.SetVisible<CreateInvoiceFilter.ignoreBillingCycles>(cache, createInvoiceFilterRow, !postToPM);
        }

		protected DateTime? GetCutOffDate(PXGraph graph, DateTime? docDate, int? customerID, string srvOrdType)
		{
			if (docDate == null)
				return null;

			string frequencyType = string.Empty;
			int? weeklyFrequency = null;
			int? monthlyFrequency = null;

			var result = (PXResult<FSCustomerBillingSetup, FSSrvOrdType>)PXSelectJoin<FSCustomerBillingSetup,
				LeftJoin<FSSrvOrdType, On<FSSrvOrdType.srvOrdType, Equal<FSCustomerBillingSetup.srvOrdType>>,
				CrossJoin<FSSetup>>,
				Where<FSCustomerBillingSetup.customerID, Equal<Required<FSCustomerBillingSetup.customerID>>,
						And<Where2<
							Where<FSSetup.customerMultipleBillingOptions, Equal<True>,
								And<FSCustomerBillingSetup.srvOrdType, Equal<Required<FSCustomerBillingSetup.srvOrdType>>>>,
							Or<Where<FSSetup.customerMultipleBillingOptions, Equal<False>,
								And<FSCustomerBillingSetup.srvOrdType, IsNull>>>>>>>.Select(graph, customerID, srvOrdType);

			FSCustomerBillingSetup fsCustomerBillingSetupRow = (FSCustomerBillingSetup)result;
			FSSrvOrdType fsSrvOrdTypeRow = (FSSrvOrdType)result;

			if (fsSrvOrdTypeRow == null)
			{
				fsSrvOrdTypeRow = FSSrvOrdType.PK.Find(graph, srvOrdType);
			}

			if (fsCustomerBillingSetupRow != null && fsSrvOrdTypeRow != null && fsSrvOrdTypeRow.PostTo != ID.SrvOrdType_PostTo.PROJECTS)
			{
				if (fsCustomerBillingSetupRow.FrequencyType != ID.Frequency_Type.NONE)
				{
					frequencyType = fsCustomerBillingSetupRow.FrequencyType;
					weeklyFrequency = fsCustomerBillingSetupRow.WeeklyFrequency;
					monthlyFrequency = fsCustomerBillingSetupRow.MonthlyFrequency;
				}
				else
				{
					FSBillingCycle fsBillingCycleRow = FSBillingCycle.PK.Find(graph, fsCustomerBillingSetupRow.BillingCycleID);

					if (fsBillingCycleRow != null && fsBillingCycleRow.BillingCycleType == ID.Billing_Cycle_Type.TIME_FRAME)
					{
						frequencyType = fsBillingCycleRow.TimeCycleType;
						weeklyFrequency = fsBillingCycleRow.TimeCycleWeekDay;
						monthlyFrequency = fsBillingCycleRow.TimeCycleDayOfMonth;
					}
				}
			}
			else if (fsSrvOrdTypeRow != null && fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.PROJECTS)
			{
				frequencyType = ID.Frequency_Type.NONE;
				weeklyFrequency = 0;
				monthlyFrequency = 0;
			}

			if (docDate == null)
				return null;

			docDate = new DateTime(docDate.Value.Year, docDate.Value.Month, docDate.Value.Day);
			DateTime? cutOffDate = docDate;

			if (frequencyType == ID.Frequency_Type.WEEKLY)
			{
				int offset = weeklyFrequency.Value - (int)docDate.Value.DayOfWeek;

				if ((int)docDate.Value.DayOfWeek > weeklyFrequency)
				{
					offset += 7;
				}

				cutOffDate = docDate.Value.AddDays(offset);
			}
			else if (frequencyType == ID.Frequency_Type.MONTHLY)
			{
				if (docDate.Value.Day <= monthlyFrequency)
				{
					int daysInMonth = DateTime.DaysInMonth(docDate.Value.Year, docDate.Value.Month);

					if (monthlyFrequency <= daysInMonth)
					{
						cutOffDate = docDate.Value.AddDays(monthlyFrequency.Value - docDate.Value.Day);
					}
					else
					{
						cutOffDate = docDate.Value.AddDays(daysInMonth - docDate.Value.Day);
					}
				}
				else
				{
					cutOffDate = docDate.Value.AddDays(monthlyFrequency.Value - docDate.Value.Day).AddMonths(1);
				}
			}

			return cutOffDate;
		}
		#endregion

		#region Public Classes


		public class PostBatchShared
        {
            public PostBatchEntry PostBatchEntryGraph;
            public FSPostBatch FSPostBatchRow;
        }
        #endregion
    }

    public class InvoicingProcessStepGroupShared
    {
        public IInvoiceProcessGraph ProcessGraph;

        public IInvoiceGraph InvoiceGraph;
        public PXCache<FSCreatedDoc> CacheFSCreatedDoc;

        public PostInfoEntry PostInfoEntryGraph;
        public PXCache<FSPostDet> CacheFSPostDet;

        public ServiceOrderEntry ServiceOrderGraph;

        public virtual void Initialize(string targetScreen, string billingBy)
        {
            if (ProcessGraph == null)
            {
                ProcessGraph = CreateInvoiceProcessGraph(billingBy);
            }
            else
            {
                ProcessGraph.Clear(PXClearOption.ClearAll);
            }

            if (InvoiceGraph == null)
            {
                InvoiceGraph = InvoiceHelper.CreateInvoiceGraph(targetScreen);
            }
            else
            {
                InvoiceGraph.Clear();
            }

            if (ServiceOrderGraph == null)
            {
                ServiceOrderGraph = PXGraph.CreateInstance<ServiceOrderEntry>();
            }
            else
            {
                ServiceOrderGraph.Clear();
            }

            if (CacheFSCreatedDoc == null)
            {
                CacheFSCreatedDoc = new PXCache<FSCreatedDoc>(ProcessGraph.GetGraph());
            }
            else
            {
                CacheFSCreatedDoc.Clear();
            }

            if (PostInfoEntryGraph == null)
            {
                PostInfoEntryGraph = PXGraph.CreateInstance<PostInfoEntry>();
            }
            else
            {
                PostInfoEntryGraph.Clear(PXClearOption.ClearAll);
            }

            if (CacheFSPostDet == null)
            {
                CacheFSPostDet = new PXCache<FSPostDet>(PostInfoEntryGraph);
            }
            else
            {
                CacheFSPostDet.Clear();
            }
        }

        public virtual IInvoiceProcessGraph CreateInvoiceProcessGraph(string billingBy)
        {
            if (billingBy == ID.Billing_By.SERVICE_ORDER)
            {
                return PXGraph.CreateInstance<CreateInvoiceByServiceOrderPost>();
            }
            else if (billingBy == ID.Billing_By.APPOINTMENT)
            {
                return PXGraph.CreateInstance<CreateInvoiceByAppointmentPost>();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public virtual void Clear()
        {
            if (ProcessGraph != null)
            {
                ProcessGraph.Clear(PXClearOption.ClearAll);
            }

            if (InvoiceGraph != null)
            {
                InvoiceGraph.Clear();
            }

            if (CacheFSCreatedDoc != null)
            {
                CacheFSCreatedDoc.Clear();
            }

            if (PostInfoEntryGraph != null)
            {
                PostInfoEntryGraph.Clear(PXClearOption.ClearAll);
            }

            if (CacheFSPostDet != null)
            {
                CacheFSPostDet.Clear();
            }
        }

        public virtual void Dispose()
        {
            Clear();

            ProcessGraph = null;
            InvoiceGraph = null;
            CacheFSCreatedDoc = null;
            PostInfoEntryGraph = null;
            CacheFSPostDet = null;
        }
    }
}
