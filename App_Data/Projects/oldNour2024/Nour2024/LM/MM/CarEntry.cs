using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using MyMaintaince;
using PX.Objects.AR;
using Nour2024.Helpers;
using Nour2024.Models;
using Newtonsoft.Json;
using PX.Objects.SO;
using PX.Objects.CR;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using System.Linq;

namespace Maintenance.MM
{
    public class CarEntry : PXGraph<CarEntry,Items2>
    {

        protected bool IsSmsSent = false;
        public PXSelect<Items2> CarItems;
        public PXSelect<ItemCustomers2, Where<ItemCustomers2.itemsID, Equal<Current<Items2.itemsID>>>> CarCustomers;
        protected virtual void Items2_code_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            

                    string code = e.NewValue.ToString();
                    if (code.Length == 17)
                    {
                        //correct so do nothing
                    }

                    else
                    {
                        throw new PXSetPropertyException("Vin Number Must be equal 17 Charcter! ");
                    }

           
        }
       
        public virtual void Items2_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            Items2 row = this.CarItems.Current;
            PXSelectBase<JobOrder> job = new PXSelect<JobOrder, Where<JobOrder.itemsID, Equal<Required<JobOrder.itemsID>>>>(this);
            PXResultset<JobOrder> result = job.Select(row.ItemsID);
            if (result.Count > 0)
            {
                throw new PXSetPropertyException("can not delete this Item Because it selected in Job Order.");
            }
        }
        protected virtual void ItemCustomers2_customerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row != null)
            {
                ItemCustomers2 row = e.Row as ItemCustomers2;
                PXSelectBase<Customer> ItemCustomerss = new PXSelectReadonly<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(this);
                ItemCustomerss.Cache.Clear();
                Customer c = ItemCustomerss.Select(row.CustomerID);
                sender.SetValue<ItemCustomers2.customerName>(row, c.AcctName);
            }
        }
        protected virtual void ItemCustomers2_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row != null)
            {
                ItemCustomers2 row = e.Row as ItemCustomers2;
                PXSelectBase<Customer> ItemCustomerss = new PXSelectReadonly<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(this);
                ItemCustomerss.Cache.Clear();
                Customer c = ItemCustomerss.Select(row.CustomerID);
                sender.SetValue<ItemCustomers2.customerName>(row, c.AcctName);
            }
        }






        protected virtual void _(Events.RowPersisted<ItemCustomers2> e)
        {
            var row = e.Row;
            if (row == null)
                return;
            if(PX.Data.Update.PXInstanceHelper.CurrentCompany == 5)
            {
                bool IsSavingForFirtTime = (/*e.Operation == PXDBOperation.Insert &&*/ !IsSmsSent);
                if (IsSavingForFirtTime == true)
                {
                    string RefNbr = CarItems.Current.Code;
                    DateTime? warrantyStartDate = CarItems.Current.WarrantySDate;
                    string dateString = String.Format("{0:yyyy/MM/dd}", warrantyStartDate);
                    Customer customer = SelectFrom<Customer>
                        .Where<Customer.bAccountID.IsEqual<@P.AsInt>>.View.Select(this, row.CustomerID);
                    string message = $"⁄„Ì·‰« «·⁄“Ì“  „  ›⁄Ì· «·÷„«‰  Õ  —ﬁ„ {customer.AcctCD} Ê Ì„ﬂ‰ﬂ„ «·„ «»⁄… „‰ Œ·«· «·≈ ’«· ⁄·Ì 19943";

                    Contact contact = SelectFrom<Contact>.Where<Contact.bAccountID.IsEqual<@P.AsInt>>.View.Select(this, row.CustomerID);
                    string phone = contact?.Phone1;
                    if (!String.IsNullOrEmpty(phone) && RefNbr != "<NEW>")
                    {
                        string response = SmsSender.SendMessage(message, phone);
                        IsSmsSent = true;
                        salssmsrespons responseRoot = JsonConvert.DeserializeObject<salssmsrespons>(response);

                        if (responseRoot != null)
                        {
                            if (responseRoot.code == 0)
                            {
                                string responseMessage = responseRoot.message;
                                string smsid = responseRoot.smsid;
                            }
                        }
                    }
                }
            }
        }



    }
}