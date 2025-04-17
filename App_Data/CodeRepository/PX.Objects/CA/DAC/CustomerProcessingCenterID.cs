/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;

namespace PX.Objects.CA
{
	[PXCacheName("Customer Processing Center ID")]
	[Serializable]
	public partial class CustomerProcessingCenterID : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CustomerProcessingCenterID>.By<instanceID>
		{
			public static CustomerProcessingCenterID Find(PXGraph graph, int? instanceID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, instanceID, options);
		}

		public static class FK
		{
			public class Customer : AR.Customer.PK.ForeignKeyOf<CustomerProcessingCenterID>.By<bAccountID> { }
			public class ProcessingCenter : CCProcessingCenter.PK.ForeignKeyOf<CustomerProcessingCenterID>.By<cCProcessingCenterID> { }
		}
		#endregion

		#region InstanceID
		public abstract class instanceID : PX.Data.BQL.BqlInt.Field<instanceID> { }

		[PXDBIdentity(IsKey = true)]
		public virtual int? InstanceID
			{
			get;
			set;
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		[Customer(DescriptionField = typeof(Customer.acctName))]
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<CustomerProcessingCenterID.bAccountID>>>>))]
		public virtual int? BAccountID
			{
			get;
			set;
		}
		#endregion
		#region CCProcessingCenterID
		public abstract class cCProcessingCenterID : PX.Data.BQL.BqlString.Field<cCProcessingCenterID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXParent(typeof(Select<CCProcessingCenter, 
			Where<CCProcessingCenter.processingCenterID, Equal<Current<CustomerProcessingCenterID.cCProcessingCenterID>>>>))]
		[PXUIField(DisplayName = "Proc. Center ID")]
		public virtual string CCProcessingCenterID
		{
			get;
			set;
		}
		#endregion
		#region CustomerCCPID
		public abstract class customerCCPID : PX.Data.BQL.BqlString.Field<customerCCPID> { }
		[PXDBString(1024, IsUnicode = true)]
		[PXDBDefault]
		[PXUIField(DisplayName = "Customer CCPID")]
		public virtual string CustomerCCPID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
	}
}
