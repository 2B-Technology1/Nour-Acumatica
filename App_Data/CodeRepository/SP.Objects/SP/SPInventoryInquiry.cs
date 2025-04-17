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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.IN;
using PX.TM;

namespace SP.Objects.SP
{
    public class SPInventoryInquiry : PXGraph<SPInventoryInquiry>
    {
        #region Filter
        [System.SerializableAttribute()]
        public partial class InventoryFilter : IBqlTable
        {
            #region Find Item
            public abstract class findItem : PX.Data.IBqlField
            {
            }
            protected String _FindItem;
            [PXString()]
            [PXUIField(DisplayName = "Find Item")]
            public virtual String FindItem
            {
                get { return this._FindItem; }
                set { _FindItem = value; }
            }
            #endregion
        }
        #endregion

        #region Select
        public PXFilter<InventoryFilter> Filter;

        public PXSelect<InventoryItem,
            Where<InventoryItem.inventoryCD, Like<Current<InventoryFilter.findItem>>,
                Or<Current<InventoryFilter.findItem>, IsNull>>>
            FilteredItems;

        /*public PXSelect<InventoryItem>
            FilteredItems;*/
        #endregion
    }
}
