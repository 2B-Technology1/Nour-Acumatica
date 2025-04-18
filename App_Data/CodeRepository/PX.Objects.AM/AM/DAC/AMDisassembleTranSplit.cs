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
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
	/// <summary>
	/// Disassembly transaction line detail that represents the <see cref="AMMTranSplit"/> class, as a line detail for the <see cref="AMDisassembleTran"/> class that is entered on Disassembly (AM301500) form (corresponding to <see cref="DisassemblyEntry"/> graph).
	/// </summary>
	[Serializable]
    [PXPrimaryGraph(typeof(DisassemblyEntry))]
    [PXCacheName(Messages.AMDisassembleTranSplit)]
    [System.Diagnostics.DebuggerDisplay("BatNbr = {BatNbr}, LineNbr = {LineNbr}, SplitLineNbr = {SplitLineNbr}, TranType = {TranType}, LotSerialNbr = {LotSerialNbr}")]
    public class AMDisassembleTranSplit : AMMTranSplit
    {
        #region Keys
        public new class PK : PrimaryKeyOf<AMDisassembleTranSplit>.By<docType, batNbr, lineNbr, splitLineNbr>
        {
	        public static AMDisassembleTranSplit Find(PXGraph graph, string docType, string batNbr, int? lineNbr, int? splitLineNbr, PKFindOptions options = PKFindOptions.None) =>
		        FindBy(graph, docType, batNbr, lineNbr, splitLineNbr, options);
        }

        public new static class FK
        {
            public class DisassembleTransaction : AMDisassembleTran.PK.ForeignKeyOf<AMDisassembleTranSplit>.By<docType, batNbr, lineNbr> { }
        }
        #endregion

        #region TranType
        public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected new String _TranType;
        [PXDBString(3, IsFixed = true)]
        [PXDefault(typeof(AMDisassembleTran.tranType))]
        public override String TranType
        {
            get
            {
                return this._TranType;
            }
            set
            {
                this._TranType = value;
            }
        }
        #endregion
        #region DocType
        public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        protected new String _DocType;
        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXDefault(typeof(AMDisassembleTran.docType))]
        public override String DocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }
        #endregion
        #region BatNbr
        public new abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }

        protected new String _BatNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDBDefault(typeof(AMDisassembleTran.batNbr))]
        [PXParent(typeof(FK.DisassembleTransaction),
            typeof(Where<AMDisassembleTran.lineNbr.IsNotEqual<AMDisassembleBatch.refLineNbr.FromCurrent>>))]
        public override String BatNbr
        {
            get
            {
                return this._BatNbr;
            }
            set
            {
                this._BatNbr = value;
            }
        }
        #endregion
        #region LineNbr
        public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected new Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(AMDisassembleTran.lineNbr))]
        public override Int32? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region SplitLineNbr
        public new abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }

        protected new Int32? _SplitLineNbr;
        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(AMDisassembleTran.lotSerCntr))]
        public override Int32? SplitLineNbr
        {
            get
            {
                return this._SplitLineNbr;
            }
            set
            {
                this._SplitLineNbr = value;
            }
        }
        #endregion
        #region TranDate
        public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        protected new DateTime? _TranDate;
        [PXDBDate]
        [PXDBDefault(typeof(AMDisassembleBatch.tranDate))]
        public override DateTime? TranDate
        {
            get
            {
                return this._TranDate;
            }
            set
            {
                this._TranDate = value;
            }
        }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected new Int32? _InventoryID;
        [Inventory(Visible = false)]
        [PXDefault(typeof(AMDisassembleTran.inventoryID))]
        public override Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SubItemID
        public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected new Int32? _SubItemID;
        [SubItem(
            typeof(AMDisassembleTranSplit.inventoryID),
            typeof(LeftJoin<INSiteStatusByCostCenter,
                On<INSiteStatusByCostCenter.subItemID, Equal<INSubItem.subItemID>,
                And<INSiteStatusByCostCenter.inventoryID, Equal<Optional<AMDisassembleTranSplit.inventoryID>>,
                And<INSiteStatusByCostCenter.siteID, Equal<Optional<AMDisassembleTranSplit.siteID>>,
				And<INSiteStatusByCostCenter.costCenterID, Equal<CostCenter.freeStock>>>>>>))]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMDisassembleTranSplit.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>))]
        [PXFormula(typeof(Default<AMDisassembleTranSplit.inventoryID>))]
        public override Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region SiteID
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected new Int32? _SiteID;
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), PX.Objects.IN.Messages.InactiveWarehouse, 
            typeof(INSite.siteCD), CacheGlobal = true)]
        [Site]
        [PXDefault(typeof(AMDisassembleTran.siteID))]
        public override Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region LocationID
        public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected new Int32? _LocationID;
        [MfgLocationAvail(typeof(AMDisassembleTranSplit.inventoryID), typeof(AMDisassembleTranSplit.subItemID),
            typeof(AMDisassembleTranSplit.siteID), false, true, typeof(AMDisassembleTran.isScrap),
            typeof(AMDisassembleTran))]
        [PXDefault]
        public override Int32? LocationID
        {
            get
            {
                return this._LocationID;
            }
            set
            {
                this._LocationID = value;
            }
        }
        #endregion
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected new String _LotSerialNbr;
        [AMLotSerialNbr(typeof(AMDisassembleTranSplit.inventoryID), typeof(AMDisassembleTranSplit.subItemID),
            typeof(AMDisassembleTranSplit.locationID), typeof(AMDisassembleTran.lotSerialNbr),
            FieldClass = "LotSerial")]
        public override String LotSerialNbr
        {
            get
            {
                return this._LotSerialNbr;
            }
            set
            {
                this._LotSerialNbr = value;
            }
        }
        #endregion
        #region InvtMult
        public new abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        protected new Int16? _InvtMult;
        [PXDBShort]
        [PXDefault(typeof(AMDisassembleTran.invtMult))]
        public override Int16? InvtMult
        {
            get
            {
                return this._InvtMult;
            }
            set
            {
                this._InvtMult = value;
            }
        }
        #endregion
        #region UOM
        public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected new String _UOM;
        [INUnit(typeof(AMDisassembleTranSplit.inventoryID), DisplayName = "UOM", Enabled = false)]
        [PXDefault(typeof(AMDisassembleTran.uOM))]
        public override String UOM
        {
            get
            {
                return this._UOM;
            }
            set
            {
                this._UOM = value;
            }
        }
        #endregion
        #region Qty
        public new abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected new Decimal? _Qty;
        [PXDBQuantity(typeof(AMDisassembleTranSplit.uOM), typeof(AMDisassembleTranSplit.baseQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Quantity")]
        public override Decimal? Qty
        {
            get
            {
                return this._Qty;
            }
            set
            {
                this._Qty = value;
            }
        }
        #endregion
        #region PlanID
        public new abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }

        protected new Int64? _PlanID;
        [PXDBLong]
        public override Int64? PlanID
        {
            get
            {
                return this._PlanID;
            }
            set
            {
                this._PlanID = value;
            }
        }
        #endregion
    }
}
