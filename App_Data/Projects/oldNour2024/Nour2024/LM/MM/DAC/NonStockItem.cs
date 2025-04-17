﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.IN;
	
	[System.SerializableAttribute()]
	public class NonStockItem : PX.Data.IBqlTable
	{
        #region OperationId
        public abstract class operationId : PX.Data.IBqlField
        {
        }
        protected int? _OperationId;
        [PXDBInt()]
        [PXDBDefault(typeof(Operation.oPerationID))]
        [PXParent(typeof(
        Select<Operation,
        Where<Operation.oPerationID,
        Equal<Current<NonStockItem.operationId>>>>))]
        public virtual int? OperationId
        {
            get
            {
                return this._OperationId;
            }
            set
            {
                this._OperationId = value;
            }
        }
        #endregion
		#region NonStockID "Line ID"
		public abstract class nonStockID : PX.Data.IBqlField
		{
		}
		protected int? _NonStockID;
		[PXDBIdentity(IsKey = true)]
		public virtual int? NonStockID
		{
			get
			{
				return this._NonStockID;
			}
			set
			{
				this._NonStockID = value;
			}
		}
		#endregion
        //poo : code and name will be changed to ServiceCode and ServiceName 
        #region Code >> ServiceCode
        public abstract class serviceCode : PX.Data.IBqlField
		{
		}
        protected int? _ServiceCode;
		[NonStockItem(DisplayName="Inventory ID")]
        [PXUIField(DisplayName = "Inventory ID")]
        public virtual int? ServiceCode
		{
			get
			{
                return this._ServiceCode;
			}
			set
			{
                this._ServiceCode = value;
			}
		}
		#endregion
        #region Name >> ServiceName
        public abstract class serviceName : PX.Data.IBqlField
		{
		}
        protected string _ServiceName;
		[PXDBString(100, IsUnicode = true)]
		[PXDefault("")]
        [PXUIField(DisplayName = "Inventory Desc")]
        public virtual string ServiceName
		{
			get
			{
                return this._ServiceName;
			}
			set
			{
                this._ServiceName = value;
			}
		}
		#endregion
		#region time
		public abstract class Time : PX.Data.IBqlField
		{
		}
		protected float? _time;
		[PXDBFloat(4)]
		[PXDefault((float)0)]
        [PXUIField(DisplayName = "Time")]
		public virtual float? time
		{
			get
			{
				return this._time;
			}
			set
			{
				this._time = value;
			}
		}
		#endregion
        //poo : not used fields
		#region OperationCode
		public abstract class operationCode : PX.Data.IBqlField
		{
		}
		protected string _OperationCode;
		[PXDBString(50, IsUnicode = true)]
        [PXDBDefault(typeof(Operation.operationCode))]
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
	}
}
