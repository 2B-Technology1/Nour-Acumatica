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

using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.PO.GraphExtensions;
using PX.Objects.Extensions;
using PX.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using PX.Objects.CS;
using PX.Objects.CM.Extensions;
using PX.Objects.Common.Extensions;
using PX.Objects.Extensions.MultiCurrency;

namespace PX.Objects.AP
{
	public class AffectedPOOrdersByAPRelease : AffectedPOOrdersByPOLineUOpen<AffectedPOOrdersByAPRelease, APReleaseProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
		}

		private POOrder[] _affectedOrders;

		[PXOverride]
		public override void Persist(Action basePersist)
		{
			_affectedOrders = GetAffectedEntities().ToArray();

			basePersist();
		}

		/// <summary>
		/// Overrides <see cref="APReleaseProcess.ExtensionsPersist"/>
		/// and <see cref="PO.GraphExtensions.APReleaseProcessExt.UpdatePOOnRelease.ExtensionsPersist"/>
		/// </summary>
		[PXOverride]
		public virtual void ExtensionsPersist(Action baseImpl)
		{
			baseImpl();

			if (_affectedOrders != null)
			{
				ProcessAffectedEntities(_affectedOrders);
				_affectedOrders = null;
			}
		}

		#region Update blanket rows

		protected override POLineUOpen UpdateBlanketRow(PXCache cache, POLineUOpen normalRow, POLineUOpen normalOldRow, bool hardCheck)
		{
			POLineUOpen blanketRow = null;
			if (!cache.ObjectsEqual<POLineUOpen.billedQty, POLineUOpen.curyBilledAmt>(normalRow, normalOldRow))
			{
				blanketRow = FindBlanketRow(cache, normalRow);
				if (blanketRow == null)
					throw new PXArgumentException(nameof(blanketRow));
				blanketRow = UpdateBilledQty(cache, blanketRow, normalRow, normalOldRow);
				if (blanketRow != null)
				{
					blanketRow = (POLineUOpen)cache.Update(blanketRow);

					//different rules to complete/close lines in APReleaseProcessExt.UpdatePOOnRelease.UpdatePOLine
					hardCheck |= POLineType.IsService(blanketRow.LineType) && blanketRow.CompletePOLine == CompletePOLineTypes.Quantity
						|| blanketRow.CompletePOLine == CompletePOLineTypes.Amount && normalOldRow.Closed == false && normalRow.Closed == true;
				}
			}

			blanketRow = base.UpdateBlanketRow(cache, normalRow, normalOldRow, hardCheck) ?? blanketRow;

			return blanketRow;
		}

		protected virtual POLineUOpen UpdateBilledQty(PXCache cache, POLineUOpen blanketRow, POLineUOpen normalRow, POLineUOpen normalOldRow)
		{
			var normalOrder = PXParentAttribute.SelectParent<POOrder>(cache, normalRow);
			var blanketOrder = PXParentAttribute.SelectParent<POOrder>(cache, blanketRow);

			var billedQtyDiff = normalRow.UOM == blanketRow.UOM
				? normalRow.BilledQty - normalOldRow.BilledQty
				: (blanketRow.InventoryID == null
					? INUnitAttribute.ConvertGlobal(Base, normalRow.UOM, blanketRow.UOM, normalRow.BaseBilledQty - normalOldRow.BaseBilledQty ?? 0, INPrecision.QUANTITY)
					: INUnitAttribute.ConvertFromBase(cache, blanketRow.InventoryID, blanketRow.UOM, normalRow.BaseBilledQty - normalOldRow.BaseBilledQty ?? 0, INPrecision.QUANTITY));
			blanketRow.BilledQty += billedQtyDiff;

			CurrencyInfo blanketCuryInfo = Base.FindImplementation<IPXCurrencyHelper>().GetCurrencyInfo(blanketRow.CuryInfoID);
			if (normalOrder.CuryID == blanketOrder.CuryID)
				blanketRow.CuryBilledAmt += normalRow.CuryBilledAmt - normalOldRow.CuryBilledAmt ?? 0;
			else
				blanketRow.CuryBilledAmt += blanketCuryInfo.CuryConvCury(normalRow.BilledAmt - normalOldRow.BilledAmt ?? 0);

			return blanketRow;
		}

		#endregion
	}
}
