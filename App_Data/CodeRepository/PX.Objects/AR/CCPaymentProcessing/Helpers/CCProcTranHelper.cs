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
using PX.Common;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.Common;

namespace PX.Objects.AR.CCPaymentProcessing.Helpers
{
	public static class CCProcTranHelper
	{
		public class CCProcTranOrderComparer : IComparer<CCProcTran>,IComparer<ICCPaymentTransaction>
		{
			private bool _descending;

			public CCProcTranOrderComparer(bool aDescending)
			{
				this._descending = aDescending;
			}
			public CCProcTranOrderComparer()
			{
				this._descending = false;
			}

			#region IComparer<CCProcTran> Members

			public virtual int Compare(CCProcTran x, CCProcTran y)
			{
				int order = x.TranNbr.Value.CompareTo(y.TranNbr.Value);
				return (this._descending ? -order : order);
			}

			public virtual int Compare(ICCPaymentTransaction pt1, ICCPaymentTransaction pt2)
			{
				int order = pt1.TranNbr.Value.CompareTo(pt2.TranNbr.Value);
				return this._descending ? -order : order;
			}
			#endregion
		}

		public static IEnumerable<ICCPaymentTransaction> FilterDeclinedTrans(IEnumerable<ICCPaymentTransaction> ccProcTrans)
		{
			bool declineAfterReviewFound = false;
			string declinedPCTranNbr = null;
			foreach (ICCPaymentTransaction tran in ccProcTrans)
			{
				if (!declineAfterReviewFound && tran.TranStatus == CCTranStatusCode.Declined
					&& ccProcTrans.Any(i => i.PCTranNumber == tran.PCTranNumber && i.TranStatus == CCTranStatusCode.HeldForReview))
				{
					declineAfterReviewFound = true;
					declinedPCTranNbr = tran.PCTranNumber;
				}
				else if (declineAfterReviewFound && tran.PCTranNumber == declinedPCTranNbr)
				{
					if (tran.TranStatus == CCTranStatusCode.HeldForReview)
					{
						declineAfterReviewFound = false;
					}
				}
				else if (tran.TranStatus != CCTranStatusCode.Declined)
				{
					yield return tran;
				}
			}
		}

		public static bool IsExpired(ICCPaymentTransaction tran)
		{
			return (tran.ExpirationDate.HasValue && tran.ExpirationDate.Value < PXTimeZoneInfo.Now) || tran.TranStatus == CCTranStatusCode.Expired;
		}

		public static bool HasOpenCCTran(IEnumerable<ICCPaymentTransaction> paymentTransaction)
		{
			return paymentTransaction.Any(i=>i.ProcStatus == CCProcStatus.Opened && !IsExpired(i));
		}

		public static ICCPaymentTransaction FindCCLastSuccessfulTran(IEnumerable<ICCPaymentTransaction> ccProcTran)
		{
			IEnumerable<ICCPaymentTransaction> filtered = FilterDeclinedTrans(ccProcTran);
			ICCPaymentTransaction lastTran = null;
			CCProcTranOrderComparer ascComparer = new CCProcTranOrderComparer();
			foreach (ICCPaymentTransaction iTran in filtered)
			{
				if (iTran.ProcStatus != CCProcStatus.Finalized)
					continue;
				if (iTran.TranStatus == CCTranStatusCode.Approved || iTran.TranStatus == CCTranStatusCode.HeldForReview)
				{
					if (lastTran == null || (ascComparer.Compare(iTran, lastTran) > 0))
					{
						lastTran = iTran;
					}
				}
			}
			return lastTran;
		}

		public static ICCPaymentTransaction FindOpenForReviewTran(IEnumerable<ICCPaymentTransaction> ccProcTran)
		{
			IEnumerable<ICCPaymentTransaction> filtered = FilterDeclinedTrans(ccProcTran);
			ICCPaymentTransaction openTran = filtered.Where(i => i.TranStatus == CCTranStatusCode.HeldForReview)
				.Where(i => !ccProcTran.Where(ii => ii.PCTranNumber == i.PCTranNumber && ii.TranNbr != i.TranNbr).Any())
				.FirstOrDefault();
			return openTran;
		}

		public static IEnumerable<ICCPaymentTransaction> FindAuthCaptureActiveTrans(IEnumerable<ICCPaymentTransaction> ccProcTran)
		{
			IEnumerable<ICCPaymentTransaction> filtered = FilterDeclinedTrans(ccProcTran).OrderBy(i => i.TranNbr);
			List<ICCPaymentTransaction> activeTrans = new List<ICCPaymentTransaction>();
			foreach (ICCPaymentTransaction item in filtered)
			{
				if (item.ProcStatus == CCProcStatus.Error || item.TranStatus == CCTranStatusCode.Error)
					continue;

				if (item.TranType == CCTranTypeCode.AuthorizeAndCapture
					|| (item.TranType == CCTranTypeCode.Authorize && item.TranStatus != CCTranStatusCode.Expired)
					|| item.TranType == CCTranTypeCode.CaptureOnly || item.TranType == CCTranTypeCode.PriorAuthorizedCapture)
				{
					var refTran = activeTrans.Where(i => (item.RefTranNbr != null && item.RefTranNbr == i.TranNbr)
						|| i.PCTranNumber == item.PCTranNumber).FirstOrDefault();
					if (refTran != null)
					{
						activeTrans.Remove(refTran);
					}
					activeTrans.Add(item);
				}

				if (item.TranType == CCTranTypeCode.VoidTran)
				{
					var refTran = activeTrans.Where(i => i.PCTranNumber == item.PCTranNumber).FirstOrDefault();
					if (refTran != null)
					{
						activeTrans.Remove(refTran);
					}
				}

				if (item.TranType == CCTranTypeCode.Credit)
				{
					var refTran = activeTrans.Where(i => i.TranNbr == item.RefTranNbr).FirstOrDefault();
					if (refTran != null)
					{
						activeTrans.Remove(refTran);
					}
				}

				if (item.TranType == CCTranTypeCode.Authorize && item.TranStatus == CCTranStatusCode.Expired)
				{
					var refTran = activeTrans.Where(i => i.PCTranNumber == item.PCTranNumber)
						.FirstOrDefault();
					if (refTran != null)
					{
						activeTrans.Remove(refTran);
					}
				}
			}
			return activeTrans;
		}

