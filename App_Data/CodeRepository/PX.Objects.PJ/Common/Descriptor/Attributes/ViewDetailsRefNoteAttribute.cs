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
using PX.Data;

namespace PX.Objects.PJ.Common.Descriptor.Attributes
{
    public class ViewDetailsRefNoteAttribute : PXEventSubscriberAttribute
    {
        private readonly Type[] referenceTypes;
        private EntityHelper helper;

        public ViewDetailsRefNoteAttribute(params Type[] referenceTypes)
        {
            this.referenceTypes = referenceTypes;
        }

        public override void CacheAttached(PXCache cache)
        {
            base.CacheAttached(cache);
            helper = new EntityHelper(cache.Graph);
            InitializeViewActionForReferenceType(cache);
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
                var row = GetEntityRow(noteId, out var referenceType);
                var graphType = EntityHelper.GetPrimaryGraphType(cache.Graph, referenceType);
                var graph = PXGraph.CreateInstance(graphType);
                PXRedirectHelper.TryRedirect(graph, row, PXRedirectHelper.WindowMode.NewWindow);
            }
            return adapter.Get();
        }

        private object GetEntityRow(Guid? noteId, out Type referenceType)
        {
            object row = null;
            referenceType = null;
            foreach (var type in referenceTypes)
            {
                row = helper.GetEntityRow(type, noteId);
                if (row != null)
                {
                    referenceType = type;
                    break;
                }
            }
            return row;
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
