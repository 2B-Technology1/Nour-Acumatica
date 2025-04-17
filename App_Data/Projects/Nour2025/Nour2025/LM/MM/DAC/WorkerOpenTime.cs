﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
    using PX.Objects.EP;
	
	[System.SerializableAttribute()]
	public class WorkerOpenTime : PX.Data.IBqlTable
	{
         public const string Open = "Open";
        #region openTimeID
        public abstract class OpenTimeID : PX.Data.IBqlField
        {
        }
        protected int? _openTimeID;
        [PXDBInt()]
        [PXDBDefault(typeof(OPenTime.openTimeID))]
        [PXUIField(DisplayName = "Open Time Id",IsReadOnly=true)]
        [PXParent(typeof(Select<OPenTime, Where<OPenTime.openTimeID, Equal<Current<WorkerOpenTime.OpenTimeID>>>>))]
        public virtual int? openTimeID
        {
            get
            {
                return this._openTimeID;
            }
            set
            {
                this._openTimeID = value;
            }
        }
        #endregion
		#region Code
		public abstract class code : PX.Data.IBqlField
		{
		}
		protected string _Code;
		[PXDBString(50, IsUnicode = true,IsKey=true)]
		[PXDefault()]
        [PXUIField(DisplayName = "Code")]
        [PXSelector(typeof(Search<EPEmployee.acctCD, Where<EPEmployee.timeCardRequired, Equal<False>, Or<EPEmployee.timeCardRequired,IsNull>>>)

         , new Type[] 
                 { 
                     typeof(EPEmployee.acctCD)
                    ,typeof(EPEmployee.acctName)
                 }
             , DescriptionField = typeof(EPEmployee.acctName)
             , SubstituteKey = typeof(EPEmployee.acctCD))]

		public virtual string Code
		{
			get
			{
				return this._Code;
			}
			set
			{
				this._Code = value; 
			}
		}
		#endregion
		#region Name
		public abstract class name : PX.Data.IBqlField
		{
		}
		protected string _Name;
		[PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Name")]
		public virtual string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = value;
			}
		}
		#endregion
        #region LineId
        public abstract class lineId : PX.Data.IBqlField
        {
        }
        protected int? _LineId;
        [PXDBIdentity(IsKey = true)]
        [PXUIField(DisplayName = "Line Id", IsReadOnly = true)]
        public virtual int? LineId
        {
            get
            {
                return this._LineId;
            }
            set
            {
                this._LineId = value;
            }
        }
        #endregion
	}
}
