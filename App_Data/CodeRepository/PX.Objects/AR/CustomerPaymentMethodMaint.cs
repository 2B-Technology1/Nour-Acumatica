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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.Repositories;
using PX.Objects.CA;
using PX.Objects.Common;
using PX.Objects.CR;

namespace PX.Objects.AR
{
	public class CustomerPaymentMethodMaint : PXGraph<CustomerPaymentMethodMaint, CustomerPaymentMethod>
	{
		#region InternalVariables

		[InjectDependency]
		public ICCDisplayMaskService CCDisplayMaskService { get; set; }

		#endregion

		#region Buttons

		public PXAction<CustomerPaymentMethod> viewBillAddressOnMap;

		[PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton()]
		public virtual IEnumerable ViewBillAddressOnMap(PXAdapter adapter)
		{

			BAccountUtility.ViewOnMap(this.BillAddress.Current);
			return adapter.Get();
		}

		public PXAction<CustomerPaymentMethod> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			CustomerPaymentMethod doc = this.CustomerPaymentMethod.Current;
			if (doc != null && doc.BillAddressID.HasValue)
			{
				Address address = this.BillAddress.Current;
				if (address != null && address.IsValidated == false)
				{
					CS.PXAddressValidator.Validate<Address>(this, address, true, true);
				}
			}
			return adapter.Get();
		}

		#endregion

