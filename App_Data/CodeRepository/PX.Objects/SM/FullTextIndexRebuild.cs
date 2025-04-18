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
using System.Diagnostics;
using System.Linq;
using PX.Data;
using PX.Data.Search;
using PX.Data.Wiki.Parser;
using PX.Data.Wiki.Parser.PlainTxt;
using PX.Objects.BQLConstants;
using PX.SM;
using PX.Web.UI;
using Messages = PX.Objects.CA.Messages;


namespace PX.Objects.SM
{
    public class FullTextIndexRebuild : PXGraph<FullTextIndexRebuild>
    {
        public PXCancel<FullTextIndexRebuildProc.RecordType> Cancel;
        public PXProcessing<FullTextIndexRebuildProc.RecordType> Items;

        public PXSelectJoin<WikiPage,
            InnerJoin<WikiPageLanguage, On<WikiPageLanguage.pageID, Equal<WikiPage.pageID>>,
            InnerJoin<WikiRevision, On<WikiRevision.pageID, Equal<WikiPage.pageID>>>>,
            Where<WikiRevision.plainText, Equal<EmptyString>, 
            And<WikiRevision.pageRevisionID, Equal<WikiPageLanguage.lastRevisionID>>>> WikiArticles;
        
        [InjectDependency]
        private ISearchManagementService SearchManagementService { get; set; }
		
		/* All revisions:
        public PXSelectJoin<WikiPage,
            InnerJoin<WikiPageLanguage, On<WikiPageLanguage.pageID, Equal<WikiPage.pageID>>,
            InnerJoin<WikiRevision, On<WikiRevision.pageID, Equal<WikiPage.pageID>>>>,
            Where<WikiRevision.plainText, Equal<EmptyString>>> WikiArticles;*/

        public virtual IEnumerable items()
        {
            bool found = false;
            foreach (FullTextIndexRebuildProc.RecordType item in Items.Cache.Inserted)
            {
                found = true;
                yield return item;
            }
            if (found)
                yield break;

            foreach (Type entity in PXSearchableAttribute.GetAllSearchableEntities(this))
            {
                yield return Items.Insert(new FullTextIndexRebuildProc.RecordType() { Entity = entity.FullName, Name = entity.Name, DisplayName = Caches[entity].DisplayName });
            }

            Items.Cache.IsDirty = false;
        }

        public FullTextIndexRebuild()
        {
            Items.SetProcessDelegate<FullTextIndexRebuildProc>(FullTextIndexRebuildProc.BuildIndex);
        }

        #region Actions/Buttons

        public PXAction<FullTextIndexRebuildProc.RecordType> clearAllIndexes;
        [PXUIField(DisplayName = Messages.ClearAllIndexes, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton(Tooltip = Messages.ClearAllIndexesTip, Category = ActionsMessages.Actions)]
        public virtual IEnumerable ClearAllIndexes(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, delegate()
            {
                PXDatabase.Delete(typeof(SearchIndex));
            });

            return adapter.Get();
        }

