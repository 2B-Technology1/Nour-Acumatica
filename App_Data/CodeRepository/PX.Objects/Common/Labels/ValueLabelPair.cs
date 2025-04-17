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

namespace PX.Objects.Common
{
	public struct ValueLabelPair
	{
		public string Value { get; private set; }
		public string Label { get; private set; }

		/// <summary>
		/// Initializes a new instance of <see cref="ValueLabelPair"/> using a
		/// value and its corresponding label.
		/// </summary>
		public ValueLabelPair(string value, string label)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (label == null) throw new ArgumentNullException(nameof(label));

			this.Value = value;
			this.Label = label;
		}

		public class KeyComparer : IEqualityComparer<ValueLabelPair>
		{
			public bool Equals(ValueLabelPair x, ValueLabelPair y)
			{
				return string.Equals(x.Value, y.Value);
			}

			public int GetHashCode(ValueLabelPair valueLabelPair)
			{
				return valueLabelPair.Value.GetHashCode();
			}
		}
	}
}
