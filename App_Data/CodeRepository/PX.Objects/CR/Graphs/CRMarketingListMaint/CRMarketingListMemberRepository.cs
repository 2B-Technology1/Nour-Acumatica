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
using System.Threading.Tasks;
using System.Web.Compilation;
using JetBrains.Annotations;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Description.GI;
using PX.Data.Maintenance.GI;
using PX.Objects.CR.Extensions;
using PX.Objects.CS;
using Options = PX.Objects.CR.CRMarketingListMaint_Extensions.ICRMarketingListMemberRepository.Options;

namespace PX.Objects.CR.CRMarketingListMaint_Extensions
{
	[PXInternalUseOnly]
	public interface ICRMarketingListMemberRepository
	{
		IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembers(int marketingListId, Options options = null);
		IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembers(CRMarketingList marketingList, Options options = null);
		IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembers(IEnumerable<int> marketingListIds, Options options = null);
		IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembers(IEnumerable<CRMarketingList> marketingLists, Options options = null);

		IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembersFromGI(Guid designId, Guid? sharedFilterId, Options options = null);

		public void InsertMember(CRMarketingListMember member);

		public void UpdateMember(CRMarketingListMember member);

		public void DeleteMember(CRMarketingListMember member);

		public class Options
		{
			public bool WithViewContext { get; set; }
			[Obsolete("Currently not working")]
			public int ChunkSize { get; set; }
			public PXFilterRow[] ExternalFilters { get; set; }
		}

	}

	internal class CRMarketingListMemberRepository : ICRMarketingListMemberRepository
	{
		#region State

		private readonly PXGraph _graph;

		private const string TABLE_ALIAS_POSTFIX = "MARKETINGLISTEXT";

		private const string LeadAlias = nameof(CRLead) + TABLE_ALIAS_POSTFIX;
		private const string BAccountAlias = nameof(BAccount) + TABLE_ALIAS_POSTFIX;
		private const string AddressAlias = nameof(Address) + TABLE_ALIAS_POSTFIX;
		private const string BAccountAddressAlias = nameof(Address2) + TABLE_ALIAS_POSTFIX;
		private const string MarketingMemberAlias = nameof(CRMarketingListMember) + TABLE_ALIAS_POSTFIX;

		private static readonly IReadOnlyDictionary<Type, string> _aliasMapping = new Dictionary<Type, string>
		{
			[typeof(CRLead)]   = LeadAlias,
			[typeof(BAccount)] = BAccountAlias,
			[typeof(Address)]  = AddressAlias,
			[typeof(Address2)] = BAccountAddressAlias,
			[typeof(CRMarketingListMember)] = MarketingMemberAlias,
		};
		#endregion

		#region ctor

		public CRMarketingListMemberRepository(PXGraph graph)
		{
			_graph = graph ?? PXGraph.CreateInstance<PXGraph>();
		}

		#endregion

		#region Interface implementation

		#region Insert, Update, and Delete functions for Members

		public virtual void InsertMember(CRMarketingListMember member)
		{
			var datetime = DateTime.UtcNow;
			var userId = _graph.Accessinfo.UserID;
			var screenId = _graph.Accessinfo.GetNormalizedScreenID();

			_graph.ProviderInsert<CRMarketingListMember>(
				new PXDataFieldAssign<CRMarketingListMember.marketingListID>(member.MarketingListID),
				new PXDataFieldAssign<CRMarketingListMember.contactID>(member.ContactID),
				new PXDataFieldAssign<CRMarketingListMember.format>(member.Format),
				new PXDataFieldAssign<CRMarketingListMember.isSubscribed>(member.IsSubscribed),
				new PXDataFieldAssign<CRMarketingListMember.createdByID>(userId),
				new PXDataFieldAssign<CRMarketingListMember.lastModifiedByID>(userId),
				new PXDataFieldAssign<CRMarketingListMember.createdByScreenID>(screenId),
				new PXDataFieldAssign<CRMarketingListMember.lastModifiedByScreenID>(screenId),
				new PXDataFieldAssign<CRMarketingListMember.createdDateTime>(datetime),
				new PXDataFieldAssign<CRMarketingListMember.lastModifiedDateTime>(datetime));
		}

