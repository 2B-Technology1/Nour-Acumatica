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
using PX.Objects.CR.Standalone;
using PX.Objects.IN;

namespace PX.Objects.PM.CacheExtensions
{
	[Serializable]
	public sealed class CRQuoteExt : PXCacheExtension<PX.Objects.CR.CRQuote>
	{
		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the extension is active.
		/// </summary>
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectQuotes>();
		}

		#region CostTotal

		public abstract class costTotal : PX.Data.BQL.BqlDecimal.Field<costTotal> { }
		/// <summary>
		/// A pointer to <see cref="CROpportunityRevision.CostTotal"/>.
		/// </summary>
		[PXDBDecimal(4, BqlField = typeof(CROpportunityRevision.costTotal))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public Decimal? CostTotal { get; set; }
		#endregion

		#region CuryCostTotal
		public abstract class curyCostTotal : PX.Data.BQL.BqlDecimal.Field<curyCostTotal> { }
		/// <summary>
		/// A pointer to <see cref="CROpportunityRevision.CuryCostTotal"/>.
		/// </summary>
		[PXDBCurrency(typeof(PX.Objects.CR.CRQuote.curyInfoID), typeof(costTotal), BqlField = typeof(CROpportunityRevision.curyCostTotal))]
		[PXUIField(DisplayName = "Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public Decimal? CuryCostTotal { get; set; }
		#endregion

		#region GrossMarginAmount
		public abstract class grossMarginAmount : PX.Data.BQL.BqlDecimal.Field<grossMarginAmount> { }
		/// <summary>
		/// The gross margin amount, which is calculated from <see cref="PX.Objects.CR.CRQuote.Amount"/> and <see cref="CostTotal"/>.
		/// </summary>
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Gross Margin")]
		public Decimal? GrossMarginAmount
		{
			[PXDependsOnFields(typeof(PX.Objects.CR.CRQuote.amount), typeof(costTotal))]
			get
			{
				return base.Base.Amount - CostTotal;
			}

		}
		#endregion

		#region CuryGrossMarginAmount
		public abstract class curyGrossMarginAmount : PX.Data.BQL.BqlDecimal.Field<curyGrossMarginAmount> { }
		/// <summary>
		/// The gross margin amount in the base currency. The amount is calculated from <see cref="PX.Objects.CR.CRQuote.CuryAmount"/> and <see cref="CuryCostTotal"/>.
		/// </summary>
		[PXCurrency(typeof(PX.Objects.CR.CRQuote.curyInfoID), typeof(grossMarginAmount))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Gross Margin Amount")]
		public Decimal? CuryGrossMarginAmount
		{
			[PXDependsOnFields(typeof(PX.Objects.CR.CRQuote.curyAmount), typeof(curyCostTotal))]
			get
			{
				return base.Base.CuryAmount - CuryCostTotal;
			}

		}
		#endregion

		#region GrossMarginPct
		public abstract class grossMarginPct : PX.Data.BQL.BqlDecimal.Field<grossMarginPct> { }
		/// <summary>
		/// The percentage of the gross margin.
		/// </summary>
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Gross Margin (%)")]
		public Decimal? GrossMarginPct
		{
			[PXDependsOnFields(typeof(PX.Objects.CR.CRQuote.amount), typeof(costTotal))]
			get
			{
				if (base.Base.Amount != 0)
				{
					return 100 * (base.Base.Amount - CostTotal) / base.Base.Amount;
				}
				else
					return 0;
			}
		}
		#endregion

	}
}
