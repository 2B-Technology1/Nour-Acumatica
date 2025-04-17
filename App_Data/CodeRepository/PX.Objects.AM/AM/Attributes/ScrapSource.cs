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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Production Scrap Source 
    /// </summary>
    public class ScrapSource
    {
        /// <summary>
        /// None
        /// </summary>
        public const int None = 0;
        /// <summary>
        /// Item -- Item Warehouse Details
        /// </summary>
        public const int Item = 1;
        /// <summary>
        /// Warehouse -- Order Warehouse
        /// </summary>
        public const int Warehouse = 2;
        /// <summary>
        /// OrderType -- Order Type
        /// </summary>
        public const int OrderType = 3;
        
        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            /// <summary>
            /// None
            /// </summary>
            public static string None => Messages.GetLocal(Messages.None);

            /// <summary>
            /// Item
            /// </summary>
            public static string Item => Messages.GetLocal(Messages.Item);

            /// <summary>
            /// Warehouse
            /// </summary>
            public static string Warehouse => Messages.GetLocal(Messages.Warehouse);

            /// <summary>
            /// Order Type
            /// </summary>
            public static string OrderType => Messages.GetLocal(Messages.OrderType);
        }


        /// <summary>
        /// None
        /// </summary>
        public class none : PX.Data.BQL.BqlInt.Constant<none>
        {
            public none() : base(None) {; }
        }
        /// <summary>
        /// Item
        /// </summary>
        public class item : PX.Data.BQL.BqlInt.Constant<item>
        {
            public item() : base(Item) {; }
        }
        /// <summary>
        /// Warehouse
        /// </summary>
        public class warehouse : PX.Data.BQL.BqlInt.Constant<warehouse>
        {
            public warehouse() : base(Warehouse) {; }
        }
        /// <summary>
        /// OrderType
        /// </summary>
        public class orderType : PX.Data.BQL.BqlInt.Constant<orderType>
        {
            public orderType() : base(OrderType) {; }
        }

        /// <summary>
        /// List for Production Scrap Source
        /// </summary>
        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                new int[] { None, Item, Warehouse, OrderType },
                new string[] { Messages.None, Messages.Item, Messages.Warehouse, Messages.OrderType})
            { }
        }
    }
}