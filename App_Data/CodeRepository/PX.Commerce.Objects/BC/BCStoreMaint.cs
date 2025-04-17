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
using System.Linq;
using PX.Api;
using PX.Data;
using PX.Commerce.Core;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Data.PushNotifications;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.CA;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.Common.Extensions;

namespace PX.Commerce.Objects
{
	public abstract class BCStoreMaint : BCBindingMaint
	{
		public PXSelect<BCBindingExt, Where<BCBindingExt.bindingID, Equal<Current<BCBinding.bindingID>>>> CurrentStore;

		[PXCopyPasteHiddenView]
		public PXSelect<ExportBCLocations, Where<ExportBCLocations.bindingID, Equal<Current<BCBinding.bindingID>>, And<ExportBCLocations.mappingDirection, Equal<BCMappingDirectionAttribute.export>>>> ExportLocations;

		public PXSelect<ImportBCLocations, Where<ImportBCLocations.mappingDirection, Equal<BCMappingDirectionAttribute.import>, And<ImportBCLocations.bindingID, Equal<Current<BCBinding.bindingID>>>>> ImportLocations;

		[PXVirtualDAC]
		public PXSelect<BCExternLocations, Where<BCExternLocations.bindingID, Equal<Current<BCBinding.bindingID>>>> ExternalLocations;

		public BCStoreMaint()
		{
			//this is needed to navigate GiftCertificate to the Non Stock item. Without this code, it will go to stock item if the field is empty.
			FieldDefaulting.AddHandler<PX.Objects.IN.InventoryItem.stkItem>(delegate (PXCache cache, PXFieldDefaultingEventArgs e) { e.NewValue = false; });
		}

		public override void Clear()
		{
			base.Clear();
		}

		#region BCBinding Events
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelector(typeof(Search<Branch.branchID>),
			typeof(Branch.acctName), typeof(Branch.baseCuryID),
			SubstituteKey = typeof(Branch.branchCD))]
		public virtual void BCBinding_BranchID_CacheAttached(PXCache sender) { }

