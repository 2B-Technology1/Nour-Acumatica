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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;

namespace PX.Objects.FS
{
    public class FSPOReceiptProcess : PXGraph<FSPOReceiptProcess>
    {
        public static FSPOReceiptProcess SingleFSProcessPOReceipt =>
            (FSPOReceiptProcess)PXContext.GetSlot<FSPOReceiptProcess>() ??
            PXContext.SetSlot<FSPOReceiptProcess>(CreateInstance<FSPOReceiptProcess>());

        public static List<PXResult<INItemPlan, INPlanType>> ProcessPOReceipt(PXGraph graph, IEnumerable<PXResult<INItemPlan, INPlanType>> list, string POReceiptType, string POReceiptNbr, bool stockItemProcessing)
            => SingleFSProcessPOReceipt.ProcessPOReceiptInt(graph, list, POReceiptType, POReceiptNbr, stockItemProcessing);

        public virtual List<PXResult<INItemPlan, INPlanType>> ProcessPOReceiptInt(PXGraph graph, IEnumerable<PXResult<INItemPlan, INPlanType>> list, string POReceiptType, string POReceiptNbr, bool stockItemProcessing)
        {
            var serviceOrder = new PXSelect<FSServiceOrder>(graph);

            if (!graph.Views.Caches.Contains(typeof(FSServiceOrder)))
                graph.Views.Caches.Add(typeof(FSServiceOrder));

            var soDetSplit = new PXSelect<FSSODetSplit>(graph);

            if (!graph.Views.Caches.Contains(typeof(FSSODetSplit)))
                graph.Views.Caches.Add(typeof(FSSODetSplit));

            var initemplan = new PXSelect<INItemPlan>(graph);

            List<PXResult<INItemPlan, INPlanType>> returnList = new List<PXResult<INItemPlan, INPlanType>>();

            List<FSSODetSplit> splitsToDeletePlanID = new List<FSSODetSplit>();

            List<FSSODetSplit> insertedSchedules = new List<FSSODetSplit>();
            List<INItemPlan> deletedPlans = new List<INItemPlan>();

            var soDetView = new PXSelect<FSSODet>(graph);
            if (!graph.Views.Caches.Contains(typeof(FSSODet)))
                graph.Views.Caches.Add(typeof(FSSODet));

            var appointmentView = new PXSelect<FSAppointment>(graph);
            if (!graph.Views.Caches.Contains(typeof(FSAppointment)))
                graph.Views.Caches.Add(typeof(FSAppointment));

            var apptLineView = new PXSelect<FSAppointmentDet>(graph);
            if (!graph.Views.Caches.Contains(typeof(FSAppointmentDet)))
                graph.Views.Caches.Add(typeof(FSAppointmentDet));

            var apptLineSplitView = new PXSelect<FSApptLineSplit>(graph);
            if (!graph.Views.Caches.Contains(typeof(FSApptLineSplit)))
                graph.Views.Caches.Add(typeof(FSApptLineSplit));

            var srvOrdLinesWithModifiedSplits = new List<SrvOrdLineWithSplits>();
            FSSODet srvOrdLine = null;
            SrvOrdLineWithSplits srvOrdLineExt = null;

            foreach (PXResult<INItemPlan, INPlanType> res in list)
            {
                bool includeInReturnList = true;
                INItemPlan plan = PXCache<INItemPlan>.CreateCopy(res);
                INPlanType plantype = res;

                //avoid ReadItem()
                if (initemplan.Cache.GetStatus(plan) != PXEntryStatus.Inserted)
                {
                    initemplan.Cache.SetStatus(plan, PXEntryStatus.Notchanged);
                }

                //Original Schedule Marked for PO / Allocated on Remote Whse
                //FSSODetSplit schedule = PXSelect<FSSODetSplit, Where<FSSODetSplit.planID, Equal<Required<FSSODetSplit.planID>>, And<FSSODetSplit.completed, Equal<False>>>>.Select(this, plan.DemandPlanID);
                FSSODetSplit schedule = PXSelect<FSSODetSplit, Where<FSSODetSplit.planID, Equal<Required<FSSODetSplit.planID>>>>.Select(graph, plan.DemandPlanID);

                if (schedule != null && (schedule.Completed == false || soDetSplit.Cache.GetStatus(schedule) == PXEntryStatus.Updated))
                {
                    includeInReturnList = false;
                    schedule = PXCache<FSSODetSplit>.CreateCopy(schedule);

                    schedule.BaseReceivedQty += plan.PlanQty;
                    schedule.ReceivedQty = INUnitAttribute.ConvertFromBase(soDetSplit.Cache, schedule.InventoryID, schedule.UOM, (decimal)schedule.BaseReceivedQty, INPrecision.QUANTITY);

                    schedule = (FSSODetSplit)soDetSplit.Cache.Update(schedule);

                    INItemPlan origplan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(graph, plan.DemandPlanID);
                    if (origplan != null)
                    {
                        origplan.PlanQty = schedule.BaseQty - schedule.BaseReceivedQty;
                        initemplan.Cache.Update(origplan);
                    }

                    //select Allocated line if any, exclude allocated on Remote Whse
                    PXSelectBase<INItemPlan> cmd = new PXSelectJoin<INItemPlan, InnerJoin<FSSODetSplit, On<FSSODetSplit.planID, Equal<INItemPlan.planID>>>, Where<INItemPlan.demandPlanID, Equal<Required<INItemPlan.demandPlanID>>, And<FSSODetSplit.isAllocated, Equal<True>, And<FSSODetSplit.siteID, Equal<Required<FSSODetSplit.siteID>>>>>>(graph);
                    if (!string.IsNullOrEmpty(plan.LotSerialNbr))
                    {
                        cmd.WhereAnd<Where<INItemPlan.lotSerialNbr, Equal<Required<INItemPlan.lotSerialNbr>>>>();
                    }
                    PXResult<INItemPlan> allocres = cmd.Select(plan.DemandPlanID, plan.SiteID, plan.LotSerialNbr);

                    if (allocres != null)
                    {
                        schedule = PXResult.Unwrap<FSSODetSplit>(allocres);
                        soDetSplit.Cache.SetStatus(schedule, PXEntryStatus.Notchanged);
                        schedule = PXCache<FSSODetSplit>.CreateCopy(schedule);
                        schedule.BaseQty += plan.PlanQty;
                        schedule.Qty = INUnitAttribute.ConvertFromBase(soDetSplit.Cache, schedule.InventoryID, schedule.UOM, (decimal)schedule.BaseQty, INPrecision.QUANTITY);
                        schedule.POReceiptType = POReceiptType;
                        schedule.POReceiptNbr = POReceiptNbr;

                        schedule = (FSSODetSplit)soDetSplit.Cache.Update(schedule);

                        INItemPlan allocplan = PXCache<INItemPlan>.CreateCopy(res);
                        allocplan.PlanQty += plan.PlanQty;

                        initemplan.Cache.Update(allocplan);

                        plantype = PXCache<INPlanType>.CreateCopy(plantype);
                        plantype.ReplanOnEvent = null;
                        plantype.DeleteOnEvent = true;

                        // Received Quantity is being incremented in the split
                        srvOrdLine = (FSSODet)PXParentAttribute.SelectParent(soDetSplit.Cache, schedule, typeof(FSSODet));
                        if (srvOrdLine == null)
                        {
                            throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSSODet)));
                        }

                        srvOrdLineExt = srvOrdLinesWithModifiedSplits.Find(e => e.SrvOrdLine.SODetID == srvOrdLine.SODetID);
                        if (srvOrdLineExt == null)
                        {
                            srvOrdLinesWithModifiedSplits.Add(new SrvOrdLineWithSplits(srvOrdLine, schedule, plan.PlanQty));
                        }
                        else
                        {
                            srvOrdLineExt.AddUpdateSplit(schedule, plan.PlanQty);
                        }
                    }
                    else
                    {
                        serviceOrder.Current = (FSServiceOrder)PXParentAttribute.SelectParent(soDetSplit.Cache, schedule, typeof(FSServiceOrder));
                        schedule = PXCache<FSSODetSplit>.CreateCopy(schedule);

                        long? oldPlanID = schedule.PlanID;
                        ClearScheduleReferences(ref schedule);

                        if (stockItemProcessing == true)
                        {
                            // Called from INReleaseProcess (stock items)
                            schedule.IsAllocated = (plantype.ReplanOnEvent != INPlanConstants.Plan60);
                        }
                        else
                        {
                            // Called from POReceiptEntry (non-stock items)
                            plantype.ReplanOnEvent = INPlanConstants.PlanF1;
                            schedule.IsAllocated = false;
                        }

                        schedule.LotSerialNbr = plan.LotSerialNbr;
                        schedule.POCreate = false;
                        schedule.POSource = null;
                        schedule.POReceiptType = POReceiptType;
                        schedule.POReceiptNbr = POReceiptNbr;
                        schedule.SiteID = plan.SiteID;
                        schedule.VendorID = null;

                        schedule.BaseReceivedQty = 0m;
                        schedule.ReceivedQty = 0m;
                        schedule.BaseQty = plan.PlanQty;
                        schedule.Qty = INUnitAttribute.ConvertFromBase(soDetSplit.Cache, schedule.InventoryID, schedule.UOM, (decimal)schedule.BaseQty, INPrecision.QUANTITY);

                        //update SupplyPlanID in existing item plans (replenishment)
                        foreach (PXResult<INItemPlan> demand_res in PXSelect<INItemPlan,
                            Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>.Select(graph, oldPlanID))
                        {
                            INItemPlan demand_plan = PXCache<INItemPlan>.CreateCopy(demand_res);
                            initemplan.Cache.SetStatus(demand_plan, PXEntryStatus.Notchanged);
                            demand_plan.SupplyPlanID = plan.PlanID;
                            initemplan.Cache.Update(demand_plan);
                        }

                        schedule.PlanID = plan.PlanID;

                        schedule = (FSSODetSplit)soDetSplit.Cache.Insert(schedule);
                        insertedSchedules.Add(schedule);

                        // Split is being inserted with the Receipt reference
                        srvOrdLine = (FSSODet)PXParentAttribute.SelectParent(soDetSplit.Cache, schedule, typeof(FSSODet));
                        if (srvOrdLine == null)
                        {
                            throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSSODet)));
                        }

                        srvOrdLineExt = srvOrdLinesWithModifiedSplits.Find(e => e.SrvOrdLine.SODetID == srvOrdLine.SODetID);
                        if (srvOrdLineExt == null)
                        {
                            srvOrdLinesWithModifiedSplits.Add(new SrvOrdLineWithSplits(srvOrdLine, schedule, schedule.BaseQty));
                        }
                        else
                        {
                            srvOrdLineExt.AddUpdateSplit(schedule, schedule.BaseQty);
                        }
                    }
                }
                else if (plan.DemandPlanID == null)
                {
                    //Original schedule Marked for PO
                    //TODO: AC-142850 verify this is sufficient for Original SO marked for TR.
                    schedule = PXSelect<FSSODetSplit, Where<FSSODetSplit.planID, Equal<Required<FSSODetSplit.planID>>, And<FSSODetSplit.completed, Equal<False>>>>.Select(graph, plan.PlanID);
                    if (schedule != null)
                    {
                        includeInReturnList = false;
                        soDetSplit.Cache.SetStatus(schedule, PXEntryStatus.Notchanged);
                        schedule = PXCache<FSSODetSplit>.CreateCopy(schedule);

                        schedule.Completed = true;
                        schedule.POCompleted = true;
                        splitsToDeletePlanID.Add(schedule);
                        schedule = (FSSODetSplit)soDetSplit.Cache.Update(schedule);

                        // Split is being completed
                        srvOrdLine = (FSSODet)PXParentAttribute.SelectParent(soDetSplit.Cache, schedule, typeof(FSSODet));
                        if (srvOrdLine == null)
                        {
                            throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSSODet)));
                        }

                        srvOrdLineExt = srvOrdLinesWithModifiedSplits.Find(e => e.SrvOrdLine.SODetID == srvOrdLine.SODetID);
                        if (srvOrdLineExt == null)
                        {
                            srvOrdLinesWithModifiedSplits.Add(new SrvOrdLineWithSplits(srvOrdLine, schedule, 0m));
                        }
                        else
                        {
                            srvOrdLineExt.AddUpdateSplit(schedule, 0m);
                        }

                        INItemPlan origplan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(graph, plan.PlanID);
                        deletedPlans.Add(origplan);

                        initemplan.Cache.Delete(origplan);
                    }
                }

                if (includeInReturnList == true)
                {
                    returnList.Add(res);
                }
                else
                {
                    if (plantype.ReplanOnEvent != null)
                    {
                        plan.PlanType = plantype.ReplanOnEvent;
                        plan.SupplyPlanID = null;
                        plan.DemandPlanID = null;
                        initemplan.Cache.Update(plan);
                    }
                    else if (plantype.DeleteOnEvent == true)
                    {
                        initemplan.Delete(plan);
                    }
                }
            }

            //Create new schedules for partially received schedules marked for PO.
            FSSODetSplit prevSplit = null;

            foreach (FSSODetSplit newsplit in insertedSchedules)
            {
                if (prevSplit != null && prevSplit.SrvOrdType == newsplit.SrvOrdType && prevSplit.RefNbr == newsplit.RefNbr
                    && prevSplit.LineNbr == newsplit.LineNbr && prevSplit.InventoryID == newsplit.InventoryID
                    && prevSplit.SubItemID == newsplit.SubItemID && prevSplit.ParentSplitLineNbr == newsplit.ParentSplitLineNbr
                    && prevSplit.LotSerialNbr != null && newsplit.LotSerialNbr != null)
                    continue;

                FSSODetSplit parentschedule = PXSelect<FSSODetSplit,
                                              Where<
                                                  FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                                  And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                                  And<FSSODetSplit.lineNbr, Equal<Required<FSSODetSplit.lineNbr>>,
                                                  And<FSSODetSplit.splitLineNbr, Equal<Required<FSSODetSplit.parentSplitLineNbr>>>>>>>
                                              .Select(graph, newsplit.SrvOrdType, newsplit.RefNbr, newsplit.LineNbr, newsplit.ParentSplitLineNbr);

                if (parentschedule != null
                    && parentschedule.Completed == true
                    && parentschedule.POCompleted == true
                    && parentschedule.BaseQty > parentschedule.BaseReceivedQty
                    && deletedPlans.Exists(x => x.PlanID == parentschedule.PlanID))
                {
                    serviceOrder.Current = (FSServiceOrder)PXParentAttribute.SelectParent(soDetSplit.Cache, parentschedule, typeof(FSServiceOrder));

                    parentschedule = PXCache<FSSODetSplit>.CreateCopy(parentschedule);
                    INItemPlan demand = PXCache<INItemPlan>.CreateCopy(deletedPlans.First(x => x.PlanID == parentschedule.PlanID));

                    UpdateSchedulesFromCompletedPO(graph, soDetSplit, initemplan, parentschedule, serviceOrder, demand);
                }

                prevSplit = newsplit;
            }

            // Update POCompleted, POReceipt info and set Lot/Serials in Appointments
            foreach (SrvOrdLineWithSplits lineExt in srvOrdLinesWithModifiedSplits)
            {
                UpdatePOReceiptInfoInAppointments(graph, lineExt, soDetSplit, appointmentView, apptLineView, apptLineSplitView, false);
            }

            //Added because of MySql AutoIncrement counters behavior
            foreach (FSSODetSplit split in splitsToDeletePlanID)
            {
                FSSODetSplit schedule = (FSSODetSplit)soDetSplit.Cache.Locate(split);
                if (schedule != null)
                {
                    schedule.PlanID = null;
                    soDetSplit.Cache.Update(schedule);
                }
            }

            return returnList;
        }

        public static void UpdatePOReceiptInfoInAppointmentsStatic(PXGraph graph, SrvOrdLineWithSplits srvOrdLineExt, PXSelect<FSSODetSplit> soDetSplitView, PXSelect<FSAppointment> appointmentView, PXSelect<FSAppointmentDet> apptLineView, PXSelect<FSApptLineSplit> apptLineSplitView, bool ignoreCurrentAppointmentAllocation)
            => SingleFSProcessPOReceipt.UpdatePOReceiptInfoInAppointments(graph, srvOrdLineExt, soDetSplitView, appointmentView, apptLineView, apptLineSplitView, ignoreCurrentAppointmentAllocation);
		public virtual void UpdatePOReceiptInfoInAppointments(PXGraph graph, SrvOrdLineWithSplits srvOrdLineExt, PXSelect<FSSODetSplit> soDetSplitView, PXSelect<FSAppointment> appointmentView, PXSelect<FSAppointmentDet> apptLineView, PXSelect<FSApptLineSplit> apptLineSplitView, bool ignoreCurrentAppointmentAllocation)
		{
			PXCache serviceOrderCache = graph.Caches[typeof(FSServiceOrder)];
			PXCache soDetCache = graph.Caches[typeof(FSSODet)];

			var srvOrdTypeRow = FSSrvOrdType.PK.Find(graph, srvOrdLineExt.SrvOrdLine.SrvOrdType);
			if (srvOrdTypeRow == null)
			{
				throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSSrvOrdType)));
			}

			// This flag is ignored in this process for consistency
			//bool addLotSerialToAppointments = (bool)srvOrdTypeRow.SetLotSerialNbrInAppts;
			bool addLotSerialToAppointments = true;

			var item = InventoryItem.PK.Find(graph, srvOrdLineExt.SrvOrdLine.InventoryID);
			if (item == null)
			{
				throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(InventoryItem)));
			}

			decimal baseMarkForPOQty = 0m;
			decimal baseTotalReceivedShippedQty = 0m;
			var srvOrdLineSplitCopyList = new List<FSSODetSplit>();

			// Calculate Total MarkForPO Qty, Total Received/Shipped Qty, and prepare CopyList
			foreach (FSSODetSplit srvOrdSplit in PXParentAttribute.SelectChildren(soDetSplitView.Cache, srvOrdLineExt.SrvOrdLine, typeof(FSSODet)).RowCast<FSSODetSplit>().OrderBy(e => e.SplitLineNbr))
			{
				if (srvOrdSplit.POCreate == true)
				{
					baseMarkForPOQty += (decimal)srvOrdSplit.BaseQty;
					baseTotalReceivedShippedQty += (decimal)srvOrdSplit.BaseReceivedQty + (decimal)srvOrdSplit.BaseShippedQty;
				}
				else if (ignoreCurrentAppointmentAllocation == true)
				{
					if (srvOrdSplit.BaseQty > 0m)
					{
						srvOrdLineSplitCopyList.Add((FSSODetSplit)soDetSplitView.Cache.CreateCopy(srvOrdSplit));
					}
				}
			}

			decimal baseQtyBalanceToAllocate = 0m;

			if (ignoreCurrentAppointmentAllocation == false)
			{
				foreach (SplitWithAdjustQty srvOrdSplit in srvOrdLineExt.Splits.OrderBy(e => e.Split.SplitLineNbr))
				{
					if (srvOrdSplit.Split.POCreate == false)
					{
						srvOrdLineSplitCopyList.Add((FSSODetSplit)soDetSplitView.Cache.CreateCopy(srvOrdSplit.Split));
					}
				}

				baseQtyBalanceToAllocate = srvOrdLineExt.BaseNewReceivedShippedQty;
			}
			else
			{
				baseQtyBalanceToAllocate = baseTotalReceivedShippedQty;
			}

			var apptLines = GetRelatedApptLines(graph, srvOrdLineExt.SrvOrdLine.SODetID, excludeSpecificApptLine: false, apptDetID: null, onlyMarkForPOLines: true, sortResult: true);

			foreach (FSAppointmentDet apptLine in apptLines.OrderBy(x => x.TranDate).
																ThenBy(x => x.AppointmentID).
																ThenBy(x => x.AppDetID))
			{
				apptLineView.Current = apptLine;
				appointmentView.Current = (FSAppointment)PXParentAttribute.SelectParent(apptLineView.Cache, apptLine, typeof(FSAppointment));
				if (appointmentView.Current == null)
				{
					throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSAppointment)));
				}

				if (appointmentView.Current.Canceled == true)
				{
					continue;
				}

				FSAppointmentDet apptLineCopy = (FSAppointmentDet)apptLineView.Cache.CreateCopy(apptLineView.Current);

				var apptSplitLines = PXParentAttribute.SelectChildren(apptLineSplitView.Cache, apptLineView.Current, typeof(FSAppointmentDet)).RowCast<FSApptLineSplit>().ToList();
				int splitWithLotSerialCount = apptSplitLines.Where(x => string.IsNullOrEmpty(x.LotSerialNbr) == false).Count();

				if (ignoreCurrentAppointmentAllocation == false)
				{
					if (appointmentView.Current.Status == FSAppointment.status.Values.Closed)
					{
						continue;
					}
				}
				else
				{
					// **********************************************************************************
					// This block is to delete the old allocation from the appointment line and its split
					// ----------------------------------------------------------------------------------

					// Currently FieldService doesn't manage DropShip
					// but there is a task to add it in 2021R2
					string poSource = INReplenishmentSource.PurchaseToOrder;

					if (poSource == INReplenishmentSource.PurchaseToOrder)
					{
						List<FSApptLineSplit> usedApptSplits = PXParentAttribute.SelectChildren(apptLineSplitView.Cache, srvOrdLineExt.SrvOrdLine, typeof(FSAppointmentDet)).RowCast<FSApptLineSplit>().OrderBy(e => e.SplitLineNbr).ToList();
						UpdateAvailableQtyOfUsedSplits(srvOrdLineSplitCopyList, usedApptSplits, ref baseQtyBalanceToAllocate);
					}
					else if (poSource == INReplenishmentSource.DropShipToOrder)
					{
						baseQtyBalanceToAllocate -= (decimal)apptLineCopy.BaseAllocatedFromSrvOrdPOQty;
					}
					else
					{
						throw new NotImplementedException();
					}

					// Allocation in Completed/Closed appointments must be kept
					if (appointmentView.Current.Status.IsIn(
											FSAppointment.status.Values.Completed,
											FSAppointment.status.Values.Closed))
					{
						continue;
					}

					apptLineCopy.BaseAllocatedFromSrvOrdPOQty = 0m;

					foreach (FSApptLineSplit split in apptSplitLines)
					{
						/*
                        // If this field is different from null,
                        // then this is an allocation from ServiceOrder automatically generated.
                        // TODO: Consider to add a flag for this.
                        if (split.POReceiptNbr != null)
                        {
                            apptLineSplitView.Delete(split);
                        }
                        */

						apptLineCopy.BaseAllocatedFromSrvOrdPOQty += split.BaseQty;
					}

					apptLineCopy.AllocatedFromSrvOrdPOQty = INUnitAttribute.ConvertFromBase(apptLineView.Cache, apptLineCopy.InventoryID, apptLineCopy.UOM, (decimal)apptLineCopy.BaseAllocatedFromSrvOrdPOQty, INPrecision.QUANTITY);

					if (apptLineCopy.BaseEffTranQty < apptLineCopy.BaseAllocatedFromSrvOrdPOQty)
					{
						apptLineCopy.Status = FSAppointmentDet.status.WaitingForPO;
					}

					apptLineCopy = apptLineView.Update(apptLineCopy);
					apptLineCopy = (FSAppointmentDet)apptLineView.Cache.CreateCopy(apptLineCopy);

					// **********************************************************************************
				}

				decimal basePendingAllocationQty = (decimal)apptLineCopy.BaseEffTranQty - (decimal)apptLineCopy.BaseAllocatedFromSrvOrdPOQty;
				decimal baseNewAllocationQty = 0m;

				if (baseQtyBalanceToAllocate > basePendingAllocationQty)
				{
					baseNewAllocationQty = basePendingAllocationQty;
					baseQtyBalanceToAllocate -= baseNewAllocationQty;
				}
				else
				{
					baseNewAllocationQty = baseQtyBalanceToAllocate;
					baseQtyBalanceToAllocate = 0m;
				}

				string lastLotSerialNbr = null;

				decimal baseNewAllocationQtyToAppointmentDet = baseNewAllocationQty;

				if (addLotSerialToAppointments == true && baseNewAllocationQty > 0)
				{
					foreach (FSSODetSplit soDetSplit in srvOrdLineSplitCopyList.Where(e => e.BaseQty > 0m))
					{
						FSApptLineSplit newSplit = FSAppointmentLineSplittingExtension.StaticConvert(apptLineCopy);
						newSplit.BaseQty = 0;

						newSplit = (FSApptLineSplit)apptLineSplitView.Cache.Insert(newSplit);

						PXDefaultAttribute.SetPersistingCheck<FSApptLineSplit.locationID>(apptLineSplitView.Cache, newSplit, apptLineCopy.LineType == ID.LineType_ALL.NONSTOCKITEM ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

						newSplit = (FSApptLineSplit)apptLineSplitView.Cache.CreateCopy(newSplit);

						FillLotSerialAndPOFields(newSplit, soDetSplit);

						if (string.IsNullOrEmpty(newSplit.LotSerialNbr) == false)
						{
							splitWithLotSerialCount++;
							lastLotSerialNbr = newSplit.LotSerialNbr;
						}

						var poLine = POLine.PK.Find(graph, apptLineCopy.POType, apptLineCopy.PONbr, apptLineCopy.POLineNbr);
						var poReceiptLineRow = SelectFrom<POReceiptLine>
							.Where<POReceiptLine.receiptNbr.IsEqual<P.AsString>
								.And<POReceiptLine.receiptType.IsEqual<P.AsString>
								.And<POReceiptLine.pOLineNbr.IsEqual<P.AsInt>>>>
							.View
							.SelectSingleBound(graph, null, newSplit.POReceiptNbr, newSplit.POReceiptType, poLine.LineNbr)
							.TopFirst;

						newSplit.ExpireDate = poReceiptLineRow.ExpireDate;
						newSplit.BaseQty = baseNewAllocationQty < soDetSplit.BaseQty ? baseNewAllocationQty : soDetSplit.BaseQty;
						newSplit.Qty = INUnitAttribute.ConvertFromBase(apptLineSplitView.Cache, newSplit.InventoryID, newSplit.UOM, (decimal)newSplit.BaseQty, INPrecision.QUANTITY);

						newSplit = (FSApptLineSplit)apptLineSplitView.Cache.Update(newSplit);

						soDetSplit.BaseQty -= (decimal)newSplit.BaseQty;
						baseNewAllocationQty -= (decimal)newSplit.BaseQty;

						if (baseNewAllocationQty <= 0m)
						{
							break;
						}
					}
				}

				apptLineCopy.BaseAllocatedFromSrvOrdPOQty += baseNewAllocationQtyToAppointmentDet;
				apptLineCopy.AllocatedFromSrvOrdPOQty = INUnitAttribute.ConvertFromBase(apptLineView.Cache, apptLineCopy.InventoryID, apptLineCopy.UOM, (decimal)apptLineCopy.BaseAllocatedFromSrvOrdPOQty, INPrecision.QUANTITY);

				string oldApptLineStatus = apptLineCopy.Status;
				UpdateLineStatusBasedOnReceivedPurchaseItems(appointmentView.Current, apptLineView.Cache, apptLineCopy, MustHaveRequestPOStatus(apptLineCopy), null, apptLineCopy.BaseAllocatedFromSrvOrdPOQty, apptLineCopy.AllocatedFromSrvOrdPOQty, false);

				// Only fill the LotSerial and the PO reference in ApptLine
				// when the requirement is fully covered and there is only 1 split line
				if (splitWithLotSerialCount == 1)
				{
					apptLineCopy.LotSerialNbr = lastLotSerialNbr;
				}
				else
				{
					apptLineCopy.LotSerialNbr = null;
				}

				apptLineCopy = (FSAppointmentDet)apptLineView.Cache.Update(apptLineCopy);

				//TODO: AC-169443 if you dont persist here only latest FSAppointmentDet changes are persisted
				// and we need to reset persisted because line migth be updated again in UpdatePOReferenceInSrvOrdLineInt.
				if ((graph is POOrderEntry) == false)
				{
					apptLineView.Cache.Persist(apptLineCopy, PXDBOperation.Update);
				}

				FSServiceOrder serviceOrder = null;
				FSSODet soDet = null;
				if (UpdateFSSODetStatus(apptLineCopy, oldApptLineStatus, serviceOrderCache, soDetCache, out serviceOrder, out soDet) == true)
				{
					if ((graph is POOrderEntry) == false)
					{
						soDetCache.Persist(soDet, PXDBOperation.Update);
					}
				}
			}
		}

		public static bool UpdateFSSODetStatus(FSAppointmentDet apptDet, string oldApptDetStatus, PXCache serviceOrderCache, PXCache soDetCache, out FSServiceOrder serviceOrder, out FSSODet soDet)
        {
            soDet = null;
            serviceOrder = null;

            if (apptDet.Status != oldApptDetStatus)
            {
                if (apptDet.Status == FSAppointmentDet.status.COMPLETED
                    && apptDet.EnablePO == true
                    && apptDet.POSource == FSAppointmentDet.pOSource.Values.PurchaseToAppointment)
                {
                    soDet = PXSelect<FSSODet,
                                Where<FSSODet.srvOrdType, Equal<Required<FSSODet.srvOrdType>>,
                                    And<FSSODet.refNbr, Equal<Required<FSSODet.refNbr>>,
                                    And<FSSODet.lineNbr, Equal<Required<FSSODet.lineNbr>>>>>>
                            .Select(soDetCache.Graph, apptDet.SrvOrdType, apptDet.OrigSrvOrdNbr, apptDet.OrigLineNbr);

                    if (soDet == null)
                    {
                        throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSSODet)));
                    }

                    serviceOrder = PXSelect<FSServiceOrder,
                                Where<FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>,
                                    And<FSServiceOrder.refNbr, Equal<Required<FSServiceOrder.refNbr>>>>>
                            .Select(serviceOrderCache.Graph, soDet.SrvOrdType, soDet.RefNbr);

                    if (serviceOrder == null)
                    {
                        throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSServiceOrder)));
                    }

                    serviceOrderCache.Current = serviceOrder;

                    soDet = (FSSODet)soDetCache.CreateCopy(soDet);

                    soDet.Status = ID.Status_SODet.COMPLETED;

                    soDet = (FSSODet)soDetCache.Update(soDet);

                    return true;
                }
            }

            return false;
        }

        protected virtual void UpdateAvailableQtyOfUsedSplits(List<FSSODetSplit> srvOrdLineSplitCopyList, List<FSApptLineSplit> usedApptSplits, ref decimal baseQtyBalanceToAllocate)
        {
            foreach (FSApptLineSplit usedApptSplit in usedApptSplits)
            {
                /*
                if (usedApptSplit.POReceiptType == null || usedApptSplit.POReceiptNbr == null)
                {
                    continue;
                }
                */

                int index = srvOrdLineSplitCopyList.FindIndex(e =>
                        (
                            usedApptSplit.POReceiptType != null && usedApptSplit.POReceiptNbr != null
                            && e.POReceiptType == usedApptSplit.POReceiptType && e.POReceiptNbr == usedApptSplit.POReceiptNbr
                            && string.Equals(e.LotSerialNbr, usedApptSplit.LotSerialNbr, StringComparison.OrdinalIgnoreCase)
                        ) || (
                            usedApptSplit.POReceiptType == null && usedApptSplit.POReceiptNbr == null
                            && string.IsNullOrEmpty(e.LotSerialNbr) == false
                            && string.Equals(e.LotSerialNbr, usedApptSplit.LotSerialNbr, StringComparison.OrdinalIgnoreCase))
                        );
                if (index > -1)
                {
                    FSSODetSplit srvOrdSplit = srvOrdLineSplitCopyList[index];

                    if (srvOrdSplit.BaseQty > usedApptSplit.BaseQty)
                    {
                        baseQtyBalanceToAllocate -= (decimal)srvOrdSplit.BaseQty - (decimal)usedApptSplit.BaseQty;
                        srvOrdSplit.BaseQty -= usedApptSplit.BaseQty;
                    }
                    else
                    {
                        baseQtyBalanceToAllocate -= (decimal)srvOrdSplit.BaseQty;
                        srvOrdSplit.BaseQty = 0m;
                    }

                    if (srvOrdSplit.BaseQty <= 0m)
                    {
                        srvOrdLineSplitCopyList.RemoveAt(index);
                    }
                }
            }
        }

        public virtual void FillLotSerialAndPOFields(FSApptLineSplit split, FSSODetSplit soDetSplit)
        {
            FillLotSerialAndPOFieldsStatic(split, soDetSplit);

            split.POSource = null;

            split.SiteID = soDetSplit.SiteID;
            split.VendorID = null;
        }

        public static void FillLotSerialAndPOFieldsStatic(FSApptLineSplit split, FSSODetSplit soDetSplit)
        {
            split.LotSerialNbr = soDetSplit.LotSerialNbr;

            split.POCreate = false;
            split.POReceiptType = soDetSplit.POReceiptType;
            split.POReceiptNbr = soDetSplit.POReceiptNbr;

            split.OrigSrvOrdType = soDetSplit.SrvOrdType;
            split.OrigSrvOrdNbr = soDetSplit.RefNbr;
            split.OrigLineNbr = soDetSplit.LineNbr;
            split.OrigSplitLineNbr = soDetSplit.SplitLineNbr;
        }

        public static void UpdateSchedulesFromCompletedPOStatic(PXGraph graph, PXSelect<FSSODetSplit> fsSODetSplit, PXSelect<INItemPlan> initemplan, FSSODetSplit parentschedule, PXSelect<FSServiceOrder> fsServiceOrder, INItemPlan demand)
            => SingleFSProcessPOReceipt.UpdateSchedulesFromCompletedPO(graph, fsSODetSplit, initemplan, parentschedule, fsServiceOrder, demand);

        public virtual void UpdateSchedulesFromCompletedPO(PXGraph graph, PXSelect<FSSODetSplit> fsSODetSplit, PXSelect<INItemPlan> initemplan, FSSODetSplit parentschedule, PXSelect<FSServiceOrder> fsServiceOrder, INItemPlan demand)
        {
            graph.FieldDefaulting.AddHandler<FSSODetSplit.locationID>((sender, e) =>
            {
                if (e.Row != null && ((FSSODetSplit)e.Row).RequireLocation != true)
                {
                    e.NewValue = null;
                    e.Cancel = true;
                }
            });

            FSSODetSplit newschedule = PXCache<FSSODetSplit>.CreateCopy(parentschedule);

            ClearScheduleReferences(ref newschedule);

            newschedule.LotSerialNbr = demand.LotSerialNbr;
            newschedule.SiteID = demand.SiteID;

            newschedule.BaseQty = parentschedule.BaseQty - parentschedule.BaseReceivedQty;
            newschedule.Qty = INUnitAttribute.ConvertFromBase(fsSODetSplit.Cache, newschedule.InventoryID, newschedule.UOM, (decimal)newschedule.BaseQty, INPrecision.QUANTITY);
            newschedule.BaseReceivedQty = 0m;
            newschedule.ReceivedQty = 0m;

            //creating new plan
            INItemPlan newPlan = PXCache<INItemPlan>.CreateCopy(demand);
            newPlan.PlanID = null;
            newPlan.SupplyPlanID = null;
            newPlan.DemandPlanID = null;
            newPlan.PlanQty = newschedule.BaseQty;
            newPlan.VendorID = null;
            newPlan.VendorLocationID = null;
            newPlan.FixedSource = INReplenishmentSource.None;
            newPlan.PlanType = (fsServiceOrder.Current != null && fsServiceOrder.Current.Hold == true) ? INPlanConstants.PlanF0 : INPlanConstants.PlanF1;
            newPlan = (INItemPlan)initemplan.Cache.Insert(newPlan);

			//initemplan.Cache.Persist(newPlan, PXDBOperation.Insert);

            newschedule.PlanID = newPlan.PlanID;
            fsSODetSplit.Cache.Insert(newschedule);
        }

        public static void ClearScheduleReferences(ref FSSODetSplit schedule)
        {
            schedule.ParentSplitLineNbr = schedule.SplitLineNbr;
            schedule.SplitLineNbr = null;
            schedule.Completed = false;
            schedule.PlanID = null;

            ClearPOFlags(schedule);
            ClearPOReferences(schedule);
            schedule.POSource = INReplenishmentSource.None;

            ClearSOReferences(schedule);

            schedule.RefNoteID = null;
        }

        public static void ClearPOFlags(FSSODetSplit split)
        {
            split.POCompleted = false;
            split.POCancelled = false;

            split.POCreate = false;
            split.POSource = null;
        }

        public static void ClearPOReferences(FSSODetSplit split)
        {
            split.POType = null;
            split.PONbr = null;
            split.POLineNbr = null;

            split.POReceiptType = null;
            split.POReceiptNbr = null;
        }

        public static void ClearSOReferences(FSSODetSplit split)
        {
            split.SOOrderType = null;
            split.SOOrderNbr = null;
            split.SOLineNbr = null;
            split.SOSplitLineNbr = null;
        }

		

		#region SrvOrdLineWithSplits
		public class SrvOrdLineWithSplits
        {
            public FSSODet SrvOrdLine = null;
            public List<SplitWithAdjustQty> Splits = null;

            protected decimal _baseNewReceivedShippedQty = 0m;

            public decimal BaseNewReceivedShippedQty { get { return _baseNewReceivedShippedQty; } }

            public SrvOrdLineWithSplits(FSSODet srvOrdLine, FSSODetSplit split, decimal? baseReceivedShippedAdjustQty)
            {
                SrvOrdLine = srvOrdLine;
                Splits = new List<SplitWithAdjustQty>();

                Splits.Add(new SplitWithAdjustQty(split, baseReceivedShippedAdjustQty));

                AccumulateQuantities(baseReceivedShippedAdjustQty);
            }

            public virtual void AddUpdateSplit(FSSODetSplit split, decimal? baseReceivedShippedAdjustQty)
            {
                var existingSplit = Splits.Find(e => e.Split.SrvOrdType == split.SrvOrdType
                                && e.Split.RefNbr == split.RefNbr
                                && e.Split.LineNbr == split.LineNbr
                                && e.Split.SplitLineNbr == split.SplitLineNbr);

                if (existingSplit != null)
                {
                    AccumulateQuantities(-existingSplit.AdjustQty);
                    Splits.Remove(existingSplit);
                }

                Splits.Add(new SplitWithAdjustQty(split, baseReceivedShippedAdjustQty));

                AccumulateQuantities(baseReceivedShippedAdjustQty);
            }

            protected virtual void AccumulateQuantities(decimal? baseReceivedShippedAdjustQty)
            {
                _baseNewReceivedShippedQty += (decimal)baseReceivedShippedAdjustQty;
            }
        }
        #endregion

        #region SplitWithAdjustQty
        public class SplitWithAdjustQty
        {
            public FSSODetSplit Split = null;
            public decimal AdjustQty = 0m;

            public SplitWithAdjustQty(FSSODetSplit split, decimal? adjustQty)
            {
                Split = split;
                AdjustQty = (decimal)adjustQty;
            }
        }
        #endregion

        public static void UpdateSrvOrdLinePOStatus(PXGraph graph, POOrder poOrderRow)
            => SingleFSProcessPOReceipt.UpdateSrvOrdLinePOStatusInt(graph, poOrderRow);

        public virtual void UpdateSrvOrdLinePOStatusInt(PXGraph graph, POOrder poOrderRow)
        {
            // Update POStatus in service order lines
            PXCache soLineCache = graph.Caches[typeof(FSSODet)];
            PXCache apptLineCache = graph.Caches[typeof(FSAppointmentDet)];
            PXCache appointmentCache = graph.Caches[typeof(FSAppointment)];

            var relatedSOLines = PXSelect<FSSODet,
                    Where<
                        FSSODet.poType, Equal<Required<FSSODet.poType>>,
                        And<FSSODet.poNbr, Equal<Required<FSSODet.poNbr>>>>>
                    .Select(graph, poOrderRow.OrderType, poOrderRow.OrderNbr);

            foreach (FSSODet soLine in relatedSOLines)
            {
                FSSODet soLineCopy = (FSSODet)soLineCache.CreateCopy(soLine);

                soLineCopy.POStatus = poOrderRow.Status;

                soLineCopy = (FSSODet)soLineCache.Update(soLineCopy);

                // Update POStatus in related appointment lines
                var apptLines = GetRelatedApptLines(graph, soLineCopy.SODetID, excludeSpecificApptLine: false, apptDetID: null, onlyMarkForPOLines: false, sortResult: false);
                int? currentAppointmentID = null;
                foreach (FSAppointmentDet apptLine in apptLines.OrderBy(x=> x.AppointmentID))
                {
                    if (currentAppointmentID == null || currentAppointmentID != apptLine.AppointmentID)
                    {
                        appointmentCache.Current = (FSAppointment)PXParentAttribute.SelectParent(apptLineCache, apptLine, typeof(FSAppointment));
                        currentAppointmentID = apptLine.AppointmentID;
                    }

                    FSAppointmentDet apptLineCopy = (FSAppointmentDet)apptLineCache.CreateCopy(apptLine);

                    apptLineCopy.POStatus = poOrderRow.Status;

                    apptLineCopy = (FSAppointmentDet)apptLineCache.Update(apptLineCopy);
                    apptLineCache.Persist(apptLineCopy, PXDBOperation.Update);
                }

                soLineCache.Persist(soLineCopy, PXDBOperation.Update);
			}
		}


		public static void UpdateSrvOrdApptLineUnitCost(PXGraph graph, POOrder poOrderRow)
		   => SingleFSProcessPOReceipt.UpdateSrvOrdApptLineUnitCostInt(graph, poOrderRow);

		public virtual void UpdateSrvOrdApptLineUnitCostInt(PXGraph graph, POOrder poOrderRow)
		{
			PXCache soLineCache = graph.Caches[typeof(FSSODet)];
			PXCache apptLineCache = graph.Caches[typeof(FSAppointmentDet)];

			if (soLineCache.IsDirty)
				soLineCache.Graph.Actions.PressSave();

			if (apptLineCache.IsDirty)
				apptLineCache.Graph.Actions.PressSave();

			foreach (PXResult<FSSODet, POLine> Line in SelectFrom<FSSODet>
						.InnerJoin<POLine>.On<POLine.orderType.IsEqual<FSSODet.poType>
							.And<POLine.orderNbr.IsEqual<FSSODet.poNbr>>
							.And<POLine.lineNbr.IsEqual<FSSODet.poLineNbr>>>
						.Where<FSSODet.poType.IsEqual<@P.AsString>
							.And<FSSODet.poNbr.IsEqual<@P.AsString>>
							.And<POLine.curyUnitCost.IsNotEqual<FSSODet.curyUnitCost>>
							.And<Brackets<FSSODet.lineType.IsEqual<FSLineType.NonStockItem>
								.Or<FSSODet.lineType.IsEqual<FSLineType.Service>>>>>
				.View
				.Select(graph, poOrderRow.OrderType, poOrderRow.OrderNbr))
			{
				FSSODet soLine = (FSSODet)Line;
				POLine poLine = (POLine)Line;
				FSSODet soLineCopy = (FSSODet)soLineCache.CreateCopy(soLine);
					
				soLineCache.SetValueExt<FSSODet.curyUnitCost>(soLineCopy, poLine.CuryUnitCost);
				soLineCache.SetValueExt<FSSODet.unitCost>(soLineCopy, poLine.UnitCost);
				soLineCopy = (FSSODet)soLineCache.Update(soLineCopy);
				soLineCache.Persist(soLineCopy, PXDBOperation.Update);

				foreach (PXResult<FSAppointmentDet> apptLine in SelectFrom<FSAppointmentDet>
					.Where<FSAppointmentDet.sODetID.IsEqual<@P.AsInt>>
				.View
				.Select(graph, soLineCopy.SODetID))
				{
					FSAppointmentDet aLine = (FSAppointmentDet)apptLine;
					FSAppointmentDet apptLineCopy = (FSAppointmentDet)apptLineCache.CreateCopy(aLine);

					apptLineCache.SetValueExt<FSAppointmentDet.curyUnitCost>(apptLineCopy, poLine.CuryUnitCost);
					apptLineCache.SetValueExt<FSAppointmentDet.unitCost>(apptLineCopy, poLine.UnitCost);
					apptLineCopy = (FSAppointmentDet)apptLineCache.Update(apptLineCopy);
					apptLineCache.Persist(apptLineCopy, PXDBOperation.Update);
				}
			}
		}


		public static void UpdatePOReferenceInSrvOrdLine(PXGraph graph,
                                                            PXSelectBase<FSSODet> fsSODetFixedDemand,
                                                            PXSelectBase<FSSODetSplit> fsSODetSplitFixedDemand,
                                                            PXSelectBase<FSAppointment> appointmentView,
                                                            PXSelectBase<FSAppointmentDet> appointmentLineView,
                                                            FSSODet fsSODet,
                                                            POOrder poOrder,
                                                            int? poLineNbr,
                                                            bool? poLineCompleted,
                                                            PXCache inItemPlanCache,
                                                            INItemPlan inItemPlan,
                                                            bool clearPOReference)
       => SingleFSProcessPOReceipt.UpdatePOReferenceInSrvOrdLineInt(
                                                            graph,
                                                            fsSODetFixedDemand,
                                                            fsSODetSplitFixedDemand,
                                                            appointmentView,
                                                            appointmentLineView,
                                                            fsSODet,
                                                            poOrder,
                                                            poLineNbr,
                                                            poLineCompleted,
                                                            inItemPlanCache,
                                                            inItemPlan,
                                                            clearPOReference);
        public virtual void UpdatePOReferenceInSrvOrdLineInt(PXGraph graph,
                                                            PXSelectBase<FSSODet> fsSODetFixedDemand,
                                                            PXSelectBase<FSSODetSplit> fsSODetSplitFixedDemand,
                                                            PXSelectBase<FSAppointment> appointmentView,
                                                            PXSelectBase<FSAppointmentDet> appointmentLineView,
                                                            FSSODet fsSODet,
                                                            POOrder poOrder,
                                                            int? poLineNbr,
                                                            bool? poLineCompleted,
                                                            PXCache inItemPlanCache,
                                                            INItemPlan inItemPlan,
                                                            bool clearPOReference)
        {
            if (fsSODet == null)
                return;

            string newPOType = null;
            string newPONbr = null;
            int? newPOLineNbr = null;
            string newPOStatus = null;
            int? newPOVendorID = null;
            int? newPOVendorLocationID = null;
            bool? newPOCompleted = false;

            if (clearPOReference == false)
            {
                newPOType = poOrder.OrderType;
                newPONbr = poOrder.OrderNbr;
                newPOLineNbr = poLineNbr;
                newPOStatus = poOrder.Status;
                newPOVendorID = poOrder.VendorID;
                newPOVendorLocationID = poOrder.VendorLocationID;
                newPOCompleted = poLineCompleted;
            }
            else
            {
                var inItemPlanCopy = (INItemPlan)inItemPlanCache.CreateCopy(inItemPlan);

                inItemPlanCopy.SupplyPlanID = null;

                inItemPlanCopy = (INItemPlan)inItemPlanCache.Update(inItemPlanCopy);                
            }

			var fsSODetCopy = (FSSODet)fsSODetFixedDemand.Cache.CreateCopy(fsSODet);
			if (fsSODetCopy.POType != newPOType ||
				fsSODetCopy.PONbr != newPONbr ||
				fsSODetCopy.POLineNbr != newPOLineNbr ||
				fsSODetCopy.POStatus != newPOStatus ||
				fsSODetCopy.POCompleted != newPOCompleted ||
				fsSODetCopy.POVendorID != newPOVendorID ||
				fsSODetCopy.POVendorLocationID != newPOVendorLocationID)
			{
				fsSODetCopy.POType = newPOType;
				fsSODetCopy.PONbr = newPONbr;
				fsSODetCopy.POLineNbr = newPOLineNbr;
				fsSODetCopy.POStatus = newPOStatus;
				fsSODetCopy.POCompleted = newPOCompleted;

				if (clearPOReference == false)
				{
					fsSODetCopy.POVendorID = newPOVendorID;
					fsSODetCopy.POVendorLocationID = newPOVendorLocationID;
				}

				fsSODetCopy = fsSODetFixedDemand.Update(fsSODetCopy);
			}

			// Update the related appointment lines
			var apptLines = GetRelatedApptLines(graph, fsSODet.SODetID,
                    excludeSpecificApptLine: false, apptDetID: null, onlyMarkForPOLines: false, sortResult: false);
            int? lastAppointment = null;

			foreach (FSAppointmentDet apptLine in apptLines.OrderBy(x => x.AppointmentID))
			{
				if (lastAppointment == null || lastAppointment != apptLine.AppointmentID)
				{
					appointmentView.Current = (FSAppointment)PXParentAttribute.SelectParent(appointmentLineView.Cache, apptLine, typeof(FSAppointment));
					lastAppointment = apptLine.AppointmentID;
				}

				var apptLineCopy = appointmentLineView.Current = (FSAppointmentDet)appointmentLineView.Cache.CreateCopy(apptLine);
				if (apptLineCopy.POType != newPOType ||
					 apptLineCopy.PONbr != newPONbr ||
					 apptLineCopy.POLineNbr != newPOLineNbr ||
					 apptLineCopy.POStatus != newPOStatus ||
					 apptLineCopy.POCompleted != newPOCompleted ||
					 apptLineCopy.POVendorID != newPOVendorID ||
					 apptLineCopy.POVendorLocationID != newPOVendorLocationID)
				{
					apptLineCopy.POType = newPOType;
					apptLineCopy.PONbr = newPONbr;
					apptLineCopy.POLineNbr = newPOLineNbr;
					apptLineCopy.POStatus = newPOStatus;
					apptLineCopy.POCompleted = newPOCompleted;
					apptLineCopy.tstamp = PXDatabase.SelectTimeStamp();

					if (clearPOReference == false)
					{
						apptLineCopy.POVendorID = newPOVendorID;
						apptLineCopy.POVendorLocationID = newPOVendorLocationID;
					}

					apptLineCopy = appointmentLineView.Update(apptLineCopy);

					//TODO: AC - 169443 if you dont persist here only latest appointment changes are persisted
					if ((graph is POOrderEntry) == false)
					{
						appointmentLineView.Cache.ResetPersisted(apptLineCopy);
						appointmentLineView.Cache.Persist(apptLineCopy, PXDBOperation.Update);
					}
				}
			}

            if (inItemPlan != null)
            {
                FSSODetSplit fsSODetSplit = fsSODetSplitFixedDemand.Select(inItemPlan.PlanID);

                if (fsSODetSplit != null)
                {
					var fsSODetSplitCopy = (FSSODetSplit)fsSODetSplitFixedDemand.Cache.CreateCopy(fsSODetSplit);
					if (fsSODetSplitCopy.POType != newPOType ||
						fsSODetSplitCopy.PONbr != newPONbr ||
						fsSODetSplitCopy.POLineNbr != newPOLineNbr ||
						fsSODetSplitCopy.POCompleted != newPOCompleted)
					{
						fsSODetSplitCopy.POType = newPOType;
						fsSODetSplitCopy.PONbr = newPONbr;
						fsSODetSplitCopy.POLineNbr = newPOLineNbr;
						fsSODetSplitCopy.POCompleted = newPOCompleted;

						fsSODetSplitCopy = fsSODetSplitFixedDemand.Update(fsSODetSplitCopy);
					}
				}
			}
        }

        public virtual void UpdateLineStatusBasedOnReceivedPurchaseItems(FSAppointment appt, PXCache sender, FSAppointmentDet row, bool rowMustHaveRequestPOStatus, List<FSApptLineSplit> existingSplits, decimal? baseExistingSplitTotalQty, decimal? existingSplitTotalQty, bool runSetValueExt)
            => UpdateLineStatusBasedOnReceivedPurchaseItemsStatic(appt, sender, row, rowMustHaveRequestPOStatus, existingSplits, baseExistingSplitTotalQty, existingSplitTotalQty, runSetValueExt);

        public static void UpdateLineStatusBasedOnReceivedPurchaseItemsStatic(FSAppointment appt, PXCache sender, FSAppointmentDet row, bool rowMustHaveRequestPOStatus, List<FSApptLineSplit> existingSplits, decimal? baseExistingSplitTotalQty, decimal? existingSplitTotalQty, bool runSetValueExt)
        {
            decimal? allocatedFromSrvOrdPOQty = row.AllocatedFromSrvOrdPOQty;

            InventoryItem item = SharedFunctions.GetInventoryItemRow(sender.Graph, row.InventoryID);
            FSxService service = PXCache<InventoryItem>.GetExtension<FSxService>(item);
            bool isTravelItem = service?.IsTravelItem == true ? true : false;

            decimal? baseAllocatedFromSrvOrdPOQty = row.BaseAllocatedFromSrvOrdPOQty;
            string newStatus = row.Status;

            if (row.EnablePO == false)
            {
                allocatedFromSrvOrdPOQty = 0m;
                baseAllocatedFromSrvOrdPOQty = 0m;

                newStatus = FSAppointmentDet.status.NOT_STARTED;
                if (appt.Completed == true
                    && appt.ReopenActionRunning == false
                    && isTravelItem == false)
                {
                    newStatus = FSAppointmentDet.status.COMPLETED;
                }
                else
                {
                    newStatus = FSAppointmentDet.status.NOT_STARTED;
                }
            }
            else
            {
                if (rowMustHaveRequestPOStatus == true)
                {
                    allocatedFromSrvOrdPOQty = 0m;
                    baseAllocatedFromSrvOrdPOQty = 0m;
                    newStatus = FSAppointmentDet.status.RequestForPO;
                }
                else
                {
                    allocatedFromSrvOrdPOQty = existingSplitTotalQty;
                    baseAllocatedFromSrvOrdPOQty = baseExistingSplitTotalQty;

                    if (baseExistingSplitTotalQty >= row.BaseEffTranQty || row.POCompleted == true)
                    {
                        if (appt.Completed == true
                            && appt.ReopenActionRunning == false 
                            && isTravelItem == false)
                        {
                            newStatus = FSAppointmentDet.status.COMPLETED;
                        }
                        else
                        {
                            newStatus = FSAppointmentDet.status.NOT_STARTED;
                        }
                    }
                    else
                    {
                        newStatus = FSAppointmentDet.status.WaitingForPO;
                    }
                }
            }

            if (baseAllocatedFromSrvOrdPOQty != row.BaseAllocatedFromSrvOrdPOQty)
            {
                if (runSetValueExt == true)
                {
                    sender.SetValueExt<FSAppointmentDet.baseAllocatedFromSrvOrdPOQty>(row, baseAllocatedFromSrvOrdPOQty);
                }
                else
                {
                    row.BaseAllocatedFromSrvOrdPOQty = baseAllocatedFromSrvOrdPOQty;
                }
            }

            if (allocatedFromSrvOrdPOQty != row.AllocatedFromSrvOrdPOQty)
            {
                if (runSetValueExt == true)
                {
                    sender.SetValueExt<FSAppointmentDet.allocatedFromSrvOrdPOQty>(row, allocatedFromSrvOrdPOQty);
                }
                else
                {
                    row.AllocatedFromSrvOrdPOQty = allocatedFromSrvOrdPOQty;
                }
            }

            if (newStatus != row.Status)
            {
                if (newStatus == FSAppointmentDet.status.NOT_STARTED
                    && row.Status != FSAppointmentDet.status.WaitingForPO
                    && row.Status != FSAppointmentDet.status.RequestForPO
					// "Not Finished" status is a valid status of a completed appointment and should not be automatically converted to "Completed"
					|| newStatus == FSAppointmentDet.status.COMPLETED
					&& row.Status == FSAppointmentDet.status.NOT_FINISHED
                )
                {
                    return;
                }

                if (runSetValueExt == true)
                {
                    sender.SetValueExt<FSAppointmentDet.status>(row, newStatus);
                    var appointmentCache = sender.Graph.Caches[typeof(FSAppointment)];
                    if (appointmentCache != null && appointmentCache.Current != null)
                    {
                        int? oldPendingPOLineCntr = ((FSAppointment)appointmentCache.Current).PendingPOLineCntr;
                        PXUnboundFormulaAttribute.CalcAggregate<FSAppointmentDet.status>(sender, appointmentCache.Current);
                        appointmentCache.RaiseFieldUpdated<FSAppointment.pendingPOLineCntr>(appointmentCache.Current, oldPendingPOLineCntr);
                    }
                }
                else
                {
                    row.Status = newStatus;
                }
            }
        }

        public virtual bool MustHaveRequestPOStatus(FSAppointmentDet apptLine)
            => MustHaveRequestPOStatusStatic(apptLine);

        public static bool MustHaveRequestPOStatusStatic(FSAppointmentDet apptLine)
        {
            return apptLine.EnablePO == true
                    && apptLine.POSource == FSAppointmentDet.pOSource.Values.PurchaseToServiceOrder
                    && apptLine.SODetCreate == true;
        }

        public virtual List<FSAppointmentDet> GetRelatedApptLines(PXGraph graph, int? soDetID, bool excludeSpecificApptLine, int? apptDetID, bool onlyMarkForPOLines, bool sortResult)
        {
            return AppointmentEntry.GetRelatedApptLinesInt(graph, soDetID, excludeSpecificApptLine, apptDetID, onlyMarkForPOLines, sortResult);
        }
    }
}
