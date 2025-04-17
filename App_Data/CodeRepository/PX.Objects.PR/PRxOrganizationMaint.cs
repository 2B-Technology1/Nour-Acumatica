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
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CS.DAC;
using PX.Objects.EP;
using PX.Objects.GL.DAC;

namespace PX.Objects.PR
{
	public class PRxOrganizationMaint : PXGraphExtension<OrganizationMaint>
	{
		public SelectFrom<PRTaxReportingAccount>.Where<PRTaxReportingAccount.bAccountID.IsEqual<OrganizationBAccount.bAccountID.FromCurrent>>.View TaxReportingAccount;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollCAN>();
		}

		protected virtual void OrganizationBAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			OrganizationBAccount orgBAccount = e.Row as OrganizationBAccount;
			Organization org = SelectFrom<Organization>.Where<Organization.bAccountID.IsEqual<P.AsInt>>.View.Select(Base, orgBAccount.BAccountID);
			TaxReportingAccount.Cache.AllowSelect = org != null && org.FileTaxesByBranches == false;
		}
	}
}
