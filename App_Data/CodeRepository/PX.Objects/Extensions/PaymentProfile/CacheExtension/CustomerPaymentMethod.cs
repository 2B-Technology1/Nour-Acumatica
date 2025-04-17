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
namespace PX.Objects.Extensions.PaymentProfile
{
	public class CustomerPaymentMethod : PXMappedCacheExtension, ICCPaymentProfile
	{
		public abstract class bAccountID : PX.Data.IBqlField { }
		public virtual int? BAccountID { get; set; }
		public abstract class pMInstanceID : PX.Data.IBqlField { }
		public virtual int? PMInstanceID { get; set; }
		public abstract class cCProcessingCenterID : PX.Data.IBqlField { }
		public virtual string CCProcessingCenterID { get; set; }
		public abstract class customerCCPID : PX.Data.IBqlField { }
		public virtual string CustomerCCPID { get; set; }
		public abstract class paymentMethodID : PX.Data.IBqlField { }
		public virtual string PaymentMethodID { get; set; }
		public abstract class cashAccountID : PX.Data.IBqlField { }
		public virtual int? CashAccountID { get; set; }
		public abstract class descr : PX.Data.IBqlField {  }
		public virtual string Descr { get; set; }
		public abstract class expirationDate : PX.Data.IBqlField { }
		public virtual DateTime? ExpirationDate { get; set; }
		public string CardType { get; set; }
		public string ProcCenterCardTypeCode { get; set; }
	}
}
