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
using System.Collections.Generic;
using System.Linq;

using PX.Common;
using PX.Data;

using PX.Objects.Common;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Discount.Mappers;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.DR.Descriptor;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.EP;
using static PX.Objects.AR.ARReleaseProcess;
using Amount = PX.Objects.AR.ARReleaseProcess.Amount;
using static PX.Objects.AR.ARSalesPriceMaint;

namespace PX.Objects.DR
{
	public class DRSingleProcess : DRProcess, ISalesPriceProvider, ISingleScheduleViewProvider, IDRDataProvider
	{
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<DRSchedule.curyInfoID>>>> CuryInfo;
		public PXSelect<Location, Where<Location.locationID, Equal<Current<DRSchedule.bAccountLocID>>>> BAccountLocation;
		public PXSelectJoin<ARTran,
			InnerJoin<InventoryItem,
				On<ARTran.inventoryID, Equal<InventoryItem.inventoryID>>,
			InnerJoin<DRDeferredCode,
				On<ARTran.deferredCode, Equal<DRDeferredCode.deferredCodeID>>,
			LeftJoin<INComponent,
				On<InventoryItem.inventoryID, Equal<INComponent.inventoryID>>,
			LeftJoin<ComponentINItem,
				On<INComponent.componentID, Equal<ComponentINItem.inventoryID>>,
			LeftJoin<ComponentDeferredCode,
				On<INComponent.deferredCode, Equal<ComponentDeferredCode.deferredCodeID>>>>>>>,
			Where<ARTran.tranType, Equal<Current<DRSchedule.docType>>,
				And<ARTran.refNbr, Equal<Current<DRSchedule.refNbr>>>>,
			OrderBy<Asc<ARTran.tranType,
				Asc<ARTran.refNbr,
				Asc<ARTran.lineNbr,
				Asc<INComponent.amtOptionASC606>>>>>> ARTransactionWithItems;

		public class ComponentINItem : InventoryItem
		{
			public abstract new class inventoryID : IBqlField { }
		}
		public class ComponentDeferredCode : DRDeferredCode
		{
			public abstract new class deferredCodeID : IBqlField { }
		}

		public virtual void CreateSingleSchedule(ARInvoice originalDocument, Amount lineTotal, int? defScheduleID, bool isDraft)
		{
			DRScheduleParameters scheduleParameters = GetSingleScheduleParameters(originalDocument);

			CreateSingleSchedule<ARInvoice>(originalDocument, lineTotal, defScheduleID, isDraft, scheduleParameters);
		}

		public virtual void CreateSingleSchedule(AR.Standalone.ARCashSale originalDocument, Amount lineTotal, int? defScheduleID, bool isDraft)
		{
			DRScheduleParameters scheduleParameters = GetSingleScheduleParameters(originalDocument);

			CreateSingleSchedule<AR.Standalone.ARCashSale>(originalDocument, lineTotal, defScheduleID, isDraft, scheduleParameters);
		}

