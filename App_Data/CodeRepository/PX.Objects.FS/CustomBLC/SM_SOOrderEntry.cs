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
using PX.Data.WorkflowAPI;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static PX.Objects.FS.MessageHelper;

namespace PX.Objects.FS
{
    using State = SOOrderStatus;
    using static BoundedTo<SOOrderEntry, SOOrder>;
    using static PX.Objects.FS.ListField;

    public class SM_SOOrderEntry : FSPostingBase<SOOrderEntry>, IInvoiceContractGraph
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public class WorkflowChanges : PXGraphExtension<PX.Objects.SO.Workflow.SalesOrder.ScreenConfiguration, SOOrderEntry>
        {
            public static bool IsActive() => SM_SOOrderEntry.IsActive();

            public class Conditions : Condition.Pack
            {
                public Condition IsFSActivatedForOrderType => GetOrCreate(b => b.FromBql<
                    FSxSOOrderType.enableFSIntegration.FromCurrent.IsEqual<True>
                >());
            }

            public sealed override void Configure(PXScreenConfiguration config) =>
                Configure(config.GetScreenConfigurationContext<SOOrderEntry, SOOrder>());
            protected static void Configure(WorkflowContext<SOOrderEntry, SOOrder> context)
            {
                var conditions = context.Conditions.GetPack<Conditions>();

                var servicesCategory = context.Categories.CreateNew(ActionCategories.ServicesCategoryID,
                    category => category.DisplayName(ActionCategories.DisplayNames.Services).
                    PlaceAfter(SO.Workflow.SalesOrder.ScreenConfiguration.ActionCategories.ReplenishmentCategoryID));

                context.UpdateScreenConfigurationFor(screen =>
                {
                    return screen
                        .WithActions(actions =>
                        {
                            actions.Add<SM_SOOrderEntry>(g => g.CreateServiceOrder, a => a.WithCategory(servicesCategory).IsHiddenWhen(!conditions.IsFSActivatedForOrderType));
                            actions.Add<SM_SOOrderEntry>(g => g.OpenAppointmentBoard, a => a.WithCategory(servicesCategory).IsHiddenWhen(!conditions.IsFSActivatedForOrderType));
                            actions.Add<SM_SOOrderEntry>(g => g.ViewServiceOrder, a => a.WithCategory(PredefinedCategory.Inquiries).IsHiddenWhen(!conditions.IsFSActivatedForOrderType));
                        })
                        .WithCategories(categories =>
                        {
                            categories.Add(servicesCategory);
                        });
                });
            }

            public static class ActionCategories
            {
                public const string ServicesCategoryID = "Services Category";

                [PXLocalizable]
                public static class DisplayNames
                {
                    public const string Services = "Services";
                }

            }
        }

        [PXCopyPasteHiddenView]
        public PXFilter<FSCreateServiceOrderFilter> CreateServiceOrderFilter;

        #region SrvOrdType
        [PXDefault(typeof(Coalesce<
            Search<FSxUserPreferences.dfltSrvOrdType,
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>>>,
            Search<FSSetup.dfltSOSrvOrdType>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSCreateServiceOrderFilter_SrvOrdType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        
        public virtual bool IsFSIntegrationEnabled()
        {
            bool isFSIntegrated = true;

            if (Base.soordertype.Current == null)
            {
                isFSIntegrated = false;
            }
            else
            {
                FSxSOOrderType fsxSOOrderTypeRow = Base.soordertype.Cache.GetExtension<FSxSOOrderType>(Base.soordertype.Current);

                if (fsxSOOrderTypeRow == null || fsxSOOrderTypeRow.EnableFSIntegration != true)
                {
                    isFSIntegrated = false;
                }
            }

            if (Base.Document.Current != null)
            {
                FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(Base.Document.Current);

                if (fsxSOOrderRow != null)
                {
                    fsxSOOrderRow.IsFSIntegrated = isFSIntegrated;
                }
            }

            return isFSIntegrated;
        }

        public delegate void InvoiceOrderDelegate(Dictionary<string, object> parameters,
                                                 IEnumerable<SOOrder> list,
                                                 InvoiceList created,
                                                 bool isMassProcess,
                                                 PXQuickProcess.ActionFlow quickProcessFlow,
                                                 bool groupByCustomerOrderNumber);

        /// <summary>
        /// Overrides <see cref="SOOrderEntry.InvoiceOrder(Dictionary{string, object}, IEnumerable{SOOrder}, InvoiceList, bool, PXQuickProcess.ActionFlow, bool)"/>
        /// </summary>
        [PXOverride]
        public virtual void InvoiceOrder(Dictionary<string, object> parameters,
                                         IEnumerable<SOOrder> list,
                                         InvoiceList created,
                                         bool isMassProcess,
                                         PXQuickProcess.ActionFlow quickProcessFlow,
                                         bool groupByCustomerOrderNumber,
                                         InvoiceOrderDelegate del)
        {
            foreach (SOOrder order in list)
            {
                ValidatePostBatchStatus(PXDBOperation.Update, ID.Batch_PostTo.SO, order.OrderType, order.RefNbr);
            }

            del(parameters, list, created, isMassProcess, quickProcessFlow, groupByCustomerOrderNumber);
        }

        #region Selects
        [PXHidden]
        public PXSelect<FSSetup> SetupRecord;
       
        #endregion

        #region Actions

        public PXAction<SOOrder> OpenAppointmentBoard;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Schedule on the Calendar Board", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openAppointmentBoard()
        {
            if (Base.Document == null || Base.Document.Current == null)
            {
                return;
            }

            //Try to save all unsaved changes
		    if (Base.IsDirty)
		    {
                Base.Save.Press();
		    }

            FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(Base.Document.Current);

            if (fsxSOOrderRow != null && fsxSOOrderRow.ServiceOrderRefNbr != null)
            {
                ServiceOrderEntry graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();
                FSServiceOrder fsServiceOrderRow = GetServiceOrderRecord(Base.Document.Current);

                if (fsServiceOrderRow != null)
                {
                    graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
                                    .Search<FSServiceOrder.refNbr>(fsServiceOrderRow.RefNbr, fsServiceOrderRow.SrvOrdType);
                    graphServiceOrder.OpenEmployeeBoard();
                }
            }
        }

        #region CopyOrder

        [PXOverride]
        public virtual IEnumerable CopyOrder(PXAdapter adapter)
        {
            if (Base.Document == null || Base.Document.Current == null)
            {
                return adapter.Get();
            }

            List<SOOrder> soOrderList = new List<SOOrder>();
            SOOrder soOrderRow = Base.Document.Current;

            FSxSOOrder fsxSOOrderRow = PXCache<SOOrder>.GetExtension<FSxSOOrder>(soOrderRow);
            if (fsxSOOrderRow != null
                    && fsxSOOrderRow.SDEnabled == true)
            {
                fsxSOOrderRow.ServiceOrderRefNbr = null;
            }

            soOrderList.Add(soOrderRow);

            return soOrderList;
        }
        #endregion

        #region CreateServiceOrder
        public PXAction<SOOrder> CreateServiceOrder;
        [PXButton]
        [PXUIField(DisplayName = "Create Service Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void createServiceOrder()
        {
            if (Base.Document == null || Base.Document.Current == null)
            {
                return;
            }

            //Try to save all unsaved changes
            if (Base.IsDirty)
            {
                Base.Save.Press();
            }

            SOOrder soOrderRow = Base.Document.Current;
            FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(soOrderRow);

            if (CreateServiceOrderFilter.AskExt() == WebDialogResult.OK)
            {
                if (CreateServiceOrderFilter.Current.SrvOrdType == null)
                {
                    CreateServiceOrderFilter.Cache.RaiseExceptionHandling<FSCreateServiceOrderFilter.srvOrdType>(CreateServiceOrderFilter.Current, null, new PXSetPropertyException(TX.Error.FIELD_EMPTY, PXErrorLevel.Error));
                }
                else
                {
                    fsxSOOrderRow.SDEnabled = true;
                    fsxSOOrderRow.AssignedEmpID = CreateServiceOrderFilter.Current.AssignedEmpID;
                    fsxSOOrderRow.SrvOrdType = CreateServiceOrderFilter.Current.SrvOrdType;
                    fsxSOOrderRow.SLAETA = CreateServiceOrderFilter.Current.SLAETA;

                    PXLongOperation.StartOperation(Base, delegate ()
                    {
                        InsertUpdateDeleteServiceOrderDocument(soOrderRow, fsxSOOrderRow, PXDBOperation.Insert);
                    });
                }
            }
        }
        #endregion

        #region ViewServiceOrder
        public PXAction<SOOrder> ViewServiceOrder;
        [PXButton]
        [PXUIField(DisplayName = "View Service Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void viewServiceOrder()
        {
            if (Base.Document == null || Base.Document.Current == null)
            {
                return;
            }

            if (Base.IsDirty)
            {
                Base.Save.Press();
            }

            SOOrder soOrderRow = Base.Document.Current;
            FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(soOrderRow);

            if (fsxSOOrderRow != null
                && fsxSOOrderRow.SDEnabled == true
                && fsxSOOrderRow.SrvOrdType != null
                && fsxSOOrderRow.ServiceOrderRefNbr != null)
            {
                FSServiceOrder fsServiceOrderRow = GetServiceOrderRecord(soOrderRow);
                CRExtensionHelper.LaunchServiceOrderScreen(Base, fsServiceOrderRow.RefNbr, fsServiceOrderRow.SrvOrdType);
            }
        }
        #endregion

        [PXOverride]
        public virtual Type[] GetAlternativeKeyFields(Func<Type[]> baseMethod)
        {
            var result = new List<Type>(baseMethod());

            result.Add(typeof(FSxSOLine.componentID));
            result.Add(typeof(FSxSOLine.equipmentComponentLineNbr));
            result.Add(typeof(FSxSOLine.srvOrdType));
            result.Add(typeof(FSxSOLine.appointmentRefNbr));
            result.Add(typeof(FSxSOLine.serviceOrderRefNbr));

            return result.ToArray();
        }
        #endregion

        #region Virtual Methods
        public virtual bool CanCreateServiceOrder(PXCache cache, SOOrder soOrderRow, FSxSOOrder fsxSOOrderRow)
        {
            if (soOrderRow == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(soOrderRow.OrderType) == true)
            {
                return false;
            }

            FSPostInfo fsPostInfoRow = PXSelect<FSPostInfo,
                                       Where<
                                           FSPostInfo.sOOrderType, Equal<Required<FSPostInfo.sOOrderType>>,
                                           And<FSPostInfo.sOOrderNbr, Equal<Required<FSPostInfo.sOOrderNbr>>>>>
                                       .Select(Base, soOrderRow.OrderType, soOrderRow.OrderNbr);

            if (fsPostInfoRow != null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the ServiceOrder record associated to the selected Sales Order.
        /// </summary>
        public virtual FSServiceOrder GetServiceOrderRecord(SOOrder soOrderRow)
        {
            return PXSelect<FSServiceOrder,
                   Where<
                       FSServiceOrder.sourceType, Equal<ListField_SourceType_ServiceOrder.SalesOrder>,
                       And<FSServiceOrder.sourceDocType, Equal<Required<FSServiceOrder.sourceDocType>>,
                       And<FSServiceOrder.sourceRefNbr, Equal<Required<FSServiceOrder.sourceRefNbr>>>>>>
                   .Select(Base, soOrderRow.OrderType, soOrderRow.OrderNbr);
        }

        /// <summary>
        /// Initializes a new Service Order.
        /// </summary>
        public virtual FSServiceOrder InitNewServiceOrder(ServiceOrderEntry graphServiceOrder, SOOrder soOrderRow, FSxSOOrder fsxSOOrderRow)
        {
            //Initialize the FSServiceOrder with its KEY fieds
            var fsServiceOrderRow = new FSServiceOrder();

            fsServiceOrderRow.SrvOrdType = fsxSOOrderRow.SrvOrdType;

            //Assigns the Sales Order reference to the new Service Order
            fsServiceOrderRow.SourceType = ID.SourceType_ServiceOrder.SALES_ORDER;
            fsServiceOrderRow.SourceDocType = soOrderRow.OrderType;
            fsServiceOrderRow.SourceRefNbr = soOrderRow.OrderNbr;
            fsServiceOrderRow.Hold = soOrderRow.Hold;

			fsServiceOrderRow = graphServiceOrder.ServiceOrderRecords.Insert(fsServiceOrderRow);

			CheckAutoNumbering(graphServiceOrder.ServiceOrderTypeSelected.SelectSingle().SrvOrdNumberingID);

			return fsServiceOrderRow;
        }

        /// <summary>
        /// Deletes the Service Order and blanks the <c>fsxSOOrderRow.SOID</c> memory field.
        /// </summary>
        public virtual void DeleteServiceOrder(ServiceOrderEntry graphServiceOrder, FSServiceOrder fsServiceOrderRow, FSxSOOrder fsxSOOrderRow)
        {
            graphServiceOrder.ServiceOrderRecords.Delete(fsServiceOrderRow);
            graphServiceOrder.Save.Press();
            fsxSOOrderRow.ServiceOrderRefNbr = null;
        }

        /// <summary>
        /// Updates the ServiceOrder information using the Sales Order definition.
        /// </summary>
        public virtual void UpdateServiceOrderHeader(ServiceOrderEntry graphServiceOrder,
                                                     FSServiceOrder fsServiceOrderRow,
                                                     SOOrder soOrderRow,
                                                     FSxSOOrder fsxSOOrderRow,
                                                     PXDBOperation operation)
        {
            bool updateRequired = false;

            SOShippingContact soShippingContactRow = Base.Shipping_Contact.Select();
            SOShippingAddress soShippingAddressRow = Base.Shipping_Address.Select();

            //**************************************************************
            // Update all FSServiceOrder fields but key fields *************
            //**************************************************************
            if (fsServiceOrderRow.CustomerID != soOrderRow.CustomerID)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.customerID>(fsServiceOrderRow, soOrderRow.CustomerID);
                updateRequired = true;
            }

            if (fsServiceOrderRow.CuryID != soOrderRow.CuryID 
                    && operation == PXDBOperation.Insert)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.curyID>(fsServiceOrderRow, soOrderRow.CuryID);
                updateRequired = true;
            }

            if (fsServiceOrderRow.ProjectID != soOrderRow.ProjectID)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.projectID>(fsServiceOrderRow, soOrderRow.ProjectID);
                updateRequired = true;
            }

            if (fsServiceOrderRow.OrderDate != soOrderRow.RequestDate)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.orderDate>(fsServiceOrderRow, soOrderRow.RequestDate);
                updateRequired = true;
            }

            if (soOrderRow.OrderDesc != null && soOrderRow.OrderDesc != fsServiceOrderRow.DocDesc)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.docDesc>(fsServiceOrderRow, soOrderRow.OrderDesc);
                updateRequired = true;
            }

            if (fsServiceOrderRow.LocationID != soOrderRow.CustomerLocationID)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.locationID>(fsServiceOrderRow, soOrderRow.CustomerLocationID);
                updateRequired = true;
            }

            if (graphServiceOrder.ServiceOrderRecords.Current.BranchLocationID == null)
            {
                int? branchLocationID = GetBranchLocation(this.Base.Accessinfo.BranchID);
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.branchLocationID>(fsServiceOrderRow, branchLocationID);
                updateRequired = true;
            }

            if (fsServiceOrderRow.AllowOverrideContactAddress != soShippingContactRow.OverrideContact
                || fsServiceOrderRow.AllowOverrideContactAddress != soShippingAddressRow.OverrideAddress)
                {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.allowOverrideContactAddress>
                                (fsServiceOrderRow, soShippingContactRow.OverrideContact == true || soShippingAddressRow.OverrideAddress == true);

                FSContact fsContactRow = graphServiceOrder.ServiceOrder_Contact.Select();
                FSAddress fsAddressRow = graphServiceOrder.ServiceOrder_Address.Select();

				Guid ? fsContactNoteID = fsContactRow?.NoteID;
                InvoiceHelper.CopyContact(fsContactRow, soShippingContactRow);
				fsContactRow.NoteID = fsContactNoteID ?? soShippingContactRow.NoteID;
                graphServiceOrder.ServiceOrder_Contact.Update(fsContactRow);

                InvoiceHelper.CopyAddress(fsAddressRow, soShippingAddressRow);
                graphServiceOrder.ServiceOrder_Address.Update(fsAddressRow);

                    updateRequired = true;
                }

            if (fsServiceOrderRow.AssignedEmpID != fsxSOOrderRow.AssignedEmpID)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.assignedEmpID>(fsServiceOrderRow, fsxSOOrderRow.AssignedEmpID);
                updateRequired = true;
            }

            if (fsServiceOrderRow.SLAETA != fsxSOOrderRow.SLAETA)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.sLAETA>(fsServiceOrderRow, fsxSOOrderRow.SLAETA);
                updateRequired = true;
            }

            if (fsServiceOrderRow.CustPORefNbr != soOrderRow.CustomerOrderNbr)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.custPORefNbr>(fsServiceOrderRow, soOrderRow.CustomerOrderNbr);
                updateRequired = true;
            }

            if (fsServiceOrderRow.CustWorkOrderRefNbr != soOrderRow.CustomerRefNbr)
            {
                graphServiceOrder.ServiceOrderRecords.SetValueExt<FSServiceOrder.custWorkOrderRefNbr>(fsServiceOrderRow, soOrderRow.CustomerRefNbr);
                updateRequired = true;
            }

            UDFHelper.CopyAttributes(graphServiceOrder.Caches<SOOrder>(), soOrderRow, graphServiceOrder.ServiceOrderRecords.Cache, fsServiceOrderRow, null);

            if (operation == PXDBOperation.Update && updateRequired)
            {
                graphServiceOrder.ServiceOrderRecords.Cache.SetStatus(graphServiceOrder.ServiceOrderRecords.Current, PXEntryStatus.Updated);
            }

            if (fsxSOOrderRow?.ServiceOrderRefNbr == null)
            {
                SharedFunctions.CopyNotesAndFiles(Base.Document.Cache,
                                                  graphServiceOrder.ServiceOrderRecords.Cache,
                                                  soOrderRow,
                                                  graphServiceOrder.ServiceOrderRecords.Current,
                                                  Base.soordertype?.Current.CopyNotes,
                                                  Base.soordertype?.Current.CopyFiles);
            }
        }

