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
using PX.Data;
using PX.Objects.CA;


namespace PX.Objects.GL
{
	[PXDBString(10, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, FieldClass = DimensionName)]
	public sealed class CashAccountRawAttribute : PXEntityAttribute
	{
		public const string DimensionName = "CASHACCOUNT";

		public CashAccountRawAttribute()
		{
			Type searchType = typeof(Search2<CashAccount.cashAccountCD,
								   InnerJoin<
											 Account, On<Account.accountID, Equal<CashAccount.accountID>,
											 And2<
												  Match<Account, Current<AccessInfo.userName>>,
											  And<Match<Account, Current<AccessInfo.branchID>>>>>,
								   InnerJoin<
											 Sub, On<Sub.subID, Equal<CashAccount.subID>,
											 And<Match<Sub, Current<AccessInfo.userName>>>>>>>);

			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, searchType, typeof(CashAccount.cashAccountCD))
			{
				CacheGlobal = true,
				DescriptionField = typeof(CashAccount.descr)
			};

			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}
}