		private void CreateSingleSchedule<T>(T originalDocument, Amount lineTotal, int? DefScheduleID, bool isDraft, DRScheduleParameters scheduleParameters)
			where T : ARRegister
		{
			ARSetup arSetup = PXSelect<ARSetup>.Select(this);

			DRSchedule existingSchedule = PXSelect<
				DRSchedule,
				Where<
					DRSchedule.module, Equal<BatchModule.moduleAR>,
					And<DRSchedule.docType, Equal<Required<ARRegister.docType>>,
					And<DRSchedule.refNbr, Equal<Required<ARRegister.refNbr>>>>>>
				.SelectSingleBound(this, null, originalDocument.DocType, originalDocument.RefNbr, 1);

			if (existingSchedule?.LineNbr != null)
			{
				throw new PXException(Messages.CantCompleteBecauseASC606FeatureIsEnabled);
			}

			if (DefScheduleID == null)
			{

				Location location = PXSelect<Location, Where<Location.locationID, Equal<Required<DRSchedule.bAccountLocID>>>>.SelectSingleBound(this, null, scheduleParameters.BAccountLocID);
				CurrencyInfo currencyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<DRSchedule.curyInfoID>>>>.SelectSingleBound(this, null, scheduleParameters.CuryInfoID);
				scheduleParameters.BaseCuryID = currencyInfo.BaseCuryID;
				SingleScheduleCreator scheduleCreator = new SingleScheduleCreator(
					this, new ARSubaccountProvider(this), this, this, this, this, this, FinPeriodRepository,
                    roundingFunction: x => PXDBCurrencyAttribute.Round(Schedule.Cache, Schedule.Current, x, CMPrecision.TRANCURY),
					branchID: originalDocument.BranchID, isDraft: isDraft, location: location, currencyInfo: currencyInfo);

				if (existingSchedule == null)
				{
					scheduleCreator.CreateOriginalSchedule(
						scheduleParameters,
						lineTotal);
				}
				else
				{
					scheduleCreator.ReevaluateSchedule(
						existingSchedule,
						scheduleParameters,
						lineTotal,
						attachedToOriginalSchedule: false);
				}
			}
			else
			{
				if (originalDocument.DocType == ARDocType.CreditMemo || originalDocument.DocType == ARDocType.DebitMemo ||
					(originalDocument.OrigModule == BatchModule.SO && originalDocument.DocType == ARDocType.Invoice && !string.IsNullOrEmpty(originalDocument.OrigRefNbr)))
				{
					bool accountForPostedTransactions = originalDocument.DocType == ARDocType.CreditMemo;

					if (existingSchedule == null)
					{
						DRSchedule relatedSchedule = (this as IDREntityStorage).CreateCopy(scheduleParameters);
						relatedSchedule.IsDraft = isDraft;
						relatedSchedule.IsCustom = false;

						relatedSchedule = Schedule.Insert(relatedSchedule);

						FillRelatedSingleSchedule(
							relatedSchedule,
							scheduleParameters,
							DefScheduleID,
							lineTotal,
							isDraft,
							accountForPostedTransactions);
					}
					else
					{
						Location location = PXSelect<Location, Where<Location.locationID, Equal<Required<DRSchedule.bAccountLocID>>>>.SelectSingleBound(this, null, scheduleParameters.BAccountLocID);
						CurrencyInfo currencyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<DRSchedule.curyInfoID>>>>.SelectSingleBound(this, null, scheduleParameters.CuryInfoID);
						scheduleParameters.BaseCuryID = currencyInfo.BaseCuryID;

						var scheduleCreator = new SingleScheduleCreator(
								this, new ARSubaccountProvider(this), this, this, this, this, this, FinPeriodRepository,
                                    roundingFunction: x => PXCurrencyAttribute.BaseRound(this, x),
									branchID: originalDocument.BranchID, isDraft: isDraft,
									location: location, currencyInfo: currencyInfo);

						scheduleCreator.ReevaluateSchedule(
							existingSchedule,
							scheduleParameters,
							lineTotal,
							attachedToOriginalSchedule: true);

						DRSchedule relatedSchedule = GetDeferralSchedule(existingSchedule.ScheduleID);

						FillRelatedSingleSchedule(
							relatedSchedule,
							scheduleParameters,
							DefScheduleID,
							lineTotal,
							isDraft,
							accountForPostedTransactions);
					}
				}
			}
		}

		public static DRScheduleParameters GetSingleScheduleParameters(ARInvoice document)
		{
			return new DRScheduleParameters
			{
				Module = BatchModule.AR,
				DocType = document.DocType,
				RefNbr = document.RefNbr,
				DocDate = document.DocDate,
				BAccountID = document.CustomerID,
				BAccountLocID = document.CustomerLocationID,
				FinPeriodID = document.FinPeriodID,
				TranDesc = document.DocDesc,
				CuryID = document.CuryID,
				CuryInfoID = document.CuryInfoID,
				ProjectID = document.ProjectID,
				BaseCuryID = PXAccess.GetBranch(document.BranchID).BaseCuryID,
			};
		}

		public static DRScheduleParameters GetSingleScheduleParameters(AR.Standalone.ARCashSale document)
		{
			return new DRScheduleParameters
			{
				Module = BatchModule.AR,
				DocType = document.DocType,
				RefNbr = document.RefNbr,
				DocDate = document.DocDate,
				BAccountID = document.CustomerID,
				BAccountLocID = document.CustomerLocationID,
				FinPeriodID = document.TranPeriodID,
				TranDesc = document.DocDesc,
				CuryID = document.CuryID,
				CuryInfoID = document.CuryInfoID,
				ProjectID = document.ProjectID,
				BaseCuryID = PXAccess.GetBranch(document.BranchID).BaseCuryID,
			};
		}

