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
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CN.Common.Extensions;

namespace PX.Objects.AR
{
	public class ARPaymentEntryVATRecognitionOnPrepayments : PXGraphExtension<ARPaymentEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.vATRecognitionOnPrepayments>();
		}
		public override void Initialize()
		{
			AddPrepaymentInvoiceDocType();
		}
		private void AddPrepaymentInvoiceDocType()
		{
			var allowedValues = ARDocType.PrepaymentInvoice.CreateArray();
			var allowedLabels = Messages.PrepaymentInvoice.CreateArray();
			PXStringListAttribute.AppendList<ARPayment.docType>(Base.Document.Cache, null, allowedValues, allowedLabels);
		}

		#region Cache Attached Events

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[Account(Visibility = PXUIVisibility.Invisible)]
		protected virtual void ARInvoice_PrepaymentAccountID_CacheAttached(PXCache sender) { }

		#endregion
	}
}
