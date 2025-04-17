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
using System;
using System.Linq;
using System.Reflection;

namespace PX.Objects.CA.BankFeed
{
	internal static class CustomizationFeedHelper
	{
		internal static void DisconnectFeed(CABankFeed bankFeed)
		{
			var completed = false;
			if (bankFeed == null || bankFeed.AccessToken == null || bankFeed.Type == null)
			{
				WriteDiconnectionWarnInTrace(bankFeed);
				return;
			}

			var feedType = bankFeed.Type;
			var accessToken = bankFeed.AccessToken;

			try
			{
				var asm = AppDomain.CurrentDomain.GetAssemblies().Where(i => i.FullName.Contains("EIS.BankFeed")).FirstOrDefault();
				if (asm == null || !PXSubstManager.IsSuitableTypeExportAssembly(asm, false))
				{
					WriteDiconnectionWarnInTrace(bankFeed);
					return;
				}

				if (feedType.IsIn(CABankFeedType.Plaid, CABankFeedType.TestPlaid))
				{
					completed = DicsonnectPlaidFeed(asm, accessToken);
				}

				if (feedType == CABankFeedType.MX)
				{
					var guids = bankFeed.AccessToken.Split(';');
					if (guids.Length == 2)
					{
						var userGuid = guids.Where(i => i.StartsWith("USR")).FirstOrDefault();
						var memberGuid = guids.Where(i => i.StartsWith("MBR")).FirstOrDefault();

						completed = DicsonnectMXFeed(asm, userGuid, memberGuid);
					}
				}

				if (!completed)
				{
					WriteDiconnectionWarnInTrace(bankFeed);
				}
			}
			catch (Exception ex)
			{
				var errMessage = ex.Message;
				if (ex.InnerException != null)
				{
					errMessage = ex.InnerException.Message;
				}
				PXTrace.WriteError("Unable to disconnect the {bankFeed} bank feed: {error}.", bankFeed.BankFeedID, errMessage);
			}
		}

		private static bool DicsonnectPlaidFeed(Assembly asm, string accessToken)
		{
			var ret = false;
			if (accessToken == null || !accessToken.StartsWith("access-production")) return ret;

			var types = asm.GetExportedTypes();
			var proxyApiType = types.Where(i => i.Name.Contains("BankFeedsProxyAPI")).FirstOrDefault();
			if (proxyApiType != null)
			{
				var proxyApiObj = Activator.CreateInstance(proxyApiType);

				var getTokenMethod = proxyApiObj.GetType().GetMethod("GetToken");
				var res = getTokenMethod.Invoke(proxyApiObj, null);

				var enumType = types.Where(i => i.Name.Contains("Environment") && i.BaseType == typeof(Enum)).FirstOrDefault();
				var deleteItemRequestType = types.Where(i => i.Name.Contains("DeleteItemRequest")).FirstOrDefault();

				if (enumType != null && deleteItemRequestType != null)
				{
					var enumValue = Enum.Parse(enumType, "Production");
					var requestObj = Activator.CreateInstance(deleteItemRequestType);
					var accessTokenProp = deleteItemRequestType.GetProperty("AccessToken");
					accessTokenProp.SetValue(requestObj, accessToken, null);

					var deleteMethod = proxyApiType.GetMethod("DeleteItem");
					deleteMethod.Invoke(proxyApiObj, new object[] { enumValue, requestObj });
					ret = true;
				}
			}
			return ret;
		}

		private static bool DicsonnectMXFeed(Assembly asm, string userGuid, string memberGuid)
		{
			var ret = false;
			if (userGuid == null || memberGuid == null) return ret;

			var types = asm.GetExportedTypes();
			var proxyApiType = types.Where(i => i.Name.Contains("BankFeedsProxyAPI")).FirstOrDefault();
			if (proxyApiType != null)
			{
				var proxyApiObj = Activator.CreateInstance(proxyApiType);

				var getTokenMethod = proxyApiObj.GetType().GetMethod("GetToken");
				var res = getTokenMethod.Invoke(proxyApiObj, null);

				var deleteMethod = proxyApiType.GetMethod("DeleteMember");
				deleteMethod.Invoke(proxyApiObj, new object[] { memberGuid, userGuid });
				ret = true;
			}
			return ret;
		}

		private static void WriteDiconnectionWarnInTrace(CABankFeed bankFeed)
		{
			PXTrace.WriteWarning("Disconnection of the {bankfeed} bank feed was skipped.", bankFeed.BankFeedID);
		}
	}
}