		private DRSchedule GetDeferralSchedule(int? scheduleID)
		{
			return PXSelect<
				DRSchedule,
				Where<
					DRSchedule.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>
				.Select(this, scheduleID);
		}

		private void FillRelatedSingleSchedule(DRSchedule relatedSchedule, DRScheduleParameters scheduleParameters, int? defScheduleID, Amount tranAmt, bool isDraft, bool accountForPostedTransactions)
		{
			DRSchedule originalDeferralSchedule = GetDeferralSchedule(defScheduleID);

			DRScheduleDetail originalDetailsTotalDetail = PXSelectGroupBy<
				DRScheduleDetail,
				Where<
					DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>>, Aggregate<Sum<DRScheduleDetail.curyTotalAmt>>>
				.Select(this, originalDeferralSchedule.ScheduleID);

			decimal originalDetailsTotal = originalDetailsTotalDetail.CuryTotalAmt ?? 0m;
			decimal adjustmentTotal = tranAmt.Cury.Value;

			decimal newDetailTotal = 0m;

			var originalDetails = PXSelectJoin<DRScheduleDetail,
					LeftJoin<ARTran,
						On<DRScheduleDetail.lineNbr, Equal<ARTran.lineNbr>>,
					InnerJoin<InventoryItem,
						On<ARTran.inventoryID, Equal<InventoryItem.inventoryID>>,
					LeftJoin<INComponent,
						On<DRScheduleDetail.componentID, Equal<INComponent.inventoryID>>,
					InnerJoin<DRDeferredCode,
						On<ARTran.deferredCode, Equal<DRDeferredCode.deferredCodeID>>>>>>,
					Where<DRScheduleDetail.scheduleID, Equal<Required<DRSchedule.scheduleID>>,
						And<ARTran.tranType, Equal<Required<DRSchedule.docType>>,
						And<ARTran.refNbr, Equal<Required<DRSchedule.refNbr>>>>>>.Select(this, defScheduleID, scheduleParameters.DocType, scheduleParameters.RefNbr);

			foreach (PXResult<DRScheduleDetail, ARTran, InventoryItem, INComponent, DRDeferredCode> item in originalDetails)
			{
				DRScheduleDetail originalDetail = item;
				ARTran tran = item;
				InventoryItem inventoryItem = item;
				INComponent inComponent = item;
				DRDeferredCode defCode = item;


				decimal detailPartRaw = originalDetailsTotal == 0 ? 0 :
					originalDetail.CuryTotalAmt.Value * adjustmentTotal / originalDetailsTotal;

				decimal detailPart = PXDBCurrencyAttribute.BaseRound(this, detailPartRaw);

				decimal takeFromPostedRaw = 0;

				if (accountForPostedTransactions && originalDetail.CuryTotalAmt.Value != 0)
				{
					takeFromPostedRaw =
						detailPartRaw * (originalDetail.CuryTotalAmt.Value - originalDetail.CuryDefAmt.Value) / originalDetail.CuryTotalAmt.Value;
				}

				decimal takeFromPosted = PXDBCurrencyAttribute.BaseRound(this, takeFromPostedRaw);

				decimal adjustmentDeferredAmountRaw = detailPartRaw - takeFromPosted;
				decimal adjustmentDeferredAmount = PXDBCurrencyAttribute.BaseRound(this, adjustmentDeferredAmountRaw);

				INComponent inventoryItemComponent = null;
				DRDeferredCode componentDeferralCode = null;

				if (inventoryItem != null && inComponent != null)
				{
					inventoryItemComponent = GetInventoryItemComponent(inventoryItem.InventoryID, originalDetail.ComponentID);

					if (inventoryItemComponent != null)
					{
						componentDeferralCode = PXSelect<
							DRDeferredCode,
							Where<
								DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>
							.Select(this, inventoryItemComponent.DeferredCode);
					}
				}

				InventoryItem component = GetInventoryItem(originalDetail.ComponentID);

				DRScheduleDetail relatedScheduleDetail;

				if (componentDeferralCode != null)
				{
					// Use component's deferral code
					// -
					relatedScheduleDetail = InsertScheduleDetail(
						tran.BranchID,
						relatedSchedule,
						inventoryItemComponent,
						component,
						componentDeferralCode,
						detailPart,
						originalDetail.DefAcctID,
						originalDetail.DefSubID,
						isDraft,
						tran.DRTermStartDate,
						tran.DRTermEndDate);
				}
				else
				{
					// Use deferral code and accounts from the document line
					// -
					relatedScheduleDetail = InsertScheduleDetail(
						tran.BranchID,
						relatedSchedule,
						component == null ? DRScheduleDetail.EmptyComponentID : component.InventoryID,
						defCode,
						detailPart,
						originalDetail.DefAcctID,
						originalDetail.DefSubID,
						tran.AccountID,
						tran.SubID,
						isDraft,
						tran.DRTermStartDate,
						tran.DRTermEndDate);
				}

				newDetailTotal += detailPart;

				IList<DRScheduleTran> relatedTransactions = new List<DRScheduleTran>();
				DRDeferredCode relatedTransactionsDeferralCode = componentDeferralCode ?? defCode;

				IEnumerable<DRScheduleTran> originalPostedTransactions = null;

				if (accountForPostedTransactions)
				{
					originalPostedTransactions = PXSelect<
						DRScheduleTran,
						Where<
							DRScheduleTran.status, Equal<DRScheduleTranStatus.PostedStatus>,
							And<DRScheduleTran.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>,
							And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
							And<DRScheduleTran.detailLineNbr, Equal<Required<DRScheduleTran.detailLineNbr>>,
							And<DRScheduleTran.lineNbr, NotEqual<Required<DRScheduleTran.lineNbr>>>>>>>>
						.Select(
							this,
							originalDetail.ScheduleID,
							originalDetail.ComponentID,
							originalDetail.DetailLineNbr,
							originalDetail.CreditLineNbr)
						.RowCast<DRScheduleTran>();
				}

				if (adjustmentDeferredAmount != 0m
					|| accountForPostedTransactions && takeFromPosted != 0m)
				{
					string requiredTransactionStatus =
						relatedTransactionsDeferralCode.Method == DeferredMethodType.CashReceipt ?
							DRScheduleTranStatus.Projected :
							DRScheduleTranStatus.Open;

					IEnumerable<DRScheduleTran> originalOpenTransactions = PXSelect<
						DRScheduleTran,
						Where<
							DRScheduleTran.status, Equal<Required<DRScheduleTran.status>>,
							And<DRScheduleTran.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>,
							And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
							And<DRScheduleTran.detailLineNbr, Equal<Required<DRScheduleTran.detailLineNbr>>>>>>>
						.Select(
							this,
							requiredTransactionStatus,
							originalDetail.ScheduleID,
							originalDetail.ComponentID,
							originalDetail.DetailLineNbr)
						.RowCast<DRScheduleTran>();

					IList<DRScheduleTran> relatedDeferralTransactions =
						GetTransactionsGenerator(relatedTransactionsDeferralCode).GenerateRelatedTransactions(
							relatedScheduleDetail,
							originalOpenTransactions,
							originalPostedTransactions,
							adjustmentDeferredAmount,
							takeFromPosted,
							tran.BranchID);

					foreach (DRScheduleTran deferralTransaction in relatedDeferralTransactions)
					{
						Transactions.Insert(deferralTransaction);
						relatedTransactions.Add(deferralTransaction);
					}
				}

				UpdateBalanceProjection(relatedTransactions, relatedScheduleDetail, defCode.AccountType);
			}
		}

