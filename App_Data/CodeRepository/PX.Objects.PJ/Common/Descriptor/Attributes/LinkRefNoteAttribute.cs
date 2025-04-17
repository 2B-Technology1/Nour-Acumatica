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
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;

namespace PX.Objects.PJ.Common.Descriptor.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LinkRefNoteAttribute : PXDBGuidAttribute
    {
        private readonly Type[] referenceFieldTypes;
        private PXGraph graph;
        private EntityHelper helper;

        public LinkRefNoteAttribute(params Type[] referenceFieldTypes)
        {
            this.referenceFieldTypes = referenceFieldTypes;
        }

        public override void CacheAttached(PXCache cache)
        {
            base.CacheAttached(cache);
            graph = cache.Graph;
            helper = new EntityHelper(cache.Graph);
            InitializeViewActionForReferenceType(cache);
        }

        public override void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
        {
            if (args.Row != null)
            {
                using (new PXReadBranchRestrictedScope())
                {
                    var noteId = (Guid?) cache.GetValue(args.Row, _FieldOrdinal);
                    args.ReturnValue = noteId.HasValue
                        ? GetEntityRowId(noteId.Value)
                        : string.Empty;
                }
            }
        }

        private string GetEntityRowId(Guid noteId)
        {
            var (row, referenceType, referenceFieldType) = GetEntityRow(noteId);
            return row != null
                ? graph.Caches[referenceType].GetValue(row, referenceFieldType).ToString()
                : string.Empty;
        }

        private (object row, Type referenceType, Type referenceFieldType) GetEntityRow(Guid? noteId)
        {
            foreach (var type in referenceFieldTypes)
            {
                var row = helper.GetEntityRow(type.DeclaringType, noteId);
                if (row != null)
                {
                    var referenceType = type.DeclaringType;
                    var referenceFieldType = type;
                    return (row, referenceType, referenceFieldType);
                }
            }
            return (null, null, null);
        }

        private void InitializeViewActionForReferenceType(PXCache cache)
        {
            var actionName = GetActionName(cache);
            cache.Graph.Actions[actionName] = (PXAction) Activator.CreateInstance(
                typeof(PXNamedAction<>).MakeGenericType(GetDacTypeOfPrimaryView(cache)),
                cache.Graph,
                actionName,
                (PXButtonDelegate) ViewReferenceEntity,
                GetEventSubscriberAttributes());
            cache.Graph.Actions[actionName].SetVisible(false);
        }

        private string GetActionName(PXCache cache)
        {
            return $"{cache.GetItemType().Name}${_FieldName}$Link";
        }

        private IEnumerable ViewReferenceEntity(PXAdapter adapter)
        {
            var cache = adapter.View.Graph.Caches[BqlTable];
            var noteId = (Guid?) cache.GetValue(cache.Current, _FieldOrdinal);
            if (noteId != null)
            {
                RedirectToEntity(cache, noteId);
            }
            return adapter.Get();
        }

        private void RedirectToEntity(PXCache cache, Guid? noteId)
        {
            var primaryGraph = GetPrimaryGraph(cache, noteId);
            if (primaryGraph is ChangeRequestEntry && !PXAccess.FeatureInstalled<FeaturesSet.changeRequest>())
            {
                throw new PXException(ProjectManagementMessages.ChangeRequestsFeatureHasBeenDisabled);
            }
            PXRedirectHelper.TryRedirect(primaryGraph, PXRedirectHelper.WindowMode.NewWindow);
        }

        private PXGraph GetPrimaryGraph(PXCache cache, Guid? noteId)
        {
            var (row, referenceType, _) = GetEntityRow(noteId);
            var primaryGraphType = GetPrimaryGraphType(cache, row, referenceType);
            var primaryGraph = PXGraph.CreateInstance(primaryGraphType);
            primaryGraph.GetPrimaryCache().Current = row;
            return primaryGraph;
        }

        /// <summary>
        /// Temporary fix for Change Request
        /// because Acumatica commented out the PXPrimaryGraph attribute in PMChangeRequest DAC.
        /// </summary>
        private static Type GetPrimaryGraphType(PXCache cache, object row, Type referenceType)
        {
            var graphType = EntityHelper.GetPrimaryGraphType(cache.Graph, referenceType);
            return graphType == null && row is PMChangeRequest
                ? typeof(ChangeRequestEntry)
                : graphType;
        }

        private static PXEventSubscriberAttribute[] GetEventSubscriberAttributes()
        {
            return new PXEventSubscriberAttribute[]
            {
                new PXUIFieldAttribute
                {
                    MapEnableRights = PXCacheRights.Select
                }
            };
        }

        private static Type GetDacTypeOfPrimaryView(PXCache cache)
        {
            return cache.Graph.Views.ContainsKey(cache.Graph.PrimaryView)
                ? cache.Graph.Views[cache.Graph.PrimaryView].GetItemType()
                : cache.BqlTable;
        }
    }
}
