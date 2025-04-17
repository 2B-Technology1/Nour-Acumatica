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

using PX.Commerce.Shopify.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Api.ContractBased.Models;
using Serilog.Context;
using System.Threading.Tasks;
using System.Threading;

namespace PX.Commerce.Shopify
{
	public class SPLocationEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Address;
		public IMappedEntity[] Entities => new IMappedEntity[] { Address };

		public MappedLocation Address;
		public MappedCustomer Customer;
	}

	public class SPLocationRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			return base.Restrict<MappedLocation>(mapped, delegate (MappedLocation obj)
			{
				var result = PXSelectJoin<BCSyncStatus,
					InnerJoin<PX.Objects.AR.Customer, On<BCSyncStatus.localID, Equal<PX.Objects.AR.Customer.noteID>>>,
					Where<PX.Objects.AR.Customer.acctCD, Equal<Required<PX.Objects.AR.Customer.acctCD>>,
						And<BCSyncStatus.connectorType, Equal<Required<BCSyncStatus.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Required<BCSyncStatus.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCSyncStatus.entityType>>>>>>>
					.Select((PXGraph)processor, obj.Local?.Customer?.Value?.Trim(), processor.Operation.ConnectorType, processor.Operation.Binding, BCEntitiesAttribute.Customer)
					.FirstOrDefault();
				var customerStatus = result.GetItem<BCSyncStatus>();
				var customer = result.GetItem<PX.Objects.AR.Customer>();

				if (customer?.GetExtension<CustomerExt>().IsGuestCustomer == true)
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogLocationSkippedGuest, obj.Local.LocationID?.Value ?? obj.Local.SyncID.ToString(), obj.Local.Customer.Value));
				}
				if (customerStatus?.ExternID == null)
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogLocationSkippedCustomerNotSynced, obj.Local.LocationID?.Value ?? obj.Local.SyncID.ToString(), obj.Local.Customer.Value));
				}

				return null;
			});
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			#region Locations
			return base.Restrict<MappedLocation>(mapped, delegate (MappedLocation obj)
			{
				#region Locations
				return base.Restrict<MappedLocation>(mapped, delegate (MappedLocation obj)
				{
					if (processor.SelectStatus(BCEntitiesAttribute.Customer, obj.Extern?.CustomerId?.ToString()) == null)
					{
						//Skip if customer not synced
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogLocationSkippedCustomerNotSynced, obj.Extern?.Id?.ToString(), obj.Extern?.CustomerId?.ToString()));
					}

					return null;
				});
				#endregion
			});
			#endregion
		}
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.Address, BCCaptions.Address, 20,
		ParentEntity = BCEntitiesAttribute.Customer,
		IsInternal = false,
		Direction = SyncDirection.Bidirect,
		PrimaryDirection = SyncDirection.Bidirect,
		PrimarySystem = PrimarySystem.Extern,
		PrimaryGraph = typeof(PX.Objects.AR.CustomerLocationMaint),
		ExternTypes = new Type[] { typeof(CustomerAddressData) },
		LocalTypes = new Type[] { typeof(CustomerLocation) },
		AcumaticaPrimaryType = typeof(PX.Objects.CR.Location),
		URL = "customers/{0}",
		Requires = new string[] { BCEntitiesAttribute.Customer }
	)]
	public class SPLocationProcessor : SPLocationBaseProcessor<SPLocationProcessor, SPLocationEntityBucket, MappedLocation>
	{

		#region Constructor
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
		}
		#endregion
		public override async Task<MappedLocation> PullEntity(string externID, string externalInfo, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public override async Task<MappedLocation> PullEntity(Guid? localID, Dictionary<string, object> externalInfo, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		#region Import
		public override async Task FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
		}
		public override async Task<EntityStatus> GetBucketForImport(SPLocationEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			CustomerData customerData = customerDataProvider.GetByID(syncstatus.ExternID.KeySplit(0));
			if (customerData == null) return EntityStatus.None;
			CustomerAddressData customerAddressData = null;
			customerAddressData = customerData.Addresses.FirstOrDefault(x => x.Id == syncstatus.ExternID.KeySplit(1).ToLong());
			if (customerAddressData == null)
				customerAddressData = customerAddressDataProvider.GetByID(syncstatus.ExternID.KeySplit(0), syncstatus.ExternID.KeySplit(1));
			if (customerAddressData == null) return EntityStatus.None;

			MappedLocation addressObj = bucket.Address = bucket.Address.Set(customerAddressData, new object[] { customerData.Id, customerAddressData.Id }.KeyCombine(), customerData.Email, customerAddressData.CalculateHash());
			EntityStatus status = EnsureStatus(addressObj, SyncDirection.Import);

			MappedCustomer customerObj = bucket.Customer = bucket.Customer.Set(customerData, customerData.Id?.ToString(), customerData.Email, customerData.DateModifiedAt.ToDate(false));
			EntityStatus customerStatus = EnsureStatus(customerObj, SyncDirection.Import);

			return status;

		}

		public override async Task MapBucketImport(SPLocationEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedLocation addressObj = bucket.Address;
			MappedCustomer customerObj = bucket.Customer;

			//Existing
			PX.Objects.AR.Customer customer = PXSelect<PX.Objects.AR.Customer,
				Where<PX.Objects.AR.Customer.noteID, Equal<Required<PX.Objects.AR.Customer.noteID>>>>.Select(this, customerObj.LocalID);
			if (customer == null) throw new PXException(BCMessages.NoCustomerForAddress, addressObj.ExternID);

			addressObj.Local = MapLocationImport(addressObj.Extern, customerObj);
			addressObj.Local.Customer = customer.AcctCD?.Trim().ValueField();
			addressObj.Local.Active = true.ValueField();
		}
		public override async Task SaveBucketImport(SPLocationEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
			MappedLocation obj = bucket.Address;

			if (existing?.Local != null) obj.Local.Customer = ((CustomerLocation)existing.Local).Customer.Value.SearchField();
			if (existing?.Local != null) obj.Local.LocationID = ((CustomerLocation)existing.Local).LocationID.Value.SearchField();


			if (obj.LocalID == null)
			{
				PX.Objects.CR.Location location = PXSelectJoin<PX.Objects.CR.Location,
				InnerJoin<PX.Objects.CR.BAccount, On<PX.Objects.CR.Location.locationID, Equal<PX.Objects.CR.BAccount.defLocationID>>>,
				Where<PX.Objects.CR.BAccount.noteID, Equal<Required<PX.Objects.CR.BAccount.noteID>>>>.Select(this, bucket.Customer.LocalID);

				if (location != null && obj.Extern.Default == true)
				{
					obj.LocalID = location.NoteID; //if location already created
				}
				if (obj.LocalID == null)
				{
					obj.Local.ShipVia = location?.CCarrierID.ValueField();
				}
			}
			CustomerLocation addressImpl = cbapi.Put<CustomerLocation>(obj.Local, obj.LocalID);

			obj.AddLocal(addressImpl, addressImpl.SyncID, addressImpl.SyncTime);
			UpdateStatus(obj, operation);
		}
		#endregion

		#region Export
		public override async Task FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
		}
		public override async Task<EntityStatus> GetBucketForExport(SPLocationEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			CustomerLocation addressImpl = cbapi.GetByID<CustomerLocation>(syncstatus.LocalID, GetCustomFieldsForExport());
			if (addressImpl == null) return EntityStatus.None;

			//Address
			MappedLocation addressObj = bucket.Address = bucket.Address.Set(addressImpl, addressImpl.SyncID, addressImpl.SyncTime);
			EntityStatus status = EnsureStatus(bucket.Address, SyncDirection.Export);

			//Customer
			BCSyncStatus customerStatus = PXSelectJoin<BCSyncStatus,
				InnerJoin<PX.Objects.AR.Customer, On<BCSyncStatus.localID, Equal<PX.Objects.AR.Customer.noteID>>>,
				Where<PX.Objects.AR.Customer.acctCD, Equal<Required<PX.Objects.AR.Customer.acctCD>>,
					And<BCSyncStatus.connectorType, Equal<Required<BCSyncStatus.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Required<BCSyncStatus.bindingID>>,
					And<BCSyncStatus.entityType, Equal<Required<BCSyncStatus.entityType>>>>>>>
				.Select(this, addressObj.Local.Customer?.Value?.Trim(), syncstatus.ConnectorType, syncstatus.BindingID, BCEntitiesAttribute.Customer);
			if (customerStatus == null) throw new PXException(BCMessages.CustomerNotSyncronized, addressImpl.Customer?.Value);

			MappedCustomer customerObj = bucket.Customer = bucket.Customer.Set((Customer)null, customerStatus.LocalID, customerStatus.LocalTS);
			customerObj.AddExtern(null, customerStatus.ExternID, null, customerStatus.ExternTS);
			addressObj.ParentID = customerStatus.SyncID;
			return status;
		}

		public override async Task MapBucketExport(SPLocationEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedLocation addressObj = bucket.Address;
			MappedCustomer customerObj = bucket.Customer;
			if (customerObj == null || customerObj.ExternID == null) throw new PXException(BCMessages.CustomerNotSyncronized, addressObj.Local.Customer.Value);

			addressObj.Extern = MapLocationExport(addressObj, customerObj);

		}

		public override async Task SaveBucketExport(SPLocationEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
			MappedLocation obj = bucket.Address;

			CustomerAddressData addressData;
			try
			{
				if (obj.ExternID == null || existing == null)
					addressData = customerAddressDataProvider.Create(obj.Extern, bucket.Customer.ExternID);
				else
					addressData = customerAddressDataProvider.Update(obj.Extern, obj.ExternID.KeySplit(0), obj.ExternID.KeySplit(1));

				if (obj.Local.Active?.Value == false)
				{
					obj.Local.Active = true.ValueField();
					CustomerLocation addressImpl = cbapi.Put<CustomerLocation>(obj.Local, obj.LocalID);
				}
			}
			catch (Exception ex)
			{
				throw new PXException(ex.Message);
			}
			obj.AddExtern(addressData, new object[] { addressData.CustomerId, addressData.Id }.KeyCombine(), bucket.Customer.Extern.Email, addressData.CalculateHash());
			UpdateStatus(obj, operation);
		}
		#endregion
	}
}
