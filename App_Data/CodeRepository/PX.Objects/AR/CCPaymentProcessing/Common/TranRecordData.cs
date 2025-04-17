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

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public class TranRecordData
	{
		public string ExternalTranId { get; set; }

		public string RefExternalTranId { get; set; }

		public string ProcessingCenterId { get; set; }

		public int? RefInnerTranId { get; set; }
		
		public string AuthCode { get; set; }

		public string TranStatus { get; set; }

		public decimal? Amount { get; set; }

		public string ResponseCode { get; set; }

		public string ResponseText { get; set; }

		public DateTime? ExpirationDate { get; set; }

		public DateTime? TransactionDate { get; set; }

		public string CvvVerificationCode { get; set; }

		public string ExtProfileId { get; set; } 

		public bool CreateProfile { get; set; }

		public bool NeedSync { get; set; }

		public bool Imported { get; set; }

		public bool NewDoc { get; set; }

		public bool AllowFillVoidRef { get; set; }

		public bool KeepNewTranDeactivated { get; set; }
		/// <summary>The <see cref="ExternalTransaction.TransactionID" /> identifier after recording operation.</summary>
		public int? InnerTranId { get; set; }
		public string PayLinkExternalId { get; set; }

		public bool ValidateDoc { get; set; } = true;

		public Guid? TranUID { get; set; }
		public CCCardType CardType { get; set; }
		public string ProcCenterCardTypeCode { get; set; }
		public bool IsLocalValidation { get; set; }
		public decimal? Tax { get; set; }
		public decimal? Subtotal { get; set; }
	}
}
