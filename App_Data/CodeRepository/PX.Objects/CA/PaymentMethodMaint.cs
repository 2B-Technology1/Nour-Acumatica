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
using System.Linq;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Attributes;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AP;
using PX.Api;
using PX.Objects.CA.Descriptor;
using System.Collections.Generic;
using PX.Common;
using System.Web.Compilation;
using PX.ACHPlugInBase;
using PX.Metadata;

namespace PX.Objects.CA
{
	public class PaymentMethodMaint : PXGraph<PaymentMethodMaint, PaymentMethod>
	{
		[InjectDependency]
		protected DirectDepositTypeService DirectDepositService { get; set; }

		#region Type Override events
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault()]
		[CCProcessingCenterPaymentMethodFilter(typeof(Search<CCProcessingCenter.processingCenterID, Where<CCProcessingCenter.isActive, Equal<True>>>))]
		[PXParent(typeof(Select<CCProcessingCenter, Where<CCProcessingCenter.processingCenterID, Equal<Current<CCProcessingCenterPmntMethod.processingCenterID>>>>))]
		[PXUIField(DisplayName = "Proc. Center ID")]
		[DeprecatedProcessing(ChckVal = DeprecatedProcessingAttribute.CheckVal.ProcessingCenterId)]
		[DisabledProcCenter(CheckFieldValue = DisabledProcCenterAttribute.CheckFieldVal.ProcessingCenterId)]
		protected virtual void CCProcessingCenterPmntMethod_ProcessingCenterID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(CA.PaymentMethod.paymentMethodID))]
		[PXSelector(typeof(Search<CA.PaymentMethod.paymentMethodID>))]
		[PXUIField(DisplayName = "Payment Method")]
		[PXParent(typeof(Select<CA.PaymentMethod, Where<CA.PaymentMethod.paymentMethodID, Equal<Current<CCProcessingCenterPmntMethod.paymentMethodID>>>>))]
		protected virtual void CCProcessingCenterPmntMethod_PaymentMethodID_CacheAttached(PXCache sender)
		{
		}

		/// <summary>
		/// Overriding CashAccount attribute of the DAC <see cref="PaymentMethodAccount.CashAccountID"/> property to suppress CashAccount active property verification by PXRestrictor attribute
		/// which works incorrectly on key fields. Instead in this graph we use verification of <see cref="CashAccount.Active"/> in events.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CashAccount(suppressActiveVerification: true, IsKey = true, DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible,
					 DescriptionField = typeof(CashAccount.descr))]
		protected virtual void PaymentMethodAccount_CashAccountID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region Public Selects

		public PXSelect<PaymentMethod> PaymentMethod;
		public PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>
			> PaymentMethodCurrent;
		[PXCopyPasteHiddenFields(typeof(PaymentMethodDetail.paymentMethodID))]
		public PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>> Details;

		[PXCopyPasteHiddenFields(typeof(PaymentMethodDetail.paymentMethodID))]
		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>
			> DetailsForReceivable;
		[PXCopyPasteHiddenFields(typeof(PaymentMethodDetail.paymentMethodID))]
		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
				  Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>,
			OrderBy<Asc<PaymentMethodDetail.orderIndex>>
			> DetailsForCashAccount;

		public string[] GetAddendaInfoFields()
		{
			var screenID = "AP305000";
			var addendaMember = "AddendaInfo";
			if (string.IsNullOrEmpty(screenID)) return null;

			var info = ScreenUtils.ScreenInfo.TryGet(screenID);
			if (info == null) return null;

			var res = info.Containers
				.Where(m => m.Key == addendaMember)
				.Select(c => new { container = c, viewName = c.Key.Split(new[] { ": " }, StringSplitOptions.None)[0] })
				.SelectMany(t => info.Containers[t.container.Key].Fields, (t, field) => {
					string tViewName = string.Empty, tFieldName = field.FieldName;
					if (field.FieldName.Contains("__"))
					{
						var result = field.FieldName.Replace("__", "_").Split('_');
						tViewName = result[0];
						tFieldName = result[1];
					}
					else
					{
						tViewName = t.viewName.Replace(addendaMember, nameof(PX.Objects.AP.APPayment));
					}

					if (tFieldName == "NoteText")
					{
						return string.Empty;
					}

					if (AddendaAliases.Direct.TryGetValue(tViewName, out var tViewAlias))
					{
						tViewName = tViewAlias;
					}

					return string.Concat("[", tViewName, ".", tFieldName, "]");
				})
				.Distinct();

			return res.ToArray();
		}

		[PXCopyPasteHiddenFields(typeof(PaymentMethodDetail.paymentMethodID))]
		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
				  Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>,
			OrderBy<Asc<PaymentMethodDetail.orderIndex>>
			> DetailsForVendor;
		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<PaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>,
				And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
				  Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>
			> RemmittanceSettings;
		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<PaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>,
				And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
				  Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>,
			OrderBy<Asc<PaymentMethodDetail.orderIndex>>
			> VendorInstructions;

		// We need this dummy view for correct VendorPaymentMethodDetail
		// records deletion using PXParentAttribute on an appropriate DAC.
		//
		public PXSelect<VendorPaymentMethodDetail> dummy_VendorPaymentMethodDetail;

		public PXSelectJoin<PaymentMethodAccount,
			InnerJoin<CashAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
			Where<PaymentMethodAccount.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>
			> CashAccounts;
		public PXSelect<CCProcessingCenterPmntMethod, Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>
			> ProcessingCenters;

		public PXSelect<CCProcessingCenterPmntMethod, Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
			And<CCProcessingCenterPmntMethod.isDefault, Equal<True>>>> DefaultProcCenter;

		public PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>
			> PaymentMethodCurrentForPlugIn;

		public PXSelect<PlugInFilter, Where<PlugInFilter.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
			And<PlugInFilter.plugInTypeName, Equal<Current<PaymentMethod.aPBatchExportPlugInTypeName>>>>> plugInFilter;

		protected virtual IEnumerable PlugInFilter()
		{
			var filter = plugInFilter.Current;

			if(filter == null)
			{
				foreach(PlugInFilter item in plugInFilter.View.QuickSelect())
				{
					filter = item;
					break;
				}
			}

			if(filter == null)
			{
				return new List<PlugInFilter> { plugInFilter.Current };
			}

			var parameter = aCHPlugInParametersByParameter.SelectSingle(nameof(IACHExportParameters.IncludeOffsetRecord).ToUpper());
			bool showOffsetSettings;
			bool.TryParse(parameter?.Value, out showOffsetSettings);
			filter.ShowOffsetSettings = showOffsetSettings;

			return new List<PlugInFilter> { filter };
		}

		protected virtual void _(Events.FieldUpdated<PlugInFilter.showAllSettings> e)
		{
			if (e.NewValue != e.OldValue)
			{
				aCHPlugInParameters.View.RequestRefresh();
			}
		}

		protected virtual void _(Events.FieldUpdated<PlugInFilter.showOffsetSettings> e)
		{
			if (e.NewValue != e.OldValue)
			{
				aCHPlugInParameters.View.RequestRefresh();
			}
		}

		[PXCopyPasteHiddenView]
		public PXSelect<ACHPlugInParameter, Where<ACHPlugInParameter.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
			And<ACHPlugInParameter.plugInTypeName, Equal<Current<PaymentMethod.aPBatchExportPlugInTypeName>>>>,
			OrderBy<Asc<ACHPlugInParameter.order>>> aCHPlugInParameters;

		public PXSelect<ACHPlugInParameter2, Where<ACHPlugInParameter2.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
			And<ACHPlugInParameter2.plugInTypeName, Equal<Current<PaymentMethod.aPBatchExportPlugInTypeName>>>>> PlugInParameters;

		public PXSelect<ACHPlugInParameter, Where<ACHPlugInParameter.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
			And<ACHPlugInParameter.plugInTypeName, Equal<Current<PaymentMethod.aPBatchExportPlugInTypeName>>,
			And<ACHPlugInParameter.value, Equal<Required<PaymentMethodDetail.detailID>>,
			And<ACHPlugInParameter.type, Equal<Required<ACHPlugInParameter.type>>>>>>> aCHPlugInParametersByID;

		public PXSelect<ACHPlugInParameter, Where<ACHPlugInParameter.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
			And<ACHPlugInParameter.plugInTypeName, Equal<Current<PaymentMethod.aPBatchExportPlugInTypeName>>,
			And<ACHPlugInParameter.parameterID, Equal<Required<ACHPlugInParameter.parameterID>>>>>> aCHPlugInParametersByParameter;

		public PXSelect<VendorPaymentMethodDetail,
			Where<VendorPaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>> vendorPaymentMethodDetail;

		public PXSelectJoin<VendorPaymentMethodDetail, RightJoin<VendorPaymentMethodDetailAlias,
				On<VendorPaymentMethodDetail.paymentMethodID, Equal<VendorPaymentMethodDetailAlias.paymentMethodID>,
					And<VendorPaymentMethodDetail.bAccountID, Equal<VendorPaymentMethodDetailAlias.bAccountID>,
					And<VendorPaymentMethodDetail.locationID, Equal<VendorPaymentMethodDetailAlias.locationID>>>>>,
			Where<VendorPaymentMethodDetailAlias.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<VendorPaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>,
				And<VendorPaymentMethodDetailAlias.detailID, Equal<Required<PaymentMethodDetail.detailID>>>>>> vendorPaymentMethodDetailByID;

		protected virtual IEnumerable ACHPlugInParameters()
		{
			return GetACHPlugInParameters();
		}

		private IEnumerable SelectParameters() => aCHPlugInParameters.View.QuickSelect();

		protected virtual void _(Events.RowInserted<ACHPlugInParameter2> e)
		{
			var row = e.Row;

			var parameters = GetParametersOfSelectedPlugIn().ToDictionary(m => m.ParameterID);

			if(parameters.ContainsKey(row?.ParameterID))
			{
				row.Description = parameters[row.ParameterID].Description;
				row.DetailMapping = parameters[row.ParameterID].DetailMapping;
				row.ExportScenarioMapping = parameters[row.ParameterID].ExportScenarioMapping;
				row.Type = parameters[row.ParameterID].Type;
				row.IsFormula = parameters[row.ParameterID].IsFormula;
			}

			var result = aCHPlugInParameters.Insert((ACHPlugInParameter)row);
		}

		protected IEnumerable GetACHPlugInParameters()
		{
			var plugInParameter = GetParametersOfSelectedPlugIn();

			bool includeOffsetRecord = false;
			bool includeAddendaRecords = false;

			foreach (ACHPlugInParameter param in SelectParameters())
			{
				if(param.ParameterID.ToUpper() == nameof(IACHExportParameters.IncludeOffsetRecord).ToUpper())
				{
					bool.TryParse(param.Value, out includeOffsetRecord);
				}
				if (param.ParameterID.ToUpper() == nameof(IACHExportParameters.IncludeAddendaRecords).ToUpper())
				{
					bool.TryParse(param.Value, out includeAddendaRecords);
				}
			}

			var result = new List<ACHPlugInParameter>();

			foreach (ACHPlugInParameter storedParam in SelectParameters())
			{
				foreach(IACHPlugInParameter templateParam in plugInParameter)
				{
					if(plugInFilter.Current?.ShowAllSettings != true && storedParam.IsAvailableInShortForm != true)
					{
						continue;
					}

					if (plugInFilter.Current?.ShowOffsetSettings != true && (storedParam.ParameterID.ToUpper() == nameof(IACHExportParameters.OffsetDFIAccountNbr).ToUpper()
											|| storedParam.ParameterID.ToUpper() == nameof(IACHExportParameters.OffsetReceivingDEFIID).ToUpper()
											|| storedParam.ParameterID.ToUpper() == nameof(IACHExportParameters.OffsetReceivingID).ToUpper()))
						
					{
						continue;
					}
					if (!includeAddendaRecords && storedParam.ParameterID.ToUpper() == nameof(IACHExportParameters.AddendaRecordTemplate).ToUpper())
					{
						continue;
					}
					if (storedParam.ParameterID.ToUpper() == templateParam.ParameterID.ToUpper())
					{
						result.Add(storedParam);
					}
				}
			}

			foreach (ACHPlugInParameter it in result)
			{
				if (it.Visible != true)
				{
					continue;
				}

				yield return it;
			}
		}

		private IEnumerable<IACHPlugInParameter> GetParametersOfSelectedPlugIn()
		{
			var plugIn = PaymentMethod.Current?.APBatchExportPlugInTypeName;

			if (string.IsNullOrEmpty(plugIn))
			{
				return new List<ACHPlugInParameter>();
			}

			Type plugInType = PXBuildManager.GetType(plugIn, true);
			var plugInInstance = Activator.CreateInstance(plugInType) as PX.ACHPlugInBase.IACHPlugIn;

			return plugInInstance.GetACHPlugInParameters();
		}
		#endregion
		public PaymentMethodMaint()
		{
			GLSetup setup = GLSetup.Current;
		}

		public PXSetup<GLSetup> GLSetup;

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			try
			{
				if (cacheType == typeof(PaymentMethodAccount))
				{

				}

				return base.Persist(cacheType, operation);
			}
			catch (PXDatabaseException e)
			{
				if (cacheType == typeof(PaymentMethodAccount)
					&& (operation == PXDBOperation.Delete
						|| operation == PXDBOperation.Command)
					&& (e.ErrorCode == PXDbExceptions.DeleteForeignKeyConstraintViolation
						|| e.ErrorCode == PXDbExceptions.DeleteReferenceConstraintViolation))
				{
					CashAccount ca = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.
						Select(this, e.Keys[1]);
					string CashAccountCD = ca.CashAccountCD;
					throw new PXException(Messages.CannotDeletePaymentMethodAccount, e.Keys[0], CashAccountCD);
				}
				else
				{
					throw;
				}
			}
		}

		#region Header Events
		protected virtual void PaymentMethod_PaymentType_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			bool found = false;
			foreach (PaymentMethodDetail iDet in this.Details.Select())
			{
				found = true; break;
			}
			if (found)
			{

				WebDialogResult res = this.PaymentMethod.Ask(Messages.AskConfirmation, Messages.PaymentMethodDetailsWillReset, MessageButtons.YesNo);
				if (res != WebDialogResult.Yes)
				{
					PaymentMethod row = (PaymentMethod)e.Row;
					e.Cancel = true;
					e.NewValue = row.PaymentType;
				}
			}
		}

		protected virtual void PaymentMethod_DirectDepositFileFormat_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			bool found = false;
			foreach (PaymentMethodDetail iDet in this.Details.Select())
			{
				found = true; break;
			}
			if (found)
			{
				WebDialogResult res = this.PaymentMethod.Ask(Messages.AskConfirmation, Messages.PaymentMethodDetailsWillReset, MessageButtons.YesNo);
				if (res != WebDialogResult.Yes)
				{
					PaymentMethod row = (PaymentMethod)e.Row;
					e.Cancel = true;
					e.NewValue = row.DirectDepositFileFormat;
				}
			}
		}

		protected virtual void PaymentMethod_PaymentType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			foreach(var item in ProcessingCenters.Select())
			{
				ProcessingCenters.Delete(item); 
			}
			PaymentMethod row = (PaymentMethod)e.Row;
			cache.SetDefaultExt<PaymentMethod.aRHasBillingInfo>(row);
			cache.SetDefaultExt<PaymentMethod.useForAP>(row);
			cache.SetDefaultExt<PaymentMethod.aRVoidOnDepositAccount>(row);
		}

		protected virtual void PaymentMethod_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			PaymentMethod row = (PaymentMethod)e.Row;

			bool isCreditCard = row.PaymentType == PaymentMethodType.CreditCard;
			bool isEft = row.PaymentType == PaymentMethodType.EFT;
			bool printChecks = (row.APPrintChecks == true);
			bool createBatch = (row.APCreateBatchPayment == true);
			bool usePlugInForExport = row.APBatchExportMethod == ACHExportMethod.PlugIn && !string.IsNullOrEmpty(row.APBatchExportPlugInTypeName);

			bool useForAP = row.UseForAP.GetValueOrDefault(false);
			bool useForAR = row.UseForAR.GetValueOrDefault(false);

			PXUIFieldAttribute.SetVisible<PaymentMethod.aPCheckReportID>(cache, row, printChecks);
			PXUIFieldAttribute.SetVisible<PaymentMethod.aPStubLines>(cache, row, printChecks);
			PXUIFieldAttribute.SetVisible<PaymentMethod.aPPrintRemittance>(cache, row, printChecks);
			PXUIFieldAttribute.SetVisible<PaymentMethod.aPRemittanceReportID>(cache, row, printChecks);

			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPPrintRemittance>(cache, row, printChecks);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPRemittanceReportID>(cache, row, printChecks && (row.APPrintRemittance == true));
			PXUIFieldAttribute.SetRequired<PaymentMethod.aPRemittanceReportID>(cache, printChecks && (row.APPrintRemittance == true));
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPCheckReportID>(cache, row, printChecks);
			PXUIFieldAttribute.SetRequired<PaymentMethod.aPCheckReportID>(cache, printChecks);

			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPPrintChecks>(cache, row, true);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPStubLines>(cache, row, printChecks);

			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPCreateBatchPayment>(cache, row, !printChecks);
			PXUIFieldAttribute.SetVisible<PaymentMethod.aPBatchExportSYMappingID>(cache, row, createBatch);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPBatchExportSYMappingID>(cache, row, createBatch);
			PXUIFieldAttribute.SetRequired<PaymentMethod.aPBatchExportSYMappingID>(cache, createBatch);

			PXUIFieldAttribute.SetVisible<PaymentMethod.skipPaymentsWithZeroAmt>(cache, row, createBatch);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.skipPaymentsWithZeroAmt>(cache, row, createBatch);

			PXUIFieldAttribute.SetVisible<PaymentMethod.requireBatchSeqNum>(cache, row, createBatch);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.requireBatchSeqNum>(cache, row, createBatch);

			PXUIFieldAttribute.SetVisible<PaymentMethod.aPRequirePaymentRef>(cache, row, !printChecks && !createBatch);

			PXUIFieldAttribute.SetEnabled<PaymentMethod.useForAP>(cache, row, !isEft);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aRVoidOnDepositAccount>(cache, row, !isEft);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aRIsProcessingRequired>(cache, row, !isEft);

			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.isExpirationDate>(this.Details.Cache, null, isCreditCard || isEft);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.isIdentifier>(this.Details.Cache, null, isCreditCard || isEft);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.isOwnerName>(this.Details.Cache, null, isCreditCard || isEft);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.displayMask>(this.Details.Cache, null, isCreditCard || isEft);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.isCCProcessingID>(this.Details.Cache, null, isCreditCard || isEft);

			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.isExpirationDate>(this.Details.Cache, null, isCreditCard || isEft);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.isIdentifier>(this.Details.Cache, null, isCreditCard || isEft);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.isIdentifier>(this.Details.Cache, null, isCreditCard || isEft);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.displayMask>(this.Details.Cache, null, isCreditCard || isEft);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.isCCProcessingID>(this.Details.Cache, null, isCreditCard || isEft);
			PXUIFieldAttribute.SetVisible<PaymentMethod.aRDepositAsBatch>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aRDepositAsBatch>(cache, null, false);

			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.useForAP>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPIsDefault>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPAutoNextNbr>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPLastRefNbr>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPBatchLastRefNbr>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPQuickBatchGeneration>(this.CashAccounts.Cache, null, useForAP && createBatch);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPQuickBatchGeneration>(this.CashAccounts.Cache, null, useForAP && createBatch);

			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.useForAR>(this.CashAccounts.Cache, null, useForAR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aRIsDefault>(this.CashAccounts.Cache, null, useForAR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aRAutoNextNbr>(this.CashAccounts.Cache, null, useForAR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aRLastRefNbr>(this.CashAccounts.Cache, null, useForAR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aRIsDefaultForRefund>(this.CashAccounts.Cache, null, useForAR);

			PXUIFieldAttribute.SetVisible<CCProcessingCenterPmntMethod.fundHoldPeriod>(this.ProcessingCenters.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetVisible<CCProcessingCenterPmntMethod.reauthDelay>(this.ProcessingCenters.Cache, null, isCreditCard);

			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.controlType>(this.Details.Cache, null, usePlugInForExport);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.defaultValue>(this.Details.Cache, null, usePlugInForExport);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.controlType>(this.Details.Cache, null, usePlugInForExport);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.defaultValue>(this.Details.Cache, null, usePlugInForExport);

			bool pluginSettingsEnabled = row.APBatchExportMethod == ACHExportMethod.PlugIn;
			aCHPlugInParameters.AllowSelect = pluginSettingsEnabled;
			//aCHPlugInParameters.AllowDelete = false;
			//aCHPlugInParameters.View.AllowDelete = false;

			PlugInParameters.AllowSelect = IsCopyPasteContext;
			PlugInParameters.AllowInsert = IsCopyPasteContext;
			PlugInParameters.AllowUpdate = IsCopyPasteContext;
			PlugInParameters.AllowDelete = IsCopyPasteContext;

			bool enableDirectDepositType = false;
			if (row.PaymentType == PaymentMethodType.DirectDeposit)
			{
				var records = DirectDepositService.GetDirectDepositTypes();
				if (records.Count() > 0)
				{
					enableDirectDepositType = true;
				}
			}

			PXUIFieldAttribute.SetVisible<PaymentMethod.directDepositFileFormat>(cache, row, enableDirectDepositType);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.directDepositFileFormat>(cache, row, enableDirectDepositType);

			PXUIFieldAttribute.SetVisible<PaymentMethod.requireBatchSeqNum>(cache, row, enableDirectDepositType);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.requireBatchSeqNum>(cache, row, false);

			bool showProcCenters = (row.ARIsProcessingRequired == true);
			this.ProcessingCenters.Cache.AllowDelete = showProcCenters;
			this.ProcessingCenters.Cache.AllowUpdate = showProcCenters;
			this.ProcessingCenters.Cache.AllowInsert = showProcCenters;

			PXResultset<CCProcessingCenterPmntMethod> currDefaultProcCenter = DefaultProcCenter.Select();

			bool isAccountNumberRequired = CCProcessingHelper.PaymentMethodSupportsIntegratedProcessing(row) &&
				row.UseForAR == true && row.IsAccountNumberRequired == false;

			PXSetPropertyException exception = null;
			if (isAccountNumberRequired)
			{
				exception = new PXSetPropertyException(Messages.CardAccountNumberMustBeSelected);
			}

			PaymentMethodCurrent.Cache.RaiseExceptionHandling<PaymentMethod.isAccountNumberRequired>(row, row.IsAccountNumberRequired, exception);
			PaymentMethodCurrent.Cache.RaiseExceptionHandling<PaymentMethod.aRIsProcessingRequired>(row, row.ARIsProcessingRequired, exception);

			if (PXAccess.FeatureInstalled<CS.FeaturesSet.integratedCardProcessing>() && row.ARIsProcessingRequired == true && currDefaultProcCenter.Count == 0)
			{
				PaymentMethod.Cache.RaiseExceptionHandling<PaymentMethod.aRIsProcessingRequired>(row, row.ARIsProcessingRequired,
					new PXSetPropertyException(Messages.NoProcCenterSetAsDefault, PXErrorLevel.Warning));
			}
			else
			{
				PXFieldState state = (PXFieldState)cache.GetStateExt<PaymentMethod.aRIsProcessingRequired>(row);
				if (state.IsWarning && String.Equals(state.Error, Messages.NoProcCenterSetAsDefault))
				{
					PaymentMethod.Cache.RaiseExceptionHandling<PaymentMethod.aRIsProcessingRequired>(row, null, null);
				}
			}

			foreach (CCProcessingCenterPmntMethod procCenter in ProcessingCenters.Select())
			{
				ProcessingCenters.Cache.RaiseRowSelected(procCenter);
			}
		}

		protected virtual void PaymentMethod_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			PaymentMethod oldRow = (PaymentMethod)e.OldRow;
			if (oldRow.PaymentType != row.PaymentType)
			{
				foreach (PaymentMethodDetail iDet in this.Details.Select())
				{
					this.Details.Cache.Delete(iDet);
				}
				if (row.PaymentType == PaymentMethodType.CreditCard || row.PaymentType == PaymentMethodType.EFT)
				{
					this.fillCreditCardDefaults();
				}
				row.ARIsOnePerCustomer = row.PaymentType == PaymentMethodType.CashOrCheck;
				row.ARIsProcessingRequired = PXAccess.FeatureInstalled<CS.FeaturesSet.integratedCardProcessing>() && (row.PaymentType == PaymentMethodType.CreditCard || row.PaymentType == PaymentMethodType.EFT);
			}

			if ((oldRow.UseForAR != row.UseForAR) && row.UseForAR.GetValueOrDefault(false) == false)
			{
				row.ARIsProcessingRequired = false;

				foreach (PaymentMethodAccount pma in CashAccounts.Select())
				{
					pma.UseForAR = pma.ARIsDefault = pma.ARIsDefaultForRefund = false;
					CashAccounts.Update(pma);
				}
			}

			if ((oldRow.UseForAR != row.UseForAR) && row.UseForAR.GetValueOrDefault(true) == true && row.PaymentType == PaymentMethodType.EFT)
			{
				row.ARIsProcessingRequired = true;
			}

			if ((oldRow.UseForAP != row.UseForAP) && row.UseForAP.GetValueOrDefault(false) == false)
			{
				foreach (PaymentMethodAccount pma in CashAccounts.Select())
				{
					pma.UseForAP = pma.APIsDefault = false;
					CashAccounts.Update(pma);
				}
			}

			if (oldRow.APCreateBatchPayment != row.APCreateBatchPayment)
			{
				foreach (PaymentMethodAccount pma in CashAccounts.Select())
				{
					if (pma.APQuickBatchGeneration == true)
					{
						pma.APQuickBatchGeneration = false;
						CashAccounts.Update(pma);
					}
				}
			}

			var isPluginSelected = row.APBatchExportMethod == ACHExportMethod.PlugIn;
			if (isPluginSelected && !aCHPlugInParameters.Any())
			{
				if(string.IsNullOrEmpty(row.APBatchExportPlugInTypeName))
				{
					var copy = (PaymentMethod)PaymentMethod.Cache.CreateCopy(row);
					copy.APBatchExportPlugInTypeName = "PX.ACHPlugIn.ACHPlugIn";
					PaymentMethod.Update(copy);
				}

				if (!IsCopyPasteContext)
				{
					bool settingsExists = CheckIfACHSettingsExists();
					if (!settingsExists)
					{
						AppendDefaultSettings();
						AppendDefaultPlugInParameters();
					}
					else
					{
						if (CheckAcumaticaExportScenariosMapping())
						{
							UpdateDetailsAccordingToPlugIn();
							AppendDefaultPlugInParameters(useExportScenarioMapping: true);
						}
						else
						{
							AppendDefaultPlugInParameters();
						}
					}
				}
			}
		}

		private bool CheckIfACHSettingsExists() => DetailsForCashAccount.Any() || DetailsForVendor.Any();

		private bool CheckAcumaticaExportScenariosMapping()
		{
			var remSettings = new Dictionary<string, string>
			{
				{ "1","Beneficiary Account No:"},
				{ "2","Beneficiary Name:"},
				{ "3","Bank Routing Number (ABA):"},
				{ "4","Bank Name:"},
				{ "5","Company ID"},
				{ "6","Company ID Type"},
				{ "7","Offset ABA/Routing #"},
				{ "8","Offset Account #"},
				{ "9","Offset Description"},
			};

			var vendorSettings = new Dictionary<string, string>
			{
				{ "1","Beneficiary Account No:"},
				{ "2","Beneficiary Name:"},
				{ "3","Bank Routing Number (ABA):"},
				{ "4","Bank Name:"},
			};

			try
			{
				foreach (PaymentMethodDetail detail in DetailsForCashAccount.Select())
				{
					var id = detail.DetailID.Trim();
					if (remSettings[id] != detail.Descr.Trim())
					{
						return false;
					}
				}

				foreach (PaymentMethodDetail detail in DetailsForVendor.Select())
				{
					var id = detail.DetailID.Trim();
					if (vendorSettings[id] != detail.Descr.Trim())
					{
						return false;
					}
				}
			}
			catch
			{
				return false;
			}

			return true;
		}

		private void AppendDefaultSettings()
		{
			foreach(var settings in GetDefaultSettings(ACHPlugInBase.DefaultDetails.DetailsToAddByDefault))
			{
				if(settings.UseFor == PaymentMethodDetailUsage.UseForCashAccount)
				{
					DetailsForCashAccount.Insert(settings.ToPaymentMethodDetail(this.PaymentMethod.Current));
				}

				if (settings.UseFor == PaymentMethodDetailUsage.UseForVendor)
				{
					DetailsForVendor.Insert(settings.ToPaymentMethodDetail(this.PaymentMethod.Current));
				}
			}
		}

		private void AppendTransactionCodeSetting()
		{
			foreach (var settings in GetDefaultSettings(ACHPlugInBase.DefaultDetails.DetailsToAddTransactionCode))
			{
				if (settings.UseFor == PaymentMethodDetailUsage.UseForCashAccount)
				{
					DetailsForCashAccount.Insert(settings.ToPaymentMethodDetail(this.PaymentMethod.Current));
				}

				if (settings.UseFor == PaymentMethodDetailUsage.UseForVendor)
				{
					DetailsForVendor.Insert(settings.ToPaymentMethodDetail(this.PaymentMethod.Current));
				}
			}
		}

		private void UpdateDetailsAccordingToPlugIn()
		{
			foreach (PaymentMethodDetail detail in DetailsForCashAccount.Select())
			{
				if(detail.DetailID == "5" && detail.Descr == "Company ID")
				{
					detail.ValidRegexp = "^([\\w]|\\s){0,10}$";
					DetailsForCashAccount.Update(detail);
				}
				if (detail.DetailID == "6" && detail.Descr == "Company ID Type" && detail.IsRequired == true)
				{
					detail.IsRequired = false;
					DetailsForCashAccount.Update(detail);
				}
			}

			AppendTransactionCodeSetting();
		}

		private void AppendDefaultPlugInParameters(bool useExportScenarioMapping = false)
		{
			var plugInParameter = GetParametersOfSelectedPlugIn();
			var rsIDs = GetRemittenceDetailsID();
			var viIDs = GetVendorDetailsID();

			foreach (var parameter in plugInParameter)
			{
				var newParam = new ACHPlugInParameter {
					ParameterID = parameter.ParameterID.ToUpper(),
					ParameterCode = parameter.ParameterCode,
					Description = parameter.Description,
					Order = parameter.Order,
					Required = parameter.Required,
					Type = parameter.Type,
					UsedIn = parameter.UsedIn,
					Visible = parameter.Visible,
					IsGroupHeader = parameter.IsGroupHeader,
					IsAvailableInShortForm = parameter.IsAvailableInShortForm,
					IsFormula = parameter.IsFormula,
				};

				var detailIDs = parameter.Type == (int)SelectorType.RemittancePaymentMethodDetail ? rsIDs : new HashSet<string>();
				detailIDs = parameter.Type == (int)SelectorType.VendorPaymentMethodDetail ? viIDs : detailIDs;

				if (parameter.DetailMapping.HasValue &&
					DefaultPaymentMethodDetailsHelper.Dictionary.TryGetValue((DefaultPaymentMethodDetails)parameter.DetailMapping, out var mappingID)
					&& detailIDs.Contains(mappingID))
				{
					if (useExportScenarioMapping)
					{
						if (parameter.ExportScenarioMapping.HasValue &&
						DefaultPaymentMethodDetailsHelper.Dictionary.TryGetValue((DefaultPaymentMethodDetails)parameter.ExportScenarioMapping, out var scenarioMappingID)
						&& detailIDs.Contains(scenarioMappingID))
						{
							newParam.Value = scenarioMappingID;
						}
						else
						{
							newParam.Value = mappingID;
						}
					}
					else
					{
						newParam.Value = mappingID;
					}
				}
				else
				{
					newParam.Value = parameter.Value;
				}

				newParam = aCHPlugInParameters.Insert(newParam);
			}

		}

		private HashSet<string> GetRemittenceDetailsID()
		{
			var detailsID = new HashSet<string>();

			foreach (PaymentMethodDetail detail in DetailsForCashAccount.Select())
			{
				var id = detail.DetailID.Trim();
				detailsID.Add(id);
			}

			return detailsID;
		}

		private HashSet<string> GetVendorDetailsID(bool selectRemittanceOnly = false)
		{
			var detailsID = new HashSet<string>();

			foreach (PaymentMethodDetail detail in DetailsForVendor.Select())
			{
				var id = detail.DetailID.Trim();
				detailsID.Add(id);
			}

			return detailsID;
		}

		private void AppendOffsetSettings()
		{
			var newSettings = new Dictionary<DefaultPaymentMethodDetails, string>();

			foreach(var settings in GetDefaultSettings(ACHPlugInBase.DefaultDetails.DetailsToAddForOffset))
			{
				var ss = DetailsForCashAccount.Insert(settings.ToPaymentMethodDetail(this.PaymentMethod.Current));
				newSettings.Add((DefaultPaymentMethodDetails)settings.DetailIDInt, settings.DetailID);
			}

			foreach (ACHPlugInParameter item in aCHPlugInParameters.View.QuickSelect())
			{
				if (item.ParameterID == nameof(IACHExportParameters.OffsetDFIAccountNbr).ToUpper())
				{
					item.Value = newSettings[DefaultPaymentMethodDetails.OffsetAccountNumber];
					aCHPlugInParameters.Update(item);
				}

				if (item.ParameterID == nameof(IACHExportParameters.OffsetReceivingDEFIID).ToUpper())
				{
					item.Value = newSettings[DefaultPaymentMethodDetails.OffsetABARoutingNumber];
					aCHPlugInParameters.Update(item);
				}

				if (item.ParameterID == nameof(IACHExportParameters.OffsetReceivingID).ToUpper())
				{
					item.Value = newSettings[DefaultPaymentMethodDetails.OffsetDescription];
					aCHPlugInParameters.Update(item);
				}
			}
		}

		private IEnumerable<NewPaymentMethodDetail> GetDefaultSettings(DefaultPaymentMethodDetails[] details)
		{
			foreach(var id in details)
			{
				string mappingID;
				if (!DefaultPaymentMethodDetailsHelper.Dictionary.TryGetValue(id, out mappingID))
				{
					continue;
				}

				var descr = string.Empty;
				ACHPlugInBase.DefaultDetails.DefaultDetailDescription.TryGetValue(id, out descr);
				var useFor = string.Empty;
				ACHPlugInBase.DefaultDetails.DetailsUsedFor.TryGetValue(id, out useFor);
				var regExp = string.Empty;
				ACHPlugInBase.DefaultDetails.DefaultDetailValidationRegexp.TryGetValue(id, out regExp);
				var entryMask = string.Empty;
				ACHPlugInBase.DefaultDetails.DefaultDetailEntryMask.TryGetValue(id, out entryMask);
				var required = ACHPlugInBase.DefaultDetails.RequiredDetailsByDefault.Contains(id);
				var controlType = ACHPlugInBase.DefaultDetails.AccountTypeFields.Contains(id) ? PaymentMethodDetailType.AccountType : PaymentMethodDetailType.Text;
				var selected = ACHPlugInBase.DefaultDetails.NotSelectedFieldsByDefault.Contains(id);

				yield return new NewPaymentMethodDetail { DetailIDInt = (int)id, DetailID = mappingID, Description = descr, IsRequired = required, ControlType = (int)controlType, ValidRegexp = regExp, UseFor = useFor, EntryMask = entryMask };
			}
		}

		

		protected virtual void PaymentMethod_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			foreach (PaymentMethodDetail iDet in this.Details.Select())
			{
				this.Details.Cache.Delete(iDet);
			}

			if (row.PaymentType == PaymentMethodType.CreditCard || row.PaymentType == PaymentMethodType.EFT)
			{
				this.fillCreditCardDefaults();
				row.ARIsOnePerCustomer = false;
			}

			row.ARIsOnePerCustomer = row.PaymentType == PaymentMethodType.CashOrCheck;
		}

		protected virtual void PaymentMethod_aRVoidOnDepositAccount_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			DefaultEFTSettings(e);
		}

		protected virtual void PaymentMethod_UseForAP_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			DefaultEFTSettings(e);
		}

		private void DefaultEFTSettings(PXFieldDefaultingEventArgs e)
		{
			PaymentMethod pm = this.PaymentMethod.Current;

			if (pm != null)
			{
				if (pm.PaymentType == PaymentMethodType.EFT)
				{
					e.NewValue = false;
				}
			}
		}

		protected virtual void _(Events.RowPersisting<PaymentMethod> e)
		{
			if (e.Operation != PXDBOperation.Delete)
			{
				PaymentMethod pm = e.Row;

				VerifyAPRequirePaymentRefAndAPAdditionalProcessing(e.Row.APRequirePaymentRef, e.Row.APAdditionalProcessing);

				CheckPaymentMethodDetailsIfProcessingReq(pm);
			}

			if (e.Operation == PXDBOperation.Delete)
			{
				foreach (PXResult<PaymentMethodAccount, CashAccount> row in CashAccounts.Select())
				{
					VerifyCashAccountLinkOrMethodCanBeDeleted(row);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PaymentMethod.paymentType>e)
		{
			if (e.NewValue.ToString() != PaymentMethodType.DirectDeposit)
			{
				this.PaymentMethod.Cache.SetValueExt<PaymentMethod.directDepositFileFormat>(e.Row, null);
			}
		}

		protected virtual void _(Events.FieldUpdated<PaymentMethod.directDepositFileFormat>e)
		{
			if (e.NewValue != null)
			{
				PaymentMethod paymentMethod = (PaymentMethod)e.Row;

				if (!string.IsNullOrEmpty(paymentMethod.DirectDepositFileFormat))
				{
					DirectDepositService?.SetPaymentMethodDefaults(this.PaymentMethod.Cache);

					foreach (PaymentMethodDetail iDet in Details.Select())
					{
						Details.Cache.Delete(iDet);
					}

					var details = DirectDepositService?.GetDefaults(e.NewValue.ToString());
					if (details != null)
					{
						foreach (var det in details)
						{
							Details.Cache.Insert(det);
						}
					}
				}
			}
		}

		protected virtual void _(Events.RowPersisting<PaymentMethodDetail> e)
		{
			PaymentMethodDetail detail = e.Row;

			if(e.Operation == PXDBOperation.Delete)
			{
				var selectorType = SelectorType.RemittancePaymentMethodDetail;

				if(detail.UseFor == PaymentMethodDetailUsage.UseForVendor)
				{
					selectorType = SelectorType.VendorPaymentMethodDetail;
				}

				if (detail.UseFor == PaymentMethodDetailUsage.UseForCashAccount)
				{
					selectorType = SelectorType.RemittancePaymentMethodDetail;
				}

				var plugInParameter = aCHPlugInParametersByID.SelectSingle(detail?.DetailID, (int?)selectorType);

				if(!string.IsNullOrEmpty(plugInParameter?.ParameterID))
				{
					if(detail.UseFor == PaymentMethodDetailUsage.UseForVendor)
					{
						throw new PXException(Messages.PaymentMethodDetailIsMappedWithPlugInSettingAndMissingOnSettingsForUseInAP, detail.Descr, plugInParameter.ParameterCode);
					}
					if(detail.UseFor == PaymentMethodDetailUsage.UseForCashAccount)
					{
						throw new PXException(Messages.PaymentMethodDetailIsMappedWithPlugInSettingAndMissingOnRemittanceSettings, detail.Descr, plugInParameter.ParameterCode);
					}
				}
			}

			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				if (detail.ControlType == (int?)PaymentMethodDetailType.AccountType && !string.IsNullOrEmpty(detail.DefaultValue))
				{
					var firstDetail = DetailsForVendor.SelectSingle();

					if (!string.IsNullOrEmpty(firstDetail.DetailID))
					{
						var vendorDetail = vendorPaymentMethodDetail.SelectSingle();

						if (!string.IsNullOrEmpty(vendorDetail?.DetailID))
						{
							// Acuminator disable once PX1046 LongOperationInEventHandlers [for a while and the code will be fixed in the future]
							// Acuminator disable once PX1008 LongOperationDelegateSynchronousExecution [for a while and the code will be fixed in the future]
							UpdateVendorDetails(detail, firstDetail);
						}
					}
				}
			}

			if (e.Operation == PXDBOperation.Delete || detail == null) return;

			PXDefaultAttribute.SetPersistingCheck<PaymentMethodDetail.displayMask>(e.Cache, detail, detail.IsIdentifier == true
				? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}

		private void UpdateVendorDetails(PaymentMethodDetail detail, PaymentMethodDetail firstDetail)
		{
			// Acuminator disable once PX1008 LongOperationDelegateSynchronousExecution [for a while and the code will be fixed in the future]
			PXLongOperation.StartOperation(this, () =>
			{
				foreach (PXResult<VendorPaymentMethodDetail, VendorPaymentMethodDetailAlias> detailResult in vendorPaymentMethodDetailByID.Select(detail.DetailID, firstDetail.DetailID))
				{
					VendorPaymentMethodDetail vDetail = detailResult;
					VendorPaymentMethodDetailAlias vaDetail = detailResult;

					if (string.IsNullOrEmpty(vDetail.DetailID))
					{
						vDetail = (VendorPaymentMethodDetail)vendorPaymentMethodDetail.Cache.CreateCopy((VendorPaymentMethodDetail)vaDetail);
						vDetail.DetailID = detail.DetailID;
						vDetail.DetailValue = detail.DefaultValue;
						vDetail = vendorPaymentMethodDetail.Insert(vDetail);
					}
					else
					{
						vDetail.DetailValue = detail.DefaultValue;
						vDetail = vendorPaymentMethodDetail.Update(vDetail);
					}
				}

				vendorPaymentMethodDetail.Cache.Persist(PXDBOperation.Insert);
				vendorPaymentMethodDetail.Cache.Persist(PXDBOperation.Update);
			});
		}

		protected virtual void PaymentMethod_ARHasBillingInfo_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			if (row.PaymentType == PaymentMethodType.CreditCard || row.PaymentType == PaymentMethodType.EFT)
			{
				e.NewValue = true;
				e.Cancel = true;
			}


		}

		protected virtual void _(Events.FieldVerifying<PaymentMethod.aPAdditionalProcessing> e)
		{
			string newValue = (string) e.NewValue;

			if (newValue != null)
			{
				VerifyAPRequirePaymentRefAndAPAdditionalProcessing(null, newValue);
			}
		}

		protected virtual void _(Events.FieldVerifying<PaymentMethod.aPRequirePaymentRef> e)
		{
			bool? newValue = (bool?)e.NewValue;

			if (newValue != null)
			{
				VerifyAPRequirePaymentRefAndAPAdditionalProcessing(newValue, null);
			}
		}

		protected virtual void PaymentMethod_APAdditionalProcessing_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (PaymentMethod)e.Row;
			switch(row.APAdditionalProcessing)
			{
				case CA.PaymentMethod.aPAdditionalProcessing.PrintChecks:
					row.APPrintChecks = true;
					row.APCreateBatchPayment = false;
					break;
				case CA.PaymentMethod.aPAdditionalProcessing.CreateBatchPayment:
					row.APCreateBatchPayment = true;
					row.APPrintChecks = false;
					break;
				default:
					row.APPrintChecks = false;
					row.APCreateBatchPayment = false;
					break;
			}

			if(row.APPrintChecks == true || row.APCreateBatchPayment == true)
			{
				sender.SetValuePending<PaymentMethod.aPRequirePaymentRef>(row, true);
			}

			if(row.APPrintChecks != true)
			{
				sender.SetDefaultExt<PaymentMethod.aPCheckReportID>(row);
				sender.SetDefaultExt<PaymentMethod.aPStubLines>(row);
				sender.SetDefaultExt<PaymentMethod.aPPrintRemittance>(row);
				sender.SetDefaultExt<PaymentMethod.aPRemittanceReportID>(row);
			}

			if(row.APCreateBatchPayment != true)
			{
				sender.SetDefaultExt<PaymentMethod.aPBatchExportSYMappingID>(row);
			}
		}

		protected virtual void PaymentMethod_APPrintChecks_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			if ((bool)row.APPrintChecks)
			{
				row.APCreateBatchPayment = false;
				row.APCheckReportID = null;
			}
			else
			{
				sender.SetDefaultExt<PaymentMethod.aPCreateBatchPayment>(row);
			}
		}

		public override int ExecuteInsert(string viewName, IDictionary values, params object[] parameters)
		{
			switch (viewName)
			{
				case "DetailsForCashAccount":
					values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForCashAccount;
					break;
				case "DetailsForVendor":
					values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForVendor;
					break;
				case "DetailsForReceivable":
					values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForARCards;
					break;
			}
			return base.ExecuteInsert(viewName, values, parameters);
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			string value = (String)values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()];
			if (string.IsNullOrEmpty(value) || value == PaymentMethodDetailUsage.UseForAll)
			{
				switch (viewName)
				{
					case "DetailsForCashAccount":
						keys[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForCashAccount;
						values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForCashAccount;
						break;
					case "DetailsForVendor":
						keys[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForVendor;
						values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForVendor;
						break;
					case "DetailsForReceivable":
						keys[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForARCards;
						values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForARCards;
						break;
				}
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		#endregion
		#region ACHPlugInParameter
		protected virtual void _(Events.RowDeleting<ACHPlugInParameter> e)
		{
			e.Cancel = true;
		}

		protected virtual void _(Events.RowPersisting<ACHPlugInParameter> e)
		{
			if(PaymentMethod.Current?.IsUsingPlugin != true)
			{
				return;
			}

			if(PlugInParameters.Cache.Inserted.Any_())
			{
				PlugInParameters.Cache.Clear();
			}

			bool offsetFieldsRequired = false;
			bool addendaTemplateRequired = false;

			foreach (ACHPlugInParameter param in aCHPlugInParameters.Select())
			{
				if (param.ParameterID == nameof(IACHExportParameters.IncludeOffsetRecord).ToUpper())
				{
					bool.TryParse(param.Value, out offsetFieldsRequired);
				}
				if (param.ParameterID == nameof(IACHExportParameters.IncludeAddendaRecords).ToUpper())
				{
					bool.TryParse(param.Value, out addendaTemplateRequired);
				}
			}

			var isValueEmpty = string.IsNullOrEmpty(e.Row?.Value);

			if (isValueEmpty)
			{
				if (e.Row.Required == true)
				{
					e.Cache.RaiseExceptionHandling<ACHPlugInParameter.value>(e.Row, e.Row?.Value, new PXSetPropertyException<ACHPlugInParameter.value>(CS.Messages.CannotBeEmpty));
				}

				if (offsetFieldsRequired && (e.Row.ParameterID == nameof(IACHExportParameters.OffsetDFIAccountNbr).ToUpper() ||
							e.Row.ParameterID == nameof(IACHExportParameters.OffsetReceivingDEFIID).ToUpper() ||
							e.Row.ParameterID == nameof(IACHExportParameters.OffsetReceivingID).ToUpper()))
				{
					e.Cache.RaiseExceptionHandling<ACHPlugInParameter.value>(e.Row, e.Row?.Value, new PXSetPropertyException<ACHPlugInParameter.value>(CS.Messages.CannotBeEmpty));
				}

				if (addendaTemplateRequired && (e.Row.ParameterID == nameof(IACHExportParameters.AddendaRecordTemplate).ToUpper()))
				{
					e.Cache.RaiseExceptionHandling<ACHPlugInParameter.value>(e.Row, e.Row?.Value, new PXSetPropertyException<ACHPlugInParameter.value>(CS.Messages.CannotBeEmpty));
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<ACHPlugInParameter, ACHPlugInParameter.value> e)
		{
			if (e.Row?.ParameterID.ToUpper() == nameof(IACHExportParameters.IncludeOffsetRecord).ToUpper())
			{
				var newValue = false;
				bool.TryParse(e.NewValue.ToString(), out newValue);

				if (newValue == true && !CheckIfOffsetSettingsExists())
				{
					var result = PaymentMethod.Ask("Do you want to add remittance settings for the offset record?", MessageButtons.YesNo);

					if (result == WebDialogResult.Yes)
					{
						AppendOffsetSettings();
					}
				}

				plugInFilter.Cache.SetValueExt<PlugInFilter.showOffsetSettings>(plugInFilter.Current, newValue);
			}

			if (e.Row?.ParameterID.ToUpper() == nameof(IACHExportParameters.IncludeAddendaRecords).ToUpper())
			{
				aCHPlugInParameters.View.RequestRefresh();
			}
		}

		protected virtual void _(Events.ExceptionHandling<ACHPlugInParameter.value> e)
		{
			if(e.Exception != null)
			{
				plugInFilter.Cache.SetValueExt<PlugInFilter.showAllSettings>(plugInFilter.Current, true);
			}
		}
		#endregion
		#region ACH Export Settings
		[Obsolete("Temporary not used. Seems to be deleted.")]
		private string GetRemmittanceDetailFor(DefaultPaymentMethodDetails detailMapping)
		{
			var detail = RemmittanceSettings.SelectSingle((int)detailMapping);
			if (string.IsNullOrEmpty(detail?.DetailID))
			{
				detail = DetailsForCashAccount.Locate(new PaymentMethodDetail { DetailID = ((int)detailMapping).ToString(), PaymentMethodID = this.PaymentMethod.Current.PaymentMethodID, UseFor = ACHPlugInBase.DefaultDetails.DetailsUsedFor[detailMapping] });
			}

			return detail?.DetailID;
		}

		private bool CheckIfOffsetSettingsExists()
		{
			var offsetDescriptions = new string[] { ACHPlugInBase.Messages.OffsetAccountNumber,
													ACHPlugInBase.Messages.OffsetABARoutingNumber,
													ACHPlugInBase.Messages.OffsetDescription };

			// Acuminator disable once PX1015 IncorrectNumberOfSelectParameters [there is in(...) statement that cannot be recognized]
			return PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<PaymentMethodDetail.descr, In<Required<PaymentMethodDetail.descr>>>>>.Select(this, offsetDescriptions).Any();
		}

		[Obsolete("Temporary not used. Seems to be deleted.")]
		private string GetVendorDetailFor(DefaultPaymentMethodDetails detailMapping)
		{
			var detail = VendorInstructions.SelectSingle((int)detailMapping);
			if (string.IsNullOrEmpty(detail?.DetailID))
			{
				detail = DetailsForVendor.Locate(new PaymentMethodDetail { DetailID = ((int)detailMapping).ToString(), PaymentMethodID = this.PaymentMethod.Current.PaymentMethodID, UseFor = ACHPlugInBase.DefaultDetails.DetailsUsedFor[detailMapping] });
			}

			return detail?.DetailID;
		}
		#endregion
		#region Detail Events
		protected virtual void PaymentMethodDetail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			bool enableID = false;
			PaymentMethodDetail row = e.Row as PaymentMethodDetail;
			if (row == null ||(row!=null && string.IsNullOrEmpty(row.DetailID))) enableID = true;
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.detailID>(cache, e.Row, enableID);

			bool isID = (row!= null) && (row.IsIdentifier ?? false);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.displayMask>(cache, e.Row, isID);

			bool isAccountType = ((PaymentMethodDetailType?)row?.ControlType) == PaymentMethodDetailType.AccountType;
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.defaultValue>(cache, e.Row, isAccountType);
		}

		protected virtual void CCProcessingCenterPmntMethod_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			CCProcessingCenterPmntMethod procCenterPM = e.Row as CCProcessingCenterPmntMethod;
			CCProcessingCenter procCenter = (CCProcessingCenter)PXParentAttribute.SelectParent<CCProcessingCenter>(cache, procCenterPM);

			UIState.RaiseOrHideErrorByErrorLevelPriority<CCProcessingCenterPmntMethod.processingCenterID>(cache, e.Row, procCenter?.IsExternalAuthorizationOnly == true,
				Messages.ProcCenterDoesNotSupportReauth, PXErrorLevel.RowWarning, procCenterPM.ProcessingCenterID);
		}

		protected virtual void PaymentMethodDetail_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			if (errorKey)
			{
				errorKey = false;
				e.Cancel = true;
			}
			else
			{
				PaymentMethodDetail row = (PaymentMethodDetail)e.Row;
				string detID = row.DetailID;
				string UseFor = row.UseFor;

				bool isExist = false;
				foreach (PaymentMethodDetail it in this.Details.Select())
				{
					if ((it.DetailID == detID) && (UseFor == it.UseFor))
					{
						isExist = true;
					}
				}

				if (isExist)
				{
					cache.RaiseExceptionHandling<PaymentMethodDetail.detailID>(e.Row, detID, new PXException(Messages.DuplicatedPaymentMethodDetail));
					e.Cancel = true;
				}
			}
		}

		protected virtual void PaymentMethodDetail_DetailID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			PaymentMethodDetail a = e.Row as PaymentMethodDetail;
			if (a.DetailID != null)
			{
				errorKey = true;
			}
		}
		#endregion

		#region Account Events

		protected virtual void PaymentMethodAccount_PaymentMethodID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PaymentMethodAccount_CashAccountID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			int? newCashAccountID = e.NewValue as int?;

			if (newCashAccountID == null)
				return;

			CashAccount cashAccount = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, newCashAccountID);

			if (cashAccount != null && cashAccount.Active != true)
			{
				string errorMsg = string.Format(CA.Messages.CashAccountInactive, cashAccount.CashAccountCD);
				cache.RaiseExceptionHandling<PaymentMethodAccount.cashAccountID>(e.Row, cashAccount.CashAccountCD, new PXSetPropertyException(errorMsg, PXErrorLevel.Error));
			}
		}

		protected virtual void PaymentMethodAccount_APQuickBatchGeneration_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
			=> PaymentMethodAccountHelper.APQuickBatchGenerationFieldVerifying(cache, e);

		protected virtual void PaymentMethodAccount_CashAccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			cache.SetDefaultExt<PaymentMethodAccount.useForAP>(row);
		}

		protected virtual void PaymentMethodAccount_UseForAP_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			CA.PaymentMethod pm = this.PaymentMethod.Current;

			if (row != null && pm != null)
			{
				e.NewValue = (pm.UseForAP == true);

				if (pm.UseForAP == true && row.CashAccountID.HasValue)
				{
					CashAccount c = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);
					e.NewValue = (c != null);
				}

				e.Cancel = true;
			}
		}

		protected virtual void PaymentMethodAccount_UseForAR_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			CA.PaymentMethod pm = this.PaymentMethod.Current;
			e.NewValue = (pm != null) && pm.UseForAR == true;
			e.Cancel = true;
		}

		protected virtual void PaymentMethodAccount_APQuickBatchGeneration_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			var row = (PaymentMethodAccount)e.Row;
			e.NewValue = row?.UseForAP == true && PaymentMethod.Current.APCreateBatchPayment == true;
			e.Cancel = true;
		}

		protected virtual void PaymentMethodAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount) e.Row;

			if (row == null)
				return;

			if (string.IsNullOrEmpty(row.PaymentMethodID) == false)
			{
				PaymentMethod pt = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
				bool enabled = (pt != null) && pt.APCreateBatchPayment == true;
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPBatchLastRefNbr>(cache, row, enabled);
			}

			bool isCashAccountActive = true;
			if (row.CashAccountID.HasValue)
			{
				CashAccount c = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.useForAP>(cache, row, (c != null));
				isCashAccountActive = c.Active ?? true;
			}

			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.useForAP>(cache, e.Row, isCashAccountActive);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.useForAR>(cache, e.Row, isCashAccountActive);

			bool enableAP = row.UseForAP & isCashAccountActive ?? false;
			bool enableAR = row.UseForAR & isCashAccountActive ?? false;

			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPIsDefault>(cache, e.Row, enableAP);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPAutoNextNbr>(cache, e.Row, enableAP);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPLastRefNbr>(cache, e.Row, enableAP);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPBatchLastRefNbr>(cache, e.Row, enableAP);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPQuickBatchGeneration>(cache, e.Row, enableAP);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRIsDefault>(cache, e.Row, enableAR);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRIsDefaultForRefund>(cache, e.Row, enableAR);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRAutoNextNbr>(cache, e.Row, enableAR);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRLastRefNbr>(cache, e.Row, enableAR);

			PaymentMethodAccountHelper.VerifyAPAutoNextNbr(cache, row, PaymentMethodCurrent.Current);
		}

		protected virtual void PaymentMethodAccount_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.NewRow;
			if (row != null)
			{
				PaymentMethodAccount oldrow = (PaymentMethodAccount)e.Row;
				if ((row.UseForAP != oldrow.UseForAP) && !row.UseForAP.GetValueOrDefault(false))
				{
					row.APIsDefault = false;
				}

				if ((row.UseForAR != oldrow.UseForAR) && !row.UseForAR.GetValueOrDefault(false))
				{
					row.ARIsDefault = false;
				}
			}
		}

		protected virtual void PaymentMethodAccount_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = (PaymentMethodAccount)e.Row;

			PXException ex = null;
			if (row.APQuickBatchGeneration == true && !PaymentMethodAccountHelper.TryToVerifyAPLastReferenceNumber<PaymentMethodAccount.aPQuickBatchGeneration>(row.APAutoNextNbr, row.APLastRefNbr, null, out ex))
			{
				cache.RaiseExceptionHandling<PaymentMethodAccount.aPQuickBatchGeneration>(row, row.APQuickBatchGeneration, ex);
			}
			else
			{
				cache.RaiseExceptionHandling<PaymentMethodAccount.aPQuickBatchGeneration>(row, row.APQuickBatchGeneration, null);
			}
		}

		protected virtual void PaymentMethodAccount_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			PXEntryStatus status = cache.GetStatus(e.Row);

			if (row.CashAccountID != null && status != PXEntryStatus.Inserted && status != PXEntryStatus.InsertedDeleted)
			{

				CustomerPaymentMethod cpm = PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.paymentMethodID, Equal<Required<CustomerPaymentMethod.paymentMethodID>>,
														And<CustomerPaymentMethod.cashAccountID, Equal<Required<CustomerPaymentMethod.cashAccountID>>>>>.SelectWindowed(this, 0, 1, row.PaymentMethodID, row.CashAccountID);
				if (cpm != null)
				{
					throw new PXException(Messages.PaymentMethodAccountIsInUseAndCantBeDeleted);
				}

				CashAccount cashAccount = CashAccount.PK.Find(this, row.CashAccountID);

				VerifyCashAccountLinkOrMethodCanBeDeleted(cashAccount);
			}
		}

		protected virtual void PaymentMethodAccount_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;

			if (row == null)
				return;

			if (!string.IsNullOrEmpty(row.PaymentMethodID) && row.CashAccountID.HasValue)
			{
				foreach (PXResult<PaymentMethodAccount, CashAccount> iRes in this.CashAccounts.Select())
				{
					PaymentMethodAccount paymentMethodAccount = iRes;

					if (!object.ReferenceEquals(row, paymentMethodAccount) && paymentMethodAccount.PaymentMethodID == row.PaymentMethodID &&
						row.CashAccountID == paymentMethodAccount.CashAccountID)
					{
						CashAccount cashAccount = iRes;
						throw new PXSetPropertyException(Messages.DuplicatedCashAccountForPaymentMethod, cashAccount.CashAccountCD);
					}
				}
			}
		}

		protected virtual void PaymentMethodAccount_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;

			PXDefaultAttribute.SetPersistingCheck<PaymentMethodAccount.aPLastRefNbr>(sender, e.Row, row.APAutoNextNbr == true ? PXPersistingCheck.NullOrBlank
																															  : PXPersistingCheck.Nothing);
			if (row.APAutoNextNbr == true && row.APLastRefNbr == null)
			{
				sender.RaiseExceptionHandling<PaymentMethodAccount.aPAutoNextNbr>(row, row.APAutoNextNbr, new PXSetPropertyException(Messages.SpecifyLastRefNbr, GL.Messages.ModuleAP));
			}

			if (row.ARAutoNextNbr == true && row.ARLastRefNbr == null)
			{
				sender.RaiseExceptionHandling<PaymentMethodAccount.aRAutoNextNbr>(row, row.ARAutoNextNbr, new PXSetPropertyException(Messages.SpecifyLastRefNbr, GL.Messages.ModuleAR));
			}

			PaymentMethodAccountHelper.VerifyQuickBatchGenerationOnRowPersisting(sender, row);

			CashAccount cashAccount = PXSelectReadonly<CashAccount,
												 Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);

			if (cashAccount != null && cashAccount.Active != true)
			{
				if(e.Operation == PXDBOperation.Update)
				{
					PaymentMethodAccount origPaymentAccount = PXSelectReadonly<PaymentMethodAccount,
						Where<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>,
							And<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>>>>.Select(this, row.PaymentMethodID, row.CashAccountID);

					if (origPaymentAccount?.CashAccountID == row.CashAccountID)
						return;
				}

				string errorMsg = string.Format(CA.Messages.CashAccountInactive, cashAccount.CashAccountCD.Trim());
				sender.RaiseExceptionHandling<PaymentMethodAccount.cashAccountID>(e.Row, cashAccount.CashAccountCD, new PXSetPropertyException(errorMsg, PXErrorLevel.Error));
			}
		}
		#endregion

		#region ProcessingCenter Events
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

				foreach (CCProcessingCenterPmntMethod it in this.ProcessingCenters.Select())
				{
					if (!Object.ReferenceEquals(it, row) && it.ProcessingCenterID == row.ProcessingCenterID)
					{
						isExist = true;
					}
				}

				if (isExist)
				{
					cache.RaiseExceptionHandling<CCProcessingCenterPmntMethod.processingCenterID>(e.Row, detID, new PXException(Messages.ProcessingCenterIsAlreadyAssignedToTheCard));
					e.Cancel = true;
				}
				else
				{
					CCProcessingCenter procCenter = GetProcessingCenterById(row.ProcessingCenterID);
					bool supported = CCProcessingFeatureHelper.IsPaymentHostedFormSupported(procCenter);

					if (supported)
					{
						if (row.IsDefault == false)
						{
							WebDialogResult result = ProcessingCenters.Ask(Messages.DefaultProcessingCenterConfirmation, MessageButtons.YesNo);
							if (result == WebDialogResult.Yes)
							{
								row.IsDefault = true;
							}
						}
					}
				}
			}
		}

		protected virtual void CCProcessingCenterPmntMethod_IsDefault_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (!this.ProcessingCenters.Any())
			{
				e.NewValue = true;
			}
		}
		#endregion

		#region Internal Auxillary Functions
		protected virtual void fillCreditCardDefaults()
		{
			PaymentMethodDetail det = new PaymentMethodDetail();
			det.DetailID = CreditCardAttributes.CCPID;
			det.EntryMask = CreditCardAttributes.MaskDefaults.CCPID;
			det.ValidRegexp = CreditCardAttributes.ValidationRegexp.CCPID;
			det.IsRequired = true;
			det.IsCCProcessingID = true;
			det.Descr = Messages.CCPID;
			det.UseFor = PaymentMethodDetailUsage.UseForARCards;
			det.OrderIndex = 1;
			det = (PaymentMethodDetail)this.Details.Cache.Insert(det);
			if (PXDBLocalizableStringAttribute.IsEnabled)
			{
				PXDBLocalizableStringAttribute.DefaultTranslationsFromMessage(this.Details.Cache, det, "Descr", Messages.CCPID);
			}
		}

		private CCProcessingCenter GetProcessingCenterById(string id)
		{
			CCProcessingCenter procCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>.Select(this, id);
			return procCenter;
		}

		private void CheckPaymentMethodDetailsIfProcessingReq(PaymentMethod pm)
		{
			if (pm?.ARIsProcessingRequired == false) return;

			foreach (CCProcessingCenterPmntMethod row in ProcessingCenters.Select())
			{
				CCProcessingCenter processingCenter = GetProcessingCenterById(row.ProcessingCenterID);
				if (CCProcessingFeatureHelper.IsFeatureSupported(processingCenter, CCProcessingFeature.ProfileManagement))
				{
					PaymentMethodDetail ccpid = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
						And<PaymentMethodDetail.isCCProcessingID, Equal<True>>>>.Select(this);
					if (ccpid == null)
					{
						throw new PXException(Messages.CCPaymentProfileIDNotSetUp);
					}
				}
			}
		}

		public virtual void VerifyAPRequirePaymentRefAndAPAdditionalProcessing(bool? apRequirePaymentRef, string apAdditionalProcessing)
		{
			if (apRequirePaymentRef == true ||
			    apAdditionalProcessing != CA.PaymentMethod.aPAdditionalProcessing.NotRequired)
			{
				foreach (PXResult<PaymentMethodAccount, CashAccount> row in CashAccounts.Select())
				{
					CashAccount account = row;

					if (account.UseForCorpCard == true)
					{
						throw new PXSetPropertyException(Messages.PaymentAndAdditionalProcessingSettingsHaveWrongValuesPaymentSide);
					}
				}
			}
		}

		public virtual void VerifyCashAccountLinkOrMethodCanBeDeleted(CashAccount cashAccount)
		{
			if (cashAccount.UseForCorpCard == true)
			{
				throw new PXException(Messages.CashAccountLinkOrMethodCannotBeDeleted);
			}
		}

		#endregion

		#region Private members
		private bool errorKey;
		#endregion
	}

	public static class CreditCardAttributes
	{
		public enum AttributeName
		{
			CardNumber = 0,
			ExpirationDate,
			NameOnCard,
			CCVCode,
			CCPID
		}

		public static string GetID(AttributeName aID)
		{
			return IDS[(int)aID];
		}

		public static string GetMask(AttributeName aID)
		{
			return EntryMasks[(int)aID];
		}

		public static string GetValidationRegexp(AttributeName aID)
		{
			return ValidationRegexps[(int)aID];
		}

		public const string CardNumber = "CCDNUM";
		public const string ExpirationDate = "EXPDATE";
		public const string NameOnCard = "NAMEONCC";
		public const string CVV = "CVV";
		public const string CCPID = "CCPID";

		public static class MaskDefaults
		{
			public const string CardNumber = "0000-0000-0000-0000";
			public const string ExpirationDate = "00/0000";
			public const string DefaultIdentifier = "****-****-****-0000";
			public const string CVV = "000";
			public const string CCPID = "";
		}

		public static class ValidationRegexp
		{
			public const string CardNumber = "";
			public const string ExpirationDate = "";
			public const string DefaultIdentifier = "";
			public const string CVV = "";
			public const string CCPID = "";
		}

		#region Private Members
		private static string[] IDS = { CardNumber, ExpirationDate, NameOnCard, CVV, CCPID };
		private static string[] EntryMasks = { MaskDefaults.CardNumber, MaskDefaults.ExpirationDate, String.Empty, MaskDefaults.CVV, MaskDefaults.CCPID };
		private static string[] ValidationRegexps = { ValidationRegexp.CardNumber, ValidationRegexp.ExpirationDate, String.Empty, ValidationRegexp.CVV, ValidationRegexp.CCPID };

		#endregion
	}
}


