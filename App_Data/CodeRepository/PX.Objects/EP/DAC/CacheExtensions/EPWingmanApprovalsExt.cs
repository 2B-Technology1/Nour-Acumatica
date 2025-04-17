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

using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.EP
{
	public sealed class EPWingmanApprovalsExt : PXCacheExtension<EPWingman>
	{
		public static bool IsActive() =>
			PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>()
			&& !PXAccess.FeatureInstalled<FeaturesSet.expenseManagement>();

		#region DelegationOf
		public abstract class delegationOf : PX.Data.BQL.BqlString.Field<delegationOf> { }

		[PXDBString(1, IsFixed = true)]
		[EPDelegationOf.ListApprovalsOnly]
		[PXDefault(EPDelegationOf.Approvals, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public string DelegationOf { get; set; }
		#endregion

		#region StartsOn
		public abstract class startsOn : PX.Data.BQL.BqlDateTime.Field<startsOn> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Starts On", Visibility = PXUIVisibility.SelectorVisible, Visible = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(Where<EPWingman.delegationOf.IsEqual<EPDelegationOf.approvals>>))]
		[PXUIRequired(typeof(Where<EPWingman.delegationOf.IsEqual<EPDelegationOf.approvals>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public DateTime? StartsOn { get; set; }
		#endregion

		#region ExpiresOn
		public abstract class expiresOn : PX.Data.BQL.BqlDateTime.Field<expiresOn> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Expires On", Visibility = PXUIVisibility.SelectorVisible, Visible = true)]
		[PXUIEnabled(typeof(Where<EPWingman.delegationOf.IsEqual<EPDelegationOf.approvals>>))]
		[EPVerifyEndDate(typeof(startsOn), AllowAutoChange = true, AutoChangeWarning = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public DateTime? ExpiresOn { get; set; }
		#endregion
	}

}
