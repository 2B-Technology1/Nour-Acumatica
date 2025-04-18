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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.IN;
using PX.Web.UI;

namespace PX.Objects.PM
{
	[Serializable]
	public class RegisterEntry : PXGraph<RegisterEntry, PMRegister>, PXImportAttribute.IPXPrepareItems
	{
		public class MultiCurrency : PMTranMultiCurrencyPM<RegisterEntry>
		{
			/// <summary>
			/// I have no idea what actuall conditions should  be here
			/// </summary>
			/// <returns></returns>
			protected override CurySource CurrentSourceSelect()
			{
				CurySource curySource = base.CurrentSourceSelect();
				curySource.AllowOverrideRate = true;
				return curySource;
			}

			protected override PXSelectBase[] GetChildren() => new PXSelectBase[]
			{
				Base.Transactions,
			};

			protected virtual void _(Events.FieldDefaulting<PMTran, PMTran.tranCuryID> e)
			{
				if (e.Row?.ProjectID != null && PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>() && ProjectDefaultAttribute.IsProject(Base, e.Row.ProjectID, out PMProject project))
				{
					e.NewValue = project.CuryID;
				}
				else
				{
					e.NewValue = Base.Company.Current.BaseCuryID;
				}
			}

			protected virtual void _(Events.FieldUpdating<PMTran, PMTran.tranCuryID> e)
			{
				if (e.NewValue == null) e.NewValue = e.OldValue;
			}

			protected virtual void _(Events.FieldUpdated<PMTran, PMTran.tranCuryID> e)
			{
				if (e.Row == null || !PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>()) return;

				CurrencyInfo projectCuryInfo = GetCurrencyInfo(e.Row.ProjectCuryInfoID);
				if (projectCuryInfo != null && projectCuryInfo.CuryInfoID > 0 && (
						CM.CurrencyCollection.IsBaseCuryInfo(projectCuryInfo)))
				{
					CurrencyInfo newProjectCuryInfo = PXCache<CurrencyInfo>.CreateCopy(projectCuryInfo);
					newProjectCuryInfo.CuryInfoID = null;
					// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Insert a new CuryInfo]
					newProjectCuryInfo = currencyinfo.Cache.Insert(newProjectCuryInfo) as CurrencyInfo;
					e.Row.ProjectCuryInfoID = newProjectCuryInfo.CuryInfoID;

					if (newProjectCuryInfo.CuryRateTypeID == null)
					{
						// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Insert a new CuryInfo]
						currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyRateTypeID>(newProjectCuryInfo);
					}

					// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Insert a new CuryInfo]
					currencyinfo.Cache.SetValueExt<CurrencyInfo.curyID>(newProjectCuryInfo, e.NewValue);
					currencyinfo.Cache.RaiseRowUpdated(newProjectCuryInfo, projectCuryInfo);

					recalculateRowBaseValues(Base.Transactions.Cache, Base.Transactions.Current, TrackedItems[Base.Transactions.Cache.GetItemType()]);

					if (!string.IsNullOrEmpty(PXUIFieldAttribute.GetError<CurrencyInfo.curyID>(currencyinfo.Cache, newProjectCuryInfo)))
						e.Cache.RaiseExceptionHandling<PMTran.tranCuryID>(e.Row, null, GetCurrencyRateError(newProjectCuryInfo));
				}
				else
				{
				currencyinfo.Cache.SetValueExt<CurrencyInfo.curyID>(projectCuryInfo, e.NewValue);
				currencyinfo.Cache.MarkUpdated(projectCuryInfo, assertError: true);
				recalculateRowBaseValues(Base.Transactions.Cache, Base.Transactions.Current, TrackedItems[Base.Transactions.Cache.GetItemType()]);

				if (!string.IsNullOrEmpty(PXUIFieldAttribute.GetError<CurrencyInfo.curyID>(currencyinfo.Cache, projectCuryInfo)))
					e.Cache.RaiseExceptionHandling<PMTran.tranCuryID>(e.Row, null, GetCurrencyRateError(projectCuryInfo));
				}

				CurrencyInfo baseCuryInfo = GetCurrencyInfo(e.Row.BaseCuryInfoID);
				currencyinfo.Cache.SetValueExt<CurrencyInfo.curyID>(baseCuryInfo, e.NewValue);
				currencyinfo.Cache.MarkUpdated(baseCuryInfo, assertError: true);
				if (!string.IsNullOrEmpty(PXUIFieldAttribute.GetError<CurrencyInfo.curyID>(currencyinfo.Cache, baseCuryInfo)))
					e.Cache.RaiseExceptionHandling<PMTran.tranCuryID>(e.Row, null, GetCurrencyRateError(baseCuryInfo));
			}

			protected override void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyEffDate> e)
			{
				if (Base.Transactions.Current != null && (e.Row.CuryInfoID == Base.Transactions.Current.ProjectCuryInfoID || e.Row.CuryInfoID == Base.Transactions.Current.BaseCuryInfoID))
				{
					e.NewValue = Base.Transactions.Current.Date;
					e.Cancel = true;
				}
				//on import from excel there is no way to obtain tran date so it doesnt need to redefault it
				else if (e.Row.CuryEffDate != null)
				{
					e.NewValue = e.Row.CuryEffDate;
					e.Cancel = true;
				}

				base._(e);
			}


			protected override void _(Events.FieldUpdated<CurrencyInfo, CurrencyInfo.curyEffDate> e)
			{
				try
				{
					defaultCurrencyRate(e.Cache, e.Row, true, false);
				}
				catch (PXSetPropertyException)
				{
					e.Cache.RaiseExceptionHandling<CurrencyInfo.curyEffDate>(e.Row, e.Row.CuryEffDate, GetCurrencyRateError(e.Row));
				}
			}

			protected override void _(Events.FieldUpdated<CurrencyInfo, CurrencyInfo.curyID> e)
			{
				if (e.Row == null) return;

				defaultEffectiveDate(e.Cache, e.Row);
				try
				{
					defaultCurrencyRate(e.Cache, e.Row, true, false);
				}
				catch (PXSetPropertyException)
				{
					e.Cache.RaiseExceptionHandling<CurrencyInfo.curyID>(e.Row, e.Row.CuryID, GetCurrencyRateError(e.Row));
				}
				e.Row.CuryPrecision = null;
			}


			public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<PMTran.projectCuryInfoID>>>> ProjectCuryInfo;

			public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<PMTran.baseCuryInfoID>>>> BaseCuryInfo;

			public CurrencyInfo GetCurrencyInfoGetDefault(long? key)
			{
				return base.GetCurrencyInfo(key)
					//TODO: ask someone about Search<>(...) method returning null in extension but returning some "default" entity in the graph
					?? Base.CuryInfo.Search<CurrencyInfo.curyInfoID>(key);
			}

			public void CalcCuryRatesForProject(PXCache cache, PMTran tran)
			{
				PMProject project = Base.Project.Search<PMProject.contractID>(tran.ProjectID);
				bool pmcOn = PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>();
				bool mcpOn = PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();

				if (pmcOn && tran.ProjectCuryInfoID != null)
				{
					CurrencyInfo projectCuryInfo = GetCurrencyInfoGetDefault(tran.ProjectCuryInfoID);
					projectCuryInfo.BaseCuryID = (project.NonProject == true || project.BaseType != CTPRType.Project) ? Base.Company.Current.BaseCuryID : project.CuryID;
					if (project.RateTypeID != null)
					{
						projectCuryInfo.CuryRateTypeID = project.RateTypeID;
					}
					//needed for CuryInfo recalculation with changed BaseCuryID
					projectCuryInfo.CuryEffDate = DateTime.MinValue;
					currencyinfo.Cache.Update(projectCuryInfo);
					projectCuryInfo.CuryEffDate = tran.Date;
					currencyinfo.Cache.Update(projectCuryInfo);
					if (projectCuryInfo.CuryRate == null && !Base.IsCopyPasteContext && !string.IsNullOrEmpty(PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, projectCuryInfo)))
						cache.RaiseExceptionHandling<PMTran.projectID>(tran, null, GetCurrencyRateError(projectCuryInfo));
				}