		#region Public Selects
		public CustomerPaymentMethodMaint()
		{
			ARSetup setup = ARSetup.Current;
			this.Details.Cache.AllowInsert = false;
			this.Details.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethodDetail.detailID>(this.Details.Cache, null, false);
			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.CustomerType; });
		}

		[PXCopyPasteHiddenFields(typeof(CustomerPaymentMethod.customerCCPID),
								 typeof(CustomerPaymentMethod.expirationDate),
								 typeof(CustomerPaymentMethod.descr))]
		public PXSelect<CustomerPaymentMethod,
				Where<CustomerPaymentMethod.bAccountID, Equal<Optional<CustomerPaymentMethod.bAccountID>>>> CustomerPaymentMethod;
		public PXSelect<CustomerPaymentMethod,
				Where<CustomerPaymentMethod.bAccountID, Equal<Current<CustomerPaymentMethod.bAccountID>>,
						And<CustomerPaymentMethod.pMInstanceID, Equal<Current<CustomerPaymentMethod.pMInstanceID>>>>> CurrentCPM;


		[PXCopyPasteHiddenFields(typeof(CustomerPaymentMethodDetail.value))]
		public PXSelectJoinOrderBy<CustomerPaymentMethodDetail, InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>>>,
			OrderBy<Asc<PaymentMethodDetail.orderIndex>>> Details;

		public PXSelectJoin<CustomerPaymentMethodDetail, InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
					And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
				Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Current<CustomerPaymentMethod.pMInstanceID>>>, OrderBy<Asc<PaymentMethodDetail.orderIndex>>> DetailsAll;
		public PXSelect<PaymentMethodDetail,
				Where<PaymentMethodDetail.paymentMethodID, Equal<Optional<CustomerPaymentMethod.paymentMethodID>>,
				And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>> PMDetails;
		public PXSelect<PaymentMethod,
				Where<PaymentMethod.paymentMethodID, Equal<Optional<CustomerPaymentMethod.paymentMethodID>>>> PaymentMethodDef;

		public PXSelectJoin<CustomerPaymentMethodDetail, InnerJoin<PaymentMethodDetail,
			On<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
				And<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
			Where<PaymentMethodDetail.isCCProcessingID, Equal<True>, And<CustomerPaymentMethodDetail.pMInstanceID, Equal<Current<CustomerPaymentMethod.pMInstanceID>>>>> ccpIdDet;

		public PXSelect<Address, Where<Address.addressID, Equal<Optional<CustomerPaymentMethod.billAddressID>>>> BillAddress;
		public PXSelect<Contact, Where<Contact.contactID, Equal<Optional<CustomerPaymentMethod.billContactID>>>> BillContact;
		public PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<CustomerPaymentMethod.bAccountID>>>> Customer;

		public PXSelect<CustomerProcessingCenterID, Where<CustomerProcessingCenterID.bAccountID, Equal<Current<CustomerPaymentMethod.bAccountID>>,
			And<CustomerProcessingCenterID.cCProcessingCenterID, Equal<Current<CustomerPaymentMethod.cCProcessingCenterID>>,
			And<CustomerProcessingCenterID.customerCCPID, Equal<Optional<CustomerPaymentMethod.customerCCPID>>>>>> CustomerProcessingID;

		public PXSetup<ARSetup> ARSetup;

		#endregion

		#region Cache Attached
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.Visible)]
		protected virtual void Customer_AcctCD_CacheAttached(PXCache sender)
		{
		}
        #endregion

        #region Select Delagates
        public IEnumerable billAddress()
		{
			CustomerPaymentMethod row = this.CustomerPaymentMethod.Current;
			if (row != null && row.BAccountID != null)
			{
				if (row.BillAddressID != null)
				{
					return PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(this, row.BillAddressID);
				}
				else
				{
					return PXSelectJoin<Address, InnerJoin<Customer, On<Customer.defBillAddressID, Equal<Address.addressID>>>,
										Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, row.BAccountID);
				}
			}
			return null;
		}

		public IEnumerable billContact()
		{
			CustomerPaymentMethod row = this.CustomerPaymentMethod.Current;
			if (row != null && row.BAccountID != null)
			{
				if (row.BillContactID != null)
				{
					return PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(this, row.BillContactID);
				}
				else
				{
					return PXSelectJoin<Contact, InnerJoin<Customer, On<Customer.defBillContactID, Equal<Contact.contactID>>>,
										Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, row.BAccountID);
				}
			}
			return null;
		}

		public IEnumerable details()
		{
			return CCProcessingHelper.GetPMdetails(this, CustomerPaymentMethod.Current);
		}

		#endregion

		#region Main Record Events

		protected virtual void CustomerPaymentMethod_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
			if (row != null)
			{
				cache.SetDefaultExt<CustomerPaymentMethod.pMInstanceID>(row);
			}
		}


		protected virtual void CustomerPaymentMethod_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CustomerPaymentMethod row = e.Row as CustomerPaymentMethod;
			if (row == null) return;

			if (row.BAccountID != null)
			{
				this.bAccountID = row.BAccountID;
			}

			PXUIFieldAttribute.SetEnabled(this.Details.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.descr>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.customerCCPID>(cache, row, string.IsNullOrEmpty(row.Descr));
			PXUIFieldAttribute.SetVisible<CustomerPaymentMethod.expirationDate>(cache, row, false);
			bool isTokenized = CCProcessingHelper.IsTokenizedPaymentMethod(this, CustomerPaymentMethod.Current.PMInstanceID, true);
			bool isTokenizedMethodActive = PXAccess.FeatureInstalled<CS.FeaturesSet.integratedCardProcessing>() && isTokenized;			
			Details.Cache.AllowUpdate = (cache.GetStatus(row) == PXEntryStatus.Inserted) || !isTokenized;
			validateAddresses.SetEnabled(row.HasBillingInfo == true);

			if (!string.IsNullOrEmpty(row.PaymentMethodID))
			{
				PaymentMethod pmDef = (PaymentMethod)this.PaymentMethodDef.Select(row.PaymentMethodID);

				bool singleInstance = pmDef.ARIsOnePerCustomer ?? false;
				bool isIDMaskExists = false;
				PaymentMethod pm = PaymentMethodDef.Select();
				if (!singleInstance)
				{
					foreach (PaymentMethodDetail iDef in this.PMDetails.Select(row.PaymentMethodID))
					{
						if ((iDef.IsIdentifier ?? false) && (!string.IsNullOrEmpty(iDef.DisplayMask)))
						{
							isIDMaskExists = true;
							break;
						}
					}
				}
				if (!(singleInstance || isIDMaskExists || isTokenizedMethodActive))
				{
					PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.descr>(cache, row, true);
				}
				bool isCCProcessingPM = pmDef.PaymentType.IsIn(PaymentMethodType.EFT, PaymentMethodType.CreditCard);
				PXUIFieldAttribute.SetVisible<CustomerPaymentMethod.displayCardType>(cache, row, isCCProcessingPM);
				PXUIFieldAttribute.SetVisible<CustomerPaymentMethod.descr>(cache, row, isCCProcessingPM || (pmDef.IsAccountNumberRequired ?? false));
				PXDefaultAttribute.SetPersistingCheck<CustomerPaymentMethod.descr>(cache, row, (isIDMaskExists ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank));
				row.HasBillingInfo = pmDef.ARHasBillingInfo;
				bool integratedProcessing = PXAccess.FeatureInstalled<CS.FeaturesSet.integratedCardProcessing>() && pmDef.ARIsProcessingRequired == true;
				PXUIFieldAttribute.SetVisible<CustomerPaymentMethod.cCProcessingCenterID>(cache, row, (integratedProcessing || row.CCProcessingCenterID != null) &&isCCProcessingPM);
				if (!string.IsNullOrEmpty(row.CCProcessingCenterID))
				{
					bool visibleCustId = (isTokenizedMethodActive || row.CustomerCCPID != null) && isCCProcessingPM;
					PXUIFieldAttribute.SetVisible<CustomerPaymentMethod.customerCCPID>(cache, row, visibleCustId);
					PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.cCProcessingCenterID>(cache, row, integratedProcessing && (!isTokenized || string.IsNullOrEmpty(row.Descr)));
				}
				if (pmDef.PaymentType.Equals(PaymentMethodType.EFT))
				{
					UIState.RaiseOrHideError<CustomerPaymentMethod.paymentMethodID>(cache, row, true, Messages.WarningEftAllPayments, PXErrorLevel.Warning, pmDef);
				}
				bool isExpirationDateVisible = isTokenizedMethodActive || row.ExpirationDate != null;
				PXUIFieldAttribute.SetVisible<CustomerPaymentMethod.expirationDate>(cache, row, isExpirationDateVisible && isCCProcessingPM);
			}
			bool isInserted = (cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
			PXUIFieldAttribute.SetEnabled<CustomerPaymentMethod.paymentMethodID>(cache, row, (isInserted || String.IsNullOrEmpty(row.PaymentMethodID)));
			if (!isInserted && (!string.IsNullOrEmpty(row.PaymentMethodID)))
			{
				if (!this.IsContractBasedAPI)
				{
					this.MergeDetailsWithDefinition(row);
				}
				bool hasTransactions = ExternalTranHelper.HasTransactions(this, row.PMInstanceID);
				this.Details.Cache.AllowDelete = !hasTransactions;
				PXUIFieldAttribute.SetEnabled(this.Details.Cache, null, !hasTransactions);
			}
			if (row.BAccountID != null)
			{
				Customer customer = (Customer)this.Customer.Select(row.BAccountID);
				row.IsBillContactSameAsMain = (row.BillContactID == null) || (customer.DefBillContactID == row.BillContactID);
				row.IsBillAddressSameAsMain = (row.BillAddressID == null) || (customer.DefBillAddressID == row.BillAddressID);
			}
			if (row.CashAccountID.HasValue)
			{
				PaymentMethodAccount pmAcct = PXSelect<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
					And<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>,
					And<PaymentMethodAccount.useForAR, Equal<True>>>>>.Select(this, row.CashAccountID, row.PaymentMethodID);
				PXUIFieldAttribute.SetWarning<CustomerPaymentMethod.cashAccountID>(cache, e.Row, pmAcct == null ? PXMessages.LocalizeFormatNoPrefixNLA(Messages.CashAccountIsNotConfiguredForPaymentMethodInAR, row.PaymentMethodID) : null);
			}

			CCProcessingCenter procCenter = new PXSelect<CCProcessingCenter,
			Where<CCProcessingCenter.processingCenterID, Equal<Current<CustomerPaymentMethod.cCProcessingCenterID>>>>(this).SelectSingle();

			if (row.IsActive == false)
			{
				ARInvoice doc = PXSelectJoin<ARInvoice, InnerJoin<GL.Schedule, On<GL.Schedule.scheduleID, Equal<ARInvoice.scheduleID>, And<GL.Schedule.active, Equal<True>>>>,
										Where<ARInvoice.scheduled, Equal<True>, And<ARInvoice.pMInstanceID, Equal<Required<ARInvoice.pMInstanceID>>>>>.Select(this, row.PMInstanceID);
				if (doc != null)
				{
					cache.RaiseExceptionHandling<CustomerPaymentMethod.isActive>(row, row.IsActive, new PXSetPropertyException(Messages.InactiveCustomerPaymentMethodIsUsedInTheScheduledInvoices, PXErrorLevel.Warning));
				}
				else
				{
					cache.RaiseExceptionHandling<CustomerPaymentMethod.isActive>(row, null, null);
				}
			}
			else
			{
				cache.RaiseExceptionHandling<CustomerPaymentMethod.isActive>(row, null, null);
			}

			if (row.IsActive == true)
			{
				bool rowWithDataExists = DetailsAll.Select().RowCast<CustomerPaymentMethodDetail>().Any(i => !string.IsNullOrEmpty(i.Value));
				if (!isInserted && !rowWithDataExists && !string.IsNullOrEmpty(row.CCProcessingCenterID) && !string.IsNullOrEmpty(row.Descr))
				{
					cache.RaiseExceptionHandling<CustomerPaymentMethod.isActive>(row, null, new PXSetPropertyException(Messages.CCProcessingDetailsWereDeleted, PXErrorLevel.Warning));
				}
				else
				{
					cache.RaiseExceptionHandling<CustomerPaymentMethod.isActive>(row, null, null);
				}
			}

			if (row.CCProcessingCenterID != null && CCPluginTypeHelper.CheckProcessingCenterPlugin(this, row.CCProcessingCenterID) != CCPaymentProcessing.Common.CCPluginCheckResult.Ok)
			{
				PXUIFieldAttribute.SetEnabled(this.Details.Cache, null, false);
			}

			foreach (CustomerPaymentMethodDetail CPMDetail in Details.Select())
			{
				Details.Cache.RaiseRowSelected(CPMDetail);
				if (Details.Cache.GetStatus(CPMDetail) == PXEntryStatus.Notchanged)
					Details.Cache.SetStatus(CPMDetail, PXEntryStatus.Held);
			}
		}

		protected virtual void CustomerPaymentMethod_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
			{
				CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
				PaymentMethod def = this.PaymentMethodDef.Select(row.PaymentMethodID);
				if (def != null && (def.ARIsOnePerCustomer ?? false) && e.Operation == PXDBOperation.Insert)
				{
					CustomerPaymentMethod existing = PXSelect<CustomerPaymentMethod,
					Where<CustomerPaymentMethod.bAccountID, Equal<Required<CustomerPaymentMethod.bAccountID>>,
					And<CustomerPaymentMethod.paymentMethodID, Equal<Required<CustomerPaymentMethod.paymentMethodID>>,
					And<CustomerPaymentMethod.pMInstanceID, NotEqual<Required<CustomerPaymentMethod.pMInstanceID>>>>>>.Select(this, row.BAccountID, row.PaymentMethodID, row.PMInstanceID);
					if (existing != null)
					{
						throw new PXException(Messages.PaymentMethodIsAlreadyDefined);
					}
				}

				if (row != null)
				{
					if (row.CCProcessingCenterID != null)
					{
						var checkResult = CCPluginTypeHelper.CheckProcessingCenterPlugin(this, row.CCProcessingCenterID);
						switch (checkResult)
						{
							case CCPaymentProcessing.Common.CCPluginCheckResult.Empty:
								throw new PXException(AR.Messages.ERR_PluginTypeIsNotSelectedForProcessingCenter, row.CCProcessingCenterID);
							case CCPaymentProcessing.Common.CCPluginCheckResult.Missing:
								throw new PXException(AR.Messages.PaymentProfileProcCenterMissing, row.CCProcessingCenterID);
							case CCPaymentProcessing.Common.CCPluginCheckResult.Unsupported:
								throw new PXException(AR.Messages.PaymentProfileProcCenterNotSupported, row.CCProcessingCenterID);
						}
					}

					Customer customer = Customer.Select(row.BAccountID);
					if (customer != null && customer.DefPaymentMethodID == row.PaymentMethodID)
					{
						PaymentMethod pm = PaymentMethodDef.Select();
						if (pm != null && pm.ARIsOnePerCustomer == true)
						{
							customer.DefPMInstanceID = row.PMInstanceID;
							Customer.Update(customer);
						}
					}

					if (!string.IsNullOrEmpty(row.CustomerCCPID))
					{
						CustomerProcessingCenterID test = CustomerProcessingID.Select(row.CustomerCCPID);
						if (test == null)
						{
							CustomerProcessingCenterID cPCID = new CustomerProcessingCenterID();
							cPCID.BAccountID = row.BAccountID;
							cPCID.CCProcessingCenterID = row.CCProcessingCenterID;
							cPCID.CustomerCCPID = row.CustomerCCPID;
							CustomerProcessingID.Insert(cPCID);
						}
					}
				}

			}
			else
			{
				CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
				if (row != null)
				{
					Customer customer = Customer.Select(row.BAccountID);
					if (customer != null && customer.DefPaymentMethodID == row.PaymentMethodID)
					{
						PaymentMethod pm = PaymentMethodDef.Select(row.PaymentMethodID);
						if (pm != null)
						{
							if (pm.ARIsOnePerCustomer == true)
							{
								customer.DefPMInstanceID = pm.PMInstanceID;
								Customer.Update(customer);
							}
							else
							{
								PXResultset<CustomerPaymentMethod> otherMethods = PXSelect<CustomerPaymentMethod,
									Where<CustomerPaymentMethod.paymentMethodID, Equal<Required<CustomerPaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
								if (otherMethods.Count == 0)
								{
									customer.DefPMInstanceID = null;
									customer.DefPaymentMethodID = null;
									Customer.Update(customer);
								}
							}
						}
					}
				}
			}
		}

		protected virtual void CustomerPaymentMethod_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			CustomerPaymentMethod row = e.Row as CustomerPaymentMethod;

			if (row.IsActive != true
				&& !cache.ObjectsEqual<CustomerPaymentMethod.isActive>(e.OldRow, e.Row))
			{
				Customer currCustomer = Customer.Select(row.BAccountID);

				if (currCustomer != null
					&& currCustomer.DefPMInstanceID == row.PMInstanceID)
				{
					currCustomer.DefPaymentMethodID = null;
					currCustomer.DefPMInstanceID = null;
					Customer.Update(currCustomer);
				}
			}
		}

		protected virtual void CustomerPaymentMethod_BAccountID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
			if (this.bAccountID != null)
			{
				e.NewValue = this.bAccountID;
				e.Cancel = true;
			}
		}
		protected virtual void CustomerPaymentMethod_PaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
			this.ClearDetails();
			this.AddDetails(row.PaymentMethodID);
			row.CashAccountID = null;
			cache.SetDefaultExt<CustomerPaymentMethod.cashAccountID>(e.Row);
			PaymentMethod pmDef = this.PaymentMethodDef.Select(row.PaymentMethodID);
			if(PXAccess.FeatureInstalled<CS.FeaturesSet.integratedCardProcessing>() && pmDef?.ARIsProcessingRequired == true)
			{
				cache.SetDefaultExt<CustomerPaymentMethod.cCProcessingCenterID>(e.Row);
			}
			if (pmDef?.ARIsOnePerCustomer == true)
			{
				cache.SetValueExt<CustomerPaymentMethod.descr>(row, pmDef.Descr);
			}
			this.Details.View.RequestRefresh();
		}

		protected virtual void CustomerPaymentMethod_CCProcessingCenterID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethod row = e.Row as CustomerPaymentMethod;
			if (row == null) return;
			cache.SetDefaultExt<CustomerPaymentMethod.customerCCPID>(e.Row);
		}

		protected virtual void CustomerPaymentMethod_CustomerCCPID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			CustomerPaymentMethod row = e.Row as CustomerPaymentMethod;
			if (row == null) return;
			if (row.CustomerCCPID == e.NewValue?.ToString())
			{
				// this validation should affect only newsly created cpm 
				// updating other fields should not affect this
				return;
			}

			CustomerPaymentMethod cpm = SelectFrom<CustomerPaymentMethod>
											.Where<CustomerPaymentMethod.bAccountID.IsNotEqual<@P.AsInt>
											.And<CustomerPaymentMethod.customerCCPID.IsEqual<@P.AsString>>>
											.View.ReadOnly.Select(this, row.BAccountID, e.NewValue);
			if (cpm != null)
			{
				BAccount bAccount = SelectFrom<BAccount>.Where<BAccount.bAccountID.IsEqual<@P.AsInt>>.View.ReadOnly.Select(this, cpm.BAccountID);
				cache.RaiseExceptionHandling<CustomerPaymentMethod.customerCCPID>(e.Row, e.NewValue, new PXSetPropertyException(Messages.CustomerProfileIDAlreadyInUse, e.NewValue, bAccount.AcctCD, PXErrorLevel.Error));
				e.NewValue = null;
			}
		}

		protected virtual void CustomerPaymentMethod_Descr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
			PaymentMethod def = this.PaymentMethodDef.Select(row.PaymentMethodID);
			if (!(def.ARIsOnePerCustomer ?? false))
			{
				CustomerPaymentMethod existing = PXSelect<CustomerPaymentMethod,
				Where<CustomerPaymentMethod.bAccountID, Equal<Required<CustomerPaymentMethod.bAccountID>>,
				And<CustomerPaymentMethod.paymentMethodID, Equal<Required<CustomerPaymentMethod.paymentMethodID>>,
				And<CustomerPaymentMethod.pMInstanceID, NotEqual<Required<CustomerPaymentMethod.pMInstanceID>>,
				And<CustomerPaymentMethod.descr, Equal<Required<CustomerPaymentMethod.descr>>,
				And<CustomerPaymentMethod.isActive, Equal<True>>>>>>>.Select(this, row.BAccountID, row.PaymentMethodID, row.PMInstanceID, row.Descr);
				if (existing != null)
				{
					cache.RaiseExceptionHandling<CustomerPaymentMethod.descr>(row, row.Descr, new PXSetPropertyException(Messages.CustomerPMInstanceHasDuplicatedDescription, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void CustomerPaymentMethod_IsBillAddressSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
			if (row != null)
			{
				if (row.IsBillAddressSameAsMain == true)
				{
					Customer def = this.Customer.Select(row.BAccountID);
					if (row.BillAddressID.HasValue)
					{
						Address addr = this.BillAddress.Select(row.BillAddressID);
						if (addr != null && addr.AddressID != def.DefBillAddressID && addr.AddressID != def.DefAddressID)
						{
							this.BillAddress.Delete(addr);
						}
					}
					//row.BillAddressID = def.DefBillAddressID;
					row.BillAddressID = null;
				}
				else
				{
					int? id = row.BillAddressID;
					if (!id.HasValue)
					{
						Customer def = this.Customer.Select(row.BAccountID);
						id = def.DefBillAddressID;
						id = null;
					}
					Address addr = this.BillAddress.Select(id);
					if (addr != null)
					{
						Address copy = (Address)this.BillAddress.Cache.CreateCopy(addr);
						copy.AddressID = null;
						addr = this.BillAddress.Insert(copy);
						row.BillAddressID = addr.AddressID;
					}
				}
				//this.BillAddress.View.RequestRefresh();
			}
		}
		protected virtual void CustomerPaymentMethod_IsBillContactSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;
			if (row != null)
			{
				if (row.IsBillContactSameAsMain == true)
				{
					Customer def = this.Customer.Select(row.BAccountID);
					if (row.BillContactID.HasValue)
					{
						Contact addr = this.BillContact.Select(row.BillContactID);
						if (addr != null && addr.ContactID != def.DefBillContactID && addr.ContactID != def.DefContactID)
						{
							this.BillContact.Delete(addr);
						}
					}
					row.BillContactID = null;
				}
				else
				{
					int? id = row.BillContactID;
					if (!id.HasValue)
					{
						Customer def = this.Customer.Select(row.BAccountID);
						id = def.DefBillContactID;
					}
					Contact addr = this.BillContact.Select(id);
					if (addr != null)
					{
						Contact copy = (Contact)this.BillContact.Cache.CreateCopy(addr);
						copy.ContactID = null;
						addr = this.BillContact.Insert(copy);
						row.BillContactID = addr.ContactID;
					}
				}
				//this.BillContact.View.RequestRefresh();
			}
		}

		protected virtual void CustomerPaymentMethod_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			CustomerPaymentMethod row = (CustomerPaymentMethod)e.Row;

			if (row != null)
			{
				Customer def = this.Customer.Select(row.BAccountID);
				PXEntryStatus status = this.CustomerPaymentMethod.Cache.GetStatus(e.Row);

				ARPayment payment = PXSelectReadonly<
					ARPayment,
					Where<
						ARPayment.customerID, Equal<Required<CustomerPaymentMethod.bAccountID>>,
						And<ARPayment.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>>
					.Select(this, row.BAccountID, row.PMInstanceID);

				if (payment != null)
				{
					if (CCProcessingHelper.IsTokenizedPaymentMethod(this, row.PMInstanceID, true)
						|| row.CCProcessingCenterID == null)
					{
						string msg = GetReferentialIntegrityViolationMessage(row, payment);

						PXTrace.WriteWarning(msg);
						throw new PXException(msg);
					}
					else
					{
						string docType;
						ARDocType.ListAttribute list = new ARDocType.ListAttribute();
						list.ValueLabelDic.TryGetValue(payment.DocType, out docType);
						WebDialogResult confirmDelete = CurrentCPM.Ask(
							PXMessages.LocalizeFormatNoPrefix(Messages.ConfirmDeleteCustomerPaymentMethod, docType, payment.RefNbr),
							MessageButtons.YesNo);
						if (confirmDelete != WebDialogResult.Yes)
						{
							e.Cancel = true;
							return;
						}
					}
				}

				if (row.BillContactID.HasValue)
				{
					Contact addr = this.BillContact.Select(row.BillContactID);
					if (addr != null && addr.ContactID != def.DefBillContactID
						&& addr.ContactID != def.DefContactID)
					{
						this.BillContact.Delete(addr);
					}
				}
				if (row.BillAddressID.HasValue)
				{
					Address addr = this.BillAddress.Select(row.BillAddressID);
					if (addr != null && addr.AddressID != def.DefBillAddressID
							&& addr.AddressID != def.DefAddressID)
					{
						this.BillAddress.Delete(addr);
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<CustomerPaymentMethod.cashAccountID> e)
		{
			if (e.NewValue == null)
				return;
			
			if (!(e.NewValue is int newCashAccountId))
				return;

			CustomerPaymentMethod customerPaymentMethod = e.Row as CustomerPaymentMethod;
			if (string.IsNullOrEmpty(customerPaymentMethod?.CCProcessingCenterID))
				return;

			CCProcessingCenter processingCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>.Select(this, customerPaymentMethod.CCProcessingCenterID);
			if (!(processingCenter?.CashAccountID).HasValue)
				return;

			CashAccount cashAccountProccessingCenter = CashAccount.PK.Find(this, processingCenter.CashAccountID);
			CashAccount cashAccount = CashAccount.PK.Find(this, newCashAccountId);
			if (string.IsNullOrEmpty(cashAccountProccessingCenter.CuryID) || string.IsNullOrEmpty(cashAccount.CuryID))
				return;

			if (!cashAccountProccessingCenter.CuryID.Equals(cashAccount.CuryID))
			{
				e.NewValue = cashAccount.CashAccountCD;
				e.Cancel = true;
				throw new PXSetPropertyException<CustomerPaymentMethod.cashAccountID>(Messages.ProcCenterCuryIDDifferentFromCashAccountCuryID,
					processingCenter.ProcessingCenterID, cashAccountProccessingCenter.CuryID,
					cashAccount.CashAccountCD, cashAccount.CuryID, PXErrorLevel.Error);
			}
		}
		#endregion

		#region PM Details Events
		protected virtual void CustomerPaymentMethodDetail_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			CustomerPaymentMethodDetail row = (CustomerPaymentMethodDetail)e.Row;
			PaymentMethodDetail def = this.FindTemplate(row);
			if (def != null && def.IsIdentifier == true)
			{
				this.CustomerPaymentMethod.Current.Descr = null;
			}
		}

		protected virtual void CustomerPaymentMethodDetailRowPersisting(Events.RowPersisting<CustomerPaymentMethodDetail> e)
		{
			if (e.Row == null) return;

			var row = e.Row;
			PaymentMethodDetail iTempl = this.FindTemplate(row);
			if (iTempl != null && iTempl.IsRequired == true && string.IsNullOrWhiteSpace(row.Value))
			{
				var paymentMethodId = CustomerPaymentMethod.Current?.PaymentMethodID;
				e.Cache.RaiseExceptionHandling(nameof(CustomerPaymentMethodDetail.value), e.Row, null, new PXSetPropertyException(Messages.RequiredCPMDetailIsEmpty, row.DetailID, paymentMethodId));
			}
		}

		protected virtual void CustomerPaymentMethodDetail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CustomerPaymentMethodDetail row = (CustomerPaymentMethodDetail)e.Row;
			if (row != null && !String.IsNullOrEmpty(row.PaymentMethodID))
			{
				PaymentMethodDetail iTempl = this.FindTemplate(row);
				if (iTempl != null)
				{
					bool isRequired = (iTempl.IsRequired ?? false);
					PXDefaultAttribute.SetPersistingCheck<CustomerPaymentMethodDetail.value>(cache, row, (isRequired) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
					bool showDecripted = !(iTempl.IsEncrypted ?? false);
					PXRSACryptStringAttribute.SetDecrypted<CustomerPaymentMethodDetail.value>(cache, row, showDecripted);
				}
				else
				{
					PXDefaultAttribute.SetPersistingCheck<CustomerPaymentMethodDetail.value>(cache, row, PXPersistingCheck.Nothing);
				}
			}
		}

		protected virtual void CustomerPaymentMethodDetail_Value_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			CustomerPaymentMethodDetail row = e.Row as CustomerPaymentMethodDetail;
			PaymentMethodDetail def = this.FindTemplate(row);
			if (def != null)
			{
				if (def.IsCCProcessingID == true)
				{
					var graph = PXGraph.CreateInstance<CCCustomerInformationManagerGraph>();
					PXResultset<CustomerPaymentMethodDetail> otherCards = graph.GetAllCustomersCardsInProcCenter(this, CustomerPaymentMethod.Current.BAccountID, 
						CustomerPaymentMethod.Current.CCProcessingCenterID);

					PXResult<CustomerPaymentMethodDetail> duplicate = otherCards.AsEnumerable()
						.FirstOrDefault(result => ((CustomerPaymentMethodDetail)result).Value == (string)e.NewValue);

					if (duplicate != null)
					{
						CustomerPaymentMethodDetail duplicateDet = duplicate;
						CustomerPaymentMethod dublicateCPM = duplicate.GetItem<CustomerPaymentMethod>();

						throw new PXSetPropertyException(Messages.DuplicateCCProcessingID, row.Value, dublicateCPM.Descr);
					}
				}
			}
		}

		protected virtual void CustomerPaymentMethodDetail_Value_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethodDetail row = e.Row as CustomerPaymentMethodDetail;
			PaymentMethodDetail def = this.FindTemplate(row);
			if (def != null)
			{
				if (def.IsIdentifier == true)
				{
					bool isTokenized = CCProcessingHelper.IsTokenizedPaymentMethod(this, CustomerPaymentMethod.Current.PMInstanceID, true);
					string id;
					if (isTokenized)
					{
						id = CCDisplayMaskService.UseAdjustedDisplayMaskForCardNumber(row.Value, def.DisplayMask);
					}
					else
					{
						id = CCDisplayMaskService.UseDisplayMaskForCardNumber(row.Value, def.DisplayMask);
					}

					if (!this.CustomerPaymentMethod.Current.Descr.Contains(id))
					{
						CustomerPaymentMethod parent = this.CustomerPaymentMethod.Current;
						this.CustomerPaymentMethod.Cache.SetValueExt<CustomerPaymentMethod.descr>(parent, FormatDescription(parent.CardType ?? CardType.OtherCode, id));
						this.CustomerPaymentMethod.Update(parent);
					}
				}

				if ((def.IsExpirationDate ?? false) && !String.IsNullOrEmpty(row.Value))
				{
					CustomerPaymentMethod parent = this.CustomerPaymentMethod.Current;
					this.CustomerPaymentMethod.Cache.SetValueExt<CustomerPaymentMethod.expirationDate>(parent, CustomerPaymentMethodMaint.ParseExpiryDate(this, parent, row.Value));
					this.CustomerPaymentMethod.Cache.SetValueExt<CustomerPaymentMethod.lastNotificationDate>(parent, null);
					this.CustomerPaymentMethod.Update(parent);
				}				
			}
		}


		#endregion

		#region Address & Contact Events
		protected virtual void Address_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Address row = (Address)e.Row;
			bool enabled = false;
			if (row != null)
			{
				CustomerPaymentMethod parent = this.CustomerPaymentMethod.Current;
				if (parent != null && parent.BillAddressID == row.AddressID && parent.IsBillAddressSameAsMain == false)
				{
					enabled = true;
				}
			}
			PXUIFieldAttribute.SetEnabled(cache, row, null, enabled);
		}

		protected virtual void Contact_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Contact row = (Contact)e.Row;
			bool enabled = false;
			if (row != null)
			{
				CustomerPaymentMethod parent = this.CustomerPaymentMethod.Current;
				if (parent != null && parent.BillContactID == row.ContactID
					&& parent.IsBillContactSameAsMain == false)
				{
					enabled = true;
				}
			}
			PXUIFieldAttribute.SetEnabled(cache, row, null, enabled);
		}
		#endregion

		#region Internal Functions
		protected virtual PaymentMethodDetail FindTemplate(CustomerPaymentMethodDetail aDet)
		{
			PaymentMethodDetail res = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
				And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
				And<PaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>>>>>.Select(this, aDet.PaymentMethodID, aDet.DetailID);
			return res;
		}
		protected virtual void ClearDetails()
		{
			foreach (CustomerPaymentMethodDetail iDet in this.DetailsAll.Select())
			{
				this.DetailsAll.Delete(iDet);
			}
		}
		protected virtual void AddDetails(string aPaymentMethodID)
		{
			if (!String.IsNullOrEmpty(aPaymentMethodID))
			{
				foreach (PaymentMethodDetail it in this.PMDetails.Select(aPaymentMethodID))
				{
					if(SkipPaymentProfileDetail(it))
					{
						continue;
					}

					CustomerPaymentMethodDetail det = new CustomerPaymentMethodDetail();
					det.DetailID = it.DetailID;
					det = this.Details.Insert(det);
				}
			}
		}

		protected virtual void MergeDetailsWithDefinition(CustomerPaymentMethod row)
		{
			string aPaymentMethod = row.PaymentMethodID;
			if (aPaymentMethod != this.mergedPaymentMethod)
			{
				List<PaymentMethodDetail> toAdd = new List<PaymentMethodDetail>();
				foreach (PaymentMethodDetail it in this.PMDetails.Select(aPaymentMethod))
				{
					if (SkipPaymentProfileDetail(it))
					{
						continue;
					}

					CustomerPaymentMethodDetail detail = null;
					foreach (CustomerPaymentMethodDetail iPDet in this.Details.Select())
					{
						if (iPDet.DetailID == it.DetailID)
						{
							detail = iPDet;
							break;
						}
					}
					if (detail == null && !(it.DetailID == CreditCardAttributes.CVV && row.CVVVerifyTran != null))
					{
						toAdd.Add(it);
					}
				}
				using (ReadOnlyScope rs = new ReadOnlyScope(this.Details.Cache))
				{
					foreach (PaymentMethodDetail it in toAdd)
					{
						CustomerPaymentMethodDetail detail = new CustomerPaymentMethodDetail();
						detail.DetailID = it.DetailID;
						detail = this.Details.Insert(detail);
					}
					if (toAdd.Count > 0)
					{
						this.Details.View.RequestRefresh();
					}
				}
				this.mergedPaymentMethod = aPaymentMethod;
			}
		}

		protected virtual bool SkipPaymentProfileDetail(PaymentMethodDetail it)
		{
			PaymentMethod pmDef = this.PaymentMethodDef.Select(it.PaymentMethodID);
			bool isCCPaymentMethod = pmDef.PaymentType == PaymentMethodType.CreditCard && pmDef.ARIsProcessingRequired == true && pmDef.UseForAR == true;
			bool result = !PXAccess.FeatureInstalled<CS.FeaturesSet.integratedCardProcessing>() && it.IsCCProcessingID == true && isCCPaymentMethod;
			return result;
		}
		#endregion


		#region Utilities
		public static string FormatDescription(string cardType, string aMaskedID)
		{
			return String.Format("{0}:{1}", CardType.GetDisplayName(cardType), aMaskedID);
		}
		public static DateTime? ParseExpiryDate(PXGraph graph, CustomerPaymentMethod cpm, string aValue)
		{
			return ParseExpiryDate(graph,(ICCPaymentProfile)cpm,aValue);
		}

		public static DateTime? ParseExpiryDate(PXGraph graph, ICCPaymentProfile cpm, string aValue)
		{
			return ParseExpiryDate(graph, cpm?.CCProcessingCenterID, aValue);
		}

		public static DateTime? ParseExpiryDate(PXGraph graph, string procCenterId, string value)
		{
			DateTime datetime;
			string dateStringFormat = null;
			if (graph != null && procCenterId != null)
			{
				dateStringFormat = CCProcessingHelper.GetExpirationDateFormat(graph, procCenterId);
			}

			bool res = false;
			if (!string.IsNullOrEmpty(dateStringFormat))
			{
				res = DateTime.TryParseExact(value, dateStringFormat, null, DateTimeStyles.None, out datetime);
			}
			else
			{
				res = DateTime.TryParseExact(value, "Myyyy", null, DateTimeStyles.None, out datetime)
					|| DateTime.TryParseExact(value, "Myy", null, DateTimeStyles.None, out datetime);
			}
			if (!res)
			{
				return null;
			}
			return datetime;
		}

		[Obsolete]
		public static class IDObfuscator
		{
			private const char CS_UNDERSCORE = '_';
			private const char CS_DASH = '-';
			private const char CS_DOT = '.';
			private const char CS_MASKER = '*';
			private const char CS_NUMBER_MASK_0 = '#';
			private const char CS_NUMBER_MASK_1 = '0';
			private const char CS_NUMBER_MASK_2 = '9';
			private const char CS_ANY_CHAR_0 = '&';
			private const char CS_ANY_CHAR_1 = 'C';
			private const char CS_ALPHANUMBER_MASK_0 = 'a';
			private const char CS_ALPHANUMBER_MASK_1 = 'A';
			private const char CS_ALPHA_MASK_0 = 'L';
			private const char CS_ALPHA_MASK_1 = '?';

			[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
			public static string MaskID(string aID, string aEditMask, string aDisplayMask)
			{
				if (string.IsNullOrEmpty(aID) || string.IsNullOrWhiteSpace(aDisplayMask)) return aID;
				if (!string.IsNullOrEmpty(aEditMask))
				{
					int mskLength = aEditMask.Length;
					int displayMskLength = aDisplayMask.Length;
					int valueLength = aID.Length;
					char[] entryMask = aEditMask.ToCharArray();
					char[] displayMask = aDisplayMask.ToCharArray();
					char[] value = aID.ToCharArray();
					int valueIndex = 0;
					int displayMaskIndex = 0;
					StringBuilder res = new StringBuilder(mskLength);
					for (int i = 0; i < mskLength; i++)
					{
						if (valueIndex >= valueLength) break;
						if (displayMaskIndex >= displayMskLength)
						{
							res.Append(CS_MASKER);
						}
						else
						{
							if (IsSymbol(entryMask[i]))
							{
								if (IsSymbol(displayMask[displayMaskIndex]))
									res.Append(value[valueIndex]);
								else
									res.Append(CS_MASKER);
								valueIndex++;
								displayMaskIndex++;
							}
							else
							{
								if (IsSeparator(entryMask[i]) && IsSeparator(displayMask[displayMaskIndex]))
								{
									res.Append(displayMask[displayMaskIndex]);
									displayMaskIndex++;
								}
								//Any other characters are omited
							}
						}
					}
					return res.ToString();
				}
				return MaskID(aID, aDisplayMask);
			}

			[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
			public static string MaskID(string aID, string aDisplayMask)
			{
				if (string.IsNullOrEmpty(aID) || string.IsNullOrEmpty(aDisplayMask) || string.IsNullOrEmpty(aDisplayMask.Trim())) return aID;
				int mskLength = aDisplayMask.Length;
				int valueLength = aID.Length;
				char[] displayMask = aDisplayMask.ToCharArray();
				char[] value = aID.ToCharArray();
				int valueIndex = 0;
				StringBuilder res = new StringBuilder(mskLength);
				for (int i = 0; i < mskLength; i++)
				{
					if (valueIndex >= valueLength) break;
					if (IsSymbol(displayMask[i]))
					{
						res.Append(value[valueIndex]);
						valueIndex++;
					}
					else
					{
						//Any other characters are treated as separator and are omited
						if (IsSeparator(displayMask[i]))
						{
							res.Append(displayMask[i]);
						}
						else
						{
							res.Append(CS_MASKER);
							valueIndex++;
						}
					}
				}
				return res.ToString();
			}

			[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
			public static string AdjustedMaskID(string aID, string aDisplayMask)
			{
				string adjustedID = aID;
				int maskSymbolsLength = aDisplayMask.ToArray().Where(symbol => !IsSeparator(symbol)).Count();
				if (aID.Length > maskSymbolsLength)
				{
					adjustedID = aID.Substring(aID.Length - maskSymbolsLength);
				}
				else if (aID.Length < maskSymbolsLength)
				{
					adjustedID = aID.PadLeft(maskSymbolsLength, '0');
				}

				return MaskID(adjustedID, aDisplayMask);
			}

		    public static string GetMaskByID(CCPaymentHelperGraph graph, string aID, string aDisplayMask, int? PMInstanceID)
		    {
                if (CCProcessingHelper.IsTokenizedPaymentMethod(graph, PMInstanceID, true))
                {
                    return graph.CCDisplayMaskService.UseAdjustedDisplayMaskForCardNumber(aID, aDisplayMask);
                }

                return graph.CCDisplayMaskService.UseDisplayMaskForCardNumber(aID, aDisplayMask);
            }

			//This Function is intended to restore value to masked if Displayed Mask was Used as Entry Mask. 
			//Mask Separator characters  may  be removed (optionally).

			public static string RestoreToMasked(string aValue, string aDisplayMask, string aMissedValuePlaceholder, bool aRemoveSeparators)
			{
				return RestoreToMasked(aValue, aDisplayMask, aMissedValuePlaceholder, aRemoveSeparators, false, CS_MASKER.ToString(), false);
			}

			public static string RestoreToMasked(string aValue, string aDisplayMask, string aMissedValuePlaceholder, bool aSkipSeparators, bool aReplaceMaskChars, string aNewMasker, bool aMergeNonValue)
			{
				char[] displayMask = aDisplayMask != null ? aDisplayMask.ToCharArray() : string.Empty.ToCharArray();
				char[] value = aValue != null ? aValue.ToCharArray() : string.Empty.ToCharArray();
				int valueIndex = 0;
				int mskLength = aDisplayMask != null ? aDisplayMask.Length : 0;
				int valueLength = aValue != null ? aValue.Length : 0;
				StringBuilder res = new StringBuilder(mskLength);
				bool lastAddedIsValue = false;
				for (int i = 0; i < mskLength; i++)
				{
					//if (valueIndex >= valueLength) break;
					if (IsSymbol(displayMask[i]))
					{
						if (valueIndex < valueLength && !Char.IsWhiteSpace(value[valueIndex]))
						{
							res.Append(value[valueIndex]);
						}
						else
						{
							res.Append(aMissedValuePlaceholder);
						}
						valueIndex++;
						lastAddedIsValue = true;
					}
					else
					{
						if (!aSkipSeparators || IsMasked(displayMask[i]))
						{
							if (aReplaceMaskChars)
							{
								if (!aMergeNonValue || lastAddedIsValue)
									res.Append(aNewMasker);
							}
							else
							{
								res.Append(displayMask[i]);
							}
							lastAddedIsValue = false;
						}
					}
				}
				return res.ToString();
			}

			private static bool IsSeparator(char aCh)
			{
				return aCh == CS_UNDERSCORE || aCh == CS_DASH || aCh == CS_DOT;
			}
			private static bool IsMasked(char aCh)
			{
				return aCh == CS_MASKER;
			}
			private static bool IsSymbol(char aCh)
			{
				switch (aCh)
				{
					case CS_NUMBER_MASK_0:
					case CS_NUMBER_MASK_1:
					case CS_NUMBER_MASK_2:
					case CS_ALPHANUMBER_MASK_0:
					case CS_ALPHANUMBER_MASK_1:
					case CS_ANY_CHAR_0:
					case CS_ANY_CHAR_1:
					case CS_ALPHA_MASK_0:
					case CS_ALPHA_MASK_1:
						return true;
				}
				return false;
			}


		}
		#endregion

		public class PaymentProfileHostedForm : Extensions.PaymentProfile.PaymentProfileGraph<CustomerPaymentMethodMaint, CustomerPaymentMethod>
		{
			protected override CustomerPaymentMethodMapping GetCustomerPaymentMethodMapping()
			{
				return new CustomerPaymentMethodMapping(typeof(CustomerPaymentMethod));
			}

			protected override CustomerPaymentMethodDetailMapping GetCusotmerPaymentMethodDetailMapping()
			{
				return new CustomerPaymentMethodDetailMapping(typeof(AR.CustomerPaymentMethodDetail));
			}

			protected override PaymentmethodDetailMapping GetPaymentMethodDetailMapping()
			{
				return new PaymentmethodDetailMapping(typeof(CA.PaymentMethodDetail));
			}

			protected override void RowSelected(Events.RowSelected<CustomerPaymentMethod> e)
			{
				base.RowSelected(e);
				CustomerPaymentMethod row = e.Row;
				if (row == null)
					return;
				int? pmInstance = CustomerPaymentMethod.Current.PMInstanceID;
				bool isHFPM = CCProcessingHelper.IsHFPaymentMethod(this.Base, pmInstance, false);
				bool isIDFilled = CCProcessingHelper.IsCCPIDFilled(this.Base, pmInstance);
				this.RefreshCreatePaymentAction(isHFPM && !isIDFilled, isHFPM);
				this.RefreshSyncPaymentAction(true);
				this.RefreshManagePaymentAction(isHFPM && !string.IsNullOrEmpty(row.Descr), isHFPM);
			}

			protected override void MapViews(CustomerPaymentMethodMaint graph)
			{
				var cpmDetails = graph.DetailsAll;
				this.CustomerPaymentMethodDetail = new PXSelectExtension<Extensions.PaymentProfile.CustomerPaymentMethodDetail>(cpmDetails);
			}
		}

		#region Private Functions
		private int? bAccountID;
		private string mergedPaymentMethod;

		private string GetReferentialIntegrityViolationMessage(CustomerPaymentMethod row, ARPayment payment)
		{
			string parentRecordInfo = GetRecordInfo(row);
			string childRecordInfo = GetRecordInfo(payment);

			string msg = PXLocalizer.LocalizeFormat(Messages.EntityCannotBeDeletedBecauseOfOneRefRecord, parentRecordInfo, childRecordInfo);
			return msg;
		}

		private string GetRecordInfo(object entity)
		{
			string cacheName = EntityHelper.GetFriendlyEntityName(entity.GetType(), entity);
			EntityHelper _entityHelper = new EntityHelper(this);
			string info = $"{cacheName} ({_entityHelper.GetEntityRowID(entity.GetType(), entity, ", ")})";
			return info;
		}

		#endregion

		#region Address Lookup Extension
		/// <exclude/>
		public class CustomerPaymentMethodMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<CustomerPaymentMethodMaint, CustomerPaymentMethod, Address>
		{
			protected override string AddressView => nameof(Base.BillAddress);
			protected override string ViewOnMap => nameof(Base.viewBillAddressOnMap);
		}
		#endregion
	}
}
