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
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.CN.Common.Extensions;

namespace PX.Objects.AM
{
    /// <summary>
    /// Production scheduling engine
    /// </summary>
    public class ProductionScheduleEngine : PXGraph<ProductionScheduleEngine>
    {
		public class AMProdMatlSplitPlan : AMProdMatlSplitPlan<ProductionScheduleEngine> { }
		public class AMProdItemSplitPlan : AMProdItemSplitPlan<ProductionScheduleEngine> { }

        public PXSelect<AMProdItem> ProdItems;

        public PXSelect<
            AMProdOper,
            Where<AMProdOper.orderType, Equal<Current<AMProdItem.orderType>>,
                And<AMProdOper.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>
            ProdOpers;

        public PXSelect<
            AMSchdItem,
            Where<AMSchdItem.orderType, Equal<Current<AMProdItem.orderType>>,
                And<AMSchdItem.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>
            SchdItems;

        public PXSelect<
            AMSchdOper,
            Where<AMSchdOper.orderType, Equal<Current<AMProdItem.orderType>>,
                And<AMSchdOper.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>
            SchdOpers;

        public PXSelect<
            AMProdMatl,
            Where<AMProdMatl.orderType, Equal<Current<AMProdItem.orderType>>,
                And<AMProdMatl.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>,
                And<AMProdMatl.operationID, Equal<Current<AMProdOper.operationID>>>>>>
            ProdMatls;

        public PXSelect<
            AMProdMatlSplit,
            Where<AMProdMatlSplit.orderType, Equal<Current<AMProdMatl.orderType>>,
                And<AMProdMatlSplit.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>,
                And<AMProdMatlSplit.operationID, Equal<Current<AMProdMatl.operationID>>,
                And<AMProdMatlSplit.lineID, Equal<Current<AMProdMatl.lineID>>>>>>>
            ProdMatlSplits;

        public PXSelect<
                AMOrderType,
                Where<AMOrderType.orderType, Equal<Current<AMProdItem.orderType>>>>
            OrderTypes;

        public PXSelect<AMWCSchd> WorkCenterSchdRecs;
        public PXSelect<AMWCSchdDetail> WorkCenterSchdDetailRecs;
        public PXSelect<AMSchdOperDetail> SchdOperDetails;
        public PXSelect<AMMachSchd> MachineSchdRecs;
        public PXSelect<AMMachSchdDetail> MachineSchdDetailRecs;
        public PXSelect<AMToolSchdDetail> ToolSchdDetailRecs;

        public PXSelect<AMPSetup> ProductionSetup;

        public PXSelect<
            AMProdItemSplit,
            Where<AMProdItemSplit.orderType, Equal<Current<AMProdItem.orderType>>,
                And<AMProdItemSplit.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>
            ProdItemSplits;

        #region Matl Splitting+Availability Extensions - former LSAMProdMatl lsSelectMatl view
        // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
        public class MatlLineSplittingExtension : AMProdMatlLineSplittingExtension<ProductionScheduleEngine> { }
        public MatlLineSplittingExtension MatlLineSplittingExt => FindImplementation<MatlLineSplittingExtension>();

        // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
        public class MatlItemAvailabilityExtension : AMProdMatlItemAvailabilityExtension<ProductionScheduleEngine> { }

        // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
        [PXProtectedAccess(typeof(MatlItemAvailabilityExtension))]
        public abstract class MatlItemAvailabilityAllocatedExtension : AMProdMatlItemAvailabilityAllocatedExtension<ProductionScheduleEngine, MatlItemAvailabilityExtension> { }
        #endregion

        //Required to support LSSelect
        public override string PrimaryView => nameof(ProdItems);

        protected const int TempBlockWindow = 120; //days

        /// <summary>
        /// Is schedule running as CTP schedule?
        /// </summary>
        internal bool IsCTP;

        public AMOrderType CurrentOrderType => OrderTypes.Current ?? (OrderTypes.Current = OrderTypes.Select());

        /// <summary>
        /// DateTime used for infinite scheduled entries (1/1/1900 @ 23:59:00)
        /// </summary>
        protected static DateTime InfiniteSchdDateTime => new DateTime(1900, 1, 1, 23, 59, 0);
        protected static TimeSpan InfiniteSchdTimeSpan => new TimeSpan(23, 59, 00);

