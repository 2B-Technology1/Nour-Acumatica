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
using PX.Data.BQL;
using PX.Objects.IN.Matrix.Graphs;
using System;

namespace PX.Objects.IN
{
	/// <summary>
	/// The DAC provides only common properties of the <see cref="InventoryItem"/> DAC
	/// </summary>
	[PXPrimaryGraph(new Type[]{
		typeof(InventoryItemMaint),
		typeof(NonStockItemMaint),
		typeof(TemplateInventoryItemMaint)
		},
		new Type[]{
			typeof(Select<InventoryItem, Where<InventoryItem.inventoryCD, Equal<Current<InventoryItemCommon.inventoryCD>>, And<InventoryItem.isTemplate, Equal<False>, And<InventoryItem.stkItem, Equal<True>>>>>),
			typeof(Select<InventoryItem, Where<InventoryItem.inventoryCD, Equal<Current<InventoryItemCommon.inventoryCD>>, And<InventoryItem.isTemplate, Equal<False>, And<InventoryItem.stkItem, NotEqual<True>>>>>),
			typeof(Select<InventoryItem, Where<InventoryItem.inventoryCD, Equal<Current<InventoryItemCommon.inventoryCD>>, And<InventoryItem.isTemplate, Equal<True>>>>),
		})]
	[PXCacheName(Messages.InventoryItemCommon, PXDacType.Catalogue)]
	[PXProjection(typeof(Select<InventoryItem>), Persistent = false)]
	public class InventoryItemCommon: IBqlTable
	{
		#region Selected
		/// <inheritdoc cref="InventoryItem.Selected"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty [False alert, remove this suppression after the ATR-741 is fixed]
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		[PXFormula(typeof(False))]
		public virtual bool? Selected { get; set; }
		public abstract class selected : BqlBool.Field<selected> { }
		#endregion

		#region InventoryID
		/// <inheritdoc cref="InventoryItem.InventoryID"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty [False alert, remove this suppression after the ATR-741 is fixed]
		[PXDBInt(BqlField = typeof(InventoryItem.inventoryID))]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion

		#region InventoryCD
		/// <inheritdoc cref="InventoryItem.InventoryCD"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty [False alert, remove this suppression after the ATR-741 is fixed]
		[PXDBString(InputMask = "", IsUnicode = true, IsKey = true, BqlField = typeof(InventoryItem.inventoryCD))]
		[PXDimensionSelector(BaseInventoryAttribute.DimensionName, typeof(Search<InventoryItemCommon.inventoryCD>), typeof(InventoryItemCommon.inventoryCD))]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string InventoryCD { get; set; }
		public abstract class inventoryCD : BqlString.Field<inventoryCD> { }
		#endregion

		#region ItemStatus
		/// <inheritdoc cref="InventoryItem.ItemStatus"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty [False alert, remove this suppression after the ATR-741 is fixed]
		[PXDBString(2, IsFixed = true, BqlField = typeof(InventoryItem.itemStatus))]
		[InventoryItemStatus.List]
		public virtual string ItemStatus { get; set; }
		public abstract class itemStatus : BqlString.Field<itemStatus> { }
		#endregion

		#region ItemClassID
		/// <inheritdoc cref="InventoryItem.ItemClassID"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty [False alert, remove this suppression after the ATR-741 is fixed]
		[PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
		public virtual int? ItemClassID { get; set; }
		public abstract class itemClassID : BqlInt.Field<itemClassID> { }
		#endregion

		#region LotSerClassID
		/// <inheritdoc cref="InventoryItem.LotSerClassID"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty [False alert, remove this suppression after the ATR-741 is fixed]
		[PXDBString(10, IsUnicode = true, BqlField = typeof(InventoryItem.lotSerClassID))]
		public virtual string LotSerClassID { get; set; }
		public abstract class lotSerClassID : BqlString.Field<lotSerClassID> { }
		#endregion

		#region StkItem
		/// <inheritdoc cref="InventoryItem.StkItem"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty [False alert, remove this suppression after the ATR-741 is fixed]
		[PXDBBool(BqlField = typeof(InventoryItem.stkItem))]
		public virtual bool? StkItem { get; set; }
		public abstract class stkItem : BqlBool.Field<stkItem> { }
		#endregion

		#region IsTemplate
		/// <inheritdoc cref="InventoryItem.IsTemplate"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty [False alert, remove this suppression after the ATR-741 is fixed]
		[PXDBBool(BqlField = typeof(InventoryItem.isTemplate))]
		public virtual bool? IsTemplate { get; set; }
		public abstract class isTemplate : BqlBool.Field<isTemplate> { }
		#endregion

		#region NoteID
		[PXNote(BqlField = typeof(InventoryItem.noteID))]
		public virtual Guid? NoteID { get; set; }
		public abstract class noteID : BqlGuid.Field<noteID> { }
		#endregion
	}
}
