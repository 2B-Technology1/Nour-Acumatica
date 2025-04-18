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
using PX.Objects.Common;
using PX.Objects.CR;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class RouteClosingMaint : PXGraph<RouteClosingMaint, FSRouteDocument>
    {
        public RouteClosingMaint()
            : base()
        {
            PXUIFieldAttribute.SetDisplayName<FSServiceOrder.locationID>(ServiceOrder.Cache, TX.CustomTextFields.CUSTOMER_LOCATION);
            PXUIFieldAttribute.SetDisplayName<FSAppointment.estimatedDurationTotal>(Appointment.Cache, TX.CustomTextFields.ESTIMATED_DURATION);

            FieldUpdating.AddHandler(typeof(FSRouteDocument),
                                     typeof(FSRouteDocument.actualStartTime).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSRouteDocument_ActualStartTime_FieldUpdating);

            FieldUpdating.AddHandler(typeof(FSRouteDocument),
                                     typeof(FSRouteDocument.actualEndTime).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSRouteDocument_ActualEndTime_FieldUpdating);

            AppointmentsInRoute.Cache.AllowUpdate = false;
        }

        public bool AutomaticallyCloseRoute = false;
        public bool AutomaticallyUncloseRoute = false;

        #region CacheAttached

        #region FSRouteDocument_RefNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Route Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(
            typeof(
                Search<FSRouteDocument.refNbr,
                Where<
                    FSRouteDocument.status, Equal<FSRouteDocument.status.Completed>,
                    Or<FSRouteDocument.status, Equal<FSRouteDocument.status.Closed>>>>))]
        protected virtual void FSRouteDocument_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_ServiceContractID
        [PXDBInt]
        [PXSelector(typeof(Search<FSServiceContract.serviceContractID,
                           Where<
                                FSServiceContract.customerID, Equal<Current<FSServiceOrder.customerID>>>>),
                           SubstituteKey = typeof(FSServiceContract.refNbr))]
        [PXUIField(DisplayName = "Service Contract ID", Enabled = false)]
        protected virtual void FSAppointment_ServiceContractID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_ScheduleID
        [PXDBInt]
        [PXSelector(typeof(Search<FSSchedule.scheduleID,
                           Where<
                                FSSchedule.entityType, Equal<ListField_Schedule_EntityType.Contract>,
                                And<FSSchedule.entityID, Equal<Current<FSServiceOrder.serviceContractID>>>>>),
                           SubstituteKey = typeof(FSSchedule.refNbr))]
        [PXUIField(DisplayName = "Schedule ID", Enabled = false)]
        protected virtual void FSAppointment_ScheduleID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSRouteDocument_ActualStartTime
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Actual Start Time")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Actual Start Time")]
        protected virtual void FSRouteDocument_ActualStartTime_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSRouteDocument_ActualEndTime
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Actual End Time")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Actual End Time")]
        protected virtual void FSRouteDocument_ActualEndTime_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #endregion

        #region Select

        // Baccount workaround
        [PXHidden]
        public PXSelect<BAccount> BAccount;
        [PXHidden]
        public PXSelect<Contact> Contact;

        // Baccount workaround
        [PXHidden]
        public PXSelect<FSServiceOrder> ServiceOrder;

        [PXHidden]
        public PXSelect<FSAppointment> Appointment;

        public PXSetup<FSRouteSetup> RouteSetupRecord;

        public PXSelect<FSRouteDocument, 
               Where<
                   FSRouteDocument.status, Equal<ListField_Status_Route.Completed>, 
                   Or<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>>>> RouteRecords;

        public PXSelect<FSRouteDocument,
               Where<
                   FSRouteDocument.routeDocumentID, Equal<Current<FSRouteDocument.routeDocumentID>>>> RouteDocumentSelected;

        public PXSelectJoin<FSAppointment,
               InnerJoin<FSServiceOrder,
                   On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
			InnerJoin<FSAddress, On<FSAddress.addressID, Equal<FSServiceOrder.serviceOrderAddressID>>,
               LeftJoin<FSServiceContract, 
                   On<FSServiceContract.serviceContractID, Equal<FSAppointment.serviceContractID>>>>>,
               Where<
                   FSAppointment.routeDocumentID, Equal<Current<FSRouteDocument.routeDocumentID>>>,
               OrderBy<
                   Asc<FSAppointment.routePosition>>> AppointmentsInRoute;
        #endregion

        #region Actions

        #region Close
        public PXAction<FSRouteDocument> closeRoute;
        [PXUIField(DisplayName = "Close", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(Category = CommonActionCategories.DisplayNames.Processing)]
        public virtual IEnumerable CloseRoute(PXAdapter adapter)
        {
            FSRouteDocument fsRouteDocumentRow = RouteRecords.Current;
            if (fsRouteDocumentRow != null)
            {
                if (AutomaticallyCloseRoute || WebDialogResult.Yes == RouteRecords.Ask(TX.WebDialogTitles.CONFIRM_ROUTE_CLOSING,
                                                                                       TX.Messages.ASK_CONFIRM_ROUTE_CLOSING,
                                                                                       MessageButtons.YesNo))
                {
                    var fsAppointmentSet = PXSelectReadonly2<FSAppointment,
                                           InnerJoin<FSSrvOrdType,
                                                On<FSSrvOrdType.srvOrdType, Equal<FSAppointment.srvOrdType>>,
                                           InnerJoin<FSAppointmentDet,
                                                On<FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>>,
                                           LeftJoin<FSPostInfo,
                                                On<FSPostInfo.postID, Equal<FSAppointmentDet.postID>>>>>,
                                           Where<
                                                FSAppointmentDet.lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                                                And<FSAppointment.routeDocumentID, Equal<Current<FSRouteDocument.routeDocumentID>>,
                                                And<FSAppointment.canceled, Equal<False>,
                                                And<FSSrvOrdType.enableINPosting, Equal<True>,
                                                And<
                                                    Where<
                                                        FSPostInfo.postID, IsNull,
                                                        Or<FSPostInfo.iNPosted, Equal<False>>>>>>>>>
                                           .Select(this, fsRouteDocumentRow.RouteDocumentID);

                    if (fsAppointmentSet.Count != 0)
                    {
                        throw new PXException(TX.Error.ROUTE_DOCUMENT_APPOINTMENTS_NOT_POSTED);
                    }
                    else
                    {
                        string errorMessage = string.Empty;
                        if (CloseAppointmentsInRoute(ref errorMessage) == false)
                        {
                            throw new PXException(errorMessage);
                        }

                        this.SelectTimeStamp();
                        RouteRecords.Cache.AllowUpdate = true;
                        fsRouteDocumentRow.Status = ID.Status_Route.CLOSED;
                        RouteRecords.Cache.SetStatus(fsRouteDocumentRow, PXEntryStatus.Updated);
                        Save.Press();
                    }
                }
            }

            return adapter.Get();
        }
        #endregion

        #region Unclose
        public PXAction<FSRouteDocument> uncloseRoute;
        [PXUIField(DisplayName = "Unclose", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(Category = FSToolbarCategory.CategoryNames.Corrections)]
        public virtual IEnumerable UncloseRoute(PXAdapter adapter)
        {
            FSRouteDocument fsRouteDocumentRow = RouteRecords.Current;

            if (fsRouteDocumentRow != null)
            {
                if (AutomaticallyUncloseRoute || WebDialogResult.Yes == RouteRecords.Ask(TX.WebDialogTitles.CONFIRM_ROUTE_UNCLOSING,
                                                                                         TX.Messages.ASK_CONFIRM_ROUTE_UNCLOSING,
                                                                                         MessageButtons.YesNo))
                {
                    UncloseRouteAndSave(RouteRecords.Cache, fsRouteDocumentRow);
                }
            }

            return adapter.Get();
        }
        #endregion

        #region Open Appointment
        public PXAction<FSRouteDocument> openAppointment;
        [PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
        [PXUIField(DisplayName = "Open Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenAppointment()
        {
            if (AppointmentsInRoute.Current != null && AppointmentsInRoute.Current.RouteDocumentID != null)
            {
                AppointmentClosingMaint graphClosingAppointment = PXGraph.CreateInstance<AppointmentClosingMaint>();
                graphClosingAppointment.ClosingAppointmentRecords.Current = graphClosingAppointment.ClosingAppointmentRecords.Search<FSAppointment.appointmentID>
                                                                            (AppointmentsInRoute.Current.AppointmentID, AppointmentsInRoute.Current.SrvOrdType, AppointmentsInRoute.Current.RouteDocumentID);

                throw new PXRedirectRequiredException(graphClosingAppointment, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region Open CustomerLocation
        public PXDBAction<FSRouteDocument> openCustomerLocation;
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenCustomerLocation()
        {
            LocationHelper.OpenCustomerLocation(this, AppointmentsInRoute.Current.SOID);
        }
        #endregion

        #region OpenRouteScheduleScreen
        public PXAction<FSRouteDocument> OpenRouteSchedule;
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void openRouteSchedule()
        {
            if (AppointmentsInRoute.Current != null)
            {
                var graphRouteServiceContractScheduleEntry = PXGraph.CreateInstance<RouteServiceContractScheduleEntry>();

                graphRouteServiceContractScheduleEntry.ContractScheduleRecords.Current = graphRouteServiceContractScheduleEntry
                                                                                         .ContractScheduleRecords.Search<FSRouteContractSchedule.scheduleID>
                                                                                         (AppointmentsInRoute.Current.ScheduleID,
                                                                                          AppointmentsInRoute.Current.ServiceContractID);

                throw new PXRedirectRequiredException(graphRouteServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region OpenRouteContract
        public PXAction<FSRouteDocument> OpenRouteContract;
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        public virtual void openRouteContract()
        {
            if (AppointmentsInRoute.Current != null)
            {
                RouteServiceContractEntry graphRouteServiceContractEntry = PXGraph.CreateInstance<RouteServiceContractEntry>();

                FSServiceContract fsServiceContractRow_Local = PXSelectJoin<FSServiceContract,
                                                               InnerJoin<FSSchedule,
                                                               On<
                                                                   FSSchedule.entityID, Equal<FSServiceContract.serviceContractID>,
                                                                   And<FSSchedule.customerID, Equal<FSServiceContract.customerID>>>>,
                                                               Where<
                                                                   FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                                                               .Select(this, AppointmentsInRoute.Current.ScheduleID);

                graphRouteServiceContractEntry.ServiceContractRecords.Current = fsServiceContractRow_Local;
                throw new PXRedirectRequiredException(graphRouteServiceContractEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #endregion

        #region Virtual Functions
        /// <summary>
        /// Enables/Disables the actions defined for Route Closing.
        /// </summary>
        public virtual void EnableDisable_ActionButtons(PXCache cache, FSRouteDocument fsRouteDocumentRow)
        {
            closeRoute.SetEnabled(fsRouteDocumentRow.Status != ID.Status_Route.CLOSED);
            uncloseRoute.SetEnabled(fsRouteDocumentRow.Status == ID.Status_Route.CLOSED);
        }

        /// <summary>
        /// Enable/Disable additional info fields.
        /// </summary>
        public virtual void EnableDisable_AdditionalInfoFields(PXCache cache, FSRouteDocument fsRouteDocumentRow)
        {
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.miles>(cache, fsRouteDocumentRow, fsRouteDocumentRow.Status != ID.Status_Route.CLOSED);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.weight>(cache, fsRouteDocumentRow, fsRouteDocumentRow.Status != ID.Status_Route.CLOSED);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.fuelQty>(cache, fsRouteDocumentRow, fsRouteDocumentRow.Status != ID.Status_Route.CLOSED);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.fuelType>(cache, fsRouteDocumentRow, fsRouteDocumentRow.Status != ID.Status_Route.CLOSED);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.oil>(cache, fsRouteDocumentRow, fsRouteDocumentRow.Status != ID.Status_Route.CLOSED);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.antiFreeze>(cache, fsRouteDocumentRow, fsRouteDocumentRow.Status != ID.Status_Route.CLOSED);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.dEF>(cache, fsRouteDocumentRow, fsRouteDocumentRow.Status != ID.Status_Route.CLOSED);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.propane>(cache, fsRouteDocumentRow, fsRouteDocumentRow.Status != ID.Status_Route.CLOSED);
        }

        /// <summary>
        /// Closes all appointments belonging to the current Route, in case an error occurs with any appointment,
        /// the route will not be closed and a message will be displayed alerting the user about the appointment's issue.
        /// The row of the appointment having problems is marked with its error.
        /// </summary>
        /// <param name="errorMessage">Error message to be displayed.</param>
        /// <returns>True in case all appointments are closed, otherwise False.</returns>
        public virtual bool CloseAppointmentsInRoute(ref string errorMessage)
        {
            bool allAppointmentsClosed = true;

            PXResultset<FSAppointment> bqlResultSet = AppointmentsInRoute.Select();

            if (bqlResultSet.Count > 0)
            {
                Dictionary<FSAppointment, string> appWithErrors = CloseAppointments(bqlResultSet);

                if (appWithErrors.Count > 0)
                {
                    foreach (KeyValuePair<FSAppointment, string> kvp in appWithErrors)
                    {
                        AppointmentsInRoute.Cache.RaiseExceptionHandling<FSAppointment.refNbr>(kvp.Key,
                                                                                               kvp.Key.RefNbr,
                                                                                               new PXSetPropertyException(kvp.Value, PXErrorLevel.RowError));
                    }

                    allAppointmentsClosed = false;
                    errorMessage = PXMessages.LocalizeFormatNoPrefix(TX.Error.ROUTE_CANT_BE_CLOSED_APPOINTMENTS_IN_ROUTE_HAVE_ISSUES);
                }
            }

            return allAppointmentsClosed;
        }

        public virtual void UncloseRouteAndSave(PXCache viewCache, FSRouteDocument fsRouteDocumentRow)
        {
            fsRouteDocumentRow.Status = ID.Status_Route.COMPLETED;
            ForceUpdateCacheAndSave(viewCache, fsRouteDocumentRow);
        }

        public virtual void ForceUpdateCacheAndSave(PXCache cache, object row)
        {
            cache.AllowUpdate = true;
            cache.SetStatus(row, PXEntryStatus.Updated);
            this.GetSaveAction().Press();
        }

        /// <summary>
        /// Closes all appointments belonging to appointmentList, in case an error occurs with any appointment,
        /// the method will return a Dictionary listing each appointment with its error.
        /// </summary>
        public virtual Dictionary<FSAppointment, string> CloseAppointments(PXResultset<FSAppointment> bqlResultSet)
        {
            return ServiceOrderEntry.CloseAppointmentsInt(bqlResultSet);
        }
        #endregion

        #region Event Handlers

        #region FSRouteDocument
        #region FieldSelecting
        #endregion
        #region FieldDefaulting

        protected virtual void _(Events.FieldDefaulting<FSRouteDocument, FSRouteDocument.actualStartTime> e)
        {
            e.NewValue = PXDBDateAndTimeAttribute.CombineDateTime(Accessinfo.BusinessDate.Value, PXTimeZoneInfo.Now);
        }

        protected virtual void _(Events.FieldDefaulting<FSRouteDocument, FSRouteDocument.actualEndTime> e)
        {
            e.NewValue = PXDBDateAndTimeAttribute.CombineDateTime(Accessinfo.BusinessDate.Value, PXTimeZoneInfo.Now);
        }

        #endregion
        #region FieldUpdating

        //Cant move this to new event format
        protected virtual void FSRouteDocument_ActualStartTime_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;
            DateTime? dateTimeAux = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            if (dateTimeAux.HasValue == true)
            {
                fsRouteDocumentRow.ActualStartTime = PXDBDateAndTimeAttribute.CombineDateTime(Accessinfo.BusinessDate, dateTimeAux);
            }
        }

        //Cant move this to new event format
        protected virtual void FSRouteDocument_ActualEndTime_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;
            DateTime? dateTimeAux = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            if (dateTimeAux.HasValue == true)
            {
                fsRouteDocumentRow.ActualEndTime = PXDBDateAndTimeAttribute.CombineDateTime(Accessinfo.BusinessDate, dateTimeAux);
            }
        }
        
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSRouteDocument> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;
            PXCache cache = e.Cache;

            EnableDisable_ActionButtons(cache, fsRouteDocumentRow);
            EnableDisable_AdditionalInfoFields(cache, fsRouteDocumentRow);
            SharedFunctions.CheckRouteActualDateTimes(cache, fsRouteDocumentRow, Accessinfo.BusinessDate);

            PXUIFieldAttribute.SetEnabled<FSRouteDocument.routeID>(cache, fsRouteDocumentRow, false);
        }

        protected virtual void _(Events.RowInserting<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSRouteDocument> e)
        {
        }

        #endregion

        #endregion
    }
}
