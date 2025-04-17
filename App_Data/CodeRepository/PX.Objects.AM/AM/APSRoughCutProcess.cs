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
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;
using System.Linq;
using PX.Objects.GL;

namespace PX.Objects.AM
{
    /// <summary>
    /// Advanced Planning and Scheduling Rough Cut Process
    /// </summary>
    public class APSRoughCutProcess : PXGraph<APSRoughCutProcess>
    {
        public PXCancel<AMSchdItem> Cancel;

        /// <summary>
        /// Processing records
        /// </summary>
        [PXFilterable]
		public PXProcessingJoin<AMSchdItem,
			InnerJoin<AMProdItem, On<AMSchdItem.orderType, Equal<AMProdItem.orderType>,
				And<AMSchdItem.prodOrdID, Equal<AMProdItem.prodOrdID>>>,
			InnerJoin<AMOrderType, On<AMProdItem.orderType, Equal<AMOrderType.orderType>>,
			InnerJoin<Branch, On<AMProdItem.branchID, Equal<Branch.branchID>>>>>,
			Where2<Where<AMProdItem.canceled, Equal<False>,
				And<AMProdItem.completed, Equal<False>>>,
			And2<Where<Current<APSRoughCutProcessFilter.excludePlanningOrders>, Equal<False>,
				Or<AMProdItem.function, NotEqual<OrderTypeFunction.planning>>>,
			And2<Where<Current<APSRoughCutProcessFilter.excludeFirmOrders>, Equal<False>,
				Or<AMSchdItem.scheduleStatus, NotEqual<ProductionScheduleStatus.firmed>>>,
			And<Branch.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>>>>>> OrderList;

		/// <summary>
		/// Processing page filter
		/// </summary>
		public PXFilter<APSRoughCutProcessFilter> Filter;

