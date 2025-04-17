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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Discount;
using PX.Objects.CR;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using SerilogTimings.Extensions;

namespace SP.Objects.CR
{
	using PortalBAccountVisibilityDacs;


	// thread safety over instance via db slot (for multitenant and subscribe)
	// and concurrent dictionary (for iterative fetches)
	internal class PortalBAccountVisibilityProvider
	{
		private const string SlotSuffix = "$PortalVisiblitySlot";
		public static PortalBAccountVisibilityProvider Definition
		{
			get
			{
				// no tables for self instance to avoid full reinitialization
				var instance = PXDatabase.GetSlot<PortalBAccountVisibilityProvider>(
					nameof(PortalBAccountVisibilityProvider) + SlotSuffix);

				if (instance != null)
				{
					var slot = PXDatabase.GetSlot<SlotHolder, PortalBAccountVisibilityProvider>(
						nameof(SlotHolder) + SlotSuffix,
						instance,
						typeof(BAccount),
						typeof(Contact));

					if (slot != null)
						return slot.Instance;
				}

				PXTrace.WriteError($"Cannot initialize {nameof(PortalBAccountVisibilityProvider)}");
				// avoid null refs - just empty provider
				return new PortalBAccountVisibilityProvider();

			}
		}

		protected readonly Node _tree = new Node(default);
		protected readonly ConcurrentDictionary<Guid, int?> _usersToBAccount = new ConcurrentDictionary<Guid, int?>();
		protected readonly HashSet<int> _deletedAccounts = new HashSet<int>();
		protected DateTime? _lastReadDateTime = null;
		protected readonly int _batchSize = 10000;
		protected readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
		protected static ILogger Logger => PXTrace.Logger.ForContext<PortalBAccountVisibilityProvider>();

