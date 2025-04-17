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
using PX.SM;

namespace PX.Objects.EP.EmployeeMaint_Extensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class EmployeeMaint_EMailSyncAccountExt : PXGraphExtension<EmployeeMaint>
	{
		#region Views

		[PXHidden]
		[PXCopyPasteHiddenView]
		public SelectFrom<EMailSyncAccount>.View EMailSyncAccount;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public SelectFrom<EMailSyncAccountPreferences>.View EMailSyncAccountPreferences;

		#endregion

		#region Cache Attached

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EMailSyncAccount.employeeID>>>>))]
		public virtual void _(Events.CacheAttached<EMailSyncAccount.employeeID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EMailSyncAccountPreferences.employeeID>>>>))]
		public virtual void _(Events.CacheAttached<EMailSyncAccountPreferences.employeeID> e) { }

		#endregion
	}
}