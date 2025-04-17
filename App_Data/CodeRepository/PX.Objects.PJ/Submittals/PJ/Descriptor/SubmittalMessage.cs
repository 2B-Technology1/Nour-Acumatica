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

namespace PX.Objects.PJ.Submittals.PJ.Descriptor
{
	[PXLocalizable]
	public static class SubmittalMessage
	{
		public const string WorkflowPromptFormTitle = "Details";
		public const string DateClosed = "Date Closed";
		public const string CloseAction = "Close";
		public const string OpenAction = "Open";
		public const string SubmittleReportNamePattern = "Submittal {0}-{1} ({2}).pdf";
		public const string OldRevisionWarning = "Viewing an old revision of this submittal";
		public const string SubmittleSearchTitle = "Submittal: {0} Revision: {1}";
		public const string BallInCourt = "Ball in Court";


	}

	[PXLocalizable]
	public static class SubmittalReason
	{
		public const string New = "New";
		public const string Revision = "Revision";
		public const string Issued = "Issued";
		public const string Submitted = "Submitted";
		public const string PendingApproval = "Pending Approval";
		public const string Approved = "Approved";
		public const string ApprovedAsNoted = "Approved as Noted";
		public const string Rejected = "Rejected";
		public const string Canceled = "Canceled";
		public const string ReviseAndResubmit = "Revise and Resubmit";
	}
}
