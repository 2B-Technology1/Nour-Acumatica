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
using PX.Objects.PM;
using System;

namespace PX.Objects.FS
{
    public class FSSelectorVendorRestrictVisibilityAttribute : PXDimensionSelectorAttribute
    {
        public FSSelectorVendorRestrictVisibilityAttribute()
            : base(
                VendorAttribute.DimensionName,
                typeof(Search2<
                    Vendor.bAccountID,
                    LeftJoin<Contact,
                        On<Contact.bAccountID, Equal<Vendor.bAccountID>,
                        And<Contact.contactID, Equal<Vendor.defContactID>>>,
                    LeftJoin<Address,
                        On<Address.bAccountID, Equal<Vendor.bAccountID>,
                        And<Address.addressID, Equal<Vendor.defAddressID>>>>>,
                    Where<FSxVendor.sDEnabled, Equal<True>,
                        And<Vendor.vStatus, NotEqual<VendorStatus.inactive>,
                        And<Vendor.vOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>>>),
                typeof(Vendor.acctCD),
                new Type[]
                {
                    typeof(Vendor.acctCD),
                    typeof(Vendor.vStatus),
                    typeof(Vendor.acctName),
                    typeof(Vendor.classID),
                    typeof(Contact.phone1),
                    typeof(Address.city),
                    typeof(Address.countryID)
                })
        {
            DescriptionField = typeof(Vendor.acctName);
            DirtyRead = true;
        }
    }

    public class FSSelectorBusinessAccount_VEVisibilityRestrictionAttribute : PXDimensionSelectorAttribute
    {
        public FSSelectorBusinessAccount_VEVisibilityRestrictionAttribute()
            : base(
                VendorAttribute.DimensionName,
                BqlCommand.Compose(
                            typeof(Search2<,,>),
                            typeof(Vendor.bAccountID),
                            typeof(LeftJoin<,,>),
                            typeof(Contact),
                            typeof(On<
                                    Contact.bAccountID, Equal<Vendor.bAccountID>,
                                    And<Contact.contactID, Equal<Vendor.defContactID>>>),
                            typeof(LeftJoin<,>),
                            typeof(Address),
                            typeof(On<
                                    Address.bAccountID, Equal<Vendor.bAccountID>,
                                    And<Address.addressID, Equal<Vendor.defAddressID>>>),
                            typeof(Where<
                                    Vendor.vOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
                                    And<Match<Vendor, Current<AccessInfo.userName>>>>)
                            ),
                typeof(Vendor.acctCD),
                FSSelectorBusinessAccount_BaseAttribute.VendorSelectorColumns)
        {
            DescriptionField = typeof(Vendor.acctName);
        }
    }

    public class FSSelector_StaffMember_ServiceOrderProjectIDVisibilityRestrictionAttribute : PXDimensionSelectorAttribute
    {
        public FSSelector_StaffMember_ServiceOrderProjectIDVisibilityRestrictionAttribute()
            : base(
                BAccountAttribute.DimensionName,
                typeof(
                    Search2<BAccountStaffMember.bAccountID,
                       LeftJoin<Vendor,
                       On<
                           Vendor.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                        And<Vendor.vStatus, NotEqual<VendorStatus.inactive>>>,
                       LeftJoin<EPEmployee,
                       On<
                           EPEmployee.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                        And<EPEmployee.vStatus, NotEqual<VendorStatus.inactive>>>,
                       LeftJoin<PMProject,
                       On<
                           PMProject.contractID, Equal<Current<FSServiceOrder.projectID>>>,
                       LeftJoin<EPEmployeeContract,
                       On<
                           EPEmployeeContract.contractID, Equal<PMProject.contractID>,
                           And<EPEmployeeContract.employeeID, Equal<BAccountStaffMember.bAccountID>>>,
                       LeftJoin<EPEmployeePosition,
                       On<
                           EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>,
                           And<EPEmployeePosition.isActive, Equal<True>>>>>>>>,
                       Where<
                           PMProject.isActive, Equal<True>,
                       And<
                           PMProject.baseType, Equal<CT.CTPRType.project>,
                       And<
                           Where2<
                               Where2<
                                   Where<FSxVendor.sDEnabled, Equal<True>>,
                                   And<Vendor.vOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>,
                               Or<
                                   Where<
                                       FSxEPEmployee.sDEnabled, Equal<True>,
                                   And<
                                       Where<
                                           PMProject.restrictToEmployeeList, Equal<False>,
                                       Or<
                                           EPEmployeeContract.employeeID, IsNotNull>>>>>>>>>,
                       OrderBy<
                           Asc<BAccountStaffMember.acctCD>>>),
                typeof(BAccountStaffMember.acctCD),
                new Type[]
                {
                    typeof(BAccountStaffMember.acctCD),
                    typeof(BAccountStaffMember.acctName),
                    typeof(BAccountStaffMember.type),
                    typeof(BAccountStaffMember.vStatus),
                    typeof(EPEmployeePosition.positionID)
                })
        {
            DescriptionField = typeof(BAccountStaffMember.acctName);
        }
    }
}
