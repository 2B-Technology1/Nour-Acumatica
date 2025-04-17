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
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXVirtual]
    [PXBreakInheritance]
    public class FSLogActionPCRFilter : FSLogActionFilter
    {
        #region Action
        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Action")]
        [PXUnboundDefault(ID.LogActions.RESUME)]
        [action.PCRListAttribute]
        public override string Action { get; set; }
        #endregion
        #region Type

        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Logging")]
        [PXUnboundDefault(FSLogActionFilter.type.Values.Service)]
        [FSLogTypeAction.STListAttribute]
        public override string Type { get; set; }
        #endregion
    }

    [Serializable]
    [PXVirtual]
    [PXBreakInheritance]
    public class FSLogActionStartFilter : FSLogActionFilter
    {
        #region Action
        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        [PXUnboundDefault(ID.LogActions.START)]
        [action.StartListAttribute]
        public override string Action { get; set; }
        #endregion
        #region Type

        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Logging")]
        [PXUnboundDefault(FSLogActionFilter.type.Values.Service)]
        [FSLogTypeAction.STListAttribute]
        public override string Type { get; set; }
        #endregion
    }

    [Serializable]
    [PXVirtual]
    [PXBreakInheritance]
    public class FSLogActionStartServiceFilter : FSLogActionFilter
    {
        #region Action
        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        [PXUnboundDefault(ID.LogActions.START)]
        [action.StartListAttribute]
        public override string Action { get; set; }
        #endregion
        #region Type

        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Logging", Enabled = false)]
        [PXUnboundDefault(FSLogActionFilter.type.Values.ServBasedAssignment)]
        [FSLogTypeAction.List]
        public override string Type { get; set; }
        #endregion
    }

    [Serializable]
    [PXVirtual]
    [PXBreakInheritance]
    public class FSLogActionStartStaffFilter : FSLogActionFilter
    {
        #region Action
        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        [PXUnboundDefault(ID.LogActions.START)]
        [action.StartListAttribute]
        public override string Action { get; set; }
        #endregion
        #region Type

        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Logging", Enabled = false)]
        [PXUnboundDefault(FSLogActionFilter.type.Values.Staff)]
        [FSLogTypeAction.List]
        public override string Type { get; set; }
        #endregion
    }

    [Serializable]
    [PXVirtual]
    public class FSLogActionFilter : IBqlTable
    {
        #region Action
        public abstract class action : ListField_LogActions { }

        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Action")]
        [PXUnboundDefault(ID.LogActions.START)]
        [action.ListAtrribute]
        public virtual string Action { get; set; }
        #endregion
        #region Type
        public abstract class type : Data.BQL.BqlString.Field<type>
        {
            public abstract class Values : ListField_LogAction_Type { }
        }

        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Logging")]
        [PXUnboundDefault(FSLogActionFilter.type.Values.Travel)]
        [FSLogTypeAction.List]
        public virtual string Type { get; set; }
        #endregion
        #region LogDateTime
        public abstract class logDateTime : Data.BQL.BqlDateTime.Field<logDateTime> { }

        [PXDBDateAndTime(UseTimeZone = true, DisplayNameDate = "Date", DisplayNameTime = "Time")]
        [PXUIField(DisplayName = "Date")]
        public virtual DateTime? LogDateTime { get; set; }
        #endregion
        #region IsTravelAction
        public abstract class isTravelAction : PX.Data.BQL.BqlBool.Field<isTravelAction> { }

        protected Boolean? _IsTravelAction;

        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(Visible = false, Enabled = false)]
        public virtual bool? IsTravelAction
        {
            get
            {
                return Type == FSLogActionFilter.type.Values.Travel;
            }
        }
        #endregion
        #region DetLineRef
        public abstract class detLineRef : Data.BQL.BqlString.Field<detLineRef> { }

        [PXString(4, IsFixed = true)]
        [PXFormula(typeof(Default<type>))]
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
        [FSSelectorAppointmentSODetID(typeof(Where<
                                                FSAppointmentDet.lineType, Equal<FSLineType.Service>,
                                             And<
                                                 FSAppointmentDet.isTravelItem, Equal<Current<FSLogActionFilter.isTravelAction>>,
                                             And<
                                                FSAppointmentDet.lineRef, IsNotNull,
                                             And<
                                                 Where<
                                                     FSAppointmentDet.status, Equal<FSAppointmentDet.status.NotStarted>,
                                                 Or<
                                                    FSAppointmentDet.status, Equal<FSAppointmentDet.status.InProcess>>>>>>>))]
        [PXUIField(DisplayName = "Detail Ref. Nbr.")]
        public virtual string DetLineRef { get; set; }
        #endregion
        #region Me
        public abstract class me : PX.Data.BQL.BqlBool.Field<me> { }

        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Show Only Mine")]
        [PXUIVisible(typeof(Where<type, NotEqual<FSLogTypeAction.SrvBasedOnAssignment>>))]
        public virtual bool? Me { get; set; }
        #endregion
        #region VerifyRequired
        public abstract class verifyRequired : PX.Data.BQL.BqlBool.Field<verifyRequired> { }

        [PXBool]
        [PXUnboundDefault(false)]
        public virtual bool? VerifyRequired { get; set; }
        #endregion
    }
}