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

using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.Descriptor.Attributes;

namespace PX.Objects.PJ.PhotoLogs.PJ.DAC
{
    [PXCacheName("Photo Log Status")]
    public class PhotoLogStatus : IStatus, IBqlTable
    {
        [PXDBIdentity(IsKey = true, DatabaseFieldName = "PhotoLogStatusId")]
        public int? StatusId
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [Unique(ErrorMessage = ProjectManagementMessages.StatusNameUniqueConstraint)]
        [PXUIField(DisplayName = "Status")]
        public string Name
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public string Description
        {
            get;
            set;
        }

        [PXDBBool]
        public bool? IsDefault
        {
            get;
            set;
        }

        public abstract class statusId : BqlInt.Field<statusId>
        {
        }

        public abstract class name : BqlString.Field<name>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }

        public abstract class isDefault : BqlBool.Field<isDefault>
        {
        }
    }
}