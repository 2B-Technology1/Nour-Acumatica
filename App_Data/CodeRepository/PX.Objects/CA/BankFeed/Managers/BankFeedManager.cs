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
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace PX.Objects.CA.BankFeed
{
	internal abstract class BankFeedManager
	{
		public const string CategoriesSeparator = ":";
		public abstract (string, string)[] AvailableCorpCardFilters { get; }
		public abstract string[] AvailableTransactionFields { get; }
 		public abstract Task ConnectAsync(CABankFeed bankFeed);
		public abstract Task UpdateAsync(CABankFeed bankFeed);
		public abstract Task ProcessConnectResponseAsync(string responseStr, CABankFeed bankFeed);
		public abstract Task ProcessUpdateResponseAsync(string responseStr, CABankFeed bankFeed);
		public abstract Task<IEnumerable<BankFeedCategory>> GetCategoriesAsync(CABankFeed bankFeed);
		public abstract Task<IEnumerable<BankFeedAccount>> GetAccountsAsync(CABankFeed bankFeed);
		public abstract Task<IEnumerable<BankFeedTransaction>> GetTransactionsAsync(LoadTransactionsData input, CABankFeed bankFeed);
		public abstract Task DeleteAsync(CABankFeed bankFeed);
		public abstract LoadTransactionsData GetTransactionsFilterForTesting(DateTime loadingDate);

		protected virtual CABankFeedMaint GetBankFeedGraph(CABankFeed bankFeed)
		{
			var graph = PXGraph.CreateInstance<CABankFeedMaint>();
			if (bankFeed?.BankFeedID != null)
			{
				graph.BankFeed.Current = PXSelect<CABankFeed, Where<CABankFeed.bankFeedID, Equal<Required<CABankFeed.bankFeedID>>>>
					.Select(graph, bankFeed.BankFeedID);
			}
			return graph;
		}

		protected virtual CABankFeedMaint GetBankFeedGraph()
		{
			return GetBankFeedGraph(null);
		}

		protected virtual T ParseResponse<T>(string responseStr)
		{
			try
			{
				return JsonSerializer.Deserialize<T>(responseStr);
			}
			catch (Exception ex)
			{
				throw new PXException(Messages.CouldNotParseResponseFromForm, ex);
			}
		}
	}
}
