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
using PX.SM;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class ServiceOrderProcess : PXGraph<ServiceOrderProcess>
    {
        #region Delegate
        public ServiceOrderProcess()
        {
            ServiceOrderProcess graphServiceOrderProcess;

            ServiceOrderRecords.SetProcessDelegate(
                delegate(List<FSServiceOrder> fsServiceOrderRowList)
                {
                    graphServiceOrderProcess = PXGraph.CreateInstance<ServiceOrderProcess>();

                    ServiceOrderEntry graphServiceOrderEntry = PXGraph.CreateInstance<ServiceOrderEntry>();

                    int index = 0;

                    foreach (FSServiceOrder fsServiceOrderRow in fsServiceOrderRowList)
                    {
                        try
                        {
                            graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords
                                                                                 .Search<FSServiceOrder.refNbr>(fsServiceOrderRow.RefNbr, fsServiceOrderRow.SrvOrdType);

                            switch (Filter.Current.SOAction)
                            {
                                case ID.ServiceOrder_Action_Filter.COMPLETE:
                                    graphServiceOrderEntry.completeOrder.Press();
                                    break;
                                case ID.ServiceOrder_Action_Filter.CANCEL:
                                    graphServiceOrderEntry.cancelOrder.Press();
                                    break;
                                case ID.ServiceOrder_Action_Filter.REOPEN:
                                    graphServiceOrderEntry.reopenOrder.Press();
                                    break;
                                case ID.ServiceOrder_Action_Filter.CLOSE:
                                    graphServiceOrderEntry.closeOrder.Press();
                                    break;
                                case ID.ServiceOrder_Action_Filter.UNCLOSE:
                                    graphServiceOrderEntry.uncloseOrder.Press();
                                    break;
                                case ID.ServiceOrder_Action_Filter.ALLOWINVOICE:
                                    graphServiceOrderEntry.allowBilling.Press();
                                    break;
                            }

                            PXProcessing<FSServiceOrder>.SetInfo(index, TX.Messages.RECORD_PROCESSED_SUCCESSFULLY);
                        }
                        catch (Exception e)
                        {
                            PXProcessing<FSServiceOrder>.SetError(index, e);
                        }

                        index++;
                    }
                });
        }
        #endregion

        #region Filter+Select
        public PXFilter<ServiceOrderFilter> Filter;
        public PXCancel<ServiceOrderFilter> Cancel;

        [PXFilterable]
        [PXViewDetailsButton(typeof(ServiceOrderFilter))]
        public PXFilteredProcessingJoin<FSServiceOrder, ServiceOrderFilter,
                    LeftJoinSingleTable<Customer,
                        On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>,
                Where2<
                    Where<CurrentValue<ServiceOrderFilter.soAction>, NotEqual<ServiceOrderFilter.soAction.Undefined>>,
                And2<
                    Where<CurrentValue<ServiceOrderFilter.srvOrdType>, IsNull,
                    Or<FSServiceOrder.srvOrdType, Equal<CurrentValue<ServiceOrderFilter.srvOrdType>>>>,
                And2<
                    Where<CurrentValue<ServiceOrderFilter.branchID>, IsNull,
                    Or<FSServiceOrder.branchID, Equal<CurrentValue<ServiceOrderFilter.branchID>>>>,
                And2<
                    Where<CurrentValue<ServiceOrderFilter.branchLocationID>, IsNull,
                    Or<FSServiceOrder.branchLocationID, Equal<CurrentValue<ServiceOrderFilter.branchLocationID>>>>,
                And2<
                    Where<CurrentValue<ServiceOrderFilter.status>, IsNull,
                    Or<FSServiceOrder.status, Equal<CurrentValue<ServiceOrderFilter.status>>>>,
                And2<
                    Where<CurrentValue<ServiceOrderFilter.customerID>, IsNull,
                    Or<FSServiceOrder.customerID, Equal<CurrentValue<ServiceOrderFilter.customerID>>>>,
                And2<
                    Where<CurrentValue<ServiceOrderFilter.serviceContractID>, IsNull,
                    Or<FSServiceOrder.serviceContractID, Equal<CurrentValue<ServiceOrderFilter.serviceContractID>>>>,
                And2<
                    Where<CurrentValue<ServiceOrderFilter.fromDate>, IsNull,
                    Or<FSServiceOrder.orderDate, GreaterEqual<CurrentValue<ServiceOrderFilter.fromDate>>>>,
                And2<
                    Where<CurrentValue<ServiceOrderFilter.toDate>, IsNull,
                    Or<FSServiceOrder.orderDate, LessEqual<CurrentValue<ServiceOrderFilter.toDate>>>>,
                    And2<
                        Where<Customer.bAccountID, IsNull,
                        Or<Match<Customer, Current<AccessInfo.userName>>>>,
                And<
                        Where<
                            Where2<
                                Where<CurrentValue<ServiceOrderFilter.soAction>, Equal<ServiceOrderFilter.soAction.Complete>,
                                And<FSServiceOrder.openDoc, Equal<True>>>,
                    Or2<
                                Where<CurrentValue<ServiceOrderFilter.soAction>, Equal<ServiceOrderFilter.soAction.Cancel>,
                                And<FSServiceOrder.openDoc, Equal<True>>>,
                    Or2<
                                Where<CurrentValue<ServiceOrderFilter.soAction>, Equal<ServiceOrderFilter.soAction.Reopen>,
                        And<
                                Where<FSServiceOrder.canceled, Equal<True>,
                                    Or<
                                        Where<FSServiceOrder.completed, Equal<True>,
                                        And<FSServiceOrder.closed, Equal<False>>>>>>>,
                    Or2<
                                Where<CurrentValue<ServiceOrderFilter.soAction>, Equal<ServiceOrderFilter.soAction.Close>,
                                And<FSServiceOrder.completed, Equal<True>,
                                And<FSServiceOrder.closed, Equal<False>>>>,
                    Or2<
                                Where<CurrentValue<ServiceOrderFilter.soAction>, Equal<ServiceOrderFilter.soAction.Unclose>,
                                And<FSServiceOrder.closed, Equal<True>,
                                And<FSServiceOrder.completed, Equal<True>>>>,
                    Or<
                                Where<CurrentValue<ServiceOrderFilter.soAction>, Equal<ServiceOrderFilter.soAction.AllowInvoice>,
                                And<FSServiceOrder.allowInvoice, Equal<False>>>>>>>>>>>>>>>>>>>>>> ServiceOrderRecords;
        #endregion
    }
}