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
using PX.Data;

namespace PX.Objects.CN.Common.Descriptor.Attributes
{
	public class DependsOnFieldAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber
	{
		public bool ShouldDisable = true;
		private readonly Type fieldType;

		public DependsOnFieldAttribute(Type fieldType)
		{
			this.fieldType = fieldType;
		}

		public override void CacheAttached(PXCache cache)
		{
			cache.Graph.RowUpdated.AddHandler(BqlTable, RowUpdated);
		}

		public void RowSelected(PXCache cache, PXRowSelectedEventArgs args)
		{
			if (args.Row != null && ShouldDisable)
			{
				var field = cache.GetField(fieldType);
				var fieldValue = cache.GetValue(args.Row, field);
				PXUIFieldAttribute.SetEnabled(cache, args.Row, FieldName, fieldValue != null);
			}
		}

		private void RowUpdated(PXCache cache, PXRowUpdatedEventArgs args)
		{
			var field = cache.GetField(fieldType);
			var oldValue = cache.GetValue(args.OldRow, field);
			var newValue = cache.GetValue(args.Row, field);
			if (!Equals(oldValue, newValue))
			{
				cache.SetValue(args.Row, FieldName, null);
				cache.RaiseExceptionHandling(FieldName, args.Row, null, null);
			}
		}
	}
}