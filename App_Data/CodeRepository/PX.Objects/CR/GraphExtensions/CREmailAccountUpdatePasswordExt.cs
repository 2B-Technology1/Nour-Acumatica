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
using System.Collections;
using PX.Data;
using PX.SM;

namespace PX.Objects.CR.Extensions
{
	#region DACs

	[Serializable]
	[PXHidden]
	public class UpdatePasswordFilter : IBqlTable
	{

		#region EmailAccountPassword

		public abstract class emailAccountPassword : PX.Data.BQL.BqlBool.Field<emailAccountPassword> { }

		[PXUIField(DisplayName = "Email Account Password")]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual string EmailAccountPassword { get; set; }

		#endregion
	}

	#endregion

	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CREmailAccountUpdatePasswordExt : PXGraphExtension<SMAccessPersonalMaint>
	{
		#region Views

		[PXViewName(ActionsMessages.UpdatePassword)]
		public CRValidationFilter<UpdatePasswordFilter> UpdatePasswordFilterView;

		#endregion

		#region Actions
		public PXAction<Users> UpdatePassword;
		[PXUIField(DisplayName = ActionsMessages.UpdatePassword, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(Tooltip = ActionsMessages.UpdatePassword)]
		protected virtual IEnumerable updatePassword(PXAdapter adapter)
		{
			if (UpdatePasswordFilterView.AskExtFullyValid(DialogAnswerType.Positive))
			{
				EMailAccount eMailAccount = Base.EMailAccounts.Current;

				eMailAccount.Password = UpdatePasswordFilterView.Current.EmailAccountPassword;

				Base.EMailAccounts.Update(eMailAccount);
			}

			Base.Actions.PressSave();

			return adapter.Get();
		}

		#endregion
	}
}
