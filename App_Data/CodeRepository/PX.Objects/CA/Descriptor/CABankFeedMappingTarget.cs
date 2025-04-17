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
using System.Collections.Generic;

namespace PX.Objects.CA
{
	public class CABankFeedMappingTarget
	{
		public const string ExtRefNbr = "ExtRefNbr";
		public const string TranDesc = "TranDesc";
		public const string UserDesc = "UserDesc";
		public const string CardNumber = "CardNumber";
		public const string InvoiceNbr = "InvoiceNbr";
		public const string PayeeName = "PayeeName";
		public const string TranCode = "TranCode";
		public const string PaymentMethod = "PaymentMethod";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(GetTargets())
			{ }

			public static (string, string)[] GetTargets()
			{
				return new List<(string, string)>()
				{
					(ExtRefNbr, Messages.ExtRefNbr),
					(TranDesc, Messages.TranDesc),
					(UserDesc, Messages.UserDesc),
					(CardNumber, Messages.CardNumber),
					(InvoiceNbr, Messages.InvoiceNbr),
					(PayeeName, Messages.PayeeName),
					(TranCode, Messages.TranCode),
					(PaymentMethod, Messages.PaymentMethod)
				}.ToArray();
			}
		}
	}
}
