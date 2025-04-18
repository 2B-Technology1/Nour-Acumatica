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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using PX.CCProcessingBase;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using PX.Objects.CA;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using PX.Objects.AR.CCPaymentProcessing.Specific;
using System.Text.RegularExpressions;

namespace PX.Objects.AR.CCPaymentProcessing.Helpers
{
	public static class CCProcessingHelper
	{
		public const string CustomerPrefix = "__";
		public const string MaskedCardTmpl = "****-****-****-";
		private static readonly Regex parseCardNum = new Regex("[\\d]+", RegexOptions.Compiled);
		#region Processing Center Methods

		public static IEnumerable GetPMdetails(PXGraph graph, CustomerPaymentMethod cpm)
		{
			if (cpm == null) yield break;

			int? pmInstanceId = cpm.PMInstanceID;
			PaymentMethod paymentMethod = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID,Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(graph, cpm.PaymentMethodID);
			string paymentType = paymentMethod?.PaymentType;

			var items = PXSelectJoin<CustomerPaymentMethodDetail, InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
				And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
				Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(graph, pmInstanceId);

			if (pmInstanceId != null && pmInstanceId > 0)
			{
				if (paymentType == PaymentMethodType.CreditCard || paymentType == PaymentMethodType.EFT)
				{
					PXResult<CustomerPaymentMethodDetail, PaymentMethodDetail> stored = HasSavedPaymentProfile(items);
					if (stored != null)
					{
						yield return stored;
						yield break;
					}
				}
			}

			bool ccProcCenterSupportProfileManagement = false;
			if (cpm.CCProcessingCenterID != null && (paymentType == PaymentMethodType.CreditCard || paymentType == PaymentMethodType.EFT) &&
				IsFeatureSupported(graph, pmInstanceId, CCProcessingFeature.ProfileManagement, false))
			{
				ccProcCenterSupportProfileManagement = true;
			}

			foreach (PXResult<CustomerPaymentMethodDetail, PaymentMethodDetail> res in items)
			{
				PaymentMethodDetail pmd = res;
				if (!ccProcCenterSupportProfileManagement
					|| (ccProcCenterSupportProfileManagement && pmd.IsCCProcessingID == true))
				{
					yield return res;
				}
			}
		}

		public static string GetExpirationDateFormat(PXGraph graph, string ProcessingCenterID)
		{
			PXResultset<CCProcessingCenter> pc = PXSelectJoin<CCProcessingCenter, LeftJoin<CCProcessingCenterDetail,
				On<CCProcessingCenterDetail.processingCenterID, Equal<CCProcessingCenter.processingCenterID>,
					And<CCProcessingCenterDetail.detailID, Equal<Required<CCProcessingCenterDetail.detailID>>>>>,
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>
				.Select(graph, InterfaceConstants.ExpDateFormatDetailID, ProcessingCenterID);
			if (pc.Count == 0)
			{
				return null;
			}
			CCProcessingCenterDetail detail = pc[0].GetItem<CCProcessingCenterDetail>();
			if (string.IsNullOrEmpty(detail.DetailID))
			{
				return null;
			}
			return detail.Value;
		}
		
		public static bool IsCCPIDFilled(PXGraph graph, int? PMInstanceID)
		{
			if (PMInstanceID == null || PMInstanceID.Value < 0)
				return false;

			CustomerPaymentMethod cpm = PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.pMInstanceID,
				Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(graph, PMInstanceID);

			if (cpm == null)
				return false;

			PXResultset<PaymentMethodDetail> paymentMethodDetail = PXSelectJoin<PaymentMethodDetail, LeftJoin<CustomerPaymentMethodDetail,
				On<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
					And<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
						And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
						And<CustomerPaymentMethodDetail.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>>>>,
				Where<PaymentMethodDetail.isCCProcessingID, Equal<True>,
					And<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>>>>.Select(graph, PMInstanceID, cpm.PaymentMethodID);

			PaymentMethodDetail pmIDDetail = paymentMethodDetail.Count > 0 ? paymentMethodDetail[0].GetItem<PaymentMethodDetail>() : null;
			CustomerPaymentMethodDetail ccpIDDetail = paymentMethodDetail.Count > 0 ? paymentMethodDetail[0].GetItem<CustomerPaymentMethodDetail>() : null;

			if (IsTokenizedPaymentMethod(graph, PMInstanceID) && pmIDDetail == null)
			{
				throw new PXException(Messages.PaymentMethodNotConfigured);
			}
			return ccpIDDetail != null && !string.IsNullOrEmpty(ccpIDDetail.Value);
		}

