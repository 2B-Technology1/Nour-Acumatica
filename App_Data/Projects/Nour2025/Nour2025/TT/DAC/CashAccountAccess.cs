﻿namespace Maintenance
{
	using System;
	using PX.Data;
	using PX.SM;
	using PX.Objects.CA;
	
	[System.SerializableAttribute()]
	public class CashAccountAccess : PX.Data.IBqlTable
	{
		#region UserID
		public abstract class userID : PX.Data.IBqlField
		{
		}
		protected Guid? _UserID;
		[PXDBField(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "UserID")]
		[PXSelector(typeof(Search<Users.pKID>),
                    new Type[]
                    {
                        //typeof(Users.username),
                        //typeof(Users.pKID),
                        typeof(Users.fullName),
                    },
            DescriptionField = typeof(Users.username),
            SubstituteKey = typeof(Users.username))]
		public virtual Guid? UserID
		{
			get
			{
				return this._UserID;
			}
			set
			{
				this._UserID = value;
			}
		}
		#endregion
		#region AccessID
		public abstract class accessID : PX.Data.IBqlField
		{
		}
		protected int? _AccessID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
		public virtual int? AccessID
		{
			get
			{
				return this._AccessID;
			}
			set
			{
				this._AccessID = value;
			}
		}
		#endregion
		#region UserName
		public abstract class userName : PX.Data.IBqlField
		{
		}
		protected string _UserName;
		[PXDBString(100)]
		[PXDefault((typeof(Search<Users.username,Where<Users.pKID, Equal<Current<Users.pKID>>>>)),PersistingCheck =PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "UserName")]
		public virtual string UserName
		{
			get
			{
				return this._UserName;
			}
			set
			{
				this._UserName = value;
			}
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.IBqlField
		{
		}
		protected int? _CashAccountID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Cash Account")]
		[PXSelector (typeof(Search<CashAccount.cashAccountID>),
            new Type[]
            {
                typeof(CashAccount.cashAccountCD),
                typeof(CashAccount.descr)
            },
            DescriptionField = typeof(CashAccount.descr),
            SubstituteKey = typeof(CashAccount.descr))]
		public virtual int? CashAccountID
		{
            get{return this._CashAccountID;}
            set{this._CashAccountID = value;}
		}
		#endregion
	}
}
