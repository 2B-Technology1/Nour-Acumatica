﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class OperationJOB : PX.Data.IBqlTable
	{
		
		#region JobOrderID
		public abstract class jobOrderID : PX.Data.IBqlField
		{
		}
		protected string _JobOrderID;
        [PXDBString(50, IsUnicode = true)]
		[PXDBDefault(typeof(JobOrder.jobOrdrID))]
		[PXParent(typeof(
        Select<JobOrder,
        Where<JobOrder.jobOrdrID,
        Equal<Current<OperationJOB.jobOrderID>>>>))]
		public virtual string JobOrderID
		{
			get
			{
				return this._JobOrderID;
			}
			set
			{
				this._JobOrderID = value;
			}
		}
		#endregion
        #region OperationJobID
        public abstract class operationJobID : PX.Data.IBqlField
        {
        }
        protected int? _OperationJobID;
        [PXDBIdentity(IsKey = true)]
        public virtual int? OperationJobID
        {
            get
            {
                return this._OperationJobID;
            }
            set
            {
                this._OperationJobID = value;
            }
        }
        #endregion
        #region OperationID
        public abstract class operationID : PX.Data.IBqlField
        {
        }
        protected int? _OperationID;
        [PXDBInt()]
        [PXUIField(DisplayName = "Operation ID")]
        //[PXSelector(typeof(Search<Operation.oPerationID>)
        //                   ,new Type[] { typeof(Operation.operationCode), typeof(Operation.operationName) }
        //                   , DescriptionField = typeof(Operation.operationName)
        //                   , SubstituteKey = typeof(Operation.operationCode))]



        [PXSelector(typeof(Search2<Operation.oPerationID,InnerJoin<MoelOperation,On<Operation.oPerationID,Equal<MoelOperation.operationID>>>,Where<MoelOperation.modelCode,Equal<Current<JobOrder.modelName>> >>)
                          , new Type[] { typeof(Operation.operationCode), typeof(Operation.operationName) }
                          , DescriptionField = typeof(Operation.operationName)
                          , SubstituteKey = typeof(Operation.operationCode))]


        //[PXSelector(
            
        //           typeof(Search2<InventoryItem.inventoryID, InnerJoin<Balance, On<Balance.inventoryID, Equal<InventoryItem.inventoryID>>>, Where<Balance.joborderID, Equal<Current<INTranExt.usrOrdID>>>>)
        //           , new Type[]{
        //                typeof(InventoryItem.inventoryCD),
        //                typeof(InventoryItem.descr),
        //                typeof(Balance.qty)
        //            }
        //           , SubstituteKey = typeof(InventoryItem.inventoryCD)
        //           , DescriptionField = typeof(InventoryItem.descr))]



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
        #region OperationName
        public abstract class operationName : PX.Data.IBqlField
		{
		}
        protected string _OperationName;
		[PXDBString(100, IsUnicode = true)]
		[PXDefault("")]
        [PXUIField(DisplayName = "Operation Name")]
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
        #region Price
        public abstract class price : PX.Data.IBqlField
        {
        }
        protected Decimal? _Price;
        [PXDBDecimal()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Price")]
        public virtual Decimal? Price
        {
            get
            {
                return this._Price;
            }
            set
            {
                this._Price = value;
            }
        }
        #endregion
        /** Not Used Field 
        #region Code
        public abstract class code : PX.Data.IBqlField
        {
        }
        protected string _Code;
        [PXDBString(50, IsUnicode = true)]
        //[PXDefault(typeof(Search<Operation.code,Where<Operation.oPerationID,Equal<Current<Operation.oPerationID>>>>))]
        [PXSelector(typeof(Search<MoelOperation.operationCode, Where<MoelOperation.BrandID, Equal<Current<JobOrder.itemBrand>>>>))]
        [PXUIField(DisplayName = "Operation Code")]
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
	}
}
