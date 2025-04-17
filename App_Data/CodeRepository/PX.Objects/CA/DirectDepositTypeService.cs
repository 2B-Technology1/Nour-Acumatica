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

namespace PX.Objects.CA
{
	public class DirectDepositTypeService
	{
		private IEnumerable<IDirectDepositType> _directDepositTypes;
		public DirectDepositTypeService(IEnumerable<IDirectDepositType> directDepositTypes)
		{
			_directDepositTypes = directDepositTypes;
		}

		public IEnumerable<DirectDepositType> GetDirectDepositTypes()
		{
			foreach (var type in _directDepositTypes)
			{
				if (type.IsActive())
				{
					yield return type.GetDirectDepositType();
				}
			}
		}

		public IEnumerable<PaymentMethodDetail> GetDefaults(string code)
		{
			foreach (var type in _directDepositTypes)
			{
				if (type.IsActive())
				{
					var currentType = type.GetDirectDepositType();
					if (currentType.Code == code)
					{
						return type.GetDefaults();
					}
				}
			}
			return Enumerable.Empty<PaymentMethodDetail>();
		}

		public void SetPaymentMethodDefaults(PXCache cache)
		{
			PaymentMethod paymentMethod = (PaymentMethod)cache.Current;

			foreach (var type in _directDepositTypes)
			{
				if (type.IsActive())
				{
					var currentType = type.GetDirectDepositType();
					if (currentType.Code == paymentMethod.DirectDepositFileFormat)
					{
						type.SetPaymentMethodDefaults(cache);
						break;
					}
				}
			}
		}
	}
}
