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

namespace PX.Objects.Localizations.CA.Messages
{
    [PXLocalizable]
    public static class T5018Messages
    {
        public const string EightCharMax = "The value should not exceed 8 characters.";
        public const string SNFormat = "The SIN must be entered in the format of #########.";
        public const string BNFormat = "The Program Account Number format should be 123456789RT1234.";

		public const string
			T5018IndividualEmptyPrimary = "The primary contact must be specified if the vendor files the T5018 form as an individual.";
    
		public const string TaxYear = "Once you choose a reporting period, subsequent returns must be filed for the same reporting year unless otherwise authorized in writting by the CRA.";
		public const string TaxYearImmutable = "You cannot change T5018 Year Type as you already have prepared data for this company in the system. If data was prepared by mistake, you should delete existing data first. If you need to keep the data and still change the T5018 Year Type, please contact your Acumatica support provider";
		public const string NoNewRows = "A new revision cannot be created because there are no new transactions for any of the vendors. The latest revision will be displayed.";
		public const string NoPreviousSubmissions = "Before a report can be amended, the previous revision of the report must be submitted to the CRA. After submitting the report, select the E-File Submitted to CRA check box, and then prepare the amended report.";
		internal const string NotLatestRevision = "Only the last revision for T5018 can be deleted.";

		public const string PrepareButton = "Prepare Data";
		public const string ReportButton = "VIEW VALIDATION REPORT";
		public const string GenerateButton = "CREATE E-FILE";

		public const string CalendarYear = "Calendar Year";
		public const string FiscalYear = "Fiscal Year";
		public const string Corporation = "Corporation";
		public const string Partnership = "Partnership";
		public const string Individual = "Individual";

		public const string Transmitter = "Transmitter";
		public const string T5018Year = "T5018 Tax Year";
		public const string Revision = "Revision";
		public const string T5018MasterTable = "T5018 Master Table";

		public const string NewValue = PX.Objects.AP.Messages.NewKey;

		public const string Original = "Original";
		public const string Amendment = "Amendment";
		internal const string English = "English";
		internal const string French = "French";
		
		public const string OrigDocAmtLessThanTaxTotal = "The original document amount is less than the document tax total. (AP301000) APInvoice type: {0}, reference number: {1}.";
    }
}
