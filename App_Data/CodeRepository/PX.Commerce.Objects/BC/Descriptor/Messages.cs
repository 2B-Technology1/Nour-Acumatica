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

using PX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PX.Objects.CT.ContractAction;
using static PX.Objects.RUTROT.RUTROTDistribution;

namespace PX.Commerce.Objects
{
	[PXLocalizable()]
	public static class BCObjectsMessages
	{

		public const string RefundedItemNotFound = "The refund cannot be applied because the {0} item is not found in the original sales order.";
		public const string DuplicateCustomerEmail = "The {0} customer has a duplicate email. Please provide a unique email address for correct synchronization with BigCommerce.";

		public const string NoTranIDForCC = "The externally authenticated transaction could not be registered. The transaction ID is not provided.";
		public const string NoProfileForCC = "The externally authenticated transaction could not be registered. The Create Payment Profile flag is not provided.";
		public const string DuplicateLocationRows = "Duplicate locations or a combination of a specified location and an empty location cannot be assigned to the same warehouse.";
		public const string CannotDeleteSOline = "This sales order line could not be deleted because it is associated with line {0}. Delete the associated item and try again.";
		public const string OrderMissingShipVia = "The order is missing the ship via code. Please map it with the shipping option in the store settings or specify it on the Customer Locations (AR303020) form.";
		public const string FailureToMatchSavedTaxes = "Taxes could not be imported from the external provider. The order was sent with taxes, but the system could not parse the provided tax field ({0}).";
		public const string TaxNameDoesntExist = "The received tax code is invalid or mapped to an empty string. Please check the tax code mapping on the Substitution Lists (SM206026) form.";
		public const string ExternalTaxProviderTaxFor = "External Tax Provider {0}";
		public const string WrongModeDefaultTaxZone = "This default tax zone cannot be set for with current tax synchronization settings.";
		public const string CannotFindSaveTaxIDs = "The following tax IDs received from the external system are too long and cannot be saved: {0}. Map these tax IDs to tax IDs of up to {1} characters in the {2} substitution list on the Substitution Lists (SM206026) form.";
		public const string CannotFindTaxZoneVendor = "The order could not be created because the tax agency is not configured for the {0} tax zone. Please specify the tax agency for the tax zone on the Tax Zones (TX206000) form.";
		public const string CannotFindTaxZone = "The order could not be created because the tax zone is not found for the following tax IDs: {0}. Please review the tax configuration.";
		public const string CannotFindMatchingTaxAcu = "The following tax IDs from Acumatica ERP could not be matched: {0}. Make sure the taxes are configured for the {1} tax zone and tax categories, and that they have the same IDs in Acumatica ERP and the external system or are mapped in the substitution list on the Substitution Lists (SM206026) form.";
		public const string CannotFindMatchingTaxExt = "The following tax identifiers from the external system could not be matched: {0}. The ERP has failed to save the tax ID or the tax amount differs from the one that has been expected. Make sure that taxes are configured for the tax zone and tax categories. Also, make sure that taxes either have the same identifiers in the ERP and external system or are mapped on the Substitution Lists (SM206026) form.";
		public const string IntegratedCCProcessing = "The {0} processing center can be used only if integrated card processing is enabled on the Accounts Receivable Preferences (AR101000) form. Enable integrated card processing or remove the processing center.";
		public const string IntegratedCCProcessingSync = "The synchronization could not be completed because integrated card processing is disabled. Enable integrated card processing on the Accounts Receivable Preferences (AR101000) form or remove the {0} processing center from the mapping of the {1} payment method in the store settings.";
		public const string MissingProcessingCenterCreditCard = "This ERP payment method is based on credit cards. Link it with a processing center on the Processing Centers tab of the Payment Methods (CA204000) form and then specify the processing center in the mapping.";
		public const string MissingProcessingCenterCreditCardExists = "This ERP payment method is based on credit cards. A linked processing center must be specified for it in the mapping.";
		public const string RemoveProcessingCenterNotCreditCard = "This ERP payment method is not based on credit cards but has a processing center selected in the mapping. Configure the ERP payment method as a card-based payment method on the Payment Methods (CA204000) form or remove the processing center from the mapping.";
		public const string MissingIntegratedCCProcessing = "The Integrated Processing check box is not selected for this ERP payment method on the Settings for Use in AR tab of the Payment Methods (CA204000) form.";
		public const string CannotConvertInventoryGiftCertificate = "Cannot convert the {0} item because it is used as a gift certificate item in the following stores: {1}.";
		public const string CannotConvertInventoryGiftWrapping = "Cannot convert the {0} item because it is used as a gift wrapping item in the following stores: {1}.";
		public const string CannotConvertInventoryRefundItem = "Cannot convert the {0} item because it is used as a refund amount item in the following stores: {1}.";
		public const string DuplicateRoleAssignmentForLocation = "Your changes cannot be saved because the location cannot have more than one role assigned to the same contact.";
		public const string DuplicateRoleAssignmentForContact = "Your changes cannot be saved because the contact cannot have more than one role assigned for the same location.";

		public const string InventoryItemDescriptionRequired = "The item description is required to export the item to external systems. Specify the description, or clear the Export to External System check box on the eCommerce tab.";
		public const string LineDiscount = "Line Discounts";
		public const string DocumentDiscount = "Document Discounts";
		public const string HighRisk = "High Risk";
		public const string MediumOrHighRisk = "Medium or High Risk";
		public const string StoreCreditCaption = "Store Credit";
		public const string GiftCertificateCaption = "Gift Certificate";
		public const string RiskHold = "Risk Hold";
		public const string RemoveRiskHold = "Remove Risk Hold";
		public const string DoNotExport = "Do Not Export";
		public const string Export = "Export Without Empty Boxes";

		public const string AttributeHasNoDefinedValues = "No values have been defined for the {0} attribute.";
		public const string AttributeValueCannotBeEmpty = "The value of the {0} attribute must be specified.";
		public const string InvItemHasSameAttrValues = "The {0} matrix item has the same attribute values as the {1} matrix item.";
		public const string AskAllowOverwritingFieldsContent = "The values of the following fields in the {0} matrix item will be overwritten to match the values in the {1} template item: {2}. Do you want to continue?";
		public const string AskAllowOverwritingFieldsHeader = "Overwrite Matrix Item Values";

		public const string FeeTypeAlreadyExistsInPaymentMethod = "The mapping for the {0} fee already exists for the {1} store payment method.";

		//PII Processing
		public const string EraseDataWarning = "The system will permanently delete all personally identifiable information from the processed sales orders, and you will not be able to create invoices for them anymore.";
	}
}
