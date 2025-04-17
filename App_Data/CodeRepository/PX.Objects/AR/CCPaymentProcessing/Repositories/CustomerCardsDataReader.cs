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

using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.CCProcessingBase;

namespace PX.Objects.AR.CCPaymentProcessing.Repositories
{
	public class CustomerCardsDataReader
	{
		private PXGraph _graph;
		private int? _customerID;
		private string _processingCenterId;
		private Func<PXGraph, int?, ICreditCardDataReader> _readerFactory;

		public CustomerCardsDataReader(PXGraph graph, int? customerId, string processingCenterId, Func<PXGraph, int?, ICreditCardDataReader> readerFactory)
		{
			if (graph == null)
			{
				throw new ArgumentNullException(nameof(graph));
			}
			if (customerId == null)
			{
				throw new ArgumentNullException(nameof(customerId));
			}
			if (string.IsNullOrEmpty(processingCenterId))
			{
				throw new ArgumentNullException(nameof(processingCenterId));
			}
			if (readerFactory == null)
			{
				throw new ArgumentNullException(nameof(readerFactory));
			}

			_graph = graph;
			_customerID = customerId;
			_processingCenterId = processingCenterId;
			_readerFactory = readerFactory;
		}

		private IEnumerable<int?> GetPMIntances()
		{
			return PXSelect<CustomerPaymentMethod, 
				Where<CustomerPaymentMethod.bAccountID, 
					Equal<Required<CustomerPaymentMethod.bAccountID>>,
				And<CustomerPaymentMethod.cCProcessingCenterID, 
					Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>>>>
						.Select(_graph, _customerID, _processingCenterId)
						.Select( detail => detail.GetItem<CustomerPaymentMethod>().PMInstanceID);
		}

		public IEnumerable<ICreditCardDataReader> GetCardReaders()
		{
			return GetPMIntances().Select(id => _readerFactory(_graph, id));
		}
	}
}
