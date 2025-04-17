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
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.CM.Extensions;
using PX.Objects.Common.Interfaces;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace PX.Objects.EP
{
	/// <summary>
	/// A helper for the approval mechanism.
	/// </summary>
	public static class EPApprovalHelper
	{
		public static class PXTimeZoneInfoToday
		{
			public class dayBegin : PX.Data.BQL.BqlDateTime.Constant<dayBegin>
			{
				public dayBegin() : base(PXTimeZoneInfo.Today)
				{
				}
			}
			public class dayEnd : PX.Data.BQL.BqlDateTime.Constant<dayEnd>
			{
				public dayEnd() : base(PXTimeZoneInfo.Today.AddDays(1).AddSeconds(-1))
				{
				}
			}
		}

		[Serializable]
		internal class PXReassignmentApproverNotAvailableException : PXSetPropertyException
		{
			public PXReassignmentApproverNotAvailableException(PXErrorLevel errorLevel)
				: base(MessagesNoPrefix.ReassignmentApproverNotAvailable, errorLevel) { }

			public PXReassignmentApproverNotAvailableException(SerializationInfo info, StreamingContext context)
				: base(info, context) { }

		}

		public static string BuildEPApprovalDetailsString(PXCache sender, IApprovalDescription currentDocument)
		{
			CashAccount ca = PXSelect<CashAccount>.Search<CashAccount.cashAccountID>(sender.Graph, currentDocument.CashAccountID).First();
			PaymentMethod pm = PXSelect<PaymentMethod>.Search<PaymentMethod.paymentMethodID>(sender.Graph, currentDocument.PaymentMethodID).First();
			CurrencyInfo ci = PXSelect<CurrencyInfo>.Search<CurrencyInfo.curyInfoID>(sender.Graph, currentDocument.CuryInfoID).First();

			return string.Concat(ca?.Descr, " (", pm?.Descr, "; ", GetChargeString(currentDocument, ci), ")");
		}

		private static string GetChargeString(IApprovalDescription currentDocument, CurrencyInfo ci)
		{
			if (currentDocument.CuryChargeAmt == null || currentDocument.CuryChargeAmt == 0.0m)
				return PXLocalizer.Localize(Common.Messages.NoCharges);
			else
			{
				int precision = ci.BasePrecision ?? 4;
				return string.Join("=",
					PXLocalizer.Localize(Common.Messages.Charges),
					Math.Round(currentDocument.CuryChargeAmt.Value, precision, MidpointRounding.AwayFromZero).ToString("N" + precision)
					);
			}
		}

		/// <summary>
		/// Find today delegate for approver, if no delegate found same contactID returned or exception thrown
		/// </summary>
		public static int? GetTodayApproverContactID(PXGraph graph, int? contactID, ref int? delegationRecordID, bool throwExceptionIfNoApproveFound = false, List<int?> list = null)
		{
			if (list == null)
			{
				list = new List<int?>();
			}
			else
			{
				if (list.Contains(contactID))
				{
					PXTrace.WriteInformation(Messages.TraceReassignToDelegateCantFindAvailableDelagete, list[0]);
					delegationRecordID = null;
					if (throwExceptionIfNoApproveFound)
					{
						throw new PXReassignmentApproverNotAvailableException(PXErrorLevel.RowWarning);
					}
					return list[0];
				}
			}

			var todayDelegate =
					SelectFrom<
						EPWingmanForApprovals>
					.InnerJoin<BAccount>
						.On<BAccount.bAccountID.IsEqual<EPWingmanForApprovals.employeeID>>
					.InnerJoin<BAccount2>
						.On<BAccount2.bAccountID.IsEqual<EPWingmanForApprovals.wingmanID>>
					.Where<
						BAccount.defContactID.IsEqual<@P.AsInt>
						.And<EPWingmanForApprovals.startsOn.IsLessEqual<EPApprovalHelper.PXTimeZoneInfoToday.dayEnd>>
						.And<Brackets<
								EPWingmanForApprovals.expiresOn.IsNull
								.Or< EPWingmanForApprovals.expiresOn.IsGreaterEqual<EPApprovalHelper.PXTimeZoneInfoToday.dayBegin>>
							>>
					>
					.View
					.SelectSingleBound(graph, null, new object[] { contactID });

			if (todayDelegate.Any() == false)
				return contactID;

			list.Add(contactID);
			contactID = todayDelegate.RowCast<BAccount2>().First().DefContactID;
			delegationRecordID = todayDelegate.RowCast<EPWingmanForApprovals>().First().RecordID;

			return GetTodayApproverContactID(graph, contactID, ref delegationRecordID, throwExceptionIfNoApproveFound, list);
		}

		/// <summary>
		/// Reassign approval to contactID
		/// </summary>
		public static void ReassignToContact(PXGraph graph, EPApproval approval, int? contactID, bool? ignoreApproversDelegations)
		{
			if (approval == null)
			{
				throw new PXSetPropertyException(MessagesNoPrefix.ApprovalRecordNotFound, PXErrorLevel.RowError);
			}

			EPRule rule = EPApproval.FK.Rule.FindParent(graph, approval);

			if (rule == null)
			{
				throw new PXSetPropertyException(MessagesNoPrefix.ReassignmentOfApprovalsNotSupported, PXErrorLevel.RowError);
			}
			else if (rule.AllowReassignment == false)
			{
				throw new PXSetPropertyException(MessagesNoPrefix.ReassignmentNotAllowed, PXErrorLevel.RowError);
			}

			if (ignoreApproversDelegations == false)
			{
				int? delegationRecordID = approval.DelegationRecordID;
				approval.OwnerID = EPApprovalHelper.GetTodayApproverContactID(graph, contactID, ref delegationRecordID, true);
				approval.DelegationRecordID = delegationRecordID;
			}
			else
			{
				approval.OwnerID = contactID;
				approval.IgnoreDelegations = true;
			}
		}

		/// <summary>
		/// Reassign approval to today's delegate of contactID
		/// </summary>
		public static void ReassignToDelegate(PXGraph graph, EPApproval approval, int? contactID)
		{
			if (approval == null)
			{
				throw new PXSetPropertyException(MessagesNoPrefix.ApprovalRecordNotFound, PXErrorLevel.RowError);
			}

			EPRule rule = EPApproval.FK.Rule.FindParent(graph, approval);

			if (rule == null)
			{
				throw new PXSetPropertyException(MessagesNoPrefix.ReassignmentOfApprovalsNotSupported, PXErrorLevel.RowError);
			}

			int? delegationRecordID = approval.DelegationRecordID;
			approval.OwnerID = EPApprovalHelper.GetTodayApproverContactID(graph, contactID, ref delegationRecordID, true);
			approval.DelegationRecordID = delegationRecordID;
		}
	}
}
