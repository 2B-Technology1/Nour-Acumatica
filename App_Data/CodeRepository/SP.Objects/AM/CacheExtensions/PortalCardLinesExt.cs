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
using SP.Objects.IN;

namespace SP.Objects.AM
{
    /// <summary>
    /// Manufacturing extension to B2B Portal Cart items (PortalCardLines)
    /// </summary>
    [Serializable]
    public sealed class PortalCardLinesExt : PXCacheExtension<PortalCardLines>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.manufacturingProductConfigurator>();
        }

        #region AMConfigurationID
        public abstract class aMConfigurationID : PX.Data.IBqlField
        {
        }
        [PXDBString(IsUnicode = true)]
        [PXUIField(DisplayName = "Configuration ID", Enabled = false, Visible = false)]
        public string AMConfigurationID
        {
            get;
            set;
        }

        #endregion

        #region AMIsConfigurable
        public abstract class aMIsConfigurable : PX.Data.IBqlField
        {
        }
        [PXBool]
        [PXUIField(DisplayName = "Configurable", Enabled = false)]
        [PXDependsOnFields(typeof(PortalCardLinesExt.aMConfigurationID))]
        public bool? AMIsConfigurable
        {
            get
            {
                return !string.IsNullOrEmpty(AMConfigurationID);
            }
        }
        #endregion
    }
}