		public static bool IsTokenizedPaymentMethod(PXGraph graph, int? PMInstanceID)
		{
			return IsFeatureSupported(graph, PMInstanceID, CCProcessingFeature.ProfileManagement, false);
		}

		public static bool IsTokenizedPaymentMethod(PXGraph graph, int? PMInstanceID, bool CheckJustDeletedPM = false)
		{
			return IsFeatureSupported(graph, PMInstanceID, CCProcessingFeature.ProfileManagement, CheckJustDeletedPM);
		}

		public static bool IsHFPaymentMethod(PXGraph graph, int? pmInstanceID, bool throwOnError)
		{
			CCProcessingCenter processingCenter = GetProcessingCenter(graph, pmInstanceID, false);
			bool ret = CCProcessingFeatureHelper.IsFeatureSupported(processingCenter, CCProcessingFeature.HostedForm, throwOnError)
				|| CCProcessingFeatureHelper.IsFeatureSupported(processingCenter, CCProcessingFeature.ProfileForm, throwOnError);
			return ret;
		}

		public static bool IsFeatureSupported(PXGraph graph, int? PMInstanceID, CCProcessingFeature FeatureName, bool CheckJustDeletedPM)
		{
			CCProcessingCenter processingCenter = GetProcessingCenter(graph, PMInstanceID, CheckJustDeletedPM);
			return CCProcessingFeatureHelper.IsFeatureSupported(processingCenter, FeatureName, false);
		}

		[Obsolete(PX.Objects.Common.Messages.MethodIsObsoleteAndWillBeRemoved2019R2)]
		public static CustomerPaymentMethod GetCustomerPaymentMethod(PXGraph graph, int? PMInstanceID)
		{
			CustomerPaymentMethod current = null;
			if (PMInstanceID != null)
			{
				//Clear SelectQueries because sometimes it stores the wrong value and Select returns null.
				PXSelectBase<CustomerPaymentMethod> cmd = new PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>(graph);
				cmd.View.Clear();
				current = (CustomerPaymentMethod)cmd.View.SelectSingle(PMInstanceID);
			}
			if (current == null)
			{
				//assumin that payment method was just deleted
				IEnumerator cpmEnumerator = graph.Caches[typeof(CustomerPaymentMethod)].Deleted.GetEnumerator();
				if (cpmEnumerator.MoveNext())
				{
					current = (CustomerPaymentMethod)cpmEnumerator.Current;
				}
			}
			return current;
		}

		public static CCProcessingCenter GetProcessingCenter(PXGraph graph, string ProcessingCenterID)
		{
			return PXSelect<CCProcessingCenter, Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>.Select(graph, ProcessingCenterID);
		}

		public static CCProcessingCenter GetProcessingCenter(PXGraph graph, int? PMInstanceID, bool UsePMFromCacheDeleted)
		{
			if (PMInstanceID == null && !UsePMFromCacheDeleted) 
				return null;

			CCProcessingCenter processingCenter = null;
			if (PMInstanceID != null)
			{
				PXResult<CustomerPaymentMethod, CCProcessingCenter> res =
				(PXResult<CustomerPaymentMethod, CCProcessingCenter>)PXSelectJoin<
					CustomerPaymentMethod,
					InnerJoin<CCProcessingCenter,
						On<CCProcessingCenter.processingCenterID, Equal<CustomerPaymentMethod.cCProcessingCenterID>>>,
					Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>
					.Select(graph, PMInstanceID);

				processingCenter = (CCProcessingCenter)res;
			}

			if (processingCenter == null && UsePMFromCacheDeleted)
			{
				//assuming that payment method was just deleted
				IEnumerator cpmEnumerator = graph.Caches[typeof(CustomerPaymentMethod)].Deleted.GetEnumerator();
				if (cpmEnumerator.MoveNext())
				{
					CustomerPaymentMethod customerPaymentMethod = (CustomerPaymentMethod)cpmEnumerator.Current;
					processingCenter = GetProcessingCenter(graph, customerPaymentMethod.CCProcessingCenterID);
				}
			}

			return processingCenter;
		}

