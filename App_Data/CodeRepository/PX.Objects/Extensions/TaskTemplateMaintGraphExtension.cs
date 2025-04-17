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
using PX.Api;
using PX.Common;
using PX.Common.Extensions;
using PX.Data;
using PX.Data.Process;
using PX.Objects.CR;
using PX.SM;
using PX.TM;
using PX.Data.Wiki.Parser.BlockParsers;
using PX.Data.Wiki.Parser;
using PX.PushNotifications.SourceProcessors;

namespace PX.Objects
{
    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod [extension should be constantly active]
    public class TaskTemplateMaintGraphExtension : PXGraphExtension<TaskTemplateMaint>
    {
        #region Constants and fields

        private const string EntityKey = "Entity";
        private const string OwnersKey = "Owners";

        private const char KeySeparator = '-';
        internal const char ValueSeparator = '|';

        private readonly PXGraph taskGraph = PXGraph.CreateInstance<CRTaskMaint>();
        private readonly BqlCommand ownerSelect = new OwnerAttribute().GetSelect();

        private static readonly Type[] fieldsList = new[]
        {
            typeof(CRActivity.startDate),
            typeof(CRActivity.endDate),
            typeof(CRActivity.priority),
            typeof(CRActivity.uistatus),
            typeof(CRActivity.categoryID),
            typeof(CRActivity.workgroupID),
            typeof(CRActivity.contactID),
            typeof(CRActivity.bAccountID),
            typeof(CRActivity.isPrivate),
            typeof(CRReminder.isReminderOn),
            typeof(CRReminder.reminderDate),
            typeof(PMTimeActivity.projectID),
            typeof(PMTimeActivity.projectTaskID),
        };

		[InjectDependency]
		private IPXPageIndexingService PageIndexingService { get; set; }

		#endregion

		#region Event handlers

		[PXFieldNamesList(typeof(CRTaskMaint),
            typeof(CRActivity.startDate),
            typeof(CRActivity.endDate),
            typeof(CRActivity.priority),
            typeof(CRActivity.uistatus),
            typeof(CRActivity.categoryID),
            typeof(CRActivity.workgroupID),
            typeof(CRActivity.contactID),
            typeof(CRActivity.bAccountID),
            typeof(CRActivity.isPrivate),
            typeof(CRReminder.isReminderOn),
            typeof(CRReminder.reminderDate),
            typeof(PMTimeActivity.projectID),
            typeof(PMTimeActivity.projectTaskID))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public void TaskTemplateSetting_FieldName_CacheAttached(PXCache cache) { }

        [PXFieldValuesList(4000, typeof(CRTaskMaint), typeof(TaskTemplateSetting.fieldName), ExclusiveValues = false, IsActive = false)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        public void TaskTemplateSetting_Value_CacheAttached(PXCache cache) { }

        protected virtual void TaskTemplate_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (!(e.Row is TaskTemplate row)) return;

            var settingsCache = Base.TaskTemplateSettings.Cache;
            if (row.TaskTemplateID < 0 && !settingsCache.Inserted.Any_())
            {
                foreach (var fieldType in fieldsList)
                {
                    ((TaskTemplateSetting)settingsCache.NonDirtyInsert()).FieldName =
                        PXFieldNamesListAttribute.MergeNames(fieldType.DeclaringType.Name, fieldType.Name);
                }
            }
        }

        protected virtual void TaskTemplateSetting_Value_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
        {
            if (e.Row is TaskTemplateSetting row)
            {
                Base.UpdateValueFieldState(cache, row);
                InsertOrUpdateValueInCache(row);
            }
        }

        protected virtual void TaskTemplateSetting_Value_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row is TaskTemplateSetting row)
            {
                InsertOrUpdateValueInCache(row);
            }
        }

        #endregion

        #region Delegates

