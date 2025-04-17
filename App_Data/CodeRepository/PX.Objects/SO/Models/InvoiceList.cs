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
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO
{
	public class InvoiceList : DocumentList<ARInvoice, SOInvoice>
	{
		private Type[] _typesArray;
		protected Type[] TypesArray
		{
			get { return _typesArray ?? (_typesArray = new Type[] { typeof(ARInvoice), typeof(SOInvoice), typeof(CurrencyInfo) }); }
		}

		public InvoiceList(PXGraph graph) : base(graph)
		{
		}

		public virtual void Add(ARInvoice item0, SOInvoice item1, CurrencyInfo curyInfo)
		{
			this.Add(new PXResult<ARInvoice, SOInvoice, CurrencyInfo>(item0, item1, curyInfo));
		}

		public override PXResult<ARInvoice, SOInvoice> Find(object item)
		{
			Type tableType = item.GetType();
			Type appropriateType = TypesArray.FirstOrDefault(t => t.IsAssignableFrom(tableType));
			if (appropriateType != null)
			{
				PXCache cache = _Graph.Caches[appropriateType];
				return this.Find(data => cache.ObjectsEqual(data[appropriateType], item));
			}

			return null;
		}

		protected override object GetValue(object data, Type field)
		{
			Type tableType = BqlCommand.GetItemType(field);
			Type appropriateType = TypesArray.FirstOrDefault(t => t.IsAssignableFrom(tableType));

			return _Graph.Caches[appropriateType].GetValue(((PXResult)data)[tableType], field.Name);
		}
	}

	[PXInternalUseOnly]
	public class ShipmentInvoices : InvoiceList
	{
		private readonly Dictionary<string, List<int>> typedInvoices;

		public ShipmentInvoices(PXGraph graph) : base(graph)
		{
			typedInvoices = new Dictionary<string, List<int>>();
		}

		public override void Add(ARInvoice item0, SOInvoice item1, CurrencyInfo curyInfo)
		{
			base.Add(item0, item1, curyInfo);
			List<int> indexes;
			if (!typedInvoices.TryGetValue(item0.DocType, out indexes))
				typedInvoices.Add(item0.DocType, indexes = new List<int>());
			indexes.Add(Count - 1);
		}

		public IEnumerable<PXResult<ARInvoice, SOInvoice>> GetInvoices(string docType)
		{
			List<int> indexes;
			if (typedInvoices.TryGetValue(docType, out indexes))
			{
				foreach (var index in indexes)
					yield return this[index];
			}
		}
	}
}
