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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PX.Objects.AR.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using System.Text.RegularExpressions;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Objects.AR.CCPaymentProcessing.CardsSynchronization;
using PX.Web.UI;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using CCCardType = PX.Objects.AR.CCPaymentProcessing.Common.CCCardType;
using V2 = PX.CCProcessingBase.Interfaces.V2;

namespace PX.Objects.CA
{
	public class CCSynchronizeCards : PXGraph<CCSynchronizeCards>
	{
		public PXCancel<CreditCardsFilter> Cancel;
		public PXSave<CreditCardsFilter> Save;

		private CustomerRepository customerRepo;

		[InjectDependency]
		public ICCDisplayMaskService CCDisplayMaskService { get; set; }

		public PXAction<CreditCardsFilter> LoadCards;
		[PXProcessButton(CommitChanges = true)]
		[PXUIField(DisplayName = "Load Card Data")]
		protected virtual IEnumerable loadCards(PXAdapter adapter)
		{
			CreditCardsFilter filter = Filter.Current;
			filter.EnableCustomerPaymentDialog = false;
			CCProcessingHelper.CheckHttpsConnection();

			if (!ValidateLoadCardsAction())
			{
				int rowCnt = CustomerCardPaymentData.Select().Count;

				if (rowCnt == 0 || !adapter.ExternalCall
					|| Filter.Ask(CA.Messages.RelodCardDataDialogMsg, MessageButtons.YesNo) == WebDialogResult.Yes)
				{
					foreach (CCSynchronizeCard item in CustomerCardPaymentData.Select().Select(cc => cc.GetItem<CCSynchronizeCard>()))
					{
						CustomerCardPaymentData.Delete(item);
					}
					PXLongOperation.StartOperation(this, delegate
					{
						int newCardsCnt = GetCardsAllProfiles();
						if (newCardsCnt > 0)
						{
							this.Persist();
						}
					});
				}
			}
			return adapter.Get();
		}

		public PXAction<CreditCardsFilter> SetDefaultPaymentMethod;
		[PXButton(CommitChanges = true, Category = "Actions")]
		[PXUIField(DisplayName = "Set Payment Method")]
		protected virtual IEnumerable setDefaultPaymentMethod(PXAdapter adapter)
		{
			CreditCardsFilter filter = Filter.Current;

			if (!adapter.ExternalCall || PMFilter.AskExt((graph, vName) => { filter.OverwriteEftPaymentMethod = false; filter.OverwriteCCPaymentMethod = false; }) == WebDialogResult.OK)
			{
				string eftPaymentMethod = filter.EftPaymentMethodId;
				string ccPaymentMethod = filter.CCPaymentMethodId;
				PXFilterRow[] filterRows = CustomerCardPaymentData.View.GetExternalFilters();
				int startRow = 0;
				int totalRows = 0;
				IEnumerable<CCSynchronizeCard> retList = CustomerCardPaymentData.View.Select(null, null, null, null, null, filterRows, ref startRow, 0, ref totalRows).RowCast<CCSynchronizeCard>();

				foreach (CCSynchronizeCard item in retList)
				{
					if (filter.OverwriteEftPaymentMethod.GetValueOrDefault() == true || item.PaymentMethodID == null)
					{
						if (item.PaymentType == PaymentMethodType.EFT)
							item.PaymentMethodID = eftPaymentMethod;
						CustomerCardPaymentData.Update(item);
					}

					if (filter.OverwriteCCPaymentMethod.GetValueOrDefault() == true || item.PaymentMethodID == null)
					{
						if (item.PaymentType == PaymentMethodType.CreditCard)
							item.PaymentMethodID = ccPaymentMethod;
						CustomerCardPaymentData.Update(item);
					}
				}
			}
			return adapter.Get();
		}

		public PXAction<CreditCardsFilter> ViewCustomer;
		[PXButton]
		protected virtual void viewCustomer()
		{
			CCSynchronizeCard syncCard = CustomerCardPaymentData.Current;
			CustomerMaint customer = CreateInstance<CustomerMaint>();
			PXSelectBase<Customer> customerQuery = new PXSelect<Customer,
				Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(this);
			customer.CurrentCustomer.Current = customerQuery.SelectSingle(syncCard.BAccountID);

			if (customer.CurrentCustomer.Current != null)
			{
				throw new PXRedirectRequiredException(customer, true, string.Empty);
			}

		}
		
		public PXMenuAction<CreditCardsFilter> GroupAction;

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		[PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.ActionsFolder)]
		[PXUIField(DisplayName = "Actions")]
		protected virtual IEnumerable groupAction(PXAdapter adapter, [PXString] string ActionName)
		{
			return adapter.Get();
		}

		public PXFilter<CreditCardsFilter> Filter;
		public PXFilter<CreditCardsFilter> PMFilter;

		public PXSelect<CustomerPaymentMethodDetail, Where<True, Equal<False>>> DummyCPMD;

