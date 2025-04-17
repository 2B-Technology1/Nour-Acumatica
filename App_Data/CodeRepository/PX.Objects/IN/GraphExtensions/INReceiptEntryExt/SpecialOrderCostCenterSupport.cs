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
using PX.Objects.GL;

namespace PX.Objects.IN.GraphExtensions.INReceiptEntryExt
{
	public class SpecialOrderCostCenterSupport : INRegisterEntryBaseExt.SpecialOrderCostCenterSupport<INReceiptEntry>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<CS.FeaturesSet.specialOrders>();

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(SpecialOrderCostCenterSelectorAttribute),
			nameof(SpecialOrderCostCenterSelectorAttribute.AllowEnabled), false)]
		protected virtual void _(Events.CacheAttached<INTran.specialOrderCostCenterID> e) { }


		protected virtual void _(Events.RowSelected<INRegister> e)
		{
			if (e.Row == null)
				return;

			bool inModule = (e.Row.OrigModule == BatchModule.IN);
			bool transfer = (e.Row.TransferNbr != null);

			Base.transactions.Cache.AdjustUI()
				.For<INTran.specialOrderCostCenterID>(a => a.Visible = !inModule || transfer);
		}

		protected override void _(Events.RowSelected<INTran> e)
		{
			base._(e);

			if (e.Row == null)
				return;

			var doc = Base.CurrentDocument.Current;
			bool inModule = (doc?.OrigModule == BatchModule.IN);
			bool released = (doc?.Released == true);

			e.Cache.Adjust<CostLayerType.ListAttribute>(e.Row)
				.For<INTran.costLayerType>(a => a.AllowSpecialOrders = !inModule);

			var numberOfValues = e.Cache.GetAttributes<INTran.costLayerType>(e.Row).OfType<CostLayerType.ListAttribute>()
				.FirstOrDefault()?.SetValues(e.Cache, e.Row);

			e.Cache.AdjustUI(e.Row)
				.For<INTran.costLayerType>(a => a.Enabled = (numberOfValues > 1 && !released && inModule));
		}
	}
}