		public virtual void _(Events.FieldDefaulting<BCBindingExt, BCBindingExt.taxSubstitutionListID> e)
		{
			BCBindingExt row = e.Row;

			if (row != null && base.CurrentBinding.Current.ConnectorType != null)
			{
				e.NewValue = CurrentBinding.Current.ConnectorType + BCObjectsConstants.TaxCodes;
			}
		}
		public virtual void _(Events.FieldDefaulting<BCBindingExt, BCBindingExt.taxCategorySubstitutionListID> e)
		{
			BCBindingExt row = e.Row;

			if (row != null && base.CurrentBinding.Current.ConnectorType != null)
			{
				e.NewValue = CurrentBinding.Current.ConnectorType + BCObjectsConstants.TaxClasses;
			}
		}
		public virtual void _(Events.FieldDefaulting<BCBindingExt, BCBindingExt.shippingCarrierListID> e)
		{
			BCBindingExt row = e.Row;

			if (row != null && base.CurrentBinding.Current.ConnectorType != null)
			{
				e.NewValue = CurrentBinding.Current.ConnectorType + BCObjectsConstants.Carriers;
			}
		}
		public virtual void _(Events.RowSelected<BCBinding> e)
		{
			BCBinding row = e.Row as BCBinding;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<BCBinding.isDefault>(e.Cache, e.Row, row.IsActive == true);
			//Check BCSyncStatus record
			BCSyncStatus result = PXSelectReadonly<BCSyncStatus, Where<BCSyncStatus.bindingID, Equal<Required<BCSyncStatus.bindingID>>>>.SelectWindowed(this, 0, 1, row.BindingID);
			Delete.SetEnabled(result == null ? true : false);
			//Logic for the for settings checker
			List<Tuple<PXCache, String>> fieldToCheck = new List<Tuple<PXCache, String>>();
			foreach (Type cacheType in this.Views.Caches)
			{
				PXCache cache = this.Caches[cacheType];
				//Check the cache data whether belongs to the same store
				if (string.Equals(cache.GetValue(cache.Current, nameof(BCBinding.BindingID)), row.BindingID) == false) continue;
				
				foreach (string fieldName in cache.BqlFields.Select(i => i.Name))
				{
					foreach (BCSettingsCheckerAttribute attr in cache.GetAttributesOfType<BCSettingsCheckerAttribute>(cache.Current, fieldName))
					{
						fieldToCheck.Add(Tuple.Create(cache, fieldName));
					}
				}
			}

			var entities = Entities.Select().RowCast<BCEntity>();
			foreach (BCEntity entity in entities)
			{
				foreach (Tuple<PXCache, String> tuple in fieldToCheck)
				{
					if (BCSettingsCheckerAttribute.CheckEntity(tuple.Item1, tuple.Item2, tuple.Item1.Current, entity.EntityType))
						BCSettingsCheckerAttribute.SetMandatory(tuple.Item1, tuple.Item2, tuple.Item1.Current, entity.IsActive);
				}
			}
			foreach (Tuple<PXCache, String> tuple in fieldToCheck)
			{
				PXPersistingCheck check = BCSettingsCheckerAttribute.GetMandatory(tuple.Item1, tuple.Item2, tuple.Item1.Current)
					? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;
				PXDefaultAttribute.SetPersistingCheck(tuple.Item1, tuple.Item2, tuple.Item1.Current, check);
			}

			if (CurrentStore.Current?.BindingID != null && e.Row?.BindingID == CurrentStore.Current?.BindingID)
				checkStoreVsBranchCurrencies();			

			var paymentEntity = entities?.FirstOrDefault(x => x.EntityType == BCEntitiesAttribute.Payment);
			FeeMappings.AllowDelete = FeeMappings.AllowInsert = FeeMappings.AllowUpdate = paymentEntity?.IsActive == true;
		}
		public virtual void _(Events.RowSelected<BCBindingExt> e)
		{
			BCBindingExt row = e.Row;
			if (row == null) return;

			var multiWarehouseFeatureEnabled = PXAccess.FeatureInstalled<FeaturesSet.warehouse>() == true || PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>() == true;
			ExportLocations.AllowSelect = ExportLocations.AllowInsert = ExportLocations.AllowDelete = ExportLocations.AllowUpdate = multiWarehouseFeatureEnabled && row.WarehouseMode == BCWarehouseModeAttribute.SpecificWarehouse;
			ImportLocations.AllowSelect = ImportLocations.AllowInsert = ImportLocations.AllowDelete = ImportLocations.AllowUpdate = true;

			if (HasSynchronizedRecord())
			{
				BCBindingExt original = e.Cache.GetOriginal(e.Row) as BCBindingExt;

				if (original != null)
				{
					if (row.Availability != original.Availability)
						PXUIFieldAttribute.SetWarning<BCBindingExt.availability>(e.Cache, e.Row, BCMessages.AvalabilityChangesWarning);
					else
						PXUIFieldAttribute.SetWarning<BCBindingExt.availability>(e.Cache, e.Row, null);
					if (row.AvailabilityCalcRule != original.AvailabilityCalcRule)
						PXUIFieldAttribute.SetWarning<BCBindingExt.availabilityCalcRule>(e.Cache, e.Row, BCMessages.AvalabilityChangesWarning);
					else
						PXUIFieldAttribute.SetWarning<BCBindingExt.availabilityCalcRule>(e.Cache, e.Row, null);
					if (row.NotAvailMode != original.NotAvailMode)
						PXUIFieldAttribute.SetWarning<BCBindingExt.notAvailMode>(e.Cache, e.Row, BCMessages.AvalabilityChangesWarning);
					else
						PXUIFieldAttribute.SetWarning<BCBindingExt.notAvailMode>(e.Cache, e.Row, null);
					if (row.WarehouseMode == BCWarehouseModeAttribute.SpecificWarehouse && !ExportLocations.Select().Any())
					{
						CurrentStore.Cache.RaiseExceptionHandling<BCBindingExt.warehouseMode>(row, row.WarehouseMode,
								new PXSetPropertyException<BCBindingExt.warehouseMode>(PX.Commerce.Core.BCMessages.SpecificWarehouseExportLocationsEmpty, PXErrorLevel.Error));
					}
					else
					{
						if (IsWarehouseUpdated() || row.WarehouseMode != original.WarehouseMode)
							PXUIFieldAttribute.SetWarning<BCBindingExt.warehouseMode>(e.Cache, e.Row, BCMessages.AvalabilityChangesWarning);
						else
							PXUIFieldAttribute.SetWarning<BCBindingExt.warehouseMode>(e.Cache, e.Row, null);
					}
				}
			}

			Boolean orderActive = Entities.Select().RowCast<BCEntity>().FirstOrDefault(r => r.EntityType == BCEntitiesAttribute.Order)?.IsActive == true;
			PXDefaultAttribute.SetPersistingCheck<BCBindingExt.guestCustomerID>(e.Cache, e.Row,
				(orderActive && (row.MultipleGuestAccounts ?? false)) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			//TaxSynchronization set to True, should check the DisableAutomaticTaxCalculation option in the order type
			bool shouldShowWarningMsg = false;
			if (row.TaxSynchronization == true && !string.IsNullOrEmpty(row.OrderType))
			{
				PX.Objects.SO.SOOrderType orderType = PXSelectReadonly<PX.Objects.SO.SOOrderType, Where<PX.Objects.SO.SOOrderType.orderType, Equal<Required<PX.Objects.SO.SOOrderType.orderType>>>>.Select(this, row.OrderType);
				if (orderType != null && orderType.DisableAutomaticTaxCalculation != true)
				{
					shouldShowWarningMsg = true;
				}
			}

			if (shouldShowWarningMsg)
			{
				ConnectorInfo connectorInfo = ConnectorHelper.GetConnectors().FirstOrDefault(c => c.ConnectorType == Bindings.Current?.ConnectorType);
				e.Cache.RaiseExceptionHandling<BCBindingExt.taxSynchronization>(row, row.TaxSynchronization,
					new PXSetPropertyException<BCBindingExt.taxSynchronization>(PX.Commerce.Core.BCMessages.DisableAutoTaxCalculationWarning, PXErrorLevel.Warning, row.OrderType, connectorInfo?.ConnectorName));
			}
			else
			{
				e.Cache.RaiseExceptionHandling<BCBindingExt.taxSynchronization>(row, row.TaxSynchronization, null);
			}

			if (CurrentStore.Current?.BindingID != null && e.Row?.BindingID == CurrentStore.Current?.BindingID)
				checkStoreVsBranchCurrencies();
		}

		public virtual void _(Events.RowUpdated<BCBinding> e)
		{
			BCBindingExt row = CurrentStore?.Current;
			if (row == null || row.BindingID != e.Row.BindingID) row = CurrentStore.Select();
			if (row == null) row = new BCBindingExt();

			CurrentStore.Update(row);
		}


		public virtual void _(Events.RowInserted<BCBinding> e)
		{
			bool dirty = CurrentStore.Cache.IsDirty;
			CurrentStore.Insert();
			CurrentStore.Cache.IsDirty = dirty;
		}

		public virtual void _(Events.RowPersisting<BCBinding> e)
		{
			BCBinding row = e.Row as BCBinding;
			if (row == null) return;
			Boolean anyError = false;

			BCBindingExt store = CurrentStore.Current;

			if (store != null && store.WarehouseMode == BCWarehouseModeAttribute.SpecificWarehouse)
			{
				if (!ExportLocations.Select().Any())
				{
					CurrentStore.Cache.RaiseExceptionHandling<BCBindingExt.warehouseMode>(store, store.WarehouseMode,
						new PXSetPropertyException<BCBindingExt.warehouseMode>(PX.Commerce.Core.BCMessages.SpecificWarehouseExportLocationsEmpty, PXErrorLevel.Error));
					throw new PXException(PX.Objects.Common.Messages.RecordCanNotBeSaved);
				}
			}

			foreach (BCEntity entity in Entities.Select())
			{
				Dictionary<string, string> keys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
				if (entity.EntityType == BCEntitiesAttribute.Payment)
				{
					foreach (BCPaymentMethods item in PaymentMethods.Select())
					{
						if (keys.ContainsKey(item.StorePaymentMethod + item.StoreCurrency))
						{
							throw new PXException(BCMessages.PaymentMethodDuplicated, item.StorePaymentMethod ?? string.Empty);
						}
						else
							keys.Add(item.StorePaymentMethod + item.StoreCurrency, item.StorePaymentMethod);

						if (item.Active == true)
						{
							if (item.CashAccountID == null || string.IsNullOrEmpty(item.PaymentMethodID))
							{
								anyError = true;

								PaymentMethods.Cache.RaiseExceptionHandling<BCPaymentMethods.cashAccountID>(item, item.Active,
									new PXSetPropertyException<BCPaymentMethods.cashAccountID>(PX.Commerce.Core.BCMessages.PaymentMethodRequired, PXErrorLevel.RowError));
							}
						}
					}

					if (anyError) throw new PXException(PX.Objects.Common.Messages.RecordCanNotBeSaved);
				}
				if (entity.EntityType == BCEntitiesAttribute.Order)
				{
					foreach (BCShippingMappings item in ShippingMappings.Select())
					{
						var key = $"{item.ShippingZone ?? string.Empty}#{item.ShippingMethod}";
						if (keys.ContainsKey(key))
						{
							throw new PXException(BCMessages.OrderShippingMappingDuplicated, item.ShippingZone ?? string.Empty, item.ShippingMethod);
						}
						else
							keys.Add(key, item.ShippingMethod);
						if (item.Active == true)
						{
							if (string.IsNullOrEmpty(item.CarrierID))
							{
								anyError = true;

								ShippingMappings.Cache.RaiseExceptionHandling<BCShippingMappings.carrierID>(item, item.Active,
									new PXSetPropertyException<BCShippingMappings.carrierID>(PX.Commerce.Core.BCMessages.OrderShippingShipViaRequired, PXErrorLevel.RowError));
							}

						}
					}
					if (anyError) throw new PXException(PX.Objects.Common.Messages.RecordCanNotBeSaved);
				}
			}
		}

		public virtual void _(Events.FieldUpdated<BCBinding, BCBinding.branchID> e)
		{
			var row = e.Row;
			if (row == null || e.OldValue == null) return;
			foreach (BCEntity entity in Entities.Select())
			{
				if (entity.EntityType == BCEntitiesAttribute.Payment)
				{
					bool found = false;
					foreach (BCPaymentMethods item in PaymentMethods.Select())
					{
						if (item.Active == true || !string.IsNullOrEmpty(item.PaymentMethodID) || item.CashAccountID != null)
						{
							found = true;
							break;
						}
					}
					if (found)
					{
						if (PaymentMethods.Ask(BCCaptions.Stores, BCMessages.BranchChangeConfirmation, MessageButtons.YesNo) == WebDialogResult.Yes)
						{
							foreach (BCPaymentMethods item in PaymentMethods.Select())
							{
								item.PaymentMethodID = null;
								item.CashAccountID = null;
								item.ReleasePayments = false;
								item.Active = false;
								PaymentMethods.Cache.Update(item);
							}
						}
						else
							row.BranchID = e.OldValue.ToInt();
					}

					break;
				}
			}
		}

		public virtual void _(Events.FieldUpdated<BCBindingExt, BCBindingExt.importOrderRisks> e)
		{
			var row = e.Row;
			if (row == null || e.NewValue == null) return;
			if (!bool.Parse(e.NewValue.ToString()))
			{
				e.Cache.SetValueExt<BCBindingExt.holdOnRiskStatus>(e.Row, null);
			}
		}
		public virtual void _(Events.FieldUpdated<BCBindingExt, BCBindingExt.warehouseMode> e)
		{
			if (e.Row.WarehouseMode == BCWarehouseModeAttribute.AllWarehouse)
			{
				foreach (ExportBCLocations loc in ExportLocations.Select())
				{
					ExportLocations.Delete(loc);
				}
			}
			ExportLocations.View.RequestRefresh();
		}

		public virtual void _(Events.FieldVerifying<BCBindingExt, BCBindingExt.postDiscounts> e)
		{
			if (e.NewValue?.ToString() == BCPostDiscountAttribute.DocumentDiscount)
			{
				if (PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() == false)
					throw new PXSetPropertyException<BCBindingExt.postDiscounts>(BCMessages.DocumentDiscountFeatureDisabled);
			}
		}
		public virtual void _(Events.FieldDefaulting<BCBindingExt, BCBindingExt.postDiscounts> e)
		{
			e.NewValue = PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() ? BCPostDiscountAttribute.DocumentDiscount : BCPostDiscountAttribute.LineDiscount;
			e.Cancel = true;
		}

		public virtual void _(Events.FieldSelecting<ExportBCLocations, BCLocations.externalLocationID> e)
		{
			ExportBCLocations row = e.Row as ExportBCLocations;
			if (row != null)
			{
				var externalLocationsList = ExternalLocations.Select().RowCast<BCExternLocations>()?.ToList();

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 100, true,
															 typeof(ExportBCLocations.externalLocationID).Name, false, 1, null,
															 externalLocationsList.Select(t => t.ExternLocationValue).ToArray(), externalLocationsList.Select(t => t.ExternLocationName).ToArray(), false, null);
			}
		}

