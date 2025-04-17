using System;
using PX.Data;

namespace Nour20241217
{
  public class CustomerLoyaltyhistoryGraph : PXGraph<CustomerLoyaltyhistoryGraph>
  {

    public PXSave<CustomerLoyalty> Save;
    public PXCancel<CustomerLoyalty> Cancel;


    public PXFilter<CustomerLoyalty> CustomerLoyalty;
    ///public PXFilter<LoyaltyActivity> LoyaltyActivity;
        public PXSelect<LoyaltyActivity,
    Where<LoyaltyActivity.customerID, Equal<Current<CustomerLoyalty.customerID>>>> LoyaltyActivity;

        protected virtual void _(Events.RowSelected<CustomerLoyalty> e)
        {
            if (e.Row == null) return;

            LoyaltyActivity.Cache.Clear();
            LoyaltyActivity.Cache.ClearQueryCache();
        }
        protected virtual void _(Events.FieldDefaulting<CustomerLoyalty.customerID> e)
        {
            if (e.Row != null)
            {
                e.NewValue = null;
            }
        }


        //[Serializable]
        //public class MasterTable : IBqlTable
        //{

        //}

        //[Serializable]
        //public class DetailsTable : IBqlTable
        //{

        //}


    }
}