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


using PX.Api;
using PX.Api.ContractBased.Common.Model;
using PX.Api.ContractBased.Models;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Shopify.API.GraphQL;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Address = PX.Commerce.Core.API.Address;
using Contact = PX.Commerce.Core.API.Contact;
using Customer = PX.Commerce.Core.API.Customer;
using CustomerLocation = PX.Commerce.Core.API.CustomerLocation;

namespace PX.Commerce.Shopify
{
	/// <summary>
	/// A container for objects necessary to sync Company Entities between internal and external systems.
	/// </summary>
	public class SPCompanyEntityBucket : EntityBucketBase, IEntityBucket
	{
		///<inheritdoc/>
		public IMappedEntity Primary => Company;

		///<inheritdoc/>
		public IMappedEntity[] Entities => new IMappedEntity[] { Company };

		/// <summary>
		/// The <see cref="MappedCompany"/> containing the local <see cref="Core.API.Customer"/> and the external <see cref="CompanyDataGQL"/>
		/// </summary>
		public MappedCompany Company { get; set; }

		/// <summary>
		/// The list of <see cref="CustomerLocation"/>s belonging to the customer.
		/// </summary>
		public IEnumerable<CustomerLocation> CustomerLocations { get; set; }

		/// <summary>
		/// The list of <see cref="Contact"/>s belonging to the customer
		/// </summary>
		public IEnumerable<Contact> CustomerContacts { get; set; }

		/// <summary>
		/// List of roles assignments for the company.
		/// </summary>
		public IEnumerable<RoleAssignment> RoleAssignments { get; set; }
	}

	/// <summary>
	/// Determines whether the company entity is eligible for synchronization and restricts invalid entities.
	/// </summary>
	public class SPCompanyRestrictor : BCBaseRestrictor, IRestrictor
	{
		/// <summary>
		/// Determines whether <paramref name="mapped"/> should be exported.
		/// </summary>
		/// <param name="processor">The processor performing the synchronization.</param>
		/// <param name="mapped">The entity being synchronized.</param>
		/// <param name="mode">Specifies the filtering mode of the operation.</param>
		/// <returns>Returns <see cref="FilterResult"/> of <see cref="FilterStatus.Invalid"/>, <see cref="FilterStatus.Filtered"/>,
		/// or <see cref="FilterStatus.Ignore"/> if the object is restricted. Otherwise null indicating it should not be filtered.</returns>
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			return base.Restrict(mapped, delegate (MappedCompany obj)
			{
				BCBindingExt bindingExt = processor.GetBindingExt<BCBindingExt>();

				var customer = obj.Local;

				//Confirm it is not an individual customer,
				
				if (customer != null && BCCustomerCategoryAttribute.IsIndividual(customer?.CustomerCategory?.Value))
				{
					return new FilterResult(FilterStatus.Ignore);
				}

				//Confirm it is not a guest customer
				if (customer != null && (customer.IsGuestCustomer.Value == true ||
										 bindingExt.GuestCustomerID == customer.BAccountID.Value))
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogCustomerSkippedGuest,
							customer.CustomerID?.Value ?? obj.Local.SyncID.ToString()));
				}

