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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.CRMarketingListMaint_Extensions;
using PX.Objects.CS;
using PX.SM;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.Common.Extensions;
using static PX.Objects.CR.CRMarketingListMaint_Extensions.ICRMarketingListMemberRepository;

namespace PX.Objects.CR.Extensions
{
	/// <exclude/>
	public abstract class MarketingListDetailsExt<TGraph, TMaster, TContactField>
		: PXGraphExtension<TGraph>
			where TGraph : PXGraph, new()
			where TMaster : class, IBqlTable, new()
			where TContactField : class, IBqlField
	{
		[InjectDependency]
		public ICRMarketingListMemberRepository MemberRepository { get; set; }

		#region Views

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelectReadonly<CRMarketingList,
					Where<CRMarketingList.marketingListID,
						Equal<Required<CRMarketingListMember.marketingListID>>>>
				CRMarketingListView;

		[PXViewName(Messages.Subscriptions)]
		[PXCopyPasteHiddenView]
		[PXFilterable]
		public PXSelectJoin<
					CRMarketingListMember,
				InnerJoin<CRMarketingList,
					On<True, Equal<False>>,
				InnerJoin<Contact,
					On<True, Equal<False>>>>,
				Where<
					CRMarketingListMember.contactID, Equal<Current<TContactField>>>,
				OrderBy<
					Desc<CRMarketingListMember.isSubscribed,
					Asc<CRMarketingList.mailListCode>>>>
			Subscriptions;

		protected virtual IEnumerable subscriptions()
		{
			PXResultset<CRMarketingListMember, CRMarketingList, Contact> subscriptions = new PXResultset<CRMarketingListMember, CRMarketingList, Contact>();

			var primaryCache = Base.Views[Base.PrimaryView]?.Cache;

			if (primaryCache?.Current != null)
			{
				bool oldDirty = Base.Caches[typeof(CRMarketingListMember)].IsDirty;

				GetFromStatic().ForEach(_ => subscriptions.Add(_));

				if (primaryCache.GetStatus(primaryCache.Current) != PXEntryStatus.Inserted)
				{
					GetFromDynamic().ForEach(_ => subscriptions.Add(_));
				}

				Base.Caches[typeof(CRMarketingListMember)].IsDirty = oldDirty;
			}

			return subscriptions;
		}

		protected virtual PXResultset<CRMarketingListMember, CRMarketingList> GetFromStatic()
		{
			PXResultset<CRMarketingListMember, CRMarketingList, Contact> result = new PXResultset<CRMarketingListMember, CRMarketingList, Contact>();

			var resultset = PXSelectJoin<
					CRMarketingList,
				LeftJoin<CRMarketingListMember,
					On<CRMarketingListMember.marketingListID, Equal<CRMarketingList.marketingListID>,
					And<CRMarketingListMember.contactID, Equal<Current<TContactField>>>>,
				LeftJoin<Contact,
					On<Contact.contactID, Equal<CRMarketingListMember.contactID>>>>,
				Where<
					CRMarketingList.type, Equal<CRMarketingList.type.@static>,
					And<CRMarketingList.status, Equal<CRMarketingList.status.active>>>,
				OrderBy<
					Desc<CRMarketingListMember.isSubscribed>>>
				.Select(Base);

			foreach (var item in resultset)
			{
				var list = item.GetItem<CRMarketingList>();
				var member = item.GetItem<CRMarketingListMember>();
				var contact = item.GetItem<Contact>();

				member = GetCachedMember(member, list);

				result.Add(new PXResult<CRMarketingListMember, CRMarketingList, Contact>(member, list, contact));
			}

			return result;
		}

		protected virtual PXResultset<CRMarketingListMember, CRMarketingList> GetFromDynamic()
		{
			PXResultset<CRMarketingListMember, CRMarketingList, Contact> result = new PXResultset<CRMarketingListMember, CRMarketingList, Contact>();

			List<CRMarketingList> lists = new List<CRMarketingList>();

			PXSelect<
					CRMarketingList,
				Where<
					CRMarketingList.type, Equal<CRMarketingList.type.dynamic>,
					And<CRMarketingList.status, Equal<CRMarketingList.status.active>>>>
				.Select(Base)
				.ForEach(_ => lists.Add(_));

			foreach (var list in lists)
			{
				var item = MemberRepository.GetMembers(
						list,
						new Options()
						{
							ExternalFilters = PrepareFilter().SingleToArray()
						})
					.FirstOrDefault();

				if (item == null)
					continue;

				var member = item.GetItem<CRMarketingListMember>();
				var contact = item.GetItem<Contact>();

				member = GetCachedMember(member, list);

				result.Add(new PXResult<CRMarketingListMember, CRMarketingList, Contact>(member, list, contact));
			}

			return result;
		}

		#endregion

		#region ctor

		public override void Initialize()
		{
			base.Initialize();

			PXUIFieldAttribute.SetVisible<CRMarketingListMember.format>(Subscriptions.Cache, null, false);

			// for cb api
			Base.Caches<CRMarketingList>().Fields.Add("IsDynamic");
			Base.FieldSelecting.AddHandler(typeof(CRMarketingList), "IsDynamic", (s, e) =>
			{
				e.ReturnValue = (e.Row is CRMarketingList { Type: CRMarketingList.type.Dynamic });
			});
		}

		public virtual Options PersistMembersSelectOptions { get; } = new Options
		{
			ChunkSize = 1000000000,
		};


		private ILogger _logger;
		[InjectDependency]
		public ILogger Logger
		{
			get => _logger;
			set => _logger = value?.ForContext<CRMarketingListMaint>();
		}

		#endregion

		#region Actions

		public PXAction<TMaster> SubscribeAll;
		[PXUIField(DisplayName = "Subscribe All", MapEnableRights = PXCacheRights.Update)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable subscribeAll(PXAdapter adapter)
		{
			Base.Actions.PressSave();

			ChangeMemberSubscription(subscribe: true);

			return adapter.Get();
		}

		public PXAction<TMaster> UnsubscribeAll;
		[PXUIField(DisplayName = "Unsubscribe All", MapEnableRights = PXCacheRights.Update)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable unsubscribeAll(PXAdapter adapter)
		{
			Base.Actions.PressSave();

			ChangeMemberSubscription(subscribe: false);

			return adapter.Get();
		}

		public PXAction<TMaster> ViewMarketingList;
		[PXUIField(Visible = false)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable viewMarketingList(PXAdapter adapter)
		{
			var member = Subscriptions.Current;

			if (member == null)
				return adapter.Get();

			var list = CRMarketingList.PK.Find(Base, member.MarketingListID);

			PXRedirectHelper.TryRedirect(Base.Caches[typeof(CRMarketingList)], list, "", PXRedirectHelper.WindowMode.NewWindow);

			return adapter.Get();
		}

		public PXAction<TMaster> RefreshMarketingListMembers;
		[PXUIField(Visible = false)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable refreshMarketingListMembers(PXAdapter adapter)
		{
			Base.SelectTimeStamp();
			Subscriptions.Cache.ClearQueryCache();
			Subscriptions.Cache.Clear();

			return adapter.Get();
		}

		#endregion

		#region Events

		#region CacheAttached

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Member Since", Enabled = false)]
		protected virtual void _(Events.CacheAttached<CRCampaignMembers.createdDateTime> e) { }


		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search<CRMarketingList.marketingListID,
			Where<CRMarketingList.type, Equal<CRMarketingList.type.@static>,
				And<CRMarketingList.status, Equal<CRMarketingList.status.active>>>>),
			DescriptionField = typeof(CRMarketingList.mailListCode))]
		protected virtual void _(Events.CacheAttached<CRMarketingListMember.marketingListID> e) { }

		#endregion

		protected virtual void _(Events.RowDeleting<CRMarketingListMember> e)
		{
			if (e.Row == null) return;

			var marketingList = CRMarketingListView.SelectSingle(e.Row.MarketingListID);
			if (marketingList?.Type is CRMarketingList.type.Dynamic)
			{
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.RowPersisting<CRMarketingListMember> e)
		{
			if (e.Operation.Command() != PXDBOperation.Update)
				return;

			switch (e?.Row?.Type)
			{
				case CRMarketingList.type.Dynamic:

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
						}
					}
					else if (e.Row.IsSubscribed is true)
					{
						MemberRepository.DeleteMember(e.Row);
					}

					e.Cancel = true;

					break;

				case CRMarketingList.type.Static:

					if (e.Cache.GetOriginal(e.Row) == null)
					{
						// sync ContactID after dirty primary persisted
						e.Cache.SetDefaultExt<CRMarketingListMember.contactID>(e.Row);
						MemberRepository.InsertMember(e.Row);
						e.Cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
						e.Cancel = true;
					}

					break;
			}
		}

		#endregion

		#region Methods

		public virtual void ChangeMemberSubscription(bool subscribe)
		{
			var filters = Subscriptions.View.GetExternalFilters();
			var graph = Base.CloneGraphState();

			PXLongOperation.StartOperation(Base, () =>
			{
				using (var scope = new PXTransactionScope())
				{
					graph.FindImplementation<MarketingListDetailsExt<TGraph,TMaster,TContactField>>()
						.ChangeMemberSubscription(subscribe, filters);

					graph.Actions.PressCancel();
					scope.Complete();
				}
			});
		}

		public virtual void ChangeMemberSubscription(bool subscribe, PXFilterRow[] filters)
		{
			int startRow = 0, totalRows = 0;
			foreach (PXResult item in Subscriptions
						.View.Select(null,null,null,null,null, filters, ref startRow, 0, ref totalRows))
			{
				var list = item.GetItem<CRMarketingList>();
				var member = item.GetItem<CRMarketingListMember>();

				member.IsSubscribed = subscribe;
				MemberRepository.UpdateMember(member);
			}
		}

		public virtual PXFilterRow PrepareFilter()
		{
			return new PXFilterRow()
			{
				OrOperator = false,
				OpenBrackets = 1,
				DataField = nameof(Contact) + "__" + nameof(Contact.ContactID),
				Condition = PXCondition.EQ,
				Value = Base.Caches<TMaster>().GetValue<TContactField>(Base.Caches<TMaster>().Current),
				CloseBrackets = 1
			};
		}

		protected virtual CRMarketingListMember GetCachedMember(CRMarketingListMember member, CRMarketingList list)
		{
			if (member?.ContactID == null)
			{
				member = Base.Caches[typeof(CRMarketingListMember)].InitNewRow(member);

				member.MarketingListID = list?.MarketingListID;
				member.IsSubscribed = false;
			}
			var cached = Base.Caches[typeof(CRMarketingListMember)].Locate(member) as CRMarketingListMember;
			if (cached == null)
			{
				Base.Caches[typeof(CRMarketingListMember)].Hold(member);
				cached = member;
			}

			return cached;
		}

		#endregion
	}
}
