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
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Data;
namespace PX.Objects.Extensions.PaymentTransaction
{
	public class Payment : PXMappedCacheExtension, ICCPayment
	{
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		public int? PMInstanceID { get; set; }
		public string PaymentMethodID { get; set; }
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }
		public string ProcessingCenterID { get; set; }
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		public int? CashAccountID { get; set; }
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		public decimal? CuryDocBal { get; set; }
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		public string CuryID { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		public string DocType { get; set; }
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		public string RefNbr { get; set; }
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		public string OrigDocType { get; set; }
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		public string OrigRefNbr { get; set; }
		public abstract class refTranExtNbr : PX.Data.BQL.BqlString.Field<refTranExtNbr> { }
		public string RefTranExtNbr { get; set; }
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		public bool? Released { get; set; }
		public abstract class saveCard : PX.Data.BQL.BqlBool.Field<saveCard> { }
		public bool? SaveCard { get; set; }
		public abstract class cCTransactionRefund : PX.Data.BQL.BqlBool.Field<cCTransactionRefund> { }
		public bool? CCTransactionRefund { get; set; }
		public abstract class cCPaymentStateDescr : PX.Data.BQL.BqlString.Field<cCPaymentStateDescr> { }
		public string CCPaymentStateDescr { get; set; }
		public abstract class cCActualExternalTransactionID : PX.Data.BQL.BqlInt.Field<cCActualExternalTransactionID> { }
		public int? CCActualExternalTransactionID { get; set; }
		public abstract class subtotalAmount : PX.Data.BQL.BqlDecimal.Field<subtotalAmount> { }
		/// <summary>Amount before tax.</summary>
		public decimal? SubtotalAmount { get; set; }
		public abstract class tax : PX.Data.BQL.BqlDecimal.Field<tax> { }
		/// <summary>Total tax amount.</summary>
		public decimal? Tax { get; set; }
	}
}
