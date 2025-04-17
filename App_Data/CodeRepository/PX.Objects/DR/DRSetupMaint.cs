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

namespace PX.Objects.DR
{
	public class DRSetupMaint: PXGraph<DRSetupMaint>
	{
		public PXSelect<DRSetup> DRSetupRecord;
		public PXSave<DRSetup> Save;
		public PXCancel<DRSetup> Cancel;

		protected virtual void DRSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)  // exclude in extension
		{
			DRSetup row = e.Row as DRSetup;
			if (row != null)
			{
				bool isMulticurrency = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();  // exclude in extension
				bool isASC606 = PXAccess.FeatureInstalled<FeaturesSet.aSC606>();  // exclude in extension
				PXUIFieldAttribute.SetVisible<DRSetup.useFairValuePricesInBaseCurrency>(sender, row, isMulticurrency && isASC606);  // exclude in extension

				PXUIFieldAttribute.SetVisible<DRSetup.suspenseAccountID>(sender, row, isASC606);  // exclude in extension
				PXUIFieldAttribute.SetVisible<DRSetup.suspenseSubID>(sender, row, isASC606);  // exclude in extension

				PXUIFieldAttribute.SetVisible<DRSetup.recognizeAdjustmentsInPreviousPeriods>(sender, row, isASC606);  // exclude in extension
			}
		}
	}
}
