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
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CA
{
	[PXHidden]
	public partial class VendorPaymentMethodDetailAlias : AP.VendorPaymentMethodDetail
	{
		#region BAccountID
		public abstract new class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion
		#region LocationID
		public abstract new class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion
		#region PaymentMethodID
		public abstract new class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		#endregion
		#region DetailID
		public abstract new class detailID : PX.Data.BQL.BqlString.Field<detailID> { }
		#endregion
	}
}
