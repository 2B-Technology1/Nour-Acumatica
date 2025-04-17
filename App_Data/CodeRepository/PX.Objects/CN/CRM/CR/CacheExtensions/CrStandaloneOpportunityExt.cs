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
using PX.Objects.CM;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;

namespace PX.Objects.CN.CRM.CR.CacheExtensions
{
    public sealed class CrStandaloneOpportunityExt : PXCacheExtension<CROpportunity>
    {
        [PXDBBaseCury]
        public decimal? Cost
        {
            get;
            set;
        }

        [PXDBBool]
        public bool? MultipleAccounts
        {
            get;
            set;
        }

		[System.Obsolete]
		[PXDBBaseCury]
		public decimal? QuotedAmount
		{
			get;
			set;
		}

		[System.Obsolete]
		[PXDBBaseCury]
		public decimal? TotalAmount
		{
			get;
			set;
		}

		public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class cost : IBqlField
        {
        }

        public abstract class multipleAccounts : IBqlField
        {
        }

		public abstract class quotedAmount : IBqlField
		{
		}

		public abstract class totalAmount : IBqlField
		{
		}
	}
}
