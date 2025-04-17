using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;
using PX.Objects.AR;
namespace MyMaintaince
{
   public  class ItemsEntry:PXGraph<ItemsEntry,Items>
    {
       //public PXSelect<Items> items;
       public PXSelect<Items> ViechleItems;
       public PXSelect<ItemCustomers, Where<ItemCustomers.itemsID,Equal<Current<Items.itemsID>>>> itemCustomers;

       //protected virtual void Items_code_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
       //{
       //    string code = e.NewValue.ToString();
       //    if (code.Length < 17 || code.Length == 17)
       //    {
       //        //correct so do nothing
       //    }
       //    else {
       //        throw new PXSetPropertyException("Code must be less or equal 17 Charcter! ");
       //    }
       //}
       //protected virtual void Items_gurarantYear_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
       //{
       //    DateTime sysDate = DateTime.Now;
       //    DateTime date = new DateTime(sysDate.Year + Convert.ToInt32(e.NewValue), sysDate.Month, sysDate.Day);
       //    this.ViechleItems.Current.ExpiredDate = date;
       //}
       //protected virtual void Items_charactersAndNumbers_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
       //{
       //   if (e.Row != null)
       //    {
       //        Items row = e.Row as Items;
       //        if( row.CharactersAndNumbers != null){
       //            if (row.CharactersAndNumbers == true)
       //            {
                      
       //            }
       //        }
       //    }
       //}
       //public virtual void Items_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
       //{
       //    Items row = this.ViechleItems.Current;
       //    PXSelectBase<JobOrder> job = new PXSelect<JobOrder, Where<JobOrder.itemsID, Equal<Required<JobOrder.itemsID>>>>(this);
       //    PXResultset<JobOrder> result = job.Select(row.ItemsID);
       //    if (result.Count > 0)
       //    {
       //        throw new PXSetPropertyException("can not delete this Item Because it selected in Job Order.");
       //    }
       //}
       //protected virtual void ItemCustomers_customerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
       //{
       //    if(e.Row != null){
       //        ItemCustomers row = e.Row as ItemCustomers;
       //        PXSelectBase<Customer> ItemCustomerss = new PXSelectReadonly<Customer, Where<Customer.bAccountID,Equal<Required<Customer.bAccountID>>>>(this);
       //        ItemCustomerss.Cache.Clear();
       //        Customer c = ItemCustomerss.Select(row.CustomerID);
       //        sender.SetValue<ItemCustomers.customerName>(row, c.AcctName);
       //    }
       //}
       //protected virtual void ItemCustomers_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
       //{
       //    if (e.Row != null)
       //    {
       //        ItemCustomers row = e.Row as ItemCustomers;
       //        PXSelectBase<Customer> ItemCustomerss = new PXSelectReadonly<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(this);
       //        ItemCustomerss.Cache.Clear();
       //        Customer c = ItemCustomerss.Select(row.CustomerID);
       //        sender.SetValue<ItemCustomers.customerName>(row, c.AcctName);
       //    }
       //}


    }
}
