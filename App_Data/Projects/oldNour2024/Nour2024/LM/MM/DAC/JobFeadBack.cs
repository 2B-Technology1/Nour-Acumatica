﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class JobFeadBack : PX.Data.IBqlTable
	{
        #region JoberderID
        public abstract class joberderID : PX.Data.IBqlField
        {
        }
        protected string _JoberderID;
        [PXDBString(50, IsUnicode = true)]
        [PXDBDefault(typeof(JobOrder.jobOrdrID))]
        [PXParent(typeof(Select<JobOrder, Where<JobOrder.jobOrdrID, Equal<Current<JobFeadBack.joberderID>>>>))]
        public virtual string JoberderID
        {
            get
            {
                return this._JoberderID;
            }
            set
            {
                this._JoberderID = value;
            }
        }
        #endregion
		#region FeadBackID
		public abstract class feadBackID : PX.Data.IBqlField
		{
		}
		protected int? _FeadBackID;
		[PXDBIdentity(IsKey = true)]
		public virtual int? FeadBackID
		{
			get
			{
				return this._FeadBackID;
			}
			set
			{
				this._FeadBackID = value;
			}
		}
		#endregion
		#region FeadBackDesc
		public abstract class feadBackDesc : PX.Data.IBqlField
		{
		}
		protected string _FeadBackDesc;
		[PXDBString(2147483647, IsUnicode = true)]
        [PXUIField(DisplayName="")]
        [PXDefault("")]
		public virtual string FeadBackDesc
		{
			get
			{
				return this._FeadBackDesc;
			}
			set
			{
				this._FeadBackDesc = value;
			}
		}
		#endregion
	}
}
