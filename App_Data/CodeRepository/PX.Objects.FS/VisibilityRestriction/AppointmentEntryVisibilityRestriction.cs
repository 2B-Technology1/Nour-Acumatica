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
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using System.Collections.Generic;

namespace PX.Objects.FS
{
	public class AppointmentEntryVisibilityRestriction : PXGraphExtension<AppointmentEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(RestrictCustomerByUserBranches))]
		public void _(Events.CacheAttached<FSServiceOrder.customerID> e)
		{
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

			string branchViewName = nameof(AppointmentEntry.ServiceOrderRelated) + ": 2";
			string customerViewName = nameof(AppointmentEntry.ServiceOrderRelated) + ": 1";

			(string name, string viewName) branch = (nameof(FSServiceOrder.BranchID), branchViewName);

			List<(string name, string viewName)> fieldList = new List<(string name, string viewName)>();
			fieldList.Add((nameof(FSServiceOrder.CustomerID), customerViewName));
			fieldList.Add((nameof(FSServiceOrder.LocationID), customerViewName));
			fieldList.Add((nameof(FSServiceOrder.BranchLocationID), customerViewName));
			fieldList.Add((nameof(FSServiceOrder.BillCustomerID), branchViewName));
			fieldList.Add((nameof(FSServiceOrder.BillLocationID), branchViewName));

			Common.Utilities.SetDependentFieldsAfterBranch(script, branch, fieldList);
		}

		[PXRemoveBaseAttribute(typeof(FSSelector_StaffMember_ServiceOrderProjectIDAttribute))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[FSSelector_StaffMember_ServiceOrderProjectIDVisibilityRestriction]
		public void _(Events.CacheAttached<FSAppointmentEmployee.employeeID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictVendorByUserBranches]
		public void _(Events.CacheAttached<FSAppointmentDet.poVendorID> e)
		{
		}
	}
}
