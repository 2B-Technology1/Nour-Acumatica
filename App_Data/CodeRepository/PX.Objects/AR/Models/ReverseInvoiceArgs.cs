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
using PX.Objects.CM.Extensions;

namespace PX.Objects.AR
{
	/// <exclude/>
	public class ReverseInvoiceArgs
	{
		public enum CopyOption
		{
			SetOriginal,
			SetDefault,
			Override,
		}

		public bool ApplyToOriginalDocument { get; set; }
		public bool PreserveOriginalDocumentSign { get; set; }
		public bool? OverrideDocumentHold { get; set; }
		public string OverrideDocumentDescr { get; set; }
		public bool? ReverseINTransaction { get; set; }

		public CopyOption DateOption { get; set; } = CopyOption.SetOriginal;
		public DateTime? DocumentDate { get; set; }
		public string DocumentFinPeriodID { get; set; }

		public CopyOption CurrencyRateOption { get; set; } = CopyOption.SetOriginal;
		public CurrencyInfo CurrencyRate { get; set; }
	}
}
