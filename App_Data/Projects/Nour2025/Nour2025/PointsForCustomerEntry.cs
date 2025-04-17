using System;
using System.Collections.Generic;
using System.Threading;
using PX.Data;
using PX.Data.Licensing;
using PX.Objects.AR;

namespace Nour20240918
{
  public class PointsForCustomerEntry : PXGraph<PointsForCustomerEntry>
  {

        //[PXFilterable]
        //public PXProcessing<Customer> PointsForCustomer;
        [PXFilterable]
        public PXProcessingJoin<Customer,
            InnerJoin<BAccount, On<Customer.bAccountID, Equal<BAccount.bAccountID>>>,
            Where<Customer.bAccountID, IsNotNull>> PointsForCustomer;
        public PointsForCustomerEntry()
        {
            PointsForCustomer.SetProcessDelegate(ProcessInventory);
        }

        private void ProcessInventory(List<Customer> arg1, CancellationToken arg2)
        {
            throw new NotImplementedException();
        }
    }
}