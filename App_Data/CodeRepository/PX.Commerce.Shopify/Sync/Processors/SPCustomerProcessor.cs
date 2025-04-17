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

using PX.Api.ContractBased.Models;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Shopify.API.REST;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using Location = PX.Objects.CR.Standalone.Location;
using System.Threading.Tasks;
using System.Threading;
using PX.Commerce.Shopify.API.GraphQL;
using PX.Objects.Extensions.MultiCurrency;

namespace PX.Commerce.Shopify
{
	public class SPCustomerEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary { get => Customer; }
		public IMappedEntity[] Entities => new IMappedEntity[] { Customer };
		public CustomerLocation ConnectorGeneratedAddress;
		public MappedCustomer Customer;
		public List<MappedLocation> CustomerAddresses;
		public MappedLocation CustomerAddress;

	}

	public class SPCustomerRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			return base.Restrict<MappedCustomer>(mapped, delegate (MappedCustomer obj)
			{
				BCBindingExt bindingExt = processor.GetBindingExt<BCBindingExt>();

				var customer = obj.Local;

				//Confirm it is not a guest customer
				if (customer != null && (customer.IsGuestCustomer.Value == true || bindingExt.GuestCustomerID == customer.BAccountID.Value))
				{
					return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogCustomerSkippedGuest, customer.CustomerID?.Value ?? obj.Local.SyncID.ToString()));
				}

				//Ignore organization customer category,
				if (customer != null && BCCustomerCategoryAttribute.IsOrganization(customer?.CustomerCategory?.Value))				
				{
					return new FilterResult(FilterStatus.Ignore);
				}

				return null;
			});
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			return base.Restrict<MappedCustomer>(mapped, delegate (MappedCustomer obj)
			{
				CustomerData data = obj.Extern;
				if (data != null && string.IsNullOrEmpty(data.FirstName) && string.IsNullOrEmpty(data.LastName))
				{
					return new FilterResult(FilterStatus.Ignore,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.CustomerNameIsEmpty, data.Id));
				}
			return null;
			});
		}
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.Customer, BCCaptions.Customer, 10,
		IsInternal = false,
		Direction = SyncDirection.Bidirect,
		PrimaryDirection = SyncDirection.Import,
		PrimarySystem = PrimarySystem.Extern,
		PrimaryGraph = typeof(PX.Objects.AR.CustomerMaint),
		ExternTypes = new Type[] { typeof(CustomerData) },
		LocalTypes = new Type[] { typeof(Customer) },
		AcumaticaPrimaryType = typeof(PX.Objects.AR.Customer),
		AcumaticaPrimarySelect = typeof(PX.Objects.AR.Customer.acctCD),
		URL = "customers/{0}",
		Requires = new string[] { }
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = true, Hidden = true,
		PushSources = new String[] { "BC-PUSH-Customers", "BC-PUSH-Locations" }, PushDestination = BCConstants.PushNotificationDestination,
	WebHookType = typeof(WebHookMessage),
		WebHooks = new String[]
		{
			"customers/create",
			"customers/update",
			"customers/delete",
			"customers/disable",
			"customers/enable"
		})]
	[BCProcessorExternCustomField(BCConstants.ShopifyMetaFields, ShopifyCaptions.Metafields, nameof(CustomerData.Metafields), typeof(CustomerData))]
	public class SPCustomerProcessor : SPLocationBaseProcessor<SPCustomerProcessor, SPCustomerEntityBucket, MappedCustomer>
	{
		bool isLocationActive;
		protected IMetafieldsGQLDataProvider metafieldDataGQLProvider;

		[InjectDependency]
		public ISPGraphQLDataProviderFactory<MetaFielsGQLDataProvider> metafieldGrahQLDataProviderFactory { get; set; }

		[InjectDependency]
		public ISPGraphQLAPIClientFactory shopifyGraphQLClientFactory { get; set; }

		[InjectDependency]
		public ISPMetafieldsMappingServiceFactory spMetafieldsMappingServiceFactory { get; set; }

		public SPGraphQLAPIClient shopifyGraphQLClient { get; set; }

		private ISPMetafieldsMappingService metafieldsMappingService;

		
		#region Initialization
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
			isLocationActive = ConnectorHelper.GetConnectorBinding(operation.ConnectorType, operation.Binding).ActiveEntities.Any(x => x == BCEntitiesAttribute.Address);
			if (isLocationActive)
			{
				locationProcessor = PXGraph.CreateInstance<SPLocationProcessor>();
				await locationProcessor.Initialise(iconnector, operation.Clone().With(_ => { _.EntityType = BCEntitiesAttribute.Address; return _; }));
			}

			var graphQLClient = shopifyGraphQLClientFactory.GetClient(GetBindingExt<BCBindingShopify>());
			metafieldDataGQLProvider = metafieldGrahQLDataProviderFactory.GetProvider(graphQLClient);
			metafieldsMappingService = spMetafieldsMappingServiceFactory.GetInstance(metafieldDataGQLProvider);
		}
		#endregion

		#region Common

		public override async Task<PXTransactionScope> WithTransaction(Func<Task> action)
		{
			await action();
			return null;
		}

		public override List<(string fieldName, string fieldValue)> GetExternCustomFieldList(BCEntity entity, ExternCustomFieldInfo customFieldInfo)
		{
			List<(string fieldName, string fieldValue)> fieldsList = new List<(string fieldName, string fieldValue)>() { (BCConstants.MetafieldFormat, BCConstants.MetafieldFormat) };
			return fieldsList;
		}
		public override void ValidateExternCustomField(BCEntity entity, ExternCustomFieldInfo customFieldInfo, string sourceObject, string sourceField, string targetObject, string targetField, EntityOperationType direction)
		{
			//Skip formula expression
			if (direction == EntityOperationType.ImportMapping && sourceField.StartsWith("="))
				return;
			//Validate the field format
			var fieldStrGroup = direction == EntityOperationType.ImportMapping ? sourceField.Split('.') : targetField.Split('.');
			if (fieldStrGroup.Length == 2)
			{
				var keyFieldName = fieldStrGroup[0].Replace("[", "").Replace("]", "").Replace(" ", "");
				if (!string.IsNullOrWhiteSpace(keyFieldName) && string.Equals(keyFieldName, BCConstants.MetafieldFormat, StringComparison.OrdinalIgnoreCase) == false)
					return;
			}
			throw new PXException(BCMessages.InvalidFilter, direction == EntityOperationType.ImportMapping ? "Source" : "Target", BCConstants.MetafieldFormat);
		}
		public override object GetExternCustomFieldValue(SPCustomerEntityBucket entity, ExternCustomFieldInfo customFieldInfo, object sourceData, string sourceObject, string sourceField, out string displayName)
		{
			var sourceinfo = sourceField.Split('.');
			var metafields = GetPropertyValue(sourceData, sourceObject.Replace(" -> ", "."), out displayName);
			if (metafields != null && (metafields.GetType().IsGenericType && (metafields.GetType().GetGenericTypeDefinition() == typeof(List<>) || metafields.GetType().GetGenericTypeDefinition() == typeof(IList<>))))
			{
				var metafieldsList = (System.Collections.IList)metafields;
				if (sourceinfo.Length == 2)
				{
					var nameSpaceField = sourceinfo[0].Replace("[", "").Replace("]", "")?.Trim();
					var keyField = sourceinfo[1].Replace("[", "").Replace("]", "")?.Trim();
					foreach (object metaItem in metafieldsList)
					{
						if (metaItem is MetafieldData)
						{
							var metaField = metaItem as MetafieldData;
							if (metaField != null && string.Equals(metaField.Namespace, nameSpaceField, StringComparison.OrdinalIgnoreCase) && string.Equals(metaField.Key, keyField, StringComparison.OrdinalIgnoreCase))
							{
								return metaField.Value;
							}
						}
					}

				}
			}
			return null;
		}
		public override void SetExternCustomFieldValue(SPCustomerEntityBucket entity, ExternCustomFieldInfo customFieldInfo, object targetData, string targetObject, string targetField, string sourceObject, object value, IMappedEntity existing)
		{
			if (value != PXCache.NotSetValue && value != null)
			{
				var targetinfo = targetField?.Split('.');
				if (targetinfo.Length == 2)
				{
					var nameSpaceField = targetinfo[0].Replace("[", "").Replace("]", "")?.Trim();
					var keyField = targetinfo[1].Replace("[", "").Replace("]", "")?.Trim();
					CustomerData data = (CustomerData)entity.Primary.Extern;
					CustomerData existingCustomer = existing?.Extern as CustomerData;
					var metaFieldList = data.Metafields = data.Metafields ?? new List<MetafieldData>();

					var entityType = ShopifyGraphQLConstants.OWNERTYPE_CUSTOMER;

					//Correct the metafield name - metafield names are case sensitive. But if we submit the same metafield name with different case, Shopify will raise an error
					//thus, we must check whethe the metafield exists or not and correct the name if needed
					var correctedName = metafieldsMappingService.CorrectMetafieldName(entityType, nameSpaceField, keyField, existingCustomer?.Metafields);
					nameSpaceField = correctedName.Item1;
					keyField = correctedName.Item2;

					var metafieldValue = metafieldsMappingService.GetFormattedMetafieldValue(entityType, nameSpaceField, keyField, Convert.ToString(value));

					var newMetaField = new MetafieldData()
					{
						Namespace = nameSpaceField,
						Key = keyField,
						Value = metafieldValue.Value,
						Type = metafieldValue.GetShopifyType()
					};
					if (existingCustomer != null && existingCustomer.Metafields?.Count > 0)
					{
						var existedMetaField = existingCustomer.Metafields.FirstOrDefault(x => string.Equals(x.Namespace, nameSpaceField, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Key, keyField, StringComparison.OrdinalIgnoreCase));
						newMetaField.Id = existedMetaField?.Id;
					}
					var matchedData = metaFieldList.FirstOrDefault(x => string.Equals(x.Namespace, nameSpaceField, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Key, keyField, StringComparison.OrdinalIgnoreCase));
					if (matchedData != null)
					{
						matchedData = newMetaField;
					}
					else
						metaFieldList.Add(newMetaField);
				}
			}
		}
		public override async Task<MappedCustomer> PullEntity(Guid? localID, Dictionary<string, object> externalInfo, CancellationToken cancellationToken = default)
		{
			Customer impl = cbapi.GetByID(localID,
				new Customer()
				{
					ReturnBehavior = ReturnBehavior.OnlySpecified,
					Attributes = new List<AttributeValue>() { new AttributeValue() },
					BillingContact = new Core.API.Contact(),
					CreditVerificationRules = new CreditVerificationRules(),
					MainContact = new Core.API.Contact(),
					PrimaryContact = new Core.API.Contact(),
					ShippingContact = new Core.API.Contact()
				});
			
			if (impl == null || !BCCustomerCategoryAttribute.IsIndividual(impl.CustomerCategory?.Value)) return null;
			MappedCustomer obj = new MappedCustomer(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}

		//TODO
		public override async Task<MappedCustomer> PullEntity(String externID, String externalInfo, CancellationToken cancellationToken = default)
		{
			CustomerData data = customerDataProvider.GetByID(externID);
			if (data == null) return null;

			MappedCustomer obj = new MappedCustomer(data, data.Id?.ToString(), data.Email, data.DateModifiedAt.ToDate(false));
			return obj;
		}

		public override async Task<PullSimilarResult<MappedCustomer>> PullSimilar(IExternEntity entity, CancellationToken cancellationToken = default)
		{
			CustomerData externCustomer = ((CustomerData)entity);
			bool fetchByEmail = string.IsNullOrEmpty(externCustomer?.Email) == false;
			string uniqueField = fetchByEmail ? externCustomer?.Email : externCustomer?.Phone;
			List <MappedCustomer> result = new List<MappedCustomer>();

			PXResultset<PX.Objects.AR.Customer> listOfCustomers = fetchByEmail
								? GetHelper<SPHelper>().CustomerByEmail.Select(uniqueField)
								: string.IsNullOrWhiteSpace(externCustomer?.Phone) == false
									? GetHelper<SPHelper>().CustomerByPhone.Select(uniqueField, uniqueField)
									: new PXResultset<PX.Objects.AR.Customer>();


			foreach (PX.Objects.AR.Customer localCustomer in listOfCustomers)
			{
				if (BCCustomerCategoryAttribute.IsIndividual(localCustomer.GetExtension<CustomerExt>()?.CustomerCategory))					
				{
					Customer data = new Customer()
					{
						SyncID = localCustomer.NoteID,
						CustomerClass = localCustomer.CustomerClassID.ValueField(),
						SendInvoicesbyEmail = localCustomer.MailInvoices.ValueField(),
						SendDunningLettersbyEmail = localCustomer.MailDunningLetters.ValueField(),
						SendStatementsbyEmail = localCustomer.SendStatementByEmail.ValueField(),
						PrintDunningLetters = localCustomer.PrintDunningLetters.ValueField(),
						LastModifiedDateTime = localCustomer.LastModifiedDateTime.ValueField(),
						SyncTime = localCustomer.LastModifiedDateTime
					};
					result.Add(new MappedCustomer(data, data.SyncID, data.SyncTime));
					BCExtensions.SetSharedSlot<Customer>(this.GetType(), data.SyncID?.ToString(), data);
				}
			}

			if (result == null || result?.Count == 0) return null;

			return new PullSimilarResult<MappedCustomer>() { UniqueField = uniqueField, Entities = result };
		}

		public override async Task<PullSimilarResult<MappedCustomer>> PullSimilar(ILocalEntity entity, CancellationToken cancellationToken = default)
		{
			var uniqueField = ((Customer)entity)?.MainContact?.Email?.Value;
			string queryField = string.Empty;

			if (!string.IsNullOrEmpty(uniqueField))
			{
				queryField = nameof(CustomerData.Email);
			}
			else if (string.IsNullOrEmpty(uniqueField) && !string.IsNullOrWhiteSpace(((Customer)entity)?.MainContact?.Phone1?.Value))
			{
				queryField = nameof(CustomerData.Phone);
				uniqueField = ((Customer)entity)?.MainContact?.Email?.Value;
			}
			else
				return null;
			IEnumerable<CustomerData> datas = customerDataProvider.GetByQuery(queryField, uniqueField, true, cancellationToken);
			if (datas == null) return null;

			return new PullSimilarResult<MappedCustomer>() { UniqueField = uniqueField, Entities = datas.Select(data => new MappedCustomer(data, data.Id.ToString(), data.Email, data.DateModifiedAt.ToDate(false))) };
		}

		#endregion

		#region Import
		public override async Task FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			FilterCustomers filter = new FilterCustomers() { Order = "updated_at asc" };
			if (minDateTime != null)
				filter.UpdatedAtMin = minDateTime.Value.ToLocalTime().AddSeconds(-GetBindingExt<BCBindingShopify>().ApiDelaySeconds ?? 0);
			if (maxDateTime != null)
				filter.UpdatedAtMax = maxDateTime.Value.ToLocalTime();
			filter.Fields = BCRestHelper.PrepareFilterFields(typeof(CustomerData), filters, "id", "first_name", "last_name", "updated_at");

			IEnumerable<CustomerData> datas = customerDataProvider.GetAll(filter, cancellationToken: cancellationToken);

			int countNum = 0;
			List<IMappedEntity> mappedList = new List<IMappedEntity>();
			try
			{
				foreach (CustomerData data in datas)
				{
					IMappedEntity obj = new MappedCustomer(data, data.Id?.ToString(), data.Email, data.DateModifiedAt.ToDate(false));
					mappedList.Add(obj);
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
		public override async Task<EntityStatus> GetBucketForImport(SPCustomerEntityBucket bucket, BCSyncStatus bcstatus, CancellationToken cancellationToken = default)
		{
			CustomerData data = BCExtensions.GetSharedSlot<CustomerData>(bcstatus.ExternID) ?? customerDataProvider.GetByID(bcstatus.ExternID, includeAllAddresses: true);
			if (data == null) return EntityStatus.None;

			MappedCustomer obj = bucket.Customer = bucket.Customer.Set(data, data.Id?.ToString(), data.Email, data.DateModifiedAt.ToDate(false));
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			return status;
		}

		public override async Task MapBucketImport(SPCustomerEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedCustomer customerObj = bucket.Customer;
			Customer customerImpl = customerObj.Local = new Customer();
			customerImpl.Custom = GetCustomFieldsForImport();
			Customer presented = existing?.Local != null ? existing?.Local as Customer : null;

			//Existing
			PX.Objects.AR.Customer customer = PXSelect<PX.Objects.AR.Customer, Where<PX.Objects.AR.Customer.noteID, Equal<Required<PX.Objects.AR.Customer.noteID>>>>.Select(this, customerObj.LocalID);

			string customerClassID = presented?.CustomerClass?.Value ?? GetBindingExt<BCBindingExt>()?.CustomerClassID;
			PX.Objects.AR.CustomerClass customerClass = SelectFrom<PX.Objects.AR.CustomerClass>
					.Where<PX.Objects.AR.CustomerClass.customerClassID.IsEqual<P.AsString>>
					.View.Select(this, customerClassID);

			//General Info
			string firstLastName = (customerObj.Extern.FirstName?.Equals(customerObj.Extern.LastName, StringComparison.OrdinalIgnoreCase)) == false ?
				(String.Concat(customerObj.Extern.FirstName ?? string.Empty, " ", customerObj.Extern.LastName ?? string.Empty)).Trim() : customerObj.Extern.FirstName ?? string.Empty;
			bool useCustomerClassValues = presented == null || presented.LastModifiedDateTime.Value < customerClass.LastModifiedDateTime;
			customerImpl.AccountRef = APIHelper.ReferenceMake(customerObj.Extern.Id, currentBinding.BindingName).ValueField();
			customerImpl.CurrencyID = customerObj.Extern.Currency.ValueField();
			customerImpl.CustomerClass = customerClassID.ValueField();
			customerImpl.CustomerCategory = BCCustomerCategoryAttribute.IndividualValue.ValueField();
			customerImpl.PriceClassID = (useCustomerClassValues)
				? customerClass?.PriceClassID.ValueField() ?? new StringValue() { Value = null }
				: presented?.PriceClassID ?? new StringValue() { Value = null };

			//Primary Contact
			customerImpl.PrimaryContact = new Core.API.Contact();
			customerImpl.PrimaryContact.LastName = (string.IsNullOrWhiteSpace(customerObj.Extern.LastName) ? customerObj.Extern.FirstName : customerObj.Extern.LastName).ValueField();
			if (!string.Equals(customerImpl.PrimaryContact.LastName?.Value, customerObj.Extern.FirstName, StringComparison.OrdinalIgnoreCase))
				customerImpl.PrimaryContact.FirstName = customerObj.Extern.FirstName.ValueField();

			//Main Contact
			Core.API.Contact contactImpl = customerImpl.MainContact = new Core.API.Contact();
			bool hasEmail = !string.IsNullOrWhiteSpace(customerObj.Extern.Email);

			contactImpl.FirstName = customerObj.Extern.FirstName.ValueField();
			contactImpl.LastName = customerObj.Extern.LastName.ValueField();
			contactImpl.Attention = firstLastName.ValueField();
			contactImpl.Email = customerObj.Extern.Email.ValueField();
			contactImpl.Phone1 = customerObj.Extern.Phone.ValueField();
			contactImpl.Note = customerObj.Extern.Note;
			contactImpl.Active = true.ValueField();
			contactImpl.DoNotEmail = (hasEmail == false).ValueField();
			if (hasEmail)
			{
				customerImpl.SendDunningLettersbyEmail = (useCustomerClassValues)
					? customerClass?.MailDunningLetters.ValueField() ?? false.ValueField()
					: presented?.SendDunningLettersbyEmail ?? false.ValueField();
				customerImpl.SendInvoicesbyEmail = (useCustomerClassValues)
					? customerClass?.MailInvoices.ValueField() ?? false.ValueField()
					: presented?.SendInvoicesbyEmail ?? false.ValueField();
				customerImpl.SendStatementsbyEmail = (useCustomerClassValues)
					? customerClass?.SendStatementByEmail.ValueField() ?? false.ValueField()
					: presented?.SendStatementsbyEmail ?? false.ValueField();
			}
			else
			{
				customerImpl.SendDunningLettersbyEmail = false.ValueField();
				customerImpl.SendInvoicesbyEmail = false.ValueField();
				customerImpl.SendStatementsbyEmail = false.ValueField();
			}

			customerImpl.PrintDunningLetters = (useCustomerClassValues)
				? customerClass?.PrintDunningLetters.ValueField() ?? false.ValueField()
				: presented?.PrintDunningLetters ?? false.ValueField();
			customerImpl.PrintInvoices = (useCustomerClassValues)
				? customerClass?.PrintInvoices.ValueField() ?? false.ValueField()
				: presented?.PrintInvoices ?? false.ValueField();
			customerImpl.PrintStatements = (useCustomerClassValues)
				? customerClass?.PrintStatements.ValueField() ?? false.ValueField()
				: presented?.PrintStatements ?? false.ValueField();

			//Address
			Core.API.Address addressImpl = contactImpl.Address = new Core.API.Address();
			bucket.CustomerAddresses = new List<MappedLocation>();
			StringValue shipVia = null;
			string countryCodeFromCustomerClass = null;
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
				if (customerClass != null)
				{
					countryCodeFromCustomerClass = customerClass.CountryID; // no address is present then set country from customer class
					shipVia = customerClass.ShipVia.ValueField();
					if (customerClass.OrgBAccountID != null)
						customerImpl.RestrictVisibilityTo = BAccount.PK.Find(this, customerClass.OrgBAccountID)?.AcctCD?.ValueField();
				}
			CustomerAddressData addressObj = null;

			if (customerObj.Extern.Addresses?.Count() > 0)
			{
				addressObj = customerObj.Extern.Addresses.FirstOrDefault(x => x.Default == true);
				if (string.IsNullOrWhiteSpace(customerObj.Extern.Phone) && !string.IsNullOrWhiteSpace(addressObj.Phone))
				{
					contactImpl.Phone1 = addressObj.Phone.ValueField();
					contactImpl.Phone2 = string.Empty.ValueField();
				}
				else if (!string.IsNullOrWhiteSpace(addressObj.Phone) && !addressObj.Phone.Equals(customerObj.Extern.Phone, StringComparison.OrdinalIgnoreCase))
				{
					contactImpl.Phone1 = customerObj.Extern.Phone.ValueField();
					contactImpl.Phone2 = addressObj.Phone.ValueField();
				}
				else if (string.IsNullOrWhiteSpace(customerObj.Extern.Phone) && string.IsNullOrWhiteSpace(addressObj.Phone))
				{
					contactImpl.Phone1 = string.Empty.ValueField();
					contactImpl.Phone2 = string.Empty.ValueField();
				}
				addressImpl.AddressLine1 = addressObj.Address1.ValueField() ?? string.Empty.ValueField();
				addressImpl.AddressLine2 = addressObj.Address2.ValueField() ?? string.Empty.ValueField();
				addressImpl.City = addressObj.City.ValueField();
				addressImpl.Country = (string.IsNullOrEmpty(addressObj.CountryCode) ? countryCodeFromCustomerClass : addressObj.CountryCode).ValueField();
				if (!string.IsNullOrEmpty(addressObj.ProvinceCode))
				{
					addressImpl.State = GetHelper<SPHelper>().GetSubstituteLocalByExtern(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.State), addressObj.ProvinceCode, addressObj.ProvinceCode)?.ValueField();
				}
				else
					addressImpl.State = string.Empty.ValueField();
				addressImpl.PostalCode = addressObj.PostalCode?.ToUpperInvariant()?.ValueField();

				if (isLocationActive)
				{
					var addressStatus = SelectStatusChildren(customerObj.SyncID).Where(s => s.EntityType == BCEntitiesAttribute.Address)?.ToList();

					List<PXResult<Location, PX.Objects.CR.Address, PX.Objects.CR.Contact, BAccount>> pXResult = null;
					if (existing != null && addressStatus?.Count() == 0)// merge locations for clearsync or if no sync status hystory exists for existing customer /Locations
					{
						pXResult = PXSelectJoin<Location, InnerJoin<PX.Objects.CR.Address, On<Location.defAddressID, Equal<PX.Objects.CR.Address.addressID>>,
						   InnerJoin<PX.Objects.CR.Contact, On<Location.defContactID, Equal<PX.Objects.CR.Contact.contactID>>,
						   InnerJoin<BAccount, On<Location.bAccountID, Equal<BAccount.bAccountID>>>>>
						   , Where<BAccount.noteID, Equal<Required<BAccount.noteID>>>>.
						   Select(this, existing?.LocalID).AsEnumerable().
						   Cast<PXResult<Location, PX.Objects.CR.Address, PX.Objects.CR.Contact, BAccount>>().ToList();
					}
					foreach (CustomerAddressData externLocation in customerObj.Extern.Addresses)
					{
						MappedLocation mappedLocation = new MappedLocation(externLocation, new object[] { customerObj.ExternID, externLocation.Id }.KeyCombine(), customerObj.Extern.Email, externLocation.CalculateHash(), customerObj.SyncID);

						mappedLocation.Local = MapLocationImport(externLocation, customerObj);

						//Assign the country code from customer class if it's empty in external system
						if (string.IsNullOrEmpty(mappedLocation.Local.LocationContact.Address.Country?.Value) && countryCodeFromCustomerClass != null)
							mappedLocation.Local.LocationContact.Address.Country = countryCodeFromCustomerClass.ValueField();

						if (addressObj.Id == externLocation.Id && !addressStatus.Any(y => y.ExternID.KeySplit(1) == externLocation.Id.ToString()))
							mappedLocation.Local.ShipVia = shipVia;
						locationProcessor.RemapBucketImport(new SPLocationEntityBucket() { Address = mappedLocation }, null);
						if (pXResult != null)
							mappedLocation.LocalID = CheckIfExists(mappedLocation.Extern, pXResult, bucket.CustomerAddresses);

						bucket.CustomerAddresses.Add(mappedLocation);
					}

				}
			}
			else if (existing == null)
			{
				bucket.ConnectorGeneratedAddress = new CustomerLocation();
				bucket.ConnectorGeneratedAddress.ShipVia = shipVia;
				bucket.ConnectorGeneratedAddress.ContactOverride = true.ValueField();
				bucket.ConnectorGeneratedAddress.AddressOverride = true.ValueField();
			}
			customerImpl.CustomerName = (firstLastName ?? addressObj?.Company ?? customerObj.ExternID.ToString()).ValueField();
			contactImpl.CompanyName = contactImpl.FullName = string.IsNullOrWhiteSpace(addressObj?.Company) ? customerImpl.CustomerName : addressObj?.Company?.ValueField();
		}

		public override async Task SaveBucketImport(SPCustomerEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
			MappedCustomer obj = bucket.Customer;
			Customer impl = null;
			//After customer created successfully, it ensures system couldn't rollback customer creation if customer location fails later.
			using (var transaction = await base.WithTransaction(async () =>
			{
				impl = cbapi.Put<Customer>(obj.Local, obj.LocalID);
				obj.AddLocal(impl, impl.SyncID, impl.SyncTime);
				UpdateStatus(obj, operation);
			})) { transaction?.Complete(); };

			Location location = PXSelectJoin<Location,
			InnerJoin<BAccount, On<Location.locationID, Equal<BAccount.defLocationID>>>,
			Where<BAccount.noteID, Equal<Required<PX.Objects.CR.BAccount.noteID>>>>.Select(this, impl.SyncID);

			if (bucket.ConnectorGeneratedAddress != null)
			{
				CustomerLocation addressImpl = cbapi.Put<CustomerLocation>(bucket.ConnectorGeneratedAddress, location.NoteID);
			}
			if (isLocationActive)
			{
				bool lastmodifiedUpdated = false;
				PX.Objects.AR.CustomerMaint graph;
				List<Location> locations;
				graph = PXGraph.CreateInstance<PX.Objects.AR.CustomerMaint>();
				graph.BAccount.Current = PXSelect<PX.Objects.AR.Customer, Where<PX.Objects.AR.Customer.acctCD,
										 Equal<Required<PX.Objects.AR.Customer.acctCD>>>>.Select(graph, impl.CustomerID.Value);

				locations = graph.GetExtension<PX.Objects.AR.CustomerMaint.LocationDetailsExt>().Locations.Select().RowCast<Location>().ToList();
				//create/update other address and create status line(including Main)
				var alladdresses = SelectStatusChildren(obj.SyncID).Where(s => s.EntityType == BCEntitiesAttribute.Address)?.ToList();

				if (bucket.CustomerAddresses.Count > 0)
				{
					if (alladdresses.Count == 0)// we copy into main if its first location
					{
						var main = bucket.CustomerAddresses.FirstOrDefault(x => x.Extern.Default == true);

						if (location != null && main.LocalID == null)
						{
							main.LocalID = location.NoteID; //if location already created
						}
					}
				}

				foreach (var loc in bucket.CustomerAddresses)
				{
					try
					{
						loc.Local.Customer = impl.CustomerID;
						var status = EnsureStatus(loc, SyncDirection.Import, conditions: Conditions.DoNotPersist);
						if (loc.LocalID == null)
						{
							loc.Local.ShipVia = location.CCarrierID.ValueField();
						}
						else if (loc.LocalID != null && !locations.Any(x => x.NoteID == loc.LocalID)) continue; // means deletd location
						if (status == EntityStatus.Pending || Operation.SyncMethod == SyncMode.Force)
						{
							lastmodifiedUpdated = true;
							CustomerLocation addressImpl = cbapi.Put<CustomerLocation>(loc.Local, loc.LocalID);

							loc.AddLocal(addressImpl, addressImpl.SyncID, addressImpl.SyncTime);
							UpdateStatus(loc, operation);
						}
					}
					catch (Exception ex)
					{
						LogError(Operation.LogScope(loc), ex);
						UpdateStatus(loc, BCSyncOperationAttribute.LocalFailed, message: ex.Message);
					}

				}
				DateTime? date = null;
				bool updated = UpdateDefault(bucket, locations, graph);
				if (lastmodifiedUpdated || updated)
				{
					date = GetHelper<SPHelper>().GetUpdatedDate(impl.CustomerID?.Value, date);
				}
				obj.AddLocal(impl, impl.SyncID, date ?? graph.BAccount.Current?.LastModifiedDateTime ?? impl.SyncTime);
				UpdateStatus(obj, operation);

			}
		}

		/// <summary>
		/// Updates the default customer address in ERP.
		/// </summary>
		/// <param name="bucket"></param>
		/// <param name="locations">List to retrieve a <see cref="Location"/></param>
		/// <param name="graph">A <see cref="PX.Objects.AR.CustomerMaint"/> graph.</param>
		/// <returns>A boolean indicating if the default address was updated.</returns>
		protected virtual bool UpdateDefault(SPCustomerEntityBucket bucket, List<Location> locations, PX.Objects.AR.CustomerMaint graph)
		{
			MappedCustomer mappedCustomer = bucket.Customer;
			bool updated = false;
			if (mappedCustomer.LocalID == null) return updated;

			List<MappedLocation> customerLocations = bucket.CustomerAddresses;
			var addressesSyncStatus = SelectStatusChildren(mappedCustomer.SyncID).Where(status => status.EntityType == BCEntitiesAttribute.Address && status.Deleted == false)?.ToList();
			var deletedSyncStatuses = addressesSyncStatus?.Where(x => customerLocations.All(y => x.ExternID != y.ExternID)).ToList();

			int? previousDefault = null;
			Guid? currentDefaultNoteID = locations.FirstOrDefault(location => location.LocationID == graph.BAccount.Current.DefLocationID)?.NoteID;
			if (ShouldUpdateDefaultAddress(customerLocations, deletedSyncStatuses, currentDefaultNoteID))
			{
				var mappedDefault = customerLocations.FirstOrDefault(x => x.Extern.Default == true); // get new default mapped address
				previousDefault = GetHelper<SPHelper>().SetDefaultLocation(mappedDefault?.LocalID, locations, graph, ref updated);
			}

				//mark status for address as deleted for addresses deleted at BC
			if (deletedSyncStatuses != null && deletedSyncStatuses.Count > 0)
				{
					var locGraph = PXGraph.CreateInstance<PX.Objects.AR.CustomerLocationMaint>();

				foreach (BCSyncStatus bcSyncStatus in deletedSyncStatuses)
					{
					DeleteStatus(bcSyncStatus, BCSyncOperationAttribute.ExternDelete, BCMessages.RecordDeletedInExternalSystem);
					GetHelper<SPHelper>().DeactivateLocation(graph, previousDefault, locGraph, bcSyncStatus);
					}

					updated = true;
				}

			return updated;
		}

		/// <summary>
		/// Decides if the provided location should be updated locally.
		/// </summary>
		/// <param name="mappedLocations"></param>
		/// <param name="deletedSyncStatuses">List of deleted sync statuses.</param>
		/// <param name="defaultLocationNoteID">The current default location noted ID.</param>
		/// <returns>A boolean indicating if the value should be updated or not.</returns>
		public virtual bool ShouldUpdateDefaultAddress(List<MappedLocation> mappedLocations, List<BCSyncStatus> deletedSyncStatuses, Guid? defaultLocationNoteID)
		{
			bool isNew = defaultLocationNoteID.HasValue == false;
			bool wasDefaultDeleted = defaultLocationNoteID.HasValue & deletedSyncStatuses.Any(syncStatus => syncStatus.LocalID == defaultLocationNoteID);

			string primarySystem = GetEntity().PrimarySystem;
			string syncDirection = GetEntity().Direction;
			bool hasModifications = mappedLocations.Any(mappedLocation => mappedLocation.Local.Default?.Value != mappedLocation.Extern.Default);
			bool shouldBeUpdated = hasModifications && syncDirection != BCSyncDirectionAttribute.Export && primarySystem == BCSyncSystemAttribute.External;

			return isNew || wasDefaultDeleted || shouldBeUpdated;
		}

		protected Guid? CheckIfExists(CustomerAddressData custAddr, List<PXResult<Location, PX.Objects.CR.Address, PX.Objects.CR.Contact, BAccount>> pXResults, List<MappedLocation> mappedLocations)
		{
			foreach (PXResult<Location, PX.Objects.CR.Address, PX.Objects.CR.Contact, BAccount> loc in pXResults)
			{
				Location location = loc.GetItem<Location>();
				PX.Objects.CR.Address address = loc.GetItem<PX.Objects.CR.Address>();
				PX.Objects.CR.Contact contact = loc.GetItem<PX.Objects.CR.Contact>();
				BAccount account = loc.GetItem<BAccount>();
				if (CompareLocation(custAddr, address, contact, account.AcctName))
				{
					if (!mappedLocations.Any(x => x.LocalID == location.NoteID))
						return location.NoteID;
				}
			}
			return null;
		}
		protected bool CompareLocation(CustomerAddressData custAddr, PX.Objects.CR.Address address, PX.Objects.CR.Contact contact, string accountName)
		{
			return GetHelper<SPHelper>().CompareStrings(custAddr.City, address.City)
											&& GetHelper<SPHelper>().CompareStrings(custAddr.CountryCode, address.CountryID)
											&& (GetHelper<SPHelper>().CompareStrings(custAddr.Phone, contact.Phone1) || GetHelper<SPHelper>().CompareStrings(custAddr.Phone, contact.Phone2))
											&& GetHelper<SPHelper>().CompareStrings(custAddr.ProvinceCode, address.State)
											&& (GetHelper<SPHelper>().CompareStrings(custAddr.Name, contact.Attention) || GetHelper<SPHelper>().CompareStrings(custAddr.Name, accountName))
											&& GetHelper<SPHelper>().CompareStrings(custAddr.Address1, address.AddressLine1)
											&& GetHelper<SPHelper>().CompareStrings(custAddr.Address2, address.AddressLine2)
											&& GetHelper<SPHelper>().CompareStrings(custAddr.PostalCode, address.PostalCode)
											&& GetHelper<SPHelper>().CompareStrings(custAddr.Company, contact.FullName);
		}

		#endregion

		#region Export
		public override async Task FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			IEnumerable<Core.API.Customer> impls = cbapi.GetAll<Core.API.Customer>(
					new Core.API.Customer { CustomerID = new StringReturn(), BAccountID = new IntReturn(), IsGuestCustomer = new BooleanReturn(),
											CustomerCategory = new StringSearch { Value = BCCustomerCategoryAttribute.IndividualValue } }, 
				minDateTime, maxDateTime, filters, cancellationToken: cancellationToken);

			int countNum = 0;
			List<IMappedEntity> mappedList = new List<IMappedEntity>();
			foreach (Customer impl in impls)
			{
				IMappedEntity obj = new MappedCustomer(impl, impl.SyncID, impl.SyncTime);

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

		public override async Task<EntityStatus> GetBucketForExport(SPCustomerEntityBucket bucket, BCSyncStatus bcstatus, CancellationToken cancellationToken = default)
		{
			Customer impl = BCExtensions.GetSharedSlot<Customer>(bcstatus.LocalID.ToString()) ?? cbapi.GetByID(bcstatus.LocalID,
				new Customer()
				{
					ReturnBehavior = ReturnBehavior.OnlySpecified,
					Attributes = new List<AttributeValue>() { new AttributeValue() },
					BillingContact = new Core.API.Contact(),
					CreditVerificationRules = new CreditVerificationRules(),
					MainContact = new Core.API.Contact(),
					PrimaryContact = new Core.API.Contact(),
					ShippingContact = new Core.API.Contact()
				}, GetCustomFieldsForExport());
			if (impl == null) return EntityStatus.None;

			//Ensure that the entity is a customer and not a company (if the user changed it after the sync)
			if (impl == null || !BCCustomerCategoryAttribute.IsIndividual(impl?.CustomerCategory?.Value))
				return EntityStatus.Deleted;

			MappedCustomer obj = bucket.Customer = bucket.Customer.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(bucket.Customer, SyncDirection.Export);

			return status;
		}

		public override async Task MapBucketExport(SPCustomerEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedCustomer customerObj = bucket.Customer;
			MappedLocation addressObj = bucket.CustomerAddress;

			Customer customerImpl = customerObj.Local;
			Core.API.Contact contactImpl = customerImpl.MainContact;
			Core.API.Contact primaryContact = customerImpl.PrimaryContact;
			Core.API.Address addressImpl = contactImpl.Address;
			CustomerData customerData = customerObj.Extern = new CustomerData();

			//Customer
			customerData.Id = customerObj.ExternID?.ToLong();

			//Contact			
			customerData.FirstName = primaryContact?.FirstName?.Value ?? primaryContact?.LastName?.Value ?? customerImpl.CustomerName?.Value.FieldsSplit(0, customerImpl.CustomerName?.Value);
			customerData.LastName = primaryContact?.LastName?.Value ?? primaryContact?.FirstName?.Value ?? customerImpl.CustomerName?.Value.FieldsSplit(1, customerImpl.CustomerName?.Value);
			customerData.Email = contactImpl.Email?.Value;
			customerData.Phone = contactImpl.Phone1?.Value ?? contactImpl.Phone2?.Value;


			//Address
			if (isLocationActive)
			{
				bucket.CustomerAddresses = new List<MappedLocation>();
				var addressReference = SelectStatusChildren(customerObj.SyncID).Where(s => s.EntityType == BCEntitiesAttribute.Address)?.ToList();

				var customerLocations = cbapi.GetAll<CustomerLocation>(new CustomerLocation()
				{
					Customer = new StringSearch() { Value = customerObj.Local.CustomerID.Value },
					Active = new BooleanReturn(),
					LocationName = new StringReturn(),
					ReturnBehavior = ReturnBehavior.OnlySpecified,
					LocationContact = new Core.API.Contact()
					{
						ReturnBehavior = ReturnBehavior.OnlySpecified,
						Phone1 = new StringReturn(),
						Phone2 = new StringReturn(),
						FullName = new StringReturn(),
						Attention = new StringReturn(),
						Address = new Core.API.Address() { ReturnBehavior = ReturnBehavior.All }
					},
				}, cancellationToken:cancellationToken);

				foreach (CustomerLocation customerLocation in customerLocations)
				{
					var mapped = addressReference.FirstOrDefault(x => x.LocalID == customerLocation.NoteID.Value);
					if ((mapped == null && customerLocation.Active.Value == true) || (mapped != null && mapped.Deleted == false))
					{
						if (mapped == null && customerLocation.LocationContact.Address.City?.Value == null && customerLocation.LocationContact.Address.AddressLine1?.Value == null
							 && customerLocation.LocationContact.Address.AddressLine2?.Value == null && customerLocation.LocationContact.Address.PostalCode?.Value == null)
							continue;// connector generated address
						MappedLocation mappedLocation = new MappedLocation(customerLocation, customerLocation.NoteID.Value, customerLocation.LastModifiedDateTime.Value, customerObj.SyncID);
						mappedLocation.Extern = MapLocationExport(mappedLocation, customerObj);
						if (addressReference?.Count == 0 && existing != null)
						{
							mappedLocation.ExternID = CheckIfExists(mappedLocation.Local, existing, bucket.CustomerAddresses, customerImpl.CustomerName?.Value);
						}
						locationProcessor.RemapBucketExport(new SPLocationEntityBucket() { Address = mappedLocation }, null);
						bucket.CustomerAddresses.Add(mappedLocation);
					}

				}
			}
		}
		protected virtual string CheckIfExists(CustomerLocation custAddr, IMappedEntity existing, List<MappedLocation> mappedLocations, string customerName)
		{
			CustomerData data = (CustomerData)existing.Extern;
			foreach (CustomerAddressData address in data.Addresses)
			{
				string fullName = new String[] { custAddr.LocationContact.Attention?.Value, custAddr.LocationName?.Value }.FirstNotEmpty();
				if (CompareLocation(custAddr, address, customerName))
				{
					var id = new object[] { data.Id, address.Id }.KeyCombine();
					if (!mappedLocations.Any(x => x.ExternID == id))
						return id;
				}
			}
			return null;
		}

		protected bool CompareLocation(CustomerLocation custAddr, CustomerAddressData address, string customerName)
		{
			return GetHelper<SPHelper>().CompareStrings(address.City, custAddr.LocationContact?.Address?.City?.Value)
					&& (GetHelper<SPHelper>().CompareStrings(address.Company, custAddr.LocationContact?.CompanyName?.Value) || GetHelper<SPHelper>().CompareStrings(address.Company, custAddr.LocationContact.FullName?.Value))
					&& (GetHelper<SPHelper>().CompareStrings(address.CountryCode, custAddr.LocationContact?.Address?.Country?.Value) || GetHelper<SPHelper>().CompareStrings(address.Country, custAddr.LocationContact?.Address?.Country?.Value))
					&& (GetHelper<SPHelper>().CompareStrings(address.Phone, custAddr.LocationContact?.Phone1?.Value) || GetHelper<SPHelper>().CompareStrings(address.Phone, custAddr.LocationContact?.Phone2?.Value))
					&& GetHelper<SPHelper>().CompareStrings(address.ProvinceCode, custAddr.LocationContact?.Address?.State?.Value)
					&& GetHelper<SPHelper>().CompareStrings(address.Address1, custAddr.LocationContact?.Address?.AddressLine1?.Value)
					&& GetHelper<SPHelper>().CompareStrings(address.Address2, custAddr.LocationContact?.Address?.AddressLine2?.Value)
					&& GetHelper<SPHelper>().CompareStrings(address.PostalCode, custAddr.LocationContact?.Address?.PostalCode?.Value)
					&& GetHelper<SPHelper>().CompareStrings(address.Name, custAddr.LocationContact?.Attention?.Value) || GetHelper<SPHelper>().CompareStrings(address.Name, customerName);
		}

		public override object GetAttribute(SPCustomerEntityBucket bucket, string attributeID)
		{
			MappedCustomer obj = bucket.Customer;
			Customer impl = obj.Local;
			return impl.Attributes?.Where(x => string.Equals(x?.AttributeDescription?.Value, attributeID, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

		}
		public override void AddAttributeValue(SPCustomerEntityBucket bucket, string attributeID, object attributeValue)
		{
			MappedCustomer obj = bucket.Customer;
			Customer impl = obj.Local;
			impl.Attributes = impl.Attributes ?? new List<AttributeValue>();
			AttributeValue attribute = new AttributeValue();
			attribute.AttributeID = new StringValue() { Value = attributeID };
			attribute.Value = new StringValue() { Value = attributeValue?.ToString() };
			attribute.ValueDescription = new StringValue() { Value = attributeValue?.ToString() };
			impl.Attributes.Add(attribute);
		}

		public override async Task SaveBucketExport(SPCustomerEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
			MappedCustomer obj = bucket.Customer;
			MappedLocation addressObj = bucket.CustomerAddress;

			//Customer
			CustomerData customerData = null;
			try
			{
				if (obj.ExternID == null || existing == null)
				{
					customerData = customerDataProvider.Create(obj.Extern);
				}
				else
				{
					customerData = customerDataProvider.Update(obj.Extern);
				}
				obj.AddExtern(customerData, customerData.Id?.ToString(), customerData.Email, customerData.DateModifiedAt.ToDate(false));
				UpdateStatus(obj, operation);

			}
			catch (Exception ex)
			{
				throw new PXException(ex.Message);
			}

			// update ExternalRef
			string externalRef = APIHelper.ReferenceMake(customerData.Id?.ToString(), GetBinding().BindingName);

			string[] keys = obj.Local?.AccountRef?.Value?.Split(';');
			if (keys?.Contains(externalRef) != true)
			{
				if (!string.IsNullOrEmpty(obj.Local?.AccountRef?.Value))
					externalRef = new object[] { obj.Local?.AccountRef?.Value, externalRef }.KeyCombine();

				if (externalRef.Length < 50)
					PXDatabase.Update<BAccount>(
									  new PXDataFieldAssign(typeof(BAccount.acctReferenceNbr).Name, PXDbType.NVarChar, externalRef),
									  new PXDataFieldRestrict(typeof(BAccount.noteID).Name, PXDbType.UniqueIdentifier, obj.Local.NoteID?.Value)
									  );
			}

			//Address
			if (isLocationActive)
			{
				bool locationUpdated = false;

				foreach (var address in bucket.CustomerAddresses)
				{
					try
					{
						if (address.Local?.Active?.Value == false)
						{
							BCSyncStatus bCSyncStatus = PXSelect<BCSyncStatus,
									Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
									And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
									And<BCSyncStatus.entityType, Equal<Required<BCSyncStatus.entityType>>,
									And<BCSyncStatus.localID, Equal<Required<BCSyncStatus.localID>>>>>>>.Select(this, BCEntitiesAttribute.Address, address.LocalID);
							if (bCSyncStatus == null) continue; //New inactive location should not be synced
						}
						var status = EnsureStatus(address, SyncDirection.Export, conditions: Conditions.DoNotPersist);

						if (status == EntityStatus.Pending || Operation.SyncMethod == SyncMode.Force)
						{
							string locOperation;
							locationUpdated = true;
							CustomerAddressData addressData = null;
							if (address.ExternID == null || existing == null)
							{
								addressData = customerAddressDataProvider.Create(address.Extern, customerData.Id.ToString());
								locOperation = BCSyncOperationAttribute.ExternInsert;
							}
							else
							{
								addressData = customerAddressDataProvider.Update(address.Extern, address.ExternID.KeySplit(0), address.ExternID.KeySplit(1));
								locOperation = BCSyncOperationAttribute.ExternUpdate;
							}

							address.AddExtern(addressData, new object[] { customerData.Id, addressData.Id }.KeyCombine(), customerData.Email, addressData.CalculateHash());

							UpdateStatus(address, locOperation);
						}
					}
					catch (Exception ex)
					{
						LogError(Operation.LogScope(address), ex);
						UpdateStatus(address, BCSyncOperationAttribute.ExternFailed, message: ex.Message);
					}
					if (locationUpdated)
					{
						customerData = customerDataProvider.GetByID(customerData.Id.ToString());
						obj.AddExtern(customerData, customerData.Id?.ToString(), customerData.Email, customerData.DateModifiedAt.ToDate(false));
						UpdateStatus(obj, operation);
					}

					var addressReferences = SelectStatusChildren(obj.SyncID).Where(s => s.EntityType == BCEntitiesAttribute.Address && s.Deleted == false)?.ToList();
					var deletedValues = addressReferences?.Where(x => bucket.CustomerAddresses.All(y => x.LocalID != y.LocalID)).ToList();

					if (deletedValues != null && deletedValues.Count > 0)
					{
						foreach (var value in deletedValues)
						{
							DeleteStatus(value, BCSyncOperationAttribute.LocalDelete, BCMessages.RecordDeletedInERP);
						}
					}
				}
			}
		}
		#endregion
	}
}