		public static bool? CCProcessingCenterNeedsExpDateUpdate(PXGraph graph, CCProcessingCenter ProcessingCenter)
		{
			if (CCProcessingFeatureHelper.IsFeatureSupported(ProcessingCenter, CCProcessingFeature.ProfileManagement))
			{
				PXResultset<CustomerPaymentMethod> unupdatedCpms = PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.cCProcessingCenterID,
					Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>, And<CustomerPaymentMethod.expirationDate, IsNull>>>.Select(graph, ProcessingCenter.ProcessingCenterID);
				return unupdatedCpms.Count != 0;
			}
			return null;
		}

		public static string GetTokenizedPMsString(PXGraph graph)
		{
			List<CCProcessingCenter> tokenizedPCs = new List<CCProcessingCenter>();
			HashSet<string> pmSet = new HashSet<string>();
			foreach (CCProcessingCenter pc in PXSelect<CCProcessingCenter, Where<CCProcessingCenter.isActive, Equal<True>>>.Select(graph))
			{
				if (CCProcessingFeatureHelper.IsFeatureSupported(pc, CCProcessingFeature.ProfileManagement)
					&& CCProcessingCenterNeedsExpDateUpdate(graph, pc) != false)
				{
					tokenizedPCs.Add(pc);
				}
			}

			foreach (CCProcessingCenter pc in tokenizedPCs)
			{
				foreach (PXResult<CustomerPaymentMethod, PaymentMethod> tokenizedPM in PXSelectJoinGroupBy<CustomerPaymentMethod,
					InnerJoin<PaymentMethod, On<CustomerPaymentMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>>,
					Where<CustomerPaymentMethod.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>>,
					Aggregate<GroupBy<CustomerPaymentMethod.paymentMethodID>>>.Select(graph, pc.ProcessingCenterID))
				{
					PaymentMethod pm = tokenizedPM;
					pmSet.Add(pm.Descr);
				}
			}

			if (pmSet.Count == 0)
			{
				return string.Empty;
			}

			StringBuilder sb = new StringBuilder();

			foreach (string descr in pmSet)
			{
				if (sb.Length > 0)
				{
					sb.Append(", ");
				}
				sb.Append(descr);
			}

			return sb.ToString();
		}
		#endregion

		#region Multi Credit Card Methods

		public static bool IsCreditCardCountEnough(int creditCardCount, int limit)
		{
			return creditCardCount != 0 && creditCardCount >= limit;
		}

		public static int CustomerProfileCountPerCustomer(PXGraph graph, int? aBAccountID, string aCCProcessingCenterID)
		{
			PXResult<CustomerProcessingCenterID> result =
				PXSelectGroupBy<CustomerProcessingCenterID,
				Where<CustomerProcessingCenterID.bAccountID, Equal<Required<CustomerPaymentMethod.bAccountID>>,
					And<CustomerProcessingCenterID.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>>>,
				Aggregate<Count<CustomerProcessingCenterID.customerCCPID>>>
					.Select(graph, aBAccountID, aCCProcessingCenterID);
			int customerProfileCount = result.RowCount ?? 0;
			return customerProfileCount;
		}
		public static Customer GetCustomer(PXGraph graph, int? aBAccountID)
		{
			return Customer.PK.Find(graph, aBAccountID);
		}

		public static Customer GetCustomer(PXGraph graph, string acctCD)
		{
			return new PXSelectReadonly<Customer,
					Where<Customer.acctCD, Equal<Required<Customer.acctCD>>>>(graph)
					.SelectSingle(acctCD);
		}
		public static string BuildPrefixForCustomerCD(int customerProfileCount, CCProcessingCenter processingCenter)
		{
			return (customerProfileCount + 1) + CustomerPrefix;
		}
		#endregion
		public static string ExtractStreetAddress(IAddressBase aAddress)
		{
			var res = new[] { aAddress.AddressLine1, aAddress.AddressLine2, aAddress.AddressLine3 }
				.Where(i => !string.IsNullOrWhiteSpace(i))
				.Aggregate(string.Empty,(cur, next) => string.IsNullOrEmpty(cur) ? next : cur + ", " + next);
			return res;
		}

