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
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.PM.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PM.GraphExtensions
{
    public class ProjectEntryExt : PXGraphExtension<ProjectEntry>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.construction>();
	    }

		public override void Initialize()
        {
            AddSubcontractType();
        }

        [PXOverride]
        public virtual IEnumerable ViewPurchaseOrder(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
        {
            var commitment = Base.PurchaseOrders.Current;
            if (commitment.OrderType == POOrderType.RegularSubcontract)
            {
                RedirectToSubcontractEntry(commitment);
            }
            return baseHandler(adapter);
        }

		public virtual bool CreateSubcontractVisible()
		{
			return Base.CostCommitmentTrackingEnabled()
				&& !(Base.RequireSingleProjectPerDocument() && Base.IsUserNumberingOn(PX.Objects.CN.Subcontracts.PO.Descriptor.Messages.PoSetup.SubcontractNumberingName));
		}

		protected virtual void _(Events.RowSelected<PMProject> e)
		{
			createSubcontract.SetVisible(CreateSubcontractVisible());
		}

		private static void RedirectToSubcontractEntry(POOrder commitment)
        {
            var graph = PXGraph.CreateInstance<SubcontractEntry>();
            graph.Document.Current = commitment;
            throw new PXRedirectRequiredException(graph, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private void AddSubcontractType()
        {
            var allowedValues = POOrderType.RegularSubcontract.CreateArray();
            var allowedLabels = Messages.Subcontract.CreateArray();
            PXStringListAttribute.AppendList<POOrder.orderType>(Base.PurchaseOrders.Cache, null, allowedValues,
                allowedLabels);
        }

        public PXAction<PMProject> createSubcontract;
        [PXUIField(DisplayName = Messages.CreateSubcontract, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable CreateSubcontract(PXAdapter adapter)
        {
	        return Base.CreatePOOrderBase<SubcontractEntry>(adapter, Messages.CreateSubcontract);
        }
	}
}
