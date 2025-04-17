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
using PX.Objects.AM.Attributes;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AM.Attributes
{
    [PXDBString(INLotSerialStatusByCostCenter.lotSerialNbr.Length, IsUnicode =true, InputMask ="")]
    [PXUIField(DisplayName = "Parent Lot/Serial Nbr.", FieldClass ="LotSerial")]
    [PXDefault("")]
    public class ParentLotSerialNbrAttribute : PXEntityAttribute//, IPXFieldVerifyingSubscriber
    {
        public ParentLotSerialNbrAttribute(Type ProdOrderType, Type ProdOrdIDType) : base()
        {
            var prodOrderType = BqlCommand.GetItemType(ProdOrderType);
            if (!typeof(ILSMaster).IsAssignableFrom(prodOrderType))
            {
                throw new PXArgumentException(nameof(itemType), IN.Messages.TypeMustImplementInterface, prodOrderType.GetLongName(), typeof(ILSMaster).GetLongName());
            }

            Type SearchType = BqlCommand.Compose(
                typeof(Search<,>),
                typeof(AMProdItemSplit.lotSerialNbr),
                typeof(Where<,,>),
                typeof(AMProdItemSplit.orderType),
                typeof(Equal<>),
                typeof(Optional<>),
                ProdOrderType,
                typeof(And<,>),
                typeof(AMProdItemSplit.prodOrdID),
                typeof(Equal<>),
                typeof(Optional<>),
                ProdOrdIDType
                );

            {
                PXSelectorAttribute attr = new PXSelectorAttribute(SearchType,
                                                                     typeof(AMProdItemSplit.lotSerialNbr),
                                                                     typeof(AMProdItemSplit.qty),
                                                                     typeof(AMProdItemSplit.qtyComplete),
                                                                     typeof(AMProdItemSplit.qtyScrapped),
                                                                     typeof(AMProdItemSplit.qtyRemaining));
                _Attributes.Add(attr);
                _SelAttrIndex = _Attributes.Count - 1;
            }
        }
    }
}
