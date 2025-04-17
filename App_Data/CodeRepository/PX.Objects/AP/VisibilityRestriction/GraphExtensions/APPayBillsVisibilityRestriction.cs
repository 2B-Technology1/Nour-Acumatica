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
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	public class APPayBillsVisibilityRestriction : PXGraphExtension<APPayBills>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[APInvoiceType.AdjdRefNbr(typeof(Search2<
				APInvoice.refNbr,
				InnerJoin<BAccount,
					On<BAccount.bAccountID, Equal<APInvoice.vendorID>,
						And<Where<
							BAccount.vStatus, Equal<VendorStatus.active>,
							Or<BAccount.vStatus, Equal<VendorStatus.oneTime>>>>>,
					LeftJoin<APAdjust,
						On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
							And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
								And<APAdjust.released, Equal<False>,
									And<Where<
										APAdjust.adjgDocType, NotEqual<Current<APPayment.docType>>,
										Or<APAdjust.adjgRefNbr, NotEqual<Current<APPayment.refNbr>>>>>>>>,
						LeftJoin<APPayment,
							On<APPayment.docType, Equal<APInvoice.docType>,
								And<APPayment.refNbr, Equal<APInvoice.refNbr>,
									And<APPayment.docType, Equal<APDocType.prepayment>>>>>>>,
				Where<
					APInvoice.docType, Equal<Optional<APAdjust.adjdDocType>>,
					And<BAccount.vOrgBAccountID, RestrictByBranch<Current<PayBillsFilter.branchID>>,
					And2<Where<
							APInvoice.released, Equal<True>,
							Or<APInvoice.prebooked, Equal<True>>>,
						And<APInvoice.openDoc, Equal<True>,
							And<APAdjust.adjgRefNbr, IsNull,
								And<APPayment.refNbr, IsNull>>>>>>>),
			Filterable = true)]
		protected virtual void APAdjust_AdjdRefNbr_CacheAttached(PXCache sender)
		{
		}

		public delegate BqlCommand ComposeBQLCommandForAPDocumentListSelectDelegate();

		[PXOverride]
		public virtual BqlCommand ComposeBQLCommandForAPDocumentListSelect(ComposeBQLCommandForAPDocumentListSelectDelegate baseMethod)
		{
			var cmd = baseMethod.Invoke();
			cmd.WhereAnd<Where<Vendor.vOrgBAccountID, RestrictByBranch<Current<PayBillsFilter.branchID>>>>();
			return cmd;
		}
	}
}