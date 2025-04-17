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
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public class ExternalTransactionState
	{
		public IExternalTransaction ExternalTransaction { get; private set; }
		public ProcessingStatus ProcessingStatus { get; set; }
		public bool IsActive { get; private set; }
		public bool IsCompleted { get; private set; }
		public bool NeedSync { get; private set; }
		public bool CreateProfile { get; private set; }
		public bool IsVoided { get; private set; }
		public bool IsCaptured { get; set; }
		public bool IsPreAuthorized { get; set; }
		public bool IsRefunded { get; set; }
		public bool IsOpenForReview { get; set; }
		public bool IsDeclined { get; set; }
		public bool IsExpired { get; set; }
		public string Description { get; set; }
		public bool HasErrors { get; set; }
		public bool SyncFailed { get; set; }
		/// <summary>
		/// Flag that indicates that credit card transaction was submitted for settlement. 
		/// </summary>
		public bool IsSettlementDue { get { return IsCaptured || IsRefunded; } }
		public bool IsImportedUnknown { get { return ProcessingStatus == ProcessingStatus.Unknown 
			&& ExternalTransaction != null && (IsActive || NeedSync); } }

		public ExternalTransactionState(IExternalTransaction extTran)
		{
			ExternalTransaction = extTran;
			SetProps(extTran);
		}

		public ExternalTransactionState()
		{

		}

		private void SetProps(IExternalTransaction extTran)
		{
			ProcessingStatus = ExtTransactionProcStatusCode.GetProcessingStatusByProcStatusStr(extTran.ProcStatus);
			IsActive = extTran.Active.GetValueOrDefault();
			IsCompleted = extTran.Completed.GetValueOrDefault();
			NeedSync = extTran.NeedSync.GetValueOrDefault();
			CreateProfile = extTran.SaveProfile.GetValueOrDefault();
			SyncFailed = extTran.SyncStatus == CCSyncStatusCode.Error;
			IsVoided = ProcessingStatus == ProcessingStatus.VoidSuccess
				|| ProcessingStatus == ProcessingStatus.VoidHeldForReview;
			IsCaptured = ProcessingStatus == ProcessingStatus.CaptureSuccess 
				|| ProcessingStatus == ProcessingStatus.CaptureHeldForReview;
			IsPreAuthorized = ProcessingStatus == ProcessingStatus.AuthorizeSuccess 
				|| ProcessingStatus == ProcessingStatus.AuthorizeHeldForReview;
			IsRefunded = ProcessingStatus == ProcessingStatus.CreditSuccess
				|| ProcessingStatus == ProcessingStatus.CreditHeldForReview;
			IsOpenForReview = ProcessingStatus == ProcessingStatus.AuthorizeHeldForReview
				|| ProcessingStatus == ProcessingStatus.CaptureHeldForReview
				|| ProcessingStatus == ProcessingStatus.AuthorizeHeldForReview
				|| ProcessingStatus == ProcessingStatus.VoidHeldForReview
				|| ProcessingStatus == ProcessingStatus.CreditHeldForReview;
			IsDeclined = ProcessingStatus == ProcessingStatus.AuthorizeDecline
				|| ProcessingStatus == ProcessingStatus.CaptureDecline
				|| ProcessingStatus == ProcessingStatus.VoidDecline
				|| ProcessingStatus == ProcessingStatus.CreditDecline;
			IsExpired = ProcessingStatus == ProcessingStatus.AuthorizeExpired
				|| ProcessingStatus == ProcessingStatus.CaptureExpired;
			HasErrors = ProcessingStatus == ProcessingStatus.AuthorizeFail
				|| ProcessingStatus == ProcessingStatus.CaptureFail
				|| ProcessingStatus == ProcessingStatus.VoidFail
				|| ProcessingStatus == ProcessingStatus.CreditFail;
		}
	}
}
