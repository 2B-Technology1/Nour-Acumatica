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
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.PO.GraphExtensions.POOrderEntryExt
{
	public class SpecialOrderCostCenterSupport : SO.SpecialOrderCostCenterSupport<POOrderEntry, POLine>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<CS.FeaturesSet.specialOrders>();

		public override IEnumerable<Type> GetFieldsDependOn()
		{
			yield return typeof(POLine.isSpecialOrder);
			yield return typeof(POLine.siteID);
		}

		public override bool IsSpecificCostCenter(POLine line)
			=> line.IsSpecialOrder == true
			&& line.SiteID != null;

		protected override CostCenterKeys GetCostCenterKeys(POLine line)
		{
			if (!string.IsNullOrEmpty(line.SOOrderType) && !string.IsNullOrEmpty(line.SOOrderNbr) && line.SOLineNbr != null)
			{
				// during Create PO we depend on these unbound fields
				return new CostCenterKeys()
				{
					SiteID = line.SiteID,
					OrderType = line.SOOrderType,
					OrderNbr = line.SOOrderNbr,
					LineNbr = line.SOLineNbr,
				};
			}
			var split = (SO.SOLineSplit)Base.RelatedSOLineSplit.View.SelectSingleBound(new[] { line });
			if (split == null)
				throw new Common.Exceptions.RowNotFoundException(Base.Transactions.Cache, line.OrderType, line.OrderNbr, line.LineNbr);
			return new CostCenterKeys()
			{
				SiteID = line.SiteID,
				OrderType = split.OrderType,
				OrderNbr = split.OrderNbr,
				LineNbr = split.LineNbr,
			};
		}
	}
}
