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
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL.DAC;
using static PX.Data.BQL.BqlPlaceholder;
using AccountTypeList = PX.Objects.GL.AccountType;

namespace PX.Objects.GL
{
    public class JournalEntryImport : PXGraph<JournalEntryImport, GLTrialBalanceImportMap>, PXImportAttribute.IPXPrepareItems
    {
        #region OperationParam

        [Serializable]
        public partial class OperationParam : IBqlTable
        {
            public abstract class action : PX.Data.BQL.BqlString.Field<action> { }

            protected String _Action;
            [PXDefault(_VALIDATE_ACTION)]
            public String Action
            {
                get { return _Action; }
                set { _Action = value; }
            }
        }

        #endregion

        #region TrialBalanceTemplate
        //Alias
        [Serializable]
        public partial class TrialBalanceTemplate : GLTrialBalanceImportDetails
        {
            public new abstract class mapNumber : PX.Data.BQL.BqlString.Field<mapNumber> { }
            public new abstract class line : PX.Data.BQL.BqlInt.Field<line> { }
			public new abstract class importBranchCD : PX.Data.BQL.BqlString.Field<importBranchCD> { }
			public new abstract class mapBranchID : PX.Data.BQL.BqlInt.Field<mapBranchID> { }
            public new abstract class importAccountCD : PX.Data.BQL.BqlString.Field<importAccountCD> { }
            public new abstract class mapAccountID : PX.Data.BQL.BqlInt.Field<mapAccountID> { }
            public new abstract class importSubAccountCD : PX.Data.BQL.BqlString.Field<importSubAccountCD> { }
            public new abstract class mapSubAccountID : PX.Data.BQL.BqlInt.Field<mapSubAccountID> { }
            public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
            public new abstract class status : PX.Data.BQL.BqlInt.Field<status> { }
            public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }
        }

        #endregion

        #region GLHistoryEnquiryWithSubResult

        [Serializable]
        public partial class GLHistoryEnquiryWithSubResult : GLHistoryEnquiryResult
        {
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			/// <summary>
			/// A reference to the <see cref="Branch"/> to which the history belongs.
			/// </summary>
			/// <value>
			/// Corresponds to the <see cref="Branch.BranchID"/> field.
			/// </value>
			[Branch(IsKey = true, FieldClass = BranchAttribute._DimensionName)]
			public virtual Int32? BranchID { get; set; }
			#endregion

            public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

            #region SubID
            public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

            protected Int32? _SubID;
            [SubAccount(typeof(accountID))]
            public virtual Int32? SubID
            {
                get { return _SubID; }
                set { _SubID = value; }
            }
            #endregion

            #region AccountType
            public abstract class accountType : PX.Data.BQL.BqlString.Field<accountType> { }

            protected String _AccountType;
            [PXDBString(1)]
            [PXDefault(AccountTypeList.Asset)]
            [AccountTypeList.List()]
            [PXUIField(DisplayName = "Account Type")]
            public virtual String AccountType
            {
                get { return _AccountType; }
                set { _AccountType = value; }
            }

            #endregion
        }

        #endregion

        #region JournalEntryImportProcessing

        public class JournalEntryImportProcessing : PXGraph<JournalEntryImportProcessing>
        {
            #region Fields

            public PXSetup<Company> CompanySetup;

            public PXSetup<GLSetup> GLSetup;

			public PXSelect<CurrencyInfo> CurrencyInfoView;
            
			public PXSelect<Batch> Batch;

            public PXSelect<GLTran, Where<GLTran.batchNbr, Equal<Current<Batch.batchNbr>>>> GLTrans;

            public PXSelect<GLTrialBalanceImportMap> Map;

            public PXSelect<GLTrialBalanceImportDetails,
                Where<GLTrialBalanceImportDetails.mapNumber, Equal<Current<GLTrialBalanceImportMap.number>>>> MapDetails;

	        public PXSetup<Ledger, Where<Ledger.ledgerID, Equal<Optional<Batch.ledgerID>>>> LedgerView;

	        private Dictionary<string, CurrencyInfo> _curyInfosByCuryID;

            #endregion

	        public JournalEntryImportProcessing()
	        {
		        _curyInfosByCuryID = new Dictionary<string, CurrencyInfo>();
	        }

	        #region ReleaseImport

