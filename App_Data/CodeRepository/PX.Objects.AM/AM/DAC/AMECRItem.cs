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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// ECR Item (Master Engineering Change Request Header Record)
    /// </summary>
    [PXEMailSource]
    [Serializable]
    [PXCacheName(Messages.ECRItem)]
    [PXPrimaryGraph(typeof(ECRMaint))]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMECRItem : IBqlTable, PX.Data.EP.IAssign, IECCItem
    {
        internal string DebuggerDisplay => $"ECRID = {ECRID}, BOMID = {BOMID}, RevisionID = {RevisionID}, InventoryID = {InventoryID}, SiteID = {SiteID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMECRItem>.By<eCRID>
        {
            public static AMECRItem Find(PXGraph graph, string eCRID, PKFindOptions options = PKFindOptions.None)
                => FindBy(graph, eCRID, options);
        }

        public static class FK
        {
            public class ECO : AMECOItem.PK.ForeignKeyOf<AMECRItem>.By<eCOID> { }
            public class BOM : AMBomItem.PK.ForeignKeyOf<AMECRItem>.By<bOMID, bOMRevisionID> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMECRItem>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMECRItem>.By<siteID> { }
            public class SubItem : INSubItem.PK.ForeignKeyOf<AMECRItem>.By<subItemID> { }
            public class Workgroup : PX.TM.EPCompanyTree.PK.ForeignKeyOf<AMECRItem>.By<workgroupID> { }
			public class Owner : CR.Standalone.EPEmployee.PK.ForeignKeyOf<AMECRItem>.By<ownerID> { }
        }

        #endregion

        #region Selected

        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }

        #endregion
        #region ECRID
        public abstract class eCRID : PX.Data.BQL.BqlString.Field<eCRID> { }
        protected string _ECRID;
        [ECRID(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        [AutoNumber(typeof(AMBSetup.eCRNumberingID), typeof(AMECRItem.requestDate))]
        [PXSelector(typeof(Search<AMECRItem.eCRID>))]
        [PXDefault]
        public virtual string ECRID
        {
            get
            {
                return this._ECRID;
            }
            set
            {
                this._ECRID = value;
            }
        }

		[PXString]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		public virtual string ID
        {
            get
            {
                return this._ECRID;
            }
            set
            {
                this._ECRID = value;
            }
        }
        #endregion
        #region RevisionID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }
        protected string _RevisionID;
        [PXDefault]
        [RevisionIDField(Required = true, Enabled = false, Visible = false)]
        public virtual string RevisionID
        {
            get
            {
                return this._RevisionID;
            }
            set
            {
                this._RevisionID = value;
            }
        }
        #endregion
        #region ECOID
        public abstract class eCOID : PX.Data.BQL.BqlString.Field<eCOID> { }
        protected string _ECOID;
        [ECOID(Enabled = false)]
        [PXSelector(typeof(Search<AMECOItem.eCOID>))]
        [PXDefault(PersistingCheck =PXPersistingCheck.Nothing)]
        public virtual string ECOID
        {
            get
            {
                return this._ECOID;
            }
            set
            {
                this._ECOID = value;
            }
        }
        #endregion
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
        protected string _BOMID;
        [BomID(Required = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [PXSelector(typeof(Search2<AMBomItem.bOMID,
            InnerJoin<AMBomItemBomAggregate,
                On<AMBomItem.bOMID, Equal<AMBomItemBomAggregate.bOMID>,
                    And<AMBomItem.revisionID, Equal<AMBomItemBomAggregate.revisionID>>>>>))]
        public virtual string BOMID
        {
            get
            {
                return this._BOMID;
            }
            set
            {
                this._BOMID = value;
            }
        }
        #endregion
        #region BOMRevisionID
        public abstract class bOMRevisionID : PX.Data.BQL.BqlString.Field<bOMRevisionID> { }
        protected string _BOMRevisionID;
        [RevisionIDField(DisplayName = "BOM Revision", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<AMBomItem.revisionID,
                Where<AMBomItem.bOMID, Equal<Current<AMECRItem.bOMID>>>>)
            , typeof(AMBomItem.revisionID)
            , typeof(AMBomItem.status)
            , typeof(AMBomItem.descr)
            , typeof(AMBomItem.effStartDate)
            , typeof(AMBomItem.effEndDate)
            , DescriptionField = typeof(AMBomItem.descr))]
        [PXDefault(typeof(Search<AMBomItemActiveAggregate.revisionID,
            Where<AMBomItemActiveAggregate.bOMID, Equal<Current<AMECRItem.bOMID>>>>))]
        [PXFormula(typeof(Default<AMECRItem.bOMID>))]
        [PXForeignReference(typeof(CompositeKey<Field<AMECRItem.bOMID>.IsRelatedTo<AMBomItem.bOMID>, Field<AMECRItem.bOMRevisionID>.IsRelatedTo<AMBomItem.revisionID>>))]
        public virtual string BOMRevisionID
        {
            get
            {
                return this._BOMRevisionID;
            }
            set
            {
                this._BOMRevisionID = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        protected string _Descr;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Descr
        {
            get
            {
                return this._Descr;
            }
            set
            {
                this._Descr = value;
            }
        }
        #endregion        
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [StockItem(Visibility = PXUIVisibility.SelectorVisible, Enabled =false)]
        [PXDefault]
        [PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
        public virtual Int32? InventoryID
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
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMECRItem.inventoryID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(typeof(AMECRItem.inventoryID), Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
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
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXSearchable(PX.Objects.SM.SearchCategory.IN, "ECR {0} - {1} - {2}", new[] { typeof(AMECRItem.eCRID), typeof(AMECRItem.bOMID), typeof(AMECRItem.revisionID) },
            new Type[] { typeof(AMECRItem.descr) },
            NumberFields = new Type[] { typeof(AMECRItem.bOMID) },
            Line1Format = "{1}{2:d}", Line1Fields = new Type[] { typeof(AMECRItem.inventoryID), typeof(InventoryItem.inventoryCD), typeof(AMECRItem.effectiveDate) },
            Line2Format = "{0}", Line2Fields = new Type[] { typeof(AMECRItem.descr) }
        )]
        [PXNote(DescriptionField = typeof(bOMID), Selector = typeof(bOMID))]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
        [AMSite(Visibility = PXUIVisibility.SelectorVisible, Enabled =false)] 
        [PXDefault]
        public virtual Int32? SiteID
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
        #region LineCntrAttribute
        public abstract class lineCntrAttribute : PX.Data.BQL.BqlInt.Field<lineCntrAttribute> { }
        protected Int32? _LineCntrAttribute;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrAttribute
        {
            get
            {
                return this._LineCntrAttribute;
            }
            set
            {
                this._LineCntrAttribute = value;
            }
        }
        #endregion
        #region LineCntrOperation
        public abstract class lineCntrOperation : PX.Data.BQL.BqlInt.Field<lineCntrOperation> { }
        protected int? _LineCntrOperation;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Operation Line Cntr", Enabled = false, Visible = false)]
        public virtual int? LineCntrOperation
        {
            get
            {
                return this._LineCntrOperation;
            }
            set
            {
                this._LineCntrOperation = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByte.Field<Tstamp> { }
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
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        protected string _Status;
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [AMECRStatus.List]
        [PXDefault]
        public virtual string Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                this._Status = value;
            }
        }
        #endregion
        #region OwnerID
        public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
        protected int? _OwnerID;
        //[PXDefault(typeof(Coalesce<
        //    Search<CREmployee.userID, Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>, And<CREmployee.vStatus, NotEqual<VendorStatus.inactive>>>>,
        //    Search<BAccount.ownerID, Where<BAccount.bAccountID, Equal<Current<SOOrder.customerID>>>>>),
        //    PersistingCheck = PXPersistingCheck.Nothing)]
        [PX.TM.Owner]
        public virtual int? OwnerID
        {
            get
            {
                return this._OwnerID;
            }
            set
            {
                this._OwnerID = value;
            }
        }
        #endregion
        #region WorkgroupID
        public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
        protected int? _WorkgroupID;
        [PXDBInt]
        //[PXDefault(typeof(Customer.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PX.TM.PXCompanyTreeSelector]
        [PXUIField(DisplayName = "Workgroup", Enabled = false)]
        public virtual int? WorkgroupID
        {
            get
            {
                return this._WorkgroupID;
            }
            set
            {
                this._WorkgroupID = value;
            }
        }
        #endregion
        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
        protected Boolean? _Hold;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Hold")]
        public virtual Boolean? Hold
        {
            get
            {
                return this._Hold;
            }
            set
            {
                this._Hold = value;
            }
        }
        #endregion
        #region Approved
        public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
        protected Boolean? _Approved;
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Approved", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual Boolean? Approved
        {
            get
            {
                return this._Approved;
            }
            set
            {
                this._Approved = value;
            }
        }
        #endregion
        #region Rejected
        public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
        protected bool? _Rejected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? Rejected
        {
            get
            {
                return _Rejected;
            }
            set
            {
                _Rejected = value;
            }
        }
        #endregion
        #region Requestor
        public abstract class requestor : PX.Data.BQL.BqlInt.Field<requestor> { }
        protected Int32? _Requestor;

        [PXDBInt]
        [ProductionEmployeeSelector]
        [PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>))]
        [PXUIField(DisplayName = "Requestor")]
        public virtual Int32? Requestor
        {
            get
            {
                return this._Requestor;
            }
            set
            {
                this._Requestor = value;
            }
        }
        #endregion
        #region Priority
        public abstract class priority : PX.Data.BQL.BqlInt.Field<priority> { }
        protected int? _Priority;
        [PXDBInt(MinValue = 1, MaxValue = 10)]
        [PXUIField(DisplayName = "Priority", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(1)]
        public virtual int? Priority
        {
            get
            {
                return this._Priority;
            }
            set
            {
                this._Priority = value;
            }
        }
        #endregion
        #region EffectiveDate
        public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
        protected DateTime? _EffectiveDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Effective Date")]
        public virtual DateTime? EffectiveDate
        {
            get
            {
                return this._EffectiveDate;
            }
            set
            {
                this._EffectiveDate = value;
            }
        }
        #endregion
        #region RequestDate
        public abstract class requestDate : PX.Data.BQL.BqlDateTime.Field<requestDate> { }
        protected DateTime? _RequestDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Request Date", IsReadOnly = true)]
        public virtual DateTime? RequestDate
        {
            get
            {
                return this._RequestDate;
            }
            set
            {
                this._RequestDate = value;
            }
        }
        #endregion

        /// <summary>
        /// Constant Revision ID Value for all ECR Item records. A unique value to prevent any key violations.
        /// </summary>
        public const string ECRRev = "-ECR";

        /// <summary>
        /// BQL Constant Revision ID Value for all ECR Item records. A unique value to prevent any key violations.
        /// </summary>
        public sealed class eCRRev : PX.Data.BQL.BqlString.Constant<eCRRev>
        {
            public eCRRev() : base(ECRRev) { }
        }
    }
}
