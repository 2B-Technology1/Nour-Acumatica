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
using PX.Objects.CS;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class BillingCycleMaint : PXGraph<BillingCycleMaint, FSBillingCycle>
    {
        protected class BillingCycleIDAndDate
        {
            public int? BillingCycleID;
            public DateTime? DocDate;
        }

        public PXSelect<FSBillingCycle> BillingCycleRecords;

        [PXHidden]
        public PXSetup<FSSetup> Setup;

        #region Virtual Methods
        /// <summary>
        /// Show/Hide fields and make them Required/Not Required depending on the Billing Cycle Type selected.
        /// </summary>
        /// <param name="cache">BillingCycleRecords cache.</param>
        /// <param name="fsBillingCycleRow">FSBillingCycle row.</param>
        public virtual void BillingCycleTypeFieldsSetup(PXCache cache, FSBillingCycle fsBillingCycleRow)
        {
            switch (fsBillingCycleRow.BillingCycleType)
            {
                case ID.Billing_Cycle_Type.APPOINTMENT:
                case ID.Billing_Cycle_Type.SERVICE_ORDER:
                    PXUIFieldAttribute.SetVisible<FSBillingCycle.timeCycleType>(cache, fsBillingCycleRow, false);
                    PXUIFieldAttribute.SetVisible<FSBillingCycle.timeCycleWeekDay>(cache, fsBillingCycleRow, false);
                    PXUIFieldAttribute.SetVisible<FSBillingCycle.timeCycleDayOfMonth>(cache, fsBillingCycleRow, false);
                    PXUIFieldAttribute.SetEnabled<FSBillingCycle.groupBillByLocations>(cache, fsBillingCycleRow, false);
                    PXDefaultAttribute.SetPersistingCheck<FSBillingCycle.timeCycleDayOfMonth>(cache, fsBillingCycleRow, PXPersistingCheck.Nothing);
                    break;
                case ID.Billing_Cycle_Type.PURCHASE_ORDER:
                case ID.Billing_Cycle_Type.WORK_ORDER:
                    PXUIFieldAttribute.SetVisible<FSBillingCycle.timeCycleType>(cache, fsBillingCycleRow, false);
                    PXUIFieldAttribute.SetVisible<FSBillingCycle.timeCycleWeekDay>(cache, fsBillingCycleRow, false);
                    PXUIFieldAttribute.SetVisible<FSBillingCycle.timeCycleDayOfMonth>(cache, fsBillingCycleRow, false);
                    PXUIFieldAttribute.SetEnabled<FSBillingCycle.groupBillByLocations>(cache, fsBillingCycleRow, true);
                    PXDefaultAttribute.SetPersistingCheck<FSBillingCycle.timeCycleDayOfMonth>(cache, fsBillingCycleRow, PXPersistingCheck.Nothing);
                    break;
                case ID.Billing_Cycle_Type.TIME_FRAME:
                    PXUIFieldAttribute.SetVisible<FSBillingCycle.timeCycleType>(cache, fsBillingCycleRow, true);
                    PXUIFieldAttribute.SetEnabled<FSBillingCycle.groupBillByLocations>(cache, fsBillingCycleRow, true);

                    switch (fsBillingCycleRow.TimeCycleType)
                    {
                        case ID.Time_Cycle_Type.WEEKDAY:
                            PXUIFieldAttribute.SetVisible<FSBillingCycle.timeCycleDayOfMonth>(cache, fsBillingCycleRow, false);
                            PXUIFieldAttribute.SetVisible<FSBillingCycle.timeCycleWeekDay>(cache, fsBillingCycleRow, true);
                            PXDefaultAttribute.SetPersistingCheck<FSBillingCycle.timeCycleWeekDay>(cache, fsBillingCycleRow, PXPersistingCheck.NullOrBlank);
                            PXDefaultAttribute.SetPersistingCheck<FSBillingCycle.timeCycleDayOfMonth>(cache, fsBillingCycleRow, PXPersistingCheck.Nothing);
                            break;
                        case ID.Time_Cycle_Type.DAY_OF_MONTH:
                            PXUIFieldAttribute.SetVisible<FSBillingCycle.timeCycleWeekDay>(cache, fsBillingCycleRow, false);
                            PXUIFieldAttribute.SetVisible<FSBillingCycle.timeCycleDayOfMonth>(cache, fsBillingCycleRow, true);
                            PXDefaultAttribute.SetPersistingCheck<FSBillingCycle.timeCycleDayOfMonth>(cache, fsBillingCycleRow, PXPersistingCheck.NullOrBlank);
                            PXDefaultAttribute.SetPersistingCheck<FSBillingCycle.timeCycleWeekDay>(cache, fsBillingCycleRow, PXPersistingCheck.Nothing);
                            break;
                        default:
                            break;
                    }

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Resets the values of the Time Cycle options depending on the Billing and Time Cycle Types.
        /// </summary>
        /// <param name="fsBillingCycleRow">FSBillingCycle row.</param>
        public virtual void ResetTimeCycleOptions(FSBillingCycle fsBillingCycleRow)
        {
            if (fsBillingCycleRow.BillingCycleType != ID.Billing_Cycle_Type.TIME_FRAME)
            {
                fsBillingCycleRow.TimeCycleWeekDay = null;
                fsBillingCycleRow.TimeCycleDayOfMonth = null;
            }
            else
            {
                switch (fsBillingCycleRow.TimeCycleType)
                {
                    case ID.Time_Cycle_Type.DAY_OF_MONTH:
                        fsBillingCycleRow.TimeCycleWeekDay = null;
                        break;
                    case ID.Time_Cycle_Type.WEEKDAY:
                        fsBillingCycleRow.TimeCycleDayOfMonth = null;
                        break;
                    default:
                        break;
                }
            }
        }

        public virtual void VerifyPrepaidContractRelated(PXCache cache, FSBillingCycle fsBillingCycleRow)
        {
            if (fsBillingCycleRow.BillingBy == (string)cache.GetValueOriginal<FSBillingCycle.billingBy>(fsBillingCycleRow))
            {
                return;
            }

            if (Setup.Current != null)
            {
                string billingByOldValue = (string)cache.GetValueOriginal<FSBillingCycle.billingBy>(fsBillingCycleRow);

                List<object> args = new List<object>();
                BqlCommand bqlCommand = null;
                string entityDocument = TX.Billing_By.SERVICE_ORDER;

                if (billingByOldValue == ID.Billing_By.SERVICE_ORDER)
                {
					bqlCommand = new Select2<FSCustomerBillingSetup,
						CrossJoin<FSSetup,
						InnerJoin<FSServiceOrder, On<FSCustomerBillingSetup.customerID, Equal<FSServiceOrder.billCustomerID>,
							And<Where2<
								Where<FSSetup.customerMultipleBillingOptions, Equal<True>,
									And<FSCustomerBillingSetup.srvOrdType, Equal<FSServiceOrder.srvOrdType>>>,
								Or<Where<FSSetup.customerMultipleBillingOptions, Equal<False>,
									And<FSCustomerBillingSetup.srvOrdType, IsNull>>>>>>,
						InnerJoin<FSServiceContract, On<FSServiceContract.serviceContractID, Equal<FSServiceOrder.billServiceContractID>>,
						InnerJoin<FSContractPeriod, On<FSContractPeriod.serviceContractID, Equal<FSServiceContract.serviceContractID>,
							And<FSContractPeriod.contractPeriodID, Equal<FSServiceOrder.billContractPeriodID>>>>>>>,
						Where<FSCustomerBillingSetup.billingCycleID, Equal<Required<FSCustomerBillingSetup.billingCycleID>>,
							And<FSServiceOrder.canceled, Equal<False>,
							And<FSServiceContract.status, NotEqual<FSServiceContract.status.Canceled>,
							And<FSContractPeriod.status, Equal<FSContractPeriod.status.Active>>>>>>();
                }
                else if (billingByOldValue == ID.Billing_By.APPOINTMENT)
                {
					bqlCommand = new Select2<FSCustomerBillingSetup,
						CrossJoin<FSSetup,
						InnerJoin<FSServiceOrder, On<FSCustomerBillingSetup.customerID, Equal<FSServiceOrder.billCustomerID>,
							And<Where2<
								Where<FSSetup.customerMultipleBillingOptions, Equal<True>,
									And<FSCustomerBillingSetup.srvOrdType, Equal<FSServiceOrder.srvOrdType>>>,
								Or<Where<FSSetup.customerMultipleBillingOptions, Equal<False>,
									And<FSCustomerBillingSetup.srvOrdType, IsNull>>>>>>,
						InnerJoin<FSAppointment, On<FSAppointment.sOID, Equal<FSServiceOrder.sOID>>,
						InnerJoin<FSServiceContract, On<FSServiceContract.serviceContractID, Equal<FSAppointment.billServiceContractID>>,
						InnerJoin<FSContractPeriod, On<FSContractPeriod.serviceContractID, Equal<FSServiceContract.serviceContractID>,
							And<FSContractPeriod.contractPeriodID, Equal<FSAppointment.billContractPeriodID>>>>>>>>,
						Where<FSCustomerBillingSetup.billingCycleID, Equal<Required<FSCustomerBillingSetup.billingCycleID>>,
							And<FSAppointment.canceled, Equal<False>,
							And<FSServiceContract.status, NotEqual<FSServiceContract.status.Canceled>,
							And<FSContractPeriod.status, Equal<FSContractPeriod.status.Active>>>>>>();

                    entityDocument = TX.Billing_By.APPOINTMENT;
                }

                args.Add(fsBillingCycleRow.BillingCycleID);

                PXView documentsView = new PXView(this, true, bqlCommand);
                var document = documentsView.SelectSingle(args.ToArray());

                if (document != null)
                {
                    PXException exception = new PXSetPropertyException(TX.Error.NO_UPDATE_BILLING_CYCLE_SERVICE_CONTRACT_RELATED, PXErrorLevel.Error, entityDocument);
					cache.RaiseExceptionHandling<FSBillingCycle.billingCycleCD>(fsBillingCycleRow, fsBillingCycleRow.BillingCycleCD, exception);
                    throw exception;
                }
            }
        }
        #endregion

        #region Event Handlers

        #region FSBillingCycle

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSBillingCycle, FSBillingCycle.billingCycleType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSBillingCycle fsBillingCycleRow = (FSBillingCycle)e.Row;

            if (fsBillingCycleRow.BillingCycleType == ID.Billing_Cycle_Type.APPOINTMENT
                    || fsBillingCycleRow.BillingCycleType == ID.Billing_Cycle_Type.SERVICE_ORDER)
            {
                fsBillingCycleRow.GroupBillByLocations = false;
            }

            ResetTimeCycleOptions(fsBillingCycleRow);
        }

        protected virtual void _(Events.FieldUpdated<FSBillingCycle, FSBillingCycle.timeCycleType> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSBillingCycle fsBillingCycleRow = (FSBillingCycle)e.Row;
            ResetTimeCycleOptions(fsBillingCycleRow);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSBillingCycle> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSBillingCycle> e)
        {
            if (e.Row == null)
            {
                return;
            }

            BillingCycleTypeFieldsSetup(e.Cache, e.Row);

            PXUIFieldAttribute.SetVisible<FSBillingCycle.groupBillByLocations>(e.Cache,
                                                                                e.Row,
                                                                                PXAccess.FeatureInstalled<FeaturesSet.accountLocations>());
        }

        protected virtual void _(Events.RowInserting<FSBillingCycle> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSBillingCycle> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSBillingCycle> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSBillingCycle> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSBillingCycle> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSBillingCycle> e)
        {
        }

		protected virtual void _(Events.RowPersisting<FSBillingCycle> e)
		{
			if (e.Row == null)
			{
				return;
			}

			FSBillingCycle fsBillingCycleRow = (FSBillingCycle)e.Row;

			if (fsBillingCycleRow.BillingBy == ID.Billing_By.SERVICE_ORDER && fsBillingCycleRow.BillingCycleType == ID.Billing_Cycle_Type.APPOINTMENT)
			{
				throw new PXException(TX.Error.CANT_DEFINE_BILLING_CYCLE_BILLED_BY_SERVICE_ORDER_AND_GROUPED_BY_APPOINTMENT);
			}

			if (e.Operation == PXDBOperation.Delete)
			{
				int? billingCycleCount = PXSelectJoinGroupBy<FSCustomerBillingSetup,
					CrossJoin<FSSetup>,
                    Where<FSCustomerBillingSetup.billingCycleID, Equal<Required<FSCustomerBillingSetup.billingCycleID>>,
						And<Where2<
							Where<FSSetup.customerMultipleBillingOptions, Equal<True>,
								And<FSCustomerBillingSetup.srvOrdType, IsNotNull>>,
                            Or<Where<FSSetup.customerMultipleBillingOptions, Equal<False>,
								And<FSCustomerBillingSetup.srvOrdType, IsNull>>>>>>,
					Aggregate<Count>>.Select(this, fsBillingCycleRow.BillingCycleID).RowCount;

				if (billingCycleCount > 0)
				{
					throw new PXException(TX.Error.BILLING_CYCLE_ERROR_DELETING_CUSTOMER_USING_IT, fsBillingCycleRow);
				}
			}

			if (e.Operation == PXDBOperation.Update)
			{
				VerifyPrepaidContractRelated(e.Cache, fsBillingCycleRow);
			}
		}

        #endregion

        #endregion
    }
}
