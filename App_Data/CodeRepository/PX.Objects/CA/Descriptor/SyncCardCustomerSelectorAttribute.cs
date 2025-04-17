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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.CA
{
	public class SyncCardCustomerSelectorAttribute : PXCustomSelectorAttribute
	{
		public SyncCardCustomerSelectorAttribute(params Type[] fields) : base(typeof(Customer.bAccountID), fields)
		{
			SubstituteKey = typeof(Customer.acctCD);
		}

		public IEnumerable GetRecords()
		{
			CCSynchronizeCards graph = _Graph as CCSynchronizeCards;

			if (graph == null)
			{ 
				return GetAllCustomers();
			}

			CCSynchronizeCard syncCards = graph.Caches[typeof(CCSynchronizeCard)].Current as CCSynchronizeCard;

			if (syncCards == null || syncCards.CCProcessingCenterID == null || syncCards.CustomerCCPID == null)
			{
				return GetAllCustomers();
			}
			else
			{
				string customerId = syncCards.CustomerCCPID;
				string processingCenterID = syncCards.CCProcessingCenterID;
				IEnumerable<Customer> customers = GetCustomersWithSameCCPID(processingCenterID,customerId);

				if (!customers.Any())
				{
					return GetAllCustomers();
				}
				return customers;
			}		
		}

		private IEnumerable<Customer> GetCustomersWithSameCCPID(string processingCenterID,string customerCCPID)
		{
			PXSelectBase<Customer> query = new PXSelectReadonly2<Customer,
				InnerJoin<CustomerProcessingCenterID,On<Customer.bAccountID,Equal<CustomerProcessingCenterID.bAccountID>>>,
				Where<CustomerProcessingCenterID.cCProcessingCenterID,Equal<Required<CustomerProcessingCenterID.cCProcessingCenterID>>,
					And<CustomerProcessingCenterID.customerCCPID,Equal<Required<CustomerProcessingCenterID.customerCCPID>>>>>(_Graph);

			var attributes = _Graph.Caches[typeof(CCSynchronizeCard)].GetAttributesReadonly(_FieldName);
			foreach (PXRestrictorAttribute restrictorAttr in attributes.OfType<PXRestrictorAttribute>())
			{
				query.WhereAnd(restrictorAttr.RestrictingCondition);
			}

			PXResultset<Customer> customers = query.Select(processingCenterID,customerCCPID);
			return customers.RowCast<Customer>();
		}

		private IEnumerable GetAllCustomers()
		{
			PXSelectBase<Customer> query = new PXSelectReadonly<Customer>(_Graph);

			var attributes = _Graph.Caches[typeof(CCSynchronizeCard)].GetAttributesReadonly(_FieldName);

			foreach (PXRestrictorAttribute restrictorAttr in attributes.OfType<PXRestrictorAttribute>())
			{
				query.WhereAnd(restrictorAttr.RestrictingCondition);
			}
			return query.Select();
		}
	}
}
