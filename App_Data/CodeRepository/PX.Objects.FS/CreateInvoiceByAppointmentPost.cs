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
using PX.Objects.PM;
using PX.Objects.AR;
using System;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.GL;

namespace PX.Objects.FS
{
    public class CreateInvoiceByAppointmentPost : CreateInvoiceBase<CreateInvoiceByAppointmentPost, AppointmentToPost>
    {
        #region Selects
        [PXFilterable]
		public new PXFilteredProcessing<AppointmentToPost, CreateInvoiceFilter, Where<True, Equal<True>>, OrderBy<Asc<AppointmentToPost.billingCycleID, Asc<AppointmentToPost.groupKey>>>> PostLines;

		public virtual IEnumerable postLines()
		{
			var query = new PXSelectJoin<AppointmentToPost,
				LeftJoin<FSServiceContract,
						On<FSServiceContract.serviceContractID, Equal<AppointmentToPost.serviceContractID>>,
                LeftJoinSingleTable<Customer,
						On<Customer.bAccountID, Equal<AppointmentToPost.billCustomerID>>>>,
                   Where2<
                        Where<Current<FSSetup.filterInvoicingManually>, Equal<False>,
                        Or<Current<CreateInvoiceFilter.loadData>, Equal<True>>>,
                    And2<
                        Where<Customer.bAccountID, IsNull,
                        Or<Match<Customer, Current<AccessInfo.userName>>>>,
						And2<
							Where<FSServiceContract.serviceContractID, IsNull,
								Or<FSServiceContract.billingType, NotEqual<ListField.ServiceContractBillingType.standardizedBillings>>>,
                        And<
                           Where2<
                               Where2<
                                   Where<
                                       Current<CreateInvoiceFilter.postTo>, Equal<ListField_PostTo.AR_AP>,
                                       And<AppointmentToPost.postTo, Equal<ListField_PostTo.AR>>>,
                                   Or<
                                       Where2<
                                           Where<
                                               Current<CreateInvoiceFilter.postTo>, Equal<ListField_PostTo.SO>,
                                               And<AppointmentToPost.postTo, Equal<ListField_PostTo.SO>>>,
                                           Or<
                                               Where2<
                                                   Where<
                                                       Current<CreateInvoiceFilter.postTo>, Equal<ListField_PostTo.SI>,
                                                       And<AppointmentToPost.postTo, Equal<ListField_PostTo.SI>>>,
                                                        Or<
                                                            Where<
                                                                Current<CreateInvoiceFilter.postTo>, Equal<ListField_PostTo.PM>,
                                                                And<AppointmentToPost.postTo, Equal<ListField_PostTo.PM>>>>>>>>>,
                               And<
                                   Where2<
                                       Where<
                                           AppointmentToPost.billingBy, Equal<ListField_Billing_By.Appointment>,
                                           And<AppointmentToPost.pendingAPARSOPost, Equal<True>,
                                           And<
                                               Where<AppointmentToPost.postedBy, Equal<ListField_Billing_By.Appointment>,
                                               Or<AppointmentToPost.postedBy, IsNull>>>>>,
                                       And<
                                           Where2<
                                               Where<
                                                   Current<CreateInvoiceFilter.billingCycleID>, IsNull,
                                                   Or<AppointmentToPost.billingCycleID, Equal<Current<CreateInvoiceFilter.billingCycleID>>>>,
                                               And<
                                                   Where2<
                                                       Where<
                                                           Current<CreateInvoiceFilter.customerID>, IsNull,
                                                           Or<AppointmentToPost.billCustomerID, Equal<Current<CreateInvoiceFilter.customerID>>>>,
                                                       And<
                                                                        Where<Current<CreateInvoiceFilter.ignoreBillingCycles>, Equal<False>,
															  Or<AppointmentToPost.actualDateTimeEnd, LessEqual<Current<CreateInvoiceFilter.upToDateWithTimeZone>>>>>>>>>>>>>>>>>(this);
			foreach (PXResult<AppointmentToPost> line in query.Select())
			{
				var appt = (AppointmentToPost)line;
				appt.GroupKey = GetGroupKey(appt);
				appt.CutOffDate = GetCutOffDate(this, appt.ActualDateTimeEnd, appt.BillCustomerID, appt.SrvOrdType);
				if (Filter.Current.IgnoreBillingCycles == true || appt.CutOffDate <= Filter.Current.UpToDate)
					yield return line;
			}
		}
        #endregion