        public virtual int? GetBranchLocation(int? branchID)
        {
            int? branchLocationID = null;

            FSBranchLocation fSBranchLocationRow = PXSelect<FSBranchLocation,
                                                   Where<
                                                       FSBranchLocation.branchID, Equal<Required<FSBranchLocation.branchID>>>>
                                                   .Select(Base, branchID);
            if (fSBranchLocationRow != null)
            {
                branchLocationID = fSBranchLocationRow.BranchLocationID; 
            }

            if (branchLocationID == null)
            {
                throw new PXException(TX.Error.NO_AVAILABLE_BRANCH_LOCATION_IN_CURRENT_BRANCH);
            }

            return branchLocationID;
        }

        /// <summary>
        /// Adjusts the Header after removing, updating or creating the Sales Order.
        /// </summary>
        public virtual FSServiceOrder InsertUpdateDeleteServiceOrderDocument(SOOrder soOrderRow, FSxSOOrder fsxSOOrderRow, PXDBOperation salesOrderOperation)
        {
            ServiceOrderEntry graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();
            FSServiceOrder fsServiceOrderRow = GetServiceOrderRecord(soOrderRow);
            PXDBOperation serviceOrderOperation = PXDBOperation.Insert;

            if (fsServiceOrderRow != null)
            {
                //Load existing ServiceOrder
                graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
                                    .Search<FSServiceOrder.refNbr>(fsServiceOrderRow.RefNbr, fsServiceOrderRow.SrvOrdType);

                //Delete existing ServiceOrder
                if (salesOrderOperation == PXDBOperation.Delete || salesOrderOperation == (PXDBOperation.Insert | PXDBOperation.Update) || fsxSOOrderRow.SDEnabled == false)
                {
                    DeleteServiceOrder(graphServiceOrder, graphServiceOrder.ServiceOrderRecords.Current, fsxSOOrderRow);
                    return null;
                }

                serviceOrderOperation = PXDBOperation.Update;
                graphServiceOrder.SelectTimeStamp();
            }
            else
            {
                //This Sales Order does not require Service Order
                if (fsxSOOrderRow.SDEnabled == false)
                {
                    return null;
                }

                //Init a NEW Service Order
                graphServiceOrder.ServiceOrderRecords.Current = InitNewServiceOrder(graphServiceOrder, soOrderRow, fsxSOOrderRow);
            }

            //Update ServiceOrder header
            UpdateServiceOrderHeader(graphServiceOrder, 
                                     graphServiceOrder.ServiceOrderRecords.Current,
                                     soOrderRow, 
                                     fsxSOOrderRow,
                                     serviceOrderOperation);

            ////Update ServiceOrder detail
            InsertUpdateDeleteServiceOrderDetail(graphServiceOrder);

            if (graphServiceOrder.IsDirty == true
                    || graphServiceOrder.ServiceOrderRecords.Cache.GetStatus(graphServiceOrder.ServiceOrderRecords.Current) == PXEntryStatus.Updated)
            {
                if (!Base.IsContractBasedAPI 
                        && graphServiceOrder.ServiceOrderRecords.Cache.GetStatus(graphServiceOrder.ServiceOrderRecords.Current) == PXEntryStatus.Inserted)
                {
                    throw new PXRedirectRequiredException(graphServiceOrder, null);
                }

                graphServiceOrder.Save.Press();
            }

            return graphServiceOrder.ServiceOrderRecords.Current;
        }

        public virtual void InsertUpdateDeleteFSSODet<SODetType>(PXCache cacheFSSODet,
                                                                 SOLine soLineRow,
                                                                 FSxSOLine fsxSOLineRow,
                                                                 FSSODet fsSODetRow,
                                                                 string lineType)
            where SODetType : FSSODet, IBqlTable, new()
        {
            if (fsSODetRow != null)
            {
                //Delete an existing appointment requirement
                if (fsxSOLineRow.SDSelected == false)
                {
                    cacheFSSODet.Delete(fsSODetRow);
                    return;
                }
                else
                {
                    cacheFSSODet.Current = fsSODetRow;

                    // This copy is to edit the copy row and after update the record in the cache with cache.Update
                    fsSODetRow = (SODetType)cacheFSSODet.CreateCopy(fsSODetRow);

                    InsertUpdateServiceOrderLine<SODetType>(cacheFSSODet, fsSODetRow, false, lineType, soLineRow, fsxSOLineRow);
                    return;
                }
            }
            else
            {
                //This line does not require appointment
                if (fsxSOLineRow.SDSelected == false)
                {
                    return;
                }

                InsertUpdateServiceOrderLine<SODetType>(cacheFSSODet, fsSODetRow, true, lineType, soLineRow, fsxSOLineRow);
            }
        }

        /// <summary>
        /// Enable or Disable Sales Order lines fields.
        /// </summary>
        /// <param name="cache">Sales Order line cache.</param>
        /// <param name="soLineRow">Sales Order line row.</param>
        public virtual void EnableDisableSOline(PXCache cache, SOLine soLineRow, bool fsIntegrationEnabled)
        {
            FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(Base.Document.Current);

            PXUIFieldAttribute.SetEnabled<FSxSOLine.sDSelected>(cache, soLineRow, fsIntegrationEnabled);
            PXUIFieldAttribute.SetEnabled<FSxSOLine.equipmentAction>(cache, soLineRow, soLineRow.InventoryID != null && fsIntegrationEnabled);
        }

        public virtual void DeleteFSSODetLinesFromDeletedSOlines(ServiceOrderEntry graphServiceOrder)
        {
            FSxSOLine fsxSOLineRow = null;

            //Delete SODet lines based on deleted-SOLine collection
            foreach (SOLine soLineRow in Base.Transactions.Cache.Deleted)
            {
                fsxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(soLineRow);

                //There is nothing to do with a posted line
                if (fsxSOLineRow.SDPosted == true)
                {
                    continue;
                }

                graphServiceOrder.ServiceOrderDetails.Current = graphServiceOrder.ServiceOrderDetails
                                                                .Search<FSSODet.sourceLineNbr>(soLineRow.LineNbr);

                if (graphServiceOrder.ServiceOrderDetails.Current != null)
                {
                    graphServiceOrder.ServiceOrderDetails.Delete(graphServiceOrder.ServiceOrderDetails.Current);
                }
            }
        }

