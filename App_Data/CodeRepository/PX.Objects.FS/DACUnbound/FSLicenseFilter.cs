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

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class FSLicenseFilter : PX.Data.IBqlTable
    {
        #region OwnerType
        public abstract class ownerType : ListField_OwnerType
        {            
        }

        [PXString(1, IsFixed = true, IsUnicode = true)]
        [ownerType.ListAtrribute]
        [PXDefault(ID.OwnerType.BUSINESS)]
        [PXUIField(DisplayName = "Owner Type")]
        public virtual string OwnerType { get; set; }
        #endregion
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXInt]
        [PXUIField(DisplayName = "Staff Member Name")]
        [FSSelector_StaffMember_All]
        public virtual int? EmployeeID { get; set; }
        #endregion
    }
}
