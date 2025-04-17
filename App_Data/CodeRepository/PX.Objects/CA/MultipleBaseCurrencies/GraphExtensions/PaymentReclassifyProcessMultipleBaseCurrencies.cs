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
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CR;

namespace PX.Objects.CA
{
	public class PaymentReclassifyProcessMultipleBaseCurrencies : PXGraphExtension<PaymentReclassifyProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		protected virtual void _(Events.FieldUpdated<PaymentReclassifyProcess.Filter.branchID> e)
		{
			PaymentReclassifyProcess.Filter row = e.Row as PaymentReclassifyProcess.Filter;
			if (row == null) return;

			Branch currentBranch = PXSelectorAttribute.Select<PaymentReclassifyProcess.Filter.branchID>(e.Cache, row) as Branch;
			PXFieldState accFieldState = e.Cache.GetValueExt<PaymentReclassifyProcess.Filter.cashAccountID>(e.Row) as PXFieldState;

			if (accFieldState == null) return;
			CashAccount currentCashAccount = PXSelectorAttribute.Select<PaymentReclassifyProcess.Filter.cashAccountID>(e.Cache, row, accFieldState.Value) as CashAccount;

			if (currentCashAccount != null && (currentBranch?.BaseCuryID != currentCashAccount.BaseCuryID || currentCashAccount.RestrictVisibilityWithBranch == true))
			{
				e.Cache.SetValue<PaymentReclassifyProcess.Filter.cashAccountID>(row, null);
				e.Cache.SetValueExt<PaymentReclassifyProcess.Filter.cashAccountID>(row, null);
				e.Cache.SetValuePending<PaymentReclassifyProcess.Filter.cashAccountID>(row, null);
				e.Cache.RaiseExceptionHandling<PaymentReclassifyProcess.Filter.cashAccountID>(row, null, null);
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<BAccountR.baseCuryID, EqualBaseCuryID<Current<PaymentReclassifyProcess.Filter.branchID>>>), "")]
		protected virtual void _(Events.CacheAttached<CASplitExt.referenceID> e)
		{
		}

		protected virtual void _(Events.RowSelected<CASplitExt> e)
		{
			CASplitExt row = e.Row as CASplitExt;

			if (row != null && row.ReferenceID != null)
			{
				BAccountR bAccount = PXSelectorAttribute.Select<CASplitExt.referenceID>(e.Cache, row) as BAccountR;

				if (bAccount == null) 
				{
					bAccount = PXSelectReadonly<BAccountR,Where<BAccountR.bAccountID, Equal<Required<CASplitExt.referenceID>>>>.Select(this.Base, row.ReferenceID);
					
					var newValue = bAccount == null ? (object)row.ReferenceID : (object)bAccount.AcctCD;
					e.Cache.RaiseExceptionHandling<CASplitExt.referenceID>(row, newValue, 
						new PXSetPropertyException(Messages.FieldCanNotBeFound, PXUIFieldAttribute.GetDisplayName<CASplitExt.referenceID>(e.Cache)));
				}
			}
		}
	}
}
