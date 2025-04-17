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
using PX.CCProcessingBase;
using PX.Objects.AR.CCPaymentProcessing.Common;

namespace PX.Objects.AR.CCPaymentProcessing.Repositories
{
	public class CardProcessingReadersProvider : ICardProcessingReadersProvider
	{
		private CCProcessingContext _context;
		private String2DateConverterFunc _string2DateConverter;

		public CardProcessingReadersProvider(CCProcessingContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_string2DateConverter = _context.expirationDateConverter;
		}

		public ICreditCardDataReader GetCardDataReader()
		{
			return new CreditCardDataReader(_context.callerGraph, _context.aPMInstanceID);
		}

		public IEnumerable<ICreditCardDataReader> GetCustomerCardsDataReaders()
		{
			return new CustomerCardsDataReader(
				_context.callerGraph,
				_context.aCustomerID,
				_context.processingCenter.ProcessingCenterID,
				(graph, insID) => { return new CreditCardDataReader(graph, insID); }).GetCardReaders();
		}

		public ICustomerDataReader GetCustomerDataReader()
		{
			return new CustomerDataReader(_context);
		}

		public IDocDetailsDataReader GetDocDetailsDataReader()
		{
			return new DocDetailsDataReader(_context.callerGraph, _context.aDocType, _context.aRefNbr);
		}

		public IProcessingCenterSettingsStorage GetProcessingCenterSettingsStorage()
		{
			return new ProcessingCenterSettingsStorage(_context.callerGraph, _context.processingCenter.ProcessingCenterID);
		}

		public String2DateConverterFunc GetExpirationDateConverter()
		{
			return _string2DateConverter;
		}

		public IPaymentMethodDataReader GetPaymentMethodDataReader()
		{
			return new PaymentMethodDataReader(_context.callerGraph, _context.PaymentMethodID);
		}
	}
}
