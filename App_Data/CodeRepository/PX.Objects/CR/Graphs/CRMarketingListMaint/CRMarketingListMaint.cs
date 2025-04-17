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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.Linq;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Maintenance.GI;
using PX.Data.PushNotifications;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CR.Extensions;
using PX.Objects.CS;
using PX.Objects.CR.CRMarketingListMaint_Extensions;
using PX.Objects.CR.Extensions.CRCreateActions;
using PX.SM;
using Serilog;
using static PX.Objects.CR.CRMarketingListMaint_Extensions.ICRMarketingListMemberRepository;

namespace PX.Objects.CR
{
	public class CRMarketingListMaint : PXGraph<CRMarketingListMaint, CRMarketingList>
	{
		#region Views

		[PXHidden]
		public PXSelect<BAccount> BAccount;

		[PXHidden]
		public PXSelect<Address>
			Addresses;

		[PXViewName(Messages.MarketingList)]
		public PXSelect<CRMarketingList>
			MailLists;

		[PXHidden]
		public PXSelect<CRMarketingList, Where<CRMarketingList.marketingListID, Equal<Current<CRMarketingList.marketingListID>>>>
			MailListsCurrent;

		[PXViewName(Messages.MailRecipients)]
		[PXImportSubstitute(typeof(CRMarketingList), typeof(CRMarketingMemberForImport))]
		[PXCopyPasteHiddenView]
		[PXFilterable]
		[PXDependToCache(typeof(CRMarketingList))]
		public SelectFrom<CRMarketingListMember>
			.InnerJoin<Contact> // for proper tail update
				.On<Contact.contactID.IsEqual<CRMarketingListMember.contactID>>
			.OrderBy<
				CRMarketingListMember.createdDateTime.Desc,
				CRMarketingListMember.isSubscribed.Desc,
				Contact.displayName.Asc
			>.View ListMembers;

