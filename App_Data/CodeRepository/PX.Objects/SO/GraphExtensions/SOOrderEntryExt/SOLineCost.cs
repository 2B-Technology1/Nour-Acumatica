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
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	[PXProtectedAccess]
	public abstract class SOLineCost: PXGraphExtension<SOOrderEntry>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();

		#region Protected Access
		/// Uses <see cref="SOOrderEntry.IsCuryUnitCostEnabled(SOLine, SOOrder)"/>
		[PXProtectedAccess]
		protected abstract bool IsCuryUnitCostEnabled(SOLine line, SOOrder order);
		#endregion

		protected virtual void _(Events.FieldDefaulting<SOLine, SOLine.unitCost> e)
		{
			var soline = e.Row;

			decimal? baseUomUnitCost = null;
			if (!string.IsNullOrEmpty(soline?.UOM)
				&& soline.LineType.IsIn(SOLineType.NonInventory, SOLineType.MiscCharge) && Base.Document.Current != null)
			{
				InventoryItem initem = InventoryItem.PK.Find(Base, soline.InventoryID);
				var branch = Branch.PK.Find(Base, soline.BranchID);
				var itemCurySettings = InventoryItemCurySettings.PK.Find(Base, soline.InventoryID, branch?.BaseCuryID);

				if (initem != null)
				{
					if (itemCurySettings?.StdCostDate <= Base.Document.Current.OrderDate)
						baseUomUnitCost = itemCurySettings?.StdCost ?? 0m;
					else
						baseUomUnitCost = itemCurySettings?.LastStdCost ?? 0m;
				}
			}

			if (!string.IsNullOrEmpty(soline?.UOM) && baseUomUnitCost == null)
			{
				INItemSite itemSite = PXSelect<INItemSite,
					Where<INItemSite.inventoryID, Equal<Current<SOLine.inventoryID>>,
						And<INItemSite.siteID, Equal<Current<SOLine.siteID>>>>>
					.SelectSingleBound(Base, new[] { soline });
				baseUomUnitCost = itemSite?.TranUnitCost;
			}

			if (baseUomUnitCost == null)
			{
				e.NewValue = 0m;
			}
			else
			{
				decimal? lineUomUnitCost = INUnitAttribute.ConvertToBase<SOLine.inventoryID>(e.Cache, soline, soline.UOM, baseUomUnitCost.Value, INPrecision.UNITCOST);
				e.NewValue = lineUomUnitCost;
				e.Cancel = true;
			}
		}

		private bool _isUnitCostCorrection;
		private bool _isCuryUnitCostCorrection;

		protected virtual void _(Events.FieldUpdated<SOLine, SOLine.unitCost> e)
		{
			if (_isCuryUnitCostCorrection)
				return;

			decimal baseValue = (decimal?)e.NewValue ?? 0m;

			PXDBCurrencyAttribute.CuryConvCury<SOLine.curyInfoID>(e.Cache, e.Row, baseValue, out decimal curyValue, CommonSetupDecPl.PrcCst);
			if (e.Row.CuryUnitCost != curyValue)
			{
				try
				{
					_isUnitCostCorrection = true;
					e.Cache.SetValueExt<SOLine.curyUnitCost>(e.Row, curyValue);
				}
				finally
				{
					_isUnitCostCorrection = false;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<SOLine, SOLine.curyUnitCost> e)
		{
			if (_isUnitCostCorrection
				|| e.ExternalCall == false)
				return;
			
			decimal curyValue = (decimal?)e.NewValue ?? 0m;

			PXDBCurrencyAttribute.CuryConvBase<SOLine.curyInfoID>(e.Cache, e.Row, curyValue, out decimal baseValue, CommonSetupDecPl.PrcCst);
			if (e.Row.UnitCost != baseValue)
			{
				try
				{
					_isCuryUnitCostCorrection = true;
					e.Cache.SetValueExt<SOLine.unitCost>(e.Row, baseValue);
				}
				finally
				{
					_isCuryUnitCostCorrection = false;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<SOLine, SOLine.extCost> e)
		{
			decimal baseValue = (decimal?)e.NewValue ?? 0m;

			PXDBCurrencyAttribute.CuryConvCury<SOLine.curyInfoID>(e.Cache, e.Row, baseValue, out decimal curyValue);
			if (e.Row.CuryExtCost != curyValue)
				e.Cache.SetValueExt<SOLine.curyExtCost>(e.Row, curyValue);
		}
	}
}
