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
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common.Attributes;

namespace PX.Objects.CA
{
	[Serializable]
	[PXCacheName(Messages.CCProcessingCenterPmntMethod)]
	public partial class CCProcessingCenterPmntMethod : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CCProcessingCenterPmntMethod>.By<processingCenterID, paymentMethodID>
		{
			public static CCProcessingCenterPmntMethod Find(PXGraph graph, string processingCenterID, string paymentMethodID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, processingCenterID, paymentMethodID, options);
		}

		public static class FK
		{
			public class ProcessingCenter : CCProcessingCenter.PK.ForeignKeyOf<CCProcessingCenterPmntMethod>.By<processingCenterID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<CCProcessingCenterPmntMethod>.By<paymentMethodID> { }
		}
		#endregion

		#region ProcessingCenterID
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDBDefault(typeof(CCProcessingCenter.processingCenterID))]
		[PXSelector(typeof(Search<CCProcessingCenter.processingCenterID, Where<CCProcessingCenter.isActive, Equal<True>>>))]
		[PXParent(typeof(Select<CCProcessingCenter,
			Where<CCProcessingCenter.processingCenterID, Equal<Current<CCProcessingCenterPmntMethod.processingCenterID>>>>))]
		[PXUIField(DisplayName = "Proc. Center ID")]
		public virtual string ProcessingCenterID
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(PaymentMethod.paymentMethodID))]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
			Where<PaymentMethod.aRIsProcessingRequired, Equal<True>>>))]
		[PXUIField(DisplayName = "Payment Method")]
		[PXParent(typeof(Select<PaymentMethod,
			Where<PaymentMethod.paymentMethodID, Equal<Current<CCProcessingCenterPmntMethod.paymentMethodID>>>>))]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive
		{
			get;
			set;
		}
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default")]
		[Common.UniqueBool(typeof(CCProcessingCenterPmntMethod.paymentMethodID))]
		public virtual bool? IsDefault
		{
			get;
			set;
		}
		#endregion
		#region FundHoldPeriod

		public abstract class fundHoldPeriod : Data.BQL.BqlDateTime.Field<fundHoldPeriod> { }

		[PXDBInt]
		[PXDefault(10, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Funds Hold Period (Days)")]
		public virtual int? FundHoldPeriod
		{
			get;
			set;
		}
		#endregion
		#region ReauthDelay

		public abstract class reauthDelay : Data.BQL.BqlDateTime.Field<reauthDelay> { }

		[PXDBInt]
		[PXDefault(typeof(Null.When<Use<Parent<CCProcessingCenter.isExternalAuthorizationOnly>>.AsBool.IsEqual<True>>.Else<CS.int0>))]
		[PXFormula(typeof(Null.When<Use<Parent<CCProcessingCenter.isExternalAuthorizationOnly>>.AsBool.IsEqual<True>>.Else<reauthDelay>))]
		[PXUIRequired(typeof(Where<Parent<CCProcessingCenter.isExternalAuthorizationOnly>, Equal<False>>))]
		[PXUIEnabled(typeof(Where<Parent<CCProcessingCenter.isExternalAuthorizationOnly>, Equal<False>>))]
		[PXUIField(DisplayName = "Reauthorization Delay (Hours)")]
		public virtual int? ReauthDelay
		{
			get;
			set;
		}
		#endregion
	}
}
