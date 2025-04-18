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

using PX.Data;
using PX.Objects.Common.Tools;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;
using System.Linq;

namespace PX.Objects.GL
{
	public class AllocationMaint : PXGraph<AllocationMaint, GLAllocation>, PXImportAttribute.IPXPrepareItems
	{

		#region Ctor+Public Members
		public AllocationMaint()
		{
			GLSetup setup = GLSetup.Current;
			PXUIFieldAttribute.SetEnabled(this.Batches.Cache, null, false);
			this.Batches.Cache.AllowInsert = false;
			this.Batches.Cache.AllowDelete = false;
			this.Batches.Cache.AllowUpdate = false;
		}

		public PXSelect<GLAllocation> AllocationHeader;
		public PXSelect<GLAllocation, Where<GLAllocation.gLAllocationID, Equal<Current<GLAllocation.gLAllocationID>>>> Allocation;
		[PXImport(typeof(GLAllocation))]
		public PXSelect<GLAllocationSource, Where<GLAllocationSource.gLAllocationID, Equal<Current<GLAllocation.gLAllocationID>>>, OrderBy<Asc<GLAllocationSource.lineID>>> Source;
		[PXImport(typeof(GLAllocation))]
		public PXSelect<GLAllocationDestination, Where<GLAllocationDestination.gLAllocationID, Equal<Current<GLAllocation.gLAllocationID>>>, OrderBy<Asc<GLAllocationDestination.lineID>>> Destination;
		public PXSelectJoin<Batch, InnerJoin<GLAllocationHistory,
														On<Batch.batchNbr, Equal<GLAllocationHistory.batchNbr>,
														And<Batch.module, Equal<GLAllocationHistory.module>>>>,
									Where<GLAllocationHistory.gLAllocationID, Equal<Current<GLAllocation.gLAllocationID>>>, OrderBy<Desc<Batch.tranPeriodID,Desc<Batch.batchNbr>>>> Batches;
		public PXSetup<Company> Company;
		public PXSetup<GLSetup> GLSetup;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }
		#endregion

		#region Buttons
		public PXAction<GLAllocation> viewBatch;
		[PXUIField(
			DisplayName = Messages.BatchDetails, 
			MapEnableRights = PXCacheRights.Select, 
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			Batch row = Batches.Current;
			if (row != null)
			{
				JournalEntry graph = CreateInstance<JournalEntry>();
				graph.BatchModule.Current = row;
				throw new PXRedirectRequiredException(graph, true, "View Batch"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}
			return adapter.Get();
		}

		#endregion

		#region GLAllocation Events Handlers
		protected virtual void GLAllocation_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			GLAllocation row = (GLAllocation)e.Row;
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
			{
				if (row.Active == true && (this.Source.Select().Count == 0))
				{
					throw new PXException(Messages.SourceAccountNotSpecified);
				}

				//In the case of external rule Distribution is defined dynamically in external table
				if (row.Active == true && 
					row.AllocMethod != Constants.AllocationMethod.ByExternalRule 
						&& this.Destination.Select().Count == 0)
				{
					throw new PXException(Messages.DestAccountNotSpecified);
				}

				if (row.StartFinPeriodID != null && row.EndFinPeriodID != null)
				{
					int startYr, endYr;
					int startNbr, endNbr;
					if (!FinPeriodUtils.TryParse(row.StartFinPeriodID, out startYr, out startNbr)) 
					{
						cache.RaiseExceptionHandling<GLAllocation.startFinPeriodID>(e.Row, row.StartFinPeriodID, new PXSetPropertyException(Messages.AllocationStartPeriodHasIncorrectFormat));
					}

					if (!FinPeriodUtils.TryParse(row.EndFinPeriodID, out endYr, out endNbr))
					{
						cache.RaiseExceptionHandling<GLAllocation.endFinPeriodID>(e.Row, row.EndFinPeriodID, new PXSetPropertyException(Messages.AllocationEndPeriodHasIncorrectFormat));
					}
					bool isReversed = (endYr < startYr) || (endYr == startYr && startNbr > endNbr);
					if (isReversed)
					{
						cache.RaiseExceptionHandling<GLAllocation.endFinPeriodID>(e.Row, row.EndFinPeriodID, new PXSetPropertyException(Messages.AllocationEndPeriodIsBeforeStartPeriod));
					}
				}
				if (!this.ValidateSrcAccountsForCurrency()) 
				{
					throw new PXException(Messages.AccountsNotInBaseCury);
				}

				GLAllocationSource src;
				if (!this.ValidateSrcAccountsForInterlacing(out src))
				{
					this.Source.Cache.RaiseExceptionHandling<GLAllocationSource.accountCD>(src, src.AccountCD, new PXSetPropertyException(Messages.AllocationSourceAccountSubInterlacingDetected, PXErrorLevel.RowError));
				}

				if (row.Active == true 
					&& this.isWeigthRecalcRequired())
				{
					decimal total = 0.00m;
					GLAllocationDestination dest = null; 
					foreach (GLAllocationDestination iDest in this.Destination.Select())
					{
						if (iDest.Weight.HasValue)
						{
							total += iDest.Weight.Value;
							decimal weight = (decimal)iDest.Weight.Value;
							if (weight <= 0.00m || weight > 100.00m)
							{
								this.Destination.Cache.RaiseExceptionHandling<GLAllocationDestination.weight>(iDest, iDest.Weight, 
										new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(Messages.ValueForWeight,0,100)));
								return;
							}
						}
						if (dest == null)
							dest = iDest;
						
					}
					if (Math.Abs(total - 100.0m) >= 0.000001m)
					{
						if (dest != null)
						{
							this.Destination.Cache.RaiseExceptionHandling<GLAllocationDestination.weight>(dest, dest.Weight,
										new PXSetPropertyException(Messages.SumOfDestsMustBe100));
						}
						else
						{
							throw new PXException(Messages.SumOfDestsMustBe100);
						}
					}
				}

			}
		}
	
