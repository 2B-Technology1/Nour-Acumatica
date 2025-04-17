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

using PX.Commerce.Core;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.SO;

using Customer = PX.Objects.AR.Customer;
using CustomerEntity = PX.Commerce.Core.API.Customer;
using PX.Commerce.Core.API;
using PX.Api.ContractBased.Models;
using PX.Common;
using PX.Commerce.Core.Model;
using Autofac;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// Service which provides the correct guest account for the binding.
	/// </summary>
	public interface IGuestCustomerService
	{
		/// <summary>
		/// Retrieves the guest account for the passed binding.
		/// </summary>
		/// <param name="graph">The graph used for data retrieval</param>
		/// <param name="apiService">The API service used to create a new guest customer account</param>
		/// <param name="binding">The store binding</param>
		/// <returns>The guest customer account</returns>
		Customer GetGuestCustomer(PXGraph graph, ICBAPIService apiService, BCBindingExt binding);
	}

	/// <summary>
	/// Class which provides caching and retrieval of guest customer accounts and thread synchronization.
	/// </summary>
	public class GuestCustomerService : IGuestCustomerService
	{
		/// <summary>
		/// Cache containing the guest customer account information for each binding.
		/// </summary>
		protected Dictionary<int, (int GuestAccountID, int OrderCount)> _guestAccountCache = new Dictionary<int, (int GuestAccountID, int OrderCount)>();

		/// <summary>
		/// locker for thread synchronization
		/// </summary>
		private object _locker = new object();

		/// <summary>
		/// The maximum number of orders allowed per guest account.
		/// </summary>
		private int maxOrderCount = WebConfig.GetInt(BCConstants.MaxOrdersPerGuestAccount, BCConstants.DefaultMaxOrderPerGuestAccount);

		/// <summary>
		/// Retrieves the guest account for the passed binding.
		/// </summary>
		/// <param name="graph">The graph used for data retrieval</param>
		/// <param name="apiService">The API service used to create a new guest customer account</param>
		/// <param name="binding">The store binding</param>
		/// <returns>The guest customer account</returns>
		public virtual Customer GetGuestCustomer(PXGraph graph, ICBAPIService apiService, BCBindingExt binding)
		{
			lock (_locker)
			{
				if (binding.GuestCustomerID == null) return null;

				int guestCustomerID = binding.GuestCustomerID.Value;

				if (binding.MultipleGuestAccounts == true)
				{
					(int GuestAccountID, int OrderCount) entry;

					//Check cache for guest account
					if (!_guestAccountCache.TryGetValue(binding.BindingID.Value, out entry))
					{
						//If not create the entry
						entry.GuestAccountID = guestCustomerID;
						entry.OrderCount = SelectFrom<SOOrder>
							.Where<SOOrder.customerID.IsEqual<@P.AsInt>>
							.AggregateTo<Count<SOOrder.orderNbr>>
							.View.Select(graph, entry.GuestAccountID).RowCount.Value;
					}

					if (entry.OrderCount >= maxOrderCount)
					{
						//If it isn't, Confirm by retrieving the order count
						var orderCount = SelectFrom<SOOrder>
							.Where<SOOrder.customerID.IsEqual<@P.AsInt>>
							.AggregateTo<Count<SOOrder.orderNbr>>
							.View.Select(graph, entry.GuestAccountID).RowCount.Value;
						//Check if actual order count is less than max
						if (orderCount < maxOrderCount)
						{
							//if it is update order count
							entry.OrderCount = orderCount;
						}
						else
						{
							//if it isn't, create new guest account
							guestCustomerID = CreateGuestCustomer(graph, apiService, binding, guestCustomerID);
							entry.OrderCount = 0;
						}
					}

					entry.GuestAccountID = guestCustomerID;
					entry.OrderCount++;
					_guestAccountCache[binding.BindingID.Value] = entry;
				}

				return Customer.PK.Find(graph, guestCustomerID);
			}
		}

		/// <summary>
		/// Creates a new guest customer account for the given binding.
		/// </summary>
		/// <param name="graph">The graph used for data retrieval</param>
		/// <param name="apiService">The API service used to create a new guest customer account</param>
		/// <param name="binding">The store binding</param>
		/// <param name="guestAccountID">ID of the current guest customer account</param>
		/// <returns>The created guest customer account</returns>
		protected virtual int CreateGuestCustomer(PXGraph graph, ICBAPIService apiService, BCBindingExt binding, int guestAccountID)
		{
			var currentGuest = Customer.PK.Find(graph, guestAccountID);
			var currentGuestEntity = apiService.GetByID(currentGuest.NoteID, new CustomerEntity()
			{
				ReturnBehavior = ReturnBehavior.All
			});

			CustomerEntity newGuestEntity = null;

			using (PXTransactionScope tscope = new PXTransactionScope())
			{
				//When a guest account is set in the binding through the UI, the IsGuestCustomer flag isn't updated. This check updates the IsGuestCustomer flag in those cases.
				if (currentGuestEntity.IsGuestCustomer.Value != true)
				{
					apiService.Put(new CustomerEntity() { IsGuestCustomer = new BooleanValue() { Value = true } }, currentGuestEntity.NoteID.Value);
				}

				newGuestEntity = apiService.Put(MapGuestCustomer(currentGuestEntity));

				//Update guest customer id in store settings.
				bool isAborted = false;
				PXCache cache = graph.Caches[typeof(BCBindingExt)];
				try
				{
					binding.GuestCustomerID = newGuestEntity.BAccountID.Value;
					cache.Update(binding);
					cache.Persist(binding, PXDBOperation.Update);
				}
				catch (Exception)
				{
					isAborted = true;
					throw;
				}
				cache.Persisted(isAborted);

				tscope.Complete();
			}

			return newGuestEntity.BAccountID.Value.Value;
		}

		/// <summary>
		/// Creates a new guest customer from the passed customer object.
		/// </summary>
		/// <param name="currentGuestEntity">The customer to use as a base</param>
		/// <returns>A new guest customer with values copied from currentGuestEntity"</returns>
		protected virtual CustomerEntity MapGuestCustomer(CustomerEntity currentGuestEntity)
		{
			return new CustomerEntity()
			{
				IsGuestCustomer = new BooleanValue() { Value = true },
				CustomerName = currentGuestEntity.CustomerName,
				PriceClassID = currentGuestEntity.PriceClassID,
				CustomerClass = currentGuestEntity.CustomerClass,
				BAccountID = new IntValue(),
				PrimaryContact = new Contact()
				{
					FullName = currentGuestEntity.PrimaryContact?.FullName,
					Attention = currentGuestEntity.PrimaryContact?.Attention,
					Email = currentGuestEntity.PrimaryContact?.Email,
					Phone1 = currentGuestEntity.PrimaryContact?.Phone1,
					Phone2 = currentGuestEntity.PrimaryContact?.Phone2,
					Active = currentGuestEntity.PrimaryContact?.Active,
					Address = new Address()
					{
						AddressLine1 = currentGuestEntity?.PrimaryContact?.Address?.AddressLine1,
						AddressLine2 = currentGuestEntity?.PrimaryContact?.Address?.AddressLine2,
						City = currentGuestEntity?.PrimaryContact?.Address?.City,
						State = currentGuestEntity?.PrimaryContact?.Address?.State,
						Country = currentGuestEntity?.PrimaryContact?.Address?.Country,
						PostalCode = currentGuestEntity?.PrimaryContact?.Address?.PostalCode,
						DefaultId = currentGuestEntity?.PrimaryContact?.Address?.DefaultId,
					}
				},
				MainContact = new Contact()
				{
					FullName = currentGuestEntity.MainContact?.FullName,
					Attention = currentGuestEntity.MainContact?.Attention,
					Email = currentGuestEntity.MainContact?.Email,
					Phone1 = currentGuestEntity.MainContact?.Phone1,
					Phone2 = currentGuestEntity.MainContact?.Phone2,
					Active = currentGuestEntity.MainContact?.Active,
					Address = new Address()
					{
						AddressLine1 = currentGuestEntity?.MainContact?.Address?.AddressLine1,
						AddressLine2 = currentGuestEntity?.MainContact?.Address?.AddressLine2,
						City = currentGuestEntity?.MainContact?.Address?.City,
						State = currentGuestEntity?.MainContact?.Address?.State,
						Country = currentGuestEntity?.MainContact?.Address?.Country,
						PostalCode = currentGuestEntity?.MainContact?.Address?.PostalCode,
						DefaultId = currentGuestEntity?.MainContact?.Address?.DefaultId,
					}
				},
				BillingContactOverride = currentGuestEntity.BillingContactOverride,
				BillingAddressOverride = currentGuestEntity.BillingAddressOverride,
				BillingContact = new Contact()
				{
					FullName = currentGuestEntity.BillingContact?.FullName,
					Attention = currentGuestEntity.BillingContact?.Attention,
					Email = currentGuestEntity.BillingContact?.Email,
					Phone1 = currentGuestEntity.BillingContact?.Phone1,
					Phone2 = currentGuestEntity.BillingContact?.Phone2,
					Active = currentGuestEntity.BillingContact?.Active,
					Address = new Address()
					{
						AddressLine1 = currentGuestEntity?.BillingContact?.Address?.AddressLine1,
						AddressLine2 = currentGuestEntity?.BillingContact?.Address?.AddressLine2,
						City = currentGuestEntity?.BillingContact?.Address?.City,
						State = currentGuestEntity?.BillingContact?.Address?.State,
						Country = currentGuestEntity?.BillingContact?.Address?.Country,
						PostalCode = currentGuestEntity?.BillingContact?.Address?.PostalCode,
						DefaultId = currentGuestEntity?.BillingContact?.Address?.DefaultId,
					}
				},
				ShippingContactOverride = currentGuestEntity.ShippingContactOverride,
				ShippingAddressOverride = currentGuestEntity.ShippingAddressOverride,
				ShippingContact = new Contact()
				{
					FullName = currentGuestEntity.ShippingContact?.FullName,
					Attention = currentGuestEntity.ShippingContact?.Attention,
					Email = currentGuestEntity.ShippingContact?.Email,
					Phone1 = currentGuestEntity.ShippingContact?.Phone1,
					Phone2 = currentGuestEntity.ShippingContact?.Phone2,
					Active = currentGuestEntity.ShippingContact?.Active,
					Address = new Address()
					{
						AddressLine1 = currentGuestEntity?.ShippingContact?.Address?.AddressLine1,
						AddressLine2 = currentGuestEntity?.ShippingContact?.Address?.AddressLine2,
						City = currentGuestEntity?.ShippingContact?.Address?.City,
						State = currentGuestEntity?.ShippingContact?.Address?.State,
						Country = currentGuestEntity?.ShippingContact?.Address?.Country,
						PostalCode = currentGuestEntity?.ShippingContact?.Address?.PostalCode,
						DefaultId = currentGuestEntity?.ShippingContact?.Address?.DefaultId,
					}
				}
			};
		}
	}

	internal class GuestAccountServiceRegistration : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder
				.RegisterType<GuestCustomerService>()
				.As<IGuestCustomerService>()
				.SingleInstance();
		}
	}
}
