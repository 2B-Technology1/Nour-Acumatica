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
using PX.Data;

namespace PX.Objects.CA
{
	public class PaymentMethodAccountHelper
	{
		public static void VerifyQuickBatchGenerationOnRowPersisting(PXCache sender, PaymentMethodAccount row)
		{
			if(row.APQuickBatchGeneration == true && row.APAutoNextNbr != true)
			{
				sender.RaiseExceptionHandling<PaymentMethodAccount.aPQuickBatchGeneration>(row, row.APQuickBatchGeneration, new PXSetPropertyException(Messages.APSuggestNextNumberCannotBeClearedIfQuickBatchGenerationIsSelected));
				return;
			}

			var aPLastRefNbr = row.APLastRefNbr;
			if (row.APQuickBatchGeneration == true && string.IsNullOrEmpty(aPLastRefNbr))
			{
				sender.RaiseExceptionHandling<PaymentMethodAccount.aPQuickBatchGeneration>(row, row.APQuickBatchGeneration, new PXSetPropertyException(Messages.APLastReferenceNumberCannotBeEmptyIfQuickBatchGenerationSelected));
				return;
			}

			if (row.APQuickBatchGeneration == true && !CS.AutoNumberAttribute.TryToGetNextNumber(aPLastRefNbr, 1, out aPLastRefNbr))
			{
				if (CS.AutoNumberAttribute.CheckIfNumberEndsDigit(row.APLastRefNbr))
				{
					sender.RaiseExceptionHandling<PaymentMethodAccount.aPQuickBatchGeneration>(row, row.APQuickBatchGeneration, new PXSetPropertyException(Messages.APLastReferenceNumberCannotBeIncremented));
				}
				else
				{
					sender.RaiseExceptionHandling<PaymentMethodAccount.aPQuickBatchGeneration>(row, row.APQuickBatchGeneration, new PXSetPropertyException(Messages.APLastReferenceNumberMustEndWithNumber));
				}
				return;
			}
		}

		public static void APQuickBatchGenerationFieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if ((e.NewValue as bool?) != true)
			{
				return;
			}

			var row = (PaymentMethodAccount)e.Row;

			try
			{
				VerifyAPLastReferenceNumber<PaymentMethodAccount.aPQuickBatchGeneration>(row?.APAutoNextNbr, row?.APLastRefNbr, null);
			}
			catch(PXSetPropertyException ex)
			{
				cache.RaiseExceptionHandling<PaymentMethodAccount.aPQuickBatchGeneration>(e.Row, e.NewValue, ex);
			}
		}

		public static bool TryToVerifyAPLastReferenceNumber<TField>(bool? suggestNextNbr, string aPLastRefNbr, string cashAccountCD, out PXException error)
			where TField : IBqlField
		{
			try
			{
				VerifyAPLastReferenceNumber<TField>(suggestNextNbr, aPLastRefNbr, cashAccountCD);
				error = null;
				return true;
			}
			catch(PXSetPropertyException ex)
			{
				error = ex;
				return false;
			}
		}

		public static void VerifyAPLastReferenceNumber<TField>(bool? suggestNextNbr, string aPLastRefNbr, string cashAccountCD)
			where TField : IBqlField
		{
			var outAPLastRefNbr = aPLastRefNbr;

			var accountSet = !string.IsNullOrEmpty(cashAccountCD);

			if (suggestNextNbr != true)
			{
				throw new PXSetPropertyException<TField>(accountSet ? PXLocalizer.LocalizeFormat(Messages.APSuggestNextNumberShouldBeSelectedForCashAccount, cashAccountCD) : Messages.APSuggestNextNumberCannotBeClearedIfQuickBatchGenerationIsSelected);
			}
			if (string.IsNullOrEmpty(aPLastRefNbr))
			{
				throw new PXSetPropertyException<TField>(accountSet ? PXLocalizer.LocalizeFormat(CA.Messages.SpecifyAPLastReferenceNumber, cashAccountCD) : Messages.APLastReferenceNumberCannotBeEmptyIfQuickBatchGenerationSelected);
			}
			else if (!CS.AutoNumberAttribute.TryToGetNextNumber(aPLastRefNbr, 1, out outAPLastRefNbr))
			{
				if (CS.AutoNumberAttribute.CheckIfNumberEndsDigit(aPLastRefNbr))
				{
					throw new PXSetPropertyException<TField>(accountSet ? PXLocalizer.LocalizeFormat(CA.Messages.APLastReferenceNumberCannotBeIncrementedForCashAccount, cashAccountCD) : Messages.APLastReferenceNumberCannotBeIncremented);
				}
				else
				{
					throw new PXSetPropertyException<TField>(accountSet ? PXLocalizer.LocalizeFormat(CA.Messages.APLastReferenceNumberMustEndWithNumberForCashAccount, cashAccountCD) : Messages.APLastReferenceNumberMustEndWithNumber);
				}
			}
		}