		public PXSelectJoin<CCSynchronizeCard,
			LeftJoin<Customer, On<Customer.bAccountID, Equal<CCSynchronizeCard.bAccountID>>>,
			Where<CCSynchronizeCard.cCProcessingCenterID, Equal<Required<CCSynchronizeCard.cCProcessingCenterID>>,
				And<CCSynchronizeCard.imported, Equal<False>>>,
			OrderBy<Asc<CCSynchronizeCard.customerCCPID>>> SynchronizeCardPaymentData;

		public CCSyncFilteredProcessing<CCSynchronizeCard, CreditCardsFilter,
			Where<CCSynchronizeCard.cCProcessingCenterID, Equal<Current<CreditCardsFilter.processingCenterId>>>,
			OrderBy<Asc<CCSynchronizeCard.customerCCPID>>> CustomerCardPaymentData;
		public IEnumerable customerCardPaymentData()
		{
			CreditCardsFilter filter = Filter.Current;

			PXDelegateResult delegateResult = new PXDelegateResult();

			if (filter.ProcessingCenterId != null)
			{
				CCProcessingHelper.CheckHttpsConnection();

				PXResultset<CCSynchronizeCard> result = SynchronizeCardPaymentData.Select(filter.ProcessingCenterId);

				delegateResult.AddRange(result);
			}

			return delegateResult;
		}
	
		public PXSelect<CustomerPaymentProfile> CustPaymentProfileForDialog;
		public IEnumerable custPaymentProfileForDialog()
		{
			foreach (CustomerPaymentProfile item in CustPaymentProfileForDialog.Cache.Cached)
			{
				yield return item;
			}
		}

		List<CCSynchronizeCard> cacheRecordsSameCustomerCCPID;

		[Serializable]
		public class CustomerPaymentProfile : IBqlTable
		{
			public abstract class recordID : IBqlField
			{
			}
			[PXInt(IsKey = true)]
			public virtual int? RecordID { get; set; }

			public abstract class bAccountID : IBqlField
			{
			}
			[PXInt]
			[PXUIField(Visible = false)]
			public virtual int? BAccountID { get; set; }

