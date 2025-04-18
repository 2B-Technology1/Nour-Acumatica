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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Compilation;

using CommonServiceLocator;

using PX.Common;
using PX.Common.Mail;
using PX.CS;
using PX.Data;
using PX.Data.EP;
using PX.Data.Wiki.Parser;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;
using PX.TM;
using FileInfo = PX.SM.FileInfo;
using PX.Data.RichTextEdit;
using PX.Web.UI;
using System.Net.Mail;
using PX.Objects.Common;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.CR.Extensions;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.BQL.Fluent;
using PX.Objects.PM;
using PX.Data.WorkflowAPI;
using PX.Objects.GL;

namespace PX.Objects.CR
{
	public class CREmailActivityMaint : CRBaseActivityMaint<CREmailActivityMaint, CRSMEmail>
	{
		#region Extensions

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class EmbeddedImagesExtractor
			: EmbeddedImagesExtractorExtension<CREmailActivityMaint, CRSMEmail, CRSMEmail.body>
				.WithFieldForExceptionPersistence<CRSMEmail.exception>
		{
		}

		private readonly EmbeddedImagesExtractor _extractor;

		#endregion

		public class TemplateSourceType
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						 new string[] { Notification, Activity, KnowledgeBase },
						 new string[] { Messages.EmailNotificationTemplate, Messages.EmailActivityTemplate, Messages.KnowledgeBaseArticle }
					) { }
			}
			public class ShortListAttribute : PXStringListAttribute
			{
				public ShortListAttribute()
					: base(
						 new string[] { KnowledgeBase },
						 new string[] { Messages.KnowledgeBaseArticle }
					) { }
			}

			public const string Notification = "NO";
			public const string Activity = "AC";
			public const string KnowledgeBase = "KB";

			public class notification : PX.Data.BQL.BqlString.Constant<notification>
			{
				public notification() : base(Notification) { }
			}
			public class activity : PX.Data.BQL.BqlString.Constant<activity>
			{
				public activity() : base(Activity) { }
			}
		}

		#region SendEmailParams
		public class SendEmailParams
		{
			private readonly IList<FileInfo> _attachments;

			public SendEmailParams()
			{
				_attachments = new List<FileInfo>();
			}

			public IList<FileInfo> Attachments
			{
				get { return _attachments; }
			}

			public string From { get; set; }

			public string To { get; set; }

			public string Cc { get; set; }

			public string Bcc { get; set; }

			public string Subject { get; set; }

			public string Body { get; set; }

			public object Source { get; set; }

			public object ParentSource { get; set; }

			public string TemplateID { get; set; }
		}
		#endregion

		#region Selects

		[PXCopyPasteHiddenFields(typeof(CRSMEmail.body))]
		public PXSelect<CRSMEmail>
			Message;

		public PXSelect<PMTimeActivity>
			TAct;

		public PXSelect<CRSMEmail, Where<CRSMEmail.noteID, Equal<Current<CRSMEmail.noteID>>>>
			CurrentMessage;

		[PXHidden]
		[Obsolete(InternalMessages.FieldIsObsoleteAndWillBeRemoved2019R2)]
		public PXSelect<CRActivity,
			Where<CRActivity.noteID, Equal<Current<CRSMEmail.noteID>>>>
			CRAct;

		[PXHidden]
		public PXSelect<SMEmail,
			Where<SMEmail.refNoteID, Equal<Current<CRSMEmail.noteID>>>>
			SMMail;

		public PMTimeActivityList<CRSMEmail>
			TimeActivity;
		
		[PXHidden]
		public PXSetup<CRSetup>
			crSetup;

		[PXHidden]
		public PXSelect<CT.Contract>
			 BaseContract;

		public PXSelect<Notification> Notification;

		public PX.Objects.SM.SPWikiCategoryMaint.PXSelectWikiFoldersTree Folders;

		#endregion

		#region Dependency Injection
		[InjectDependency]
		public PX.Reports.IReportLoaderService ReportLoader { get; private set; }

		[InjectDependency]
		public PX.Reports.IReportDataBinder ReportDataBinder { get; private set; }

		[InjectDependency]
		private IOriginalMailProvider OriginalMailProvider { get; set; }

		[InjectDependency]
		internal IMessageProccessor MessageProcessor { get; private set; }

		[InjectDependency]
		private IMailSendProvider MailSendProvider { get; set; }
		#endregion

		#region Ctors

		public CREmailActivityMaint()
		{
			PXStringListAttribute actionSource =
				PXAccess.FeatureInstalled<FeaturesSet.customerModule>() ? (PXStringListAttribute)new EntityList() : new EntityListSimple();

			Create.SetMenu(actionSource.ValueLabelDic.Select(entity => new ButtonMenu(entity.Key, PXMessages.LocalizeFormatNoPrefix(entity.Value), null)).ToArray());

			FieldVerifying.AddHandler(typeof(UploadFile), typeof(UploadFile.name).Name, UploadFileNameFieldVerifying);

			CRCaseActivityHelper.Attach(this);
			
			Action.AddMenuAction(Forward);
			Action.AddMenuAction(process);
			Action.AddMenuAction(DownloadEmlFile);
			Action.AddMenuAction(Archive);
			Action.AddMenuAction(RestoreArchive);
			Action.AddMenuAction(Restore);
			_extractor = GetExtension<EmbeddedImagesExtractor>();

			PXUIFieldAttribute.SetRequired<CRSMEmail.subject>(this.Caches[typeof(CRSMEmail)], false);

			// hack to suppress Ask on pressing the FILES button in the top right corner
			this.Message.View.Answer = WebDialogResult.OK;
		}

		#endregion

		#region Actions

		public class PXEMailActivityDelete<TNode> : PXDelete<TNode>
			where TNode : class, IBqlTable, new()
		{
			public PXEMailActivityDelete(PXGraph graph, string name)
				: base(graph, name)
			{
			}

			[PXUIField(DisplayName = ActionsMessages.Delete, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
			[PXDeleteButton(ConfirmationMessage = ActionsMessages.ConfirmDeleteExplicit, ClosePopup = false)]
			protected override IEnumerable Handler(PXAdapter adapter)
			{
				if (!adapter.View.Cache.AllowDelete)
				{
					throw new PXException(ErrorMessages.CantDeleteRecord);
				}
				int startRow = adapter.StartRow;
				bool deleted = false;
				CREmailActivityMaint graph = (CREmailActivityMaint)adapter.View.Graph;
				foreach (CRSMEmail record in adapter.Get())
				{
					CRSMEmail newMessage = (CRSMEmail)graph.Message.Cache.CreateCopy(record);
					if (newMessage.MPStatus != MailStatusListAttribute.Deleted)
					{
						UpdateEmailBeforeDeleting(graph, newMessage);
						graph.Actions.PressSave();
						yield return newMessage;
					}
					else
					{
						newMessage = graph.Message.Delete(newMessage);
						deleted = true;
					}
				}

				if (deleted)
				{
					try
					{
						graph.Actions.PressSave();
					}
					catch
					{
						graph.Clear();
						throw;
					}
					graph.SelectTimeStamp();
					adapter.StartRow = startRow;
					if (adapter.View.Cache.AllowInsert)
					{
						Insert(adapter);
						foreach (object ret in adapter.Get())
						{
							yield return ret;
						}
						adapter.View.Cache.IsDirty = false;
					}
					else
					{
						adapter.StartRow = 0;
						adapter.Searches = null;
						foreach (object item in adapter.Get())
						{
							yield return item;
						}
					}
				}
			}
		}

		public PXEMailActivityDelete<CRSMEmail> Delete;

		public PXAction<CRSMEmail> Send;
		[PXUIField(DisplayName = Messages.Send, MapEnableRights = PXCacheRights.Update)]
		[PXSendMailButton]
		protected virtual IEnumerable send(PXAdapter adapter)
		{
			var activity = Message.Current;
			if (activity == null) return new CRSMEmail[0];

			var res = new[] { activity };
			if (activity.MPStatus != ActivityStatusListAttribute.Draft &&
					activity.MPStatus != MailStatusListAttribute.Failed)
			{
				return res;
			}

			ValidateEmailFields(activity);
			Actions.PressSave();

			var newMessage = (CRSMEmail)Message.Cache.CreateCopy(activity);
			TryCorrectMailDisplayNames(newMessage);
			newMessage.MPStatus = MailStatusListAttribute.PreProcess;
			newMessage.RetryCount = 0;
			newMessage = (CRSMEmail)Message.Cache.Update(newMessage);
			this.SaveClose.Press();

			var pref = MailAccountManager.GetEmailPreferences();
			if (pref?.SendUserEmailsImmediately == true)
			{
				var graph = this.CloneGraphState();
				PXLongOperation.StartOperation(this, delegate ()
				{
					graph.Process(adapter);
				});
			}

			return new[] { newMessage };
		}

		public PXAction<CRSMEmail> Forward;
		[PXUIField(DisplayName = Messages.Forward, MapEnableRights = PXCacheRights.Select)]
		[PXForwardMailButton]
		protected void forward()
		{
			var targetGraph = CreateTargetMail(Message.Current);
			throw new PXRedirectRequiredException(targetGraph, true, "Forward")
			{
				Mode = PXBaseRedirectException.WindowMode.NewWindow
			};
		}

		public PXAction<CRSMEmail> ReplyAll;
		[PXUIField(DisplayName = Messages.ReplyAll, MapEnableRights = PXCacheRights.Select)]
		[PXReplyMailButton]
		protected void replyAll()
		{
			var oldMessage = Message.Current;
			var mailAccountAddress = GetMailAccountAddress(oldMessage);
			var targetGraph = CreateTargetMail(oldMessage,
				GetReplyAllAddress(oldMessage, mailAccountAddress),
				GetReplyAllCCAddress(oldMessage, mailAccountAddress),
				GetReplyAllBCCAddress(oldMessage, mailAccountAddress)
			);

			PXRedirectHelper.TryRedirect(targetGraph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<CRSMEmail> Reply;
		[PXUIField(DisplayName = Messages.Reply, MapEnableRights = PXCacheRights.Select)]
		[PXReplyMailButton]
		protected IEnumerable reply(PXAdapter adapter)
		{
			foreach (CRSMEmail oldMessage in adapter.Get())
			{
				var targetGraph = CreateTargetMail(oldMessage, GetReplyAddress(oldMessage));

				PXRedirectHelper.TryRedirect(targetGraph, PXRedirectHelper.WindowMode.NewWindow);
				yield return oldMessage;
			}
		}

		// for Outlook add-in
		public PXAction<CRSMEmail> ReplyInline;
		[PXUIField(DisplayName = Messages.Reply, MapEnableRights = PXCacheRights.Select, Visible = false)]
		[PXReplyMailButton]
		protected IEnumerable replyInline(PXAdapter adapter)
		{
			foreach (CRSMEmail oldMessage in adapter.Get())
			{
				var targetGraph = CreateTargetMail(oldMessage, GetReplyAddress(oldMessage));

				PXRedirectHelper.TryRedirect(targetGraph, PXRedirectHelper.WindowMode.Same);
				yield return oldMessage;
			}
		}

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns,
			bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{			
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		public virtual CREmailActivityMaint CreateTargetMail(CRSMEmail oldMessage, string replyTo = null, string cc = null, string bcc = null)
		{
			var targetGraph = PXGraph.CreateInstance<CREmailActivityMaint>();
			var message = targetGraph.Message.Insert();

			if (MailAccountManager.GetEmailAccountIfAllowed(this, oldMessage.MailAccountID) != null)
				message.MailAccountID = oldMessage.MailAccountID;

			message.RefNoteID = oldMessage.RefNoteID;
			message.BAccountID = oldMessage.BAccountID;
			message.ContactID = oldMessage.ContactID;
			message.ResponseToNoteID = oldMessage.EmailNoteID;
			message.IsIncome = false;
			message.Subject = GetSubjectPrefix(oldMessage.Subject, replyTo == null);
			message.MailTo = replyTo;
			message.MailCc = cc;
			message.MailBcc = bcc;

			message.MessageId = "<" + Guid.NewGuid() + "_acumatica" + GetMessageIDAppendix(this, message) + ">";

			message.Body = CreateReplyBody(oldMessage.MailFrom, oldMessage.MailTo, oldMessage.MailCc, oldMessage.Subject,
				oldMessage.Body, (DateTime)oldMessage.LastModifiedDateTime);

			if (OwnerAttribute.BelongsToWorkGroup(this, oldMessage.WorkgroupID, this.Accessinfo.ContactID))
				message.WorkgroupID = oldMessage.WorkgroupID;

			try
			{
			targetGraph.Message.Update(message);
			}
			catch (PXSetPropertyException e)
			{
				throw PXException.ExtractInner(PXException.ExtractInner(e));
			}

			message.ParentNoteID = oldMessage.ParentNoteID;
			message.NoteID = PXNoteAttribute.GetNoteID<CRSMEmail.noteID>(targetGraph.Message.Cache, message);
			//message.ProjectID = oldMessage.ProjectID;
			//message.ProjectTaskID = oldMessage.ProjectTaskID;
			targetGraph.Message.Update(message);

			CopyAttachments(targetGraph, oldMessage, message, replyTo != null);

			targetGraph.Message.Cache.IsDirty = false;
			return targetGraph;
		}

		public PXAction<CRSMEmail> process;
		[PXUIField(DisplayName = "Process", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected IEnumerable Process(PXAdapter adapter)
		{			
			var message = SMMail.SelectSingle();
			message.RetryCount = MailAccountManager.GetEmailPreferences().RepeatOnErrorSending;

			PXLongOperation.StartOperation(this, delegate()
			{
				var graph = PXGraph.CreateInstance<CREmailActivityMaint>();
				graph.ProcessEmailMessage(message);
			});
			return adapter.Get();
		}

		[Obsolete("This method is obsolete. Please use " + nameof(ProcessEmailMessage) + " instance method instead")]
		public static void ProcessMessage(SMEmail message)
		{
			var graph = PXGraph.CreateInstance<CREmailActivityMaint>();
			graph.ProcessEmailMessage(message);
		}

		public virtual void ProcessEmailMessage(SMEmail message)
		{
			if (MailAccountManager.IsMailProcessingOff) throw new PXException(PX.SM.Messages.MailProcessingIsTurnedOff);

			if (message != null &&
				(message.MPStatus == MailStatusListAttribute.PreProcess ||
				message.MPStatus == MailStatusListAttribute.Failed))
			{
				if (message.IsIncome == true)
					MessageProcessor.Process(message);
				else
					MailSendProvider.SendMessage(message);

				if (!string.IsNullOrEmpty(message.Exception))
					throw new PXException(message.Exception);
			}
		}

		public PXAction<CRSMEmail> CancelSending;
		[PXUIField(DisplayName = EP.Messages.CancelSending, MapEnableRights = PXCacheRights.Update)]
		[PXButton(Tooltip = EP.Messages.CancelSendingTooltip, Connotation = ActionConnotation.Danger)]
		public virtual IEnumerable cancelSending(PXAdapter adapter)
		{
			var message = Message.Current;
			if (message != null && message.MPStatus == MailStatusListAttribute.PreProcess)
			{
				var newMessage = (CRSMEmail)Message.Cache.CreateCopy(message);
				newMessage.MPStatus = ActivityStatusAttribute.Draft;
				Message.Cache.Update(newMessage);
				Actions.PressSave();
			}

			return adapter.Get<CRSMEmail>();
		}

		public PXAction<CRSMEmail> DownloadEmlFile;
		[PXUIField(DisplayName = EP.Messages.DownloadEmlFile)]
		[PXButton(Tooltip = EP.Messages.DownloadEmlFileTooltip)]
		public virtual void downloadEmlFile()
		{
			var message = SMMail.SelectSingle();
			if (message != null && message.IsIncome == true)
			{
				var mail = OriginalMailProvider.GetMail(message);

				if (mail == null)
					throw new PXException(Messages.EmailDoesntExistInRemoteServer);
				throw PXExportHandlerEml.GenerateException(mail);
			}
		}

		public PXMenuAction<CRSMEmail> Action;

		public PXAction<CRSMEmail> Create;
		[PXUIField(DisplayName = "Create")]
		[PXButton(MenuAutoOpen = true)]
		public virtual IEnumerable create(PXAdapter adapter)
		{
			if (string.IsNullOrEmpty(adapter.Menu))
				return adapter.Get();

			if (adapter.Menu == ExpenseReceipt &&
				!EmployeeMaint.GetCurrentEmployeeID(this).HasValue)
			{
				throw new PXException(Messages.MustBeEmployee, Accessinfo.DisplayName);
			}

			PXGraph targetGraph = PXGraph.CreateInstance(GraphTypes[adapter.Menu]);
			PXCache targetPrimaryCache = targetGraph.GetPrimaryCache();
			object targetEntity = targetPrimaryCache.Insert();

			CRLead targetEntity_AsLead = targetEntity as CRLead;
			Contact targetEntity_AsContact = targetEntity as Contact;
			CRCase targetEntity_AsCase = targetEntity as CRCase;
			CROpportunity targetEntity_AsOpportunity = targetEntity as CROpportunity;
			CRActivity targetEntity_AsActivity = targetEntity as CRActivity;

			var activityExt = targetGraph.FindImplementation<IActivityDetailsExt>();

			if (activityExt == null)
				return adapter.Get();

			var activityType = activityExt.GetActivityType();
			PXCache activityCache = targetGraph.Caches[activityType];


			List<CRSMEmail> sourceemails = adapter.Get().RowCast<CRSMEmail>().ToList();
			List<CRSMEmail> emails = adapter.MassProcess ? sourceemails.Where(e => e.Selected == true).ToList() : sourceemails;

			CRSMEmail mainEmail = null;

			var view = new PXView(this, false,
				BqlCommand.CreateInstance(
					BqlCommand.Compose(
						typeof(Select<,>), activityType,
						typeof(Where<,>), activityType.GetNestedType(typeof(CRActivity.noteID).Name),
						typeof(Equal<>), typeof(Required<>), activityType.GetNestedType(typeof(CRActivity.noteID).Name))));

			HashSet<EmailAddress> names = new HashSet<EmailAddress>();

			foreach (CRSMEmail emailFrame in emails)
			{
				if (mainEmail == null)
					mainEmail = emailFrame;

				CRActivity email = (CRActivity)view.SelectSingle(emailFrame.NoteID);

				if (targetEntity_AsActivity != null && targetEntity_AsActivity.ClassID != CRActivityClass.Task && targetEntity_AsActivity.ClassID != CRActivityClass.Event)
				{
					email.ContactID = targetEntity_AsActivity.ContactID;
					email.BAccountID = targetEntity_AsActivity.BAccountID;
					email.RefNoteID = targetEntity_AsActivity.RefNoteID;
				}
				else
				{
					email.RefNoteID = PXNoteAttribute.GetNoteID(targetPrimaryCache, targetEntity, EntityHelper.GetNoteField(targetPrimaryCache.GetItemType()));
					email.ContactID = targetEntity_AsLead == null
						? targetEntity_AsContact?.ContactID
						: null;
				}

				object updated = email;
				if (activityType != email.GetType())
				{
					updated = activityCache.CreateInstance();
					activityCache.RestoreCopy(updated, email);
				}

				updated = activityCache.Update(updated);
				if (targetEntity_AsActivity != null)
				{
					targetEntity_AsActivity.Subject = mainEmail.Subject;
					targetPrimaryCache.Update(targetEntity_AsActivity);
					activityCache.SetValue<CRActivity.parentNoteID>(updated, targetEntity_AsActivity.NoteID);
				}
				EmailAddress name;
				if ((targetEntity_AsContact != null || targetEntity_AsOpportunity != null) && names.Count <= 1 && (name = ParseNames(emailFrame.MailFrom)) != null)
				{
					names.Add(name);
				}
			}

			if (targetEntity_AsContact != null && names.Count == 1)
			{
				targetEntity_AsContact.LastName = string.IsNullOrEmpty(names.ToArray()[0].LastName) ? names.ToArray()[0].Email : names.ToArray()[0].LastName;
				targetEntity_AsContact.FirstName = names.ToArray()[0].FirstName;
				targetEntity_AsContact.EMail = names.ToArray()[0].Email;

				targetPrimaryCache.Update(targetEntity_AsContact);
			}

			if (targetEntity_AsCase != null && mainEmail != null)
			{
				targetEntity_AsCase.Subject = mainEmail.Subject;
				targetEntity_AsCase.Description = mainEmail.Body;

				targetPrimaryCache.Update(targetEntity_AsCase);
			}

			if (targetEntity_AsOpportunity != null && mainEmail != null)
			{
				targetEntity_AsOpportunity.Subject = mainEmail.Subject;
				targetEntity_AsOpportunity.Details = mainEmail.Body;

				targetPrimaryCache.Update(targetEntity_AsOpportunity);

				PXCache contactCache = targetGraph.Caches[typeof(CRContact)];
				if (contactCache != null && names.Count == 1)
				{
					CRContact crcontact = targetGraph.Views[nameof(OpportunityMaint.Opportunity_Contact)].SelectSingle() as CRContact;
					if (crcontact != null)
					{
						targetEntity_AsOpportunity.AllowOverrideContactAddress = true;
						crcontact.LastName = names.ToArray()[0].LastName;
						crcontact.FirstName = names.ToArray()[0].FirstName;
						crcontact.Email = names.ToArray()[0].Email;
					}

					contactCache.Update(crcontact);

					targetPrimaryCache.Update(targetEntity_AsOpportunity);
				}
			}

			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(targetGraph, !adapter.ExternalCall ? PXRedirectHelper.WindowMode.InlineWindow : PXRedirectHelper.WindowMode.NewWindow);

			targetGraph.Actions.PressSave();

			return emails;
		}

		public PXAction<CRSMEmail> Archive;
		[PXUIField(DisplayName = "Archive", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable archive(PXAdapter adapter)
		{
			var cache = this.Caches<CRSMEmail>();

			foreach (CRSMEmail newMessage in adapter.Get().Cast<CRSMEmail>())
			{
				if (!(newMessage.IsArchived ?? false))
				{
					newMessage.IsArchived = true;
					PXDefaultAttribute.SetPersistingCheck<CRSMEmail.mailTo>(cache, newMessage, PXPersistingCheck.Nothing);
					Message.Update(newMessage);
				}
				Actions.PressSave();
				yield return newMessage;
			}
		}

		public PXAction<CRSMEmail> RestoreArchive;
		[PXUIField(DisplayName = "Restore from Archive", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable restoreArchive(PXAdapter adapter)
		{
			var cache = this.Caches<CRSMEmail>();

			foreach (CRSMEmail newMessage in adapter.Get().Cast<CRSMEmail>())
			{
				if (newMessage.IsArchived ?? false)
				{
					newMessage.IsArchived = false;
					PXDefaultAttribute.SetPersistingCheck<CRSMEmail.mailTo>(cache, newMessage, PXPersistingCheck.Nothing);
					Message.Update(newMessage);
				}
				Actions.PressSave();
				yield return newMessage;
			}
		}

		public PXAction<CRSMEmail> Restore;
		[PXUIField(DisplayName = "Restore Deleted", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable restore(PXAdapter adapter)
		{
			foreach (CRSMEmail newMessage in adapter.Get().Cast<CRSMEmail>().Select(record => (CRSMEmail)Message.Cache.CreateCopy(record)))
			{
				CRSMEmail msg = newMessage;
				if (newMessage.MPStatus == MailStatusListAttribute.Deleted)
				{
					newMessage.MPStatus = newMessage.IsIncome == true
						? MailStatusListAttribute.Processed
						: MailStatusListAttribute.Draft;
					msg = Message.Update(newMessage);
				}
				Actions.PressSave();
				yield return msg;
			}
		}
		#endregion

		#region Data Handlers

		public override void Persist()
		{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				base.Persist();

				CorrectFileNames();

				ts.Complete();
			}
		}

		#endregion

		#region Event Handlers
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[EPProject(typeof(PMTimeActivity.ownerID), true, FieldClass = ProjectAttribute.DimensionName)]
		protected virtual void _(Events.CacheAttached<PMTimeActivity.projectID> e) { }

		[PXUIField(DisplayName = "Parent Activity", Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRSMEmail_ParentNoteID_CacheAttached(PXCache cache) { }

		[PXDefault]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void CRSMEmail_MailAccountID_CacheAttached(PXCache sender) { }

		[PXUIField(DisplayName = "Incoming")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void CRSMEmail_IsIncome_CacheAttached(PXCache sender) { }

		[CREmailSelector]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void CRSMEmail_MailTo_CacheAttached(PXCache sender) { }

		[CREmailSelector]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void CRSMEmail_MailCc_CacheAttached(PXCache sender) { }

		[CREmailSelector]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void CRSMEmail_MailBcc_CacheAttached(PXCache sender) { }

		[PXDefault(MailStatusListAttribute.Draft)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRSMEmail_MPStatus_CacheAttached(PXCache cache) { }

		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRSMEmail_Subject_CacheAttached(PXCache sender) { }
		
		[PXParent(typeof(Select<CRSMEmail, Where<CRSMEmail.noteID, Equal<Current<PMTimeActivity.refNoteID>>>>), ParentCreate = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void PMTimeActivity_RefNoteID_CacheAttached(PXCache sender) { }

		[PXSubordinateGroupSelector]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void PMTimeActivity_WorkgroupID_CacheAttached(PXCache sender) { }

		[PXFormula(typeof(Switch<
			Case<Where<PMTimeActivity.trackTime, Equal<True>>, ActivityStatusAttribute.open,
			Case<Where<PMTimeActivity.released, Equal<True>>, ActivityStatusAttribute.released,
			Case<Where<PMTimeActivity.approverID, IsNotNull>, ActivityStatusAttribute.pendingApproval>>>,
			ActivityStatusAttribute.completed>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected void _(Events.CacheAttached<PMTimeActivity.approvalStatus> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PM.ProjectTask(typeof(PMTimeActivity.projectID), "TA", DisplayName = "Project Task", AllowNull = true, DefaultActiveTask = true)]
		public void _(Events.CacheAttached<PMTimeActivity.projectTaskID> args)
		{
		}

		protected virtual void CRSMEmail_Body_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = MailAccountManager.AppendSignature(e.NewValue as string, this, MailAccountManager.SignatureOptions.NewEmail);
		}

		protected virtual void CRSMEmail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CRSMEmail row = (CRSMEmail)e.Row;
			if (row == null)
			{
				//It's need for correct working outlook plugin.
				Forward.SetEnabled(true);
				Reply.SetEnabled(true);
				ReplyAll.SetEnabled(true);
				Forward.SetVisible(true);
				Reply.SetVisible(true);
				ReplyAll.SetVisible(true);
				return;
			}

			var tAct = (PMTimeActivity)TimeActivity.SelectSingle();
			var tActCache = TimeActivity.Cache;

			
			var wasUsed = !string.IsNullOrEmpty(tAct?.TimeCardCD) || tAct?.Billed == true;
			if (wasUsed)
				PXUIFieldAttribute.SetEnabled(cache, row, false);

			var showMinutes = EPSetupCurrent.RequireTimes == true;			
			PXDBDateAndTimeAttribute.SetTimeVisible<CRSMEmail.startDate>(cache, row, true);
			PXDBDateAndTimeAttribute.SetTimeEnabled<CRSMEmail.startDate>(cache, row, true);
			PXDBDateAndTimeAttribute.SetTimeVisible<CRSMEmail.endDate>(cache, row, showMinutes && tAct?.TrackTime == true);

			string origStatus =
				(string)this.Message.Cache.GetValueOriginal<CRSMEmail.uistatus>(row) ?? ActivityStatusListAttribute.Open;

			bool? oringTrackTime =
				(bool?)this.TimeActivity.Cache.GetValueOriginal<PMTimeActivity.trackTime>(tAct) ?? false;

			if (origStatus == ActivityStatusAttribute.Completed && oringTrackTime != true)
				origStatus = ActivityStatusAttribute.Open;

			if (row.IsLocked == true)
				origStatus = ActivityStatusAttribute.Completed;

			PXUIFieldAttribute.SetEnabled(cache, row, row.MPStatus == MailStatusListAttribute.Draft || row.MPStatus == MailStatusListAttribute.Failed);

			if (origStatus == ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled<CRSMEmail.isExternal>(cache, row, true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<CRSMEmail.isExternal>(cache, row, false);
			}

			PXUIFieldAttribute.SetEnabled<CRSMEmail.parentNoteID>(cache, row, IsContractBasedAPI);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.mpstatus>(cache, row, IsContractBasedAPI);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.startDate>(cache, row, IsContractBasedAPI);
			PXUIFieldAttribute.SetVisible<CRSMEmail.uistatus>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.type>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.isPrivate>(cache, row, true);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.ownerID>(cache, row, (origStatus == ActivityStatusListAttribute.Open && row.MPStatus == MailStatusListAttribute.Draft && row.IsLocked != true) || row.IsIncome == true);
			PXUIFieldAttribute.SetEnabled<CRSMEmail.workgroupID>(cache, row, (origStatus == ActivityStatusListAttribute.Open && row.MPStatus == MailStatusListAttribute.Draft && row.IsLocked != true) || row.IsIncome == true);

			row.EntityDescription = CacheUtility.GetErrorDescription(row.Exception) + GetEntityDescription(row);

			var isIncome = row.IsIncome == true;
			var isImap = row.ImapUID != null;

			// TODO: clear redundant Enables
			Create.SetEnabled(isIncome);
			Create.SetVisible(isIncome);
			Send.SetVisible(!isIncome && (row.MPStatus == MailStatusListAttribute.Failed || row.MPStatus == MailStatusListAttribute.Draft));
			Send.SetEnabled(!String.IsNullOrEmpty(row.MailFrom));

			Action.SetVisible(nameof(downloadEmlFile), isIncome);

			CancelSending.SetVisible(!isIncome && row.MPStatus == MailStatusListAttribute.PreProcess);
			CancelSending.SetEnabled(!isIncome && row.MPStatus == MailStatusListAttribute.PreProcess);

			var account = PXSelectorAttribute.Select<CRSMEmail.mailAccountID>(cache, row) as EMailAccount;
			this.Action.SetVisible(nameof(Process), account?.EmailAccountType != EmailAccountTypesAttribute.Exchange);
			process.SetEnabled(row.MPStatus == MailStatusListAttribute.PreProcess || (isIncome && row.MPStatus == MailStatusListAttribute.Failed));

			var isInserted = cache.GetStatus(row) == PXEntryStatus.Inserted;

			Forward.SetEnabled(!isInserted && row.MPStatus != MailStatusListAttribute.Draft && row.MPStatus != MailStatusListAttribute.PreProcess);
			Reply.SetEnabled(!isInserted && row.MPStatus != MailStatusListAttribute.Draft && row.MPStatus != MailStatusListAttribute.PreProcess);
			ReplyAll.SetEnabled(!isInserted && row.MPStatus != MailStatusListAttribute.Draft && row.MPStatus != MailStatusListAttribute.PreProcess);
			DownloadEmlFile.SetEnabled(isIncome && isImap);

			PXUIFieldAttribute.SetRequired<CRSMEmail.ownerID>(cache, !this.UnattendedMode && tAct?.TrackTime == true && row.MPStatus != MailStatusListAttribute.Deleted);

            if(row.IsIncome == true)
				// Acuminator disable once PX1043 SavingChangesInEventHandlers [Persist is called only once on the entity opening by the user]
				// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [EPView is placed in cache to Hold the entity]
				MarkAs(cache, row, Accessinfo.ContactID, EPViewStatusAttribute.VIEWED);

			Archive.SetEnabled(row.IsArchived == false);
			Action.SetVisible(nameof(archive), row.IsArchived == false);
			RestoreArchive.SetEnabled(row.IsArchived == true);
			Action.SetVisible(nameof(restoreArchive), row.IsArchived == true);

			Restore.SetEnabled(row.MPStatus == MailStatusListAttribute.Deleted);
			Action.SetVisible(nameof(restore), row.MPStatus == MailStatusListAttribute.Deleted);

			if (row.ClassID == -2)
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);
				PXUIFieldAttribute.SetEnabled<CRSMEmail.isPrivate>(cache, row, true);
			}

			PXUIFieldAttribute.SetEnabled<CRActivity.refNoteID>(cache, row, cache.GetValue<CRActivity.refNoteIDType>(row) != null || IsContractBasedAPI);
		}

		protected virtual void PMTimeActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PMTimeActivity row = (PMTimeActivity)e.Row;

			var email = this.CurrentMessage.Current;
			if (email == null) return;

			bool isPmVisible = PM.ProjectAttribute.IsPMVisible(GL.BatchModule.TA);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.trackTime>(cache, null, email.IsIncome != true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.approvalStatus>(cache, null, row?.TrackTime == true && email.IsIncome != true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.timeSpent>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.earningTypeID>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.isBillable>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.released>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.timeBillable>(cache, null, row?.IsBillable == true && row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.overtimeBillable>(cache, null, row?.IsBillable == true && row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.approverID>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.overtimeSpent>(cache, null, false);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.overtimeBillable>(cache, null, false);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.projectID>(cache, null, row?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.certifiedJob>(cache, null, row?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.projectTaskID>(cache, null, row?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.costCodeID>(cache, null, row?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.labourItemID>(cache, null, row?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.unionID>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.workCodeID>(cache, null, row?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.shiftID>(cache, null, row?.TrackTime == true);

			if (row != null)
			{
				PXUIFieldAttribute.SetRequired<PMTimeActivity.projectTaskID>(cache, row.ProjectID != null && row.ProjectID != PM.ProjectDefaultAttribute.NonProject());
				PXDefaultAttribute.SetPersistingCheck<PMTimeActivity.projectTaskID>(cache, row, row.TrackTime == true && row.ProjectID != null && row.ProjectID != PM.ProjectDefaultAttribute.NonProject() ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}

			string origStatus =
				(string)this.Message.Cache.GetValueOriginal<CRSMEmail.uistatus>(email)
				?? ActivityStatusListAttribute.Open;

			bool? oringTrackTime =
				(bool?)this.TimeActivity.Cache.GetValueOriginal<PMTimeActivity.trackTime>(row)
				?? false;

			string origTimeStatus =
				(string)this.TimeActivity.Cache.GetValueOriginal<PMTimeActivity.approvalStatus>(row)
				?? ActivityStatusListAttribute.Open;

			if (origStatus == ActivityStatusAttribute.Completed && oringTrackTime != true)
				origStatus = ActivityStatusAttribute.Open;

			if (email.IsLocked == true)
				origStatus = ActivityStatusAttribute.Completed;

			if (origStatus != ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.trackTime>(cache, row, email.IsLocked != true);
			}

			var wasUsed = !string.IsNullOrEmpty(row?.TimeCardCD) || row?.Billed == true;

			Delete.SetEnabled(!wasUsed && row?.Released != true);

			// TimeActivity
			if (row?.Released == true)
				origTimeStatus = ActivityStatusAttribute.Completed;

			if (origTimeStatus == ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled(cache, row, true);

				PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeBillable>(cache, row, !wasUsed && row?.IsBillable == true);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.overtimeBillable>(cache, row, !wasUsed && row?.IsBillable == true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);
			}

			PXUIFieldAttribute.SetEnabled<PMTimeActivity.approvalStatus>(cache, row, row != null && row.TrackTime == true && !wasUsed);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.released>(cache, row, false);

		}

		protected virtual void _(Events.FieldUpdated<CRSMEmail, CRSMEmail.refNoteID> e)
		{
			CRSMEmail email = (CRSMEmail)e.Row;
			if (email != null && email.IsIncome == true)
			{
				email.Exception = null;
			}
		}

		protected virtual void CRSMEmail_OwnerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CRSMEmail email = (CRSMEmail)e.Row;
			if (email != null && email.OwnerID != null && email.OwnerID != (int?)e.OldValue && email.IsIncome == true)
			{
				// Acuminator disable once PX1043 SavingChangesInEventHandlers [Persist is called only once on the entity opening by the user]
				// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [EPView is placed in cache to Hold the entity]
				MarkAs(sender, email, email.OwnerID, EPViewStatusAttribute.NOTVIEWED);
			}
		}

		protected virtual void CRSMEmail_MailAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as CRSMEmail;
			if (row == null) return;

			row.MessageId = "<" + Guid.NewGuid() + "_acumatica" + GetMessageIDAppendix(this, row) + ">";
			row.MailFrom = FillMailFrom(this, row, true);
            row.MailReply = FillMailReply(this, row);
        }

        protected virtual void CRSMEmail_UIStatus_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CRSMEmail_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CRSMEmail;
			if (row == null) return;

			row.ClassID = CRActivityClass.Email;

			if (row.OwnerID == null)
			{
				var newOwner = EmployeeMaint.GetCurrentOwnerID(this);
				if (OwnerAttribute.BelongsToWorkGroup(this, row.WorkgroupID, newOwner))
					row.OwnerID = newOwner;
			}

			if (row.MailAccountID == null && row.IsIncome != true && row.ClassID != CRActivityClass.EmailRouting)
			{
				row.MailAccountID = GetDefaultAccountId(row.OwnerID);
				cache.RaiseFieldUpdated<CRSMEmail.mailAccountID>(row, null);
			}

			if (row.IsIncome != true && row.ClassID != CRActivityClass.EmailRouting)
				row.MailFrom = FillMailFrom(this, row, true);

			row.MailFrom = FillMailFrom(this, row, true);
			row.MailReply = FillMailReply(this, row);
			
			row.MessageId = "<" + Guid.NewGuid() + "_acumatica" + GetMessageIDAppendix(this, row) + ">";
		}

		protected virtual void CRSMEmail_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRSMEmail;
			var oldRow = e.OldRow as CRSMEmail;
			if (row == null || oldRow == null) return;

            row.ClassID = CRActivityClass.Email;

			if (row.IsIncome != true && row.ClassID != CRActivityClass.EmailRouting && oldRow.OwnerID != row.OwnerID)
				row.MailFrom = FillMailFrom(this, row, true);

			if (row.IsIncome != true && row.ClassID != CRActivityClass.EmailRouting && oldRow.OwnerID != row.OwnerID || row.MailAccountID == null)
				row.MailAccountID = GetDefaultAccountId(row.OwnerID);
			if(row.MessageId == null)
                row.MessageId = "<" + Guid.NewGuid() + "_acumatica" + GetMessageIDAppendix(this, row) + ">";
        }

		protected virtual void CRSMEmail_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			
		}

		protected static void UpdateEmailBeforeDeleting(CREmailActivityMaint graph, CRSMEmail record)
		{
			graph.TryCorrectMailDisplayNames(record);
			record.MPStatus = MailStatusListAttribute.Deleted;
			record.UIStatus = ActivityStatusAttribute.Canceled;
			record = graph.Message.Update(record);
			foreach (Type field in graph.Message.Cache.BqlFields.Where(f => f != typeof(CRSMEmail.mpstatus)
				&& f != typeof(CRSMEmail.noteID)
				&& f != typeof(CRSMEmail.emailNoteID)
				&& f != typeof(CRSMEmail.uistatus)
				&& f != typeof(CRActivity.noteID)
				&& f != typeof(CRActivity.uistatus)
				&& f != typeof(SMEmail.noteID)
				&& f != typeof(CRSMEmail.emailLastModifiedByID)
				&& f != typeof(CRSMEmail.emailLastModifiedByScreenID)
				&& f != typeof(CRSMEmail.emailLastModifiedDateTime)
				&& f != typeof(CRActivity.lastModifiedByID)
				&& f != typeof(CRActivity.lastModifiedByScreenID)
				&& f != typeof(CRActivity.lastModifiedDateTime)
				))
			{
				graph.CommandPreparing.AddHandler(typeof(CRSMEmail), field.Name,
						(sender, args) => { if (args.Operation == PXDBOperation.Update) args.Cancel = true; });
			}
		}

		protected virtual void _(Events.RowDeleting<CRSMEmail> e)
		{
			CRSMEmail newMessage = (CRSMEmail)this.Message.Cache.CreateCopy(e.Row);
			if (newMessage.MPStatus != MailStatusListAttribute.Deleted)
			{
				UpdateEmailBeforeDeleting(this, newMessage);
				throw new PXSetPropertyException(Messages.EmailStatusUpdatedBeforeDeleting, PXErrorLevel.RowWarning);
			}
			var row = (CRSMEmail)e.Row;
			if (row == null) return;

			var timeAct = TimeActivity.Current;

			if (timeAct != null)
			{
				if (timeAct.Billed == true || !string.IsNullOrEmpty(timeAct.TimeCardCD))
				{
					e.Cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
					throw new PXException(TM.Messages.EmailActivityCannotBeDeleted);
				}

				TimeActivity.Cache.Delete(timeAct);
			}
		}


		protected virtual void UploadFileNameFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void _(Events.RowPersisting<CRSMEmail> e)
		{
			if (e.Row == null) return;
			var email = e.Row;

			if (e.Operation != PXDBOperation.Delete && String.IsNullOrEmpty(email.Subject))
			{
				Message.Cache.SetValueExt<CRSMEmail.subject>(email, PXMessages.LocalizeNoPrefix(Messages.NoSubject));
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.trackTime> e)
		{
			PMTimeActivity row = e.Row;
			if (row == null) return;
			if (ProjectDefaultAttribute.IsNonProject(row.ProjectID)) return;

			bool isPmNotVisible = !ProjectAttribute.IsPMVisible(BatchModule.TA);
			if (row.TrackTime == false || isPmNotVisible)
			{
				e.Cache.SetValueExt<PMTimeActivity.projectID>(row, PM.ProjectDefaultAttribute.NonProject());
			}
		}

		protected virtual void CRSMEmail_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			var row = (CRSMEmail)e.Row;
			var tAct = (PMTimeActivity)TimeActivity.SelectSingle();

			PXDefaultAttribute.SetPersistingCheck<CRSMEmail.ownerID>(cache, row, !this.UnattendedMode && tAct?.TrackTime == true 
				&& row.MPStatus != MailStatusListAttribute.Deleted ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);

			if (e.Operation != PXDBOperation.Insert || e.TranStatus != PXTranStatus.Completed)
			{
				return;
			}
			using (PXDataRecord msg =
				PXDatabase.SelectSingle<SMEmail>(
					new PXDataField(typeof(SMEmail.id).Name),
					new PXDataFieldValue(typeof(SMEmail.noteID).Name, row.EmailNoteID)))
			{
				if (msg != null)
				{
					cache.SetValue<CRSMEmail.id>(e.Row, msg.GetInt32(0));
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.ownerID> e)
		{
			e.Cache.SetDefaultExt<PMTimeActivity.labourItemID>(e.Row);
		}
		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.costCodeID> e)
		{
			e.Cache.SetDefaultExt<PMTimeActivity.workCodeID>(e.Row);
		}
		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.projectID> e)
		{
			e.Cache.SetDefaultExt<PMTimeActivity.unionID>(e.Row);
			e.Cache.SetDefaultExt<PMTimeActivity.certifiedJob>(e.Row);
			e.Cache.SetDefaultExt<PMTimeActivity.labourItemID>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMTimeActivity, PMTimeActivity.costCodeID> e)
		{
			if (CostCodeAttribute.UseCostCode())
			{
				e.NewValue = CostCodeAttribute.DefaultCostCode;
			}
		}

		#endregion

		#region SendEmail


		public static void SendEmail(SendEmailParams sendEmailParams)
		{
            var graph = PXGraph.CreateInstance<CREmailActivityMaint>();
            graph.SendEmail(sendEmailParams, null);
		}

        protected virtual void SendEmail(SendEmailParams sendEmailParams, Action<CRSMEmail> handler)
		{
			var cache = Message.Cache;
			var activityCache = cache;
			var activityIsDirtyOld = activityCache.IsDirty;
			var newEmail = cache.NonDirtyInsert<CRSMEmail>(null);
			var owner = EP.EmployeeMaint.GetCurrentOwnerID(this);

			newEmail.MailTo = sendEmailParams.To;
			newEmail.MailCc = sendEmailParams.Cc;
			newEmail.MailBcc = sendEmailParams.Bcc;
			newEmail.Subject = sendEmailParams.Subject;

			newEmail.MPStatus = ActivityStatusListAttribute.Draft;
			activityCache.IsDirty = activityIsDirtyOld;
			cache.Current = newEmail;

			var sourceType = sendEmailParams.Source.With(s => s.GetType());
			var sourceCache = sourceType.With(type => this.Caches[type]);
			Guid? refNoteId = sourceCache.With(c => PXNoteAttribute.GetNoteID(c, sendEmailParams.Source, EntityHelper.GetNoteField(sourceType)));
			newEmail.RefNoteID = refNoteId;

			var parentSourceType = sendEmailParams.ParentSource.With(s => s.GetType());
			var parentSourceCache = parentSourceType.With(type => Activator.CreateInstance(BqlCommand.Compose(typeof(PXCache<>), type), this) as PXCache);

			EntityHelper helper = new EntityHelper(this);
			if (parentSourceCache != null)
			{
				var parentSource = helper.GetEntityRow(parentSourceCache.GetItemType(),
					 helper.GetEntityRowKeys(parentSourceCache.GetItemType(), sendEmailParams.ParentSource));

				var fieldName = EntityHelper.GetIDField(this.Caches[parentSourceType]);
				int? bAccountId = (int?)this.Caches[parentSourceType].GetValue(parentSource, fieldName);
				
				newEmail.BAccountID = bAccountId;
			}
			newEmail.Type = null;
			newEmail.IsIncome = false;
			var newBody = sendEmailParams.Body;
			newEmail.OwnerID = owner;
			newEmail.Subject = newEmail.Subject;
			if (!string.IsNullOrEmpty(sendEmailParams.TemplateID))
			{
				TemplateNotificationGenerator generator =
					 TemplateNotificationGenerator.Create(sendEmailParams.Source,
						  sendEmailParams.TemplateID);
				var template = generator.ParseNotification();

				if (string.IsNullOrEmpty(newBody))
					newBody = template.Body;
				if (string.IsNullOrEmpty(newEmail.Subject))
					newEmail.Subject = template.Subject;
				if (string.IsNullOrEmpty(newEmail.MailTo))
					newEmail.MailTo = template.To;
				if (string.IsNullOrEmpty(newEmail.MailCc))
					newEmail.MailCc = template.Cc;
				if (string.IsNullOrEmpty(newEmail.MailBcc))
					newEmail.MailBcc = template.Bcc;

				newEmail.MailAccountID = template.MailAccountId ?? MailAccountManager.DefaultMailAccountID;
				
				newEmail.MessageId = "<" + Guid.NewGuid() + "_acumatica" + GetMessageIDAppendix(cache.Graph, newEmail) + ">";

				if (template.AttachmentsID != null)
				{
					List<Guid> _attachmentLinks = new List<Guid>();

					foreach (NoteDoc doc in
						PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(this, template.AttachmentsID))
					{
						if (doc.FileID != null && !_attachmentLinks.Contains(doc.FileID.Value))
							_attachmentLinks.Add(doc.FileID.Value);
					}

					PXNoteAttribute.SetFileNotes(activityCache, newEmail, _attachmentLinks.ToArray());
				}
			}
			else
			{
				if (!IsHtml(newBody))
					newBody = Tools.ConvertSimpleTextToHtml(newBody);

				var html = new StringBuilder();
				html.AppendLine("<html><body>");
				html.Append(Tools.RemoveHeader(newBody));
				html.Append("<br/>");
				html.Append(Tools.RemoveHeader(newEmail.Body));
				html.Append("</body></html>");
				newBody = html.ToString();
			}

			newEmail.MailFrom = FillMailFrom(this, newEmail);
			newEmail.MailReply = FillMailReply(this, newEmail);
			newEmail.Body = PX.Web.UI.PXRichTextConverter.NormalizeHtml(newBody);

			if (sendEmailParams.Attachments.Count > 0)
			{
				AttachFiles(newEmail, refNoteId, cache, sendEmailParams.Attachments);
			}
			if (handler != null) handler(newEmail);
			Caches[newEmail.GetType()].RaiseRowSelected(newEmail);
			throw new PXPopupRedirectException(this, this.GetType().Name, true);
		}

		public static string FillMailReply(PXGraph graph, CRSMEmail message)
		{
			string defaultDisplayName = null;
			string defaultAddress = null;

			if (message.MailAccountID != null)
			{
				EMailAccount account =
					PXSelectReadonly<EMailAccount,
						Where<EMailAccount.emailAccountID, Equal<Required<CRSMEmail.mailAccountID>>>>.Select(graph, message.MailAccountID);


				if (account != null)
				{
					defaultDisplayName = account.Description.With(_ => _.Trim());
					defaultAddress = account.ReplyAddress.With(_ => _.Trim());
					if (string.IsNullOrEmpty(defaultAddress))
						defaultAddress = account.Address.With(_ => _.Trim());

					if (account.SenderDisplayNameSource == SenderDisplayNameSourceAttribute.Account)
						return string.IsNullOrEmpty(account.AccountDisplayName)
							? defaultAddress
							: new MailAddress(defaultAddress, account.AccountDisplayName).ToString();
				}
				else
				{
					MailAddress mailReply;
					EmailParser.TryParse(message.MailFrom, out mailReply);

					defaultAddress = mailReply?.Address;
				}
			}

			return GenerateBackAddress(graph,
				message,
				defaultDisplayName,
				defaultAddress,
				true);
		}


		public static string FillMailFrom(PXGraph graph, CRSMEmail message, bool allowUseCurrentUser = false)
		{
			string defaultDisplayName = null;
			string defaultAddress = null;

			if (message.MailAccountID != null)
			{
				EMailAccount account =
					PXSelectReadonly<EMailAccount,
						Where<EMailAccount.emailAccountID, Equal<Required<CRSMEmail.mailAccountID>>>>.Select(graph, message.MailAccountID);

				MailAddress mailFrom;

				EmailParser.TryParse(message.MailFrom, out mailFrom);

				defaultDisplayName = account?.Description.With(_ => _.Trim());
				defaultAddress = account != null 
					? account.Address.With(_ => _.Trim()) 
					: mailFrom?.Address;

				if (account != null && account.SenderDisplayNameSource == SenderDisplayNameSourceAttribute.Account)
					return string.IsNullOrEmpty(account.AccountDisplayName)
						? defaultAddress
						: new MailAddress(defaultAddress, account.AccountDisplayName).ToString();
			}

			return GenerateBackAddress(graph, 
				message,
				defaultDisplayName,
				defaultAddress,
				allowUseCurrentUser);
		}

		private static string GenerateBackAddress(PXGraph graph, CRSMEmail message, string defaultDisplayName, string defaultAddress, bool allowUseCurrentUser)
		{
			string result = null;

			if (message != null)
			{
				if (message.OwnerID == null || message.ClassID == CRActivityClass.EmailRouting)
			{
					return string.IsNullOrEmpty(defaultDisplayName)
					? defaultAddress
					: new MailAddress(defaultAddress, defaultDisplayName).ToString();
				}

				var results = PXSelectReadonly2<Contact,
					LeftJoin<Users,
						On<Users.pKID, Equal<Contact.userID>>>,
					Where<
						Contact.contactID, Equal<Required<CRSMEmail.ownerID>>>>.
					SelectWindowed(graph, 0, 1, message.OwnerID);

				if (results == null || results.Count == 0) return defaultAddress;

				var contact = (Contact)results[0][typeof(Contact)];
				var user = (Users)results[0][typeof(Users)];

				string displayName = null;
				string address = defaultAddress;
				if (user != null && user.PKID != null)
				{
					var userDisplayName = user.FullName.With(_ => _.Trim());
					if (!string.IsNullOrEmpty(userDisplayName))
						displayName = userDisplayName;
				}
				if (contact != null && contact.BAccountID != null)
				{
					var contactDisplayName = contact.DisplayName.With(_ => _.Trim());
					if (!string.IsNullOrEmpty(contactDisplayName))
						displayName = contactDisplayName;
				}

				result = string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(address)
					? address
					: new MailAddress(address, displayName).ToString();
			}

			if (string.IsNullOrEmpty(result) && allowUseCurrentUser)
			{
				return graph.Accessinfo.UserID.
					With(id => (Users)PXSelect<Users>.Search<Users.pKID>(graph, id)).
					With(u => PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(u.Email, u.FullName));
			}

			return result;
		}

		private static bool IsHtml(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				var htmlIndex = text.IndexOf("<html", StringComparison.CurrentCultureIgnoreCase);
				var bodyIndex = text.IndexOf("<body", StringComparison.CurrentCultureIgnoreCase);
				return htmlIndex > -1 && bodyIndex > -1 && bodyIndex > htmlIndex;
			}
			return false;
		}

		private static object[] GetKeys(object e, PXCache cache)
		{
			var keys = new List<object>();

			foreach (Type t in cache.BqlKeys)
				keys.Add(cache.GetValue(e, t.Name));

			return keys.ToArray();
		}
		
		public virtual void ValidateEmailFields(CRSMEmail row)
		{
			//From
			Message.Cache.RaiseExceptionHandling<CRSMEmail.mailAccountID>(row, null, null);
			if (row.MailAccountID == null)
			{
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty);
				Message.Cache.RaiseExceptionHandling<CRSMEmail.mailAccountID>(row, null, exception);
				PXUIFieldAttribute.SetError<CRSMEmail.mailAccountID>(Message.Cache, row, exception.Message);
				throw exception;
			}

			//To
			Message.Cache.RaiseExceptionHandling<CRSMEmail.mailTo>(row, null, null);
			if (string.IsNullOrWhiteSpace(row.MailTo) && string.IsNullOrWhiteSpace(row.MailCc) && string.IsNullOrWhiteSpace(row.MailBcc))
			{
				var exception = new PXSetPropertyException(Messages.RecipientsNotFound);
				throw exception;
			}
		}
		#endregion

		#region Private Methods

		private Int32? GetDefaultAccountId(int? owner)
		{
			if (owner != null)
			{
				int? result = MailAccountManager.GetDefaultMailAccountID((int)owner, true);
				if (result != null && MailAccountManager.GetEmailAccountIfAllowed(this, result) != null)
					return result;
			}

			return MailAccountManager.GetEmailAccountIfAllowed(this, MailAccountManager.DefaultAnyMailAccountID) != null
				? MailAccountManager.DefaultAnyMailAccountID
				: null;
		}

		public virtual string GetReplyAddress(CRSMEmail oldMessage)
		{
			var newAddressList = new List<MailAddress>();

			if (oldMessage.MailReply != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailReply))
				{
					string displayName = null;
					if (String.IsNullOrWhiteSpace(item.DisplayName) && oldMessage.MailFrom != null)
					{
						foreach (var item1 in EmailParser.ParseAddresses(oldMessage.MailCc)
							.Where(_ => string.Equals(_.Address, item.Address, StringComparison.OrdinalIgnoreCase)))
						{
							displayName = item1.DisplayName;
						}
					}
					else
					{
						displayName = item.DisplayName;
					}
					var newitem = new MailAddress(item.Address, displayName);
					newAddressList.Add(newitem);
				}
			}

			if (newAddressList.Count == 0 && oldMessage.MailFrom != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailFrom))
				{
					newAddressList.Add(new MailAddress(item.Address, item.DisplayName));
				}
			}

			return PXDBEmailAttribute.ToString(newAddressList);
		}

		public string GetMailAccountAddress(CRSMEmail oldMessage)
		{
			return oldMessage.MailAccountID.
				With(_ => (EMailAccount)PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
				Select(this, _.Value)).
				With(_ => _.Address);
		}

		private static string GetMessageIDAppendix(PXGraph graph, CRSMEmail message)
		{
			if (message.MailAccountID == null) return ""; 

			var hostName = message.MailAccountID.
				With(_ => (EMailAccount) PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
					Select(graph, _.Value)).
				With(_ => _.OutcomingHostName);

			if (hostName == null) return "";

			return "@" + hostName;
		}

		public virtual string GetReplyAllCCAddress(CRSMEmail oldMessage, string mailAccountAddress)
		{
			var newAddressList = new List<MailAddress>();

			if (oldMessage.MailCc != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailCc)
					.Where(_ => !string.Equals(_.Address, mailAccountAddress, StringComparison.OrdinalIgnoreCase)))
				{
					newAddressList.Add(new MailAddress(item.Address, item.DisplayName));
				}
			}
			
			return PXDBEmailAttribute.ToString(newAddressList);
		}

		public virtual string GetReplyAllBCCAddress(CRSMEmail oldMessage, string mailAccountAddress)
		{
			var newAddressList = new List<MailAddress>();

			if (oldMessage.MailBcc != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailBcc)
					.Where(_ => !string.Equals(_.Address, mailAccountAddress, StringComparison.OrdinalIgnoreCase)))
				{
					newAddressList.Add(new MailAddress(item.Address, item.DisplayName));
				}
			}
			
			return PXDBEmailAttribute.ToString(newAddressList);
		}

		public virtual string GetReplyAllAddress(CRSMEmail oldMessage, string mailAccountAddress)
		{
			var newAddressList = new List<MailAddress>();

			if (oldMessage.MailReply != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailReply))
				{
					string displayName = null;
					if (String.IsNullOrEmpty(item.DisplayName) && oldMessage.MailFrom != null)
					{
						foreach (var item1 in EmailParser.ParseAddresses(oldMessage.MailTo)
							.Where(_ => string.Equals(_.Address, item.Address, StringComparison.OrdinalIgnoreCase)))
						{
							displayName = item1.DisplayName;
						}
					}
					else
					{
						displayName = item.DisplayName;
					}
					newAddressList.Add(new MailAddress(item.Address, displayName));
				}
			}

			if (newAddressList.Count == 0 && oldMessage.MailFrom != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailFrom))
				{
					newAddressList.Add(new MailAddress(item.Address, item.DisplayName));
				}
			}

			if (oldMessage.MailTo != null)
			{
				foreach (var item in EmailParser.ParseAddresses(oldMessage.MailTo)
					.Where(_ => !string.Equals(_.Address, mailAccountAddress, StringComparison.OrdinalIgnoreCase)))
				{
					newAddressList.Add(new MailAddress(item.Address, item.DisplayName));
				}
			}
			
			return PXDBEmailAttribute.ToString(newAddressList);
		}

		private static string GetSubjectPrefix(string subject, bool forward)
		{
			bool startWith;
			if (subject != null)
				do
				{
					startWith = false;
					if (subject.ToUpper().StartsWith("RE: ") || subject.ToUpper().StartsWith("FW: "))
					{
						subject = subject.Substring(4);
						startWith = true;
					}
					if (subject.ToUpper().StartsWith(PXMessages.LocalizeNoPrefix(Messages.EmailReplyPrefix).ToUpper()))
					{
						subject = subject.Substring(Messages.EmailReplyPrefix.Length);
						startWith = true;
					}
					if (subject.ToUpper().StartsWith(PXMessages.LocalizeNoPrefix(Messages.EmailForwardPrefix).ToUpper()))
					{
						subject = subject.Substring(Messages.EmailForwardPrefix.Length);
						startWith = true;
					}
				} while (startWith);
			return (forward ? "FW: " : "RE: ") + subject;
		}

		protected virtual string CreateReplyBody(string mailFrom, string mailTo, string subject, string message, DateTime lastModifiedDateTime)
		{
			return CreateReplyBody(mailFrom, mailTo, null, subject, message, lastModifiedDateTime);
		}

		private string CreateReplyBody(string mailFrom, string mailTo, string mailCc, string subject, string message, DateTime lastModifiedDateTime)
		{
			var wikiTitle =
				 "<br/><br/><div class=\"wiki\" style=\"border-top:solid 1px black;padding:2px 0px;line-height:1.5em;\">" +
				 "\r\n<b>From:</b> " + mailFrom +
				 "<br/>\r\n<b>Sent:</b> " + lastModifiedDateTime +
				 "<br/>\r\n<b>To:</b> " + mailTo +
				 (string.IsNullOrEmpty(mailCc) ? "" : "<br/>\r\n<b>Cc:</b> " + mailCc) +
				 "<br/>\r\n<b>Subject:</b> " + subject +
				 "<br/><br/>\r\n</div>";

            return PXRichTextConverter.NormalizeHtml(MailAccountManager.GetSignature(this, MailAccountManager.SignatureOptions.Default) + wikiTitle + message);
		}

		private void TryCorrectMailDisplayNames(CRSMEmail message)
		{
			if(TryCorrectMailDisplayNames(this, message))
				Caches[message.GetType()].Update(message);
		}

		internal static bool TryCorrectMailDisplayNames(PXGraph graph, CRSMEmail message)
		{
			string ownerEmail = FillMailFrom(graph, message);
			string ownerDisplayName = null;

			MailAddress fromBox;
			if (ownerEmail != null && EmailParser.TryParse(ownerEmail, out fromBox))
				ownerDisplayName = fromBox.DisplayName;

			if (ownerDisplayName == null)
			{
				ownerDisplayName = PXSelect<EMailAccount>
					.Search<EMailAccount.emailAccountID>(graph, message.MailAccountID)
					.FirstOrDefault()
					?.GetItem<EMailAccount>()
					?.Description;
			}

			bool wasCorrected = false;

			//from			
			var fromAddress = message.MailFrom;
			if (!string.IsNullOrEmpty(fromAddress) &&
				EmailParser.TryParse(fromAddress, out fromBox))
			{
				message.MailFrom = new MailAddress(fromBox.Address, ownerDisplayName).ToString();
				wasCorrected = true;
			}

			//reply
			MailAddress replyBox;
			var replyAddress = message.MailReply;
			if (!string.IsNullOrEmpty(replyAddress) &&
				EmailParser.TryParse(replyAddress, out replyBox) &&
				!object.Equals(replyBox.DisplayName, ownerDisplayName))
			{
				message.MailReply = new MailAddress(replyBox.Address, ownerDisplayName).ToString();
				wasCorrected = true;
			}

			return wasCorrected;
		}
		

		private void CorrectFileNames()
		{
			var noteId = Message.Current.With(m => m.NoteID);
			var actNoteId = Message.Current.With(act => act.NoteID);
			if (noteId == null || actNoteId == null) return;

			var searchText = "[" + Message.Current.MessageId + "]";
			var replaceText = "[" + Message.Current.NoteID + "]";
			var cache = Caches[typeof(UploadFile)];
			PXSelectJoin<UploadFile,
					InnerJoin<NoteDoc, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
					Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.
					Clear(this);
			foreach (UploadFile file in
				PXSelectJoin<UploadFile,
					InnerJoin<NoteDoc, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
					Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.
				Select(this, noteId))
			{
				if (!string.IsNullOrEmpty(file.Name) && file.Name.Contains(searchText))
				{
					file.Name = file.Name.Replace(searchText, replaceText);
					cache.PersistUpdated(file);
				}
			}
		}

		private string GetEntityDescription(CRActivity row)
		{
			string res = string.Empty;
			var helper = new EntityHelper(this);
			var entity = row.RefNoteID.With(_ => helper.GetEntityRow(_.Value, true));

			if (entity != null)
			{
				res = CacheUtility.GetDescription(helper, entity, entity.GetType());
			}

			return res;
		}

		protected static void AttachFiles(CRSMEmail newEmail, Guid? refNoteId, PXCache cache, IEnumerable<FileInfo> files)
		{
			var uploadFile = PXGraph.CreateInstance<UploadFileMaintenance>();
			var filesID = new List<Guid>();
			uploadFile.IgnoreFileRestrictions = true;
			foreach (FileInfo file in files)
			{
				var separator = file.FullName.IndexOf('\\') > -1 ? string.Empty : "\\";
				file.FullName = string.Format("[{0}] {2}{3}", newEmail.ImcUID, refNoteId, separator, file.FullName);
				uploadFile.SaveFile(file, FileExistsAction.CreateVersion);
				var uid = (Guid)file.UID;
				if (!filesID.Contains(uid))
					filesID.Add(uid);
			}
			cache.SetValueExt(newEmail, "NoteFiles", filesID.ToArray());
		}

		private void CopyAttachments(PXGraph targetGraph, CRSMEmail message, CRSMEmail newMessage, bool isReply)
		{
			if (message == null || newMessage == null) return;

			var filesIDs = PXNoteAttribute.GetFileNotes(Message.Cache, message);
			if (filesIDs == null || filesIDs.Length == 0) return;

			if (isReply && !String.IsNullOrEmpty(message.Body))
			{
				var t = PXSelectJoin<UploadFile,
						InnerJoin<NoteDoc, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
						Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.
					Select(this, message.NoteID).RowCast<UploadFile>().ToList();

				var extractor = new ImageExtractor();
				if(!extractor.FindImages(message.Body, t, out var found))
				{
					return;
				}
				filesIDs = found.ToArray();
			}

			PXNoteAttribute.SetFileNotes(targetGraph.Caches<CRSMEmail>(), newMessage, filesIDs);
		}

		private EPSetup EPSetupCurrent
		{
			get
			{
				return (EPSetup)PXSelect<EPSetup>.SelectWindowed(this, 0, 1) ?? EmptyEpSetup;
			}
		}
		private static readonly EPSetup EmptyEpSetup = new EPSetup();

		public class EmailAddress
		{
			public string FirstName { get; set; }
			public string LastName { get; set; }
			public string Email { get; set; }
		}

		public static EmailAddress ParseNames(string source)
		{
			List<string> patterns = new List<string>
			{
				@"'(?<first>[\w.@]+)\s+(?<last>[\w.@]+).*'\s+<(?<email>[^<>]+)>",
				@"'(?<last>[\w.@]+),\s+(?<first>[\w.@]+).*'\s+<(?<email>[^<>]+)>",
				@"""(?<first>[\w.@]+)\s+(?<last>[\w.@]+).*""\s+<(?<email>[^<>]+)>",
				@"""(?<last>[\w.@]+),\s+(?<first>[\w.@]+).*""\s+<(?<email>[^<>]+)>",
				@"(?<first>[\w.@]+)\s+(?<last>[\w.@]+).*\s+<(?<email>[^<>]+)>",
				@"(?<last>[\w.@]+),\s+(?<first>[\w.@]+).*\s+<(?<email>[^<>]+)>",
				@"'(?<last>[\w.@]+)'\s+<(?<email>[^<>]+)>",
				@"""(?<last>[\w.@]+)""\s+<(?<email>[^<>]+)>",
				@"(?<last>[\w.@]+)\s+<(?<email>[^<>]+)>",
				@"(?<email>[\w@.-]+)",
			};
			return (patterns.Select(pattern => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline))
								 .Select(re => re.Match(source).Groups)
								 .Select(groups => new { groups, email_grp = groups["email"] })
								 .Where(@t => @t.email_grp.Success)
								 .Select(@t => new EmailAddress()
								 {
									 FirstName = @t.groups["first"].Value,
									 LastName = @t.groups["last"].Value,
									 Email = @t.email_grp.Value
								 })).FirstOrDefault();
		}

		public class EntityList : PXStringListAttribute
		{
			public EntityList()
				: base(
					 new string[] { Event, Task, Lead, Contact, Case, Opportunity, ExpenseReceipt },
					 new string[] { EP.Messages.Event, EP.Messages.Task, Messages.Lead, Messages.Contact, Messages.Case, Messages.Opportunity, EP.Messages.ExpenseReceipt }) { }
		}

		public class EntityListSimple : PXStringListAttribute
		{
			public EntityListSimple()
				: base(
					 new string[] { Event, Task },
					 new string[] { EP.Messages.Event, EP.Messages.Task }) { }
		}

		public const string Event = "E";
		public const string Task = "T";
		public const string Lead = "L";
		public const string Contact = "C";
		public const string Case = "S";
		public const string Opportunity = "O";
		public const string ExpenseReceipt = "R";

		protected Dictionary<string, Type> GraphTypes = new Dictionary<string, Type>()
			{
				{Event, typeof(EPEventMaint)},
				{Task,typeof(CRTaskMaint)}, 
				{Lead, typeof(LeadMaint)},
				{Contact, typeof(ContactMaint)},
				{Case, typeof(CRCaseMaint)},
				{Opportunity, typeof(OpportunityMaint)},
                {ExpenseReceipt, typeof(ExpenseClaimDetailEntry)}
			};
		#endregion

		#region Helpers

		internal void InsertFile(FileDto file)
		{
			_extractor.InsertFile(file);
		}

		#endregion
	}
}
