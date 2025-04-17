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
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AM.Attributes
{
    public class AMVendorShipLotSerialNbrAttribute : AMLotSerialNbrAttribute
    {
        public AMVendorShipLotSerialNbrAttribute(Type LineType, Type OrderTypeType, Type ProdOrderType, Type InventoryType, Type SubItemType, Type LocationType) : base(InventoryType, SubItemType, LocationType)
        {
            Type selType = typeof(Search2<,,>);
            Type field = typeof(INLotSerialStatusByCostCenter.lotSerialNbr);
            Type join = typeof(InnerJoin<AMProdItem, On<INLotSerialStatusByCostCenter.inventoryID, Equal<AMProdItem.inventoryID>,
					And<INLotSerialStatusByCostCenter.costCenterID, Equal<CostCenter.freeStock>>>,
                LeftJoin<AMProdItemSplit, On<AMProdItem.orderType, Equal<AMProdItemSplit.orderType>,
                And<AMProdItem.prodOrdID, Equal<AMProdItemSplit.prodOrdID>
                , And<Sub<AMProdItemSplit.qty, AMProdItemSplit.qtyComplete>, Greater<decimal0>
                , And<AMProdItemSplit.lotSerialNbr, Equal<INLotSerialStatusByCostCenter.lotSerialNbr>>
                >
                >>>>);
            Type where = BqlCommand.Compose(typeof(Where<,,>), typeof(AMProdItem.orderType), typeof(Equal<>), typeof(Optional<>), OrderTypeType
                ,
                typeof(And<,,>), typeof(AMProdItem.prodOrdID), typeof(Equal<>), typeof(Optional<>), ProdOrderType
            ,
            typeof(And2<,>),
                typeof(Where2<,>),
                    typeof(Where<,,>),
                        typeof(AMProdItem.preassignLotSerial), typeof(Equal<>), typeof(boolFalse),
                        typeof(Or<,,>), typeof(AMShipLineType.material), typeof(Equal<>), typeof(Optional<>), LineType,
                    typeof(Or<,,>),
                        typeof(AMProdItemSplit.lotSerialNbr), typeof(IsNotNull)
            );

            var SearchType = BqlCommand.Compose(selType, field, join, where);

            PXSelectorAttribute attr = new PXSelectorAttribute(SearchType,
                                                    typeof(INLotSerialStatusByCostCenter.inventoryID),
                                                        typeof(INLotSerialStatusByCostCenter.lotSerialNbr),
                                                        typeof(INLotSerialStatusByCostCenter.siteID),
                                                        typeof(INLotSerialStatusByCostCenter.locationID),
                                                        typeof(INLotSerialStatusByCostCenter.qtyOnHand),
                                                        typeof(INLotSerialStatusByCostCenter.qtyAvail),
                                                        typeof(INLotSerialStatusByCostCenter.expireDate));
            _Attributes.Add(attr);
            _SelAttrIndex = _Attributes.Count - 1;



        }
    }
}