        public PXAction<FullTextIndexRebuildProc.RecordType> indexCustomArticles;
        [PXUIField(DisplayName = Messages.IndexCustomArticles, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton(Tooltip = Messages.IndexCustomArticles, Category = ActionsMessages.Actions)]
        public virtual IEnumerable IndexCustomArticles(PXAdapter adapter)
        {
			PXLongOperation.StartOperation(this, delegate()
            {
				foreach (var result in WikiArticles.Select())
				{
					string plaintext = null;
                        
					var _wp = (WikiPage)result[typeof(WikiPage)];
					var _wr = (WikiRevision)result[typeof(WikiRevision)];
					var _wl = (WikiPageLanguage) result[typeof (WikiPageLanguage)];

					if (_wp.IsHtml != true)
					{
						WikiReader reader = PXGraph.CreateInstance<WikiReader>();
						PXWikiSettings settings = new PXWikiSettings(new PXPage(), reader);
						PXTxtRenderer renderer = new PXTxtRenderer(settings.Absolute);
						var ctx = new PXDBContext(settings.Absolute);
						ctx.Renderer = renderer;
						plaintext = (_wl.Title ?? "") + Environment.NewLine + PXWikiParser.Parse(_wr.Content, ctx);
					}
					else
					{
						plaintext = (_wl.Title ?? "") + Environment.NewLine + SearchService.Html2PlainText(_wr.Content);
					}


					//Try updating the article in current Company
					if (!PXDatabase.Update<WikiRevision>(
						new PXDataFieldAssign("PlainText", PXDbType.NVarChar, plaintext),
						new PXDataFieldRestrict("PageID", PXDbType.UniqueIdentifier, _wr.PageID),
						new PXDataFieldRestrict("PageRevisionID", PXDbType.Int, _wr.PageRevisionID),
						new PXDataFieldRestrict("Language", PXDbType.VarChar, _wr.Language)
						))
					{
						//Article may be shared. Try updating the article through graph (thus handling the shared record update stratagy)
						//if article is not updatable an exception may be thrown - ignore.
						try
						{
							ArticleUpdater updater = PXGraph.CreateInstance<ArticleUpdater>();
							WikiRevision rev = updater.Revision.Select(_wr.PageID, _wr.PageRevisionID, _wr.Language);
							rev.PlainText = plaintext;
							updater.Revision.Update(rev);
							updater.Persist();
						}
						catch (Exception ex)
						{
							PXTrace.WriteInformation("Plain text field could not be updated for article = {0}. Error Message: {1}", _wr.PageID, ex.Message);
						}
						
					}
				}
			});

			

            return adapter.Get();
        }
        
        public PXAction<FullTextIndexRebuildProc.RecordType> restartFts;
        [PXUIField(DisplayName = Messages.RestartFTS, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton(Tooltip = Messages.RestartFTS, Category = ActionsMessages.Actions)]
        public virtual IEnumerable RestartFts(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, delegate()
            {
                SearchManagementService.RestartFullTextFeature();
            });

            return adapter.Get();
        }
        
        #endregion
    }

	public class ArticleUpdater : PXGraph<ArticleUpdater>
	{
		public PXSelect<WikiRevision, Where<WikiRevision.pageID, Equal<Required<WikiRevision.pageID>>,
			And<WikiRevision.pageRevisionID, Equal<Required<WikiRevision.pageRevisionID>>,
			And<WikiRevision.language, Equal<Required<WikiRevision.language>>>>>> Revision;
		
	}
	

