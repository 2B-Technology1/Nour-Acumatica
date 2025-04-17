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
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PX.Commerce.Core;
using static PX.Data.BQL.BqlPlaceholder;

namespace PX.Commerce.Shopify.API.GraphQL
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CustomersResponseData : EntitiesResponse<CustomerDataGQL, CustomerNode>, INodeListResponse<CustomerDataGQL>, IEdgesResponse<CustomerDataGQL, CustomerNode>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CustomerNode : EntityNodeResponse<CustomerDataGQL>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CustomerResponse : IEntityResponse<CustomerDataGQL>
	{
		[JsonProperty("customer")]
		public CustomerDataGQL TEntityData { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CustomersResponse : IEntitiesResponses<CustomerDataGQL, CustomersResponseData>
	{
		[JsonProperty("customers")]
		public CustomersResponseData TEntitiesData { get; set; }
	}

	/// <summary>
	/// Represents information about a customer of the shop, such as the customer's contact details,
	/// their order history, and whether they've agreed to receive marketing material by email.
	/// </summary>
	[JsonObject(Description = "Customer")]
	[CommerceDescription(ShopifyCaptions.Customer, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[GraphQLObject(NodeName = "customer", ConnectionName = "customers")]
	public class CustomerDataGQL : BCAPIEntity, INode
	{
		public class Arguments
		{
			[GraphQLArgument("sortKey", "CustomerSortKeys")]
			public abstract class SortKey { }
		}

		/// <summary>
		/// A list of addresses associated with the customer.
		/// </summary>
		[JsonProperty("addresses")]
		[GraphQLField("addresses", GraphQLConstants.DataType.Object, typeof(MailingAddress))]
		[CommerceDescription(ShopifyCaptions.CustomerAddress, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public List<MailingAddress> Addresses { get; set; }

		/// <summary>
		/// The total amount that the customer has spent on orders in their lifetime.
		/// </summary>
		[JsonProperty("amountSpent")]
		[GraphQLField("amountSpent", GraphQLConstants.DataType.Object, typeof(Money))]
		public Money AmountSpent { get; set; }


		/// <summary>
		/// Whether the merchant can delete the customer from their store.
		/// A customer can be deleted from a store only if they have not yet made an order.After a customer makes an order, they can't be deleted from a store.
		/// </summary>
		[JsonProperty("canDelete")]
		[GraphQLField("canDelete", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool CanDelete { get; set; }

		/// <summary>
		/// A list of the customer's company contact profiles.
		/// </summary>
		[JsonProperty("companyContactProfiles")]
		[GraphQLField("companyContactProfiles", GraphQLConstants.DataType.Object, typeof(CompanyContactDataGQL))]
		public List<CompanyContactDataGQL> CompanyContactProfiles { get; set; }

		/// <summary>
		/// The date and time when the customer was added to the store.
		/// </summary>
		[JsonProperty("createdAt")]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		[CommerceDescription(ShopifyCaptions.DateCreated, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// The default address associated with the customer.
		/// </summary>
		[JsonProperty("defaultAddress")]
		[GraphQLField("defaultAddress", GraphQLConstants.DataType.Object, typeof(MailingAddress))]
		public MailingAddress DefaultAddress { get; set; }

		/// <summary>
		/// The full name of the customer, based on the values for first_name and last_name.
		/// If the first_name and last_name are not available, then this falls back to the customer's email address, and if that is not available, the customer's phone number.
		/// </summary>
		[JsonProperty("displayName")]
		[GraphQLField("displayName", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string DisplayName { get; set; }

		/// <summary>
		/// The customer's email address.
		/// </summary>
		[JsonProperty("email")]
		[GraphQLField("email", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.EmailAddress, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Email { get; set; }

		/// <summary>
		/// The customer's first name.
		/// </summary>
		[JsonProperty("firstName")]
		[GraphQLField("firstName", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.FirstName, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string FirstName { get; set; }

		///<inheritdoc/>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The customer's last name.
		/// </summary>
		[JsonProperty("lastName")]
		[GraphQLField("lastName", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.LastName, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string LastName { get; set; }

		/// <summary>
		/// The ID of the corresponding resource in the REST Admin API.
		/// </summary>
		[JsonProperty("legacyResourceId")]
		[GraphQLField("legacyResourceId", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.UnsignedInt64)]
		[CommerceDescription(ShopifyCaptions.CustomerId, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public long? LegacyResourceId { get; set; }

		/// <summary>
		/// The customer's locale.
		/// </summary>
		[JsonProperty("locale")]
		[GraphQLField("locale", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Locale { get; set; }

		/// <summary>
		/// Attaches additional metadata to a shop's resources:
		///key(required) : An identifier for the metafield(maximum of 30 characters).
		///namespace(required): A container for a set of metadata(maximum of 20 characters). Namespaces help distinguish between metadata that you created and metadata created by another individual with a similar namespace.
		///value (required): Information to be stored as metadata.
		///value_type(required): The value type.Valid values: string and integer.
		///description(optional): Additional information about the metafield.
		/// </summary>
		[JsonProperty("metafields", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Metafields, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[BCExternCustomField(BCConstants.ShopifyMetaFields)]
		[GraphQLField("metafields", GraphQLConstants.DataType.Connection, typeof(MetafieldGQL))]
		public Connection<MetafieldGQL> MetafieldNodes { get; set; }

		[JsonIgnore]
		public List<MetafieldGQL> MetafieldList { get => MetafieldNodes.Nodes?.ToList(); set => MetafieldNodes = new Connection<MetafieldGQL> { Nodes = value }; }

		/// <summary>
		/// A note about the customer.
		/// </summary>
		[JsonProperty("note")]
		[GraphQLField("note", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Note, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Note { get; set; }

		/// <summary>
		/// The customer's phone number.
		/// </summary>
		[JsonProperty("phone")]
		[GraphQLField("phone", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Phone, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Phone { get; set; }

		/// <summary>
		/// The state of the customer's account with the shop.
		/// </summary>
		[JsonProperty("state")]
		[GraphQLField("state", GraphQLConstants.DataType.Enum, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.State, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public CustomerState? State { get; set; }

		/// <summary>
		/// A comma separated list of tags that have been added to the customer.
		/// </summary>
		[JsonProperty("tags")]
		[GraphQLField("tags", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Tags, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public List<string> TagsList { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.Tags, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public string Tags { get => string.Join(";", TagsList); }

		/// <summary>
		/// Whether the customer is exempt from being charged taxes on their orders.
		/// </summary>
		[JsonProperty("taxExempt")]
		[GraphQLField("taxExempt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		[CommerceDescription(ShopifyCaptions.TaxExempt, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public bool? TaxExempt { get; set; }

		/// <summary>
		/// The list of tax exemptions applied to the customer.
		/// </summary>
		//public ICollection<TaxExemption> TaxExemptions { get; set; }

		/// <summary>
		/// The URL to unsubscribe the customer from the mailing list.
		/// </summary>
		[JsonProperty("unsubscribeUrl")]
		[GraphQLField("unsubscribeUrl", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.URL)]
		public string UnsubscribeUrl { get; set; }

		/// <summary>
		/// The date and time when the customer was last updated.
		/// </summary>
		[JsonProperty("updatedAt")]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		[CommerceDescription(ShopifyCaptions.DateModified, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public DateTime? DateModifiedAt { get; set; }

		/// <summary>
		/// Whether the email address is formatted correctly.
		/// Returns true when the email is formatted correctly and belongs to an existing domain.
		/// This doesn't guarantee that the email address actually exists.
		/// </summary>
		[JsonProperty("validEmailAddress")]
		[GraphQLField("validEmailAddress", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? ValidEmailAddress { get; set; }

		/// <summary>
		/// Whether the customer has verified their email address. Defaults to true if
		/// the customer is created through the Shopify admin or API.
		/// </summary>
		[JsonProperty("verifiedEmail")]
		[GraphQLField("verifiedEmail", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? VerifiedEmail { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }
	}
}
