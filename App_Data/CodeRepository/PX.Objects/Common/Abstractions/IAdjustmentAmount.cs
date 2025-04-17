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

namespace PX.Objects.Common
{
	/// <summary>
	/// An abstraction that represents the full set of 
	/// application amounts in base / adjusting document / 
	/// adjusted document currencies.
	/// </summary>
	public interface IAdjustmentAmount
	{
		decimal? CuryAdjgAmt
		{
			get;
			set;
		}
		decimal? CuryAdjgDiscAmt
		{
			get;
			set;
		}
		decimal? CuryAdjdAmt
		{
			get;
			set;
		}
		decimal? CuryAdjdDiscAmt
		{
			get;
			set;
		}
		decimal? AdjAmt
		{
			get;
			set;
		}
		decimal? AdjDiscAmt
		{
			get;
			set;
		}
		decimal? RGOLAmt
		{
			get;
			set;
		}
		bool? ReverseGainLoss
		{
			get;
		}
		/// <summary>
		/// Represents third amount (in adjusting document currency) that is used in balance calculation methods.
		/// In case of APAdjust it is <see cref="APAdjust.CuryAdjgWhTaxAmt"/>. 
		/// In case of ARAdjust it is <see cref="ARAdjust.CuryAdjgWOAmt"/> 
		/// </summary>
		decimal? CuryAdjgThirdAmount
		{
			get;
			set;
		}
		/// <summary>
		/// Represents third amount (in adjusted document currency) that is used in balance calculation methods.
		/// In case of APAdjust it is <see cref="APAdjust.CuryAdjdWhTaxAmt"/>.
		/// In case of ARAdjust it is <see cref="ARAdjust.CuryAdjdWOAmt"/>
		/// </summary>
		decimal? CuryAdjdThirdAmount
		{
			get;
			set;
		}
		/// <summary>
		/// Represents third amount (in base currency) that is used in balance calculation methods.
		/// In case of APAdjust it is <see cref="APAdjust.CuryAdjdWhTaxAmt"/>.
		/// In case of ARAdjust it is <see cref="ARAdjust.CuryAdjdWOAmt"/>
		/// </summary>
		decimal? AdjThirdAmount
		{
			get;
			set;
		}
	}
}
