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
using System;

namespace PX.Objects.CA
{
	public class CABankFeedMatchRule
	{
		public const string Empty = "N";
		public const string StartsWith = "S";
		public const string Contains = "C";
		public const string EndsWith = "E";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute(bool allowEmptyValue)
			{
				const string emptyLabel = " ";
				string[] values = new string[] { StartsWith, Contains, EndsWith };
				string[] labels = new string[] { Messages.StartsWith, Messages.Contains, Messages.EndsWith };
				string[] newValues = null;
				string[] newLabels = null;

				if (allowEmptyValue)
				{
					newValues = new string[4];
					newValues[0] = Empty;
					newLabels = new string[4];
					newLabels[0] = emptyLabel;
					Array.Copy(values, 0, newValues, 1, 3);
					Array.Copy(labels, 0, newLabels, 1, 3);
				}

				if (newValues != null)
				{
					_AllowedValues = newValues;
					_AllowedLabels = newLabels;
				}
				else
				{
					_AllowedValues = values;
					_AllowedLabels = labels;
				}

				_NeutralAllowedLabels = _AllowedLabels;
			}
		}
	}
}
