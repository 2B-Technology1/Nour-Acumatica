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
using PX.Objects.CR;
using PX.Objects.PM;
using System;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM.GraphExtensions
{
    public class RegisterEntryAMExtension : PXGraphExtension<RegisterEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        [PXOverride]
        public virtual PMTran CreateTransaction(RegisterEntry.CreatePMTran createPMTran, Func<RegisterEntry.CreatePMTran, PMTran> baseMethod)
        {
            var timeActExt = PXCache<PMTimeActivity>.GetExtension<PMTimeActivityExt>(createPMTran.TimeActivity);
            if(timeActExt != null && timeActExt.AMIsProd == true)
            {
                return null;
            }

            return baseMethod?.Invoke(createPMTran);
        }
    }
}
