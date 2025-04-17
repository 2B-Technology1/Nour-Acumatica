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
using PX.Data.BQL.Fluent;
using PX.Objects.CS;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	public class AffectedPOOrdersByPOReceipt : AffectedPOOrdersByPOLineUOpen<AffectedPOOrdersByPOReceipt, POReceiptEntry> 
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
		}

		protected override bool EntityIsAffected(POOrder entity)
		{
			if (entity.OrderType == POOrderType.RegularSubcontract)
				return false;
			return base.EntityIsAffected(entity);
		}

		#region POLineR fields overriding (POLineR is updating in edit mode)

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(SelectFrom<POLineR>
			.Where<POLineR.orderType.IsEqual<POOrderType.blanket>
				.And<POLineR.orderType.IsEqual<POLineR.pOType.FromCurrent>>
				.And<POLineR.orderNbr.IsEqual<POLineR.pONbr.FromCurrent>>
				.And<POLineR.lineNbr.IsEqual<POLineR.pOLineNbr.FromCurrent>>>))]
		protected virtual void _(Events.CacheAttached<POLineR.pOLineNbr> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(null, typeof(SumCalc<POLineR.baseReceivedQty>), ValidateAggregateCalculation = true)]
		protected virtual void _(Events.CacheAttached<POLineR.baseReceivedQty> e) { }

		#endregion
	}
}
