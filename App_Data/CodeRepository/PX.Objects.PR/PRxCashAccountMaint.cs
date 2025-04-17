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
using PX.Objects.CA;
using PX.Objects.CS;

namespace PX.Objects.PR
{
	/// <summary>
	/// Extends the CashAccountMaint graph to adapt AP business logic / UI to also work with PR fields
	/// </summary>
	public class PRxCashAccountMaint : PXGraphExtension<CashAccountMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region CacheAttached

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), "DisplayName", "AP/PR Default")]
		protected virtual void _(Events.CacheAttached<PaymentMethodAccount.aPIsDefault> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), "DisplayName", "AP/PR - Suggest Next Number")]
		protected virtual void _(Events.CacheAttached<PaymentMethodAccount.aPAutoNextNbr> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), "DisplayName", "AP/PR Last Reference Number")]
		protected virtual void _(Events.CacheAttached<PaymentMethodAccount.aPLastRefNbr> e) { }

		#endregion CacheAttached

		protected virtual void _(Events.RowSelected<PaymentMethodAccount> e, PXRowSelected del)
		{
			del?.Invoke(e.Cache, e.Args);

			var rowExt = PXCache<PaymentMethodAccount>.GetExtension<PRxPaymentMethodAccount>(e.Row);
			if (rowExt != null)
			{
				bool useForAPorPR = e.Row.UseForAP == true || rowExt.UseForPR == true;
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPIsDefault>(Base.Details.Cache, null, useForAPorPR);
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPAutoNextNbr>(Base.Details.Cache, null, useForAPorPR);
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPLastRefNbr>(Base.Details.Cache, null, useForAPorPR);
			}
		}

		[PXOverride]
		public virtual PXResultset<PaymentMethodAccount> GetCurrentUsedPaymentMethodAccounts()
		{
			return PXSelect<PaymentMethodAccount,
			Where2<Where<PaymentMethodAccount.useForAP, Equal<True>,
					Or<PaymentMethodAccount.useForAR, Equal<True>,
					Or<PRxPaymentMethodAccount.useForPR, Equal<True>>>>,
				And<PaymentMethodAccount.cashAccountID, Equal<Current2<CashAccount.cashAccountID>>>>>.Select(Base);
		}
	}
}
