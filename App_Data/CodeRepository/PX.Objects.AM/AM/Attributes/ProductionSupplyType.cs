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
    /// Prodction Supply Types 
    /// </summary>
    public class ProductionSupplyType
    {
        /// <summary>
        /// Inventory
        /// </summary>
        public const int Inventory = 1;
        /// <summary>
        /// Production
        /// </summary>
        public const int Production = 2;
        /// <summary>
        /// SalesOrder
        /// </summary>
        public const int SalesOrder = 3;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            /// <summary>
            /// Inventory
            /// </summary>
            public static string Inventory => Messages.GetLocal(Messages.Inventory);

            /// <summary>
            /// Production
            /// </summary>
            public static string Production => Messages.GetLocal(Messages.Production);

            /// <summary>
            /// SalesOrder
            /// </summary>
            public static string SalesOrder => Messages.GetLocal(Messages.SalesOrder);
        }

        /// <summary>
        /// Production Supply Type Inventory
        /// </summary>
        public class inventory : PX.Data.BQL.BqlInt.Constant<inventory>
        {
            public inventory() : base(Inventory) { }
        }
        /// <summary>
        /// Production Supply Type Production
        /// </summary>
        public class production : PX.Data.BQL.BqlInt.Constant<inventory>
        {
            public production() : base(Production) { }
        }

        /// <summary>
        /// Production Supply Type Sales Order
        /// </summary>
        public class salesOrder : PX.Data.BQL.BqlInt.Constant<salesOrder>
        {
            public salesOrder() : base(SalesOrder) { }
        }

        /// <summary>
        /// List for Production Supply Types
        /// </summary>
        public class SupplyTypeListAttribute : PXIntListAttribute
        {
            public SupplyTypeListAttribute()
                : base(
                new int[] { Inventory, Production, SalesOrder },
                new string[] { Messages.Inventory, Messages.Production, Messages.SalesOrder })
            { }
        }
    }
}