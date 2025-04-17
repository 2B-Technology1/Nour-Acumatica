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
using PX.Data;

namespace PX.Objects.AM
{
    public class GITransactionsByProductionOrder : AMGenericInquiry
    {
        protected const string GIID = "91d4d726-cbcd-42a5-a12b-d5f52986e4ef";

        public GITransactionsByProductionOrder()
            : base(new Guid(GIID))
        {
        }

        /// <summary>
        /// Set the GI call to filter for a specific production order
        /// </summary>
        public virtual void SetFilterByProductionOrder(string orderType, string prodOrdId)
        {
            AddFilter(typeof(AMProdItem), typeof(AMProdItem.orderType), PXCondition.EQ, orderType);
            AddFilter(typeof(AMProdItem), typeof(AMProdItem.prodOrdID), PXCondition.EQ, prodOrdId);
        }

        /// <summary>
        /// Set the GI call to filter for a specific production order status
        /// </summary>
        public virtual void SetFilterByProductionStatus(string status)
        {
            AddFilter(typeof(AMProdItem), typeof(AMProdItem.statusID), PXCondition.EQ, status);
        }

        /// <summary>
        /// Set the GI call to filter for showing only unreleased batches
        /// </summary>
        public virtual void SetFilterByUnreleasedBatches()
        {
            AddFilter(typeof(AMBatch), typeof(AMBatch.released), PXCondition.EQ, false);
        }
    }
}