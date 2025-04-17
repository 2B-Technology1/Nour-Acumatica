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
    /// Order Source Types for Configurations
    /// </summary>
    public class OrderSource
    {
        /// <summary>
        /// None
        /// </summary>
        public const int None = 0;
        /// <summary>
        /// Sales Order
        /// </summary>
        public const int SalesOrder = 1;
        /// <summary>
        /// Opportunity
        /// </summary>
        public const int Opportunity = 2;

        /// <summary>
        /// Descriptions/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string None => Messages.GetLocal(Messages.None);
            public static string SalesOrder => Messages.GetLocal(Messages.SalesOrder);
            public static string Opportunity => PX.Objects.CR.Messages.GetLocal(CR.Messages.Opportunity);
        }

        public class none : PX.Data.BQL.BqlInt.Constant<none>
        {
            public none() : base(None) { }
        }

        public class salesOrder : PX.Data.BQL.BqlInt.Constant<salesOrder>
        {
            public salesOrder() : base(SalesOrder) { }
        }

        public class opportunity : PX.Data.BQL.BqlInt.Constant<opportunity>
        {
            public opportunity() : base(Opportunity) { }
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                new int[] { None, SalesOrder, Opportunity },
                new string[] { Messages.None, Messages.SalesOrder, CR.Messages.Opportunity })
            { }
        }
    }
}