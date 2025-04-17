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
using PX.Objects.CS;
using PX.Objects.GL.DAC;
using System.Linq;

namespace PX.Objects.GL.Helpers
{
	public sealed class BranchHelper
	{
		/// <summary>
		/// The function returns BranchIDs of documents which can be applied if feature Inter-Branch Transactions is off.
		/// </summary>
		/// <param name="graph">PXGraph</param>
		/// <param name="branchID">BranchID of a document</param>
		/// <returns>BranchIDs of applicable documents. Returns NULL if the function is not applicable</returns>
		public static int[] GetBranchesToApplyDocuments(PXGraph graph, int? branchID)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.interBranch>() || branchID == null){	return null;}

			var organizationID = BranchMaint.FindBranchByID(graph, branchID)?.OrganizationID;
			Organization organization = OrganizationMaint.FindOrganizationByID(graph, organizationID);

			if (organization==null || organization?.OrganizationType == OrganizationTypes.WithBranchesBalancing)
			{
				return new[] { branchID.Value };
			}
			else
			{
				return BranchMaint.GetChildBranches(graph, organizationID).Select(x => x.BranchID.Value).ToArray();
			}
		}
	}
}
