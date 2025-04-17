﻿namespace MyMaintaince.SN
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using PX.SM;
	using PX.Data;
    using PX.Objects.GL;
	
	[System.SerializableAttribute()]
	public class AccountSubSerials : PX.Data.IBqlTable
	{
		#region ID
		public abstract class iD : PX.Data.IBqlField
		{
		}
		protected int? _ID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
		public virtual int? ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				this._ID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.IBqlField
		{
		}
		protected int? _AccountID;
		[PXDBInt()]
		[PXUIField(DisplayName = "AccountID")]
        [PXSelector(typeof(Account.accountID)
                     ,new Type[]{
                      typeof(Account.accountCD),
                      typeof(Account.description) 
                    }
                    , SubstituteKey = typeof(Account.accountCD)
                    , DescriptionField = typeof(Account.description))]
		public virtual int? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.IBqlField
		{
		}
		protected int? _SubID;
		[PXDBInt()]
		[PXUIField(DisplayName = "SubID")]
        [PXSelector(typeof(Sub.subID)
             , new Type[]{
                      typeof(Sub.subCD),
                      typeof(Sub.description) 
                    }
            , SubstituteKey = typeof(Sub.subCD)
            , DescriptionField = typeof(Sub.description))]
		public virtual int? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
        #region TranType
        public abstract class tranType : PX.Data.IBqlField
        {
        }
        protected string _TranType;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "TranType")]
        [PXStringList(new String[] {"D", "C"}, new String[] {"Debit","Credit"})]
        public virtual string TranType
        {
            get
            {
                return this._TranType;
            }
            set
            {
                this._TranType = value;
            }
        }
        #endregion
		#region LastRefNbr
		public abstract class lastRefNbr : PX.Data.IBqlField
		{
		}
		protected int? _LastRefNbr;
		[PXDBInt()]
		[PXUIField(DisplayName = "LastRefNbr")]
		public virtual int? LastRefNbr
		{
			get
			{
				return this._LastRefNbr;
			}
			set
			{
				this._LastRefNbr = value;
			}
		}
		#endregion
	}
}
