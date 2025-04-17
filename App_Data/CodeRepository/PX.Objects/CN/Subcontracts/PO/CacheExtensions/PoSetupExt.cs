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
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PO;
using PoMessages = PX.Objects.CN.Subcontracts.PO.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PO.CacheExtensions
{
    public sealed class PoSetupExt : PXCacheExtension<POSetup>
    {
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PoMessages.PoSetup.SubcontractNumberingName)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = PoMessages.PoSetup.SubcontractNumberingId)]
        public string SubcontractNumberingID
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = PoMessages.PoSetup.RequireSubcontractControlTotal)]
        public bool? RequireSubcontractControlTotal
        {
            get;
            set;
        }

        [EPRequireApproval]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = PoMessages.PoSetup.SubcontractRequireApproval)]
        public bool? SubcontractRequestApproval
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        public bool? IsSubcontractSetupSaved
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class subcontractNumberingID : IBqlField
        {
        }

        public abstract class requireSubcontractControlTotal : IBqlField
        {
        }

        public abstract class subcontractRequestApproval : IBqlField
        {
        }

        public abstract class isSubcontractSetupSaved : IBqlField
        {
        }
    }
}