        public virtual void InsertUpdateDeleteServiceOrderDetail(ServiceOrderEntry graphServiceOrder)
        {
            DeleteFSSODetLinesFromDeletedSOlines(graphServiceOrder);

            FSxSOLine fsxSOLineRow;

            //Insert/Update SODet lines based on existing SOLine lines
            foreach (SOLine soLineRow in Base.Transactions.Select())
            {
                fsxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(soLineRow);

                //There is nothing to do with a posted line
                if (fsxSOLineRow.SDPosted == true)
                {
                    continue;
                }
                
                InventoryItem inventoryItemRow = InventoryItem.PK.Find(this.Base, soLineRow.InventoryID);

                PXCache cacheSODet = null;
                FSSODet fsSODetRow = null;
                string lineType = null;

                cacheSODet = graphServiceOrder.ServiceOrderDetails.Cache;
                fsSODetRow = graphServiceOrder.ServiceOrderDetails.Search<FSSODet.sourceLineNbr>(soLineRow.LineNbr);

                if (inventoryItemRow.ItemType == INItemTypes.ServiceItem)
                {
                    lineType = ID.LineType_ALL.SERVICE;
                }
                else if (inventoryItemRow.ItemType == INItemTypes.FinishedGood
                            || inventoryItemRow.ItemType == INItemTypes.Component
                            || inventoryItemRow.ItemType == INItemTypes.SubAssembly)
                {
                    lineType = ID.LineType_ALL.INVENTORY_ITEM;
                }
                else
                {
                    lineType = ID.LineType_ALL.NONSTOCKITEM;
                }

                InsertUpdateDeleteFSSODet<FSSODet>(cacheSODet, soLineRow, fsxSOLineRow, fsSODetRow, lineType);
            }

            if (graphServiceOrder.ServiceOrderDetails.Cache.Inserted.RowCast<FSSODet>().Where(x => x.IsInventoryItem == true).Count() > 0)
            { 
                //Assigning the NewTargetEquipmentLineNbr field value for the component type records
                foreach (FSSODet fsSODetPartModelRow in graphServiceOrder.ServiceOrderDetails
                                                                         .Cache.Cached
                                                                         .Cast<FSSODet>()
                                                                         .Where(x => x.IsInventoryItem == true 
                                                                                  && x.EquipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT)
                                                                         .ToList())
                {
                    foreach (FSSODet fsSODetPartComponentRow in graphServiceOrder.ServiceOrderDetails
                                                                                 .Cache.Cached
                                                                                 .Cast<FSSODet>()
                                                                                 .Where(x => x.SONewTargetEquipmentLineNbr > 0)
                                                                                 .ToList())
                    {
                        if (fsSODetPartComponentRow.SONewTargetEquipmentLineNbr == fsSODetPartModelRow.SourceLineNbr)
                        {
                            if (fsSODetPartComponentRow.NewTargetEquipmentLineNbr == null)
                            {
                                fsSODetPartComponentRow.NewTargetEquipmentLineNbr = fsSODetPartModelRow.LineRef;
                            }
                        }
                    }
                }
            }
        }

