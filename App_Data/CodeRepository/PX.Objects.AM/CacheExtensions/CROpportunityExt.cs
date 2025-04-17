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
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.IN;

namespace PX.Objects.AM.CacheExtensions
{
    /// <summary>
    /// MFG extension to CROpportunity PXProjection to use CROpportunityRevisionExt
    /// (non table extension)
    /// </summary>
    [PXCopyPasteHiddenFields(
        typeof(CROpportunityExt.aMEstimateQty),
        typeof(CROpportunityExt.aMEstimateTotal),
        typeof(CROpportunityExt.aMCuryEstimateTotal))]
    [Serializable]
    public sealed class CROpportunityExt : PXCacheExtension<CROpportunity>
    {
        public static bool IsActive()
        {
            // features in this extension only related to estimating module
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingEstimating>();
        }

        #region AMEstimateQty (CROpportunityRevisionExt.aMEstimateQty)
        /// <summary>
        /// Pointer to CROpportunityRevisionExt.aMEstimateQty
        /// </summary>
        public abstract class aMEstimateQty : PX.Data.BQL.BqlDecimal.Field<aMEstimateQty> { }

        /// <summary>
        /// Pointer to CROpportunityRevisionExt.AMEstimateQty
        /// </summary>
        [PXDBQuantity(BqlField = typeof(CROpportunityRevisionExt.aMEstimateQty))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimate Qty", Enabled = false)]
        public Decimal? AMEstimateQty { get; set; }

        #endregion
        #region AMCuryEstimateTotal (CROpportunityRevisionExt.aMCuryEstimateTotal)
        /// <summary>
        /// Pointer to CROpportunityRevisionExt.aMCuryEstimateTotal
        /// </summary>
        public abstract class aMCuryEstimateTotal : PX.Data.BQL.BqlDecimal.Field<aMCuryEstimateTotal> { }

        /// <summary>
        /// Pointer to CROpportunityRevisionExt.AMCuryEstimateTotal
        /// </summary>
        [PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunityExt.aMEstimateTotal), BqlField = typeof(CROpportunityRevisionExt.aMCuryEstimateTotal))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Est. Amount", Enabled = false)]
        public Decimal? AMCuryEstimateTotal { get; set; }
        #endregion
        #region AMEstimateTotal (CROpportunityRevisionExt.aMEstimateTotal)
        /// <summary>
        /// Pointer to CROpportunityRevisionExt.aMEstimateTotal
        /// </summary>
        public abstract class aMEstimateTotal : PX.Data.BQL.BqlDecimal.Field<aMEstimateTotal> { }

        /// <summary>
        /// Pointer to CROpportunityRevisionExt.AMEstimateTotal
        /// </summary>
        [PXDBBaseCury(BqlField = typeof(CROpportunityRevisionExt.aMEstimateTotal))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimate Total", Enabled = false)]
        public Decimal? AMEstimateTotal { get; set; }
        #endregion
    }
}