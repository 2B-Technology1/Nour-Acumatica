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
using PX.Data.BQL.Fluent;

namespace PX.Objects.IN.DAC.Unbound
{
	[PXCacheName(Messages.MassConvertStockNonStockFilter)]
	public class MassConvertStockNonStockFilter : IBqlTable
	{
		#region Action
		public abstract class action : Data.BQL.BqlString.Field<action>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					new string[] { None, ConvertStockToNonStock, ConvertNonStockToStock },
					new string[] { DisplayNames.None, DisplayNames.ConvertStockToNonStock, DisplayNames.ConvertNonStockToStock })
				{ }
			}

			public const string None = "_";
			public const string ConvertStockToNonStock = "S";
			public const string ConvertNonStockToStock = "N";

			[PXLocalizable]
			public abstract class DisplayNames
			{
				public const string None = "<SELECT>";
				public const string ConvertStockToNonStock = "Convert Stock to Non-Stock";
				public const string ConvertNonStockToStock = "Convert Non-Stock to Stock";
			}

			public class convertStockToNonStock : Data.BQL.BqlString.Constant<convertStockToNonStock>
			{
				public convertStockToNonStock() : base(ConvertStockToNonStock) {; }
			}
			public class convertNonStockToStock : Data.BQL.BqlString.Constant<convertNonStockToStock>
			{
				public convertNonStockToStock() : base(ConvertNonStockToStock) {; }
			}
		}
		[PXDBString(1)]
		[PXUIField(DisplayName = "Action")]
		[action.List]
		[PXDefault(action.None)]
		public virtual string Action
		{
			get;
			set;
		}
		#endregion

		#region StkItem
		public abstract class stkItem : Data.BQL.BqlBool.Field<stkItem> { }
		[PXDBBool]
		[PXFormula(typeof(Switch<Case<Where<action, Equal<action.convertStockToNonStock>>, True,
			Case<Where<action, Equal<action.convertNonStockToStock>>, False>>,
			Null>))]
		public virtual bool? StkItem
		{
			get;
			set;
		}
		#endregion
		#region ItemClassID
		public abstract class itemClassID : Data.BQL.BqlInt.Field<itemClassID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Item Class")]
		[PXDimensionSelector(INItemClass.Dimension,
			typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<Current<stkItem>>>>),
			typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
		[PXFormula(typeof(Default<stkItem>))]
		public virtual int? ItemClassID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID> { }
		[AnyInventory(
			typeof(Search<InventoryItem.inventoryID,
				Where<InventoryItem.stkItem, Equal<Current<stkItem>>,
				And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>,
				And<InventoryItem.isTemplate, Equal<False>,
				And<InventoryItem.templateItemID, IsNull,
				And<InventoryItem.kitItem, Equal<False>,
				And2<Where<InventoryItem.stkItem, Equal<True>,
					Or<InventoryItem.nonStockReceipt, Equal<True>, And<InventoryItem.nonStockShip, Equal<True>, And<InventoryItem.itemType, Equal<INItemTypes.nonStockItem>>>>>,
				And<MatchUser>>>>>>>>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr),
			DisplayName = "Inventory ID")]
		[PXFormula(typeof(Default<stkItem>))]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion

		#region NonStockItemClassID
		public abstract class nonStockItemClassID : Data.BQL.BqlInt.Field<nonStockItemClassID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Item Class")]
		[PXDimensionSelector(INItemClass.Dimension,
			typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<False>>>),
			typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
		public virtual int? NonStockItemClassID
		{
			get;
			set;
		}
		#endregion
		#region NonStockPostClassID
		public abstract class nonStockPostClassID : Data.BQL.BqlString.Field<nonStockPostClassID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Posting Class")]
		[PXSelector(typeof(INPostClass.postClassID), DescriptionField = typeof(INPostClass.descr))]
		[PXDefault(typeof(SelectFrom<INItemClass>.Where<INItemClass.itemClassID.IsEqual<nonStockItemClassID.FromCurrent>>), SourceField = typeof(INItemClass.postClassID), CacheGlobal = true)]
		[PXFormula(typeof(Default<nonStockItemClassID>))]
		public virtual string NonStockPostClassID
		{
			get;
			set;
		}
		#endregion

		#region StockItemClassID
		public abstract class stockItemClassID : Data.BQL.BqlInt.Field<stockItemClassID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Item Class")]
		[PXDimensionSelector(INItemClass.Dimension,
			typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<True>>>),
			typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
		public virtual int? StockItemClassID
		{
			get;
			set;
		}
		#endregion
		#region StockPostClassID
		public abstract class stockPostClassID : Data.BQL.BqlString.Field<stockPostClassID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Posting Class")]
		[PXSelector(typeof(INPostClass.postClassID), DescriptionField = typeof(INPostClass.descr))]
		[PXDefault(typeof(SelectFrom<INItemClass>.Where<INItemClass.itemClassID.IsEqual<stockItemClassID.FromCurrent>>), SourceField = typeof(INItemClass.postClassID), CacheGlobal = true)]
		[PXFormula(typeof(Default<stockItemClassID>))]
		public virtual string StockPostClassID
		{
			get;
			set;
		}
		#endregion
		#region StockValMethod
		public abstract class stockValMethod : Data.BQL.BqlString.Field<stockValMethod> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Valuation Method")]
		[INValMethod.List]
		[PXDefault(INValMethod.Standard, typeof(SelectFrom<INItemClass>.Where<INItemClass.itemClassID.IsEqual<stockItemClassID.FromCurrent>>), SourceField = typeof(INItemClass.valMethod), CacheGlobal = true)]
		[PXFormula(typeof(Default<stockItemClassID>))]
		public virtual string StockValMethod
		{
			get;
			set;
		}
		#endregion
		#region StockLotSerClassID
		public abstract class stockLotSerClassID : Data.BQL.BqlString.Field<stockLotSerClassID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(INLotSerClass.lotSerClassID), DescriptionField = typeof(INLotSerClass.descr), CacheGlobal = true)]
		[PXUIField(DisplayName = "Lot/Serial Class", FieldClass = "LotSerial")]
		[PXDefault(typeof(SelectFrom<INItemClass>.Where<INItemClass.itemClassID.IsEqual<stockItemClassID.FromCurrent>>), SourceField = typeof(INItemClass.lotSerClassID), CacheGlobal = true)]
		[PXFormula(typeof(Default<stockItemClassID>))]
		public virtual string StockLotSerClassID
		{
			get;
			set;
		}
		#endregion
	}
}
