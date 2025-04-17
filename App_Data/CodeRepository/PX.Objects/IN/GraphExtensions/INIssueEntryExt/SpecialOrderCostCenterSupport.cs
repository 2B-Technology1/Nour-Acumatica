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
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.IN.GraphExtensions.INIssueEntryExt
{
	public class SpecialOrderCostCenterSupport : INRegisterEntryBaseExt.SpecialOrderCostCenterSupport<INIssueEntry>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<CS.FeaturesSet.specialOrders>();

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[SpecialOrderCostCenterSelector(typeof(INTran.inventoryID), typeof(INTran.siteID), typeof(INTran.invtMult),
			SOOrderTypeField = typeof(INTran.sOOrderType), SOOrderNbrField = typeof(INTran.sOOrderNbr),
			SOOrderLineNbrField = typeof(INTran.sOOrderLineNbr), IsSpecialOrderField = typeof(INTran.isSpecialOrder),
			CostCenterIDField = typeof(INTran.costCenterID), CostLayerTypeField = typeof(INTran.costLayerType),
			OrigModuleField = typeof(INTran.origModule), ReleasedField = typeof(INTran.released),
			ProjectIDField = typeof(INTran.projectID), TaskIDField = typeof(INTran.taskID), CostCodeIDField = typeof(INTran.costCodeID))]
		protected virtual void _(Events.CacheAttached<INTran.specialOrderCostCenterID> e) { }

		protected override void _(Events.RowSelected<INTran> e)
		{
			base._(e);

			if (e.Row == null)
				return;

			if (Base.CurrentDocument.Current?.OrigModule == BatchModule.IN && Base.CurrentDocument.Current.Released != true)
			{
				e.Cache.AdjustUI().For<INTran.uOM>(a => a.Enabled = e.Row.CostLayerType != CostLayerType.Special);
			}
		}

		protected virtual void _(Events.FieldVerifying<INTran, INTran.tranType> e)
			=> VerifyTranType(e.Row?.CostLayerType, e.NewValue as string, e.Row);

		protected virtual void _(Events.FieldVerifying<INTran, INTran.costLayerType> e)
			=> VerifyTranType(e.NewValue as string, e.Row?.TranType, e.Row);

		protected virtual void VerifyTranType(string costLayerType, string tranType, INTran row)
		{
			if (costLayerType == CostLayerType.Special && tranType != INTranType.Issue && row.OrigModule == BatchModule.IN)
			{
				throw new PXSetPropertyException(Messages.SpecialCostLayerIssueType);
			}
		}
	}
}