		public static bool TryToVerifyAPLastReferenceNumber(PXGraph graph, string paymentMethodID, int? cashAccountID, out string errorMessage, int count = 1, string filterLastRefNbr = "")
		{
			try
			{
				VerifyAPLastReferenceNumber(graph, paymentMethodID, cashAccountID, count, filterLastRefNbr, true);
				errorMessage = string.Empty;
				return true;
			}
			catch(PXException e)
			{
				errorMessage = e.Message;
				return false;
			}
		}

		public static void VerifyAPLastReferenceNumber(PXGraph graph, string paymentMethodID, int? cashAccountID, int count = 1, string filterLastRefNbr = "", bool skipSuggestNextNbrCheck = false)
		{
			var paymentMethodAccountResults = (PXResult<PaymentMethodAccount, CashAccount>)PXSelectReadonly2<PaymentMethodAccount,
					InnerJoin<CashAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
					Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
						And<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>>>>.Select(graph, cashAccountID, paymentMethodID);

			PaymentMethodAccount paymentMethodAccount = paymentMethodAccountResults;
			CashAccount cashAccount = paymentMethodAccountResults;

			bool? suggestNextNbr = paymentMethodAccount.APAutoNextNbr;
			if (skipSuggestNextNbrCheck)
			{
				suggestNextNbr = true;
			}

			VerifyAPLastRefNbr(suggestNextNbr, string.IsNullOrEmpty(filterLastRefNbr) ? paymentMethodAccount.APLastRefNbr : filterLastRefNbr, cashAccount.CashAccountCD, count);
		}

		public static void VerifyAPLastReferenceNumberSettings(PXGraph graph, string paymentMethodID, int? cashAccountID, int count, string filterLastRefNbr)
		{
			var paymentMethodAccountResults = (PXResult<PaymentMethodAccount, CashAccount>)PXSelectReadonly2<PaymentMethodAccount,
					InnerJoin<CashAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
					Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
						And<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>>>>.Select(graph, cashAccountID, paymentMethodID);

			PaymentMethodAccount paymentMethodAccount = paymentMethodAccountResults;
			CashAccount cashAccount = paymentMethodAccountResults;

			string nextRefNbr = null;
			if (paymentMethodAccount.APAutoNextNbr != true)
			{
				nextRefNbr = filterLastRefNbr;
			}
			else
			{
				nextRefNbr = paymentMethodAccount.APLastRefNbr;
			}

			VerifyAPLastRefNbr(true, nextRefNbr, cashAccount.CashAccountCD, count);
		}

		public static void VerifyAPLastRefNbr(bool? suggestNextNbr, string aPLastRefNbr, string cashAccountCD, int count = 1)
		{
			var outAPLastRefNbr = aPLastRefNbr;

			if(suggestNextNbr != true)
			{
				throw new PXException(Messages.APSuggestNextNumberShouldBeSelectedForCashAccount, cashAccountCD);
			}
			if (string.IsNullOrEmpty(aPLastRefNbr))
			{
				throw new PXException(CA.Messages.SpecifyAPLastReferenceNumber, cashAccountCD);
			}
			else if (!CS.AutoNumberAttribute.TryToGetNextNumber(aPLastRefNbr, count, out outAPLastRefNbr))
			{
				if (CS.AutoNumberAttribute.CheckIfNumberEndsDigit(aPLastRefNbr))
				{
					throw new PXException(CA.Messages.APLastReferenceNumberCannotBeIncrementedForCashAccount, cashAccountCD);
				}
				else
				{
					throw new PXException(CA.Messages.APLastReferenceNumberMustEndWithNumberForCashAccount, cashAccountCD);
				}
			}
		}

		public static void VerifyAPAutoNextNbr(PXCache cache, PaymentMethodAccount paymentMethodAccount, PaymentMethod paymentMethod)
		{
			PXException exception = null;

			if (paymentMethodAccount.APAutoNextNbr != true && paymentMethod?.APAdditionalProcessing == CA.PaymentMethod.aPAdditionalProcessing.CreateBatchPayment)
			{
				exception = new PXSetPropertyException<PaymentMethodAccount.aPAutoNextNbr>(Messages.APSuggestNextNumberShouldBeSelected, PXErrorLevel.RowWarning);
			}

			cache.RaiseExceptionHandling<PaymentMethodAccount.aPAutoNextNbr>(paymentMethodAccount, paymentMethodAccount.APAutoNextNbr, exception);
		}
	}
}
