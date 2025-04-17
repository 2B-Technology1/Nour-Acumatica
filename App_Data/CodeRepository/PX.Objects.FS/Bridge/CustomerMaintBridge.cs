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
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    // TODO: AC-137974 Delete this graph
    public class CustomerMaintBridge : PXGraph<CustomerMaintBridge, FSCustomer>
    {
        #region Selects
        public PXSelect<FSCustomer, 
               Where2<
                   Match<Current<AccessInfo.userName>>, 
               And<
                   Where<
                       FSCustomer.type, Equal<BAccountType.customerType>, 
                       Or<FSCustomer.type, Equal<BAccountType.combinedType>>>>>> Customers;
        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowSelected<FSCustomer> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSCustomer fsCustomerRow = e.Row;

            var graph = PXGraph.CreateInstance<CustomerMaint>();

            if (fsCustomerRow.BAccountID >= 0)
            {
                graph.BAccount.Current = graph.BAccount.Search<Customer.bAccountID>(fsCustomerRow.BAccountID);
            }

            throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.Same };
        }
        #endregion
    }
}
