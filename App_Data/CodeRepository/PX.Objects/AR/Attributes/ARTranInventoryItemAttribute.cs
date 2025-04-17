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
using PX.Objects.IN;

namespace PX.Objects.AR
{
	/// <summary>
	/// Provides a selector for the <see cref="InventoryItem"> items, which may be put into the <see cref="ARInvoice"> line <br/>
	/// The list is filtered by the user access rights and Inventory Item status - <see cref="InventoryItemStatus.Inactive"> <br/>
	/// and marked to delete items are not shown. If the Purchase order <see cref="ARTran.sOOrderNbr"> is specified - <br/>
	/// all the items are shown, restriction is made in other place. <br/>
	/// May be used only on DAC derived from <see cref="ARTran">. <br/>
	/// <example>
	/// [ARTranInventoryItem(Filterable = true)]
	/// </example>
	/// </summary>
	[PXDBInt]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<
		Current<ARSetup.migrationMode>, Equal<True>,
		Or<InventoryItem.stkItem, NotEqual<True>>>), AP.Messages.CannotFindInventoryItem)]
	[PXRestrictor(typeof(Where<
		Current<ARSetup.migrationMode>, Equal<True>,
		Or<Current<ARTran.sOOrderNbr>, IsNotNull,
		Or<InventoryItem.stkItem, NotEqual<False>,
		Or<InventoryItem.kitItem, NotEqual<True>>>>>), SO.Messages.CannotAddNonStockKitDirectly)]
	[PXRestrictor(typeof(Where<
		Current<ARSetup.migrationMode>, Equal<True>,
		Or<InventoryItem.stkItem, NotEqual<False>,
		Or<InventoryItem.kitItem, NotEqual<True>>>>), IN.Messages.CannotAddNonStockKit)]
	public class ARTranInventoryItemAttribute : InventoryAttribute
	{
		public ARTranInventoryItemAttribute()
			: base(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>),
				typeof(InventoryItem.inventoryCD),
				typeof(InventoryItem.descr))
		{
		}
	}
}