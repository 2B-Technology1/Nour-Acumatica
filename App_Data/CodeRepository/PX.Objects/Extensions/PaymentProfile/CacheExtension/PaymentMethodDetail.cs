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
	public class PaymentMethodDetail : PXMappedCacheExtension, ICCPaymentMethodDetail
	{
		public abstract class paymentMethodID : IBqlField { }
		public virtual string PaymentMethodID { get; set; }
		public abstract class useFor : IBqlField { }
		public virtual string UseFor { get; set; }
		public abstract class detailID : IBqlField { }
		public virtual string DetailID { get; set; }
		public abstract class descr : IBqlField { }
		public virtual string Descr { get; set; }
		public abstract class isEncrypted : IBqlField { }
		public virtual bool? IsEncrypted { get; set; }
		public abstract class isRequired : IBqlField { }
		public virtual bool? IsRequired{ get;set; }
		public abstract class isIdentifier : IBqlField { }
		public virtual bool? IsIdentifier { get; set; }
		public abstract class isExpirationDate : IBqlField { }
		public virtual bool? IsExpirationDate { get; set; }
		public abstract class isOwnerName : IBqlField { }
		public virtual bool? IsOwnerName { get; set; }
		public abstract class isCCProcessingID : IBqlField { }
		public virtual bool? IsCCProcessingID { get; set; }
		public abstract class isCVV : PX.Data.IBqlField { }
		public virtual bool? IsCVV { get; set; }
	}
}
