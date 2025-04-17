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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.CA;

namespace PX.Objects.CC
{
	/// <summary>
	/// Represents a mapping row for Processing Center, Branch, Payment Method and Cash Account
	/// </summary>
	[PXCacheName("Payment Creation Settings")]
	public partial class CCProcessingCenterBranch : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CCProcessingCenterBranch>.By<processingCenterID, branchID>
		{
			public static CCProcessingCenterBranch Find(PXGraph graph, string procCenterId, int? branchId) => FindBy(graph, procCenterId, branchId);
		}
		#endregion

		#region ProcessingCenterID
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }
		/// <exclude/>
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDBDefault(typeof(CCProcessingCenter.processingCenterID))]
		[PXParent(typeof(Select<CCProcessingCenter,
			Where<CCProcessingCenter.processingCenterID, Equal<Current<CCProcessingCenterBranch.processingCenterID>>>>))]
		[PXUIField(DisplayName = "Proc. Center ID")]
		public virtual string ProcessingCenterID { get; set; }
		#endregion


		#region BranchID
		
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		/// <exclude/>
		[Branch(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? BranchID { get; set; }
		#endregion

		#region DefaultforBranch
		public abstract class defaultForBranch : PX.Data.BQL.BqlBool.Field<defaultForBranch> { }
		/// <summary>
		/// Indicates that Processing Center will be set as defult for documents with the same Branch Id
		/// and Processing Center currency.  
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use by Default")]
		public virtual bool? DefaultForBranch { get; set; }
		#endregion

		#region CCPaymentMethodID
		public abstract class cCPaymentMethodID : PX.Data.BQL.BqlString.Field<cCPaymentMethodID> { }
		/// <summary>
		/// Indicates that Payment Method ID will be set to Payment created by Payment Link with the Credit Card means of payments. 
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<PaymentMethod.paymentMethodID,
			InnerJoin<CCProcessingCenterPmntMethod, On<CCProcessingCenterPmntMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,
			InnerJoin<CCProcessingCenter, On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>>>>,
			Where<PaymentMethod.isActive, Equal<True>, And<PaymentMethod.useForAR, Equal<True>,
				And<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>, And<CCProcessingCenterPmntMethod.processingCenterID,
					Equal<Current<CCProcessingCenter.processingCenterID>>>>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXUIField(DisplayName = "Credit Card Payment Method")]
		[PXDBString(10, IsUnicode = true)]
		public virtual string CCPaymentMethodID { get; set; }
		#endregion

		#region CCCashAccountID
		public abstract class cCCashAccountID : PX.Data.BQL.BqlInt.Field<cCCashAccountID> { }
		/// <summary>
		/// Indicates that Cash Account ID will be set to Payment created by Payment Link with the Credit Card means of payments. 
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(branchID), typeof(Search2<CashAccount.cashAccountID,
				InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
					And<PaymentMethodAccount.paymentMethodID, Equal<Current<CCProcessingCenterBranch.cCPaymentMethodID>>,
					And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
					Where<Match<Current<AccessInfo.userName>>>>), ValidateValue = true, DisplayName = "Credit Card Cash Account")]
		public virtual int? CCCashAccountID { get; set; }
		#endregion

		#region EFTPaymentMethodID
		public abstract class eFTPaymentMethodID : PX.Data.BQL.BqlString.Field<eFTPaymentMethodID> { }
		/// <summary>
		/// Indicates that Payment Method ID will be set to Payment created by Payment Link with the EFT(ACH) means of payments. 
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<PaymentMethod.paymentMethodID,
			InnerJoin<CCProcessingCenterPmntMethod, On<CCProcessingCenterPmntMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,
			InnerJoin<CCProcessingCenter, On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>>>>,
			Where<PaymentMethod.isActive, Equal<True>, And<PaymentMethod.useForAR, Equal<True>,
				And<PaymentMethod.paymentType, Equal<PaymentMethodType.eft>, And<CCProcessingCenterPmntMethod.processingCenterID,
					Equal<Current<CCProcessingCenter.processingCenterID>>>>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXUIField(DisplayName = "EFT Payment Method")]
		[PXDBString(10, IsUnicode = true)]
		public virtual string EFTPaymentMethodID { get; set; }
		#endregion

		#region EFTCashAccountID
		public abstract class eFTCashAccountID : PX.Data.BQL.BqlInt.Field<eFTCashAccountID> { }
		/// <summary>
		/// Indicates that Cash Account ID will be set to Payment created by Payment Link with the EFT(ACH) means of payments. 
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(branchID), typeof(Search2<CashAccount.cashAccountID,
				InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
					And<PaymentMethodAccount.paymentMethodID, Equal<Current<CCProcessingCenterBranch.eFTPaymentMethodID>>,
					And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
					Where<Match<Current<AccessInfo.userName>>>>), DisplayName = "EFT Cash Account")]
		public virtual int? EFTCashAccountID { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		/// <exclude/>
		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		#endregion
	}
}