        protected IEnumerable screenOwnerItems(string parent, Boolean KeyEqualsPath = false)
	    {
            if (Base.CurrentSiteMapNode == null)
                yield break;
            if (PXView.Searches?.FirstOrDefault() is string search)
                parent = search.FirstSegment(ValueSeparator);
            if (parent == null)
            {
                yield return new CacheEntityItem {Key = EntityKey, Name = EntityKey, Number = 0};
                yield return new CacheEntityItem {Key = OwnersKey, Name = OwnersKey, Number = 1};
            }
            else
            {
                if (parent.OrdinalEquals(OwnersKey))
                {
                    foreach (var item in GetFirstLettersOfOwnerNames(parent))
                        yield return item;
                }
                else if (parent.StartsWith(OwnersKey))
                {
                    foreach (var item in GetAllOwnerNames(parent))
                        yield return item;
                }
                else if (parent.OrdinalEquals(EntityKey))
                {
                    if (Base.CurrentScreenIsGI)
                    {
                        foreach (var item in GetAllOwnerFieldsForGI(parent, KeyEqualsPath))
                            yield return item;
                    }
                    else
                    {
                        //Acuminator disable once PX1084 GraphCreationInDataViewDelegate
                        //[graph is needed to form the list of fields of primary view with OwnerAttribute]
                        foreach (var item in GetOwnerFieldsForEntry(parent, KeyEqualsPath))
                            yield return item;
                    }
                }
            }
            yield break;
        }

		protected IEnumerable ScreenOwnerItemsWithPaths(string parent)
		{
			return screenOwnerItems(parent, true);
		}

		protected IEnumerable screenOwnerUserItems()
		{
			var parent = OwnersKey;
			if (Base.CurrentSiteMapNode == null)
				yield break;
			if (PXView.Searches?.FirstOrDefault() is string search)
				parent = search.FirstSegment(ValueSeparator);
			if (parent.OrdinalEquals(OwnersKey))
			{
				foreach (var item in GetFirstLettersOfOwnerNames(parent))
				{
					if (item.Key.StartsWith(OwnersKey))
					{
						foreach (var userItem in GetAllOwnerNames(item.Key))
							yield return userItem;
					}
				}
			}
			yield break;
		}

		protected IEnumerable screenOwnerEntityItems(string parent)
		{
			if (Base.CurrentSiteMapNode == null)
				yield break;
			if (PXView.Searches?.FirstOrDefault() is string search)
				parent = search.FirstSegment(ValueSeparator);
			if (parent == null)
			{
				yield return new CacheEntityItem { Key = EntityKey, Name = EntityKey, Number = 0 };
			}
			else
			{
				if (parent.OrdinalEquals(EntityKey))
				{
					if (Base.CurrentScreenIsGI)
					{
						foreach (var item in GetAllOwnerFieldsForGI(parent, true))
							yield return item;
					}
					else
					{
						//Acuminator disable once PX1084 GraphCreationInDataViewDelegate
						//[graph is needed to form the list of fields of primary view with OwnerAttribute]
						foreach (var item in GetOwnerFieldsForEntry(parent, true))
							yield return item;
					}
				}
			}
			yield break;
		}

		#endregion

		#region Functions

		///<summary>Inserts or updates data from the Value field of TaskTemplateSetting to the current record of an appropriate cache.</summary>
		///<remarks>Procures work of connected selectors, such as on PMTimeActivity.ProjectID and PMTimeActivity.ProjectTaskID fields.</remarks>
		private void InsertOrUpdateValueInCache(TaskTemplateSetting row)
        {
            if (PXFieldNamesListAttribute.SplitNames(row.FieldName, out var tableName, out var fieldName))
            {
                var taskCache = taskGraph.Caches[tableName];
                if (taskCache == null) return;
                var itemCache = Base.Caches[taskCache.GetItemType()];
                if (itemCache.Current == null) itemCache.Current = itemCache.CreateInstance();
                try { itemCache.SetValueExt(itemCache.Current, fieldName, row.Value); }
                catch {/* Prevents errors when connected field value is not set */}
            }
        }

