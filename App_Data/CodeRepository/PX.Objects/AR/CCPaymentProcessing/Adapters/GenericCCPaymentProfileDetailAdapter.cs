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
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.AR.CCPaymentProcessing
{
	class GenericCCPaymentProfileDetailAdapter<T,P> : ICCPaymentProfileDetailAdapter 
		where T : class, IBqlTable, ICCPaymentProfileDetail, new()
		where P : class, IBqlTable, ICCPaymentMethodDetail, new()
	{
		PXSelectBase<T> profileDetailView;
		PXSelectBase<P> pmDetailView;
		public GenericCCPaymentProfileDetailAdapter(PXSelectBase<T> profileDetail, PXSelectBase<P> pmDetail)
		{
			this.profileDetailView = profileDetail;
			this.pmDetailView = pmDetail;
		}

		public ICCPaymentProfileDetail Current => profileDetailView.Current;

		public PXCache Cache => profileDetailView.Cache;

		public IEnumerable<Tuple<ICCPaymentProfileDetail,ICCPaymentMethodDetail>> Select(params object[] arguments)
		{
			PXResultset<T> resultSet = profileDetailView.Select(arguments);
			foreach (PXResult<T> item in resultSet)
			{
				var p1 = (T)item;
				var p2 = GetPMDetail(p1);
				yield return Tuple.Create<ICCPaymentProfileDetail,ICCPaymentMethodDetail>(p1,p2);
			}
		}

		private P GetPMDetail(T input)
		{
			P ret = pmDetailView.Select().ToList().Where(i => {
				P detail = i;
				return detail.PaymentMethodID == input.PaymentMethodID && detail.DetailID == input.DetailID;
			}).First();
			return ret;
		}
	}
}
