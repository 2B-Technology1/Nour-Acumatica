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

namespace PX.Objects.SO
{
	public abstract class LinesSortingAttribute: PXEventSubscriberAttribute, IPXRowDeletedSubscriber
	{
		protected Type ParentType { get; }

		protected LinesSortingAttribute(Type parentType)
		{
			ParentType = parentType;
		}

		protected abstract bool AllowSorting(object parent);

		public virtual void RowDeleted(PXCache linesCache, PXRowDeletedEventArgs e)
		{
			var parent = PXParentAttribute.SelectParent(linesCache, e.Row, ParentType);

			if (!AllowSorting(parent))
				linesCache.SetValue(e.Row, FieldName, null);//disable sorting inside the PXOrderedSelectBase{Primary, Table}.RowDeleted
		}

		public static bool AllowSorting<TParent>(PXCache linesCache, TParent parent)
			where TParent: IBqlTable
		{
			if (parent == null)
				return false;

			foreach (var attr in linesCache.GetAttributesOfType<LinesSortingAttribute>(null, null))
				if(attr.ParentType == typeof(TParent))
					return attr.AllowSorting(parent);

			return false;
		}
	}

	public class SOInvoiceLinesSortingAttribute : LinesSortingAttribute
	{
		public SOInvoiceLinesSortingAttribute() : base(typeof(SOInvoice)) { }

		protected override bool AllowSorting(object parent)
		{
			var invoice = (SOInvoice)parent;
			return invoice?.SOOrderType != null && invoice?.SOOrderNbr != null;
		}
	}
}
