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

using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.SO;
using System.Linq;

namespace PX.Commerce.BigCommerce
{
	public class OrderValidator : BCBaseValidator, ISettingsValidator, ILocalValidator
	{
		public int Priority { get { return 0; } }

		public virtual void Validate(IProcessor iproc)
		{
			Validate<BCSalesOrderProcessor>(iproc, (processor) =>
			{
				BCEntity entity = processor.GetEntity();
				BCBinding store = processor.GetBinding();
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();

				//Branch
				if (store.BranchID == null)
					throw new PXException(BigCommerceMessages.NoBranch);

				ARSetup arSetup = PXSelect<ARSetup>.Select(processor);
				if (arSetup?.MigrationMode == true)
					throw new PXException(BCMessages.MigrationModeOnSO);
				//Integrated CC Porcessing
				if (arSetup?.IntegratedCCProcessing != true)
				{
					foreach (BCPaymentMethods method in BCPaymentMethodsMappingSlot.Get(store.BindingID).Where(x => x.Active == true && x.ProcessingCenterID != null))
					{
						throw new PXException(BCObjectsMessages.IntegratedCCProcessingSync, method.ProcessingCenterID, method.StorePaymentMethod);
					}
				}

				SOOrderType type = PXSelect<SOOrderType, Where<SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>>>.Select(processor, storeExt.OrderType);
				//OrderType
				if (type == null || type.Active != true)
					throw new PXException(BigCommerceMessages.NoSalesOrderType);
				//Order Numberings
				BCAutoNumberAttribute.CheckAutoNumbering(processor, type.OrderNumberingID);
			});
			Validate<BCPaymentProcessor>(iproc, (processor) =>
			{
				BCEntity entity = processor.GetEntity();
				BCBinding store = processor.GetBinding();
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();

				//Branch
				if (store.BranchID == null)
					throw new PXException(BigCommerceMessages.NoBranch);

				ARSetup arSetup = PXSelect<ARSetup>.Select(processor);
				if (arSetup?.MigrationMode == true)
					throw new PXException(BCMessages.MigrationModeOn);
				//Integrated CC Porcessing
				if (arSetup?.IntegratedCCProcessing != true)
				{
					foreach (BCPaymentMethods method in BCPaymentMethodsMappingSlot.Get(store.BindingID).Where(x => x.Active == true && x.ProcessingCenterID != null))
					{
						throw new PXException(BCObjectsMessages.IntegratedCCProcessingSync,  method.ProcessingCenterID, method.StorePaymentMethod);
					}
				}

				//Numberings
				BCAutoNumberAttribute.CheckAutoNumbering(processor, arSetup.PaymentNumberingID);
			});
			Validate<BCRefundsProcessor>(iproc, (processor) =>
			{
				BCEntity entity = processor.GetEntity();
				BCBinding store = processor.GetBinding();
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();

				//Branch
				if (store.BranchID == null)
					throw new PXException(BigCommerceMessages.NoBranch);

				ARSetup arSetup = PXSelect<ARSetup>.Select(processor);
				if (arSetup?.MigrationMode == true)
					throw new PXException(BCMessages.MigrationModeOn);
				//Integrated CC Porcessing
				if (arSetup?.IntegratedCCProcessing != true)
				{
					foreach (BCPaymentMethods method in BCPaymentMethodsMappingSlot.Get(store.BindingID).Where(x => x.Active == true && x.ProcessingCenterID != null))
					{
						throw new PXException(BCObjectsMessages.IntegratedCCProcessingSync, method.ProcessingCenterID, method.StorePaymentMethod);
					}
				}

				//Numberings
				BCAutoNumberAttribute.CheckAutoNumbering(processor, arSetup.PaymentNumberingID);
			});
		}

		public void Validate(IProcessor iproc, ILocalEntity ilocal)
		{
			Validate<BCSalesOrderProcessor, SalesOrder>(iproc, ilocal, (processor, entity) =>
			{
				BCBinding store = processor.GetBinding();
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();
				if (storeExt.PostDiscounts == BCPostDiscountAttribute.DocumentDiscount && entity.DiscountDetails?.Count > 0 && PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() == false)
					throw new PXException(BCMessages.DocumentDiscountSOMsg);
			});
			Validate<BCRefundsProcessor, SalesOrder>(iproc, ilocal, (processor, entity) =>
			{
				BCBinding store = processor.GetBinding();
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();
				if (storeExt.PostDiscounts == BCPostDiscountAttribute.DocumentDiscount && (entity.RefundOrders != null && entity.RefundOrders.Any(x => x.DiscountDetails?.Count > 0)) && PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() == false)
					throw new PXException(BCMessages.DocumentDiscountSOMsg);
			});
		}
	}
}
