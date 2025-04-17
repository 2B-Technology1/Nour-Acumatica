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

using PX.Data;
using PX.Objects.CN.Subcontracts.PO.CacheExtensions;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.SC.Views
{
    public class SubcontractSetup : PXSetup<POSetup>
    {
        public SubcontractSetup(PXGraph graph)
            : base(graph)
        {
            graph.Initialized += Initialized;
        }

        private void Initialized(PXGraph graph)
        {
            var baseHandler = graph.Defaults[typeof(POSetup)];
            graph.Defaults[typeof(POSetup)] = () => GetPurchaseOrderSetup(baseHandler);
        }

        private POSetup GetPurchaseOrderSetup(PXGraph.GetDefaultDelegate baseHandler)
        {
            var setup = (POSetup) baseHandler();
            var extension = PXCache<POSetup>.GetExtension<PoSetupExt>(setup);
            return !extension.IsSubcontractSetupSaved.GetValueOrDefault()
                ? throw new PXSetupNotEnteredException<SubcontractsPreferences>(ErrorMessages.SetupNotEntered)
                : setup;
        }

        [PXPrimaryGraph(typeof(SubcontractSetupMaint))]
        [PXCacheName(Messages.SubcontractsPreferencesScreenName)]
        private class SubcontractsPreferences : POSetup
        {
        }
    }
}