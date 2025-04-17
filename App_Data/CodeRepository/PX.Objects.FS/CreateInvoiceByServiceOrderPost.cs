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
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.PM;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class CreateInvoiceByServiceOrderPost : CreateInvoiceBase<CreateInvoiceByServiceOrderPost, ServiceOrderToPost>
    {
		#region Selects
		[PXFilterable]
		public new PXFilteredProcessing<ServiceOrderToPost, CreateInvoiceFilter, Where<True, Equal<True>>, OrderBy<Asc<ServiceOrderToPost.billingCycleID, Asc<ServiceOrderToPost.groupKey>>>> PostLines;

		public virtual IEnumerable postLines()
		{
			var query = new PXSelectJoin<ServiceOrderToPost,
				LeftJoinSingleTable<Customer,
					On<Customer.bAccountID, Equal<ServiceOrderToPost.billCustomerID>>,
				InnerJoin<FSBillingCycle,
					On<FSBillingCycle.billingCycleID, Equal<ServiceOrderToPost.billingCycleID>>>>,
			   Where2<
				   Where<Current<FSSetup.filterInvoicingManually>, Equal<False>,
					Or<Current<CreateInvoiceFilter.loadData>, Equal<True>>>,
			   And2<
				   Where<Customer.bAccountID, IsNull,
					Or<Match<Customer, Current<AccessInfo.userName>>>>,
			   And<
				   Where2<
					   Where2<
						   Where<
								Current<CreateInvoiceFilter.postTo>, Equal<ListField_PostTo.AR_AP>,
								And<
									ServiceOrderToPost.postTo, Equal<ListField_PostTo.AR>>>,
						Or<
						   Where2<
							   Where<
									Current<CreateInvoiceFilter.postTo>, Equal<ListField_PostTo.SO>,
									And<
										ServiceOrderToPost.postTo, Equal<ListField_PostTo.SO>>>,
							Or<
								Where2<
									Where<
										Current<CreateInvoiceFilter.postTo>, Equal<ListField_PostTo.SI>,
										And<
											ServiceOrderToPost.postTo, Equal<ListField_PostTo.SI>>>,
									Or<
										Where<
											Current<CreateInvoiceFilter.postTo>, Equal<ListField_PostTo.PM>,
											And<ServiceOrderToPost.postTo, Equal<ListField_PostTo.PM>>>>>>>>>,
				   And<
					   Where2<
							Where<
								IsNull<ServiceOrderToPost.billingBy, FSBillingCycle.billingBy>, Equal<ListField_Billing_By.ServiceOrder>,
								And<ServiceOrderToPost.postedBy, IsNull,
								And<ServiceOrderToPost.pendingAPARSOPost, Equal<True>>>>,
						And<
							Where2<
								Where<
									Current<CreateInvoiceFilter.billingCycleID>, IsNull,
								Or<
									ServiceOrderToPost.billingCycleID, Equal<Current<CreateInvoiceFilter.billingCycleID>>>>,
							And<
								Where2<
									Where<
										Current<CreateInvoiceFilter.customerID>, IsNull,
									Or<
										ServiceOrderToPost.billCustomerID, Equal<Current<CreateInvoiceFilter.customerID>>>>,
								And<
									Where<Current<CreateInvoiceFilter.ignoreBillingCycles>, Equal<False>,
									   Or<ServiceOrderToPost.orderDate, LessEqual<Current<CreateInvoiceFilter.upToDate>>>>>>>>>>>>>>>>(this);

			foreach (PXResult<ServiceOrderToPost> line in query.Select())
			{
				var servOrd = (ServiceOrderToPost)line;
				servOrd.GroupKey = GetGroupKey(servOrd);
				servOrd.CutOffDate = GetCutOffDate(this, servOrd.OrderDate, servOrd.BillCustomerID, servOrd.SrvOrdType);
				if (Filter.Current.IgnoreBillingCycles == true || servOrd.CutOffDate <= Filter.Current.UpToDate)
					yield return line;
			}
		}
		#endregion

		#region ViewPostBatch
		public PXAction<CreateInvoiceFilter> viewPostBatch;
        [PXUIField(DisplayName = "")]
        public virtual IEnumerable ViewPostBatch(PXAdapter adapter)
        {
            if (PostLines.Current != null)
            {
                ServiceOrderToPost postLineRow = PostLines.Current;
                PostBatchMaint graphPostBatchMaint = PXGraph.CreateInstance<PostBatchMaint>();

                if (postLineRow.BatchID != null)
                {
                    graphPostBatchMaint.BatchRecords.Current = graphPostBatchMaint.BatchRecords.Search<FSPostBatch.batchID>(postLineRow.BatchID);
                    throw new PXRedirectRequiredException(graphPostBatchMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }

            return adapter.Get();
        }
        #endregion

        #region CacheAttached
        #region ServiceOrderToPost_SOID
        [PXDBInt]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [PXSelector(typeof(
            Search<ServiceOrderToPost.sOID,
            Where<
                    ServiceOrderToPost.srvOrdType, Equal<Current<ServiceOrderToPost.srvOrdType>>>>),
            SubstituteKey = typeof(ServiceOrderToPost.refNbr))]
        protected virtual void ServiceOrderToPost_SOID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Event Handlers
        protected override void _(Events.RowSelected<CreateInvoiceFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            base._(e);

            CreateInvoiceFilter createInvoiceFilterRow = (CreateInvoiceFilter)e.Row;

            string errorMessage = PXUIFieldAttribute.GetErrorOnly<CreateInvoiceFilter.invoiceFinPeriodID>(e.Cache, createInvoiceFilterRow);
            bool enableProcessButtons = string.IsNullOrEmpty(errorMessage) == true;

            PostLines.SetProcessAllEnabled(enableProcessButtons);
            PostLines.SetProcessEnabled(enableProcessButtons);
        }
        #endregion

        public CreateInvoiceByServiceOrderPost() : base()
        {
            billingBy = ID.Billing_By.SERVICE_ORDER;
            CreateInvoiceByServiceOrderPost graphCreateInvoiceByServiceOrderPost = null;

			if (PX.Common.WebConfig.ParallelProcessingDisabled == false)
			{
				PostLines.ParallelProcessingOptions = settings => {
					settings.IsEnabled = true;
					settings.BatchSize = 1000;
					settings.SplitToBatches = SplitToBatches;
				};
			}
			// Acuminator disable once PX1008 LongOperationDelegateSynchronousExecution [compatibility with legacy code]
			PostLines.SetProcessDelegate(
                delegate(List<ServiceOrderToPost> serviceOrderToPostRows)
                {
                    graphCreateInvoiceByServiceOrderPost = PXGraph.CreateInstance<CreateInvoiceByServiceOrderPost>();
                    CreateInvoices(graphCreateInvoiceByServiceOrderPost, serviceOrderToPostRows, Filter.Current, PXQuickProcess.ActionFlow.NoFlow, true);
                });
			PeriodValidation validationValue = this.IsContractBasedAPI ||
				this.IsImport || this.IsExport || this.UnattendedMode ? PeriodValidation.DefaultUpdate : PeriodValidation.DefaultSelectUpdate;
			OpenPeriodAttribute.SetValidatePeriod<CreateInvoiceFilter.invoiceFinPeriodID>(Filter.Cache, null, validationValue);
		}

		private IEnumerable<(int, int)> SplitToBatches(IList list, PXParallelProcessingOptions options)
		{
			ServiceOrderToPost tEnd, tNext;
			int start = 0, end = 0;

			while (start < list.Count)
			{
				end = Math.Min(start + options.BatchSize - 1, list.Count - 1) - 1;
				do
				{
					end++;
					tEnd = (ServiceOrderToPost)list[end];
					tNext = (end + 1) < list.Count ? (ServiceOrderToPost)list[end + 1] : null;
				}
				while (tEnd.BillingCycleID == tNext?.BillingCycleID && tEnd.GroupKey == tNext?.GroupKey);

				yield return (start, end);
				start = end + 1;
			}
		}

		public override List<DocLineExt> GetInvoiceLines(Guid currentProcessID, int billingCycleID, string groupKey, bool getOnlyTotal, out decimal? invoiceTotal, string postTo)
        {
            PXGraph tempGraph = new PXGraph();

            if (getOnlyTotal == true)
            {
                /* Always keep both BQLs with the same Joins and Where conditions */
                FSSODet fsSODetRow =
                        PXSelectJoinGroupBy<FSSODet,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.srvOrdType, Equal<FSSODet.srvOrdType>,
                                    And<FSServiceOrder.refNbr, Equal<FSSODet.refNbr>>>,
                            InnerJoin<FSSrvOrdType,
                                On<FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                            InnerJoin<FSPostDoc,
                                On<
                                    FSPostDoc.sOID, Equal<FSSODet.sOID>,
                                    And<FSPostDoc.entityType, Equal<ListField_PostDoc_EntityType.Service_Order>>>,
                            LeftJoin<FSPostInfo,
                                On<
                                    FSPostInfo.postID, Equal<FSSODet.postID>>>>>>,
                        Where<
                            FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>,
                            And<FSPostDoc.billingCycleID, Equal<Required<FSPostDoc.billingCycleID>>,
                            And<FSPostDoc.groupKey, Equal<Required<FSPostDoc.groupKey>>,
                            And<FSSODet.lineType, NotEqual<FSLineType.Comment>,
                            And<FSSODet.lineType, NotEqual<FSLineType.Instruction>,
                            And<FSSODet.status, NotEqual<FSSODet.status.Canceled>,
                            And<FSSODet.isPrepaid, Equal<False>,
                            And<FSSODet.isBillable, Equal<True>,
                            And<
                                Where2<
                                    Where<
                                        FSSODet.postID, IsNull>,
                                    Or<
                                        Where<
                                            FSPostInfo.aRPosted, Equal<False>,
                                            And<FSPostInfo.aPPosted, Equal<False>,
                                            And<FSPostInfo.sOPosted, Equal<False>,
                                            And<FSPostInfo.sOInvPosted, Equal<False>,
                                            And<
                                                Where2<
                                                    Where<
                                                        Required<FSPostBatch.postTo>, NotEqual<FSPostBatch.postTo.SO>>,
                                                    Or<
                                                        Where<
                                                            Required<FSPostBatch.postTo>, Equal<FSPostBatch.postTo.SO>,
                                                            And<FSPostInfo.iNPosted, Equal<False>>>>>>>>>>>>>>>>>>>>>,
                        Aggregate<
                            Sum<FSSODet.curyBillableTranAmt>>>
                        .Select(tempGraph, currentProcessID, billingCycleID, groupKey, postTo, postTo);

                invoiceTotal = fsSODetRow.CuryBillableTranAmt;

                return null;
            }
            else
            {
                invoiceTotal = null;

                /* Always keep both BQLs with the same Joins and Where conditions */
                var resultSet = PXSelectJoin<FSSODet,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.srvOrdType, Equal<FSSODet.srvOrdType>,
                                    And<FSServiceOrder.refNbr, Equal<FSSODet.refNbr>>>,
                            InnerJoin<FSSrvOrdType,
                                On<FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                            InnerJoin<FSPostDoc,
                                On<
                                    FSPostDoc.sOID, Equal<FSSODet.sOID>,
                                    And<FSPostDoc.entityType, Equal<ListField_PostDoc_EntityType.Service_Order>>>,
                            LeftJoin<FSPostInfo,
                                On<
                                    FSPostInfo.postID, Equal<FSSODet.postID>>,
                            LeftJoin<PMTask,
                                On<PMTask.projectID, Equal<FSSODet.projectID>, And<PMTask.taskID, Equal<FSSODet.projectTaskID>>>>>>>>,
                        Where<
                            FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>,
                            And<FSPostDoc.billingCycleID, Equal<Required<FSPostDoc.billingCycleID>>,
                            And<FSPostDoc.groupKey, Equal<Required<FSPostDoc.groupKey>>,
                            And<FSSODet.lineType, NotEqual<FSLineType.Comment>,
                            And<FSSODet.lineType, NotEqual<FSLineType.Instruction>,
                            And<FSSODet.status, NotEqual<FSSODet.status.Canceled>,
                            And<FSSODet.isPrepaid, Equal<False>,
                            And<FSSODet.isBillable, Equal<True>,
                            And<
                                Where2<
                                    Where<
                                        FSSODet.postID, IsNull>,
                                    Or<
                                        Where<
                                            FSPostInfo.aRPosted, Equal<False>,
                                            And<FSPostInfo.aPPosted, Equal<False>,
                                            And<FSPostInfo.sOPosted, Equal<False>,
                                            And<FSPostInfo.sOInvPosted, Equal<False>,
                                            And<
                                                Where2<
                                                    Where<
                                                        Required<FSPostBatch.postTo>, NotEqual<FSPostBatch.postTo.SO>>,
                                                    Or<
                                                        Where<
                                                            Required<FSPostBatch.postTo>, Equal<FSPostBatch.postTo.SO>,
                                                            And<FSPostInfo.iNPosted, Equal<False>>>>>>>>>>>>>>>>>>>>>,
                        OrderBy<
                            Asc<FSServiceOrder.orderDate,
                            Asc<FSSODet.sOID,
                            Asc<FSSODet.sODetID>>>>>
                        .Select(tempGraph, currentProcessID, billingCycleID, groupKey, postTo, postTo);

                var docLines = new List<DocLineExt>();

                foreach (PXResult<FSSODet, FSServiceOrder, FSSrvOrdType, FSPostDoc, FSPostInfo,  PMTask> row in resultSet)
                {
                    docLines.Add(new DocLineExt(row));
                }

                return docLines;
            }
        }

        public override void UpdateSourcePostDoc(ServiceOrderEntry soGraph,
                                                 AppointmentEntry apptGraph,
                                                 PXCache<FSPostDet> cacheFSPostDet,
                                                 FSPostBatch fsPostBatchRow,
                                                 FSPostDoc fsPostDocRow)
        {
            if (fsPostBatchRow == null || fsPostDocRow == null || fsPostDocRow.SOID == null)
            {
                throw new ArgumentNullException();
            }

            FSServiceOrder serviceOrder = soGraph.ServiceOrderRecords.Current = FSServiceOrder.UK.Find(soGraph, fsPostDocRow.SOID);

            if (serviceOrder == null)
            {
                throw new PXException(TX.Error.SERVICE_ORDER_NOT_FOUND);
            }

			if (serviceOrder.PendingAPARSOPost != true)
				return; // further actions should be executed just once for the service order

            if (serviceOrder.PostedBy == null)
            {
                serviceOrder = (FSServiceOrder)soGraph.ServiceOrderRecords.Cache.CreateCopy(serviceOrder);

                serviceOrder.PostedBy = ID.Billing_By.SERVICE_ORDER;
                serviceOrder.PendingAPARSOPost = false;
                serviceOrder.Billed = true;
				serviceOrder.BillingBy = soGraph.BillingCycleRelated.Current.BillingBy;

                soGraph.ServiceOrderRecords.Update(serviceOrder);

                soGraph.ServiceOrderRecords.Cache.SetValue<FSServiceOrder.finPeriodID>(serviceOrder, fsPostBatchRow.FinPeriodID);

                soGraph.SkipTaxCalcAndSave();
            }
        }
    }
}