        public override int Persist(Type cacheType, PXDBOperation operation)
        {
            try
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}");
#endif
                return base.Persist(cacheType, operation);
            }
            catch (Exception e)
            {
                PXTrace.WriteInformation($"Persist; cacheType = {cacheType.Name}; operation = {Enum.GetName(typeof(PXDBOperation), operation)}; Error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Scheduling block size (in minutes) for APS
        /// </summary>
        public int BlockSize
        {
            get
            {
                if (ProductionSetup.Current == null)
                {
                    ProductionSetup.Current = ProductionSetup.Select();
                }

                return ProductionSetup.Current?.SchdBlockSize ?? 0;
            }
        }

        public static ProductionScheduleEngine Construct()
        {
            return CreateInstance<ProductionScheduleEngine>();
        }

        /// <summary>
        /// Production Preferences - Include Scrap setting
        /// </summary>
        public bool IncludeScrap
        {
            get
            {
                if (ProductionSetup?.Current != null)
                {
                    return ProductionSetup.Current.InclScrap.GetValueOrDefault();
                }

                return false;
            }
        }

        /// <summary>
        /// Production Preferences - Machine Scheduling setting
        /// </summary>
        public bool MachineScheduling
        {
            get
            {
                if (ProductionSetup.Current == null)
                {
                    ProductionSetup.Current = ProductionSetup.Select();
                }

                return ProductionSetup.Current?.MachineScheduling == true && Features.AdvancedPlanningEnabled();
            }
        }

        internal bool ToolScheduling
        {
            get
            {
                if (ProductionSetup.Current == null)
                {
                    ProductionSetup.Current = ProductionSetup.Select();
                }

                return ProductionSetup.Current?.ToolScheduling == true && Features.AdvancedPlanningEnabled();
            }
        }

        /// <summary>
        /// When scheduling a production order should the process use crew size and efficiency
        /// </summary>
        internal bool UseShiftCrewSize
        {
            get
            {
                if (ProductionSetup.Current == null)
                {
                    ProductionSetup.Current = ProductionSetup.Select();
                }

                return ProductionSetup.Current?.UseShiftCrewSize == true;
            }
        }

        /// <summary>
        /// When scheduling a production order should the process use crew size and efficiency
        /// </summary>
        public bool UseShiftCrewSizeEfficiency(AMWC workCenter)
        {
            return UseShiftCrewSize && workCenter != null && workCenter.WcBasis == AMWC.BasisForCapacity.CrewSize;
        }

        public bool IsMachineScheduling(AMWC workCenter)
        {
            return MachineScheduling && workCenter?.WcBasis == AMWC.BasisForCapacity.Machines;
        }

        public static int AddDays(ReadDirection scheduleDirection)
        {
            return scheduleDirection == ReadDirection.Backward ? -1 : 1;
        }

        public static void ProcessSchedule(PXGraph graph, AMProdItem amProdItem)
        {
            var scheduleGraph = CreateInstance<ProductionScheduleEngine>();
            scheduleGraph.ProcessScheduleToGraph(graph, amProdItem);
        }

        public static void ProcessPersistSchedule(AMProdItem amProdItem)
        {
            var scheduleGraph = CreateInstance<ProductionScheduleEngine>();
            scheduleGraph.ProcessSchedule(amProdItem);
#if DEBUG
            AMDebug.TraceDirtyCaches(scheduleGraph, false, true);
#endif
            scheduleGraph.Persist();
        }

        protected virtual void ProcessScheduleToGraph(PXGraph graph, AMProdItem amProdItem)
        {
#if DEBUG
            var updatedProdItem =
#endif
            ProcessSchedule(amProdItem);
#if DEBUG
            AMDebug.TraceWriteLine("[ProcessScheduleToGraph] --------------------------------");
            AMDebug.PrintChangedValues(amProdItem, updatedProdItem);
            AMDebug.TraceWriteLine("-------------------------------------------------------");
#endif
            TransferCache(this, graph);
        }

        protected virtual void TransferCache(ProductionScheduleEngine scheduleEngine, PXGraph receivingGraph)
        {
			using (new RemoveMultiFormulaAggregrateScope<PXFormulaAttribute>(receivingGraph.Caches[typeof(AMWCSchdDetail)], new List<Type> { typeof(AMWCSchdDetail.runTime),
			typeof(AMWCSchdDetail.schdBlocks), typeof(AMWCSchdDetail.planBlocks)}))
			{
			Common.Cache.TransferCache(scheduleEngine, receivingGraph, typeof(AMProdItemSplit)); 
        }

        }

        public virtual AMProdItem ProcessSchedule(AMProdItem amProdItem, bool persistProdItem = true)
        {
            if (amProdItem == null || string.IsNullOrWhiteSpace(amProdItem.ProdOrdID))
            {
                throw new PXException(Messages.GetLocal(Messages.RecordMissing),
                        Common.Cache.GetCacheName(typeof(AMProdItem)));
            }

            var amProdItem2 = PXCache<AMProdItem>.CreateCopy(amProdItem);
            ProdItems.Current = amProdItem2;
            ProductionSetup.Current = PXSelect<AMPSetup>.Select(this);

            DeleteSchedule(PXSelect<AMSchdItem,
                    Where<AMSchdItem.orderType, Equal<Required<AMSchdItem.orderType>>,
                    And<AMSchdItem.prodOrdID, Equal<Required<AMSchdItem.prodOrdID>>>>>.Select(this, amProdItem2.OrderType, amProdItem2.ProdOrdID));

            UpdateScheduleQuantities();

            var prodDetailExists = ProdDetailExists(amProdItem2);

            AMProdItem amProdItem3 = PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>
                    >.Select(this, amProdItem2.OrderType, amProdItem2.ProdOrdID);
            ProdItems.Current = amProdItem3;

            if (amProdItem3 == null)
            {
                return amProdItem2;
            }

            if (ProductionStatus.IsOpenOrder(amProdItem3))
            {
                Common.Cache.AddCacheView<AMProdEvnt>(this);
                ProductionEventHelper.InsertInformationEvent(this, Messages.GetLocal(Messages.OrderRescheduled, ProductionOrderStatus.GetStatusDescription(amProdItem3.StatusID)), amProdItem3.ProdOrdID, amProdItem3.OrderType, false);
            }

            // Reset status back to default
            amProdItem3.ScheduleStatus = ProductionScheduleStatus.Unscheduled;

            if (!amProdItem3.SchedulingMethod.EqualsWithTrim(ScheduleMethod.UserDates))
            {
                amProdItem3.StartDate = null;
                amProdItem3.EndDate = null;
            }

            if (amProdItem3.FMLTime.GetValueOrDefault() || amProdItem3.SchedulingMethod.EqualsWithTrim(ScheduleMethod.UserDates))
            {
                CreateFixLeadTimeSchedule(ref amProdItem3);
            }
            else
            {
                foreach (AMSchdItem row in PXSelect<AMSchdItem,
                    Where<AMSchdItem.orderType, Equal<Current<AMProdItem.orderType>>,
                    And<AMSchdItem.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>.Select(this))
                {
                    var schdItem = SchdItems.Cache.LocateElse(row);
                    schdItem.ConstDate = amProdItem3.ConstDate;
                    schdItem.SchedulingMethod = amProdItem3.SchedulingMethod;

                    if (prodDetailExists)
                    {
                        CreateSchedule(ref schdItem);
                    }
                    else
                    {
                        schdItem.StartDate = schdItem.ConstDate;
                        schdItem.EndDate = schdItem.ConstDate;
                    }

                    if (Common.Dates.IsDateNull(amProdItem3.StartDate) || schdItem.StartDate < amProdItem3.StartDate)
                    {
                        amProdItem3.StartDate = schdItem.StartDate;
                    }
                    if (Common.Dates.IsDateNull(amProdItem3.EndDate) || schdItem.EndDate > amProdItem3.EndDate)
                    {
                        amProdItem3.EndDate = schdItem.EndDate;
                    }

                    amProdItem3.ScheduleStatus = ProductionScheduleStatus.MaxStatus(amProdItem3.ScheduleStatus, schdItem.ScheduleStatus);
                }
            }

			if (amProdItem3.StartDate == null && amProdItem3.EndDate == null)
			{
				amProdItem3.StartDate = amProdItem.StartDate;
				amProdItem3.EndDate = amProdItem.EndDate;
			}

			return PXCache<AMProdItem>.CreateCopy(ProdItems.Update(amProdItem3));
        }

        protected virtual int? GetFixLeadTimeHours(AMProdItem amProdItem)
        {
            if (amProdItem == null)
            {
                return null;
            }

            return InventoryHelper.GetFixMfgLeadTime(this, amProdItem.InventoryID, amProdItem.SiteID);
        }

        protected virtual bool ProdDetailExists(AMProdItem amProdItem)
        {
            AMProdOper row =
                PXSelectReadonly<AMProdOper,
                    Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>>
                    >.SelectWindowed(this, 0, 1, amProdItem.OrderType, amProdItem.ProdOrdID);

            return row != null;
        }

        protected virtual void CreateFixLeadTimeSchedule(ref AMProdItem amProdItem)
        {
            if (amProdItem == null)
            {
                return;
            }

            var fixLeadTime = GetFixLeadTimeHours(amProdItem);
            if (fixLeadTime < 0)
            {
                fixLeadTime = 0;
            }

            if (ProductionSetup.Current == null)
            {
                ProductionSetup.Current = PXSelect<AMPSetup>.Select(this);
            }

            if (ProductionSetup.Current.FixMfgCalendarID == null)
            {
                throw new PXException(Messages.MissingFixMfgCalendar);
            }


            DateTime planDate = amProdItem.ConstDate ?? Common.Current.BusinessDate(this);

            if (!amProdItem.SchedulingMethod.EqualsWithTrim(ScheduleMethod.UserDates))
            {
                bool scheduleBack = amProdItem.SchedulingMethod == ScheduleMethod.FinishOn;

				var calendarHelper = this.GetService<ICalendarHelper>();
				calendarHelper.Initialize(ProductionSetup.Current.FixMfgCalendarID, scheduleBack
                        ? ReadDirection.Backward
						: ReadDirection.Forward);

                DateTime? firstDate = calendarHelper.GetNextWorkDay(planDate, true);
                DateTime lastDate = firstDate ?? planDate;
                int leadTimeUnits = ProductionSetup.Current.FMLTimeUnits ?? TimeUnits.Days;
                decimal usedUnits = 0;

                if (firstDate != null && usedUnits != fixLeadTime)
                {
                    while (1 < 2)
                    {
                        DateTime? nextDateTime = calendarHelper.GetNextWorkDay(lastDate, true);

                        if (nextDateTime == null)
                        {
                            break;
                        }

                        lastDate = nextDateTime ?? lastDate;

                        AMDateInfo dateInfo = calendarHelper.GetDateInfo(lastDate);

                        usedUnits += leadTimeUnits == TimeUnits.Hours ? dateInfo.WorkingHoursDecimal : 1;

                        if (usedUnits >= fixLeadTime.GetValueOrDefault())
                        {
                            break;
                        }

                        lastDate = calendarHelper.NextDateTime(lastDate);
                    }
                }

                if (firstDate == null)
                {
                    firstDate = lastDate;
                }

                amProdItem.StartDate = scheduleBack ? lastDate : firstDate;
                amProdItem.EndDate = scheduleBack ? firstDate : lastDate;
            }

            if (amProdItem.StartDate == null || amProdItem.EndDate == null)
            {
                return;
            }

            amProdItem.ScheduleStatus = ProductionScheduleStatus.Unscheduled;

            foreach (AMProdItemSplit itemSplit in PXSelect<AMProdItemSplit,
                Where<AMProdItemSplit.orderType, Equal<Required<AMProdItemSplit.orderType>>,
                    And<AMProdItemSplit.prodOrdID, Equal<Required<AMProdItemSplit.prodOrdID>>>>
            >.Select(this, amProdItem.OrderType, amProdItem.ProdOrdID))
            {
                itemSplit.TranDate = amProdItem.EndDate;
                ProdItemSplits.Update(itemSplit);
            }

            //Set all of the operations to match the item start/stop
            foreach (AMProdOper prodOper in PXSelect<AMProdOper,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>>
                    >.Select(this, amProdItem.OrderType, amProdItem.ProdOrdID))
            {
                prodOper.StartDate = amProdItem.StartDate;
                prodOper.EndDate = amProdItem.EndDate;
                ProdOpers.Update(prodOper);
            }

            // Material needs updated also to reflect correct dates in Allocation Details and MRP
            foreach (AMProdMatl prodMatl in PXSelect<AMProdMatl,
                Where<AMProdMatl.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>>>
            >.Select(this, amProdItem.OrderType, amProdItem.ProdOrdID))
            {
                prodMatl.TranDate = amProdItem.StartDate;
                ProdMatls.Update(prodMatl);
            }

            foreach (AMProdMatlSplit prodMatlSplit in PXSelect<AMProdMatlSplit,
                Where<AMProdMatlSplit.orderType, Equal<Required<AMProdMatlSplit.orderType>>,
                    And<AMProdMatlSplit.prodOrdID, Equal<Required<AMProdMatlSplit.prodOrdID>>>>
            >.Select(this, amProdItem.OrderType, amProdItem.ProdOrdID))
            {
                prodMatlSplit.TranDate = amProdItem.StartDate;
                ProdMatlSplits.Update(prodMatlSplit);
            }
        }

        protected virtual List<AMSchdOper> GetSchdOperations(AMSchdItem schdItem)
        {
            return PXSelect<
                    AMSchdOper,
                Where<AMSchdOper.orderType, Equal<Required<AMSchdOper.orderType>>,
                    And<AMSchdOper.prodOrdID, Equal<Required<AMSchdOper.prodOrdID>>,
                    And<AMSchdOper.schdID, Equal<Required<AMSchdOper.schdID>>>>>>
                .Select(this, schdItem.OrderType, schdItem.ProdOrdID, schdItem.SchdID).ToFirstTableList();
        }

        protected virtual List<AMSchdOper> GetSchdOperationsOrderByDirection(AMSchdItem schdItem, bool scheduleBackwards)
        {
            return SchdOperationsOrderByDirection(GetSchdOperations(schdItem), scheduleBackwards);
        }

        protected virtual List<AMSchdOper> SchdOperationsOrderByDirection(List<AMSchdOper> schdOperations, bool scheduleBackwards)
        {
            if (schdOperations == null || schdOperations.Count == 0)
            {
                return null;
            }

            return scheduleBackwards
                ? schdOperations.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.LineNbr).ToList()
                : schdOperations.OrderBy(x => x.SortOrder).ThenBy(x => x.LineNbr).ToList();
        }

        /// <summary>c
        /// Create the schedule for the current production order
        /// </summary>
        protected virtual void CreateSchedule(ref AMSchdItem schdItem)
        {
            if (schdItem == null)
            {
                return;
            }

            var planDate = schdItem.ConstDate ?? Common.Current.BusinessDate(this);
            var usedTime = 0;
            var currentDate = planDate;
            var schdConst = schdItem.SchedulingMethod;

            if (schdConst.EqualsWithTrim(ScheduleMethod.UserDates))
            {
                schdConst = ScheduleMethod.FinishOn;
            }

            var scheduleBack = schdConst.EqualsWithTrim(ScheduleMethod.FinishOn);

            var orderedSchdOpers = GetSchdOperationsOrderByDirection(schdItem, scheduleBack);

            schdItem.StartDate = null;
            schdItem.EndDate = null;
            schdItem.ScheduleStatus = ProductionScheduleStatus.Unscheduled;

            DateTime? lastOperDateTime = null;
            foreach (var row in orderedSchdOpers)
            {
                if (row?.OperationID == null)
                {
                    continue;
                }

                var schdOperation = SchdOpers.Cache.LocateElseCopy(row);
                if (schdOperation == null)
                {
                    continue;
                }

                schdOperation.SchedulingMethod = schdConst;

                AMProdOper amProdOper = PXSelect<AMProdOper,
                    Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                        And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>
                    >.Select(this, schdOperation.OrderType, schdOperation.ProdOrdID, schdOperation.OperationID);

                if (string.IsNullOrWhiteSpace(amProdOper?.ProdOrdID))
                {
                    throw new PXException(Messages.RecordMissing, Common.Cache.GetCacheName(typeof(AMProdOper)));
                }

                schdOperation.WcID = amProdOper.WcID;

                //First shift and work center records paired
                var wcResult =
                    (PXResult<AMShift, AMWC>)PXSelectJoin<AMShift,
                        InnerJoin<AMWC, On<AMShift.wcID, Equal<AMWC.wcID>>>,
                        Where<AMShift.wcID, Equal<Required<AMShift.wcID>>>>.SelectWindowed(this, 0, 1, schdOperation.WcID);

                var amWorkCenter = (AMWC)wcResult;

                if (string.IsNullOrWhiteSpace(amWorkCenter?.WcID))
                {
                    throw new PXException(Messages.RecordMissing, Common.Cache.GetCacheName(typeof(AMWC)));
                }

                var amShift = (AMShift)wcResult;
                if (string.IsNullOrWhiteSpace(amShift?.ShiftCD))
                {
                    throw new PXException(Messages.RecordMissing, Common.Cache.GetCacheName(typeof(AMShift)));
                }

                var phtmpriorlevelqty = amProdOper.PhtmPriorLevelQty.GetValueOrDefault() == 0
                    ? 1
                    : amProdOper.PhtmPriorLevelQty.GetValueOrDefault();

                var fgQty = amProdOper.BaseQtyRemaining.GetValueOrDefault() * phtmpriorlevelqty;

                var totalTime = GetTotalOperationTime(amWorkCenter, amShift, amProdOper, fgQty);
                schdOperation.RunTime = totalTime;
                schdOperation.RunTimeBase = totalTime;
				schdOperation.FinishTime = amProdOper.FinishTime;
				schdOperation.MoveTime = amProdOper.MoveTime;
				schdOperation.QueueTime = amProdOper.QueueTime;
                schdOperation.QtytoProd = fgQty;

                var scheduleDirection = schdOperation.SchedulingMethod == ScheduleMethod.FinishOn
                    ? ReadDirection.Backward
                    : ReadDirection.Forward;

                currentDate = schdItem.ConstDate ?? planDate;

                if (scheduleDirection == ReadDirection.Backward && currentDate > (lastOperDateTime ?? currentDate))
                {
                    currentDate = lastOperDateTime ?? currentDate;
                }
                if (scheduleDirection == ReadDirection.Forward && currentDate < (lastOperDateTime ?? currentDate))
                {
                    currentDate = lastOperDateTime ?? currentDate;
                }

				ScheduleTime(ref schdOperation, ref amProdOper, scheduleDirection, totalTime, ref currentDate, ref usedTime, amWorkCenter, amShift);

				lastOperDateTime = currentDate;

				schdOperation = SchdOpers.Update(schdOperation);
				amProdOper.StartDate = schdOperation.StartDate;
				amProdOper.EndDate = schdOperation.EndDate;
				ProdOpers.Update(amProdOper);

				schdItem.ScheduleStatus = ProductionScheduleStatus.Unscheduled;
				if (schdItem.StartDate == null || schdOperation.QueueStartDate < schdItem.StartDate)
				{
					schdItem.StartDate = schdOperation.QueueStartDate;
				}
				if (schdItem.EndDate == null || schdOperation.MoveEndDate > schdItem.EndDate)
				{
					schdItem.EndDate = schdOperation.MoveEndDate;
				}
			}

			if (schdItem.StartDate == null && schdItem.EndDate == null)
			{
				schdItem.StartDate = Common.Current.BusinessDate(this);
				schdItem.EndDate = Common.Current.BusinessDate(this);
			}

			UpdateMaterialSplitDate(PXSelectJoin<
                AMProdMatlSplit,
                InnerJoin<AMProdMatl,
                    On<AMProdMatlSplit.orderType, Equal<AMProdMatl.orderType>,
                    And<AMProdMatlSplit.prodOrdID, Equal<AMProdMatl.prodOrdID>,
                    And<AMProdMatlSplit.operationID, Equal<AMProdMatl.operationID>,
                    And<AMProdMatlSplit.lineID, Equal<AMProdMatl.lineID>>>>>>,
                Where<AMProdMatlSplit.orderType, Equal<Required<AMProdMatl.orderType>>,
                    And<AMProdMatlSplit.prodOrdID, Equal<Required<AMProdMatl.prodOrdID>>>>>
                .Select(this, schdItem.OrderType, schdItem.ProdOrdID)
                .ToList<AMProdMatlSplit, AMProdMatl>());

            SchdItems.Update(schdItem);
        }

        protected virtual void SetSchdOperMachID(AMSchdOper schdOperation, string workCenterID)
        {
            if (schdOperation == null || !string.IsNullOrWhiteSpace(schdOperation.MachID) || string.IsNullOrWhiteSpace(workCenterID))
            {
                return;
            }

            AMWCMach amwcMach = PXSelectJoin<AMWCMach
                , InnerJoin<AMMach, On<AMWCMach.machID, Equal<AMMach.machID>>>
                , Where<AMWCMach.wcID, Equal<Required<AMWCMach.wcID>>
                , And<AMMach.activeFlg, Equal<True>
                , And<AMMach.downFlg, Equal<False>>>>
                    >.SelectWindowed(this, 0, 1, workCenterID);

            if (string.IsNullOrWhiteSpace(amwcMach?.MachID))
            {
                return;
            }

            schdOperation.MachID = amwcMach.MachID;
            SchdOpers.Update(schdOperation);
        }

        protected virtual IEnumerable<MachineCalendarHelper> GetActiveMachineCalendars(AMWC workCenter, AMShift shift, ReadDirection scheduleDirection)
        {
            if (string.IsNullOrWhiteSpace(workCenter?.WcID))
            {
                yield break;
            }

            foreach (var machine in GetActiveWorkCenterMachines(workCenter.WcID))
            {
                if (machine?.MachID == null)
                {
                    continue;
                }

                yield return new MachineCalendarHelper(this, machine, workCenter)
                {
                    CalendarReadDirection = scheduleDirection,
                    Shift = shift
                };
            }
        }

        protected virtual IEnumerable<AMMach> GetActiveWorkCenterMachines(string workCenterID)
        {
            if (string.IsNullOrWhiteSpace(workCenterID))
            {
                return null;
            }

            return PXSelectJoin<
                AMMach,
                InnerJoin<AMWCMach,
                    On<AMMach.machID, Equal<AMWCMach.machID>>>,
                Where<AMWCMach.wcID, Equal<Required<AMWCMach.wcID>>,
                    And<AMMach.activeFlg, Equal<True>>>>
                .Select(this, workCenterID)
                .ToFirstTable();
        }

		[Obsolete]
        protected virtual DateTime? GetNextDateByTime(DateTime? currentDateTime, int minutes, CalendarHelper calendarHelper, bool nextDayWhenZero, out int timeConsumedForDate)
        {
			throw new NotImplementedException();
        }

		/// <summary>
		/// For Queue and Move time we need
		/// </summary>
        protected virtual DateTime? GetNextDateByTime(DateTime? currentDateTime, int minutes, ICalendarHelper calendarHelper, bool nextDayWhenZero, out int timeConsumedForDate)
		{
			timeConsumedForDate = 0;
			var remainingTime = minutes;
			if(currentDateTime == null)
			{
				return currentDateTime;
			}

			var nextDate = currentDateTime.GetValueOrDefault();
			var tries = 0;
			var maxTries = 2000; // bad calendar setup could result getting stuck
			while(remainingTime > 0 && tries < maxTries)
			{
				tries++;

				var dateInfo = calendarHelper.GetDateInfo(nextDate);
				timeConsumedForDate = remainingTime;
				remainingTime -= dateInfo.WorkingMinutes;
				if(remainingTime >= 0)
				{
					if(!nextDayWhenZero && remainingTime == 0)
					{
						continue;
					}

					var newDate = calendarHelper.GetNextWorkDay(nextDate);
					if (newDate == null)
					{
						// bad calendar setup
						timeConsumedForDate = 0;
						return null;
					}
					nextDate = newDate.GetValueOrDefault();
					if(remainingTime == 0)
					{
						timeConsumedForDate = 0;
					}
				}
			}

			return nextDate;
		}

        protected virtual void ScheduleTime(ref AMSchdOper schdOperation, ref AMProdOper amProdOper, ReadDirection scheduleDirection,
            int totalSchdTime, ref DateTime currentDateTime, ref int usedTime, AMWC workCenter, AMShift amShift)
        {
			if(schdOperation == null)
			{
				return;
			}

			schdOperation.QueueStartDate = null;
            schdOperation.StartDate = currentDateTime;
            schdOperation.EndDate = currentDateTime;
			schdOperation.MoveEndDate = null;

			// Note that totalSchedTime does not include FinishTime.
			// FinishTime is not effected by efficiency and does not impact machine scheduling.
			// Queue and Move time do not generate schddetail records however do move the date based on consumed time
            if (totalSchdTime <= 0 &&
				schdOperation.FinishTime.GetValueOrDefault() <= 0 &&
				schdOperation.QueueTime.GetValueOrDefault() <= 0 &&
				schdOperation.MoveTime.GetValueOrDefault() <= 0)
            {
				schdOperation.QueueStartDate = currentDateTime;
				schdOperation.MoveEndDate = currentDateTime;
                return;
            }

            if (workCenter?.WcID == null)
            {
                throw new ArgumentNullException(nameof(workCenter));
            }

            if (Common.Dates.IsDefaultDate(schdOperation.StartDate))
            {
                schdOperation.StartDate = currentDateTime;
            }

            if (Common.Dates.IsDefaultDate(schdOperation.EndDate))
            {
                schdOperation.EndDate = currentDateTime;
            }
			var calendarHelper = this.GetService<ICalendarHelper>();
			calendarHelper.Initialize(amShift.CalendarID, scheduleDirection);

            List<MachineCalendarHelper> machineCalendars = null;
            var isMachineScheduling = IsMachineScheduling(workCenter);
            if (isMachineScheduling)
            {
                machineCalendars = GetActiveMachineCalendars(workCenter, amShift, scheduleDirection).ToList();
                isMachineScheduling = machineCalendars.Count > 0;
            }
			var floatTimeBefore = scheduleDirection == ReadDirection.Forward ? schdOperation.QueueTime.GetValueOrDefault() : schdOperation.MoveTime.GetValueOrDefault();
			var floatTimeAfter = scheduleDirection == ReadDirection.Forward ? schdOperation.MoveTime.GetValueOrDefault() : schdOperation.QueueTime.GetValueOrDefault();
			var origDate = currentDateTime;
            var totalTime = totalSchdTime;
            var maxLoop = Math.Max(30, totalTime / 60 / 8 * 2);
            var cntr = 0; //It is possible with Machine schd setup to not have available time and this would just keep running for next date
			var firstFloatTimeBefore = floatTimeBefore > 0;
			var firstFloatTimeAfter = floatTimeAfter > 0;
			var finishTimeRemaining = schdOperation.FinishTime.GetValueOrDefault();
			var hasSchdDetailTime = totalTime > 0 || finishTimeRemaining > 0;
            while (totalTime > 0 || firstFloatTimeBefore || firstFloatTimeAfter || finishTimeRemaining > 0)
            {
                cntr++;
                var nextWorkDate = isMachineScheduling
                    ? MachineCalendarHelper.GetNextWorkDay(machineCalendars, currentDateTime, true)
                    : calendarHelper.GetNextWorkDay(currentDateTime, true);

				if(firstFloatTimeBefore)
				{
					if (scheduleDirection == ReadDirection.Forward)
					{
						schdOperation.QueueStartDate = nextWorkDate.GetValueOrDefault();
					}
					else
					{
						schdOperation.MoveEndDate = nextWorkDate.GetValueOrDefault();
					}

					nextWorkDate = GetNextDateByTime(nextWorkDate, floatTimeBefore + usedTime, calendarHelper, true, out var floatTimeConsumed);
					usedTime = floatTimeConsumed;
					firstFloatTimeBefore = false;
				}
				else
				{
					floatTimeBefore = 0;
				}

                if (nextWorkDate == null)
                {
                    //  Unable to find a valid next date. 
                    //  Could be bad setup of work calendar with working days but no hours.
                    return;
                }

                if (!Common.Dates.DatesEqual(currentDateTime, nextWorkDate))
                {
                    //if the set start and end dates equal a date that was determined as a non work day we need to update those values to the work date value
                    if (Common.Dates.DatesEqual(currentDateTime, schdOperation.StartDate.GetValueOrDefault(currentDateTime)))
                    {
                        schdOperation.StartDate = nextWorkDate;
                    }
                    if (Common.Dates.DatesEqual(currentDateTime, schdOperation.EndDate.GetValueOrDefault(currentDateTime)))
                    {
                        schdOperation.EndDate = nextWorkDate;
                    }
                }

                currentDateTime = nextWorkDate.GetValueOrDefault();

                var dateInfo = isMachineScheduling
                    ? MachineCalendarHelper.AddWorkingDateInfo(machineCalendars, currentDateTime, out var notUsed)
                    : calendarHelper.GetDateInfo(currentDateTime);

                if (dateInfo.WorkingMinutes <= 0)
                {
                    return;
                }

                GetWorkCenterSchd(schdOperation, amShift.ShiftCD, dateInfo);
                var wcSchdDetail = CreateWCSchdDetail(schdOperation, currentDateTime, amShift.ShiftCD);

                var availTime = dateInfo.WorkingMinutes - usedTime;
				var schdFinishTime = 0;
				var	schdTime = Math.Min(availTime.NotLessZero(), totalTime);
				
				if(scheduleDirection == ReadDirection.Forward && finishTimeRemaining > 0 && availTime > totalTime)
				{
					schdFinishTime = Math.Min(availTime - totalTime, finishTimeRemaining);
					finishTimeRemaining -= schdFinishTime;
				}

				if(scheduleDirection == ReadDirection.Backward && finishTimeRemaining > 0)
				{
					schdTime = 0;
					schdFinishTime = Math.Min(availTime.NotLessZero(), finishTimeRemaining);
					if(finishTimeRemaining < availTime.NotLessZero())
					{
						schdFinishTime = finishTimeRemaining;
						schdTime = totalTime > 0 ? availTime.NotLessZero() - schdFinishTime : 0;
					}
					finishTimeRemaining -= schdFinishTime;
				}
				var schdEfficiencyTime = schdTime;

                var machineSchdDetail = new List<AMMachSchdDetail>();
                if (isMachineScheduling && schdTime > 0m)
                {
                    wcSchdDetail.RunTimeBase = schdTime;
                    wcSchdDetail.RunTime = schdTime;
                    wcSchdDetail.SchdTime = schdTime;
                    machineSchdDetail = GetIdealMachineSchdDetail(machineCalendars, wcSchdDetail, scheduleDirection, currentDateTime, schdTime, 0);
                    if (machineSchdDetail != null)
                    {
                        var machTotal = 0;
                        var machEfficiencyTotal = 0;
                        foreach (var machSchdDetail in machineSchdDetail)
                        {
                            machTotal += machSchdDetail.RunTimeBase.GetValueOrDefault();
                            machEfficiencyTotal += machSchdDetail.RunTime.GetValueOrDefault();
                        }

                        schdTime = machTotal;
                        schdEfficiencyTime = machEfficiencyTotal;
                    }
                }

				if ((schdTime > 0m || schdFinishTime > 0) && wcSchdDetail != null)
				{
					if (isMachineScheduling && schdTime > 0m)
					{
						wcSchdDetail = InsertMachineSchdDetail(machineSchdDetail, wcSchdDetail, schdOperation, true, out schdTime);
						if(wcSchdDetail != null && floatTimeBefore > 0)
						{
							if (scheduleDirection == ReadDirection.Forward)
							{
								wcSchdDetail.QueueTime = floatTimeBefore;
							}
							else
							{
								wcSchdDetail.MoveTime = floatTimeBefore;
							}
							floatTimeBefore = 0;
							WorkCenterSchdDetailRecs.Update(wcSchdDetail);
						}
						hasSchdDetailTime = wcSchdDetail != null;
					}
					else
					{
						wcSchdDetail.SchdKey = Guid.NewGuid();
						wcSchdDetail.RunTimeBase = schdTime + schdFinishTime;
						wcSchdDetail.RunTime = schdEfficiencyTime + schdFinishTime;
						wcSchdDetail.SchdTime = wcSchdDetail.RunTime;
						wcSchdDetail.FinishTime = schdFinishTime;

						if (schdTime > 0 && UseShiftCrewSizeEfficiency(workCenter) && Features.AdvancedPlanningEnabled())
						{
							wcSchdDetail.RunTimeBase = CalcTotalEfficiencyTimeReverse(schdTime, amShift?.CrewSize, amShift?.ShftEff) + schdFinishTime;
						}

						if(floatTimeBefore > 0)
						{
							if (scheduleDirection == ReadDirection.Forward)
							{
								wcSchdDetail.QueueTime = floatTimeBefore;
							}
							else
							{
								wcSchdDetail.MoveTime = floatTimeBefore;
							}
							floatTimeBefore = 0;
						}

						wcSchdDetail.PlanBlocks = MinutesToBlocks(wcSchdDetail.RunTime.GetValueOrDefault(), BlockSize, true);
						wcSchdDetail.SchdBlocks = 0; // only finite sets schd blocks
						wcSchdDetail = InsertAMWCSchdDetail(wcSchdDetail, schdOperation);
						hasSchdDetailTime = wcSchdDetail != null;
					}
				}

				if (schdTime > 0 || schdFinishTime > 0)
				{
					if (scheduleDirection == ReadDirection.Backward && currentDateTime < (schdOperation.StartDate ?? currentDateTime))
					{
						schdOperation.StartDate = currentDateTime;
					}
					if (scheduleDirection == ReadDirection.Forward && currentDateTime > (schdOperation.EndDate ?? currentDateTime))
					{
						schdOperation.EndDate = currentDateTime;
					}
				}

                usedTime += schdTime + schdFinishTime;

                if (cntr >= maxLoop)
                {
                    //Unable to use up time 
                    if (totalTime == totalSchdTime)
                    {
                        PXTrace.WriteWarning($"Maxed out date/time lookup between {origDate.ToShortDateString()} and {currentDateTime.ToShortDateString()} for {schdOperation.OrderType} {schdOperation.ProdOrdID} {amProdOper.OperationCD} with work center {schdOperation.WcID}");
                        currentDateTime = origDate;
                        usedTime = 0;
                    }
                    break;
                }

				totalTime = (totalTime - schdTime).NotLessZero();
				if(totalTime > 0 || finishTimeRemaining > 0)
				{
					currentDateTime = calendarHelper.NextDateTime(currentDateTime);
					usedTime = 0;
				}

				if(totalTime <= 0 && finishTimeRemaining <= 0 && firstFloatTimeAfter)
				{
					// TODO: need to see if we can set to same day when "end"
					var floatDateAfter = GetNextDateByTime(currentDateTime, floatTimeAfter + usedTime, calendarHelper, false, out var floatTimeConsumed);
					if(floatDateAfter != null)
					{
						usedTime = floatTimeConsumed;
						currentDateTime = floatDateAfter.GetValueOrDefault();
					}

					if (scheduleDirection == ReadDirection.Forward)
					{
						if(hasSchdDetailTime)
						{
							wcSchdDetail.MoveTime = floatTimeAfter;
						}
						schdOperation.MoveEndDate = currentDateTime;
					}
					else
					{
						if (hasSchdDetailTime)
						{
							wcSchdDetail.QueueTime = floatTimeAfter; 
						}
						schdOperation.QueueStartDate = currentDateTime;
					}
					if (hasSchdDetailTime)
					{
						WorkCenterSchdDetailRecs.Update(wcSchdDetail); 
					}

					if(usedTime > 0)
					{
						var dateInfoAfter = calendarHelper.GetDateInfo(currentDateTime);
						if(dateInfoAfter.WorkingMinutes == usedTime)
						{
							// Possible when we need the correct date for move or queue datetime but all time used up for the day so push forward for next schedule call
							currentDateTime = calendarHelper.GetNextWorkDay(currentDateTime) ?? currentDateTime;
							usedTime = 0;
						}
					}

					firstFloatTimeAfter = false;
				}
            }

			if(schdOperation.QueueStartDate == null)
			{
				schdOperation.QueueStartDate = schdOperation.StartDate;
			}
			if(schdOperation.MoveEndDate == null)
			{
				schdOperation.MoveEndDate = schdOperation.EndDate;
			}
        }

        public static int HoursToMinutes(decimal hours, bool roundUp)
        {
            return HoursToMinutes(Convert.ToDouble(hours), roundUp);
        }

        public static int HoursToMinutes(double hours, bool roundUp)
        {
            return Convert.ToInt32(RoundUp(hours * 60, roundUp));
        }

        /// <summary>
        /// Calculate the total scheduling blocks
        /// </summary>
        /// <param name="hours">Working/scheduling hours</param>
        /// <param name="blockMinutes">Block definition/setup related in minutes</param>
        /// <param name="roundUp">Indicates if the block size is to be rounded up (true) or down (false)</param>
        /// <returns>Number of blocks</returns>
        public static int HoursToBlocks(double hours, int blockMinutes, bool roundUp)
        {
            if (hours <= 0)
            {
                return 0;
            }

            return MinutesToBlocks(HoursToMinutes(hours, roundUp), blockMinutes, roundUp);
        }

        protected static double RoundUp(double value, bool roundUp)
        {
            return roundUp ? Math.Ceiling(value) : Math.Floor(value);
        }

        public DateTime AddBlocks(DateTime date, int blocks)
        {
            return date.AddMinutes(blocks * BlockSize);
        }

        public static int DateTimesToBlocks(DateTime? startDateTime, DateTime? endDateTime, int blockMinutes, bool roundUp)
        {
            return (startDateTime == null || endDateTime == null) ?
                0 :
                DateTimesToBlocks(startDateTime.GetValueOrDefault(), endDateTime.GetValueOrDefault(), blockMinutes, roundUp);
        }

        public static int DateTimesToBlocks(DateTime startDateTime, DateTime endDateTime, int blockMinutes, bool roundUp)
        {
            var totalHours = (endDateTime - startDateTime).TotalHours;
            if (totalHours < -24)
            {
                // Mismatch start and end dates passed
                return 0;
            }

            var end = totalHours < 0 ? endDateTime.AddDays(1) : endDateTime;
            return MinutesToBlocks((int)Math.Ceiling((end - startDateTime).TotalMinutes), blockMinutes, roundUp);
        }

        protected virtual int MinutesToBlocks(int minutes, bool roundUp)
        {
            return MinutesToBlocks(minutes, BlockSize, roundUp);
        }

        /// <summary>
        /// Calculate the total scheduling blocks
        /// </summary>
        /// <param name="minutes">Working/scheduling hours</param>
        /// <param name="blockMinutes">Block definition/setup related in minutes</param>
        /// <param name="roundUp">Indicates if the block size is to be rounded up (true) or down (false)</param>
        /// <returns>Number of blocks</returns>
        public static int MinutesToBlocks(int minutes, int blockMinutes, bool roundUp)
        {
            //work time as an int is stored in "minutes" as an int value.
            if (blockMinutes <= 0)
            {
                throw new PXArgumentException(nameof(blockMinutes));
            }

            if (minutes <= 0)
            {
                return 0;
            }

            var blocksAsDouble = minutes / (double)blockMinutes;
            var blocks = 0;

            try
            {
                blocks = Convert.ToInt32(RoundUp(blocksAsDouble, roundUp));
            }
            catch (Exception e)
            {
                PXTrace.WriteWarning(e);
            }

            if (blocks < 0)
            {
                blocks = 0;
            }

            return blocks;
        }

		protected virtual int BlockRoundMinutes(int minutes, bool roundUp)
		{
			return BlockRoundMinutes(minutes, BlockSize, roundUp);
		}

		public static int BlockRoundMinutes(int minutes, int blockMinutes, bool roundUp)
		{
			return MinutesToBlocks(minutes, blockMinutes, roundUp) * blockMinutes;
		}

        internal AMWCSchdDetail InsertMachineSchdDetail(List<AMMachSchdDetail> machSchdDetails, AMWCSchdDetail wcSchdDetail, AMSchdOper schdOper, bool isPlanSchd, out int totalSchdTime)
        {
            totalSchdTime = 0;
            if (machSchdDetails.Count == 0)
            {
                //Unable to schedule the next work day (gap in schedule) so skip to next available date (which should be set in currentSchedDateTime)
                return null;
            }

            wcSchdDetail.RunTimeBase = 0;
            wcSchdDetail.RunTime = 0;
            wcSchdDetail.SchdTime = 0;
            wcSchdDetail.StartTime = null;
            wcSchdDetail.EndTime = null;

            var machineList = new HashSet<string>();
            foreach (var msd in machSchdDetails)
            {
                var machSchdDetail = PXCache<AMMachSchdDetail>.CreateCopy(msd);
                wcSchdDetail.RunTimeBase += machSchdDetail.RunTimeBase.GetValueOrDefault();
                wcSchdDetail.RunTime += machSchdDetail.RunTime.GetValueOrDefault();
                totalSchdTime += machSchdDetail.RunTimeBase.GetValueOrDefault();

                if (isPlanSchd)
                {
                    wcSchdDetail.StartTime = InfiniteSchdDateTime;
                    wcSchdDetail.EndTime = InfiniteSchdDateTime;
                    machSchdDetail.StartTime = InfiniteSchdDateTime;
                    machSchdDetail.EndTime = InfiniteSchdDateTime;
                }
                else
                {
                    wcSchdDetail.StartTime = wcSchdDetail.StartTime == null
                        ? machSchdDetail.StartTime
                        : machSchdDetail.StartTime.LesserDateTime(wcSchdDetail.StartTime);

                    wcSchdDetail.EndTime = wcSchdDetail.EndTime == null
                        ? machSchdDetail.EndTime
                        : machSchdDetail.EndTime.GreaterDateTime(wcSchdDetail.EndTime);
                }

                machineList.Add(machSchdDetail.MachID);
                if (MachineSchdDetailRecs.Insert(machSchdDetail) == null)
                {
                    PXTrace.WriteWarning($"Unable to insert machine schedule detail for {machSchdDetail.DebuggerDisplay}");
                }
            }

            var blocks = MinutesToBlocks(wcSchdDetail.RunTimeBase.GetValueOrDefault(), true);
            wcSchdDetail.SchdBlocks = isPlanSchd ? 0 : blocks;
            wcSchdDetail.PlanBlocks = blocks;

            //Limit Description to 256 characters
            wcSchdDetail.Description = string.Join(",", machineList);
            if (wcSchdDetail.Description.Length > 256)
            {
                wcSchdDetail.Description = wcSchdDetail.Description.Substring(0, 256);
            }

            wcSchdDetail.SchdTime = wcSchdDetail.RunTime;
            return InsertAMWCSchdDetail(wcSchdDetail, schdOper);
        }

        protected AMWCSchdDetail InsertAMWCSchdDetail(AMWCSchdDetail schdDetail, AMSchdOper schdOper)
        {
            return InsertAMWCSchdDetail(schdDetail, schdOper, null, null);
        }

        protected AMWCSchdDetail InsertAMWCSchdDetail(AMWCSchdDetail schdDetail, AMSchdOper schdOper, AMWC workCenter, AMShift shift)
        {
            if (schdDetail != null)
            {
                if (schdDetail.SchdKey == null)
                {
                    schdDetail.SchdKey = Guid.NewGuid();
                }

                var resourceSize = 0m;
                if(Features.AdvancedPlanningEnabled() && workCenter != null)
                {
                    resourceSize = UseShiftCrewSizeEfficiency(workCenter) 
                        ? (shift.CrewSize ?? 1m) 
                        : 1m;
                }
                schdDetail.ResourceSize = resourceSize;
            }

            return SchdOperDetails.Insert(MakeSchdOperDetail(schdDetail, schdOper)) == null
                ? null
                : WorkCenterSchdDetailRecs.Insert(schdDetail);
        }

        protected AMSchdOperDetail MakeSchdOperDetail(AMWCSchdDetail schdDetail, AMSchdOper schdOper)
        {
            if (schdDetail == null || schdOper == null)
            {
                return null;
            }

            var schdOperDetail = CreateSchdOperDetail(schdOper, schdDetail.SchdDate.GetValueOrDefault());
            schdOperDetail.SchdDate = schdDetail.SchdDate;
            schdOperDetail.RunTimeBase = schdDetail.RunTimeBase;
            schdOperDetail.RunTime = schdDetail.RunTime;
            schdOperDetail.SchdKey = schdDetail.SchdKey;

            return schdOperDetail;
        }

#if DEBUG
        // Depending on Machine schedule or efficiency we might be able to distribute more time to another machine on a retry. This method will do that for us and give us the best collection of machine schedule records for schdtime
#endif
        internal List<AMMachSchdDetail> GetIdealMachineSchdDetail(List<MachineCalendarHelper> machineCalendars, AMWCSchdDetail wcSchdDetail, ReadDirection scheduleDirection, DateTime currentSchedDateTime, int schdTime, int level)
        {
            var firstPass = new List<AMMachSchdDetail>();

            if (wcSchdDetail == null)
            {
                throw new ArgumentNullException(nameof(wcSchdDetail));
            }

            if ((machineCalendars?.Count ?? 0) == 0)
            {
                return firstPass;
            }

            var machineSchdTime = schdTime / machineCalendars.Count;

            foreach (var machineCalendar in machineCalendars)
            {
                firstPass.Add(CreateMachineSchdDetail(wcSchdDetail, machineCalendar, scheduleDirection, currentSchedDateTime, machineSchdTime));
            }

            // If all the times are the same, there is no need to adjust the time
            if (firstPass.HasSameSchdTime() || level >= 10)
            {
                return firstPass;
            }

            //Else here lets retry some...
            var unusedTime = 0;
            var returnList = new List<AMMachSchdDetail>();
            var retryList = new List<AMMachSchdDetail>();
            var retryMachineCalList = new List<MachineCalendarHelper>();

            foreach (var machSchdDetail in firstPass)
            {
                if (machSchdDetail.RunTimeBase.GetValueOrDefault() >= machineSchdTime &&
                    machSchdDetail.RunTimeBase.GetValueOrDefault() < wcSchdDetail.RunTimeBase.GetValueOrDefault())
                {
                    //There is potential room for these entries to reuse any shortage of time not scheduled by other machines
                    retryList.Add(machSchdDetail);
                    retryMachineCalList.Add(machineCalendars.FindCalendarByMachineId(machSchdDetail));
                    continue;
                }

                unusedTime += (machineSchdTime - machSchdDetail.RunTimeBase.GetValueOrDefault()).NotLessZero();
                if (machSchdDetail.RunTimeBase.GetValueOrDefault() <= 0)
                {
                    continue;
                }

                returnList.Add(machSchdDetail);
            }

            var totalRescheduleTime = machineSchdTime * retryMachineCalList.Count + unusedTime;
            // Recursive call
            var retryResults = GetIdealMachineSchdDetail(retryMachineCalList, wcSchdDetail, scheduleDirection, currentSchedDateTime, totalRescheduleTime, ++level);

            if (retryResults != null && retryResults.Count > 0)
            {
                returnList.AddRange(retryResults);
            }

            return returnList;
        }

        internal AMMachSchdDetail CreateMachineSchdDetail(AMWCSchdDetail wcSchdDetail,
            MachineCalendarHelper machineCalendar,
            ReadDirection scheduleDirection,
            DateTime schedDateTime, int schdTime)
        {
            if (wcSchdDetail?.WcID == null)
            {
                throw new ArgumentNullException(nameof(wcSchdDetail));
            }

            var machineSchdDetail = (AMMachSchdDetail)wcSchdDetail;
            if (machineSchdDetail == null)
            {
                return null;
            }

            machineSchdDetail.MachID = machineCalendar.Machine.MachID;

            if (schdTime <= 0)
            {
                return machineSchdDetail;
            }

            if (wcSchdDetail == null)
            {
                throw new ArgumentNullException(nameof(wcSchdDetail));
            }

            if (machineCalendar.Machine.DownFlg.GetValueOrDefault())
            {
                //Machine is down for all dates (difference than scheduling down time in machine calendar as exception)
                return machineSchdDetail;
            }

            var dateInfo = machineCalendar.GetDateInfo(schedDateTime);
            if (!dateInfo.IsWorkingDay || dateInfo.WorkingMinutes == 0)
            {
                return machineSchdDetail;
            }

            var efficiency = machineCalendar.Machine.MachEff.GetValueOrDefault();
            CalcBestSchdEfficiencyTime(schdTime, wcSchdDetail.RunTimeBase.GetValueOrDefault(), efficiency,
                out var schedulingTime, out var schedulingEfficiencyTime);

            if (schedulingEfficiencyTime == 0)
            {
                if (efficiency <= 0)
                {
                    PXTrace.WriteInformation($"Machine {machineCalendar.Machine.MachID} has an efficiency of {efficiency}");
                }
                return machineSchdDetail;
            }

            machineSchdDetail.RunTimeBase = schedulingTime;
            machineSchdDetail.RunTime = schedulingEfficiencyTime;
            machineSchdDetail.SchdBlocks = MinutesToBlocks(machineSchdDetail.RunTimeBase.GetValueOrDefault(), true);

            machineSchdDetail.PlanBlocks = MinutesToBlocks(machineSchdDetail.RunTime.GetValueOrDefault(), true);

            machineSchdDetail.StartTime = wcSchdDetail.StartTime;
            machineSchdDetail.EndTime = wcSchdDetail.EndTime;

            //TODO: if partial we need to adjust the start/end time...

            var blockMinutes = machineSchdDetail.PlanBlocks.GetValueOrDefault() * BlockSize;
            if (scheduleDirection == ReadDirection.Backward)
            {
                //adjust start time
                machineSchdDetail.StartTime = wcSchdDetail.EndTime.GetValueOrDefault().AddMinutes(blockMinutes * -1);
            }
            else
            {
                //adjust end time
                machineSchdDetail.EndTime = wcSchdDetail.StartTime.GetValueOrDefault().AddMinutes(blockMinutes);
            }

            machineSchdDetail.OrderByDate = machineSchdDetail.SchdDate.GetValueOrDefault().Date +
                                            machineSchdDetail.StartTime.GetValueOrDefault().TimeOfDay;

            return machineSchdDetail;
        }

        public static void CalcBestSchdEfficiencyTime(int timeToSchd, int maxSchdTime, decimal efficiency, out int scheduledTime, out int scheduledEfficiencyTime)
        {
            CalcBestSchdEfficiencyTime(timeToSchd, maxSchdTime, efficiency, null, out scheduledTime, out scheduledEfficiencyTime);
        }

        /// <summary>
        /// Find the best schedule time value with the efficiency
        /// </summary>
        public static void CalcBestSchdEfficiencyTime(int timeToSchd, int? maxSchdTime, decimal efficiency, decimal? size, out int scheduledTime, out int scheduledEfficiencyTime)
        {
            scheduledTime = 0;
            scheduledEfficiencyTime = 0;

            if (efficiency <= 0)
            {
                return;
            }

            // When MaxSchdTime is passed in as null/zero then there is no max limit to the time
            var limitByMaxSchdTime = maxSchdTime.GetValueOrDefault() != 0;

            var calcEfficiency = (size.GetValueOrDefault() == 0 ? 1 : size.GetValueOrDefault()) * efficiency;
            var localMaxSchdTime = limitByMaxSchdTime ? maxSchdTime.GetValueOrDefault() : timeToSchd;
            var isSchdTimeAtMax = limitByMaxSchdTime && timeToSchd >= maxSchdTime;
            decimal schedulingTime = Math.Min(localMaxSchdTime, timeToSchd);
            decimal schedulingEffTime = schedulingTime / calcEfficiency;

            if (schedulingEffTime == 0)
            {
                return;
            }

            //  It is possible the schedule with efficiency could make the time higher than available for the wc sched record...
            if (calcEfficiency < 1m && limitByMaxSchdTime && schedulingEffTime > localMaxSchdTime)
            {
                schedulingEffTime = localMaxSchdTime;
                schedulingTime = schedulingEffTime * calcEfficiency;
            }

            //  Also possible when efficiency is > 1 it might schedule short time in a day when more time is remaining to schedule. If we are looking for a full schd then max it out
            if (calcEfficiency > 1m && isSchdTimeAtMax && schedulingEffTime < schedulingTime)
            {
                schedulingEffTime = schedulingTime;
                schedulingTime = schedulingEffTime * calcEfficiency;
            }

            scheduledTime = Convert.ToInt32(schedulingTime);
            scheduledEfficiencyTime = Convert.ToInt32(schedulingEffTime);
        }

        public static AMSchdOperDetail CreateSchdOperDetail(AMSchdOper schdOper, DateTime date)
        {
            if (schdOper == null || string.IsNullOrWhiteSpace(schdOper.ProdOrdID))
            {
                throw new PXArgumentException(nameof(schdOper));
            }

            return new AMSchdOperDetail
            {
                OrderType = schdOper.OrderType,
                ProdOrdID = schdOper.ProdOrdID,
                OperationID = schdOper.OperationID,
                SchdDate = date,
                FirmSchedule = schdOper.FirmSchedule,
                IsPlan = schdOper.IsPlan,
                IsMRP = schdOper.IsMRP
            };
        }

        public static AMWCSchdDetail CreateWCSchdDetail(AMSchdOper schdOper, DateTime date, string shiftCD)
        {
            if (schdOper == null || string.IsNullOrWhiteSpace(schdOper.ProdOrdID))
            {
                throw new PXArgumentException(nameof(schdOper));
            }

            if (string.IsNullOrWhiteSpace(shiftCD))
            {
                throw new PXArgumentException(nameof(shiftCD));
            }

            return new AMWCSchdDetail
            {
                WcID = schdOper.WcID,
                ShiftCD = shiftCD,
                SiteID = schdOper.SiteID,
                SchdDate = date,
                StartTime = InfiniteSchdDateTime,
                EndTime = InfiniteSchdDateTime,
                OrderByDate = date.Date + InfiniteSchdTimeSpan
            };
        }

        protected virtual int GetTotalOperationTime(AMWC workCenter, AMShift shift, IOperationMaster operation, decimal quantity)
        {
            var totalTime = CalcTotalTime(workCenter, operation, quantity);
            totalTime += operation.SetupTime.GetValueOrDefault();
            return UseShiftCrewSizeEfficiency(workCenter) 
                ? CalcTotalEfficiencyTime(workCenter, shift, totalTime).NotLessZero() 
                : totalTime.NotLessZero();
        }

        internal static int CalcTotalEfficiencyTime(AMWC workCenter, AMShift shift, int? totalRunTime)
        {
            var size = workCenter?.WcBasis == AMWC.BasisForCapacity.Machines
                ? 1m //shift.MachNbr.GetValueOrDefault() - MachNbr not implemented
                : shift?.CrewSize;

            return CalcTotalEfficiencyTime(totalRunTime, size, shift?.ShftEff);
        }

        public static int CalcTotalEfficiencyTime(int? totalRunTime, decimal? size, decimal? efficiency)
        {
            var calcEfficiency = (size ?? 1) * (efficiency ?? 1);
            return calcEfficiency == 0 ? 0 : Convert.ToInt32(totalRunTime / calcEfficiency);
        }

        public static int CalcTotalEfficiencyTimeReverse(int? totalRunTime, decimal? size, decimal? efficiency)
        {
            var calcEfficiency = (size ?? 1) * (efficiency ?? 1);
            return calcEfficiency == 0 ? 0 : Convert.ToInt32(totalRunTime * calcEfficiency);
        }

        /// <summary>
        /// Calculate the operation total required time (in minutes) per quantity
        /// EXCLUDES SETUP TIME
        /// </summary>
        public static int CalcTotalTime(AMWC workCenter, IOperationMaster operation, decimal quantity)
        {
            if (workCenter?.WcBasis == null)
            {
                throw new PXArgumentException(nameof(workCenter));
            }

            if (operation?.OperationID == null)
            {
                throw new PXArgumentException(nameof(operation));
            }

            if (!workCenter.WcID.EqualsWithTrim(operation.WcID))
            {
                throw new PXException(Messages.WorkCenterOperationMismatch, workCenter.WcID.TrimIfNotNullEmpty(), operation.WcID.TrimIfNotNullEmpty());
            }

            if (workCenter.WcBasis == AMWC.BasisForCapacity.Machines)
            {
                return CalcTotalTime(operation.MachineUnits, operation.MachineUnitTime, quantity);
            }

            return CalcTotalTime(operation.RunUnits, operation.RunUnitTime, quantity);
        }

        /// <summary>
        /// Calculate the total required time (in minutes) per quantity
        /// </summary>
        protected static int CalcTotalTime(decimal? units, int? unitTime, decimal? quantity)
        {
            if (units.GetValueOrDefault() == 0)
            {
                return 0;
            }

            var rate = unitTime.GetValueOrDefault() / units.GetValueOrDefault();
            return Convert.ToInt32(rate * quantity.GetValueOrDefault());
        }

        /// <summary>
        /// Update the schedule quantities to match changes to order progress or qty to produce
        /// </summary>
        protected virtual void UpdateScheduleQuantities()
        {
            SchdItemUpdate();

            foreach (AMSchdItem schdItem in PXSelect<AMSchdItem, 
                Where<AMSchdItem.orderType, Equal<Current<AMProdItem.orderType>>, 
                    And<AMSchdItem.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>.Select(this))
            {
                SchdOperUpdate(schdItem);
            }
        }

        public static void UpdateScheduleQuantities(PXGraph graph, AMProdItem prodItem)
        {
            var schdItems = PXSelect<AMSchdItem,
                    Where<AMSchdItem.orderType, Equal<Required<AMSchdItem.orderType>>,
                        And<AMSchdItem.prodOrdID, Equal<Required<AMSchdItem.prodOrdID>>>>,
                    OrderBy<Asc<AMSchdItem.schdID>>>.Select(graph, prodItem?.OrderType, prodItem?.ProdOrdID)
                ?.FirstTableItems?.ToList();
            if (schdItems == null)
            {
                return;
            }
            
            SchdItemUpdate(graph, prodItem, schdItems);

            foreach (var schdItem in schdItems)
            {
                SchdOperUpdate(graph, schdItem);
            }
        }

        public static void DeleteOrderSchedule(AMProdItem amProdItem)
        {
            var graph = Construct();
            graph.DeleteSchedule(amProdItem);
            if (graph.IsDirty)
            {
                graph.Persist();
            }
        }

        public static void DeleteOrderSchedule(AMSchdItem schdItem)
        {
            var graph = Construct();
            graph.DeleteSchedule(schdItem);
            if (graph.IsDirty)
            {
                graph.Persist();
            }
        }

        public virtual void DeleteSchedule(AMProdItem amProdItem)
        {
            foreach (AMSchdItem amSchdItem in PXSelect<AMSchdItem, 
                Where<AMSchdItem.orderType, Equal<Required<AMSchdItem.orderType>>, 
                    And<AMSchdItem.prodOrdID, Equal<Required<AMSchdItem.prodOrdID>>>
                >>.Select(this, amProdItem.OrderType, amProdItem.ProdOrdID))
            {
                DeleteSchedule(amSchdItem);
                SchdItems.Delete(amSchdItem);
            }
        }

        public virtual void DeleteSchedule(List<AMSchdItem> schdItems, bool deleteSchdOpers)
        {
            if (schdItems == null)
            {
                return;
            }

            foreach (var schdItem in schdItems)
            {
                DeleteSchedule(schdItem, deleteSchdOpers);

				var cachedSchdItem = SchdItems.Cache.LocateElse(schdItem);
				cachedSchdItem.ScheduleStatus = ProductionScheduleStatus.Unscheduled;
				SchdItems.Update(cachedSchdItem);
            }
        }

        /// <summary>
        /// Delete prior schedule
        /// </summary>
        protected virtual void DeleteSchedule(AMSchdItem schdItem)
        {
            DeleteSchedule(schdItem, true);
        }

        /// <summary>
        /// Delete prior schedule
        /// </summary>
        protected virtual void DeleteSchedule(AMSchdItem schdItem, bool deleteSchdOper)
        {
            if (schdItem == null || string.IsNullOrWhiteSpace(schdItem.ProdOrdID))
            {
                return;
            }

            foreach (var row in GetSchdOperations(schdItem))
            {
                var schdOperation = SchdOpers.Cache.LocateElse(row);
                DeleteSchdOperationDetail(schdOperation);

                if (schdOperation?.OperationID == null || !deleteSchdOper)
                {
                    continue;
                }

                SchdOpers.Delete(schdOperation);
            }
        }

        protected virtual void DeleteSchdOperationDetail(AMSchdOper schdOper)
        {
            if (string.IsNullOrWhiteSpace(schdOper?.ProdOrdID))
            {
                return;
            }

            DeleteMachSchdDetail(schdOper);
            DeleteToolSchdDetail(schdOper);

            foreach (PXResult<AMSchdOperDetail, AMWCSchdDetail> result in PXSelectJoin<
                AMSchdOperDetail,
                LeftJoin<AMWCSchdDetail, 
                    On<AMSchdOperDetail.schdKey, Equal<AMWCSchdDetail.schdKey>>>, 
                Where<AMSchdOperDetail.orderType, Equal<Required<AMSchdOperDetail.orderType>>,
                    And<AMSchdOperDetail.prodOrdID, Equal<Required<AMSchdOperDetail.prodOrdID>>,
                    And<AMSchdOperDetail.operationID, Equal<Required<AMSchdOperDetail.operationID>>>>>>
                .Select(this, schdOper.OrderType, schdOper.ProdOrdID, schdOper.OperationID))
            {
                var schdOperDetail = (AMSchdOperDetail) result;
                var wcSchdDetail = (AMWCSchdDetail) result;

                if (schdOperDetail == null || string.IsNullOrWhiteSpace(schdOperDetail.ProdOrdID))
                {
                    continue;
                }

                SchdOperDetails.Delete(schdOperDetail);

                if (wcSchdDetail == null || string.IsNullOrWhiteSpace(wcSchdDetail.WcID))
                {
                    continue;
                }

                WorkCenterSchdDetailRecs.Delete(wcSchdDetail);    
            }
        }

        protected virtual void DeleteMachSchdDetail(AMSchdOper schdOper)
        {
            DeleteMachSchdDetail(MachineSchdDetailRecs.Cache, schdOper);
        }

        internal static void DeleteMachSchdDetail(PXCache cache, IProdOper oper)
        {
            foreach (AMMachSchdDetail machSchdDetail in PXSelectJoin<
                    AMMachSchdDetail,
                    InnerJoin<AMSchdOperDetail,
                        On<AMMachSchdDetail.schdKey, Equal<AMSchdOperDetail.schdKey>>>,
                    Where<AMSchdOperDetail.orderType, Equal<Required<AMSchdOperDetail.orderType>>,
                        And<AMSchdOperDetail.prodOrdID, Equal<Required<AMSchdOperDetail.prodOrdID>>,
                            And<AMSchdOperDetail.operationID, Equal<Required<AMSchdOperDetail.operationID>>>>>>
                .Select(cache.Graph, oper?.OrderType, oper?.ProdOrdID, oper?.OperationID))
            {
                if (machSchdDetail?.MachID == null)
                {
                    continue;
                }

                cache.Delete(machSchdDetail);
            }
        }

        protected virtual void DeleteToolSchdDetail(AMSchdOper schdOper)
        {
            DeleteToolSchdDetail(ToolSchdDetailRecs.Cache, schdOper);
        }

        internal static void DeleteToolSchdDetail(PXCache cache, IProdOper oper)
        {
            foreach (AMToolSchdDetail schdDetail in PXSelectJoin<
                    AMToolSchdDetail,
                    InnerJoin<AMSchdOperDetail,
                        On<AMToolSchdDetail.schdKey, Equal<AMSchdOperDetail.schdKey>>>,
                    Where<AMSchdOperDetail.orderType, Equal<Required<AMSchdOperDetail.orderType>>,
                        And<AMSchdOperDetail.prodOrdID, Equal<Required<AMSchdOperDetail.prodOrdID>>,
                            And<AMSchdOperDetail.operationID, Equal<Required<AMSchdOperDetail.operationID>>>>>>
                .Select(cache.Graph, oper?.OrderType, oper?.ProdOrdID, oper?.OperationID))
            {
                if (schdDetail?.ToolID == null)
                {
                    continue;
                }

                cache.Delete(schdDetail);
            }
        }

        internal static AMSchdItem CreateSchdItem(AMProdItem prodItem)
        {
            return new AMSchdItem
            {
                OrderType = prodItem.OrderType,
                ProdOrdID = prodItem.ProdOrdID,
                InventoryID = prodItem.InventoryID,
                SiteID = prodItem.SiteID,
                ConstDate = prodItem.ConstDate,
                StartDate = prodItem.StartDate,
                EndDate = prodItem.EndDate,
                SchedulingMethod = prodItem.SchedulingMethod,
                QtyComplete = prodItem.BaseQtyComplete,
                QtyScrapped = prodItem.BaseQtyScrapped,
                QtytoProd = prodItem.BaseQtytoProd
            };
        }

        protected virtual void SchdItemUpdate()
        {
            if (ProdItems?.Current?.OrderType == null)
            {
                return;
            }

            var schdItems = new List<AMSchdItem>();

            foreach (AMSchdItem result in PXSelect<
                AMSchdItem,
                Where<AMSchdItem.orderType, Equal<Required<AMSchdItem.orderType>>,
                    And<AMSchdItem.prodOrdID, Equal<Required<AMSchdItem.prodOrdID>>>>,
                OrderBy<
                    Asc<AMSchdItem.schdID>>>
                .Select(this, ProdItems.Current.OrderType, ProdItems.Current.ProdOrdID))
            {
                schdItems.Add(result);
            }

            if (schdItems.Count == 0)
            {
                var newAmSchdItem = new AMSchdItem
                {
                    OrderType = ProdItems.Current.OrderType,
                    ProdOrdID = ProdItems.Current.ProdOrdID,
                    InventoryID = ProdItems.Current.InventoryID,
                    SiteID = ProdItems.Current.SiteID,
                    ConstDate = ProdItems.Current.ConstDate,
                    StartDate = ProdItems.Current.StartDate,
                    EndDate = ProdItems.Current.EndDate,
                    SchedulingMethod = ProdItems.Current.SchedulingMethod,
                    QtyComplete = ProdItems.Current.BaseQtyComplete,
                    QtyScrapped = ProdItems.Current.BaseQtyScrapped,
                    QtytoProd = ProdItems.Current.BaseQtytoProd
                };

                newAmSchdItem = SchdItems.Insert(newAmSchdItem);
                if (newAmSchdItem != null)
                {
                    schdItems.Add(newAmSchdItem);
                }
            }

            SchdItemUpdate(ProdItems.Current, schdItems);
        }

        protected virtual void SchdItemUpdate(AMProdItem prodItem, List<AMSchdItem> schdItems)
        {
            SchdItemUpdate(this, prodItem, schdItems);
        }

        public static void SchdItemUpdate(PXGraph graph, AMProdItem prodItem, List<AMSchdItem> schdItems)
        {
            if (prodItem?.OrderType == null || graph == null || schdItems == null || schdItems.Count == 0)
            {
                return;
            }

            Common.Cache.AddCacheView<AMSchdItem>(graph);

            AMPSetup ampSetup = PXSelect<AMPSetup>.Select(graph);

            var schdQtyToProd = 0m;
            var schdQtyComplete = 0m;
            var schdQtyScrapped = 0m;

            foreach (var result in schdItems)
            {
                schdQtyToProd += result.QtytoProd.GetValueOrDefault();
                schdQtyComplete += result.QtyComplete.GetValueOrDefault();
                schdQtyScrapped += result.QtyScrapped.GetValueOrDefault();
            }

            // Adjust schedule record to reflect any changes to the production order qty
            var adjustedSchdQtyToProd = prodItem.BaseQtytoProd.GetValueOrDefault() - schdQtyToProd;
            var adjustedSchdQtyToComplete = prodItem.BaseQtyComplete.GetValueOrDefault() - schdQtyComplete;
            var adjustedSchdQtyScrapped = prodItem.BaseQtyScrapped.GetValueOrDefault() - schdQtyScrapped;

            if (adjustedSchdQtyToProd > 0 && schdItems.Count > 0)
            {
                var schdItem = graph.Caches<AMSchdItem>().LocateElse(schdItems[0]);
                if (schdItem != null)
                {
                    schdItem.QtytoProd += adjustedSchdQtyToProd;
                    graph.Caches<AMSchdItem>().Update(schdItem);
                }
            }

            if (adjustedSchdQtyToProd < 0 && schdItems.Count > 0)
            {
                foreach (var row in schdItems.OrderByDescending(x => x.SchdID))
                {
                    var schdItem = graph.Caches<AMSchdItem>().LocateElse(row);
                    if (adjustedSchdQtyToProd == 0)
                    {
                        break;
                    }

                    if (schdItem.QtytoProd.GetValueOrDefault() > Math.Abs(adjustedSchdQtyToProd))
                    {
                        schdItem.QtytoProd += adjustedSchdQtyToProd;
                        graph.Caches<AMSchdItem>().Update(schdItem);
                        adjustedSchdQtyToProd = 0m;
                        continue;
                    }

                    schdQtyComplete += schdItem.QtyComplete.GetValueOrDefault();
                    schdQtyScrapped += schdItem.QtyScrapped.GetValueOrDefault();

                    adjustedSchdQtyToProd += schdItem.QtytoProd.GetValueOrDefault();
                    graph.Caches<AMSchdItem>().Delete(schdItem);
                }
            }

            if (adjustedSchdQtyToComplete != 0 || adjustedSchdQtyScrapped != 0)
            {
                var list = adjustedSchdQtyToComplete <= 0 ? schdItems.OrderBy(x => x.SchdID).ToList() : schdItems.OrderByDescending(x => x.SchdID).ToList();
                var minSchdId = list.Min(schd => schd.SchdID.GetValueOrDefault());

                var counter = 0;
                foreach (var row in list)
                {
                    var schdItem = graph.Caches<AMSchdItem>().LocateElse(row);
                    if (graph.Caches<AMSchdItem>().GetStatus(schdItem) == PXEntryStatus.Deleted)
                    {
                        continue;
                    }
                    counter++;

                    var currentQtyToProd = schdItem.QtytoProd.GetValueOrDefault();
                    if (adjustedSchdQtyToComplete < 0)
                    {
                        currentQtyToProd = Math.Abs(adjustedSchdQtyToComplete);
                        if (currentQtyToProd > schdItem.QtyComplete.GetValueOrDefault())
                        {
                            currentQtyToProd = schdItem.QtyComplete.GetValueOrDefault();
                        }
                        adjustedSchdQtyToComplete += currentQtyToProd;
                        schdItem.QtyComplete -= currentQtyToProd;
                    }

                    if (adjustedSchdQtyScrapped < 0)
                    {
                        currentQtyToProd = (adjustedSchdQtyScrapped > schdItem.QtyScrapped.GetValueOrDefault())
                            ? schdItem.QtyScrapped.GetValueOrDefault()
                            : adjustedSchdQtyScrapped;

                        adjustedSchdQtyScrapped += currentQtyToProd;

                        schdItem.QtyScrapped -= Math.Abs(currentQtyToProd);
                    }

                    var currentQtyComplete = schdItem.QtyComplete.GetValueOrDefault() + (ampSetup?.InclScrap == true ? schdItem.QtyScrapped.GetValueOrDefault() : 0);

                    if ((currentQtyToProd > currentQtyComplete || counter == list.Count)
                        && (adjustedSchdQtyToComplete > 0 || adjustedSchdQtyScrapped > 0))
                    {
                        currentQtyComplete = adjustedSchdQtyToComplete > 0 ? adjustedSchdQtyToComplete : 0;
                        var currentQtyScrapped = adjustedSchdQtyScrapped > 0 ? adjustedSchdQtyScrapped : 0;
                        schdItem.QtyComplete += currentQtyComplete;
                        schdItem.QtyScrapped += currentQtyScrapped;
                        adjustedSchdQtyToComplete -= currentQtyComplete;
                        adjustedSchdQtyScrapped -= currentQtyScrapped;
                    }

                    if (schdItem.SchdID == minSchdId)
                    {
                        schdItem.ConstDate = prodItem.ConstDate;
                        schdItem.SchedulingMethod = prodItem.SchedulingMethod;
                    }

                    graph.Caches<AMSchdItem>().Update(schdItem);
                }
            }
        }

        protected static AMSchdOper CopySchdOper(AMSchdOper schdOper, AMProdOper amProdOper)
        {
            return new AMSchdOper
            {
                OperationID = amProdOper.OperationID,
                SortOrder = schdOper.SortOrder,
                LineNbr = schdOper.LineNbr,
                OrderType = schdOper.OrderType,
                ProdOrdID = schdOper.ProdOrdID,
                SchdID = schdOper.SchdID,
                QtytoProd = schdOper.QtytoProd,
                IsPlan = schdOper.IsPlan,
                MoveTime = amProdOper.MoveTime,
                QueueTime = amProdOper.QueueTime,
                FinishTime = amProdOper.FinishTime,
                SetupTime = amProdOper.SetupTime,
                WcID = amProdOper.WcID,
                SiteID = schdOper.SiteID,
                FirmSchedule = schdOper.FirmSchedule
            };
        }

        protected static AMSchdOper ConstructSchdOperationFromProdOper(AMSchdItem amSchdItem, AMProdOper amProdOper)
        {
            return SetSchdOperationFromProdOper(amProdOper, new AMSchdOper
            {
                QtytoProd = amSchdItem.QtytoProd.GetValueOrDefault(),
                SchdID = amSchdItem.SchdID.GetValueOrDefault(),
                SchedulingMethod = ScheduleMethod.StartOn,
                SiteID = amSchdItem.SiteID,
                FirmSchedule = amSchdItem.FirmSchedule,
                StartDate = amSchdItem.StartDate,
                EndDate = amSchdItem.EndDate
            });
        }

        internal static AMSchdOper ConstructSchdOperationFromProdOper(AMProdItem prodItem, AMProdOper amProdOper)
        {
            return SetSchdOperationFromProdOper(amProdOper, new AMSchdOper
            {
                QtytoProd = prodItem.BaseQtytoProd.GetValueOrDefault(),
                SchedulingMethod = ScheduleMethod.StartOn,
                SiteID = prodItem.SiteID,
                FirmSchedule = prodItem.FirmSchedule,
                StartDate = prodItem.StartDate,
                EndDate = prodItem.EndDate
            });
        }

        internal static AMSchdOper SetSchdOperationFromProdOper(AMProdOper amProdOper, AMSchdOper schdOper)
        {
            if (schdOper == null)
            {
                schdOper = new AMSchdOper();
            }

            schdOper.OrderType = amProdOper.OrderType;
            schdOper.ProdOrdID = amProdOper.ProdOrdID;
            schdOper.OperationID = amProdOper.OperationID;
            schdOper.SchedulingMethod = ScheduleMethod.StartOn;
            schdOper.MoveTime = amProdOper.MoveTime;
            schdOper.QueueTime = amProdOper.QueueTime;
            schdOper.FinishTime = amProdOper.FinishTime;
            schdOper.SetupTime = amProdOper.SetupTime;
            schdOper.WcID = amProdOper.WcID;
            schdOper.SortOrder = OperationHelper.ToCalculatedOperationID(amProdOper.OperationCD);
            schdOper.StartDate = schdOper.StartDate ?? amProdOper.StartDate;
            schdOper.EndDate = schdOper.EndDate ?? amProdOper.EndDate;

            return schdOper;
        }

        protected virtual void SchdOperUpdate(AMSchdItem schdItem)
        {
            SchdOperUpdate(this, schdItem);
        }

        protected static void SchdOperUpdate(PXGraph graph, AMSchdItem schdItem)
        {
            if (schdItem == null)
            {
                return;
            }

            Common.Cache.AddCacheView<AMSchdOper>(graph);

            var allSchdOpers = PXSelect<AMSchdOper,
                Where<AMSchdOper.orderType, Equal<Required<AMSchdOper.orderType>>,
                    And<AMSchdOper.prodOrdID, Equal<Required<AMSchdOper.prodOrdID>>>>
            >.Select(graph, schdItem.OrderType, schdItem.ProdOrdID).ToFirstTableList().LocateElse(graph);
            if (allSchdOpers == null)
            {
                allSchdOpers = new List<AMSchdOper>();
            }

            foreach (AMProdOper operResult in PXSelect<AMProdOper,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>>
                >.Select(graph, schdItem.OrderType, schdItem.ProdOrdID))
            {
                var prodOper = graph.Caches<AMProdOper>().LocateElse(operResult);

                var schdOpers = allSchdOpers.Where(x => x.OperationID == prodOper.OperationID && x.SchdID == schdItem.SchdID).ToList();

                if (schdOpers.Count == 0 )
                {
                    var newSchdOper = ConstructSchdOperationFromProdOper(schdItem, prodOper);
                    if (newSchdOper != null)
                    {
                        newSchdOper = (AMSchdOper)graph.Caches<AMSchdOper>().Insert(newSchdOper);
                        schdOpers.Add(newSchdOper);
                        allSchdOpers.Add(newSchdOper);
                    }
                }

                foreach (var schdOper in schdOpers)
                {
                    var updated = false;
                    if (schdOper.SiteID != schdItem.SiteID)
                    {
                        updated = true;
                        schdOper.SiteID = schdItem.SiteID;
                    }
                    if (schdOper.WcID != prodOper.WcID)
                    {
                        updated = true;
                        schdOper.WcID = prodOper.WcID;
                    }
                    if (schdOper.MoveTime != prodOper.MoveTime)
                    {
                        updated = true;
                        schdOper.MoveTime = prodOper.MoveTime;
                    }
                    if (schdOper.QueueTime != prodOper.QueueTime)
                    {
                        updated = true;
                        schdOper.QueueTime = prodOper.QueueTime;
                    }

                    if (schdOper.tstamp == null)
                    {
                        schdOper.tstamp = PXDatabase.SelectTimeStamp();
                    }

                    if (updated)
                    {
                        graph.Caches<AMSchdOper>().Update(schdOper);
                    }
                }

                SchdOperUpdateToProduce(graph, schdItem, prodOper, schdOpers);
                SchdOperUpdateComplete(graph, schdItem, prodOper, schdOpers);
            }
        }

        protected static void SchdOperUpdateComplete(PXGraph graph, AMSchdItem schdItem, AMProdOper amProdOper, List<AMSchdOper> schdOpers)
        {
            if (amProdOper == null)
            {
                return;
            }

            var currentQtyComplete = schdOpers.Sum(x => x.QtyComplete.GetValueOrDefault());
            var currentQtyScrapped = schdOpers.Sum(x => x.QtyScrapped.GetValueOrDefault());
            var adjustQtyComplete = amProdOper.BaseQtyComplete.GetValueOrDefault() - currentQtyComplete;
            var adjustQtyScrapped = amProdOper.BaseQtyScrapped.GetValueOrDefault() - currentQtyScrapped;

            if (adjustQtyComplete == 0 && adjustQtyScrapped == 0)
            {
                return;
            }

            var orderedSchdOpers = schdOpers.Count == 1
                ? schdOpers
                : adjustQtyComplete <= 0m ? schdOpers.OrderBy(x => x.StartDate).ToList() : schdOpers.OrderByDescending(x => x.StartDate).ToList();

            var schdOperCounter = 0;
            foreach (var schdOperRow in orderedSchdOpers)
            {
                var schdOper = graph.Caches<AMSchdOper>().LocateElse(schdOperRow);
                schdOperCounter++;
                var schdOperQtyToPRod = schdOper.QtytoProd.GetValueOrDefault();

                if (adjustQtyComplete < 0)
                {
                    schdOperQtyToPRod = adjustQtyComplete*-1;

                    if (schdOperQtyToPRod > schdOper.QtyComplete.GetValueOrDefault())
                    {
                        schdOperQtyToPRod = schdOper.QtyComplete.GetValueOrDefault();
                    }
                    adjustQtyComplete += schdOperQtyToPRod;
                    schdOper.QtyComplete -= schdOperQtyToPRod;
                }
                if (adjustQtyScrapped < 0)
                {
                    schdOperQtyToPRod = adjustQtyScrapped;
                    if (schdOperQtyToPRod > schdOper.QtyScrapped.GetValueOrDefault())
                    {
                        schdOperQtyToPRod = schdOper.QtyScrapped.GetValueOrDefault();
                    }
                    adjustQtyScrapped += schdOperQtyToPRod;
                    schdOper.QtyScrapped -= Math.Abs(schdOperQtyToPRod);                  
                }

                if (schdOperQtyToPRod <= schdOper.QtyComplete.GetValueOrDefault() &&
                    schdOperCounter != orderedSchdOpers.Count ||
                    adjustQtyComplete <= 0 && adjustQtyScrapped <= 0)
                {
                    graph.Caches<AMSchdOper>().Update(schdOper);
                    continue;
                }

                var currentComplete = adjustQtyComplete > 0 ? adjustQtyComplete : 0m;
                var currentScrap = adjustQtyScrapped > 0 ? adjustQtyScrapped : 0m;

                schdOper.QtyComplete += currentComplete;
                schdOper.QtyScrapped += currentScrap;

                adjustQtyComplete -= currentComplete;
                adjustQtyScrapped -= currentScrap;
                graph.Caches<AMSchdOper>().Update(schdOper);
            }
        }

        protected static void SchdOperUpdateToProduce(PXGraph graph, AMSchdItem schdItem, AMProdOper amProdOper, List<AMSchdOper> schdOpers)
        {
            if (schdItem == null || amProdOper == null || schdOpers == null || schdOpers.Count == 0)
            {
                return;
            }

            var currentSchdQtyToProd = schdOpers.Sum(x => x.QtytoProd.GetValueOrDefault());
            var adjustQtyToProd = schdItem.QtytoProd.GetValueOrDefault() - currentSchdQtyToProd;
            if (adjustQtyToProd == 0)
            {
                return;
            }

            var adjustUp = adjustQtyToProd > 0m;
            if (!adjustUp)
            {
                adjustQtyToProd = adjustQtyToProd * -1;
            }

            var orderedSchdOpers = schdOpers.Count == 1
                ? schdOpers
                : adjustUp ? schdOpers.OrderBy(x => x.LineNbr).ToList() : schdOpers.OrderByDescending(x => x.LineNbr).ToList();

            foreach (var schdOperRow in orderedSchdOpers)
            {
                var schdOper = graph.Caches<AMSchdOper>().LocateElse(schdOperRow);
                if (adjustUp)
                {
                    schdOper.QtytoProd += adjustQtyToProd;
                    graph.Caches<AMSchdOper>().Update(schdOper);
                    break;
                }

                if (schdOper.QtytoProd.GetValueOrDefault() > adjustQtyToProd)
                {
                    schdOper.QtytoProd -= adjustQtyToProd;
                    graph.Caches<AMSchdOper>().Update(schdOper);
                    adjustQtyToProd = 0m;
                    continue;
                }

                graph.Caches<AMSchdOper>().Delete(schdOper);
                adjustQtyToProd -= schdOper.QtytoProd.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Let the values of the machines for a given work center control the overall total values for the work center schd record
        /// </summary>
        protected virtual AMWCSchd UpdateWCSchdFromMachSchds(List<AMMachSchd> machineSchds, AMWCSchd wcSchd)
        {
            if (wcSchd == null || machineSchds == null || machineSchds.Count == 0)
            {
                return wcSchd;
            }

            wcSchd.TotalBlocks = 0;
            wcSchd.WorkTime = 0;
            wcSchd.StartTime = null;
            wcSchd.EndTime = null;

            foreach (var machineSchd in machineSchds)
            {
                wcSchd.TotalBlocks += machineSchd.TotalBlocks.GetValueOrDefault();
                wcSchd.WorkTime += machineSchd.WorkTime.GetValueOrDefault();

                wcSchd.StartTime = wcSchd.StartTime == null
                    ? machineSchd.StartTime
                    : machineSchd.StartTime.LesserDateTime(wcSchd.StartTime);

                wcSchd.EndTime = wcSchd.EndTime == null
                    ? machineSchd.EndTime
                    : machineSchd.EndTime.GreaterDateTime(wcSchd.EndTime);
            }

            return wcSchd;
        }

        protected virtual AMWCSchd GetWorkCenterSchd(AMSchdOper schdOperation, string shiftCD, AMDateInfo dateInfo)
        {
            if (schdOperation == null || string.IsNullOrWhiteSpace(schdOperation.WcID))
            {
                throw new PXArgumentException(nameof(schdOperation));
            }

            return GetWorkCenterSchd(schdOperation.WcID, shiftCD, schdOperation.SiteID, dateInfo);
        }

        protected virtual AMWCSchd GetWorkCenterSchd(string workCenterID, string shiftCD, int? siteID, AMDateInfo dateInfo)
        {
            return GetWorkCenterSchd(workCenterID, shiftCD, siteID, dateInfo, dateInfo.WorkingMinutes);
        }

        protected virtual AMWCSchd GetWorkCenterSchd(string workCenterID, string shiftCD, int? siteID, AMDateInfo dateInfo, int workingMinutes)
        {
            if (string.IsNullOrWhiteSpace(workCenterID))
            {
                throw new PXArgumentException(nameof(workCenterID));
            }

            if (string.IsNullOrWhiteSpace(shiftCD))
            {
                throw new PXArgumentException(nameof(shiftCD));
            }

            if (siteID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            if (Common.Dates.IsDefaultDate(dateInfo.Date))
            {
                throw new PXArgumentException(nameof(dateInfo));
            }

            var workCenterSchd = WorkCenterSchdRecs.Cache.LocateElseCopy((AMWCSchd)PXSelect<AMWCSchd,
                    Where<AMWCSchd.wcID, Equal<Required<AMWCSchd.wcID>>,
                    And<AMWCSchd.shiftCD, Equal<Required<AMWCSchd.shiftCD>>,
                    And<AMWCSchd.schdDate, Equal<Required<AMWCSchd.schdDate>>>>>
                    >.Select(this, workCenterID, shiftCD, dateInfo.Date));

            var totalBlocks = MinutesToBlocks(workingMinutes, false);
            if (workCenterSchd == null)
            {
                workCenterSchd = new AMWCSchd
                {
                    WcID = workCenterID,
                    ShiftCD = shiftCD,
                    SchdDate = dateInfo.Date,
                    SiteID = siteID,
                    TotalBlocks = totalBlocks
                };
                workCenterSchd = this.WorkCenterSchdRecs.Insert(workCenterSchd);
            }

            workCenterSchd.TotalBlocks = totalBlocks;
            workCenterSchd.WorkTime = dateInfo.WorkingMinutes;
            workCenterSchd.StartTime = new DateTime(1900, 1, 1) + dateInfo.StartTime;
            workCenterSchd.EndTime = new DateTime(1900, 1, 1) + dateInfo.EndTime;
            workCenterSchd.ExceptionDate = dateInfo.IsExceptionDate;

            workCenterSchd = this.WorkCenterSchdRecs.Update(workCenterSchd);

            WorkCenterSchdRecs.Current = workCenterSchd;
            return workCenterSchd;
        }

        protected virtual IEnumerable<AMMachSchd> GetMachineSchds(List<MachineCalendarHelper> machineCalendars, DateTime date)
        {
            if ((machineCalendars?.Count ?? 0) == 0)
            {
                yield break;
            }

            foreach (var machineCalendar in machineCalendars)
            {
                if (machineCalendar == null)
                {
                    continue;
                }

                yield return GetMachineSchd(machineCalendar.Machine?.MachID, machineCalendar.WorkCenter?.SiteID, machineCalendar.GetDateInfo(date));
            }
        }

        protected virtual AMMachSchd GetMachineSchd(string machineId, int? siteId, AMDateInfo dateInfo)
        {
            if (string.IsNullOrWhiteSpace(machineId))
            {
                throw new PXArgumentException(nameof(machineId));
            }

            if (siteId.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(siteId));
            }

            if (Common.Dates.IsDefaultDate(dateInfo.Date))
            {
                throw new PXArgumentException(nameof(dateInfo));
            }

            var machineSchd = MachineSchdRecs.Cache.LocateElseCopy((AMMachSchd)PXSelect<AMMachSchd,
                    Where<AMMachSchd.machID, Equal<Required<AMMachSchd.machID>>,
                    And<AMMachSchd.schdDate, Equal<Required<AMMachSchd.schdDate>>>>
                    >.Select(this, machineId, dateInfo.Date));

            var totalBlocks = MinutesToBlocks(dateInfo.WorkingMinutes, false);
            if (machineSchd == null)
            {
                machineSchd = new AMMachSchd
                {
                    MachID = machineId,
                    SchdDate = dateInfo.Date,
                    SiteID = siteId,
                    TotalBlocks = totalBlocks
                };
                machineSchd = MachineSchdRecs.Insert(machineSchd);
            }

            machineSchd.WorkTime = dateInfo.WorkingMinutes;
            machineSchd.TotalBlocks = totalBlocks;
            machineSchd.StartTime = new DateTime(1900, 1, 1) + dateInfo.StartTime;
            machineSchd.EndTime = new DateTime(1900, 1, 1) + dateInfo.EndTime;
            machineSchd.ExceptionDate = dateInfo.IsExceptionDate;

            MachineSchdRecs.Current = machineSchd;
            return machineSchd;
        }

        protected void UpdateMaterialSplitDate(List<PXResult<AMProdMatlSplit, AMProdMatl>> splitResults)
        {
            if (splitResults == null || splitResults.Count == 0)
            {
                return;
            }

            foreach (var result in splitResults)
            {
                var locatedProdMatl = ProdMatls.Cache.LocateElse((AMProdMatl)result);
                if (locatedProdMatl == null)
                {
                    continue;
                }

				var prodOper = ProdOpers.Cache.Cached.RowCast<AMProdOper>()
					.Where(r => r.OrderType == locatedProdMatl.OrderType
						&& r.ProdOrdID == locatedProdMatl.ProdOrdID
						&& r.OperationID == locatedProdMatl.OperationID).FirstOrDefault();
				if (prodOper == null)
                {
					prodOper = AMProdOper.PK.Find(this, locatedProdMatl.OrderType, locatedProdMatl.ProdOrdID, locatedProdMatl.OperationID);
				}

                if (prodOper?.StartDate == null)
                {
                    continue;
                }

                ProdMatls.Current = locatedProdMatl;
                locatedProdMatl.TranDate = prodOper.StartDate.GetValueOrDefault().Date;
                ProdMatls.Update(locatedProdMatl);

                var locatedProdMatlSplit = ProdMatlSplits.Cache.LocateElse((AMProdMatlSplit)result);
                if (locatedProdMatlSplit == null)
                {
                    continue;
                }
                ProdMatlSplits.Current = locatedProdMatlSplit;
                locatedProdMatlSplit.TranDate = prodOper.StartDate;
                ProdMatlSplits.Update(locatedProdMatlSplit);
            }
        }

		protected class RemoveMultiFormulaAggregrateScope<TAttribute> : IDisposable
			where TAttribute : PXFormulaAttribute
		{
			public PXCache Cache { get; }

			protected List<FieldAggregateValues> _oldAggregateValues;

			protected class FieldAggregateValues
			{
				public Type Field { get; set; }
				public Dictionary<string, Type> AggregateValues { get; set; }
			}

			public RemoveMultiFormulaAggregrateScope(PXCache cache, IEnumerable<Type> fields)
			{
				Cache = cache ?? throw new ArgumentNullException(nameof(cache));
				_oldAggregateValues = new List<FieldAggregateValues>();
				foreach(Type field in fields)
				{
					var fieldAggValues = new FieldAggregateValues
					{
						Field = field ?? throw new ArgumentNullException(nameof(field)),
						AggregateValues = new Dictionary<string, Type>()
					};

					if (!typeof(IBqlField).IsAssignableFrom(field))
					{
						// Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
						throw new PXException($"The type {field.FullName} is not a BQL field.");
					}

					IEnumerable<TAttribute> attributesOfType = this.Cache
						.GetAttributesReadonly(field.Name)
						.OfType<TAttribute>();

					if (!attributesOfType.Any())
					{
						return;
					}

					attributesOfType.ForEach(attribute =>
					{
						fieldAggValues.AggregateValues[attribute.FormulaFieldName] = attribute.Aggregate;
						attribute.Aggregate = null;
					});
					_oldAggregateValues.Add(fieldAggValues);
				}

			}

			public void Dispose()
			{
				foreach(var fieldAggValues in _oldAggregateValues)
				{
					IEnumerable<TAttribute> attributesOfType = Cache
						.GetAttributes(fieldAggValues.Field.Name)
						.OfType<TAttribute>();

					foreach (TAttribute attribute in attributesOfType)
					{
						var oldValue = fieldAggValues.AggregateValues[attribute.FormulaFieldName];
						attribute.Aggregate = oldValue;
					}
				}

			}
		}
    }
}