		protected virtual void GLAllocation_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			GLAllocation record = (GLAllocation)e.Row;
			bool isBasisAcctEnabled = (record.AllocMethod == Constants.AllocationMethod.ByAcctPTD) || (record.AllocMethod == Constants.AllocationMethod.ByAcctYTD);
			PXUIFieldAttribute.SetEnabled<GLAllocation.basisLederID>(cache, record, isBasisAcctEnabled);
			PXUIFieldAttribute.SetEnabled<GLAllocationDestination.basisBranchID>(this.Destination.Cache, null, isBasisAcctEnabled);
			PXUIFieldAttribute.SetEnabled<GLAllocationDestination.basisAccountCD>(this.Destination.Cache, null, isBasisAcctEnabled);
			PXUIFieldAttribute.SetEnabled<GLAllocationDestination.basisSubCD>(this.Destination.Cache, null, isBasisAcctEnabled);
			
			PXUIFieldAttribute.SetVisible<GLAllocationDestination.basisBranchID>(this.Destination.Cache, null, isBasisAcctEnabled);
			PXUIFieldAttribute.SetVisible<GLAllocationDestination.basisAccountCD>(this.Destination.Cache, null, isBasisAcctEnabled);
			PXUIFieldAttribute.SetVisible<GLAllocationDestination.basisSubCD>(this.Destination.Cache, null, isBasisAcctEnabled);

			bool isWeightEnabled = (record.AllocMethod == Constants.AllocationMethod.ByWeight) || (record.AllocMethod == Constants.AllocationMethod.ByPercent);
			PXUIFieldAttribute.SetEnabled<GLAllocationDestination.weight>(this.Destination.Cache, null, isWeightEnabled);
			PXUIFieldAttribute.SetVisible<GLAllocationDestination.weight>(this.Destination.Cache, null, isWeightEnabled);

