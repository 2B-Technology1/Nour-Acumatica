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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common
{
	public class ValueLabelList : IEnumerable<ValueLabelPair>
	{
		private readonly List<ValueLabelPair> _valueLabelPairs = new List<ValueLabelPair>();
		public ValueLabelList()
		{ }
		public ValueLabelList(IEnumerable<ValueLabelPair> valueLabelPairs)
		{
			_valueLabelPairs = valueLabelPairs.ToList();
		}
		public void Add(string value, string label)
		{
			_valueLabelPairs.Add(new ValueLabelPair(value, label));
		}
		public void Add(IEnumerable<ValueLabelPair> list)
		{
			_valueLabelPairs.AddRange(list);
		}
		public IEnumerator<ValueLabelPair> GetEnumerator() =>
			_valueLabelPairs.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			_valueLabelPairs.GetEnumerator();
	}
}
