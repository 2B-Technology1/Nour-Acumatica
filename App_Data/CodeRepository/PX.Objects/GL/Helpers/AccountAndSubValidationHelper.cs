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
using PX.Objects.Common;

namespace PX.Objects.GL.Helpers
{
	/// <summary>
	/// Class for instantiating validation helper objects.
	/// </summary>
	public sealed class AccountAndSubValidationHelper : AccountAndSubValidationHelper<AccountAndSubValidationHelper>
	{
		public AccountAndSubValidationHelper(PXCache cache, object row) : base(cache, row)
		{
		}
	}

	/// <summary>
	/// Validation helper that checks if Account or SubAccount are active
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class AccountAndSubValidationHelper<T> : ValidationHelper<T>
		where T : AccountAndSubValidationHelper<T>
	{
		public AccountAndSubValidationHelper(PXCache cache, object row) : base(cache, row)
		{
		}

		public static bool SetErrorIfInactiveAccount<TField>(PXCache cache, object row, object value)
			where TField : IBqlField
		{
			Account account = (Account)PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>
				.Select(cache.Graph, value);
			if (account?.Active == false)
			{
				cache.RaiseExceptionHandling<TField>(row, value,
					new PXSetPropertyException(GL.Messages.AccountInactive, PXErrorLevel.RowError, account.AccountCD));
				return false;
			}
			return true;
		}
		public static bool SetErrorIfInactiveSubAccount<TField>(PXCache cache, object row, object value)
			where TField : IBqlField
		{
			Sub sub = (Sub)PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>
				.Select(cache.Graph, value);
			if (sub?.Active == false)
			{
				cache.RaiseExceptionHandling<TField>(row, value,
					new PXSetPropertyException(GL.Messages.SubaccountInactive, PXErrorLevel.RowError, sub.SubCD));
				return false;
			}
			return true;
		}
		public T SetErrorIfInactiveAccount<TField>(object value)
			where TField : IBqlField
		{
			IsValid &= SetErrorIfInactiveAccount<TField>(Cache, Row, value);
			return (T)this;
		}

		public T SetErrorIfInactiveSubAccount<TField>(object value)
			where TField : IBqlField
		{
			IsValid &= SetErrorIfInactiveSubAccount<TField>(Cache, Row, value);
			return (T)this;
		}
	}
}