		public virtual void UpdateMember(CRMarketingListMember member)
		{
			if (member?.Type is CRMarketingList.type.Dynamic)
			{
				if (member.IsSubscribed is true)
				{
					DeleteMember(member);
					return;
				}
				else if (member.IsSubscribed is false)
				{
					if (member.LastModifiedByScreenID is null)
					{
						try
						{
							InsertMember(member);
							return;
						}
						catch (PXDatabaseException ex) when (ex.ErrorCode is PXDbExceptions.PrimaryKeyConstraintViolation)
						{
						}
					}
				}
			}
			else if (member?.LastModifiedByScreenID is null)
			{
				try
				{
					InsertMember(member);
					return;
				}
				catch (PXDatabaseException ex) when (ex.ErrorCode is PXDbExceptions.PrimaryKeyConstraintViolation)
				{
				}
			}

			_graph.ProviderUpdate<CRMarketingListMember>(
				new PXDataFieldRestrict<CRMarketingListMember.marketingListID>(member.MarketingListID),
				new PXDataFieldRestrict<CRMarketingListMember.contactID>(member.ContactID),
				new PXDataFieldAssign<CRMarketingListMember.isSubscribed>(member.IsSubscribed));
		}

		public virtual void DeleteMember(CRMarketingListMember member)
		{
			_graph.ProviderDelete<CRMarketingListMember>(
				new PXDataFieldRestrict<CRMarketingListMember.marketingListID>(member.MarketingListID),
				new PXDataFieldRestrict<CRMarketingListMember.contactID>(member.ContactID));
		}

		#endregion

		public IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembers(int marketingListId, Options options = null)
		{
			var list = _graph.Select<CRMarketingList>().FirstOrDefault(m => m.MarketingListID == marketingListId);
			if (list == null)
				throw new PXArgumentException(MessagesNoPrefix.MarketingListNotFound, marketingListId);

			return GetMembers(list);
		}

