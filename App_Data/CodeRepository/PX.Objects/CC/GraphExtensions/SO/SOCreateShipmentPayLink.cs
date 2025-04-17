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
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.Extensions.PayLink;
using PX.Objects.SO;
using System;

namespace PX.Objects.CC.GraphExtensions
{
	public class SOCreateShipmentPayLink : PXGraphExtension<SOCreateShipment>
	{
		public const string CreateLinkAction = "SO301000$" + nameof(PayLinkDocumentGraph<SOOrderEntry, SOOrder>.createLink);

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.acumaticaPayments>();
		}

		[PXOverride]
		public PXSelectBase<SOOrder> GetSelectCommand(SOOrderFilter filter, Func<SOOrderFilter, PXSelectBase<SOOrder>> baseMethod)
		{
			if (filter.Action == CreateLinkAction)
			{
				return BuildCommandCreateLink();
			}
			return baseMethod(filter);
		}

		protected virtual PXSelectBase<SOOrder> BuildCommandCreateLink()
		{
			return new SelectFrom<SOOrder>
				.InnerJoin<SOOrderType>.On<SOOrder.FK.OrderType>
				.LeftJoin<Carrier>.On<SOOrder.FK.Carrier>
				.LeftJoin<Customer>.On<SOOrder.FK.Customer>.SingleTableOnly
				.LeftJoin<CustomerClass>.On<Customer.FK.CustomerClass>
				.LeftJoin<CCPayLink>.On<CCPayLink.payLinkID.IsEqual<SOOrderPayLink.payLinkID>>
				.Where<SOOrderType.canHavePayments.IsEqual<True>
				.And<SOOrderPayLink.processingCenterID.IsNotNull>
				.And<SOOrder.cancelled.IsEqual<False>>
				.And<SOOrder.completed.IsEqual<False>>
				.And<SOOrder.isExpired.IsEqual<False>>
				.And<SOOrder.curyUnpaidBalance.IsGreater<decimal0>>
				.And<Where<SOOrder.behavior.IsNotIn<SOBehavior.iN, SOBehavior.mO>.Or<SOOrder.billedCntr.IsEqual<Zero>>>>
				.And<Where<SOOrder.approved.IsEqual<True>.Or<SOOrder.hold.IsEqual<True>>>>
				.And<Where<SOOrderPayLink.payLinkID.IsNull.Or<CCPayLink.linkStatus.IsIn<PayLinkStatus.none, PayLinkStatus.closed>>>>>
				.View(Base);
		}
	}
}
