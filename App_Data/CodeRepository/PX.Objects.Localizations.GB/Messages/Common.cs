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

namespace PX.Objects.Localizations.GB.Messages
{
	[PX.Common.PXLocalizable]
	public static class Common
	{
		public const string CannotBeEmpty = "The value cannot be empty.";
		public const string CannotBeFound = "The value {0} cannot be found in the system.";
	}

	[PX.Common.PXLocalizable]
	public class BACSLloyds
	{
		public const string ProviderName = "BACS Lloyds Export Provider";
		public const string FileCreationNumberInvalid = "The Batch Seq. Number '{0}' is invalid. The valid values are between 1 and 9999.";
		public const string CurrencyCodeInvalid = "The currency code {0} is invalid. The valid values are CAD or USD.";
		public const string OriginatorIDEmpty = "The Originator's ID cannot be empty.";
		public const string OriginatorIDTooLong = "The Originator's ID cannot exceed 10 characters.";
		public const string InstitutionIdentNbrEmpty = "The Institutional ID Number cannot be empty.";
		public const string InstitutionIdentNbrInvalid = "The Institutional ID Number {0} is invalid. All characters must be numeric.";
		public const string InstitutionIdentNbrTooLong = "The Institutional ID Number {0} is invalid. The value cannot exceed 3 characters.";
		public const string BatchCannotBeReleased = "This batch has errors and cannot be released.";
		public const string BatchSeqNumberAcceptedValues = "The valid values are between 1 and 9999.";
		public const string DifferentBatchPaymentDates = "All payments within the batch must have the same value in {0}.";
	}
}
