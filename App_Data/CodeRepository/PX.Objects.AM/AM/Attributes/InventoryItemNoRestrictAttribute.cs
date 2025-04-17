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
using System;
using PX.Objects.CS;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Inventory item field/selector with no PX restrictions for item status.
    /// There is a need to have the InventoryItemAttribute without the item status restriction.
    /// </summary>
    [PXDBInt]
    [PXUIField(DisplayName = "Inventory ID")]
    public class InventoryItemNoRestrictAttribute : PXEntityAttribute
    {
        public const string DimensionName = "INVENTORY";

        public class dimensionName : PX.Data.BQL.BqlString.Constant<dimensionName>
        {
            public dimensionName() : base(DimensionName) {; }
        }

        public InventoryItemNoRestrictAttribute()
			: this(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
        }

        public InventoryItemNoRestrictAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base()
		{
            PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, SubstituteKey);
            attr.CacheGlobal = true;
            attr.DescriptionField = DescriptionField;
            _Attributes.Add(attr);
            _SelAttrIndex = _Attributes.Count - 1;
        }
    }

    /// <summary>
    /// Stock item field/selector with no PX restrictions for item status.
    /// There is a need to have the InventoryItemAttribute without the item status restriction.
    /// </summary>
    [PXDBInt]
    [PXRestrictor(typeof(Where<InventoryItem.stkItem, Equal<boolTrue>>), PX.Objects.IN.Messages.InventoryItemIsNotAStock)]
    [PXUIField(DisplayName = "Inventory ID")]
    public class StockItemNoRestrictAttribute : InventoryItemNoRestrictAttribute
    {

    }
}
