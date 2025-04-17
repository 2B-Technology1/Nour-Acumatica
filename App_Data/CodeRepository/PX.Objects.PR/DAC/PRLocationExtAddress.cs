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
using PX.Objects.CR;
using PX.Objects.GL;
using System;

namespace PX.Objects.PR
{
	[Serializable]
	public class PRLocationExtAddress : PXCacheExtension<LocationExtAddress>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.payrollModule>();
		}

		#region CMPPayrollSubID
		public abstract class cMPPayrollSubID : BqlInt.Field<cMPPayrollSubID> { }
		[SubAccount(BqlField = typeof(Standalone.PRBranchLocation.cMPPayrollSubID), DisplayName = "Payroll Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PRBranchSubRequired(GLAccountSubSource.Branch)]
		public virtual Int32? CMPPayrollSubID { get; set; }
		#endregion
	}

	[Serializable]
	[PXTable(typeof(Location.locationID), typeof(Location.bAccountID), IsOptional = true)]
	public class PRBranchLocation : PXCacheExtension<Location>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.payrollModule>();
		}

		#region CMPPayrollSubID
		public abstract class cMPPayrollSubID : BqlInt.Field<cMPPayrollSubID> { }
		[SubAccount(DisplayName = "Payroll Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PRBranchSubRequired(GLAccountSubSource.Branch)]
		public virtual Int32? CMPPayrollSubID { get; set; }
		#endregion
	}
}

namespace PX.Objects.PR.Standalone
{
	[Serializable]
	[PXTable(typeof(Location.locationID), typeof(Location.bAccountID), IsOptional = true)]
	public class PRBranchLocation : PXCacheExtension<CR.Standalone.Location>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.payrollModule>();
		}

		#region CMPPayrollSubID
		public abstract class cMPPayrollSubID : BqlInt.Field<cMPPayrollSubID> { }
		[SubAccount(DisplayName = "Payroll Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? CMPPayrollSubID { get; set; }
		#endregion
	}
}

