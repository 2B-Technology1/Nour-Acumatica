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
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSLogActionMobileFilter : FSLogActionFilter
    {
        #region SrvOrdType
        public abstract class srvOrdType : Data.BQL.BqlString.Field<srvOrdType> { }

        [PXString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXDefault(typeof(Coalesce<
                            Search<FSxUserPreferences.dfltSrvOrdType,
                            Where<
                                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>>>,
                            Search<FSSetup.dfltSrvOrdType>>))]
        [PXUIField(DisplayName = "Service Order Type")]
        [FSSelectorSrvOrdTypeNOTQuote]
        [PXUIVerify(typeof(Where<Current<FSSrvOrdType.active>, Equal<True>>),
                    PXErrorLevel.Warning, TX.Error.SRVORDTYPE_INACTIVE, CheckOnRowSelected = true)]
        [Data.EP.PXFieldDescription]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : Data.BQL.BqlInt.Field<sOID> { }

        [PXInt]
        [PXUIField(DisplayName = "Appointment Nbr.")]
        [PXSelector(typeof(Search2<FSAppointment.appointmentID,
                           LeftJoin<FSServiceOrder,
                           On<
                               FSServiceOrder.sOID, Equal<FSAppointment.sOID>>>,
                           Where<
                               FSAppointment.srvOrdType, Equal<Optional<FSLogActionMobileFilter.srvOrdType>>>,
                           OrderBy<
                               Desc<FSAppointment.refNbr>>>),
                    new Type[] {
                                typeof(FSAppointment.refNbr),
                                typeof(FSAppointment.docDesc),
                                typeof(FSAppointment.status),
                                typeof(FSAppointment.scheduledDateTimeBegin)
                    }, SubstituteKey = typeof(FSAppointment.refNbr))]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : Data.BQL.BqlInt.Field<sOID> { }

        [PXInt]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [PXSelector(typeof(Search2<FSServiceOrder.sOID,
                           LeftJoin<Customer,
                           On<
                               Customer.bAccountID, Equal<FSServiceOrder.customerID>>>,
                           Where<
                               FSServiceOrder.srvOrdType, Equal<Current<FSLogActionMobileFilter.srvOrdType>>>,
                           OrderBy<
                               Desc<FSServiceOrder.refNbr>>>), SubstituteKey = typeof(FSServiceOrder.refNbr))]
        public virtual int? SOID { get; set; }
        #endregion
        #region LineRef
        public abstract class employeeLineRef : PX.Data.BQL.BqlString.Field<employeeLineRef> { }

        [PXString(3, IsFixed = true)]
        [PXUIField(Enabled = false, Visible = false)]
        public virtual string EmployeeLineRef { get; set; }
        #endregion
        #region Overrides
        #region DetLineRef
        public new abstract class detLineRef : Data.BQL.BqlString.Field<detLineRef> { }

        [PXString(4, IsFixed = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIVisible(typeof(Where<
                                Current<action>, Equal<ListField_LogActions.Start>,
                                And<type, NotEqual<FSLogTypeAction.StaffAssignment>,
                                And<type, NotEqual<FSLogTypeAction.SrvBasedOnAssignment>>>>))]
        [PXUIRequired(typeof(Where<
                                Current<action>, Equal<ListField_LogActions.Start>,
                                And<type, NotEqual<FSLogTypeAction.StaffAssignment>,
                                And<type, NotEqual<FSLogTypeAction.SrvBasedOnAssignment>,
                                And<type, NotEqual<FSLogTypeAction.Travel>>>>>))]
        [PXSelector(typeof(Search2<FSAppointmentDet.lineRef,
            InnerJoin<InventoryItem,
            On<
                InventoryItem.inventoryID, Equal<FSAppointmentDet.inventoryID>>>,
            Where<
                FSAppointmentDet.appointmentID, Equal<Current<FSLogActionMobileFilter.appointmentID>>,
            And<
                FSAppointmentDet.lineType, Equal<FSLineType.Service>,
            And<
                FSAppointmentDet.lineRef, IsNotNull,
            And<
                FSxService.isTravelItem, Equal<Current<FSLogActionMobileFilter.isTravelAction>>>>>>>),
            new Type[] {
                        typeof(FSAppointmentDet.lineRef),
                        typeof(InventoryItem.inventoryCD),
                        typeof(FSAppointmentDet.tranDesc),
                        typeof(FSAppointmentDet.estimatedDuration)
            }, 
            DescriptionField = typeof(FSAppointmentDet.tranDesc))]
        [PXUIField(DisplayName = "Detail Ref. Nbr.")]
        public override string DetLineRef { get; set; }
        #endregion
        #endregion
    }
}