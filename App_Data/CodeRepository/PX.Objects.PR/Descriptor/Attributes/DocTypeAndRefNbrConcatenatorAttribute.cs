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

namespace PX.Objects.PR
{
	public class DocTypeAndRefNbrDisplayNameAttribute : PXStringAttribute, IPXFieldDefaultingSubscriber
	{
		private readonly Type _DocTypeField;
		private readonly Type _RefNbrField;

		public DocTypeAndRefNbrDisplayNameAttribute(Type docTypeField, Type refNbrField) : base(50)
		{
			_DocTypeField = docTypeField;
			_RefNbrField = refNbrField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.RowPersisted.AddHandler(sender.GetItemType(),
				(cache, e) =>
				{
					if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
					{
						cache.SetValue(e.Row, _FieldName, GetDocAndRef(cache, e.Row));
					}
				});
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			if (e.Row == null)
				return;

			e.ReturnValue = GetDocAndRef(sender, e.Row);
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null)
				return;

			e.NewValue = GetDocAndRef(sender, e.Row);
		}

		private string GetDocAndRef(PXCache sender, object row)
		{
			string docType = (string)sender.GetValue(row, _DocTypeField.Name);
			string refNbr = (string)sender.GetValue(row, _RefNbrField.Name);

			if (string.IsNullOrWhiteSpace(docType) || string.IsNullOrWhiteSpace(refNbr))
				return string.Empty;

			return $"{docType},{refNbr}";
		}
	}
}
