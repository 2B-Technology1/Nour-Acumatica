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
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.IN.GraphExtensions.INRegisterEntryBaseExt;

namespace PX.Objects.IN.GraphExtensions.KitAssemblyEntryExt
{
	public class ComponentTranSplitPlan : INTranSplitPlanBase<KitAssemblyEntry, INKitRegister, INComponentTranSplit>
	{
		protected override void PrefetchDocumentPlansToCache()
		{
			PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<INKitRegister.noteID>>>>.Select(Base).Consume();
		}

		protected override IEnumerable<INComponentTranSplit> GetDocumentSplits()
		{
			return PXSelect<INComponentTranSplit,
				Where<INComponentTranSplit.docType, Equal<Current<INKitRegister.docType>>,
					And<INComponentTranSplit.refNbr, Equal<Current<INKitRegister.refNbr>>>>>
				.Select(Base)
				.RowCast<INComponentTranSplit>();
		}

		public override INItemPlan DefaultValues(INItemPlan planRow, INComponentTranSplit origRow)
		{
			planRow = base.DefaultValues(planRow, origRow);
			if (planRow == null)
				return null;

			INComponentTran parent = PXParentAttribute.SelectParent<INComponentTran>(ItemPlanSourceCache, origRow);
			planRow.ProjectID = parent.ProjectID;
			planRow.TaskID = parent.TaskID;
			planRow.CostCenterID = parent.CostCenterID;
			planRow.UOM = parent.UOM;
			planRow.BAccountID = parent.BAccountID;

			return planRow;
		}
	}
}