    public class FullTextIndexRebuildProc : PXGraph<FullTextIndexRebuildProc>
    {
        public static void BuildIndex(FullTextIndexRebuildProc graph, RecordType item)
        {
            Debug.Print("Start processing {0}", item.Name);
            Stopwatch sw = new Stopwatch();

            graph.Caches.Clear();
            graph.Clear(PXClearOption.ClearAll);

            PXProcessing<RecordType>.SetCurrentItem(item);
            Type entity = GraphHelper.GetType(item.Entity);
            PXCache entityCache = graph.Caches[entity];
            PXSearchableAttribute searchableAttribute = entityCache.GetAttributes("NoteID")
                                                                   .OfType<PXSearchableAttribute>()
                                                                   .FirstOrDefault();
            if (searchableAttribute == null)
                return;
             
            Type viewType = ComposeViewToSelectRecordsForIndexing(searchableAttribute, entity);
            BqlCommand cmd = BqlCommand.CreateInstance(viewType);

            PXView itemView = new PXView(graph, isReadOnly: true, cmd);
            List<object> resultset;

            List<Type> fieldList = new List<Type>(searchableAttribute.GetSearchableFields(entityCache));
			Type entityForNoteId = entity;

            while (typeof(IBqlTable).IsAssignableFrom(entityForNoteId))
            {
                Type noteIdField = entityForNoteId.GetNestedType("noteID");

                if (null != noteIdField)
                    fieldList.Add(noteIdField);

                entityForNoteId = entityForNoteId.BaseType;
            }

            fieldList.Add(typeof(SearchIndex.noteID));
            fieldList.Add(typeof(SearchIndex.category));
            fieldList.Add(typeof(SearchIndex.content));
            fieldList.Add(typeof(SearchIndex.entityType));
            fieldList.Add(typeof(Note.noteID));
            fieldList.Add(typeof(Note.noteText));

            sw.Start();

			const int batchSize = 50000;
            int startRow = 0;

            do
            {
                using (new PXFieldScope(itemView, fieldList))
                {
                    //resultset = itemView.SelectMulti();
                    resultset = itemView.SelectWindowed(currents: null, parameters: null, sortcolumns: null, descendings: null, startRow, batchSize);
                }

                sw.Stop();
                Debug.Print("{0} GetResultset in {1} sec. Total records={2}", item.DisplayName, sw.Elapsed.TotalSeconds, resultset.Count);
                sw.Reset();
                sw.Start();

                startRow += batchSize;

                int totalcount = resultset.Count;
                int processedCounter = 0;
                int searchableEntitiesCounter = 0;

                try
                {
                    Dictionary<Guid, SearchIndex> insertDict = new Dictionary<Guid, SearchIndex>(resultset.Count);

                    foreach (PXResult res in resultset)
                    {
                        processedCounter++;

                        if (!searchableAttribute.IsSearchable(entityCache, res[entity]))
                            continue;

                        searchableEntitiesCounter++;
                        Note note = (Note)res[typeof(Note)];
                        SearchIndex newSearchIndex = searchableAttribute.BuildSearchIndex(entityCache, res[entity], res, ExtractNoteText(note));
                        SearchIndex existingSearchIndex = (SearchIndex)res[typeof(SearchIndex)];

                        if (existingSearchIndex.NoteID != null && existingSearchIndex.NoteID != newSearchIndex.NoteID)
                        {
                            PXSearchableAttribute.Delete(newSearchIndex);
                        }

                        if (existingSearchIndex.NoteID == null)
                        {
                            if (!insertDict.ContainsKey(newSearchIndex.NoteID.Value))
                                insertDict.Add(newSearchIndex.NoteID.Value, newSearchIndex);
                        }
                        else if (newSearchIndex.Content != existingSearchIndex.Content || newSearchIndex.Category != existingSearchIndex.Category
                                 || newSearchIndex.EntityType != existingSearchIndex.EntityType)
                        {
                            PXSearchableAttribute.Update(newSearchIndex);
                        }
                    }

                    sw.Stop();
                    Debug.Print("{0} Content building in {1} sec. Records processed = {2}. Searchable={3}", item.DisplayName, sw.Elapsed.TotalSeconds, totalcount, searchableEntitiesCounter);
                    sw.Reset();
                    sw.Start();
                    PXSearchableAttribute.BulkInsert(insertDict.Values);
                    sw.Stop();
                    Debug.Print("{0} BulkInsert in {1} sec.", item.DisplayName, sw.Elapsed.TotalSeconds);
                }
                catch (Exception ex)
                {
                    string msg = string.Format(Messages.OutOfProcessed, processedCounter, totalcount, searchableEntitiesCounter, ex.Message);
                    throw new Exception(msg, ex);
                }
            } while (resultset.Count > 0);

            PXProcessing<RecordType>.SetProcessed();
        }

