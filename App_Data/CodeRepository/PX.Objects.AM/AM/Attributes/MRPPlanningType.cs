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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// MRP Types
    /// </summary>
    public class MRPPlanningType
    {
        public const string Unknown = "UK";

        /// <summary>
        /// Sales Order = SO (10)
        /// </summary>
        public const string SalesOrder = "10";

        /// <summary>
        /// SO Shipment
        /// </summary>
        public const string Shipment = "12";

        /// <summary>
        /// Purchase Order = PU (15)
        /// </summary>
        public const string PurchaseOrder = "15";

        /// <summary>
        /// Forecast Demand = FC (20)
        /// </summary>
        public const string ForecastDemand = "20";

        /// <summary>
        /// Production Order = PI (25)
        /// </summary>
        public const string ProductionOrder = "25";

        /// <summary>
        /// Production Material = PM (30)
        /// </summary>
        public const string ProductionMaterial = "30";

        /// <summary>
        /// Stock Adjustment = IN (35)
        /// </summary>
        public const string StockAdjustment = "35";

        /// <summary>
        /// Safety Stock = SS (40)
        /// </summary>
        public const string SafetyStock = "40";
		// treated same as SafetyStock but just for difference in display
		public const string ReorderPoint = "41";

        /// <summary>
        /// MPS = MS (45)
        /// </summary>
        public const string MPS = "45";

        /// <summary>
        /// MRP Plan = MP (50)
        /// </summary>
        public const string MrpPlan = "50";

        /// <summary>
        /// MRP Requirement (Blowdown) = BD (55)
        /// </summary>
        public const string MrpRequirement = "55";

		/// <summary>
		/// MRP Generated Planned Transfer Demand Order
		/// </summary>
		public const string PlannedTransferDemand = "59";

		/// <summary>
		/// MRP SO Transfer Demand
		/// </summary>
		public const string TransferDemand = "60";

        /// <summary>
        /// MRP SO Transfer Supply
        /// </summary>
        public const string TransferSupply = "65";

        /// <summary>
        /// MRP KIT Demand
        /// </summary>
        public const string AssemblyDemand = "66";

        /// <summary>
        /// MRP KIT Supply
        /// </summary>
        public const string AssemblySupply = "67";

        /// <summary>
        /// MRP Generic Inventory Demand
        /// </summary>
        public const string InventoryDemand = "68";

        /// <summary>
        /// MRP Generic Inventory Supply
        /// </summary>
        public const string InventorySupply = "69";

        /// <summary>
        /// MRP Field Service
        /// </summary>
        public const string FieldService = "70";

		/// <summary>
		/// Consolidated plan
		/// </summary>
		public const string Consolidated = "75";

		/// <summary>
		/// Blanket Order
		/// </summary>
		public const string BlanketOrder = "80";

		public static string GetDescription(string id)
        {
            if (id == null)
            {
                return Messages.GetLocal(Messages.Unknown);
            }

            try
            {
                return new ListAttribute().ValueLabelDic[id];
            }
            catch
            {
                return Messages.GetLocal(Messages.Unknown);
            }
        }

        //BQL constants declaration
        public class salesOrder : PX.Data.BQL.BqlString.Constant<salesOrder>
        {
            public salesOrder() : base(SalesOrder) {}
        }
        public class shipment : PX.Data.BQL.BqlString.Constant<shipment>
        {
            public shipment() : base(Shipment) { }
        }
        public class purchaseOrder : PX.Data.BQL.BqlString.Constant<purchaseOrder>
        {
            public purchaseOrder() : base(PurchaseOrder) {}
        }
        public class forecastDemand : PX.Data.BQL.BqlString.Constant<forecastDemand>
        {
            public forecastDemand() : base(ForecastDemand) {}
        }
        public class productionOrder : PX.Data.BQL.BqlString.Constant<productionOrder>
        {
            public productionOrder() : base(ProductionOrder) {}
        }
        public class productionMaterial : PX.Data.BQL.BqlString.Constant<productionMaterial>
        {
            public productionMaterial() : base(ProductionMaterial) {}
        }
        public class stockAdjustment : PX.Data.BQL.BqlString.Constant<stockAdjustment>
        {
            public stockAdjustment() : base(StockAdjustment) {}
        }
        public class safetyStock : PX.Data.BQL.BqlString.Constant<safetyStock>
        {
            public safetyStock() : base(SafetyStock) {}
        }
		public class reorderPoint : PX.Data.BQL.BqlString.Constant<reorderPoint>
        {
            public reorderPoint() : base(ReorderPoint) {}
        }
        public class mps : PX.Data.BQL.BqlString.Constant<mps>
        {
            public mps() : base(MPS) {}
        }
        public class mrpPlan : PX.Data.BQL.BqlString.Constant<mrpPlan>
        {
            public mrpPlan() : base(MrpPlan) {}
        }
        public class mrpRequirement : PX.Data.BQL.BqlString.Constant<mrpRequirement>
        {
            public mrpRequirement() : base(MrpRequirement) {}
        }
		public class plannedTransferDemand : PX.Data.BQL.BqlString.Constant<plannedTransferDemand>
		{
			public plannedTransferDemand() : base(PlannedTransferDemand) { }
		}
		public class transferDemand : PX.Data.BQL.BqlString.Constant<transferDemand>
        {
            public transferDemand() : base(TransferDemand) {}
        }
        public class transferSupply : PX.Data.BQL.BqlString.Constant<transferSupply>
        {
            public transferSupply() : base(TransferSupply) {}
        }
        public class assemblyDemand : PX.Data.BQL.BqlString.Constant<assemblyDemand>
        {
            public assemblyDemand() : base(AssemblyDemand) { }
        }
        public class assemblySupply : PX.Data.BQL.BqlString.Constant<assemblySupply>
        {
            public assemblySupply() : base(AssemblySupply) { }
        }
        public class inventoryDemand : PX.Data.BQL.BqlString.Constant<inventoryDemand>
        {
            public inventoryDemand() : base(InventoryDemand) { }
        }
        public class inventorySupply : PX.Data.BQL.BqlString.Constant<inventorySupply>
        {
            public inventorySupply() : base(InventorySupply) { }
        }
        public class fieldService : PX.Data.BQL.BqlString.Constant<fieldService>
        {
            public fieldService() : base(FieldService) { }
        }
		public class consolidated : PX.Data.BQL.BqlString.Constant<consolidated>
        {
            public consolidated() : base(Consolidated) { }
        }

		public class blanketOrder : PX.Data.BQL.BqlString.Constant<blanketOrder>
		{
			public blanketOrder() : base(BlanketOrder) { }
		}

		public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new[]
                    {
                        Unknown,
                        SalesOrder,
                        Shipment,
                        PurchaseOrder,
                        ForecastDemand,
                        ProductionOrder,
                        ProductionMaterial,
                        StockAdjustment,
                        SafetyStock,
						ReorderPoint,
                        MPS,
                        MrpPlan,
                        MrpRequirement,
						PlannedTransferDemand,
                        TransferDemand,
                        TransferSupply,
                        AssemblyDemand,
                        AssemblySupply,
                        InventoryDemand,
                        InventorySupply,
                        FieldService,
						Consolidated,
						BlanketOrder
					},
                    new[]
                    {
                        Messages.Unknown,
                        Messages.SalesOrder,
                        Messages.Shipment,
                        Messages.PurchaseOrder,
                        Messages.Forecast,
                        Messages.ProductionOrder,
                        Messages.ProductionMaterial,
                        Messages.StockAdjustment,
                        Messages.StockingMethodSafetyStock,
						Messages.StockingMethodReorderPoint,
                        Messages.MPS,
                        Messages.MRPPlan,
                        Messages.MrpRequirement,
						Messages.PlannedTransferDemand,
                        Messages.TransferDemand,
                        Messages.TransferSupply,
                        Messages.AssemblyDemand,
                        Messages.AssemblySupply,
                        Messages.InventoryDemand,
                        Messages.InventorySupply,
                        Messages.FieldService,
						Messages.Consolidated,
						Messages.BlanketOrder
					})
            {
            }
        }
    }
}
