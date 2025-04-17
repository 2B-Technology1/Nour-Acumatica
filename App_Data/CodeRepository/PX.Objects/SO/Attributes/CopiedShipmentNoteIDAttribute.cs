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
using PX.Objects.Common.Attributes;
using System;

namespace PX.Objects.SO.Attributes
{
	public class CopiedShipmentNoteIDAttribute : CopiedNoteIDAttribute
	{
		public CopiedShipmentNoteIDAttribute() : base(entityType: null)
		{
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1 + " The constructor is obsolete. Use the parameterless constructor instead. " +
				  "The " + nameof(CopiedShipmentNoteIDAttribute) + " constructor is exactly the same as the parameterless one. " +
				  "It does not provide any additional functionality and does not save values of provided fields in the note. " +
				  "The constructor will be removed in a future version of Acumatica ERP.")]
		public CopiedShipmentNoteIDAttribute(params Type[] searches)
			: base(null, searches)
		{
		}

		protected override string GetEntityType(PXCache cache, Guid? noteId)
		{
			var cmd = new PXSelect<SOOrderShipment,
				Where<SOOrderShipment.shippingRefNoteID, Equal<Required<Note.noteID>>>>(cache.Graph);
			SOOrderShipment orderShipment = cmd.Select(noteId);
			if (orderShipment != null)
			{
				ShippingRefNoteAttribute.GetTargetTypeAndKeys(cmd.Cache, orderShipment, out Type targetType, out object[] targetKeys);
				if (targetType != null)
					return targetType.FullName;
			}
			return cache.GetItemType().FullName;
		}
	}
}
