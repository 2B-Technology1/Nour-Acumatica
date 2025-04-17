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
﻿using PX.Data;

using PX.Objects.Common;
using PX.Objects.CR;

namespace PX.Objects.AR.Repositories
{
	public class CustomerRepository : RepositoryBase<Customer>
	{
		public CustomerRepository(PXGraph graph)
			: base(graph)
		{ }

		public Customer FindByID(int? accountID)
		{
			foreach (PXResult<BAccount, Customer> result in PXSelectJoin<
				BAccount,
					InnerJoinSingleTable<Customer,
						On<BAccount.bAccountID, Equal<Customer.bAccountID>>>,
				Where2<
					Where<
						BAccount.type, Equal<BAccountType.customerType>,
						Or<BAccount.type, Equal<BAccountType.combinedType>>>,
					And<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>>
				.Select(_graph, accountID))
			{
				BAccount businessAccount = result;
				Customer customer = result;

				PXCache<BAccount>.RestoreCopy(customer, businessAccount);

				return customer;
			}

			return null;
		}

		public Customer FindByCD(string accountCD)
		{
			foreach (PXResult<BAccount, Customer> result in PXSelectJoin<
				BAccount,
					InnerJoinSingleTable<Customer,
						On<BAccount.bAccountID, Equal<Customer.bAccountID>>>,
				Where2<
					Where<
						BAccount.type, Equal<BAccountType.customerType>,
						Or<BAccount.type, Equal<BAccountType.combinedType>>>,
					And<BAccount.acctCD, Equal<Required<Customer.acctCD>>>>>
				.Select(_graph, accountCD))
			{
				BAccount businessAccount = result;
				Customer customer = result;

				PXCache<BAccount>.RestoreCopy(customer, businessAccount);

				return customer;
			}

			return null;
		}

		public Customer GetByCD(string accountCD) => ForceNotNull(FindByCD(accountCD), accountCD);

		public Tuple<CustomerClass, Customer> GetCustomerAndClassById(int? customerId)
		{
			var res = (PXResult<CustomerClass, Customer>)PXSelectJoin<CustomerClass,
				InnerJoin<Customer, On<CustomerClass.customerClassID, Equal<Customer.customerClassID>>>,
				Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(_graph, customerId);

			if (res != null)
			{
				CustomerClass customerClass = res;
				Customer customer = (Customer)res;
				return new Tuple<CustomerClass, Customer>(customerClass, customer);
			}
			return null;
		}
	}
}
