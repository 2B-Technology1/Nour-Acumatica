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

using V2 = PX.CCProcessingBase.Interfaces.V2;
using V1 = PX.CCProcessingBase;
using PX.Data;
using PX.Data.EP;
using System;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.Common.Attributes;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Common;
using PX.Data.BQL;
using PX.Api.Webhooks.DAC;

namespace PX.Objects.CA
{
	[PXCacheName(Messages.CCProcessingCenter)]
	[Serializable]
	[PXPrimaryGraph(typeof(CCProcessingCenterMaint))]
	public partial class CCProcessingCenter : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CCProcessingCenter>.By<processingCenterID>
		{
			public static CCProcessingCenter Find(PXGraph graph, string processingCenterID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, processingCenterID, options);
		}

		public static class FK
		{
			public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<CCProcessingCenter>.By<cashAccountID> { }
		}
		#endregion

		#region ProcessingCenterID
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXSelector(typeof(Search<CCProcessingCenter.processingCenterID>))]
		[PXUIField(DisplayName = "Proc. Center ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ProcessingCenterID
		{
			get;
			set;
		}
		#endregion
		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }

		[PXDBString(255, IsUnicode = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual string Name
		{
			get;
			set;
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive
		{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[CashAccount(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr))]
		[PXDefault]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region ProcessingTypeName
		public abstract class processingTypeName : PX.Data.BQL.BqlString.Field<processingTypeName> { }

		[PXDBString(255)]
		[PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
		[PXCCPluginTypeSelector]
		[DeprecatedProcessing(ChckVal = DeprecatedProcessingAttribute.CheckVal.ProcessingCenterType)]
		[PXUIField(DisplayName = "Payment Plug-In (Type)")]
		public virtual string ProcessingTypeName
		{
			get;
			set;
		}
		#endregion
		#region ProcessingAssemblyName
		public abstract class processingAssemblyName : PX.Data.BQL.BqlString.Field<processingAssemblyName> { }

		[PXDBString(255)]
		[PXUIField(DisplayName = "Assembly Name")]
		public virtual string ProcessingAssemblyName
		{
			get;
			set;
		}
		#endregion
		#region OpenTranTimeout
		public abstract class openTranTimeout : PX.Data.BQL.BqlInt.Field<openTranTimeout> { }

		[PXDBInt(MinValue = 0, MaxValue = 60)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Transaction Timeout (s)", Visibility = PXUIVisibility.Visible)]
		public virtual int? OpenTranTimeout
		{
			get;
			set;
		}
		#endregion
		#region AllowDirectInput
		public abstract class allowDirectInput : PX.Data.BQL.BqlBool.Field<allowDirectInput> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Direct Input", Visible = false)]
		public virtual bool? AllowDirectInput
		{
			get;
			set;
		}
		#endregion
		#region NeedsExpDateUpdate
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? NeedsExpDateUpdate
		{
			get;
			set;
		}
		#endregion
		#region SyncronizeDeletion
		public abstract class syncronizeDeletion : PX.Data.BQL.BqlBool.Field<syncronizeDeletion> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Synchronize Deletion", Visible = false)]
		public virtual bool? SyncronizeDeletion
		{
			get;
			set;
		}
		#endregion
		#region UseAcceptPaymentForm
		public abstract class useAcceptPaymentForm : PX.Data.BQL.BqlBool.Field<useAcceptPaymentForm> { }
	
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Accept Payments from New Cards")]
		public virtual bool? UseAcceptPaymentForm
		{
			get;
			set;
		}
		#endregion
		#region AllowSaveProfile
		public abstract class allowSaveProfile : PX.Data.BQL.BqlBool.Field<allowSaveProfile> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Allow Saving Payment Profiles")]
		public virtual bool? AllowSaveProfile
		{
			get;
			set;
		}
		#endregion
		#region AllowUnlinkedRefund
		public abstract class allowUnlinkedRefund : PX.Data.BQL.BqlBool.Field<allowUnlinkedRefund> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Unlinked Refunds")]
		public virtual bool? AllowUnlinkedRefund
		{
			get;
			set;
		}
		#endregion
		#region SyncRetryAttemptsNo
		public abstract class syncRetryAttemptsNo : PX.Data.BQL.BqlInt.Field<syncRetryAttemptsNo> { }
		[PXDBInt(MinValue = 0, MaxValue = 10)]
		[PXDefault(TypeCode.Int32, "3", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Number of Additional Synchronization Attempts", Visibility = PXUIVisibility.Visible)]
		public virtual int? SyncRetryAttemptsNo
		{
			get;
			set;
		}
		#endregion
		#region SyncRetryDelayMs
		public abstract class syncRetryDelayMs : PX.Data.BQL.BqlInt.Field<syncRetryDelayMs> { }

		[PXDBInt(MinValue = 0, MaxValue = 1000)]
		[PXDefault(TypeCode.Int32, "500", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Delay Between Synchronization Attempts (ms)", Visibility = PXUIVisibility.Visible)]
		public virtual int? SyncRetryDelayMs
		{
			get;
			set;
		}
		#endregion

		#region CreditCardLimit
		public abstract class creditCardLimit : PX.Data.BQL.BqlInt.Field<creditCardLimit> { }

		[PXDBInt(MinValue = 1)]
		[PXDefault(10, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Maximum Credit Cards per Profile", Visible = true)]
		public virtual int? CreditCardLimit
		{
			get;
			set;
		}
		#endregion

		#region CreateAdditionalCustomerProfile
		public abstract class createAdditionalCustomerProfiles : PX.Data.BQL.BqlBool.Field<createAdditionalCustomerProfiles> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Create Additional Customer Profiles", Visible = true)]
		public virtual Boolean? CreateAdditionalCustomerProfiles
		{
			get;
			set;
		}
		#endregion
		#region ImportSettlementBatches
		public abstract class importSettlementBatches : PX.Data.BQL.BqlBool.Field<importSettlementBatches> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Import Settlement Batches")]
		public virtual bool? ImportSettlementBatches
		{
			get;
			set;
		}
		#endregion
		#region ImportStartDate
		public abstract class importStartDate : PX.Data.BQL.BqlDateTime.Field<importStartDate> { }

		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Import Start Date")]
		public virtual DateTime? ImportStartDate
		{
			get;
			set;
		}
		#endregion
		#region LastSettlementDateUTC
		public abstract class lastSettlementDateUTC : PX.Data.BQL.BqlDateTime.Field<lastSettlementDateUTC> { }
		[PXDate(InputMask = "G", DisplayMask = "G")]
		[PXUIField(DisplayName = "Last Settlement Date UTC", Enabled = false)]
		[PXDBScalar(typeof(Search<CCBatch.settlementTimeUTC,
			Where<CCBatch.processingCenterID, Equal<CCProcessingCenter.processingCenterID>>,
			OrderBy<Desc<CCBatch.settlementTimeUTC>>>))]
		public virtual DateTime? LastSettlementDateUTC
		{
			get;
			set;
		}
		#endregion
		#region LastSettlementDate
		public abstract class lastSettlementDate : PX.Data.BQL.BqlDateTime.Field<lastSettlementDate> { }
		[PXDate(InputMask = "G", DisplayMask = "G")]
		[PXUIField(DisplayName = "Last Settlement Date", Enabled = false)]
		public virtual DateTime? LastSettlementDate
		{
			[PXDependsOnFields(typeof(lastSettlementDateUTC))]
			get
			{
				return LastSettlementDateUTC.HasValue
					? PXTimeZoneInfo.ConvertTimeFromUtc(LastSettlementDateUTC.Value, LocaleInfo.GetTimeZone())
					: (DateTime?)null;
			}
		}
		#endregion
		#region ReauthRetryDelay

		public abstract class reauthRetryDelay : Data.BQL.BqlDateTime.Field<reauthRetryDelay> { }

		[PXDBInt]
		[PXDefault(3, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Reauthorization Retry Delay (Hours)")]
		public virtual int? ReauthRetryDelay
		{
			get;
			set;
		}
		#endregion
		#region ReauthRetryNbr

		public abstract class reauthRetryNbr : Data.BQL.BqlInt.Field<reauthRetryNbr> { }

		[PXDBInt]
		[PXDefault(3, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Number of Reauthorization Retries")]
		public virtual int? ReauthRetryNbr
		{
			get;
			set;
		}
		#endregion
		#region DepositAccountID
		public abstract class depositAccountID : PX.Data.BQL.BqlInt.Field<depositAccountID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(Search<CashAccount.cashAccountID, 
			Where<CashAccount.clearingAccount, NotEqual<boolTrue>, And<CashAccount.curyID, Equal<Current<CashAccount.curyID>>>>>), DisplayName = "Deposit Account")]
		public virtual int? DepositAccountID
		{
			get;
			set;
		}
		#endregion
		#region AutoCreateBankDeposit
		public abstract class autoCreateBankDeposit : PX.Data.BQL.BqlBool.Field<autoCreateBankDeposit> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Create Bank Deposits")]
		public virtual bool? AutoCreateBankDeposit
		{
			get;
			set;
		}
		#endregion
		#region IsExternalAuthorizationOnly
		public abstract class isExternalAuthorizationOnly : PX.Data.BQL.BqlBool.Field<isExternalAuthorizationOnly> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsExternalAuthorizationOnly
		{
			get;
			set;
		}
		#endregion

		#region AllowPayLink
		public abstract class allowPayLink : PX.Data.BQL.BqlBool.Field<allowPayLink> { }
		/// <summary>
		/// Allow the Payment Link feature for Processing Center.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Payment Links")]
		public virtual bool? AllowPayLink
		{
			get;
			set;
		}
		#endregion

		#region AllowPartialPayment
		public abstract class allowPartialPayment : PX.Data.BQL.BqlBool.Field<allowPartialPayment> { }
		/// <summary>
		/// Indicates that Customers can pay for the payment link partially.  
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Partial Payment")]
		public virtual bool? AllowPartialPayment
		{
			get;
			set;
		}
		#endregion

		#region WebhookID
		public abstract class webhookID : BqlGuid.Field<webhookID> { }
		/// <summary>
		/// Acumatica specidic Webhook Id. Id points to a database record with the webhook handler.
		/// </summary>
		[PXDBGuid]
		[PXForeignReference(typeof(Field<webhookID>.IsRelatedTo<WebHook.webHookID>))]
		[PXSelector(typeof(Search<WebHook.webHookID>))]
		[PXUIField(DisplayName = "Webhook ID")]
		public virtual Guid? WebhookID
		{
			get;
			set;
		}
		#endregion

		#region NoteID

		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(CCProcessingCenter.processingCenterID))]
		public virtual Guid? NoteID
		{
			get;
			set;
		}

		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
