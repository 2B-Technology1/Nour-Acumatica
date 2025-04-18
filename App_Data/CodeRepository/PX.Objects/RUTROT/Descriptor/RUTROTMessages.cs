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
using PX.Common;
using PX.Objects.AR;


namespace PX.Objects.RUTROT
{
	[PXLocalizable]
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public static class RUTROTMessages
	{
		public const string RUTType = "RUT";
		public const string ROTType = "ROT";

		public const string Service = "Service";
		public const string MaterialCost = "Material Cost";
		public const string OtherCost = "Other Cost";

		public const string Construction = "Construction";
		public const string ElectricityWork = "Electricity Work";
		public const string GlassWork = "Glass Work";
		public const string DrainWork = "Drain Work";
		public const string Bricklaying = "Bricklaying";
		public const string Painting = "Painting";
		public const string Plumbing = "Plumbing";

		public const string Cleaning = "Cleaning";
		public const string ClothMaintenance = "Cloth Maintenance";
		public const string Cooking = "Cooking";
		public const string SnowJob = "SnowJob";
		public const string Gardening = "Gardening";
		public const string BabySitting = "Babysitting";
		public const string OtherCare = "Other Care";
		public const string HomeStudies = "Home Studies";

        public const string Release = "Release AR Documents";
        public const string Claim = "Claim ROT & RUT";

        public const string DeductibleExceedsAllowance = "Deductible amount exceeds allowance amount";
		public const string PeopleAreRequiredForDeduction = "At least one person must be specified for ROT or RUT deductible document";
		public const string PositiveUndistributedAmount = "The deductible amount is not distributed among the household members entirely.";
		public const string NegativeUndistributedAmount = "Distributed amount exceeds deductible amount";
		public const string UndistributedAmount = "Deduction is not distributed properly";

		public const string NonpositivePersonAmount = "Non-positive amounts are not allowed in ROT & RUT distribution";
		public const string PersonExceedsAllowance = "Amount exceeds personal allowance";

		public const string LineDoesNotMatchDoc = "Selected item's RUT or RUT does not match document's RUT or ROT";
		public const string FailedToExport = "Failed to export file";

		public const string ClaimNextRefDecreased = "The field value was decreased. This may lead to duplicated reference numbers for exported documents";

		public const string AllowanceLimit = "Standard Allowance Limit";
		public const string AllowanceLimitExtra = "Extra Allowance Limit";
		public const string DeductionPercent = "Deduction,%";

		public const string CannotDeleteWorkType = "This Work Type cannot be deleted as it is in use.";
		public const string CannotClaimWorkType = "This document cannot be claimed because it contains overdue or not active yet work type.";

	    public const string MemoDescription = "This document was created to balance the invoice {0}.";

		public const string TaxAgencyAccountWasNotFoundForThisBranch = "Tax Agency Account was not found for the current branch.";

		#region Export Validation
		public const string NoDocumentsSelected = "No documents selected for export.";
		public const string AtLeastOneBuyerMustBeMentioned = "At least one buyer must be mentioned in claim.";
		public const string NoMoreThan100Buyers = "Claim must include no more than 100 buyers.";

		public const string DateShouldBeSpecifiedOnDocument = "Date should be specified for each claimed invoice.";
		public const string DocumentDateIsBelowAllowed = "Claim is allowed for documents not earlier than {0:d}.";
		public const string SomeDocumentDatesIncorrect = "Some documents have incorrect dates.";
		public const string AllDocumentsMustBeSameYear = "All documents in the claim must be from the same year.";
		public const string PaymentDatesMustNotExceedClaimDate = "Dates of payments must not exceed claim date.";

		public const string CompanyIDMustBeSameForDocuments = "Company ID must be the same for all documents in claim.";
		public const string CompanyIDMustNotBeSameAsBuyerID = "Claimer ID must not be the same as any of the Buyer IDs in claim.";
		public const string SomeCompanyBuyerIDsAreIncorrect = "Company ID and/or Buyer ID are specified incorrectly for some documents.";

		public const string ClaimedPaidMustNotExceedTotal = "The sum of claimed and paid amounts must not exceed invoice amount.";
		public const string SomeAmountsIncorrect = "Amounts are incorrect for some documents.";
		[Obsolete(Common.Messages.PropertyIsObsoleteRemoveInAcumatica8)]
		public const string ClaimedTotalTooMuch = "Claim amount must not exceed SEK 50000 per company.";
		public const string ObsoleteWorkType = "The selected type of work is not eligible.";
		public const string ObsoleteWorkTypeWarning = "Please note that the selected type of work is not eligible.";

		public const string ServiceMustBeHour = "Make sure that unit of measure for Service item type is set to hour.";
		#endregion

		
		public const string FieldClass = "RUTROT";
	}
}
