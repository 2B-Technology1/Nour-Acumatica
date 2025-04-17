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

using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL;


namespace PX.Objects.CA
{
	/// <summary>
	/// Common interface for CA transactions like <see cref="CASplit"/> and <see cref="CABankTranDetail"/>.
	/// </summary>
	internal interface ICATranDetail : IBqlTable
	{
		/// <summary>
		/// Gets or sets the identifier of the branch.
		/// </summary>
		int? BranchID
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the identifier of the account.
		/// </summary>
		int? AccountID
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the identifier of the sub account.
		/// </summary>
		int? SubID
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the identifier of the cash account.
		/// </summary>
		int? CashAccountID
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets information describing the transaction.
		/// </summary>
		string TranDesc
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the identifier of the currency information.
		/// </summary>
		long? CuryInfoID
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the transaction amount specified in currency.
		/// </summary>
		decimal? CuryTranAmt
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the transaction amount.
		/// </summary>
		decimal? TranAmt
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the quantity.
		/// </summary>
		decimal? Qty
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the unit price.
		/// </summary>
		decimal? UnitPrice
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the unit price specified in currency.
		/// </summary>
		decimal? CuryUnitPrice
		{
			get;
			set;
		}
	}
}
