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
using PX.Objects.AM.Attributes;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;

namespace PX.Objects.AM
{
    /// <summary>
    /// Inventory Planning detail plan records loaded from INItemPlan table rebuilt during Inventory Planning regen process
    /// </summary>
	[Serializable]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [PXCacheName(AM.Messages.MRPDetailPlan)]
    public class AMRPDetailPlan : IBqlTable
    {
        internal string DebuggerDisplay => $"[{PlanID}] InventoryID={InventoryID}, SiteID={SiteID}, FPRecordID={FPRecordID}, RefType={RefType}";

        #region Keys

        public class PK : PrimaryKeyOf<AMRPDetailPlan>.By<planID>
        {
            public static AMRPDetailPlan Find(PXGraph graph, int? planID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, planID, options);
        }

        public static class FK
        {
            public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<AMRPDetailPlan>.By<inventoryID> { }
            public class Site : IN.INSite.PK.ForeignKeyOf<AMRPDetailPlan>.By<siteID> { }
            public class ItemPlan : IN.INItemPlan.PK.ForeignKeyOf<AMRPDetailPlan>.By<planID> { }
            public class BAccount : CR.BAccount.PK.ForeignKeyOf<AMRPDetailPlan>.By<bAccountID> { }
            public class SupplyItemPlan : INItemPlanSupply.PK.ForeignKeyOf<AMRPDetailPlan>.By<supplyPlanID> { }
			public class DemandItemPlan : INItemPlanDemand.PK.ForeignKeyOf<AMRPDetailPlan>.By<demandPlanID> { }
            public class ParentInventoryItem : IN.InventoryItem.PK.ForeignKeyOf<AMRPDetailPlan>.By<parentInventoryID> { }
        }

        #endregion

        #region PlanID (key)
        public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }

        protected Int64? _PlanID;
        [PXDBLong(IsKey = true)]
        public virtual Int64? PlanID
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
        #region FPRecordID
        public abstract class fPRecordID : PX.Data.BQL.BqlInt.Field<fPRecordID> { }

        protected int? _FPRecordID;
        [PXDBInt]
        [PXUIField(DisplayName = "FP Record ID")]
        public virtual int? FPRecordID
        {
            get
            {
                return this._FPRecordID;
            }
            set
            {
                this._FPRecordID = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected int? _InventoryID;
        [Inventory]
        [PXDefault]
        public virtual int? InventoryID
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
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [PXDBInt]
        [PXUIField(DisplayName = "Subitem")]
        public virtual Int32? SubItemID
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
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected int? _SiteID;
        [Site]
        public virtual int? SiteID
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
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		protected int? _BranchID;
		[Branch(IsDetail = false, Enabled = false, Visible = false)]
		public virtual int? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region PlanDate
		public abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }

        protected DateTime? _PlanDate;
        [PXDBDate]
        [PXUIField(DisplayName = "Plan Date")]
        public virtual DateTime? PlanDate
        {
            get
            {
                return this._PlanDate;
            }
            set
            {
                this._PlanDate = value;
            }
        }
        #endregion
        #region PlanType
        public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }

        protected String _PlanType;
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Plan Type")]
        public virtual String PlanType
        {
            get
            {
                return this._PlanType;
            }
            set
            {
                this._PlanType = value;
            }
        }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected decimal? _Qty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Stock Qty")]
        public virtual decimal? Qty
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
        #region Processed
        public abstract class processed : PX.Data.BQL.BqlBool.Field<processed> { }

        protected bool? _Processed;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Processed")]
        public virtual bool? Processed
        {
            get
            {
                return this._Processed;
            }
            set
            {
                this._Processed = value;
            }
        }
        #endregion
        #region LowLevel 
        public abstract class lowLevel : PX.Data.BQL.BqlInt.Field<lowLevel> { }

        protected int? _LowLevel;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Low Level")]
        public virtual int? LowLevel
        {
            get
            {
                return this._LowLevel;
            }
            set
            {
                this._LowLevel = value;
            }
        }
        #endregion   
        #region OnHoldStatus
        public abstract class onHoldStatus : PX.Data.BQL.BqlInt.Field<onHoldStatus> { }

