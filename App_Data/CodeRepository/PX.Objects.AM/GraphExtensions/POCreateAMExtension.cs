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

using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using CRLocation = PX.Objects.CR.Standalone.Location;
using System;
using System.Collections.Generic;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.FS;
using PX.Objects.Common.DAC;

namespace PX.Objects.AM.GraphExtensions
{
    /// <summary>
    /// MFG Extension to Create Purchase Orders screen
    /// </summary>
    public class POCreateAMExtension : PXGraphExtension<POCreate>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        public override void Initialize()
        {
            base.Initialize();

            Base.FixedDemand.Join<LeftJoin<AMProdMatlSplitPlan, On<AMProdMatlSplitPlan.planID, Equal<POFixedDemand.planID>>>>();
            Base.FixedDemand.WhereAnd<Where<
                Where2<Where<AMProdMatlSplitPlan.orderType, Equal<Current<POCreateFilterExt.aMOrderType>>, Or<Current<POCreateFilterExt.aMOrderType>, IsNull>>,
                And<Where<AMProdMatlSplitPlan.prodOrdID, Equal<Current<POCreateFilterExt.prodOrdID>>, Or<Current<POCreateFilterExt.prodOrdID>, IsNull>>>>>>();
        }

        /// <summary>
        /// Overrides <see cref="POCreate.GetFixedDemandFieldScope"/>
        /// </summary>
        [PXOverride]
        public virtual IEnumerable<Type> GetFixedDemandFieldScope(Func<IEnumerable<Type>> baseFunc)
        {
            foreach (Type r in baseFunc())
            {
                yield return r;
            }
            yield return typeof(AMProdMatlSplitPlan);
        }

		/// <summary>
		/// Overrides <see cref="POCreate.EnumerateAndPrepareFixedDemands(PXResultset&lt;POFixedDemand&gt;)"/>
		/// </summary>
		[PXOverride]
		public virtual IEnumerable EnumerateAndPrepareFixedDemands(PXResultset<POFixedDemand> fixedDemands,
			Func<PXResultset<POFixedDemand>, IEnumerable> baseFunc)
		{
			foreach (PXResult<POFixedDemand> rec in baseFunc(fixedDemands))
			{
				PrepareFixedDemandMFGRow(
					(POFixedDemand)rec,
					PXResult.Unwrap<AMProdMatlSplitPlan>(rec)
				);

				yield return rec;
			}
		}

		public virtual void PrepareFixedDemandMFGRow(POFixedDemand demand, AMProdMatlSplitPlan aMProdMatlSplitPlan)
		{
			if (aMProdMatlSplitPlan?.SplitLineNbr != null)
			{
				demand.NoteID = aMProdMatlSplitPlan.NoteID;
			}
		}

		public static void POCreatePOOrders(POCreate poCreateGraph, List<POFixedDemand> list, DateTime? purchaseDate)
        {
            var poredirect = poCreateGraph.CreatePOOrders(list, purchaseDate, false);
            PXLongOperationHelper.TraceProcessingMessages<POFixedDemand>();
            if (poredirect != null)
            {
                throw poredirect;
            }

            throw new PXException(ErrorMessages.SeveralItemsFailed);
        }

        /// <summary>
        /// Create a PO using manual numbering
        /// </summary>
        public static void POCreatePOOrders(POCreate poCreateGraph, List<POFixedDemand> list, DateTime? purchaseDate, string manualOrdNbr)
        {
            PXGraph.InstanceCreated.AddHandler<POOrderEntry>(graph =>
            {
                graph.RowInserting.AddHandler<POOrder>((cache, e) =>
                {
                    var row = (POOrder)e.Row;
                    row.OrderNbr = manualOrdNbr;
                });
            });

            POCreatePOOrders(poCreateGraph, list, purchaseDate);
        }

        public PXAction<POCreate.POCreateFilter> viewProdDocument;
        [PXUIField(DisplayName = "viewProdDocument", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
        protected virtual System.Collections.IEnumerable ViewProdDocument(PXAdapter adapter)
        {
            var graph = GetProductionGraph(Base.FixedDemand?.Current);
            if (graph != null)
            {
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            }

            return adapter.Get();
        }

        protected virtual PXGraph GetProductionGraph(POFixedDemand fixedDemand)
        {
            if (fixedDemand?.PlanID == null)
            {
                return null;
            }

            var prodMatlSplit = (AMProdMatlSplitPlan)PXSelect<AMProdMatlSplitPlan,
                    Where<AMProdMatlSplitPlan.planID, Equal<Required<AMProdMatlSplitPlan.planID>>>>
                .Select(Base, fixedDemand.PlanID);

            if (prodMatlSplit?.ProdOrdID == null)
            {
                return null;
            }

            var graph = PXGraph.CreateInstance<ProdMaint>();
            graph.ProdMaintRecords.Current = graph.ProdMaintRecords.Search<AMProdItem.prodOrdID>(prodMatlSplit.ProdOrdID, prodMatlSplit.OrderType);
            if (graph.ProdMaintRecords.Current == null)
            {
                return null;
            }

            return graph;
        }
    }
}
