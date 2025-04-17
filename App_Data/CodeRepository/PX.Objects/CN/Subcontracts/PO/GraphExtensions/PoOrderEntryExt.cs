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
using PX.Objects.CN.Subcontracts.PO.DAC;
using PX.Objects.CN.Subcontracts.PO.Descriptor.Attributes;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.PO.GraphExtensions
{
    public class PoOrderEntryExt : PXGraphExtension<POOrderEntry>
    {
        public PXFilter<PurchaseOrderTypeFilter> TypeFilter;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public override void Initialize()
        {
            base.Initialize();
            ApplyBaseTypeFiltering();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PurchaseOrderTypeRestrictor]
        protected virtual void _(Events.CacheAttached<POOrder.orderNbr> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PurchaseOrderTypeRestrictor]
        protected virtual void _(Events.CacheAttached<POLine.pONbr> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PurchaseOrderTypeRestrictor]
        protected virtual void _(Events.CacheAttached<SO.SOLineSplit.pONbr> e)
        {
        }

        protected virtual void _(Events.FieldDefaulting<PurchaseOrderTypeFilter.type1> args)
        {
            args.NewValue = IsSubcontractScreen()
                ? POOrderType.RegularSubcontract
                : POOrderType.Blanket;
        }

        protected virtual void _(Events.FieldDefaulting<PurchaseOrderTypeFilter.type2> args)
        {
            args.NewValue = IsSubcontractScreen()
                ? POOrderType.RegularSubcontract
                : POOrderType.DropShip;
        }

        protected virtual void _(Events.FieldDefaulting<PurchaseOrderTypeFilter.type3> args)
        {
            args.NewValue = IsSubcontractScreen()
                ? POOrderType.RegularSubcontract
                : POOrderType.RegularOrder;
        }

        protected virtual void _(Events.FieldDefaulting<PurchaseOrderTypeFilter.type4> args)
        {
            args.NewValue = IsSubcontractScreen()
                ? POOrderType.RegularSubcontract
                : POOrderType.StandardBlanket;
        }

        protected virtual void _(Events.FieldDefaulting<PurchaseOrderTypeFilter.type5> args)
        {
            args.NewValue = IsSubcontractScreen()
                ? POOrderType.RegularSubcontract
                : POOrderType.ProjectDropShip;
        }

        private void ApplyBaseTypeFiltering()
        {
            if (IsSubcontractScreen())
            {
                AddSubcontractFilters();
            }
            else
            {
                AddPurchaseOrderFilters();
            }
        }

        private void AddPurchaseOrderFilters()
        {
            Base.Document.WhereAnd<Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>>();
            Base.SetupApproval.WhereAnd<Where<POSetupApproval.orderType, NotEqual<POOrderType.regularSubcontract>>>();
        }

        private void AddSubcontractFilters()
        {
            Base.Document.WhereAnd<Where<POOrder.orderType, Equal<POOrderType.regularSubcontract>>>();
            Base.SetupApproval.WhereAnd<Where<POSetupApproval.orderType, Equal<POOrderType.regularSubcontract>>>();
        }

        private bool IsSubcontractScreen()
        {
            // In case multiple extensions are available Acumatica create dynamic BaseClass
            // (for example "Cst_SubcontractEntry" instead of "SubcontractEntry") with BaseType equal current class.
            return Base.GetType() == typeof(SubcontractEntry)
                || Base.GetType().BaseType == typeof(SubcontractEntry);
        }
    }
}
