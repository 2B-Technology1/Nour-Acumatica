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
using System.Collections;

namespace PX.Objects.FS
{
    public class ProcessServiceContracts : PXGraph<ProcessServiceContracts>
    {
        public ServiceContractEntry graphServiceContractEntry;
        public RouteServiceContractEntry graphRouteServiceContractEntry;

        #region CacheAttached
        #region FSServiceContract_CustomerID
        [PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSServiceContract_CustomerID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSServiceContract_CustomerLocationID
        [PXUIField(DisplayName = "Location ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSServiceContract_CustomerLocationID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        public ProcessServiceContracts()
        {
            ProcessServiceContracts graphServiceContractStatusProcess = null;

            ServiceContracts.SetProcessDelegate(
                delegate (FSServiceContract fsServiceContractRow)
                {
                    if (graphServiceContractStatusProcess == null)
                    {
                        graphServiceContractStatusProcess = PXGraph.CreateInstance<ProcessServiceContracts>();
                        graphServiceContractStatusProcess.graphServiceContractEntry = PXGraph.CreateInstance<ServiceContractEntry>();
                        graphServiceContractStatusProcess.graphServiceContractEntry.skipStatusSmartPanels = true;
                        graphServiceContractStatusProcess.graphRouteServiceContractEntry = PXGraph.CreateInstance<RouteServiceContractEntry>();
                        graphServiceContractStatusProcess.graphRouteServiceContractEntry.skipStatusSmartPanels = true;
                    }

                    switch (this.Filter.Current.ActionType)
                    {
                        case ID.ActionType_ProcessServiceContracts.STATUS:
                            graphServiceContractStatusProcess.processServiceContract(graphServiceContractStatusProcess, fsServiceContractRow);
                            break;
                        case ID.ActionType_ProcessServiceContracts.PERIOD:
                            graphServiceContractStatusProcess.ProcessContractPeriod(graphServiceContractStatusProcess, fsServiceContractRow);
                            break;
						case ID.ActionType_ProcessServiceContracts.RENEW:
							graphServiceContractStatusProcess.processServiceContract(graphServiceContractStatusProcess, fsServiceContractRow, true);
							break;
						case ID.ActionType_ProcessServiceContracts.FORECAST:
							graphServiceContractStatusProcess.ProcessContractForecast(graphServiceContractStatusProcess, fsServiceContractRow);
							break;
						case ID.ActionType_ProcessServiceContracts.EMAILQUOTE:
							graphServiceContractStatusProcess.ProcessContractEmailQuote(graphServiceContractStatusProcess, fsServiceContractRow);
							break;
						default:
                            break;
                    }
                });
        }

		public virtual void processServiceContract(ProcessServiceContracts graphServiceContractStatusProcess, FSServiceContract fsServiceContractRow, bool isRenew = false)
        {
            if (fsServiceContractRow.RecordType == ID.RecordType_ServiceContract.SERVICE_CONTRACT)
            {
                graphServiceContractStatusProcess.graphServiceContractEntry.ServiceContractRecords.Current =
                graphServiceContractStatusProcess.graphServiceContractEntry.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsServiceContractRow.ServiceContractID, fsServiceContractRow.CustomerID);

                var adapter = new PXAdapter(graphServiceContractStatusProcess.graphServiceContractEntry.ServiceContractRecords);
                adapter.SortColumns = new string[] { typeof(FSServiceContract.refNbr).Name };
                adapter.Searches = new object[] { graphServiceContractStatusProcess.graphServiceContractEntry.ServiceContractRecords.Current.RefNbr };
                adapter.MaximumRows = 1;

				if (isRenew == false)
				{
					if (fsServiceContractRow.UpcomingStatus == ID.Status_ServiceContract.ACTIVE)
					{
						graphServiceContractStatusProcess.graphServiceContractEntry.ActivateContract(adapter);
					}
					else if (fsServiceContractRow.UpcomingStatus == ID.Status_ServiceContract.CANCELED)
					{
						graphServiceContractStatusProcess.graphServiceContractEntry.CancelContract(adapter);
					}
					else if (fsServiceContractRow.UpcomingStatus == ID.Status_ServiceContract.SUSPENDED)
					{
						graphServiceContractStatusProcess.graphServiceContractEntry.SuspendContract(adapter);
					}
					else if (fsServiceContractRow.UpcomingStatus == ID.Status_ServiceContract.EXPIRED)
					{
						graphServiceContractStatusProcess.graphServiceContractEntry.ExpireContract();
					}
				}
				else
				{
					graphServiceContractStatusProcess.graphServiceContractEntry.RenewContract(adapter);
				}
			}
            else if (fsServiceContractRow.RecordType == ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
            {
                graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords.Current =
                graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsServiceContractRow.ServiceContractID, fsServiceContractRow.CustomerID);

				if (isRenew == false)
				{
					if (fsServiceContractRow.UpcomingStatus == ID.Status_ServiceContract.ACTIVE)
					{
						graphServiceContractStatusProcess.graphRouteServiceContractEntry.ActivateContract(new PXAdapter(graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords));
					}
					else if (fsServiceContractRow.UpcomingStatus == ID.Status_ServiceContract.CANCELED)
					{
						graphServiceContractStatusProcess.graphRouteServiceContractEntry.CancelContract(new PXAdapter(graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords));
					}
					else if (fsServiceContractRow.UpcomingStatus == ID.Status_ServiceContract.SUSPENDED)
					{
						graphServiceContractStatusProcess.graphRouteServiceContractEntry.SuspendContract(new PXAdapter(graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords));
					}
					else if (fsServiceContractRow.UpcomingStatus == ID.Status_ServiceContract.EXPIRED)
					{
						graphServiceContractStatusProcess.graphRouteServiceContractEntry.ExpireContract();
					}
				}
				else
				{
					graphServiceContractStatusProcess.graphServiceContractEntry.RenewContract(new PXAdapter(graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords));
				}
			}
        }

        public virtual void ProcessContractPeriod(ProcessServiceContracts graphServiceContractStatusProcess, FSServiceContract fsServiceContractRow)
        {
            if (fsServiceContractRow.RecordType == ID.RecordType_ServiceContract.SERVICE_CONTRACT)
            {
                graphServiceContractStatusProcess.graphServiceContractEntry.ServiceContractRecords.Current =
                    graphServiceContractStatusProcess.graphServiceContractEntry.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsServiceContractRow.ServiceContractID, fsServiceContractRow.CustomerID);

                graphServiceContractStatusProcess.graphServiceContractEntry.ContractPeriodFilter.SetValueExt<FSContractPeriodFilter.actions>(graphServiceContractStatusProcess.graphServiceContractEntry.ContractPeriodFilter.Current, ID.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD);
                graphServiceContractStatusProcess.graphServiceContractEntry.ActivatePeriod();
            }
            else if (fsServiceContractRow.RecordType == ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
            {
                graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords.Current =
                    graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsServiceContractRow.ServiceContractID, fsServiceContractRow.CustomerID);

                graphServiceContractStatusProcess.graphRouteServiceContractEntry.ContractPeriodFilter.SetValueExt<FSContractPeriodFilter.actions>(graphServiceContractStatusProcess.graphServiceContractEntry.ContractPeriodFilter.Current, ID.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD);
                graphServiceContractStatusProcess.graphRouteServiceContractEntry.ActivatePeriod();
            }
        }

		public virtual void ProcessContractForecast(ProcessServiceContracts graphServiceContractStatusProcess, FSServiceContract fsServiceContractRow)
		{
			if (fsServiceContractRow.RecordType == ID.RecordType_ServiceContract.SERVICE_CONTRACT)
			{
				graphServiceContractStatusProcess.graphServiceContractEntry.ServiceContractRecords.Current =
				graphServiceContractStatusProcess.graphServiceContractEntry.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsServiceContractRow.ServiceContractID, fsServiceContractRow.CustomerID);

				graphServiceContractStatusProcess.graphServiceContractEntry.ContractForecastProc(fsServiceContractRow,
																								 graphServiceContractStatusProcess.graphServiceContractEntry.GetDfltForecastFilterStartDate(),
																								 graphServiceContractStatusProcess.graphServiceContractEntry.GetDfltForecastFilterEndDate());
			}
			else if (fsServiceContractRow.RecordType == ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
			{
				graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords.Current =
				graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsServiceContractRow.ServiceContractID, fsServiceContractRow.CustomerID);

				graphServiceContractStatusProcess.graphRouteServiceContractEntry.ContractForecastProc(fsServiceContractRow,
																				 graphServiceContractStatusProcess.graphRouteServiceContractEntry.GetDfltForecastFilterStartDate(),
																				 graphServiceContractStatusProcess.graphRouteServiceContractEntry.GetDfltForecastFilterEndDate());
			}
		}

