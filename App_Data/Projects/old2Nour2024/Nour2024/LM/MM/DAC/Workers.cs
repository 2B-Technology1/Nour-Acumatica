﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class Workers : PX.Data.IBqlTable
	{
        #region JobOrdrID
        public abstract class jobOrdrID : PX.Data.IBqlField
        {
        }
        protected string _JobOrdrID;
        [PXDBString(50, IsUnicode = true)]
        [PXDBDefault(typeof(JobOrder.jobOrdrID))]
        [PXParent(typeof(
        Select<JobOrder,
        Where<JobOrder.jobOrdrID,
        Equal<Current<Workers.jobOrdrID>>>>))]
        public virtual string JobOrdrID
        {
            get
            {
                return this._JobOrdrID;
            }
            set
            {
                this._JobOrdrID = value;
            }
        }
        #endregion
		#region WorkerID   Just LineID
		public abstract class workerID : PX.Data.IBqlField
		{
		}
		protected int? _WorkerID;
		[PXDBIdentity(IsKey = true)]
		public virtual int? WorkerID
		{
			get
			{
				return this._WorkerID;
			}
			set
			{
				this._WorkerID = value;
			}
		}
		#endregion
        #region WorkerCode
        public abstract class workerCode : PX.Data.IBqlField
		{
		}
        protected string _WorkerCode;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault("")]
        [PXSelector(typeof(Search<PX.Objects.EP.EPEmployee.acctCD>)
                 ,new Type[] { 
                     typeof(PX.Objects.EP.EPEmployee.acctCD)
                    ,typeof(PX.Objects.EP.EPEmployee.acctName)
                 }
                 , DescriptionField = typeof(PX.Objects.EP.EPEmployee.acctName)
                 , SubstituteKey    = typeof(PX.Objects.EP.EPEmployee.acctCD))]
        [PXUIField(DisplayName = "Worker ID")]
        public virtual string WorkerCode
		{
			get
			{
                return this._WorkerCode;
			}
			set
			{
                this._WorkerCode = value;
			}
		}
		#endregion
        #region WorkerName
        public abstract class workerName : PX.Data.IBqlField
		{
		}
        protected string _WorkerName;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault("")]
        [PXUIField(DisplayName = "Worker Name")]
        public virtual string WorkerName
		{
			get
			{
                return this._WorkerName;
			}
			set
			{
                this._WorkerName = value;
			}
		}
		#endregion
	}
}