		public virtual IReadOnlyCollection<int> GetVisibleBAccountIds(Guid userId)
		{
			var logger = Logger;
			logger.Verbose("Get visible business accounts for user {UserId}", userId);
			try
			{
				if (GetBAccountForUser(userId) is { } baccountId)
				{
					if (TryFindNode(baccountId, out var node, out var isDeleted))
					{
						var result = node.FlattenKeys();
						logger.Verbose("Visible business accounts ids for user {UserId} are {@VisibleBAccountsIds}",
						               userId, result);
						return result;
					}

					if (isDeleted is false)
					{
						logger.Warning(
							"Node was not found for the business account {BAccountId} related with the user {UserId}",
							baccountId, userId);
					}
					else
					{
						logger.Verbose("The business account {BAccountId} related with the user {UserId} was deleted",
						               baccountId, userId);
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, "Unexpected error during getting visible business accounts for user {UserId}", userId);
			}

			logger.Warning("User {UserId} doesn't have related business account", userId);
			return Array.Empty<int>();
		}

		public virtual IReadOnlyCollection<int> GetChildrenBAccountsIds(int parentBAccountId, bool onlyDirectChildren)
		{
			var logger = Logger;
			logger.Verbose("Get children business accounts for business account {BAccountId} " +
			               "with only direct children {OnlyDirectChildren}",
			               parentBAccountId, onlyDirectChildren);
			try
			{
				if (TryFindNode(parentBAccountId, out var node, out var isDeleted))
				{
					var children = onlyDirectChildren
						? node.Children
						: node.Flatten().Skip(1);

					var result = children.Select(c => c.Key).ToList();

					logger.Verbose("Children business accounts for business account {BAccountID} " +
					               "with only direct children {OnlyDirectChildren} are {@VisibleBAccountsIds}",
					               parentBAccountId, onlyDirectChildren, result);

					return result;
				}

				if (isDeleted is false)
				{
					logger.Warning("Node was not found for the parent business account {BAccountId}", parentBAccountId);
				}
				else
				{
					logger.Verbose("Business account {BAccountId} was deleted", parentBAccountId);
				}
			}
			catch (Exception ex)
			{
				logger.Error(
					ex, "Unexpected error during getting children business accounts for business account {BAccountId}",
					parentBAccountId);
			}

			return Array.Empty<int>();
		}

		protected virtual bool TryFindNode(int baccountId, out Node node, out bool isDeleted)
		{
			var logger = Logger;
			_locker.EnterReadLock();
			try
			{
				if (_deletedAccounts.Contains(baccountId))
				{
					logger.Verbose("Business account {BAccountId} deleted", baccountId);
					node = null;
					isDeleted = true;
					return false;
				}

				node = _tree.Find(baccountId);
			}
			finally
			{
				_locker.ExitReadLock();
			}
			if (node == null)
			{
				logger.Verbose("Node for business account {BAccountId} not found", baccountId);
				_locker.EnterWriteLock();
				try
				{
					UpdateBAccountTree(ReadBAccountsFor(GetGraph(), baccountId));
					if (_deletedAccounts.Contains(baccountId))
					{
						logger.Verbose("Business account {BAccountId} deleted", baccountId);
						node = null;
						isDeleted = true;
						return false;
					}
					node = _tree.Find(baccountId);
				}
				finally
				{
					_locker.ExitWriteLock();
				}
			}
			isDeleted = false;

			LogNodeForBAccount(logger, node, baccountId);
			
			logger.Verbose("Tree: {@Tree}", _tree.ForLogging());

			return node != null;
		}

		protected virtual int? GetBAccountForUser(Guid userId)
		{
			return _usersToBAccount.GetOrAdd(userId, userId_ =>
				ReadContactFor(GetGraph(), userId_)?.BAccountID);
		}

		protected virtual void UpdateBAccountTree(IEnumerable<BAccount> bAccounts)
		{
			var logger = Logger;
			using (logger.TimeOperationVerbose("Update business account tree"))
			{
				foreach (var baccount in bAccounts)
				{
					logger.Verbose("Update business account {@BAccount}", baccount);
					if (baccount.BAccountID is { } baccountId)
					{
						var node = _tree.Find(baccountId);

						LogNodeForBAccount(logger, node, baccountId);

						if (baccount.DeletedDatabaseRecord is true)
						{
							logger.Verbose("Business account {BAccountId} is deleted", baccountId);
							_deletedAccounts.Add(baccountId);
							// unlink
							if (node != null)
							{
								logger.Verbose("Unlink node {NodeId}, make children {@ChildrenIds} top level",
								               node.Key, node.Children.Select(c => c.Key));
								node.Parent = null;
								foreach (var child in node.Children.ToList() /* avoid collection modified */)
								{
									// make toplevel
									child.Parent = _tree;
								}
							}

							continue;
						}

						node ??= new Node(baccountId);

						var parentId = baccount.ParentBAccountID.GetValueOrDefault();
						if (baccount.ParentBAccountID == null
						    // account was deleted sometime ago 
						 || _deletedAccounts.Contains(parentId))
						{
							logger.Verbose("No parent for business account {BAccountId}", baccountId);
							// no parent -> consider as top level
							node.Parent = _tree;
							continue;
						}

						// no changes
						if (node.Parent?.Key == parentId)
						{
							logger.Verbose("No changes to business account {BAccountId}", baccountId);
							continue;
						}

						string parentFoundStatus;
						if (_tree.Find(parentId) is { } parent)
						{
							node.Parent = parent;
							parentFoundStatus = "found";
						}
						else
						{
							// temporary consider as top level
							// but could be updated later
							parent = new Node(parentId)
							{
								Parent = _tree
							};
							node.Parent = parent;
							parentFoundStatus = "not found";
						}
						logger.Verbose("Parent {ParentId} was {FoundStatus} for business account {BAccountId}", parentId, parentFoundStatus, baccountId);
					}
				}
			}
		}

		protected virtual IEnumerable<BAccount> ReadBAccounts(
			PXGraph graph,
			DateTime? since = null,
			// specify to fetch all baccount, not only already read
			bool fetchUnknownItems = false)
		{
			using (Logger.TimeOperationVerbose(
				"Read business accounts since {Since} fetch unknown items {FetchUnknownItems}",
				since, fetchUnknownItems))
			{
				BqlCommand select = new SelectFrom<BAccount>
					.OrderBy<BAccount.bAccountID.Asc>();
				if (since != null)
					select = select.WhereNew<Where<BAccount.lastModifiedDateTime.IsGreaterEqual<@P.AsDateTime>>>();

				List<object> accountIds = null;
				var commands = new List<BqlCommand>(2);

				if (fetchUnknownItems)
				{
					commands.Add(select);
				}
				else
				{
					commands.Add(select.WhereAnd<Where<BAccount.bAccountID.IsIn<@P.AsInt>>>());
					commands.Add(select.WhereAnd<Where<BAccount.parentBAccountID.IsIn<@P.AsInt>>>());
					var flattenKeys = _tree.FlattenKeys();
					accountIds = new List<object>(flattenKeys.Count - 1 + _deletedAccounts.Count);
					accountIds.AddRange(flattenKeys.Skip(1/*root*/).OfType<object>());
					accountIds.AddRange(_deletedAccounts.OfType<object>());
				}

				var ids = new HashSet<int>();

				foreach (var account in SelectWithDeletedWindowed<BAccount>(graph, commands, since, accountIds))
				{
					foreach (var innerAccount in ReadBAccountsFor(graph, account))
					{
						if (ids.Add(innerAccount.BAccountID!.Value))
							yield return innerAccount;
					}
				}
			}
		}


		protected virtual IEnumerable<BAccount> ReadBAccountsFor(PXGraph graph, int baccountId)
		{
			using (Logger.TimeOperationVerbose("Read business account {BAccountId}", baccountId))
			{
				BAccount baccount = SelectFrom<BAccount>
				                   .Where<BAccount.bAccountID.IsEqual<@P.AsInt>>
				                   .View
				                   .ReadOnly
				                   .Select(graph, baccountId)
				                   .FirstOrDefault();
				if (baccount == null)
				{
					// calling thread will handle not existing account
					return Array.Empty<BAccount>();
				}

				return ReadBAccountsFor(graph, baccount);
			}

		}
		protected virtual IEnumerable<BAccount> ReadBAccountsFor(PXGraph graph, BAccount baccount)
		{
			// no need to get to top
			// but need to go to the bottom (grandchildren)
			// however it is still needed to fetch baccount to see if it was deleted
			using (Logger.TimeOperationVerbose("Read business accounts for business account {BAccountId}", baccount.BAccountID))
			using (new PXReadDeletedScope())
			{
				yield return baccount;

				var childrenView = new SelectFrom<BAccount>
					.Where<BAccount.parentBAccountID.IsIn<@P.AsInt>>
					.View
					.ReadOnly(graph);

				// ReSharper disable once PossibleInvalidOperationException (can't be null)
				var childrenIds = new List<int>(1){baccount.BAccountID.Value};
				var fetched = new HashSet<int>();
				do
				{
					// to avoid duplication
					childrenIds.RemoveAll(i => fetched.Add(i) is false);

					// to avoid too much isin arguments count that could lead to error is mssql
					var isInParam = childrenIds.Take(_batchSize).ToArray<object>();
					childrenIds.RemoveRange(0, Math.Min(isInParam.Length, childrenIds.Count));

					var children = childrenView.View.SelectMultiBound(null, new object[]{ isInParam });
					if (children == null || children.Count == 0)
					{
						yield break;
					}

					childrenIds.Clear();

					if (childrenIds.Capacity < children.Count)
						childrenIds.Capacity = children.Count;

					foreach (BAccount child in children)
					{
						yield return child;
						// ReSharper disable once PossibleInvalidOperationException (can't be null)
						childrenIds.Add(child.BAccountID.Value);
					}
					
				} while (true);
			}
		}


		protected virtual void UpdateUserToBAccountAssociations(IEnumerable<Contact> contacts)
		{
			foreach (var contact in contacts)
			{
				if (contact.UserID is { } userId)
				{
					_usersToBAccount[userId] = contact.DeletedDatabaseRecord is true
						? null : contact.BAccountID;
				}
			}
		}

		protected virtual Contact ReadContactFor(PXGraph graph, Guid userId)
		{
			using (Logger.TimeOperationVerbose("Read contact for user {UserId}", userId))
			using (new PXReadDeletedScope())
			{
				return SelectFrom<Contact>
					.Where<Contact.userID.IsEqual<@P.AsGuid>>
					.View
					.ReadOnly
					.Select(graph, userId)
					.FirstOrDefault();
			}
		}


		protected virtual IEnumerable<Contact> ReadContacts(
			PXGraph graph,
			DateTime? since = null,
			// specify to fetch all baccount, not only already read
			bool fetchUnknownItems = false)
		{
			using (Logger.TimeOperationVerbose("Read contacts since {Since} fetch unknown items {FetchUnknownItems}",
				since, fetchUnknownItems))
			{
				BqlCommand select = new SelectFrom<Contact>
					.Where<Contact.userID.IsNotNull>
					.OrderBy<Contact.contactID.Asc>();
				if (since != null)
					select = select.WhereAnd<Where<Contact.lastModifiedDateTime.IsGreaterEqual<@P.AsDateTime>>>();
				List<object> userIds = null;
				if (fetchUnknownItems is false)
				{
					select  = select.WhereAnd<Where<Contact.userID.IsIn<@P.AsGuid>>>();
					var keys = _usersToBAccount.Keys;
					userIds = new List<object>(keys.Count);
					userIds.AddRange(keys.OfType<object>());
				}

				return SelectWithDeletedWindowed<Contact>(graph, select, since, userIds);
			}

		}

		protected virtual IEnumerable<T> SelectWithDeletedWindowed<T>(
			PXGraph graph,
			BqlCommand select,
			DateTime? since,
			List<object> ids)
			where T : class, IBqlTable, new()
		{
			return SelectWithDeletedWindowed<T>(graph, new[] {select}, since, ids);
		}

		protected virtual IEnumerable<T> SelectWithDeletedWindowed<T>(
			PXGraph graph,
			IEnumerable<BqlCommand> selects,
			DateTime? since,
			List<object> ids)
			where T : class, IBqlTable, new()
		{
			using (new PXReadDeletedScope())
			{
				var views = selects.Select(s => new PXView(graph, true, s)).ToList();
				if (ids != null)
				{
					object[] idPars = null;
					object[] pars;
					if (since != null)
					{
						pars = new object[2];
						pars[0] = since;
					}
					else
					{
						pars = new object[1];
					}

					for (int start = 0, count = _batchSize; start < ids.Count; start += _batchSize)
					{
						if (ids.Count < start + count)
						{
							count = ids.Count - start;
							idPars = new object[count];
						}

						idPars ??= new object[_batchSize];
						ids.CopyTo(start, idPars, 0, count);
						pars[pars.Length - 1] = idPars;

						foreach (var item in InnerSelect(pars))
						{
							yield return item;
						}
					}
				}
				else
				{
					var pars = since != null ? new object[]{since } : null;
					foreach (var item in InnerSelect(pars))
					{
						yield return item;
					}
				}

				IEnumerable<T> InnerSelect(object[] pars)
				{
					foreach (var view in views)
					{
						List<object> result;
						int          startRow = 0;
						do
						{
							// windowed for view the only REAL windowed for now
							result   =  view.SelectWindowed(null, pars, null, null, startRow, _batchSize);
							startRow += _batchSize;
							foreach (T item in result)
							{
								yield return item;
							}
						} while (result.Count > 0);
					}
				}
			}
		}

		protected virtual PXGraph GetGraph() => PXGraph.CreateInstance<PXGraph>();

		// parameters for debug primarily
		public virtual void Prefetch(bool fetchUnknownItems = false, bool fetchWithoutDateRestriction = false)
		{
			//DateTime? newLastReadDateTime, since;
			// still update last read date even if it fetches all
			var (newLastReadDateTime, since) = (PXTimeZoneInfo.UtcNow, _lastReadDateTime);
			since = fetchWithoutDateRestriction ? null : since;
			try
			{
				var graph = GetGraph();
				// just refetch recent known items
				UpdateBAccountTree(ReadBAccounts(graph, since, fetchUnknownItems));
				UpdateUserToBAccountAssociations(ReadContacts(graph, since, fetchUnknownItems));
			}
			catch (Exception e)
			{
				Logger.Error(e, "Unexpected error during prefetch of BAccount Tree.");
				throw;
			}

			_lastReadDateTime = newLastReadDateTime;
		}

		private void LogNodeForBAccount(ILogger logger, Node node, int baccountId)
		{
			if (logger.IsEnabled(LogEventLevel.Verbose))
			{
				var (log, foundStatus) = node != null
					? (logger.ForContext("Node", node.ForLogging(), destructureObjects: true), "found")
					: (logger, "not found");
				log.Verbose("Node for business account {BAccountID} was {FoundStatus}", baccountId, foundStatus);
			}
		}

		internal class Node : IEquatable<Node>
		{
			public int Key { get; }

			public NodeForLogging ForLogging() => new NodeForLogging(this);

			public Node(int key)
			{
				Key = key;
			}

			private HashSet<Node> _children;
			public IReadOnlyCollection<Node> Children => (IReadOnlyCollection<Node>)_children ?? Array.Empty<Node>();

			private Node _parent;
			public Node Parent
			{
				get => _parent;
				set
				{
					if (ReferenceEquals(_parent, value))
						return;

					_parent?.RemoveChild(this);
					_parent = value;
					_parent?.AddChild(this);
				}
			}

			private bool RemoveChild(Node child)
			{
				return _children?.Remove(child) ?? false;
			}

			private bool AddChild(Node child)
			{
				_children ??= new HashSet<Node>();
				return _children.Add(child);
			}

			public Node Find(int key)
			{
				using (Logger.TimeOperationVerbose("Find node {key}", key))
				{
					return FindInner(key);
				}
			}

			private Node FindInner(int key)
			{
				if (Key == key)
					return this;
				if (Children == null)
					return null;

				foreach (var child in Children)
				{
					if (child.FindInner(key) is { } result)
						return result;
				}
				return null;
			}

			public IReadOnlyCollection<Node> Flatten()
			{
				var result = new HashSet<Node>();
				Flatten(result);
				return result;
			}

			private void Flatten(HashSet<Node> result)
			{
				if (result.Add(this) && _children != null)
				{
					foreach (var child in _children)
					{
						child.Flatten(result);
					}
				}
			}

			// optimized version of Flatten
			public IReadOnlyCollection<int> FlattenKeys()
			{
				var result = new HashSet<int>();
				FlattenKeys(result);
				return result;
			}

			private void FlattenKeys(HashSet<int> result)
			{
				if (result.Add(Key) && _children != null)
				{
					foreach (var child in _children)
					{
						child.FlattenKeys(result);
					}
				}
			}

			public override string ToString()
			{
				return (Key == default
					? "Root"
					: $"Key = {Key}")
					+ $", Children = {Children?.Count ?? 0}, Total = {Flatten().Count()}";
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as Node);
			}

			public bool Equals(Node other)
			{
				return other != null && Key == other.Key;
			}

			public override int GetHashCode()
			{
				return Key;
			}

			// hack: remove Parent to avoid recursion in structured logging
			public readonly struct NodeForLogging
			{
				private readonly Node _node;

				public NodeForLogging(Node node)
				{
					_node = node;
				}

				public int Key => _node.Key;

				public IEnumerable<NodeForLogging> Children => _node.Children?.Select(n => n.ForLogging())
					?? Enumerable.Empty<NodeForLogging>();
			}
		}

		private class SlotHolder : IPrefetchable<PortalBAccountVisibilityProvider>
		{
			public PortalBAccountVisibilityProvider Instance { get; private set; }

			public void Prefetch(PortalBAccountVisibilityProvider parameter)
			{
				Instance = parameter;
				Instance.Prefetch();
			}
		}
	}
}
