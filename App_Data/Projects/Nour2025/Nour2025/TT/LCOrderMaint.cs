using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CA;


namespace Maintenance
{
    public class LCOrderMaint : PXGraph<LCOrderMaint,LCOrder>
    {
        
        public PXSelect<LCOrder> orders;
        public PXSelect<LCDates,
            Where<LCDates.lCNbr, Equal<Current<LCOrder.lCNbr>>>> orderDates;

        public virtual void LCOrder_CashAccountID_FieldUpdated(
PXCache cache, PXFieldUpdatedEventArgs e)
        {
            LCOrder order = e.Row as LCOrder;
            if (order != null)
            {
                CashAccount account = PXSelect<CashAccount, 
                    Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, order.CashAccountID);
                order.Currancy = account.CuryID;
            }
        }

        public virtual void LCOrder_Type_FieldUpdated(
PXCache cache, PXFieldUpdatedEventArgs e)
        {
            LCOrder order = e.Row as LCOrder;
            if (order != null && order.Type == "LG")
            {
                PXUIFieldAttribute.SetEnabled<LCOrder.guranteeType>(cache, order, true);
            }
        }
    }
}