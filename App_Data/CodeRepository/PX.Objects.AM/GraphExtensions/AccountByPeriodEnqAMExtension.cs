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

using System.Collections;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AM.GraphExtensions
{
    public class AccountByPeriodEnqAMExtension : PXGraphExtension<AccountByPeriodEnq>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        public PXAction<AccountByPeriodFilter> ViewDocument;
        [PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable viewDocument(PXAdapter adapter)
        {
            GLTranR tran = Base.GLTranEnq.Current;
            if (tran != null && tran.OrigModule == Common.ModuleAM && tran.Module == BatchModule.GL)
            {
                // This is a manufactured created entry...
                AMDocType.DocTypeRedirectRequiredException(tran.TranType, tran.OrigBatchNbr, Base);
                return adapter.Get();
            }
            return Base.viewDocument(adapter);
        }
    }
}