		public static ProcessingStatus GetProcessingStatusByTranData(V2.TransactionData tranData)
		{
			string tranStatusCode = CCTranStatusCode.GetCode(V2Converter.ConvertTranStatus(tranData.TranStatus));
			string tranTypeCode = CCTranTypeCode.GetTypeCode(V2Converter.ConvertTranType(tranData.TranType.Value));
			string procStatus = ExtTransactionProcStatusCode.GetStatusByTranStatusTranType(tranStatusCode, tranTypeCode);
			return ExtTransactionProcStatusCode.GetProcessingStatusByProcStatusStr(procStatus);
		}

		public static bool IsV1ProcessingInterface(Type pluginType)
		{
			bool ret = CCPluginTypeHelper.CheckParentClass(pluginType, PluginConstants.V1PluginBaseFullName, 0, 3);
			return ret;
		}

		public static V2.ICCProcessingPlugin IsV2ProcessingInterface(object pluginObject)
		{
			var ret = pluginObject as V2.ICCProcessingPlugin;
			return ret;
		}

		public static bool IsV2ProcessingInterface(Type pluginType)
		{
			return pluginType.GetInterfaces().Contains(typeof(V2.ICCProcessingPlugin));
		}

		public static void CheckHttpsConnection()
		{
			if (HttpContext.Current?.Request != null)
			{
				Uri uri = new Uri(HttpContext.Current.Request.GetWebsiteUrl());
				if (uri.Scheme != Uri.UriSchemeHttps)
					throw new PXException(CCProcessingBase.Messages.MustUseHttps);
			}
		}

		[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		public static string FormatMaskedCardNumber(string cardNbr, string cardType)
		{
			string ret = null;
			Match match = parseCardNum.Match(cardNbr);
			if (match.Success)
			{
				if (cardType == null)
				{
					ret = MaskedCardTmpl + match.Value;
				}
				else
				{
					ret = cardType.Trim() + ":" + MaskedCardTmpl + match.Value;
				}	
			}
			return ret;
		}

		private static PXResult<CustomerPaymentMethodDetail, PaymentMethodDetail> HasSavedPaymentProfile(PXResultset<CustomerPaymentMethodDetail> items)
		{
			PXResult<CustomerPaymentMethodDetail, PaymentMethodDetail> ret = null;
			foreach (PXResult<CustomerPaymentMethodDetail, PaymentMethodDetail> item in items)
			{
				CustomerPaymentMethodDetail det = item;
				PaymentMethodDetail pmd = item;
				if (pmd.IsCCProcessingID == true && !string.IsNullOrEmpty(det.Value))
				{
					ret = item;
					break;
				}
			}
			return ret;
		}

		public static string DeleteCustomerPrefix(string customerID)
		{
			if (customerID == null)
				return customerID;

			const string prefDelimeter = CustomerPrefix;
			int index = customerID.IndexOf(prefDelimeter);

			if (index >= 0)
			{
				customerID = customerID.Substring(index + prefDelimeter.Length);
			}
			return customerID;
		}

		public static string GetTransactionTypeName(Common.CCTranType tranType)
		{
			string ret = string.Empty;
			if (tranType == Common.CCTranType.AuthorizeOnly)
			{
				ret = PXMessages.LocalizeNoPrefix(Messages.Authorization);
			}
			if (CCTranTypeCode.IsCaptured(tranType))
			{
				ret = PXMessages.LocalizeNoPrefix(Messages.Capture);
			}
			if (tranType == Common.CCTranType.Void)
			{
				ret = PXMessages.LocalizeNoPrefix(Messages.Void);
			}
			return ret;
		}

		public static bool PaymentMethodSupportsIntegratedProcessing(PaymentMethod pm)
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.integratedCardProcessing>()
				&& (pm?.PaymentType == CA.PaymentMethodType.CreditCard || pm?.PaymentType == CA.PaymentMethodType.EFT)
				&& pm?.ARIsProcessingRequired == true;
		}

		public static bool IntegratedProcessingActivated(ARSetup setup)
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.integratedCardProcessing>()
				&& setup?.IntegratedCCProcessing == true;
		}
	}
}