        public APSRoughCutProcess()
        {
            var filter = Filter.Current;
            OrderList.SetProcessDelegate(
            delegate (List<AMSchdItem> list)
            {
                ProcessSchedule(list, filter, true);
            });

            PXUIFieldAttribute.SetEnabled<AMSchdItem.schPriority>(OrderList.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMSchdItem.constDate>(OrderList.Cache, null, true);

            InquiresMenu.AddMenuAction(OrderScheduleInquiry);
            InquiresMenu.AddMenuAction(ProductionDetails);
            InquiresMenu.AddMenuAction(CriticalMatl);
            InquiresMenu.AddMenuAction(InventoryAllocationDetailInq);
			InquiresMenu.AddMenuAction(ProductionScheduleBoardRedirect);
        }

        public PXAction<AMSchdItem> InquiresMenu;
        [PXUIField(DisplayName = Messages.Inquiries)]
        [PXButton(MenuAutoOpen = true)]
        protected virtual IEnumerable inquiresMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<AMSchdItem> OrderScheduleInquiry;
        [PXUIField(DisplayName = "Order Schedule", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable orderscheduleInquiry(PXAdapter adapter)
        {
            if (OrderList.Current != null)
            {
                var gi = new GIWorkCenterSchedule();
                gi.SetParameter(GIWorkCenterSchedule.Parameters.OrderType, OrderList.Current.OrderType);
                gi.SetParameter(GIWorkCenterSchedule.Parameters.ProductionNbr, OrderList.Current.ProdOrdID);
                gi.CallGenericInquiry(PXBaseRedirectException.WindowMode.New);
            }
            return adapter.Get();
        }

        public PXAction<AMSchdItem> ProductionDetails;
        [PXUIField(DisplayName = Messages.ProductionDetail, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable productionDetails(PXAdapter adapter)
        {
            if (OrderList.Current != null)
            {
                var pd = CreateInstance<ProdDetail>();
                pd.ProdItemRecords.Current = pd.ProdItemRecords.Search<AMProdItem.prodOrdID>(OrderList.Current.ProdOrdID, OrderList.Current.OrderType);

                if (pd.ProdItemRecords.Current?.ProdOrdID != null)
                {
                    PXRedirectHelper.TryRedirect(pd, PXRedirectHelper.WindowMode.New);
                }
            }
            return adapter.Get();
        }

        public PXAction<AMSchdItem> CriticalMatl;
        [PXUIField(DisplayName = Messages.CriticalMaterial, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable criticalMatl(PXAdapter adapter)
        {
            if (OrderList.Current != null)
            {
                var cm = CreateInstance<CriticalMaterialsInq>();
                cm.ProdItemRecs.Current.OrderType = OrderList.Current.OrderType;
                cm.ProdItemRecs.Current.ProdOrdID = OrderList.Current.ProdOrdID;
                cm.ProdItemRecs.Current.ShowAll = false;
                throw new PXRedirectRequiredException(cm, Messages.CriticalMaterial);
            }
            return adapter.Get();
        }

        public PXAction<AMSchdItem> InventoryAllocationDetailInq;
        [PXUIField(DisplayName = "Allocation Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable inventoryAllocationDetailInq(PXAdapter adapter)
        {
            if (OrderList?.Current?.InventoryID != null)
            {
                var allocGraph = CreateInstance<InventoryAllocDetEnq>();
                allocGraph.Filter.Current.InventoryID = OrderList.Current.InventoryID;
                if (InventoryHelper.SubItemFeatureEnabled)
                {
                    var subItem = (INSubItem)PXSelectorAttribute.Select<AMProdItem.subItemID>(OrderList.Cache, OrderList.Current);
                    if (!string.IsNullOrWhiteSpace(subItem?.SubItemCD))
                    {
                        allocGraph.Filter.Current.SubItemCD = subItem.SubItemCD;
                    }
                }
                allocGraph.Filter.Current.SiteID = OrderList.Current.SiteID;
                allocGraph.RefreshTotal.Press();
                PXRedirectHelper.TryRedirect(allocGraph, PXRedirectHelper.WindowMode.New);
            }

            return adapter.Get();
        }

		public PXAction<AMSchdItem> ProductionScheduleBoardRedirect;
		[PXUIField(DisplayName = "Production Schedule Board", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public IEnumerable productionScheduleBoardRedirect(PXAdapter adapter)
        {
			if(OrderList.Current?.OrderType == null || Filter.Current == null)
			{
				return adapter.Get(); 
			}

			ManufacturingDiagram.RedirectTo(new ManufacturingDiagram.ManufacturingDiagramFilter
			{
				ScheduleStatus = Filter.Current.ExcludeFirmOrders.GetValueOrDefault() ?
					ProductionScheduleStatus.Scheduled :
					ManufacturingDiagram.ManufacturingDiagramFilter.ScheduleStatusFilterListAttribute.BothStatus,
				IncludeOnHold = true
			});

            return adapter.Get();
        }

		protected virtual void _(Events.FieldUpdated<APSRoughCutProcessFilter, APSRoughCutProcessFilter.processAction> e)
		{
			if(IsImport || IsContractBasedAPI)
			{
				return;
			}

			if(((string)e.NewValue) == APSRoughCutProcessActions.UndoFirm)
			{
				// If user wants to undo firm we should by default show them orders which are firm
				e.Cache.SetValueExt<APSRoughCutProcessFilter.excludeFirmOrders>(e.Row, false);
				return;
			}

			var oldValue = (string)e.OldValue;
			if(oldValue == APSRoughCutProcessActions.UndoFirm)
			{
				e.Cache.SetDefaultExt<APSRoughCutProcessFilter.excludeFirmOrders>(e.Row);
				return;
			}
		}

        public static void ProcessSchedule(List<AMSchdItem> list, APSRoughCutProcessFilter filter, bool isMassProcess)
        {
            ProcessSchedule(CreateInstance<ProductionScheduleEngineAdv>(), list, filter, isMassProcess);
        }

        public static void ProcessSchedule(ProductionScheduleEngineAdv schdEngine, List<AMSchdItem> list, APSRoughCutProcessFilter filter, bool isMassProcess)
        {
            var failed = false;
            var schdList = schdEngine.OrderScheduleBy(list);
			var prodMaint = CreateInstance<ProdMaint>();

			if (schdList == null || schdList.Count == 0)
            {
                PXTrace.WriteInformation("No schedules to process");
                return;
            }

			var processAction = filter?.ProcessAction ?? APSRoughCutProcessActions.Schedule;
			if (processAction == APSRoughCutProcessActions.Schedule || processAction == APSRoughCutProcessActions.ScheduleAndFirm)
			{
				try
				{
					if (schdList != null)
					{
						schdEngine.DeleteSchedule(schdList.Where(x => x.FirmSchedule == false).ToList(), false); 
						if (schdEngine.IsDirty)
						{
							schdEngine.Persist();
						}
					}
				}
				catch
				{
					PXTrace.WriteWarning("Error deleting schedules");
					throw;
				} 
			}

            for (var i = 0; i < schdList.Count; i++)
            {
                var schdItem = schdList[i];
                if (schdItem == null)
                {
                    continue;
                }
#if DEBUG
                AMDebug.TraceWriteMethodName(
                    $"Processing '{schdItem.OrderType.TrimIfNotNullEmpty()}' '{schdItem.ProdOrdID.TrimIfNotNullEmpty()}' with dispatch priority '{schdItem.SchPriority.GetValueOrDefault()}' and firm '{schdItem.FirmSchedule.GetValueOrDefault()}' and current end date {schdItem.EndDate}");
#endif

                try
                {
                    schdEngine.Clear();
					prodMaint.Clear();

					switch (processAction)
					{
						case APSRoughCutProcessActions.ScheduleAndFirm:
						case APSRoughCutProcessActions.Schedule:
							if(schdItem.FirmSchedule == true)
							{
								throw new PXException(Messages.GetLocal(Messages.ProductionOrderIsFirmNoSchedule, schdItem.OrderType, schdItem.ProdOrdID));
							}

							schdEngine.Process(schdEngine.SchdItems.Update(schdItem), processAction == APSRoughCutProcessActions.ScheduleAndFirm);
							break;
						case APSRoughCutProcessActions.Firm:
							if(schdItem.ScheduleStatus == ProductionScheduleStatus.Unscheduled)
							{
								throw new PXException(Messages.GetLocal(Messages.ProductionOrderIsUnscheduled, schdItem.OrderType, schdItem.ProdOrdID));
							}

							schdEngine.FirmOrder(schdEngine.SchdItems.Update(schdItem));
							break;
						case APSRoughCutProcessActions.UndoFirm:
							if(schdItem.ScheduleStatus == ProductionScheduleStatus.Unscheduled && schdItem.FirmSchedule == false)
							{
								throw new PXException(Messages.GetLocal(Messages.ProductionOrderIsUnscheduled, schdItem.OrderType, schdItem.ProdOrdID));
							}

							schdEngine.UndoFirmOrder(schdEngine.SchdItems.Update(schdItem));
							break;
					}

					schdEngine.Persist();

                    if (filter != null && filter.ReleaseOrders.GetValueOrDefault())
                    {
                        var prodItem = (AMProdItem)schdEngine.ProdItems.Cache.Locate(new AMProdItem
                        {
                            OrderType = schdItem.OrderType,
                            ProdOrdID = schdItem.ProdOrdID
                        }) ?? PXSelect<AMProdItem,
                                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>
                            .Select(schdEngine, schdItem.OrderType, schdItem.ProdOrdID);

                        if (prodItem != null && prodItem.Released == false)
                        {
							prodMaint.ProdMaintRecords.Current = prodItem;
							prodMaint.release.Press();
						}
                    }

                    if (isMassProcess)
                    {
                        PXProcessing<AMSchdItem>.SetInfo(i, PX.Data.ActionsMessages.RecordProcessed);
                    }
                }
                catch (Exception e)
                {
                    PXTraceHelper.PxTraceException(e);
                    failed = true;

                    if (isMassProcess)
                    {
                        PXProcessing<AMSchdItem>.SetError(i, e);
                    }
                    else if (schdList.Count == 1)
                    {
                        throw new PXOperationCompletedSingleErrorException(e);
                    }
                }
            }

            if (failed)
            {
                throw new PXOperationCompletedException(PX.Data.ErrorMessages.SeveralItemsFailed);
            }
        }
    }

    /// <summary>
    /// Filter DAC for APSRoughCutProcess graph
    /// </summary>
    [Serializable]
    [PXCacheName("Rough Cut Process Filter")]
    public class APSRoughCutProcessFilter : IBqlTable
    {
        #region ReleaseOrders
        /// <summary>
        /// During processing, should the selected orders be released (true)
        /// </summary>
        public abstract class releaseOrders : PX.Data.BQL.BqlBool.Field<releaseOrders> { }

        protected Boolean? _ReleaseOrders;
        /// <summary>
        /// During processing, should the selected orders be released (true)
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Release Orders")]
        public virtual Boolean? ReleaseOrders
        {
            get
            {
                return this._ReleaseOrders;
            }
            set
            {
                this._ReleaseOrders = value;
            }
        }
        #endregion
        #region ExcludePlanningOrders
        /// <summary>
        /// Show/hide planning type orders
        /// </summary>
        public abstract class excludePlanningOrders : PX.Data.BQL.BqlBool.Field<excludePlanningOrders> { }

        protected Boolean? _ExcludePlanningOrders;
        /// <summary>
        /// Show/hide planning type orders
        /// </summary>
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Exclude Planning Orders")]
        public virtual Boolean? ExcludePlanningOrders
        {
            get
            {
                return this._ExcludePlanningOrders;
            }
            set
            {
                this._ExcludePlanningOrders = value;
            }
        }
        #endregion
        #region ExcludeFirmOrders
        /// <summary>
        /// Show/hide orders which are schedule status Firm
        /// </summary>
        public abstract class excludeFirmOrders : PX.Data.BQL.BqlBool.Field<excludeFirmOrders> { }

        /// <summary>
        /// Show/hide orders which are schedule status Firm
        /// </summary>
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Exclude Firm Orders")]
        public virtual Boolean? ExcludeFirmOrders { get; set; }
		#endregion
		#region ProcessAction

        public abstract class processAction : PX.Data.BQL.BqlString.Field<processAction> { }

        [PXString(1, IsFixed = true)]
		[PXUnboundDefault(APSRoughCutProcessActions.Schedule)]
		[PXUIField(DisplayName = "Action")]
		[APSRoughCutProcessActions.List]
		public virtual string ProcessAction { get; set; }

        #endregion
	}
}
