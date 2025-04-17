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
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using System;

namespace PX.Objects.FS
{
    public class SetupMaint : PXGraph<SetupMaint>
    {
        public PXSave<FSSetup> Save;
        public PXCancel<FSSetup> Cancel;
        public PXSelect<FSSetup> SetupRecord;
        public CRNotificationSetupList<FSNotification> Notifications;
        public PXSelect<NotificationSetupRecipient,
               Where<
                   NotificationSetupRecipient.setupID, Equal<Current<FSNotification.setupID>>>> Recipients;

        public PXSelect<FSSrvOrdType, Where<FSSrvOrdType.requireRoom, Equal<True>>> SrvOrdTypeRequireRoomRecords;

        public SetupMaint()
            : base()
        {
            FieldUpdating.AddHandler(typeof(FSSetup),
                                     typeof(FSSetup.dfltCalendarStartTime).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSSetup_DfltCalendarStartTime_Time_FieldUpdating);

            FieldUpdating.AddHandler(typeof(FSSetup),
                                     typeof(FSSetup.dfltCalendarEndTime).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSSetup_DfltCalendarEndTime_Time_FieldUpdating);

            FieldUpdating.AddHandler(typeof(FSSetup),
                                     typeof(FSSetup.rOLunchBreakStartTimeFrame).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSSetup_ROLunchBreakStartTimeFrame_Time_FieldUpdating);
            FieldUpdating.AddHandler(typeof(FSSetup),
                                     typeof(FSSetup.rOLunchBreakEndTimeFrame).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSSetup_ROLunchBreakEndTimeFrame_Time_FieldUpdating);
        }

        #region CacheAttached
        #region NotificationSetupRecipient_ContactType
        [PXDBString(10)]
        [PXDefault]
        [ApptContactType.ClassList]
        [PXUIField(DisplayName = "Contact Type")]
        [PXCheckUnique(typeof(NotificationSetupRecipient.contactID),
            Where = typeof(Where<NotificationSetupRecipient.setupID, Equal<Current<NotificationSetupRecipient.setupID>>>))]
        public virtual void NotificationSetupRecipient_ContactType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region NotificationSetupRecipient_ContactID
        [PXDBInt]
        [PXUIField(DisplayName = "Contact ID")]
        [PXNotificationContactSelector(typeof(NotificationSetupRecipient.contactType),
            typeof(
            Search2<Contact.contactID,
            LeftJoin<EPEmployee,
                On<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>,
                And<EPEmployee.defContactID, Equal<Contact.contactID>>>>,
            Where<
                Current<NotificationSetupRecipient.contactType>, Equal<NotificationContactType.employee>,
                And<EPEmployee.acctCD, IsNotNull>>>))]
        public virtual void NotificationSetupRecipient_ContactID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Public methods

        /// <summary>
        /// Updates <c>FSSrvOrdType.createTimeActivitiesFromAppointment</c> when the Time Card integration is enabled.
        /// </summary>
        /// <param name="graph">PXGraph instance.</param>
        /// <param name="enableEmpTimeCardIntegration">Flag that says whether the TimeCard integration is enabled or not.</param>
        public virtual void Update_SrvOrdType_TimeActivitiesFromAppointment(PXGraph graph, bool? enableEmpTimeCardIntegration)
        {
            if (enableEmpTimeCardIntegration == true)
            {
                PXUpdate<
                    Set<FSSrvOrdType.createTimeActivitiesFromAppointment, True>,
                FSSrvOrdType>
                .Update(graph);
            }
        }

        public virtual void EnableDisable_Document(PXCache cache, FSSetup fsSetupRow)
        {
            PXDefaultAttribute.SetPersistingCheck<FSSetup.contractPostOrderType>(cache, fsSetupRow, PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<FSSetup.dfltContractTermIDARSO>(cache, fsSetupRow, PXPersistingCheck.Nothing);
            PXUIFieldAttribute.SetEnabled<FSSetup.rOLunchBreakEndTimeFrame>(cache, fsSetupRow, fsSetupRow.ROLunchBreakDuration != null);
            PXUIFieldAttribute.SetEnabled<FSSetup.rOLunchBreakStartTimeFrame>(cache, fsSetupRow, fsSetupRow.ROLunchBreakDuration != null);
        }

        #endregion


        #region Event Handlers

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        protected virtual void _(Events.FieldDefaulting<FSSetup, FSSetup.rOLunchBreakStartTimeFrame> e)
        {
            if (e.Row == null)
            {
                return;
            }

            e.NewValue = new DateTime(1900, 1, 1, 12, 0, 0);
        }

        protected virtual void _(Events.FieldDefaulting<FSSetup, FSSetup.rOLunchBreakEndTimeFrame> e)
        {
            if (e.Row == null)
            {
                return;
            }

            e.NewValue = new DateTime(1900, 1, 1, 14, 0, 0);
        }
        #endregion
        #region FieldUpdating
        //Cannot change to new event format
        protected virtual void FSSetup_DfltCalendarStartTime_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;

            fsSetupRow.DfltCalendarStartTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            DateTime? bussinessDatePlusCurrentHours = new DateTime(Accessinfo.BusinessDate.Value.Year,
                                                                   Accessinfo.BusinessDate.Value.Month,
                                                                   Accessinfo.BusinessDate.Value.Day,
                                                                   0,
                                                                   0,
                                                                   0);

            fsSetupRow.DfltCalendarStartTime = PXDBDateAndTimeAttribute.CombineDateTime(bussinessDatePlusCurrentHours, fsSetupRow.DfltCalendarStartTime);
        }

