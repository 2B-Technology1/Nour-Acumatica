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

using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.AR
{
	/// <exclude/>
	public class ARInvoiceState
	{
		public bool PaymentsByLinesAllowed;
		public bool RetainageApply;
		public bool IsRetainageDocument;
		public bool IsDocumentReleased;
		public bool IsDocumentInvoice;
		public bool IsDocumentPrepaymentInvoice;
		public bool IsDocumentCreditMemo;
		public bool IsPrepaymentInvoiceReversing;
		public bool IsDocumentDebitMemo;
		public bool IsRetainageReversing;
		public bool RetainTaxes;
		public bool IsDocumentOnHold;
		public bool IsDocumentOnCreditHold;
		public bool IsDocumentScheduled;
		public bool IsDocumentVoided;
		public bool IsDocumentRejected;
		public bool InvoiceUnreleased;
		public bool IsRetainageApplyDocument;
		public bool IsDocumentRejectedOrPendingApproval;
		public bool IsDocumentApprovedBalanced;
		public bool IsUnreleasedWO;
		public bool IsUnreleasedPPD;
		public bool IsMigratedDocument;
		public bool IsUnreleasedMigratedDocument;
		public bool IsReleasedMigratedDocument;
		public bool IsMigrationMode;
		public bool IsCancellationDocument;
		public bool IsCorrectionDocument;
		public bool IsRegularBalancedDocument;

		public bool CuryEnabled;
		public bool ShouldDisableHeader;
		public bool AllowDeleteDocument;
		public bool DocumentHoldEnabled;
		public bool DocumentDateEnabled;
		public bool DocumentDescrEnabled;
		public bool EditCustomerEnabled;
		public bool AddressValidationEnabled;
		public bool IsTaxZoneIDEnabled;
		public bool IsAvalaraCustomerUsageTypeEnabled;
		public bool ApplyFinChargeVisible;
		public bool ApplyFinChargeEnable;
		public bool ShowCashDiscountInfo;
		public bool IsAssignmentEnabled;
		public bool BalanceBaseCalc;
		public bool AllowDeleteTransactions;
		public bool AllowInsertTransactions;
		public bool AllowUpdateTransactions;
		public bool AllowDeleteTaxes;
		public bool AllowInsertTaxes;
		public bool AllowUpdateTaxes;
		public bool AllowDeleteDiscounts;
		public bool AllowInsertDiscounts;
		public bool AllowUpdateDiscounts;
		public bool LoadDocumentsEnabled;
		public bool AutoApplyEnabled;
		public bool AllowUpdateAdjustments;
		public bool AllowDeleteAdjustments;
		public bool AllowUpdateCMAdjustments;

		public IList<string> ExplicitlyEnabledTranFields = new List<string>(); 
	}
}
