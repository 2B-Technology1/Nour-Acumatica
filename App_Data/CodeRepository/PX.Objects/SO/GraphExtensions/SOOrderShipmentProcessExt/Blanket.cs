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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.SO.DAC.Projections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO.GraphExtensions.SOOrderShipmentProcessExt
{
	public class Blanket : PXGraphExtension<SOOrderShipmentProcess>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();

		public SelectFrom<BlanketSOAdjust>
			.Where<BlanketSOAdjust.adjdOrderType.IsEqual<BlanketSOAdjust.adjdOrderType.AsOptional>
				.And<BlanketSOAdjust.adjdOrderNbr.IsEqual<BlanketSOAdjust.adjdOrderNbr.AsOptional>>>
			.View BlanketAdjustments;

		public SelectFrom<BlanketSOOrder>.
			Where<BlanketSOOrder.completed.IsEqual<True>
				.And<BlanketSOOrder.curyPaymentTotal.IsGreater<decimal0>>
				.And<Exists<
					Select<ARTran,
					Where<ARTran.blanketType.IsEqual<BlanketSOOrder.orderType>
						.And<ARTran.blanketNbr.IsEqual<BlanketSOOrder.orderNbr>>
						.And<ARTran.tranType.IsEqual<ARRegister.docType.AsOptional>>
						.And<ARTran.refNbr.IsEqual<ARRegister.refNbr.AsOptional>>>>>>>
			.View BlanketOrders;

		/// <summary>
		/// Overrides <see cref="SOOrderShipmentProcess.UpdateOrderShipments" />
		/// </summary>
		[PXOverride]
		public virtual List<PXResult<SOOrderShipment, SOOrder>> UpdateOrderShipments(ARRegister arDoc, HashSet<object> processed,
			Func<ARRegister, HashSet<object>,List<PXResult<SOOrderShipment, SOOrder>>> baseMethod)
		{
			var result = baseMethod(arDoc, processed);

			if (arDoc.IsCancellation != true && arDoc.IsCorrection != true &&
				result.Any(r => PXResult.Unwrap<SOOrder>(r).BlanketLineCntr > 0))
			{
				foreach(var blanketOrder in BlanketOrders.Select(arDoc.DocType, arDoc.RefNbr))
					ResetPaymentAmount(blanketOrder);
			}

			return result;
		}

		protected virtual void ResetPaymentAmount(BlanketSOOrder blanketOrder)
		{
			foreach (BlanketSOAdjust adj in BlanketAdjustments.Select(blanketOrder.OrderType, blanketOrder.OrderNbr))
			{
				BlanketSOAdjust adjcopy = PXCache<BlanketSOAdjust>.CreateCopy(adj);
				adjcopy.CuryAdjdAmt = 0m;
				adjcopy.CuryAdjgAmt = 0m;
				adjcopy.AdjAmt = 0m;
				BlanketAdjustments.Update(adjcopy);
			}
		}
	}
}
