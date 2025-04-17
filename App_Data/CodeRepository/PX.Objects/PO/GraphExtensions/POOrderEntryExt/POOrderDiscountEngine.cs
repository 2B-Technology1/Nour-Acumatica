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

using PX.Common;
using PX.Data;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Discount.Mappers;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PO.GraphExtension.POOrderEntryExt
{
	public class POOrderDiscountEngine : PXGraphExtension<DiscountEngine<POLine, POOrderDiscountDetail>>
	{
		public static bool IsActive()
		{
			return !PXAccess.FeatureInstalled<FeaturesSet.vendorDiscounts>();
		}

		#region Overrides

		[PXOverride]
		public virtual void CalculateDocumentDiscountRate(PXCache cache,
													PXSelectBase<POLine> documentDetails,
													POLine currentLine,
													PXSelectBase<POOrderDiscountDetail> discountDetails,
													bool forceFormulaCalculation,
													Action<PXCache, PXSelectBase<POLine>, POLine, PXSelectBase<POOrderDiscountDetail>, bool> baseMethod)
		{
			if (Base.IsDocumentDiscountRateCalculationNeeded(cache, currentLine, discountDetails))
			{
				baseMethod(cache, documentDetails, currentLine, discountDetails, forceFormulaCalculation);
			}
		}

		#endregion
	}
}