			public abstract class customerCCPID : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "Proc. Center Cust. Profile ID", Enabled = false)]
			public virtual string CustomerCCPID { get; set; }

			public abstract class pCCustomerID : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "Proc. Center Cust. ID", Enabled = false)]
			public virtual string PCCustomerID { get; set; }

			public abstract class pCCustomerDescription : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "Proc. Center Cust. Descr.", Enabled = false)]
			public virtual string PCCustomerDescription { get; set; }

			public abstract class pCCustomerEmail : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "Proc. Center Cust. Email", Enabled = false)]
			public virtual string PCCustomerEmail { get; set; }

			public abstract class paymentCCPID : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "Proc. Center Payment Profile ID", Enabled = false)]
			public virtual string PaymentCCPID { get; set; }

			public abstract class paymentProfileFirstName : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "Proc. Center Payment Profile First Name", Enabled = false)]
			public virtual string PaymentProfileFirstName { get; set; }

			public abstract class paymentProfileLastName : IBqlField
			{
			}
			[PXString]
			[PXUIField(DisplayName = "Proc. Center Payment Profile Last Name", Enabled = false)]
			public virtual string PaymentProfileLastName { get; set; }

			public abstract class setPaymentProfile : IBqlField { }
			[PXBool]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected { get; set; }

			public static CustomerPaymentProfile CreateFromSyncCard(CCSynchronizeCard syncCard)
			{
				CustomerPaymentProfile ret = new CustomerPaymentProfile()
				{
					RecordID = syncCard.RecordID,
					CustomerCCPID = syncCard.CustomerCCPID,
					PCCustomerDescription = syncCard.PCCustomerDescription,
					PCCustomerEmail = syncCard.PCCustomerEmail,
					PCCustomerID = syncCard.PCCustomerID,
					BAccountID = syncCard.BAccountID,
					PaymentProfileFirstName = syncCard.FirstName,
					PaymentProfileLastName = syncCard.LastName,
					PaymentCCPID = syncCard.PaymentCCPID
				};
				return ret;
			}
		}

		[Serializable]
		public class CreditCardsFilter : IBqlTable
		{
			public abstract class processingCenterId : IBqlField { }
			[PXString]
			[CCProcessingCenterSelector(CCProcessingFeature.ExtendedProfileManagement)]
			[PXUIField(DisplayName = "Processing Center")]
			public virtual string ProcessingCenterId { get; set; }

			public abstract class scheduledServiceSync : IBqlField { }
			[PXBool]
			[PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Scheduled Sync")]
			public virtual bool? ScheduledServiceSync { get; set; }

			public abstract class loadExpiredCard : IBqlField { }
			[PXBool]
			[PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Load Expired Card Data")]
			public virtual bool? LoadExpiredCards { get; set; }

			public abstract class eftPaymentMethodId : IBqlField { }
			[PXString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXSelector(typeof(Search5<CCProcessingCenterPmntMethod.paymentMethodID,
			InnerJoin<PaymentMethod, On<CCProcessingCenterPmntMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>>,
				Where<CCProcessingCenterPmntMethod.processingCenterID, Equal<Current<CreditCardsFilter.processingCenterId>>,
					And<PaymentMethod.paymentType, Equal<PaymentMethodType.eft>,
					And<PaymentMethod.isActive, Equal<True>, And<CCProcessingCenterPmntMethod.isActive, Equal<True>, And<PaymentMethod.useForAR, Equal<True>>>>>>,
				Aggregate<GroupBy<CCProcessingCenterPmntMethod.paymentMethodID>>>), typeof(CCProcessingCenterPmntMethod.paymentMethodID), typeof(PaymentMethod.descr))]
			[PXUIField(DisplayName = "Payment Method for EFT")]
			public virtual string EftPaymentMethodId { get; set; }

			public abstract class ccPaymentMethodId : IBqlField { }
			[PXString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXSelector(typeof(Search5<CCProcessingCenterPmntMethod.paymentMethodID,
			InnerJoin<PaymentMethod, On<CCProcessingCenterPmntMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>>,
				Where<CCProcessingCenterPmntMethod.processingCenterID, Equal<Current<CreditCardsFilter.processingCenterId>>,
					And<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>,
					And<PaymentMethod.isActive, Equal<True>, And<CCProcessingCenterPmntMethod.isActive, Equal<True>, And<PaymentMethod.useForAR, Equal<True>>>>>>,
				Aggregate<GroupBy<CCProcessingCenterPmntMethod.paymentMethodID>>>), typeof(CCProcessingCenterPmntMethod.paymentMethodID), typeof(PaymentMethod.descr))]
			[PXUIField(DisplayName = "Payment Method for Credit Card")]
			public virtual string CCPaymentMethodId { get; set; }

			public abstract class overwriteEftPaymentMethod : IBqlField
			{
			}
			[PXBool]
			[PXUIField(DisplayName = "Overwrite EFT Values")]
			public virtual bool? OverwriteEftPaymentMethod { get; set; }

			public abstract class overwriteCCPaymentMethod : IBqlField
			{
			}
			[PXBool]
			[PXUIField(DisplayName = "Overwrite Credit Card Values")]
			public virtual bool? OverwriteCCPaymentMethod { get; set; }

			public abstract class enableMultipleSettingCustomer : IBqlField { }
			[PXBool]
			public virtual bool? EnableCustomerPaymentDialog { get; set; }

			[PXBool]
			[PXUnboundDefault(false)]
			public virtual bool? IsScheduleProcess { get; set; }

			public abstract class customerName : IBqlField { }
			[PXUIField(DisplayName = "Select the payment profiles to be assigned to the customer", Enabled = false)]
			public virtual string CustomerName { get; set; }
		}
		public CCSynchronizeCards()
		{
			customerRepo = new CustomerRepository(this);
			CustomerCardPaymentData.SetBeforeScheduleAddAction(() =>
				Filter.Current.ScheduledServiceSync = true
			);
			CustomerCardPaymentData.SetAfterScheduleAddAction(() =>
				Filter.Current.ScheduledServiceSync = false
			);
			CustomerCardPaymentData.SetBeforeScheduleProcessAllAction(() =>
				Filter.Current.IsScheduleProcess = true
			);
			CreditCardsFilter filter = Filter.Current;
			CustomerCardPaymentData.SetProcessDelegate((List<CCSynchronizeCard> items) =>
			{

				if (filter.IsScheduleProcess.GetValueOrDefault() == true
					&& filter.ScheduledServiceSync.GetValueOrDefault() == true)
				{
					DoLoadCards(filter);
				}
				else
				{
					DoImportCards(items);
				}
			});
			
			GroupAction.AddMenuAction(SetDefaultPaymentMethod);
		}

		private static void DoLoadCards(CreditCardsFilter filter)
		{
			CCSynchronizeCards graph = PXGraph.CreateInstance<CCSynchronizeCards>();
			filter.EnableCustomerPaymentDialog = false;
			graph.Filter.Current = filter;

			if (!graph.ValidateLoadCardsAction())
			{
				int newCardsCnt = graph.GetCardsAllProfiles();

				if (newCardsCnt > 0)
				{
					foreach (CCSynchronizeCard syncCard in graph.CustomerCardPaymentData.Cache.Inserted)
					{
						if (syncCard.NoteID.HasValue)
						{
							ProcessingInfo.AppendProcessingInfo(syncCard.NoteID.Value, Messages.LoadCardCompleted);
						}
					}

					try
					{
						graph.Persist();
					}
					catch
					{
						ProcessingInfo.ClearProcessingRows();
						throw;
					}
				}
			}
		}

		private static void DoImportCards(List<CCSynchronizeCard> items)
		{
			int index = 0;
			string procCenterId = procCenterId = items.First()?.CCProcessingCenterID;
			CCSynchronizeCards graph = PXGraph.CreateInstance<CCSynchronizeCards>();

			foreach (CCSynchronizeCard item in items)
			{
				if (graph.ValidateImportCard(item, index) && !graph.CheckCustomerPaymentProfileExists(item, index))
				{
					using (PXTransactionScope scope = new PXTransactionScope())
					{
						graph.CreateCustomerPaymentMethodRecord(item);
						graph.UpdateCCProcessingSyncronizeCardRecord(item);
						scope.Complete();
						PXProcessing<CCSynchronizeCard>.SetInfo(index, Messages.Completed);
					}
				}
				index++;
			}
		}

		public void CreditCardsFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs args)
		{
			CreditCardsFilter filter = args.Row as CreditCardsFilter;
			if (filter == null) return;

			CustomerCardPaymentData.AllowInsert = false;
			CustomerCardPaymentData.AllowDelete = false;
			Actions["LoadCards"].SetIsLockedOnToolbar(true);
			PXButtonState buttonState = (PXButtonState)Actions["Process"].GetState(Filter.Current);
			bool enabled = buttonState.Visible && buttonState.Enabled;
			SetDefaultPaymentMethod.SetEnabled(enabled);
			LoadCards.SetEnabled(enabled);
			Save.SetEnabled(string.IsNullOrEmpty(filter.ProcessingCenterId) ? false : true);
			PXCache cache = Caches[typeof(CustomerPaymentMethod)];
			PXUIFieldAttribute.SetVisible<CustomerPaymentMethod.customerCCPID>(cache, null, true);
			PXUIFieldAttribute.SetVisible<CreditCardsFilter.scheduledServiceSync>(sender, null, false);

			bool isEFTFeatureInstalled = PXAccess.FeatureInstalled<CS.FeaturesSet.acumaticaPayments>();
			PXUIFieldAttribute.SetVisible<CreditCardsFilter.eftPaymentMethodId>(sender, null, isEFTFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CreditCardsFilter.overwriteEftPaymentMethod>(sender, null, isEFTFeatureInstalled);
			LoadCards.SetCaption(isEFTFeatureInstalled ? Messages.LoadCardAccountData : Messages.LoadCardData);
		}

		public virtual void CCSynchronizeCard_RowSelected(PXCache sender, PXRowSelectedEventArgs args)
		{
			PXUIFieldAttribute.SetEnabled<CCSynchronizeCard.bAccountID>(sender, args.Row);
			PXUIFieldAttribute.SetEnabled<CCSynchronizeCard.paymentMethodID>(sender, args.Row);
			PXUIFieldAttribute.SetEnabled<CCSynchronizeCard.cashAccountID>(sender, args.Row);
		}

		public virtual void CCSynchronizeCard_BAccountID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			CCSynchronizeCard syncCard = e.Row as CCSynchronizeCard;
			if (syncCard == null) return;

			string customerCCPID = syncCard.CustomerCCPID;
			PXSelectBase<CustomerProcessingCenterID> cpcQuery = new PXSelect<CustomerProcessingCenterID,
				Where<CustomerProcessingCenterID.customerCCPID, Equal<Required<CustomerProcessingCenterID.customerCCPID>>>, 
				OrderBy<Desc<CustomerProcessingCenterID.createdDateTime>>>(this);
			CustomerProcessingCenterID customerPaymentMethod = cpcQuery.SelectSingle(customerCCPID);

			if (customerPaymentMethod != null)
			{
				e.NewValue = customerPaymentMethod.BAccountID;
				return;
			}

			string customerID = syncCard.PCCustomerID;
			if (customerID == null) return;

			customerID = CCProcessingHelper.DeleteCustomerPrefix(customerID);
			PXSelectBase<Customer> cQuery = new PXSelect<Customer,
				Where<Customer.acctCD, Equal<Required<CCSynchronizeCard.pCCustomerID>>>>(this);
			Customer customer = cQuery.SelectSingle(customerID);

			if (customer != null)
			{
				e.NewValue = customer.BAccountID;
			}
		}

		public virtual void CCSynchronizeCard_PaymentMethodId_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CCSynchronizeCard syncCard = e.Row as CCSynchronizeCard;
			if (syncCard == null) return;

			if (syncCard.CashAccountID != null)
			{
				bool exists = CheckCashAccountAvailability(syncCard);
				if (!exists)
				{
					cache.SetDefaultExt<CCSynchronizeCard.cashAccountID>(syncCard);
				}
			}
		}

		public virtual void CCSynchronizeCard_BAccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CCSynchronizeCard syncCard = e.Row as CCSynchronizeCard;
			if(syncCard == null || !e.ExternalCall || syncCard.BAccountID == null)
				return;

			if (Filter.Current.EnableCustomerPaymentDialog.GetValueOrDefault() == false)
			{
				Filter.Current.EnableCustomerPaymentDialog = true;
				Filter.Current.CustomerName = GetCustomerNameByID(syncCard.BAccountID);
				CustPaymentProfileForDialog.Cache.Clear();
				int insertedCnt = PopulatePaymentProfileForDialog(syncCard.CustomerCCPID, syncCard.PCCustomerID, syncCard.BAccountID);

				if (insertedCnt > 0)
				{
					CustPaymentProfileForDialog.AskExt();
				}
			}

			if (Filter.Current.EnableCustomerPaymentDialog.GetValueOrDefault() == true)
			{
				Filter.Current.EnableCustomerPaymentDialog = false;
			}
		}

		public virtual void CustomerPaymentProfile_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			CustomerPaymentProfile row = e.Row as CustomerPaymentProfile;
			if(row?.Selected == true)
			{ 
				foreach(CCSynchronizeCard syncCard in GetRecordsWithSameCustomerCCPID(row.PCCustomerID))
				{
					if (syncCard.RecordID == row.RecordID && !syncCard.BAccountID.HasValue)
					{
						syncCard.BAccountID = row.BAccountID;
						CustomerCardPaymentData.Update(syncCard);
						CustomerCardPaymentData.View.RequestRefresh();
					}
				}
			}
		}

		private int PopulatePaymentProfileForDialog(string customerCCPID, string customerID, int? bAccountID)
		{
			int insertedRow = 0;
			customerID = CCProcessingHelper.DeleteCustomerPrefix(customerID);
			PXResultset<CCSynchronizeCard> results = CustomerCardPaymentData.Select();

			foreach (CCSynchronizeCard item in results)
			{
				string chkCustromerID = CCProcessingHelper.DeleteCustomerPrefix(item.PCCustomerID);
				if (!item.BAccountID.HasValue && (customerCCPID == item.CustomerCCPID || (!string.IsNullOrEmpty(customerID) && chkCustromerID == customerID)))
				{
					CustomerPaymentProfile cpp = CustomerPaymentProfile.CreateFromSyncCard(item);
					cpp.BAccountID = bAccountID;
					CustPaymentProfileForDialog.Insert(cpp);
					insertedRow++;
				}
			}

			return insertedRow;
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[SyncCardCustomerSelector(typeof(Customer.acctCD), typeof(Customer.acctName), ValidateValue = false)]
		protected virtual void CCSynchronizeCard_BAccountID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXRSACryptStringWithMaskAttribute(1028, typeof(Search<
			PaymentMethodDetail.entryMask,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<CustomerPaymentMethodDetail.paymentMethodID>>,
				And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
				And<PaymentMethodDetail.detailID, Equal<Current<CustomerPaymentMethodDetail.detailID>>>>>>), IsUnicode = true)]
		protected virtual void CustomerPaymentMethodDetail_Value_CacheAttached(PXCache sender) { }


		private string GetCustomerNameByID(int? bAccountId)
		{
			string ret = string.Empty;
			PXSelectBase<Customer> query = new PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(this);
			Customer customer = query.SelectSingle(bAccountId);

			if (customer != null)
			{
				ret = customer.AcctName;
			}
			return ret;
		}

		private List<CCSynchronizeCard> GetRecordsWithSameCustomerCCPID(string customerID)
		{
			customerID = CCProcessingHelper.DeleteCustomerPrefix(customerID);

			if (cacheRecordsSameCustomerCCPID == null)
			{
				cacheRecordsSameCustomerCCPID = new List<CCSynchronizeCard>();

				foreach (CCSynchronizeCard syncCard in CustomerCardPaymentData.Select())
				{
					string chkCustomerID = CCProcessingHelper.DeleteCustomerPrefix(syncCard.PCCustomerID);
					if (chkCustomerID == customerID)
					{
						cacheRecordsSameCustomerCCPID.Add(syncCard);
					}
				}
			}
			return cacheRecordsSameCustomerCCPID;
		}

		private int GetCardsAllProfiles()
		{
			CreditCardsFilter filter = Filter.Current;
			string processingCenter = filter.ProcessingCenterId;
			CreditCardReceiverFactory factory = new CreditCardReceiverFactory(filter);
			CCSynchronizeCardManager syncronizeCardManager = new CCSynchronizeCardManager(this, processingCenter, factory);
			Dictionary<string, CustomerData> customerDatas = syncronizeCardManager.GetCustomerProfilesFromService();
			syncronizeCardManager.SetCustomerProfileIds(customerDatas.Select(i => i.Key));
			Dictionary<string, CustomerCreditCard> unsyncCustomerCreditCards = syncronizeCardManager.GetUnsynchronizedPaymentProfiles();
			int unsyncCardCnt = 0;
			var defaultOnlyEftPaymentMethodId = GetPaymentMethodId(PaymentMethodType.EFT, processingCenter);
			var defaultOnlyCCPaymentMethodId = GetPaymentMethodId(PaymentMethodType.CreditCard, processingCenter);

			foreach (var item in unsyncCustomerCreditCards)
			{
				List<CCSynchronizeCard> alreadyAdded = GetExistedSyncCardEntriesByCustomerCCPID(item.Key, processingCenter);
				CustomerCreditCard cards = item.Value;

				foreach (CreditCardData card in cards.CreditCards)
				{
					if (CheckNotImportedRecordExists(cards.CustomerProfileId, card.PaymentProfileID, alreadyAdded))
						continue;

					CCSynchronizeCard syncCard = new CCSynchronizeCard();
					CustomerData customerData = customerDatas[cards.CustomerProfileId];
					string cardType = GetCardTypeCode(card.CardTypeCode);
					string paymentMethodId;
					syncCard.PaymentType = GetPaymentMethodInfo(card.PaymentMethodType, defaultOnlyEftPaymentMethodId, defaultOnlyCCPaymentMethodId, out paymentMethodId);
					syncCard.PaymentMethodID = paymentMethodId;
					syncCard.CardType = cardType;
					syncCard.ProcCenterCardTypeCode = cardType == CardType.OtherCode ? card.CardType : null;
					string cardNumber = card.CardNumber.Trim('X');
					FormatMaskedCardNum(syncCard, cardNumber);
					syncCard.CCProcessingCenterID = processingCenter;
					syncCard.CustomerCCPID = cards.CustomerProfileId;
					syncCard.CustomerCCPIDHash = CCSynchronizeCard.GetSha1HashString(syncCard.CustomerCCPID);
					syncCard.PaymentCCPID = card.PaymentProfileID;
					syncCard.PCCustomerID = customerData.CustomerCD;
					syncCard.PCCustomerDescription = customerData.CustomerName;
					syncCard.PCCustomerEmail = customerData.Email;

					if (card.CardExpirationDate != null)
					{
						syncCard.ExpirationDate = card.CardExpirationDate.Value;
					}

					if (card.AddressData != null)
					{
						AddressData addrData = card.AddressData;
						syncCard.FirstName = addrData.FirstName;
						syncCard.LastName = addrData.LastName;
					}

					CustomerCardPaymentData.Insert(syncCard);
					unsyncCardCnt++;
				}
			}

			return unsyncCardCnt;
		}

		protected virtual string GetPaymentMethodId(string paymentType, string processingCenterId)
		{
			PXResultset<CCProcessingCenterPmntMethod> queryResult = PXSelectReadonly2<CCProcessingCenterPmntMethod,
				InnerJoin<PaymentMethod, On<CCProcessingCenterPmntMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
				And<PaymentMethod.paymentType, Equal<Required<PaymentMethod.paymentType>>>>>,
				Where<CCProcessingCenterPmntMethod.processingCenterID, Equal<Required<CCSynchronizeCard.cCProcessingCenterID>>,
				And<PaymentMethod.isActive, Equal<True>, And<CCProcessingCenterPmntMethod.isActive, Equal<True>, And<PaymentMethod.useForAR, Equal<True>>>>>>.Select(this, paymentType, processingCenterId);

			if (queryResult.Count == 1)
			{
				return ((CCProcessingCenterPmntMethod)queryResult).PaymentMethodID;
			}

			return null;
		}

		protected virtual string GetPaymentMethodInfo(V2.MeansOfPayment? paymentMethodType, string eftPaymentMethodId, string ccPaymentMethodId, out string paymentMethodId)
		{
			switch (paymentMethodType)
			{
				case MeansOfPayment.CreditCard:
					paymentMethodId = ccPaymentMethodId;
					return PaymentMethodType.CreditCard;

				case MeansOfPayment.EFT:
					paymentMethodId = eftPaymentMethodId;
					return PaymentMethodType.EFT;

				default:
					paymentMethodId = null;
					return null;
			}
		}

		protected virtual string GetCardTypeCode(V2.CCCardType cCCardType)
		{
			CCCardType cardType = V2Converter.ConvertCardType(cCCardType);
			string result = CardType.GetCardTypeCode(cardType);
			return result;
		}

		private bool ValidateLoadCardsAction()
		{
			bool errorOccured = false;
			CreditCardsFilter filter = Filter.Current;
			string processingCenter = filter.ProcessingCenterId;

			if (processingCenter == null && filter.ScheduledServiceSync.GetValueOrDefault() == true)
			{
				throw new PXException(Messages.ProcessingCenterNotSelected);
			}

			if (processingCenter == null)
			{
				Filter.Cache.RaiseExceptionHandling<CreditCardsFilter.processingCenterId>(filter, filter.ProcessingCenterId,
					new PXSetPropertyException(Messages.ProcessingCenterNotSelected));
				errorOccured = true;
			}
			return errorOccured;
		}

		public bool ValidateImportCard(CCSynchronizeCard card, int cardIndex)
		{
			bool ret = true;
			if (card.BAccountID == null)
			{
				PXProcessing<CCSynchronizeCard>.SetError(cardIndex, CA.Messages.CustomerNotDefined);
				ret = false;
			}

			Tuple<CustomerClass, Customer> res = customerRepo.GetCustomerAndClassById(card.BAccountID); 
			if (res != null)
			{
				CustomerClass custClass = res.Item1;
				if (custClass.SavePaymentProfiles == SavePaymentProfileCode.Prohibit)
				{
					Customer cust = res.Item2;
					PXProcessing<CCSynchronizeCard>.SetError(cardIndex, PXMessages.LocalizeFormatNoPrefix(AR.Messages.SavingCardsNotAllowedForCustomerClass, custClass.CustomerClassID, cust.AcctCD));
					ret = false;
				}
			}

			if (card.PaymentMethodID == null)
			{
				PXProcessing<CCSynchronizeCard>.SetError(cardIndex, Messages.PaymentMethodNotDefined);
				ret = false;
			}

			if (card.CashAccountID != null)
			{
				bool exists = CheckCashAccountAvailability(card);

				if (!exists)
				{
					PXProcessing<CCSynchronizeCard>.SetError(cardIndex, 
						PXMessages.LocalizeFormatNoPrefixNLA(AR.Messages.CashAccountIsNotConfiguredForPaymentMethodInAR, card.PaymentMethodID));
					ret = false;
				}
			}
			return ret;
		}

		public void CreateCustomerPaymentMethodRecord(CCSynchronizeCard item)
		{
			PXCache customerPaymentMethodCache = Caches[typeof(CustomerPaymentMethod)];
			CustomerPaymentMethod customerPM = customerPaymentMethodCache.CreateInstance() as CustomerPaymentMethod;
			customerPM.BAccountID = item.BAccountID;
			customerPM.CustomerCCPID = item.CustomerCCPID;
			customerPM.PaymentMethodID = item.PaymentMethodID;
			customerPM.CashAccountID = item.CashAccountID;
			customerPM.CCProcessingCenterID = item.CCProcessingCenterID;
			customerPM.CardType = item.CardType;
			customerPM.ProcCenterCardTypeCode = item.ProcCenterCardTypeCode;

			if (item.ExpirationDate != null)
			{
				customerPaymentMethodCache.SetValueExt<CustomerPaymentMethod.expirationDate>(customerPM, item.ExpirationDate);
			}

			string cardNbr = item.CardNumber;
			string displayMask = GetDisplayMask(item);

			if (displayMask != null)
			{
				string cardType = cardNbr.Split(':')[0];
				string cardId = "XXXX" + cardNbr.Substring(cardNbr.Length - 4);

				cardNbr = CCDisplayMaskService.UseAdjustedDisplayMaskForCardNumber(cardId, displayMask);
				cardNbr = cardType + ':' + cardNbr;
			}

			customerPaymentMethodCache.SetValueExt<CustomerPaymentMethod.descr>(customerPM, cardNbr);
			customerPaymentMethodCache.Insert(customerPM);
			customerPaymentMethodCache.Persist(PXDBOperation.Insert);
			customerPM = customerPaymentMethodCache.Current as CustomerPaymentMethod;
			CreateCustomerPaymentMethodDetailRecord(customerPM, item);
			CreateCustomerProcessingCenterRecord(customerPM, item);
		}

		public void UpdateCCProcessingSyncronizeCardRecord(CCSynchronizeCard item)
		{
			PXCache syncCardCache = Caches[typeof(CCSynchronizeCard)];
			item.Imported = true;
			syncCardCache.Update(item);
			syncCardCache.Persist(PXDBOperation.Update);
		}

		private void CreateCustomerProcessingCenterRecord(CustomerPaymentMethod customerPM, CCSynchronizeCard syncCard)
		{
			PXCache customerProcessingCenterCache = Caches[typeof(CustomerProcessingCenterID)];
			customerProcessingCenterCache.ClearQueryCacheObsolete();
			PXSelectBase<CustomerProcessingCenterID> checkRecordExist = new PXSelectReadonly<CustomerProcessingCenterID,
				Where<CustomerProcessingCenterID.cCProcessingCenterID, Equal<Required<CreditCardsFilter.processingCenterId>>,
					And<CustomerProcessingCenterID.bAccountID, Equal<Required<CustomerProcessingCenterID.bAccountID>>,
					And<CustomerProcessingCenterID.customerCCPID, Equal<Required<CustomerProcessingCenterID.customerCCPID>>>>>>(this);
			CustomerProcessingCenterID cProcessingCenter = checkRecordExist.SelectSingle(syncCard.CCProcessingCenterID, syncCard.BAccountID, syncCard.CustomerCCPID);

			if (cProcessingCenter == null)
			{
				cProcessingCenter = customerProcessingCenterCache.CreateInstance() as CustomerProcessingCenterID;
				cProcessingCenter.BAccountID = syncCard.BAccountID;
				cProcessingCenter.CCProcessingCenterID = syncCard.CCProcessingCenterID;
				cProcessingCenter.CustomerCCPID = syncCard.CustomerCCPID;
				customerProcessingCenterCache.Insert(cProcessingCenter);
				customerProcessingCenterCache.Persist(PXDBOperation.Insert);
			}
		}

		private void CreateCustomerPaymentMethodDetailRecord(CustomerPaymentMethod customerPM, CCSynchronizeCard syncCard)
		{
			PXResultset<PaymentMethodDetail> details = GetPaymentMethodDetailParams(customerPM.PaymentMethodID);
			PXCache customerPaymentMethodDetailCache = Caches[typeof(CustomerPaymentMethodDetail)];
			CustomerPaymentMethodDetail customerPaymentDetails;

			foreach (PaymentMethodDetail detail in details)
			{
				customerPaymentDetails = customerPaymentMethodDetailCache.CreateInstance() as CustomerPaymentMethodDetail;
				customerPaymentDetails.DetailID = detail.DetailID;
				customerPaymentDetails.PMInstanceID = customerPM.PMInstanceID;
				customerPaymentDetails.PaymentMethodID = customerPM.PaymentMethodID;

				if (customerPaymentDetails.DetailID == CreditCardAttributes.CardNumber)
				{
					Match match = new Regex("[\\d]+").Match(syncCard.CardNumber);
					if (match.Success)
					{
						string cardNum = match.Value.PadLeft(8, 'X');
						customerPaymentDetails.Value = cardNum;
					}
				}

				if (customerPaymentDetails.DetailID == CreditCardAttributes.CCPID)
				{
					customerPaymentDetails.Value = syncCard.PaymentCCPID;
				}

				customerPaymentMethodDetailCache.Insert(customerPaymentDetails);
				customerPaymentMethodDetailCache.Persist(PXDBOperation.Insert);
			}
		}

		private string GetDisplayMask(CCSynchronizeCard item)
		{
			PXResultset<PaymentMethodDetail> details = GetPaymentMethodDetailParams(item.PaymentMethodID);

			foreach (PaymentMethodDetail detail in details)
				if (detail.DetailID == "CCDNUM")
					return detail.DisplayMask;

			return null;
		}

		private PXResultset<PaymentMethodDetail> GetPaymentMethodDetailParams(string paymentMethodId)
		{
			PXSelectBase<PaymentMethodDetail> query = new PXSelectReadonly<PaymentMethodDetail,
				Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>>>(this);
			PXResultset<PaymentMethodDetail> result = query.Select(paymentMethodId);
			return result;
		}

		protected virtual void FormatMaskedCardNum(CCSynchronizeCard syncCard, string cardNumber)
		{
			syncCard.CardNumber = CCDisplayMaskService.UseDefaultMaskForCardNumber(cardNumber, CardType.GetDisplayName(syncCard.CardType));
		}

		private bool CheckCashAccountAvailability(CCSynchronizeCard row)
		{
			IEnumerable<CashAccount> availableCA = PXSelectorAttribute.SelectAll<CCSynchronizeCard.cashAccountID>(this.CustomerCardPaymentData.Cache, row)
					.RowCast<CashAccount>();
			bool exists = availableCA.Any(i => i.CashAccountID == row.CashAccountID);
			return exists;
		}

		private IEnumerable<PXResult<CustomerPaymentMethod, CustomerPaymentMethodDetail>> GetPaymentsProfilesByCustomer(string processingCenterID, string customerCCPID)
		{
			PXSelectBase<CustomerPaymentMethod> query = new PXSelectReadonly2<CustomerPaymentMethod, 
				InnerJoin<CustomerPaymentMethodDetail, On<CustomerPaymentMethod.pMInstanceID, Equal<CustomerPaymentMethodDetail.pMInstanceID>>,
				InnerJoin<PaymentMethodDetail, On<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
					And<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>>>>>,
				Where<CustomerPaymentMethod.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>,
					And<CustomerPaymentMethod.customerCCPID, Equal<Required<CustomerPaymentMethod.customerCCPID>>,
					And<PaymentMethodDetail.isCCProcessingID, Equal<True>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>>(this);
			var result = query.Select(processingCenterID, customerCCPID).Select(i => (PXResult<CustomerPaymentMethod, CustomerPaymentMethodDetail>)i);
			return result;
		}

		private List<CCSynchronizeCard> GetExistedSyncCardEntriesByCustomerCCPID(string customerCCPID, string processingCenterId)
		{
			string customerIdHash = CCSynchronizeCard.GetSha1HashString(customerCCPID);
			PXSelectBase<CCSynchronizeCard> query = new PXSelect<CCSynchronizeCard,
				Where<CCSynchronizeCard.customerCCPIDHash, Equal<Required<CCSynchronizeCard.customerCCPIDHash>>,
					And<CCSynchronizeCard.cCProcessingCenterID, Equal<Required<CCSynchronizeCard.cCProcessingCenterID>>>>>(this);
			var ret = query.Select(customerIdHash, processingCenterId).RowCast<CCSynchronizeCard>().ToList();
			return ret;
		}

		private bool CheckNotImportedRecordExists(string custCCPID, string paymentCCPID, List<CCSynchronizeCard> checkList)
		{
			bool ret = false;
			CCSynchronizeCard item = checkList.Where(i => i.CustomerCCPID == custCCPID && i.PaymentCCPID == paymentCCPID
				&& i.Imported.GetValueOrDefault() == false).FirstOrDefault();

			if (item != null)
			{
				ret = true;
			}
			return ret;
		}

		private bool CheckCustomerPaymentProfileExists(CCSynchronizeCard syncCard, int cardIndex)
		{
			var result = GetPaymentsProfilesByCustomer(syncCard.CCProcessingCenterID, syncCard.CustomerCCPID);
			string checkPaymentCCPID = syncCard.PaymentCCPID;

			foreach (CustomerPaymentMethodDetail cpmDetail in result.RowCast<CustomerPaymentMethodDetail>())
			{
				if (cpmDetail.Value == checkPaymentCCPID)
				{
					PXProcessing<CCSynchronizeCard>.SetError(cardIndex, Messages.RecordWithPaymentCCPIDExists);
					return true;
				}
			}
			return false;
		}

		public override void Persist()
		{
			CustPaymentProfileForDialog.Cache.Clear();
			base.Persist();
		}
	}
}