            public static void ReleaseImport(object mapNumber, bool isReversedSign)
            {
                var graph = (JournalEntryImportProcessing)PXGraph.CreateInstance(typeof(JournalEntryImportProcessing));
				JournalEntry journalEntry = (JournalEntry)PXGraph.CreateInstance(typeof(JournalEntry));

                GLTrialBalanceImportMap map = graph.Map.Search<GLTrialBalanceImportMap.number>(mapNumber);
                if (map == null) return;

                Batch newBatch = new Batch();
                graph.Map.Current = map;

                using (new PXConnectionScope())
                {
                    using (var ts = new PXTransactionScope())
                    {
                        List<GLTrialBalanceImportDetails> details = new List<GLTrialBalanceImportDetails>();
                        foreach (GLTrialBalanceImportDetails item in graph.MapDetails.Select())
                            details.Add(item);
                        var refNumber = _TRAN_REFNUMBER_PREFIX + mapNumber;
						Ledger ledger = GetLedger(graph, map.LedgerID);

						int? firstLineBranchID = details.Count > 0 ? details.First().MapBranchID : null;

						newBatch.BranchID = firstLineBranchID;
						newBatch.Module = BatchModule.GL;
						newBatch.BatchType = BatchTypeCode.TrialBalance;
						newBatch.DateEntered = map.ImportDate;
						newBatch = (Batch)journalEntry.BatchModule.Cache.Insert(newBatch);
						newBatch.FinPeriodID = map.FinPeriodID;
						FinPeriodIDAttribute.SetMasterPeriodID<Batch.finPeriodID>(journalEntry.BatchModule.Cache, newBatch);
						newBatch.LedgerID = map.LedgerID;
						newBatch.Description = map.Description;
                        newBatch.DebitTotal = 0m;
                        newBatch.CreditTotal = 0m;

	                    var curyInfo = graph.GetOrCreateCuryInfo(ledger.BaseCuryID, ledger.BaseCuryID);

	                    newBatch.CuryInfoID = curyInfo.CuryInfoID;
	                    newBatch.CuryID = curyInfo.CuryID;

						CurrencyInfo batchCuryInfo = journalEntry.currencyInfo;
						if (batchCuryInfo.BaseCuryID != curyInfo.BaseCuryID && batchCuryInfo.CuryID != curyInfo.CuryID)
						{
							journalEntry.currencyinfo.Update(curyInfo);
						}

                        foreach (var item in
                            GetBalances(graph, isReversedSign, map.OrgBAccountID, map.LedgerID,
                                        map.FinPeriodID, map.BegFinPeriod))
                        {
                            GLTrialBalanceImportDetails importItem = null;
                            for (int index = 0; index < details.Count; index++)
                            {
                                GLTrialBalanceImportDetails detail = details[index];
                                if (detail.MapBranchID == item.BranchID && detail.MapAccountID == item.AccountID && detail.MapSubAccountID == item.SubID)
                                {
                                    importItem = detail;
                                    details.RemoveAt(index);
                                    break;
                                }
                            }

                            decimal diff = (importItem == null ? 0m : (decimal)importItem.YtdBalance) - (decimal)item.EndBalance;
                            decimal curyDiff = (importItem == null ? 0m : (decimal)importItem.CuryYtdBalance) - (decimal)item.CuryEndBalance;
                            if (diff == 0m && curyDiff == 0) continue;

							var account = GetAccount(graph, item.AccountID);

							GLTran tran = new GLTran();
							tran.BranchID = item.BranchID;
							tran.AccountID = account.AccountID;
                            tran.SubID = item.SubID;
                            FillDebitAndCreditAmt(tran, diff, curyDiff, isReversedSign, item.AccountType);
                            tran.RefNbr = refNumber;
							tran.CuryInfoID = graph.GetOrCreateCuryInfoForTran(account, ledger);
							tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();
							tran = (GLTran) journalEntry.GLTranModuleBatNbr.Cache.Insert(tran);
						}
                        foreach (var item in details)
                        {
                            decimal diff = (decimal)item.YtdBalance;
                            decimal curyDiff = (decimal)item.CuryYtdBalance;
                            if (diff == 0m) continue;

							var account = GetAccount(graph, item.MapAccountID);

							GLTran tran = new GLTran();
							tran.BranchID = item.MapBranchID;
							tran.AccountID = account.AccountID;
                            tran.SubID = item.MapSubAccountID;
							FillDebitAndCreditAmt(tran, diff, curyDiff, isReversedSign, account.Type);
                            tran.RefNbr = refNumber;
							tran.CuryInfoID = graph.GetOrCreateCuryInfoForTran(account, ledger);
							tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();
							tran = (GLTran)journalEntry.GLTranModuleBatNbr.Cache.Insert(tran);
						}
                        newBatch.ControlTotal = (newBatch.DebitTotal == newBatch.CreditTotal) ? newBatch.DebitTotal : 0m;
                        newBatch.Hold = false;
						newBatch = (Batch)journalEntry.BatchModule.Cache.Update(newBatch);
						journalEntry.Persist();

                        ts.Complete();
                    }
                }

                using (new PXTimeStampScope(null))
                {
                    newBatch = graph.Batch.Search<Batch.batchNbr, Batch.module>(newBatch.BatchNbr, newBatch.Module);
					if (graph.Map.Current != null)
					{
						graph.Map.Current.BatchNbr = newBatch.BatchNbr;
						graph.Map.Cache.MarkUpdated(graph.Map.Current);
						graph.Persist();
					}
                    graph.Clear();
                    PXRedirectHelper.TryRedirect(graph.Batch.Cache, newBatch, Messages.ViewBatch);
                }
            }

			private long? GetOrCreateCuryInfoForTran(Account account, Ledger ledger)
			{
				if (account == null)
					return null;

				var curyID = account.CuryID != null && ledger.BalanceType != LedgerBalanceType.Report
								? account.CuryID
								: ledger.BaseCuryID;

				var curyInfo = GetOrCreateCuryInfo(curyID, ledger.BaseCuryID);

				return curyInfo.CuryInfoID;
			}

			private CurrencyInfo GetOrCreateCuryInfo(string curyID, string baseCuryID)
	        {
		        if (_curyInfosByCuryID.ContainsKey(curyID))
			        return _curyInfosByCuryID[curyID];

				var curyInfo = new CurrencyInfo
				{
					BaseCuryID = baseCuryID,
					CuryID = curyID,
					BaseCalc = false,
					CuryRate = 1,
					RecipRate = 1
				};

		        curyInfo = CurrencyInfoView.Insert(curyInfo);
		        CurrencyInfoView.Cache.PersistInserted(curyInfo);
				CurrencyInfoView.Cache.Persisted(false);

				_curyInfosByCuryID.Add(curyInfo.CuryID, curyInfo);

		        return curyInfo;
	        }

            #endregion
        }

        #endregion

        #region Fields

        #region Constants

        private const string _VALIDATE_ACTION = "Validate";
        private const string _MERGE_DUPLICATES_ACTION = "Merge Duplicates";
        private const string _IMPORTTEMPLATE_VIEWNAME = "ImportTemplate";
        private const string _TRAN_REFNUMBER_PREFIX = "";

        #endregion

        private readonly string _mapNumberFieldName;
        private readonly string _importFieldNameAccount;

        [PXHidden]
        public PXSetup<GLSetup> GLSetup;

        [PXHidden]
        public PXFilter<OperationParam> Operations;

        public PXSelect<GLTrialBalanceImportMap> Map;

        [PXFilterable]
        public PXSelect<GLTrialBalanceImportDetails,
            Where<GLTrialBalanceImportDetails.mapNumber, Equal<Current<GLTrialBalanceImportMap.number>>>> MapDetails;

        [PXImport(typeof(GLTrialBalanceImportMap))]
		public PXSelect<TrialBalanceTemplate,
			Where<TrialBalanceTemplate.mapNumber, Equal<Current<GLTrialBalanceImportMap.number>>>> ImportTemplate;

        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelectOrderBy<GLHistoryEnquiryWithSubResult,
            OrderBy<Asc<GLHistoryEnquiryWithSubResult.accountID, Asc<GLHistoryEnquiryWithSubResult.subID>>>> Exceptions;

		public PXSetup<Ledger,
							Where<Ledger.ledgerID, Equal<Optional<GLTrialBalanceImportMap.ledgerID>>>> Ledger;

        #endregion

        #region Ctors