				if ((pmcOn || mcpOn) && tran.BaseCuryInfoID != null)
				{
					CurrencyInfo baseCuryInfo = GetCurrencyInfoGetDefault(tran.BaseCuryInfoID);
					if (mcpOn)
					{
						baseCuryInfo.BaseCuryID = (project.NonProject == true || project.BaseType != CTPRType.Project) ? Base.Company.Current.BaseCuryID : project.BaseCuryID;
					}
					if (pmcOn && project.RateTypeID != null)
					{
						baseCuryInfo.CuryRateTypeID = project.RateTypeID;
					}
					//needed for CuryInfo recalculation with changed BaseCuryID
					baseCuryInfo.CuryEffDate = DateTime.MinValue;
					currencyinfo.Cache.Update(baseCuryInfo);
					baseCuryInfo.CuryEffDate = tran.Date;
					currencyinfo.Cache.Update(baseCuryInfo);
					if (!Base.IsCopyPasteContext && !string.IsNullOrEmpty(PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, baseCuryInfo)))
						cache.RaiseExceptionHandling<PMTran.projectID>(tran, null, GetCurrencyRateError(baseCuryInfo));
				}
			}

			private PXSetPropertyException GetCurrencyRateError(CurrencyInfo info)
			{
				return new PXSetPropertyException(Messages.CurrencyRateIsNotDefined, PXErrorLevel.Warning,
					info.CuryID, info.BaseCuryID, info.CuryRateTypeID, info.CuryEffDate);
			}

			public virtual void ConfigureCurrencyInfoAfterImport(PMTran tran)
			{
				PMProject project = Base.Project.Search<PMProject.contractID>(tran.ProjectID);

				string tranCuryID = tran.TranCuryID;
				CurrencyInfo projectCuryInfo = currencyinfo.Insert(new CurrencyInfo
				{
					CuryID = tranCuryID,
					BaseCuryID = project.NonProject == true ? Base.Company.Current.BaseCuryID : project.CuryID,
					CuryRateTypeID = project.RateTypeID ?? Base.CMSetup.Current.PMRateTypeDflt,
					CuryEffDate = tran.Date,
				});
				tran.ProjectCuryInfoID = projectCuryInfo.CuryInfoID;

				CurrencyInfo baseCuryInfo = currencyinfo.Insert(new CurrencyInfo
				{
					CuryID = tranCuryID,
					CuryEffDate = tran.Date,
					CuryRateTypeID = project.RateTypeID ?? Base.CMSetup.Current.PMRateTypeDflt,
				});

				tran.BaseCuryInfoID = baseCuryInfo.CuryInfoID;
				tran.TranCuryID = tranCuryID ?? tran.TranCuryID;
			}
		}

		#region DAC Attributes Override

		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<PMTran.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PMUnit(typeof(PMTran.inventoryID))]
		protected virtual void _(Events.CacheAttached<PMTran.uOM> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDimensionSelector("EMPLOYEE",
            typeof(Search<CR.Standalone.EPEmployee.bAccountID>),
            typeof(CR.Standalone.EPEmployee.acctCD),
                typeof(CR.Standalone.EPEmployee.bAccountID),
                typeof(CR.Standalone.EPEmployee.acctCD),
                typeof(CR.Standalone.EPEmployee.acctName),
            typeof(CR.Standalone.EPEmployee.departmentID),
            typeof(CR.Standalone.EPEmployee.status),
			DescriptionField = typeof(BAccountCRM.acctName),
			Filterable = true)]
		protected virtual void _(Events.CacheAttached<PMTran.resourceID> e) { }

        #endregion

        [PXHidden]
		public PXSelect<PMProject> Project;

		[PXHidden]
		public PXSetup<Company> Company;

		[PXHidden]
		public PXSelect<BAccount> dummy;

		[PXHidden]
		public PXSelect<Account> accountDummy;

		public PXSelect<PMRegister, Where<PMRegister.module, Equal<Optional<PMRegister.module>>>> Document;

		[PXImport(typeof(PMRegister))]
		public SelectFrom<PMTran>.
			LeftJoin<PMAccountGroup>.On<
				PMAccountGroup.groupID.IsEqual<PMTran.accountGroupID>>.
			LeftJoin<RegisterReleaseProcess.OffsetPMAccountGroup>.On<
				RegisterReleaseProcess.OffsetPMAccountGroup.groupID.IsEqual<PMTran.offsetAccountGroupID>>.
			Where<
				PMTran.tranType.IsEqual<PMRegister.module.FromCurrent>.
				And<PMTran.refNbr.IsEqual<PMRegister.refNbr.FromCurrent>>.
				And<
					RegisterReleaseProcess.OffsetPMAccountGroup.groupID.IsNull.
					Or<Match<RegisterReleaseProcess.OffsetPMAccountGroup, AccessInfo.userName.FromCurrent>>>.
				And<
					PMAccountGroup.groupID.IsNull.
					Or<Match<PMAccountGroup, AccessInfo.userName.FromCurrent>>>>.View Transactions;

		public PXSelect<CurrencyInfo> CuryInfo;

		public PXSelect<PMAllocationSourceTran, 
			Where<PMAllocationSourceTran.allocationID, Equal<Required<PMAllocationSourceTran.allocationID>>,
			And<PMAllocationSourceTran.tranID, Equal<Required<PMAllocationSourceTran.tranID>>>>> SourceTran;

		public PXSelect<PMAllocationAuditTran> AuditTrans;

		public PXSelect<PMRecurringItemAccum> RecurringItems;

		public PXSelect<PMTaskAllocTotalAccum> AllocationTotals;

		public PXSetupOptional<PMSetup> Setup;
		public PXSetup<EPSetup> epSetup;

		public CM.CMSetupSelect CMSetup;

		public PXSelect<PMTimeActivity> Activities;

		public PXSelect<ContractDetailAcum> ContractDetails;


		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IProjectMultiCurrency MultiCurrencyService { get; set; }

		public RegisterEntry()
        {
            if (PXAccess.FeatureInstalled<CS.FeaturesSet.projectModule>())
            {
				PMSetup setup = PXSelect<PMSetup>.Select(this);
				if ( setup == null)
                throw new PXException(Messages.SetupNotConfigured);
            }
            else
            {
				ARSetup setup = PXSelect<ARSetup>.Select(this);
				if (setup == null)
					throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(ARSetup), typeof(ARSetup).Name);
				AutoNumberAttribute.SetNumberingId<PMRegister.refNbr>(Document.Cache, setup.UsageNumberingID);
            }

			selectBaseRate.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>());
			selectProjectRate.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>());
			curyToggle.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>());

            PXUIFieldAttribute.SetVisible<CR.Standalone.EPEmployee.status>(this.Caches[typeof(CR.Standalone.EPEmployee)], null, false);
        }

		public override void Persist()
		{
			FillDataInMigrationMode();

			base.Persist();
		}

		private void FillDataInMigrationMode()
		{
			if (MigrationMode == true)
			{
				foreach (PMTran transaction in Transactions.Select())
				{
					transaction.OrigModule = Document.Current.Module;
					transaction.OrigTranType = PMOrigDocType.GetOrigDocType(Document.Current.OrigDocType);
				}
			}
		}

		public virtual PMTran InsertTransactionWithManuallyChangedCurrencyInfo(PMTran transaction)
		{
			if (ManualCurrencyInfoCreation)
				return Transactions.Insert(transaction);

			ManualCurrencyInfoCreation = true;

			try
			{
				return Transactions.Insert(transaction);
			}
			finally
			{
				ManualCurrencyInfoCreation = false;
			}
		}

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{		
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items) { }

		/// <summary>
		/// Gets the source for the generated PMTran.AccountID
		/// </summary>
		public string ExpenseAccountSource
        {
            get
            {
                string result = PM.PMExpenseAccountSource.InventoryItem;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccountSource))
                {
                    result = setup.ExpenseAccountSource;
                }

                return result;
            }
        }

        public string ExpenseSubMask
        {
            get
            {
                string result = null;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseSubMask))
                {
                    result = setup.ExpenseSubMask;
                }

                return result;
            }
        }

        public string ExpenseAccrualAccountSource
        {
            get
            {
                string result = PM.PMExpenseAccountSource.InventoryItem;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccrualAccountSource))
                {
                    result = setup.ExpenseAccrualAccountSource;
                }

                return result;
            }
        }

        public string ExpenseAccrualSubMask
        {
            get
            {
                string result = null;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccrualSubMask))
                {
                    result = setup.ExpenseAccrualSubMask;
                }

                return result;
            }
        }

		public bool MigrationMode
		{
			get
			{
				PMSetup setup = PXSelect<PMSetup>.Select(this);
				return setup?.MigrationMode == true;
			}
		}

		public PXAction<PMRegister> curyToggle;
		[PXUIField(DisplayName = Messages.ViewBase)]
		[PXProcessButton]
		public IEnumerable CuryToggle(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				var wasDirty = Document.Cache.IsDirty;
				Document.Current.IsBaseCury = !Document.Current.IsBaseCury.GetValueOrDefault();
				Document.Update(Document.Current);
				if (!wasDirty && Document.Cache.IsDirty)
					Document.Cache.IsDirty = false;
			}
			return adapter.Get();
		}


        public PXAction<PMRegister> release;
        [PXUIField(DisplayName = GL.Messages.Release)]
        [PXProcessButton]
        public IEnumerable Release(PXAdapter adapter)
        {
			if (!IsAllPMTranLinesVisible(Document.Current))
			{
				throw new PXException(Messages.CannotReleaseTran);
			}
			
			ReleaseDocument(Document.Current);

			yield return Document.Current;
        }

		public virtual void ReleaseDocument(PMRegister doc)
		{
			if (doc != null && doc.Released != true)
			{
				this.Save.Press();
				PXLongOperation.StartOperation(this, delegate()
				{
					RegisterRelease.Release(doc);
				});
			}
		}

		public PXAction<PMRegister> reverse;
		[PXUIField(DisplayName = Messages.ReverseAllocation)]
		[PXProcessButton(Tooltip = Messages.ReverseAllocationTip)]
		public virtual IEnumerable Reverse(PXAdapter adapter)
		{
			Save.Press();

			var registerEntry = CreateInstance<RegisterEntry>();
			registerEntry.Document.Current = Document.Current;

			bool redirectToResult = !IsImport && !IsContractBasedAPI;

			PXLongOperation.StartOperation(this, delegate ()
			{
				registerEntry.ReverseCurrentDocument(redirectToResult);
			});

			return adapter.Get();
		}

		protected virtual void ReverseCurrentDocument(bool redirectToResult)
		{
			if (Document.Current != null)
			{
				var reversal = ReverseDocument(Document.Current);

				if (reversal != null && redirectToResult)  
				{
					var registerEntry = CreateInstance<RegisterEntry>();
					registerEntry.Document.Current = reversal;

					throw new PXRedirectRequiredException(registerEntry, "Open Reversal");
				}
			}
		}

		protected virtual PMRegister ReverseDocument(PMRegister document)
		{
			if (document == null)
				return null;

			Document.Current = document;

			if (Document.Current.IsAllocation == true && Document.Current.Released == true)
			{
				var reversalSelect = new PXSelect<PMRegister, Where<PMRegister.module, Equal<Current<PMRegister.module>>,
					And<PMRegister.origNoteID, Equal<Current<PMRegister.noteID>>,
					And<PMRegister.origDocType, Equal<PMOrigDocType.reversal>>>>>(this);

				PMRegister reversalDoc = reversalSelect.Select();

				if (reversalDoc != null)
				{
					throw new PXException(Messages.ReversalExists, reversalDoc.RefNbr);
				}

				if (!IsAllPMTranLinesVisible(Document.Current))
				{
					throw new PXException(Messages.CannotReverseAllocation);
				}

				List<ProcessInfo<Batch>> infoList;
				using (new PXConnectionScope())
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						var target = PXGraph.CreateInstance<RegisterEntry>();
						target.FieldVerifying.AddHandler<PMTran.inventoryID>(SuppressFieldVerifying);

						PMRegister reversalDocument = (PMRegister)target.Document.Cache.Insert();
						reversalDocument.Description = PXMessages.LocalizeNoPrefix(Messages.AllocationReversal);
						reversalDocument.OrigDocType = PMOrigDocType.Reversal;
						reversalDocument.OrigNoteID = Document.Current.NoteID;

						PMBillEngine billEngine = CreateInstance<PMBillEngine>();

						foreach (PMTran tran in Transactions.Select())
						{
							var reversalTran = billEngine.ReverseTran(tran).First();
							UpdateReversalTransactionDateOnAllocationSetting(tran, reversalTran);

							reversalTran = target.InsertTransactionWithManuallyChangedCurrencyInfo(reversalTran);
							target.ValidateOffestAccountGroup(tran, reversalTran);

							tran.ExcludedFromBilling = true;
							tran.ExcludedFromBillingReason = PXMessages.LocalizeNoPrefix(Messages.ExcludedFromBillingAsReversed);
							RegisterReleaseProcess.SubtractFromUnbilledSummary(this, tran);
							Transactions.Update(tran);
						}

						target.Save.Press();

						List<PMRegister> list = new List<PMRegister>
						{
							reversalDocument
						};

						bool releaseSuccess = RegisterRelease.ReleaseWithoutPost(list, false, out infoList);
						if (!releaseSuccess)
						{
							throw new PXException(GL.Messages.DocumentsNotReleased);
						}

						Transactions.Cache.AllowUpdate = true;
						foreach (PMTran tran in Transactions.Select())
						{
							UnallocateTran(tran);
						}

						Save.Press();
						ts.Complete();
					}

					//Posting should always be performed outside of transaction
					bool postSuccess = RegisterRelease.Post(infoList, false);
					if (!postSuccess)
					{
						throw new PXException(GL.Messages.DocumentsNotPosted);
					}
				}

				return reversalSelect.Select();
			}

			return null;
		}

		public void ValidateOffestAccountGroup(PMTran tran, PMTran reversal)
		{
			UpdateOffsetAccountId(this, Transactions.Cache, reversal);
			reversal = Transactions.Cache.Update(reversal) as PMTran;

			if (reversal.OffsetAccountGroupID != tran.OffsetAccountGroupID && tran.OffsetAccountID != null)
			{
				Account acc = SelectFrom<Account>
					.Where<Account.accountID.IsEqual<P.AsInt>>
					.View
					.SelectSingleBound(this, null, tran.OffsetAccountID);

				var origGroup = GetAccountGroup(tran.OffsetAccountGroupID);

				throw new PXException(GL.Messages.AccountGroupChanged, acc.AccountCD, origGroup.GroupCD);
			}
		}

		private void UpdateReversalTransactionDateOnAllocationSetting(PMTran original, PMTran reversal)
		{
			if (original.AllocationID == null)
			{
				reversal.Date = null;
				reversal.FinPeriodID = null;

				return;
			}

			PMAllocationDetail allocationDetail = SelectFrom<PMAllocationDetail>
				.Where<PMAllocationDetail.allocationID.IsEqual<P.AsString>>
				.View
				.SelectSingleBound(this, null, original.AllocationID);
			if (allocationDetail != null && allocationDetail.UseReversalDateFromOriginal == false)
			{
				reversal.Date = null;
				reversal.FinPeriodID = null;
			}
		}

		private PMAccountGroup GetAccountGroup(int? groupId)
		{
			return SelectFrom<PMAccountGroup>
				.Where<PMAccountGroup.groupID.IsEqual<P.AsInt>>
				.View
				.SelectSingleBound(this, null, groupId);
		}

		public PXAction<PMRegister> viewProject;
        [PXUIField(DisplayName = Messages.ViewProject, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewProject(PXAdapter adapter)
        {
            if (Transactions.Current != null)
            {
				var service = PXGraph.CreateInstance<PM.ProjectAccountingService>();
				service.NavigateToProjectScreen(Transactions.Current.ProjectID, PXRedirectHelper.WindowMode.NewWindow);
			}
            return adapter.Get();
        }

		public PXAction<PMRegister> viewTask;
        [PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewTask(PXAdapter adapter)
        {
            var graph = CreateInstance<ProjectTaskEntry>();
            graph.Task.Current = PXSelect<PMTask, Where<PMTask.projectID, Equal<Current<PMTran.projectID>>, And<PMTask.taskID, Equal<Current<PMTran.taskID>>>>>.Select(this);
            throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

		public PXAction<PMRegister> viewAllocationSorce;
		[PXUIField(DisplayName = Messages.ViewAllocationSource)]
		[PXButton]
		public IEnumerable ViewAllocationSorce(PXAdapter adapter)
		{
			if (Transactions.Current != null)
			{
				AllocationAudit graph = PXGraph.CreateInstance<AllocationAudit>();
				graph.Clear();
				graph.destantion.Current.TranID = Transactions.Current.TranID;
				throw new PXRedirectRequiredException(graph, true, Messages.ViewAllocationSource) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public PXAction<PMRegister> viewInventory;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewInventory(PXAdapter adapter)
		{
			InventoryItem inv = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMTran.inventoryID>>>>.SelectSingleBound(this, new object[] { Transactions.Current });
			if (inv != null && inv.StkItem == true)
			{
				InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
				graph.Item.Current = inv;
				throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			else if (inv != null)
			{
				NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
				graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(inv.InventoryID);
				throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

        public PXAction<PMRegister> viewCustomer;
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewCustomer(PXAdapter adapter)
        {
            BAccount account = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Current<PMTran.bAccountID>>>>.Select(this);

            if (account != null)
            {
                if (account.Type == BAccountType.CustomerType || account.Type == BAccountType.CombinedType)
                {
                    CustomerMaint graph = CreateInstance<CustomerMaint>();
                    graph.BAccount.Current = graph.BAccount.Search<BAccount.bAccountID>(Transactions.Current.BAccountID);
                    if (graph.BAccount.Current != null)
                    {
                        throw new PXRedirectRequiredException(graph, true, "") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                    }
                    else
                    {
                        Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<PMTran.bAccountID>>>>.Select(this);
                        throw new PXException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExistOrNoRights, customer.AcctCD));
                    }
                }
                else if (account.Type == BAccountType.VendorType)
                {
                    VendorMaint graph = CreateInstance<VendorMaint>();
                    graph.BAccount.Current = graph.BAccount.Search<VendorR.bAccountID>(Transactions.Current.BAccountID);
                    if (graph.BAccount.Current != null)
                    {
                        throw new PXRedirectRequiredException(graph, true, "") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                    }
                    else
                    {
                        VendorR vendor = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Current<PMTran.bAccountID>>>>.Select(this);
                        throw new PXException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExistOrNoRights, vendor.AcctCD));
                    }
                }
                else if (account.Type == BAccountType.EmployeeType || account.Type == BAccountType.EmpCombinedType)
                {
                    EmployeeMaint graph = CreateInstance<EmployeeMaint>();
                    graph.Employee.Current = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<PMTran.bAccountID>>>>.Select(this);
                    throw new PXRedirectRequiredException(graph, true, "") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
            return adapter.Get();
        }

        public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		public PXAction<PMRegister> selectProjectRate;
		[PXUIField(DisplayName = "Select Project Currency Rate")]
		[PXButton]
		public IEnumerable SelectProjectRate(PXAdapter adapter)
		{
			if (Transactions.Cache.Cached.Count() > 0)
			{
				MultiCurrency multiCurrency = GetExtension<MultiCurrency>();
				multiCurrency.currencyinfo.Cache.ClearQueryCache();
				multiCurrency.ProjectCuryInfo.AskExt();
			}

			return adapter.Get();
		}

		public PXAction<PMRegister> selectBaseRate;
		[PXUIField(DisplayName = "Select Base Currency Rate")]
		[PXButton]
		public IEnumerable SelectBaseRate(PXAdapter adapter)
		{
			if (Transactions.Cache.Cached.Count() > 0)
			{
				MultiCurrency multiCurrency = GetExtension<MultiCurrency>();
				multiCurrency.currencyinfo.Cache.ClearQueryCache();
				multiCurrency.BaseCuryInfo.AskExt();
			}
			return adapter.Get();
		}

		#region Event Handlers


		#region PMRegister

		protected virtual void _(Events.FieldUpdated<PMRegister, PMRegister.isBaseCury> e)
		{
			if (e.Row != null)
				Accessinfo.CuryViewState = e.Row.IsBaseCury.GetValueOrDefault();
				}

		protected virtual void _(Events.FieldUpdated<PMRegister, PMRegister.hold> e)
		{
			if (e.Row.Released == true)
				return;

			if (e.Row.Hold == true)
			{
				e.Row.Status = PMRegister.status.Hold;
			}
			else
			{
				e.Row.Status = PMRegister.status.Balanced;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMRegister, PMRegister.origDocType> e)
		{
			if (e.Row?.OrigDocType == null)
				return;

			e.Cache.SetValue<PMRegister.origDocNbr>(e.Row, null);
			e.Cache.SetValue<PMRegister.origNoteID>(e.Row, null);
		}

		protected virtual void _(Events.FieldUpdated<PMRegister, PMRegister.origDocNbr> e)
		{
			if (e.Row == null)
				return;

			var docNbr = e.Row.OrigDocNbr;
			var docType = PMOrigDocType.GetOrigDocType(e.Row.OrigDocType);

			if (string.IsNullOrEmpty(docType) || string.IsNullOrEmpty(docNbr))
			{
				e.Cache.SetValue<PMRegister.origDocNbr>(e.Row, null);
				e.Cache.SetValue<PMRegister.origNoteID>(e.Row, null);
				return;
			}

			var invoice = ARInvoice.PK.Find(this, docType, docNbr);
			e.Cache.SetValue<PMRegister.origDocNbr>(e.Row, invoice?.RefNbr);
			e.Cache.SetValue<PMRegister.origNoteID>(e.Row, invoice?.NoteID);
		}

		protected virtual void _(Events.FieldDefaulting<PMRegister, PMRegister.isMigratedRecord> e)
		{
			if (e.Row != null)
			{
				e.NewValue = MigrationMode;
			}
		}

		protected virtual void _(Events.RowSelected<PMRegister> e)
		{
			if (e.Row != null)
			{
				curyToggle.SetCaption(e.Row.IsBaseCury == true ? Messages.ViewCury: Messages.ViewBase);

				PXUIFieldAttribute.SetEnabled<PMRegister.date>(e.Cache, e.Row, e.Row.Released != true);
				PXUIFieldAttribute.SetEnabled<PMRegister.description>(e.Cache, e.Row, e.Row.Released != true);
				PXUIFieldAttribute.SetEnabled<PMRegister.status>(e.Cache, e.Row, e.Row.Released != true);
				PXUIFieldAttribute.SetEnabled<PMRegister.hold>(e.Cache, e.Row, e.Row.Released != true);

				Document.Cache.AllowUpdate = e.Row.Released != true && (MigrationMode || e.Row.Module == BatchModule.PM);
				Document.Cache.AllowDelete = e.Row.Released != true && (MigrationMode || e.Row.Module == BatchModule.PM);
				Insert.SetEnabled(MigrationMode || e.Row.Module == BatchModule.PM);
				release.SetEnabled(e.Row.Released != true && e.Row.Hold != true);

				Transactions.Cache.AllowDelete = e.Row.Released != true && e.Row.IsAllocation != true;
				Transactions.Cache.AllowInsert = e.Row.Released != true && e.Row.IsAllocation != true && (MigrationMode || e.Row.Module == BatchModule.PM);
				Transactions.Cache.AllowUpdate = e.Row.Released != true;

				reverse.SetEnabled(e.Row.Released == true && e.Row.IsAllocation == true);
				viewAllocationSorce.SetEnabled(e.Row.OrigDocType == PMOrigDocType.Allocation);
				curyToggle.SetEnabled(true);
				selectProjectRate.SetEnabled(true);
				selectBaseRate.SetEnabled(true);

				var isModuleAR = e.Row.Module == BatchModule.AR;
				var isModulePM = e.Row.Module == BatchModule.PM;
				var isReleased = e.Row.Released == true;

				PXUIFieldAttribute.SetVisible<PMRegister.origDocType>(e.Cache, e.Row,
					MigrationMode || isModulePM);
				PXUIFieldAttribute.SetEnabled<PMRegister.origDocType>(e.Cache, e.Row,
					MigrationMode && isModuleAR && !isReleased);

				PXUIFieldAttribute.SetVisible<PMRegister.origDocNbr>(e.Cache, e.Row,
					MigrationMode && isModuleAR);
				PXUIFieldAttribute.SetEnabled<PMRegister.origDocNbr>(e.Cache, e.Row,
					MigrationMode && isModuleAR && !isReleased);

				PXUIFieldAttribute.SetVisible<PMRegister.origNoteID>(e.Cache, e.Row,
					(MigrationMode && !isModuleAR) || (!MigrationMode && isModulePM));
				PXUIFieldAttribute.SetEnabled<PMRegister.origNoteID>(e.Cache, e.Row,
					false);

				PXStringListAttribute.SetList<PMRegister.origDocType>(
					e.Cache,
					e.Row,
					MigrationMode && isModuleAR
						? (PXStringListAttribute) new PMOrigDocType.ListARAttribute()
						: new PMOrigDocType.ListAttribute());

				PXUIFieldAttribute.SetVisible<PMRegister.amtTotal>(e.Cache, e.Row, !PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>());

				if (!this.IsImport && !this.IsContractBasedAPI)
				{
					decimal qty = 0, billableQty = 0, amount = 0;
					//no need to calculate when doing import. It will just slow down the import.

					foreach (PMTran tran in Transactions.Select())
					{
						qty += tran.Qty.GetValueOrDefault();
						billableQty += tran.BillableQty.GetValueOrDefault();
						amount += tran.Amount.GetValueOrDefault();
					}

					e.Row.QtyTotal = qty;
					e.Row.BillableQtyTotal = billableQty;
					e.Row.AmtTotal = amount;
				}

				if (!IsAllPMTranLinesVisible(e.Row))
				{
					e.Cache.RaiseExceptionHandling<PMRegister.qtyTotal>(e.Row, e.Row.QtyTotal, new PXSetPropertyException(Messages.LinesAreHidden, PXErrorLevel.Warning));
					e.Cache.RaiseExceptionHandling<PMRegister.billableQtyTotal>(e.Row, e.Row.BillableQtyTotal, new PXSetPropertyException(Messages.LinesAreHidden, PXErrorLevel.Warning));
					e.Cache.RaiseExceptionHandling<PMRegister.amtTotal>(e.Row, e.Row.AmtTotal, new PXSetPropertyException(Messages.LinesAreHidden, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void _(Events.RowDeleted<PMRegister> e)
		{
			if (e.Row != null)
			{
				if (e.Row.Released != true && e.Row.OrigDocType == PMOrigDocType.Timecard && e.Row.OrigNoteID != null)
				{
					EPTimeCard timeCard = PXSelect<EPTimeCard, Where<EPTimeCard.noteID, Equal<Required<EPTimeCard.noteID>>>>.Select(this, e.Row.OrigNoteID);
					if (timeCard != null)
					{
						Views.Caches.Add(typeof(EPTimeCard));
						UnreleaseTimeCard(timeCard);
					}
				}
			}
		}

		protected virtual void UnreleaseTimeCard(EPTimeCard timeCard)
		{
			timeCard.IsReleased = false;
			timeCard.Status = EPTimeCardStatusAttribute.ApprovedStatus;
			Caches[typeof(EPTimeCard)].Update(timeCard);
		}

		#endregion

		#region PMTran

		protected virtual void PMTran_BranchID_FieldUpdated(Events.FieldUpdated<PMTran, PMTran.branchID> e)
		{
			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<PMTran.finPeriodID>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.bAccountID> e)
		{
			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<PMTran.locationID>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.inventoryID> e)
		{
			if (e.Row != null && string.IsNullOrEmpty(e.Row.Description) && e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, e.Row.InventoryID);
				if (item != null)
				{
					e.Row.Description = item.Descr;

					PMProject project = PXSelect<PMProject,
						Where<PMProject.contractID, Equal<Required<PMTran.projectID>>>>.Select(this, e.Row.ProjectID);

					if (project != null && project.CustomerID != null)
					{
						Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, project.CustomerID);
						if (customer != null && !string.IsNullOrEmpty(customer.LocaleName))
						{
							e.Row.Description = PXDBLocalizableStringAttribute.GetTranslation(Caches[typeof(InventoryItem)], item, nameof(InventoryItem.Descr), customer.LocaleName);
						}
					}
				}
			}

			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<PMTran.uOM>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.qty> e)
		{
			if (e.Row != null && e.Row.Billable == true)
			{
				e.Cache.SetDefaultExt<PMTran.billableQty>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.billable> e)
		{
			if (e.Row != null)
			{
				if (e.Row.Billable == true)
				{
					e.Cache.SetDefaultExt<PMTran.billableQty>(e.Row);
				}
				else
				{
					e.Cache.SetValueExt<PMTran.billableQty>(e.Row, 0m);
				}
			}
		}
				
		protected virtual void _(Events.FieldDefaulting<PMTran, PMTran.billableQty> e)
		{
			if (e.Row != null && e.Row.Billable == true)
			{
				e.NewValue = e.Row.Qty;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.billableQty> e)
		{
			if (e.Row != null && e.Row.BillableQty != 0)
			{
				SubtractUsage(e.Cache, e.Row, (decimal?)e.OldValue, e.Row.UOM);
				AddUsage(e.Cache, e.Row, e.Row.BillableQty, e.Row.UOM);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.uOM> e)
		{
			if (e.Row != null && e.Row.BillableQty != 0)
			{
				SubtractUsage(e.Cache, e.Row, e.Row.BillableQty, (string)e.OldValue);
				AddUsage(e.Cache, e.Row, e.Row.BillableQty, e.Row.UOM);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.date> e)
		{
			if (e.Row == null) return;
			else e.Cache.SetDefaultExt<PMTran.finPeriodID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.offsetAccountID> e)
		{
			UpdateOffsetAccountId(this, e.Cache, e.Row);
		}

		public static void UpdateOffsetAccountId(PXGraph graph, PXCache cache, PMTran tran)
		{
			if (tran != null)
			{
				int? accountId = tran.OffsetAccountID;
				int? groupId = null;
				if (accountId != null)
				{
					var accountGroup = SelectFrom<Account>
						.LeftJoin<PMAccountGroup>.On<Account.accountGroupID.IsEqual<PMAccountGroup.groupID>>
						.Where<Account.accountID.IsEqual<P.AsInt>>
						.View
						.SelectSingleBound(graph, null, accountId)
						.FirstOrDefault()
						?.GetItem<PMAccountGroup>();

					groupId = accountGroup?.GroupID;
				}
				cache.SetValueExt<PMTran.offsetAccountGroupID>(tran, groupId);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMTran, PMTran.resourceID> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMTran.projectID>>>>.Select(this, e.Row.ProjectID);
				if (project != null && project.RestrictToEmployeeList == true)
				{
					EPEmployeeContract rate = PXSelect<EPEmployeeContract, Where<EPEmployeeContract.contractID, Equal<Required<PMTran.projectID>>,
						And<EPEmployeeContract.employeeID, Equal<Required<EPEmployeeContract.employeeID>>>>>.Select(this, e.Row.ProjectID, e.NewValue);
					if (rate == null)
					{
						EPEmployee emp = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, e.NewValue);
						if (emp != null)
							e.NewValue = emp.AcctCD;

						throw new PXSetPropertyException(Messages.EmployeeNotInProjectList);
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMTran, PMTran.offsetAccountID> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				Account offsetAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, e.NewValue);
				int availableAccountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>, And<Match<PMAccountGroup, Current<AccessInfo.userName>>>>>.Select(this, offsetAccount.AccountGroupID).Count();

				if(offsetAccount.AccountGroupID != null && availableAccountGroup == 0)
				{
					var ex = new PXSetPropertyException(Messages.AccountInRestrictedAG, PXErrorLevel.Error);
					ex.ErrorValue = offsetAccount.AccountCD;
					throw ex;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.projectID> e)
		{
			if (e.Row != null && e.Row.ProjectID != null)
			{
				GetExtension<MultiCurrency>().CalcCuryRatesForProject(e.Cache, e.Row);
			}
		}

		protected virtual void _(Events.RowSelected<PMTran> e)
		{
			if (e.Row != null)
			{
				var canBeAllocated = e.Row.Allocated != true && e.Row.ExcludedFromAllocation != true;
				PXUIFieldAttribute.SetEnabled<PMTran.billableQty>(e.Cache, e.Row, e.Row.Billable == true);
				PXUIFieldAttribute.SetEnabled<PMTran.projectID>(e.Cache, e.Row, canBeAllocated);
				PXUIFieldAttribute.SetEnabled<PMTran.taskID>(e.Cache, e.Row, canBeAllocated);
				PXUIFieldAttribute.SetEnabled<PMTran.accountGroupID>(e.Cache, e.Row, canBeAllocated);
				PXUIFieldAttribute.SetEnabled<PMTran.accountID>(e.Cache, e.Row, canBeAllocated);
				PXUIFieldAttribute.SetEnabled<PMTran.offsetAccountID>(e.Cache, e.Row, canBeAllocated);
			}
		}

		protected virtual void _(Events.RowInserting<PMTran> e)
		{
			if (e.Row == null) return;
			else if (IsImportFromExcel || IsImport || ManualCurrencyInfoCreation)
			{
				configureOnImport = true;
				try
				{
					ConfigureCurrencyManually(e.Row);
				}
				finally
				{
					configureOnImport = false;
				}
			}
		}

		protected bool ManualCurrencyInfoCreation = false;

		protected virtual void _(Events.RowInserting<CurrencyInfo> e)
		{
			if (e.Row != null)
			{
				e.Cancel = (IsImportFromExcel || IsImport || ManualCurrencyInfoCreation) && !configureOnImport;
			}
		}

		protected virtual void _(Events.RowInserted<PMTran> e)
		{
			if (e.Row != null)
			{
				AddAllocatedTotal(e.Row);

				if (e.Row.BillableQty != 0)
				{
					AddUsage(e.Cache, e.Row, e.Row.BillableQty, e.Row.UOM);
				}
			}
		}

		bool configureOnImport = false;
		protected virtual void ConfigureCurrencyManually(PMTran tran)
		{
			PMProject project = Project.Search<PMProject.contractID>(tran.ProjectID);

			string projectCuryID = (project.NonProject == true || project.BaseType != CTPRType.Project) ? Company.Current.BaseCuryID : project.CuryID;
			string baseCuryID = (project.NonProject == true || project.BaseType != CTPRType.Project) ? Company.Current.BaseCuryID : project.BaseCuryID;
			string tranCuryID = tran.TranCuryID ?? projectCuryID;

			if (tranCuryID == projectCuryID)
			{
				CurrencyInfo projectCuryInfo = MultiCurrencyService.CreateDirectRate(this, tranCuryID, tran.Date, BatchModule.PM);
				tran.ProjectCuryInfoID = projectCuryInfo.CuryInfoID;
			}
			else
			{
				CurrencyInfo projectCuryInfo = MultiCurrencyService.CreateRate(this, tranCuryID, projectCuryID, tran.Date, project.RateTypeID, BatchModule.PM);
				tran.ProjectCuryInfoID = projectCuryInfo.CuryInfoID;
			}

			if (tranCuryID == baseCuryID)
			{
				CurrencyInfo projectCuryInfo = MultiCurrencyService.CreateDirectRate(this, tranCuryID, tran.Date, BatchModule.PM);
				tran.BaseCuryInfoID = projectCuryInfo.CuryInfoID;
			}
			else
			{
				CurrencyInfo projectCuryInfo = MultiCurrencyService.CreateRate(this, tranCuryID, baseCuryID, tran.Date, project.RateTypeID, BatchModule.PM);
				tran.BaseCuryInfoID = projectCuryInfo.CuryInfoID;
			}
		}

		protected virtual void _(Events.RowUpdated<PMTran> e)
		{
			if (e.Row != null && e.OldRow != null && e.Row.Released != true &&
				e.Row.TranCuryAmount != e.OldRow.TranCuryAmount || e.Row.BillableQty != e.OldRow.BillableQty || e.Row.Qty != e.OldRow.Qty)
			{
				SubtractAllocatedTotal(e.OldRow);
				AddAllocatedTotal(e.Row);
			}
		}

		protected virtual void _(Events.RowDeleted<PMTran> e)
        {
			UnallocateTran(e.Row);
			UnreleaseActivity(e.Row);
			RefreshOriginalTran(e.Row);
		}

		protected virtual void UnallocateTran(PMTran row)
		{
			if (row != null)
			{
				PXSelectBase<PMAllocationAuditTran> select = new PXSelectJoin<PMAllocationAuditTran,
					InnerJoin<PMTran, On<PMTran.tranID, Equal<PMAllocationAuditTran.sourceTranID>>>,
					Where<PMAllocationAuditTran.tranID, Equal<Required<PMAllocationAuditTran.tranID>>>>(this);

				foreach (PXResult<PMAllocationAuditTran, PMTran> res in select.Select(row.TranID))
				{
					PMAllocationAuditTran aTran = (PMAllocationAuditTran) res;
					PMTran pmTran = (PMTran)res;

					if (!(pmTran.TranType == row.TranType && pmTran.RefNbr == row.RefNbr))
					{
						pmTran.Allocated = false;
						Transactions.Update(pmTran);
					}

					PMAllocationSourceTran ast = SourceTran.Select(aTran.AllocationID, aTran.SourceTranID);
					SourceTran.Delete(ast);
					AuditTrans.Delete(aTran);
				}

				SubtractAllocatedTotal(row);
			}
		}

        protected virtual void UnreleaseActivity(PMTran row)
        {
			if (row.OrigRefID != null && Document.Current != null && Document.Current.IsAllocation != true)
            {
                PMTimeActivity activity = PXSelect<PMTimeActivity, 
					Where<PMTimeActivity.noteID, Equal<Required<PMTimeActivity.noteID>>>>.Select(this, row.OrigRefID);

                if (activity != null)
                {
                    activity.Released = false;
                    activity.EmployeeRate = null;
                    Activities.Update(activity);
                }
            }
        }

		protected virtual void RefreshOriginalTran(PMTran row)
		{
			if (row == null)
				return;

			if (Document.Current?.OrigDocType != PMOrigDocType.WipReversal)
				return;

			if (!row.OrigTranID.HasValue)
				return;

			PMTran origTran = PXSelect<PMTran,
							 	 Where<PMTran.tranID, Equal<Required<PMTran.origTranID>>>>
								.Select(this, row.OrigTranID);

			if (origTran == null)
				return;

			PMRegister origDocument = PXSelect<PMRegister,
										 Where<PMRegister.module, Equal<Required<PMTran.tranType>>,
									   	   And<PMRegister.refNbr, Equal<Required<PMTran.refNbr>>>>>
										.Select(this, origTran.TranType, origTran.RefNbr);

			if (origDocument == null)
				return;

			if (origDocument.OrigDocType != PMOrigDocType.Allocation)
				return;

			if (origTran.ExcludedFromBilling == true)
			{
				origTran.ExcludedFromBilling = false;
				Transactions.Update(origTran);
			}
		}

		protected virtual void _(Events.RowPersisting<PMTran> e)
		{
			if (e.Row != null && e.Operation != PXDBOperation.Delete)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMTran.projectID>>>>.Select(this, e.Row.ProjectID);
				if (project != null && e.Row.AccountGroupID == null && project.BaseType == CT.CTPRType.Project && !ProjectDefaultAttribute.IsNonProject(project.ContractID))
				{
					e.Cache.RaiseExceptionHandling<PMTran.accountGroupID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMTran.accountGroupID)}]"));
				}
			}
		}
				
		#endregion

		#endregion

		public virtual void ReverseCreditMemo(ARRegister arDoc, List<PXResult<ARTran, PMTran>> list, List<PMTran> remainders)
		{
			var billEngine = PXGraph.CreateInstance<PMBillEngine>();

			PMRegister doc = Document.Insert();
			doc.OrigDocType = PMOrigDocType.CreditMemo;
			doc.OrigNoteID = arDoc.NoteID;
			doc.Description = PXMessages.LocalizeNoPrefix(Messages.CreditMemo);

			foreach (PXResult<ARTran, PMTran> item in list)
			{
				ARTran ar = (ARTran)item;
				PMTran pm = (PMTran)item;

				PMTran newTran = PXCache<PMTran>.CreateCopy(pm);
				newTran.OrigTranID = pm.TranID;
				
				newTran.Date = pm.Date;
				newTran.FinPeriodID = pm.FinPeriodID;
				if (!IsFinPeriodValid(newTran))
				{
					newTran.FinPeriodID = ar.FinPeriodID;
				}

				if (newTran.AccountGroupID != null)
				{
					ValidateAccount(newTran);
				}

				newTran.TranID = null;
				newTran.TranType = null;
				newTran.RefNbr = null;
				newTran.RefLineNbr = null;
				newTran.ARRefNbr = null;
				newTran.ARTranType = null;
				newTran.ProformaRefNbr = null;
				newTran.ProformaLineNbr = null;
				newTran.BatchNbr = null;
				newTran.TranDate = null;
				newTran.TranPeriodID = null;
				newTran.BilledDate = null;
				newTran.NoteID = null;
				newTran.Released = false;
				newTran.Billed = false;
				newTran.Allocated = false;
				newTran.ExcludedFromBilling = false;
				newTran = Transactions.Insert(newTran);

				string note = PXNoteAttribute.GetNote(Transactions.Cache, pm);
				if (note != null)
					PXNoteAttribute.SetNote(Transactions.Cache, newTran, note);

				Guid[] files = PXNoteAttribute.GetFileNotes(Transactions.Cache, pm);
				if (files != null && files.Length > 0)
					PXNoteAttribute.SetFileNotes(Transactions.Cache, newTran, files);

				if (pm.Reverse == PMReverse.Never && pm.RemainderOfTranID == null)
				{
					var reversal = billEngine.ReverseTran(pm).First();
					if (!IsFinPeriodValid(reversal))
					{
						reversal.FinPeriodID = ar.FinPeriodID;
					}
					Transactions.Insert(reversal);
				}

				PMTran remainder = SelectFrom<PMTran>.Where<PMTran.remainderOfTranID.IsEqual<@P.AsInt>>.View.Select(this, pm.TranID);
				if (remainder != null && remainder.Billed != true)
				{
					remainders.Add(remainder);
					remainder.ExcludedFromBilling = true;
					remainder.ExcludedFromBillingReason = PXMessages.LocalizeFormatNoPrefix(Messages.ExcludedFromBillingAsCreditMemoWrittenOff, arDoc.RefNbr);
					Transactions.Cache.MarkUpdated(remainder, assertError: true);
				}
			}
		}

		private void ValidateAccount(PMTran tran)
		{
			if (tran != null && tran.AccountID != null && tran.ProjectID != null)
			{
				Account account = new PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>(this).Select(tran.AccountID);
				if (account != null && account.AccountGroupID != tran.AccountGroupID)
				{
					PMProject project = new PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>(this).Select(tran.ProjectID);
					throw new Exception(PXMessages.LocalizeFormatNoPrefix(Messages.CreditMemoCannotReleaseWithOutAppropriateAccount, project?.ContractCD, account.AccountCD));
				}
			}
		}

		public virtual void ValidateContractBaseCurrency(Contract contract)
		{
		}

		protected virtual bool IsFinPeriodValid(PMTran tran)
		{
			try
			{
				string newValue = tran.FinPeriodID;
				OpenPeriodAttribute attribute = new OpenPeriodAttribute();
				attribute.IsValidPeriod(Transactions.Cache, tran, newValue);
			}
			catch (PXSetPropertyException)
			{
				return false;
			}

			return true;
		}

		public virtual void BillLater(ARRegister arDoc, List<Tuple<PMProformaTransactLine, PMTran>> billLater)
		{
			var billEngine = PXGraph.CreateInstance<PMBillEngine>();

			PMRegister doc = Document.Insert();
			doc.OrigDocType = PMOrigDocType.UnbilledRemainder;
			doc.OrigNoteID = arDoc.NoteID;
			doc.Description = PXMessages.LocalizeNoPrefix(Messages.UnbilledRemainder);

			foreach (var res in billLater)
			{
				var pfLine = res.Item1;
				var tran = res.Item2;
				PMProject project;
				if (ProjectDefaultAttribute.IsProject(this, tran.ProjectID, out project))
				{
					PMTran newTran = PXCache<PMTran>.CreateCopy(tran);
					
					newTran.RemainderOfTranID = tran.TranID;
					if (newTran.TranCuryID != arDoc.CuryID)
					{
						newTran.TranCuryID = arDoc.CuryID;
						newTran.BaseCuryInfoID = null;
						newTran.ProjectCuryInfoID = null;
					}

					newTran.Date = tran.Date;
					newTran.FinPeriodID = tran.FinPeriodID;
					if (!IsFinPeriodValid(newTran))
					{
						newTran.FinPeriodID = arDoc.FinPeriodID;
					}

					newTran.TranID = null;
					newTran.TranType = null;
					newTran.RefNbr = null;
					newTran.ARRefNbr = null;
					newTran.ARTranType = null;
					newTran.RefLineNbr = null;
					newTran.ProformaRefNbr = null;
					newTran.ProformaLineNbr = null;
					newTran.BatchNbr = null;
					newTran.TranDate = null;
					newTran.TranPeriodID = null;
					newTran.BilledDate = null;
					newTran.NoteID = null;
					newTran.AllocationID = null;

					newTran.Description = pfLine.Description;
					newTran.UOM = pfLine.UOM;
					newTran.Qty = Math.Max(0, pfLine.BillableQty.GetValueOrDefault() - pfLine.Qty.GetValueOrDefault());
					newTran.BillableQty = newTran.Qty;

					newTran.Released = false;
					newTran.Billed = false;
					newTran.Allocated = false;
					newTran.ExcludedFromBilling = false;
					newTran.ExcludedFromAllocation = true;
					newTran.Reverse = PMReverse.Never;

					newTran = Transactions.Insert(newTran);
					
					if (!billEngine.IsNonGL(tran) &&
						Setup.Current?.UnbilledRemainderAccountID != null &&
						Setup.Current?.UnbilledRemainderOffsetAccountID != null &&
						Setup.Current?.UnbilledRemainderSubID != null &&
						Setup.Current?.UnbilledRemainderOffsetSubID != null)
					{
						newTran.AccountID = Setup.Current.UnbilledRemainderAccountID;
						newTran.SubID = Setup.Current.UnbilledRemainderSubID;
						newTran.OffsetAccountID = Setup.Current.UnbilledRemainderOffsetAccountID;
						newTran.OffsetSubID = Setup.Current.UnbilledRemainderOffsetSubID;

						Account account = Account.PK.Find(this, newTran.AccountID);
						newTran.AccountGroupID = account.AccountGroupID;
					}

					if (arDoc.CuryID == project.CuryID)
					{
						newTran.Amount = Math.Max(0, pfLine.BillableAmount.GetValueOrDefault() - pfLine.LineTotal.GetValueOrDefault());
						newTran.TranCuryAmount = Math.Max(0, pfLine.CuryBillableAmount.GetValueOrDefault() - pfLine.CuryLineTotal.GetValueOrDefault());
						newTran.ProjectCuryAmount = Math.Max(0, pfLine.CuryBillableAmount.GetValueOrDefault() - pfLine.CuryLineTotal.GetValueOrDefault());
					}
					else
					{
						newTran.Amount = Math.Max(0, pfLine.BillableAmount.GetValueOrDefault() - pfLine.LineTotal.GetValueOrDefault());
						decimal val = GetExtension<MultiCurrency>().GetCurrencyInfo(newTran.ProjectCuryInfoID).CuryConvCury(newTran.Amount.GetValueOrDefault());
						newTran.TranCuryAmount = val;
						newTran.ProjectCuryAmount = val;
					}
					Transactions.Update(newTran);
				}
			}
		}

		public virtual List<PMTran> GetRemaindersToReverse(List<PMTran> trans)
		{
			//reversals can be already exists from creditmemo cases and old logic versions
			var result = new List<PMTran>();
			var resversalSelect = new PXSelect<PMTran, Where<PMTran.origTranID, Equal<Required<PMTran.tranID>>>>(this);
			foreach (var tran in trans)
			{
				if (tran.ExcludedFromBalance == true) continue; //backward compatibility
				bool reversalExist = resversalSelect.Select(tran.TranID).Any();
				if (reversalExist) continue;

				result.Add(tran);
			}
			return result;
		}

		public virtual void ReverseRemainders(ARRegister arDoc, List<PMTran> trans)
		{
			PMRegister doc = Document.Insert();
			doc.OrigDocType = PMOrigDocType.UnbilledRemainderReversal;
			doc.OrigNoteID = arDoc.NoteID;
			doc.Description = PXMessages.LocalizeNoPrefix(Messages.UnbilledRemainderReversal);

			var billEngine = PXGraph.CreateInstance<PMBillEngine>();
			foreach (PMTran tran in trans)
			{
				PMTran reversal = billEngine.ReverseTran(tran).First();

				reversal.Date = tran.Date;
				reversal.FinPeriodID = tran.FinPeriodID;
				if (!IsFinPeriodValid(reversal))
				{
				reversal.FinPeriodID = arDoc.FinPeriodID;
				}

				Transactions.Insert(reversal);
			}
		}

		public virtual void ReverseAllocations(ARRegister arDoc, List<PMTran> trans)
		{
			PMRegister doc = Document.Insert();
			doc.OrigDocType = PMOrigDocType.AllocationReversal;
			doc.OrigNoteID = arDoc.NoteID;
			doc.Description = PXMessages.LocalizeNoPrefix(Messages.AllocationReversalOnARInvoiceRelease);

			PMBillEngine billEngine = PXGraph.CreateInstance<PMBillEngine>();
			foreach (PMTran tran in trans)
			{
				foreach (PMTran reverse in billEngine.ReverseTran(tran))
				{
					reverse.Date = arDoc.DocDate;
					reverse.FinPeriodID = arDoc.FinPeriodID;
					Transactions.Insert(reverse);
				}
			}
		}

		protected void SuppressFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		private void AddUsage(PXCache sender, PMTran tran, decimal? used, string UOM)
		{
			//Only project is handled here. Contracts are handled explicitly in UsageMaint.cs
			if (tran.ProjectID != null && tran.TaskID != null && tran.InventoryID != null && tran.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				RecurringItemEx targetItem = PXSelect<RecurringItemEx,
					Where<RecurringItemEx.projectID, Equal<Required<RecurringItemEx.projectID>>,
					And<RecurringItemEx.taskID, Equal<Required<RecurringItemEx.taskID>>,
					And<RecurringItemEx.inventoryID, Equal<Required<RecurringItemEx.inventoryID>>>>>>.Select(this, tran.ProjectID, tran.TaskID, tran.InventoryID);

				if (targetItem != null)
				{
					decimal inTargetUnit = used ?? 0;
					if (!string.IsNullOrEmpty(UOM))
					{
						inTargetUnit = INUnitAttribute.ConvertToBase(sender, tran.InventoryID, UOM, used ?? 0, INPrecision.QUANTITY);
					}

					PMRecurringItemAccum item = new PMRecurringItemAccum();
					item.ProjectID = tran.ProjectID;
					item.TaskID = tran.TaskID;
					item.InventoryID = tran.InventoryID;

					item = RecurringItems.Insert(item);
					item.Used += inTargetUnit;
					item.UsedTotal += inTargetUnit;
				}
			}
		}

		private void SubtractUsage(PXCache sender, PMTran tran, decimal? used, string UOM)
		{
			if (used != 0)
				AddUsage(sender, tran, -used, UOM);
		}

		private void AddAllocatedTotal(PMTran tran)
		{
			if (tran.OrigProjectID != null && tran.OrigTaskID != null && tran.OrigAccountGroupID != null)
			{
				PMTaskAllocTotalAccum tat = new PMTaskAllocTotalAccum();
				tat.ProjectID = tran.OrigProjectID;
				tat.TaskID = tran.OrigTaskID;
				tat.AccountGroupID = tran.OrigAccountGroupID;
				tat.InventoryID = tran.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID);
				tat.CostCodeID = tran.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());

				tat = AllocationTotals.Insert(tat);
				tat.Amount += tran.Amount;
				tat.Quantity += (tran.Billable == true && tran.UseBillableQty == true) ? tran.BillableQty : tran.Qty;
			}
		}

		private void SubtractAllocatedTotal(PMTran tran)
		{
			if (tran.OrigProjectID != null && tran.OrigTaskID != null && tran.OrigAccountGroupID != null && tran.InventoryID != null)
			{
				PMTaskAllocTotalAccum tat = new PMTaskAllocTotalAccum();
				tat.ProjectID = tran.OrigProjectID;
				tat.TaskID = tran.OrigTaskID;
				tat.AccountGroupID = tran.OrigAccountGroupID;
				tat.InventoryID = tran.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID);
				tat.CostCodeID = tran.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());

				tat = AllocationTotals.Insert(tat);
				tat.Amount -= tran.Amount;
				tat.Quantity -= (tran.Billable == true && tran.UseBillableQty == true) ? tran.BillableQty : tran.Qty;
			}
		}

		public virtual PMTran CreateTransaction(CreatePMTran createPMTran)
		{
			if (!CanCreateTransaction(createPMTran.TimeActivity, createPMTran.TimeSpent, createPMTran.TimeBillable))
				return null;

			bool postToOffBalance = GetPostToOffbalance(createPMTran.EmployeeID);
			InventoryItem laborItem = InventoryItem.PK.Find(this, createPMTran.TimeActivity.LabourItemID);
			EPEmployee employee = EPEmployee.PK.Find(this, createPMTran.EmployeeID);
			PMProject project = PMProject.PK.Find(this, createPMTran.TimeActivity.ProjectID);
			PMTask task = PMTask.PK.FindDirty(this, createPMTran.TimeActivity.ProjectID, createPMTran.TimeActivity.ProjectTaskID);
			Branch branch = PXSelect<Branch, Where<Branch.bAccountID, Equal<Required<EPEmployee.parentBAccountID>>>>.Select(this, employee.ParentBAccountID);
			FinPeriod finPeriod = FinPeriodRepository.FindFinPeriodByDate(createPMTran.Date, PXAccess.GetParentOrganizationID(branch?.BranchID));
			if (finPeriod == null)
			{
				throw new PXException(Messages.FinPeriodForDateNotFound);
			}

			WriteWarningsToTrace(project, task);

			string subCD = null;
			string offsetSubCD = null;

			if (!postToOffBalance && project.NonProject != true)
			{
				ValidateExpenseSubMask(project, task, laborItem, employee);
				ValidateExpenseAccrualSubMask(project, task, laborItem, employee);

				subCD = CombineCostSubAccount(project, task, laborItem, employee);
				offsetSubCD = CombineOffsetSubAccount(project, task, laborItem, employee);
			}

			PMTran tran = new PMTran();
			tran.ProjectID = createPMTran.TimeActivity.ProjectID;
			tran.BranchID = branch?.BranchID;
			tran.ProjectID = createPMTran.TimeActivity.ProjectID;
			tran.TaskID = createPMTran.TimeActivity.ProjectTaskID;
			tran.CostCodeID = createPMTran.TimeActivity.CostCodeID;
			tran.InventoryID = createPMTran.TimeActivity.LabourItemID;
			tran.UnionID = createPMTran.TimeActivity.UnionID;
			tran.WorkCodeID = createPMTran.TimeActivity.WorkCodeID;
			tran.ResourceID = createPMTran.EmployeeID;
			tran.Date = createPMTran.Date;
			tran.TranCuryID = createPMTran.TranCuryID ?? branch?.BaseCuryID ?? Accessinfo.BaseCuryID;
			tran.FinPeriodID = finPeriod.FinPeriodID;
			tran.Qty = GetConvertedAndRoundedTime(createPMTran.TimeSpent, laborItem.BaseUnit);
			tran.Billable = createPMTran.TimeActivity.IsBillable;
			tran.BillableQty = GetConvertedAndRoundedTime(createPMTran.TimeBillable, laborItem.BaseUnit);
			tran.UOM = laborItem.BaseUnit;
			tran.TranCuryUnitRate = PXDBPriceCostAttribute.Round(createPMTran.Cost.GetValueOrDefault());
			tran.Description = createPMTran.TimeActivity.Summary;
			tran.StartDate = createPMTran.TimeActivity.Date;
			tran.EndDate = createPMTran.TimeActivity.Date;
			tran.OrigRefID = createPMTran.TimeActivity.NoteID;
			tran.EarningType = createPMTran.TimeActivity.EarningTypeID;
			tran.OvertimeMultiplier = createPMTran.OvertimeMult;
			tran.IsNonGL = IsNonGlTransaction(createPMTran.EmployeeID);
			if (createPMTran.TimeActivity.RefNoteID != null)
			{
				Note note = PXSelectJoin<Note, 
					InnerJoin<CRActivityLink, 
						On<CRActivityLink.refNoteID, Equal<Note.noteID>>>,
					Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>>>.Select(this, createPMTran.TimeActivity.RefNoteID);
				if (note != null && note.EntityType == typeof(CRCase).FullName)
				{
					CRCase crCase = PXSelectJoin<CRCase,
						InnerJoin<CRActivityLink,
							On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>, 
						Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>>>.Select(this, createPMTran.TimeActivity.RefNoteID);

					if (crCase != null && crCase.IsBillable != true)
					{
						//Case is not billable, do not mark the cost transactions as Billed. User may configure Project and use Project Billing for these transactions.
					}
					else
					{
						//Activity associated with the case will be billed (or is already billed) by the Case Billing procedure. 
						tran.ExcludedFromAllocation = true;
						tran.ExcludedFromBilling = true;
						tran.ExcludedFromBillingReason = PXMessages.LocalizeFormatNoPrefix(Messages.ExcludedFromBillingAsBillableWithCase, crCase.CaseCD);
					}
				}
			}

			if (postToOffBalance)
			{
				tran.AccountGroupID = EPSetupMaint.GetOffBalancePostingAccount(this, epSetup.Current, createPMTran.EmployeeID);
			}
			else
			{
				tran.AccountID = GetCostAccount(project, task, laborItem, employee);
				tran.AccountGroupID = GetAccountGroupFromAccount(tran.AccountID);
				tran.OffsetAccountID = GetOffsetAccount(project, task, laborItem, employee);
				
				if (string.IsNullOrEmpty(subCD))
					tran.SubID = laborItem.COGSSubID;

				if (string.IsNullOrEmpty(offsetSubCD))
					tran.OffsetSubID = laborItem.InvtSubID;
			}

			if (createPMTran.InsertTransaction)
			{
				try
				{
					tran = InsertTransactionWithManuallyChangedCurrencyInfo(tran);
				}
				catch (PXFieldValueProcessingException ex)
				{
					if (ex.InnerException is PXTaskIsCompletedException)
					{
						PMTask taskEx = PMTask.PK.FindDirty(this, ((PXTaskIsCompletedException)ex.InnerException).ProjectID, ((PXTaskIsCompletedException)ex.InnerException).TaskID);
						if (taskEx != null)
						{
							PMProject projectEx = PMProject.PK.Find(this, taskEx.ProjectID);
							if (projectEx != null)
							{
								throw new PXException(Messages.ProjectTaskIsCompletedDetailed, projectEx.ContractCD.Trim(), taskEx.TaskCD.Trim());
							}
						}
					}

					throw ex;
				}
				catch (PXException ex)
				{
					throw ex;
				}

				if (!string.IsNullOrEmpty(subCD))
					Transactions.SetValueExt<PMTran.subID>(tran, subCD);

				if (!string.IsNullOrEmpty(offsetSubCD))
					Transactions.SetValueExt<PMTran.offsetSubID>(tran, offsetSubCD);

				PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(PMTimeActivity)], createPMTran.TimeActivity, Transactions.Cache, tran, epSetup.Current.GetCopyNoteSettings<PXModule.pm>());
			}
			return tran;
		}

		protected virtual int? GetCostAccount(PMProject project, PMTask task, InventoryItem laborItem, EPEmployee employee)
		{
			if (project == null) throw new ArgumentNullException(nameof(project));

			if ( project.NonProject == true)
			{
				return GetCostAccountForNonProject(laborItem);
			}
			else
			{
				return GetCostAccountForProject(project, task, laborItem, employee);
			}
		}

		private int? GetCostAccountForNonProject(InventoryItem laborItem)
		{
			if ( laborItem.COGSAcctID == null)
			{
				throw new PXException(EP.Messages.CogsNotDefinedForInventoryItem, laborItem.InventoryCD.Trim());
			}

			return laborItem.COGSAcctID;
		}
		
		protected virtual int? GetCostAccountForProject(PMProject project, PMTask task, InventoryItem laborItem, EPEmployee employee)
		{
			if (project == null) throw new ArgumentNullException(nameof(project));
			if (task == null) throw new ArgumentNullException(nameof(task));
			if (laborItem == null) throw new ArgumentNullException(nameof(laborItem));
			if (employee == null) throw new ArgumentNullException(nameof(employee));

			int? accountID = null;
			
			if (ExpenseAccountSource == PMAccountSource.Project)
			{
				if (project.DefaultExpenseAccountID != null)
				{
					accountID = project.DefaultExpenseAccountID;
				}
				else
				{
					PXTrace.WriteWarning(EP.Messages.NoDefualtAccountOnProject2, project.ContractCD.Trim());
				}
			}
			else if (ExpenseAccountSource == PMAccountSource.Task)
			{
				if (task.DefaultExpenseAccountID != null)
				{
					accountID = task.DefaultExpenseAccountID;
				}
				else
				{
					PXTrace.WriteWarning(EP.Messages.NoDefualtAccountOnTask2, project.ContractCD.Trim(), task.TaskCD.Trim());
				}
			}
			else if (ExpenseAccountSource == PMAccountSource.Employee)
			{
				if (employee.ExpenseAcctID != null)
				{
					accountID = employee.ExpenseAcctID;
				}
				else
				{
					PXTrace.WriteWarning(EP.Messages.NoExpenseAccountOnEmployee2, employee.AcctCD.Trim());
				}
			}
			else //InventoryItem
			{
				if (laborItem.COGSAcctID == null)
				{
					PXTrace.WriteWarning(EP.Messages.CogsNotDefinedForInventoryItem2, laborItem.InventoryCD.Trim());
				}

				accountID = laborItem.COGSAcctID;
			}

			if (accountID == null)
			{
				throw new PXException(Messages.FailedToDetermineCostAccount2);
			}

			return accountID;
		}

		protected virtual int? GetOffsetAccount(PMProject project, PMTask task, InventoryItem laborItem, EPEmployee employee)
		{
			if (project == null) throw new ArgumentNullException(nameof(project));
			
			if (project.NonProject == true)
			{
				return GetOffsetAccountForNonProject(laborItem);
			}
			else
			{
				return GetOffsetAccountForProject(project, task, laborItem, employee);
			}
		}

		private int? GetOffsetAccountForNonProject(InventoryItem laborItem)
		{
			if (laborItem.InvtAcctID == null)
			{
				throw new PXException(EP.Messages.InventoryAccountNotDefinedForInventoryItem, laborItem.InventoryCD.Trim());
			}

			return laborItem.InvtAcctID;
		}

		protected virtual int? GetOffsetAccountForProject(PMProject project, PMTask task, InventoryItem laborItem, EPEmployee employee)
		{
			int? accountID = null;
			
			if (ExpenseAccrualAccountSource == PMAccountSource.Project)
			{
				if (project.DefaultAccrualAccountID != null)
				{
					accountID = project.DefaultAccrualAccountID;
				}
				else
				{
					PXTrace.WriteWarning(EP.Messages.NoDefualtAccrualAccountOnProject2, project.ContractCD.Trim());
				}
			}
			else if (ExpenseAccrualAccountSource == PMAccountSource.Task)
			{
				if (task != null && task.DefaultAccrualAccountID != null)
				{
					accountID = task.DefaultAccrualAccountID;
				}
				else
				{
					PXTrace.WriteWarning(EP.Messages.NoDefualtAccrualAccountOnTask2, project.ContractCD.Trim(), task.TaskCD.Trim());
				}
			}
			else if (ExpenseAccrualAccountSource == PMAccountSource.Employee)
			{
				if (employee.ExpenseAcctID != null)
				{
					accountID = employee.ExpenseAcctID;
				}
				else
				{
					PXTrace.WriteWarning(EP.Messages.NoExpenseAccountOnEmployee, employee.AcctCD.Trim());
				}
			}
			else //InventoryItem
			{
				if (laborItem.InvtAcctID == null)
				{
					PXTrace.WriteWarning(EP.Messages.InventoryAccountNotDefinedForInventoryItem2, laborItem.InventoryCD.Trim());
				}

				accountID = laborItem.InvtAcctID;
			}

			if (accountID == null)
			{
				throw new PXException(Messages.FailedToDetermineAccrualAccount);
			}

			return accountID;
		}

		private int? GetAccountGroupFromAccount(int? accountID)
        {
			Account account = Account.PK.Find(this, accountID);
			if (account.AccountGroupID == null)
			{
				throw new PXException(Messages.AccountIsNotMappedToAccountGroup, account.AccountCD);
			}

			return account.AccountGroupID;
		}

		protected virtual string CombineCostSubAccount(PMProject project, PMTask task, InventoryItem laborItem, EPEmployee employee)
        {
			return PM.SubAccountMaskAttribute.MakeSub<PMSetup.expenseSubMask>(this, ExpenseSubMask,
							new object[] { laborItem.COGSSubID, project.DefaultExpenseSubID, task.DefaultExpenseSubID, employee.ExpenseSubID },
							new Type[] { typeof(InventoryItem.cOGSSubID), typeof(Contract.defaultExpenseSubID), typeof(PMTask.defaultExpenseSubID), typeof(EPEmployee.expenseSubID) });
		}

		protected virtual string CombineOffsetSubAccount(PMProject project, PMTask task, InventoryItem laborItem, EPEmployee employee)
		{
			return PM.SubAccountMaskAttribute.MakeSub<PMSetup.expenseAccrualSubMask>(this, ExpenseAccrualSubMask,
							new object[] { laborItem.InvtSubID, project.DefaultAccrualSubID, task.DefaultAccrualSubID, employee.ExpenseSubID },
							new Type[] { typeof(InventoryItem.invtSubID), typeof(Contract.defaultAccrualSubID), typeof(PMTask.defaultAccrualSubID), typeof(EPEmployee.expenseSubID) });
		}

		private void ValidateExpenseSubMask(PMProject project, PMTask task, InventoryItem laborItem, EPEmployee employee)
        {
			if (!string.IsNullOrEmpty(ExpenseSubMask))
			{
				if (ExpenseSubMask.Contains(PMAccountSource.InventoryItem) && laborItem.COGSSubID == null)
				{
					PXTrace.WriteError(EP.Messages.NoExpenseSubOnInventory, laborItem.InventoryCD.Trim());
					throw new PXException(EP.Messages.NoExpenseSubOnInventory, laborItem.InventoryCD.Trim());
				}
				if (ExpenseSubMask.Contains(PMAccountSource.Project) && project.DefaultExpenseSubID == null)
				{
					PXTrace.WriteError(EP.Messages.NoExpenseSubOnProject, project.ContractCD.Trim());
					throw new PXException(EP.Messages.NoExpenseSubOnProject, project.ContractCD.Trim());
				}
				if (ExpenseSubMask.Contains(PMAccountSource.Task) && task.DefaultExpenseSubID == null)
				{
					PXTrace.WriteError(EP.Messages.NoExpenseSubOnTask, project.ContractCD.Trim(), task.TaskCD.Trim());
					throw new PXException(EP.Messages.NoExpenseSubOnTask, project.ContractCD.Trim(), task.TaskCD.Trim());
				}
				if (ExpenseSubMask.Contains(PMAccountSource.Employee) && employee.ExpenseSubID == null)
				{
					PXTrace.WriteError(EP.Messages.NoExpenseSubOnEmployee, employee.AcctCD.Trim());
					throw new PXException(EP.Messages.NoExpenseSubOnEmployee, employee.AcctCD.Trim());
				}
			}
		}

		private void ValidateExpenseAccrualSubMask(PMProject project, PMTask task, InventoryItem laborItem, EPEmployee employee)
		{
			if (!string.IsNullOrEmpty(ExpenseAccrualSubMask))
			{
				if (ExpenseAccrualSubMask.Contains(PMAccountSource.InventoryItem) && laborItem.InvtSubID == null)
				{
					PXTrace.WriteError(EP.Messages.NoExpenseAccrualSubOnInventory, laborItem.InventoryCD.Trim());
					throw new PXException(EP.Messages.NoExpenseAccrualSubOnInventory, laborItem.InventoryCD.Trim());
				}
				if (ExpenseAccrualSubMask.Contains(PMAccountSource.Project) && project.DefaultAccrualSubID == null)
				{
					PXTrace.WriteError(EP.Messages.NoExpenseAccrualSubOnProject, project.ContractCD.Trim());
					throw new PXException(EP.Messages.NoExpenseAccrualSubOnProject, project.ContractCD.Trim());
				}
				if (ExpenseAccrualSubMask.Contains(PMAccountSource.Task) && task.DefaultAccrualSubID == null)
				{
					PXTrace.WriteError(EP.Messages.NoExpenseAccrualSubOnTask, project.ContractCD.Trim(), task.TaskCD.Trim());
					throw new PXException(EP.Messages.NoExpenseAccrualSubOnTask, project.ContractCD.Trim(), task.TaskCD.Trim());
				}
				if (ExpenseAccrualSubMask.Contains(PMAccountSource.Employee) && employee.ExpenseSubID == null)
				{
					PXTrace.WriteError(EP.Messages.NoExpenseSubOnEmployee, employee.AcctCD.Trim());
					throw new PXException(EP.Messages.NoExpenseSubOnEmployee, employee.AcctCD.Trim());
				}
			}
		}

		private void WriteWarningsToTrace(PMProject project, PMTask task)
		{
			if (project.IsActive != true)
			{
				PXTrace.WriteWarning(EP.Messages.ProjectIsNotActive, project.ContractCD.Trim());
			}
			if (project.IsCompleted == true)
			{
				PXTrace.WriteWarning(EP.Messages.ProjectIsCompleted, project.ContractCD.Trim());
			}

			if (task != null && task.IsActive != true)
			{
				PXTrace.WriteWarning(EP.Messages.ProjectTaskIsNotActive, project.ContractCD.Trim(), task.TaskCD.Trim());
			}
			if (task != null && task.IsCompleted == true)
			{
				PXTrace.WriteWarning(EP.Messages.ProjectTaskIsCompleted, project.ContractCD.Trim(), task.TaskCD.Trim());
			}
			if (task != null && task.IsCancelled == true)
			{
				PXTrace.WriteWarning(EP.Messages.ProjectTaskIsCancelled, project.ContractCD.Trim(), task.TaskCD.Trim());
			}
		}

		private bool GetPostToOffbalance(int? employeeID)
        {
			if (!PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
				return true;

			return EPSetupMaint.GetPostToOffBalance(this, epSetup.Current, employeeID);
		}

		private string GetActivityTimeUOM()
        {
			string ActivityTimeUnit = EPSetup.Minute;
			if (!string.IsNullOrEmpty(epSetup.Current.ActivityTimeUnit))
			{
				ActivityTimeUnit = epSetup.Current.ActivityTimeUnit;
			}

			return ActivityTimeUnit;
		}

		private bool IsNonGlTransaction(int? employeeID)
        {
			string postingOption = EPSetupMaint.GetPostingOption(this, epSetup.Current, employeeID);

			return postingOption != EPPostOptions.Post && postingOption != EPPostOptions.OverridePMAndGLInPayroll;
		}

		private bool CanCreateTransaction(PMTimeActivity timeActivity, int? timeSpent, int? timeBillable)
        {
			if (timeActivity.ApprovalStatus == ActivityStatusAttribute.Canceled)
				return false;

			if (timeSpent.GetValueOrDefault() == 0 && timeBillable.GetValueOrDefault() == 0)
				return false;

			return true;
		}

		private decimal GetConvertedAndRoundedTime(decimal? time, string itemBaseUnit)
        {
			string uom = GetActivityTimeUOM();

			decimal qty = time.GetValueOrDefault();
			if (qty > 0 && epSetup.Current.MinBillableTime > qty)
				qty = (decimal)epSetup.Current.MinBillableTime;
			try
			{
				qty = INUnitAttribute.ConvertGlobalUnits(this, uom, itemBaseUnit, qty, INPrecision.QUANTITY);
			}
			catch (PXException ex)
			{
				PXTrace.WriteError(ex);
				throw ex;
			}

			return qty;
		}

		public virtual PMTran CreateContractUsage(PMTimeActivity timeActivity, int billableMinutes)
        {
            if (timeActivity.ApprovalStatus == ActivityStatusAttribute.Canceled)
                return null;

            if (timeActivity.RefNoteID == null)
                return null;

			if (timeActivity.IsBillable != true)
				return null;

	        CRCase refCase = PXSelectJoin<CRCase,
		        InnerJoin<CRActivityLink,
			        On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>,
		        Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>>>.Select(this, timeActivity.RefNoteID);
            
            if (refCase == null)
                throw new Exception(CR.Messages.CaseCannotBeFound);

            CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.Select(this, refCase.CaseClassID);

			if (caseClass.PerItemBilling != BillingTypeListAttribute.PerActivity)
                return null;//contract-usage will be created as a result of case release.

            Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, refCase.ContractID);
            if (contract == null)
                return null;//activity has no contract and will be billed through Project using the cost-transaction. Contract-Usage is not created in this case.

            ValidateContractBaseCurrency(contract);

            int? laborItemID = CRCaseClassLaborMatrix.GetLaborClassID(this, caseClass.CaseClassID, timeActivity.EarningTypeID);

            if (laborItemID == null)
                laborItemID = EP.EPContractRate.GetContractLaborClassID(this, timeActivity);

            if (laborItemID == null)
            {
                EP.EPEmployee employeeSettings = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.defContactID, Equal<Required<PMTimeActivity.ownerID>>>>.Select(this, timeActivity.OwnerID);
                if (employeeSettings != null)
                {
                    laborItemID = EP.EPEmployeeClassLaborMatrix.GetLaborClassID(this, employeeSettings.BAccountID, timeActivity.EarningTypeID) ??
                                  employeeSettings.LabourItemID;
                }
            }

            InventoryItem laborItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, laborItemID);

            if (laborItem == null)
            {
                throw new PXException(CR.Messages.LaborNotConfigured);
            }

			//save the sign of the value and do the rounding against absolute value.
			//reuse sign later when setting value to resulting transaction.
	        int sign = billableMinutes < 0 ? -1 : 1; 
	        billableMinutes = Math.Abs(billableMinutes);
			
			if (caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity && caseClass.RoundingInMinutes > 1)
            {
				decimal fraction = Convert.ToDecimal(billableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
                int points = Convert.ToInt32(Math.Ceiling(fraction));
				billableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
            }

			if (billableMinutes > 0 && caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity && caseClass.MinBillTimeInMinutes > 0)
            {
				billableMinutes = Math.Max(billableMinutes, (int)caseClass.MinBillTimeInMinutes);
            }
			
            if (billableMinutes > 0)
            {
				PMTran newLabourTran = new PMTran();
                newLabourTran.ProjectID = refCase.ContractID;
                newLabourTran.InventoryID = laborItem.InventoryID;
                newLabourTran.AccountGroupID = contract.ContractAccountGroup;
                newLabourTran.OrigRefID = timeActivity.NoteID;
                newLabourTran.BAccountID = refCase.CustomerID;
                newLabourTran.LocationID = refCase.LocationID;
                newLabourTran.Description = timeActivity.Summary;
                newLabourTran.StartDate = timeActivity.Date;
                newLabourTran.EndDate = timeActivity.Date;
                newLabourTran.Date = timeActivity.Date;
                newLabourTran.UOM = laborItem.SalesUnit;
                newLabourTran.Qty = sign * Convert.ToDecimal(TimeSpan.FromMinutes(billableMinutes).TotalHours);
                newLabourTran.BillableQty = newLabourTran.Qty;
                newLabourTran.Released = true;
                newLabourTran.ExcludedFromAllocation = true;
                newLabourTran.IsQtyOnly = true;
                newLabourTran.BillingID = contract.BillingID;
				newLabourTran.CaseCD = refCase.CaseCD;
				return this.Transactions.Insert(newLabourTran);
            }
            else
            {
                return null;
            }
        }

		public override void CopyPasteGetScript(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers)
		{
			//move useBillableQty to prevent overriding amount by formula
			var useBillableQty = script.Where(_ => _.FieldName == nameof(PMTran.UseBillableQty)).SingleOrDefault();
			var tranCuryAmountIndex = script.FindIndex(_ => _.FieldName == nameof(PMTran.TranCuryAmount));
			if (useBillableQty != null && tranCuryAmountIndex >= 0)
			{
				script.Remove(useBillableQty);
				script.Insert(tranCuryAmountIndex, useBillableQty);
			}
			//Move all Dodument&Details to begin
			int index = 0;
			for (int i=0; i<containers.Count; i++)
			{
				var c = containers[i];
				var s = script[i];
				if (containers[i].ViewName() == nameof(this.Transactions) || 
				    containers[i].ViewName() == nameof(this.Document))
				{
					containers.RemoveAt(i);
					containers.Insert(index, c);
					script.RemoveAt(i);
					script.Insert(index, s);
					index++;
				}
			}
		}

		protected virtual bool IsAllPMTranLinesVisible(PMRegister doc)
		{
			int tranCount = SelectFrom<PMTran>.
				Where<PMTran.tranType.IsEqual<@P.AsString>.And<PMTran.refNbr.IsEqual<@P.AsString>>>.
				View.Select(this, doc.Module, doc.RefNbr).Count();

			int visibleTranCount =
				SelectFrom<PMTran>.
				LeftJoin<Account>.On<
					Account.accountID.IsEqual<PMTran.offsetAccountID>>.
				LeftJoin<PMAccountGroup>.On<
					PMAccountGroup.groupID.IsEqual<PMTran.accountGroupID>>.
				LeftJoin<RegisterReleaseProcess.OffsetPMAccountGroup>.On<
					RegisterReleaseProcess.OffsetPMAccountGroup.groupID.IsEqual<Account.accountGroupID>>.
				Where<
					PMTran.tranType.IsEqual<@P.AsString>.
					And<PMTran.refNbr.IsEqual<@P.AsString>>.
					And<
						RegisterReleaseProcess.OffsetPMAccountGroup.groupID.IsNull.
						Or<Match<RegisterReleaseProcess.OffsetPMAccountGroup, AccessInfo.userName.FromCurrent>>>.
					And<
						PMAccountGroup.groupID.IsNull.
						Or<Match<PMAccountGroup, AccessInfo.userName.FromCurrent>>>>.
				View.Select(this, doc.Module, doc.RefNbr).Count();

			return tranCount == visibleTranCount;
		}

		[PXBreakInheritance]
        [Serializable]
        [PXHidden]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class RecurringItemEx : PMRecurringItem
		{
			#region ProjectID
			public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			[PXDBInt(IsKey = true)]
			public override Int32? ProjectID
			{
				get;
				set;
			}
			#endregion
			#region TaskID
			public new abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

			[PXDBInt(IsKey = true)]
			public override Int32? TaskID
			{
				get;
				set;
			}
			#endregion
			#region InventoryID
			public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			[PXDBInt(IsKey = true)]
			public override Int32? InventoryID
			{
				get;
				set;
			}
			#endregion
		}

		public class CreatePMTran
		{
			public CreatePMTran(PMTimeActivity timeActivity, int? employeeID, DateTime date, int? timeSpent, int? timeBillable, decimal? cost, decimal? overtimeMult, string tranCuryID, bool insertTransaction)
			{
				TimeActivity = timeActivity;
				EmployeeID = employeeID;
				Date = date;
				TimeSpent = timeSpent;
				TimeBillable = timeBillable;
				Cost = cost;
				OvertimeMult = overtimeMult;
				TranCuryID = tranCuryID;
				InsertTransaction = insertTransaction;
			}

			public PMTimeActivity TimeActivity { get; }
			public int? EmployeeID { get; }
			public DateTime Date { get; }
			public int? TimeSpent { get; }
			public int? TimeBillable { get; }
			public decimal? Cost { get; }
			public decimal? OvertimeMult { get; }
			public string TranCuryID { get; }
			public bool InsertTransaction { get; }
		}
	}
}
