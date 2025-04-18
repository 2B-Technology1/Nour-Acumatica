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
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// MRP Bucket Detail table for maintaining MRP Bucket Details
    /// </summary>
    [Serializable]
    [PXCacheName(AM.Messages.MRPBucketDetail)]
    public class AMMRPBucketDetail : IBqlTable
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMMRPBucketDetail>.By<bucketID, bucket>
        {
            public static AMMRPBucketDetail Find(PXGraph graph, string bucketID, int? bucket, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bucketID, bucket, options);
        }

        public static class FK
        {
            public class Bucket : AMMRPBucket.PK.ForeignKeyOf<AMMRPBucketDetail>.By<bucketID> { }
        }

        #endregion

        #region BucketID (key)
        public abstract class bucketID : PX.Data.BQL.BqlString.Field<bucketID> { }

        protected String _BucketID;
        [PXDBString(30, IsUnicode = true, IsKey = true, InputMask = ">AAAAAAAAAAAAAAA")]
        [PXUIField(DisplayName = "Bucket ID", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMMRPBucket.bucketID))]
        public virtual String BucketID
        {
            get
            {
                return this._BucketID;
            }
            set
            {
                this._BucketID = value;
            }
        }
        #endregion
        #region Bucket (key)
        public abstract class bucket : PX.Data.BQL.BqlInt.Field<bucket> { }

        protected Int32? _Bucket;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Bucket")]
        [PXDefault]
        [PXParent(typeof(Select<AMMRPBucket, Where<AMMRPBucket.bucketID, Equal<Current<AMMRPBucketDetail.bucketID>>>>))]
        public virtual Int32? Bucket
        {
            get
            {
                return this._Bucket;
            }
            set
            {
                this._Bucket = value;
            }
        }
        #endregion
        #region Value
        public abstract class value : PX.Data.BQL.BqlString.Field<value> { }

        protected Int32? _Value;
        [PXDBInt(MinValue = 1)]
        [PXDefault(1)]
        [PXUIField(DisplayName = "Value")]
        public virtual Int32? Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
            }
        }
        #endregion
        #region Interval
        public abstract class interval : PX.Data.BQL.BqlInt.Field<interval> { }

        protected int? _Interval;
        [PXDBInt]
        [PXDefault(AMIntervals.Week)]
        [PXUIField(DisplayName = "Interval")]
        [AMIntervals.List]
        public virtual int? Interval
        {
            get
            {
                return this._Interval;
            }
            set
            {
                this._Interval = value;
            }
        }
        #endregion

        #region System Fields
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
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
        [PXDBCreatedByScreenID()]
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
        [PXUIField(DisplayName = "MRP Bucket Date", Enabled = false, Visible = false)]
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
        [PXDBLastModifiedByID()]
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
        [PXDBLastModifiedByScreenID()]
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
        [PXDBLastModifiedDateTime()]
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
        [PXDBTimestamp()]
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
        #endregion
    }
}
