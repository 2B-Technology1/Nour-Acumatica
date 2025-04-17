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
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		public class UnionDeductionQuery : SelectFrom<PREarningDetail>
			   .InnerJoin<PRDeductionAndBenefitUnionPackage>.On<PRDeductionAndBenefitUnionPackage.unionID.IsEqual<PREarningDetail.unionID>>
			   .InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID>>
			   .InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			   .Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				   .And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				   .And<PREarningDetail.isFringeRateEarning.IsNotEqual<True>>
				   .And<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID.IsEqual<P.AsInt>>
				   .And<PRDeductionAndBenefitUnionPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>
				   .And<PRDeductCode.isActive.IsEqual<True>>
				   .And<PRDeductCode.countryID.IsEqual<PRPayment.countryID.FromCurrent>>
				   .And<PREarningDetail.labourItemID.IsEqual<PRDeductionAndBenefitUnionPackage.laborItemID>
					   .Or<PRDeductionAndBenefitUnionPackage.laborItemID.IsNull
						   .And<PREarningDetail.labourItemID.IsNull
							   .Or<PREarningDetail.labourItemID.IsNotInSubselect<SearchFor<PRDeductionAndBenefitUnionPackage.laborItemID>
								   .Where<PRDeductionAndBenefitUnionPackage.unionID.IsEqual<PREarningDetail.unionID>
									   .And<PRDeductionAndBenefitUnionPackage.laborItemID.IsNotNull>
									   .And<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID.IsEqual<PRDeductCode.codeID>>
									   .And<PRDeductionAndBenefitUnionPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>>>>>>>.View
		{
			public UnionDeductionQuery(PXGraph graph) : base(graph) { }
		}
	}
}
