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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using PX.Payroll.Data;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		public class ProjectDeductionQuery : SelectFrom<PREarningDetail>
			.InnerJoin<PRDeductionAndBenefitProjectPackage>.On<PRDeductionAndBenefitProjectPackage.projectID.IsEqual<PREarningDetail.projectID>>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID>>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			.Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PREarningDetail.isFringeRateEarning.IsNotEqual<True>>
				.And<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID.IsEqual<P.AsInt>>
				.And<PRDeductionAndBenefitProjectPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>
				.And<PRDeductCode.isActive.IsEqual<True>>
				.And<PRDeductCode.countryID.IsEqual<BQLLocationConstants.CountryUS>>
				.And<PREarningDetail.labourItemID.IsEqual<PRDeductionAndBenefitProjectPackage.laborItemID>
					.Or<PRDeductionAndBenefitProjectPackage.laborItemID.IsNull
						.And<PREarningDetail.labourItemID.IsNull
							.Or<PREarningDetail.labourItemID.IsNotInSubselect<SearchFor<PRDeductionAndBenefitProjectPackage.laborItemID>
								.Where<PRDeductionAndBenefitProjectPackage.projectID.IsEqual<PREarningDetail.projectID>
									.And<PRDeductionAndBenefitProjectPackage.laborItemID.IsNotNull>
									.And<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID.IsEqual<PRDeductCode.codeID>>
									.And<PRDeductionAndBenefitProjectPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>>>>>>>.View
		{
			public ProjectDeductionQuery(PXGraph graph) : base(graph) { }
		}
	}
}
