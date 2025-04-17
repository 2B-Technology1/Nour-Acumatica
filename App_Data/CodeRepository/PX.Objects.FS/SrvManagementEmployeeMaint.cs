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
using PX.Objects.CR;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    public class SrvManagementEmployeeMaint : PXGraph<SrvManagementEmployeeMaint>
    {
        public PXSelectJoin<BAccountStaffMember,
               LeftJoin<Vendor,
                   On<Vendor.bAccountID, Equal<BAccountStaffMember.bAccountID>, 
                   And<Vendor.vStatus, NotEqual<VendorStatus.inactive>>>,
               LeftJoin<EPEmployee,
                  On<EPEmployee.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                  And<EPEmployee.vStatus, NotEqual<VendorStatus.inactive>>>,
               InnerJoin<Contact,
                   On<Contact.contactID, Equal<BAccountStaffMember.defContactID>>>>>,
               Where<
                   FSxEPEmployee.sDEnabled, Equal<True>,
                   Or<FSxVendor.sDEnabled, Equal<True>>>>               
               SrvManagementStaffRecords;

        public SrvManagementEmployeeMaint() 
        {
            PXUIFieldAttribute.SetDisplayName<BAccountStaffMember.acctCD>(SrvManagementStaffRecords.Cache, TX.CustomTextFields.STAFF_MEMBER_ID);
            PXUIFieldAttribute.SetDisplayName<BAccountStaffMember.acctName>(SrvManagementStaffRecords.Cache, TX.CustomTextFields.STAFF_MEMBER_NAME);
        }

        #region Actions

        public PXAction<BAccountStaffMember> AddEmployee;
        [PXButton]
        [PXUIField(DisplayName = "Add Employee", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void addEmployee()
        {
            var graphEmployeeMaint = PXGraph.CreateInstance<EmployeeMaint>();
            graphEmployeeMaint.Insert.Press();

            FSxEPEmployee fsxEPEmployeeRow = graphEmployeeMaint.Employee.Cache.GetExtension<FSxEPEmployee>(graphEmployeeMaint.Employee.Current);
            fsxEPEmployeeRow.SDEnabled = true;
            
            throw new PXRedirectRequiredException(graphEmployeeMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        public PXAction<BAccountStaffMember> AddVendor;
        [PXButton]
        [PXUIField(DisplayName = "Add Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void addVendor()
        {
            var graphVendorMaint = PXGraph.CreateInstance<VendorMaint>();
            graphVendorMaint.Insert.Press();

            FSxVendor fsxVendorRow = graphVendorMaint.CurrentVendor.Cache.GetExtension<FSxVendor>(graphVendorMaint.CurrentVendor.Current);
            fsxVendorRow.SDEnabled = true;

            throw new PXRedirectRequiredException(graphVendorMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }        
        #endregion
    }
}