        public virtual void InsertUpdateServiceOrderLine<SODetType>(PXCache cacheSODet,
                                                                    FSSODet fsSODetRow,
                                                                    bool insertRow,
                                                                    string lineType,
                                                                    SOLine soLineRow,
                                                                    FSxSOLine fsxSOLineRow)
            where SODetType : FSSODet, IBqlTable, new()
        {
            bool updateRequired = false;

            if (insertRow == true)
            {
                fsSODetRow = new SODetType();
                fsSODetRow.SourceLineNbr = soLineRow.LineNbr;
                fsSODetRow.IsPrepaid = true;
                fsSODetRow.LinkedDocType = soLineRow.OrderType;
                fsSODetRow.LinkedDocRefNbr = soLineRow.OrderNbr;
                fsSODetRow.LinkedEntityType = FSSODet.linkedEntityType.Values.SalesOrder;
            }

            if (fsSODetRow.InventoryID != soLineRow.InventoryID)
            {
                fsSODetRow.InventoryID = soLineRow.InventoryID;
                updateRequired = true;

                if (insertRow)
                {
                    fsSODetRow.LineType = lineType;

                    if (lineType == ID.LineType_ALL.SERVICE)
                    {
                        fsSODetRow.BillingRule = ID.BillingRule.FLAT_RATE;
                    }

                    fsSODetRow = (SODetType)cacheSODet.Insert(fsSODetRow);
                }
            }

            if (fsSODetRow.SubItemID != soLineRow.SubItemID)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.subItem>())
                {
                    fsSODetRow.SubItemID = soLineRow.SubItemID;
                    updateRequired = true;
                }
            }

            if (fsSODetRow.SiteID != soLineRow.SiteID)
            {
                fsSODetRow.SiteID = soLineRow.SiteID;
                updateRequired = true;
            }

            if (fsSODetRow.SiteLocationID != soLineRow.LocationID)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
                {
                    fsSODetRow.SiteLocationID = soLineRow.LocationID;
                    updateRequired = true;
                }
            }

            if (fsSODetRow.UOM != soLineRow.UOM)
            {
                fsSODetRow.UOM = soLineRow.UOM;
                updateRequired = true;
            }

            if (fsSODetRow.ProjectID != soLineRow.ProjectID
                    && ProjectDefaultAttribute.IsNonProject(soLineRow.ProjectID) == false)
            {
                fsSODetRow.ProjectID = soLineRow.ProjectID;
                updateRequired = true;
            }

            if (fsSODetRow.ProjectTaskID != soLineRow.TaskID)
            {
                fsSODetRow.ProjectTaskID = soLineRow.TaskID;
                updateRequired = true;
            }

            if (fsSODetRow.CostCodeID != soLineRow.CostCodeID)
            {
                fsSODetRow.CostCodeID = soLineRow.CostCodeID;
                updateRequired = true;
            }

            if (fsSODetRow.DiscPct != soLineRow.DiscPct)
            {
                fsSODetRow.DiscPct = soLineRow.DiscPct;
                updateRequired = true;
            }

            if (fsSODetRow.CuryDiscAmt != soLineRow.CuryDiscAmt)
            {
                fsSODetRow.CuryDiscAmt = soLineRow.CuryDiscAmt;
                updateRequired = true;
            }

            if (fsSODetRow.TranDesc != soLineRow.TranDesc)
            {
                fsSODetRow.TranDesc = soLineRow.TranDesc;
                updateRequired = true;
            }

            if (fsSODetRow.EstimatedQty != soLineRow.OrderQty
                || fsSODetRow.CuryUnitPrice != soLineRow.CuryUnitPrice)
            {
                updateRequired = true;
            }

            if (updateRequired == true || insertRow == true)
            {
                fsSODetRow.LineType = lineType;
                if (lineType == ID.LineType_ALL.SERVICE)
                {
                    fsSODetRow.BillingRule = ID.BillingRule.FLAT_RATE;
                }

                fsSODetRow.IsFree = soLineRow.IsFree;
                fsSODetRow.EstimatedQty = soLineRow.OrderQty;
                fsSODetRow.CuryUnitPrice = soLineRow.CuryUnitPrice;
                fsSODetRow.ManualPrice = soLineRow.ManualPrice;
                    fsSODetRow.EquipmentAction = fsxSOLineRow.EquipmentAction ?? ID.Equipment_Action.NONE;
                    fsSODetRow.SMEquipmentID = fsxSOLineRow.SMEquipmentID;
                    fsSODetRow.EquipmentLineRef = fsxSOLineRow.EquipmentComponentLineNbr;
                    fsSODetRow.ComponentID = fsxSOLineRow.ComponentID;

                    //Saving original values from SalesOrder to allow assign the proper NewTargetEquipmentLineNbr value in ServiceOrder
                    fsSODetRow.SONewTargetEquipmentLineNbr = fsxSOLineRow.NewEquipmentLineNbr;

                SharedFunctions.CopyNotesAndFiles(Base.Transactions.Cache,
                                                  cacheSODet,
                                                  soLineRow,
                                                  fsSODetRow,
                                                  Base.soordertype?.Current.CopyNotes,
                                                  Base.soordertype?.Current.CopyFiles);

                cacheSODet.Update(fsSODetRow);
            }
        }

        /// <summary>
        /// Clean SM fields depending if SDEnabled checkbox is not selected.
        /// </summary>
        public virtual void CleanSMFieldsBeforeSaving(SOOrder soOrderRow)
        {
            FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(soOrderRow);

            if (fsxSOOrderRow.SDEnabled == false)
            {
                fsxSOOrderRow.SrvOrdType = null;
                fsxSOOrderRow.ServiceOrderRefNbr = null;
                fsxSOOrderRow.AssignedEmpID = null;
                fsxSOOrderRow.SLAETA = (System.DateTime?)null;
                fsxSOOrderRow.Installed = false;

                foreach (SOLine soLineRow in Base.Transactions.Select())
                {
                    FSxSOLine fsxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(soLineRow);
                    fsxSOLineRow.SDSelected = false;
                }
            }
        }

        /// <summary>
        /// Check if the current selected project belongs to the current customer.
        /// This applies only to a Sales Order that will be related with a Service Order.
        /// </summary>
        public virtual void CheckIfCurrentProjectBelongsToCustomer(PXCache cache, SOOrder soOrderRow)
        {
            FSxSOOrder fsxSOOrderRow = cache.GetExtension<FSxSOOrder>(soOrderRow);
            PXSetPropertyException exception = null;

            if (soOrderRow.ProjectID != null
                    && soOrderRow.CustomerID != null
                        && soOrderRow.ProjectID != 0
                            && fsxSOOrderRow.SDEnabled == true)
            {
                Contract contract = PXSelect<Contract,
                                    Where<
                                        Contract.contractID, Equal<Required<Contract.contractID>>,
                                        And<Contract.customerID, Equal<Required<Contract.contractID>>>>>
                                    .Select(Base, soOrderRow.ProjectID, soOrderRow.CustomerID);

                if (contract == null)
                {
                    exception = new PXSetPropertyException(TX.Error.PROJECT_MUST_BELONG_TO_CUSTOMER, PXErrorLevel.RowError);
                }

                if (PXUIFieldAttribute.GetError<SOOrder.customerID>(cache, soOrderRow) == string.Empty
                            || PXUIFieldAttribute.GetError<SOOrder.customerID>(cache, soOrderRow) == TX.Error.PROJECT_MUST_BELONG_TO_CUSTOMER)
                {
                    cache.RaiseExceptionHandling<SOOrder.customerID>(soOrderRow,
                                                                     soOrderRow.CustomerID,
                                                                     exception);
                }
            }
        }

        /// <summary>
        /// Check if the given Sales Order line is related with any appointment details.
        /// </summary>
        /// <param name="soOrderRow">Sales Order row.</param>
        /// <param name="soLineRow">Sales Order line.</param>
        /// <returns>Returns true if the Sales Order Line is related with at least one appointment detail.</returns>
        public virtual bool IsLineSourceForAppoinmentLine(SOOrder soOrderRow, SOLine soLineRow, FSServiceOrder fsServiceOrderRow)
        {
            if (fsServiceOrderRow == null)
            {
                fsServiceOrderRow = GetServiceOrderRecord(soOrderRow);
            }

            if (fsServiceOrderRow != null)
            {
                FSAppointmentDet fsAppointmentDetRow = PXSelectJoin<FSAppointmentDet,
                                                       InnerJoin<FSSODet,
                                                       On<
                                                           FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>>,
                                                       Where<
                                                           FSSODet.sourceLineNbr, Equal<Required<FSSODet.sourceLineNbr>>,
                                                           And<FSSODet.sOID, Equal<Required<FSSODet.sOID>>>>>
                                                       .SelectWindowed(Base, 0, 1, soLineRow.LineNbr, fsServiceOrderRow.SOID);

                return fsAppointmentDetRow != null;
            }

            return false;
        }

        /// <summary>
        /// Check if the given Sales Order Lines are different in Service Management prepaid related fields.
        /// </summary>
        public virtual bool ArePrepaidFieldsBeingModified(SOLine soLineRow, SOLine newSOLineRow)
        {
            FSxSOLine fsxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(soLineRow);
            FSxSOLine newFSxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(newSOLineRow);

            return soLineRow.InventoryID        != newSOLineRow.InventoryID
                   || soLineRow.SubItemID       != newSOLineRow.SubItemID
                   || soLineRow.SiteID          != newSOLineRow.SiteID
                   || soLineRow.UOM             != newSOLineRow.UOM
                   || soLineRow.UnitPrice       != newSOLineRow.UnitPrice
                   || soLineRow.OrderQty        != newSOLineRow.OrderQty
                   || soLineRow.ProjectID       != newSOLineRow.ProjectID
                   || soLineRow.TaskID          != newSOLineRow.TaskID
                   || soLineRow.TranDesc        != newSOLineRow.TranDesc
                   || fsxSOLineRow.SDSelected   != newFSxSOLineRow.SDSelected;
        }

        #region EnableDisable functions
        /// <summary>
        /// Enables/Disables the "Open Appointment Board" button.
        /// </summary>
        protected virtual void EnableDisableActions(PXCache cache, SOOrder soOrderRow, FSxSOOrder fsxSOOrderRow, bool allowCreateServiceOrder, bool fsIntegrationEnabled)
        {
            bool enabledBoard = false;
            bool enabledCreateSO = false;

            if (soOrderRow != null && cache.GetStatus(soOrderRow) != PXEntryStatus.Inserted
                                && fsxSOOrderRow != null)
            {
                if (fsxSOOrderRow.ServiceOrderRefNbr != null)
                {
                    enabledBoard = true;
                }
                else
                {
                    enabledCreateSO = true;
                }
            }

            OpenAppointmentBoard.SetEnabled(enabledBoard && fsIntegrationEnabled);
            CreateServiceOrder.SetEnabled(enabledCreateSO && allowCreateServiceOrder && fsIntegrationEnabled);
            ViewServiceOrder.SetEnabled(enabledBoard && fsIntegrationEnabled);
        }

        /// <summary>
        /// Enables the Fields if they can be edited.
        /// </summary>
        protected virtual void EnableDisable_All(PXCache cache, SOOrder soOrderRow, bool fsIntegrationEnabled)
        {
            FSxSOOrder fsxSOOrderRow = cache.GetExtension<FSxSOOrder>(soOrderRow);

            bool allowCreateServiceOrder = CanCreateServiceOrder(cache, soOrderRow, fsxSOOrderRow);

            EnableDisableActions(cache, soOrderRow, fsxSOOrderRow, allowCreateServiceOrder, fsIntegrationEnabled);

            bool areReferencesVisible = allowCreateServiceOrder == false && fsIntegrationEnabled;

            PXUIFieldAttribute.SetVisible<FSxSOLine.relatedDocument>(Base.Transactions.Cache, null, areReferencesVisible);
        }
        #endregion

        #region Equipment Customization

        protected virtual void UpdateQty(PXCache cache, SOLine soLineRow)
        {
            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            bool updateQty = false;
            FSxSOLine fsxSOLineRow = cache.GetExtension<FSxSOLine>(soLineRow);

            if (fsxSOLineRow == null)
            {
                return;
            }

            updateQty = fsxSOLineRow.EquipmentAction == ID.Equipment_Action.REPLACING_TARGET_EQUIPMENT;

            updateQty = updateQty || ((fsxSOLineRow.EquipmentAction == ID.Equipment_Action.CREATING_COMPONENT || fsxSOLineRow.EquipmentAction == ID.Equipment_Action.REPLACING_COMPONENT)
                                            && ((fsxSOLineRow.SMEquipmentID != null && fsxSOLineRow.ComponentID != null)
                                                    || (fsxSOLineRow.NewEquipmentLineNbr != null && fsxSOLineRow.ComponentID != null)));
            if (updateQty)
            {
                cache.SetValueExt<SOLine.orderQty>(soLineRow, (decimal)1.0);
            }
        }

        #endregion

        public virtual DateTime? GetShipDate(FSServiceOrder serviceOrder, FSAppointment appointment)
        {
            return AppointmentEntry.GetShipDateInt(Base, serviceOrder, appointment);
        }
        #endregion

        #region Event Handlers

        #region SOOrder

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<SOOrder, FSxSOOrder.sDEnabled> e)
        {
            if (e.Row == null)
            {
                return;
            }

            SOOrder soOrderRow = (SOOrder)e.Row;
            FSxSOOrder fsxSOOrderRow = e.Cache.GetExtension<FSxSOOrder>(soOrderRow);

            if (IsFSIntegrationEnabled() == false)
            {
                fsxSOOrderRow.SDEnabled = false;
                return;
            }

            if (fsxSOOrderRow.SDEnabled == false)
            {
                FSServiceOrder fsServiceOrderRow = GetServiceOrderRecord(soOrderRow);
                fsxSOOrderRow.SrvOrdType = null;

                if (fsServiceOrderRow != null)
                {
                    foreach (SOLine soLineRow in Base.Transactions.Select())
                    {
                        if (IsLineSourceForAppoinmentLine(soOrderRow, soLineRow, fsServiceOrderRow))
                        {
                            throw new PXException(TX.Error.ERROR_DELETING_RELATED_SERVICE_ORDER + " " + TX.Error.SOME_SOLINES_HAVE_RELATED_APPOINTMENT_DETAILS, fsServiceOrderRow.RefNbr);
                        }
                    }
                }

                CleanSMFieldsBeforeSaving(soOrderRow);
            }
            else
            {
                if (fsxSOOrderRow.SrvOrdType == null)
                {
                    FSSetup fsSetupRow = PXSelect<FSSetup>.Select(Base);
                    if (fsSetupRow != null)
                    {
                        fsxSOOrderRow.SrvOrdType = fsSetupRow.DfltSOSrvOrdType;
                    }
                }
            }
        }

        #endregion

        protected virtual void _(Events.RowSelecting<SOOrder> e)
        {
        }

        protected virtual void _(Events.RowSelected<SOOrder> e)
        {
            if (e.Row == null)
            {
                return;
            }

            SOOrder soOrderRow = (SOOrder)e.Row;
            PXCache cache = e.Cache;

            bool fsIntegrationEnabled = IsFSIntegrationEnabled();
            SetExtensionVisibleInvisible<FSxSOOrder>(cache, e.Args, fsIntegrationEnabled, false);

            bool inventoryAndEquipmentModuleEnabled = PXAccess.FeatureInstalled<FeaturesSet.inventory>()
                                            && PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>();

            PXUIFieldAttribute.SetVisibility<FSxSOLine.comment>(Base.Transactions.Cache, null, inventoryAndEquipmentModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
            PXUIFieldAttribute.SetVisibility<FSxSOLine.equipmentAction>(Base.Transactions.Cache, null, inventoryAndEquipmentModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
            PXUIFieldAttribute.SetVisibility<FSxSOLine.newEquipmentLineNbr>(Base.Transactions.Cache, null, inventoryAndEquipmentModuleEnabled ? PXUIVisibility.Visible : PXUIVisibility.Invisible);

            EnableDisable_All(cache, soOrderRow, fsIntegrationEnabled);

            if (fsIntegrationEnabled == true)
            {
                CheckIfCurrentProjectBelongsToCustomer(cache, soOrderRow);
            }
        }

        protected virtual void _(Events.RowInserting<SOOrder> e)
        {
        }

        protected virtual void _(Events.RowInserted<SOOrder> e)
        {
        }

        protected virtual void _(Events.RowUpdating<SOOrder> e)
        {
        }

        protected virtual void _(Events.RowUpdated<SOOrder> e)
        {
        }

        protected virtual void _(Events.RowDeleting<SOOrder> e)
        {
        }

        protected virtual void _(Events.RowDeleted<SOOrder> e)
        {
        }

        protected virtual void _(Events.RowPersisting<SOOrder> e)
        {
            if (e.Row == null)
            {
                return;
            }

			if (IsFSIntegrationEnabled() == false)
			{
                return;
            }

            SOOrder soOrderRow = (SOOrder)e.Row;
            FSxSOOrder fsxSOOrderRow = e.Cache.GetExtension<FSxSOOrder>(soOrderRow);
            FSServiceOrder fsServiceOrderRow;

            if (e.Operation == PXDBOperation.Delete)
            {
                if (fsxSOOrderRow != null
                        && fsxSOOrderRow.ServiceOrderRefNbr != null)
                {
                    throw new PXException(TX.Error.CANNOT_DELETE_SALES_ORDER_BECAUSE_IT_HAS_A_SERVICE_ORDER);
                }
            }
            else
            {
                ValidatePostBatchStatus(e.Operation, ID.Batch_PostTo.SO, soOrderRow.OrderType, soOrderRow.RefNbr);
            }

            if (soOrderRow.Status != SOOrderStatus.Completed)
            {
                fsServiceOrderRow = InsertUpdateDeleteServiceOrderDocument(soOrderRow, fsxSOOrderRow, e.Operation);

                if (fsServiceOrderRow != null
                        && fsxSOOrderRow.ServiceOrderRefNbr != fsServiceOrderRow.RefNbr)
                {
                    Base.Document.Cache.SetValueExtIfDifferent<FSxSOOrder.serviceOrderRefNbr>(soOrderRow, fsServiceOrderRow.RefNbr);
                }
            }
        }

        protected virtual void _(Events.RowPersisted<SOOrder> e)
        {
            if (e.Row == null)
            {
                return;
            }

			SOOrder soOrderRow = (SOOrder)e.Row;
			PXCache cache = e.Cache;

			FSxSOOrder fsxSOOrderRow = cache.GetExtension<FSxSOOrder>(soOrderRow);

			if (IsFSIntegrationEnabled() == false)
			{
				if (e.TranStatus == PXTranStatus.Open && e.Operation == PXDBOperation.Delete)
				{
					UpdatePostingInformation(soOrderRow);
				}

				return;
            }

            if (e.TranStatus == PXTranStatus.Open)
            {
                if (e.Operation == PXDBOperation.Delete)
                {
					UpdatePostingInformation(soOrderRow);
				}

                if (!CanCreateServiceOrder(cache, soOrderRow, fsxSOOrderRow))
                {
					cache.RaiseExceptionHandling<FSxSOOrder.sDEnabled>(soOrderRow,
																	   fsxSOOrderRow.SDEnabled,
																	   new PXSetPropertyException(TX.Warning.CANT_CREATE_A_SERVICE_ORDER_FROM_AN_INVOICED_SALES_ORDER,
																								  PXErrorLevel.Warning));
                    return;
                }

                if (fsxSOOrderRow.SDEnabled == true
                        && fsxSOOrderRow.SrvOrdType != null
                            && fsxSOOrderRow.ServiceOrderRefNbr != null)
                {
                    PXUpdate<
                        Set<FSServiceOrder.sourceDocType, Required<FSServiceOrder.sourceDocType>,
                        Set<FSServiceOrder.sourceRefNbr, Required<FSServiceOrder.sourceRefNbr>>>,
                    FSServiceOrder,
                    Where<
                        FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>,
                        And<FSServiceOrder.refNbr, Equal<Required<FSServiceOrder.refNbr>>>>>
                    .Update(cache.Graph, soOrderRow.OrderType, soOrderRow.OrderNbr, fsxSOOrderRow.SrvOrdType, fsxSOOrderRow.ServiceOrderRefNbr);
                }
            }

            if (e.TranStatus == PXTranStatus.Aborted)
            {
                Base.Document.Cache.SetValueExtIfDifferent<FSxSOOrder.serviceOrderRefNbr>(soOrderRow, null);
            }
        }

        #endregion

        #region SOLine

        #region FieldSelecting
        #endregion
        #region FieldDefaulting

        protected virtual void _(Events.FieldDefaulting<SOLine, FSxSOLine.sDSelected> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLine = (SOLine)e.Row;
            FSxSOOrder fsxSOOrderRow = Base.Document.Cache.GetExtension<FSxSOOrder>(Base.Document.Current);

            e.NewValue = (bool)fsxSOOrderRow.SDEnabled && fsxSOOrderRow.ServiceOrderRefNbr != null;
        }

        #endregion
        #region FieldVerifying

        protected virtual void _(Events.FieldVerifying<SOLine, SOLine.orderQty> e)
        {
            if (e.Row == null || IsFSIntegrationEnabled() == false || e.NewValue == null)
            {
                return;
            }

            decimal newValue = Convert.ToDecimal(e.NewValue);
            if (e.Row.OrderQty != newValue && IsLineCreatedFromAppSO(Base, Base.Document.Current, e.Row, typeof(SOLine.orderQty).Name) == true)
            {
                throw new PXSetPropertyException(TX.Error.NO_UPDATE_ALLOWED_DOCLINE_LINKED_TO_APP_SO);
            }

			if (e.Row.IsStockItem == true)
            {
				FSxSOLine fsxSOLineRow = e.Cache.GetExtension<FSxSOLine>(e.Row);
				if (fsxSOLineRow.EquipmentAction != null && fsxSOLineRow.EquipmentAction != ID.Equipment_Action.NONE)
				{
					bool haveRemainder = decimal.Remainder((decimal)e.NewValue, 1m) != 0m;
					if (haveRemainder)
						throw new PXSetPropertyException(TX.Error.DecimalNumberCannotBeEntered, PXErrorLevel.Error);
				}
            }
        }

        protected virtual void _(Events.FieldVerifying<SOLine, SOLine.uOM> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;

            if (IsLineCreatedFromAppSO(Base, Base.Document.Current, soLineRow, typeof(SOLine.uOM).Name) == true)
            {
                throw new PXSetPropertyException(TX.Error.NO_UPDATE_ALLOWED_DOCLINE_LINKED_TO_APP_SO);
            }
        }

        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<SOLine, SOLine.inventoryID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLine = (SOLine)e.Row;

            bool updateQty = SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.SALES_ORDER) == Base.Accessinfo.ScreenID;
            SharedFunctions.UpdateEquipmentFields(Base, e.Cache, soLine, soLine.InventoryID, updateQty: updateQty);
        }

        protected virtual void _(Events.FieldUpdated<SOLine, FSxSOLine.equipmentAction> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLine = (SOLine)e.Row;
            PXCache cache = e.Cache;

            if (e.ExternalCall == true)
            {
                SharedFunctions.ResetEquipmentFields(cache, soLine);
                SharedFunctions.SetEquipmentFieldEnablePersistingCheck(cache, soLine);
                UpdateQty(cache, soLine);
            }
        }

        protected virtual void _(Events.FieldUpdated<SOLine, FSxSOLine.sMEquipmentID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            UpdateQty(e.Cache, soLineRow);
        }

        protected virtual void _(Events.FieldUpdated<SOLine, FSxSOLine.componentID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            UpdateQty(e.Cache, soLineRow);
        }

        protected virtual void _(Events.FieldUpdated<SOLine, FSxSOLine.equipmentComponentLineNbr> e)
        {
            if (e.Row == null)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            FSxSOLine fsxSOLineRow = e.Cache.GetExtension<FSxSOLine>(soLineRow);

            if (fsxSOLineRow.ComponentID == null)
            {
                fsxSOLineRow.ComponentID = SharedFunctions.GetEquipmentComponentID(Base, fsxSOLineRow.SMEquipmentID, fsxSOLineRow.EquipmentComponentLineNbr);
            }
        }

        #endregion

        protected virtual void _(Events.RowSelected<SOLine> e)
        {
            bool fsIntegrationEnabled = IsFSIntegrationEnabled();
            PXCache cache = e.Cache;

            SetExtensionVisibleInvisible<FSxSOLine>(cache, e.Args, fsIntegrationEnabled, true);

            if (e.Row == null)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            EnableDisableSOline(cache, soLineRow, fsIntegrationEnabled);

            if (fsIntegrationEnabled == true)
            {
                bool updateQty = SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.SALES_ORDER) == Base.Accessinfo.ScreenID;
                SharedFunctions.SetEquipmentFieldEnablePersistingCheck(cache, soLineRow, updateQty);
            }
        }

        protected virtual void _(Events.RowInserting<SOLine> e)
        {
        }

        protected virtual void _(Events.RowInserted<SOLine> e)
        {
        }

        protected virtual void _(Events.RowUpdating<SOLine> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            PXCache cache = e.Cache;

            FSxSOLine fsxSOLineRow = cache.GetExtension<FSxSOLine>(soLineRow);

            SOLine newSOLineRow = (SOLine)e.NewRow;
            FSxSOLine newfsxSOLineRow = cache.GetExtension<FSxSOLine>(newSOLineRow);

            if (e.ExternalCall == true)
            {
                if (ArePrepaidFieldsBeingModified(soLineRow, newSOLineRow))
                {
                    if (IsLineSourceForAppoinmentLine(Base.Document.Current, soLineRow, null))
                    {
                        e.Cancel = true;
                        throw new PXException(TX.Error.SOLINE_HAS_RELATED_APPOINTMENT_DETAILS);
                    }
                }

                if ((soLineRow.InventoryID != newSOLineRow.InventoryID
                        || soLineRow.SubItemID != newSOLineRow.SubItemID)
                            && IsLineCreatedFromAppSO(Base, Base.Document.Current, soLineRow, null) == true)
                {
                    throw new PXException(TX.Error.NO_UPDATE_ALLOWED_DOCLINE_LINKED_TO_APP_SO);
                }
            }
        }

        protected virtual void _(Events.RowUpdated<SOLine> e)
        {
        }

        protected virtual void _(Events.RowDeleting<SOLine> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;

            if (IsLineSourceForAppoinmentLine(Base.Document.Current, soLineRow, null))
            {
                throw new PXException(TX.Error.SOLINE_HAS_RELATED_APPOINTMENT_DETAILS);
            }

            if (e.ExternalCall == true)
            {
                if (IsLineCreatedFromAppSO(Base, Base.Document.Current, soLineRow, null) == true)
                {
                    throw new PXException(TX.Error.NO_DELETION_ALLOWED_DOCLINE_LINKED_TO_APP_SO);
                }
            }
        }

        protected virtual void _(Events.RowDeleted<SOLine> e)
        {
        }

        protected virtual void _(Events.RowPersisting<SOLine> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;
            PXCache cache = e.Cache;

            FSxSOLine fsxSOLineRow = cache.GetExtension<FSxSOLine>(soLineRow);
            string errorMessage = string.Empty;

            if (e.Operation != PXDBOperation.Delete
                    && !SharedFunctions.AreEquipmentFieldsValid(cache, soLineRow.InventoryID, fsxSOLineRow.SMEquipmentID, fsxSOLineRow.NewEquipmentLineNbr, fsxSOLineRow.EquipmentAction, ref errorMessage))
            {
                cache.RaiseExceptionHandling<FSxSOLine.equipmentAction>(soLineRow, fsxSOLineRow.EquipmentAction, new PXSetPropertyException(errorMessage));
            }
        }

        protected virtual void _(Events.RowPersisted<SOLine> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsFSIntegrationEnabled() == false)
            {
                return;
            }

            SOLine soLineRow = (SOLine)e.Row;

            // We call here cache.GetStateExt for every field when the transaction is aborted
            // to set the errors in the fields and then the Generate Invoice screen can read them
            if (e.TranStatus == PXTranStatus.Aborted && IsInvoiceProcessRunning == true)
            {
                MessageHelper.GetRowMessage(e.Cache, soLineRow, false, false);
            }
        }

        #endregion

        #endregion

        #region Invoicing Methods
        public override List<ErrorInfo> GetErrorInfo()
        {
            return MessageHelper.GetErrorInfo<SOLine>(Base.Document.Cache, Base.Document.Current, Base.Transactions, typeof(SOLine));
        }

        public override void CreateInvoice(PXGraph graphProcess, List<DocLineExt> docLines, short invtMult, DateTime? invoiceDate, string invoiceFinPeriodID, OnDocumentHeaderInsertedDelegate onDocumentHeaderInserted, OnTransactionInsertedDelegate onTransactionInserted, PXQuickProcess.ActionFlow quickProcessFlow)
        {
            if (docLines.Count == 0)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = docLines[0].fsServiceOrder;
            FSSrvOrdType fsSrvOrdTypeRow = docLines[0].fsSrvOrdType;
            FSPostDoc fsPostDocRow = docLines[0].fsPostDoc;
            FSAppointment fsAppointmentRow = docLines[0].fsAppointment;

            bool? initialHold = false;

            //// This event is raised so the Sales Order page can be opened with the branch indicated in the Appointment
            //// WARNING: Assigning the BranchID directly, behaves incorrectly
            //// Note: The AddHandler method is run before the corresponding method defined in the original graph
            var cancel_defaulting = new PXFieldDefaulting((sender, e) =>
            {
                e.NewValue = fsServiceOrderRow.BranchID;
                e.Cancel = true;
            });

            try
            {
            	Base.FieldDefaulting.AddHandler<SOOrder.branchID>(cancel_defaulting);
                
				SOOrder sOOrderRow = new SOOrder();

				if (invtMult >= 0)
				{
					if (string.IsNullOrEmpty(fsPostDocRow.PostOrderType))
					{
						throw new PXException(TX.Error.POST_ORDER_TYPE_MISSING_IN_SETUP);
					}

					sOOrderRow.OrderType = fsPostDocRow.PostOrderType;
				}
				else
				{
					if (string.IsNullOrEmpty(fsPostDocRow.PostOrderTypeNegativeBalance))
					{
						throw new PXException(TX.Error.POST_ORDER_NEGATIVE_BALANCE_TYPE_MISSING_IN_SETUP);
					}

					sOOrderRow.OrderType = fsPostDocRow.PostOrderTypeNegativeBalance;
				}

				sOOrderRow.InclCustOpenOrders = true;
				sOOrderRow.CustomerOrderNbr = fsServiceOrderRow.CustPORefNbr;

				CheckAutoNumbering(Base.soordertype.SelectSingle(sOOrderRow.OrderType).OrderNumberingID);
				sOOrderRow = Base.Document.Current = Base.Document.Insert(sOOrderRow);

				initialHold = sOOrderRow.Hold;
				Base.Document.Cache.SetValueExtIfDifferent<SOOrder.hold>(sOOrderRow, true);
				Base.Document.Cache.SetValueExtIfDifferent<SOOrder.orderDate>(sOOrderRow, invoiceDate);

				// TODO: What to do if there are different dates between the different documents?
				DateTime? requestDate = fsServiceOrderRow.OrderDate;

				// TODO: AC-169637 - Uncomment this line and delete the previous one.
				//DateTime? requestDate = GetShipDate(fsServiceOrderRow, fsAppointmentRow);

				Base.Document.Cache.SetValueExtIfDifferent<SOOrder.requestDate>(sOOrderRow, requestDate);

				Base.Document.Cache.SetValueExtIfDifferent<SOOrder.customerID>(sOOrderRow, fsServiceOrderRow.BillCustomerID);
				Base.Document.Cache.SetValueExtIfDifferent<SOOrder.customerLocationID>(sOOrderRow, fsServiceOrderRow.BillLocationID);
				Base.Document.Cache.SetValueExtIfDifferent<SOOrder.curyID>(sOOrderRow, fsServiceOrderRow.CuryID);

				string docTaxZoneID = fsAppointmentRow != null ? fsAppointmentRow.TaxZoneID : fsServiceOrderRow.TaxZoneID;
				if (sOOrderRow.TaxZoneID != docTaxZoneID)
				{
					Base.Document.Cache.SetValueExtIfDifferent<SOOrder.overrideTaxZone>(sOOrderRow, true);
					Base.Document.Cache.SetValueExtIfDifferent<SOOrder.taxZoneID>(sOOrderRow, docTaxZoneID);
				}

				Base.Document.Cache.SetValueExtIfDifferent<SOOrder.taxCalcMode>(sOOrderRow, fsAppointmentRow != null ? fsAppointmentRow.TaxCalcMode : fsServiceOrderRow.TaxCalcMode);

				Base.Document.Cache.SetValueExtIfDifferent<SOOrder.orderDesc>(sOOrderRow, fsServiceOrderRow.DocDesc);

				if (fsServiceOrderRow.ProjectID != null)
				{
					Base.Document.Cache.SetValueExtIfDifferent<SOOrder.projectID>(sOOrderRow, fsServiceOrderRow.ProjectID);
				}

				string termsID = GetTermsIDFromCustomerOrVendor(Base, fsServiceOrderRow.BillCustomerID, null);
				if (termsID != null)
				{
					Base.Document.Cache.SetValueExtIfDifferent<SOOrder.termsID>(sOOrderRow, termsID);
				}
				else
				{
					Base.Document.Cache.SetValueExtIfDifferent<SOOrder.termsID>(sOOrderRow, fsSrvOrdTypeRow.DfltTermIDARSO);
				}

				Base.Document.Cache.SetValueExtIfDifferent<SOOrder.ownerID>(sOOrderRow, null);

			Base.Document.Cache.SetValueExtIfDifferent<SOOrder.salesPersonID>(sOOrderRow, fsServiceOrderRow?.SalesPersonID);

				sOOrderRow = Base.Document.Update(sOOrderRow);

				SetContactAndAddress(Base, fsServiceOrderRow);

				if (onDocumentHeaderInserted != null)
				{
					onDocumentHeaderInserted(Base, sOOrderRow);
				}

				IDocLine docLine = null;
				FSSODet soDet = null;
				FSAppointmentDet appointmentDet = null;
				SOLine sOLineRow = null;
				FSxSOLine fSxSOLineRow = null;
				PMTask pmTaskRow = null;
				FSAppointmentDet fsAppointmentDetRow = null;
				FSSODet fsSODetRow = null;
				List<SharedClasses.SOARLineEquipmentComponent> componentList = new List<SharedClasses.SOARLineEquipmentComponent>();

				foreach (DocLineExt line in docLines)
				{
					// Marker of a document (Appointment or Service Order) beginning
					// Supposed that records in docLines are sorted by document
					bool nextDoc = fsAppointmentRow == null
									? (line.fsSODet == docLines[0].fsSODet || fsServiceOrderRow.RefNbr != line.fsServiceOrder.RefNbr)
									: (line.fsAppointmentDet == docLines[0].fsAppointmentDet || fsAppointmentRow.RefNbr != line.fsAppointment.RefNbr);

					docLine = line.docLine;
					soDet = line.fsSODet;
					appointmentDet = line.fsAppointmentDet;
					fsPostDocRow = line.fsPostDoc;
					fsServiceOrderRow = line.fsServiceOrder;
					fsSrvOrdTypeRow = line.fsSrvOrdType;
					fsAppointmentRow = line.fsAppointment;
					pmTaskRow = line.pmTask;
					fsAppointmentDetRow = line.fsAppointmentDet;

					if (pmTaskRow != null && pmTaskRow.Status == ProjectTaskStatus.Completed)
					{
						throw new PXException(TX.Error.POSTING_PMTASK_ALREADY_COMPLETED, fsServiceOrderRow.RefNbr, GetLineDisplayHint(Base, docLine.LineRef, docLine.TranDesc, docLine.InventoryID), pmTaskRow.TaskCD);
					}

					sOLineRow = new SOLine();
					sOLineRow = Base.Transactions.Current = Base.Transactions.Insert((SOLine)sOLineRow);
					sOLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);

					sOLineRow.OrderQty = 0;

					sOLineRow.BranchID = docLine.BranchID;
					sOLineRow.InventoryID = docLine.InventoryID;

					if (sOLineRow.LineType == SOLineType.Inventory && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
					{
						sOLineRow.SubItemID = docLine.SubItemID;
					}

					sOLineRow.SiteID = docLine.SiteID;
					if (docLine.SiteLocationID != null)
					{
						sOLineRow.LocationID = docLine.SiteLocationID;
					}

					sOLineRow.UOM = docLine.UOM;

					if (docLine.ProjectID != null && docLine.ProjectTaskID != null)
					{
						sOLineRow.TaskID = docLine.ProjectTaskID;
					}

					sOLineRow.SalesAcctID = docLine.AcctID;
					sOLineRow.TaxCategoryID = docLine.TaxCategoryID;
					sOLineRow.TranDesc = docLine.TranDesc;
					sOLineRow = Base.Transactions.Update(sOLineRow);

					bool isLotSerialRequired = SharedFunctions.IsLotSerialRequired(Base.Transactions.Cache, sOLineRow.InventoryID);
					bool validateLotSerQty = false;

					if (isLotSerialRequired == true && fsAppointmentRow == null) // is posted by Service Order
					{
						foreach (FSSODetSplit split in PXSelect<FSSODetSplit,
													   Where<
															FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
															And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
															And<FSSODetSplit.lineNbr, Equal<Required<FSSODetSplit.lineNbr>>>>>,
													   OrderBy<Asc<FSSODetSplit.splitLineNbr>>>
													   .Select(Base, soDet.SrvOrdType, soDet.RefNbr, soDet.LineNbr))
						{
							if (split.POCreate == false && string.IsNullOrEmpty(split.LotSerialNbr) == false)
							{
								SOLineSplit newSplit = new SOLineSplit();
								newSplit = (SOLineSplit)Base.splits.Cache.CreateCopy(Base.splits.Insert(newSplit));

								newSplit.SiteID = split.SiteID != null ? split.SiteID : newSplit.SiteID;
                            newSplit.LocationID = sOOrderRow.OrderType != SOOrderTypeConstants.SalesOrder ? (split.LocationID != null ? split.LocationID : newSplit.LocationID) : null;
								newSplit.LotSerialNbr = split.LotSerialNbr;
								newSplit.Qty = split.Qty;

								newSplit = Base.splits.Update(newSplit);
								validateLotSerQty = true;
							}
						}

						sOLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);
					}
					else if(isLotSerialRequired == true && fsAppointmentRow != null)
					{
						foreach (FSApptLineSplit split in PXSelect<FSApptLineSplit,
														   Where<
																FSApptLineSplit.srvOrdType, Equal<Required<FSApptLineSplit.srvOrdType>>,
																And<FSApptLineSplit.apptNbr, Equal<Required<FSApptLineSplit.apptNbr>>,
																And<FSApptLineSplit.lineNbr, Equal<Required<FSApptLineSplit.lineNbr>>>>>,
														   OrderBy<Asc<FSApptLineSplit.splitLineNbr>>>
														   .Select(Base, appointmentDet.SrvOrdType, appointmentDet.RefNbr, appointmentDet.LineNbr))
						{
							if (string.IsNullOrEmpty(split.LotSerialNbr) == false)
							{
								SOLineSplit newSplit = new SOLineSplit();
								newSplit = (SOLineSplit)Base.splits.Cache.CreateCopy(Base.splits.Insert(newSplit));

								newSplit.SiteID = split.SiteID != null ? split.SiteID : newSplit.SiteID;
                            newSplit.LocationID = sOOrderRow.OrderType != SOOrderTypeConstants.SalesOrder ? (split.LocationID != null  ? split.LocationID : newSplit.LocationID) : null;
								newSplit.LotSerialNbr = split.LotSerialNbr;
								newSplit.Qty = split.Qty;

								newSplit = Base.splits.Update(newSplit);
								validateLotSerQty = true;
							}
						}

						sOLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);
					}

					if (validateLotSerQty == false)
					{
						sOLineRow.OrderQty = docLine.GetQty(FieldType.BillableField);
					}
					else if (sOLineRow.OrderQty != docLine.GetQty(FieldType.BillableField))
					{
						throw new PXException(TX.Error.QTY_POSTED_ERROR);
					}

					sOLineRow.IsFree = docLine.IsFree;
					sOLineRow.ManualPrice = docLine.ManualPrice;
					sOLineRow.CuryUnitPrice = docLine.CuryUnitPrice * invtMult;

					sOLineRow = Base.Transactions.Update(sOLineRow);
					sOLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);

                sOLineRow.SalesPersonID = fsServiceOrderRow?.SalesPersonID;

					bool manualDisc = false;
					if (appointmentDet != null)
					{
						manualDisc = (bool)appointmentDet.ManualDisc;
					}
					else if (soDet != null)
					{
						manualDisc = (bool)soDet.ManualDisc;
					}

					sOLineRow.ManualDisc = manualDisc;
					if (sOLineRow.ManualDisc == true)
					{
					sOLineRow.DiscPct = docLine.DiscPct;
					}

					sOLineRow.CuryExtPrice = docLine.CuryBillableExtPrice * invtMult;
					sOLineRow.CuryExtPrice = CM.PXCurrencyAttribute.Round(Base.Transactions.Cache, sOLineRow, sOLineRow.CuryExtPrice ?? 0m, CM.CMPrecision.TRANCURY);

					sOLineRow = Base.Transactions.Update(sOLineRow);

					if (docLine.SubID != null)
					{
						try
						{
							Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.salesSubID>(sOLineRow, docLine.SubID);
						}
						catch (PXException)
						{
							sOLineRow.SalesSubID = null;
							sOLineRow = Base.Transactions.Update(sOLineRow);
						}
					}
					else
					{
						SetCombinedSubID(Base,
										Base.Transactions.Cache,
										null,
										null,
										sOLineRow,
										fsSrvOrdTypeRow,
										sOLineRow.BranchID,
										sOLineRow.InventoryID,
										fsServiceOrderRow.BillLocationID,
										fsServiceOrderRow.BranchLocationID,
										fsServiceOrderRow.SalesPersonID,
										docLine.IsService);

						sOLineRow = Base.Transactions.Update(sOLineRow);
					}

					// TODO: Add TaxCategoryID in Service Contract line definition
					//Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.taxCategoryID>(sOLineRow, docLine.TaxCategoryID);
                Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.commissionable>(sOLineRow, fsServiceOrderRow?.Commissionable);

					Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.costCodeID>(sOLineRow, docLine.CostCodeID);

					//Set the line as a posted
					fSxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(sOLineRow);

					fSxSOLineRow.SDPosted = true;

					fSxSOLineRow.SrvOrdType = fsServiceOrderRow.SrvOrdType;
					fSxSOLineRow.ServiceOrderRefNbr = fsServiceOrderRow.RefNbr;
					fSxSOLineRow.AppointmentRefNbr = fsAppointmentRow?.RefNbr;
					fSxSOLineRow.ServiceOrderLineNbr = soDet?.LineNbr;
					fSxSOLineRow.AppointmentLineNbr = fsAppointmentDetRow?.LineNbr;

					if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
					{
						if (docLine.EquipmentAction != null)
						{
							Base.Transactions.Cache.SetValueExtIfDifferent<FSxSOLine.equipmentAction>(sOLineRow, docLine.EquipmentAction);
							Base.Transactions.Cache.SetValueExtIfDifferent<FSxSOLine.sMEquipmentID>(sOLineRow, docLine.SMEquipmentID);
							Base.Transactions.Cache.SetValueExtIfDifferent<FSxSOLine.equipmentComponentLineNbr>(sOLineRow, docLine.EquipmentLineRef);

							fSxSOLineRow.Comment = docLine.Comment;

							if (docLine.EquipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT
								|| ((docLine.EquipmentAction == ID.Equipment_Action.CREATING_COMPONENT
									 || docLine.EquipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
									 || docLine.EquipmentAction == ID.Equipment_Action.NONE)
										&& string.IsNullOrEmpty(docLine.NewTargetEquipmentLineNbr) == false))
							{
								componentList.Add(new SharedClasses.SOARLineEquipmentComponent(docLine, sOLineRow, fSxSOLineRow));
							}
							else
							{
								fSxSOLineRow.ComponentID = docLine.ComponentID;
							}
						}
					}

					if (nextDoc)
					{
						if (fsAppointmentRow == null)
							SharedFunctions.CopyNotesAndFiles(
								Base.Caches[typeof(FSServiceOrder)],
								Base.Document.Cache,
								fsServiceOrderRow,
								sOOrderRow,
								fsSrvOrdTypeRow.CopyNotesToInvoice,
								fsSrvOrdTypeRow.CopyAttachmentsToInvoice);
						else
							SharedFunctions.CopyNotesAndFiles(
								Base.Caches[typeof(FSAppointment)],
								Base.Document.Cache,
								fsAppointmentRow,
								sOOrderRow,
								fsSrvOrdTypeRow.CopyNotesToInvoice,
								fsSrvOrdTypeRow.CopyAttachmentsToInvoice);
					}

					SharedFunctions.CopyNotesAndFiles(Base.Transactions.Cache, sOLineRow, docLine, fsSrvOrdTypeRow);
					fsPostDocRow.DocLineRef = sOLineRow = Base.Transactions.Update(sOLineRow);

					if (onTransactionInserted != null)
					{
						onTransactionInserted(Base, sOLineRow);
					}
				}

				if(componentList.Count > 0)
				{ 
					//Assigning the NewTargetEquipmentLineNbr field value for the component type records
					foreach (SharedClasses.SOARLineEquipmentComponent currLineModel in componentList.Where(x => x.equipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT))
					{
						foreach (SharedClasses.SOARLineEquipmentComponent currLineComponent in componentList.Where(x => (x.equipmentAction == ID.Equipment_Action.CREATING_COMPONENT
																														|| x.equipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
																														|| x.equipmentAction == ID.Equipment_Action.NONE)))
						{
							if (currLineComponent.sourceNewTargetEquipmentLineNbr == currLineModel.sourceLineRef)
							{
								currLineComponent.fsxSOLineRow.ComponentID = currLineComponent.componentID;
								currLineComponent.fsxSOLineRow.NewEquipmentLineNbr = currLineModel.currentLineRef;
							}
						}
					}
				}

				if (Base.soordertype.Current.RequireControlTotal == true)
				{
					Base.Document.Cache.SetValueExtIfDifferent<SOOrder.curyControlTotal>(sOOrderRow, sOOrderRow.CuryOrderTotal);
				}

				if (initialHold != true || quickProcessFlow != PXQuickProcess.ActionFlow.NoFlow)
				{
					Base.Document.Cache.SetValueExtIfDifferent<SOOrder.hold>(sOOrderRow, false);
				}

				sOOrderRow = Base.Document.Update(sOOrderRow);
			}
            finally 
            {
                Base.FieldDefaulting.RemoveHandler<SOOrder.branchID>(cancel_defaulting);
            }
        }

        public override FSCreatedDoc PressSave(int batchID, List<DocLineExt> docLines, BeforeSaveDelegate beforeSave)
        {
            if (Base.Document.Current == null)
            {
                throw new SharedClasses.TransactionScopeException();
            }

            if (beforeSave != null)
            {
                beforeSave(Base);
            }

            Base.SelectTimeStamp();

            SOOrderEntryExternalTax TaxGraphExt = Base.GetExtension<SOOrderEntryExternalTax>();

            if (TaxGraphExt != null)
            {
                TaxGraphExt.SkipTaxCalcAndSave();
            }
            else
            {
                Base.Save.Press();
            }

            string orderType = Base.Document.Current.OrderType;
            string orderNbr = Base.Document.Current.OrderNbr;

            // Reload SOOrder to get the current value of IsTaxValid
            Base.Clear();
            Base.Document.Current = PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>, And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(Base, orderType, orderNbr);
            SOOrder soOrderRow = Base.Document.Current;

            var fsCreatedDocRow = new FSCreatedDoc()
            {
                BatchID = batchID,
                PostTo = ID.Batch_PostTo.SO,
                CreatedDocType = soOrderRow.OrderType,
                CreatedRefNbr = soOrderRow.OrderNbr
            };

            return fsCreatedDocRow;
        }

        public override void Clear()
        {
            Base.Clear(PXClearOption.ClearAll);
        }

        public override PXGraph GetGraph()
        {
            return Base;
        }

        public override void DeleteDocument(FSCreatedDoc fsCreatedDocRow)
        {
            Base.Document.Current = Base.Document.Search<SOOrder.orderNbr>(fsCreatedDocRow.CreatedRefNbr, fsCreatedDocRow.CreatedDocType);

            if (Base.Document.Current != null)
            {
                if (Base.Document.Current.OrderNbr == fsCreatedDocRow.CreatedRefNbr
                        && Base.Document.Current.OrderType == fsCreatedDocRow.CreatedDocType)
                {
                    Base.Delete.Press();
                }
            }
        }

        public override void CleanPostInfo(PXGraph cleanerGraph, FSPostDet fsPostDetRow)
        {
            PXUpdate<
                Set<FSPostInfo.sOLineNbr, Null,
                Set<FSPostInfo.sOOrderNbr, Null,
                Set<FSPostInfo.sOOrderType, Null,
                Set<FSPostInfo.sOPosted, False>>>>,
            FSPostInfo,
            Where<
                FSPostInfo.postID, Equal<Required<FSPostInfo.postID>>,
                And<FSPostInfo.sOPosted, Equal<True>>>>
            .Update(cleanerGraph, fsPostDetRow.PostID);
        }

        public virtual bool IsLineCreatedFromAppSO(PXGraph cleanerGraph, object document, object lineDoc, string fieldName)
        {
            if (document == null || lineDoc == null
                || Base.Accessinfo.ScreenID.Replace(".", "") == ID.ScreenID.SERVICE_ORDER
                || Base.Accessinfo.ScreenID.Replace(".", "") == ID.ScreenID.APPOINTMENT
                || Base.Accessinfo.ScreenID.Replace(".", "") == ID.ScreenID.INVOICE_BY_SERVICE_ORDER
                || Base.Accessinfo.ScreenID.Replace(".", "") == ID.ScreenID.INVOICE_BY_APPOINTMENT)
            {
                return false;
            }

            string refNbr = ((SOOrder)document).RefNbr;
            string docType = ((SOOrder)document).OrderType;
            int? lineNbr = ((SOLine)lineDoc).LineNbr;

            return PXSelect<FSPostInfo, 
                   Where<
                       FSPostInfo.sOOrderNbr, Equal<Required<FSPostInfo.sOOrderNbr>>,
                       And<FSPostInfo.sOOrderType, Equal<Required<FSPostInfo.sOOrderType>>,
                       And<FSPostInfo.sOLineNbr, Equal<Required<FSPostInfo.sOLineNbr>>,
                       And<FSPostInfo.sOPosted, Equal<True>>>>>>
                   .Select(cleanerGraph, refNbr, docType, lineNbr).Count() > 0;
        }

        #region Invoice By Contract Period Methods 
        public virtual FSContractPostDoc CreateInvoiceByContract(PXGraph graphProcess, DateTime? invoiceDate, string invoiceFinPeriodID, FSContractPostBatch fsContractPostBatchRow, FSServiceContract fsServiceContractRow, FSContractPeriod fsContractPeriodRow, List<ContractInvoiceLine> docLines)
        {
            if (docLines.Count == 0)
            {
                return null;
            }

            FSSetup fsSetupRow = ServiceManagementSetup.GetServiceManagementSetup(graphProcess);

            //// This event is raised so the Sales Order page can be opened with the branch indicated in the Appointment
            //// WARNING: Assigning the BranchID directly, behaves incorrectly
            //// Note: The AddHandler method is run before the corresponding method defined in the original graph
            var cancel_defaulting = new PXFieldDefaulting((sender, e) =>
            {
                e.NewValue = fsServiceContractRow.BranchID;
                e.Cancel = true;
            });

            SOOrder sOOrderRow = null;

            try
            {
                Base.FieldDefaulting.AddHandler<SOOrder.branchID>(cancel_defaulting);

            FSBranchLocation fSBranchLocationRow = FSBranchLocation.PK.Find(graphProcess, fsServiceContractRow.BranchLocationID);

            Base.Document.Cache.Clear();
            Base.Document.Cache.ClearQueryCacheObsolete();
            Base.Document.View.Clear();

                sOOrderRow = new SOOrder();

            sOOrderRow.OrderType = fsSetupRow.ContractPostOrderType;

            sOOrderRow.InclCustOpenOrders = true;
            sOOrderRow.Hold = true;

            CheckAutoNumbering(Base.soordertype.SelectSingle(sOOrderRow.OrderType).OrderNumberingID);
            sOOrderRow = Base.Document.Current = Base.Document.Insert(sOOrderRow);

            Base.Document.Cache.SetValueExtIfDifferent<SOOrder.orderDate>(sOOrderRow, invoiceDate);
            Base.Document.Cache.SetValueExtIfDifferent<SOOrder.requestDate>(sOOrderRow, invoiceDate);
            Base.Document.Cache.SetValueExtIfDifferent<SOOrder.customerID>(sOOrderRow, fsServiceContractRow.BillCustomerID);
            Base.Document.Cache.SetValueExtIfDifferent<SOOrder.customerLocationID>(sOOrderRow, fsServiceContractRow.BillLocationID);

            string prefixDesc = ServiceContractBillingType.ListAttribute.GetLocalizedLabel<FSServiceContract.billingType>(graphProcess.Caches[typeof(FSServiceContract)], fsServiceContractRow);

            Base.Document.Cache.SetValueExtIfDifferent<SOOrder.orderDesc>(sOOrderRow, 
                (PXMessages.LocalizeFormatNoPrefix(TX.Messages.ContractDesctInvoicePrefix, prefixDesc, fsServiceContractRow.RefNbr, (string.IsNullOrEmpty(fsServiceContractRow.DocDesc) ? string.Empty : fsServiceContractRow.DocDesc))));

            string termsID = GetTermsIDFromCustomerOrVendor(Base, fsServiceContractRow.BillCustomerID, null);

            if (termsID != null)
            {
                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.termsID>(sOOrderRow, termsID);
            }
            else
            {
                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.termsID>(sOOrderRow, fsSetupRow.DfltContractTermIDARSO);
            }

            Base.Document.Cache.SetValueExtIfDifferent<SOOrder.ownerID>(sOOrderRow, null);

            Base.Document.Cache.SetValueExtIfDifferent<SOOrder.projectID>(sOOrderRow, fsServiceContractRow.ProjectID);

            sOOrderRow = Base.Document.Update(sOOrderRow);

            Base.Transactions.Cache.Clear();
            Base.Transactions.Cache.ClearQueryCacheObsolete();
            Base.Transactions.View.Clear();

            SOLine sOLineRow = null;
            FSxSOLine fSxSOLineRow = null;
            FSSODet soDet = null;
            FSAppointmentDet apptDet = null;
            int? acctID;
            List<SharedClasses.SOARLineEquipmentComponent> componentList = new List<SharedClasses.SOARLineEquipmentComponent>();

            foreach (ContractInvoiceLine docLine in docLines)
            {
                sOLineRow = new SOLine();
                sOLineRow = Base.Transactions.Current = Base.Transactions.Insert(sOLineRow);
                soDet = docLine.fsSODet;
                apptDet = docLine.fsAppointmentDet;

                Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.inventoryID>(sOLineRow, docLine.InventoryID);

                if (sOLineRow.SubItemID == null && docLine.SubItemID != null 
                        && sOLineRow.LineType == SOLineType.Inventory && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.subItemID>(sOLineRow, docLine.SubItemID);
                }

                if (docLine.SiteID != null)
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.siteID>(sOLineRow, docLine.SiteID);
                }

                if (docLine.SiteLocationID != null)
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.locationID>(sOLineRow, docLine.SiteLocationID);
                }

                Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.uOM>(sOLineRow, docLine.UOM);

                Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.salesPersonID>(sOLineRow, docLine.SalesPersonID);

                sOLineRow = Base.Transactions.Update(sOLineRow);

                if (docLine.AcctID != null)
                {
                    acctID = docLine.AcctID;
                }
                else
                {
                    acctID = (int?)Get_INItemAcctID_DefaultValue(graphProcess,
                                                                    fsSetupRow.ContractSalesAcctSource,
                                                                    docLine.InventoryID,
                                                                    fsServiceContractRow?.CustomerID,
                                                                    fsServiceContractRow?.CustomerLocationID);
                }

                Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.salesAcctID>(sOLineRow, acctID);

                if (docLine.SubID != null)
                {
                    try
                    {
                        Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.salesSubID>(sOLineRow, docLine.SubID);
                    }
                    catch (PXException)
                    {
                        sOLineRow.SalesSubID = null;
                    }
                }
                else
                {
                    SetCombinedSubID(Base,
                                    Base.Transactions.Cache,
                                    null,
                                    null,
                                    sOLineRow,
                                    fsSetupRow,
                                    sOLineRow.BranchID,
                                    sOLineRow.InventoryID,
                                    fsServiceContractRow.BillLocationID,
                                    fsServiceContractRow.BranchLocationID);
                }

                bool isLotSerialRequired = SharedFunctions.IsLotSerialRequired(Base.Transactions.Cache, sOLineRow.InventoryID);
                bool validateLotSerQty = false;

                if (isLotSerialRequired == true && docLine.AppDetID == null) // is posted by Service Order
                {
                    foreach (FSSODetSplit split in PXSelect<FSSODetSplit,
                                                   Where<
                                                        FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                                        And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                                        And<FSSODetSplit.lineNbr, Equal<Required<FSSODetSplit.lineNbr>>>>>,
                                                   OrderBy<Asc<FSSODetSplit.splitLineNbr>>>
                                                   .Select(Base, soDet.SrvOrdType, soDet.RefNbr, soDet.LineNbr))
                    {
                        if (split.POCreate == false && string.IsNullOrEmpty(split.LotSerialNbr) == false)
                        {
                            SOLineSplit newSplit = new SOLineSplit();
                            newSplit = (SOLineSplit)Base.splits.Cache.CreateCopy(Base.splits.Insert(newSplit));

                            newSplit.SiteID = split.SiteID != null ? split.SiteID : newSplit.SiteID;
                            newSplit.LocationID = split.LocationID != null ? split.LocationID : newSplit.LocationID;
                            newSplit.LotSerialNbr = split.LotSerialNbr;
                            newSplit.Qty = split.Qty;

                            newSplit = Base.splits.Update(newSplit);
                            validateLotSerQty = true;
                        }
                    }

                    sOLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);
                }
                else if (isLotSerialRequired == true && docLine.AppDetID != null)
                {
                    foreach (FSApptLineSplit split in PXSelect<FSApptLineSplit,
                                                       Where<
                                                            FSApptLineSplit.srvOrdType, Equal<Required<FSApptLineSplit.srvOrdType>>,
                                                            And<FSApptLineSplit.apptNbr, Equal<Required<FSApptLineSplit.apptNbr>>,
                                                            And<FSApptLineSplit.lineNbr, Equal<Required<FSApptLineSplit.lineNbr>>>>>,
                                                       OrderBy<Asc<FSApptLineSplit.splitLineNbr>>>
                                                       .Select(Base, apptDet.SrvOrdType, apptDet.RefNbr, apptDet.LineNbr))
                    {
                        if (string.IsNullOrEmpty(split.LotSerialNbr) == false)
                        {
                            SOLineSplit newSplit = new SOLineSplit();
                            newSplit = (SOLineSplit)Base.splits.Cache.CreateCopy(Base.splits.Insert(newSplit));

                            newSplit.SiteID = split.SiteID != null ? split.SiteID : newSplit.SiteID;
                            newSplit.LocationID = split.LocationID != null ? split.LocationID : newSplit.LocationID;
                            newSplit.LotSerialNbr = split.LotSerialNbr;
                            newSplit.Qty = split.Qty;

                            newSplit = Base.splits.Update(newSplit);
                            validateLotSerQty = true;
                        }
                    }

                    sOLineRow = (SOLine)Base.Transactions.Cache.CreateCopy(sOLineRow);
                }

                if (validateLotSerQty == false)
                {
                    sOLineRow.OrderQty = docLine.Qty;
                }
                else if (sOLineRow.OrderQty != docLine.Qty)
                {
                    throw new PXException(TX.Error.QTY_POSTED_ERROR);
                }

                Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.isFree>(sOLineRow, docLine.IsFree == true);

                if (docLine.IsFree == true)
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.manualPrice>(sOLineRow, true);
                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.curyUnitPrice>(sOLineRow, 0m);

                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.manualDisc>(sOLineRow, true);
                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.discPct>(sOLineRow, 0m);
                }
                else
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.manualPrice>(sOLineRow, docLine.ManualPrice);
                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.curyUnitPrice>(sOLineRow, docLine.CuryUnitPrice);

                    bool manualDisc = false;
                    if (apptDet != null)
                    {
                        manualDisc = (bool)apptDet.ManualDisc;
                    }
                    else if (soDet != null)
                    {
                        manualDisc = (bool)soDet.ManualDisc;
                    }

                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.manualDisc>(sOLineRow, manualDisc);

                    if (manualDisc == true)
                    {
                        Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.discPct>(sOLineRow, docLine.DiscPct);
                    }
                }

                if (docLine.ServiceContractID != null
                        && docLine.ContractRelated == false
                        && (docLine.SODetID != null || docLine.AppDetID != null))
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.curyExtPrice>(sOLineRow, docLine.CuryBillableExtPrice);
                }

                sOLineRow = Base.Transactions.Update(sOLineRow);

                Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.commissionable>(sOLineRow, docLine.Commissionable ?? false);
                Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.tranDesc>(sOLineRow, docLine.TranDescPrefix + sOLineRow.TranDesc);

                Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.taskID>(sOLineRow, docLine.ProjectTaskID);
                Base.Transactions.Cache.SetValueExtIfDifferent<SOLine.costCodeID>(sOLineRow, docLine.CostCodeID);

                //Set the line as a posted
                fSxSOLineRow = Base.Transactions.Cache.GetExtension<FSxSOLine>(sOLineRow);

                fSxSOLineRow.SDPosted = true;

                fSxSOLineRow.ServiceContractRefNbr = fsServiceContractRow.RefNbr;
                fSxSOLineRow.ServiceContractPeriodID = fsContractPeriodRow.ContractPeriodID;

                if (docLine.ContractRelated == false)
                {
                    fSxSOLineRow.SrvOrdType = docLine.SrvOrdType;
                    fSxSOLineRow.ServiceOrderRefNbr = soDet.RefNbr;
                    fSxSOLineRow.ServiceOrderLineNbr = soDet.LineNbr;
                    fSxSOLineRow.AppointmentRefNbr = docLine.fsAppointmentDet?.RefNbr;
                    fSxSOLineRow.AppointmentLineNbr = docLine.fsAppointmentDet?.LineNbr;
                }

                if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
                {
                    if (docLine.EquipmentAction != null)
                    {
                        Base.Transactions.Cache.SetValueExtIfDifferent<FSxSOLine.equipmentAction>(sOLineRow, docLine.EquipmentAction);
                        Base.Transactions.Cache.SetValueExtIfDifferent<FSxSOLine.sMEquipmentID>(sOLineRow, docLine.SMEquipmentID);
                        Base.Transactions.Cache.SetValueExtIfDifferent<FSxSOLine.equipmentComponentLineNbr>(sOLineRow, docLine.EquipmentLineRef);

                        if (docLine.EquipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT
                            || ((docLine.EquipmentAction == ID.Equipment_Action.CREATING_COMPONENT
                                 || docLine.EquipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
                                 || docLine.EquipmentAction == ID.Equipment_Action.NONE)
                                    && string.IsNullOrEmpty(docLine.NewTargetEquipmentLineNbr) == false))
                        {
                            componentList.Add(new SharedClasses.SOARLineEquipmentComponent(docLine, sOLineRow, fSxSOLineRow));
                        }
                        else
                        {
                            fSxSOLineRow.ComponentID = docLine.ComponentID;
                        }
                    }
                }

                sOLineRow = Base.Transactions.Update(sOLineRow);
            }

				SetChildCustomerShipToInfo(fsServiceContractRow.ServiceContractID);

				if (componentList.Count > 0)
            {
                //Assigning the NewTargetEquipmentLineNbr field value for the component type records
                foreach (SharedClasses.SOARLineEquipmentComponent currLineModel in componentList.Where(x => x.equipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT))
                {
                    foreach (SharedClasses.SOARLineEquipmentComponent currLineComponent in componentList.Where(x => (x.equipmentAction == ID.Equipment_Action.CREATING_COMPONENT
                                                                                                                    || x.equipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
                                                                                                                    || x.equipmentAction == ID.Equipment_Action.NONE)))
                    {
                        if (currLineComponent.sourceNewTargetEquipmentLineNbr == currLineModel.sourceLineRef)
                        {
                            currLineComponent.fsxSOLineRow.ComponentID = currLineComponent.componentID;
                            currLineComponent.fsxSOLineRow.NewEquipmentLineNbr = currLineModel.currentLineRef;
                        }
                    }
                }
            }

            if (Base.soordertype.Current.RequireControlTotal == true)
            {
                Base.Document.Cache.SetValueExtIfDifferent<SOOrder.curyControlTotal>(sOOrderRow, sOOrderRow.CuryOrderTotal);
            }

            Base.Document.Cache.SetValueExtIfDifferent<SOOrder.hold>(sOOrderRow, false);

            sOOrderRow = Base.Document.Update(sOOrderRow);

            sOOrderRow.Status = SOOrderStatus.Open;
            }
            finally 
            {
                Base.FieldDefaulting.RemoveHandler<SOOrder.branchID>(cancel_defaulting);
            }

            Exception newException = null;

            try
            {
                Base.Save.Press();
            }
            catch (Exception e)
            {
                List<ErrorInfo> errorList = this.GetErrorInfo();
                newException = GetErrorInfoInLines(errorList, e);
            }

            if (newException != null)
            {
                throw newException;
            }

            FSContractPostDoc fsContractCreatedDocRow = new FSContractPostDoc()
            {
                ContractPeriodID = fsContractPeriodRow.ContractPeriodID,
                ContractPostBatchID = fsContractPostBatchRow.ContractPostBatchID,
                PostDocType = sOOrderRow.OrderType,
                PostedTO = ID.Batch_PostTo.SO,
                PostRefNbr = sOOrderRow.RefNbr,
                ServiceContractID = fsServiceContractRow.ServiceContractID
            };

            return fsContractCreatedDocRow;
        }
        #endregion
        #endregion

        #region Validations
        public virtual void ValidatePostBatchStatus(PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
        {
            DocGenerationHelper.ValidatePostBatchStatus<SOOrder>(Base, dbOperation, postTo, createdDocType, createdRefNbr);
        }
        #endregion

        #region Virtual Functions
        public virtual int? Get_INItemAcctID_DefaultValue(PXGraph graph, string salesAcctSource, int? inventoryID, int? customerID, int? locationID)
        {
            return ServiceOrderEntry.Get_INItemAcctID_DefaultValueInt(graph, salesAcctSource, inventoryID, customerID, locationID);
        }

		protected virtual bool IsRunningServiceContractBilling(PXGraph graph)
		{
			return InvoiceHelper.IsRunningServiceContractBilling(graph);
		}

		protected virtual void GetChildCustomerShippingContactAndAddress(PXGraph graph, int? serviceContractID, out Contact shippingContact, out Address shippingAddress)
		{
			InvoiceHelper.GetChildCustomerShippingContactAndAddress(graph, serviceContractID, out shippingContact, out shippingAddress);
		}

		#endregion

		#region Private methods

		private void UpdatePostingInformation(SOOrder soRow)
		{
			var requiredAllocationList = FSAllocationProcess.GetRequiredAllocationList(Base, soRow);

			FSAllocationProcess.ReallocateServiceOrderSplits(requiredAllocationList);

			CleanPostingInfoLinkedToDoc(soRow);
			CleanContractPostingInfoLinkedToDoc(soRow);
		}

		public virtual void SetChildCustomerShipToInfo(int? serviceContractID)
		{
			if (Base.Document.Current == null)
			{
				return;
			}

			if (IsRunningServiceContractBilling(Base) == false)
			{
				return;
			}

			if (Base.Document.Cache.GetStatus(Base.Document.Current) != PXEntryStatus.Inserted)
			{
				return;
			}

			Contact childCustomerShippingContact = null;
			Address childCustomerShippingAddress = null;

			GetChildCustomerShippingContactAndAddress(Base, serviceContractID, out childCustomerShippingContact, out childCustomerShippingAddress);

			if (childCustomerShippingContact != null)
			{
				SOShippingContact docShippingContact = null;

				Base.Shipping_Contact.Current = docShippingContact = Base.Shipping_Contact.SelectSingle();

				if (docShippingContact != null)
				{
					Base.Shipping_Contact.Cache.SetValueExt<SOShippingContact.overrideContact>(docShippingContact, true);
					docShippingContact = Base.Shipping_Contact.SelectSingle();

					docShippingContact = (SOShippingContact)Base.Shipping_Contact.Cache.CreateCopy(docShippingContact);

					int? customerID = docShippingContact.CustomerID;
					int? customerContactID = docShippingContact.CustomerContactID;
					InvoiceHelper.CopyContact(docShippingContact, childCustomerShippingContact);
					docShippingContact.CustomerID = customerID;
					docShippingContact.CustomerContactID = customerContactID;
					docShippingContact.RevisionID = 0;

					docShippingContact.IsDefaultContact = false;

					Base.Shipping_Contact.Update(docShippingContact);
				}
			}

			if (childCustomerShippingAddress != null)
			{
				SOShippingAddress docShippingAddress = null;

				Base.Shipping_Address.Current = docShippingAddress = Base.Shipping_Address.SelectSingle();

				if (docShippingAddress != null)
				{
					Base.Shipping_Address.Cache.SetValueExt<SOShippingAddress.overrideAddress>(docShippingAddress, true);
					docShippingAddress = Base.Shipping_Address.SelectSingle();

					docShippingAddress = (SOShippingAddress)Base.Shipping_Address.Cache.CreateCopy(docShippingAddress);

					int? customerID = docShippingAddress.CustomerID;
					int? customerAddressID = docShippingAddress.CustomerAddressID;
					InvoiceHelper.CopyAddress(docShippingAddress, childCustomerShippingAddress);
					docShippingAddress.CustomerID = customerID;
					docShippingAddress.CustomerAddressID = customerAddressID;
					docShippingAddress.RevisionID = 0;

					docShippingAddress.IsDefaultAddress = false;

					Base.Shipping_Address.Update(docShippingAddress);
				}
			}
		}

		#endregion

	}
}
