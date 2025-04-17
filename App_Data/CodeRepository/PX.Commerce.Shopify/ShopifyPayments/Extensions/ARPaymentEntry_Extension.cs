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
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.GraphExtensions;
using System.Collections;
using System.Linq;

namespace PX.Commerce.Shopify.ShopifyPayments.Extensions
{
	public class ARPaymentEntry_PaymentTransaction_Extension : PXGraphExtension<ARPaymentEntryImportTransaction, ARPaymentEntryPaymentTransaction, ARPaymentEntry>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.integratedCardProcessing>();

		#region Overrides
		public delegate ICCPayment GetPaymentDocDelegate(ARPayment doc);

		[PXOverride]
		public virtual ICCPayment GetPaymentDoc(ARPayment doc, GetPaymentDocDelegate baseMethod)
		{
			if (baseMethod == null)
			{
				return null;
			}

			ICCPayment pDoc = baseMethod(doc);

			if (pDoc == null)
			{
				return pDoc;
			}

			SavePaymentKeysForPluginMethod(pDoc.DocType, pDoc.RefNbr);

			return pDoc;
		}

		public delegate IEnumerable VoidCardPaymentDelegate(PXAdapter adapter);

		[PXOverride]
		public virtual IEnumerable VoidCardPayment(PXAdapter adapter, VoidCardPaymentDelegate baseMethod)
		{
			if (baseMethod == null)
			{
				return null;
			}

			ARPayment payment = adapter.Get<ARPayment>().FirstOrDefault();

			if (payment == null || Base.IsContractBasedAPI == false)
            {
				return baseMethod(adapter);
			}

			SavePaymentKeysForPluginMethod(payment.DocType, payment.RefNbr);

			return baseMethod(adapter);
		}

		public delegate bool RunPendingOperationsDelegate(ARPayment doc);

		[PXOverride]
		public virtual bool RunPendingOperations(ARPayment doc, RunPendingOperationsDelegate baseMethod)
        {
			if (baseMethod == null)
			{
				return true;
			}

			if (doc == null)
			{
				return baseMethod(doc);
			}

			SavePaymentKeysForPluginMethod(doc.DocType, doc.RefNbr);

			return baseMethod(doc);
		}
		#endregion

		#region Methods
		public virtual void SavePaymentKeysForPluginMethod(string docType, string refNbr)
        {
			var selProcCenter = Base1.SelectedProcessingCenter;

			if (selProcCenter != null
				&& ExtensionHelper.IsShopifyPaymentsPlugin(Base, selProcCenter) == true)
			{
				// This is to save the payment document reference in a slot.
				// The reference will be used in the plugin method.
				SlotARPaymentKeys.SaveKeys(docType, refNbr);
			}
		}
		#endregion
	}
}
