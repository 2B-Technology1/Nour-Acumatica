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
	public class ARPriceWorksheetMaintASC606 : PXGraphExtension<ARPriceWorksheetMaint>
	{
		bool CustomerDiscountsFeatureEnabled = false;
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.aSC606>();
		}

		public override void Initialize()
		{
			CustomerDiscountsFeatureEnabled = PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>();
			PXUIFieldAttribute.SetVisible<ARPriceWorksheet.isFairValue>(Base.Document.Cache, null, true);
			PXUIFieldAttribute.SetVisible<ARPriceWorksheet.isProrated>(Base.Document.Cache, null, true);
			PXUIFieldAttribute.SetVisible<ARPriceWorksheet.discountable>(Base.Document.Cache, null, CustomerDiscountsFeatureEnabled);
		}

		protected virtual void _(Events.FieldVerifying<ARPriceWorksheet, ARPriceWorksheet.isPromotional> e)
		{
			if (e.Row?.IsFairValue == true && ((bool?)e.NewValue) == true)
			{
				throw new PXSetPropertyException(AR.Messages.PromotionalCannotBeFairValue, PXErrorLevel.Error);
			}
		}

		protected virtual void _(Events.FieldVerifying<ARPriceWorksheet, ARPriceWorksheet.isFairValue> e)
		{
			if ((bool?)e.NewValue == true)
			{
				foreach (ARPriceWorksheetDetail detail in Base.Details.Select())
				{
					if (detail.SkipLineDiscounts == true)
						throw new PXSetPropertyException(AR.Messages.SkipLineDiscAndFairValueCannotBeSelectedBoth, PXErrorLevel.Error);
				}
			}
		}
		protected virtual void _(Events.RowSelected<ARPriceWorksheet> e)
		{
			ARPriceWorksheet row = e.Row;
			if (row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<ARPriceWorksheet.skipLineDiscounts>(e.Cache, row, row.IsFairValue != true);
			PXUIFieldAttribute.SetEnabled<ARPriceWorksheet.isPromotional>(e.Cache, row, row.IsFairValue != true);
			PXUIFieldAttribute.SetEnabled<ARPriceWorksheet.isFairValue>(e.Cache, row, row.SkipLineDiscounts != true && row.IsPromotional != true);
		}

		protected virtual void _(Events.RowSelected<ARPriceWorksheetDetail> e)
		{
			ARPriceWorksheetDetail row = e.Row;
			if (row == null)
			{
				return;
			}
			var currentDocument = Base.Document.Current;
			if (currentDocument != null)
			{
				PXUIFieldAttribute.SetEnabled<ARPriceWorksheetDetail.skipLineDiscounts>(e.Cache, row, currentDocument.IsFairValue != true);
			}
		}
		protected virtual void _(Events.FieldUpdated<ARPriceWorksheet, ARPriceWorksheet.isFairValue> e)
		{
			if (((bool?)e.OldValue) == true && e.Row?.IsFairValue == false)
			{
				e.Row.IsProrated = false;
				e.Row.Discountable = false;
			}
		}

		protected virtual void _(Events.FieldUpdated<CopyPricesFilter, CopyPricesFilter.isFairValue> e)
		{
			if (((bool?)e.OldValue) == true && e.Row?.IsFairValue == false)
			{
				e.Row.IsProrated = false;
				e.Row.Discountable = false;
			}
		}

		protected virtual void _(Events.RowSelected<CopyPricesFilter> e)
		{
			CopyPricesFilter filter = e.Row;
			if (filter == null)
			{
				return;
			}

			PXUIFieldAttribute.SetVisible<CopyPricesFilter.isFairValue>(e.Cache, filter, true);
			PXUIFieldAttribute.SetVisible<CopyPricesFilter.isProrated>(e.Cache, filter, true);
			PXUIFieldAttribute.SetVisible<CopyPricesFilter.discountable>(e.Cache, filter, CustomerDiscountsFeatureEnabled);

			PXUIFieldAttribute.SetEnabled<CopyPricesFilter.isPromotional>(e.Cache, filter, filter.IsFairValue != true);
			PXUIFieldAttribute.SetEnabled<CopyPricesFilter.isProrated>(e.Cache, filter, filter.IsFairValue == true);
			PXUIFieldAttribute.SetEnabled<CopyPricesFilter.isFairValue>(e.Cache, filter, filter.IsPromotional != true);
			PXUIFieldAttribute.SetEnabled<CopyPricesFilter.discountable>(e.Cache, filter, filter.IsFairValue == true);
		}
	}
}