			PXUIFieldAttribute.SetRequired<GLAllocationDestination.accountCD>(this.Destination.Cache, record.AllocateSeparately != true);
			PXUIFieldAttribute.SetRequired<GLAllocationDestination.subCD>(this.Destination.Cache, record.AllocateSeparately != true);
		}

		protected virtual void GLAllocation_AllocMethod_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			GLAllocation record = (GLAllocation)e.Row;
			if ((string)e.OldValue == Constants.AllocationMethod.ByAcctPTD || ((string)e.OldValue == Constants.AllocationMethod.ByAcctYTD)) 
			{
				bool isBasisAcctEnabled = (record.AllocMethod == Constants.AllocationMethod.ByAcctPTD) || (record.AllocMethod == Constants.AllocationMethod.ByAcctYTD);
				//Reset BasisAccount Fields
				if (!isBasisAcctEnabled) 
				{
					record.BasisLederID = null;
					foreach (GLAllocationDestination iDest in this.Destination.Select()) 
					{
						iDest.BasisAccountCD = iDest.BasisSubCD = null;
						this.Destination.Cache.Update(iDest);
					}					
				}
			}
			if (((string)e.OldValue == Constants.AllocationMethod.ByPercent) || ((string)e.OldValue == Constants.AllocationMethod.ByWeight))
			{
				bool isWeightEnabled = (record.AllocMethod == Constants.AllocationMethod.ByWeight) || (record.AllocMethod == Constants.AllocationMethod.ByPercent);
				if (!isWeightEnabled) 
				{
					//Reset Weight/Percent field
					foreach (GLAllocationDestination iDest in this.Destination.Select())
					{
						iDest.Weight = null;
						this.Destination.Cache.Update(iDest);
					}
				}
			}
			if (record.AllocMethod == Constants.AllocationMethod.ByAcctPTD || (record.AllocMethod == Constants.AllocationMethod.ByAcctYTD))
			{
				record.BasisLederID = record.AllocLedgerID;
			}
		}

		public virtual void GLAllocation_StartFinPeriodID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			FinPeriod finPeriod = FinPeriodRepository.FindFinPeriodByDate(Accessinfo.BusinessDate, FinPeriod.organizationID.MasterValue);
			if (finPeriod != null)
				e.NewValue = FinPeriodIDFormattingAttribute.FormatForDisplay(finPeriod.FinPeriodID);
		}

		protected virtual void GLAllocation_AllocLedgerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			GLAllocation row = (GLAllocation)e.Row;
			if (row != null)
			{
				row.SourceLedgerID = row.AllocLedgerID;
				if (row.AllocMethod == Constants.AllocationMethod.ByAcctPTD || row.AllocMethod == Constants.AllocationMethod.ByAcctYTD)
				{
					row.BasisLederID = row.AllocLedgerID;
				}
			}
			BranchAttribute.VerifyFieldInPXCache<GLAllocationDestination, GLAllocationDestination.branchID>(this, Destination.Select());
		}

		protected virtual void _(Events.FieldUpdated<GLAllocation.branchID> e)
		{
			if (e.Row == null) return;
			
			GLAllocation currentAllocation = e.Row as GLAllocation;
			if (AllocationHeader.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted)
			{
				object newval = null;
				AllocationHeader.Cache.RaiseFieldDefaulting<GLAllocation.allocLedgerID>(e.Row, out newval);
				currentAllocation.AllocLedgerID = (int)newval;
				AllocationHeader.Cache.RaiseFieldDefaulting<GLAllocation.sourceLedgerID>(e.Row, out newval);
				currentAllocation.SourceLedgerID = (int)newval;
				AllocationHeader.Cache.RaiseFieldDefaulting<GLAllocation.allocLedgerBalanceType>(e.Row, out newval);
				currentAllocation.AllocLedgerBalanceType = (string)newval;
				AllocationHeader.Cache.RaiseFieldDefaulting<GLAllocation.allocLedgerBaseCuryID>(e.Row, out newval);
				currentAllocation.AllocLedgerBaseCuryID = (string)newval;
			}
			else
			{
				object cur = currentAllocation.AllocLedgerID;
				try
				{
					AllocationHeader.Cache.RaiseFieldVerifying<GLAllocation.allocLedgerID>(e.Row, ref cur);
				}
				catch (PXSetPropertyException ex)
				{
					AllocationHeader.Cache.RaiseExceptionHandling<GLAllocation.allocLedgerID>(e.Row, cur, ex);
				}
			}

			ValidateBranchesForLedgers();
		}

		protected virtual void _(Events.FieldUpdated<GLAllocation.sourceLedgerID> e)
		{
			BranchAttribute.VerifyFieldInPXCache<GLAllocationSource, GLAllocationSource.branchID>(this, Source.Select());
		}

		protected virtual void _(Events.FieldUpdated<GLAllocation.basisLederID> e)
		{
			BranchAttribute.VerifyFieldInPXCache<GLAllocationDestination, GLAllocationDestination.basisBranchID>(this, Destination.Select());
		}
		#endregion
		#region GLAllocationSource Events Handlers

		protected virtual void GLAllocationSource_LimitPercent_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			GLAllocationSource src = (GLAllocationSource)e.Row;
			src.LimitAmount = (src.LimitPercent == decimal.Zero ? src.LimitAmount : decimal.Zero);
		}

		protected virtual void GLAllocationSource_LimitAmount_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			GLAllocationSource src = (GLAllocationSource)e.Row;
			src.LimitPercent = (src.LimitAmount == decimal.Zero ? 100.00m : decimal.Zero);
		}

		protected virtual void GLAllocationSource_SubCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void GLAllocationSource_AccountCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			VerifyAccountIDToBeNoControl<GLAllocationSource.accountCD, GLAllocationSource.accountCD>(cache, e, e.NewValue, Allocation.Current?.SourceLedgerID);
		}
		protected virtual void GLAllocationSource_ContrAccountID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			VerifyAccountIDToBeNoControl<GLAllocationSource.contrAccountID, Account.accountID>(cache, e, e.NewValue, Allocation.Current?.SourceLedgerID);
		}
		private void VerifyAccountIDToBeNoControl<T, A>(PXCache cache, EventArgs e, object accountID, int? ledgerID) where T:IBqlField where A : IBqlField
		{
			if (accountID == null) return;

			var ledger =Ledger.PK.Find(this, ledgerID);
			if (ledger?.BalanceType != LedgerBalanceType.Actual) return;

			var account = (Account)PXSelect<Account>.Search<A>(this, accountID);
			AccountAttribute.VerifyAccountIsNotControl<T>(cache, e, account);
		}

		protected virtual void GLAllocationSource_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			this.UpdateParentStatus();
		}

		protected virtual void GLAllocationSource_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			this.UpdateParentStatus();
		}

		protected virtual void GLAllocationSource_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			this.UpdateParentStatus();
		}

		protected virtual void GLAllocationSource_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			GLAllocationSource row = (GLAllocationSource)e.Row;
			if (string.IsNullOrEmpty(row.AccountCD.Trim())) 
			{
				if (cache.RaiseExceptionHandling<GLAllocationSource.accountCD>(e.Row, row.AccountCD, new PXSetPropertyException(Messages.AllocationSrcEmptyAccMask, PXErrorLevel.Error)))
				{
					throw new PXRowPersistingException(typeof(GLAllocationSource.accountCD).Name, row.AccountCD, Messages.AllocationSrcEmptyAccMask );
				}				
			}
			else
			{
				VerifyAccountIDToBeNoControl<GLAllocationSource.accountCD, GLAllocationSource.accountCD>(cache, e, row.AccountCD, Allocation.Current?.SourceLedgerID);
				VerifyAccountIDToBeNoControl<GLAllocationSource.contrAccountID, Account.accountID>(cache, e, row.ContrAccountID, Allocation.Current?.SourceLedgerID);
			}
		}

		#endregion
		#region GLAllocation Destination Event Handlers 
		protected virtual void GLAllocationDestination_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			if (this.isWeigthRecalcRequired())
			{
				GLAllocationDestination record = (GLAllocationDestination)e.Row;
				if (record.Weight == null)
				{
					record.Weight = 100;
					foreach (GLAllocationDestination it in this.Destination.Select())
					{
						if (object.ReferenceEquals(it, record)) continue;
						record.Weight -= it.Weight;
					}

					this.justInserted = record.LineID;
				}
			}
			else
				this.justInserted = null;
		}
		protected virtual void GLAllocationDestination_Weight_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (this.isWeigthRecalcRequired())
			{
				GLAllocationDestination dest = (GLAllocationDestination)e.Row;
				decimal newWeght = (decimal)e.NewValue;
				if (newWeght <= 0.00m || newWeght > 100.00m ) 
				{
					throw new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(Messages.ValueForWeight,0,100));
				}
			}
		}
		protected virtual void GLAllocationDestination_BasisSubCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void GLAllocationDestination_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if (this.isWeigthRecalcRequired())
			{
				if (PXEntryStatus.Notchanged == this.Allocation.Cache.GetStatus(this.Allocation.Current))
				{
					//Set State  of the Terms To Modified - for the validation
					this.Allocation.Cache.SetStatus(this.Allocation.Current, PXEntryStatus.Updated);
				}
			}
			UpdateParentStatus();
		}
		protected virtual void GLAllocationDestination_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			if(this.Allocation.Cache.GetStatus(this.Allocation.Current) != PXEntryStatus.Deleted && this.isWeigthRecalcRequired())
			{
				GLAllocationDestination row = (GLAllocationDestination)e.Row;
				
				if (!this.isMassDelete)
				{
					if (!(this.justInserted.HasValue && this.justInserted.Value == row.LineID))
					{                        
						this.distributePercentOf(row);
						this.Destination.View.RequestRefresh();
					}
					this.Allocation.Cache.Update(this.Allocation.Current);
				}
			}
		}
		protected virtual void GLAllocationDestination_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			
			this.UpdateParentStatus();
		}
		protected virtual void GLAllocationDestination_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			GLAllocationDestination row = (GLAllocationDestination)e.Row;
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
			{
				VerifyAccountIDToBeNoControl<GLAllocationDestination.accountCD, Account.accountCD>(cache, e, row.AccountCD, Allocation.Current?.AllocLedgerID);
				VerifyAccountIDToBeNoControl<GLAllocationDestination.basisAccountCD, Account.accountCD>(cache, e, row.BasisAccountCD, Allocation.Current?.AllocLedgerID);

				GLAllocation parent = this.Allocation.Current;
				List<GLAllocationDestination> duplicated = FindDuplicated(row, parent);
				PXDefaultAttribute.SetPersistingCheck<GLAllocationDestination.accountCD>(this.Destination.Cache, e.Row,
					(parent.AllocateSeparately == true) ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
				PXDefaultAttribute.SetPersistingCheck<GLAllocationDestination.subCD>(this.Destination.Cache, e.Row,
					(parent.AllocateSeparately == true) ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
				//if (parent.AllocMethod == Constants.AllocationMethod.ByAcctPTD || parent.AllocMethod == Constants.AllocationMethod.ByAcctYTD)
				{
					if (duplicated.Count > 0)
					{
						duplicated.Add(row);
						foreach (GLAllocationDestination it in duplicated)
						{
							PXErrorLevel level = (parent.AllocMethod == Constants.AllocationMethod.ByAcctPTD || parent.AllocMethod == Constants.AllocationMethod.ByAcctYTD) ? PXErrorLevel.RowError : PXErrorLevel.RowWarning;
							string message = (parent.AllocMethod == Constants.AllocationMethod.ByAcctPTD || parent.AllocMethod == Constants.AllocationMethod.ByAcctYTD) ? Messages.ERR_AllocationDestinationAccountMustNotBeDuplicated : Messages.AllocationDestinationAccountAreIdentical;
							cache.RaiseExceptionHandling<GLAllocationDestination.accountCD>(it, it.AccountCD, new PXSetPropertyException(message, level));
						}
					}
				}
			}
		}
		protected virtual void GLAllocationDestination_AccountCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			VerifyAccountIDToBeNoControl<GLAllocationDestination.accountCD, Account.accountCD>(cache, e, e.NewValue, Allocation.Current?.AllocLedgerID);
			ValidateDestAccountCDMask(e.Row as GLAllocationDestination, e.NewValue?.ToString());
		}
		protected virtual void GLAllocationDestination_SubCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			ValidateDestSubCDMask(e.Row as GLAllocationDestination, e.NewValue?.ToString(), out bool fl);
			e.Cancel = true;
		}
		protected virtual void GLAllocationDestination_BasisAccountCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			VerifyAccountIDToBeNoControl<GLAllocationDestination.basisAccountCD, Account.accountCD>(cache, e, e.NewValue, Allocation.Current?.AllocLedgerID);
		}
		#endregion

		#region Helper Functions
		protected virtual void UpdateParentStatus()
		{
			if (this.Allocation.Current != null)
			{
				this.Allocation.Cache.MarkUpdated(this.Allocation.Current);
			}
		}
		protected virtual void distributePercentOf(GLAllocationDestination row) 
		{
			if (row != null && row.Weight.HasValue && row.Weight != null)
			{
				//Find last
				GLAllocationDestination lastRow = null;
				Decimal total = Decimal.Zero;
				foreach (GLAllocationDestination it in this.Destination.Select())
				{
					if (object.ReferenceEquals(it, row)) continue;
					total += it.Weight??Decimal.Zero;
					if (lastRow == null || (it.LineID.HasValue && it.LineID.Value > lastRow.LineID.Value))
					{
						lastRow = it;
					}
				}

				Decimal remainder = (100.0m - total);
				remainder = remainder < row.Weight.Value ? remainder : row.Weight.Value;
				if (lastRow != null && remainder > Decimal.Zero)
				{

					lastRow.Weight += remainder;					
				}
			}

		}
		protected virtual bool ValidateSrcAccountsForCurrency() 
		{
			var branch = (Branch)PXSelectorAttribute.Select<GLAllocation.branchID>(AllocationHeader.Cache, AllocationHeader.Cache.Current);

			PXSelectBase<Account> acctSel = 
				new PXSelect<Account,
						Where<Account.accountCD, Like<Required<Account.accountCD>>,
						And<Account.curyID, IsNotNull,
						And<Account.curyID, NotEqual<Required<Branch.baseCuryID>>>>>>(this);

			foreach (GLAllocationSource iSrc in this.Source.Select()) 
			{
				string acctCDWildCard = SubCDUtils.CreateSubCDWildcard(iSrc.AccountCD, AccountAttribute.DimensionName);
				foreach (Account iAcct in acctSel.Select(acctCDWildCard, branch.BaseCuryID))
				{
					if (iAcct != null) return false;
				}
			}
			return true;
		}

		protected virtual bool ValidateBranchesForLedgers()
		{
			bool validated = true;
			object iterBranch = null;
			foreach (GLAllocationSource sourceBranch in Source.Select())
			{
				iterBranch = sourceBranch.BranchID;
				try
				{
					Source.Cache.RaiseFieldVerifying<GLAllocationSource.branchID>(sourceBranch, ref iterBranch);
				}
				catch (PXSetPropertyException ex)
				{
					Source.Cache.RaiseExceptionHandling<GLAllocationSource.branchID>(sourceBranch, iterBranch, ex);
					validated = false;
				}
			}
			foreach (GLAllocationDestination destinationBranch in Destination.Select())
			{
				iterBranch = destinationBranch.BranchID;
				try
				{
					Destination.Cache.RaiseFieldVerifying<GLAllocationDestination.branchID>(destinationBranch, ref iterBranch);
				}
				catch (PXSetPropertyException ex)
				{
					Destination.Cache.RaiseExceptionHandling<GLAllocationDestination.branchID>(destinationBranch, iterBranch, ex);
					validated = false;
				}
			}
			if (AllocationHeader.Current?.AllocMethod == Constants.AllocationMethod.ByAcctPTD || AllocationHeader.Current?.AllocMethod == Constants.AllocationMethod.ByAcctYTD)
			{
				foreach (GLAllocationDestination destinationBranch in Destination.Select())
				{
					iterBranch = destinationBranch.BasisBranchID;
					try
					{
						Destination.Cache.RaiseFieldVerifying<GLAllocationDestination.basisBranchID>(destinationBranch, ref iterBranch);
					}
					catch (PXSetPropertyException ex)
					{
						Destination.Cache.RaiseExceptionHandling<GLAllocationDestination.basisBranchID>(destinationBranch, iterBranch, ex);
						validated = false;
					}
				}
			}
			return validated;
		}

		protected virtual bool ValidateSrcAccountsForInterlacing(out GLAllocationSource aRow)
		{
			aRow = null;
			Dictionary<AllocationProcess.BranchAccountSubKey, AllocationSourceDetail> sources = new Dictionary<AllocationProcess.BranchAccountSubKey, AllocationSourceDetail>(); 
			foreach (GLAllocationSource iSrc in this.Source.Select()) 
			{
				if (IsSourceInterlacingWithDictionary(iSrc, sources))
				{
					aRow = iSrc;
					return false;
				}
			}
			return true;
		}

		protected virtual bool IsSourceInterlacingWithDictionary(GLAllocationSource aSrc, Dictionary<AllocationProcess.BranchAccountSubKey, AllocationSourceDetail> aSrcDict) 
		{
			string acctCDWildCard = SubCDUtils.CreateSubCDWildcard(aSrc.AccountCD, AccountAttribute.DimensionName);
			string subCDWildCard = SubCDUtils.CreateSubCDWildcard(aSrc.SubCD, SubAccountAttribute.DimensionName);
			foreach (Account iAcct in PXSelect<Account, Where<Account.accountCD, Like<Required<Account.accountCD>>>>.Select(this, acctCDWildCard))
			{
				foreach (Sub iSub in PXSelect<Sub, Where<Sub.subCD, Like<Required<Sub.subCD>>>>.Select(this, subCDWildCard))
				{
					AllocationProcess.BranchAccountSubKey key = new AllocationProcess.BranchAccountSubKey(aSrc.BranchID.Value, iAcct.AccountID.Value, iSub.SubID.Value);
					if (aSrcDict.ContainsKey(key))
					{
						return true;
					}
					else
					{
						AllocationSourceDetail detail = new AllocationSourceDetail(aSrc);

						detail.AccountID = iAcct.AccountID;
						detail.SubID = iSub.SubID;
						if (detail.ContraAccountID != null && detail.ContraSubID == null)
						{
							detail.ContraSubID = detail.SubID;
						}
						aSrcDict[key] = detail;
					}
				}
			}
			return false;
		}

		protected virtual bool isWeigthRecalcRequired()
		{
			return (this.Allocation.Current.AllocMethod == Constants.AllocationMethod.ByPercent);
		}

		protected virtual List<GLAllocationDestination> FindDuplicated(GLAllocationDestination aDest, GLAllocation aDefinition)
		{
			List<GLAllocationDestination> duplicated = new List<GLAllocationDestination>();
			bool includeBasis = (aDefinition.AllocMethod == Constants.AllocationMethod.ByAcctPTD || aDefinition.AllocMethod == Constants.AllocationMethod.ByAcctYTD);
			foreach (GLAllocationDestination jDest in this.Destination.Select())
			{
				if (object.ReferenceEquals(aDest, jDest)
					   || (aDest.GLAllocationID == jDest.GLAllocationID && aDest.LineID == jDest.LineID)) continue; //Skip self

				if (aDest.AccountCD == jDest.AccountCD
					 && aDest.SubCD == jDest.SubCD
					 && aDest.BranchID == jDest.BranchID)
				{
					duplicated.Add(jDest);
				}                    
			}
			return duplicated;
		}

		protected virtual bool ValidateDestinationMasks()
		{
			bool validated = true;
			bool createNewSubAccount = false;
			GLAllocation allocation = this.Allocation.Current;
			if (allocation == null) return validated;

			SubAccountMaint subMaint = SubAccountMaint.CreateInstance<SubAccountMaint>();
			foreach (GLAllocationDestination dest in this.Destination.Select())
			{
				validated = ValidateDestAccountCDMask(dest, dest.AccountCD) && validated;
				createNewSubAccount = false;
				validated = ValidateDestSubCDMask(dest, dest.SubCD, out createNewSubAccount) && validated;
				if (createNewSubAccount)
				{
					// add new sub to system
					Sub subAcc = new Sub();
					subAcc.SubCD = dest.SubCD;
					subAcc.Active = true;
					subMaint.SubRecords.Insert(subAcc);
				}
			}

			if (validated)
			{
				subMaint.Save.Press();
			}

			return validated;
		}

		protected virtual bool ValidateDestAccountCDMask(GLAllocationDestination dest, string newValue)
		{
			bool validated = true;
			GLAllocation allocation = this.Allocation.Current;
			if (allocation == null) return validated;

			if (newValue == null)
			{
				if (allocation?.AllocateSeparately != true)
				{
					// check not null destination accounts if allocate separately is off
					Destination.Cache.RaiseExceptionHandling<GLAllocationDestination.accountCD>(dest, newValue,
						new PXSetPropertyException(Messages.AllocationDestEmptyAccMask, PXErrorLevel.Error));
					validated = false;
				}
			}
			else
			{
				// check destination accounts exist in a system
				Account acc = Account.UK.Find(this, newValue);
				if (acc == null)
				{
					Destination.Cache.RaiseExceptionHandling<GLAllocationDestination.accountCD>(dest, newValue,
						new PXSetPropertyException(Messages.CannotFindAccount, PXErrorLevel.Error, newValue));
					validated = false;
				}
				else
				{
					Ledger ledger = PXSelectorAttribute.Select<GLAllocation.allocLedgerID>(Allocation.Cache, Allocation.Current) as Ledger;
					if (acc != null &&
						(acc.Active != true
						|| (acc.CuryID != null && acc.CuryID != ledger.BaseCuryID)
						|| GLSetup.Current.YtdNetIncAccountID == acc.AccountID))
					{
						Destination.Cache.RaiseExceptionHandling<GLAllocationDestination.subCD>(dest, newValue,
							new PXSetPropertyException(PX.Data.ErrorMessages.ElementDoesntExistOrNoRights, PXErrorLevel.Error, AccountAttribute.DimensionName));
					}
				}
			}

			return validated;
		}

		protected virtual bool ValidateDestSubCDMask(GLAllocationDestination dest, string newValue, out bool createNewSubAccount)
		{
			bool validated = true; createNewSubAccount = false;
			GLAllocation allocation = this.Allocation.Current;
			if (allocation == null) return validated;

			if (newValue == null)
			{
				if (allocation?.AllocateSeparately != true)
				{
					// check not null destination sub accounts if allocate separately is off
					Destination.Cache.RaiseExceptionHandling<GLAllocationDestination.subCD>(dest, newValue,
						new PXSetPropertyException(Messages.AllocationDestEmptySubMask, PXErrorLevel.Error));
					validated = false;
				}
			}
			else
			{
				Sub sub = Sub.UK.Find(this, newValue);
				if (sub == null)
				{
					// need to check segments
					string errMesage = null; bool hasBlanks = false;
					validated = CheckSegments(Destination.Cache, newValue, out errMesage, out hasBlanks, out createNewSubAccount);

					if (errMesage != null)
					{
						Destination.Cache.RaiseExceptionHandling<GLAllocationDestination.subCD>(dest, newValue,
								new PXSetPropertyException(errMesage, PXErrorLevel.Error, dest.SubCD));
					}
					else if (hasBlanks && allocation?.AllocateSeparately != true)
					{
						Destination.Cache.RaiseExceptionHandling<GLAllocationDestination.subCD>(dest, newValue,
						new PXSetPropertyException(PX.Data.ErrorMessages.ElementDoesntExist, PXErrorLevel.Error, SubAccountAttribute.DimensionName));
						validated = false;
					}
				}
			}

			return validated;
		}
		#endregion
		#region Utility functions
		public class AllocationSourceDetail
		{
			public AllocationSourceDetail() { }
			public AllocationSourceDetail(GLAllocationSource aSrc) 
			{
				this.CopyFrom(aSrc);
			}

			public int? LineID;
			public int? BranchID;
			public int? AccountID;
			public int? SubID;
			public int? ContraAccountID;
			public int? ContraSubID;
			public decimal? LimitAmount;
			public decimal? LimitPercent;
			public virtual void CopyFrom(GLAllocationSource aSrc) 
			{
				this.LineID = aSrc.LineID;
				this.BranchID = aSrc.BranchID;
				this.ContraAccountID = aSrc.ContrAccountID;
				this.ContraSubID = aSrc.ContrSubID;
				this.LimitAmount = aSrc.LimitAmount;
				this.LimitPercent = aSrc.LimitPercent;
			}
		}

		protected virtual bool CheckSegments(PXCache cache, string subCD, out string errorMessage, out bool hasBlanks, out bool createNewSubAccount)
		{
			bool result = true; errorMessage = null; hasBlanks = false; createNewSubAccount = false;
			PXDimensionAttribute.Definition definition = PX.Common.PXContext.GetSlot<PXDimensionAttribute.Definition>();
			DimensionLookupMode lookupMode = definition.LookupModes[SubAccountAttribute.DimensionName];
			int segCount = definition.Dimensions[SubAccountAttribute.DimensionName].Count();

			int keylength = 0;
			for (int i = 1; i <= segCount; i++)
			{
				List<string> list = PXDimensionAttribute.GetSegmentValues(SubAccountAttribute.DimensionName, i).ToList<string>();
				String segvalue = subCD.Substring(keylength, subCD.Length < keylength + list[0].Length ? subCD.Length - keylength : list[0].Length);
				keylength += list[0].Length;

				if (segvalue.Trim() == "")
				{
					hasBlanks = true;
				}
				else if (!list.Contains(segvalue))
				{
					result = false;
					errorMessage = string.Format(PX.Data.ErrorMessages.ElementOfFieldDoesntExist,
						definition.Dimensions[SubAccountAttribute.DimensionName][i-1].Descr,
						SubAccountAttribute.DimensionName);
				}
			}

			if (errorMessage == null && !hasBlanks)
			{
				if (lookupMode == DimensionLookupMode.BySegmentsAndAllAvailableSegmentValues)
				{
					createNewSubAccount = true;
				}
				else
				{
					createNewSubAccount = false;
					errorMessage = string.Format(PX.Data.ErrorMessages.ElementDoesntExist, SubAccountAttribute.DimensionName);
				}
			}

			return result;
		}
		#endregion

		#region private members
		private int? justInserted;
		private bool isMassDelete = false;
		#endregion

		public override void Persist()
		{
			if (ValidateBranchesForLedgers() && ValidateDestinationMasks()) base.Persist();
		}

		#region IPXPrepareItems Members

		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
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

		public void PrepareItems(string viewName, IEnumerable items)
		{
			
		}

		#endregion
				
	}
}
