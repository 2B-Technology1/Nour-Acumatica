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
using PX.Data.BQL;
using PX.Objects.CN.Subcontracts.AP.Descriptor;
using PX.Objects.CS;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.AP.Descriptor.Messages.Subcontract;

namespace PX.Objects.CN.Subcontracts.AP.CacheExtensions
{
    public sealed class PoOrderFilterExt : PXCacheExtension<POOrderFilter>
    {
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            PO.Descriptor.Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        public string OrderNbr
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = Messages.SubcontractNumber)]
        [SubcontractNumberSelector]
        public string SubcontractNumber
        {
            get;
            set;
        }

        public abstract class showUnbilledLines : BqlString.Field<showUnbilledLines>
        {
        }
        [PXBool]
        [PXUIField(DisplayName = "Show Unbilled Lines")]
        public bool? ShowUnbilledLines
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class subcontractNumber : BqlString.Field<subcontractNumber>
        {
        }
    }
}
