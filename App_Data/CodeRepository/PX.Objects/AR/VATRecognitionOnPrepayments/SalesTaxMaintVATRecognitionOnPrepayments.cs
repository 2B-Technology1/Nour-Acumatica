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

namespace PX.Objects.TX
{
	public class SalesTaxMaintVATRecognitionOnPrepayments : PXGraphExtension<SalesTaxMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.vATRecognitionOnPrepayments>();
		}

		[PXOverride]
		public virtual void SetPendingGLAccountsUI(PXCache cache, Tax tax)
		{
			cache.Adjust<PXUIFieldAttribute>(tax)
				 .For<Tax.pendingSalesTaxAcctID>(a => a.Visible = a.Enabled = true)
				 .SameFor<Tax.pendingSalesTaxSubID>();
		}

		public delegate void ResetPendingSalesTaxDelegate(PXCache cache, Tax newTax);

		[PXOverride]
		public virtual void ResetPendingSalesTax(PXCache cache, Tax newTax, ResetPendingSalesTaxDelegate baseMethod)
		{
		}

		public virtual void Tax_TaxVendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (!(e.Row is Tax tax))
				return;

			sender.SetDefaultExt<Tax.pendingSalesTaxAcctID>(tax);
			sender.SetDefaultExt<Tax.pendingSalesTaxSubID>(tax);
		}

		public virtual void Tax_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
				return;

			Base.CheckFieldIsEmpty<Tax.pendingSalesTaxAcctID>(sender, e);
			Base.CheckFieldIsEmpty<Tax.pendingSalesTaxSubID>(sender, e);
		}
	}
}
