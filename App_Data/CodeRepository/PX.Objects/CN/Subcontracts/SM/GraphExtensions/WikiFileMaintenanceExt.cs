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
using PX.Objects.CN.Subcontracts.PM.CacheExtensions;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CN.Subcontracts.SM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.SM;
using Messages = PX.Objects.CN.Subcontracts.PM.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.SM.GraphExtensions
{
    public class WikiFileMaintenanceExt : PXGraphExtension<WikiFileMaintenance>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.construction>();
	    }

		public PXSelect<NoteDoc> Entities;
        public PXAction<UploadFileWithIDSelector> ViewEntityGraph;

        private EntityHelper entityHelper;

        public override void Initialize()
        {
            entityHelper = new EntityHelper(Base);
        }

	    internal protected IEnumerable entitiesRecords()
	    {
		    foreach (NoteDoc noteDocument in Base.entitiesRecords())
		    {
			    UpdateNoteDocumentForSubcontract(noteDocument);
			    yield return noteDocument;
		    }
		}

		[PXOverride]
        public virtual IEnumerable viewEntity(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
        {
            if (Entities.Current != null)
            {
                var commitment = GetCommitment(Entities.Current);
                if (commitment?.OrderType == POOrderType.RegularSubcontract)
                {
                    RedirectToSubcontractEntry(commitment);
                }
            }
			return baseHandler(adapter);
		}

        private static void RedirectToSubcontractEntry(POOrder subcontract)
        {
            var graph = PXGraph.CreateInstance<SubcontractEntry>();
            graph.Document.Current = subcontract;
            throw new PXRedirectRequiredException(graph, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private void UpdateNoteDocumentForSubcontract(NoteDoc noteDocument)
        {
            var commitment = GetCommitment(noteDocument);
            if (commitment?.OrderType == POOrderType.RegularSubcontract)
            {
                noteDocument.EntityName = Constants.SubcontractTypeName;
                noteDocument.EntityRowValues = GetRewrittenEntityRowValues(commitment);
            }
        }

        private string GetRewrittenEntityRowValues(POOrder subcontract)
        {
            var cache = Base.Caches<POOrder>();
            PXUIFieldAttribute.SetVisibility<POOrder.orderType>(cache, null, PXUIVisibility.Invisible);
            var description = entityHelper.DescriptionEntity(typeof(POOrder), subcontract);
            PXUIFieldAttribute.SetVisibility<POOrder.orderType>(cache, null, PXUIVisibility.SelectorVisible);
            return description;
        }

        private POOrder GetCommitment(NoteDoc noteDocument)
        {
            var query = new PXSelect<POOrder,
                Where<POOrder.noteID, Equal<Required<POOrder.noteID>>>>(Base);
            return query.SelectSingle(noteDocument.NoteID);
        }
    }
}