        protected int? _OnHoldStatus;
        [PXDBInt]
        [PXDefault(Attributes.OnHoldStatus.NotOnHold)]
        [PXUIField(DisplayName = "On hold status")]
        public virtual int? OnHoldStatus
        {
            get
            {
                return this._OnHoldStatus;
            }
            set
            {
                this._OnHoldStatus = value;
            }
        }
        #endregion
        #region RefType
        public abstract class refType : PX.Data.BQL.BqlInt.Field<refType> { }

        protected string _RefType;

		/// <summary>
		/// Reference type
		/// </summary>
		[PXDBString]
        [PXUIField(DisplayName = "Ref Type")]
        [MRPPlanningType.List]
        public virtual string RefType
        {
            get
            {
                return this._RefType;
            }
            set
            {
                this._RefType = value;
            }
        }
        #endregion
        #region SDFlag
        public abstract class sDFlag : PX.Data.BQL.BqlString.Field<sDFlag> { }

        protected string _SDFlag;
        [PXDBString(1, IsUnicode = true, IsFixed = true)]
        [PXUIField(DisplayName = "SD Flag")]
        [MRPSDFlag.List]
        public virtual string SDFlag
        {
            get
            {
                return this._SDFlag;
            }
            set
            {
                this._SDFlag = value;
            }
        }
        #endregion
        #region SupplyPlanID
        public abstract class supplyPlanID : PX.Data.BQL.BqlLong.Field<supplyPlanID> { }

        protected Int64? _SupplyPlanID;
        [PXDBLong]
        public virtual Int64? SupplyPlanID
        {
            get
            {
                return this._SupplyPlanID;
            }
            set
            {
                this._SupplyPlanID = value;
            }
        }
        #endregion
        #region DemandPlanID
        public abstract class demandPlanID : PX.Data.BQL.BqlLong.Field<demandPlanID> { }

        protected Int64? _DemandPlanID;
        [PXDBLong]
        public virtual Int64? DemandPlanID
        {
            get
            {
                return this._DemandPlanID;
            }
            set
            {
                this._DemandPlanID = value;
            }
        }
        #endregion
        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

        protected Int32? _BAccountID;
        [PXDBInt]
        public virtual Int32? BAccountID
        {
            get
            {
                return this._BAccountID;
            }
            set
            {
                this._BAccountID = value;
            }
        }
        #endregion
        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

        protected Guid? _RefNoteID;
        [PXDBGuid]
        public virtual Guid? RefNoteID
        {
            get
            {
                return this._RefNoteID;
            }
            set
            {
                this._RefNoteID = value;
            }
        }
        #endregion
        #region ParentInventoryID
        public abstract class parentInventoryID : PX.Data.BQL.BqlInt.Field<parentInventoryID> { }

        protected int? _ParentInventoryID;
        [PXDBInt]
        [PXUIField(DisplayName = "Parent Inventory ID")]
        public virtual int? ParentInventoryID
        {
            get
            {
                return this._ParentInventoryID;
            }
            set
            {
                this._ParentInventoryID = value;
            }
        }
        #endregion
        #region ParentSubItemID

        public abstract class parentSubItemID : PX.Data.BQL.BqlInt.Field<parentSubItemID> { }

        protected Int32? _ParentSubItemID;
        [PXDBInt]
        [PXUIField(DisplayName = "Parent Subitem")]
        public virtual Int32? ParentSubItemID
        {
            get
            {
                return this._ParentSubItemID;
            }
            set
            {
                this._ParentSubItemID = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected byte[] _tstamp;
        [PXDBTimestamp]
        public virtual byte[] tstamp
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
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID>
        {
        }
        protected int? _ItemClassID;
        [PXDBInt]
        [PXUIField(DisplayName = "Item Class")]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
        public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
		#endregion
		#region TransferSiteID
		public abstract class transferSiteID : PX.Data.BQL.BqlInt.Field<transferSiteID> { }

		protected Int32? _TransferSiteID;
		[Site(DisplayName = "Transfer Warehouse", ValidateValue = false)]
		public virtual Int32? TransferSiteID
		{
			get
			{
				return this._TransferSiteID;
			}
			set
			{
				this._TransferSiteID = value;
			}
		}
		#endregion
	}
}
