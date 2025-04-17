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
using PX.Objects.GL.DAC;
using static PX.Objects.CS.BranchMaint;

namespace PX.Objects.PR
{
	public class PRxBranchMaint : PXGraphExtension<BranchMaint>
	{
		public SelectFrom<PRTaxReportingAccount>.Where<PRTaxReportingAccount.bAccountID.IsEqual<BranchBAccount.bAccountID.FromCurrent>>.View TaxReportingAccount;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollCAN>();
		}

		protected virtual void BranchBAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			BranchBAccount branchBAccount = e.Row as BranchBAccount;
			Organization org = Organization.PK.Find(Base, branchBAccount.OrganizationID);
			TaxReportingAccount.Cache.AllowSelect = org != null && org.FileTaxesByBranches == true;
		}
	}
}
