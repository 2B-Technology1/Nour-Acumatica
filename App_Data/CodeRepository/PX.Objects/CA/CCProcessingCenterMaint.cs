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

using PX.CCProcessingBase;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Specific;
using PX.Objects.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.CA
{
	public class CCProcessingCenterMaint : PXGraph<CCProcessingCenterMaint, CCProcessingCenter>, IProcessingCenterSettingsStorage
	{
		private Dictionary<string, string> _displayNamesAndTypes;
		public virtual Dictionary<string, string> DisplayNamesAndTypes => _displayNamesAndTypes;
		public PXSelect<CCProcessingCenter> ProcessingCenter;
		public PXSelect<CCProcessingCenter, Where<CCProcessingCenter.processingCenterID, 
			Equal<Current<CCProcessingCenter.processingCenterID>>>> CurrentProcessingCenter;
		public PXSelect<CCProcessingCenterDetail, Where<CCProcessingCenterDetail.processingCenterID, Equal<Current<CCProcessingCenter.processingCenterID>>>> Details;

		public PXSelect<CCProcessingCenterPmntMethod, Where<CCProcessingCenterPmntMethod.processingCenterID, Equal<Current<CCProcessingCenter.processingCenterID>>>> PaymentMethods;
		public PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<CCProcessingCenter.cashAccountID>>>> CashAccount;
		public PXSelect<CCProcessingCenterFeeType, Where<CCProcessingCenterFeeType.processingCenterID, Equal<Current<CCProcessingCenter.processingCenterID>>>> FeeTypes;

		public PXAction<CCProcessingCenter> testCredentials;
		[PXUIField(DisplayName = "Test Credentials", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable TestCredentials(PXAdapter adapter)
		{
			CCProcessingCenter row = this.ProcessingCenter.Current;
			if (string.IsNullOrEmpty(row.ProcessingCenterID))
			{
				throw new PXException(Messages.ProcessingCenterNotSelected);
			}
			if(string.IsNullOrEmpty(row.ProcessingTypeName))
			{
				throw new PXException(Messages.ProcessingPluginNotSelected);
			}
			var graph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
			if (graph.TestCredentials(this, row.ProcessingCenterID))
			{
				ProcessingCenter.Ask(Messages.Result,Messages.CredentialsAccepted, MessageButtons.OK);
			}
			return adapter.Get();
		}

		public PXAction<CCProcessingCenter> updateExpirationDate;
		[PXUIField(DisplayName = "Update Expiration Dates", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable UpdateExpirationDate(PXAdapter adapter)
		{
			if (ProcessingCenter.Current != null)
			{
				CCProcessingCenter row = ProcessingCenter.Current;
				CCUpdateExpirationDatesProcess updateGraph = PXGraph.CreateInstance<CCUpdateExpirationDatesProcess>();
				updateGraph.Filter.Current = new CCUpdateExpirationDatesProcess.CPMFilter() { ProcessingCenterID = row.ProcessingCenterID };
				throw new PXRedirectRequiredException(updateGraph, true, "UpdateExpirationDate") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public CCProcessingCenterMaint()
		{
			GL.GLSetup setup = GLSetup.Current;
			this.ProcessingCenter.Cache.AllowDelete = false;
		    PaymentMethods.Cache.AllowInsert = PaymentMethods.Cache.AllowUpdate = PaymentMethods.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled<CCProcessingCenterPmntMethod.isDefault>(this.PaymentMethods.Cache, null, false);
			_displayNamesAndTypes = new Dictionary<string, string>();
			SetDisplayNamesAndTypes();
		}

		protected virtual void SetDisplayNamesAndTypes()
		{
			var recs = PXCCPluginTypeSelectorAttribute.GetPluginRecs();
			foreach (var rec in recs)
			{
				if (rec.TypeName != rec.DisplayTypeName)
				{
					_displayNamesAndTypes.Add(rec.DisplayTypeName, rec.TypeName);
				}
			}
		}

		public PXSetup<GL.GLSetup> GLSetup;
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		protected virtual void CCProcessingCenterPmntMethod_ProcessingCenterID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void CCProcessingCenter_ProcessingTypeName_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			CCProcessingCenter row = e.Row as CCProcessingCenter;
			if (row == null) return;
			if (row.ProcessingTypeName != null)
			{
				if (ProcessingCenter.Ask(Messages.ResetDetailsToDefault, MessageButtons.OKCancel) == WebDialogResult.Cancel)
				{
					e.Cancel = true;
					return;
				}
			}
		}

		protected virtual void CCProcessingCenter_ProcessingTypeName_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			CCProcessingCenter row = e.Row as CCProcessingCenter;
			if (row == null)
			{
				return;
			}

			if (row.ProcessingTypeName != null)
			{
				foreach (KeyValuePair<string, string> pair in DisplayNamesAndTypes)
				{
					if (pair.Value.Equals(row.ProcessingTypeName))
					{
						e.ReturnValue = pair.Key;
						return;
					}
					
				}
			}
		}

		protected virtual void CCProcessingCenter_ProcessingTypeName_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			CCProcessingCenter row = e.Row as CCProcessingCenter;
			if (row == null)
			{
				return;
			}
			if (e.NewValue == null)
			{
				return;
			}

			if (this.DisplayNamesAndTypes.ContainsKey(e.NewValue.ToString()))
			{
				e.NewValue = this.DisplayNamesAndTypes[e.NewValue.ToString()];
			}

			CCPluginTypeHelper.ThrowIfProcCenterFeatureDisabled((string)e.NewValue);
		}
		protected virtual void CCProcessingCenter_CreateAdditionalCustomerProfile_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CCProcessingCenter row = e.Row as CCProcessingCenter;
			if (row == null) return;
			if (!CCProcessingFeatureHelper.IsFeatureSupported(row, CCProcessingFeature.ExtendedProfileManagement, true))
			{
				string errorMessage = PXMessages.LocalizeFormatNoPrefixNLA(
					AR.Messages.FeatureNotSupportedByProcessing,
					CCProcessingFeature.ExtendedProfileManagement);
				throw new PXException(errorMessage);
			}
			if (row.CreateAdditionalCustomerProfiles == true && row.CreditCardLimit == null)
			{
				cache.SetDefaultExt<CCProcessingCenter.creditCardLimit>(row);
			}
		}

		protected virtual void CCProcessingCenter_ProcessingTypeName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CCProcessingCenter row = e.Row as CCProcessingCenter;
			if (row == null) return;
			foreach (CCProcessingCenterDetail detail in Details.Select())
			{
				Details.Delete(detail);
			}
			this.ImportSettings();

			bool supported = CCProcessingFeatureHelper.IsPaymentHostedFormSupported(row);
			if (supported)
			{
				sender.RaiseExceptionHandling<CCProcessingCenter.useAcceptPaymentForm>(ProcessingCenter.Current, null,
					new PXSetPropertyException(Messages.UseAcceptPaymentFormWarning, PXErrorLevel.Warning));
				row.UseAcceptPaymentForm = true;
			}
		}

		protected virtual void _(Events.FieldUpdated<CCProcessingCenter, CCProcessingCenter.cashAccountID> e)
		{
			var oldCashAccount = CA.CashAccount.PK.Find(this, e.OldValue as int?);
			var newCashAccount = CA.CashAccount.PK.Find(this, e.NewValue as int?);
			if (oldCashAccount?.CuryID != newCashAccount?.CuryID)
			{
				e.Cache.SetValueExt<CCProcessingCenter.depositAccountID>(e.Row, null);
				UIState.RaiseOrHideErrorByErrorLevelPriority<CCProcessingCenter.depositAccountID>(e.Cache, e.Row, e.Row.ImportSettlementBatches == true, Messages.SelectAccountWithTheSameCurrencyAsCashAccount, PXErrorLevel.Warning);
			}
		}

		protected virtual void CCProcessingCenter_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			CCProcessingCenter row = e.Row as CCProcessingCenter;
			if (row == null) return;
			using (PXConnectionScope scope = new PXConnectionScope())
			{
				row.NeedsExpDateUpdate = CCProcessingHelper.CCProcessingCenterNeedsExpDateUpdate(this, row);
			}
		}
		
		protected virtual void CCProcessingCenter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CCProcessingCenter row = e.Row as CCProcessingCenter;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<CCProcessingCenter.processingTypeName>(sender, row, row.IsActive.GetValueOrDefault());
			PXUIFieldAttribute.SetRequired<CCProcessingCenter.processingTypeName>(sender, row.IsActive.GetValueOrDefault());
			bool isTokenized = CCProcessingFeatureHelper.IsFeatureSupported(row, CCProcessingFeature.HostedForm)
				|| CCProcessingFeatureHelper.IsFeatureSupported(row, CCProcessingFeature.ProfileForm);
			bool profileManagementSupported = CCProcessingFeatureHelper.IsFeatureSupported(row, CCProcessingFeature.ExtendedProfileManagement);
			bool hasProcTypeName = row.ProcessingTypeName != null; 
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.allowDirectInput>(sender, row, isTokenized);
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.syncronizeDeletion>(sender, row, isTokenized);
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.syncRetryAttemptsNo>(sender, row, isTokenized);
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.syncRetryDelayMs>(sender, row, isTokenized);
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.allowSaveProfile>(sender, row, profileManagementSupported);
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.allowUnlinkedRefund>(sender, row, hasProcTypeName);
			bool supportsExtendedProfiles = isTokenized && CCProcessingFeatureHelper.IsFeatureSupported(row, CCProcessingFeature.ExtendedProfileManagement);
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.createAdditionalCustomerProfiles>(sender, row, supportsExtendedProfiles);
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.creditCardLimit>(sender, row, 
				supportsExtendedProfiles && row.CreateAdditionalCustomerProfiles == true);

			bool importSettlementBatches = row.ImportSettlementBatches == true;
			PXUIFieldAttribute.SetEnabled<CCProcessingCenter.importStartDate>(sender, row, importSettlementBatches);
			PXUIFieldAttribute.SetEnabled<CCProcessingCenter.depositAccountID>(sender, row, importSettlementBatches);
			PXUIFieldAttribute.SetEnabled<CCProcessingCenter.autoCreateBankDeposit>(sender, row, importSettlementBatches);
			PXDefaultAttribute.SetPersistingCheck<CCProcessingCenter.importStartDate>(sender, e.Row, importSettlementBatches ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CCProcessingCenter.depositAccountID>(sender, e.Row, importSettlementBatches ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			this.testCredentials.SetEnabled(!string.IsNullOrEmpty(row.ProcessingCenterID) && !string.IsNullOrEmpty(row.ProcessingTypeName));
			this.updateExpirationDate.SetVisible(row.NeedsExpDateUpdate == true);
			this.ProcessingCenter.Cache.AllowDelete = !HasTransactions(row);
			PaymentMethods.AllowInsert = false;
			PaymentMethods.AllowDelete = false;
			SetAllowAcceptFormCheckbox(sender, row);

			CheckUsingDirectInputMode(sender, e);
			CheckBatchImportSettings(sender, row);
			ShowAllowSaveCardCheckboxWarnIfNeeded(sender, row);
			ShowAllowUnlinkedRefundWarnIfNeeded(sender, row);
			bool isProcCenterExternalAuthorizationOnly = row.IsExternalAuthorizationOnly == true;
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.reauthRetryNbr>(sender, row, isProcCenterExternalAuthorizationOnly == false);
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.reauthRetryDelay>(sender, row, isProcCenterExternalAuthorizationOnly == false);
			PXUIFieldAttribute.SetVisible<CCProcessingCenterPmntMethod.reauthDelay>(this.PaymentMethods.Cache, null, isProcCenterExternalAuthorizationOnly == false);
			PXUIFieldAttribute.SetEnabled<CCProcessingCenter.useAcceptPaymentForm>(sender, row, isProcCenterExternalAuthorizationOnly == false);

			UIState.RaiseOrHideErrorByErrorLevelPriority<CCProcessingCenter.processingTypeName>(sender, row, isProcCenterExternalAuthorizationOnly,
								Messages.ProcCenterIsExternalAuthorizationOnly, PXErrorLevel.Warning, row?.ProcessingCenterID);
		}

		protected virtual void CCProcessingCenter_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CCProcessingCenter row = e.Row as CCProcessingCenter;
			if (row == null) return;
		
			if (e.Operation == PXDBOperation.Insert)
			{
				if (CheckUseAimPlugin(row))
				{
					throw new PXRowPersistingException(typeof(CCProcessingCenter.processingTypeName).Name, null, Messages.ProcessingCenterAimPluginNotAllowed);
				}
				if (row.AllowDirectInput == true)
				{
					throw new PXRowPersistingException(nameof(CCProcessingCenter.allowDirectInput), null, Messages.DiscontinuedDirectInputModeNotAllowed);
				}
			}

			if (e.Operation == PXDBOperation.Update)
			{
				var query = new PXSelectReadonly<CCProcessingCenter, Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>(this);
				CCProcessingCenter res = query.SelectSingle(row.ProcessingCenterID);

				if (res?.ProcessingTypeName != row.ProcessingTypeName)
				{
					if (CheckUseAimPlugin(row))
					{
						throw new PXRowPersistingException(typeof(CCProcessingCenter.processingTypeName).Name, null, Messages.ProcessingCenterAimPluginNotAllowed);
					}
				}
				if (row.AllowDirectInput == true)
				{
					throw new PXRowPersistingException(nameof(CCProcessingCenter.allowDirectInput), null, Messages.DiscontinuedDirectInputModeNotAllowed);
				}
			}

			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete && row.IsActive.GetValueOrDefault() && string.IsNullOrEmpty(row.ProcessingTypeName))
			{
				if (sender.RaiseExceptionHandling<CCProcessingCenter.processingTypeName>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CCProcessingCenter.processingTypeName).Name)))
				{
					throw new PXRowPersistingException(typeof(CCProcessingCenter.processingTypeName).Name, null, ErrorMessages.FieldIsEmpty, typeof(CCProcessingCenter.processingTypeName).Name);
				}
			}
		}

		private static bool CheckUseAimPlugin(CCProcessingCenter processingCenter)
		{
			bool ret = false;
			string selProcType = processingCenter.ProcessingTypeName;
			string type = AuthnetConstants.AIMPluginFullName;
			if (selProcType == type)
			{
				ret = true;
			}
			return ret;
		}

		protected virtual void CCProcessingCenterDetail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<CCProcessingCenterDetail.detailID>(cache, e.Row, false);
			PXUIFieldAttribute.SetEnabled<CCProcessingCenterDetail.descr>(cache, e.Row, false);
			CCProcessingCenterDetail row = e.Row as CCProcessingCenterDetail;
			if (row != null)
			{
				if (row.IsEncryptionRequired == true && row.IsEncrypted != true && Details.Cache.GetStatus(row) == PXEntryStatus.Notchanged)
				{
					Details.Cache.SetStatus(row, PXEntryStatus.Updated);
					Details.Cache.IsDirty = true;
					PXUIFieldAttribute.SetWarning<CCProcessingCenterDetail.value>(Details.Cache, row, Messages.CryptoSettingsChanged);
				}
			}
		}
		
		private bool errorKey;

		protected virtual void CCProcessingCenterDetail_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			if (errorKey)
			{
				errorKey = false;
				e.Cancel = true;
			}
			else
			{
				string detID = ((CCProcessingCenterDetail)e.Row).DetailID;				
				bool isExist = false;
				foreach (CCProcessingCenterDetail it in this.Details.Select())
				{
					if (it.DetailID == detID)
					{
						isExist = true;
					}
				}

				if (isExist)
				{
					cache.RaiseExceptionHandling<CCProcessingCenterDetail.detailID>(e.Row, detID, new PXException(Messages.RowIsDuplicated));  //Messages.DuplicatedCCProcessingCenterDetail
					e.Cancel = true;
				}
			}
		}

		protected virtual void CCProcessingCenterDetail_DetailID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			CCProcessingCenterDetail a = e.Row as CCProcessingCenterDetail;
			if (a.DetailID != null)
			{
				errorKey = true;
			}
		}

		protected virtual void CCProcessingCenterPmntMethod_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CCProcessingCenterPmntMethod row = e.Row as CCProcessingCenterPmntMethod;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<CCProcessingCenterPmntMethod.paymentMethodID>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<CCProcessingCenterPmntMethod.isActive>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<CCProcessingCenterPmntMethod.isDefault>(sender, null, false);
		}

		protected virtual void CCProcessingCenterPmntMethod_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			if (errorKey)
			{
				errorKey = false;
				e.Cancel = true;
			}
			else
			{
				CCProcessingCenterPmntMethod row = e.Row as CCProcessingCenterPmntMethod;
				string detID = row.ProcessingCenterID;
				bool isExist = false;
				foreach (CCProcessingCenterPmntMethod it in this.PaymentMethods.Select())
				{
					if (!Object.ReferenceEquals(it, row) && (it.PaymentMethodID == row.PaymentMethodID) && (it.ProcessingCenterID == it.ProcessingCenterID) )
					{
						isExist = true;
					}
				}
				if (isExist)
				{
					cache.RaiseExceptionHandling<CCProcessingCenterPmntMethod.paymentMethodID>(e.Row, detID, new PXException(Messages.PaymentMethodIsAlreadyAssignedToTheProcessingCenter));
					e.Cancel = true;
				}
			}
		}

		protected virtual void _DepositAccountFieldUpdating(Events.FieldUpdating<CCProcessingCenter.depositAccountID> e)
		{
			if (IsCopyPasteContext)
			{
				CashAccount.Current = CashAccount.SelectSingle();
			}
		}

		protected virtual void CCProcessingCenterPmntMethod_ProcessingCenterID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			CCProcessingCenterPmntMethod row = e.Row as CCProcessingCenterPmntMethod;
			if (row.PaymentMethodID!= null)
			{
				errorKey = true;
			}
		}
		
		protected virtual bool HasTransactions(CCProcessingCenter aRow) 
		{
			CCProcTran tran = PXSelect<CCProcTran, Where<CCProcTran.processingCenterID, Equal<Required<CCProcTran.processingCenterID>>>,OrderBy<Desc<CCProcTran.tranNbr>>>
				.SelectWindowed(this, 0, 1, aRow.ProcessingCenterID);
			return (tran != null);
		}

		private void SetAllowAcceptFormCheckbox(PXCache cache, CCProcessingCenter processingCenter)
		{
			bool featureSupported = CCProcessingFeatureHelper.IsPaymentHostedFormSupported(processingCenter);

			PXUIFieldAttribute.SetEnabled<CCProcessingCenter.useAcceptPaymentForm>(cache, processingCenter, featureSupported);
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.useAcceptPaymentForm>(cache, processingCenter, featureSupported);
			if (!featureSupported)
			{
				cache.SetValueExt<CCProcessingCenter.useAcceptPaymentForm>(processingCenter, featureSupported);
			}
		}

		private void CheckUsingDirectInputMode(PXCache cache, PXRowSelectedEventArgs e)
		{
			CCProcessingCenter procCenter = e.Row as CCProcessingCenter;
			if (procCenter == null) return;
			bool allowDirectInput = procCenter.AllowDirectInput.GetValueOrDefault();
			if (allowDirectInput == true)
			{
				cache.RaiseExceptionHandling<CCProcessingCenter.allowDirectInput>(e.Row, allowDirectInput,
					new PXSetPropertyException(Messages.UseDiscontinuedDirectInputMode, PXErrorLevel.Warning));
			}
			else
			{
				cache.RaiseExceptionHandling<CCProcessingCenter.allowDirectInput>(e.Row, allowDirectInput, null);
			}
			PXUIFieldAttribute.SetEnabled<CCProcessingCenter.allowDirectInput>(cache, e.Row, allowDirectInput);
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.allowDirectInput>(cache, e.Row, allowDirectInput);
		}

		private void CheckBatchImportSettings(PXCache cache, CCProcessingCenter row)
		{
			bool importBatches = row.ImportSettlementBatches == true;
			const int warningThreshold = 31;

			bool startDateWarning = importBatches && row.ImportStartDate < row.LastSettlementDate && (DateTime.Today - row.LastSettlementDate).Value.Days > warningThreshold;
			UIState.RaiseOrHideErrorByErrorLevelPriority<CCProcessingCenter.importStartDate>(cache, row, startDateWarning, Messages.ConsiderSettingImportStartDate, PXErrorLevel.Warning, row.LastSettlementDate);
		}

		private void ShowAllowUnlinkedRefundWarnIfNeeded(PXCache cache, CCProcessingCenter processingCenter)
		{
			if (processingCenter?.AllowUnlinkedRefund == false || processingCenter?.ProcessingTypeName == null)
			{ 
				return;
			}
			cache.RaiseExceptionHandling<CCProcessingCenter.allowUnlinkedRefund>(ProcessingCenter.Current, null, 
				new PXSetPropertyException(Messages.UseAllowUnlinkedRefundWarning, PXErrorLevel.Warning));
		}

		private void ShowAllowSaveCardCheckboxWarnIfNeeded(PXCache cache, CCProcessingCenter processingCenter)
		{
			if (string.IsNullOrEmpty(processingCenter?.ProcessingTypeName))
				return;
			string warning = null;
			try
			{
				bool supported = CCProcessingFeatureHelper.IsFeatureSupported(processingCenter, CCProcessingFeature.TransactionGetter, true);
				if (!supported)
					warning = Messages.GettingTranDetailsByIdNotSupportedWarn;
			}
			catch (PXException pex)
			{
				warning = pex.Message;
			}
			if (warning !=null)
				cache.RaiseExceptionHandling<CCProcessingCenter.allowSaveProfile>(ProcessingCenter.Current, null, new PXSetPropertyException(warning, PXErrorLevel.Warning));
		}

		#region IProcessingCenterSettingsStorage Members

		public virtual void ReadSettings(Dictionary<string, CCProcessingCenterDetail> aSettings)
		{
			CCProcessingCenter row = this.ProcessingCenter.Current;
			foreach (CCProcessingCenterDetail iDet in this.Details.Select())
			{
				aSettings[iDet.DetailID] = iDet;
			}
		}

		public virtual void ReadSettings(Dictionary<string, string> aSettings)
		{
			CCProcessingCenter row = this.ProcessingCenter.Current;
			foreach (CCProcessingCenterDetail iDet in this.Details.Select())
			{
				aSettings[iDet.DetailID] = iDet.Value;
			}
		}

		protected virtual void ImportSettings()
		{
			CCProcessingCenter row = this.ProcessingCenter.Current;

			if (string.IsNullOrEmpty(row.ProcessingCenterID))
			{
				throw new PXException(Messages.ProcessingCenterIDIsRequiredForImport);
			}
			if (string.IsNullOrEmpty(row.ProcessingTypeName))
			{
				throw new PXException(Messages.ProcessingObjectTypeIsNotSpecified);
			}

			this.ImportSettingsFromPC();
		}

		private bool isExportingSettings;

		private void ImportSettingsFromPC()
		{
			CCProcessingCenter row = this.ProcessingCenter.Current;
			Dictionary<string, CCProcessingCenterDetail> currentSettings = new Dictionary<string,CCProcessingCenterDetail>();
			ReadSettings(currentSettings);
			var graph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
			IList<PluginSettingDetail> processorSettings = graph.ExportSettings(this, row.ProcessingCenterID);
			isExportingSettings = true;
			foreach (PluginSettingDetail it in processorSettings)
			{
				if (!currentSettings.ContainsKey(it.DetailID))
				{
					CCProcessingCenterDetail detail = new CCProcessingCenterDetail();
					CCProcessingCenterDetail.Copy(it,detail);
					detail = this.Details.Insert(detail);
				}
				else
				{
					CCProcessingCenterDetail detail = currentSettings[it.DetailID];
					CCProcessingCenterDetail.Copy(it,detail);
					detail = this.Details.Update(detail);
				}
			}
			isExportingSettings = false;
		}

		#endregion

		protected virtual void CCProcessingCenterDetail_Value_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			CCProcessingCenterDetail row = e.Row as CCProcessingCenterDetail;
			if (row != null)
			{
				string fieldName = typeof(CCProcessingCenterDetail.value).Name;

				switch (row.ControlType)
				{
					case ControlTypeDefintion.Combo:
						List<string> labels = new List<string>();
						List<string> values = new List<string>();
						foreach (KeyValuePair<string, string> kv in row.ComboValuesCollection)
						{
							values.Add(kv.Key);
							labels.Add(kv.Value);
						}
						e.ReturnState = PXStringState.CreateInstance(e.ReturnState, CCProcessingCenterDetail.ValueFieldLength, null, fieldName, false, 1, null,
																values.ToArray(), labels.ToArray(), true, null);
						break;
					case ControlTypeDefintion.CheckBox:
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(Boolean), false, null, -1, null, null, null, fieldName,
								null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
						break;
					
					case ControlTypeDefintion.Password:
						//handled by PXRSACryptStringWithConditional attribute
						break;
					default:
						break;
				}
			}
		}

		protected virtual void CCProcessingCenterDetail_Value_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (isExportingSettings) return;

			CCProcessingCenter processingCenter = ProcessingCenter.Current;
			CCProcessingCenterDetail procCenterDetail = (CCProcessingCenterDetail)e.Row;
			//skip validation for special values - plugins don't know about this detail ids
			if (!InterfaceConstants.SpecialDetailIDs.Contains(procCenterDetail.DetailID))
			{
				var graph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				PluginSettingDetail detail = new PluginSettingDetail() { DetailID = procCenterDetail.DetailID, Value = (string)e.NewValue };
				graph.ValidateSettings(this, processingCenter.ProcessingCenterID, detail);
			}
		}
	}
}