				return null;
			});
		}

		/// <summary>
		/// Determines whether <paramref name="mapped"/> should be imported.
		/// </summary>
		/// <param name="processor">The processor performing the synchronization.</param>
		/// <param name="mapped">The entity being synced.</param>
		/// <param name="mode">Specifies the filtering mode of the operation.</param>
		/// <returns>Returns <see cref="FilterResult"/> of <see cref="FilterStatus.Invalid"/>, <see cref="FilterStatus.Filtered"/>,
		/// or <see cref="FilterStatus.Ignore"/> if the object is restricted. Otherwise null indicating it should not be filtered.</returns>
		public FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			return null;
		}
	}

	/// <summary>
	/// Graph for the synchronization of CompanyData entities between Shopify and Acumatica.
	/// </summary>
	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.Company, BCCaptions.Company, 15,
		IsInternal = false,
		Direction = SyncDirection.Bidirect,
		PrimaryDirection = SyncDirection.Import,
		PrimarySystem = PrimarySystem.Extern,
		PrimaryGraph = typeof(CustomerMaint),
		ExternTypes = new[] { typeof(CompanyDataGQL) },
		LocalTypes = new[] { typeof(Customer) },
		AcumaticaPrimaryType = typeof(PX.Objects.AR.Customer),
		AcumaticaPrimarySelect = typeof(Search<PX.Objects.AR.Customer.acctCD, Where<PX.Objects.AR.Customer.customerCategory, Equal<BCCustomerCategoryAttribute.organizationCategory>>>),
		AcumaticaFeaturesSet = typeof(FeaturesSet.commerceB2B),
		URL = "companies/{0}",
		Requires = new string[] { }
	)]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.CompanyContact, EntityName = BCCaptions.CompanyContact, AcumaticaType = typeof(PX.Objects.CR.Contact))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.CompanyLocation, EntityName = BCCaptions.CompanyLocation, AcumaticaType = typeof(PX.Objects.CR.Location))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.CompanyRoleAssignment, EntityName = BCCaptions.CompanyRoleAssignment, AcumaticaType = typeof(BCRoleAssignment))]
	public class SPCompanyProcessor : BCProcessorSingleBase<SPCompanyProcessor, SPCompanyEntityBucket, MappedCompany>
	{
		#region Common


		/// <summary>
		/// Factory to instantiate the ICompanyDataProvider used for this processor.
		/// </summary>
		[InjectDependency]
		protected ISPGraphQLDataProviderFactory<CompanyGQLDataProvider> GraphQLDataProviderFactory { get; set; }

		/// <summary>
		/// Factory used to create the GraphQLClient used by any DataProviders.
		/// </summary>
		[InjectDependency]
		protected ISPGraphQLAPIClientFactory GraphQLAPIClientFactory { get; set; }

		/// <summary>
		/// Service to send requests to the external GraphQL API.
		/// </summary>
		protected ICompanyGQLDataProvider CompanyDataProvider;

		/// <inheritdoc />
		public override async Task Initialise(IConnector connector, ConnectorOperation operation)
		{
			await base.Initialise(connector, operation);
			var client = GraphQLAPIClientFactory.GetClient(GetBindingExt<BCBindingShopify>());
			CompanyDataProvider = GraphQLDataProviderFactory.GetProvider(client);
		}

		/// <inheritdoc />
		public override async Task<MappedCompany> PullEntity(Guid? localID, Dictionary<string, object> externalInfo,
			CancellationToken cancellationToken = default)
		{
			Customer local = cbapi.GetByID<Customer>(localID);

			if (local == null || !BCCustomerCategoryAttribute.IsOrganization(local.CustomerCategory?.Value))
			{
				return null;
			}

			return new MappedCompany(local, local.Id, local.SyncTime);
		}

		/// <inheritdoc />
		public override async Task<MappedCompany> PullEntity(string externID, string externalInfo,
			CancellationToken cancellationToken = default)
		{
			var external = await CompanyDataProvider.GetCompanyByIDAsync(externID, false, false, cancellationToken);

			if (external == null)
			{
				return null;
			}

			return new MappedCompany(external, external.Id.ConvertGidToId(), external.Name, external.UpdatedAt);
		}
		
		/// <inheritdoc/>
		public override async Task<PullSimilarResult<MappedCompany>> PullSimilar(IExternEntity entity,
			CancellationToken cancellationToken = default)
		{
			string externalId = ((CompanyDataGQL)entity).Id;

			List<MappedCompany> result = new List<MappedCompany>();
			if (externalId != null)
			{
				string externalRef = APIHelper.ReferenceMake(externalId.ConvertGidToId(), GetBinding().BindingName);
				var customers = SelectFrom<PX.Objects.AR.Customer>
					.Where<PX.Objects.AR.Customer.acctReferenceNbr.Contains<P.AsString>>
					.View.Select(this, externalRef).RowCast<PX.Objects.AR.Customer>();
				foreach (PX.Objects.AR.Customer item in customers)
				{
					if (BCCustomerCategoryAttribute.IsOrganization(item?.GetExtension<CustomerExt>()?.CustomerCategory))
					{
						Customer data = new Customer
						{
							SyncID = item.NoteID,
							CustomerName = item.AcctName.ValueField(),
							CustomerClass = item.CustomerClassID.ValueField(),
							SyncTime = item.LastModifiedDateTime
						};
						result.Add(new MappedCompany(data, data.SyncID, data.SyncTime));
					}
				}
			}
			else
				return null;

			if (result.Count == 0) return null;

			return new PullSimilarResult<MappedCompany> { UniqueField = externalId, Entities = result };
		}

		/// <inheritdoc/>
		public override async Task<PullSimilarResult<MappedCompany>> PullSimilar(ILocalEntity entity,
			CancellationToken cancellationToken = default)
		{
			var customerReference = ((Customer)entity)?.AccountRef?.Value;

			var externalId = APIHelper.ReferenceParse(customerReference, GetBinding().BindingName);

			if (!string.IsNullOrEmpty(externalId))
			{
				externalId = externalId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Company);

				//We cannot use the filter with the Id. Filters do not support it.
				//We try first to get the company by id. Otherwise, we switch to the name 
				var sameCompany = await CompanyDataProvider.GetCompanyByIDAsync(externalId, false, false, cancellationToken);
				if (sameCompany != null)
				{
					return new PullSimilarResult<MappedCompany>
					{
						UniqueField = externalId,
						Entities = new List<MappedCompany>() { new MappedCompany(sameCompany, sameCompany.Id.ConvertGidToId(), sameCompany.Name, sameCompany.UpdatedAt.ToDate(true)) }
					};

				}
			}

			//If we cannot find a similar company based on the Id, we search for companies with the same name.
			var companyName = ((Customer)entity)?.CustomerName?.Value;
			if (String.IsNullOrEmpty(companyName))
				return null;

			var similarCompanies = await CompanyDataProvider.GetCompaniesAsync($"name:{companyName}", false, cancellationToken);

			if (similarCompanies?.Any() != true)
			{
				return null;
			}

			return new PullSimilarResult<MappedCompany>
			{
				UniqueField = companyName,
				Entities = similarCompanies.Where(x => string.Equals(x.Name, companyName, StringComparison.OrdinalIgnoreCase)).Select(company =>
					new MappedCompany(company, company.Id.ConvertGidToId(), company.Name, company.UpdatedAt.ToDate(true)))
			};
		}

		#endregion

		#region Import

		/// <inheritdoc cref="BCProcessorSingleBase{TGraph, TEntityBucket, TPrimaryMapped}.FetchBucketsForImport(DateTime?, DateTime?, PXFilterRow[], CancellationToken)"/>
		public override async Task FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime,
			PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			//Query syntax is here: https://shopify.dev/api/usage/search-syntax
			var companies = await CompanyDataProvider.GetCompaniesAsync(GetHelper<SPHelper>().PrepareDateFilter(minDateTime, maxDateTime, GetBindingExt<BCBindingExt>().SyncOrdersFrom), false, cancellationToken);

			var countNum = 0;
			var mappedList = new List<IMappedEntity>();
			try
			{
				foreach (var company in companies)
				{
					IMappedEntity mappedCompany = new MappedCompany(company, company.Id.ConvertGidToId(), company.Name, company.UpdatedAt.ToDate(true));
					mappedList.Add(mappedCompany);
					countNum++;
					if (countNum % BatchFetchCount == 0)
					{
						ProcessMappedListForImport(mappedList, true);
					}
				}
			}
			finally
			{
				if (mappedList.Any())
				{
					ProcessMappedListForImport(mappedList, true);
				}
			}
		}

		/// <inheritdoc/>
		protected override void ProcessMappedListForImport(List<IMappedEntity> mappedList, bool updateFetchIncermentalTime = false)
		{
			var externIDs = mappedList.Select(x => x.ExternID).ToArray();

			List<BCSyncStatus> bcSyncStatusList = GetBCSyncStatusResult(mappedList.FirstOrDefault()?.EntityType, null, null, externIDs)
				.Select(x => x.GetItem<BCSyncStatus>()).ToList() ?? new List<BCSyncStatus>();

			foreach (var oneMapped in mappedList)
			{
				BCSyncStatus existingStatus = bcSyncStatusList.Where(x => (oneMapped.LocalID != null && x.LocalID == oneMapped.LocalID)
					|| (oneMapped.SyncID != null && x.SyncID == oneMapped.SyncID)
					|| (oneMapped.ExternID != null && x.ExternID == oneMapped.ExternID)).FirstOrDefault();
				var syncStatus = EnsureStatus(oneMapped, out BCSyncStatus status, SyncDirection.Import, conditions: Conditions.DoNotFetch, existing: existingStatus);
				//Workaround for checking the Contact/Location changes
				if(existingStatus != null && syncStatus == EntityStatus.Synchronized)
				{
					// Check the Contacts and Locations quantity with existing sync record, if the quantity is less, that means some Contacts/ Locations have been removed in Shopify.
					//And then change the Status to Pending.
					var localContactQty = oneMapped.Details?.Count(x => x.EntityType == BCEntitiesAttribute.CompanyContact) ?? 0;
					var localLocationQty = oneMapped.Details?.Count(x => x.EntityType == BCEntitiesAttribute.CompanyLocation) ?? 0;
					var extCompany = (CompanyDataGQL)oneMapped.Extern;
					if ((extCompany?.ContactCount ?? 0) != localContactQty || (extCompany?.LocationCount ?? 0) != localLocationQty)
					{
						status.PendingSync = true;
						status.Status = BCSyncStatusAttribute.Pending;
						status = (BCSyncStatus)Statuses.Cache.Update(status);
						Statuses.Cache.Persist(PXDBOperation.Update);
						Statuses.Cache.Persisted(false);
					}
				}
				if (existingStatus == null && status != null)
					bcSyncStatusList.Add(status);
			}

			if (updateFetchIncermentalTime)
				IncrFetchLastProcessedTime = mappedList.LastOrDefault()?.ExternTimeStamp;

			mappedList.Clear();
		}

		/// <inheritdoc cref="BCProcessorSingleBase{TGraph, TEntityBucket, TPrimaryMapped}.GetBucketForImport(TEntityBucket, BCSyncStatus, CancellationToken)"/>
		public override async Task<EntityStatus> GetBucketForImport(SPCompanyEntityBucket bucket, BCSyncStatus status,
			CancellationToken cancellationToken = default)
		{
			var company = await CompanyDataProvider.GetCompanyByIDAsync(status.ExternID, true, true, cancellationToken);

			if (company == null)
			{
				return EntityStatus.None;
			}

			//Determine what is the last time the Company has been updated
			var lastModified = company.UpdatedAt;
			lastModified = company.Locations?.Nodes?.Where(l => l.UpdatedAt > lastModified).Select(l => l.UpdatedAt).Max() ?? lastModified;
			lastModified = company.Contacts?.Nodes?.Where(c => c.UpdatedAt > lastModified).Select(c => c.UpdatedAt).Max() ?? lastModified;

			var mappedEntity = bucket.Company = bucket.Company.Set(company, company.Id.ConvertGidToId(), company.Name, lastModified.ToDate(true));

			var updatedStatus = EnsureStatus(mappedEntity, SyncDirection.Import);

			//Because lastModified in Company doesn't change if Company contact or location has been deleted, the following code is a workaround
			//If sync status is Synchronized, we should double check the Contacts, Locations and Roles quantities.			
			updatedStatus = GetValidatedStatus(company, mappedEntity, updatedStatus);

			return updatedStatus;
		}

		/// <summary>
		/// Check the Contacts, Locations and Roles quantities with existing sync record.
		/// If the quantity is less, that means some Contacts/Locations/Roles have been removed in Shopify, then change the Status to Pending.
		/// </summary>
		/// <param name="company"></param>
		/// <param name="mappedEntity"></param>
		/// <param name="updatedStatus"></param>
		/// <returns>The validated SyncStatus</returns>
		public virtual EntityStatus GetValidatedStatus(CompanyDataGQL company, MappedCompany mappedEntity, EntityStatus updatedStatus)
		{
			if (updatedStatus != EntityStatus.Synchronized || mappedEntity.Details == null) return updatedStatus;
			IEnumerable<DetailInfo> entityDetails = mappedEntity.Details;
			var localContactQty = entityDetails.Count(x => x.EntityType == BCEntitiesAttribute.CompanyContact);
			var localLocationQty = entityDetails.Count(x => x.EntityType == BCEntitiesAttribute.CompanyLocation);
			var localRolesDetails = entityDetails.Where(x => x.EntityType == BCEntitiesAttribute.CompanyRoleAssignment);
			var externRolesAssigments = company.ContactsList.SelectMany(comp => comp.RoleAssignmentsList);

			//If a synced Role was changed, let the primary system decide what to do.
			bool hasChangeInRoles = localRolesDetails.Count() != externRolesAssigments.Count() ||
				localRolesDetails.Any(detail => externRolesAssigments.Any(extRole =>extRole.Id.ConvertGidToId() == detail.ExternID) == false);

			if ((company.ContactCount ?? 0) < localContactQty || (company.LocationCount ?? 0) < localLocationQty || hasChangeInRoles)
				updatedStatus = EntityStatus.Pending;
			return updatedStatus;
		}

		public virtual Contact MapMainContact(SPCompanyEntityBucket bucket, CompanyContactDataGQL exteranlContact, string customerClassCountryCode, IMappedEntity existing)
		{
			var mainContact = new Core.API.Contact();

			string firstLastName = (exteranlContact?.Customer?.FirstName?.Equals(exteranlContact?.Customer?.LastName, StringComparison.OrdinalIgnoreCase)) == false ?
				(String.Concat(exteranlContact?.Customer?.FirstName ?? string.Empty, " ", exteranlContact?.Customer?.LastName ?? string.Empty)).Trim() : exteranlContact?.Customer?.FirstName ?? string.Empty;

			mainContact.ExternalID = exteranlContact != null ? (new object[] { exteranlContact?.Customer?.Id.ConvertGidToId(), exteranlContact?.Id.ConvertGidToId() }.KeyCombine()) : null;
			mainContact.FirstName = exteranlContact?.Customer?.FirstName?.ValueField(true);
			mainContact.LastName = GetHelper<SPHelper>().ReplaceStrByOrder(true, exteranlContact?.Customer?.LastName, exteranlContact?.Customer?.FirstName)?.ValueField(true);
			mainContact.Attention = firstLastName.ValueField(true);
			mainContact.Email = exteranlContact?.Customer?.Email?.ValueField(true);
			mainContact.Phone1 = exteranlContact?.Customer?.Phone?.ValueField(true);
			mainContact.Note = exteranlContact?.Customer?.Note;
			mainContact.Active = true.ValueField();
			mainContact.DoNotEmail = (string.IsNullOrEmpty(mainContact.Email?.Value) ? false : true).ValueField();

			return mainContact;
		}

		public virtual Contact MapContact(CompanyDataGQL exteranlCompany, CompanyContactDataGQL exteranlContact, string customerClassCountryCode, IMappedEntity existing)
		{
			var contact = new Contact();
			if (exteranlContact != null)
			{
				contact.ExternalID = new object[] { exteranlContact.Customer?.Id.ConvertGidToId(), exteranlContact.Id.ConvertGidToId() }.KeyCombine(); 
				contact.FullName = GetHelper<SPHelper>().ReplaceStrByOrder(true, exteranlContact?.Customer?.DefaultAddress?.Company, exteranlCompany.Name).ValueField();
				contact.FirstName = exteranlContact?.Customer?.FirstName?.ValueField();
				//LastName is a necessary field in Acumatica in Contact
				contact.LastName = GetHelper<SPHelper>().ReplaceStrByOrder(true, exteranlContact?.Customer?.LastName, exteranlContact?.Customer?.FirstName)?.ValueField();
				contact.JobTitle = exteranlContact?.Title?.ValueField(true);
				contact.Email = exteranlContact?.Customer?.Email?.ValueField(true);
				contact.Phone1 = exteranlContact?.Customer?.Phone?.ValueField(true);
				contact.Active = true.ValueField();
				contact.Address = new Address
				{
					AddressLine1 = exteranlContact?.Customer?.DefaultAddress?.Address1?.ValueField(true),
					AddressLine2 = exteranlContact?.Customer?.DefaultAddress?.Address2?.ValueField(true),
					City = exteranlContact?.Customer?.DefaultAddress?.City?.ValueField(true),
					State = exteranlContact?.Customer?.DefaultAddress?.ProvinceCode?.ValueField(true),
					Country = GetHelper<SPHelper>().ReplaceStrByOrder(true, exteranlContact?.Customer?.DefaultAddress?.CountryCode, customerClassCountryCode).ValueField(),
					PostalCode = exteranlContact?.Customer?.DefaultAddress?.Zip?.ValueField(true),
				};
			}
			return contact;
		}

		/// <inheritdoc />
		public override async Task MapBucketImport(SPCompanyEntityBucket bucket, IMappedEntity existing,
			CancellationToken cancellationToken = default)
		{
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			var external = bucket.Company.Extern;
			var local = bucket.Company.Local = new Customer();
			var existingLocalLocations = bucket.CustomerLocations;
			var existingLocalContacts = bucket.CustomerContacts;
			var existingRoleAssignments = bucket.RoleAssignments?.ToList();

			var localLocations = new List<CustomerLocation>();
			bucket.CustomerLocations = localLocations;

			var localContacts = new List<Contact>();
			bucket.CustomerContacts = localContacts;

			var externRoleAssignments = external.ContactsList?
				.SelectMany(e => e.RoleAssignmentsList)?.ToList();
		
			bucket.RoleAssignments = (externRoleAssignments?.Any() == true)
				? externRoleAssignments.Select(shopifyRole => new RoleAssignment()
					{
						ExternalID = shopifyRole.Id.ConvertGidToId(),
						ExternalCompanyID = external.Id.ConvertGidToId(),
						ExternalContactID = shopifyRole.CompanyContact?.Id.ConvertGidToId(),
						ExternalLocationID = shopifyRole.CompanyLocation?.Id.ConvertGidToId(),
						Role = BCRoleListAttribute.ConvertExternRoleToLocal(shopifyRole.Role?.Name).ValueField()
					}).ToList()
				: new List<RoleAssignment>();

			var existingLocal = existing?.Local as Customer;

			//General Info
			local.CustomerName = external.Name.ValueField();
			local.AccountRef = APIHelper.ReferenceMake(external.Id.ConvertGidToId(), GetBinding().BindingName).ValueField();
			local.EnableCurrencyOverride = true.ValueField();
			local.CustomerClass = existingLocal != null
				? existingLocal.CustomerClass
				: GetBindingExt<BCBindingExt>().CustomerClassID?.ValueField();
			local.CustomerCategory = BCCustomerCategoryAttribute.OrganizationValue.ValueField();

			string countryCodeFromCustomerClass = null;
			StringValue shipVia = null;
			if (bindingExt.CustomerClassID != null)
			{
				PX.Objects.AR.CustomerClass customerClass = PXSelect<PX.Objects.AR.CustomerClass,
					Where<PX.Objects.AR.CustomerClass.customerClassID, Equal<Required<PX.Objects.AR.CustomerClass.customerClassID>>>>.
					Select(this, bindingExt.CustomerClassID);
				if (customerClass != null)
				{
					countryCodeFromCustomerClass = customerClass.CountryID; // no address is present then set country from customer class
					shipVia = customerClass.ShipVia.ValueField();
				}
			}

			#region Customer Contacts

			local.PrimaryContactID = new IntValue() { Value = null };

			foreach (var companyContact in external.ContactsList ?? Enumerable.Empty<CompanyContactDataGQL>())
			{
				var customerContact = MapContact(external, companyContact, countryCodeFromCustomerClass, existing);

				//Get the NoteID from existing contact record
				var existingContact = existingLocalContacts?.FirstOrDefault(x => string.Equals(x.Email?.Value, customerContact.Email?.Value, StringComparison.OrdinalIgnoreCase)) ??
					existingLocalContacts?.FirstOrDefault(x => string.IsNullOrEmpty(x.Email?.Value) && string.Equals(x.FullName?.Value, customerContact.FullName?.Value, StringComparison.OrdinalIgnoreCase));
				if (existingContact != null)
				{
					customerContact.Id = existingContact?.Id;
					customerContact.ContactID = existingContact?.ContactID;
				}

				//if the NoteID from existing contact record is different with the NoteId in detailInfo, and the NoteID in detailInfo exists in the Contacts list
				//should use the NoteID from DetailInfo.
				var existingDetailInfo = bucket.Primary.Details.FirstOrDefault(x => x.EntityType == BCEntitiesAttribute.CompanyContact && x.ExternID == companyContact.Id.ConvertGidToId());
				existingContact = existingDetailInfo?.LocalID != null ? existingLocalContacts?.FirstOrDefault(x => x.Id == existingDetailInfo.LocalID) : null;
				if (existingContact != null)
				{
					customerContact.NoteID = existingDetailInfo.LocalID.ValueField();
					customerContact.Id = existingDetailInfo.LocalID;
					existingRoleAssignments.ForEach(x =>
					{
						if (x.ContactID?.Value == existingContact.ContactID?.Value)
						{
							x.ExternalContactID = companyContact.Id.ConvertGidToId();
							x.LocalContactNoteID = existingDetailInfo.LocalID;
						}
					});

					bucket.RoleAssignments.ForEach(x =>
					{
						if (string.Equals(x.ExternalContactID, companyContact.Id.ConvertGidToId(), StringComparison.OrdinalIgnoreCase))
						{
							x.ContactID = existingContact.ContactID;
							x.LocalContactNoteID = existingDetailInfo.LocalID;
						}
					});
				}

				if (companyContact?.IsMainContact ?? false)
				{
					local.MainContact = MapMainContact(bucket, companyContact, countryCodeFromCustomerClass, existing);
					if (customerContact.ContactID == null && existingDetailInfo == null)
					{
						local.PrimaryContact = customerContact;
					}
					else
					{
						local.PrimaryContactID = customerContact.ContactID;
						local.PrimaryContact = null;
					}

				}

				localContacts.Add(customerContact);
			}

			//If there is no Primary Contact, we must remove the information in the main contact.
			if (local.PrimaryContact == null && local.PrimaryContactID?.Value == null)
			{
				local.MainContact = MapMainContact(bucket, null, countryCodeFromCustomerClass, existing);
			}

			//Handle existing contact with sync record but doesn't exist in the list, that means the contact has been removed from Shopify, we should change its status to inactive.
			foreach (var existingItem in existingLocalContacts ?? Enumerable.Empty<Contact>())
			{
				DetailInfo existingDetailInfo = bucket.Primary.Details.FirstOrDefault(x => x.EntityType == BCEntitiesAttribute.CompanyContact && x.LocalID == existingItem.Id);
				var externalId = existingDetailInfo?.ExternID;
				bool shouldChangeStatus = externalId != null && localContacts?.Any(x => x.ExternalID == externalId) != true;
				if (shouldChangeStatus)
				{
					existingItem.Status = PX.Objects.CR.Messages.Inactive.ValueField();
					existingItem.Impl = null;
					localContacts.Add(existingItem);
				}
			}

			#endregion

			#region Customer Locations
			//Locations
			foreach (var companyLocation in external.LocationsList?.OrderBy(x => x.CreatedAt) ?? Enumerable.Empty<CompanyLocationDataGQL>())
			{
				CustomerLocation customerLocation = new CustomerLocation
				{
					ExternalID = companyLocation.Id.ConvertGidToId(),
					LocationName = companyLocation?.Name?.ValueField(true),
					Active = true.ValueField(),
					Status = PX.Objects.CR.Messages.Active.ValueField(),
					ContactOverride = true.ValueField(),
					AddressOverride = true.ValueField(),
					ShipVia = shipVia,
					Note = companyLocation.Note,
					LocationContact = new Contact
					{
						Address = new Address
						{
							AddressLine1 = companyLocation?.ShippingAddress?.Address1?.ValueField(true),
							AddressLine2 = companyLocation?.ShippingAddress?.Address2?.ValueField(true),
							City = companyLocation?.ShippingAddress?.City?.ValueField(true),
							Country = GetHelper<SPHelper>().ReplaceStrByOrder(true, companyLocation?.ShippingAddress?.CountryCode, countryCodeFromCustomerClass).ValueField(),
							State = companyLocation?.ShippingAddress?.ZoneCode?.ValueField(true),
							PostalCode = companyLocation?.ShippingAddress?.Zip?.ToUpperInvariant()?.ValueField() ?? new StringValue { Value = null }
						}
					},
					RoleAssignments = new List<RoleAssignment>()
				};

				//if the NoteID from existing location record is different with the NoteId in detailInfo, and the NoteID in detailInfo is existing in the locations list, should use the NoteID from DetailInfo
				var existingDetailInfo = bucket.Primary.Details.FirstOrDefault(x => x.EntityType == BCEntitiesAttribute.CompanyLocation && x.ExternID == companyLocation.Id.ConvertGidToId());
				CustomerLocation existingLocation = existingDetailInfo?.LocalID != null ? existingLocalLocations?.FirstOrDefault(x => x.Id == existingDetailInfo.LocalID) : null;
				if (existingLocation != null)
				{
					customerLocation.Id = existingDetailInfo.LocalID;
					customerLocation.NoteID = existingDetailInfo.LocalID.ValueField();

					existingRoleAssignments.ForEach(x =>
					{
						if (string.Equals(x.LocationCD?.Value?.Trim(), existingLocation.LocationID?.Value.Trim(), StringComparison.OrdinalIgnoreCase))
						{
							x.ExternalLocationID = companyLocation.Id.ConvertGidToId();
							x.LocalLocationNoteID = existingDetailInfo.LocalID;
							x.Impl = null;
							//Save existing roleAssignment
							customerLocation.RoleAssignments.Add(x);
						}
					});
				}

				localLocations.Add(customerLocation);
			}

			//Handle existing location with sync record but doesn't exist in the list, that means the location has been removed from Shopify, we should change its status to inactive.
			foreach (var existingItem in existingLocalLocations ?? Enumerable.Empty<CustomerLocation>())
			{
				var existingDetailInfo = bucket.Primary.Details.FirstOrDefault(x => x.EntityType == BCEntitiesAttribute.CompanyLocation && x.LocalID == existingItem.Id);
				bool shouldChangeStatus = existingDetailInfo?.ExternID != null && localLocations?.Any(x => x.ExternalID == existingDetailInfo?.ExternID) != true;
				if (shouldChangeStatus && localLocations.Any() == true)
				{
					existingItem.Active = false.ValueField();
					existingItem.Status = PX.Objects.CR.Messages.Inactive.ValueField();
					existingItem.Default = false.ValueField();
					//Set Impl to null otherwise, it causes an exception when updating the entity
					existingItem.Impl = null;
					existingItem.RoleAssignments?.ForEach(x => {x.Impl = null; x.Delete = true; });
					if (existingItem.LocationContact != null)
					{
						existingItem.LocationContact.Impl = null;
						if (existingItem.LocationContact.Address != null)
							existingItem.LocationContact.Address.Impl = null;
					}

					localLocations.Add(existingItem);
				}
			}

			#endregion

			HandleExistingRolesAssignments(existingRoleAssignments, localContacts, localLocations);

			local.SendDunningLettersbyEmail = local.MainContact?.Email?.Value != null ? existingLocal?.SendDunningLettersbyEmail ?? true.ValueField() : false.ValueField();
			local.SendInvoicesbyEmail = local.MainContact?.Email?.Value != null ? existingLocal?.SendInvoicesbyEmail ?? true.ValueField() : false.ValueField();
			local.SendStatementsbyEmail = local.MainContact?.Email?.Value != null ? existingLocal?.SendStatementsbyEmail ?? true.ValueField() : false.ValueField();
			local.PrintDunningLetters = local.MainContact?.Email?.Value != null ? existingLocal?.PrintDunningLetters ?? true.ValueField() : false.ValueField();
		}

		/// <summary>
		/// Handle existing RolesAssignments that do not assign to any Location
		/// We should delete them first to avoid dulpicated RoleAssignment record
		/// </summary>
		protected virtual void HandleExistingRolesAssignments(List<RoleAssignment> roleAssignments, List<Contact> contacts, List<CustomerLocation> customerLocations)
		{
			//Handle existing RolesAssignments that doesn't assign to any Location
			foreach (var roleAssignment in roleAssignments ?? Enumerable.Empty<RoleAssignment>())
			{
				bool? existingInContact = contacts?.Any(x => x.ContactID?.Value != null && x.ContactID.Value == roleAssignment.ContactID?.Value);
				bool? existingInLocation = customerLocations?.Any(x => x.RoleAssignments?.Any() == true && x.RoleAssignments.Any(rl => rl.LocationCD?.Value == roleAssignment.LocationCD?.Value));
				if (existingInContact == true && existingInLocation != true)
				{
					Contact existingContact = contacts.FirstOrDefault(x => x.ContactID?.Value != null && x.ContactID.Value == roleAssignment.ContactID?.Value);
					if (existingContact.RoleAssignments == null)
						existingContact.RoleAssignments = new List<RoleAssignment>();
					roleAssignment.Impl = null;
					roleAssignment.Delete = true;
					existingContact.RoleAssignments.Add(roleAssignment);
				}
			}
		}


		/// <inheritdoc cref="BCProcessorSingleBase{TGraph, TEntityBucket, TPrimaryMapped}.SaveBucketImport(TEntityBucket, IMappedEntity, string, CancellationToken)"/>
		public override async Task SaveBucketImport(SPCompanyEntityBucket bucket, IMappedEntity existing,
			string operation, CancellationToken cancellationToken = default)
		{
			MappedCompany mappedEntity = bucket.Primary as MappedCompany;
			Customer customerResult = null;

			var syncDetailsList = new List<BCSyncDetail>();
			var syncTimeList = new List<DateTime?>(); //Use for calculating the max sync time.
			var existingRoleAssignments = bucket.CustomerLocations?.SelectMany(x => x.RoleAssignments ?? new List<RoleAssignment>()).ToList();

			//After customer created successfully, it ensures system couldn't rollback customer creation if customer location fails later.
			using var transaction = await base.WithTransaction(async () =>
			{
				customerResult = cbapi.Put(mappedEntity.Local, mappedEntity.LocalID);
				syncTimeList.Add(customerResult.SyncTime);
			});
			transaction?.Complete();

			SaveContactsForImport(bucket, customerResult, existingRoleAssignments, syncTimeList, syncDetailsList);

			SaveLocationForImport(bucket, customerResult, existingRoleAssignments, syncTimeList, syncDetailsList);

			mappedEntity.ClearDetails();

			//Update mapped entity and status.
			mappedEntity.AddLocal(customerResult, customerResult.SyncID, syncTimeList.Where(x => x != null).Max());

			//Update sync detail at the end to avoid causing unknown issue if system throws exception during the process
			if (syncDetailsList?.Any() == true)
			{
				syncDetailsList.ForEach(x =>
				{
					mappedEntity.AddDetail(x.EntityType, x.LocalID, x.ExternID);
				});
			}
			UpdateStatus(mappedEntity, operation);
		}

		protected virtual void SaveContactsForImport(SPCompanyEntityBucket bucket, Customer customer, List<RoleAssignment> existingRoleAssignments, List<DateTime?> syncTimeList, List<BCSyncDetail> syncDetailsList)
		{
			MappedCompany mappedEntity = bucket.Primary as MappedCompany;

			//To avoid the duplicates in the contacts: The call to the API that creates the Customer also creates the MainContact entity
			//therefore we must get its NoteID and put it in the instance contained in the list.
			var primaryContact = bucket.CustomerContacts.Where(x => x.ExternalID != null && string.Equals(mappedEntity.Local.PrimaryContact?.ExternalID, x.ExternalID, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			if (primaryContact != null && primaryContact.Id == null)
			{
				primaryContact.NoteID = customer.PrimaryContact?.NoteID;
				primaryContact.Id = customer.PrimaryContact?.NoteID.Value;
			}

			//Create or update contacts.
			foreach (var contact in bucket.CustomerContacts ?? Enumerable.Empty<Contact>())
			{
				Guid? contactNoteID = contact.NoteID?.Value ?? contact.Id;
				contact.BusinessAccount = customer.CustomerID;

				var resultContact = cbapi.Put(contact, contactNoteID);

				contact.ContactID = resultContact.ContactID;
				contact.Id = resultContact.Id;
				contact.Active = resultContact.Active;
				contact.Status = resultContact.Status;
				contact.NoteID = resultContact.NoteID;
				contact.DisplayName = resultContact.DisplayName;

				if (bucket.RoleAssignments?.Any() == true)
				{
					if (resultContact.Active?.Value != true)
					{
						//If contact is inactive, all roleAssignments have been removed from this contact, we should not include these roleAssignments in the list.
						bucket.RoleAssignments = bucket.RoleAssignments.Where(x => (x.LocalContactNoteID != null && x.LocalContactNoteID != resultContact.SyncID)
																			|| x.ExternalContactID != contact.ExternalID.KeySplit(1, contact.ExternalID)).ToList();
					}
					else
					{
						bucket.RoleAssignments.ForEach(x =>
						{
							if (x.ExternalCompanyID == mappedEntity.ExternID && x.ExternalContactID == contact.ExternalID.KeySplit(1, contact.ExternalID))
							{
								x.BAccountID = customer.BAccountID;
								x.ContactID = resultContact.ContactID;
								x.LocalContactNoteID = resultContact.Id;
								if (contact.Status.Value != PX.Objects.CR.Messages.Active)
								{
									x.Delete = true;
								}
							}
						});
					}
				}

				if (existingRoleAssignments?.Any() == true)
				{
					existingRoleAssignments.ForEach(x =>
					{
						if (x.ContactID?.Value == resultContact.ContactID?.Value)
						{
							x.ExternalCompanyID = mappedEntity.ExternID;
							x.ExternalContactID = contact.ExternalID.KeySplit(1, contact.ExternalID);
							if (contact.Status.Value != PX.Objects.CR.Messages.Active)
							{
								x.Delete = true;
							}
						}
					});
				}

				syncTimeList.Add(resultContact.SyncTime);
				syncDetailsList.Add(new BCSyncDetail()
				{
					//In restful order export we don't need ContactId but we need the associated CustomerId, so we have to use the "CustomerId;ContactId" format to save both of them in the SyncDetail
					EntityType = BCEntitiesAttribute.CompanyContact,
					LocalID = contact.NoteID?.Value,
					ExternID = contact.ExternalID
				});
			}
		}

		protected virtual void SaveLocationForImport(SPCompanyEntityBucket bucket, Customer customer, List<RoleAssignment> existingRoleAssignments, List<DateTime?> syncTimeList, List<BCSyncDetail> syncDetailsList)
		{
			MappedCompany mappedEntity = bucket.Primary as MappedCompany;

			var defaultCustomerInfo = SelectFrom<BAccount>
				.LeftJoin<Location>.On<Location.locationID.IsEqual<BAccount.defLocationID>>
				.LeftJoin<PX.Objects.CR.Address>.On<PX.Objects.CR.Address.addressID.IsEqual<BAccount.defAddressID>>
				.Where<BAccount.bAccountID.IsEqual<P.AsInt>>
				.View.Select(this, customer.BAccountID.Value).FirstOrDefault();

			Location defaultLocation = defaultCustomerInfo?.GetItem<Location>();
			PX.Objects.CR.Address defaultAddress = defaultCustomerInfo?.GetItem<PX.Objects.CR.Address>();

			CustomerLocation defaultCustomerLocation = bucket.CustomerLocations?.FirstOrDefault(x => x.Id != null && x.Id.Value == defaultLocation?.NoteID);
			bool defaultLocationAlreadySet = defaultCustomerLocation != null;
			bool shouldUpdateDefaultLocation = defaultCustomerLocation != null && defaultCustomerLocation.Status?.Value != PX.Objects.CR.Messages.Active;
			bool shouldUpdateDefaultAddressWithMainLocation = defaultAddress != null && string.IsNullOrEmpty(defaultAddress.AddressLine1) && string.IsNullOrEmpty(defaultAddress.AddressLine2);

			foreach (var location in bucket.CustomerLocations.OrderByDescending(x => x.Active?.Value) ?? Enumerable.Empty<CustomerLocation>())
			{
				Guid? locationNoteID = location.Id;

				//If default not set, update that.
				if (!defaultLocationAlreadySet && locationNoteID == null)
				{
					locationNoteID = defaultLocation.NoteID;
					defaultLocationAlreadySet = true;
				}

				if (shouldUpdateDefaultLocation && location.Delete != true && (location.Active?.Value == true || location.Status?.Value == PX.Objects.CR.Messages.Active))
				{
					location.Default = true.ValueField();
					shouldUpdateDefaultLocation = false;
				}

				location.Customer = customer.CustomerID;

				if (location.Delete != true && bucket.RoleAssignments?.Any() == true && (location.Active?.Value == true || location.Status?.Value == PX.Objects.CR.Messages.Active))
				{
					bucket.RoleAssignments.ForEach(x =>
					{
						if (x.ExternalCompanyID == mappedEntity.ExternID && x.ExternalLocationID == location.ExternalID)
						{
							x.BAccountID = customer.BAccountID;
							var findMatchedExistingItem = location.RoleAssignments?.FirstOrDefault(r => r.ContactID?.Value == x.ContactID?.Value
								&& r.ExternalLocationID != null && x.ExternalLocationID == r.ExternalLocationID);
							//If found the matched roleAssignment, update the data from external data source; otherwise create new roleAssignment
							if (findMatchedExistingItem != null)
							{
								findMatchedExistingItem.Delete = false;
								findMatchedExistingItem.Role = x.Role;
								findMatchedExistingItem.Impl = null;
								findMatchedExistingItem.Custom = null;
								findMatchedExistingItem.ExternalID = x.ExternalID;
								x.LocationCD = findMatchedExistingItem.LocationCD;
								x.Id = findMatchedExistingItem.Id;
								x.RoleAssignmentID = findMatchedExistingItem.RoleAssignmentID;
							}
							else
								location.RoleAssignments.Add(x);
						}
					});
				}

				CustomerLocation resultLocation = cbapi.Put(location, locationNoteID);
				location.Id = resultLocation.Id;
				location.LocationID = resultLocation.LocationID;
				location.Active = resultLocation.Active;
				location.Status = resultLocation.Status;

				if (shouldUpdateDefaultAddressWithMainLocation && location.Active?.Value == true && locationNoteID == defaultLocation.NoteID)
				{
					var customerMaintGraph = PXGraph.CreateInstance<CustomerMaint>();
					var addressCache = customerMaintGraph.Caches<PX.Objects.CR.Address>();

					shouldUpdateDefaultAddressWithMainLocation = false;
					defaultAddress.AddressLine1 = location.LocationContact?.Address?.AddressLine1?.Value;
					defaultAddress.AddressLine2 = location.LocationContact?.Address?.AddressLine2?.Value;
					defaultAddress.City = location.LocationContact?.Address?.City?.Value;
					defaultAddress.CountryID = location.LocationContact?.Address?.Country?.Value;
					defaultAddress.State = location.LocationContact?.Address?.State?.Value;
					defaultAddress.PostalCode = location.LocationContact?.Address?.PostalCode?.Value;

					addressCache.Update(defaultAddress);
					addressCache.Persist(PXDBOperation.Update);
					addressCache.Persisted(false);
				}

				syncTimeList.Add(resultLocation.SyncTime);
				syncDetailsList.Add(new BCSyncDetail()
				{
					EntityType = BCEntitiesAttribute.CompanyLocation,
					LocalID = location.Id,
					ExternID = location.ExternalID
				});


				foreach (RoleAssignment resultRole in resultLocation?.RoleAssignments ?? Enumerable.Empty<RoleAssignment>())
				{
					syncTimeList.Add(resultRole.LastModifiedDateTime.Value);

					var matchedUnsavedItem = location.RoleAssignments?.FirstOrDefault(externalRole => externalRole.ContactID?.Value == resultRole.ContactID?.Value);
					if (matchedUnsavedItem?.ExternalID != null && resultRole.Delete == false)
					{
						syncDetailsList.Add(new BCSyncDetail()
						{
							EntityType = BCEntitiesAttribute.CompanyRoleAssignment,
							LocalID = resultRole.Id,
							ExternID = matchedUnsavedItem.ExternalID
						});
					}
				}
			}
		}

		#endregion

		#region Export

		///<inheritdoc/>
		public override async Task FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime,
															 PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			var customer = new Customer
			{
				CustomerID = new StringReturn(),
				BAccountID = new IntReturn(),
				IsGuestCustomer = new BooleanReturn(),
				CustomerCategory = new StringSearch { Value = BCCustomerCategoryAttribute.OrganizationValue }
			};

			IEnumerable<Customer> impls = cbapi.GetAll(customer, minDateTime, maxDateTime, filters, cancellationToken: cancellationToken);

			int countNum = 0;
			List<IMappedEntity> mappedList = new List<IMappedEntity>();
			foreach (Customer impl in impls)
			{
				IMappedEntity obj = new MappedCompany(impl, impl.SyncID, impl.SyncTime);

				mappedList.Add(obj);
				countNum++;
				if (countNum % BatchFetchCount == 0)
				{
					ProcessMappedListForExport(mappedList);
				}
			}

			if (mappedList.Any())
			{
				ProcessMappedListForExport(mappedList);
			}
		}

		///<inheritdoc/>
		public override async Task<EntityStatus> GetBucketForExport(SPCompanyEntityBucket bucket, BCSyncStatus status, CancellationToken cancellationToken = default)
		{
			var syncTimeList = new List<DateTime?>(); //Use for calculating the max sync time.
			Customer company = new Customer
			{
				ReturnBehavior = ReturnBehavior.OnlySpecified,
				Attributes = new List<AttributeValue> { new AttributeValue() },
				BillingContact = new Contact(),
				CreditVerificationRules = new CreditVerificationRules(),
				MainContact = new Contact(),
				PrimaryContact = new Contact(),
				ShippingContact = new Contact(),
				Contacts = new List<CustomerContact>{
					new CustomerContact
					{
						Contact = new Contact
						{
							ReturnBehavior = ReturnBehavior.All
						}
					}
				}
			};
			company = cbapi.GetByID(status.LocalID, company, GetCustomFieldsForExport());

			//Ensure that the entity is a company and not a customer (if the user changed it after the sync)
			if (company == null || !BCCustomerCategoryAttribute.IsOrganization(company.CustomerCategory?.Value))
				return EntityStatus.Deleted;

			syncTimeList.Add(company.SyncTime);

			bucket.CustomerContacts = company?.Contacts?.Select(x => x.Contact)?.ToList();
			//The Id inside Contact object is different with the NoteId, the real Id of Contact is under Contacts level, we should replace it.
			bucket.CustomerContacts?.All(x =>
			{
				var matchedItem = company?.Contacts.FirstOrDefault(c => x != null && c.ContactID?.Value == x.ContactID.Value);
				if (matchedItem != null)
				{
					x.Id = matchedItem.Id;
					syncTimeList.Add(matchedItem.LastModifiedDateTime?.Value);
				}

				syncTimeList.Add(x?.SyncTime);
				syncTimeList.Add(x?.LastModifiedDateTime?.Value);

				return true;
			});

			if (company == null) return EntityStatus.None;

			var location = new CustomerLocation
			{
				ReturnBehavior = ReturnBehavior.All,
				Customer = new StringSearch { Value = company.CustomerID.Value },
				RoleAssignments = new List<RoleAssignment> {
					new RoleAssignment
					{
						ReturnBehavior= ReturnBehavior.All,
					}
				}
			};

			bucket.CustomerLocations = cbapi.GetAll(location)?.ToList();
			bucket.CustomerLocations?.ForEach(x =>
			{
				if (x?.SyncTime != null)
					syncTimeList.Add(x.SyncTime);

				if(x?.RoleAssignments?.Any() == true)
				{
					syncTimeList.Add(x.RoleAssignments.Select(r => r.LastModifiedDateTime?.Value).Max());
				}
			});

			bucket.RoleAssignments = bucket.CustomerLocations.Where(x => x.RoleAssignments != null).SelectMany(x => x.RoleAssignments).ToList();

			bucket.Company = bucket.Company.Set(company, company.SyncID, syncTimeList.Where(x => x != null).Max());
			EntityStatus updatedStatus = EnsureStatus(bucket.Company, SyncDirection.Export);

			return updatedStatus;
		}

		///<inheritdoc/>
		public override async Task MapBucketExport(SPCompanyEntityBucket bucket, IMappedEntity existing,
												   CancellationToken cancellationToken = default)
		{
			var external = bucket.Company.Extern = new CompanyDataGQL();
			var local = bucket.Company.Local;

			//Company
			external.Name = local.CustomerName?.Value;
			external.ExternalId = new object[] { local.BAccountID.Value, bucket.Company.LocalID }.KeyCombine();
			external.Note = local.Note;

			//Contacts
			var externalContacts = new List<CompanyContactDataGQL>();
			foreach (var localContact in bucket.CustomerContacts?.Where(contact => contact.Active?.Value == true) ?? Enumerable.Empty<Contact>())
			{
				if (string.IsNullOrEmpty(localContact.Email?.Value))
					throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.CompanyContactWithoutEmail, $"{localContact.ContactID?.Value} - {localContact.DisplayName?.Value}"));

				//because Shopify using Email as Key of the Contact, it should only allow the unique Email to create new contact. 
				if (externalContacts.Any(x => string.Equals(x.Email, localContact.Email?.Value, StringComparison.OrdinalIgnoreCase)))
				{
					continue;
				}

				externalContacts.Add(new CompanyContactDataGQL
				{
					LocalID = localContact.Id,
					LocalContactID = localContact.ContactID?.Value,
					IsMainContact = local.PrimaryContact?.ContactID?.Value == localContact.ContactID?.Value,
					Title = localContact.JobTitle?.Value,
					FirstName = localContact.FirstName?.Value ?? string.Empty,
					LastName = localContact.LastName?.Value ?? string.Empty,
					Email = localContact.Email?.Value,
					Customer = new CustomerDataGQL
					{
						FirstName = localContact.FirstName?.Value ?? string.Empty,
						LastName = localContact.LastName?.Value ?? string.Empty,
						Email = localContact.Email?.Value,
						Phone = GetHelper<SPHelper>().ReplaceStrByOrder(true, localContact.Phone1?.Value, localContact.Phone2?.Value, localContact.Phone3?.Value),
						DefaultAddress = new MailingAddress
						{
							Address2 = localContact.Address?.AddressLine2?.Value?? string.Empty,
							Address1 = GetHelper<SPHelper>().ReplaceStrByOrder(false, localContact.Address?.AddressLine1?.Value, string.Empty),
							City = localContact.Address?.City?.Value,
							CountryCode = localContact.Address?.Country?.Value,
							ProvinceCode = localContact.Address?.State?.Value,
							Zip = localContact.Address?.PostalCode?.Value,
						}
					}
				});
			}
			external.Contacts = new Connection<CompanyContactDataGQL>() { Nodes = externalContacts };

			//Locations
			var externalLocations = new List<CompanyLocationDataGQL>();
			foreach (var localLocation in bucket.CustomerLocations ?? Enumerable.Empty<CustomerLocation>())
			{
				externalLocations.Add(new CompanyLocationDataGQL
				{
					LocalID = localLocation.Id,
					LocalLocationCD = localLocation.LocationID?.Value,
					Name = localLocation.LocationName?.Value ?? local.CustomerName?.Value ?? " _ ",
					Phone = GetHelper<SPHelper>().ReplaceStrByOrder(true, localLocation.LocationContact?.Phone1?.Value, localLocation.LocationContact?.Phone2?.Value, localLocation.LocationContact?.Phone3?.Value),
					ShippingAddress = new CompanyAddressDataGQL
					{
						Address1 = GetHelper<SPHelper>().ReplaceStrByOrder(false, localLocation.LocationContact?.Address?.AddressLine1?.Value, string.Empty),
						Address2 = localLocation.LocationContact?.Address?.AddressLine2?.Value ?? string.Empty,
						City = localLocation.LocationContact?.Address?.City?.Value,
						CountryCode = localLocation.LocationContact?.Address?.Country?.Value,
						Phone = GetHelper<SPHelper>().ReplaceStrByOrder(true, localLocation.LocationContact?.Phone1?.Value, localLocation.LocationContact?.Phone2?.Value, localLocation.LocationContact?.Phone3?.Value),
						ZoneCode = localLocation.LocationContact?.Address?.State?.Value,
						Zip = localLocation.LocationContact?.Address?.PostalCode?.Value,
					}
				});
			}
			external.Locations = new Connection<CompanyLocationDataGQL> { Nodes = externalLocations };
		}

		///<inheritdoc/>
		public override async Task SaveBucketExport(SPCompanyEntityBucket bucket, IMappedEntity existing,
													string operation, CancellationToken cancellationToken = default)
		{
			MappedCompany mappedEntity = bucket.Primary as MappedCompany;
			CompanyDataGQL external = mappedEntity!.Extern;
			CompanyDataGQL existingExternal = existing?.Extern as CompanyDataGQL;

			var existingDetailInfo = mappedEntity.Details?.ToList();
			var syncDetailsList = new List<BCSyncDetail>();
			var syncTimeList = new List<DateTime?>(); //Use for calculating the max sync time.

			CompanyLocationDataGQL createdLocationWithCompany = null;

			CompanyDataGQL companyResult = null;

			if (mappedEntity.ExternID is null)
			{
				//If creating the new company through API, it will auto create the Location at the same time whatever there is a location data or not.
				companyResult = await CompanyDataProvider.CreateCompanyAsync(external, cancellationToken);
				createdLocationWithCompany = companyResult.LocationsList?.FirstOrDefault();
			}
			else
			{
				companyResult = await CompanyDataProvider.UpdateCompanyAsync(mappedEntity.ExternID, external, cancellationToken);
			}
			external.Id = companyResult.Id;
			external.Name = companyResult.Name;
			external.UpdatedAt = companyResult.UpdatedAt;
			syncTimeList.Add(external.UpdatedAt?.ToDate(true));

			//Get the contact roles list from Shopify
			var companyContactRoles = companyResult.ContactRolesList?.ToList();

			// update ExternalRef
			string externalRef = APIHelper.ReferenceMake(external.Id.ConvertGidToId(), GetBinding().BindingName);

			string[] keys = mappedEntity.Local?.AccountRef?.Value?.Split(';');
			if (keys?.Contains(externalRef) != true)
			{
				if (!string.IsNullOrEmpty(mappedEntity.Local?.AccountRef?.Value))
					externalRef = new object[] { mappedEntity.Local?.AccountRef?.Value, externalRef }.KeyCombine();

				if (externalRef.Length < 50)
					PXDatabase.Update<BAccount>(
						new PXDataFieldAssign(nameof(BAccount.acctReferenceNbr), PXDbType.NVarChar, externalRef),
						new PXDataFieldRestrict(nameof(BAccount.noteID), PXDbType.UniqueIdentifier, mappedEntity.Local?.NoteID?.Value)
					);
			}

			#region Export Contacts
			//Contacts
			foreach (var contact in external.Contacts?.Nodes ?? Enumerable.Empty<CompanyContactDataGQL>())
			{
				IEnumerable<CompanyContactDataGQL> existentContacts = existingExternal?.Contacts?.Nodes;
				//Try to find the sync record in SyncDetail first; if not found, find by Email or Phone by order;
				var existingContact = existentContacts?.FirstOrDefault(
					e => existingDetailInfo?.FirstOrDefault(x => x.EntityType == BCEntitiesAttribute.CompanyContact && x.LocalID == contact.LocalID && x.ExternID != null)?.ExternID == e.Id.ConvertGidToId()) ??
					existentContacts?.FirstOrDefault(e => e.Customer?.Email == contact.Email) ??
					existentContacts?.FirstOrDefault(e => !string.IsNullOrEmpty(e.Customer?.Phone) && e.Customer?.Phone == contact.Customer?.Phone);
				CompanyContactDataGQL result = null;

				if (existingContact != null)
				{
					result = await CompanyDataProvider.UpdateCompanyContactAsync(existingContact.Id, contact, cancellationToken);
					if (result.IsMainContact != true && contact.IsMainContact == true)
					{
						companyResult = await CompanyDataProvider.AssignCompanyMainContactAsync(external.Id, existingContact.Id, cancellationToken);
						syncTimeList.Add(companyResult.UpdatedAt?.ToDate(true));
					}
				}
				else
				{
					//If the contact doesn't exist, we need to do extra search to confirm the email is assigned to an existing customer or not.
					//If email belongs to existing customer, we should get this customer and assign to the company as a contact first, and then update the contact based on currrent data.
					//If email is new, we should create a new contact directly.
					CustomerDataGQL existingCustomer = null;
					if (!string.IsNullOrEmpty(contact.Email))
						existingCustomer = (await CompanyDataProvider.GetCompanyCustomerByEmailAsync(contact.Email))?.FirstOrDefault();

					if (existingCustomer != null && existingCustomer.Id != null)
					{
						CompanyContactDataGQL matchingContactByEmail = existentContacts?.FirstOrDefault(externalContact => externalContact.Customer?.Email.Equals(existingCustomer.Email) == true);
						result = matchingContactByEmail ?? await CompanyDataProvider.AssignCustomerAsContactAsync(external.Id, existingCustomer.Id);
						
						if (result?.Id != null)
							result = await CompanyDataProvider.UpdateCompanyContactAsync(result.Id, contact, cancellationToken);
					}
					else
						result = await CompanyDataProvider.CreateCompanyContactAsync(external.Id, contact, cancellationToken);

					if (result.IsMainContact != true && contact.IsMainContact == true)
					{
						companyResult = await CompanyDataProvider.AssignCompanyMainContactAsync(external.Id, result.Id, cancellationToken);
						syncTimeList.Add(companyResult.UpdatedAt?.ToDate(true));
					}
				}

				//create the role assignment for current contact
				bucket.RoleAssignments?.ForEach(x =>
				{
					if (x != null && x.ContactID?.Value == contact.LocalContactID)
					{
						x.ExternalContactID = result?.Id?.ConvertGidToId();
					}
				});

				syncDetailsList.Add(new BCSyncDetail()
				{
					EntityType = BCEntitiesAttribute.CompanyContact,
					LocalID = contact.LocalID,
					ExternID = new object[] { result?.Customer.Id?.ConvertGidToId(), result?.Id?.ConvertGidToId() }.KeyCombine()
				});
				syncTimeList.Add(result?.UpdatedAt?.ToDate(true));
			}

			#endregion

			#region Export Locations
			//Locations
			bool firstLocation = true;
			foreach (var location in external.Locations?.Nodes ?? Enumerable.Empty<CompanyLocationDataGQL>())
			{
				CompanyLocationDataGQL result = null;
				//The first location is exported with new company creation, we should skip it.
				if (firstLocation && createdLocationWithCompany?.Id != null)
				{
					result = createdLocationWithCompany;
				}
				else
				{
					//Try to find the sync record in SyncDetail first; if not found, find by name
					var existingLocation = existingExternal?.Locations?.Nodes?.FirstOrDefault(
						e => existingDetailInfo?.FirstOrDefault(x => x.EntityType == BCEntitiesAttribute.CompanyLocation
															      && x.LocalID == location.LocalID && x.ExternID != null)?.ExternID == e.Id.ConvertGidToId()) ??
						existingExternal?.Locations?.Nodes?.FirstOrDefault(e => e.Name == location.Name);

					if (existingLocation != null)
					{
						result = await CompanyDataProvider.UpdateCompanyLocationAsync(existingLocation.Id, location, cancellationToken);
					}
					else
					{
						result = await CompanyDataProvider.CreateCompanyLocationAsync(external.Id, location, cancellationToken);
					}
				}

				//create the role assignment for current contact
				bucket.RoleAssignments?.ForEach(x =>
				{
					if (x != null && x.LocationCD?.Value == location.LocalLocationCD)
					{
						x.ExternalLocationID = result?.Id?.ConvertGidToId();
						x.ExternalAssignRoleID = companyContactRoles.FirstOrDefault(shopifyRole =>
							BCRoleListAttribute.ConvertExternRoleToLocal(shopifyRole.Name) == x.Role?.Value)?.Id;
						x.ExternalID = existingDetailInfo?.FirstOrDefault(e => e.EntityType == BCEntitiesAttribute.CompanyRoleAssignment && e.LocalID == x.Id && e.ExternID != null)?.ExternID;
					}
				});

				firstLocation = false;
				//If Shopify detects that a location is the same as an existing, it will return the id of the existing location
				//and does not create a new one.
				//Therefore, we end up with two Sync records (one for each location) but both with the same ExternalId.
				//If the merged location is modified in the future, it won't sync but override the only location in Shopify
				//Solution: Before adding the record to SyncDetail, we must ensure that the ExternalId hasn't been added already.
				var externalId = result?.Id?.ConvertGidToId();
				if (!syncDetailsList.Any(x => x.ExternID == externalId && x.EntityType == BCEntitiesAttribute.CompanyLocation))
				{
					syncDetailsList.Add(new BCSyncDetail()
					{
						EntityType = BCEntitiesAttribute.CompanyLocation,
						LocalID = location.LocalID,
						ExternID = result?.Id?.ConvertGidToId()
					});
					syncTimeList.Add(result?.UpdatedAt?.ToDate(true));
				}
			}

			#endregion

			syncDetailsList.AddRange(await SaveRoleAssignmentsExport(bucket, existing, syncTimeList));

			//Clear all detail info from BCSyncDetail
			mappedEntity.ClearDetails();

			mappedEntity.AddExtern(external, external.Id.ConvertGidToId(), external.Name, syncTimeList.Where(x => x != null).Max());

			//Update sync detail at the end to avoid causing unknown issue if system throws exception during the process
			if (syncDetailsList?.Any() == true)
			{
				syncDetailsList.ForEach(x =>
				{
					mappedEntity.AddDetail(x.EntityType, x.LocalID, x.ExternID);
				});
			}

			UpdateStatus(mappedEntity, operation);
		}
		/// <summary>
		/// Compares the existing role assignments on Shopify and those existing locally.
		/// Returns the list of assignments to be deleted on Shopify.
		/// </summary>
		/// <param name="existingExternal"></param>
		/// <param name="localRoleAssignement"></param>
		/// <param name="bucket"></param>
		/// <param name="entitySetupDirection"></param>
		/// <returns></returns>
		protected virtual IEnumerable<CompanyContactAssignRoles> GetRoleAssigmentsToRevokeForExport(SPCompanyEntityBucket bucket, CompanyDataGQL existingExternal,
			IEnumerable<RoleAssignment> localRoleAssignement, string entitySetupDirection = BCSyncDirectionAttribute.Export)
		{
			if (existingExternal?.ContactsList?.Any() != true)
				return Enumerable.Empty<CompanyContactAssignRoles>();

			var revokeAssignRolesList = new List<CompanyContactAssignRoles>();

			foreach (var externalRA in existingExternal.ContactsList.Where(x => x.RoleAssignmentsList?.Any() == true))
			{
				var contactId = externalRA.Id.ConvertGidToId();
				//Does it exist in the current bucket?
				var localRolesForContactId = localRoleAssignement?.Where(x => x.ExternalContactID != null && x.ExternalLocationID != null && x.ExternalAssignRoleID != null && x.ExternalContactID == contactId) ?? Enumerable.Empty<RoleAssignment>();

				//Get the roles to revoke
				foreach (var externalRole in externalRA.RoleAssignmentsList)
				{
					var localRole = localRolesForContactId?.FirstOrDefault(x => x.ExternalID == externalRole.Id.ConvertGidToId());
					var canAddToRevokeList = localRole == null;

					if (canAddToRevokeList)
					{
						var roleAssignment = new RoleAssignment
						{
							ExternalCompanyID = externalRole.Company?.Id,
							ExternalContactID = externalRole.CompanyContact?.Id.ConvertGidToId(),
							ExternalLocationID = externalRole.CompanyLocation?.Id,
							ExternalAssignRoleID = externalRole.Id
						};
						AddRoleToCompanyContactAssignedRoles(revokeAssignRolesList, roleAssignment, externalRole?.Id);
					}
				}
			}

			return revokeAssignRolesList;
		}


		protected virtual async Task<IEnumerable<BCSyncDetail>> SaveRoleAssignmentsExport(SPCompanyEntityBucket bucket, IMappedEntity existing, List<DateTime?> syncTimeList)
		{
			CompanyDataGQL existingExternal = existing?.Extern as CompanyDataGQL;
			var assignRolesList = new List<CompanyContactAssignRoles>(); //For creating new assign role for location;
			var revokeAssignRolesList = GetRoleAssigmentsToRevokeForExport(bucket, existingExternal, bucket.RoleAssignments, GetEntity().Direction).ToList();
			var syncDetailsList = new List<BCSyncDetail>();

			//RoleAssignments
			foreach (var roleAssignment in bucket.RoleAssignments?.Where(x => x.ExternalContactID != null && x.ExternalLocationID != null
																		   && x.ExternalAssignRoleID != null) ?? Enumerable.Empty<RoleAssignment>())
			{
				var existingExternRoleItem = existingExternal?.Contacts?.Nodes?
					.Where(x => x.Id.ConvertGidToId() == roleAssignment.ExternalContactID && x.RoleAssignmentsList?.Any() == true)?
					.SelectMany(r => r.RoleAssignmentsList)?.FirstOrDefault(ra => ra.CompanyLocation?.Id?.ConvertGidToId() == roleAssignment.ExternalLocationID);

				if (existingExternRoleItem != null)
				{
					if (existingExternRoleItem.Role?.Id?.ConvertGidToId() == roleAssignment.ExternalAssignRoleID.ConvertGidToId())
					{
						//If external record is existing and no changes, update the sync detail only, don't need to send data to Shopify again.						
						syncDetailsList.Add(new BCSyncDetail()
						{
							EntityType = BCEntitiesAttribute.CompanyRoleAssignment,
							LocalID = roleAssignment.Id,
							ExternID = existingExternRoleItem.Id?.ConvertGidToId()
						});

						syncTimeList.Add(existingExternRoleItem.UpdatedAt?.ToDate(true));
						continue;
					}
					else
					{
						//if record is created, add the revoke assigned role for location to the record; otherwise create a revoke record.
						AddRoleToCompanyContactAssignedRoles(revokeAssignRolesList, roleAssignment, existingExternRoleItem?.Id);
					}
				}
				//if record is created, add the new role for location to the record; otherwise create a new record.
				AddRoleToCompanyContactAssignedRoles(assignRolesList, roleAssignment);
			}

			//Revoke old AssignRoles first
			foreach (var assignRoleItem in revokeAssignRolesList)
			{
				List<string> roleAssignmentIds = assignRoleItem.RolesToAssigns.Select(x => x.RoleAssignmentId).Distinct().ToList();
				if (roleAssignmentIds?.Any() == true)
					await CompanyDataProvider.RevokeCompanyContactRolesAsync(assignRoleItem.CompanyContactId, roleAssignmentIds);
			}

			//Push AssignRoles to Shopify
			foreach (var assignRoleItem in assignRolesList)
			{
				var resultList = await CompanyDataProvider.AssignCompanyContactRolesAsync(assignRoleItem.CompanyContactId, assignRoleItem.RolesToAssigns);
				if (resultList?.Any() == true)
				{
					foreach (var contactRoleAssignment in resultList)
					{
						var matchedItem = bucket.RoleAssignments.FirstOrDefault(r => r.ExternalContactID == contactRoleAssignment.CompanyContact?.Id?.ConvertGidToId()
																			 && r.ExternalLocationID == contactRoleAssignment.CompanyLocation?.Id?.ConvertGidToId());
						if (matchedItem != null)
						{
							syncDetailsList.Add(new BCSyncDetail() { EntityType = BCEntitiesAttribute.CompanyRoleAssignment,
																	 LocalID = matchedItem.Id, ExternID = contactRoleAssignment.Id?.ConvertGidToId() });							
						}
						syncTimeList.Add(contactRoleAssignment.UpdatedAt?.ToDate(true));
						syncTimeList.Add(contactRoleAssignment.Company?.UpdatedAt?.ToDate(true));
						syncTimeList.Add(contactRoleAssignment.CompanyLocation?.UpdatedAt?.ToDate(true));
						syncTimeList.Add(contactRoleAssignment.CompanyContact?.UpdatedAt?.ToDate(true));
					}
				}
			}
			
			return syncDetailsList;
		}

		protected virtual void AddRoleToCompanyContactAssignedRoles(List<CompanyContactAssignRoles> assignRolesList, RoleAssignment roleAssignment, string externRoleAssignmentId = null)
		{
			var existingAssignRoleRecord = assignRolesList.FirstOrDefault(x => x.CompanyContactId == roleAssignment.ExternalContactID.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyContact));
			//if record is created, add the new role for location to the record; otherwise create a new record.
			if (existingAssignRoleRecord != null)
			{
				existingAssignRoleRecord.RolesToAssigns.Add(new CompanyContactRoleAssign()
				{
					RoleAssignmentId = externRoleAssignmentId,
					CompanyContactRoleId = roleAssignment.ExternalAssignRoleID,
					CompanyLocationId = roleAssignment.ExternalLocationID.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyLocation)
				});
			}
			else
			{
				assignRolesList.Add(new CompanyContactAssignRoles()
				{
					CompanyContactId = roleAssignment.ExternalContactID.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyContact),
					RolesToAssigns = new List<CompanyContactRoleAssign>() { new CompanyContactRoleAssign() {
							RoleAssignmentId = externRoleAssignmentId,
							CompanyContactRoleId = roleAssignment.ExternalAssignRoleID,
							CompanyLocationId = roleAssignment.ExternalLocationID.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyLocation)
					} }
				});
			}
		}

		#endregion
	}
}