        private static Type ComposeViewToSelectRecordsForIndexing(PXSearchableAttribute searchableAttribute, Type entity)
		{
            Type joinNote = typeof(LeftJoin<Note, On<Note.noteID, Equal<SearchIndex.noteID>>>);
            Type viewType;

            if (searchableAttribute.SelectForFastIndexing != null)
            {
                Type noteEntity = entity;

                if (searchableAttribute.SelectForFastIndexing.IsGenericType)
                {
                    Type[] tables = searchableAttribute.SelectForFastIndexing.GetGenericArguments();

                    if (tables != null && tables.Length > 0 && typeof(IBqlTable).IsAssignableFrom(tables[0]))
                    {
                        noteEntity = tables[0];
                    }
                }

                Type joinSearchIndex = BqlCommand.Compose(
                            typeof(LeftJoin<,>),
                            typeof(SearchIndex),
                            typeof(On<,>),
                            typeof(SearchIndex.noteID),
                            typeof(Equal<>),
                            noteEntity.GetNestedType("noteID"));

                viewType = BqlCommand.AppendJoin(searchableAttribute.SelectForFastIndexing, joinSearchIndex);
                viewType = BqlCommand.AppendJoin(viewType, joinNote);
            }
            else
            {
                Type joinSearchIndex = BqlCommand.Compose(
                            typeof(LeftJoin<,>),
                            typeof(SearchIndex),
                            typeof(On<,>),
                            typeof(SearchIndex.noteID),
                            typeof(Equal<>),
                            entity.GetNestedType("noteID"));

                viewType = BqlCommand.Compose(typeof(Select<>), entity);
                viewType = BqlCommand.AppendJoin(viewType, joinSearchIndex);
                viewType = BqlCommand.AppendJoin(viewType, joinNote);
            }

            return viewType;
        }

        private static string ExtractNoteText(Note note)
        {
            String value = note.NoteText;
            if (String.IsNullOrWhiteSpace(value))
                return null;

            String[] parts = value.Split('\0');
            if (parts.Length < 1)
            {
                return null;
            }

            if (String.IsNullOrWhiteSpace(parts[0]))
            {
                return null;
            }
            else
            {
                return parts[0];
            }
        }

        [Serializable]
        public partial class RecordType : IBqlTable
        {
            #region Selected
            public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
            protected bool? _Selected = false;
            [PXBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Visible)]
            public bool? Selected
            {
                get
                {
                    return _Selected;
                }
                set
                {
                    _Selected = value;
                }
            }
            #endregion

            #region Entity
            public abstract class entity : PX.Data.BQL.BqlString.Field<entity> { }
            protected string _Entity;
            [PXString(250, IsKey = true)]
            [PXUIField(DisplayName = "Entity", Enabled = false)]
            public virtual string Entity
            {
                get { return _Entity; }
                set { _Entity = value; }
            }
            #endregion

            #region Name
            public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
            protected string _Name;
            [PXString(250)]
            [PXUIField(DisplayName = "Entity", Enabled = false)]
            public virtual string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }
            #endregion

            #region DisplayName
            public abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }
            protected string _DisplayName;
            [PXString(250)]
            [PXUIField(DisplayName = "Name", Enabled = false)]
            public virtual string DisplayName
            {
                get { return _DisplayName; }
                set { _DisplayName = value; }
            }
            #endregion
        }
    }

    public class SearchCategory : PX.Data.SearchCategory
    {
        public const int AP = 1;
        public const int AR = 2;
        public const int CA = 4;
        public const int FA = 8;
        public const int GL = 16;
        public const int IN = 32;
        public const int OS = 64;
        public const int PO = 128;
        public const int SO = 256;
        public const int RQ = 512;
        public const int CR = 1024;
        public const int PM = 2048;
        public const int TM = 4096;
        public const int FS = 8192;
        public const int PR = 16384;

        public new static int Parse(string module)
        {
            switch (module)
            {
                case "AP": return AP;
                case "AR": return AR;
                case "CA": return CA;
                case "FA": return FA;
                case "GL": return GL;
                case "IN": return IN;
                case "OS": return OS;
                case "PO": return PO;
                case "SO": return SO;
                case "RQ": return RQ;
                case "CR": return CR;
                case "PM": return PM;
				case "TM": return TM;
                case "FS": return FS;
                case "PR": return PR;

                default: return PX.Data.SearchCategory.Parse(module);
            }
        }
    }

}
