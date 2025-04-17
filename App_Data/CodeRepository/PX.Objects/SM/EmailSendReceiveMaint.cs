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
using PX.Data.Automation;
using PX.Data.WorkflowAPI;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;

namespace PX.SM
{
	//[PXGraphName(PX.Objects.EP.Messages.EmailSendReceiveMaint)]
    [Serializable]
	public class EmailSendReceiveMaint : PXGraph<EmailSendReceiveMaint>
	{
		#region OperationFilter
        [Serializable]
		[PXHidden]
		public partial class OperationFilter : IBqlTable
		{
			#region Operation

			public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }

			[PXWorkflowMassProcessing(DisplayName = "Action", AddUndefinedState = false)]
			public virtual String Operation { get; set; }

			#endregion
		}
		#endregion

		#region Selects
		[PXHidden]
		public PXFilter<OperationFilter>
			Filter;

		[PXCacheName(PX.Objects.EP.Messages.Emails)]
		[PXFilterable]
		public PXFilteredProcessing<EMailAccount, OperationFilter, 
			Where<EMailAccount.emailAccountType.IsIn<EmailAccountTypesAttribute.standard, EmailAccountTypesAttribute.plugin>
				.And<EMailAccount.isActive.IsEqual<True>>>>
			FilteredItems;

		public IEnumerable filteredItems()
		{
			foreach (EMailAccount account in SelectFrom<EMailAccount>
												.Where<EMailAccount.emailAccountType.IsIn<EmailAccountTypesAttribute.standard, EmailAccountTypesAttribute.plugin>
													.And<EMailAccount.isActive.IsEqual<True>>>.View.Select(this))
			{
				if (account.Address != null)
				{
					if (!String.IsNullOrEmpty(account.Address.Trim()))
					{
						yield return account;
					}
				}
			}
		}
		#endregion


		#region Ctors
		public EmailSendReceiveMaint()
		{
			CorrectUI();
			InitializeProcessing();
		}
		private void CorrectUI()
		{
			Actions.Move("Process", "Cancel");
			Actions.Move("Cancel", "Save");
		}

		private void InitializeProcessing()
		{
			FilteredItems.SetSelected<EMailAccount.selected>();
		}
		#endregion

		#region Actions
		public PXCancel<OperationFilter> Cancel;
		public PXAction<OperationFilter> ViewDetails;

		[PXUIField(DisplayName = PX.Objects.EP.Messages.ViewDetails, Visible = false)]
		[PXButton]
		protected IEnumerable viewDetails(PXAdapter adapter)
		{
			var cache = FilteredItems.Cache;
			var row = FilteredItems.Current;
			if (row != null)
				PXRedirectHelper.TryOpenPopup(cache, row, PX.Objects.EP.Messages.ViewDetails);
			return adapter.Get();
		}
		#endregion

		#region Event Handlers
        [PXRSACryptStringAttribute]
        [PXUIField(DisplayName = "Password")]
        protected virtual void EMailAccount_Password_CacheAttached(PXCache sender)
        {

        }

        [PXRSACryptStringAttribute]
        [PXUIField(DisplayName = "Password")]
        protected virtual void EMailAccount_OutcomingPassword_CacheAttached(PXCache sender)
        {

        }

		public virtual void _(Events.RowSelected<OperationFilter> e)
		{
			FilteredItems.SetProcessWorkflowAction(e.Row?.Operation);
		}

		public virtual void EMailAccount_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as EMailAccount;
			if (row == null) return;

			var outbox = PXSelectGroupBy<SMEmail,
                Where<SMEmail.mailAccountID, Equal<Required<SMEmail.mailAccountID>>,
					And<SMEmail.mpstatus, Equal<MailStatusListAttribute.preProcess>,
					And<SMEmail.isIncome, NotEqual<True>>>>,
				Aggregate<Count>>.
				Select(this, row.EmailAccountID).
				RowCount ?? 0;
			row.OutboxCount = outbox;

			var inbox = PXSelectGroupBy<SMEmail,
				Where<SMEmail.mailAccountID, Equal<Required<SMEmail.mailAccountID>>,
					And<SMEmail.mpstatus, Equal<MailStatusListAttribute.preProcess>,
					And<SMEmail.isIncome, Equal<True>>>>,
				Aggregate<Count>>.
                Select(this, row.EmailAccountID).
				RowCount ?? 0;
			row.InboxCount = inbox;
		}
		#endregion
	}
}
