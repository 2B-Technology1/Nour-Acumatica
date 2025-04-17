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
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.PO;

namespace PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes
{
    public class RelatedEntitySelectorAttribute : PXRefNoteSelectorAttribute
    {
        private const string NavigateByReferenceNoteAction = "ProjectIssue$Navigate_ByRefNote";
        private const string ReferenceNoteView = "ProjectIssue$RefNoteView";

        public RelatedEntitySelectorAttribute(Type primaryViewType, Type refNoteIdField)
            : base(primaryViewType, refNoteIdField)
        {
        }

        public override void ViewCreated(PXGraph graph, string viewName)
        {
            base.ViewCreated(graph, viewName);
            OverrideNavigationAction(graph);
        }

        private static void OverrideNavigationAction(PXGraph graph)
        {
            PXNamedAction.AddAction(graph, typeof(ProjectIssue), NavigateByReferenceNoteAction, string.Empty, false,
                NavigateToRelatedEntity);
        }

        private static IEnumerable NavigateToRelatedEntity(PXAdapter adapter)
        {
            var graph = adapter.View.Graph;
            var relatedEntity = graph.Views[ReferenceNoteView].Cache.Current as RelatedEntity;
            if (relatedEntity?.RefNoteID != null)
            {
                NavigateToRelatedEntity(graph, relatedEntity.RefNoteID);
            }
            return adapter.Get();
        }

        private static void NavigateToRelatedEntity(PXGraph graph, Guid? noteId)
        {
            var entityHelper = new EntityHelper(graph);
            var entity = entityHelper.GetEntityRow(noteId) as POOrder;
            if (entity?.OrderType == POOrderType.RegularSubcontract)
            {
                NavigateToSubcontractEntity(entity);
            }
            entityHelper.NavigateToRow(noteId, PXRedirectHelper.WindowMode.NewWindow);
        }

        private static void NavigateToSubcontractEntity(POOrder entity)
        {
            var subcontractEntry = PXGraph.CreateInstance<SubcontractEntry>();
            subcontractEntry.Document.Current = entity;
            throw new PXRedirectRequiredException(subcontractEntry, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }
    }
}
