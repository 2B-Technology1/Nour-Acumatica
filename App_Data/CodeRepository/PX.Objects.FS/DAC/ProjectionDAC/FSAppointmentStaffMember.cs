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
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.EP;
using System;

namespace PX.Objects.FS
{
    #region PXProjection
    [Serializable]
    [PXProjection(typeof(Select2<BAccountStaffMember,
                            LeftJoin<Vendor,
                            On<
                               Vendor.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                               And<Vendor.vStatus, NotEqual<VendorStatus.inactive>>>,
                            LeftJoin<EPEmployee,
                            On<
                               EPEmployee.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                               And<EPEmployee.vStatus, NotEqual<VendorStatus.inactive>>>,
                            LeftJoin<EPEmployeePosition,
                            On<
                               EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>,
                               And<EPEmployeePosition.isActive, Equal<True>>>>>>,
                            Where2<
                               Where<
                                   FSxVendor.sDEnabled, Equal<True>>,
                               Or<
                                   Where<
                                       FSxEPEmployee.sDEnabled, Equal<True>>>>>))]
    #endregion
    public class FSAppointmentStaffMember : BAccountStaffMember
    {
        #region BAccountID
        public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        #endregion
        #region AcctCD
        public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
        #endregion
        #region VendorSDEnabled
        public abstract class vendorSDEnabled : PX.Data.BQL.BqlBool.Field<vendorSDEnabled> { }

        [PXDBBool(BqlField = typeof(FSxVendor.sDEnabled))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Staff Member in " + TX.ModuleName.SERVICE_DISPATCH)]
        public virtual bool? VendorSDEnabled { get; set; }
        #endregion
        #region EmployeeSDEnabled
        public abstract class employeeSDEnabled : PX.Data.BQL.BqlBool.Field<employeeSDEnabled> { }

        [PXDBBool(BqlField = typeof(FSxEPEmployee.sDEnabled))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Staff Member in " + TX.ModuleName.SERVICE_DISPATCH)]
        public virtual bool? EmployeeSDEnabled { get; set; }
        #endregion
        #region PositionID
        public abstract class positionID : PX.Data.BQL.BqlString.Field<positionID> { }

        [PXDBString(10, IsUnicode = true, BqlField = typeof(EPEmployeePosition.positionID))]
        [PXDefault()]
        [PXSelector(typeof(EPPosition.positionID), DescriptionField = typeof(EPPosition.description))]
        [PXUIField(DisplayName = "Position", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String PositionID { get; set; }
        #endregion
        #region CalendarID
        public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }

        [PXDBString(10, IsUnicode = true, BqlField = typeof(EPEmployee.calendarID))]
        [PXUIField(DisplayName = "Calendar", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
        public virtual String CalendarID { get; set; }
        #endregion
    }
}
