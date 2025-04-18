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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
	/// <summary>
	/// Table that contains the planned and actual costs of a production order. These records can be viewed on the Production Order Details (AM209000) form (corresponding to the <see cref="ProdDetail"/> graph).
	/// Parent: <see cref="AMProdItem"/>
	/// </summary>
	[Serializable]
    [PXCacheName(Messages.ProductionTotals)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMProdTotal : IBqlTable, IProdOrder
    {
        //
        //  Developer Note: if adding new fields to this DAC, add the same fields to PX.Objects.AMStandalone.AMProdTotal
        //

        internal string DebuggerDisplay => $"OrderType = {OrderType}, ProdOrdID = {ProdOrdID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMProdTotal>.By<orderType, prodOrdID>
        {
            public static AMProdTotal Find(PXGraph graph, string orderType, string prodOrdID, PKFindOptions options = PKFindOptions.None) 
                => FindBy(graph, orderType, prodOrdID, options);
            public static AMProdTotal FindDirty(PXGraph graph, string orderType, string prodOrdID)
                => PXSelect<AMProdTotal,
                        Where<orderType, Equal<Required<orderType>>,
                            And<prodOrdID, Equal<Required<prodOrdID>>>>>
                    .SelectWindowed(graph, 0, 1, orderType, prodOrdID);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMProdTotal>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMProdTotal>.By<orderType, prodOrdID> { }
        }

        #endregion

        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdItem.orderType))]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr(IsKey = true)]
        [PXDBDefault(typeof(AMProdItem.prodOrdID))]
        [PXParent(typeof(Select<AMProdItem,
            Where<AMProdItem.prodOrdID, Equal<Current<AMProdTotal.prodOrdID>>,
                And<AMProdItem.orderType, Equal<Current<AMProdTotal.orderType>>>>>))]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion

        //Planned Values
        #region PlanLabor

        public abstract class planLabor : PX.Data.BQL.BqlDecimal.Field<planLabor> { }

        protected Decimal? _PlanLabor;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Labor", Enabled = false)]
        public virtual Decimal? PlanLabor
        {
            get
            {
                return _PlanLabor;
            }
            set
            {
                _PlanLabor = value;
            }
        }
        #endregion
        #region PlanLaborTime 
        public abstract class planLaborTime : PX.Data.BQL.BqlInt.Field<planLaborTime> { }

        [ProductionTotalTimeDB]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Labor Time", Enabled = false)]
        public virtual Int32? PlanLaborTime { get; set; }
		#endregion
		#region PlanMachine

		public abstract class planMachine : PX.Data.BQL.BqlDecimal.Field<planMachine> { }

        protected Decimal? _PlanMachine;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Machine", Enabled = false)]
        public virtual Decimal? PlanMachine
        {
            get
            {
                return _PlanMachine;
            }
            set
            {
                _PlanMachine = value;
            }
        }
        #endregion
        #region PlanMaterial

        public abstract class planMaterial : PX.Data.BQL.BqlDecimal.Field<planMaterial> { }

        protected Decimal? _PlanMaterial;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Material", Enabled = false)]
        public virtual Decimal? PlanMaterial
        {
            get
            {
                return _PlanMaterial;
            }
            set
            {
                _PlanMaterial = value;
            }
        }
        #endregion
        #region PlanTool

        public abstract class planTool : PX.Data.BQL.BqlDecimal.Field<planTool> { }

        protected Decimal? _PlanTool;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Tool", Enabled = false)]
        public virtual Decimal? PlanTool
        {
            get
            {
                return _PlanTool;
            }
            set
            {
                _PlanTool = value;
            }
        }
        #endregion
        #region PlanFixedOverhead

        public abstract class planFixedOverhead : PX.Data.BQL.BqlDecimal.Field<planFixedOverhead> { }

        protected Decimal? _PlanFixedOverhead;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fixed Overhead", Enabled = false)]
        public virtual Decimal? PlanFixedOverhead
        {
            get
            {
                return _PlanFixedOverhead;
            }
            set
            {
                _PlanFixedOverhead = value;
            }
        }
        #endregion
        #region PlanVariableOverhead

        public abstract class planVariableOverhead : PX.Data.BQL.BqlDecimal.Field<planVariableOverhead> { }

        protected Decimal? _PlanVariableOverhead;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Variable Overhead", Enabled = false)]
        public virtual Decimal? PlanVariableOverhead
        {
            get
            {
                return _PlanVariableOverhead;
            }
            set
            {
                _PlanVariableOverhead = value;
            }
        }
        #endregion
        #region PlanSubcontract

        public abstract class planSubcontract : PX.Data.BQL.BqlDecimal.Field<planSubcontract> { }

        protected Decimal? _PlanSubcontract;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Subcontract", Enabled = false)]
        public virtual Decimal? PlanSubcontract
        {
            get
            {
                return _PlanSubcontract;
            }
            set
            {
                _PlanSubcontract = value;
            }
        }
        #endregion
        #region PlanQtyToProduce

        public abstract class planQtyToProduce : PX.Data.BQL.BqlDecimal.Field<planQtyToProduce> { }

        protected Decimal? _PlanQtyToProduce;
        [PXDBQuantity]
        [PXDefault(typeof(Search<AMProdItem.baseQtytoProd,
            Where<AMProdItem.prodOrdID, Equal<Current<AMProdTotal.prodOrdID>>,
                And<AMProdItem.orderType, Equal<Current<AMProdTotal.orderType>>>>>))]
        [PXUIField(DisplayName = "Qty to Produce", Enabled = false)]
        public virtual Decimal? PlanQtyToProduce
        {
            get
            {
                return this._PlanQtyToProduce;
            }
            set
            {
                this._PlanQtyToProduce = value;
            }
        }
        #endregion
        #region PlanTotal

        public abstract class planTotal : PX.Data.BQL.BqlDecimal.Field<planTotal> { }

        protected Decimal? _PlanTotal;
        [PXBaseCury]
        [PXFormula(typeof(Add<AMProdTotal.planLabor, Add<AMProdTotal.planMachine, Add<AMProdTotal.planMaterial, Add<AMProdTotal.planTool,
            Add<AMProdTotal.planFixedOverhead, Add<AMProdTotal.planVariableOverhead, AMProdTotal.planSubcontract>>>>>>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Plan Total", Enabled = false)]
        public virtual Decimal? PlanTotal
        {
            get
            {
                return _PlanTotal;
            }
            set
            {
                _PlanTotal = value;
            }
        }
        #endregion
        #region PlanUnitCost

        public abstract class planUnitCost : PX.Data.BQL.BqlDecimal.Field<planUnitCost> { }

        protected Decimal? _PlanUnitCost;
        [PXPriceCost]
        [PXFormula(typeof(Switch<Case<Where<AMProdTotal.planQtyToProduce, IsNull, Or<AMProdTotal.planQtyToProduce, Equal<decimal0>>>, decimal0>,
            Div<AMProdTotal.planTotal, AMProdTotal.planQtyToProduce>>))]
        [PXUIField(DisplayName = "Unit Cost", Enabled = false)]
        public virtual Decimal? PlanUnitCost
        {
            get
            {
                return _PlanUnitCost;
            }
            set
            {
                _PlanUnitCost = value;
            }
        }
        #endregion
        #region Plan Cost Date

        public abstract class planCostDate : PX.Data.BQL.BqlDateTime.Field<planCostDate> { }

        protected DateTime? _PlanCostDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Plan Cost Date", Enabled = false)]
        public virtual DateTime? PlanCostDate
        {
            get
            {
                return this._PlanCostDate;
            }
            set
            {
                this._PlanCostDate = value;
            }
        }
        #endregion
        #region PlanReferenceMaterial
        public abstract class planReferenceMaterial : PX.Data.BQL.BqlDecimal.Field<planReferenceMaterial> { }
        protected Decimal? _PlanReferenceMaterial;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0")]
        [PXUIField(DisplayName = "Ref. Material", Enabled = false)]
        public virtual Decimal? PlanReferenceMaterial
        {
            get
            {
                return this._PlanReferenceMaterial;
            }
            set
            {
                this._PlanReferenceMaterial = value;
            }
        }
        #endregion

        //Actual Values
        #region ActualLabor

        public abstract class actualLabor : PX.Data.BQL.BqlDecimal.Field<actualLabor> { }

        protected Decimal? _ActualLabor;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Labor", Enabled = false)]
        public virtual Decimal? ActualLabor
        {
            get
            {
                return _ActualLabor;
            }
            set
            {
                _ActualLabor = value;
            }
        }
        #endregion
        #region ActualLaborTime 
        public abstract class actualLaborTime : PX.Data.BQL.BqlInt.Field<actualLaborTime> { }

        [ProductionTotalTimeDB]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Labor Time", Enabled = false)]
        public virtual Int32? ActualLaborTime { get; set; }
        #endregion
        #region ActualMachine

        public abstract class actualMachine : PX.Data.BQL.BqlDecimal.Field<actualMachine> { }

        protected Decimal? _ActualMachine;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Machine", Enabled = false)]
        public virtual Decimal? ActualMachine
        {
            get
            {
                return _ActualMachine;
            }
            set
            {
                _ActualMachine = value;
            }
        }
        #endregion
        #region ActualMaterial

        public abstract class actualMaterial : PX.Data.BQL.BqlDecimal.Field<actualMaterial> { }

        protected Decimal? _ActualMaterial;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Material", Enabled = false)]
        public virtual Decimal? ActualMaterial
        {
            get
            {
                return _ActualMaterial;
            }
            set
            {
                _ActualMaterial = value;
            }
        }
        #endregion
        #region ActualTool

        public abstract class actualTool : PX.Data.BQL.BqlDecimal.Field<actualTool> { }

        protected Decimal? _ActualTool;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Tool", Enabled = false)]
        public virtual Decimal? ActualTool
        {
            get
            {
                return _ActualTool;
            }
            set
            {
                _ActualTool = value;
            }
        }
        #endregion
        #region ActualFixedOverhead

        public abstract class actualFixedOverhead : PX.Data.BQL.BqlDecimal.Field<actualFixedOverhead> { }

        protected Decimal? _ActualFixedOverhead;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fixed Overhead", Enabled = false)]
        public virtual Decimal? ActualFixedOverhead
        {
            get
            {
                return _ActualFixedOverhead;
            }
            set
            {
                _ActualFixedOverhead = value;
            }
        }
        #endregion
        #region ActualVariableOverhead

        public abstract class actualVariableOverhead : PX.Data.BQL.BqlDecimal.Field<actualVariableOverhead> { }

        protected Decimal? _ActualVariableOverhead;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Variable Overhead", Enabled = false)]
        public virtual Decimal? ActualVariableOverhead
        {
            get
            {
                return _ActualVariableOverhead;
            }
            set
            {
                _ActualVariableOverhead = value;
            }
        }
        #endregion
        #region ActualSubcontract

        public abstract class actualSubcontract : PX.Data.BQL.BqlDecimal.Field<actualSubcontract> { }

        protected Decimal? _ActualSubcontract;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Subcontract", Enabled = false)]
        public virtual Decimal? ActualSubcontract
        {
            get
            {
                return _ActualSubcontract;
            }
            set
            {
                _ActualSubcontract = value;
            }
        }
        #endregion
        #region WIPAdjustment
        public abstract class wIPAdjustment : PX.Data.BQL.BqlDecimal.Field<wIPAdjustment> { }

        protected Decimal? _WIPAdjustment;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Adjustments", Enabled = false)]
        public virtual Decimal? WIPAdjustment
        {
            get
            {
                return this._WIPAdjustment;
            }
            set
            {
                this._WIPAdjustment = value;
            }
        }
        #endregion
        #region QtyComplete
        public abstract class qtyComplete : PX.Data.BQL.BqlDecimal.Field<qtyComplete> { }

        protected Decimal? _QtyComplete;
        [PXUIField(DisplayName = "Qty Complete", Enabled = false)]
        [PXUnboundDefault(typeof(AMProdItem.baseQtyComplete), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXQuantity]
        public virtual Decimal? QtyComplete
        {
            get
            {
                return this._QtyComplete;
            }
            set
            {
                this._QtyComplete = value;
            }
        }
        #endregion
        #region WIPTotal
        public abstract class wIPTotal : PX.Data.BQL.BqlDecimal.Field<wIPTotal> { }

        protected Decimal? _WIPTotal;
        [PXUIField(DisplayName = "WIP Total", Enabled = false)]
        [PXUnboundDefault(typeof(AMProdItem.wIPTotal), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXBaseCury]
        public virtual Decimal? WIPTotal
        {
            get
            {
                return this._WIPTotal;
            }
            set
            {
                this._WIPTotal = value;
            }
        }
        #endregion
        #region WIPComp
        public abstract class wIPComp : PX.Data.BQL.BqlDecimal.Field<wIPComp> { }

        protected Decimal? _WIPComp;
        [PXUIField(DisplayName = "MFG to Inventory", Enabled = false)]
        [PXUnboundDefault(typeof(AMProdItem.wIPComp), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXBaseCury]
        public virtual Decimal? WIPComp
        {
            get
            {
                return this._WIPComp;
            }
            set
            {
                this._WIPComp = value;
            }
        }
        #endregion

        //Variance Values
        #region VarianceLabor  Unbound

        public abstract class varianceLabor : PX.Data.BQL.BqlDecimal.Field<varianceLabor> { }

        protected Decimal? _VarianceLabor;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdTotal.actualLabor, AMProdTotal.planLabor>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor", Enabled = false)]
        public virtual Decimal? VarianceLabor
        {
            get
            {
                return _VarianceLabor;
            }
            set
            {
                _VarianceLabor = value;
            }
        }
        #endregion
        #region VarianceLaborTime Unbound
        public abstract class varianceLaborTime : PX.Data.BQL.BqlInt.Field<varianceLaborTime> { }

        [ProductionTotalTimeNonDB]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor Time", Enabled = false)]
        [PXDependsOnFields(typeof(AMProdTotal.actualLaborTime),typeof(AMProdTotal.planLaborTime))]
        public virtual Int32? VarianceLaborTime
        {
            get
            {
                // The display format doesn't work for negative time values. Work around to make sure value will not calculate as negative.
                return Math.Abs(ActualLaborTime.GetValueOrDefault() - PlanLaborTime.GetValueOrDefault());
            }
        }
        #endregion
        #region VarianceMachine  Unbound

        public abstract class varianceMachine : PX.Data.BQL.BqlDecimal.Field<varianceMachine> { }

        protected Decimal? _VarianceMachine;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdTotal.actualMachine, AMProdTotal.planMachine>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Machine", Enabled = false)]
        public virtual Decimal? VarianceMachine
        {
            get
            {
                return _VarianceMachine;
            }
            set
            {
                _VarianceMachine = value;
            }
        }
        #endregion
        #region VarianceMaterial   Unbound

        public abstract class varianceMaterial : PX.Data.BQL.BqlDecimal.Field<varianceMaterial> { }

        protected Decimal? _VarianceMaterial;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdTotal.actualMaterial, AMProdTotal.planMaterial>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Material", Enabled = false)]
        public virtual Decimal? VarianceMaterial
        {
            get
            {
                return _VarianceMaterial;
            }
            set
            {
                _VarianceMaterial = value;
            }
        }
        #endregion
        #region VarianceTool   Unbound

        public abstract class varianceTool : PX.Data.BQL.BqlDecimal.Field<varianceTool> { }

        protected Decimal? _VarianceTool;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdTotal.actualTool, AMProdTotal.planTool>))]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Tool", Enabled = false)]
        public virtual Decimal? VarianceTool
        {
            get
            {
                return _VarianceTool;
            }
            set
            {
                _VarianceTool = value;
            }
        }
        #endregion
        #region VarianceFixedOverhead   Unbound

        public abstract class varianceFixedOverhead : PX.Data.BQL.BqlDecimal.Field<varianceFixedOverhead> { }

        protected Decimal? _VarianceFixedOverhead;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdTotal.actualFixedOverhead, AMProdTotal.planFixedOverhead>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fixed Overhead", Enabled = false)]
        public virtual Decimal? VarianceFixedOverhead
        {
            get
            {
                return _VarianceFixedOverhead;
            }
            set
            {
                _VarianceFixedOverhead = value;
            }
        }
        #endregion
        #region VarianceVariableOverhead  Unbound

        public abstract class varianceVariableOverhead : PX.Data.BQL.BqlDecimal.Field<varianceVariableOverhead> { }

        protected Decimal? _VarianceVariableOverhead;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdTotal.actualVariableOverhead, AMProdTotal.planVariableOverhead>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Variable Overhead", Enabled = false)]
        public virtual Decimal? VarianceVariableOverhead
        {
            get
            {
                return _VarianceVariableOverhead;
            }
            set
            {
                _VarianceVariableOverhead = value;
            }
        }
        #endregion
        #region VarianceSubcontract Unbound

        public abstract class varianceSubcontract : PX.Data.BQL.BqlDecimal.Field<varianceSubcontract> { }

        protected Decimal? _VarianceSubcontract;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdTotal.actualSubcontract, AMProdTotal.planSubcontract>))]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Subcontract", Enabled = false)]
        public virtual Decimal? VarianceSubcontract
        {
            get
            {
                return _VarianceSubcontract;
            }
            set
            {
                _VarianceSubcontract = value;
            }
        }
        #endregion
        #region QtyRemaining  Unbound
        public abstract class qtyRemaining : PX.Data.BQL.BqlDecimal.Field<qtyRemaining> { }

        protected Decimal? _QtyRemaining;
        [PXUIField(DisplayName = "Qty Remaining", Enabled = false)]
        [PXUnboundDefault(typeof(AMProdItem.baseQtyRemaining), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXQuantity]
        public virtual Decimal? QtyRemaining
        {
            get
            {
                return this._QtyRemaining;
            }
            set
            {
                this._QtyRemaining = value;
            }
        }
        #endregion
        #region VarianceTotal  Unbound

        public abstract class varianceTotal : PX.Data.BQL.BqlDecimal.Field<varianceTotal> { }

        protected Decimal? _VarianceTotal;
        [PXBaseCury]
        [PXFormula(typeof(Add<AMProdTotal.varianceLabor, Add<AMProdTotal.varianceMachine, Add<AMProdTotal.varianceMaterial,
            Add<AMProdTotal.varianceTool, Add<AMProdTotal.varianceFixedOverhead, Add<AMProdTotal.varianceVariableOverhead, AMProdTotal.varianceSubcontract>>>>>>))]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Variance", Enabled = false)]
        public virtual Decimal? VarianceTotal
        {
            get
            {
                return _VarianceTotal;
            }
            set
            {
                _VarianceTotal = value;
            }
        }
        #endregion
        #region WIPBalance
        public abstract class wIPBalance : PX.Data.BQL.BqlDecimal.Field<wIPBalance> { }

        protected Decimal? _WIPBalance;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdItem.wIPTotal, AMProdItem.wIPComp>))]
        [PXUIField(DisplayName = "WIP Balance", Enabled = false)]
        [PXDependsOnFields(typeof(AMProdTotal.wIPTotal), typeof(AMProdTotal.wIPComp))]
        public virtual Decimal? WIPBalance => WIPTotal.GetValueOrDefault() - WIPComp.GetValueOrDefault();
        #endregion

        // Logging fields
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime

        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID

        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime

        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected Byte[] _tstamp;
        [PXDBTimestamp]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion

        #region ScrapAmount

        public abstract class scrapAmount : PX.Data.BQL.BqlDecimal.Field<scrapAmount> { }

        protected Decimal? _ScrapAmount;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Scrap", Enabled = false)]
        public virtual Decimal? ScrapAmount
        {
            get
            {
                return _ScrapAmount;
            }
            set
            {
                _ScrapAmount = value;
            }
        }
		#endregion

		#region  Raw Times

		#region ActualLaborTimeRaw
		public abstract class actualLaborTimeRaw : PX.Data.BQL.BqlInt.Field<actualLaborTimeRaw> { }

		[RawTimeField(typeof(actualLaborTime), Enabled = false)]
		[PXDependsOnFields(typeof(actualLaborTime))]
		public virtual Int32? ActualLaborTimeRaw
		{
			get
			{
				return this.ActualLaborTime;
			}
			set
			{
				this.ActualLaborTime = value;
			}
		}
		#endregion
		#region PlanLaborTimeRaw
		public abstract class planLaborTimeRaw : PX.Data.BQL.BqlInt.Field<planLaborTimeRaw> { }

		[RawTimeField(typeof(planLaborTime), Enabled = false)]
		[PXDependsOnFields(typeof(planLaborTime))]
		public virtual Int32? PlanLaborTimeRaw
		{
			get
			{
				return this.PlanLaborTime;
			}
			set
			{
				this.PlanLaborTime = value;
			}
		}
		#endregion
		#region VarianceLaborTimeRaw
		public abstract class varianceLaborTimeRaw : PX.Data.BQL.BqlInt.Field<varianceLaborTimeRaw> { }

		[RawTimeField(typeof(varianceLaborTime), Enabled = false)]
		[PXDependsOnFields(typeof(varianceLaborTime))]
		public virtual Int32? VarianceLaborTimeRaw => VarianceLaborTime;

		#endregion

		#endregion

	}
}