		public IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembers(CRMarketingList list, Options options = null)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));

			if (list.MarketingListID is null)
				throw new PXArgumentException(MessagesNoPrefix.MarketingListIDIsNull);

			return list.Type is CRMarketingList.type.Static
				? GetMembersFromStaticList(list, options)
				: GetMembersFromDynamicList(list, options);
		}

		public IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembers(
			IEnumerable<int> marketingListIds, Options options = null)
		{
			return GetMembers(
				SelectFrom<
						CRMarketingList>
					.Where<
						CRMarketingList.marketingListID.IsIn<@P.AsInt>>
					.View
					.ReadOnly
					.SelectMultiBound(_graph, null, marketingListIds as int[] ?? marketingListIds.ToArray())
					.RowCast<CRMarketingList>(),
				options);
		}

		public IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembers(
			IEnumerable<CRMarketingList> marketingLists, Options options = null)
		{
			var lists = marketingLists.GroupBy(l => l.Type).ToList();
			return GetMembersFromStaticLists(lists
					.FirstOrDefault(group => group.Key == CRMarketingList.type.Static)
					?.Select(list => list.MarketingListID!.Value), options)
				.Concat(GetMembersFromDynamicLists(
					lists.FirstOrDefault(group => group.Key == CRMarketingList.type.Dynamic), options));
		}

		public IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembersFromGI(
			Guid designId, Guid? sharedFilterId, Options options = null)
		{
			return GetMembersFromGI(designId, sharedFilterId, null, options);
		}

		#endregion

		#region Private methods

		private IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembersFromStaticList(CRMarketingList list, Options options)
		{
			if (list?.MarketingListID is null)
				return Enumerable.Empty<PXResult<CRMarketingListMember, Contact, Address>>();

			return GetMembersFromStaticLists(list.MarketingListID.Value.AsSingleEnumerable(), options);
		}

		private IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembersFromStaticLists(IEnumerable<int> listsIds, Options options)
		{
			var pars = listsIds?.OfType<object>().ToArray();
			if (pars is null || pars.Any() is false)
				return Array.Empty<PXResult<CRMarketingListMember, Contact, Address>>();

			var view = new SelectFrom<
					CRMarketingListMember>
				.InnerJoin<Contact>
					.On<Contact.contactID.IsEqual<CRMarketingListMember.contactID>>
				.LeftJoin<Address>
					.On<Address.addressID.IsEqual<Contact.defAddressID>>
				.LeftJoin<CRLead>
					.On<CRLead.contactID.IsEqual<CRMarketingListMember.contactID>>
				.LeftJoin<BAccount>
					.On<BAccount.defContactID.IsEqual<Contact.contactID>
					.And<BAccount.bAccountID.IsEqual<Contact.bAccountID>>>
				.LeftJoin<Address2>
					.On<Address2.addressID.IsEqual<BAccount.defAddressID>
					.And<Address2.bAccountID.IsEqual<BAccount.bAccountID>>>
				.Where<
					CRMarketingListMember.marketingListID.IsIn<@P.AsInt>>
				.OrderBy<
					CRMarketingListMember.isSubscribed.Desc,
					Contact.displayName.Asc
				>
				.View(_graph);

			return GetSimpleMembers(view, new object[] { pars }, options);
		}

		private IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetSimpleMembers(
			PXSelectBase<CRMarketingListMember> view, object[] pars, Options options)
		{
			if (options?.WithViewContext is true)
			{
				return view
					.SelectWithViewContext(pars)
					.OfType<PXResult<CRMarketingListMember, Contact, Address>>();
			}

			if (options is {ChunkSize: var chunkSize} && chunkSize > 0)
			{
				return view
					.SelectChunked(parameters: pars, filters: options.ExternalFilters, chunkSize: chunkSize)
					.Cast<PXResult<CRMarketingListMember, Contact, Address>>();
			}

			return view
				.SelectExtended(parameters: pars, filters: options?.ExternalFilters)
				.AsEnumerable()
				.Cast<PXResult<CRMarketingListMember, Contact, Address>>();
		}

		private IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembersFromDynamicList(
			CRMarketingList list, Options options)
		{
			if (list.GIDesignID is null)
				return Enumerable.Empty<PXResult<CRMarketingListMember, Contact, Address>>();

			return GetMembersFromGI(list.GIDesignID.Value, list.SharedGIFilter, list.MarketingListID, options);
		}

		private IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembersFromDynamicLists(
			IEnumerable<CRMarketingList> lists, Options options)
		{
			if (lists is null)
				return Array.Empty<PXResult<CRMarketingListMember, Contact, Address>>();

			return lists.Select(list => GetMembersFromDynamicList(list, options)).SelectMany(i => i);
		}

		private IEnumerable<PXResult<CRMarketingListMember, Contact, Address>> GetMembersFromGI(
			Guid designId, Guid? sharedFilterId, int? marketingListId, Options options)
		{
			var (description, contactAlias) = BuildGIDescription(designId, sharedFilterId, marketingListId);

			var graph = PXGenericInqGrph.CreateInstance(description);

			bool withViewContext = options?.WithViewContext ?? false;
			int chunkSize = options?.ChunkSize ?? 0;
			int maxRows = withViewContext ? PXView.MaximumRows : chunkSize;
			int startRow = withViewContext ? PXView.StartRow : 0;
			int totalRows = 0;
			int fetchedRows = 0;

			var (searches, sortcolumns, descendings, filters) = GetGIViewParameters(options, contactAlias);

			if (sharedFilterId != null)
			{
				var sharedFilter = GetSharedFilterRows(description, sharedFilterId).ToArray();
				filters = (filters == null) ? sharedFilter : sharedFilter?.Union(filters).ToArray();
			}

			do
			{
				List<object> results;
				try
				{
					if (withViewContext is false)
						startRow = fetchedRows;

					results = graph
						.Results.View
						.Select(null, null, searches, sortcolumns, descendings, filters,
							ref startRow, maxRows, ref totalRows);
				}
				catch (PXException ex)
				{
					throw new PXException(ex, Messages.GIException, description?.Design?.Name, ex.MessageNoPrefix);
				}

				foreach (GenericResult item in results)
				{
					var contact = (Contact) item.Values[contactAlias];
					item.Values.TryGetValue(LeadAlias, out var leadO);
					var lead = leadO as CRLead;
					item.Values.TryGetValue(AddressAlias, out var addressO);
					var address = addressO as Address;
					item.Values.TryGetValue(BAccountAlias, out var baccountO);
					var baccount = baccountO as BAccount;
					item.Values.TryGetValue(BAccountAddressAlias, out var baccountAddressO);
					var baccountAddress = baccountAddressO as Address2;

					if (item.Values.TryGetValue(MarketingMemberAlias, out var memberObj)
						&& memberObj is CRMarketingListMember { ContactID: { } } member)
					{
						member.IsVirtual = false;
						member.Type = CRMarketingList.type.Dynamic;
					}
					else
					{
						member = new CRMarketingListMember
						{
							MarketingListID = marketingListId,
							ContactID = contact?.ContactID,
							Format = NotificationFormat.Html,
							IsSubscribed = true,
							IsVirtual = true,
							Type = CRMarketingList.type.Dynamic,
						};
					}

					yield return new PXResult<CRMarketingListMember, Contact, Address, CRLead, BAccount, Address2>(
						member, contact, address, lead, baccount, baccountAddress);
				}

				fetchedRows += totalRows;
			} while (withViewContext is false // only 1 chunk for view context
					&& totalRows >= chunkSize
					&& chunkSize > 0);

			if (withViewContext)
				PXView.StartRow = 0;
		}

		private (GIDescription description, string contactAlias) BuildGIDescription(Guid designId, Guid? sharedFilterId, int? marketingListId)
		{
			var descr = PXGenericInqGrph.Def.FirstOrDefault(x => x.DesignID == designId);
			if (descr == null)
			{
				throw new PXInvalidOperationException();// todo: exception
			}

			descr = (GIDescription)descr.Clone();
			GITable contact = GetPrimaryContactTable(descr);
			if (marketingListId != null)
			{
				var leadTable =
					new GITable
					{
						DesignID = descr.DesignID,
						Alias = LeadAlias,
						Name = typeof(CRLead).FullName,
					};

				var addressTable =
					new GITable
					{
						DesignID = descr.DesignID,
						Alias = AddressAlias,
						Name = typeof(Address).FullName,
					};

				var baccountTable =
					new GITable
					{
						DesignID = descr.DesignID,
						Alias = BAccountAlias,
						Name = typeof(BAccount).FullName,
					};

				var baccountAddressTable =
					new GITable
					{
						DesignID = descr.DesignID,
						Alias = BAccountAddressAlias,
						Name = typeof(Address2).FullName,
					};

				var marketingMemberTable =
					new GITable
					{
						DesignID = descr.DesignID,
						Alias = MarketingMemberAlias,
						Name = typeof(CRMarketingListMember).FullName
					};



				descr.Tables = descr
					.Tables
					.Append(marketingMemberTable)
					.Append(leadTable)
					.Append(addressTable)
					.Append(baccountTable)
					.Append(baccountAddressTable)
					.ToList();

				int memberRelation = (descr.Relations.Max(r => r.LineNbr) + 1) ?? 0;
				int leadRelation = memberRelation + 1;
				int addressRelation = memberRelation + 2;
				int baccountRelation = memberRelation + 3;
				int baccountAddressRelation = memberRelation + 4;

				descr.Relations = descr
					.Relations
					.Append(new GIRelation
					{
						DesignID = descr.DesignID,
						LineNbr = memberRelation,
						ParentTable = contact.Alias,
						ChildTable = marketingMemberTable.Alias,
						IsActive = true,
						JoinType = "L"
					})
					.Append(new GIRelation
					{
						DesignID = descr.DesignID,
						LineNbr = leadRelation,
						ParentTable = contact.Alias,
						ChildTable = leadTable.Alias,
						IsActive = true,
						JoinType = "L",
					})
					.Append(new GIRelation
					{
						DesignID = descr.DesignID,
						LineNbr = addressRelation,
						ParentTable = contact.Alias,
						ChildTable = addressTable.Alias,
						IsActive = true,
						JoinType = "L",
					})
					.Append(new GIRelation
					{
						DesignID = descr.DesignID,
						LineNbr = baccountRelation,
						ParentTable = contact.Alias,
						ChildTable = baccountTable.Alias,
						IsActive = true,
						JoinType = "L",
					})
					.Append(new GIRelation
					{
						DesignID = descr.DesignID,
						LineNbr = baccountAddressRelation,
						ParentTable = baccountTable.Alias,
						ChildTable = baccountAddressTable.Alias,
						IsActive = true,
						JoinType = "L",
					})
					.ToList();

				descr.Ons = descr
					.Ons
					.Append(new GIOn
					{
						DesignID = descr.DesignID,
						RelationNbr = memberRelation,
						ParentField = nameof(Contact.contactID),
						Condition = PXConditionListAttribute.ConditionValues.EqualsTo,
						ChildField = nameof(CRMarketingListMember.contactID),
						Operation = "A"
					})
					.Append(new GIOn
					{
						DesignID = descr.DesignID,
						RelationNbr = memberRelation,
						ParentField = "=" + marketingListId,
						Condition = PXConditionListAttribute.ConditionValues.EqualsTo,
						ChildField = nameof(CRMarketingListMember.marketingListID),
						Operation = "A"
					})
					.Append(new GIOn
					{
						DesignID = descr.DesignID,
						RelationNbr = leadRelation,
						ParentField = nameof(Contact.contactID),
						Condition = PXConditionListAttribute.ConditionValues.EqualsTo,
						ChildField = nameof(CRLead.contactID),
						Operation = "A"
					})
					.Append(new GIOn
					{
						DesignID = descr.DesignID,
						RelationNbr = addressRelation,
						ParentField = nameof(Contact.defAddressID),
						Condition = PXConditionListAttribute.ConditionValues.EqualsTo,
						ChildField = nameof(Address.addressID),
						Operation = "A"
					})
					.Append(new GIOn
					{
						DesignID    = descr.DesignID,
						RelationNbr = baccountRelation,
						ParentField = nameof(Contact.bAccountID),
						Condition   = PXConditionListAttribute.ConditionValues.EqualsTo,
						ChildField  = nameof(BAccount.bAccountID),
						Operation   = "A"
					})
					.Append(new GIOn
					{
						DesignID = descr.DesignID,
						RelationNbr = baccountRelation,
						ParentField = nameof(Contact.contactID),
						Condition = PXConditionListAttribute.ConditionValues.EqualsTo,
						ChildField = nameof(BAccount.defContactID),
						Operation = "A"
					})
					.Append(new GIOn
					{
						DesignID    = descr.DesignID,
						RelationNbr = baccountAddressRelation,
						ParentField = nameof(BAccount.defAddressID),
						Condition   = PXConditionListAttribute.ConditionValues.EqualsTo,
						ChildField  = nameof(Address.addressID),
						Operation   = "A"
					})
					.Append(new GIOn
					{
						DesignID = descr.DesignID,
						RelationNbr = baccountAddressRelation,
						ParentField = nameof(BAccount.bAccountID),
						Condition = PXConditionListAttribute.ConditionValues.EqualsTo,
						ChildField = nameof(Address.bAccountID),
						Operation = "A"
					})
					.ToList();
			}

			descr.GroupBys = descr
				.GroupBys
				.Append(new GIGroupBy
				{
					DataFieldName = contact.Alias + "." + nameof(Contact.contactID)
				})
				.ToList();

			var results =
				new[]
				{
					GetResultFor<Contact.contactType>(),
					GetResultFor<Contact.contactID>(),
					GetResultFor<Contact.displayName>(),
					GetResultFor<Contact.isActive>(),
					GetResultFor<Contact.classID>(),
					GetResultFor<Contact.salutation>(),
					GetResultFor<Contact.bAccountID>(),
					GetResultFor<Contact.fullName>(),
					GetResultFor<Contact.eMail>(),
					GetResultFor<Contact.lastModifiedDateTime>(),
					GetResultFor<Contact.createdDateTime>(),
					GetResultFor<Contact.source>(),
					GetResultFor<Contact.assignDate>(),
					GetResultFor<Contact.duplicateStatus>(),
					GetResultFor<Contact.phone1>(),
					GetResultFor<Contact.phone2>(),
					GetResultFor<Contact.phone3>(),
					GetResultFor<Contact.dateOfBirth>(),
					GetResultFor<Contact.fax>(),
					GetResultFor<Contact.webSite>(),
					GetResultFor<Contact.consentAgreement>(),
					GetResultFor<Contact.consentDate>(),
					GetResultFor<Contact.consentExpirationDate>(),
					GetResultFor<Contact.parentBAccountID>(),
					GetResultFor<Contact.gender>(),
					GetResultFor<Contact.method>(),
					GetResultFor<Contact.noCall>(),
					GetResultFor<Contact.noEMail>(),
					GetResultFor<Contact.noFax>(),
					GetResultFor<Contact.noMail>(),
					GetResultFor<Contact.noMarketing>(),
					GetResultFor<Contact.noMassMail>(),
					GetResultFor<Contact.campaignID>(),
					GetResultFor<Contact.phone1Type>(),
					GetResultFor<Contact.phone2Type>(),
					GetResultFor<Contact.phone3Type>(),
					GetResultFor<Contact.faxType>(),
					GetResultFor<Contact.maritalStatus>(),
					GetResultFor<Contact.spouse>(),
					GetResultFor<Contact.status>(),
					GetResultFor<Contact.resolution>(),
					GetResultFor<Contact.languageID>(),
					GetResultFor<Contact.ownerID>(),
					GetResultFor<BAccount.workgroupID>(),
					GetResultFor<BAccount.ownerID>(),
					GetResultFor<BAccount.classID>(),
					GetResultFor<BAccount.parentBAccountID>(),
					GetResultFor<Address.city>(),
					GetResultFor<Address.state>(),
					GetResultFor<Address.postalCode>(),
					GetResultFor<Address.countryID>(),
					GetResultFor<Address.addressLine1>(),
					GetResultFor<Address.addressLine2>(),
					GetResultFor<Address2.city>(),
					GetResultFor<Address2.state>(),
					GetResultFor<Address2.postalCode>(),
					GetResultFor<Address2.countryID>(),
					GetResultFor<Address2.addressLine1>(),
					GetResultFor<Address2.addressLine2>(),
					GetResultFor<CRMarketingListMember.contactID>(),
					GetResultFor<CRMarketingListMember.isSubscribed>(),
					GetResultFor<CRMarketingListMember.format>(),
					GetResultFor<CRMarketingListMember.createdDateTime>(),
				}
				.ToList();

			GIResult GetResultFor<TField>()
			{
				var dac = typeof(TField).DeclaringType;
				return new GIResult
				{
					ObjectName = dac == typeof(Contact) ? contact.Alias : _aliasMapping[dac],
					Field = typeof(TField).Name,
				};
			}

			var resultsGI = descr.Results.ToList();

			foreach (var item in results)
			{
				if (resultsGI.Contains(item))
					continue;
				resultsGI.Add(item);
			}
			descr.Results = resultsGI.ToList();

			return (descr, contact.Alias);
		}

		private (object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters)
			GetGIViewParameters(Options options, string contactAlias)
		{
			bool useViewContext = options?.WithViewContext is true;

			var sorts =
				GetFromViewOrEmpty(PXView.SortColumns)
					.Prepend(DacHelper.GetExplicitField<Contact.displayName>(contactAlias))
					.Select(ReplaceExplicitContactColumn)
					.Select(ReplaceMarketingListMemberContactIdColumn)
					.ToArray();

			var indexesToRemove = sorts
				.SelectIndexesWhere(IsMarketingListMemberColumn)
				.ToArray();

			sorts = RemoveMarketingListMemberFields(sorts);

			var searches =
				GetFromViewOrEmpty(PXView.Searches)
					// for Contact.displayName
					// hack: not null to avoid: param object[] elements = null
					.Prepend(default(object))
					.ToArray();

			searches = RemoveMarketingListMemberFields(searches);


			var descs =
				GetFromViewOrEmpty(PXView.Descendings)
					// for Contact.displayName
					.Prepend(false)
					.ToArray();

			descs = RemoveMarketingListMemberFields(descs);

			PXFilterRow[] filters = useViewContext && PXView.Filters != null
				? PXView.Filters
				: options?.ExternalFilters;

			filters?.ForEach(f =>
			{
				f.DataField =
					ReplaceMarketingListMemberContactIdColumn(
						ReplaceExplicitContactColumn(f.DataField));
			});

			return (searches, sorts, descs, filters);

			T[] GetFromViewOrEmpty<T>(T[] viewItems)
			{
				if (useViewContext)
					return viewItems;
				return Array.Empty<T>();
			}

			// contact table should be replace with alias of contact table
			string ReplaceExplicitContactColumn(string column)
			{
				return Regex.Replace(column, $"^{nameof(Contact)}__", contactAlias + '_', RegexOptions.IgnoreCase);
			}

			// search by CRMarketingListMember.ContactID could be replaced by ContactAlias.ContactID
			string ReplaceMarketingListMemberContactIdColumn(string column)
			{
				return column.Equals(nameof(CRMarketingListMember.ContactID))
					? DacHelper.GetExplicitField<Contact.contactID>(contactAlias)
					: column;
			}

			bool IsMarketingListMemberColumn(string column)
			{
				return column.Contains("_") is false;
			}

			// marketing list members searches cannot be paste into gi,
			// because this list is left join, so it could be null
			// this would lead to filtering out records
			// all search for marketing list member must be applied from the calling method
			T[] RemoveMarketingListMemberFields<T>(T[] items)
			{
				return items
					.Where((_, i) => indexesToRemove.Contains(i) is false)
					.ToArray();
			}
		}

		private GITable GetPrimaryContactTable(GIDescription gi)
		{
			var (contactPrimary, contactFallback) = GetTables<Contact>();

			if (contactPrimary != null)
				return contactPrimary;

			var (leadPrimary, leadFallback) = GetTables<CRLead>();

			return leadPrimary ?? contactFallback ?? leadFallback;

			(GITable primary, GITable fallback) GetTables<TDac>()
			{
				var tables = gi.Tables
					.Where(table => table.Name == typeof(TDac).FullName)
					.ToList();

				var primary = tables.FirstOrDefault(table =>
				{
					return gi.Relations.All(relation =>
					{
						if (relation.ChildTable == table.Alias
							&& relation.JoinType == "L")
							return false;

						if (relation.ParentTable == table.Alias
							&& relation.JoinType == "R")
							return false;

						return true;
					});
				});
				return (primary, tables.FirstOrDefault());
			}
		}

		private IEnumerable<PXFilterRow> GetSharedFilterRows(GIDescription gi, Guid? sharedGIFilter)
		{
			if (sharedGIFilter == null)
				yield break;

			var filters = PXSelect<FilterRow,
					Where<FilterRow.filterID, Equal<Required<FilterRow.filterID>>,
						And<FilterRow.isUsed, Equal<True>>>>
				.Select(_graph, sharedGIFilter);
			foreach (FilterRow f in filters)
			{
				var a = f.DataField.Split(new[] { '_' });

				bool isDescriptionField = false;

				if (a.Length == 2)
				{
					var entityDAC = gi.Tables.FirstOrDefault(_ => _.Alias == a[0])?.Name;
					var entityField = a[1];

					if (!string.IsNullOrWhiteSpace(entityDAC))
					{
						Type cachetype = PXBuildManager.GetType(entityDAC, false);

						if (f.ValueSt != "@me" && f.ValueSt != "@mygroups" && f.ValueSt != "@myworktree"
							&& _graph.Caches[cachetype].GetStateExt(null, entityField) is PXFieldState state && state.DataType == typeof(int))
						{
							if (!int.TryParse(f.ValueSt, out int value1))
							{
								isDescriptionField = true;
							}

							if (!int.TryParse(f.ValueSt2, out int value2))
							{
								isDescriptionField = true;
							}
						}
					}
				}

				yield return new PXFilterRow
				{
					DataField = isDescriptionField ? f.DataField + "_description" : f.DataField,
					OpenBrackets = f.OpenBrackets ?? 0,
					CloseBrackets = f.CloseBrackets ?? 0,
					OrigValue = f.ValueSt,
					OrigValue2 = f.ValueSt2,
					OrOperator = f.Operator == 1,
					Value = f.ValueSt,
					Value2 = f.ValueSt2,
					Condition = (PXCondition)f.Condition,
				};
			}
		}
		#endregion
	}
}
