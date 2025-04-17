﻿namespace Maintenance.MM
{
	using System;
	using PX.Data;
    using MyMaintaince;
    using PX.Objects.CR;
    using PX.Objects.AR;
	[System.SerializableAttribute()]
	public class ItemCustomers2 : PX.Data.IBqlTable
	{
        #region ItemsID
        public abstract class itemsID : PX.Data.IBqlField
        {
        }
        protected int? _ItemsID;
        [PXDBInt()]
        [PXDBDefault(typeof(Items2.itemsID))]
        [PXParent(typeof(Select<Items2, Where<Items2.itemsID, Equal<Current<ItemCustomers2.itemsID>>>>))]

        public virtual int? ItemsID
        {
            get
            {
                return this._ItemsID;
            }
            set
            {
                this._ItemsID = value;
            }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.IBqlField
        {
        }
        protected int? _LineNbr;
        [PXDBIdentity(IsKey = true)]
        public virtual int? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.IBqlField
        {
        }
        protected int? _CustomerID;
        [PXDBInt()]
        [PXDBDefault()]
        [PXSelector(typeof(Customer.bAccountID)
                    , new Type[]{
                    typeof(Customer.acctCD),
                    typeof(Customer.acctName)
                    }
                    , SubstituteKey = typeof(Customer.acctCD)
                    , DescriptionField = typeof(Customer.acctName))]
        [PXUIField(DisplayName = "Customer ID")]
        public virtual int? CustomerID
        {
            get
            {
                return this._CustomerID;
            }
            set
            {
                this._CustomerID = value;
            }
        }
        #endregion
        #region CustomerName
        public abstract class customerName : PX.Data.IBqlField
        {
        }
        protected string _CustomerName;
        [PXString()]
        [PXUIField(DisplayName = "Customer Name", Enabled = false)]
        public virtual string CustomerName
        {
            get
            {
                return this._CustomerName;
            }
            set
            {
                this._CustomerName = value;
            }
        }
        #endregion
	}
}