        private IEnumerable<CacheEntityItem> GetFirstLettersOfOwnerNames(string parent)
        {
            foreach (var letter in new PXView(Base, false, ownerSelect)
                .SelectMulti().Cast<Contact>().Where(c => !string.IsNullOrEmpty(c.DisplayName))
                .Select(c => c.DisplayName.Substring(0, 1).ToUpper()).OrderBy(c => c).Distinct())
            {
                yield return new CacheEntityItem {Name = letter, Key = parent + KeySeparator + letter};
            }
        }

        private IEnumerable<CacheEntityItem> GetAllOwnerNames(string parent)
        {
            var letter = parent.LastSegment(KeySeparator);
            if (letter.IndexOf(ValueSeparator) > 0) yield break;

            foreach (Contact owner in new PXView(Base, false, ownerSelect)
                .SelectMulti().Cast<Contact>().Where(c => c.DisplayName.OrdinalStartsWith(letter)))
            {
                yield return new CacheEntityItem
                {
                    Name = owner.DisplayName + (string.IsNullOrEmpty(owner.Salutation) ? "" : $" ({owner.Salutation})"),
                    Key = parent + ValueSeparator + owner.ContactID,
                    Path = parent + ValueSeparator + owner.ContactID
                };
            }
        }

        private IEnumerable<CacheEntityItem> GetOwnerFieldsForEntry(string parent, Boolean KeyEqualsPath = false)
        {
            var result = new Dictionary<string, string>();
			var prevResult = new Dictionary<string, string>();
            var tables = new List<Type>();
            var graph = PXGraph.CreateInstance(GraphHelper.GetType(Base.CurrentSiteMapNode.GraphType));
            var cache = graph.Views[graph.PrimaryView].Cache;
            var fields = cache.Fields.ToDictionary(c => c, c=>PushNotificationsProcessorHelper.FormFieldKey(graph.PrimaryView, c), StringComparer.OrdinalIgnoreCase);
            var views = EMailSourceHelper.TemplateEntity(Base, null, null, Base.CurrentSiteMapNode.GraphType, true, true);
			var primaryView = PageIndexingService.GetPrimaryView(Base.CurrentSiteMapNode.GraphType);

			foreach (string viewName in views.OfType<CacheEntityItem>().Select(c => c.Key))
			{
				if (!graph.Views.TryGetOrCreateValue(viewName, out var view))
				{
					continue;
				}

				if (viewName.Equals(primaryView, StringComparison.OrdinalIgnoreCase))
				{
					var allowedItems = Base.GetAllowedItems();
					EnumOwnerFieldsWithPrevios(graph, view.CacheType(), tables, result, prevResult, fields, primaryView, allowedItems,
						enumEntryScreenSelectors: true);
				}
				else
				{
					EnumOwnerFields(null, null, graph, view.CacheType(), tables, result, fields);
				}
			}

            var num = 0;
			foreach (var c in result)
			{
				yield return new CacheEntityItem
				{
					Key = KeyEqualsPath ? parent + ValueSeparator + $"(({c.Key}))" : c.Value,
					Name = c.Value,
					Number = num++,
					Path = parent + ValueSeparator + $"(({c.Key}))"
				};
			}
			foreach (var p in prevResult)
			{
				yield return new CacheEntityItem
				{
					Key = KeyEqualsPath ? parent + ValueSeparator + PreviousValueHelper.GetPrevFunctionInvocationText(p.Key) : SMNotificationMaint.GetPrevKey(p.Value),
					Name = SMNotificationMaint.GetPrevName(p.Value),
					Number = num++,
					Path = parent + ValueSeparator + PreviousValueHelper.GetPrevFunctionInvocationText(p.Key)
				};
			}
		}
		private IEnumerable<CacheEntityItem> GetAllOwnerFieldsForGI(string parent, Boolean KeyEqualsPath = false)
		{
			var result = new Dictionary<string, string>();
			var prevResult = new Dictionary<string, string>();
			var tables = new List<Type>();
            var graph = PXGenericInqGrph.CreateInstance(Base.CurrentSiteMapNode.ScreenID);
            var usedTables = graph.BaseQueryDescription.UsedTables;
			var allowedItems = Base.GetAllowedItems();

			foreach (var group in graph.ResultColumns.GroupBy(c => c.ObjectName))
            {
                if (usedTables.TryGetValue(group.Key, out PX.Data.Description.GI.PXTable table))
                {
                    var fields = group.ToDictionary(c => c.Field, c => c.FieldName, StringComparer.OrdinalIgnoreCase);
					var tableName = table.BqlTable.Name;
					EnumOwnerFieldsWithPrevios(graph, table.CacheType, tables, result, prevResult, fields, tableName, allowedItems);
				}
            }

            var num = 0;
            foreach (var c in result)
			{
				yield return new CacheEntityItem
				{
					Key = KeyEqualsPath ? parent + ValueSeparator + $"(({c.Key}))" : c.Value,
					Name = c.Value,
					Number = num++,
					Path = parent + ValueSeparator + $"(({c.Key}))"
				};
			}

			foreach (var p in prevResult)
			{
				yield return new CacheEntityItem
				{
					Key = KeyEqualsPath ? parent + ValueSeparator + PreviousValueHelper.GetPrevFunctionInvocationText(p.Key) : SMNotificationMaint.GetPrevKey(p.Value),
					Name = SMNotificationMaint.GetPrevName(p.Value),
					Number = num++,
					Path = parent + ValueSeparator + PreviousValueHelper.GetPrevFunctionInvocationText(p.Key)
				};
			}
        }