		protected virtual IEnumerable listMembers()
		{
			if (MailLists.Current is null)
				yield break;

			foreach (var item in MemberRepository.GetMembers(
				MailLists.Current,
				new Options { WithViewContext = true }))
			{
				var member = item.GetItem<CRMarketingListMember>();

				if (ListMembers.Cache.Locate(member) == null)
				{
					ListMembers.Cache.Hold(member);
				}

				yield return item;
			}
		}

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.CampaignMember)]
		[PXFilterable]
		public SelectFrom<CRCampaignToCRMarketingListLink>
					.InnerJoin<CRCampaign>
						.On<CRCampaign.campaignID.IsEqual<CRCampaignToCRMarketingListLink.campaignID>>
					.Where<CRCampaignToCRMarketingListLink.marketingListID.IsEqual<CRMarketingList.marketingListID.FromCurrent>
						.And<CRCampaignToCRMarketingListLink.selectedForCampaign.IsEqual<True>>>
				.View MarketingCampaigns;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXFilter<CopyMembersFilter> CopyMembersFilterView;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<AddMembersToNewListFilter> AddMembersToNewListFilterView;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public SelectFrom<CRMarketingListAlias>
			.Where<CRMarketingListAlias.type.IsEqual<CRMarketingList.type.@static>
				.And<CRMarketingListAlias.marketingListID.IsNotEqual<CRMarketingList.marketingListID.FromCurrent>>>
			.View
			AddMembersToExistingListsFilterView;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<AddMembersFromGIFilter> AddMembersFromGIFilterView;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public SelectFrom<CRMarketingListAlias>
			.Where<CRMarketingListAlias.marketingListID.IsNotEqual<CRMarketingList.marketingListID.FromCurrent>>
			.View
			AddMembersFromMarketingListsFilterView;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public SelectFrom<CRCampaign>.View AddMembersFromCampaignsFilterView;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupUDFAttributes> AddMembersToNewListFilterUdfView;
		protected virtual IEnumerable<PopupUDFAttributes> addMembersToNewListFilterUdfView()
		{
			return UDFHelper.GetRequiredUDFFields(MailLists.Cache, MailLists.Current, GetType());
		}

		#endregion

		#region Ctors

		public CRMarketingListMaint()
		{
			UDFHelper.AddRequiredUDFFieldsEvents(this);

			PXUIFieldAttribute.SetDisplayName<Address2.addressLine1>(this.Caches<Address2>(), "Business Account Address Line 1");
			PXUIFieldAttribute.SetDisplayName<Address2.addressLine2>(this.Caches<Address2>(), "Business Account Address Line 2");
			PXUIFieldAttribute.SetDisplayName<Address2.city>(this.Caches<Address2>(), "Business Account City");
			PXUIFieldAttribute.SetDisplayName<Address2.state>(this.Caches<Address2>(), "Business Account State");
			PXUIFieldAttribute.SetDisplayName<Address2.postalCode>(this.Caches<Address2>(), "Business Account Postal Code");
			PXUIFieldAttribute.SetDisplayName<Address2.countryID>(this.Caches<Address2>(), "Business Account Country");
		}

		public virtual Options PersistMembersSelectOptions { get; } = new Options
		{
			ChunkSize = 1000000000,
		};

		[InjectDependency]
		public ICRMarketingListMemberRepository MemberRepository { get; private set; }

		private ILogger _logger;
		[InjectDependency]
		public ILogger Logger
		{
			get => _logger;
			set => _logger = value?.ForContext<CRMarketingListMaint>();
		}

		#endregion

		#region Actions

		public PXAction<CRMarketingList> AddMembersMenu;

		[PXUIField(DisplayName = "Add Members", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable addMembersMenu(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<CRMarketingList> ManageSubscriptionMenu;

		[PXUIField(DisplayName = "Manage Subscription", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable manageSubscriptionMenu(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<CRMarketingList> ConvertToDynamicList;

		[PXUIField(DisplayName = "Convert To Dynamic List", MapEnableRights = PXCacheRights.Update)]
		[PXButton(DisplayOnMainToolbar = false, Category = ActionCategories.ListManagement)]
		protected virtual IEnumerable convertToDynamicList(PXAdapter adapter)
		{
			if (MailLists.Current.Type is CRMarketingList.type.Dynamic)
				throw new PXInvalidOperationException(
					MessagesNoPrefix.MarketingListIsAlreadyDynamic,
					MailLists.Current.MarketingListID);

			if (ListMembers.Any() is false)
			{
				MailLists.Current.Type = CRMarketingList.type.Dynamic;
				MailLists.UpdateCurrent();
			}
			else if (MailLists.View.Ask(
				MailLists.Current,
				Confirmations.Confirmation,
				Confirmations.ConvertStaticListToDynamic,
				MessageButtons.OKCancel,
				new Dictionary<WebDialogResult, string>
				{
					[WebDialogResult.OK] = "Delete",
					[WebDialogResult.Cancel] = "Cancel",
				}, MessageIcon.None)
				.IsPositive())
			{
				Actions.PressSave();

				var graph = this.CloneGraphState();

				PXLongOperation.StartOperation(this, () =>
				{
					using (new SuppressPushNotificationsScope())
					{
						using (var scope = new PXTransactionScope())
						{
							graph.DeletePersistMembers();
							graph.MailLists.Current.Type = CRMarketingList.type.Dynamic;
							graph.MailLists.UpdateCurrent();
							graph.Actions.PressSave();
							scope.Complete();
						}
					}
				});
			}

			return adapter.Get();
		}

		public PXAction<CRMarketingList> ConvertToStaticList;

		[PXUIField(DisplayName = "Convert To Static List", MapEnableRights = PXCacheRights.Update)]
		[PXButton(DisplayOnMainToolbar = false, Category = ActionCategories.ListManagement)]
		protected virtual IEnumerable convertToStaticList(PXAdapter adapter)
		{
			if (MailLists.Current.Type is CRMarketingList.type.Static)
				throw new PXInvalidOperationException(
					MessagesNoPrefix.MarketingListIsAlreadyStatic,
					MailLists.Current.MarketingListID);

			if (ListMembers.Any() is false)
			{
				MailLists.Current.Type = CRMarketingList.type.Static;
				MailLists.UpdateCurrent();
			}
			else if (MailLists.View.Ask(
				MailLists.Current,
				Confirmations.Confirmation,
				Confirmations.ConvertDynamicListToStatic,
				MessageButtons.YesNoCancel,
				new Dictionary<WebDialogResult, string>
				{
					[WebDialogResult.Yes] = "Keep",
					[WebDialogResult.No] = "Delete",
					[WebDialogResult.Cancel] = "Cancel",
				}, MessageIcon.None)
				.IsIn(WebDialogResult.Yes, WebDialogResult.No))
			{
				bool keepMembers = MailLists.View.Answer == WebDialogResult.Yes;

				Actions.PressSave();
				Actions.PressCancel();

				var graph = this.CloneGraphState();

				PXLongOperation.StartOperation(this, () =>
				{
					using (new SuppressPushNotificationsScope())
					{
						using (var scope = new PXTransactionScope())
						{
							if (keepMembers)
							{
								graph.CopyPersistMembersToList(graph.MailLists.Current,
									graph.MemberRepository
										.GetMembers(graph.MailLists.Current, graph.PersistMembersSelectOptions)
										.Where(item => item.GetItem<CRMarketingListMember>()!.IsVirtual is true));
							}
							else
							{
								graph.DeletePersistMembers();
							}

							graph.MailLists.Current.Type = CRMarketingList.type.Static;
							graph.MailLists.UpdateCurrent();
							graph.Actions.PressSave();
							scope.Complete();
						}
					}
				});
			}

			return adapter.Get();
		}

		public PXAction<CRMarketingList> CopyMembers;

		[PXUIField(DisplayName = "Copy All", MapEnableRights = PXCacheRights.Update)]
		[PXButton(Tooltip = MessagesNoPrefix.MarketingListCopyMembersButtonTooltip)]
		protected virtual IEnumerable copyMembers(PXAdapter adapter)
		{
			if (CopyMembersFilterView.AskExtFullyValid(DialogAnswerType.Positive, reset: true))
			{
				switch (CopyMembersFilterView.Current.AddMembersOption)
				{
					case CopyMembersFilter.addMembersOption.AddToNew:
					{
						CreateNewListAndCopyMembers();
						break;
					}
					case CopyMembersFilter.addMembersOption.AddToExisting:
					{
						CopyMembersToExistingLists();
						break;
					}
				}
			}

			return adapter.Get();
		}

		public PXAction<CRMarketingList> ClearMembers;

		[PXUIField(DisplayName = "Clear All", MapEnableRights = PXCacheRights.Update)]
		[PXButton(Tooltip = MessagesNoPrefix.MarketingListClearMemberButtonTooltip)]
		protected virtual IEnumerable clearMembers(PXAdapter adapter)
		{
			if (MailLists.Current.Type is CRMarketingList.type.Dynamic)
				throw new PXInvalidOperationException(
					MessagesNoPrefix.CannotRemoveMembersFromDynamicList,
					MailLists.Current.MarketingListID);

			if (this.MailListsCurrent.View.Ask(
						null,
						Confirmations.MarketingListClearMembers,
						MessageButtons.OKCancel) == WebDialogResult.OK)
			{
				if (this.IsDirty)
					this.Actions.PressSave();

				var filters = ListMembers.View.GetExternalFilters();
				var graph = this.CloneGraphState();

				PXLongOperation.StartOperation(this, () =>
				{
					using (new SuppressPushNotificationsScope())
					{
						using (var scope = new PXTransactionScope())
						{
							if (filters == null)
							{
								graph.DeletePersistMembers();
							}
							else
							{
								graph.DeletePersistFilteredMembers(filters);
							}

							scope.Complete();
						}
					}
				});
			}

			return adapter.Get();
		}

		public PXAction<CRMarketingList> DeleteSelectedMembers;

		[PXUIField(DisplayName = "Delete", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable deleteSelectedMembers(PXAdapter adapter)
		{
			if (MailLists.Current.Type is CRMarketingList.type.Dynamic)
				throw new PXInvalidOperationException(
					MessagesNoPrefix.CannotRemoveMembersFromDynamicList,
					MailLists.Current.MarketingListID);

			var list =
				ListMembers
					.Cache
					.Updated
					.OfType<CRMarketingListMember>()
					.Union(
						ListMembers
						.Cache
						.Inserted
						.OfType<CRMarketingListMember>()
					)
					.Where(member => member.Selected is true)
					.ToList();

			if(list.Any() is false && ListMembers.Current is {} current)
				list.Add(current);

			list.ForEach(member => ListMembers.Cache.Delete(member));

			return adapter.Get();
		}

		public PXAction<CRMarketingList> SubscribeAll;

		[PXUIField(DisplayName = "Subscribe All", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable subscribeAll(PXAdapter adapter)
		{
			Actions.PressSave();

			var filters = ListMembers.View.GetExternalFilters();
			var graph = this.CloneGraphState();

			PXLongOperation.StartOperation(this, () =>
			{
				using (new SuppressPushNotificationsScope())
				{
					using (var scope = new PXTransactionScope())
					{
						graph.ChangeFilteredMembersSubscription(subscribe: true, filters);
						scope.Complete();
					}
				}
			});

			return adapter.Get();
		}

		public PXAction<CRMarketingList> UnsubscribeAll;

		[PXUIField(DisplayName = "Unsubscribe All", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable unsubscribeAll(PXAdapter adapter)
		{
			Actions.PressSave();

			var filters = ListMembers.View.GetExternalFilters();
			var graph = this.CloneGraphState();

			PXLongOperation.StartOperation(this, () =>
			{
				using (new SuppressPushNotificationsScope())
				{
					using (var scope = new PXTransactionScope())
					{
						graph.ChangeFilteredMembersSubscription(subscribe: false, filters);
						scope.Complete();
					}
				}
			});

			return adapter.Get();
		}

		public PXAction<CRMarketingList> AddMembersFromGI;

		[PXUIField(DisplayName = "Add from Generic Inquiry", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable addMembersFromGI(PXAdapter adapter)
		{
			if (MailLists.Current.Type is CRMarketingList.type.Dynamic)
				throw new PXInvalidOperationException(
					MessagesNoPrefix.CannotAddMembersToDynamicList,
					MailLists.Current.MarketingListID);

			if (AddMembersFromGIFilterView.AskExtFullyValid(DialogAnswerType.Positive, reset: true))
			{
				var filter = AddMembersFromGIFilterView.Current;
				Actions.PressSave();
				var graph = this.CloneGraphState();
				PXLongOperation.StartOperation(this, () =>
				{
					using (new SuppressPushNotificationsScope())
					{
						using (var scope = new PXTransactionScope())
						{
							graph.CopyPersistMembersToList(
								graph.MailLists.Current,
								graph.MemberRepository.GetMembersFromGI(
									filter.GIDesignID!.Value,
									filter.SharedGIFilter,
									graph.PersistMembersSelectOptions));
							scope.Complete();
						}
					}
				});
			}

			return adapter.Get();
		}

		public PXAction<CRMarketingList> AddMembersFromMarketingLists;

		[PXUIField(DisplayName = "Add from Marketing Lists", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable addMembersFromMarketingLists(PXAdapter adapter)
		{
			if (MailLists.Current.Type is CRMarketingList.type.Dynamic)
				throw new PXInvalidOperationException(
					MessagesNoPrefix.CannotAddMembersToDynamicList,
					MailLists.Current.MarketingListID);

			if (AddMembersFromMarketingListsFilterView
				.AskExt(AskExtResetCache(AddMembersFromMarketingListsFilterView), refreshRequired: true)
				.IsPositive())
			{
				var selectedItems = AddMembersFromMarketingListsFilterView
					.SelectMain()
					.Where(i => i.Selected is true)
					.DefaultIfEmpty(AddMembersFromMarketingListsFilterView.Current)
					.Where(i => i != null) // in case there is no current
					.ToList();

				if (selectedItems.Count == 0)
					return adapter.Get();

				Actions.PressSave();
				var graph = this.CloneGraphState();
				PXLongOperation.StartOperation(this, () =>
				{
					using (new SuppressPushNotificationsScope())
					{
						using (var scope = new PXTransactionScope())
						{
							graph.CopyPersistMembersToList(
								graph.MailLists.Current,
								graph.MemberRepository.GetMembers(selectedItems, graph.PersistMembersSelectOptions));
							scope.Complete();
						}
					}
				});
			}

			return adapter.Get();

		}

		public PXAction<CRMarketingList> AddMembersFromCampaigns;

		[PXUIField(DisplayName = "Add from Campaigns", MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable addMembersFromCampaigns(PXAdapter adapter)
		{
			if (MailLists.Current.Type is CRMarketingList.type.Dynamic)
				throw new PXInvalidOperationException(
					MessagesNoPrefix.CannotAddMembersToDynamicList,
					MailLists.Current.MarketingListID);

			if (AddMembersFromCampaignsFilterView
				.AskExt(AskExtResetCache(AddMembersFromCampaignsFilterView), refreshRequired: true)
				.IsPositive())
			{
				var selectedItems = AddMembersFromCampaignsFilterView
					.SelectMain()
					.Where(i => i.Selected is true)
					.Select(i => i.CampaignID)
					.DefaultIfEmpty(AddMembersFromCampaignsFilterView.Current?.CampaignID)
					.OfType<object>()
					.ToArray();

				if (selectedItems.Length == 0)
					return adapter.Get();

				Actions.PressSave();

				var listId = MailLists.Current.MarketingListID;
				var graph = this.CloneGraphState();
				PXLongOperation.StartOperation(this, () =>
				{
					using (new SuppressPushNotificationsScope())
					{
						using (var scope = new PXTransactionScope())
						{
							graph.CopyPersistMembersToList(graph.MailLists.Current,
								new SelectFrom<CRCampaignMembers>
										.Where<CRCampaignMembers.campaignID.IsIn<@P.AsString>>
										.View
										.ReadOnly(graph)
									.SelectChunked(graph.PersistMembersSelectOptions.ChunkSize, new object[] { selectedItems })
									.Select(res => new PXResult<CRMarketingListMember>(
										new CRMarketingListMember
										{
											MarketingListID = listId,
											ContactID = res.GetItem<CRCampaignMembers>()!.ContactID,
											Format = NotificationFormat.Html,
											IsSubscribed = true,
										})));
							scope.Complete();
						}
					}
				});
			}

			return adapter.Get();
		}

		#endregion

		#region CacheAttached

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visibility), PXUIVisibility.Visible)]
		protected virtual void _(Events.CacheAttached<Contact.fullName> e) { }

		[CRMBAccount]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<Contact.bAccountID> e) { }

		[PXDBBool]
		[PXUIField(DisplayName = GDPR.Messages.IsConsented, FieldClass = FeaturesSet.gDPRCompliance.FieldClass)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<Contact.consentAgreement> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Business Account Type")]
		protected virtual void _(Events.CacheAttached<BAccount.type> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "List Name", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<CRMarketingList.name> e) { }

		[PXSelector(typeof(State.stateID), DescriptionField = typeof(State.name))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Address.state> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Member Since", Enabled = false)]
		protected virtual void _(Events.CacheAttached<CRMarketingListMember.createdDateTime> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(CRCampaign.campaignID), DescriptionField = typeof(CRCampaign.campaignName))]
		protected virtual void _(Events.CacheAttached<CRCampaignToCRMarketingListLink.campaignID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(Current<CRMarketingList.marketingListID>))]
		protected virtual void _(Events.CacheAttached<CRCampaignToCRMarketingListLink.marketingListID> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Business Account Workgroup")]
		protected virtual void _(Events.CacheAttached<BAccount.workgroupID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Business Account Owner")]
		protected virtual void _(Events.CacheAttached<BAccount.ownerID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Business Account Parent Account")]
		protected virtual void _(Events.CacheAttached<BAccount.parentBAccountID> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Business Account Source Campaign")]
		protected virtual void _(Events.CacheAttached<BAccount.campaignSourceID> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Business Account Class ID")]
		protected virtual void _(Events.CacheAttached<BAccount.classID> e) { }

		#endregion

		#region Event Handlers

		protected virtual void _(Events.RowSelected<CRMarketingList> e)
		{
			CopyPaste.SetVisible(false);

			if (e.Row == null) return;

			bool isDynamic = e.Row.Type is CRMarketingList.type.Dynamic;
			bool isStatic = isDynamic is false;
			bool isNotEmpty = string.IsNullOrEmpty(e.Row.MailListCode) is false;

			// actions
			ManageSubscriptionMenu.AddMenuAction(SubscribeAll);
			ManageSubscriptionMenu.AddMenuAction(UnsubscribeAll, nameof(SubscribeAll), true);

			AddMembersMenu.AddMenuAction(AddMembersFromGI);
			AddMembersMenu.AddMenuAction(AddMembersFromMarketingLists, nameof(AddMembersFromGI), true);
			AddMembersMenu.AddMenuAction(AddMembersFromCampaigns, nameof(AddMembersFromMarketingLists), true);

			AddMembersFromGI.SetEnabled(isStatic);
			AddMembersFromMarketingLists.SetEnabled(isStatic);
			AddMembersFromCampaigns.SetEnabled(isStatic);

			ClearMembers.SetEnabled(isStatic);
			DeleteSelectedMembers.SetEnabled(isStatic);

			ConvertToDynamicList.SetDisplayOnMainToolbar(isStatic);
			ConvertToDynamicList.SetEnabled(isStatic && isNotEmpty);
			ConvertToStaticList.SetDisplayOnMainToolbar(isDynamic);
			ConvertToStaticList.SetEnabled(isDynamic && isNotEmpty);

			// fields
			e.Cache
				.AdjustUI(e.Row)
				.For<CRMarketingList.gIDesignID>(ui => ui.Enabled = ui.Visible = isDynamic)
				.SameFor<CRMarketingList.sharedGIFilter>();

			ListMembers.View.AllowInsert = isStatic;
			ListMembers.View.AllowDelete = isStatic;
			ListMembers.View.AllowUpdate = true;

			PXImportAttribute.SetEnabled(this, nameof(ListMembers), isStatic && e.Cache.GetOriginal(e.Row) != null);
		}

		protected virtual void _(Events.RowSelected<CRMarketingListAlias> e)
		{
			e.Cache.AllowUpdate = true;
			e.Cache.AllowInsert = false;
			e.Cache.AllowDelete = false;

			e.Cache
				.AdjustUIReadonly(e.Row)
				.ForAllFields(ui => ui.Enabled = false)
				.For<CRMarketingListAlias.selected>(ui => ui.Enabled = true);
		}

		protected virtual void _(Events.RowSelected<CRCampaign> e)
		{
			e.Cache.AllowUpdate = true;
			e.Cache.AllowInsert = false;
			e.Cache.AllowDelete = false;

			e.Cache
				.AdjustUIReadonly(e.Row)
				.ForAllFields(ui => ui.Enabled = false)
				.For<CRMarketingListAlias.selected>(ui => ui.Enabled = true);
		}

		protected virtual void _(Events.RowSelected<CRMarketingListMember> e)
		{
			bool isStatic = MailLists.Current?.Type == CRMarketingList.type.Static;

			e.Cache
				.AdjustUIReadonly(e.Row)
				.ForAllFields(ui => ui.Enabled = false)
				.For<CRMarketingListMember.isSubscribed>(ui => ui.Enabled = true)
				.For<CRMarketingListMember.selected>(ui => ui.Enabled = ui.Visible = isStatic)
				.For<CRMarketingListMember.contactID>(ui => ui.Enabled = isStatic);
		}

		protected virtual void _(Events.RowSelected<Contact> e)
		{
			e.Cache
				.AdjustUIReadonly(e.Row)
				.ForAllFields(ui => ui.Enabled = false);
		}

		protected virtual void _(Events.FieldUpdated<CRMarketingList, CRMarketingList.type> e)
		{
			if (e.NewValue is CRMarketingList.type.Dynamic)
			{
				e.Cache.SetValue<CRMarketingList.gIDesignID>(e.Cache.Current, null);
				e.Cache.SetValue<CRMarketingList.sharedGIFilter>(e.Cache.Current, null);
			}
		}

		protected virtual void _(Events.RowPersisting<CRMarketingListMember> e)
		{
			if (MailLists.Current.Type == CRMarketingList.type.Static)
				return;

			if (e.Operation.Command() == PXDBOperation.Update)
			{
				// child identity won't work here because of cancel
				if (e.Row.MarketingListID < 0)
					e.Row.MarketingListID = MailLists.Current.MarketingListID;

				if (e.Row.IsSubscribed is false)
				{
					try
					{
						MemberRepository.InsertMember(e.Row);
					}
					catch (PXDatabaseException ex) when (ex.ErrorCode is PXDbExceptions.PrimaryKeyConstraintViolation)
					{
						Logger.Verbose(ex,
							"Marketing member {ContactID} for list {MarketingListID} already exists",
							e.Row.ContactID, e.Row.MarketingListID);
						MemberRepository.UpdateMember(e.Row);
					}
				}
				else
				{
					MemberRepository.DeleteMember(e.Row);
				}

				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldUpdated<AddMembersToNewListFilter, AddMembersToNewListFilter.mailListCode> e)
		{
			if (e.NewValue is string value
				&& string.IsNullOrWhiteSpace(value) is false
				&& CRMarketingList.UK.Find(this, value) is { })
			{
				e.Cache.RaiseExceptionHandling<AddMembersToNewListFilter.mailListCode>(
					e.Row, e.NewValue,
					new PXSetPropertyException<AddMembersToNewListFilter.mailListCode>(
						MessagesNoPrefix.MarketingListAlreadyExists, e.NewValue));
			}
		}

		#endregion

		#region Methods

		public virtual void CreateNewListAndCopyMembers()
		{
			if (AddMembersToNewListFilterView.AskExtFullyValid(DialogAnswerType.Positive, reset: true))
			{
				var filter = AddMembersToNewListFilterView.Current;
				if (CRMarketingList.UK.Find(this, filter.MailListCode) is { })
				{
					AddMembersToNewListFilterView.Cache.RaiseExceptionHandling<AddMembersToNewListFilter.mailListCode>(
						filter, filter.MailListCode,
						new PXSetPropertyException<AddMembersToNewListFilter.mailListCode>(
							MessagesNoPrefix.MarketingListAlreadyExists, filter.MailListCode));
					return;
				}

				var redirect = AddMembersToNewListFilterView.View.Answer == WebDialogResult.Yes;
				Actions.PressSave();
				var graph = this.CloneGraphState();
				PXLongOperation.StartOperation(this, () =>
				{
					var newGraph = PXGraph.CreateInstance<CRMarketingListMaint>();

					using (new SuppressPushNotificationsScope())
					{
						using (var scope = new PXTransactionScope())
						{
							filter = graph.AddMembersToNewListFilterView.Current;
							var newList = newGraph.MailLists.Insert(new CRMarketingList
							{
								MailListCode = filter.MailListCode,
								Name = filter.Name,
								OwnerID = filter.OwnerID,
								Type = CRMarketingList.type.Static,
							});
							newGraph.MailLists.Current = newList;
							UDFHelper.FillfromPopupUDF(
								newGraph.MailLists.Cache,
								graph.AddMembersToNewListFilterUdfView.Cache,
								typeof(CRMarketingListMaint),
								newList);
							newGraph.Actions.PressSave();
							graph.CopyPersistMembersToList(newList);
							scope.Complete();
						}
					}

					if (redirect)
					{
						throw new PXRedirectRequiredException(newGraph, "", true);
					}
				});
			}
		}

		public virtual void CopyMembersToExistingLists()
		{
			if (AddMembersToExistingListsFilterView
				.AskExt(AskExtResetCache(AddMembersToExistingListsFilterView), refreshRequired: true)
				.IsPositive())
			{
				var selectedItems = AddMembersToExistingListsFilterView
					.SelectMain()
					.Where(i => i.Selected is true)
					.DefaultIfEmpty(AddMembersToExistingListsFilterView.Current)
					.Where(i => i?.MarketingListID != null) // in case ther is no current
					.Select(i => i.MarketingListID.Value)
					.ToList();

				if (selectedItems.Count == 0)
					return;

				Actions.PressSave();
				var graph = this.CloneGraphState();
				PXLongOperation.StartOperation(this, () =>
				{
					using (new SuppressPushNotificationsScope())
					{
						using (var scope = new PXTransactionScope())
						{
							graph.CopyPersistMembersToLists(selectedItems);
							scope.Complete();
						}
					}
				});
			}
		}

		public virtual void CopyPersistMembersToLists(IEnumerable<int> marketingListIds, IEnumerable<PXResult<CRMarketingListMember>> members)
		{
			var listIds = marketingListIds as int[] ?? marketingListIds.ToArray();
			var hashset = new HashSet<int?>();
			foreach (CRMarketingListMember member in members)
			{
				if (hashset.Add(member.ContactID) is false)
					continue;

				var memberListId = member.MarketingListID;

				foreach (var listId in listIds)
				{
					member.MarketingListID = listId;
					member.IsSubscribed = true;
					try
					{
						MemberRepository.InsertMember(member);
					}
					catch (PXDatabaseException ex) when (ex.ErrorCode is PXDbExceptions.PrimaryKeyConstraintViolation)
					{
						Logger.Verbose(ex,
							"Marketing member {ContactID} for list {MarketingListID} already exists",
							member.ContactID, member.MarketingListID);
					}
				}

				member.MarketingListID = memberListId;
			}
		}

		public virtual void CopyPersistMembersToList(CRMarketingList marketingList,
			IEnumerable<PXResult<CRMarketingListMember>> members)
		{
			// ReSharper disable once PossibleInvalidOperationException
			CopyPersistMembersToLists(new[] { marketingList.MarketingListID.Value }, members);
		}

		public virtual void CopyPersistMembersToLists(IEnumerable<int> marketingListIds)
		{
			CopyPersistMembersToLists(marketingListIds,
				MemberRepository.GetMembers(MailLists.Current, PersistMembersSelectOptions));
		}

		public virtual void CopyPersistMembersToList(CRMarketingList marketingList)
		{
			// ReSharper disable once PossibleInvalidOperationException
			CopyPersistMembersToLists(new[] { marketingList.MarketingListID.Value });
		}

		public virtual void DeletePersistMembers()
		{
			using (new PXCommandScope(PXDatabase.Provider.DefaultQueryTimeout * 20))
				ProviderDelete<CRMarketingListMember>(
					new PXDataFieldRestrict<CRMarketingListMember.marketingListID>(
						MailLists.Current.MarketingListID));
		}

		public virtual void DeletePersistFilteredMembers(PXFilterRow[] listMembersFilters)
		{
			foreach (CRMarketingListMember member in MemberRepository.GetMembers(
				MailLists.Current,
				new Options
				{
					ChunkSize = PersistMembersSelectOptions.ChunkSize,
					ExternalFilters = listMembersFilters,
				}))
			{
				if (member.IsVirtual != true)
					MemberRepository.DeleteMember(member);
			}
		}

		public virtual void ChangeFilteredMembersSubscription(bool subscribe, PXFilterRow[] listMembersFilters)
		{
			foreach (CRMarketingListMember member in MemberRepository.GetMembers(
				MailLists.Current,
				new Options
				{
					ChunkSize = PersistMembersSelectOptions.ChunkSize,
					ExternalFilters = listMembersFilters,
				}))
			{
				member.IsSubscribed = subscribe;
				MemberRepository.UpdateMember(member);
			}
		}

		public virtual PXView.InitializePanel AskExtResetCache(PXSelectBase select)
		{
			return (g, v) =>
			{
				select.Cache.ClearQueryCache();
				select.Cache.Clear();
				select.Cache.Current = null;
			};
		}

		#endregion
	}
}
