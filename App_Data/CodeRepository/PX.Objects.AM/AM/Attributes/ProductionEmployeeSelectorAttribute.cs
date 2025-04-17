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
using PX.Objects.AM.CacheExtensions;
using PX.Objects.EP;
using PX.Objects.AP;

namespace PX.Objects.AM.Attributes
{
    // similar to PXEPEmployeeSelectorAttribute

    /// <summary>
    /// Manufacturing/Production employee Selector
    /// </summary>
    public class ProductionEmployeeSelectorAttribute : PXDimensionSelectorAttribute
    {
        /// <summary>
        /// Showing only production employees that are active
        /// </summary>
        public ProductionEmployeeSelectorAttribute()
            : base("BIZACCT",
            typeof (Search2<EPEmployee.bAccountID,
                    LeftJoin<EPEmployeePosition,
                        On<EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>,
                        And<EPEmployeePosition.isActive, Equal<True>>>>,
                    Where<EPEmployeeExt.amProductionEmployee, Equal<True>,
                        And<EPEmployee.vStatus, Equal<VendorStatus.active>>>>)
            , typeof (EPEmployee.acctCD)
            , typeof (EPEmployee.bAccountID), typeof (EPEmployee.acctCD), typeof (EPEmployee.acctName), typeof (EPEmployeePosition.positionID),
                typeof (EPEmployee.departmentID), typeof (EPEmployee.defLocationID), /*typeof(EPEmployeeExt.amSiteID),*/ typeof(EPEmployee.calendarID))
            {
                this.DescriptionField = typeof (EPEmployee.acctName);
            }

        /// <summary>
        /// Allowing for any search criteria
        /// </summary>
        /// <param name="searchType"></param>
        public ProductionEmployeeSelectorAttribute(System.Type searchType)
            : base("BIZACCT",
            searchType
            , typeof(EPEmployee.acctCD)
            , typeof(EPEmployee.bAccountID), typeof(EPEmployee.acctCD), typeof(EPEmployee.acctName), typeof(EPEmployeePosition.positionID),
                typeof(EPEmployee.departmentID), typeof(EPEmployee.defLocationID), /*typeof(EPEmployeeExt.amSiteID),*/ typeof(EPEmployee.calendarID))
        {
            this.DescriptionField = typeof(EPEmployee.acctName);
        }
    }
}
