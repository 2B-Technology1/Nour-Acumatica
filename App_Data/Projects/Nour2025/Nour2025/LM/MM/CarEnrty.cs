using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using MyMaintaince;

namespace Maintenance.MM
{
    public class CarEnrty : PXGraph<CarEnrty,Items>
    {
        public PXSelect<Items> CarItems;
        public PXSelect<ItemCustomers, Where<ItemCustomers.itemsID,Equal<Current<Items.itemsID>>>> itemCustomers;
    }
}