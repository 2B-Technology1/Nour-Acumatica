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
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.CN.Compliance.PO.CacheExtensions
{
    public sealed class PoOrderExt : PXCacheExtension<POOrder>
    {
        [PXString]
        public string ClDisplayName
        {
            get
            {
                switch (Base.OrderType)
                {
                    case POOrderType.RegularOrder:
                        return string.Format("{0}, {1}", PX.Objects.PO.Messages.RegularOrder, Base.OrderNbr);
                    case POOrderType.RegularSubcontract:
                        return Base.OrderNbr;
                    case POOrderType.DropShip:
                        return string.Format("{0}, {1}", PX.Objects.PO.Messages.DropShip, Base.OrderNbr);
                    case POOrderType.Blanket:
                        return string.Format("{0}, {1}", PX.Objects.PO.Messages.Blanket, Base.OrderNbr);
                    case POOrderType.StandardBlanket:
                        return string.Format("{0}, {1}", PX.Objects.PO.Messages.StandardBlanket, Base.OrderNbr);

                }

                return string.Format("{0}, {1}", Base.OrderType, Base.OrderNbr);
            }
            set { }
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class clDisplayName : IBqlField
        {
        }
    }
}