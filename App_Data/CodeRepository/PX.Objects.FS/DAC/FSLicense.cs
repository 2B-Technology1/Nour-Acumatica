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
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    [PXCacheName(TX.TableName.LICENSE)]
    [PXPrimaryGraph(typeof(LicenseMaint))]
    public class FSLicense : PX.Data.IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSLicense>.By<licenseID>
        {
            public static FSLicense Find(PXGraph graph, int? licenseID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, licenseID, options);
        }
        public class UK : PrimaryKeyOf<FSLicense>.By<refNbr>
        {
            public static FSLicense Find(PXGraph graph, string refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, refNbr, options);
        }

        public static class FK
        {
            public class Employee : EP.EPEmployee.PK.ForeignKeyOf<FSLicense>.By<employeeID> { }
            public class LicenseType : FSLicenseType.PK.ForeignKeyOf<FSLicense>.By<licenseTypeID> { }
        }
        #endregion

        #region LicenseID
        /* Cache_Attached SM_EmployeeMaint */
        public abstract class licenseID : PX.Data.BQL.BqlInt.Field<licenseID> { }

        [PXDBIdentity]
        [PXUIField(DisplayName = "License ID", Enabled = false)]
        public virtual int? LicenseID { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "License Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(FSLicense.refNbr), DescriptionField = typeof(FSLicense.descr))]
        [AutoNumber(typeof(Search<FSSetup.licenseNumberingID>),
                    typeof(AccessInfo.businessDate))]
        public virtual string RefNbr { get; set; }
        #endregion
        // Field does not allow null
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        [PXDBLocalizableString(60, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Descr { get; set; }
        #endregion
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Staff Member")]        
        [FSSelector_StaffMember_Employee_Only]
        public virtual int? EmployeeID { get; set; }
        #endregion
        #region ExpirationDate
        public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }

        [PXDBDate]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Expiration Date")]
        public virtual DateTime? ExpirationDate { get; set; }
        #endregion
        #region IssueDate
        public abstract class issueDate : PX.Data.BQL.BqlDateTime.Field<issueDate> { }

        [PXDBDate]
        [PXDefault]
        [PXUIField(DisplayName = "Issue Date")]
        public virtual DateTime? IssueDate { get; set; }
        #endregion
        #region LicenseTypeID
        public abstract class licenseTypeID : PX.Data.BQL.BqlInt.Field<licenseTypeID> { }

        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "License Type")]
        [PXSelector(typeof(FSLicenseType.licenseTypeID), SubstituteKey = typeof(FSLicenseType.licenseTypeCD), DescriptionField = typeof(FSLicense.descr))]
        public virtual int? LicenseTypeID { get; set; }
        #endregion
        #region NeverExpires
        public abstract class neverExpires : PX.Data.BQL.BqlBool.Field<neverExpires> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Never Expires")]
        public virtual bool? NeverExpires { get; set; }
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
        [PXUIField(DisplayName = "CreatedByID")]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "CreatedByScreenID")]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "CreatedDateTime")]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "LastModifiedByID")]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "LastModifiedDateTime")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        public virtual byte[] tstamp { get; set; }
        #endregion
    }
}
