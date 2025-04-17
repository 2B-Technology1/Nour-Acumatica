﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class Operation : PX.Data.IBqlTable
	{
		
		#region OPerationID
        public abstract class oPerationID : PX.Data.IBqlField
        {
        }
        protected int? _OPerationID;
        [PXDBIdentity()]
        [PXUIField(DisplayName = "Operation Nbr")]
        public virtual int? OPerationID
        {
            get
            {
                return this._OPerationID;
            }
            set
            {
                this._OPerationID = value;
            }
        }
        #endregion
        //changed Code and Name to OperationCode and OperationName
        #region Code to OperationCode
        public abstract class operationCode : PX.Data.IBqlField
		{
		}
        protected string _OperationCode;
		[PXDBString(50,IsKey = true, IsUnicode = true,InputMask=">aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [PXSelector(typeof(Operation.operationCode)
                    , new Type[] {  typeof(Operation.operationCode), typeof(Operation.operationName) }
                    , DescriptionField = typeof(Operation.operationName))]
        [PXUIField(DisplayName="Operation Code")]
        public virtual string OperationCode
		{
			get
			{
                return this._OperationCode;
			}
			set
			{
                this._OperationCode = value;
			}
		}
		#endregion
        #region Name to OperationName
        public abstract class operationName : PX.Data.IBqlField
		{
		}
        protected string _OperationName;
		[PXDBString(100, IsUnicode = true)]
		[PXDefault("")]
        [PXUIField(DisplayName = "Desc")]
        public virtual string OperationName
		{
			get
			{
                return this._OperationName;
			}
			set
			{
                this._OperationName = value;
			}
		}
		#endregion
        #region JobTimeCode
        public abstract class jobTimeCode : PX.Data.IBqlField
        {
        }
        protected string _JobTimeCode;
        [PXDBString(50, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [PXSelector(typeof(JobTime.Code)
                    , new Type[] { typeof(JobTime.Code), typeof(JobTime.name) }
                    , DescriptionField = typeof(JobTime.name))]
        [PXUIField(DisplayName = "Job Time Code")]
        public virtual string JobTimeCode
        {
            get
            {
                return this._JobTimeCode;
            }
            set
            {
                this._JobTimeCode = value;
            }
        }
        #endregion
        //poo:not used Fields
        #region JobOrdrID
        public abstract class jobOrdrID : PX.Data.IBqlField
        {
        }
        protected int? _JobOrdrID;
        [PXDBInt()]
        public virtual int? JobOrdrID
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
    }
}
