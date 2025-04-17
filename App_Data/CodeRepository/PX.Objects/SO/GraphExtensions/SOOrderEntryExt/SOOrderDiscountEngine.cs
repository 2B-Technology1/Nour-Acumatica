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
using PX.Objects.Common.Discount;
using PX.Objects.Common.Discount.Mappers;
using PX.Objects.CS;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class SOOrderDiscountEngine : PXGraphExtension<DiscountEngine<SOLine, SOOrderDiscountDetail>>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
		}

		#region Overrides

		/// Overrides <see cref="DiscountEngine{SOLine, SOOrderDiscountDetail}.CalculateDocumentDiscountRate(PXCache, PXSelectBase{SOLine}, SOLine, PXSelectBase{SOOrderDiscountDetail}, bool)"/>
		[PXOverride]
		public virtual void CalculateDocumentDiscountRate(PXCache cache,
													PXSelectBase<SOLine> documentDetails,
													SOLine currentLine,
													PXSelectBase<SOOrderDiscountDetail> discountDetails,
													bool forceFormulaCalculation,
													Action<PXCache, PXSelectBase<SOLine>, SOLine, PXSelectBase<SOOrderDiscountDetail>, bool> baseMethod)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>()
				|| Base.IsDocumentDiscountRateCalculationNeeded(cache, currentLine, discountDetails))
			{
				baseMethod(cache, documentDetails, currentLine, discountDetails, forceFormulaCalculation);
			}
		}

		/// Overrides <see cref="DiscountEngine{SOLine, SOOrderDiscountDetail}.UpdateDocumentDiscountRate(AmountLineFields, decimal?)"/>
		[PXOverride]
		public bool UpdateDocumentDiscountRate(AmountLineFields aLine, decimal? discountRate, Func<AmountLineFields, decimal?, bool> baseImpl)
		{
			var updated = baseImpl(aLine, discountRate);

			if(updated && aLine.Cache.Current != aLine.MappedLine)
			{
				var marginImpl = aLine.Cache.Graph.FindImplementation<Margin>();
				if (marginImpl != null)
					marginImpl.RequestRefreshLines();
			}
			return updated;
		}

		/// Overrides <see cref="DiscountEngine{SOLine, SOOrderDiscountDetail}.UpdateGroupDiscountRate(AmountLineFields, decimal?)"/>
		[PXOverride]
		public bool UpdateGroupDiscountRate(AmountLineFields aLine, decimal? discountRate, Func<AmountLineFields, decimal?, bool> baseImpl)
		{
			var updated = baseImpl(aLine, discountRate);

			if (updated && aLine.Cache.Current != aLine.MappedLine)
			{
				var marginImpl = aLine.Cache.Graph.FindImplementation<Margin>();
				if (marginImpl != null)
					marginImpl.RequestRefreshLines();
			}
			return updated;
		}

		#endregion
	}
}