		public virtual void ProcessContractEmailQuote(ProcessServiceContracts graphServiceContractStatusProcess, FSServiceContract fsServiceContractRow)
		{
			if (fsServiceContractRow.RecordType == ID.RecordType_ServiceContract.SERVICE_CONTRACT)
			{
				graphServiceContractStatusProcess.graphServiceContractEntry.ServiceContractRecords.Current =
				graphServiceContractStatusProcess.graphServiceContractEntry.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsServiceContractRow.ServiceContractID, fsServiceContractRow.CustomerID);

				var adapter = new PXAdapter(graphServiceContractStatusProcess.graphServiceContractEntry.ServiceContractRecords);
				adapter.SortColumns = new string[] { typeof(FSServiceContract.refNbr).Name };
				adapter.Searches = new object[] { graphServiceContractStatusProcess.graphServiceContractEntry.ServiceContractRecords.Current.RefNbr };
				adapter.MaximumRows = 1;

				graphServiceContractStatusProcess.graphServiceContractEntry.EmailQuoteContract(adapter);
			}
			else if (fsServiceContractRow.RecordType == ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
			{
				graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords.Current =
				graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsServiceContractRow.ServiceContractID, fsServiceContractRow.CustomerID);

				var adapter = new PXAdapter(graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords);
				adapter.SortColumns = new string[] { typeof(FSServiceContract.refNbr).Name };
				adapter.Searches = new object[] { graphServiceContractStatusProcess.graphRouteServiceContractEntry.ServiceContractRecords.Current.RefNbr };
				adapter.MaximumRows = 1;

				graphServiceContractStatusProcess.graphRouteServiceContractEntry.EmailQuoteContract(adapter);
			}
		}

