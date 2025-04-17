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
    /// MRP Process Exception Types
    /// </summary>
    public class MRPExceptionType
    {
        /// <summary>
        /// Defer - Supply due prior to the promise date
        /// </summary>
        public const string Defer = "1";

        /// <summary>
        /// Delete - Supply not required for any demand
        /// </summary>
        public const string Delete = "2";

        /// <summary>
        /// Expedite - Supply due beyond the promise date 
        /// </summary>
        public const string Expedite = "3";

        /// <summary>
        /// Late - Supply item should have been received
        /// </summary>
        public const string Late = "4";

        /// <summary>
        /// Transfer Available - Supply item has inventory beyond site requirements
        /// </summary>
        public const string Transfer = "5";

        /// <summary>
        /// Order on Hold - The order is not included in the MRP Processing due to the order being on hold and MRP Setup option excluding the order type when on hold
        /// </summary>
        public const string OrderOnHold = "6";

		/// <summary>
		/// When a Transfer item does not contain a Replenishment Warehouse for planned transfer orders
		/// </summary>
		public const string NoTRReplenishmentWarehouse = "7";

		/// <summary>
		/// When a Transfer item contains a set of item warehouse detail Replenishment Warehouses which are configured in a way creating a recursive condition
		/// </summary>
		public const string ReplenishmentWarehouseLoop = "8";


		/// <summary>
		/// Order With out Schedule Date  - 
		/// </summary>
		public const string OrderWithoutSchedDate = "9";

        public class defer : PX.Data.BQL.BqlString.Constant<defer>
        {
            public defer() : base(Defer) { }
        }
        public class delete : PX.Data.BQL.BqlString.Constant<delete>
        {
            public delete() : base(Delete) { }
        }
        public class expedite : PX.Data.BQL.BqlString.Constant<expedite>
        {
            public expedite() : base(Expedite) { }
        }
        public class late : PX.Data.BQL.BqlString.Constant<late>
        {
            public late() : base(Late) { }
        }
        public class transfer : PX.Data.BQL.BqlString.Constant<transfer>
        {
            public transfer() : base(Transfer) { }
        }
        public class orderOnHold : PX.Data.BQL.BqlString.Constant<orderOnHold>
        {
            public orderOnHold() : base(OrderOnHold) { }
        }

		public class noTRReplenishmentWarehouse : PX.Data.BQL.BqlString.Constant<noTRReplenishmentWarehouse>
		{
			public noTRReplenishmentWarehouse() : base(NoTRReplenishmentWarehouse) { }
		}

		public class replenishmentWarehouseLoop : PX.Data.BQL.BqlString.Constant<replenishmentWarehouseLoop>
		{
			public replenishmentWarehouseLoop() : base(ReplenishmentWarehouseLoop) { }
		}

		public class orderWithoutSchedDate : PX.Data.BQL.BqlString.Constant<orderWithoutSchedDate>
		{
			public orderWithoutSchedDate() : base(OrderWithoutSchedDate) { }
		}

		public class ListAttribute : PXStringListAttribute
        {
			public ListAttribute()
					: base(
						new[]
						{
				Defer,
				Delete,
				Expedite,
				Late,
				Transfer,
				OrderOnHold,
                OrderWithoutSchedDate,
                NoTRReplenishmentWarehouse,
				ReplenishmentWarehouseLoop
						},
						new[]
						{
				Messages.MRPExceptionTypeDefer,
				EP.Messages.Delete,
				Messages.MRPExceptionTypeExpedite,
				Messages.MRPExceptionTypeLate,
				Messages.MRPExceptionTypeTransfer,
				Messages.MRPExceptionTypeOrderOnHold,                
                Messages.MRPExceptionTypeOrderWithoutSchedDate,
                Messages.TransferWithoutReplenishmentWarehouse,
				Messages.ReplenishmentWarehouseLoop
						})
			{
			}
		}
    }
}
