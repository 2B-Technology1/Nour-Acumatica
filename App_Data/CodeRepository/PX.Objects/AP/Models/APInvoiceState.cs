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
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AP
{
    /// <summary>
    /// This class provide information about state <see cref="APInvoice"></c>
    /// </summary>
    public class APInvoiceState
    {
        public bool DontApprove;
        public bool HasPOLink;
        public bool IsDocumentPrepayment;
        public bool IsDocumentInvoice;
        public bool IsDocumentDebitAdjustment;
        public bool IsDocumentCreditAdjustment;
        public bool IsDocumentOnHold;
        public bool IsDocumentScheduled;
        public bool IsDocumentPrebookedNotCompleted;
        public bool IsDocumentReleasedOrPrebooked;
        public bool IsDocumentVoided;
        public bool IsDocumentRejected;
        public bool RetainageApply;
        public bool IsRetainageDocument;
        public bool IsRetainageDebAdj;
		public bool IsRetainageReversing;
		public bool IsRetainageApplyDocument;
		public bool IsDocumentRejectedOrPendingApproval;
        public bool IsDocumentApprovedBalanced;
		public bool PaymentsByLinesAllowed;
		public bool LandedCostEnabled;
        public bool IsFromExpenseClaims;
        public bool AllowAddPOByProject;
		public bool IsCuryEnabled;
		public bool IsFromPO;
		public bool IsAssignmentEnabled;
		public bool IsPrepaymentRequestFromPO => IsDocumentPrepayment && IsFromPO;
        public bool IsDocumentEditable => !IsDocumentReleasedOrPrebooked && !IsDocumentRejectedOrPendingApproval && !IsDocumentApprovedBalanced;
    }
}
