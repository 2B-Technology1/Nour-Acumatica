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

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing features
    /// </summary>
    public static class Features
    {
        public const string MANUFACTURINGFIELDCLASS = "MANUFACTURING";
        public const string MRPFIELDCLASS = "MFGMRP";
        public const string PRODUCTCONFIGURATORFIELDCLASS = "MFGPRODUCTCONFIGURATOR";
        public const string ESTIMATINGFIELDCLASS = "MFGESTIMATING";
        public const string ADVANCEDPLANNINGFIELDCLASS = "MFGADVANCEDPLANNING";
        public const string ECCFIELDCLASS = "MFGECC";
        public const string DATACOLLECTIONFIELDCLASS = "MFGDATACOLLECTION";
        public const string SHAREDPRODUCTCONFIGURATORFIELDCLASS = "MFGSHAREDPRODUCTCONFIGURATOR";
        public const string InventoryPlanning = "InvPlanning";
        public const string MRPDRP = "MRPDRP";

        public static bool ManufacturingEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();
        }

        public static bool AdvancedPlanningEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingAdvancedPlanning>();
        }

        public static bool MRPEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingMRP>();
        }

        public static bool MRPOrDRPEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingMRP>() || PXAccess.FeatureInstalled<FeaturesSet.distributionReqPlan>();
        }

        public static bool ManufacturingAndReplenishmentEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturing>() && PXAccess.FeatureInstalled<FeaturesSet.replenishment>();
        }

        public static bool ManufacturingOrDRPOrReplenishmentEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturing>() || PXAccess.FeatureInstalled<FeaturesSet.distributionReqPlan>()
				|| PXAccess.FeatureInstalled<FeaturesSet.replenishment>();
        }

        public static bool ProductConfiguratorEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingProductConfigurator>();
        }

        public static bool EstimatingEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingEstimating>();
        }

        public static bool ECCEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingECC>();
        }

        public static bool DataCollectionEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingDataCollection>();
        }
    }
}
