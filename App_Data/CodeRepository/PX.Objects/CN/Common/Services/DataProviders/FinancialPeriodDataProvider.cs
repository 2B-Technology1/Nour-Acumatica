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
using PX.Objects.Common.Bql;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public class FinancialPeriodDataProvider
	{
		public static OrganizationFinPeriod GetFinancialPeriod(PXGraph graph, string financialPeriodId)
		{
			return SelectFrom<OrganizationFinPeriod>
				.Where<OrganizationFinPeriod.finPeriodID.IsEqual<P.AsString>
					.And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, AccessInfo.branchID.FromCurrent>>>
				.View.Select(graph, financialPeriodId);
		}
	}
}