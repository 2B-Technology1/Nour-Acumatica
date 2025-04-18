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
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    [PXCacheName(TX.TableName.BRANCH_LOCATION)]
    [PXPrimaryGraph(typeof(BranchLocationMaint))]
    public class FSBranchLocation : PX.Data.IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSBranchLocation>.By<branchLocationID>
        {
            public static FSBranchLocation Find(PXGraph graph, int? branchLocationID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, branchLocationID, options);
        }
        public class UK : PrimaryKeyOf<FSBranchLocation>.By<branchLocationCD>
        {
            public static FSBranchLocation Find(PXGraph graph, string branchLocationCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, branchLocationCD, options);
        }
        public static class FK
        {
            public class Branch : GL.Branch.PK.ForeignKeyOf<FSBranchLocation>.By<branchID> { }
            public class Address : FSAddress.PK.ForeignKeyOf<FSBranchLocation>.By<branchLocationAddressID> { }
            public class Contract : FSContact.PK.ForeignKeyOf<FSBranchLocation>.By<branchLocationContactID> { }
            public class Subaccount : GL.Sub.PK.ForeignKeyOf<FSBranchLocation>.By<subID> { }
            public class Site : INSite.PK.ForeignKeyOf<FSBranchLocation>.By<dfltSiteID> { }
            public class SubItem : INSubItem.PK.ForeignKeyOf<FSBranchLocation>.By<dfltSubItemID> { }
            //todo public class UnitOfMeasure : INUnit.UK.ByInventory.ForeignKeyOf<FSBranchLocation>.By<inventoryID, uOM> { }
        }
        #endregion

        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBIdentity]
        [PXUIField(Enabled = false)]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region BranchLocationCD
        public abstract class branchLocationCD : PX.Data.BQL.BqlString.Field<branchLocationCD> { }

        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", IsFixed = true)]
        [PXSelector(typeof(FSBranchLocation.branchLocationCD), DescriptionField = typeof(FSBranchLocation.descr))]
        [PXUIField(DisplayName = "Branch Location ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [NormalizeWhiteSpace]
        public virtual string BranchLocationCD { get; set; }
        #endregion

        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXDBInt]
        [PXDefault(typeof(AccessInfo.branchID))]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual int? BranchID { get; set; }
        #endregion

        #region BranchLocationAddressID
        public abstract class branchLocationAddressID : PX.Data.IBqlField
        {
        }
        protected Int32? _BranchLocationAddressID;
        [PXDBInt]
        [FSDocumentAddress(typeof(Select<Address,
             Where<True, Equal<False>>>))]
        public virtual Int32? BranchLocationAddressID
        {
            get
            {
                return this._BranchLocationAddressID;
            }
            set
            {
                this._BranchLocationAddressID = value;
            }
        }
        #endregion
        #region BranchLocationContactID
        public abstract class branchLocationContactID : PX.Data.IBqlField
        {
        }
        protected Int32? _BranchLocationContactID;
        [PXDBInt]
        [FSDocumentContact(typeof(Select<Contact,
             Where<True, Equal<False>>>))]
        public virtual Int32? BranchLocationContactID
        {
            get
            {

                return this._BranchLocationContactID;
            }
            set
            {
                this._BranchLocationContactID = value;
            }
        }
        #endregion  
        #region AllowOverrideContactAddress
        public abstract class allowOverrideContactAddress : PX.Data.IBqlField
        {
        }
        protected Boolean? _AllowOverrideContactAddress;
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? AllowOverrideContactAddress
        {
            get
            {
                return this._AllowOverrideContactAddress;
            }
            set
            {
                this._AllowOverrideContactAddress = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.IBqlField
        {
        }

        [PXDBLocalizableString(60, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Descr { get; set; }
        #endregion

        #region SubID
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        [SubAccount(DisplayName = "General Subaccount")]
        public virtual int? SubID { get; set; }
        #endregion

        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote]
        public virtual Guid? NoteID { get; set; }
        #endregion        
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created On")]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modified On")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region DfltSiteID
        public abstract class dfltSiteID : PX.Data.BQL.BqlInt.Field<dfltSiteID> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [Site(DisplayName = "Default Warehouse", DescriptionField = typeof(INSite.descr))]
        public virtual int? DfltSiteID { get; set; }
        #endregion
        #region DfltSubItemID
        public abstract class dfltSubItemID : PX.Data.BQL.BqlInt.Field<dfltSubItemID> { }

        [SubItem(DisplayName = "Default Subitem")]
        public virtual int? DfltSubItemID { get; set; }
        #endregion
        #region DfltUOM
        public abstract class dfltUOM : PX.Data.BQL.BqlString.Field<dfltUOM> { }

        [INUnit(DisplayName = "Default Unit")]
        public virtual string DfltUOM { get; set; }
        #endregion
        #region RoomFeatureEnabled
        public abstract class roomFeatureEnabled : PX.Data.BQL.BqlBool.Field<roomFeatureEnabled> { }

        [PXBool]
        [PXFormula(typeof(Current<FSSetup.manageRooms>))]
        [PXUIField(Visible = false)]
        public virtual bool? RoomFeatureEnabled { get; set; }
        #endregion
    }
}