        public JournalEntryImport()
        {
			var importAttribute = ImportTemplate.GetAttribute<PXImportAttribute>();
			importAttribute.MappingPropertiesInit += MappingPropertiesInit;

            _mapNumberFieldName = ImportTemplate.Cache.GetField(typeof(TrialBalanceTemplate.mapNumber));
            _importFieldNameAccount = ImportTemplate.Cache.GetField(typeof(GLTrialBalanceImportDetails.importAccountCD));

            MapDetails.Cache.AllowInsert = true;
            MapDetails.Cache.AllowUpdate = true;
            MapDetails.Cache.AllowDelete = true;
            OpenPeriodAttribute.SetValidatePeriod<GLTrialBalanceImportMap.finPeriodID>(Map.Cache, null, PeriodValidation.DefaultSelectUpdate);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.selected>(MapDetails.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.importBranchCD>(MapDetails.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.importAccountCD>(MapDetails.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.importSubAccountCD>(MapDetails.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.ytdBalance>(MapDetails.Cache, null, true);
            PXUIFieldAttribute.SetReadOnly<GLTrialBalanceImportDetails.status>(MapDetails.Cache, null);
            
            PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.mapNumber>(ImportTemplate.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.mapBranchID>(ImportTemplate.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.mapAccountID>(ImportTemplate.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.mapSubAccountID>(ImportTemplate.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.selected>(ImportTemplate.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.status>(ImportTemplate.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.description>(ImportTemplate.Cache, null, false);
			PXUIFieldAttribute.SetDisplayName<TrialBalanceTemplate.importBranchCD>(ImportTemplate.Cache, Messages.Branch);
			PXUIFieldAttribute.SetDisplayName<TrialBalanceTemplate.importAccountCD>(ImportTemplate.Cache, Messages.FieldNameAccount);
            PXUIFieldAttribute.SetDisplayName<TrialBalanceTemplate.importSubAccountCD>(ImportTemplate.Cache, Messages.Sub);

	        PXUIFieldAttribute.SetReadOnly(Exceptions.Cache, null);
        }

        #endregion

        #region Actions
        public PXInitializeState<GLTrialBalanceImportMap> initializeState;
		
        public PXAction<GLTrialBalanceImportMap> putOnHold;
        [PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable PutOnHold(PXAdapter adapter) => adapter.Get();

        public PXAction<GLTrialBalanceImportMap> releaseFromHold;
        [PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable ReleaseFromHold(PXAdapter adapter) => adapter.Get();

        public PXAction<GLTrialBalanceImportMap> process;
        [PXUIField(DisplayName = Messages.Process)]
        [PXButton]
        protected virtual IEnumerable Process(PXAdapter adapter)
        {
            if (CanEdit)
            {
                Dictionary<int, GLTrialBalanceImportDetails> dict = new Dictionary<int, GLTrialBalanceImportDetails>();
                foreach (GLTrialBalanceImportDetails item in MapDetails.Select(Operations.Current.Action))
                {   
                    if (item.Selected == true && item.Line != null)
                    {
                        int line = (int)item.Line;
                        if (!dict.ContainsKey(line))
                        {   dict.Add(line, (GLTrialBalanceImportDetails)MapDetails.Cache.CreateCopy(item));
                        }
                    }
                }
                if (Operations.Current != null)
                {   ProcessHandler(dict, Operations.Current.Action, true);
                }
            }
            return adapter.Get();
        }

        public PXAction<GLTrialBalanceImportMap> processAll;
        [PXUIField(DisplayName = Messages.ProcessAll)]
        [PXButton]
        protected virtual IEnumerable ProcessAll(PXAdapter adapter)
        {
            if (CanEdit)
            {
                Dictionary<int, GLTrialBalanceImportDetails> dict = new Dictionary<int, GLTrialBalanceImportDetails>();
                foreach (GLTrialBalanceImportDetails item in MapDetails.Select(Operations.Current.Action))
                {
                    if (item.Line != null)
                    {
                        int line = (int)item.Line;
                        if (!dict.ContainsKey(line))
                        {
                            item.Selected = true;
                            dict.Add(line, (GLTrialBalanceImportDetails)MapDetails.Cache.CreateCopy(item));
                        }
                    }
                }
                if (Operations.Current != null)
                {   ProcessHandler(dict, Operations.Current.Action, true);
                }
            }
            return adapter.Get();
        }

        public PXAction<GLTrialBalanceImportMap> release;
        [PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable Release(PXAdapter adapter)
        {
            GLTrialBalanceImportMap map = Map.Current;
            if (map != null)
            {
                if (map.Status != TrialBalanceImportMapStatusAttribute.Balanced)
                    throw new PXException(Messages.ImportStatusInvalid);

                if (map.CreditTotalBalance != map.TotalBalance)
					throw new Exception(Messages.DocumentIsOutOfBalancePleaseReview);
                if (map.DebitTotalBalance != map.TotalBalance)
					throw new Exception(Messages.DocumentIsOutOfBalancePleaseReview);

                PXResultset<GLTrialBalanceImportMap> res = PXSelectJoin<GLTrialBalanceImportMap,
                    InnerJoin<Batch,
                        On<Batch.batchNbr, Equal<GLTrialBalanceImportMap.batchNbr>,
                        And<Batch.module, Equal<BatchModule.moduleGL>,
                        And<Batch.posted, Equal<False>>>>>,
                    Where<GLTrialBalanceImportMap.finPeriodID, LessEqual<Current<GLTrialBalanceImportMap.finPeriodID>>>>.SelectSingleBound(this, null, null);
                if (res.Count > 0)
                {
                    throw new Exception(Messages.PreviousBatchesNotPosted);
                }

                Save.Press();
                bool isUnsignOperations = IsUnsignOperations(this);
                object mapNumber = Map.Current.Number;
                PXLongOperation.StartOperation(this,
                    delegate()
                    {
                        JournalEntryImportProcessing.ReleaseImport(mapNumber, isUnsignOperations);
                    });
            }

            yield return Map.Current;
        }

        #endregion

        #region Select Handlers

        protected virtual void mapDetails([PXString] ref string action)
        {
            if (action != null) Operations.Current.Action = action;
        }

        protected virtual IEnumerable exceptions()
        {
            if (Map.Current == null) yield break;

            foreach (GLHistoryEnquiryWithSubResult item in
                GetBalances(this, Map.Current.OrgBAccountID, Map.Current.LedgerID, Map.Current.FinPeriodID, Map.Current.BegFinPeriod))
            {
                if (item.EndBalance == 0m) continue;

                if (PXSelect<GLTrialBalanceImportDetails,
                    Where<GLTrialBalanceImportDetails.mapNumber, Equal<Required<GLTrialBalanceImportDetails.mapNumber>>,
						And<GLTrialBalanceImportDetails.mapBranchID, Equal<Required<GLTrialBalanceImportDetails.mapBranchID>>,
                        And<GLTrialBalanceImportDetails.mapAccountID, Equal<Required<GLTrialBalanceImportDetails.mapAccountID>>,
                        And<GLTrialBalanceImportDetails.mapSubAccountID, Equal<Required<GLTrialBalanceImportDetails.mapSubAccountID>>>>>>>.
                    Select(this, Map.Current.Number, item.BranchID, item.AccountID, item.SubID).Count > 0) continue;
                yield return item;
            }
        }

        #endregion

        #region Processing

        protected virtual void ProcessHandler(Dictionary<int, GLTrialBalanceImportDetails> dict, string operation, bool update)
        {
            switch (operation)
            {
                case _VALIDATE_ACTION:

                    foreach (GLTrialBalanceImportDetails item in dict.Values)
                    {
						bool validBranch = SetValue<GLTrialBalanceImportDetails.importBranchCD, GLTrialBalanceImportDetails.mapBranchID>(MapDetails.Cache, item, Messages.ImportBranchCDNotFound, Messages.ImportBranchCDIsEmpty);
                        bool validSubaccount = true;
                        bool validAccount = SetValue<GLTrialBalanceImportDetails.importAccountCD, GLTrialBalanceImportDetails.mapAccountID>(MapDetails.Cache, item, Messages.ImportAccountCDNotFound, Messages.ImportAccountCDIsEmpty);

                        if (!validAccount)
                        {
                            item.MapSubAccountID = null;
                            PersistErrorAttribute.ClearError<GLTrialBalanceImportDetails.importSubAccountCD>(MapDetails.Cache, item);
                        }
                        else if (PXAccess.FeatureInstalled<CS.FeaturesSet.subAccount>() == true)
                        {   validSubaccount = SetValue<GLTrialBalanceImportDetails.importSubAccountCD, GLTrialBalanceImportDetails.mapSubAccountID>(MapDetails.Cache, item, null, Messages.ImportSubAccountCDIsEmpty);
                        }

                        item.Status = validBranch && validAccount && validSubaccount ? TrialBalanceImportStatusAttribute.VALID : TrialBalanceImportStatusAttribute.ERROR;
                    }

                    foreach (GLTrialBalanceImportDetails item in dict.Values)
                    {
                        PXResultset<GLTrialBalanceImportDetails> duplicates = SearchDuplicates(item);
                        if (duplicates.Count >= 2)
                        {
                            if (item.Status != TrialBalanceImportStatusAttribute.ERROR)
                            {   item.Status = TrialBalanceImportStatusAttribute.DUPLICATE;
                            }
                        }
                        if (update)
                        {   MapDetails.Cache.Update(item);
                        }
                    }

					GLTrialBalanceImportMap currentMap = (GLTrialBalanceImportMap)Map.Cache.Current;
					PXFormulaAttribute.CalcAggregate<GLTrialBalanceImportDetails.ytdBalance>(MapDetails.Cache, currentMap);
					currentMap.DebitTotalBalance = (decimal?)PXFormulaAttribute.Evaluate<GLTrialBalanceImportMap.debitTotalBalance>(Map.Cache, currentMap);
					currentMap.CreditTotalBalance = (decimal?)PXFormulaAttribute.Evaluate<GLTrialBalanceImportMap.creditTotalBalance>(Map.Cache, currentMap);
					if (!IsRequireControlTotal)
					{
						currentMap.TotalBalance = currentMap.CreditTotalBalance;
					}
					Map.Cache.Update(currentMap);

					break;

                case _MERGE_DUPLICATES_ACTION:

                    foreach (GLTrialBalanceImportDetails item in dict.Values)
                    {
                        PXEntryStatus itemStatus = MapDetails.Cache.GetStatus(item);
                        if (itemStatus != PXEntryStatus.Deleted && itemStatus != PXEntryStatus.InsertedDeleted)
                        {
                            foreach (GLTrialBalanceImportDetails duplicate in SearchDuplicates(item))
                            {
                                if (duplicate.Line != null && duplicate.Line != item.Line && dict.ContainsKey((int)duplicate.Line))
                                {                                 
                                    item.YtdBalance += duplicate.YtdBalance;
                                    item.CuryYtdBalance += duplicate.CuryYtdBalance;
                                    MapDetails.Cache.Delete(duplicate);
                                }
                            }

                            if (item.Status != TrialBalanceImportStatusAttribute.ERROR)
                            {
								bool branchCDNotValidated = !string.IsNullOrEmpty(item.ImportBranchCD) && item.MapBranchID == null;
                                bool accountCDNotValidated = !string.IsNullOrEmpty(item.ImportAccountCD) && item.MapAccountID == null;
                                bool subAccountCDNotValidated = !string.IsNullOrEmpty(item.ImportSubAccountCD) && item.MapSubAccountID == null;
                                item.Status = (branchCDNotValidated || accountCDNotValidated || subAccountCDNotValidated) ? TrialBalanceImportStatusAttribute.NEW : TrialBalanceImportStatusAttribute.VALID;
                            }
                            
                            MapDetails.Cache.Update(item);
                        }
                    }
                    
                    break;
            }
        }

        #endregion

        #region Event Handlers
		
	    private void MappingPropertiesInit(object sender, PXImportAttribute.MappingPropertiesInitEventArgs e)
	    {
		    if (!IsCuryYtdBalanceFieldUsable())
		    {
				var fieldName = MapDetails.Cache.GetField(typeof(GLTrialBalanceImportDetails.curyYtdBalance));
				e.Names.Remove(fieldName);

				var displayName = PXUIFieldAttribute.GetDisplayName<GLTrialBalanceImportDetails.curyYtdBalance>(MapDetails.Cache);
				e.DisplayNames.Remove(displayName);
		    }
	    }


	    #region GLTrialBalanceImportMap

        protected virtual void GLTrialBalanceImportMap_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {   return;
            }

            GLTrialBalanceImportMap row = (GLTrialBalanceImportMap)e.Row;
            bool isEditable = IsEditable(row);
            if (isEditable)
            {   
                CheckTotalBalance(sender, row, IsRequireControlTotal);
            }

            PXUIFieldAttribute.SetVisible<GLTrialBalanceImportMap.totalBalance>(Map.Cache, null, IsRequireControlTotal);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.totalBalance>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.importDate>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.finPeriodID>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.description>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.ledgerID>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.orgBAccountID>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.isHold>(sender, row, isEditable);
            Map.Cache.AllowDelete = isEditable;
            Map.Cache.AllowUpdate = isEditable;
            MapDetails.Cache.AllowInsert = isEditable;
            MapDetails.Cache.AllowUpdate = isEditable;
            MapDetails.Cache.AllowDelete = isEditable;
            Actions["Process"].SetEnabled(isEditable);
            Actions["ProcessAll"].SetEnabled(isEditable);
            PXImportAttribute.SetEnabled(this, "ImportTemplate", isEditable);

			PXUIFieldAttribute.SetVisible<GLTrialBalanceImportDetails.curyYtdBalance>(MapDetails.Cache, null, IsCuryYtdBalanceFieldUsable());

			bool isBranchesNotBalancing = IsBranchesNotBalancing(row.OrgBAccountID);

			PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.importBranchCD>(MapDetails.Cache, null, isBranchesNotBalancing);
			PXUIFieldAttribute.SetVisible<GLTrialBalanceImportDetails.importBranchCD>(MapDetails.Cache, null, isBranchesNotBalancing);
			PXUIFieldAttribute.SetVisible<GLTrialBalanceImportDetails.mapBranchID>(MapDetails.Cache, null, isBranchesNotBalancing);

			PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.importBranchCD>(ImportTemplate.Cache, null, isBranchesNotBalancing);
		}

		protected virtual void _(Events.FieldVerifying<GLTrialBalanceImportMap, GLTrialBalanceImportMap.orgBAccountID> e)
		{
			if (e.Row == null || e.NewValue == null)
				return;

			int? orgBAccountID = e.NewValue as int?;
			Organization organization = PXSelect<Organization,
				Where<Organization.bAccountID, Equal<Required<Organization.bAccountID>>,
					And<Where<Organization.bAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
						Or<Organization.active, NotEqual<True>>>>>>
				.SelectSingleBound(this, null, orgBAccountID);

			if (organization != null)
			{
				if (organization.Active != true)
				{
					e.NewValue = PXOrgAccess.GetCD(orgBAccountID);
					throw new PXSetPropertyException(Messages.CompanyOrBranchIsInactive);
				}
				else if (organization.OrganizationType == OrganizationTypes.WithBranchesBalancing)
				{
					e.NewValue = PXOrgAccess.GetCD(orgBAccountID);
					throw new PXSetPropertyException(Messages.CompanyWithBranchesRequiringBalancingTypeCannotBeSelected);
				}
			}
			else
			{
				Branch branch = PXSelect<Branch,
					Where<Branch.bAccountID, Equal<Required<Branch.bAccountID>>,
						And<Where<Branch.bAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
							Or<Branch.active, NotEqual<True>>>>>>
					.SelectSingleBound(this, null, orgBAccountID);

				if (branch != null)
				{
					if (branch.Active != true)
					{
						e.NewValue = PXOrgAccess.GetCD(orgBAccountID);
						throw new PXSetPropertyException(Messages.CompanyOrBranchIsInactive);
					}
					else
					{
						Organization branchOrganization = OrganizationMaint.FindOrganizationByID(this, branch.OrganizationID);
						if (branchOrganization != null)
						{
							if (branchOrganization.OrganizationType == OrganizationTypes.WithBranchesNotBalancing)
							{
								e.NewValue = PXOrgAccess.GetCD(orgBAccountID);
								throw new PXSetPropertyException(Messages.BranchOfCompanyWithBranchesNotRequiringBalancingTypeCannotBeSelected);
							}
						}
					}
				}
				else
				{
					e.NewValue = PXOrgAccess.GetCD(orgBAccountID);
					throw new PXSetPropertyException(Messages.TheEntitySpecifiedInTheCompanyBranchBoxCannotBeFoundInTheSystem);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<GLTrialBalanceImportMap, GLTrialBalanceImportMap.orgBAccountID> e)
		{
			e.Cache.SetDefaultExt<GLTrialBalanceImportMap.ledgerID>(e.Row);

			PXResultset<GLTrialBalanceImportDetails> rows = MapDetails.Select();
			foreach (GLTrialBalanceImportDetails row in rows)
			{
				GLTrialBalanceImportDetails copy = (GLTrialBalanceImportDetails)MapDetails.Cache.CreateCopy(row);

				copy.Status = TrialBalanceImportStatusAttribute.NEW;

				copy.ImportBranchCDError = null;
				copy.MapBranchID = null;
				copy.ImportAccountCDError = null;
				copy.MapAccountID = null;
				copy.ImportSubAccountCDError = null;
				copy.MapSubAccountID = null;
				copy.AccountType = null;
				copy.Description = null;

				if (!IsBranchesNotBalancing(e.Row.OrgBAccountID))
				{
					MapDetails.Cache.SetDefaultExt<GLTrialBalanceImportDetails.importBranchCD>(copy);
				}

				MapDetails.Update(copy);
			}

			Map.Current.DebitTotalBalance = 0m;
			Map.Current.CreditTotalBalance = 0m;
		}

        #endregion

        #region TrialBalanceTemplate

		protected virtual void _(Events.FieldVerifying<TrialBalanceTemplate, TrialBalanceTemplate.importBranchCD> e)
		{
			if (!IsBranchesNotBalancing(Map.Current.OrgBAccountID))
			{
				e.NewValue = null;
			}
			e.Cancel = true;
		}

        protected virtual void TrialBalanceTemplate_ImportAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void TrialBalanceTemplate_ImportSubAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void TrialBalanceTemplate_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            //MapDetails.Update(ConvertToImportDetails(sender, (TrialBalanceTemplate)e.Row));
            MapDetails.Update((TrialBalanceTemplate)e.Row);
        }

        protected virtual void TrialBalanceTemplate_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            //MapDetails.Update(ConvertToImportDetails(sender, (TrialBalanceTemplate)e.Row));
            MapDetails.Update((TrialBalanceTemplate)e.Row);
        }

        protected virtual void TrialBalanceTemplate_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            e.Cancel = true;
        }

        #endregion

        #region GLTrialBalanceImportDetails

		protected virtual void _(Events.FieldVerifying<GLTrialBalanceImportDetails, GLTrialBalanceImportDetails.importBranchCD> e)
		{
			e.Cancel = true;
		}

		protected virtual void _(Events.FieldVerifying<GLTrialBalanceImportDetails, GLTrialBalanceImportDetails.mapBranchID> e)
		{
			if (e.Row == null || e.NewValue == null)
				return;

			bool isBranchesNotBalancing = false;
			int? organizationID = PXAccess.GetOrganizationByBAccountID(Map.Current.OrgBAccountID)?.OrganizationID;
			if (organizationID != null)
			{
				Organization organization = OrganizationMaint.FindOrganizationByID(this, organizationID);
				if (organization.OrganizationType == OrganizationTypes.WithBranchesNotBalancing)
				{
					isBranchesNotBalancing = true;
				}
			}

			if (isBranchesNotBalancing)
			{
				Branch validatedBrunch = PXSelect<Branch,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>,
						And<Branch.organizationID, Equal<Required<Branch.organizationID>>,
						And<Where<Branch.bAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>>>>
					.SelectSingleBound(this, null, e.NewValue, organizationID);

				if (validatedBrunch == null)
				{
					throw new PXSetPropertyException(Messages.ImportBranchCDNotFound);
				}
			}
		}

        protected virtual void GLTrialBalanceImportDetails_ImportAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void GLTrialBalanceImportDetails_ImportSubAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void GLTrialBalanceImportDetails_MapSubAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = e.Row as GLTrialBalanceImportDetails;
            if (row == null || row.MapAccountID == null || e.NewValue == null) return;
            Account acc = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(sender.Graph, row.MapAccountID);
            if (acc.IsCashAccount != true)
            {
                return;
            }
            CA.CashAccount cashAccount = PXSelect<CA.CashAccount, Where<CA.CashAccount.accountID, Equal<Required<CA.CashAccount.accountID>>,
                And<CA.CashAccount.subID, Equal<Required<CA.CashAccount.subID>>>>>.Select(sender.Graph, row.MapAccountID, (int?)e.NewValue);
            if (cashAccount == null)
            {
                throw new PXSetPropertyException(Messages.InvalidCashAccountSub);
            }
        }



        protected virtual void GLTrialBalanceImportDetails_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            GLTrialBalanceImportDetails row = (GLTrialBalanceImportDetails)e.Row;
            if (row == null)
            {   return;
            }

            CheckMappingAndBalance(sender, Map.Current, row);
			PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.curyYtdBalance>(sender, row, IsCuryYtdBalanceFieldUsable() && 
				!string.IsNullOrEmpty(row.AccountCuryID) && row.AccountCuryID != Ledger.Current?.BaseCuryID);
		}

        protected virtual void GLTrialBalanceImportDetails_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            GLTrialBalanceImportDetails oldRow = (GLTrialBalanceImportDetails)e.OldRow;
            GLTrialBalanceImportDetails row = (GLTrialBalanceImportDetails)e.Row;
            if (oldRow == null || row == null)
            {   return;
            }

            bool process = false;
			if (row.ImportBranchCD != oldRow.ImportBranchCD)
			{
				process = true;
				row.ImportBranchCDError = null;
				row.MapBranchID = null;
			}
            if (row.ImportAccountCD != oldRow.ImportAccountCD)
            {
                process = true;
                row.ImportAccountCDError = null;
                row.MapAccountID = null;
                row.ImportSubAccountCDError = null;
                row.MapSubAccountID = null;
                row.AccountType = null;
                row.Description = null;
            }
            if (row.ImportSubAccountCD != oldRow.ImportSubAccountCD)
            {
                process = true;
                row.ImportSubAccountCDError = null;
                row.MapSubAccountID = null;
            }

            if (process)
            {   ProcessRow(row);
            }

            if ((row.ImportAccountCD != null || Ledger.Current != null) && row.AccountCuryID == null)
            {
                row.CuryYtdBalance = row.YtdBalance;
            }
        }

        protected virtual void GLTrialBalanceImportDetails_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            GLTrialBalanceImportDetails row = (GLTrialBalanceImportDetails)e.Row;
            if (row == null)
            {   return;
            }

            if ((row.ImportAccountCD != null || Ledger.Current != null) && row.AccountCuryID == null)
            {
                row.CuryYtdBalance = row.YtdBalance;
            }
        }

        protected virtual void GLTrialBalanceImportDetails_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            GLTrialBalanceImportDetails row = (GLTrialBalanceImportDetails)e.Row;
            if (row == null)
            {   return;
            }
        }

        protected virtual void GLTrialBalanceImportDetails_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            GLTrialBalanceImportDetails row = (GLTrialBalanceImportDetails)e.Row;
            if (row == null)
            {   return;
            }

            if (e.Operation != PXDBOperation.Delete)
            {
                CheckMappingAndBalance(sender, Map.Current, row);
            }
        }

        protected virtual void GLTrialBalanceImportMap_IsHold_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            GLTrialBalanceImportMap header = (GLTrialBalanceImportMap)e.Row;
            if (header == null)
            {   return;
            }

            if (header.IsHold != true)
            {
                PXResultset<GLTrialBalanceImportDetails> rows = MapDetails.Select();
                foreach (GLTrialBalanceImportDetails row in rows)
                {
                    if (!CheckMappingAndBalance(MapDetails.Cache, header, row))
                    {
                        MapDetails.Cache.Update(row);
                        break;
                    }
                }
            }
        }

		protected virtual void GLTrialBalanceImportMap_LedgerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (!IsCuryYtdBalanceFieldUsable())
			{
				var details = MapDetails.Select().RowCast<GLTrialBalanceImportDetails>();

				foreach (var detail in details)
				{
					detail.CuryYtdBalance = detail.YtdBalance;
					MapDetails.Update(detail);
				}
			}
		}

        #endregion

        #endregion

        #region Private Methods

	    private bool IsCuryYtdBalanceFieldUsable()
	    {
		    if (Ledger.Current == null)
			    return true;

			return Ledger.Current.BalanceType != LedgerBalanceType.Report;
	    }

		private PXResultset<GLTrialBalanceImportDetails> SearchDuplicates(GLTrialBalanceImportDetails item)
		{
			MapDetails.Cache.ClearQueryCacheObsolete();

			if (PXAccess.FeatureInstalled<CS.FeaturesSet.subAccount>() == true)
			{
				return PXSelect<GLTrialBalanceImportDetails,
				 Where<GLTrialBalanceImportDetails.mapNumber, Equal<Required<GLTrialBalanceImportDetails.mapNumber>>,
					And<GLTrialBalanceImportDetails.importBranchCD, Equal<Required<GLTrialBalanceImportDetails.importBranchCD>>,
					  And<GLTrialBalanceImportDetails.importAccountCD, Equal<Required<GLTrialBalanceImportDetails.importAccountCD>>,
							And<GLTrialBalanceImportDetails.importSubAccountCD, Equal<Required<GLTrialBalanceImportDetails.importSubAccountCD>>>>>>>.
				 Select(this, item.MapNumber, item.ImportBranchCD, item.ImportAccountCD, item.ImportSubAccountCD);
			}
			else
			{
				return PXSelect<GLTrialBalanceImportDetails,
				 Where<GLTrialBalanceImportDetails.mapNumber, Equal<Required<GLTrialBalanceImportDetails.mapNumber>>,
					And<GLTrialBalanceImportDetails.importBranchCD, Equal<Required<GLTrialBalanceImportDetails.importBranchCD>>,
					  And <GLTrialBalanceImportDetails.importAccountCD, Equal<Required<GLTrialBalanceImportDetails.importAccountCD>>>>>>.
				 Select(this, item.MapNumber, item.ImportBranchCD, item.ImportAccountCD);
			}
		}

		private static bool SetValue<TSourceField, TTargetField>(PXCache cache, GLTrialBalanceImportDetails item, string alternativeError, string emptyError)
			where TSourceField : IBqlField
			where TTargetField : IBqlField
        {
            string error = null;
            PXUIFieldAttribute.SetError<TTargetField>(cache, item, null);
            object value = cache.GetValue<TSourceField>(item);

            if (value == null || value is string && string.IsNullOrEmpty(value.ToString())) error = emptyError;
            else
                try
                {
                    cache.SetValueExt<TTargetField>(item, value);
                }
                catch (PXSetPropertyException e)
                {
                    error = e.Message;
                }
                finally
                {
                    if (error == null) error = PXUIFieldAttribute.GetError<TTargetField>(cache, item);
                }

            if (!string.IsNullOrEmpty(error))
            {
                PersistErrorAttribute.SetError<TSourceField>(cache, item, (error != emptyError && alternativeError != null) ? alternativeError : error);
                return false;
            }
            PersistErrorAttribute.ClearError<TSourceField>(cache, item);
            return true;
        }

        private static bool IsUnsignOperations(JournalEntryImport graph)
        {
            return graph.GLSetup.Current.TrialBalanceSign != GL.GLSetup.trialBalanceSign.Normal;
        }

        private static Account GetAccount(PXGraph graph, object accountID)
        {
            PXResultset<Account> result =
                PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.
                Select(graph, accountID);
            return result.Count > 0 ? (Account)result[0] : null;
        }

        private static Ledger GetLedger(PXGraph graph, int? ledgerID)
        {
            return GL.Ledger.PK.Find(graph, ledgerID);
        }

        private bool CanEdit
        {
            get
            {
                return Map.Current != null && IsEditable(Map.Current);
            }
        }

        private static bool IsEditable(GLTrialBalanceImportMap map)
        {
            return map.Status != TrialBalanceImportMapStatusAttribute.Released;
        }

        private static void FillDebitAndCreditAmt(GLTran tran, decimal diff, decimal curyDiff, bool isReversedSign, string accountType)
        {

			if ((accountType == AccountType.Asset || accountType == AccountType.Expense) && diff > 0m ||
                (accountType == AccountType.Liability || accountType == AccountType.Income) &&
                (isReversedSign && diff > 0m || !isReversedSign && diff < 0m))
            {
				tran.CreditAmt = 0m;
				tran.DebitAmt = Math.Abs(diff);		
            }
            else
            {
                tran.DebitAmt = 0m;
                tran.CreditAmt = Math.Abs(diff);
            }

			if ((accountType == AccountType.Asset || accountType == AccountType.Expense) && curyDiff > 0m ||
				(accountType == AccountType.Liability || accountType == AccountType.Income) &&
					(isReversedSign && curyDiff > 0m || !isReversedSign && curyDiff < 0m))
			{
				tran.CuryDebitAmt = Math.Abs(curyDiff);
			}
			else
			{
				tran.CuryCreditAmt = Math.Abs(curyDiff);
			}

		}

        private static IEnumerable<GLHistoryEnquiryWithSubResult> GetBalances(JournalEntryImport graph, int? orgBAccountID, int? ledgerID, string finPeriodID, string begFinPeriod)
        {
            return GetBalances(graph, IsUnsignOperations(graph), orgBAccountID, ledgerID, finPeriodID, begFinPeriod);
        }

        private static IEnumerable<GLHistoryEnquiryWithSubResult> GetBalances(PXGraph graph, bool isReversedSign, int? orgBAccountID, int? ledgerID, string finPeriodID, string begFinPeriod)
        {
            if (ledgerID == null || finPeriodID == null) yield break;

            PXSelectBase<GLHistoryByPeriod> cmd = new PXSelectJoinGroupBy<GLHistoryByPeriod,
                                InnerJoin<Account,
                                        On<GLHistoryByPeriod.accountID, Equal<Account.accountID>, And<Match<Account, Current<AccessInfo.userName>>>>,
                                InnerJoin<Sub,
                                        On<GLHistoryByPeriod.subID, Equal<Sub.subID>, And<Match<Sub, Current<AccessInfo.userName>>>>,
                                LeftJoin<GLHistory, On<GLHistoryByPeriod.accountID, Equal<GLHistory.accountID>,
                                        And<GLHistoryByPeriod.branchID, Equal<GLHistory.branchID>,
                                        And<GLHistoryByPeriod.ledgerID, Equal<GLHistory.ledgerID>,
                                        And<GLHistoryByPeriod.subID, Equal<GLHistory.subID>,
                                        And<GLHistoryByPeriod.finPeriodID, Equal<GLHistory.finPeriodID>>>>>>,
                                LeftJoin<AH, On<GLHistoryByPeriod.ledgerID, Equal<AH.ledgerID>,
                                        And<GLHistoryByPeriod.branchID, Equal<AH.branchID>,
                                        And<GLHistoryByPeriod.accountID, Equal<AH.accountID>,
                                        And<GLHistoryByPeriod.subID, Equal<AH.subID>,
                                        And<GLHistoryByPeriod.lastActivityPeriod, Equal<AH.finPeriodID>>>>>>>>>>,
                                Where<GLHistoryByPeriod.ledgerID, Equal<Required<GLHistoryByPeriod.ledgerID>>,
                                        And<GLHistoryByPeriod.finPeriodID, Equal<Required<GLHistoryByPeriod.finPeriodID>>,
	                                    And<GLHistoryByPeriod.branchID, InsideBranchesOf<Required<GLTrialBalanceImportMap.orgBAccountID>>,
                                        And2<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                                            Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>,
                                        And<
                                            Where2<
                                                Where<Account.type, Equal<AccountType.asset>,
                                                    Or<Account.type, Equal<AccountType.liability>>>,
                                            Or<Where<GLHistoryByPeriod.lastActivityPeriod, GreaterEqual<Required<GLHistoryByPeriod.lastActivityPeriod>>,
                                                And<Where<Account.type, Equal<AccountType.expense>,
                                                Or<Account.type, Equal<AccountType.income>>>>>>>>>>>>,
                                Aggregate<
                                        Sum<AH.finYtdBalance,
                                        Sum<AH.curyFinYtdBalance,
                                        Sum<GLHistory.finPtdDebit,
                                        Sum<GLHistory.finPtdCredit,
                                        Sum<GLHistory.finBegBalance,
                                        Sum<GLHistory.finYtdBalance,
                                        Sum<GLHistory.curyFinBegBalance,
                                        Sum<GLHistory.curyFinYtdBalance,
                                        Sum<GLHistory.curyFinPtdCredit,
                                        Sum<GLHistory.curyFinPtdDebit,
                                        GroupBy<GLHistoryByPeriod.ledgerID,
                                        GroupBy<GLHistoryByPeriod.accountID,
                                        GroupBy<GLHistoryByPeriod.subID,
                                        GroupBy<GLHistoryByPeriod.finPeriodID
                                 >>>>>>>>>>>>>>>>(graph);

            foreach (PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, AH> it in
                cmd.Select(ledgerID, finPeriodID, orgBAccountID, begFinPeriod))
            {
                GLHistoryByPeriod baseview = (GLHistoryByPeriod)it;
                Account acct = (Account)it;
                GLHistory ah = (GLHistory)it;
                AH ah1 = (AH)it;

                GLHistoryEnquiryWithSubResult item = new GLHistoryEnquiryWithSubResult();
				item.BranchID = baseview.BranchID;
				item.LedgerID = baseview.LedgerID;
                item.AccountID = baseview.AccountID;
	            item.AccountCD = acct.AccountCD;
                item.AccountType = acct.Type;
                item.SubID = baseview.SubID;
                item.Type = acct.Type;
                item.Description = acct.Description;
                item.CuryID = acct.CuryID;
                item.LastActivityPeriod = baseview.LastActivityPeriod;
                item.PtdCreditTotal = ah.FinPtdCredit;
                item.PtdDebitTotal = ah.FinPtdDebit;
                item.CuryPtdCreditTotal = ah.CuryFinPtdCredit;
                item.CuryPtdDebitTotal = ah.CuryFinPtdDebit;
                bool reverseBalance = isReversedSign &&
                    (item.AccountType == AccountTypeList.Liability || item.AccountType == AccountTypeList.Income);
                item.EndBalance = reverseBalance ? -ah1.FinYtdBalance : ah1.FinYtdBalance;
                item.CuryEndBalance = reverseBalance ? -ah1.CuryFinYtdBalance : ah1.CuryFinYtdBalance;
                item.ConsolAccountCD = acct.GLConsolAccountCD;
                item.BegBalance = item.EndBalance + (reverseBalance ? item.PtdSaldo : -item.PtdSaldo);
                item.CuryBegBalance = item.CuryEndBalance + (reverseBalance ? item.CuryPtdSaldo : -item.CuryPtdSaldo);
                yield return item;
            }
        }

        private bool CheckMappingAndBalance(PXCache sender, GLTrialBalanceImportMap header, GLTrialBalanceImportDetails row)
        {
            bool ok = true;

			sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.importBranchCD>(row, null, null);
			sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.mapBranchID>(row, null, null);
            sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.importAccountCD>(row, null, null);
            sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.mapAccountID>(row, null, null);
            sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.importSubAccountCD>(row, null, null);
            sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.mapSubAccountID>(row, null, null);
            sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.ytdBalance>(row, null, null);

            if (header != null && IsEditable(header) && header.IsHold != true)
            {
				if (row.ImportBranchCD == null)
				{
					ok = false;
					sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.importBranchCD>
						(row, row.ImportBranchCD, new PXSetPropertyException(Messages.ImportBranchCDIsEmpty, PXErrorLevel.Error));
				}
				if (row.MapBranchID == null && IsBranchesNotBalancing(header.OrgBAccountID))
				{
					ok = false;
					sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.mapBranchID>
						(row, row.MapBranchID, new PXSetPropertyException(Messages.ImportBranchIDIsEmpty, PXErrorLevel.Error));
				}
                if (row.ImportAccountCD == null)
                {   ok = false;
                    sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.importAccountCD>
                        (row, row.ImportAccountCD, new PXSetPropertyException(Messages.ImportAccountCDIsEmpty, PXErrorLevel.Error));
                }
                if (row.MapAccountID == null)
                {   ok = false;
                    sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.mapAccountID>
                        (row, row.MapAccountID, new PXSetPropertyException(Messages.ImportAccountIDIsEmpty, PXErrorLevel.Error));
                }
                if (row.ImportSubAccountCD == null && PXAccess.FeatureInstalled<CS.FeaturesSet.subAccount>() == true)
                {   ok = false;
                    sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.importSubAccountCD>
                        (row, row.ImportSubAccountCD, new PXSetPropertyException(Messages.ImportSubAccountCDIsEmpty, PXErrorLevel.Error));
                }
                if (row.MapSubAccountID == null)
                {   ok = false;
                    sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.mapSubAccountID>
                        (row, row.MapSubAccountID, new PXSetPropertyException(Messages.ImportSubAccountIDIsEmpty, PXErrorLevel.Error));
                }
                if (row.YtdBalance == null)
                {   ok = false;
                    sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.ytdBalance>
                        (row, row.YtdBalance, new PXSetPropertyException(Messages.ImportYtdBalanceIsEmpty, PXErrorLevel.Error));
                }
            }

            return ok;
        }

        private bool IsRequireControlTotal
        {
            get
            {
                return GLSetup.Current != null && GLSetup.Current.RequireControlTotal == true;
            }
        }

        private void ProcessRow(GLTrialBalanceImportDetails row)
        {
            if (row.Status == TrialBalanceImportStatusAttribute.VALID && row.Line != null)
            {   
                Dictionary<int, GLTrialBalanceImportDetails> dict = new Dictionary<int, GLTrialBalanceImportDetails>();
                dict.Add((int)row.Line, row);
                ProcessHandler(dict, _VALIDATE_ACTION, false);
            }
            else
            {   row.Status = TrialBalanceImportStatusAttribute.NEW;
            }
        }


        private Account GetRowAccount(GLTrialBalanceImportDetails row)
        {
            return row != null && row.YtdBalance != null && row.MapAccountID != null ? GetAccount(this, row.MapAccountID) : null;
        }

        private void CheckTotalBalance(PXCache sender, GLTrialBalanceImportMap header, bool require)
        {
            sender.RaiseExceptionHandling<GLTrialBalanceImportMap.debitTotalBalance>(header, null, null);
            sender.RaiseExceptionHandling<GLTrialBalanceImportMap.creditTotalBalance>(header, null, null);

            if (header.IsHold != true)
            {
                if (require)
                {
                    sender.RaiseExceptionHandling<GLTrialBalanceImportMap.totalBalance>(header, null, null);

                    if (header.DebitTotalBalance != header.CreditTotalBalance)
                    { 
                            sender.RaiseExceptionHandling<GLTrialBalanceImportMap.debitTotalBalance>
                                (header, header.DebitTotalBalance, new PXSetPropertyException(Messages.DocumentIsOutOfBalancePleaseReview, PXErrorLevel.Error));
                    }
                    else
                    {
                        if (header.CreditTotalBalance != header.TotalBalance)
                            sender.RaiseExceptionHandling<GLTrialBalanceImportMap.totalBalance>
                                  (header, header.TotalBalance, new PXSetPropertyException(Messages.DocumentIsOutOfBalancePleaseReview, PXErrorLevel.Error));
                    }
                }
                else
                {
                    if (header.DebitTotalBalance != header.CreditTotalBalance)
                    {
                        sender.RaiseExceptionHandling<GLTrialBalanceImportMap.debitTotalBalance>
                            (header, header.DebitTotalBalance, new PXSetPropertyException(Messages.DocumentIsOutOfBalancePleaseReview, PXErrorLevel.Error));
                    }
                }
            }
        }

		private bool IsBranchesNotBalancing(int? orgBAccountID)
		{
			bool isBranchesNotBalancing = false;
			int? organizationID = PXAccess.GetOrganizationByBAccountID(orgBAccountID)?.OrganizationID;
			if (organizationID != null)
			{
				Organization organization = OrganizationMaint.FindOrganizationByID(this, organizationID);
				if (organization.OrganizationType == OrganizationTypes.WithBranchesNotBalancing)
				{
					isBranchesNotBalancing = true;
				}
			}
			return isBranchesNotBalancing;
		}
        #endregion

        #region Implementation of PXImportAttribute.IPXPrepareItems

        public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
        {
            if (viewName == _IMPORTTEMPLATE_VIEWNAME && Map.Current != null)
            {
				keys[_mapNumberFieldName] = Map.Current.Number;

				string accountCD = (string) values[_importFieldNameAccount];
				Account account = Account.UK.Find(this, accountCD);
				if (account != null && (account.CuryID == null || account.CuryID == Ledger.Current?.BaseCuryID)) {
				string curyYtdBalanceName = typeof(GLTrialBalanceImportDetails.curyYtdBalance).Name;
				if (values.Contains(curyYtdBalanceName))
				{
					values.Remove(curyYtdBalanceName);
				}
            }
            }
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

	    public virtual void PrepareItems(string viewName, IEnumerable items)
	    {
		    
	    }

        #endregion
    }
}
