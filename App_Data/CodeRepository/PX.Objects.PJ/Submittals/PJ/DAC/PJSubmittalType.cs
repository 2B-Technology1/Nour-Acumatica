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
using PX.Data.BQL;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.CN.Common.Descriptor.Attributes;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.PJ.Submittals.PJ.DAC
{
	[PXCacheName("Submittal Type")]
	public class PJSubmittalType : IBqlTable
	{
        #region Keys

        /// <summary>
        /// Primary Key
        /// </summary>
        public class PK : PrimaryKeyOf<PJSubmittalType>.By<submittalTypeID>
        {
            public static PJSubmittalType Find(PXGraph graph, int? submittalTypeID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, submittalTypeID, options);
        }

        #endregion
        #region SubmittalTypeId
        public abstract class submittalTypeID : BqlInt.Field<submittalTypeID>
        {
        }

        [PXDBIdentity(IsKey = true)]
        [PXReferentialIntegrityCheck]
        public virtual int? SubmittalTypeID
        {
            get;
            set;
        }
        #endregion

        #region TypeName
        public abstract class typeName : BqlString.Field<typeName>
        {
        }
        
        [PXDBString(255, IsUnicode = true)]
        [PXDefault]
        [Unique(ErrorMessage = ProjectManagementMessages.SubmittalTypeUniqueConstraint)]
        [PXUIField(DisplayName = "Submittal Type", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string TypeName
        {
            get;
            set;
        }
        #endregion

        #region Description
        public abstract class description : BqlString.Field<description>
        {
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Description
        {
            get;
            set;
        }
        #endregion

        #region System Columns

        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get;
            set;
        }
        #endregion

        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        public virtual Guid? CreatedByID
        {
            get;
            set;
        }
        #endregion

        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get;
            set;
        }
        #endregion

        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime
        {
            get;
            set;
        }
        #endregion

        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID
        {
            get;
            set;
        }
        #endregion

        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get;
            set;
        }

        #endregion

        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime
        {
            get;
            set;
        }
        #endregion

        #endregion
    }
}