		private static void EnumOwnerFieldsWithPrevios(PXGraph graph, Type table, List<Type> tables, Dictionary<string, string> result,
			Dictionary<string, string> prevResult, Dictionary<string, string> fields, string tableName,
			Dictionary<string, HashSet<string>> allowedItems, bool enumEntryScreenSelectors = false)
		{
			var tableResult = new Dictionary<string, string>();
			EnumOwnerFields(null, null, graph, table, tables, tableResult, fields);

			foreach (var r in tableResult)
			{
				if (result.ContainsKey(r.Key))
				{
					continue;
				}

				result.Add(r.Key, r.Value);

				var fieldSeparatorIndex = r.Key.IndexOf(ParserSeparators.SelectorsTreeFacetsSeparator);
				var fieldName = fieldSeparatorIndex == -1 ? r.Key : r.Key.Substring(0, fieldSeparatorIndex);

				var entryScreenFieldSeparatorIndex = fieldName.IndexOf(ParserSeparators.EntryScreenViewFieldSeparator);
				if (entryScreenFieldSeparatorIndex != -1)
				{
					fieldName = fieldName.Substring(entryScreenFieldSeparatorIndex + 1);
				}
				else
				{
					var giScreenFieldSeparatorIndex = fieldName.IndexOf(ParserSeparators.GIViewFieldSeparator);
					if (giScreenFieldSeparatorIndex != -1)
					{
						fieldName = fieldName.Substring(giScreenFieldSeparatorIndex + 1);
					}
				}

				var isPrevAllowed = allowedItems == null || allowedItems.TryGetValue(tableName, out var fieldsSet) &&
					fieldsSet.Contains(fieldName);
				if (!isPrevAllowed)
				{
					continue;
				}
				prevResult.Add(r.Key, r.Value);
			}
		}

        private static void EnumOwnerFields(string internalPath, string displayPath, PXGraph graph, Type table, List<Type> tables, Dictionary<string, string> names, Dictionary<string, string> fields)
        {
            Func<Type, List<string>> getFieldsDelegate = OwnerAttribute.GetFields;
            PX.SM.TemplateGraphHelper.EnumAssigneeFields(internalPath, displayPath, graph, table, tables, names, fields, getFieldsDelegate);
        }
        #endregion
    }
}
