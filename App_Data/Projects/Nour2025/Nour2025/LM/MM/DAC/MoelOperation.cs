﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class MoelOperation : PX.Data.IBqlTable
	{
        #region OperationID
        public abstract class operationID : PX.Data.IBqlField
        {
        }
        protected int? _OperationID;
        [PXDBInt()]
        [PXDBDefault(typeof(Operation.oPerationID))]
        [PXParent(typeof(
        Select<Operation,
        Where<Operation.oPerationID,
        Equal<Current<MoelOperation.operationID>>>>))]
        public virtual int? OperationID
        {
            get
            {
                return this._OperationID;
            }
            set
            {
                this._OperationID = value;
            }
        }
        #endregion
        #region MoOPID "Line ID"
		public abstract class moOPID : PX.Data.IBqlField
		{
		}
		protected int? _MoOPID;
		[PXDBIdentity(IsKey = true)]
		public virtual int? MoOPID
		{
			get
			{
				return this._MoOPID;
			}
			set
			{
				this._MoOPID = value;
			}
		}
		#endregion
        #region brandID
        public abstract class BrandID : PX.Data.IBqlField
        {
        }
        protected int? _brandID;
        [PXDBInt()]
        [PXDefault()]
        [PXUIField(DisplayName = "Brand ID")]
        [PXSelector(typeof(Brand.brandID)
                           ,new Type[] { typeof(Brand.Code), typeof(Brand.name) }
                           ,DescriptionField = typeof(Brand.name)
                           , SubstituteKey = typeof(Brand.Code))]
        public virtual int? brandID
        {
            get
            {
                return this._brandID;
            }
            set
            {
                this._brandID = value;
            }
        }
        #endregion
        //poo : changed the Code "ModelCode" in MoelOperation rom String to int "as we gonna store the ID in here not the CD"
        /**
		#region Code  ModelCode
		public abstract class code : PX.Data.IBqlField
		{
		}
		protected string _Code;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Model Code")]
		[PXSelector(typeof(Search<Model.code, Where<Model.brandID, Equal<Current<MoelOperation.BrandID>>>>))]
		[PXDefault("")]
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
		**/
        //poo : new one and we have changed Code and Name To ModelCode and ModelName
        #region Code  ModelCode
        public abstract class modelCode : PX.Data.IBqlField
        {
        }
        protected int? _ModelCode;
        [PXDBInt()]
        [PXUIField(DisplayName = "Model Code")]
        [PXSelector(typeof(Search<Model.modelID, Where<Model.brandID, Equal<Current<MoelOperation.BrandID>>>>)
                    ,new Type[]{
                       typeof(Model.code),
                       typeof(Model.name)
                    }
                    , SubstituteKey = typeof(Model.code)
                    , DescriptionField = typeof(Model.name))]
        [PXDefault()]
        public virtual int? ModelCode
        {
            get
            {
                return this._ModelCode;
            }
            set
            {
                this._ModelCode = value;
            }
        }
        #endregion
        #region Name ModelName
        public abstract class modelName : PX.Data.IBqlField
		{
		}
        protected string _ModelName;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName="Model Name",IsReadOnly=true)]
        public virtual string ModelName
		{
			get
			{
                return this._ModelName;
			}
			set
			{
                this._ModelName = value;
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
        [PXUIField(DisplayName="Operation")]
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