		private void UpdateOriginalSingleSchedule(ARRegister originalDocument, decimal amount)
		{
			foreach (PXResult<ARTran, InventoryItem, INComponent, DRDeferredCode> item in ARTransactionWithItems.Select())
			{
				ARTran artran = item;
				InventoryItem inventoryItem = item;
				INComponent component = item;
				DRDeferredCode deferredCode = item;

				UpdateOriginalSchedule(artran, deferredCode, amount, originalDocument.DocDate, originalDocument.FinPeriodID, originalDocument.CustomerID, originalDocument.CustomerLocationID);
			}
		}

		public PXResultset<DRScheduleDetail> GetScheduleDetailsResultset(int? scheduleID)
		{
			return PXSelectJoin<DRScheduleDetail,
				InnerJoin<DRDeferredCode,
					On<DRScheduleDetail.defCode, Equal<DRDeferredCode.deferredCodeID>>,
				LeftJoin<INComponent,
					On<DRScheduleDetail.componentID, Equal<INComponent.inventoryID>>>>,
				Where<
					DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>>>
				.Select(this, scheduleID);
		}

		[Obsolete(Common.InternalMessages.MethodIsObsoleteAndWillBeRemoved2022R2)]
		public virtual void SetFairValueSalesPrice(DRScheduleDetail scheduleDetail, Location location, CurrencyInfo currencyInfo)
		{
			SetFairValueSalesPrice(Schedule.Current, scheduleDetail, ScheduleDetail, location, currencyInfo, Setup.Current.UseFairValuePricesInBaseCurrency.Value);
		}

