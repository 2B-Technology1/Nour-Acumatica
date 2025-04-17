//using System;
//using System.Collections.Generic;
//using System.Threading;
//using PX.Data;
//using PX.Objects.AR;

//namespace Nour20240918
//{
//  public class PointsForCustomerGraph : PXGraph<PointsForCustomerGraph>
//  {
//        [PXFilterable]
//        public PXProcessing<Customer> PointsForCustomer;

//        public PointsForCustomerGraph()
//        {
//            PointsForCustomer.SetProcessDelegate(ProcessInventory);
//        }

//        private void ProcessInventory(List<Customer> arg1, CancellationToken arg2)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}