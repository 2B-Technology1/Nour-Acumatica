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

namespace PX.Objects.CA
{
	public class CABankFeedMatchField
	{
		public enum SetOfValues
		{
			CorporateCard,
			ExpenseReceipts
		}

		public const string Empty = "N";
		public const string AccountOwner = "O";
		public const string Category = "C";
		public const string Name = "A";
		public const string CheckNumber = "C";
		public const string Memo = "M";
		public const string PartnerAccountId = "P";
		public const string EmptyLabel = " ";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute(SetOfValues setOfValues)
			{
				if (setOfValues == SetOfValues.CorporateCard)
				{
					_AllowedValues = new string[] { Empty, AccountOwner, Name };
					_AllowedLabels = new string[] { EmptyLabel, Messages.AccountOwner, Messages.Name };
				}
				if (setOfValues == SetOfValues.ExpenseReceipts)
				{
					_AllowedValues = new string[] { Category, Name };
					_AllowedLabels = new string[] { Messages.Category, Messages.Name };
				}
				_NeutralAllowedLabels = _AllowedLabels;
			}
		}
	}
}
