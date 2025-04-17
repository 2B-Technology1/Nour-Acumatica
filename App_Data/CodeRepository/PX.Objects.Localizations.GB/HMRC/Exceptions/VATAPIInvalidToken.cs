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

using PX.Common;
using System;
using System.Runtime.Serialization;
namespace PX.Objects.Localizations.GB.HMRC.Exceptions
{
	//  https://developer.service.hmrc.gov.uk/api-documentation/docs/reference-guide#http-status-codes
	//  https://developer.service.hmrc.gov.uk/api-documentation/docs/authorisation/user-restricted-endpoints

	/// <summary>
	/// Exceptions returned by VAT API when the token is invalid.
	/// </summary>
	public class VATAPIInvalidToken : Exception
	{
		#region constants
		public const string IMPOSSIBLE_TO_REFRESH_TOKEN = "IMPOSSIBLE_TO_REFRESH_TOKEN";
		public const string REFRESH_TOKEN_IS_INVALID = "REFRESH_TOKEN_IS_INVALID";
		public const string REFRESH_TOKEN_IS_MISSING = "REFRESH_TOKEN_IS_MISSING";
		public const string SERVER_ERROR = "SERVER_ERROR";
		#endregion

		public string Code { get; set; }

		public VATAPIInvalidToken() { }

		public VATAPIInvalidToken(string code, string message = null) : base(getMessageByCode(code, message))
		{
			Code = code;
		}

		protected VATAPIInvalidToken(SerializationInfo info, StreamingContext context) : base(info, context) { }

		private static string getMessageByCode(string code, string message = null)
		{
			if (String.IsNullOrEmpty(message))
			{
				switch (code)
				{
					case IMPOSSIBLE_TO_REFRESH_TOKEN: message = Messages.ImpossibleToRefreshToken; break;
					case REFRESH_TOKEN_IS_INVALID: message = Messages.RefreshTokenIsInvalid; break;
					case REFRESH_TOKEN_IS_MISSING: message = Messages.RefreshTokenIsMissing; break;
				}
			}
			return message;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			ReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}
	}
}
