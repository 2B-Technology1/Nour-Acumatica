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
    public class OrderCrossRefProcessSource
    {
        public const int ProductionMaint = 1;
        public const int MRP = 2;
        public const int SalesOrder = 3;
        public const int CriticalMaterial = 4;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string ProductionMaint => Messages.GetLocal(Messages.ProductionMaint);
            public static string MRP => Messages.GetLocal(Messages.MRP);
            public static string SalesOrder => Messages.GetLocal(Messages.SalesOrder);
            public static string CriticalMaterial => Messages.GetLocal(Messages.CriticalMaterial);
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(new int[]
			{
			    ProductionMaint, 
                MRP, 
                SalesOrder,
                CriticalMaterial
			},
                new string[]
			{
                Messages.ProductionMaint,
                Messages.MRP,
                Messages.SalesOrder,
                Messages.CriticalMaterial
			})
            {
            }
        }
    }
}