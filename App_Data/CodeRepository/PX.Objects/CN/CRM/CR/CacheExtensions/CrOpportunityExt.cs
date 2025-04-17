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
using PX.Data.BQL;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CN.CRM.CR.CacheExtensions
{
    public sealed class CrOpportunityExt : PXCacheExtension<CROpportunity>
    {
        [PXDBBaseCury(BqlField = typeof(CrStandaloneOpportunityExt.cost))]
        [PXUIField(DisplayName = "Cost")]
        public decimal? Cost
        {
            get;
            set;
        }

        [PXBaseCury]
        [PXFormula(typeof(Sub<curyAmount, cost>))]
        [PXUIField(DisplayName = "Gross Margin", Enabled = false)]
        public decimal? GrossMarginAbsolute
        {
            get;
            set;
        }

        [PXDecimal]
        [PXFormula(typeof(
            Switch<
                Case<Where<curyAmount, NotEqual<decimal0>>,
                    Mult<
                        Div<
                            Sub<curyAmount, cost>,
							curyAmount>,
                        decimal100>>,
                decimal0>))]
        [PXUIField(DisplayName = "Gross Margin %", Enabled = false)]
        public decimal? GrossMarginPercentage
        {
            get;
            set;
        }

        [PXDBBool(BqlField = typeof(CrStandaloneOpportunityExt.multipleAccounts))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Multiple Customers")]
        public bool? MultipleAccounts
        {
            get;
            set;
        }

		[Obsolete]
		[PXDBBaseCury(BqlField = typeof(CrStandaloneOpportunityExt.quotedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Amount")]
		public decimal? QuotedAmount
		{
			get;
			set;
		}

		[Obsolete]
		[PXDBBaseCury(BqlField = typeof(CrStandaloneOpportunityExt.totalAmount))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Sub<quotedAmount, CROpportunity.curyDiscTot>))]
		[PXUIField(DisplayName = "Total", Enabled = false)]
		public decimal? TotalAmount
		{
			get;
			set;
		}

		public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.projectQuotes>();
        }

        public abstract class cost : BqlDecimal.Field<cost>
        {
        }

        public abstract class grossMarginAbsolute : BqlDecimal.Field<grossMarginAbsolute>
        {
        }

        public abstract class grossMarginPercentage : BqlDecimal.Field<grossMarginPercentage>
        {
        }

        public abstract class multipleAccounts : BqlBool.Field<multipleAccounts>
        {
        }

		public abstract class quotedAmount : BqlDecimal.Field<quotedAmount>
		{
		}

		public abstract class totalAmount : BqlDecimal.Field<totalAmount>
		{
		}

		public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount> { }

	}
}
