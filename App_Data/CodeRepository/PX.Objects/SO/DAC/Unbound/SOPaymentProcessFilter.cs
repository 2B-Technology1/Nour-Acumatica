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

using PX.Data;
using PX.Objects.AR;
using System;

namespace PX.Objects.SO.DAC.Unbound
{
	[PXCacheName(Messages.SOPaymentProcessFilter)]
	public class SOPaymentProcessFilter : IBqlTable
	{
		#region Action
		public abstract class action : PX.Data.BQL.BqlString.Field<action>
		{
			public const string CaptureCCPayment = nameof(CaptureCCPayment);
			public const string ValidateCCPayment = nameof(ValidateCCPayment);
			public const string VoidExpiredCCPayment = nameof(VoidExpiredCCPayment);
			public const string ReAuthorizeCCPayment = nameof(ReAuthorizeCCPayment);

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					new[] {
						Pair(CaptureCCPayment, Messages.CaptureCCPayment),
						Pair(ValidateCCPayment, Messages.ValidateCCPayment),
						Pair(VoidExpiredCCPayment, Messages.VoidExpiredCCPayment),
						Pair(ReAuthorizeCCPayment, Messages.ReAuthorizeCCPayment),
					})
				{ }
			}

			public class captureCCPayment : PX.Data.BQL.BqlString.Constant<captureCCPayment>
			{
				public captureCCPayment() : base(CaptureCCPayment) { }
			}

			public class validateCCPayment : PX.Data.BQL.BqlString.Constant<validateCCPayment>
			{
				public validateCCPayment() : base(ValidateCCPayment) { }
			}

			public class voidCCPayment : PX.Data.BQL.BqlString.Constant<voidCCPayment>
			{
				public voidCCPayment() : base(VoidExpiredCCPayment) { }
			}

			public class reAuthorizeCCPayment : PX.Data.BQL.BqlString.Constant<reAuthorizeCCPayment>
			{
				public reAuthorizeCCPayment() : base(ReAuthorizeCCPayment) { }
			}
		}
		[PXString]
		[action.List]
		[PXUIField(DisplayName = "Action")]
		public virtual string Action
		{
			get;
			set;
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		[PXDate]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate
		{
			get;
			set;
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		[PXDate]
		[PXUIField(DisplayName = "End Date")]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? EndDate
		{
			get;
			set;
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[Customer]
		public virtual Int32? CustomerID
		{
			get;
			set;
		}
		#endregion
	}
}
