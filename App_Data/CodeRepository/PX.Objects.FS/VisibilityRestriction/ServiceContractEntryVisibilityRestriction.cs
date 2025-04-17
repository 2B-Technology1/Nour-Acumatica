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
using System.Collections.Generic;

namespace PX.Objects.FS
{
	public class ServiceContractEntryVisibilityRestriction : PXGraphExtension<ServiceContractEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public delegate void CopyPasteGetScriptDelegate(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers);

		[PXOverride]
		public void CopyPasteGetScript(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers, CopyPasteGetScriptDelegate baseMethod)
		{
			baseMethod(isImportSimple, script, containers);

			// We need to process fields together that are related to the Branch and Customer for proper validation. For this:
			// 1) set the right order of the fields
			// 2) insert dependent fields after the BranchID field
			// 3) all fields must belong to the same view.

			string branchViewName = nameof(ServiceContractEntry.ServiceContractRecords) + ": 1";
			string customerViewName = nameof(ServiceContractEntry.ServiceContractRecords);

			(string name, string viewName) branch = (nameof(FSServiceContract.BranchID), branchViewName);

			List<(string name, string viewName)> fieldList = new List<(string name, string viewName)>();
			fieldList.Add((nameof(FSServiceContract.CustomerID), customerViewName));
			fieldList.Add((nameof(FSServiceContract.CustomerLocationID), customerViewName));
			fieldList.Add((nameof(FSServiceContract.BillCustomerID), branchViewName));
			fieldList.Add((nameof(FSServiceContract.BillLocationID), branchViewName));

			Common.Utilities.SetDependentFieldsAfterBranch(script, branch, fieldList);
		}

		public virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.billCustomerID> e)
		{
			if (e.Row == null)
				return;

			Base.SetBillTo(e.Row);
		}

		public virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.branchID> e)
		{
			if (e.Row == null)
				return;

			Base.SetBillTo(e.Row);
		}
	}
}