		public virtual void _(Events.FieldSelecting<ImportBCLocations, BCLocations.externalLocationID> e)
		{
			ImportBCLocations row = e.Row as ImportBCLocations;
			if (row != null)
			{
				var externalLocationsList = ExternalLocations.Select().RowCast<BCExternLocations>()?.ToList();

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 100, true,
															 typeof(ImportBCLocations.externalLocationID).Name, false, 1, null,
															 externalLocationsList.Select(t => t.ExternLocationValue).ToArray(), externalLocationsList.Select(t => t.ExternLocationName).ToArray(), false, null);
			}
		}

		public virtual void _(Events.FieldUpdated<ExportBCLocations, ExportBCLocations.locationID> e)
		{
			checkWarehouseLocation();
		}
		public virtual void _(Events.RowPersisting<ExportBCLocations> e)
		{
			checkWarehouseLocation();
		}

		public virtual void _(Events.FieldUpdated<ImportBCLocations, ImportBCLocations.locationID> e)
		{
			checkWarehouseLocation();
		}
		public virtual void _(Events.RowPersisting<ImportBCLocations> e)
		{
			checkWarehouseLocation();
		}

		public override void _(Events.FieldUpdated<BCEntity, BCEntity.isActive> e)
		{
			base._(e);

			BCEntity row = e.Row as BCEntity;
			if (row == null || e.NewValue == null || e.Row?.EntityType != BCEntitiesAttribute.Payment) return;

			if (((bool)e.NewValue) == true)
			{
				PaymentMethods.Select();
			}
		}

		#endregion

		#region PaymentMethods
		public PXSelect<BCPaymentMethods, Where<BCPaymentMethods.bindingID, Equal<Current<BCBinding.bindingID>>>,
			OrderBy<Desc<BCPaymentMethods.active, Asc<BCPaymentMethods.storePaymentMethod>>>> PaymentMethods;
		[PXCopyPasteHiddenView]
		public PXSelect<BCBigCommercePayment> BigCommerceMethods;

		public IEnumerable bigCommerceMethods()
		{
			BCBinding binding = Bindings.Current;
			if (binding == null || binding.BindingID == null || binding.BindingID <= 0) yield break;

			Boolean anyFound = false;
			foreach (BCBigCommercePayment payment in BigCommerceMethods.Cache.Cached)
			{
				anyFound = true;
				yield return payment;
			}

			if (!anyFound)
			{
				IEnumerable<IPaymentMethod> paymentMethods = new List<IPaymentMethod>();
				var externalPaymentMethods = ConnectorHelper.GetConnector(binding.ConnectorType)?.GetExternalInfo<IPaymentMethod>(BCObjectsConstants.BCPayment, binding.BindingID);
				if (externalPaymentMethods != null) paymentMethods = paymentMethods.Concat(externalPaymentMethods);

				var defaultPaymentMethods = ConnectorHelper.GetConnector(binding.ConnectorType)?.GetDefaultPaymentMethods<IPaymentMethod>(binding.BindingID);
				if (defaultPaymentMethods != null) paymentMethods = paymentMethods.Concat(defaultPaymentMethods);

				Boolean lastDirty = BigCommerceMethods.Cache.IsDirty;

				foreach (var method in paymentMethods)
				{
					if (!string.IsNullOrEmpty(method.Name))
					{
						BCBigCommercePayment inserted = BigCommerceMethods.Insert(new BCBigCommercePayment() { CreatePaymentfromOrder = method.CreatePaymentfromOrder, Name = method.Name, Currency = method.Currency });
						BigCommerceMethods.Cache.SetStatus(inserted, PXEntryStatus.Held);
						yield return inserted;
					}
				}
				BigCommerceMethods.Cache.IsDirty = lastDirty;
			}
		}

		public IEnumerable paymentMethods()
		{
			BCBinding binding = Bindings.Current;
			BCBindingExt store = CurrentStore.Current;
			if (binding == null || binding.BindingID == null || binding.BindingID <= 0 || store == null) yield break;
			var defaultCurrency = store.DefaultStoreCurrency ?? (binding.BranchID != null ? PX.Objects.GL.Branch.PK.Find(this, binding.BranchID)?.BaseCuryID : null);

			bool lastDirty = this.PaymentMethods.Cache.IsDirty;

			PXView view = new PXView(this, false, PaymentMethods.View.BqlSelect);
			List<BCPaymentMethods> stored = new List<BCPaymentMethods>();
			foreach (BCPaymentMethods row in view.SelectMulti())
			{
				if (String.IsNullOrEmpty(row.StoreCurrency) && defaultCurrency != null)
				{
					row.StoreCurrency = defaultCurrency;
					PaymentMethods.Update(row);
				}
				stored.Add(row);
			}

			foreach (BCBigCommercePayment paymentMethod in BigCommerceMethods.Select())
			{
				var matchedResult = stored.FirstOrDefault(x => x.StorePaymentMethod != null
					&& string.Equals(x.StoreCurrency, paymentMethod.Currency, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(x.StorePaymentMethod, paymentMethod.Name, StringComparison.OrdinalIgnoreCase));
				if (matchedResult == null)
				{
					BCPaymentMethods entry = new BCPaymentMethods()
					{
						CreatePaymentFromOrder = paymentMethod.CreatePaymentfromOrder,
						StorePaymentMethod = paymentMethod.Name,
						StoreCurrency = paymentMethod.Currency,
						BindingID = binding.BindingID
					};
					entry = PaymentMethods.Insert(entry);

					if (entry != null) yield return entry;
				}
			}
			foreach (BCPaymentMethods result in stored)
			{
				yield return result;
			}

			this.PaymentMethods.Cache.IsDirty = lastDirty;
		}

		public virtual void _(Events.FieldUpdated<BCPaymentMethods, BCPaymentMethods.paymentMethodID> e)
		{
			var row = e.Row;
			if (row == null || e.NewValue == null) return;
			row.CashAccountID = null;
			row.ProcessingCenterID = null;
		}
		public virtual void _(Events.RowUpdating<BCPaymentMethods> e)
		{
			if (e.NewRow?.PaymentMethodID != null) CheckCreditCardPaymentValid(e.Cache, e.NewRow, e.NewRow.ProcessingCenterID ?? "");

			if (e.NewRow.ProcessingCenterID != null) e.NewRow.CreatePaymentFromOrder = false;
		}
		public virtual void _(Events.FieldUpdated<BCPaymentMethods, BCPaymentMethods.storeCurrency> e)
		{
			var row = e.Row;
			if (row == null || e.NewValue == null) return;
			row.CashAccountID = null;
			row.ProcessingCenterID = null;
		}

	

		public virtual void _(Events.RowSelected<BCPaymentMethods> e)
		{
			if (e.Row == null) return;

			bool isProcessingCenterNull = e.Row?.ProcessingCenterID == null;
			PaymentMethod paymentMethod = PaymentMethod.PK.Find(this, e.Row?.PaymentMethodID);
			bool enableProcessingCenterIdField = (paymentMethod?.PaymentType == PaymentMethodType.CreditCard ||
								(paymentMethod?.PaymentType != PaymentMethodType.CreditCard && !isProcessingCenterNull));

			PXUIFieldAttribute.SetEnabled<BCPaymentMethods.processRefunds>(e.Cache, e.Row, isProcessingCenterNull);
			PXUIFieldAttribute.SetEnabled<BCPaymentMethods.createPaymentFromOrder>(e.Cache, e.Row, isProcessingCenterNull);
			PXUIFieldAttribute.SetEnabled<BCPaymentMethods.processingCenterID>(e.Cache, e.Row, enableProcessingCenterIdField);
			
			isValidPaymentMethodCurrency(e.Cache, e.Row, e.Row.StoreCurrency);
		}
		public virtual void _(Events.FieldUpdated<BCPaymentMethods, BCPaymentMethods.processingCenterID> e)
		{
			if (e.NewValue != null)
			{
				e.Row.ProcessRefunds = true;
			}
		}
		public virtual void _(Events.FieldVerifying<BCPaymentMethods, BCPaymentMethods.processingCenterID> e)
		{
			String procCenter = e.NewValue as String;
			if (procCenter != null)
			{
				ARSetup setup = PXSelect<ARSetup>.Select(this);
				if (setup.IntegratedCCProcessing == false)
				{
					throw new PXSetPropertyException<BCPaymentMethods.processingCenterID>(BCObjectsMessages.IntegratedCCProcessing, procCenter);
				}
			}
		}
		public virtual void _(Events.FieldVerifying<BCPaymentMethods, BCPaymentMethods.storePaymentMethod> e)
		{
			String str = e.NewValue as String;
			if (str != null)
			{
				e.NewValue = str.Trim();
			}
		}
		public virtual void _(Events.FieldVerifying<BCPaymentMethods, BCPaymentMethods.storeCurrency> e)
		{
			String str = e.NewValue as String;
			if (str != null)
			{
				e.NewValue = str.Trim();
			}

			e.Cancel = !isValidPaymentMethodCurrency(e.Cache, e.Row, str);			
		}
		public virtual void _(Events.FieldVerifying<BCPaymentMethods, BCPaymentMethods.storeOrderPaymentMethod> e)
		{
			String str = e.NewValue as String;
			if (str != null)
			{
				e.NewValue = str.Trim();
			}
		}
		#endregion

		#region Shipping Mapping
		public PXSelect<BCShippingMappings, Where<BCShippingMappings.bindingID, Equal<Current<BCBinding.bindingID>>>,
			OrderBy<Desc<BCShippingMappings.active, Asc<BCShippingMappings.shippingZone>>>> ShippingMappings;
		[PXCopyPasteHiddenView]
		public PXSelect<BCShippingZones, Where<BCShippingMappings.bindingID, Equal<Current<BCBinding.bindingID>>>> ShippingZones;

		public IEnumerable shippingZones()
		{
			BCBinding binding = Bindings.Current;
			if (binding == null || binding.BindingID == null || binding.BindingID <= 0)
				yield break;
			Boolean anyFound = false;
			foreach (BCShippingZones zone in ShippingZones.Cache.Cached)
			{
				anyFound = true;
				yield return zone;
			}

			Boolean lastDirty = ShippingZones.Cache.IsDirty;
			if (!anyFound)
			{
				IEnumerable<IShippingZone> zones = new List<IShippingZone>();
				IEnumerable<IShippingZone> externalZones = ConnectorHelper.GetConnector(binding.ConnectorType)?.GetExternalInfo<IShippingZone>(BCObjectsConstants.BCShippingZone, binding.BindingID);
				if (externalZones != null) zones = zones.Concat(externalZones);

				var defaultShippingZones = ConnectorHelper.GetConnector(binding.ConnectorType)?.GetDefaultShippingMethods<IShippingZone>(binding.BindingID);
				if (defaultShippingZones != null) zones = zones.Concat(defaultShippingZones);

				if (zones != null && zones.Count() > 0)
				{
					foreach (var zone in zones)
					{
						if (zone.ShippingMethods != null && zone.ShippingMethods.Count > 0 && zone.Enabled == true)
						{
							foreach (var method in zone.ShippingMethods)
							{
								if (method?.Enabled == true)
									yield return AddShippingZoneItem(binding.BindingID, zone.Name, method.Name);
							}
						}
					}
				}
			}
			ShippingZones.Cache.IsDirty = lastDirty;
			yield break;
		}

		public IEnumerable shippingMappings()
		{
			PXView pxView = new PXView(this, false, ShippingMappings.View.BqlSelect);
			List<BCShippingMappings> shippingMappingList = pxView.SelectMulti().Select(r => { var item = (BCShippingMappings)r; return item; }).ToList();

			foreach (BCShippingZones zone in ShippingZones.Select())
			{
				AddShippingMappingItem(shippingMappingList, zone);
			}

			return shippingMappingList ?? new List<BCShippingMappings>();

		}

		protected virtual BCShippingZones AddShippingZoneItem(int? bindingID, string shippingZone, string shippingMethod)
		{
			bool lastDirty = ShippingZones.Cache.IsDirty;

			BCShippingZones row = new BCShippingZones()
			{
				BindingID = bindingID,
				ShippingZone = shippingZone,
				ShippingMethod = shippingMethod
			};
			row = ShippingZones.Insert(row);

			ShippingZones.Cache.SetStatus(row, PXEntryStatus.Held);
			ShippingZones.Cache.IsDirty = lastDirty;

			return row;
		}
		protected virtual void AddShippingMappingItem(List<BCShippingMappings> shippingMappingList, BCShippingZones zone)
		{
			bool lastDirty = ShippingMappings.Cache.IsDirty;
			var existedItem = shippingMappingList.FirstOrDefault(x => x.ShippingZone?.ToUpper() == zone.ShippingZone?.ToUpper() && string.Equals(x.ShippingMethod, zone.ShippingMethod, StringComparison.OrdinalIgnoreCase));
			if (existedItem == null)
			{
				BCShippingMappings row = new BCShippingMappings()
				{
					BindingID = zone.BindingID,
					ShippingZone = zone.ShippingZone,
					ShippingMethod = zone.ShippingMethod,
				};
				row = ShippingMappings.Insert(row);
				shippingMappingList.Add(row);
			}
			else
			{
				existedItem.ShippingZone = zone.ShippingZone;
				existedItem.ShippingMethod = zone.ShippingMethod;
				ShippingMappings.Cache.Update(existedItem);
			}
			ShippingMappings.Cache.IsDirty = lastDirty;
		}

		#endregion

		#region FeeMappings	
		public SelectFrom<BCFeeMapping>
			.Where<BCFeeMapping.bindingID.IsEqual<BCPaymentMethods.bindingID.FromCurrent>
			.And<BCFeeMapping.paymentMappingID.IsEqual<BCPaymentMethods.paymentMappingID.FromCurrent>>>.View FeeMappings;
		#endregion

		#region Persist

		public override void Persist()
		{
			//Skip push notifications to remove extra extra messages pushed
			using (new SuppressPushNotificationsScope())
			{

				if (HasSynchronizedRecord())
				{
					BCBinding binding = Bindings.Current;
					BCBindingExt store = CurrentStore.Current;
					BCBindingExt original = CurrentStore.Cache.GetOriginal(store) as BCBindingExt;

					bool resync = false;
					if (IsFieldUpdated(CurrentStore.Cache, store, original, nameof(BCBindingExt.availability))
						|| IsFieldUpdated(CurrentStore.Cache, store, original, nameof(BCBindingExt.availabilityCalcRule))
						|| IsFieldUpdated(CurrentStore.Cache, store, original, nameof(BCBindingExt.notAvailMode))
						|| IsFieldUpdated(CurrentStore.Cache, store, original, nameof(BCBindingExt.warehouseMode))
						|| IsWarehouseUpdated())
						resync = true;

					base.Persist();

					if (resync)
					{
						PXLongOperation.StartOperation(this, delegate ()
						{
							BCEntityMaint.DoResetSync(binding.ConnectorType, binding.BindingID, BCEntitiesAttribute.ProductAvailability, false);
						});
					}
				}
				else
					base.Persist();
			}
		}
		#endregion

		#region Check Modifications
		protected bool IsFieldUpdated(PXCache cache, object row, object original, string field)
		{
			var oldValue = cache.GetValue(original, field);
			var newValue = cache.GetValue(row, field);
			if (oldValue != null && newValue != null)
				return oldValue.ToString() != newValue.ToString();
			return false;
		}

		protected bool IsWarehouseUpdated()
		{
			var originalLocation = ExportLocations.Cache.GetOriginal(ExportLocations.Current);
			if (originalLocation != null)
			{
				BCLocations newRow = (BCLocations)ExportLocations.Current;
				BCLocations oldRow = (BCLocations)originalLocation;
				return oldRow.LocationID != newRow.LocationID || oldRow.SiteID != newRow.SiteID;

			}
			return ExportLocations.Cache.IsInsertedUpdatedDeleted;
		}

		protected void checkWarehouseLocation()
		{
			foreach (var item in ExportLocations.Select().Select(x => x.GetItem<ExportBCLocations>()).ToLookup(x => x.SiteID))
			{
				if (item.Count() > 1 && (item.Any(i => i.LocationID == null) || item.GroupBy(g => g.LocationID).Where(x => x.Count() > 1).Any()))
					throw new PXException(BCObjectsMessages.DuplicateLocationRows);
			}
			foreach (var item in ImportLocations.Select().Select(x => x.GetItem<ImportBCLocations>()).ToLookup(x => x.ExternalLocationID))
			{
				if (item.Count() > 1)
					throw new PXException(BCObjectsMessages.DuplicateLocationRows);
			}
		}

		protected bool HasSynchronizedRecord()
		{
			BCSyncStatus status = PXSelect<BCSyncStatus,
											Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
												And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
												And<BCSyncStatus.pendingSync, Equal<False>,
												And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>>>>>>.Select(this, BCEntitiesAttribute.ProductAvailability);
			return status != null;
		}

		/// <summary>
		/// Validates settings for credit card payment method and processing center configuration for all payment methods.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="row"></param>
		/// <param name="processingCenterID">New processing Center Value</param>
		protected void CheckCreditCardPaymentValid(PXCache cache, BCPaymentMethods row, string processingCenterID)
		{
			var resultSet = PXSelectJoin<
				PaymentMethod,
				LeftJoin<CCProcessingCenterPmntMethod,
					On<CCProcessingCenterPmntMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>>,
					Where<PaymentMethod.paymentMethodID, Equal<Required<BCPaymentMethods.paymentMethodID>>>>
				.Select(this, row.PaymentMethodID)
				.ToArray();

			PaymentMethod paymentMethod = resultSet?.FirstOrDefault();

			if (paymentMethod?.PaymentType == PaymentMethodType.CreditCard)
			{
				//One possible error is dependent of this list's quantity.
				var listOfCCProcessingCenters = resultSet
													.RowCast<CCProcessingCenterPmntMethod>()
													.Where(processingCenter => processingCenter.IsActive == true);
				bool hasMatchingSelectedProcessingCenter = listOfCCProcessingCenters
															.Where(processingCenter => processingCenter.ProcessingCenterID == processingCenterID)
															.Count() > 0;
				ARSetup arSetup = PXSelect<ARSetup>.Select(this);

				if (arSetup?.IntegratedCCProcessing != true)
				{
					cache.RaiseExceptionHandling<BCPaymentMethods.processingCenterID>(row, processingCenterID,
						new PXSetPropertyException(BCObjectsMessages.IntegratedCCProcessingSync, PXErrorLevel.Error, row.ProcessingCenterID, row.PaymentMethodID));
				}
				else if (paymentMethod?.ARIsProcessingRequired != true)
				{
					cache.RaiseExceptionHandling<BCPaymentMethods.processingCenterID>(row, processingCenterID,
						new PXSetPropertyException(BCObjectsMessages.MissingIntegratedCCProcessing, PXErrorLevel.Error, row.PaymentMethodID));
				}
				else if (!hasMatchingSelectedProcessingCenter)
				{
					string message = (listOfCCProcessingCenters.Count() > 0) ?
						BCObjectsMessages.MissingProcessingCenterCreditCardExists : BCObjectsMessages.MissingProcessingCenterCreditCard;

					cache.RaiseExceptionHandling<BCPaymentMethods.processingCenterID>(row, processingCenterID,
						new PXSetPropertyException(message, PXErrorLevel.Error, row.PaymentMethodID));
				}
			}
			else if (row.ProcessingCenterID != null)
			{
				cache.RaiseExceptionHandling<BCPaymentMethods.processingCenterID>(row, processingCenterID,
					new PXSetPropertyException(BCObjectsMessages.RemoveProcessingCenterNotCreditCard, PXErrorLevel.Error, row.PaymentMethodID));
			}
		}

		/// <summary>
		/// Validate that the currency has been set in the ERP.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="row"></param>
		/// <param name="currencyId"></param>
		/// <returns></returns>
		public virtual bool isValidPaymentMethodCurrency(PXCache cache, BCPaymentMethods row, string currencyId)
		{

			if (string.IsNullOrEmpty(currencyId))
				return true;

			if (row != null && row.BindingID.HasValue && row.BindingID.Value == CurrentStore?.Current?.BindingID)
			{
				var systemCurrency = PX.Objects.CM.Currency.PK.Find(this, currencyId);
				if (systemCurrency != null)
					return true;

				var warningMessage = PXMessages.LocalizeFormatNoPrefix(BCMessages.CurrencyHasNotBeenSetup, currencyId);
				Boolean paymentActive = Entities.Select().RowCast<BCEntity>().FirstOrDefault(r => r.EntityType == BCEntitiesAttribute.Payment)?.IsActive == true;
				var errorLevel = paymentActive ? PXErrorLevel.Error : PXErrorLevel.Warning;

				cache.RaiseExceptionHandling<BCPaymentMethods.storeCurrency>(row, currencyId,
					new PXSetPropertyException<BCPaymentMethods.storeCurrency>(BCMessages.CurrencyHasNotBeenSetup, errorLevel, currencyId));

				return false;
			}

			return true;
		}

		public virtual void checkStoreVsBranchCurrencies()
		{
			var currentStore = CurrentStore.Select().RowCast<BCBindingExt>().FirstOrDefault();
			
			if (CurrentBinding.Current != null && currentStore != null && !String.IsNullOrEmpty(currentStore.DefaultStoreCurrency))
			{
				PX.Objects.GL.Branch branch = PX.Objects.GL.Branch.PK.Find(this, CurrentBinding.Current.BranchID);
				var storeDefaultCurrency = currentStore.DefaultStoreCurrency;

				string warningMessage = null;
				if (storeDefaultCurrency != null && branch?.BaseCuryID != storeDefaultCurrency && CurrentBinding.Current.IsActive == true)
				{
					warningMessage = PXMessages.LocalizeFormat(BCMessages.BranchCurrencyDifferentFromExternalSystemDefaultCurrency, branch?.BaseCuryID, storeDefaultCurrency);
				}

				//display the warninig on the order settings tab only if the multibranch or multicompany features are enabled
				if (PXAccess.FeatureInstalled<FeaturesSet.multiCompany>() == true || PXAccess.FeatureInstalled<FeaturesSet.branch>() == true)
				{
					PXUIFieldAttribute.SetWarning<BCBinding.branchID>(CurrentBinding.Cache, CurrentBinding.Current, warningMessage);
				}

				PXUIFieldAttribute.SetWarning<BCBindingExt.defaultStoreCurrency>(CurrentStore.Cache, CurrentStore.Current, warningMessage);
			}			
		}

		/// <summary>
		/// Check for duplicate fee mappings in Fees mapping form.
		/// </summary>
		/// <exception cref="PXSetPropertyException">This exception is thrown if there are duplicated values.</exception>
		public virtual void CheckForDuplicatedFees(PXCache cache, BCFeeMapping row)		
		{
			var duplicatesFeeMappings = SelectFrom<BCFeeMapping>.Where<BCFeeMapping.bindingID.IsEqual<@P.AsInt>>.View.Select(this, row.BindingID)
								.RowCast<BCFeeMapping>()
								.GroupBy(feeMapping => new { feeMapping.PaymentMappingID, FeeType = feeMapping.FeeType.ToLower() })
								.Where(group => group.Count() > 1);
			if (duplicatesFeeMappings.Any() == false) return ;

			string storePaymentMethod = PaymentMethods.Select()
											.RowCast<BCPaymentMethods>()
											.FirstOrDefault(x => x.PaymentMappingID == row.PaymentMappingID)?.StorePaymentMethod; 
			string message = PXMessages.LocalizeFormat(BCMessages.FeeTypeAlreadyExistsInPaymentMethod, row.FeeType, storePaymentMethod);
			cache.RaiseExceptionHandling<BCFeeMapping.feeType>(row, row.FeeType, new PXSetPropertyException<BCFeeMapping.feeType>(message, PXErrorLevel.RowError));
		}
		#endregion

		#region GetExternalLocations
		public IEnumerable externalLocations()
		{
			BCBinding binding = Bindings.Current;
			if (binding == null) yield break;

			bool found = false;
			//Checking cache first
			foreach(BCExternLocations row in ExternalLocations.Cache.Cached)
			{
				found = true;
				yield return row;
			}

			//Get the Locations from external platform
			if (!found)
			{
				ExternalLocations.Cache.Clear();
				foreach (var externalLocation in GetExternalLocationList())
				{
					BCExternLocations item = ExternalLocations.Cache.Insert(new BCExternLocations() { BindingID = binding.BindingID, ExternLocationName = externalLocation.Item2, ExternLocationValue = externalLocation.Item1, Active = true }) as BCExternLocations;
					ExternalLocations.Cache.SetStatus(item, PXEntryStatus.Held);
				}

				foreach (BCExternLocations row in ExternalLocations.Cache.Cached)
					yield return row;

				ExternalLocations.Cache.IsDirty = false;
			}
		}
		protected virtual List<Tuple<String, String>> GetExternalLocationList()
		{
			BCBinding binding = Bindings.Current;
			List<Tuple<String, String>> fieldsList = new List<Tuple<String, String>>();
			if (binding == null || binding.BindingID == null || binding.BindingID <= 0)
				return fieldsList;
			var locations = ConnectorHelper.GetConnector(binding.ConnectorType)?.GetExternalInfo<ILocation>(BCObjectsConstants.BCInventoryLocation, binding.BindingID);
			foreach (var item in locations?.Where(x => x?.Active == true)?.ToList() ?? new List<ILocation>())
			{
				fieldsList.Add(Tuple.Create(item.Id?.ToString(), item.Name));
			}
			return fieldsList;
		}
		#endregion

		#region Events
		public virtual void _(Events.RowPersisting<BCBindingExt> e)
		{
			var row = (BCBindingExt)e.Row;
			if (row == null) return;

			object discounts = row.PostDiscounts;
			e.Cache.RaiseFieldVerifying<BCBindingExt.postDiscounts>(row, ref discounts);
			if (row.MultipleGuestAccounts == true && row.GuestCustomerID == null)
			{
				e.Cache.RaiseExceptionHandling<BCBindingExt.guestCustomerID>(row, null, new PXSetPropertyException(BCMessages.GuestAccountNotSpecified));
			}
		}

		public virtual void _(Events.RowPersisting<BCFeeMapping> e)
		{
			if (e.Row == null) return;

			CheckForDuplicatedFees(e.Cache, e.Row);
		}

		public virtual void _(Events.RowPersisting<BCPaymentMethods> e)
		{
			if (e.Row == null) return;
			BCPaymentMethods row = e.Row;

			e.Cancel = !isValidPaymentMethodCurrency(e.Cache, row, row.StoreCurrency);
			if (e.Cancel) return;

			if (row.PaymentMethodID != null)	CheckCreditCardPaymentValid(e.Cache, row, row.ProcessingCenterID);
			if (row.StorePaymentMethod == BCConstants.NoneGateway && row.Active == true)
			{
				PX.Objects.CA.PaymentMethod pm = PX.Objects.CA.PaymentMethod.PK.Find(this, row.PaymentMethodID);
				if (pm?.PaymentType != PX.Objects.CA.PaymentMethodType.CashOrCheck)
					e.Cache.RaiseExceptionHandling<BCPaymentMethods.paymentMethodID>(row, null,
						new PXSetPropertyException(BCMessages.NoneGatewayWithoutCashAccount, PXErrorLevel.Error, row.PaymentMethodID));
		}
		}

		#endregion
		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[MultipleOrderType()]
		public virtual void _(Events.CacheAttached<BCBindingExt.otherSalesOrderTypes> e) { }
		#endregion
	}
}
