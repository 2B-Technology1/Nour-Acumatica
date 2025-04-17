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
    /// Production events
    /// </summary>
    public class ProductionEventType
    {
        /// <summary>
        /// Generic/info/default production event
        /// </summary>
        public const int Info = 0;
        /// <summary>
        /// Production order created
        /// </summary>
        public const int OrderCreated = 1;
        /// <summary>
        /// Production order released
        /// </summary>
        public const int OrderReleased = 2;
        /// <summary>
        /// Production order set on hold
        /// </summary>
        public const int OrderPlaceOnHold = 3;
        /// <summary>
        /// Production order removed from Hold
        /// </summary>
        public const int OrderRemoveFromHold = 4;
        /// <summary>
        /// Production order completed
        /// </summary>
        public const int OrderCompleted = 5;
        /// <summary>
        /// Production order closed
        /// </summary>
        public const int OrderClosed = 6;
        /// <summary>
        /// Production order canceled
        /// </summary>
        public const int OrderCancelled = 7;
        /// <summary>
        /// Production order changed back to a Plan status
        /// </summary>
        public const int OrderResetToPlan = 8;
        /// <summary>
        /// Production order report printed
        /// </summary>
        public const int ReportPrinted = 9;
        /// <summary>
        /// Production transaction event
        /// </summary>
        public const int Transaction = 10;
        /// <summary>
        /// User added comment
        /// </summary>
        public const int Comment = 11;
        /// <summary>
        /// Production order was edited
        /// </summary>
        public const int OrderEdit = 15;
        /// <summary>
        /// Production order operations changed
        /// </summary>
        public const int OperationChange = 16;
		/// <summary>
		/// Description/labels for identifiers
		/// </summary>
		public const int Locked = 17;
		/// <summary>
		/// Production order locked
		/// </summary>
		public const int Unlocked = 18;
		/// <summary>
		/// Production order unlocked 
		/// </summary>

        public static string GetEventTypeDescription(int eventType)
        {
            switch (eventType)
            {
                case Info:
                    return Messages.GetLocal(Messages.Info);
                case Comment:
                    return Messages.GetLocal(Messages.Comment);
                case OrderCreated:
                    return Messages.GetLocal(Messages.OrderCreated);
                case OrderReleased:
                    return Messages.GetLocal(Messages.OrderReleased);
                case OrderPlaceOnHold:
                    return Messages.GetLocal(Messages.OrderPlaceOnHold);
                case OrderRemoveFromHold:
                    return Messages.GetLocal(Messages.OrderRemoveFromHold);
                case OrderCompleted:
                    return Messages.GetLocal(Messages.OrderCompleted);
                case OrderClosed:
                    return Messages.GetLocal(Messages.OrderClosed);
                case OrderCancelled:
                    return Messages.GetLocal(Messages.OrderCancelled);
                case OrderResetToPlan:
                    return Messages.GetLocal(Messages.OrderResetToPlan);
                case Transaction:
                    return Messages.GetLocal(Messages.Transaction);
                case ReportPrinted:
                    return Messages.GetLocal(Messages.ReportPrinted);
                case OperationChange:
                    return Messages.GetLocal(Messages.OperationChange);
				case Locked:
					return Messages.GetLocal(Messages.Locked);
				case Unlocked:
					return Messages.GetLocal(Messages.Unlocked);
			}

            return string.Empty;
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                new[] {
                      Info,
                      Comment,
                      OrderCreated,
                      OrderReleased,
                      OrderPlaceOnHold,
                      OrderRemoveFromHold,
                      OrderCompleted,
                      OrderClosed,
                      OrderCancelled,
                      OrderResetToPlan,
                      ReportPrinted,
                      Transaction,
                      OrderEdit,
                      OperationChange,
					  Locked,
					  Unlocked}, 
                new[] {
                    Messages.Info,
                    Messages.Comment,
                    Messages.Created,
                    Messages.Released,
                    Messages.OnHold,
                    Messages.HoldRemoved,
                    Messages.Completed,
                    Messages.Closed,
                    Messages.Canceled,
                    Messages.ResetToPlan,
                    Messages.ReportPrinted,
                    Messages.Transaction,
                    Messages.OrderEdit,
                    Messages.OperationChange,
					Messages.Locked,
					Messages.Unlocked
				}) { }
        }

        public class info : PX.Data.BQL.BqlInt.Constant<info> { public info() : base(Info){} }

        public class comment : PX.Data.BQL.BqlInt.Constant<comment> { public comment() : base(Comment) { } }

        public class orderCreated : PX.Data.BQL.BqlInt.Constant<orderCreated> { public orderCreated() : base(OrderCreated) { } }

        public class orderReleased : PX.Data.BQL.BqlInt.Constant<orderReleased> { public orderReleased() : base(OrderReleased) { } }

        public class orderPlaceOnHold : PX.Data.BQL.BqlInt.Constant<orderPlaceOnHold> { public orderPlaceOnHold() : base(OrderPlaceOnHold) { } }

        public class orderRemoveFromHold : PX.Data.BQL.BqlInt.Constant<orderRemoveFromHold> { public orderRemoveFromHold() : base(OrderRemoveFromHold) { } }

        public class orderCompleted : PX.Data.BQL.BqlInt.Constant<orderCompleted> { public orderCompleted() : base(OrderCompleted) { } } 

        public class orderClosed : PX.Data.BQL.BqlInt.Constant<orderClosed> { public orderClosed() : base(OrderClosed) { } }

        public class orderCancelled : PX.Data.BQL.BqlInt.Constant<orderCancelled> { public orderCancelled() : base(OrderCancelled) { } }

        public class orderResetToPlan : PX.Data.BQL.BqlInt.Constant<orderResetToPlan> { public orderResetToPlan() : base(OrderResetToPlan) { } }

        public class reportPrinted : PX.Data.BQL.BqlInt.Constant<reportPrinted> { public reportPrinted() : base(ReportPrinted) { } }

        public class transaction : PX.Data.BQL.BqlInt.Constant<transaction> { public transaction() : base(Transaction) { } } 

        public class orderEdit : PX.Data.BQL.BqlInt.Constant<orderEdit> { public orderEdit() : base(OrderEdit) { } }

        public class operationChange : PX.Data.BQL.BqlInt.Constant<operationChange> { public operationChange() : base(OperationChange) { } }
    }
}
