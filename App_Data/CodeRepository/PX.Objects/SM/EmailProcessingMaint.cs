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
using System.Collections.Generic;

using CommonServiceLocator;

using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.SM.Email;

namespace PX.SM
{
	//[PXGraphName(PX.Objects.EP.Messages.EmailProcessing)]
	[Serializable]
	public class EmailProcessingMaint : PXGraph<EmailProcessingMaint>
	{
		#region EmailProcessingFilter

		[Serializable]
		public partial class EmailProcessingFilter : OwnedFilter
		{
			#region Account

			public abstract class account : PX.Data.BQL.BqlInt.Field<account> { }

			[PXUIField(DisplayName = "Account")]
			[EmailAccountRaw]
			public virtual Int32? Account { get; set; }

			#endregion

			#region Type

			public abstract class type : PX.Data.BQL.BqlInt.Field<type> { }

			[PXInt]
			[PXDefault(0)]
			[PXUIField(DisplayName = "Type")]
			[PXIntList(new [] { 0, 1, 2 }, new [] { "All", "Incoming", "Outgoing" })]
			public virtual Int32? Type { get; set; }

			#endregion

			#region IncludeFailed
			public abstract class includeFailed : PX.Data.BQL.BqlBool.Field<includeFailed> { }
			[PXDBBool]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Include Failed")]
			public virtual Boolean? IncludeFailed { get; set; }
			#endregion


			#region ownerID
			public new abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
			#endregion

			#region myOwner
			public new abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
			#endregion

			#region myWorkGroup
			public new abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
			#endregion

			#region workGroupID
			public new abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
			#endregion
		}

		#endregion

		#region AllEmailes

		public sealed class AllEmailes : PX.Data.BQL.BqlInt.Constant<AllEmailes>
		{
			public AllEmailes() : base(0) { }
		}

		#endregion

		#region IncomingEmails

		public sealed class IncomingEmails : PX.Data.BQL.BqlInt.Constant<IncomingEmails>
		{
			public IncomingEmails() : base(1) { }
		}

		#endregion

		#region OutgoingEmails

		public sealed class OutgoingEmails : PX.Data.BQL.BqlInt.Constant<OutgoingEmails>
		{
			public OutgoingEmails() : base(2) { }
		}

		#endregion

		#region Selects

		[PXCacheName(PX.Objects.EP.Messages.Filter)]
		public PXFilter<EmailProcessingFilter>
			Filter;

		[PXCacheName(PX.Objects.EP.Messages.Emails)]
		[PXFilterable]
		public PXFilteredProcessingJoin<SMEmail, EmailProcessingFilter, 
			LeftJoin<EMailAccount, 
				On<EMailAccount.emailAccountID, Equal<SMEmail.mailAccountID>>,
			LeftJoin<CRActivity, 
				On<CRActivity.noteID, Equal<SMEmail.refNoteID>>>>,
			Where2<Where<SMEmail.isIncome, Equal<True>,
					Or<SMEmail.mpstatus, Equal<MailStatusListAttribute.failed>, Or<EMailAccount.emailAccountType, NotEqual<EmailAccountTypesAttribute.exchange>>>>,
 				And2<Where<Current<EmailProcessingFilter.includeFailed>, Equal<False>, 
					Or<SMEmail.mpstatus, Equal<MailStatusListAttribute.preProcess>, 
					Or<SMEmail.mpstatus, Equal<MailStatusListAttribute.failed>>>>, 
				And2<Where<Current<EmailProcessingFilter.includeFailed>, Equal<True> , 
					Or<SMEmail.mpstatus, Equal<MailStatusListAttribute.preProcess>>>,
				And2<Where<Current<EmailProcessingFilter.account>, IsNull, 
					Or<SMEmail.mailAccountID, Equal<Current<EmailProcessingFilter.account>>>>,
				And2<Where<Current<EmailProcessingFilter.type>, Equal<AllEmailes>, 
					Or2<Where<Current<EmailProcessingFilter.type>, Equal<IncomingEmails>, And<SMEmail.isIncome, Equal<True>>>, 
					Or<Where<Current<EmailProcessingFilter.type>, Equal<OutgoingEmails>, And<SMEmail.isIncome, NotEqual<True>>>>>>,
				And<Where<Current<EmailProcessingFilter.ownerID>, IsNull, Or<CRActivity.ownerID, Equal<Current<EmailProcessingFilter.ownerID>>>>>>>>>>>
			FilteredItems;

		#endregion

		#region Ctors

		public EmailProcessingMaint()
		{
			InitializeUI();

			InitializeProcessing();
		}

		private void InitializeUI()
		{
			PXUIFieldAttribute.SetDisplayName<EMailAccount.address>(Caches[typeof (EMailAccount)], MyMessages.Account);
			Actions.Move("Process", "Cancel");
			Actions.Move("Cancel", "Save");
		}

		private void InitializeProcessing()
		{
			FilteredItems.SetSelected<SMEmail.selected>();		

			FilteredItems.SetProcessDelegate(delegate(List<SMEmail> messages) 
			{
				var graph = PXGraph.CreateInstance<CREmailActivityMaint>();

				for (int i = 0; i < messages.Count; i++)
				{
					SMEmail message = messages[i];

					try
					{
						graph.ProcessEmailMessage(message);
						PXProcessing<SMEmail>.SetProcessed();
					}
					catch (Exception exception)
					{
						PXProcessing<SMEmail>.SetError(i, exception);
					}
				}
			});
		}

		#endregion

		#region Actions

		public PXCancel<EmailProcessingFilter> Cancel;

		public PXAction<EmailProcessingFilter> ViewDetails;

		[PXUIField(DisplayName = Objects.EP.Messages.ViewDetails, Visible = false)]
		[PXButton]
		protected IEnumerable viewDetails(PXAdapter adapter)
		{
            SMEmail item = FilteredItems.Current;

		    if (item != null)
		    {
		        if (item.RefNoteID != null)
		        {
		            CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
		            graph.Message.Current = graph.Message.Search<CRSMEmail.noteID>(item.RefNoteID);
		            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		        }
		        else
		        {
		            CRSMEmailMaint graph = CreateInstance<CRSMEmailMaint>();
		            graph.Email.Current = graph.Email.Search<SMEmail.noteID>(item.NoteID);
		            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		        }
		    }
		    return adapter.Get();
		}

		#endregion

		#region Event Handlers

		protected virtual void EmailProcessingFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as EmailProcessingFilter;
			if (row == null) return;

			var me = true.Equals(sender.GetValue(e.Row, typeof(EmailProcessingFilter.myOwner).Name));
			var myGroup = true.Equals(sender.GetValue(e.Row, typeof(EmailProcessingFilter.myWorkGroup).Name));

			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(EmailProcessingFilter.ownerID).Name, !me);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(EmailProcessingFilter.workGroupID).Name, !myGroup);
		}
		
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void SMEmail_Subject_CacheAttached(PXCache sender) { }

		#endregion
	}
}