		[Obsolete(Common.InternalMessages.MethodIsObsoleteAndWillBeRemoved2022R2)]
		public static void SetFairValueSalesPrice(DRSchedule schedule, DRScheduleDetail scheduleDetail, PXSelectBase<DRScheduleDetail> scheduleDetailsView, Location location, CurrencyInfo currencyInfo, bool takeInBaseCurrency)
		{
			if (takeInBaseCurrency)
			{
				currencyInfo.CuryID = currencyInfo.BaseCuryID;
			}

			string taxCalcMode = location?.CTaxCalcMode ?? TX.TaxCalculationMode.TaxSetting;

			var salesPriceItem = ARSalesPriceMaint.CalculateFairValueSalesPrice(scheduleDetailsView.Cache,
						location?.CPriceClassID,
						schedule.BAccountID,
						scheduleDetail.ComponentID,
						currencyInfo,
						scheduleDetail.Qty,
						scheduleDetail.UOM,
						schedule.DocDate.Value,
						takeInBaseCurrency,
						taxCalcMode);

			var newValue = salesPriceItem.Price;

			if (newValue == 0m)
			{
				InventoryItem inventoryItem = PXSelect<
					InventoryItem,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
					.Select(scheduleDetailsView.Cache.Graph, scheduleDetail.ComponentID);

				if (scheduleDetail.ParentInventoryID == null)
				{
					throw new NoFairValuePriceFoundException(inventoryItem.InventoryCD, scheduleDetail.UOM, currencyInfo.CuryID, schedule.DocDate.Value);
				}
				else
				{
					InventoryItem parentInventoryItem = PXSelect<
						InventoryItem,
						Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
						.Select(scheduleDetailsView.Cache.Graph, scheduleDetail.ParentInventoryID);

					throw new NoFairValuePriceFoundException(inventoryItem.InventoryCD, parentInventoryItem.InventoryCD, scheduleDetail.UOM, currencyInfo.CuryID, schedule.DocDate.Value);
				}
			}

			scheduleDetail.FairValueCuryID = salesPriceItem.CuryID;
			scheduleDetail.FairValuePrice = newValue;
			scheduleDetail.EffectiveFairValuePrice = newValue * (salesPriceItem.Prorated ? scheduleDetail.CoTermRate : 1);
		}

		public virtual void SetFairValueSalesPrice(DRScheduleDetail scheduleDetail, ARTran artran, Location location, CurrencyInfo currencyInfo)
		{
			SetFairValueSalesPrice(Schedule.Current, scheduleDetail, ScheduleDetail, artran, ARTransactionWithItems, location, currencyInfo, Setup.Current.UseFairValuePricesInBaseCurrency.Value);
		}

