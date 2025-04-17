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

using PX.Api;
using PX.Data;
using System.Collections.Generic;
using System.Linq;
using static PX.Objects.CA.BankFeedTransaction;

namespace PX.Objects.CA.Descriptor
{
	public class CABankFeedMappingSourceHelper
	{
		private Dictionary<string, string> _internalCommonMappings;
		private Dictionary<string, string> _internalMXMappings;
		private Dictionary<string, string> _internalPlaidMappings;

		public CABankFeedMappingSourceHelper(PXCache cache)
		{
			PXCache bfCache = cache.Graph.Caches[typeof(BankFeedTransaction)];
			_internalCommonMappings = new Dictionary<string, string>()
			{
				{ PXUIFieldAttribute.GetDisplayName<accountID>(bfCache), nameof(accountID) },
				{ PXUIFieldAttribute.GetDisplayName<amount>(bfCache), nameof(amount) },
				{ PXUIFieldAttribute.GetDisplayName<category>(bfCache), nameof(category) },
				{ PXUIFieldAttribute.GetDisplayName<checkNumber>(bfCache), nameof(checkNumber) },
				{ PXUIFieldAttribute.GetDisplayName<isoCurrencyCode>(bfCache), nameof(isoCurrencyCode) },
				{ PXUIFieldAttribute.GetDisplayName<date>(bfCache), nameof(date) },
				{ PXUIFieldAttribute.GetDisplayName<memo>(bfCache), nameof(memo) },
				{ PXUIFieldAttribute.GetDisplayName<name>(bfCache), nameof(name) },
				{ PXUIFieldAttribute.GetDisplayName<pending>(bfCache), nameof(pending) },
				{ PXUIFieldAttribute.GetDisplayName<pendingTransactionID>(bfCache), nameof(pendingTransactionID) },
				{ PXUIFieldAttribute.GetDisplayName<transactionID>(bfCache), nameof(transactionID) },
				{ PXUIFieldAttribute.GetDisplayName<type>(bfCache), nameof(type) }
			};

			_internalMXMappings = new Dictionary<string, string>()
			{
				{ PXUIFieldAttribute.GetDisplayName<accountStringId>(bfCache), nameof(accountStringId) },
				{ PXUIFieldAttribute.GetDisplayName<categoryGuid>(bfCache), nameof(categoryGuid) },
				{ PXUIFieldAttribute.GetDisplayName<createdAt>(bfCache), nameof(createdAt) },
				{ PXUIFieldAttribute.GetDisplayName<extendedTransactionType>(bfCache), nameof(extendedTransactionType) },
				{ PXUIFieldAttribute.GetDisplayName<id>(bfCache), nameof(id) },
				{ PXUIFieldAttribute.GetDisplayName<isBillPay>(bfCache), nameof(isBillPay) },
				{ PXUIFieldAttribute.GetDisplayName<isDirectDeposit>(bfCache), nameof(isDirectDeposit) },
				{ PXUIFieldAttribute.GetDisplayName<isExpense>(bfCache), nameof(isExpense) },
				{ PXUIFieldAttribute.GetDisplayName<isFee>(bfCache), nameof(isFee) },
				{ PXUIFieldAttribute.GetDisplayName<isIncome>(bfCache), nameof(isIncome) },
				{ PXUIFieldAttribute.GetDisplayName<isInternational>(bfCache), nameof(isInternational) },
				{ PXUIFieldAttribute.GetDisplayName<isOverdraftFee>(bfCache), nameof(isOverdraftFee) },
				{ PXUIFieldAttribute.GetDisplayName<isPayrollAdvance>(bfCache), nameof(isPayrollAdvance) },
				{ PXUIFieldAttribute.GetDisplayName<isRecurring>(bfCache), nameof(isRecurring) },
				{ PXUIFieldAttribute.GetDisplayName<isSubscription>(bfCache), nameof(isSubscription) },
				{ PXUIFieldAttribute.GetDisplayName<latitude>(bfCache), nameof(latitude) },
				{ PXUIFieldAttribute.GetDisplayName<localizedDescription>(bfCache), nameof(localizedDescription) },
				{ PXUIFieldAttribute.GetDisplayName<localizedMemo>(bfCache), nameof(localizedMemo) },
				{ PXUIFieldAttribute.GetDisplayName<longitude>(bfCache), nameof(longitude) },
				{ PXUIFieldAttribute.GetDisplayName<memberIsManagedByUser>(bfCache), nameof(memberIsManagedByUser) },
				{ PXUIFieldAttribute.GetDisplayName<merchantCategoryCode>(bfCache), nameof(merchantCategoryCode) },
				{ PXUIFieldAttribute.GetDisplayName<merchantGuid>(bfCache), nameof(merchantGuid) },
				{ PXUIFieldAttribute.GetDisplayName<merchantLocationGuid>(bfCache), nameof(merchantLocationGuid) },
				{ PXUIFieldAttribute.GetDisplayName<metadata>(bfCache), nameof(metadata) },
				{ PXUIFieldAttribute.GetDisplayName<originalDescription>(bfCache), nameof(originalDescription) },
				{ PXUIFieldAttribute.GetDisplayName<postedAt>(bfCache), nameof(postedAt) },
				{ PXUIFieldAttribute.GetDisplayName<transactedAt>(bfCache), nameof(transactedAt) },
				{ PXUIFieldAttribute.GetDisplayName<updatedAt>(bfCache), nameof(updatedAt) },
				{ PXUIFieldAttribute.GetDisplayName<userId>(bfCache), nameof(userId) }
			};

			_internalPlaidMappings = new Dictionary<string, string>()
			{
				{ PXUIFieldAttribute.GetDisplayName<accountOwner>(bfCache), nameof(accountOwner) },
				{ PXUIFieldAttribute.GetDisplayName<address>(bfCache), nameof(address) },
				{ PXUIFieldAttribute.GetDisplayName<authorizedDate>(bfCache), nameof(authorizedDate) },
				{ PXUIFieldAttribute.GetDisplayName<authorizedDatetime>(bfCache), nameof(authorizedDatetime) },
				{ PXUIFieldAttribute.GetDisplayName<byOrderOf>(bfCache), nameof(byOrderOf) },
				{ PXUIFieldAttribute.GetDisplayName<city>(bfCache), nameof(city) },
				{ PXUIFieldAttribute.GetDisplayName<country>(bfCache), nameof(country) },
				{ PXUIFieldAttribute.GetDisplayName<datetimeValue>(bfCache), nameof(datetimeValue) },
				{ PXUIFieldAttribute.GetDisplayName<merchantName>(bfCache), nameof(merchantName) },
				{ PXUIFieldAttribute.GetDisplayName<payee>(bfCache), nameof(payee) },
				{ PXUIFieldAttribute.GetDisplayName<payer>(bfCache), nameof(payer) },
				{ PXUIFieldAttribute.GetDisplayName<paymentChannel>(bfCache), nameof(paymentChannel) },
				{ PXUIFieldAttribute.GetDisplayName<paymentMethod>(bfCache), nameof(paymentMethod) },
				{ PXUIFieldAttribute.GetDisplayName<paymentProcessor>(bfCache), nameof(paymentProcessor) },
				{ PXUIFieldAttribute.GetDisplayName<personalFinanceCategory>(bfCache), nameof(personalFinanceCategory) },
				{ PXUIFieldAttribute.GetDisplayName<ppdId>(bfCache), nameof(ppdId) },
				{ PXUIFieldAttribute.GetDisplayName<postalCode>(bfCache), nameof(postalCode) },
				{ PXUIFieldAttribute.GetDisplayName<reason>(bfCache), nameof(reason) },
				{ PXUIFieldAttribute.GetDisplayName<referenceNumber>(bfCache), nameof(referenceNumber) },
				{ PXUIFieldAttribute.GetDisplayName<region>(bfCache), nameof(region) },
				{ PXUIFieldAttribute.GetDisplayName<storeNumber>(bfCache), nameof(storeNumber) },
				{ PXUIFieldAttribute.GetDisplayName<transactionCode>(bfCache), nameof(transactionCode) },
				{ PXUIFieldAttribute.GetDisplayName<unofficialCurrencyCode>(bfCache), nameof(unofficialCurrencyCode) }
			};
		}

		public string GetFieldsForFormula(string bankFeedType)
		{
			List<string> result = new List<string>();
			result.AddRange(_internalCommonMappings.Select(kvp => $"[{kvp.Key}]").ToList());
			switch (bankFeedType)
			{
				case CABankFeedType.MX:
					result.AddRange(_internalMXMappings.Select(kvp => $"[{kvp.Key}]").ToList());
					break;
				case CABankFeedType.Plaid:
				case CABankFeedType.TestPlaid:
					result.AddRange(_internalPlaidMappings.Select(kvp => $"[{kvp.Key}]").ToList());
					break;
			}
			result.Sort();
			return string.Join(";", result.ToArray());
		}

		public string GetFieldNameByDisplayName(string displayName)
		{
			string result = string.Empty;
			if (_internalCommonMappings.TryGetValue(displayName, out result))
				return result;
			if (_internalMXMappings.TryGetValue(displayName, out result))
				return result;
			if (_internalPlaidMappings.TryGetValue(displayName, out result))
				return result;
			return result;
		}
	}
}
