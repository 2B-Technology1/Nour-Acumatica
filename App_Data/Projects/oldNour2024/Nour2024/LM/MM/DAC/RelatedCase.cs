﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class RelatedCase : PX.Data.IBqlTable
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
        Equal<Current<RelatedCase.jobOrdrID>>>>))]
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
		#region RelatedCsID
		public abstract class relatedCsID : PX.Data.IBqlField
		{
		}
		protected int? _RelatedCsID;
		[PXDBIdentity(IsKey = true)]
		public virtual int? RelatedCsID
		{
			get
			{
				return this._RelatedCsID;
			}
			set
			{
				this._RelatedCsID = value;
			}
		}
		#endregion
        #region RelatedCaseCode
        public abstract class relatedCaseCode : PX.Data.IBqlField
		{
		}
        protected string _RelatedCaseCode;
		[PXDBString(50,IsUnicode=true)]
		[PXSelector(typeof(Search<JobOrder.jobOrdrID>)
                    ,new Type[]{typeof(JobOrder.jobOrdrID),typeof(JobOrder.classID),typeof(JobOrder.itemsID),typeof(JobOrder.Status),typeof(JobOrder.customer),typeof(JobOrder.descrption)})]
        [PXUIField(DisplayName = "Related Case ID")]
        public virtual string RelatedCaseCode
		{
			get
			{
                return this._RelatedCaseCode;
			}
			set
			{
                this._RelatedCaseCode = value;
			}
		}
		#endregion
        #region RelatedCaseName
        public abstract class relatedCaseName : PX.Data.IBqlField
		{
		}
        protected string _RelatedCaseName;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault("")]
        [PXUIField(DisplayName = "Related Case Name")]
        public virtual string RelatedCaseName
		{
			get
			{
                return this._RelatedCaseName;
			}
			set
			{
                this._RelatedCaseName = value;
			}
		}
		#endregion
        #region Customer
        public abstract class customer : PX.Data.IBqlField
		{
		}
        protected string _Customer;
		[PXDBString(50, IsUnicode = true)]
		/**
        [PXSelector(typeof(Search<Items.customer>)
                          , new Type[] { typeof(Items.customer) }
                          )]
        **/
        [PXSelector(typeof(Search<PX.Objects.AR.Customer.acctCD>), typeof(PX.Objects.AR.Customer.acctCD), typeof(PX.Objects.AR.Customer.acctName))]
        [PXUIField(DisplayName = "Customer")]
        public virtual string Customer
		{
			get
			{
                return this._Customer;
			}
			set
			{
                this._Customer = value;
			}
		}
		#endregion
		#region RDescrption
		public abstract class rDescrption : PX.Data.IBqlField
		{
		}
		protected string _RDescrption;
		[PXDBString(2147483647, IsUnicode = true)]
		[PXUIField(DisplayName="Descrption")]
		public virtual string RDescrption
		{
			get
			{
				return this._RDescrption;
			}
			set
			{
				this._RDescrption = value;
			}
		}
		#endregion
	}
}
