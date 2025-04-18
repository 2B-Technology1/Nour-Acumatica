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
using PX.Objects.CR;
using PX.Objects.FS.Scheduler;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.CS;
using System.Text;
using PX.Objects.AR;
using PX.Objects.PM;

namespace PX.Objects.FS
{
    public class ServiceContractScheduleEntryBase<TGraph, TPrimary, TScheduleID, TEntityID, TCustomerID> : PXGraph<TGraph, TPrimary>
        where TGraph : PX.Data.PXGraph
        where TPrimary : class, PX.Data.IBqlTable, new()
        where TScheduleID : PX.Data.IBqlField
        where TEntityID : PX.Data.IBqlField
        where TCustomerID : PX.Data.IBqlField
    {
        public bool statusChanged = false;

		public bool IsCopyContract
		{
			get;
			set;
		}

		#region Selects
		[PXHidden]
        public PXSelect<BAccount> BAccount;
        [PXHidden]
        public PXSelect<Contact> Contact;
        [PXHidden]
        public PXSelect<FSSchedule> Schedule;

        [PXHidden]
        public PXSetup<PX.Objects.GL.Company> companySetup;

        public PXFilter<FromToFilter> FromToFilter;

        // Baccount workaround
        [PXHidden]
        public PXSetup<FSSetup> Setup;
        [PXHidden]
        public PXSelect<FSSalesPrice> salesPriceRecords;

        public PXSelectJoin<TPrimary,
            LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<TCustomerID>>>,
            Where2<
                Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>,
                And<TEntityID, Equal<Optional<TEntityID>>>>> ContractScheduleRecords;

        public PXSelect<TPrimary, 
               Where<
                   TScheduleID, Equal<Current<TScheduleID>>>> ContractScheduleSelected;

        public PXSetup<FSSrvOrdType>.Where<
               Where<
                   FSSrvOrdType.srvOrdType, Equal<Current<FSSchedule.srvOrdType>>>> SrvOrdTypeSelected;

        public PXSetup<FSServiceContract>.Where<
               Where<
                   FSServiceContract.serviceContractID, Equal<Current<TEntityID>>>> CurrentServiceContract;

        #region ScheduleDetU
        [PXDynamicButton(new string[] { PasteLineCommand, ResetOrderCommand },
                     new string[] { PX.Data.ActionsMessages.PasteLine, PX.Data.ActionsMessages.ResetOrder },
                     TranslationKeyType = typeof(PX.Objects.Common.Messages))]
        public class ScheduleDetOrdered : PXOrderedSelect<TPrimary, FSScheduleDet,
                                          Where<
                                              FSScheduleDet.scheduleID, Equal<Current<TScheduleID>>>,
                                          OrderBy<
                                              Asc<FSScheduleDet.sortOrder, 
                                              Asc<FSScheduleDet.lineNbr>>>>
        {
            public ScheduleDetOrdered(PXGraph graph) : base(graph) { }

            public ScheduleDetOrdered(PXGraph graph, Delegate handler) : base(graph, handler) { }

            public new const string PasteLineCommand = "PasteLine";
            public new const string ResetOrderCommand = "ResetOrder";

            protected override void AddActions(PXGraph graph)
            {
                AddAction(graph, PasteLineCommand, PX.Data.ActionsMessages.PasteLine, PasteLine);
                AddAction(graph, ResetOrderCommand, PX.Data.ActionsMessages.ResetOrder, ResetOrder);
            }
        }

        [PXFilterable]
        [PXImport(typeof(FSSchedule))]
        public ScheduleDetOrdered ScheduleDetails;
        #endregion

        [PXCopyPasteHiddenView]
        public PXSelect<FSSalesPrice,
               Where<
                   FSSalesPrice.serviceContractID, Equal<Required<FSSalesPrice.serviceContractID>>,
               And<
                   FSSalesPrice.inventoryID, Equal<Required<FSSalesPrice.inventoryID>>>>> SalesPriceLines;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<FSScheduleDet,
               InnerJoin<FSSchedule,
               On<
                   FSSchedule.scheduleID, Equal<FSScheduleDet.scheduleID>>>,
               Where<
                   FSSchedule.entityID, Equal<Required<FSSchedule.entityID>>,
                   And<FSScheduleDet.inventoryID, Equal<Required<FSScheduleDet.inventoryID>>,
                   And<FSScheduleDet.scheduleDetID, NotEqual<Required<FSScheduleDet.scheduleDetID>>>>>> ScheduleDetByContract;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<FSServiceTemplateDet,
               InnerJoin<FSScheduleDet,
               On<
                   FSScheduleDet.serviceTemplateID, Equal<FSServiceTemplateDet.serviceTemplateID>>,
               InnerJoin<FSSchedule,
               On<
                   FSSchedule.scheduleID, Equal<FSScheduleDet.scheduleID>>>>,
               Where<
                   FSSchedule.entityID, Equal<Required<FSSchedule.entityID>>,
                   And<FSServiceTemplateDet.inventoryID, Equal<Required<FSServiceTemplateDet.inventoryID>>,
                   And<FSScheduleDet.scheduleDetID, NotEqual<Required<FSScheduleDet.scheduleDetID>>>>>> ScheduleDetServicesInServiceTemplatesByContract;

        [PXCopyPasteHiddenView]
        [PXVirtualDAC]
        public PXSelectReadonly<ScheduleProjection> ScheduleProjectionRecords;

        [PXCopyPasteHiddenView]
        public PXSetup<FSServiceContract>.Where<
               Where<
                   FSServiceContract.serviceContractID, Equal<Current<TEntityID>>>> ContractSelected;

        [PXCopyPasteHiddenView]
        public PXSelect<FSContractAction,
               Where<
                   FSContractAction.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>>> ContractHistory;

        [PXViewName(CR.Messages.Answers)]
        public FSAttributeList<FSSchedule> Answers;

        public PXFilter<FSSiteStatusFilter> sitestatusfilter;
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public FSSiteStatusLookup<FSSiteStatusSelected, FSSiteStatusFilter> sitestatus;

		[PXCopyPasteHiddenView]
		public PXSelect<FSAppointmentDet,
			   Where<FSAppointmentDet.scheduleID, Equal<Current<FSSchedule.scheduleID>>,
				   And<FSAppointmentDet.scheduleDetID, Equal<Required<FSAppointmentDet.scheduleDetID>>>
			>> CurrentScheduleAppointmentDet;

        #endregion

        #region Actions
        #region AddInvBySite
        public PXAction<TPrimary> addInvBySite;
        [PXUIField(DisplayName = "Add Items", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable AddInvBySite(PXAdapter adapter)
        {
            sitestatusfilter.Cache.Clear();
            if (sitestatus.AskExt() == WebDialogResult.OK)
            {
                return AddInvSelBySite(adapter);
            }
            sitestatusfilter.Cache.Clear();
            sitestatus.Cache.Clear();
            return adapter.Get();
        }
        #endregion

        #region AddInvSelBySite
        public PXAction<TPrimary> addInvSelBySite;
        [PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable AddInvSelBySite(PXAdapter adapter)
        {
            ScheduleDetails.Cache.ForceExceptionHandling = true;

            foreach (FSSiteStatusSelected line in sitestatus.Cache.Cached)
            {
                if (line.Selected == true
                    && (line.QtySelected > 0 || line.DurationSelected > 0))
                {
                    InventoryItem inventoryItem =
                        PXSelect<InventoryItem,
                        Where<
                            InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                        .Select(this, line.InventoryID);

                    FSScheduleDet newline = PXCache<FSScheduleDet>.CreateCopy(ScheduleDetails.Insert(new FSScheduleDet()));
                    if (inventoryItem.StkItem == true)
                    {
                        newline.LineType = ID.LineType_ALL.INVENTORY_ITEM;
                    }
                    else
                    {
                        newline.LineType = inventoryItem.ItemType == INItemTypes.ServiceItem ? ID.LineType_ALL.SERVICE : ID.LineType_ALL.NONSTOCKITEM;
                    }

                    newline.InventoryID = line.InventoryID;
                    newline = PXCache<FSScheduleDet>.CreateCopy(ScheduleDetails.Update(newline));

                    if (line.BillingRule == ID.BillingRule.TIME)
                    {
                        newline.EstimatedDuration = line.DurationSelected;
                    }
                    else
                    {
                        newline.Qty = line.QtySelected;
                    }

                    ScheduleDetails.Update(newline);
                }
            }

            sitestatus.Cache.Clear();
            return adapter.Get();
        }
        #endregion
        #endregion

        #region Private Members
        private PXGraph tempGraph;

        private PXGraph TempGraph
        {
            get
            {
                if (this.tempGraph == null)
                {
                    this.tempGraph = new PXGraph();
                }

                return this.tempGraph;
            }
        }
        #endregion

        #region Public Functions

        public void FSSchedule_Row_Deleted_PartialHandler(PXCache cache, PXRowDeletedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            //Detaching ServiceOrders and Appointments created by schedule generation linked to this Schedule.
            PXUpdate<
                Set<FSServiceOrder.scheduleID, Required<FSServiceOrder.scheduleID>>,
            FSServiceOrder,
            Where<
                FSServiceOrder.scheduleID, Equal<Required<FSServiceOrder.scheduleID>>>>
            .Update(this, null, fsScheduleRow.ScheduleID);

            PXUpdate<
                Set<FSAppointment.scheduleID, Required<FSAppointment.scheduleID>>,
            FSAppointment,
            Where<
                FSAppointment.scheduleID, Equal<Required<FSAppointment.scheduleID>>>>
            .Update(this, null, fsScheduleRow.ScheduleID);
        }

        public virtual IEnumerable<ScheduleProjection> Delegate_ScheduleProjectionRecords(PXCache cache, FSSchedule fsScheduleRow, FromToFilter filter, string recordType)
        {
            FSSchedule fsRouteContractScheduleRow = fsScheduleRow;
            
            DateTime? dateBegin = filter.DateBegin;
            DateTime? dateEnd = null;

            if (filter.DateEnd.HasValue)
            {
                dateEnd = filter.DateEnd;
            }

            if (dateBegin.HasValue == true && dateEnd.HasValue == false)
            {
                dateEnd = filter.DateEnd.HasValue ? filter.DateEnd : dateBegin.Value.AddYears(1);
            }

            if (dateBegin != null && dateEnd != null)
            {
                var period = new Period((DateTime)dateBegin, (DateTime)dateEnd);

                List<Scheduler.Schedule> mapScheduleResults = new List<Scheduler.Schedule>();
                var generator = new TimeSlotGenerator();

                mapScheduleResults = MapFSScheduleToSchedule.convertFSScheduleToSchedule(cache, fsScheduleRow, dateEnd, recordType);

                if (recordType == ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
                {
                    foreach (Scheduler.Schedule schedule in mapScheduleResults)
                    {
                        schedule.Priority = (int?)RouteScheduleProcess.SetSchedulePriority(schedule, this);
                        schedule.RouteInfoList = RouteScheduleProcess.getRouteListFromSchedule(schedule, this);
                    }
                }

                List<TimeSlot> timeSlots = generator.GenerateCalendar(period, mapScheduleResults);

                foreach (TimeSlot timeSlot in timeSlots)
                {
                    ScheduleProjection scheduleProjection = new ScheduleProjection();
                    scheduleProjection.Date = timeSlot.DateTimeBegin;
                    scheduleProjection.BeginDateOfWeek = SharedFunctions.StartOfWeek((DateTime)scheduleProjection.Date, DayOfWeek.Monday);
                    ScheduleProjectionRecords.Cache.SetStatus(scheduleProjection, PXEntryStatus.Held);
                    yield return scheduleProjection;
                }
            }

            ScheduleProjectionRecords.Cache.IsDirty = false;
        }

        public string GetLineType(InventoryItem inventoryItemRow, string lineTypeFromSchedule)
        {
            if (lineTypeFromSchedule == ID.LineType_ServiceContract.SERVICE_TEMPLATE)
            {
                if (inventoryItemRow.StkItem == false)
                {
                    if (inventoryItemRow.ItemType == INItemTypes.ServiceItem)
                    {
                        return ID.LineType_ServiceContract.SERVICE;
                    }
                    else
                    {
                        return ID.LineType_ServiceContract.NONSTOCKITEM;
                    }
                }
                else
                {
                    return ID.LineType_ServiceContract.INVENTORY_ITEM;
                }
            }

            return lineTypeFromSchedule;
        }

        public void InsertDeleteFSSalesPrice(PXCache cache, int? serviceContractID, PXDBOperation operation)
        {
            TempGraph.Clear(PXClearOption.ClearQueriesOnly);
            Company companyRow = PXSelect<Company>.Select(cache.Graph);

            if (operation == PXDBOperation.Insert || operation == PXDBOperation.Update)
            {
                FSServiceContract fsServiceContractRow = FSServiceContract.PK.Find(TempGraph, serviceContractID);

                if (fsServiceContractRow != null)
                {
                    //Check missing FSSalesPrice records
                    var list = PXSelectJoin<FSSchedule,
                                            LeftJoin<FSScheduleDet,
                                                On<FSScheduleDet.scheduleID, Equal<FSSchedule.scheduleID>>,
                                            LeftJoin<FSServiceTemplateDet,
                                                On<FSServiceTemplateDet.serviceTemplateID, Equal<FSScheduleDet.serviceTemplateID>>,
                                            LeftJoin<FSSalesPrice,
                                                On<FSSalesPrice.serviceContractID, Equal<FSSchedule.entityID>,
                                                        And<
                                                            Where2<
                                                                 Where<FSSalesPrice.inventoryID, Equal<FSScheduleDet.inventoryID>,
                                                                    And<FSScheduleDet.inventoryID, IsNotNull>>,
                                                                 Or<Where<FSSalesPrice.inventoryID, Equal<FSServiceTemplateDet.inventoryID>,
                                                                    And<FSScheduleDet.serviceTemplateID, IsNotNull>>>>>>>>>,
                                            Where<FSSalesPrice.salesPriceID, IsNull,
                                                And2<
                                                     Where<FSScheduleDet.inventoryID, IsNotNull,
                                                        Or<FSScheduleDet.serviceTemplateID, IsNotNull>>,
                                                And<FSSchedule.entityID, Equal<Required<FSSchedule.entityID>>>>>>
                                            .Select(TempGraph, fsServiceContractRow.ServiceContractID);

                    bool runPersist = false;

                    foreach (PXResult<FSSchedule, FSScheduleDet, FSServiceTemplateDet, FSSalesPrice> result in list)
                    {
                        FSSchedule fsScheduleRow = (FSSchedule)result;
                        FSScheduleDet fsScheduleDetRow = (FSScheduleDet)result;
                        FSServiceTemplateDet fsServiceTemplateRow = (FSServiceTemplateDet)result;

                        int? inventoryID = fsScheduleDetRow.LineType == ID.LineType_ALL.SERVICE_TEMPLATE ? fsServiceTemplateRow.InventoryID : fsScheduleDetRow.InventoryID;
                        InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(TempGraph, inventoryID);

                        if (inventoryItemRow != null)
                        {
                            FSSalesPrice fsSalesPriceRow = new FSSalesPrice();
                            fsSalesPriceRow.ServiceContractID = fsServiceContractRow.ServiceContractID;
                            fsSalesPriceRow.InventoryID = inventoryItemRow.InventoryID;
                            fsSalesPriceRow.LineType = GetLineType(inventoryItemRow, fsScheduleDetRow.LineType);
                            fsSalesPriceRow.CuryID = companyRow?.BaseCuryID;
                            fsSalesPriceRow.UOM = inventoryItemRow.BaseUnit;

                            if (fsServiceContractRow.SourcePrice == ID.SourcePrice.CONTRACT)
                            {
                                SalesPriceSet salesPriceSet = FSPriceManagement.CalculateSalesPriceWithCustomerContract(
                                                                                    cache,
                                                                                    null,
                                                                                    null,
                                                                                    null,
                                                                                    fsServiceContractRow.CustomerID,
                                                                                    fsServiceContractRow.CustomerLocationID,
                                                                                    null,
                                                                                    fsSalesPriceRow.InventoryID,
                                                                                    null,
                                                                                    0m,
                                                                                    fsSalesPriceRow.UOM,
                                                                                    (DateTime)(fsServiceContractRow.StartDate ?? cache.Graph.Accessinfo.BusinessDate),
                                                                                    fsSalesPriceRow.UnitPrice,
                                                                                    alwaysFromBaseCurrency: true,
                                                                                    currencyInfo: null,
                                                                                    catchSalesPriceException: true);

                                fsSalesPriceRow.UnitPrice = salesPriceSet.Price;
                            }

                            runPersist = true;
                            salesPriceRecords.Cache.Insert(fsSalesPriceRow);
                        }
                    }

                    if (runPersist == true)
                    {
                        salesPriceRecords.Cache.Persist(PXDBOperation.Insert);
                    }
                }
            }

            if (operation == PXDBOperation.Delete || operation == PXDBOperation.Update)
            {
                //Check leftovers FSSalesPrice records
                var existingInventoryList = PXSelectJoin<FSServiceContract,
                                        LeftJoin<FSSchedule,
                                            On<FSSchedule.entityID, Equal<FSServiceContract.serviceContractID>>,
                                        LeftJoin<FSScheduleDet,
                                            On<FSScheduleDet.scheduleID, Equal<FSSchedule.scheduleID>>,
                                        LeftJoin<FSServiceTemplateDet,
                                            On<FSServiceTemplateDet.serviceTemplateID, Equal<FSScheduleDet.serviceTemplateID>>>>>,
                                        Where<FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                                        .Select(TempGraph, serviceContractID);

                var existingSalesPrice = PXSelect<FSSalesPrice,
                                                Where<FSSalesPrice.serviceContractID, Equal<Required<FSSalesPrice.serviceContractID>>>>
                                        .Select(TempGraph, serviceContractID).RowCast<FSSalesPrice>().ToList();

                bool runPersist = false;

                foreach (PXResult<FSServiceContract, FSSchedule, FSScheduleDet, FSServiceTemplateDet> existingInventory in existingInventoryList)
                {
                    FSSchedule fsScheduleRow = (FSSchedule)existingInventory;
                    FSScheduleDet fsSCheduleDetRow = (FSScheduleDet)existingInventory;
                    FSServiceTemplateDet fsServiceTemplateDetRow = (FSServiceTemplateDet)existingInventory;
                    int? inventoryID = fsSCheduleDetRow.LineType == ID.LineType_ALL.SERVICE_TEMPLATE ? fsServiceTemplateDetRow.InventoryID : fsSCheduleDetRow.InventoryID;

                    if (inventoryID != null)
                    {
                        FSSalesPrice existingSalesPriceRow = existingSalesPrice.Where(_ => ((FSSalesPrice)_).InventoryID == inventoryID).FirstOrDefault();
                        existingSalesPrice.Remove(existingSalesPriceRow);
                    }
                }

                foreach (FSSalesPrice fsSalesPriceRow in existingSalesPrice)
                {
                    runPersist = true;
                    salesPriceRecords.Cache.Delete(fsSalesPriceRow);
                }

                if (runPersist == true)
                {
                    salesPriceRecords.Cache.Persist(PXDBOperation.Delete);
                }
            }
        }

        /// <summary>
        /// Shows/Hides Season settings depending on the setup's flag EnableSeasonScheduleContract and on the frequencyType selected.
        /// </summary>
        public void ShowHideSeasonSetting(PXCache cache, FSSchedule fsScheduleRow)
        {
            bool showSeasonSettings = ShowSeasonSettings(fsScheduleRow);

            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnJan>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnFeb>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnMar>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnApr>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnMay>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnJun>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnJul>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnAug>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnSep>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnOct>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnNov>(cache, fsScheduleRow, showSeasonSettings);
            PXUIFieldAttribute.SetVisible<FSSchedule.seasonOnDec>(cache, fsScheduleRow, showSeasonSettings);
        }

        private bool ShowSeasonSettings(FSSchedule fsScheduleRow)
        {
            bool showSeasonSettings = SharedFunctions.GetEnableSeasonSetting(this, fsScheduleRow, Setup.Current);
            
            showSeasonSettings = showSeasonSettings && fsScheduleRow.FrequencyType != ID.Schedule_FrequencyType.ANNUAL;

            return showSeasonSettings;
        }

        /// <summary>
        /// Manage common actions for FSRouteContractSchedule and FSContractSchedule in RowSelected event.
        /// </summary>
        public void ContractSchedule_RowSelected_PartialHandler(PXCache cache, FSSchedule fsScheduleRow)
        {
            SetControlsState(cache, fsScheduleRow);

            PXUIFieldAttribute.SetEnabled<FSSchedule.runLimit>(cache, fsScheduleRow, fsScheduleRow.NoRunLimit == false);
            PXDefaultAttribute.SetPersistingCheck<FSSchedule.runLimit>(cache, fsScheduleRow, (fsScheduleRow.NoRunLimit == true) ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

            PXUIFieldAttribute.SetEnabled<FSSchedule.restrictionMinTime>(cache, fsScheduleRow, fsScheduleRow.RestrictionMin == true);
            PXDefaultAttribute.SetPersistingCheck<FSSchedule.restrictionMinTime>(cache, fsScheduleRow, (fsScheduleRow.RestrictionMin == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetEnabled<FSSchedule.restrictionMaxTime>(cache, fsScheduleRow, fsScheduleRow.RestrictionMax == true);
            PXDefaultAttribute.SetPersistingCheck<FSSchedule.restrictionMaxTime>(cache, fsScheduleRow, (fsScheduleRow.RestrictionMax == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthly3Selected>(cache, fsScheduleRow, fsScheduleRow.Monthly2Selected == true);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthly4Selected>(cache, fsScheduleRow, fsScheduleRow.Monthly3Selected == true);

            fsScheduleRow.SrvOrdTypeMessage = TX.Messages.SERVICE_ORDER_TYPE_USED_FOR_RECURRING_APPOINTMENTS;

            ShowHideSeasonSetting(cache, fsScheduleRow);
        }

        /// <summary>
        /// Manage common actions for FSRouteContractSchedule and FSContractSchedule in RowPersisting event.
        /// </summary>
        public void ContractSchedule_RowPersisting_PartialHandler(PXCache cache, FSServiceContract fsServiceContractRow, FSSchedule fsScheduleRow, PXDBOperation operation, string moduleName)
        {
            if (fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.WEEKLY
                    && !HasSelectedAtLeastOneDay(fsScheduleRow))
            {
                cache.RaiseExceptionHandling<FSRouteContractSchedule.weeklyFrequency>(fsScheduleRow,
                                                                                      fsScheduleRow.AnnualFrequency,
                                                                                      new PXSetPropertyException(TX.Error.SELECT_AT_LEAST_ONE_DAY_OF_WEEK));

                return;
            }

            if (fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.ANNUAL
                    && !HasSelectedAtLeastOneMonth(fsScheduleRow))
            {
                cache.RaiseExceptionHandling<FSSchedule.annualFrequency>(fsScheduleRow,
                                                                         fsScheduleRow.AnnualFrequency,
                                                                         new PXSetPropertyException(TX.Error.SELECT_AT_LEAST_ONE_MONTH));

                return;
            }

            if ((fsScheduleRow.NoRunLimit == null || !(bool)fsScheduleRow.NoRunLimit) && fsScheduleRow.RunLimit < 0)
            {
                cache.RaiseExceptionHandling<FSSchedule.runLimit>(fsScheduleRow,
                                                                  fsScheduleRow.RunLimit,
                                                                  new PXSetPropertyException(TX.Error.CONTRACT_INCORRECT_EXECUTION_LIMIT));

                return;
            }

            if (fsScheduleRow.RestrictionMin == true && fsScheduleRow.RestrictionMinTime == null)
            {
                cache.RaiseExceptionHandling<FSSchedule.restrictionMinTime>(fsScheduleRow,
                                                                            fsScheduleRow.RestrictionMinTime,
                                                                            new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(PX.Data.ErrorMessages.FieldIsEmpty,
                                                                                                                                         PXUIFieldAttribute.GetDisplayName<FSSchedule.restrictionMinTime>(cache))));

                return;
            }

            if (fsScheduleRow.RestrictionMax == true && fsScheduleRow.RestrictionMaxTime == null)
            {
                cache.RaiseExceptionHandling<FSSchedule.restrictionMaxTime>(fsScheduleRow,
                                                                            fsScheduleRow.RestrictionMaxTime,
                                                                            new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(PX.Data.ErrorMessages.FieldIsEmpty,
                                                                                                                                         PXUIFieldAttribute.GetDisplayName<FSSchedule.restrictionMaxTime>(cache))));

                return;
            }

            if (fsScheduleRow.StartDate > fsScheduleRow.EndDate)
            {
                cache.RaiseExceptionHandling<FSSchedule.startDate>(fsScheduleRow,
                                                                   fsScheduleRow.StartDate,
                                                                   new PXSetPropertyException(TX.Error.END_DATE_LESSER_THAN_START_DATE));

                cache.RaiseExceptionHandling<FSSchedule.endDate>(fsScheduleRow,
                                                                 fsScheduleRow.EndDate,
                                                                 new PXSetPropertyException(TX.Error.END_DATE_LESSER_THAN_START_DATE));

                return;
            }

            if (fsServiceContractRow != null 
                    && fsServiceContractRow.StartDate > fsScheduleRow.StartDate)
            {
                cache.RaiseExceptionHandling<FSSchedule.startDate>(fsScheduleRow,
                                                                   fsScheduleRow.StartDate,
                                                                   new PXSetPropertyException(TX.Error.SCHEDULE_DATE_LESSER_THAN_CONTRACT_DATE));
            }

            if (fsServiceContractRow != null
                    && fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.EXPIRING
                        && fsServiceContractRow.EndDate != null
                            && fsServiceContractRow.EndDate < fsScheduleRow.StartDate)
            {
                cache.RaiseExceptionHandling<FSSchedule.startDate>(fsScheduleRow,
                                                                   fsScheduleRow.StartDate,
                                                                   new PXSetPropertyException(TX.Error.SCHEDULE_START_DATE_GREATER_THAN_CONTRACT_END_DATE));
            }

            if (fsServiceContractRow != null 
                    && fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.EXPIRING 
                        && fsServiceContractRow.EndDate != null  
                            && (fsServiceContractRow.EndDate == null 
                                ||fsServiceContractRow.EndDate < fsScheduleRow.EndDate))
            {
                cache.RaiseExceptionHandling<FSSchedule.endDate>(fsScheduleRow,
                                                                 fsScheduleRow.EndDate,
                                                                 new PXSetPropertyException(TX.Error.SCHEDULE_END_DATE_GREATER_THAN_CONTRACT_END_DATE));
            }

            if (fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.DAILY && fsScheduleRow.DailyFrequency <= 0)
            {
                cache.RaiseExceptionHandling<FSSchedule.dailyFrequency>(fsScheduleRow,
                                                                        fsScheduleRow.DailyFrequency,
                                                                        new PXSetPropertyException(TX.Error.CONTRACT_INCORRECT_DAILY_FREQUENCY));

                return;
            }

            if (fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.WEEKLY && fsScheduleRow.WeeklyFrequency <= 0)
            {
                cache.RaiseExceptionHandling<FSSchedule.weeklyFrequency>(fsScheduleRow,
                                                                         fsScheduleRow.WeeklyFrequency,
                                                                         new PXSetPropertyException(TX.Error.CONTRACT_INCORRECT_WEEKLY_FREQUENCY));

                return;
            }

            if (operation == PXDBOperation.Insert || operation == PXDBOperation.Update)
            {
                fsScheduleRow.NextExecutionDate = SharedFunctions.GetNextExecution(cache, fsScheduleRow);
            }

            if (operation == PXDBOperation.Insert)
            {
                if (fsScheduleRow.ScheduleID > 0)
                {
                    //ContractScheduleRecords.Ask(moduleName,
                    //                            TX.Warning.SCHEDULE_WILL_NOT_AFFECT_SYSTEM_UNTIL_GENERATION_OCCURS,
                    //                            MessageButtons.OK,
                    //                            MessageIcon.Warning);
                }
            }
        }

        public void InsertContractAction(FSSchedule fsScheduleRow, PXDBOperation operation)
        {
            if (operation == PXDBOperation.Update && this.statusChanged == false)
            {
                return;
            }

			if (operation != PXDBOperation.Insert
				&& operation != PXDBOperation.Update
				&& operation != PXDBOperation.Delete)
			{
				return;
			}

			FSContractAction fsContractActionRow = new FSContractAction();

            fsContractActionRow.Type = ID.RecordType_ContractAction.SCHEDULE;
            fsContractActionRow.ServiceContractID = fsScheduleRow.EntityID;
            fsContractActionRow.ScheduleRefNbr = fsScheduleRow.RefNbr;
            fsContractActionRow.ActionBusinessDate = Accessinfo.BusinessDate;
            fsContractActionRow.ScheduleRecurrenceDescr = fsScheduleRow.RecurrenceDescription;

			fsContractActionRow = (FSContractAction)ContractHistory.Cache.Insert(fsContractActionRow);

			if (operation == PXDBOperation.Insert)
            {
				if (IsCopyContract != true && fsScheduleRow.OrigScheduleRefNbr == null)
				{
					fsContractActionRow.Action = ID.Action_ContractAction.CREATE;
				}
				else
				{
					fsContractActionRow.Action = ID.Action_ContractAction.Copied;
					fsContractActionRow.OrigScheduleRefNbr = fsScheduleRow.OrigScheduleRefNbr;
					fsContractActionRow.OrigServiceContractRefNbr = fsScheduleRow.OrigServiceContractRefNbr;
					fsScheduleRow.OrigServiceContractRefNbr = null;
					fsScheduleRow.OrigScheduleRefNbr = null;
				}
            }
            else if (operation == PXDBOperation.Update)
            {
				fsContractActionRow.Action = fsScheduleRow.Active == true ? ID.Action_ContractAction.ACTIVATE : ID.Action_ContractAction.INACTIVATE_SCHEDULE;
            }
            else if (operation == PXDBOperation.Delete)
            {
				fsContractActionRow.Action = ID.Action_ContractAction.DELETE_SCHEDULE;
            }

			fsContractActionRow = (FSContractAction)ContractHistory.Cache.Update(fsContractActionRow);
			ContractHistory.Cache.Persist(PXDBOperation.Insert);
        }
        #endregion

        #region Public Static Functions

        /// <summary>
        /// Makes visible the group that corresponds to the selected FrequencyType.
        /// </summary>
        public static void SetControlsState(PXCache cache, FSSchedule fsScheduleRow)
        {
            bool isWeekly           = fsScheduleRow.FrequencyType     == ID.Schedule_FrequencyType.WEEKLY;
            bool isDaily            = fsScheduleRow.FrequencyType     == ID.Schedule_FrequencyType.DAILY;
            bool isMonthly          = fsScheduleRow.FrequencyType     == ID.Schedule_FrequencyType.MONTHLY;
            bool isAnnually         = fsScheduleRow.FrequencyType     == ID.Schedule_FrequencyType.ANNUAL;
            bool isMonthly2Selected = fsScheduleRow.Monthly2Selected  == true;
            bool isMonthly3Selected = fsScheduleRow.Monthly3Selected  == true;
            bool isMonthly4Selected = fsScheduleRow.Monthly4Selected  == true;

            //Daily Frequency
            PXUIFieldAttribute.SetVisible<FSSchedule.dailyFrequency>(cache, fsScheduleRow, isDaily);
            PXUIFieldAttribute.SetVisible<FSSchedule.dailyLabel>(cache, fsScheduleRow, isDaily);

            //Weekly Frequency
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyFrequency>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnSun>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnMon>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnTue>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnWed>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnThu>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnFri>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyOnSat>(cache, fsScheduleRow, isWeekly);
            PXUIFieldAttribute.SetVisible<FSSchedule.weeklyLabel>(cache, fsScheduleRow, isWeekly);

            ////Monthly Frequency
            SetMonthlyControlsState(cache, fsScheduleRow);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyFrequency>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyLabel>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthly2Selected>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthly3Selected>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthly4Selected>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyRecurrenceType1>(cache, fsScheduleRow, isMonthly);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyRecurrenceType2>(cache, fsScheduleRow, isMonthly && isMonthly2Selected);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyRecurrenceType3>(cache, fsScheduleRow, isMonthly && isMonthly3Selected);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyRecurrenceType4>(cache, fsScheduleRow, isMonthly && isMonthly4Selected);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDay1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDay2>(cache, fsScheduleRow, isMonthly && isMonthly2Selected && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDay3>(cache, fsScheduleRow, isMonthly && isMonthly3Selected && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDay4>(cache, fsScheduleRow, isMonthly && isMonthly4Selected && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnWeek1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnWeek2>(cache, fsScheduleRow, isMonthly && isMonthly2Selected && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnWeek3>(cache, fsScheduleRow, isMonthly && isMonthly3Selected && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnWeek4>(cache, fsScheduleRow, isMonthly && isMonthly4Selected && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDayOfWeek1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDayOfWeek2>(cache, fsScheduleRow, isMonthly && isMonthly2Selected && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDayOfWeek3>(cache, fsScheduleRow, isMonthly && isMonthly3Selected && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.monthlyOnDayOfWeek4>(cache, fsScheduleRow, isMonthly && isMonthly4Selected && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.WEEKLY);

            ////Annual Frequency
            PXUIFieldAttribute.SetVisible<FSSchedule.annualFrequency>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.yearlyLabel>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualRecurrenceType>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnJan>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnFeb>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnMar>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnApr>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnMay>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnJun>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnJul>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnAug>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnSep>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnOct>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnNov>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnDec>(cache, fsScheduleRow, isAnnually);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnDay>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnWeek>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetVisible<FSSchedule.annualOnDayOfWeek>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.annualOnDay>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.annualOnWeek>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.annualOnDayOfWeek>(cache, fsScheduleRow, isAnnually && fsScheduleRow.AnnualRecurrenceType == ID.Schedule_FrequencyType.WEEKLY);

            //?????
            //PXUIFieldAttribute.SetEnabled<FSSchedule.runLimit>(cache, fsScheduleRow, !(bool)fsScheduleRow.NoRunLimit);ffffff
        }

        /// <summary>
        /// Set exceptions in date fields.
        /// </summary>
        public static void SetDateFieldExceptions(PXCache cache, FSSchedule fsScheduleRow, Exception exception)
        {
            cache.RaiseExceptionHandling<FSRouteContractSchedule.startDate>(fsScheduleRow,
                                                                            fsScheduleRow.StartDate,
                                                                            exception);

            cache.RaiseExceptionHandling<FSRouteContractSchedule.endDate>(fsScheduleRow,
                                                                          fsScheduleRow.EndDate,
                                                                          exception);
        }

        /// <summary>
        /// Check if at least one month is selected.
        /// </summary>
        /// <param name="fsScheduleRow">Instance of the FSSchedule DAC.</param>
        /// <returns>True if at least one month is selected.</returns>
        public static bool HasSelectedAtLeastOneMonth(FSSchedule fsScheduleRow)
        {
            return fsScheduleRow.AnnualOnJan == true || fsScheduleRow.AnnualOnFeb == true || fsScheduleRow.AnnualOnMar == true
                        || fsScheduleRow.AnnualOnApr == true || fsScheduleRow.AnnualOnMay == true || fsScheduleRow.AnnualOnJun == true
                            || fsScheduleRow.AnnualOnJul == true || fsScheduleRow.AnnualOnAug == true || fsScheduleRow.AnnualOnSep == true
                                || fsScheduleRow.AnnualOnOct == true || fsScheduleRow.AnnualOnNov == true || fsScheduleRow.AnnualOnDec == true;
        }

        /// <summary>
        /// Check if at least one day of the week is selected.
        /// </summary>
        /// <param name="fsScheduleRow">Instance of the FSSchedule DAC.</param>
        /// <returns>True if at least one day is selected.</returns>
        public static bool HasSelectedAtLeastOneDay(FSSchedule fsScheduleRow)
        {
            return fsScheduleRow.WeeklyOnSun == true || fsScheduleRow.WeeklyOnMon == true || fsScheduleRow.WeeklyOnTue == true
                        || fsScheduleRow.WeeklyOnWed == true || fsScheduleRow.WeeklyOnThu == true || fsScheduleRow.WeeklyOnFri == true
                            || fsScheduleRow.WeeklyOnSat == true;
        }

        public static void SetMonthlyControlsState(PXCache cache, FSSchedule fsScheduleRow)
        {
            bool isMonthly = fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.MONTHLY;

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDay1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnWeek1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDayOfWeek1>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType1 == ID.Schedule_FrequencyType.WEEKLY);

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDay2>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnWeek2>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDayOfWeek2>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType2 == ID.Schedule_FrequencyType.WEEKLY);

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDay3>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnWeek3>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDayOfWeek3>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType3 == ID.Schedule_FrequencyType.WEEKLY);

            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDay4>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.DAILY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnWeek4>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.WEEKLY);
            PXUIFieldAttribute.SetEnabled<FSSchedule.monthlyOnDayOfWeek4>(cache, fsScheduleRow, isMonthly && fsScheduleRow.MonthlyRecurrenceType4 == ID.Schedule_FrequencyType.WEEKLY);
        }

        public static void CheckLineByLineType(PXCache cache,
                                               FSScheduleDet fsScheduleDetRow,
                                               PXErrorLevel errorLevel = PXErrorLevel.Error)
        {
            switch (fsScheduleDetRow.LineType)
            {
                case ID.LineType_ServiceContract.SERVICE_TEMPLATE:

                    if (fsScheduleDetRow.InventoryID != null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, true);
                        
                        cache.RaiseExceptionHandling<FSScheduleDet.inventoryID>(fsScheduleDetRow,
                                                                                null,
                                                                                new PXSetPropertyException(TX.Error.FIELD_MUST_BE_EMPTY_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.SMEquipmentID == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.SMequipmentID>(cache, fsScheduleDetRow, true);
                    }

                    if (fsScheduleDetRow.ServiceTemplateID == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, true);

                        cache.RaiseExceptionHandling<FSScheduleDet.serviceTemplateID>(fsScheduleDetRow,
                                                                                      null,
                                                                                      new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.TranDesc == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.tranDesc>(fsScheduleDetRow,
                                                                             null,
                                                                             new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    break;
                    
                case ID.LineType_ServiceContract.SERVICE:
                case ID.LineType_ServiceContract.NONSTOCKITEM:

                    if (fsScheduleDetRow.InventoryID == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.inventoryID>(fsScheduleDetRow,
                                                                                null,
                                                                                new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.Qty == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.qty>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.qty>(fsScheduleDetRow,
                                                                        null,
                                                                        new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.SMEquipmentID == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.SMequipmentID>(cache, fsScheduleDetRow, true);
                    }

                    if (fsScheduleDetRow.ServiceTemplateID != null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.serviceTemplateID>(fsScheduleDetRow,
                                                                                      null,
                                                                                      new PXSetPropertyException(TX.Error.FIELD_MUST_BE_EMPTY_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.TranDesc == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.tranDesc>(fsScheduleDetRow,
                                                                             null,
                                                                             new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    break;

                case ID.LineType_ServiceContract.INVENTORY_ITEM:

                    if (fsScheduleDetRow.InventoryID == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.inventoryID>(fsScheduleDetRow,
                                                                                null,
                                                                                new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.Qty == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.qty>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.qty>(fsScheduleDetRow,
                                                                        null,
                                                                        new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.TranDesc == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.tranDesc>(fsScheduleDetRow,
                                                                             null,
                                                                             new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    break;

                case ID.LineType_ServiceContract.COMMENT:
                case ID.LineType_ServiceContract.INSTRUCTION:

                    if (fsScheduleDetRow.InventoryID != null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.inventoryID>(fsScheduleDetRow,
                                                                                null,
                                                                                new PXSetPropertyException(TX.Error.FIELD_MUST_BE_EMPTY_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.ServiceTemplateID != null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.serviceTemplateID>(fsScheduleDetRow,
                                                                                      null,
                                                                                      new PXSetPropertyException(TX.Error.FIELD_MUST_BE_EMPTY_FOR_LINE_TYPE, errorLevel));
                    }

                    if (fsScheduleDetRow.TranDesc == null)
                    {
                        PXUIFieldAttribute.SetEnabled<FSScheduleDet.tranDesc>(cache, fsScheduleDetRow, true);
                        cache.RaiseExceptionHandling<FSScheduleDet.tranDesc>(fsScheduleDetRow,
                                                                             null,
                                                                             new PXSetPropertyException(TX.Error.DATA_REQUIRED_FOR_LINE_TYPE, errorLevel));
                    }

                    break;
            }
        }

        /// <summary>
        /// This method enables or disables the fields on the grid depending on the selected LineType.
        /// </summary>
        public virtual void EnableDisable_LineType(PXCache cache, FSScheduleDet fsScheduleDetRow)
        {
            bool isInventoryItem = (fsScheduleDetRow.LineType == ID.LineType_ServiceContract.SERVICE
                                    || fsScheduleDetRow.LineType == ID.LineType_ServiceContract.NONSTOCKITEM
                                    || fsScheduleDetRow.LineType == ID.LineType_ServiceContract.INVENTORY_ITEM);

            bool enableInventoryID = (isInventoryItem || fsScheduleDetRow.LineType == null) && fsScheduleDetRow.ServiceTemplateID == null;
            bool enableServiceTemplateID = (fsScheduleDetRow.LineType == null || fsScheduleDetRow.LineType == ID.LineType_ServiceContract.SERVICE_TEMPLATE) && fsScheduleDetRow.InventoryID == null;

            PXUIFieldAttribute.SetEnabled<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, enableInventoryID);
            PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.inventoryID>(cache, fsScheduleDetRow, isInventoryItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetEnabled<FSScheduleDet.qty>(cache, fsScheduleDetRow, isInventoryItem
                                                                                        && fsScheduleDetRow.BillingRule != ID.BillingRule.TIME
                                                                                        && fsScheduleDetRow.InventoryID != null);

            PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.qty>(cache, fsScheduleDetRow, isInventoryItem
                                                                                        && fsScheduleDetRow.BillingRule != ID.BillingRule.TIME
                                                                                        && fsScheduleDetRow.InventoryID != null ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetEnabled<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, enableServiceTemplateID);
            PXDefaultAttribute.SetPersistingCheck<FSScheduleDet.serviceTemplateID>(cache, fsScheduleDetRow, fsScheduleDetRow.LineType == ID.LineType_ServiceContract.SERVICE_TEMPLATE ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetEnabled<FSScheduleDet.billingRule>(cache, fsScheduleDetRow, fsScheduleDetRow.LineType == ID.LineType_ALL.SERVICE);
            PXUIFieldAttribute.SetEnabled<FSScheduleDet.equipmentAction>(cache, fsScheduleDetRow, fsScheduleDetRow.LineType == ID.LineType_ServiceContract.INVENTORY_ITEM);

            PXUIFieldAttribute.SetEnabled<FSScheduleDet.estimatedDuration>(cache, fsScheduleDetRow, (fsScheduleDetRow.LineType == ID.LineType_ALL.SERVICE 
                                                                                                        || fsScheduleDetRow.LineType == ID.LineType_ALL.NONSTOCKITEM)
                                                                                                        && fsScheduleDetRow.InventoryID != null);
        }

        /// <summary>
        /// This method reset the fields of the <c>fsScheduleDetRow</c> depending on the selected LineType.
        /// </summary>
        public static void ResetLineByType(FSScheduleDet fsScheduleDetRow, PXCache cache)
        {
            fsScheduleDetRow.BillingRule = ID.BillingRule.NONE;

            switch (fsScheduleDetRow.LineType)
            {
                case ID.LineType_ServiceContract.NONSTOCKITEM:
                    fsScheduleDetRow.BillingRule = ID.BillingRule.FLAT_RATE;
                    break;
                case ID.LineType_ServiceContract.SERVICE_TEMPLATE:
                    fsScheduleDetRow.SMEquipmentID = null;
                    break;
                case ID.LineType_ServiceContract.COMMENT:
                case ID.LineType_ServiceContract.INSTRUCTION:
                    fsScheduleDetRow.ProjectTaskID = null;
                    fsScheduleDetRow.CostCodeID = null;
                    break;
            }

            fsScheduleDetRow.ServiceTemplateID = null;
            fsScheduleDetRow.InventoryID = null;
        }
        #endregion

        #region Events

        #region ContractSchedule Events

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        protected virtual void _(Events.FieldDefaulting<FSSchedule, FSSchedule.active> e)
        {
            if (e.Row == null)
                return;

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            e.NewValue = fsScheduleRow.ScheduleGenType != ID.ScheduleGenType_ServiceContract.NONE ? true : false;
            e.Cancel = true;
        }

        protected virtual void _(Events.FieldDefaulting<FSSchedule, FSSchedule.restrictionMinTime> e)
        {
            if (e.Row == null)
            {
                return;
            }

            e.NewValue = Accessinfo.BusinessDate.Value.AddHours(8);
        }

        protected virtual void _(Events.FieldDefaulting<FSSchedule, FSSchedule.restrictionMaxTime> e)
        {
            if (e.Row == null)
            {
                return;
            }

            e.NewValue = Accessinfo.BusinessDate.Value.AddHours(12);
        }

        protected virtual void _(Events.FieldDefaulting<FSSchedule, FSSchedule.recurrenceDescription> e)
        {
            e.NewValue = ScheduleHelperGraph.GetRecurrenceDescription(e.Row);
        }
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSSchedule, FSSchedule.startDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            if (fsScheduleRow.EndDate != null &&
                    !SharedFunctions.IsValidDateRange(fsScheduleRow.StartDate, fsScheduleRow.EndDate))
            {
                SetDateFieldExceptions(e.Cache, fsScheduleRow, new PXSetPropertyException(TX.Error.END_DATE_LESSER_THAN_START_DATE, PXErrorLevel.Error));
            }
            else
            {
                SetDateFieldExceptions(e.Cache, fsScheduleRow, null);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSchedule, FSSchedule.endDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            if (fsScheduleRow.EndDate != null
                    && !SharedFunctions.IsValidDateRange(fsScheduleRow.StartDate, fsScheduleRow.EndDate))
            {
                SetDateFieldExceptions(e.Cache, fsScheduleRow, new PXSetPropertyException(TX.Error.END_DATE_LESSER_THAN_START_DATE, PXErrorLevel.Error));
            }
            else
            {
                SetDateFieldExceptions(e.Cache, fsScheduleRow, null);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSchedule, FSSchedule.noRunLimit> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;
            fsScheduleRow.RunLimit = (bool)fsScheduleRow.NoRunLimit ? (short)0 : (short)1;
        }

        protected virtual void _(Events.FieldUpdated<FSSchedule, FSSchedule.monthly2Selected> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            if (fsScheduleRow.Monthly2Selected == false)
            {
                fsScheduleRow.Monthly3Selected = false;
                fsScheduleRow.Monthly4Selected = false;
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSchedule, FSSchedule.monthly3Selected> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            if (fsScheduleRow.Monthly3Selected == false)
            {
                fsScheduleRow.Monthly4Selected = false;
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSchedule, FSSchedule.entityID> e)
        {
        }

        protected virtual void _(Events.FieldUpdated<FSSchedule, FSSchedule.active> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;
            this.statusChanged = fsScheduleRow.Active != (bool)e.OldValue;
        }

        protected virtual void _(Events.FieldUpdated<FSSchedule, FSSchedule.srvOrdType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            e.Cache.SetDefaultExt<FSSchedule.dfltProjectTaskID>(fsScheduleRow);
        }

		protected virtual void _(Events.FieldUpdated<FSSchedule, FSSchedule.customerID> e)
		{
			if (e.Row == null)
			{
				return;
			}

			FSSchedule fsScheduleRow = (FSSchedule)e.Row;

			e.Cache.SetDefaultExt<FSSchedule.projectID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<FSSchedule, FSSchedule.projectID> e)
		{
			if (e.Row == null)
			{
				return;
			}

			if (ScheduleDetails != null)
			{
				foreach (FSScheduleDet fsScheduleDetRow in ScheduleDetails.Select())
				{
					ScheduleDetails.Cache.SetDefaultExt<FSScheduleDet.projectID>(fsScheduleDetRow);
					if (ProjectDefaultAttribute.IsNonProject(e.Row.ProjectID)
							|| fsScheduleDetRow.CostCodeID == null)
					{
						ScheduleDetails.Cache.SetDefaultExt<FSScheduleDet.costCodeID>(fsScheduleDetRow);
					}
				}
			}

			e.Cache.SetDefaultExt<FSSchedule.dfltProjectTaskID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<FSSchedule, FSSchedule.dfltProjectTaskID> e)
		{
			if (e.Row == null)
			{
				return;
			}

			FSSchedule fsScheduleRow = (FSSchedule)e.Row;

			if (ScheduleDetails != null)
			{
				foreach (FSScheduleDet fsScheduleDet in ScheduleDetails.Select())
				{
					int? originalProjectID = (int?)ScheduleDetails.Cache.GetValueOriginal<FSScheduleDet.projectID>(fsScheduleDet);
					if (fsScheduleDet.ProjectID != originalProjectID || fsScheduleDet.ProjectTaskID == null)
					{
						PMTask task = null;
						if (fsScheduleDet.ProjectID != null && e.Row.DfltProjectTaskID != null)
						{
							task = PXSelect<PMTask,
											Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
											And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>
											.Select(this, fsScheduleDet.ProjectID, e.Row.DfltProjectTaskID);
						}

						fsScheduleDet.ProjectTaskID = task?.TaskID;
						ScheduleDetails.Update(fsScheduleDet);
					}
				}
			}
		}
		#endregion

		protected virtual void _(Events.RowSelecting<FSSchedule> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSSchedule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;
            PXCache cache = e.Cache;

            PXUIFieldAttribute.SetEnabled<FSSchedule.active>(cache, fsScheduleRow, fsScheduleRow.ScheduleGenType != ID.ScheduleGenType_ServiceContract.NONE);
            PXUIFieldAttribute.SetEnabled<FSSchedule.scheduleGenType>(cache, fsScheduleRow, false);

            PXUIFieldAttribute.SetVisible<FSScheduleDet.equipmentAction>(ScheduleDetails.Cache, null, SrvOrdTypeSelected.Current?.PostToSOSIPM == true);
            PXUIFieldAttribute.SetVisible<FSScheduleDet.SMequipmentID>(ScheduleDetails.Cache, null, SrvOrdTypeSelected.Current?.PostToSOSIPM == true);
            PXUIFieldAttribute.SetVisible<FSScheduleDet.componentID>(ScheduleDetails.Cache, null, SrvOrdTypeSelected.Current?.PostToSOSIPM == true);
            PXUIFieldAttribute.SetVisible<FSScheduleDet.equipmentLineRef>(ScheduleDetails.Cache, null, SrvOrdTypeSelected.Current?.PostToSOSIPM == true);

            SharedFunctions.SetVisibleEnableProjectField<FSSchedule.dfltProjectTaskID>(cache, fsScheduleRow, fsScheduleRow.ProjectID);
        }

        protected virtual void _(Events.RowInserting<FSSchedule> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSSchedule> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSSchedule> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSSchedule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            object recurrenceDfltValue;
            e.Cache.RaiseFieldDefaulting<FSSchedule.recurrenceDescription>(e.Row, out recurrenceDfltValue);
            e.Row.RecurrenceDescription = (string)recurrenceDfltValue;
        }

        protected virtual void _(Events.RowDeleting<FSSchedule> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSSchedule> e)
        {
            FSSchedule_Row_Deleted_PartialHandler(e.Cache, e.Args);
        }

        protected virtual void _(Events.RowPersisting<FSSchedule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = (FSSchedule)e.Row;

            if (fsScheduleRow.ScheduleGenType == ID.ScheduleGenType_ServiceContract.NONE)
            {
                throw new PXException(TX.Error.SCHEDULE_TYPE_NONE);
            }
        }

        protected virtual void _(Events.RowPersisted<FSSchedule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (e.TranStatus == PXTranStatus.Open)
            {
                if (e.Operation == PXDBOperation.Delete)
                {
                    FSSchedule fsScheduleRow = (FSSchedule)e.Row;
                    InsertDeleteFSSalesPrice(e.Cache, fsScheduleRow.EntityID, e.Operation);
                }
            }

			if (e.TranStatus == PXTranStatus.Completed)
			{
				InsertContractAction((FSSchedule)e.Row, e.Operation);
			}
        }

        #endregion

        #region FSScheduleDet Events

        #region FSScheduleDet Events

        #region FieldSelecting
        #endregion
        #region FieldDefaulting

        protected virtual void _(Events.FieldDefaulting<FSScheduleDet, FSScheduleDet.costCodeID> e)
        {
            if (e.Row == null)
            {
                return;
            }

			if (!ProjectDefaultAttribute.IsNonProject(CurrentServiceContract.Current?.ProjectID)
				&& PXAccess.FeatureInstalled<FeaturesSet.costCodes>()
				&& (e.Row.InventoryID != null || e.Row.LineType == ID.LineType_ALL.SERVICE_TEMPLATE))
			{
				e.NewValue = CurrentServiceContract.Current?.DfltCostCodeID;
				e.Cancel = true;
			}
        }

        protected virtual void _(Events.FieldDefaulting<FSScheduleDet, FSScheduleDet.qty> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDet fsScheduleDetRow = e.Row;

            if (fsScheduleDetRow.BillingRule == ID.BillingRule.TIME && fsScheduleDetRow.LineType == ID.LineType_ALL.SERVICE)
            {
                e.NewValue = PXDBQuantityAttribute.Round(decimal.Divide((decimal)(fsScheduleDetRow.EstimatedDuration ?? 0), 60));
            }
        }
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSScheduleDet, FSScheduleDet.inventoryID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDet fsScheduleDetRow = (FSScheduleDet)e.Row;
            InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(e.Cache.Graph, fsScheduleDetRow.InventoryID);

            if (inventoryItemRow != null && fsScheduleDetRow.LineType == null)
            {
                if (inventoryItemRow.StkItem == true)
                {
                    fsScheduleDetRow.LineType = ID.LineType_ALL.INVENTORY_ITEM;
                }
                else if (inventoryItemRow.ItemType == INItemTypes.ServiceItem)
                {
                    fsScheduleDetRow.LineType = ID.LineType_ALL.SERVICE;
                }
                else
                {
                    fsScheduleDetRow.LineType = ID.LineType_ALL.NONSTOCKITEM;
                }

                e.Cache.SetDefaultExt<FSScheduleDet.billingRule>(fsScheduleDetRow);
            }

            if (inventoryItemRow != null)
            {
                fsScheduleDetRow.TranDesc = inventoryItemRow.Descr;
            }

            if (fsScheduleDetRow.IsInventoryItem == true)
            {
                SharedFunctions.UpdateEquipmentFields(this, e.Cache, fsScheduleDetRow, fsScheduleDetRow.InventoryID, SrvOrdTypeSelected.Current);
            }

            e.Cache.SetDefaultExt<FSScheduleDet.estimatedDuration>(e.Row);
            e.Cache.SetDefaultExt<FSScheduleDet.qty>(e.Row);
        }

        protected virtual void _(Events.FieldUpdated<FSScheduleDet, FSScheduleDet.serviceTemplateID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDet fsScheduleDetRow = (FSScheduleDet)e.Row;
            FSServiceTemplate fsServiceTemplateRow = FSServiceTemplate.PK.Find(this, fsScheduleDetRow.ServiceTemplateID);

            if (fsServiceTemplateRow != null && fsScheduleDetRow.LineType == null)
            {
                fsScheduleDetRow.LineType = ID.LineType_ALL.SERVICE_TEMPLATE;
            }

            if (fsServiceTemplateRow != null)
            {
                fsScheduleDetRow.TranDesc = fsServiceTemplateRow.Descr;
            }

            if (fsScheduleDetRow.IsInventoryItem == true)
            {
                SharedFunctions.UpdateEquipmentFields(this, e.Cache, fsScheduleDetRow, fsScheduleDetRow.InventoryID, SrvOrdTypeSelected.Current);
            }

            e.Cache.SetDefaultExt<FSScheduleDet.qty>(e.Row);
        }

        protected virtual void _(Events.FieldUpdated<FSScheduleDet, FSScheduleDet.billingRule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDet FSScheduleDetRow = (FSScheduleDet)e.Row;

            if (FSScheduleDetRow.BillingRule == ID.BillingRule.TIME)
            {
                e.Cache.SetDefaultExt<FSScheduleDet.qty>(e.Row);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSScheduleDet, FSScheduleDet.estimatedDuration> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDet FSScheduleDetRow = (FSScheduleDet)e.Row;

            if (FSScheduleDetRow.BillingRule == ID.BillingRule.TIME)
            {
                e.Cache.SetDefaultExt<FSScheduleDet.qty>(e.Row);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSScheduleDet, FSScheduleDet.lineType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDet FSScheduleDetRow = (FSScheduleDet)e.Row;
            ResetLineByType(FSScheduleDetRow, e.Cache);
        }
        protected virtual void _(Events.FieldUpdated<FSScheduleDet, FSScheduleDet.equipmentAction> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDet FSScheduleDetRow = (FSScheduleDet)e.Row;
            if (FSScheduleDetRow.IsInventoryItem == true)
            {

                SharedFunctions.ResetEquipmentFields(e.Cache, FSScheduleDetRow);
                SharedFunctions.SetEquipmentFieldEnablePersistingCheck(e.Cache, FSScheduleDetRow);
            }
        }
        protected virtual void _(Events.FieldUpdated<FSScheduleDet, FSScheduleDet.equipmentLineRef> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDet FSScheduleDetRow = (FSScheduleDet)e.Row;

            if (FSScheduleDetRow.ComponentID == null)
            {
                FSScheduleDetRow.ComponentID = SharedFunctions.GetEquipmentComponentID(this, FSScheduleDetRow.SMEquipmentID, FSScheduleDetRow.EquipmentLineRef);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSScheduleDet, FSScheduleDet.tranDesc> e)
        {
            if (e.Row == null)
            {
                return;
            }
            FSScheduleDet fsScheduleDetRow = e.Row;

            if (!String.IsNullOrEmpty(fsScheduleDetRow.TranDesc) && fsScheduleDetRow.LineType == null)
            {
                e.Cache.SetValueExt<FSScheduleDet.lineType>(fsScheduleDetRow, ID.LineType_ALL.INSTRUCTION);
            }
        }
    #endregion

        protected virtual void _(Events.RowSelected<FSScheduleDet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDet FSScheduleDetRow = (FSScheduleDet)e.Row;
            PXCache cache = e.Cache;

            EnableDisable_LineType(cache, FSScheduleDetRow);
            SharedFunctions.SetEquipmentFieldEnablePersistingCheck(cache, FSScheduleDetRow, false);

            FSSchedule fsContractScheduleRow = ContractScheduleRecords.Current as FSSchedule;

            if (FSScheduleDetRow != null && fsContractScheduleRow != null)
            {
                SharedFunctions.SetEnableCostCodeProjectTask<FSScheduleDet.projectTaskID, FSScheduleDet.costCodeID>(cache, FSScheduleDetRow, FSScheduleDetRow.LineType, fsContractScheduleRow.ProjectID);
            }

            bool includeIN = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>()
                                && PXAccess.FeatureInstalled<FeaturesSet.inventory>()
                                    && SrvOrdTypeSelected.Current?.AllowInventoryItems == true;

            FSLineType.SetLineTypeList<FSScheduleDet.lineType>(cache, null, includeIN, true, false, true, false);

            if (SrvOrdTypeSelected.Current != null && SrvOrdTypeSelected.Current.PostTo == ID.SrvOrdType_PostTo.PROJECTS) 
            {
                PXUIFieldAttribute.SetEnabled<FSScheduleDet.equipmentAction>(cache, null, false);
            }
        }

        protected virtual void _(Events.RowInserting<FSScheduleDet> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSScheduleDet> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSScheduleDet> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSScheduleDet> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSScheduleDet> e)
        {
			if (e.Row == null)
				return;

			var appDet = CurrentScheduleAppointmentDet
							.Select<FSAppointmentDet>(e.Row.ScheduleDetID)
							.FirstOrDefault();

			if (appDet != null)
			{
			appDet.ScheduleDetID = null;

			CurrentScheduleAppointmentDet.Update(appDet);
        }
		}

        protected virtual void _(Events.RowDeleted<FSScheduleDet> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSScheduleDet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleDet FSScheduleDetRow = (FSScheduleDet)e.Row;
            CheckLineByLineType(e.Cache, FSScheduleDetRow);
        }

        protected virtual void _(Events.RowPersisted<FSScheduleDet> e)
        {
            if (e.Row == null || CurrentServiceContract.Current == null)
            {
                return;
            }

            if (e.TranStatus == PXTranStatus.Open)
            {
                InsertDeleteFSSalesPrice(e.Cache, CurrentServiceContract.Current.ServiceContractID, e.Operation);
            }
        }

        #endregion

        #endregion

        #region FSSiteStatusFilter
        protected virtual void _(Events.RowSelected<FSSiteStatusFilter> e)
        {
            FSSiteStatusFilter row = (FSSiteStatusFilter)e.Row;

            bool includeIN = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>()
                                && PXAccess.FeatureInstalled<FeaturesSet.inventory>()
                                    && SrvOrdTypeSelected.Current?.AllowInventoryItems == true;
            row.IncludeIN = includeIN;

            if (row != null && !includeIN)
            {
                row.OnlyAvailable = false;
                PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.onlyAvailable>(e.Cache, row, includeIN);
                PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.inventory_Wildcard>(e.Cache, row, includeIN);
                PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.mode>(e.Cache, row, includeIN);
                PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.barCode>(e.Cache, row, includeIN);
                PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.barCodeWildcard>(e.Cache, row, includeIN);
                PXUIFieldAttribute.SetVisible<FSSiteStatusFilter.siteID>(e.Cache, row, includeIN);
            }

            FSLineType.SetLineTypeList<FSSiteStatusFilter.lineType>(e.Cache,
                                                                  row,
                                                                  includeIN,
                                                                  false,
                                                                  false,
                                                                  false, 
                                                                  true);
        }
        #endregion

        #endregion
    }
}