		public static void SetFairValueSalesPrice(DRSchedule schedule, DRScheduleDetail scheduleDetail, PXSelectBase<DRScheduleDetail> scheduleDetailsView,
										ARTran artran, PXSelectBase<ARTran> artranView, Location location, CurrencyInfo currencyInfo, bool takeInBaseCurrency)
		{
			if (takeInBaseCurrency)
			{
				currencyInfo.CuryID = currencyInfo.BaseCuryID;
			}

			string taxCalcMode = location?.CTaxCalcMode ?? TX.TaxCalculationMode.TaxSetting;

			SalesPriceItem salesPriceItem = ARSalesPriceMaint.CalculateFairValueSalesPrice(scheduleDetailsView.Cache,
						location?.CPriceClassID,
						schedule.BAccountID,
						scheduleDetail.ComponentID,
						currencyInfo,
						scheduleDetail.Qty,
						scheduleDetail.UOM,
						schedule.DocDate.Value,
						takeInBaseCurrency,
						taxCalcMode);

			decimal newValue = salesPriceItem.Price;

			if (newValue == 0m)
			{
				InventoryItem inventoryItem = PXSelect<
					InventoryItem,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
					.Select(scheduleDetailsView.Cache.Graph, scheduleDetail.ComponentID);

				if (scheduleDetail.ParentInventoryID == null)
				{
					throw new NoFairValuePriceFoundException(inventoryItem.InventoryCD, scheduleDetail.UOM, currencyInfo.CuryID, schedule.DocDate.Value);
				}
				else
				{
					InventoryItem parentInventoryItem = PXSelect<
						InventoryItem,
						Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
						.Select(scheduleDetailsView.Cache.Graph, scheduleDetail.ParentInventoryID);

					throw new NoFairValuePriceFoundException(inventoryItem.InventoryCD, parentInventoryItem.InventoryCD, scheduleDetail.UOM, currencyInfo.CuryID, schedule.DocDate.Value);
				}
			}

			decimal curyDiscountAmt = 0;
			if (salesPriceItem.Discountable)
			{
				if(scheduleDetail.Qty.Value == 0)
					throw new PXException(Messages.SumOfFairValuePricesEqualToZero);

				//create fake artran with Component instead of Inventory
				//to use it into DiscountEngine
				ARTran fakeLine = new ARTran();
				fakeLine.CustomerID = schedule.BAccountID;
				fakeLine.InventoryID = scheduleDetail.ComponentID;
				fakeLine.BranchID = scheduleDetail.BranchID;
				fakeLine.SiteID = artran.SiteID;

				var discountLineFields = new DiscountLineFields<DiscountLineFields.skipDisc,
																DiscountLineFields.curyDiscAmt,
																DiscountLineFields.discPct,
																DiscountLineFields.discountID,
																DiscountLineFields.discountSequenceID,
																DiscountLineFields.discountsAppliedToLine,
																DiscountLineFields.manualDisc,
																DiscountLineFields.manualPrice,
																DiscountLineFields.lineType,
																DiscountLineFields.isFree,
																DiscountLineFields.calculateDiscountsOnImport,
																DiscountLineFields.automaticDiscountsDisabled,
																DiscountLineFields.skipLineDiscounts>
																(artranView.Cache, fakeLine);
				//calculate discount for Discountable FairValue Price
				decimal curyDiscountTotalAmt = DiscountEngine.GetLineDiscountOnly<ARTran>(artranView.Cache,
												fakeLine,
												discountLineFields,
												newValue,
												newValue * scheduleDetail.Qty.Value,
												scheduleDetail.Qty.Value,
												location?.LocationID,
												schedule.BAccountID,
												salesPriceItem.CuryID,
												schedule.DocDate.Value,
												null,
												scheduleDetail.ComponentID);
				curyDiscountAmt = curyDiscountTotalAmt / scheduleDetail.Qty.Value;
			}

			scheduleDetail.FairValueCuryID = salesPriceItem.CuryID;
			scheduleDetail.FairValuePrice = newValue;
			scheduleDetail.DiscountPercent = newValue == 0 ? 0 : curyDiscountAmt / newValue * 100;
			scheduleDetail.EffectiveFairValuePrice = (newValue - curyDiscountAmt) * (salesPriceItem.Prorated ? scheduleDetail.CoTermRate : 1);
		}

		public PXResultset<ARTran> GetParentDocumentDetails()
		{
			return ARTransactionWithItems.Select();
		}

		public decimal GetQuantityInBaseUOMs(ARTran tran)
		{
			return INUnitAttribute.ConvertToBase(
						this.Caches[typeof(ARTran)],
						tran.InventoryID,
						tran.UOM,
						(tran.Qty ?? 0),
						INPrecision.QUANTITY);
		}

		public void DeleteAllDetails(int? scheduleID)
		{
			foreach (DRScheduleDetail detail in PXSelect<DRScheduleDetail,
						Where<DRScheduleDetail.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>.Select(this, scheduleID))
			{
				ScheduleDetail.Delete(detail);
			}
		}
	}
}