        //Cannot change to new event format
        protected virtual void FSSetup_DfltCalendarEndTime_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;

            fsSetupRow.DfltCalendarEndTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            DateTime? bussinessDatePlusCurrentHours = new DateTime(Accessinfo.BusinessDate.Value.Year,
                                                                   Accessinfo.BusinessDate.Value.Month,
                                                                   Accessinfo.BusinessDate.Value.Day,
                                                                   0,
                                                                   0,
                                                                   0);

            fsSetupRow.DfltCalendarEndTime = PXDBDateAndTimeAttribute.CombineDateTime(bussinessDatePlusCurrentHours, fsSetupRow.DfltCalendarEndTime);
        }

        //Cannot change to new event format
        protected virtual void FSSetup_ROLunchBreakStartTimeFrame_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;

            fsSetupRow.ROLunchBreakStartTimeFrame = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            DateTime? defaultDate = new DateTime(1900, 1, 1, 0, 0, 0);

            fsSetupRow.ROLunchBreakStartTimeFrame = PXDBDateAndTimeAttribute.CombineDateTime(defaultDate, fsSetupRow.ROLunchBreakStartTimeFrame);
        }

        //Cannot change to new event format
        protected virtual void FSSetup_ROLunchBreakEndTimeFrame_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;

            fsSetupRow.ROLunchBreakEndTimeFrame = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            DateTime? defaultDate = new DateTime(1900, 1, 1, 0, 0, 0);

            fsSetupRow.ROLunchBreakEndTimeFrame = PXDBDateAndTimeAttribute.CombineDateTime(defaultDate, fsSetupRow.ROLunchBreakEndTimeFrame);
        }
        #endregion
        #region FieldVerifying
        protected virtual void _(Events.FieldVerifying<FSSetup, FSSetup.rOLunchBreakDuration> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = e.Row;

            if (fsSetupRow.ROLunchBreakStartTimeFrame.HasValue 
                    && fsSetupRow.ROLunchBreakEndTimeFrame.HasValue)
            {
                int diffFrameDuration = (int) (fsSetupRow.ROLunchBreakEndTimeFrame.Value.TimeOfDay.TotalMinutes 
                                                    - fsSetupRow.ROLunchBreakStartTimeFrame.Value.TimeOfDay.TotalMinutes);

                if (diffFrameDuration < (int?)e.NewValue)
                {
                    e.Cache.RaiseExceptionHandling<FSSetup.rOLunchBreakDuration>(fsSetupRow, null, new PXSetPropertyException("Duration cannot be greater that difference between time frames", PXErrorLevel.Error));
                    e.NewValue = null;
                }
            }
        }
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSSetup, FSSetup.appAutoConfirmGap> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;

            if (fsSetupRow.AppAutoConfirmGap < 0)
            {
                e.Cache.RaiseExceptionHandling<FSSetup.appAutoConfirmGap>(fsSetupRow,
                                                                          fsSetupRow.AppAutoConfirmGap,
                                                                          new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(TX.Error.MINIMUN_VALUE, " 00 h 00 m"), PXErrorLevel.Error));
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSetup, FSSetup.manageRooms> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;

            if (fsSetupRow.ManageRooms == false)
            {
                var fsServiceOrderTypeSet = SrvOrdTypeRequireRoomRecords.Select();

                if (fsServiceOrderTypeSet != null && fsServiceOrderTypeSet.Count > 0)
                {
                    WebDialogResult result = SrvOrdTypeRequireRoomRecords.Ask(TX.WebDialogTitles.CONFIRM_MANAGE_ROOMS, TX.Messages.CANNOT_HIDE_ROOMS_IN_SM, MessageButtons.YesNo);

                    if (result == WebDialogResult.Yes)
                    {
                        SvrOrdTypeMaint graphSvrOrdTypeMaint = PXGraph.CreateInstance<SvrOrdTypeMaint>();

                        foreach (FSSrvOrdType fsSrvOrdTypeRow in fsServiceOrderTypeSet)
                        {
                            fsSrvOrdTypeRow.RequireRoom = false;
                            graphSvrOrdTypeMaint.SvrOrdTypeRecords.Update(fsSrvOrdTypeRow);
                            graphSvrOrdTypeMaint.Save.Press();
                            graphSvrOrdTypeMaint.Clear();
                        }
                    }
                    else
                    {
                        fsSetupRow.ManageRooms = true;
                    }
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSetup, FSSetup.rOLunchBreakStartTimeFrame> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = e.Row;

            if (fsSetupRow.ROLunchBreakStartTimeFrame.HasValue
                    && fsSetupRow.ROLunchBreakEndTimeFrame.HasValue == false)
            {
                e.Cache.SetValueExt<FSSetup.rOLunchBreakEndTimeFrame>(fsSetupRow, fsSetupRow.ROLunchBreakStartTimeFrame.Value.AddMinutes((double)fsSetupRow.ROLunchBreakDuration));
            }
            else if (fsSetupRow.ROLunchBreakStartTimeFrame.HasValue
                        && fsSetupRow.ROLunchBreakEndTimeFrame.HasValue)
            {
                if (fsSetupRow.ROLunchBreakEndTimeFrame.Value.TimeOfDay.TotalMinutes
                        - fsSetupRow.ROLunchBreakStartTimeFrame.Value.TimeOfDay.TotalMinutes < fsSetupRow.ROLunchBreakDuration)
                {
                    e.Cache.SetValueExt<FSSetup.rOLunchBreakEndTimeFrame>(fsSetupRow, fsSetupRow.ROLunchBreakStartTimeFrame.Value.AddMinutes((double)fsSetupRow.ROLunchBreakDuration));
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSetup, FSSetup.rOLunchBreakEndTimeFrame> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = e.Row;

            if (fsSetupRow.ROLunchBreakEndTimeFrame.HasValue
                    && fsSetupRow.ROLunchBreakStartTimeFrame.HasValue == false)
            {
                e.Cache.SetValueExt<FSSetup.rOLunchBreakStartTimeFrame>(fsSetupRow, fsSetupRow.ROLunchBreakEndTimeFrame.Value.AddMinutes(-(double)fsSetupRow.ROLunchBreakDuration));
            }
            else if (fsSetupRow.ROLunchBreakStartTimeFrame.HasValue
                        && fsSetupRow.ROLunchBreakEndTimeFrame.HasValue)
            {
                if (fsSetupRow.ROLunchBreakEndTimeFrame.Value.TimeOfDay.TotalMinutes
                        - fsSetupRow.ROLunchBreakStartTimeFrame.Value.TimeOfDay.TotalMinutes < fsSetupRow.ROLunchBreakDuration)
                {
                    e.Cache.SetValueExt<FSSetup.rOLunchBreakStartTimeFrame>(fsSetupRow, fsSetupRow.ROLunchBreakEndTimeFrame.Value.AddMinutes(-(double)fsSetupRow.ROLunchBreakDuration));
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<FSSetup, FSSetup.rOLunchBreakDuration> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = e.Row;

            if (fsSetupRow.ROLunchBreakDuration == null)
            {
                e.Cache.SetValueExt<FSSetup.rOLunchBreakStartTimeFrame>(fsSetupRow, null);
                e.Cache.SetValueExt<FSSetup.rOLunchBreakEndTimeFrame>(fsSetupRow, null);
            }
            else if (fsSetupRow.ROLunchBreakDuration != null && e.OldValue == null)
            {
                e.Cache.SetDefaultExt<FSSetup.rOLunchBreakStartTimeFrame>(fsSetupRow);
            }
        }

		protected virtual void _(Events.FieldUpdated<FSSetup, FSSetup.customerMultipleBillingOptions> e)
		{
			if (e.Row != null && e.NewValue != e.OldValue)
			{
				PXUIFieldAttribute.SetWarning<FSSetup.customerMultipleBillingOptions>(e.Cache, e.Row, TX.Warning.MakeSureVerifyCustomers);
			}
		}

		#endregion

		protected virtual void _(Events.RowSelected<FSSetup> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;

            EnableDisable_Document(e.Cache, fsSetupRow);
        }

		protected virtual void _(Events.RowUpdated<FSSetup> e)
		{
			if (e.Row == null)
			{
				return;
			}

			var fsSetupRow = e.Row;

			if (fsSetupRow.DfltCalendarStartTime.HasValue && fsSetupRow.DfltCalendarEndTime.HasValue
				&& fsSetupRow.DfltCalendarStartTime.Value.TimeOfDay > fsSetupRow.DfltCalendarEndTime.Value.TimeOfDay)
			{
				var exception = new PXSetPropertyException(TX.Error.DayEndTimeCannotBeEarlierDayStartTime, PXErrorLevel.RowError);
				e.Cache.RaiseExceptionHandling<FSSetup.dfltCalendarEndTime>(fsSetupRow, null, exception);
			}
		}

        protected virtual void _(Events.RowPersisting<FSSetup> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSSetup fsSetupRow = (FSSetup)e.Row;
            Update_SrvOrdType_TimeActivitiesFromAppointment(this, fsSetupRow.EnableEmpTimeCardIntegration);
        }

        #endregion
    }
}