		#region Select
		public PXFilter<ServiceContractFilter> Filter;
        public PXCancel<ServiceContractFilter> Cancel;

        [PXFilterable]
        public PXFilteredProcessingJoin<FSServiceContract, ServiceContractFilter,
                    InnerJoinSingleTable<Customer,
                        On<Customer.bAccountID, Equal<FSServiceContract.customerID>,
                            And<Match<Customer, Current<AccessInfo.userName>>>>>> ServiceContracts;
        #endregion

        #region Delegates
        public virtual IEnumerable serviceContracts()
        {
            BqlCommand commandFilter = null;


            switch (this.Filter.Current.ActionType)
            {
                case ID.ActionType_ProcessServiceContracts.STATUS:
                    commandFilter =  new Select<FSServiceContract,
                                        Where2<
                                            Where<CurrentValue<ServiceContractFilter.customerID>, IsNull,
                                            Or<FSServiceContract.customerID, Equal<CurrentValue<ServiceContractFilter.customerID>>>>,
                                        And2<
                                            Where<CurrentValue<ServiceContractFilter.refNbr>, IsNull,
                                            Or<FSServiceContract.refNbr, Equal<CurrentValue<ServiceContractFilter.refNbr>>>>,
                                        And2<
                                            Where<CurrentValue<ServiceContractFilter.customerLocationID>, IsNull,
                                            Or<FSServiceContract.customerLocationID, Equal<CurrentValue<ServiceContractFilter.customerLocationID>>>>,
                                        And2<
                                            Where<CurrentValue<ServiceContractFilter.branchID>, IsNull,
                                            Or<FSServiceContract.branchID, Equal<CurrentValue<ServiceContractFilter.branchID>>>>,
                                        And2<
                                            Where<CurrentValue<ServiceContractFilter.branchLocationID>, IsNull,
                                            Or<FSServiceContract.branchLocationID, Equal<CurrentValue<ServiceContractFilter.branchLocationID>>>>,
                                        And<
                                            FSServiceContract.upcomingStatus, IsNotNull,
                                        And<
                                            FSServiceContract.statusEffectiveUntilDate, LessEqual<Current<AccessInfo.businessDate>>>>>>>>>,
                                        OrderBy<
                                            Asc<FSServiceContract.customerID,
                                            Asc<FSServiceContract.refNbr>>>>();
                    break;

                case ID.ActionType_ProcessServiceContracts.PERIOD:
                    commandFilter =  new Select2<FSServiceContract,
                                InnerJoin<FSContractPeriod, On<FSContractPeriod.serviceContractID, Equal<FSServiceContract.serviceContractID>,
                                            And<FSContractPeriod.status, Equal<FSContractPeriod.status.Inactive>>>>,
                                        Where2<
                                            Where<CurrentValue<ServiceContractFilter.customerID>, IsNull,
                                            Or<FSServiceContract.customerID, Equal<CurrentValue<ServiceContractFilter.customerID>>>>,
                                        And2<
                                            Where<CurrentValue<ServiceContractFilter.refNbr>, IsNull,
                                            Or<FSServiceContract.refNbr, Equal<CurrentValue<ServiceContractFilter.refNbr>>>>,
                                        And2<
                                            Where<CurrentValue<ServiceContractFilter.customerLocationID>, IsNull,
                                            Or<FSServiceContract.customerLocationID, Equal<CurrentValue<ServiceContractFilter.customerLocationID>>>>,
                                        And2<
                                            Where<CurrentValue<ServiceContractFilter.branchID>, IsNull,
                                            Or<FSServiceContract.branchID, Equal<CurrentValue<ServiceContractFilter.branchID>>>>,
                                        And2<
                                            Where<CurrentValue<ServiceContractFilter.branchLocationID>, IsNull,
                                            Or<FSServiceContract.branchLocationID, Equal<CurrentValue<ServiceContractFilter.branchLocationID>>>>,
                                        And<
                                    FSServiceContract.activePeriodID, IsNull,
                                        And<
                                            FSServiceContract.billingType, NotEqual<FSServiceContract.billingType.Values.performedBillings>,
                                        And<
                                            FSServiceContract.status, Equal<FSServiceContract.status.Active>>>>>>>>>,
                                        OrderBy<
                                            Asc<FSServiceContract.customerID,
                                            Asc<FSServiceContract.refNbr>>>>();
                    break;
				case ID.ActionType_ProcessServiceContracts.RENEW:
					commandFilter = new Select<FSServiceContract,
										Where2<
											Where<CurrentValue<ServiceContractFilter.customerID>, IsNull,
											Or<FSServiceContract.customerID, Equal<CurrentValue<ServiceContractFilter.customerID>>>>,
										And2<
											Where<CurrentValue<ServiceContractFilter.refNbr>, IsNull,
											Or<FSServiceContract.refNbr, Equal<CurrentValue<ServiceContractFilter.refNbr>>>>,
										And2<
											Where<CurrentValue<ServiceContractFilter.customerLocationID>, IsNull,
											Or<FSServiceContract.customerLocationID, Equal<CurrentValue<ServiceContractFilter.customerLocationID>>>>,
										And2<
											Where<CurrentValue<ServiceContractFilter.branchID>, IsNull,
											Or<FSServiceContract.branchID, Equal<CurrentValue<ServiceContractFilter.branchID>>>>,
										And2<
											Where<CurrentValue<ServiceContractFilter.branchLocationID>, IsNull,
											Or<FSServiceContract.branchLocationID, Equal<CurrentValue<ServiceContractFilter.branchLocationID>>>>,
										And<
											FSServiceContract.expirationType, Equal<FSServiceContract.expirationType.Renewable>,
										And<
											FSServiceContract.status, NotEqual<FSServiceContract.status.Draft>>>>>>>>,
										OrderBy<
											Asc<FSServiceContract.customerID,
											Asc<FSServiceContract.refNbr>>>>();
					break;
				case ID.ActionType_ProcessServiceContracts.FORECAST:
					commandFilter = new Select<FSServiceContract,
										Where2<
											Where<CurrentValue<ServiceContractFilter.customerID>, IsNull,
											Or<FSServiceContract.customerID, Equal<CurrentValue<ServiceContractFilter.customerID>>>>,
										And2<
											Where<CurrentValue<ServiceContractFilter.refNbr>, IsNull,
											Or<FSServiceContract.refNbr, Equal<CurrentValue<ServiceContractFilter.refNbr>>>>,
										And2<
											Where<CurrentValue<ServiceContractFilter.customerLocationID>, IsNull,
											Or<FSServiceContract.customerLocationID, Equal<CurrentValue<ServiceContractFilter.customerLocationID>>>>,
										And2<
											Where<CurrentValue<ServiceContractFilter.branchID>, IsNull,
											Or<FSServiceContract.branchID, Equal<CurrentValue<ServiceContractFilter.branchID>>>>,
										And2<
											Where<CurrentValue<ServiceContractFilter.branchLocationID>, IsNull,
											Or<FSServiceContract.branchLocationID, Equal<CurrentValue<ServiceContractFilter.branchLocationID>>>>,
										And<
											FSServiceContract.status, NotEqual<FSServiceContract.status.Canceled>,
											And<FSServiceContract.status, NotEqual<FSServiceContract.status.Expired>>>>>>>>,
										OrderBy<
											Asc<FSServiceContract.customerID,
											Asc<FSServiceContract.refNbr>>>>();
					break;
				case ID.ActionType_ProcessServiceContracts.EMAILQUOTE:
					commandFilter = new Select2<FSServiceContract,
										InnerJoin<FSContractForecast, On<FSContractForecast.serviceContractID, Equal<FSServiceContract.serviceContractID>,
																	 And<FSContractForecast.active, Equal<True>>>>,
										Where2<
											Where<CurrentValue<ServiceContractFilter.customerID>, IsNull,
											Or<FSServiceContract.customerID, Equal<CurrentValue<ServiceContractFilter.customerID>>>>,
										And2<
											Where<CurrentValue<ServiceContractFilter.refNbr>, IsNull,
											Or<FSServiceContract.refNbr, Equal<CurrentValue<ServiceContractFilter.refNbr>>>>,
										And2<
											Where<CurrentValue<ServiceContractFilter.customerLocationID>, IsNull,
											Or<FSServiceContract.customerLocationID, Equal<CurrentValue<ServiceContractFilter.customerLocationID>>>>,
										And2<
											Where<CurrentValue<ServiceContractFilter.branchID>, IsNull,
											Or<FSServiceContract.branchID, Equal<CurrentValue<ServiceContractFilter.branchID>>>>,
										And<
											Where<CurrentValue<ServiceContractFilter.branchLocationID>, IsNull,
											Or<FSServiceContract.branchLocationID, Equal<CurrentValue<ServiceContractFilter.branchLocationID>>>>>>>>>,
										OrderBy<
											Asc<FSServiceContract.customerID,
											Asc<FSServiceContract.refNbr>>>>();
					break;
				default:
                    break;
            }

            PXView serviceContractView = new PXView(this, true, commandFilter);
            int startRow = PXView.StartRow;
            int totalRows = 0;
            var list = serviceContractView.Select(PXView.Currents,
                                                  PXView.Parameters,
                                                  PXView.Searches,
                                                  PXView.SortColumns,
                                                  PXView.Descendings,
                                                  PXView.Filters,
                                                  ref startRow,
                                                  PXView.MaximumRows,
                                                  ref totalRows);

            PXView.StartRow = 0;

            return list;
        }
        #endregion
    }
}
