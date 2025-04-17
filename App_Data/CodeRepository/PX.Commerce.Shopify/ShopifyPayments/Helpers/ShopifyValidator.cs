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

using PX.CCProcessingBase;
using PX.CCProcessingBase.Interfaces.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static PX.Commerce.Shopify.ShopifyPayments.ShopifyPluginHelper;
using CCTranType = PX.CCProcessingBase.Interfaces.V2.CCTranType;
using ProcessingInput = PX.CCProcessingBase.Interfaces.V2.ProcessingInput;

namespace PX.Commerce.Shopify.ShopifyPayments
{
    internal static class ShopifyValidator
	{
		public static string ValidateStoreName(string name)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				return ShopifyPluginMessages.StoreName_CannotBeEmptyWithHint;
			}

			return String.Empty;
		}

		public static string Validate(SettingsValue setting)
		{
			if (setting == null)
			{
				return ShopifyPluginMessages.SettingsEmpty;
			}

			string result = String.Empty;

			switch (setting.DetailID)
			{
				case ShopifyPluginHelper.SettingsKeys.Key_StoreName:
					result = ValidateStoreName(setting.Value);
					break;
				default:
					result = Messages.UnknownDetailID;
					break;
			}
			return result;	
		}

		public static string Validate(IEnumerable<SettingsValue> settingValues)
		{
			if (settingValues == null || settingValues.Any() == false)
			{
				return ShopifyPluginMessages.SettingsEmpty;
			}
			return settingValues.Aggregate(String.Empty, (current, sv) => current + Validate(sv));
		}

		public static string ValidateForTransaction(ProcessingInput processingInput)
		{
			if (processingInput == null)
			{
				return Messages.ProcessingInputEmpty;
			}

			StringBuilder stringBuilder = new StringBuilder();

			if (processingInput.Amount <= 0)
			{
				stringBuilder.AppendLine(Messages.AmountMustBePositive);
			}

			string errs = ValidateTranType(processingInput);
			if (!string.IsNullOrEmpty(errs))
			{
				stringBuilder.Append(errs);
			}

			if (stringBuilder.Length != 0)
			{
				return stringBuilder.ToString();
			}
			return String.Empty;
		}

		public static string ValidateTranType(ProcessingInput processingInput)
		{
			StringBuilder stringBuilder = new StringBuilder();
			switch (processingInput.TranType)
			{
				case CCTranType.PriorAuthorizedCapture:
					if (String.IsNullOrWhiteSpace(processingInput.OrigTranID))
					{
						stringBuilder.AppendLine(Messages.OrigTranIDEmpty);
					}
					break;
				case CCTranType.CaptureOnly:
					if (String.IsNullOrWhiteSpace(processingInput.AuthCode) || processingInput.AuthCode.Length != 6)
					{
						stringBuilder.AppendLine(Messages.AuthCodeMustContain6Symbols);
					}
					break;
				case CCTranType.Credit:
					if (String.IsNullOrWhiteSpace(processingInput.OrigTranID))
					{
						stringBuilder.AppendLine(Messages.OrigTranIDEmpty);
					}
					break;
				case CCTranType.Void:
					if (String.IsNullOrWhiteSpace(processingInput.OrigTranID))
					{
						stringBuilder.AppendLine(Messages.OrigTranIDEmpty);
					}
					break;
				case CCTranType.VoidOrCredit:
					stringBuilder.AppendLine(Messages.VoidOrCreditIsNotImplemented);
					break;
			}
			return stringBuilder.ToString();
		}
	}
}