        #region ViewPostBatch
        public PXAction<CreateInvoiceFilter> viewPostBatch;
		// Acuminator disable once PX1092 IncorrectAttributesOnActionHandler
        [PXUIField(DisplayName = "")]
        public virtual IEnumerable ViewPostBatch(PXAdapter adapter)
        {
            if (PostLines.Current != null)
            {
                AppointmentToPost postLineRow = PostLines.Current;
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
        #region AppointmentToPost_AppointmentID
        [PXDBIdentity]
        [PXUIField(DisplayName = "Appointment Nbr.")]
        [PXSelector(typeof(
            Search<FSAppointment.appointmentID,
            Where<
                FSAppointment.srvOrdType, Equal<Current<AppointmentToPost.srvOrdType>>>>),
            SubstituteKey = typeof(FSAppointment.refNbr))]
        protected virtual void AppointmentToPost_AppointmentID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region AppointmentToPost_SOID
        [PXDBInt]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [PXSelector(typeof(
            Search<AppointmentToPost.sOID,
            Where<
                AppointmentToPost.srvOrdType, Equal<Current<AppointmentToPost.srvOrdType>>>>),
            SubstituteKey = typeof(AppointmentToPost.soRefNbr))]
        protected virtual void AppointmentToPost_SOID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region CreateInvoiceFilter_IgnoreBillingCycles
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ignore the Time Frame")]
        protected virtual void CreateInvoiceFilter_IgnoreBillingCycles_CacheAttached(PXCache sender)
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

        public CreateInvoiceByAppointmentPost() : base()
        {
            billingBy = ID.Billing_By.APPOINTMENT;
            CreateInvoiceByAppointmentPost graphCreateInvoiceByAppointmentPost = null;

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
				delegate (List<AppointmentToPost> appointmentToPostRows)
                	{
                    	graphCreateInvoiceByAppointmentPost = PXGraph.CreateInstance<CreateInvoiceByAppointmentPost>();
                    	CreateInvoices(graphCreateInvoiceByAppointmentPost, appointmentToPostRows, Filter.Current, PXQuickProcess.ActionFlow.NoFlow, true);
                	});
			PeriodValidation validationValue = this.IsContractBasedAPI ||
				this.IsImport || this.IsExport || this.UnattendedMode ? PeriodValidation.DefaultUpdate : PeriodValidation.DefaultSelectUpdate;
			OpenPeriodAttribute.SetValidatePeriod<CreateInvoiceFilter.invoiceFinPeriodID>(Filter.Cache, null, validationValue);
        }

		private IEnumerable<(int, int)> SplitToBatches(IList list, PXParallelProcessingOptions options)
		{
			AppointmentToPost tEnd, tNext;
			int start = 0, end = 0;

			while (start < list.Count)
			{
				end = Math.Min(start + options.BatchSize - 1, list.Count - 1) - 1;
				do
				{
					end++;
					tEnd = (AppointmentToPost)list[end];
					tNext = (end + 1) < list.Count ? (AppointmentToPost)list[end + 1] : null;
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
                FSAppointmentDet fsAppointmentDetRow =
                        PXSelectJoinGroupBy<FSAppointmentDet,
                            InnerJoin<FSAppointment,
                                On<FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                            InnerJoin<FSSrvOrdType,
                                On<FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                            InnerJoin<FSPostDoc,
                                On<
                                    FSPostDoc.appointmentID, Equal<FSAppointment.appointmentID>,
                                    And<FSPostDoc.entityType, Equal<ListField_PostDoc_EntityType.Appointment>>>,
                            LeftJoin<FSPostInfo,
                                On<
                                    FSPostInfo.postID, Equal<FSAppointmentDet.postID>>>>>>>,
                        Where<
                            FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>,
                            And<FSPostDoc.billingCycleID, Equal<Required<FSPostDoc.billingCycleID>>,
                            And<FSPostDoc.groupKey, Equal<Required<FSPostDoc.groupKey>>,
                            And<FSAppointmentDet.lineType, NotEqual<FSLineType.Comment>,
                            And<FSAppointmentDet.lineType, NotEqual<FSLineType.Instruction>,
                            And<FSAppointmentDet.isCanceledNotPerformed, NotEqual<True>,
                            And<FSAppointmentDet.lineType, NotEqual<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                            And<FSAppointmentDet.isPrepaid, Equal<False>,
                            And<FSAppointmentDet.isBillable, Equal<True>,
                            And<
                                Where2<
                                    Where<
                                        FSAppointmentDet.postID, IsNull>,
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
                                                            And<FSPostInfo.iNPosted, Equal<False>>>>>>>>>>>>>>>>>>>>>>,
                        Aggregate<
                            Sum<FSAppointmentDet.billableTranAmt>>>
                        .Select(tempGraph, currentProcessID, billingCycleID, groupKey, postTo, postTo);

                invoiceTotal = fsAppointmentDetRow.BillableTranAmt;

                FSAppointmentDet fsAppointmentInventoryItem =
                        PXSelectJoinGroupBy<FSAppointmentDet,
                            InnerJoin<FSAppointment,
                                On<FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                            InnerJoin<FSSrvOrdType,
                                On<FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                            InnerJoin<FSPostDoc,
                                On<
                                    FSPostDoc.appointmentID, Equal<FSAppointment.appointmentID>,
                                    And<FSPostDoc.entityType, Equal<ListField_PostDoc_EntityType.Appointment>>>,
                            LeftJoin<FSPostInfo,
                                On<
                                    FSPostInfo.postID, Equal<FSAppointmentDet.postID>>>>>>>,
                        Where<
                            FSAppointmentDet.lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                            And<FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>,
                            And<FSPostDoc.billingCycleID, Equal<Required<FSPostDoc.billingCycleID>>,
                            And<FSPostDoc.groupKey, Equal<Required<FSPostDoc.groupKey>>,
                            And<
                                Where2<
                                    Where<
                                        FSAppointmentDet.postID, IsNull>,
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
                                                            And<FSPostInfo.iNPosted, Equal<False>>>>>>>>>>>>>>>>>,
                        Aggregate<
                            Sum<FSAppointmentDet.billableTranAmt>>>
                        .Select(tempGraph, currentProcessID, billingCycleID, groupKey, postTo, postTo);

                invoiceTotal += fsAppointmentInventoryItem.BillableTranAmt ?? 0;

                return null;
            }
            else
            {
                invoiceTotal = null;

                var resultSet1 = PXSelectJoin<FSAppointmentDet,
                            InnerJoin<FSAppointment,
                                On<FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                            InnerJoin<FSSrvOrdType,
                                On<FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                            InnerJoin<FSPostDoc,
                                On<
                                    FSPostDoc.appointmentID, Equal<FSAppointment.appointmentID>,
                                    And<FSPostDoc.entityType, Equal<ListField_PostDoc_EntityType.Appointment>>>,
                            LeftJoin<FSPostInfo,
                                On<
                                    FSPostInfo.postID, Equal<FSAppointmentDet.postID>>,
                            LeftJoin<FSSODet,
                                On<FSSODet.srvOrdType, Equal<FSServiceOrder.srvOrdType>,
                                    And<FSSODet.refNbr, Equal<FSServiceOrder.refNbr>,
                                    And<FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>>>,
                            LeftJoin<PMTask,
                                On<PMTask.projectID, Equal<FSAppointmentDet.projectID>, And<PMTask.taskID, Equal<FSAppointmentDet.projectTaskID>>>>>>>>>>,
                        Where<
                            FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>,
                            And<FSPostDoc.billingCycleID, Equal<Required<FSPostDoc.billingCycleID>>,
                            And<FSPostDoc.groupKey, Equal<Required<FSPostDoc.groupKey>>,
                            And<FSAppointmentDet.lineType, NotEqual<FSLineType.Comment>,
                            And<FSAppointmentDet.lineType, NotEqual<FSLineType.Instruction>,
                            And<FSAppointmentDet.isCanceledNotPerformed, NotEqual<True>,
                            And<FSAppointmentDet.lineType, NotEqual<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                            And<FSAppointmentDet.isPrepaid, Equal<False>,
                            And<FSAppointmentDet.isBillable, Equal<True>,
                            And<
                                Where2<
                                    Where<
                                        FSAppointmentDet.postID, IsNull>,
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
                                                            And<FSPostInfo.iNPosted, Equal<False>>>>>>>>>>>>>>>>>>>>>>,
                        OrderBy<
                            Asc<FSAppointment.executionDate,
                            Asc<FSAppointmentDet.appointmentID,
                            Asc<FSAppointmentDet.appDetID>>>>>
                        .Select(tempGraph, currentProcessID, billingCycleID, groupKey, postTo, postTo);

                var docLines = new List<DocLineExt>();

                foreach (PXResult<FSAppointmentDet, FSAppointment, FSServiceOrder, FSSrvOrdType, FSPostDoc, FSPostInfo, FSSODet,  PMTask> row in resultSet1)
                {
                    docLines.Add(new DocLineExt(row));
                }

                var resultSet2 = PXSelectJoin<FSAppointmentDet,
                            InnerJoin<FSAppointment,
                                On<FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                            InnerJoin<FSSrvOrdType,
                                On<FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                            InnerJoin<FSPostDoc,
                                On<
                                    FSPostDoc.appointmentID, Equal<FSAppointment.appointmentID>,
                                    And<FSPostDoc.entityType, Equal<ListField_PostDoc_EntityType.Appointment>>>,
                            LeftJoin<FSPostInfo,
                                On<
                                    FSPostInfo.postID, Equal<FSAppointmentDet.postID>>,
                            LeftJoin<PMTask,
                                On<PMTask.projectID, Equal<FSAppointmentDet.projectID>, And<PMTask.taskID, Equal<FSAppointmentDet.projectTaskID>>>>>>>>>,
                        Where<
                            FSAppointmentDet.lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                            And<FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>,
                            And<FSPostDoc.billingCycleID, Equal<Required<FSPostDoc.billingCycleID>>,
                            And<FSPostDoc.groupKey, Equal<Required<FSPostDoc.groupKey>>,
                            And<
                                Where2<
                                    Where<
                                        FSAppointmentDet.postID, IsNull>,
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
                                                            And<FSPostInfo.iNPosted, Equal<False>>>>>>>>>>>>>>>>>,
                        OrderBy<
                            Asc<FSAppointment.executionDate,
                            Asc<FSAppointmentDet.appointmentID,
                            Asc<FSAppointmentDet.appDetID>>>>>
                        .Select(tempGraph, currentProcessID, billingCycleID, groupKey, postTo, postTo);

                DocLineExt docLineExtRow;

                foreach (PXResult<FSAppointmentDet, FSAppointment, FSServiceOrder, FSSrvOrdType, FSPostDoc, FSPostInfo, PMTask> row in resultSet2)
                {
                    docLineExtRow = new DocLineExt(row);

                    docLineExtRow.docLine.AcctID = Get_TranAcctID_DefaultValue(this,
                                                                                docLineExtRow.fsSrvOrdType.SalesAcctSource,
                                                                                docLineExtRow.docLine.InventoryID,
                                                                                docLineExtRow.docLine.SiteID,
                                                                                docLineExtRow.fsServiceOrder);

                    docLines.Add(docLineExtRow);
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
            apptGraph.Clear(PXClearOption.ClearAll);
            soGraph.Clear(PXClearOption.ClearAll);

            FSAppointment appt = apptGraph.AppointmentRecords.Current = FSAppointment.UK.Find(apptGraph, fsPostDocRow.AppointmentID);

            if (appt == null)
            {
                throw new PXException(TX.Error.APPOINTMENT_NOT_FOUND);
            }

			if (appt.PendingAPARSOPost != true)
				return; // further actions should be executed just once for the appointment

            appt = (FSAppointment)apptGraph.AppointmentRecords.Cache.CreateCopy(appt);

            appt.PostingStatusAPARSO = ID.Status_Posting.POSTED;
            appt.PendingAPARSOPost = false;

            FSAppointment.Events.Select(ev => ev.AppointmentPosted).FireOn(apptGraph, appt);
            apptGraph.AppointmentRecords.Cache.Update(appt);
            apptGraph.AppointmentRecords.Cache.SetValue<FSAppointment.finPeriodID>(appt, fsPostBatchRow.FinPeriodID);
            apptGraph.SkipTaxCalcAndSave();

            FSServiceOrder serviceOrder = soGraph.ServiceOrderRecords.Current = FSServiceOrder.UK.Find(soGraph, fsPostDocRow.SOID);

            if (serviceOrder == null)
            {
                throw new PXException(TX.Error.SERVICE_ORDER_NOT_FOUND);
            }

            if (serviceOrder.PostedBy == null) 
            {
                serviceOrder = (FSServiceOrder)soGraph.ServiceOrderRecords.Cache.CreateCopy(serviceOrder);

                serviceOrder.PostedBy = ID.Billing_By.APPOINTMENT;
				serviceOrder.BillingBy = ID.Billing_By.APPOINTMENT;
                serviceOrder.PendingAPARSOPost = false;

                soGraph.ServiceOrderRecords.Update(serviceOrder);
                soGraph.SkipTaxCalcAndSave();
            }
        }

        public virtual int? Get_TranAcctID_DefaultValue(PXGraph graph, string salesAcctSource, int? inventoryID, int? siteID, FSServiceOrder fsServiceOrderRow)
        {
            return ServiceOrderEntry.Get_TranAcctID_DefaultValueInt(graph, salesAcctSource, inventoryID, siteID, fsServiceOrderRow);
        }
    }
}
