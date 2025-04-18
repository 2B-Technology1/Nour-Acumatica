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

using PX.Common;
using PX.Data;

using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.AP;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.FA.Overrides.AssetProcess;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.Common.Extensions;
using PX.Data.DependencyInjection;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.WorkflowAPI;
using PX.Objects.Common;
using PX.Objects.Common.Scopes;

namespace PX.Objects.FA
{
	public class OperableTran
	{
		public FATran Tran { get; }
		public PXDBOperation Op { get; set; }

		public OperableTran(FATran tran, PXDBOperation op)
		{
			Tran = tran;
			Op = op;
		}
	}

	public class SuppressHistoryUpdateScope : FlaggedModeScopeBase<SuppressHistoryUpdateScope> { }

	public class AssetMaint : PXGraph<AssetMaint, FixedAsset>, IGraphWithInitialization
	{
		#region DAC Overrides

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FixedAsset.assetID>))]
		protected virtual void _(Events.CacheAttached<FixedAsset.accumulatedDepreciationAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FixedAsset.assetID>))]
		protected virtual void _(Events.CacheAttached<FixedAsset.accumulatedDepreciationSubID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FixedAsset.assetID>))]
		protected virtual void _(Events.CacheAttached<FixedAsset.depreciatedExpenseAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FixedAsset.assetID>))]
		protected virtual void _(Events.CacheAttached<FixedAsset.depreciatedExpenseSubID> e) { }


		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FADetails.assetID>))]
		protected virtual void _(Events.CacheAttached<FADetails.depreciateFromDate> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<Standalone.FADetails.assetID>))]
		protected virtual void _(Events.CacheAttached<Standalone.FADetails.depreciateFromDate> e) { }


		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FALocationHistory.assetID>))]
		protected virtual void _(Events.CacheAttached<FALocationHistory.accumulatedDepreciationAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FALocationHistory.assetID>))]
		protected virtual void _(Events.CacheAttached<FALocationHistory.accumulatedDepreciationSubID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FALocationHistory.assetID>))]
		protected virtual void _(Events.CacheAttached<FALocationHistory.depreciatedExpenseAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FALocationHistory.assetID>))]
		protected virtual void _(Events.CacheAttached<FALocationHistory.depreciatedExpenseSubID> e) { }


		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FABookSettings.assetID>))]
		protected virtual void _(Events.CacheAttached<FABookSettings.depreciationMethodID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FABookSettings.assetID>))]
		protected virtual void _(Events.CacheAttached<FABookSettings.averagingConvention> e) { }


		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FABookBalance.assetID>))]
		protected virtual void _(Events.CacheAttached<FABookBalance.depreciationMethodID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FABookBalance.assetID>))]
		protected virtual void _(Events.CacheAttached<FABookBalance.averagingConvention> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIRequired(typeof(FixedAssetCanBeDepreciated<FABookBalance.assetID>))]
		protected virtual void _(Events.CacheAttached<FABookBalance.deprFromDate> e) { }

		#endregion

		#region Graph Extensions
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class AdditionsViewExtension : AdditionsViewExtensionBase<AssetMaint>
		{
			public override void Initialize()
			{
				base.Initialize();
				DsplAdditions.Cache.AllowInsert = false;
			}

			public PXSelect<FAAccrualTran> Additions;

			[PXFilterable]
			[PXCopyPasteHiddenView]
			public PXSelectJoin<DsplFAATran, LeftJoin<FAAccrualTran, On<DsplFAATran.tranID, Equal<FAAccrualTran.tranID>>>> DsplAdditions;

			public virtual IEnumerable additions()
			{
				if (Base.Asset.Current?.AssetID == null || Base.Asset.Current?.AssetID < 0)
					return new List<FAAccrualTran>();

				return GetFAAccrualTransactions(Base.GLTrnFilter.Current, Additions.Cache);
			}

			public virtual IEnumerable dspladditions()
			{
				Additions.View.Clear();

				int startRow = PXView.StartRow;
				int totalRows = 0;
				foreach (FAAccrualTran ext in Additions.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
					DsplFAATran dspl = new DsplFAATran
					{
						TranID = ext.TranID,
						GLTranQty = ext.GLTranQty,
						GLTranAmt = ext.GLTranAmt,
						ClosedQty = ext.ClosedQty,
						ClosedAmt = ext.ClosedAmt,
					};

					DsplAdditions.Cache.IsDirty = (DsplAdditions.Cache.GetStatus(dspl) != PXEntryStatus.Notchanged || DsplAdditions.Insert(dspl) == null) && DsplAdditions.Cache.IsDirty;

					dspl = DsplAdditions.Cache.Locate(dspl) as DsplFAATran;
					yield return new PXResult<DsplFAATran, FAAccrualTran>(dspl, ext);
				}
				PXView.StartRow = 0;
			}

			#region Button ProcessAdditions
			public PXAction<FixedAsset> ProcessAdditions;
			[PXUIField(DisplayName = Messages.ProcessAdditions, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
			[PXProcessButton]
			public virtual IEnumerable processAdditions(PXAdapter adapter)
			{
				FixedAsset asset = Base.CurrentAsset.Current;
				FADetails det = Base.AssetDetails.Current ?? Base.AssetDetails.Select();
				GLTranFilter filter = Base.GLTrnFilter.Current;
				Numbering numbering = Base.assetNumbering.Select();

				if (filter.CurrentCost != filter.ExpectedCost) //allow reconciliation except addition and deduction
				{
					AssetProcess.RestrictAdditonDeductionForCalcMethod(Base, asset.AssetID, FADepreciationMethod.depreciationMethod.AustralianPrimeCost);
					AssetProcess.RestrictAdditonDeductionForCalcMethod(Base, asset.AssetID, FADepreciationMethod.depreciationMethod.NewZealandStraightLine);
					AssetProcess.RestrictAdditonDeductionForCalcMethod(Base, asset.AssetID, FADepreciationMethod.depreciationMethod.NewZealandStraightLineEvenly);
				}

				decimal accrualBal = filter.AccrualBalance ?? 0m;
				decimal currCost = filter.CurrentCost ?? 0m;
				foreach (DsplFAATran dspl in DsplAdditions.Cache.Inserted)
				{
					if (dspl.Selected == true)
					{
						AssetGLTransactions.GLTran gltran = PXSelect<
							AssetGLTransactions.GLTran,
							Where<AssetGLTransactions.GLTran.tranID, Equal<Current<DsplFAATran.tranID>>>>
							.SelectSingleBound(Base, new[] { dspl });

						if (dspl.Component == true)
						{
							if (numbering.UserNumbering == true)
							{
								throw new PXException(Messages.ComponentsCannotBeCreatedBecauseManualNumberingIsActivated, numbering.NumberingID);
							}

							FALocationHistory hist = Base.AssetLocation.Select(asset.AssetID, det.LocationRevID);
							if (Base.AssetLocation.Cache.GetStatus(hist) != PXEntryStatus.Notchanged)
							{
								throw new PXException(Messages.AssetIsNotSaved);
							}

							FixedAsset cls = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<DsplFAATran.classID>>>>.SelectSingleBound(Base, new object[] { dspl });
							if (cls == null)
							{
								throw new PXException(ErrorMessages.FieldIsEmpty, typeof(DsplFAATran.classID).Name);
							}

							AdditionsFATran procgraph = CreateInstance<AdditionsFATran>();
							procgraph.SelectTimeStamp();

							decimal unitCost = PXDBCurrencyAttribute.BaseRound(Base, (dspl.SelectedAmt / dspl.SelectedQty) ?? 0m);
							for (int i = 0; i < dspl.SelectedQty; i++)
							{
								procgraph.InsertNewComponent(asset, cls, filter.TranDate, unitCost, 1m, hist, gltran);
							}
							procgraph.Actions.PressSave();

						}
						else
						{
							if (Base.Register.Current == null || (Base.Register.Current.Released ?? false))
							{
								AssetGLTransactions.SetCurrentRegister(Base.Register, (int)asset.BranchID);
							}
							FATran tran = new FATran();
							foreach (FABookBalance bal in PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>>.Select(Base))
							{
								FADepreciationMethod method = PXSelect<FADepreciationMethod, Where<FADepreciationMethod.methodID, Equal<Required<FADepreciationMethod.methodID>>>>.Select(Base, bal.DepreciationMethodID);
								if (bal.Depreciate == true && Base.Asset.Current.UnderConstruction == false && method == null)
								{
									throw new PXException(Messages.DepreciationMethodDoesNotExist);
								}

								if (filter.TranDate == null)
								{
									throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<GLTranFilter.tranDate>(Base.GLTrnFilter.Cache));
								}
								if (bal.UpdateGL == true && string.IsNullOrEmpty(filter.PeriodID))
								{
									throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<GLTranFilter.periodID>(Base.GLTrnFilter.Cache));
								}

								// Nearest open period
								OrganizationFinPeriod glperiod = Base.FinPeriodUtils
									.GetNearestOpenOrganizationFinPeriodInSubledger<OrganizationFinPeriod.fAClosed>(
										gltran.FinPeriodID,
										asset.BranchID,
										() => bal.UpdateGL == true);

								if (filter.ReconType == GLTranFilter.reconType.Addition)
								{
									OrganizationFinPeriod period =
										Base.FABookPeriodUtils.GetNearestOpenOrganizationMappedFABookPeriodInSubledger<OrganizationFinPeriod.fAClosed>(
											bal.BookID,
											gltran.BranchID,
											gltran.FinPeriodID,
											asset.BranchID);

									tran = new FATran
									{
										AssetID = asset.AssetID,
										BookID = bal.BookID,
										TranAmt = dspl.SelectedAmt,
										Qty = dspl.SelectedQty,
										TranDate = gltran.TranDate,
										FinPeriodID = period?.FinPeriodID,
										GLTranID = bal.UpdateGL == true ? gltran.TranID : null,
										TranType = FATran.tranType.ReconciliationPlus,
										CreditAccountID = gltran.AccountID,
										CreditSubID = gltran.SubID,
										DebitAccountID = asset.FAAccrualAcctID,
										DebitSubID = asset.FAAccrualSubID,
										TranDesc = gltran.TranDesc,
									};

									tran = Base.FATransactions.Insert(tran);

									if (tran.TranAmt + accrualBal > currCost)
									{
										tran = new FATran
										{
											AssetID = asset.AssetID,
											BookID = bal.BookID,
											TranAmt = tran.TranAmt + accrualBal - currCost,
											TranDate = filter.TranDate,
											FinPeriodID = bal.UpdateGL == true ? filter.PeriodID : null,
											TranType = FATran.tranType.PurchasingPlus,
											CreditAccountID = asset.FAAccrualAcctID,
											CreditSubID = asset.FAAccrualSubID,
											DebitAccountID = asset.FAAccountID,
											DebitSubID = asset.FASubID,
											TranDesc = gltran.TranDesc,
										};
										tran = Base.FATransactions.Insert(tran);

										if (bal.Status == FixedAssetStatus.FullyDepreciated && bal.Depreciate == true && Base.Asset.Current.UnderConstruction != true && method.IsPureStraightLine)
										{
											tran = new FATran
											{
												AssetID = asset.AssetID,
												BookID = bal.BookID,
												TranAmt = tran.TranAmt,
												TranDate = filter.TranDate,
												FinPeriodID = filter.PeriodID,
												TranType = FATran.tranType.CalculatedPlus,
												CreditAccountID = asset.AccumulatedDepreciationAccountID,
												CreditSubID = asset.AccumulatedDepreciationSubID,
												DebitAccountID = asset.DepreciatedExpenseAccountID,
												DebitSubID = asset.DepreciatedExpenseSubID,
												TranDesc = gltran.TranDesc,
											};
											tran = Base.FATransactions.Insert(tran);
										}
									}
								}
								else
								{
									tran = new FATran
									{
										AssetID = asset.AssetID,
										BookID = bal.BookID,
										TranAmt = dspl.SelectedAmt,
										Qty = dspl.SelectedQty,
										TranDate = gltran.TranDate,
										FinPeriodID = glperiod != null ? glperiod.FinPeriodID : gltran.FinPeriodID,
										GLTranID = bal.UpdateGL == true ? gltran.TranID : null,
										TranType = FATran.tranType.ReconciliationMinus,
										DebitAccountID = gltran.AccountID,
										DebitSubID = gltran.SubID,
										CreditAccountID = asset.FAAccrualAcctID,
										CreditSubID = asset.FAAccrualSubID,
										TranDesc = gltran.TranDesc,
									};
									Base.FATransactions.Insert(tran);

									tran = new FATran
									{
										AssetID = asset.AssetID,
										BookID = bal.BookID,
										TranAmt = dspl.SelectedAmt,
										TranDate = filter.TranDate,
										FinPeriodID = bal.UpdateGL == true ? filter.PeriodID : null,
										TranType = FATran.tranType.PurchasingMinus,
										DebitAccountID = asset.FAAccrualAcctID,
										DebitSubID = asset.FAAccrualSubID,
										CreditAccountID = asset.FAAccountID,
										CreditSubID = asset.FASubID,
										TranDesc = gltran.TranDesc,
									};
									tran = Base.FATransactions.Insert(tran);

									if (bal.Status == FixedAssetStatus.FullyDepreciated && bal.Depreciate == true && Base.Asset.Current.UnderConstruction != true && method.IsPureStraightLine)
									{
										tran = new FATran
										{
											AssetID = asset.AssetID,
											BookID = bal.BookID,
											TranAmt = tran.TranAmt,
											TranDate = filter.TranDate,
											FinPeriodID = filter.PeriodID,
											TranType = FATran.tranType.CalculatedMinus,
											CreditAccountID = asset.DepreciatedExpenseAccountID,
											CreditSubID = asset.DepreciatedExpenseSubID,
											DebitAccountID = asset.AccumulatedDepreciationAccountID,
											DebitSubID = asset.AccumulatedDepreciationSubID,
											TranDesc = gltran.TranDesc,
										};
										tran = Base.FATransactions.Insert(tran);
									}
								}
							}
							accrualBal += dspl.SelectedAmt ?? 0m;
							currCost += tran.TranType == FATran.tranType.PurchasingPlus ? (tran.TranAmt ?? 0m) : 0m;
						}
					}
				}
				DsplAdditions.Cache.Clear();
				return adapter.Get();
			}
			#endregion

			protected virtual void FATran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
			{
				DsplAdditions.Cache.Clear();
			}

			protected virtual void GLTranFilter_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				DsplAdditions.Cache.Clear();
			}

			protected virtual void GLTranFilter_SubID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				DsplAdditions.Cache.Clear();
			}

			protected virtual void GLTranFilter_ReconType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				DsplAdditions.Cache.Clear();
			}

			protected virtual void GLTranFilter_ShowReconciled_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				DsplAdditions.Cache.Clear();
			}

			protected virtual void GLTranFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				GLTranFilter filter = (GLTranFilter)e.Row;
				FixedAsset asset = Base.CurrentAsset.Current;
				FATran unrelp = PXSelect<FATran, Where<FATran.assetID, Equal<Current<FixedAsset.assetID>>, And<FATran.released, Equal<False>, And<Where<FATran.tranType, Equal<FATran.tranType.purchasingPlus>, Or<FATran.tranType, Equal<FATran.tranType.purchasingMinus>>>>>>>.Select(Base);
				FATran unrelr = PXSelect<FATran, Where<FATran.assetID, Equal<Current<FixedAsset.assetID>>, And<FATran.released, Equal<False>, And<Where<FATran.tranType, Equal<FATran.tranType.reconcilliationPlus>, Or<FATran.tranType, Equal<FATran.tranType.reconcilliationMinus>>>>>>>.Select(Base);

				bool accountIsGood = true;
				try
				{
					AccountAttribute.VerifyAccountIsNotControl((Account)PXSelectorAttribute.Select<GLTranFilter.accountID>(sender, e.Row));
				}
				catch (PXSetPropertyException ex)
				{
					accountIsGood = (ex.ErrorLevel < PXErrorLevel.Error);
				}

				ProcessAdditions.SetEnabled(Base.CurrentAsset.Cache.GetStatus(asset) != PXEntryStatus.Inserted && unrelp == null && unrelr == null && filter.UnreconciledAmt >= 0 && accountIsGood);
			}

			#region Additions Events
			protected virtual void DsplFAATran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				DsplFAATran tran = (DsplFAATran)e.Row;
				if (tran == null) return;

				PXUIFieldAttribute.SetEnabled<DsplFAATran.component>(sender, tran, (tran.Selected ?? false) && Base.GLTrnFilter.Current.ReconType == GLTranFilter.reconType.Addition);
				PXUIFieldAttribute.SetEnabled<DsplFAATran.classID>(sender, tran, tran.Component ?? false);
				PXUIFieldAttribute.SetVisible<GLTran.inventoryID>(Base.Caches[typeof(GLTran)], null, true);
			}

			protected virtual void DsplFAATran_ClassID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				DsplFAATran tran = (DsplFAATran)e.Row;
				if (tran == null) return;

				if (tran.Component == true && e.NewValue == null)
				{
					throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(DsplFAATran.classID)}]");
				}
			}

			protected virtual void DsplFAATran_SelectedAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				DsplFAATran tran = (DsplFAATran)e.Row;
				if (tran.SelectedAmt > 0m)
				{
					tran.Selected = true;
				}
			}

			protected virtual void DsplFAATran_SelectedQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				DsplFAATran tran = (DsplFAATran)e.Row;
				if (tran.SelectedQty > 0m)
				{
					tran.Selected = true;
				}
			}

			protected virtual void DsplFAATran_Reconciled_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				DsplFAATran tran = (DsplFAATran)e.Row;
				if (tran == null) return;

				FAAccrualTran atran = PXSelect<FAAccrualTran, Where<FAAccrualTran.tranID, Equal<Current<DsplFAATran.tranID>>>>.SelectSingleBound(Base, new object[] { tran });
				atran.Reconciled = tran.Reconciled;
				if (Additions.Cache.GetStatus(atran) == PXEntryStatus.Notchanged)
				{
					Additions.Cache.SetStatus(atran, PXEntryStatus.Updated);
					Additions.Cache.IsDirty = true;
				}
			}

			protected virtual void DsplFAATran_SelectedAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				DsplFAATran tran = (DsplFAATran)e.Row;
				if (tran == null) return;

				if (tran.Selected == true)
				{
					if (((decimal?)e.NewValue) <= 0m)
						throw new PXSetPropertyException(CS.Messages.Entry_GT, 0m);
					if (((decimal?)e.NewValue) > tran.GLTranAmt - tran.ClosedAmt)
						throw new PXSetPropertyException(CS.Messages.Entry_LE, tran.GLTranAmt - tran.ClosedAmt);
				}
			}

			protected virtual void DsplFAATran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				DsplFAATran tran = (DsplFAATran)e.Row;
				if (tran == null) return;

				GLTranFilter filter = Base.GLTrnFilter.Current;
				decimal sum = 0m;
				decimal expectedcost = filter.CurrentCost ?? 0m;
				decimal expectedaccr = filter.AccrualBalance ?? 0m;

				foreach (DsplFAATran trn in DsplAdditions.Cache.Inserted)
				{
					sum += trn.SelectedAmt ?? 0m;
				}

				if (filter.ReconType == GLTranFilter.reconType.Addition)
				{
					expectedaccr += sum;
					if (expectedaccr > filter.CurrentCost)
					{
						expectedcost = expectedaccr;
					}
				}
				else
				{
					expectedaccr -= sum;
					expectedcost = expectedaccr;
				}

				filter.SelectionAmt = sum;
				filter.ExpectedAccrualBal = expectedaccr;
				filter.ExpectedCost = expectedcost;

				try
				{
					object classID = tran.ClassID;
					sender.RaiseFieldVerifying<DsplFAATran.classID>(tran, ref classID);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<DsplFAATran.classID>(tran, tran.ClassID, ex);
				}

				try
				{
					object selAmt = tran.SelectedAmt;
					sender.RaiseFieldVerifying<DsplFAATran.selectedAmt>(tran, ref selAmt);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<DsplFAATran.selectedAmt>(tran, tran.SelectedAmt, ex);
				}
			}

			// TODO: DK - need to write a formula
			protected virtual void DsplFAATran_Component_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				DsplFAATran tran = (DsplFAATran)e.Row;
				if (tran == null) return;

				if (tran.Component != true)
				{
					tran.ClassID = null;
					object old = tran.SelectedQty;
					tran.SelectedQty = tran.Selected == true ? Math.Min((Base.Asset.Current.Qty ?? 0m), (tran.OpenQty ?? 0m)) : 0m;
					if ((decimal?)old != tran.SelectedQty)
					{
						sender.RaiseFieldUpdated<DsplFAATran.selectedQty>(tran, old);
					}
				}
			}
			protected virtual void DsplFAATran_Selected_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				DsplFAATran tran = (DsplFAATran)e.Row;
				if (tran == null) return;

				GLTranFilter filter = Base.GLTrnFilter.Current;
				object old;

				old = tran.SelectedQty;
				tran.SelectedQty = tran.Selected == true ? Math.Min((Base.Asset.Current.Qty ?? 0m), (tran.OpenQty ?? 0m)) : 0m;
				if ((decimal?)old != tran.SelectedQty)
				{
					sender.RaiseFieldUpdated<DsplFAATran.selectedQty>(tran, old);
				}

				old = tran.SelectedAmt;
				tran.SelectedAmt = tran.Selected == true ? Math.Min((filter.ExpectedCost ?? 0m) - (filter.ExpectedAccrualBal ?? 0m), tran.OpenAmt ?? 0m) : 0m;
				if ((decimal?)old != tran.SelectedAmt)
				{
					sender.RaiseFieldUpdated<DsplFAATran.selectedAmt>(tran, old);
				}
			}
			protected virtual void DsplFAATran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				e.Cancel = true;
			}
			#endregion
		}
		#endregion

		#region Repo functions

		public static FixedAsset FindByID(PXGraph graph, int? assetID)
	    {
	        return PXSelect<FixedAsset, 
	            Where<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>>>
	            .Select(graph, assetID);
	    }

        #endregion


        #region Selects Declaration

        public PXSelect<FixedAsset, Where<FixedAsset.recordType, Equal<FARecordType.assetType>>> Asset;
		public PXSelect<FAClass> Class;
		public PXSelect<BAccount> Baccount;
		public PXSelect<Vendor> Vendor;
		public PXSelect<EPEmployee> Employee;
		public PXSelect<FixedAsset, Where<FixedAsset.assetCD, Equal<Current<FixedAsset.assetCD>>>> CurrentAsset;
		public PXSelect<FADetails, Where<FADetails.assetID, Equal<Optional<FixedAsset.assetID>>>> AssetDetails;
		public PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>,
			OrderBy<Asc<FABookBalance.assetID, Asc<FABookBalance.bookID>>>> AssetBalance;
		public PXSelect<FABookSettings, Where<FABookSettings.assetID, Equal<Current<FixedAsset.classID>>>> DepreciationSettings;
		public PXSelect<FABookHistory, Where<FABookHistory.assetID, Equal<Current<FixedAsset.assetID>>,
										 And<FABookHistory.bookID, Equal<Current<FABookSettings.bookID>>>>> BookHistory;
		public PXSelect<FABookHist> BookHist;

		public PXSelectJoin<FAComponent, InnerJoin<FADetails, On<FAComponent.assetID, Equal<FADetails.assetID>>>, Where<FAComponent.parentAssetID, Equal<Current<FixedAsset.assetID>>>> AssetElements;

		public PXSelect<FARegister> Register;

		public SelectFrom<FATran>
			.Where<
				FATran.assetID.IsEqual<FixedAsset.assetID.FromCurrent>
				.And<FATran.bookID.IsEqual<TranBookFilter.bookID.FromCurrent>
					.Or<TranBookFilter.bookID.FromCurrent.IsNull>>>
			.OrderBy<Asc<FATran.finPeriodID>, Asc<FATran.bookID>>
			.View FATransactions;

		public PXSelect<FALocationHistory,
						Where<FALocationHistory.assetID, Equal<Current<FixedAsset.assetID>>>,
						OrderBy<Desc<FALocationHistory.revisionID>>> LocationHistory;

		public PXSelect<FAUsage, Where<FAUsage.assetID, Equal<Current<FixedAsset.assetID>>>> AssetUsage;

		public PXSelect<FAServiceSchedule, Where<FAServiceSchedule.scheduleID, Equal<Current<FixedAsset.serviceScheduleID>>>> ServiceSchedule;

		public PXSelect<FAService, Where<FAService.assetID, Equal<Current<FixedAsset.assetID>>>> AssetService;

		public PXSelect<FALocationHistory, Where<FALocationHistory.assetID, Equal<Optional<FADetails.assetID>>,
			And<FALocationHistory.revisionID, Equal<Optional<FADetails.locationRevID>>>>> AssetLocation;
		[PXCopyPasteHiddenView]
		public PXSelectOrderBy<FAHistory, OrderBy<Asc<FAHistory.finPeriodID>>> AssetHistory;

		[PXCopyPasteHiddenView]
		public PXSelectOrderBy<FASheetHistory, OrderBy<Asc<FASheetHistory.periodNbr>>> BookSheetHistory;

		public PXSetup<FASetup> fasetup;
		[PXCopyPasteHiddenView]
		public PXFilter<DeprBookFilter> deprbookfilter;
		[PXCopyPasteHiddenView]
		public PXFilter<TranBookFilter> bookfilter;

		public PXFilter<DisposeParams> DispParams;
		public PXFilter<SuspendParams> SuspendParams;
		public PXFilter<ReverseDisposalInfo> RevDispInfo;

		public PXFilter<GLTranFilter> GLTrnFilter;

		public SelectFrom<FATran>
				.Where<FATran.tranType.IsEqual<FATran.tranType.transferPurchasing>
					.And<FATran.assetID.IsEqual<FixedAsset.assetID.FromCurrent>>>.View transferTransactions;

		[PXFilterable]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<DsplFAATran, LeftJoin<FAAccrualTran, On<DsplFAATran.tranID, Equal<FAAccrualTran.tranID>>>> DsplAdditions;

		public PXSelectJoin<Numbering, InnerJoin<FASetup, On<FASetup.assetNumberingID, Equal<Numbering.numberingID>>>> assetNumbering;

		public PXSetup<GLSetup> glsetup;
		public PXSetup<Company> company;
		#endregion

		[InjectDependency]
		public IFABookPeriodRepository FABookPeriodRepository { get; set; }

		[InjectDependency]
		public IFABookPeriodUtils FABookPeriodUtils { get; set; }
		
		#region Declaration
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }
		#endregion

		#region Ctor

		public Dictionary<int?, int> _Books = new Dictionary<int?, int>();
		public List<string> _FieldNames = new List<string>();
		public List<string> _SheetFieldNames = new List<string>();
		private FixedAsset _PersistedAsset;

		public AssetMaint()
		{
			PXCache dummyFABookPeriodCache = Caches[typeof(FABookPeriod)]; // AC-125133, irregular conflict with FABookPeriod2 cache

			PXCache glCache = Caches[typeof(GLTran)];
			PXCache detCache = AssetDetails.Cache;
			PXCache cache = Asset.Cache;

			object setup = fasetup.Current;
			setup = glsetup.Current;

			PXUIFieldAttribute.SetRequired<FADetails.propertyType>(detCache, true);
			PXUIFieldAttribute.SetRequired<FixedAsset.assetTypeID>(cache, true);
			PXUIFieldAttribute.SetRequired<FixedAsset.classID>(cache, true);

			PXUIFieldAttribute.SetVisible<GLTran.module>(glCache, null, false);
			PXUIFieldAttribute.SetVisible<GLTran.batchNbr>(glCache, null, false);
			PXUIFieldAttribute.SetVisible<GLTran.refNbr>(glCache, null, false);

			int i = 0;
			foreach (FABook book in PXSelect<FABook>.Select(this))
			{
				int j = i++;
				string fname;
				_Books.Add(book.BookID, j);
				fname = $"{book.BookCode} {PXMessages.LocalizeNoPrefix(Messages.CalcValueFieldName)}";
				_FieldNames.Add(fname);
				AssetHistory.Cache.Fields.Add(fname);
				FieldSelecting.AddHandler(typeof(FAHistory), fname, (sender, e) => PtdCalculatedFieldSelecting(sender, e, j, true));
				fname = $"{book.BookCode} {PXMessages.LocalizeNoPrefix(Messages.DeprValueFieldName)}";
				_FieldNames.Add(fname);
				AssetHistory.Cache.Fields.Add(fname);
				FieldSelecting.AddHandler(typeof(FAHistory), fname, (sender, e) => PtdCalculatedFieldSelecting(sender, e, j, false));
			}

			int maxyears = 0;
			foreach (FABookBalance2 bal in PXSelectGroupBy<FABookBalance2, Aggregate<Max<FABookBalance2.exactLife>>>.Select(this))
			{
				maxyears = bal.ExactLife ?? 0;
			}

			for (i = 0; i < maxyears; i++)
			{
				int j = i;
				string fldname = $"Year_{j}";
				_SheetFieldNames.Add(fldname);
				BookSheetHistory.Cache.Fields.Add(fldname);
				FieldSelecting.AddHandler(typeof(FASheetHistory), fldname,
					delegate (PXCache sender, PXFieldSelectingEventArgs e)
					{
						BookSheetPeriodDepreciatedFieldSelecting(sender, e, j);
					});
				fldname = $"Calc_{j}";
				_SheetFieldNames.Add(fldname);
				BookSheetHistory.Cache.Fields.Add(fldname);
				FieldSelecting.AddHandler(typeof(FASheetHistory), fldname,
					delegate (PXCache sender, PXFieldSelectingEventArgs e)
					{
						BookSheetPtdCalcFieldSelecting(sender, e, j, true);
					});
				fldname = $"Depr_{j}";
				_SheetFieldNames.Add(fldname);
				BookSheetHistory.Cache.Fields.Add(fldname);
				FieldSelecting.AddHandler(typeof(FASheetHistory), fldname,
					delegate (PXCache sender, PXFieldSelectingEventArgs e)
					{
						BookSheetPtdCalcFieldSelecting(sender, e, j, false);
					});
			}


			FATransactions.Cache.AllowInsert = false;
			FATransactions.Cache.AllowUpdate = false;
			FATransactions.Cache.AllowDelete = true;

			PXUIFieldAttribute.SetEnabled<FABookBalance.lastDeprPeriod>(AssetBalance.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<FABookBalance.ytdDepreciated>(AssetBalance.Cache, null, true);

			FieldDefaulting.AddHandler<FixedAsset.fASubID>(FASubIDFieldDefaulting<FixedAsset.fASubMask, FixedAsset.fASubID>);
			FieldDefaulting.AddHandler<FixedAsset.accumulatedDepreciationSubID>(FASubIDFieldDefaulting<FixedAsset.accumDeprSubMask, FixedAsset.accumulatedDepreciationSubID>);
			FieldDefaulting.AddHandler<FixedAsset.depreciatedExpenseSubID>(FASubIDFieldDefaulting<FixedAsset.deprExpenceSubMask, FixedAsset.depreciatedExpenseSubID>);
			FieldDefaulting.AddHandler<FixedAsset.disposalSubID>(FASubIDFieldDefaulting<FixedAsset.proceedsSubMask, FixedAsset.disposalSubID>);
			FieldDefaulting.AddHandler<FixedAsset.gainSubID>(FASubIDFieldDefaulting<FixedAsset.gainLossSubMask, FixedAsset.gainSubID>);
			FieldDefaulting.AddHandler<FixedAsset.lossSubID>(FASubIDFieldDefaulting<FixedAsset.gainLossSubMask, FixedAsset.lossSubID>);

			FieldUpdated.AddHandler<FixedAsset.fAAccountID>(SetFABookBalanceUpdated);
			FieldUpdated.AddHandler<FixedAsset.fASubID>(SetFABookBalanceUpdated);
			FieldUpdated.AddHandler<FixedAsset.accumulatedDepreciationAccountID>(SetFABookBalanceUpdated);
			FieldUpdated.AddHandler<FixedAsset.accumulatedDepreciationSubID>(SetFABookBalanceUpdated);
			FieldUpdated.AddHandler<FixedAsset.fAAccrualAcctID>(SetFABookBalanceUpdated);
			FieldUpdated.AddHandler<FixedAsset.fAAccrualSubID>(SetFABookBalanceUpdated);
			FieldUpdated.AddHandler<FixedAsset.depreciatedExpenseAccountID>(SetFABookBalanceUpdated);
			FieldUpdated.AddHandler<FixedAsset.depreciatedExpenseSubID>(SetFABookBalanceUpdated);
		}

		void IGraphWithInitialization.Initialize()
		{
			GLTrnFilter.Cache.SetDefaultExt<GLTranFilter.currentCost>(GLTrnFilter.Current);
			GLTrnFilter.Cache.SetDefaultExt<GLTranFilter.accrualBalance>(GLTrnFilter.Current);
			GLTrnFilter.Cache.SetDefaultExt<GLTranFilter.unreconciledAmt>(GLTrnFilter.Current);
			GLTrnFilter.Cache.SetDefaultExt<GLTranFilter.selectionAmt>(GLTrnFilter.Current);
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (IsImport && viewName == nameof(AssetLocation))
			{
				if (AssetDetails.Current == null)
				{
					AssetDetails.Current = AssetDetails.Select();
				}
				if (keys.Count == 0)
				{
					keys.Add(typeof(FALocationHistory.assetID).Name, AssetDetails.Current.AssetID);
					keys.Add(typeof(FALocationHistory.revisionID).Name, AssetDetails.Current.LocationRevID);
				}
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		protected virtual void SetFABookBalanceUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (fasetup.Current.UpdateGL != true)
			{
				AssetBalance.Select().RowCast<FABookBalance>().ForEach(balance => sender.Graph.Caches<FABookBalance>().MarkUpdated(balance));
			}
		}

		public PXAction<FixedAsset> viewDocument;
		[PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			TransactionEntry graph = CreateInstance<TransactionEntry>();
			graph.Document.Current = graph.Document.Search<FARegister.refNbr>(FATransactions.Current.RefNbr);
			throw new PXRedirectRequiredException(graph, "FATransactions") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<FixedAsset> viewBatch;
		[PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			JournalEntry graph = CreateInstance<JournalEntry>();
			graph.BatchModule.Current = graph.BatchModule.Search<Batch.batchNbr>(FATransactions.Current.BatchNbr, BatchModule.FA);
			throw new PXRedirectRequiredException(graph, "Batch") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		protected virtual void GLTranFilter_UnreconciledAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranFilter filter = (GLTranFilter)e.Row;
			if (filter?.CurrentCost == null || filter.AccrualBalance == null) return;

			e.NewValue = filter.CurrentCost - filter.AccrualBalance;
		}

		protected virtual void GLTranFilter_TranDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			FABookPeriod currentPeriod = SelectFrom<FABookPeriod>
				.InnerJoin<FABookBalance>
					.On<FABookPeriod.bookID.IsEqual<FABookBalance.bookID>>
				.InnerJoin<FADetails>
					.On<FABookBalance.assetID.IsEqual<FADetails.assetID>>
				.InnerJoin<FinPeriod>
					.On<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID>
						.And<FABookPeriod.finPeriodID.IsEqual<FinPeriod.finPeriodID>>>
				.Where<FABookBalance.assetID.IsEqual<FixedAsset.assetID.FromCurrent.NoDefault>
					.And<FABookBalance.updateGL.IsEqual<True>>
					.And<FABookPeriod.endDate.IsNotEqual<FABookPeriod.startDate>>
					.And<FABookPeriod.organizationID.IsEqual<@P.AsInt>>
					.And<FADetails.receiptDate.IsGreaterEqual<FABookPeriod.startDate>>
					.And<FADetails.receiptDate.IsLess<FABookPeriod.endDate>>>
				.View
				.Select(this, PXAccess.GetParentOrganizationID(CurrentAsset.Current?.BranchID));

			if (currentPeriod == null) return;

			if (Accessinfo.BusinessDate >= currentPeriod.StartDate && Accessinfo.BusinessDate <= currentPeriod.EndDate)
			{
				e.NewValue = Accessinfo.BusinessDate;
			}
			else
			{
				e.NewValue = currentPeriod.StartDate;
			}
		}

		public virtual Standalone.FADetails FetchDetailsLite(FixedAsset asset)
		{
			return FetchDetailsLite(this, asset);
		}

		/// <summary>
		/// Performance improvement. Reduce selecting heavy projection FADetails and use lite Standalone.FADetails
		/// </summary>
		public static Standalone.FADetails FetchDetailsLite(PXGraph graph, FixedAsset asset)
		{
			Standalone.FADetails detailsLite;
			FADetails details = (FADetails)graph.Caches<FADetails>().Locate(new FADetails { AssetID = asset?.AssetID });
			if (details != null)
			{
				detailsLite = Common.Utilities.Clone<FADetails, Standalone.FADetails>(graph, details);
			}
			else
			{
				detailsLite = PXSelect<Standalone.FADetails,
					Where<Standalone.FADetails.assetID, Equal<Required<FixedAsset.assetID>>>>
					.Select(graph, asset?.AssetID);
			}
			return detailsLite;
		}

		protected virtual void _(Events.RowUpdating<FABookBalance> e)
		{
			FABookBalance fABookBalance = e.NewRow;

			if (fABookBalance.AcquisitionCost > 0 && fABookBalance.AcquisitionCost < (fABookBalance.Tax179Amount + fABookBalance.BonusAmount))
			{
				AssetBalance.Cache.RaiseExceptionHandling<FABookBalance.acquisitionCost>(fABookBalance, fABookBalance.AcquisitionCost, new PXSetPropertyException(Messages.AcquisitionCostLessThanTax179AndBonusAmounts, PXErrorLevel.RowError));
				e.Cancel = true;
			}
		}

		public override void Persist()
		{
			FixedAsset asset = CurrentAsset.Current;
			Standalone.FADetails det = FetchDetailsLite(asset);
			FALocationHistory currentLocation = this.GetCurrentLocation(det);
			//get nonpurchasing/nonreconcilliation first, then reconcilliation, purchasing last
			//purchasing plus first
			//filled batchnbr first
			//add primary key to retain sorting by expression
			PXSelectBase<FATran> cmdsel = new PXSelect<FATran,
				Where<FATran.assetID, Equal<Current<FABookBalance.assetID>>,
					And<FATran.bookID, Equal<Current<FABookBalance.bookID>>>>,
				OrderBy<Asc<Switch<Case<Where<FATran.origin, Equal<FARegister.origin.purchasing>>, int4, Case<Where<FATran.origin, Equal<FARegister.origin.reconcilliation>>, int1>>, int0>,
					Asc<Switch<Case<Where<FATran.tranType, Equal<FATran.tranType.purchasingPlus>>, int0>, int1>,
					Desc<FATran.batchNbr,
					Asc<FATran.refNbr,
					Asc<FATran.lineNbr>>>>>>>(this);

			Dictionary<FABookBalance, OperableTran> booktrn = new Dictionary<FABookBalance, OperableTran>();

			List<FARegister> created;

			FARegister register = SelectFrom<FARegister>
				.InnerJoin<FATran>.On<FARegister.refNbr.IsEqual<FATran.refNbr>>
				.Where<FARegister.origin.IsEqual<FARegister.origin.purchasing>
					.And<FARegister.released.IsEqual<False>
						.Or<FASetup.updateGL.FromCurrent.IsNotEqual<True>
							.And<FATran.batchNbr.IsNull>>>
					.And<FATran.tranType.IsEqual<FATran.tranType.purchasingPlus>>
					.And<FATran.assetID.IsEqual<FixedAsset.assetID.FromCurrent>>>
				.View
				.Select(this);

			foreach (FABookBalance bookbal in AssetBalance.Cache.Deleted)
			{
				FATran res = (FATran)cmdsel.View.SelectSingleBound(new object[] { bookbal });
				if (res != null && (res.Origin == FARegister.origin.Purchasing || res.Origin == FARegister.origin.Reconcilliation) && res.Released == false)
				{
					booktrn.Add(bookbal, new OperableTran(PXCache<FATran>.CreateCopy(res), PXDBOperation.Delete));
				}
			}

			bool assetTranExists = false;

			if (asset != null)
			{
				FATran existingTransaction = PXSelect<FATran,
					Where<FATran.assetID, Equal<Required<FATran.assetID>>>>
					.SelectSingleBound(this, null, asset.AssetID);
				assetTranExists = existingTransaction != null;

				FixedAsset faClass = AssetProcess.GetSourceForNewAccounts(this, asset);

				int? newFAAccountID = (int?)Asset.Cache.GetValue<FixedAsset.fAAccountID>(faClass);
				int? newFASubID = MakeSubID<FixedAsset.fASubMask, FixedAsset.fASubID>(Asset.Cache, asset);

				if ((asset.FAAccountID != newFAAccountID || asset.FASubID != newFASubID
					|| currentLocation.BranchID != asset.BranchID)
					&& register != null || !assetTranExists)
				{
					foreach (FABookBalance bookBalance in AssetBalance.Select())
					{
						AssetBalance.Cache.MarkUpdated(bookBalance);
					}
				}

				if (AssetBalance.Cache.Updated.Any_() && asset.IsConvertedFromAP(this) && !asset.SplittedFrom.HasValue)
				{
					if (Asset.Ask(Messages.ConfirmDeleteReconcilliationTransaction, MessageButtons.YesNo) != WebDialogResult.Yes)
					{
						return;
					}
				}
			}

			foreach (FABookBalance bookbal in AssetBalance.Cache.Updated)
			{
				CheckBookBalanceParameters(this, fasetup.Current.UpdateGL, currentLocation.BranchID, bookbal);
				FATran res = (FATran)cmdsel.View.SelectSingleBound(new object[] { bookbal });
				if (res != null &&
					(res.Origin == FARegister.origin.Purchasing || res.Origin == FARegister.origin.Reconcilliation) &&
					(res.Released == false || fasetup.Current.UpdateGL != true && string.IsNullOrEmpty(res.BatchNbr)))
				{
					booktrn.Add(bookbal, new OperableTran(PXCache<FATran>.CreateCopy(res), PXDBOperation.Update));
				}

				if (res == null)
				{
					booktrn.Add(bookbal, new OperableTran(null, PXDBOperation.Insert));
				}
			}

			foreach (FABookBalance bookbal in AssetBalance.Cache.Inserted)
			{
				CheckBookBalanceParameters(this, fasetup.Current.UpdateGL, currentLocation.BranchID, bookbal);
				booktrn.Add(bookbal, new OperableTran(null, PXDBOperation.Insert));
			}

			FARegister transferreg = null;

			if (asset != null && det.ReceiptDate != null)
			{
				FALocationHistory previousLocation = this.GetPrevLocation(currentLocation);
				FABookBalanceTransfer trbal = PXSelect<FABookBalanceTransfer, Where<FABookBalanceTransfer.assetID, Equal<Current<FixedAsset.assetID>>>, OrderBy<Desc<FABookBalanceTransfer.updateGL>>>.SelectSingleBound(this, new object[] { asset });
				FABookBalance glbal = PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>, OrderBy<Desc<FABookBalance.updateGL>>>.SelectSingleBound(this, new object[] { asset });

				//first location or newly created asset
				if (glbal == null
					|| (previousLocation == null && string.IsNullOrEmpty(currentLocation.PeriodID))
							|| trbal == null
							|| fasetup.Current.UpdateGL != true)
				{
					if (glbal?.BookID != null)
						currentLocation.PeriodID = FABookPeriodRepository.GetFABookPeriodIDOfDate(det.ReceiptDate, glbal.BookID, asset.AssetID);
					currentLocation.TransactionDate = det.ReceiptDate;
				}

				currentLocation.ClassID = asset.ClassID;
				AssetProcess.TransferAsset(this, asset, currentLocation, ref transferreg);

				if ((register != null || !assetTranExists)
					&& currentLocation.BranchID != asset.BranchID)
				{
					FixedAsset assetCopy = PXCache<FixedAsset>.CreateCopy(asset);

					assetCopy.BranchID = currentLocation.BranchID;

					Asset.Update(assetCopy);
			}
			}

			FARegister reconreg = null;

			using (new PXTimeStampScope(this.TimeStamp))
			{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				FARegister r = register;
				foreach (FATran tran in FATransactions.Cache.Inserted.Cast<FATran>().Where(tran => r != null))
				{
					tran.RefNbr = register.RefNbr;
					tran.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<FATran.lineNbr>(FATransactions.Cache, register);

					FATransactions.Cache.Normalize();
					PXParentAttribute.SetLeaveChildren<FATran.refNbr>(FATransactions.Cache, null, true);
					foreach (FARegister doc in Register.Cache.Inserted.Cast<FARegister>().Where(doc => doc.Origin == FARegister.origin.Purchasing))
					{
						Register.Cache.Delete(doc);
					}
					Register.Cache.Current = register;
				}

				foreach (FARegister item in Register.Cache.Inserted)
				{
					if (item.Origin == FARegister.origin.Purchasing)
					{
						register = item;
					}
					if (item.Origin == FARegister.origin.Reconcilliation)
					{
						reconreg = item;
					}
				}

				base.Persist();

				TransactionEntry docgraph = CreateInstance<TransactionEntry>();
					//docgraph.Clear() is the hack to set docgraph.Timestamp from PXTimeStampScope.
					//Using docgraph.SelectTimeStamp() can lead to data inconsistencies.
					docgraph.Clear(); 

				if (asset != null)
				{
					AssetProcess.AcquireAsset(docgraph, (int)asset.BranchID, booktrn, register);
				}

				created = docgraph.created;
				SelectTimeStamp();

				ts.Complete();
			}
			}

			FATransactions.Cache.Clear();

			PXHashSet<FARegister> toRelease = new PXHashSet<FARegister>(this);
			if (fasetup.Current.AutoReleaseAsset == true && created != null && created.Count > 0)
			{
				toRelease.Add(created[0]);
			}
			if (fasetup.Current.AutoReleaseAsset == true && register != null && register.Released != true)
			{
				toRelease.Add(register);
			}
			if (fasetup.Current.AutoReleaseAsset == true && reconreg != null)
			{
				toRelease.Add(reconreg);
			}
			if (fasetup.Current.AutoReleaseTransfer == true && transferreg != null)
			{
				toRelease.Add(transferreg);
			}

			if (asset != null && asset.Status != FixedAssetStatus.Hold && toRelease.Count > 0)
			{
				SelectTimeStamp();
				PXLongOperation.StartOperation(this, delegate { AssetTranRelease.ReleaseDoc(toRelease.ToList(), false); });
			}

			if(asset != null)
			{
				FixedAsset assetFromDB = SelectFrom<FixedAsset>.Where<FixedAsset.assetID.IsEqual<@P.AsInt>>.View.ReadOnly.Select(this, asset.AssetID.Value);
				Asset.Cache.RestoreCopy(Asset.Current, assetFromDB);
			}
		}

		protected int? AssetBookID(int idx)
		{
			return AssetBalance.Select()
				.RowCast<FABookBalance>()
				.FirstOrDefault(book => book?.BookID != null && _Books[book.BookID] == idx && book.Depreciate == true)
				.With(bal => bal.BookID);
		}

		protected virtual void PtdCalculatedFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, int fieldNbr, bool calc)
		{
			e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, 2, _FieldNames[2 * fieldNbr + (calc ? 0 : 1)], null, null, null, null);
			((PXDecimalState)e.ReturnState).Visibility = PXUIVisibility.Visible;
			int? bookID = AssetBookID(fieldNbr);
			((PXDecimalState)e.ReturnState).Visible = bookID != null;

			FAHistory history = (FAHistory)e.Row;
			if (history?.AssetID == null || !((PXDecimalState)e.ReturnState).Visible) return;

			FABookPeriod exist_period = PXSelect<
				FABookPeriod,
				Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
					And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
					And<FABookPeriod.finPeriodID, Equal<Required<FABookPeriod.finPeriodID>>>>>>
				.Select(
					this, 
					bookID, 
					FABookPeriodRepository.GetFABookPeriodOrganizationID(bookID, history.AssetID), 
					history.FinPeriodID);
			if (exist_period != null)
				e.ReturnValue = ((FAHistory)e.Row).PtdDepreciated[2 * fieldNbr + (calc ? 0 : 1)] ?? 0m;
		}

		protected virtual void BookSheetPeriodDepreciatedFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, int fieldNbr)
		{
			e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 7, true, _SheetFieldNames[3 * fieldNbr], false, null, "##-####", null, null, null, null);
			((PXStringState)e.ReturnState).Visibility = PXUIVisibility.Visible;

			FABookBalance bal = PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>,
				And<FABookBalance.bookID, Equal<Current<DeprBookFilter.bookID>>>>>.Select(this);

			if (bal == null)
			{
				((PXStringState)e.ReturnState).Visible = false;
			}
			else
			{
				int yearFrom = Convert.ToInt32(bal.DeprFromYear);
				int yearTo = Math.Max(Convert.ToInt32(bal.DeprToYear), Convert.ToInt32((bal.LastDeprPeriod ?? "1900").Substring(0, 4)));

				((PXStringState)e.ReturnState).Visible = yearFrom + fieldNbr <= yearTo;
			}
			((PXStringState)e.ReturnState).DisplayName = PXMessages.LocalizeNoPrefix(Messages.PeriodFieldName);

			FASheetHistory hist = (FASheetHistory)e.Row;
			if (hist?.PtdValues?[fieldNbr] == null) return;

			e.ReturnValue = FABookPeriodIDAttribute.FormatPeriod(hist.PtdValues[fieldNbr].PeriodID);
		}

		protected virtual void BookSheetPtdCalcFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, int fieldNbr, bool calc)
		{
			e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, 2, _SheetFieldNames[3 * fieldNbr + (calc ? 1 : 2)], null, null, null, null);
			((PXDecimalState)e.ReturnState).Visibility = PXUIVisibility.Visible;

			FABookBalance bal = PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>,
				And<FABookBalance.bookID, Equal<Current<DeprBookFilter.bookID>>>>>.Select(this);
			if (bal == null)
			{
				((PXDecimalState)e.ReturnState).Visible = false;
			}
			else
			{
				int yearFrom = Convert.ToInt32(bal.DeprFromYear);
				int yearTo = Math.Max(Convert.ToInt32(bal.DeprToYear), Convert.ToInt32((bal.LastDeprPeriod ?? "1900").Substring(0, 4)));

				((PXDecimalState)e.ReturnState).Visible = yearFrom + fieldNbr <= yearTo;
			}
			((PXDecimalState)e.ReturnState).DisplayName = calc ?
				PXMessages.LocalizeNoPrefix(Messages.CalcValueFieldName) :
				PXMessages.LocalizeNoPrefix(Messages.DeprValueFieldName);

			FASheetHistory hist = (FASheetHistory)e.Row;
			if (hist?.PtdValues?[fieldNbr] == null) return;

			e.ReturnValue = calc ? hist.PtdValues[fieldNbr].PtdCalculated : hist.PtdValues[fieldNbr].PtdDepreciated;
		}

		public virtual IEnumerable booksheethistory()
		{
			FixedAsset asset = CurrentAsset.Current;
			DeprBookFilter bookfltr = deprbookfilter.Current;
			if (bookfltr?.BookID == null || asset?.AssetID == null) yield break;

			FASheetHistory dyn = null;
			FABookBalance bal = PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>, And<FABookBalance.bookID, Equal<Current<DeprBookFilter.bookID>>>>>.Select(this);

			if (bal == null) yield break;

			string toYear = bal.DeprToYear;
			string lastDeprYear = string.IsNullOrEmpty(bal.LastDeprPeriod) ? toYear : bal.LastDeprPeriod.Substring(0, 4);
			if (string.CompareOrdinal(lastDeprYear, toYear) > 0)
			{
				toYear = lastDeprYear;
			}

			int fromYearInt;
			int.TryParse(bal.DeprFromYear, out fromYearInt);

			foreach (PXResult<FABookPeriod, FABookHistory> res in PXSelectJoin<
				FABookPeriod,
				LeftJoin<FABookHistory, 
					On<FABookPeriod.finPeriodID, Equal<FABookHistory.finPeriodID>,
					And<FABookPeriod.bookID, Equal<FABookHistory.bookID>,
					And<FABookHistory.assetID, Equal<Current<FixedAsset.assetID>>>>>>,
				Where<FABookPeriod.bookID, Equal<Current<DeprBookFilter.bookID>>,
					And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
					And<FABookPeriod.finYear, GreaterEqual<Required<FABookPeriod.finYear>>,
					And<FABookPeriod.finYear, LessEqual<Required<FABookPeriod.finYear>>,
					And<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>>>>>>,
				OrderBy<
					Asc<FABookPeriod.periodNbr, 
					Asc<FABookPeriod.finYear>>>>
				.Select(
					this,
					FABookPeriodRepository.GetFABookPeriodOrganizationID(bookfltr.BookID, asset.AssetID),
					bal.DeprFromYear, 
					toYear))
			{
				FABookHistory hist = res;
				FABookPeriod period = res;

				if (dyn != null && dyn.PeriodNbr != period.PeriodNbr)
				{
					yield return dyn;
				}
				if (dyn == null || dyn.PeriodNbr != period.PeriodNbr)
				{
					dyn = new FASheetHistory
					{
						AssetID = bal.AssetID,
						PeriodNbr = period.PeriodNbr,
						PtdValues = new FASheetHistory.PeriodDeprPair[_SheetFieldNames.Count / 3],
					};
				}
				decimal? calculated;
				if (fasetup.Current.AccurateDepreciation == true)
				{
					if (bal.CurrDeprPeriod == null || string.CompareOrdinal(hist.FinPeriodID, bal.CurrDeprPeriod) < 0)
					{
						calculated = hist.PtdDepreciated + hist.PtdAdjusted + hist.PtdDeprDisposed;
					}
					else if (string.CompareOrdinal(hist.FinPeriodID, bal.CurrDeprPeriod) > 0)
					{
						calculated = hist.PtdCalculated;
					}
					else //if (string.Compare(hist.FinPeriodID, bal.CurrDeprPeriod) == 0)
					{
						calculated = hist.YtdCalculated - hist.YtdDepreciated;
					}
				}
				else
				{
					calculated = hist.PtdCalculated;
				}

				int curYearInt;
				int.TryParse(period.FinYear, out curYearInt);

				int yearIndex = curYearInt - fromYearInt;
				if (dyn.PtdValues.Length > yearIndex)
				{
					dyn.PtdValues[yearIndex] = new FASheetHistory.PeriodDeprPair(period.FinPeriodID, hist.PtdDepreciated + hist.PtdAdjusted + hist.PtdDeprDisposed, calculated);
				}
			}
			yield return dyn;
		}

		public virtual IEnumerable assethistory()
		{
			string history_from = "207699";
			string history_to = "190001";

			AssetDetails.Current = AssetDetails.Select();
			if (AssetDetails.Current == null || AssetDetails.Current.AssetID < 0) yield break;

			Dictionary<int?, FABookBalance> balances = new Dictionary<int?, FABookBalance>();

			foreach (FABookBalance bookbal in AssetBalance.Select())
			{
				if (bookbal.Depreciate == true && (bookbal.DeprToDate != null || AssetDetails.Current.DepreciateFromDate != null))
				{
					string DeprFromPeriod = !string.IsNullOrEmpty(bookbal.DeprFromPeriod) 
						? bookbal.DeprFromPeriod 
						: FABookPeriodRepository.GetFABookPeriodIDOfDate(bookbal.DeprToDate ?? AssetDetails.Current.DepreciateFromDate, bookbal.BookID, bookbal.AssetID);
					string DeprToPeriod = !string.IsNullOrEmpty(bookbal.DeprToPeriod) ? bookbal.DeprToPeriod : DeprFromPeriod;
					string LastDeprPeriod = !string.IsNullOrEmpty(bookbal.LastDeprPeriod) ? bookbal.LastDeprPeriod : DeprToPeriod;
					if (string.CompareOrdinal(DeprFromPeriod, history_from) < 0)
					{
						history_from = DeprFromPeriod;
					}
					if (string.CompareOrdinal(LastDeprPeriod, DeprToPeriod) > 0)
					{
						DeprToPeriod = LastDeprPeriod;
					}
					if (string.CompareOrdinal(DeprToPeriod, history_to) > 0)
					{
						history_to = DeprToPeriod;
					}
				}
				balances[bookbal.BookID] = bookbal;
			}

			if (string.CompareOrdinal(history_from, history_to) > 0)
			{
				yield break;
			}

			FAHistory dyn = null;
			foreach (PXResult<FABookPeriod, FABookHistory> res in PXSelectReadonly2<
				FABookPeriod,
				LeftJoin<FABookHistory,
					On<FABookHistory.assetID, Equal<Current<FixedAsset.assetID>>,
					And<FABookHistory.bookID, Equal<FABookPeriod.bookID>,
					And<FABookHistory.finPeriodID, Equal<FABookPeriod.finPeriodID>>>>>,
				Where<FABookPeriod.finPeriodID, GreaterEqual<Required<FABookPeriod.finPeriodID>>,
					And<FABookPeriod.finPeriodID, LessEqual<Required<FABookPeriod.finPeriodID>>,
					And<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>>>>,
				OrderBy<
					Asc<FABookPeriod.finPeriodID>>>
				.Select(
					this,
					history_from, 
					history_to)
				.AsEnumerable()
				.Where(result => ((FABookPeriod)result[typeof(FABookPeriod)]).OrganizationID == 
					FABookPeriodRepository.GetFABookPeriodOrganizationID(
						((FABookPeriod)result[typeof(FABookPeriod)]).BookID,
						AssetDetails.Current.AssetID)))
			{
				FABookPeriod period = res;
				FABookHistory hist = res;

				FABookBalance balance;
				if (!balances.TryGetValue(period.BookID, out balance) || balance.Depreciate != true)
					continue;

				if (dyn != null && dyn.FinPeriodID != period.FinPeriodID)
				{
					yield return dyn;
				}
				if (dyn == null || dyn.FinPeriodID != period.FinPeriodID)
				{
					dyn = new FAHistory
					{
						AssetID = Asset.Current.AssetID,
						FinPeriodID = period.FinPeriodID,
						PtdDepreciated = new decimal?[_Books.Count * 2]
					};
				}
				if (fasetup.Current.AccurateDepreciation == true)
				{
					if (balance.CurrDeprPeriod == null || string.CompareOrdinal(hist.FinPeriodID, balance.CurrDeprPeriod) < 0)
					{
						dyn.PtdDepreciated[_Books[period.BookID] * 2] = hist.PtdDepreciated + hist.PtdAdjusted + hist.PtdDeprDisposed;
					}
					else if (string.CompareOrdinal(hist.FinPeriodID, balance.CurrDeprPeriod) > 0)
					{
						dyn.PtdDepreciated[_Books[period.BookID] * 2] = hist.PtdCalculated;
					}
					else //if(string.Compare(hist.FinPeriodID, balance.CurrDeprPeriod) == 0)
					{
						dyn.PtdDepreciated[_Books[period.BookID] * 2] = hist.YtdCalculated - hist.YtdDepreciated;
					}
				}
				else
				{
					dyn.PtdDepreciated[_Books[period.BookID] * 2] = hist.PtdCalculated;
				}
				dyn.PtdDepreciated[_Books[period.BookID] * 2 + 1] = hist.PtdDepreciated + hist.PtdAdjusted + hist.PtdDeprDisposed;
			}
			yield return dyn;
		}
		#endregion

		#region View Delegates
		public override IEnumerable<PXDataRecord> ProviderSelect(BqlCommand command, int topCount, params PXDataValue[] pars)
		{
			Type[] tables = command.GetTables();
			if (tables != null && tables.Length > 1 && tables[0] == typeof(FAAccrualTran))
				command = command.OrderByNew<OrderBy<Asc<FAAccrualTran.gLTranID>>>();
			return base.ProviderSelect(command, topCount, pars);
		}
		#endregion

		#region Workflow Events Handler
		public PXWorkflowEventHandler<FixedAsset> OnUpdateStatus;
		public PXWorkflowEventHandler<FixedAsset> OnActivateAsset;
		public PXWorkflowEventHandler<FixedAsset> OnDisposeAsset;
		public PXWorkflowEventHandler<FixedAsset> OnSuspendAsset;
		public PXWorkflowEventHandler<FixedAsset> OnFullyDepreciateAsset;
		public PXWorkflowEventHandler<FixedAsset> OnNotFullyDepreciateAsset;
		public PXWorkflowEventHandler<FixedAsset> OnReverseAsset;
		
		#endregion


		#region Buttons
		public PXInitializeState<FixedAsset> initializeState;

		public PXAction<FixedAsset> putOnHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable PutOnHold(PXAdapter adapter) => adapter.Get();

		public PXAction<FixedAsset> releaseFromHold;

		[PXButton(CommitChanges = true),
		 PXUIField(DisplayName = "Remove Hold", MapEnableRights = PXCacheRights.Select,
			 MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable ReleaseFromHold(PXAdapter adapter)
		{
			foreach (FixedAsset row in adapter.Get())
			{
				this.Asset.Current = row;
				FABookBalance balance = AssetBalance.Select()
					.RowCast<FABookBalance>()
					.Where(bookbal => fasetup.Current.UpdateGL == true && bookbal.UpdateGL == true && string.IsNullOrEmpty(bookbal.InitPeriod))
					.Select(bookbal => new
					{
						bookbal,
						AcquiredCost = PXSelect<FATran, Where<FATran.assetID, Equal<Current<FABookBalance.assetID>>,
								And<FATran.bookID, Equal<Current<FABookBalance.bookID>>,
									And<FATran.tranType, Equal<FATran.tranType.purchasingPlus>>>>>.SelectMultiBound(this, new object[] { bookbal })
							.RowCast<FATran>()
							.Aggregate<FATran, decimal?>(0m, (current, tran) => current + tran.TranAmt)
					})
					.Where(t => t.bookbal.AcquisitionCost - t.AcquiredCost >= 0.00005m).Select(t => t.bookbal).FirstOrDefault();
				if (balance != null)
				{
					throw new PXSetPropertyException(Messages.UnholdAssetOutOfBalance, PXCurrencyAttribute.BaseRound(this, balance.AcquisitionCost));
				}

				yield return row;
			}
		}

		public PXAction<FixedAsset> runReversal;
		[PXUIField(DisplayName = Messages.Reverse, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		// TODO: Split parameters between appropriate actions (waiting for Andrew Boulanov)
		public virtual IEnumerable RunReversal(PXAdapter adapter,
			[PXDate]
			DateTime? disposalDate,
			[PXString]
			string disposalPeriodID,
			[PXBaseCury]
			decimal? disposalCost,
			[PXInt]
			int? disposalMethID,
			[PXInt]
			int? disposalAcctID,
			[PXInt]
			int? disposalSubID,
			[PXString]
			string dispAmtMode,
			[PXBool]
			bool? deprBeforeDisposal,
			[PXString]
			string reason,
			[PXInt]
			int? assetID
			)
		{
			if (adapter.MassProcess)
			{
				throw new NotImplementedException();
			}
			else
			{
				Save.Press();
				AssetProcess.CheckUnreleasedTransactions(this, (FixedAsset)Caches[typeof(FixedAsset)].Current);
				PXLongOperation.StartOperation(this, delegate { AssetProcess.ReverseAsset(CurrentAsset.Current, fasetup.Current); });
				return adapter.Get();
			}
		}

		public PXAction<FixedAsset> runDispReversal;
		[PXUIField(DisplayName = Messages.DispReverse, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		// TODO: Split parameters between appropriate actions (waiting for Andrew Boulanov)
		public virtual IEnumerable RunDispReversal(PXAdapter adapter,
			[PXDate]
			DateTime? disposalDate,
			[PXString]
			string disposalPeriodID,
			[PXBaseCury]
			decimal? disposalCost,
			[PXInt]
			int? disposalMethID,
			[PXInt]
			int? disposalAcctID,
			[PXInt]
			int? disposalSubID,
			[PXString]
			string dispAmtMode,
			[PXBool]
			bool? deprBeforeDisposal,
			[PXString]
			string reason,
			[PXInt]
			int? assetID
			)
		{
			if (!adapter.MassProcess)
			{
				//When this action is called from import or export scenarios (which can be determined by IsImport flag) then RevDispInfo.View.Answer is already set to OK.
				//In this case RevDispInfo fields must be initialized to perform operation (or ReverseDisposal will not be called because of !RevDispInfo.VerifyRequired).
				if (RevDispInfo.View.Answer == WebDialogResult.None || IsImport)
				{
					AssetDetails.Current = AssetDetails.Select();
					RevDispInfo.Cache.Remove(RevDispInfo.Current);
					RevDispInfo.Insert();
					int? organizationID = PXAccess.GetParentOrganizationID(CurrentAsset.Current.BranchID);

					if (AssetDetails.Current.DisposalDate != null &&
						FinPeriodRepository.FindFinPeriodByDate(AssetDetails.Current.DisposalDate, organizationID).FAClosed != true)
					{
						RevDispInfo.Current.ReverseDisposalDate = AssetDetails.Current.DisposalDate;
					}

					string periodID = FinPeriodRepository.FindFinPeriodByDate(RevDispInfo.Current.ReverseDisposalDate, organizationID)?.FinPeriodID;

					if (PXSelectorAttribute.Select<ReverseDisposalInfo.reverseDisposalPeriodID>(RevDispInfo.Cache, RevDispInfo.Cache.Current, FinPeriodIDFormattingAttribute.FormatForDisplay(periodID)) == null)
					{
						//don't use PXSelectorAttribute.SelectFirst - it does not invoke custom selector delegate
						FinPeriod period =
							PXSelectorAttribute
								.SelectAll<ReverseDisposalInfo.reverseDisposalPeriodID>(RevDispInfo.Cache, RevDispInfo.Cache.Current)
								.RowCast<FinPeriod>()
								.Where(p => p.StartDate >= RevDispInfo.Current.DisposalDate && p.EndDate <= RevDispInfo.Current.DisposalDate )
								?.FirstOrDefault();

						if (period != null)
						{
							periodID = period.FinPeriodID;
						}
					}

					RevDispInfo.Current.ReverseDisposalPeriodID = periodID;
				}

				if (RevDispInfo.View.Answer == WebDialogResult.None)
				{
					RevDispInfo.AskExt();
				}
				else if (RevDispInfo.View.Answer != WebDialogResult.OK || !RevDispInfo.VerifyRequired())
				{
					return adapter.Get();
				}
			}
			else
			{
				throw new NotImplementedException();
			}
			Save.Press();
			AssetProcess.CheckUnreleasedTransactions(this, CurrentAsset.Current);
			PXLongOperation.StartOperation(this, delegate { AssetProcess.ReverseDisposal(CurrentAsset.Current, RevDispInfo.Current.ReverseDisposalDate, RevDispInfo.Current.ReverseDisposalPeriodID); });
			return adapter.Get();
		}

		public PXAction<FixedAsset> disposalOK;
		[PXUIField(DisplayName = "OK", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable DisposalOK(PXAdapter adapter)
		{
			DispParams.VerifyRequired();
			DisposeParams prm = (DisposeParams)DispParams.Cache.Current;
			if (DispParams.Cache.Fields.Select(field => PXUIFieldAttribute.GetErrorOnly(DispParams.Cache, DispParams.Cache.Current, field)).Any(err => !string.IsNullOrEmpty(err)))
			{
				return adapter.Get();
			}

			FixedAsset asset = CurrentAsset.Current;
			FADetails det = PXSelect<FADetails, Where<FADetails.assetID, Equal<Current<FixedAsset.assetID>>>>.SelectSingleBound(this, new object[] { asset });
			PXResultset<FixedAsset, FADetails> list = new PXResultset<FixedAsset, FADetails>
			{
				new PXResult<FixedAsset, FADetails>(asset, det)
			};
			Dispose(list, det.CurrentCost ?? 0m, prm.DisposalDate, prm.DisposalPeriodID, prm.DisposalAmt, prm.DisposalMethodID,
				prm.DisposalAccountID, prm.DisposalSubID, null, prm.ActionBeforeDisposal == DisposeParams.actionBeforeDisposal.Depreciate, prm.Reason, false);
			return adapter.Get();
		}

		protected virtual void Dispose(
			PXResultset<FixedAsset, FADetails> list,
			decimal cost,
			DateTime? disposalDate,
			string disposalPeriodID,
			decimal? disposalCost,
			int? disposalMethID,
			int? disposalAcctID,
			int? disposalSubID,
			string dispAmtMode,
			bool? deprBeforeDisposal,
			string reason,
			bool isMassProcess)
		{
			decimal accumulatedDisposalAmount = 0m;
			FADetails mostExpensiveAssetDet = null;

			foreach (PXResult<FixedAsset, FADetails> res in list)
			{
				FixedAsset asset = res;
				FADetails det = res;
				decimal assetDisposalAmt = cost == 0m
									   ? (disposalCost ?? 0m) / list.Count
									   : (disposalCost ?? 0m) * (det.CurrentCost ?? 0m) / cost;
				if (dispAmtMode == DisposalProcess.DisposalFilter.disposalAmtMode.Manual)
				{
					if (asset.DisposalAmt == null)
					{
						throw new PXException(Messages.DispAmtIsEmpty);
					}
					assetDisposalAmt = (decimal)asset.DisposalAmt;
				}

				asset.DisposalAccountID = disposalAcctID;
				asset.DisposalSubID = disposalSubID;
				det.DisposalDate = disposalDate;
				det.DisposalMethodID = disposalMethID;
				det.SaleAmount = PXCurrencyAttribute.BaseRound(this, assetDisposalAmt);
				det.DisposalPeriodID = disposalPeriodID;

				Asset.Update(asset);
				det = AssetDetails.Update(det);
				Save.Press();

				accumulatedDisposalAmount += det.SaleAmount.Value;
				if (mostExpensiveAssetDet == null || mostExpensiveAssetDet.SaleAmount < det.SaleAmount)
				{
					mostExpensiveAssetDet = det;
			}
			}

			decimal discrepancy = (disposalCost ?? 0m) - accumulatedDisposalAmount;
			if (dispAmtMode == DisposalProcess.DisposalFilter.disposalAmtMode.Automatic && discrepancy != 0m)
			{
				mostExpensiveAssetDet.SaleAmount = mostExpensiveAssetDet.SaleAmount + discrepancy;
				AssetDetails.Update(mostExpensiveAssetDet);
				Save.Press();
			}

			PXLongOperation.StartOperation(this, delegate
			{
				DocumentList<FARegister> created = AssetProcess.DisposeAsset(list, fasetup.Current, false, deprBeforeDisposal == true, reason);

				if (!(fasetup.Current.AutoReleaseDisp ?? false) && isMassProcess)
				{
					AssetTranRelease graph = CreateInstance<AssetTranRelease>();
					AssetTranRelease.ReleaseFilter filter = (AssetTranRelease.ReleaseFilter)graph.Filter.Cache.CreateCopy(graph.Filter.Current);
					filter.Origin = FARegister.origin.Disposal;
					graph.Filter.Update(filter);
					graph.SelectTimeStamp();
					int i = 0;
					Dictionary<string, string> parameters = new Dictionary<string, string>();
					foreach (FARegister register in created)
					{
						register.Selected = true;
						graph.FADocumentList.Update(register);
						graph.FADocumentList.Cache.SetStatus(register, PXEntryStatus.Updated);
						graph.FADocumentList.Cache.IsDirty = false;
						parameters["FARegister.RefNbr" + i] = register.RefNbr;
						i++;
					}

					parameters["DateFrom"] = null;
					parameters["DateTo"] = null;

					PXReportRequiredException reportex = new PXReportRequiredException(parameters, "FA680010", "Preview");
					throw new PXRedirectWithReportException(graph, reportex, "Release FA Transaction");
				}
			});
		}

		public PXAction<FixedAsset> runDisposal;
		[PXUIField(DisplayName = Messages.Dispose, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		// TODO: Split parameters between appropriate actions (waiting for Andrew Boulanov)
		public virtual IEnumerable RunDisposal(PXAdapter adapter,
			[PXDate]
			DateTime? disposalDate,
			[PXString]
			string disposalPeriodID,
			[PXBaseCury]
			decimal? disposalCost,
			[PXInt]
			int? disposalMethID,
			[PXInt]
			int? disposalAcctID,
			[PXInt]
			int? disposalSubID,
			[PXString]
			string dispAmtMode,
			[PXBool]
			bool? deprBeforeDisposal,
			[PXString]
			string reason,
			[PXInt]
			int? assetID
			)
		{
			PXResultset<FixedAsset, FADetails> list = new PXResultset<FixedAsset, FADetails>();
			decimal sum = 0m;
			foreach (FixedAsset asset in adapter.Get())
			{
				if (asset.Status == FixedAssetStatus.Disposed)
				{
					throw new PXException(Messages.AssetAlreadyDisposedFromImportScenario);
				}

				FADetails det = PXSelect<FADetails, Where<FADetails.assetID, Equal<Current<FixedAsset.assetID>>>>.SelectSingleBound(this, new object[] { asset });
				list.Add(new PXResult<FixedAsset, FADetails>(asset, det));
				sum += det.CurrentCost ?? 0m;
			}
			if (list.Count == 0)
				return list;

			if (!adapter.MassProcess)
			{
				PXView.InitializePanel disposalInit = delegate
				{
					DisposeParams param = DispParams.Current;
					FADetails assetdet = list[0].GetItem<FADetails>();

					if (param.DisposalMethodID == null)
					{
						param.DisposalMethodID = assetdet.DisposalMethodID;
						param.DisposalDate = assetdet.DisposalDate;
						param.DisposalAmt = assetdet.SaleAmount ?? 0m;
						DispParams.Cache.SetDefaultExt<DisposeParams.disposalPeriodID>(param);
					}

					if (param.DisposalDate == null)
					{
						param.DisposalDate = Accessinfo.BusinessDate;
						DispParams.Cache.SetDefaultExt<DisposeParams.disposalPeriodID>(param);
					}

					if (param.DisposalAccountID == null)
					{
						DispParams.Cache.SetDefaultExt<DisposeParams.disposalAccountID>(param);
						DispParams.Cache.SetDefaultExt<DisposeParams.disposalSubID>(param);
					}

					string lastAdditionPeriodID = GetMostRecentTransactionPeriodID(assetdet.AssetID);
					if (param.DisposalPeriodID == null || String.CompareOrdinal(param.DisposalPeriodID, lastAdditionPeriodID) < 0)
					{
						param.DisposalPeriodID = lastAdditionPeriodID;
						DispParams.Cache.SetDefaultExt<DisposeParams.disposalDate>(param);
					}

					param.ActionBeforeDisposal = fasetup.Current.AutoReleaseDepr == true
						? DisposeParams.actionBeforeDisposal.Depreciate
						: DisposeParams.actionBeforeDisposal.Suspend;
				};

				DispParams.AskExt(disposalInit);
				return list;
			}

			Dispose(list, sum, disposalDate, disposalPeriodID, disposalCost, disposalMethID, disposalAcctID,
						disposalSubID, dispAmtMode, deprBeforeDisposal, reason, adapter.MassProcess);
			return list;
		}

		public void GLTranFilterPeriodIDFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		public PXAction<FixedAsset> runSplit;
		[PXUIField(DisplayName = Messages.Split, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		// TODO: Split parameters between appropriate actions (waiting for Andrew Boulanov)
		public virtual IEnumerable RunSplit(PXAdapter adapter,
			[PXDate]
			DateTime? disposalDate,
			[PXString]
			string disposalPeriodID,
			[PXBaseCury]
			decimal? disposalCost,
			[PXInt]
			int? disposalMethID,
			[PXInt]
			int? disposalAcctID,
			[PXInt]
			int? disposalSubID,
			[PXString]
			string dispAmtMode,
			[PXBool]
			bool? deprBeforeDisposal,
			[PXString]
			string reason,
			[PXInt]
			int? assetID
			)
		{
			List<SplitParams> list = new List<SplitParams>();
			if (adapter.MassProcess)
			{
				Numbering numbering = assetNumbering.Select();

				FixedAsset asset = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(this, assetID);
				AssetProcess.CheckUnreleasedTransactions(this, asset);
				FADetails det = PXSelect<FADetails, Where<FADetails.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(this, assetID);
				FALocationHistory lochist = PXSelect<FALocationHistory, Where<FALocationHistory.assetID, Equal<Current<FADetails.assetID>>, And<FALocationHistory.revisionID, Equal<Current<FADetails.locationRevID>>>>>.SelectSingleBound(this, new object[] { det });
				FABookBalance bal = PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(this, assetID);

				decimal tcost = 0m;
				decimal tqty = 0m;
				foreach (SplitParams parms in adapter.Get())
				{
					PXProcessing<SplitParams>.SetCurrentItem(parms);

					if (string.IsNullOrEmpty(parms.SplittedAssetCD) && numbering.UserNumbering == true)
					{
						throw new PXSetPropertyException(Messages.CannotCreateAsset);
					}

					tcost += parms.Cost ?? 0m;
					if (Math.Abs(tcost) > Math.Abs(bal.YtdDeprBase ?? 0m))
					{
						throw new PXSetPropertyException(Messages.SplittedCostGreatherOrigin);
					}
					tqty += parms.SplittedQty ?? 0m;
					list.Add(parms);
				}

				string descriptionTemplate = string.Format("{0} {1}", PXMessages.LocalizeNoPrefix(Messages.SplitAssetDesc), asset.AssetCD);
				if (!string.IsNullOrEmpty(asset.Description))
				{
					descriptionTemplate = string.Format("{0} - {1}", "{0}", descriptionTemplate);
				}

				Dictionary<int, (decimal BonusAmount, decimal Tax179Amount)> sourceBonusTax179Amounts = new Dictionary<int, (decimal BonusAmount, decimal Tax179Amount)>();

				decimal totalSplittedSalvageAmount = 0m;
				Dictionary<FixedAsset, decimal> ratio = new Dictionary<FixedAsset, decimal>();
				foreach (SplitParams split in list)
				{
					FixedAsset split_asset = (FixedAsset)Asset.Cache.CreateCopy(asset);
					split_asset.NoteID = null;
					split_asset.AssetID = null;
					split_asset.AssetCD = split.SplittedAssetCD;
					split_asset.ClassID = null;
					split_asset.Description = string.IsNullOrEmpty(asset.Description)
						? descriptionTemplate
						: this.MakeDescription<FixedAsset.description>(descriptionTemplate, asset.Description);
					split_asset.Qty = split.SplittedQty;
					string assetCD = AssetGLTransactions.GetTempKey<FixedAsset.assetCD>(Asset.Cache);
					split_asset = Asset.Insert(split_asset);
					split_asset.AssetCD = split.SplittedAssetCD ?? assetCD;
					Asset.Cache.Normalize();
					split_asset.ClassID = asset.ClassID;
					split_asset.FAAccountID = asset.FAAccountID;
					split_asset.FASubID = asset.FASubID;
					split_asset.AccumulatedDepreciationAccountID = asset.AccumulatedDepreciationAccountID;
					split_asset.AccumulatedDepreciationSubID = asset.AccumulatedDepreciationSubID;
					split_asset.DepreciatedExpenseAccountID = asset.DepreciatedExpenseAccountID;
					split_asset.DepreciatedExpenseSubID = asset.DepreciatedExpenseSubID;
					split_asset.FAAccrualAcctID = asset.FAAccrualAcctID;
					split_asset.FAAccrualSubID = asset.FAAccrualSubID;
					split_asset.SplittedFrom = asset.AssetID;

					// To prevent verifying of GLTranFilter.PeriodID default value, AC-64486
					FieldVerifying.AddHandler<GLTranFilter.periodID>(GLTranFilterPeriodIDFieldVerifying);

					FADetails split_det = (FADetails)AssetDetails.Cache.CreateCopy(det);
					split_det.AssetID = split_asset.AssetID;
					split_det.AcquisitionCost = split.Cost;
					split_det.SalvageAmount = PXRounder.RoundBaseCury((det.SalvageAmount * split.Ratio / 100m) ?? 0m);
					split_det.LocationRevID = AssetDetails.Current.LocationRevID;
					split_det = AssetDetails.Update(split_det);

					totalSplittedSalvageAmount += split_det.SalvageAmount ?? 0m;

					FieldVerifying.RemoveHandler<GLTranFilter.periodID>(GLTranFilterPeriodIDFieldVerifying);

					FALocationHistory split_hist = (FALocationHistory)AssetLocation.Cache.CreateCopy(lochist);
					split_hist.AssetID = split_det.AssetID;
					split_hist.RevisionID = split_det.LocationRevID;
					split_hist.Department = lochist.Department;
					split_hist.PeriodID = lochist.PeriodID;
					AssetLocation.Update(split_hist);

					foreach (FABookBalance bookbal in PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>>.SelectMultiBound(this, new object[] { asset }))
					{
						FABookBalance split_bal = (FABookBalance)AssetBalance.Cache.CreateCopy(bookbal);
						split_bal.NoteID = null;
						split_bal.AssetID = split_asset.AssetID;
						split_bal.MaxHistoryPeriodID = null;
						split_bal.UsefulLife = null; //prevent an exception on defaulting in the PXFormulaAttribute on DepreciationMethodID
						split_bal = AssetBalance.Insert(split_bal);
						split_bal.UsefulLife = bookbal.UsefulLife;
						split_bal.AcquisitionCost = PXRounder.RoundBaseCury((bookbal.YtdAcquired * split.Ratio / 100) ?? 0m);
						split_bal.SalvageAmount = split_det.SalvageAmount;

						(decimal BonusAmount, decimal Tax179Amount) sourceBonusTax179Amount;
						if(!sourceBonusTax179Amounts.TryGetValue((int)bookbal.BookID, out sourceBonusTax179Amount))
						{
							sourceBonusTax179Amounts[(int)bookbal.BookID] = sourceBonusTax179Amount = (bookbal.BonusAmount ?? 0m, bookbal.Tax179Amount ?? 0m);
						}

						split_bal.BonusAmount = PXRounder.RoundBaseCury((sourceBonusTax179Amount.BonusAmount * split.Ratio / 100m) ?? 0m);
						split_bal.Tax179Amount = PXRounder.RoundBaseCury((sourceBonusTax179Amount.Tax179Amount * split.Ratio / 100m) ?? 0m);

						bookbal.BonusAmount -= split_bal.BonusAmount;
						bookbal.Tax179Amount -= split_bal.Tax179Amount;
						AssetBalance.Update(bookbal);
					}

					Save.Press();
					split.SplittedAssetCD = split_asset.AssetCD;
					ratio.Add(split_asset, (decimal)split.Ratio);
				}
				asset.Qty -= tqty;
				if (asset.Qty <= 0m) asset.Qty = 1m;
				Asset.Update(asset);

				Save.Press();

				// Pass actual salvage amount to update the fixed asses after depreciation, AC-171955
				asset.SalvageAmtAfterSplit = det.SalvageAmount - totalSplittedSalvageAmount;

				PXLongOperation.StartOperation(this, delegate
				{
					AssetProcess.SplitAsset(asset, disposalDate, disposalPeriodID, deprBeforeDisposal == true, ratio);
				});
				return list;
			}
			else
			{
				Save.Press();
				PXLongOperation.StartOperation(this, delegate
				{
					SplitProcess graph = CreateInstance<SplitProcess>();
					graph.Filter.Current = new SplitProcess.SplitFilter { AssetID = CurrentAsset.Current.AssetID };
					graph.Filter.Insert(graph.Filter.Current);
					graph.Filter.Cache.IsDirty = false;
					throw new PXRedirectRequiredException(graph, true, "SplitProcess") { Mode = PXBaseRedirectException.WindowMode.Same };
				});

				return adapter.Get();
			}
		}

		public PXAction<FixedAsset> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder, MenuAutoOpen = true, CommitChanges = true)]
		// TODO: Split parameters between appropriate actions (waiting for Andrew Boulanov)
		protected virtual IEnumerable Action(PXAdapter adapter,
			[PXDate]
			DateTime? disposalDate,
			[PXString]
			string disposalPeriodID,
			[PXBaseCury]
			decimal? disposalCost,
			[PXInt]
			int? disposalMethID,
			[PXInt]
			int? disposalAcctID,
			[PXInt]
			int? disposalSubID,
			[PXString]
			string dispAmtMode,
			[PXBool]
			bool? deprBeforeDisposal,
			[PXString]
			string reason,
			[PXInt]
			int? assetID
			)
		{
			return adapter.Get();
		}

		#region Button Create Lines
		public PXAction<FixedAsset> CalculateDepreciation;
		[PXUIField(DisplayName = Messages.CalculateDepreciation, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable calculateDepreciation(PXAdapter adapter)
		{
			FixedAsset fixedAsset = Asset.Current;
			FADetails assetDetails = AssetDetails.Select();

			if (fixedAsset != null && fixedAsset.ClassID != null && assetDetails != null && assetDetails.DepreciateFromDate != null)
			{
				Save.Press();
				DepreciationCalculation(fixedAsset);
				Save.Press();
			}
			Caches[typeof(FABookPeriod)].ClearQueryCache();
			Caches[typeof(FABookHistory)].ClearQueryCache();
			if (!IsContractBasedAPI && !IsImport)
			{
				return Cancel.Press(adapter);
			}
			return adapter.Get();
		}
		#endregion
		#region Button Suspend
		public PXAction<FixedAsset> Suspend;
		[PXUIField(DisplayName = Messages.Suspend, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable suspend(PXAdapter adapter,
			[PXString(6)]
			string CurrentPeriodID)
		{
			List<FixedAsset> list = adapter.Get().Cast<FixedAsset>().ToList();

			if (adapter.MassProcess == false && list.Count > 0 && list[0].Suspended == true)
			{
				if (SuspendParams.View.Answer == WebDialogResult.None)
				{
					SuspendParams.Cache.SetDefaultExt<SuspendParams.currentPeriodID>(SuspendParams.Current);
				}

				if (SuspendParams.AskExt() != WebDialogResult.OK)
				{
					return adapter.Get();
				}

				CurrentPeriodID = SuspendParams.Current.CurrentPeriodID;
			}

			PXLongOperation.StartOperation(this, delegate
			{
				foreach (FixedAsset asset in list)
				{
					if (asset.Suspended == false)
					{
						AssetProcess.SuspendAsset(asset);
					}
					else
					{
						AssetProcess.UnsuspendAsset(asset, CurrentPeriodID);
					}
				}
			});

			return adapter.Get();
		}
		#endregion
		#region Button Dispose
		#endregion

		public PXAction<FixedAsset> ReduceUnreconCost;
		[PXUIField(DisplayName = Messages.ReduceUnreconCost, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable reduceUnreconCost(PXAdapter adapter)
		{
			FixedAsset asset = CurrentAsset.Current;
			GLTranFilter filter = GLTrnFilter.Current;

			if (filter.CurrentCost != filter.ExpectedCost) //allow reconciliation except addition and deduction
			{
				AssetProcess.RestrictAdditonDeductionForCalcMethod(this, asset.AssetID, FADepreciationMethod.depreciationMethod.AustralianPrimeCost);
				AssetProcess.RestrictAdditonDeductionForCalcMethod(this, asset.AssetID, FADepreciationMethod.depreciationMethod.NewZealandStraightLine);
				AssetProcess.RestrictAdditonDeductionForCalcMethod(this, asset.AssetID, FADepreciationMethod.depreciationMethod.NewZealandStraightLineEvenly);
			}

			if (Register.Current == null || (Register.Current.Released ?? false))
			{
				AssetGLTransactions.SetCurrentRegister(Register, (int)asset.BranchID);
			}
			foreach (FABookBalance bal in PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>>.Select(this))
			{
				FATran tran = new FATran
				{
					AssetID = asset.AssetID,
					BookID = bal.BookID,
					TranAmt = filter.UnreconciledAmt,
					TranDate = filter.TranDate,
					TranType = FATran.tranType.PurchasingMinus,
					CreditAccountID = asset.FAAccountID,
					CreditSubID = asset.FASubID,
					DebitAccountID = asset.FAAccrualAcctID,
					DebitSubID = asset.FAAccrualSubID,
					TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.ReduceUnreconciledCost, asset.AssetCD),
				};
				FATransactions.Insert(tran);
			}

			return adapter.Get();
		}


		#endregion

		#region Functions
		private void DepreciationCalculation(FixedAsset fixedAsset)
		{
			if (fixedAsset.Depreciable != true  || fixedAsset.UnderConstruction == true) return;

			foreach (FABookBalance bookBalance in PXSelectReadonly<FABookBalance, Where<FABookBalance.assetID, Equal<Required<FABookBalance.assetID>>,
				And<FABookBalance.depreciationMethodID, IsNotNull>>>.Select(this, fixedAsset.AssetID))
			{
				DepreciationCalculation graph = CreateInstance<DepreciationCalculation>();
				graph.Calculate(bookBalance, null, this);
			}
		}

		private static void CopyLocation(FALocationHistory from, FALocationHistory to)
		{
			to.ClassID = from.ClassID;

			to.LocationID = from.LocationID;
			to.BuildingID = from.BuildingID;
			to.Floor = from.Floor;
			to.Room = from.Room;
			to.EmployeeID = from.EmployeeID;
			to.Department = from.Department;
			to.Reason = from.Reason;

			to.FAAccountID = from.FAAccountID;
			to.FASubID = from.FASubID;
			to.AccumulatedDepreciationAccountID = from.AccumulatedDepreciationAccountID;
			to.AccumulatedDepreciationSubID = from.AccumulatedDepreciationSubID;
			to.DepreciatedExpenseAccountID = from.DepreciatedExpenseAccountID;
			to.DepreciatedExpenseSubID = from.DepreciatedExpenseSubID;
			to.DisposalAccountID = from.DisposalAccountID;
			to.DisposalSubID = from.DisposalSubID;
			to.GainAcctID = from.GainAcctID;
			to.GainSubID = from.GainSubID;
			to.LossAcctID = from.LossAcctID;
			to.LossSubID = from.LossSubID;
		}

		public static (decimal MinValue, decimal MaxValue) GetSignedRange(decimal? value) => (Math.Min(0, value ?? 0m), Math.Max(0, value ?? 0m));

		public static bool IsValueInSignedRange(decimal? value, decimal? boundValue, out (decimal MinValue, decimal MaxValue) range)
		{
			range = GetSignedRange(boundValue);
			return value >= range.MinValue && value <= range.MaxValue;
		}

		protected bool HasAnyGLTran(int? assetID)
		{
			return SelectFrom<FATran>
					.Where<FATran.assetID.IsEqual<@P.AsInt>
						.And<FATran.batchNbr.IsNotNull>>.View.ReadOnly.Select(this, assetID).Any();
		}

		[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R2)]
		public static void CheckPostingBookBalanceParameters(PXGraph graph, bool? UpdateGL, int? branchID, FABookBalance bookBalance)
		{ }

		public static void CheckBookBalanceParameters(PXGraph graph, bool? UpdateGL, int? branchID, FABookBalance bookBalance)
		{
			PXAccess.Organization organization = PXAccess.GetParentOrganization(branchID);
			PXCache cache = graph.Caches[typeof(FABookBalance)];

			if (UpdateGL == true)
			{
				if (!IsPostingBookBalanceParametersValidInNonMigrationMode(graph, organization?.OrganizationID, bookBalance))
				{
					cache.RaiseExceptionHandling<FABookBalance.deprToPeriod>(
						bookBalance,
						bookBalance.DeprToPeriod,
						new PXSetPropertyException(
							Messages.DeprToPeriodInThePostingBookIsClosed,
							organization?.OrganizationCD));
				}
			}
			else
			{
				if (!IsPostingBookBalanceParametersValidInMigrationMode(graph, organization?.OrganizationID, bookBalance))
				{
					cache.RaiseExceptionHandling<FABookBalance.deprToPeriod>(
						bookBalance,
						bookBalance.DeprToPeriod,
						new PXSetPropertyException(
							Messages.NonzeroNetBookValueAndDeprToPeriodInThePostingBookIsClosed,
							organization?.OrganizationCD));
				}

				if (bookBalance.LastDeprPeriod == null && bookBalance.YtdDepreciated != 0)
				{
					cache.RaiseExceptionHandling<FABookBalance.lastDeprPeriod>(
						bookBalance,
						bookBalance.LastDeprPeriod,
						new PXSetPropertyException(
							Messages.NonzeroAccumDepreciationAndLastDeprPeriodIsEmptyInThePostingBook));
				}
			}
		}

		public static bool IsPostingBookBalanceParametersValidInNonMigrationMode(PXGraph graph, int? organizationID, FABookBalance bookBalance)
		{
			bool isParametersValid = true;

			if (bookBalance.UpdateGL == true
				&& bookBalance.DeprToPeriod != null
				&& !IsFinPeriodValid(graph, organizationID, bookBalance.DeprToPeriod)
				&& bookBalance.AcquisitionCost != 0)
			{
				isParametersValid = false;
			}

			return isParametersValid;
		}

		public static bool IsPostingBookBalanceParametersValidInMigrationMode(PXGraph graph, int? organizationID, FABookBalance bookBalance)
		{
			bool isParametersValid = true;

			if (bookBalance.UpdateGL == true
				&& bookBalance.DeprToPeriod != null
				&& !IsFinPeriodValid(graph, organizationID, bookBalance.DeprToPeriod)
				&& bookBalance.LastDeprPeriod == null
				&& bookBalance.AcquisitionCost != 0)
			{
				isParametersValid = false;
			}

			return isParametersValid;
		}

		public static bool IsFinPeriodValid(PXGraph graph, int? calendarOrganizationID, string finPeriodID)
		{
			FinPeriod finPeriod = graph.GetService<IFinPeriodRepository>().FindByID(calendarOrganizationID, finPeriodID);

			bool isFinPeriodValid = finPeriod != null && finPeriod.FAClosed == true ? false : true;

			return isFinPeriodValid;
		}
		#endregion

		#region Header Events

		protected virtual void FixedAsset_RecordType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = FARecordType.AssetType;
			e.Cancel = true;
		}

		protected virtual void FAClass_RecordType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = FARecordType.ClassType;
			e.Cancel = true;
		}

		protected virtual void FADetails_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FADetails det = (FADetails)e.Row;
			if (det == null) return;

			FATran tran = PXSelect<FATran, Where<FATran.assetID, Equal<Required<FADetails.assetID>>>, 
				OrderBy<
					Desc<Switch<Case<Where<FATran.origin, Equal<FARegister.origin.depreciation>>, intMax,
					Case<Where<FATran.origin, Equal<FARegister.origin.purchasing>>, int2,
					Case<Where<FATran.origin, Equal<FARegister.origin.reconcilliation>>, int1>>>, int0>,
					Desc<Switch<Case<Where<FATran.tranType, Equal<FATran.tranType.depreciationPlus>>, intMax,
						Case<Where<FATran.tranType, Equal<FATran.tranType.depreciationMinus>>, intMax,
						Case<Where<FATran.tranType, Equal<FATran.tranType.calculatedPlus>>, intMax,
						Case<Where<FATran.tranType, Equal<FATran.tranType.calculatedMinus>>, intMax>>>>, int0>,
					Desc<FATran.released, 
					Asc<FATran.refNbr, 
					Asc<FATran.lineNbr>>>>>>>.SelectSingleBound(this, null, det.AssetID);

			bool depreciated = IsDepreciated(tran);
			bool purchased = depreciated || IsPurchased(tran);
			bool isUnderConstruction = Asset.Current.UnderConstruction == true;

			PXUIFieldAttribute.SetEnabled<FADetails.receiptDate>(sender, det, !purchased);
			PXUIFieldAttribute.SetEnabled<FADetails.depreciateFromDate>(sender, det, !depreciated);
			PXUIFieldAttribute.SetRequired<FADetails.depreciateFromDate>(sender, !isUnderConstruction);
			PXUIFieldAttribute.SetEnabled<FADetails.tagNbr>(sender, det, fasetup.Current.TagNumberingID == null && fasetup.Current.CopyTagFromAssetID != true);

			string errors = PXUIFieldAttribute.GetError<FADetails.depreciateFromDate>(sender, det);
			if (string.IsNullOrEmpty(errors))
			{
				PXSetPropertyException error = (isUnderConstruction && det.DepreciateFromDate != null) ? 
					new PXSetPropertyException<FADetails.depreciateFromDate>(Messages.AssetIsUnderConstructionAndWillNotBeDepreciated, PXErrorLevel.Warning) : null;
				sender.RaiseExceptionHandling<FADetails.depreciateFromDate>(e.Row, det.DepreciateFromDate, error);
			}
		}

		protected virtual void FADetails_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			FADetails assetdet = (FADetails)e.Row;

			if (!sender.ObjectsEqual<FADetails.receiptDate>(e.Row, e.OldRow))
			{
				PXResultset<FALocationHistory> locations = LocationHistory.SelectWindowed(0, 2);

				if (locations.Count == 1)
				{
					AssetLocation.Current = locations[0];
					AssetLocation.Current.TransactionDate = assetdet.ReceiptDate;
					AssetLocation.Current.PeriodID = FinPeriodRepository.FindFinPeriodByDate(assetdet.DepreciateFromDate, PXAccess.GetParentOrganizationID(CurrentAsset.Current.BranchID))?.FinPeriodID;
					AssetLocation.Update(AssetLocation.Current);
				}
			}
		}

		protected virtual void FADetails_Hold_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			FixedAsset asset = this.Asset.Current;
			if (asset == null) return;
			PXBoolAttribute.ConvertValue(e);
			asset.HoldEntry = (bool?)e.NewValue == true;
			FixedAsset.Events.FireOnPropertyChanged<FixedAsset.classID>(this, asset);
		}

		protected virtual bool IsSplittedFixedAsset(FixedAsset fixedAsset) =>
			fixedAsset.SplittedFrom != null
			|| SelectFrom<FixedAsset>
			.Where<FixedAsset.splittedFrom.IsEqual<@P.AsInt>>
					.View
			.Select(this, fixedAsset.AssetID)
					.Any();

		protected virtual bool IsFixedAssetAccountSubUsed(FixedAsset fixedAsset) =>
			SelectFrom<FATran>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.released.IsEqual<True>>
					.And<FATran.batchNbr.IsNotNull>
					.And<FATran.tranType.IsEqual<FATran.tranType.purchasingPlus>
						.Or<FATran.tranType.IsEqual<FATran.tranType.purchasingMinus>>
						.Or<FATran.tranType.IsEqual<FATran.tranType.transferPurchasing>>>>
				.View
			.Select(this, fixedAsset.AssetID)
				.Any();

		protected virtual bool IsAccumulatedDepreciationAccountSubUsed(FixedAsset fixedAsset) => 
			SelectFrom<FATran>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.released.IsEqual<True>>
					.And<FATran.batchNbr.IsNotNull>
					.And<FATran.tranType.IsEqual<FATran.tranType.depreciationPlus>
						.Or<FATran.tranType.IsEqual<FATran.tranType.depreciationMinus>>
						.Or<FATran.tranType.IsEqual<FATran.tranType.adjustingDeprPlus>>
						.Or<FATran.tranType.IsEqual<FATran.tranType.adjustingDeprMinus>>
						.Or<FATran.tranType.IsEqual<FATran.tranType.transferDepreciation>>>>
				.View
			.Select(this, fixedAsset.AssetID)
				.Any();

		protected virtual bool HasSplittedDepreciation(FixedAsset fixedAsset) =>
			SelectFrom<FATran>
				.InnerJoin<FABook>
					.On<FATran.bookID.IsEqual<FABook.bookID>>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.tranType.IsEqual<FATran.tranType.adjustingDeprPlus>
						.Or<FATran.tranType.IsEqual<FATran.tranType.adjustingDeprMinus>>>
					.And<FABook.updateGL.IsEqual<True>>>
				.View
			.Select(this, fixedAsset.AssetID)
				.Any();

		protected Dictionary<Type, Type> AccountSubPairs = new Dictionary<Type, Type>
		{
			[typeof(FixedAsset.fAAccountID)] = typeof(FixedAsset.fASubID),
			[typeof(FixedAsset.accumulatedDepreciationAccountID)] = typeof(FixedAsset.accumulatedDepreciationSubID),
			[typeof(FixedAsset.depreciatedExpenseAccountID)] = typeof(FixedAsset.depreciatedExpenseSubID),
			[typeof(FixedAsset.fAAccrualAcctID)] = typeof(FixedAsset.fAAccrualSubID),
			[typeof(FixedAsset.disposalAccountID)] = typeof(FixedAsset.disposalSubID),
			[typeof(FixedAsset.gainAcctID)] = typeof(FixedAsset.gainSubID),
			[typeof(FixedAsset.lossAcctID)] = typeof(FixedAsset.lossSubID),
			[typeof(FixedAsset.constructionAccountID)] = typeof(FixedAsset.constructionSubID),
		};

		protected virtual Dictionary<Type, Type> GetFAAccountSubPairs() => AccountSubPairs;

		protected virtual bool IsAccountChangeable<AccountSubField>(FixedAsset fixedAsset)
			where AccountSubField : IBqlField
		{
			return IsAccountChangeable(fixedAsset, typeof(AccountSubField));
		}

		protected virtual bool IsAccountChangeable(FixedAsset fixedAsset, Type accountSubField)
		{
			if(new Type[] 
				{
					typeof(FixedAsset.fAAccountID),
					typeof(FixedAsset.fASubID),
					typeof(FixedAsset.fAAccrualAcctID),
					typeof(FixedAsset.fAAccrualSubID),
				}
				.Contains(accountSubField))
			{
				return !(IsFixedAssetAccountSubUsed(fixedAsset) || IsSplittedFixedAsset(fixedAsset));
			}

			if (new Type[]
				{
					typeof(FixedAsset.accumulatedDepreciationAccountID),
					typeof(FixedAsset.accumulatedDepreciationSubID),
				}
				.Contains(accountSubField))
			{
				return !(IsAccumulatedDepreciationAccountSubUsed(fixedAsset) || (IsSplittedFixedAsset(fixedAsset) && HasSplittedDepreciation(fixedAsset)));
			}

			Dictionary<Type, Type> accountSubPairs = GetFAAccountSubPairs();
			if (accountSubPairs.Keys.Contains(accountSubField)
				|| accountSubPairs.Values.Contains(accountSubField))
			{
				return true;
			}
			else
			{
				throw new ArgumentOutOfRangeException(accountSubField.FullName);
			}

		}

		protected virtual void FixedAsset_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FixedAsset asset = (FixedAsset)e.Row;
			if (asset == null) return;
			
			Suspend.SetCaption(asset.Suspended == true ? Messages.Unsuspend : Messages.Suspend);
			Suspend.SetEnabled(asset.Depreciable == true);

			bool enabled = (asset.Status != FixedAssetStatus.Disposed) && (asset.Status != FixedAssetStatus.Reversed);
			sender.AllowUpdate = enabled;
			sender.AllowDelete = enabled;

			AssetBalance.Cache.AllowInsert = enabled;
			AssetBalance.Cache.AllowUpdate = enabled;
			AssetBalance.Cache.AllowDelete = enabled;
			AssetUsage.Cache.AllowInsert = asset.UsageScheduleID != null && enabled;
			AssetUsage.Cache.AllowUpdate = enabled;
			AssetUsage.Cache.AllowDelete = enabled;
			FATransactions.Cache.AllowDelete = enabled;

			bool isInserted = sender.GetStatus(asset) == PXEntryStatus.Inserted;
			AssetHistory.Cache.AllowInsert = !isInserted;
			BookSheetHistory.Cache.AllowInsert = !isInserted;

			//TODO: Move to the AssetMaint_Workflow
			CalculateDepreciation.SetEnabled(asset.Status == FixedAssetStatus.Active && asset.Depreciable == true && asset.UnderConstruction == false && transferTransactions.SelectSingle()?.Released != false);

			bool fixedAssetAccountEnabled = IsAccountChangeable<FixedAsset.fAAccountID>(asset);
			bool accumulatedDepreciationAccountEnabled = IsAccountChangeable<FixedAsset.accumulatedDepreciationAccountID>(asset);

			PXUIFieldAttribute.SetEnabled<FixedAsset.fAAccountID>(sender, asset, fixedAssetAccountEnabled);
			PXUIFieldAttribute.SetEnabled<FixedAsset.fASubID>(sender, asset, fixedAssetAccountEnabled);
			PXUIFieldAttribute.SetEnabled<FixedAsset.fAAccrualAcctID>(sender, asset, fixedAssetAccountEnabled);
			PXUIFieldAttribute.SetEnabled<FixedAsset.fAAccrualSubID>(sender, asset, fixedAssetAccountEnabled);
			PXUIFieldAttribute.SetEnabled<FixedAsset.accumulatedDepreciationAccountID>(sender, asset, accumulatedDepreciationAccountEnabled);
			PXUIFieldAttribute.SetEnabled<FixedAsset.accumulatedDepreciationSubID>(sender, asset, accumulatedDepreciationAccountEnabled);

			FADepreciationMethod ulMeth = PXSelectJoin<FADepreciationMethod, InnerJoin<FABookBalance, On<FADepreciationMethod.methodID, Equal<FABookBalance.depreciationMethodID>>>, Where<FADepreciationMethod.usefulLife, IsNotNull, And<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>>>.SelectSingleBound(this, new object[] { asset });
			bool inactive = ((FABookBalance)PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>, And<FABookBalance.status, NotEqual<FixedAssetStatus.active>>>>.SelectSingleBound(this, new object[] { asset })) != null;
			PXUIFieldAttribute.SetEnabled<FixedAsset.usefulLife>(sender, asset, ulMeth == null && !inactive);

			bool depreciate = asset.Depreciable == true;

			PXUIFieldAttribute.SetVisible<DisposeParams.actionBeforeDisposal>(DispParams.Cache, null, depreciate);

			PXUIFieldAttribute.SetDisplayName<FABookBalance.deprFromDate>(AssetBalance.Cache, depreciate ? Messages.DeprFromDate : Messages.PlacedInServiceDate);
			PXUIFieldAttribute.SetDisplayName<FABookBalance.deprFromPeriod>(AssetBalance.Cache, depreciate ? Messages.DeprFromPeriod : Messages.PlacedInServicePeriod);
			PXUIFieldAttribute.SetVisible<FABookBalance.deprToDate>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.deprToPeriod>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.lastDeprPeriod>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.salvageAmount>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.ytdDepreciated>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.averagingConvention>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.tax179Amount>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.ytdTax179Recap>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.bonusID>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.bonusRate>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.bonusAmount>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.ytdBonusRecap>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.depreciationMethodID>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.midMonthType>(AssetBalance.Cache, null, depreciate);
			PXUIFieldAttribute.SetVisible<FABookBalance.midMonthDay>(AssetBalance.Cache, null, depreciate);

			deprbookfilter.AllowSelect = asset.UnderConstruction != true;
			AssetHistory.AllowSelect = asset.Depreciable == true && asset.UnderConstruction != true && fasetup.Current.DeprHistoryView == FASetup.deprHistoryView.SideBySide;
			BookSheetHistory.AllowSelect = deprbookfilter.AllowSelect = asset.Depreciable == true && asset.UnderConstruction != true && fasetup.Current.DeprHistoryView == FASetup.deprHistoryView.BookSheet;

			var isReversedOrDisposed = asset.Status != FixedAssetStatus.Reversed && asset.Status != FixedAssetStatus.Disposed;
			CurrentAsset.Cache.AllowUpdate = isReversedOrDisposed;
			AssetDetails.Cache.AllowUpdate = isReversedOrDisposed;
			AssetLocation.Cache.AllowUpdate = isReversedOrDisposed;
			GLTrnFilter.Cache.AllowSelect = isReversedOrDisposed;
			DsplAdditions.Cache.AllowSelect = isReversedOrDisposed;
		}

		protected virtual void FixedAsset_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			AssetDetails.Cache.Insert();
			AssetLocation.Cache.Insert();
			AssetLocation.Cache.IsDirty = false;
			AssetDetails.Cache.IsDirty = false;
		}

		protected virtual DateTime? GetTransferDate(int? assetID, string transferPeriodID)
		{
			FADetails details = SelectFrom<FADetails>
				.Where<FADetails.assetID.IsEqual<@P.AsInt>>
				.View
				.Select(this, assetID);

			FABookBalance balance = SelectFrom<FABookBalance>
				.Where<FABookBalance.assetID.IsEqual<@P.AsInt>>
				.OrderBy<Desc<FABookBalance.updateGL>>
				.View
				.SelectSingleBound(this, null, assetID);

			DateTime? transferDate = Accessinfo.BusinessDate;
			FABookPeriod transferPeriod = FABookPeriodRepository.FindOrganizationFABookPeriodByID(transferPeriodID, balance.BookID, assetID);

			if(transferPeriod.StartDate > transferDate || transferPeriod.EndDate <= transferDate)
			{
				transferDate = transferPeriod.StartDate;
			}

			if(transferDate < details.ReceiptDate)
			{
				transferDate = details.ReceiptDate;
			}

			return transferDate;
		}

		protected virtual void FixedAsset_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var newAsset = e.NewRow as FixedAsset;
			var oldAsset = e.Row as FixedAsset;
			if (newAsset == null) return;

			if (this.WillAccountsBeChanged(newAsset, oldAsset, out int? faAccID, out int? faSubID, out int? accDeprAccID, out int? accDeprSubID)
				&& FAInnerStateDescriptor.IsAcquired(newAsset.AssetID, this)
				&& !FAInnerStateDescriptor.WillBeTransferred(newAsset.AssetID, this)
				)
			{
				if (AssetDetails.Current == null)
				{
					AssetDetails.Current = AssetDetails.Select();
				}
				if (AssetLocation.Current == null)
				{
					AssetLocation.Current = AssetLocation.Select();
				};
				FALocationHistory newLocation = PXCache<FALocationHistory>.CreateCopy(AssetLocation.Current);
				try
				{
					if (fasetup.Current.UpdateGL != true && !HasAnyGLTran(newAsset.AssetID))
					{
						AssetLocation.Cache.Update(newLocation);
						return;
					}

					newLocation.RefNbr = null;

					if (IsImport)
					{
						int? organizationID = PXAccess.GetParentOrganizationID(CurrentAsset.Current.BranchID);
						string periodID = FinPeriodRepository.FindFinPeriodByDate(Accessinfo.BusinessDate, organizationID)?.FinPeriodID;
						newLocation.PeriodID = periodID;
					}
					else
					{
					newLocation.PeriodID = GetTransferPeriod(newAsset.AssetID);
					}

					newLocation.ClassID = CurrentAsset.Current.ClassID;
					newLocation.RevisionID = AssetDetails.Current.LocationRevID;
					newLocation.TransactionDate = GetTransferDate(newAsset.AssetID, newLocation.PeriodID);
					newLocation = (FALocationHistory)AssetLocation.Cache.Insert(newLocation);
				}
				finally { }
			}
		}

		protected virtual void _(Events.RowUpdated<FixedAsset> e)
		{
			if (!e.Cache.ObjectsEqual<FixedAsset.status>(e.Row, e.OldRow))
			{
				FixedAsset row = (FixedAsset)e.Row;
				FADetails details = PXSelect<FADetails, Where<FADetails.assetID,
					Equal<Required<FADetails.assetID>>>>.Select(this, row?.AssetID);

				if (Asset.Cache.GetStatus(row) != PXEntryStatus.Inserted
					&& row.Status != FixedAssetStatus.FullyDepreciated
				&& AssetProcess.IsFullyDepreciatedAsset(this, row.AssetID))
				{
					FixedAsset.Events
						.Select(ev => ev.FullyDepreciateAsset)
						.FireOn(this, row);

					return;
				}

				AssetDetails.Cache.SetValue<FADetails.status>(details, row.Status);
				AssetDetails.Cache.MarkUpdated(details);
			}

			if (e.Row?.ClassID != null && e.OldRow?.ClassID != e.Row?.ClassID)
			{
				FixedAsset cls = SelectFrom<FixedAsset>
					.Where<FixedAsset.assetID.IsEqual<FixedAsset.classID.FromCurrent>>
					.View.SelectSingleBound(this, new object[] { e.Row });

				var copy = (FixedAsset)e.Cache.CreateCopy(e.Row);
				copy.UnderConstruction = cls.UnderConstruction;
				copy = (FixedAsset)e.Cache.Update(copy);
			}
		}

		protected virtual void FixedAsset_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FixedAsset header = (FixedAsset)e.Row;
			if (header.ClassID == null)
			{
				sender.RaiseExceptionHandling<FixedAsset.classID>(header, header.ClassID, new PXSetPropertyException(Messages.ValueCanNotBeEmpty));
			}
			_PersistedAsset = header;
		}

		protected virtual void FixedAsset_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			FixedAsset asset = (FixedAsset)e.Row;
			if (asset == null) return;

			if (null != (FATran)SelectFrom<FATran>.
				Where<FATran.assetID.IsEqual<FixedAsset.assetID.FromCurrent>
				.And<FATran.released.IsEqual<True>>
				.And<FATran.batchNbr.IsNotNull.
					Or<FixedAsset.splittedFrom.FromCurrent.IsNotNull>>>
				.View
				.SelectSingleBound(this, new object[] { asset }))
			{
				throw new PXSetPropertyException(Messages.BalanceRecordCannotBeDeleted);
			}

			this.EnsureCachePersistence(typeof(FARegister));
			this.EnsureCachePersistence(typeof(FABookHistory));

			foreach (FARegister reg in PXSelectJoinGroupBy<FARegister,
				LeftJoin<FATran, On<FARegister.refNbr, Equal<FATran.refNbr>>>,
				Where<FATran.assetID, Equal<Required<FixedAsset.assetID>>>,
				Aggregate<GroupBy<FARegister.refNbr>>>.Select(this, asset.AssetID))
			{
				this.Caches<FARegister>().Delete(reg);
			}

			foreach (FABookHistory hist in PXSelect<FABookHistory, Where<FABookHistory.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(this, asset.AssetID))
			{
				this.Caches<FABookHistory>().Delete(hist);
			}
		}

		protected virtual void CheckNewUsefulLife(PXCache sender, FABookBalance bal, decimal usefulLife)
		{
			CheckLastDeprPeriodBetweenDeprFromAndDeprTo<FABookBalance.usefulLife>(sender, bal, usefulLife);
		}

		/// <summary>
		/// The method to validate that <see cref="FABookBalance.LastDeprPeriod"/> greater than 
		/// <see cref="FABookBalance.DeprFromPeriod"/> and less than <see cref="FABookBalance.DeprToPeriod"/> for the book.
		/// Throws an exception with an appropriate error message in case of negative validation.
		/// </summary>
		protected virtual void CheckLastDeprPeriodBetweenDeprFromAndDeprTo<T>(PXCache sender, FABookBalance bal, object newValue)
			where T : IBqlField
		{
			FABookBalance copy = (FABookBalance)sender.CreateCopy(bal);
			sender.SetValue<T>(copy, newValue);

			using (new SuppressHistoryUpdateScope())
			{
				sender.SetDefaultExt<FABookBalance.recoveryPeriod>(copy);
				sender.SetDefaultExt<FABookBalance.deprFromPeriod>(copy);
				sender.SetDefaultExt<FABookBalance.deprToPeriod>(copy);
			}

			if (copy.LastDeprPeriod != null && copy.DeprFromPeriod != null && copy.DeprToPeriod != null)
			{
				if (string.CompareOrdinal(copy.LastDeprPeriod, copy.DeprFromPeriod) < 0)
				{
					throw new PXSetPropertyException(
						Messages.DeprFromPeriodGreaterLastDeprPeriod,
						FinPeriodIDFormattingAttribute.FormatForError(copy.DeprFromPeriod),
						FinPeriodIDFormattingAttribute.FormatForError(copy.LastDeprPeriod));
				}

				if (string.CompareOrdinal(copy.LastDeprPeriod, copy.DeprToPeriod) > 0)
				{
					throw new PXSetPropertyException(
						Messages.DeprToPeriodLessLastDeprPeriod,
						FinPeriodIDFormattingAttribute.FormatForError(copy.DeprToPeriod),
						FinPeriodIDFormattingAttribute.FormatForError(copy.LastDeprPeriod));
				}
			}
		}

		[Obsolete("Obsoilete. Will be removed in Acumatica ERP 2020R2")]
		protected virtual void CheckDeprToPeriodGreaterLastPeriod<T>(PXCache sender, FABookBalance bal, object newValue)
			where T: IBqlField
		{
			FABookBalance copy = (FABookBalance)sender.CreateCopy(bal);
			sender.SetValue<T>(copy, newValue);
			sender.SetDefaultExt<FABookBalance.recoveryPeriod>(copy);
			sender.SetDefaultExt<FABookBalance.deprToPeriod>(copy);

			if (copy.LastDeprPeriod != null && copy.DeprToPeriod != null && string.CompareOrdinal(copy.LastDeprPeriod, copy.DeprToPeriod) >= 0)
			{
				throw new PXSetPropertyException(
					Messages.CannotChangeUsefulLife, 
					FinPeriodIDFormattingAttribute.FormatForError(copy.DeprToPeriod), 
					FinPeriodIDFormattingAttribute.FormatForError(copy.LastDeprPeriod));
			}
		}		

		protected virtual void FixedAsset_UsefulLife_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FixedAsset asset = (FixedAsset)e.Row;
			if (asset.Depreciable == true && e.NewValue == null)
			{
				throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(FixedAsset.usefulLife)}]");
			}
			if (asset.Depreciable == true && (decimal)e.NewValue == 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, "0");
			}
			foreach (FABookBalance bal in PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>,
						 OrderBy<Asc<FABookBalance.assetID, Asc<FABookBalance.bookID>>>>.SelectMultiBound(this, new object[] { asset }))
			{
				CheckNewUsefulLife(AssetBalance.Cache, bal, (decimal)e.NewValue);
			}
		}

		protected virtual void FABookBalance_UsefulLife_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FABookBalance bal = e.Row as FABookBalance;
			if (bal == null) return;
			if (bal.Depreciate == true)
			{
				if (e.NewValue == null)
				{
					throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(FABookBalance.usefulLife)}]");
				}
				if ((decimal)e.NewValue == 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GT, "0");
				}
			}
			if (e.NewValue != null)
			{
			CheckNewUsefulLife(sender, (FABookBalance)e.Row, (decimal)e.NewValue);
		}
		}

		protected virtual void _(Events.FieldVerifying<FABookBalance.bonusAmount> e)
		{
			FABookBalance balance = e.Row as FABookBalance;
			if (balance == null || e.NewValue == null) return;

			DateTime? date = balance.DeprFromDate;
			int? bonusID = balance.BonusID;

			if (bonusID == null || date == null) return;

			FABonusDetails bonusDetails = PXSelect<FABonusDetails, Where<FABonusDetails.bonusID, Equal<Required<FABonus.bonusID>>,
				And<FABonusDetails.startDate, LessEqual<Required<FABookBalance.deprFromDate>>,
				And<FABonusDetails.endDate, GreaterEqual<Required<FABookBalance.deprFromDate>>>>>>.Select(this, bonusID, date, date);

			if (bonusDetails != null)
			{
				var bonusMax = bonusDetails.BonusMax;

				if (bonusMax > 0 && (decimal)e.NewValue > bonusMax)
				{
					e.NewValue = bonusMax;
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<FABookBalance.averagingConvention> e)
		{
			FABookBalance bal = e.Row as FABookBalance;
			if (bal == null || e.NewValue == null) return;

			CheckDeprToPeriodGreaterLastPeriod<FABookBalance.averagingConvention>(e.Cache, bal, e.NewValue);
		}

		protected virtual void _(Events.FieldVerifying<FixedAsset.classID> e)
		{
			FixedAsset asset = e.Row as FixedAsset;
			FAClass newFAClass = SelectFrom<FAClass>
				.Where<FAClass.assetID.IsEqual<@P.AsInt>>.View.Select(this, e.NewValue);

			foreach (PXResult<FABookSettings, FABook> result in SelectFrom<FABookSettings>
									.LeftJoin<FABook>.On<FABookSettings.bookID.IsEqual<FABook.bookID>>
									.Where<FABookSettings.assetID.IsEqual<@P.AsInt>.And<FABook.updateGL.IsNotEqual<True>>>.View.Select(this, e.NewValue))
			{
				FABook book = result;
				IYearSetup yearSetup = FABookPeriodRepository.FindFABookYearSetup(book);

				if (yearSetup == null || (!yearSetup.IsFixedLengthPeriod && book != null &&
					(SelectFrom<FABookPeriodSetup>.Where<FABookPeriodSetup.bookID.IsEqual<@P.AsInt>>.View.Select(this, book.BookID).FirstOrDefault()) == null))
				{
					e.NewValue = newFAClass.AssetCD;
					throw new PXSetPropertyException(Messages.BookCalendarNotExist, book.BookCode);
				}
			}

			if (asset.UnderConstruction == true && newFAClass.UnderConstruction != true && AssetDetails.Current.DepreciateFromDate == null)
			{
				AssetDetails.Cache.RaiseExceptionHandling<FADetails.depreciateFromDate>(AssetDetails.Current, AssetDetails.Current.DepreciateFromDate, new PXSetPropertyException(PX.Data.ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<FADetails.depreciateFromDate>(AssetDetails.Cache)));
				e.NewValue = newFAClass.AssetCD;
				throw new PXSetPropertyException(Messages.AssetUnderConstructionWithEmptyPlaceInServiceDateCannotBeTransferred);
			}
			
			if (newFAClass?.UnderConstruction == true)
			{
				bool accumDeprExists = false;
				foreach (FABookBalance balance in AssetBalance.Select())
				{
					if (balance.YtdDepreciated == 0m) continue;
					accumDeprExists = true;
					break;
				}

				if (asset.UnderConstruction == true && accumDeprExists)
				{
					throw new PXSetPropertyException(Messages.AssetUnderConstructionWithEmptyPlaceInServiceDateCannotBeTransferred);
				} 
			}
		}

		public virtual void _(Events.FieldUpdated<FABookBalance.usefulLife> e)
		{
			FABookBalance balance = (FABookBalance)e.Row;
			if (balance == null) return;

			FADepreciationMethod method = PXSelect<FADepreciationMethod, Where<FADepreciationMethod.methodID, Equal<Current<FABookBalance.depreciationMethodID>>>>
			  .SelectSingleBound(this, new object[] { balance });
			if (method?.UsefulLife != null && balance.UsefulLife != method.UsefulLife)
			{
				balance.DepreciationMethodID = null;
			}

			if (balance.DeprToPeriod != null &&
				balance.LastDeprPeriod != null &&
				string.CompareOrdinal(balance.DeprToPeriod, balance.LastDeprPeriod)==0)
			{
				balance.CurrDeprPeriod = null;
				balance.Status = FixedAssetStatus.FullyDepreciated;
			}
		}

		protected virtual void FixedAsset_UsefulLife_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue == null) return;

			FixedAsset asset = e.Row as FixedAsset;
			
			if (AssetDetails.Current != null && AssetDetails.Current.ReceiptDate != null)
			{
				bool fullyDepreciated = true;
				foreach (FABookBalance bal in AssetBalance.Select())
				{
					FABookBalance balCopy = (FABookBalance)AssetBalance.Cache.CreateCopy(bal);
					balCopy.UsefulLife = (decimal)e.NewValue;
					object deprToPeriod = null;
					AssetBalance.Cache.RaiseFieldDefaulting<FABookBalance.deprToPeriod>(balCopy, out deprToPeriod);
					if (string.CompareOrdinal(deprToPeriod?.ToString(), balCopy.LastDeprPeriod) > 0)
					{
						fullyDepreciated = false;
						break;
					}
		}

				if (fullyDepreciated && Asset.Ask(Messages.UsefulLifeChangeCausedFullDepreciatedStatus, MessageButtons.YesNo) == WebDialogResult.No)
				{
					e.Cancel = true;
					e.NewValue = asset.UsefulLife;
				}
			}
		}
		protected virtual void FixedAsset_UsefulLife_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UpdateBalances<FABookBalance.usefulLife, FixedAsset.usefulLife>(sender, e);

			FixedAsset asset = e.Row as FixedAsset;
			if (Asset.Cache.GetStatus(asset) != PXEntryStatus.Inserted
				&& asset.Status != FixedAssetStatus.FullyDepreciated
				&& AssetProcess.IsFullyDepreciatedAsset(this, asset.AssetID))
			{
				asset.Status = FixedAssetStatus.FullyDepreciated;
				FADetails details = AssetDetails.Current ?? PXSelect<FADetails, Where<FADetails.assetID,
					Equal<Required<FADetails.assetID>>>>.Select(this, asset?.AssetID);
				if (details != null)
				{
					details.Status = FixedAssetStatus.FullyDepreciated;
				}
			}
		}

		protected virtual void FixedAsset_Depreciable_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UpdateBalances<FABookBalance.depreciate, FixedAsset.depreciable>(sender, e);
		}

		protected virtual void FixedAsset_ParentAssetID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FixedAsset asset = (FixedAsset)e.Row;
			if (asset == null) return;

			PXSelectBase<FixedAsset> cmd = new PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.parentAssetID>>>>(this);
			int? parentID = (int?)e.NewValue;
			string parentCD = null;
			while (parentID != null)
			{
				FixedAsset parent = cmd.Select(parentID);
				parentCD = parentCD ?? parent.AssetCD;
				if (parent.ParentAssetID != null && parent.ParentAssetID == asset.AssetID)
				{
					e.NewValue = asset.ParentAssetID != null ? ((FixedAsset)cmd.Select(asset.ParentAssetID)).AssetCD : null;
					throw new PXSetPropertyException(Messages.CyclicParentRef, parentCD);
				}
				parentID = parent.ParentAssetID;
			}
		}

		protected virtual void FixedAsset_FASubID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FixedAsset asset = e.Row as FixedAsset;
			if (asset == null)
				return;

			int? faSubID = (int?)e.NewValue;
			if (faSubID == null)
				return;

			string errorSubCD = VerifySubIDByFAClass<FAClass.fASubMask, FixedAsset.fASubID>(sender, asset, faSubID);

			if (!string.IsNullOrEmpty(errorSubCD))
			{
				e.NewValue = errorSubCD;
				throw new PXSetPropertyException(Messages.SubAccountNotCorrespondToMask);
			}
		}

		protected virtual void FixedAsset_AccumulatedDepreciationSubID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FixedAsset asset = e.Row as FixedAsset;
			if (asset == null)
				return;

			int? accumulatedDepreciationSubID = (int?)e.NewValue;
			if (accumulatedDepreciationSubID == null)
				return;

			string errorSubCD = VerifySubIDByFAClass<FAClass.accumDeprSubMask, FixedAsset.accumulatedDepreciationSubID>(sender, asset, accumulatedDepreciationSubID);

			if (!string.IsNullOrEmpty(errorSubCD))
			{
				e.NewValue = errorSubCD;
				throw new PXSetPropertyException(Messages.SubAccountNotCorrespondToMask);
			}
		}

		protected void UpdateFATranAccountSub<UpdatingField>(int? assetID, int? newValue, params string[] transactionTypes)
			where UpdatingField : IBqlField
		{
			PXCache cache = Caches[BqlCommand.GetItemType<UpdatingField>()];
			foreach (FATran transaction in SelectFrom<FATran>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.batchNbr.IsNull>
					.And<FATran.tranType.IsIn<@P.AsString>>>
				.View
				.Select(this, assetID, transactionTypes))
			{
				cache.SetValue<UpdatingField>(transaction, newValue);
				cache.Update(transaction);
			}
		}

		protected void _(Events.FieldUpdated<FixedAsset, FixedAsset.fAAccountID> e)
		{
			if (e.Row == null || (fasetup.Current.UpdateGL == true && e.Row.IsAcquired == true)) return;

			UpdateFATranAccountSub<FATran.debitAccountID>(e.Row.AssetID, (int?)e.NewValue, FATran.tranType.PurchasingPlus);
			UpdateFATranAccountSub<FATran.creditAccountID>(e.Row.AssetID, (int?)e.NewValue, FATran.tranType.PurchasingMinus);
		}

		protected void _(Events.FieldUpdated<FixedAsset, FixedAsset.fASubID> e)
		{
			if (e.Row == null || (fasetup.Current.UpdateGL == true && e.Row.IsAcquired == true)) return;

			UpdateFATranAccountSub<FATran.debitSubID>(e.Row.AssetID, (int?)e.NewValue, FATran.tranType.PurchasingPlus);
			UpdateFATranAccountSub<FATran.creditSubID>(e.Row.AssetID, (int?)e.NewValue, FATran.tranType.PurchasingMinus);
		}

		protected void _(Events.FieldUpdated<FixedAsset, FixedAsset.fAAccrualAcctID> e)
		{
			if (e.Row == null || (fasetup.Current.UpdateGL == true && e.Row.IsAcquired == true)) return;

			UpdateFATranAccountSub<FATran.debitAccountID>(e.Row.AssetID, (int?)e.NewValue, FATran.tranType.PurchasingMinus, FATran.tranType.ReconciliationPlus);
			UpdateFATranAccountSub<FATran.creditAccountID>(e.Row.AssetID, (int?)e.NewValue, FATran.tranType.PurchasingPlus, FATran.tranType.ReconciliationMinus);
		}

		protected void _(Events.FieldUpdated<FixedAsset, FixedAsset.fAAccrualSubID> e)
		{
			if (e.Row == null || (fasetup.Current.UpdateGL == true && e.Row.IsAcquired == true)) return;

			UpdateFATranAccountSub<FATran.debitSubID>(e.Row.AssetID, (int?)e.NewValue, FATran.tranType.PurchasingMinus, FATran.tranType.ReconciliationPlus);
			UpdateFATranAccountSub<FATran.creditSubID>(e.Row.AssetID, (int?)e.NewValue, FATran.tranType.PurchasingPlus, FATran.tranType.ReconciliationMinus);
		}

		protected void _(Events.FieldUpdated<FixedAsset, FixedAsset.accumulatedDepreciationAccountID> e)
		{
			if (e.Row == null || (fasetup.Current.UpdateGL == true && e.Row.IsAcquired == true)) return;

			UpdateFATranAccountSub<FATran.debitAccountID>(e.Row.AssetID, (int?)e.NewValue, FATran.tranType.DepreciationMinus, FATran.tranType.CalculatedMinus, FATran.tranType.AdjustingDeprMinus);
			UpdateFATranAccountSub<FATran.creditAccountID>(e.Row.AssetID, (int?)e.NewValue, FATran.tranType.DepreciationPlus, FATran.tranType.CalculatedPlus, FATran.tranType.AdjustingDeprPlus);

			PXCache cache = this.Caches<FATran>();
			foreach (FATran transaction in SelectFrom<FATran>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.batchNbr.IsNull>
					.And<FATran.released.IsEqual<True>>
					.And<FATran.creditAccountID.IsEqual<@P.AsInt>>
					.And<FATran.tranType.IsEqual<@P.AsString>>>
				.View
				.Select(this, e.Row.AssetID, (int?)e.OldValue, FATran.tranType.TransferDepreciation))
			{
				transaction.CreditAccountID = (int?)e.NewValue;
				cache.Update(transaction);
			}
		}

		protected void _(Events.FieldUpdated<FixedAsset, FixedAsset.accumulatedDepreciationSubID> e)
		{
			if (e.Row == null || (fasetup.Current.UpdateGL == true && e.Row.IsAcquired == true)) return;

			UpdateFATranAccountSub<FATran.debitSubID>(e.Row.AssetID, (int?)e.NewValue, FATran.tranType.DepreciationMinus, FATran.tranType.CalculatedMinus, FATran.tranType.AdjustingDeprMinus);
			UpdateFATranAccountSub<FATran.creditSubID>(e.Row.AssetID, (int?)e.NewValue, FATran.tranType.DepreciationPlus, FATran.tranType.CalculatedPlus, FATran.tranType.AdjustingDeprPlus);

			PXCache cache = this.Caches<FATran>();
			foreach (FATran transaction in SelectFrom<FATran>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.batchNbr.IsNull>
					.And<FATran.released.IsEqual<True>>
					.And<FATran.creditSubID.IsEqual<@P.AsInt>>
					.And<FATran.tranType.IsEqual<@P.AsString>>>
				.View
				.Select(this, e.Row.AssetID, (int?)e.OldValue, FATran.tranType.TransferDepreciation))
			{
				transaction.CreditSubID = (int?)e.NewValue;
				cache.Update(transaction);
			}
		}

		protected virtual string VerifySubIDByFAClass<MaskField, SubIDField>(PXCache sender, FixedAsset asset, int? subID)
			where MaskField : IBqlField
			where SubIDField : IBqlField
		{
			string errorSubCD = String.Empty;

			Sub sub = PXSelect<Sub, Where<Sub.subID, Equal<Required<FixedAsset.fASubID>>>>.SelectSingleBound(sender.Graph, new object[] { }, subID);
			string subCD = sub.SubCD;

			FAClass cls = PXSelect<FAClass, Where<FAClass.assetID, Equal<Current<FixedAsset.classID>>>>.SelectSingleBound(sender.Graph, new object[] { asset });
			if (cls == null)
				return String.Empty;

			if(cls.UnderConstruction == true && typeof(SubIDField) == typeof(FixedAsset.accumulatedDepreciationSubID))
			{
				return String.Empty;
			}

			int? defaultSubID = MakeSubID<MaskField, SubIDField>(sender.Graph.Caches[typeof(FixedAsset)], asset);

			Sub defaultSub = PXSelect<Sub, Where<Sub.subID, Equal<Required<FAClass.fASubID>>>>.SelectSingleBound(sender.Graph, new object[] { }, defaultSubID);
			string defaultSubCD = defaultSub.SubCD;

			//Compare subCD and defaultSubCD in the segments that have classSubMask other than AA
			string classSubMask = (string)sender.Graph.Caches[typeof(FAClass)].GetValue<MaskField>(cls);

			for (int i = 0; i < subCD.Count(); i++)
			{
				if (classSubMask[i] == 'A')
					continue;

				if (subCD[i] != defaultSubCD[i])
				{
					errorSubCD = subCD;
					break;
				}
			}

			return errorSubCD;
		}

		public static int? MakeSubID<MaskField, SubIDField>(PXCache sender, FixedAsset asset)
			where MaskField : IBqlField
			where SubIDField : IBqlField
		{
			FAClass cls = PXSelect<FAClass, Where<FAClass.assetID, Equal<Current<FixedAsset.classID>>>>.SelectSingleBound(sender.Graph, new object[] { asset });
			if (cls == null) return null;

			FADetails det = PXSelect<FADetails, Where<FADetails.assetID, Equal<Current<FixedAsset.assetID>>>>.SelectSingleBound(sender.Graph, new object[] { asset });
			FALocationHistory hist = PXSelect<FALocationHistory, Where<FALocationHistory.assetID, Equal<Current<FADetails.assetID>>,
											And<FALocationHistory.revisionID, Equal<Current<FADetails.locationRevID>>>>>.SelectSingleBound(sender.Graph, new object[] { asset, det });

			Location loc = PXSelectJoin<Location, LeftJoin<BAccount, On<Location.bAccountID, Equal<BAccount.bAccountID>, And<Location.locationID, Equal<BAccount.defLocationID>>>, LeftJoin<Branch, On<Branch.bAccountID, Equal<BAccount.bAccountID>>>>, Where<Branch.branchID, Equal<Current<FALocationHistory.locationID>>>>.SelectSingleBound(sender.Graph, new object[] { hist });
			EPDepartment dep = PXSelect<EPDepartment, Where<EPDepartment.departmentID, Equal<Current<FALocationHistory.department>>>>.SelectSingleBound(sender.Graph, new object[] { hist });

			string masksub = (string)sender.Graph.Caches[typeof(FAClass)].GetValue<MaskField>(cls);
			int? asset_subID = (int?)sender.Graph.Caches[typeof(FixedAsset)].GetValue<SubIDField>(asset);
			int? loc_subID = (int?)sender.Graph.Caches[typeof(Location)].GetValue<Location.cMPExpenseSubID>(loc);
			int? dep_subID = (int?)sender.Graph.Caches[typeof(EPDepartment)].GetValue<EPDepartment.expenseSubID>(dep);
			int? cls_subID = (int?)sender.Graph.Caches[typeof(FixedAsset)].GetValue<SubIDField>(cls);


			object value = SubAccountMaskAttribute.MakeSub<MaskField>(sender.Graph, masksub,
																	  new object[] { asset_subID, loc_subID, dep_subID, cls_subID },
																	  new[] { typeof(SubIDField), typeof(Location.cMPExpenseSubID), typeof(EPDepartment.expenseSubID), typeof(SubIDField) });

			sender.RaiseFieldUpdating<SubIDField>(asset, ref value);

			return (int?)value;
		}

		protected static void FASubIDFieldDefaulting<MaskField, SubIDField>(PXCache sender, PXFieldDefaultingEventArgs e)
			where MaskField : IBqlField
			where SubIDField : IBqlField
		{
			FixedAsset asset = (FixedAsset)e.Row;
			e.NewValue = MakeSubID<MaskField, SubIDField>(sender, asset);
			e.Cancel = true;
		}

		protected virtual void  _(Events.FieldVerifying<FixedAsset, FixedAsset.classID> e)
		{
			if (e.Row == null && string.IsNullOrWhiteSpace(e.NewValue as string) ) return;

			if (fasetup.Current.UpdateGL != true && HasAnyGLTran(e.Row.AssetID))
			{
				FAClass faClass = PXSelectorAttribute.Select<FAClass.classID>(e.Cache, e.Row, e.NewValue) as FAClass;
				e.NewValue = faClass?.AssetCD;

				throw new PXSetPropertyException(Messages.OperationCanNotBeCompletedInMigrationMode);
			}
		}

		protected virtual void FixedAsset_ClassID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			FixedAsset asset = (FixedAsset)e.Row;
			if (asset?.ClassID == null) return;

			FixedAsset cls = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>.SelectSingleBound(this, new object[] { asset });
			if (cls != null && string.CompareOrdinal((string)e.NewValue, cls.AssetCD) != 0)
			{
				WebDialogResult answer = CurrentAsset.Ask(asset, GL.Messages.ImportantConfirmation, Messages.FAClassChangeConfirmation, MessageButtons.YesNo, MessageIcon.Question);
				if (answer == WebDialogResult.No)
				{
					e.NewValue = cls.AssetCD;
				}
			}
		}

		protected virtual void FixedAsset_ClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FixedAsset currAsset = (FixedAsset)e.Row;
			if (currAsset == null) return;

			foreach(KeyValuePair<Type, Type> accountSubPair in GetFAAccountSubPairs())
			{
				if(IsAccountChangeable(currAsset, accountSubPair.Key))
				{
					sender.SetDefaultExt(currAsset, accountSubPair.Key.Name);
					sender.RaiseExceptionHandling(accountSubPair.Value.Name, currAsset, null, null);
					sender.SetDefaultExt(currAsset, accountSubPair.Value.Name);
				}
			}

			if (e.OldValue != null)
			{
				SelectFrom<FABookSettings>
				.Where<FABookSettings.assetID.IsEqual<FixedAsset.classID.FromCurrent>>
				.View.SelectMultiBound(this, new[] { e.Row })
				.RowCast<FABookSettings>()
				.ForEach(sett =>
				{
					FABookBalance balance = AssetBalance.Cache.Locate(new FABookBalance { AssetID = currAsset.AssetID, BookID = sett.BookID }) as FABookBalance;
					if (balance != null && balance.DepreciationMethodID == null && balance.AveragingConvention == null)
					{
						balance.DepreciationMethodID = GetDeprMethodOfAssetType(sett.DepreciationMethodID, balance);
						balance.AveragingConvention = sett.AveragingConvention;
						AssetBalance.Update(balance);
					}
				});

				return;
			}

			sender.SetDefaultExt<FixedAsset.isTangible>(e.Row);
			sender.SetDefaultExt<FixedAsset.assetTypeID>(e.Row);
			sender.SetDefaultExt<FixedAsset.usefulLife>(e.Row);
			sender.SetDefaultExt<FixedAsset.serviceScheduleID>(e.Row);
			sender.SetDefaultExt<FixedAsset.usageScheduleID>(e.Row);

			SelectFrom<FABookSettings>
				.Where<FABookSettings.assetID.IsEqual<FixedAsset.classID.FromCurrent>>
				.View.SelectMultiBound(this, new[] { e.Row })
				.RowCast<FABookSettings>()
				.ForEach(sett => AssetBalance.Insert(new FABookBalance
				{
					AssetID = currAsset.AssetID,
					ClassID = currAsset.ClassID,
					BookID = sett.BookID,
					AveragingConvention = sett.AveragingConvention,
				}));

			AssetBalance.Cache.IsDirty = false;

			FAClass cls = PXSelect<FAClass, Where<FAClass.assetID, Equal<Required<FixedAsset.classID>>>>.SelectSingleBound(sender.Graph, null, currAsset.ClassID);
			currAsset.HoldEntry = cls.HoldEntry;
		}

		protected virtual void FixedAsset_Status_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FixedAsset row = (FixedAsset) e.Row;
			if (row == null) return;
			if (row.Status == FixedAssetStatus.Hold && (string)e.NewValue != FixedAssetStatus.Hold)
			{
				FABookBalance balance = AssetBalance.Select()
					.RowCast<FABookBalance>()
					.Where(bookbal => fasetup.Current.UpdateGL == true && bookbal.UpdateGL == true && string.IsNullOrEmpty(bookbal.InitPeriod))
					.Select(bookbal => new
					{
						bookbal,
						AcquiredCost = PXSelect<FATran, Where<FATran.assetID, Equal<Current<FABookBalance.assetID>>,
							And<FATran.bookID, Equal<Current<FABookBalance.bookID>>,
								And<FATran.tranType, Equal<FATran.tranType.purchasingPlus>>>>>.SelectMultiBound(this, new object[] { bookbal })
								.RowCast<FATran>()
								.Aggregate<FATran, decimal?>(0m, (current, tran) => current + tran.TranAmt)
					})
					.Where(t => t.bookbal.AcquisitionCost - t.AcquiredCost >= 0.00005m).Select(t => t.bookbal).FirstOrDefault();
				if (balance != null)
				{
					throw new PXSetPropertyException(Messages.UnholdAssetOutOfBalance, PXCurrencyAttribute.BaseRound(this, balance.AcquisitionCost));
				}
			}
		}
		#endregion

		#region Detail Events
		public static void UpdateBalances<Field, ParentField>(PXCache sourceCache, PXFieldUpdatedEventArgs e)
			where Field : IBqlField
			where ParentField : IBqlField
		{
			Type balType = BqlCommand.GetItemType<Field>();
			PXCache balanceCache = sourceCache.Graph.Caches[balType];

			BqlCommand command = BqlTemplate.OfCommand<
			Select<BqlPlaceholder.A, 
				Where<BqlPlaceholder.B, Equal<Required<BqlPlaceholder.C>>>>>
				.Replace<BqlPlaceholder.A>(balType)
				.Replace<BqlPlaceholder.B>(balanceCache.GetBqlField<FixedAsset.assetID>())
				.Replace<BqlPlaceholder.C>(sourceCache.GetBqlField<FixedAsset.assetID>())
				.ToCommand();

			Type newsort = null;
			for (var i = balanceCache.BqlKeys.Count - 1; i >= 0; i--)
			{
				var key = balanceCache.BqlKeys[i];
				if (newsort == null)
				{
					newsort = typeof(Asc<>).MakeGenericType(key);
				}
				else
				{
					newsort = typeof(Asc<,>).MakeGenericType(key, newsort);
				}
			}
			
			if (newsort != null)
				command = command.OrderByNew(typeof(OrderBy<>).MakeGenericType(newsort));

			foreach (object bal in new PXView(sourceCache.Graph, false, command).SelectMulti(sourceCache.GetValue(e.Row, nameof(FixedAsset.assetID))))
			{
				balanceCache.RaiseRowSelected(bal);

				PXFieldState state;
				if ((state = (PXFieldState)balanceCache.GetStateExt<Field>(bal)) != null && state.Enabled)
				{
					object newBal = balanceCache.CreateCopy(bal);
					balanceCache.SetValue<Field>(newBal, sourceCache.GetValue<ParentField>(e.Row));
					try
					{
						FABookBalance newBalance = newBal as FABookBalance;
						if (newBalance != null) newBalance.NoteID = Guid.NewGuid(); //because copy is updating later
						if (newBalance != null && typeof(ParentField) == typeof(FADetails.depreciateFromDate) && newBalance.DeprFromDate != null)
						{
							((sourceCache.Graph as DepreciationCalculation)?.Params ?? new DeprCalcParameters()).Fill(sourceCache.Graph, newBalance);
						}
						balanceCache.Update(newBal);
					}
					catch (PXSetPropertyException ex)
					{
						if (ex.InnerException is TranDateOutOfRangeException)
						{
							throw new PXSetPropertyException(Messages.TranDateOutOfRange, balanceCache.GetValueExt<FABookBalance.bookID>(bal));
						}
						throw;
					}
				}
			}
		}

		protected virtual void FADetails_DepreciateFromDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FADetails det = e.Row as FADetails;

			if (det?.ReceiptDate != null && e.NewValue != null && det.ReceiptDate.Value.CompareTo(e.NewValue) > 0)
			{
				throw new PXSetPropertyException(Messages.PlacedInServiceDateIsEarlierThanReceiptDate,
					PXUIFieldAttribute.GetDisplayName<FADetails.depreciateFromDate>(sender),
					PXUIFieldAttribute.GetDisplayName<FADetails.receiptDate>(sender));
			}
		}

		protected virtual void FADetails_DepreciateFromDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			try
			{
				UpdateBalances<FABookBalance.deprFromDate, FADetails.depreciateFromDate>(sender, e);
				sender.RaiseExceptionHandling<FADetails.depreciateFromDate>(e.Row, ((FADetails)(e.Row)).DepreciateFromDate, null);
			}
			catch (PXException ex)
			{
				sender.SetValue<FADetails.depreciateFromDate>(e.Row, e.OldValue);
				sender.RaiseExceptionHandling<FADetails.depreciateFromDate>(e.Row, e.OldValue, ex);
			}

			GLTrnFilter.Cache.SetDefaultExt<GLTranFilter.tranDate>(GLTrnFilter.Current);
		}

		protected virtual void FADetails_AcquisitionCost_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UpdateBalances<FABookBalance.acquisitionCost, FADetails.acquisitionCost>(sender, e);

			GLTrnFilter.Cache.SetDefaultExt<GLTranFilter.acquisitionCost>(GLTrnFilter.Current);
		}

		protected virtual void FADetails_SalvageAmount_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UpdateBalances<FABookBalance.salvageAmount, FADetails.salvageAmount>(sender, e);
		}

		protected virtual void FADetails_SalvageAmount_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FADetails det = (FADetails)e.Row;

			if ((decimal?) e.NewValue > det.AcquisitionCost)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, det.AcquisitionCost);
			}
		}

		protected virtual void FADetails_TagNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			FixedAsset asset = (FixedAsset)PXParentAttribute.SelectParent(sender, e.Row);
			if (asset?.AssetCD != null && fasetup.Current.CopyTagFromAssetID == true)
			{
				e.NewValue = asset.AssetCD;
				e.Cancel = true;
			}
		}

		protected virtual void FADetails_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FADetails det = (FADetails)e.Row;
			if (det != null && fasetup.Current.CopyTagFromAssetID == true && (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				det.TagNbr = _PersistedAsset.AssetCD;
			}

			PXDefaultAttribute.SetPersistingCheck<FADetails.depreciateFromDate>(sender, e.Row, Asset.Current?.UnderConstruction == true ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
		}

		protected virtual void FASetup_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				FASetup setup = (FASetup)e.Row;
				if (setup.CopyTagFromAssetID == true)
				{
					setup.TagNumberingID = null;
				}
			}
		}

		#endregion
		#region FABookBalance Events
		protected virtual void FABookBalance_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			FABookBalance bookbal = (FABookBalance)e.Row;

			bool acquired = !string.IsNullOrEmpty(bookbal.InitPeriod);

			if (acquired && e.ExternalCall)
			{
				throw new PXSetPropertyException(Messages.BalanceRecordCannotBeDeleted);
			}
		}

		protected virtual void _(Events.RowPersisting<FABookBalance> e)
		{
			FABookBalance bookBalance = e.Row;

			if (fasetup.Current.UpdateGL != true &&
				bookBalance.UpdateGL == true &&
				Asset.Current != null /* there is not the deletion operation */)
			{
				int organizationID = (int)PXAccess.GetParentOrganizationID(Asset.Current.BranchID);

				List<string> periodIDs = new List<string>();

				if (bookBalance.DeprFromPeriod != null) // not under construction
				{
					periodIDs.Add(bookBalance.DeprFromPeriod);
				}

				if (bookBalance.LastDeprPeriod != null)
				{
					periodIDs.Add(bookBalance.LastDeprPeriod);
				}

				foreach (string periodID in periodIDs)
				{
					PXResultset<FABookPeriod, FinPeriod> synchronizedPeriod = SelectFrom<FABookPeriod>
					.LeftJoin<FinPeriod>
						.On<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID>
							.And<FABookPeriod.finPeriodID.IsEqual<FinPeriod.finPeriodID>>>
					.Where<FABookPeriod.finPeriodID.IsEqual<@P.AsString>
						.And<FABookPeriod.organizationID.IsEqual<@P.AsInt>>
						.And<FABookPeriod.bookID.IsEqual<@P.AsInt>>>
					.View
					.Select<PXResultset<FABookPeriod, FinPeriod>>(this, periodID, organizationID, bookBalance.BookID);

					FABookPeriod faPeriod = synchronizedPeriod;
					FinPeriod finPeriod = synchronizedPeriod;

					PXException exception = new PXException(Messages.PostingBookNotMatchFinPeriodInGL, PeriodIDAttribute.FormatForError(periodID), PXAccess.GetOrganizationCD(organizationID));

					if (faPeriod == null ||
						(faPeriod != null && finPeriod != null && finPeriod.FinPeriodID != null &&
						(faPeriod.StartDate != finPeriod.StartDate ||
						faPeriod.EndDate != finPeriod.EndDate)))
					{
						throw exception;
					}

					if(finPeriod == null || finPeriod.FinPeriodID == null)
					{
						FinPeriod firstFinPeriod = FinPeriodRepository.FindFirstPeriod(organizationID);
						if (firstFinPeriod == null ||
							firstFinPeriod.FinPeriodID == null ||
							firstFinPeriod.StartDate < faPeriod.StartDate)
						{
							throw exception;
						}
					}
				}
			}
		}

		protected virtual void _(Events.RowInserted<FABookBalance> e)
		{
			AssetProcess.AdjustFixedAssetStatus(this, e.Row.AssetID);
		}

		protected virtual void _(Events.RowDeleted<FABookBalance> e)
		{
			AssetProcess.AdjustFixedAssetStatus(this, e.Row.AssetID);
		}

		protected virtual void FABookBalance_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FABookBalance bookbal = (FABookBalance)e.Row;
			if (bookbal == null) return;

			bool isDepreciated = false;
			bool isAcquired = !string.IsNullOrEmpty(bookbal.InitPeriod);
			bool isInitMode = fasetup.Current.UpdateGL != true;
			bool isNewAsset = sender.GetStatus(e.Row) == PXEntryStatus.Inserted;
			bool isUnderConstruction = this.Asset.Current.UnderConstruction == true;

			if (!isNewAsset)
			{
				FATran other = PXSelectReadonly<FATran, Where<FATran.assetID, Equal<Required<FABookBalance.assetID>>,
						And<FATran.bookID, Equal<Required<FABookBalance.bookID>>>>,
					OrderBy<
						Desc<Switch<Case<Where<FATran.origin, Equal<FARegister.origin.depreciation>>, intMax,
						Case<Where<FATran.origin, Equal<FARegister.origin.purchasing>>, int2,
						Case<Where<FATran.origin, Equal<FARegister.origin.reconcilliation>>, int1>>>, int0>,
						Desc<Switch<Case<Where<FATran.tranType, Equal<FATran.tranType.depreciationPlus>>, intMax,
							Case<Where<FATran.tranType, Equal<FATran.tranType.depreciationMinus>>, intMax,
							Case<Where<FATran.tranType, Equal<FATran.tranType.calculatedPlus>>, intMax,
							Case<Where<FATran.tranType, Equal<FATran.tranType.calculatedMinus>>, intMax>>>>, int0>,
						Desc<FATran.released, 
						Asc<FATran.refNbr, 
						Asc<FATran.lineNbr>>>>>>>.SelectSingleBound(this, null, bookbal.AssetID, bookbal.BookID);

				isDepreciated = IsDepreciated(other);
				isNewAsset = !isDepreciated && !IsPurchased(other);
			}

			bool isMigratedDepreciationEnabled = isNewAsset || IsPureMigrated(bookbal);

			bool isDepreciable = bookbal?.Depreciate == true;

			PXUIFieldAttribute.SetEnabled<FABookBalance.lastDeprPeriod>(sender, e.Row, isMigratedDepreciationEnabled && isInitMode && isDepreciable && !isUnderConstruction);
			PXUIFieldAttribute.SetEnabled<FABookBalance.ytdDepreciated>(sender, e.Row, isMigratedDepreciationEnabled && isInitMode && isDepreciable && !isUnderConstruction);
			PXUIFieldAttribute.SetEnabled<FABookBalance.deprFromDate>(sender, e.Row, !isDepreciated);
			PXUIFieldAttribute.SetEnabled<FABookBalance.acquisitionCost>(sender, e.Row, !isAcquired || (isNewAsset && isInitMode));

			PXUIFieldAttribute.SetEnabled<FABookBalance.tax179Amount>(sender, e.Row, !isAcquired && isDepreciable);
			PXUIFieldAttribute.SetEnabled<FABookBalance.bonusAmount>(sender, e.Row, !isAcquired && isDepreciable);
			PXUIFieldAttribute.SetEnabled<FABookBalance.bonusRate>(sender, e.Row, !isAcquired && isDepreciable);
			PXUIFieldAttribute.SetEnabled<FABookBalance.bonusID>(sender, e.Row, !isAcquired && isDepreciable);

			if (bookbal.Status == FixedAssetStatus.Disposed)
			{
				PXUIFieldAttribute.SetEnabled(sender, bookbal, false);
			}

			IYearSetup yearSetup = FABookPeriodRepository.FindFABookYearSetup(bookbal.BookID);
			FADepreciationMethod meth = PXSelect<FADepreciationMethod, Where<FADepreciationMethod.methodID, Equal<Current<FABookBalance.depreciationMethodID>>>>.SelectSingleBound(this, new object[] { bookbal });

			PXUIFieldAttribute.SetEnabled<FABookBalance.usefulLife>(sender, bookbal, bookbal.Status == FixedAssetStatus.Active);
			PXUIFieldAttribute.SetEnabled<FABookBalance.averagingConvention>(sender, bookbal, meth != null && !meth.IsYearlyAccountancyTableMethod);

			List<KeyValuePair<object, Dictionary<object, string[]>>> parsList = new List<KeyValuePair<object, Dictionary<object, string[]>>>();
			if (meth != null)
			{
				parsList.Add(meth.IsTableMethod == true
					? new KeyValuePair<object, Dictionary<object, string[]>>(meth.RecordType, FAAveragingConvention.RecordTypeDisabledValues)
					: new KeyValuePair<object, Dictionary<object, string[]>>(meth.DepreciationMethod, FAAveragingConvention.DeprMethodDisabledValues));
			}
			if (yearSetup != null)
			{
				parsList.Add(new KeyValuePair<object, Dictionary<object, string[]>>(yearSetup.IsFixedLengthPeriod, FAAveragingConvention.FixedLengthPeriodDisabledValues));
			}

			FAAveragingConvention.SetAveragingConventionsList<FADepreciationMethod.averagingConvention>(sender, bookbal, parsList.ToArray());
			PXUIFieldAttribute.SetRequired<FABookBalance.deprFromDate>(sender, bookbal.Depreciate == true);

			FABookSettings booksettings = PXSelect<FABookSettings, Where<FABookSettings.assetID, Equal<Current<FixedAsset.classID>>,
				And<FABookSettings.bookID, Equal<Required<FABookBalance.bookID>>>>>.Select(this, bookbal.BookID);
			PXUIFieldAttribute.SetEnabled<FABookBalance.tax179Amount>(sender, bookbal, booksettings?.Sect179 == true);
			PXUIFieldAttribute.SetEnabled<FABookBalance.ytdTax179Recap>(sender, bookbal, booksettings?.Sect179 == true);

			bookbal.AllowChangeDeprFromPeriod = !isDepreciated;
		}

		/// <exclude/>
		protected virtual void _(Events.FieldVerifying<FABookBalance.deprFromDate> e)
		{
			FABookBalance balance = e.Row as FABookBalance;
			if (balance == null || e.NewValue == null) return;

			FADetails details = AssetDetails.Current ?? AssetDetails.Select();

			if (details.ReceiptDate != null && details.ReceiptDate.Value.CompareTo(e.NewValue) > 0)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, AssetDetails.Current.ReceiptDate.Value.ToShortDateString());
			}

			CheckLastDeprPeriodBetweenDeprFromAndDeprTo<FABookBalance.deprFromDate>(e.Cache, balance, e.NewValue);
		}

		protected virtual void FABookBalance_DeprFromPeriod_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FABookBalance bal = e.Row as FABookBalance;
			string oldValue = (string)e.OldValue;

			bool isUnderConstruction = Asset.Current?.UnderConstruction == true;
			
			if (!isUnderConstruction && (bal == null ||
				string.IsNullOrEmpty(bal.DeprFromPeriod) ||
				string.IsNullOrEmpty(oldValue) ||
				string.CompareOrdinal(bal.DeprFromPeriod, oldValue) == 0)) return;

			if (isUnderConstruction && (bal == null ||
				string.IsNullOrEmpty(bal.DeprFromPeriod) ||
				string.IsNullOrEmpty(oldValue) == false ||
				string.CompareOrdinal(bal.DeprFromPeriod, oldValue) == 0)) return;

			if (string.CompareOrdinal(bal.DeprFromPeriod, bal.CurrDeprPeriod) != 0 
				&& bal.LastDeprPeriod == null)
			{
				bal.CurrDeprPeriod = bal.DeprFromPeriod;
				if (bal.InitPeriod != null)
				{
					bal.InitPeriod = bal.DeprFromPeriod;
				}
			}

			// We shouldn't do something with FABookHistory
			// in the "inicialize" mode, because in such case,
			// purchasing transaction will be recreated together
			// with history.
			//
			if (fasetup.Current.UpdateGL != true
				|| SuppressHistoryUpdateScope.IsActive) return;

			if(isUnderConstruction)
			{
				FABookHistory purchasingHistory = SelectFrom<FABookHistory>
				.Where<FABookHistory.assetID.IsEqual<@P.AsInt>
					.And<FABookHistory.bookID.IsEqual<@P.AsInt>>
					.And<FABookHistory.ptdDeprBase.IsNotEqual<decimal0>>>
				.OrderBy<FABookHistory.finPeriodID.Asc>
				.View
				.Select(this, bal.AssetID, bal.BookID);

				oldValue = purchasingHistory?.FinPeriodID ?? bal.DeprFromPeriod;
			}

			bool isPeriodGreater = string.CompareOrdinal(bal.DeprFromPeriod, oldValue) > 0;
			string fromPeriod = isPeriodGreater ? oldValue : bal.DeprFromPeriod;
			string toPeriod = isPeriodGreater ? bal.DeprFromPeriod : oldValue;

			decimal? prevPtdDeprBase = 0m;
			decimal sign = isPeriodGreater ? 1m : -1m;

			// We need FABookPeriod records here to cover situations
			// when we don't have FABookHist history yet for the new
			// depreciation interval.
			//

			foreach (PXResult<FABookPeriod, FABookHist> res in PXSelectJoin<
				FABookPeriod,
				LeftJoin<FABookHist, 
					On<FABookHist.assetID, Equal<Required<FABookBalance.assetID>>,
					And<FABookHist.bookID, Equal<FABookPeriod.bookID>,
					And<FABookHist.finPeriodID, Equal<FABookPeriod.finPeriodID>>>>>,
				Where<FABookPeriod.bookID, Equal<Required<FABookBalance.bookID>>,
					And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
					And<FABookPeriod.finPeriodID, GreaterEqual<Required<FABookBalance.deprFromPeriod>>,
					And<FABookPeriod.finPeriodID, LessEqual<Required<FABookBalance.deprToPeriod>>>>>>,
				OrderBy<
					Asc<FABookPeriod.finPeriodID>>>
				.Select(
					this, 
					bal.AssetID, 
					bal.BookID,
					FABookPeriodRepository.GetFABookPeriodOrganizationID(bal),
					fromPeriod, 
					toPeriod))
			{
				FABookPeriod bookPeriod = res;
				FABookHist origHist = res;

				FABookHist hist = new FABookHist
				{
					AssetID = bal.AssetID,
					BookID = bal.BookID,
					FinPeriodID = bookPeriod.FinPeriodID
				};

				hist = FAHelper.InsertFABookHist(this, hist, ref bal);

				decimal? ptdDeprBase = isPeriodGreater
					? origHist.PtdDeprBase
					: string.CompareOrdinal(origHist.FinPeriodID ?? bookPeriod.FinPeriodID, fromPeriod) == 0
						? (origHist.YtdAcquired ?? bal.YtdAcquired)
						: origHist.PtdAcquired;

				ptdDeprBase = (ptdDeprBase ?? 0m) * sign;
				hist.PtdDeprBase -= ptdDeprBase;
				hist.YtdDeprBase -= ptdDeprBase;
				prevPtdDeprBase += ptdDeprBase;

				if (string.CompareOrdinal(bookPeriod.FinPeriodID, toPeriod) == 0)
				{
					hist.PtdDeprBase += prevPtdDeprBase;
					hist.YtdDeprBase += prevPtdDeprBase;
				}
			}
		}

		protected virtual void FABookBalance_DeprFromPeriod_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			FABookBalance balance = e.Row as FABookBalance;
			if (balance != null && (e.Operation & PXDBOperation.Command) == PXDBOperation.Update && balance.AllowChangeDeprFromPeriod != true)
			{
				e.IsRestriction = true;
			}
		}

		protected virtual void FABookBalance_BookID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{

			if ((FABookSettings)PXSelect<FABookSettings,
						Where<FABookSettings.assetID, Equal<Current<FixedAsset.classID>>,
						And<FABookSettings.bookID, Equal<Required<FABookSettings.bookID>>>>>.Select(this, e.NewValue) == null)
			{
				throw new PXSetPropertyException(ErrorMessages.ElementDoesntExist, $"[{nameof(FABookBalance.bookID)}]");
			}

		}

		protected virtual void FABookBalance_DepreciationMethodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FABookBalance bookBalance = (FABookBalance)e.Row;
			if (bookBalance == null) return;

			if (bookBalance.BookID != null)
			{
				IYearSetup yearSetup = FABookPeriodRepository.FindFABookYearSetup(bookBalance.BookID);
				FADepreciationMethod deprMethod = PXSelectorAttribute.Select<FABookSettings.depreciationMethodID>(DepreciationSettings.Cache, bookBalance, e.NewValue) as FADepreciationMethod;

				if ((yearSetup.PeriodType == FinPeriodType.Week
						|| yearSetup.PeriodType == FinPeriodType.BiWeek
						|| yearSetup.PeriodType == FinPeriodType.FourWeek)
					&& (deprMethod?.IsNewZealandMethod == true))
				{
					e.NewValue = deprMethod?.MethodCD;

					string errorMessage = PXMessages.LocalizeFormat(Messages.WeeklyBooksDisabledForCalcMethod,
														PXStringListAttribute.GetLocalizedLabel<FADepreciationMethod.depreciationMethod>(Caches[typeof(FADepreciationMethod)], deprMethod));
					throw new PXSetPropertyException(errorMessage);
				}
			}
		}

		protected virtual void FABookBalance_DepreciationMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FABookBalance bal = (FABookBalance)e.Row;
			if (bal?.DepreciationMethodID == null) return;

			if (e.ExternalCall)
			{
				FADepreciationMethod method = PXSelect<
					FADepreciationMethod,
					Where<FADepreciationMethod.methodID, Equal<Current<FABookBalance.depreciationMethodID>>>>
					.SelectSingleBound(this, new object[] { bal });

				sender.SetValueExt<FABookBalance.averagingConvention>(bal, method?.AveragingConvention ?? bal.AveragingConvention);
			}

			if (IsImport) // #35003
			{
				bool failed = false;
				object methID = bal.DepreciationMethodID;
				try
				{
					sender.RaiseFieldVerifying<FABookBalance.depreciationMethodID>(bal, ref methID);
				}
				catch (PXException)
				{
					failed = true;
				}
				if (!failed)
				{
					sender.RaiseExceptionHandling<FABookBalance.depreciationMethodID>(bal, methID, null);
				}
			}
		}

		protected virtual void FABookBalance_DeprToPeriod_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FABookBalance bal = (FABookBalance)e.Row;
			if (bal != null && e.OldValue != null)
			{
				bal.LastPeriod = bal.DeprToPeriod;
			}
		}

		protected virtual void FABookBalance_SalvageAmount_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FABookBalance bal = (FABookBalance)e.Row;

			if (!IsValueInSignedRange((decimal?)e.NewValue, bal.AcquisitionCost, out (decimal MinValue, decimal MaxValue) range))
			{
				throw new PXSetPropertyException(CS.Messages.EntryInRange, range.MinValue, range.MaxValue);
			}
		}

		protected virtual void FABookBalance_LastDeprPeriod_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FABookBalance bal = (FABookBalance)e.Row;
			if (e.NewValue == null) return;

			if (string.CompareOrdinal((string)e.NewValue, bal.DeprFromPeriod) < 0)
			{
				e.NewValue = FinPeriodIDAttribute.FormatForDisplay((string)e.NewValue);
				throw new PXSetPropertyException(CS.Messages.Entry_GE, FinPeriodIDAttribute.FormatForError(bal.DeprFromPeriod));
			}
			if (string.CompareOrdinal((string)e.NewValue, bal.DeprToPeriod) > 0)
			{
				e.NewValue = FinPeriodIDAttribute.FormatForDisplay((string)e.NewValue);
				throw new PXSetPropertyException(CS.Messages.Entry_LE, FinPeriodIDAttribute.FormatForError(bal.DeprToPeriod));
			}
		}

		protected virtual void FABookBalance_DeprToDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FABookBalance bal = (FABookBalance)e.Row;

			if ((DateTime?) e.NewValue < bal.DeprFromDate)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, bal.DeprFromDate);
			}
		}
		#endregion
		#region FAUsage Events
		protected virtual void FAUsage_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			FAUsage usage = (FAUsage)e.Row;
			if (usage == null) return;

			if (usage.Depreciated == true)
			{
				throw new PXSetPropertyException(Messages.BalanceRecordCannotBeDeleted);
			}
		}

		protected virtual void FAUsage_Value_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FAUsage cur = (FAUsage)e.Row;
			if (cur == null) return;

			if ((decimal)e.NewValue <= 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, 0m);
			}

			FAUsage prev = PXSelect<FAUsage, Where<FAUsage.assetID, Equal<Current<FADetails.assetID>>,
				And<FAUsage.number, Less<Current<FAUsage.number>>>>,
									OrderBy<Desc<FAUsage.number>>>.SelectSingleBound(this, new object[] { cur });

			if (prev != null && (decimal)e.NewValue <= prev.Value)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, prev.Value);
			}

			FADetails det = PXSelect<FADetails, Where<FADetails.assetID, Equal<Required<FADetails.assetID>>>>.Select(this, cur.AssetID);
			if (det != null && (decimal)e.NewValue > det.TotalExpectedUsage)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, det.TotalExpectedUsage);
			}
		}

		protected virtual void FAUsage_MeasurementDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			FAUsage cur = (FAUsage)e.Row;
			e.NewValue = Accessinfo.BusinessDate;
			if (cur == null || e.NewValue == null) return;

			FAUsage prev = PXSelect<FAUsage, Where<FAUsage.assetID, Equal<Current<FADetails.assetID>>,
				And<FAUsage.number, Less<Current<FAUsage.number>>>>,
									OrderBy<Desc<FAUsage.number>>>.SelectSingleBound(this, new object[] { cur });

			if (prev != null && ((DateTime)e.NewValue) <= prev.MeasurementDate)
			{
				sender.RaiseExceptionHandling<FAUsage.measurementDate>(cur, e.NewValue, new PXSetPropertyException(CS.Messages.Entry_GT, prev.MeasurementDate));
			}
		}

		protected virtual void FAUsage_MeasurementDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FAUsage cur = (FAUsage)e.Row;
			if (cur == null) return;

			FAUsage prev = PXSelect<FAUsage, Where<FAUsage.assetID, Equal<Current<FADetails.assetID>>,
				And<FAUsage.number, Less<Current<FAUsage.number>>>>,
									OrderBy<Desc<FAUsage.number>>>.SelectSingleBound(this, new object[] { cur });

			if (prev != null && ((DateTime)e.NewValue) <= prev.MeasurementDate)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, prev.MeasurementDate);
			}
			else
			{
				sender.RaiseExceptionHandling<FAUsage.measurementDate>(cur, e.NewValue, null);
			}
		}

		#endregion
		#region FALocationHistory Events
		protected virtual void FALocationHistory_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			FALocationHistory location = e.NewRow as FALocationHistory;
			if (location == null) return;

			if (FAInnerStateDescriptor.IsAcquired(location.AssetID, this) &&
				!FAInnerStateDescriptor.WillBeTransferred(location.AssetID, this) &&
				this.IsLocationChanged(e.Row as FALocationHistory, location))
			{
				FALocationHistory newLocation = PXCache<FALocationHistory>.CreateCopy(location);
				try
				{
					if (sender.GetStatus(e.Row) == PXEntryStatus.Updated)
						sender.SetStatus(e.Row, PXEntryStatus.Notchanged);

					newLocation.RefNbr = null;
					newLocation.PeriodID = GetTransferPeriod(location.AssetID);
					newLocation.ClassID = CurrentAsset.Current.ClassID;
					newLocation.RevisionID = AssetDetails.Current.LocationRevID;
					newLocation.TransactionDate = GetTransferDate(location.AssetID, newLocation.PeriodID);
					newLocation = (FALocationHistory)sender.Insert(newLocation);
				}
				finally
				{
					e.Cancel = newLocation != null;

					if (!e.Cancel)
						sender.MarkUpdated(location);
				}
			}
		}

		public static void LiveUpdateMaskedSubs(PXGraph graph, PXCache facache, FALocationHistory lochist)
		{
			if (lochist == null) return;
			FixedAsset asset = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FALocationHistory.assetID>>>>.SelectSingleBound(graph, new object[] { lochist });
			FABookBalance bal = PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>, OrderBy<Desc<FABookBalance.updateGL>>>.SelectSingleBound(graph, new object[] { asset });

			if (bal == null || bal.YtdDeprBase == 0m)
			{
				asset.FASubID = MakeSubID<FixedAsset.fASubMask, FixedAsset.fASubID>(facache, asset);
				asset.AccumulatedDepreciationSubID = MakeSubID<FixedAsset.accumDeprSubMask, FixedAsset.accumulatedDepreciationSubID>(facache, asset);
				asset.DepreciatedExpenseSubID = MakeSubID<FixedAsset.deprExpenceSubMask, FixedAsset.depreciatedExpenseSubID>(facache, asset);
				asset.DisposalSubID = MakeSubID<FixedAsset.proceedsSubMask, FixedAsset.disposalSubID>(facache, asset);
				asset.GainSubID = MakeSubID<FixedAsset.gainLossSubMask, FixedAsset.gainSubID>(facache, asset);
				asset.LossSubID = MakeSubID<FixedAsset.gainLossSubMask, FixedAsset.lossSubID>(facache, asset);

				facache.MarkUpdated(asset);
			}
		}

		protected virtual void FALocationHistory_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			LiveUpdateMaskedSubs(this, Asset.Cache, (FALocationHistory)e.Row);
		}

		protected virtual void FALocationHistory_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			LiveUpdateMaskedSubs(this, Asset.Cache, (FALocationHistory)e.Row);
		}

		protected virtual void FALocationHistory_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			FALocationHistory newLocation = (FALocationHistory)e.Row;
			if (newLocation == null) return;
			foreach (FALocationHistory row in sender.Cached)
			{
				PXEntryStatus status = sender.GetStatus(row);
				if (status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated)
				{
					CopyLocation(newLocation, row);
					e.Cancel = true;
					break;
				}
			}

			if (!e.Cancel)
			{
				if (AssetDetails.Current == null)
				{
					AssetDetails.Current = AssetDetails.Select();
				}
				AssetDetails.Current.LocationRevID = ++newLocation.RevisionID;
				AssetDetails.Cache.Update(AssetDetails.Current);
			}
		}

		protected virtual void FALocationHistory_EmployeeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FALocationHistory hist = (FALocationHistory)e.Row;
			sender.SetDefaultExt<FALocationHistory.locationID>(hist);
			sender.SetDefaultExt<FALocationHistory.department>(hist);
		}

		protected virtual void _(Events.FieldVerifying<FALocationHistory.locationID> e)
		{
			FixedAsset asset = CurrentAsset.Current;
			if (asset == null) return;

			if (fasetup.Current.UpdateGL != true && HasAnyGLTran(asset.AssetID))
			{
				Branch branch = PXSelectorAttribute.Select<FALocationHistory.locationID>(e.Cache, e.Row, e.NewValue) as Branch;
				e.NewValue = branch?.BranchCD;

				throw new PXSetPropertyException(Messages.OperationCanNotBeCompletedInMigrationMode);
			}
		
			if(PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>() && asset.IsAcquired == true)
			{
				int? originalOrganizationID = PXAccess.GetParentOrganizationID(asset.BranchID);
				int? targetOrganizationID = PXAccess.GetParentOrganizationID((int?)e.NewValue);

				if(originalOrganizationID != targetOrganizationID)
				{
					e.NewValue = PXAccess.GetBranchCD((int?)e.NewValue);
					throw new PXSetPropertyException(Messages.NotAllowedTransferBetweenOrganizations);
				}
			}
		}

		protected virtual void FALocationHistory_LocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.ExternalCall)
			{
				sender.SetDefaultExt<FALocationHistory.buildingID>(e.Row);
				sender.SetValuePending<FALocationHistory.buildingID>(e.Row, null);
			}

			FixedAsset currentAsset = CurrentAsset.Current;
			FADetails currentDetails = AssetDetails.Select();
			FALocationHistory locationHistory = e.Row as FALocationHistory;
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>() 
				&& currentAsset.IsAcquired != true 
				&& currentDetails != null 
				&& locationHistory != null)
			{
				currentAsset.BranchID = locationHistory.BranchID;
				CurrentAsset.Cache.MarkUpdated(currentAsset);
				foreach(FABookBalance balance in AssetBalance.Select())
				{
					FABookBalance updatingBalance = PXCache<FABookBalance>.CreateCopy(balance);
					updatingBalance.NoteID = Guid.NewGuid(); //because copy is updating later
					updatingBalance.DeprFromPeriod = (string)PXFormulaAttribute.Evaluate<FABookBalance.deprFromPeriod>(AssetBalance.Cache, balance);
					updatingBalance.DeprToPeriod = (string)PXFormulaAttribute.Evaluate<FABookBalance.deprToPeriod>(AssetBalance.Cache, balance);
					AssetBalance.Update(updatingBalance);
				}
			}
		}

		protected virtual void FALocationHistory_BuildingID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall) e.Cancel = true;
		}

		protected virtual void _(Events.FieldVerifying<FALocationHistory.department> e)
		{
			FixedAsset asset = CurrentAsset.Current;

			if (asset == null) return;

			if (fasetup.Current.UpdateGL != true && HasAnyGLTran(asset.AssetID))
			{
				throw new PXSetPropertyException(Messages.OperationCanNotBeCompletedInMigrationMode);
			}
		}

		#endregion
		#region FAComponent Events
		protected virtual void FAComponent_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FixedAsset parent = Asset.Current;
			FAComponent child = (FAComponent)e.Row;
			if (parent == null || child == null) return;

			if (e.Operation == PXDBOperation.Delete)
			{
				e.Cancel = true;
			}
		}

		protected virtual void FAComponent_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			FAComponent child = (FAComponent)e.Row;
			FixedAsset parent = Asset.Current;
			if (child == null || parent == null) return;

			if (child.AssetCD != null)
			{
				FAComponent copy = PXSelectReadonly<FAComponent, Where<FAComponent.assetCD, Equal<Required<FAComponent.assetCD>>>>.Select(this, child.AssetCD);
				PXCache<FAComponent>.RestoreCopy((FAComponent)e.Row, copy);
				sender.SetStatus(e.Row, PXEntryStatus.Updated);
				sender.SetValue<FAComponent.parentAssetID>(e.Row, parent.AssetID);
			}
		}
		protected virtual void FAComponent_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			FAComponent child = (FAComponent)e.Row;

			sender.SetValue<FAComponent.parentAssetID>(child, null);
			sender.SetStatus(child, PXEntryStatus.Updated);
			e.Cancel = true;
		}
		#endregion
		#region ReverseDisposalInfo Events

		protected virtual void ReverseDisposalInfo_ReverseDisposalPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ReverseDisposalInfo row = (ReverseDisposalInfo)e.Row;
			if (row == null) return;

			OrganizationFinPeriod period = (OrganizationFinPeriod)PXSelectJoin<
				OrganizationFinPeriod,
				InnerJoin<Branch,
						On<Branch.organizationID, Equal<OrganizationFinPeriod.organizationID>>>,
				Where<OrganizationFinPeriod.finPeriodID, Equal<Required<OrganizationFinPeriod.finPeriodID>>,
					And<Branch.branchID, Equal<Current<FixedAsset.branchID>>>>>
				.Select(this, (string)e.NewValue);

			if (period != null)
			{
				if (period.Status == FinPeriod.status.Inactive)
				{
					sender.RaiseExceptionHandling<ReverseDisposalInfo.reverseDisposalPeriodID>(e.Row, null, 
						new FiscalPeriodInactiveException(period.FinPeriodID, PXAccess.GetOrganizationCD(period.OrganizationID), PXErrorLevel.Warning));
				}
				if (period.Status == FinPeriod.status.Locked)
				{
					sender.RaiseExceptionHandling<ReverseDisposalInfo.reverseDisposalPeriodID>(e.Row, null, 
						new FiscalPeriodLockedException(period.FinPeriodID, PXAccess.GetOrganizationCD(period.OrganizationID), PXErrorLevel.Warning));
				}
			}
		}

		#endregion
		#region FATran
		protected virtual void FATran_AssetID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void FATran_BookID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void FATran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			FATran tran = (FATran)e.Row;
			if ((tran.Released == true && e.ExternalCall) || (tran.Released == true && !string.IsNullOrEmpty(tran.BatchNbr)))
			{
				throw new PXSetPropertyException(ErrorMessages.CantDeleteRecord);
			}
		}

		protected virtual void FATran_RefNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			FATran tran = (FATran)e.Row;
			if (tran == null) return;

			if (AssetGLTransactions.IsTempKey(tran.RefNbr))
			{
				e.ReturnValue = null;
			}
		}

		protected virtual void GLTranFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (UnattendedMode && !IsContractBasedAPI && !IsImport && !IsExport)
				return;

			GLTranFilter filter = (GLTranFilter)e.Row;
			FixedAsset asset = CurrentAsset.Current;
			FADetails det = AssetDetails.Current ?? AssetDetails.Select();
			if (filter == null || asset == null || det == null) return;

			FATran unrelp = PXSelect<FATran, Where<FATran.assetID, Equal<Current<FixedAsset.assetID>>, And<FATran.released, Equal<False>, And<Where<FATran.tranType, Equal<FATran.tranType.purchasingPlus>, Or<FATran.tranType, Equal<FATran.tranType.purchasingMinus>>>>>>>.Select(this);
			FATran unrelr = PXSelect<FATran, Where<FATran.assetID, Equal<Current<FixedAsset.assetID>>, And<FATran.released, Equal<False>, And<Where<FATran.tranType, Equal<FATran.tranType.reconcilliationPlus>, Or<FATran.tranType, Equal<FATran.tranType.reconcilliationMinus>>>>>>>.Select(this);
			FinPeriod finPeriod = null;
			if (filter.TranDate != null && filter.BranchID != null)
			{
				finPeriod = FinPeriodRepository.FindFinPeriodByDate(filter.TranDate, PXAccess.GetParentOrganizationID(filter.BranchID));
			}

			PXUIFieldAttribute.SetWarning<GLTranFilter.acquisitionCost>(sender, filter,
																		CurrentAsset.Cache.GetStatus(asset) == PXEntryStatus.Inserted
																			? Messages.FixedAssetNotSaved
																			: null);
			PXUIFieldAttribute.SetWarning<GLTranFilter.currentCost>(sender, filter,
																		unrelp != null
																			? Messages.FixedAssetHasUnreleasedPurchasing
																			: null);
			PXUIFieldAttribute.SetWarning<GLTranFilter.accrualBalance>(sender, filter,
																		unrelr != null
																			? Messages.FixedAssetHasUnreleasedRecon
																			: null);
			PXUIFieldAttribute.SetError<GLTranFilter.unreconciledAmt>(sender, filter,
																		Math.Sign(filter.UnreconciledAmt ?? 0m) * Math.Sign(filter.CurrentCost ?? 0m) < 0
																			? string.Format(PXMessages.LocalizeNoPrefix(Messages.FixedAssetIsOverReconciled), asset?.AssetCD)
																			: null);
			PXUIFieldAttribute.SetWarning<GLTranFilter.periodID>(sender, filter,
																		finPeriod != null && finPeriod.FAClosed == true && glsetup.Current.PostClosedPeriods == false
																			? Messages.PeriodWillBeAutoChangedToNearestOpenPeriod
																			: null);

			PXUIFieldAttribute.SetEnabled<GLTranFilter.reconType>(sender, filter, det.IsReconciled ?? false);
			ReduceUnreconCost.SetEnabled(filter.ReconType == GLTranFilter.reconType.Addition && filter.UnreconciledAmt > 0m && filter.AccrualBalance > 0m && unrelp == null && unrelr == null);
		}
		#endregion

		protected virtual void DisposeParams_DisposalMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<DisposeParams.disposalAccountID>(e.Row);
			sender.SetDefaultExt<DisposeParams.disposalSubID>(e.Row);
		}
		
		protected virtual string GetMostRecentTransactionPeriodID(int? AssetID) {
			FATran trn = SelectFrom<FATran>
				.InnerJoin<FABook>
					.On<FABook.bookID.IsEqual<FATran.bookID>.And<FABook.updateGL.IsEqual<True>>>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.released.IsEqual<True>>>
				.OrderBy<Desc<FATran.tranPeriodID>>
				.View.SelectSingleBound(this, null, AssetID);
			return trn?.TranPeriodID;
		}
		/// <summary>
		/// find the most recent transaction in non-posting book. Return transaction from posting book if there is no non-posting book for the asset
		/// </summary>
		/// <returns></returns>
		protected virtual FATran GetMostRecentTransaction(int? AssetID)
		{
			return SelectFrom<FATran>
				.InnerJoin<FABook>
					.On<FABook.bookID.IsEqual<FATran.bookID>>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.released.IsEqual<True>>>
				.OrderBy<Asc<FABook.updateGL>, Desc<FATran.tranDate>>
				.View.SelectSingleBound(this, null, AssetID);

		}

		protected virtual void _(Events.FieldVerifying<DisposeParams.disposalPeriodID> e)
		{
			string newPeriodID = (string)e.NewValue;
			
			if (newPeriodID == null)
			{
				int? organizationID = PXAccess.GetParentOrganizationID(CurrentAsset.Current.BranchID);
				DisposeParams disposParams = e.Row as DisposeParams;
				newPeriodID = FinPeriodRepository.FindFinPeriodByDate(disposParams.DisposalDate, organizationID)?.FinPeriodID;
			}

			if (newPeriodID == null)
			{
				throw new PXSetPropertyException(Messages.NoPeriodsDefined);
			}

			string lastAdditionPeriodID = GetMostRecentTransactionPeriodID(CurrentAsset.Current.AssetID);
			if (String.CompareOrdinal(newPeriodID, lastAdditionPeriodID) < 0)
			{
				e.NewValue = FinPeriodIDAttribute.FormatForDisplay(newPeriodID);
				throw new PXSetPropertyException(Messages.DisposalPeriodCannotBeEarlierThanPeriodOfMostRecentTransaction,
								FinPeriodIDFormattingAttribute.FormatForError(newPeriodID),
								FinPeriodIDFormattingAttribute.FormatForError(lastAdditionPeriodID));
			}
		}
		protected virtual void _(Events.FieldDefaulting<DisposeParams.disposalDate> e)
		{
			DisposeParams disposParams = e.Row as DisposeParams;
			if (disposParams == null) return;

			if (disposParams.DisposalPeriodID == null)
			{
				e.NewValue = Accessinfo.BusinessDate;
			}
			else
			{
				FATran lastTran = GetMostRecentTransaction(Asset.Current?.AssetID);
				e.NewValue = Accessinfo.BusinessDate > lastTran?.TranDate ? Accessinfo.BusinessDate : lastTran?.TranDate;
			}
		}

		protected virtual void DisposeParams_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			DisposeParams param = (DisposeParams)e.Row;
			if (param == null) return;

			param.DisposalAccountID = null;
			param.DisposalSubID = null;
		}

		protected virtual void _(Events.RowSelected<DisposeParams> e)
		{
			if (e.Row == null) return;

			FABookBalance postingBalance = SelectFrom<FABookBalance>
				.Where<FABookBalance.assetID.IsEqual<FixedAsset.assetID.FromCurrent.NoDefault>
					.And<FABookBalance.updateGL.IsEqual<True>>>
				.View
				.Select(this);

			if(postingBalance != null
				&& string.CompareOrdinal(postingBalance.CurrDeprPeriod, e.Row.DisposalPeriodID) < 0
				&& e.Row.ActionBeforeDisposal == DisposeParams.actionBeforeDisposal.Suspend)
			{
				PXUIFieldAttribute.SetWarning<DisposeParams.actionBeforeDisposal>(
					e.Cache, 
					e.Row, 
					PXMessages.LocalizeFormatNoPrefix(
						postingBalance.LastDeprPeriod == null
							? Messages.NonDeprFixedAssetWiilBeSuspended
							: Messages.FixedAssetWiilBeSuspended, 
						FinPeriodIDFormattingAttribute.FormatForError(postingBalance.LastDeprPeriod ?? postingBalance.DeprFromPeriod), 
						FinPeriodIDFormattingAttribute.FormatForError(e.Row.DisposalPeriodID)));
			}
			else
			{
				PXUIFieldAttribute.SetWarning<DisposeParams.actionBeforeDisposal>(e.Cache, e.Row, null);
			}

			FATran lastTran = GetMostRecentTransaction(Asset.Current?.AssetID);
			Dictionary<int, FABook> books = PXDatabase.GetSlot<FABookCollection>(nameof(FABookCollection), typeof(FABook)).Books;
			if (lastTran != null && e.Row.DisposalDate != null && books[(int)lastTran.BookID].UpdateGL != true &&
				(e.Row.DisposalDate < lastTran.TranDate))
			{
				e.Cache.RaiseExceptionHandling<DisposeParams.disposalDate>(e.Row, e.Row.DisposalDate,
					new PXSetPropertyException(Messages.DisposalDateMustBeInDisposalPeriod,
						((DateTime)lastTran.TranDate).ToShortDateString()));
			}
			else
			{
				e.Cache.RaiseExceptionHandling<DisposeParams.disposalDate>(e.Row, e.Row.DisposalDate, null);
			}
		}
		#region Overrides

		#endregion

		protected virtual bool WasSuspended(int? assetID)
		{
			FABookHistory history = SelectFrom<FABookHistory>
				.Where<FABookHistory.closed.IsEqual<True>
					.And<FABookHistory.suspended.IsEqual<True>>
					.And<FABookHistory.assetID.IsEqual<@P.AsInt>>>
				.View
				.ReadOnly
				.SelectSingleBound(this, null, assetID);

			return history != null;
		}

		private static bool IsDepreciated(FATran tran)
		{
			return tran != null && 
				(tran.Origin == FARegister.origin.Depreciation ||
				tran.TranType == FATran.tranType.DepreciationPlus ||
				tran.TranType == FATran.tranType.DepreciationMinus ||
				tran.TranType == FATran.tranType.CalculatedPlus ||
				tran.TranType == FATran.tranType.CalculatedMinus);
		}

		private static bool IsPurchased(FATran tran)
		{
			return tran != null && 
				(tran.Origin == FARegister.origin.Purchasing ||
				tran.Origin == FARegister.origin.Reconcilliation) &&
				tran.Released == true;
		}

		protected virtual bool IsPureMigrated(FABookBalance bookBalance)
		{
			bool isPureMigrated = false;
			if (bookBalance.UpdateGL == true)
			{
				List<FATran> unpostedTransactions = SelectFrom<FATran>
					.Where<FATran.assetID.IsEqual<@P.AsInt>
						.And<FATran.bookID.IsEqual<@P.AsInt>>
						.And<FATran.released.IsEqual<True>>
						.And<FATran.batchNbr.IsNull>
						.And<FATran.tranType.IsEqual<FATran.tranType.purchasingPlus>
							.Or<FATran.tranType.IsEqual<FATran.tranType.reconcilliationPlus>>>>
					.View
				.Select(this, bookBalance.AssetID, bookBalance.BookID)
				.RowCast<FATran>()
				.ToList();

				bool isMigrated = unpostedTransactions.Count == 2 
					&& new HashSet<(string, decimal?)>(unpostedTransactions.Select(transaction => (transaction.RefNbr, transaction.TranAmt))).Count == 1;

				if(isMigrated)
				{
					FATran regularTransaction = SelectFrom<FATran>
						.Where<FATran.assetID.IsEqual<@P.AsInt>
							.And<FATran.bookID.IsEqual<@P.AsInt>>
							.And<FATran.released.IsEqual<True>>
							.And<FATran.batchNbr.IsNotNull>>
						.View
						.SelectSingleBound(this, null, bookBalance.AssetID, bookBalance.BookID);
					isPureMigrated = regularTransaction == null;
				}
			}
			else
			{
				List<FATran> unpostedTransactions = SelectFrom<FATran>
					.Where<FATran.assetID.IsEqual<@P.AsInt>
						.And<FATran.bookID.IsEqual<@P.AsInt>>
						.And<FATran.released.IsEqual<True>>
						.And<FATran.batchNbr.IsNull>>
					.View
				.Select(this, bookBalance.AssetID, bookBalance.BookID)
				.RowCast<FATran>()
				.ToList();

				if((unpostedTransactions.Count == 2 && bookBalance.YtdDepreciated == 0
						|| unpostedTransactions.Count == 3 && bookBalance.YtdDepreciated != 0)
					&& new HashSet<string>(unpostedTransactions.Select(transaction => transaction.RefNbr)).Count == 1)
				{
					List<FATran> purchasingTransactions = unpostedTransactions
						.Where(transaction => transaction.TranType == FATran.tranType.PurchasingPlus)
						.RowCast<FATran>()
						.ToList();

					List<FATran> reconciliationTransactions = unpostedTransactions
						.Where(transaction => transaction.TranType == FATran.tranType.ReconciliationPlus)
						.RowCast<FATran>()
						.ToList();

					List<FATran> depreciationTransactions = unpostedTransactions
						.Where(transaction => transaction.TranType == FATran.tranType.DepreciationPlus)
						.RowCast<FATran>()
						.ToList();

					FATran purchasingTransaction = purchasingTransactions.FirstOrDefault();
					FATran reconciliationTransaction = reconciliationTransactions.FirstOrDefault();
					FATran depreciationTransaction = depreciationTransactions.FirstOrDefault();

					isPureMigrated = purchasingTransactions.Count == 1
						&& reconciliationTransactions.Count == 1
						&& purchasingTransaction.TranAmt == reconciliationTransaction.TranAmt
						&& (depreciationTransactions.IsEmpty()
							|| depreciationTransactions.Count == 1
								&& depreciationTransaction.TranAmt == bookBalance.YtdDepreciated);
				}
			}

			if(isPureMigrated)
			{
				FATran unreleasedTran = SelectFrom<FATran>
					.Where<FATran.assetID.IsEqual<@P.AsInt>
						.And<FATran.bookID.IsEqual<@P.AsInt>>
						.And<FATran.released.IsNotEqual<True>>>
					.View
					.SelectSingleBound(this, new object[] { }, bookBalance.AssetID, bookBalance.BookID);

				isPureMigrated = unreleasedTran == null;
			}

			return isPureMigrated;
		}

		private string GetTransferPeriod(int? assetID)
		{
			FABookBalanceTransfer trbal =
						PXSelect<FABookBalanceTransfer,
						Where<FABookBalanceTransfer.assetID, Equal<Required<FixedAsset.assetID>>>,
						OrderBy<Desc<FABookBalanceTransfer.updateGL>>>
						.SelectSingleBound(this, null, assetID);

			return trbal?.TransferPeriodID;
		}

		protected virtual int? GetDeprMethodOfAssetType(int? depreciationMethodID, FABookBalance balance) => GetDeprMethodOfAssetType(this, depreciationMethodID, balance);

		public static int? GetDeprMethodOfAssetType(PXGraph graph, int? depreciationMethodID, FABookBalance balance)
		{
			if(depreciationMethodID == null)
			{
				return null;
			}

			FADepreciationMethod classDM = SelectFrom<FADepreciationMethod>.Where<FADepreciationMethod.methodID.IsEqual<@P.AsInt>>.View.Select(graph, depreciationMethodID);

			if (classDM.RecordType != FARecordType.ClassType)
			{
				return depreciationMethodID;
			}
			else
			{
				FADepreciationMethod suitableDepreciationMethod = GetSuitableDepreciationMethod(graph, classDM, balance.DeprFromDate, balance.BookID, balance.AssetID);
				return suitableDepreciationMethod.MethodID;
			}
		}

		public static FADepreciationMethod GetSuitableDepreciationMethod(PXGraph graph, FADepreciationMethod classMethod, DateTime? deprDate, int? bookID, int? assetID)
		{
			int? num = null;
			if (classMethod.AveragingConvention == FAAveragingConvention.HalfQuarter)
			{
				num = graph.GetService<IFABookPeriodRepository>().GetQuarterNumberOfDate(deprDate, bookID, assetID);
			}
			if (classMethod.AveragingConvention == FAAveragingConvention.HalfPeriod)
			{
				num = graph.GetService<IFABookPeriodRepository>().GetPeriodNumberOfDate(deprDate, bookID, assetID);
			}

			FADepreciationMethod suitableDepreciationMethod =
				SelectFrom<FADepreciationMethod>
					.Where<FADepreciationMethod.parentMethodID.IsEqual<@P.AsInt>.
						And<FADepreciationMethod.averagingConvPeriod.IsEqual<@P.AsInt>>>.View.Select(graph, classMethod.MethodID, num);

			return suitableDepreciationMethod;
		}
	}

	

	#region DAC Definitions

	#region FABookBalance2
	[PXProjection(typeof(Select2<
		FABookBalance,
		InnerJoin<FixedAsset, 
			On<FABookBalance.assetID, Equal<FixedAsset.assetID>>,
		InnerJoin<Branch, 
			On<FixedAsset.branchID, Equal<Branch.branchID>>,
		InnerJoin<FABook,
			On<FABookBalance.bookID, Equal<FABook.bookID>>,
		LeftJoin<FABookPeriod, 
			On<FABookPeriod.bookID, Equal<FABookBalance.bookID>, 
			And<FABookPeriod.organizationID, Equal<IIf<Where<FABook.updateGL, Equal<True>>, Branch.organizationID, FinPeriod.organizationID.masterValue>>,
			And<FABookPeriod.finPeriodID, Equal<FABookBalance.deprFromPeriod>>>>,
		LeftJoin<FABookPeriod2, 
			On<FABookPeriod2.bookID, Equal<FABookBalance.bookID>, 
			And<FABookPeriod2.organizationID, Equal<IIf<Where<FABook.updateGL, Equal<True>>, Branch.organizationID, FinPeriod.organizationID.masterValue>>,
			And<FABookPeriod2.finPeriodID, Equal<FABookBalance.deprToPeriod>>>>>>>>>,
		Where<FixedAsset.depreciable, Equal<True>, And<FixedAsset.underConstruction, NotEqual<True>>>>))]
	[Serializable]
	[PXHidden]
	public class FABookBalance2 : IBqlTable
	{
		#region ExactLife
		public abstract class exactLife : PX.Data.BQL.BqlInt.Field<exactLife> { }
		[PXDBCalced(typeof(Sub<Add<int1, FABookPeriod2.finYear>, FABookPeriod.finYear>), typeof(int))]
		public virtual int? ExactLife
		{
			get;
			set;
		}
		#endregion
		#region FABookPeriod2
		[Serializable]
		[PXHidden]
		public class FABookPeriod2 : FABookPeriod
		{
			public new abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
			public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
			public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			public new abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		}
		#endregion
	}
	#endregion

	#region Dynamic Representation of Depreciation (Periods Side by Side)
	[Serializable]
	[PXCacheName(Messages.FAHistory)]
	public partial class FAHistory : IBqlTable
	{
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		protected Int32? _AssetID;
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual Int32? AssetID
		{
			get
			{
				return _AssetID;
			}
			set
			{
				_AssetID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FABookPeriodID]
		[PXUIField(DisplayName = "Period")]
		public virtual String FinPeriodID
		{
			get
			{
				return _FinPeriodID;
			}
			set
			{
				_FinPeriodID = value;
			}
		}
		#endregion
		#region PtdDepreciated
		public Decimal?[] PtdDepreciated;
		#endregion
	}
	#endregion

	#region Dynamic Representation of Depreciation (Book Sheet)
	[Serializable]
	public partial class FASheetHistory : IBqlTable
	{
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		protected Int32? _AssetID;
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual Int32? AssetID
		{
			get
			{
				return _AssetID;
			}
			set
			{
				_AssetID = value;
			}
		}
		#endregion
		#region PeriodNbr
		public abstract class periodNbr : PX.Data.BQL.BqlString.Field<periodNbr> { }
		protected string _PeriodNbr;
		[PXString(2, IsFixed = true, IsKey = true)]
		public virtual string PeriodNbr
		{
			get
			{
				return _PeriodNbr;
			}
			set
			{
				_PeriodNbr = value;
			}
		}
		#endregion
		#region PtdDepreciated
		public class PeriodDeprPair
		{
			private readonly string _PeriodID;
			private readonly decimal? _PtdDepreciated;
			private readonly decimal? _PtdCalculated;
			public PeriodDeprPair(string periodID, decimal? depr, decimal? calc)
			{
				_PeriodID = periodID;
				_PtdDepreciated = depr;
				_PtdCalculated = calc;
			}

			public string PeriodID
			{
				get { return _PeriodID; }
			}
			public decimal? PtdDepreciated
			{
				get { return _PtdDepreciated; }
			}
			public decimal? PtdCalculated
			{
				get { return _PtdCalculated; }
			}
		}

		public PeriodDeprPair[] PtdValues;

		#endregion
	}
	#endregion

	#region FATransactions History Filter
	[Serializable]
	public partial class TranBookFilter : IBqlTable
	{
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected Int32? _BookID;
		[PXDBInt]
		[PXSelector(typeof(Search2<FABook.bookID,
								InnerJoin<FABookBalance, On<FABookBalance.bookID, Equal<FABook.bookID>>>,
								Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>>),
					SubstituteKey = typeof(FABook.bookCode),
					DescriptionField = typeof(FABook.description))]
		[PXUIField(DisplayName = "Book")]
		public virtual Int32? BookID
		{
			get
			{
				return _BookID;
			}
			set
			{
				_BookID = value;
			}
		}
		#endregion
	}

	[Serializable]
	public partial class DeprBookFilter : IBqlTable
	{
		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		protected Int32? _BookID;
		[PXDBInt]
		[PXSelector(typeof(Search2<FABook.bookID,
								InnerJoin<FABookBalance, On<FABookBalance.bookID, Equal<FABook.bookID>>>,
								Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>>),
					SubstituteKey = typeof(FABook.bookCode),
					DescriptionField = typeof(FABook.description))]
		[PXDefault(typeof(Search2<FABookBalance.bookID, LeftJoin<FixedAsset, On<FABookBalance.assetID, Equal<FixedAsset.assetID>>>, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>, 
				And<Current<FixedAsset.depreciable>, Equal<True>, And<Current<FixedAsset.underConstruction>, NotEqual<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Book")]
		public Int32? BookID
		{
			get
			{
				return _BookID;
			}
			set
			{
				_BookID = value;
			}
		}
		#endregion
	}
	#endregion

	#region Parameters for Dispose
	[Serializable]
	[PXCacheName(Messages.DisposeParams)]
	public partial class DisposeParams : IBqlTable
	{
		#region DisposalDate
		public abstract class disposalDate : PX.Data.BQL.BqlDateTime.Field<disposalDate> { }
		protected DateTime? _DisposalDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Disposal Date")]
		public virtual DateTime? DisposalDate
		{
			get
			{
				return _DisposalDate;
			}
			set
			{
				_DisposalDate = value;
			}
		}
		#endregion
		#region DisposalPeriodID
		public abstract class disposalPeriodID : PX.Data.BQL.BqlString.Field<disposalPeriodID> { }
		protected string _DisposalPeriodID;
		[PXUIField(DisplayName = "Disposal Period")]
		[FABookPeriodSelector(
			selectorSearchType: typeof(Search2<
				FABookPeriod.finPeriodID,
				InnerJoin<FABook, On<FABookPeriod.bookID, Equal<FABook.bookID>>,
				LeftJoin<FABookBalance,
					On<FABookBalance.updateGL, Equal<True>,
						And<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>,
						And<FABookPeriod.bookID, Equal<FABookBalance.bookID>>>>,
				LeftJoin<FinPeriod,
					On<FABookPeriod.organizationID, Equal<FinPeriod.organizationID>,
						And<FABookPeriod.finPeriodID, Equal<FinPeriod.finPeriodID>,
						And<FABook.updateGL, Equal<True>>>>>>>,
				Where<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>,
					And2<Where<FinPeriod.finPeriodID, GreaterEqual<FABookBalance.lastDeprPeriod>, 
						Or<FABookBalance.lastDeprPeriod, IsNull>>,
					And<Where<FinPeriod.finPeriodID, IsNotNull,
					And<FinPeriod.fAClosed, NotEqual<True>,
						Or<FABook.updateGL, NotEqual<True>>>>>>>>),
			dateType: typeof(DisposeParams.disposalDate),
			branchSourceType: typeof(FixedAsset.branchID),
			isBookRequired: false)]
		public virtual string DisposalPeriodID
		{
			get
			{
				return _DisposalPeriodID;
			}
			set
			{
				_DisposalPeriodID = value;
			}
		}
		#endregion
		#region DisposalAmt
		public abstract class disposalAmt : PX.Data.BQL.BqlDecimal.Field<disposalAmt> { }
		protected Decimal? _DisposalAmt;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Proceeds Amount")]
		public virtual Decimal? DisposalAmt
		{
			get
			{
				return _DisposalAmt;
			}
			set
			{
				_DisposalAmt = value;
			}
		}
		#endregion
		#region DisposalMethodID
		public abstract class disposalMethodID : PX.Data.BQL.BqlInt.Field<disposalMethodID> { }
		protected Int32? _DisposalMethodID;
		[PXDBInt()]
		[PXDefault()]
		[PXSelector(typeof(Search<FADisposalMethod.disposalMethodID>),
					SubstituteKey = typeof(FADisposalMethod.disposalMethodCD),
					DescriptionField = typeof(FADisposalMethod.description))]
		[PXUIField(DisplayName = "Disposal Method", Required = true)]
		public virtual Int32? DisposalMethodID
		{
			get
			{
				return _DisposalMethodID;
			}
			set
			{
				_DisposalMethodID = value;
			}
		}
		#endregion
		#region DisposalAccountID
		public abstract class disposalAccountID : PX.Data.BQL.BqlInt.Field<disposalAccountID> { }
		protected Int32? _DisposalAccountID;
		[PXDefault(typeof(Coalesce<Search<FixedAsset.disposalAccountID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>>>, Coalesce<Search<FADisposalMethod.proceedsAcctID, Where<FADisposalMethod.disposalMethodID, Equal<Current<DisposeParams.disposalMethodID>>>>, Search<FASetup.proceedsAcctID>>>))]
		[Account(null, typeof(Search2<Account.accountID,
			LeftJoin<CA.CashAccount,
				On<CA.CashAccount.accountID, Equal<Account.accountID>>>,
			Where<Match<Current<AccessInfo.userName>>>>), DisplayName = "Proceeds Account", DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXRestrictor(typeof(Where<CA.CashAccount.cashAccountID, IsNull,
				Or<CA.CashAccount.branchID, Equal<Current<FixedAsset.branchID>>>>), Messages.AccountLinkToCashAccountHasDifferentBranch, typeof(Account.accountCD))]
		public virtual Int32? DisposalAccountID
		{
			get
			{
				return _DisposalAccountID;
			}
			set
			{
				_DisposalAccountID = value;
			}
		}
		#endregion
		#region DisposalSubID
		public abstract class disposalSubID : PX.Data.BQL.BqlInt.Field<disposalSubID> { }
		protected Int32? _DisposalSubID;
		[PXDefault(typeof(Coalesce<Search<FixedAsset.disposalSubID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>>>, Coalesce<Search<FADisposalMethod.proceedsSubID, Where<FADisposalMethod.disposalMethodID, Equal<Current<DisposeParams.disposalMethodID>>>>, Search<FASetup.proceedsSubID>>>))]
		[SubAccount(typeof(DisposeParams.disposalAccountID), typeof(FixedAsset.branchID), DisplayName = "Proceeds Sub.", DescriptionField = typeof(Sub.description))]
		public virtual Int32? DisposalSubID
		{
			get
			{
				return _DisposalSubID;
			}
			set
			{
				_DisposalSubID = value;
			}
		}
		#endregion
		#region ActionBeforeDisposal
		public abstract class actionBeforeDisposal : BqlString.Field<actionBeforeDisposal>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Depreciate, Suspend },
					new string[] { Messages.Depreciate, Messages.Suspend})
				{ }
			}

			public const string Depreciate = "D";
			public const string Suspend = "S";

			public class depreciate : BqlString.Constant<depreciate>
			{
				public depreciate() : base(Depreciate) { }
			}
			public class suspend : BqlString.Constant<suspend>
			{
				public suspend() : base(Suspend) { }
			}
		}

		[PXDBString]
		[PXDefault(actionBeforeDisposal.Suspend)]
		[PXUIField(DisplayName = "Before Disposal")]
		[actionBeforeDisposal.List]
		public virtual string ActionBeforeDisposal
		{
			get;
			set;
		}
		#endregion
		#region Reason
		public abstract class reason : PX.Data.BQL.BqlString.Field<reason> { }
		protected String _Reason;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Reason")]
		public virtual String Reason
		{
			get
			{
				return this._Reason;
			}
			set
			{
				this._Reason = value;
			}
		}
		#endregion

	}
	#endregion

	#region Info for Reverse Disposal
	[Serializable]
	[PXCacheName(Messages.ReverseDisposalInfo)]
	public partial class ReverseDisposalInfo : DisposeParams
	{
		#region DisposalDate
		[PXDBDate]
		[PXDefault(typeof(FADetails.disposalDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Disposal Date", Enabled = false)]
		public override DateTime? DisposalDate
		{
			get
			{
				return _DisposalDate;
			}
			set
			{
				_DisposalDate = value;
			}
		}
		#endregion
		#region DisposalPeriodID
		[PXUIField(DisplayName = "Disposal Period", Enabled = false)]
		[PXDBString(6, IsFixed = true)]
		[FinPeriodIDFormatting]
		[PXDefault(typeof(FADetails.disposalPeriodID), PersistingCheck = PXPersistingCheck.Nothing)]
		public override string DisposalPeriodID
		{
			get
			{
				return _DisposalPeriodID;
			}
			set
			{
				_DisposalPeriodID = value;
			}
		}
		#endregion
		#region DisposalAmt
		[PXDBBaseCury]
		[PXDefault(typeof(FADetails.saleAmount), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Proceeds Amount", Enabled = false)]
		public override Decimal? DisposalAmt
		{
			get
			{
				return _DisposalAmt;
			}
			set
			{
				_DisposalAmt = value;
			}
		}
		#endregion
		#region DisposalMethodID
		[PXDBInt]
		[PXDefault(typeof(FADetails.disposalMethodID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<FADisposalMethod.disposalMethodID>),
					SubstituteKey = typeof(FADisposalMethod.disposalMethodCD),
					DescriptionField = typeof(FADisposalMethod.description))]
		[PXUIField(DisplayName = "Disposal Method", Enabled = false)]
		public override Int32? DisposalMethodID
		{
			get
			{
				return _DisposalMethodID;
			}
			set
			{
				_DisposalMethodID = value;
			}
		}
		#endregion
		#region DisposalAccountID
		[PXDefault(typeof(FixedAsset.disposalAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Proceeds Account", DescriptionField = typeof(Account.description), Enabled = false)]
		public override Int32? DisposalAccountID
		{
			get
			{
				return _DisposalAccountID;
			}
			set
			{
				_DisposalAccountID = value;
			}
		}
		#endregion
		#region DisposalSubID
		[PXDefault(typeof(FixedAsset.disposalSubID), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(FixedAsset.disposalAccountID), DisplayName = "Proceeds Sub.", DescriptionField = typeof(Sub.description), Enabled = false)]
		public override Int32? DisposalSubID
		{
			get
			{
				return _DisposalSubID;
			}
			set
			{
				_DisposalSubID = value;
			}
		}
		#endregion
		#region ReverseDisposalDate
		public abstract class reverseDisposalDate : PX.Data.BQL.BqlDateTime.Field<reverseDisposalDate> { }
		protected DateTime? _ReverseDisposalDate;
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Reversal Date")]
		public virtual DateTime? ReverseDisposalDate
		{
			get
			{
				return _ReverseDisposalDate;
			}
			set
			{
				_ReverseDisposalDate = value;
			}
		}
		#endregion
		#region ReverseDisposalPeriodID
		public abstract class reverseDisposalPeriodID : PX.Data.BQL.BqlString.Field<reverseDisposalPeriodID> { }
		protected string _ReverseDisposalPeriodID;
		[PXUIField(DisplayName = "Reversal Period", Required = true)]
		[FABookPeriodSelector(
			selectorSearchType: typeof(Search2<
				FABookPeriod.finPeriodID,
				InnerJoin<FABook, On<FABookPeriod.bookID, Equal<FABook.bookID>>,
				LeftJoin<FinPeriod,
					On<FABookPeriod.organizationID, Equal<FinPeriod.organizationID>,
						And<FABookPeriod.finPeriodID, Equal<FinPeriod.finPeriodID>,
						And<FABook.updateGL, Equal<True>>>>>>,
				Where<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>,
					And<FinPeriod.finPeriodID, GreaterEqual<Current<FADetails.disposalPeriodID>>,
					And<Where<FinPeriod.finPeriodID, IsNotNull,
					And<FinPeriod.fAClosed, NotEqual<True>,
						Or<FABook.updateGL, NotEqual<True>>>>>>>>),
			dateType: typeof(ReverseDisposalInfo.reverseDisposalDate),
			assetSourceType: typeof(FixedAsset.assetID),
			isBookRequired: false)]
		[PXDefault]
		public virtual string ReverseDisposalPeriodID
		{
			get
			{
				return _ReverseDisposalPeriodID;
			}
			set
			{
				_ReverseDisposalPeriodID = value;
			}
		}
		#endregion
	}
	#endregion

	#region Parameters for Suspend
	public class FASuspendPeriodSelectorAttribute : FABookPeriodSelectorAttribute
	{
		public FASuspendPeriodSelectorAttribute()
			:base(
			selectorSearchType: typeof(Search2<
				FABookPeriod.finPeriodID,
				InnerJoin<FABookBalance,
					On<FABookBalance.bookID.IsEqual<FABookPeriod.bookID>
						.And<FABookBalance.assetID.IsEqual<FixedAsset.assetID.FromCurrent>>>>,
				Where<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>,
					And<FABookPeriod.finPeriodID, GreaterEqual<IsNull<FABookBalance.currDeprPeriod, IsNull<FABookBalance.lastDeprPeriod, FABookBalance.deprFromPeriod>>>>>>),
			branchSourceType: typeof(FixedAsset.branchID),
			isBookRequired: false)
		{ }

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			// AC-177922 Unfortunately, newly inserted FABookBalance objects (AssetID < 0) can not be joined by BQL
			if (!(sender.Graph.IsImport && ((sender.Graph.Caches<FixedAsset>().Current as FixedAsset)?.AssetID ?? int.MinValue) < 0)) // AC-
			{
				base.FieldVerifying(sender, e);
			}
		}
	}

	[Serializable]
	public partial class SuspendParams : IBqlTable
	{
		#region CurrentPeriodID
		public abstract class currentPeriodID : PX.Data.BQL.BqlString.Field<currentPeriodID> { }
		protected string _CurrentPeriodID;
		[PXUIField(DisplayName = "Current Period")]
		[FASuspendPeriodSelector]
		[PXDefault(typeof(Search2<
			FABookPeriod.finPeriodID,
			InnerJoin<FABookBalance, 
				On<FABookPeriod.bookID, Equal<FABookBalance.bookID>>,
			InnerJoin<FABook, On<FABookBalance.bookID, Equal<FABook.bookID>>,
			LeftJoin<FinPeriod,
				On<FABook.updateGL, Equal<True>, 
					And<FABookPeriod.finPeriodID, Equal<FinPeriod.finPeriodID>,
					And<FABookPeriod.organizationID, Equal<FinPeriod.organizationID>>>>,
			InnerJoin<FixedAsset, On<FABookBalance.assetID, Equal<FixedAsset.assetID>>,
			InnerJoin<Branch, On<FixedAsset.branchID, Equal<Branch.branchID>>>>>>>,
			Where2<Where<FinPeriod.fAClosed, Equal<False>, Or<FinPeriod.fAClosed, IsNull>>,
				And<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>,
				And<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>,
				And<FABookPeriod.organizationID, Equal<IIf<
					Where<FABook.updateGL, Equal<True>, 
						And<Not<FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>>>>, 
					Branch.organizationID, 
					FinPeriod.organizationID.masterValue>>,
				And<FABookPeriod.finPeriodID, GreaterEqual<IsNull<FABookBalance.currDeprPeriod, IsNull<FABookBalance.lastDeprPeriod, FABookBalance.deprFromPeriod>>>>>>>>,
			OrderBy<
				Desc<FABookBalance.updateGL, 
				Asc<FABookPeriod.finPeriodID>>>>))]
		public virtual string CurrentPeriodID
		{
			get
			{
				return _CurrentPeriodID;
			}
			set
			{
				_CurrentPeriodID = value;
			}
		}
		#endregion
	}
	#endregion

	#region Parameters for Split
	[Serializable]
	public partial class SplitParams : FixedAsset
	{
		#region SplitID
		public abstract class splitID : PX.Data.BQL.BqlInt.Field<splitID> { }
		protected int? _SplitID;
		[PXInt(IsKey = true)]
		[PXLineNbr(typeof(SplitProcess.SplitFilter))]
		public virtual int? SplitID
		{
			get
			{
				return _SplitID;
			}
			set
			{
				_SplitID = value;
			}
		}
		#endregion
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Asset ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		public override string AssetCD { get; set; }

		#region SplittedAssetCD
		public abstract class splittedAssetCD : PX.Data.BQL.BqlString.Field<splittedAssetCD> { }
		protected String _SplittedAssetCD;
		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Asset ID")]
		public virtual string SplittedAssetCD
		{
			get
			{
				return _SplittedAssetCD;
			}
			set
			{
				_SplittedAssetCD = value;
			}
		}
		#endregion
		#region Cost
		public abstract class cost : PX.Data.BQL.BqlDecimal.Field<cost> { }
		protected decimal? _Cost;
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cost")]
		public virtual decimal? Cost
		{
			get
			{
				return _Cost;
			}
			set
			{
				_Cost = value;
			}
		}
		#endregion
		#region SplittedQty
		public abstract class splittedQty : PX.Data.BQL.BqlDecimal.Field<splittedQty> { }
		protected Decimal? _SplittedQty;
		[PXQuantity(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? SplittedQty
		{
			get
			{
				return _SplittedQty;
			}
			set
			{
				_SplittedQty = value;
			}
		}
		#endregion
		#region Ratio
		public abstract class ratio : PX.Data.BQL.BqlDecimal.Field<ratio> { }
		protected decimal? _Ratio;
		[PXDecimal(18, MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ratio")]
		public virtual decimal? Ratio
		{
			get
			{
				return this._Ratio;
			}
			set
			{
				this._Ratio = value;
			}
		}
		#endregion
	}
	#endregion


	[Serializable]
	public partial class DsplFAATran : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlInt.Field<tranID> { }
		protected int? _TranID;
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? TranID
		{
			get
			{
				return _TranID;
			}
			set
			{
				_TranID = value;
			}
		}
		#endregion
		#region GLTranQty
		public abstract class gLTranQty : PX.Data.BQL.BqlDecimal.Field<gLTranQty> { }
		protected Decimal? _GLTranQty;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Orig. Quantity", Enabled = false)]
		public virtual Decimal? GLTranQty
		{
			get
			{
				return _GLTranQty;
			}
			set
			{
				_GLTranQty = value;
			}
		}
		#endregion
		#region GLTranAmt
		public abstract class gLTranAmt : PX.Data.BQL.BqlDecimal.Field<gLTranAmt> { }
		protected Decimal? _GLTranAmt;
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Orig. Amount", Enabled = false)]
		public virtual Decimal? GLTranAmt
		{
			get
			{
				return _GLTranAmt;
			}
			set
			{
				_GLTranAmt = value;
			}
		}
		#endregion
		#region SelectedQty
		public abstract class selectedQty : PX.Data.BQL.BqlDecimal.Field<selectedQty> { }
		protected Decimal? _SelectedQty;
		[PXDBQuantity]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Selected Quantity")]
		public virtual Decimal? SelectedQty
		{
			get
			{
				return _SelectedQty;
			}
			set
			{
				_SelectedQty = value;
			}
		}
		#endregion
		#region SelectedAmt
		public abstract class selectedAmt : PX.Data.BQL.BqlDecimal.Field<selectedAmt> { }
		protected Decimal? _SelectedAmt;
		[PXDBBaseCury]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Selected Amount")]
		public virtual Decimal? SelectedAmt
		{
			get
			{
				return _SelectedAmt;
			}
			set
			{
				_SelectedAmt = value;
			}
		}
		#endregion
		#region OpenQty
		public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
		protected Decimal? _OpenQty;
		[PXQuantity]
		[PXFormula(typeof(Sub<gLTranQty, Add<closedQty, selectedQty>>))]
		[PXUIField(DisplayName = "Open Quantity", Enabled = false)]
		public virtual Decimal? OpenQty
		{
			get
			{
				return _OpenQty;
			}
			set
			{
				_OpenQty = value;
			}
		}
		#endregion
		#region OpenAmt
		public abstract class openAmt : PX.Data.BQL.BqlDecimal.Field<openAmt> { }
		protected Decimal? _OpenAmt;
		[PXBaseCury]
		[PXFormula(typeof(Sub<gLTranAmt, Add<closedAmt, selectedAmt>>))]
		[PXUIField(DisplayName = "Open Amount", Enabled = false)]
		public virtual Decimal? OpenAmt
		{
			get
			{
				return _OpenAmt;
			}
			set
			{
				_OpenAmt = value;
			}
		}
		#endregion
		#region ClosedAmt
		public abstract class closedAmt : PX.Data.BQL.BqlDecimal.Field<closedAmt> { }
		protected Decimal? _ClosedAmt;
		[PXDBBaseCury]
		public virtual Decimal? ClosedAmt
		{
			get
			{
				return _ClosedAmt;
			}
			set
			{
				_ClosedAmt = value;
			}
		}
		#endregion
		#region ClosedQty
		public abstract class closedQty : PX.Data.BQL.BqlDecimal.Field<closedQty> { }
		protected Decimal? _ClosedQty;
		[PXDBQuantity]
		public virtual Decimal? ClosedQty
		{
			get
			{
				return _ClosedQty;
			}
			set
			{
				_ClosedQty = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXDBBaseCury]
		[PXFormula(typeof(Switch<Case<Where<gLTranQty, LessEqual<decimal0>>, gLTranAmt>, Div<gLTranAmt, gLTranQty>>))]
		[PXUIField(DisplayName = "Unit Cost", Enabled = false)]
		public virtual Decimal? UnitCost
		{
			get
			{
				return _UnitCost;
			}
			set
			{
				_UnitCost = value;
			}
		}
		#endregion
		#region Reconciled
		public abstract class reconciled : PX.Data.BQL.BqlBool.Field<reconciled> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Reconciled")]
		[PXDefault(typeof(Search<FAAccrualTran.reconciled, Where<FAAccrualTran.tranID, Equal<Current<tranID>>>>))]
		public bool? Reconciled { get; set; }
		#endregion

		#region Component
		public abstract class component : PX.Data.BQL.BqlBool.Field<component> { }
		protected Boolean? _Component = false;
		[PXBool]
		[PXUIField(DisplayName = "Component")]
		public virtual Boolean? Component
		{
			get
			{
				return _Component;
			}
			set
			{
				_Component = value;
			}
		}
		#endregion
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }
		protected Int32? _ClassID;
		[PXInt]
		[PXSelector(typeof(Search2<FixedAsset.assetID,
			LeftJoin<FABookSettings, On<FixedAsset.assetID, Equal<FABookSettings.assetID>>,
			LeftJoin<FABook, On<FABookSettings.bookID, Equal<FABook.bookID>>>>,
			Where<FixedAsset.recordType, Equal<FARecordType.classType>,
			And<FABook.updateGL, Equal<True>>>>),
					SubstituteKey = typeof(FixedAsset.assetCD),
					DescriptionField = typeof(FixedAsset.description))]
		[PXUIField(DisplayName = "Asset Class")]
		public virtual Int32? ClassID
		{
			get
			{
				return _ClassID;
			}
			set
			{
				_ClassID = value;
			}
		}
		#endregion
	}

	#endregion

	#region Formulas
	#region SelectDepreciationMethod
	public class SelectDepreciationMethod<DeprDate, ClassID, BookID, AssetID> : BqlFormulaEvaluator<DeprDate, ClassID, BookID, AssetID>
		where DeprDate : IBqlOperand
		where ClassID : IBqlOperand
		where BookID : IBqlOperand
		where AssetID : IBqlOperand
	{

		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			DateTime? deprDate = (DateTime?)pars[typeof(DeprDate)];
			int? bookID = (int?)pars[typeof(BookID)];
			int? assetID = (int?)pars[typeof(AssetID)];
			int? classID = (int?)pars[typeof(ClassID)];

			FADepreciationMethod parent = PXSelectJoin<FADepreciationMethod,
								LeftJoin<FABookSettings,
									On<FADepreciationMethod.methodID, Equal<FABookSettings.depreciationMethodID>>>,
								Where<FABookSettings.assetID, Equal<Required<FABookBalance.classID>>,
									And<FABookSettings.bookID, Equal<Required<FABookBalance.bookID>>>>>.Select(cache.Graph, classID, bookID);
			if (parent != null && parent.RecordType == FARecordType.BothType)
			{
				return parent.MethodCD;
			}
			else if (parent != null && parent.RecordType == FARecordType.ClassType && deprDate != null)
			{
				FADepreciationMethod meth = AssetMaint.GetSuitableDepreciationMethod(cache.Graph, parent, deprDate, bookID, assetID);

				if (meth != null)
				{
					return meth.MethodCD;
				}
			}
			return null;
		}
	}
	#endregion
	#region CalcNextMeasurementDate
	public class CalcNextMeasurementDate<LastDate, DateFrom, AssetID> : BqlFormulaEvaluator<LastDate, DateFrom, AssetID>
		where LastDate : IBqlOperand
		where DateFrom : IBqlOperand
		where AssetID : IBqlOperand
	{

		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			DateTime? lastDate = (DateTime?)pars[typeof(LastDate)];
			DateTime? dateFrom = (DateTime?)pars[typeof(DateFrom)];
			int? assetID = (int?)pars[typeof(AssetID)];

			lastDate = lastDate ?? dateFrom;
			if (lastDate == null)
			{
				return null;
			}

			FAUsageSchedule sched = PXSelectJoin<FAUsageSchedule,
				InnerJoin<FixedAsset, On<FixedAsset.usageScheduleID, Equal<FAUsageSchedule.scheduleID>>>,
				Where<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(cache.Graph, assetID);

			if (sched == null)
			{
				return null;
			}

			switch (sched.ReadUsageEveryPeriod)
			{
				case FAUsageSchedule.readUsageEveryPeriod.Day:
					return lastDate.Value.AddDays(sched.ReadUsageEveryValue ?? 0);
				case FAUsageSchedule.readUsageEveryPeriod.Week:
					return lastDate.Value.AddDays((sched.ReadUsageEveryValue ?? 0) * 7);
				case FAUsageSchedule.readUsageEveryPeriod.Month:
					return lastDate.Value.AddMonths(sched.ReadUsageEveryValue ?? 0);
				case FAUsageSchedule.readUsageEveryPeriod.Year:
					return lastDate.Value.AddYears(sched.ReadUsageEveryValue ?? 0);
				default:
					return null;
			}
		}
	}
	#endregion
	#region DepreciatedPartTotalUsage
	public class DepreciatedPartTotalUsage<Value, AssetID> : BqlFormulaEvaluator<Value, AssetID>
		where Value : IBqlOperand
		where AssetID : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			decimal? val = (decimal?)pars[typeof(Value)];
			int? assetID = (int?)pars[typeof(AssetID)];

			FADetails det = PXSelect<FADetails, Where<FADetails.assetID, Equal<Required<FADetails.assetID>>>>.Select(cache.Graph, assetID);
			return det == null ? (object) null : ((val/det.TotalExpectedUsage) ?? 0m);
		}
	}
	#endregion

	public class IsAcquiredAsset<AssetID> : BqlFormulaEvaluator<AssetID>, IBqlOperand
		where AssetID : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			int? assetID = pars[typeof(AssetID)] as int?;
			if (assetID != null)
			{
				FATran released = PXSelect<FATran, Where<FATran.assetID, Equal<Required<FixedAsset.assetID>>, And<FATran.released, Equal<True>>>>.SelectSingleBound(cache.Graph, new object[0], assetID);
				return released != null;
			}
			return null;
		}
	}
	#endregion
}
