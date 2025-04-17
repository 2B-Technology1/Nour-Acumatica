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
using PX.Objects.IN;

namespace PX.Objects.SO
{
	/// <summary>
	/// Specific <see cref="INUnitAttribute"/> for additional verifying of selected UOM in <see cref="SOShipLine"/> entity
	/// </summary>
	public class SOShipLineUnitAttribute : INUnitAttribute
	{
		public SOShipLineUnitAttribute() : base(VerifyingMode.InventoryUnitConversion)
		{
			InventoryType = typeof(SOShipLine.inventoryID);
			var searchType = typeof(Search<INUnit.fromUnit,
				Where<INUnit.unitType, Equal<INUnitType.inventoryItem>,
				And<INUnit.inventoryID, Equal<Current<SOShipLine.inventoryID>>,
				And<Where<INUnit.fromUnit, Equal<INUnit.toUnit>, Or<INUnit.fromUnit, Equal<Current<SOShipLine.orderUOM>>>>>>>>);
			Init(searchType, searchType);
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null || e.NewValue == null)
				return;
			if (!VerifyOnCopyPaste && sender.Graph.IsCopyPasteContext)
				return;
			var shipLine = (SOShipLine)e.Row;
			var inventory = InventoryItem.PK.Find(sender.Graph, shipLine.InventoryID);
			if (inventory == null)
			{
				UnitVerifying(sender, e, null);
				return;
			}

			var unit = INUnit.UK.ByInventory.Find(sender.Graph, inventory.InventoryID, (string)e.NewValue);
			UnitVerifying(sender, e, unit);
			if(!unit.FromUnit.IsIn(shipLine.OrderUOM, unit.ToUnit))
				throw new PXSetPropertyException(Messages.InvalidUOM);
		}
	}
}
