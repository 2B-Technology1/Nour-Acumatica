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
using PX.Objects.AR;
using PX.Data;

namespace PX.Objects.Extensions.PaymentTransaction
{
	public class TranTypeList : PXStringListAttribute
	{
		public const string AUTCode = "AUT";
		public const string AACCode = "AAC";
		public const string PACCode = "PAC";
		public const string CDTCode = "CDT";
		public const string VDGCode = "VDG";
		public const string UKNCode = "UKN";

		const string AUTTypeName = CCTranTypeCode.AUTLabel;
		const string AACTypeName = CCTranTypeCode.AACLabel;
		const string PACTypeName = CCTranTypeCode.PACLabel;
		const string CDTTypeName = CCTranTypeCode.CDTLabel;
		const string VDGTypeName = CCTranTypeCode.VDGLabel;
		const string UKNTypeName = CCTranTypeCode.UKNLabel;

		public TranTypeList() : base(GetCommonInputTypes())
		{

		}

		public static Tuple<string, string>[] GetCommonInputTypes()
		{
			return new[] {
				Pair(AUTCode, AUTTypeName),
				Pair(AACCode, AACTypeName),
				Pair(PACCode, PACTypeName),
				Pair(CDTCode, CDTTypeName),
				Pair(VDGCode, VDGTypeName),
				Pair(UKNCode, UKNTypeName)
			};
		}

		public static Tuple<string, string>[] GetCreditInputType()
		{
			return new[] { Pair(CDTCode, CDTTypeName) };
		}

		public static string GetStrCodeByTranType(CCTranType tranType)
		{
			string ret = null;
			bool found = false;
			foreach (var item in mapping)
			{
				if (item.Item1 == tranType)
				{
					ret = item.Item2;
					found = true;
					break;
				}
			}

			if (!found)
			{
				throw new PXInvalidOperationException();
			}
			return ret;
		}

		public static CCTranType GetTranTypeByStrCode(string strCode)
		{
			bool found = false;
			CCTranType val = CCTranType.AuthorizeAndCapture;
			foreach (var item in mapping)
			{
				if (item.Item2 == strCode)
				{
					val = item.Item1;
					found = true;
					break;
				}
			}

			if (!found)
			{
				throw new PXInvalidOperationException();
			}
			return val;
		}

		private static (CCTranType, string)[] mapping = {
				(CCTranType.AuthorizeAndCapture, AACCode), (CCTranType.AuthorizeOnly, AUTCode),
				(CCTranType.PriorAuthorizedCapture, PACCode), (CCTranType.Credit, CDTCode),
				(CCTranType.Void, VDGCode), (CCTranType.Unknown, UKNCode)
			};
	}
}
