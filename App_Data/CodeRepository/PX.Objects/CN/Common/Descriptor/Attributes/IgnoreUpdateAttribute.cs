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
using System.Linq;
using PX.Data;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.Common.Descriptor.Attributes
{
	public class IgnoreUpdateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber
	{
		private readonly Type[] graphTypes;

		public IgnoreUpdateAttribute(params Type[] graphTypes)
		{
			this.graphTypes = graphTypes;
		}

		public void FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs args)
		{
			if (IsGraphApplicable(cache.Graph) && !IsAnyFieldUpdated(cache, args.Row))
			{
				cache.SetStatus(args.Row, PXEntryStatus.Notchanged);
			}
		}

		private bool IsGraphApplicable(PXGraph graph)
		{
			var graphType = graph.GetType();
			return graphTypes.IsEmpty() || graphTypes.Contains(graphType);
		}

		private bool IsAnyFieldUpdated(PXCache cache, object newRow)
		{
			var original = cache.GetOriginal(newRow);
			foreach (var fieldType in cache.BqlFields)
			{
				var fieldName = cache.GetField(fieldType);
				var oldValue = cache.GetValue(original, fieldName)?.ToString();
				var newValue = cache.GetValue(newRow, fieldName)?.ToString();
				if (fieldName != _FieldName && oldValue != newValue)
				{
					return true;
				}
			}
			return false;
		}
	}
}