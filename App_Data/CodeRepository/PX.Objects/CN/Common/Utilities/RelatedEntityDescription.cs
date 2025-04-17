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
using PX.Data;
using PX.Objects.PO;

namespace PX.Objects.CN.Common.Utilities
{
	public class RelatedEntityDescription<TRefNoteId> : BqlFormulaEvaluator<TRefNoteId>
		where TRefNoteId : IBqlField
	{
		public const string RelatedEntityDescriptionForSubcontract = "Subcontract, {0}";

		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> entityTypes)
		{
			var refNoteId = (Guid?)entityTypes[typeof(TRefNoteId)];
			var entityHelper = new EntityHelper(cache.Graph);
			var commitment = entityHelper.GetEntityRow(refNoteId) as POOrder;
			var entityDescription = entityHelper.GetEntityDescription(refNoteId, item.GetType());
			return commitment?.OrderType == POOrderType.RegularSubcontract
				? string.Format(RelatedEntityDescriptionForSubcontract, commitment.OrderNbr)
				: entityDescription;
		}
	}
}
