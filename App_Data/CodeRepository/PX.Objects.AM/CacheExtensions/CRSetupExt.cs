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

using System;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class CRSetupExt : PXCacheExtension<CRSetup>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingEstimating>() ||
                   PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingProductConfigurator>();
        }

        #region AMConfigurationEntry
        public abstract class aMConfigurationEntry : PX.Data.BQL.BqlBool.Field<aMConfigurationEntry> { }

        /// <summary>
        /// Indicates if Opportunities allow the user to launch the configuration entry page.
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Allow Configuration Entry", FieldClass = Features.SHAREDPRODUCTCONFIGURATORFIELDCLASS)]
        public bool? AMConfigurationEntry { get; set; }
        #endregion
        #region AMEstimateEntry
        public abstract class aMEstimateEntry : PX.Data.BQL.BqlBool.Field<aMEstimateEntry> { }

        /// <summary>
        /// Indicates if opportunities will allow estimates
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Allow Estimating", FieldClass = Features.ESTIMATINGFIELDCLASS)]
        public bool? AMEstimateEntry { get; set; }
        #endregion
    }
}
