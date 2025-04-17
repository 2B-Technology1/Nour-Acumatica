using System;
//using Avalara.AvaTax.Adapter.AvaCert2Service;
using PX.Common;
using PX.Data;
using System.Collections;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.SO;
using PX.SM;
using System.Collections.Generic;
using PX.Objects;
using PX.Objects.AR;
using PX.Objects.CR;
using MyMaintaince;
using NourSc202007071;

namespace PX.Objects.CR
{
   
  public class BusinessAccountMaint_Extension:PXGraphExtension<BusinessAccountMaint>
  {

    #region Custom Views
     
       //public PXSelect<MyMaintaince.Items, Where<MyMaintaince.Items.customer, Equal<Current<BAccount.acctCD>>>> Vehicles;
  public PXSelectJoin<MyMaintaince.Items,InnerJoin<ItemCustomers,On<Items.itemsID,Equal<ItemCustomers.itemsID>>,
            InnerJoin<Customer,On<Customer.bAccountID,Equal<ItemCustomers.customerID>>>>, Where<Customer.acctCD, Equal<Current<BAccount.acctCD>>>> Vehicles;
       
  public PXSelect<MyMaintaince.JobOrder, Where<MyMaintaince.JobOrder.customer, Equal<Current<BAccount.acctCD>>>> JobOrderss; 
//request form
public PXSelect<RequestForm, Where<RequestForm.customer, Equal<Current<BAccount.acctCD>>>> RequestF; 


    #endregion

    #region Event Handlers



    #endregion

  }


}