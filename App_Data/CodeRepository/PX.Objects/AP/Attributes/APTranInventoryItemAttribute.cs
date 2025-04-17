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

namespace PX.Objects.AP
{
	/// <summary>
	/// Provides a selector for the <see cref="InventoryItem"> items, which may be put into the <see cref="APInvoice"> line <br/>
	/// The list is filtered by the user access rights and Inventory Item status - <see cref="InventoryItemStatus.Inactive"> <br/>
	/// and marked to delete items are not shown. If the Purchase order <see cref="APTran.PONbr"> or PO Receipt <br/>
	/// <see cref="APTran.receiptNbr"> is specified - all the items are shown, restriction is made in other place. <br/>
	/// May be used only on DAC derived from <see cref="APTran">. <br/>
	/// <example>
	/// [APTranInventoryItem(Filterable = true)]
	/// </example>
	/// </summary>
	[PXDBInt]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<
		Current<APSetup.migrationMode>, Equal<True>,
		Or<InventoryItem.stkItem, Equal<False>,
		Or<Current<APTran.pONbr>, IsNotNull,
		Or<Current<APTran.receiptNbr>, IsNotNull,
		Or<Current<APTran.tranType>, Equal<APInvoiceType.invoice>,
		Or<Current<APInvoice.isRetainageDocument>, Equal<True>>>>>>>), Messages.CannotStockItemInAPBillDirectly)]
	[PXRestrictor(typeof(Where<
		Current<APSetup.migrationMode>, Equal<True>,
		Or<Current<APTran.pONbr>, IsNotNull,
		Or<InventoryItem.stkItem, NotEqual<False>,
		Or<InventoryItem.kitItem, NotEqual<True>>>>>), Messages.CannotAddNonStockKitInAPBillDirectly)]
	public class APTranInventoryItemAttribute : InventoryAttribute
	{
		public APTranInventoryItemAttribute()
			: base(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>),
				typeof(InventoryItem.inventoryCD),
				typeof(InventoryItem.descr))
		{
		}
	}
}