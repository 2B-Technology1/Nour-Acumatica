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
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	public class SpecialOrderCostCenterSupport : SO.SpecialOrderCostCenterSupport<POReceiptEntry, POReceiptLine>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<CS.FeaturesSet.specialOrders>();

		public override IEnumerable<Type> GetFieldsDependOn()
		{
			yield return typeof(POReceiptLine.isSpecialOrder);
			yield return typeof(POReceiptLine.siteID);
		}

		public override bool IsSpecificCostCenter(POReceiptLine line)
			=> line.IsSpecialOrder == true
			&& line.SiteID != null;

		protected override CostCenterKeys GetCostCenterKeys(POReceiptLine line)
		{
			if (line.ReceiptType != POReceiptType.TransferReceipt)
			{
				SOLineSplit solineSplit = SelectFrom<SOLineSplit>
					.Where<SOLineSplit.pOType.IsEqual<POReceiptLine.pOType.FromCurrent.NoDefault>
						.And<SOLineSplit.pONbr.IsEqual<POReceiptLine.pONbr.FromCurrent.NoDefault>>
						.And<SOLineSplit.pOLineNbr.IsEqual<POReceiptLine.pOLineNbr.FromCurrent.NoDefault>>>
					.View.ReadOnly.SelectSingleBound(Base, new object[] { line });

				if (solineSplit == null)
					throw new Common.Exceptions.RowNotFoundException(Base.Caches[typeof(SOLineSplit)], line.POType, line.PONbr, line.POLineNbr);

				return new CostCenterKeys()
				{
					SiteID = line.SiteID,
					OrderType = solineSplit.OrderType,
					OrderNbr = solineSplit.OrderNbr,
					LineNbr = solineSplit.LineNbr,
				};
			}
			else return GetTransferCostCenterKeys(line);
		}

		protected virtual CostCenterKeys GetTransferCostCenterKeys(POReceiptLine line)
		{
			var soTransferLine = SOLine.PK.Find(Base, line.SOOrderType, line.SOOrderNbr, line.SOOrderLineNbr);
			if (string.IsNullOrEmpty(soTransferLine?.OrigOrderType)
				|| string.IsNullOrEmpty(soTransferLine.OrigOrderNbr)
				|| soTransferLine.OrigLineNbr == null)
			{
				throw new PXInvalidOperationException();
			}
			return new CostCenterKeys
			{
				SiteID = line.SiteID,
				OrderType = soTransferLine.OrigOrderType,
				OrderNbr = soTransferLine.OrigOrderNbr,
				LineNbr = soTransferLine.OrigLineNbr,
			};
		}

		/// <summary>
		/// Overrides <see cref="POReceiptEntry.GetCostStatusCommandCostSiteID(POReceiptLine)"/>
		/// </summary>
		[PXOverride]
		public virtual int? GetCostStatusCommandCostSiteID(POReceiptLine line, Func<POReceiptLine, int?> baseMethod)
		{
			if (line?.IsSpecialOrder == true)
			{
				var keys = GetTransferCostCenterKeys(line);
				return FindOrCreateCostCenter(keys);
			}
			
			return baseMethod(line);
		}
	}
}
