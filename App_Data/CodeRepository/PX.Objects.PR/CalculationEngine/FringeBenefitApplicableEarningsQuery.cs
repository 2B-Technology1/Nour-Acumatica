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
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using PX.Objects.PM;
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		public class FringeBenefitApplicableEarningsQuery : SelectFrom<PREarningDetail>
			   .InnerJoin<PRProjectFringeBenefitRate>.On<PRProjectFringeBenefitRate.projectID.IsEqual<PREarningDetail.projectID>
				   .And<PRProjectFringeBenefitRate.laborItemID.IsEqual<PREarningDetail.labourItemID>>
				   .And<PRProjectFringeBenefitRate.projectTaskID.IsEqual<PREarningDetail.projectTaskID>
					   .Or<PRProjectFringeBenefitRate.projectTaskID.IsNull>>>
			   .InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<PREarningDetail.projectID>>
			   .InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			   .InnerJoin<PREmployee>.On<PREmployee.bAccountID.IsEqual<PREarningDetail.employeeID>>
			   .Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				   .And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				   .And<PRProjectFringeBenefitRate.effectiveDate.IsLessEqual<PREarningDetail.date>>
				   .And<PREarningDetail.isFringeRateEarning.IsEqual<False>>
				   .And<PREmployee.exemptFromCertifiedReporting.IsNotEqual<True>>>
			   .OrderBy<PRProjectFringeBenefitRate.projectTaskID.Desc, PRProjectFringeBenefitRate.effectiveDate.Desc>.View
		{
			public FringeBenefitApplicableEarningsQuery(PXGraph graph) : base(graph) { }
		}
	}
}