		public static bool IsActiveTran(CCProcTran procTran)
		{
			bool ret = true;
			if (procTran.TranStatus == CCTranStatusCode.Declined
				|| (procTran.TranType == CCTranTypeCode.VoidTran && procTran.TranStatus == CCTranStatusCode.Approved))
			{
				ret = false;
			}

			if ((procTran.RefTranNbr == null || procTran.TranType == CCTranTypeCode.Credit) && procTran.TranStatus == CCTranStatusCode.Error)
			{
				ret = false;
			}
			if (procTran.TranStatus == CCTranStatusCode.Expired)
			{
				ret = false;
			}
			return ret;
		}

		public static bool IsCompletedTran(CCProcTran procTran)
		{
			bool ret = false;
			if (procTran.TranStatus == CCTranStatusCode.Approved
				&& (procTran.TranType == CCTranTypeCode.AuthorizeAndCapture || procTran.TranType == CCTranTypeCode.CaptureOnly
					|| procTran.TranType == CCTranTypeCode.PriorAuthorizedCapture || procTran.TranType == CCTranTypeCode.Credit))
			{
				ret = true;
			}
			return ret;
		}

		public static bool IsNeedSync(CCProcTran procTran)
		{
			return procTran.TranStatus == CCTranStatusCode.Approved && procTran.Imported == true ? true : false;
		}

		public static bool IsFailedTran(CCProcTran procTran)
		{
			return procTran.TranStatus == CCTranStatusCode.Declined || procTran.TranStatus == CCTranStatusCode.Error;
		}

		public static bool HasCaptureFailed(IEnumerable<ICCPaymentTransaction> trans)
		{
			bool ret = false;
			foreach (ICCPaymentTransaction tran in trans)
			{
				if ((tran.TranType == CCTranTypeCode.VoidTran || tran.TranType == CCTranTypeCode.CaptureOnly
					|| tran.TranType == CCTranTypeCode.PriorAuthorizedCapture || tran.TranType == CCTranTypeCode.AuthorizeAndCapture)
					&& (tran.TranStatus == CCTranStatusCode.Approved || tran.TranStatus == CCTranStatusCode.HeldForReview))
				{
					break;
				}
				else if ((tran.TranType == CCTranTypeCode.PriorAuthorizedCapture || tran.TranType == CCTranTypeCode.AuthorizeAndCapture)
					&& (tran.ProcStatus == CCProcStatus.Error || tran.TranStatus == CCTranStatusCode.Error || tran.TranStatus == CCTranStatusCode.Declined))
				{
					ret = true;
					break;
				}
			}
			return ret;
		}

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2022R1)]
		public static bool HasVoidPreAuthorized(IEnumerable<ICCPaymentTransaction> trans)
		{
			IEnumerable<ICCPaymentTransaction> filtered = FilterDeclinedTrans(trans).OrderByDescending(i =>i.TranNbr);
			var firstTran = filtered.FirstOrDefault();
			if (firstTran != null && firstTran.TranType != CCTranTypeCode.VoidTran) return false;
			bool ret = filtered.Where(i => i.ProcStatus == CCProcStatus.Finalized && i.TranType == CCTranTypeCode.Authorize
				&& (i.TranStatus == CCTranStatusCode.Approved || i.TranStatus == CCTranStatusCode.HeldForReview)
				&& filtered.Where(ii => ii.RefTranNbr == i.TranNbr && ii.TranType == CCTranTypeCode.VoidTran
					&& (ii.TranStatus == CCTranStatusCode.Approved || ii.TranStatus == CCTranStatusCode.HeldForReview)).Any()
			).Any();
			return ret;
		}

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2022R1)]
		public static bool HasVoidAfterReview(IEnumerable<ICCPaymentTransaction> trans)
		{
			IEnumerable<ICCPaymentTransaction> filtered = FilterDeclinedTrans(trans).OrderByDescending(i => i.TranNbr);
			var firstTran = filtered.FirstOrDefault();
			if (firstTran != null && firstTran.TranType != CCTranTypeCode.VoidTran) return false;
			bool ret = filtered.Where(i => i.ProcStatus == CCProcStatus.Finalized && i.TranStatus == CCTranStatusCode.HeldForReview
				&& filtered.Where(ii => ii.RefTranNbr == i.TranNbr && ii.TranType == CCTranTypeCode.VoidTran
					&& ii.TranStatus == CCTranStatusCode.Approved).Any()
			).Any();
			return ret;
		}
	}
}
