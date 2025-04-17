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
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.EP;
using PX.SM;
using PX.TM;
using PX.Objects.CS;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.Extensions;

namespace PX.Objects.CR
{
	public class CRMassMailMaint : PXGraph<CRMassMailMaint, CRMassMail>
	{
		#region Selects

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSetup<CRSetup>
			Setup;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<Contact>
			BaseContacts;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<BAccount>
			BAccount;

		[PXViewName(Messages.MassMailSummary)]
		public PXSelect<CRMassMail>
			MassMails;

		[PXViewName(Messages.History)]
		[PXFilterable]
		[PXCopyPasteHiddenView]
		[PXViewDetailsButton]
		[PXViewDetailsButton(typeof(CRMassMail),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CRSMEmail.contactID>>>>),
			ActionName = "History_Contact_ViewDetails")]
		[PXViewDetailsButton(typeof(CRMassMail),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<CRSMEmail.bAccountID>>>>),
			ActionName = "History_BAccount_ViewDetails")]
		public PXSelectJoin<CRSMEmail,
			InnerJoin<CRMassMailMessage,
				On<CRMassMailMessage.messageID, Equal<CRSMEmail.imcUID>>>,
			Where<CRMassMailMessage.massMailID, Equal<Optional<CRMassMail.massMailID>>>>
			History;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<CRMassMailMessage,
			Where<CRMassMailMessage.massMailID, Equal<Current<CRMassMail.massMailID>>>>
			SendedMessages;

		[PXViewName(Messages.EntityFields)]
		[PXCopyPasteHiddenView]
		public PXSelectOrderBy<CacheEntityItem,
			OrderBy<Asc<CacheEntityItem.number>>>
			EntityItems;
		protected virtual IEnumerable entityItems(string parent)
		{
			PXSiteMapNode siteMapNode = PXSiteMap.Provider.FindSiteMapNode(typeof(ContactMaint));

			if (siteMapNode != null)
			{
				foreach (var entry in EMailSourceHelper.TemplateEntity(this, parent, null, siteMapNode.GraphType, true).OfType<CacheEntityItem>())
				{
					if (entry.SubKey == typeof(Contact).FullName || parent == typeof(Contact).Name)
					{
						yield return entry;
					}
				}
			}
		}

		[PXViewName(Messages.MailLists)]
		[PXViewDetailsButton(typeof(CRMarketingList),
			ActionName = "MassEmailMarketingLists_CRMarketingList_ViewDetails",
			WindowMode = PXRedirectHelper.WindowMode.New)]
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<CRMarketingList,
			LeftJoin<CRMassMailMarketingList,
				On<CRMassMailMarketingList.mailListID, Equal<CRMarketingList.marketingListID>,
					And<CRMassMailMarketingList.massMailID, Equal<Current<CRMassMail.massMailID>>>>>,
			Where<CRMarketingList.status, Equal<CRMarketingList.status.active>>,
			OrderBy<Asc<CRMarketingList.name>>>
			MailLists;
		protected virtual IEnumerable mailLists()
		{
			foreach (PXResult row in MailListsExt.Select())
			{
				var rec = (CRMarketingList)row[typeof(CRMarketingList)];
				var mailList = (CRMassMailMarketingList)row[typeof(CRMassMailMarketingList)];
				CRMarketingList cache = (CRMarketingList)this.MailLists.Cache.Locate(rec);

				if (rec.Selected != true && mailList.MailListID != null)
					rec.Selected = true;

				if (cache != null)
				{
					bool? selected = cache.Selected;
					MailLists.Cache.RestoreCopy(cache, rec);
					cache.Selected = selected;
					rec = cache;
				}
				yield return new PXResult<CRMarketingList>(rec);
			}
		}

		[PXCopyPasteHiddenView]
		public PXSelectReadonly2<CRMarketingList,
				LeftJoin<CRMassMailMarketingList,
					On<CRMassMailMarketingList.mailListID, Equal<CRMarketingList.marketingListID>,
						And<CRMassMailMarketingList.massMailID, Equal<Current<CRMassMail.massMailID>>>>>,
				Where<CRMarketingList.status, Equal<CRMarketingList.status.active>>,
				OrderBy<Asc<CRMarketingList.name>>>
			MailListsExt;

		[PXViewName(Messages.Campaigns)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(CRMarketingList),
			ActionName = "MassEmailCampaign_CRMarketingCampaign_ViewDetails",
			WindowMode = PXRedirectHelper.WindowMode.New)]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<CRCampaign,
			LeftJoin<CRMassMailCampaign,
				On<CRMassMailCampaign.campaignID, Equal<CRCampaign.campaignID>,
					And<CRMassMailCampaign.massMailID, Equal<Current<CRMassMail.massMailID>>>>>,
			Where<CRCampaign.isActive, Equal<boolTrue>>,
			OrderBy<Asc<CRCampaign.campaignName>>>
			Campaigns;
		protected virtual IEnumerable campaigns()
		{
			foreach (PXResult row in Campaigns.View.QuickSelect())
			{
				var campaign = (CRCampaign)row[typeof(CRCampaign)];
				var mailCampaign = (CRMassMailCampaign)row[typeof(CRMassMailCampaign)];
				if (campaign.Selected != true && mailCampaign.CampaignID != null &&
					Campaigns.Cache.GetStatus(campaign) != PXEntryStatus.Updated)
				{
					campaign.Selected = true;
				}
				yield return new PXResult<CRCampaign>(campaign);
			}
		}

		[PXViewName(Messages.LeadsAndContacts)]
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public SelectFrom<Contact>
		.LeftJoin<BAccount>.On<
			Brackets<
				BAccount.bAccountID.IsEqual<Contact.bAccountID>
				.And<BAccount.type.IsNotEqual<BAccountType.employeeType>>
			>
			.Or<
				BAccount.defContactID.IsEqual<Contact.contactID>
				.And<BAccount.type.IsEqual<BAccountType.employeeType>>
			>
		>
		.LeftJoin<Address>.On<
			Address.addressID.IsEqual<Contact.defAddressID>
		>
		.LeftJoin<CRMassMailMember>.On<
			CRMassMailMember.contactID.IsEqual<Contact.contactID>
			.And<CRMassMailMember.massMailID.IsEqual<CRMassMail.massMailID.FromCurrent>>
		>
		.LeftJoin<CRLead>.On<
			CRLead.contactID.IsEqual<Contact.contactID>
		>
		.Where<
			Brackets<
				Contact.noMassMail.IsNull
				.Or<Contact.noMassMail.IsNotEqual<True>>
			>
			.And<
				Contact.noEMail.IsNull
				.Or<Contact.noEMail.IsNotEqual<True>>
			>
			.And<
				Contact.noMarketing.IsNull
				.Or<Contact.noMarketing.IsNotEqual<True>>
			>
			.And<
				BAccount.bAccountID.IsNull
				.Or<BAccount.type.IsIn<
					BAccountType.customerType,
					BAccountType.prospectType,
					BAccountType.combinedType,
					BAccountType.vendorType,
					BAccountType.employeeType>>
			>
		>
		.OrderBy<Contact.displayName.Asc, Contact.contactID.Asc>
		.View
			Leads;

		protected virtual IEnumerable leads()
		{
			foreach (PXResult row in Leads.View.QuickSelect())
			{
				var contact = row.GetItem<Contact>();
				var mailLead = row.GetItem<CRMassMailMember>();
				if (contact.Selected != true && mailLead.ContactID != null &&
					Leads.Cache.GetStatus(contact) != PXEntryStatus.Updated)
				{
					contact.Selected = true;
				}
				yield return new PXResult<Contact, BAccount, Address, CRLead>(
					contact,
					row.GetItem<BAccount>(),
					row.GetItem<Address>(),
					row.GetItem<CRLead>()
				);
			}
		}

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<CRMassMailMarketingList,
			Where<CRMassMailMarketingList.massMailID, Equal<Required<CRMassMail.massMailID>>>>
			selectedMailList;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<CRMassMailCampaign,
			Where<CRMassMailCampaign.massMailID, Equal<Required<CRMassMail.massMailID>>>>
			selectedCampaigns;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<CRMassMailMember,
			Where<CRMassMailMember.massMailID, Equal<Required<CRMassMail.massMailID>>>>
			selectedLeads;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<Contact,
			InnerJoin<CRMarketingListMember,
				On<CRMarketingListMember.contactID, Equal<Contact.contactID>>>> DynamicSourceList;
		protected virtual IEnumerable dynamicSourceList([PXInt] int mailListID)
		{
			return CRSubscriptionsSelect.Select(this, mailListID);
		}

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<Contact, Where<True, Equal<False>>> Contact;

		public PXFilter<CRMassMailPrepare> MassMailPrepare;

		#endregion

		#region Ctors

		public CRMassMailMaint()
		{
			if (string.IsNullOrEmpty(Setup.Current.MassMailNumberingID))
				throw new PXSetPropertyException(Messages.NumberingIDIsNull, Messages.CRSetup);


			PXUIFieldAttribute.SetEnabled(Campaigns.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRCampaign.selected>(Campaigns.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<CRCampaign.sendFilter>(Campaigns.Cache, null, true);
			PXUIFieldAttribute.SetDisplayName<Contact.fullName>(Leads.Cache, Messages.ContactFullName);
			PXUIFieldAttribute.SetDisplayName<BAccount.classID>(Caches[typeof(BAccount)], Messages.CompanyClass);
			PXDBAttributeAttribute.Activate(BaseContacts.Cache);
		}

		#endregion

		#region Event Handlers

		[PXSelector(typeof(Contact.contactID), DescriptionField = typeof(Contact.memberName))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRSMEmail.contactID> e) { }

		[PXSelector(typeof(BAccount.bAccountID), DescriptionField = typeof(BAccount.acctName))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRSMEmail.bAccountID> e) { }

		[PXFormula(typeof(EntityDescription<CRSMEmail.documentNoteID>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRSMEmail.documentSource> e) { }

		[PXUIField(DisplayName = "Display Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBString(255, IsUnicode = true)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof(Search<Contact.displayName>))]
		protected virtual void _(Events.CacheAttached<Contact.displayName> e) { }

		[Owner(typeof(Contact.workgroupID), Visibility = PXUIVisibility.SelectorVisible)]
		[PXChildUpdatable(AutoRefresh = true, TextField = "AcctName", ShowHint = false)]
		protected virtual void _(Events.CacheAttached<Contact.ownerID> e) { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Class ID")]
		[PXNavigateSelector(typeof(CRContactClass.classID), DescriptionField = typeof(CRContactClass.description), CacheGlobal = true)]
		protected virtual void _(Events.CacheAttached<Contact.classID> e) { }

		[PXDefault("((Contact.Email))")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRMassMail.mailTo> e) { }

		protected virtual void _(Events.FieldUpdated<CRCampaign, CRCampaign.selected> e)
		{
			CRCampaign row = (CRCampaign)e.Row;
			if (row != null && !(bool)e.OldValue && (bool)row.Selected)
			{
				foreach (CRCampaign item in Campaigns.Select())
				{
					if (item.Selected == true && item != row)
					{
						e.Cache.SetValue<CRCampaign.selected>(item, false);
						e.Cache.SetStatus(item, PXEntryStatus.Updated);
					}
				}
				Campaigns.View.RequestRefresh();
			}
		}


		protected virtual void _(Events.RowSelected<CRMassMail> e)
		{
			var row = (CRMassMail)e.Row;
			if (row == null) return;

			CorrectUI(e.Cache, row);
		}

		protected virtual void _(Events.RowSelected<CRMassMailPrepare> e)
		{
			CRMassMailPrepare row = e.Row;
			if (row == null) return;

			PXUIFieldAttribute.SetVisible<CRMassMailPrepare.campaignUpdateListMembers>(e.Cache, row, MassMails.Current?.Source == CRMassMailSourcesAttribute.Campaign);
		}

		protected virtual void _(Events.RowPersisting<CRMarketingList> e)
		{
			e.Cancel = true;
		}

		protected virtual void _(Events.RowPersisting<Contact> e)
		{
			e.Cancel = true;
		}

		protected virtual void _(Events.RowPersisting<CRCampaign> e)
		{
			e.Cancel = true;
		}

		#endregion

		#region Overrides

		public override void Persist()
		{
			saveMailLists();
			saveCampaigns();
			saveLeads();

			base.Persist();

			CorrectUI(MassMails.Cache, MassMails.Current);
		}

		#endregion

		#region Actions

		public PXAction<CRMassMail> PreviewMail;
		[PXUIField(DisplayName = Messages.PreviewMessage)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable previewMail(PXAdapter a)
		{
			// todo: probably should be IQueryable to optimize FirstOrDefault request
			var recipient = EnumerateRecipientsForSending(true).FirstOrDefault();
			if (recipient == null) throw new PXException(MessagesNoPrefix.NoRecipientMassEmail);

			this.Caches[typeof(Contact)].Current = recipient.Contact;

			var massMail = PXCache<CRMassMail>.CreateCopy(MassMails.Current);
			massMail.MailAccountID = MassMails.Current.MailAccountID ?? MailAccountManager.DefaultMailAccountID;
			massMail.MailTo = MailAccountManager.GetDefaultEmailAccount(false)?.Address;
			massMail.MailCc = null;
			massMail.MailBcc = null;

			IEnumerable<CRSMEmail> messages = new Recipient(recipient.Contact, recipient.Format).GetSender(this, massMail).MailMessages();

			var testMessage = messages.FirstOrDefault();

			testMessage.MPStatus = MailStatusListAttribute.Draft;

			AddSendedMessages(massMail, messages);
			Actions.PressSave();

			CREmailActivityMaint graph = CreateInstance<CREmailActivityMaint>();
			graph.Message.Current = graph.Message.Insert(testMessage);
			graph.Message.Cache.IsDirty = false;
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			yield return MassMails.Current;
		}

		public PXAction<CRMassMail> Send;
		[PXUIField(DisplayName = Messages.Send)]
		[PXSendMailButton]
		public virtual IEnumerable send(PXAdapter a)
		{
			CheckFields(MassMails.Cache, MassMails.Current,
						typeof(CRMassMail.mailAccountID),
						typeof(CRMassMail.mailSubject),
						typeof(CRMassMail.mailTo),
						typeof(CRMassMail.plannedDate));

			SendMails();

			yield return MassMails.Current;
		}

		public void MassMailsPrepare()
		{
			if (MassMails.Current.Source == CRMassMailSourcesAttribute.Campaign)
			{
				CampaignMaint graph = PXGraph.CreateInstance<CampaignMaint>();

				foreach (CRCampaign campaign in Campaigns.Select().RowCast<CRCampaign>().Where(campaign => campaign.Selected == true))
				{
					try
					{
						graph.Campaign.Current = campaign;

						var selectedMarketingLists =
							graph.CampaignMarketingLists
								.SelectMain()
								.Where(marketingList => marketingList.SelectedForCampaign == true)
								.RowCast<CRMarketingList>()
								.ToList();

						graph.CampaignUpdateListMembers(campaign, selectedMarketingLists);
					}
					catch (Exception e)
					{
						throw new PXException(Messages.FailedToPrepareMassEmail, e);
					}
				}
			}
		}

		public PXAction<CRMassMail> MessageDetails;
		[PXUIField(Visible = false)]
		[PXButton]
		public virtual IEnumerable messageDetails(PXAdapter a)
		{
			PXRedirectHelper.TryOpenPopup(History.Cache, History.Current, string.Empty);

			yield return MassMails.Current;
		}

		public PXAction<SMEmail> ViewEntity;
		[PXUIField(DisplayName = "", Visible = false)]
		[PXButton]
		protected virtual IEnumerable viewEntity(PXAdapter adapter)
		{
			var row = History.Current;
			if (row != null)
			{
				new EntityHelper(this).NavigateToRow(row.RefNoteID, PXRedirectHelper.WindowMode.NewWindow);
			}

			return adapter.Get();
		}

		public PXAction<SMEmail> ViewDocument;
		[PXUIField(DisplayName = "", Visible = false)]
		[PXButton]
		protected virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			var row = History.Current;
			if (row != null)
			{
				new EntityHelper(this).NavigateToRow(row.DocumentNoteID, PXRedirectHelper.WindowMode.NewWindow);
			}

			return adapter.Get();
		}

		#endregion

		#region Private Methods

		protected virtual void CorrectUI(PXCache cache, CRMassMail row)
		{
			if (row == null) return;

			var isEnabled = row.Status != CRMassMailStatusesAttribute.Send;
			PXUIFieldAttribute.SetEnabled<CRMassMail.massMailID>(MassMails.Cache, row);
			PXUIFieldAttribute.SetEnabled<CRMassMail.mailAccountID>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.mailSubject>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.mailTo>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.mailCc>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.mailBcc>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.mailContent>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.source>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.sourceType>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.plannedDate>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.status>(MassMails.Cache, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<CRMassMail.sentDateTime>(MassMails.Cache, row, false);
			MailLists.Cache.AllowUpdate = isEnabled;
			Leads.Cache.AllowUpdate = isEnabled;
			Campaigns.Cache.AllowUpdate = isEnabled;

			var isNotInserted = cache.GetStatus(row) != PXEntryStatus.Inserted;
			Send.SetEnabled(isEnabled && isNotInserted);
			PreviewMail.SetEnabled(isEnabled && isNotInserted);
		}

		protected virtual void SendMails()
		{
			if (MassMails.Current == null
				|| MassMails.Current.Status == CRMassMailStatusesAttribute.Send)
				return;

			if (MassMailPrepare.AskExt(true) == WebDialogResult.OK)
			{
				Save.Press();

				var massMail = PXCache<CRMassMail>.CreateCopy(MassMails.Current);

				var graph = this.CloneGraphState();

				PXLongOperation.StartOperation(this, () =>
				{
					if (graph.MassMailPrepare.Current.CampaignUpdateListMembers == true)
					{
						graph.MassMailsPrepare();
					}

					var list = graph.GetRecipientsForSendingDistinctByEmail();

					if (list.Count == 0)
					{
						throw new PXException(Messages.RecipientsNotFound);
					}

					graph.ProcessMassMailEmails(graph.MassMails.Current, list);
				});
			}
			else
			{
				this.Caches<CRLead>().Clear();
				this.Caches<Contact>().Clear();
				this.Caches<BAccount>().Clear();
				this.Caches<EPEmployee>().Clear();
				this.Caches<CRMarketingListMember>().Clear();
				this.Caches<CRMarketingList>().Clear();
				this.Caches<CRCampaign>().Clear();
				return;
			}
		}

		protected virtual void ProcessMassMailEmails(CRMassMail massMail, IEnumerable<Recipient> recipients)
		{
			this.EnsureCachePersistence(typeof(Note));

			foreach (Recipient recipient in recipients)
			{
				var cache = this.Caches[recipient.Entity.GetType()];
				cache.SetStatus(recipient.Entity, PXEntryStatus.Notchanged);

				IEnumerable<CRSMEmail> messages;
				try
				{
					this.Contact.Current = recipient.Contact;

					messages = recipient.GetSender(this, massMail).Send();
				}
				catch (Exception e)
				{
					PXTrace.WriteError(new PXException(Messages.FailedToSendMassEmail, e));

					continue;
				}

				this.AddSendedMessages(massMail, messages);
			}

			this.MassMails.Current = massMail;
			this.MassMails.Current.Status = CRMassMailStatusesAttribute.Send;
			this.MassMails.Current.SentDateTime = DateTime.Now;
			this.MassMails.UpdateCurrent();
			this.Actions.PressSave();
		}

		protected virtual void AddSendedMessages(CRMassMail massMail, IEnumerable<CRSMEmail> messages)
		{
			foreach (CRSMEmail message in messages)
			{
				SendedMessages.Insert(
					new CRMassMailMessage
					{
						MassMailID = massMail.MassMailID,
						MessageID = message.ImcUID,
					});
			}
		}

		protected virtual List<Recipient> GetRecipientsForSendingDistinctByEmail()
		{
			return EnumerateRecipientsForSending(false)
				.GroupBy(i => i.Contact.EMail)
				.Select(i => i.OrderByDescending(c => c.Contact.ContactPriority /*leads first*/).First())
				.ToList();
		}

		protected virtual IEnumerable<Recipient> EnumerateRecipientsForSending(bool allowEmptyMails)
		{
			var helper = new EntityHelper(this);
			IEnumerable<Recipient> recipients;
			switch (MassMails.Current.Source)
			{
				case CRMassMailSourcesAttribute.MailList:
					recipients = EnumerateRecipientsForMailList(helper);
					break;

				case CRMassMailSourcesAttribute.Campaign:
					recipients = EnumerateRecipientsForCampaign(helper);
					break;

				case CRMassMailSourcesAttribute.Lead:
					recipients = EnumerateRecipientsForLeads(helper);
					break;

				default:
					yield break;
			}
			foreach (var recipient in recipients)
			{
				if (!allowEmptyMails && string.IsNullOrEmpty(recipient.Contact.EMail))
					continue;
				yield return recipient;
			}
		}

		protected virtual IEnumerable<Recipient> EnumerateRecipientsForMailList(EntityHelper helper)
		{
			ResetView(MailListsExt.View);

			foreach (CRMarketingList list in MailLists.Select().RowCast<CRMarketingList>().Where(l => l.Selected == true))
			{
				var marketingGraph = PXGraph.CreateInstance<CRMarketingListMaint>();
				marketingGraph.MailLists.Current = list;

				foreach (PXResult row in marketingGraph.ListMembers.Select())
				{
					var contact = row.GetItem<Contact>();
					var subscription = row.GetItem<CRMarketingListMember>();
					var lead = row.GetItem<CRLead>();

					if (subscription?.IsSubscribed != true
						|| (contact.NoMassMail | contact.NoEMail | contact.NoMarketing) == true)
						continue;

					yield return Recipient.SmartCreate(helper, contact, entity: list, lead: lead, format: subscription?.Format ?? NotificationFormat.Html);
				}
			}
		}

		protected virtual IEnumerable<Recipient> EnumerateRecipientsForCampaign(EntityHelper helper)
		{
			ResetView(Campaigns.View);
			foreach (CRCampaign campaign in Campaigns.Select().RowCast<CRCampaign>().Where(c => c.Selected == true))
			{
				Campaigns.Cache.Current = campaign;

				foreach (var (contact, _, lead) in
					SelectFrom<Contact>
					.InnerJoin<CRCampaignMembers>
						.On<CRCampaignMembers.contactID.IsEqual<Contact.contactID>>
					.LeftJoin<CRLead>
						.On<Contact.contactType.IsEqual<ContactTypesAttribute.lead>.And<CRLead.contactID.IsEqual<Contact.contactID>>>
					.Where<
						CRCampaignMembers.campaignID.IsEqual<@P.AsString>
						.And<
							Brackets<
								GDPR.ContactExt.pseudonymizationStatus.IsEqual<PXPseudonymizationStatusListAttribute.notPseudonymized>
								.Or<GDPR.ContactExt.pseudonymizationStatus.IsNull>
							>
						>
						.And<True.IsNotIn<
							Contact.noMassMail,
							Contact.noEMail,
							Contact.noMarketing>>
						.And<
							Brackets<
								@P.AsString.IsNotEqual<SendFilterSourcesAttribute.neverSent>
								.Or<CRCampaignMembers.emailSendCount.IsEqual<Zero>>>
							>
						>
					.View
					.ReadOnly
					.Select(this, campaign.CampaignID, campaign.SendFilter).AsEnumerable()
					.Cast<PXResult<Contact, CRCampaignMembers, CRLead>>())
				{
					yield return Recipient.SmartCreate(helper, contact, entity: campaign, lead: lead);
				}
			}
		}

		protected virtual IEnumerable<Recipient> EnumerateRecipientsForLeads(EntityHelper helper)
		{
			// emails only for selected (should be cached)
			foreach (var (contact, lead) in Leads
				.Select()
				.ToList()
				.Select(i => (contact: i.GetItem<Contact>(), lead: i.GetItem<CRLead>()))
				.Where(i => i.contact.Selected == true))
			{
				yield return Recipient.SmartCreate(helper, contact, lead: lead);
			}
		}

		protected virtual void ResetView(PXView view)
		{
			view.Cache.Current = null;
			view.Clear();
		}

		protected virtual void saveLeads()
		{
			if (MassMails.Current != null && MassMails.Current.MassMailID != null)
			{
				var massMailID = (int)MassMails.Current.MassMailID;
				selectedLeads.View.Clear();
				if (MassMails.Current.Source == CRMassMailSourcesAttribute.Lead)
					foreach (Contact batch in Leads.Cache.Updated)
					{
						if (batch == null || batch.ContactID == null) continue;

						var item = (CRMassMailMember)PXSelect<CRMassMailMember>.
							Search<CRMassMailMember.massMailID, CRMassMailMember.contactID>(this, massMailID, batch.ContactID);

						if (batch.Selected != true && item != null)
							selectedLeads.Delete(item);

						if (batch.Selected == true && item == null)
						{
							item = new CRMassMailMember();
							item.MassMailID = massMailID;
							item.ContactID = batch.ContactID;
							selectedLeads.Insert(item);
						}
					}
				else
					foreach (CRMassMailMember item in selectedLeads.Select(massMailID))
						selectedLeads.Delete(item);
			}
		}

		protected virtual void saveCampaigns()
		{
			if (MassMails.Current != null && MassMails.Current.MassMailID != null)
			{
				var massMailID = (int)MassMails.Current.MassMailID;
				selectedCampaigns.View.Clear();
				if (MassMails.Current.Source == CRMassMailSourcesAttribute.Campaign)
					foreach (CRCampaign batch in Campaigns.Cache.Updated)
					{
						if (batch == null || batch.CampaignID == null) continue;

						var item = (CRMassMailCampaign)PXSelect<CRMassMailCampaign>.
							Search<CRMassMailCampaign.massMailID, CRMassMailCampaign.campaignID>(this, massMailID, batch.CampaignID);

						if (batch.Selected != true && item != null)
							selectedCampaigns.Delete(item);

						if (batch.Selected == true)
						{
							if (item == null)
							{
								item = new CRMassMailCampaign();
								item.MassMailID = massMailID;
								item.CampaignID = batch.CampaignID;
								selectedCampaigns.Insert(item);
							}
							else
							{
								selectedCampaigns.Update(item);
							}
						}
					}
				else
					foreach (CRMassMailCampaign item in selectedCampaigns.Select(massMailID))
						selectedCampaigns.Delete(item);
			}
		}

		protected virtual void saveMailLists()
		{
			if (MassMails.Current != null && MassMails.Current.MassMailID != null)
			{
				var massMailID = (int)MassMails.Current.MassMailID;
				selectedMailList.View.Clear();
				if (MassMails.Current.Source == CRMassMailSourcesAttribute.MailList)
					foreach (CRMarketingList batch in MailLists.Cache.Updated)
					{
						if (batch == null || !batch.MarketingListID.HasValue) continue;

						var item = (CRMassMailMarketingList)PXSelect<CRMassMailMarketingList>.
							Search<CRMassMailMarketingList.massMailID, CRMassMailMarketingList.mailListID>(this, massMailID, batch.MarketingListID);

						if (batch.Selected != true && item != null)
							selectedMailList.Delete(item);

						if (batch.Selected == true && item == null)
						{
							item = new CRMassMailMarketingList();
							item.MassMailID = massMailID;
							item.MailListID = batch.MarketingListID;
							selectedMailList.Insert(item);
						}
					}
				else
					foreach (CRMassMailMarketingList item in selectedMailList.Select(massMailID))
						selectedMailList.Delete(item);

			}
		}

		protected virtual void CheckFields(PXCache cache, object row, params Type[] fields)
		{
			var errors = new Dictionary<string, string>(fields.Length);
			foreach (Type field in fields)
			{
				var value = cache.GetValue(row, field.Name);
				if (value == null || (value is string && string.IsNullOrEmpty(value as string)))
				{
					var state = cache.GetValueExt(row, field.Name) as PXFieldState;
					var fieldDisplayName = state == null || string.IsNullOrEmpty(state.DisplayName)
						? field.Name
						: state.DisplayName;
					var errorMessage = PXMessages.LocalizeFormatNoPrefix(Messages.EmptyValueErrorFormat, fieldDisplayName);
					var fieldName = cache.GetField(field);
					errors.Add(fieldName, errorMessage);
					PXUIFieldAttribute.SetError(cache, row, fieldName, errorMessage);
				}
			}
			if (errors.Count > 0)
				throw new PXOuterException(errors, GetType(), row, ErrorMessages.RecordRaisedErrors, null, cache.GetItemType().Name);
		}

		#endregion
	}
}
