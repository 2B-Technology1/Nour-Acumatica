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
using PX.Objects.IN;
using System;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class INLocationExt : PXCacheExtension<INLocation>
    {
        public static bool IsActive()
        {
			return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingMRP>() || PXAccess.FeatureInstalled<CS.FeaturesSet.distributionReqPlan>();
		}

        #region AMMRPFlag
        public abstract class aMMRPFlag : PX.Data.BQL.BqlBool.Field<aMMRPFlag> { }
        [PXDBBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Inventory Planning")]
        public Boolean? AMMRPFlag { get; set; }
        #endregion
    }
}
