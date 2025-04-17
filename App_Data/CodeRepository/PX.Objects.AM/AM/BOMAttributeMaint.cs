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
using PX.Objects.CS;

namespace PX.Objects.AM
{
	/// <summary>
	/// BOM Attribute Maintenance graph
	/// Main graph for managing BOM Attributes
	/// </summary>
	public class BOMAttributeMaint : BOMInquiryGraph<BOMAttributeMaint>
	{
		//Primary view "Documents" comes from PXRevisionablegraph

		[PXImport(typeof(AMBomItem))]
		public PXSelect<AMBomAttribute,
					Where<AMBomAttribute.bOMID, Equal<Current<AMBomItem.bOMID>>,
						And<AMBomAttribute.revisionID, Equal<Current<AMBomItem.revisionID>>>>> BomAttributes;

		protected override void _(Events.RowSelected<AMBomItem> e)
		{
			base._(e);

			var onHold = e.Row?.Hold == true;
			BomAttributes.AllowDelete =
				BomAttributes.AllowUpdate =
					BomAttributes.AllowInsert = onHold;
		}

		protected virtual void _(Events.FieldUpdated<AMBomAttribute, AMBomAttribute.attributeID> e)
		{
			if (e.Row == null)
			{
				return;
			}

			var item = (CSAttribute)PXSelectorAttribute.Select<AMBomAttribute.attributeID>(e.Cache, e.Row);
			if (item == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(e.Row.Label))
			{
				e.Cache.SetValueExt<AMBomAttribute.label>(e.Row, item.AttributeID);
			}
			if (string.IsNullOrWhiteSpace(e.Row.Descr))
			{
				e.Cache.SetValueExt<AMBomAttribute.descr>(e.Row, item.Description);
			}
		}
	}
}
