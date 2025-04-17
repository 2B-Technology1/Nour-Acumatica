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
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.DR
{
	public class ARSalesPriceMaintASC606 : PXGraphExtension<ARSalesPriceMaint>
	{
		bool CustomerDiscountsFeatureEnabled = false;
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.aSC606>();
		}

		public override void Initialize()
		{
			CustomerDiscountsFeatureEnabled = PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>();

			PXUIFieldAttribute.SetVisible<ARSalesPrice.isFairValue>(Base.Records.Cache, null, true);
			PXUIFieldAttribute.SetVisible<ARSalesPrice.isProrated>(Base.Records.Cache, null, true);
			PXUIFieldAttribute.SetVisibility<ARSalesPrice.isFairValue>(Base.Records.Cache, null, PXUIVisibility.SelectorVisible);
			PXUIFieldAttribute.SetVisibility<ARSalesPrice.isProrated>(Base.Records.Cache, null, PXUIVisibility.SelectorVisible);
			PXUIFieldAttribute.SetVisible<ARSalesPrice.discountable>(Base.Records.Cache, null, CustomerDiscountsFeatureEnabled);
			PXUIFieldAttribute.SetVisibility<ARSalesPrice.discountable>(Base.Records.Cache, null, PXUIVisibility.SelectorVisible);
		}

		protected virtual void _(Events.RowSelected<ARSalesPrice> e)
		{
			if (e.Row == null)
				return;

			PXUIFieldAttribute.SetEnabled<ARSalesPrice.isPromotionalPrice>(e.Cache, e.Row, e.Row.IsFairValue != true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.skipLineDiscounts>(e.Cache, e.Row, e.Row.IsFairValue != true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.isFairValue>(e.Cache, e.Row, e.Row.IsPromotionalPrice != true && e.Row.SkipLineDiscounts != true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.isProrated>(e.Cache, e.Row, e.Row.IsFairValue == true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.discountable>(e.Cache, e.Row, e.Row.IsFairValue == true);

		}

		protected virtual void _(Events.FieldUpdated<ARSalesPrice, ARSalesPrice.isFairValue> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.IsPromotionalPrice == false && (bool)e.OldValue == true)
			{
				e.Row.IsProrated = false;
			}
		}
	}
}
