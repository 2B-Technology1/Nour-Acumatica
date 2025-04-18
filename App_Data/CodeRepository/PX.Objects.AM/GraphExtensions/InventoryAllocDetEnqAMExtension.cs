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
using PX.Data;
using PX.Objects.IN;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using System.Linq;

namespace PX.Objects.AM.GraphExtensions
{
    /// <summary>
    /// Manufacturing Inventory Allocation Details graph extension
    /// </summary>
    public class InventoryAllocDetEnqAMExtension : PXGraphExtension<InventoryAllocDetEnq>
    {
        /// <summary>
        /// Determines if extension is active
        /// </summary>
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.manufacturing>();
        }

        [PXFilterable]
        public PXSelectOrderBy<InventoryAllocDetEnqResult,
                OrderBy<Asc<InventoryAllocDetEnqResult.module,
                    Asc<InventoryAllocDetEnqResult.qADocType,
                        Asc<InventoryAllocDetEnqResult.refNbr>>>>> ResultRecords;

        // For cache attached
        [PXHidden]
        public PXSelect<AMProdOper> ProdOper;

        #region CacheAttahed

        //Changing the production order keys for display of reference number
        [OperationIDField(IsKey = false, Visible = false, Enabled = false)]
        protected virtual void _(Events.CacheAttached<AMProdOper.operationID> e) { }

        //Changing the production order keys for display of reference number
        [OperationCDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void _(Events.CacheAttached<AMProdOper.operationCD> e) { }

        //  Production Supply
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = true, InclQtyFieldName = nameof(InventoryAllocDetEnqFilter.InclQtyProductionSupplyPrepared), SortOrder = 62)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyProductionSupplyPrepared> e) { }

        //  Production Supply
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = true, InclQtyFieldName = nameof(InventoryAllocDetEnqFilter.InclQtyProductionSupply), SortOrder = 63)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyProductionSupply> e) { }

        //  Production for Prod. Prepared
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = true, SortOrder = 64)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyProdFixedProdOrdersPrepared> e) { }

        //  Production for Production
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = true, SortOrder = 65)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyProdFixedProdOrders> e) { }

        //  Production for SO Prepared
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = true, SortOrder = 66)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyProdFixedSalesOrdersPrepared> e) { }

        //  Production for SO
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = true, SortOrder = 67)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyProdFixedSalesOrders> e) { }

        //  Purchase for Prod Prepared
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = true, SortOrder = 77)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyPOFixedProductionPrepared> e) { }

        //  Purchase for Production
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = true, SortOrder = 78)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyPOFixedProductionOrders> e) { }

        //  Production Demand Prepared
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InventoryAllocDetEnqFilter.InclQtyProductionDemandPrepared), SortOrder = 192)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyProductionDemandPrepared> e) { }

        //  Production Demand
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InventoryAllocDetEnqFilter.InclQtyProductionDemand), SortOrder = 193)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyProductionDemand> e) { }

        //  Production Allocated
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = false, InclQtyFieldName = nameof(InventoryAllocDetEnqFilter.InclQtyProductionAllocated), SortOrder = 194)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyProductionAllocated> e) { }

        //  Production to Production
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = false, SortOrder = 195)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyProdFixedProduction> e) { }

        //  Production to Purchase
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = false, SortOrder = 196)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtyProdFixedPurchase> e) { }

        //  SO to Production
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMInventoryAllocationField(IsAddition = false, SortOrder = 238)]
		protected virtual void _(Events.CacheAttached<InventoryAllocDetEnqFilter.qtySOFixedProduction> e) { }

        #endregion

        /// <summary>
        /// Extending ResultsRecords view results
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable resultRecords()
        {
            return ConditionResultRecords(Base.ResultRecords.Select().FirstTableItems);
        }

        protected virtual IEnumerable<InventoryAllocDetEnqResult> ConditionResultRecords(IEnumerable<InventoryAllocDetEnqResult> resultRecords)
        {
            if (resultRecords == null)
            {
                yield break;
            }

            var entityHelper = new EntityHelper(Base);
            var prodMaintGraph = typeof(PX.Objects.AM.ProdMaint).Name;

            foreach (var result in resultRecords)
            {
                if (result == null)
                {
                    continue;
                }

                if (IsAMModuleAllocationType(result.AllocationType))
                {
                    result.Module = Common.ModuleAM;
                    result.QADocType = prodMaintGraph;
                    result.RefNbr = entityHelper.GetEntityRowID(result.RefNoteID);
                }

                yield return result;
            }
        }

        /// <summary>
        /// All related MFG allocation entries
        /// </summary>
        protected static bool IsAMModuleAllocationType(string allocationType)
        {
            return !string.IsNullOrWhiteSpace(allocationType) &&
                   (allocationType == typeof(INPlanType.inclQtyInTransitToProduction).Name ||
                    allocationType == typeof(INPlanType.inclQtyProductionSupplyPrepared).Name ||
                    allocationType == typeof(INPlanType.inclQtyProductionSupply).Name ||
                    allocationType == typeof(INPlanType.inclQtyProductionDemandPrepared).Name ||
                    allocationType == typeof(INPlanType.inclQtyProductionDemand).Name ||
                    allocationType == typeof(INPlanType.inclQtyProductionAllocated).Name ||
                    allocationType == typeof(INPlanType.inclQtyProdFixedPurchase).Name ||
                    allocationType == typeof(INPlanType.inclQtyProdFixedProduction).Name ||
                    allocationType == typeof(INPlanType.inclQtyProdFixedProdOrdersPrepared).Name ||
                    allocationType == typeof(INPlanType.inclQtyProdFixedProdOrders).Name ||
                    allocationType == typeof(INPlanType.inclQtyProdFixedSalesOrdersPrepared).Name ||
                    allocationType == typeof(INPlanType.inclQtyProdFixedSalesOrders).Name);
        }

        public delegate IEnumerable<InventoryAllocDetEnqResult> CalculateResultRecordsDelegate();

        [PXOverride]
        public virtual IEnumerable<InventoryAllocDetEnqResult> CalculateResultRecords(CalculateResultRecordsDelegate del)
        {
            var resultsList = del?.Invoke()?.ToList();
            if (resultsList == null)
            {
                return null;
            }

            PXStringListAttribute.AppendList<InventoryAllocDetEnqResult.qADocType>(Base.Caches<InventoryAllocDetEnqResult>(), null,
                new[] { typeof(PX.Objects.AM.ProdMaint).Name }, new[] { AM.Messages.ProductionOrder });

            return resultsList;
        }
    }
}
