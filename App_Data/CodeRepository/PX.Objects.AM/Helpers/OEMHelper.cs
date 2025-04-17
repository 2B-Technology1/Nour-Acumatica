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

namespace PX.Objects.AM
{
    /// <summary>
    /// Acumatica OEM helper class for Manufacturing
    /// </summary>
    public static class OEMHelper
    {
        private const string CoreFeatureSet = "PX.Objects.CS.FeaturesSet+";
        private const string MYOBFeatureSet = "MYOB.AdvancedLive.Core.Extensions.CS.DAC.FeaturesSetExtension+";

        public static class MYOBFeatures
        {
            /// <summary>
            /// MYOB - "Basic Inventory Replenishments" feature.
            /// This is a limited display of the full Acumatica set of features found in "Replenishments" (PX.Objects.CS.FeaturesSet+replenishment)
            /// </summary>
            public const string BasicInvReplenish = "basicInvReplenish";
        }

        public static bool FeatureInstalled(string feature)
        {
            if (feature.StartsWith(MYOBFeatureSet))
            {
                return PXAccess.FeatureInstalled(feature.Replace(MYOBFeatureSet, CoreFeatureSet));
            }
            
            if (!feature.StartsWith(CoreFeatureSet))
            {
                return PXAccess.FeatureInstalled($"{CoreFeatureSet}{feature}");
            }

            return PXAccess.FeatureInstalled(feature);
        }
    }
}